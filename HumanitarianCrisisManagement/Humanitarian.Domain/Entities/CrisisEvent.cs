using System;

namespace Humanitarian.Domain.Entities
{
    public class CrisisEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; }
        public string CrisisType { get; set; }
        public string Severity { get; set; }
        public string OperationalArea { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}