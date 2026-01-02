using System.Windows;

namespace AsukaTree
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm = new();

        public MainWindow()
        {
            InitializeComponent();

            DataContext = _vm;
            PortBox.Text = _vm.PortText;

            _vm.RaiseStatusChanged += UpdateUi;
            Loaded += (_, __) => UpdateUi(); // 起動時は開始しない
        }

        private async void OnToggleClick(object sender, RoutedEventArgs e)
        {
            _vm.PortText = PortBox.Text;
            await _vm.TogglePollingAsync();
            UpdateUi();
        }

        private void UpdateUi()
        {
            StatusText.Text = _vm.Status;
            ToggleButton.Content = _vm.IsPolling ? "停止" : "開始";
            PortBox.IsEnabled = !_vm.IsPolling;
        }
    }
}
