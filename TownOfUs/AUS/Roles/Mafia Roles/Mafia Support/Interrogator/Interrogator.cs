using Il2CppInterop.Runtime.Attributes;
using System.Text;
using UnityEngine;

namespace AmongUsSalem.Roles;

public sealed class Interrogator(IntPtr cppPtr) : NeutralRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Interrogator";
    public string RoleDescription => "";
    public string RoleLongDescription => "";
    public Color RoleColor { get; set; } = RoleColors.Mafia;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;

    public Faction Faction { get; set; } = Faction.Mafia;
    public Alignment Alignment => Alignment.MafiaSupport;

    public CustomRoleConfiguration Configuration => new(this)
    {
        CanUseSabotage = OptionGroupSingleton<MafiaOptions>.Instance.CanSabotage,
        CanUseVent = OptionGroupSingleton<MafiaOptions>.Instance.CanVent,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
        // Icon = AUSAssets.SerialKillerRoleCard
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ICustomAURole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return
            $"Interrogates one person each night and finds out their exact role. That person has no memory of the interrogation. Sided with the Werewolf.\n\n" +
            "Wins if Werewolf outnumber the village. Sided with the Werewolf." +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new("Kill",
            "Kill a player at night to kill them.",
            AUSAssets.KillSprite),

        new("Interrogate",
            "Interrogate a player at night and discover their exact role.",
            AUSAssets.KillSprite),
    ];

    public bool WinConditionMet() => MafiaGameOver.WinConditionMet();
    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet() || MafiaGameOver.AnyWon(gameOverReason);
    }

    public void Function(PlayerControl target, int Button)
    {
        if (Button == 1)
        {
            target.RpcAddModifier<RoleLearn>(Player);
        }
    }
}

public sealed class Interrogator_Interrogate : TownOfUsRoleButton<Interrogator, PlayerControl>
{
    public override string Name => "Interrogate";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => RoleColors.Mafia;
    public override float Cooldown => OptionGroupSingleton<Interrogator_Options>.Instance.Cooldown;
    public override LoadableAsset<Sprite> Sprite => AUSAssets.KillSprite;

    public override PlayerControl? GetTarget()
    {
        return Player.GetClosestLivingPlayer(false, Distance, predicate: x =>
            !x.Is(Faction.Mafia) && !x.HasModifier<RoleLearn>(x => x.Visitor == Player) && !x.HasModifier<GlobalReveal>());
    }

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Target, 1, false, true);
}

public sealed class Interrogator_Options : AbstractOptionGroup<Interrogator>
{
    public override string GroupName => "Interrogator";

    [ModdedNumberOption("Interrogator Interrogate Cooldown", 2.5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 25f;
}