using System.Collections.Concurrent;
using Gomoku.Domain;
using Gomoku.Application;

namespace Gomoku.Infrastructure;

/// <summary>
/// MVP阶段的内存房间仓储实现，线程安全，服务重启后数据丢失
/// </summary>
public class InMemoryRoomRepository : IRoomRepository
{
    // 使用线程安全的字典存储所有房间
    // ConcurrentDictionary是线程安全的集合，适用于多线程环境
    private static readonly ConcurrentDictionary<Guid, Room> _rooms = new();

    /// <summary>
    /// 异步获取指定ID的房间
    /// </summary>
    /// <param name="id">房间ID</param>
    /// <returns>如果找到房间则返回房间对象，否则返回null</returns>
    public Task<Room?> GetByIdAsync(Guid id)
        // Task.FromResult创建一个已完成的Task，其结果为指定的值
        => Task.FromResult(_rooms.TryGetValue(id, out var room) ? room : null);

    /// <summary>
    /// 异步获取所有公开房间
    /// </summary>
    /// <returns>所有公开房间的集合</returns>
    public Task<IEnumerable<Room>> GetAllPublicAsync()
        // Task.FromResult创建一个已完成的Task，其结果为指定的值
        => Task.FromResult(_rooms.Values.AsEnumerable());

    /// <summary>
    /// 异步添加一个新房间
    /// </summary>
    /// <param name="room">要添加的房间</param>
    /// <returns>表示异步操作的Task</returns>
    public Task AddAsync(Room room)
    {
        _rooms[room.Id] = room;
        // Task.CompletedTask返回一个已成功完成的Task
        return Task.CompletedTask;
    }

    /// <summary>
    /// 异步更新一个现有房间
    /// </summary>
    /// <param name="room">要更新的房间</param>
    /// <returns>表示异步操作的Task</returns>
    public Task UpdateAsync(Room room)
    {
        _rooms[room.Id] = room;
        // Task.CompletedTask返回一个已成功完成的Task
        return Task.CompletedTask;
    }
}