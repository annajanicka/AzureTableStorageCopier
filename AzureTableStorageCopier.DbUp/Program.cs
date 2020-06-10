using DbUp;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;

namespace AzureTableStorageCopier.DbUp
{
    class Program
    {
        static void Main()
        {
            var config = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json")
                    .AddJsonFile("appsettings.local.json", true)
                    .Build();

            var connectionString = config.GetConnectionString("TargetDatabase");

            var upgrader = DeployChanges.To
                .SqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .WithTransactionPerScript()
                .Build();

            EnsureDatabase.For.SqlDatabase(connectionString);

            var result = upgrader.PerformUpgrade();

            if (result.Successful)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Success!");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("An error has occured, please see the details above.");
            }

            Console.ResetColor();
        }
    }
}
