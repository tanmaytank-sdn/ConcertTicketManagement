using AuthService.Abstract;
using AuthService.Models;
using Microsoft.EntityFrameworkCore;
using Solution.Core.Entity;
using Solution.Persistence;
using System.Security.Claims;

namespace AuthService.Service
{
    public class UserService : IUserService
    {
        private readonly UserDbContext _dbContext;
        private readonly ITokenService _tokenService;

        public UserService(UserDbContext dbContext, ITokenService tokenService)
        {
            _dbContext = dbContext;
            _tokenService = tokenService;
        }

        public async Task<(bool Success, string? ErrorMessage, string? Token, object? UserInfo)> AuthenticateAsync(LoginRequest request)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.Password == request.Password);

            if (user == null)
            {
                return (false, "Invalid username or password.", null, null);
            }

            var claims = new List<Claim>
            {
                new Claim("UserId", user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("name", user.FullName)
            };

            var token = _tokenService.GenerateToken(claims);

            return (true, null, token, new { user.Id, user.Username, user.Role });
        }

        public async Task<(bool Success, string? ErrorMessage)> AddOrUpdateUserAsync(AddOrUpdateUserRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                    return (false, "Username and password are required.");

                var existingUser = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Id == request.Id || u.Username == request.Username);

                if (existingUser != null && (request.Id != null && request.Id != 0))
                {
                    existingUser.Email = request.Email;
                    existingUser.FullName = request.FullName;
                    existingUser.Role = request.Role;
                    existingUser.Password = request.Password;

                    _dbContext.Users.Update(existingUser);
                }
                else if (existingUser == null)
                {
                    var newUser = new Users
                    {
                        Username = request.Username,
                        Email = request.Email,
                        FullName = request.FullName,
                        Role = request.Role,
                        Password = request.Password,
                        CreatedAt = DateTime.UtcNow
                    };

                    _dbContext.Users.Add(newUser);
                }
                else
                {
                    return (false, "User already exists.");
                }

                await _dbContext.SaveChangesAsync();
                return (true, null);
            }
            catch
            {
                return (false, "Internal Server Error");
            }
        }
    }
}
