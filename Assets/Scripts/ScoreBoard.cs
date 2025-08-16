using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using Newtonsoft.Json;

[Serializable]
public class ScoreRec
{
    public string Version { get; set; } = "1.0";
    public int Score { get; set; } = 0;
    public float ElapsedTime { get; set; } = 0;
    public string Date { get; set; } = "";
    public string Difficulity { get; set; } = "Easy";
}

public class ScoreBoard
{
    public static ScoreBoard Instance = new ScoreBoard();
    
    public readonly string path = Application.persistentDataPath + @"/score_data.txt";    
    public readonly int BoardSize = 10;
    

    public Dictionary<string, List<ScoreRec>> leaderBoard;
    //public int AppSessionsCount { get; set; } = 1;
    //public int RatingEventsCount { get; set; } = 0;    

    public ScoreBoard()
    {
        load();
    }

    private void load()
    {
        if (File.Exists(path))
        {
            StreamReader file = new StreamReader(path);
            string json = file.ReadToEnd();
            leaderBoard = JsonConvert.DeserializeObject<Dictionary<string, List<ScoreRec>>>(json);
            //    .Also(x =>
            //{
            //    this.leaderBoard = x.leaderBoard;
            //    this.AppSessionsCount = x.AppSessionsCount;
            //    this.RatingEventsCount = x.RatingEventsCount;
            //});
            file.Close();

            //this.AppSessionsCount++;
            save();
        }
        else
        {
            leaderBoard = new Dictionary<string, List<ScoreRec>>();
            save();
        }
    }
    private void save()
    {
        try
        {
            File.WriteAllText(path, string.Empty);
            StreamWriter writer = new StreamWriter(path, false);
            string json = JsonConvert.SerializeObject(leaderBoard);
            writer.WriteLine(json);
            writer.Close();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
        
    }
    public bool AddScore(ScoreRec aScore) // best score
    {
        bool bestScore = false;

        //this.RatingEventsCount++;
        if (leaderBoard.ContainsKey(aScore.Difficulity))
        {
            var scores = leaderBoard[aScore.Difficulity];
            scores.Add(aScore);
            scores.Sort((a, b) => (a.ElapsedTime > b.ElapsedTime) ? 1 : (a.ElapsedTime < b.ElapsedTime) ? -1 : 0);
            if (scores.Count > BoardSize)
                scores.RemoveRange(BoardSize, scores.Count - BoardSize);

            leaderBoard[aScore.Difficulity] = scores;
            bestScore = (scores[0] == aScore);
        }
        else
        {
            leaderBoard.Add(aScore.Difficulity, new List<ScoreRec>() { aScore });
            bestScore = true;
        }

        save();

        return bestScore;
    }
    public ScoreRec BestRecord(string Difficulity)
    {
        if (!leaderBoard.ContainsKey(Difficulity))
            return null;
        else
            return leaderBoard[Difficulity][0];
    }
    //public void didOpenSession()
    //{
    //    this.AppSessionsCount++;
    //    save();
    //}
    //public void IncreaseRatingEvent()
    //{
    //    this.RatingEventsCount++;
    //    save();
    //}
}