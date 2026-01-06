namespace Spc.Client.Services;

public sealed class ApiError
{
    public string? Error { get; set; }
    public int Status { get; set; }
    public string? Path { get; set; }
}

public sealed class ApiException : Exception
{
    public int StatusCode { get; }

    public ApiException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }
}
