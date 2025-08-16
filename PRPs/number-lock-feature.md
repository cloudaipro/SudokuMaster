# PRP: Number Lock Feature Implementation

## Overview

Implement a Number Lock/Pin Mode feature for the SudokuMaster Unity game that allows users to lock a specific number (1-9) for rapid sequential entry across multiple cells. This feature will enhance gameplay efficiency for intermediate to advanced players.

## Context and Requirements

### Feature Requirements (from PRD)
- **Activation**: 3-second long press on any number button (1-9)
- **Visual Feedback**: Enlarged number overlay + progress bar during hold
- **Locked State**: Distinct visual styling, single number lock limitation
- **Auto-Fill**: Clicking empty cells automatically enters locked number
- **Deactivation**: Single tap on locked number
- **Persistence**: Lock state survives game save/load

## Codebase Analysis

### Current Architecture Patterns

**Number Input System** (`Assets/Scripts/NumberButton.cs:7-112`):
- Uses `IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler`
- Smooth transitions via coroutines (lines 95-110)
- Triggers `GameEvents.UpdateSquareNumberMethod(value)` on click (line 91)

**Event Communication** (`Assets/Scripts/GameEvents.cs:6-96`):
- Static delegate pattern for component communication
- Example: `UpdateSquareNumber` event (lines 8-10)
- All components listen/trigger via GameEvents

**State Management Patterns**:
- **Toggle State**: `NoteButton.cs:13-28` shows toggle with visual feedback
- **Global State**: `GameSettings.cs:6-41` singleton pattern
- **Persistence**: `GameProgress.cs:6-19` + `Config.cs:26-106` JSON serialization

**Cell Input Handling** (`Assets/Scripts/SudokuCell.cs:251-282`):
- Listens to `GameEvents.onUpdateSquareNumber`
- Processes input in `OnSetNumber(int number)` method
- Respects game state (pause, selected cell, etc.)

**Visual Transitions** (`Assets/Scripts/NumberButton.cs:95-110`):
- Coroutine-based smooth transitions
- Color and scale interpolation over time
- Pattern: `Vector3.Lerp(startSize, newSize, timer / transitionTime)`

## External Research References

### Unity UI Long Press Detection
- **Documentation**: https://docs.unity3d.com/Packages/com.unity.ugui@1.0/api/UnityEngine.EventSystems.IPointerDownHandler.html
- **Community Examples**: https://discussions.unity.com/t/long-press-gesture-on-ugui-button/548218
- **Implementation Pattern**: Timer-based approach using `Time.time - startTime > threshold`

### UI Animation Patterns
- **Button Transitions**: https://medium.com/geekculture/unity-ui-animated-buttons-c1f48b59625f
- **Coroutine Animations**: https://gamedevbeginner.com/coroutines-in-unity-when-and-how-to-use-them/
- **Progress Bars**: https://docs.unity3d.com/6000.0/Documentation/Manual/UIE-uxml-element-ProgressBar.html

## Implementation Blueprint

### Phase 1: Extend GameEvents Communication
**File**: `Assets/Scripts/GameEvents.cs`
**Location**: Add after line 38 (following existing pattern)

```csharp
// Number Lock Events
public delegate void NumberLocked(int number);
public static event NumberLocked OnNumberLocked;
public static void OnNumberLockedMethod(int number) => OnNumberLocked?.Invoke(number);

public delegate void NumberUnlocked();
public static event NumberUnlocked OnNumberUnlocked;  
public static void OnNumberUnlockedMethod() => OnNumberUnlocked?.Invoke();
```

### Phase 2: Extend NumberButton with Long Press
**File**: `Assets/Scripts/NumberButton.cs`
**Pattern**: Extend existing interface implementation (line 7)

```csharp
public class NumberButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, 
    IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler
{
    // Add new fields
    public static int LockedNumber = -1; // Global lock state
    [SerializeField] private float holdDuration = 3.0f;
    [SerializeField] private GameObject lockOverlay; // Enlarged number display
    [SerializeField] private GameObject progressBar; // Hold progress indicator
    [SerializeField] private GameObject lockIndicator; // Lock state badge
    
    private bool isHoldingDown = false;
    private float holdStartTime = 0f;
    private Coroutine holdCoroutine = null;
    
    // Extend existing methods...
}
```

**Key Methods**:
- `StartHoldGesture()`: Begin 3-second timer + visual feedback
- `StopHoldGesture()`: Cancel hold if released early
- `ActivateLock()`: Set lock state + visual update
- `DeactivateLock()`: Clear lock state
- `UpdateHoldProgress()`: Coroutine for progress bar animation

### Phase 3: Visual Feedback Components

**Hold Overlay**: Enlarged number display during gesture
- Parent to NumberButton, initially inactive
- Scale up animation using existing transition pattern
- Text shows button's number value

**Progress Bar**: 3-second countdown indicator  
- Fill amount: `timer / holdDuration`
- Smooth update via coroutine
- Color transition: start → success color

**Lock Indicator**: Visual state when locked
- Badge/background highlight (reference `NoteButton.cs:23` pattern)
- Distinct color scheme
- Toggle active based on lock state

### Phase 4: Modify Cell Input Handling
**File**: `Assets/Scripts/SudokuCell.cs:251-282`
**Pattern**: Extend existing `OnSetNumber(int number)` method

```csharp
public void OnSetNumber(int number)
{
    if (IsSelected && !Has_default_value)
    {
        // Check if we're in auto-fill mode (number is locked)
        if (NumberButton.LockedNumber > 0 && NumberButton.LockedNumber != number)
        {
            // Auto-fill with locked number instead
            number = NumberButton.LockedNumber;
        }
        
        // Existing logic continues...
    }
}
```

