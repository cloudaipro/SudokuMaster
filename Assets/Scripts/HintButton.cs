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
        textBadge.GetComponent<Text>().text = Setting.Instance.RemainHints.ToString();
        //playAd.SetActive(Setting.Instance.RemainHint <= 0);
        //hintBadge.SetActive(Setting.Instance.RemainHint > 0);
    }
}
