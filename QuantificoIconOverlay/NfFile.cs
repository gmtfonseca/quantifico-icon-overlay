using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using Razorvine.Pickle;
using Newtonsoft.Json;

namespace QuantificoIconOverlay
{
    public class Settings
    {
        public string NfsDir { get; set; }
    }

    public abstract class NfFile
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
        
        protected object LoadSnapshotFile(string fileName)
        {
            Unpickler unpickler = new Unpickler();
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), fileName);
            byte[] bytes = System.IO.File.ReadAllBytes(filePath);
            object result = unpickler.loads(bytes);
            return result;
        }

        protected abstract bool CanShowIcon();
    }

    public class SyncedNfFile : NfFile
    {
        public SyncedNfFile(string filePath) : base(filePath) { }

        protected override bool CanShowIcon()
        {
            object cloudSnapshot = LoadSnapshotFile(Constants.CloudFile);
            HashSet<object> set = (HashSet<object>)cloudSnapshot;
            FileInfo fileInfo = new FileInfo(FilePath);
            string lastWriteTimeSeconds = ((DateTimeOffset)fileInfo.LastWriteTimeUtc).ToUnixTimeSeconds().ToString();
            string state = String.Format("{0}/{1}", fileInfo.Name, lastWriteTimeSeconds);
            return set.Contains(state);
        }
    }

    public class BlacklistedNfFile : NfFile
    {
        public BlacklistedNfFile(string filePath) : base(filePath) { }

        protected override bool CanShowIcon()
        {
            object blacklistSnapshot = LoadSnapshotFile(Constants.BlacklistedFile);
            Hashtable dict = (Hashtable)blacklistSnapshot;
            FileInfo fileInfo = new FileInfo(FilePath);
            return dict.ContainsKey(fileInfo.Name);
        }
    }

    public class SyncingNfFile : NfFile
    {
        public SyncingNfFile(string filePath) : base(filePath) { }

        protected override bool CanShowIcon()
        {
            return true;
        }
    }
}
