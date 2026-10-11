# Findy

<p align="center">
  <img src="./Resources/AppIcon/appicon.svg" alt="App logo" width="200"/>
</p>

Findy is a simple Android tool for finding a lost Bluetooth device, such as an earbud that fell into the grass. It allows you to:

- Scan for nearby Bluetooth Low Energy devices and see them sorted by signal strength, with the strongest one at the top.
- Recognize your paired devices even when they advertise under a different name than the one saved on your phone.
- Tell devices apart at a glance by type (audio, watch / band, phone, computer), connection type (Classic, LE, Dual) and signal quality.
- Track a chosen device live with an approximate distance in meters, a closer / farther trend and a pulsing signal radar.

The app is intentionally small: two screens, no accounts, no ads, no subscriptions and no data saved. It is a rescue tool that works from a cold start, even if you never set anything up before losing the device.

## How It Works

Findy reads the RSSI (received signal strength) from Bluetooth LE advertisements, smooths it with an exponential moving average and converts it to an approximate distance using a path-loss model. Distance is an estimate: walls, bodies and grass all affect the signal. The trend indicator (closer / farther) is usually more useful than the exact number when you are walking around searching.

Devices that only support Classic Bluetooth and do not advertise over LE cannot be tracked.

## Technologies

- [.NET MAUI](https://learn.microsoft.com/en-us/dotnet/maui/) (.NET 10) — cross-platform UI framework, currently targeting Android only.
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/) — MVVM source generators for observable properties and commands.
- [CommunityToolkit.Maui](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/maui/) — additional MAUI controls and helpers.
- [MauiIcons](https://github.com/AathifMahir/MauiIcons) — Material icon set.
- [Android Bluetooth LE API](https://developer.android.com/develop/connectivity/bluetooth/ble/ble-overview) — scanning and RSSI readings.

## Running the App

1. Clone the repository:

```bash
git clone https://github.com/NikitaBerezhnyj/Findy.git
cd Findy
```

2. Install the [.NET 10 SDK](https://dotnet.microsoft.com/download) and the MAUI workload:

```bash
dotnet workload install maui
```

Make sure the Android SDK is installed and an Android phone is connected with USB debugging enabled. A real device is recommended, because emulators have no real Bluetooth.

3. Build and run on the connected device:

```bash
dotnet build -t:Run -f net10.0-android
```

## License & Community Guidelines

- [MIT License](LICENSE) — project license.
- [Code of Conduct](CODE_OF_CONDUCT.md) — expected behavior for contributors.
- [Contributing Guide](CONTRIBUTING.md) — how to help the project.
- [Security Policy](SECURITY.md) — reporting security issues.
