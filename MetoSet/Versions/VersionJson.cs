﻿using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System;
using Newtonsoft.Json;
using System.ComponentModel;

namespace MTMCL.Versions
{
    [DataContract]
    public class SimplifyVersionJson
    {
        [DataMember]
        public string assets{ get; set; }
        [DataMember]
        public string id{ get; set; }
        [DataMember]
        public string mainClass{ get; set; }
        [DataMember]
        public string minecraftArguments{ get; set; }
        [DataMember]
        public int minimumLauncherVersion{ get; set; }
        [DataMember]
        public string releaseTime{ get; set; }
        [IgnoreDataMember]
        [JsonIgnore]
        public DateTime relTime { get
            {
                return DateTime.Parse(releaseTime);
            }
        }
        [DataMember]
        public string time{ get; set; }
        [DataMember]
        public string type{ get; set; }
        [DataMember]
        public string inheritsFrom{ get; set; }
        [DataMember]
        public AssetIndex assetIndex{ get; set; }
        [DataMember]
        public Downloads downloads{ get; set; }
        [DataMember]
        public Library[] libraries{ get; set; }
        [DataContract]
        public class Library
        {
            [DataMember]
            public string name{ get; set; }
            [DataMember]
            public string url{ get; set; }
            [DataMember]
            public Rule[] rules{ get; set; }
            [DataContract]
            public class Rule
            {
                [DataMember]
                public string action{ get; set; }
                [DataMember]
                public OS os{ get; set; }
                [DataContract]
                public class OS {[DataMember]public string name{ get; set; } }
            }
            [DataMember]
            public Extract extract{ get; set; }
            [DataContract]
            public class Extract
            {
                [DataMember]
                public string[] exclude{ get; set; }
            }
            [DataMember]
            public Native natives{ get; set; }
            [DataContract]
            public class Native
            {
                [DataMember]
                public string linux{ get; set; }
                [DataMember]
                public string osx{ get; set; }
                [DataMember]
                public string windows{ get; set; }
            }
        }
        [DataContract]
        public class AssetIndex
        {
            [DataMember]
            public bool known{ get; set; }
            [DataMember]
            public string id{ get; set; }
            [DataMember]
            public string sha1{ get; set; }
            [DataMember]
            public int size{ get; set; }
            [DataMember]
            public string url{ get; set; }
            [DataMember]
            public int totalSize{ get; set; }
        }
    }
    [DataContract]
    public class VersionJson
    {
        [IgnoreDataMember]
        [JsonIgnore]
        public bool Error { get; set; }
        [IgnoreDataMember]
        [JsonIgnore]
        public bool BaseErrored { get; set; }
        [DataMember]
        public string assets{ get; set; }
        [DataMember]
        public string id{ get; set; }
        [DataMember]
        public string jar{ get; set; }
        [DataMember]
        public string mainClass{ get; set; }
        [DataMember]
        public string minecraftArguments{ get; set; }
        [DataMember]
        public int minimumLauncherVersion{ get; set; }
        [DataMember]
        public string releaseTime{ get; set; }
        [IgnoreDataMember]
        [JsonIgnore]
        public DateTime relTime
        {
            get
            {
                return DateTime.Parse(releaseTime);
            }
        }
        [DataMember]
        public string time{ get; set; }
        [DataMember]
        public string type{ get; set; }
        [DataMember]
        public string inheritsFrom{ get; set; }
        [DataMember]
        public AssetIndex assetIndex{ get; set; }
        [DataMember]
        public Downloads downloads{ get; set; }
        [DataMember]
        public Library[] libraries{ get; set; }
        [DataContract]
        public class Library : SimplifyVersionJson.Library
        {
            [DataMember]
            public Downloads download{ get; set; }
            [DataContract]
            public class Downloads
            {
                [DataMember]
                public Artifact artifact{ get; set; }
                [DataContract]
                public class Artifact
                {
                    [DataMember]
                    public int size{ get; set; }
                    [DataMember]
                    public string sha1{ get; set; }
                    [DataMember]
                    public string path{ get; set; }
                    [DataMember]
                    public string url{ get; set; }
                }
                [DataMember]
                public Classifier classifiers{ get; set; }
                [DataContract]
                public class Classifier
                {
                    [DataMember(Name = "natives-linux")]
                    [JsonProperty("natives-linux")]
                    public Artifact natives_linux{ get; set; }
                    [DataMember(Name = "natives-osx")]
                    [JsonProperty("natives-osx")]
                    public Artifact natives_osx{ get; set; }
                    [DataMember(Name = "natives-windows")]
                    [JsonProperty("natives-windows")]
                    public Artifact natives_windows{ get; set; }
                }
            }
        }
        [DataContract]
        public class AssetIndex
        {
            [DataMember]
            public bool known{ get; set; }
            [DataMember]
            public string id{ get; set; }
            [DataMember]
            public string sha1{ get; set; }
            [DataMember]
            public int size{ get; set; }
            [DataMember]
            public string url{ get; set; }
            [DataMember]
            public int totalSize{ get; set; }
        }
    }
    [DataContract]
    public class Downloads
    {
        [DataMember]
        public Side client{ get; set; }
        [DataMember]
        public Side server{ get; set; }
        [DataContract]
        public class Side
        {
            [DataMember]
            public string sha1{ get; set; }
            [DataMember]
            public int size{ get; set; }
            [DataMember]
            public string url{ get; set; }
        }
    }
    public class LibraryUniversal
    {
        public string name { get; set; }
        public string sha1 { get; set; }
        public int size { get; set; }
        public string path { get; set; }
        public string url { get; set; }
        public bool isNative { get; set; }
        public SimplifyVersionJson.Library.Extract extract { get; set; }
    }

