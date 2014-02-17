using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteTech
{
    internal class CelestialBodyWrapper : ISatellite
    {
        private static Dictionary<CelestialBody, Guid> guidCache = new Dictionary<CelestialBody, Guid>();
        bool ISatellite.Visible { get { return true; } }
        string ISatellite.Name { get { return celestialBody.bodyName; } set { } }
        Guid ISatellite.Guid { get { return guid; } }
        Vector3d ISatellite.Position { get { return celestialBody.position; } }
        CelestialBody ISatellite.Body { get { return celestialBody; } }
        bool ISatellite.Powered { get { return true; } }
        Group ISatellite.Group { get { return Group.Empty; } set { } }
        bool ISatellite.IsCommandStation { get { return false; } }
        bool ISatellite.HasLocalControl { get { return false; } }
        IEnumerable<IAntenna> ISatellite.Antennas { get { return Enumerable.Empty<IAntenna>(); } }

        private readonly CelestialBody celestialBody;
        private readonly Guid guid;
        private readonly int hash;
        public CelestialBodyWrapper(CelestialBody celestialBody)
        {
            this.celestialBody = celestialBody;
            if (!guidCache.TryGetValue(celestialBody, out guid))
            {
                this.guid = celestialBody.GenerateGuid();
            }
            this.hash = guid.GetHashCode();
        }

        public override int GetHashCode()
        {
            return hash;
        }
    }

    public static class CelestialBodyExtensions
    {
        public static ISatellite AsSatellite(this CelestialBody cb)
        {
            return new CelestialBodyWrapper(cb);
        }
    }
}
