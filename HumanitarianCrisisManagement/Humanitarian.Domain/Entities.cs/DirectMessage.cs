using Humanitarian.Domain.Common;

namespace Humanitarian.Domain.Entities;

public class DirectMessage : BaseEntity
{
    public Guid RequestId { get; set; }
    
    public Guid SenderId { get; set; }
    
    public Guid ReceiverId { get; set; }
    
    public string MessageBody { get; set; } = string.Empty;
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Navigation property
    public AssistanceRequest? Request { get; set; }
}