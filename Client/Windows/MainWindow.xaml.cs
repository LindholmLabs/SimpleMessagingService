using Client.Utilities;
using Shared.Contracts;
using Client.Models;
using System.Net;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Client.Windows;

public partial class MainWindow : Window
{
    private IApiInterface _apiInterface;
    private SettingsModel _settingsModel;
    private DispatcherTimer _timer;

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _settingsModel = await SettingsModel.CreateAsync();
        _apiInterface = new ApiInterface(_settingsModel.SelectedServer.Uri);
        _apiInterface.ServerKey = _settingsModel.SelectedServer.Key;
        await LoadServers();

        _settingsModel.PropertyChanged += async (s, e) =>
        {
            if (e.PropertyName == "Servers" || e.PropertyName == "selectedServer")
            {
                await LoadServers();
                _apiInterface = new ApiInterface(_settingsModel.SelectedServer.Uri);
                _apiInterface.ServerKey = _settingsModel.SelectedServer.Key;
            }
        };

        //_timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(_settingsModel.PollingInterval) };
        //_timer.Tick += PollingCallback;
        //_timer.Start();
    }

    private async void PollingCallback(object sender, EventArgs e)
    {
        await LoadMessages();
        await LoadUsers();
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

        foreach (var server in _settingsModel.Servers)
        {
            var newServerItem = new MenuItem { Header = server.Name, Name = "server_item", IsChecked = server.serverId == _settingsModel.SelectedServer.serverId ? true : false };
            newServerItem.Click += (s, args) => ConnectToServer(server);
            ServerMenu.Items.Insert(ServerMenu.Items.Count, newServerItem);
        }

        if (_settingsModel.SelectedServer != null)
        {
            MainHeading.Text = $"Connecting to {_settingsModel.SelectedServer.Name}";

            Register(_settingsModel.SelectedServer.User);
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

        Dispatcher.Invoke(() => MainHeading.Text = $"Connected to {_settingsModel.SelectedServer.Name}");

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

            var newServer = new Server { Name = serverName, User = username, Uri = serverUri };
            _settingsModel.SaveServer(newServer);

            await LoadServers();
        }
    }

    private async void ConnectToServer(Server server)
    {
        Dispatcher.Invoke(() => UserList.Items.Clear());
        Dispatcher.Invoke(() => MessageList.Items.Clear());

        _settingsModel.SelectedServer = server;
        if (_settingsModel.SelectedServer.Uri == null) return;
        _apiInterface = new ApiInterface(_settingsModel.SelectedServer.Uri);

        await LoadServers();
    }

    private async void Register(string newUserName)
    {
        if (_settingsModel?.SelectedServer.Key != null && _settingsModel.SelectedServer.Key != Guid.Empty) return;

        var response = await _apiInterface.PostUserAsync(newUserName);

        if (response == null || !response.IsSuccessStatusCode)
        {
            MessageBox.Show("Failed to register");
            return;
        } 

        var newUserContract = JsonSerializer.Deserialize<NewUserContract>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (newUserContract == null) return;

        _settingsModel.SelectedServer.Key = newUserContract.Key;
    }

    private async void SendButton_Click(object sender, RoutedEventArgs e)
    {
        var response = await _apiInterface.PostMessageAsync(MessageInput.Text);
        if (response != null && !response.IsSuccessStatusCode)
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
        _settingsModel.RemoveServer(_settingsModel.SelectedServer);
    }
}