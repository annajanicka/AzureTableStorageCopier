using System;
using System.Collections.Generic;
using Microsoft.Azure.Management.DataFactory.Models;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using AzureTableStorageCopier.Common;

namespace AzureTableStorageCopier.ToSQL
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
            var sourceStorageConnectionString = configurationRoot.GetConnectionString("SourceStorage");
            var targetDatabaseConnectionString = configurationRoot.GetConnectionString("TargetDatabase");
            var tableName = "UserTable"; // name of your table storage table

            // name of the Azure Storage linked service, blob dataset, and the pipeline
            var sourceStorageLinkedServiceName = "SourceAzureStorageLinkedService";
            var targetDatabaseLinkedServiceName = "TargetAzureSqlDatabaseLinkedService";

            var pipelineName = "CopyATSPipeline";

            // Authenticate and create a data factory management client
            using var client = await new DataFactoryManagementClientProvider(config).GetClient();
            client.CreateDataFactory(config);

            // Create an Azure Storage linked service for target
            client.AddAzureSqlDatabaseLinkedService(config, targetDatabaseConnectionString, targetDatabaseLinkedServiceName);

            // Create an Azure Storage linked service for source
            client.AddAzureStorageLinkedService(config, sourceStorageConnectionString, sourceStorageLinkedServiceName);

            var activities = new List<Activity>();

            var sourceStorageDatasetName = $"{tableName}-source";
            var targetDatasetName = $"{tableName}-target";

            // Create an Azure Table Storage dataset - this has to match your data type created in script 001_CreateUserType.sql
            client.CreateAzureTableDataset(config, sourceStorageLinkedServiceName, sourceStorageDatasetName, tableName);
            // Create an Azure SQL Table dataset
            client.CreateAzureSqlTableDataset(config, targetDatabaseLinkedServiceName, targetDatasetName, tableName);

            activities.Add(GetTableStorageCopyActivity(sourceStorageDatasetName, targetDatasetName, tableName));

            // Create a pipeline with a copy activity
            client.CreatePipeline(config, pipelineName, activities.ToArray());

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
                Source = new AzureTableSource() ,
                Sink = new AzureSqlSink
                {
                    SqlWriterStoredProcedureName = "spUpsetrUser", // stored procedure defined in 003_CreateUpsertUsers.sql
                    StoredProcedureTableTypeParameterName = "User",
                    SqlWriterTableType = "UserType", // UserType defined in 001_CreateUserType.sql,
                   
                }
            };
        }
    }
}
