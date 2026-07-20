using Il2CppInterop.Runtime.Attributes;
using System.Text;
using UnityEngine;

namespace AmongUsSalem.Roles;

public sealed class Gravedigger(IntPtr cppPtr) : CrewmateRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Gravedigger";
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
            $"Can talk to dead members at night. May choose one person to revive, but will die in their place. Sided with the Village.\n\n" +
            "Victory Condition:\nWins if all Mafia are dead." +
            MiscUtils.AppendOptionsText(GetType());
    }

    public string GetPassives()
    {
        return
            "- You may chat to dead players at night.\n" +
            "- You cannot revive a Fairy.";
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new("Revive",
            "Revive a player at night to revive them and suicide in return.",
            AUSAssets.KillSprite),
    ];

    public static string Info(PlayerControl gravedigger, PlayerControl target)
    {
        return $"{target.Name()} was revived by {gravedigger.Name()}, the Gravedigger.";
    }

    public void Function(PlayerControl target, int Button)
    {
        if (Button == 1)
        {
            var player1Menu = CustomPlayerMenu.Create();
            player1Menu.transform.FindChild("PhoneUI").GetChild(0).GetComponent<SpriteRenderer>().material =
                PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
            player1Menu.transform.FindChild("PhoneUI").GetChild(1).GetComponent<SpriteRenderer>().material =
                PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;

            player1Menu.Begin(
                plr => (plr.HasDied() && !plr.Is(Faction.Mafia) && !plr.IsRole<Fairy>()),
                plr =>
                {
                    player1Menu.ForceClose();

                    if (plr == null)
                    {
                        return;
                    }

                    // Stuff here.
                    ModifierUtils.GetActiveModifiers<PendingReviveModifier>(x => x.Caster == Player).Do(x => x.Player.RemoveModifier(x));
                    plr.RpcAddModifier<PendingReviveModifier>(Player);
                }
            );
            foreach (var panel in player1Menu.potentialVictims)
            {
                panel.PlayerIcon.cosmetics.SetPhantomRoleAlpha(1f);
                if (panel.NameText.text != PlayerControl.LocalPlayer.Data.PlayerName)
                {
                    panel.NameText.color = Color.white;
                }
            }
        }
    }

    public bool hasShovel = true;
}

public sealed class Gravedigger_Revive : TownOfUsRoleButton<Gravedigger>
{
    public override string Name => "Revive";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => RoleColors.Village;
    public override float Cooldown => OptionGroupSingleton<Gravedigger_Options>.Instance.Cooldown;
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
            Button!.usesRemainingText.text = !Role.hasShovel ? "0" : "1";
        }

        base.FixedUpdate(playerControl);
    }

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Player, 1, false, false);
    public override bool CanUse()
    {
        return base.CanUse() && Role.hasShovel;
    }
}

public sealed class Gravedigger_Options : AbstractOptionGroup<Gravedigger>
{
    public override string GroupName => "Gravedigger";

    [ModdedNumberOption("Gravedigger Revive Cooldown", 2.5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 25f;
}