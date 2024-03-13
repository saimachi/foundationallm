namespace FoundationaLLM.Authorization.Models.Configuration
{
    public record AuthorizationServiceSettings
    {
        public required string APIUrl { get; set; }

        public required string APIScope { get; set; }
    }
}
