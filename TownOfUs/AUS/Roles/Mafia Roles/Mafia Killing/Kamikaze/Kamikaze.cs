using Il2CppInterop.Runtime.Attributes;
using System.Collections;
using System.Text;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace AmongUsSalem.Roles;

public sealed class Kamikaze(IntPtr cppPtr) : NeutralRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Kamikaze";
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
            $"Chooses one person to kill during the day. The chosen person and Kamikaze both blow up and die. Sided with the Mafia.\n\n" +
            "Wins if Mafia outnumber the village. Sided with the Mafia." +
            MiscUtils.AppendOptionsText(GetType());
    }

    public string GetPassives()
    {
        return "- You cannot Blow Up while Arrested by a Deputy.";
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new("Kill",
            "Kill a player at night to kill them.",
            AUSAssets.KillSprite),

        new("Blow Up",
            "Blow Up a player during the day to kill them and yourself.",
            AUSAssets.KillSprite),
    ];

    public bool WinConditionMet() => MafiaGameOver.WinConditionMet();
    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet() || MafiaGameOver.AnyWon(gameOverReason);
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (Player.AmOwner())
        {
            meetingMenu = new MeetingMenu(
                this,
                ClickGuess,
                MeetingAbilityType.Click,
                AUSAssets.Day_KillSprite,
                null!,
                IsExempt)
            {
                Position = new Vector3(-0.40f, 0f, -3f)
            };
        }
    }

    public void Role_OnMeetingStart()
    {
        if (Player.HasDied())
            return;

        SmartKamikaze.Start(this);
        if (Player.AmOwner) Coroutines.Start(GenButtons());
    }

    public IEnumerator GenButtons(float delay = 3f)
    {
        yield return new WaitForSeconds(delay);
        meetingMenu.GenButtons(MeetingHud.Instance, Player.AmOwner && !Player.HasDied() && DayNightMechanic.DayCount >= 2 && !Player.HasModifier<ArrestedModifier>());
    }

    public override void OnVotingComplete()
    {
        RoleBehaviourStubs.OnVotingComplete(this);

        if (Player.AmOwner)
        {
            meetingMenu.HideButtons();
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        if (Player.AmOwner)
        {
            meetingMenu?.Dispose();
            meetingMenu = null!;
        }
    }

    public void ClickGuess(PlayerVoteArea voteArea, MeetingHud __)
    {
        var target = GameData.Instance.GetPlayerById(voteArea.TargetPlayerId).Object;

        // Kill and suicide.
        if (target.Data.Role is Bulletproof bulletproof && bulletproof.hasVest) bulletproof.PerformInteraction(Player, target);
        else
        {
            Player.RpcCustomMurder(target);
            VisitingMechanic.RpcAddDeathReason(target, (int)Player.GetDeathReason(DeathReasonShow.BlownUpByTheKamikaze));
        }

        Player.RpcCustomMurder(Player);
        VisitingMechanic.RpcAddDeathReason(Player, (int)Player.GetDeathReason(DeathReasonShow.BlownUpByTheKamikaze));

        if (Player.AmOwner)
        {
            meetingMenu?.HideButtons();
        }
    }

    public bool IsExempt(PlayerVoteArea voteArea)
    {
        return Player.Data.IsDead || voteArea!.AmDead || MiscUtils.PlayerById(voteArea.TargetPlayerId).Is(Faction.Mafia);
    }

    public MeetingMenu meetingMenu;
}