﻿using MahApps.Metro.Controls.Dialogs;
using MTMCL.Lang;
using MTMCL.Task;
using MTMCL.util;
using MTMCL.Versions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;


namespace MTMCL
{
    /// <summary>
    /// GridMCDL.xaml 的互動邏輯
    /// </summary>
    public partial class GridMCDL : Grid
    {
        Grid parent;
        public GridMCDL ()
        {
            InitializeComponent();
        }

        public GridMCDL(Grid parent) : this() {
            this.parent = parent;
        }

        private void butBack_Click(object sender, RoutedEventArgs e)
        {
            Back();
        }
        private void Back()
        {
            MeCore.MainWindow.gridOthers.Children.Clear();
            MeCore.MainWindow.gridOthers.Children.Add(parent);
            var ani = new DoubleAnimationUsingKeyFrames();
            ani.KeyFrames.Add(new LinearDoubleKeyFrame(0, TimeSpan.FromSeconds(0)));
            ani.KeyFrames.Add(new LinearDoubleKeyFrame(1, TimeSpan.FromSeconds(0.2)));
            MeCore.MainWindow.gridOthers.BeginAnimation(OpacityProperty, ani);
        }
        private bool LoadedVersion = false;
        private System.Threading.Tasks.Task readthread;
        private void Grid_Initialized(object sender, EventArgs e)
        {
            ReloadVanillaVersion();LoadedVersion = true;
        }

        private void butReloadMC_Click(object sender, EventArgs e) {
            ReloadVanillaVersion();
        }

