using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class CellPlacement
{
    public int cellIndex;
    public int value;
    public int previousValue;
    
    public CellPlacement(int cellIndex, int value, int previousValue = 0)
    {
        this.cellIndex = cellIndex;
        this.value = value;
        this.previousValue = previousValue;
    }
}

public class HypothesisValidationStrategy : IValidationStrategy
{
    private SudokuBoard sudokuBoard;
    private List<GameObject> gridSquares;
    private List<CellPlacement> hypothesisPlacements;
    
    public HypothesisValidationStrategy(SudokuBoard board, List<GameObject> squares)
    {
        sudokuBoard = board;
        gridSquares = squares;
        hypothesisPlacements = new List<CellPlacement>();
        
        // Subscribe to clear number event to handle hypothesis cleanup
        GameEvents.OnClearNumber += OnClearNumber;
    }

    public ValidationResult ProcessNumberPlacement(int cellIndex, int value)
    {
        if (cellIndex < 0 || cellIndex >= gridSquares.Count)
        {
            return ValidationResult.Error("Invalid cell index", new int[] { cellIndex });
        }

        var cell = gridSquares[cellIndex].GetComponent<SudokuCell>();
        if (cell == null || cell.Has_default_value)
        {
            return ValidationResult.Error("Cannot modify default value cell", new int[] { cellIndex });
        }

        // Store the placement for later validation without immediate feedback
        var existingPlacement = hypothesisPlacements.FirstOrDefault(p => p.cellIndex == cellIndex);
        if (existingPlacement != null)
        {
            existingPlacement.value = value;
        }
        else
        {
            hypothesisPlacements.Add(new CellPlacement(cellIndex, value, cell.Number));
        }

        return ValidationResult.Deferred("Number placed - validate when ready");
    }

    public ValidationResult ValidateCompleteBoard()
    {
        var allCells = gridSquares.Select(x => x.GetComponent<SudokuCell>()).ToList();
        var errorCells = new List<int>();
        var errorMessages = new List<string>();
        int correctCount = 0;
        int filledCells = 0;

        // Check each cell for correctness
        for (int i = 0; i < allCells.Count; i++)
        {
            var cell = allCells[i];
            if (cell.Number != 0)
            {
                filledCells++;
                if (cell.Number == cell.Correct_number)
                {
                    correctCount++;
                }
                else
                {
                    errorCells.Add(i);
                }
            }
        }

        // Additional validation: Check for duplicates in rows, columns, and blocks
        var duplicateErrors = ValidateSudokuRules(allCells);
        errorCells.AddRange(duplicateErrors);
        errorCells = errorCells.Distinct().ToList();

        // Update Has_Wrong_value property on cells for visual feedback
        UpdateCellErrorStates(allCells, errorCells);

        float completionPercentage = (correctCount / 81f) * 100f;
        
        // Generate graded feedback message
        string message = GenerateGradedFeedback(correctCount, filledCells, errorCells.Count, allCells);
        
        if (errorCells.Count == 0 && correctCount == 81)
        {
            return ValidationResult.Success("🎉 Perfect! Hell Level completed flawlessly!");
        }
        else if (errorCells.Count == 0 && filledCells < 81)
        {
            return ValidationResult.Partial(completionPercentage, message);
        }
        else
        {
            return ValidationResult.Error(message, errorCells.ToArray(), errorCells.Count);
        }
    }

    private List<int> ValidateSudokuRules(List<SudokuCell> allCells)
    {
        var errorCells = new List<int>();
        
        // Check rows for duplicates
        for (int row = 0; row < 9; row++)
        {
            var rowCells = allCells.Where(c => c.Row_index == row && c.Number != 0).ToList();
            var duplicates = rowCells.GroupBy(c => c.Number).Where(g => g.Count() > 1);
            foreach (var duplicate in duplicates)
            {
                errorCells.AddRange(duplicate.Where(c => c.IsHypothesisNumber()).Select(c => c.Cell_index));
            }
        }
        
        // Check columns for duplicates
        for (int col = 0; col < 9; col++)
        {
            var colCells = allCells.Where(c => c.Column_index == col && c.Number != 0).ToList();
            var duplicates = colCells.GroupBy(c => c.Number).Where(g => g.Count() > 1);
            foreach (var duplicate in duplicates)
            {
                errorCells.AddRange(duplicate.Where(c => c.IsHypothesisNumber()).Select(c => c.Cell_index));
            }
        }
        
        // Check blocks for duplicates
        for (int block = 0; block < 9; block++)
        {
            var blockCells = allCells.Where(c => c.Block_index == block && c.Number != 0).ToList();
            var duplicates = blockCells.GroupBy(c => c.Number).Where(g => g.Count() > 1);
            foreach (var duplicate in duplicates)
            {
                errorCells.AddRange(duplicate.Where(c => c.IsHypothesisNumber()).Select(c => c.Cell_index));
            }
        }
        
        return errorCells;
    }

