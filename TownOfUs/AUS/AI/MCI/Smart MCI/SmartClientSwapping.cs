using AUPathfinder;
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
                if (NavMeshLoader.GlobalNavMesh == null)
                {
                    var loader = new NavMeshLoader();
                    loader.Load();
                    loader.ConnectNearbyNodes(1.25f);
                    NavMeshLoader.GlobalNavMesh = loader;
                }

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
            foreach (var bot in Bot.Bots)
            {
                bot.isTurn = false;
            }

            GiveGroupClears();
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

    // Clear Method
    public static void GiveGroupClears()
    {
        foreach (var tc in ModifierUtils.GetActiveModifiers<TempCleared>().ToArray())
        {
            tc.Player.RemoveModifier(tc);
        }

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (!player.Is(Faction.Impostor) && !player.HasDied())
            {
                foreach (var p in PlayerControl.AllPlayerControls)
                {
                    if (!p.HasDied() && player != p && !p.HasModifier<InvisibleStatus>())
                    {
                        if (WitnessKill.CanSee(player, p.gameObject))
                        {
                            p.AddModifier<TempCleared>();
                        }
                    }
                }
            }
        }
    }
}
