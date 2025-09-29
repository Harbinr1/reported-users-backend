using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using RentACarReport.Models;
using ReportedUsersSystem.Data;
using ReportedUsersSystem.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ReportedUsersSystem.Services;

public class AuthService
{
    private readonly MongoDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public AuthService(MongoDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task<UserResponseDto> Register(UserRegisterDto userDto)
    {
        // Check if user already exists
        var existingUser = await _dbContext.Users.Find(u => u.Email == userDto.Email).FirstOrDefaultAsync();
        if (existingUser != null)
        {
            throw new Exception("User with this email already exists.");
        }

        // Hash password
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

        bool isAdmin = false;
        if (!string.IsNullOrEmpty(userDto.AdminSecret) && userDto.AdminSecret == "AgonSecret")
        {
            isAdmin = true;
        }

        var user = new User
        {
            Name = userDto.Name,
            Email = userDto.Email,
            Password = hashedPassword,
            IsAdmin = isAdmin
        };

        await _dbContext.Users.InsertOneAsync(user);

        return new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    public async Task<string> Login(UserLoginDto userDto)
    {
        var user = await _dbContext.Users.Find(u => u.Email == userDto.Email).FirstOrDefaultAsync();
        if (user == null || !BCrypt.Net.BCrypt.Verify(userDto.Password, user.Password))
        {
            throw new Exception("Invalid email or password.");
        }

        return GenerateJwtToken(user);
    }

    private string GenerateJwtToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, user.Name)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(3),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}