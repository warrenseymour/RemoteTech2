﻿using System;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RemoteTech
{
    [KSPModule("Signal Processor")]
    public class ModuleSPU : PartModule, ISignalProcessor
    {
        public event Action<ISignalProcessor> Destroyed = delegate { };
        public String Name { get { return String.Format("ModuleSPU({0})", VesselName); } }
        public String VesselName { get { return vessel.vesselName; } set { vessel.vesselName = value; } }
        public Guid Guid { get { return Vessel.id; } }
        public bool Visible { get { return MapViewFiltering.CheckAgainstFilter(vessel); } }
        public bool Powered { get { return IsRTPowered; } }
        public Group Group { get { return group; } }
        public bool IsCommandStation { get { return IsRTPowered && IsRTCommandStation && vessel.GetVesselCrew().Count >= 6; } }
        public Vessel Vessel { get { return vessel; } }
        public FlightComputer FlightComputer { get; private set; }
        private VesselSatellite Satellite { get { return RTCore.Instance.Satellites[Guid]; } }

        [KSPField(isPersistant = true)]
        public Group group = new Group("");

        [KSPField(isPersistant = true)]
        public bool 
            IsRTPowered = false,
            IsRTSignalProcessor = true,
            IsRTCommandStation = false;

        [KSPField]
        public bool
            ShowGUI_Status = true,
            ShowEditor_Type = true;

        [KSPField(guiName = "SPU", guiActive = true)]
        public String GUI_Status;

        private enum State
        {
            Operational,
            ParentDefect,
            NoConnection
        }
        public override String GetInfo()
        {
            if (!ShowEditor_Type) return String.Empty;
            return IsRTCommandStation ? "Remote Command capable (6+ crew)" : "Remote Control capable";
        }

        public override void OnStart(StartState state)
        {
            if (state != StartState.Editor)
            {
                RTCore.Instance.Satellites.Register(this);
                FlightComputer = new FlightComputer(this);
            }
            Fields["GUI_Status"].guiActive = ShowGUI_Status;
        }

        public void OnDestroy()
        {
            RTLog.Notify("ModuleSPU: OnDestroy");
            if (FlightComputer != null) FlightComputer.Dispose();
            Destroyed.Invoke(this);
        }

        private State UpdateControlState()
        {
            IsRTPowered = part.isControlSource;
            if (!RTCore.Instance)
                return State.Operational;
            if (!IsRTPowered)
                return State.ParentDefect;
            if (Satellite == null || !RTCore.Instance.Network[Satellite].Any())
                return State.NoConnection;
            return State.Operational;
        }

        public void Update()
        {
            if (FlightComputer != null)
            {
                FlightComputer.OnUpdate();
            }
        }

        public void FixedUpdate()
        {
            if (FlightComputer != null)
            {
                FlightComputer.OnFixedUpdate();
            }
            HookPartMenus();
            switch (UpdateControlState())
            {
                case State.Operational:
                    GUI_Status = "Operational.";
                    break;
                case State.ParentDefect:
                case State.NoConnection:
                    GUI_Status = "No connection.";
                    break;
            }
        }

        public void HookPartMenus()
        {
            UIPartActionMenuPatcher.Wrap(vessel, (e, ignore_delay) =>
            {
                var v = FlightGlobals.ActiveVessel;
                if (v == null || v.isEVA || RTCore.Instance == null)
                {
                    e.Invoke();
                    return;
                }
                var vs = RTCore.Instance.Satellites[v];
                if (vs == null || vs.HasLocalControl)
                {
                    e.Invoke();
                }
                else if (vs.FlightComputer != null && vs.FlightComputer.InputAllowed)
                {
                    if (ignore_delay)
                    {
                        e.Invoke();
                    }
                    else
                    {
                        vs.SignalProcessor.FlightComputer.Enqueue(EventCommand.Event(e));
                    }
                }
                else
                {
                    ScreenMessages.PostScreenMessage(new ScreenMessage("No connection to send command on.", 4.0f, ScreenMessageStyle.UPPER_LEFT));
                }
            });
        }

        public override string ToString()
        {
            return String.Format("ModuleSPU({0}, {1})", Vessel != null ? Vessel.vesselName : "null", Guid);
        }
    }
}