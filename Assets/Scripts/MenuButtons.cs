using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    public void LoadScene(string name) 
    { 
        SceneManager.LoadScene(name);
    }
    private void newGame()
    {
        GameSettings.Instance.ContinuePreviousGame = false;
        GameSettings.Instance.Pause = false;        
    }
    public void LoadEasyScene(string name) 
    { 
        GameSettings.Instance.GameMode = EGameMode.EASY;
        newGame();
        SceneManager.LoadScene(name);
    }
    public void LoadMediumScene(string name) 
    { 
        GameSettings.Instance.GameMode = EGameMode.MEDIUM;
        newGame();
        SceneManager.LoadScene(name);
    }
    public void LoadHardScene(string name) 
    { 
        GameSettings.Instance.GameMode = EGameMode.HARD;
        newGame();
        SceneManager.LoadScene(name);
    }
    public void LoadVeryHardScene(string name) 
    { 
        GameSettings.Instance.GameMode = EGameMode.EXTREME;
        newGame();
        SceneManager.LoadScene(name);
    }
    public void LoadHellScene(string name) 
    { 
        GameSettings.Instance.GameMode = EGameMode.HELL;
        newGame();
        SceneManager.LoadScene(name);
    }

    public void ActivateObject(GameObject obj) => obj.SetActive(true);
    public void DeActivateObject(GameObject obj) => obj.SetActive(false);

    public void SetPause(bool paused)
    {
        //GameSettings.Instance.Pause = paused;
        PauseClock(paused);
        GameEvents.OnPauseMethod(paused);
    }
    public void PauseClock(bool paused)
    {
        GameSettings.Instance.Pause = paused;
    }

    public void ContinuePreviousGame(bool continue_game)
    {
        GameSettings.Instance.ContinuePreviousGame = continue_game;
    }

    public void ExistAfterWon()
    {
        GameSettings.Instance.ExistAfterWon = true;
    }

    public void ContinueAfterGameOver()
    {        
        AdManager.Instance.ShowLiveRewardAd();
    }
    public void ContinueAfterGameWon()
    {
        if (RatingAppManager.Instance.ShouldAskForRating())
            RatingAppManager.Instance.AskForRating();
        else
            AdManager.Instance.ShowInterstitialAd();
    }
}
