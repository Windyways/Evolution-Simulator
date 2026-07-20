using Random = UnityEngine.Random;

namespace AmongUsSalem.MCI.SmartMCI;

public static class CalculatedVoting
{
    public static byte SkipVote(PlayerControl player, MeetingHud __instance)
    {
        __instance.CmdCastVote(player.PlayerId, __instance.SkipVoteButton.TargetPlayerId);
        return __instance.SkipVoteButton.TargetPlayerId;
    }

    public static byte RandomVote(PlayerControl player, MeetingHud __instance, List<PlayerControl> validTargets, bool canSkip = true)
    {
        bool skip = Random.RandomRangeInt(-1, validTargets.Count) == -1;
        if (skip && canSkip) return SkipVote(player, __instance);

        PlayerControl newTarget = validTargets[Random.RandomRangeInt(0, validTargets.Count)];
        validTargets.Remove(newTarget);
        __instance.CmdCastVote(player.PlayerId, newTarget.PlayerId);
        return newTarget.PlayerId;
    }

    public static byte TryCastVote(PlayerControl player, MeetingHud __instance, byte target)
    {
        __instance.CmdCastVote(player.PlayerId, target);
        return target;
    }

    public static byte TryCastVote(PlayerControl player, MeetingHud __instance, PlayerControl target)
    {
        if (target == null) return SkipVote(player, __instance);

        __instance.CmdCastVote(player.PlayerId, target.PlayerId);
        return target.PlayerId;
    }

