using System;

namespace Humanitarian.Domain.Entities
{
    public class Citizen
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Nic { get; set; }
        public string ContactNumber { get; set; }
        public string Location { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}