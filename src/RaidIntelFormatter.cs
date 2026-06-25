using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RaidIntelOverlay
{
    internal static class RaidIntelFormatter
    {
        private const int MaxColumnWidth = 22;
        private const int MinColumnWidth = 10;
        private const int ColumnGap = 2;

        public static string Format(
            RaidIntelSnapshot data,
            bool isRaidClient,
            bool hasRemotePresence,
            bool remotePresenceStale,
            bool isRaidHost)
        {
            var sb = new StringBuilder(1024);
            sb.AppendLine("<b>RAID INTEL</b>");
            sb.AppendLine("<color=#666666>────────────────────</color>");

            if (data.InRaid && !string.IsNullOrEmpty(data.MapArea.ActiveTriggerZones))
            {
                sb.AppendLine();
                sb.AppendFormat(
                    "<color=#888888>  TriggerZones:</color> <color=#C8C864>{0}</color>\n",
                    EscapeRichText(data.MapArea.ActiveTriggerZones));
            }

            sb.AppendLine();
            sb.AppendLine("<color=#BBBBBB><b>ИГРОКИ (НЕ БОТЫ)</b></color>");
            sb.Append("<color=#888888>  </color>");
            sb.AppendFormat("<color=#5DA8FF>USEC: {0}</color>  ", data.PlayersUsec);
            sb.AppendFormat("<color=#FF8C5A>BEAR: {0}</color>  ", data.PlayersBear);
            sb.AppendFormat("<color=#C8C864>Дикий: {0}</color>\n", data.PlayersSavage);

            sb.AppendLine();
            var botTotal = data.BotRoles.Values.Sum();
            sb.AppendFormat("<color=#BBBBBB><b>БОТЫ НА КАРТЕ ({0})</b></color>\n", botTotal);
            AppendBotColumns(sb, data.BotRoles);

            if (data.TrackedItems.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("<color=#BBBBBB><b>ОТСЛЕЖИВАЕМЫЕ ПРЕДМЕТЫ</b></color>");
                foreach (var pair in data.TrackedItems.OrderByDescending(p => p.Value).ThenBy(p => p.Key))
                {
                    if (!ItemNameResolver.IsKnownTemplate(pair.Key))
                    {
                        continue;
                    }

                    sb.AppendFormat(
                        "<color=#888888>  {0}:</color> <color=#7FD37F>{1}</color>\n",
                        EscapeRichText(ItemNameResolver.Resolve(pair.Key)),
                        pair.Value);
                }
            }

            if (!data.InRaid)
            {
                sb.AppendLine();
                sb.AppendLine("<color=#888888>Ожидание рейда...</color>");
            }
            else if (isRaidClient)
            {
                sb.AppendLine();
                if (!hasRemotePresence)
                {
                    sb.AppendLine("<color=#888888>  (ожидание данных хоста…)</color>");
                }
                else if (remotePresenceStale)
                {
                    sb.AppendLine("<color=#C86464>  (данные хоста устарели — ждём обновление…)</color>");
                }
                else
                {
                    sb.AppendLine("<color=#666666>  (игроки/боты с хоста)</color>");
                }
            }
            else if (isRaidHost)
            {
                sb.AppendLine();
                sb.AppendLine("<color=#666666>  (локальные данные хоста)</color>");
            }

            return sb.ToString();
        }

        private static void AppendBotColumns(StringBuilder sb, Dictionary<string, int> botRoles)
        {
            var entries = botRoles
                .Where(p => p.Value > 0)
                .Select(p => (Label: WildSpawnTypeLabels.GetDisplayName(p.Key), p.Value, Role: p.Key))
                .OrderByDescending(p => p.Value)
                .ThenBy(p => p.Label)
                .ToList();

            if (entries.Count == 0)
            {
                sb.AppendLine("<color=#888888>  Нет активных ботов</color>");
                return;
            }

            const int columnCount = 2;
            int rowCount = (entries.Count + columnCount - 1) / columnCount;
            var columns = new List<string>[columnCount];
            for (int i = 0; i < columnCount; i++)
            {
                columns[i] = new List<string>(rowCount);
            }

            for (int index = 0; index < entries.Count; index++)
            {
                int column = index / rowCount;
                if (column >= columnCount)
                {
                    column = columnCount - 1;
                }

                var entry = entries[index];
                columns[column].Add($"{entry.Label}: {entry.Value}");
            }

            int columnWidth = columns
                .Select(column => column.Count == 0 ? MinColumnWidth : column.Max(cell => cell.Length))
                .DefaultIfEmpty(MinColumnWidth)
                .Max();
            columnWidth = System.Math.Clamp(columnWidth, MinColumnWidth, MaxColumnWidth);

            for (int row = 0; row < rowCount; row++)
            {
                sb.Append("<color=#888888>  </color>");
                for (int col = 0; col < columnCount; col++)
                {
                    string cell = col < columns.Length && row < columns[col].Count
                        ? columns[col][row]
                        : string.Empty;
                    sb.Append(PadCell(cell, columnWidth));
                    if (col < columnCount - 1)
                    {
                        sb.Append(new string(' ', ColumnGap));
                    }
                }

                sb.AppendLine();
            }
        }

        private static string PadCell(string text, int width)
        {
            text ??= string.Empty;
            if (text.Length >= width)
            {
                return text;
            }

            return text + new string(' ', width - text.Length);
        }

        private static string EscapeRichText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            return text.Replace("<", "‹").Replace(">", "›");
        }
    }
}
