using Android.Bluetooth.LE;

namespace Findy.Services;

class RssiScanCallback : ScanCallback
{
    readonly Action<ScanResult> _onResult;
    readonly Action<ScanFailure> _onFailed;

    public RssiScanCallback(Action<ScanResult> onResult, Action<ScanFailure> onFailed)
    {
        _onResult = onResult;
        _onFailed = onFailed;
    }

    public override void OnScanResult(ScanCallbackType callbackType, ScanResult? result)
    {
        if (result is not null)
            _onResult(result);
    }

    public override void OnScanFailed(ScanFailure errorCode) => _onFailed(errorCode);
}