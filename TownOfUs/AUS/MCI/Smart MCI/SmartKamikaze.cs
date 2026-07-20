using System.Collections;
using UnityEngine;

namespace AmongUsSalem.MCI.SmartMCI;

public static class SmartKamikaze
{
    public static void Start(Kamikaze kamikaze)
    {
        if (Debugger.IsDebuggerActive && Debugger.SmartBotsEnabled) Coroutines.Start(DelayStart(kamikaze));
    }

    public static IEnumerator DelayStart(Kamikaze kamikaze)
    {
        yield return new WaitForSeconds(DayNightMechanic.PostMeetingIntroTime + 1.5f);
        if (DayNightMechanic.DayCount == 1)
            yield break;

        if (kamikaze.Player.HasDied() || DayNightMechanic.DayCount == 1 || kamikaze.Player.HasModifier<ArrestedModifier>())
            yield break;

        if (kamikaze.Player.HasModifier<SeenKill>() || kamikaze.Player.HasModifier<ConfirmedEvil>() || kamikaze.Player.HasModifier<IncriminatingEvidence>())
        {
            // Player, Priority.
            List<(PlayerControl, int)> knownPriorityRoles = new List<(PlayerControl, int)>();

            // --- INTERROGATOR ---
            foreach (var interrogator in MiscUtils.GetRoles<Interrogator>())
            {
                List<RoleBehaviour> revealedRoles = new List<RoleBehaviour>();
                foreach (var revealed in ModifierUtils.GetActiveModifiers<RoleLearn>(x => x.Visitor == interrogator.Player))
                    revealedRoles.Add(revealed.Player.Data.Role);

                for (var i = 0; i < revealedRoles.Count; i++)
                {
                    var role = revealedRoles[i];
                    var value = IsPriorityRole(role);
                    if (value.Item1 != null) knownPriorityRoles.Add((value.Item1, value.Item2)); 
                }
            }

            List<RoleBehaviour> revealedRoles2 = new List<RoleBehaviour>();
            foreach (var revealed in ModifierUtils.GetActiveModifiers<GlobalReveal>())
                revealedRoles2.Add(revealed.Player.Data.Role);

            foreach (var revealed in ModifierUtils.GetActiveModifiers<Confirmed>())
                revealedRoles2.Add(revealed.Player.Data.Role);

            for (var i = 0; i < revealedRoles2.Count; i++)
            {
                var role = revealedRoles2[i];
                var value = IsPriorityRole(role);
                if (value.Item1 != null) knownPriorityRoles.Add((value.Item1, value.Item2));
            }

            if (knownPriorityRoles.Count > 0)
            {
                var target = knownPriorityRoles.OrderBy(x => x.Item2).FirstOrDefault().Item1;
                Perform(kamikaze, target);
            }
            else
            {
                var allPlayers = PlayerControl.AllPlayerControls.ToArray().Where(x => !x.HasDied()).ToList();
                var target = allPlayers.Random();
                Perform(kamikaze, target);
            }
        }
    }


    public static void Perform(Kamikaze kamikaze, PlayerControl target)
    {
        // Kill and suicide.
        if (target.Data.Role is Bulletproof bulletproof && bulletproof.hasVest) bulletproof.PerformInteraction(kamikaze.Player, target);
        else
        {
            kamikaze.Player.RpcCustomMurder(target);
            VisitingMechanic.RpcAddDeathReason(target, (int)kamikaze.Player.GetDeathReason(DeathReasonShow.BlownUpByTheKamikaze));
        }

        kamikaze.Player.RpcCustomMurder(kamikaze.Player);
        VisitingMechanic.RpcAddDeathReason(kamikaze.Player, (int)kamikaze.Player.GetDeathReason(DeathReasonShow.BlownUpByTheKamikaze));
    }

    public static (PlayerControl, int) IsPriorityRole(this RoleBehaviour role)
    {
        if (role is Granny granny) return (granny.Player, 10);
        if (role is Deputy deputy) return  (deputy.Player, 180);
        if (role is Bulletproof bulletproof) return  (bulletproof.Player, 200);
        if (role is Sniper sniper && sniper.hasBullet) return (sniper.Player, 20);
        if (role is Sniper sniper2) return (sniper2.Player, 190);
        if (role is Fool fool) return (fool.Player, 15);
        if (role is MadScientist madScientist) return (madScientist.Player, 100);
        if (role is Cop cop) return (cop.Player, 8);
        if (role is Doctor doctor) return (doctor.Player, 11);
        if (role is Gravedigger gravedigger && gravedigger.hasShovel) return (gravedigger.Player, 17);
        return (null, 0);
    }
}
