namespace ReportedUsersSystem.DTOs;

public class UserRegisterDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; }
    public string? Password { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? AdminSecret { get; set; } // Only you know this
}

public class UserLoginDto
{
    
    public string Email { get; set; }
    public string Password { get; set; }
}

public class UserResponseDto
{
    public string Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
    public bool isAdmin { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}