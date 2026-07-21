using System.Collections;
using UnityEngine;

namespace AmongUsSalem.MCI.SmartMCI;

public static class SmartClientSwapping
{
    [RegisterEvent()]
    public static void RoundStartEvent(RoundStartEvent @event)
    {
        CustomExtentions.AvailableRooms.GetAvailableRooms();
        if (Debugger.windywaysMode)
        {
            if (@event.TriggeredByIntro)
            {
                Bot.Bots.Clear();
                foreach (var player in PlayerControl.AllPlayerControls) Bot.Create(player);
            }
            else
            {
                Keyboard_Joystick.RefreshSwapTargets();
                Keyboard_Joystick.Switch(true);
            }
        }
    }

    [RegisterEvent()]
    public static void StartMeetingEvent(StartMeetingEvent @event)
    {
        if (Debugger.windywaysMode)
        {
            Coroutines.Start(DelayVote());
        }
    }

    private static IEnumerator DelayVote()
    {
        yield return new WaitForSeconds(7f);
        foreach (PlayerVoteArea playerVoteArea in MeetingHud.Instance.playerStates)
        {
            playerVoteArea.UnsetVote();
            MeetingHud.Instance.ClearVote();
        }

        CalculatedVoting.DoVotes(MeetingHud.Instance);

        yield return new WaitForSeconds(2f);
        MeetingHud.Instance.RpcClose();
    }
}
