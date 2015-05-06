using log4net;
using Microsoft.VisualBasic.FileIO;
using MigrateMPData.Interfaces;
using MigrateMPData.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace MigrateMPData
{
    public class MinistryPlatformTableConfigReader : IMinistryPlatformTableConfigReader
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public List<MinistryPlatformTable> readConfig(string fileName)
        {
            try
            {
            var fileStream = new BufferedStream(File.Open(fileName, FileMode.Open));
            var ret = readConfig(fileStream);
            fileStream.Close();
            return (ret);
            }
            catch (IOException e)
            {
                logger.Error("Could not read input file " + fileName, e);
                throw (e);
            }
        }

        public List<MinistryPlatformTable> readConfig(Stream fileStream)
        {
            List<MinistryPlatformTable> tables = new List<MinistryPlatformTable>();
            using (TextFieldParser parser = new TextFieldParser(fileStream))
            {
                parser.Delimiters = new string[] { "," };
                parser.CommentTokens = new string[] { "#", "--", "//", "'" };
                parser.TextFieldType = FieldType.Delimited;
                parser.HasFieldsEnclosedInQuotes = true;
                parser.TrimWhiteSpace = true;

                string[] fields;
                while(!parser.EndOfData) {
                    fields = parser.ReadFields();

                    var t = new MinistryPlatformTable
                    {
                        tableName = fields[0],
                        migrationType = fields[1].Length > 0 ? (MigrationType)Enum.Parse(typeof(MigrationType), fields[1]) : MigrationType.INSERT_OR_UPDATE,
                        filterClause = fields[2].Length > 0 ? fields[2] : null,
                    };
                    tables.Add(t);
                }
            }

            return (tables);
        }
    }
}
