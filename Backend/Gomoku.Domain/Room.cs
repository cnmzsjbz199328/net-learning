using System.Collections.Generic; // 引入泛型集合的命名空间，以便使用 List<T>。

namespace Gomoku.Domain; // 定义命名空间为 Gomoku.Domain，用于组织领域相关的类

/// <summary>
/// 表示一个游戏房间，是游戏的核心聚合。它管理玩家、棋盘和游戏流程。
/// </summary>
public class Room
{
    /// <summary>
    /// 房间的唯一标识符。
    /// public 表示可以从外部读取。
    /// private set 表示只能在 Room 类内部设置。
    /// Guid.NewGuid() 会在创建 Room 实例时为其生成一个全新的、唯一的 ID。
    /// </summary>
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// 房间的名称。
    /// private set 确保房间名称在创建后不能从外部更改。
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// 在这个房间里的玩家列表。
    /// private set 防止外部代码替换整个列表，但仍然可以通过 AddPlayer 方法添加玩家。
    /// </summary>
    public List<Player> Players { get; private set; } = new();

    /// <summary>
    /// 房间内的游戏棋盘实例。
    /// private set 确保棋盘实例在房间创建后不会被替换。
    /// </summary>
    public Board GameBoard { get; private set; } = new();

    /// <summary>
    /// Room 类的构造函数。
    /// 当创建一个新的 Room 实例时，必须提供一个房间名称。
    /// 这是一个表达式体构造函数（Expression-bodied constructor），是 C# 6.0 的语法糖。
    /// </summary>
    /// <param name="name">要创建的房间的名称。</param>
    public Room(string name) => Name = name;

    /// <summary>
    /// 向房间中添加一个玩家。
    /// </summary>
    /// <param name="player">要添加的玩家对象。</param>
    /// <returns>如果成功添加返回 true，如果房间已满（已有8名玩家）则返回 false。</returns>
    public bool AddPlayer(Player player)
    {
        // 检查当前玩家数量是否已经达到上限（8人）。
        if (Players.Count >= 8) return false; // 如果是，则添加失败，返回 false。

        // 将新玩家添加到玩家列表中。
        Players.Add(player);

        // 添加成功，返回 true。
        return true;
    }

    // 这是一个占位符注释，提醒开发者未来可以在这里添加更多功能。
    // 可扩展：StartGame、RemovePlayer、房间状态等
}
