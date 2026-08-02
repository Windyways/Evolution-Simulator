using System.Text;
using Il2CppInterop.Runtime.Attributes;
using UnityEngine;

namespace AmongUsSalem.Roles;

public sealed class Itinerant(IntPtr cppPtr) : CrewmateRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Itinerant";
    public string RoleDescription => "";
    public string RoleLongDescription => "";
    public Color RoleColor { get; set; } = RoleColors.Crewmate;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;

    public Faction Faction { get; set; } = Faction.Crewmate;
    public Alignment Alignment => Alignment.CrewmateUtility;

    public CustomRoleConfiguration Configuration => new(this)
    {
        // Icon = AUSAssets.Itinerant
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ICustomAURole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return
            $"I can complete tasks." +
            $"I have the same vision as Impostors do.\n" +
            $"My turn only ends after reaching my destination.\n" +
            MiscUtils.AppendOptionsText(GetType());
    }
}