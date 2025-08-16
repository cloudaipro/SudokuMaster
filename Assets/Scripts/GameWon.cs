using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Globalization;
using static CommonUtils;

public class GameWon : MonoBehaviour
{
    [SerializeField] AudioSource Win_Audio = null;
    public GameObject WinPopup;
    public Text ClockText;
    public Text LevelText;
    public Text BestTime;

    // Start is called before the first frame update
    void Start()
    {
        WinPopup.SetActive(false);
        ClockText.text = Clock.Instance.textClock.text;
        LevelText.text = GameSettings.Instance.GameMode.GetDescription();
    }

    private void OnBoardCompleted()
    {
        // save level
        int level = (GameSettings.Instance.GameMode) switch
        {
            EGameMode.MEDIUM => Setting.Instance.MediumLevel + 1,
            EGameMode.HARD => Setting.Instance.HardLevel + 1,
            EGameMode.EXTREME => Setting.Instance.ExtremeLevel + 1,
            _ => 1
        };
        Setting.Instance.UpdateLevel(GameSettings.Instance.GameMode, level);

        var score = new ScoreRec
        {
            Date = DateTime.Now.ToString("G", DateTimeFormatInfo.InvariantInfo),
            ElapsedTime = Clock.Instance.delta_time,
            Difficulity = GameSettings.Instance.GameMode.GetDescription()
        };
        bool bestScore = ScoreBoard.Instance.AddScore(score);

        WinPopup.SetActive(true);
        ClockText.text = Clock.Instance.textClock.text;
        BestTime.text = FormatDeltaTime(
                                (bestScore) ? score.ElapsedTime :
                                              ScoreBoard.Instance.BestRecord(score.Difficulity).ElapsedTime);

        Win_Audio.Play();
    }

    private void OnEnable() {
        GameEvents.OnBoardCompleted += OnBoardCompleted;
    }

    private void OnDisable() {
        GameEvents.OnBoardCompleted -= OnBoardCompleted;
    }
}
