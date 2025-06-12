using System.Threading.Tasks;
using Wpf.Ui.Abstractions.Controls;
using wpfChat.ViewModels.Pages;

namespace wpfChat.Views.Pages
{
    public partial class ChatPage : INavigableView<ChatViewModel>
    {
        public ChatViewModel ViewModel { get; }

        public ChatPage(ChatViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
            viewModel.updateResultEvent += (sender, result) =>
            {
                DisplayTextBox.Text = result;
            };
        }

        private async void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            string message = SendTextBox.Text.Trim();
            string result = "";
            if (!string.IsNullOrEmpty(message))
            {
                result = await ViewModel.SendMessage(message);
                //DisplayTextBox.Text = result;
                SendTextBox.Clear();
            }
        }
    }
}
