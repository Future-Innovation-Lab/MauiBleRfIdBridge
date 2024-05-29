using CommunityToolkit.Maui.Converters;
using CommunityToolkit.Maui.Views;
using MauiBleRfIdBridgeExampleApp.ViewModels;

namespace MauiBleRfIdBridgeExampleApp.Views.Controls;

public partial class DevicePopup : Popup
{

    public DevicePopup(DevicePopupViewModel viewModel)
    {
        InitializeComponent();

        DevicesFoundLabel.SetBinding(
        Label.IsVisibleProperty,
        new Binding(
            nameof(viewModel.IsScanning),
            converter: new InvertedBoolConverter()));

        DevicesFoundImage.SetBinding(
        Image.IsVisibleProperty,
        new Binding(
            nameof(viewModel.IsScanning),
            converter: new InvertedBoolConverter()));
      
        BindingContext = viewModel;

        // DevicesListview.Behaviors.Add(behavior);

        this.Opened += DevicePopup_Opened;

        viewModel.CloseAction = new Action(() => this.Close());
    }

    private async void DevicePopup_Opened(object sender, CommunityToolkit.Maui.Core.PopupOpenedEventArgs e)
    {
        await Initialize();
    }

    public async Task Initialize()
    {
        if (BindingContext is DevicePopupViewModel viewModel)
        {
            await viewModel.BeginScan();
        }

    }

}