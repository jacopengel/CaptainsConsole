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
    internal sealed class ThemedListView : ListView
    {
        private ThemeColors themeColors;

        public ThemedListView()
        {
            themeColors = ThemeColors.CreateLight();
            OwnerDraw = true;
            DrawColumnHeader += OnDrawColumnHeader;
            DrawItem += OnDrawItem;
            DrawSubItem += OnDrawSubItem;
        }

        public void ApplyTheme(ThemeColors colors)
        {
            themeColors = colors;
            BackColor = colors.InputBackground;
            ForeColor = colors.InputForeground;
            Invalidate();
        }

        private void OnDrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs args)
        {
            using (var bg = new SolidBrush(themeColors.GroupBackground))
                args.Graphics.FillRectangle(bg, args.Bounds);
            using (var sep = new Pen(themeColors.BorderColor))
            {
                args.Graphics.DrawLine(sep, args.Bounds.Right - 1, args.Bounds.Top, args.Bounds.Right - 1, args.Bounds.Bottom - 1);
                args.Graphics.DrawLine(sep, args.Bounds.Left, args.Bounds.Bottom - 1, args.Bounds.Right, args.Bounds.Bottom - 1);
            }
            var textRect = new Rectangle(args.Bounds.X + 6, args.Bounds.Y, args.Bounds.Width - 6, args.Bounds.Height);
            TextRenderer.DrawText(args.Graphics, args.Header.Text, Font, textRect, themeColors.GroupForeground,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        private void OnDrawItem(object sender, DrawListViewItemEventArgs args)
        {
            args.DrawDefault = true;
        }

        private void OnDrawSubItem(object sender, DrawListViewSubItemEventArgs args)
        {
            args.DrawDefault = true;
        }
    }

}
