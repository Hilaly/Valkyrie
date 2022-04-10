using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Valkyrie.Profile
{
    public abstract class DbContext : IDisposable
    {
        private readonly IProfileLoader _loader;
        private readonly DbSchema _dbSchema;

        public ulong Id { get; set; }
        
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

            _dbSchema = new DbSchema();
        }

        public void Add(object o)
        {
            _dbSchema.Context.Add(o);
        }

        public void Remove(object o)
        {
            _dbSchema.Context.Remove(o);
        }

        public List<T> GetAll<T>()
        {
            return _dbSchema.Context.Get(typeof(T)).Values.OfType<T>().ToList();
        }

        public void Dispose()
        {
            SaveAsync().Wait();
        }

        public async Task LoadAsync()
        {
            await _loader.Load(_dbSchema, this);
        }

        public async Task SaveAsync()
        {
            await _loader.Save(_dbSchema, this);
        }
    }
}