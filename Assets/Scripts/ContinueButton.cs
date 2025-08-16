using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static CommonUtils;

public class ContinueButton : MonoBehaviour
{
    public Text timeText;
    public Text levelText;
    // Start is called before the first frame update
    
    void Start()
    {
        if (Config.GameDataFileExist() == false) {
            gameObject.GetComponent<Button>().interactable = false;
            timeText.text = "";
            levelText.text = "";
        }
        else {
            var gameProgress = Config.LoadBoardData();
            var delta_time = gameProgress.current_time;
            //delta_time += Time.deltaTime;
            ///TimeSpan span = TimeSpan.FromSeconds(delta_time);
            timeText.text = FormatDeltaTime(delta_time); //String.Format("{0}:{1}:{2}", span.Hours.LeadingZero(2), span.Minutes.LeadingZero(2), span.Seconds.LeadingZero(2));
            levelText.text = gameProgress.game_Mode.GetDescription();
            Debug.Log(gameProgress.game_Mode.GetDescription());
        }
    }

    public void SetGameData()
    {
        var gameProgress = Config.LoadBoardData();
        GameSettings.Instance.GameMode = gameProgress.game_Mode;
    }
}
