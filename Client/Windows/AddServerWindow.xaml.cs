using System.Windows;

namespace Client.Windows;

public partial class AddServerWindow : Window
{
    public string? ServerName { get; private set; }
    public string? ServerUrl { get; private set; }
    public string? Username { get; private set; }

    public AddServerWindow()
    {
        InitializeComponent();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        ServerName = NameTextBox.Text;
        ServerUrl = UrlTextBox.Text;
        Username = UsernameTextBox.Text;
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}