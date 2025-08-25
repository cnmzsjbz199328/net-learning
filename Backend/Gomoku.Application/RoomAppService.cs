using Gomoku.Domain; // 引入领域层命名空间，使用 Room、Player 类型

namespace Gomoku.Application; // 应用层命名空间

/// <summary>
/// 房间应用服务，封装房间相关的业务逻辑
/// </summary>
public class RoomAppService
{
    private readonly IRoomRepository _roomRepository; // 依赖注入仓储接口

    /// <summary>
    /// 构造函数，注入房间仓储
    /// </summary>
    /// <param name="roomRepository">房间仓储实现</param>
    public RoomAppService(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    /// <summary>
    /// 创建新房间，并添加房主
    /// </summary>
    /// <param name="roomName">房间名称</param>
    /// <param name="owner">房主玩家</param>
    /// <returns>新房间ID</returns>
    public async Task<Guid> CreateRoomAsync(string roomName, Player owner)
    {
        var room = new Room(roomName);      // 创建房间
        room.AddPlayer(owner);              // 添加房主
        await _roomRepository.AddAsync(room); // 持久化
        return room.Id;
    }

    /// <summary>
    /// 获取所有公开房间
    /// </summary>
    public async Task<IEnumerable<Room>> GetAllPublicRoomsAsync()
        => await _roomRepository.GetAllPublicAsync();

    /// <summary>
    /// 获取指定房间
    /// </summary>
    /// <param name="roomId">房间ID</param>
    public async Task<Room?> GetRoomAsync(Guid roomId)
        => await _roomRepository.GetByIdAsync(roomId);

    /// <summary>
    /// 玩家加入房间
    /// </summary>
    /// <param name="roomId">房间ID</param>
    /// <param name="player">玩家对象</param>
    /// <returns>是否加入成功</returns>
    public async Task<bool> JoinRoomAsync(Guid roomId, Player player)
    {
        var room = await _roomRepository.GetByIdAsync(roomId); // 查找房间
        if (room == null) return false;
        var ok = room.AddPlayer(player);                       // 尝试加入
        if (ok) await _roomRepository.UpdateAsync(room);       // 更新房间
        return ok;
    }
}