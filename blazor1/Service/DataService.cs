using blazor1.Model;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Net.Http.Json;

namespace blazor1.Service;

public class DataService : IDataService
{
    HttpClient Client { get; init; }
    CredentialService CredService { get; init; }

    public DataService(IHttpClientFactory httpfact, CredentialService credSvc)
    {
        Client = httpfact.CreateClient("Api");
        CredService = credSvc;
    }
    private async Task UpdateHttpClient()
    {
        var cognitoData = await CredService.GetCognitoCredentials();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", cognitoData.IdToken);
    }

    public async Task<DataItemModel?> GetItem(string itemId)
    {
        await UpdateHttpClient();
        var resp = (await Client.GetAsync($"/DataItem").ConfigureAwait(false)).EnsureSuccessStatusCode();
        var data = await resp.Content.ReadFromJsonAsync<IEnumerable<DataItemModel>>();
        return data?.FirstOrDefault();
    }

    public async Task<DataItemModel> SaveItem(DataItemModel item)
    {
        await UpdateHttpClient();
        var resp = (await Client.PostAsJsonAsync($"/DataItem", item).ConfigureAwait(false)).EnsureSuccessStatusCode();
        var data = await resp.Content.ReadFromJsonAsync<IEnumerable<DataItemModel>>();
        return item;
    }
}
