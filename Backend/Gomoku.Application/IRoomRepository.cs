using Gomoku.Domain; // 引入领域层命名空间，使用 Room 类型

namespace Gomoku.Application; // 应用层命名空间

/// <summary>
/// 房间仓储接口，定义房间相关的数据访问操作（依赖倒置，便于切换实现）
/// </summary>
public interface IRoomRepository
{
    /// <summary>
    /// 根据房间ID获取房间
    /// </summary>
    /// <param name="id">房间唯一标识</param>
    /// <returns>房间对象或null</returns>
    Task<Room?> GetByIdAsync(Guid id);

    /// <summary>
    /// 获取所有公开房间（如大厅房间列表）
    /// </summary>
    /// <returns>房间集合</returns>
    Task<IEnumerable<Room>> GetAllPublicAsync();

    /// <summary>
    /// 新增房间
    /// </summary>
    /// <param name="room">房间对象</param>
    Task AddAsync(Room room);

    /// <summary>
    /// 更新房间（如玩家加入、棋盘变化等）
    /// </summary>
    /// <param name="room">房间对象</param>
    Task UpdateAsync(Room room);
}