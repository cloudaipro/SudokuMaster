
using System;
using System.Collections.Generic;
using System.Linq;

public class Board
{
    public List<Cell> cells { get; set; }

    // Dictionary of rows
    public Dictionary<int, List<Cell>> rows { get; set; }

    // Dictionary of columns
    public Dictionary<int, List<Cell>> columns { get; set; }

    // Dictionary of boxes
    public Dictionary<int, List<Cell>> boxes { get; set; }
    // initializing a board
    public Board(List<int> numbers = null)
    {
        // we keep list of cells and dictionaries to point to each cell
        // by various locations
        this.rows = new Dictionary<int, List<Cell>>();
        this.columns = new Dictionary<int, List<Cell>>();
        this.boxes = new Dictionary<int, List<Cell>>();
        this.cells = new List<Cell>();


        // looping rows
        for (int row = 0; row < 9; row++)
        {
            // looping columns
            for (int col = 0; col < 9; col++)
            {
                // calculating box
                int box = 3 * (row / 3) + (col / 3);

                // creating cell instance
                Cell cell = new Cell(row, col, box);

                // if initial set is given, set cell value
                if (numbers != null && numbers.Count > 0)
                {
                    cell.Value = numbers[0];
                    numbers.RemoveAt(0);
                }

                // initializing dictionary keys and corresponding lists
                // if they are not initialized
                if (!this.rows.ContainsKey(row))
                {
                    this.rows[row] = new List<Cell>();
                }
                if (!this.columns.ContainsKey(col))
                {
                    this.columns[col] = new List<Cell>();
                }
                if (!this.boxes.ContainsKey(box))
                {
                    this.boxes[box] = new List<Cell>();
                }

                // adding cells to each list
                this.rows[row].Add(cell);
                this.columns[col].Add(cell);
                this.boxes[box].Add(cell);
                this.cells.Add(cell);
            }
        }
    }

    // returning cells in puzzle that are not set to zero
    public List<Cell> get_used_cells()
    {
        return this.cells.Where(x => x.Value != 0).ToList();
    }

    // returning cells in puzzle that are set to zero
    public List<Cell> get_unused_cells()
    {
        return this.cells.Where(x => x.Value == 0).ToList();
    }

    // returning all possible values that could be assigned to the
    // cell provided as argument
    public List<int> get_possibles(Cell cell)
    {
        List<Cell> possibilities = this.rows[cell.Row].Concat(this.columns[cell.Col]).Concat(this.boxes[cell.Box]).ToList();
        HashSet<int> excluded = new HashSet<int>(possibilities.Where(x => x.Value != 0 && x.Value != cell.Value).Select(x => x.Value));
        List<int> results = Enumerable.Range(1, 9).Except(excluded).ToList();
        return results;
    }

    // calculates the density of a specific cell's context
    public double get_density(Cell cell)
    {
        var possibilities = rows[cell.Row].Concat(columns[cell.Col]).Concat(boxes[cell.Box]).ToList();
        if (cell.Value != 0)
        {
            possibilities.Remove(cell);
        }
        return possibilities.Where(x => x.Value != 0).Distinct().Count() / 20.0;
    }

    public HashSet<int> get_excluded(Cell cell)
    {
        var possibilities = rows[cell.Row].Concat(columns[cell.Col]).Concat(boxes[cell.Box]).ToList();
        return new HashSet<int>(possibilities.Where(x => x.Value != 0 && x.Value != cell.Value).Select(x => x.Value));
    }

    public void swap_row(int rowIndex1, int rowIndex2, bool allow = false)
    {
        if (allow || rowIndex1 / 3 == rowIndex2 / 3)
        {
            for (int i = 0; i < rows[rowIndex2].Count; i++)
            {
                int temp = rows[rowIndex1][i].Value;
                rows[rowIndex1][i].Value = rows[rowIndex2][i].Value;
                rows[rowIndex2][i].Value = temp;
            }
        }
        else
        {
            throw new Exception("Tried to swap non-familial rows.");
        }
    }
    public void swap_column(int col_index1, int col_index2, bool allow = false)
    {
        if (allow || col_index1 / 3 == col_index2 / 3)
        {
            for (int x = 0; x < this.columns[col_index2].Count; x++)
            {
                int temp = this.columns[col_index1][x].Value;
                this.columns[col_index1][x].Value = this.columns[col_index2][x].Value;
                this.columns[col_index2][x].Value = temp;
            }
        }
        else
        {
            throw new Exception("Tried to swap non-familial columns.");
        }
    }

    public void swap_stack(int stack_index1, int stack_index2)
    {
        for (int x = 0; x < 3; x++)
        {
            this.swap_column(stack_index1 * 3 + x, stack_index2 * 3 + x, true);
        }
    }

    public void swap_band(int band_index1, int band_index2)
    {
        for (int x = 0; x < 3; x++)
        {
            this.swap_row(band_index1 * 3 + x, band_index2 * 3 + x, true);
        }
    }

    public Board copy()
    {
        Board b = new Board();
        for (int row = 0; row < this.rows.Count; row++)
        {
            for (int col = 0; col < this.columns.Count; col++)
            {
                b.rows[row][col].Value = this.rows[row][col].Value;
            }
        }
        return b;
    }

    public override string ToString()
    {
        List<string> output = new List<string>();
        foreach (int index in rows.Keys)
        {
            List<string> my_set = rows[index].Select(x => x.Value.ToString()).ToList();
            List<string> new_set = new List<string>();
            foreach (string x in my_set)
            {
                if (x == "0")
                {
                    new_set.Add("_");
                }
                else
                {
                    new_set.Add(x);
                }
            }
            output.Add(String.Join("|", new_set));
        }
        return String.Join(Environment.NewLine, output);
    }
    public string ToHtml()
    {
        string html = "<table>";
        foreach (int index in rows.Keys)
        {
            List<string> values = new List<string>();
            string row_string = "<tr>";
            foreach (Cell cell in rows[index])
            {
                if (cell.Value == 0)
                {
                    values.Add(" ");
                    row_string += "<td>%s</td>";
                }
                else
                {
                    values.Add(cell.Value.ToString());
                    row_string += "<td>%d</td>";
                }
            }
            row_string += "</tr>";
            html += String.Format(row_string, values.ToArray());
        }
        html += "</table>";
        return html;
    }
}

    