using AuthService.Models;
using AuthService.Service;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserAuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] LoginRequest request)
        {
            var result = await _userService.AuthenticateAsync(request);

            if (!result.Success)
                return Unauthorized(result.ErrorMessage);

            return Ok(new
            {
                Token = result.Token,
                User = result.UserInfo
            });
        }

        [HttpPost("add-update")]
        public async Task<IActionResult> AddOrUpdateUser([FromBody] AddOrUpdateUserRequest request)
        {
            var result = await _userService.AddOrUpdateUserAsync(request);

            if (!result.Success)
                return BadRequest(result.ErrorMessage);

            return Ok("User added/updated successfully.");
        }
    }
}
