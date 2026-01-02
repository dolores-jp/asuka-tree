using Microsoft.Win32;
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

        private void OnChooseBackgroundClick(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "背景画像を選択",
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.webp;*.bmp|All Files|*.*",
                CheckFileExists = true
            };

            if (dlg.ShowDialog(this) == true)
            {
                _vm.SetBackgroundImage(dlg.FileName);
            }
        }

        private void OnClearBackgroundClick(object sender, RoutedEventArgs e)
        {
            _vm.ClearBackgroundImage();
        }

        private void UpdateUi()
        {
            StatusText.Text = _vm.Status;
            ToggleButton.Content = _vm.IsPolling ? "停止" : "開始";
            PortBox.IsEnabled = !_vm.IsPolling;
        }
    }
}
