using System.Globalization;
using UnityEngine;

namespace AmongUsSalem.Misc
{
    public class RoleStats
    {
        public string RoleName { get; set; }

        public int Wins { get; set; }
        public int GamesPlayed { get; set; }
        public Color Color { get; set; }
        
        public int Kills { get; set; }

        // Deaths
        public Dictionary<DeathReasonShow, int> Deaths = new();

        public float WinRate
        {
            get
            {
                if (GamesPlayed == 0) return 0;
                return (float)Wins / GamesPlayed;
            }
        }

        public RoleStats(string roleName, Color color)
        {
            RoleName = roleName;
            Wins = 0;
            GamesPlayed = 0;
            Kills = 0;
            Color = color;

            Deaths = new Dictionary<DeathReasonShow, int>();
        }
    }

    public static class RoleReferences
    {
        public static Dictionary<string, RoleStats> roleStats = new Dictionary<string, RoleStats>();
        public static string filePath = "WinData.txt";
        public static bool CountRoundToLeaderboard = true;

        public static void Initialize()
        {
            // --- CREWMATE ---
            roleStats.Add("Crewmate", new RoleStats("Crewmate", RoleColors.Crewmate));
            roleStats.Add("Itinerant", new RoleStats("Itinerant", RoleColors.Crewmate));
            roleStats.Add("Thanatologist", new RoleStats("Thanatologist", RoleColors.Crewmate));
            
            // --- NEUTRAL ---
            roleStats.Add("Concordant", new RoleStats("Concordant", RoleColors.Concordant));
            roleStats.Add("Pharmakos", new RoleStats("Pharmakos", RoleColors.Pharmakos));
            roleStats.Add("Palingenist", new RoleStats("Palingenist", RoleColors.Palingenist));

            // --- IMPOSTOR ---
            roleStats.Add("Impostor", new RoleStats("Impostor", RoleColors.Impostor));
            roleStats.Add("Tenebrist", new RoleStats("Tenebrist", RoleColors.Impostor));
            roleStats.Add("Noctivagant", new RoleStats("Noctivagant", RoleColors.Impostor));

            LoadRoleStats(filePath);
        }

        public static List<string> PendingNotifications = new List<string>();
        public static void UpdateRoleDeaths(RoleBehaviour roleBehaviour, DeathReasonShow death)
        {
            string roleName = roleBehaviour.NiceName;
            if (!CountRoundToLeaderboard)
            {
                AUSPlugin.DebugLogMessage("CountRoundToLeaderboard is false or Debugger is inactive, deaths do not count this game.");
                return;
            }

            if (roleStats.TryGetValue(roleName, out RoleStats? stats))
            {
                if (!stats.Deaths.ContainsKey(death))
                {
                    stats.Deaths[death] = 0;
                }

                stats.Deaths[death]++;
            }

            SaveRoleStats(filePath);
        }

        public static void UpdateRoleResult(RoleBehaviour roleBehaviour, int kills, bool won)
        {
            string roleName = roleBehaviour.NiceName;
            if (!CountRoundToLeaderboard || !Debugger.IsDebuggerActive)
            {
                AUSPlugin.DebugLogMessage("CountRoundToLeaderboard is false or Debugger is inactive, wins and loses do not count this game.");
                return;
            }

            if (roleStats.TryGetValue(roleName, out RoleStats? stats))
            {
                // Store snapshot before updating
                var oldStats = new RoleStats(stats.RoleName, stats.Color)
                {
                    Wins = stats.Wins,
                    GamesPlayed = stats.GamesPlayed,
                    Kills = stats.Kills
                };

                int oldRank = GetLeaderboardPosition(roleName);

                // Update stats
                stats.GamesPlayed++;
                stats.Kills += kills;
                if (won) stats.Wins++;

                int newRank = GetLeaderboardPosition(roleName);

                // Create notification text
                //string arrow = newRank < oldRank ? "^" : (newRank > oldRank ? "?" : ">");
                string colorArrow = newRank < oldRank ? "<color=#00ff00>^</color>" : (newRank > oldRank ? "<color=#ff0000>?</color>" : ">");
                string hexColor = ColorUtility.ToHtmlStringRGB(stats.Color);
                string coloredRole = $"<b><color=#{hexColor}>{stats.RoleName}</color></b>";

                string msg = $"{coloredRole} {oldStats.WinRate * 100:F2}% > {stats.WinRate * 100:F2}% ({colorArrow} #{oldRank} > #{newRank})";

                // Add to pending notifications
                PendingNotifications.Add(msg);

            }

            SaveRoleStats(filePath);
        }

        private static int GetLeaderboardPosition(string roleName)
        {
            var sorted = roleStats.Values
                .Where(r => r.GamesPlayed > 0)
                .OrderByDescending(r => r.WinRate)
                .ToList();

            return sorted.FindIndex(r => r.RoleName == roleName) + 1; // +1 because 0-based index
        }

