# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

AstroMiner is a 2D mining game built with MonoGame Framework (.NET 8.0). Players control a character and/or mining vehicle to explore procedurally generated asteroids, collect resources, and manage inventory. The game features an Entity-Component-System (ECS) architecture, multiple world states, and serializable game state.

## Build and Development Commands

This is a .NET solution using MSBuild:

```bash
# Build the solution
dotnet build

# Run the game
dotnet run --project AstroMiner/AstroMiner.csproj

# Run tests
dotnet test

# Clean build artifacts
dotnet clean

# Restore packages and tools
dotnet restore
```

## Core Architecture

### Game Structure
- **BaseGame**: Abstract base class providing core game infrastructure
- **AstroMinerGame**: Main game class inheriting from BaseGame
- **GameStateManager**: Central coordinator managing all game systems and world states
- **GameModel**: Serializable data model containing complete game state

### World System
The game supports multiple world types:
- **AsteroidWorld**: Procedurally generated mining environments
- **StaticWorld**: Predefined areas like the launch pad and shop

All world states inherit from `BaseWorldState` and must implement collision detection and grid size methods.

### Entity-Component-System (ECS)
- **Ecs.cs**: Main ECS manager handling entities and components
- **EntityFactories**: Factory methods for creating common entity types
- **Systems/**: Individual systems processing specific component combinations (Movement, Health, Mining, etc.)
- **Components/**: Pure data containers for entity properties

### State Management and Persistence
- **GameStateStorage**: Handles JSON serialization/deserialization of entire game state
- **GameModel**: Root serializable data structure
- Custom JSON converters for MonoGame types (Vector2, Color, RectangleF)
- Save files stored on desktop as `game_save_2.json`

### Rendering Architecture
- **Renderer**: Main rendering coordinator
- **BaseWorldRenderer**: Abstract base for world-specific rendering
- Specialized renderers for entities, UI, and world types
- Multiple render layers with depth sorting

### UI System
- **UI/UI.cs**: Main UI manager
- **UIElement**: Base class for all UI components
- **UIScreen**: Container for grouped UI elements
- Modal dialogs and inventory management

### Procedural Generation
- **AsteroidGen.cs**: Main asteroid generation logic
- **CellGenRules.cs**: Rule-based cell type generation
- **PerlinNoiseGenerator.cs**: Noise generation for terrain
- Grid-based system with different cell types (walls, floors, resources)

## Key Patterns and Conventions

### Component Management
Components are stored in type-specific dictionaries within `ComponentsByEntityId`. The ECS uses manual type checking rather than reflection for performance.

### Control System
- **ControlMapping.cs**: Maps input to game actions
- **ActiveControls**: Container for currently active control inputs
- Separate control contexts for different game modes (mining, menu, global)

### Game State Transitions
- **TransitionManager**: Handles smooth transitions between game states
- **NewGameManager**: Sets up fresh game state
- World switching handled through `GameStateManager.SetActiveWorldAndInitialize()`

### Resource Management
- **Inventory**: Manages player items and credits
- **ItemTypes**: Enumeration of all available items
- Grid-based inventory system with selected item tracking

## Testing
Uses MSTest framework with test projects:
- Unit tests for core systems
- Serialization/deserialization tests
- Procedural generation validation