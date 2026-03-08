using MiApp.Data;
using MiApp.Domain;

namespace MiApp.Business;

public class AuditService
{
    private readonly DatabaseService _db;

    public AuditService(DatabaseService db)
    {
        _db = db;
    }

    public async Task LogAsync(int userId, string action, string description)
    {
        await _db.InsertAuditAsync(new AuditLog
        {
            UserId = userId,
            Action = action,
            Description = description,
            CreatedAt = DateTime.Now
        });
    }

    public Task<List<(string Username, string Action, string Description, DateTime CreatedAt)>> 
        GetAllAsync()
    {
        return _db.GetAuditLogsAsync();
    }
}