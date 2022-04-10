using System.Threading.Tasks;
using UnityEngine;

namespace Valkyrie.Profile
{
    internal class PlayerPrefsProfileLoader : IProfileLoader
    {
        private const string DataKey = "PlayerPrefsProfileLoaderData";
        
        public Task Load(DbSchema schema, DbContext dbContext)
        {
            var strData = PlayerPrefs.GetString(DataKey, "{}");
            schema.Deserialize(dbContext, strData);
            Debug.Log($"[PROFILE]: loaded {strData}");
            
            return Task.CompletedTask;
        }

        public Task Save(DbSchema schema, DbContext dbContext)
        {
            var strData = schema.Serialize(dbContext);
            PlayerPrefs.SetString(DataKey, strData);
            Debug.Log($"[PROFILE]: saved {strData}");
            
            return Task.CompletedTask;
        }
    }
}