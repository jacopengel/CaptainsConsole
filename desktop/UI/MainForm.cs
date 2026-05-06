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
using System.Net.Sockets;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace WindroseServerManager.Desktop
{
    internal sealed partial class MainForm : Form
    {
        private const string AppTitle = "Windrose Captain's Console";
        private const string DefaultRootDir = @"C:\WindroseCC";
        private const string DefaultServerDir = @"C:\WindroseCC\server";
        private const string DefaultSteamCmdDir = @"C:\WindroseCC\steamcmd";
        private const string DefaultSteamCmdExe = @"C:\WindroseCC\steamcmd\steamcmd.exe";
        private const string SteamCmdDownloadUrl = "https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip";
        private const string ModProviderCurseForge = "CurseForge";
        private const string ModProviderNexusMods = "Nexus Mods";
        private const string EmbeddedAppIconResourceName = "WindroseServerManager.Resources.AppIcon";
        private const string EmbeddedWindroseRconVersionDllResourceName = "WindroseServerManager.Resources.WindroseRconVersionDll";
        private const string EmbeddedWindroseRconNoticeResourceName = "WindroseServerManager.Resources.WindroseRconNotice";
        private const string EmbeddedWindroseRconLicenseResourceName = "WindroseServerManager.Resources.WindroseRconLicense";
        private const int CurseForgeGameId = 99078;
        private const int SidebarPreferredWidth = 320;
        private const int SteamCmdMissingConfigurationRetryLimit = 2;
        private static readonly TimeSpan AutomaticProvisioningStartupDelay = TimeSpan.FromSeconds(4);
        private static readonly TimeSpan AutomaticProvisioningTimeout = TimeSpan.FromSeconds(75);

        private readonly TextBox pathTextBox;
        private readonly Button loadButton;
        private readonly Button browseButton;
        private readonly Button startServerButton;
        private readonly Button stopServerButton;
        private readonly Button restartServerButton;
        private readonly Button deleteServerButton;
        private readonly Button backupServerButton;
        private readonly Button restoreFullServerButton;
        private readonly Button backupCaptainSettingsButton;
        private readonly Button restoreCaptainSettingsButton;
        private readonly Button backupWorldSettingsButton;
        private readonly Button restoreWorldSettingsButton;
        private readonly Button browseBackupFolderButton;
        private readonly Button createNewWorldButton;
        private readonly Button importWorldButton;
        private readonly Button deleteWorldButton;
        private readonly Button clearLogButton;
        private readonly Button exportLogsButton;
        private readonly Button applyLogFilterButton;
        private readonly Button openRootButton;
        private readonly Button browseSteamCmdButton;
        private readonly Button browseInstallDirButton;
        private readonly Button openInstallDirButton;
        private readonly Button chooseInstallFolderButton;
        private readonly Button installSteamCmdButton;
        private readonly Button installServerButton;
        private readonly Button updateServerButton;
        private readonly Button openSteamCmdGuideButton;
        private readonly Button openCurseForgeButton;
        private readonly Button openModsFolderButton;
        private readonly Button importModFolderButton;
        private readonly Button searchCurseForgeButton;
        private readonly Button installSelectedCurseForgeButton;
        private readonly Button refreshInstalledModsButton;
        private readonly Button enableModButton;
        private readonly Button disableModButton;
        private readonly Button updateModButton;
        private readonly Button removeModButton;
        private readonly Button installRconButton;
        private readonly Button uninstallRconButton;
        private readonly Button saveRconSettingsButton;
        private readonly Button testRconButton;
        private readonly Button refreshRconPlayersButton;
        private readonly Button viewRconLicenseButton;
        private readonly Button setActiveWorldButton;
        private readonly Button toggleWarningsButton;
        private readonly Label statusLabel;
        private readonly Label launchTargetLabel;
        private readonly Label processStatusLabel;
        private readonly Label serverStateTextLabel;
        private readonly Label serverStateValueLabel;
        private readonly Label playerCountTextLabel;
        private readonly Label playerCountValueLabel;
        private readonly Label themeLabel;
        private readonly Label titleLabel;
        private readonly Label subtitleLabel;
        private readonly Panel serverStateDotPanel;
        private readonly ProgressBar provisioningProgressBar;
        private readonly ListBox warningsListBox;
        private readonly ThemedListView worldsListView;
        private readonly TextBox steamCmdPathTextBox;
        private readonly TextBox installDirTextBox;
        private readonly ThemedTabControl tabs;
        private readonly Button saveServerTabButton;
        private readonly Button saveWorldTabButton;
        private readonly ThemedListView availableModsListView;
        private readonly ThemedListView installedModsListView;
        private readonly RichTextBox logTextBox;
        private readonly RichTextBox rconPlayersTextBox;
        private readonly TextBox logFilterTextBox;
        private readonly TextBox backupFolderTextBox;
        private readonly ComboBox modsProviderComboBox;
        private readonly TextBox modsApiKeyTextBox;
        private readonly Button saveModsApiKeyButton;
        private readonly TextBox rconBindAddressTextBox;
        private readonly NumericUpDown rconPortNumeric;
        private readonly TextBox rconPasswordTextBox;
        private readonly TextBox rconAllowedIpsTextBox;
        private readonly NumericUpDown rconMaxFailedAttemptsNumeric;
        private readonly NumericUpDown rconTimeoutNumeric;
        private readonly CheckBox rconEnableLoggingCheckBox;
        private readonly CheckBox rconSecureEnabledCheckBox;
        private readonly TextBox rconAesKeyTextBox;
        private readonly ComboBox curseForgeSearchModeComboBox;
        private readonly TextBox curseForgeSearchTextBox;
        private readonly Label curseForgeStatusLabel;
        private readonly Label rconStatusLabel;
        private readonly CheckBox scheduledBackupEnabledCheckBox;
        private readonly NumericUpDown scheduledBackupIntervalNumeric;
        private readonly ComboBox scheduledBackupTypeComboBox;
        private readonly Label scheduledBackupNextLabel;
        private readonly Timer scheduledBackupTimer;
        private readonly CheckBox scheduledRebootEnabledCheckBox;
        private readonly DateTimePicker scheduledRebootDatePicker;
        private readonly DateTimePicker scheduledRebootTimePicker;
        private readonly CheckBox recurringRebootCheckBox;
        private readonly NumericUpDown recurringRebootIntervalNumeric;
        private readonly ComboBox recurringRebootIntervalUnitComboBox;
        private readonly Label scheduledRebootNextLabel;
        private readonly Button rebootNowButton;
        private readonly Timer scheduledRebootTimer;
        private readonly ComboBox themeComboBox;
        private readonly ToolTip helpToolTip;
        private readonly Timer processPollTimer;
        private readonly Timer fileLogPollTimer;

        private readonly TextBox serverNameTextBox;
        private readonly TextBox inviteCodeTextBox;
        private readonly TextBox passwordTextBox;
        private readonly NumericUpDown maxPlayersNumeric;
        private readonly TextBox activeWorldIdTextBox;
        private readonly ComboBox regionComboBox;
        private readonly CheckBox directConnectionCheckBox;
        private readonly NumericUpDown directPortNumeric;
        private readonly TextBox bindInterfaceTextBox;
        private readonly TextBox listeningIpTextBox;
        private readonly TextBox directAddressTextBox;

        private readonly TextBox worldNameTextBox;
        private readonly TextBox worldPresetTextBox;
        private readonly ComboBox combatDifficultyComboBox;
        private readonly CheckBox sharedQuestsCheckBox;
        private readonly CheckBox immersiveExploreCheckBox;
        private readonly NumericUpDown mobHealthNumeric;
        private readonly NumericUpDown mobDamageNumeric;
        private readonly NumericUpDown shipHealthNumeric;
        private readonly NumericUpDown shipDamageNumeric;
        private readonly NumericUpDown boardingNumeric;
        private readonly NumericUpDown coopStatsNumeric;
        private readonly NumericUpDown coopShipStatsNumeric;

        private WindroseState currentState;
        private bool isBinding;
        private bool isDirty;
        private Process managedServerProcess;
        private Process managedTaskProcess;
        private LaunchTarget currentLaunchTarget;
        private string fileLogPath;
        private long fileLogPosition;
        private string fileLogRoot;
        private readonly List<string> logLines;
        private string logFilterKeyword;
        private bool isServerStarting;
        private bool isServerStopping;
        private bool isServerReady;
        private DateTime? serverStartUtc;
        private DateTime? lastServerLogUtc;
        private DateTime? lastServerOutputUtc;
        private bool hasActiveServerSession;
        private bool isAutomaticProvisioning;
        private bool automaticProvisioningWaitingForShutdown;
        private bool automaticProvisioningWasUpdate;
        private string automaticProvisioningRoot;
        private DateTime? automaticProvisioningStartedUtc;
        private readonly Image headerWatermarkImage;
        private readonly PictureBox headerWatermarkBox;
        private string currentThemeName;
        private ThemeColors currentThemeColors;
        private DateTime? nextScheduledBackupUtc;
        private DateTime? nextScheduledRebootUtc;
        private readonly Panel topHeaderPanel;
        private string selectedModsProvider = ModProviderCurseForge;
        private string storedCurseForgeApiKey = string.Empty;
        private string storedNexusModsApiKey = string.Empty;
        private GroupBox discoverModsGroup;
        private DateTime? lastPlayerCountPollUtc;
        private bool playerCountPollInFlight;
        private string currentPlayerCountDisplay = "n/a";

        private string PreferencesPath
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "WindroseCaptainsConsole",
                    "manager-preferences.json");
            }
        }

        public MainForm()
        {
            Text = AppTitle;
            Width = 1440;
            Height = 920;
            MinimumSize = new Size(800, 600);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            BackColor = Color.FromArgb(244, 240, 232);
            ForeColor = Color.FromArgb(34, 30, 23);
            headerWatermarkImage = LoadHeaderWatermarkImage();
            Icon = CreateAppIcon();
            helpToolTip = new ToolTip();
            helpToolTip.AutoPopDelay = 12000;
            helpToolTip.InitialDelay = 250;
            helpToolTip.ReshowDelay = 150;
            helpToolTip.ShowAlways = true;
            processPollTimer = new Timer();
            processPollTimer.Interval = 1000;
            processPollTimer.Tick += delegate
            {
                HandleAutomaticProvisioningTick();
                UpdateProcessUi();
            };
            processPollTimer.Start();
            fileLogPollTimer = new Timer();
            fileLogPollTimer.Interval = 1500;
            fileLogPollTimer.Tick += delegate { PollServerFileLog(); };
            fileLogPollTimer.Start();
            scheduledBackupTimer = new Timer();
            scheduledBackupTimer.Interval = 60000;
            scheduledBackupTimer.Tick += delegate { HandleScheduledBackupTick(); };
            scheduledRebootTimer = new Timer();
            scheduledRebootTimer.Interval = 60000;
            scheduledRebootTimer.Tick += delegate { HandleScheduledRebootTick(); };
            logLines = new List<string>();
            logFilterKeyword = string.Empty;
            isServerStarting = false;
            isServerStopping = false;
            isServerReady = false;
            serverStartUtc = null;
            lastServerLogUtc = null;
            lastServerOutputUtc = null;
            hasActiveServerSession = false;
            isAutomaticProvisioning = false;
            automaticProvisioningWaitingForShutdown = false;
            automaticProvisioningWasUpdate = false;
            automaticProvisioningRoot = string.Empty;
            automaticProvisioningStartedUtc = null;
            currentThemeName = "Windrose";
            currentThemeColors = ThemeColors.Create(currentThemeName);

            var root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.Padding = new Padding(16);
            root.ColumnCount = 1;
            root.RowCount = 3;
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            Controls.Add(root);

            topHeaderPanel = new Panel();
            topHeaderPanel.Dock = DockStyle.Fill;
            topHeaderPanel.AutoScroll = true;
            topHeaderPanel.Height = 324;
            topHeaderPanel.BackColor = Color.FromArgb(21, 33, 46);
            root.Controls.Add(topHeaderPanel, 0, 0);

            titleLabel = new Label();
            titleLabel.Text = AppTitle;
            titleLabel.Font = new Font("Georgia", 19F, FontStyle.Bold);
            titleLabel.AutoSize = true;
            titleLabel.Location = new Point(0, 4);
            titleLabel.ForeColor = Color.FromArgb(227, 197, 122);
            topHeaderPanel.Controls.Add(titleLabel);

            subtitleLabel = new Label();
            subtitleLabel.Text = "Locate or provision a server first, then run it, tune it, and manage its community mods.";
            subtitleLabel.AutoSize = true;
            subtitleLabel.ForeColor = Color.FromArgb(196, 205, 213);
            subtitleLabel.Location = new Point(2, 40);
            topHeaderPanel.Controls.Add(subtitleLabel);

            themeLabel = new Label();
            themeLabel.Text = "Theme:";
            themeLabel.AutoSize = true;
            themeLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            themeLabel.ForeColor = Color.FromArgb(196, 205, 213);
            topHeaderPanel.Controls.Add(themeLabel);

            themeComboBox = new ComboBox();
            themeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            themeComboBox.Width = 160;
            themeComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            themeComboBox.Items.AddRange(new object[] { "Light", "Dark", "Windrose" });
            themeComboBox.SelectedIndexChanged += delegate
            {
                if (themeComboBox.SelectedItem != null)
                {
                    ApplySelectedTheme(themeComboBox.SelectedItem.ToString());
                }
            };
            topHeaderPanel.Controls.Add(themeComboBox);

            Action positionThemeControls = delegate
            {
                var rightEdge = topHeaderPanel.ClientSize.Width - 8;
                themeComboBox.Location = new Point(Math.Max(8, rightEdge - themeComboBox.Width), 8);
                themeLabel.Location = new Point(Math.Max(8, themeComboBox.Left - themeLabel.Width - 6), 12);
            };
            topHeaderPanel.Resize += delegate { positionThemeControls(); };
            positionThemeControls();

            var pathPanel = new FlowLayoutPanel();
            pathPanel.Location = new Point(0, 76);
            pathPanel.Width = 1230;
            pathPanel.Height = 36;
            pathPanel.WrapContents = false;
            pathPanel.FlowDirection = FlowDirection.LeftToRight;
            pathPanel.BackColor = Color.FromArgb(21, 33, 46);
            topHeaderPanel.Controls.Add(pathPanel);

            pathTextBox = new TextBox();
            pathTextBox.Width = 620;
            pathPanel.Controls.Add(pathTextBox);

            browseButton = new Button();
            browseButton.Text = "Browse";
            browseButton.Width = 90;
            browseButton.Click += delegate { BrowseForFolder(); };
            pathPanel.Controls.Add(browseButton);

            loadButton = new Button();
            loadButton.Text = "Refresh";
            loadButton.Width = 110;
            loadButton.Click += delegate { LoadServer(pathTextBox.Text); };
            pathPanel.Controls.Add(loadButton);

            var serverMetaPanel = new FlowLayoutPanel();
            serverMetaPanel.Location = new Point(0, 120);
            serverMetaPanel.Width = 1230;
            serverMetaPanel.Height = 28;
            serverMetaPanel.WrapContents = false;
            serverMetaPanel.FlowDirection = FlowDirection.LeftToRight;
            serverMetaPanel.BackColor = Color.Transparent;
            topHeaderPanel.Controls.Add(serverMetaPanel);

            var actionGroupsPanel = new TableLayoutPanel();
            actionGroupsPanel.Location = new Point(0, 156);
            actionGroupsPanel.Width = 1230;
            actionGroupsPanel.Height = 164;
            actionGroupsPanel.ColumnCount = 3;
            actionGroupsPanel.RowCount = 1;
            actionGroupsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44F));
            actionGroupsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24F));
            actionGroupsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32F));
            actionGroupsPanel.BackColor = Color.Transparent;
            topHeaderPanel.Controls.Add(actionGroupsPanel);

            headerWatermarkBox = new PictureBox();
            topHeaderPanel.Paint += delegate(object sender, PaintEventArgs args)
            {
                PaintHeaderWatermark(args.Graphics, topHeaderPanel.ClientRectangle);
            };
            Action layoutHeaderBands = delegate
            {
                var availableWidth = Math.Max(340, topHeaderPanel.ClientSize.Width - 8);
                pathPanel.Width = Math.Max(340, availableWidth - pathPanel.Left);
                serverMetaPanel.Width = Math.Max(340, availableWidth - serverMetaPanel.Left);
                actionGroupsPanel.Width = Math.Max(340, availableWidth - actionGroupsPanel.Left);

                if (pathPanel.WrapContents)
                {
                    pathTextBox.Width = Math.Max(220, Math.Min(720, pathPanel.ClientSize.Width - 8));
                }
                else
                {
                    var pathReservedWidth = browseButton.Width + loadButton.Width
                        + browseButton.Margin.Horizontal + loadButton.Margin.Horizontal + 24;
                    pathTextBox.Width = Math.Max(220, Math.Min(880, pathPanel.ClientSize.Width - pathReservedWidth));
                }
            };
            topHeaderPanel.Layout += delegate { positionThemeControls(); layoutHeaderBands(); };
            topHeaderPanel.Resize += delegate
            {
                layoutHeaderBands();
                topHeaderPanel.Invalidate();
            };
            positionThemeControls();
            layoutHeaderBands();

            var provisionGroup = CreateGroupBox("Harbor Setup", 0, 0);
            provisionGroup.Dock = DockStyle.Fill;
            var runtimeGroup = CreateGroupBox("Raise Anchor", 0, 0);
            runtimeGroup.Dock = DockStyle.Fill;
            var utilityGroup = CreateGroupBox("Deck Tools", 0, 0);
            utilityGroup.Dock = DockStyle.Fill;
            actionGroupsPanel.Controls.Add(provisionGroup, 0, 0);
            actionGroupsPanel.Controls.Add(runtimeGroup, 1, 0);
            actionGroupsPanel.Controls.Add(utilityGroup, 2, 0);

            var provisionGroupLayout = new TableLayoutPanel();
            provisionGroupLayout.Dock = DockStyle.Fill;
            provisionGroupLayout.ColumnCount = 1;
            provisionGroupLayout.RowCount = 3;
            provisionGroupLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            provisionGroupLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            provisionGroupLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            provisionGroupLayout.BackColor = Color.FromArgb(21, 33, 46);
            provisionGroup.Controls.Add(provisionGroupLayout);

            var provisionButtonsPanel = new FlowLayoutPanel();
            provisionButtonsPanel.Dock = DockStyle.Fill;
            provisionButtonsPanel.WrapContents = true;
            provisionButtonsPanel.AutoScroll = false;
            provisionButtonsPanel.Padding = new Padding(0, 4, 0, 0);
            provisionButtonsPanel.BackColor = Color.FromArgb(21, 33, 46);
            provisionGroupLayout.Controls.Add(provisionButtonsPanel, 0, 2);

            var runtimeButtonsPanel = new FlowLayoutPanel();
            runtimeButtonsPanel.Dock = DockStyle.Fill;
            runtimeButtonsPanel.WrapContents = true;
            runtimeButtonsPanel.Padding = new Padding(0, 8, 0, 0);
            runtimeButtonsPanel.BackColor = Color.FromArgb(21, 33, 46);
            runtimeGroup.Controls.Add(runtimeButtonsPanel);

            var utilityButtonsPanel = new FlowLayoutPanel();
            utilityButtonsPanel.Dock = DockStyle.Fill;
            utilityButtonsPanel.WrapContents = true;
            utilityButtonsPanel.Padding = new Padding(0, 8, 0, 0);
            utilityButtonsPanel.BackColor = Color.FromArgb(21, 33, 46);
            utilityGroup.Controls.Add(utilityButtonsPanel);

            statusLabel = new Label();
            statusLabel.Dock = DockStyle.Fill;
            statusLabel.Text = "Waiting for a Windrose server folder.";
            statusLabel.BackColor = Color.FromArgb(237, 224, 193);
            statusLabel.Padding = new Padding(10, 8, 10, 8);
            statusLabel.BorderStyle = BorderStyle.None;
            root.Controls.Add(statusLabel, 0, 1);

            var splitContainer = new SplitContainer();
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.FixedPanel = FixedPanel.Panel2;
            splitContainer.IsSplitterFixed = true;
            splitContainer.BackColor = Color.FromArgb(244, 240, 232);
            root.Controls.Add(splitContainer, 0, 2);

            Action updateMainSplitLayout = delegate
            {
                splitContainer.Panel1MinSize = 360;
                splitContainer.Panel2MinSize = 170;
                var sidebarWidth = Math.Min(SidebarPreferredWidth, Math.Max(splitContainer.Panel2MinSize, splitContainer.Width / 4));
                var desiredDistance = splitContainer.Width - sidebarWidth;
                if (desiredDistance < splitContainer.Panel1MinSize)
                {
                    desiredDistance = splitContainer.Panel1MinSize;
                }
                var maxDistance = splitContainer.Width - splitContainer.Panel2MinSize;
                if (desiredDistance > maxDistance)
                {
                    desiredDistance = maxDistance;
                }
                if (desiredDistance > 0)
                {
                    splitContainer.SplitterDistance = desiredDistance;
                }
            };

            Load += delegate
            {
                updateMainSplitLayout();
            };
            splitContainer.Resize += delegate { updateMainSplitLayout(); };

            var leftLayout = new TableLayoutPanel();
            leftLayout.Dock = DockStyle.Fill;
            leftLayout.ColumnCount = 1;
            leftLayout.RowCount = 2;
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150F));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            leftLayout.BackColor = Color.FromArgb(244, 240, 232);
            splitContainer.Panel1.Controls.Add(leftLayout);

            var warningsGroup = CreateGroupBox("Captain's Warnings", 975, 150);
            warningsGroup.Dock = DockStyle.Fill;
            warningsGroup.Margin = new Padding(0);
            ((ThemedGroupBox)warningsGroup).TitleLeftInset = 42;
            warningsListBox = new ListBox();
            warningsListBox.Dock = DockStyle.Fill;
            warningsGroup.Controls.Add(warningsListBox);
            leftLayout.Controls.Add(warningsGroup, 0, 0);

            toggleWarningsButton = new Button();
            toggleWarningsButton.Text = "-";
            toggleWarningsButton.Width = 26;
            toggleWarningsButton.Height = 24;
            toggleWarningsButton.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            warningsGroup.Controls.Add(toggleWarningsButton);

            Action positionWarningsToggle = delegate
            {
                toggleWarningsButton.Left = 8;
                toggleWarningsButton.Top = 2;
            };
            warningsGroup.Resize += delegate { positionWarningsToggle(); };
            positionWarningsToggle();

            var warningsCollapsed = false;
            var warningsExpandedHeight = 150F;
            var manualWarningsCollapsed = false;
            Action<bool> setWarningsCollapsed = delegate(bool collapsed)
            {
                warningsCollapsed = collapsed;
                warningsListBox.Visible = !collapsed;
                leftLayout.RowStyles[0].Height = collapsed ? 34F : warningsExpandedHeight;
                toggleWarningsButton.Text = collapsed ? "+" : "-";
                positionWarningsToggle();
            };
            toggleWarningsButton.Click += delegate
            {
                manualWarningsCollapsed = !warningsCollapsed;
                setWarningsCollapsed(manualWarningsCollapsed);
            };
            setWarningsCollapsed(false);

            Action updateResponsiveShellLayout = delegate
            {
                var compactShell = ClientSize.Height < 860 || ClientSize.Width < 1180;
                var veryCompactShell = ClientSize.Height < 740 || ClientSize.Width < 980;

                root.Padding = veryCompactShell ? new Padding(10) : (compactShell ? new Padding(12) : new Padding(16));
                root.RowStyles[1].Height = veryCompactShell ? 30F : 34F;

                warningsExpandedHeight = veryCompactShell ? 72F : (compactShell ? 104F : 150F);
                setWarningsCollapsed(manualWarningsCollapsed || compactShell);
            };

            Resize += delegate { updateResponsiveShellLayout(); };
            Load += delegate { updateResponsiveShellLayout(); };

            tabs = new ThemedTabControl();
            tabs.Dock = DockStyle.Fill;
            tabs.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            tabs.Selecting += OnTabsSelecting;
            leftLayout.Controls.Add(tabs, 0, 1);

            var serverTab = new ThemedTabPage("Captain");
            var worldTab = new ThemedTabPage("World");
            var modsTab = new ThemedTabPage("Mods");
            var manageModsTab = new ThemedTabPage("Manage Mods");
            var backupTab = new ThemedTabPage("Backups");
            var quarterdeckTab = new ThemedTabPage("Quarterdeck");
            var rconTab = new ThemedTabPage("RCON");
            var operationsTab = new ThemedTabPage("Logbook");
            tabs.TabPages.Add(serverTab);
            tabs.TabPages.Add(worldTab);
            tabs.TabPages.Add(modsTab);
            tabs.TabPages.Add(manageModsTab);
            tabs.TabPages.Add(backupTab);
            tabs.TabPages.Add(quarterdeckTab);
            tabs.TabPages.Add(rconTab);
            tabs.TabPages.Add(operationsTab);

            Action<Panel, Control> configureTabScrollRange = delegate(Panel scrollPanel, Control contentRoot)
            {
                var isUpdatingScrollRange = false;

                Action updateScrollRange = delegate
                {
                    if (isUpdatingScrollRange)
                    {
                        return;
                    }

                    isUpdatingScrollRange = true;
                    try
                    {
                        var availableWidth = Math.Max(1, scrollPanel.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 6);
                        if (contentRoot.Width != availableWidth)
                        {
                            contentRoot.Width = availableWidth;
                        }

                        var contentHeight = Math.Max(contentRoot.PreferredSize.Height, contentRoot.Height);
                        scrollPanel.AutoScrollMinSize = new Size(0, contentHeight + 8);
                    }
                    finally
                    {
                        isUpdatingScrollRange = false;
                    }
                };

                scrollPanel.Resize += delegate { updateScrollRange(); };
                contentRoot.SizeChanged += delegate
                {
                    if (!isUpdatingScrollRange)
                    {
                        var contentHeight = Math.Max(contentRoot.PreferredSize.Height, contentRoot.Height);
                        scrollPanel.AutoScrollMinSize = new Size(0, contentHeight + 8);
                    }
                };
                Shown += delegate { updateScrollRange(); };
                updateScrollRange();
            };

            var serverScrollPanel = new Panel();
            serverScrollPanel.Dock = DockStyle.Fill;
            serverScrollPanel.AutoScroll = true;
            serverScrollPanel.Margin = new Padding(0);
            serverTab.Controls.Add(serverScrollPanel);

            var serverTabLayout = new TableLayoutPanel();
            serverTabLayout.Dock = DockStyle.Top;
            serverTabLayout.AutoSize = true;
            serverTabLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            serverTabLayout.Margin = new Padding(0);
            serverTabLayout.MinimumSize = new Size(0, 0);
            serverTabLayout.ColumnCount = 1;
            serverTabLayout.RowCount = 1;
            serverTabLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            serverScrollPanel.Controls.Add(serverTabLayout);

            var serverGroup = CreateGroupBox("Server Settings", 975, 210);
            serverGroup.AutoSize = false;
            serverGroup.Dock = DockStyle.Top;
            serverGroup.Margin = new Padding(0);
            var serverGroupLayout = new TableLayoutPanel();
            serverGroupLayout.Dock = DockStyle.Top;
            serverGroupLayout.AutoSize = true;
            serverGroupLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            serverGroupLayout.Margin = new Padding(0);
            serverGroupLayout.ColumnCount = 1;
            serverGroupLayout.RowCount = 2;
            serverGroupLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            serverGroupLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var serverLayout = CreateFieldGrid();
            serverLayout.MinimumSize = new Size(0, 0);
            serverGroupLayout.Controls.Add(serverLayout, 0, 0);
            serverTabLayout.Controls.Add(serverGroup, 0, 0);
            serverGroup.Controls.Add(serverGroupLayout);

            var serverActionsPanel = new Panel();
            serverActionsPanel.Dock = DockStyle.Top;
            serverActionsPanel.Height = 52;
            serverActionsPanel.Margin = new Padding(0);
            serverGroupLayout.Controls.Add(serverActionsPanel, 0, 1);

            saveServerTabButton = new Button();
            saveServerTabButton.Text = "Save Captain Config";
            saveServerTabButton.Width = 160;
            saveServerTabButton.Height = 34;
            saveServerTabButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            saveServerTabButton.Location = new Point(Math.Max(0, serverActionsPanel.ClientSize.Width - saveServerTabButton.Width), 8);
            serverActionsPanel.Resize += delegate
            {
                saveServerTabButton.Location = new Point(Math.Max(0, serverActionsPanel.ClientSize.Width - saveServerTabButton.Width), 8);
            };
            saveServerTabButton.Click += delegate { SaveState(); };
            serverActionsPanel.Controls.Add(saveServerTabButton);

            serverNameTextBox = AddTextField(serverLayout, "ServerName", 0, 0, 300);
            passwordTextBox = AddTextField(serverLayout, "Password", 1, 0, 300);
            maxPlayersNumeric = AddNumericField(serverLayout, "MaxPlayerCount", 2, 0, 1, 20, 0, 300);
            activeWorldIdTextBox = AddTextField(serverLayout, "WorldIslandId", 0, 1, 300);
            regionComboBox = AddComboField(serverLayout, "UserSelectedRegion", 1, 1, new[] { "Auto", "EU", "CIS", "SEA" }, 300);
            inviteCodeTextBox = AddTextField(serverLayout, "InviteCode", 2, 1, 300);
            directAddressTextBox = AddTextField(serverLayout, "DirectConnectionServerAddress", 0, 2, 300);
            directPortNumeric = AddNumericField(serverLayout, "DirectConnectionServerPort", 1, 2, 1, 65535, 0, 300);
            directConnectionCheckBox = AddCheckField(serverLayout, "UseDirectConnection", 2, 2);
            bindInterfaceTextBox = AddTextField(serverLayout, "DirectConnectionProxyAddress", 0, 3, 300);
            listeningIpTextBox = AddTextField(serverLayout, "P2pProxyAddress", 1, 3, 300);
            ConfigureResponsiveFieldGrid(serverLayout, 220,
                serverNameTextBox.Parent,
                passwordTextBox.Parent,
                maxPlayersNumeric.Parent,
                activeWorldIdTextBox.Parent,
                regionComboBox.Parent,
                inviteCodeTextBox.Parent,
                directAddressTextBox.Parent,
                directPortNumeric.Parent,
                directConnectionCheckBox.Parent,
                bindInterfaceTextBox.Parent,
                listeningIpTextBox.Parent);
            SyncGroupBoxHeight(serverGroup, serverGroupLayout);
            configureTabScrollRange(serverScrollPanel, serverTabLayout);

            var worldScrollPanel = new Panel();
            worldScrollPanel.Dock = DockStyle.Fill;
            worldScrollPanel.AutoScroll = true;
            worldScrollPanel.Margin = new Padding(0);
            worldTab.Controls.Add(worldScrollPanel);

            var worldGroup = CreateGroupBox("World Settings", 975, 290);
            worldGroup.AutoSize = false;
            worldGroup.Dock = DockStyle.Top;
            worldGroup.Margin = new Padding(0);
            var worldContainer = new TableLayoutPanel();
            worldContainer.Dock = DockStyle.Top;
            worldContainer.AutoSize = true;
            worldContainer.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            worldContainer.Margin = new Padding(0);
            worldContainer.MinimumSize = new Size(0, 0);
            worldContainer.ColumnCount = 1;
            worldContainer.RowCount = 3;
            worldContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            worldContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            worldContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            worldGroup.Controls.Add(worldContainer);
            worldScrollPanel.Controls.Add(worldGroup);

            var worldToolbar = new FlowLayoutPanel();
            worldToolbar.Dock = DockStyle.Top;
            worldToolbar.AutoSize = true;
            worldToolbar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            worldToolbar.WrapContents = true;
            worldContainer.Controls.Add(worldToolbar, 0, 0);

            setActiveWorldButton = new Button();
            setActiveWorldButton.Text = "Set Selected World Active";
            setActiveWorldButton.Width = 180;
            setActiveWorldButton.Click += delegate { SetSelectedWorldActive(); };
            worldToolbar.Controls.Add(setActiveWorldButton);

            createNewWorldButton = new Button();
            createNewWorldButton.Text = "Create New World";
            createNewWorldButton.Width = 140;
            createNewWorldButton.Click += delegate { CreateNewWorld(); };
            worldToolbar.Controls.Add(createNewWorldButton);

            importWorldButton = new Button();
            importWorldButton.Text = "Import World";
            importWorldButton.Width = 120;
            importWorldButton.Click += delegate { ImportWorld(); };
            worldToolbar.Controls.Add(importWorldButton);

            deleteWorldButton = new Button();
            deleteWorldButton.Text = "Delete World";
            deleteWorldButton.Width = 120;
            deleteWorldButton.Click += delegate { DeleteSelectedWorld(); };
            worldToolbar.Controls.Add(deleteWorldButton);

            var worldActionsPanel = new FlowLayoutPanel();
            worldActionsPanel.Dock = DockStyle.Top;
            worldActionsPanel.AutoSize = true;
            worldActionsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            worldActionsPanel.FlowDirection = FlowDirection.RightToLeft;
            worldActionsPanel.Padding = new Padding(0, 8, 0, 0);
            worldActionsPanel.WrapContents = true;
            worldContainer.Controls.Add(worldActionsPanel, 0, 2);

            saveWorldTabButton = new Button();
            saveWorldTabButton.Text = "Save World Config";
            saveWorldTabButton.Width = 150;
            saveWorldTabButton.Click += delegate { SaveState(); };
            worldActionsPanel.Controls.Add(saveWorldTabButton);

            var backupScrollPanel = new Panel();
            backupScrollPanel.Dock = DockStyle.Fill;
            backupScrollPanel.AutoScroll = true;
            backupScrollPanel.Margin = new Padding(0);
            backupTab.Controls.Add(backupScrollPanel);

            var backupGroup = CreateGroupBox("Backups", 975, 360);
            backupGroup.Dock = DockStyle.Top;
            backupGroup.Margin = new Padding(0);
            backupScrollPanel.Controls.Add(backupGroup);

            var backupLayout = new TableLayoutPanel();
            backupLayout.Dock = DockStyle.Fill;
            backupLayout.ColumnCount = 1;
            backupLayout.RowCount = 3;
            backupLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            backupLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            backupLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            backupGroup.Controls.Add(backupLayout);

            var backupPathPanel = new FlowLayoutPanel();
            backupPathPanel.Dock = DockStyle.Top;
            backupPathPanel.AutoSize = true;
            backupPathPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            backupPathPanel.WrapContents = true;
            backupLayout.Controls.Add(backupPathPanel, 0, 0);

            var backupFolderLabel = new Label();
            backupFolderLabel.Text = "Backup Folder:";
            backupFolderLabel.AutoSize = true;
            backupFolderLabel.Padding = new Padding(0, 8, 0, 0);
            backupPathPanel.Controls.Add(backupFolderLabel);

            backupFolderTextBox = new TextBox();
            backupFolderTextBox.Width = 520;
            backupPathPanel.Controls.Add(backupFolderTextBox);

            browseBackupFolderButton = new Button();
            browseBackupFolderButton.Text = "Browse";
            browseBackupFolderButton.Width = 90;
            browseBackupFolderButton.Click += delegate { BrowseForBackupFolder(); };
            backupPathPanel.Controls.Add(browseBackupFolderButton);

            var backupButtonsPanel = new FlowLayoutPanel();
            backupButtonsPanel.Dock = DockStyle.Top;
            backupButtonsPanel.AutoSize = true;
            backupButtonsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            backupButtonsPanel.WrapContents = true;
            backupLayout.Controls.Add(backupButtonsPanel, 0, 1);

            backupServerButton = new Button();
            backupServerButton.Text = "Backup Full Server";
            backupServerButton.Width = 140;
            backupServerButton.Click += delegate { BackupFullServer(); };
            backupButtonsPanel.Controls.Add(backupServerButton);

            restoreFullServerButton = new Button();
            restoreFullServerButton.Text = "Restore Full Server";
            restoreFullServerButton.Width = 140;
            restoreFullServerButton.Click += delegate { RestoreFullServer(); };
            backupButtonsPanel.Controls.Add(restoreFullServerButton);

            backupCaptainSettingsButton = new Button();
            backupCaptainSettingsButton.Text = "Backup Captain Settings";
            backupCaptainSettingsButton.Width = 170;
            backupCaptainSettingsButton.Click += delegate { BackupCaptainSettings(); };
            backupButtonsPanel.Controls.Add(backupCaptainSettingsButton);

            restoreCaptainSettingsButton = new Button();
            restoreCaptainSettingsButton.Text = "Restore Captain Settings";
            restoreCaptainSettingsButton.Width = 170;
            restoreCaptainSettingsButton.Click += delegate { RestoreCaptainSettings(); };
            backupButtonsPanel.Controls.Add(restoreCaptainSettingsButton);

            backupWorldSettingsButton = new Button();
            backupWorldSettingsButton.Text = "Backup World Settings";
            backupWorldSettingsButton.Width = 160;
            backupWorldSettingsButton.Click += delegate { BackupWorldSettings(); };
            backupButtonsPanel.Controls.Add(backupWorldSettingsButton);

            restoreWorldSettingsButton = new Button();
            restoreWorldSettingsButton.Text = "Restore World Settings";
            restoreWorldSettingsButton.Width = 160;
            restoreWorldSettingsButton.Click += delegate { RestoreWorldSettings(); };
            backupButtonsPanel.Controls.Add(restoreWorldSettingsButton);

            var backupNotes = new Label();
            backupNotes.Dock = DockStyle.Fill;
            backupNotes.AutoSize = false;
            backupNotes.Padding = new Padding(4, 4, 4, 4);
            backupNotes.Text =
                "\u2022 Use full backup before major updates or mod changes.\n" +
                "\u2022 Captain settings backup copies ServerDescription.json.\n" +
                "\u2022 World settings backup copies selected World's WorldDescription.json.\n" +
                "\u2022 Restore Full Server copies a backup folder over the current server install.\n" +
                "\u2022 Restore Captain/World Settings overwrites the live JSON file after a safety .bak copy.";
            backupLayout.Controls.Add(backupNotes, 0, 2);
            configureTabScrollRange(backupScrollPanel, backupGroup);

            var scheduledGroup = CreateGroupBox("Scheduled Backups", 975, 100);
            scheduledGroup.Dock = DockStyle.Top;
            scheduledGroup.Margin = new Padding(0, 4, 0, 0);
            backupScrollPanel.Controls.Add(scheduledGroup);

            var scheduledLayout = new TableLayoutPanel();
            scheduledLayout.Dock = DockStyle.Fill;
            scheduledLayout.ColumnCount = 1;
            scheduledLayout.RowCount = 2;
            scheduledLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            scheduledLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            scheduledGroup.Controls.Add(scheduledLayout);

            var scheduledControlsPanel = new FlowLayoutPanel();
            scheduledControlsPanel.Dock = DockStyle.Top;
            scheduledControlsPanel.AutoSize = true;
            scheduledControlsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            scheduledControlsPanel.WrapContents = true;
            scheduledLayout.Controls.Add(scheduledControlsPanel, 0, 0);

            scheduledBackupEnabledCheckBox = new CheckBox();
            scheduledBackupEnabledCheckBox.Text = "Enable Scheduled Backups";
            scheduledBackupEnabledCheckBox.AutoSize = true;
            scheduledBackupEnabledCheckBox.Padding = new Padding(0, 5, 12, 0);
            scheduledBackupEnabledCheckBox.CheckedChanged += delegate { ApplyScheduledBackupSettings(); };
            scheduledControlsPanel.Controls.Add(scheduledBackupEnabledCheckBox);

            var scheduledIntervalLabel = new Label();
            scheduledIntervalLabel.Text = "Every:";
            scheduledIntervalLabel.AutoSize = true;
            scheduledIntervalLabel.Padding = new Padding(0, 8, 4, 0);
            scheduledControlsPanel.Controls.Add(scheduledIntervalLabel);

            scheduledBackupIntervalNumeric = new NumericUpDown();
            scheduledBackupIntervalNumeric.Minimum = 1;
            scheduledBackupIntervalNumeric.Maximum = 720;
            scheduledBackupIntervalNumeric.Value = 24;
            scheduledBackupIntervalNumeric.Width = 60;
            scheduledBackupIntervalNumeric.ValueChanged += delegate { ApplyScheduledBackupSettings(); };
            scheduledControlsPanel.Controls.Add(scheduledBackupIntervalNumeric);

            var scheduledIntervalSuffixLabel = new Label();
            scheduledIntervalSuffixLabel.Text = "hours";
            scheduledIntervalSuffixLabel.AutoSize = true;
            scheduledIntervalSuffixLabel.Padding = new Padding(4, 8, 16, 0);
            scheduledControlsPanel.Controls.Add(scheduledIntervalSuffixLabel);

            var scheduledTypeLabel = new Label();
            scheduledTypeLabel.Text = "Backup:";
            scheduledTypeLabel.AutoSize = true;
            scheduledTypeLabel.Padding = new Padding(0, 8, 4, 0);
            scheduledControlsPanel.Controls.Add(scheduledTypeLabel);

            scheduledBackupTypeComboBox = new ComboBox();
            scheduledBackupTypeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            scheduledBackupTypeComboBox.Width = 220;
            scheduledBackupTypeComboBox.Items.AddRange(new object[] { "Captain + World Settings", "Full Server" });
            scheduledBackupTypeComboBox.SelectedIndex = 0;
            scheduledControlsPanel.Controls.Add(scheduledBackupTypeComboBox);

            scheduledBackupNextLabel = new Label();
            scheduledBackupNextLabel.AutoSize = true;
            scheduledBackupNextLabel.Padding = new Padding(4, 4, 4, 4);
            scheduledBackupNextLabel.Text = "Scheduled backups are disabled.";
            scheduledLayout.Controls.Add(scheduledBackupNextLabel, 0, 1);

            var rebootScrollPanel = new Panel();
            rebootScrollPanel.Dock = DockStyle.Fill;
            rebootScrollPanel.AutoScroll = true;
            rebootScrollPanel.Margin = new Padding(0);
            quarterdeckTab.Controls.Add(rebootScrollPanel);

            var rebootGroup = CreateGroupBox("Reboot Watch", 975, 170);
            rebootGroup.Dock = DockStyle.Top;
            rebootGroup.Margin = new Padding(0);
            rebootScrollPanel.Controls.Add(rebootGroup);

            var rebootLayout = new TableLayoutPanel();
            rebootLayout.Dock = DockStyle.Fill;
            rebootLayout.ColumnCount = 1;
            rebootLayout.RowCount = 3;
            rebootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rebootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rebootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rebootGroup.Controls.Add(rebootLayout);

            var rebootTopPanel = new FlowLayoutPanel();
            rebootTopPanel.Dock = DockStyle.Top;
            rebootTopPanel.AutoSize = true;
            rebootTopPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            rebootTopPanel.WrapContents = true;
            rebootLayout.Controls.Add(rebootTopPanel, 0, 0);

            scheduledRebootEnabledCheckBox = new CheckBox();
            scheduledRebootEnabledCheckBox.Text = "Enable Scheduled Reboots";
            scheduledRebootEnabledCheckBox.AutoSize = true;
            scheduledRebootEnabledCheckBox.Padding = new Padding(0, 5, 12, 0);
            scheduledRebootEnabledCheckBox.CheckedChanged += delegate { ApplyScheduledRebootSettings(); };
            rebootTopPanel.Controls.Add(scheduledRebootEnabledCheckBox);

            rebootNowButton = new Button();
            rebootNowButton.Text = "Reboot Now";
            rebootNowButton.Width = 120;
            rebootNowButton.Click += delegate { TriggerManualReboot(); };
            rebootTopPanel.Controls.Add(rebootNowButton);

            var rebootTimingPanel = new FlowLayoutPanel();
            rebootTimingPanel.Dock = DockStyle.Top;
            rebootTimingPanel.AutoSize = true;
            rebootTimingPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            rebootTimingPanel.WrapContents = true;
            rebootLayout.Controls.Add(rebootTimingPanel, 0, 1);

            var rebootAtLabel = new Label();
            rebootAtLabel.Text = "Start At:";
            rebootAtLabel.AutoSize = true;
            rebootAtLabel.Padding = new Padding(0, 8, 4, 0);
            rebootTimingPanel.Controls.Add(rebootAtLabel);

            scheduledRebootDatePicker = new DateTimePicker();
            scheduledRebootDatePicker.Format = DateTimePickerFormat.Short;
            scheduledRebootDatePicker.Width = 110;
            scheduledRebootDatePicker.ValueChanged += delegate { ApplyScheduledRebootSettings(); };
            rebootTimingPanel.Controls.Add(scheduledRebootDatePicker);

            scheduledRebootTimePicker = new DateTimePicker();
            scheduledRebootTimePicker.Format = DateTimePickerFormat.Time;
            scheduledRebootTimePicker.ShowUpDown = true;
            scheduledRebootTimePicker.Width = 110;
            scheduledRebootTimePicker.ValueChanged += delegate { ApplyScheduledRebootSettings(); };
            rebootTimingPanel.Controls.Add(scheduledRebootTimePicker);

            recurringRebootCheckBox = new CheckBox();
            recurringRebootCheckBox.Text = "Recurring";
            recurringRebootCheckBox.AutoSize = true;
            recurringRebootCheckBox.Checked = true;
            recurringRebootCheckBox.Padding = new Padding(12, 5, 0, 0);
            recurringRebootCheckBox.CheckedChanged += delegate { ApplyScheduledRebootSettings(); };
            rebootTimingPanel.Controls.Add(recurringRebootCheckBox);

            var recurringEveryLabel = new Label();
            recurringEveryLabel.Text = "Every:";
            recurringEveryLabel.AutoSize = true;
            recurringEveryLabel.Padding = new Padding(8, 8, 4, 0);
            rebootTimingPanel.Controls.Add(recurringEveryLabel);

            recurringRebootIntervalNumeric = new NumericUpDown();
            recurringRebootIntervalNumeric.Minimum = 1;
            recurringRebootIntervalNumeric.Maximum = 365;
            recurringRebootIntervalNumeric.Value = 1;
            recurringRebootIntervalNumeric.Width = 60;
            recurringRebootIntervalNumeric.ValueChanged += delegate { ApplyScheduledRebootSettings(); };
            rebootTimingPanel.Controls.Add(recurringRebootIntervalNumeric);

            recurringRebootIntervalUnitComboBox = new ComboBox();
            recurringRebootIntervalUnitComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            recurringRebootIntervalUnitComboBox.Width = 90;
            recurringRebootIntervalUnitComboBox.Items.AddRange(new object[] { "Hours", "Days" });
            recurringRebootIntervalUnitComboBox.SelectedIndex = 1;
            recurringRebootIntervalUnitComboBox.SelectedIndexChanged += delegate { ApplyScheduledRebootSettings(); };
            rebootTimingPanel.Controls.Add(recurringRebootIntervalUnitComboBox);

            scheduledRebootNextLabel = new Label();
            scheduledRebootNextLabel.AutoSize = true;
            scheduledRebootNextLabel.Padding = new Padding(4, 4, 4, 4);
            scheduledRebootNextLabel.Text = "Scheduled reboots are disabled.";
            rebootLayout.Controls.Add(scheduledRebootNextLabel, 0, 2);
            configureTabScrollRange(rebootScrollPanel, rebootGroup);

            var rconScrollPanel = new Panel();
            rconScrollPanel.Dock = DockStyle.Fill;
            rconScrollPanel.AutoScroll = true;
            rconScrollPanel.Margin = new Padding(0);
            rconTab.Controls.Add(rconScrollPanel);

            var rconLayout = new TableLayoutPanel();
            rconLayout.Dock = DockStyle.Top;
            rconLayout.AutoSize = true;
            rconLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            rconLayout.Margin = new Padding(0);
            rconLayout.ColumnCount = 1;
            rconLayout.RowCount = 3;
            rconLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rconLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rconLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rconScrollPanel.Controls.Add(rconLayout);

            var rconManageGroup = CreateGroupBox("WindroseRCON", 975, 120);
            rconManageGroup.Dock = DockStyle.Top;
            rconManageGroup.Margin = new Padding(0);
            rconLayout.Controls.Add(rconManageGroup, 0, 0);

            var rconManageLayout = new TableLayoutPanel();
            rconManageLayout.Dock = DockStyle.Fill;
            rconManageLayout.ColumnCount = 1;
            rconManageLayout.RowCount = 2;
            rconManageLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rconManageLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rconManageGroup.Controls.Add(rconManageLayout);

            var rconButtonsPanel = new FlowLayoutPanel();
            rconButtonsPanel.Dock = DockStyle.Top;
            rconButtonsPanel.AutoSize = true;
            rconButtonsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            rconButtonsPanel.WrapContents = true;
            rconManageLayout.Controls.Add(rconButtonsPanel, 0, 0);

            installRconButton = new Button();
            installRconButton.Text = "Install RCON";
            installRconButton.Width = 120;
            installRconButton.Click += delegate { InstallRconFiles(); };
            rconButtonsPanel.Controls.Add(installRconButton);

            uninstallRconButton = new Button();
            uninstallRconButton.Text = "Uninstall RCON";
            uninstallRconButton.Width = 120;
            uninstallRconButton.Click += delegate { UninstallRconFiles(); };
            rconButtonsPanel.Controls.Add(uninstallRconButton);

            saveRconSettingsButton = new Button();
            saveRconSettingsButton.Text = "Save RCON Settings";
            saveRconSettingsButton.Width = 150;
            saveRconSettingsButton.Click += delegate { SaveRconSettings(); };
            rconButtonsPanel.Controls.Add(saveRconSettingsButton);

            testRconButton = new Button();
            testRconButton.Text = "Test RCON";
            testRconButton.Width = 110;
            testRconButton.Click += delegate { TestRconConnection(); };
            rconButtonsPanel.Controls.Add(testRconButton);

            refreshRconPlayersButton = new Button();
            refreshRconPlayersButton.Text = "Refresh Players";
            refreshRconPlayersButton.Width = 130;
            refreshRconPlayersButton.Click += delegate { RefreshRconPlayers(); };
            rconButtonsPanel.Controls.Add(refreshRconPlayersButton);

            viewRconLicenseButton = new Button();
            viewRconLicenseButton.Text = "RCON License";
            viewRconLicenseButton.Width = 120;
            viewRconLicenseButton.Click += delegate { ShowRconLicenseDialog(); };
            rconButtonsPanel.Controls.Add(viewRconLicenseButton);

            rconStatusLabel = new Label();
            rconStatusLabel.AutoSize = true;
            rconStatusLabel.Padding = new Padding(4, 4, 4, 4);
            rconStatusLabel.Text = "RCON status: not detected";
            rconManageLayout.Controls.Add(rconStatusLabel, 0, 1);

            var rconSettingsGroup = CreateGroupBox("RCON Settings", 975, 240);
            rconSettingsGroup.Dock = DockStyle.Top;
            rconSettingsGroup.Margin = new Padding(0, 6, 0, 0);
            rconLayout.Controls.Add(rconSettingsGroup, 0, 1);

            var rconSettingsGrid = CreateFieldGrid();
            rconSettingsGroup.Controls.Add(rconSettingsGrid);

            rconBindAddressTextBox = AddTextField(rconSettingsGrid, "BindAddress", 0, 0, 300);
            rconPortNumeric = AddNumericField(rconSettingsGrid, "Port", 1, 0, 1, 65535, 0, 220);
            rconPasswordTextBox = AddTextField(rconSettingsGrid, "Password", 2, 0, 260);
            rconPasswordTextBox.UseSystemPasswordChar = true;
            rconAllowedIpsTextBox = AddTextField(rconSettingsGrid, "AllowedIPs", 0, 1, 300);
            rconMaxFailedAttemptsNumeric = AddNumericField(rconSettingsGrid, "MaxFailedAttempts", 1, 1, 1, 50, 0, 220);
            rconTimeoutNumeric = AddNumericField(rconSettingsGrid, "Timeout", 2, 1, 5, 600, 0, 220);
            rconEnableLoggingCheckBox = AddCheckField(rconSettingsGrid, "EnableLogging", 0, 2);
            rconSecureEnabledCheckBox = AddCheckField(rconSettingsGrid, "SecureRCON Enabled", 1, 2);
            rconAesKeyTextBox = AddTextField(rconSettingsGrid, "AESKey", 2, 2, 340);
            rconAesKeyTextBox.UseSystemPasswordChar = true;
            ConfigureResponsiveFieldGrid(rconSettingsGrid, 220,
                rconBindAddressTextBox.Parent,
                rconPortNumeric.Parent,
                rconPasswordTextBox.Parent,
                rconAllowedIpsTextBox.Parent,
                rconMaxFailedAttemptsNumeric.Parent,
                rconTimeoutNumeric.Parent,
                rconEnableLoggingCheckBox.Parent,
                rconSecureEnabledCheckBox.Parent,
                rconAesKeyTextBox.Parent);
            SyncGroupBoxHeight(rconSettingsGroup, rconSettingsGrid);

            var rconPlayersGroup = CreateGroupBox("Online Players", 975, 240);
            rconPlayersGroup.Dock = DockStyle.Top;
            rconPlayersGroup.Margin = new Padding(0, 6, 0, 0);
            rconLayout.Controls.Add(rconPlayersGroup, 0, 2);

            rconPlayersTextBox = new RichTextBox();
            rconPlayersTextBox.Dock = DockStyle.Fill;
            rconPlayersTextBox.ReadOnly = true;
            rconPlayersTextBox.WordWrap = false;
            rconPlayersTextBox.Text = "RCON player list will appear here.";
            rconPlayersGroup.Controls.Add(rconPlayersTextBox);

            configureTabScrollRange(rconScrollPanel, rconLayout);

            var worldLayout = CreateFieldGrid();
            worldLayout.MinimumSize = new Size(0, 0);
            worldContainer.Controls.Add(worldLayout, 0, 1);

            worldNameTextBox = AddTextField(worldLayout, "World Name", 0, 0, 260);
            worldPresetTextBox = AddTextField(worldLayout, "Preset", 1, 0, 140);
            worldPresetTextBox.ReadOnly = true;
            combatDifficultyComboBox = AddComboField(worldLayout, "Combat Difficulty", 2, 0, new[]
            {
                "Easy",
                "Normal",
                "Hard"
            }, 220);
            sharedQuestsCheckBox = AddCheckField(worldLayout, "Shared Quests", 3, 0);
            immersiveExploreCheckBox = AddCheckField(worldLayout, "Immersive Exploration", 0, 1);
            mobHealthNumeric = AddNumericField(worldLayout, "Enemy Health", 1, 1, 0.2M, 5M, 1, 180);
            mobDamageNumeric = AddNumericField(worldLayout, "Enemy Damage", 2, 1, 0.2M, 5M, 1, 180);
            shipHealthNumeric = AddNumericField(worldLayout, "Enemy Ship Health", 3, 1, 0.4M, 5M, 1, 180);
            shipDamageNumeric = AddNumericField(worldLayout, "Enemy Ship Damage", 0, 2, 0.2M, 2.5M, 1, 180);
            boardingNumeric = AddNumericField(worldLayout, "Boarding Difficulty", 1, 2, 0.2M, 5M, 1, 180);
            coopStatsNumeric = AddNumericField(worldLayout, "Enemy Scaling By Player Count", 2, 2, 0M, 2M, 1, 180);
            coopShipStatsNumeric = AddNumericField(worldLayout, "Enemy Ship Scaling By Player Count", 3, 2, 0M, 2M, 1, 180);
            ConfigureResponsiveFieldGrid(worldLayout, 220,
                worldNameTextBox.Parent,
                worldPresetTextBox.Parent,
                combatDifficultyComboBox.Parent,
                sharedQuestsCheckBox.Parent,
                immersiveExploreCheckBox.Parent,
                mobHealthNumeric.Parent,
                mobDamageNumeric.Parent,
                shipHealthNumeric.Parent,
                shipDamageNumeric.Parent,
                boardingNumeric.Parent,
                coopStatsNumeric.Parent,
                coopShipStatsNumeric.Parent);
            SyncGroupBoxHeight(worldGroup, worldContainer);
            configureTabScrollRange(worldScrollPanel, worldLayout);

            var modsScrollPanel = new Panel();
            modsScrollPanel.Dock = DockStyle.Fill;
            modsScrollPanel.AutoScroll = true;
            modsScrollPanel.Margin = new Padding(0);
            modsTab.Controls.Add(modsScrollPanel);

            var modsGroup = CreateGroupBox("Community Mod Workflow", 975, 360);
            modsGroup.Dock = DockStyle.Top;
            modsGroup.Margin = new Padding(0);
            modsScrollPanel.Controls.Add(modsGroup);

            var modsLayout = new TableLayoutPanel();
            modsLayout.Dock = DockStyle.Fill;
            modsLayout.ColumnCount = 1;
            modsLayout.RowCount = 1;
            modsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            modsGroup.Controls.Add(modsLayout);

            var modsTopPanel = new Panel();
            modsTopPanel.Dock = DockStyle.Fill;
            modsLayout.Controls.Add(modsTopPanel, 0, 0);

            discoverModsGroup = CreateGroupBox("Discover & Install (" + ModProviderCurseForge + ")", 975, 240);
            discoverModsGroup.Dock = DockStyle.Fill;
            modsTopPanel.Controls.Add(discoverModsGroup);

            var discoverLayout = new TableLayoutPanel();
            discoverLayout.Dock = DockStyle.Fill;
            discoverLayout.ColumnCount = 1;
            discoverLayout.RowCount = 4;
            discoverLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            discoverLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            discoverLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            discoverLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            discoverModsGroup.Controls.Add(discoverLayout);

            var discoverSettingsPanel = new FlowLayoutPanel();
            discoverSettingsPanel.Dock = DockStyle.Top;
            discoverSettingsPanel.AutoSize = true;
            discoverSettingsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            discoverSettingsPanel.WrapContents = true;
            discoverLayout.Controls.Add(discoverSettingsPanel, 0, 0);

            var providerLabel = new Label();
            providerLabel.Text = "Provider:";
            providerLabel.AutoSize = true;
            providerLabel.Padding = new Padding(0, 8, 4, 0);
            discoverSettingsPanel.Controls.Add(providerLabel);

            modsProviderComboBox = new ComboBox();
            modsProviderComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            modsProviderComboBox.Width = 130;
            modsProviderComboBox.Items.Add(ModProviderCurseForge);
            modsProviderComboBox.Items.Add(ModProviderNexusMods);
            modsProviderComboBox.SelectedIndex = 0;
            modsProviderComboBox.SelectedIndexChanged += delegate { OnModsProviderChanged(); };
            discoverSettingsPanel.Controls.Add(modsProviderComboBox);

            var apiKeyLabel = new Label();
            apiKeyLabel.Text = "API Key:";
            apiKeyLabel.AutoSize = true;
            apiKeyLabel.Padding = new Padding(10, 8, 4, 0);
            discoverSettingsPanel.Controls.Add(apiKeyLabel);

            modsApiKeyTextBox = new TextBox();
            modsApiKeyTextBox.Width = 320;
            modsApiKeyTextBox.UseSystemPasswordChar = true;
            modsApiKeyTextBox.TextChanged += delegate
            {
                SetStoredApiKeyForProvider(GetSelectedModsProvider(), modsApiKeyTextBox.Text.Trim());
                UpdateProcessUi();
            };
            discoverSettingsPanel.Controls.Add(modsApiKeyTextBox);

            saveModsApiKeyButton = new Button();
            saveModsApiKeyButton.Text = "Save Key";
            saveModsApiKeyButton.Width = 95;
            saveModsApiKeyButton.Click += delegate { SaveCurrentModsApiKey(); };
            discoverSettingsPanel.Controls.Add(saveModsApiKeyButton);

            openCurseForgeButton = new Button();
            openCurseForgeButton.Text = "Open CurseForge";
            openCurseForgeButton.Width = 130;
            openCurseForgeButton.Click += delegate { OpenUrl(GetSelectedModsProviderBrowseUrl()); };
            discoverSettingsPanel.Controls.Add(openCurseForgeButton);

            var discoverSearchPanel = new FlowLayoutPanel();
            discoverSearchPanel.Dock = DockStyle.Top;
            discoverSearchPanel.AutoSize = true;
            discoverSearchPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            discoverSearchPanel.WrapContents = true;
            discoverLayout.Controls.Add(discoverSearchPanel, 0, 1);

            var searchLabel = new Label();
            searchLabel.Text = "Search:";
            searchLabel.AutoSize = true;
            searchLabel.Padding = new Padding(0, 8, 4, 0);
            discoverSearchPanel.Controls.Add(searchLabel);

            curseForgeSearchModeComboBox = new ComboBox();
            curseForgeSearchModeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            curseForgeSearchModeComboBox.Width = 120;
            curseForgeSearchModeComboBox.Items.Add("Keyword");
            curseForgeSearchModeComboBox.Items.Add("Mod ID");
            curseForgeSearchModeComboBox.SelectedIndex = 0;
            discoverSearchPanel.Controls.Add(curseForgeSearchModeComboBox);

            curseForgeSearchTextBox = new TextBox();
            curseForgeSearchTextBox.Width = 300;
            curseForgeSearchTextBox.Text = string.Empty;
            discoverSearchPanel.Controls.Add(curseForgeSearchTextBox);

            searchCurseForgeButton = new Button();
            searchCurseForgeButton.Text = "Search Mods";
            searchCurseForgeButton.Width = 120;
            searchCurseForgeButton.Click += delegate { SearchSelectedModsProvider(); };
            discoverSearchPanel.Controls.Add(searchCurseForgeButton);

            installSelectedCurseForgeButton = new Button();
            installSelectedCurseForgeButton.Text = "Install Selected";
            installSelectedCurseForgeButton.Width = 130;
            installSelectedCurseForgeButton.Click += delegate { InstallSelectedModFromProvider(); };
            discoverSearchPanel.Controls.Add(installSelectedCurseForgeButton);

            availableModsListView = new ThemedListView();
            availableModsListView.Dock = DockStyle.Fill;
            availableModsListView.View = View.Details;
            availableModsListView.FullRowSelect = true;
            availableModsListView.HideSelection = false;
            availableModsListView.MultiSelect = false;
            availableModsListView.GridLines = true;
            availableModsListView.Columns.Add("Mod", 260);
            availableModsListView.Columns.Add("Mod ID", 90);
            availableModsListView.Columns.Add("Author", 170);
            availableModsListView.Columns.Add("Last Updated", 140);
            availableModsListView.Columns.Add("Latest File", 420);
            availableModsListView.SelectedIndexChanged += delegate { UpdateProcessUi(); };
            discoverLayout.Controls.Add(availableModsListView, 0, 2);

            curseForgeStatusLabel = new Label();
            curseForgeStatusLabel.AutoSize = true;
            curseForgeStatusLabel.Text = "Select a provider and add your API key to enable online mod tools.";
            discoverLayout.Controls.Add(curseForgeStatusLabel, 0, 3);

            var manageModsScrollPanel = new Panel();
            manageModsScrollPanel.Dock = DockStyle.Fill;
            manageModsScrollPanel.AutoScroll = true;
            manageModsScrollPanel.Margin = new Padding(0);
            manageModsTab.Controls.Add(manageModsScrollPanel);

            var manageGroup = CreateGroupBox("Manage Installed Mods", 975, 320);
            manageGroup.Dock = DockStyle.Top;
            manageGroup.Margin = new Padding(0);
            manageModsScrollPanel.Controls.Add(manageGroup);

            var manageLayout = new TableLayoutPanel();
            manageLayout.Dock = DockStyle.Fill;
            manageLayout.ColumnCount = 1;
            manageLayout.RowCount = 2;
            manageLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            manageLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            manageGroup.Controls.Add(manageLayout);

            var manageButtonsPanel = new FlowLayoutPanel();
            manageButtonsPanel.Dock = DockStyle.Top;
            manageButtonsPanel.AutoSize = true;
            manageButtonsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            manageButtonsPanel.WrapContents = true;
            manageLayout.Controls.Add(manageButtonsPanel, 0, 0);

            refreshInstalledModsButton = new Button();
            refreshInstalledModsButton.Text = "Refresh Installed";
            refreshInstalledModsButton.Width = 130;
            refreshInstalledModsButton.Click += delegate { PopulateModsInfo(); };
            manageButtonsPanel.Controls.Add(refreshInstalledModsButton);

            enableModButton = new Button();
            enableModButton.Text = "Enable";
            enableModButton.Width = 90;
            enableModButton.Click += delegate { EnableSelectedMod(); };
            manageButtonsPanel.Controls.Add(enableModButton);

            disableModButton = new Button();
            disableModButton.Text = "Disable";
            disableModButton.Width = 90;
            disableModButton.Click += delegate { DisableSelectedMod(); };
            manageButtonsPanel.Controls.Add(disableModButton);

            updateModButton = new Button();
            updateModButton.Text = "Update";
            updateModButton.Width = 90;
            updateModButton.Click += delegate { UpdateSelectedMod(); };
            manageButtonsPanel.Controls.Add(updateModButton);

            removeModButton = new Button();
            removeModButton.Text = "Remove";
            removeModButton.Width = 90;
            removeModButton.Click += delegate { RemoveSelectedMod(); };
            manageButtonsPanel.Controls.Add(removeModButton);

            openModsFolderButton = new Button();
            openModsFolderButton.Text = "Open ~mods Folder";
            openModsFolderButton.Width = 130;
            openModsFolderButton.Click += delegate { OpenModsFolder(); };
            manageButtonsPanel.Controls.Add(openModsFolderButton);

            importModFolderButton = new Button();
            importModFolderButton.Text = "Import Mod Folder";
            importModFolderButton.Width = 130;
            importModFolderButton.Click += delegate { ImportModFolder(); };
            manageButtonsPanel.Controls.Add(importModFolderButton);

            installedModsListView = new ThemedListView();
            installedModsListView.Dock = DockStyle.Fill;
            installedModsListView.View = View.Details;
            installedModsListView.FullRowSelect = true;
            installedModsListView.HideSelection = false;
            installedModsListView.MultiSelect = false;
            installedModsListView.GridLines = true;
            installedModsListView.Columns.Add("Folder", 230);
            installedModsListView.Columns.Add("State", 85);
            installedModsListView.Columns.Add("Mod ID", 80);
            installedModsListView.Columns.Add("Last Updated", 140);
            installedModsListView.Columns.Add("Update", 110);
            installedModsListView.Columns.Add("Source", 150);
            installedModsListView.SelectedIndexChanged += delegate { UpdateProcessUi(); };
            manageLayout.Controls.Add(installedModsListView, 0, 1);

            configureTabScrollRange(manageModsScrollPanel, manageGroup);
            configureTabScrollRange(modsScrollPanel, modsGroup);

            var steamCmdPanel = new FlowLayoutPanel();
            steamCmdPanel.Dock = DockStyle.Fill;
            steamCmdPanel.WrapContents = false;
            steamCmdPanel.BackColor = Color.FromArgb(21, 33, 46);
            provisionGroupLayout.Controls.Add(steamCmdPanel, 0, 0);

            var steamCmdLabel = new Label();
            steamCmdLabel.Text = "SteamCMD:";
            steamCmdLabel.AutoSize = true;
            steamCmdLabel.Padding = new Padding(0, 8, 0, 0);
            steamCmdPanel.Controls.Add(steamCmdLabel);

            steamCmdPathTextBox = new TextBox();
            steamCmdPathTextBox.Width = 156;
            steamCmdPanel.Controls.Add(steamCmdPathTextBox);

            browseSteamCmdButton = new Button();
            browseSteamCmdButton.Text = "Browse SteamCMD Folder";
            browseSteamCmdButton.Width = 170;
            browseSteamCmdButton.Click += delegate { BrowseForSteamCmd(); };
            steamCmdPanel.Controls.Add(browseSteamCmdButton);

            openSteamCmdGuideButton = new Button();
            openSteamCmdGuideButton.Text = "Setup Guide";
            openSteamCmdGuideButton.Width = 110;
            openSteamCmdGuideButton.Click += delegate { OpenUrl("https://playwindrose.com/dedicated-server-guide/"); };

            var installDirPanel = new FlowLayoutPanel();
            installDirPanel.Dock = DockStyle.Fill;
            installDirPanel.WrapContents = false;
            installDirPanel.BackColor = Color.FromArgb(21, 33, 46);
            provisionGroupLayout.Controls.Add(installDirPanel, 0, 1);

            var installDirLabel = new Label();
            installDirLabel.Text = "Install Dir:";
            installDirLabel.AutoSize = true;
            installDirLabel.Padding = new Padding(0, 8, 0, 0);
            installDirPanel.Controls.Add(installDirLabel);

            installDirTextBox = new TextBox();
            installDirTextBox.Width = 272;
            installDirPanel.Controls.Add(installDirTextBox);

            browseInstallDirButton = new Button();
            browseInstallDirButton.Text = "Browse Install Folder";
            browseInstallDirButton.Width = 156;
            browseInstallDirButton.Click += delegate { BrowseForInstallDirectory(); };
            installDirPanel.Controls.Add(browseInstallDirButton);

            Action layoutProvisionRows = delegate
            {
                var steamReserved = steamCmdLabel.PreferredWidth + browseSteamCmdButton.Width
                    + steamCmdLabel.Margin.Horizontal + browseSteamCmdButton.Margin.Horizontal + 26;
                steamCmdPathTextBox.Width = Math.Max(100, steamCmdPanel.ClientSize.Width - steamReserved);

                var installReserved = installDirLabel.PreferredWidth + browseInstallDirButton.Width
                    + installDirLabel.Margin.Horizontal + browseInstallDirButton.Margin.Horizontal + 26;
                installDirTextBox.Width = Math.Max(100, installDirPanel.ClientSize.Width - installReserved);
            };
            steamCmdPanel.Resize += delegate { layoutProvisionRows(); };
            installDirPanel.Resize += delegate { layoutProvisionRows(); };
            provisionGroupLayout.Resize += delegate { layoutProvisionRows(); };
            Load += delegate { layoutProvisionRows(); };

            openInstallDirButton = new Button();
            openInstallDirButton.Text = "Open Install Folder";
            openInstallDirButton.Width = 146;
            openInstallDirButton.Click += delegate { OpenInstallDirectory(); };

            chooseInstallFolderButton = new Button();
            chooseInstallFolderButton.Text = "Choose Install Folder";
            chooseInstallFolderButton.Width = 150;
            chooseInstallFolderButton.Click += delegate { BrowseForInstallDirectory(); };
            chooseInstallFolderButton.Visible = false;

            installSteamCmdButton = new Button();
            installSteamCmdButton.Text = "Install SteamCMD";
            installSteamCmdButton.Width = 146;
            installSteamCmdButton.Click += delegate { InstallSteamCmd(); };
            provisionButtonsPanel.Controls.Add(installSteamCmdButton);

            installServerButton = new Button();
            installServerButton.Text = "Install Windrose";
            installServerButton.Width = 136;
            installServerButton.Click += delegate { RunSteamCmd(false); };
            provisionButtonsPanel.Controls.Add(installServerButton);

            updateServerButton = new Button();
            updateServerButton.Text = "Update Windrose";
            updateServerButton.Width = 136;
            updateServerButton.Click += delegate { RunSteamCmd(true); };
            provisionButtonsPanel.Controls.Add(updateServerButton);

            deleteServerButton = new Button();
            deleteServerButton.Text = "Delete Server";
            deleteServerButton.Width = 110;
            deleteServerButton.Click += delegate { DeleteServerInstall(); };
            utilityButtonsPanel.Controls.Add(deleteServerButton);

            launchTargetLabel = new Label();
            launchTargetLabel.AutoSize = true;
            launchTargetLabel.Text = "Launch target: not detected yet";
            launchTargetLabel.Padding = new Padding(0, 4, 0, 0);
            serverMetaPanel.Controls.Add(launchTargetLabel);

            processStatusLabel = new Label();
            processStatusLabel.AutoSize = true;
            processStatusLabel.Text = "Process status: idle";
            processStatusLabel.Padding = new Padding(0, 4, 0, 0);
            processStatusLabel.Margin = new Padding(24, 0, 0, 0);
            serverMetaPanel.Controls.Add(processStatusLabel);

            var serverStatePanel = new FlowLayoutPanel();
            serverStatePanel.AutoSize = true;
            serverStatePanel.WrapContents = false;
            serverStatePanel.Padding = new Padding(0);
            serverStatePanel.Margin = new Padding(24, 0, 0, 0);
            serverMetaPanel.Controls.Add(serverStatePanel);

            serverStateTextLabel = new Label();
            serverStateTextLabel.AutoSize = true;
            serverStateTextLabel.Text = "Server State:";
            serverStateTextLabel.Padding = new Padding(0, 4, 4, 0);
            serverStatePanel.Controls.Add(serverStateTextLabel);

            serverStateDotPanel = new Panel();
            serverStateDotPanel.Width = 12;
            serverStateDotPanel.Height = 12;
            serverStateDotPanel.Margin = new Padding(2, 7, 6, 0);
            serverStateDotPanel.BackColor = Color.FromArgb(131, 48, 42);
            serverStateDotPanel.Paint += delegate(object sender, PaintEventArgs args)
            {
                args.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(serverStateDotPanel.BackColor))
                {
                    args.Graphics.FillEllipse(brush, 0, 0, serverStateDotPanel.Width - 1, serverStateDotPanel.Height - 1);
                }
            };
            serverStatePanel.Controls.Add(serverStateDotPanel);

            serverStateValueLabel = new Label();
            serverStateValueLabel.Name = "serverStateValueLabel";
            serverStateValueLabel.AutoSize = true;
            serverStateValueLabel.Text = "Stopped";
            serverStateValueLabel.Padding = new Padding(0, 4, 0, 0);
            serverStatePanel.Controls.Add(serverStateValueLabel);

            playerCountTextLabel = new Label();
            playerCountTextLabel.AutoSize = true;
            playerCountTextLabel.Text = "Players:";
            playerCountTextLabel.Padding = new Padding(18, 4, 4, 0);
            serverStatePanel.Controls.Add(playerCountTextLabel);

            playerCountValueLabel = new Label();
            playerCountValueLabel.AutoSize = true;
            playerCountValueLabel.Text = currentPlayerCountDisplay;
            playerCountValueLabel.Padding = new Padding(0, 4, 0, 0);
            serverStatePanel.Controls.Add(playerCountValueLabel);

            provisioningProgressBar = new ProgressBar();
            provisioningProgressBar.Width = 220;
            provisioningProgressBar.Height = 16;
            provisioningProgressBar.Style = ProgressBarStyle.Marquee;
            provisioningProgressBar.MarqueeAnimationSpeed = 24;
            provisioningProgressBar.Visible = false;
            provisioningProgressBar.Margin = new Padding(20, 4, 0, 0);
            serverMetaPanel.Controls.Add(provisioningProgressBar);

            startServerButton = new Button();
            startServerButton.Text = "Start Server";
            startServerButton.Width = 120;
            startServerButton.Click += delegate { StartServer(); };
            runtimeButtonsPanel.Controls.Add(startServerButton);

            stopServerButton = new Button();
            stopServerButton.Text = "Stop Server";
            stopServerButton.Width = 120;
            stopServerButton.Click += delegate { StopServer(); };
            runtimeButtonsPanel.Controls.Add(stopServerButton);

            restartServerButton = new Button();
            restartServerButton.Text = "Restart";
            restartServerButton.Width = 120;
            restartServerButton.Click += delegate { RestartServer(); };
            runtimeButtonsPanel.Controls.Add(restartServerButton);

            openRootButton = new Button();
            openRootButton.Text = "Open Folder";
            openRootButton.Width = 120;
            openRootButton.Click += delegate { OpenLoadedRoot(); };
            utilityButtonsPanel.Controls.Add(openRootButton);

            utilityButtonsPanel.Controls.Add(openSteamCmdGuideButton);

            Action applyTopHeaderLayout = delegate
            {
                var compactHeader = ClientSize.Width < 1120 || ClientSize.Height < 760;
                var veryCompactHeader = ClientSize.Width < 920 || ClientSize.Height < 680;

                headerWatermarkBox.Visible = false;
                themeLabel.Visible = !veryCompactHeader;
                themeComboBox.Width = compactHeader ? 132 : 160;
                positionThemeControls();

                pathPanel.WrapContents = compactHeader;
                pathPanel.SetFlowBreak(pathTextBox, compactHeader);
                pathPanel.Height = compactHeader ? 72 : 36;
                browseButton.Width = compactHeader ? 100 : 90;
                loadButton.Width = compactHeader ? 100 : 110;

                serverMetaPanel.WrapContents = compactHeader;
                serverMetaPanel.Height = compactHeader ? 52 : 28;

                steamCmdPanel.WrapContents = veryCompactHeader;
                steamCmdPanel.Height = veryCompactHeader ? 60 : 34;
                installDirPanel.WrapContents = veryCompactHeader;
                installDirPanel.Height = veryCompactHeader ? 60 : 34;
                provisionGroupLayout.RowStyles[0].Height = veryCompactHeader ? 60F : 34F;
                provisionGroupLayout.RowStyles[1].Height = veryCompactHeader ? 60F : 34F;
                actionGroupsPanel.Height = compactHeader ? 188 : 164;
                actionGroupsPanel.ColumnStyles[0].Width = 44F;
                actionGroupsPanel.ColumnStyles[1].Width = 24F;
                actionGroupsPanel.ColumnStyles[2].Width = 32F;
                actionGroupsPanel.SetColumnSpan(provisionGroup, 1);

                pathPanel.Location = new Point(0, 76);
                serverMetaPanel.Location = new Point(0, pathPanel.Bottom + 6);
                actionGroupsPanel.Location = new Point(0, serverMetaPanel.Bottom + 8);

                var rightEdge = topHeaderPanel.ClientSize.Width - 8;
                var availableWidth = Math.Max(340, rightEdge);
                pathPanel.Width = Math.Max(340, availableWidth - pathPanel.Left);
                serverMetaPanel.Width = Math.Max(340, availableWidth - serverMetaPanel.Left);
                actionGroupsPanel.Width = Math.Max(340, availableWidth - actionGroupsPanel.Left);

                if (compactHeader)
                {
                    pathTextBox.Width = Math.Max(180, pathPanel.ClientSize.Width - 8);
                }
                else
                {
                    var pathReservedWidth = browseButton.Width + loadButton.Width
                        + browseButton.Margin.Horizontal + loadButton.Margin.Horizontal + 24;
                    pathTextBox.Width = Math.Max(140, pathPanel.ClientSize.Width - pathReservedWidth);
                }

                layoutProvisionRows();
                topHeaderPanel.Height = actionGroupsPanel.Bottom + 12;
            };

            Resize += delegate { applyTopHeaderLayout(); };
            Load += delegate { applyTopHeaderLayout(); };

            var operationsScrollPanel = new Panel();
            operationsScrollPanel.Dock = DockStyle.Fill;
            operationsScrollPanel.AutoScroll = true;
            operationsScrollPanel.Margin = new Padding(0);
            operationsTab.Controls.Add(operationsScrollPanel);

            var operationsLayout = new TableLayoutPanel();
            operationsLayout.Dock = DockStyle.Top;
            operationsLayout.AutoSize = true;
            operationsLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            operationsLayout.ColumnCount = 1;
            operationsLayout.RowCount = 2;
            operationsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            operationsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            operationsScrollPanel.Controls.Add(operationsLayout);

            var logToolbar = new FlowLayoutPanel();
            logToolbar.Dock = DockStyle.Top;
            logToolbar.AutoSize = true;
            logToolbar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            logToolbar.Height = 34;
            logToolbar.WrapContents = true;
            operationsLayout.Controls.Add(logToolbar, 0, 0);

            clearLogButton = new Button();
            clearLogButton.Text = "Clear Logs";
            clearLogButton.Width = 100;
            clearLogButton.Click += delegate { ClearLogs(); };
            logToolbar.Controls.Add(clearLogButton);

            exportLogsButton = new Button();
            exportLogsButton.Text = "Export Logs";
            exportLogsButton.Width = 100;
            exportLogsButton.Click += delegate { ExportLogs(); };
            logToolbar.Controls.Add(exportLogsButton);

            var filterLabel = new Label();
            filterLabel.Text = "Keyword Filter:";
            filterLabel.AutoSize = true;
            filterLabel.Padding = new Padding(8, 8, 0, 0);
            logToolbar.Controls.Add(filterLabel);

            logFilterTextBox = new TextBox();
            logFilterTextBox.Width = 240;
            logFilterTextBox.KeyDown += delegate(object sender, KeyEventArgs args)
            {
                if (args.KeyCode == Keys.Enter)
                {
                    ApplyLogFilter();
                    args.SuppressKeyPress = true;
                }
            };
            logToolbar.Controls.Add(logFilterTextBox);

            applyLogFilterButton = new Button();
            applyLogFilterButton.Text = "Apply Filter";
            applyLogFilterButton.Width = 100;
            applyLogFilterButton.Click += delegate { ApplyLogFilter(); };
            logToolbar.Controls.Add(applyLogFilterButton);

            var logGroup = CreateGroupBox("Live Log", 975, 360);
            logGroup.Dock = DockStyle.Top;
            logGroup.Height = 360;
            logGroup.Margin = new Padding(0);
            operationsLayout.Controls.Add(logGroup, 0, 1);
            configureTabScrollRange(operationsScrollPanel, operationsLayout);

            logTextBox = new RichTextBox();
            logTextBox.Dock = DockStyle.Fill;
            logTextBox.ScrollBars = RichTextBoxScrollBars.Vertical;
            logTextBox.ReadOnly = true;
            logTextBox.WordWrap = false;
            logTextBox.DetectUrls = false;
            logGroup.Controls.Add(logTextBox);

            var sidebarGroup = CreateGroupBox("Discovered Worlds", 320, 620);
            sidebarGroup.Dock = DockStyle.Fill;
            splitContainer.Panel2.Controls.Add(sidebarGroup);

            worldsListView = new ThemedListView();
            worldsListView.Dock = DockStyle.Fill;
            worldsListView.View = View.Details;
            worldsListView.FullRowSelect = true;
            worldsListView.HideSelection = false;
            worldsListView.MultiSelect = false;
            worldsListView.GridLines = true;
            worldsListView.Columns.Add("World", 135);
            worldsListView.Columns.Add("Version", 70);
            worldsListView.Columns.Add("State", 65);
            worldsListView.Columns.Add("Preset", 60);
            worldsListView.Columns.Add("Folder ID", 240);
            worldsListView.SelectedIndexChanged += delegate { OnSelectedWorldChanged(); };
            sidebarGroup.Controls.Add(worldsListView);

            WireFieldEvents();
            ApplyTheme();
            ConfigureToolTips();
            UpdateProcessUi();
            UpdateLaunchTargetUi();

            pathTextBox.Text = DefaultServerDir;
            steamCmdPathTextBox.Text = DefaultSteamCmdDir;
            installDirTextBox.Text = DefaultServerDir;
            backupFolderTextBox.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WindroseBackups");
            themeComboBox.SelectedItem = currentThemeName;
            rconBindAddressTextBox.Text = "0.0.0.0";
            rconPortNumeric.Value = 27065;
            rconPasswordTextBox.Text = "windrose_admin";
            rconAllowedIpsTextBox.Text = string.Empty;
            rconMaxFailedAttemptsNumeric.Value = 5;
            rconTimeoutNumeric.Value = 60;
            rconEnableLoggingCheckBox.Checked = true;
            rconSecureEnabledCheckBox.Checked = false;
            rconAesKeyTextBox.Text = string.Empty;

            Load += delegate
            {
                LoadPreferencesIntoUi();
                RefreshModsProviderUi();
                TryAutoLoadLastServer();
                ApplyNativeControlTheme();
            };

            FormClosing += delegate
            {
                SavePreferences();
            };

            SetConfigEditorsEnabled(false);
        }
    }
}
