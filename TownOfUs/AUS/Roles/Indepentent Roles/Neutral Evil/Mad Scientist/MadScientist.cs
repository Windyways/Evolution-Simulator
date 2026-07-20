using Il2CppInterop.Runtime.Attributes;
using System.Text;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace AmongUsSalem.Roles;

public sealed class MadScientist(IntPtr cppPtr) : NeutralRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Mad Scientist";
    public string RoleDescription => "";
    public string RoleLongDescription => "";
    public Color RoleColor { get; set; } = RoleColors.Independent;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;

    public Faction Faction { get; set; } = Faction.Independent;
    public Alignment Alignment => Alignment.IndependentEvil;

    public CustomRoleConfiguration Configuration => new(this)
    {
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
        //Icon = AUSAssets.JesterRoleCard
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ICustomAURole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return 
            $"Experiments on one person each night. Wins if everyone alive has been experimented on. Sided with no one.\n" +
            "Wins if executed by the village during the day. Sided with no one." + 
            MiscUtils.AppendOptionsText(GetType());
    }

    public string GetPassives()
    {
        return
            "- Upon winning, you will be the sole winner.";
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new("Experiment",
            "Perform an Experiment on a player at night.",
            AUSAssets.KillSprite),
    ];

    public bool WinConditionMet()
    {
        return CheckForWin();
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet();
    }

    private bool CheckForWin()
    {
        var valuedExperimented = experimentedPlayers.Where(x => !x.HasDied()).ToList();
        var nonInfected = PlayerControl.AllPlayerControls.ToArray().Where(x =>
            !x.HasDied() && x != Player && !valuedExperimented.Contains(x)).ToList();

        if (nonInfected.Count <= 0) return true;
        return false;
    }

    public void Function(PlayerControl target, int Button)
    {
        if (Button == 1)
        {
            experimentedPlayers.Add(target);
        }
    }

    public List<PlayerControl> experimentedPlayers = new List<PlayerControl>();
}

public sealed class MadScientist_Experiment : TownOfUsRoleButton<MadScientist, PlayerControl>
{
    public override string Name => "Experiment";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => RoleColors.Independent;
    public override float Cooldown => OptionGroupSingleton<MadScientist_Options>.Instance.Cooldown;
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
            var nonInfected = PlayerControl.AllPlayerControls.ToArray().Where(x =>
                !x.HasDied() && x != Player && !Role.experimentedPlayers.Contains(x)).ToList();

            Button?.usesRemainingText.gameObject.SetActive(true);
            Button?.usesRemainingSprite.gameObject.SetActive(true);
            Button!.usesRemainingText.text = nonInfected.Count.ToString();
        }

        base.FixedUpdate(playerControl);
    }

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Target, 1, false, true);
    public override PlayerControl? GetTarget()
    {
        return Player.GetClosestLivingPlayer(true, Distance, predicate: x =>
            !Role.experimentedPlayers.Contains(x));
    }
}

public sealed class MadScientist_Options : AbstractOptionGroup<MadScientist>
{
    public override string GroupName => "Mad Scientist";

    [ModdedNumberOption("Mad Scientist Experiment Cooldown", 2.5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 25f;
}