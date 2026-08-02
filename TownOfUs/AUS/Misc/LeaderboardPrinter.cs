using System.Diagnostics;
using System.Text;

namespace AmongUsSalem.Misc
{
    public static class LeaderboardPrinter
    {
        public static void Print()
        {
            Directory.CreateDirectory("Leaderboard");

            string fileName = $"Simulation_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
            string path = Path.Combine("Leaderboard", fileName);

            var sb = new StringBuilder();

            sb.AppendLine("Evolution Simulator Statistics");
            sb.AppendLine($"Generated: {DateTime.Now}");
            sb.AppendLine();

            var count = 0;
            foreach (var r in RoleReferences.roleStats.Values) count += r.GamesPlayed;

            sb.AppendLine($"Simulations: {count / 15}");
            sb.AppendLine();

            sb.AppendLine("=== Win Rates ===");

            var sortedRoles = RoleReferences.roleStats.Values
                                        .Where(r => r.GamesPlayed > 0)
                                        .OrderByDescending(r => r.WinRate)  // Sort by WinRate first
                                        .ThenByDescending(r => r.Wins - r.GamesPlayed) // Then by number of losses 
                                        .ThenByDescending(r => r.Kills) // Then by number of kills 
                                        .ToList();
            foreach (var role in sortedRoles)
            {
                sb.AppendLine($"{role.RoleName} - {role.WinRate * 100:F2}% ({role.GamesPlayed} games)");
            }

            sb.AppendLine();
            sb.AppendLine("Data:");

            foreach (var role in sortedRoles)
            {
                sb.AppendLine(role.RoleName);
                sb.AppendLine(WinRateCommand.RoleStatsDisplay(role.RoleName, false));
                sb.AppendLine();
            }

            File.WriteAllText(path, sb.ToString());

            Process.Start(new ProcessStartInfo()
            {
                FileName = Path.GetDirectoryName(Path.GetFullPath(path)),
                UseShellExecute = true
            });
        }
    }
}