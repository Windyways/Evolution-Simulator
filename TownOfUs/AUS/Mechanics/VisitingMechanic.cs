using System.Collections;
using TownOfUs.Modifiers;
using UnityEngine;
using xCloud;

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
        if (isVisiting && player.TryGetModifier<ToastedModifier>(out var toasted)) blockVisit += toasted.PerformInteraction();
        if (isVisiting && target.Data.Role is Traveler traveler) blockVisit += traveler.PerformInteraction(player);

        // --- UNKNOWN OBSTACLE INTERACTIONS ---

        if (blockVisit < 100)
        {
            // Redirection!
            if (isVisiting && target.TryGetModifier<Traveled>(out var traveled) && !traveled.Caster.HasDied()) target = traveled.Caster; // Redirect visit to Traveler.

            // Other Interactions
            if (isAttacking && target.Data.Role is Bulletproof bulletproof && bulletproof.hasVest) blockVisit += bulletproof.PerformInteraction(player, target);
            if (isVisiting && target.Data.Role is Granny granny) blockVisit += granny.PerformInteraction(player, target);
            if (isVisiting && isAttacking && target.TryGetModifier<SavedModifier>(out var saved)) blockVisit += saved.PerformInteraction(player, target);
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
            // Village
            if (role is Cop) CustomButtonSingleton<Cop_Investigate>.Instance.ResetCooldownAndOrEffect();
            if (role is Sniper) CustomButtonSingleton<Sniper_Kill>.Instance.ResetCooldownAndOrEffect();
            if (role is Doctor)
            {
                CustomButtonSingleton<Doctor_Save>.Instance.ResetCooldownAndOrEffect();
                CustomButtonSingleton<Doctor_SelfSave>.Instance.ResetCooldownAndOrEffect();
            }
            if (role is Deputy) CustomButtonSingleton<Deputy_Arest>.Instance.ResetCooldownAndOrEffect();
            if (role is Fairy) CustomButtonSingleton<Fairy_Restore>.Instance.ResetCooldownAndOrEffect();
            if (role is Gravedigger) CustomButtonSingleton<Gravedigger_Revive>.Instance.ResetCooldownAndOrEffect();
            if (role is Traveler) CustomButtonSingleton<Traveler_Travel>.Instance.ResetCooldownAndOrEffect();

            // Mafia
            if (role is Framer) CustomButtonSingleton<Framer_Frame>.Instance.ResetCooldownAndOrEffect();
            if (role is Toaster)
            { 
                CustomButtonSingleton<Toaster_Toast>.Instance.ResetCooldownAndOrEffect();
                CustomButtonSingleton<Toaster_SelfToast>.Instance.ResetCooldownAndOrEffect();
            }
            if (role is Interrogator) CustomButtonSingleton<Interrogator_Interrogate>.Instance.ResetCooldownAndOrEffect();
            if (role is Lawyer) CustomButtonSingleton<Lawyer_Intervene>.Instance.ResetCooldownAndOrEffect();
            if (role is Godfather) CustomButtonSingleton<Godfather_Kill>.Instance.ResetCooldownAndOrEffect();
            if (role is Henchman) CustomButtonSingleton<Henchman_Threaten>.Instance.ResetCooldownAndOrEffect();
            if (role is Mafia) CustomButtonSingleton<Mafia_Kill>.Instance.ResetCooldownAndOrEffect();
            if (role is Robber) CustomButtonSingleton<Robber_Steal>.Instance.ResetCooldownAndOrEffect();
            if (role is Maid) CustomButtonSingleton<Maid_CleanUp>.Instance.ResetCooldownAndOrEffect();

            // Independent
            if (role is MadScientist) CustomButtonSingleton<MadScientist_Experiment>.Instance.ResetCooldownAndOrEffect();
        }
    }
}