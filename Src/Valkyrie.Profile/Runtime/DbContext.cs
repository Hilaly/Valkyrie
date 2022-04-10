using System;

namespace Valkyrie.Profile
{
    public abstract class DbContext : IDisposable
    {
        private readonly IProfileLoader _loader;
        private readonly DbSchema _dbSchema;

        protected DbContext(ProfileConnectionString connectionString)
        {
            switch (connectionString.ToString())
            {
                case "playerPrefs":
                    _loader = new PlayerPrefsProfileLoader();
                    break;
                default:
                    throw new NotImplementedException("Now you can use only player prefs data storage");
            }

            _dbSchema = new DbSchema(GetType());
            
            _loader.Load(_dbSchema, this);
        }

        public void Remove(object o)
        {
            throw new NotImplementedException();
        }

        public void SaveChanges()
        {
            _loader.Save(_dbSchema, this);
        }

        public void Dispose()
        {
            SaveChanges();
        }
    }
}