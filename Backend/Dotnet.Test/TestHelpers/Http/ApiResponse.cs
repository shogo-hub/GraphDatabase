using System.Net;
using System.Net.Http.Headers;

namespace Backend.Dotnet.Tests.TestHelpers.Http;

public sealed class ApiResponse<TValue>
{
    private readonly string _content;
    private readonly Func<string, TValue> _deserialize;

    public ApiResponse(
        HttpStatusCode status,
        string content,
        HttpResponseHeaders headers,
        Func<string, TValue> deserialize)
    {
        Status = status;
        _content = content;
        Headers = headers;
        _deserialize = deserialize;
    }

    public HttpStatusCode Status { get; }

    public HttpResponseHeaders Headers { get; }

    public TValue GetValueOrThrow()
    {
        if ((int)Status < 200 || (int)Status >= 400)
        {
            throw new InvalidOperationException($"Response is {Status}: {_content}");
        }

        return _deserialize(_content);
    }
}