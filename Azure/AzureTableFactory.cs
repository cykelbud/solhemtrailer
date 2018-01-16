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
            //var table = GetTableAsync().GetAwaiter().GetResult();


            //Account
            CloudStorageAccount storageAccount =
                //CloudStorageAccount.DevelopmentStorageAccount;
                new CloudStorageAccount(new StorageCredentials("solhemtrailer",
                    "LqzKFHhs0z4TqoRBvru7Q4gyl/V90A6ZV0+6il69U0siZGaomqtTadPOcB29UPTiiT2SdQYUpDgQ4XbyYqcY7A=="), true);
                
                //CloudStorageAccount.Parse(
                //    "DefaultEndpointsProtocol=https;AccountName=solhemtrailer;AccountKey=LqzKFHhs0z4TqoRBvru7Q4gyl/V90A6ZV0+6il69U0siZGaomqtTadPOcB29UPTiiT2SdQYUpDgQ4XbyYqcY7A==;EndpointSuffix=core.windows.net");


            //new CloudStorageAccount(
            //new StorageCredentials(token), true);

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
