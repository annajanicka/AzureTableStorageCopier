using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.DataFactory;
using Microsoft.Azure.Management.DataFactory.Models;
using Microsoft.Rest.Serialization;
using Activity = Microsoft.Azure.Management.DataFactory.Models.Activity;

namespace AzureTableStorageCopier.Common
{
    public static class DataFactoryManagementClientExtensions
    {
        public static void AddAzureStorageLinkedService(
            this DataFactoryManagementClient client,
            AzureConfig config,
            string storageConnectionString,
            string storageLinkedServiceName)
        {
            Console.WriteLine($"Creating linked service {storageLinkedServiceName}...");
            var storageLinkedService = new LinkedServiceResource(
                new AzureStorageLinkedService
                {
                    ConnectionString = storageConnectionString
                }
            );
            client.LinkedServices.CreateOrUpdate(config.ResourceGroup, config.DataFactoryName, storageLinkedServiceName, storageLinkedService);
            Console.WriteLine(SafeJsonConvert.SerializeObject(storageLinkedService, client.SerializationSettings));
        }

        public static void AddAzureSqlDatabaseLinkedService(
            this DataFactoryManagementClient client,
            AzureConfig config,
            string sqlDatabaseConnectionString,
            string sqlDatabaseLinkedServiceName)
        {
            Console.WriteLine($"Creating linked service {sqlDatabaseLinkedServiceName}...");
            var storageLinkedService = new LinkedServiceResource(
                new AzureSqlDatabaseLinkedService
                {
                    ConnectionString = sqlDatabaseConnectionString
                }
            );
            client.LinkedServices.CreateOrUpdate(config.ResourceGroup, config.DataFactoryName, sqlDatabaseLinkedServiceName, storageLinkedService);
            Console.WriteLine(SafeJsonConvert.SerializeObject(storageLinkedService, client.SerializationSettings));
        }

        public static void CreateDataFactory(this DataFactoryManagementClient client, AzureConfig config)
        {
            Console.WriteLine($"Creating data factory {config.DataFactoryName}...");
            var dataFactory = new Factory
            {
                Location = config.Region,
                Identity = new FactoryIdentity()
            };
            client.Factories.CreateOrUpdate(config.ResourceGroup, config.DataFactoryName, dataFactory);
            Console.WriteLine(SafeJsonConvert.SerializeObject(dataFactory, client.SerializationSettings));

            while (client.Factories.Get(config.ResourceGroup, config.DataFactoryName).ProvisioningState == "PendingCreation")
            {
                System.Threading.Thread.Sleep(1000);
            }
        }

        public static void DeleteDataFactory(this DataFactoryManagementClient client, AzureConfig config)
        {
            Console.WriteLine($"Deleting data factory {config.DataFactoryName}...");
            client.Factories.Delete(config.ResourceGroup, config.DataFactoryName);
        }

        public static void CreateAzureTableDataset(
            this DataFactoryManagementClient client,
            AzureConfig config,
            string storageLinkedServiceName,
            string storageDatasetName,
            string tableName)
        {
            Console.WriteLine($"Creating dataset {storageDatasetName}...");
            var blobDataset = new DatasetResource(
                new AzureTableDataset
                {
                    LinkedServiceName = new LinkedServiceReference
                    {
                        ReferenceName = storageLinkedServiceName
                    },
                    TableName = tableName
                }
            );
            client.Datasets.CreateOrUpdate(config.ResourceGroup, config.DataFactoryName, storageDatasetName, blobDataset);
            Console.WriteLine(SafeJsonConvert.SerializeObject(blobDataset, client.SerializationSettings));
        }

        public static void CreateAzureSqlTableDataset(
            this DataFactoryManagementClient client,
            AzureConfig config,
            string sqlDatabaseLinkedServiceName,
            string sqlDatabaseDasetName,
            string tableName)
        {
            Console.WriteLine($"Creating dataset {sqlDatabaseDasetName}...");
            var blobDataset = new DatasetResource(
                new AzureSqlTableDataset
                {
                    LinkedServiceName = new LinkedServiceReference
                    {
                        ReferenceName = sqlDatabaseLinkedServiceName
                    },
                    TableName = tableName
                }
            );
            client.Datasets.CreateOrUpdate(config.ResourceGroup, config.DataFactoryName, sqlDatabaseDasetName, blobDataset);
            Console.WriteLine(SafeJsonConvert.SerializeObject(blobDataset, client.SerializationSettings));
        }

        public static void CreatePipeline(
            this DataFactoryManagementClient client,
            AzureConfig config,
            string pipelineName,
            Activity[] activities)
        {
            Console.WriteLine($"Creating pipeline {pipelineName}...");
            var pipeline = new PipelineResource
            {
                Activities = activities
            };
            client.Pipelines.CreateOrUpdate(config.ResourceGroup, config.DataFactoryName, pipelineName, pipeline);
            Console.WriteLine(SafeJsonConvert.SerializeObject(pipeline, client.SerializationSettings));
        }

        public static async Task<PipelineRun> CreatePipelineRunAsync(this DataFactoryManagementClient client, AzureConfig config, string pipelineName)
        {
            Console.WriteLine("Creating pipeline run...");
            var runResponse = (await client.Pipelines.CreateRunWithHttpMessagesAsync(config.ResourceGroup, config.DataFactoryName, pipelineName)).Body;
            Console.WriteLine($"Pipeline run ID: {runResponse.RunId}");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            PipelineRun pipelineRun;
            while (true)
            {
                pipelineRun = await client.PipelineRuns.GetAsync(config.ResourceGroup, config.DataFactoryName, runResponse.RunId);
                Console.WriteLine($"Status: {pipelineRun.Status}");
                if (pipelineRun.Status == "InProgress" || pipelineRun.Status == "Queued")
                {
                    System.Threading.Thread.Sleep(5000);
                }
                else
                    break;
            }
            stopwatch.Stop();
            Console.WriteLine($"Coping done. Total time: {stopwatch.Elapsed}");
            return pipelineRun;
        }

        public static void GetDetails(this DataFactoryManagementClient client, AzureConfig config, string pipelineRunId, string status)
        {
            Console.WriteLine("Checking copy activity run details...");
            var filterParams = new RunFilterParameters(DateTime.UtcNow.AddMinutes(-10), DateTime.UtcNow.AddMinutes(10));
            var queryResponse = client.ActivityRuns.QueryByPipelineRun(config.ResourceGroup, config.DataFactoryName, pipelineRunId, filterParams);
            Console.WriteLine(status == "Succeeded"
                ? queryResponse.Value.First().Output
                : queryResponse.Value.First().Error);
        }
    } 
}
