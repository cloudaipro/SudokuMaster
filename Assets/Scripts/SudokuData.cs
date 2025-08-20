 using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using static GNPXcore.NuPz_Win;

public class SudokuEasyData : MonoBehaviour {
    public static List<SudokuData.SudokuBoardData> getData()
    {
        var data = new List<SudokuData.SudokuBoardData>();
        data.Add(
            new SudokuData.SudokuBoardData(
                new int[81] { 9, 0, 4, 0, 0, 5, 0, 0, 2, 2, 5, 0, 3, 8, 4, 1, 6, 0, 0, 8, 1, 0, 2, 0, 0, 0, 4, 0, 0, 0, 6, 5, 0, 2, 3, 7, 7, 2, 8, 0, 1, 3, 0, 0, 0, 5, 3, 0, 2, 0, 7, 0, 4, 0, 6, 7, 3, 0, 4, 0, 9, 1, 0, 0, 1, 2, 5, 0, 0, 6, 0, 8, 0, 9, 5, 7, 6, 0, 4, 0, 0 },
                new int[81] { 9, 6, 4, 1, 7, 5, 3, 8, 2, 2, 5, 7, 3, 8, 4, 1, 6, 9, 3, 8, 1, 9, 2, 6, 7, 5, 4, 1, 4, 9, 6, 5, 8, 2, 3, 7, 7, 2, 8, 4, 1, 3, 5, 9, 6, 5, 3, 6, 2, 9, 7, 8, 4, 1, 6, 7, 3, 8, 4, 2, 9, 1, 5, 4, 1, 2, 5, 3, 9, 6, 7, 8, 8, 9, 5, 7, 6, 1, 4, 2, 3 })
            /*
            new SudokuData.SudokuBoardData(
new int[81]{ 0, 2, 3, 0, 0, 0, 8, 0, 0, 4, 0, 6, 0, 8, 0, 0, 7, 0, 7, 8, 0, 3, 0, 0, 5, 0, 2, 0, 0, 7, 0, 0, 8, 2, 0, 0, 0, 6, 0, 0, 2, 0, 7, 0, 0, 0, 0, 0, 6, 5, 0, 0, 8, 4, 6, 0, 2, 5, 9, 0, 4, 0, 0, 0, 1, 0, 0, 0, 4, 0, 5, 7, 0, 0, 5, 0, 0, 1, 0, 2, 0 },
new int[81]{ 1, 2, 3, 7, 4, 5, 8, 9, 6, 4, 5, 6, 9, 8, 2, 1, 7, 3, 7, 8, 9, 3, 1, 6, 5, 4, 2, 5, 9, 7, 4, 3, 8, 2, 6, 1, 8, 6, 4, 1, 2, 9, 7, 3, 5, 2, 3, 1, 6, 5, 7, 9, 8, 4, 6, 7, 2, 5, 9, 3, 4, 1, 8, 9, 1, 8, 2, 6, 4, 3, 5, 7, 3, 4, 5, 8, 7, 1, 6, 2, 9 })
            */
);
        return data;
    }
}
public class SudokuMediumData : MonoBehaviour {
    public static List<SudokuData.SudokuBoardData> getData()
    {
        return SudokuData.Instance.LoadDatasetFromFile("Medium");
    }
}
public class SudokuHardData : MonoBehaviour {
    public static List<SudokuData.SudokuBoardData> getData()
    {
        return SudokuData.Instance.LoadDatasetFromFile("Hard");
    }
}
public class SudokuVeryHardData : MonoBehaviour {
    public static List<SudokuData.SudokuBoardData> getData()
    {
        return SudokuData.Instance.LoadDatasetFromFile("Extreme");
    }
}



public class SudokuData : MonoBehaviour
{
    public struct SudokuBoardData 
    {
        public int[] unsolved_data;
        public int[] solved_data;
        public int unsolvedCells;
        public int removeSingle;
        public int cutoff;
        public int difficulity;
        public int step;
        public double solvedTime;
        public int level;
        public List<_MethodCounter> methodCounters;

        public SudokuBoardData(int[] unsolved, int[] solved) : this()
        {
            this.unsolved_data = unsolved;
            this.solved_data = solved;
        }
        public void ShuffleNumber()
        {
            var numberShuffle = System.Linq.Enumerable.Range(1, 9).Shuffle();
            for (int i = 0; i < 81; i++)
            {
                if (unsolved_data[i] > 0)
                    unsolved_data[i] = numberShuffle[unsolved_data[i] - 1];
                solved_data[i] = numberShuffle[solved_data[i] - 1];
            }
        }
    };

    public static SudokuData Instance;
 
    //public static Dictionary<string, List<SudokuBoardData>> sudoku_game = new Dictionary<string, List<SudokuBoardData>>();
    void Awake() 
    {
        //if (Instance == null)
        //{
        //    Instance = this;
        //    DontDestroyOnLoad(this);
        //    sudoku_game.Add("Easy", SudokuEasyData.getData());
        //    sudoku_game.Add("Medium", SudokuMediumData.getData());
        //    sudoku_game.Add("Hard", SudokuHardData.getData());
        //    sudoku_game.Add("VeryHard", SudokuVeryHardData.getData());
        //}
        //else
        //    Destroy(this);

        if (!Instance)
            Instance = this;
        else
            Destroy(this);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        //if (!sudoku_game.ContainsKey("Easy"))
        //    sudoku_game.Add("Easy", SudokuEasyData.getData());
        //if (!sudoku_game.ContainsKey("Medium"))
        //    sudoku_game.Add("Medium", SudokuMediumData.getData());
        //if (!sudoku_game.ContainsKey("Hard"))
        //    sudoku_game.Add("Hard", SudokuHardData.getData());
        //if (!sudoku_game.ContainsKey("Extreme"))
        //    sudoku_game.Add("Extreme", SudokuVeryHardData.getData());
    }


    public List<SudokuData.SudokuBoardData> LoadDatasetFromFile(string level)
    {
        TextAsset mydata = Resources.Load(@"Dataset/" + level) as TextAsset;

        string json = mydata.text;
        return JsonConvert.DeserializeObject<List<SudokuData.SudokuBoardData>>(json);
    }
    public static List<SudokuData.SudokuBoardData> GetData(string level)
    {
        try
        {
            string path = "";
            string tmpStr = level.ToLower();
            int iLevel = (tmpStr) switch
            {
                "medium" => Setting.Instance.MediumLevel,
                "hard" => Setting.Instance.HardLevel,
                "extreme" => Setting.Instance.ExtremeLevel,
                _ => 0
            };
            iLevel = ((int)((iLevel - 1) / 3)) + 1;
            if (iLevel > 10)
                iLevel = Random.Range(1, 11);

            path = $"Dataset/{tmpStr}/{iLevel}-{tmpStr}";
            //if (level == "Medium")
            //    path = $"Dataset/medium/{subLevel}-medium";
            //else if (level == "Hard")
            //    path = $"Dataset/hard/{subLevel}-hard";
            //else if (level == "Extreme")
            //    path = $"Dataset/extreme/{subLevel}-extreme";
            //else
            //    return new List<SudokuData.SudokuBoardData>();

            TextAsset mydata = Resources.Load(path) as TextAsset;

            string json = mydata.text;
            return JsonConvert.DeserializeObject<List<SudokuData.SudokuBoardData>>(json);

        }
        catch (System.Exception)
        {
            return new List<SudokuData.SudokuBoardData>();
        }

    }
}
