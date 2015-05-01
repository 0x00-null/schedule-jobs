using MigrateMPData.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrateMPData.Interfaces
{
    public interface IMinistryPlatformTableConfigReader
    {
        List<MinistryPlatformTable> readConfig(string fileName);

        List<MinistryPlatformTable> readConfig(Stream fileStream);
    }
}
