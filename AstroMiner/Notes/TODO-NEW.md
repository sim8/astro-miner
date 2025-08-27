# TODO (really)

- Fix death/off asteroid states
    - Shouldn't crash if out of bounds (edge case)
- Simplify UI code? Callback function to set props?
    - ALSO shouldn't manually be doing `* game.StateManager.Ui.UIScale`
- More aggressive fog-of-warw
- Improve entity management
    - Get static entities (merge into dynamic)
    - CreateIfNotExisting (maybe using predetermined IDs?) for things like NPCs