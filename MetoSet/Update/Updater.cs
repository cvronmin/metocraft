﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MTMCL.Update
{
    public class CheckUpdateFinishEventArgs : EventArgs {
        public bool UpdateAvailable { get; set; }
        public string UpdateAddress { get; set; }
        public string UpdateInfo { get; set; }
        public int UpdateBuild { get; set; }
    }
    public class Updater
    {
        public delegate void CheckFinishEventHandler(object sender, CheckUpdateFinishEventArgs e);

        public event CheckFinishEventHandler CheckFinishEvent;

        protected virtual void OnCheckFinishEvent(bool hasupdate, string updateaddr, string updateinfo, int updateBuild)
        {
            CheckFinishEventHandler handler = CheckFinishEvent;
            if (handler != null) MeCore.Invoke(new Action(() => handler(this, new CheckUpdateFinishEventArgs() { UpdateAvailable = hasupdate, UpdateAddress = updateaddr, UpdateInfo = updateinfo, UpdateBuild = updateBuild })));
        }
        private const string CheckFile = @"http://cvronmin.github.io/mtmcl-version.json";
        private const string CheckFile2 = @"http://cvrunmin.coding.me/cvrunmin/mtmcl-version.json";
        public bool HasUpdate { get; private set; }
        public string DownloadUrl { get; private set; }
        public Updater()
        {
            var thread = new Thread(Run);
            thread.Start();
        }
        private void Run()
        {
            try
            {
                CheckUpdate(MeCore.Config.UpdateSource == 1 ? CheckFile2 : CheckFile);
            }
            catch (Exception e)
            {
                Logger.log(e);
                try
                {
                    CheckUpdate(MeCore.Config.UpdateSource == 1 ? CheckFile : CheckFile2);
                }
                catch (Exception e1)
                {
                    Logger.log(e1);
                    HasUpdate = false;
                }

            }
        }
        private void CheckUpdate(string url)
        {
            string[] builds = MeCore.version.Split('.');
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "GET";
            req.Timeout = 7500;
            req.ReadWriteTimeout = 7500;
            var res = (HttpWebResponse)req.GetResponse();
            UpdateJson updatejs = Newtonsoft.Json.JsonConvert.DeserializeObject<UpdateJson>(new StreamReader(res.GetResponseStream()).ReadToEnd());

            if (updatejs == null | updatejs.versions.Length == 0)
            {
                Logger.log("failed to deserialize update json", Logger.LogType.Error);
                HasUpdate = false;
                return;
            }
            foreach (var item in updatejs.versions)
            {
                if (item.version.Equals(MeCore.Config.SearchLatest ? updatejs.specific_version.latest : updatejs.specific_version.recommand))
                {
                    string[] latest = item.version.Split('.');
                    if (Convert.ToInt32(builds[0]) < Convert.ToInt32(latest[0]))
                    {
                        HasUpdate = true;
                        DownloadUrl = updatejs.versions[0].url;
                        Logger.log("new major version found, version is: " + updatejs.versions[0].version);
                        Logger.log("download url is: " + DownloadUrl);
                    }
                    else if (Convert.ToInt32(builds[1]) < Convert.ToInt32(latest[1]))
                    {
                        HasUpdate = true;
                        DownloadUrl = updatejs.versions[0].url;
                        Logger.log("new version found, version is: " + updatejs.versions[0].version);
                        Logger.log("download url is: " + DownloadUrl);
                    }
                    else if (Convert.ToInt32(builds[2]) < Convert.ToInt32(latest[2]))
                    {
                        HasUpdate = true;
                        DownloadUrl = updatejs.versions[0].url;
                        Logger.log("new minor version found, version is: " + updatejs.versions[0].version);
                        Logger.log("download url is: " + DownloadUrl);
                    }
                    else if (Convert.ToInt32(builds[3]) < Convert.ToInt32(latest[3]))
                    {
                        HasUpdate = true;
                        DownloadUrl = updatejs.versions[0].url;
                        Logger.log("new build found, version is: " + updatejs.versions[0].version);
                        Logger.log("download url is: " + DownloadUrl);
                    }
                    else
                    {
                        HasUpdate = false;
                        Logger.log("no update.");
                    }
                    OnCheckFinishEvent(HasUpdate, DownloadUrl, item.info, Convert.ToInt32(latest[3]));
                    break;
                }
            }
        }
    }
}