    public static void DoVotes(MeetingHud __instance)
    {
        declaredVillageTarget = (byte.MinValue, false);
        lastMafiaVoteTarget = (byte.MinValue, false);

        // var alivePlayers = PlayerControl.AllPlayerControls.ToArray().Where(x => !x.HasDied()).ToList();
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            byte voted = 0;
            if (player.Is(Faction.Mafia)) voted = MafiaVoting(player, __instance);
            else if (player.Is(Faction.Village)) voted = VillageVoting(player, __instance);
            else if (player.IsRole<Fool>()) voted = FoolVoting(player, __instance);
            else if (player.IsRole<MadScientist>()) voted = MadScientistVoting(player, __instance);

            if (voted == MeetingHud.Instance.SkipVoteButton.TargetPlayerId) AUS_AfterVoteEvent.RoleFunctionOnSkip(player);
            else AUS_AfterVoteEvent.RoleFunctionOnVote(player, MiscUtils.PlayerById(voted));
        }
    }

    /// <summary>
    /// If a player is Confirmed Evil, vote them.
    /// If a player witnesses a kill, vote who they saw kill with a % chance. If failed, vote the witness.
    /// If there are less than 7 players, vote a random player that isn't yourself.
    /// Abstain otherwise.
    /// </summary>
    public static (byte, bool) declaredVillageTarget = (byte.MinValue, false);
    public static byte VillageVoting(PlayerControl player, MeetingHud __instance)
    {
        var allPlayers = PlayerControl.AllPlayerControls.ToArray().Where(x => !x.HasDied()).ToList();
        var validPlayers = PlayerControl.AllPlayerControls.ToArray().Where(x =>
            !x.HasDied() && x != player && !x.HasModifier<TI>() && !x.HasModifier<Confirmed>() && !x.HasModifier<SoftCleared>() && 
            !(x.Is(Faction.Village) && x.HasModifier<GlobalReveal>())).ToList();

        var seenKill = SeenKill.GetAll();
        var confirmedEvil = ConfirmedEvil.GetAll();
        var incriminatingEvidence = ModifierUtils.GetPlayersWithModifier<IncriminatingEvidence>(x => validPlayers.Contains(x.Player) && !x.Finder.HasModifier<ThreatenedModifier>()).ToList();
        var icTarget = incriminatingEvidence.Random();

        if (declaredVillageTarget.Item2 && ChanceIs(80)) return TryCastVote(player, __instance, declaredVillageTarget.Item1);
        else if (confirmedEvil != null) declaredVillageTarget = (TryCastVote(player, __instance, confirmedEvil.Player), true);
        else if (icTarget != null && !icTarget.HasDied()) declaredVillageTarget = (TryCastVote(player, __instance, icTarget), true);
        else if (seenKill != null)
        {
            var a = seenKill.killer;
            var b = seenKill.Player;

            int suspicionA = SeenKill.GetSuspicion(a);
            int suspicionB = SeenKill.GetSuspicion(b);

            // Bias by the witness credibility
            if (ChanceIs(seenKill.voteChance)) suspicionA += 25;
            else suspicionB += 25;

            if (suspicionA > suspicionB) return TryCastVote(player, __instance, a);
            else if (suspicionB > suspicionA) return TryCastVote(player, __instance, b);
            else return TryCastVote(player, __instance, UnityEngine.Random.value < 0.5f ? a : b);
        }
        else if (validPlayers.Count > 0) return RandomVote(player, __instance, validPlayers, allPlayers.Count >= 7);
        else return SkipVote(player, __instance);

        return declaredVillageTarget.Item1;
    }

    /// <summary>
    /// Mafia has a 30% chance to vote with each other.
    /// If a Mafia is caught, vote the accuser.
    /// If there are less than 7 players, vote a random Non-Mafia.
    /// Abstain otherwise.
    /// </summary>
    public static (byte, bool) lastMafiaVoteTarget = (byte.MinValue, false);
    public static byte MafiaVoting(PlayerControl player, MeetingHud __instance)
    {
        var allPlayers = PlayerControl.AllPlayerControls.ToArray().Where(x => !x.HasDied()).ToList();
        var validPlayers = PlayerControl.AllPlayerControls.ToArray().Where(x =>
            !x.HasDied() && !x.Is(Faction.Mafia)).ToList();

        var seenKill = SeenKill.GetAll();

        var incriminatingEvidence = IncriminatingEvidence.GetAll();
        if (incriminatingEvidence != null && incriminatingEvidence.Count >= 2)
        {
            // If one is Mafia and one isn't -> vote the non-Mafia
            var nonMafiaTarget = incriminatingEvidence.FirstOrDefault(p => !p.Player.Is(Faction.Mafia));
            if (nonMafiaTarget != null)
            {
                lastMafiaVoteTarget = (TryCastVote(player, __instance, nonMafiaTarget.Player), true);
                return lastMafiaVoteTarget.Item1;
            }
        }

        if (lastMafiaVoteTarget.Item2 && ChanceIs(30)) return TryCastVote(player, __instance, lastMafiaVoteTarget.Item1);
        else if (seenKill != null && !seenKill.killer.Is(Faction.Mafia) && !seenKill.Player.HasDied()) lastMafiaVoteTarget = (TryCastVote(player, __instance, seenKill.killer.PlayerId), true);
        else if (validPlayers.Count > 0) lastMafiaVoteTarget = (RandomVote(player, __instance, validPlayers, allPlayers.Count >= 7), true);
        else lastMafiaVoteTarget = (SkipVote(player, __instance), true);

        return lastMafiaVoteTarget.Item1;
    }

    public static byte MadScientistVoting(PlayerControl player, MeetingHud __instance)
    {
        var validPlayers = PlayerControl.AllPlayerControls.ToArray().Where(x =>
            !x.HasDied() && x != player && !x.HasModifier<TI>() && !x.HasModifier<Confirmed>() && !x.HasModifier<SoftCleared>() &&
            !(x.Is(Faction.Village) && x.HasModifier<GlobalReveal>()
             && !x.HasModifier<SeenKill>() && !x.HasModifier<ConfirmedEvil>() && !x.HasModifier<IncriminatingEvidence>())).ToList();

        if (validPlayers.Count > 0) return RandomVote(player, __instance, validPlayers);
        return SkipVote(player, __instance);
    }

    public static byte FoolVoting(PlayerControl player, MeetingHud __instance)
    {
        return TryCastVote(player, __instance, player.PlayerId);
    }

    public static bool ChanceIsNull(int? num)
    {
        var r = Random.Range(0, 100);
        return r < num;
    }

    public static bool ChanceIsFloat(float num)
    {
        var r = Random.Range(0f, 100f);
        return r < num;
    }

    public static bool ChanceIs(int num)
    {
        var r = Random.Range(0, 100);
        return r < num;
    }
}