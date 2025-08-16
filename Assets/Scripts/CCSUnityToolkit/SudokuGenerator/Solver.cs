using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    public class Solver
    {
        private Board board;
        private List<Cell> vacants;
    // constructor for a solver, keeps a local copy of provided board
    public Solver(Board board)
    {
        this.board = board.copy();
        this.vacants = this.board.get_unused_cells();
    }

    // checks to make sure each compartment contains
    public bool is_valid()
    {
        HashSet<int> valid = new HashSet<int>(Enumerable.Range(1, 9));
        foreach (var box in this.board.boxes.Values)
        {
            if (!valid.SetEquals(box.Select(x => x.Value)))
            {
                return false;
            }
        }
        foreach (var row in this.board.rows.Values)
        {
            if (!valid.SetEquals(row.Select(x => x.Value)))
            {
                return false;
            }
        }
        foreach (var col in this.board.columns.Values)
        {
            if (!valid.SetEquals(col.Select(x => x.Value)))
            {
                return false;
            }
        }
        return true;
    }

    // solves a puzzle by moving forward and backwards through puzzle
    // and testing values
    public bool solve()
    {
        int index = 0;
        while (-1 < index && index < this.vacants.Count)
        {
            Cell current = this.vacants[index];
            bool flag = false;
            var my_range = Enumerable.Range(current.Value + 1, 9 - current.Value);
            foreach (var x in my_range)
            {
                if (this.board.get_possibles(current).Contains(x))
                {
                    current.Value = x;
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                current.Value = 0;
                index -= 1;
            }
            else
            {
                index += 1;
            }
        }
        if (this.vacants.Count == 0)
        {
            return false;
        }
        else
        {
            return index == this.vacants.Count;
        }
    }
}
