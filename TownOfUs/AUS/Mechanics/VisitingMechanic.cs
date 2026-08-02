using TownOfUs.Modifiers;
using UnityEngine;

namespace AmongUsSalem.Mechanics;

public static class VisitingMechanic
{
    public static void RpcAddDeathReason(PlayerControl player, DeathReasonShow deathReasonShow, Color color)
    {
        RoleReferences.UpdateRoleDeaths(player.GetRoleWhenAlive(), deathReasonShow);
        DeathHandlerModifier.UpdateDeathHandler(player, color, deathReasonShow, DeathHandlerOverride.SetFalse);
    }

    public static void CheckVisit(PlayerControl player, PlayerControl target, int Button, bool isAttacking, bool isVisiting)
    {
        int blockVisit = 0;

        var playerRole = player.GetRoleWhenAlive();
        var targetRole = target?.GetRoleWhenAlive();

        if (blockVisit < 100)
        {
            //if (isAttacking && targetRole is Palingenist palingenist && palingenist.CanCounterattack) blockVisit++; //blockVisit += palingenist.Counter(player);
            if (isAttacking && targetRole is Concordant concordant && concordant.Immunities > 0) blockVisit += concordant.Function();
        }

        if (blockVisit > 0)
        {
            foreach (var players in PlayerControl.AllPlayerControls)
            {
                if (players.HasDied())
                {
                    var role = players.GetRoleWhenAlive();
                    if (role is ICustomAURole cr)
                    {
                        cr.Role_OnVisitFail(player, target, isAttacking, isVisiting, blockVisit);
                    }
                }
                else if (players.Data.Role is ICustomAURole customRole)
                {
                    customRole.Role_OnVisitFail(player, target, isAttacking, isVisiting, blockVisit);
                }
            }

            AUSPlugin.DebugLogMessage($"{player.Name()} has failed their visit.");
            return; // Code below only runs if visit was successful.
        }

        SuccessfulVisit(player, target, Button);
    }

    public static void SuccessfulVisit(PlayerControl player, PlayerControl target, int Button)
    {
        AUSPlugin.DebugLogMessage($"{player.Name()}'s visit was Successful!");

        var role = player.GetRoleWhenAlive();
        if (role is ICustomAURole customRole) customRole.Function(target, Button);
    }
}