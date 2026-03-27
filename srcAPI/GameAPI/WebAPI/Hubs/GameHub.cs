using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace WebAPI.Hubs
{
    public class GameHub : Hub
    {
        // Khi một user join vào phòng
        public async Task JoinRoom(string roomCode)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
            // Có thể báo cho các client khác trong phòng biết có người vào
            await Clients.Group(roomCode).SendAsync("PlayerJoined", Context.ConnectionId);
        }

        // Khi một user rời phòng
        public async Task LeaveRoom(string roomCode)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomCode);
            await Clients.Group(roomCode).SendAsync("PlayerLeft", Context.ConnectionId);
        }

        // Cập nhật điểm số
        public async Task SendScoreUpdate(string roomCode, int newScore)
        {
            // Gửi cho TẤT CẢ các client khác trong cùng phòng (trừ người gửi)
            await Clients.OthersInGroup(roomCode).SendAsync("UpdateOpponentScore", newScore);
        }

        // Thông báo kết thúc game
        public async Task SendGameOver(string roomCode, string winnerConnectionId)
        {
            await Clients.Group(roomCode).SendAsync("GameOver", winnerConnectionId);
        }
    }
}
