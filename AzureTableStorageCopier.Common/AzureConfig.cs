namespace AzureTableStorageCopier.Common
{
    public class AzureConfig
    {
        public string TenantId { get; set; }
        public string ApplicationId { get; set; }
        public string AuthenticationKey { get; set; }
        public string SubscriptionId { get; set; }
        public string ResourceGroup { get; set; }
        public string Region { get; set; }
        public string DataFactoryName { get; set; }
    }
}
