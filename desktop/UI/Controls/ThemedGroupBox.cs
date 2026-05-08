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
    internal sealed class ThemedGroupBox : GroupBox
    {
        public int TitleLeftInset { get; set; }
        public Color BorderColor { get; set; }
        public Color TitleColor { get; set; }
        public Color FillColor { get; set; }
        public Color FrameBackgroundColor { get; set; }
        public Image WatermarkImage { get; set; }

        public ThemedGroupBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            DoubleBuffered = true;
            TitleLeftInset = 10;
            BorderColor = Color.Gray;
            TitleColor = ForeColor;
            FillColor = BackColor;
            FrameBackgroundColor = BackColor;
        }

        private int GetLabelHeight()
        {
            var measured = TextRenderer.MeasureText(Text, Font, new Size(Math.Max(1, Width - 20), 0),
                TextFormatFlags.Left | TextFormatFlags.SingleLine);
            return measured.Height + 4;
        }

        public override Rectangle DisplayRectangle
        {
            get
            {
                var labelH = GetLabelHeight();
                const int pad = 8;
                return new Rectangle(pad, labelH + 4, Math.Max(0, Width - pad * 2), Math.Max(0, Height - labelH - pad - 6));
            }
        }

        protected override void OnPaint(PaintEventArgs args)
        {
            var g = args.Graphics;
            g.SmoothingMode = SmoothingMode.None;

            var parentColor = FrameBackgroundColor;
            var labelH = GetLabelHeight();
            var borderY = labelH / 2;
            var labelLeft = Math.Max(10, TitleLeftInset);

            // 1. Fill entire control with parent color
            g.Clear(parentColor);

            // 2. Fill the box body (below border line) with the group fill color
            using (var b = new SolidBrush(FillColor))
                g.FillRectangle(b, 1, borderY, Math.Max(0, Width - 3), Math.Max(0, Height - borderY - 3));

            if (WatermarkImage != null && Width > 20 && Height - borderY > 20)
            {
                var bodyRect = new Rectangle(1, borderY, Width - 2, Height - borderY - 1);
                var imageAspect = (float)WatermarkImage.Width / Math.Max(1, WatermarkImage.Height);
                var bodyAspect = (float)bodyRect.Width / Math.Max(1, bodyRect.Height);
                int drawWidth;
                int drawHeight;

                if (imageAspect > bodyAspect)
                {
                    drawHeight = bodyRect.Height;
                    drawWidth = (int)Math.Ceiling(drawHeight * imageAspect);
                }
                else
                {
                    drawWidth = bodyRect.Width;
                    drawHeight = (int)Math.Ceiling(drawWidth / Math.Max(0.01F, imageAspect));
                }

                var drawRect = new Rectangle(
                    bodyRect.X + (bodyRect.Width - drawWidth) / 2,
                    bodyRect.Y + (bodyRect.Height - drawHeight) / 2,
                    drawWidth,
                    drawHeight);

                var matrix = new ColorMatrix();
                matrix.Matrix33 = 0.10F;

                using (var imageAttributes = new ImageAttributes())
                {
                    imageAttributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                    g.DrawImage(
                        WatermarkImage,
                        drawRect,
                        0,
                        0,
                        WatermarkImage.Width,
                        WatermarkImage.Height,
                        GraphicsUnit.Pixel,
                        imageAttributes);
                }
            }

            // 3. Measure caption and compute gap
            var textSize = TextRenderer.MeasureText(Text, Font, new Size(Math.Max(1, Width - 20), 0),
                TextFormatFlags.Left | TextFormatFlags.SingleLine);
            var gapRight = labelLeft + textSize.Width + 6;

            // 4. Draw border with gap for caption
            using (var pen = new Pen(BorderColor, 1F))
            {
                var right = Math.Max(2, Width - 3);
                var bottom = Math.Max(borderY + 1, Height - 3);
                if (labelLeft > 3) g.DrawLine(pen, 1, borderY, labelLeft - 2, borderY);
                g.DrawLine(pen, gapRight, borderY, right, borderY);
                g.DrawLine(pen, 1, borderY, 1, bottom);
                g.DrawLine(pen, right, borderY, right, bottom);
                g.DrawLine(pen, 1, bottom, right, bottom);
            }

            // 5. Draw caption text (vertically centered on the border line)
            var labelRect = new Rectangle(labelLeft, 0, textSize.Width + 8, labelH);
            TextRenderer.DrawText(g, Text, Font, labelRect, TitleColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
        }
    }

}
