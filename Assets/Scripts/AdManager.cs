using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance;
    // public string AppId { get; set; }
    public string BannerAdId;
    public string InterstitialAdId;
    public string FastNoteRewardId;
    public string HintRewardId;
    public string LiveRewardId;
    public string BannerAdIdAndroid;
    public string InterstitialAdIdAndroid;
    public string FastNoteRewardIdAndroid;
    public string HintRewardIdAndroid;
    public string LiveRewardIdAndroid;
    public AdPosition BannerPosition;
    public bool TestDevice;
    public BannerView _bannerView;
    public InterstitialAd _interstitial;
    public RewardedAd _hintRewardAd;
    public RewardedAd _fastNoteRewardAd;
    public RewardedAd _liveRewardAd;

    //[SerializeField] private RectTransform game;
    //[SerializeField] private RectTransform ad;
    [SerializeField] private Canvas canvas;
    float bannerHeight;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
            Destroy(this);
    }
    // Start is called before the first frame update
    void Start()
    {
        MobileAds.Initialize(initStatus => { });        
        CreateBanner(CreateRequest());
        CreateInterstitialAd(CreateRequest());
        CreateHintRewardAd(CreateRequest());
        CreateFastNoteRewardAd(CreateRequest());
        CreateLiveRewardAd(CreateRequest());
    }
    private AdRequest CreateRequest()
    {        
        return new AdRequest.Builder().Build();        
    }

    #region InterstitialAd
    public void CreateInterstitialAd(AdRequest request)
    {
        InterstitialAd.Load(TestDevice ? "ca-app-pub-3940256099942544/4411468910" : 
        #if UNITY_IOS
        InterstitialAdId,
        #else
        InterstitialAdIdAndroid,
        #endif
        request, (ad, error) => {
            this._interstitial = ad;
        });
        // this._interstitial = new InterstitialAd( TestDevice ? "ca-app-pub-3940256099942544/4411468910" : InterstitialAdId);
        // this._interstitial.LoadAd(request);
    }
    public void ShowInterstitialAd()
    {
        try
        {
            if ((this._interstitial?.CanShowAd() ?? false))
                this._interstitial.Show();

            CreateInterstitialAd(CreateRequest());
            //this._interstitial.LoadAd(CreateRequest());

        }
        catch (System.Exception)
        {

        }
    }
    #endregion

    #region FastNoteReward
    public void CreateFastNoteRewardAd(AdRequest request)
    {
        if (_fastNoteRewardAd != null)
        {
            _fastNoteRewardAd.Destroy();
            _fastNoteRewardAd = null;
        }

        RewardedAd.Load(
            TestDevice ?
#if UNITY_IOS
        "ca-app-pub-3940256099942544/1712485313"
#else
        "ca-app-pub-3940256099942544/5224354917"
#endif
            :
#if UNITY_IOS
        FastNoteRewardId
#else
        FastNoteRewardIdAndroid
#endif
        , request, (ad, error) => {
            if (error != null || ad == null)
            {
                Debug.LogError("Rewarded ad failed to load an ad " +
                               "with error : " + error);
                return;
            }

            this._fastNoteRewardAd = ad;
            ad.OnAdFullScreenContentClosed += () =>
            {
                GameEvents.GiveFastNoteMethod();
                CreateFastNoteRewardAd(CreateRequest());
            };
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                GameEvents.GiveFastNoteMethod();
                CreateFastNoteRewardAd(CreateRequest());
            };
        });
    }
    public void ShowFastNoteRewardAd()
    {
        if (this._fastNoteRewardAd?.CanShowAd() ?? false)
            this._fastNoteRewardAd.Show(reward =>
            {
                Debug.Log("-----ShowFastNoteRewardAd: " + reward.Type);
                //if (reward.Type == "Reward")
                //GameEvents.GiveFastNoteMethod();                
            });
        else
        {
            //GameEvents.GiveFastNoteMethod();
            GameEvents.RewardAdFailMethod();
            CreateFastNoteRewardAd(CreateRequest());
        }
    }
    #endregion

    #region HintRewardAd
    public void CreateHintRewardAd(AdRequest request)
    {
        if (_hintRewardAd != null)
        {
            _hintRewardAd.Destroy();
            _hintRewardAd = null;
        }

        RewardedAd.Load(
            TestDevice ?
#if UNITY_IOS
        "ca-app-pub-3940256099942544/1712485313"
#else
        "ca-app-pub-3940256099942544/5224354917"
#endif
            :
#if UNITY_IOS
        HintRewardId
#else
        HintRewardIdAndroid
#endif
        , request, (ad, error) => {
            if (error != null || ad == null)
            {
                Debug.LogError("Rewarded ad failed to load an ad " +
                               "with error : " + error);
                return;
            }

            this._hintRewardAd = ad;
            ad.OnAdFullScreenContentClosed += () =>
            {
                GameEvents.DidFinishHintAdMethod();
                CreateHintRewardAd(CreateRequest());
            };
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                GameEvents.DidFinishHintAdMethod();
                CreateHintRewardAd(CreateRequest());
            };
        });
    }
    public void ShowHintRewardAd()
    {
        if (this._hintRewardAd?.CanShowAd() ?? false)
            this._hintRewardAd.Show(reward =>
            {
                Debug.Log("-----ShowHintRewardAd: " + reward.Type);
                //if (reward.Type == "Reward" || reward.Type == "coins")
                //GameEvents.DidFinishHintAdMethod();
            });
        else
        {
            //GameEvents.DidFinishHintAdMethod();
            GameEvents.RewardAdFailMethod();
            CreateHintRewardAd(CreateRequest());
        }
    }
    #endregion

    #region LiveReward
    public void CreateLiveRewardAd(AdRequest request)
    {
        if (_liveRewardAd != null)
        {
            _liveRewardAd.Destroy();
            _liveRewardAd = null;
        }

        RewardedAd.Load(
            TestDevice ?
#if UNITY_IOS
        "ca-app-pub-3940256099942544/1712485313"
#else
        "ca-app-pub-3940256099942544/5224354917"
#endif
            :
#if UNITY_IOS
        LiveRewardId
#else
        LiveRewardIdAndroid
#endif
        , request, (ad, error) => {
            if (error != null || ad == null)
            {
                Debug.LogError("Rewarded ad failed to load an ad " +
                               "with error : " + error);
                return;
            }

            this._liveRewardAd = ad;
            ad.OnAdFullScreenContentClosed += () =>
            {
                GameEvents.DidFinishLiveRewardAdMethod();
                CreateLiveRewardAd(CreateRequest());
            };
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                GameEvents.DidFinishLiveRewardAdMethod();
                CreateLiveRewardAd(CreateRequest());
            };
        });
    }
    public bool ShowLiveRewardAd()
    {
        if (this._liveRewardAd?.CanShowAd() ?? false)
        {
            this._liveRewardAd.Show(reward =>
            {
                Debug.Log("-----ShowLiveRewardAd: " + reward.Type);
                //if (reward.Type == "Reward")
                //GameEvents.GiveFastNoteMethod();                
            });
            return true;
        }
        else
        {
            //GameEvents.GiveFastNoteMethod();
            GameEvents.RewardAdFailMethod();
            CreateLiveRewardAd(CreateRequest());
            return false;
        }
    }
    #endregion

    #region BannerAd
    /*
    public void OnGUI()
    {
        GUI.skin.label.fontSize = 60;
        Rect textOutputRect = new Rect(
          0.15f * Screen.width,
          0.25f * Screen.height,
          0.7f * Screen.width,
          0.3f * Screen.height);
        GUI.Label(textOutputRect, "Adaptive Banner Example");
    }
    */
    public void CreateBanner(AdRequest request)
    {
        AdSize adaptiveSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);

        this._bannerView = new BannerView(TestDevice ? "ca-app-pub-3940256099942544/2934735716" :
#if UNITY_EDITOR
            "unused",
#elif UNITY_IOS
        BannerAdId,
#else
        BannerAdIdAndroid, 
#endif
        //AdSize.SmartBanner, BannerPosition);
        adaptiveSize, BannerPosition);

        //this._bannerView.OnAdLoaded += HandleOnAdLoaded;

        this._bannerView.LoadAd(request);
        HideBanner();
    }
    public void HideBanner()
    {
        _bannerView.Hide();
    }
    public void ShowBanner()
    {
        _bannerView.Show();
    }

    private void HandleOnAdLoaded(object sender, System.EventArgs args)
    {
        bannerHeight = this._bannerView.GetHeightInPixels() / canvas.scaleFactor;
        
        //game.offsetMin = new Vector2(0, bannerHeight);
        //ad.sizeDelta = new Vector2(0, bannerHeight);
    }
    #endregion
}
