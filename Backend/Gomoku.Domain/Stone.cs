namespace Gomoku.Domain; // 定义命名空间为 Gomoku.Domain，用于组织领域相关的类

/// <summary>
/// 表示一颗落在棋盘上的棋子。使用 record 类型，因为棋子一旦落下，其属性（位置和归属）不应改变。
/// </summary>
/// <param name="X">棋子在棋盘上的 X 坐标（水平位置）。</param>
/// <param name="Y">棋子在棋盘上的 Y 坐标（垂直位置）。</param>
/// <param name="PlayerId">下这颗棋子的玩家的唯一标识符。</param>
public record Stone(int X, int Y, Guid PlayerId);
