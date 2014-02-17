using System;
using System.Collections.Generic;
using System.Linq;

namespace RemoteTech
{
    internal class ProtoAntenna : IVesselAntenna
    {
        public event Action<IVesselAntenna> Destroyed = delegate { };
        public String Name { get; private set; }
        public Vector3d Position { get { return Vessel.GetWorldPos3D(); } }
        public Guid Guid { get; private set; }
        public bool Powered { get; private set; }
        public bool Activated { get; set; }
        public float Consumption { get; private set; }
        public bool CanTarget { get { return Dish > 0; } }
        public IList<Target> Targets { get { return targets; } }
        public float Dish { get; private set; }
        public double Radians { get; private set; }
        public float Omni { get; private set; }
        public Vessel Vessel { get; private set; }

        private readonly ProtoPartModuleSnapshot protoModule;
        private readonly EventListWrapper<Target> targets;

        public ProtoAntenna(Vessel vessel, ProtoPartSnapshot p, ProtoPartModuleSnapshot ppms)
        {
            ConfigNode n = new ConfigNode();
            ppms.Save(n);

            Name = p.partInfo.title;
            Consumption = 0.0f;
            Guid = vessel.id;
            Vessel = vessel;
            protoModule = ppms;

            targets = new EventListWrapper<Target>(VesselAntenna.ParseAntennaTargets(n));
            Dish = VesselAntenna.ParseAntennaDishRange(n);
            Radians = VesselAntenna.ParseAntennaRadians(n);
            Omni = VesselAntenna.ParseAntennaOmniRange(n);
            Powered = VesselAntenna.ParseAntennaIsPowered(n);
            Activated = VesselAntenna.ParseAntennaIsActivated(n);

            targets.AddingNew += OnTargetsModified;
            targets.ListChanged += OnTargetsModified;

            RTLog.Notify(ToString());
        }

        public void OnConnectionRefresh() { }

        private void OnTargetsModified()
        {
            ConfigNode n = new ConfigNode();
            protoModule.Save(n);
            VesselAntenna.SaveAntennaTargets(n, targets);
            protoModule.moduleValues = n;
        }
        public int CompareTo(IAntenna antenna)
        {
            return Consumption.CompareTo(antenna.Consumption);
        }

        public override string ToString()
        {
            return String.Format("ProtoAntenna(Name: {0}, Guid: {1}, Dish: {2}, Omni: {3}, Target: {4}, Radians: {5})", Name, Guid, Dish, Omni, Targets, Radians);
        }
    }
}