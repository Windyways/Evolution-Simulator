using Il2CppInterop.Runtime.Attributes;
using System.Text;
using UnityEngine;

namespace AmongUsSalem.Roles;

public sealed class Survivor(IntPtr cppPtr) : NeutralRole(cppPtr), ICustomAURole, IWikiDiscoverable, INotThreatable
{
    public string RoleName { get; set; } = "Survivor";
    public string RoleDescription => "";
    public string RoleLongDescription => "";
    public Color RoleColor { get; set; } = RoleColors.Survivor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;

    public Faction Faction { get; set; } = Faction.Neutral;
    public Alignment Alignment => Alignment.NeutralBenign;

    public CustomRoleConfiguration Configuration => new(this)
    {
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
        //Icon = AUSAssets.Survivor
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ICustomAURole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return 
            $"You win with the winning team if you survive until the end of the game." + 
            MiscUtils.AppendOptionsText(GetType());
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return !PlayerControl.LocalPlayer.HasDied();
    }
}