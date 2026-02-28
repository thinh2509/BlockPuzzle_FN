using DAL.Entities;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _usersCollection;

        public UserRepository(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var mongoClient = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _usersCollection = mongoDatabase.GetCollection<User>("Users");
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _usersCollection.Find(u => u.Email == email).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(User user)
        {
            await _usersCollection.InsertOneAsync(user);
        }
    }
}
