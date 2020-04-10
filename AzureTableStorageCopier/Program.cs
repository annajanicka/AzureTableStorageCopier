using System;
using System.Collections.Generic;
using Microsoft.Rest;
using Microsoft.Azure.Management.DataFactory;
using Microsoft.Azure.Management.DataFactory.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace AzureTableStorageCopier
{
    class Program
    {
        static async Task Main()
        {
            var configurationRoot = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json")
                    .AddJsonFile("appsettings.local.json", true)
                    .Build();

            var config = configurationRoot.GetSection("Azure").Get<AzureConfig>();
            var tables = configurationRoot.GetSection("tables").Get<string>().Split(",");
            var sourceStorageConnectionString = configurationRoot.GetConnectionString("SourceStorage");
            var targetStorageConnectionString = configurationRoot.GetConnectionString("TargetStorage");

            // name of the Azure Storage linked service, blob dataset, and the pipeline
            var sourceStorageLinkedServiceName = "SourceAzureStorageLinkedService";
            var targetStorageLinkedServiceName = "TargetAzureStorageLinkedService";

            var pipelineName = "CopyATSPipeline";

            // Authenticate and create a data factory management client
            using var client = await GetClient(config);
            client.CreateDataFactory(config);

            // Create an Azure Storage linked service for target
            client.AddAzureStorageLinkedService(config, targetStorageConnectionString, targetStorageLinkedServiceName);

            // Create an Azure Storage linked service for source
            client.AddAzureStorageLinkedService(config, sourceStorageConnectionString, sourceStorageLinkedServiceName);

            var activities = new List<Activity>();
            foreach (var tableName in tables)
            {
                var sourceStorageDatasetName = $"{tableName}-source";
                var targetDatasetName = $"{tableName}-target";

                // Create an Azure Blob datasets
                client.CreateAzureTableDataset(config, sourceStorageLinkedServiceName, sourceStorageDatasetName, tableName);
                client.CreateAzureTableDataset(config, targetStorageLinkedServiceName, targetDatasetName, tableName);

                activities.Add(GetTableStorageCopyActivity(sourceStorageDatasetName, targetDatasetName, tableName));
            }

            // Create a pipeline with a copy activity
            client.CreatePipline(config, pipelineName, activities.ToArray());

            // Create a pipeline run
            var pipelineRun = await client.CreatePipelineRunAsync(config, pipelineName);

            // Check the copy activity run details
            client.GetDetails(config, pipelineRun.RunId, pipelineRun.Status);

            client.DeleteDataFactory(config);

            Console.WriteLine("All done!");
        }

        private static Activity GetTableStorageCopyActivity(string sourceStorageDatasetName, string targetStorageDatasetName, string tableName)
        {
            return new CopyActivity
            {
                Name = $"Copy-{tableName}",
                Inputs = new List<DatasetReference>
                {
                    new DatasetReference
                    {
                        ReferenceName = sourceStorageDatasetName
                    }
                },
                Outputs = new List<DatasetReference>
                {
                    new DatasetReference
                    {
                        ReferenceName = targetStorageDatasetName
                    }
                },
                Source = new AzureTableSource(),
                Sink = new AzureTableSink { AzureTableRowKeyName = "RowKey", AzureTablePartitionKeyName = "PartitionKey" }
            };
        }

        private static async Task<DataFactoryManagementClient> GetClient(AzureConfig config)
        {
            var context = new AuthenticationContext("https://login.windows.net/" + config.TenantId);
            var cc = new ClientCredential(config.ApplicationId, config.AuthenticationKey);
            var result = await context.AcquireTokenAsync("https://management.azure.com/", cc);
            var client = new DataFactoryManagementClient(new TokenCredentials(result.AccessToken))
            {
                SubscriptionId = config.SubscriptionId
            };

            return client;
        }
    }
}
