using System;
using UnityEngine;

namespace RemoteTech
{
    public class ProtoSignalProcessor : ISignalProcessor
    {
        public event Action<ISignalProcessor> Destroyed = delegate { };
        public String Name { get { return String.Format("ProtoSignalProcessor({0})", Vessel.vesselName); } }
        public bool Visible { get { return MapViewFiltering.CheckAgainstFilter(vessel); } }
        public bool Powered { get; private set; }
        public bool IsCommandStation { get; private set; }
        public Guid Guid { get { return vessel.id; } }
        public Group Group { get; set; }
        public Vessel Vessel { get { return vessel; } }
        public FlightComputer FlightComputer { get { return null; } }

        private readonly Vessel vessel;

        public ProtoSignalProcessor(ProtoPartModuleSnapshot ppms, Vessel v)
        {
            vessel = v;
            Powered = ppms.GetBool("IsRTPowered");
            IsCommandStation = Powered && v.HasCommandStation() && v.GetVesselCrew().Count >= 6;
            RTLog.Notify("ProtoSignalProcessor(Powered: {0}, HasCommandStation: {1}, Crew: {2})", Powered, v.HasCommandStation(), v.GetVesselCrew().Count);
        }

        public override String ToString()
        {
            return Name;
        }
    }
}