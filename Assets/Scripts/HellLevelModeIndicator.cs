using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HellLevelModeIndicator : MonoBehaviour
{
    [Header("Hell Level Mode Indicator")]
    [SerializeField] private GameObject modeIndicatorPanel;
    [SerializeField] private Text modeIndicatorText;
    [SerializeField] private Image modeIndicatorBackground;
    [SerializeField] private AnimationCurve pulseAnimation = AnimationCurve.EaseInOut(0, 1, 1, 1.2f);
    [SerializeField] private float pulseDuration = 2f;
    [SerializeField] private bool enablePulseAnimation = true;
    
    [Header("Visuals & Aesthetics")]
    [SerializeField] private Color hellLevelBackgroundColor = new Color(0.8f, 0.2f, 0.1f, 0.9f); // Vibrant hellish red
    [SerializeField] private Color hellLevelTextColor = Color.white; // White for high contrast
    [SerializeField] private Color normalLevelBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private Color normalLevelTextColor = Color.white;
    
    [Header("Display Messages")]
    [SerializeField] private string hellLevelMessage = "🔥 HELL LEVEL 🔥";
    [SerializeField] private string normalLevelMessage = "";
    
    private SudokuBoard sudokuBoard;
    private ValidationContext validationContext;
    private Coroutine pulseCoroutine;
    private bool isHellLevel = false;
    
    void Start()
    {
        // Find required components
        sudokuBoard = FindObjectOfType<SudokuBoard>();
        if (sudokuBoard == null)
        {
            Debug.LogError("HellLevelModeIndicator: SudokuBoard not found!");
            gameObject.SetActive(false);
            return;
        }
        
        // Initialize the mode indicator
        InitializeModeIndicator();
    }
    
    void InitializeModeIndicator()
    {
        // Create UI components if they don't exist
        if (modeIndicatorPanel == null)
        {
            CreateModeIndicatorUI();
        }
        
        // Get ValidationContext and subscribe to changes
        validationContext = sudokuBoard.GetValidationContext();
        if (validationContext != null)
        {
            validationContext.OnStrategyChanged += OnStrategyChanged;
        }
        
        // Set initial state
        UpdateModeIndicator(sudokuBoard.IsHellLevel());
    }
    
    void CreateModeIndicatorUI()
    {
        // Create main panel
        GameObject panelObj = new GameObject("HellModeIndicatorPanel");
        panelObj.transform.SetParent(transform, false);
        
        // Add RectTransform and Image components
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        modeIndicatorBackground = panelObj.AddComponent<Image>();
        
        // Configure panel layout to be a more prominent part of the header
        panelRect.anchorMin = new Vector2(0.5f, 1f);
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.anchoredPosition = new Vector2(0, -60); // Positioned cleanly in the header
        panelRect.sizeDelta = new Vector2(280, 50);
        
        // Configure background
        modeIndicatorBackground.color = normalLevelBackgroundColor;
        modeIndicatorBackground.raycastTarget = false;
        
        // Create text element
        GameObject textObj = new GameObject("ModeIndicatorText");
        textObj.transform.SetParent(panelObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        modeIndicatorText = textObj.AddComponent<Text>();
        
        // Configure text layout (fill parent)
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // Configure text appearance
        modeIndicatorText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); // Using a modern font like Nunito or OpenSans is recommended
        modeIndicatorText.fontSize = 24; // Larger for better visibility
        modeIndicatorText.fontStyle = FontStyle.BoldAndItalic; // More dramatic styling
        modeIndicatorText.color = normalLevelTextColor;
        modeIndicatorText.alignment = TextAnchor.MiddleCenter;
        modeIndicatorText.raycastTarget = false;
        
        modeIndicatorPanel = panelObj;
        
        Debug.Log("HellLevelModeIndicator: UI created programmatically");
    }
    
    void OnStrategyChanged(bool isHellLevelActive)
    {
        UpdateModeIndicator(isHellLevelActive);
    }
    
    void UpdateModeIndicator(bool hellLevelActive)
    {
        if (modeIndicatorPanel == null || modeIndicatorText == null || modeIndicatorBackground == null)
            return;
            
        isHellLevel = hellLevelActive;
        
        if (hellLevelActive)
        {
            // Show Hell Level indicator
            modeIndicatorPanel.SetActive(true);
            modeIndicatorText.text = hellLevelMessage;
            modeIndicatorText.color = hellLevelTextColor;
            modeIndicatorBackground.color = hellLevelBackgroundColor;
            
            // Start pulse animation if enabled
            if (enablePulseAnimation && pulseCoroutine == null)
            {
                pulseCoroutine = StartCoroutine(PulseAnimation());
            }
        }
        else
        {
            // Hide or show minimal indicator for normal levels
            if (string.IsNullOrEmpty(normalLevelMessage))
            {
                modeIndicatorPanel.SetActive(false);
            }
            else
            {
                modeIndicatorPanel.SetActive(true);
                modeIndicatorText.text = normalLevelMessage;
                modeIndicatorText.color = normalLevelTextColor;
                modeIndicatorBackground.color = normalLevelBackgroundColor;
            }
            
            // Stop pulse animation
            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
                pulseCoroutine = null;
                
                // Reset scale
                if (modeIndicatorPanel != null)
                {
                    modeIndicatorPanel.transform.localScale = Vector3.one;
                }
            }
        }
        
        Debug.Log($"HellLevelModeIndicator: Updated for {(hellLevelActive ? "Hell" : "Normal")} level");
    }
    
    IEnumerator PulseAnimation()
    {
        if (modeIndicatorPanel == null) yield break;
        
        while (isHellLevel && modeIndicatorPanel.activeInHierarchy)
        {
            float elapsed = 0f;
            
            while (elapsed < pulseDuration)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = elapsed / pulseDuration;
                float scale = pulseAnimation.Evaluate(normalizedTime);
                
                if (modeIndicatorPanel != null)
                {
                    modeIndicatorPanel.transform.localScale = Vector3.one * scale;
                }
                
                yield return null;
            }
            
            // Reset scale before next cycle
            if (modeIndicatorPanel != null)
            {
                modeIndicatorPanel.transform.localScale = Vector3.one;
            }
            
            // Short pause between pulses
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    // Public methods for external control
    public void SetHellLevelMessage(string message)
    {
        hellLevelMessage = message;
        if (isHellLevel)
        {
            UpdateModeIndicator(true);
        }
    }
    
    public void SetNormalLevelMessage(string message)
    {
        normalLevelMessage = message;
        if (!isHellLevel)
        {
            UpdateModeIndicator(false);
        }
    }
    
    public void SetPulseEnabled(bool enabled)
    {
        enablePulseAnimation = enabled;
        
        if (!enabled && pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
            
            if (modeIndicatorPanel != null)
            {
                modeIndicatorPanel.transform.localScale = Vector3.one;
            }
        }
        else if (enabled && isHellLevel && pulseCoroutine == null)
        {
            pulseCoroutine = StartCoroutine(PulseAnimation());
        }
    }
    
    void OnDisable()
    {
        // Clean up coroutines
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (validationContext != null)
        {
            validationContext.OnStrategyChanged -= OnStrategyChanged;
        }
    }
    
    #if UNITY_EDITOR
    [Header("Debug Info (Editor Only)")]
    [SerializeField] private bool debugIsHellLevel;
    [SerializeField] private bool debugPanelActive;
    
    void Update()
    {
        if (Application.isEditor)
        {
            debugIsHellLevel = isHellLevel;
            debugPanelActive = modeIndicatorPanel != null && modeIndicatorPanel.activeInHierarchy;
        }
    }
    #endif
}