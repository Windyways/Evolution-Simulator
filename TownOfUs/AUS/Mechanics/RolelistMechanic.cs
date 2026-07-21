using AmongUs.GameOptions;
using TownOfUs.Options;

namespace AmongUsSalem.Mechanics;

public static class RolelistMechanic
{
    public static int ImpostorCount;
    public static void GenerateRoleListAndApplyRoles(List<NetworkedPlayerInfo> infected)
    {
        var impostors = MiscUtils.GetImpostors(infected);
        var crewmates = MiscUtils.GetCrewmates(impostors);

        var rolesAssigned = new List<ushort>();

        var buckets = GetBuckets();
        int guaranteedCovenCount = buckets.Count(x => x is RoleListOption.RandomImpostor or RoleListOption.ImpostorDeception or RoleListOption.ImpostorKilling or RoleListOption.ImpostorSupport or RoleListOption.ImpostorUtility);

        ImpostorCount += guaranteedCovenCount;
        foreach (var bucket in buckets.OrderBy(x => x is RoleListOption.Any))
        {
            if (bucket is RoleListOption.CrewmateInvestigative) AssignCrewmateole(rolesAssigned, Alignment.CrewmateInvestigative);
            if (bucket is RoleListOption.CrewmateKilling) AssignCrewmateole(rolesAssigned, Alignment.CrewmateKilling);
            if (bucket is RoleListOption.CrewmateProtective) AssignCrewmateole(rolesAssigned, Alignment.CrewmateProtective);
            if (bucket is RoleListOption.CrewmateSupport) AssignCrewmateole(rolesAssigned, Alignment.CrewmateSupport);
            if (bucket is RoleListOption.CrewmateUtility) AssignCrewmateole(rolesAssigned, Alignment.CrewmateUtility);
            if (bucket is RoleListOption.RandomCrewmate) AssignCrewmateole(rolesAssigned, Alignment.None, bucket);

            if (bucket is RoleListOption.NeutralEvil) AssignNeutralRole(rolesAssigned, Alignment.NeutralEvil);
            if (bucket is RoleListOption.RandomNeutral) AssignNeutralRole(rolesAssigned, Alignment.None, bucket);

            if (bucket is RoleListOption.ImpostorDeception) AssignImpostorRole(rolesAssigned, Alignment.ImpostorDeception);
            if (bucket is RoleListOption.ImpostorKilling) AssignImpostorRole(rolesAssigned, Alignment.ImpostorKilling);
            if (bucket is RoleListOption.ImpostorSupport) AssignImpostorRole(rolesAssigned, Alignment.ImpostorSupport);
            if (bucket is RoleListOption.ImpostorUtility) AssignImpostorRole(rolesAssigned, Alignment.ImpostorUtility);
            if (bucket is RoleListOption.RandomImpostor) AssignImpostorRole(rolesAssigned, Alignment.None, bucket);

            if (bucket is RoleListOption.Any) AssignAnyRole(rolesAssigned);
        }

        foreach (var role in rolesAssigned)
        {
            var num = HashRandom.FastNext(crewmates.Count);
            var player = crewmates[num];

            player.RpcSetRole((RoleTypes)role);

            crewmates.RemoveAt(num);

            if (AUSPlugin.IsDevBuild) Logger<AUSPlugin>.Warning($"Assigning {RoleManager.Instance.GetRole((RoleTypes)role).NiceName} to {player.Data.PlayerName}.");
        }

        // Assign vanilla roles to anyone who did not receive a role.
        foreach (var player in crewmates) player.RpcSetRole((RoleTypes)RoleId.Get<Crewmate>());
        foreach (var player in impostors) player.RpcSetRole((RoleTypes)RoleId.Get<Impostor>());
    }

