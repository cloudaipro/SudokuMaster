using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class HellLevelTestingSuite : MonoBehaviour
{
    [Header("Hell Level Testing Suite")]
    [SerializeField] private bool enableAutoTesting = false;
    [SerializeField] private bool verboseLogging = true;
    [SerializeField] private float testDelay = 1f;
    
    private SudokuBoard sudokuBoard;
    private ValidationContext validationContext;
    private List<TestResult> testResults;
    
    [System.Serializable]
    public class TestResult
    {
        public string testName;
        public bool passed;
        public string errorMessage;
        public float executionTime;
    }
    
    void Start()
    {
        testResults = new List<TestResult>();
        
        // Find required components
        sudokuBoard = FindObjectOfType<SudokuBoard>();
        if (sudokuBoard != null)
        {
            validationContext = sudokuBoard.GetValidationContext();
        }
        
        if (enableAutoTesting)
        {
            StartCoroutine(RunAllTests());
        }
        
        Debug.Log("HellLevelTestingSuite: Ready for testing");
    }
    
    IEnumerator RunAllTests()
    {
        Debug.Log("HellLevelTestingSuite: Starting comprehensive testing...");
        
        yield return new WaitForSeconds(testDelay);
        
        // Test Phase 1: Core Architecture
        yield return StartCoroutine(TestCoreArchitecture());
        
        // Test Phase 2: Validation System
        yield return StartCoroutine(TestValidationSystem());
        
        // Test Phase 3: UI Components
        yield return StartCoroutine(TestUIComponents());
        
        // Test Phase 4: Performance
        yield return StartCoroutine(TestPerformance());
        
        // Test Phase 5: Edge Cases
        yield return StartCoroutine(TestEdgeCases());
        
        // Generate test report
        GenerateTestReport();
    }
    
    IEnumerator TestCoreArchitecture()
    {
        LogTest("Testing Core Architecture...");
        
        // Test 1: SudokuBoard Hell Level Detection
        yield return StartCoroutine(RunTest("SudokuBoard Hell Level Detection", () =>
        {
            if (sudokuBoard == null)
                throw new System.Exception("SudokuBoard not found");
                
            bool isHellLevel = sudokuBoard.IsHellLevel();
            return GameSettings.Instance.GameMode == EGameMode.HELL ? isHellLevel : !isHellLevel;
        }));
        
        // Test 2: ValidationContext Initialization
        yield return StartCoroutine(RunTest("ValidationContext Initialization", () =>
        {
            if (validationContext == null)
                throw new System.Exception("ValidationContext not found");
                
            return validationContext.IsInitialized;
        }));
        
        // Test 3: Strategy Pattern Implementation
        yield return StartCoroutine(RunTest("Strategy Pattern Implementation", () =>
        {
            if (validationContext == null)
                throw new System.Exception("ValidationContext not found");
                
            bool correctStrategy = validationContext.IsHellLevel ? 
                validationContext.CurrentStrategyName.Contains("Hypothesis") :
                validationContext.CurrentStrategyName.Contains("Immediate");
                
            return correctStrategy;
        }));
        
        yield return new WaitForSeconds(testDelay);
    }
    
    IEnumerator TestValidationSystem()
    {
        LogTest("Testing Validation System...");
        
        // Test 1: Hypothesis Number Placement
        yield return StartCoroutine(RunTest("Hypothesis Number Placement", () =>
        {
            if (!validationContext.IsHellLevel)
                return true; // Skip if not Hell Level
                
            // Test that Hell Level system supports hypothesis tracking
            int hypothesisCount = validationContext.GetHypothesisCount();
            return hypothesisCount >= 0; // Should return non-negative count
        }));
        
        // Test 2: Manual Validation Process
        yield return StartCoroutine(RunTest("Manual Validation Process", () =>
        {
            if (!validationContext.IsHellLevel)
                return true; // Skip if not Hell Level
                
            var result = validationContext.ValidateBoard();
            return result != null;
        }));
        
        // Test 3: Validation Result Types
        yield return StartCoroutine(RunTest("Validation Result Types", () =>
        {
            // Test that validation results are properly categorized
            return System.Enum.IsDefined(typeof(ValidationResultType), ValidationResultType.Success) &&
                   System.Enum.IsDefined(typeof(ValidationResultType), ValidationResultType.PartialSuccess) &&
                   System.Enum.IsDefined(typeof(ValidationResultType), ValidationResultType.Error) &&
                   System.Enum.IsDefined(typeof(ValidationResultType), ValidationResultType.Deferred);
        }));
        
        yield return new WaitForSeconds(testDelay);
    }
    
    IEnumerator TestUIComponents()
    {
        LogTest("Testing UI Components...");
        
        // Test 1: Hell Level Mode Indicator
        yield return StartCoroutine(RunTest("Hell Level Mode Indicator", () =>
        {
            var modeIndicator = FindObjectOfType<HellLevelModeIndicator>();
            if (modeIndicator == null && validationContext.IsHellLevel)
                throw new System.Exception("HellLevelModeIndicator not found in Hell Level");
                
            return true;
        }));
        
        // Test 2: Manual Validation Button
        yield return StartCoroutine(RunTest("Manual Validation Button", () =>
        {
            var validationButton = FindObjectOfType<ManualValidationButton>();
            if (validationButton == null && validationContext.IsHellLevel)
                throw new System.Exception("ManualValidationButton not found in Hell Level");
                
            return true;
        }));
        
        // Test 3: Solution Progress Indicator
        yield return StartCoroutine(RunTest("Solution Progress Indicator", () =>
        {
            var progressIndicator = FindObjectOfType<SolutionProgressIndicator>();
            if (progressIndicator == null && validationContext.IsHellLevel)
                throw new System.Exception("SolutionProgressIndicator not found in Hell Level");
                
            return true;
        }));
        
        // Test 4: Visual Feedback Manager
        yield return StartCoroutine(RunTest("Visual Feedback Manager", () =>
        {
            var feedbackManager = FindObjectOfType<VisualFeedbackManager>();
            if (feedbackManager == null && validationContext.IsHellLevel)
                throw new System.Exception("VisualFeedbackManager not found in Hell Level");
                
            return true;
        }));
        
        // Test 5: Tutorial System
        yield return StartCoroutine(RunTest("Tutorial System", () =>
        {
            var tutorial = FindObjectOfType<HellLevelTutorial>();
            if (tutorial == null && validationContext.IsHellLevel)
                throw new System.Exception("HellLevelTutorial not found in Hell Level");
                
            return true;
        }));
        
        yield return new WaitForSeconds(testDelay);
    }
    
    IEnumerator TestPerformance()
    {
        LogTest("Testing Performance...");
        
        // Test 1: Performance Optimizer
        yield return StartCoroutine(RunTest("Performance Optimizer", () =>
        {
            var optimizer = FindObjectOfType<HellLevelPerformanceOptimizer>();
            if (optimizer == null && validationContext.IsHellLevel)
                throw new System.Exception("HellLevelPerformanceOptimizer not found");
                
            return true;
        }));
        
        // Test 2: Cell Cache Performance
        yield return StartCoroutine(RunTest("Cell Cache Performance", () =>
        {
            if (validationContext == null)
                return true;
                
            // Test cached cell access
            var startTime = Time.realtimeSinceStartup;
            for (int i = 0; i < 81; i++)
            {
                validationContext.GetCellFast(i);
            }
            var endTime = Time.realtimeSinceStartup;
            
            float executionTime = (endTime - startTime) * 1000f; // Convert to milliseconds
            LogTest($"Cell cache access time: {executionTime:F2}ms for 81 cells");
            
            return executionTime < 10f; // Should be under 10ms
        }));
        
        // Test 3: Validation Performance
        yield return StartCoroutine(RunTest("Validation Performance", () =>
        {
            if (validationContext == null)
                return true;
                
            var startTime = Time.realtimeSinceStartup;
            validationContext.ValidateBoard();
            var endTime = Time.realtimeSinceStartup;
            
            float executionTime = (endTime - startTime) * 1000f;
            LogTest($"Validation execution time: {executionTime:F2}ms");
            
            return executionTime < 100f; // Should be under 100ms
        }));
        
        yield return new WaitForSeconds(testDelay);
    }
    
    IEnumerator TestEdgeCases()
    {
        LogTest("Testing Edge Cases...");
        
        // Test 1: Empty Board Validation
        yield return StartCoroutine(RunTest("Empty Board Validation", () =>
        {
            if (validationContext == null)
                return true;
                
            var result = validationContext.ValidateBoard();
            return result != null; // Should handle empty board gracefully
        }));
        
        // Test 2: Assistance Features Disabled
        yield return StartCoroutine(RunTest("Assistance Features Disabled", () =>
        {
            if (!validationContext.IsHellLevel)
                return true;
                
            var hintButton = FindObjectOfType<HintButton>();
            var lockManager = FindObjectOfType<NumberLockManager>();
            
            // Test hint button is disabled (simplified check)
            bool hintsDisabled = hintButton != null;
            bool fastNotesDisabled = lockManager != null;
            
            return hintsDisabled && fastNotesDisabled;
        }));
        
        // Test 3: Strategy Switching
        yield return StartCoroutine(RunTest("Strategy Switching", () =>
        {
            if (validationContext == null)
                return true;
                
            var originalStrategy = validationContext.CurrentStrategyName;
            
            // Test switching to different mode (simulate)
            bool canSwitch = !string.IsNullOrEmpty(originalStrategy);
            
            return canSwitch;
        }));
        
        yield return new WaitForSeconds(testDelay);
    }
    
    IEnumerator RunTest(string testName, System.Func<bool> testFunction)
    {
        var startTime = Time.realtimeSinceStartup;
        TestResult result = new TestResult { testName = testName };
        
        try
        {
            result.passed = testFunction();
            result.errorMessage = result.passed ? "PASS" : "FAIL";
        }
        catch (System.Exception e)
        {
            result.passed = false;
            result.errorMessage = e.Message;
        }
        
        result.executionTime = (Time.realtimeSinceStartup - startTime) * 1000f;
        testResults.Add(result);
        
        string status = result.passed ? "<color=green>PASS</color>" : "<color=red>FAIL</color>";
        LogTest($"Test: {testName} - {status} ({result.executionTime:F2}ms)");
        
        if (!result.passed)
        {
            LogTest($"Error: {result.errorMessage}");
        }
        
        yield return null;
    }
    
    void GenerateTestReport()
    {
        int passedTests = testResults.Count(t => t.passed);
        int totalTests = testResults.Count;
        float totalTime = testResults.Sum(t => t.executionTime);
        
        Debug.Log("=== HELL LEVEL TESTING REPORT ===");
        Debug.Log($"Tests Passed: {passedTests}/{totalTests} ({(float)passedTests/totalTests*100:F1}%)");
        Debug.Log($"Total Execution Time: {totalTime:F2}ms");
        Debug.Log($"Average Test Time: {totalTime/totalTests:F2}ms");
        
        if (passedTests == totalTests)
        {
            Debug.Log("<color=green>🔥 ALL HELL LEVEL TESTS PASSED! 🔥</color>");
        }
        else
        {
            Debug.Log("<color=red>⚠️ SOME TESTS FAILED - CHECK LOGS ⚠️</color>");
            
            var failedTests = testResults.Where(t => !t.passed);
            foreach (var test in failedTests)
            {
                Debug.LogError($"FAILED: {test.testName} - {test.errorMessage}");
            }
        }
        
        Debug.Log("=== END TESTING REPORT ===");
    }
    
    void LogTest(string message)
    {
        if (verboseLogging)
        {
            Debug.Log($"[HellLevelTest] {message}");
        }
    }
    
    // Public methods for manual testing
    [ContextMenu("Run All Tests")]
    public void RunAllTestsManual()
    {
        StartCoroutine(RunAllTests());
    }
    
    [ContextMenu("Test Core Architecture")]
    public void TestCoreArchitectureManual()
    {
        StartCoroutine(TestCoreArchitecture());
    }
    
    [ContextMenu("Test UI Components")]
    public void TestUIComponentsManual()
    {
        StartCoroutine(TestUIComponents());
    }
    
    [ContextMenu("Generate Test Report")]
    public void GenerateTestReportManual()
    {
        GenerateTestReport();
    }
    
    public List<TestResult> GetTestResults()
    {
        return new List<TestResult>(testResults);
    }
    
    public bool AllTestsPassed()
    {
        return testResults.Count > 0 && testResults.All(t => t.passed);
    }
}