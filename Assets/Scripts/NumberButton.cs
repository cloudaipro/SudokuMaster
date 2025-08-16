using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
public class NumberButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{

    [System.Serializable]
    public class CustomUIEvent : UnityEvent { }
    public CustomUIEvent OnEvent;
    public Image backgroundGraphic;

    public bool buttonEnabled = true;

    public Color defaultColor = Color.white;
    public Color hoverColor = Color.white;
    public Color pressedColor = Color.white;
    public Color disabledColor = Color.gray;

    public Vector3 defaultScale = Vector3.one;
    public Vector3 hoverScale = Vector3.one;
    public Vector3 pressedScale = Vector3.one;

    public int value = 0;
    public int sub_value = 0;
    [SerializeField] GameObject number_text;
    [SerializeField] GameObject sub_text;

    private void Awake()
    {
        backgroundGraphic.color = (buttonEnabled) ? defaultColor : disabledColor;
        transform.localScale = defaultScale;
        
        number_text.GetComponent<Text>()?.Also(x => x.text = value.ToString());
        sub_text.GetComponent<Text>()?.Also(x => x.text = sub_value.ToString());
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
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!buttonEnabled) return;

        StartCoroutine(Transition(hoverScale, hoverColor, 0.25f));
        GameEvents.UpdateSquareNumberMethod(value);
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

