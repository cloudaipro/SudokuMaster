using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

public class Generator
{
    public Board board { get; set; }

    //public Generator(string starting_file)
    //{
    //    var f = File.OpenText(starting_file);
    //    var numbers = new string(f.ReadToEnd().Where(x => "123456789".Contains(x)).ToArray());
    //    var numbersList = numbers.Select(x => Convert.ToInt32(x.ToString())).ToList();
    //    f.Close();

    //    board = new Board(numbersList);
    //}
    public Generator(string sudoku_sample)
    {
        var numbersList = sudoku_sample.Select(x => Convert.ToInt32(x.ToString())).ToList();        

        board = new Board(numbersList);
    }
    public Generator(List<int> sudoku_sample)
    {
        board = new Board(sudoku_sample);
    }
    
    public void Randomize(int iterations)
    {
        if (board.get_used_cells().Count == 81)
        {
            var random = new System.Random((int)DateTime.Now.Ticks);
            int caseNum, block;
            List<int> options;
            for (int i = 0; i < iterations; i++)
            {
                caseNum = random.Next(0, 4);
                block = random.Next(0, 3) * 3;
                options = Enumerable.Range(0, 3).ToList();
                options.Shuffle();
                var piece1 = options[0];
                var piece2 = options[1];

                switch (caseNum)
                {
                    case 0:
                        board.swap_row(block + piece1, block + piece2);
                        break;
                    case 1:
                        board.swap_column(block + piece1, block + piece2);
                        break;
                    case 2:
                        board.swap_stack(piece1, piece2);
                        break;
                    case 3:
                        board.swap_band(piece1, piece2);
                        break;
                    default:
                        break;
                }
            }
        }
        else
        {
            throw new Exception("Rearranging partial board may compromise uniqueness.");
        }
    }
    
    
    // method gets all possible values for a particular cell, if there is only one
    // then we can remove that cell
    public void ReduceViaLogical(int cutoff = 81)
    {
        var usedCells = board.get_used_cells();
        usedCells.Shuffle();
        foreach (var cell in usedCells)
        {
            var possibles = board.get_possibles(cell);
            if (possibles.Count == 1)
            {
                cell.Value = 0;
                cutoff -= 1;
            }

            if (cutoff == 0)
            {
                break;
            }
        }
    }

    // method attempts to remove a cell and checks that solution is still unique
    public void ReduceViaRandom(int cutoff = 81)
    {
        var temp = board;
        var existing = temp.get_used_cells();

        // sorting used cells by density heuristic, highest to lowest
        var newSet = existing.Select(x => new { Cell = x, Density = temp.get_density(x) }).ToList();
        var elements = newSet.OrderByDescending(x => x.Density).Select(x => x.Cell).ToList();

        foreach (var cell in elements)
        {
            var original = cell.Value;
            var complement = Enumerable.Range(1, 9).Where(x => x != original).ToList();
            var ambiguous = false;

            foreach (var x in complement)
            {
                cell.Value = x;
                var s = new Solver(temp);
                // if solver can fill every box and the solution is valid then
                // puzzle becomes ambiguous after removing particular cell, so we can break out
                if (s.solve() && s.is_valid())
                {
                    cell.Value = original;
                    ambiguous = true;
                    break;
                }
            }

            // if every value was checked and puzzle remains unique, we can remove it
            if (!ambiguous)
            {
                cell.Value = 0;
                cutoff -= 1;
            }
            // if we ever meet the cutoff limit we can break out
            if (cutoff == 0)
            {
                break;
            }
        }
    }

    public string GetCurrentState()
    {
        var template = "There are currently {0} starting cells.\n\rCurrent puzzle state:\n\r\n\r{1}\n\r";
        return string.Format(template, board.get_used_cells().Count, board.ToString());
    }


    public static (int[] unsolved, int[] solved) Sudoku_Generator(string difficultyStr)
    {
        Dictionary<string, Tuple<int, int>> difficulties =
            new Dictionary<string, Tuple<int, int>>() {
                { "Easy", Tuple.Create(35, 0) },
                { "Medium", Tuple.Create(81, 5) },
                { "Hard", Tuple.Create(81, 15) },
                { "Extreme", Tuple.Create(81, 45) }
            };
        Tuple<int, int> difficulty = difficulties[difficultyStr];
        return Sudoku_Generator(difficulty);
    }

