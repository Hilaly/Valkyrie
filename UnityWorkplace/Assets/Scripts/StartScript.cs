using UnityEngine;
using Valkyrie.Di;

public class StartScript : MonoBehaviour
{
    private void Awake()
    {
        TestContainerInterface();
    }

    private void TestContainerInterface()
    {
        //1. Create container
        using IContainer container = new Container();
        //2. Register types in container
        container.Register(this).AsInterfacesAndSelf().NonLazy();
        //3. Build Dependency graph
        container.Build();
        //4. Resolve dependencies
        Debug.Assert(this == container.Resolve<StartScript>());
        //5. Dispose after using (using operator in first line of method)
    }
}