using System.ComponentModel;
using Findy.ViewModels;
using MauiIcons.Core;

namespace Findy.Pages;

public partial class RadarPage : ContentPage
{
    readonly RadarViewModel _vm;

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
        DeviceDisplay.Current.KeepScreenOn = true;
        _vm.Start();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.PropertyChanged -= OnVmPropertyChanged;
        Pulse.CancelAnimations();
        _vm.Stop();
        DeviceDisplay.Current.KeepScreenOn = false;
    }

    async void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(RadarViewModel.SignalLevel)) return;

        Pulse.CancelAnimations();
        await Pulse.ScaleToAsync(_vm.SignalLevel, 300, Easing.CubicOut);
    }
}