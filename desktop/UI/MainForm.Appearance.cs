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
        private void ApplySelectedTheme(string themeName)
        {
            if (string.IsNullOrWhiteSpace(themeName) || string.Equals(currentThemeName, themeName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            currentThemeName = themeName;
            ApplyTheme();
            SavePreferences();
        }

        private static GroupBox CreateGroupBox(string title, int width, int height)
        {
            var box = new ThemedGroupBox();
            box.Text = title;
            box.Width = width;
            box.Height = height;
            box.Padding = new Padding(8);
            return box;
        }

        private static TableLayoutPanel CreateFieldGrid()
        {
            var layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Top;
            layout.AutoSize = true;
            layout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            layout.Margin = new Padding(0);
            layout.Padding = new Padding(0);
            layout.ColumnCount = 1;
            layout.RowCount = 1;
            return layout;
        }

        private const int FieldPanelHeight = 42;
        private const int FieldInputWidth = 300;

        private static void BindPreferredWidth(Control control, Panel panel, int preferredWidth, int minWidth)
        {
            control.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            control.Width = FieldInputWidth;
            panel.Resize += delegate
            {
                // Shrink only if the column itself is narrower than our fixed width
                control.Width = Math.Max(minWidth, Math.Min(FieldInputWidth, panel.ClientSize.Width - 4));
            };
        }

        private static TextBox AddTextField(TableLayoutPanel layout, string label, int column, int row, int width)
        {
            var panel = CreateFieldPanel(label);
            var top = AddFieldLabel(panel, label);
            var textBox = new TextBox();
            textBox.Left = 0;
            textBox.Top = top;
            BindPreferredWidth(textBox, panel, width, 120);
            panel.Controls.Add(textBox);
            panel.Height = FieldPanelHeight;
            layout.Controls.Add(panel);
            return textBox;
        }

        private static NumericUpDown AddNumericField(TableLayoutPanel layout, string label, int column, int row, decimal minimum, decimal maximum, int decimals, int width)
        {
            var panel = CreateFieldPanel(label);
            var top = AddFieldLabel(panel, label);
            var numeric = new NumericUpDown();
            numeric.DecimalPlaces = decimals;
            numeric.Minimum = minimum;
            numeric.Maximum = maximum;
            numeric.Left = 0;
            numeric.Top = top;
            BindPreferredWidth(numeric, panel, width, 120);
            numeric.Increment = decimals == 0 ? 1M : 0.1M;
            panel.Controls.Add(numeric);
            panel.Height = FieldPanelHeight;
            layout.Controls.Add(panel);
            return numeric;
        }

        private static ComboBox AddComboField(TableLayoutPanel layout, string label, int column, int row, IEnumerable<string> items, int width)
        {
            var panel = CreateFieldPanel(label);
            var top = AddFieldLabel(panel, label);
            var combo = new ComboBox();
            combo.DropDownStyle = ComboBoxStyle.DropDownList;
            combo.Left = 0;
            combo.Top = top;
            BindPreferredWidth(combo, panel, width, 140);
            foreach (var item in items)
            {
                combo.Items.Add(item);
            }
            panel.Controls.Add(combo);
            panel.Height = FieldPanelHeight;
            layout.Controls.Add(panel);
            return combo;
        }

        private static CheckBox AddCheckField(TableLayoutPanel layout, string label, int column, int row)
        {
            var panel = CreateFieldPanel(" ");
            var checkBox = new CheckBox();
            checkBox.Text = label;
            checkBox.AutoSize = true;
            checkBox.Left = 0;
            checkBox.Top = 15;  // align with label+input top used by other fields
            checkBox.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            panel.Controls.Add(checkBox);
            panel.Height = FieldPanelHeight;
            layout.Controls.Add(panel);
            return checkBox;
        }

        private static Panel CreateFieldPanel(string label)
        {
            var panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.AutoSize = false;
            panel.Margin = new Padding(1, 2, 1, 3);
            panel.MinimumSize = new Size(0, 0);
            return panel;
        }

        private static int AddFieldLabel(Panel panel, string label)
        {
            if (label == " ")
            {
                return 12;
            }

            var labelControl = new Label();
            labelControl.Text = label;
            labelControl.AutoSize = true;
            labelControl.Left = 0;
            labelControl.Top = 0;
            panel.Controls.Add(labelControl);
            return labelControl.Bottom + 2;
        }

        private static void ConfigureResponsiveFieldGrid(TableLayoutPanel layout, int minimumFieldWidth, params Control[] fieldPanels)
        {
            Action applyLayout = delegate
            {
                var hostWidth = layout.Parent != null ? layout.Parent.ClientSize.Width : layout.ClientSize.Width;
                var availableWidth = Math.Max(1, hostWidth);
                var gap = 10;
                var effectiveMinimumWidth = Math.Max(160, minimumFieldWidth);
                var columns = Math.Max(1, Math.Min(3, (availableWidth + gap) / Math.Max(1, effectiveMinimumWidth + gap)));
                var rows = (int)Math.Ceiling(fieldPanels.Length / (double)columns);

                layout.SuspendLayout();
                layout.Controls.Clear();
                layout.ColumnStyles.Clear();
                layout.RowStyles.Clear();
                layout.Width = availableWidth;
                layout.ColumnCount = columns;
                layout.RowCount = rows;

                for (var i = 0; i < columns; i++)
                {
                    layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / columns));
                }
                for (var i = 0; i < rows; i++)
                {
                    layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                }

                for (var i = 0; i < fieldPanels.Length; i++)
                {
                    var panel = fieldPanels[i];
                    panel.Dock = DockStyle.Fill;
                    var isLastColumn = (i % columns) == columns - 1;
                    panel.Margin = new Padding(0, 2, isLastColumn ? 0 : gap, 6);
                    layout.Controls.Add(panel, i % columns, i / columns);
                }

                layout.ResumeLayout();
            };

            layout.Resize += delegate { applyLayout(); };
            applyLayout();
        }

        private static void SyncGroupBoxHeight(GroupBox groupBox, Control content)
        {
            Action sync = delegate
            {
                groupBox.Height = Math.Max(96, content.PreferredSize.Height + 52);
            };

            content.SizeChanged += delegate { sync(); };
            content.Layout += delegate { sync(); };
            sync();
        }

        private void ApplyTheme()
        {
            currentThemeColors = ThemeColors.Create(currentThemeName);
            BackColor = currentThemeColors.WindowBackground;
            ForeColor = currentThemeColors.BodyForeground;
            topHeaderPanel.BackColor = currentThemeColors.HeaderBackground;
            statusLabel.BackColor = currentThemeColors.StatusBackground;
            statusLabel.ForeColor = currentThemeColors.StatusForeground;
            titleLabel.ForeColor = currentThemeColors.HeaderAccent;
            subtitleLabel.ForeColor = currentThemeColors.HeaderMutedForeground;
            ApplyThemeRecursive(this);
            StyleButton(loadButton, currentThemeColors.ButtonSuccess, currentThemeColors.ButtonText);
            StyleButton(browseButton, currentThemeColors.ButtonNeutral, currentThemeColors.ButtonText);
            StyleButton(startServerButton, currentThemeColors.ButtonSuccess, currentThemeColors.ButtonText);
            StyleButton(stopServerButton, currentThemeColors.ButtonDanger, currentThemeColors.ButtonText);
            StyleButton(restartServerButton, currentThemeColors.ButtonSuccess, currentThemeColors.ButtonText);
            StyleButton(clearLogButton, currentThemeColors.ButtonNeutral, currentThemeColors.ButtonText);
            StyleButton(exportLogsButton, currentThemeColors.ButtonNeutral, currentThemeColors.ButtonText);
            StyleButton(applyLogFilterButton, currentThemeColors.ButtonSuccess, currentThemeColors.ButtonText);
            StyleButton(deleteServerButton, currentThemeColors.ButtonDanger, currentThemeColors.ButtonText);
            StyleButton(backupServerButton, currentThemeColors.ButtonNeutral, currentThemeColors.ButtonText);
            StyleButton(restoreFullServerButton, currentThemeColors.ButtonWarning, currentThemeColors.ButtonText);
            StyleButton(backupCaptainSettingsButton, currentThemeColors.ButtonSuccess, currentThemeColors.ButtonText);
            StyleButton(restoreCaptainSettingsButton, currentThemeColors.ButtonWarning, currentThemeColors.ButtonText);
            StyleButton(backupWorldSettingsButton, currentThemeColors.ButtonSuccess, currentThemeColors.ButtonText);
            StyleButton(restoreWorldSettingsButton, currentThemeColors.ButtonWarning, currentThemeColors.ButtonText);
            StyleButton(browseBackupFolderButton, currentThemeColors.ButtonNeutral, currentThemeColors.ButtonText);
            StyleButton(createNewWorldButton, currentThemeColors.ButtonSuccess, currentThemeColors.ButtonText);
            StyleButton(importWorldButton, currentThemeColors.ButtonSuccess, currentThemeColors.ButtonText);
            StyleButton(deleteWorldButton, currentThemeColors.ButtonDanger, currentThemeColors.ButtonText);
            StyleButton(openRootButton, currentThemeColors.ButtonNeutral, currentThemeColors.ButtonText);
            StyleButton(browseSteamCmdButton, currentThemeColors.ButtonNeutral, currentThemeColors.ButtonText);
            StyleButton(browseInstallDirButton, currentThemeColors.ButtonNeutral, currentThemeColors.ButtonText);
            StyleButton(openInstallDirButton, currentThemeColors.ButtonNeutral, currentThemeColors.ButtonText);
            StyleButton(chooseInstallFolderButton, currentThemeColors.ButtonNeutral, currentThemeColors.ButtonText);
            StyleButton(openSteamCmdGuideButton, currentThemeColors.ButtonNeutral, currentThemeColors.ButtonText);
            StyleButton(installSteamCmdButton, currentThemeColors.ButtonNeutral, currentThemeColors.ButtonText);
            StyleButton(installServerButton, currentThemeColors.ButtonSuccess, currentThemeColors.ButtonText);
            StyleButton(updateServerButton, currentThemeColors.ButtonWarning, currentThemeColors.ButtonText);
            StyleButton(saveServerTabButton, currentThemeColors.ButtonWarning, currentThemeColors.ButtonText);
            StyleButton(saveWorldTabButton, currentThemeColors.ButtonWarning, currentThemeColors.ButtonText);
            StyleButton(saveModsApiKeyButton, currentThemeColors.ButtonSuccess, currentThemeColors.ButtonText);
            StyleButton(openCurseForgeButton, currentThemeColors.ButtonNeutral, currentThemeColors.ButtonText);
            StyleButton(openModsFolderButton, currentThemeColors.ButtonNeutral, currentThemeColors.ButtonText);
            StyleButton(importModFolderButton, currentThemeColors.ButtonSuccess, currentThemeColors.ButtonText);
            StyleButton(searchCurseForgeButton, currentThemeColors.ButtonSuccess, currentThemeColors.ButtonText);
            StyleButton(installSelectedCurseForgeButton, currentThemeColors.ButtonWarning, currentThemeColors.ButtonText);
            StyleButton(installRconButton, currentThemeColors.ButtonSuccess, currentThemeColors.ButtonText);
            StyleButton(uninstallRconButton, currentThemeColors.ButtonDanger, currentThemeColors.ButtonText);
            StyleButton(saveRconSettingsButton, currentThemeColors.ButtonWarning, currentThemeColors.ButtonText);
            StyleButton(testRconButton, currentThemeColors.ButtonNeutral, currentThemeColors.ButtonText);
            StyleButton(refreshRconPlayersButton, currentThemeColors.ButtonSuccess, currentThemeColors.ButtonText);
            StyleButton(viewRconLicenseButton, currentThemeColors.ButtonNeutral, currentThemeColors.ButtonText);
            StyleButton(refreshInstalledModsButton, currentThemeColors.ButtonNeutral, currentThemeColors.ButtonText);
            StyleButton(enableModButton, currentThemeColors.ButtonSuccess, currentThemeColors.ButtonText);
            StyleButton(disableModButton, currentThemeColors.ButtonWarning, currentThemeColors.ButtonText);
            StyleButton(updateModButton, currentThemeColors.ButtonWarning, currentThemeColors.ButtonText);
            StyleButton(removeModButton, currentThemeColors.ButtonDanger, currentThemeColors.ButtonText);
            StyleButton(setActiveWorldButton, currentThemeColors.ButtonSuccess, currentThemeColors.ButtonText);
            StyleButton(rebootNowButton, currentThemeColors.ButtonWarning, currentThemeColors.ButtonText);
            toggleWarningsButton.UseVisualStyleBackColor = false;
            toggleWarningsButton.FlatStyle = FlatStyle.Flat;
            toggleWarningsButton.BackColor = currentThemeColors.GroupBackground;
            toggleWarningsButton.ForeColor = currentThemeColors.GroupForeground;
            toggleWarningsButton.FlatAppearance.BorderColor = currentThemeColors.GroupForeground;
            toggleWarningsButton.FlatAppearance.MouseOverBackColor = currentThemeColors.GroupBackground;
            toggleWarningsButton.FlatAppearance.MouseDownBackColor = currentThemeColors.GroupBackground;
            launchTargetLabel.ForeColor = currentThemeColors.MetaForeground;
            processStatusLabel.ForeColor = currentThemeColors.MetaForeground;
            serverStateTextLabel.ForeColor = currentThemeColors.MetaForeground;
            serverStateValueLabel.ForeColor = currentThemeColors.MetaForeground;
            playerCountTextLabel.ForeColor = currentThemeColors.MetaForeground;
            playerCountValueLabel.ForeColor = currentThemeColors.MetaForeground;
            rconStatusLabel.ForeColor = currentThemeColors.MetaForeground;
            themeLabel.ForeColor = currentThemeColors.HeaderForeground;
            themeComboBox.FlatStyle = FlatStyle.Flat;
            themeComboBox.BackColor = currentThemeColors.InputBackground;
            themeComboBox.ForeColor = currentThemeColors.InputForeground;
            tabs.ApplyTheme(currentThemeColors);
            worldsListView.ApplyTheme(currentThemeColors);
            if (IsHandleCreated)
            {
                ApplyNativeControlTheme();
            }
            UpdateProcessUi();
        }

        [System.Runtime.InteropServices.DllImport("uxtheme.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hwnd, string pszSubAppName, string pszSubIdList);

        private void ApplyNativeControlTheme()
        {
            try
            {
                var dark = !string.Equals(currentThemeName, "Light", StringComparison.OrdinalIgnoreCase);
                var scrollTheme = dark ? "DarkMode_Explorer" : "Explorer";
                // Tabs are fully custom drawn; applying a native theme adds unwanted system borders.
                SetWindowTheme(tabs.Handle, string.Empty, string.Empty);
                SetWindowTheme(worldsListView.Handle, scrollTheme, null);
                SetWindowTheme(logTextBox.Handle, scrollTheme, null);
                SetWindowTheme(warningsListBox.Handle, scrollTheme, null);
                SetWindowTheme(availableModsListView.Handle, scrollTheme, null);
                SetWindowTheme(installedModsListView.Handle, scrollTheme, null);
                ApplyScrollThemeRecursive(this, scrollTheme);
                tabs.Invalidate();
            }
            catch
            {
                // SetWindowTheme is best-effort; ignore failures on older OS versions.
            }
        }

        private void ApplyScrollThemeRecursive(Control root, string scrollTheme)
        {
            foreach (Control c in root.Controls)
            {
                if (c is Panel)
                {
                    var p = (Panel)c;
                    if (p.AutoScroll && p.IsHandleCreated)
                    {
                        SetWindowTheme(p.Handle, scrollTheme, null);
                    }
                }
                ApplyScrollThemeRecursive(c, scrollTheme);
            }
        }

        private void ApplyThemeRecursive(Control control)
        {
            if (control is ThemedGroupBox)
            {
                var themedGroupBox = (ThemedGroupBox)control;
                var isHeaderGroup = themedGroupBox.Parent == topHeaderPanel;
                themedGroupBox.FrameBackgroundColor = isHeaderGroup
                    ? currentThemeColors.HeaderBackground
                    : currentThemeColors.WindowBackground;
                themedGroupBox.FillColor = isHeaderGroup
                    ? Color.FromArgb(92, currentThemeColors.GroupBackground)
                    : currentThemeColors.GroupBackground;
                themedGroupBox.BorderColor = currentThemeColors.BorderColor;
                themedGroupBox.TitleColor = currentThemeColors.GroupForeground;
                themedGroupBox.WatermarkImage = null;
                themedGroupBox.BackColor = isHeaderGroup ? Color.Transparent : currentThemeColors.GroupBackground;
                themedGroupBox.ForeColor = currentThemeColors.GroupForeground;
                themedGroupBox.Invalidate();
            }
            else if (control is GroupBox)
            {
                control.BackColor = currentThemeColors.GroupBackground;
                control.ForeColor = currentThemeColors.GroupForeground;
            }
            else if (control is ThemedTabControl)
            {
                ((ThemedTabControl)control).ApplyTheme(currentThemeColors);
            }
            else if (control is TabPage)
            {
                control.BackColor = currentThemeColors.TabBackground;
                control.ForeColor = currentThemeColors.BodyForeground;
            }
            else if (control is ListBox)
            {
                control.BackColor = currentThemeColors.InputBackground;
                control.ForeColor = currentThemeColors.InputForeground;
                ((ListBox)control).BorderStyle = BorderStyle.FixedSingle;
            }
            else if (control is ListView)
            {
                control.BackColor = currentThemeColors.InputBackground;
                control.ForeColor = currentThemeColors.InputForeground;
                ((ListView)control).BorderStyle = BorderStyle.FixedSingle;
            }
            else if (control is TextBox)
            {
                var textBox = (TextBox)control;
                textBox.BackColor = currentThemeColors.InputBackground;
                textBox.ForeColor = currentThemeColors.InputForeground;
                textBox.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (control is MaskedTextBox)
            {
                var maskedTextBox = (MaskedTextBox)control;
                maskedTextBox.BackColor = currentThemeColors.InputBackground;
                maskedTextBox.ForeColor = currentThemeColors.InputForeground;
                maskedTextBox.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (control is RichTextBox)
            {
                var richTextBox = (RichTextBox)control;
                richTextBox.BackColor = currentThemeColors.InputBackground;
                richTextBox.ForeColor = currentThemeColors.InputForeground;
                richTextBox.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (control is ComboBox)
            {
                var comboBox = (ComboBox)control;
                comboBox.BackColor = currentThemeColors.InputBackground;
                comboBox.ForeColor = currentThemeColors.InputForeground;
                comboBox.FlatStyle = FlatStyle.Flat;
            }
            else if (control is NumericUpDown)
            {
                var numericUpDown = (NumericUpDown)control;
                numericUpDown.BackColor = currentThemeColors.InputBackground;
                numericUpDown.ForeColor = currentThemeColors.InputForeground;
                numericUpDown.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (control is DateTimePicker)
            {
                var dateTimePicker = (DateTimePicker)control;
                dateTimePicker.BackColor = currentThemeColors.InputBackground;
                dateTimePicker.ForeColor = currentThemeColors.InputForeground;
                dateTimePicker.CalendarMonthBackground = currentThemeColors.InputBackground;
                dateTimePicker.CalendarForeColor = currentThemeColors.InputForeground;
                dateTimePicker.CalendarTitleBackColor = currentThemeColors.InputBackground;
                dateTimePicker.CalendarTitleForeColor = currentThemeColors.InputForeground;
                dateTimePicker.CalendarTrailingForeColor = currentThemeColors.InputForeground;
            }
            else if (control is CheckBox || control is Label)
            {
                if (!(control == statusLabel))
                {
                    control.ForeColor = control.Parent != null && control.Parent.BackColor == currentThemeColors.HeaderBackground
                        ? currentThemeColors.HeaderForeground
                        : currentThemeColors.BodyForeground;
                }
            }
            else if (control is SplitContainer)
            {
                control.BackColor = currentThemeColors.WindowBackground;
            }
            else if (control is FlowLayoutPanel || control is TableLayoutPanel || control is Panel)
            {
                if (control == topHeaderPanel)
                {
                    control.BackColor = currentThemeColors.HeaderBackground;
                    control.BackgroundImage = null;
                }
                else if (control.Parent == topHeaderPanel)
                {
                    control.BackColor = Color.Transparent;
                    control.BackgroundImage = null;
                }
                else if (control.Parent is ThemedGroupBox && control.Parent.Parent == topHeaderPanel)
                {
                    control.BackColor = Color.Transparent;
                    control.BackgroundImage = null;
                }
                else if (control.Parent is ThemedGroupBox)
                {
                    control.BackColor = currentThemeColors.GroupBackground;
                    control.BackgroundImage = null;
                }
                else if (!(control.Parent == null))
                {
                    control.BackColor = control.Parent.BackColor;
                    control.BackgroundImage = null;
                }
            }

            foreach (Control child in control.Controls)
            {
                ApplyThemeRecursive(child);
            }
        }

        private static void StyleButton(Button button, Color backColor, Color foreColor)
        {
            button.BackColor = backColor;
            button.ForeColor = foreColor;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = ControlPaint.Dark(backColor, 0.18F);
            button.Cursor = Cursors.Hand;
            button.Height = Math.Max(button.Height, 32);
            button.Margin = new Padding(0, 0, 10, 8);
            button.Padding = new Padding(10, 4, 10, 4);
        }
    }
}
