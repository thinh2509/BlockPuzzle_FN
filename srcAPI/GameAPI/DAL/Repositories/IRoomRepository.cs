using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public interface IRoomRepository
    {
        Task<Room> CreateRoom(Room room);
        Task<Room?> GetById(string id);
        Task<Room?> GetByCode(string code);
        Task<List<Room>> GetWaitingRooms();
        Task UpdateRoom(Room room);
    }
}
