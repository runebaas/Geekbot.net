﻿using System;
using System.Text;
using NLog;
using NLog.Config;
using NLog.Targets;
using SumoLogic.Logging.NLog;

namespace Geekbot.net.Lib
{
    public class LoggerFactory
    {
        public static Logger CreateNLog(bool sumologicActive)
        {
            var config = new LoggingConfiguration();

            if (sumologicActive)
            {
                Console.WriteLine("Logging Geekbot Logs to Sumologic");
                config.LoggingRules.Add(
                    new LoggingRule("*", LogLevel.Info, LogLevel.Fatal, 
                        new BufferedSumoLogicTarget()
                        {
                            Url = Environment.GetEnvironmentVariable("GEEKBOT_SUMO"),
                            SourceName = "GeekbotLogger",
                            Layout = "${message}",
                            UseConsoleLog = false,
                            MaxQueueSizeBytes = 500000,
                            FlushingAccuracy = 250,
                            MaxFlushInterval = 10000,
                            OptimizeBufferReuse = true,
                            MessagesPerRequest = 10,
                            RetryInterval = 5000,
                            Name = "Geekbot"
                        })
                    );
            }
            else
            {
                config.LoggingRules.Add(
                    new LoggingRule("*", LogLevel.Trace, LogLevel.Fatal,
                        new ColoredConsoleTarget
                        {
                            Name = "Console",
                            Encoding = Encoding.Unicode,
                            Layout = "[${longdate} ${level:format=FirstCharacter}] ${message} ${exception:format=toString}"
                        })
                    );
                
                config.LoggingRules.Add(
                    new LoggingRule("*", LogLevel.Trace, LogLevel.Fatal,
                        new FileTarget
                        {
                            Name = "File",
                            Layout = "[${longdate} ${level}] ${message}",
                            Encoding = Encoding.Unicode,
                            LineEnding = LineEndingMode.Default,
                            MaxArchiveFiles = 30,
                            ArchiveNumbering = ArchiveNumberingMode.Date,
                            ArchiveEvery = FileArchivePeriod.Day,
                            ArchiveFileName = "./Logs/Archive/{#####}.log",
                            FileName = "./Logs/Geekbot.log"
                        })
                    );
            }
            
            var loggerConfig = new LogFactory { Configuration = config };
            return loggerConfig.GetCurrentClassLogger();
        }
    }
}