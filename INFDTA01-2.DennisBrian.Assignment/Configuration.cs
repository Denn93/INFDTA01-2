using System.Configuration;

namespace INFDTA01_2.DennisBrian.Assignment
{
    public static class Configuration
    {
        public static string MySqlConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["mysql"].ConnectionString; }
        }
    }
}