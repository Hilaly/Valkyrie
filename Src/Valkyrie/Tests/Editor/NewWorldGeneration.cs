using System.IO;
using Editor.ClassEntitiesModel;
using NUnit.Framework;
using UnityEngine;

namespace Valkyrie.Language
{
    public class NewWorldGeneration
    {
        // A Test behaves as an ordinary method
        [Test]
        public void DeliveryMasters()
        {
            var world = new WorldModelInfo
            {
                Namespace = "Naive"
            };

            var trInterface = world.CreateEntityInterface("ITransform")
                .AddProperty<Vector3>("Position")
                .AddProperty<Quaternion>("Rotation")
                .AddTimer("TestTimer")
                .View();

            var visibleInterface = world.CreateEntityInterface("IVisible")
                .Inherit(trInterface)
                .AddConfig("Valkyrie.Entities.IEntity", "Config")
                .AddProperty("string", "AssetName")
                .ViewWithPrefabByProperty("AssetName");

            var movableInterface = world.CreateEntityInterface("IMovable")
                .Inherit(trInterface)
                .AddProperty<Vector3>("MoveDirection", false)
                .AddProperty("Hilaly.Tools.IPooledInstance<Rigidbody>", "Physic", false)
                .AddInfo("float", "Speed", "1f")
                .View();

            var orderInterface = world.CreateEntityInterface("IOrder")
                .AddProperty<string>("SourcePoint")
                .AddProperty<string>("TargetPoint")
                .AddProperty<string>("PersonName")
                .AddProperty<string>("Goods")
                .AddProperty<int>("Reward")
                .View();

            var takenOrder = world.CreateEntity("Order")
                .Inherit(orderInterface)
                .AddTimer("DeliveryTimer")
                .View();

            var ep = world.CreateEntity("Player")
                .Inherit(visibleInterface, movableInterface)
                .AddProperty("Hilaly.Tools.INavPath", "Path", false)
                .AddTimer("NavigationTimer")
                .AddSlot(takenOrder, "Order")
                .AddProperty<int>("Money", false)
                .Singleton()
                .ViewWithPrefabByProperty("AssetName")
                .View();

            var freeOrder = world.CreateEntity("FreeOrder")
                .Inherit(orderInterface)
                .AddTimer("AvailableTimer")
                .View();

            Debug.Log(world);
            // Use the Assert class to test conditions

            File.WriteAllText(Path.Combine("Assets", "Scripts", "Gen.cs"), world.ToString());
        }
    }
}