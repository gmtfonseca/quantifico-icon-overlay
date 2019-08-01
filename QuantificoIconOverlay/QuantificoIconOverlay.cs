using Razorvine.Pickle;
using SharpShell.Interop;
using SharpShell.SharpIconOverlayHandler;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using System.Diagnostics;

namespace QuantificoIconOverlay
{
    static class Constants
    {
        public const string NfExtension = ".XML";
        public const string AppName = "googledrivesync";
        public const string SettingsFile = @"Quantifico/settings.json";
        public const string CloudFile = @"Quantifico/cloud.dat";
        public const string BlacklistedFile = @"Quantifico/blacklisted.dat";
    }

    class Settings
    {
        public string NfsDir { get; set; }
    }

    class App
    {
        private readonly string AppName;
        public App(string appName)
        {
            AppName = appName;
        }

        public bool IsRunning()
        {
            Process[] appProcesses = Process.GetProcessesByName(AppName);
            return appProcesses.Length > 0;
        }
    }

    abstract class NfFile
    {
        protected readonly string FilePath;

        public NfFile(string filePath)
        {
            FilePath = filePath;
        }

        public bool CanShowOverlay()
        {
            if (HasValidExtension())
            {
                if (InNfDir())
                {
                    return CanShowIcon();
                }
            }

            return false;
        }

        private bool HasValidExtension()
        {
            return Path.GetExtension(FilePath) == Constants.NfExtension;
        }

        private bool InNfDir()
        {
            var settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Constants.SettingsFile);

            using (StreamReader streamReader = new StreamReader(settingsPath))
            {
                string json = streamReader.ReadToEnd();
                Settings settings = JsonConvert.DeserializeObject<Settings>(json);

                if (settings.NfsDir != string.Empty)
                {
                    DirectoryInfo dir = new DirectoryInfo(FilePath);

                    return dir.Parent.FullName.Equals(settings.NfsDir);
                }                
            }

            return false;
        }

        protected abstract bool CanShowIcon();
    }

    class SyncedNfFile : NfFile
    {
        public SyncedNfFile(string filePath) : base(filePath) { }

        protected override bool CanShowIcon()
        {
            Unpickler unpickler = new Unpickler();
            string cloudFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Constants.CloudFile);
            byte[] bytes = System.IO.File.ReadAllBytes(cloudFilePath);
            object result = unpickler.loads(bytes);

            HashSet<object> set = (HashSet<object>)result;
            FileInfo fileInfo = new FileInfo(FilePath);
            string lastWriteTimeSeconds = ((DateTimeOffset)fileInfo.LastWriteTimeUtc).ToUnixTimeSeconds().ToString();
            string state = String.Format("{0}/{1}", fileInfo.Name, lastWriteTimeSeconds);           
            return set.Contains(state);
        }
    }

    class BlacklistedNfFile : NfFile
    {
        public BlacklistedNfFile(string filePath) : base(filePath) { }

        protected override bool CanShowIcon()
        {
            Unpickler unpickler = new Unpickler();            
            string blacklistedFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Constants.BlacklistedFile);
            byte[] bytes = System.IO.File.ReadAllBytes(blacklistedFilePath);
            object result = unpickler.loads(bytes);

            Hashtable dict = (Hashtable)result;
            FileInfo fileInfo = new FileInfo(FilePath);
            return dict.ContainsKey(fileInfo.Name);
        }
    }

    class SyncingNfFile : NfFile
    {
        public SyncingNfFile(string filePath) : base(filePath) { }

        protected override bool CanShowIcon()
        {
            return true;
        }
    }



    [ComVisible(true)]
    public class QuantificoSynced : SharpIconOverlayHandler
    {

        protected override bool CanShowOverlay(string path, FILE_ATTRIBUTE attributes)
        {
            try
            {
                App app = new App(Constants.AppName);
                if (app.IsRunning())
                {
                    SyncedNfFile nfFile = new SyncedNfFile(path);
                    return nfFile.CanShowOverlay();
                }            
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected override Icon GetOverlayIcon()
        {
            return Properties.Resources.Synced;
        }

        protected override int GetPriority()
        {
            return 1;
        }
    }

    [ComVisible(true)]
    public class QuantificoBlacklisted : SharpIconOverlayHandler
    {
        protected override bool CanShowOverlay(string path, FILE_ATTRIBUTE attributes)
        {
            try
            {
                App app = new App(Constants.AppName);
                if (app.IsRunning())
                {
                    BlacklistedNfFile nfFile = new BlacklistedNfFile(path);
                    return nfFile.CanShowOverlay();
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }            
        }

        protected override Icon GetOverlayIcon()
        {
            return Properties.Resources.Blacklisted;
        }

        protected override int GetPriority()
        {
            return 1;
        }
    }

    [ComVisible(true)]
    public class QuantificoSyncing : SharpIconOverlayHandler
    {
        protected override bool CanShowOverlay(string path, FILE_ATTRIBUTE attributes)
        {
            try
            {
                App app = new App(Constants.AppName);
                if (app.IsRunning())
                {
                    SyncingNfFile nfFile = new SyncingNfFile(path);
                    return nfFile.CanShowOverlay();
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected override Icon GetOverlayIcon()
        {
            return Properties.Resources.Syncing;
        }

        protected override int GetPriority()
        {
            return 1;
        }
    }
}
