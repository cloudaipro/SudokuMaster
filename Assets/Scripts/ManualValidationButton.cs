using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ManualValidationButton : MonoBehaviour
{
    [Header("Manual Validation Button")]
    [SerializeField] private Button validationButton;
    [SerializeField] private Text buttonText;
    [SerializeField] private Image buttonBackground;
    [SerializeField] private GameObject loadingIndicator;
    
    [Header("Button States")]
    [SerializeField] private string enabledText = "🔥 VALIDATE";
    [SerializeField] private string disabledText = "Make Your Moves";
    [SerializeField] private string loadingText = "Judging...";
    
    [Header("Visual Design")]
    [SerializeField] private Color enabledBackgroundColor = new Color(0.9f, 0.4f, 0f, 1f); // Strong, inviting orange
    [SerializeField] private Color enabledTextColor = Color.white;
    [SerializeField] private Color disabledBackgroundColor = new Color(0.4f, 0.4f, 0.4f, 0.8f); // Neutral gray for disabled state
    [SerializeField] private Color disabledTextColor = new Color(0.14f, 0.18f, 0.41f, 1f);
    [SerializeField] private Color loadingBackgroundColor = new Color(0.8f, 0.3f, 0f, 1f); // Slightly darker orange for loading
    [SerializeField] private Color loadingTextColor = Color.white;
    
    [Header("Layout Settings")]
    [SerializeField] private Vector2 buttonSize = new Vector2(320, 120);
    [SerializeField] private Vector2 buttonPosition = new Vector2(0, -60); // Lowered to be closer to other controls
    
    private SudokuBoard sudokuBoard;
    private ValidationContext validationContext;
    private bool isHellLevel = false;
    private bool hasChanges = false;
    private bool isValidating = false;
    
    
    void Start()
    {
        // Find required components
        sudokuBoard = FindObjectOfType<SudokuBoard>();
        if (sudokuBoard == null)
        {
            Debug.LogError("ManualValidationButton: SudokuBoard not found!");
            gameObject.SetActive(false);
            return;
        }
        
        // Initialize the validation button
        InitializeValidationButton();
    }
    
    void InitializeValidationButton()
    {
        // Create UI components if they don't exist
        if (validationButton == null)
        {
            CreateValidationButtonUI();
        }
        
        // Get ValidationContext and subscribe to events
        validationContext = sudokuBoard.GetValidationContext();
        if (validationContext != null)
        {
            validationContext.OnStrategyChanged += OnStrategyChanged;
            validationContext.OnValidationResult += OnValidationResult;
        }
        
        // Set up button click handler
        if (validationButton != null)
        {
            validationButton.onClick.AddListener(OnValidateButtonClicked);
        }
        
        // ValidationResultModal no longer used - visual feedback through cell colors instead
        
        // Subscribe to game events for change tracking
        GameEvents.OnDidSetNumber += OnCellChanged;
        GameEvents.OnClearNumber += OnCellCleared;
        
        // Set initial state
        UpdateButtonState(sudokuBoard.IsHellLevel());
    }
    
    void CreateValidationButtonUI()
    {
        // Create main button
        GameObject buttonObj = new GameObject("ManualValidationButton");
        buttonObj.transform.SetParent(transform, false);
        
        // Add RectTransform and Button components
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        validationButton = buttonObj.AddComponent<Button>();
        buttonBackground = buttonObj.AddComponent<Image>();
        
        // Configure button layout (in bottom margin area, above number buttons)
        buttonRect.anchorMin = new Vector2(0.5f, 0f);
        buttonRect.anchorMax = new Vector2(0.5f, 0f);
        buttonRect.pivot = new Vector2(0.5f, 0f);
        buttonRect.anchoredPosition = buttonPosition;
        buttonRect.sizeDelta = buttonSize;
        
        // Configure button appearance
        buttonBackground.color = disabledBackgroundColor;
        validationButton.targetGraphic = buttonBackground;
        
        // Create button text
        GameObject textObj = new GameObject("ButtonText");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        buttonText = textObj.AddComponent<Text>();
        
        // Configure text layout (fill parent)
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // Configure text appearance
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 36; // Slightly smaller for better fit
        buttonText.fontStyle = FontStyle.Bold; // Make it bold for impact
        buttonText.color = disabledTextColor;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.text = disabledText;
        
        // Create loading indicator (simple rotating element)
        CreateLoadingIndicator(buttonObj);
        
        Debug.Log("ManualValidationButton: UI created programmatically");
    }
    
    void CreateLoadingIndicator(GameObject parent)
    {
        GameObject loadingObj = new GameObject("LoadingIndicator");
        loadingObj.transform.SetParent(parent.transform, false);
        
        RectTransform loadingRect = loadingObj.AddComponent<RectTransform>();
        Image loadingImage = loadingObj.AddComponent<Image>();
        
        // Position on the right side of button
        loadingRect.anchorMin = new Vector2(1f, 0.5f);
        loadingRect.anchorMax = new Vector2(1f, 0.5f);
        loadingRect.pivot = new Vector2(1f, 0.5f);
        loadingRect.anchoredPosition = new Vector2(-10, 0);
        loadingRect.sizeDelta = new Vector2(30, 30);
        
        // Use a simple circle for loading indicator
        loadingImage.color = loadingTextColor;
        loadingImage.sprite = CreateCircleSprite();
        
        loadingIndicator = loadingObj;
        loadingIndicator.SetActive(false);
    }
    
    Sprite CreateCircleSprite()
    {
        // Create a simple circle texture for loading indicator
        int size = 32;
        Texture2D texture = new Texture2D(size, size);
        Vector2 center = Vector2.one * (size / 2f);
        float radius = size / 2f - 2f;
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, center);
                
                if (distance <= radius && distance >= radius - 3f)
                {
                    texture.SetPixel(x, y, Color.white);
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
        UpdateButtonState(isHellLevelActive);
    }
    
    void UpdateButtonState(bool hellLevelActive)
    {
        if (validationButton == null || buttonText == null || buttonBackground == null)
            return;
            
        isHellLevel = hellLevelActive;
        
        if (hellLevelActive)
        {
            // Show validation button for Hell Level
            gameObject.SetActive(true);
            UpdateButtonVisuals();
        }
        else
        {
            // Hide validation button for normal levels
            gameObject.SetActive(false);
        }
        
        Debug.Log($"ManualValidationButton: Updated for {(hellLevelActive ? "Hell" : "Normal")} level");
    }
    
    void UpdateButtonVisuals()
    {
        if (!isHellLevel) return;
        
        if (isValidating)
        {
            // Loading state
            buttonText.text = loadingText;
            buttonText.color = loadingTextColor;
            buttonBackground.color = loadingBackgroundColor;
            validationButton.interactable = false;
            
            if (loadingIndicator != null)
            {
                loadingIndicator.SetActive(true);
                StartCoroutine(RotateLoadingIndicator());
            }
        }
        else if (hasChanges)
        {
            // Enabled state
            buttonText.text = enabledText;
            buttonText.color = enabledTextColor;
            buttonBackground.color = enabledBackgroundColor;
            validationButton.interactable = true;
            
            if (loadingIndicator != null)
            {
                loadingIndicator.SetActive(false);
            }
        }
        else
        {
            // Disabled state
            buttonText.text = disabledText;
            buttonText.color = disabledTextColor;
            buttonBackground.color = disabledBackgroundColor;
            validationButton.interactable = false;
            
            if (loadingIndicator != null)
            {
                loadingIndicator.SetActive(false);
            }
        }
    }
    
    IEnumerator RotateLoadingIndicator()
    {
        if (loadingIndicator == null) yield break;
        
        while (isValidating && loadingIndicator.activeInHierarchy)
        {
            loadingIndicator.transform.Rotate(0, 0, -180 * Time.deltaTime);
            yield return null;
        }
        
        // Reset rotation
        if (loadingIndicator != null)
        {
            loadingIndicator.transform.rotation = Quaternion.identity;
        }
    }
    
    void OnValidateButtonClicked()
    {
        if (!isHellLevel || isValidating || validationContext == null)
            return;
        
        Debug.Log("Manual validation requested");
        StartCoroutine(PerformValidation());
    }
    
    IEnumerator PerformValidation()
    {
        isValidating = true;
        UpdateButtonVisuals();
        
        // Small delay for visual feedback
        yield return new WaitForSeconds(0.3f);
        
        // Perform validation through ValidationContext
        ValidationResult result = validationContext.ValidateBoard();
        
        // Unlock any locked numbers since validation converts hypothesis to regular numbers
        var lockManager = FindObjectOfType<NumberLockManager>();
        if (lockManager != null && lockManager.HasLockedNumber())
        {
            lockManager.UnlockNumber();
            Debug.Log("ManualValidationButton: Unlocked number after validation");
        }
        
        // Process result
        OnValidationResult(result);
        
        isValidating = false;
        UpdateButtonVisuals();
    }
    
    void OnValidationResult(ValidationResult result)
    {
        if (result == null) return;
        
        Debug.Log($"Validation result: {result.Type} - {result.Message}");
        
        // No longer show modal - visual feedback through cell colors instead
        // The validation will update has_wrong_value on cells, causing red text
        
        // Update button state based on result
        switch (result.Type)
        {
            case ValidationResultType.Success:
                hasChanges = false;
                break;
                
            case ValidationResultType.PartialSuccess:
            case ValidationResultType.Error:
                // Keep changes flag as is - player can continue working
                break;
        }
    }
    
    // Change tracking methods
    void OnCellChanged(int cellIndex)
    {
        if (isHellLevel)
        {
            SetHasChanges(true);
        }
    }
    
    void OnCellCleared()
    {
        if (isHellLevel)
        {
            SetHasChanges(true);
        }
    }
    
    // Public methods for external control
    public void SetHasChanges(bool changes)
    {
        hasChanges = changes;
        if (isHellLevel)
        {
            UpdateButtonVisuals();
        }
    }
    
    public void TriggerValidation()
    {
        if (validationButton != null && validationButton.interactable)
        {
            OnValidateButtonClicked();
        }
    }
    
    void OnDisable()
    {
        // Stop any running coroutines
        StopAllCoroutines();
        isValidating = false;
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (validationContext != null)
        {
            validationContext.OnStrategyChanged -= OnStrategyChanged;
            validationContext.OnValidationResult -= OnValidationResult;
        }
        
        // Clean up button click listener
        if (validationButton != null)
        {
            validationButton.onClick.RemoveAllListeners();
        }
    }
    
    #if UNITY_EDITOR
    [Header("Debug Info (Editor Only)")]
    [SerializeField] private bool debugIsHellLevel;
    [SerializeField] private bool debugHasChanges;
    [SerializeField] private bool debugIsValidating;
    
    void Update()
    {
        if (Application.isEditor)
        {
            debugIsHellLevel = isHellLevel;
            debugHasChanges = hasChanges;
            debugIsValidating = isValidating;
        }
    }
    #endif
}