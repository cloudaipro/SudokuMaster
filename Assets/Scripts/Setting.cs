using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;


[Serializable]
public class Setting
{
    private static string dir = Application.persistentDataPath;

    static string settingFile = @"/setting.txt";
    static string settingPath = dir + settingFile;

    private static Setting _Instance = null;
    public static Setting Instance
    {
        get
        {
            if (_Instance == null)
                _Instance = LoadSetting();
            return _Instance;
        }
    }

    public int FreeHints { get; set; } = 3;
    public int IAPHints { get; set; } = 0;
    public int RemainHints { get => FreeHints + IAPHints; }
    public int MediumLevel { get; set; } = 1;
    public int HardLevel { get; set; } = 1;
    public int ExtremeLevel { get; set; } = 1;
    public string LastDateOfMediumLevel { get; set; } = "";
    public string LastDateOfHardLevel { get; set; } = "";
    public string LastDateOfExtremeLevel { get; set; } = "";


    public static Setting LoadSetting()
    {
        if (File.Exists(settingPath) == false)
            return new Setting();
        try
        {
            Debug.Log(settingPath);
            StreamReader file = new StreamReader(settingPath);
            string json = file.ReadToEnd();
            var obj = JsonConvert.DeserializeObject<Setting>(json);//JsonUtility.FromJson<GameProgress>(json);
            file.Close();
            return obj ?? new Setting();
        }
        catch (Exception ex)
        {
            return new Setting();
        }
    }

    public void SaveSetting()
    {
        try
        {
            File.WriteAllText(settingPath, string.Empty);
            StreamWriter writer = new StreamWriter(settingPath, false);
            string json = JsonConvert.SerializeObject(this);
            writer.WriteLine(json);
            writer.Close();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
        
    }

    public int UseHint()
    {
        if (FreeHints > 0)
            FreeHints--;
        else if (IAPHints > 0)
            IAPHints--;
        SaveSetting();
        return RemainHints;
    }
    public void UpdateFreeHints(int value)
    {
        FreeHints = value;
        SaveSetting();
    }
    public void UpdateIAPHints(int value)
    {
        IAPHints = value;
        SaveSetting();
    }

    public void UpdateLevel(EGameMode game_mode, int level)
    {
        var lastDate = DateTime.Now.ToString("G", DateTimeFormatInfo.InvariantInfo);
        switch (game_mode)
        {
            case EGameMode.MEDIUM:
                if (level > MediumLevel)
                {
                    MediumLevel = level;
                    LastDateOfMediumLevel = lastDate;
                }
                break;
            case EGameMode.HARD:
                if (level > HardLevel)
                {
                    HardLevel = level;
                    LastDateOfHardLevel = lastDate;
                }
                break;
            case EGameMode.EXTREME:
                if (level > ExtremeLevel)
                {
                    ExtremeLevel = level;
                    LastDateOfExtremeLevel = lastDate;
                }
                break;
            default:
                break;
        }
        SaveSetting();
    }
}
