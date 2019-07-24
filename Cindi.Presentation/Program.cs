using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetEscapades.Extensions.Logging.RollingFile;

namespace Cindi.Presentation
{
    public class Program
    {
        public static FileLoggerOptions _fileLoggingOptions { get; set; }
        public static bool EnableLogToFile { get; set; }

        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddCommandLine(args)
                .Build();

            _fileLoggingOptions = config.GetSection("Logging:File").Get<FileLoggerOptions>();
            // config.Bind("Logging:File",_fileLoggingOptions);//.Bind(_fileLoggingOptions);
            EnableLogToFile = config.GetValue<bool>("Logging:File:Enabled");

            if (_fileLoggingOptions.LogDirectory == "" && EnableLogToFile)
            {
                var logPath = Directory.GetCurrentDirectory() + "/Logs";
                Console.WriteLine("Failed to detect log directory, writing logs to " + logPath);
                _fileLoggingOptions.LogDirectory = logPath;
            }
            else if(!EnableLogToFile)
            {
                Console.WriteLine("WARNING: Logs are not being persisted to disk, this can be configured using the setting Logging:File:Enabled");
            }
            else
            {
                Console.WriteLine("Writing logs to path " + _fileLoggingOptions.LogDirectory);
            }

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging(builder =>
                {
                    if (_fileLoggingOptions != null && EnableLogToFile)
                        builder.AddFile(options => options = _fileLoggingOptions);
                })
                .UseStartup<Startup>();
    }
}
