using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace AsukaTree
{
    public sealed class MainViewModel
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

        public string PortText { get; set; } = "8080";
        public ObservableCollection<JsonTreeNode> Roots { get; } = new();

        public bool IsPolling { get; private set; } = false;

        public string Status { get; private set; } = "Ready";
        public event Action? RaiseStatusChanged;

        public MainViewModel()
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            _timer.Tick += async (_, __) => await RefreshAsync();
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

                // 開始時に1回即時更新してから、3秒ポーリングへ
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
                Status = $"GET {uri}";
                RaiseStatusChanged?.Invoke();

                var json = await _http.GetStringAsync(uri);

                // items配列だけを表示（type 0/1/10 の表示ルール込み）
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
    }
}
