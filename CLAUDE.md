# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

SudokuMaster is a Unity-based Sudoku game with multiple difficulty levels, in-app purchases, and Google Play integration. The project targets Unity 2021.3.24f1 and includes Android app bundle publishing capabilities.

## Unity Development Commands

Since this is a Unity project, most development tasks are performed within the Unity Editor:

- **Open Project**: Open the project in Unity 2021.3.24f1 or compatible version
- **Build Android**: File → Build Settings → Android → Build (or Build And Run)
- **Build iOS**: File → Build Settings → iOS → Build
- **Run Tests**: Window → General → Test Runner (if test assemblies exist)

## Sudoku Data Generation

The project includes a custom Sudoku generator system:

**Compile generator** (from `Assets/Scripts/CCSUnityToolkit/SudokuGenerator/`):
```bash
mcs Generator.cs Board.cs Cell.cs Solver.cs ../CommonExtensions.cs
```

**Generate puzzles**:
```bash
# By difficulty name
mono ./Generator.exe 100 Hard Hard.txt

# By cutoff number (custom difficulty)
mono ./Generator.exe 1000 10 medium.txt
mono ./Generator.exe 1000 15 hard.txt
mono ./Generator.exe 500 72 extreme.txt
```

**Difficulty mappings**:
- Easy: (35, 0) - 35 logical reductions, 0 random
- Medium: (81, 5) - 81 logical, 5 random reductions
- Hard: (81, 15) - 81 logical, 15 random reductions
- Extreme: (81, 45) - 81 logical, 45 random reductions

## Architecture Overview

### Core Game Systems

**Game State Management**:
- `GameSettings`: Singleton managing game mode, pause state, and continuation flags
- `Config`: Handles save/load of game progress using JSON serialization
- `GameProgress`: Data structure for persistent game state

**Sudoku Engine**:
- `SUDOKUcore/`: Advanced Sudoku solving engine with multiple algorithms
  - `00 ApplicationMain/`: Core puzzle classes (G, GSudoku, NuPz_Solver, NuPz_Display)
  - `20 SuDoKu_Ver4.0/`: Main solving algorithms and control logic
  - `25 GNPX_analyzer/`: Advanced solving techniques (fish, chains, wings, etc.)
- `SudokuBoard`: Unity-specific board management and UI integration
- `SudokuCell`: Individual cell behavior and validation

**UI Systems**:
- `MenuButtons`: Scene navigation and game mode selection
- `SudokuCell`/`SudokuNote`: Interactive cell components with notes support
- `NumberLockManager`: Number lock system for rapid input (hold to lock numbers)
- `NumberButton`: Enhanced number buttons with lock functionality
- `Clock`: Game timer with pause/resume functionality
- `Lives`: Error tracking system
- `HintButton`: Hint system integration

### Data Flow

1. **Game Start**: `GameSettings` determines mode → Load puzzle from `Assets/Resources/Dataset/`
2. **Gameplay**: `SudokuBoard` manages state → `SudokuCell` handles input → Validation via core engine
3. **Number Lock**: Hold number button → `NumberLockManager` locks number → Click cells for rapid input
4. **Notes Integration**: Note mode + locked numbers = places locked number as notes in cells
5. **Save/Load**: `Config` serializes complete game state including notes and timer
6. **Puzzle Generation**: External generator creates datasets stored in Resources

### Monetization Integration

- **IAP System**: `Assets/Scripts/IAP/` - Unity IAP integration
- **Ad Manager**: `AdManager.cs` - Advertisement integration
- **Google Play**: Review API and App Bundle support via GooglePlayPlugins

### Key File Locations

- **Scenes**: `Assets/Scenes/` (MainMenu.unity, GameScene.unity)
- **Prefabs**: `Assets/Prefabs/` (SudokuCell, SudokuNote)
- **Puzzle Data**: `Assets/Resources/Dataset/[difficulty]/` (txt files with puzzle arrays)
- **Scripts**: `Assets/Scripts/` (game logic) + `Assets/Scripts/SUDOKUcore/` (engine)
- **Number Lock System**: `NumberLockManager.cs`, `NumberButton.cs` with enhanced hold-to-lock functionality

## Development Notes

- Game uses persistent data path for save files (`Application.persistentDataPath`)
- Puzzle validation leverages the comprehensive SUDOKUcore engine with 50+ solving techniques
- Google Play dependencies managed via External Dependency Manager
- Android builds support App Bundle format for Play Store distribution
- Notes system supports multiple candidate numbers per cell
- Number lock system: Hold number buttons to lock, works with both normal and note modes
- Recent bug fix: Fixed double invocation issue in locked number + note mode by moving logic to OnPointerDown
- Error tracking limits mistakes before game over

## Build Configuration

The project includes Android-specific settings and iOS bitcode configuration. Google Play integration requires proper signing and App Bundle configuration for store submission.