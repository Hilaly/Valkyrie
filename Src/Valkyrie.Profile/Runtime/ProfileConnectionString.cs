namespace Valkyrie.Profile
{
    public class ProfileConnectionString
    {
        private readonly string _path;

        private ProfileConnectionString(string path)
        {
            _path = path;
        }

        public static ProfileConnectionString PlayerPrefs => new("playerPrefs");

        public override string ToString()
        {
            return _path;
        }
    }
}