using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using WrathCombo.CustomComboNS.Functions;
using static WrathCombo.CustomComboNS.Functions.CustomComboFunctions;
using Status = Dalamud.Game.ClientState.Statuses.Status; // conflicts with structs if not defined

namespace WrathCombo.Data
{
    internal partial class CustomComboCache : IDisposable
    {
        //Invalidate this
        private readonly ConcurrentDictionary<(uint StatusID, ulong? TargetID, ulong? SourceID), Status?> statusCache = new();

        /// <summary> Finds a status on the given object. </summary>
        /// <param name="statusID"> Status effect ID. </param>
        /// <param name="obj"> Object to look for effects on. </param>
        /// <param name="sourceID"> Source object ID. </param>
        /// <returns> Status object or null. </returns>
        internal Status? GetStatus(uint statusID, IGameObject? obj, ulong? sourceID)
        {
            if (obj is null) return null;
            var key = (statusID, obj?.GameObjectId, sourceID);
            if (statusCache.TryGetValue(key, out Status? found))
                return found;

            if (obj is null)
                return statusCache[key] = null;

            if (obj is not IBattleChara chara)
                return statusCache[key] = null;

            foreach (Status? status in chara.StatusList)
            {
                if (status.StatusId == statusID && (!sourceID.HasValue || status.SourceId == 0 || status.SourceId == InvalidObjectID || status.SourceId == sourceID))
                    return statusCache[key] = status;
            }

            return statusCache[key] = null;
        }
    }

    internal class StatusCache
    {
        /// <summary>
        /// Lumina Status Sheet Dictionary
        /// </summary>
        private static readonly Dictionary<uint, Lumina.Excel.Sheets.Status> StatusSheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Status>()!
            .ToDictionary(i => i.RowId, i => i);

        /// <summary>
        /// Enum defining status effects with their name IDs.
        /// </summary>
        public enum StatusEffect
        {
            Amnesia = 5,
            Pacification = 6,
            Silence = 7,
            Weakness = 43,
            BrinkOfDeath = 44,
            DamageDown = 62
        }

        /// <summary>
        /// Cached dictionary mapping status effects to their HashSet of status IDs.
        /// </summary>
        private static readonly Dictionary<StatusEffect, HashSet<uint>> StatusIdCaches = InitializeStatusIdCaches();

        /// <summary>
        /// Initializes the status ID caches in a single StatusSheet iteration.
        /// </summary>
        private static Dictionary<StatusEffect, HashSet<uint>> InitializeStatusIdCaches()
        {
            var statusEffects = Enum.GetValues<StatusEffect>();
            var nameToEffect = statusEffects
                .Select(e => (Effect: e, Name: GetStatusName((uint)e)))
                .Where(x => !string.IsNullOrEmpty(x.Name))
                .ToDictionary(x => x.Name, x => x.Effect, StringComparer.OrdinalIgnoreCase);

            var caches = statusEffects
                .SelectMany(e => StatusSheet
                    .Where(s => s.Value.Name.ToString().Equals(GetStatusName((uint)e), StringComparison.OrdinalIgnoreCase))
                    .Select(s => (Effect: e, Id: s.Key)))
                .GroupBy(x => x.Effect)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Id).ToHashSet());

