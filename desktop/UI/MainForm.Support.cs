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
    internal sealed partial class MainForm : Form
    {
        private void AppendLog(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            var line = DateTime.Now.ToString("HH:mm:ss") + "  " + message;
            if (logTextBox.InvokeRequired)
            {
                logTextBox.BeginInvoke((MethodInvoker)delegate
                {
                    AppendLogLineToView(line);
                });
            }
            else
            {
                AppendLogLineToView(line);
            }
        }

        private void AppendLogLineToView(string line)
        {
            logLines.Add(line);
            if (LineMatchesLogFilter(line))
            {
                logTextBox.AppendText(line + Environment.NewLine);
            }
        }

        private bool LineMatchesLogFilter(string line)
        {
            if (string.IsNullOrWhiteSpace(logFilterKeyword))
            {
                return true;
            }

            return line.IndexOf(logFilterKeyword, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void ApplyLogFilter()
        {
            logFilterKeyword = (logFilterTextBox.Text ?? string.Empty).Trim();
            RefreshLogView();
            if (string.IsNullOrWhiteSpace(logFilterKeyword))
            {
                SetStatus("Log filter cleared.", false);
            }
            else
            {
                SetStatus("Filtering logs by keyword: " + logFilterKeyword, false);
            }
        }

        private void RefreshLogView()
        {
            if (logTextBox.InvokeRequired)
            {
                logTextBox.BeginInvoke((MethodInvoker)delegate { RefreshLogView(); });
                return;
            }

            logTextBox.Clear();
            foreach (var line in logLines)
            {
                if (LineMatchesLogFilter(line))
                {
                    logTextBox.AppendText(line + Environment.NewLine);
                }
            }
        }

        private void ClearLogs()
        {
            logLines.Clear();
            logTextBox.Clear();
            SetStatus("Logbook cleared.", false);
        }

        private void ExportLogs()
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "Text Files|*.txt|Log Files|*.log|All Files|*.*";
                dialog.Title = "Export Logbook";
                dialog.FileName = "windrose-logbook-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".log";
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                var linesToExport = logLines.Where(delegate(string item) { return LineMatchesLogFilter(item); }).ToList();
                File.WriteAllLines(dialog.FileName, linesToExport);
                SetStatus("Exported " + linesToExport.Count + " log lines to " + dialog.FileName, false);
            }
        }

        private void ApplyPreset(string presetName)
        {
            var world = GetSelectedWorld();
            if (world == null)
            {
                return;
            }

            WindroseRepository.ApplyPreset(world, presetName);
            world.WorldPresetType = presetName;
            BindSelectedWorldToUi();
            RefreshWorldList();
            RefreshWarnings();
            MarkDirty();
        }

        private void UpdateWorldPresetAndDirty()
        {
            var world = GetSelectedWorld();
            if (world == null)
            {
                return;
            }

            var detectedPreset = WindroseRepository.DetectPreset(world);
            world.WorldPresetType = detectedPreset;
            worldPresetTextBox.Text = world.WorldPresetType;
            RefreshWorldList();
            RefreshWarnings();
            MarkDirty();
        }

        private void MarkDirty()
        {
            if (isDirty)
            {
                return;
            }

            isDirty = true;
            UpdateWindowTitle();
            SetStatus("Unsaved changes in memory. Stop the server before saving config edits.", false);
        }

        private void UpdateWindowTitle()
        {
            Text = isDirty ? AppTitle + " *" : AppTitle;
        }

        private void SetStatus(string message, bool isError)
        {
            statusLabel.Text = message;
            statusLabel.ForeColor = isError ? currentThemeColors.StatusErrorForeground : currentThemeColors.StatusForeground;
        }

        private ServerEditableState EnsureServer()
        {
            return currentState.Server;
        }

        private WorldEditableState EnsureWorld()
        {
            var world = GetSelectedWorld();
            if (world == null)
            {
                throw new InvalidOperationException("No world selected.");
            }
            return world;
        }

        private WorldEditableState GetSelectedWorld()
        {
            if (currentState == null || string.IsNullOrEmpty(currentState.SelectedWorldKey))
            {
                return null;
            }

            return currentState.Worlds.FirstOrDefault(delegate(WorldEditableState item)
            {
                return item.Key == currentState.SelectedWorldKey;
            });
        }

        private static decimal ClampNumeric(double value, NumericUpDown numeric)
        {
            var decimalValue = Convert.ToDecimal(value);
            if (decimalValue < numeric.Minimum)
            {
                decimalValue = numeric.Minimum;
            }
            if (decimalValue > numeric.Maximum)
            {
                decimalValue = numeric.Maximum;
            }
            return decimalValue;
        }

        private static decimal ClampNumeric(int value, NumericUpDown numeric)
        {
            return ClampNumeric(Convert.ToDouble(value), numeric);
        }

        private static string ConvertCombatDisplayToTag(string display)
        {
            switch (display)
            {
                case "Easy":
                    return WindroseRepository.CombatEasyTag;
                case "Hard":
                    return WindroseRepository.CombatHardTag;
                default:
                    return WindroseRepository.CombatNormalTag;
            }
        }

        private static string ConvertCombatTagToDisplay(string tag)
        {
            switch (tag)
            {
                case WindroseRepository.CombatEasyTag:
                    return "Easy";
                case WindroseRepository.CombatHardTag:
                    return "Hard";
                default:
                    return "Normal";
            }
        }

        private static Icon CreateAppIcon()
        {
            var shipWheelImage = LoadShipWheelBitmap();
            if (shipWheelImage != null)
            {
                using (shipWheelImage)
                {
                    return CreateIconFromBitmap(shipWheelImage, 64);
                }
            }

            return CreateFallbackAppIcon();
        }

        private static Icon CreateFallbackAppIcon()
        {
            var bitmap = new Bitmap(64, 64);
            using (var graphics = Graphics.FromImage(bitmap))
            using (var goldBrush = new SolidBrush(Color.FromArgb(223, 186, 98)))
            using (var darkBrush = new SolidBrush(Color.FromArgb(23, 39, 52)))
            using (var seaBrush = new SolidBrush(Color.FromArgb(24, 92, 102)))
            using (var wavePen = new Pen(Color.FromArgb(184, 225, 230), 2F))
            using (var wheelPen = new Pen(Color.FromArgb(130, 88, 38), 4F))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.Clear(Color.Transparent);
                graphics.FillEllipse(darkBrush, 2, 2, 60, 60);
                graphics.FillEllipse(goldBrush, 14, 10, 36, 20);
                graphics.FillRectangle(seaBrush, 10, 32, 44, 18);
                graphics.DrawArc(wavePen, 12, 34, 16, 10, 0, 180);
                graphics.DrawArc(wavePen, 24, 38, 16, 10, 0, 180);
                graphics.DrawArc(wavePen, 36, 34, 16, 10, 0, 180);
                graphics.FillPolygon(darkBrush, new[]
                {
                    new Point(18, 34),
                    new Point(44, 34),
                    new Point(40, 40),
                    new Point(22, 40)
                });
                graphics.FillRectangle(darkBrush, 30, 18, 3, 18);
                graphics.FillPolygon(darkBrush, new[]
                {
                    new Point(32, 20),
                    new Point(32, 34),
                    new Point(20, 31)
                });
                graphics.DrawEllipse(wheelPen, 4, 4, 56, 56);
                graphics.DrawEllipse(wheelPen, 25, 25, 14, 14);
                graphics.DrawLine(wheelPen, 32, 4, 32, 18);
                graphics.DrawLine(wheelPen, 32, 46, 32, 60);
                graphics.DrawLine(wheelPen, 4, 32, 18, 32);
                graphics.DrawLine(wheelPen, 46, 32, 60, 32);
            }

            return Icon.FromHandle(bitmap.GetHicon());
        }

        private static Icon CreateIconFromBitmap(Bitmap sourceBitmap, int iconSize)
        {
            var iconBitmap = new Bitmap(iconSize, iconSize, PixelFormat.Format32bppArgb);
            using (var graphics = Graphics.FromImage(iconBitmap))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.Clear(Color.Transparent);

                var scale = Math.Min((float)iconSize / Math.Max(1, sourceBitmap.Width), (float)iconSize / Math.Max(1, sourceBitmap.Height));
                var drawWidth = Math.Max(1, (int)Math.Round(sourceBitmap.Width * scale));
                var drawHeight = Math.Max(1, (int)Math.Round(sourceBitmap.Height * scale));
                var drawX = (iconSize - drawWidth) / 2;
                var drawY = (iconSize - drawHeight) / 2;
                graphics.DrawImage(sourceBitmap, new Rectangle(drawX, drawY, drawWidth, drawHeight));
            }

            return Icon.FromHandle(iconBitmap.GetHicon());
        }

        private static Bitmap LoadShipWheelBitmap()
        {
            var imagePath = ResolveShipWheelImagePath();
            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            {
                return null;
            }

            using (var stream = File.OpenRead(imagePath))
            using (var image = Image.FromStream(stream))
            {
                return new Bitmap(image);
            }
        }

        private static string ResolveShipWheelImagePath()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var candidates = new[]
            {
                Path.Combine(baseDir, "shipwheel.png"),
                Path.GetFullPath(Path.Combine(baseDir, "..", "shipwheel.png")),
                Path.GetFullPath(Path.Combine(baseDir, "..", "..", "shipwheel.png"))
            };

            return candidates.FirstOrDefault(File.Exists) ?? string.Empty;
        }

        private static Image LoadHeaderWatermarkImage()
        {
            var bitmap = LoadShipWheelBitmap();
            if (bitmap == null)
            {
                return null;
            }

            return bitmap;
        }

        private static Image CreateWatermarkDisplayImage(Image sourceImage, float opacity)
        {
            if (sourceImage == null)
            {
                return null;
            }

            var bitmap = new Bitmap(sourceImage.Width, sourceImage.Height, PixelFormat.Format32bppArgb);
            using (var graphics = Graphics.FromImage(bitmap))
            using (var imageAttributes = new ImageAttributes())
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                var matrix = new ColorMatrix();
                matrix.Matrix33 = opacity;
                imageAttributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                graphics.DrawImage(
                    sourceImage,
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    0,
                    0,
                    sourceImage.Width,
                    sourceImage.Height,
                    GraphicsUnit.Pixel,
                    imageAttributes);
            }

            return bitmap;
        }

        private void PaintHeaderWatermark(Graphics graphics, Rectangle bounds)
        {
            if (graphics == null || headerWatermarkImage == null || bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            var wheelSize = Math.Max(bounds.Width, bounds.Height) * 1.45F;
            var drawWidth = (int)Math.Round(wheelSize);
            var drawHeight = drawWidth;
            var drawX = bounds.Right - (drawWidth / 2);
            var drawY = bounds.Top - (drawHeight / 2);

            var matrix = new ColorMatrix();
            matrix.Matrix33 = 0.22F;

            using (var imageAttributes = new ImageAttributes())
            {
                imageAttributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                graphics.DrawImage(
                    headerWatermarkImage,
                    new Rectangle(drawX, drawY, drawWidth, drawHeight),
                    0,
                    0,
                    headerWatermarkImage.Width,
                    headerWatermarkImage.Height,
                    GraphicsUnit.Pixel,
                imageAttributes);
            }
        }

        private static void OpenPathInShell(string target)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = target,
                UseShellExecute = true
            };
            Process.Start(startInfo);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (headerWatermarkBox != null && headerWatermarkBox.Image != null)
                {
                    headerWatermarkBox.Image.Dispose();
                }
                if (headerWatermarkImage != null)
                {
                    headerWatermarkImage.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}
