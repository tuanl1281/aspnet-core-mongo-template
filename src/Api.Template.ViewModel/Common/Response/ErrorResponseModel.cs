using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Gateway.ViewModel.Common.Response;

public class Error
{
    [JsonProperty("code")]
    [JsonPropertyName("code")]
    public string Code { get; set; }
    
    [JsonProperty("message")]
    [JsonPropertyName("message")]
    public string Message { get; set; }
    
    [JsonProperty("details")]
    [JsonPropertyName("details")]
    public dynamic Details { get; set; }
}

public class ErrorResponseModel
{
    [JsonProperty("succeed")]
    [JsonPropertyName("succeed")]
    public bool Succeed { get; set; }

    [JsonProperty("error")]
    [JsonPropertyName("error")]
    public Error Error { get; set; }

    [JsonProperty("data")]
    [JsonPropertyName("data")]
    public dynamic Data { get; set; }

    public ErrorResponseModel()
    {
    }

    public ErrorResponseModel(object data)
    {
        Data = data;
    }
    
    public ErrorResponseModel(dynamic data, Error error)
    {
        Data = data;
        Error = error;
    }
        
    public override string ToString() => System.Text.Json.JsonSerializer.Serialize(this);
}

public class ErrorResponseModel<T>
{
    [JsonProperty("succeed")]
    [JsonPropertyName("succeed")]
    public bool Succeed { get; set; }
    
    [JsonProperty("error")]
    [JsonPropertyName("error")]
    public Error Error { get; set; }

    [JsonProperty("data")]
    [JsonPropertyName("data")]
    public T Data { get; set; }

    public ErrorResponseModel()
    {
    }

    public ErrorResponseModel(T data)
    {
        Data = data;
    }
    
    public ErrorResponseModel(T data, Error error)
    {
        Data = data;
        Error = error;
    }
        
    public override string ToString() => System.Text.Json.JsonSerializer.Serialize(this);
}