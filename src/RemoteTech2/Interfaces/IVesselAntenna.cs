using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace RemoteTech
{
    public interface IVesselAntenna : IAntenna
    {
        Vessel Vessel { get; }

        event Action<IVesselAntenna> Destroyed;
    }

    public static class VesselAntenna
    {
        public const String AntennaIdentifier = "IsRTAntenna";
        public const String SingleTargetIdentifier = "RTAntennaTarget";
        public const String TargetsIdentifier = "RTAntennaTargets";
        public const String RadiansIdentifier = "RTDishRadians";
        public const String OmniRangeIdentifier = "RTOmniRange";
        public const String DishRangeIdentifier = "RTDishRange";
        public const String IsPoweredIdentifier = "IsRTPowered";
        public const String IsActiveIdentifier = "IsRTActive";

        public static EventListWrapper<Target> ParseAntennaSingleTarget(ConfigNode n)
        {
            var guid = RTUtil.TryParseGuidNullable(n.GetValue(SingleTargetIdentifier) ?? String.Empty) ?? Guid.Empty;
            return new EventListWrapper<Target>(new List<Target>() { Target.SingleUnsafeCompatibility(guid) });
        }

        public static EventListWrapper<Target> ParseAntennaTargets(ConfigNode n)
        {
            if (!n.HasNode(TargetsIdentifier)) 
                return new EventListWrapper<Target>(new List<Target>() { Target.Empty });
            return ConfigNode.CreateObjectFromConfig<EventListWrapper<Target>>(n.GetNode(TargetsIdentifier));
        }

        public static void SaveAntennaTargets(ConfigNode n, EventListWrapper<Target> targets)
        {
            var targetsNode = ConfigNode.CreateConfigFromObject(targets, 0, null);
            targetsNode.name = TargetsIdentifier;
            if (n.HasNode(TargetsIdentifier))
            {
                n.SetNode(TargetsIdentifier, targetsNode);
            }
            else
            {
                n.AddNode(targetsNode);
            }
        }

        public static double ParseAntennaRadians(ConfigNode n)
        {
            return RTUtil.TryParseDoubleNullable(n.GetValue(RadiansIdentifier)) ?? 0.0f;
        }

        public static float ParseAntennaOmniRange(ConfigNode n)
        {
            return RTUtil.TryParseSingleNullable(n.GetValue(OmniRangeIdentifier)) ?? 0.0f;
        }

        public static float ParseAntennaDishRange(ConfigNode n)
        {
            return RTUtil.TryParseSingleNullable(n.GetValue(DishRangeIdentifier)) ?? 0.0f;
        }
        public static bool ParseAntennaIsPowered(ConfigNode n)
        {
            return RTUtil.TryParseBooleanNullable(n.GetValue(IsPoweredIdentifier)) ?? false;
        }

        public static bool ParseAntennaIsActivated(ConfigNode n)
        {
            return RTUtil.TryParseBooleanNullable(n.GetValue(IsActiveIdentifier)) ?? false;
        }
        public static bool IsAntenna(this ProtoPartModuleSnapshot ppms)
        {
            return ppms.GetBool(AntennaIdentifier) &&
                   ppms.GetBool(IsPoweredIdentifier) &&
                   ppms.GetBool(IsActiveIdentifier);
        }

        public static bool IsAntenna(this PartModule pm)
        {
            return pm.Fields.GetValue<bool>(AntennaIdentifier) &&
                   pm.Fields.GetValue<bool>(IsPoweredIdentifier) &&
                   pm.Fields.GetValue<bool>(IsActiveIdentifier);
        }
    }
}
