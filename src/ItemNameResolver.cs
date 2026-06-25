using System;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;

namespace RaidIntelOverlay
{
    /// <summary>
    /// Имя предмета только из игрового рантайма: ItemFactoryClass + LocaleManager (как в UI EFT).
    /// Без items.json, без внешних справочников и без файлового кэша.
    /// </summary>
    internal static class ItemNameResolver
    {
        public static string Resolve(string templateId)
        {
            if (string.IsNullOrWhiteSpace(templateId))
            {
                return templateId;
            }

            return TryResolve(templateId, out var name) ? name : FormatUnknown(templateId.Trim());
        }

        public static bool IsKnownTemplate(string templateId)
        {
            return TryResolve(templateId, out _);
        }

        private static bool TryResolve(string templateId, out string displayName)
        {
            displayName = null;

            if (string.IsNullOrWhiteSpace(templateId))
            {
                return false;
            }

            var normalized = templateId.Trim();

            try
            {
                if (!Singleton<ItemFactoryClass>.Instantiated)
                {
                    return false;
                }

                var factory = Singleton<ItemFactoryClass>.Instance;
                if (factory?.ItemTemplates == null)
                {
                    return false;
                }

                if (!factory.ItemTemplates.TryGetValue(normalized, out var template) || template == null)
                {
                    return false;
                }

                MongoID mongoId = normalized;

                var fullName = mongoId.LocalizedName();
                if (IsValidLocalizedName(normalized, fullName))
                {
                    displayName = fullName;
                    return true;
                }

                var fromTemplate = template.NameLocalizationKey.Localized();
                if (IsValidLocalizedName(template.NameLocalizationKey, fromTemplate))
                {
                    displayName = fromTemplate;
                    return true;
                }

                var fromShort = template.ShortNameLocalizationKey.Localized();
                if (IsValidLocalizedName(template.ShortNameLocalizationKey, fromShort))
                {
                    displayName = fromShort;
                    return true;
                }
            }
            catch
            {
                // ItemFactory или локаль ещё не готовы
            }

            return false;
        }

        public static void ClearCache()
        {
            // Намеренно пусто: справочник не ведём, каждый раз читаем из игры.
        }

        private static bool IsValidLocalizedName(string contextId, string candidate)
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                return false;
            }

            if (string.Equals(candidate, contextId, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (candidate.EndsWith(" Name", StringComparison.Ordinal) ||
                candidate.EndsWith(" ShortName", StringComparison.Ordinal))
            {
                return false;
            }

            return true;
        }

        private static string FormatUnknown(string templateId)
        {
            if (templateId.Length <= 12)
            {
                return templateId;
            }

            return templateId.Substring(0, 10) + "…";
        }
    }
}
