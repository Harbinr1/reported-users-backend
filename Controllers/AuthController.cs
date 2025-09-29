using Microsoft.AspNetCore.Mvc;
using ReportedUsersSystem.DTOs;
using ReportedUsersSystem.Services;

namespace ReportedUsersSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterDto userDto)
    {
        try
        {
            var user = await _authService.Register(userDto);
            return Ok(user);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginDto userDto)
    {
        try
        {
            var token = await _authService.Login(userDto);
            return Ok(new { Token = token });
        }
        catch (Exception ex)
        {
            return Unauthorized(ex.Message);
        }
    }
}