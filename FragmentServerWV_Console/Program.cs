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
        static async Task Main(string[] args)
        {
            LogEventDelegate logEventDelegate = new LogEventDelegate();
            logEventDelegate.Logging += LogToConsole;
            Config.Load();
            Log.InitLogs(logEventDelegate);
            Server.Instance.Start();
            
            CreateHostBuilder(args).Build().Run();



            /*DBAcess dbAcess = DBAcess.getInstance();

            List<BbsCategoryModel> bbsCategoryModels= dbAcess.GetListOfBbsCategory();
            List<BbsThreadModel> bbsThreadModels = dbAcess.getThreadsByCategoryID(bbsCategoryModels[0].categoryID);
            List<BbsPostMetaModel> bbsPostMetaModels = dbAcess.getPostsMetaByThreadID(bbsThreadModels[0].threadID);
            BbsPostBody bbsPostBody = dbAcess.getPostBodyByPostID(bbsPostMetaModels[0].postID);
            
            Console.WriteLine(bbsPostBody.postBody);*/

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
