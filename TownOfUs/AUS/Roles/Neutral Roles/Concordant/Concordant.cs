using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events.Vanilla.Player;
using System.Text;
using UnityEngine;

namespace AmongUsSalem.Roles;

public sealed class Concordant(IntPtr cppPtr) : NeutralRole(cppPtr), ICustomAURole, IWikiDiscoverable, INotThreatable
{
    public string RoleName { get; set; } = "Concordant";
    public string RoleDescription => "";
    public string RoleLongDescription => "";
    public Color RoleColor { get; set; } = RoleColors.Concordant;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;

    public Faction Faction { get; set; } = Faction.Neutral;
    public Alignment Alignment => Alignment.NeutralBenign;

    public CustomRoleConfiguration Configuration => new(this)
    {
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
        //Icon = AUSAssets.Concordant
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ICustomAURole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return 
            $"I can complete tasks. If all my tasks are completed, I cannot be killed.\n" +
            "I win with the winning team if I survive until the end of the game." + 
            MiscUtils.AppendOptionsText(GetType());
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return !Player.HasDied();
    }

    public int Function()
    {
        Immunities--;
        return 1;
    }

    public int Immunities;
}

public static class ConcordantEvents
{
    [RegisterEvent()]
    public static void CompleteTaskEvent(CompleteTaskEvent @event)
    {
        var player = @event.Player;
        if (player.Data.Role is Concordant concordant) concordant.Immunities++;
    }
}