using AUPathfinder;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace AmongUsSalem.AI;

[RegisterInIl2Cpp]
public class Bot(IntPtr ptr) : MonoBehaviour(ptr)
{
    public PlayerControl player;
    public PlayerControl target;
    public BotNavigator nav;

    public bool UsedAbility;
    public bool isMoving;
    public bool endTurn;
    public int nodes;
    public bool isTurn;
    public SystemTypes startingRoom;
    public PlayerTask targetTask;
    public ActionType2 currentAction;
    private void Start()
    {
        Bots.Add(this);
    }

    private void Update()
    {
        if (isTurn)
        {
            var currentRoom = player.GetPlayerRoom();
            if (currentRoom != startingRoom && currentRoom != SystemTypes.Hallway && !player.IsRole<Itinerant>())
            {
                startingRoom = currentRoom;
                AUSPlugin.DebugLogMessage($"{player.Data.PlayerName} is stopping in {currentRoom}");

                EndTurn();
            }
        }
    }

    public void TurnStart()
    {
        InstanceControlPatches.SwitchTo(player.PlayerId);
        startingRoom = player.GetPlayerRoom();
        isTurn = true;
        endTurn = false;
        UsedAbility = false;

        if (player.Data.Role is ICustomAURole customRole) customRole.OnTurnStart();

        Move();
    }

    private bool TaskMove()
    {
        if (player.CanCompleteTasks())
        {
            var tasks = player.myTasks.ToArray().Where(x => x.TryCast<NormalPlayerTask>() != null && !x.IsComplete).ToList();
            if (tasks.Count > 0)
            {
                var randomTask = tasks.OrderBy(x => Vector2.Distance(x.transform.position, player.GetTruePosition())).FirstOrDefault();
                if (targetTask == null) targetTask = randomTask;

                var pos = targetTask.FindConsoles().ToArray().Random().transform.position;
                nav.MoveTo(pos, ActionType2.Move);
                return true;
            }
        }

        return false;
    }

    private bool PriorityMove()
    {
        if (player.Data.Role is Noctivagant noctivagant && noctivagant.IsolatedPlayer != null)
        {
            nav.MoveTo(noctivagant.IsolatedPlayer.transform.position, ActionType2.Move);
            return true;
        }
        if (player.Data.Role is Thanatologist thanatologist && thanatologist.TargetPlayer != null)
        {
            nav.MoveTo(thanatologist.TargetPlayer.transform.position, ActionType2.Move);
            return true;
        }

        return false;
    }

    private bool AdjacentRoomMove()
    {
        if (AdjacentRooms.TryGetValue(startingRoom, out var value))
        {
            var possibleRooms = new List<SystemTypes>(value);
            possibleRooms.Remove(startingRoom);

            var roomTarget = possibleRooms.Random();
            if (RoomLocations.TryGetValue((roomTarget, CheckMap.MapSelected), out var pos))
            {
                nav.MoveTo(pos, ActionType2.Move);
                return true;
            }
        }

        return false;
    }

    private bool RandomRoomMove()
    {
        var roomTarget = CustomExtentions.AvailableRooms.Random();
        if (RoomLocations.TryGetValue((roomTarget, CheckMap.MapSelected), out var pos))
        {
            nav.MoveTo(pos, ActionType2.Move);
            return true;
        }

        return false;
    }

    public void Move()
    {
        AUSPlugin.DebugLogMessage($"{player.Data.PlayerName} is in {startingRoom}");
        if (player.HasDied() && !player.Is(Faction.Crewmate) && player.GetTasksLeft() == 0)
        {
            EndTurn();
            return;
        }

        if (TaskMove())
            return;

        if (PriorityMove())
            return;

        if (AdjacentRoomMove())
            return;

        RandomRoomMove();
    }

    private bool TryKill(bool killAny = false)
    {
        var players = PlayerControl.AllPlayerControls.ToArray().ToList();
        players.Shuffle();
        foreach (var t in players)
        {
            if (t.HasDied() || t == player) continue;
            if (player.Is(Faction.Impostor) && t.Is(Faction.Impostor)) continue;
            if (!WitnessKill.CanSee(player, t.gameObject)) continue;

            if (!WitnessKill.WouldBeWitnessed(player, t) || killAny)
            {
                if (player.HasModifier<InvisibleStatus>() && player.IsRole<Tenebrist>() && t.PlayersInRoom().Count(x => x != player) > 3)
                    return false;

                AUSPlugin.DebugLogMessage("Attempting Ability!");

                target = t;
                nav.MoveTo(target.GetTruePosition(), ActionType2.Ability);

                UsedAbility = true;
                return true;
            }
        }

        return false;
    }

