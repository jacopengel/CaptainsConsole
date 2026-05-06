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
    public sealed class ServerDescriptionDocument
    {
        public int Version { get; set; }
        public string DeploymentId { get; set; }
        public ServerPersistent ServerDescription_Persistent { get; set; }
    }

    public sealed class ServerPersistent
    {
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

    public sealed class WorldDescriptionDocument
    {
        public int Version { get; set; }
        public WorldDescription WorldDescription { get; set; }
    }

    public sealed class WorldDescription
    {
        public string islandId { get; set; }
        public string WorldName { get; set; }
        public double CreationTime { get; set; }
        public string WorldPresetType { get; set; }
        public WorldSettings WorldSettings { get; set; }
    }

    public sealed class WorldSettings
    {
        public Dictionary<string, bool> BoolParameters { get; set; }
        public Dictionary<string, double> FloatParameters { get; set; }
        public Dictionary<string, TagParameterValue> TagParameters { get; set; }
    }

    public sealed class TagParameterValue
    {
        public string TagName { get; set; }
    }
}
