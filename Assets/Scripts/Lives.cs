using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lives : MonoBehaviour
{
    public static Lives Instance;
    public List<GameObject> error_images;
    public GameObject GameOverPopup;
    public int lives { get; set; } = 0;
    public int error_number { get; set; } = 0;

    // Start is called before the first frame update
    private void Awake()
    {
        if (Instance)
            Destroy(Instance);
        Instance = this;
    }
    void Start()
    {
        lives = error_images.Count;
        error_number = 0;

        if (GameSettings.Instance.ContinuePreviousGame) {
            var gameProgress = Config.LoadBoardData();
            error_number = gameProgress.error_number;
            lives = error_images.Count - error_number;
            for (int i = 0; i < error_number; i++) {
                error_images[i].SetActive(true);
            }
        }
    }
    private void OnEnable()
    {
        GameEvents.OnWrongNumber += WrongNumber;
        GameEvents.OnDidFinishLiveRewardAd += DidFinisLiveRewardAd;
    }
    private void OnDisable()
    {
        GameEvents.OnWrongNumber -= WrongNumber;
        GameEvents.OnDidFinishLiveRewardAd -= DidFinisLiveRewardAd;
    }

    private void WrongNumber()
    {
        if (error_number < error_images.Count)
        {
            error_images[error_number++].SetActive(true);
            lives--;
        }
        CheckForGameOver();
    }
    private void CheckForGameOver()
    {
        if (lives <= 0)
            DoGameOverProcess();
    }
    public void DoGameOverProcess()
    {
        GameEvents.OnGameOverMethod();
        GameOverPopup.SetActive(true);
    }
    public void DidFinisLiveRewardAd()
    {
        Dispatcher.RunOnMainThread(() =>
        {
            foreach (var error in error_images)
            {
                error.SetActive(false);
            }
            error_number = 0;
            lives = error_images.Count;

            GameEvents.OnSaveProgressDataMethod();
        });
    //}
    //public void ResetLives()
    //{
    }
}
