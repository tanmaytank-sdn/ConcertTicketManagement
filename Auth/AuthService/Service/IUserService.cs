using AuthService.Models;

namespace AuthService.Service
{
    public interface IUserService
    {
        Task<(bool Success, string? ErrorMessage, string? Token, object? UserInfo)> AuthenticateAsync(LoginRequest request);
        Task<(bool Success, string? ErrorMessage)> AddOrUpdateUserAsync(AddOrUpdateUserRequest request);
    }
}
