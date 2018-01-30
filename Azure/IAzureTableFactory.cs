using Microsoft.WindowsAzure.Storage.Table;

namespace Azure
{
    public interface IAzureTableFactory
    {
        CloudTable GetTable(string tableName);
    }
}