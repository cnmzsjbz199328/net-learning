using Microsoft.AspNetCore.SignalR;
using Gomoku.Application;

namespace Gomoku.Api;

/// <summary>
/// 五子棋实时对战Hub,处理客户端的实时请求
/// </summary>
public class GomokuHub : Hub
{
    private readonly RoomAppService _roomService;

    public GomokuHub(RoomAppService roomService)
    {
        _roomService = roomService;
    }

    /// <summary>
    /// 客户端调用：加入房间
    /// </summary>
    /// <param name="roomId">房间ID</param>
    /// <param name="nickname">用户昵称</param>
    public async Task JoinRoom(string roomId, string nickname)
    {
        // Context.ConnectionId 是每个连接到Hub的客户端的唯一标识
        // Groups.AddToGroupAsync 将当前客户端连接添加到一个组中，组名就是房间ID
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        // Clients.Group(roomId) 向指定组中的所有客户端发送消息
        // SendAsync("PlayerJoined", nickname) 异步发送一个名为 "PlayerJoined" 的消息，并附带昵称参数
        await Clients.Group(roomId).SendAsync("PlayerJoined", nickname);
    }

    /// <summary>
    /// 客户端调用：落子
    /// </summary>
    /// <param name="roomId">房间ID</param>
    /// <param name="x">X坐标</param>
    /// <param name="y">Y坐标</param>
    /// <param name="playerId">玩家ID</param>
    public async Task PlaceStone(string roomId, int x, int y, string playerId)
    {
        // 业务逻辑：更新棋盘、切换回合、广播棋盘状态
        // 这里只做演示，实际应调用 RoomAppService 处理
        // Clients.Group(roomId) 向指定组中的所有客户端发送消息
        // SendAsync("BoardUpdated", x, y, playerId) 异步发送一个名为 "BoardUpdated" 的消息，并附带落子信息
        await Clients.Group(roomId).SendAsync("BoardUpdated", x, y, playerId);
    }
}