    public bool Actions()
    {
        if (player.HasDied()) return false;

        // Role Functions
        if (currentAction != ActionType2.Ability && !UsedAbility)
        {
            if (player.Data.Role is Tenebrist tenebrist)
            {
                if (WitnessKill.IsIsolated(player) && !player.HasModifier<InvisibleStatus>()) tenebrist.DoVisit(player, 2, false, false);
                return TryKill(player.HasModifier<InvisibleStatus>());
            }
            else if (player.IsStandardKiller())
            {
                return TryKill();
            }
        }

        // Report
        if (!player.IsStandardKiller() && currentAction != ActionType2.Report)
        {
            var deadBodies = UnityEngine.Object.FindObjectsOfType<DeadBody>().ToList();
            foreach (var db in deadBodies)
            {
                var t = MiscUtils.PlayerById(db.ParentId);
                if (WitnessKill.CanSee(player, db.gameObject))
                {
                    AUSPlugin.DebugLogMessage("Attempting Report!");
                    player.CmdReportDeadBody(t.Data);
                    return true;
                }
            }
        }

        return false;
    }

    public void PerformAction(bool roleAction)
    {
        bool killed = false;
        if (roleAction && !player.HasDied())
        {
            if (player.Data.Role is ICustomAURole customRole)
            {
                if (player.Data.Role is Impostor or Tenebrist or Palingenist or Noctivagant)
                {
                    killed = true;
                    customRole.DoVisit(target, 1, true, true);
                    target = null;
                }
            }
        }

        EndTurn(killed);
    }

    public void EndTurn(bool killed = false)
    {
        if (nodes == 1 && targetTask != null)
        {
            HudManager.Instance.ShowTaskComplete();
            player.RpcCompleteTask(targetTask.Id);
            targetTask = null;
        }

        /*if (player.Data.Role is Palingenist palingenist && !palingenist.UsedSecondMove)
        {
            palingenist.UsedSecondMove = true;
            Move();
            return;
        }*/

        if (player.Data.Role is ICustomAURole customRole) customRole.OnTurnEnd();

        endTurn = true;
        isTurn = false;
        currentAction = ActionType2.None;
        Keyboard_Joystick.Switch(true);
    }

    // Static Methods
    public static List<Bot> Bots = new List<Bot>();
    public static void Create(PlayerControl player)
    {
        var botObj = new GameObject($"{player.Name()} Bot");
        botObj.transform.position = player.transform.position;

        var bot = botObj.AddComponent<Bot>();
        var botNav = botObj.AddComponent<BotNavigator>();

        botNav.player = player;
        botNav.bot = bot;

        bot.player = player;
        bot.nav = botNav;

        botObj.transform.SetParent(player.gameObject.transform);
    }

    public static Dictionary<SystemTypes, List<SystemTypes>> AdjacentRooms = new Dictionary<SystemTypes, List<SystemTypes>>()
    {
        { SystemTypes.Cafeteria, new List<SystemTypes>() { SystemTypes.Cafeteria, SystemTypes.UpperEngine, SystemTypes.Weapons, SystemTypes.Admin, SystemTypes.Storage, SystemTypes.MedBay } },
        { SystemTypes.Weapons, new List<SystemTypes>() { SystemTypes.Weapons, SystemTypes.Cafeteria, SystemTypes.LifeSupp, SystemTypes.Nav, SystemTypes.Shields } },
        { SystemTypes.LifeSupp, new List<SystemTypes>() { SystemTypes.LifeSupp, SystemTypes.Weapons, SystemTypes.Nav, SystemTypes.Shields } },
        { SystemTypes.Nav, new List<SystemTypes>() { SystemTypes.Nav, SystemTypes.LifeSupp, SystemTypes.Weapons, SystemTypes.Shields } },
        { SystemTypes.Shields, new List<SystemTypes>() { SystemTypes.Shields, SystemTypes.Storage, SystemTypes.Comms, SystemTypes.Nav, SystemTypes.LifeSupp, SystemTypes.Weapons } },
        { SystemTypes.Comms, new List<SystemTypes>() { SystemTypes.Comms, SystemTypes.Storage, SystemTypes.Shields } },
        { SystemTypes.Storage, new List<SystemTypes>() { SystemTypes.Storage, SystemTypes.Admin, SystemTypes.Cafeteria, SystemTypes.LowerEngine, SystemTypes.Electrical, SystemTypes.Comms, SystemTypes.Shields } },
        { SystemTypes.Electrical, new List<SystemTypes>() { SystemTypes.Electrical, SystemTypes.LowerEngine, SystemTypes.Storage } },
        { SystemTypes.LowerEngine, new List<SystemTypes>() { SystemTypes.LowerEngine, SystemTypes.Electrical, SystemTypes.Storage, SystemTypes.Security, SystemTypes.Reactor, SystemTypes.UpperEngine } },
        { SystemTypes.Reactor, new List<SystemTypes>() { SystemTypes.Reactor, SystemTypes.UpperEngine, SystemTypes.LowerEngine, SystemTypes.Security } },
        { SystemTypes.Security, new List<SystemTypes>() { SystemTypes.Security, SystemTypes.Reactor, SystemTypes.UpperEngine, SystemTypes.LowerEngine } },
        { SystemTypes.UpperEngine, new List<SystemTypes>() { SystemTypes.UpperEngine, SystemTypes.LowerEngine, SystemTypes.Reactor, SystemTypes.Cafeteria, SystemTypes.Security, SystemTypes.MedBay } },
        { SystemTypes.MedBay, new List<SystemTypes>() { SystemTypes.MedBay, SystemTypes.UpperEngine, SystemTypes.Cafeteria } },
        { SystemTypes.Admin, new List<SystemTypes>() { SystemTypes.Admin, SystemTypes.Cafeteria, SystemTypes.Storage} },
    };

