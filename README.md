# AzureTableStorageCopier

Some sweet code that will copy tables from one Azure Table storage to another Table Storage or SQL database.

If you want to learn more visit

- https://docs.microsoft.com/en-us/azure/data-factory/connector-sql-server

## How to run?

1. Create a source Azure Storage account and add a table with the structure as per file 001_CreateUserType.sql. Remember that order of the columns matters, it seems like it's identical to Azure Storate Explorer order: Partition Key + RowKey + Timestamp + other columns alphabetically.
2. Create a target storage account OR Azure SQL database (it depends what scenario you want to run, there are two console apps in the solution)
3. [Create an application in Azure Active Directory](https://docs.microsoft.com/en-us/azure/data-factory/quickstart-create-data-factory-dot-net#create-an-application-in-azure-active-directory) - configure applicationId and authenticationKey in appsettings.json
4. Configure other properies in appsettings.json (if you want to migrate to SQL, remember about appsettings.json in DbUp project)
5. If you migrate to SQL run DbUp project

## Contains

- [x] .NETCore 3.1
- [x] Microsoft.Azure.Management.DataFactory 4.8.0
- [x] Microsoft.Azure.Management.ResourceManager 3.4.0-preview

## License

MIT
