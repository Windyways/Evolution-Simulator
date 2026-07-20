using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Text;
using UnityEngine;
using static MeetingHud;
using static UnityEngine.GraphicsBuffer;

namespace AmongUsSalem.Roles;

public sealed class Lawyer(IntPtr cppPtr) : NeutralRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Lawyer";
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
            $"Visits one person at night. That person will appear guilty to Cops. Sided with the Mafia.\n\n" +
            "Wins if Mafia outnumber the village. Sided with the Mafia." +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new("Kill",
            "Kill a player at night to kill them.",
            AUSAssets.KillSprite),

        new("Intervene ( 1 )",
            "Protect a player at night to prevent them from being executed by the village the next day. This is not considered as a visit.",
            AUSAssets.KillSprite),
    ];

    public bool WinConditionMet() => MafiaGameOver.WinConditionMet();
    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet() || MafiaGameOver.AnyWon(gameOverReason);
    }

    public void Role_OnRoundStart()
    {
        ModifierUtils.GetActiveModifiers<IntervenedModifier>().Do(x => x.Player.RemoveModifier(x));
    }

    public void Function(PlayerControl target, int Button)
    {
        if (Button == 1)
        {
            target.RpcAddModifier<IntervenedModifier>(Player);
        }
    }
}

public sealed class Lawyer_Intervene : TownOfUsRoleButton<Lawyer, PlayerControl>
{
    public override string Name => "Intervene";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => RoleColors.Mafia;
    public override float Cooldown => OptionGroupSingleton<Lawyer_Options>.Instance.Cooldown;
    public override LoadableAsset<Sprite> Sprite => AUSAssets.KillSprite;
    public override int MaxUses => 1;

    public override PlayerControl? GetTarget()
    {
        return Player.GetClosestLivingPlayer(false, Distance);
    }

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Target, 1, false, false);
}

public sealed class Lawyer_Options : AbstractOptionGroup<Lawyer>
{
    public override string GroupName => "Lawyer";

    [ModdedNumberOption("Lawyer Intervene Cooldown", 2.5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 25f;
}