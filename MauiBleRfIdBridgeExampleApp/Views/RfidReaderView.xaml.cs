using MauiBleRfIdBridgeExampleApp.ViewModels;

namespace MauiBleRfIdBridgeExampleApp.Views
{
    public partial class RfidReaderView : ContentPage
    {
        public RfidReaderView(RfidReaderViewModel vm)
        {
            InitializeComponent();

            BindingContext = vm;
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is RfidReaderViewModel vm)
            {
                await vm.InitializeAsync();
            }
        }
    }
}
