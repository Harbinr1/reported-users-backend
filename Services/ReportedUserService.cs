using MongoDB.Bson;
using MongoDB.Driver;
using ReportedUsersSystem.Data;
using ReportedUsersSystem.DTOs;
using ReportedUsersSystem.Models;

namespace ReportedUsersSystem.Services
{
    public class ReportedUserService
    {
        private readonly MongoDbContext _dbContext;
        private readonly UserContextAccessor _userContextAccessor;

        public ReportedUserService(MongoDbContext dbContext, UserContextAccessor userContextAccessor)
        {
            _dbContext = dbContext;
            _userContextAccessor = userContextAccessor;
        }

        public async Task<bool> UpdateReportedUserStatusAsAdmin(string id, ReportedUserStatus status)
        {
            var currentUserId = _userContextAccessor.GetCurrentUserId();
            var user = await _dbContext.Users.Find(u => u.Id == currentUserId).FirstOrDefaultAsync();
            if (user == null || !user.IsAdmin)
            {
                throw new UnauthorizedAccessException("Only admin can update reported user status.");
            }
            var filter = Builders<ReportedUser>.Filter.Eq(ru => ru.Id, id);
            var update = Builders<ReportedUser>.Update
                .Set(ru => ru.Status, status)
                .Set(ru => ru.UpdatedAt, DateTime.UtcNow);
            await _dbContext.ReportedUsers.UpdateOneAsync(filter, update);
            var updatedUser = await _dbContext.ReportedUsers.Find(filter).FirstOrDefaultAsync();
            if (updatedUser == null)
            {
                throw new Exception("Reported user not found.");
            }
            return true;
        }

        public async Task<List<ReportedUserResponseDto>> GetDraftReportedUsersForAdmin()
        {
            var currentUserId = _userContextAccessor.GetCurrentUserId();
            var user = await _dbContext.Users.Find(u => u.Id == currentUserId).FirstOrDefaultAsync();
            if (user == null || !user.IsAdmin)
            {
                throw new UnauthorizedAccessException("Only admin can access draft reported users.");
            }
            var filter = Builders<ReportedUser>.Filter.And(
                Builders<ReportedUser>.Filter.Ne(x => x.Id, null),
                Builders<ReportedUser>.Filter.Eq(x => x.Deleted, false),
                Builders<ReportedUser>.Filter.Eq(x => x.Status, ReportedUserStatus.Draft)
            );
            var reportedUsers = await _dbContext.ReportedUsers.Find(filter).ToListAsync();
            return reportedUsers.Select(ru => new ReportedUserResponseDto
            {
                Id = ru.Id,
                Name = ru.Name,
                IdNumber = ru.IdNumber,
                Date = ru.Date,
                Location = ru.Location,
                Description = ru.Description,
                Status = ru.Status,
                CreatedAt = ru.CreatedAt,
                UpdatedAt = ru.UpdatedAt
            }).ToList();
        }

        public async Task<List<ReportedUserResponseDto>> GetAllReportedUsers()
        {
            var currentUserId = _userContextAccessor.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                throw new UnauthorizedAccessException("User not authenticated.");
            }

            var filter = Builders<ReportedUser>.Filter.And(
               Builders<ReportedUser>.Filter.Ne(x => x.Id, null),
               Builders<ReportedUser>.Filter.Eq(x => x.Deleted, false),
               Builders<ReportedUser>.Filter.Eq(x => x.CreatedBy, currentUserId)
           );

            var reportedUsers = await _dbContext.ReportedUsers.Find(filter).ToListAsync();
            return reportedUsers.Select(ru => new ReportedUserResponseDto
            {
                Id = ru.Id,
                Name = ru.Name,
                IdNumber = ru.IdNumber,
                Date = ru.Date,
                Location = ru.Location,
                Description = ru.Description,
                CreatedAt = ru.CreatedAt,
                UpdatedAt = ru.UpdatedAt
            }).ToList();
        }

        public async Task<ReportedUserResponseDto> GetReportedUserById(string id)
        {
            var currentUserId = _userContextAccessor.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                throw new UnauthorizedAccessException("User not authenticated.");
            }

            var reportedUser = await _dbContext.ReportedUsers
                .Find(ru => ru.Id == id && ru.CreatedBy == currentUserId)
                .FirstOrDefaultAsync();

            if (reportedUser == null)
            {
                throw new Exception("Reported user not found or you don't have permission to access it.");
            }

