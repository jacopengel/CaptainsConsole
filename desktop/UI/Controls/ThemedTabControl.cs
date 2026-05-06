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
    internal sealed class ThemedTabControl : TabControl
    {
        private ThemeColors themeColors;

        public ThemedTabControl()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            DrawMode = TabDrawMode.OwnerDrawFixed;
            ItemSize = new Size(150, 32);
            SizeMode = TabSizeMode.Fixed;
            Padding = new Point(18, 8);
            themeColors = ThemeColors.CreateLight();
        }

        public void ApplyTheme(ThemeColors colors)
        {
            themeColors = colors;
            BackColor = colors.TabBackground;
            ForeColor = colors.BodyForeground;
            foreach (TabPage page in TabPages)
            {
                page.BackColor = colors.TabBackground;
                page.ForeColor = colors.BodyForeground;
                page.UseVisualStyleBackColor = false;
            }
            Invalidate();
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_ERASEBKGND = 0x0014;
            const int WM_PAINT      = 0x000F;
            const int WM_NCPAINT    = 0x0085;

            if (m.Msg == WM_ERASEBKGND)
            {
                using (var g = Graphics.FromHdc(m.WParam))
                using (var b = new SolidBrush(themeColors.TabBackground))
                    g.FillRectangle(b, ClientRectangle);
                m.Result = (IntPtr)1;
                return;
            }

            if (m.Msg == WM_NCPAINT)
            {
                m.Result = IntPtr.Zero;
                return;
            }

            base.WndProc(ref m);

            if (m.Msg == WM_PAINT && TabPages.Count > 0)
            {
                var hdc = GetWindowDC(Handle);
                if (hdc != IntPtr.Zero)
                {
                    try
                    {
                        using (var g = Graphics.FromHdc(hdc))
                        {
                            PaintStripGaps(g);
                            PaintTabFrames(g);
                        }
                    }
                    finally
                    {
                        ReleaseDC(Handle, hdc);
                    }
                }
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hwnd);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

        private void PaintStripGaps(Graphics g)
        {
            var firstTab = GetTabRect(0);
            var lastTab  = GetTabRect(TabPages.Count - 1);
            var stripHeight = lastTab.Bottom;

            using (var b = new SolidBrush(themeColors.TabBackground))
            {
                // Gap to the right of the last tab button
                if (lastTab.Right < ClientSize.Width)
                    g.FillRectangle(b, lastTab.Right, 0, ClientSize.Width - lastTab.Right, stripHeight);
                // Tiny gap to the left of the first tab (if any)
                if (firstTab.Left > 0)
                    g.FillRectangle(b, 0, 0, firstTab.Left, stripHeight);
            }

            // Overpaint the body frame so any system white page edge is replaced by themed border.
            var pageBounds = DisplayRectangle;
            var contentRect = new Rectangle(pageBounds.X - 1, pageBounds.Y - 1, pageBounds.Width + 1, pageBounds.Height + 1);
            using (var borderPen = new Pen(themeColors.BorderColor, 1.2F))
            {
                g.DrawRectangle(borderPen, contentRect);
                g.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);
            }
        }

        private void PaintTabFrames(Graphics g)
        {
            // Final pass: enforce themed tab edges so no system white outline remains.
            using (var borderPen = new Pen(themeColors.HeaderAccent, 1F))
            {
                for (var i = 0; i < TabPages.Count; i++)
                {
                    var r = GetTabRect(i);
                    if (r.Width <= 1 || r.Height <= 1)
                    {
                        continue;
                    }

                    // Keep selected tab merged with page body by skipping bottom edge.
                    var isSelected = (i == SelectedIndex);
                    g.DrawLine(borderPen, r.Left, r.Top, r.Right - 1, r.Top);
                    g.DrawLine(borderPen, r.Left, r.Top, r.Left, r.Bottom - 1);
                    g.DrawLine(borderPen, r.Right - 1, r.Top, r.Right - 1, r.Bottom - 1);
                    if (!isSelected)
                    {
                        g.DrawLine(borderPen, r.Left, r.Bottom - 1, r.Right - 1, r.Bottom - 1);
                    }
                }
            }
        }

        protected override void OnDrawItem(DrawItemEventArgs args)
        {
            if (args.Index < 0 || args.Index >= TabPages.Count)
                return;

            var g = args.Graphics;
            var page = TabPages[args.Index];
            var bounds = GetTabRect(args.Index);
            var selected = SelectedIndex == args.Index;
            var bgColor = selected ? themeColors.TabSelectedBackground : themeColors.TabInactiveBackground;
            var fgColor = selected ? themeColors.TabSelectedForeground : themeColors.TabInactiveForeground;

            // Clear the full native tab bounds first so no system border/color can bleed through.
            using (var clear = new SolidBrush(themeColors.TabBackground))
                g.FillRectangle(clear, bounds);

            var fillBounds = selected
                ? new Rectangle(bounds.X, bounds.Y + 2, bounds.Width, bounds.Height - 1)
                : new Rectangle(bounds.X + 1, bounds.Y + 4, bounds.Width - 2, bounds.Height - 5);

            using (var b = new SolidBrush(bgColor))
                g.FillRectangle(b, fillBounds);

            using (var pen = new Pen(themeColors.BorderColor))
            {
                g.DrawLine(pen, fillBounds.Left,  fillBounds.Top,    fillBounds.Right, fillBounds.Top);
                g.DrawLine(pen, fillBounds.Left,  fillBounds.Top,    fillBounds.Left,  fillBounds.Bottom);
                g.DrawLine(pen, fillBounds.Right, fillBounds.Top,    fillBounds.Right, fillBounds.Bottom);
                if (!selected)
                    g.DrawLine(pen, fillBounds.Left, fillBounds.Bottom, fillBounds.Right, fillBounds.Bottom);
            }

            if (selected)
            {
                using (var accent = new SolidBrush(themeColors.HeaderAccent))
                    g.FillRectangle(accent, fillBounds.X + 1, fillBounds.Top + 1, fillBounds.Width - 1, 3);
            }

            TextRenderer.DrawText(g, page.Text, Font, fillBounds, fgColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
        }
    }

}
