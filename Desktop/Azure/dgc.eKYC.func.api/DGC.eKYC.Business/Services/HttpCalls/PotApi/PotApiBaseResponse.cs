namespace DGC.eKYC.Business.Services.HttpCalls.PotApi;

public class PotApiBaseResponse<T>
{
    public int StatusCode { get; set; }

    public T? Data { get; set; }

    public object? Errors { get; set; }

    public bool Succeeded { get; set; }

    public object? Extras { get; set; }

    public int Timestamp { get; set; }
}