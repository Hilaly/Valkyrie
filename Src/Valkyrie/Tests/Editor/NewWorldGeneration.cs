using System.IO;
using Editor.ClassEntitiesModel;
using NUnit.Framework;
using UnityEngine;

public class NewWorldGeneration
{
    // A Test behaves as an ordinary method
    [Test]
    public void TestWorldGeneration()
    {
        var world = new WorldModelInfo
        {
            Namespace = "Naive"
        };

        var trInterface = world.CreateEntityInterface("ITransform")
            .AddProperty<Vector3>("Position")
            .AddProperty<Quaternion>("Rotation");

        var visibleInterface = world.CreateEntityInterface("IVisible")
            .Inherit(trInterface)
            .AddConfig("Valkyrie.Entities.IEntity", "Config")
            .AddProperty("string", "AssetName");

        var movableInterface = world.CreateEntityInterface("IMovable")
            .Inherit(trInterface)
            .AddProperty<Vector3>("MoveDirection", false)
            .AddProperty("Hilaly.Tools.IPooledInstance<Rigidbody>", "Physic", false)
            .AddInfo("float", "Speed", "1f");

        var takenOrder = world.CreateEntity("Order")
            ;

        var ep = world.CreateEntity("Player")
            .Inherit(visibleInterface, movableInterface)
            .AddProperty("Hilaly.Tools.INavPath", "Path", false)
            .AddTimer("NavigationTimer")
            .AddSlot("Order", "Order")
            .AddProperty<int>("Money", false)
            .Singleton();

        var freeOrder = world.CreateEntity("FreeOrder")
            .AddProperty<string>("SourcePoint")
            .AddProperty<string>("TargetPoint")
            .AddProperty<string>("PersonName")
            .AddTimer("AvailableTimer")
            .AddProperty<string>("Goods")
            .AddProperty<int>("Reward");

        Debug.Log(world);
        // Use the Assert class to test conditions

        File.WriteAllText(Path.Combine("Assets", "Scripts", "Gen.cs"), world.ToString());
    }
}