    public static Dictionary<(SystemTypes, CurrentMap), Vector2> RoomLocations = new Dictionary<(SystemTypes, CurrentMap), Vector2>()
    {
        { (SystemTypes.Cafeteria, CurrentMap.Skeld), new Vector2(-00.96f, -02.93f) },
        { (SystemTypes.Weapons, CurrentMap.Skeld), new Vector2(09.34f, 00.68f) },
        { (SystemTypes.LifeSupp, CurrentMap.Skeld), new Vector2(06.60f, -03.51f) },
        { (SystemTypes.Nav, CurrentMap.Skeld), new Vector2(16.93f, -04.54f) },
        { (SystemTypes.Shields, CurrentMap.Skeld), new Vector2(09.38f, -12.30f) },
        { (SystemTypes.Comms, CurrentMap.Skeld), new Vector2(04.00f, -15.39f) },
        { (SystemTypes.Storage, CurrentMap.Skeld), new Vector2(-01.47f, -15.87f) },
        { (SystemTypes.Electrical, CurrentMap.Skeld), new Vector2(-08.27f, -11.14f) },
        { (SystemTypes.LowerEngine, CurrentMap.Skeld), new Vector2(-15.76f, -13.17f) },
        { (SystemTypes.Reactor, CurrentMap.Skeld), new Vector2(-20.36f, -05.18f) },
        { (SystemTypes.Security, CurrentMap.Skeld), new Vector2(-13.59f, -04.53f) },
        { (SystemTypes.UpperEngine, CurrentMap.Skeld), new Vector2(-15.68f, 02.57f) },
        { (SystemTypes.MedBay, CurrentMap.Skeld), new Vector2(-08.70f, -04.08f) },
        { (SystemTypes.Admin, CurrentMap.Skeld), new Vector2(04.41f, -07.44f) },

        { (SystemTypes.Launchpad, CurrentMap.MiraHQ), new Vector2(-04.42f, 02.70f) },
        { (SystemTypes.MedBay, CurrentMap.MiraHQ), new Vector2(15.38f, 00.01f) },
        { (SystemTypes.Comms, CurrentMap.MiraHQ), new Vector2(15.34f, 04.10f) },
        { (SystemTypes.LockerRoom, CurrentMap.MiraHQ), new Vector2(08.74f, 01.53f) },
        { (SystemTypes.Decontamination, CurrentMap.MiraHQ), new Vector2(06.04f, 06.32f) },
        { (SystemTypes.Reactor, CurrentMap.MiraHQ), new Vector2(02.61f, 11.08f) },
        { (SystemTypes.Laboratory, CurrentMap.MiraHQ), new Vector2(09.35f, 12.60f) },
        { (SystemTypes.Office, CurrentMap.MiraHQ), new Vector2(14.96f, 19.27f) },
        { (SystemTypes.Admin, CurrentMap.MiraHQ), new Vector2(20.96f, 20.60f) },
        { (SystemTypes.Greenhouse, CurrentMap.MiraHQ), new Vector2(17.71f, 23.32f) },
        { (SystemTypes.Storage, CurrentMap.MiraHQ), new Vector2(19.53f, 04.64f) },
        { (SystemTypes.Balcony, CurrentMap.MiraHQ), new Vector2(22.36f, -01.75f) },
        { (SystemTypes.Cafeteria, CurrentMap.MiraHQ), new Vector2(25.65f, 04.78f) },

        { (SystemTypes.Dropship, CurrentMap.Polus), new Vector2(16.63f, -04.10f) },
        { (SystemTypes.Electrical, CurrentMap.Polus), new Vector2(05.41f, -09.91f) },
        { (SystemTypes.Security, CurrentMap.Polus), new Vector2(02.89f, -12.01f) },
        { (SystemTypes.LifeSupp, CurrentMap.Polus), new Vector2(03.00f, -20.19f) },
        { (SystemTypes.BoilerRoom, CurrentMap.Polus), new Vector2(02.18f, -23.83f) },
        { (SystemTypes.Comms, CurrentMap.Polus), new Vector2(12.31f, -16.38f) },
        { (SystemTypes.Weapons, CurrentMap.Polus), new Vector2(12.32f, -23.18f) },
        { (SystemTypes.Office, CurrentMap.Polus), new Vector2(20.95f, -19.16f) },
        { (SystemTypes.Admin, CurrentMap.Polus), new Vector2(21.28f, -22.89f) },
        { (SystemTypes.Storage, CurrentMap.Polus), new Vector2(20.17f, -11.63f) },
        { (SystemTypes.Laboratory, CurrentMap.Polus), new Vector2(34.92f, -06.50f) },
        { (SystemTypes.Specimens, CurrentMap.Polus), new Vector2(36.41f, -21.29f) },
    };

    public enum ActionType2
    {
        None,
        Ability,
        Report,
        Button,
        Vent,
        Move
    }
}