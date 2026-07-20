using Il2CppInterop.Runtime.Attributes;
using System.Text;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace AmongUsSalem.Roles;

public sealed class Toaster(IntPtr cppPtr) : NeutralRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Toaster";
    public string RoleDescription => "";
    public string RoleLongDescription => "";
    public Color RoleColor { get; set; } = RoleColors.Mafia;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;

    public Faction Faction { get; set; } = Faction.Mafia;
    public Alignment Alignment => Alignment.MafiaKilling;

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
            $"Choose one person at night. That person’s role is blocked and they die one night later. Sided with the Mafia.\n\n" +
            "Wins if Mafia outnumber the village. Sided with the Mafia." +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new("Toast",
            "Toast a player at night to prevent their ability. Your target will die one night later unless saved by a Doctor. You can Toast yourself.",
            AUSAssets.KillSprite),
    ];

    public bool WinConditionMet() => MafiaGameOver.WinConditionMet();
    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet() || MafiaGameOver.AnyWon(gameOverReason);
    }

    public static string Info()
    {
        return "Buttered toast was left on your doorstep. You were roleblocked!";
    }

    public void Function(PlayerControl target, int Button)
    {
        if (Button == 1)
        {
            target.RpcAddModifier<ToastedModifier>(Player);
        }
    }
}

public sealed class Toaster_Toast : TownOfUsRoleButton<Toaster, PlayerControl>
{
    public override string Name => "Toast";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => RoleColors.Mafia;
    public override float Cooldown => OptionGroupSingleton<Toaster_Options>.Instance.Cooldown;
    public override LoadableAsset<Sprite> Sprite => AUSAssets.KillSprite;

    public override PlayerControl? GetTarget()
    {
        return Player.GetClosestLivingPlayer(false, Distance);
    }

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Target, 1, false, true);
}

public sealed class Toaster_SelfToast : TownOfUsRoleButton<Toaster>
{
    public override string Name => "Self Toast";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => RoleColors.Mafia;
    public override float Cooldown => OptionGroupSingleton<Toaster_Options>.Instance.Cooldown;
    public override LoadableAsset<Sprite> Sprite => AUSAssets.KillSprite;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Player, 1, false, false);
}

public sealed class Toaster_Options : AbstractOptionGroup<Toaster>
{
    public override string GroupName => "Toaster";

    [ModdedNumberOption("Toaster Toast Cooldown", 2.5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 25f;
}