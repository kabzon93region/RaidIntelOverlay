using System.Collections.Generic;
using System.Text;
using Comfort.Common;
using EFT;

namespace RaidIntelOverlay
{
    internal sealed class MapAreaSnapshot
    {
        public string ActiveTriggerZones = string.Empty;
    }

    /// <summary>
    /// Только активные TriggerZones игрока — без FindObjectsOfType (без фризов).
    /// </summary>
    internal static class MapAreaResolver
    {
        private static string _cachedZones = string.Empty;
        private static int _cachedZoneCount = -1;
        private static string _cachedFirstZone = string.Empty;

        public static MapAreaSnapshot Collect()
        {
            return new MapAreaSnapshot
            {
                ActiveTriggerZones = ResolveActiveTriggerZonesCached()
            };
        }

        public static void InvalidateCache()
        {
            _cachedZoneCount = -1;
            _cachedFirstZone = string.Empty;
            _cachedZones = string.Empty;
        }

        private static string ResolveActiveTriggerZonesCached()
        {
            if (!Singleton<GameWorld>.Instantiated)
            {
                return string.Empty;
            }

            var player = Singleton<GameWorld>.Instance.MainPlayer;
            if (player == null)
            {
                return string.Empty;
            }

            try
            {
                var zones = player.TriggerZones;
                var count = zones?.Count ?? 0;
                var first = count > 0 ? zones[0] ?? string.Empty : string.Empty;

                if (count == _cachedZoneCount && first == _cachedFirstZone)
                {
                    return _cachedZones;
                }

                _cachedZoneCount = count;
                _cachedFirstZone = first;
                _cachedZones = FormatTriggerZones(zones);
                return _cachedZones;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string FormatTriggerZones(IReadOnlyList<string> zones)
        {
            if (zones == null || zones.Count == 0)
            {
                return string.Empty;
            }

            var names = new List<string>(zones.Count);
            for (int i = 0; i < zones.Count; i++)
            {
                var zoneId = zones[i];
                if (string.IsNullOrWhiteSpace(zoneId))
                {
                    continue;
                }

                var localized = zoneId.Localized(null);
                names.Add(string.IsNullOrWhiteSpace(localized) || localized == zoneId ? zoneId : localized);
            }

            if (names.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder(128);
            for (int i = 0; i < names.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }

                sb.Append(names[i]);
            }

            return sb.ToString();
        }
    }
}
