using TownOfUs.Events;
using UnityEngine;

namespace AmongUsSalem.Misc;

public static class CustomExtentions
{
    public static bool IsAlignedWith(this PlayerControl player, PlayerControl target)
    {
        if (player.Is(Faction.Impostor) && target.Is(Faction.Impostor)) return true;
        return false;
    }

    public static bool IsStandardKiller(this PlayerControl player)
    {
        return player.Is(Faction.Impostor) || player.Data.Role is Palingenist;
    }

    public static bool CanCompleteTasks(this PlayerControl player)
    {
        return player.Is(Faction.Crewmate) || player.Data.Role is Concordant;
    }

    public static string Name(this PlayerControl player)
    {
        return player.GetDefaultAppearance().PlayerName;
    }

    public static string ToSpacedString(this Enum value)
    {
        var name = value.ToString();

        // Insert space before capital letters that follow a lowercase
        name = System.Text.RegularExpressions.Regex.Replace(name, "([a-z])([A-Z])", "$1 $2");

        // Insert space when a capital is followed by another capital + lowercase (e.g., "AShroud")
        name = System.Text.RegularExpressions.Regex.Replace(name, "([A-Z])([A-Z][a-z])", "$1 $2");

        return name;
    }

    public static bool Is(this PlayerControl player, Faction faction)
    {
        if (player == null) return false;
        if (player.HasDied())
        {
            var deadRole = player.GetRoleWhenAlive();
            if (deadRole is ICustomAURole customRole && customRole.Faction == faction)
            {
                return true;
            }
        }

        if (player.Data.Role is ICustomAURole role && role.Faction == faction)
        {
            return true;
        }

        return false;
    }

    public static bool Is(this PlayerControl player, Alignment alignment)
    {
        if (player.Data.Role is ICustomAURole role && role.Alignment == alignment)
        {
            return true;
        }

        return false;
    }

    public static bool AmOwner(this PlayerControl player)
    {
        return player.AmOwner || (Debugger.IsDebuggerActive && Debugger.ShowAllMessages);
    }

    public static bool IsSameFaction(this PlayerControl player, PlayerControl target)
    {
        if (player.Data.Role is ICustomAURole ausRole && target.Data.Role is ICustomAURole targetAusRole && ausRole.Faction == targetAusRole.Faction) return true;
        //if (player.Is(Faction.Town) && target.HasModifier<VampireRecruit>()) return true;
        //if (player.HasModifier<VampireRecruit>() && target.Is(Faction.Town)) return true;
        return false;
    }

    public static bool IsTrueRole<T>(this PlayerControl player) where T : RoleBehaviour
    {
        if (player == null) return false;
        if (player.HasDied())
        {
            var r = player.GetRoleWhenAlive();
            return r is T;
        }
        return player.Data?.Role is T;
    }

    public static T? GetTrueRole<T>(this PlayerControl player) where T : RoleBehaviour
    {
        if (player == null) return null;
        if (player.HasDied())
        {
            var r = player.GetRoleWhenAlive() as T;
            return r;
        }

        var role = player.Data?.Role as T;
        return role;
    }

    public static ICustomAURole GetICustomAURoleWhenAlive(this PlayerControl player)
    {
        ICustomAURole customRole = null;
        //var role = RoleHistory.LastOrDefault(x => x.Key == player.PlayerId && !x.Value.IsDead);
        //return role.Value != null ? role.Value : null;

        if (GameHistory.RoleWhenAlive.TryGetValue(player.PlayerId, out var role))
        {
            if (role is ICustomAURole c3) customRole = c3;
        }

        if (!player.Data.IsDead)
        {
            if (player.Data.Role is ICustomAURole c4) customRole = c4;
        }

        var role2 = player.Data.RoleWhenAlive;
        if (role2.HasValue)
        {
            if (RoleManager.Instance.GetRole(role2.Value) is ICustomAURole c2) customRole = c2;
        }

        if (player.Data.Role is ICustomAURole c) customRole = c;
        return customRole;
    }

    public static bool IsNeutral(this ICustomAURole customRole)
    {
        return customRole.Faction == Faction.Neutral;
    }

    public static bool IsHidden(this SpriteRenderer sr)
    {
        return sr.color == new Color(1, 1, 1, 0);
    }

    public static void Hide(this SpriteRenderer sr)
    {
        sr.color = new Color(1, 1, 1, 0);
    }

    public static void Show(this SpriteRenderer sr)
    {
        sr.color = new Color(1, 1, 1, 1);
    }

    public static SpriteRenderer AddSpriteRenderer(this GameObject gameObject, Sprite sprite, int sortingOrder, int rendererPriority, Vector3 pos, Color color, Vector3 localScale)
    {
        SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = sortingOrder;
        sr.rendererPriority = rendererPriority;
        sr.transform.position = pos;
        sr.color = color;
        sr.gameObject.transform.localScale = localScale;
        return sr;
    }

    public static ShowRoleIcon? GetIcon(this PlayerControl player)
    {
        foreach (var icon in ShowRoleIcon.AllRoleIcons)
        {
            if (icon.Owner == player) return icon;
        }
        return null;
    }

    public static Alignment GetAlignment(this PlayerControl player)
    {
        if (player == null) return Alignment.None;
        if (player.Data.Role is ICustomAURole customRole) return customRole.Alignment;
        return Alignment.None;
    }

    public static RoleBehaviour GetRoleFromRoleBehaviour(this RoleBehaviour role)
    {
        var ushortRole = RoleId.Get(role.GetType());
        return RoleManager.Instance.GetRole((RoleTypes)ushortRole);
    }

