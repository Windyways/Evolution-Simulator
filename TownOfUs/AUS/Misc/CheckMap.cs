namespace AmongUsSalem.Misc;

public static class CheckMap
{
    public static CurrentMap MapSelected;

    [HarmonyPatch(typeof(LobbyBehaviour), "Update")]
    public static class LobbyBehaviourUpdate
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (MapSelected != (CurrentMap)GameOptionsManager.Instance.currentNormalGameOptions.MapId)
            {
                MapSelected = (CurrentMap)GameOptionsManager.Instance.currentNormalGameOptions.MapId;
                AUSPlugin.DebugLogMessage($"Map set to {MapSelected}.", AUSPlugin.MsgType.Message);
            }
        }
    }
}

public enum CurrentMap
{
    Skeld,
    MiraHQ,
    Polus,
    Airship = 4,
    Fungle,
    Submerged,
    LevelImpostor
}