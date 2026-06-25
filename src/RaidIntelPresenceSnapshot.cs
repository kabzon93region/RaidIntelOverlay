using System;
using System.Collections.Generic;

namespace RaidIntelOverlay
{
    internal sealed class RaidIntelPresenceSnapshot
    {
        public int PlayersUsec;
        public int PlayersBear;
        public int PlayersSavage;
        public Dictionary<string, int> BotRoles = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public void CopyFrom(RaidIntelPresenceSnapshot other)
        {
            if (other == null)
            {
                return;
            }

            PlayersUsec = other.PlayersUsec;
            PlayersBear = other.PlayersBear;
            PlayersSavage = other.PlayersSavage;
            BotRoles = new Dictionary<string, int>(other.BotRoles, StringComparer.OrdinalIgnoreCase);
        }

        public void ApplyTo(RaidIntelSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            snapshot.PlayersUsec = PlayersUsec;
            snapshot.PlayersBear = PlayersBear;
            snapshot.PlayersSavage = PlayersSavage;
            snapshot.BotRoles = new Dictionary<string, int>(BotRoles, StringComparer.OrdinalIgnoreCase);
        }
    }
}
