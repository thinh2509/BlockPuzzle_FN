using DAL.Entities;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
    
namespace DAL.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly IMongoCollection<Room> _rooms;

        public RoomRepository(IOptions<MongoDBSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _rooms = database.GetCollection<Room>("Rooms");
        }
        public async Task<Room?> GetById(string id)
        {
            return await _rooms.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Room> CreateRoom(Room room)
        {
            await _rooms.InsertOneAsync(room);
            return room;
        }

        public async Task<Room?> GetByCode(string code)
        {
            return await _rooms.Find(x => x.RoomCode == code).FirstOrDefaultAsync();
        }

        public async Task<List<Room>> GetWaitingRooms()
        {
            return await _rooms.Find(x => x.Status == "Waiting").ToListAsync();
        }

        public async Task UpdateRoom(Room room)
        {
            await _rooms.ReplaceOneAsync(x => x.Id == room.Id, room);
        }
    }
}
