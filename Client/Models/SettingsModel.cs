using Client.Utilities;
using System.ComponentModel;

namespace Client.Models
{
    internal class SettingsModel : INotifyPropertyChanged
    {
        private ISettingsInterface _settingsInterface;
        private Settings? _settings;

        public Settings? Settings
        {
            get => _settings;
            set
            {
                if (_settings != value)
                {
                    _settings = value;
                    OnPropertyChanged(nameof(Settings));
                    _ = SaveSettingsAsync();
                }
            }
        }

        public List<Server> Servers
        {
            get => Settings?.Servers ?? new List<Server>();
            set
            {
                if (_settings != null)
                {
                    _settings.Servers = value;
                    OnPropertyChanged(nameof(Servers));
                    _ = SaveSettingsAsync();
                }
            }
        }

        public void SaveServer(Server newServer)
        {
            var temp = Servers;
            temp.Add(newServer);
            Servers = temp;
            SelectedServer = newServer;
        }

        public void RemoveServer(Server server)
        {
            var temp = Servers;
            temp.Remove(server);
            Servers = temp;
        }

        public Server SelectedServer
        {
            get => Servers.FirstOrDefault(x => x.serverId == Settings?.SelectedServer) ?? Servers.FirstOrDefault() ?? new Server();
            set
            {
                if (_settings != null)
                {
                    _settings.SelectedServer = value.serverId;
                    OnPropertyChanged(nameof(SelectedServer));
                    _ = SaveSettingsAsync();
                }
            }
        }

        public int PollingInterval
        {
            get => Settings?.PollingInterval ?? 1000;
            set
            {
                if (_settings != null)
                {
                    _settings.PollingInterval = value;
                    OnPropertyChanged(nameof(PollingInterval));
                    _ = SaveSettingsAsync();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private SettingsModel(ISettingsInterface settingsInterface)
        {
            _settingsInterface = settingsInterface;
        }

        public static async Task<SettingsModel> CreateAsync()
        {
            var model = new SettingsModel(new SettingsInterface());
            await model.InitializeAsync();
            return model;
        }

        private async Task InitializeAsync()
        {
            Settings = await _settingsInterface.LoadSettingsAsync<Settings>();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task SaveSettingsAsync()
        {
            if (Settings != null)
            {
                await _settingsInterface.SaveSettingsAsync(Settings);
            }
        }
    }
}