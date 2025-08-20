# SudokuMaster

A feature-rich Sudoku game built with Unity, offering multiple difficulty levels, advanced solving algorithms, and comprehensive mobile platform support.

## Features

- **Multiple Difficulty Levels**: Easy, Medium, Hard, and Extreme puzzles
- **Advanced Sudoku Engine**: 50+ solving techniques including fish patterns, chains, and wings
- **Save/Resume Functionality**: Complete game state persistence with notes and timer
- **Notes System**: Multiple candidate number support per cell
- **Number Lock Feature**: Hold number buttons to lock them for rapid input across cells
- **Hint System**: Intelligent hints powered by the solving engine
- **Error Tracking**: Mistake counter with game-over mechanics
- **Monetization Ready**: In-app purchases and advertisement integration
- **Google Play Integration**: Review API and App Bundle support
- **Cross-Platform**: Android and iOS deployment ready

## Requirements

- Unity 2021.3.24f1 or compatible version
- .NET Framework support
- Android SDK (for Android builds)
- Xcode (for iOS builds)

## Project Structure

```
Assets/
├── Scenes/
│   ├── MainMenu.unity          # Main menu scene
│   └── GameScene.unity         # Gameplay scene
├── Scripts/
│   ├── SUDOKUcore/            # Advanced solving engine
│   ├── CCSUnityToolkit/       # Sudoku generator and utilities
│   ├── IAP/                   # In-app purchase system
│   ├── SudokuBoard.cs         # Main game board controller
│   ├── SudokuCell.cs          # Individual cell behavior
│   ├── GameSettings.cs        # Global game state management
│   └── Config.cs              # Save/load functionality
├── Prefabs/
│   ├── SudokuCell.prefab      # Interactive cell component
│   └── SudokuNote.prefab      # Notes display component
├── Resources/
│   └── Dataset/               # Puzzle data files
│       ├── easy/              # Easy difficulty puzzles
│       ├── medium/            # Medium difficulty puzzles
│       ├── hard/              # Hard difficulty puzzles
│       └── extreme/           # Extreme difficulty puzzles
└── Graphics/                  # Game assets and UI elements
```

## Getting Started

### Opening the Project

1. Install Unity 2021.3.24f1 or compatible version
2. Open Unity Hub
3. Click "Open" and select the SudokuMaster project folder
4. Wait for Unity to import and compile the project

### Building the Game

#### Android Build
1. Go to **File → Build Settings**
2. Select **Android** platform
3. Click **Switch Platform** if needed
4. Configure **Player Settings** for your target
5. Click **Build** or **Build And Run**

#### iOS Build
1. Go to **File → Build Settings**
2. Select **iOS** platform
3. Click **Switch Platform** if needed
4. Configure **Player Settings** for iOS
5. Click **Build** to generate Xcode project

### Generating Custom Puzzles

The project includes a custom Sudoku generator for creating new puzzle datasets:

#### Compile the Generator
Navigate to `Assets/Scripts/CCSUnityToolkit/SudokuGenerator/` and run:
```bash
mcs Generator.cs Board.cs Cell.cs Solver.cs ../CommonExtensions.cs
```

#### Generate Puzzles
```bash
# Generate by difficulty name
mono ./Generator.exe 100 Hard Hard.txt

# Generate by custom parameters
mono ./Generator.exe 1000 10 medium.txt
mono ./Generator.exe 1000 15 hard.txt
mono ./Generator.exe 500 72 extreme.txt
```

#### Difficulty Parameters
- **Easy**: (35, 0) - 35 logical reductions, 0 random
- **Medium**: (81, 5) - 81 logical, 5 random reductions  
- **Hard**: (81, 15) - 81 logical, 15 random reductions
- **Extreme**: (81, 45) - 81 logical, 45 random reductions

## Game Architecture

### Core Systems

**Game State Management**
- `GameSettings`: Singleton managing game modes and state
- `Config`: JSON-based save/load system
- `GameProgress`: Persistent game data structure

**Sudoku Engine**
- Advanced solving algorithms with 50+ techniques
- Support for complex patterns like X-Wings, Swordfish, and chains
- Intelligent hint generation
- Complete puzzle validation

**UI Systems**
- Interactive cell grid with touch/mouse support
- Notes system for candidate numbers
- Number lock system for rapid input (hold number buttons to lock)
- Timer with pause/resume functionality
- Error tracking and game-over handling

### Data Flow

1. **Initialization**: `GameSettings` loads configuration and determines game mode
2. **Puzzle Loading**: Board loads puzzle data from Resources/Dataset
3. **Gameplay Loop**: User input → validation → UI update → save state
4. **Completion**: Win detection → score calculation → progression

## Monetization Features

### In-App Purchases
- Hint packages
- Ad removal
- Premium features
- Unity IAP integration ready

### Advertisement Integration
- Rewarded video ads for hints
- Interstitial ads between games
- Banner ad support

### Google Play Services
- Achievement system ready
- Leaderboards support
- In-app review prompts
- App Bundle optimization

## Development Notes

- Game data saves to `Application.persistentDataPath`
- Puzzle validation uses the comprehensive SUDOKUcore engine
- Error handling includes graceful degradation for invalid puzzles
- Memory-efficient design suitable for mobile devices
- Supports both portrait and landscape orientations

## Contributing

When working with this codebase:

1. Follow Unity naming conventions
2. Test changes on both Android and iOS when possible
3. Validate new puzzles with the solving engine
4. Maintain backward compatibility for save files
5. Document any new solving techniques added to the engine

## License

This project includes third-party components:
- Google Play Plugins (Google)
- External Dependency Manager (Google)
- Newtonsoft.Json (Unity Package)

Please refer to individual component licenses for usage terms.

## Support

For technical issues or questions about the codebase, refer to the included documentation and code comments throughout the project.