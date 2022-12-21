using Newtonsoft.Json;
using Valkyrie.Meta.DataSaver;

namespace Valkyrie.Meta.PlayerInfo
{
    class PlayerInfoProvider : IPlayerInfoProvider, ISaveDataProvider
    {
        public PlayerInfo Info { get; private set; } = new();

        public string Key => "INTERNAL_PLAYER_INFO";
        
        public string GetData() => JsonConvert.SerializeObject(Info);

        public void SetData(string jsonData)
        {
            Info = JsonConvert.DeserializeObject<PlayerInfo>(jsonData);
        }
    }
}