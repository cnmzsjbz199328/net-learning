namespace Gomoku.Domain;
public class Board
{
    public const int Size = 15;
    private readonly int[,] _stones = new int[Size, Size];

    public int[,] Stones => (int[,])_stones.Clone();

    public bool PlaceStone(int x, int y, int color)
    {
        if (_stones[x, y] != 0) return false;
        _stones[x, y] = color;
        return true;
    }

    // 可扩展：胜负判断、消除逻辑等
}