    /*
     // START: Create seed grid
        void Sudoku::fillEmptyDiagonalBox(int idx)
        {
          int start = idx*3;
          random_shuffle(this->guessNum, (this->guessNum) + 9, genRandNum);
          for (int i = 0; i < 3; ++i)
          {
            for (int j = 0; j < 3; ++j)
            {
              this->grid[start+i][start+j] = guessNum[i*3+j];
            }
          }
        } 
     */

    private static void fillEmptyDiagonalBox(IList<int> data, int idx)
    {
        int start = idx * 3;
        var numberShuffle = Enumerable.Range(1, 9).Shuffle();
        int offset;
        for (int i = 0; i < 3; i++)
        {
            offset = (start + i) * 9;
            for (int j = 0; j < 3; j++)
            {
                //Debug.LogFormat("i={0},j={1},offset={2},idx={3},nIdx={4}", i, j, offset, offset + start + j, i * 3 + j);
                data[offset + start + j] = numberShuffle[i * 3 + j];
            }
        }        
    }
    private static IList<int> genSampleData()
    {
        var sampleData = new List<int>(81);
        for (int i = 0; i < 81; i++) sampleData.Add(0);
        fillEmptyDiagonalBox(sampleData, 0);
        fillEmptyDiagonalBox(sampleData, 1);
        fillEmptyDiagonalBox(sampleData, 2);

        var numberShuffle = Enumerable.Range(1, 9).Shuffle();
        fillRemainingBlocks(sampleData, numberShuffle);
        return sampleData;
    }
    
