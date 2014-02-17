﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace RemoteTech
{
    public sealed class MissionControlAntenna : IAntenna
    {
        [Persistent] public float Omni = 75000000;
        public ISatellite Parent { get; set; }
        public Vector3d Position { get { return Parent.Position; } }
        float IAntenna.Omni { get { return Omni; } }
        Guid IAntenna.Guid { get { return Parent.Guid; } }
        String IAntenna.Name { get { return "Dummy Antenna"; } }
        bool IAntenna.Powered { get { return true; } }
        bool IAntenna.Activated { get { return true; } set { return; } }
        float IAntenna.Consumption { get { return 0.0f; } }
        bool IAntenna.CanTarget { get { return false; } }
        IList<Target> IAntenna.Targets { get { return new Target[0]; } }
        float IAntenna.Dish { get { return 0.0f; } }
        double IAntenna.Radians { get { return 1.0; } }

        public void OnConnectionRefresh() { }

        public int CompareTo(IAntenna antenna)
        {
            return ((IAntenna)this).Consumption.CompareTo(antenna.Consumption);
        }
    }
}