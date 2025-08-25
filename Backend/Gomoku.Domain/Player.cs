namespace Gomoku.Domain; // 定义命名空间为 Gomoku.Domain，用于组织领域相关的类

/// <summary>
/// 表示一个玩家。使用 record 类型，它是一种特殊的类，主要用于存储数据。
/// </summary>
/// <param name="Id">玩家的唯一标识符，使用 GUID 以确保全局唯一。</param>
/// <param name="Nickname">玩家的昵称，用于显示。</param>
/// <param name="IsBot">一个布尔值，指示该玩家是否为机器人（AI）。默认为 false。</param>
public record Player(Guid Id, string Nickname, bool IsBot = false);
