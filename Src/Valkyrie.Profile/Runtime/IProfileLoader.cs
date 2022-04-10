namespace Valkyrie.Profile
{
    interface IProfileLoader
    {
        void Load(DbSchema schema, DbContext dbContext);
        void Save(DbSchema schema, DbContext dbContext);
    }
}