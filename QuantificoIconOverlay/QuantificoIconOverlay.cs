using SharpShell.Interop;
using SharpShell.SharpIconOverlayHandler;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace QuantificoIconOverlay
{

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
