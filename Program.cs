using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace api.cabcheap.com
{
    public class Program
    {

        public static string DevUrl = "http://127.0.0.1:8888";
        public static string ProdUrl = "http://127.0.0.1:8888";
        
        public static void Main(string[] args)
        {
            /* foreach (var item in args)
            {
                Console.WriteLine("item");
            } */
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            //export ASPNETCORE_ENVIRONMENT=Development or Production
            string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            Console.WriteLine(env);
            string url = ProdUrl;
            if(env.Equals("Development")) 
            {
                url = DevUrl;
            }
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls(url)
                .Build();
        }
    }
}
