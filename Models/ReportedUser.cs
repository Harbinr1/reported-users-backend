using MongoDB.Bson.Serialization.Attributes;

public enum ReportedUserStatus
{
    Draft,
    Active
}

namespace ReportedUsersSystem.Models
{
    public class ReportedUser
    {
        [BsonId]
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? IdNumber { get; set; }
        public int Fine { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string? Location { get; set; }
        public string? Description { get; set; }
        public bool Deleted { get; set; } = false;
        public ReportedUserStatus Status { get; set; } = ReportedUserStatus.Draft;

        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}