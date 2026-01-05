using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Networking;
using SmartNavigationMAD.Data;
using SmartNavigationMAD.Models;

namespace SmartNavigationMAD;

public partial class MainPage : ContentPage
{
    private readonly TripDatabase _db;

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

        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        await LoadLocationAndConnectivity();
        await LoadTripsAsync();
    }

    // 🔄 Load current trip status
    private async Task LoadLocationAndConnectivity()
    {
        try
        {
            var location = await Geolocation.GetLocationAsync(
                new GeolocationRequest(GeolocationAccuracy.Medium));

            if (location != null)
            {
                LatitudeLabel.Text = location.Latitude.ToString("F6");
                LongitudeLabel.Text = location.Longitude.ToString("F6");
            }

            NetworkLabel.Text = Connectivity.Current.NetworkAccess.ToString();
            TimeLabel.Text = DateTime.Now.ToString("dd MMM yyyy HH:mm:ss");
            StatusLabel.Text = "Live trip tracking active";
        }
        catch
        {
            StatusLabel.Text = "Unable to retrieve location";
        }
    }

    // 💾 Save trip
    private async void SaveTrip_Clicked(object sender, EventArgs e)
    {
        if (!ValidateTripId(TripEntry.Text))
            return;

        await _db.SaveTripAsync(new TripLog
        {
            TripId = TripEntry.Text!,
            Location = $"{LatitudeLabel.Text}, {LongitudeLabel.Text}"
        });

        TripEntry.Text = "";
        TripErrorLabel.IsVisible = false;

        await LoadTripsAsync();
        StatusLabel.Text = "Trip saved";
    }

    // 💾 Save button press feedback
    private async void SaveButton_Pressed(object sender, EventArgs e)
    {
        if (sender is Button btn)
            await btn.ScaleTo(0.96, 80, Easing.CubicOut);
    }

    private async void SaveButton_Released(object sender, EventArgs e)
    {
        if (sender is Button btn)
            await btn.ScaleTo(1, 80, Easing.CubicIn);
    }

    // 🗑️ Delete trip with animation
    private async void DeleteTrip_Clicked(object sender, EventArgs e)
    {
        if (sender is ImageButton btn && btn.CommandParameter is TripLog trip)
        {
            if (btn.Parent?.Parent is Border border)
                await border.TranslateTo(120, 0, 200, Easing.CubicIn);

            await _db.DeleteTripAsync(trip);
            await LoadTripsAsync();

            StatusLabel.Text = "Trip deleted";
        }
    }

    // 🗑️ Delete button press feedback
    private async void DeleteButton_Pressed(object sender, EventArgs e)
    {
        if (sender is ImageButton btn)
            await btn.ScaleTo(0.85, 80, Easing.CubicOut);
    }

    private async void DeleteButton_Released(object sender, EventArgs e)
    {
        if (sender is ImageButton btn)
            await btn.ScaleTo(1, 80, Easing.CubicIn);
    }

    // ✨ Item appear animation
    private async void TripItem_Loaded(object sender, EventArgs e)
    {
        if (sender is Border border)
        {
            await border.TranslateTo(0, -4, 120, Easing.CubicOut);
            await border.TranslateTo(0, 0, 120, Easing.CubicIn);
        }
    }

    // 🔄 Pull-to-refresh
    private async void OnRefreshRequested(object sender, EventArgs e)
    {
        await LoadLocationAndConnectivity();
        await LoadTripsAsync();
        PageRefreshView.IsRefreshing = false;
    }

    // ✅ Validation
    private bool ValidateTripId(string? tripId)
    {
        TripErrorLabel.IsVisible = false;

        if (string.IsNullOrWhiteSpace(tripId))
        {
            TripErrorLabel.Text = "Trip ID cannot be empty";
            TripErrorLabel.IsVisible = true;
            return false;
        }

        if (tripId.Length < 3 || !tripId.All(char.IsLetterOrDigit))
        {
            TripErrorLabel.Text = "Trip ID must be alphanumeric";
            TripErrorLabel.IsVisible = true;
            return false;
        }

        return true;
    }

    // 📋 Load trips
    private async Task LoadTripsAsync()
    {
        TripsView.ItemsSource = await _db.GetTripsAsync();
    }

    // ℹ️ Expand / collapse help sections
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
}
