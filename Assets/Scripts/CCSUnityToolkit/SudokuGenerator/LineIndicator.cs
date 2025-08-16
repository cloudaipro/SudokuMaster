using System.Linq;
using System.Collections;
using System.Collections.Generic;



public class LineIndicator
{
    public static LineIndicator Instance = new LineIndicator();
    public int[,] Line_data = new int[9, 9];
    public int[] Line_data_falt = new int[81];
    public int[,] Block_data = new int[9, 9];
    public LineIndicator()
    {
        int start_num = 0, iSquareRow = 0, iSquareRowSub = 0, iSquareCol = 0, iSquareColSub = 0;
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
            {
                Line_data[r, c] = start_num;
                Line_data_falt[start_num] = start_num;

                iSquareRow = r / 3;
                iSquareRowSub = r % 3;
                iSquareCol = c / 3;
                iSquareColSub = c % 3;
                Block_data[iSquareRow * 3 + iSquareCol, iSquareRowSub * 3 + iSquareColSub] = start_num;

                start_num++;
            }                  
    }

    public (int r, int c, int b) GetCellPosition(int square_index)
    {
        int row = (int)(square_index / 9);
        int col = square_index % 9;
        return (row, col, ((row / 3) * 3 + col / 3));        
    }

    public int[] GetHouse(int h) => (h < 9) ? Line_data.row(h) :
                                    (h < 18) ? Line_data.column(h - 9) :
                                               Block_data.row(h - 18);

    public int[] GetHorizontalLine(int cell_index) => Line_data.row(GetCellPosition(cell_index).Item1);
    public int[] GetVerticalLine(int cell_index) => Line_data.column(GetCellPosition(cell_index).Item2);
    public int[] GetBlockFlat(int cell_index)
    {
        var cellPos = GetCellPosition(cell_index);
        return Block_data.row(cellPos.b);
    }
    public int[] GetAllRelatedSudokuCell(int cell_index) =>
        GetBlockFlat(cell_index)
            .Union(GetHorizontalLine(cell_index))
            .Union(GetVerticalLine(cell_index)).Distinct().ToArray();

    public int[] GetAllSquaresIndexes() => Line_data_falt;
    public (int hLineIdx1, int hLineIdx2, int vLineIdx1, int vLineIdx2) GetExcludeLineIndices(int cell_index)
    {
        int hLineIdx1, hLineIdx2, vLineIdx1, vLineIdx2;        
        var r = ((int)(cell_index / 9)) % 3;
        if (r == 0)
        {
            hLineIdx1 = cell_index + 9;
            hLineIdx2 = cell_index + 18;
        }
        else if (r == 1)
        {
            hLineIdx1 = cell_index - 9;
            hLineIdx2 = cell_index + 9;
        }
        else
        {
            hLineIdx1 = cell_index - 18;
            hLineIdx2 = cell_index - 9;
        }
        var c = cell_index % 3;
        if (c == 0)
        {
            vLineIdx1 = cell_index + 1;
            vLineIdx2 = cell_index + 2;
        }
        else if (c == 1)
        {
            vLineIdx1 = cell_index - 1;
            vLineIdx2 = cell_index + 1;
        }
        else
        {
            vLineIdx1 = cell_index - 2;
            vLineIdx2 = cell_index - 1;
        }
        return (hLineIdx1, hLineIdx2, vLineIdx1, vLineIdx2);
    }
}
