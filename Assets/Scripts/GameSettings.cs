using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance;
    private void Awake()
    {
        Pause = false;
        if (Instance == null)
        {
            DontDestroyOnLoad(this);
            Instance = this;
        }
        else
            Destroy(this);
    }

    public EGameMode GameMode { get; set; }
    public bool ContinuePreviousGame { get; set; } = false;
    public int Level { get; set; } = 1;
    private bool _existAfterWon = false;
    public bool ExistAfterWon
    {
        get => _existAfterWon;
        set
        {
            _existAfterWon = value;
            ContinuePreviousGame = false;
        }
    }
    public bool Pause { get; set; } = false;
    private void Start()
    {
        ExistAfterWon = false;
        GameMode = EGameMode.NOT_SET;
        ContinuePreviousGame = false;
    }
    public bool bBoardInteractable { get; set; } = true;
}
