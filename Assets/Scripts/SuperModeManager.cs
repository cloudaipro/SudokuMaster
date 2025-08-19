using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SuperModeManager : MonoBehaviour
{
    public static SuperModeManager Instance;
    
    [Header("Super Mode Settings")]
    [SerializeField] private float longPressThreshold = 5f;
    [SerializeField] private Color goldColor = new Color(1f, 0.843f, 0f, 1f);
    [SerializeField] private AudioSource winAudioSource;
    
    [Header("Debug")]
    [SerializeField] private bool isSuperModeActive = false;
    
    private Coroutine longPressCoroutine;
    private bool isHolding = false;
    
    public bool IsSuperModeActive => isSuperModeActive;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    private void Start()
    {
        if (winAudioSource == null)
        {
            GameWon gameWon = FindObjectOfType<GameWon>();
            if (gameWon != null)
            {
                winAudioSource = gameWon.GetComponent<AudioSource>();
                if (winAudioSource == null)
                {
                    winAudioSource = gameWon.GetComponentInChildren<AudioSource>();
                }
            }
        }
        
        SetupXMarkerInteractions();
    }
    
    private void SetupXMarkerInteractions()
    {
        if (Lives.Instance != null)
        {
            // Setup interactions for normal X images
            if (Lives.Instance.normal_x_images != null)
            {
                foreach (GameObject normalXImage in Lives.Instance.normal_x_images)
                {
                    SetupEventTrigger(normalXImage);
                }
            }
            
            // Setup interactions for error images
            if (Lives.Instance.error_images != null)
            {
                foreach (GameObject errorImage in Lives.Instance.error_images)
                {
                    SetupEventTrigger(errorImage);
                }
            }
        }
    }
    
    private void SetupEventTrigger(GameObject targetObject)
    {
        if (targetObject == null) return;
        
        EventTrigger eventTrigger = targetObject.GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = targetObject.AddComponent<EventTrigger>();
        }
        
        eventTrigger.triggers.Clear();
        
        EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry();
        pointerDownEntry.eventID = EventTriggerType.PointerDown;
        pointerDownEntry.callback.AddListener((data) => { OnPointerDown(targetObject); });
        eventTrigger.triggers.Add(pointerDownEntry);
        
        EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry();
        pointerUpEntry.eventID = EventTriggerType.PointerUp;
        pointerUpEntry.callback.AddListener((data) => { OnPointerUp(); });
        eventTrigger.triggers.Add(pointerUpEntry);
        
        EventTrigger.Entry pointerExitEntry = new EventTrigger.Entry();
        pointerExitEntry.eventID = EventTriggerType.PointerExit;
        pointerExitEntry.callback.AddListener((data) => { OnPointerUp(); });
        eventTrigger.triggers.Add(pointerExitEntry);
    }
    
    private void OnPointerDown(GameObject xImage)
    {
        if (isSuperModeActive || isHolding) return;
        
        isHolding = true;
        longPressCoroutine = StartCoroutine(LongPressCoroutine());
    }
    
    private void OnPointerUp()
    {
        isHolding = false;
        
        if (longPressCoroutine != null)
        {
            StopCoroutine(longPressCoroutine);
            longPressCoroutine = null;
        }
    }
    
    private IEnumerator LongPressCoroutine()
    {
        float elapsedTime = 0f;
        
        while (isHolding && elapsedTime < longPressThreshold)
        {
            Debug.Log(elapsedTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        if (isHolding && elapsedTime >= longPressThreshold)
        {
            ActivateSuperMode();
        }
        
        longPressCoroutine = null;
    }
    
    public void ActivateSuperMode()
    {
        if (isSuperModeActive) return;
        
        isSuperModeActive = true;
        
        PlayActivationSound();
        ApplyGoldColorToXMarkers();
        
        Debug.Log("Super Mode Activated! Unlimited lives enabled.");
    }
    
    private void PlayActivationSound()
    {
        if (winAudioSource != null)
        {
            winAudioSource.Play();
        }
        else
        {
            Debug.LogWarning("Win audio source not found for Super Mode activation sound");
        }
    }
    
    private void ApplyGoldColorToXMarkers()
    {
        if (Lives.Instance != null)
        {
            // Apply gold color to normal X images
            if (Lives.Instance.normal_x_images != null)
            {
                foreach (GameObject normalXImage in Lives.Instance.normal_x_images)
                {
                    Image imageComponent = normalXImage.GetComponent<Image>();
                    if (imageComponent != null)
                    {
                        imageComponent.color = goldColor;
                    }
                    normalXImage.SetActive(false);
                }
            }
            
            // Apply gold color to error images
            if (Lives.Instance.error_images != null)
            {
                foreach (GameObject errorImage in Lives.Instance.error_images)
                {
                    Image imageComponent = errorImage.GetComponent<Image>();
                    if (imageComponent != null)
                    {
                        imageComponent.color = goldColor;
                    }
                    errorImage.SetActive(false);
                }
            }
        }
    }
    
    public void DeactivateSuperMode()
    {
        isSuperModeActive = false;
        ResetXMarkersColor();
        Debug.Log("Super Mode Deactivated");
    }
    
    private void ResetXMarkersColor()
    {
        if (Lives.Instance != null)
        {
            // Reset normal X images to default color
            if (Lives.Instance.normal_x_images != null)
            {
                foreach (GameObject normalXImage in Lives.Instance.normal_x_images)
                {
                    Image imageComponent = normalXImage.GetComponent<Image>();
                    if (imageComponent != null)
                    {
                        imageComponent.color = Color.white;
                    }
                    normalXImage.SetActive(true);
                }
            }
            
            // Reset error images to default color
            if (Lives.Instance.error_images != null)
            {
                foreach (GameObject errorImage in Lives.Instance.error_images)
                {
                    Image imageComponent = errorImage.GetComponent<Image>();
                    if (imageComponent != null)
                    {
                        imageComponent.color = Color.white;
                    }
                    errorImage.SetActive(true);
                }
            }
        }
    }
    
    public void ResetForNewGame()
    {
        DeactivateSuperMode();
    }
    
    private void OnDestroy()
    {
        if (longPressCoroutine != null)
        {
            StopCoroutine(longPressCoroutine);
        }
    }
}