    public static void AssignAnyRole(List<ushort> rolesAssigned)
    {
        int maxImpostor = (int)OptionGroupSingleton<ImpostorOptions>.Instance.MaxImpostor;
        var allRoles = MiscUtils.AllRoles.Where(x => x is ICustomAURole && x is not ISpawnChange).ToList();

        if (ImpostorCount >= maxImpostor) allRoles = MiscUtils.AllRoles.Where(x => x is ICustomAURole customRole && x is not ISpawnChange &&
            customRole.Faction != Faction.Impostor && !rolesAssigned.Contains(RoleId.Get(customRole.GetType()))).ToList();

        allRoles.AddRange(allRoles.AddDuplicateRolesToPool());
        var rolesAssignable = allRoles.ToList();

        bool gotRole = false;
        int attempts = 0;
        while (!gotRole && attempts < 50)
        {
            ImpostorCount = rolesAssigned.Count(x => RoleManager.Instance.GetRole((RoleTypes)x) is ICustomAURole i && i.Faction == Faction.Impostor);
            if (rolesAssignable.Count == 0)
            {
                attempts++;
                rolesAssignable = allRoles.Where(x => !rolesAssigned.Contains(RoleId.Get(x.GetType()))).ToList();
                if (attempts < 5) AUSPlugin.DebugLogMessage($"No more roles to assign, refreshing!", AUSPlugin.MsgType.Warning);
            }

            var role = rolesAssignable.Random();
            var customRole = role as ICustomAURole;
            if (customRole != null && role != null)
            {
                var chance = customRole.GetChance();
                var count = customRole.GetCount();
                if (chance != null && count != null)
                {
                    if (CalculatedVoting.ChanceIsNull(chance))
                    {
                        var roleUSHORT = RoleId.Get(customRole.GetType());
                        
                        if (rolesAssigned.Count(x => x.UshortToRole().NiceName == role.NiceName) >= count) AUSPlugin.DebugLogMessage($"Failed to assign {customRole.RoleName} because it has reached its max count!", AUSPlugin.MsgType.Warning);
                        else if (ImpostorCount > maxImpostor && customRole.Faction == Faction.Impostor) AUSPlugin.DebugLogMessage($"Failed to assign {customRole.RoleName} because max Impostor members reached!");
                        else
                        {
                            if (customRole.Faction == Faction.Impostor) ImpostorCount++;

                            gotRole = true;
                            rolesAssigned.Add(roleUSHORT);
                            AUSPlugin.DebugLogMessage($"Assigned {customRole.RoleName}!");
                        }
                    }
                    else AUSPlugin.DebugLogMessage($"Failed to assign {customRole.RoleName} because it failed to roll chance!");
                }
            }

            rolesAssignable.Remove(role);
        }
    }

    public static void AssignNeutralRole(List<ushort> rolesAssigned, Alignment alignment, RoleListOption bucket = RoleListOption.None)
    {
        var allRoles = MiscUtils.AllRoles.Where(x => x is ICustomAURole customRole && x is not ISpawnChange &&
            customRole.Alignment == alignment).ToList();

        if (bucket == RoleListOption.RandomNeutral) allRoles = MiscUtils.AllRoles.Where(x => x is ICustomAURole customRole && x is not ISpawnChange && customRole.IsNeutral()).ToList();
        
        allRoles.AddRange(allRoles.AddDuplicateRolesToPool());
        var rolesAssignable = allRoles.ToList();

        bool gotRole = false;
        int attempts = 0;
        while (!gotRole && attempts < 50)
        {
            if (rolesAssignable.Count == 0)
            {
                attempts++;
                rolesAssignable = allRoles.Where(x => !rolesAssigned.Contains(RoleId.Get(x.GetType()))).ToList();
                if (attempts < 5) AUSPlugin.DebugLogMessage($"No more roles to assign, refreshing!", AUSPlugin.MsgType.Warning);
            }

            var role = rolesAssignable.Random();
            var customRole = role as ICustomAURole;
            if (customRole != null && role != null)
            {
                var chance = customRole.GetChance();
                var count = customRole.GetCount();
                if (chance != null && count != null)
                {
                    if (CalculatedVoting.ChanceIsNull(chance))
                    {
                        var roleUSHORT = RoleId.Get(customRole.GetType());
                        if (rolesAssigned.Count(x => x.UshortToRole().NiceName == role.NiceName) >= count) AUSPlugin.DebugLogMessage($"Failed to assign {customRole.RoleName} because it has reached its max count!", AUSPlugin.MsgType.Warning);
                        else
                        {
                            gotRole = true;
                            rolesAssigned.Add(roleUSHORT);
                            AUSPlugin.DebugLogMessage($"Assigned {customRole.RoleName}!");
                        }
                    }
                    else AUSPlugin.DebugLogMessage($"Failed to assign {customRole.RoleName} because it failed to roll chance!");
                }
            }

            rolesAssignable.Remove(role);
        }
    }

