using System;

namespace Meta.PlayerInfo
{
    public class PlayerInfo
    {
        public DateTime Created;
        public DateTime Updated;

        public PlayerInfo() => Updated = Created = DateTime.UtcNow;
    }
}