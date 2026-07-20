using Il2CppInterop.Runtime.Attributes;
using System.Text;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace AmongUsSalem.Roles;

public sealed class Robber(IntPtr cppPtr) : NeutralRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Robber";
    public string RoleDescription => "";
    public string RoleLongDescription => "";
    public Color RoleColor { get; set; } = RoleColors.Mafia;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;

    public Faction Faction { get; set; } = Faction.Mafia;
    public Alignment Alignment => Alignment.MafiaUtility;

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
            $"Has two attempts to steal an item. Can take a Sniper’s bullet, Bulletproof’s vest, or Gravedigger’s shovel, rendering it useless. Sided with the Werewolf.\n\n" +
            "Wins if Werewolf outnumber the village. Sided with the Werewolf." +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new("Kill",
            "Kill a player at night to kill them.",
            AUSAssets.KillSprite),

        new("Steal ( 2 )",
            "Steal a player’s item at night. If successful, your target will be notified when the day begins. This is not considered as a visit.",
            AUSAssets.KillSprite),
    ];

    public bool WinConditionMet() => MafiaGameOver.WinConditionMet();
    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet() || MafiaGameOver.AnyWon(gameOverReason);
    }

    [MethodRpc((uint)AUSRpc.RpcSteal)]
    public static void RpcSteal(PlayerControl player, PlayerControl target)
    {
        bool stole = false;
        if (target.Data.Role is Bulletproof bulletproof && bulletproof.hasVest)
        {
            bulletproof.hasVest = false;
            stole = true;
        }
        if (target.Data.Role is Sniper sniper && sniper.hasBullet)
        {
            sniper.hasBullet = false;
            stole = true;
        }
        if (target.Data.Role is Gravedigger gravedigger && gravedigger.hasShovel)
        {
            gravedigger.hasShovel = false;
            stole = true;
        }

        if (stole && player.Data.Role is Robber robber)
        {
            robber.Steals--;
        }
    }

    public void Function(PlayerControl target, int Button)
    {
        if (Button == 1)
        {
            RpcSteal(Player, target);
        }
    }

    public int Steals = 2;
}

public sealed class Robber_Steal : TownOfUsRoleButton<Robber, PlayerControl>
{
    public override string Name => "Steal";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => RoleColors.Mafia;
    public override float Cooldown => OptionGroupSingleton<Robber_Options>.Instance.Cooldown;
    public override LoadableAsset<Sprite> Sprite => AUSAssets.KillSprite;

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        if (KeybindIcon != null) KeybindIcon.transform.localPosition = new Vector3(0.4f, 0.45f, -9f);
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        if (playerControl == Player)
        {
            Button?.usesRemainingText.gameObject.SetActive(true);
            Button?.usesRemainingSprite.gameObject.SetActive(true);
            Button!.usesRemainingText.text = Role.Steals.ToString();
        }

        base.FixedUpdate(playerControl);
    }

    public override PlayerControl? GetTarget()
    {
        return Player.GetClosestLivingPlayer(false, Distance);
    }

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Target, 1, false, true);
    public override bool CanUse()
    {
        return base.CanUse() && Role.Steals > 0;
    }
}

public sealed class Robber_Options : AbstractOptionGroup<Robber>
{
    public override string GroupName => "Robber";

    [ModdedNumberOption("Robber Interrogate Cooldown", 2.5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 25f;
}