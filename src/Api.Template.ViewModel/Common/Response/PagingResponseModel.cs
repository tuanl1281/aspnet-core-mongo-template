using Newtonsoft.Json;

namespace Gateway.ViewModel.Common.Response;

public class PagingResponseModel
{
    [JsonProperty("totalCounts")]
    public double TotalCounts { get; set; } = 0;

    [JsonProperty("data")]
    public dynamic Data { get; set; }

    public PagingResponseModel()
    {
    }

    public PagingResponseModel(object data, double totalCounts)
    {
        Data = data;
        TotalCounts = totalCounts;
    }
}

public class PagingResponseModel<T>
{
    [JsonProperty("totalCounts")]
    public double TotalCounts { get; set; } = 0;

    [JsonProperty("data")]
    public List<T> Data { get; set; } = new();
    
    public PagingResponseModel()
    {
    }

    public PagingResponseModel(List<T> data, double totalCounts)
    {
        Data = data;
        TotalCounts = totalCounts;
    }
}
