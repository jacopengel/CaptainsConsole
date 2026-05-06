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
    internal sealed class WorldListItem
    {
        private readonly string display;

        public WorldListItem(WorldEditableState world, string activeWorldId)
        {
            World = world;

            var parts = new List<string>();
            parts.Add(world.WorldName);
            parts.Add("[" + world.GameVersion + "]");
            if (world.FolderName == activeWorldId)
            {
                parts.Add("(Active)");
            }
            if (world.FolderName != world.IslandId)
            {
                parts.Add("(Mismatch)");
            }
            parts.Add("- " + WindroseRepository.DetectPreset(world));
            display = string.Join(" ", parts);
        }

        public WorldEditableState World { get; private set; }

        public override string ToString()
        {
            return display;
        }
    }

}
