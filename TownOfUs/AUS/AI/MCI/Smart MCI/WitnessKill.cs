using UnityEngine;

namespace AmongUsSalem.MCI.SmartMCI;

public static class WitnessKill
{
    [RegisterEvent]
    public static void AfterMurderEvent(AfterMurderEvent @event)
    {
        if (MeetingHud.Instance || !Debugger.IsDebuggerActive || !Debugger.SmartBotsEnabled)
            return;

        var killer = @event.Source;
        var target = @event.Target;
        TryWitness(killer, target);
    }

    [RegisterEvent]
    public static void EnterVentEvent(EnterVentEvent @event)
    {
        if (MeetingHud.Instance || !Debugger.IsDebuggerActive || !Debugger.SmartBotsEnabled)
            return;

        var venter = @event.Player;
        TryWitness(venter, venter);
    }

    [RegisterEvent]
    public static void ExitVentEvent(ExitVentEvent @event)
    {
        if (MeetingHud.Instance || !Debugger.IsDebuggerActive || !Debugger.SmartBotsEnabled)
            return;

        var venter = @event.Player;
        TryWitness(venter, venter);
    }

    public static void TryWitness(PlayerControl killer, PlayerControl target)
    {
        if (killer.HasModifier<InvisibleStatus>())
            return;

        if (Vector2.Distance(killer.GetTruePosition(), target.GetTruePosition()) >= 0.5f)
            return;

        foreach (var witness in PlayerControl.AllPlayerControls)
        {
            if (witness != target && witness != killer && !witness.HasDied())
            {
                if (!IgnoreKill(killer, witness) && BotCanSeeKill(killer, witness, target.transform.position) && witness != target)
                {
                    // Add murder see modifier here.
                    var modifier = witness.AddModifier<SeenKill>();
                    if (modifier != null) modifier.killer = killer;

                    var modifier2 = killer.AddModifier<SeenKill>();
                    if (modifier2 != null) modifier2.killer = witness;
                }
            }
        }
    }

    public static bool WouldBeWitnessed(PlayerControl killer, PlayerControl target)
    {
        foreach (var witness in PlayerControl.AllPlayerControls)
        {
            if (witness == killer) continue;
            if (witness == target) continue;
            if (witness.HasDied()) continue;
            if (IgnoreKill(killer, witness)) continue;

            if (BotCanSeeKill(killer, witness, target.GetTruePosition()))
                return true;
        }

        return false;
    }

    public static bool IgnoreKill(PlayerControl killer, PlayerControl witness)
    {
        return killer.IsAlignedWith(witness);
    }

    public static bool BotCanSeeKill(PlayerControl killer, PlayerControl witness, Vector3 killPos)
    {
        // 1) distance
        float dist = Vector3.Distance(witness.transform.position, killPos);
        float baseVision = GameOptionsManager.Instance.currentNormalGameOptions.CrewLightMod;

        // impostor bots maybe get impostor mod — you decide:
        if (witness.Is(Faction.Impostor)) baseVision = GameOptionsManager.Instance.currentNormalGameOptions.ImpostorLightMod;
        //else if (witness.Data.Role is ICustomAURole customRole && witness.Is(Faction.Neutral)) baseVision = customRole.visionValue;

        // scale vision by map lighting (lights sabotage etc.)
        float vision = baseVision * ShipStatus.Instance.CalculateLightRadius(witness.Data);

        if (dist > vision)
            return false;

        var vector = witness.GetTruePosition() - killer.GetTruePosition();
        var magnitude = vector.magnitude;

        if (PhysicsHelpers.AnyNonTriggersBetween(killer.GetTruePosition(), vector.normalized, magnitude, Constants.ShipAndObjectsMask))
            return false;

        // AUSPlugin.DebugLogMessage($"{witness.Name()} has witnessed {killer.Name()} do something bad!");
        return true;
    }

    public static bool CanSee(PlayerControl player, GameObject target)
    {
        // 1) distance
        float dist = Vector3.Distance(player.transform.position, target.transform.position);
        float baseVision = GameOptionsManager.Instance.currentNormalGameOptions.CrewLightMod;

        // impostor bots maybe get impostor mod — you decide:
        if (player.Is(Faction.Impostor)) baseVision = GameOptionsManager.Instance.currentNormalGameOptions.ImpostorLightMod;
        //else if (witness.Data.Role is ICustomAURole customRole && witness.Is(Faction.Neutral)) baseVision = customRole.visionValue;

        // scale vision by map lighting (lights sabotage etc.)
        float vision = baseVision * ShipStatus.Instance.CalculateLightRadius(player.Data);

        if (dist > vision)
            return false;

        var vector = player.GetTruePosition() - (Vector2)target.transform.position;
        var magnitude = vector.magnitude;

        if (PhysicsHelpers.AnyNonTriggersBetween(target.transform.position, vector.normalized, magnitude, Constants.ShipAndObjectsMask))
            return false;

        // AUSPlugin.DebugLogMessage($"{witness.Name()} has witnessed {killer.Name()} do something bad!");
        return true;
    }

    public static List<PlayerControl> IsolatedPlayers(PlayerControl player)
    {
        /*var list = new List<PlayerControl>();
        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (player.Is(Faction.Impostor))
            {
                var playersInVision = PlayerControl.AllPlayerControls.ToArray().Count(x => x != p && !x.HasDied() && !x.Is(Faction.Impostor) && CanSee(x, p.gameObject));
                if (playersInVision == 0) list.Add(p);
            }
            else
            {
                var playersInVision = PlayerControl.AllPlayerControls.ToArray().Count(x => x != p! && x.HasDied() && x != player && CanSee(x, p.gameObject));
                if (playersInVision == 0) list.Add(p);
            }
        }

        return list;*/

        if (player.Is(Faction.Impostor)) return PlayerControl.AllPlayerControls.ToArray().Where(x => !x.HasDied() && !x.Is(Faction.Impostor) && x.GetPlayerRoom() == player.GetPlayerRoom()).ToList();
        return PlayerControl.AllPlayerControls.ToArray().Where(x => !x.HasDied() && x != player && x.GetPlayerRoom() == player.GetPlayerRoom()).ToList();
    }

    public static List<PlayerControl> PlayersInRoom(this PlayerControl player)
    {
        List<PlayerControl> count = new List<PlayerControl>();
        count.Add(player);
        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (p != player && !p.HasDied() && player.GetPlayerRoom() == p.GetPlayerRoom())
            {
                count.Add(p);
            }
        }

        return count;
    }

    public static bool PlayerIsInRoomAlone(this PlayerControl player) => player.PlayersInRoom().Count == 1;

    public static bool IsIsolated(PlayerControl player)
    {
        bool isolated = true;
        foreach (var t in PlayerControl.AllPlayerControls)
        {
            if (t.HasDied() || t == player) continue;
            if (player.IsAlignedWith(t)) continue;

            if (CanSee(t, player.gameObject))
            {
                isolated = false;
            }
        }

        return isolated;
    }
}