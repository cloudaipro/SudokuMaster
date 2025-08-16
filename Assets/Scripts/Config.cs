using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using Newtonsoft.Json;

public class Config : MonoBehaviour
{
//#if UNITY_ANDROID && !UNITY_EDITOR
    private static string dir = Application.persistentDataPath;
// #else
//     private static string dir = Directory.GetCurrentDirectory();
// #endif

    static string file = @"/board_data.txt";
    static string path = dir + file;

    public static bool GameDataFileExist() => File.Exists(path);
    public static void DeleteDataFile()
    {
        if (File.Exists(path))
            File.Delete(path);
    }

    public static GameProgress LoadBoardData()
    {
        Debug.Log(path);
        StreamReader file = new StreamReader(path);
        string json = file.ReadToEnd();
        var obj = JsonConvert.DeserializeObject<GameProgress>(json);//JsonUtility.FromJson<GameProgress>(json);
        file.Close();
        return obj;
    }

    public static void SaveBoardData(SudokuData.SudokuBoardData board_data, EGameMode gameMode, int level, int selected_index_at_dataOfLevel, 
                                    int error_number, Dictionary<int, List<int>> grid_notes, bool[] hasDefaultFlags, bool noteHintMode)
    {
        try
        {
            Debug.Log(path);
            File.WriteAllText(path, string.Empty);
            StreamWriter writer = new StreamWriter(path, false);
            GameProgress saveObj = new GameProgress
            {
                current_time = Clock.Instance.delta_time,
                game_Mode = gameMode,
                level = level,
                selected_index_at_dataOfLevel = selected_index_at_dataOfLevel,
                error_number = error_number,
                grid_notes = grid_notes,
                unsolved = board_data.unsolved_data,
                solved = board_data.solved_data,
                hasDefaultFlags = hasDefaultFlags,
                noteHintMode = noteHintMode,
                //MediumLevel = GameSettings.Instance. 
            };

            string json = JsonConvert.SerializeObject(saveObj);//JsonUtility.ToJson(saveObj);
            writer.WriteLine(json);
            writer.Close();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }


        /*
        string current_time = "#time:" + Clock.CurrentTime;
        string level_string = "#level:" + level;
        string error_number_string = "#errors:" + error_number;
        string board_index_string = "#board_index:" + board_index.ToString();
        string unsolved_string = "#unsolved";
        string solved_string = "#solved";
        
        foreach(var unsolved_data in board_data.unsolved_data)
        {
            unsolved_string += unsolved_data.ToString() + ",";
        }
        foreach(var solved_data in board_data.solved_data)
        {
            solved_string += solved_data.ToString() + ",";
        }
        writer.WriteLine(current_time);
        writer.WriteLine(level_string);
        writer.WriteLine(error_number_string);
        writer.WriteLine(board_index_string);
        writer.WriteLine(unsolved_string);
        writer.WriteLine(solved_string);

        foreach(var square in grid_notes) {
            string square_string = "#" + square.Key + ":";
            bool save = false;
            foreach(var note in square.Value) {
                if (note != " ") {
                    square_string += note + ",";
                    save = true;
                }                
            }
            if (save)
            writer.WriteLine(square_string);
        }        
        */
        
    }
    /*
    public static Dictionary<int, List<int>> GetGridNotes()
    {
        var grid_notes = new Dictionary<int, List<int>>();
        string lines;
        StreamReader file = new StreamReader(path);

        while(lines = file.ReadLine() != null) {
            string[] word = lines.Split(':');
            if (word[0] == "square_note"){
                int square_index = -1;
                List<int> notes = 
            }
        }
    }
    */
}
