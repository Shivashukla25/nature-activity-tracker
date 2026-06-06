using Microsoft.AspNetCore.Identity;

namespace NatureActivityTracker.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }

        public string? ActivityName { get; set; }

        public int Coins { get; set; }

        public DateTime Date { get; set; }

        public string? UserId { get; set; }

        public string? ProofImagePath { get; set; }

        public bool IsVerified { get; set; } = false;
    }
}