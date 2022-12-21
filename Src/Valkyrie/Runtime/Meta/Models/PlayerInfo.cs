using System;
using Valkyrie.Meta.DataSaver;

namespace Valkyrie.Meta.Models
{
    public class PlayerInfo : BaseModel
    {
        public DateTime Created;
        public DateTime Updated;

        public PlayerInfo() => Updated = Created = DateTime.UtcNow;
    }
}