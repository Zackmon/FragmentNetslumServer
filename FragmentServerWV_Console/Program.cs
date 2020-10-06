﻿using System;
using System.Collections.Generic;
using FragmentServerWV;
using FragmentServerWV_WebApi;
using FragmentServerWV.Models;
using FragmentServerWV.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;


namespace FragmentServerWV_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            LogEventDelegate logEventDelegate = new LogEventDelegate();
            logEventDelegate.Logging += LogToConsole;
            Config.Load();
            Log.InitLogs(logEventDelegate);
            Server.Start();
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
        
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}
