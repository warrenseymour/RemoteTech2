﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace RemoteTech
{
    public interface ISatellite
    {
        bool Visible { get; }
        String Name { get; set; }
        Guid Guid { get; }
        Vector3d Position { get; }
        CelestialBody Body { get; }

        bool Powered { get; }
        bool IsCommandStation { get; }
        bool HasLocalControl { get; }

        IEnumerable<IAntenna> Antennas { get; }
        Group Group { get; set; }
    }

    public static class SatelliteExtensions
    {
        public static ConnectionMap Connections(this ISatellite satellite)
        {
            return RTCore.Instance.Network[satellite];
        }
    }


}
