using System;
using System.Diagnostics;
using System.IO;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;

namespace RustMeTweaker;

public sealed partial class MainPage : Page
{
    private readonly MainViewModel _viewModel = new();

    private async void Web()
    {
        try
        {
            await MainPageWeb.EnsureCoreWebView2Async();

            MainPageWeb.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "rustme.tweaker",
                Path.Combine(AppContext.BaseDirectory, "Pub"),
                CoreWebView2HostResourceAccessKind.Allow
            );

            MainPageWeb.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
            // MainPageWeb.CoreWebView2.Settings.AreDevToolsEnabled = false;
            MainPageWeb.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
            // MainPageWeb.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            MainPageWeb.CoreWebView2.Navigate("https://rustme.tweaker/index.html");
            MainPageWeb.CoreWebView2.Settings.IsNonClientRegionSupportEnabled = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
        }
    }

    private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs args)
    {
        string request = args.TryGetWebMessageAsString();
        string response = _viewModel.HandleMessage(request);
        MainPageWeb.CoreWebView2.PostWebMessageAsString(response);
    }

    public MainPage()
    {
        InitializeComponent();
        _viewModel.Initialize();
        Web();
    }
}