using UnityEngine;

namespace Valkyrie.Profile
{
    class PlayerPrefsProfileLoader : IProfileLoader
    {
        private const string DataKey = "PlayerPrefsProfileLoaderData";
        
        public void Load(DbSchema schema, DbContext dbContext)
        {
            var strData = PlayerPrefs.GetString(DataKey, "{}");
            schema.Deserialize(dbContext, strData);
            Debug.Log($"[PROFILE]: loaded {strData}");
        }

        public void Save(DbSchema schema, DbContext dbContext)
        {
            var strData = schema.Serialize(dbContext);
            PlayerPrefs.SetString(DataKey, strData);
            Debug.Log($"[PROFILE]: saved {strData}");
        }
    }
}