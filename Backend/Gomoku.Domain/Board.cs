namespace Gomoku.Domain; // 定义命名空间为 Gomoku.Domain，用于组织领域相关的类

/// <summary>
/// 表示一个五子棋棋盘，负责管理棋盘状态和落子逻辑。
/// </summary>
public class Board
{
    /// <summary>
    /// 定义棋盘的大小为一个 15x15 的常量。const 关键字表示这是一个编译时常量。
    /// </summary>
    public const int Size = 15;

    /// <summary>
    /// 使用一个二维整型数组来存储棋盘的状态。
    /// private 表示它只能在 Board 类内部访问。
    /// readonly 表示这个数组实例在构造后不能被替换（但其内部元素可以被修改）。
    /// 0 通常表示空位，其他数字（如 1 和 2）可以代表不同颜色的棋子。
    /// </summary>
    private readonly int[,] _stones = new int[Size, Size];

    /// <summary>
    /// 提供一个公共的、只读的棋盘状态视图。
    /// 这是一个计算属性，每次访问时，它都会返回 _stones 数组的一个浅克隆（Clone）。
    /// 这样做可以防止外部代码直接修改内部的 _stones 数组，从而保护了棋盘状态的完整性。
    /// </summary>
    public int[,] Stones => (int[,])_stones.Clone();

    /// <summary>
    /// 在棋盘上放置一颗棋子。
    /// </summary>
    /// <param name="x">要放置的 X 坐标。</param>
    /// <param name="y">要放置的 Y 坐标。</param>
    /// <param name="color">代表棋子颜色的整数（例如，1 代表黑棋，2 代表白棋）。</param>
    /// <returns>如果成功放置返回 true，如果该位置已有棋子则返回 false。</returns>
    public bool PlaceStone(int x, int y, int color)
    {
        // 检查目标位置是否已经有棋子（值不为 0）。
        if (_stones[x, y] != 0) return false; // 如果是，则落子失败，返回 false。

        // 将指定颜色的棋子放置在目标位置。
        _stones[x, y] = color;

        // 落子成功，返回 true。
        return true;
    }

    // 这是一个占位符注释，提醒开发者未来可以在这里添加更多功能。
    // 可扩展：胜负判断、消除逻辑等
}
