 using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using static GNPXcore.NuPz_Win;



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
                "hell" => Setting.Instance.HellLevel,
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
