using Gomoku.Domain;

namespace Gomoku.Application;
public class RoomAppService
{
    private readonly IRoomRepository _roomRepository;

    public RoomAppService(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task<Guid> CreateRoomAsync(string roomName, Player owner)
    {
        var room = new Room(roomName);
        room.AddPlayer(owner);
        await _roomRepository.AddAsync(room);
        return room.Id;
    }

    public async Task<IEnumerable<Room>> GetAllPublicRoomsAsync()
        => await _roomRepository.GetAllPublicAsync();

    public async Task<Room?> GetRoomAsync(Guid roomId)
        => await _roomRepository.GetByIdAsync(roomId);

    public async Task<bool> JoinRoomAsync(Guid roomId, Player player)
    {
        var room = await _roomRepository.GetByIdAsync(roomId);
        if (room == null) return false;
        var ok = room.AddPlayer(player);
        if (ok) await _roomRepository.UpdateAsync(room);
        return ok;
    }
}