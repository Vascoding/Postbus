using System.IO;
using Newtonsoft.Json.Linq;
using Postbus.Client.Enums;

namespace Postbus.Client
{
    static class Configuration
    {
        public static string GetHostName(AppMode mode)
        {
            var configs = JObject.Parse(File.ReadAllText("../../../appsettings.json"));

            return configs["Server"][mode.ToString()].ToString();
        }
    }
}