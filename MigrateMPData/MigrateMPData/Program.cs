using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using CommandLine;
using CommandLine.Text;
using MigrateMPData.Models;
using System.ComponentModel;
using Microsoft.Practices.Unity;
using MigrateMPData.Interfaces;
using System.IO;
using Microsoft.Practices.Unity.Configuration;

[assembly: XmlConfigurator(Watch = true)]
namespace MigrateMPData
{
    public class Program
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IMinistryPlatformTableConfigReader configReader { set; get; }
        private IMinistryPlatformDataMover dataMover {set; get;}

        public static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            var section = (UnityConfigurationSection)ConfigurationManager.GetSection("unity");
            IUnityContainer container = new UnityContainer();
            section.Configure(container);

            var options = new Options();
            if (!CommandLine.Parser.Default.ParseArguments(args, options))
            {
                logger.Error("Invalid Arguments.");
                logger.Error(options.GetUsage());
                Environment.Exit(1);
            }

            Program program = container.Resolve<Program>();
            logger.Info("Starting data migration");
            program.Run(options);
        }

        public Program(IMinistryPlatformTableConfigReader configReader, IMinistryPlatformDataMover dataMover)
        {
            this.configReader = configReader;
            this.dataMover = dataMover;
        }

        public void Run(Options options) {
            logger.Info("Beginning Data Migration using input file " + options.InputFile);
            List<MinistryPlatformTable> tables = configReader.readConfig(options.InputFile);
            logger.Debug("Tables to migrate: " + string.Join<MinistryPlatformTable>(",\r\n", tables.ToArray()));
            foreach (var table in tables)
            {
                logger.Info("Migrating table: " + table.tableName);
                var success = dataMover.moveData(table, options.ExecuteMode);
                if (success)
                {
                    logger.Info("Successfully migrated table: " + table.tableName);
                }
                else
                {
                    logger.Error("Failed to migrate table: " + table.tableName);
                }
            }
        }
    }

    public class Options
    {
        [Option('f', "file", Required = true,
          HelpText = "Input file to be processed.")]
        public string InputFile { get; set; }

        [Option('x', "execute", Required = false, DefaultValue = false,
          HelpText = "Execute mode - by default will run in 'test' mode")]
        public bool ExecuteMode { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
