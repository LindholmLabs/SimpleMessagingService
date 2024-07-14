using Shared.Contracts;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;

namespace Client.Utilities;

public interface IApiInterface
{
    Task<GetMessagesContract?> GetMessagesAsync();
    Task<GetUsersContract?> GetUsersAsync();
    Task<HttpResponseMessage?> PostMessageAsync(string message);
    Task<HttpResponseMessage?> PostUserAsync(string name);
    Guid ServerKey { get; set; }
}

public class ApiInterface : IApiInterface
{
    private readonly HttpClient _httpClient;
    public Guid ServerKey { get; set; } = Guid.Empty;

    public ApiInterface(Uri baseUrl)
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = baseUrl;
    }

    public async Task<GetMessagesContract?> GetMessagesAsync()
    { 
        return await getContract<GetMessagesContract>(Shared.Endpoints.Messages);
    }

    public async Task<GetUsersContract?> GetUsersAsync()
    {
        return await getContract<GetUsersContract>(Shared.Endpoints.Users);
    }

    public async Task<HttpResponseMessage?> PostMessageAsync(string message)
    {
        var messageContract = new MessageContract { Content = message, Key = ServerKey };
        return await postContract(messageContract, Shared.Endpoints.Messages);
    }

    public async Task<HttpResponseMessage?> PostUserAsync(string name)
    {
        var userContract = new UserContract { Username = name };
        return await postContract(userContract, Shared.Endpoints.Users);
    }

    private async Task<HttpResponseMessage?> postContract<T>(T content, string endpoint)
    {
        try
        {
            var json = JsonSerializer.Serialize(content);
            HttpContent httpContent = new StringContent(json);
            httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var response = await _httpClient.PostAsync(endpoint, httpContent);
            Debug.WriteLine(await response.Content.ReadAsStringAsync());
            return response;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return default;
        }
    }

    private async Task<T?> getContract<T>(string endpoint)
    {
        int attempts = 0;
        do
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                var responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine(responseContent);
                return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception e)
            {
                attempts++;
                Debug.WriteLine($"Failed after {attempts} tries.\n{e.Message}");
            }
        }
        while (attempts < 10);

        return default;
    }
}