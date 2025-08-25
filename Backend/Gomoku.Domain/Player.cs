namespace Gomoku.Domain;
public record Player(Guid Id, string Nickname, bool IsBot = false);