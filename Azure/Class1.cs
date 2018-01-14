using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;

namespace Azure
{
    class Class1
    {


        private async Task<CloudTable> GetTableAsync()
        {
            //var table = GetTableAsync().GetAwaiter().GetResult();


            var token = "?sv=2017-04-17&ss=t&srt=sco&sp=rwdlacu&se=2019-01-15T00:10:26Z&st=2018-01-14T16:10:26Z&spr=https&sig=ITMU4faYIrrXlvxeKdi6iKa7bcjPwsDc6I0q8m1TVUM%3D";
            //Account
            CloudStorageAccount storageAccount = new CloudStorageAccount(
                new StorageCredentials(token), true);

            //Client
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            //Table
            CloudTable table = tableClient.GetTableReference("trailer");
            await table.CreateIfNotExistsAsync();

            return table;
        }
    }
}
