using System;
using System.Collections.Generic;
using Fika.Core.Networking.LiteNetLib.Utils;

namespace RaidIntelOverlay
{
    internal static class RaidIntelFikaPackets
    {
        internal sealed class RaidIntelRequestPacket : INetSerializable
        {
            public long RequestId;

            public void Serialize(NetDataWriter writer)
            {
                writer.Put(RequestId);
            }

            public void Deserialize(NetDataReader reader)
            {
                RequestId = reader.GetLong();
            }
        }

        internal sealed class RaidIntelSnapshotPacket : INetSerializable
        {
            public int PlayersUsec;
            public int PlayersBear;
            public int PlayersSavage;
            public readonly List<string> BotRoleKeys = new List<string>();
            public readonly List<int> BotRoleCounts = new List<int>();

            public void SetFrom(RaidIntelPresenceSnapshot snapshot)
            {
                PlayersUsec = snapshot?.PlayersUsec ?? 0;
                PlayersBear = snapshot?.PlayersBear ?? 0;
                PlayersSavage = snapshot?.PlayersSavage ?? 0;
                BotRoleKeys.Clear();
                BotRoleCounts.Clear();

                if (snapshot?.BotRoles == null)
                {
                    return;
                }

                foreach (var pair in snapshot.BotRoles)
                {
                    if (pair.Value <= 0 || string.IsNullOrEmpty(pair.Key))
                    {
                        continue;
                    }

                    BotRoleKeys.Add(pair.Key);
                    BotRoleCounts.Add(pair.Value);
                }
            }

            public RaidIntelPresenceSnapshot ToPresenceSnapshot()
            {
                var snapshot = new RaidIntelPresenceSnapshot
                {
                    PlayersUsec = PlayersUsec,
                    PlayersBear = PlayersBear,
                    PlayersSavage = PlayersSavage
                };

                for (int i = 0; i < BotRoleKeys.Count && i < BotRoleCounts.Count; i++)
                {
                    snapshot.BotRoles[BotRoleKeys[i]] = BotRoleCounts[i];
                }

                return snapshot;
            }

            public void Serialize(NetDataWriter writer)
            {
                writer.Put(PlayersUsec);
                writer.Put(PlayersBear);
                writer.Put(PlayersSavage);
                writer.Put(BotRoleKeys.Count);

                for (int i = 0; i < BotRoleKeys.Count; i++)
                {
                    writer.Put(BotRoleKeys[i] ?? string.Empty);
                    writer.Put(i < BotRoleCounts.Count ? BotRoleCounts[i] : 0);
                }
            }

            public void Deserialize(NetDataReader reader)
            {
                PlayersUsec = reader.GetInt();
                PlayersBear = reader.GetInt();
                PlayersSavage = reader.GetInt();

                BotRoleKeys.Clear();
                BotRoleCounts.Clear();

                int count = reader.GetInt();
                for (int i = 0; i < count; i++)
                {
                    BotRoleKeys.Add(reader.GetString());
                    BotRoleCounts.Add(reader.GetInt());
                }
            }
        }
    }
}
