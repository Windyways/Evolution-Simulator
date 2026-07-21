using TownOfUs.Modifiers;

namespace AmongUsSalem.Mechanics;

public static class VisitingMechanic
{
    [MethodRpc((uint)AUSRpc.RpcAddDeathReason)]
    public static void RpcAddDeathReason(PlayerControl player, int deathReasonShow)
    {
        DeathHandlerModifier.UpdateDeathHandler(player, (DeathReasonShow)deathReasonShow, DeathHandlerOverride.SetFalse);
    }

    public static void CheckVisit(PlayerControl player, PlayerControl target, int Button, bool isAttacking, bool isVisiting)
    {
        int blockVisit = 0;

        var playerRole = player.GetRoleWhenAlive();
        var targetRole = target?.GetRoleWhenAlive();

        ResetCooldowns(player, target, Button, isAttacking);

        // --- ROLEBLOCK INTERACTIONS ---
        // --- UNKNOWN OBSTACLE INTERACTIONS ---

        if (blockVisit < 100)
        {
            // Redirection!
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

    public static void ResetCooldowns(PlayerControl player, PlayerControl target, int Button, bool Attacking)
    {
        var role = player.GetRoleWhenAlive();
        var targetRole = target.GetRoleWhenAlive();

        if (player.AmOwner)
        {
            // Crewmate

            // Impostor
            if (role is Impostor) CustomButtonSingleton<Impostor_Kill>.Instance.ResetCooldownAndOrEffect();
        }
    }
}