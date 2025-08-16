using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToastMessage : MonoBehaviour
{
    void Start()
    {
        //showToast("Hello", 2);
    }
    public Image background;
    public Text txt;

    public void showToast(string text, int duration)
    {
        StartCoroutine(showToastCOR(text, duration));
    }

    public void showToast(int duration)
    {
        gameObject.SetActive(true);
        StartCoroutine(showToastCOR(null, duration));
    }


    private IEnumerator showToastCOR(string text, int duration)
    {
        
        Color orginalColor = background.color;

        if (text != null)
        {
            txt.text = text;
            //txt.enabled = true;
        }

        //Fade in
        yield return fadeInAndOut(background, true, 0.5f);

        //Wait for the duration
        float counter = 0;
        while (counter < duration)
        {
            counter += Time.deltaTime;
            yield return null;
        }

        //Fade out
        yield return fadeInAndOut(background, false, 0.5f);

        //txt.enabled = false;
        background.color = orginalColor;

        gameObject.SetActive(false);
    }

    IEnumerator fadeInAndOut(Image targetBackground, bool fadeIn, float duration)
    {
        //Set Values depending on if fadeIn or fadeOut
        float a, b;
        if (fadeIn)
        {
            a = 0f;
            b = 0.75f;
        }
        else
        {
            a = 0.75f;
            b = 0f;
        }

        Color currentColor = Color.clear;
        float counter = 0f;

        while (counter < duration)
        {
            counter += Time.deltaTime;
            float alpha = Mathf.Lerp(a, b, counter / duration);

            targetBackground.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
            yield return null;
        }
    }
}