        private void ReloadVanillaVersion()
        {
            butReloadMC.IsEnabled = false;
            listRemoteVer.Visibility = Visibility.Collapsed;
            gridMCRFail.Visibility = Visibility.Collapsed;
            gridMCRing.Visibility = Visibility.Visible;
            listRemoteVer.ItemsSource = null;
            var rawJson = new DataContractJsonSerializer(typeof(RawVersionListType));
            var getJson = (HttpWebRequest)WebRequest.Create(MTMCL.Resources.UrlReplacer.getVersionsUrl());
            getJson.Timeout = 10000;
            getJson.ReadWriteTimeout = 10000;
#if DEBUG
            getJson.UserAgent = "MTMCL DEBUGGING";
#else
            getJson.UserAgent = "MTMCL " + MeCore.version;
#endif
            readthread = new System.Threading.Tasks.Task(delegate
            {
                try
                {
                    Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(async delegate
                    {
                        butReloadMC.SetLocalizedContent("RemoteVerGetting");
                        if (MeCore.Config.DownloadSource == 1)
                        {
                            await TaskEx.Delay(TimeSpan.FromSeconds(1));
                        }
                    }));
                    var getJsonAns = (HttpWebResponse)getJson.GetResponse();
                    // ReSharper disable once AssignNullToNotNullAttribute
                    var remoteVersion = rawJson.ReadObject(getJsonAns.GetResponseStream()) as RawVersionListType;
                    Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate
                    {
                        gridMCRing.Visibility = Visibility.Collapsed;
                        butReloadMC.SetLocalizedContent("Reload");
                        butReloadMC.IsEnabled = true;
                        if (remoteVersion != null)
                        {
                            listRemoteVer.ItemsSource = remoteVersion.getVersions();
                            listRemoteVer.Visibility = Visibility.Visible;
                            ICollectionView view = CollectionViewSource.GetDefaultView(listRemoteVer.ItemsSource);
                            view.Filter = (obj) => {
                                if (!(obj is RemoteVerType)) return false;
                                if (!((bool)butFilter.IsChecked)) return true;
                                else {
                                    var ver = (RemoteVerType)obj;
                                    if (!(bool)chkRel.IsChecked & ver.type.Equals("release")) return false;
                                    if (!(bool)chkSS.IsChecked & ver.type.Equals("snapshot")) return false;
                                    if (!(bool)chkBeta.IsChecked & ver.type.Equals("old_beta")) return false;
                                    if (!(bool)chkAlpha.IsChecked & ver.type.Equals("old_alpha")) return false;
                                }
                                return true;
                            };
                        }
                    }));
                }
                catch (WebException ex)
                {
                    Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate
                    {
                        gridMCRing.Visibility = Visibility.Collapsed;
                        gridMCRFail.Visibility = Visibility.Visible;
                        Dispatcher.Invoke(new Action(() => MeCore.MainWindow.addNotice(new Notice.CrashErrorBar(string.Format(LangManager.GetLocalized("ErrorNameFormat"), DateTime.Now.ToLongTimeString()), LangManager.GetLocalized("RemoteVerFailedTimeout"), ex.ToWellKnownExceptionString()) { ImgSrc = new BitmapImage(new Uri("pack://application:,,,/Resources/error-banner.jpg")) })));
                        butReloadMC.SetLocalizedContent("Reload");
                        butReloadMC.IsEnabled = true;
                    }));
                }
                catch (TimeoutException ex)
                {
                    Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate
                    {
                        gridMCRing.Visibility = Visibility.Collapsed;
                        gridMCRFail.Visibility = Visibility.Visible;
                        MeCore.MainWindow.addNotice(new Notice.CrashErrorBar(string.Format(LangManager.GetLocalized("ErrorNameFormat"), DateTime.Now.ToLongTimeString()), LangManager.GetLocalized("RemoteVerFailedTimeout"), ex.ToWellKnownExceptionString()) { ImgSrc = new BitmapImage(new Uri("pack://application:,,,/Resources/error-banner.jpg")) });
                        butReloadMC.SetLocalizedContent("Reload");
                        butReloadMC.IsEnabled = true;
                    }));
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate
                    {
                        gridMCRing.Visibility = Visibility.Collapsed;
                        gridMCRFail.Visibility = Visibility.Visible;
                        MeCore.MainWindow.addNotice(new Notice.CrashErrorBar(string.Format(LangManager.GetLocalized("ErrorNameFormat")), ex.ToWellKnownExceptionString()) { ImgSrc = new BitmapImage(new Uri("pack://application:,,,/Resources/error-banner.jpg")) });
                        butReloadMC.SetLocalizedContent("Reload");
                        butReloadMC.IsEnabled = true;
                    }));
                }
            });
            readthread.Start();
        }
        private void DownloadMinecraft(RemoteVerType ver)
        {
            if (ver == null)
            {
                MessageBox.Show(LangManager.GetLocalized("RemoteVerErrorNoVersionSelect"));
                return;
            }
                TaskListBar taskbar = new TaskListBar() { /*ImgSrc = new BitmapImage(new Uri("pack://application:,,,/Resources/download-banner.jpg"))*/ Icon = Application.Current.Resources["task_download_icon"] as Visual };
                var task = new Thread(new ThreadStart(async delegate
                {
                    Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(async delegate
                    {
                        if (MeCore.Config.DownloadSource == 1)
                        {
                            await TaskEx.Delay(TimeSpan.FromSeconds(1));
                        }
                    }));
                    var selectver = ver.id;
                    var downpath = new StringBuilder(MeCore.Config.MCPath + @"\versions\");
                    downpath.Append(selectver).Append("\\");
                    downpath.Append(selectver).Append(".jar");
                    if (MeCore.IsServerDedicated)
                    {
                        if (!string.IsNullOrWhiteSpace(MeCore.Config.Server.ClientPath))
                        {
                            downpath.Replace(MeCore.Config.MCPath, Path.Combine(MeCore.BaseDirectory, MeCore.Config.Server.ClientPath));
                        }
                    }
                    using (var downer = new WebClient())
                    {
                        downer.Headers.Add("User-Agent", "MTMCL" + MeCore.version);
                        var downurl = new StringBuilder(MTMCL.Resources.UrlReplacer.getDownloadUrl());
                        downurl.Append(@"versions\");
                        downurl.Append(selectver).Append("\\");
                        downurl.Append(selectver).Append(".jar");
#if DEBUG
                        MessageBox.Show(downpath + "\n" + downurl);
#endif
                        if (!Directory.Exists(Path.GetDirectoryName(downpath.ToString())))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(downpath.ToString()));
                        }
                        string downjsonpath;
                        DownloadJson(ver, downer, taskbar, out downjsonpath);
                        await TaskEx.Delay(TimeSpan.FromMilliseconds(500));
                        taskbar.log(Logger.HelpLog(string.Format("Start reading json of version {0} for further downloading", selectver)));
                        var sr = new StreamReader(downjsonpath);
                        VersionJson verjson = JsonConvert.DeserializeObject<VersionJson>(sr.ReadToEnd());
                        sr.Close();
                        var sw = new StreamWriter(downjsonpath);
                        sw.Write(JsonConvert.SerializeObject(verjson, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
                        sw.Close();
                        try
                        {
                            downer.DownloadFileCompleted += delegate (object sender, AsyncCompletedEventArgs e)
                            {
                                taskbar.log(Logger.HelpLog("Success to download client file."));
                                taskbar.noticeFinished();
                            };
                            downer.DownloadProgressChanged += delegate (object sender, DownloadProgressChangedEventArgs e)
                            {
                                Dispatcher.Invoke(new Action(() => taskbar.setTaskStatus(e.ProgressPercentage + "%")));
                            };
                            
                            if (verjson.downloads?.client?.url != null)
                            {
                                downurl.Clear().Append(verjson.downloads.client.url);
                            }
                            taskbar.log(Logger.HelpLog("Start download file from url " + downurl.ToString()));
                            downer.DownloadFileAsync(new Uri(downurl.ToString()), downpath.ToString());
                        }
                        catch (Exception ex)
                        {
                            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate
                            {
                                Dispatcher.Invoke(new Action(() => MeCore.MainWindow.addNotice(new Notice.CrashErrorBar(string.Format(LangManager.GetLocalized("ErrorNameFormat"), DateTime.Now.ToLongTimeString()), ex.ToWellKnownExceptionString()) { ImgSrc = new BitmapImage(new Uri("pack://application:,,,/Resources/error-banner.jpg")) })));
                                taskbar.noticeFailed();
                            }));
                        }
                    }
                }));
                MeCore.MainWindow.addTask("dl-mcclient-" + ver.id, taskbar.setThread(task).setTask(LangManager.GetLocalized("TaskDLMC")).setDetectAlive(false));
                MeCore.MainWindow.addBalloonNotice(new Notice.NoticeBalloon(LangManager.GetLocalized("Download"), string.Format(LangManager.GetLocalized("BalloonNoticeSTTaskFormat"), LangManager.GetLocalized("TaskDLMC"))));
            
        }

        internal static void DownloadJson(RemoteVerType selectver, WebClient downloader, TaskListBar taskbar, out string downjsonfile)
        {
            var downpath = new StringBuilder(MeCore.Config.MCPath + @"\versions\");
            downpath.Append(selectver.id).Append("\\");
            downpath.Append(selectver.id).Append(".json");
            var downurl = new StringBuilder(MTMCL.Resources.UrlReplacer.getDownloadUrl());
            downurl.Append(@"versions\");
            downurl.Append(selectver.id).Append("\\");
            downurl.Append(selectver.id).Append(".json");
            downjsonfile = downurl.ToString();
            if (!string.IsNullOrWhiteSpace(selectver.url) & MeCore.Config.DownloadSource == 0)
            {
                downjsonfile = selectver.url;
            }
            string downjsonpath = downpath.ToString();
            taskbar.log(Logger.HelpLog("Start download file from url " + downjsonfile));
            try
            {
                downloader.DownloadFile(new Uri(downjsonfile), downjsonpath);
            }
            catch (Exception)
            {
                taskbar.noticeFailed();
                return;
            }
            taskbar.log(Logger.HelpLog("Finish downloading file " + downjsonfile));
        }

        private void butDL_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is RemoteVerType version)
                    DownloadMinecraft(version);
        }

        private void filter_Click(object sender, RoutedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(listRemoteVer.ItemsSource).Refresh();
        }

        public async Task<bool> DirectDownload(string version) {
            //if (!LoadedVersion) System.Threading.Tasks.Task.Factory.StartNew(ReloadVanillaVersion).Wait();
            while (!LoadedVersion)
            {
                await TaskEx.Delay(1000);
            }
            if (readthread != null) readthread.Wait(1000);
            System.Collections.IEnumerable l = null;
            while (l == null)
            {
                l = listRemoteVer.ItemsSource;
                await TaskEx.Delay(1000);
            }
            IEnumerable<RemoteVerType> list = l.OfType<RemoteVerType>();
            try
            {
                RemoteVerType rv = list.Single(ver => ver.id.Equals(version));
                if (rv != null) {
                    var result = await MeCore.MainWindow.ShowMessageAsync("", string.Format(LangManager.GetLocalized("DownloadConfirm"), "Minecraft " + rv.id),MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings() { AffirmativeButtonText = LangManager.GetLocalized("Yes"), NegativeButtonText = LangManager.GetLocalized("No")});
                    if (result == MessageDialogResult.Affirmative)
                    {
                        DownloadMinecraft(rv);
                        return true;
                    }
                    return false;
                }
                await MeCore.MainWindow.ShowMessageAsync(LangManager.GetLocalized("Oops"), LangManager.GetLocalized("NoVersionFound1"));
                return false;
            }
            catch (Exception)
            {
                await MeCore.MainWindow.ShowMessageAsync(LangManager.GetLocalized("Oops"), LangManager.GetLocalized("NoVersionFound1"));
                return false;
            }
        }
    }
}
