using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RemoteTech
{
    [KSPModule("Antenna")]
    public class ModuleRTAntenna : PartModule, IVesselAntenna
    {
        public event Action<IVesselAntenna> Destroyed = delegate { };
        public String Name { get { return part.partInfo.title; } }
        public Guid Guid { get { return Vessel.id; } }
        public bool Powered { get { return IsRTPowered; } }
        public bool Activated { get { return IsRTActive; } set { SetState(value); } }
        public bool Animating { get { return deployFxModules.Any(fx => fx.GetScalar > 0.1f && fx.GetScalar < 0.9f); } }
        public IList<Target> Targets { get { return targets; } }
        public bool CanTarget { get { return Mode1DishRange > 0; } }
        public float Dish { get { return IsRTBroken ? 0.0f : ((IsRTActive && IsRTPowered) ? Mode1DishRange : Mode0DishRange) * RangeMultiplier; } }
        public double Radians { get { return RTDishRadians; } }
        public float Omni { get { return IsRTBroken ? 0.0f : ((IsRTActive && IsRTPowered) ? Mode1OmniRange : Mode0OmniRange) * RangeMultiplier; } }
        public float Consumption { get { return IsRTBroken ? 0.0f : IsRTActive ? EnergyCost * ConsumptionMultiplier : 0.0f; } }
        public Vector3d Position { get { return Vessel.GetWorldPos3D(); } }
        public Vessel Vessel { get { return vessel; } }
        private float RangeMultiplier { get { return RTSettings.Instance.RangeMultiplier; } }
        private float ConsumptionMultiplier { get { return RTSettings.Instance.ConsumptionMultiplier; } }

        [KSPField]
        public bool
            ShowGUI_DishRange = true,
            ShowGUI_OmniRange = true,
            ShowGUI_EnergyReq = true,
            ShowGUI_Status = true,
            ShowEditor_OmniRange = true,
            ShowEditor_DishRange = true,
            ShowEditor_EnergyReq = true,
            ShowEditor_DishAngle = true;

        [KSPField(guiName = "Dish range")]
        public String GUI_DishRange;
        [KSPField(guiName = "Energy")]
        public String GUI_EnergyReq;
        [KSPField(guiName = "Omni range")]
        public String GUI_OmniRange;
        [KSPField(guiName = "Status")]
        public String GUI_Status;

        [KSPField]
        public String
            Mode0Name = "Off",
            Mode1Name = "Operational",
            ActionMode0Name = "Deactivate",
            ActionMode1Name = "Activate",
            ActionToggleName = "Toggle";

        [KSPField]
        public float
            Mode0DishRange = -1.0f,
            Mode1DishRange = -1.0f,
            Mode0OmniRange = 0.0f,
            Mode1OmniRange = 0.0f,
            EnergyCost = 0.0f,
            DishAngle = 0.0f,
            MaxQ = -1;

        [KSPField(isPersistant = true)]
        public bool
            IsRTAntenna = true,
            IsRTActive = false,
            IsRTPowered = false,
            IsRTBroken = false;

        [KSPField(isPersistant = true)]
        public double RTDishRadians = 1.0f;

        [KSPField(isPersistant = true)]
        public float
            RTOmniRange = 0.0f,
            RTDishRange = 0.0f;

        [KSPField(isPersistant = true)]
        public EventListWrapper<Target> targets = new EventListWrapper<Target>(new List<Target>() { Target.Empty }); 

        public int[] deployFxModuleIndices, progressFxModuleIndices;
        private List<IScalarModule> deployFxModules = new List<IScalarModule>();
        private List<IScalarModule> progressFxModules = new List<IScalarModule>();
        private ConfigNode transmitterConfig;
        private IScienceDataTransmitter transmitter;

        private enum State
        {
            Off,
            Operational,
            NoResources,
            Malfunction,
        }
        public override string GetInfo()
        {
            var info = new StringBuilder();

            if (ShowEditor_OmniRange && Mode1OmniRange > 0)
                info.AppendFormat("Omni range: {0} / {1}", 
                    RTUtil.FormatSI(Mode0OmniRange * RangeMultiplier, "m"), 
                    RTUtil.FormatSI(Mode1OmniRange * RangeMultiplier, "m")).AppendLine();
            
            if (ShowEditor_DishRange && Mode1DishRange > 0)
                info.AppendFormat("Dish range: {0} / {1}", 
                    RTUtil.FormatSI(Mode0DishRange * RangeMultiplier, "m"), 
                    RTUtil.FormatSI(Mode1DishRange * RangeMultiplier, "m")).AppendLine();
            
            if (ShowEditor_EnergyReq && EnergyCost > 0)
                info.AppendFormat("Energy req.: {0}", 
                    RTUtil.FormatConsumption(EnergyCost * ConsumptionMultiplier)).AppendLine();

            if (ShowEditor_DishAngle && Mode1DishRange > 0)
                info.AppendFormat("Cone angle: {0} degrees", 
                    DishAngle.ToString("F2")).AppendLine();

            if (IsRTActive)
                info.AppendLine("Activated by default");

            if (MaxQ > 0)
                info.AppendLine("Snaps under high dynamic pressure");

            return info.ToString().TrimEnd(Environment.NewLine.ToCharArray());
        }

        public virtual void SetState(bool state)
        {
            bool prev_state = IsRTActive;
            IsRTActive = state && !IsRTBroken;
            Events["EventOpen"].guiActive = Events["EventOpen"].active = 
            Events["EventEditorOpen"].guiActiveEditor = 
            Events["OverrideOpen"].guiActiveUnfocused = !IsRTActive && !IsRTBroken;

            Events["EventClose"].guiActive = Events["EventClose"].active = 
            Events["EventEditorClose"].guiActiveEditor =
            Events["OverrideClose"].guiActiveUnfocused = IsRTActive && !IsRTBroken;

            UpdateContext();
            StartCoroutine(SetFXModules_Coroutine(deployFxModules, IsRTActive ? 1.0f : 0.0f));

            if (RTCore.Instance != null)
            {
                var satellite = RTCore.Instance.Network[Guid];
                bool routeToKSC = RTCore.Instance.Network[satellite].ConnectedToKSC();
                if (transmitter == null && routeToKSC)
                {
                    AddTransmitter();
                }
                else if (!routeToKSC && transmitter != null)
                {
                    RemoveTransmitter();
                }
            }
        }

        [KSPEvent(name = "EventToggle", guiActive = false)]
        public void EventToggle() { if (Animating) return; if (IsRTActive) { EventClose(); } else { EventOpen(); } }

        [KSPEvent(name = "EventTarget", guiActive = false, guiName = "Set Target", category = "skip_delay")]
        public void EventTarget() { (new AntennaWindow(this)).Show(); }

        [KSPEvent(name = "EventEditorOpen", guiActive = false, guiName = "Start deployed")]
        public void EventEditorOpen() { SetState(true); }

        [KSPEvent(name = "EventEditorClose", guiActive = false, guiName = "Start retracted")]
        public void EventEditorClose() { SetState(false); }

        [KSPEvent(name = "EventOpen", guiActive = false)]
        public void EventOpen() { if (!Animating) { SetState(true); } }

        [KSPEvent(name = "EventClose", guiActive = false)]
        public void EventClose() { if (!Animating) { SetState(false); } }

        [KSPAction("ActionToggle", KSPActionGroup.None)]
        public void ActionToggle(KSPActionParam param) { EventToggle(); }

        [KSPAction("ActionOpen", KSPActionGroup.None)]
        public void ActionOpen(KSPActionParam param) { EventOpen(); }

        [KSPAction("ActionClose", KSPActionGroup.None)]
        public void ActionClose(KSPActionParam param) { EventClose(); }

        [KSPEvent(name = "OverrideTarget",   active = true, 
                  guiActiveUnfocused = true, unfocusedRange = 5,
                  externalToEVAOnly = true,  guiName = "[EVA] Set Target",
                  category = "skip_delay;skip_control")]
        public void OverrideTarget() { (new AntennaWindow(this)).Show(); }

        [KSPEvent(name = "OverrideOpen",     active = true, 
                  guiActiveUnfocused = true, unfocusedRange = 5, 
                  externalToEVAOnly = true,  guiName = "[EVA] Force Open", 
                  category = "skip_delay;skip_control")]
        public void OverrideOpen() { EventOpen(); }

        [KSPEvent(name = "OverrideClose",    active = true, 
                  guiActiveUnfocused = true, unfocusedRange = 5, 
                  externalToEVAOnly = true,  guiName = "[EVA] Force Close", 
                  category = "skip_delay;skip_control")]
        public void OverrideClose() { EventClose(); }

        public void OnConnectionRefresh()
        {
            SetState(IsRTActive);
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            // Backwards compatibility with the old target system.
            targets = node.HasValue(VesselAntenna.SingleTargetIdentifier) ? VesselAntenna.ParseAntennaSingleTarget(node)
                                                                          : VesselAntenna.ParseAntennaTargets(node);
            if (node.HasValue("DishAngle"))
            {
                RTDishRadians = Math.Cos(DishAngle / 2 * Math.PI / 180);
            }
            if (node.HasValue("DeployFxModules"))
            {
                deployFxModuleIndices = KSPUtil.ParseArray<Int32>(node.GetValue("DeployFxModules"), new ParserMethod<Int32>(Int32.Parse));
            }
            if (node.HasValue("ProgressFxModules"))
            {
                progressFxModuleIndices = KSPUtil.ParseArray<Int32>(node.GetValue("ProgressFxModules"), new ParserMethod<Int32>(Int32.Parse));
            }
            if (node.HasNode("TRANSMITTER"))
            {
                RTLog.Notify("ModuleRTAntenna: Found TRANSMITTER block.");
                transmitterConfig = node.GetNode("TRANSMITTER");
                transmitterConfig.AddValue("name", "ModuleRTDataTransmitter");
            }
        }

        public override void OnStart(StartState state)
        {
            Actions["ActionOpen"].guiName = ActionMode1Name;
            Actions["ActionOpen"].active = !IsRTBroken;
            Actions["ActionClose"].guiName = ActionMode0Name;
            Actions["ActionClose"].active = !IsRTBroken;
            Actions["ActionToggle"].guiName = ActionToggleName;
            Actions["ActionToggle"].active = !IsRTBroken;

            Events["EventOpen"].guiName = ActionMode1Name;
            Events["EventClose"].guiName = ActionMode0Name;
            Events["EventToggle"].guiName = ActionToggleName;
            Events["EventTarget"].guiActive = (Mode1DishRange > 0);
            Events["EventTarget"].active = Events["EventTarget"].guiActive;

            Fields["GUI_OmniRange"].guiActive = (Mode1OmniRange > 0) && ShowGUI_OmniRange;
            Fields["GUI_DishRange"].guiActive = (Mode1DishRange > 0) && ShowGUI_DishRange;
            Fields["GUI_EnergyReq"].guiActive = (EnergyCost > 0) && ShowGUI_EnergyReq;
            Fields["GUI_Status"].guiActive = ShowGUI_Status;

            if (RTCore.Instance != null)
            {
                RTCore.Instance.Antennas.Register(this);
            }

            LoadAnimations();
            SetState(IsRTActive);
        }

        private void LoadAnimations()
        {
            deployFxModules = FindFxModules(this.deployFxModuleIndices, true);
            progressFxModules = FindFxModules(this.progressFxModuleIndices, false);
            deployFxModules.ForEach(fx => { fx.SetUIRead(false); fx.SetUIWrite(false); });
            progressFxModules.ForEach(fx => { fx.SetUIRead(false); fx.SetUIWrite(false); });
        }

        private void AddTransmitter()
        {
            if (transmitterConfig == null || !transmitterConfig.HasValue("name")) return;
            var transmitters = part.FindModulesImplementing<IScienceDataTransmitter>();
            if (transmitters.Count > 0)
            {
                RTLog.Notify("ModuleRTAntenna: Find TRANSMITTER success.");
                transmitter = transmitters.First();
            }
            else
            {
                var copy = new ConfigNode();
                transmitterConfig.CopyTo(copy);
                part.AddModule(copy);
                AddTransmitter();
                RTLog.Notify("ModuleRTAntenna: Add TRANSMITTER success.");
            }
        }

        private void RemoveTransmitter()
        {
            RTLog.Notify("ModuleRTAntenna: Remove TRANSMITTER success.");
            if (transmitter == null) return;
            part.RemoveModule((PartModule) transmitter);
            transmitter = null;
        }

        private State UpdateControlState()
        {
            if (RTCore.Instance == null) return State.Operational;

            if (IsRTBroken) return State.Malfunction;

            if (!IsRTActive) return State.Off;

            ModuleResource request = new ModuleResource();
            float resourceRequest = Consumption * TimeWarp.fixedDeltaTime;
            float resourceAmount = part.RequestResource("ElectricCharge", resourceRequest);
            if (resourceAmount < resourceRequest * 0.9) return State.NoResources;
            
            return State.Operational;
        }

        private void FixedUpdate()
        {
            switch (UpdateControlState())
            {
                case State.Off:
                    GUI_Status = Mode0Name;
                    IsRTPowered = false;
                    break;
                case State.Operational:
                    GUI_Status = Mode1Name;
                    IsRTPowered = true;
                    break;
                case State.NoResources:
                    GUI_Status = "Out of power";
                    IsRTPowered = false;
                    break;
                case State.Malfunction:
                    GUI_Status = "Malfunction";
                    IsRTPowered = false;
                    break;
            }
            RTDishRange = Dish;
            RTOmniRange = Omni;
            HandleDynamicPressure();
            UpdateContext();
        }

        private void UpdateContext()
        {
            GUI_OmniRange = RTUtil.FormatSI(Omni, "m");
            GUI_DishRange = RTUtil.FormatSI(Dish, "m");
            GUI_EnergyReq = RTUtil.FormatConsumption(Consumption);
        }

        private void HandleDynamicPressure()
        {
            if (vessel == null) return;
            if (!vessel.HoldPhysics && vessel.atmDensity > 0 && MaxQ > 0 && deployFxModules.Any(a => a.GetScalar > 0.9f))
            {
                if (vessel.srf_velocity.sqrMagnitude * vessel.atmDensity / 2 > MaxQ)
                {
                    MaxQ = -1.0f;
                    part.decouple(0.0f);
                }
            }
        }

        private List<IScalarModule> FindFxModules(int[] indices, bool showUI)
        {
            var modules = new List<IScalarModule>();
            if (indices == null) return modules;
            foreach (int i in indices)
            {
                var item = base.part.Modules[i] as IScalarModule;
                if (item != null)
                {
                    item.SetUIWrite(showUI);
                    item.SetUIRead(showUI);
                    modules.Add(item);
                }
                else
                {
                    RTLog.Notify("ModuleRTAntenna: Part Module {0} doesn't implement IScalarModule", part.Modules[i].name);
                }
            }
            return modules;
        }

        private IEnumerator SetFXModules_Coroutine(List<IScalarModule> modules, float tgtValue)
        {
            bool done = false;
            while (!done)
            {
                done = true;
                foreach (var module in modules)
                {
                    if (Mathf.Abs(module.GetScalar - tgtValue) > 0.01f)
                    {
                        module.SetScalar(tgtValue);
                        done = false;
                    }
                }
                yield return true;
            }
        }

        private void OnDestroy()
        {
            RTLog.Notify("ModuleRTAntenna: OnDestroy");
            Destroyed.Invoke(this);
        }
        public override string ToString()
        {
            return String.Format("ModuleRTAntenna(Name: {0}, Guid: {1}, Dish: {2}, Omni: {3}, Target: {4}, Radians: {5})", Name, Guid, Dish, Omni, Targets.ToArray(), Radians);
        }
    }
}