using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace AnalyzerStudio;

/// <summary>
/// Interaktionslogik für AboutWindow.xaml
/// </summary>
public partial class AboutWindow : Window
{
	public AboutWindow()
	{
		InitializeComponent();
	}

	private void ButtonOk_Click(object sender, RoutedEventArgs e)
	{
		Close();
	}

	private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
	{
		var url = e.Uri.AbsoluteUri;
		url = url.Replace("&", "^&");
		Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
	}
}