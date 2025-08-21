using System;

public enum ValidationResultType
{
    Success,
    Error,
    Deferred,
    PartialSuccess
}

[Serializable]
public class ValidationResult
{
    public ValidationResultType Type { get; set; }
    public string Message { get; set; }
    public int[] ErrorCells { get; set; }
    public float CompletionPercentage { get; set; }
    public int ErrorCount { get; set; }

    public ValidationResult(ValidationResultType type, string message = "", int[] errorCells = null, float completionPercentage = 0f, int errorCount = 0)
    {
        Type = type;
        Message = message ?? "";
        ErrorCells = errorCells ?? new int[0];
        CompletionPercentage = completionPercentage;
        ErrorCount = errorCount;
    }

    public static ValidationResult Success(string message = "Correct!")
    {
        return new ValidationResult(ValidationResultType.Success, message, null, 100f, 0);
    }

    public static ValidationResult Error(string message, int[] errorCells = null, int errorCount = 1)
    {
        return new ValidationResult(ValidationResultType.Error, message, errorCells, 0f, errorCount);
    }

    public static ValidationResult Deferred(string message = "")
    {
        return new ValidationResult(ValidationResultType.Deferred, message);
    }

    public static ValidationResult Partial(float completionPercentage, string message, int errorCount = 0, int[] errorCells = null)
    {
        return new ValidationResult(ValidationResultType.PartialSuccess, message, errorCells, completionPercentage, errorCount);
    }
}

public interface IValidationStrategy
{
    ValidationResult ProcessNumberPlacement(int cellIndex, int value);
    ValidationResult ValidateCompleteBoard();
    void OnNumberPlaced(int cellIndex, int value);
    bool ShouldProvideImmediateFeedback();
    bool ShouldUpdateLivesSystem();
    bool ShouldPlayAudio();
    void Reset();
}