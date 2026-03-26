using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public interface IRoomService
    {
        Task<Room> CreateRoom(string userId);
        Task<Room?> JoinRoom(string roomCode, string userId);
        Task<Room?> QuickMatch(string userId);
        Task<Room?> GetRoomById(string roomId);
        Task<Room?> GetRoomByCode(string roomCode);
        Task UpdateScore(string roomId, string userId, int score);
        Task AddScore(string roomId, string userId, int score);
    }
}
