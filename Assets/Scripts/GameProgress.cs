using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameProgress
{
    public float current_time;
    public EGameMode game_Mode;
    public int level;
    public int selected_index_at_dataOfLevel;
    public int error_number;
    public Dictionary<int, List<int>> grid_notes;
    public int[] unsolved;
    public int[] solved;
    public bool[] hasDefaultFlags;
    public bool[] isHypothesisNumberFlags;
    public bool noteHintMode;
}
