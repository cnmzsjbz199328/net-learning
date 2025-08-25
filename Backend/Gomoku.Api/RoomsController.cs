using Microsoft.AspNetCore.Mvc;
using Gomoku.Application;
using Gomoku.Domain;

namespace Gomoku.Api;

/// <summary>
/// 房间相关API控制器
/// </summary>
[ApiController]
[Route("api/rooms")]
public class RoomsController : ControllerBase
{
    private readonly RoomAppService _roomService;

    public RoomsController(RoomAppService roomService)
    {
        _roomService = roomService;
    }

    /// <summary>
    /// 获取所有公开房间
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var rooms = await _roomService.GetAllPublicRoomsAsync();
        // 可根据需要映射为DTO
        return Ok(rooms.Select(r => new { r.Id, r.Name, PlayerCount = r.Players.Count }));
    }

    /// <summary>
    /// 创建新房间
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoomDto dto)
    {
        // 这里简化为匿名玩家，后续可集成认证
        var owner = new Player(Guid.NewGuid(), dto.OwnerNickname);
        var roomId = await _roomService.CreateRoomAsync(dto.RoomName, owner);
        return CreatedAtAction(nameof(GetAll), new { id = roomId }, new { roomId });
    }
}

/// <summary>
/// 创建房间请求DTO
/// </summary>
public record CreateRoomDto(string RoomName, string OwnerNickname);