using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Networking;
using SmartNavigationMAD.Data;
using SmartNavigationMAD.Models;

namespace SmartNavigationMAD;

public partial class MainPage : ContentPage
{
    private readonly TripDatabase _db;
    private Location? _currentLocation;
    private bool _tripIdHelpVisible;
    private bool _savedTripsHelpVisible;

    public MainPage(TripDatabase db)
    {
        InitializeComponent();
        _db = db;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _db.InitializeAsync();

        if (await EnsureLocationPermissionAsync())
        {
            await LoadLocationAndConnectivity();
        }
        else
        {
            SetLocationUnavailableState("Location permission is required to track trips");
        }

        await LoadTripsAsync();
    }

    private static async Task<bool> EnsureLocationPermissionAsync()
    {
        var platform = DeviceInfo.Platform;
        if (platform != DevicePlatform.Android &&
            platform != DevicePlatform.iOS &&
            platform != DevicePlatform.MacCatalyst)
        {
            return true;
        }

        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status == PermissionStatus.Granted)
        {
            return true;
        }

        status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        return status == PermissionStatus.Granted;
    }

    private async Task LoadLocationAndConnectivity()
    {
        try
        {
            _currentLocation = await Geolocation.GetLocationAsync(
                new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10)));

            if (_currentLocation == null)
            {
                SetLocationUnavailableState("Current location is unavailable");
                return;
            }

            LatitudeLabel.Text = _currentLocation.Latitude.ToString("F6");
            LongitudeLabel.Text = _currentLocation.Longitude.ToString("F6");
            NetworkLabel.Text = Connectivity.Current.NetworkAccess.ToString();
            TimeLabel.Text = DateTime.Now.ToString("dd MMM yyyy HH:mm:ss");
            StatusLabel.Text = "Live trip tracking active";
        }
        catch
        {
            SetLocationUnavailableState("Unable to retrieve location");
        }
    }

    private async void SaveTrip_Clicked(object sender, EventArgs e)
    {
        if (!ValidateTripId(TripEntry.Text, out var normalizedTripId))
        {
            return;
        }

        if (_currentLocation == null)
        {
            TripErrorLabel.Text = "Refresh until a location is available before saving";
            TripErrorLabel.IsVisible = true;
            return;
        }

        await _db.SaveTripAsync(new TripLog
        {
            TripId = normalizedTripId,
            Location = $"{_currentLocation.Latitude:F6}, {_currentLocation.Longitude:F6}"
        });

        TripEntry.Text = string.Empty;
        TripErrorLabel.IsVisible = false;

        await LoadTripsAsync();
        StatusLabel.Text = "Trip saved";
    }

    private async void SaveButton_Pressed(object sender, EventArgs e)
    {
        if (sender is Button btn)
        {
            await btn.ScaleTo(0.96, 80, Easing.CubicOut);
        }
    }

    private async void SaveButton_Released(object sender, EventArgs e)
    {
        if (sender is Button btn)
        {
            await btn.ScaleTo(1, 80, Easing.CubicIn);
        }
    }

    private async void DeleteTrip_Clicked(object sender, EventArgs e)
    {
        if (sender is ImageButton btn && btn.CommandParameter is TripLog trip)
        {
            if (btn.Parent?.Parent is Border border)
            {
                await border.TranslateTo(120, 0, 200, Easing.CubicIn);
            }

            await _db.DeleteTripAsync(trip);
            await LoadTripsAsync();
            StatusLabel.Text = "Trip deleted";
        }
    }

    private async void DeleteButton_Pressed(object sender, EventArgs e)
    {
        if (sender is ImageButton btn)
        {
            await btn.ScaleTo(0.85, 80, Easing.CubicOut);
        }
    }

    private async void DeleteButton_Released(object sender, EventArgs e)
    {
        if (sender is ImageButton btn)
        {
            await btn.ScaleTo(1, 80, Easing.CubicIn);
        }
    }

    private async void TripItem_Loaded(object sender, EventArgs e)
    {
        if (sender is Border border)
        {
            await border.TranslateTo(0, -4, 120, Easing.CubicOut);
            await border.TranslateTo(0, 0, 120, Easing.CubicIn);
        }
    }

    private async void OnRefreshRequested(object sender, EventArgs e)
    {
        if (await EnsureLocationPermissionAsync())
        {
            await LoadLocationAndConnectivity();
        }
        else
        {
            SetLocationUnavailableState("Location permission is required to track trips");
        }

        await LoadTripsAsync();
        PageRefreshView.IsRefreshing = false;
    }

    private bool ValidateTripId(string? tripId, out string normalizedTripId)
    {
        normalizedTripId = string.Empty;
        TripErrorLabel.IsVisible = false;

        var candidate = tripId?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(candidate))
        {
            TripErrorLabel.Text = "Trip ID cannot be empty";
            TripErrorLabel.IsVisible = true;
            return false;
        }

        if (candidate.Length is < 3 or > 20)
        {
            TripErrorLabel.Text = "Trip ID must be 3 to 20 characters";
            TripErrorLabel.IsVisible = true;
            return false;
        }

        if (!candidate.All(char.IsLetterOrDigit))
        {
            TripErrorLabel.Text = "Trip ID must use letters and numbers only";
            TripErrorLabel.IsVisible = true;
            return false;
        }

        normalizedTripId = candidate;
        return true;
    }

    private async Task LoadTripsAsync()
    {
        TripsView.ItemsSource = await _db.GetTripsAsync();
    }

    private async void ToggleTripIdHelp(object sender, EventArgs e)
    {
        _tripIdHelpVisible = !_tripIdHelpVisible;

        if (_tripIdHelpVisible)
        {
            TripIdHelp.IsVisible = true;
            TripIdHelp.Opacity = 0;
            await TripIdHelp.FadeTo(1, 150);
        }
        else
        {
            await TripIdHelp.FadeTo(0, 120);
            TripIdHelp.IsVisible = false;
        }
    }

    private async void ToggleSavedTripsHelp(object sender, EventArgs e)
    {
        _savedTripsHelpVisible = !_savedTripsHelpVisible;

        if (_savedTripsHelpVisible)
        {
            SavedTripsHelp.IsVisible = true;
            SavedTripsHelp.Opacity = 0;
            await SavedTripsHelp.FadeTo(1, 150);
        }
        else
        {
            await SavedTripsHelp.FadeTo(0, 120);
            SavedTripsHelp.IsVisible = false;
        }
    }

    private void SetLocationUnavailableState(string message)
    {
        _currentLocation = null;
        LatitudeLabel.Text = "--";
        LongitudeLabel.Text = "--";
        NetworkLabel.Text = Connectivity.Current.NetworkAccess.ToString();
        TimeLabel.Text = "--";
        StatusLabel.Text = message;
    }
}
