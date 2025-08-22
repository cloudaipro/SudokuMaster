using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class HellLevelTutorial : MonoBehaviour
{
    [Header("Hell Level Tutorial")]
    [SerializeField] private GameObject tutorialOverlay;
    [SerializeField] private Text tutorialText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private GameObject highlightCircle;
    [SerializeField] private Image tutorialBackground;
    
    [Header("Visual Design")]
    [SerializeField] private Color overlayColor = new Color(0f, 0f, 0f, 0.8f);
    [SerializeField] private Color textBackgroundColor = new Color(0.05f, 0.02f, 0.02f, 0.95f);
    [SerializeField] private Color textColor = new Color(1f, 0.85f, 0.7f, 1f);
    [SerializeField] private Color highlightColor = new Color(1f, 0.4f, 0.1f, 0.8f);
    [SerializeField] private Color buttonColor = new Color(0.9f, 0.4f, 0f, 1f);
    
    [Header("Animation")]
    [SerializeField] private float highlightPulseDuration = 1.5f;
    [SerializeField] private float tutorialStepDelay = 0.5f;
    
    private SudokuBoard sudokuBoard;
    private ValidationContext validationContext;
    private ManualValidationButton validationButton;
    private HellLevelModeIndicator modeIndicator;
    private SolutionProgressIndicator progressIndicator;
    
    private int currentStep = 0;
    private bool isTutorialActive = false;
    private bool tutorialCompleted = false;
    private Coroutine highlightCoroutine;
    
    // Tutorial steps data
    private List<TutorialStep> tutorialSteps;
    
    [System.Serializable]
    public class TutorialStep
    {
        public string title;
        public string description;
        public Vector2 highlightPosition;
        public Vector2 highlightSize;
        public bool showHighlight;
        public string targetComponent; // For automatic highlighting
    }
    
    void Start()
    {
        // Find required components
        sudokuBoard = FindObjectOfType<SudokuBoard>();
        validationButton = FindObjectOfType<ManualValidationButton>();
        modeIndicator = FindObjectOfType<HellLevelModeIndicator>();
        progressIndicator = FindObjectOfType<SolutionProgressIndicator>();
        
        if (sudokuBoard != null)
        {
            validationContext = sudokuBoard.GetValidationContext();
        }
        
        // Initialize tutorial system
        InitializeTutorial();
        
        // Check if tutorial should be shown
        CheckTutorialState();
    }
    
    void InitializeTutorial()
    {
        // Create tutorial UI if it doesn't exist
        if (tutorialOverlay == null)
        {
            CreateTutorialUI();
        }
        
        // Define tutorial steps
        SetupTutorialSteps();
        
        // Initially hide tutorial
        if (tutorialOverlay != null)
        {
            tutorialOverlay.SetActive(false);
        }
    }
    
    void CreateTutorialUI()
    {
        // Create tutorial overlay
        GameObject overlayObj = new GameObject("HellTutorialOverlay");
        overlayObj.transform.SetParent(transform, false);
        
        RectTransform overlayRect = overlayObj.AddComponent<RectTransform>();
        tutorialBackground = overlayObj.AddComponent<Image>();
        
        // Configure full-screen overlay
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        
        tutorialBackground.color = overlayColor;
        tutorialBackground.raycastTarget = true;
        
        tutorialOverlay = overlayObj;
        
        // Create highlight circle
        GameObject highlightObj = new GameObject("TutorialHighlight");
        highlightObj.transform.SetParent(overlayObj.transform, false);
        
        RectTransform highlightRect = highlightObj.AddComponent<RectTransform>();
        Image highlightImage = highlightObj.AddComponent<Image>();
        
        highlightRect.anchorMin = new Vector2(0.5f, 0.5f);
        highlightRect.anchorMax = new Vector2(0.5f, 0.5f);
        highlightRect.pivot = new Vector2(0.5f, 0.5f);
        highlightRect.sizeDelta = new Vector2(100, 100);
        
        // Create circle sprite for highlight
        highlightImage.sprite = CreateCircleSprite();
        highlightImage.color = highlightColor;
        
        highlightCircle = highlightObj;
        
        // Create text panel
        GameObject textPanelObj = new GameObject("TutorialTextPanel");
        textPanelObj.transform.SetParent(overlayObj.transform, false);
        
        RectTransform textPanelRect = textPanelObj.AddComponent<RectTransform>();
        Image textPanelBg = textPanelObj.AddComponent<Image>();
        
        textPanelRect.anchorMin = new Vector2(0.1f, 0.1f);
        textPanelRect.anchorMax = new Vector2(0.9f, 0.4f);
        textPanelRect.offsetMin = Vector2.zero;
        textPanelRect.offsetMax = Vector2.zero;
        
        textPanelBg.color = textBackgroundColor;
        
        // Create tutorial text
        GameObject textObj = new GameObject("TutorialText");
        textObj.transform.SetParent(textPanelObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        tutorialText = textObj.AddComponent<Text>();
        
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(20, 60);
        textRect.offsetMax = new Vector2(-20, -20);
        
        tutorialText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tutorialText.fontSize = 18;
        tutorialText.color = textColor;
        tutorialText.alignment = TextAnchor.UpperLeft;
        tutorialText.text = "Tutorial text will appear here";
        
        // Create buttons
        CreateTutorialButtons(textPanelObj);
        
        Debug.Log("HellLevelTutorial: UI created programmatically");
    }
    
    void CreateTutorialButtons(GameObject parent)
    {
        // Next Button
        GameObject nextButtonObj = new GameObject("NextButton");
        nextButtonObj.transform.SetParent(parent.transform, false);
        
        RectTransform nextRect = nextButtonObj.AddComponent<RectTransform>();
        nextButton = nextButtonObj.AddComponent<Button>();
        Image nextBg = nextButtonObj.AddComponent<Image>();
        
        nextRect.anchorMin = new Vector2(1f, 0f);
        nextRect.anchorMax = new Vector2(1f, 0f);
        nextRect.pivot = new Vector2(1f, 0f);
        nextRect.anchoredPosition = new Vector2(-10, 10);
        nextRect.sizeDelta = new Vector2(120, 40);
        
        nextBg.color = buttonColor;
        nextButton.targetGraphic = nextBg;
        nextButton.onClick.AddListener(NextStep);
        
        // Next button text
        GameObject nextTextObj = new GameObject("NextButtonText");
        nextTextObj.transform.SetParent(nextButtonObj.transform, false);
        
        RectTransform nextTextRect = nextTextObj.AddComponent<RectTransform>();
        Text nextText = nextTextObj.AddComponent<Text>();
        
        nextTextRect.anchorMin = Vector2.zero;
        nextTextRect.anchorMax = Vector2.one;
        nextTextRect.offsetMin = Vector2.zero;
        nextTextRect.offsetMax = Vector2.zero;
        
        nextText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        nextText.fontSize = 16;
        nextText.fontStyle = FontStyle.Bold;
        nextText.color = Color.white;
        nextText.alignment = TextAnchor.MiddleCenter;
        nextText.text = "Next";
        
        // Skip Button
        GameObject skipButtonObj = new GameObject("SkipButton");
        skipButtonObj.transform.SetParent(parent.transform, false);
        
        RectTransform skipRect = skipButtonObj.AddComponent<RectTransform>();
        skipButton = skipButtonObj.AddComponent<Button>();
        Image skipBg = skipButtonObj.AddComponent<Image>();
        
        skipRect.anchorMin = new Vector2(0f, 0f);
        skipRect.anchorMax = new Vector2(0f, 0f);
        skipRect.pivot = new Vector2(0f, 0f);
        skipRect.anchoredPosition = new Vector2(10, 10);
        skipRect.sizeDelta = new Vector2(100, 40);
        
        skipBg.color = new Color(0.4f, 0.4f, 0.4f, 1f);
        skipButton.targetGraphic = skipBg;
        skipButton.onClick.AddListener(SkipTutorial);
        
        // Skip button text
        GameObject skipTextObj = new GameObject("SkipButtonText");
        skipTextObj.transform.SetParent(skipButtonObj.transform, false);
        
        RectTransform skipTextRect = skipTextObj.AddComponent<RectTransform>();
        Text skipText = skipTextObj.AddComponent<Text>();
        
        skipTextRect.anchorMin = Vector2.zero;
        skipTextRect.anchorMax = Vector2.one;
        skipTextRect.offsetMin = Vector2.zero;
        skipTextRect.offsetMax = Vector2.zero;
        
        skipText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        skipText.fontSize = 14;
        skipText.color = Color.white;
        skipText.alignment = TextAnchor.MiddleCenter;
        skipText.text = "Skip Tutorial";
    }
    
    Sprite CreateCircleSprite()
    {
        int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = Vector2.one * (size / 2f);
        float radius = size / 2f - 2f;
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, center);
                
                if (distance <= radius && distance >= radius - 4f)
                {
                    float alpha = 1f - Mathf.Abs(distance - (radius - 2f)) / 2f;
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
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
    
    void SetupTutorialSteps()
    {
        tutorialSteps = new List<TutorialStep>
        {
            new TutorialStep
            {
                title = "🔥 Welcome to Hell Level!",
                description = "This is the ultimate Sudoku challenge. In Hell Level, you work with HYPOTHESIS NUMBERS that aren't validated until you choose to check them.\n\nLook for the Hell Level indicator at the top of the screen.",
                highlightPosition = new Vector2(0, -60),
                highlightSize = new Vector2(300, 60),
                showHighlight = true,
                targetComponent = "HellLevelModeIndicator"
            },
            new TutorialStep
            {
                title = "📊 Progress Tracking",
                description = "The circular indicator shows how many cells you've filled. It updates in real-time as you place numbers.\n\nKeep track of your progress toward completing all 81 cells.",
                highlightPosition = new Vector2(-30, -60),
                highlightSize = new Vector2(100, 100),
                showHighlight = true,
                targetComponent = "SolutionProgressIndicator"
            },
            new TutorialStep
            {
                title = "🧪 Hypothesis Numbers",
                description = "When you place a number in Hell Level, it appears in ORANGE color. These are hypothesis numbers - they're not confirmed as correct yet.\n\nTry placing a number in any empty cell to see this effect.",
                highlightPosition = Vector2.zero,
                highlightSize = new Vector2(400, 400),
                showHighlight = true,
                targetComponent = "SudokuBoard"
            },
            new TutorialStep
            {
                title = "🔥 Manual Validation",
                description = "When you're ready to check your work, use the VALIDATE button. This will analyze all your hypothesis numbers and give you detailed feedback.\n\nThe button only becomes active when you've made changes.",
                highlightPosition = new Vector2(0, 150),
                highlightSize = new Vector2(320, 70),
                showHighlight = true,
                targetComponent = "ManualValidationButton"
            },
            new TutorialStep
            {
                title = "📝 Comprehensive Feedback",
                description = "After validation, you'll see:\n• Completion percentage\n• Which cells have errors\n• Option to continue or reset incorrect numbers\n\nThis helps you learn from mistakes and improve your solving strategy.",
                highlightPosition = Vector2.zero,
                highlightSize = new Vector2(500, 400),
                showHighlight = false,
                targetComponent = ""
            },
            new TutorialStep
            {
                title = "🚫 No Assistance Mode",
                description = "In Hell Level:\n• Hints are disabled\n• Fast Notes won't help\n• No immediate error checking\n\nYou must rely on pure logical deduction and advanced Sudoku techniques.",
                highlightPosition = Vector2.zero,
                highlightSize = Vector2.zero,
                showHighlight = false,
                targetComponent = ""
            },
            new TutorialStep
            {
                title = "🎯 Ready to Begin!",
                description = "You're now ready to face the ultimate Sudoku challenge!\n\nRemember:\n• Place numbers freely (they'll be orange)\n• Use the VALIDATE button when ready\n• Think carefully - every move counts\n\nGood luck, master solver!",
                highlightPosition = Vector2.zero,
                highlightSize = Vector2.zero,
                showHighlight = false,
                targetComponent = ""
            }
        };
    }
    
    void CheckTutorialState()
    {
        // Check if tutorial should be shown automatically
        bool hasSeenTutorial = PlayerPrefs.GetInt("HellLevelTutorialSeen", 0) == 1;
        bool isHellLevel = sudokuBoard != null && sudokuBoard.IsHellLevel();
        
        if (!hasSeenTutorial && isHellLevel)
        {
            // Show tutorial after a brief delay
            StartCoroutine(StartTutorialAfterDelay(2f));
        }
    }
    
    IEnumerator StartTutorialAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartTutorial();
    }
    
    public void StartTutorial()
    {
        if (!isTutorialActive && !tutorialCompleted)
        {
            isTutorialActive = true;
            currentStep = 0;
            
            if (tutorialOverlay != null)
            {
                tutorialOverlay.SetActive(true);
            }
            
            ShowCurrentStep();
            Debug.Log("HellLevelTutorial: Tutorial started");
        }
    }
    
    void ShowCurrentStep()
    {
        if (currentStep >= 0 && currentStep < tutorialSteps.Count)
        {
            var step = tutorialSteps[currentStep];
            
            // Update tutorial text
            if (tutorialText != null)
            {
                tutorialText.text = $"<b>{step.title}</b>\n\n{step.description}";
            }
            
            // Update highlight
            if (step.showHighlight && highlightCircle != null)
            {
                highlightCircle.SetActive(true);
                PositionHighlight(step);
                StartHighlightAnimation();
            }
            else if (highlightCircle != null)
            {
                highlightCircle.SetActive(false);
            }
            
            // Update button text
            if (nextButton != null)
            {
                var buttonText = nextButton.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    buttonText.text = (currentStep == tutorialSteps.Count - 1) ? "Finish" : "Next";
                }
            }
        }
    }
    
    void PositionHighlight(TutorialStep step)
    {
        if (highlightCircle == null) return;
        
        RectTransform highlightRect = highlightCircle.GetComponent<RectTransform>();
        
        // Position based on target component or manual position
        Vector2 position = step.highlightPosition;
        Vector2 size = step.highlightSize.magnitude > 0 ? step.highlightSize : new Vector2(120, 120);
        
        // Automatic positioning based on component
        if (!string.IsNullOrEmpty(step.targetComponent))
        {
            GameObject target = FindTargetComponent(step.targetComponent);
            if (target != null)
            {
                RectTransform targetRect = target.GetComponent<RectTransform>();
                if (targetRect != null)
                {
                    position = targetRect.anchoredPosition;
                    if (step.highlightSize.magnitude == 0)
                    {
                        size = targetRect.sizeDelta + new Vector2(20, 20); // Add padding
                    }
                }
            }
        }
        
        highlightRect.anchoredPosition = position;
        highlightRect.sizeDelta = size;
    }
    
    GameObject FindTargetComponent(string componentName)
    {
        switch (componentName)
        {
            case "HellLevelModeIndicator":
                return modeIndicator?.gameObject;
            case "SolutionProgressIndicator":
                return progressIndicator?.gameObject;
            case "ManualValidationButton":
                return validationButton?.gameObject;
            case "SudokuBoard":
                return sudokuBoard?.gameObject;
            default:
                return GameObject.Find(componentName);
        }
    }
    
    void StartHighlightAnimation()
    {
        if (highlightCoroutine != null)
        {
            StopCoroutine(highlightCoroutine);
        }
        highlightCoroutine = StartCoroutine(AnimateHighlight());
    }
    
    IEnumerator AnimateHighlight()
    {
        if (highlightCircle == null) yield break;
        
        Vector3 baseScale = Vector3.one;
        
        while (highlightCircle.activeInHierarchy)
        {
            float elapsed = 0f;
            
            while (elapsed < highlightPulseDuration)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = elapsed / highlightPulseDuration;
                float scale = 1f + 0.2f * Mathf.Sin(normalizedTime * Mathf.PI * 2f);
                
                highlightCircle.transform.localScale = baseScale * scale;
                
                yield return null;
            }
            
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    public void NextStep()
    {
        currentStep++;
        
        if (currentStep >= tutorialSteps.Count)
        {
            FinishTutorial();
        }
        else
        {
            StartCoroutine(ShowNextStepAfterDelay());
        }
    }
    
    IEnumerator ShowNextStepAfterDelay()
    {
        yield return new WaitForSeconds(tutorialStepDelay);
        ShowCurrentStep();
    }
    
    public void SkipTutorial()
    {
        FinishTutorial();
    }
    
    void FinishTutorial()
    {
        isTutorialActive = false;
        tutorialCompleted = true;
        
        // Mark tutorial as seen
        PlayerPrefs.SetInt("HellLevelTutorialSeen", 1);
        PlayerPrefs.Save();
        
        // Hide tutorial overlay
        if (tutorialOverlay != null)
        {
            tutorialOverlay.SetActive(false);
        }
        
        // Stop highlight animation
        if (highlightCoroutine != null)
        {
            StopCoroutine(highlightCoroutine);
            highlightCoroutine = null;
        }
        
        Debug.Log("HellLevelTutorial: Tutorial completed");
    }
    
    // Public methods for external control
    public void ResetTutorial()
    {
        PlayerPrefs.DeleteKey("HellLevelTutorialSeen");
        PlayerPrefs.Save();
        tutorialCompleted = false;
        Debug.Log("HellLevelTutorial: Tutorial reset");
    }
    
    public bool IsTutorialActive()
    {
        return isTutorialActive;
    }
    
    void OnDestroy()
    {
        // Clean up button listeners
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
        }
        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
        }
        
        // Stop coroutines
        if (highlightCoroutine != null)
        {
            StopCoroutine(highlightCoroutine);
        }
    }
}