    private string GenerateGradedFeedback(int correctCount, int filledCells, int errorCount, List<SudokuCell> allCells)
    {
        float correctPercentage = (correctCount / (float)filledCells) * 100f;
        float completionPercentage = (filledCells / 81f) * 100f;
        
        if (correctCount == 81)
        {
            return "🏆 Congratulations! Hell Level mastered with perfect solution!";
        }
        else if (errorCount == 0 && filledCells < 81)
        {
            return $"✅ Excellent progress! {completionPercentage:F1}% complete with no errors detected.";
        }
        else if (correctPercentage >= 90f)
        {
            return $"⚡ Strong work! {correctPercentage:F1}% accuracy - {errorCount} cells need review.";
        }
        else if (correctPercentage >= 70f)
        {
            return $"💪 Good progress! {correctPercentage:F1}% accuracy - {errorCount} conflicts to resolve.";
        }
        else if (correctPercentage >= 50f)
        {
            return $"🤔 Partially correct ({correctPercentage:F1}%) - {errorCount} areas need attention.";
        }
        else
        {
            return $"🔄 Multiple conflicts detected - {errorCount} cells require revision. Keep testing theories!";
        }
    }

    public void OnNumberPlaced(int cellIndex, int value)
    {
        // In Hell Level, we don't trigger immediate events like lives system or audio
        // The number is simply placed and tracked for later validation
        var cell = gridSquares[cellIndex].GetComponent<SudokuCell>();
        
        // Don't clear notes or trigger completion checks in Hell Level
        // Don't update lives system
        // Don't play audio feedback
        
        // BUT we do need to notify ManualValidationButton that changes occurred
        GameEvents.didSetNumberMethod(cellIndex);
        
        // Just visually mark the cell as having a hypothesis value
        cell.Has_Wrong_value = false; // Reset wrong value flag for hypothesis mode
    }

    public bool ShouldProvideImmediateFeedback()
    {
        return false;
    }

    public bool ShouldUpdateLivesSystem()
    {
        return false;
    }

    public bool ShouldPlayAudio()
    {
        return false;
    }

    public void Reset()
    {
        hypothesisPlacements.Clear();
        
        // Reset all cells that were marked as hypothesis
        var allCells = gridSquares.Select(x => x.GetComponent<SudokuCell>());
        foreach (var cell in allCells)
        {
            if (!cell.Has_default_value)
            {
                cell.Has_Wrong_value = false;
            }
        }
    }

    // Additional Hell Level specific methods
    public List<CellPlacement> GetHypothesisPlacements()
    {
        return new List<CellPlacement>(hypothesisPlacements);
    }

    public void ClearHypothesis()
    {
        Reset();
    }

    public int GetHypothesisCount()
    {
        return hypothesisPlacements.Count;
    }

    public void Dispose()
    {
        // Unsubscribe from events to prevent memory leaks
        GameEvents.OnClearNumber -= OnClearNumber;
    }

    private void OnClearNumber()
    {
        // Find which cells have been cleared and remove them from hypothesis placements
        var cellsToRemove = new List<CellPlacement>();
        
        foreach (var placement in hypothesisPlacements)
        {
            if (placement.cellIndex >= 0 && placement.cellIndex < gridSquares.Count)
            {
                var cell = gridSquares[placement.cellIndex].GetComponent<SudokuCell>();
                if (cell != null && cell.Number == 0)
                {
                    cellsToRemove.Add(placement);
                }
            }
        }
        
        // Remove cleared cells from hypothesis tracking
        foreach (var placement in cellsToRemove)
        {
            hypothesisPlacements.Remove(placement);
        }
        
        if (cellsToRemove.Count > 0)
        {
            Debug.Log($"HypothesisValidationStrategy: Removed {cellsToRemove.Count} cleared cells from hypothesis tracking");
        }
    }

    private void UpdateCellErrorStates(List<SudokuCell> allCells, List<int> errorCells)
    {
        // First, process all hypothesis numbers and convert them to regular numbers
        for (int i = 0; i < allCells.Count; i++)
        {
            var cell = allCells[i];
            if (!cell.Has_default_value)
            {
                // Clear hypothesis state for all cells after validation
                if (cell.IsHypothesisNumber())
                {
                    cell.SetAsHypothesis(false); // This calls UpdateSquareColor internally
                }
                
                // Reset error state initially
                cell.Has_Wrong_value = false;
            }
        }

        // Then set error state for cells with wrong numbers
        foreach (int errorIndex in errorCells)
        {
            if (errorIndex >= 0 && errorIndex < allCells.Count)
            {
                var cell = allCells[errorIndex];
                if (!cell.Has_default_value)
                {
                    cell.Has_Wrong_value = (cell.Number != cell.Correct_number);
                    // Force cell visual update
                    cell.UpdateSquareColor();
                }
            }
        }
        
        Debug.Log($"UpdateCellErrorStates: Cleared hypothesis state for all cells, set {errorCells.Count} cells to error state");
    }
}