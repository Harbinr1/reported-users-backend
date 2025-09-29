using MongoDB.Driver;
using RentACarReport.Models;
using ReportedUsersSystem.Data;
using ReportedUsersSystem.DTOs;

namespace ReportedUsersSystem.Services;

public class UserService
{
    private readonly MongoDbContext _dbContext;
    private readonly UserContextAccessor _userContextAccessor;

    public UserService(MongoDbContext dbContext, UserContextAccessor userContextAccessor)
    {
        _dbContext = dbContext;
        _userContextAccessor = userContextAccessor;
    }

    public async Task<UserResponseDto> GetCurrentUser()
    {
        var currentUserId = _userContextAccessor.GetCurrentUserId();
        if (string.IsNullOrEmpty(currentUserId))
        {
            throw new UnauthorizedAccessException("User not authenticated.");
        }

        var user = await _dbContext.Users.Find(u => u.Id == currentUserId).FirstOrDefaultAsync();
        if (user == null)
        {
            throw new Exception("User not found.");
        }

        return new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Address = user.Address,
            PhoneNumber = user.PhoneNumber,
            isAdmin = user.IsAdmin,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    public async Task<UserResponseDto> GetUserById(string id)
    {
        var currentUserId = _userContextAccessor.GetCurrentUserId();
        if (string.IsNullOrEmpty(currentUserId))
        {
            throw new UnauthorizedAccessException("User not authenticated.");
        }

        // Users can only access their own information
        if (currentUserId != id)
        {
            throw new UnauthorizedAccessException("You can only access your own user information.");
        }

        var user = await _dbContext.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
        if (user == null)
        {
            throw new Exception("User not found.");
        }

        return new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Address = user.Address,
            PhoneNumber = user.PhoneNumber,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    public async Task<UserResponseDto> UpdateUser(string id, UserRegisterDto userDto)
    {
        var currentUserId = _userContextAccessor.GetCurrentUserId();
        if (string.IsNullOrEmpty(currentUserId))
        {
            throw new UnauthorizedAccessException("User not authenticated.");
        }

        // Users can only update their own account
        if (currentUserId != id)
        {
            throw new UnauthorizedAccessException("You can only update your own account.");
        }

        var existingUser = await _dbContext.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
        if (existingUser == null)
        {
            throw new Exception("User not found.");
        }

        var update = Builders<User>.Update
            .Set(u => u.Name, userDto.Name)
            .Set(u => u.Email, userDto.Email)
            .Set(u => u.Address, userDto.Address)
            .Set(u => u.PhoneNumber, userDto.PhoneNumber)
            .Set(u => u.UpdatedAt, DateTime.UtcNow);

        if (!string.IsNullOrWhiteSpace(userDto.Password))
        {
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
            update.Set(u => u.Password, hashedPassword);
        }

        await _dbContext.Users.UpdateOneAsync(u => u.Id == id, update);

        var updatedUser = await _dbContext.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
        return new UserResponseDto
        {
            Id = updatedUser.Id,
            Name = updatedUser.Name,
            Email = updatedUser.Email,
            Address = updatedUser.Address,
            PhoneNumber = updatedUser.PhoneNumber,
            CreatedAt = updatedUser.CreatedAt,
            UpdatedAt = updatedUser.UpdatedAt
        };
    }

    public async Task DeleteUser(string id)
    {
        var currentUserId = _userContextAccessor.GetCurrentUserId();
        if (string.IsNullOrEmpty(currentUserId))
        {
            throw new UnauthorizedAccessException("User not authenticated.");
        }

        // Users can only delete their own account
        if (currentUserId != id)
        {
            throw new UnauthorizedAccessException("You can only delete your own account.");
        }

        var result = await _dbContext.Users.DeleteOneAsync(u => u.Id == id);
        if (result.DeletedCount == 0)
        {
            throw new Exception("User not found.");
        }
    }
}