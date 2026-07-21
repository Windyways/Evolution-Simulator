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
            if (Faction == Faction.Crewmate) return TouRoleGroups.Crewmate;
            if (Faction == Faction.Neutral) return TouRoleGroups.Neutral;
            if (Faction == Faction.Impostor) return TouRoleGroups.Impostor;

            /*if (Alignment == Alignment.CrewmateInvestigative) return TouRoleGroups.VI;
            if (Alignment == Alignment.CrewmateKilling) return TouRoleGroups.VK;
            if (Alignment == Alignment.CrewmateProtective) return TouRoleGroups.VP;
            if (Alignment == Alignment.CrewmateSupport) return TouRoleGroups.VS;
            if (Alignment == Alignment.CrewmateUtility) return TouRoleGroups.VU;

            if (Alignment == Alignment.NeutralEvil) return TouRoleGroups.IE;

            if (Alignment == Alignment.ImpostorDeception) return TouRoleGroups.MD;
            if (Alignment == Alignment.ImpostorKilling) return TouRoleGroups.MK;
            if (Alignment == Alignment.ImpostorSupport) return TouRoleGroups.MS;
            if (Alignment == Alignment.ImpostorUtility) return TouRoleGroups.MU;*/

            return Team switch
            {
                ModdedRoleTeams.Crewmate => TouRoleGroups.Crewmate,
                ModdedRoleTeams.Impostor => TouRoleGroups.Impostor,
                _ => TouRoleGroups.Neutral
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

        if (alignment.Contains("Crewmate")) alignment = alignment.Replace("Crewmate", $"<color=#" + RoleColors.Crewmate.ToHtmlStringRGBA() + ">Crewmate</color>");
        if (alignment.Contains("Neutral")) alignment = alignment.Replace("Neutral", $"<color=#" + RoleColors.Neutral.ToHtmlStringRGBA() + ">Neutral</color>");
        if (alignment.Contains("Impostor")) alignment = alignment.Replace("Impostor", $"<color=#" + RoleColors.Impostor.ToHtmlStringRGBA() + ">Impostor</color>");

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

    void DoVisit(PlayerControl target, int Button, bool visiting, bool kill)
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