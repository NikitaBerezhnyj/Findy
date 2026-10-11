using Android.Bluetooth.LE;

namespace Findy.Services;

class RssiScanCallback : ScanCallback
{
    readonly Action<ScanResult> _onResult;

    public RssiScanCallback(Action<ScanResult> onResult)
        => _onResult = onResult;

    public override void OnScanResult(
        ScanCallbackType callbackType,
        ScanResult? result)
    {
        if (result is not null)
            _onResult(result);
    }

    public override void OnScanFailed(ScanFailure errorCode)
        => System.Diagnostics.Debug.WriteLine(
            $"Findy: scan failed {errorCode}");
}