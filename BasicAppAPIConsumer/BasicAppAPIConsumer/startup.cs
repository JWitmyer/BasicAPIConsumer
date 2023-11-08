using Azure.Data.Tables;
using MerriamWebster.NET;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


[assembly: FunctionsStartup(typeof(BasicAppAPIConsumer.Startup))]

namespace BasicAppAPIConsumer
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {

            //Will leave this blank for now could connect it to app insights Logger spec not needed atm

            //builder.Services.AddSingleton<ILoggerProvider, MyLoggerProvider>();
            var config = BuildConfiguration(builder.GetContext().ApplicationRootPath);

            builder.Services.AddAppConfiguration(config);

             

            builder.Services.AddScoped<ICheapoRepo, CheapoRepo>();
            builder.Services.AddScoped<ISearchWordAndPersist, SearchWordAndPersist>();

            var tableClientConnectionString = config["WordlyerOptions:TableConnStr"];

            builder.Services.AddScoped(_ => new TableServiceClient(tableClientConnectionString));
            builder.Services.AddScoped<ICheapoRepo, CheapoRepo>();


            var mWconfig = new MerriamWebsterConfig{ApiKey = config["WordlyerOptions:DictonaryKey"] };
            builder.Services.RegisterMerriamWebster(mWconfig);


            //This is not ideal. Was having issues with the config DI 
            builder.Services.AddSingleton(new WordlyerOptions
            {
                DictonaryKey = config["WordlyerOptions:DictonaryKey"]
            }); 

        }
        

        private IConfiguration BuildConfiguration(string applicationRootPath)
        {
            var config =
                new ConfigurationBuilder()
                    .SetBasePath(applicationRootPath)
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    //.AddJsonFile("settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();

            return config;
        }
    }

    public class WordlyerOptions
    {
        public string DictonaryKey { get; set; }
        public string ThesaurusKey { get; set; }
    }

    internal static class ConfigurationServiceCollectionExtensions
    {
        public static IServiceCollection AddAppConfiguration(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<WordlyerOptions>(config.GetSection(nameof(WordlyerOptions)));
            return services;
        }
    }
}
