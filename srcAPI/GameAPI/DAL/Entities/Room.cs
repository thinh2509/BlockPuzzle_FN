using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Room
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string RoomCode { get; set; }

        public string? Player1Id { get; set; }

        public string? Player2Id { get; set; }

        public int Player1Score { get; set; } = 0;   
        public int Player2Score { get; set; } = 0;   

        public string Status { get; set; } // Waiting, Full, Playing

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
