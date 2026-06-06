using NatureActivityTracker.Models;

namespace NatureActivityTracker.Services
{
    public class EcoIntelligenceService
    {
        public int CalculateEcoScore(List<ActivityLog> logs)
        {
            if (logs == null || !logs.Any())
                return 0;

            int totalCoins = logs.Sum(a => a.Coins);

            int consistencyBonus = logs.Count >= 10 ? 10 : logs.Count;

            int score = totalCoins + consistencyBonus;

            return Math.Min(score, 100); // Max 100
        }

        public string GetGrade(int score)
        {
            if (score >= 80) return "A 🌟";
            if (score >= 60) return "B 👍";
            if (score >= 40) return "C 🙂";
            return "D ⚠";
        }

        public string GenerateInsight(List<ActivityLog> logs)
        {
            if (logs == null || !logs.Any())
                return "Start logging activities to unlock your eco potential 🌱";

            var last7Days = logs
                .Where(a => a.Date >= DateTime.Now.AddDays(-7))
                .Sum(a => a.Coins);

            var thisMonth = logs
                .Where(a => a.Date.Month == DateTime.Now.Month)
                .Sum(a => a.Coins);

            if (last7Days < 10)
                return "Your recent activity is low. Try adding one eco-friendly action this week 🌿";

            if (thisMonth > 100)
                return "Amazing consistency this month! You're building real sustainable habits 🌎";

            return "You're progressing steadily. Add a high-impact activity to boost your eco score 🚀";
        }
    }
}