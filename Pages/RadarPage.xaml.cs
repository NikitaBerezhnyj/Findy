using System.ComponentModel;
using Findy.ViewModels;
using MauiIcons.Core;

namespace Findy.Pages;

public partial class RadarPage : ContentPage
{
    readonly RadarViewModel _vm;
    Window? _window;

    public RadarPage(RadarViewModel vm)
    {
        InitializeComponent();
        _ = new MauiIcon();
        BindingContext = _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.PropertyChanged += OnVmPropertyChanged;

        _window = Window;
        if (_window is not null)
        {
            _window.Stopped += OnWindowStopped;
            _window.Resumed += OnWindowResumed;
        }

        DeviceDisplay.Current.KeepScreenOn = true;
        _vm.Start();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.PropertyChanged -= OnVmPropertyChanged;

        if (_window is not null)
        {
            _window.Stopped -= OnWindowStopped;
            _window.Resumed -= OnWindowResumed;
            _window = null;
        }

        Pulse.CancelAnimations();
        _vm.Stop();
        DeviceDisplay.Current.KeepScreenOn = false;
    }

    void OnWindowStopped(object? sender, EventArgs e) => _vm.Stop();

    void OnWindowResumed(object? sender, EventArgs e) => _vm.Start();

    async void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(RadarViewModel.SignalLevel)) return;

        Pulse.CancelAnimations();
        await Pulse.ScaleToAsync(_vm.SignalLevel, 300, Easing.CubicOut);
    }
}