            return caches;
        }

        public static bool HasAmnesia() => HasStatusInCacheList(StatusEffect.Amnesia);

        public static bool HasPacification() => HasStatusInCacheList(StatusEffect.Pacification);

        public static bool HasSilence() => HasStatusInCacheList(StatusEffect.Silence);

        public static bool HasWeakness(IGameObject? target) => HasStatusInCacheList(StatusEffect.Weakness, target);

        public static bool HasBrinkOfDeath(IGameObject? target) => HasStatusInCacheList(StatusEffect.BrinkOfDeath, target);

        public static bool HasDamageDown(IGameObject? target) => HasStatusInCacheList(StatusEffect.DamageDown, target);

        /// <summary>
        /// A cached set of dispellable status IDs for quick lookup.
        /// </summary>
        private static readonly HashSet<uint> DispellableStatuses = [..
            StatusSheet
                .Where(kvp => kvp.Value.CanDispel)
                .Select(kvp => kvp.Key)];
        public static bool HasCleansableDebuff(IGameObject? target) => HasStatusInCacheList(DispellableStatuses, target);

        /// <summary>
        /// A set of status effect IDs that grant general invincibility.
        /// </summary>
        /// <remarks>
        /// Includes statuses like Hallowed Ground (151), Living Dead (325), etc.
        /// </remarks>
        private static readonly HashSet<uint> InvincibleStatuses =
        [
            151, 198, 325, 328, 385, 394, 469, 529, 592, 656, 671, 775, 776, 895, 969, 981,
            1240, 1302, 1303, 1567, 1570, 1697, 1829, 1936, 2413, 2654, 3012, 3039, 3052, 3054,
            4410, 4175
        ];

        public static bool TargetIsInvincible(IGameObject? target)
        {
            if (target is not IBattleChara tar || tar.StatusList == null)
                return false;

            // Turn Target's status to uint hashset
            var targetStatuses = tar.StatusList.Select(s => s.StatusId).ToHashSet();
            uint targetID = tar.DataId;


            switch (Svc.ClientState.TerritoryType)
            {
                case 174:   // Labyrinth of the Ancients
                    // Thanatos, Spooky Ghosts Only
                    if (targetID is 2350) return !HasStatusEffect(398);

                    // Allagan Bomb
                    if (targetID is 2407) return NumberOfEnemiesInRange(30) > 1;

                    return false;
                case 1248:  // Jeuno 1 Ark Angels

                    // ArkAngel HM = 1804
                    if (targetID is 18049 && HasStatusEffect(4410, tar, true)) return true;

                    // ArkAngel MR = 18051 (A)
                    // ArkAngel GK = 18053 (B)
                    // ArkAngel TT = 18052 (C)
                    if (targetID is 18051 or 18052 or 18053)
                    {
                        if (HasStatusEffect(4192)) return targetID != 18051; // Alliance A Red Epic
                        if (HasStatusEffect(4194)) return targetID != 18053; // Alliance B Yellow Fated
                        if (HasStatusEffect(4196)) return targetID != 18052; // Alliance C Blue Vaunted
                    }
                    return false;
                case 917:   //Puppet's Bunker, Flight Mechs
                    // 724P Alpha = 11792 (A)
                    // 767P Beta  = 11793 (B)
                    // 772P Chi   = 11794 (C)
                    if (targetID is 11792 or 11793 or 11794)
                    {
                        if (HasStatusEffect(2288)) return targetID != 11792;
                        if (HasStatusEffect(2289)) return targetID != 11793;
                        if (HasStatusEffect(2290)) return targetID != 11794;
                    }
                    return false;
                default:
                    break;
            }

            // Omega
            //if (Svc.ClientState.TerritoryType is 801 or 805 or 1122)
            //{

            // Omega
            byte Omega = 1;
            if (Omega == 1)
            {
                if (
                    (tar.StatusList.Any(x => x.StatusId == 1674 || x.StatusId == 3454) && (HasStatusEffect(1660) || HasStatusEffect(3499)))
                    ||
                    (tar.StatusList.Any(x => x.StatusId == 1675) && (HasStatusEffect(1661) || HasStatusEffect(3500))))
                    return true;

            }

            // General invincibility check
            // Due to large size of InvincibleStatuses, best to check process this way
            if (targetStatuses.Any(id => InvincibleStatuses.Contains(id))) return true;

            return false;
        }

        public static string GetStatusName(uint id) => StatusSheet.TryGetValue(id, out var status) ? status.Name.ToString() : "Unknown Status";

        /// <summary>
        /// Returns an uint List of Status IDs based on Name.
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public static List<uint>? GetStatusesByName(string status)
        {
            if (string.IsNullOrEmpty(status))
                return null;
            var statusIds = StatusSheet
                .Where(x => x.Value.Name.ToString().Equals(status, StringComparison.CurrentCultureIgnoreCase))
                .Select(x => x.Key)
                .ToList();
            return statusIds.Count != 0 ? statusIds : null;
        }

        /// <summary>
        /// Compares a list of Status IDs against the Player or optional Target.
        /// </summary>
        /// <param name="list">HashSet uint. Use list from StatusCache</param>
        /// <param name="target">Optional IGameObject to check</param>
        /// 
        /// <returns></returns>
        internal static bool HasStatusInCacheList(HashSet<uint> list, IGameObject? target)
        {
            if (list.Count is not 0)
            {
                if (target is not IBattleChara chara || chara.StatusList == null)
                    return false;

                foreach (uint status in list)
                {
                    if (HasStatusEffect(status, chara, true)) return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Checks if a target has any status effect for the given status effect.
        /// </summary>
        internal static bool HasStatusInCacheList(StatusEffect effect, IGameObject? target = null)
        {
            return HasStatusInCacheList(StatusIdCaches[effect], target);
        }

        //internal static bool HasStatusInCacheList(HashSet<uint> list) => list.Count switch
        //{
        //    0 => false,
        //    _ => HasStatusInCacheList(list, Player.Object)
        //};

    }
}
