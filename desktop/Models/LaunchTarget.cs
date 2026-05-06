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
    internal sealed class LaunchTarget
    {
        public LaunchTarget(string path, string kind)
        {
            Path = path;
            Kind = kind;
            WorkingDirectory = System.IO.Path.GetDirectoryName(path) ?? string.Empty;
            DisplayName = System.IO.Path.GetFileName(path);
        }

        public string Path { get; private set; }
        public string Kind { get; private set; }
        public string WorkingDirectory { get; private set; }
        public string DisplayName { get; private set; }
    }

}
