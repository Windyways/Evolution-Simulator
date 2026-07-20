using System.Text;
using Il2CppInterop.Runtime.Attributes;
using UnityEngine;

namespace AmongUsSalem.Roles;

public sealed class Bulletproof(IntPtr cppPtr) : CrewmateRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Bulletproof";
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
            $"Has an extra life, unless executed by the village. Sided with the Village.\n\n" +
            "Victory Condition:\nWins if all Mafia are dead." +
            MiscUtils.AppendOptionsText(GetType());
    }

    public string GetPassives()
    {
        return
            "- You will prevent any first attack you receive.\n" +
            "- You cannot be protected while you have your Kevlar vest.";
    }

    public static string Info(PlayerControl player, DeathReasonShow deathReason)
    {
        return $"{player.Name()}, the Bulletproof, was almost {deathReason.ToSpacedString()}, but has been saved by their vest!";
    }

    [MethodRpc((uint)AUSRpc.RpcAddRemoveVest)]
    public static void RpcAddRemoveVest(PlayerControl player, PlayerControl visitor, bool enableVest, DeathReasonShow dr)
    {
        if (player.Data.Role is not Bulletproof)
        {
            Logger<AUSPlugin>.Error("RpcAddRemoveVest - Invalid Bulletproof");
            return;
        }

        var role = player.GetRole<Bulletproof>();
        role.hasVest = enableVest;

        PlayerControl.LocalPlayer.Notify(Info(player, visitor.GetDeathReason(dr)), NotifyMode.OnlyMeeting);
    }

    public int PerformInteraction(PlayerControl visitor, PlayerControl bp, DeathReasonShow dr = DeathReasonShow.None)
    {
        bp.RpcAddModifier<GlobalReveal>();
        RpcAddRemoveVest(bp, visitor, false, dr);
        return 1; // Block Visit.
    }

    public bool hasVest = true;
}