namespace Valkyrie
{
    public static class SimulationOrder
    {
        public const int ReadPlayerInput = -1000;
        public const int ReadAiInput = -900;

        public const int ApplyInput = -800;
        
        public const int Default = 0;

        public const int ApplyPhysicData = 100;
        public const int SimulatePhysic = 200;
        public const int ReadPhysicData = 300;

        public const int ProcessAfterPhysic = 500;

        public const int ApplyToView = 1000;
    }
}