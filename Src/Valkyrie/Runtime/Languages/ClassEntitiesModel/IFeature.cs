namespace Valkyrie
{
    public interface IFeature
    {
        public string Name { get; }
        
        void Import(WorldModelInfo world);
    }
    
    public interface IEntity { }
	
    public interface ISimSystem
    {
        void Simulate(float dt);
    }

}