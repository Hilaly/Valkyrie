using System.Collections.Generic;
using Valkyrie.Meta.DataSaver;

namespace Valkyrie.Meta.Models
{
    class InventoryProvider : DefaultModelProvider<InventoryModel>, IInventory
    {
        public InventoryProvider(IModelsProvider modelsProvider) : base(modelsProvider)
        {
        }

        public void Add(IInventoryItem item) => Model.Items.Add(item, item.Id);
        public T Get<T>(string id) where T : IInventoryItem => Model.Items.Get<T>(id);
        public IReadOnlyList<IInventoryItem> Get() => Model.Items.Get<IInventoryItem>();

        public IReadOnlyList<T> Get<T>() where T : IInventoryItem => Model.Items.Get<T>();

        public void Remove(string id) => Model.Items.Remove(id);
    }
}