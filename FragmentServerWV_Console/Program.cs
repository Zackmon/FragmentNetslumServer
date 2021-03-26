using FragmentServerWV;
using FragmentServerWV_WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Text;
using System.Threading.Tasks;

namespace FragmentServerWV_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Config.Load();
            Server.Instance.Start();
            CreateHostBuilder(args).Build().Run();
        }

        public static void LogToConsole(String text, int logSize)
        {
            Console.WriteLine(text);
        }
        
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("http://").Append(Config.configs["ip"]).Append(":5000");
            //StringBuilder sbSecure = new StringBuilder();
            //sbSecure.Append("https://").Append(Config.configs["ip"]).Append(":5001");
            
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls(sb.ToString());
                });
        }
    }
}
