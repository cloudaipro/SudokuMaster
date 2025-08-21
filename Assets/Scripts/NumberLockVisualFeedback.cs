using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NumberLockVisualFeedback : MonoBehaviour
{
    [Header("Visual Feedback Settings")]
    public float holdDuration = 1.0f;
    public float progressBarShowDelay = 0.2f;
    
    [Header("Canvas References")]
    public Canvas feedbackCanvas;
    
    private GameObject currentOverlay;
    private GameObject currentProgressBar;
    private Image progressBarFill;
    private Coroutine feedbackCoroutine;
    private bool isShowingFeedback = false;
    
    private void Awake()
    {
        if (feedbackCanvas == null)
        {
            feedbackCanvas = FindObjectOfType<Canvas>();
        }
    }
    
    public void StartFeedback(Vector3 worldPosition, int number)
    {
        if (isShowingFeedback)
        {
            StopFeedback();
        }
        
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
        Vector2 canvasPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            feedbackCanvas.transform as RectTransform,
            screenPosition,
            feedbackCanvas.worldCamera,
            out canvasPosition
        );
        
        feedbackCoroutine = StartCoroutine(FeedbackSequence(canvasPosition, number));
    }
    
    public void StopFeedback()
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
            feedbackCoroutine = null;
        }
        
        HideAllFeedback();
        isShowingFeedback = false;
    }
    
    private IEnumerator FeedbackSequence(Vector2 position, int number)
    {
        isShowingFeedback = true;
        
        ShowEnlargedOverlay(position, number);
        
        yield return new WaitForSeconds(progressBarShowDelay);
        
        if (!isShowingFeedback) yield break;
        
        ShowProgressBar(position);
        
        float fillDuration = holdDuration - progressBarShowDelay;
        float elapsedTime = 0f;
        
        while (elapsedTime < fillDuration && isShowingFeedback)
        {
            float progress = elapsedTime / fillDuration;
            if (progressBarFill != null)
            {
                progressBarFill.fillAmount = progress;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        if (progressBarFill != null && isShowingFeedback)
        {
            progressBarFill.fillAmount = 1f;
        }
    }
    
    private void ShowEnlargedOverlay(Vector2 position, int number)
    {
        GameObject overlay = new GameObject("NumberOverlay");
        overlay.transform.SetParent(feedbackCanvas.transform, false);
        
        Image backgroundImage = overlay.AddComponent<Image>();
        backgroundImage.color = new Color(1f, 1f, 1f, 0.9f);
        
        RectTransform rectTransform = overlay.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(240f, 240f);
        rectTransform.anchoredPosition = position + new Vector2(0, 120f);
        
        GameObject textObj = new GameObject("NumberText");
        textObj.transform.SetParent(overlay.transform, false);
        
        Text numberText = textObj.AddComponent<Text>();
        numberText.text = number.ToString();
        numberText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        numberText.fontSize = 80;
        numberText.color = Color.black;
        numberText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        currentOverlay = overlay;
        
        overlay.transform.localScale = Vector3.zero;
        overlay.SetActive(true);
        
        StartCoroutine(AnimateScale(overlay.transform, Vector3.one, 0.1f));
    }
    
    private void ShowProgressBar(Vector2 position)
    {
        GameObject progressBar = new GameObject("ProgressBar");
        progressBar.transform.SetParent(feedbackCanvas.transform, false);
        
        Image backgroundImage = progressBar.AddComponent<Image>();
        backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        RectTransform rectTransform = progressBar.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(100f, 8f);
        rectTransform.anchoredPosition = position + new Vector2(0, 180f);
        
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(progressBar.transform, false);
        
        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = new Color(0.3f, 0.6f, 1f, 1f);
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = 0;
        fillImage.fillAmount = 0f;
        
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        
        currentProgressBar = progressBar;
        progressBarFill = fillImage;
        
        progressBar.SetActive(true);
    }
    
    private IEnumerator AnimateScale(Transform target, Vector3 targetScale, float duration)
    {
        Vector3 startScale = target.localScale;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;
            target.localScale = Vector3.Lerp(startScale, targetScale, progress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        target.localScale = targetScale;
    }
    
    private void HideAllFeedback()
    {
        if (currentOverlay != null)
        {
            Destroy(currentOverlay);
            currentOverlay = null;
        }
        
        if (currentProgressBar != null)
        {
            Destroy(currentProgressBar);
            currentProgressBar = null;
        }
        
        progressBarFill = null;
    }
    
    public bool IsShowingFeedback()
    {
        return isShowingFeedback;
    }
}