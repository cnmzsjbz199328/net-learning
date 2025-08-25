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
    private static readonly ConcurrentDictionary<Guid, Room> _rooms = new();

    public Task<Room?> GetByIdAsync(Guid id)
        => Task.FromResult(_rooms.TryGetValue(id, out var room) ? room : null);

    public Task<IEnumerable<Room>> GetAllPublicAsync()
        => Task.FromResult(_rooms.Values.AsEnumerable());

    public Task AddAsync(Room room)
    {
        _rooms[room.Id] = room;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Room room)
    {
        _rooms[room.Id] = room;
        return Task.CompletedTask;
    }
}