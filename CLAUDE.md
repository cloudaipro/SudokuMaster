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
- Hell: (81, 72+) - Ultra-challenging with manual validation system

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

## Hell Level Feature Implementation

### Implementation Progress
**Current Status**: Phase 4 Complete - Cell Interaction & Visual Feedback implemented ✅
**Target**: Ultra-challenging difficulty with manual validation system
**Next**: Phase 5 - Onboarding & Tutorial

### Hell Level Architecture
- **Strategy Pattern**: `IValidationStrategy` interface with `ImmediateValidationStrategy` (normal levels) and `HypothesisValidationStrategy` (Hell level) ✅
- **ValidationContext**: Central manager handling strategy switching based on game mode ✅
- **Manual Validation**: Players can place multiple numbers to test theories, then manually validate when ready ✅
- **Graded Feedback**: Comprehensive validation results with completion percentage and error analysis ✅
- **Dataset Integration**: Hell Level puzzle loading and progression tracking ✅
- **SudokuBoard Integration**: ValidationContext automatically initialized and strategy switching active ✅
- **UI Components**: Complete Hell Level UI system with mode indicator, validation button, progress tracking, and graded feedback modal ✅
- **Manual Validation System**: Players can test multiple theories then validate when ready ✅
- **Cell Interaction**: Visual feedback for hypothesis vs confirmed numbers with orange styling ✅
- **Visual Feedback System**: Error highlighting, progress tracking, and comprehensive UI feedback ✅
- **Hell Level Theme**: Cohesive "Hell Fire" visual design with dramatic red/orange color palette ✅
- **No Assistance**: Fast Note and Hint systems disabled for Hell Level (pending)

### Files Implemented
- `Assets/Scripts/EGameMode.cs` - Added HELL enum ✅
- `Assets/Scripts/Validation/IValidationStrategy.cs` - Interface and ValidationResult classes ✅  
- `Assets/Scripts/Validation/ImmediateValidationStrategy.cs` - Normal level validation ✅
- `Assets/Scripts/Validation/HypothesisValidationStrategy.cs` - Hell level deferred validation ✅
- `Assets/Scripts/Validation/ValidationContext.cs` - Strategy management and event system ✅
- `Assets/Scripts/MenuButtons.cs` - Added LoadHellScene method ✅
- `Assets/Scripts/NumberLockVisualFeedback.cs` - Fixed font compatibility ✅
- `Assets/Scripts/Setting.cs` - Added HellLevel and LastDateOfHellLevel properties ✅
- `Assets/Scripts/SudokuData.cs` - Added SudokuHellData class and hell case support ✅
- `Assets/Scripts/SudokuBoard.cs` - Integrated ValidationContext and Hell Level UI ✅
- `Assets/Resources/Dataset/hell/` - Ultra-challenging puzzle dataset (pre-existing) ✅
- `Assets/Scripts/HellLevelModeIndicator.cs` - Mode indicator with pulse animation ✅
- `Assets/Scripts/ManualValidationButton.cs` - Manual validation button with state management ✅
- `Assets/Scripts/SolutionProgressIndicator.cs` - Circular progress indicator with cell tracking ✅
- `Assets/Scripts/ValidationResultModal.cs` - Comprehensive graded feedback modal ✅
- `Assets/Scripts/VisualFeedbackManager.cs` - Visual feedback system for error highlighting ✅
- `Assets/Scripts/SudokuCell.cs` - Updated for Hell Level hypothesis number support ✅

### Implementation Subtasks Checklist
**Phase 1: Core Architecture & Game Mode Support** ✅ COMPLETE
- [x] 1.1: Add HELL enum to EGameMode
- [x] 1.2: Create IValidationStrategy interface
- [x] 1.3: Implement ImmediateValidationStrategy
- [x] 1.4: Implement HypothesisValidationStrategy
- [x] 1.5: Create ValidationContext manager
- [x] 1.6: Update MenuButtons.cs for Hell Level

**Phase 2: Data & Puzzle Integration** ✅ COMPLETE
- [x] 2.1: Create Hell Level puzzle dataset structure
- [x] 2.2: Update SudokuData.cs for Hell Level
- [x] 2.3: Update SudokuBoard.cs integration

**Phase 3: UI Components & Visual Design** ✅ COMPLETE
- [x] 3.1: Create Hell Level mode indicator UI
- [x] 3.2: Implement Manual Validation Button
- [x] 3.3: Add Solution Progress Indicator
- [x] 3.4: Design Validation Results Modal

**Phase 4: Cell Interaction & Visual Feedback** ✅ COMPLETE
- [x] 4.1: Update SudokuCell.cs for Hell Level
- [x] 4.2: Implement visual feedback system
- [x] 4.3: Fix UI component positioning
- [x] 4.4: Implement cohesive Hell Level visual design

**Phase 5: Onboarding & Tutorial**
- [ ] 5.1: Create Hell Level introduction screen
- [ ] 5.2: Implement tutorial system

**Phase 6: Integration & Polish**
- [ ] 6.1: Update game state management
- [ ] 6.2: Disable assistance features for Hell Level
- [ ] 6.3: Performance optimization
- [ ] 6.4: Testing & validation

### Key Technical Achievements

**Hell Level System Features:**
- **Hypothesis Testing**: Players can place multiple numbers without immediate validation
- **Orange Hypothesis Styling**: Visual distinction between hypothesis and confirmed numbers
- **Manual Validation**: "🔥 VALIDATE" button with state management (enabled/disabled/loading)
- **Graded Feedback**: Comprehensive modal showing completion percentage and error analysis
- **Progress Tracking**: Circular indicator showing filled cells (e.g., "45/81")
- **Hell Fire Theme**: Cohesive visual design with deep reds, fire oranges, and dramatic styling
- **Error Highlighting**: Visual feedback for validation errors with Hell-themed colors
- **Mode Indicator**: Pulsing "🔥 HELL LEVEL 🔥" banner with dramatic animation

**Technical Implementation:**
- **Strategy Pattern**: Clean separation between normal and Hell Level validation logic
- **Event-Driven Architecture**: ValidationContext events for UI synchronization
- **Programmatic UI**: All Hell Level components created in code for flexibility
- **Visual Consistency**: Bold typography and cohesive color palette across all components
- **Performance Optimized**: Efficient cell tracking and state management

**IMPORTANT**: Complete each subtask fully and get user approval before proceeding to the next. This prevents interruptions from disrupting the implementation flow.

## Build Configuration

The project includes Android-specific settings and iOS bitcode configuration. Google Play integration requires proper signing and App Bundle configuration for store submission.