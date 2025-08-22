using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HellLevelIntroModal : MonoBehaviour
{
    [Header("Hell Level Introduction Modal")]
    [SerializeField] private GameObject modalPanel;
    [SerializeField] private GameObject modalBackground;
    [SerializeField] private Text titleText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Button beginButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private ScrollRect instructionsScrollView;
    [SerializeField] private Text instructionsText;
    
    [Header("Visual Design")]
    [SerializeField] private Color modalBackgroundColor = new Color(0.05f, 0.02f, 0.02f, 0.95f);
    [SerializeField] private Color titleBackgroundColor = new Color(0.9f, 0.15f, 0.05f, 1f);
    [SerializeField] private Color titleTextColor = new Color(1f, 0.95f, 0.85f, 1f);
    [SerializeField] private Color descriptionTextColor = new Color(1f, 0.85f, 0.7f, 1f);
    [SerializeField] private Color buttonBackgroundColor = new Color(0.9f, 0.4f, 0f, 1f);
    [SerializeField] private Color buttonTextColor = Color.white;
    [SerializeField] private Color cancelButtonColor = new Color(0.4f, 0.4f, 0.4f, 1f);
    
    [Header("Layout Settings")]
    [SerializeField] private Vector2 modalSize = new Vector2(600, 500);
    [SerializeField] private float animationDuration = 0.3f;
    
    [Header("Content")]
    [SerializeField] private string hellLevelTitle = "🔥 HELL LEVEL 🔥";
    [SerializeField] private string hellLevelDescription = "Welcome to the ultimate Sudoku challenge!";
    [SerializeField] private string hellLevelInstructions = @"HELL LEVEL RULES:

🔥 HYPOTHESIS MODE
• Place numbers freely to test your theories
• Numbers appear in ORANGE until validated
• No immediate feedback - think carefully!

🔥 MANUAL VALIDATION
• Click '🔥 VALIDATE' when ready to check your work
• Get comprehensive feedback on your progress
• See exactly which cells have errors

🔥 NO ASSISTANCE
• Hints are disabled - rely on pure logic
• Fast Notes won't help you here
• Every move counts toward your final score

🔥 CHALLENGE FEATURES
• Track your progress with the circular indicator
• Ultra-difficult puzzles requiring advanced techniques
• Graded feedback shows completion percentage

Are you ready to face the ultimate test?";
    
    private bool isVisible = false;
    private Coroutine animationCoroutine;
    
    void Start()
    {
        // Initialize the introduction modal
        InitializeIntroModal();
        
        // Check if this is the first time accessing Hell Level
        CheckFirstTimeAccess();
    }
    
    void InitializeIntroModal()
    {
        // Create UI components if they don't exist
        if (modalPanel == null)
        {
            CreateIntroModalUI();
        }
        
        // Initially hide the modal
        if (modalPanel != null)
        {
            modalPanel.SetActive(false);
        }
    }
    
    void CreateIntroModalUI()
    {
        // Create modal background overlay
        GameObject backgroundObj = new GameObject("HellIntroModalBackground");
        backgroundObj.transform.SetParent(transform, false);
        
        RectTransform backgroundRect = backgroundObj.AddComponent<RectTransform>();
        Image backgroundImage = backgroundObj.AddComponent<Image>();
        Button backgroundButton = backgroundObj.AddComponent<Button>();
        
        // Configure full-screen overlay
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        
        backgroundImage.color = new Color(0f, 0f, 0f, 0.8f);
        backgroundButton.onClick.AddListener(CloseModal);
        
        modalBackground = backgroundObj;
        
        // Create main modal panel
        GameObject panelObj = new GameObject("HellIntroModalPanel");
        panelObj.transform.SetParent(backgroundObj.transform, false);
        
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        Image panelImage = panelObj.AddComponent<Image>();
        
        // Configure modal panel
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = modalSize;
        
        panelImage.color = modalBackgroundColor;
        
        modalPanel = panelObj;
        
        // Create title section
        CreateTitleSection(panelObj);
        
        // Create description section
        CreateDescriptionSection(panelObj);
        
        // Create instructions scroll view
        CreateInstructionsSection(panelObj);
        
        // Create buttons
        CreateButtonsSection(panelObj);
        
        Debug.Log("HellLevelIntroModal: UI created programmatically");
    }
    
    void CreateTitleSection(GameObject parent)
    {
        GameObject titleObj = new GameObject("TitleSection");
        titleObj.transform.SetParent(parent.transform, false);
        
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        Image titleBackground = titleObj.AddComponent<Image>();
        
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = Vector2.zero;
        titleRect.sizeDelta = new Vector2(0, 60);
        
        titleBackground.color = titleBackgroundColor;
        
        // Create title text
        GameObject titleTextObj = new GameObject("TitleText");
        titleTextObj.transform.SetParent(titleObj.transform, false);
        
        RectTransform titleTextRect = titleTextObj.AddComponent<RectTransform>();
        titleText = titleTextObj.AddComponent<Text>();
        
        titleTextRect.anchorMin = Vector2.zero;
        titleTextRect.anchorMax = Vector2.one;
        titleTextRect.offsetMin = Vector2.zero;
        titleTextRect.offsetMax = Vector2.zero;
        
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 28;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = titleTextColor;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.text = hellLevelTitle;
    }
    
    void CreateDescriptionSection(GameObject parent)
    {
        GameObject descObj = new GameObject("DescriptionSection");
        descObj.transform.SetParent(parent.transform, false);
        
        RectTransform descRect = descObj.AddComponent<RectTransform>();
        
        descRect.anchorMin = new Vector2(0f, 1f);
        descRect.anchorMax = new Vector2(1f, 1f);
        descRect.pivot = new Vector2(0.5f, 1f);
        descRect.anchoredPosition = new Vector2(0, -60);
        descRect.sizeDelta = new Vector2(-20, 40);
        
        descriptionText = descObj.AddComponent<Text>();
        
        descriptionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        descriptionText.fontSize = 18;
        descriptionText.fontStyle = FontStyle.Italic;
        descriptionText.color = descriptionTextColor;
        descriptionText.alignment = TextAnchor.MiddleCenter;
        descriptionText.text = hellLevelDescription;
    }
    
    void CreateInstructionsSection(GameObject parent)
    {
        GameObject scrollObj = new GameObject("InstructionsScrollView");
        scrollObj.transform.SetParent(parent.transform, false);
        
        RectTransform scrollRect = scrollObj.AddComponent<RectTransform>();
        instructionsScrollView = scrollObj.AddComponent<ScrollRect>();
        Image scrollBackground = scrollObj.AddComponent<Image>();
        
        scrollRect.anchorMin = new Vector2(0f, 0f);
        scrollRect.anchorMax = new Vector2(1f, 1f);
        scrollRect.pivot = new Vector2(0.5f, 0.5f);
        scrollRect.anchoredPosition = Vector2.zero;
        scrollRect.offsetMin = new Vector2(10, 80);
        scrollRect.offsetMax = new Vector2(-10, -100);
        
        scrollBackground.color = new Color(0.1f, 0.05f, 0.05f, 0.8f);
        
        // Create content container
        GameObject contentObj = new GameObject("ScrollContent");
        contentObj.transform.SetParent(scrollObj.transform, false);
        
        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 600); // Tall enough for all content
        
        // Create instructions text
        instructionsText = contentObj.AddComponent<Text>();
        
        instructionsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        instructionsText.fontSize = 14;
        instructionsText.color = descriptionTextColor;
        instructionsText.alignment = TextAnchor.UpperLeft;
        instructionsText.text = hellLevelInstructions;
        
        // Configure scroll view
        instructionsScrollView.content = contentRect;
        instructionsScrollView.vertical = true;
        instructionsScrollView.horizontal = false;
        instructionsScrollView.movementType = ScrollRect.MovementType.Clamped;
    }
    
    void CreateButtonsSection(GameObject parent)
    {
        // Begin Button
        GameObject beginButtonObj = new GameObject("BeginButton");
        beginButtonObj.transform.SetParent(parent.transform, false);
        
        RectTransform beginRect = beginButtonObj.AddComponent<RectTransform>();
        beginButton = beginButtonObj.AddComponent<Button>();
        Image beginBackground = beginButtonObj.AddComponent<Image>();
        
        beginRect.anchorMin = new Vector2(0f, 0f);
        beginRect.anchorMax = new Vector2(0.5f, 0f);
        beginRect.pivot = new Vector2(0.5f, 0f);
        beginRect.anchoredPosition = new Vector2(0, 15);
        beginRect.sizeDelta = new Vector2(-10, 50);
        
        beginBackground.color = buttonBackgroundColor;
        beginButton.targetGraphic = beginBackground;
        beginButton.onClick.AddListener(BeginHellLevel);
        
        // Begin button text
        GameObject beginTextObj = new GameObject("BeginButtonText");
        beginTextObj.transform.SetParent(beginButtonObj.transform, false);
        
        RectTransform beginTextRect = beginTextObj.AddComponent<RectTransform>();
        Text beginText = beginTextObj.AddComponent<Text>();
        
        beginTextRect.anchorMin = Vector2.zero;
        beginTextRect.anchorMax = Vector2.one;
        beginTextRect.offsetMin = Vector2.zero;
        beginTextRect.offsetMax = Vector2.zero;
        
        beginText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        beginText.fontSize = 18;
        beginText.fontStyle = FontStyle.Bold;
        beginText.color = buttonTextColor;
        beginText.alignment = TextAnchor.MiddleCenter;
        beginText.text = "🔥 BEGIN HELL LEVEL";
        
        // Cancel Button
        GameObject cancelButtonObj = new GameObject("CancelButton");
        cancelButtonObj.transform.SetParent(parent.transform, false);
        
        RectTransform cancelRect = cancelButtonObj.AddComponent<RectTransform>();
        cancelButton = cancelButtonObj.AddComponent<Button>();
        Image cancelBackground = cancelButtonObj.AddComponent<Image>();
        
        cancelRect.anchorMin = new Vector2(0.5f, 0f);
        cancelRect.anchorMax = new Vector2(1f, 0f);
        cancelRect.pivot = new Vector2(0.5f, 0f);
        cancelRect.anchoredPosition = new Vector2(0, 15);
        cancelRect.sizeDelta = new Vector2(-10, 50);
        
        cancelBackground.color = cancelButtonColor;
        cancelButton.targetGraphic = cancelBackground;
        cancelButton.onClick.AddListener(CloseModal);
        
        // Cancel button text
        GameObject cancelTextObj = new GameObject("CancelButtonText");
        cancelTextObj.transform.SetParent(cancelButtonObj.transform, false);
        
        RectTransform cancelTextRect = cancelTextObj.AddComponent<RectTransform>();
        Text cancelText = cancelTextObj.AddComponent<Text>();
        
        cancelTextRect.anchorMin = Vector2.zero;
        cancelTextRect.anchorMax = Vector2.one;
        cancelTextRect.offsetMin = Vector2.zero;
        cancelTextRect.offsetMax = Vector2.zero;
        
        cancelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        cancelText.fontSize = 16;
        cancelText.color = buttonTextColor;
        cancelText.alignment = TextAnchor.MiddleCenter;
        cancelText.text = "Cancel";
    }
    
    void CheckFirstTimeAccess()
    {
        // Check if this is the first time the player is accessing Hell Level
        bool hasSeenIntro = PlayerPrefs.GetInt("HellLevelIntroSeen", 0) == 1;
        
        if (!hasSeenIntro)
        {
            // Show introduction on first access
            StartCoroutine(ShowModalAfterDelay(1f));
        }
    }
    
    IEnumerator ShowModalAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowModal();
    }
    
    public void ShowModal()
    {
        if (modalPanel != null && !isVisible)
        {
            isVisible = true;
            modalPanel.SetActive(true);
            
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            animationCoroutine = StartCoroutine(AnimateModalIn());
        }
    }
    
    public void CloseModal()
    {
        if (modalPanel != null && isVisible)
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            animationCoroutine = StartCoroutine(AnimateModalOut());
        }
    }
    
    void BeginHellLevel()
    {
        // Mark that the player has seen the introduction
        PlayerPrefs.SetInt("HellLevelIntroSeen", 1);
        PlayerPrefs.Save();
        
        Debug.Log("HellLevelIntroModal: Player chose to begin Hell Level");
        CloseModal();
    }
    
    IEnumerator AnimateModalIn()
    {
        if (modalPanel == null) yield break;
        
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one;
        float elapsed = 0f;
        
        modalPanel.transform.localScale = startScale;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / animationDuration;
            float easedTime = Mathf.SmoothStep(0f, 1f, normalizedTime);
            
            modalPanel.transform.localScale = Vector3.Lerp(startScale, endScale, easedTime);
            
            yield return null;
        }
        
        modalPanel.transform.localScale = endScale;
        animationCoroutine = null;
    }
    
    IEnumerator AnimateModalOut()
    {
        if (modalPanel == null) yield break;
        
        Vector3 startScale = Vector3.one;
        Vector3 endScale = Vector3.zero;
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / animationDuration;
            float easedTime = Mathf.SmoothStep(0f, 1f, normalizedTime);
            
            modalPanel.transform.localScale = Vector3.Lerp(startScale, endScale, easedTime);
            
            yield return null;
        }
        
        modalPanel.transform.localScale = endScale;
        modalPanel.SetActive(false);
        isVisible = false;
        animationCoroutine = null;
    }
    
    // Public method to manually show the introduction
    public void ShowIntroduction()
    {
        ShowModal();
    }
    
    // Public method to reset the introduction flag (for testing)
    public void ResetIntroductionFlag()
    {
        PlayerPrefs.DeleteKey("HellLevelIntroSeen");
        PlayerPrefs.Save();
        Debug.Log("HellLevelIntroModal: Introduction flag reset");
    }
    
    void OnDestroy()
    {
        // Clean up button listeners
        if (beginButton != null)
        {
            beginButton.onClick.RemoveAllListeners();
        }
        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveAllListeners();
        }
    }
}