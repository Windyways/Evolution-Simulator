using Reactor.Networking.Rpc;
using UnityEngine;

namespace AmongUsSalem.Misc;

public static class CustomExtentions
{
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


    /// <summary>
    /// Networked Custom Murder method.
    /// </summary>
    /// <param name="source">The killer.</param>
    /// <param name="target">The player to murder.</param>
    [MethodRpc((uint)AUSRpc.GhostRoleMurder, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcGhostRoleMurder(
        this PlayerControl source,
        PlayerControl target)
    {
        if (LobbyBehaviour.Instance)
            return;

        if (!source.HasDied())
            return;

        var role = source.GetRoleWhenAlive();
        if (source.Data.Role is IGhostRole)
        {
            role = source.Data.Role;
        }

        var customRole = role as ICustomAURole;
        if (customRole == null)
            return;

        source.CustomMurder(
            target,
            MurderResultFlags.Succeeded);

        // Force-sync death state after ghost role murder to prevent desyncs
        if (target.HasDied())
        {
            DeathStateSync.ScheduleDeathStateSync(target, true);
            // Request validation after kill to ensure all clients are in sync
            if (source.AmOwner)
            {
                DeathStateSync.RequestValidationAfterKill(source);
            }
        }
    }

    public static bool IsNeutral(this ICustomAURole customRole)
    {
        return customRole.Faction == Faction.Independent;
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

    public static ShowRoleIcon GetIcon(this PlayerControl player)
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

    public static DeathReasonShow GetDeathReason(this PlayerControl player, DeathReasonShow newDR = DeathReasonShow.None)
    {
        if (newDR != DeathReasonShow.None) return newDR; // For roles that can apply multiple death reasons, like Toaster.

        if (player.Is(Faction.Mafia)) return DeathReasonShow.KilledByTheMafia;
        if (player.IsRole<Sniper>()) return DeathReasonShow.KilledByTheSniper;
        if (player.IsRole<Granny>()) return DeathReasonShow.KilledByTheGranny;
        return DeathReasonShow.None;
    }

    public static RoleBehaviour GetRoleFromRoleBehaviour(this RoleBehaviour role)
    {
        var ushortRole = RoleId.Get(role.GetType());
        return RoleManager.Instance.GetRole((RoleTypes)ushortRole);
    }

    public static bool IsSheriff(this PlayerControl player)
    {
        return player.IsRole<Cop>() || player.IsRole<Deputy>();
    }
}