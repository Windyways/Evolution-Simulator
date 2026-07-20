using Il2CppInterop.Runtime.Attributes;
using System.Text;
using UnityEngine;

namespace AmongUsSalem.Roles;

public sealed class Godfather(IntPtr cppPtr) : NeutralRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Godfather";
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
            $"A Mafia that is undetectable by Cops. Sided with the Mafia.\n\n" +
            "Wins if Mafia outnumber the village. Sided with the Mafia." +
            MiscUtils.AppendOptionsText(GetType());
    }

    public string GetPassives()
    {
        return "- You appear as not guilty to Cops.";
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new("Kill",
            "Kill a player at night to kill them.",
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
            Player.RpcCustomMurder(target);
            VisitingMechanic.RpcAddDeathReason(target, (int)Player.GetDeathReason());
        }
    }
}

public sealed class Godfather_Kill : TownOfUsRoleButton<Godfather, PlayerControl>
{
    public override string Name => "Kill";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => RoleColors.Mafia;
    public override float Cooldown => OptionGroupSingleton<MafiaOptions>.Instance.Cooldown;
    public override LoadableAsset<Sprite> Sprite => AUSAssets.KillSprite;

    public override PlayerControl? GetTarget()
    {
        return Player.GetClosestLivingPlayer(false, Distance);
    }

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Target, 1, true, true);
}