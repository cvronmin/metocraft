﻿using MTMCL.Forge;
using MTMCL.JsonClass;
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
    public partial class GridForgeDLMinor : Grid
    {
        GridForgeDLMain parent;
        public string mcversion { get; private set; }
        public GridForgeDLMinor ()
        {
            InitializeComponent();
        }

        public GridForgeDLMinor(GridForgeDLMain parent, string mcver) : this() {
            this.parent = parent;
            mcversion = mcver;
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

        private void Grid_Initialized(object sender, EventArgs e)
        {
            ReloadForgeVersion();
        }
        private void ReloadForgeVersion () {
            var fl = parent._forgeVer.GetForgeVersions(mcversion);
            listRemoteVer.ItemsSource = fl;
            listRemoteVer.Items.SortDescriptions.Add(new SortDescription("version", ListSortDirection.Descending));
        }
        private void DownloadForge (ForgeVersion ver)
        {
            TaskListBar task = new TaskListBar() { /*ImgSrc = new BitmapImage(new Uri("pack://application:,,,/Resources/download-banner.jpg"))*/ Icon = Application.Current.Resources["task_download_icon"] as Visual };
            var thDL = new Thread(new ThreadStart(delegate
            {
                bool universalInstead = !ver.urls.ContainsKey("installer");
                bool clientInstead = universalInstead & !ver.urls.ContainsKey("universal");
                var url = new Uri(clientInstead ? ver.urls["client"] : universalInstead ? ver.urls["universal"] : ver.urls["installer"]);
                using (var downer = new WebClient())
                {
                    downer.Headers.Add("User-Agent", "MTMCL" + MeCore.version);
                    var filename = !universalInstead ? "forge.jar" : "forge.zip";
                    var filecount = 0;
                    while (File.Exists(filename))
                    {
                        ++filecount;
                        filename = "forge-" + filecount + (!universalInstead ? ".jar" : ".zip");
                    }
                    downer.DownloadProgressChanged += delegate (object sender, DownloadProgressChangedEventArgs e)
                    {
                        MeCore.Invoke(new Action(() => task.setTaskStatus(string.Format(LangManager.GetLocalized("SubTaskDLForge"), e.ProgressPercentage))));
                    };
                    downer.DownloadFileCompleted += delegate (object sender, AsyncCompletedEventArgs e)
                    {
                        try
                        {
                            task.log(Logger.HelpLog("Trying to install forge"));
                            MeCore.Invoke(new Action(() => task.setTaskStatus(LangManager.GetLocalized("SubTaskInstallForge"))));
                            if (universalInstead) new ForgeInstaller().installOld(filename, ver.mcversion, "forge-" + ver.mcversion + "-" + ver.version + (!string.IsNullOrWhiteSpace(ver.branch) ? "-" + ver.branch : ""));
                            else new ForgeInstaller().install(filename);
                            File.Delete(filename);
                            task.log(Logger.HelpLog("Installation finished"));
                            MeCore.Invoke(new Action(() => task.setTaskStatus(LangManager.GetLocalized("TaskFinish"))));
                            task.noticeFinished();
                        }
                        catch (Exception ex)
                        {
                            task.log(Logger.HelpLog("Installation failed"));
                            Logger.log(ex);
                            task.noticeFailed();
                        }
                    };
                    task.log(Logger.HelpLog("Start downloading forge installer"));
                    MeCore.Invoke(new Action(() => task.setTaskStatus(string.Format(LangManager.GetLocalized("SubTaskDLForge"), "0"))));
                    downer.DownloadFileAsync(url, filename);
                }
            }));
            MeCore.MainWindow.addTask("dl-instl-forgeclient-" + ver, task.setThread(thDL).setTask(LangManager.GetLocalized("TaskInstallForge")).setDetectAlive(false));
            MeCore.MainWindow.addBalloonNotice(new Notice.NoticeBalloon(LangManager.GetLocalized("Download"), string.Format(LangManager.GetLocalized("BalloonNoticeSTTaskFormat"), LangManager.GetLocalized("TaskInstallMC"))));
        }
        private void butDL_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ForgeVersion version)
                    DownloadForge(version);
        }

    }
}
