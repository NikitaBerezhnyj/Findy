using Findy.Models;
using Findy.ViewModels;
using MauiIcons.Core;

namespace Findy.Pages;

public partial class DevicesPage : ContentPage
{
    readonly DevicesViewModel _vm;
    bool _navigating;

    public DevicesPage(DevicesViewModel vm)
    {
        InitializeComponent();
        _ = new MauiIcon();
        BindingContext = _vm = vm;
    }

    void OnScrolled(object? sender, ItemsViewScrolledEventArgs e) => _vm.NotifyScrolled();

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.StartAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.Stop();
    }

    async void OnSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not DevicesItem item) return;

        ((CollectionView)sender!).SelectedItem = null;

        if (_navigating) return;
        _navigating = true;
        try
        {
            await Shell.Current.GoToAsync(nameof(RadarPage),
                new Dictionary<string, object> { ["target"] = item.ToTarget() });
        }
        finally
        {
            _navigating = false;
        }
    }

    async void OnLinkClicked(object? sender, EventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is not DevicesItem item) return;
        await DisplayAlertAsync("Тип зв'язку", item.LinkHelp, "OK");
    }
}