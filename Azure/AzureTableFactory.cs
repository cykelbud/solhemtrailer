using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;

namespace Azure
{
    public class AzureTableFactory : IAzureTableFactory
    {
        private async Task<CloudTable> GetTableAsync()
        {
            //Account
            CloudStorageAccount storageAccount =
                new CloudStorageAccount(new StorageCredentials("solhemtrailer",
                    "LqzKFHhs0z4TqoRBvru7Q4gyl/V90A6ZV0+6il69U0siZGaomqtTadPOcB29UPTiiT2SdQYUpDgQ4XbyYqcY7A=="), true);
                
            //Client
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            //Table
            CloudTable table = tableClient.GetTableReference("bookings");
            await table.CreateIfNotExistsAsync();
            
            return table;
        }

        public CloudTable GetTable()
        {
            return GetTableAsync().GetAwaiter().GetResult();
        }


    }
}
