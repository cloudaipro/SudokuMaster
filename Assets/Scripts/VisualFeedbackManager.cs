using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class VisualFeedbackManager : MonoBehaviour
{
    [Header("Visual Feedback Manager")]
    [SerializeField] private bool enableErrorHighlighting = true;
    [SerializeField] private bool enableConfirmationEffects = true;
    [SerializeField] private float errorBlinkDuration = 2f;
    [SerializeField] private float confirmationDuration = 1f;
    
    [Header("Error Highlighting Colors")]
    [SerializeField] private Color errorHighlightColor = new Color(1f, 0.2f, 0.1f, 0.9f); // Hell fire red
    [SerializeField] private Color conflictCellColor = new Color(1f, 0.4f, 0.05f, 0.7f); // Hell orange for conflicts
    [SerializeField] private Color validationBorderColor = new Color(0.2f, 0.9f, 0.2f, 1f); // Bright success green
    
    [Header("Animation Settings")]
    [SerializeField] private AnimationCurve errorBlinkCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [SerializeField] private AnimationCurve confirmationScaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 1.2f);
    
    private SudokuBoard sudokuBoard;
    private ValidationContext validationContext;
    private List<GameObject> gridSquares;
    private Dictionary<int, Coroutine> activeHighlights;
    private ManualValidationButton validationButton;
    
    void Start()
    {
        // Find required components
        sudokuBoard = FindObjectOfType<SudokuBoard>();
        validationButton = FindObjectOfType<ManualValidationButton>();
        
        if (sudokuBoard == null)
        {
            Debug.LogError("VisualFeedbackManager: SudokuBoard not found!");
            gameObject.SetActive(false);
            return;
        }
        
        // Initialize feedback system
        InitializeFeedbackSystem();
    }
    
    void InitializeFeedbackSystem()
    {
        activeHighlights = new Dictionary<int, Coroutine>();
        
        // Get ValidationContext and subscribe to events
        validationContext = sudokuBoard.GetValidationContext();
        if (validationContext != null)
        {
            validationContext.OnValidationResult += OnValidationResult;
            validationContext.OnStrategyChanged += OnStrategyChanged;
        }
        
        // Subscribe to game events for immediate feedback
        //alex GameEvents.OnDidSetNumber += OnNumberPlaced;
        //alex GameEvents.OnClearNumber += OnNumberCleared;
        
        Debug.Log("VisualFeedbackManager: Initialized successfully");
    }
    
    void OnStrategyChanged(bool isHellLevel)
    {
        // Clear any active highlights when switching strategies
        ClearAllHighlights();
        
        // Update visual feedback behavior based on level
        enableErrorHighlighting = isHellLevel; // Always enable for both modes
        enableConfirmationEffects = false; // alex !isHellLevel; // Disable confirmation effects in Hell Level
        
        Debug.Log($"VisualFeedbackManager: Updated for {(isHellLevel ? "Hell" : "Normal")} level");
    }
    
    void OnValidationResult(ValidationResult result)
    {
        if (result == null || !enableErrorHighlighting) return;
        
        // Clear previous highlights
        ClearAllHighlights();
        
        // Handle validation result based on type
        switch (result.Type)
        {
            case ValidationResultType.Success:
                ShowSuccessEffects();
                break;
                
            case ValidationResultType.PartialSuccess:
            case ValidationResultType.Error:
                if (result.ErrorCells != null && result.ErrorCells.Length > 0)
                {
                    HighlightErrorCells(result.ErrorCells);
                }
                break;
        }
    }
    
    void OnNumberPlaced(int cellIndex)
    {
        // alex disable ShowConfirmationEffect
        return;
        // if (!enableConfirmationEffects || validationContext == null || validationContext.IsHellLevel)
        //     return;
        
        // // Show confirmation effect for correct placement in normal levels
        // var cell = GetCellAtIndex(cellIndex);
        // if (cell != null && cell.Number == cell.Correct_number)
        // {
        //     StartCoroutine(ShowConfirmationEffect(cellIndex));
        // }
    }
    
    void OnNumberCleared()
    {
        // Clear any highlights when number is cleared
        ClearRecentHighlights();
    }
    
    void HighlightErrorCells(int[] errorCellIndices)
    {
        if (errorCellIndices == null || errorCellIndices.Length == 0)
            return;

        sudokuBoard.Play_Audio("Wrong");

        foreach (int cellIndex in errorCellIndices)
        {
            //GameEvents.OnWrongNumberMethod();
            //if (activeHighlights.ContainsKey(cellIndex))
            //{
            //    StopCoroutine(activeHighlights[cellIndex]);
            //}
            
            //activeHighlights[cellIndex] = StartCoroutine(HighlightErrorCell(cellIndex));

        }
        
        Debug.Log($"VisualFeedbackManager: Highlighted {errorCellIndices.Length} error cells");
    }
    
    IEnumerator HighlightErrorCell(int cellIndex)
    {
        var cell = GetCellAtIndex(cellIndex);
        if (cell == null) yield break;
        
        Color originalColor = cell.colors.disabledColor;
        float elapsed = 0f;
        
        while (elapsed < errorBlinkDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / errorBlinkDuration;
            float blinkIntensity = errorBlinkCurve.Evaluate(normalizedTime % 1f);
            
            // Blend between original color and error highlight color
            Color blendedColor = Color.Lerp(originalColor, errorHighlightColor, blinkIntensity);
            
            // Update cell color
            var colors = cell.colors;
            colors.disabledColor = blendedColor;
            cell.colors = colors;
            
            yield return null;
        }
        
        // Restore original color
        var finalColors = cell.colors;
        finalColors.disabledColor = originalColor;
        cell.colors = finalColors;
        
        // Remove from active highlights
        if (activeHighlights.ContainsKey(cellIndex))
        {
            activeHighlights.Remove(cellIndex);
        }
    }
    
    IEnumerator ShowConfirmationEffect(int cellIndex)
    {
        var cell = GetCellAtIndex(cellIndex);
        if (cell == null) yield break;
        
        GameObject cellObject = cell.gameObject;
        Vector3 originalScale = cellObject.transform.localScale;
        float elapsed = 0f;
        
        while (elapsed < confirmationDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / confirmationDuration;
            float scale = confirmationScaleCurve.Evaluate(normalizedTime);
            
            cellObject.transform.localScale = originalScale * scale;
            
            yield return null;
        }
        
        // Restore original scale
        cellObject.transform.localScale = originalScale;
    }
    
    void ShowSuccessEffects()
    {
        // alex disable ShowBoardSuccessEffect
        sudokuBoard.Play_Audio("Correct");
        return;
        // if (!enableConfirmationEffects) return;
        
        // // Show brief success effect for completed puzzle
        // StartCoroutine(ShowBoardSuccessEffect());
    }
    
    IEnumerator ShowBoardSuccessEffect()
    {
        var allCells = GetAllCells();
        
        // Brief flash effect for all correct cells
        foreach (var cell in allCells)
        {
            if (cell != null && cell.Number == cell.Correct_number)
            {
                StartCoroutine(ShowBriefSuccessFlash(cell));
            }
        }
        
        yield return new WaitForSeconds(0.5f);
        
        Debug.Log("VisualFeedbackManager: Board success effect completed");
    }
    
    IEnumerator ShowBriefSuccessFlash(SudokuCell cell)
    {
        if (cell == null) yield break;
        
        Color originalColor = cell.colors.disabledColor;
        Color successColor = validationBorderColor;
        
        // Quick flash to success color and back
        var colors = cell.colors;
        colors.disabledColor = successColor;
        cell.colors = colors;
        
        yield return new WaitForSeconds(0.2f);
        
        colors.disabledColor = originalColor;
        cell.colors = colors;
    }
    
    void ClearAllHighlights()
    {
        foreach (var kvp in activeHighlights)
        {
            if (kvp.Value != null)
            {
                StopCoroutine(kvp.Value);
            }
        }
        
        activeHighlights.Clear();
        
        // Reset all cell colors to their natural state
        var allCells = GetAllCells();
        foreach (var cell in allCells)
        {
            if (cell != null)
            {
                cell.UpdateSquareColor(); // Restore natural colors
            }
        }
    }
    
    void ClearRecentHighlights()
    {
        // Clear highlights that have been active for less than 1 second
        var recentHighlights = activeHighlights.Where(kvp => kvp.Value != null).Take(5).ToList();
        
        foreach (var kvp in recentHighlights)
        {
            StopCoroutine(kvp.Value);
            activeHighlights.Remove(kvp.Key);
        }
    }
    
    SudokuCell GetCellAtIndex(int cellIndex)
    {
        if (gridSquares == null)
        {
            gridSquares = GetGridSquares();
        }
        
        if (gridSquares != null && cellIndex >= 0 && cellIndex < gridSquares.Count)
        {
            return gridSquares[cellIndex].GetComponent<SudokuCell>();
        }
        
        return null;
    }
    
    List<SudokuCell> GetAllCells()
    {
        if (gridSquares == null)
        {
            gridSquares = GetGridSquares();
        }
        
        if (gridSquares != null)
        {
            return gridSquares.Select(square => square.GetComponent<SudokuCell>())
                             .Where(cell => cell != null)
                             .ToList();
        }
        
        return new List<SudokuCell>();
    }
    
    List<GameObject> GetGridSquares()
    {
        // Find all SudokuCell components in the scene
        var sudokuCells = FindObjectsOfType<SudokuCell>();
        return sudokuCells.OrderBy(cell => cell.Cell_index)
                         .Select(cell => cell.gameObject)
                         .ToList();
    }
    
    // Public methods for external control
    // alex mark because they are not used
    // public void HighlightCell(int cellIndex, Color highlightColor, float duration = 2f)
    // {
    //     if (activeHighlights.ContainsKey(cellIndex))
    //     {
    //         StopCoroutine(activeHighlights[cellIndex]);
    //     }
        
    //     activeHighlights[cellIndex] = StartCoroutine(HighlightCellWithColor(cellIndex, highlightColor, duration));
    // }
    
    // IEnumerator HighlightCellWithColor(int cellIndex, Color highlightColor, float duration)
    // {
    //     var cell = GetCellAtIndex(cellIndex);
    //     if (cell == null) yield break;
        
    //     Color originalColor = cell.colors.disabledColor;
        
    //     // Set highlight color
    //     var colors = cell.colors;
    //     colors.disabledColor = highlightColor;
    //     cell.colors = colors;
        
    //     yield return new WaitForSeconds(duration);
        
    //     // Restore original color
    //     colors.disabledColor = originalColor;
    //     cell.colors = colors;
        
    //     // Remove from active highlights
    //     if (activeHighlights.ContainsKey(cellIndex))
    //     {
    //         activeHighlights.Remove(cellIndex);
    //     }
    // }
    
    // public void ClearHighlight(int cellIndex)
    // {
    //     if (activeHighlights.ContainsKey(cellIndex))
    //     {
    //         StopCoroutine(activeHighlights[cellIndex]);
    //         activeHighlights.Remove(cellIndex);
            
    //         var cell = GetCellAtIndex(cellIndex);
    //         if (cell != null)
    //         {
    //             cell.UpdateSquareColor();
    //         }
    //     }
    // }
    
    // public void SetErrorHighlighting(bool enabled)
    // {
    //     enableErrorHighlighting = enabled;
    //     if (!enabled)
    //     {
    //         ClearAllHighlights();
    //     }
    // }
    
    // public void SetConfirmationEffects(bool enabled)
    // {
    //     enableConfirmationEffects = enabled;
    // }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (validationContext != null)
        {
            validationContext.OnValidationResult -= OnValidationResult;
            validationContext.OnStrategyChanged -= OnStrategyChanged;
        }
        
        // alex GameEvents.OnDidSetNumber -= OnNumberPlaced;
        // alex GameEvents.OnClearNumber -= OnNumberCleared;
        
        // Stop all active coroutines
        ClearAllHighlights();
    }
    
    #if UNITY_EDITOR
    [Header("Debug Info (Editor Only)")]
    [SerializeField] private bool debugShowErrorHighlighting;
    [SerializeField] private bool debugShowConfirmationEffects;
    [SerializeField] private int debugActiveHighlights;
    
    void Update()
    {
        if (Application.isEditor)
        {
            debugShowErrorHighlighting = enableErrorHighlighting;
            debugShowConfirmationEffects = enableConfirmationEffects;
            debugActiveHighlights = activeHighlights?.Count ?? 0;
        }
    }
    #endif
}