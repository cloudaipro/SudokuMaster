using UnityEngine;
using System.Collections.Generic;

public class ValidationContext : MonoBehaviour
{
    [Header("Validation Context")]
    [SerializeField] private EGameMode currentGameMode = EGameMode.NOT_SET;
    [SerializeField] private bool isInitialized = false;
    
    private IValidationStrategy currentStrategy;
    private SudokuBoard sudokuBoard;
    private List<GameObject> gridSquares;
    private Dictionary<int, SudokuCell> cellCache; // Performance optimization
    private bool isDirty = true;
    
    // Strategy instances (cached for performance)
    private ImmediateValidationStrategy immediateStrategy;
    private HypothesisValidationStrategy hypothesisStrategy;
    
    // Events for UI updates
    public System.Action<ValidationResult> OnValidationResult;
    public System.Action<bool> OnStrategyChanged; // true = Hell Level, false = Normal levels
    
    void Awake()
    {
        // Find required components
        sudokuBoard = FindObjectOfType<SudokuBoard>();
        if (sudokuBoard == null)
        {
            Debug.LogError("ValidationContext: SudokuBoard not found!");
            return;
        }
    }

    public void Initialize(EGameMode gameMode, List<GameObject> squares)
    {
        currentGameMode = gameMode;
        gridSquares = squares;
        
        if (gridSquares == null || gridSquares.Count == 0)
        {
            Debug.LogError("ValidationContext: Invalid grid squares provided!");
            return;
        }
        
        // Initialize cell cache for performance
        InitializeCellCache();
        
        // Create strategy instances
        immediateStrategy = new ImmediateValidationStrategy(sudokuBoard, gridSquares);
        hypothesisStrategy = new HypothesisValidationStrategy(sudokuBoard, gridSquares);
        
        isInitialized = true;
        
        // Set current strategy based on game mode
        SwitchStrategy(gameMode);
        
        Debug.Log($"ValidationContext initialized for {gameMode} mode");
    }
    
    public void SwitchStrategy(EGameMode gameMode)
    {
        if (!isInitialized && gameMode != EGameMode.HELL)
        {
            // For non-Hell levels, initialize with basic immediate strategy
            currentGameMode = gameMode;
            currentStrategy = new ImmediateValidationStrategy(null, new List<GameObject>());
            Debug.Log($"ValidationContext: Basic initialization for {gameMode} mode");
            return;
        }
        
        var previousStrategy = currentStrategy;
        currentGameMode = gameMode;
        
        // Reset previous strategy if switching
        previousStrategy?.Reset();
        
        // Select appropriate strategy
        currentStrategy = gameMode == EGameMode.HELL 
            ? (IValidationStrategy)hypothesisStrategy 
            : (IValidationStrategy)immediateStrategy;
        
        // Notify UI about strategy change
        OnStrategyChanged?.Invoke(gameMode == EGameMode.HELL);
        
        Debug.Log($"Switched to {(gameMode == EGameMode.HELL ? "Hypothesis" : "Immediate")} validation strategy");
    }
    
    public ValidationResult ProcessMove(int cellIndex, int value)
    {
        if (!isInitialized || currentStrategy == null)
        {
            Debug.LogWarning("ValidationContext: Not properly initialized!");
            return ValidationResult.Error("Validation system not ready");
        }
        
        // Process the move through current strategy
        var result = currentStrategy.ProcessNumberPlacement(cellIndex, value);

        // Handle post-placement logic
        // alex if (result.Type != ValidationResultType.Error)
        //{
        //    currentStrategy.OnNumberPlaced(cellIndex, value);
        //}

        currentStrategy.OnNumberPlaced(cellIndex, value);

        // Only notify listeners for non-deferred results (Hell Level uses deferred validation)
        if (result.Type != ValidationResultType.Deferred)
        {
            OnValidationResult?.Invoke(result);
        }
        
        return result;
    }
    
    public ValidationResult ValidateBoard()
    {
        if (!isInitialized || currentStrategy == null)
        {
            Debug.LogWarning("ValidationContext: Not properly initialized!");
            return ValidationResult.Error("Validation system not ready");
        }
        
        var result = currentStrategy.ValidateCompleteBoard();
        
        // Notify listeners of validation result
        OnValidationResult?.Invoke(result);
        
        return result;
    }
    
    // Strategy-specific properties
    public bool IsHellLevel => currentGameMode == EGameMode.HELL;
    public bool ShouldProvideImmediateFeedback => currentStrategy?.ShouldProvideImmediateFeedback() ?? false;
    public bool ShouldUpdateLivesSystem => currentStrategy?.ShouldUpdateLivesSystem() ?? false;
    public bool ShouldPlayAudio => currentStrategy?.ShouldPlayAudio() ?? false;
    
    // Hell Level specific methods
    public List<CellPlacement> GetHypothesisPlacements()
    {
        if (IsHellLevel && hypothesisStrategy != null)
        {
            return hypothesisStrategy.GetHypothesisPlacements();
        }
        return new List<CellPlacement>();
    }
    
    public int GetHypothesisCount()
    {
        if (IsHellLevel && hypothesisStrategy != null)
        {
            return hypothesisStrategy.GetHypothesisCount();
        }
        return 0;
    }
    
    public void ClearHypothesis()
    {
        if (IsHellLevel && hypothesisStrategy != null)
        {
            hypothesisStrategy.ClearHypothesis();
        }
    }
    
    // Performance optimization methods
    private void InitializeCellCache()
    {
        cellCache = new Dictionary<int, SudokuCell>();
        
        if (gridSquares != null)
        {
            foreach (var square in gridSquares)
            {
                var cell = square.GetComponent<SudokuCell>();
                if (cell != null)
                {
                    cellCache[cell.Cell_index] = cell;
                }
            }
        }
        
        isDirty = false;
    }
    
    public SudokuCell GetCellFast(int cellIndex)
    {
        if (isDirty)
        {
            InitializeCellCache();
        }
        
        cellCache.TryGetValue(cellIndex, out SudokuCell cell);
        return cell;
    }
    
    public void MarkCacheDirty()
    {
        isDirty = true;
    }
    
    // Batch validation for better performance
    public void BatchProcessCellChanges(List<int> cellIndices)
    {
        if (currentStrategy == null || cellIndices == null || cellIndices.Count == 0)
            return;
        
        foreach (int cellIndex in cellIndices)
        {
            var cell = GetCellFast(cellIndex);
            if (cell != null)
            {
                currentStrategy.OnNumberPlaced(cellIndex, cell.Number);
            }
        }
    }
    
    // Public accessors for debugging
    public EGameMode CurrentGameMode => currentGameMode;
    public bool IsInitialized => isInitialized;
    public string CurrentStrategyName => currentStrategy?.GetType().Name ?? "None";
    public int CachedCellCount => cellCache?.Count ?? 0;
    
    // Reset validation context (useful for new games)
    public void ResetValidation()
    {
        currentStrategy?.Reset();
        Debug.Log("ValidationContext reset");
    }
    
    void OnDisable()
    {
        // Clean up when component is disabled
        currentStrategy?.Reset();
    }
    
    #if UNITY_EDITOR
    [Header("Debug Info (Editor Only)")]
    [SerializeField] private string debugCurrentStrategy;
    [SerializeField] private int debugHypothesisCount;
    
    void Update()
    {
        if (Application.isEditor && isInitialized)
        {
            debugCurrentStrategy = CurrentStrategyName;
            debugHypothesisCount = GetHypothesisCount();
        }
    }
    #endif
}