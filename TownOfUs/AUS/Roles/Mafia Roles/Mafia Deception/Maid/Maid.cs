using Il2CppInterop.Runtime.Attributes;
using System.Text;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace AmongUsSalem.Roles;

public sealed class Maid(IntPtr cppPtr) : NeutralRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Maid";
    public string RoleDescription => "";
    public string RoleLongDescription => "";
    public Color RoleColor { get; set; } = RoleColors.Mafia;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;

    public Faction Faction { get; set; } = Faction.Mafia;
    public Alignment Alignment => Alignment.MafiaDeception;

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
            $"Visits one person each night and cleans up the body if they die, leaving their role and the cause of death a mystery. Sided with the Mafia.\n\n" +
            "Wins if Mafia outnumber the village. Sided with the Mafia." +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new("Clean Up",
            "Clean Up a dead player to hide their death reason and role from everyone.",
            AUSAssets.KillSprite),
    ];

    public bool WinConditionMet() => MafiaGameOver.WinConditionMet();
    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet() || MafiaGameOver.AnyWon(gameOverReason);
    }

    public void Role_OnRoundStart()
    {
        ModifierUtils.GetActiveModifiers<CleanedUp>().Do(x => x.Player.RemoveModifier(x));
    }

    public void Role_AfterMurder(PlayerControl killer, PlayerControl victim)
    {
        if (victim.HasModifier<CleanedUp>())
        {
            foreach (var p in PlayerControl.AllPlayerControls)
            {
                if (p.AmOwner()) victim.AddModifier<DeepfakeRole>(p, "Mystery", Color.gray);
            }
        }
    }

    public void Function(PlayerControl target, int Button)
    {
        if (Button == 1)
        {
            target.RpcAddModifier<CleanedUp>(Player);
        }
    }
}

public sealed class Maid_CleanUp : TownOfUsRoleButton<Maid, PlayerControl>
{
    public override string Name => "Clean Up";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => RoleColors.Mafia;
    public override float Cooldown => OptionGroupSingleton<Maid_Options>.Instance.Cooldown;
    public override LoadableAsset<Sprite> Sprite => AUSAssets.KillSprite;

    public override PlayerControl? GetTarget()
    {
        return Player.GetClosestLivingPlayer(false, Distance);
    }

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Target, 1, false, false);
}

public sealed class Maid_Options : AbstractOptionGroup<Maid>
{
    public override string GroupName => "Maid";

    [ModdedNumberOption("Maid Clean Up Cooldown", 2.5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 25f;
}