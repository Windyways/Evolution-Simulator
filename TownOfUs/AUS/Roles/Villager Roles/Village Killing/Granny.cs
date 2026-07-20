using Il2CppInterop.Runtime.Attributes;
using System.Text;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace AmongUsSalem.Roles;

public sealed class Granny(IntPtr cppPtr) : CrewmateRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Granny";
    public string RoleDescription => "";
    public string RoleLongDescription => "";
    public Color RoleColor { get; set; } = RoleColors.Village;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;

    public Faction Faction { get; set; } = Faction.Village;
    public Alignment Alignment => Alignment.VillageKilling;

    public CustomRoleConfiguration Configuration => new(this)
    {
        // Icon = AUSAssets.PilgrimRoleCard
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ICustomAURole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return
            $"Kills anyone who visits her in the night. Sided with the Village.\n\n" +
            "Victory Condition:\nWins if all Mafia are dead." +
            MiscUtils.AppendOptionsText(GetType());
    }

    public string GetPassives()
    {
        return "- Kill all players that visit you at night.";
    }

    public static string Info(PlayerControl player)
    {
        return $"You visited {player.name}, the Granny, and survived.";
    }

    public int PerformInteraction(PlayerControl visitor, bool isAttacking)
    {
        Player.RpcAddModifier<Confirmed>();
        if (visitor.TryGetModifier<SavedModifier>(out var saved))
        {
            saved.PerformInteraction(visitor, Player);
            visitor.Notify(Info(Player), NotifyMode.InstantlyAndMeeting);
        }
        else if (!visitor.IsRole<Henchman>())
        {
            Player.RpcCustomMurder(visitor);
            VisitingMechanic.RpcAddDeathReason(visitor, (int)Player.GetDeathReason());
        }

        if (!isAttacking) return 0;
        return 1; // Block Visit.
    }
}