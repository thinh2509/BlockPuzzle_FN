using DAL.Entities;
using DAL.Repositories;

namespace BLL.Services
{
    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _roomRepo;
        

        public RoomService(IRoomRepository roomRepo)
        {
            _roomRepo = roomRepo;
        }

        public async Task<Room> CreateRoom(string userId)
        {
            var room = new Room
            {
                RoomCode = GenerateCode(),
                Player1Id = userId,
                Status = "Waiting"
            };

            return await _roomRepo.CreateRoom(room);
        }

        public async Task<Room?> JoinRoom(string roomCode, string userId)
        {
            var room = await _roomRepo.GetByCode(roomCode);

            if (room == null || room.Status != "Waiting")
                return null;

            room.Player2Id = userId;
            room.Status = "Full";

            await _roomRepo.UpdateRoom(room);

            return room;
        }

        public async Task<Room?> QuickMatch(string userId)
        {
            var waitingRooms = await _roomRepo.GetWaitingRooms();

            var room = waitingRooms.FirstOrDefault();

            if (room != null)
            {
                room.Player2Id = userId;
                room.Status = "Full";
                await _roomRepo.UpdateRoom(room);
                return room;
            }

            return await CreateRoom(userId);
        }
        public async Task UpdateScore(string roomId, string userId, int score)
        {
            // 1️⃣ Lấy Room từ MongoDB
            var room = await _roomRepo.GetById(roomId);

            // 2️⃣ Kiểm tra room tồn tại không
            if (room == null)
                throw new Exception("Room not found");

            // 3️⃣ Kiểm tra user là Player1 hay Player2
            if (room.Player1Id == userId)
            {
                room.Player1Score = score;   // cập nhật điểm player 1
            }
            else if (room.Player2Id == userId)
            {
                room.Player2Score = score;   // cập nhật điểm player 2
            }
            else
            {
                throw new Exception("User not in this room");
            }

            // 4️⃣ Lưu lại vào Mongo (ReplaceOneAsync)
            await _roomRepo.UpdateRoom(room);
        }
        public async Task<Room> GetRoomByCode(string roomCode)
        {
            return await _roomRepo.GetByCode(roomCode);
                

        }
        public async Task<Room?> GetRoomById(string roomId)
        {
            return await _roomRepo.GetById(roomId);
        }
        private string GenerateCode()
        {
            return new Random().Next(100000, 999999).ToString();
        }
    }
}