    public static void AssignCrewmateole(List<ushort> rolesAssigned, Alignment alignment, RoleListOption bucket = RoleListOption.None)
    {
        var allRoles = MiscUtils.AllRoles.Where(x => x is ICustomAURole customRole && x is not ISpawnChange &&
            customRole.Alignment == alignment).ToList();

        if (bucket == RoleListOption.RandomCrewmate) allRoles = MiscUtils.AllRoles.Where(x => x is ICustomAURole customRole && x is not ISpawnChange && customRole.Faction == Faction.Crewmate).ToList();
        
        allRoles.AddRange(allRoles.AddDuplicateRolesToPool());
        var rolesAssignable = allRoles.ToList();

        bool gotRole = false;
        int attempts = 0;
        while (!gotRole && attempts < 50)
        {
            if (rolesAssignable.Count == 0)
            {
                attempts++;
                rolesAssignable = allRoles.Where(x => !rolesAssigned.Contains(RoleId.Get(x.GetType()))).ToList();
                if (attempts < 5) AUSPlugin.DebugLogMessage($"No more roles to assign, refreshing!", AUSPlugin.MsgType.Warning);
            }

            var role = rolesAssignable.Random();
            var customRole = role as ICustomAURole;
            if (customRole != null && role != null)
            {
                var chance = customRole.GetChance();
                var count = customRole.GetCount();
                if (chance != null && count != null)
                {
                    if (CalculatedVoting.ChanceIsNull(chance))
                    {
                        var roleUSHORT = RoleId.Get(customRole.GetType());
                        if (rolesAssigned.Count(x => x.UshortToRole().NiceName == role.NiceName) >= count) AUSPlugin.DebugLogMessage($"Failed to assign {customRole.RoleName} because it has reached its max count!", AUSPlugin.MsgType.Warning);
                        else
                        {
                            gotRole = true;
                            rolesAssigned.Add(roleUSHORT);
                            AUSPlugin.DebugLogMessage($"Assigned {customRole.RoleName}!");
                        }
                    }
                    else AUSPlugin.DebugLogMessage($"Failed to assign {customRole.RoleName} because it failed to roll chance!");
                }
            }

            rolesAssignable.Remove(role);
        }
    }