    public static SystemTypes GetPlayerRoom(this PlayerControl player)
    {
        var gameObject = player.gameObject;
        foreach (var room in ShipStatus.Instance.FastRooms)
        {
            bool flag = room.Value.roomArea.OverlapPoint(gameObject.transform.position);
            if (flag)
            {
                return room.Key;
            }
        }
        return SystemTypes.Hallway;
    }


    public static List<SystemTypes> AvailableRooms = new List<SystemTypes>();
    public static void GetAvailableRooms(this List<SystemTypes> list, bool isGodzilla = false)
    {
        if (CheckMap.MapSelected == CurrentMap.Skeld)
        {
            list.Add(SystemTypes.Cafeteria);
            list.Add(SystemTypes.Electrical);
            list.Add(SystemTypes.LowerEngine);
            list.Add(SystemTypes.Nav);
            list.Add(SystemTypes.Reactor);
            list.Add(SystemTypes.Shields);
            list.Add(SystemTypes.Storage);
            list.Add(SystemTypes.UpperEngine);
            list.Add(SystemTypes.Weapons);
            list.Add(SystemTypes.Admin);
            list.Add(SystemTypes.Comms);
            list.Add(SystemTypes.Security);
            list.Add(SystemTypes.MedBay);
            list.Add(SystemTypes.LifeSupp);
            if (DeathEventHandlers.CurrentRound == 1 && isGodzilla) list.Remove(SystemTypes.Cafeteria);
        }
        else if (CheckMap.MapSelected == CurrentMap.MiraHQ)
        {
            list.Add(SystemTypes.Launchpad);
            list.Add(SystemTypes.Reactor);
            list.Add(SystemTypes.Laboratory);
            list.Add(SystemTypes.LockerRoom);
            list.Add(SystemTypes.Comms);
            list.Add(SystemTypes.MedBay);
            list.Add(SystemTypes.Decontamination);
            list.Add(SystemTypes.Office);
            list.Add(SystemTypes.Greenhouse);
            list.Add(SystemTypes.Admin);
            list.Add(SystemTypes.Cafeteria);
            list.Add(SystemTypes.Storage);
            list.Add(SystemTypes.Balcony);
            if (DeathEventHandlers.CurrentRound == 1 && isGodzilla) list.Remove(SystemTypes.Launchpad);
            if (DeathEventHandlers.CurrentRound == 2 && isGodzilla) list.Remove(SystemTypes.Cafeteria);
        }
        else if (CheckMap.MapSelected == CurrentMap.Polus)
        {
            list.Add(SystemTypes.Dropship);
            list.Add(SystemTypes.Electrical);
            list.Add(SystemTypes.Security);
            list.Add(SystemTypes.LifeSupp);
            list.Add(SystemTypes.BoilerRoom);
            list.Add(SystemTypes.Weapons);
            list.Add(SystemTypes.Comms);
            list.Add(SystemTypes.Office);
            list.Add(SystemTypes.Admin);
            list.Add(SystemTypes.Laboratory);
            list.Add(SystemTypes.Specimens);
            list.Add(SystemTypes.Storage);
            if (DeathEventHandlers.CurrentRound == 1 && isGodzilla) list.Remove(SystemTypes.Dropship);
            if (DeathEventHandlers.CurrentRound == 2 && isGodzilla) list.Remove(SystemTypes.Office);
        }
        else if (CheckMap.MapSelected == CurrentMap.Airship)
        {
            list.Add(SystemTypes.Records);
            list.Add(SystemTypes.GapRoom);
            list.Add(SystemTypes.MeetingRoom);
            list.Add(SystemTypes.Brig);
            list.Add(SystemTypes.VaultRoom);
            list.Add(SystemTypes.Engine);
            list.Add(SystemTypes.Comms);
            list.Add(SystemTypes.Cockpit);
            list.Add(SystemTypes.Armory);
            list.Add(SystemTypes.Kitchen);
            list.Add(SystemTypes.ViewingDeck);
            list.Add(SystemTypes.Security);
            list.Add(SystemTypes.Electrical);
            list.Add(SystemTypes.MedBay);
            list.Add(SystemTypes.CargoBay);
            list.Add(SystemTypes.Lounge);
            list.Add(SystemTypes.Showers);
            list.Add(SystemTypes.MainHall);
            if (DeathEventHandlers.CurrentRound == 1 && isGodzilla)
            {
                list.Remove(SystemTypes.Records);
                list.Remove(SystemTypes.Brig);
                list.Remove(SystemTypes.Engine);
                list.Remove(SystemTypes.Kitchen);
                list.Remove(SystemTypes.CargoBay);
                list.Remove(SystemTypes.MainHall);
            }
        }
        else if (CheckMap.MapSelected == CurrentMap.Fungle)
        {
            list.Add(SystemTypes.Cafeteria);
            list.Add(SystemTypes.Kitchen);
            list.Add(SystemTypes.Storage);
            list.Add(SystemTypes.MeetingRoom);
            list.Add(SystemTypes.Laboratory);
            list.Add(SystemTypes.Greenhouse);
            list.Add(SystemTypes.Reactor);
            list.Add(SystemTypes.UpperEngine);
            list.Add(SystemTypes.Comms);
            list.Add(SystemTypes.MiningPit);
            list.Add(SystemTypes.Lookout);
            list.Add(SystemTypes.Dropship);
            if (DeathEventHandlers.CurrentRound == 2 && isGodzilla) list.Remove(SystemTypes.MeetingRoom);
        }
    }

    public static SystemTypes GetPlayerRoom(this GameObject gameObject)
    {
        foreach (var room in ShipStatus.Instance.FastRooms)
        {
            bool flag = room.Value.roomArea.OverlapPoint(gameObject.transform.position);
            if (flag)
            {
                return room.Key;
            }
        }
        return SystemTypes.Comms;
    }
}