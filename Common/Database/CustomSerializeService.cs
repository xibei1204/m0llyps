using Newtonsoft.Json;
using SqlSugar;

namespace MollyPS.Database;

public class CustomSerializeService : ISerializeService
{
    public string SerializeObject(object value)
    {
        return JsonConvert.SerializeObject(value);
    }

    public T DeserializeObject<T>(string value)
    {
        return JsonConvert.DeserializeObject<T>(value)!;
    }

    public string SugarSerializeObject(object value)
    {
        return JsonConvert.SerializeObject(value);
    }
}
