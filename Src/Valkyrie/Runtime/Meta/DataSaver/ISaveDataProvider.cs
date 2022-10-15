namespace Meta
{
    public interface ISaveDataProvider
    {
        string Key { get; }
        string GetData();
        void SetData(string jsonData);
    }
}