**Add Click Handler for Auto-Fill**:
```csharp
public override void OnPointerDown(PointerEventData eventData)
{
    if (GameSettings.Instance.bBoardInteractable)
    {
        // If number is locked and this is an empty cell, auto-fill
        if (NumberButton.LockedNumber > 0 && Number == 0 && !Has_default_value)
        {
            GameEvents.UpdateSquareNumberMethod(NumberButton.LockedNumber);
        }
        
        GameEvents.SquareSelectedMethod(Cell_index);
    }
}
```

### Phase 5: State Persistence
**File**: `Assets/Scripts/GameProgress.cs:18`
```csharp
[Serializable]
public class GameProgress
{
    // Existing fields...
    public int lockedNumber = -1; // Add number lock state
}
```

**File**: `Assets/Scripts/Config.cs:36-60`
**Pattern**: Extend `SaveBoardData` method signature and implementation

```csharp
public static void SaveBoardData(SudokuData.SudokuBoardData board_data, EGameMode gameMode, 
    int level, int selected_index_at_dataOfLevel, int error_number, 
    Dictionary<int, List<int>> grid_notes, bool[] hasDefaultFlags, bool noteHintMode, 
    int lockedNumber) // Add parameter
{
    // Existing logic...
    GameProgress saveObj = new GameProgress
    {
        // Existing fields...
        lockedNumber = lockedNumber, // Add to save object
    };
    // Rest of save logic...
}
```

## Implementation Tasks (Sequential Order)

1. **Setup Phase**
   - [ ] Add NumberLocked/NumberUnlocked events to GameEvents.cs
   - [ ] Create lock state management in NumberButton.cs

2. **Gesture Detection**  
   - [ ] Implement IPointerUpHandler in NumberButton.cs
   - [ ] Add hold timer logic using Time.time approach
   - [ ] Create hold gesture start/stop methods

3. **Visual Feedback**
   - [ ] Create lock overlay GameObject hierarchy
   - [ ] Implement progress bar coroutine animation
   - [ ] Add lock indicator visual state

4. **Lock State Management**
   - [ ] Implement ActivateLock/DeactivateLock methods
   - [ ] Add lock state visual updates
   - [ ] Ensure single number lock limitation

5. **Input Integration**
   - [ ] Modify SudokuCell.OnSetNumber for auto-fill logic
   - [ ] Update SudokuCell.OnPointerDown for locked input
   - [ ] Test interaction with existing game mechanics

6. **State Persistence**
   - [ ] Extend GameProgress with lockedNumber field
   - [ ] Update Config.SaveBoardData method
   - [ ] Update Config.LoadBoardData handling
   - [ ] Add lock state restoration on game load

7. **Integration Testing**
   - [ ] Test hold gesture timing and visual feedback
   - [ ] Test lock/unlock functionality
   - [ ] Test auto-fill behavior
   - [ ] Test save/load persistence
   - [ ] Test edge cases (pause during hold, etc.)

## Validation Gates

### Unity Editor Testing
```bash
# Open Unity project
# Test basic functionality:
# 1. Long press number button for 3 seconds
# 2. Verify progress bar and overlay appear
# 3. Verify lock state activates with visual indicator
# 4. Click empty cells to verify auto-fill
# 5. Single tap locked number to deactivate
# 6. Test save/load with locked state
```

### Code Quality Checks
- No compile errors
- All coroutines properly started/stopped
- Event subscriptions have matching unsubscriptions
- Lock state properly resets on game state changes
- Visual elements properly show/hide

### Integration Validation
- Works with existing pause system
- Compatible with notes mode
- Respects game board interactable state
- Maintains existing save/load functionality
- No performance impact on number button interactions

## Error Handling & Edge Cases

**Coroutine Lifecycle**: Ensure hold coroutines stop when:
- User releases finger (OnPointerUp/OnPointerExit)
- Game pauses (GameSettings.Instance.Pause)
- Scene changes or component destruction

**State Cleanup**: Reset lock state when:
- New game starts
- Game over occurs
- User exits to main menu

**Input Validation**: 
- Prevent lock activation during pause
- Respect existing game board interactable state
- Handle rapid successive presses gracefully

## Success Metrics

- Hold gesture activates consistently within 3.1 seconds
- Visual feedback appears smoothly without frame drops
- Auto-fill works correctly for empty cells
- Lock state persists through save/load cycles
- No interference with existing game mechanics
- Compatible with both mouse (editor) and touch (mobile) input

## External Dependencies

- Unity EventSystems package (existing)
- Newtonsoft.Json (existing for save/load)
- Unity Coroutines (existing)
- Unity UI components (existing)

## Gotchas & Common Pitfalls

1. **Coroutine Management**: Always store and stop hold coroutines to prevent memory leaks
2. **Touch vs Mouse**: Test both input methods; ensure OnPointerExit works correctly
3. **Game State Integration**: Respect pause state and board interactable settings
4. **Visual Z-Order**: Ensure lock overlay appears above other UI elements
5. **Performance**: Use object pooling for progress bar if creating many instances
6. **Platform Differences**: Mobile touch sensitivity may require hold duration tuning

## Confidence Assessment: 9/10

**High confidence** for one-pass implementation due to:
- ✅ Comprehensive existing pattern analysis with specific file references
- ✅ Proven external Unity UI patterns and documentation
- ✅ Clear sequential implementation tasks
- ✅ Thorough validation approach covering edge cases
- ✅ Detailed error handling and integration considerations

**Minor risks**: Unity UI platform quirks, hold timing refinement, visual feedback iteration needs.