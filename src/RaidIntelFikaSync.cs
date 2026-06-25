using System;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using Fika.Core.Main.Utils;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;
using Fika.Core.Networking.LiteNetLib;

namespace RaidIntelOverlay
{
    internal sealed class RaidIntelFikaSync
    {
        private static RaidIntelFikaSync _instance;

        private ManualLogSource _logger;
        private IFikaNetworkManager _networkManager;
        private bool _packetsRegistered;
        private long _requestCounter;
        private RaidIntelPresenceSnapshot _remotePresence = new RaidIntelPresenceSnapshot();
        private bool _hasRemotePresence;
        private float _hostBroadcastTimer;
        private float _clientRequestTimer;
        private float _remotePresenceAge;
        private int _lastLoggedRemoteBotTotal = -1;
        private bool _loggedRaidRole;

        private const float HostBroadcastInterval = 3f;
        private const float ClientRequestInterval = 2.5f;
        private const float RemotePresenceStaleSeconds = 12f;

        public static RaidIntelFikaSync Instance => _instance ?? (_instance = new RaidIntelFikaSync());

        public bool HasRemotePresence => _hasRemotePresence;

        public bool IsRemotePresenceStale =>
            _hasRemotePresence && _remotePresenceAge > RemotePresenceStaleSeconds;

        public RaidIntelPresenceSnapshot RemotePresence => _remotePresence;

        public void Initialize(ManualLogSource logger)
        {
            _logger = logger;
            FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerCreatedEvent>(OnNetworkManagerCreated);
            FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerDestroyedEvent>(OnNetworkManagerDestroyed);
            FikaEventDispatcher.SubscribeEvent<FikaRaidStartedEvent>(OnRaidStarted);
            TryRefreshAvailability();
        }

        public void Shutdown()
        {
            FikaEventDispatcher.UnsubscribeEvent<FikaNetworkManagerCreatedEvent>(OnNetworkManagerCreated);
            FikaEventDispatcher.UnsubscribeEvent<FikaNetworkManagerDestroyedEvent>(OnNetworkManagerDestroyed);
            FikaEventDispatcher.UnsubscribeEvent<FikaRaidStartedEvent>(OnRaidStarted);
            ResetSessionState();
            _networkManager = null;
            _packetsRegistered = false;
        }

        public void Tick(float deltaTime, bool inRaid)
        {
            if (!inRaid)
            {
                ResetSessionState();
                return;
            }

            if (!TryRefreshAvailability())
            {
                return;
            }

            _remotePresenceAge += deltaTime;

            if (IsRaidHost())
            {
                _hostBroadcastTimer -= deltaTime;
                if (_hostBroadcastTimer > 0f)
                {
                    return;
                }

                _hostBroadcastTimer = HostBroadcastInterval;
                BroadcastHostPresence("tick");
                return;
            }

            if (!IsRaidClient())
            {
                return;
            }

            _clientRequestTimer -= deltaTime;
            if (_clientRequestTimer > 0f)
            {
                return;
            }

            _clientRequestTimer = ClientRequestInterval;
            RequestPresenceFromHost();
        }

        public bool TryRefreshAvailability()
        {
            if (_networkManager == null)
            {
                _networkManager = GetNetworkManagerFromSingletons();
            }

            if (_networkManager == null)
            {
                return false;
            }

            if (!_packetsRegistered)
            {
                RegisterPackets();
            }

            return _packetsRegistered;
        }

