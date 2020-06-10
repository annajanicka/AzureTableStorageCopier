using Microsoft.Azure.Management.DataFactory;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using System.Threading.Tasks;

namespace AzureTableStorageCopier.Common
{
    public class DataFactoryManagementClientProvider
    {
        private readonly AzureConfig _azureConfig;

        public DataFactoryManagementClientProvider(AzureConfig config)
        {
            _azureConfig = config;
        }

        public async Task<DataFactoryManagementClient> GetClient()
        {
            var context = new AuthenticationContext($"https://login.windows.net/{_azureConfig.TenantId}");
            var cc = new ClientCredential(_azureConfig.ApplicationId, _azureConfig.AuthenticationKey);
            var result = await context.AcquireTokenAsync("https://management.azure.com/", cc);
            var client = new DataFactoryManagementClient(new TokenCredentials(result.AccessToken))
            {
                SubscriptionId = _azureConfig.SubscriptionId
            };

            return client;
        }
    }
}
