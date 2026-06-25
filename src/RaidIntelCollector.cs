using System;

using System.Collections.Generic;

using System.Linq;

using Comfort.Common;

using EFT;

using EFT.Interactive;

using EFT.InventoryLogic;
using Fika.Core.Main.Components;



namespace RaidIntelOverlay

{

    internal sealed class RaidIntelSnapshot

    {

        public int PlayersUsec;

        public int PlayersBear;

        public int PlayersSavage;

        public Dictionary<string, int> BotRoles = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, int> TrackedItems = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public MapAreaSnapshot MapArea = new MapAreaSnapshot();

        public bool InRaid;

        public bool UsesRemotePresence;

    }



    internal static class RaidIntelCollector

    {

        public static RaidIntelSnapshot Collect(IEnumerable<string> trackedTemplateIds, RaidIntelPresenceSnapshot remotePresence = null)

        {

            var snapshot = new RaidIntelSnapshot();

            if (!Singleton<GameWorld>.Instantiated)

            {

                return snapshot;

            }



            var world = Singleton<GameWorld>.Instance;

            snapshot.InRaid = world.MainPlayer != null;

            if (snapshot.InRaid)

            {

                snapshot.MapArea = MapAreaResolver.Collect();

            }



            if (remotePresence != null)

            {

                remotePresence.ApplyTo(snapshot);

                snapshot.UsesRemotePresence = true;

            }

            else

            {

                ApplyPresence(snapshot, CollectPresence(world));

            }



            var tracked = trackedTemplateIds?.ToList() ?? new List<string>();

            if (tracked.Count > 0)

            {

                var counts = CountTrackedTemplates(world, tracked);

                foreach (var pair in counts)

                {

                    snapshot.TrackedItems[pair.Key] = pair.Value;

                }

            }



            return snapshot;

        }



        public static RaidIntelPresenceSnapshot CollectPresence()

        {

            if (!Singleton<GameWorld>.Instantiated)

            {

                return new RaidIntelPresenceSnapshot();

            }



            return CollectPresence(Singleton<GameWorld>.Instance);

        }



        private static RaidIntelPresenceSnapshot CollectPresence(GameWorld world)

        {

            var presence = new RaidIntelPresenceSnapshot();

            var countedHumanIds = new HashSet<string>(StringComparer.Ordinal);

            foreach (var player in world.RegisteredPlayers)

            {

                if (!IsHumanPlayer(player))

                {

                    continue;

                }

                if (!countedHumanIds.Add(player.ProfileId))

                {

                    continue;

                }

                CountHumanSide(presence, player.Profile.Info.Side);

            }

            if (CoopHandler.TryGetCoopHandler(out var coopHandler) && coopHandler?.HumanPlayers != null)

            {

                foreach (var human in coopHandler.HumanPlayers)

                {

                    if (human?.Profile?.Info == null || human.IsAI)

                    {

                        continue;

                    }

                    if (!countedHumanIds.Add(human.ProfileId))

                    {

                        continue;

                    }

                    CountHumanSide(presence, human.Profile.Info.Side);

                }

            }



            if (Singleton<IBotGame>.Instantiated)

            {

                var botOwners = Singleton<IBotGame>.Instance?.BotsController?.Bots?.BotOwners;

                if (botOwners != null)

                {

                    foreach (var bot in botOwners)

                    {

                        if (!IsActiveBot(bot))

                        {

                            continue;

                        }



                        var role = bot.Profile.Info.Settings.Role.ToString();

                        if (!presence.BotRoles.ContainsKey(role))

                        {

                            presence.BotRoles[role] = 0;

                        }



                        presence.BotRoles[role]++;

                    }

                }

            }



            return presence;

        }



        private static void ApplyPresence(RaidIntelSnapshot snapshot, RaidIntelPresenceSnapshot presence)

        {

            presence?.ApplyTo(snapshot);

        }



        private static void CountHumanSide(RaidIntelPresenceSnapshot presence, EPlayerSide side)

        {

            switch (side)

            {

                case EPlayerSide.Usec:

                    presence.PlayersUsec++;

                    break;

                case EPlayerSide.Bear:

                    presence.PlayersBear++;

                    break;

                case EPlayerSide.Savage:

                    presence.PlayersSavage++;

                    break;

            }

        }



        private static bool IsHumanPlayer(IPlayer player)

        {

            if (player?.Profile?.Info == null || player.IsAI)

            {

                return false;

            }



            return true;

        }



        /// <summary>

        /// Считаем только живых ботов: мёртвые иногда остаются в BotOwners до Remove.

        /// </summary>

        private static bool IsActiveBot(BotOwner bot)

        {

            if (bot?.Profile?.Info?.Settings == null)

            {

                return false;

            }



            if (bot.IsDead)

            {

                return false;

            }



            if (bot.BotState == EBotState.Disposed)

            {

                return false;

            }



            try

            {

                var player = bot.GetPlayer;

                if (player?.HealthController != null && !player.HealthController.IsAlive)

                {

                    return false;

                }

            }

            catch

            {

                // ignore reflection / lifecycle edge cases

            }



            return true;

        }



        private static Dictionary<string, int> CountTrackedTemplates(GameWorld world, List<string> templateIds)

        {

            var totals = templateIds.ToDictionary(id => id, _ => 0, StringComparer.OrdinalIgnoreCase);



            foreach (var killable in world.LootList)

            {

                if (killable is LootItem lootItem && lootItem.Item != null)

                {

                    AddItemCounts(totals, lootItem.Item);

                }

                else if (killable is LootableContainer container && container.ItemOwner?.RootItem != null)

                {

                    AddItemCounts(totals, container.ItemOwner.RootItem);

                }

            }



            foreach (var player in world.RegisteredPlayers)

            {

                var inventory = player?.Profile?.Inventory;

                if (inventory == null)

                {

                    continue;

                }



                foreach (var templateId in templateIds)

                {

                    foreach (var item in inventory.GetAllItemByTemplate(templateId))

                    {

                        if (item == null)

                        {

                            continue;

                        }



                        totals[templateId] += Math.Max(1, item.StackObjectsCount);

                    }

                }

            }



            return totals;

        }



        private static void AddItemCounts(Dictionary<string, int> totals, Item rootItem)

        {

            foreach (var item in rootItem.GetAllItems())

            {

                if (item?.TemplateId == null)

                {

                    continue;

                }



                var tpl = item.TemplateId.ToString();

                if (totals.ContainsKey(tpl))

                {

                    totals[tpl] += Math.Max(1, item.StackObjectsCount);

                }

            }

        }

    }

}