    public static void AssignImpostorRole(List<ushort> rolesAssigned, Alignment alignment, RoleListOption bucket = RoleListOption.None)
    {
        var allRoles = MiscUtils.AllRoles.Where(x => x is ICustomAURole customRole && x is not ISpawnChange &&
            customRole.Alignment == alignment).ToList();

        if (bucket == RoleListOption.RandomImpostor) allRoles = MiscUtils.AllRoles.Where(x => x is ICustomAURole customRole && x is not ISpawnChange && customRole.Faction == Faction.Impostor).ToList();
        
        allRoles.AddRange(allRoles.AddDuplicateRolesToPool());
        var rolesAssignable = allRoles.ToList();

        bool gotRole = false;
        int attempts = 0;
        while (!gotRole && attempts < 50)
        {
            if (rolesAssignable.Count == 0)
            {
                attempts++;
                rolesAssignable = allRoles.Where(x => !rolesAssigned.Contains(RoleId.Get(x.GetType()))).ToList();
                if (attempts < 5) AUSPlugin.DebugLogMessage($"No more roles to assign, refreshing!", AUSPlugin.MsgType.Warning);
            }

            var role = rolesAssignable.Random();
            var customRole = role as ICustomAURole;
            if (customRole != null && role != null)
            {
                var chance = customRole.GetChance();
                var count = customRole.GetCount();
                if (chance != null && count != null)
                {
                    if (CalculatedVoting.ChanceIsNull(chance))
                    {
                        var roleUSHORT = RoleId.Get(customRole.GetType());
                        if (rolesAssigned.Count(x => x.UshortToRole().NiceName == role.NiceName) >= count) AUSPlugin.DebugLogMessage($"Failed to assign {customRole.RoleName} because it has reached its max count!", AUSPlugin.MsgType.Warning);
                        else
                        {
                            gotRole = true;
                            rolesAssigned.Add(roleUSHORT);
                            AUSPlugin.DebugLogMessage($"Assigned {customRole.RoleName}!");
                        }
                    }
                    else AUSPlugin.DebugLogMessage($"Failed to assign {customRole.RoleName} because it failed to roll chance!");
                }
            }

            rolesAssignable.Remove(role);
        }
    }

    public static List<RoleBehaviour> AddDuplicateRolesToPool(this List<RoleBehaviour> list)
    {
        var allRoles = new List<RoleBehaviour>();
        if (!OptionGroupSingleton<RoleGenOptions>.Instance.LegacyRoleGen) return allRoles;

        foreach (var role in list.ToList())
        {
            if (role is ICustomAURole customRole)
            {
                var count = customRole.GetCount();
                if (count != null)
                {
                    for (int i = 1; i < count; i++)
                    {
                        allRoles.Add(role);
                    }
                }
            }
        }

        return allRoles;
    }

    public static List<RoleListOption> GetBuckets()
    {
        var playerCount = GameData.Instance.AllPlayers.Count;
        var opts = OptionGroupSingleton<RoleOptions>.Instance;
        List<RoleListOption> buckets =
        [
            (RoleListOption)opts.Slot1.Value, (RoleListOption)opts.Slot2.Value, (RoleListOption)opts.Slot3.Value,
            (RoleListOption)opts.Slot4.Value
        ];

        var anySlots = 0;

        if (playerCount > 4) buckets.Add((RoleListOption)opts.Slot5.Value);
        if (playerCount > 5) buckets.Add((RoleListOption)opts.Slot6.Value);
        if (playerCount > 6) buckets.Add((RoleListOption)opts.Slot7.Value);
        if (playerCount > 7) buckets.Add((RoleListOption)opts.Slot8.Value);
        if (playerCount > 8) buckets.Add((RoleListOption)opts.Slot9.Value);
        if (playerCount > 9) buckets.Add((RoleListOption)opts.Slot10.Value);
        if (playerCount > 10) buckets.Add((RoleListOption)opts.Slot11.Value);
        if (playerCount > 11) buckets.Add((RoleListOption)opts.Slot12.Value);
        if (playerCount > 12) buckets.Add((RoleListOption)opts.Slot13.Value);
        if (playerCount > 13) buckets.Add((RoleListOption)opts.Slot14.Value);
        if (playerCount > 14) buckets.Add((RoleListOption)opts.Slot15.Value);
        if (playerCount > 15)
        {
            for (var i = 0; i < playerCount - 15; i++) buckets.Add(RoleListOption.Any);
        }

        return buckets;
    }

    public static RoleBehaviour UshortToRole(this ushort x)
    {
        return RoleManager.Instance.GetRole((RoleTypes)x);
    }
}