        public static void SaveRoleStats(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var role in roleStats.Values)
                {
                    //                    TryParse 0    TryParse 1     TryParse 2        TryParse 3 
                    writer.WriteLine($"{role.RoleName},{role.Wins},{role.GamesPlayed},{role.Kills},{role.Deaths}");
                }
            }
        }

        public static void LoadRoleStats(string filePath)
        {
            if (!File.Exists(filePath))
            {
                SaveRoleStats(filePath); // Save the current roleStats (even if empty/default)
                AUSPlugin.DebugLogMessage(".txt file not found, creating a new one.", AUSPlugin.MsgType.Error);
                return;
            }

            foreach (var line in File.ReadLines(filePath))
            {
                var parts = line.Split(',');
                if (parts.Length != 5) continue; // skip broken lines
                                                 // Add this by 1 for each saved stat i want.

                string roleName = parts[0];
                Color color = Color.white;

                // Load Deaths
                var deaths = new Dictionary<DeathReasonShow, int>();

                foreach (var pair in parts[4].Split('|'))
                {
                    var values = pair.Split(':');

                    if (values.Length != 2)
                        continue;

                    if (Enum.TryParse(values[0], out DeathReasonShow reason) &&
                        int.TryParse(values[1], out int count))
                    {
                        deaths[reason] = count;
                    }
                }

                if (int.TryParse(parts[1], out int wins) && int.TryParse(parts[2], out int gamesPlayed) && int.TryParse(parts[3], out int kills))
                {
                    if (roleStats.TryGetValue(roleName, out RoleStats? value))
                    {
                        value.Wins = wins;
                        value.GamesPlayed = gamesPlayed;
                        value.Kills = kills;
                        value.Deaths = deaths;
                    }
                    else
                    {
                        // Optionally add new roles if missing
                        roleStats.Add(roleName, new RoleStats(roleName, color)
                        {
                            Wins = wins,
                            GamesPlayed = gamesPlayed,
                            Kills = kills,
                            Color = color
                        });
                    }
                }
            }
        }

        public static void ResetLeaderboard(string filePath)
        {
            // Clear the file (or you can delete it)
            if (File.Exists(filePath))
            {
                File.WriteAllText(filePath, string.Empty);  // This just empties the file
                AUSPlugin.DebugLogMessage("Leaderboard has been reset!");
            }

            // Optional: Reset in-memory role stats as well (you could leave it as-is)
            foreach (var role in roleStats.Values)
            {
                role.Wins = 0;
                role.GamesPlayed = 0;
                role.Kills = 0;
            }

            // Optionally save the empty stats back to the file
            SaveRoleStats(filePath);  // This will save an empty leaderboard
        }
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
    public static class WinRateCommand
    {
        public static bool Prefix(ChatController __instance)
        {
            if (__instance.freeChatField.Text.ToLower(CultureInfo.CurrentCulture).Contains("/lb", StringComparison.CurrentCultureIgnoreCase))
            {
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (!string.IsNullOrWhiteSpace(WinRate()) && player == PlayerControl.LocalPlayer) DestroyableSingleton<HudManager>.Instance.Chat.AddChat(player, WinRate());
                }

                __instance.freeChatField.Clear();
                __instance.quickChatMenu.Clear();
                __instance.quickChatField.Clear();
                __instance.UpdateChatMode();
                return true;
            }

            if (__instance.freeChatField.Text.ToLower(CultureInfo.CurrentCulture).Contains("/print", StringComparison.CurrentCultureIgnoreCase))
            {
                LeaderboardPrinter.Print();

                __instance.freeChatField.Clear();
                __instance.quickChatMenu.Clear();
                __instance.quickChatField.Clear();
                __instance.UpdateChatMode();
                return true;
            }

            string message = __instance.freeChatField.Text.Trim();

            if (message.StartsWith("/"))
            {
                string roleName = message.Substring(1); // removes "/"

                // Case insensitive search
                var role = RoleReferences.roleStats
                    .FirstOrDefault(x => x.Key.Equals(roleName, StringComparison.CurrentCultureIgnoreCase));

                if (role.Value != null)
                {
                    MiscUtils.AddFakeChat(
                        PlayerControl.LocalPlayer.CachedPlayerData,
                        "Stats",
                        RoleStatsDisplay(role.Value.RoleName)
                    );

                    __instance.freeChatField.Clear();
                    __instance.quickChatMenu.Clear();
                    __instance.quickChatField.Clear();
                    __instance.UpdateChatMode();
                    return true;
                }
            }

            if (__instance.freeChatField.Text.ToLower(CultureInfo.CurrentCulture).Contains("/resetlb", StringComparison.CurrentCultureIgnoreCase))
            {
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    var playerResults = ResetLeaderboard();
                    RoleReferences.ResetLeaderboard(RoleReferences.filePath);
                    
                    if (!string.IsNullOrWhiteSpace(playerResults) && player == PlayerControl.LocalPlayer)
                    {
                        DestroyableSingleton<HudManager>.Instance.Chat.AddChat(player, playerResults);
                    }

                    __instance.freeChatField.Clear();
                    __instance.quickChatMenu.Clear();
                    __instance.quickChatField.Clear();
                    __instance.UpdateChatMode();
                }
                return true;
            }

            if (__instance.freeChatField.Text.ToLower(CultureInfo.CurrentCulture).Contains("/state", StringComparison.CurrentCultureIgnoreCase))
            {
                if (Debugger.IsDebuggerActive && AUSPlugin.InGame())
                {
                    MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.CachedPlayerData, "Stats", GetState());
                    __instance.freeChatField.Clear();
                    __instance.quickChatMenu.Clear();
                    __instance.quickChatField.Clear();
                    __instance.UpdateChatMode();
                }
                return true;
            }
            return true;
        }

        public static string RoleStatsDisplay(string roleName, bool colors = true)
        {
            if (!RoleReferences.roleStats.TryGetValue(roleName, out RoleStats role))
                return "Role not found.";

            string stats = "";

            string hexColor = ColorUtility.ToHtmlStringRGB(role.Color);
            string coloredRoleName = $"<b><color=#{hexColor}>{role.RoleName}</color></b>";
            if (!colors) coloredRoleName = role.RoleName;

            stats += $"--- {coloredRoleName} ---\n\n";

            stats += $"Games Played: {role.GamesPlayed}\n";
            stats += $"Wins: {role.Wins}\n";
            stats += $"Losses: {role.GamesPlayed - role.Wins}\n";
            stats += $"Win Rate: {role.WinRate * 100:F2}%\n";
            stats += $"Kills: {role.Kills}\n\n";


            stats += "--- DEATHS ---\n";

            if (role.Deaths.Count == 0)
            {
                stats += "No deaths recorded.\n";
            }
            else
            {
                foreach (var death in role.Deaths)
                {
                    stats += $"{death.Key.ToSpacedString()}: {death.Value}\n";
                }
            }

            return "<size=62%>" + stats + "</size>";
        }

        public static string WinRate(bool colors = true)
        {
            string rates = "";

            var sortedRoles = RoleReferences.roleStats.Values
                                        .Where(r => r.GamesPlayed > 0) 
                                        .OrderByDescending(r => r.WinRate)  // Sort by WinRate first
                                        .ThenByDescending(r => r.Wins - r.GamesPlayed) // Then by number of losses 
                                        .ThenByDescending(r => r.Kills) // Then by number of kills 
                                        .ToList();

            foreach (var role in sortedRoles)
            {
                string hexColor = ColorUtility.ToHtmlStringRGB(role.Color);
                string coloredRoleName = $"<b><color=#{hexColor}>{role.RoleName}</color></b>";

                string KillsMSG = "";
                rates += $"{coloredRoleName} | <b><color=#ff0000>{role.GamesPlayed - role.Wins}</color></b> | <b><color=#00ff00>{role.Wins}</color></b> |{KillsMSG} {role.WinRate * 100:F2}% |\n";
                if (!colors)
                {
                    rates = $"{role.RoleName} | {role.GamesPlayed - role.Wins} | {role.Wins} |{KillsMSG} {role.WinRate * 100:F2}% |\n";
                }
            }

            if (rates == "") rates = "There are no data logged on this slot.";
            if (!colors) return rates;
            return "<size=62%>" + rates + "</size>";
        }

        public static string ResetLeaderboard()
        {
            return "The leaderboard has been reset successfully.";
        }

        public static string GetState()
        {
            string state = "";

            state += "--- SEEN KILL ---\n";
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.TryGetModifier<SeenKill>(out var seenKill))
                {
                    state += $"{player.Name()} saw {seenKill.killer.Name()} kill. ({seenKill.voteChance}%).\n";
                }
            }

            state += "\n\n--- INCRIMINATING EVIDENCE ---\n";
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.TryGetModifier<IncriminatingEvidence>(out var incriminating))
                {
                    state += $"{player.Name()}\n";
                }
            }

            state += "\n\n--- TOWN INVESTIGATIVES ---\n";
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.TryGetModifier<TI>(out var ti))
                {
                    state += $"{player.Name()}\n";
                }
            }

            state += "\n\n--- SOFT CLEARED ---\n";
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.TryGetModifier<SoftCleared>(out var ti))
                {
                    state += $"{player.Name()}\n";
                }
            }

            state += "\n\n--- CONFIRMED TOWNIES ---\n";
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.TryGetModifier<Confirmed>(out var ti))
                {
                    state += $"{player.Name()}\n";
                }
            }

            state += "\n\n--- CONFIRMED EVILS ---\n";
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.TryGetModifier<ConfirmedEvil>(out var ti))
                {
                    state += $"{player.Name()}\n";
                }
            }

            return state;
        }
    }
}