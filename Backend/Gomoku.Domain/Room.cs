using System.Collections.Generic;

namespace Gomoku.Domain;
public class Room
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; }
    public List<Player> Players { get; private set; } = new();
    public Board GameBoard { get; private set; } = new();
    public Room(string name) => Name = name;

    public bool AddPlayer(Player player)
    {
        if (Players.Count >= 2) return false;
        Players.Add(player);
        return true;
    }

    // 可扩展：StartGame、RemovePlayer、房间状态等
}