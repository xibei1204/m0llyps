using SqlSugar;

namespace MollyPS.Database;

public abstract class BaseDatabaseDataHelper
{
    [SugarColumn(IsPrimaryKey = true)] public int Uid { get; set; }
}
