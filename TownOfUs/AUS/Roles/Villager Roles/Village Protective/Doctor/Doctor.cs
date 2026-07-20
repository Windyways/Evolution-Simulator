using Il2CppInterop.Runtime.Attributes;
using System.Text;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace AmongUsSalem.Roles;

public sealed class Doctor(IntPtr cppPtr) : CrewmateRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Doctor";
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
            $"Visits one person each night and protects them from dying that night. Sided with the Village.\n\n" +
            "Victory Condition:\nWins if all Mafia are dead." +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new("Save",
            "Choose someone to Save at night to prevent their death that night. You can Save yourself.",
            AUSAssets.KillSprite),
    ];

    public static string Info(PlayerControl player, DeathReasonShow deathReason)
    {
        return $"{player.Name()} was almost {deathReason.ToSpacedString()}, but was saved by the doctor.";
    }

    public void Role_OnRoundStart()
    {
        ModifierUtils.GetActiveModifiers<SavedModifier>().Do(x => x.Player.RemoveModifier(x));
    }

    public void Function(PlayerControl target, int Button)
    {
        if (Button == 1)
        {
            ModifierUtils.GetActiveModifiers<SavedModifier>(x => x.Caster == Player).Do(x => x.Player.RemoveModifier(x));
            target.RpcAddModifier<SavedModifier>(Player);
        }
    }
}

public sealed class Doctor_Save : TownOfUsRoleButton<Doctor, PlayerControl>
{
    public override string Name => "Save";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => RoleColors.Village;
    public override float Cooldown => OptionGroupSingleton<Doctor_Options>.Instance.Cooldown;
    public override LoadableAsset<Sprite> Sprite => AUSAssets.KillSprite;

    public override PlayerControl? GetTarget()
    {
        return Player.GetClosestLivingPlayer(true, Distance);
    }

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Target, 1, false, true);
}

public sealed class Doctor_SelfSave : TownOfUsRoleButton<Doctor>
{
    public override string Name => "Self Save";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => RoleColors.Village;
    public override float Cooldown => OptionGroupSingleton<Doctor_Options>.Instance.Cooldown;
    public override LoadableAsset<Sprite> Sprite => AUSAssets.KillSprite;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Player, 1, false, false);
}

public sealed class Doctor_Options : AbstractOptionGroup<Doctor>
{
    public override string GroupName => "Doctor";

    [ModdedNumberOption("Doctor Save Cooldown", 2.5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 25f;
}