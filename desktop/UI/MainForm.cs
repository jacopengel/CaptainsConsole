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
using System.Threading.Tasks;
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
        private const string NexusModsGameDomainName = "windrose";
        private const string EmbeddedAppIconResourceName = "WindroseServerManager.Resources.AppIcon";
        private const string UpdateFeedFileName = "update-feed-url.txt";
        private const string WindroseDedicatedServerGuideUrl = "https://playwindrose.com/dedicated-server-guide/";
        private const string WindroseDedicatedServerGuideFallbackUrl = "https://playwindrose.com/windrose-crew/dedicated-server-guide/";
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
        private readonly Button checkServerUpdatesButton;
        private readonly Button installSteamCmdButton;
        private readonly Button installServerButton;
        private readonly Button updateServerButton;
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
        private readonly Button browseRconDllButton;
        private readonly Button saveRconSettingsButton;
        private readonly Button testRconButton;
        private readonly Button refreshRconPlayersButton;
        private readonly Button viewRconLicenseButton;
        private readonly Button rconHelpButton;
        private readonly Button rconInfoButton;
        private readonly Button rconShowPlayersButton;
        private readonly Button rconPlayerInfoButton;
        private readonly Button rconGetPosButton;
        private readonly Button rconKickButton;
        private readonly Button rconBanButton;
        private readonly Button rconUnbanButton;
        private readonly Button rconBanListButton;
        private readonly Button saveDiscordSettingsButton;
        private readonly Button connectDiscordButton;
        private readonly Button publishDiscordPublicPanelButton;
        private readonly Button publishDiscordAdminPanelButton;
        private readonly Button refreshDiscordPanelsButton;
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
        private readonly Label appVersionLabel;
        private readonly Label appUpdateStatusLabel;
        private readonly Label installedServerVersionLabel;
        private readonly Label latestServerVersionLabel;
        private readonly Label serverUpdateSummaryLabel;
        private readonly Button appUpdateButton;
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
        private readonly ThemedListView rconPlayersListView;
        private readonly RichTextBox logTextBox;
        private readonly RichTextBox rconPlayersTextBox;
        private readonly TextBox logFilterTextBox;
        private readonly TextBox backupFolderTextBox;
        private readonly TextBox rconDllPathTextBox;
        private readonly TextBox rconSelectedAccountIdTextBox;
        private readonly TextBox rconBanReasonTextBox;
        private readonly ComboBox modsProviderComboBox;
        private readonly TextBox modsApiKeyTextBox;
        private readonly Button saveModsApiKeyButton;
        private readonly TextBox rconBindAddressTextBox;
        private readonly NumericUpDown rconPortNumeric;
        private readonly TextBox rconPasswordTextBox;
        private readonly TextBox rconAllowedIpsTextBox;
        private readonly NumericUpDown rconMaxFailedAttemptsNumeric;
        private readonly NumericUpDown rconTimeoutNumeric;
        private readonly TextBox discordBotTokenTextBox;
        private readonly TextBox discordGuildIdTextBox;
        private readonly TextBox discordPublicChannelIdTextBox;
        private readonly TextBox discordAdminChannelIdTextBox;
        private readonly TextBox discordAdminRoleIdsTextBox;
        private readonly NumericUpDown discordRefreshSecondsNumeric;
        private readonly CheckBox discordAutoConnectCheckBox;
        private readonly Label discordStatusLabel;
        private readonly CheckBox rconEnableLoggingCheckBox;
        private readonly CheckBox rconSecureEnabledCheckBox;
        private readonly TextBox rconAesKeyTextBox;
        private readonly ComboBox curseForgeSearchModeComboBox;
        private readonly TextBox curseForgeSearchTextBox;
        private readonly Label curseForgeStatusLabel;
        private readonly Label rconStatusLabel;
        private readonly CheckBox scheduledBackupEnabledCheckBox;
        private readonly CheckBox zipFullBackupsCheckBox;
        private readonly NumericUpDown scheduledBackupIntervalNumeric;
        private readonly NumericUpDown scheduledBackupRetentionNumeric;
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
        private readonly Timer discordRefreshTimer;

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
        private DateTime? lastPlayerCountRefreshUtc;
        private string lastKnownPlayerCount = "offline";
        private bool playerCountRefreshBusy;
        private bool updateCheckInProgress;
        private bool updateAvailable;
        private string currentApplicationVersion;
        private string availableUpdateVersion;
        private string availableUpdateDownloadUrl;
        private string availableUpdateNotes;
        private string appUpdateStatusMessage;
        private bool serverUpdateCheckInProgress;
        private bool serverUpdateAvailable;
        private string installedServerVersionDisplay;
        private string latestServerVersionDisplay;
        private string serverUpdateSummaryMessage;
        private readonly Panel topHeaderPanel;
        private string selectedModsProvider = ModProviderCurseForge;
        private string storedCurseForgeApiKey = string.Empty;
        private string storedNexusModsApiKey = string.Empty;
        private string storedDiscordBotToken = string.Empty;
        private string storedDiscordGuildId = string.Empty;
        private string storedDiscordPublicChannelId = string.Empty;
        private string storedDiscordAdminChannelId = string.Empty;
        private string storedDiscordAdminRoleIds = string.Empty;
        private bool discordAutoConnect;
        private ulong discordPublicMessageId;
        private ulong discordAdminMessageId;
        private bool discordConnectionBusy;
        private bool discordPanelsBusy;
        private GroupBox discoverModsGroup;

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
            discordRefreshTimer = new Timer();
            discordRefreshTimer.Interval = 60000;
            discordRefreshTimer.Tick += delegate { HandleDiscordRefreshTick(); };
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
            currentApplicationVersion = GetCurrentApplicationVersion();
            availableUpdateVersion = string.Empty;
            availableUpdateDownloadUrl = string.Empty;
            availableUpdateNotes = string.Empty;
            appUpdateStatusMessage = "Updates not checked yet.";
            updateCheckInProgress = false;
            updateAvailable = false;
            serverUpdateCheckInProgress = false;
            serverUpdateAvailable = false;
            installedServerVersionDisplay = "not detected";
            latestServerVersionDisplay = "not checked";
            serverUpdateSummaryMessage = "Server update status not checked.";

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
            topHeaderPanel.Height = 392;
            topHeaderPanel.BackColor = Color.FromArgb(21, 33, 46);
            root.Controls.Add(topHeaderPanel, 0, 0);

            titleLabel = new Label();
            titleLabel.Text = AppTitle;
            titleLabel.Font = new Font("Georgia", 17F, FontStyle.Bold);
            titleLabel.AutoSize = true;
            titleLabel.Location = new Point(0, 4);
            titleLabel.ForeColor = Color.FromArgb(227, 197, 122);
            topHeaderPanel.Controls.Add(titleLabel);

            subtitleLabel = new Label();
            subtitleLabel.Text = "Locate or provision a server first, then run it, tune it, and manage its community mods.";
            subtitleLabel.AutoSize = true;
            subtitleLabel.ForeColor = Color.FromArgb(196, 205, 213);
            subtitleLabel.Location = new Point(2, 32);
            topHeaderPanel.Controls.Add(subtitleLabel);

            var appVersionPanel = new FlowLayoutPanel();
            appVersionPanel.Location = new Point(0, 66);
            appVersionPanel.Width = 1230;
            appVersionPanel.Height = 30;
            appVersionPanel.WrapContents = false;
            appVersionPanel.FlowDirection = FlowDirection.LeftToRight;
            appVersionPanel.BackColor = Color.Transparent;
            topHeaderPanel.Controls.Add(appVersionPanel);

            appVersionLabel = new Label();
            appVersionLabel.AutoSize = true;
            appVersionLabel.Padding = new Padding(0, 6, 12, 0);
            appVersionLabel.Text = "Version " + currentApplicationVersion;
            appVersionPanel.Controls.Add(appVersionLabel);

            appUpdateStatusLabel = new Label();
            appUpdateStatusLabel.AutoSize = true;
            appUpdateStatusLabel.Padding = new Padding(0, 6, 12, 0);
            appUpdateStatusLabel.Text = "Updates not checked yet.";
            appVersionPanel.Controls.Add(appUpdateStatusLabel);

            appUpdateButton = new Button();
            appUpdateButton.Text = "Check Updates";
            appUpdateButton.Width = 120;
            appUpdateButton.Click += delegate { HandleUpdateButtonClick(); };
            appVersionPanel.Controls.Add(appUpdateButton);

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
            pathPanel.Location = new Point(0, 100);
            pathPanel.Width = 1230;
            pathPanel.Height = 34;
            pathPanel.WrapContents = false;
            pathPanel.FlowDirection = FlowDirection.LeftToRight;
            pathPanel.BackColor = Color.FromArgb(21, 33, 46);
            topHeaderPanel.Controls.Add(pathPanel);
            pathPanel.Visible = false;

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
            serverMetaPanel.Location = new Point(0, 98);
            serverMetaPanel.Width = 1230;
            serverMetaPanel.Height = 24;
            serverMetaPanel.WrapContents = false;
            serverMetaPanel.FlowDirection = FlowDirection.LeftToRight;
            serverMetaPanel.BackColor = Color.Transparent;
            topHeaderPanel.Controls.Add(serverMetaPanel);

            var actionGroupsPanel = new TableLayoutPanel();
            actionGroupsPanel.Location = new Point(0, 126);
            actionGroupsPanel.Width = 1230;
            actionGroupsPanel.Height = 164;
            actionGroupsPanel.ColumnCount = 2;
            actionGroupsPanel.RowCount = 1;
            actionGroupsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            actionGroupsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            actionGroupsPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            actionGroupsPanel.Padding = new Padding(0, 0, 0, 6);
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
                appVersionPanel.Width = Math.Max(340, availableWidth - appVersionPanel.Left);
                serverMetaPanel.Width = Math.Max(340, availableWidth - serverMetaPanel.Left);
                actionGroupsPanel.Width = Math.Max(340, availableWidth - actionGroupsPanel.Left);

                appUpdateStatusLabel.MaximumSize = new Size(Math.Max(180, appVersionPanel.ClientSize.Width - appVersionLabel.Width - appUpdateButton.Width - 48), 0);
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
            provisionGroup.Dock = DockStyle.Top;
            provisionGroup.Margin = new Padding(0, 0, 10, 4);
            var actionsColumnPanel = new TableLayoutPanel();
            actionsColumnPanel.Dock = DockStyle.Top;
            actionsColumnPanel.AutoSize = true;
            actionsColumnPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            actionsColumnPanel.ColumnCount = 1;
            actionsColumnPanel.RowCount = 2;
            actionsColumnPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            actionsColumnPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            actionsColumnPanel.Margin = new Padding(0);
            actionsColumnPanel.BackColor = Color.Transparent;
            var runtimeGroup = CreateGroupBox("Raise Anchor", 0, 0);
            runtimeGroup.Dock = DockStyle.Top;
            runtimeGroup.Margin = new Padding(0, 0, 0, 6);
            var utilityGroup = CreateGroupBox("Deck Tools", 0, 0);
            utilityGroup.Dock = DockStyle.Top;
            utilityGroup.Margin = new Padding(0);
            actionGroupsPanel.Controls.Add(provisionGroup, 0, 0);
            actionGroupsPanel.Controls.Add(actionsColumnPanel, 1, 0);
            actionsColumnPanel.Controls.Add(runtimeGroup, 0, 0);
            actionsColumnPanel.Controls.Add(utilityGroup, 0, 1);

            var provisionGroupLayout = new TableLayoutPanel();
            provisionGroupLayout.Dock = DockStyle.Top;
            provisionGroupLayout.AutoSize = true;
            provisionGroupLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            provisionGroupLayout.ColumnCount = 1;
            provisionGroupLayout.RowCount = 4;
            provisionGroupLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            provisionGroupLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            provisionGroupLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            provisionGroupLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            provisionGroupLayout.BackColor = Color.FromArgb(21, 33, 46);
            provisionGroup.Controls.Add(provisionGroupLayout);

            var provisionButtonsPanel = new FlowLayoutPanel();
            provisionButtonsPanel.Dock = DockStyle.Top;
            provisionButtonsPanel.AutoSize = true;
            provisionButtonsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            provisionButtonsPanel.WrapContents = true;
            provisionButtonsPanel.AutoScroll = false;
            provisionButtonsPanel.Padding = new Padding(0, 6, 0, 2);
            provisionButtonsPanel.BackColor = Color.FromArgb(21, 33, 46);
            provisionGroupLayout.Controls.Add(provisionButtonsPanel, 0, 3);

            var runtimeButtonsPanel = new FlowLayoutPanel();
            runtimeButtonsPanel.Dock = DockStyle.Top;
            runtimeButtonsPanel.AutoSize = true;
            runtimeButtonsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            runtimeButtonsPanel.WrapContents = true;
            runtimeButtonsPanel.Padding = new Padding(0, 4, 0, 0);
            runtimeButtonsPanel.Margin = new Padding(0);
            runtimeButtonsPanel.BackColor = Color.FromArgb(21, 33, 46);
            runtimeGroup.Controls.Add(runtimeButtonsPanel);

            var utilityButtonsPanel = new FlowLayoutPanel();
            utilityButtonsPanel.Dock = DockStyle.Top;
            utilityButtonsPanel.AutoSize = true;
            utilityButtonsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            utilityButtonsPanel.WrapContents = true;
            utilityButtonsPanel.Padding = new Padding(0, 6, 0, 0);
            utilityButtonsPanel.Margin = new Padding(0);
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
            warningsGroup.Visible = false;
            warningsListBox = new ListBox();
            warningsListBox.Dock = DockStyle.Fill;
            warningsGroup.Controls.Add(warningsListBox);
            leftLayout.Controls.Add(warningsGroup, 0, 0);

            toggleWarningsButton = new Button();
            toggleWarningsButton.Text = "-";
            toggleWarningsButton.Width = 26;
            toggleWarningsButton.Height = 24;
            toggleWarningsButton.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            toggleWarningsButton.Visible = false;
            warningsGroup.Controls.Add(toggleWarningsButton);

            Action positionWarningsToggle = delegate
            {
                toggleWarningsButton.Left = 8;
                toggleWarningsButton.Top = 2;
            };
            warningsGroup.Resize += delegate { positionWarningsToggle(); };
            positionWarningsToggle();

            var warningsCollapsed = true;
            var warningsExpandedHeight = 150F;
            var manualWarningsCollapsed = true;
            Action<bool> setWarningsCollapsed = delegate(bool collapsed)
            {
                warningsCollapsed = collapsed;
                warningsListBox.Visible = !collapsed;
                leftLayout.RowStyles[0].Height = 0F;
                toggleWarningsButton.Text = collapsed ? "+" : "-";
                positionWarningsToggle();
            };
            toggleWarningsButton.Click += delegate
            {
                manualWarningsCollapsed = !warningsCollapsed;
                setWarningsCollapsed(manualWarningsCollapsed);
            };
            setWarningsCollapsed(true);

            Action updateResponsiveShellLayout = delegate
            {
                var compactShell = ClientSize.Height < 860 || ClientSize.Width < 1180;
                var veryCompactShell = ClientSize.Height < 740 || ClientSize.Width < 980;

                root.Padding = veryCompactShell ? new Padding(10) : (compactShell ? new Padding(12) : new Padding(16));
                root.RowStyles[1].Height = veryCompactShell ? 30F : 34F;

                warningsExpandedHeight = veryCompactShell ? 56F : (compactShell ? 72F : 96F);
                setWarningsCollapsed(true);
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
            var rconTab = new ThemedTabPage("RCON");
            var discordTab = new ThemedTabPage("Discord");
            var backupTab = new ThemedTabPage("Backups");
            var quarterdeckTab = new ThemedTabPage("Quarterdeck");
            var operationsTab = new ThemedTabPage("Logbook");
            tabs.TabPages.Add(serverTab);
            tabs.TabPages.Add(worldTab);
            tabs.TabPages.Add(modsTab);
            tabs.TabPages.Add(manageModsTab);
            tabs.TabPages.Add(rconTab);
            tabs.TabPages.Add(discordTab);
            tabs.TabPages.Add(backupTab);
            tabs.TabPages.Add(quarterdeckTab);
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
                "\u2022 Full server backups can be saved as .zip archives to reduce disk usage.\n" +
                "\u2022 Restore Full Server supports both zipped backups and older folder-style backups.\n" +
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
            zipFullBackupsCheckBox = new CheckBox();
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

            scheduledBackupRetentionNumeric = new NumericUpDown();
            scheduledBackupRetentionNumeric.Minimum = 1;
            scheduledBackupRetentionNumeric.Maximum = 100;
            scheduledBackupRetentionNumeric.Value = 7;
            scheduledBackupRetentionNumeric.Width = 60;
            scheduledBackupRetentionNumeric.ValueChanged += delegate { ApplyScheduledBackupSettings(); };

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
            scheduledBackupTypeComboBox.SelectedIndexChanged += delegate { ApplyScheduledBackupSettings(); };
            scheduledControlsPanel.Controls.Add(scheduledBackupTypeComboBox);

            var scheduledRetentionLabel = new Label();
            scheduledRetentionLabel.Text = "Keep last:";
            scheduledRetentionLabel.AutoSize = true;
            scheduledRetentionLabel.Padding = new Padding(12, 8, 4, 0);
            scheduledControlsPanel.Controls.Add(scheduledRetentionLabel);

            scheduledControlsPanel.Controls.Add(scheduledBackupRetentionNumeric);

            var scheduledRetentionSuffixLabel = new Label();
            scheduledRetentionSuffixLabel.Text = "full backups";
            scheduledRetentionSuffixLabel.AutoSize = true;
            scheduledRetentionSuffixLabel.Padding = new Padding(4, 8, 12, 0);
            scheduledControlsPanel.Controls.Add(scheduledRetentionSuffixLabel);

            zipFullBackupsCheckBox.Text = "Zip Full Server Backups";
            zipFullBackupsCheckBox.AutoSize = true;
            zipFullBackupsCheckBox.Padding = new Padding(0, 5, 0, 0);
            zipFullBackupsCheckBox.Checked = true;
            zipFullBackupsCheckBox.CheckedChanged += delegate { UpdateScheduledBackupNextLabel(); SavePreferences(); };
            scheduledControlsPanel.Controls.Add(zipFullBackupsCheckBox);

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

            installRconButton = new Button();
            uninstallRconButton = new Button();
            browseRconDllButton = new Button();
            saveRconSettingsButton = new Button();
            testRconButton = new Button();
            refreshRconPlayersButton = new Button();
            viewRconLicenseButton = new Button();
            saveDiscordSettingsButton = new Button();
            connectDiscordButton = new Button();
            publishDiscordPublicPanelButton = new Button();
            publishDiscordAdminPanelButton = new Button();
            refreshDiscordPanelsButton = new Button();
            rconHelpButton = new Button();
            rconInfoButton = new Button();
            rconShowPlayersButton = new Button();
            rconPlayerInfoButton = new Button();
            rconGetPosButton = new Button();
            rconKickButton = new Button();
            rconBanButton = new Button();
            rconUnbanButton = new Button();
            rconBanListButton = new Button();
            rconStatusLabel = new Label();
            discordStatusLabel = new Label();
            rconDllPathTextBox = new TextBox();
            rconSelectedAccountIdTextBox = new TextBox();
            rconBanReasonTextBox = new TextBox();
            discordBotTokenTextBox = new TextBox();
            discordGuildIdTextBox = new TextBox();
            discordPublicChannelIdTextBox = new TextBox();
            discordAdminChannelIdTextBox = new TextBox();
            discordAdminRoleIdsTextBox = new TextBox();
            rconBindAddressTextBox = new TextBox();
            rconPortNumeric = new NumericUpDown();
            rconPasswordTextBox = new TextBox();
            rconAllowedIpsTextBox = new TextBox();
            rconMaxFailedAttemptsNumeric = new NumericUpDown();
            rconTimeoutNumeric = new NumericUpDown();
            discordRefreshSecondsNumeric = new NumericUpDown();
            discordAutoConnectCheckBox = new CheckBox();
            rconEnableLoggingCheckBox = new CheckBox();
            rconSecureEnabledCheckBox = new CheckBox();
            rconAesKeyTextBox = new TextBox();
            rconPlayersTextBox = new RichTextBox();
            rconPlayersListView = new ThemedListView();

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

            var rconScrollPanel = new Panel();
            rconScrollPanel.Dock = DockStyle.Fill;
            rconScrollPanel.AutoScroll = true;
            rconScrollPanel.Margin = new Padding(0);
            rconTab.Controls.Add(rconScrollPanel);

            var rconRootLayout = new TableLayoutPanel();
            rconRootLayout.Dock = DockStyle.Top;
            rconRootLayout.AutoSize = true;
            rconRootLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            rconRootLayout.Margin = new Padding(0);
            rconRootLayout.ColumnCount = 1;
            rconRootLayout.RowCount = 4;
            rconRootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rconRootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rconRootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rconRootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rconScrollPanel.Controls.Add(rconRootLayout);

            var rconInstallGroup = CreateGroupBox("Install WindroseRCON", 975, 190);
            rconInstallGroup.Dock = DockStyle.Top;
            rconInstallGroup.Margin = new Padding(0);
            rconRootLayout.Controls.Add(rconInstallGroup, 0, 0);

            var rconInstallLayout = new TableLayoutPanel();
            rconInstallLayout.Dock = DockStyle.Fill;
            rconInstallLayout.ColumnCount = 1;
            rconInstallLayout.RowCount = 4;
            rconInstallLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rconInstallLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rconInstallLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rconInstallLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rconInstallGroup.Controls.Add(rconInstallLayout);

            var rconIntroLabel = new Label();
            rconIntroLabel.AutoSize = true;
            rconIntroLabel.MaximumSize = new Size(920, 0);
            rconIntroLabel.Text = "Captain's Console does not bundle or auto-download WindroseRCON. Download the release yourself, select the local version.dll here, and the app will copy it into the loaded server's Win64 folder.";
            rconInstallLayout.Controls.Add(rconIntroLabel, 0, 0);

            var rconDllPanel = new FlowLayoutPanel();
            rconDllPanel.Dock = DockStyle.Top;
            rconDllPanel.AutoSize = true;
            rconDllPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            rconDllPanel.WrapContents = true;
            rconInstallLayout.Controls.Add(rconDllPanel, 0, 1);

            var rconDllLabel = new Label();
            rconDllLabel.Text = "Local version.dll:";
            rconDllLabel.AutoSize = true;
            rconDllLabel.Padding = new Padding(0, 8, 4, 0);
            rconDllPanel.Controls.Add(rconDllLabel);

            rconDllPathTextBox.Width = 500;
            rconDllPathTextBox.ReadOnly = true;
            rconDllPanel.Controls.Add(rconDllPathTextBox);

            browseRconDllButton.Text = "Browse DLL";
            browseRconDllButton.Width = 110;
            browseRconDllButton.Click += delegate { BrowseForRconVersionDll(); };
            rconDllPanel.Controls.Add(browseRconDllButton);

            var rconInstallButtonsPanel = new FlowLayoutPanel();
            rconInstallButtonsPanel.Dock = DockStyle.Top;
            rconInstallButtonsPanel.AutoSize = true;
            rconInstallButtonsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            rconInstallButtonsPanel.WrapContents = true;
            rconInstallLayout.Controls.Add(rconInstallButtonsPanel, 0, 2);

            installRconButton.Text = "Install Selected DLL";
            installRconButton.Width = 150;
            installRconButton.Click += delegate { InstallRconFiles(); };
            rconInstallButtonsPanel.Controls.Add(installRconButton);

            uninstallRconButton.Text = "Uninstall RCON";
            uninstallRconButton.Width = 120;
            uninstallRconButton.Click += delegate { UninstallRconFiles(); };
            rconInstallButtonsPanel.Controls.Add(uninstallRconButton);

            viewRconLicenseButton.Text = "Open Download Page";
            viewRconLicenseButton.Width = 145;
            viewRconLicenseButton.Click += delegate { OpenRconDownloadPage(); };
            rconInstallButtonsPanel.Controls.Add(viewRconLicenseButton);

            rconStatusLabel.AutoSize = true;
            rconStatusLabel.MaximumSize = new Size(920, 0);
            rconStatusLabel.Text = string.Empty;
            rconInstallLayout.Controls.Add(rconStatusLabel, 0, 3);
            SyncGroupBoxHeight(rconInstallGroup, rconInstallLayout);

            var rconSettingsGroup = CreateGroupBox("RCON Settings", 975, 250);
            rconSettingsGroup.Dock = DockStyle.Top;
            rconSettingsGroup.Margin = new Padding(0, 12, 0, 0);
            rconRootLayout.Controls.Add(rconSettingsGroup, 0, 1);

            var rconSettingsContainer = new TableLayoutPanel();
            rconSettingsContainer.Dock = DockStyle.Top;
            rconSettingsContainer.AutoSize = true;
            rconSettingsContainer.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            rconSettingsContainer.ColumnCount = 1;
            rconSettingsContainer.RowCount = 2;
            rconSettingsContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rconSettingsContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rconSettingsGroup.Controls.Add(rconSettingsContainer);

            var rconSettingsLayout = CreateFieldGrid();
            rconSettingsLayout.MinimumSize = new Size(0, 0);
            rconSettingsContainer.Controls.Add(rconSettingsLayout, 0, 0);

            AddLabeledControl(rconSettingsLayout, "Bind Address", rconBindAddressTextBox, 0, 0, 220);
            AddLabeledControl(rconSettingsLayout, "Port", rconPortNumeric, 1, 0, 140);
            AddLabeledControl(rconSettingsLayout, "Password", rconPasswordTextBox, 2, 0, 220);
            AddLabeledControl(rconSettingsLayout, "Allowed IPs", rconAllowedIpsTextBox, 3, 0, 220);
            AddLabeledControl(rconSettingsLayout, "Max Failed Attempts", rconMaxFailedAttemptsNumeric, 0, 1, 140);
            AddLabeledControl(rconSettingsLayout, "Timeout Seconds", rconTimeoutNumeric, 1, 1, 140);
            AddLabeledControl(rconSettingsLayout, "AES Key", rconAesKeyTextBox, 2, 1, 220);
            rconEnableLoggingCheckBox.Text = "Enable Logging";
            rconEnableLoggingCheckBox.AutoSize = true;
            AddLabeledControl(rconSettingsLayout, "Options", rconEnableLoggingCheckBox, 3, 1, 180);
            rconSecureEnabledCheckBox.Text = "Enable Secure RCON";
            rconSecureEnabledCheckBox.AutoSize = true;
            AddLabeledControl(rconSettingsLayout, "Secure", rconSecureEnabledCheckBox, 0, 2, 180);

            rconPortNumeric.Minimum = 1;
            rconPortNumeric.Maximum = 65535;
            rconPortNumeric.Width = 140;
            rconMaxFailedAttemptsNumeric.Minimum = 1;
            rconMaxFailedAttemptsNumeric.Maximum = 100;
            rconMaxFailedAttemptsNumeric.Width = 140;
            rconTimeoutNumeric.Minimum = 5;
            rconTimeoutNumeric.Maximum = 600;
            rconTimeoutNumeric.Width = 140;
            rconPasswordTextBox.UseSystemPasswordChar = true;
            rconAesKeyTextBox.UseSystemPasswordChar = true;

            ConfigureResponsiveFieldGrid(rconSettingsLayout, 220,
                rconBindAddressTextBox.Parent,
                rconPortNumeric.Parent,
                rconPasswordTextBox.Parent,
                rconAllowedIpsTextBox.Parent,
                rconMaxFailedAttemptsNumeric.Parent,
                rconTimeoutNumeric.Parent,
                rconAesKeyTextBox.Parent,
                rconEnableLoggingCheckBox.Parent,
                rconSecureEnabledCheckBox.Parent);

            var rconSettingsButtonsPanel = new FlowLayoutPanel();
            rconSettingsButtonsPanel.Dock = DockStyle.Top;
            rconSettingsButtonsPanel.AutoSize = true;
            rconSettingsButtonsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            rconSettingsButtonsPanel.WrapContents = true;
            rconSettingsButtonsPanel.Padding = new Padding(0, 8, 0, 0);
            rconSettingsContainer.Controls.Add(rconSettingsButtonsPanel, 0, 1);

            saveRconSettingsButton.Text = "Save Settings";
            saveRconSettingsButton.Width = 120;
            saveRconSettingsButton.Click += delegate { SaveRconSettings(); };
            rconSettingsButtonsPanel.Controls.Add(saveRconSettingsButton);

            testRconButton.Text = "Test RCON";
            testRconButton.Width = 110;
            testRconButton.Click += delegate { TestRconConnection(); };
            rconSettingsButtonsPanel.Controls.Add(testRconButton);

            refreshRconPlayersButton.Text = "Refresh Players";
            refreshRconPlayersButton.Width = 130;
            refreshRconPlayersButton.Click += delegate { RefreshRconPlayers(); };
            rconSettingsButtonsPanel.Controls.Add(refreshRconPlayersButton);
            SyncGroupBoxHeight(rconSettingsGroup, rconSettingsContainer);

            var rconCommandsGroup = CreateGroupBox("RCON Commands", 975, 240);
            rconCommandsGroup.Dock = DockStyle.Top;
            rconCommandsGroup.Margin = new Padding(0, 12, 0, 0);
            rconRootLayout.Controls.Add(rconCommandsGroup, 0, 2);

            var rconCommandsLayout = new TableLayoutPanel();
            rconCommandsLayout.Dock = DockStyle.Top;
            rconCommandsLayout.AutoSize = true;
            rconCommandsLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            rconCommandsLayout.ColumnCount = 1;
            rconCommandsLayout.RowCount = 3;
            rconCommandsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rconCommandsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rconCommandsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rconCommandsGroup.Controls.Add(rconCommandsLayout);

            var rconTargetPanel = new FlowLayoutPanel();
            rconTargetPanel.Dock = DockStyle.Top;
            rconTargetPanel.AutoSize = true;
            rconTargetPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            rconTargetPanel.WrapContents = true;
            rconCommandsLayout.Controls.Add(rconTargetPanel, 0, 0);

            var accountIdLabel = new Label();
            accountIdLabel.Text = "Account ID:";
            accountIdLabel.AutoSize = true;
            accountIdLabel.Padding = new Padding(0, 8, 4, 0);
            rconTargetPanel.Controls.Add(accountIdLabel);

            rconSelectedAccountIdTextBox.Width = 220;
            rconTargetPanel.Controls.Add(rconSelectedAccountIdTextBox);

            var reasonLabel = new Label();
            reasonLabel.Text = "Ban Reason:";
            reasonLabel.AutoSize = true;
            reasonLabel.Padding = new Padding(10, 8, 4, 0);
            rconTargetPanel.Controls.Add(reasonLabel);

            rconBanReasonTextBox.Width = 260;
            rconTargetPanel.Controls.Add(rconBanReasonTextBox);

            var rconReadButtonsPanel = new FlowLayoutPanel();
            rconReadButtonsPanel.Dock = DockStyle.Top;
            rconReadButtonsPanel.AutoSize = true;
            rconReadButtonsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            rconReadButtonsPanel.WrapContents = true;
            rconCommandsLayout.Controls.Add(rconReadButtonsPanel, 0, 1);

            rconHelpButton.Text = "Help";
            rconHelpButton.Width = 90;
            rconHelpButton.Click += delegate { RunNamedRconCommand("help"); };
            rconReadButtonsPanel.Controls.Add(rconHelpButton);

            rconInfoButton.Text = "Info";
            rconInfoButton.Width = 90;
            rconInfoButton.Click += delegate { RunNamedRconCommand("info"); };
            rconReadButtonsPanel.Controls.Add(rconInfoButton);

            rconShowPlayersButton.Text = "Show Players";
            rconShowPlayersButton.Width = 110;
            rconShowPlayersButton.Click += delegate { RefreshRconPlayers(); };
            rconReadButtonsPanel.Controls.Add(rconShowPlayersButton);

            rconPlayerInfoButton.Text = "Player Info";
            rconPlayerInfoButton.Width = 110;
            rconPlayerInfoButton.Click += delegate { RunNamedRconCommand("playerinfo"); };
            rconReadButtonsPanel.Controls.Add(rconPlayerInfoButton);

            rconGetPosButton.Text = "Get Position";
            rconGetPosButton.Width = 110;
            rconGetPosButton.Click += delegate { RunNamedRconCommand("getpos"); };
            rconReadButtonsPanel.Controls.Add(rconGetPosButton);

            rconBanListButton.Text = "Ban List";
            rconBanListButton.Width = 100;
            rconBanListButton.Click += delegate { RunNamedRconCommand("banlist"); };
            rconReadButtonsPanel.Controls.Add(rconBanListButton);

            var rconWriteButtonsPanel = new FlowLayoutPanel();
            rconWriteButtonsPanel.Dock = DockStyle.Top;
            rconWriteButtonsPanel.AutoSize = true;
            rconWriteButtonsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            rconWriteButtonsPanel.WrapContents = true;
            rconCommandsLayout.Controls.Add(rconWriteButtonsPanel, 0, 2);

            rconKickButton.Text = "Kick Player";
            rconKickButton.Width = 110;
            rconKickButton.Click += delegate { RunNamedRconCommand("kick"); };
            rconWriteButtonsPanel.Controls.Add(rconKickButton);

            rconBanButton.Text = "Ban Player";
            rconBanButton.Width = 110;
            rconBanButton.Click += delegate { RunNamedRconCommand("ban"); };
            rconWriteButtonsPanel.Controls.Add(rconBanButton);

            rconUnbanButton.Text = "Unban Player";
            rconUnbanButton.Width = 120;
            rconUnbanButton.Click += delegate { RunNamedRconCommand("unban"); };
            rconWriteButtonsPanel.Controls.Add(rconUnbanButton);
            SyncGroupBoxHeight(rconCommandsGroup, rconCommandsLayout);

            var rconPlayersGroup = CreateGroupBox("Online Players", 975, 220);
            rconPlayersGroup.Dock = DockStyle.Top;
            rconPlayersGroup.Margin = new Padding(0, 12, 0, 0);
            rconRootLayout.Controls.Add(rconPlayersGroup, 0, 3);

            var rconPlayersLayout = new TableLayoutPanel();
            rconPlayersLayout.Dock = DockStyle.Fill;
            rconPlayersLayout.ColumnCount = 1;
            rconPlayersLayout.RowCount = 2;
            rconPlayersLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));
            rconPlayersLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 55F));
            rconPlayersGroup.Controls.Add(rconPlayersLayout);

            rconPlayersListView.Dock = DockStyle.Fill;
            rconPlayersListView.View = View.Details;
            rconPlayersListView.FullRowSelect = true;
            rconPlayersListView.HideSelection = false;
            rconPlayersListView.MultiSelect = false;
            rconPlayersListView.GridLines = true;
            rconPlayersListView.Columns.Add("Player", 260);
            rconPlayersListView.Columns.Add("Account ID", 220);
            rconPlayersListView.Columns.Add("Details", 360);
            rconPlayersListView.SelectedIndexChanged += delegate { OnSelectedRconPlayerChanged(); };
            rconPlayersLayout.Controls.Add(rconPlayersListView, 0, 0);

            rconPlayersTextBox.Dock = DockStyle.Fill;
            rconPlayersTextBox.ReadOnly = true;
            rconPlayersTextBox.WordWrap = false;
            rconPlayersTextBox.DetectUrls = false;
            rconPlayersTextBox.Text = "Install WindroseRCON, save your settings, then use Test RCON or Refresh Players.";
            rconPlayersLayout.Controls.Add(rconPlayersTextBox, 0, 1);

            configureTabScrollRange(rconScrollPanel, rconRootLayout);

            var discordScrollPanel = new Panel();
            discordScrollPanel.Dock = DockStyle.Fill;
            discordScrollPanel.AutoScroll = true;
            discordScrollPanel.Margin = new Padding(0);
            discordTab.Controls.Add(discordScrollPanel);

            var discordRootLayout = new TableLayoutPanel();
            discordRootLayout.Dock = DockStyle.Top;
            discordRootLayout.AutoSize = true;
            discordRootLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            discordRootLayout.Margin = new Padding(0);
            discordRootLayout.ColumnCount = 1;
            discordRootLayout.RowCount = 2;
            discordRootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            discordRootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            discordScrollPanel.Controls.Add(discordRootLayout);

            var discordSettingsGroup = CreateGroupBox("Discord Bot", 975, 250);
            discordSettingsGroup.Dock = DockStyle.Top;
            discordSettingsGroup.Margin = new Padding(0);
            discordRootLayout.Controls.Add(discordSettingsGroup, 0, 0);

            var discordSettingsContainer = new TableLayoutPanel();
            discordSettingsContainer.Dock = DockStyle.Top;
            discordSettingsContainer.AutoSize = true;
            discordSettingsContainer.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            discordSettingsContainer.ColumnCount = 1;
            discordSettingsContainer.RowCount = 3;
            discordSettingsContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            discordSettingsContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            discordSettingsContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            discordSettingsGroup.Controls.Add(discordSettingsContainer);

            var discordIntroLabel = new Label();
            discordIntroLabel.AutoSize = true;
            discordIntroLabel.MaximumSize = new Size(920, 0);
            discordIntroLabel.Text = "Discord integration runs only while Captain's Console is open. Connect a bot here, then publish a public status panel and an admin panel that can trigger RCON actions.";
            discordSettingsContainer.Controls.Add(discordIntroLabel, 0, 0);

            var discordSettingsLayout = CreateFieldGrid();
            discordSettingsLayout.MinimumSize = new Size(0, 0);
            discordSettingsContainer.Controls.Add(discordSettingsLayout, 0, 1);

            AddLabeledControl(discordSettingsLayout, "Bot Token", discordBotTokenTextBox, 0, 0, 280);
            AddLabeledControl(discordSettingsLayout, "Guild ID", discordGuildIdTextBox, 1, 0, 220);
            AddLabeledControl(discordSettingsLayout, "Public Channel ID", discordPublicChannelIdTextBox, 2, 0, 220);
            AddLabeledControl(discordSettingsLayout, "Admin Channel ID", discordAdminChannelIdTextBox, 3, 0, 220);
            AddLabeledControl(discordSettingsLayout, "Admin Role IDs", discordAdminRoleIdsTextBox, 0, 1, 320);
            AddLabeledControl(discordSettingsLayout, "Refresh Seconds", discordRefreshSecondsNumeric, 1, 1, 140);
            discordAutoConnectCheckBox.Text = "Connect on app start";
            discordAutoConnectCheckBox.AutoSize = true;
            AddLabeledControl(discordSettingsLayout, "Options", discordAutoConnectCheckBox, 2, 1, 180);

            discordBotTokenTextBox.UseSystemPasswordChar = true;
            discordRefreshSecondsNumeric.Minimum = 15;
            discordRefreshSecondsNumeric.Maximum = 3600;
            discordRefreshSecondsNumeric.Value = 60;

            ConfigureResponsiveFieldGrid(discordSettingsLayout, 220,
                discordBotTokenTextBox.Parent,
                discordGuildIdTextBox.Parent,
                discordPublicChannelIdTextBox.Parent,
                discordAdminChannelIdTextBox.Parent,
                discordAdminRoleIdsTextBox.Parent,
                discordRefreshSecondsNumeric.Parent,
                discordAutoConnectCheckBox.Parent);

            var discordButtonsPanel = new FlowLayoutPanel();
            discordButtonsPanel.Dock = DockStyle.Top;
            discordButtonsPanel.AutoSize = true;
            discordButtonsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            discordButtonsPanel.WrapContents = true;
            discordButtonsPanel.Padding = new Padding(0, 8, 0, 0);
            discordSettingsContainer.Controls.Add(discordButtonsPanel, 0, 2);

            saveDiscordSettingsButton.Text = "Save Discord Settings";
            saveDiscordSettingsButton.Width = 150;
            saveDiscordSettingsButton.Click += delegate { SaveDiscordSettings(); };
            discordButtonsPanel.Controls.Add(saveDiscordSettingsButton);

            connectDiscordButton.Text = "Connect Discord";
            connectDiscordButton.Width = 130;
            connectDiscordButton.Click += delegate { ToggleDiscordConnection(); };
            discordButtonsPanel.Controls.Add(connectDiscordButton);

            publishDiscordPublicPanelButton.Text = "Publish Public Panel";
            publishDiscordPublicPanelButton.Width = 150;
            publishDiscordPublicPanelButton.Click += delegate { PublishDiscordPublicPanel(); };
            discordButtonsPanel.Controls.Add(publishDiscordPublicPanelButton);

            publishDiscordAdminPanelButton.Text = "Publish Admin Panel";
            publishDiscordAdminPanelButton.Width = 150;
            publishDiscordAdminPanelButton.Click += delegate { PublishDiscordAdminPanel(); };
            discordButtonsPanel.Controls.Add(publishDiscordAdminPanelButton);

            refreshDiscordPanelsButton.Text = "Refresh Discord Panels";
            refreshDiscordPanelsButton.Width = 160;
            refreshDiscordPanelsButton.Click += delegate { RefreshDiscordPanels(); };
            discordButtonsPanel.Controls.Add(refreshDiscordPanelsButton);
            SyncGroupBoxHeight(discordSettingsGroup, discordSettingsContainer);

            var discordStatusGroup = CreateGroupBox("Discord Status", 975, 140);
            discordStatusGroup.Dock = DockStyle.Top;
            discordStatusGroup.Margin = new Padding(0, 12, 0, 0);
            discordRootLayout.Controls.Add(discordStatusGroup, 0, 1);

            discordStatusLabel.AutoSize = true;
            discordStatusLabel.MaximumSize = new Size(920, 0);
            discordStatusLabel.Text = "Discord bot is not connected.";
            discordStatusGroup.Controls.Add(discordStatusLabel);
            SyncGroupBoxHeight(discordStatusGroup, discordStatusLabel);
            configureTabScrollRange(discordScrollPanel, discordRootLayout);

            var steamCmdPanel = new TableLayoutPanel();
            steamCmdPanel.Dock = DockStyle.Top;
            steamCmdPanel.AutoSize = true;
            steamCmdPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            steamCmdPanel.ColumnCount = 4;
            steamCmdPanel.RowCount = 1;
            steamCmdPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            steamCmdPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            steamCmdPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            steamCmdPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            steamCmdPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            steamCmdPanel.BackColor = Color.FromArgb(21, 33, 46);
            steamCmdPanel.Padding = new Padding(0, 4, 0, 2);
            provisionGroupLayout.Controls.Add(steamCmdPanel, 0, 0);

            var steamCmdLabel = new Label();
            steamCmdLabel.Text = "SteamCMD:";
            steamCmdLabel.AutoSize = true;
            steamCmdLabel.TextAlign = ContentAlignment.MiddleLeft;
            steamCmdLabel.Dock = DockStyle.Fill;
            steamCmdPanel.Controls.Add(steamCmdLabel, 0, 0);

            steamCmdPathTextBox = new TextBox();
            steamCmdPathTextBox.Dock = DockStyle.Fill;
            steamCmdPanel.Controls.Add(steamCmdPathTextBox, 1, 0);

            browseSteamCmdButton = new Button();
            browseSteamCmdButton.Text = "Browse SteamCMD Folder";
            browseSteamCmdButton.AutoSize = false;
            browseSteamCmdButton.Width = 210;
            browseSteamCmdButton.Margin = new Padding(8, 0, 0, 0);
            browseSteamCmdButton.Click += delegate { BrowseForSteamCmd(); };
            steamCmdPanel.Controls.Add(browseSteamCmdButton, 2, 0);

            var installDirPanel = new TableLayoutPanel();
            installDirPanel.Dock = DockStyle.Top;
            installDirPanel.AutoSize = true;
            installDirPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            installDirPanel.ColumnCount = 3;
            installDirPanel.RowCount = 1;
            installDirPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            installDirPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            installDirPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            installDirPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            installDirPanel.BackColor = Color.FromArgb(21, 33, 46);
            installDirPanel.Padding = new Padding(0, 4, 0, 2);
            provisionGroupLayout.Controls.Add(installDirPanel, 0, 1);

            var installDirLabel = new Label();
            installDirLabel.Text = "Install Dir:";
            installDirLabel.AutoSize = true;
            installDirLabel.TextAlign = ContentAlignment.MiddleLeft;
            installDirLabel.Dock = DockStyle.Fill;
            installDirPanel.Controls.Add(installDirLabel, 0, 0);

            installDirTextBox = new TextBox();
            installDirTextBox.Dock = DockStyle.Fill;
            installDirPanel.Controls.Add(installDirTextBox, 1, 0);

            browseInstallDirButton = new Button();
            browseInstallDirButton.Text = "Browse Install Folder";
            browseInstallDirButton.AutoSize = false;
            browseInstallDirButton.Width = 178;
            browseInstallDirButton.Margin = new Padding(8, 0, 0, 0);
            browseInstallDirButton.Click += delegate { BrowseForInstallDirectory(); };
            installDirPanel.Controls.Add(browseInstallDirButton, 2, 0);

            var serverUpdateInfoPanel = new TableLayoutPanel();
            serverUpdateInfoPanel.Dock = DockStyle.Fill;
            serverUpdateInfoPanel.ColumnCount = 2;
            serverUpdateInfoPanel.RowCount = 1;
            serverUpdateInfoPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            serverUpdateInfoPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            serverUpdateInfoPanel.BackColor = Color.FromArgb(21, 33, 46);
            serverUpdateInfoPanel.AutoSize = true;
            serverUpdateInfoPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            serverUpdateInfoPanel.Margin = new Padding(0, 4, 0, 0);
            provisionGroupLayout.Controls.Add(serverUpdateInfoPanel, 0, 2);

            var serverUpdateLabelsPanel = new TableLayoutPanel();
            serverUpdateLabelsPanel.Dock = DockStyle.Fill;
            serverUpdateLabelsPanel.ColumnCount = 1;
            serverUpdateLabelsPanel.RowCount = 2;
            serverUpdateLabelsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            serverUpdateLabelsPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            serverUpdateLabelsPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            serverUpdateLabelsPanel.BackColor = Color.FromArgb(21, 33, 46);
            serverUpdateLabelsPanel.AutoSize = true;
            serverUpdateLabelsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            serverUpdateInfoPanel.Controls.Add(serverUpdateLabelsPanel, 0, 0);

            var serverVersionLinePanel = new FlowLayoutPanel();
            serverVersionLinePanel.Dock = DockStyle.Top;
            serverVersionLinePanel.AutoSize = true;
            serverVersionLinePanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            serverVersionLinePanel.WrapContents = true;
            serverVersionLinePanel.Margin = new Padding(0);
            serverVersionLinePanel.Padding = new Padding(0);
            serverVersionLinePanel.BackColor = Color.FromArgb(21, 33, 46);
            serverUpdateLabelsPanel.Controls.Add(serverVersionLinePanel, 0, 0);

            installedServerVersionLabel = new Label();
            installedServerVersionLabel.AutoSize = true;
            installedServerVersionLabel.Text = "Installed: not detected";
            installedServerVersionLabel.TextAlign = ContentAlignment.TopLeft;
            installedServerVersionLabel.Padding = new Padding(0, 2, 12, 0);
            serverVersionLinePanel.Controls.Add(installedServerVersionLabel);

            latestServerVersionLabel = new Label();
            latestServerVersionLabel.AutoSize = true;
            latestServerVersionLabel.Text = "Latest: not checked";
            latestServerVersionLabel.TextAlign = ContentAlignment.TopLeft;
            latestServerVersionLabel.Padding = new Padding(0, 2, 0, 0);
            serverVersionLinePanel.Controls.Add(latestServerVersionLabel);

            serverUpdateSummaryLabel = new Label();
            serverUpdateSummaryLabel.AutoSize = true;
            serverUpdateSummaryLabel.Dock = DockStyle.Top;
            serverUpdateSummaryLabel.Text = "Status: Server update status not checked.";
            serverUpdateSummaryLabel.TextAlign = ContentAlignment.TopLeft;
            serverUpdateSummaryLabel.Padding = new Padding(0, 2, 0, 0);
            serverUpdateSummaryLabel.MaximumSize = new Size(520, 0);
            serverUpdateLabelsPanel.Controls.Add(serverUpdateSummaryLabel, 0, 1);

            checkServerUpdatesButton = new Button();
            checkServerUpdatesButton.Text = "Check Latest Version";
            checkServerUpdatesButton.Width = 156;
            checkServerUpdatesButton.Height = 30;
            checkServerUpdatesButton.Margin = new Padding(12, 12, 0, 0);
            checkServerUpdatesButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            checkServerUpdatesButton.Click += delegate { BeginServerUpdateCheck(true); };
            serverUpdateInfoPanel.Controls.Add(checkServerUpdatesButton, 1, 0);


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
            SyncGroupBoxHeight(provisionGroup, provisionGroupLayout);

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
            playerCountTextLabel.Padding = new Padding(0, 4, 4, 0);
            playerCountTextLabel.Margin = new Padding(24, 0, 0, 0);
            playerCountTextLabel.Visible = false;
            serverMetaPanel.Controls.Add(playerCountTextLabel);

            playerCountValueLabel = new Label();
            playerCountValueLabel.AutoSize = true;
            playerCountValueLabel.Text = "offline";
            playerCountValueLabel.Padding = new Padding(0, 4, 0, 0);
            playerCountValueLabel.Visible = false;
            serverMetaPanel.Controls.Add(playerCountValueLabel);

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
            SyncGroupBoxHeight(runtimeGroup, runtimeButtonsPanel);
            SyncGroupBoxHeight(utilityGroup, utilityButtonsPanel);

            Action applyTopHeaderLayout = delegate
            {
                var compactHeader = ClientSize.Width < 1120 || ClientSize.Height < 760;
                var veryCompactHeader = ClientSize.Width < 920 || ClientSize.Height < 680;

                headerWatermarkBox.Visible = false;
                themeLabel.Visible = !veryCompactHeader;
                themeComboBox.Width = compactHeader ? 132 : 160;
                positionThemeControls();

                appVersionPanel.WrapContents = compactHeader;
                appVersionPanel.Height = compactHeader ? 46 : 28;
                pathPanel.Visible = false;
                pathPanel.Height = 0;

                serverMetaPanel.WrapContents = compactHeader;
                serverMetaPanel.Height = compactHeader ? 40 : 22;

                actionGroupsPanel.ColumnStyles[0].Width = compactHeader ? 68F : 70F;
                actionGroupsPanel.ColumnStyles[1].Width = compactHeader ? 32F : 30F;

                appVersionPanel.Location = new Point(0, subtitleLabel.Bottom + 4);
                serverMetaPanel.Location = new Point(0, appVersionPanel.Bottom + 4);
                actionGroupsPanel.Location = new Point(0, serverMetaPanel.Bottom + 4);

                var rightEdge = topHeaderPanel.ClientSize.Width - 8;
                var availableWidth = Math.Max(340, rightEdge);
                appVersionPanel.Width = Math.Max(340, availableWidth - appVersionPanel.Left);
                pathPanel.Width = Math.Max(340, availableWidth - pathPanel.Left);
                serverMetaPanel.Width = Math.Max(340, availableWidth - serverMetaPanel.Left);
                actionGroupsPanel.Width = Math.Max(340, availableWidth - actionGroupsPanel.Left);
                serverUpdateSummaryLabel.MaximumSize = new Size(Math.Max(280, (int)(actionGroupsPanel.Width * 0.52F)), 0);

                provisionGroup.PerformLayout();
                actionsColumnPanel.PerformLayout();
                runtimeGroup.PerformLayout();
                utilityGroup.PerformLayout();
                var desiredActionHeight = Math.Max(
                    provisionGroup.Height,
                    Math.Max(actionsColumnPanel.Height, runtimeGroup.Height + utilityGroup.Height + runtimeGroup.Margin.Bottom));
                actionGroupsPanel.Height = Math.Max(compactHeader ? 156 : 144, desiredActionHeight + actionGroupsPanel.Padding.Bottom + 2);

                appUpdateStatusLabel.MaximumSize = new Size(Math.Max(180, appVersionPanel.ClientSize.Width - appVersionLabel.Width - appUpdateButton.Width - 48), 0);

                topHeaderPanel.Height = actionGroupsPanel.Bottom + 8;
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

            Load += delegate
            {
                LoadPreferencesIntoUi();
                RefreshModsProviderUi();
                LoadRconSettingsIntoUi();
                RefreshRconStatusUi();
                RefreshDiscordUi();
                RefreshServerVersionInfo();
                UpdateAppVersionUi();
                BeginUpdateCheck(false);
                TryAutoLoadLastServer();
                BeginDiscordAutoConnect();
                ApplyNativeControlTheme();
            };

            FormClosing += delegate
            {
                ShutdownDiscordRuntime();
                SavePreferences();
            };

            SetConfigEditorsEnabled(false);
        }
    }
}
