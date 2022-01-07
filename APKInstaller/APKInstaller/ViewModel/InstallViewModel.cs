﻿using AAPTForNet;
using AAPTForNet.Models;
using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using APKInstaller.Helpers;
using APKInstaller.Pages;
using APKInstaller.Pages.SettingsPages;
using APKInstaller.Properties;
using APKInstaller.Strings.InstallPage;
using ModernWpf.Controls;
using PortableDownloader;
using SharpCompress.Archives;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Resources;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Windows.System;

namespace APKInstaller.ViewModel
{
    internal class InstallViewModel : INotifyPropertyChanged, IDisposable
    {
        private DeviceData? _device;
        private readonly InstallPage _page;

        private bool _disposedValue;
        private readonly string _path;
        private static bool IsOnlyWSA => Settings.Default.IsOnlyWSA;
        private readonly ResourceManager _loader = new ResourceManager(typeof(InstallStrings));

        public string? InstallFormat => _loader.GetString("InstallFormat");
        public string? VersionFormat => _loader.GetString("VersionFormat");
        public string? PackageNameFormat => _loader.GetString("PackageNameFormat");

        private ApkInfo? _apkInfo = null;
        public ApkInfo? ApkInfo
        {
            get => _apkInfo;
            set
            {
                _apkInfo = value;
                RaisePropertyChangedEvent();
            }
        }

        private bool _isOpenApp = Settings.Default.IsOpenApp;
        public bool IsOpenApp
        {
            get => _isOpenApp;
            set
            {
                Settings.Default.IsOpenApp = value;
                Settings.Default.Save();
                _isOpenApp = Settings.Default.IsOpenApp;
                RaisePropertyChangedEvent();
            }
        }

        private bool _isInstalling;
        public bool IsInstalling
        {
            get => _isInstalling;
            set
            {
                _isInstalling = value;
                RaisePropertyChangedEvent();
            }
        }

        private bool _isInitialized;
        public bool IsInitialized
        {
            get => _isInitialized;
            set
            {
                _isInitialized = value;
                RaisePropertyChangedEvent();
            }
        }

        private string? _appName;
        public string? AppName
        {
            get => _appName;
            set
            {
                _appName = value;
                RaisePropertyChangedEvent();
            }
        }

        private string? _appVersion;
        public string? AppVersion
        {
            get => _appVersion;
            set
            {
                _appVersion = value;
                RaisePropertyChangedEvent();
            }
        }

        private string? _packageName;
        public string? PackageName
        {
            get => _packageName;
            set
            {
                _packageName = value;
                RaisePropertyChangedEvent();
            }
        }

        private string? _textOutput;
        public string? TextOutput
        {
            get => _textOutput;
            set
            {
                _textOutput = value;
                RaisePropertyChangedEvent();
            }
        }

        private string? _infoMessage;
        public string? InfoMessage
        {
            get => _infoMessage;
            set
            {
                _infoMessage = value;
                RaisePropertyChangedEvent();
            }
        }

        private bool _actionButtonEnable;
        public bool ActionButtonEnable
        {
            get => _actionButtonEnable;
            set
            {
                _actionButtonEnable = value;
                RaisePropertyChangedEvent();
            }
        }

        private bool _secondaryActionButtonEnable;
        public bool SecondaryActionButtonEnable
        {
            get => _secondaryActionButtonEnable;
            set
            {
                _secondaryActionButtonEnable = value;
                RaisePropertyChangedEvent();
            }
        }

        private bool _cancelOperationButtonEnable;
        public bool CancelOperationButtonEnable
        {
            get => _cancelOperationButtonEnable;
            set
            {
                _cancelOperationButtonEnable = value;
                RaisePropertyChangedEvent();
            }
        }

        private string? _waitProgressText;
        public string? WaitProgressText
        {
            get => _waitProgressText;
            set
            {
                _waitProgressText = value;
                RaisePropertyChangedEvent();
            }
        }

        private double _waitProgressValue = 0;
        public double WaitProgressValue
        {
            get => _waitProgressValue;
            set
            {
                _waitProgressValue = value;
                RaisePropertyChangedEvent();
            }
        }

        private bool _waitProgressIndeterminate = true;
        public bool WaitProgressIndeterminate
        {
            get => _waitProgressIndeterminate;
            set
            {
                _waitProgressIndeterminate = value;
                RaisePropertyChangedEvent();
            }
        }

        private string? _actionButtonText;
        public string? ActionButtonText
        {
            get => _actionButtonText;
            set
            {
                _actionButtonText = value;
                RaisePropertyChangedEvent();
            }
        }

