using Il2CppInterop.Runtime.Attributes;
using System.Text;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace AmongUsSalem.Roles;

public sealed class Sniper(IntPtr cppPtr) : CrewmateRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Sniper";
    public string RoleDescription => "";
    public string RoleLongDescription => "";
    public Color RoleColor { get; set; } = RoleColors.Village;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;

    public Faction Faction { get; set; } = Faction.Village;
    public Alignment Alignment => Alignment.VillageKilling;

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
            $"Can kill one person per game at night. Sided with the Village.\n\n" +
            "Victory Condition:\nWins if all Werewolf are dead." +
            MiscUtils.AppendOptionsText(GetType());
    }

    public string GetPassives()
    {
        return "- You cannot Kill the first night.";
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new("Kill ( 1 )",
            "Kill a player at night to kill them. This is not considered as a visit.",
            AUSAssets.KillSprite),
    ];

    public void Function(PlayerControl target, int Button)
    {
        if (Button == 1)
        {
            Player.RpcAddModifier<Confirmed>();

            hasBullet = false;
            Player.RpcCustomMurder(target);
            VisitingMechanic.RpcAddDeathReason(target, (int)Player.GetDeathReason());
        }
    }

    public bool hasBullet = true;
}

public sealed class Sniper_Kill : TownOfUsRoleButton<Sniper, PlayerControl>
{
    public override string Name => "Kill";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => RoleColors.Village;
    public override float Cooldown => OptionGroupSingleton<Sniper_Options>.Instance.Cooldown;
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
            Button!.usesRemainingText.text = Role.hasBullet ? "1" : "0";
        }

        base.FixedUpdate(playerControl);
    }

    public override PlayerControl? GetTarget()
    {
        return Player.GetClosestLivingPlayer(true, Distance);
    }

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Target, 1, true, true);
    public override bool CanUse()
    {
        return base.CanUse() && DayNightMechanic.NightCount != 1 && Role.hasBullet;
    }
}

public sealed class Sniper_Options : AbstractOptionGroup<Sniper>
{
    public override string GroupName => "Sniper";

    [ModdedNumberOption("Sniper Kill Cooldown", 2.5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 25f;
}