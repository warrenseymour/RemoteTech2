using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RemoteTech
{
    public class ManeuverCommand : AbstractCommand
    {
        public ManeuverNode Node { get; set; }
        public double OriginalDelta { get; set; }
        public double RemainingTime { get; set; }
        public double RemainingDelta { get; set; }
        public override int Priority { get { return 0; } }

        public override string Description
        {
            get
            {
                if (RemainingTime > 0 || RemainingDelta > 0)
                    return "Executing maneuver: " + RemainingDelta.ToString("F2") + "m/s" + Environment.NewLine +
                           "Remaining duration: " + RTUtil.FormatDuration(RemainingTime) + Environment.NewLine + base.Description;
                else
                    return "Execute planned maneuver" + Environment.NewLine + base.Description;
            }
        }

        public override bool Pop(FlightComputer f)
        {
            var burn = f.ActiveCommands.FirstOrDefault(c => c is BurnCommand);
            if (burn != null) f.Remove(burn);
            OriginalDelta = Node.DeltaV.magnitude;
            RemainingDelta = Node.GetBurnVector(f.Vessel.orbit).magnitude;
            RemainingTime = BurnTime(Node, f);
            return true;
        }

        public override bool Execute(FlightComputer f, FlightCtrlState fcs)
        {
            fcs.mainThrottle = RemainingDelta > 1 ? 1.0f : 0.5f;

            if (RemainingDelta > 0.1)
            {
                var forward = Node.GetBurnVector(f.Vessel.orbit).normalized;
                var up = (f.SignalProcessor.Body.position - f.SignalProcessor.Position).normalized;
                var orientation = Quaternion.LookRotation(forward, up);
                FlightCore.HoldOrientation(fcs, f, orientation);

                RemainingTime -= TimeWarp.deltaTime;
                RemainingDelta = Node.GetBurnVector(f.Vessel.orbit).magnitude;

                return false;
            }
            f.Enqueue(AttitudeCommand.Off(), true, true, true);
            return true;
        }

        private static double BurnTime(ManeuverNode node, FlightComputer f)
        {
            double deltaV = node.DeltaV.magnitude;
            double accel = (FlightCore.GetTotalThrust(f.Vessel) / f.Vessel.GetTotalMass());

            return deltaV / accel;
        }

        public static ManeuverCommand WithNode(ManeuverNode node, FlightComputer f)
        {
            var newNode = new ManeuverCommand()
            {
                Node = new ManeuverNode()
                {
                    DeltaV = node.DeltaV,
                    patch = node.patch,
                    solver = node.solver,
                    scaledSpaceTarget = node.scaledSpaceTarget,
                    nextPatch = node.nextPatch,
                    UT = node.UT,
                    nodeRotation = node.nodeRotation,
                },
                TimeStamp = node.UT - (BurnTime(node, f) / 2)
            };
            return newNode;
        }
    }
}
