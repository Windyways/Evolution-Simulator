using Il2CppInterop.Runtime.Attributes;
using System.Text;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace AmongUsSalem.Roles;

public sealed class Deputy(IntPtr cppPtr) : CrewmateRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Deputy";
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
            $"Meets with Cops and can arrest one person each night keeping them from voting the next day. Sided with the Village.\n\n" +
            "Victory Condition:\nWins if all Mafia are dead." +
            MiscUtils.AppendOptionsText(GetType());
    }

    public string GetPassives()
    {
        return
            "- You can privately chat with Cops at night.";
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new("Arrest",
            "Arrest a player at night to prevent them from voting the next day.",
            AUSAssets.KillSprite),
    ];

    public static string Info(PlayerControl player)
    {
        return $"{player.Name()} was arrested by the Deputy and cannot vote today.";
    }

    [MethodRpc((uint)AUSRpc.RpcArrest)]
    public static void RpcArrest(PlayerControl target)
    {
        PlayerControl.LocalPlayer.Notify(Info(target), NotifyMode.OnlyMeeting);
    }

    public void Role_OnRoundStart()
    {
        ModifierUtils.GetActiveModifiers<ArrestedModifier>().Do(x => x.Player.RemoveModifier(x));
    }

    public void Function(PlayerControl target, int Button)
    {
        if (Button == 1)
        {
            Player.RpcAddModifier<Confirmed>();
            ModifierUtils.GetActiveModifiers<ArrestedModifier>(x => x.Caster == Player).Do(x => x.Player.RemoveModifier(x));

            RpcArrest(target);
            target.RpcAddModifier<ArrestedModifier>(Player);
        }
    }
}

public sealed class Deputy_Arest : TownOfUsRoleButton<Deputy, PlayerControl>
{
    public override string Name => "Arrest";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => RoleColors.Village;
    public override float Cooldown => OptionGroupSingleton<Deputy_Options>.Instance.Cooldown;
    public override LoadableAsset<Sprite> Sprite => AUSAssets.KillSprite;

    public override PlayerControl? GetTarget()
    {
        return Player.GetClosestLivingPlayer(true, Distance);
    }

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Target, 1, false, true);
}

public static class Deputy_Events
{
    [RegisterEvent()]
    public static void HandleVoteEvent(HandleVoteEvent @event)
    {
        var player = @event.VoteData.Owner;
        if (player.HasModifier<ArrestedModifier>())
        {
            @event.VoteData.SetRemainingVotes(0);
            @event.Cancel();
        }
    }
}

public sealed class Deputy_Options : AbstractOptionGroup<Deputy>
{
    public override string GroupName => "Deputy";

    [ModdedNumberOption("Deputy Arrest Cooldown", 2.5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 25f;
}