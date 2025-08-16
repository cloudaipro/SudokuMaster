using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WaitingCursor : MonoBehaviour
{
    public static WaitingCursor Instance;
    public Image cursorImage;

    private void Awake()
    {
        Instance = this;
    }

    public void ChangeWaittingState(bool bWaitting)
    {
        if (bWaitting)
            ShowCursor();
        else
            HideCursor();
    }

    public void ShowCursor()
    {
        gameObject.SetActive(true);
        cursorImage.enabled = true;
        StartCoroutine(RotateCursor());
        DisableUserInput();
    }

    public void HideCursor()
    {
        cursorImage.enabled = false;
        EnableUserInput();
        gameObject.SetActive(false);
    }

    private IEnumerator RotateCursor()
    {
        while (true)
        {
            cursorImage.rectTransform.Rotate(0, 0, -90);
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void DisableUserInput()
    {
        // Disable user input here, for example:
         EventSystem.current.SetSelectedGameObject(null);
         //InputManager.Instance.DisableInput();
    }

    private void EnableUserInput()
    {
        // Enable user input here, for example:
        //InputManager.Instance.EnableInput();
    }
}