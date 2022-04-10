using System.Threading.Tasks;

namespace Valkyrie.Profile
{
    interface IProfileLoader
    {
        Task Load(DbSchema schema, DbContext dbContext);
        Task Save(DbSchema schema, DbContext dbContext);
    }
}