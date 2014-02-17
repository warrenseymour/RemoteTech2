using System;
using System.Collections.Generic;

namespace RemoteTech
{
    public interface IAntenna
    {
        String Name { get; }
        Guid Guid { get; }
        Vector3d Position { get; }
        bool Activated { get; set; }
        bool Powered { get; }
        bool CanTarget { get; }
        IList<Target> Targets { get; }
        float Dish { get; }
        double Radians { get; }
        float Omni { get; }
        float Consumption { get; }

        void OnConnectionRefresh();
    }
}
