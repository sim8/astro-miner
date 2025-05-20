using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace AstroMiner;

public enum MiningControls
{
    // Movement
    MoveUp,
    MoveRight,
    MoveDown,
    MoveLeft,

    Drill,
    ExitVehicle,

    // Player-only
    UseItem,
    Interact,

    // Miner-only
    UseGrapple,

    OpenInventory,
    NewGameOrReturnToBase, // TODO factor out
    SaveGame // TEMP
}

/// <summary>
/// Contains all active control sets that can be passed to game systems
/// </summary>
public class ActiveControls
{
    public HashSet<MiningControls> Mining { get; set; } = new();
    // Add more control sets as needed (e.g. MenuControls, InventoryControls, etc.)
}

/// <summary>
/// Central manager for all game controls
/// </summary>
public class ControlManager
{
    private readonly ControlMapper<MiningControls> _miningControlMapper = new();
    private readonly ActiveControls _activeControls = new();
    private bool _miningControlsEnabled = true;

    public ControlManager()
    {
        InitializeControls();
    }

    private void InitializeControls()
    {
        // Mining controls
        _miningControlMapper.AddMapping(MiningControls.MoveUp, Keys.W, Buttons.LeftThumbstickUp, true);
        _miningControlMapper.AddMapping(MiningControls.MoveRight, Keys.D, Buttons.LeftThumbstickRight, true);
        _miningControlMapper.AddMapping(MiningControls.MoveDown, Keys.S, Buttons.LeftThumbstickDown, true);
        _miningControlMapper.AddMapping(MiningControls.MoveLeft, Keys.A, Buttons.LeftThumbstickLeft, true);
        _miningControlMapper.AddMapping(MiningControls.Drill, Keys.Space, Buttons.RightTrigger, true);
        _miningControlMapper.AddMapping(MiningControls.UseItem, Keys.Space, Buttons.RightTrigger, true);
        _miningControlMapper.AddMapping(MiningControls.Interact, Keys.E, Buttons.Y, false);
        _miningControlMapper.AddMapping(MiningControls.ExitVehicle, Keys.E, Buttons.Y, false);
        _miningControlMapper.AddMapping(MiningControls.UseGrapple, Keys.G, Buttons.LeftTrigger, true);
        _miningControlMapper.AddMapping(MiningControls.NewGameOrReturnToBase, Keys.N, Buttons.Start, false);
        _miningControlMapper.AddMapping(MiningControls.SaveGame, Keys.B, Buttons.Back, false);
        _miningControlMapper.AddMapping(MiningControls.OpenInventory, Keys.Tab, Buttons.Back, false);

        // You can add additional control sets here as needed
    }

    /// <summary>
    /// Enables or disables mining controls
    /// </summary>
    public void EnableMiningControls(bool enabled)
    {
        _miningControlsEnabled = enabled;
    }

    /// <summary>
    /// Updates all control states based on current input and returns active controls
    /// </summary>
    public ActiveControls Update(KeyboardState keyboardState, GamePadState gamePadState)
    {
        if (_miningControlsEnabled)
        {
            _activeControls.Mining = _miningControlMapper.GetActiveControls(keyboardState, gamePadState);
        }
        else
        {
            _activeControls.Mining = new HashSet<MiningControls>();
        }

        // Update other control sets here as they are added

        return _activeControls;
    }
}

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