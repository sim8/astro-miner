using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace AstroMiner;

public class ControlMapper<TEnum> where TEnum : Enum
{
    private readonly HashSet<TEnum> _activeControls = new();
    private readonly Dictionary<TEnum, ControlMapping> _mappings = new();
    private readonly HashSet<TEnum> _triggeredControls = new();
    private GamePadState _previousGamePadState;

    public void AddMapping(TEnum control, Keys key, Buttons? button, bool isContinuous)
    {
        _mappings[control] = new ControlMapping { Key = key, Button = button, IsContinuous = isContinuous };
    }

    public HashSet<TEnum> GetActiveControls(KeyboardState keyboardState, GamePadState gamePadState)
    {
        _activeControls.Clear();

        foreach (var mapping in _mappings)
        {
            var control = mapping.Key;
            var key = mapping.Value.Key;
            var button = mapping.Value.Button;
            var isContinuous = mapping.Value.IsContinuous;

            var isActive = keyboardState.IsKeyDown(key);

            // Check gamepad button if one is mapped
            if (button.HasValue)
            {
                if (button.Value == Buttons.LeftThumbstickUp)
                    isActive |= gamePadState.ThumbSticks.Left.Y > 0.5f;
                else if (button.Value == Buttons.LeftThumbstickDown)
                    isActive |= gamePadState.ThumbSticks.Left.Y < -0.5f;
                else if (button.Value == Buttons.LeftThumbstickLeft)
                    isActive |= gamePadState.ThumbSticks.Left.X < -0.5f;
                else if (button.Value == Buttons.LeftThumbstickRight)
                    isActive |= gamePadState.ThumbSticks.Left.X > 0.5f;
                else
                    isActive |= gamePadState.IsButtonDown(button.Value);
            }

            if (isActive)
            {
                if (isContinuous)
                {
                    _activeControls.Add(control);
                }
                else
                {
                    var wasButtonPressed = button.HasValue &&
                                           !_previousGamePadState.IsButtonDown(button.Value) &&
                                           gamePadState.IsButtonDown(button.Value);

                    if (!_triggeredControls.Contains(control) || wasButtonPressed)
                    {
                        _activeControls.Add(control);
                        _triggeredControls.Add(control);
                    }
                }
            }
            else
            {
                _triggeredControls.Remove(control);
            }
        }

        _previousGamePadState = gamePadState;
        return _activeControls;
    }

    private class ControlMapping
    {
        public Keys Key { get; set; }
        public Buttons? Button { get; set; }
        public bool IsContinuous { get; set; }
    }
}