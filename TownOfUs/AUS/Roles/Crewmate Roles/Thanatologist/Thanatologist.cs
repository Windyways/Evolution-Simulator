using System.Text;
using Il2CppInterop.Runtime.Attributes;
using UnityEngine;

namespace AmongUsSalem.Roles;

public sealed class Thanatologist(IntPtr cppPtr) : CrewmateRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Thanatologist";
    public string RoleDescription => "";
    public string RoleLongDescription => "";
    public Color RoleColor { get; set; } = RoleColors.Crewmate;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;

    public Faction Faction { get; set; } = Faction.Crewmate;
    public Alignment Alignment => Alignment.CrewmateUtility;

    public CustomRoleConfiguration Configuration => new(this)
    {
        // Icon = AUSAssets.Thanatologist
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ICustomAURole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return
            $"I can complete tasks." +
            $"I know where all dead bodies are and prioritize going to them.\n" +
            MiscUtils.AppendOptionsText(GetType());
    }

    public void OnTurnEnd()
    {
        TargetPlayer = null;

        var players2 = ModifierUtils.GetPlayersWithModifier<TrackerArrowTargetModifier>([HideFromIl2Cpp] (x) => x.Owner == Player);
        foreach (var player in players2) player.RemoveModifier<TrackerArrowTargetModifier>();
    }

    public void OnTurnStart()
    {
        var deadBodies = UnityEngine.Object.FindObjectsOfType<DeadBody>().ToList();
        foreach (var db in deadBodies)
        {
            var t = MiscUtils.PlayerById(db.ParentId);
            Color color = RoleColors.Crewmate;
            var update = 0.1f;

            t.AddModifier<TrackerArrowTargetModifier>(Player, color, update);
        }

        if (deadBodies.Count > 0)
        {
            TargetPlayer = MiscUtils.PlayerById(deadBodies.OrderBy(x => Vector2.Distance(x.transform.position, Player.transform.position)).FirstOrDefault().ParentId);
        }
    }

    public PlayerControl TargetPlayer;
}