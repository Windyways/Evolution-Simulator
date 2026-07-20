using Il2CppInterop.Runtime.Attributes;
using System.Text;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace AmongUsSalem.Roles;

public sealed class Fairy(IntPtr cppPtr) : CrewmateRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Fairy";
    public string RoleDescription => "";
    public string RoleLongDescription => "";
    public Color RoleColor { get; set; } = RoleColors.Village;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;

    public Faction Faction { get; set; } = Faction.Village;
    public Alignment Alignment => Alignment.VillageSupport;

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
            $"Restores a used or stolen item once per game. Can restore a Sniper’s bullet, Gravedigger’s shovel, or Bulletproof’s vest. Sided with the Village.\n\n" +
            "Victory Condition:\nWins if all Mafia are dead." +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new("Restore ( 1 )",
            "Restore a player’s item at night to give them an item if they ran out. You will know if this was successful.",
            AUSAssets.KillSprite),
    ];

    /*public static string Info(PlayerControl player)
    {
        return $"{player.Name()} was arrested by the Fairy and cannot vote today.";
    }*/

    [MethodRpc((uint)AUSRpc.RpcRestore)]
    public static void RpcRestore(PlayerControl player, PlayerControl target)
    {
        bool restored = false;
        if (target.Data.Role is Bulletproof bulletproof && !bulletproof.hasVest)
        {
            bulletproof.hasVest = true;
            restored = true;
        }
        if (target.Data.Role is Sniper sniper && !sniper.hasBullet)
        {
            sniper.hasBullet = true;
            restored = true;
        }
        if (target.Data.Role is Gravedigger gravedigger && !gravedigger.hasShovel)
        {
            gravedigger.hasShovel = true;
            restored = true;
        }

        if (restored && player.Data.Role is Fairy fairy)
        {
            fairy.hasGivenItem = true;
            player.RpcAddModifier<Confirmed>();
        }
    }

    public void Function(PlayerControl target, int Button)
    {
        if (Button == 1)
        {
            RpcRestore(Player, target);
        }
    }

    public bool hasGivenItem;
}

public sealed class Fairy_Restore : TownOfUsRoleButton<Fairy, PlayerControl>
{
    public override string Name => "Restore";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => RoleColors.Village;
    public override float Cooldown => OptionGroupSingleton<Fairy_Options>.Instance.Cooldown;
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
            Button!.usesRemainingText.text = Role.hasGivenItem ? "0" : "1";
        }

        base.FixedUpdate(playerControl);
    }

    public override PlayerControl? GetTarget()
    {
        return Player.GetClosestLivingPlayer(true, Distance);
    }

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Target, 1, false, true);
    public override bool CanUse()
    {
        return base.CanUse() && !Role.hasGivenItem;
    }
}

public sealed class Fairy_Options : AbstractOptionGroup<Fairy>
{
    public override string GroupName => "Fairy";

    [ModdedNumberOption("Fairy Restore Cooldown", 2.5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 25f;
}