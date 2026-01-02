using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace AsukaTree
{
    public sealed class MainViewModel : INotifyPropertyChanged
    {
        private const string Host = "localhost";
        private const string Path = "/api/info";
        private const string Scheme = "http";

        private readonly HttpClient _http = new()
        {
            Timeout = TimeSpan.FromSeconds(2.5)
        };

        private readonly DispatcherTimer _timer;
        private int _refreshing = 0;

        public string PortText { get; set; } = "33333";
        public ObservableCollection<JsonTreeNode> Roots { get; } = new();

        public bool IsPolling { get; private set; } = false;

        public string Status { get; private set; } = "Ready";
        public event Action? RaiseStatusChanged;

        // ===== 背景関連 =====
        private string? _backgroundImagePath;
        public string? BackgroundImagePath
        {
            get => _backgroundImagePath;
            private set { _backgroundImagePath = value; OnPropertyChanged(nameof(BackgroundImagePath)); }
        }

        private double _backgroundOpacity = 0.35;
        public double BackgroundOpacity
        {
            get => _backgroundOpacity;
            set
            {
                _backgroundOpacity = Math.Clamp(value, 0.0, 1.0);
                OnPropertyChanged(nameof(BackgroundOpacity));
                UpdateBackgroundBrush();
            }
        }

        private Brush? _treeBackgroundBrush;
        public Brush? TreeBackgroundBrush
        {
            get => _treeBackgroundBrush;
            private set { _treeBackgroundBrush = value; OnPropertyChanged(nameof(TreeBackgroundBrush)); }
        }

        public MainViewModel()
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            _timer.Tick += async (_, __) => await RefreshAsync();

            // デフォルトは背景なし（必要ならここで初期化）
            UpdateBackgroundBrush();
        }

        public void SetBackgroundImage(string filePath)
        {
            BackgroundImagePath = filePath;
            UpdateBackgroundBrush();
        }

        public void ClearBackgroundImage()
        {
            BackgroundImagePath = null;
            UpdateBackgroundBrush();
        }

        private void UpdateBackgroundBrush()
        {
            if (string.IsNullOrWhiteSpace(BackgroundImagePath))
            {
                TreeBackgroundBrush = null; // 既定背景
                return;
            }

            // ローカルファイルを読み込む（ロック回避のため CacheOption=OnLoad）
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(BackgroundImagePath, UriKind.Absolute);
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            bmp.Freeze();

            var brush = new ImageBrush(bmp)
            {
                Opacity = BackgroundOpacity,
                Stretch = Stretch.UniformToFill,   // 好みで Uniform / None など
                AlignmentX = AlignmentX.Center,
                AlignmentY = AlignmentY.Center
            };
            brush.Freeze();

            TreeBackgroundBrush = brush;
        }

        private Uri BuildUri()
        {
            if (!int.TryParse(PortText, out var port) || port < 1 || port > 65535)
                throw new FormatException("ポート番号が不正です。");

            return new UriBuilder(Scheme, Host, port, Path).Uri;
        }

        public async Task TogglePollingAsync()
        {
            if (!IsPolling)
            {
                IsPolling = true;
                RaiseStatusChanged?.Invoke();

                await RefreshAsync();
                _timer.Start();
            }
            else
            {
                _timer.Stop();
                IsPolling = false;
                Status = "Stopped";
                RaiseStatusChanged?.Invoke();
            }
        }

        public async Task RefreshAsync()
        {
            if (Interlocked.Exchange(ref _refreshing, 1) == 1) return;

            try
            {
                var uri = BuildUri();

                var json = await _http.GetStringAsync(uri);

                var nodes = JsonItemsParser.ParseItemsOnly(json);

                Roots.Clear();
                foreach (var n in nodes) Roots.Add(n);

                Status = $"Updated: {DateTime.Now:HH:mm:ss}";
                RaiseStatusChanged?.Invoke();
            }
            catch (Exception ex)
            {
                Status = $"Error: {ex.Message}";
                RaiseStatusChanged?.Invoke();
            }
            finally
            {
                Interlocked.Exchange(ref _refreshing, 0);
            }
        }


        // ===== INotifyPropertyChanged =====
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private double _itemFontSize = 15.0;

        public double ItemFontSize
        {
            get => _itemFontSize;
            set
            {
                // 好きな範囲に調整してOK
                var v = Math.Clamp(value, 10.0, 24.0);
                if (Math.Abs(_itemFontSize - v) < 0.001) return;
                _itemFontSize = v;
                OnPropertyChanged(nameof(ItemFontSize));
            }
        }
    }
}