            return new ReportedUserResponseDto
            {
                Id = reportedUser.Id,
                Name = reportedUser.Name,
                IdNumber = reportedUser.IdNumber,
                Date = reportedUser.Date,
                Location = reportedUser.Location,
                Description = reportedUser.Description,
                CreatedAt = reportedUser.CreatedAt,
                UpdatedAt = reportedUser.UpdatedAt
            };
        }

        public async Task<ReportedUserResponseDto> CreateReportedUser(ReportedUserCreateDto reportedUserDto)
        {
            var currentUserId = _userContextAccessor.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                throw new UnauthorizedAccessException("User not authenticated.");
            }

            var reportedUser = new ReportedUser
            {
                Id = Guid.NewGuid().ToString(),
                Name = reportedUserDto.Name,
                IdNumber = reportedUserDto.IdNumber,
                Date = DateTime.UtcNow,
                Location = reportedUserDto.Location,
                Description = reportedUserDto.Description,
                Status = ReportedUserStatus.Draft,
                CreatedBy = currentUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _dbContext.ReportedUsers.InsertOneAsync(reportedUser);

            return new ReportedUserResponseDto
            {
                Id = reportedUser.Id,
                Name = reportedUser.Name,
                IdNumber = reportedUser.IdNumber,
                Date = reportedUser.Date,
                Location = reportedUser.Location,
                Description = reportedUser.Description,
                CreatedAt = reportedUser.CreatedAt,
                UpdatedAt = reportedUser.UpdatedAt
            };
        }

        public async Task<ReportedUserResponseDto> UpdateReportedUser(string id, ReportedUserUpdateDto reportedUserDto)
        {
            var currentUserId = _userContextAccessor.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                throw new UnauthorizedAccessException("User not authenticated.");
            }

            var user = await _dbContext.Users.Find(u => u.Id == currentUserId).FirstOrDefaultAsync();
            var isAdmin = user != null && user.IsAdmin;
            var filter = isAdmin
                ? Builders<ReportedUser>.Filter.Eq(ru => ru.Id, id)
                : Builders<ReportedUser>.Filter.And(
                    Builders<ReportedUser>.Filter.Eq(ru => ru.Id, id),
                    Builders<ReportedUser>.Filter.Eq(ru => ru.CreatedBy, currentUserId)
                );
            var existingUser = await _dbContext.ReportedUsers.Find(filter).FirstOrDefaultAsync();
            if (existingUser == null)
            {
                throw new Exception("Reported user not found or you don't have permission to update it.");
            }

            var updateBuilder = Builders<ReportedUser>.Update
                .Set(ru => ru.Name, reportedUserDto.Name)
                .Set(ru => ru.IdNumber, reportedUserDto.IdNumber)
                .Set(ru => ru.Location, reportedUserDto.Location)
                .Set(ru => ru.Description, reportedUserDto.Description)
                .Set(ru => ru.Deleted, reportedUserDto.Deleted)
                .Set(ru => ru.UpdatedAt, DateTime.UtcNow);

            if (reportedUserDto.Status.HasValue)
            {
                if (!isAdmin)
                {
                    throw new UnauthorizedAccessException("Only admin can update reported user status.");
                }
                updateBuilder = updateBuilder.Set(ru => ru.Status, reportedUserDto.Status.Value);
            }

            await _dbContext.ReportedUsers.UpdateOneAsync(
                filter,
                updateBuilder);

            var updatedUser = await _dbContext.ReportedUsers.Find(filter).FirstOrDefaultAsync();
            return new ReportedUserResponseDto
            {
                Id = updatedUser.Id,
                Name = updatedUser.Name,
                Date = updatedUser.Date,
                IdNumber = updatedUser.IdNumber,
                Location = updatedUser.Location,
                Description = updatedUser.Description,
                Status = updatedUser.Status,
                CreatedAt = updatedUser.CreatedAt,
                UpdatedAt = updatedUser.UpdatedAt
            };
        }
        public async Task DeleteReportedUser(string id)
        {
            var currentUserId = _userContextAccessor.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                throw new UnauthorizedAccessException("User not authenticated.");
            }

            var filter = Builders<ReportedUser>.Filter.And(
                Builders<ReportedUser>.Filter.Eq(ru => ru.Id, id),
                Builders<ReportedUser>.Filter.Eq(ru => ru.CreatedBy, currentUserId)
            );
            var update = Builders<ReportedUser>.Update.Set(ru => ru.Deleted, true);

            var result = await _dbContext.ReportedUsers.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
            {
                throw new Exception("Reported user not found or you don't have permission to delete it.");
            }
        }

        public async Task<List<ReportedUserResponseDto>> SearchReportedUsers(string NameOrIdNumber)
        {
            var currentUserId = _userContextAccessor.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                throw new UnauthorizedAccessException("User not authenticated.");
            }

            var filter = Builders<ReportedUser>.Filter.And(
               Builders<ReportedUser>.Filter.Ne(x => x.Id, null),
               Builders<ReportedUser>.Filter.Eq(x => x.Deleted, false),
               Builders<ReportedUser>.Filter.Eq(x => x.Status, ReportedUserStatus.Active)
           );

            // Add OR condition for Name or IdNumber if NameOrIdNumber is provided
            if (!string.IsNullOrEmpty(NameOrIdNumber))
            {
                var nameFilter = Builders<ReportedUser>.Filter.Regex(ru => ru.Name, new BsonRegularExpression(NameOrIdNumber, "i"));
                var idNumberFilter = Builders<ReportedUser>.Filter.Regex(ru => ru.IdNumber, new BsonRegularExpression(NameOrIdNumber, "i"));
                // Combine filters with OR
                filter &= Builders<ReportedUser>.Filter.Or(nameFilter, idNumberFilter);
            }

            var reportedUsers = await _dbContext.ReportedUsers.Find(filter).ToListAsync();

            return reportedUsers.Select(ru => new ReportedUserResponseDto
            {
                Id = ru.Id,
                Name = ru.Name,
                Date = ru.Date,
                Location = ru.Location,
                IdNumber = ru.IdNumber,
                Description = ru.Description,
                CreatedAt = ru.CreatedAt,
                UpdatedAt = ru.UpdatedAt
            }).ToList();
        }
    }
}