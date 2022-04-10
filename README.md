# Valkyrie
A set of tools for unity

## Valkyrie.Di
A very simple implementation of Dependency Injection Container.
To use it in your project just include package "https://github.com/Hilaly/Valkyrie.git?path=/Src/Valkyrie.Di"
```c#
class BaseClass {}
class InheritedClass : BaseClass {}

void SampleMethod()
{
    var container = new Valkyrie.Di.Container();
    container.Register<InheritedClass>().AsInterfacesAndSelf().As<BaseClass>().SingleInstance().NonLazy();
    container.Build();
    var resolvedInstance = container.Resolve<InheritedClass>();
    var resolvedInheritedInstance = container.Resolve<BaseClass>();
    Debug.Assert(resolvedInstance == resolvedInheritedInstance);
    container.Dispose(); //All created instances, that implement IDisposable will be disposed here
}
```

## Valkyrie.Profile
Non-optimized version of saving data between game sessions, inspired by Entity Framework.
Provides an automatic mechanism for saving and loading data. 
Allows to use a hierarchical data model for the view. Able to save and restore references to entities.
To use it, you must create a context class and inherit from DbContext. 
All data added as public properties to the context will be automatically loaded and saved.

It has some limitations: 
- Properties of types are supported as a key: byte, sales, short, short, int, wint, long, ulong
- Types of data are also supported: string, float, double
- Supports only playerPrefs as storage now

```c#
//Model entites
public class Unit
{
    //Entities must have property named 'Id' or '<ClassName>Id' 
    public int Id { get; set; }
    //Entites can have references on other references
    public Weapon Weapon { get; set; }
}

public class Weapon
{
    public long WeaponId { get; set; }
    //or simple properties
    public int Strength { get; set; }
}

//Context
public class PlayerContext : DbContext
{
    public PlayerContext() : base(ProfileConnectionString.PlayerPrefs)
    {
    }
    
    public int Value { get; set; }
    public List<Unit> Units { get; set; }
    public List<Weapon> Weapons { get; set; }
}

async void SampleMethod()
{
    //Create context
    using var context = new PlayerContext();
    //Restore data
    await context.LoadAsync();
    //Some works with data
    context.Weapons.Add(new Weapon());
    //Saving manual
    await _profile.SaveAsync();
    //Or it will be saved on context.Dispose()
}
```

To use this in your project, you need to include a package "https://github.com/Hilaly/Valkyrie.git?path=/Src/Valkyrie.Profile"