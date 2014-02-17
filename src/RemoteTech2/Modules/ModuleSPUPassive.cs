using System;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RemoteTech
{
    public class ModuleSPUPassive : PartModule, ISignalProcessor
    {
        public event Action<ISignalProcessor> Destroyed = delegate { };

        public String Name { get { return String.Format("ModuleSPUPassive({0})", Vessel.vesselName); } }
        public Guid Guid { get { return Vessel.id; } }
        public bool Visible { get { return MapViewFiltering.CheckAgainstFilter(vessel); } }
        public bool Powered { get { return Vessel.IsControllable; } }
        public Group Group { get; set; }
        public bool IsCommandStation { get { return false; } }
        public FlightComputer FlightComputer { get { return null; } }
        public Vessel Vessel { get { return vessel; } }

        private ISatellite Satellite { get { return RTCore.Instance.Satellites[Guid]; } }

        [KSPField(isPersistant = true)]
        public bool
            IsRTPowered = false,
            IsRTSignalProcessor = true,
            IsRTCommandStation = false;

        public override void OnStart(StartState state)
        {
            if (state != StartState.Editor)
            {
                RTCore.Instance.Satellites.Register(this);
            }
        }

        private void FixedUpdate()
        {
            if (Vessel != null)
            {
                IsRTPowered = Powered;
            }
        }

        public void OnDestroy()
        {
            RTLog.Notify("ModuleSPUPassive: OnDestroy");
            Destroyed.Invoke(this);
        }
        public override string ToString()
        {
            return String.Format("ModuleSPUPassive({0}, {1})", Vessel != null ? Vessel.vesselName : "null", Guid);
        }
    }
}