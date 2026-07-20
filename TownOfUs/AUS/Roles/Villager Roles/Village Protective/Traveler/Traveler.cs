using Il2CppInterop.Runtime.Attributes;
using System.Text;
using UnityEngine;

namespace AmongUsSalem.Roles;

public sealed class Traveler(IntPtr cppPtr) : CrewmateRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Traveler";
    public string RoleDescription => "";
    public string RoleLongDescription => "";
    public Color RoleColor { get; set; } = RoleColors.Village;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;

    public Faction Faction { get; set; } = Faction.Village;
    public Alignment Alignment => Alignment.VillageProtective;

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
            $"Chooses one person to stay with each night and receives visits in place of their host. Sided with the Village.\n\n" +
            "Victory Condition:\nWins if all Mafia are dead." +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new("Travel",
            "Travel to a player’s house at night to receive any visits your target gets.",
            AUSAssets.KillSprite),
    ];

    public string GetPassives()
    {
        return "- Visits you directly receive will fail while you are traveling.";
    }

    public static string Info(bool visitedTraveler)
    {
        if (visitedTraveler) return "You discovered an empty van. The Traveler must be away.";
        return $""; // The Traveler stayed at {player.Name()}'s house and received all visits in their place.
    }

    public void Role_OnRoundStart()
    {
        ModifierUtils.GetActiveModifiers<Traveled>().Do(x => x.Player.RemoveModifier(x));
    }

    public int PerformInteraction(PlayerControl visitor)
    {
        if (ModifierUtils.GetActiveModifiers<Traveled>(x => x.Caster == Player).Any())
        {
            if (visitor.Is(Faction.Village) && Debugger.IsDebuggerActive) Player.RpcAddModifier<Confirmed>();

            visitor.Notify(Info(true), NotifyMode.InstantlyAndMeeting);
            return 100;
        }

        return 0;
    }

    public void Function(PlayerControl target, int Button)
    {
        if (Button == 1)
        {
            target.RpcAddModifier<Traveled>(Player);
        }
    }
}

public sealed class Traveler_Travel : TownOfUsRoleButton<Traveler, PlayerControl>
{
    public override string Name => "Travel";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => RoleColors.Village;
    public override float Cooldown => OptionGroupSingleton<Traveler_Options>.Instance.Cooldown;
    public override LoadableAsset<Sprite> Sprite => AUSAssets.KillSprite;

    public override PlayerControl? GetTarget()
    {
        return Player.GetClosestLivingPlayer(true, Distance);
    }

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Target, 1, false, true);
}

public sealed class Traveler_Options : AbstractOptionGroup<Traveler>
{
    public override string GroupName => "Traveler";

    [ModdedNumberOption("Traveler Travel Cooldown", 2.5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 25f;
}