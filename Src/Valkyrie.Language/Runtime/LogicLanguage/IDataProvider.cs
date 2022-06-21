namespace Valkyrie.Language
{
    public interface IDataProvider
    {
        public int GetFactId(string factName);
        public string GetFactName(int factId);
        int Generate();
    }
}