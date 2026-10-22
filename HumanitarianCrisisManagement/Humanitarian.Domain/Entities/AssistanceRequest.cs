using System;

namespace Humanitarian.Domain.Entities
{
    public class AssistanceRequest
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CitizenId { get; set; }
        public Guid CrisisId { get; set; }
        public string Category { get; set; }
        public string UrgencyLevel { get; set; }
        public string Description { get; set; }
        public string LocationCoordinates { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}