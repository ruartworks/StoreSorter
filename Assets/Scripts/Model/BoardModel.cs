
using System;

[Serializable]
public class BoardModel
{
    public int rows, cols;
    public CellModel[] cells;

    public BoardModel(int r, int c)
    {
        rows = r; cols = c;
        cells = new CellModel[r * c];
        for (int i = 0; i < cells.Length; i++) cells[i] = new CellModel();
    }

    public int Index(int row, int col) => row * cols + col;
    public CellModel Cell(int index) => cells[index];
}
