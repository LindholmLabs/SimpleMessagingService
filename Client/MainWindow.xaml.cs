using Client.Utilities;
using Shared.Contracts;
using System.Linq.Expressions;
using System.Net;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Client;

public partial class MainWindow : Window
{
    private IApiInterface _apiInterface;
    private ISettingsInterface _settingsInterface = new SettingsInterface();
    private Settings _settings { get; set; }
    private DispatcherTimer _timer;

    private Server? _selectedServer => _settings?.Servers?.FirstOrDefault(x => x.serverId == _settings.SelectedServer);

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadSettings();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(_settings.PollingInterval) };
        _timer.Tick += PollingCallback;
        _timer.Start();
    }

    private async void PollingCallback(object sender, EventArgs e)
    {
        await LoadMessages();
        await LoadUsers();
    }

    private async Task LoadSettings()
    {
        _settings = await _settingsInterface.LoadSettingsAsync<Settings>();

        if (_settings == null)
        {
            _settings = new Settings();
            await _settingsInterface.SaveSettingsAsync(_settings);
        }

        await LoadServers();
    }

    private async Task LoadServers()
    {
        var itemsToRemove = new List<MenuItem>();

        foreach (var item in ServerMenu.Items)
        {
            if (item is MenuItem menuItem && menuItem.Name == "server_item")
            {
                itemsToRemove.Add(menuItem);
            }
        }

        foreach (var item in itemsToRemove)
        {
            ServerMenu.Items.Remove(item);
        }

        foreach (var server in _settings.Servers)
        {
            var newServerItem = new MenuItem { Header = server.Name, Name = "server_item", IsChecked = server.serverId == _settings.SelectedServer ? true : false };
            newServerItem.Click += (s, args) => ConnectToServer(server.serverId);
            ServerMenu.Items.Insert(ServerMenu.Items.Count, newServerItem);
        }


        var selectedServerUrl = _settings.Servers.FirstOrDefault(s => s.serverId == _settings.SelectedServer)?.Url;
        if (selectedServerUrl != null)
        {
            MainHeading.Text = $"Connecting to {_selectedServer.Name}";
            _apiInterface = new ApiInterface(selectedServerUrl);

            Register(_selectedServer.User);
            _apiInterface.ServerKey = (Guid)_selectedServer.Key;
            await LoadMessages();
            await LoadUsers();
        }
    }

    private async Task LoadMessages()
    {
        var messages = await _apiInterface.GetMessagesAsync();

        Dispatcher.Invoke(() => MessageList.Items.Clear());

        if (messages?.messages == null)
        {
            return;
        }

        Dispatcher.Invoke(() => MainHeading.Text = $"Connected to {_selectedServer.Name}");

        foreach (var message in messages?.messages)
        {
            var timeStamp = message.TimeStamp.ToString("g");
            Dispatcher.Invoke(() => MessageList.Items.Add($"{timeStamp} - {message.Sender}: {message.Content}"));
        }
    }

    private async Task LoadUsers()
    {
        var users = await _apiInterface.GetUsersAsync();
        Dispatcher.Invoke(() => UserList.Items.Clear());

        if (users?.users == null)
        {
            return;
        }

        foreach (var user in users?.users)
        {
            Dispatcher.Invoke(() => UserList.Items.Add(user.Username));
        }
    }

    private async void Server_Add_Click(object sender, RoutedEventArgs e)
    {
        AddServerWindow addServerWindow = new AddServerWindow();
        if (addServerWindow.ShowDialog() == true)
        {
            string serverName = addServerWindow.ServerName;
            string username = addServerWindow.Username;
            Uri serverUri;
            try
            {
                serverUri = new Uri(addServerWindow.ServerUrl);
            } 
            catch (UriFormatException)
            {
                MessageBox.Show("Invalid Url");
                return;
            }

            var newServer = new Server { Name = serverName, User = username, Url = serverUri };
            _settings.Servers.Add(newServer);
            _settings.SelectedServer = newServer.serverId;
            await _settingsInterface.SaveSettingsAsync(_settings);
            await LoadServers();
        }
    }

    private async void ConnectToServer(Guid key)
    {
        Dispatcher.Invoke(() => UserList.Items.Clear());
        Dispatcher.Invoke(() => MessageList.Items.Clear());
        _settings.SelectedServer = key;
        await _settingsInterface.SaveSettingsAsync(_settings);
        await LoadServers();
        var server = _settings.Servers.FirstOrDefault(s => s.serverId == key);
    }

    private async void Register(string newUserName)
    {
        if (_selectedServer?.Key != null && _selectedServer?.Key != Guid.Empty)
        {
            return;
        }

        var response = await _apiInterface.PostUserAsync(newUserName);
        if (response == null || !response.IsSuccessStatusCode)
        {
            MessageBox.Show("Failed to register");
            return;
        } 
        else
        {
            var test = await response.Content.ReadAsStringAsync();
            var newUserContract = JsonSerializer.Deserialize<NewUserContract>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var index = _settings.Servers.FindIndex(x => x.serverId == _selectedServer.serverId);
            _settings.Servers[index].Key = newUserContract?.Key;
            await _settingsInterface.SaveSettingsAsync(_settings);
        }
    }

    private async void SendButton_Click(object sender, RoutedEventArgs e)
    {
        var response = await _apiInterface.PostMessageAsync(MessageInput.Text);
        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            switch (response.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    MessageBox.Show($"Bad request.\n{errorMessage}");
                    break;
                case HttpStatusCode.InternalServerError:
                    MessageBox.Show("Internal server error");
                    break;
                default:
                    MessageBox.Show("Unknown error");
                    break;
            }
        } 
        else
        {
            MessageInput.Text = "";
            await LoadMessages();
        }
    }

    private void Server_Remove_Click(object sender, RoutedEventArgs e)
    {
        var index = _settings.Servers.FindIndex(x => x.serverId == _settings.SelectedServer);
        _settings.Servers.RemoveAt(index);
        _settings.SelectedServer = _settings.Servers.FirstOrDefault()?.serverId;
        _settingsInterface.SaveSettingsAsync(_settings);
        LoadSettings();
    }
}