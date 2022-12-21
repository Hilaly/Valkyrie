using Valkyrie.Meta.DataSaver;

namespace Valkyrie.Meta.Models
{
    class InventoryModel : BaseModel
    {
        public readonly DataStorage<IInventoryItem> Items = new();
    }
}