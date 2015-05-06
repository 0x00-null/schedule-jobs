using MigrateMPData.Models;
using System.Collections.Generic;
using System.IO;

namespace MigrateMPData.Interfaces
{
    public interface IMinistryPlatformTableConfigReader
    {
        List<MinistryPlatformTable> readConfig(string fileName);

        List<MinistryPlatformTable> readConfig(Stream fileStream);
    }
}
