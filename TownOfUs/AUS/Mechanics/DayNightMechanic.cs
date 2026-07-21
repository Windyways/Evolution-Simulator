namespace AmongUsSalem.Mechanics;

public static class DayNightMechanic
{
    public static float PostMeetingIntroTime = 7f;
    public static int DayCount = 0;
    public static int NightCount = 1;
    public static float NightTimer;

    public static bool FullMoon()
    {
        return NightCount == 2 || NightCount >= 4;
    }

    public static bool HalfMoon()
    {
        return !FullMoon();
    }

    [MethodRpc((uint)AUSRpc.StartDayOne)]
    public static void StartDayOne(PlayerControl player)
    {
        if (AmongUsClient.Instance.AmHost)
        {
            MeetingRoomManager.Instance.AssignSelf(player, null);

            if (GameManager.Instance.CheckTaskCompletion())
            {
                return;
            }

            HudManager.Instance.OpenMeetingRoom(player);
            player.RpcStartMeeting(null);
        }
    }

    [RegisterEvent(-1)]
    public static void StartMeetingEventHandler(StartMeetingEvent @event)
    {
        DayCount++;
    }

    [RegisterEvent(-1)]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            DayCount = 0;
            NightCount = 1;
            return; // Only run when round starts.
        }

        NightCount++;
    }
    
    [RegisterEvent]
    public static void GameStartHandler(RoundStartEvent @event)
    {
        if (!@event.TriggeredByIntro)
        {
            return; // Only run when game starts.
        }

        foreach (var player in PlayerControl.AllPlayerControls) 
            ShowRoleIcon.Add(player);
    }
}