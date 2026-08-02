using Il2CppInterop.Runtime.Attributes;
using System.Text;
using TownOfUs.Modifiers;
using UnityEngine;

namespace AmongUsSalem.Roles;

public sealed class Pharmakos(IntPtr cppPtr) : NeutralRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Pharmakos";
    public string RoleDescription => "";
    public string RoleLongDescription => "";
    public Color RoleColor { get; set; } = RoleColors.Pharmakos;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;

    public Faction Faction { get; set; } = Faction.Neutral;
    public Alignment Alignment => Alignment.NeutralBenign;

    public CustomRoleConfiguration Configuration => new(this)
    {
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
        //Icon = AUSAssets.Pharmakos
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ICustomAURole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return 
            $"I win with the winning team if I am ejected." + 
            MiscUtils.AppendOptionsText(GetType());
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        if (Player.TryGetModifier<DeathHandlerModifier>(out var death) && death.CauseOfDeath == DeathReasonShow.Ejected) return true;
        return false;
    }
}