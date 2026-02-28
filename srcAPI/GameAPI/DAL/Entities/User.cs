using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace DAL.Entities
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("Email")]
        public string Email { get; set; } = null!;

        [BsonElement("PasswordHash")]
        public string PasswordHash { get; set; } = null!;

        [BsonElement("Name")]
        public string? Name { get; set; }
        [BsonElement("Address")]
        public string? Address { get; set; }

        [BsonElement("DateOfBirth")]
        public DateTime? DateOfBirth { get; set; }
        [BsonElement("Gender")]
        public string? Gender { get; set; }

        [BsonElement("HighScores")]
        public List<ScoreRecord> HighScores { get; set; } = new List<ScoreRecord>();
    }
}
