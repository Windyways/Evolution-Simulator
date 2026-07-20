using Il2CppInterop.Runtime.Attributes;
using System.Text;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace AmongUsSalem.Roles;

public sealed class Cop(IntPtr cppPtr) : CrewmateRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Cop";
    public string RoleDescription => "";
    public string RoleLongDescription => "";
    public Color RoleColor { get; set; } = RoleColors.Village;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;

    public Faction Faction { get; set; } = Faction.Village;
    public Alignment Alignment => Alignment.VillageInvestigative;

    public CustomRoleConfiguration Configuration => new(this)
    {
        // Icon = AUSAssets.PilgrimRoleCard
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ICustomAURole.SetNewTabText(this);
        if (Information.Count > 0)
        {
            foreach (var info in Information)
            {
                string goodEvil = info.Item2 ? "<color=#e89349>Mafia</color>" : "<color=#a1ef75>Village</color>";
                stringB.AppendLine(AUSPlugin.Culture, $"{info.Item1.Name()} - {goodEvil}!");
            }
        }

        return stringB;
    }

    public string GetAdvancedDescription()
    {
        return
            $"Visits one person each night and receives a report of their alignment (Mafia  or Village). Sided with the Village.\n\n" +
            "Victory Condition:\nWins if all Mafia are dead." +
            MiscUtils.AppendOptionsText(GetType());
    }

    public string GetPassives()
    {
        return "- The Godfather appears to be sided with the Village.";
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new("Investigate",
            "Investigate a player at night to see if the target is sided with the Village or with the Mafia.",
            AUSAssets.KillSprite),
    ];

    public static bool IsSuspicious(PlayerControl target)
    {
        if (target.IsRole<Godfather>()) return false;

        if (target.Is(Faction.Mafia) || target.HasModifier<FramedModifier>()) return true;
        return false;
    }

    public static string Info(PlayerControl player, PlayerControl target)
    {
        player.AddModifier<TI>();
        if (IsSuspicious(target))
        {
            target.AddModifier<IncriminatingEvidence>(player);
            return $"Your investigation revealed that {target.Name()} is sided with the Mafia!";
        }

        target.AddModifier<SoftCleared>();
        return $"Your investigation revealed that {target.Name()} is sided with the village.";
    }

    public void Function(PlayerControl target, int Button)
    {
        if (Button == 1)
        {
            Information.Add((target, Cop.IsSuspicious(target)));
            Player.Notify(Cop.Info(Player, target), NotifyMode.InstantlyAndMeeting);
        }
    }

    public List<(PlayerControl, bool)> Information = new List<(PlayerControl, bool)>();
}

public sealed class Cop_Investigate : TownOfUsRoleButton<Cop, PlayerControl>
{
    public override string Name => "Investigate";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => RoleColors.Village;
    public override float Cooldown => OptionGroupSingleton<Cop_Options>.Instance.Cooldown;
    public override LoadableAsset<Sprite> Sprite => AUSAssets.KillSprite;

    public override PlayerControl? GetTarget()
    {
        return Player.GetClosestLivingPlayer(true, Distance);
    }

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Target, 1, false, true);
}

public sealed class Cop_Options : AbstractOptionGroup<Cop>
{
    public override string GroupName => "Cop";

    [ModdedNumberOption("Cop Investigate Cooldown", 2.5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 25f;
}