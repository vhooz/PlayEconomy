using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Play.Inventory.Service.Dtos;

namespace Play.Inventory.Service
{
    public class CatalogClient
    {
        /**
        * For this client to comunicate with other http endpoint,
        * We'll use the HttpClient Class.
        **/

        private readonly HttpClient httpClient;

        //dependency injection
        public CatalogClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        //IReadOnlyCollection: used since we are not going to modify anything. 
        public async Task<IReadOnlyCollection<CatalogItemDto>> GetCatalogItemsAsync()
        {
            var items = await httpClient.GetFromJsonAsync<IReadOnlyCollection<CatalogItemDto>>("/items");
            return items;
        }
    }
}