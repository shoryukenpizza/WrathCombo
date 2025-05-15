using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using WrathCombo.CustomComboNS.Functions;
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
        /// Initializes all status ID caches in a single StatusSheet iteration.
        /// </summary>
        /// <returns>A dictionary mapping status names to their ID sets.</returns>
        private static Dictionary<string, HashSet<uint>> InitializeStatusIdCaches()
        {
            var statusNames = new Dictionary<uint, string>
            {
                { 5, GetStatusName(5) },  // Amnesia
                { 6, GetStatusName(6) },  // Pacification
                { 7, GetStatusName(7) },  // Silence
                { 43, GetStatusName(43) }, // Weakness
                { 44, GetStatusName(44) }, // Brink of Death
                { 62, GetStatusName(62) }  // Damage Down
            };

            var caches = statusNames.Values.ToDictionary(name => name, _ => new HashSet<uint>(), StringComparer.OrdinalIgnoreCase);

            foreach (var status in StatusSheet)
            {
                var name = status.Value.Name.ToString();
                if (caches.TryGetValue(name, out HashSet<uint>? value))
                    value.Add(status.Key);
            }

            foreach (var name in statusNames.Values)
            {
                if (caches[name].Count == 0)
                    Svc.Log.Warning($"No status IDs found for status name: {name}");
            }

            return caches;
        }

        /// <summary>
        /// A cached set of status IDs for the "Amnesia" status effect (name ID 5).
        /// </summary>
        /// <remarks>
        /// Populated at startup. Status IDs are stable across languages.
        /// </remarks>
        private static readonly HashSet<uint> AmnesiaStatusIds = InitializeStatusIdCaches()[GetStatusName(5)];
        public static bool HasAmnesia() => HasStatusInCacheList(AmnesiaStatusIds);

        /// <summary>
        /// A cached set of status IDs for the "Pacification" status effect (name ID 6).
        /// </summary>
        /// <remarks>
        /// Populated at startup. Status IDs are stable across languages.
        /// </remarks>
        private static readonly HashSet<uint> PacificationStatusIds = InitializeStatusIdCaches()[GetStatusName(6)];
        public static bool HasPacification() => HasStatusInCacheList(PacificationStatusIds);

        /// <summary>
        /// A cached set of status IDs for the "Silence" status effect (name ID 7).
        /// </summary>
        /// <remarks>
        /// Populated at startup. Status IDs are stable across languages.
        /// </remarks>
        private static readonly HashSet<uint> SilenceStatusIds = InitializeStatusIdCaches()[GetStatusName(7)];
        public static bool HasSilence() => HasStatusInCacheList(SilenceStatusIds);

        /// <summary>
        /// A cached set of status IDs for the "Weakness" status effect (name ID 43).
        /// </summary>
        /// <remarks>
        /// Populated at startup. Status IDs are stable across languages.
        /// </remarks>
        private static readonly HashSet<uint> WeaknessStatusIds = InitializeStatusIdCaches()[GetStatusName(43)];
        public static bool HasWeakness(IGameObject? target) => HasStatusInCacheList(WeaknessStatusIds, target);

        /// <summary>
        /// A cached set of status IDs for the "Brink of Death" status effect (name ID 44).
        /// </summary>
        /// <remarks>
        /// Populated at startup. Status IDs are stable across languages.
        /// </remarks>
        private static readonly HashSet<uint> BrinkOfDeathStatusIds = InitializeStatusIdCaches()[GetStatusName(44)];
        public static bool HasBrinkOfDeath(IGameObject? target) => HasStatusInCacheList(BrinkOfDeathStatusIds, target);

        /// <summary>
        /// A cached set of status IDs for the "Damage Down" status effect (name ID 62).
        /// </summary>
        /// <remarks>
        /// Populated at startup. Status IDs are stable across languages.
        /// </remarks>
        private static readonly HashSet<uint> DamageDownStatusIds = InitializeStatusIdCaches()[GetStatusName(62)];
        public static bool HasDamageDown(IGameObject? target) => HasStatusInCacheList(DamageDownStatusIds, target);

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
        /// Last updated: May 2025 for FFXIV patch 7.x.
        /// </remarks>
        private static readonly HashSet<uint> InvincibleStatuses =
        [
            151, 198, 325, 328, 385, 394, 469, 529, 592, 656, 671, 775, 776, 895, 969, 981,
            1240, 1302, 1303, 1567, 1570, 1697, 1829, 1936, 2413, 2654, 3012, 3039, 3052, 3054,
            4410, 4175
        ];
        private static readonly HashSet<uint> JeunoPlayerStatuses = [4192, 4194, 4196];
        private static readonly HashSet<uint> JeunoVulnerableStatuses = [4193, 4195, 4197];
        private static readonly HashSet<uint> YoRHaStatuses = [2409, 2410, 2411];
        private static readonly HashSet<uint> OmegaTargetStatuses = [1674, 3454, 1675];
        private static readonly HashSet<uint> OmegaPlayerStatuses = [1660, 3499, 1661, 3500];

        public static bool TargetIsInvincible(IGameObject? target)
        {
            if (target is not IBattleChara tar || tar.StatusList == null)
                return false;

            // General invincibility check
            if (HasStatusInCacheList(InvincibleStatuses, tar))
                return true;

            // Jeuno Ark Angel Encounter
            if (HasStatusInCacheList(JeunoPlayerStatuses) && !HasStatusInCacheList(JeunoVulnerableStatuses, tar))
                return true;

            // YoRHa raid encounter
            var alliance = CustomComboFunctions.GetAllianceGroup();
            if ((alliance != AllianceGroup.GroupA && HasStatusInCacheList(YoRHaStatuses, tar)) ||
                (alliance != AllianceGroup.GroupB && HasStatusInCacheList(YoRHaStatuses, tar)) ||
                (alliance != AllianceGroup.GroupC && HasStatusInCacheList(YoRHaStatuses, tar)))
                return true;

            // Omega
            if ((HasStatusInCacheList(OmegaTargetStatuses, tar) && HasStatusInCacheList(OmegaPlayerStatuses)) ||
                (HasStatusInCacheList(OmegaTargetStatuses, tar) && HasStatusInCacheList(OmegaPlayerStatuses)))
                return true;

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
                    if (CustomComboFunctions.HasStatusEffect(status, chara, true)) return true;
                }
            }
            return false;
        }

        internal static bool HasStatusInCacheList(HashSet<uint> list) => list.Count switch
        {
            0 => false,
            _ => HasStatusInCacheList(list, Player.Object)
        };

    }
}
