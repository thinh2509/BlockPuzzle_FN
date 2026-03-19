using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using System;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public RoomController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] string userId)
        {
            var room = await _roomService.CreateRoom(userId);
            return Ok(room);
        }
        [HttpGet("{roomCode}")]
        public async Task<IActionResult> GetRoom(string roomCode)
        {
            var room = await _roomService.GetRoomByCode(roomCode);

            if (room == null)
                return NotFound("Room not found");

            return Ok(room);
        }

        [HttpPost("join")]
        public async Task<IActionResult> Join([FromForm] string roomCode, [FromForm] string userId)
        {
            var room = await _roomService.JoinRoom(roomCode, userId);

            if (room == null)
                return BadRequest("Room not available");

            return Ok(room);
        }

        [HttpPost("quick-match")]
        public async Task<IActionResult> QuickMatch(string userId)
        {
            var room = await _roomService.QuickMatch(userId);
            return Ok(room);
        }
        [HttpPost("update-score")]

public async Task<IActionResult> UpdateScore([FromBody] UpdateScoreRequest request)
{
    // 1️⃣ Kiểm tra request null
    if (request == null)
        return BadRequest("Request is null");

    // 2️⃣ Validate dữ liệu cơ bản
    if (string.IsNullOrEmpty(request.RoomId) ||
        string.IsNullOrEmpty(request.UserId))
    {
        return BadRequest("RoomId and UserId are required");
    }

    try
    {
        // 3️⃣ Gọi service
        await _roomService.UpdateScore(
            request.RoomId,
            request.UserId,
            request.Score
        );

        // 4️⃣ Trả thành công
        return Ok(new
        {
            message = "Score updated successfully"
        });
    }
    catch (Exception ex)
    {
        return BadRequest(new
        {
            error = ex.Message
        });
    }
}

        [HttpPut("add-score")]
        public async Task<IActionResult> AddScore([FromBody] UpdateScoreRequest request)
        {
            if (request == null)
                return BadRequest("Request is null");

            if (string.IsNullOrEmpty(request.RoomId) ||
                string.IsNullOrEmpty(request.UserId))
            {
                return BadRequest("RoomId and UserId are required");
            }

            try
            {
                await _roomService.AddScore(
                    request.RoomId,
                    request.UserId,
                    request.Score
                );

                return Ok(new
                {
                    message = "Score added successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    error = ex.Message
                });
            }
        }

        [HttpGet("{roomId}/score")]
        public async Task<IActionResult> GetScore(string roomId)
        {
            var room = await _roomService.GetRoomById(roomId);

            if (room == null)
                return NotFound("Room not found");

            return Ok(new
            {
                Player1Score = room.Player1Score,
                Player2Score = room.Player2Score
            });
        }
    }
}
