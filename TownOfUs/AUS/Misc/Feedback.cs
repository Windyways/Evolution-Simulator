using UnityEngine;

namespace AmongUsSalem.Misc;

public static class Feedback
{
    public static string LeaveTown(PlayerControl player)
    {
        return $"{player.Name()} has accomplished their goal as {player.Data.Role.NiceName} and left town.";
    }

    public static string AttackedButDefense()
    {
        return "Someone attacked you, but your defense was too high!";
    }

    public static string TooMuchDefense(PlayerControl player, PlayerControl target)
    {
        /* if (ConcordantFunction.Interfere(target)) ConcordantFunction.NotifyConcordant(player, target);
         else if (ClericFunction.Interfere(target)) ClericFunction.NotifyCleric(player, target);
         else if (OracleFunction.Interfere(target)) OracleFunction.NotifyOracle(player, target);
         else if (GuardianAngelFunction.Interfere(target)) GuardianAngelFunction.NotifyGuardianAngel(player, target);
         else if (target.IsTrapped()) // Some sort of trapper 'attacked' RPC here.
 */

        if (player.AmOwner()) Coroutines.Start(MiscUtils.CoFlash(RoleColors.Impostor));

        /*if (player.IsRole<Vigilante>())
        {
            return target.GetDefaultAppearance().PlayerName + " was immune to your attack.";
        }*/

        RpcTMDNotify(target); //target.Notify(AttackedButDefense(), NotifyMode.OnlyMeeting);

        return target.GetDefaultAppearance().PlayerName + "'s defense was too high to kill!";
    }

    public static void Notify(this PlayerControl player, string feedback, NotifyMode type, Color color = new(), Sprite? sprite = null, NetworkedPlayerInfo? basePlayer = null)
    {
        if (player.AmOwner())
        {
            if (type == NotifyMode.Instantly) MiscUtils.ShowNotification(feedback, color, sprite);
            if (type == NotifyMode.InstantlyAndMeeting)
            {
                MiscUtils.ShowNotification(feedback, color, sprite);
                MiscUtils.AddFakeChat(basePlayer == null ? player.CachedPlayerData : basePlayer, "FEEDBACK", feedback);
            }
            if (type == NotifyMode.OnlyMeeting) MiscUtils.AddFakeChat(basePlayer == null ? player.CachedPlayerData : basePlayer, "FEEDBACK", feedback);
        }
    }

    [MethodRpc((uint)AUSRpc.RpcTMDNotify)]
    public static void RpcTMDNotify(PlayerControl player)
    {
        if (player.AmOwner())
        {
            player.Notify(AttackedButDefense(), NotifyMode.OnlyMeeting);
        }
    }
}