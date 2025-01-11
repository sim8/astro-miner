using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace AstroMiner;

public class ControlMapper<TEnum> where TEnum : Enum
{
    private readonly HashSet<TEnum> _activeControls = new();
    private readonly Dictionary<TEnum, ControlMapping> _mappings = new();
    private readonly HashSet<TEnum> _triggeredControls = new();

    public void AddMapping(TEnum control, Keys key, bool isContinuous)
    {
        _mappings[control] = new ControlMapping { Key = key, IsContinuous = isContinuous };
    }

    public HashSet<TEnum> GetActiveControls(KeyboardState keyboardState)
    {
        _activeControls.Clear();

        foreach (var mapping in _mappings)
        {
            var control = mapping.Key;
            var key = mapping.Value.Key;
            var isContinuous = mapping.Value.IsContinuous;

            if (keyboardState.IsKeyDown(key))
            {
                if (isContinuous)
                {
                    // Add continuous controls every frame
                    _activeControls.Add(control);
                }
                else
                {
                    // Add non-continuous controls only if not already triggered
                    if (!_triggeredControls.Contains(control))
                    {
                        _activeControls.Add(control);
                        _triggeredControls.Add(control);
                    }
                }
            }
            else
            {
                // If the key is released, remove it from triggered controls
                _triggeredControls.Remove(control);
            }
        }

        return _activeControls;
    }

    private class ControlMapping
    {
        public Keys Key { get; set; }
        public bool IsContinuous { get; set; }
    }
}