using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ImmediateValidationStrategy : IValidationStrategy
{
    private SudokuBoard sudokuBoard;
    private List<GameObject> gridSquares;
    
    public ImmediateValidationStrategy(SudokuBoard board, List<GameObject> squares)
    {
        sudokuBoard = board;
        gridSquares = squares ?? new List<GameObject>();
    }

    public ValidationResult ProcessNumberPlacement(int cellIndex, int value)
    {
        // If not fully initialized, use simplified validation
        if (gridSquares == null || gridSquares.Count == 0)
        {
            // Fallback: assume immediate validation is handled by legacy code
            return ValidationResult.Success("Legacy validation active");
        }
        
        if (cellIndex < 0 || cellIndex >= gridSquares.Count)
        {
            return ValidationResult.Error("Invalid cell index", new int[] { cellIndex });
        }

        var cell = gridSquares[cellIndex].GetComponent<SudokuCell>();
        if (cell == null || cell.Has_default_value)
        {
            return ValidationResult.Error("Cannot modify default value cell", new int[] { cellIndex });
        }

        if (value == cell.Correct_number)
        {
            return ValidationResult.Success("Correct number!");
        }
        else
        {
            return ValidationResult.Error("Wrong number", new int[] { cellIndex }, 1);
        }
    }

    public ValidationResult ValidateCompleteBoard()
    {
        var allCells = gridSquares.Select(x => x.GetComponent<SudokuCell>()).ToList();
        var errorCells = new List<int>();
        int correctCount = 0;

        for (int i = 0; i < allCells.Count; i++)
        {
            var cell = allCells[i];
            if (cell.Number != 0)
            {
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

        float completionPercentage = (correctCount / 81f) * 100f;
        
        if (errorCells.Count == 0 && correctCount == 81)
        {
            return ValidationResult.Success("Perfect! Board completed correctly!");
        }
        else if (errorCells.Count > 0)
        {
            return ValidationResult.Error($"Board has {errorCells.Count} errors", errorCells.ToArray(), errorCells.Count);
        }
        else
        {
            return ValidationResult.Partial(completionPercentage, $"Board is {completionPercentage:F1}% complete");
        }
    }

    public void OnNumberPlaced(int cellIndex, int value)
    {
        // If not fully initialized, skip post-placement logic (handled by legacy code)
        if (gridSquares == null || gridSquares.Count == 0 || cellIndex >= gridSquares.Count)
        {
            return;
        }
        
        var cell = gridSquares[cellIndex].GetComponent<SudokuCell>();
        if (cell == null) return;
        
        if (value == cell.Correct_number)
        {
            cell.Has_Wrong_value = false;
            cell.ClearupAllNotes();
            GameEvents.didSetNumberMethod(cellIndex);
            GameEvents.OnNumberUsedMethod(value);
            GameEvents.CheckBoardCompletedMethod();
        }
        else
        {
            cell.Has_Wrong_value = true;
            GameEvents.OnWrongNumberMethod();
        }
    }

    public bool ShouldProvideImmediateFeedback()
    {
        return true;
    }

    public bool ShouldUpdateLivesSystem()
    {
        return true;
    }

    public bool ShouldPlayAudio()
    {
        return true;
    }

    public void Reset()
    {
        // Clear any validation state if needed
        // For immediate validation, there's typically no persistent state to clear
    }
}