        public bool IsCoopActive()
        {
            try
            {
                return TryRefreshAvailability();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Хост рейда: Fika listen-host или dedicated headless (FikaBackendUtils.IsServer).
        /// </summary>
        public bool IsRaidHost()
        {
            return IsCoopActive() && FikaBackendUtils.IsServer;
        }

        /// <summary>
        /// Клиент рейда: join, headless-requester и любой игрок без IsServer.
        /// </summary>
        public bool IsRaidClient()
        {
            return IsCoopActive() && !FikaBackendUtils.IsServer;
        }

        /// <summary>
        /// Синк Fika нужен в рейде: у игрока есть MainPlayer или на хосте уже крутится bot game.
        /// </summary>
        public static bool ShouldRunFikaSync(GameWorld world)
        {
            if (world == null)
            {
                return false;
            }

            if (world.MainPlayer != null)
            {
                return true;
            }

            return FikaBackendUtils.IsServer && Singleton<IBotGame>.Instantiated;
        }

        public void RequestPresenceFromHost()
        {
            if (!IsRaidClient() || _networkManager == null)
            {
                return;
            }

            var packet = new RaidIntelFikaPackets.RaidIntelRequestPacket
            {
                RequestId = ++_requestCounter
            };

            SendData(ref packet, DeliveryMethod.ReliableOrdered);
        }

        public void PublishPresence(RaidIntelPresenceSnapshot presence)
        {
            if (!IsRaidHost() || _networkManager == null || presence == null)
            {
                return;
            }

            var packet = new RaidIntelFikaPackets.RaidIntelSnapshotPacket();
            packet.SetFrom(presence);
            SendData(ref packet, DeliveryMethod.ReliableOrdered, broadcast: true);
        }

        public RaidIntelPresenceSnapshot GetRemotePresenceForDisplay()
        {
            if (!_hasRemotePresence)
            {
                return null;
            }

            return _remotePresence;
        }

        private void OnNetworkManagerCreated(FikaNetworkManagerCreatedEvent evt)
        {
            _networkManager = evt.Manager;
            RegisterPackets();
        }

        private void OnNetworkManagerDestroyed(FikaNetworkManagerDestroyedEvent evt)
        {
            _networkManager = null;
            _packetsRegistered = false;
            ResetSessionState();
        }

        private void OnRaidStarted(FikaRaidStartedEvent evt)
        {
            _hostBroadcastTimer = 0f;
            _clientRequestTimer = 0f;
            _loggedRaidRole = false;

            if (!TryRefreshAvailability())
            {
                _logger?.LogWarning("[RAID_INTEL] Raid started but Fika network manager is not ready");
                return;
            }

            LogRaidRoleOnce();

            if (IsRaidHost())
            {
                BroadcastHostPresence("raid-started");
                return;
            }

            if (IsRaidClient())
            {
                RequestPresenceFromHost();
            }
        }

        private IFikaNetworkManager GetNetworkManagerFromSingletons()
        {
            if (FikaBackendUtils.IsServer && Singleton<FikaServer>.Instantiated)
            {
                return Singleton<FikaServer>.Instance;
            }

            if (Singleton<FikaClient>.Instantiated)
            {
                return Singleton<FikaClient>.Instance;
            }

            return Singleton<FikaServer>.Instance;
        }

        private void RegisterPackets()
        {
            if (_packetsRegistered || _networkManager == null)
            {
                return;
            }

            _networkManager.RegisterPacket<RaidIntelFikaPackets.RaidIntelRequestPacket>(OnRequestPacketReceived);
            _networkManager.RegisterPacket<RaidIntelFikaPackets.RaidIntelSnapshotPacket>(OnSnapshotPacketReceived);
            _packetsRegistered = true;
            _logger?.LogInfo("[RAID_INTEL] Fika packets registered");
        }

        private void OnRequestPacketReceived(RaidIntelFikaPackets.RaidIntelRequestPacket packet)
        {
            if (!IsRaidHost())
            {
                return;
            }

            BroadcastHostPresence($"request#{packet.RequestId}");
        }

        private void OnSnapshotPacketReceived(RaidIntelFikaPackets.RaidIntelSnapshotPacket packet)
        {
            if (IsRaidHost() || packet == null)
            {
                return;
            }

            _remotePresence = packet.ToPresenceSnapshot();
            _hasRemotePresence = true;
            _remotePresenceAge = 0f;

            var botTotal = 0;
            foreach (var count in _remotePresence.BotRoles.Values)
            {
                botTotal += count;
            }

            if (botTotal != _lastLoggedRemoteBotTotal)
            {
                _lastLoggedRemoteBotTotal = botTotal;
                _logger?.LogInfo(
                    $"[RAID_INTEL] Snapshot from host: USEC={_remotePresence.PlayersUsec} BEAR={_remotePresence.PlayersBear} Savage={_remotePresence.PlayersSavage} bots={botTotal}");
            }
        }

        private void BroadcastHostPresence(string reason)
        {
            try
            {
                var presence = RaidIntelCollector.CollectPresence();
                PublishPresence(presence);

                var botTotal = 0;
                foreach (var count in presence.BotRoles.Values)
                {
                    botTotal += count;
                }

                _logger?.LogInfo($"[RAID_INTEL] Host broadcast ({reason}): bots={botTotal}");
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"[RAID_INTEL] Failed to broadcast presence ({reason}): {ex.Message}");
            }
        }

        private void LogRaidRoleOnce()
        {
            if (_loggedRaidRole)
            {
                return;
            }

            _loggedRaidRole = true;
            _logger?.LogInfo(
                "[RAID_INTEL] Role: " +
                $"host={IsRaidHost()} client={IsRaidClient()} " +
                $"isServer={FikaBackendUtils.IsServer} isClient={FikaBackendUtils.IsClient} " +
                $"headlessGame={FikaBackendUtils.IsHeadlessGame} headlessRequester={FikaBackendUtils.IsHeadlessRequester}");
        }

        private void ResetSessionState()
        {
            _hasRemotePresence = false;
            _remotePresence = new RaidIntelPresenceSnapshot();
            _remotePresenceAge = 0f;
            _hostBroadcastTimer = 0f;
            _clientRequestTimer = 0f;
            _lastLoggedRemoteBotTotal = -1;
            _loggedRaidRole = false;
        }

        private void SendData<T>(ref T packet, DeliveryMethod deliveryMethod, bool broadcast = false)
            where T : Fika.Core.Networking.LiteNetLib.Utils.INetSerializable
        {
            if (_networkManager == null)
            {
                return;
            }

            _networkManager.SendData(ref packet, deliveryMethod, broadcast);
        }
    }
}
