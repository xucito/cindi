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
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Cindi.Presentation
{
    public class Program
    {
        public static FileLoggerOptions _fileLoggingOptions { get; set; }
        public static bool EnableLogToFile { get; set; }
        public static IConfiguration configuration;

        public static void Main(string[] args)
        { 
            var host = CreateWebHostBuilder(args).Build();
            IWebHostEnvironment env = host.Services.GetRequiredService<IWebHostEnvironment>();

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddCommandLine(args)
                .AddEnvironmentVariables()
                .Build();

            configuration = config;

            _fileLoggingOptions = config.GetSection("Logging:File").Get<FileLoggerOptions>();
            // config.Bind("Logging:File",_fileLoggingOptions);//.Bind(_fileLoggingOptions);
            EnableLogToFile = config.GetValue<bool>("Logging:File:Enabled");

            if (_fileLoggingOptions.LogDirectory == "" && EnableLogToFile)
            {
                var logPath = Directory.GetCurrentDirectory() + "\\test";
                Console.WriteLine("Failed to detect log directory, writing logs to " + logPath);
                _fileLoggingOptions.LogDirectory = logPath;
            }
            else if(!EnableLogToFile)
            {
                Console.WriteLine("WARNING: Logs are not being persisted to disk, this can be configured using the setting Logging:File:Enabled");
            }

            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureKestrel(serverOptions =>
                {
                        serverOptions.Configure(configuration.GetSection("Kestrel"));
                })
                .ConfigureServices(services => services.AddAutofac())
                .ConfigureLogging(builder =>
                {
                    if (_fileLoggingOptions != null && EnableLogToFile)
                    {
                        Console.WriteLine("Writing logs to path " + _fileLoggingOptions.LogDirectory);
                        builder.AddFile(options => {
                            options.FileSizeLimit = _fileLoggingOptions.FileSizeLimit;
                            options.RetainedFileCountLimit = _fileLoggingOptions.RetainedFileCountLimit;
                            options.FileName = _fileLoggingOptions.FileName;
                            options.Extension = _fileLoggingOptions.Extension;
                            options.Periodicity = _fileLoggingOptions.Periodicity;
                            options.LogDirectory = _fileLoggingOptions.LogDirectory;
                        });
                    }
                })
                .UseStartup<Startup>();
    }
}
