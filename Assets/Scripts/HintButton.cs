using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Purchasing;

public class HintButton : Selectable, IPointerClickHandler
{
    public GameObject hintBadge;
    public GameObject textBadge;
    public GameObject playAd;
    public GameObject moreHintDialog;

    protected override void Start()
    {
        OnUpdateHintBadge();
    }

    protected override void OnEnable()
    {
        GameEvents.OnDidFinishHintAd += OnDidFinishHintAd;
        GameEvents.OnUpdateHintBadge += OnUpdateHintBadge;
    }
    protected override void OnDisable()
    {
        GameEvents.OnDidFinishHintAd -= OnDidFinishHintAd;
        GameEvents.OnUpdateHintBadge -= OnUpdateHintBadge;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Check if this is Hell Level - hints are disabled
        var sudokuBoard = FindObjectOfType<SudokuBoard>();
        if (sudokuBoard != null && sudokuBoard.IsHellLevel())
        {
            Debug.Log("Hints are disabled in Hell Level");
            return;
        }
        
        if (Setting.Instance.RemainHints <= 0)
            //AdManager.Instance.ShowHintRewardAd();
            moreHintDialog.SetActive(true);
        else
            DoHint();
    }

    private void OnDidFinishHintAd()
    {
        GameSettings.Instance.Pause = false;
        Setting.Instance.UpdateFreeHints(2);

        DoHint();
    }

    //public void BuyHintsSuccess(Product product)
    //{
    //    GameSettings.Instance.Pause = false;
    //    Debug.Log(product);
    //    Setting.Instance.UpdateIAPHints( Setting.Instance.IAPHints + (int)product.definition.payout.quantity);
    //}

    //public void BuyHintsFail(Product product, PurchaseFailureReason reason)
    //{
    //    GameSettings.Instance.Pause = false;
    //    Debug.Log(product);
    //}

    private void DoHint()
    {
        Setting.Instance.UseHint();
        
        Dispatcher.RunOnMainThread(() => OnUpdateHintBadge());
        Dispatcher.RunOnMainThread(() => GameEvents.GiveAHintMethod());
    }
    private void OnUpdateHintBadge()
    {
        // Check if this is Hell Level - show disabled state
        var sudokuBoard = FindObjectOfType<SudokuBoard>();
        bool isHellLevel = sudokuBoard != null && sudokuBoard.IsHellLevel();
        
        if (isHellLevel)
        {
            // Update text badge safely
            if (textBadge != null)
            {
                var textComponent = textBadge.GetComponent<Text>();
                if (textComponent != null)
                {
                    textComponent.text = "X";
                }
            }
            
            // Update button interactability safely
            var button = gameObject.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = false;
            }
            
            // Change color to indicate disabled state
            var buttonImage = gameObject.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = new Color(0.5f, 0.5f, 0.5f, 0.7f); // Grayed out
            }
        }
        else
        {
            // Update text badge safely
            if (textBadge != null)
            {
                var textComponent = textBadge.GetComponent<Text>();
                if (textComponent != null)
                {
                    textComponent.text = Setting.Instance.RemainHints.ToString();
                }
            }
            
            // Update button interactability safely
            var button = gameObject.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = true;
            }
            
            // Restore normal color
            var buttonImage = gameObject.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = Color.white; // Normal state
            }
        }
        
        //playAd.SetActive(Setting.Instance.RemainHint <= 0);
        //hintBadge.SetActive(Setting.Instance.RemainHint > 0);
    }
}
