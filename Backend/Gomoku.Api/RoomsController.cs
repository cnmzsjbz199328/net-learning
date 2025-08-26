using Microsoft.AspNetCore.Mvc;
using Gomoku.Application;
using Gomoku.Domain;

namespace Gomoku.Api;

/// <summary>
/// 房间相关API控制器
/// </summary>
// ApiController属性启用了一系列API特定的行为，例如自动模型验证和更详细的问题详细信息
[ApiController]
// Route属性定义了此控制器中所有操作的路由模板
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
    // HttpGet属性表示此方法处理HTTP GET请求
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var rooms = await _roomService.GetAllPublicRoomsAsync();
        // 可根据需要映射为DTO
        // Ok()方法创建一个包含200 OK状态码和响应体的IActionResult
        return Ok(rooms.Select(r => new { r.Id, r.Name, PlayerCount = r.Players.Count }));
    }

    /// <summary>
    /// 创建新房间
    /// </summary>
    // HttpPost属性表示此方法处理HTTP POST请求
    [HttpPost]
    // FromBody属性告诉框架从请求正文中获取CreateRoomDto的值
    public async Task<IActionResult> Create([FromBody] CreateRoomDto dto)
    {
        // 这里简化为匿名玩家，后续可集成认证
        var owner = new Player(Guid.NewGuid(), dto.OwnerNickname);
        var roomId = await _roomService.CreateRoomAsync(dto.RoomName, owner);
        // CreatedAtAction方法创建一个201 Created响应，该响应包含一个Location头，指向新创建的资源
        return CreatedAtAction(nameof(GetAll), new { id = roomId }, new { roomId });
    }
}

/// <summary>
/// 创建房间请求DTO
/// </summary>
public record CreateRoomDto(string RoomName, string OwnerNickname);