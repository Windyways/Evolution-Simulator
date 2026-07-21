using System.Text;
using Il2CppInterop.Runtime.Attributes;
using UnityEngine;

namespace AmongUsSalem.Roles;

public sealed class Crewmate(IntPtr cppPtr) : CrewmateRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Crewmate";
    public string RoleDescription => "";
    public string RoleLongDescription => "";
    public Color RoleColor { get; set; } = RoleColors.Crewmate;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;

    public Faction Faction { get; set; } = Faction.Crewmate;
    public Alignment Alignment => Alignment.CrewmateUtility;

    public CustomRoleConfiguration Configuration => new(this)
    {
        // Icon = AUSAssets.Crewmate
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ICustomAURole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return
            $"Standard vanilla crewmate." +
            MiscUtils.AppendOptionsText(GetType());
    }
}