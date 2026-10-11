namespace Findy.Services;

public class BluetoothPermission : Permissions.BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
        OperatingSystem.IsAndroidVersionAtLeast(31)
            ? [(Android.Manifest.Permission.BluetoothScan, true),
               (Android.Manifest.Permission.BluetoothConnect, true)]
            : [(Android.Manifest.Permission.AccessFineLocation, true)];
}