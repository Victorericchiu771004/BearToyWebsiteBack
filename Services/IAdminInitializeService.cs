namespace BearToyWebsiteBack.Services
{
    public interface IAdminInitializeService
    {
        Task InitializeAsync();
        Task SeedDefaultDataAsync();
    }
}