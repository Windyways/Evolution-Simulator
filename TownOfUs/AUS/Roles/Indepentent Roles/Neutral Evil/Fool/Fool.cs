using Il2CppInterop.Runtime.Attributes;
using System.Text;
using TownOfUs.Modifiers;
using UnityEngine;

namespace AmongUsSalem.Roles;

public sealed class Fool(IntPtr cppPtr) : NeutralRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Fool";
    public string RoleDescription => "";
    public string RoleLongDescription => "";
    public Color RoleColor { get; set; } = RoleColors.Independent;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;

    public Faction Faction { get; set; } = Faction.Independent;
    public Alignment Alignment => Alignment.IndependentEvil;

    public CustomRoleConfiguration Configuration => new(this)
    {
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
        //Icon = AUSAssets.JesterRoleCard
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ICustomAURole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return 
            $"Wins if executed by the village during the day. Sided with no one.\n" +
            "Wins if executed by the village during the day. Sided with no one." + 
            MiscUtils.AppendOptionsText(GetType());
    }

    public string GetPassives()
    {
        return
            "- Upon winning, you will be the sole winner. If the Mafia has majority, you will both win.\n" +
            "- Fools win with each other.";
    }

    public bool VotedOut()
    {

        return Player.TryGetModifier<DeathHandlerModifier>(out var deathHandler) && deathHandler.CauseOfDeath == DeathReasonShow.ExecutedByTheVillage;
    }

    public static bool AnyWon(GameOverReason gameOverReason)
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player.Data.Role is Fool fool && (fool.WinConditionMet() || fool.VotedOut())) return true;
        }
        return false;
    }

    public bool WinConditionMet()
    {
        if (MafiaGameOver.WinConditionMet()) return false; // Don't end game if Mafia has majority now.
        return VotedOut();
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return VotedOut() || AnyWon(gameOverReason);
    }
}