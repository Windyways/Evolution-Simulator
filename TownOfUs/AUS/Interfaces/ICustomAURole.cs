using Il2CppInterop.Runtime.Attributes;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace AmongUsSalem.Interfaces;

public interface ICustomAURole : ICustomRole
{
    string RoleName { get; set; }
    Color RoleColor { get; set; }

    Faction Faction { get; set; }
    Alignment Alignment { get; }

    public virtual bool MetWinCon => false;
    public virtual string YouAreText
    {
        get
        {
            var prefix = " a";
            if (RoleName.StartsWithVowel()) prefix = " an";
            if (Configuration.MaxRoleCount is 0 or 1) prefix = " the";
            if (RoleName.StartsWith("the", StringComparison.OrdinalIgnoreCase)) prefix = "";
            return $"You are{prefix}";
        }
    }

    RoleOptionsGroup ICustomRole.RoleOptionsGroup
    {
        get
        {
            if (Alignment == Alignment.VillageInvestigative) return TouRoleGroups.VI;
            if (Alignment == Alignment.VillageKilling) return TouRoleGroups.VK;
            if (Alignment == Alignment.VillageProtective) return TouRoleGroups.VP;
            if (Alignment == Alignment.VillageSupport) return TouRoleGroups.VS;
            if (Alignment == Alignment.VillageUtility) return TouRoleGroups.VU;

            if (Alignment == Alignment.IndependentEvil) return TouRoleGroups.IE;

            if (Alignment == Alignment.MafiaDeception) return TouRoleGroups.MD;
            if (Alignment == Alignment.MafiaKilling) return TouRoleGroups.MK;
            if (Alignment == Alignment.MafiaSupport) return TouRoleGroups.MS;
            if (Alignment == Alignment.MafiaUtility) return TouRoleGroups.MU;

            return Team switch
            {
                ModdedRoleTeams.Crewmate => TouRoleGroups.VS,
                ModdedRoleTeams.Impostor => TouRoleGroups.MS,
                _ => TouRoleGroups.IE
            };
        }
    }

    bool WinConditionMet()
    {
        return false;
    }

    /// <summary>
    ///     LobbyStart - Called for each role when a lobby begins.
    /// </summary>
    void LobbyStart()
    {
    }

    public static StringBuilder SetNewTabText(ICustomRole role)
    {
        var alignment = role is ICustomAURole customRole
            ? customRole.Alignment.ToDisplayString().ApplyKeywords()
            : "Custom";

        var prefix = " a";
        if (role.RoleName.StartsWithVowel()) prefix = " an";
        if (role.Configuration.MaxRoleCount is 0 or 1) prefix = " the";
        if (role.RoleName.StartsWith("the", StringComparison.OrdinalIgnoreCase)) prefix = "";

        var stringB = new StringBuilder();
        stringB.AppendLine(CultureInfo.InvariantCulture,
            $"{role.RoleColor.ToTextColor()}You are{prefix}<b> {role.RoleName}.</b></color>");
        stringB.AppendLine(CultureInfo.InvariantCulture, $"<size=60%>Alignment: <b>{alignment}</color></b></size>");
        stringB.Append("<size=70%>");
        // stringB.AppendLine(CultureInfo.InvariantCulture, $"{role.RoleLongDescription}");

        return stringB;
    }

    public static StringBuilder SetDeadTabText(ICustomRole role)
    {
        var alignment = role is ICustomAURole customRole
            ? "<color=#ffffff" + customRole.Alignment.ToDisplayString()
            : "Custom";

        if (alignment.Contains("Village")) alignment = alignment.Replace("Town", $"<color=#" + RoleColors.Village.ToHtmlStringRGBA() + ">Village</color>");
        if (alignment.Contains("Independent")) alignment = alignment.Replace("Neutral", $"<color=#" + RoleColors.Independent.ToHtmlStringRGBA() + ">Independent</color>");
        if (alignment.Contains("Mafia")) alignment = alignment.Replace("Mafia", $"<color=#" + RoleColors.Mafia.ToHtmlStringRGBA() + ">Mafia</color>");

        var prefix = " a";
        if (role.RoleName.StartsWithVowel()) prefix = " an";
        if (role.Configuration.MaxRoleCount is 0 or 1) prefix = " the";
        if (role.RoleName.StartsWith("the", StringComparison.OrdinalIgnoreCase)) prefix = "";

        var stringB = new StringBuilder();
        stringB.AppendLine(CultureInfo.InvariantCulture,
            $"{role.RoleColor.ToTextColor()}You were{prefix}<b> {role.RoleName}.</b></color>");
        stringB.AppendLine(CultureInfo.InvariantCulture, $"<size=60%>Alignment: <b>{alignment}</color></b></size>");
        stringB.Append("<size=70%>");
        stringB.AppendLine(CultureInfo.InvariantCulture, $"{role.RoleLongDescription}");

        return stringB;
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return SetNewTabText(this);
    }

    void Function(PlayerControl target, int Button)
    {
    }


    void Role_OnMeetingStart()
    {
    }

    void Role_OnRoundStart()
    {
    }

    void Role_AfterMurder(PlayerControl killer, PlayerControl victim)
    {
    }

    void Role_OnEjection(PlayerControl? ejected, ExileController exileController)
    {
    }

    void Role_OnVisitFail(PlayerControl visitor, PlayerControl target, bool isAttacking, bool isVisiting, int blockedVisit)
    {
    }

    void Role_OnDeath(PlayerControl? player)
    {
    }
}