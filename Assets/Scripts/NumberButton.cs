using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
public class NumberButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{

    [System.Serializable]
    public class CustomUIEvent : UnityEvent { }
    public CustomUIEvent OnEvent;
    public Image backgroundGraphic;

    public bool buttonEnabled = true;
    
    [Header("Number Lock Settings")]
    public float holdDuration = 0.4f;

    public Color defaultColor = Color.white;
    public Color hoverColor = Color.white;
    public Color pressedColor = Color.white;
    public Color disabledColor = Color.gray;

    public Vector3 defaultScale = Vector3.one;
    public Vector3 hoverScale = Vector3.one;
    public Vector3 pressedScale = Vector3.one;

    public int value = 0;
    public int sub_value = 0;
    [SerializeField] public GameObject number_text;
    [SerializeField] GameObject sub_text;
    
    // Number Lock functionality
    private bool isHolding = false;
    private bool holdCompleted = false;
    private Coroutine holdCoroutine;
    private NumberLockManager lockManager;
    private NumberLockVisualFeedback visualFeedback;

    private void Awake()
    {
        backgroundGraphic.color = (buttonEnabled) ? defaultColor : disabledColor;
        transform.localScale = defaultScale;
        
        number_text.GetComponent<Text>()?.Also(x => x.text = value.ToString());
        sub_text.GetComponent<Text>()?.Also(x => x.text = sub_value.ToString());
        
        // Find manager references
        lockManager = FindObjectOfType<NumberLockManager>();
        visualFeedback = FindObjectOfType<NumberLockVisualFeedback>();
    }

    private void OnEnable()
    {
        GameEvents.OnNumberUsed += OnNumberUsed;
    }
    private void OnDisable()
    {
        GameEvents.OnNumberUsed -= OnNumberUsed;
    }

    private void OnNumberUsed(int number)
    {
        if (number != value || sub_value <= 0)
            return;
        SetSubText(sub_value - 1);
    }

    public void SetSubText(int newSubValue)
    {
        sub_value = newSubValue;
        sub_text.GetComponent<Text>()?.Also(x => x.text = sub_value.ToString());

        if (sub_value <= 0)
            gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!buttonEnabled) return;

        StartCoroutine(Transition(hoverScale, hoverColor, 0.25f));
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        if (!buttonEnabled) return;

        StartCoroutine(Transition(defaultScale, defaultColor, 0.25f));
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        if (!buttonEnabled) return;

        StartCoroutine(Transition(pressedScale, pressedColor, 0.25f));
        
        // Start hold detection for number lock
        isHolding = true;
        holdCompleted = false;
        
        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
        }
        
        holdCoroutine = StartCoroutine(HoldDetection());
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!buttonEnabled) return;

        // Only process regular click if hold was not completed
        if (!holdCompleted)
        {
            StartCoroutine(Transition(hoverScale, hoverColor, 0.25f));
            
            // Check if this number is already locked and toggle if so
            if (lockManager != null && lockManager.IsNumberLocked(value))
            {
                lockManager.UnlockNumber();
            }
            else
            {
                GameEvents.UpdateSquareNumberMethod(value);
            }
        }
        
        // Reset hold state
        holdCompleted = false;
    }
    
    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        if (!buttonEnabled) return;
        
        // Stop holding
        isHolding = false;
        
        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
        }
        
        // Stop visual feedback
        if (visualFeedback != null)
        {
            visualFeedback.StopFeedback();
        }
        
        // If hold was completed, activate the lock
        if (holdCompleted && lockManager != null)
        {
            lockManager.LockNumber(value, this);
        }
    }
    
    private IEnumerator HoldDetection()
    {
        // Start visual feedback immediately
        if (visualFeedback != null)
        {
            visualFeedback.StartFeedback(transform.position, value);
        }
        
        // Wait for hold duration
        float elapsedTime = 0f;
        
        while (elapsedTime < holdDuration && isHolding)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // If we completed the hold duration and still holding
        if (isHolding && elapsedTime >= holdDuration)
        {
            holdCompleted = true;
        }
    }


    public IEnumerator Transition(Vector3 newSize, Color newColor, float transitionTime)
    {
        float timer = 0;
        Vector3 startSize = transform.localScale;
        Color startColor = backgroundGraphic.color;

        while (timer < transitionTime)
        {
            timer += Time.deltaTime;

            yield return null;

            transform.localScale = Vector3.Lerp(startSize, newSize, timer / transitionTime);
            backgroundGraphic.color = Color.Lerp(startColor, newColor, timer / transitionTime);
        }
    }

}

