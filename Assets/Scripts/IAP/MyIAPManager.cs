using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class MyIAPManager : MonoBehaviour, IStoreListener
{
    private IStoreController m_StoreController;
#if UNITY_ANDROID
    private IGooglePlayStoreExtensions m_StoreExtensions;
#elif UNITY_IOS
    private IAppleExtensions m_StoreExtensions;
#endif

    public GameObject MoreHintDialog;
    public GameObject Price25Hints;
    public GameObject WaittingCursorPanel;
    public string ProductID = "com.ccs.sudokumaster.25hint";

    private void Awake()
    {
        InitializeMyIAPManager();
    }

    public void InitializeMyIAPManager()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct(ProductID, ProductType.Consumable, new IDs
        {
            {ProductID, GooglePlay.Name},
            {ProductID, AppleAppStore.Name}
        });
        StandardPurchasingModule.Instance().useFakeStoreUIMode = FakeStoreUIMode.StandardUser;
        UnityPurchasing.Initialize(this, builder);
    }

    /// <summary>
    /// Called when Unity IAP is ready to make purchases.
    /// </summary>
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        this.m_StoreController = controller;
#if UNITY_ANDROID
        this.m_StoreExtensions = extensions.GetExtension<IGooglePlayStoreExtensions>();
#elif UNITY_IOS
        this.m_StoreExtensions = extensions.GetExtension<IAppleExtensions>();
#endif        

        Debug.Log("Congratulations!\nUnity IAP is ready to make purchases.");
        GetProductDetailsForProductId(ProductID);
    }

    void GetProductDetailsForProductId(string productId)
    {
        var product = this.m_StoreController.products.WithID(productId);
        
        Price25Hints.GetComponent<Text>().text = (product != null) ? $"{product.metadata.isoCurrencyCode}{product.metadata.localizedPriceString}"
                                                                   : "not available";
//#if UNITY_IOS
//        var detailsDictionary = m_StoreExtensions.GetProductDetails();
        
//        if (detailsDictionary.ContainsKey(productId))
//        {
//            var detail = detailsDictionary[productId];
//            Debug.Log($"Product Details for {productId}: {detail}");
//            //Price25Hints.GetComponent<Text>().text = ..localizedPriceString ?? "";//.localizedPrice.ToString() ?? "";
//        }
//#endif
    }
    /// <summary>
    /// Called when Unity IAP encounters an unrecoverable initialization error.
    ///
    /// Note that this will not be called if Internet is unavailable; Unity IAP
    /// will attempt initialization until it becomes available.
    /// </summary>
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log(error);
    }

    private void CloseDialog()
    {
        GameSettings.Instance.Pause = false;
        MoreHintDialog.SetActive(false);
        WaitingCursor.Instance.HideCursor();
        WaittingCursorPanel.SetActive(false);
    }

    public void Buy25Hints()
    {
        if (Price25Hints.GetComponent<Text>().text == "not available")
            GameEvents.RewardAdFailMethod();
        else
        {
            WaittingCursorPanel.SetActive(true);
            WaitingCursor.Instance.ShowCursor();
            m_StoreController.InitiatePurchase(ProductID);
        }
    }

    /// <summary>
    /// Called when a purchase completes.
    ///
    /// May be called at any time after OnInitialized().
    /// </summary>
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        Setting.Instance.UpdateIAPHints(Setting.Instance.IAPHints + (int)(e.purchasedProduct.definition.payout?.quantity ?? 25));
        CloseDialog();
        GameEvents.OnUpdateHintBadgeMethod();
        return PurchaseProcessingResult.Complete;
    }

    /// <summary>
    /// Called when a purchase fails.
    /// </summary>
    public void OnPurchaseFailed(Product i, PurchaseFailureReason p)
    {
        Debug.Log(i);
        CloseDialog();
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.Log(message);
        //throw new System.NotImplementedException();
    }
}