using System.Collections.Concurrent;
using System.Diagnostics;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Findy.Models;

namespace Findy.Services;

public class BluetoothService
{
    readonly BluetoothAdapter? _adapter =
        (Platform.AppContext.GetSystemService(Context.BluetoothService) as BluetoothManager)?.Adapter;

    readonly ConcurrentDictionary<string, (string Category, string Type)> _info = new();

    Action<ScanSample>[] _handlers = [];
    RssiScanCallback? _callback;

    public bool IsSupported => _adapter is not null;
    public bool IsEnabled => _adapter?.IsEnabled == true;

    public async Task<bool> EnsurePermissionsAsync()
    {
        var status = await Permissions.CheckStatusAsync<BluetoothPermission>();
        if (status != PermissionStatus.Granted)
            status = await Permissions.RequestAsync<BluetoothPermission>();
        return status == PermissionStatus.Granted;
    }

    public List<BtDevice> GetPairedDevices()
    {
        var result = new List<BtDevice>();
        if (_adapter?.BondedDevices is null) return result;

        foreach (var d in _adapter.BondedDevices)
        {
            if (d.Name is null) continue;
            result.Add(new BtDevice(d.Name, d.Address ?? "", GetCategory(d), GetLinkType(d)));
        }
        return result;
    }

    public IDisposable? Subscribe(Action<ScanSample> handler)
    {
        if (_handlers.Length == 0 && !StartScan())
            return null;

        _handlers = [.. _handlers, handler];
        return new Subscription(() => Unsubscribe(handler));
    }

    void Unsubscribe(Action<ScanSample> handler)
    {
        _handlers = _handlers.Where(h => h != handler).ToArray();
        if (_handlers.Length == 0) StopScan();
    }

    bool StartScan()
    {
        var scanner = _adapter?.BluetoothLeScanner;
        if (scanner is null) return false;

        _info.Clear();
        _callback = new RssiScanCallback(OnResult);

        var settings = new ScanSettings.Builder()!
            .SetScanMode(Android.Bluetooth.LE.ScanMode.LowLatency)!
            .Build()!;

        try
        {
            scanner.StartScan(null, settings, _callback);
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Findy: start scan failed: {ex.Message}");
            _callback = null;
            return false;
        }
    }

    void StopScan()
    {
        if (_callback is null) return;
        _adapter?.BluetoothLeScanner?.StopScan(_callback);
        _callback = null;
    }

    void OnResult(ScanResult r)
    {
        var device = r.Device;
        var address = device?.Address ?? "";


        (string Category, string Type) info = device is null
            ? (DeviceKind.Unknown, "Unknown")
            : _info.GetOrAdd(address, _ => (GetCategory(device), GetLinkType(device)));

        var connectable = !OperatingSystem.IsAndroidVersionAtLeast(26) || r.IsConnectable;

        var sample = new ScanSample(
            address,
            r.ScanRecord?.DeviceName ?? device?.Name,
            r.Rssi,
            info.Category,
            info.Type,
            connectable);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            foreach (var handler in _handlers) handler(sample);
        });
    }

    static string GetCategory(BluetoothDevice d) => d.BluetoothClass?.MajorDeviceClass switch
    {
        MajorDeviceClass.AudioVideo => "Audio",
        MajorDeviceClass.Wearable => "Watch / Band",
        MajorDeviceClass.Phone => "Phone",
        MajorDeviceClass.Computer => "Computer",
        _ => DeviceKind.Unknown
    };

    static string GetLinkType(BluetoothDevice d) => d.Type switch
    {
        BluetoothDeviceType.Classic => "Classic",
        BluetoothDeviceType.Le => "LE",
        BluetoothDeviceType.Dual => "Dual",
        _ => "Unknown"
    };

    sealed class Subscription(Action onDispose) : IDisposable
    {
        Action? _onDispose = onDispose;

        public void Dispose()
        {
            _onDispose?.Invoke();
            _onDispose = null;
        }
    }
}