using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;

namespace RaidIntelOverlay
{
    internal sealed class TrackedItemsConfig
    {
        // Старые примеры из ранних сборок — не показываем даже если остались в cfg.
        private static readonly HashSet<string> LegacyTemplateIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "5447a9e4bdc2dbc1538b4597",
            "590c661e86f7741e566b646a"
        };

        private readonly ConfigEntry<string> _templateIdsEntry;
        private readonly ConfigEntry<string> _templateIdsFileEntry;
        private HashSet<string> _cachedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public TrackedItemsConfig(ConfigFile config)
        {
            _templateIdsEntry = config.Bind(
                "Tracked Items",
                "Template Ids",
                string.Empty,
                "Доп. Tpl через запятую (поверх tracked-items.txt). Пусто = только файл.");

            _templateIdsFileEntry = config.Bind(
                "Tracked Items",
                "Template Ids File",
                "tracked-items.txt",
                "Файл со списком Tpl (по одному в строке) в папке BepInEx/plugins/RaidIntelOverlay/");

            _templateIdsEntry.SettingChanged += (_, __) => Reload();
            _templateIdsFileEntry.SettingChanged += (_, __) => Reload();
            Reload();
        }

        public IReadOnlyCollection<string> TemplateIds => _cachedIds;

        public void Reload()
        {
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            LoadFromFile(ids);
            AddParsed(ids, _templateIdsEntry.Value);
            RemoveLegacy(ids);

            _cachedIds = ids;
        }

        private void LoadFromFile(HashSet<string> ids)
        {
            try
            {
                var fileName = _templateIdsFileEntry.Value;
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return;
                }

                var path = Path.Combine(BepInEx.Paths.PluginPath, "RaidIntelOverlay", fileName);
                if (!File.Exists(path))
                {
                    return;
                }

                foreach (var line in File.ReadAllLines(path))
                {
                    AddToken(ids, line);
                }
            }
            catch
            {
                // ignore file read errors
            }
        }

        private static void RemoveLegacy(HashSet<string> ids)
        {
            foreach (var legacyId in LegacyTemplateIds)
            {
                ids.Remove(legacyId);
            }
        }

        private static void AddParsed(HashSet<string> ids, string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return;
            }

            foreach (var part in raw.Split(new[] { ',', ';', '\r', '\n', '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                AddToken(ids, part);
            }
        }

        private static void AddToken(HashSet<string> ids, string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return;
            }

            var trimmed = token.Trim();
            if (trimmed.StartsWith("#", StringComparison.Ordinal))
            {
                return;
            }

            if (LegacyTemplateIds.Contains(trimmed))
            {
                return;
            }

            ids.Add(trimmed);
        }
    }
}
