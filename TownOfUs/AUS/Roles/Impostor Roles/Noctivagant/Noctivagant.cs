using Il2CppInterop.Runtime.Attributes;
using System.Text;
using UnityEngine;

namespace AmongUsSalem.Roles;

public sealed class Noctivagant(IntPtr cppPtr) : NeutralRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Noctivagant";
    public string RoleDescription => "";
    public string RoleLongDescription => "";
    public Color RoleColor { get; set; } = RoleColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;

    public Faction Faction { get; set; } = Faction.Impostor;
    public Alignment Alignment => Alignment.ImpostorUtility;

    public CustomRoleConfiguration Configuration => new(this)
    {
        CanUseSabotage = OptionGroupSingleton<ImpostorOptions>.Instance.CanSabotage,
        CanUseVent = OptionGroupSingleton<ImpostorOptions>.Instance.CanVent,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
        // Icon = AUSAssets.Noctivagant
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ICustomAURole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return
            $"I can kill players.\n" +
            "If I am out of range, I go invisible for this turn." +
            MiscUtils.AppendOptionsText(GetType());
    }

    public bool WinConditionMet() => ImpostorGameOver.WinConditionMet();
    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet() || ImpostorGameOver.AnyWon(gameOverReason);
    }

    public void DoVisit(PlayerControl target, int Button, bool visiting, bool kill) => VisitingMechanic.CheckVisit(Player, target, Button, kill, visiting);
    public void Function(PlayerControl target, int Button)
    {
        if (Button == 1)
        {
            Player.RpcCustomMurder(target, showKillAnim: false);
            VisitingMechanic.RpcAddDeathReason(target, DeathReasonShow.KilledByANoctivagant, RoleColors.Impostor);
        }
    }

    public void OnTurnEnd()
    {
        IsolatedPlayer = null;

        var players2 = ModifierUtils.GetPlayersWithModifier<TrackerArrowTargetModifier>([HideFromIl2Cpp] (x) => x.Owner == Player);
        foreach (var player in players2) player.RemoveModifier<TrackerArrowTargetModifier>();
    }

    public void OnTurnStart()
    {
        var players = PlayerControl.AllPlayerControls.ToArray().ToList();
        players.Shuffle();
        foreach (var t in players.OrderBy(x => Vector2.Distance(x.transform.position, Player.transform.position)))
        {
            if (t.HasDied() || t == Player) continue;
            if (t.Is(Faction.Impostor)) continue;

            if (!WitnessKill.WouldBeWitnessed(Player, t))
            {
                IsolatedPlayer = t;

                Color color = Color.red;
                var update = 0.1f;

                t.AddModifier<TrackerArrowTargetModifier>(Player, color, update);
                break;
            }
        }
    }

    public PlayerControl IsolatedPlayer;
}