using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Oma.WndwCtrl.Api.IntegrationTests.TestFramework.Interfaces;

namespace Oma.WndwCtrl.Api.IntegrationTests.TestFramework;

public class ApiFixtureTestBase<TFixture> : IDisposable
    where TFixture : IApiFixture
{
    protected readonly HttpClient _httpClient;
    protected readonly CancellationToken _cancelToken;
 
    private readonly string _baseRoute;
    
    public ApiFixtureTestBase(TFixture apiFixture, string baseRoute)
    {
        _baseRoute = baseRoute;
        _cancelToken = TestContext.Current.CancellationToken;
        _httpClient = apiFixture.CreateClient();
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _httpClient.Dispose();
        }
    }

    protected HttpRequestMessage ConstructCommandHttpRequestMessage(string payload, bool isJson = true)
    {
        ArgumentNullException.ThrowIfNull(payload);

        HttpContent? json = null;
        HttpRequestMessage? httpRequestMessage = null;

        try
        {
            object? inputValue = isJson
                ? JsonSerializer.Deserialize<object>(payload)
                : payload;
            
            json = JsonContent.Create(inputValue);
            httpRequestMessage = new(HttpMethod.Post, _baseRoute);
            httpRequestMessage.Content = json;
            return httpRequestMessage;
        }
        catch
        {
            json?.Dispose();
            httpRequestMessage?.Dispose();
            throw;
        }
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}