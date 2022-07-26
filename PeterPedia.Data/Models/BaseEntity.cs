using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace PeterPedia.Data.Models;

public class BaseEntity
{
    private PartitionKey? _key;

    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; } = string.Empty;

    [JsonIgnore]
    public PartitionKey PartitionKey
    {
        get
        {
            if (_key is null)
            {
                _key = new PartitionKey(Id);
            }

            return _key.Value;
        }
    }
}
