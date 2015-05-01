using MigrateMPData.Models;

namespace MigrateMPData.Interfaces
{
    public interface IMinistryPlatformDataMover
    {
        bool moveData(MinistryPlatformTable table, bool execute);
    }
}
