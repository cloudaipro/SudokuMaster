using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using static CommonUtils;

public class Clock : MonoBehaviour
{
    public static Clock Instance;
    public int Hour { get; set; } = 0;
    public int Minute { get; set; } = 0;
    public int Seconds { get; set; } = 0;
    public Text textClock { get; set; }
    public float delta_time { get; set; } = 0.0f;
    bool Stop_click { get; set; } = false;

    private void Awake()
    {
        if (Instance)
            Destroy(Instance);
        Instance = this;
        textClock = GetComponent<Text>();

        if (GameSettings.Instance.ContinuePreviousGame)
        {
            var gameProgress = Config.LoadBoardData();
            delta_time = gameProgress.current_time;
        }
        else
            delta_time = 0;

    }

    // Start is called before the first frame update
    void Start()
    {
        Stop_click = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Stop_click && !(GameSettings.Instance?.Pause ?? true))
        {
            delta_time += Time.deltaTime;
            textClock.text = FormatDeltaTime(delta_time); //String.Format("{0}:{1}:{2}", span.Hours.LeadingZero(2), span.Minutes.LeadingZero(2), span.Seconds.LeadingZero(2));
        }
    }
    public void OnGameOver()
    {
        Stop_click = true;
    }

    private void OnEnable()
    {
        GameEvents.OnGameOver += OnGameOver;
    }
    private void OnDisable()
    {
        GameEvents.OnGameOver -= OnGameOver;
    }

    public void StartClock()
    {
        Stop_click = false;
    }
}
