using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class SolutionProgressIndicator : MonoBehaviour
{
    [Header("Solution Progress Indicator")]
    [SerializeField] private GameObject progressPanel;
    [SerializeField] private Text progressText;
    [SerializeField] private Image progressCircle;
    [SerializeField] private Image progressBackground;
    
    [Header("Visual Design")]
    [SerializeField] private Color progressCircleColor = new Color(1f, 0.6f, 0f, 1f); // Vibrant orange
    [SerializeField] private Color progressBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.7f); // Dark, neutral background
    [SerializeField] private Color progressTextColor = Color.white; // White for readability
    [SerializeField] private float circleThickness = 8f;
    
    [Header("Layout Settings")]
    [SerializeField] private Vector2 indicatorSize = new Vector2(80, 80);
    [SerializeField] private Vector2 indicatorPosition = new Vector2(-30, -60); // Aligned with new header
    [SerializeField] private int fontSize = 20;
    
    [Header("Animation")]
    [SerializeField] private float animationSpeed = 2f;
    [SerializeField] private bool enableSmoothAnimation = true;
    
    private SudokuBoard sudokuBoard;
    private ValidationContext validationContext;
    private bool isHellLevel = false;
    private int currentFilledCells = 0;
    private int totalCells = 81;
    private float targetProgress = 0f;
    private float currentProgress = 0f;
    private Coroutine animationCoroutine;
    
    void Start()
    {
        // Find required components
        sudokuBoard = FindObjectOfType<SudokuBoard>();
        if (sudokuBoard == null)
        {
            Debug.LogError("SolutionProgressIndicator: SudokuBoard not found!");
            gameObject.SetActive(false);
            return;
        }
        
        // Initialize the progress indicator
        InitializeProgressIndicator();
    }
    
    void InitializeProgressIndicator()
    {
        // Create UI components if they don't exist
        if (progressPanel == null)
        {
            CreateProgressIndicatorUI();
        }
        
        // Get ValidationContext and subscribe to events
        validationContext = sudokuBoard.GetValidationContext();
        if (validationContext != null)
        {
            validationContext.OnStrategyChanged += OnStrategyChanged;
        }
        
        // Subscribe to game events for cell updates
        GameEvents.OnDidSetNumber += OnCellNumberSet;
        GameEvents.OnClearNumber += OnCellCleared;
        
        // Set initial state
        UpdateIndicatorState(sudokuBoard.IsHellLevel());
    }
    
    void CreateProgressIndicatorUI()
    {
        // Create main panel
        GameObject panelObj = new GameObject("SolutionProgressPanel");
        panelObj.transform.SetParent(transform, false);
        
        // Add RectTransform
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        
        // Configure panel layout (top right corner, in margin area)
        panelRect.anchorMin = new Vector2(1f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(1f, 1f);
        panelRect.anchoredPosition = indicatorPosition;
        panelRect.sizeDelta = indicatorSize;
        
        // Create progress background circle
        GameObject bgCircleObj = new GameObject("ProgressBackground");
        bgCircleObj.transform.SetParent(panelObj.transform, false);
        
        RectTransform bgRect = bgCircleObj.AddComponent<RectTransform>();
        progressBackground = bgCircleObj.AddComponent<Image>();
        
        // Configure background circle
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        progressBackground.sprite = CreateCircleSprite(false);
        progressBackground.color = progressBackgroundColor;
        progressBackground.type = Image.Type.Filled;
        progressBackground.fillMethod = Image.FillMethod.Radial360;
        progressBackground.fillAmount = 1f;
        
        // Create progress circle
        GameObject progressCircleObj = new GameObject("ProgressCircle");
        progressCircleObj.transform.SetParent(panelObj.transform, false);
        
        RectTransform progressRect = progressCircleObj.AddComponent<RectTransform>();
        progressCircle = progressCircleObj.AddComponent<Image>();
        
        // Configure progress circle
        progressRect.anchorMin = Vector2.zero;
        progressRect.anchorMax = Vector2.one;
        progressRect.offsetMin = Vector2.zero;
        progressRect.offsetMax = Vector2.zero;
        
        progressCircle.sprite = CreateCircleSprite(true);
        progressCircle.color = progressCircleColor;
        progressCircle.type = Image.Type.Filled;
        progressCircle.fillMethod = Image.FillMethod.Radial360;
        progressCircle.fillClockwise = true;
        progressCircle.fillAmount = 0f;
        
        // Create progress text
        GameObject textObj = new GameObject("ProgressText");
        textObj.transform.SetParent(panelObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        progressText = textObj.AddComponent<Text>();
        
        // Configure text layout (center of circle)
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // Configure text appearance
        progressText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        progressText.fontSize = fontSize;
        progressText.fontStyle = FontStyle.Bold; // Bold for Hell Level consistency
        progressText.color = progressTextColor;
        progressText.alignment = TextAnchor.MiddleCenter;
        progressText.text = "0/81";
        
        progressPanel = panelObj;
        
        Debug.Log("SolutionProgressIndicator: UI created programmatically");
    }
    
    Sprite CreateCircleSprite(bool isProgressRing)
    {
        int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = Vector2.one * (size / 2f);
        float outerRadius = size / 2f - 1f;
        float innerRadius = isProgressRing ? outerRadius - circleThickness : 0f;
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, center);
                
                if (distance <= outerRadius && distance >= innerRadius)
                {
                    // Create anti-aliased edge
                    float edgeAlpha = 1f;
                    if (distance > outerRadius - 1f)
                    {
                        edgeAlpha = outerRadius - distance;
                    }
                    else if (distance < innerRadius + 1f && innerRadius > 0f)
                    {
                        edgeAlpha = distance - innerRadius;
                    }
                    
                    edgeAlpha = Mathf.Clamp01(edgeAlpha);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, edgeAlpha));
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f);
    }
    
    void OnStrategyChanged(bool isHellLevelActive)
    {
        UpdateIndicatorState(isHellLevelActive);
    }
    
    void UpdateIndicatorState(bool hellLevelActive)
    {
        if (progressPanel == null)
            return;
            
        isHellLevel = hellLevelActive;
        
        if (hellLevelActive)
        {
            // Show progress indicator for Hell Level
            progressPanel.SetActive(true);
            RefreshProgress();
        }
        else
        {
            // Hide progress indicator for normal levels
            progressPanel.SetActive(false);
        }
        
        Debug.Log($"SolutionProgressIndicator: Updated for {(hellLevelActive ? "Hell" : "Normal")} level");
    }
    
    void OnCellNumberSet(int cellIndex)
    {
        if (isHellLevel)
        {
            RefreshProgress();
        }
    }
    
    void OnCellCleared()
    {
        if (isHellLevel)
        {
            RefreshProgress();
        }
    }
    
    void RefreshProgress()
    {
        if (!isHellLevel || sudokuBoard == null)
            return;
        
        // Count filled cells from the grid
        var gridSquares = GetGridSquares();
        if (gridSquares == null || gridSquares.Count == 0)
            return;
        
        int filledCells = 0;
        foreach (var square in gridSquares)
        {
            var cell = square.GetComponent<SudokuCell>();
            if (cell != null && cell.Number != 0)
            {
                filledCells++;
            }
        }
        
        currentFilledCells = filledCells;
        targetProgress = (float)filledCells / totalCells;
        
        // Update text immediately
        if (progressText != null)
        {
            progressText.text = $"{filledCells}/81";
        }
        
        // Animate progress circle
        if (enableSmoothAnimation)
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            animationCoroutine = StartCoroutine(AnimateProgress());
        }
        else
        {
            currentProgress = targetProgress;
            UpdateProgressVisual();
        }
    }
    
    IEnumerator AnimateProgress()
    {
        while (Mathf.Abs(currentProgress - targetProgress) > 0.001f)
        {
            currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, 
                animationSpeed * Time.deltaTime);
            UpdateProgressVisual();
            yield return null;
        }
        
        currentProgress = targetProgress;
        UpdateProgressVisual();
        animationCoroutine = null;
    }
    
    void UpdateProgressVisual()
    {
        if (progressCircle != null)
        {
            progressCircle.fillAmount = currentProgress;
        }
    }
    
    System.Collections.Generic.List<GameObject> GetGridSquares()
    {
        // Access grid squares through reflection or public method
        // For now, we'll use a simple approach by finding all SudokuCell components
        var sudokuCells = FindObjectsOfType<SudokuCell>();
        return sudokuCells.Select(cell => cell.gameObject).ToList();
    }
    
    // Public methods for external control
    public void SetProgress(int filledCells, int totalCells = 81)
    {
        this.totalCells = totalCells;
        currentFilledCells = filledCells;
        targetProgress = (float)filledCells / totalCells;
        
        if (progressText != null)
        {
            progressText.text = $"{filledCells}/{totalCells}";
        }
        
        if (enableSmoothAnimation)
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            animationCoroutine = StartCoroutine(AnimateProgress());
        }
        else
        {
            currentProgress = targetProgress;
            UpdateProgressVisual();
        }
    }
    
    public void ForceRefresh()
    {
        if (isHellLevel)
        {
            RefreshProgress();
        }
    }
    
    void OnDisable()
    {
        // Stop any running animations
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (validationContext != null)
        {
            validationContext.OnStrategyChanged -= OnStrategyChanged;
        }
        
        GameEvents.OnDidSetNumber -= OnCellNumberSet;
        GameEvents.OnClearNumber -= OnCellCleared;
    }
    
    #if UNITY_EDITOR
    [Header("Debug Info (Editor Only)")]
    [SerializeField] private bool debugIsHellLevel;
    [SerializeField] private int debugFilledCells;
    [SerializeField] private float debugCurrentProgress;
    [SerializeField] private float debugTargetProgress;
    
    void Update()
    {
        if (Application.isEditor)
        {
            debugIsHellLevel = isHellLevel;
            debugFilledCells = currentFilledCells;
            debugCurrentProgress = currentProgress;
            debugTargetProgress = targetProgress;
        }
    }
    #endif
}