        private string? _secondaryActionButtonText;
        public string? SecondaryActionButtonText
        {
            get => _secondaryActionButtonText;
            set
            {
                _secondaryActionButtonText = value;
                RaisePropertyChangedEvent();
            }
        }

        private string? _cancelOperationButtonText;
        public string? CancelOperationButtonText
        {
            get => _cancelOperationButtonText;
            set
            {
                _cancelOperationButtonText = value;
                RaisePropertyChangedEvent();
            }
        }

        private Visibility _textOutputVisibility;
        public Visibility TextOutputVisibility
        {
            get => _textOutputVisibility;
            set
            {
                _textOutputVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        private Visibility _installOutputVisibility;
        public Visibility InstallOutputVisibility
        {
            get => _installOutputVisibility;
            set
            {
                _installOutputVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        private Visibility _actionVisibility;
        public Visibility ActionVisibility
        {
            get => _actionVisibility;
            set
            {
                _actionVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        private Visibility _secondaryActionVisibility;
        public Visibility SecondaryActionVisibility
        {
            get => _secondaryActionVisibility;
            set
            {
                _secondaryActionVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        private Visibility _cancelOperationVisibility;
        public Visibility CancelOperationVisibility
        {
            get => _cancelOperationVisibility;
            set
            {
                _cancelOperationVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        private Visibility _messagesToUserVisibility;
        public Visibility MessagesToUserVisibility
        {
            get => _messagesToUserVisibility;
            set
            {
                _messagesToUserVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        private Visibility _launchWhenReadyVisibility;
        public Visibility LaunchWhenReadyVisibility
        {
            get => _launchWhenReadyVisibility;
            set
            {
                _launchWhenReadyVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        private Visibility _appVersionVisibility;
        public Visibility AppVersionVisibility
        {
            get => _appVersionVisibility;
            set
            {
                _appVersionVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        private Visibility _appPublisherVisibility;
        public Visibility AppPublisherVisibility
        {
            get => _appPublisherVisibility;
            set
            {
                _appPublisherVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        private Visibility _appCapabilitiesVisibility;
        public Visibility AppCapabilitiesVisibility
        {
            get => _appCapabilitiesVisibility;
            set
            {
                _appCapabilitiesVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        public InstallViewModel(string Path, InstallPage Page)
        {
            _path = Path;
            _page = Page;
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: false);
        }

        public async Task Refresh()
        {
            await InitilizeADB();
            await InitilizeUI();
        }

        public async Task CheckADB(bool force = false)
        {
        checkadb:
            if (!force && File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"platform-tools\adb.exe")))
            {
                WaitProgressText = _loader.GetString("ADBExist");
            }
            else if (NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
            {
                StackPanel StackPanel = new StackPanel();
                StackPanel.Children.Add(
                    new TextBlock()
                    {
                        TextWrapping = TextWrapping.Wrap,
                        Text = _loader.GetString("AboutADB")
                    });
                StackPanel.Children.Add(
                    new HyperlinkButton()
                    {
                        Content = _loader.GetString("ClickToRead"),
                        NavigateUri = new Uri("https://developer.android.google.cn/studio/releases/platform-tools?hl=zh-cn")
                    });
                ContentDialog dialog = new ContentDialog()
                {
                    Title = _loader.GetString("ADBMissing"),
                    PrimaryButtonText = _loader.GetString("OK"),
                    CloseButtonText = _loader.GetString("Cancel"),
                    Content = new ScrollViewer()
                    {
                        Content = StackPanel
                    },
                    DefaultButton = ContentDialogButton.Primary
                };
                ContentDialogResult result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    DownloadADB();
                }
                else
                {
                    Application.Current.Shutdown();
                    return;
                }
            }
            else
            {
                ContentDialog dialog = new ContentDialog()
                {
                    Title = "找不到网络",
                    PrimaryButtonText = "重试",
                    CloseButtonText = "关闭应用",
                    Content = new ScrollViewer()
                    {
                        Content = new TextBlock { Text = "请连接网络后再打开应用" }
                    },
                    DefaultButton = ContentDialogButton.Primary
                };
                ContentDialogResult result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    goto checkadb;
                }
                else
                {
                    Application.Current.Shutdown();
                    return;
                }
            }
        }

        public async void DownloadADB()
        {
            using Downloader downloader = new Downloader(new DownloaderOptions()
            {
                Uri = new Uri("https://dl.google.com/android/repository/platform-tools-latest-windows.zip?hl=zh-cn"),
                Stream = File.OpenWrite(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "platform-tools.zip"))
            });
        downloadadb:
            _ = downloader.Start();
            WaitProgressText = _loader.GetString("WaitDownload");

            while (downloader.TotalSize <= 0 && downloader.IsStarted)
            {
                await Task.Delay(1);
            }
            WaitProgressIndeterminate = false;
            while (downloader.IsStarted)
            {
                WaitProgressText = $"{((double)downloader.BytesPerSecond).GetSizeString()}/s";
                WaitProgressValue = (double)downloader.CurrentSize * 100 / downloader.TotalSize;
                await Task.Delay(1);
            }
            if (downloader.State != DownloadState.Finished)
            {
                ContentDialog dialog = new ContentDialog()
                {
                    Title = "ADB 下载失败",
                    PrimaryButtonText = "重试",
                    CloseButtonText = "关闭应用",
                    Content = new ScrollViewer()
                    {
                        Content = new TextBlock { Text = "请检查网络连接后再重试" }
                    },
                    DefaultButton = ContentDialogButton.Primary
                };
                ContentDialogResult result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    goto downloadadb;
                }
                else
                {
                    Application.Current.Shutdown();
                    return;
                }
            }
            WaitProgressValue = 0;
            WaitProgressIndeterminate = true;
            WaitProgressText = _loader.GetString("UnzipADB");
            await Task.Delay(1);
            IArchive archive = ArchiveFactory.Open(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "platform-tools.zip"));
            WaitProgressIndeterminate = false;
            foreach (IArchiveEntry entry in archive.Entries)
            {
                WaitProgressValue = archive.Entries.GetProgressValue(entry);
                WaitProgressText = string.Format(_loader.GetString("UnzippingFormat") ?? string.Empty, archive.Entries.ToList().IndexOf(entry) + 1, archive.Entries.Count());
                if (!entry.IsDirectory)
                {
                    entry.WriteToDirectory(AppDomain.CurrentDomain.BaseDirectory, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                }
                await Task.Delay(1);
            }
            WaitProgressValue = 0;
            WaitProgressIndeterminate = true;
            WaitProgressText = _loader.GetString("UnzipComplete");
        }

        public async Task InitilizeADB()
        {
            WaitProgressText = _loader.GetString("CheckingADB");
            await CheckADB();
            if (!string.IsNullOrEmpty(_path))
            {
                WaitProgressText = _loader.GetString("StartingADB");
                Process[] processes = Process.GetProcessesByName("adb");
                if (processes != null && processes.Length > 1)
                {
                    foreach (Process process in processes)
                    {
                        process.Kill();
                    }
                }
                if (processes != null && processes.Length == 1)
                {
                    try
                    {
                        await Task.Run(() => new AdbServer().StartServer(processes.First().MainModule?.FileName, restartServerIfNewer: false));
                    }
                    catch
                    {
                        foreach (Process process in processes)
                        {
                            process.Kill();
                        }
                        try
                        {
                            await Task.Run(() => new AdbServer().StartServer(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"platform-tools\adb.exe"), restartServerIfNewer: false));
                        }
                        catch
                        {
                            await CheckADB(true);
                            WaitProgressText = _loader.GetString("StartingADB");
                            await Task.Run(() => new AdbServer().StartServer(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"platform-tools\adb.exe"), restartServerIfNewer: false));
                        }
                    }
                }
                else
                {
                    try
                    {
                        await Task.Run(() => new AdbServer().StartServer(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"platform-tools\adb.exe"), restartServerIfNewer: false));
                    }
                    catch
                    {
                        await CheckADB(true);
                        WaitProgressText = _loader.GetString("StartingADB");
                        await Task.Run(() => new AdbServer().StartServer(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"platform-tools\adb.exe"), restartServerIfNewer: false));
                    }
                }
                if (IsOnlyWSA)
                {
                    new AdvancedAdbClient().Connect(new DnsEndPoint("127.0.0.1", 58526));
                }
                ADBHelper.Monitor.DeviceChanged += OnDeviceChanged;
            }
        }

        public async Task InitilizeUI()
        {
            if (!string.IsNullOrEmpty(_path))
            {
                WaitProgressText = _loader.GetString("Loading");
                try
                {
                    ApkInfo = await Task.Run(() => { return AAPTool.Decompile(_path); });
                    AppVersion = string.Format(_loader.GetString("VersionFormat") ?? string.Empty, ApkInfo.VersionName);
                    PackageName = string.Format(_loader.GetString("PackageNameFormat") ?? string.Empty, ApkInfo.PackageName);
                }
                catch (Exception ex)
                {
                    PackageError(ex.Message);
                    IsInitialized = true;
                    return;
                }
                if (string.IsNullOrEmpty(ApkInfo.PackageName))
                {
                    PackageError(_loader.GetString("InvalidPackage"));
                }
                else
                {
                checkdevice:
                    WaitProgressText = _loader.GetString("Checking");
                    if (CheckDevice() && _device != null)
                    {
                        CheckAPK();
                    }
                    else
                    {
                        ResetUI();
                        ActionButtonEnable = false;
                        ActionButtonText = _loader.GetString("Install");
                        InfoMessage = _loader.GetString("WaitingDevice");
                        ActionVisibility = MessagesToUserVisibility = Visibility.Visible;
                        AppName = string.Format(_loader.GetString("WaitingForInstallFormat") ?? string.Empty, ApkInfo.AppName);
                        if (IsOnlyWSA)
                        {
                            WaitProgressText = _loader.GetString("FindingWSA");
                            if ((await PackageHelper.FindPackagesByName("MicrosoftCorporationII.WindowsSubsystemForAndroid_8wekyb3d8bbwe")).isfound)
                            {
                                WaitProgressText = _loader.GetString("FoundWSA");
                                ContentDialog dialog = new ContentDialog()
                                {
                                    DefaultButton = ContentDialogButton.Close,
                                    Title = _loader.GetString("WSANotConnect"),
                                    Content = "WSA 可能没有启动，请打开 WSA 后重试",
                                    CloseButtonText = _loader.GetString("IKnow"),
                                    PrimaryButtonText = _loader.GetString("StartWSA"),
                                };
                                ContentDialogResult result = await dialog.ShowAsync();
                                if (result == ContentDialogResult.Primary)
                                {
                                    WaitProgressText = _loader.GetString("LaunchingWSA");
                                    _ = await Launcher.LaunchUriAsync(new Uri("wsa://"));
                                    bool IsWSARunning = false;
                                    while (!IsWSARunning)
                                    {
                                        await Task.Run(() =>
                                        {
                                            Process[] ps = Process.GetProcessesByName("vmmemWSA");
                                            IsWSARunning = ps != null && ps.Length > 0;
                                        });
                                    }
                                    while (!CheckDevice())
                                    {
                                        new AdvancedAdbClient().Connect(new DnsEndPoint("127.0.0.1", 58526));
                                        await Task.Delay(100);
                                    }
                                    WaitProgressText = _loader.GetString("WSARunning");
                                    goto checkdevice;
                                }
                            }
                            else
                            {
                                ContentDialog dialog = new ContentDialog()
                                {
                                    Title = _loader.GetString("GoToSetting"),
                                    DefaultButton = ContentDialogButton.Close,
                                    CloseButtonText = _loader.GetString("IKnow"),
                                    PrimaryButtonText = _loader.GetString("GoToSetting"),
                                    Content = "你可以安装 WSA 或者前往设置解除仅限 WSA 的限制",
                                };
                                ContentDialogResult result = await dialog.ShowAsync();
                                if (result == ContentDialogResult.Primary)
                                {
                                    UIHelper.Navigate(typeof(SettingsPage), null);
                                }
                            }
                        }
                    }
                }
                WaitProgressText = _loader.GetString("Finished");
            }
            else
            {
                ResetUI();
                ApkInfo = new ApkInfo();
                AppName = _loader.GetString("NoPackageWranning");
                AppVersionVisibility = AppPublisherVisibility = AppCapabilitiesVisibility = Visibility.Collapsed;
            }
            IsInitialized = true;
        }

        public void CheckAPK()
        {
            ResetUI();
            AdvancedAdbClient client = new AdvancedAdbClient();
            if (_device == null)
            {
                ActionButtonEnable = false;
                ActionButtonText = _loader.GetString("Install");
                InfoMessage = _loader.GetString("WaitingDevice");
                ActionVisibility = MessagesToUserVisibility = Visibility.Visible;
                AppName = string.Format(_loader.GetString("WaitingForInstallFormat") ?? string.Empty, ApkInfo?.AppName);
                return;
            }
            PackageManager manager = new PackageManager(client, _device);
            VersionInfo? info = null;
            if (ApkInfo != null)
            {
                info = manager.GetVersionInfo(ApkInfo.PackageName);
            }
            if (info == null)
            {
                ActionButtonText = _loader.GetString("Install");
                AppName = string.Format(_loader.GetString("InstallFormat") ?? string.Empty, ApkInfo?.AppName);
                ActionVisibility = LaunchWhenReadyVisibility = Visibility.Visible;
            }
            else if (info.VersionCode < int.Parse(ApkInfo?.VersionCode ?? "0"))
            {
                ActionButtonText = _loader.GetString("Update");
                AppName = string.Format(_loader.GetString("UpdateFormat") ?? string.Empty, ApkInfo?.AppName);
                ActionVisibility = LaunchWhenReadyVisibility = Visibility.Visible;
            }
            else
            {
                ActionButtonText = _loader.GetString("Reinstall");
                SecondaryActionButtonText = _loader.GetString("Launch");
                AppName = string.Format(_loader.GetString("ReinstallFormat") ?? string.Empty, ApkInfo?.AppName);
                TextOutput = string.Format(_loader.GetString("ReinstallOutput") ?? string.Empty, ApkInfo?.AppName);
                ActionVisibility = SecondaryActionVisibility = TextOutputVisibility = Visibility.Visible;
            }
        }

        private void ResetUI()
        {
            ActionVisibility =
            SecondaryActionVisibility =
            CancelOperationVisibility =
            TextOutputVisibility =
            InstallOutputVisibility =
            LaunchWhenReadyVisibility =
            MessagesToUserVisibility = Visibility.Collapsed;
            ActionButtonEnable =
            SecondaryActionButtonEnable =
            CancelOperationButtonEnable = true;
        }

        private void PackageError(string? message)
        {
            ResetUI();
            ApkInfo = new ApkInfo();
            TextOutput = message;
            AppName = _loader.GetString("CannotOpenPackage");
            TextOutputVisibility = InstallOutputVisibility = Visibility.Visible;
            AppVersionVisibility = AppPublisherVisibility = AppCapabilitiesVisibility = Visibility.Collapsed;
        }

        private void OnDeviceChanged(object? sender, DeviceDataEventArgs e)
        {
            if (IsOnlyWSA)
            {
                new AdvancedAdbClient().Connect(new DnsEndPoint("127.0.0.1", 58526));
            }
            if (!IsInstalling)
            {
                _page.RunOnUIThread(() =>
                {
                    if (CheckDevice() && _device != null)
                    {
                        CheckAPK();
                    }
                });
            }
        }

        public bool CheckDevice()
        {
            AdvancedAdbClient client = new AdvancedAdbClient();
            List<DeviceData> devices = client.GetDevices();
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            if (devices.Count <= 0) { return false; }
            foreach (DeviceData device in devices)
            {
                if (device == null || device.State == DeviceState.Offline) { continue; }
                if (IsOnlyWSA)
                {
                    client.ExecuteRemoteCommand("getprop ro.boot.hardware", device, receiver);
                    if (receiver.ToString().Contains("windows"))
                    {
                        _device = device ?? _device;
                        return true;
                    }
                }
                else
                {
                    DeviceData? data = string.IsNullOrEmpty(Settings.Default.DefaultDevice) ? null : JsonSerializer.Deserialize<DeviceData>(Settings.Default.DefaultDevice);
                    if (data != null && data.Name == device.Name && data.Model == device.Model && data.Product == device.Product)
                    {
                        _device = data;
                        return true;
                    }
                }
            }
            return false;
        }

        public void OpenAPP() => new AdvancedAdbClient().StartApp(_device, ApkInfo?.PackageName);

        public async void InstallAPP()
        {
            try
            {
                IsInstalling = true;
                CancelOperationButtonText = _loader.GetString("Cancel");
                CancelOperationVisibility = LaunchWhenReadyVisibility = Visibility.Visible;
                ActionVisibility = SecondaryActionVisibility = TextOutputVisibility = InstallOutputVisibility = Visibility.Collapsed;
                await Task.Run(() =>
                {
                    new AdvancedAdbClient().Install(_device, File.Open(_path, FileMode.Open, FileAccess.Read));
                });
                if (IsOpenApp)
                {
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(1000);// 据说如果安装完直接启动会崩溃。。。
                        OpenAPP();
                    });
                }
                IsInstalling = false;
                SecondaryActionVisibility = Visibility.Visible;
                SecondaryActionButtonText = _loader.GetString("Launch");
                CancelOperationVisibility = LaunchWhenReadyVisibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                IsInstalling = false;
                TextOutput = ex.Message;
                TextOutputVisibility = InstallOutputVisibility = Visibility.Visible;
                ActionVisibility = SecondaryActionVisibility = CancelOperationVisibility = LaunchWhenReadyVisibility = Visibility.Collapsed;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    ADBHelper.Monitor.DeviceChanged -= OnDeviceChanged;
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
