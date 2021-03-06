﻿using MTMCL.Versions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Windows;
using System.Reflection;
using System.Resources;
using System.Linq;
using MTMCL.Launch;
using MTMCL.Task;
using MTMCL.Threads;
using MTMCL.Lang;
using MTMCL.Notice;
using MTMCL.Assets;
using System.Windows.Media;

namespace MTMCL.util
{
    public static class FileHelper
    {
        static public void dircopy(string from, string to, bool force = false)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(from);
                if (!Directory.Exists(to))
                {
                    Directory.CreateDirectory(to);
                }
                else
                {
                    if (force)
                    {
                        Directory.Delete(to, true);
                    }
                }
                foreach (DirectoryInfo sondir in dir.GetDirectories())
                {
                    dircopy(sondir.FullName, to + "\\" + sondir.Name);
                }
                foreach (FileInfo file in dir.GetFiles())
                {
                    File.Copy(file.FullName, to + "\\" + file.Name, true);
                }
            }
            catch(Exception e)
            {
                Logger.log(e);
            }
        }
        static public void dirmove(string to, string from, bool force = false)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(from);
                if (!Directory.Exists(to))
                {
                    Directory.CreateDirectory(to);
                }
                else
                {
                    if (force)
                    {
                        Directory.Delete(to, true);
                    }
                }
                foreach (DirectoryInfo sondir in dir.GetDirectories())
                {
                    dircopy(sondir.FullName, to + "\\" + sondir.Name);
                }
                foreach (FileInfo file in dir.GetFiles())
                {
                    File.Copy(file.FullName, to + "\\" + file.Name, true);
                }
                Directory.Delete(from, true);
            }
            catch(Exception e)
            {
                Logger.log(e);
            }
        }

        static public bool IfFileVaild(string Path, long Length = -1, string sha1 = "")
        {
            if (!File.Exists(Path))
            {
                return false;
            }
            if (new FileInfo(Path).Length == 0)
            {
                return false;
            }
            if (Length > 0)
            {
                if (new FileInfo(Path).Length != Length)
                {
                    return false;
                }
            }
            if (!string.IsNullOrWhiteSpace(sha1))
            {
                if (!hashSHA1(File.ReadAllBytes(Path)).Equals(sha1))
                {
                    return false;
                }
            }
            return true;
        }
        private static string hashSHA1(byte[] data)
        {
            byte[] hashed = System.Security.Cryptography.SHA1.Create().ComputeHash(data);
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < hashed.Length; i++)
            {
                builder.Append(hashed[i].ToString());
            }
            return builder.ToString();
        }
        static public void CreateDirectoryIfNotExist(string Dir)
        {
            if (!Directory.Exists(Dir))
            {
                Directory.CreateDirectory(Dir);
            }
        }

        static public void CreateDirectoryForFile(string File)
        {
            CreateDirectoryIfNotExist(Path.GetDirectoryName(File));
        }

        public static bool ResourceExists(string resourcePath)
        {
            var assembly = Assembly.GetExecutingAssembly();

            return ResourceExists(assembly, resourcePath);
        }

        public static bool ResourceExists(Assembly assembly, string resourcePath)
        {
            return GetResourcePaths(assembly)
                .Contains(resourcePath.ToLowerInvariant());
        }

        public static IEnumerable<object> GetResourcePaths(Assembly assembly)
        {
            var culture = System.Threading.Thread.CurrentThread.CurrentCulture;
            var resourceName = assembly.GetName().Name + ".g";
            var resourceManager = new ResourceManager(resourceName, assembly);

            try
            {
                var resourceSet = resourceManager.GetResourceSet(culture, true, true);

                foreach (DictionaryEntry resource in resourceSet)
                {
                    yield return resource.Key;
                }
            }
            finally
            {
                resourceManager.ReleaseAllResources();
            }
        }

        public static bool AssetsExist(KeyValuePair<string, AssetsEntity> entity)
        {
            string path = MeCore.Config.MCPath;
            if (MeCore.IsServerDedicated && !string.IsNullOrWhiteSpace(MeCore.Config.Server.ClientPath))
            {
                path = path.Replace(MeCore.Config.MCPath, Path.Combine(MeCore.BaseDirectory, MeCore.Config.Server.ClientPath));
            }
            return File.Exists(path + @"\assets\objects\" + entity.Value.Hash.Substring(0, 2) + @"\" + entity.Value.Hash);
        }
    }
    public static class TimeHelper
    {
        static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        static readonly double MaxUnixSeconds = (DateTime.MaxValue - UnixEpoch).TotalSeconds;

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            return unixTimeStamp > MaxUnixSeconds
               ? UnixEpoch.AddMilliseconds(unixTimeStamp)
               : UnixEpoch.AddSeconds(unixTimeStamp);
        }
    }
    public static class ContentHelper
    {
        public static void AddContentFromSpecficString(this System.Windows.Controls.TextBlock textblock, string spstring)
        {
            string[] strings = spstring.Split('[', ']');
            bool recordBold = false;
            StringBuilder st = new StringBuilder();
            for (int i = 0; i < strings.Length; i++)
            {
                switch (strings[i])
                {
                    case "/bold/":
                        recordBold = true;
                        break;
                    case "/endbold/":
                        if (recordBold)
                        {
                            recordBold = false;
                            textblock.Inlines.Add(new System.Windows.Documents.Run(st.ToString()) { FontWeight = System.Windows.FontWeights.Bold });
                            st.Clear();
                        }
                        break;
                    case "/newline/":
                    case "/n/":
                        textblock.Inlines.Add(new System.Windows.Documents.LineBreak());
                        break;
                    case "/paragraph/":
                    case "/p/":
                        textblock.Inlines.Add(new System.Windows.Documents.LineBreak());
                        textblock.Inlines.Add(new System.Windows.Documents.LineBreak());
                        break;
                    default:
                        if (!recordBold)
                        {
                            textblock.Inlines.Add(strings[i]);
                        }
                        else
                        {
                            st.Append(strings[i]);
                        }
                        break;
                }
            }

        }

        public static string FromSpecficString(this string spstring)
        {
            string[] strings = spstring.Split('[', ']');
            StringBuilder st = new StringBuilder();
            for (int i = 0; i < strings.Length; i++)
            {
                switch (strings[i])
                {
                    case "/bold/":
                        break;
                    case "/newline/":
                    case "/n/":
                        st.AppendLine();
                        break;
                    case "/paragraph/":
                    case "/p/":
                        st.AppendLine().AppendLine();
                        break;
                    default:
                        st.Append(strings[i]);
                        break;
                }
            }
            return st.ToString();
        }

        public static string ToWellKnownExceptionString(this Exception ex) {
            var message = new StringBuilder();
            message.AppendLine("MTMCL, version " + MeCore.version);
            message.AppendFormat("Target Site: {0}", ex.TargetSite).AppendLine();
            message.AppendFormat("Error Type: {0}", ex.GetType()).AppendLine();
            message.AppendFormat("Messge: {0}", ex.Message).AppendLine();
            foreach (DictionaryEntry data in ex.Data)
                message.AppendLine(string.Format("Key:{0}\nValue:{1}", data.Key, data.Value));
            message.AppendLine("StackTrace");
            message.AppendLine(ex.StackTrace);
            var iex = ex;
            while (iex.InnerException != null)
            {
                message.AppendLine("------------Inner Exception------------");
                iex = iex.InnerException;
                message.AppendFormat("Target Site: {0}", ex.TargetSite).AppendLine();
                message.AppendFormat("Error Type: {0}", ex.GetType()).AppendLine();
                message.AppendFormat("Messge: {0}", ex.Message).AppendLine();
                foreach (DictionaryEntry data in ex.Data)
                    message.AppendLine(string.Format("Key:{0}\nValue:{1}", data.Key, data.Value));
                message.AppendLine("StackTrace");
                message.AppendLine(iex.StackTrace);
            }
            message.AppendLine("\n\n-----------------MTMCL LOG----------------------\n");
            try
            {
                var sr = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "mtmcl.log");
                message.AppendLine(sr.ReadToEnd());
                sr.Close();
            }
            catch (IOException)
            {
                message.AppendLine("unable to read log");
            }
            return message.ToString();
        }
        public static char[] RandomizeABCNO(int? seed = null)
        {
            Random rand = seed != null ? new Random((int)seed) : new Random();
            List<char> alist = new List<char>();
            while (alist.Count < 62)
            {
                char c = (char)(rand.Next(75) + 48);
                if (char.IsLetterOrDigit(c))
                {
                    if (!alist.Contains(c))
                    {
                        alist.Add(c);
                    }
                }
            }
            return alist.ToArray();
        }
        public static readonly char[] refer = new char[] {
                'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
                'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
                '0','1','2','3','4','5','6','7','8','9'
            };
        public static string Encrystal(this string src)
        {
            char[] encode = RandomizeABCNO();

            char[] enchant = new char[src.Length];
            for (int i = 0; i < encode.Length; i++)
            {
                int j = -1;
                while ((j = src.IndexOf(refer[i], j == -1 ? 0 : j)) != -1)
                {
                    enchant[j] = encode[i];
                    j++;
                }
            }
            return Encoding.Default.GetString(Encoding.Default.GetBytes(encode)) + Encoding.Default.GetString(Encoding.Default.GetBytes(enchant));
        }
        public static string Decrystal(this string src)
        {
            char[] encode = src.Substring(0, 62).ToCharArray();
            string yummystring = src.Substring(62);
            char[] enchant = new char[yummystring.Length];
            for (int i = 0; i < refer.Length; i++)
            {
                int j = -1;
                while ((j = yummystring.IndexOf(encode[i], j == -1 ? 0 : j)) != -1)
                {
                    enchant[j] = refer[i];
                    j++;
                }
            }
            return Encoding.Default.GetString(Encoding.Default.GetBytes(enchant));
        }
        public static string ToHyphenizedUUID(this string src)
        {
            var a = new string[] { src.Substring(0,8), src.Substring(8,4), src.Substring(12,4), src.Substring(16, 4),src.Substring(20, 12)};
            return a[0] + "-" + a[1] + "-" + a[2] + "-" + a[3] + "-" + a[4]; 
        }
        public static string ToNonHyphenizedUUID(this string src)
        {
            var a = src.Split('-');
            return a[0] + a[1] + a[2] + a[3] + a[4];
        }
        /// <summary>
        /// reverse keys and values
        /// </summary>
        /// <typeparam name="K">Key</typeparam>
        /// <typeparam name="V">Value</typeparam>
        /// <param name="dict">Dictionary to be reverse</param>
        /// <returns>Reversed Dictionary</returns>
        public static Dictionary<V,K> KVReverse<K,V>(this Dictionary<K,V> dict) {
            Dictionary<V, K> result = new Dictionary<V, K>();
            foreach (var pair in dict) {
                if (result.ContainsKey(pair.Value)) continue;
                result.Add(pair.Value, pair.Key);
            }
            return result;
        }

        public static string GetCorrespondingJava(int javaver)
        {
            switch (javaver)
            {
                case 53:
                    return "Java SE 9";
                case 52:
                    return "Java SE 8";
                case 51:
                    return "Java SE 7";
                case 50:
                    return "Java SE 6.0";
                case 49:
                    return "Java SE 5.0";
                case 48:
                    return "JDK 1.4";
                case 47:
                    return "JDK 1.3";
                case 46:
                    return "JDK 1.2";
                case 45:
                    return "JDK 1.1";
                default:
                    return "";
            }
        }
    }
    public static class LibraryHelper
    {
        public static bool CheckLibrary(this VersionJson version)
        {
            try
            {
                var libs = version.libraries;
                foreach (var libflie in libs)
                {
                    if (!File.Exists(System.IO.Path.Combine(MeCore.Config.MCPath,libflie.name)))
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
    public static class OSHelper {
        public static string HKLM_GetString (string path, string key)
        {
            try
            {
                RegistryKey rk = Registry.LocalMachine.OpenSubKey(path);
                if (rk == null) return "";
                return (string) rk.GetValue(key);
            }
            catch { return ""; }
        }

        public static string GetOSName () {
            return HKLM_GetString(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductName");
        }
        public static string GetDotNETs () {
            var sb = new StringBuilder();
            using (RegistryKey ndpKey =
            RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, "").
            OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\"))
            {
                // As an alternative, if you know the computers you will query are running .NET Framework 4.5 
                // or later, you can use:
                // using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, 
                // RegistryView.Registry32).OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\"))
                foreach (string versionKeyName in ndpKey.GetSubKeyNames())
                {
                    if (versionKeyName.StartsWith("v"))
                    {

                        RegistryKey versionKey = ndpKey.OpenSubKey(versionKeyName);
                        string name = (string) versionKey.GetValue("Version", "");
                        string sp = versionKey.GetValue("SP", "").ToString();
                        string install = versionKey.GetValue("Install", "").ToString();
                        if (install == "") //no install info, must be later.
                            sb.AppendLine(".NET Framework " + versionKeyName + "  " + name);
                        else
                        {
                            if (sp != "" && install == "1")
                            {
                                sb.AppendLine(".NET Framework " + versionKeyName + "  " + name + "  SP" + sp);
                            }

                        }
                        if (name != "")
                        {
                            continue;
                        }
                        foreach (string subKeyName in versionKey.GetSubKeyNames())
                        {
                            RegistryKey subKey = versionKey.OpenSubKey(subKeyName);
                            name = (string) subKey.GetValue("Version", "");
                            if (name != "")
                                sp = subKey.GetValue("SP", "").ToString();
                            install = subKey.GetValue("Install", "").ToString();
                            if (install == "") //no install info, must be later.
                                sb.AppendLine(".NET Framework " + versionKeyName + "  " + name);
                            else
                            {
                                if (sp != "" && install == "1")
                                {
                                    sb.AppendLine("  " + subKeyName + "  " + name + "  SP" + sp);
                                }
                                else if (install == "1")
                                {
                                    sb.AppendLine("  " + subKeyName + "  " + name);
                                }
                            }
                        }
                    }
                }
                using (var key = ndpKey.OpenSubKey(@"v4\Full\")) {
                    if (key != null && key.GetValue("Release") != null)
                    {
                        sb.AppendLine(".NET Framework " + CheckFor45PlusVersion((int) key.GetValue("Release")));
                    }
                }
            }
            return sb.ToString();
        }
        private static string CheckFor45PlusVersion (int releaseKey)
        {
            if (releaseKey >= 394802)
                return "4.6.2 or later";
            if (releaseKey >= 394254)
            {
                return "4.6.1";
            }
            if (releaseKey >= 393295)
            {
                return "4.6";
            }
            if ((releaseKey >= 379893))
            {
                return "4.5.2";
            }
            if ((releaseKey >= 378675))
            {
                return "4.5.1";
            }
            if ((releaseKey >= 378389))
            {
                return "4.5";
            }
            // This code should never execute. A non-null release key should mean
            // that 4.5 or later is installed.
            return "No 4.5 or later version detected";
        }
        public static string GetDotNETUpdates () {
            var sb = new StringBuilder();
            using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(@"SOFTWARE\Microsoft\Updates"))
            {
                foreach (string baseKeyName in baseKey.GetSubKeyNames())
                {
                    if (baseKeyName.Contains(".NET Framework") || baseKeyName.StartsWith("KB") || baseKeyName.Contains(".NETFramework"))
                    {
                        using (RegistryKey updateKey = baseKey.OpenSubKey(baseKeyName))
                        {
                            string name = (string) updateKey.GetValue("PackageName", "");
                            sb.AppendLine(baseKeyName + "  " + name);
                            foreach (string kbKeyName in updateKey.GetSubKeyNames())
                            {
                                using (RegistryKey kbKey = updateKey.OpenSubKey(kbKeyName))
                                {
                                    name = (string) kbKey.GetValue("PackageName", "");
                                    sb.AppendLine("  " + kbKeyName + "  " + name);

                                    if (kbKey.SubKeyCount > 0)
                                    {
                                        foreach (string sbKeyName in kbKey.GetSubKeyNames())
                                        {
                                            using (RegistryKey sbSubKey = kbKey.OpenSubKey(sbKeyName))
                                            {
                                                name = (string) sbSubKey.GetValue("PackageName", "");
                                                if (name == "")
                                                    name = (string) sbSubKey.GetValue("Description", "");
                                                sb.AppendLine("    " + sbKeyName + "  " + name);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
            }
            return sb.ToString();
        }
    }

    public static class LaunchGameHelper {

        public static Tuple<LaunchGameInfo, Tuple<string, TaskListBar>, NoticeBalloon> LaunchGame (LaunchGameInfo options, LaunchMode mode)
        {
            if (options != null)
            {
                options.SetMode(mode);
                //string uri = "pack://application:,,,/Resources/play-normal-banner.jpg";
                TaskListBar gui = new TaskListBar() { /*ImgSrc = new BitmapImage(new Uri(uri)),*/ Icon = Application.Current.Resources["task_play_mc_icon"] as Visual };
                if (mode is BMCLLaunchMode)
                {
                    //uri = "pack://application:,,,/Resources/play-bmcl-banner.jpg";
                    gui.Icon = Application.Current.Resources["task_play_mc_bmcl_icon"] as Visual;
                }
                if (mode is BakaXLLaunchMode)
                {
                    //uri = "pack://application:,,,/Resources/play-bakaxl-banner.jpg";
                    gui.Icon = Application.Current.Resources["task_play_mc_bakaxl_icon"] as Visual;
                }
                var task = new LaunchMCThread(options);
                task.StateChange += delegate (string state)
                {
                    MeCore.Dispatcher.Invoke(new Action(() => gui.setTaskStatus(state)));
                };
                task.TaskCountTime += delegate
                {
                    if(MeCore.MainWindow != null)
                      MeCore.Dispatcher.Invoke(new Action(() => MeCore.MainWindow.butPlayQuick.IsEnabled = false));
                    MeCore.Dispatcher.Invoke(new Action(() => gui.countTime()));
                };
                task.GameExit += delegate
                {
                    if (MeCore.MainWindow != null)
                        MeCore.Dispatcher.Invoke(new Action(() => MeCore.MainWindow.butPlayQuick.IsEnabled = true));
                    MeCore.Dispatcher.Invoke(new Action(() => gui.stopCountTime().noticeFinished()));
                };
                task.OnLogged += delegate (string s) {
                    MeCore.Dispatcher.Invoke(new Action(() => gui.log(s)));
                };
                task.GameCrash += delegate (string content, string path)
                {
                    //new MCCrash(content, path).Show();
                };
                task.OnAuthUpdate += delegate (Launch.Login.AuthInfo info)
                {
                    try
                    {
                        MeCore.Config.SavedAuths[info.DisplayName].AccessToken = info.Session.ToString();
                    }
                    catch (Exception) { }
                };
                task.Failed += () => {
                    MeCore.Dispatcher.Invoke(new Action(() => gui.noticeFailed()));
                };
                gui = gui.setTask(string.Format(LangManager.GetLocalized("TaskLaunch"), options.Version.id)).setThread(task).setDetectAlive(false);
                var nb = new NoticeBalloon("MTMCL", string.Format(LangManager.GetLocalized("BalloonNoticeSTTaskFormat"), string.Format(LangManager.GetLocalized("TaskLaunch"), options.Version.id)));
                if (MeCore.MainWindow != null)
                {
                    MeCore.MainWindow.addTask("game", gui);
                    MeCore.MainWindow.addBalloonNotice(nb);
                }
                return new Tuple<LaunchGameInfo, Tuple<string, TaskListBar>, NoticeBalloon>(options,new Tuple<string, TaskListBar>("game",gui), nb);
            }
            return null;
        }

        public static Tuple<LaunchGameInfo, Tuple<string, TaskListBar>, NoticeBalloon> QuickLaunch () {
            try
            {
                if (!string.IsNullOrWhiteSpace(MeCore.Config.LastPlayVer))
                {
                    VersionJson version = VersionReader.GetFurtherVersion(MeCore.Config.MCPath, MeCore.Config.LastPlayVer);
                    if (version != null)
                    {
                        Launch.Login.IAuth auth;
                        if (string.IsNullOrWhiteSpace(MeCore.Config.DefaultAuth))
                        {
                            ACSelect ac = new ACSelect();
                            ac.ShowDialog();
                            auth = ac.auth;
                        }
                        else
                        {
                            SavedAuth dauth;
                            MeCore.Config.SavedAuths.TryGetValue(MeCore.Config.DefaultAuth, out dauth);
                            auth = dauth.AuthType.Equals("Yggdrasil") ? new Launch.Login.YggdrasilRefreshAuth(dauth.AccessToken) : new Launch.Login.AuthWarpper(new Launch.Login.AuthInfo { Pass = true, DisplayName = MeCore.Config.DefaultAuth, Session = dauth.AccessToken, UUID = dauth.UUID, UserType = dauth.UserType, Prop = dauth.Properies }) as Launch.Login.IAuth;
                        }
                        if (auth != null)
                        {
                            LaunchGameInfo option = LaunchGameInfo.CreateInfo(MeCore.Config.MCPath, auth, version, MeCore.Config.Javaw, (int) MeCore.Config.Javaxmx, CreateServerInfo());
                            return LaunchGame(option, LaunchMode.GetMode(MeCore.Config.LastLaunchMode));
                        }
                    }
                }
            }
            catch { }
            return null;
        }
        public static Launch.ServerInfo CreateServerInfo ()
        {
            if (MeCore.IsServerDedicated)
            {
                if (!string.IsNullOrWhiteSpace(MeCore.Config.Server.ServerIP))
                {
                    if (MeCore.Config.Server.ServerIP.IndexOf(':') != -1)
                    {
                        ushort port = 25565;
                        if (!ushort.TryParse(MeCore.Config.Server.ServerIP.Substring(MeCore.Config.Server.ServerIP.IndexOf(':')).Trim(':'), out port))
                        {
                            port = 25565;
                        }
                        return new Launch.ServerInfo
                        {
                            Ip = MeCore.Config.Server.ServerIP.Substring(0, MeCore.Config.Server.ServerIP.IndexOf(':')).Trim(':'),
                            Port = port
                        };
                    }
                }
            }
            return null;
        }
    }

    internal sealed class ImageConverter : IValueConverter
    {
        public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || parameter.Equals(string.Empty)) parameter = "{0}";
            string path = string.Format((string)parameter, value);
            if (!FileHelper.ResourceExists(path.Replace("pack://application:,,,/", "")))
                return DependencyProperty.UnsetValue;
            return new BitmapImage(new Uri(path));
        }

        public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal sealed class ColorFromAccentConverter : IValueConverter
    {
        public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || parameter.Equals(String.Empty)) parameter = "AccentBaseColor";

            try
            {
                MahApps.Metro.Accent accent = null;
                accent = (MahApps.Metro.Accent) value;
                System.Windows.Media.Color color;
                    if (!accent.Resources.Contains(parameter)) color = (System.Windows.Media.Color) accent.Resources["AccentColor"];
                    else color = (System.Windows.Media.Color) accent.Resources[parameter];
                return color;
            } catch (Exception ex){
                Exception ex1 = new ArgumentException("unexpected value, Accent prefered, argument type: " + value.GetType().Name, "value", ex);
#if DEBUG
                if(Logger.loaded)Logger.error(ex1);
#else
                throw ex1;
#endif
            }
            return null;
        }

        public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal sealed class InvertBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool)) return DependencyProperty.UnsetValue;
            if ((bool)value) return 0;
            else return 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    [ValueConversion(typeof(bool), typeof(bool))]
    internal sealed class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    internal sealed class ValuesValueFromPairConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var obj = value;
            if (value is KeyValuePair<object, object> pair)
            {
                obj = pair;
            }
            try
            {
                var b = obj.GetType().GetProperty((string)parameter, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                if (b != null) return b.GetValue(obj, null);
                var a = obj.GetType().GetField((string)parameter, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                if (a != null) return a.GetValue(obj);
                return DependencyProperty.UnsetValue;
            }
            catch (Exception)
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal sealed class VersionComparer : IComparer
    {
        public bool Descending { get; set; }
        public int Compare (object x, object y)
        {
            if (x is string && y is string) return Compare(x as string, y as string);
            if (x is Forge.ForgeMajorVersions && y is Forge.ForgeMajorVersions)
                return Compare((x as Forge.ForgeMajorVersions).Version, (y as Forge.ForgeMajorVersions).Version);
            return Comparer.Default.Compare(x, y);
        }

        public int Compare (string x, string y)
        {
            if (x.Equals(y)) return 0;
            var ax = x.Split('.');
            var ay = y.Split('.');
            for (int i=0;i< ax.Length || i< ay.Length;i++) {
                if (ax.Length <= i) return Descending ? 1 : -1;
                if (ay.Length <= i) return Descending ? -1 : 1;
                var rx = System.Text.RegularExpressions.Regex.Match(ax[i], @"(\d+)(_pre\d*)?");
                var ry = System.Text.RegularExpressions.Regex.Match(ay[i], @"(\d+)(_pre\d*)?");
                if (rx.Success && !string.IsNullOrWhiteSpace(rx.Groups[2].Value))
                {
                    if (int.Parse(rx.Groups[1].Value) > int.Parse(ry.Groups[1].Value))
                        return Descending ? -1 : 1;
                    else return Descending ? 1 : -1;
                }
                if (ry.Success && !string.IsNullOrWhiteSpace(ry.Groups[2].Value)) {
                    if (int.Parse(ry.Groups[1].Value) > int.Parse(rx.Groups[1].Value))
                        return Descending ? 1 : -1;
                    else return Descending ? -1 : 1;
                }
                short ix = Convert.ToInt16(ax[i]), iy = Convert.ToInt16(ay[i]);
                if (ix < iy) return Descending ? 1 : -1;
                if (ix > iy) return Descending ? -1 : 1;
            }
            return string.Compare(x, y);
        }
    }
}