    public static class VersionJsonController
    {
        public static SimplifyVersionJson Simplify(this VersionJson json)
        {
            SimplifyVersionJson simjson = new SimplifyVersionJson
            {
                id = json.id,
                type = json.type,
                assetIndex = json.assetIndex == null ? null : new SimplifyVersionJson.AssetIndex
                {
                    id = json.assetIndex.id,
                    sha1 = json.assetIndex.sha1,
                    size = json.assetIndex.size,
                    totalSize = json.assetIndex.totalSize,
                    url = json.assetIndex.url,
                    known = true
                },
                assets = json.assets,
                downloads = json.downloads,
                libraries = json.libraries.Simplify(),
                mainClass = json.mainClass,
                minecraftArguments = json.minecraftArguments,
                minimumLauncherVersion = json.minimumLauncherVersion,
                time = json.time,
                releaseTime = json.releaseTime
            };
            return simjson;
        }
        public static SimplifyVersionJson.Library[] Simplify(this VersionJson.Library[] list)
        {
            SimplifyVersionJson.Library[] simlist = new SimplifyVersionJson.Library[list.Length];
            for (int i = 0; i < list.Length; i++)
            {
                simlist[i] = new SimplifyVersionJson.Library
                {
                    name = list[i].name,
                    extract = list[i].extract,
                    natives = list[i].natives,
                    rules = list[i].rules
                };
            }
            return simlist;
        }
        public static void BackupJson(string file, VersionJson json)
        {
            file = file + ".bak";
            TextWriter writer = File.CreateText(file);
            writer.Write(JsonConvert.SerializeObject(json, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
            writer.Close();
        }
        /*public static List<LibraryUniversal> ToUniversalLibrary(this List<Library> libs)
        {
            List<LibraryUniversal> liblist = new List<LibraryUniversal>();
            foreach (var item in libs)
            {
                liblist.Add(new LibraryUniversal
                {
                    name = item.NS + ":" + item.Name + ":" + item.Version,
                    path = App.core.GetLibPath(item),
                    url = item.Url
                });
            }
            return liblist;
        }
        public static List<LibraryUniversal> ToUniversalLibrary(this List<Native> libs)
        {
            List<LibraryUniversal> liblist = new List<LibraryUniversal>();
            foreach (var item in libs)
            {
                liblist.Add(new LibraryUniversal
                {
                    name = item.NS + ":" + item.Name + ":" + item.Version,
                    path = App.core.GetNativePath(item),
                    url = item.Url
                });
            }
            return liblist;
        }*/
        public static List<LibraryUniversal> ToUniversalLibrary(this SimplifyVersionJson.Library[] libs)
        {
            List<LibraryUniversal> liblist = new List<LibraryUniversal>();
            foreach (var item in libs)
            {
                if (!IsAllowed(item.rules))
                {
                    continue;
                }
                string[] name = item.name.Split(':');
                if (item.natives != null)
                {
                    liblist.Add(new LibraryUniversal
                    {
                        name = item.name,
                        path = Path.Combine(MeCore.Config.MCPath, "libraries", name[0].Replace(".", "\\"), name[1], name[2], name[1] + "-" + name[2] + "-" + item.natives.windows.Replace("${arch}", Environment.Is64BitOperatingSystem ? "64" : "32") + ".jar"),
                        url = item.url,
                        isNative = true,
                        extract = item.extract
                    });
                }
                else
                {
                    liblist.Add(new LibraryUniversal
                    {
                        name = item.name,
                        path = Path.Combine(MeCore.Config.MCPath, "libraries", name[0].Replace(".", "\\"), name[1], name[2], name[1] + "-" + name[2] + ".jar"),
                        url = item.url
                    });
                }
            }
            return liblist;
        }
        public static List<LibraryUniversal> ToUniversalLibrary(this VersionJson.Library[] libs)
        {
            List<LibraryUniversal> liblist = new List<LibraryUniversal>();
            foreach (var item in libs)
            {
                if (!IsAllowed(item.rules))
                {
                    continue;
                }
                if (item.download != null)
                {
                    if (item.download.classifiers != null)
                    {
                        liblist.Add(new LibraryUniversal
                        {
                            name = item.name,
                            path = Path.Combine(MeCore.Config.MCPath, "libraries", item.download.classifiers.natives_windows.path),
                            url = item.download.classifiers.natives_windows.url,
                            sha1 = item.download.classifiers.natives_windows.sha1,
                            size = item.download.classifiers.natives_windows.size,
                            isNative = true,
                            extract = item.extract
                        });
                    }
                    else if (item.download.artifact != null)
                    {
                        liblist.Add(new LibraryUniversal
                        {
                            name = item.name,
                            path = Path.Combine(MeCore.Config.MCPath, "libraries", item.download.artifact.path),
                            url = item.download.artifact.url,
                            sha1 = item.download.artifact.sha1,
                            size = item.download.artifact.size
                        });
                    }
                }
                else
                {
                    liblist.AddRange(ToUniversalLibrary((SimplifyVersionJson.Library[])libs));
                    break;
                }
            }
            return liblist;
        }
        public static bool IsAllowed(SimplifyVersionJson.Library.Rule[] rules)
        {
            if (rules == null)
            {
                return true;
            }
            if (rules.Length == 0)
            {
                return true;
            }
            var allowed = false;
            foreach (var rule in rules)
            {
                if (rule.os == null)
                {
                    allowed = rule.action == "allow";
                    continue;
                }
                if (rule.os.name == "windows")
                {
                    allowed = rule.action == "allow";
                }
            }
            return allowed;
        }
    }
}
