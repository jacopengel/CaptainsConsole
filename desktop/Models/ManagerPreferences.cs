using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace WindroseServerManager.Desktop
{
    internal sealed class ManagerPreferences
    {
        public string LastServerRoot { get; set; }
        public string LastInstallDir { get; set; }
        public string LastSteamCmdDir { get; set; }
        public string LastBackupDir { get; set; }
        public string LastTheme { get; set; }
        public string ModsProvider { get; set; }
        public string CurseForgeApiKey { get; set; }
        public string NexusModsApiKey { get; set; }
        public bool ScheduledBackupEnabled { get; set; }
        public int ScheduledBackupIntervalHours { get; set; }
        public string ScheduledBackupType { get; set; }
        public string NextScheduledBackupUtc { get; set; }
        public int ScheduledFullBackupRetentionCount { get; set; }
        public bool ZipFullServerBackups { get; set; }
        public bool ScheduledRebootEnabled { get; set; }
        public bool ScheduledRebootRecurring { get; set; }
        public int ScheduledRebootEveryValue { get; set; }
        public string ScheduledRebootEveryUnit { get; set; }
        public string ScheduledRebootStartLocal { get; set; }
        public string NextScheduledRebootUtc { get; set; }
        public int WindowX { get; set; }
        public int WindowY { get; set; }
        public int WindowWidth { get; set; }
        public int WindowHeight { get; set; }
        public string WindowState { get; set; }
    }

}
