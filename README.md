# Loca

Loca is a simple .NET MAUI mobile app for saving trip checkpoints with the device's current location. It captures live latitude and longitude, shows current network status, and stores saved trip entries locally with SQLite so they remain available offline.

## Features

- Live trip status view with latitude, longitude, network access, and last updated time
- Trip checkpoint saving with Trip ID validation
- Local SQLite storage for saved trips
- Pull-to-refresh for current device status
- Delete support for saved trip records
- Android, iOS, Mac Catalyst, and Windows targets in a single MAUI project

## Tech Stack

- .NET 9
- .NET MAUI
- `sqlite-net-pcl`
- MAUI Essentials APIs for geolocation, connectivity, and permissions

## Project Structure

- `SmartNavigationMAD/` - MAUI application source
- `SmartNavigationMAD/Data` - SQLite access layer
- `SmartNavigationMAD/Models` - persisted models
- `SmartNavigationMAD/Platforms` - platform-specific manifests and plist files
- `SmartNavigationMAD/Resources` - app icons, splash screen, images, fonts, and styles

## Requirements

- .NET 9 SDK
- MAUI workloads installed
- Visual Studio 2022 or later with .NET MAUI support

## Running The App

1. Clone the repository.
2. Open `SmartNavigationMAD.sln` in Visual Studio.
3. Restore packages if prompted.
4. Choose a target platform such as Android, Windows, or iOS simulator.
5. Build and run the app.

From the command line:

```powershell
dotnet build SmartNavigationMAD.sln
```

## Permissions

Loca requests location access to capture the current trip checkpoint.

- Android uses `ACCESS_COARSE_LOCATION` and `ACCESS_FINE_LOCATION`
- iOS and Mac Catalyst include `NSLocationWhenInUseUsageDescription`

If location permission is denied, the app will still load saved trips, but it will not allow saving a new checkpoint until location access is available.

## Current Behavior

- Trip IDs are normalized to uppercase before saving
- Trip IDs must be 3 to 20 alphanumeric characters
- Trips are stored locally on-device in a SQLite database
- Saved trips are ordered newest first

## Build Status

The solution builds successfully with:

```powershell
dotnet build SmartNavigationMAD.sln
```

## Future Improvements

- Add map-based trip visualization
- Add timestamps to saved trip records
- Add export or sync support
- Add automated tests
- Add screenshots to this README
