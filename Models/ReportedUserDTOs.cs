
namespace ReportedUsersSystem.DTOs;

public class ReportedUserStatusUpdateDto
{
    public ReportedUserStatus Status { get; set; }
}
public class ReportedUserCreateDto
{
    public string? Name { get; set; }
    public string? IdNumber { get; set; }
    public int Fine { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
}

public class ReportedUserUpdateDto
{
    public string? Name { get; set; }
    public string? IdNumber { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
    public bool Deleted { get; set; } = false;
    public ReportedUserStatus? Status { get; set; }
}

public class ReportedUserResponseDto
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public DateTime Date { get; set; }
    public string? IdNumber { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
    public ReportedUserStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ReportedUserSearchDto
{
    public string? NameOrIdNumber { get; set; }
}