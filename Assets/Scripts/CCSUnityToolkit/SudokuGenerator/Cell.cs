using System;

public class Cell
{
    public int Row { get; set; }
    public int Col { get; set; }
    public int Box { get; set; }
    public int Value { get; set; }
    public Cell(int row, int col, int box)
    {
        Row = row;
        Col = col;
        Box = box;
        Value = 0;
    }

    // returns a string representation of cell (for debugging)
    public override string ToString()
    {
        return string.Format("Value: {0}, Row: {1}, Col: {2}, Box: {3}", Value, Row, Col, Box);
    }
    
}