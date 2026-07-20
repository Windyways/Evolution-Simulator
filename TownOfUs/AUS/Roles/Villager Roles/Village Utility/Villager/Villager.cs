using System.Text;
using Il2CppInterop.Runtime.Attributes;
using UnityEngine;

namespace AmongUsSalem.Roles;

public sealed class Villager(IntPtr cppPtr) : CrewmateRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Villager";
    public string RoleDescription => "";
    public string RoleLongDescription => "";
    public Color RoleColor { get; set; } = RoleColors.Village;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;

    public Faction Faction { get; set; } = Faction.Village;
    public Alignment Alignment => Alignment.VillageUtility;

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
            $"Wins if all Mafia are dead. Sided with the Village.\n\n" +
            "Victory Condition:\nWins if all Mafia are dead." +
            MiscUtils.AppendOptionsText(GetType());
    }
}