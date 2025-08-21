using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ValidationResultModal : MonoBehaviour
{
    [Header("Validation Result Modal")]
    [SerializeField] private GameObject modalPanel;
    [SerializeField] private GameObject modalBackground;
    [SerializeField] private Text modalTitle;
    [SerializeField] private Text modalMessage;
    [SerializeField] private Text progressText;
    [SerializeField] private Image progressBar;
    [SerializeField] private ScrollRect errorScrollView;
    [SerializeField] private Text errorDetailsText;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button resetIncorrectButton;
    [SerializeField] private Button closeButton;
    
    [Header("Visual Design")]
    [SerializeField] private Color successBackgroundColor = new Color(0.1f, 0.4f, 0.1f, 0.98f); // Dark success green
    [SerializeField] private Color partialBackgroundColor = new Color(0.7f, 0.25f, 0.05f, 0.98f); // Hell fire partial
    [SerializeField] private Color errorBackgroundColor = new Color(0.8f, 0.1f, 0.05f, 0.98f); // Intense Hell red
    [SerializeField] private Color overlayColor = new Color(0.05f, 0.01f, 0.01f, 0.85f); // Dark red overlay
    
    [Header("Layout Settings")]
    [SerializeField] private Vector2 modalSize = new Vector2(500, 400);
    [SerializeField] private float animationDuration = 0.3f;
    
    private SudokuBoard sudokuBoard;
    private ValidationContext validationContext;
    private ValidationResult currentResult;
    private Coroutine animationCoroutine;
    
    void Start()
    {
        // Find required components
        sudokuBoard = FindObjectOfType<SudokuBoard>();
        if (sudokuBoard != null)
        {
            validationContext = sudokuBoard.GetValidationContext();
        }
        
        // Initialize the modal
        InitializeModal();
        
        // Hide modal initially
        if (modalPanel != null)
        {
            modalPanel.SetActive(false);
        }
    }
    
    void InitializeModal()
    {
        // Create UI components if they don't exist
        if (modalPanel == null)
        {
            CreateModalUI();
        }
        
        // Set up button click handlers
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }
        
        if (resetIncorrectButton != null)
        {
            resetIncorrectButton.onClick.AddListener(OnResetIncorrectClicked);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseClicked);
        }
    }
    
    void CreateModalUI()
    {
        // Create overlay background
        GameObject overlayObj = new GameObject("ValidationModalOverlay");
        overlayObj.transform.SetParent(transform, false);
        
        RectTransform overlayRect = overlayObj.AddComponent<RectTransform>();
        Image overlayImage = overlayObj.AddComponent<Image>();
        Button overlayButton = overlayObj.AddComponent<Button>(); // For click-to-close
        
        // Configure overlay (full screen)
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        
        overlayImage.color = overlayColor;
        overlayButton.onClick.AddListener(OnCloseClicked);
        
        modalBackground = overlayObj;
        
        // Create main modal panel
        GameObject panelObj = new GameObject("ValidationModalPanel");
        panelObj.transform.SetParent(overlayObj.transform, false);
        
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        Image panelImage = panelObj.AddComponent<Image>();
        
        // Configure panel (center of screen)
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = modalSize;
        
        panelImage.color = errorBackgroundColor; // Default color
        
        modalPanel = panelObj;
        
        // Create modal content
        CreateModalContent(panelObj);
        
        Debug.Log("ValidationResultModal: UI created programmatically");
    }
    
    void CreateModalContent(GameObject parent)
    {
        float yOffset = modalSize.y / 2f - 30f; // Start from top
        
        // Title
        modalTitle = CreateTextElement("Title", parent, new Vector2(0, yOffset), new Vector2(modalSize.x - 40, 40));
        modalTitle.fontSize = 24;
        modalTitle.fontStyle = FontStyle.Bold;
        modalTitle.alignment = TextAnchor.MiddleCenter;
        modalTitle.text = "Validation Results";
        
        yOffset -= 60f;
        
        // Progress bar background
        GameObject progressBgObj = new GameObject("ProgressBackground");
        progressBgObj.transform.SetParent(parent.transform, false);
        
        RectTransform progressBgRect = progressBgObj.AddComponent<RectTransform>();
        Image progressBgImage = progressBgObj.AddComponent<Image>();
        
        progressBgRect.anchoredPosition = new Vector2(0, yOffset);
        progressBgRect.sizeDelta = new Vector2(modalSize.x - 40, 20);
        progressBgImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        
        // Progress bar fill
        GameObject progressFillObj = new GameObject("ProgressFill");
        progressFillObj.transform.SetParent(progressBgObj.transform, false);
        
        RectTransform progressFillRect = progressFillObj.AddComponent<RectTransform>();
        progressBar = progressFillObj.AddComponent<Image>();
        
        progressFillRect.anchorMin = new Vector2(0, 0);
        progressFillRect.anchorMax = new Vector2(1, 1);
        progressFillRect.offsetMin = Vector2.zero;
        progressFillRect.offsetMax = Vector2.zero;
        
        progressBar.color = new Color(0.2f, 0.8f, 0.2f, 1f);
        progressBar.type = Image.Type.Filled;
        progressBar.fillMethod = Image.FillMethod.Horizontal;
        
        yOffset -= 40f;
        
        // Progress text
        progressText = CreateTextElement("Progress", parent, new Vector2(0, yOffset), new Vector2(modalSize.x - 40, 30));
        progressText.fontSize = 16;
        progressText.alignment = TextAnchor.MiddleCenter;
        
        yOffset -= 50f;
        
        // Main message
        modalMessage = CreateTextElement("Message", parent, new Vector2(0, yOffset), new Vector2(modalSize.x - 40, 60));
        modalMessage.fontSize = 18;
        modalMessage.alignment = TextAnchor.MiddleCenter;
        
        yOffset -= 80f;
        
        // Error details scroll view
        CreateErrorScrollView(parent, new Vector2(0, yOffset), new Vector2(modalSize.x - 40, 120));
        
        yOffset -= 140f;
        
        // Buttons
        CreateButtons(parent, yOffset);
    }
    
    Text CreateTextElement(string name, GameObject parent, Vector2 position, Vector2 size)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        Text text = textObj.AddComponent<Text>();
        
        textRect.anchoredPosition = position;
        textRect.sizeDelta = size;
        
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 14;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleLeft;
        
        return text;
    }
    
    void CreateErrorScrollView(GameObject parent, Vector2 position, Vector2 size)
    {
        // Create scroll view
        GameObject scrollObj = new GameObject("ErrorScrollView");
        scrollObj.transform.SetParent(parent.transform, false);
        
        RectTransform scrollRect = scrollObj.AddComponent<RectTransform>();
        Image scrollImage = scrollObj.AddComponent<Image>();
        errorScrollView = scrollObj.AddComponent<ScrollRect>();
        
        scrollRect.anchoredPosition = position;
        scrollRect.sizeDelta = size;
        scrollImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        
        // Create viewport
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(scrollObj.transform, false);
        
        RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
        Image viewportImage = viewportObj.AddComponent<Image>();
        Mask viewportMask = viewportObj.AddComponent<Mask>();
        
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(5, 5);
        viewportRect.offsetMax = new Vector2(-5, -5);
        viewportImage.color = Color.clear;
        
        // Create content
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportObj.transform, false);
        
        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, size.y - 10);
        
        // Create error details text
        errorDetailsText = CreateTextElement("ErrorDetails", contentObj, Vector2.zero, new Vector2(size.x - 20, size.y - 10));
        errorDetailsText.alignment = TextAnchor.UpperLeft;
        
        RectTransform errorTextRect = errorDetailsText.GetComponent<RectTransform>();
        errorTextRect.anchorMin = Vector2.zero;
        errorTextRect.anchorMax = Vector2.one;
        errorTextRect.offsetMin = Vector2.zero;
        errorTextRect.offsetMax = Vector2.zero;
        
        // Configure scroll view
        errorScrollView.content = contentRect;
        errorScrollView.viewport = viewportRect;
        errorScrollView.horizontal = false;
        errorScrollView.vertical = true;
    }
    
    void CreateButtons(GameObject parent, float yOffset)
    {
        float buttonWidth = 120f;
        float buttonHeight = 40f;
        float buttonSpacing = 20f;
        
        // Continue button
        continueButton = CreateButton("Continue", parent, 
            new Vector2(-buttonWidth - buttonSpacing/2, yOffset), 
            new Vector2(buttonWidth, buttonHeight),
            new Color(0.2f, 0.6f, 0.2f, 1f));
        
        // Reset Incorrect button  
        resetIncorrectButton = CreateButton("Reset Errors", parent,
            new Vector2(buttonWidth + buttonSpacing/2, yOffset),
            new Vector2(buttonWidth, buttonHeight), 
            new Color(0.6f, 0.4f, 0.1f, 1f));
        
        // Close button (X in top right)
        closeButton = CreateButton("✕", parent,
            new Vector2(modalSize.x/2 - 25, modalSize.y/2 - 25),
            new Vector2(40, 40),
            new Color(0.6f, 0.2f, 0.2f, 1f));
        
        var closeText = closeButton.GetComponentInChildren<Text>();
        if (closeText != null)
        {
            closeText.fontSize = 20;
            closeText.fontStyle = FontStyle.Bold;
        }
    }
    
    Button CreateButton(string text, GameObject parent, Vector2 position, Vector2 size, Color color)
    {
        GameObject buttonObj = new GameObject($"Button_{text}");
        buttonObj.transform.SetParent(parent.transform, false);
        
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        Image buttonImage = buttonObj.AddComponent<Image>();
        Button button = buttonObj.AddComponent<Button>();
        
        buttonRect.anchoredPosition = position;
        buttonRect.sizeDelta = size;
        
        buttonImage.color = color;
        button.targetGraphic = buttonImage;
        
        // Create button text
        Text buttonText = CreateTextElement($"{text}Text", buttonObj, Vector2.zero, size);
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.fontSize = 16;
        
        RectTransform textRect = buttonText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        return button;
    }
    
    public void ShowValidationResult(ValidationResult result)
    {
        if (modalPanel == null || result == null)
            return;
        
        currentResult = result;
        
        // Configure modal based on result type
        ConfigureModalForResult(result);
        
        // Show modal with animation
        modalPanel.SetActive(true);
        
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(AnimateModalIn());
    }
    
    void ConfigureModalForResult(ValidationResult result)
    {
        Image panelImage = modalPanel.GetComponent<Image>();
        
        switch (result.Type)
        {
            case ValidationResultType.Success:
                panelImage.color = successBackgroundColor;
                modalTitle.text = "🎉 Validation Results";
                progressText.text = "Solution Progress: 100% Complete";
                progressBar.fillAmount = 1f;
                progressBar.color = new Color(0.2f, 0.8f, 0.2f, 1f);
                resetIncorrectButton.gameObject.SetActive(false);
                break;
                
            case ValidationResultType.PartialSuccess:
                panelImage.color = partialBackgroundColor;
                modalTitle.text = "✅ Validation Results";
                progressText.text = $"Solution Progress: {result.CompletionPercentage:F1}% Complete";
                progressBar.fillAmount = result.CompletionPercentage / 100f;
                progressBar.color = new Color(0.8f, 0.6f, 0.2f, 1f);
                resetIncorrectButton.gameObject.SetActive(result.ErrorCount > 0);
                break;
                
            case ValidationResultType.Error:
                panelImage.color = errorBackgroundColor;
                modalTitle.text = "❌ Validation Results";
                progressText.text = $"Solution Progress: {result.CompletionPercentage:F1}% Complete";
                progressBar.fillAmount = result.CompletionPercentage / 100f;
                progressBar.color = new Color(0.8f, 0.2f, 0.2f, 1f);
                resetIncorrectButton.gameObject.SetActive(true);
                break;
                
            default:
                panelImage.color = errorBackgroundColor;
                modalTitle.text = "Validation Results";
                progressText.text = "Processing...";
                progressBar.fillAmount = 0f;
                resetIncorrectButton.gameObject.SetActive(false);
                break;
        }
        
        // Set main message
        modalMessage.text = result.Message;
        
        // Generate detailed error information
        GenerateErrorDetails(result);
    }
    
    void GenerateErrorDetails(ValidationResult result)
    {
        if (errorDetailsText == null)
            return;
        
        string details = "";
        
        if (result.ErrorCount == 0)
        {
            details = result.Type == ValidationResultType.Success 
                ? "🏆 Congratulations! Perfect solution with no errors detected!"
                : "✨ Excellent progress! No conflicts detected in current placement.";
        }
        else
        {
            details += $"📋 Error Summary:\n";
            details += $"• Total conflicts: {result.ErrorCount}\n";
            details += $"• Completion: {result.CompletionPercentage:F1}%\n\n";
            
            if (result.ErrorCells != null && result.ErrorCells.Length > 0)
            {
                details += "🎯 Problematic Areas:\n";
                
                // Group errors by type (this is a simplified version)
                var errorCellsList = result.ErrorCells.ToList();
                int displayLimit = 10; // Limit display to avoid overwhelming
                
                for (int i = 0; i < Mathf.Min(errorCellsList.Count, displayLimit); i++)
                {
                    int cellIndex = errorCellsList[i];
                    int row = cellIndex / 9 + 1;
                    int col = cellIndex % 9 + 1;
                    details += $"• Cell R{row}C{col} has conflicts\n";
                }
                
                if (errorCellsList.Count > displayLimit)
                {
                    details += $"• ... and {errorCellsList.Count - displayLimit} more cells\n";
                }
            }
            
            details += "\n💡 Tip: Focus on one region at a time and verify each number follows Sudoku rules.";
        }
        
        errorDetailsText.text = details;
        
        // Adjust content size for scrolling
        if (errorScrollView != null)
        {
            float textHeight = errorDetailsText.preferredHeight;
            var contentRect = errorScrollView.content;
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, Mathf.Max(textHeight, 100f));
        }
    }
    
    IEnumerator AnimateModalIn()
    {
        if (modalPanel == null) yield break;
        
        // Start with scale 0
        modalPanel.transform.localScale = Vector3.zero;
        
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / animationDuration;
            
            // Ease out animation
            float scale = Mathf.Lerp(0f, 1f, 1f - Mathf.Pow(1f - progress, 3f));
            modalPanel.transform.localScale = Vector3.one * scale;
            
            yield return null;
        }
        
        modalPanel.transform.localScale = Vector3.one;
        animationCoroutine = null;
    }
    
    IEnumerator AnimateModalOut()
    {
        if (modalPanel == null) yield break;
        
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / animationDuration;
            
            // Ease in animation
            float scale = Mathf.Lerp(1f, 0f, Mathf.Pow(progress, 3f));
            modalPanel.transform.localScale = Vector3.one * scale;
            
            yield return null;
        }
        
        modalPanel.SetActive(false);
        modalPanel.transform.localScale = Vector3.one;
        animationCoroutine = null;
    }
    
    void OnContinueClicked()
    {
        HideModal();
    }
    
    void OnResetIncorrectClicked()
    {
        if (currentResult != null && currentResult.ErrorCells != null && sudokuBoard != null)
        {
            // Reset incorrect cells (this would need integration with SudokuBoard)
            Debug.Log($"Reset {currentResult.ErrorCells.Length} incorrect cells");
            
            // For now, just close the modal
            // TODO: Implement actual cell reset functionality
        }
        
        HideModal();
    }
    
    void OnCloseClicked()
    {
        HideModal();
    }
    
    public void HideModal()
    {
        if (modalPanel == null || !modalPanel.activeInHierarchy)
            return;
        
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(AnimateModalOut());
    }
    
    void OnDestroy()
    {
        // Clean up button listeners
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
        }
        
        if (resetIncorrectButton != null)
        {
            resetIncorrectButton.onClick.RemoveAllListeners();
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
        }
    }
}