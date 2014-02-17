using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RemoteTech
{
    public class AntennaFragment : IFragment, IDisposable
    {
        private enum Mode
        {
            PlanetSatellite,
            Planet,
            Satellite,
            Group
        }
        private class Entry
        {
            public String Text { get; set; }
            public Target Target { get; set; }
            public Color Color;
            public List<Entry> SubEntries { get; private set; }
            public bool Expanded { get; set; }
            public int Depth { get; set; }

            public Entry()
            {
                SubEntries = new List<Entry>();
                Expanded = true;
            }
        }

        public IAntenna Antenna { 
            get { return antenna; }
            set { if (antenna != value) { antenna = value; Refresh(); } }
        }

        private IAntenna antenna;
        private Vector2 scrollPosition1 = Vector2.zero;
        private Vector2 scrollPosition2 = Vector2.zero;
        private Entry rootEntry = new Entry();
        private Entry selection;

        private int targetIndex = 0;
        private Mode currentMode = Mode.PlanetSatellite;

        public AntennaFragment(IAntenna antenna)
        {
            Antenna = antenna;
            RTCore.Instance.Satellites.OnRegister += Refresh;
            RTCore.Instance.Satellites.OnUnregister += Refresh;
            RTCore.Instance.Antennas.OnUnregister += Refresh;
            Refresh();
        }

        public void Dispose()
        {
            if (RTCore.Instance != null)
            {
                RTCore.Instance.Satellites.OnRegister -= Refresh;
                RTCore.Instance.Satellites.OnUnregister -= Refresh;
                RTCore.Instance.Antennas.OnUnregister -= Refresh;
            }
        }
        public void Draw()
        {
            RTGui.HorizontalBlock(() =>
            {
                RTGui.StateButton("P/S", currentMode, Mode.PlanetSatellite, (s) => OnClickMode(Mode.PlanetSatellite, s));
                RTGui.StateButton("P", currentMode, Mode.Planet, (s) => OnClickMode(Mode.Planet, s));
                RTGui.StateButton("S", currentMode, Mode.Satellite, (s) => OnClickMode(Mode.Satellite, s));
                RTGui.StateButton("G", currentMode, Mode.Group, (s) => OnClickMode(Mode.Group, s));
                GUILayout.FlexibleSpace();
            });
            switch (currentMode)
            {
                case Mode.PlanetSatellite:
                    DrawPlanetSatelliteTree();
                    break;
                case Mode.Planet:
                    break;
                case Mode.Satellite:
                    break;
                case Mode.Group:
                    break;
            }
        }

        private void OnClickMode(Mode m, int s)
        {
            currentMode = (s >= 0) ? m : Mode.PlanetSatellite;
        }
        private void DrawPlanetList()
        {

        }

        private void DrawPlanetSatelliteTree()
        {
            RTGui.ScrollViewBlock(ref scrollPosition1, () =>
            {
                Color pushColor = GUI.backgroundColor;
                TextAnchor pushAlign = GUI.skin.button.alignment;
                GUI.skin.button.alignment = TextAnchor.MiddleLeft;
                // Depth-first tree traversal.
                Stack<Entry> dfs = new Stack<Entry>();
                foreach (Entry child in rootEntry.SubEntries)
                {
                    dfs.Push(child);
                }
                while (dfs.Count > 0)
                {
                    Entry current = dfs.Pop();
                    GUI.backgroundColor = current.Color;

                    RTGui.HorizontalBlock(() =>
                    {
                        GUILayout.Space(current.Depth * (GUI.skin.button.margin.left + 24));
                        if (current.SubEntries.Count > 0)
                        {
                            RTGui.Button(current.Expanded ? " <" : " >", () =>
                            {
                                current.Expanded = !current.Expanded;
                            }, GUILayout.Width(24));
                        }
                        RTGui.StateButton(current.Text, selection, current, (s) =>
                        {
                            selection = current;
                            Antenna.Targets[targetIndex] = current.Target;
                        });
                    });

                    if (current.Expanded)
                    {
                        foreach (Entry child in current.SubEntries)
                        {
                            dfs.Push(child);
                        }
                    }
                }

                GUI.skin.button.alignment = pushAlign;
                GUI.backgroundColor = pushColor;
            });
        }

        public void Refresh(IAntenna sat) { if (sat == Antenna) { Antenna = null; } }
        public void Refresh(ISatellite sat) { Refresh(); }
        public void Refresh()
        {
            Dictionary<CelestialBody, Entry> entries = new Dictionary<CelestialBody, Entry>();

            rootEntry = new Entry();
            selection = new Entry()
            {
                Text = "No Target",
                Target = Target.Empty,
                Color = Color.white,
                Depth = 0,
            };
            rootEntry.SubEntries.Add(selection);

            if (Antenna == null) return;

            // Add the planets
            foreach (var cb in RTCore.Instance.Network.Planets)
            {
                if (!entries.ContainsKey(cb.Value))
                {
                    entries[cb.Value] = new Entry();
                }

                var current = entries[cb.Value];
                current.Text = cb.Value.bodyName;
                current.Target = Target.Planet(cb.Value);
                current.Color = cb.Value.GetOrbitDriver() != null ? cb.Value.GetOrbitDriver().orbitColor : Color.yellow;
                current.Color.a = 1.0f;

                if (cb.Value.referenceBody != cb.Value)
                {
                    var parent = cb.Value.referenceBody;
                    if (!entries.ContainsKey(parent))
                    {
                        entries[parent] = new Entry();
                    }
                    entries[parent].SubEntries.Add(current);
                }
                else
                {
                    rootEntry.SubEntries.Add(current);
                }

                if (Antenna.Targets.Count < targetIndex && current.Target.Includes(cb.Value))
                {
                    selection = current;
                }
            }

            // Sort the lists based on semi-major axis. In reverse because of how we render it.
            foreach (var entryPair in entries)
            {
                entryPair.Value.SubEntries.Sort((b, a) =>
                {
                    return RTCore.Instance.Network.Planets[a.Target.GetTargets().First().Guid].orbit.semiMajorAxis.CompareTo(
                           RTCore.Instance.Network.Planets[b.Target.GetTargets().First().Guid].orbit.semiMajorAxis);
                });
            }

            // Add the satellites.
            foreach (ISatellite s in RTCore.Instance.Network)
            {
                if (s.Guid == Antenna.Guid) continue;
                Entry current = new Entry()
                {
                    Text = s.Name,
                    Target = Target.Single(s),
                    Color = Color.white,
                };
                entries[s.Body].SubEntries.Add(current);

                if (Antenna.Targets.Count < targetIndex && current.Target.Equals(Antenna.Targets[targetIndex]))
                {
                    selection = current;
                }
            }

            // Set a local depth variable so we can refer to it when rendering.
            rootEntry.SubEntries.Reverse();
            Stack<Entry> dfs = new Stack<Entry>();
            foreach (Entry child in rootEntry.SubEntries)
            {
                child.Depth = 0;
                dfs.Push(child);
            }
            while (dfs.Count > 0)
            {
                Entry current = dfs.Pop();
                foreach (Entry child in current.SubEntries)
                {
                    child.Depth = current.Depth + 1;
                    dfs.Push(child);
                }
            }
        }
    }
}
