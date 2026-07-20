using TownOfUs.Options;
using UnityEngine;

namespace TownOfUs.Utilities;

public static class PlayerRoleTextExtensions
{
    public static Color UpdateTargetColor(this Color color, PlayerControl player, bool hidden = false)
    {
        if (PlayerControl.LocalPlayer.Data.Role is Deputy deputy && player.HasModifier<ArrestedModifier>(x => x.Caster == deputy.Player))
        {
            color = RoleColors.Village;
        }

        if (PlayerControl.LocalPlayer.Data.Role is Doctor doctor && player.HasModifier<SavedModifier>(x => x.Caster == doctor.Player))
        {
            color = RoleColors.Village;
        }

        if (PlayerControl.LocalPlayer.Data.Role is Traveler traveler && player.HasModifier<Traveled>(x => x.Caster == traveler.Player))
        {
            color = RoleColors.Village;
        }

        if (PlayerControl.LocalPlayer.Data.Role is MadScientist madScientist && madScientist.experimentedPlayers.Contains(player))
        {
            color = RoleColors.Independent;
        }

        if (player.HasModifier<FramedModifier>() && PlayerControl.LocalPlayer.Is(Faction.Mafia))
        {
            color = RoleColors.Mafia;
        }

        if (player.HasModifier<ToastedModifier>() && PlayerControl.LocalPlayer.Is(Faction.Mafia))
        {
            color = RoleColors.Mafia;
        }

        if (player.HasModifier<CleanedUp>() && PlayerControl.LocalPlayer.Is(Faction.Mafia))
        {
            color = RoleColors.Mafia;
        }
        return color;
    }

    public static string UpdateTargetSymbols(this string name, PlayerControl player, bool hidden = false)
    {
        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
        return name;
    }

    public static string UpdateProtectionSymbols(this string name, PlayerControl player, bool hidden = false)
    {
        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
        return name;
    }

    public static string UpdateAllianceSymbols(this string name, PlayerControl player, bool hidden = false)
    {
        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
        return name;
    }

    public static string UpdateStatusSymbols(this string name, PlayerControl player, bool hidden = false)
    {
        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;

        if ((PlayerControl.LocalPlayer.Data.Role is Deputy deputy && player.HasModifier<ArrestedModifier>(x => x.Caster == deputy.Player))
            || (player.HasModifier<ArrestedModifier>() && MeetingHud.Instance)
            || (player.HasModifier<ArrestedModifier>() && PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !hidden))
        {
            name += "<color=#3f7095> Ⓐ</color>";
        }

        if ((PlayerControl.LocalPlayer.Data.Role is Toaster toaster && player.HasModifier<ToastedModifier>(x => x.Caster == toaster.Player))
            || (player.HasModifier<ToastedModifier>() && PlayerControl.LocalPlayer.Is(Faction.Mafia))
            || (player.HasModifier<ToastedModifier>() && PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !hidden))
        {
            name += "<color=#903e3f> Ⓣ</color>";
        }

        if ((PlayerControl.LocalPlayer.Data.Role is Maid maid && player.HasModifier<CleanedUp>(x => x.Caster == maid.Player))
            || (player.HasModifier<CleanedUp>() && PlayerControl.LocalPlayer.Is(Faction.Mafia))
            || (player.HasModifier<CleanedUp>() && PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !hidden))
        {
            name += "<color=#903e3f> Ⓒ</color>";
        }

        if ((player.HasModifier<FramedModifier>() && PlayerControl.LocalPlayer.Is(Faction.Mafia))
            || (player.HasModifier<FramedModifier>() && PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !hidden))
        {
            name += "<color=#903e3f> Ⓕ</color>";
        }

        if ((player.HasModifier<ThreatenedModifier>() && PlayerControl.LocalPlayer.Is(Faction.Mafia))
            || (player.HasModifier<ThreatenedModifier>() && PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !hidden))
        {
            name += "<size=30%><color=#903e3f> (Threatened)</color></size>";
        }
        return name;
    }
}


/* Status Alphabet

Ⓐ
Ⓑ
Ⓒ
Ⓓ
Ⓔ
Ⓕ
Ⓖ
Ⓗ
Ⓘ
Ⓙ
Ⓚ
Ⓛ
Ⓜ
Ⓝ
Ⓞ
Ⓟ
Ⓠ
Ⓡ
Ⓢ
Ⓣ
Ⓤ
Ⓥ
Ⓦ
Ⓧ
Ⓨ
Ⓩ

*/