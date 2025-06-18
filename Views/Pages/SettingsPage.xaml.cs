using Wpf.Ui.Abstractions.Controls;
using wpfChat.ViewModels.Pages;

namespace wpfChat.Views.Pages
{
    public partial class SettingsPage : INavigableView<SettingsViewModel>
    {
        public SettingsViewModel ViewModel { get; }

        public SettingsPage(SettingsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

        private void TotalLayersBox_ValueChanged(object sender, Wpf.Ui.Controls.NumberBoxValueChangedEventArgs args)
        {
            ViewModel.TotalLayersChanged((int)args.NewValue);
        }

        private void ContextSizeBox_ValueChanged(object sender, Wpf.Ui.Controls.NumberBoxValueChangedEventArgs args)
        {
            ViewModel.ContextSizeChanged((uint)args.NewValue);
        }

        private void MaxTokensBox_ValueChanged(object sender, Wpf.Ui.Controls.NumberBoxValueChangedEventArgs args)
        {
            ViewModel.MaxTokensChanged((int)args.NewValue);
        }
    }
}
