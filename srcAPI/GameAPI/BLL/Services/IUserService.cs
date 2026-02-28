using DAL.Entities;
using System.Threading.Tasks;
using WebAPI.DTOs;

namespace BLL.Services
{
    public interface IUserService
    {
        Task<User> RegisterAsync(RegisterDto registerDto);
        Task<string?> LoginAsync(string email, string password);
        string GenerateJwtToken(User user);
    }
}