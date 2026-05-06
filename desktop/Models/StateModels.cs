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
    internal sealed class DataRepairResult
    {
        public string BackupPath { get; set; }
        public int MovedFolderCount { get; set; }
        public bool ServerDescriptionReset { get; set; }
    }

    internal sealed class WindroseState
    {
        public string ServerRoot { get; set; }
        public string ServerFilePath { get; set; }
        public ServerEditableState Server { get; set; }
        public List<WorldEditableState> Worlds { get; set; }
        public string SelectedWorldKey { get; set; }
    }

    internal sealed class ServerEditableState
    {
        public int Version { get; set; }
        public string DeploymentId { get; set; }
        public string PersistentServerId { get; set; }
        public string InviteCode { get; set; }
        public bool IsPasswordProtected { get; set; }
        public string Password { get; set; }
        public string ServerName { get; set; }
        public string WorldIslandId { get; set; }
        public int MaxPlayerCount { get; set; }
        public string UserSelectedRegion { get; set; }
        public string P2pProxyAddress { get; set; }
        public bool UseDirectConnection { get; set; }
        public string DirectConnectionServerAddress { get; set; }
        public int DirectConnectionServerPort { get; set; }
        public string DirectConnectionProxyAddress { get; set; }
    }

    internal sealed class WorldEditableState
    {
        public string Key { get; set; }
        public string GameVersion { get; set; }
        public string FolderName { get; set; }
        public string FilePath { get; set; }
        public int Version { get; set; }
        public string IslandId { get; set; }
        public string WorldName { get; set; }
        public double CreationTime { get; set; }
        public string WorldPresetType { get; set; }
        public bool SharedQuests { get; set; }
        public bool EasyExplore { get; set; }
        public double MobHealthMultiplier { get; set; }
        public double MobDamageMultiplier { get; set; }
        public double ShipsHealthMultiplier { get; set; }
        public double ShipsDamageMultiplier { get; set; }
        public double BoardingDifficultyMultiplier { get; set; }
        public double CoopStatsCorrectionModifier { get; set; }
        public double CoopShipStatsCorrectionModifier { get; set; }
        public string CombatDifficulty { get; set; }
    }

    internal sealed class ValidationWarning
    {
        public ValidationWarning(string severity, string message)
        {
            Severity = severity;
            Message = message;
        }

        public string Severity { get; private set; }
        public string Message { get; private set; }
    }

}
