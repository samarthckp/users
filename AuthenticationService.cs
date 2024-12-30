using ERP_API.Moduls;
using ERP_API.Moduls;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
namespace UI_ERP.Reusable;

public class AuthenticationServices
{
    private readonly HttpClient _httpClient;



    public bool IsLoggedIn { get; private set; }
    public string LoggedInUserName { get; private set; }
    public int? LoggedInUserId { get; private set; }
    public int? LoggedInLocationId { get; private set; }
    public string LoggedInLocationCode { get; private set; }
    public string LoggedInLocationName { get; private set; }



    public AuthenticationServices(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> LoginAsync(int userId, string password, int? selectedLocationId)
    {
        var response = await _httpClient.PostAsJsonAsync("api/users/login", new { UserId = userId, Password = password, SelectedLocationId = selectedLocationId });

        if (response.IsSuccessStatusCode)
        {
            var user = await response.Content.ReadFromJsonAsync<UserReadOnlyDto>();

            IsLoggedIn = true;
            LoggedInUserName = user?.UserName;
            LoggedInUserId = user?.UserId;
            LoggedInLocationId = user?.LocationId;
            var locationDetails = await GetLocationDetailsAsync(LoggedInLocationId);
            //LoggedInLocationCode = locationDetails? .LocationCode;
            LoggedInLocationName = locationDetails?.Name;

            return true;
        }

        IsLoggedIn = false;
        return false;
    }



    public async Task LogoutAsync()
    {
        var response = await _httpClient.PostAsync("api/users/logout", null);

        if (response.IsSuccessStatusCode)
        {
            IsLoggedIn = false;
        }
        else
        {
            // Handle logout failure if needed
            throw new HttpRequestException("Logout failed.");
        }
    }

    public void Logout()
    {
        IsLoggedIn = false;
    }

    private async Task<LocationReadOnlyDto> GetLocationDetailsAsync(int? locationId)
    {
        if (locationId.HasValue)
        {
            var response = await _httpClient.GetAsync($"api/locations/{locationId}");

            if (response.IsSuccessStatusCode)
            {
                var location = await response.Content.ReadFromJsonAsync<LocationReadOnlyDto>();

                if (location != null)
                {
                    return location;
                }
            }
            else
            {
                // Handle failure case if needed
            }
        }

        return null; // Handle appropriately if location details fetch fails
    }
}