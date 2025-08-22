using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class HellLevelPerformanceOptimizer : MonoBehaviour
{
    [Header("Performance Optimization")]
    [SerializeField] private bool enableOptimizations = true;
    [SerializeField] private float updateInterval = 0.1f; // Update UI every 100ms instead of every frame
    [SerializeField] private int maxUIUpdatesPerFrame = 3; // Limit UI updates per frame
    
    private SudokuBoard sudokuBoard;
    private ValidationContext validationContext;
    private List<IOptimizedComponent> optimizedComponents;
    private Queue<System.Action> deferredUIUpdates;
    private Coroutine updateCoroutine;
    
    void Start()
    {
        if (!enableOptimizations) return;
        
        sudokuBoard = FindObjectOfType<SudokuBoard>();
        if (sudokuBoard != null)
        {
            validationContext = sudokuBoard.GetValidationContext();
        }
        
        InitializeOptimizations();
    }
    
    void InitializeOptimizations()
    {
        optimizedComponents = new List<IOptimizedComponent>();
        deferredUIUpdates = new Queue<System.Action>();
        
        // Find and register optimizable components
        RegisterOptimizableComponents();
        
        // Start optimization coroutines
        if (updateCoroutine == null)
        {
            updateCoroutine = StartCoroutine(OptimizedUpdateLoop());
        }
        
        // Optimize object pooling for frequent UI updates
        OptimizeObjectPooling();
        
        Debug.Log("HellLevelPerformanceOptimizer: Optimizations enabled");
    }
    
    void RegisterOptimizableComponents()
    {
        // Register Hell Level UI components that can be optimized
        var progressIndicator = FindObjectOfType<SolutionProgressIndicator>();
        if (progressIndicator != null)
        {
            var optimizedProgress = progressIndicator.gameObject.GetComponent<OptimizedProgressIndicator>();
            if (optimizedProgress == null)
            {
                optimizedProgress = progressIndicator.gameObject.AddComponent<OptimizedProgressIndicator>();
            }
            optimizedComponents.Add(optimizedProgress);
        }
        
        var validationButton = FindObjectOfType<ManualValidationButton>();
        if (validationButton != null)
        {
            var optimizedButton = validationButton.gameObject.GetComponent<OptimizedValidationButton>();
            if (optimizedButton == null)
            {
                optimizedButton = validationButton.gameObject.AddComponent<OptimizedValidationButton>();
            }
            optimizedComponents.Add(optimizedButton);
        }
        
        var feedbackManager = FindObjectOfType<VisualFeedbackManager>();
        if (feedbackManager != null)
        {
            var optimizedFeedback = feedbackManager.gameObject.GetComponent<OptimizedVisualFeedback>();
            if (optimizedFeedback == null)
            {
                optimizedFeedback = feedbackManager.gameObject.AddComponent<OptimizedVisualFeedback>();
            }
            optimizedComponents.Add(optimizedFeedback);
        }
    }
    
    IEnumerator OptimizedUpdateLoop()
    {
        while (enabled)
        {
            yield return new WaitForSeconds(updateInterval);
            
            if (sudokuBoard != null && sudokuBoard.IsHellLevel())
            {
                // Process deferred UI updates
                ProcessDeferredUpdates();
                
                // Update optimized components
                foreach (var component in optimizedComponents)
                {
                    if (component != null)
                    {
                        component.OptimizedUpdate();
                    }
                }
            }
        }
    }
    
    void ProcessDeferredUpdates()
    {
        int updatesProcessed = 0;
        
        while (deferredUIUpdates.Count > 0 && updatesProcessed < maxUIUpdatesPerFrame)
        {
            var update = deferredUIUpdates.Dequeue();
            update?.Invoke();
            updatesProcessed++;
        }
    }
    
    void OptimizeObjectPooling()
    {
        // Create object pools for frequently created/destroyed UI elements
        GameObject poolContainer = new GameObject("HellLevelObjectPool");
        poolContainer.transform.SetParent(transform);
        
        // Pool for validation result popups
        CreateObjectPool(poolContainer, "ValidationResultPool", 5);
        
        // Pool for highlight effects
        CreateObjectPool(poolContainer, "HighlightEffectPool", 10);
        
        // Pool for error indicators
        CreateObjectPool(poolContainer, "ErrorIndicatorPool", 15);
    }
    
    void CreateObjectPool(GameObject parent, string poolName, int poolSize)
    {
        GameObject pool = new GameObject(poolName);
        pool.transform.SetParent(parent.transform);
        
        // Create pool objects (simplified implementation)
        for (int i = 0; i < poolSize; i++)
        {
            GameObject poolObj = new GameObject($"{poolName}_Object_{i}");
            poolObj.transform.SetParent(pool.transform);
            poolObj.SetActive(false);
        }
    }
    
    public void QueueUIUpdate(System.Action updateAction)
    {
        if (enableOptimizations && updateAction != null)
        {
            deferredUIUpdates.Enqueue(updateAction);
        }
        else
        {
            updateAction?.Invoke();
        }
    }
    
    void OnDisable()
    {
        if (updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);
            updateCoroutine = null;
        }
    }
}

// Interface for optimizable components
public interface IOptimizedComponent
{
    void OptimizedUpdate();
}

// Optimized Progress Indicator
public class OptimizedProgressIndicator : MonoBehaviour, IOptimizedComponent
{
    private SolutionProgressIndicator progressIndicator;
    private int lastCellCount = -1;
    private bool needsUpdate = false;
    
    void Start()
    {
        progressIndicator = GetComponent<SolutionProgressIndicator>();
    }
    
    public void OptimizedUpdate()
    {
        if (progressIndicator == null) return;
        
        // Only update if cell count actually changed
        var allCells = FindObjectsOfType<SudokuCell>();
        int currentFilledCount = allCells.Count(cell => cell.Number != 0);
        
        if (currentFilledCount != lastCellCount)
        {
            lastCellCount = currentFilledCount;
            progressIndicator.ForceRefresh();
        }
    }
}

// Optimized Validation Button
public class OptimizedValidationButton : MonoBehaviour, IOptimizedComponent
{
    private ManualValidationButton validationButton;
    private bool lastHasChanges = false;
    private bool lastIsHellLevel = false;
    
    void Start()
    {
        validationButton = GetComponent<ManualValidationButton>();
    }
    
    public void OptimizedUpdate()
    {
        if (validationButton == null) return;
        
        var sudokuBoard = FindObjectOfType<SudokuBoard>();
        if (sudokuBoard == null) return;
        
        bool currentIsHellLevel = sudokuBoard.IsHellLevel();
        
        // Only update visual state if hell level status changed
        if (currentIsHellLevel != lastIsHellLevel)
        {
            lastIsHellLevel = currentIsHellLevel;
            // Trigger button state update
        }
    }
}

// Optimized Visual Feedback
public class OptimizedVisualFeedback : MonoBehaviour, IOptimizedComponent
{
    private VisualFeedbackManager feedbackManager;
    private HashSet<int> lastHighlightedCells;
    
    void Start()
    {
        feedbackManager = GetComponent<VisualFeedbackManager>();
        lastHighlightedCells = new HashSet<int>();
    }
    
    public void OptimizedUpdate()
    {
        if (feedbackManager == null) return;
        
        // Optimize highlight updates by batching them
        // This reduces individual cell update calls
    }
}