    private static bool fillRemainingBlocks(IList<int> data, IList<int> numberShuffle)
    {
        // START: Modified Sudoku solver
        int row = 0, col = 0;
        
        // If there is no unassigned location, we are done
        if (!FindUnassignedLocation(data, ref row, ref col))
                return true; // success!

        // Consider digits 1 to 9
        for (int num = 0; num < 9; num++)
        {
            // if looks promising
            if (isSafe(data, row, col, numberShuffle[num]))
            {
                // make tentative assignment
                data[row * 9 + col] = numberShuffle[num];

                // return, if success, yay!
                if (fillRemainingBlocks(data, numberShuffle))
                    return true;

                    // failure, unmake & try again
                    data[row * 9 + col] = 0;
            }
        }

        return false; // this triggers backtracking
    }
    

// START: Helper functions for solving grid
private static bool FindUnassignedLocation(IList<int> data, ref int row, ref int col)
{ 
    for (row = 0; row < 9; row++)
    {
        for (col = 0; col < 9; col++)
        {
            if (data[row * 9 + col] == 0)
                return true;
        }
    }

    return false;
}

private static bool isSafe(IList<int> data, int row, int col, int num)
{
    return !UsedInRow(data, row, num) && !UsedInCol(data, col, num) && !UsedInBox(data, row - row % 3, col - col % 3, num);
}
private static bool UsedInRow(IList<int> data, int row, int num)
{
    for (int col = 0; col < 9; col++)
    {
        if (data[row * 9 + col] == num)
            return true;
    }

    return false;
}

private static bool UsedInCol(IList<int> data, int col, int num)
{
    for (int row = 0; row < 9; row++)
    {
        if (data[row * 9 + col] == num)
            return true;
    }

    return false;
}

private static bool UsedInBox(IList<int> data, int boxStartRow, int boxStartCol, int num)
{
    for (int row = 0; row < 3; row++)
    {
        for (int col = 0; col < 3; col++)
        {
            if (data[(row + boxStartRow) * 9 + col + boxStartCol] == num)
                return true;
        }
    }

    return false;
}
public static (int[] unsolved, int[] solved) Sudoku_Generator(Tuple<int, int> difficulty)
{
        // constructing generator object from puzzle file (space delimited columns, line delimited rows)
        /*
        var sampleData = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 4, 5, 6, 7, 8, 9, 1, 2, 3, 7, 8, 9, 1, 2, 3, 4, 5, 6, 2, 1, 4, 3, 6, 5, 8, 9, 7, 3, 6, 5, 8, 9, 7, 2, 1, 4, 8, 9, 7, 2, 1, 4, 3, 6, 5, 5, 3, 1, 6, 4, 2, 9, 7, 8, 6, 4, 2, 9, 7, 8, 5, 3, 1, 9, 7, 8, 5, 3, 1, 6, 4, 2 };
        var numberShuffle = Enumerable.Range(1, 9).Shuffle();
        for (int i = 0; i < 81; i++)
            sampleData[i] = numberShuffle[sampleData[i] - 1];
        */
        /*
          @"
            1 2 3 4 5 6 7 8 9
            4 5 6 7 8 9 1 2 3
            7 8 9 1 2 3 4 5 6
            2 1 4 3 6 5 8 9 7
            3 6 5 8 9 7 2 1 4
            8 9 7 2 1 4 3 6 5
            5 3 1 6 4 2 9 7 8
            6 4 2 9 7 8 5 3 1
            9 7 8 5 3 1 6 4 2"
        */
        var startTime = DateTime.Now;
        Console.Write(startTime.ToString("HH:mm:ss.ffff") + " Generate sample data ...");
        Generator gen = new Generator(genSampleData().ToList());// sampleData.ToList());//= new Generator(sampleData.Shuffle());
        Console.WriteLine(" done! Elapsed time: " + (DateTime.Now - startTime).ToString("g"));
        // applying 100 random transformations to puzzle
        //gen.Randomize(100);        

        startTime = DateTime.Now;
        // getting a copy before slots are removed
        Board initial = gen.board.copy();

        // applying logical reduction with corresponding difficulty cutoff
        gen.ReduceViaLogical(difficulty.Item1);

        // catching zero case
        if (difficulty.Item2 != 0)
        {
            // applying random reduction with corresponding difficulty cutoff
            gen.ReduceViaRandom(difficulty.Item2);
        }

        // getting copy after reductions are completed
        Board final = gen.board.copy();

        // printing out complete board (solution)
        Console.WriteLine("The initial board before removals was: \r\n\r\n{0}", initial);

        // printing out board after reduction
        Console.WriteLine("The generated board after removals was: \r\n\r\n{0}", final);
        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.ffff") +" Generate complete board done! Elapsed time: " + (DateTime.Now - startTime).ToString("g"));

        return (final.cells.Select(x => x.Value).ToArray(), initial.cells.Select(x => x.Value).ToArray());
    }
    
    private struct SudokuBoardData
    {
        public int[] unsolved_data;
        public int[] solved_data;
        public SudokuBoardData(int[] unsolved, int[] solved) : this()
        {
            this.unsolved_data = unsolved;
            this.solved_data = solved;
        }
    };
    static void Main(string[] args) {        
        var count = Int32.Parse(args[0]);
        var level = args[1];
        string file = args[2];

        int cutoff = 0;
        bool cutoffMode = false;
        if (Int32.TryParse(level, out cutoff))
            cutoffMode = true;

        StreamWriter writer = new StreamWriter(file, true);
        writer.WriteLine("[");
        while (count-- > 0)
        {
            var sudoku = (cutoffMode) ? Generator.Sudoku_Generator(Tuple.Create(81, cutoff)) : Generator.Sudoku_Generator(level);
            var board = new SudokuBoardData(sudoku.unsolved, sudoku.solved);
            var json = "{\"unsolved_data\":[";
            json += board.unsolved_data.Select(y => y.ToString()).Aggregate((a, b) => $"{a},{b}");
            json += "],";
            json += "\"solved_data\":[";
            json += board.solved_data.Select(y => y.ToString()).Aggregate((a, b) => $"{a},{b}");
            json += ("],\"difficulity\":"+ cutoff + "}" + ((count == 0) ? "" : ","));
            writer.WriteLine(json);
            Console.WriteLine(count.ToString() + "=> " + json);
        }
        writer.Write("]");
        writer.Close();        
    }
    

}