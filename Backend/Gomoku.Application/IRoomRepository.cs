using Gomoku.Domain;

namespace Gomoku.Application;
public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(Guid id);
    Task<IEnumerable<Room>> GetAllPublicAsync();
    Task AddAsync(Room room);
    Task UpdateAsync(Room room);
}