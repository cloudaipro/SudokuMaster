using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_ANDROID
using Google.Play.Review;
#endif


public class RatingAppManager : MonoBehaviour
{
    public static RatingAppManager Instance;

    [Tooltip("Display app rating window after x game opened.")]
    public int remindRating = 5; // open rating window at X game start
    public bool TestMode;
    public string AndroidAppID = "";
#if UNITY_ANDROID
    private ReviewManager _reviewManager;
    PlayReviewInfo _playReviewInfo;
    private Coroutine _coroutine;
#endif

    private void Awake()
    {
        // increase game open counter
        int gameOpenCounter = PlayerPrefs.GetInt("gameOpenCounter", 0) + 1;
        PlayerPrefs.SetInt("gameOpenCounter", gameOpenCounter);
        DontDestroyOnLoad(this);

        Instance = this;
    }
    

    void Start()
    {       
#if UNITY_ANDROID       
        _coroutine = StartCoroutine(InitReview());
#endif
    }

    public bool ShouldAskForRating()
    {
        bool isRated = PlayerPrefs.GetInt("isAppRated", 0) == 1 ? true : false;
        return (TestMode) ? true : ((PlayerPrefs.GetInt("gameOpenCounter", 0) % remindRating) == 0) && isRated == false;
    }

    public void AskForRating()
    {
        Debug.Log("+++++++++  AskForRating  ++++++++++++");
#if UNITY_IOS
        UnityEngine.iOS.Device.RequestStoreReview();
#else
        StartCoroutine(LaunchReview());
#endif
        // set app is rated 
        PlayerPrefs.SetInt("isAppRated", 1);
    }

#if UNITY_ANDROID
    private IEnumerator InitReview(bool force = false)
    {
        if (_reviewManager == null) _reviewManager = new ReviewManager();

        var requestFlowOperation = _reviewManager.RequestReviewFlow();
        yield return requestFlowOperation;
        if (requestFlowOperation.Error != ReviewErrorCode.NoError)
        {
            if (force) DirectlyOpen();
            yield break;
        }

        _playReviewInfo = requestFlowOperation.GetResult();
    }

    public IEnumerator LaunchReview()
    {
        if (_playReviewInfo == null)
        {
            if (_coroutine != null) StopCoroutine(_coroutine);
            yield return StartCoroutine(InitReview(true));
        }

        var launchFlowOperation = _reviewManager.LaunchReviewFlow(_playReviewInfo);
        yield return launchFlowOperation;
        _playReviewInfo = null;
        if (launchFlowOperation.Error != ReviewErrorCode.NoError)
        {
            DirectlyOpen();
            yield break;
        }
    }
    private void DirectlyOpen()
    {
        Application.OpenURL($"https://play.google.com/store/apps/details?id="+ AndroidAppID);
    }

#endif

}



