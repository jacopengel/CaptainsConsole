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
    internal sealed class ThemedTabPage : TabPage
    {
        private Image watermarkImage;

        public ThemedTabPage(string text) : base(text)
        {
            DoubleBuffered = true;
        }

        public Image WatermarkImage
        {
            get { return watermarkImage; }
            set
            {
                watermarkImage = value;
                Invalidate();
            }
        }

        protected override void OnPaintBackground(PaintEventArgs args)
        {
            base.OnPaintBackground(args);

            if (watermarkImage == null || ClientSize.Width <= 0 || ClientSize.Height <= 0)
            {
                return;
            }

            var graphics = args.Graphics;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            var imageAspect = (float)watermarkImage.Width / Math.Max(1, watermarkImage.Height);
            var pageAspect = (float)ClientSize.Width / Math.Max(1, ClientSize.Height);
            int drawWidth;
            int drawHeight;

            if (imageAspect > pageAspect)
            {
                drawHeight = ClientSize.Height;
                drawWidth = (int)Math.Ceiling(drawHeight * imageAspect);
            }
            else
            {
                drawWidth = ClientSize.Width;
                drawHeight = (int)Math.Ceiling(drawWidth / Math.Max(0.01F, imageAspect));
            }

            var drawX = (ClientSize.Width - drawWidth) / 2;
            var drawY = (ClientSize.Height - drawHeight) / 2;

            var matrix = new ColorMatrix();
            matrix.Matrix33 = 0.055F;

            using (var imageAttributes = new ImageAttributes())
            {
                imageAttributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                graphics.DrawImage(
                    watermarkImage,
                    new Rectangle(drawX, drawY, drawWidth, drawHeight),
                    0,
                    0,
                    watermarkImage.Width,
                    watermarkImage.Height,
                    GraphicsUnit.Pixel,
                    imageAttributes);
            }
        }

        protected override void WndProc(ref Message m)
        {
            // WM_NCPAINT = 0x0085 â€” suppressing it removes the system-drawn white border around the tab page
            if (m.Msg == 0x0085)
            {
                m.Result = IntPtr.Zero;
                return;
            }
            base.WndProc(ref m);
        }
    }

}
