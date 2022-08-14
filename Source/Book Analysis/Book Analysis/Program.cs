using Book_Analysis.Models;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace Book_Analysis
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //parse appsettings.json  add to Models
            var baseAddress = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var myJsonString = File.ReadAllText(baseAddress + @"\appsettings.json");
            var myJObject = JObject.Parse(myJsonString);
            Config.All = myJObject.ToObject<ConfigModels>();

            ApplicationConfiguration.Initialize();
            Application.Run(new Book_Analysis());
        }
    }
}