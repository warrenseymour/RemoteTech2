using System;
using System.Linq;
using UnityEngine;

namespace RemoteTech
{
    public interface ISignalProcessor
    {
        String Name { get; }
        Guid Guid { get; }
        bool Visible { get; }
        bool Powered { get; }
        Group Group { get; }
        bool IsCommandStation { get; }
        Vessel Vessel { get; }
        FlightComputer FlightComputer { get; }

        event Action<ISignalProcessor> Destroyed;
    }

    public static class SignalProcessor
    {
        private const String SignalProcessorIdentifier = "IsRTSignalProcessor";
        private const String CommandStationIdentifier = "IsRTCommandStation";
        private const String IsPoweredIdentifier = "IsRTPowered";
        private const String IsActiveIdentifier = "IsRTActive";
        private const String GroupIdentifier = "RTGroup";
        public static bool IsSignalProcessor(this ProtoPartModuleSnapshot ppms)
        {
            return ppms.GetBool(SignalProcessorIdentifier);

        }

        public static bool IsSignalProcessor(this PartModule pm)
        {
            return pm.Fields.GetValue<bool>(SignalProcessorIdentifier);
        }

        public static ISignalProcessor GetSignalProcessor(this Vessel v)
        {
            RTLog.Notify("GetSignalProcessor({0}): Check", v.vesselName);
            if (v.loaded)
            {
                foreach (PartModule pm in v.Parts.SelectMany(p => p.Modules.Cast<PartModule>()).Where(pm => pm.IsSignalProcessor()))
                {
                    RTLog.Notify("GetSignalProcessor({0}): Found", v.vesselName);
                    return pm as ISignalProcessor;
                }

            }
            else
            {
                foreach (ProtoPartModuleSnapshot ppms in v.protoVessel.protoPartSnapshots.SelectMany(x => x.modules).Where(ppms => ppms.IsSignalProcessor()))
                {
                    RTLog.Notify("GetSignalProcessor({0}): Found", v.vesselName);
                    return new ProtoSignalProcessor(ppms, v);
                }
            }
            return null;
        }

        public static bool IsCommandStation(this ProtoPartModuleSnapshot ppms)
        {
            return ppms.GetBool(CommandStationIdentifier);
        }

        public static bool IsCommandStation(this PartModule pm)
        {
            return pm.Fields.GetValue<bool>(CommandStationIdentifier);
        }

        public static bool HasCommandStation(this Vessel v)
        {
            RTLog.Notify("HasCommandStation({0})", v.vesselName);
            if (v.loaded)
            {
                return v.Parts.SelectMany(p => p.Modules.Cast<PartModule>()).Any(pm => pm.IsCommandStation());
            }
            else
            {
                return v.protoVessel.protoPartSnapshots.SelectMany(x => x.modules).Any(pm => pm.IsCommandStation());
            }
        }
    }
}