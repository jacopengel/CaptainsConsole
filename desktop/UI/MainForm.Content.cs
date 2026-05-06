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
        private void ConfigureToolTips()
        {
            helpToolTip.SetToolTip(pathTextBox, "Point this at the Windrose dedicated server root folder.");
            helpToolTip.SetToolTip(browseButton, "Pick the server folder instead of typing the path manually.");
            helpToolTip.SetToolTip(loadButton, "Refresh from disk: reload ServerDescription.json and discovered world folders for the current path.");
            helpToolTip.SetToolTip(steamCmdPathTextBox, "Folder that contains steamcmd.exe. Windrose's official SteamCMD install/update flow uses app ID 4129620.");
            helpToolTip.SetToolTip(installDirTextBox, "Directory where Windrose Dedicated Server should be installed or updated.");
            helpToolTip.SetToolTip(chooseInstallFolderButton, "Pick where the dedicated server should be installed.");
            helpToolTip.SetToolTip(browseSteamCmdButton, "Browse to the folder that contains steamcmd.exe.");
            helpToolTip.SetToolTip(browseInstallDirButton, "Choose where the dedicated server should be installed.");
            helpToolTip.SetToolTip(openInstallDirButton, "Open the current install directory in Explorer. The folder is created if it does not exist yet.");
            helpToolTip.SetToolTip(installSteamCmdButton, "Download and set up SteamCMD in C:\\WindroseCC\\steamcmd using Valve's official Windows zip.");
            helpToolTip.SetToolTip(installServerButton, "Install the Windrose dedicated server into the selected install folder using SteamCMD.");
            helpToolTip.SetToolTip(updateServerButton, "Update the existing Windrose dedicated server in the selected install folder using SteamCMD.");
            helpToolTip.SetToolTip(deleteServerButton, "Delete the selected installed server folder. This is permanent.");
            helpToolTip.SetToolTip(backupFolderTextBox, "Folder where backup files and snapshots are stored.");
            helpToolTip.SetToolTip(browseBackupFolderButton, "Choose the folder where backups should be written.");
            helpToolTip.SetToolTip(backupServerButton, "Create a full backup copy of the current server folder.");
            helpToolTip.SetToolTip(restoreFullServerButton, "Restore the server from a full backup folder. Copies all files from the backup over the current server install. Server must be stopped first.");
            helpToolTip.SetToolTip(backupCaptainSettingsButton, "Backup ServerDescription.json (Captain settings).");
            helpToolTip.SetToolTip(restoreCaptainSettingsButton, "Restore ServerDescription.json from a previously backed up json file. The current live file is backed up first.");
            helpToolTip.SetToolTip(backupWorldSettingsButton, "Backup the selected world's WorldDescription.json.");
            helpToolTip.SetToolTip(restoreWorldSettingsButton, "Restore the selected world's WorldDescription.json from a backup json file. The current live file is backed up first.");
                helpToolTip.SetToolTip(scheduledBackupEnabledCheckBox, "Enable automatic scheduled backups on a repeating interval. The app must be running for scheduled backups to fire.");
                helpToolTip.SetToolTip(scheduledBackupIntervalNumeric, "How often to run the scheduled backup, in hours. Minimum 1 hour, maximum 720 hours (30 days).");
                helpToolTip.SetToolTip(scheduledBackupTypeComboBox, "What to back up when the schedule fires: Captain and World settings JSON files, or a full copy of the entire server folder.");
                helpToolTip.SetToolTip(scheduledBackupNextLabel, "Shows when the next scheduled backup will run.");
            helpToolTip.SetToolTip(scheduledRebootEnabledCheckBox, "Enable automatic server reboots at the selected date/time. Recurring mode keeps repeating on your chosen interval.");
                helpToolTip.SetToolTip(scheduledRebootDatePicker, "Select the first reboot date.");
                helpToolTip.SetToolTip(scheduledRebootTimePicker, "Select the reboot time of day.");
            helpToolTip.SetToolTip(recurringRebootCheckBox, "When enabled, reboot scheduling repeats forever using the Every value and unit.");
            helpToolTip.SetToolTip(recurringRebootIntervalNumeric, "Recurring interval count.");
            helpToolTip.SetToolTip(recurringRebootIntervalUnitComboBox, "Recurring interval unit: Hours or Days.");
            helpToolTip.SetToolTip(scheduledRebootNextLabel, "Shows when the next scheduled reboot will run.");
            helpToolTip.SetToolTip(rebootNowButton, "Restart the currently running server immediately.");
            helpToolTip.SetToolTip(createNewWorldButton, "Create a new world. If a world is selected it will be cloned; if no worlds exist yet, Captain's Console will create an initial default world.");
            helpToolTip.SetToolTip(importWorldButton, "Import a world folder or another Windrose server's saved worlds into this server.");
            helpToolTip.SetToolTip(deleteWorldButton, "Delete the selected world and its save folder permanently.");
            helpToolTip.SetToolTip(openSteamCmdGuideButton, "Open the official Windrose dedicated server guide with the SteamCMD instructions.");
            helpToolTip.SetToolTip(startServerButton, "Start the detected Windrose server launch target from inside this app.");
            helpToolTip.SetToolTip(stopServerButton, "Stop the process tree for the server launched by this app.");
            helpToolTip.SetToolTip(restartServerButton, "Restart the detected launch target.");
            helpToolTip.SetToolTip(serverStateTextLabel, "Current server lifecycle state shown as Starting, Running, Stopping, or Stopped.");
            helpToolTip.SetToolTip(serverStateDotPanel, "Current server lifecycle state shown as Starting, Running, Stopping, or Stopped.");
            helpToolTip.SetToolTip(clearLogButton, "Clear all captured lines from the Logbook view.");
            helpToolTip.SetToolTip(exportLogsButton, "Export currently visible log lines to a text file.");
            helpToolTip.SetToolTip(logFilterTextBox, "Show only log lines that contain this keyword.");
            helpToolTip.SetToolTip(applyLogFilterButton, "Apply the keyword filter to the Logbook.");
            helpToolTip.SetToolTip(openRootButton, "Open the loaded server folder in Explorer.");
            helpToolTip.SetToolTip(modsProviderComboBox, "Choose which mod platform this tab is currently targeting.");
            helpToolTip.SetToolTip(modsApiKeyTextBox, "Enter the API key for the selected mod provider. It will be saved in your local app preferences.");
            helpToolTip.SetToolTip(saveModsApiKeyButton, "Save the API key for the currently selected mod provider.");
            helpToolTip.SetToolTip(openCurseForgeButton, "Open the selected provider's Windrose page in your browser.");
            helpToolTip.SetToolTip(installRconButton, "Install WindroseRCON into the currently loaded server if a bundled or local version.dll is available.");
            helpToolTip.SetToolTip(uninstallRconButton, "Remove WindroseRCON version.dll from the current server.");
            helpToolTip.SetToolTip(saveRconSettingsButton, "Write the current RCON settings to windrosercon/settings.ini.");
            helpToolTip.SetToolTip(testRconButton, "Connect to the configured local RCON server and request basic server info.");
            helpToolTip.SetToolTip(refreshRconPlayersButton, "Refresh the live online player list through RCON.");
            helpToolTip.SetToolTip(viewRconLicenseButton, "View the bundled WindroseRCON third-party notice and Apache license.");
            helpToolTip.SetToolTip(rconBindAddressTextBox, "RCON bind address from windrosercon/settings.ini.");
            helpToolTip.SetToolTip(rconPortNumeric, "TCP port used by WindroseRCON.");
            helpToolTip.SetToolTip(rconPasswordTextBox, "Password required to authenticate with WindroseRCON.");
            helpToolTip.SetToolTip(rconAllowedIpsTextBox, "Optional comma-separated allowlist of client IPs.");
            helpToolTip.SetToolTip(rconMaxFailedAttemptsNumeric, "Number of failed authentication attempts allowed before temporary blocking.");
            helpToolTip.SetToolTip(rconTimeoutNumeric, "Per-client socket timeout in seconds.");
            helpToolTip.SetToolTip(rconEnableLoggingCheckBox, "Enable WindroseRCON activity logging.");
            helpToolTip.SetToolTip(rconSecureEnabledCheckBox, "Enable the encrypted Secure RCON protocol.");
            helpToolTip.SetToolTip(rconAesKeyTextBox, "AES key used by Secure RCON when enabled.");
            helpToolTip.SetToolTip(rconPlayersTextBox, "Latest online player list returned by the RCON showplayers command.");
            helpToolTip.SetToolTip(openModsFolderButton, "Open the community-convention ~mods folder for the loaded server root.");
            helpToolTip.SetToolTip(importModFolderButton, "Copy an extracted mod folder into the current ~mods folder.");
            helpToolTip.SetToolTip(curseForgeSearchModeComboBox, "Keyword searches platform listings. Mod ID fetches one exact item when supported by the selected provider.");
            helpToolTip.SetToolTip(curseForgeSearchTextBox, "Keyword mode: optional text filter. Mod ID mode: enter numeric mod ID when supported.");
            helpToolTip.SetToolTip(searchCurseForgeButton, "Search the selected mod provider.");
            helpToolTip.SetToolTip(installSelectedCurseForgeButton, "Download and install the selected provider item into ~mods when supported.");
            helpToolTip.SetToolTip(refreshInstalledModsButton, "Rescan ~mods and refresh the installed mods list.");
            helpToolTip.SetToolTip(enableModButton, "Enable selected mod by removing disabled prefix from its folder.");
            helpToolTip.SetToolTip(disableModButton, "Disable selected mod by prefixing its folder with _disabled_.");
            helpToolTip.SetToolTip(updateModButton, "Update the selected provider-installed mod when the current provider supports in-app updates.");
            helpToolTip.SetToolTip(removeModButton, "Delete selected mod folder from ~mods.");
            helpToolTip.SetToolTip(worldsListView, "Select which world to inspect or edit. The active world is the one the server will boot into.");
            helpToolTip.SetToolTip(serverNameTextBox, "ServerDescription.json: ServerName. Friendly display name for the server.");
            helpToolTip.SetToolTip(inviteCodeTextBox, "ServerDescription.json: InviteCode. Official rules: at least 6 characters, only 0-9, a-z, and A-Z.");
            helpToolTip.SetToolTip(passwordTextBox, "Optional server password. If non-empty, password protection will be enabled on save.");
            helpToolTip.SetToolTip(maxPlayersNumeric, "ServerDescription.json: MaxPlayerCount. The app allows up to 20 here, but official guidance still recommends smaller crews for smoother performance.");
            helpToolTip.SetToolTip(activeWorldIdTextBox, "ServerDescription.json: WorldIslandId. It must match the world folder name and the world's islandId.");
            helpToolTip.SetToolTip(regionComboBox, "ServerDescription.json: UserSelectedRegion. Auto lets Windrose choose. EU currently covers both EU and NA.");
            helpToolTip.SetToolTip(directConnectionCheckBox, "ServerDescription.json: UseDirectConnection. Direct IP mode on or off.");
            helpToolTip.SetToolTip(directPortNumeric, "ServerDescription.json: DirectConnectionServerPort. Port used for Direct IP mode. It must be available for both TCP and UDP.");
            helpToolTip.SetToolTip(bindInterfaceTextBox, "ServerDescription.json: DirectConnectionProxyAddress. This must be valid on the actual server host, not necessarily on the PC running Captain's Console. Usually 0.0.0.0 unless you need a specific host interface.");
            helpToolTip.SetToolTip(listeningIpTextBox, "ServerDescription.json: P2pProxyAddress. This must be valid on the actual server host, not necessarily on the PC running Captain's Console.");
            helpToolTip.SetToolTip(directAddressTextBox, "ServerDescription.json: DirectConnectionServerAddress. This must be valid on the actual server host, not necessarily on the PC running Captain's Console.");
            helpToolTip.SetToolTip(worldNameTextBox, "Human-readable name of the selected world.");
            helpToolTip.SetToolTip(worldPresetTextBox, "Detected preset based on the current world values. Manual edits usually turn this into Custom.");
            helpToolTip.SetToolTip(combatDifficultyComboBox, "Boss aggression preset for the selected world.");
            helpToolTip.SetToolTip(sharedQuestsCheckBox, "If enabled, some co-op quest completions are shared. This is the only officially documented sharing toggle and it does not cover base permissions or shared materials.");
            helpToolTip.SetToolTip(immersiveExploreCheckBox, "Officially named EasyExplore, but when true it disables map markers and makes exploration harder.");
            helpToolTip.SetToolTip(mobHealthNumeric, "Enemy health multiplier. Official range: 0.2 to 5.0.");
            helpToolTip.SetToolTip(mobDamageNumeric, "Enemy damage multiplier. Official range: 0.2 to 5.0.");
            helpToolTip.SetToolTip(shipHealthNumeric, "Enemy ship health multiplier. Official range: 0.4 to 5.0.");
            helpToolTip.SetToolTip(shipDamageNumeric, "Enemy ship damage multiplier. Official range: 0.2 to 2.5.");
            helpToolTip.SetToolTip(boardingNumeric, "Enemy sailors needed for boarding success. Official range: 0.2 to 5.0.");
            helpToolTip.SetToolTip(coopStatsNumeric, "Scales enemy health and posture by player count. Official range: 0.0 to 2.0.");
            helpToolTip.SetToolTip(coopShipStatsNumeric, "Scales enemy ship health by player count. Official range: 0.0 to 2.0.");
            helpToolTip.SetToolTip(setActiveWorldButton, "Make the selected world the one that launches next time the server starts.");
            helpToolTip.SetToolTip(logTextBox, "Live output captured from the server process launched by this app.");
            helpToolTip.SetToolTip(availableModsListView, "Available mods found from the selected provider search.");
            helpToolTip.SetToolTip(installedModsListView, "Mods currently detected under ~mods.");
            helpToolTip.SetToolTip(saveServerTabButton, "Save server settings from the Captain tab.");
            helpToolTip.SetToolTip(saveWorldTabButton, "Save world settings from the World tab.");
        }

        private void BrowseForFolder()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select the Windrose dedicated server root folder";
                dialog.SelectedPath = Directory.Exists(pathTextBox.Text) ? pathTextBox.Text : string.Empty;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    pathTextBox.Text = dialog.SelectedPath;
                }
            }
        }

        private void BrowseForSteamCmd()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select the folder that contains steamcmd.exe";
                dialog.SelectedPath = Directory.Exists(steamCmdPathTextBox.Text) ? steamCmdPathTextBox.Text : DefaultSteamCmdDir;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    steamCmdPathTextBox.Text = dialog.SelectedPath;
                }
            }
        }

        private void BrowseForInstallDirectory()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select the folder where Windrose Dedicated Server should be installed or updated";
                dialog.SelectedPath = Directory.Exists(installDirTextBox.Text) ? installDirTextBox.Text : string.Empty;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    installDirTextBox.Text = dialog.SelectedPath;
                }
            }
        }

        private void BrowseForBackupFolder()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select a folder for Windrose backups";
                dialog.SelectedPath = Directory.Exists(backupFolderTextBox.Text) ? backupFolderTextBox.Text : string.Empty;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    backupFolderTextBox.Text = dialog.SelectedPath;
                    SavePreferences();
                }
            }
        }

        private void BackupFullServer()
        {
            var rootPath = currentState != null && !string.IsNullOrWhiteSpace(currentState.ServerRoot)
                ? currentState.ServerRoot
                : installDirTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(rootPath) || !Directory.Exists(rootPath))
            {
                SetStatus("Load a valid server root before creating a full backup.", true);
                return;
            }

            try
            {
                var backupResult = CreateFullServerBackup(rootPath);
                AppendLog("Created full server backup: " + backupResult.TargetFolder);
                if (backupResult.SkippedFileCount > 0)
                {
                    SetStatus("Full server backup created with " + backupResult.SkippedFileCount + " skipped live files. Check Live Log.", false);
                }
                else
                {
                    SetStatus("Full server backup created.", false);
                }
            }
            catch (Exception ex)
            {
                SetStatus("Backup failed: " + ex.Message, true);
            }
        }

        private void BackupCaptainSettings()
        {
            var rootPath = currentState != null && !string.IsNullOrWhiteSpace(currentState.ServerRoot)
                ? currentState.ServerRoot
                : pathTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(rootPath) || !Directory.Exists(rootPath))
            {
                SetStatus("Load a valid server root before backing up Captain settings.", true);
                return;
            }

            try
            {
                var sourceFile = GetServerDescriptionPath(rootPath);
                var backupFolder = EnsureBackupFolder();
                var backupFile = Path.Combine(backupFolder, "captain-settings-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".json");
                File.Copy(sourceFile, backupFile, true);
                AppendLog("Backed up Captain settings to " + backupFile);
                SetStatus("Captain settings backup created.", false);
            }
            catch (Exception ex)
            {
                SetStatus("Captain settings backup failed: " + ex.Message, true);
            }
        }

        private void BackupWorldSettings()
        {
            if (currentState == null)
            {
                SetStatus("Load a server first.", true);
                return;
            }

            var world = GetSelectedWorld();
            if (world == null || string.IsNullOrWhiteSpace(world.FilePath) || !File.Exists(world.FilePath))
            {
                SetStatus("Select a valid world before backing up world settings.", true);
                return;
            }

            try
            {
                var backupFolder = EnsureBackupFolder();
                var backupFile = Path.Combine(backupFolder, "world-settings-" + world.FolderName + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".json");
                File.Copy(world.FilePath, backupFile, true);
                AppendLog("Backed up world settings to " + backupFile);
                SetStatus("World settings backup created.", false);
            }
            catch (Exception ex)
            {
                SetStatus("World settings backup failed: " + ex.Message, true);
            }
        }

        private void RestoreFullServer()
        {
            var rootPath = currentState != null && !string.IsNullOrWhiteSpace(currentState.ServerRoot)
                ? currentState.ServerRoot
                : installDirTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(rootPath) || !Directory.Exists(rootPath))
            {
                SetStatus("Load a valid server root before restoring a full backup.", true);
                return;
            }

            if (ResolveRunningServerProcess() != null)
            {
                SetStatus("Stop the server before restoring a full backup.", true);
                return;
            }

            using (var dialog = new FolderBrowserDialog())
            {
                var backupFolder = backupFolderTextBox.Text.Trim();
                dialog.Description = "Select a full server backup folder (named server-full-...)";
                dialog.SelectedPath = Directory.Exists(backupFolder) ? backupFolder : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                if (dialog.ShowDialog(this) != DialogResult.OK || string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    return;
                }

                var selectedBackup = dialog.SelectedPath;
                if (!Directory.Exists(selectedBackup))
                {
                    SetStatus("Selected backup folder does not exist.", true);
                    return;
                }

                var confirm = MessageBox.Show(
                    this,
                    "This will copy all files from the selected backup folder over the current server install at:\n" + rootPath +
                    "\n\nExisting files will be overwritten. Files in the server that are not in the backup will not be removed." +
                    "\n\nAre you sure you want to continue?",
                    AppTitle,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);

                if (confirm != DialogResult.Yes)
                {
                    return;
                }

                try
                {
                    CopyDirectory(selectedBackup, rootPath);
                    AppendLog("Restored full server from backup: " + selectedBackup);

                    if (currentState != null && Directory.Exists(currentState.ServerRoot))
                    {
                        LoadServer(currentState.ServerRoot);
                        BindStateToUi();
                    }

                    SetStatus("Full server restored from backup.", false);
                }
                catch (Exception ex)
                {
                    SetStatus("Restore failed: " + ex.Message, true);
                }
            }
        }

        private void RestoreCaptainSettings()
        {
            var rootPath = currentState != null && !string.IsNullOrWhiteSpace(currentState.ServerRoot)
                ? currentState.ServerRoot
                : pathTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(rootPath) || !Directory.Exists(rootPath))
            {
                SetStatus("Load a valid server root before restoring Captain settings.", true);
                return;
            }

            try
            {
                RestoreSettingsFile(
                    GetServerDescriptionPath(rootPath),
                    "captain-settings-*.json|captain-settings-*.json|JSON files (*.json)|*.json|All files (*.*)|*.*",
                    "Select a Captain settings backup",
                    "Captain settings restored.",
                    "Restored Captain settings from ");
            }
            catch (Exception ex)
            {
                SetStatus("Captain settings restore failed: " + ex.Message, true);
            }
        }

        private void RestoreWorldSettings()
        {
            if (currentState == null)
            {
                SetStatus("Load a server first.", true);
                return;
            }

            var world = GetSelectedWorld();
            if (world == null || string.IsNullOrWhiteSpace(world.FilePath) || !File.Exists(world.FilePath))
            {
                SetStatus("Select a valid world before restoring world settings.", true);
                return;
            }

            try
            {
                RestoreSettingsFile(
                    world.FilePath,
                    "world-settings-*.json|world-settings-*.json|JSON files (*.json)|*.json|All files (*.*)|*.*",
                    "Select a world settings backup",
                    "World settings restored.",
                    "Restored world settings from ");
            }
            catch (Exception ex)
            {
                SetStatus("World settings restore failed: " + ex.Message, true);
            }
        }

        private void ApplyScheduledBackupSettings()
        {
            if (scheduledBackupEnabledCheckBox.Checked)
            {
                var intervalHours = Decimal.ToInt32(scheduledBackupIntervalNumeric.Value);
                if (nextScheduledBackupUtc == null)
                {
                    nextScheduledBackupUtc = DateTime.UtcNow.AddHours(intervalHours);
                }
                scheduledBackupTimer.Start();
            }
            else
            {
                scheduledBackupTimer.Stop();
                nextScheduledBackupUtc = null;
            }

            UpdateScheduledBackupNextLabel();
            SavePreferences();
        }

        private void HandleScheduledBackupTick()
        {
            if (!scheduledBackupEnabledCheckBox.Checked || nextScheduledBackupUtc == null)
            {
                return;
            }

            if (DateTime.UtcNow < nextScheduledBackupUtc.Value)
            {
                return;
            }

            if (currentState == null || string.IsNullOrWhiteSpace(currentState.ServerRoot) || !Directory.Exists(currentState.ServerRoot))
            {
                return;
            }

            try
            {
                var backupType = scheduledBackupTypeComboBox.SelectedItem != null
                    ? scheduledBackupTypeComboBox.SelectedItem.ToString()
                    : "Captain + World Settings";
                var backupSummary = "Scheduled backup completed.";

                if (string.Equals(backupType, "Full Server", StringComparison.OrdinalIgnoreCase))
                {
                    var backupResult = CreateFullServerBackup(currentState.ServerRoot);
                    backupSummary = backupResult.SkippedFileCount > 0
                        ? "Scheduled full backup completed with " + backupResult.SkippedFileCount + " skipped live files."
                        : "Scheduled full backup completed.";
                }
                else
                {
                    BackupCaptainSettings();
                    if (GetSelectedWorld() != null)
                    {
                        BackupWorldSettings();
                    }
                }

                var intervalHours = Decimal.ToInt32(scheduledBackupIntervalNumeric.Value);
                nextScheduledBackupUtc = DateTime.UtcNow.AddHours(intervalHours);
                SavePreferences();
                UpdateScheduledBackupNextLabel();
                AppendLog(backupSummary);
            }
            catch (Exception ex)
            {
                AppendLog("Scheduled backup failed: " + ex.Message);
                var intervalHours = Decimal.ToInt32(scheduledBackupIntervalNumeric.Value);
                nextScheduledBackupUtc = DateTime.UtcNow.AddHours(intervalHours);
                SavePreferences();
                UpdateScheduledBackupNextLabel();
            }
        }

        private void UpdateScheduledBackupNextLabel()
        {
            if (!scheduledBackupEnabledCheckBox.Checked || nextScheduledBackupUtc == null)
            {
                scheduledBackupNextLabel.Text = "Scheduled backups are disabled.";
                return;
            }

            var localNext = nextScheduledBackupUtc.Value.ToLocalTime();
            scheduledBackupNextLabel.Text = "Next backup: " + localNext.ToString("ddd MMM d yyyy h:mm tt");
        }

        private void ApplyScheduledRebootSettings()
        {
            if (!scheduledRebootEnabledCheckBox.Checked)
            {
                scheduledRebootTimer.Stop();
                nextScheduledRebootUtc = null;
                UpdateScheduledRebootNextLabel();
                SavePreferences();
                return;
            }

            var scheduledLocal = scheduledRebootDatePicker.Value.Date + scheduledRebootTimePicker.Value.TimeOfDay;

            if (scheduledLocal <= DateTime.Now)
            {
                if (recurringRebootCheckBox.Checked)
                {
                    var step = GetRebootIntervalStep();
                    while (scheduledLocal <= DateTime.Now)
                    {
                        scheduledLocal = scheduledLocal.Add(step);
                    }
                }
                else
                {
                    scheduledLocal = DateTime.Now.AddMinutes(1);
                }
            }

            nextScheduledRebootUtc = scheduledLocal.ToUniversalTime();
            scheduledRebootTimer.Start();
            UpdateScheduledRebootNextLabel();
            SavePreferences();
        }

        private void HandleScheduledRebootTick()
        {
            if (!scheduledRebootEnabledCheckBox.Checked || nextScheduledRebootUtc == null)
            {
                return;
            }

            if (DateTime.UtcNow < nextScheduledRebootUtc.Value)
            {
                return;
            }

            if (currentLaunchTarget == null)
            {
                AppendLog("Scheduled reboot skipped: no launch target available.");
                return;
            }

            var runningProcess = ResolveRunningServerProcess();
            if (runningProcess == null)
            {
                AppendLog("Scheduled reboot skipped: server is not running.");
            }
            else
            {
                AppendLog("Scheduled reboot starting.");
                RestartServer();
            }

            if (recurringRebootCheckBox.Checked)
            {
                var next = nextScheduledRebootUtc.Value.ToLocalTime().Add(GetRebootIntervalStep());
                while (next <= DateTime.Now)
                {
                    next = next.Add(GetRebootIntervalStep());
                }
                nextScheduledRebootUtc = next.ToUniversalTime();
            }
            else
            {
                scheduledRebootEnabledCheckBox.Checked = false;
                nextScheduledRebootUtc = null;
                scheduledRebootTimer.Stop();
            }

            UpdateScheduledRebootNextLabel();
            SavePreferences();
        }

        private void TriggerManualReboot()
        {
            if (currentLaunchTarget == null)
            {
                SetStatus("No launch target was found for reboot.", true);
                return;
            }

            if (ResolveRunningServerProcess() == null)
            {
                SetStatus("Server is not running. Start it first, then use Reboot Now.", true);
                return;
            }

            var confirm = MessageBox.Show(
                this,
                "Restart the running server now?",
                AppTitle,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (confirm != DialogResult.Yes)
            {
                return;
            }

            RestartServer();
            SetStatus("Reboot requested.", false);
        }

        private TimeSpan GetRebootIntervalStep()
        {
            var amount = Decimal.ToInt32(recurringRebootIntervalNumeric.Value);
            var unit = recurringRebootIntervalUnitComboBox.SelectedItem != null
                ? recurringRebootIntervalUnitComboBox.SelectedItem.ToString()
                : "Days";
            if (string.Equals(unit, "Hours", StringComparison.OrdinalIgnoreCase))
            {
                return TimeSpan.FromHours(amount);
            }

            return TimeSpan.FromDays(amount);
        }

        private void UpdateScheduledRebootNextLabel()
        {
            if (!scheduledRebootEnabledCheckBox.Checked || nextScheduledRebootUtc == null)
            {
                scheduledRebootNextLabel.Text = "Scheduled reboots are disabled.";
                return;
            }

            var localNext = nextScheduledRebootUtc.Value.ToLocalTime();
            scheduledRebootNextLabel.Text = "Next reboot: " + localNext.ToString("ddd MMM d yyyy h:mm tt");
        }

        private void CreateNewWorld()
        {
            if (currentState == null)
            {
                SetStatus("Load a server before creating a new world.", true);
                return;
            }

            var sourceWorld = GetSelectedWorld();
            if (sourceWorld == null || string.IsNullOrWhiteSpace(sourceWorld.FilePath) || !File.Exists(sourceWorld.FilePath))
            {
                try
                {
                    var preferredWorldId = currentState.Server != null ? currentState.Server.WorldIslandId : string.Empty;
                    var targetWorldFile = WindroseRepository.CreateInitialWorld(currentState.ServerRoot, "The Archipelago", preferredWorldId);
                    AppendLog("Created initial world at " + targetWorldFile);
                    LoadServer(currentState.ServerRoot);
                    TryAutoAssignSingleGeneratedWorldAsActive();
                    SetStatus("Created a new initial world. It has been loaded and set active when possible.", false);
                }
                catch (Exception ex)
                {
                    SetStatus("Create world failed: " + ex.Message, true);
                }
                return;
            }

            try
            {
                var serializer = new JavaScriptSerializer();
                var sourceDocument = serializer.Deserialize<WorldDescriptionDocument>(File.ReadAllText(sourceWorld.FilePath));
                if (sourceDocument == null || sourceDocument.WorldDescription == null)
                {
                    throw new InvalidOperationException("WorldDescription.json could not be parsed for the selected world.");
                }

                var newWorldId = Guid.NewGuid().ToString("N").ToUpperInvariant();
                var sourceWorldDir = Path.GetDirectoryName(sourceWorld.FilePath);
                if (string.IsNullOrWhiteSpace(sourceWorldDir))
                {
                    throw new InvalidOperationException("Could not resolve source world directory.");
                }

                var worldsRoot = Path.GetDirectoryName(sourceWorldDir);
                if (string.IsNullOrWhiteSpace(worldsRoot))
                {
                    throw new InvalidOperationException("Could not resolve worlds root directory.");
                }

                var newWorldDir = Path.Combine(worldsRoot, newWorldId);
                Directory.CreateDirectory(newWorldDir);

                sourceDocument.WorldDescription.islandId = newWorldId;
                sourceDocument.WorldDescription.WorldName = sourceWorld.WorldName + " (New)";
                sourceDocument.WorldDescription.CreationTime = DateTime.UtcNow.Ticks;

                var targetWorldFile = Path.Combine(newWorldDir, "WorldDescription.json");
                File.WriteAllText(targetWorldFile, serializer.Serialize(sourceDocument));

                AppendLog("Created new world: " + newWorldId + " at " + targetWorldFile);
                LoadServer(currentState.ServerRoot);
                SetStatus("New world created. Select it and click 'Set Selected World Active' when ready.", false);
            }
            catch (Exception ex)
            {
                SetStatus("Create world failed: " + ex.Message, true);
            }
        }

        private void DeleteSelectedWorld()
        {
            if (currentState == null)
            {
                SetStatus("Load a server before deleting a world.", true);
                return;
            }

            if (ResolveRunningServerProcess() != null)
            {
                SetStatus("Stop the server before deleting a world.", true);
                return;
            }

            var world = GetSelectedWorld();
            if (world == null || string.IsNullOrWhiteSpace(world.FilePath) || !File.Exists(world.FilePath))
            {
                SetStatus("Select a valid world to delete.", true);
                return;
            }

            var result = MessageBox.Show(
                this,
                "Move world '" + world.WorldName + "' (" + world.FolderName + ") to trash?",
                AppTitle,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes)
            {
                return;
            }

            try
            {
                var worldDir = Path.GetDirectoryName(world.FilePath);
                if (string.IsNullOrWhiteSpace(worldDir) || !Directory.Exists(worldDir))
                {
                    throw new DirectoryNotFoundException("Could not find world folder for the selected world.");
                }

                var wasActive = string.Equals(currentState.Server.WorldIslandId, world.FolderName, StringComparison.OrdinalIgnoreCase);
                var trashedWorldDir = MoveDirectoryToTrash(worldDir);
                AppendLog("Moved world folder to trash: " + trashedWorldDir);

                currentState.Worlds.RemoveAll(delegate(WorldEditableState item) { return item.Key == world.Key; });
                currentState.SelectedWorldKey = currentState.Worlds.Count > 0 ? currentState.Worlds[0].Key : null;

                if (wasActive)
                {
                    currentState.Server.WorldIslandId = string.Empty;
                    activeWorldIdTextBox.Text = string.Empty;
                    MarkDirty();
                }

                RefreshWorldList();
                SelectWorldInListView();
                BindSelectedWorldToUi();
                RefreshWarnings();
                SetStatus("World moved to trash.", false);
                UpdateProcessUi();
            }
            catch (Exception ex)
            {
                SetStatus("Delete world failed: " + ex.Message, true);
            }
        }

        private void ImportWorld()
        {
            if (currentState == null)
            {
                SetStatus("Load a server before importing a world.", true);
                return;
            }

            if (ResolveRunningServerProcess() != null)
            {
                SetStatus("Stop the server before importing a world.", true);
                return;
            }

            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select a world folder, a Worlds folder, a RocksDB folder, or another Windrose server root";
                if (dialog.ShowDialog(this) != DialogResult.OK || string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    return;
                }

                try
                {
                    var sourceWorldDirs = ResolveImportWorldDirectories(dialog.SelectedPath);
                    if (sourceWorldDirs.Count == 0)
                    {
                        throw new InvalidOperationException(
                            "No importable Windrose world folders were found under the selected path.\n\n" +
                            "Expected one of these:\n" +
                            "- a world folder containing WorldDescription.json\n" +
                            "- a Worlds folder containing world subfolders\n" +
                            "- a RocksDB folder\n" +
                            "- a Windrose server root");
                    }

                    var validationFailures = new List<string>();
                    foreach (var sourceWorldDir in sourceWorldDirs)
                    {
                        var issues = WindroseRepository.ValidateImportWorldSource(sourceWorldDir);
                        if (issues.Count > 0)
                        {
                            validationFailures.Add(
                                Path.GetFileName(sourceWorldDir) + ":\n- " +
                                string.Join("\n- ", issues.ToArray()));
                        }
                    }

                    if (validationFailures.Count > 0)
                    {
                        var message = "One or more selected worlds cannot be imported because required files or settings are missing or invalid.\n\n"
                            + string.Join("\n\n", validationFailures.ToArray());
                        MessageBox.Show(this, message, AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        SetStatus("Import world validation failed. Review the details and fix the source world first.", true);
                        return;
                    }

                    if (sourceWorldDirs.Count > 1)
                    {
                        var importMany = MessageBox.Show(
                            this,
                            "Found " + sourceWorldDirs.Count + " world folders under the selected path.\n\nImport all of them into the current server?",
                            AppTitle,
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2);

                        if (importMany != DialogResult.Yes)
                        {
                            return;
                        }
                    }

                    var importedCount = 0;
                    foreach (var sourceWorldDir in sourceWorldDirs)
                    {
                        var targetPath = WindroseRepository.ImportWorldIntoServer(currentState.ServerRoot, sourceWorldDir);
                        AppendLog("Imported world from " + sourceWorldDir + " to " + targetPath);
                        importedCount++;
                    }

                    var previousActiveWorldId = currentState.Server.WorldIslandId;
                    LoadServer(currentState.ServerRoot);
                    currentState.Server.WorldIslandId = previousActiveWorldId;
                    BindStateToUi();
                    SetStatus("Imported " + importedCount + " world" + (importedCount == 1 ? string.Empty : "s") + ". Select one and click 'Set Selected World Active' when you want the server to boot into it.", false);
                }
                catch (Exception ex)
                {
                    SetStatus("Import world failed: " + ex.Message, true);
                }
            }
        }

        private static List<string> ResolveImportWorldDirectories(string selectedPath)
        {
            var normalized = Path.GetFullPath(selectedPath);
            var worlds = new List<string>();

            if (File.Exists(Path.Combine(normalized, "WorldDescription.json")))
            {
                worlds.Add(normalized);
            }

            if (Directory.Exists(normalized))
            {
                foreach (var childDir in Directory.GetDirectories(normalized))
                {
                    if (File.Exists(Path.Combine(childDir, "WorldDescription.json")))
                    {
                        worlds.Add(childDir);
                    }
                }
            }

            var rocksDbRoots = new[]
            {
                Path.Combine(normalized, "R5", "Saved", "SaveProfiles", "Default", "RocksDB"),
                Path.Combine(normalized, "RocksDB")
            };

            foreach (var rocksDbRoot in rocksDbRoots)
            {
                if (!Directory.Exists(rocksDbRoot))
                {
                    continue;
                }

                foreach (var versionDir in Directory.GetDirectories(rocksDbRoot))
                {
                    var worldsRoot = Path.Combine(versionDir, "Worlds");
                    if (!Directory.Exists(worldsRoot))
                    {
                        continue;
                    }

                    foreach (var worldDir in Directory.GetDirectories(worldsRoot))
                    {
                        if (File.Exists(Path.Combine(worldDir, "WorldDescription.json")))
                        {
                            worlds.Add(worldDir);
                        }
                    }
                }
            }

            return worlds
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(delegate(string item) { return item; }, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private string EnsureBackupFolder()
        {
            var backupFolder = backupFolderTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(backupFolder))
            {
                backupFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WindroseBackups");
                backupFolderTextBox.Text = backupFolder;
            }

            Directory.CreateDirectory(backupFolder);
            SavePreferences();
            return backupFolder;
        }

        private void RestoreSettingsFile(string targetFilePath, string filter, string dialogTitle, string successStatus, string successLogPrefix)
        {
            if (ResolveRunningServerProcess() != null)
            {
                SetStatus("Stop the server before restoring settings.", true);
                return;
            }

            var backupFolder = EnsureBackupFolder();
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = dialogTitle;
                dialog.Filter = filter;
                dialog.InitialDirectory = Directory.Exists(backupFolder) ? backupFolder : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                dialog.CheckFileExists = true;
                dialog.CheckPathExists = true;

                if (dialog.ShowDialog(this) != DialogResult.OK || string.IsNullOrWhiteSpace(dialog.FileName))
                {
                    return;
                }

                var confirm = MessageBox.Show(
                    this,
                    "This will overwrite the live settings file with the selected backup.\n\nA safety .bak copy of the current file will be created first. Continue?",
                    AppTitle,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);

                if (confirm != DialogResult.Yes)
                {
                    return;
                }

                WindroseRepository.BackupAndReplaceFile(targetFilePath, dialog.FileName);
                AppendLog(successLogPrefix + dialog.FileName);

                if (currentState != null && Directory.Exists(currentState.ServerRoot))
                {
                    LoadServer(currentState.ServerRoot);
                    BindStateToUi();
                }

                SetStatus(successStatus, false);
            }
        }

        private static string GetServerDescriptionPath(string serverRoot)
        {
            var candidates = new[]
            {
                Path.Combine(serverRoot, "R5", "ServerDescription.json"),
                Path.Combine(serverRoot, "ServerDescription.json")
            };

            foreach (var candidate in candidates)
            {
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            throw new FileNotFoundException("Could not find ServerDescription.json under " + serverRoot + ".");
        }

        private void InstallSteamCmd()
        {
            try
            {
                Directory.CreateDirectory(DefaultRootDir);
                var steamCmdDir = DefaultSteamCmdDir;
                Directory.CreateDirectory(steamCmdDir);

                var zipPath = Path.Combine(steamCmdDir, "steamcmd.zip");
                AppendLog("Downloading SteamCMD from the official Valve CDN...");
                SetStatus("Downloading SteamCMD...", false);

                DownloadFileToPath(SteamCmdDownloadUrl, zipPath, null);

                AppendLog("Extracting SteamCMD to " + steamCmdDir);
                SetStatus("Extracting SteamCMD...", false);

                foreach (var existing in Directory.GetFileSystemEntries(steamCmdDir))
                {
                    var name = Path.GetFileName(existing);
                    if (string.Equals(name, "steamcmd.zip", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    MovePathToTrash(existing);
                }

                ZipFile.ExtractToDirectory(zipPath, steamCmdDir);
                File.Delete(zipPath);
                steamCmdPathTextBox.Text = DefaultSteamCmdDir;
                AppendLog("SteamCMD installed to " + DefaultSteamCmdExe);
                SetStatus("SteamCMD installed successfully.", false);
            }
            catch (Exception ex)
            {
                AppendLog("SteamCMD install failed: " + ex.Message);
                SetStatus(ex.Message, true);
            }
        }

        private void RunSteamCmd(bool updateOnly)
        {
            var steamCmdDir = steamCmdPathTextBox.Text.Trim();
            var installDir = installDirTextBox.Text.Trim();
            var steamCmdPath = Path.Combine(steamCmdDir, "steamcmd.exe");

            if (string.IsNullOrEmpty(steamCmdDir) || !File.Exists(steamCmdPath))
            {
                SetStatus("Select a valid SteamCMD folder first, or click Install SteamCMD.", true);
                return;
            }

            if (string.IsNullOrEmpty(installDir))
            {
                SetStatus("Select an install directory first.", true);
                return;
            }

            if (!updateOnly)
            {
                if (!PromptForFreshInstallCleanup(installDir))
                {
                    return;
                }
            }

            if (managedTaskProcess != null && !managedTaskProcess.HasExited)
            {
                SetStatus("Another SteamCMD task is already running.", true);
                return;
            }

            Directory.CreateDirectory(installDir);
            SetStatus("Preparing SteamCMD before " + (updateOnly ? "updating" : "installing") + " Windrose...", false);
            StartTaskProcess(steamCmdPath, "+quit", steamCmdDir, "bootstrap", false, delegate(int bootstrapExitCode, string bootstrapOutput)
            {
                if (bootstrapExitCode == 7 && DidSteamCmdSelfUpdate(bootstrapOutput))
                {
                    AppendLog("SteamCMD updated itself during bootstrap. Running bootstrap one more time before Windrose " + (updateOnly ? "update" : "install") + "...");
                    SetStatus("SteamCMD updated itself. Finalizing bootstrap...", false);
                    StartTaskProcess(steamCmdPath, "+quit", steamCmdDir, "bootstrap retry", false, delegate(int retryExitCode, string retryOutput)
                    {
                        if (retryExitCode != 0)
                        {
                            SetStatus("SteamCMD bootstrap retry failed with exit code " + retryExitCode + ". Check Live Log.", true);
                            return;
                        }

                        StartSteamCmdInstallOrUpdate(steamCmdPath, steamCmdDir, installDir, updateOnly);
                    });
                    return;
                }

                if (bootstrapExitCode != 0)
                {
                    SetStatus("SteamCMD bootstrap failed with exit code " + bootstrapExitCode + ". Check Live Log.", true);
                    return;
                }

                StartSteamCmdInstallOrUpdate(steamCmdPath, steamCmdDir, installDir, updateOnly);
            });
        }

        private void StartSteamCmdInstallOrUpdate(string steamCmdPath, string steamCmdDir, string installDir, bool updateOnly)
        {
            AppendLog("SteamCMD bootstrap finished cleanly. Starting Windrose " + (updateOnly ? "update" : "install") + "...");
            var arguments = "+force_install_dir \"" + installDir + "\" +login anonymous +app_update 4129620 validate +quit";
            StartTaskProcess(steamCmdPath, arguments, steamCmdDir, updateOnly ? "update" : "install", true, delegate(int exitCode, string taskOutput)
            {
                var installSuccess = exitCode == 0 || DidSteamCmdInstallWindrose(taskOutput) || IsInstalledServerDetected(installDir);
                if (!installSuccess)
                {
                    if (DidSteamCmdReportMissingConfiguration(taskOutput))
                    {
                        SetStatus("SteamCMD " + (updateOnly ? "update" : "install") + " failed with exit code " + exitCode + " after automatic retries. Try again, or use the Setup Guide's Steam account login fallback.", true);
                    }
                    else
                    {
                        SetStatus("SteamCMD " + (updateOnly ? "update" : "install") + " failed with exit code " + exitCode + ". Check Live Log.", true);
                    }

                    return;
                }

                BeginAutomaticProvisioningAfterInstall(installDir, updateOnly);
            });
        }

        private void OpenInstallDirectory()
        {
            var installDir = installDirTextBox.Text.Trim();
            if (string.IsNullOrEmpty(installDir))
            {
                SetStatus("Select an install directory first.", true);
                return;
            }

            try
            {
                Directory.CreateDirectory(installDir);
                OpenPathInShell(installDir);
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
        }

        private bool PromptForFreshInstallCleanup(string installDir)
        {
            var hasOldSaveData = Directory.Exists(Path.Combine(installDir, "R5", "Saved", "SaveProfiles"));
            var hasServerDescription = File.Exists(Path.Combine(installDir, "R5", "ServerDescription.json"));
            if (!hasOldSaveData && !hasServerDescription)
            {
                return true;
            }

            var result = MessageBox.Show(
                this,
                "Existing server data was found in the install folder.\n\nFor a cleaner first boot, move old save/config data to backup before installing?",
                AppTitle,
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);

            if (result == DialogResult.Cancel)
            {
                return false;
            }

            if (result != DialogResult.Yes)
            {
                return true;
            }

            var backupPath = WindroseRepository.PrepareFreshInstallData(installDir);
            AppendLog("Moved existing server save/config data to " + backupPath + " before install.");
            return true;
        }

        private void RepairDataInconsistency()
        {
            if (managedTaskProcess != null && !managedTaskProcess.HasExited)
            {
                SetStatus("Stop install/update tasks before running repair.", true);
                return;
            }

            if (managedServerProcess != null && !managedServerProcess.HasExited)
            {
                SetStatus("Stop the server process before running repair.", true);
                return;
            }

            var rootPath = currentState != null && !string.IsNullOrWhiteSpace(currentState.ServerRoot)
                ? currentState.ServerRoot
                : installDirTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(rootPath))
            {
                SetStatus("Load a server root or set an install directory first.", true);
                return;
            }

            var result = MessageBox.Show(
                this,
                "Repair will move the entire SaveProfiles folder into a timestamped backup, recreate a clean Default profile, and clear active world ID. Continue?",
                AppTitle,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
            {
                return;
            }

            try
            {
                var repairResult = WindroseRepository.RepairDataInconsistency(rootPath);
                AppendLog("Repair backup folder: " + repairResult.BackupPath);
                AppendLog("Repair moved folder count: " + repairResult.MovedFolderCount);
                if (repairResult.ServerDescriptionReset)
                {
                    AppendLog("Repair moved ServerDescription.json to backup. The server will regenerate a fresh config on next start.");
                }

                SetStatus("Repair completed. Start server once to regenerate clean config/data, then click Load Server.", false);
                pathTextBox.Text = rootPath;
                currentState = null;
                SetConfigEditorsEnabled(false);
            }
            catch (Exception ex)
            {
                AppendLog("Repair failed: " + ex.Message);
                SetStatus(ex.Message, true);
            }
        }

        private void DeleteServerInstall()
        {
            if (managedTaskProcess != null && !managedTaskProcess.HasExited)
            {
                SetStatus("Stop install/update tasks before deleting a server.", true);
                return;
            }

            if (managedServerProcess != null && !managedServerProcess.HasExited)
            {
                SetStatus("Stop the server process before deleting a server.", true);
                return;
            }

            var rootPath = !string.IsNullOrWhiteSpace(installDirTextBox.Text)
                ? installDirTextBox.Text.Trim()
                : (currentState == null ? string.Empty : currentState.ServerRoot);

            if (string.IsNullOrWhiteSpace(rootPath) || !Directory.Exists(rootPath))
            {
                SetStatus("Select a valid install folder to delete.", true);
                return;
            }

            var fullPath = Path.GetFullPath(rootPath);
            var rootOnly = Path.GetPathRoot(fullPath);
            if (string.Equals(fullPath, rootOnly, StringComparison.OrdinalIgnoreCase))
            {
                SetStatus("Refusing to delete a drive root folder.", true);
                return;
            }

            var result = MessageBox.Show(
                this,
                "Move this server install folder to trash?\n\n" + fullPath,
                AppTitle,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes)
            {
                return;
            }

            try
            {
                var trashedServerDir = MoveDirectoryToTrash(fullPath);
                AppendLog("Moved server install folder to trash: " + trashedServerDir);

                if (currentState != null && string.Equals(Path.GetFullPath(currentState.ServerRoot), fullPath, StringComparison.OrdinalIgnoreCase))
                {
                    currentState = null;
                    currentLaunchTarget = null;
                    isDirty = false;
                    SetConfigEditorsEnabled(false);
                    worldsListView.Items.Clear();
                    warningsListBox.Items.Clear();
                    UpdateWindowTitle();
                    UpdateLaunchTargetUi();
                }

                fileLogPath = null;
                fileLogPosition = 0;
                fileLogRoot = null;
                SetStatus("Server folder moved to trash.", false);
                UpdateProcessUi();
                PopulateModsInfo();
            }
            catch (Exception ex)
            {
                SetStatus("Delete failed: " + ex.Message, true);
            }
        }

        private void StartTaskProcess(string exePath, string arguments, string workingDirectory, string taskName, bool allowRetryAfterSelfUpdate, Action<int, string> completionAction, int automaticRetryCount = 0)
        {
            try
            {
                var outputBuffer = new StringBuilder();
                var outputLock = new object();
                var installDir = installDirTextBox.Text.Trim();

                var taskProcess = new Process();
                managedTaskProcess = taskProcess;
                taskProcess.StartInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                taskProcess.EnableRaisingEvents = true;
                taskProcess.Exited += delegate
                {
                    taskProcess.WaitForExit();
                    var exitCode = taskProcess.ExitCode;
                    string taskOutput;
                    lock (outputLock)
                    {
                        taskOutput = outputBuffer.ToString();
                    }

                    var installSuccess = taskOutput.IndexOf("Success! App '4129620' fully installed.", StringComparison.OrdinalIgnoreCase) >= 0
                        || IsInstalledServerDetected(installDir);
                    var selfUpdated = DidSteamCmdSelfUpdate(taskOutput);
                    var missingConfiguration = DidSteamCmdReportMissingConfiguration(taskOutput);

                    AppendLog("SteamCMD " + taskName + " finished with exit code " + exitCode + ".");

                    if (allowRetryAfterSelfUpdate && exitCode == 7 && selfUpdated && !installSuccess)
                    {
                        AppendLog("SteamCMD updated itself and exited before the task fully completed. Retrying once automatically.");
                        BeginInvoke((MethodInvoker)delegate
                        {
                            SetStatus("SteamCMD updated itself. Retrying " + taskName + " once...", false);
                            StartTaskProcess(exePath, arguments, workingDirectory, taskName, false, completionAction, automaticRetryCount);
                        });
                        return;
                    }

                    if (missingConfiguration && exitCode == 8 && !installSuccess && automaticRetryCount < SteamCmdMissingConfigurationRetryLimit)
                    {
                        var nextAttempt = automaticRetryCount + 1;
                        AppendLog("SteamCMD reported 'Missing configuration'. This is often transient, so retrying automatically (" + nextAttempt + "/" + SteamCmdMissingConfigurationRetryLimit + ").");
                        BeginInvoke((MethodInvoker)delegate
                        {
                            SetStatus("SteamCMD hit a temporary Steam error. Retrying " + taskName + " (" + nextAttempt + "/" + SteamCmdMissingConfigurationRetryLimit + ")...", false);
                            StartTaskProcess(exePath, arguments, workingDirectory, taskName, allowRetryAfterSelfUpdate, completionAction, nextAttempt);
                        });
                        return;
                    }

                    if (completionAction != null)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            completionAction(exitCode, taskOutput);
                            UpdateProcessUi();
                        });
                        return;
                    }

                    BeginInvoke((MethodInvoker)delegate
                    {
                        if (exitCode == 0 || installSuccess)
                        {
                            SetStatus(
                                exitCode == 0
                                    ? "SteamCMD " + taskName + " completed."
                                    : "SteamCMD " + taskName + " appears to have completed even though SteamCMD returned exit code " + exitCode + ".", false);
                        }
                        else
                        {
                            if (missingConfiguration)
                            {
                                SetStatus("SteamCMD " + taskName + " failed with exit code " + exitCode + " after automatic retries. Try again, or use the Setup Guide's Steam account login fallback.", true);
                            }
                            else
                            {
                                SetStatus("SteamCMD " + taskName + " failed with exit code " + exitCode + ". Check Live Log.", true);
                            }
                        }

                        UpdateProcessUi();
                    });
                };

                if (!taskProcess.Start())
                {
                    throw new InvalidOperationException("Could not start SteamCMD.");
                }

                taskProcess.OutputDataReceived += delegate(object sender, DataReceivedEventArgs args)
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        lock (outputLock)
                        {
                            outputBuffer.AppendLine(args.Data);
                        }
                        AppendLog("[steamcmd] " + args.Data);
                    }
                };
                taskProcess.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs args)
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        lock (outputLock)
                        {
                            outputBuffer.AppendLine(args.Data);
                        }
                        AppendLog("[steamcmd:stderr] " + args.Data);
                    }
                };
                taskProcess.BeginOutputReadLine();
                taskProcess.BeginErrorReadLine();
                AppendLog("SteamCMD " + taskName + " started for " + installDir);
                AppendLog("Command: \"" + exePath + "\" " + arguments);
                SetStatus("SteamCMD " + taskName + " running. This can take several minutes.", false);
                UpdateProcessUi();
            }
            catch (Exception ex)
            {
                AppendLog("SteamCMD failed to start: " + ex.Message);
                SetStatus(ex.Message, true);
            }
        }

        private static bool DidSteamCmdSelfUpdate(string taskOutput)
        {
            return taskOutput.IndexOf("Update complete, launching Steamcmd", StringComparison.OrdinalIgnoreCase) >= 0
                || taskOutput.IndexOf("Update complete, launching...", StringComparison.OrdinalIgnoreCase) >= 0
                || taskOutput.IndexOf("Redirecting stderr to", StringComparison.OrdinalIgnoreCase) >= 0
                || taskOutput.IndexOf("Checking for available updates", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool DidSteamCmdReportMissingConfiguration(string taskOutput)
        {
            return taskOutput.IndexOf("Missing configuration", StringComparison.OrdinalIgnoreCase) >= 0
                || taskOutput.IndexOf("Failed to install app '4129620'", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool DidSteamCmdInstallWindrose(string taskOutput)
        {
            return taskOutput.IndexOf("Success! App '4129620' fully installed.", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool IsInstalledServerDetected(string installDir)
        {
            if (string.IsNullOrWhiteSpace(installDir) || !Directory.Exists(installDir))
            {
                return false;
            }

            return File.Exists(Path.Combine(installDir, "R5", "ServerDescription.json"))
                || File.Exists(Path.Combine(installDir, "StartServerForeground.bat"))
                || File.Exists(Path.Combine(installDir, "StartServer.bat"))
                || File.Exists(Path.Combine(installDir, "WindroseServer.exe"))
                || File.Exists(Path.Combine(installDir, "R5", "Binaries", "Win64", "WindroseServer-Win64-Shipping.exe"));
        }

        private void BeginAutomaticProvisioningAfterInstall(string installDir, bool updateOnly)
        {
            var normalizedInstallDir = Path.GetFullPath(installDir);
            installDirTextBox.Text = normalizedInstallDir;
            pathTextBox.Text = normalizedInstallDir;
            SavePreferences();

            if (!NeedsAutomaticProvisioning(normalizedInstallDir))
            {
                LoadServer(normalizedInstallDir);
                SetStatus("Windrose " + (updateOnly ? "update" : "install") + " completed and the server was loaded.", false);
                return;
            }

            currentState = null;
            currentLaunchTarget = DetectLaunchTarget(normalizedInstallDir);
            UpdateLaunchTargetUi();
            UpdateProcessUi();

            if (currentLaunchTarget == null)
            {
                SetStatus("Windrose " + (updateOnly ? "update" : "install") + " completed, but no launch target was found for the automatic first boot. Check the install folder and try loading it manually.", true);
                return;
            }

            isAutomaticProvisioning = true;
            automaticProvisioningWaitingForShutdown = false;
            automaticProvisioningWasUpdate = updateOnly;
            automaticProvisioningRoot = normalizedInstallDir;
            automaticProvisioningStartedUtc = DateTime.UtcNow;

            AppendLog("Starting automatic first boot so Windrose can generate server and world config files.");
            StartServer();
            if (!hasActiveServerSession && !IsServerStillRunning())
            {
                isAutomaticProvisioning = false;
                automaticProvisioningRoot = string.Empty;
                automaticProvisioningStartedUtc = null;
                automaticProvisioningWasUpdate = false;
                return;
            }

            SetStatus("Windrose files are ready. Starting the server once so config files are generated automatically...", false);
            UpdateProcessUi();
        }

        private void HandleAutomaticProvisioningTick()
        {
            if (!isAutomaticProvisioning || string.IsNullOrWhiteSpace(automaticProvisioningRoot))
            {
                return;
            }

            var elapsed = automaticProvisioningStartedUtc.HasValue
                ? DateTime.UtcNow - automaticProvisioningStartedUtc.Value
                : TimeSpan.Zero;
            var hasServerDescription = HasGeneratedServerDescription(automaticProvisioningRoot);
            var generatedWorldIslandId = ReadGeneratedWorldIslandId(automaticProvisioningRoot);
            var hasWorldData = HasGeneratedWorldData(automaticProvisioningRoot);
            var hasGeneratedWorldIslandId = !string.IsNullOrWhiteSpace(generatedWorldIslandId);
            var serverRunning = IsServerStillRunning() || hasActiveServerSession;

            if (!automaticProvisioningWaitingForShutdown)
            {
                if (hasGeneratedWorldIslandId && elapsed >= AutomaticProvisioningStartupDelay)
                {
                    automaticProvisioningWaitingForShutdown = true;
                    AppendLog("Windrose generated ServerDescription.json and populated WorldIslandId (" + generatedWorldIslandId + "). Stopping the automatic first boot so the manager can load it.");
                    SetStatus("Initial Windrose world config detected. Stopping the server so Captain's Console can load it...", false);
                    if (IsServerStillRunning())
                    {
                        StopServer();
                    }
                    return;
                }

                if (!serverRunning && elapsed >= AutomaticProvisioningStartupDelay)
                {
                    automaticProvisioningWaitingForShutdown = true;
                    return;
                }

                if (elapsed >= AutomaticProvisioningTimeout)
                {
                    automaticProvisioningWaitingForShutdown = true;
                    AppendLog("Automatic first boot timed out while waiting for Windrose to generate config files.");
                    SetStatus("Initial Windrose boot is taking longer than expected. Stopping it and loading whatever was generated...", false);
                    if (IsServerStillRunning())
                    {
                        StopServer();
                    }
                }

                return;
            }

            if (serverRunning)
            {
                return;
            }

            if (!hasServerDescription)
            {
                FinishAutomaticProvisioning(false, "Windrose installed, but no ServerDescription.json was generated during the automatic first boot. Check Live Log, then try Start Server once.", true);
                return;
            }

            LoadServer(automaticProvisioningRoot);
            if (currentState == null)
            {
                FinishAutomaticProvisioning(false, "Windrose generated files, but Captain's Console could not load the server automatically. Check Live Log, then try Load Server once.", true);
                return;
            }

            if (string.IsNullOrWhiteSpace(currentState.Server.WorldIslandId))
            {
                if (TryAutoAssignSingleGeneratedWorldAsActive())
                {
                    FinishAutomaticProvisioning(true, "Windrose " + (automaticProvisioningWasUpdate ? "update" : "install") + " completed. Captain's Console auto-filled the generated WorldIslandId and loaded the server.", false);
                    return;
                }
            }

            if (!hasWorldData)
            {
                if (TryCreateInitialWorldAfterInstall())
                {
                    FinishAutomaticProvisioning(true, "Windrose " + (automaticProvisioningWasUpdate ? "update" : "install") + " completed. Captain's Console created an initial world and loaded the server automatically.", false);
                    return;
                }
            }

            if (string.IsNullOrWhiteSpace(currentState.Server.WorldIslandId) && !hasWorldData)
            {
                FinishAutomaticProvisioning(false, "Windrose installed, but no world data was generated during the automatic first boot. Check Live Log, then try Start Server once.", true);
                return;
            }

            if (string.IsNullOrWhiteSpace(currentState.Server.WorldIslandId))
            {
                FinishAutomaticProvisioning(false, "Windrose generated config files, but WorldIslandId is still empty. If multiple worlds were created, choose the correct one and click 'Set Selected World Active'.", true);
                return;
            }

            if (!hasWorldData)
            {
                FinishAutomaticProvisioning(true, "Windrose " + (automaticProvisioningWasUpdate ? "update" : "install") + " completed and the server config was loaded. World settings were not generated yet, so one more server start may still be needed for world controls.", false);
                return;
            }

            FinishAutomaticProvisioning(true, "Windrose " + (automaticProvisioningWasUpdate ? "update" : "install") + " completed. Captain's Console loaded the server automatically after the first boot.", false);
        }

        private void FinishAutomaticProvisioning(bool success, string statusMessage, bool isError)
        {
            isAutomaticProvisioning = false;
            automaticProvisioningWaitingForShutdown = false;
            automaticProvisioningWasUpdate = false;
            automaticProvisioningRoot = string.Empty;
            automaticProvisioningStartedUtc = null;
            SetStatus(statusMessage, isError && !success);
            UpdateProcessUi();
        }

        private static bool HasGeneratedServerDescription(string serverRoot)
        {
            return File.Exists(Path.Combine(serverRoot, "R5", "ServerDescription.json"))
                || File.Exists(Path.Combine(serverRoot, "ServerDescription.json"));
        }

        private static bool HasGeneratedWorldData(string serverRoot)
        {
            return ResolveImportWorldDirectories(serverRoot).Count > 0;
        }

        private static bool NeedsAutomaticProvisioning(string serverRoot)
        {
            return !HasGeneratedServerDescription(serverRoot) || string.IsNullOrWhiteSpace(ReadGeneratedWorldIslandId(serverRoot));
        }

        private static string ReadGeneratedWorldIslandId(string serverRoot)
        {
            try
            {
                var serverDescriptionPath = WindroseRepository.FindServerDescriptionPathOrNull(serverRoot);
                if (string.IsNullOrWhiteSpace(serverDescriptionPath) || !File.Exists(serverDescriptionPath))
                {
                    return string.Empty;
                }

                var serializer = new JavaScriptSerializer();
                var document = serializer.Deserialize<ServerDescriptionDocument>(File.ReadAllText(serverDescriptionPath));
                if (document == null || document.ServerDescription_Persistent == null || string.IsNullOrWhiteSpace(document.ServerDescription_Persistent.WorldIslandId))
                {
                    return string.Empty;
                }

                return document.ServerDescription_Persistent.WorldIslandId.Trim();
            }
            catch
            {
                return string.Empty;
            }
        }

        private bool TryAutoAssignSingleGeneratedWorldAsActive()
        {
            if (currentState == null || currentState.Server == null || currentState.Worlds == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(currentState.Server.WorldIslandId) || currentState.Worlds.Count != 1)
            {
                return false;
            }

            var onlyWorld = currentState.Worlds[0];
            if (onlyWorld == null || string.IsNullOrWhiteSpace(onlyWorld.FolderName))
            {
                return false;
            }

            currentState.Server.WorldIslandId = onlyWorld.FolderName.Trim();
            currentState = WindroseRepository.Save(currentState);
            currentLaunchTarget = DetectLaunchTarget(currentState.ServerRoot);
            isDirty = false;
            BindStateToUi();
            UpdateWindowTitle();
            AppendLog("Auto-filled blank WorldIslandId with generated world folder " + currentState.Server.WorldIslandId + ".");
            return true;
        }

        private bool TryCreateInitialWorldAfterInstall()
        {
            if (currentState == null || currentState.Server == null)
            {
                return false;
            }

            try
            {
                var targetWorldFile = WindroseRepository.CreateInitialWorld(
                    currentState.ServerRoot,
                    "The Archipelago",
                    currentState.Server.WorldIslandId);
                AppendLog("Created initial world after install at " + targetWorldFile);
                LoadServer(currentState.ServerRoot);
                TryAutoAssignSingleGeneratedWorldAsActive();
                return currentState != null && currentState.Worlds != null && currentState.Worlds.Count > 0;
            }
            catch (Exception ex)
            {
                AppendLog("Automatic initial world creation failed: " + ex.Message);
                return false;
            }
        }

        private void OpenUrl(string url)
        {
            try
            {
                OpenPathInShell(url);
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
        }

        private string GetModsRootPath()
        {
            var basePath = currentState != null && !string.IsNullOrEmpty(currentState.ServerRoot)
                ? currentState.ServerRoot
                : installDirTextBox.Text.Trim();

            if (string.IsNullOrEmpty(basePath))
            {
                return string.Empty;
            }

            return Path.Combine(basePath, "R5", "Content", "Paks", "~mods");
        }

        private void OpenModsFolder()
        {
            var modsRoot = GetModsRootPath();
            if (string.IsNullOrEmpty(modsRoot))
            {
                SetStatus("Load a server root or set an install directory first.", true);
                return;
            }

            Directory.CreateDirectory(modsRoot);
            OpenPathInShell(modsRoot);
        }

        private void InstallRconFiles()
        {
            var win64Dir = GetRconWin64Directory();
            if (string.IsNullOrWhiteSpace(win64Dir))
            {
                SetStatus("Load a server first so Captain's Console knows where to install RCON.", true);
                return;
            }

            var bundledDllBytes = LoadEmbeddedBinaryResource(EmbeddedWindroseRconVersionDllResourceName);
            var sourceDll = string.Empty;
            if (bundledDllBytes == null || bundledDllBytes.Length == 0)
            {
                sourceDll = FindLocalRconVersionDllSource();
            }

            if ((bundledDllBytes == null || bundledDllBytes.Length == 0) && (string.IsNullOrWhiteSpace(sourceDll) || !File.Exists(sourceDll)))
            {
                SetStatus("No embedded or local WindroseRCON version.dll was found. Keep version.dll in the project root before building, or place it under WindroseRCON-main\\build\\windows\\x64\\release.", true);
                return;
            }

            try
            {
                Directory.CreateDirectory(win64Dir);
                var destinationDll = GetRconVersionDllPath();
                if (bundledDllBytes != null && bundledDllBytes.Length > 0)
                {
                    File.WriteAllBytes(destinationDll, bundledDllBytes);
                }
                else
                {
                    File.Copy(sourceDll, destinationDll, true);
                }
                SaveRconSettings();
                AppendLog("Installed WindroseRCON version.dll to " + destinationDll);
                SetStatus("WindroseRCON installed. Start or restart the server so the DLL initializes.", false);
            }
            catch (Exception ex)
            {
                SetStatus("RCON install failed: " + ex.Message, true);
            }
        }

        private void UninstallRconFiles()
        {
            var destinationDll = GetRconVersionDllPath();
            if (string.IsNullOrWhiteSpace(destinationDll) || !File.Exists(destinationDll))
            {
                SetStatus("No installed WindroseRCON version.dll was found for this server.", true);
                return;
            }

            try
            {
                File.Delete(destinationDll);
                RefreshRconStatusUi();
                currentPlayerCountDisplay = "n/a";
                playerCountValueLabel.Text = currentPlayerCountDisplay;
                rconPlayersTextBox.Text = "RCON player list will appear here.";
                AppendLog("Removed WindroseRCON version.dll from " + destinationDll);
                SetStatus("WindroseRCON uninstalled. Restart the server if it is currently running.", false);
            }
            catch (Exception ex)
            {
                SetStatus("RCON uninstall failed: " + ex.Message, true);
            }
        }

        private void SaveRconSettings()
        {
            var settingsPath = GetRconSettingsPath();
            if (string.IsNullOrWhiteSpace(settingsPath))
            {
                SetStatus("Load a server first so Captain's Console knows where to save RCON settings.", true);
                return;
            }

            try
            {
                var settings = BuildRconSettingsFromUi();
                var settingsDir = Path.GetDirectoryName(settingsPath);
                if (!string.IsNullOrWhiteSpace(settingsDir))
                {
                    Directory.CreateDirectory(settingsDir);
                }

                var builder = new StringBuilder();
                builder.AppendLine("# Windrose RCON Configuration");
                builder.AppendLine();
                builder.AppendLine("[RCON]");
                builder.AppendLine("BindAddress=" + settings.BindAddress);
                builder.AppendLine("Port=" + settings.Port);
                builder.AppendLine("Password=" + settings.Password);
                builder.AppendLine("AllowedIPs=" + settings.AllowedIPs);
                builder.AppendLine("MaxFailedAttempts=" + settings.MaxFailedAttempts);
                builder.AppendLine("Timeout=" + settings.TimeoutSeconds);
                builder.AppendLine("EnableLogging=" + (settings.EnableLogging ? "true" : "false"));
                builder.AppendLine();
                builder.AppendLine("[SecureRCON]");
                builder.AppendLine("Enabled=" + (settings.SecureEnabled ? "true" : "false"));
                builder.AppendLine("AESKey=" + settings.AesKey);
                File.WriteAllText(settingsPath, builder.ToString());
                RefreshRconStatusUi();
                AppendLog("Saved RCON settings to " + settingsPath);
                SetStatus("RCON settings saved.", false);
            }
            catch (Exception ex)
            {
                SetStatus("Could not save RCON settings: " + ex.Message, true);
            }
        }

        private void TestRconConnection()
        {
            try
            {
                var response = ExecuteRconCommand(BuildRconSettingsFromUi(), "info");
                rconPlayersTextBox.Text = response;
                SetStatus("RCON connection succeeded.", false);
            }
            catch (Exception ex)
            {
                SetStatus("RCON test failed: " + ex.Message, true);
            }
        }

        private void RefreshRconPlayers()
        {
            try
            {
                var response = ExecuteRconCommand(BuildRconSettingsFromUi(), "showplayers");
                rconPlayersTextBox.Text = string.IsNullOrWhiteSpace(response) ? "(no response)" : response;
                SetStatus("RCON player list refreshed.", false);
            }
            catch (Exception ex)
            {
                SetStatus("Could not refresh RCON players: " + ex.Message, true);
            }
        }

        private string FindLocalRconVersionDllSource()
        {
            var candidates = new[]
            {
                Path.Combine(Application.StartupPath, "WindroseRCON-main", "build", "windows", "x64", "release", "version.dll"),
                Path.Combine(Application.StartupPath, "WindroseRCON-main", "version.dll"),
                Path.Combine(Application.StartupPath, "..", "WindroseRCON-main", "build", "windows", "x64", "release", "version.dll"),
                Path.Combine(Application.StartupPath, "..", "WindroseRCON-main", "version.dll"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WindroseRCON-main", "build", "windows", "x64", "release", "version.dll"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WindroseRCON-main", "version.dll"),
            };

            foreach (var candidate in candidates)
            {
                try
                {
                    var fullPath = Path.GetFullPath(candidate);
                    if (File.Exists(fullPath))
                    {
                        return fullPath;
                    }
                }
                catch
                {
                }
            }

            return string.Empty;
        }

        private void ImportModFolder()
        {
            var modsRoot = GetModsRootPath();
            if (string.IsNullOrEmpty(modsRoot))
            {
                SetStatus("Load a server root or set an install directory first.", true);
                return;
            }

            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select an extracted mod folder to copy into ~mods";
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                var sourceDir = dialog.SelectedPath;
                var folderName = Path.GetFileName(sourceDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                var destinationDir = Path.Combine(modsRoot, folderName);
                CopyDirectory(sourceDir, destinationDir);
                AppendLog("Imported mod folder to " + destinationDir);
                SetStatus("Imported mod folder into ~mods.", false);
                PopulateModsInfo();
            }
        }

        private void ImportPakFiles()
        {
            var modsRoot = GetModsRootPath();
            if (string.IsNullOrEmpty(modsRoot))
            {
                SetStatus("Load a server root or set an install directory first.", true);
                return;
            }

            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Windrose mod files|*.pak;*.ucas;*.utoc|All files|*.*";
                dialog.Multiselect = true;
                dialog.Title = "Select .pak/.ucas/.utoc files to install";
                if (dialog.ShowDialog(this) != DialogResult.OK || dialog.FileNames.Length == 0)
                {
                    return;
                }

                var firstName = Path.GetFileNameWithoutExtension(dialog.FileNames[0]);
                var targetFolderName = string.IsNullOrEmpty(firstName) ? "ImportedMod" : firstName;
                var destinationDir = Path.Combine(modsRoot, targetFolderName);
                Directory.CreateDirectory(destinationDir);

                foreach (var file in dialog.FileNames)
                {
                    File.Copy(file, Path.Combine(destinationDir, Path.GetFileName(file)), true);
                }

                AppendLog("Imported pak files to " + destinationDir);
                SetStatus("Imported pak files into ~mods.", false);
                PopulateModsInfo();
            }
        }

        private void SearchCurseForgeMods(bool resetFilter, bool silent)
        {
            string apiKey;
            if (!TryGetConfiguredCurseForgeApiKey(out apiKey))
            {
                curseForgeStatusLabel.Text = GetCurseForgeDisabledMessage();
                if (!silent)
                {
                    SetStatus(curseForgeStatusLabel.Text, true);
                }
                return;
            }

            var gameId = CurseForgeGameId;
            if (resetFilter)
            {
                curseForgeSearchModeComboBox.SelectedIndex = 0;
                curseForgeSearchTextBox.Text = string.Empty;
            }
            var searchFilter = curseForgeSearchTextBox.Text.Trim();
            var searchById = curseForgeSearchModeComboBox.SelectedItem != null
                && string.Equals(curseForgeSearchModeComboBox.SelectedItem.ToString(), "Mod ID", StringComparison.OrdinalIgnoreCase);

            try
            {
                var serializer = new JavaScriptSerializer();
                List<CurseForgeModData> mods;

                if (searchById)
                {
                    int modId;
                    if (!int.TryParse(searchFilter, out modId) || modId <= 0)
                    {
                        throw new InvalidOperationException("Enter a valid numeric Mod ID.");
                    }

                    var singleModJson = DownloadCurseForgeJson(apiKey, "https://api.curseforge.com/v1/mods/" + modId);
                    var singleModResponse = serializer.Deserialize<CurseForgeSingleModResponse>(singleModJson);
                    mods = singleModResponse != null && singleModResponse.data != null
                        ? new List<CurseForgeModData> { singleModResponse.data }
                        : new List<CurseForgeModData>();
                }
                else
                {
                    var url = "https://api.curseforge.com/v1/mods/search?gameId=" + gameId
                        + "&pageSize=50&sortField=2&sortOrder=desc";
                    if (!string.IsNullOrWhiteSpace(searchFilter))
                    {
                        url += "&searchFilter=" + Uri.EscapeDataString(searchFilter);
                    }

                    var json = string.Empty;
                    try
                    {
                        json = DownloadCurseForgeJson(apiKey, url);
                    }
                    catch (WebException ex)
                    {
                        var webResponse = ex.Response as HttpWebResponse;
                        if (webResponse == null || webResponse.StatusCode != HttpStatusCode.BadRequest)
                        {
                            throw;
                        }

                        var fallbackUrl = "https://api.curseforge.com/v1/mods/search?gameId=" + gameId;
                        if (!string.IsNullOrWhiteSpace(searchFilter))
                        {
                            fallbackUrl += "&searchFilter=" + Uri.EscapeDataString(searchFilter);
                        }

                        json = DownloadCurseForgeJson(apiKey, fallbackUrl);
                    }

                    var response = serializer.Deserialize<CurseForgeModsSearchResponse>(json);
                    mods = response != null && response.data != null ? response.data : new List<CurseForgeModData>();
                }

                availableModsListView.Items.Clear();
                foreach (var mod in mods)
                {
                    var author = mod.authors != null && mod.authors.Count > 0 && !string.IsNullOrWhiteSpace(mod.authors[0].name)
                        ? mod.authors[0].name
                        : "Unknown";
                    var latestFile = mod.latestFiles != null && mod.latestFiles.Count > 0
                        ? (!string.IsNullOrWhiteSpace(mod.latestFiles[0].displayName) ? mod.latestFiles[0].displayName : mod.latestFiles[0].fileName)
                        : "(no files)";
                    var lastUpdated = "(unknown)";
                    DateTime lastUpdatedUtc;
                    if (!string.IsNullOrWhiteSpace(mod.dateModified)
                        && DateTime.TryParse(mod.dateModified, out lastUpdatedUtc))
                    {
                        lastUpdated = lastUpdatedUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
                    }

                    var item = new ListViewItem(string.IsNullOrWhiteSpace(mod.name) ? ("Mod " + mod.id) : mod.name);
                    item.SubItems.Add(mod.id.ToString());
                    item.SubItems.Add(author);
                    item.SubItems.Add(lastUpdated);
                    item.SubItems.Add(string.IsNullOrWhiteSpace(latestFile) ? "(no files)" : latestFile);
                    item.Tag = mod;
                    availableModsListView.Items.Add(item);
                }

                curseForgeStatusLabel.Text = searchById
                    ? (availableModsListView.Items.Count > 0 ? "Found mod by ID." : "No mod found for that ID.")
                    : ("Found " + availableModsListView.Items.Count + " mods.");
                if (!silent)
                {
                    SetStatus("CurseForge search completed.", false);
                }
            }
            catch (Exception ex)
            {
                curseForgeStatusLabel.Text = "CurseForge search failed.";
                if (!silent)
                {
                    SetStatus("CurseForge search failed: " + ex.Message, true);
                }
            }
        }

        private void SearchCurseForgeMods()
        {
            SearchCurseForgeMods(false, false);
        }

        private void InstallSelectedCurseForgeMod()
        {
            if (availableModsListView.SelectedItems.Count == 0)
            {
                SetStatus("Select a mod from available mods first.", true);
                return;
            }

            var selected = availableModsListView.SelectedItems[0].Tag as CurseForgeModData;
            if (selected == null)
            {
                SetStatus("Selected mod details are unavailable.", true);
                return;
            }

            try
            {
                var modsRoot = GetModsRootPath();
                if (string.IsNullOrWhiteSpace(modsRoot))
                {
                    SetStatus("Load a server root or set an install directory first.", true);
                    return;
                }

                Directory.CreateDirectory(modsRoot);
                var safeName = SanitizeFolderName(selected.name);
                var targetFolder = Path.Combine(modsRoot, safeName + "_" + selected.id);
                InstallCurseForgeModToFolder(selected, targetFolder, true);
                PopulateModsInfo();
                SetStatus("Installed mod: " + selected.name, false);
            }
            catch (Exception ex)
            {
                SetStatus("Install mod failed: " + ex.Message, true);
            }
        }

        private void EnableSelectedMod()
        {
            var folderPath = GetSelectedInstalledModFolderPath();
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                SetStatus("Select an installed mod first.", true);
                return;
            }

            var folderName = Path.GetFileName(folderPath);
            const string disabledPrefix = "_disabled_";
            if (!folderName.StartsWith(disabledPrefix, StringComparison.OrdinalIgnoreCase))
            {
                SetStatus("Selected mod is already enabled.", false);
                return;
            }

            var enabledName = folderName.Substring(disabledPrefix.Length);
            var enabledPath = Path.Combine(Path.GetDirectoryName(folderPath), enabledName);
            if (Directory.Exists(enabledPath))
            {
                SetStatus("Cannot enable mod because target folder already exists: " + enabledName, true);
                return;
            }

            Directory.Move(folderPath, enabledPath);
            PopulateModsInfo();
            SetStatus("Enabled mod: " + enabledName, false);
        }

        private void DisableSelectedMod()
        {
            var folderPath = GetSelectedInstalledModFolderPath();
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                SetStatus("Select an installed mod first.", true);
                return;
            }

            var folderName = Path.GetFileName(folderPath);
            const string disabledPrefix = "_disabled_";
            if (folderName.StartsWith(disabledPrefix, StringComparison.OrdinalIgnoreCase))
            {
                SetStatus("Selected mod is already disabled.", false);
                return;
            }

            var disabledPath = Path.Combine(Path.GetDirectoryName(folderPath), disabledPrefix + folderName);
            if (Directory.Exists(disabledPath))
            {
                SetStatus("Cannot disable mod because disabled folder already exists.", true);
                return;
            }

            Directory.Move(folderPath, disabledPath);
            PopulateModsInfo();
            SetStatus("Disabled mod: " + folderName, false);
        }

        private void RemoveSelectedMod()
        {
            var folderPath = GetSelectedInstalledModFolderPath();
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                SetStatus("Select an installed mod first.", true);
                return;
            }

            var folderName = Path.GetFileName(folderPath);
            var confirm = MessageBox.Show(
                this,
                "Move mod folder to trash?\n\n" + folderPath,
                AppTitle,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);
            if (confirm != DialogResult.Yes)
            {
                return;
            }

            MoveDirectoryToTrash(folderPath);
            PopulateModsInfo();
            SetStatus("Moved mod to trash: " + folderName, false);
        }

        private void UpdateSelectedMod()
        {
            var folderPath = GetSelectedInstalledModFolderPath();
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                SetStatus("Select an installed mod first.", true);
                return;
            }

            var manifest = ReadInstalledModManifest(folderPath);
            if (manifest == null || manifest.ModId <= 0)
            {
                SetStatus("Selected mod was not installed from CurseForge by Captain's Console, so update is not available.", true);
                return;
            }

            string apiKey;
            if (!TryGetConfiguredCurseForgeApiKey(out apiKey))
            {
                SetStatus(GetCurseForgeDisabledMessage(), true);
                return;
            }

            try
            {
                var serializer = new JavaScriptSerializer();
                var url = "https://api.curseforge.com/v1/mods/" + manifest.ModId;
                var json = DownloadCurseForgeJson(apiKey, url);
                var response = serializer.Deserialize<CurseForgeSingleModResponse>(json);
                var mod = response != null ? response.data : null;
                if (mod == null)
                {
                    throw new InvalidOperationException("CurseForge returned no mod data for mod id " + manifest.ModId + ".");
                }

                var latestFile = SelectBestDownloadableFile(apiKey, mod);
                if (latestFile == null || string.IsNullOrWhiteSpace(latestFile.downloadUrl))
                {
                    throw new InvalidOperationException("No downloadable file found for this mod.");
                }

                if (manifest.FileId == latestFile.id)
                {
                    SetStatus("Selected mod is already up to date.", false);
                    return;
                }

                InstallCurseForgeModToFolder(mod, folderPath, true);
                PopulateModsInfo();
                SetStatus("Updated mod: " + mod.name, false);
            }
            catch (Exception ex)
            {
                SetStatus("Update mod failed: " + ex.Message, true);
            }
        }

        private void PopulateModsInfo()
        {
            installedModsListView.Items.Clear();
            var modsRoot = GetModsRootPath();
            var latestFileIdByModId = new Dictionary<int, int?>();
            var hasCurseForgeApiKey = HasConfiguredCurseForgeApiKey();

            if (string.IsNullOrEmpty(modsRoot))
            {
                curseForgeStatusLabel.Text = "Load a server root to manage mods.";
                return;
            }

            if (!Directory.Exists(modsRoot))
            {
                Directory.CreateDirectory(modsRoot);
            }

            foreach (var dir in Directory.GetDirectories(modsRoot).OrderBy(delegate(string item) { return item; }, StringComparer.OrdinalIgnoreCase))
            {
                var folderName = Path.GetFileName(dir);
                var disabled = folderName.StartsWith("_disabled_", StringComparison.OrdinalIgnoreCase);
                var manifest = ReadInstalledModManifest(dir);
                var source = manifest != null && manifest.ModId > 0
                    ? ("CurseForge #" + manifest.ModId)
                    : "Manual";
                var updateStatus = GetInstalledModUpdateStatus(manifest, latestFileIdByModId);
                var modIdText = manifest != null && manifest.ModId > 0
                    ? manifest.ModId.ToString()
                    : "-";
                var lastUpdatedText = "-";
                if (manifest != null && !string.IsNullOrWhiteSpace(manifest.InstalledUtc))
                {
                    DateTime installedUtc;
                    if (DateTime.TryParse(manifest.InstalledUtc, out installedUtc))
                    {
                        lastUpdatedText = installedUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
                    }
                }
                else
                {
                    try
                    {
                        lastUpdatedText = Directory.GetLastWriteTimeUtc(dir).ToLocalTime().ToString("yyyy-MM-dd HH:mm");
                    }
                    catch
                    {
                    }
                }

                var item = new ListViewItem(folderName);
                item.SubItems.Add(disabled ? "Disabled" : "Enabled");
                item.SubItems.Add(modIdText);
                item.SubItems.Add(lastUpdatedText);
                item.SubItems.Add(updateStatus);
                item.SubItems.Add(source);
                item.Tag = dir;
                installedModsListView.Items.Add(item);
            }

            if (!hasCurseForgeApiKey)
            {
                curseForgeStatusLabel.Text = GetCurseForgeDisabledMessage();
            }
            else if (installedModsListView.Items.Count > 0)
            {
                curseForgeStatusLabel.Text = "CurseForge tools are ready.";
            }

            if (installedModsListView.Items.Count == 0)
            {
                var item = new ListViewItem("(no mods installed)");
                item.SubItems.Add("-");
                item.SubItems.Add("-");
                item.SubItems.Add("-");
                item.SubItems.Add("-");
                item.SubItems.Add("-");
                installedModsListView.Items.Add(item);
            }
        }

        private string GetInstalledModUpdateStatus(InstalledModManifest manifest, Dictionary<int, int?> latestFileIdByModId)
        {
            if (manifest == null || manifest.ModId <= 0)
            {
                return "Manual";
            }

            string apiKey;
            if (!TryGetConfiguredCurseForgeApiKey(out apiKey))
            {
                return "API unavailable";
            }

            int? latestFileId;
            if (!latestFileIdByModId.TryGetValue(manifest.ModId, out latestFileId))
            {
                latestFileId = null;
                try
                {
                    var serializer = new JavaScriptSerializer();
                    var url = "https://api.curseforge.com/v1/mods/" + manifest.ModId;
                    var json = DownloadCurseForgeJson(apiKey, url);
                    var response = serializer.Deserialize<CurseForgeSingleModResponse>(json);
                    var mod = response != null ? response.data : null;
                    var latestFile = SelectBestDownloadableFile(apiKey, mod);
                    if (latestFile != null)
                    {
                        latestFileId = latestFile.id;
                    }
                }
                catch
                {
                }

                latestFileIdByModId[manifest.ModId] = latestFileId;
            }

            if (!latestFileId.HasValue || manifest.FileId <= 0)
            {
                return "Unknown";
            }

            return manifest.FileId == latestFileId.Value ? "Up to date" : "Update required";
        }

        private string DownloadCurseForgeJson(string apiKey, string url)
        {
            var headers = new Dictionary<string, string>();
            headers["x-api-key"] = apiKey;
            headers["Accept"] = "application/json";
            return DownloadStringFromUrl(url, headers);
        }

        private void InstallCurseForgeModToFolder(CurseForgeModData mod, string targetFolder, bool allowOverwrite)
        {
            if (mod == null)
            {
                throw new ArgumentNullException("mod");
            }

            string apiKey;
            if (!TryGetConfiguredCurseForgeApiKey(out apiKey))
            {
                throw new InvalidOperationException(GetCurseForgeDisabledMessage());
            }

            var file = SelectBestDownloadableFile(apiKey, mod);
            if (file == null || string.IsNullOrWhiteSpace(file.downloadUrl))
            {
                throw new InvalidOperationException("No downloadable file found for " + mod.name + ". This mod may not provide a downloadable server-compatible file.");
            }

            if (Directory.Exists(targetFolder))
            {
                if (!allowOverwrite)
                {
                    throw new InvalidOperationException("Target mod folder already exists: " + Path.GetFileName(targetFolder));
                }

                var overwrite = MessageBox.Show(
                    this,
                    "The mod folder already exists. Replace it?\n\n" + targetFolder,
                    AppTitle,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (overwrite != DialogResult.Yes)
                {
                    return;
                }

                MoveDirectoryToTrash(targetFolder);
            }

            Directory.CreateDirectory(targetFolder);

            var tempFile = Path.Combine(Path.GetTempPath(), "windrose-mod-" + Guid.NewGuid().ToString("N") + Path.GetExtension(file.downloadUrl));
            var headers = new Dictionary<string, string>();
            headers["x-api-key"] = apiKey;
            DownloadFileToPath(file.downloadUrl, tempFile, headers);

            var extension = Path.GetExtension(tempFile);
            if (string.Equals(extension, ".zip", StringComparison.OrdinalIgnoreCase))
            {
                ZipFile.ExtractToDirectory(tempFile, targetFolder);
            }
            else
            {
                var outputName = !string.IsNullOrWhiteSpace(file.fileName)
                    ? file.fileName
                    : Path.GetFileName(tempFile);
                File.Copy(tempFile, Path.Combine(targetFolder, outputName), true);
            }

            var manifest = new InstalledModManifest
            {
                ModId = mod.id,
                ModName = mod.name,
                FileId = file.id,
                FileName = file.fileName,
                InstalledUtc = DateTime.UtcNow.ToString("o")
            };
            var serializer = new JavaScriptSerializer();
            File.WriteAllText(Path.Combine(targetFolder, "_windrose_mod.json"), serializer.Serialize(manifest));

            try
            {
                File.Delete(tempFile);
            }
            catch
            {
            }
        }

        private void RefreshModsProviderUi()
        {
            selectedModsProvider = GetSelectedModsProvider();
            if (discoverModsGroup != null)
            {
                discoverModsGroup.Text = "Discover & Install (" + selectedModsProvider + ")";
            }

            openCurseForgeButton.Text = selectedModsProvider == ModProviderNexusMods
                ? "Open Nexus Mods"
                : "Open CurseForge";

            if (selectedModsProvider == ModProviderNexusMods)
            {
                curseForgeStatusLabel.Text = HasConfiguredNexusModsApiKey()
                    ? "Nexus Mods key saved. In-app Nexus search/install is not wired yet."
                    : "Enter and save a Nexus Mods API key for future Nexus integration.";
                availableModsListView.Items.Clear();
            }
            else
            {
                curseForgeStatusLabel.Text = HasConfiguredCurseForgeApiKey()
                    ? "CurseForge online tools are enabled."
                    : GetCurseForgeDisabledMessage();
            }

            UpdateProcessUi();
        }

        private void OnModsProviderChanged()
        {
            selectedModsProvider = GetSelectedModsProvider();
            modsApiKeyTextBox.Text = GetStoredApiKeyForProvider(selectedModsProvider);
            RefreshModsProviderUi();
            SavePreferences();
        }

        private string GetSelectedModsProvider()
        {
            return modsProviderComboBox.SelectedItem != null
                && string.Equals(modsProviderComboBox.SelectedItem.ToString(), ModProviderNexusMods, StringComparison.OrdinalIgnoreCase)
                ? ModProviderNexusMods
                : ModProviderCurseForge;
        }

        private string GetSelectedModsProviderBrowseUrl()
        {
            return selectedModsProvider == ModProviderNexusMods
                ? "https://www.nexusmods.com/windrose"
                : "https://www.curseforge.com/windrose";
        }

        private void SaveCurrentModsApiKey()
        {
            SetStoredApiKeyForProvider(GetSelectedModsProvider(), modsApiKeyTextBox.Text.Trim());
            SavePreferences();
            RefreshModsProviderUi();
            SetStatus(GetSelectedModsProvider() + " API key saved locally.", false);
        }

        private string GetStoredApiKeyForProvider(string provider)
        {
            return string.Equals(provider, ModProviderNexusMods, StringComparison.OrdinalIgnoreCase)
                ? storedNexusModsApiKey
                : storedCurseForgeApiKey;
        }

        private void SetStoredApiKeyForProvider(string provider, string apiKey)
        {
            if (string.Equals(provider, ModProviderNexusMods, StringComparison.OrdinalIgnoreCase))
            {
                storedNexusModsApiKey = apiKey ?? string.Empty;
            }
            else
            {
                storedCurseForgeApiKey = apiKey ?? string.Empty;
            }
        }

        private bool HasConfiguredSelectedModsApiKey()
        {
            return selectedModsProvider == ModProviderNexusMods
                ? HasConfiguredNexusModsApiKey()
                : HasConfiguredCurseForgeApiKey();
        }

        private bool SelectedModsProviderSupportsSearch()
        {
            return selectedModsProvider == ModProviderCurseForge && HasConfiguredSelectedModsApiKey();
        }

        private bool SelectedModsProviderSupportsInstall()
        {
            return selectedModsProvider == ModProviderCurseForge && HasConfiguredSelectedModsApiKey();
        }

        private bool SelectedModsProviderSupportsUpdates()
        {
            return selectedModsProvider == ModProviderCurseForge && HasConfiguredCurseForgeApiKey();
        }

        private void SearchSelectedModsProvider()
        {
            if (selectedModsProvider == ModProviderNexusMods)
            {
                curseForgeStatusLabel.Text = HasConfiguredNexusModsApiKey()
                    ? "Nexus Mods key saved. In-app Nexus search/install is not wired yet."
                    : "Enter and save a Nexus Mods API key first.";
                SetStatus(curseForgeStatusLabel.Text, true);
                return;
            }

            SearchCurseForgeMods();
        }

        private void InstallSelectedModFromProvider()
        {
            if (selectedModsProvider == ModProviderNexusMods)
            {
                SetStatus("In-app Nexus Mods install is not wired yet. Use Open Nexus Mods for now.", true);
                return;
            }

            InstallSelectedCurseForgeMod();
        }

        private bool HasConfiguredCurseForgeApiKey()
        {
            string apiKey;
            return TryGetConfiguredCurseForgeApiKey(out apiKey);
        }

        private bool HasConfiguredNexusModsApiKey()
        {
            string apiKey;
            return TryGetConfiguredNexusModsApiKey(out apiKey);
        }

        private bool TryGetConfiguredCurseForgeApiKey(out string apiKey)
        {
            apiKey = storedCurseForgeApiKey == null ? string.Empty : storedCurseForgeApiKey.Trim();
            return !string.IsNullOrWhiteSpace(apiKey);
        }

        private bool TryGetConfiguredNexusModsApiKey(out string apiKey)
        {
            apiKey = storedNexusModsApiKey == null ? string.Empty : storedNexusModsApiKey.Trim();
            return !string.IsNullOrWhiteSpace(apiKey);
        }

        private string GetCurseForgeDisabledMessage()
        {
            return "Online CurseForge tools are disabled until you enter and save a CurseForge API key.";
        }

        private static string DownloadStringFromUrl(string url, IDictionary<string, string> headers)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            request.UserAgent = "WindroseCaptainsConsole/0.9.5";

            if (headers != null)
            {
                foreach (var pair in headers)
                {
                    ApplyRequestHeader(request, pair.Key, pair.Value);
                }
            }

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        private static void DownloadFileToPath(string url, string destinationPath, IDictionary<string, string> headers)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            request.UserAgent = "WindroseCaptainsConsole/0.9.5";

            if (headers != null)
            {
                foreach (var pair in headers)
                {
                    ApplyRequestHeader(request, pair.Key, pair.Value);
                }
            }

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var responseStream = response.GetResponseStream())
            using (var fileStream = File.Create(destinationPath))
            {
                responseStream.CopyTo(fileStream);
            }
        }

        private static void ApplyRequestHeader(HttpWebRequest request, string key, string value)
        {
            if (request == null || string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            if (string.Equals(key, "Accept", StringComparison.OrdinalIgnoreCase))
            {
                request.Accept = value;
                return;
            }

            request.Headers[key] = value;
        }

        private CurseForgeFileData SelectBestDownloadableFile(string apiKey, CurseForgeModData mod)
        {
            if (mod == null)
            {
                return null;
            }

            var latestMatch = SelectFirstWithDownloadUrl(apiKey, mod.id, mod.latestFiles);
            if (latestMatch != null)
            {
                return latestMatch;
            }

            var files = FetchCurseForgeModFiles(apiKey, mod.id);
            return SelectFirstWithDownloadUrl(apiKey, mod.id, files);
        }

        private List<CurseForgeFileData> FetchCurseForgeModFiles(string apiKey, int modId)
        {
            if (modId <= 0)
            {
                return new List<CurseForgeFileData>();
            }

            var serializer = new JavaScriptSerializer();
            var url = "https://api.curseforge.com/v1/mods/" + modId + "/files?pageSize=50&index=0";
            var json = DownloadCurseForgeJson(apiKey, url);
            var response = serializer.Deserialize<CurseForgeModFilesResponse>(json);
            return response != null && response.data != null ? response.data : new List<CurseForgeFileData>();
        }

        private CurseForgeFileData SelectFirstWithDownloadUrl(string apiKey, int modId, List<CurseForgeFileData> files)
        {
            if (files == null)
            {
                return null;
            }

            foreach (var candidate in files)
            {
                if (candidate == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(candidate.downloadUrl))
                {
                    candidate.downloadUrl = ResolveCurseForgeDownloadUrl(apiKey, modId, candidate.id);
                }

                if (!string.IsNullOrWhiteSpace(candidate.downloadUrl))
                {
                    return candidate;
                }
            }

            return null;
        }

        private string ResolveCurseForgeDownloadUrl(string apiKey, int modId, int fileId)
        {
            if (modId <= 0 || fileId <= 0)
            {
                return string.Empty;
            }

            try
            {
                var serializer = new JavaScriptSerializer();
                var url = "https://api.curseforge.com/v1/mods/" + modId + "/files/" + fileId + "/download-url";
                var json = DownloadCurseForgeJson(apiKey, url);
                var response = serializer.Deserialize<CurseForgeDownloadUrlResponse>(json);
                return response != null && !string.IsNullOrWhiteSpace(response.data)
                    ? response.data.Trim()
                    : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private string GetSelectedInstalledModFolderPath()
        {
            if (installedModsListView.SelectedItems.Count == 0)
            {
                return string.Empty;
            }

            var tag = installedModsListView.SelectedItems[0].Tag as string;
            if (string.IsNullOrWhiteSpace(tag) || !Directory.Exists(tag))
            {
                return string.Empty;
            }

            return tag;
        }

        private InstalledModManifest ReadInstalledModManifest(string folderPath)
        {
            try
            {
                var manifestPath = Path.Combine(folderPath, "_windrose_mod.json");
                if (!File.Exists(manifestPath))
                {
                    return null;
                }

                var serializer = new JavaScriptSerializer();
                return serializer.Deserialize<InstalledModManifest>(File.ReadAllText(manifestPath));
            }
            catch
            {
                return null;
            }
        }

        private string MoveDirectoryToTrash(string sourcePath)
        {
            return MovePathToTrash(sourcePath);
        }

        private string MovePathToTrash(string sourcePath)
        {
            var isDirectory = !string.IsNullOrWhiteSpace(sourcePath) && Directory.Exists(sourcePath);
            var isFile = !string.IsNullOrWhiteSpace(sourcePath) && File.Exists(sourcePath);
            if (!isDirectory && !isFile)
            {
                throw new DirectoryNotFoundException("Could not find path to move to trash.");
            }

            var parentDir = Path.GetDirectoryName(sourcePath);
            if (string.IsNullOrWhiteSpace(parentDir) || !Directory.Exists(parentDir))
            {
                throw new DirectoryNotFoundException("Could not resolve the parent folder for trash placement.");
            }

            var trashRoot = Path.Combine(parentDir, ".windrosecc-trash");
            Directory.CreateDirectory(trashRoot);

            var folderName = Path.GetFileName(sourcePath);
            var stamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var destinationPath = Path.Combine(trashRoot, folderName + "_" + stamp);
            var suffix = 1;
            while (Directory.Exists(destinationPath) || File.Exists(destinationPath))
            {
                destinationPath = Path.Combine(trashRoot, folderName + "_" + stamp + "_" + suffix);
                suffix++;
            }

            Directory.Move(sourcePath, destinationPath);
            return destinationPath;
        }

        private static string SanitizeFolderName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return "CurseForgeMod";
            }

            var cleaned = new string(input.Where(delegate(char ch)
            {
                return !Path.GetInvalidFileNameChars().Contains(ch);
            }).ToArray()).Trim();

            return string.IsNullOrWhiteSpace(cleaned) ? "CurseForgeMod" : cleaned;
        }

        private sealed class CurseForgeModsSearchResponse
        {
            public List<CurseForgeModData> data { get; set; }
        }

        private sealed class CurseForgeSingleModResponse
        {
            public CurseForgeModData data { get; set; }
        }

        private sealed class CurseForgeModFilesResponse
        {
            public List<CurseForgeFileData> data { get; set; }
        }

        private sealed class CurseForgeDownloadUrlResponse
        {
            public string data { get; set; }
        }

        private sealed class CurseForgeModData
        {
            public int id { get; set; }
            public string name { get; set; }
            public string dateModified { get; set; }
            public List<CurseForgeAuthorData> authors { get; set; }
            public List<CurseForgeFileData> latestFiles { get; set; }
        }

        private sealed class CurseForgeAuthorData
        {
            public string name { get; set; }
        }

        private sealed class CurseForgeFileData
        {
            public int id { get; set; }
            public string displayName { get; set; }
            public string fileName { get; set; }
            public string downloadUrl { get; set; }
        }

        private sealed class InstalledModManifest
        {
            public int ModId { get; set; }
            public string ModName { get; set; }
            public int FileId { get; set; }
            public string FileName { get; set; }
            public string InstalledUtc { get; set; }
        }

        private FullServerBackupResult CreateFullServerBackup(string rootPath)
        {
            var backupFolder = EnsureBackupFolder();
            var targetFolder = Path.Combine(backupFolder, "server-full-" + DateTime.Now.ToString("yyyyMMdd-HHmmss"));
            if (targetFolder.StartsWith(Path.GetFullPath(rootPath), StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Choose a backup folder outside the server install directory.");
            }

            var skippedFiles = new List<string>();
            CopyDirectory(rootPath, targetFolder, skippedFiles);
            foreach (var skippedFile in skippedFiles)
            {
                AppendLog("[backup skipped] " + skippedFile);
            }

            return new FullServerBackupResult
            {
                TargetFolder = targetFolder,
                SkippedFileCount = skippedFiles.Count
            };
        }

        private static void CopyDirectory(string sourceDir, string destinationDir, IList<string> skippedFiles = null)
        {
            Directory.CreateDirectory(destinationDir);
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var destinationFile = Path.Combine(destinationDir, Path.GetFileName(file));
                try
                {
                    CopyFileAllowingReadersAndWriters(file, destinationFile);
                }
                catch (IOException ex)
                {
                    if (skippedFiles == null || !ShouldSkipLockedBackupFile(file))
                    {
                        throw;
                    }

                    skippedFiles.Add(file + " (" + ex.Message + ")");
                }
                catch (UnauthorizedAccessException ex)
                {
                    if (skippedFiles == null || !ShouldSkipLockedBackupFile(file))
                    {
                        throw;
                    }

                    skippedFiles.Add(file + " (" + ex.Message + ")");
                }
            }

            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                CopyDirectory(dir, Path.Combine(destinationDir, Path.GetFileName(dir)), skippedFiles);
            }
        }

        private static void CopyFileAllowingReadersAndWriters(string sourceFilePath, string destinationFilePath)
        {
            var destinationDir = Path.GetDirectoryName(destinationFilePath);
            if (!string.IsNullOrWhiteSpace(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            Exception lastError = null;
            for (var attempt = 0; attempt < 3; attempt++)
            {
                try
                {
                    using (var sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                    using (var destinationStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        sourceStream.CopyTo(destinationStream);
                        destinationStream.Flush(true);
                        return;
                    }
                }
                catch (IOException ex)
                {
                    lastError = ex;
                }
                catch (UnauthorizedAccessException ex)
                {
                    lastError = ex;
                }

                System.Threading.Thread.Sleep(150);
            }

            if (lastError != null)
            {
                throw lastError;
            }
        }

        private static bool ShouldSkipLockedBackupFile(string filePath)
        {
            var extension = Path.GetExtension(filePath);
            var fileName = Path.GetFileName(filePath);
            if (string.Equals(extension, ".log", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".tmp", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".lock", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(fileName, "LOCK", StringComparison.OrdinalIgnoreCase)
                || string.Equals(fileName, "CURRENT", StringComparison.OrdinalIgnoreCase)
                || fileName.StartsWith("LOG", StringComparison.OrdinalIgnoreCase)
                || fileName.StartsWith("MANIFEST-", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var normalized = filePath.Replace('/', '\\');
            return normalized.IndexOf("\\R5\\Saved\\Logs\\", StringComparison.OrdinalIgnoreCase) >= 0
                || normalized.IndexOf("\\windrosercon\\", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private sealed class FullServerBackupResult
        {
            public string TargetFolder { get; set; }
            public int SkippedFileCount { get; set; }
        }

        private void WireFieldEvents()
        {
            serverNameTextBox.TextChanged += delegate
            {
                if (!isBinding && currentState != null)
                {
                    var server = EnsureServer();
                    if (server.ServerName != serverNameTextBox.Text)
                    {
                        server.ServerName = serverNameTextBox.Text;
                        MarkDirty();
                    }
                }
            };
            inviteCodeTextBox.TextChanged += delegate
            {
                if (!isBinding && currentState != null)
                {
                    var server = EnsureServer();
                    var newValue = inviteCodeTextBox.Text.Trim();
                    if (server.InviteCode != newValue)
                    {
                        server.InviteCode = newValue;
                        MarkDirty();
                        RefreshWarnings();
                    }
                }
            };
            passwordTextBox.TextChanged += delegate
            {
                if (!isBinding && currentState != null)
                {
                    var server = EnsureServer();
                    var newPassword = passwordTextBox.Text;
                    var newProtected = newPassword.Length > 0;
                    if (server.Password != newPassword || server.IsPasswordProtected != newProtected)
                    {
                        server.Password = newPassword;
                        server.IsPasswordProtected = newProtected;
                        MarkDirty();
                        RefreshWarnings();
                    }
                }
            };
            maxPlayersNumeric.ValueChanged += delegate
            {
                if (!isBinding && currentState != null)
                {
                    var server = EnsureServer();
                    var newValue = Decimal.ToInt32(maxPlayersNumeric.Value);
                    if (server.MaxPlayerCount != newValue)
                    {
                        server.MaxPlayerCount = newValue;
                        MarkDirty();
                        RefreshWarnings();
                    }
                }
            };
            activeWorldIdTextBox.TextChanged += delegate
            {
                if (!isBinding && currentState != null)
                {
                    var server = EnsureServer();
                    var newValue = activeWorldIdTextBox.Text.Trim();
                    if (server.WorldIslandId != newValue)
                    {
                        server.WorldIslandId = newValue;
                        MarkDirty();
                        RefreshWarnings();
                        RefreshWorldList();
                    }
                }
            };
            regionComboBox.SelectedIndexChanged += delegate
            {
                if (!isBinding && currentState != null)
                {
                    var server = EnsureServer();
                    var newValue = regionComboBox.SelectedItem == null || regionComboBox.SelectedItem.ToString() == "Auto"
                        ? string.Empty
                        : regionComboBox.SelectedItem.ToString();
                    if (server.UserSelectedRegion != newValue)
                    {
                        server.UserSelectedRegion = newValue;
                        MarkDirty();
                    }
                }
            };
            directConnectionCheckBox.CheckedChanged += delegate
            {
                if (!isBinding && currentState != null)
                {
                    var server = EnsureServer();
                    if (server.UseDirectConnection != directConnectionCheckBox.Checked)
                    {
                        server.UseDirectConnection = directConnectionCheckBox.Checked;
                        MarkDirty();
                        RefreshWarnings();
                    }
                }
            };
            directPortNumeric.ValueChanged += delegate
            {
                if (!isBinding && currentState != null)
                {
                    var server = EnsureServer();
                    var newValue = Decimal.ToInt32(directPortNumeric.Value);
                    if (server.DirectConnectionServerPort != newValue)
                    {
                        server.DirectConnectionServerPort = newValue;
                        MarkDirty();
                        RefreshWarnings();
                    }
                }
            };
            bindInterfaceTextBox.TextChanged += delegate
            {
                if (!isBinding && currentState != null)
                {
                    var server = EnsureServer();
                    var newValue = bindInterfaceTextBox.Text.Trim();
                    if (server.DirectConnectionProxyAddress != newValue)
                    {
                        server.DirectConnectionProxyAddress = newValue;
                        MarkDirty();
                    }
                }
            };
            listeningIpTextBox.TextChanged += delegate
            {
                if (!isBinding && currentState != null)
                {
                    var server = EnsureServer();
                    var newValue = listeningIpTextBox.Text.Trim();
                    if (server.P2pProxyAddress != newValue)
                    {
                        server.P2pProxyAddress = newValue;
                        MarkDirty();
                    }
                }
            };
            directAddressTextBox.TextChanged += delegate
            {
                if (!isBinding && currentState != null)
                {
                    var server = EnsureServer();
                    var newValue = directAddressTextBox.Text.Trim();
                    if (server.DirectConnectionServerAddress != newValue)
                    {
                        server.DirectConnectionServerAddress = newValue;
                        MarkDirty();
                    }
                }
            };

            worldNameTextBox.TextChanged += delegate
            {
                if (!isBinding && currentState != null)
                {
                    var world = GetSelectedWorld();
                    if (world == null)
                    {
                        return;
                    }
                    if (world.WorldName != worldNameTextBox.Text)
                    {
                        world.WorldName = worldNameTextBox.Text;
                        UpdateWorldPresetAndDirty();
                    }
                }
            };
            combatDifficultyComboBox.SelectedIndexChanged += delegate
            {
                if (!isBinding && currentState != null)
                {
                    var world = GetSelectedWorld();
                    if (world == null)
                    {
                        return;
                    }
                    var newValue = ConvertCombatDisplayToTag(combatDifficultyComboBox.SelectedItem as string);
                    if (world.CombatDifficulty != newValue)
                    {
                        world.CombatDifficulty = newValue;
                        UpdateWorldPresetAndDirty();
                    }
                }
            };
            sharedQuestsCheckBox.CheckedChanged += delegate
            {
                if (!isBinding && currentState != null)
                {
                    var world = GetSelectedWorld();
                    if (world == null)
                    {
                        return;
                    }
                    if (world.SharedQuests != sharedQuestsCheckBox.Checked)
                    {
                        world.SharedQuests = sharedQuestsCheckBox.Checked;
                        UpdateWorldPresetAndDirty();
                    }
                }
            };
            immersiveExploreCheckBox.CheckedChanged += delegate
            {
                if (!isBinding && currentState != null)
                {
                    var world = GetSelectedWorld();
                    if (world == null)
                    {
                        return;
                    }
                    if (world.EasyExplore != immersiveExploreCheckBox.Checked)
                    {
                        world.EasyExplore = immersiveExploreCheckBox.Checked;
                        UpdateWorldPresetAndDirty();
                    }
                }
            };
            mobHealthNumeric.ValueChanged += delegate
            {
                if (!isBinding && currentState != null)
                {
                    var world = GetSelectedWorld();
                    if (world == null)
                    {
                        return;
                    }
                    var newValue = Decimal.ToDouble(mobHealthNumeric.Value);
                    if (world.MobHealthMultiplier != newValue)
                    {
                        world.MobHealthMultiplier = newValue;
                        UpdateWorldPresetAndDirty();
                    }
                }
            };
            mobDamageNumeric.ValueChanged += delegate
            {
                if (!isBinding && currentState != null)
                {
                    var world = GetSelectedWorld();
                    if (world == null)
                    {
                        return;
                    }
                    var newValue = Decimal.ToDouble(mobDamageNumeric.Value);
                    if (world.MobDamageMultiplier != newValue)
                    {
                        world.MobDamageMultiplier = newValue;
                        UpdateWorldPresetAndDirty();
                    }
                }
            };
            shipHealthNumeric.ValueChanged += delegate
            {
                if (!isBinding && currentState != null)
                {
                    var world = GetSelectedWorld();
                    if (world == null)
                    {
                        return;
                    }
                    var newValue = Decimal.ToDouble(shipHealthNumeric.Value);
                    if (world.ShipsHealthMultiplier != newValue)
                    {
                        world.ShipsHealthMultiplier = newValue;
                        UpdateWorldPresetAndDirty();
                    }
                }
            };
            shipDamageNumeric.ValueChanged += delegate
            {
                if (!isBinding && currentState != null)
                {
                    var world = GetSelectedWorld();
                    if (world == null)
                    {
                        return;
                    }
                    var newValue = Decimal.ToDouble(shipDamageNumeric.Value);
                    if (world.ShipsDamageMultiplier != newValue)
                    {
                        world.ShipsDamageMultiplier = newValue;
                        UpdateWorldPresetAndDirty();
                    }
                }
            };
            boardingNumeric.ValueChanged += delegate
            {
                if (!isBinding && currentState != null)
                {
                    var world = GetSelectedWorld();
                    if (world == null)
                    {
                        return;
                    }
                    var newValue = Decimal.ToDouble(boardingNumeric.Value);
                    if (world.BoardingDifficultyMultiplier != newValue)
                    {
                        world.BoardingDifficultyMultiplier = newValue;
                        UpdateWorldPresetAndDirty();
                    }
                }
            };
            coopStatsNumeric.ValueChanged += delegate
            {
                if (!isBinding && currentState != null)
                {
                    var world = GetSelectedWorld();
                    if (world == null)
                    {
                        return;
                    }
                    var newValue = Decimal.ToDouble(coopStatsNumeric.Value);
                    if (world.CoopStatsCorrectionModifier != newValue)
                    {
                        world.CoopStatsCorrectionModifier = newValue;
                        UpdateWorldPresetAndDirty();
                    }
                }
            };
            coopShipStatsNumeric.ValueChanged += delegate
            {
                if (!isBinding && currentState != null)
                {
                    var world = GetSelectedWorld();
                    if (world == null)
                    {
                        return;
                    }
                    var newValue = Decimal.ToDouble(coopShipStatsNumeric.Value);
                    if (world.CoopShipStatsCorrectionModifier != newValue)
                    {
                        world.CoopShipStatsCorrectionModifier = newValue;
                        UpdateWorldPresetAndDirty();
                    }
                }
            };
        }

        private void SetConfigEditorsEnabled(bool enabled)
        {
            serverNameTextBox.Enabled = enabled;
            inviteCodeTextBox.Enabled = enabled;
            passwordTextBox.Enabled = enabled;
            maxPlayersNumeric.Enabled = enabled;
            activeWorldIdTextBox.Enabled = enabled;
            regionComboBox.Enabled = enabled;
            directConnectionCheckBox.Enabled = enabled;
            directPortNumeric.Enabled = enabled;
            bindInterfaceTextBox.Enabled = enabled;
            listeningIpTextBox.Enabled = enabled;

            worldNameTextBox.Enabled = enabled;
            combatDifficultyComboBox.Enabled = enabled;
            sharedQuestsCheckBox.Enabled = enabled;
            immersiveExploreCheckBox.Enabled = enabled;
            mobHealthNumeric.Enabled = enabled;
            mobDamageNumeric.Enabled = enabled;
            shipHealthNumeric.Enabled = enabled;
            shipDamageNumeric.Enabled = enabled;
            boardingNumeric.Enabled = enabled;
            coopStatsNumeric.Enabled = enabled;
            coopShipStatsNumeric.Enabled = enabled;
            setActiveWorldButton.Enabled = enabled;
            saveServerTabButton.Enabled = enabled;
            saveWorldTabButton.Enabled = enabled;
        }

        private string GetSamplePath()
        {
            return Path.GetFullPath(Path.Combine(Application.StartupPath, "..", "fixtures", "sample-windrose-server"));
        }

        private void LoadServer(string rootPath)
        {
            if (string.IsNullOrWhiteSpace(rootPath))
            {
                SetStatus("Please enter a Windrose server path first.", true);
                return;
            }

            try
            {
                currentState = WindroseRepository.Discover(rootPath.Trim());
                currentLaunchTarget = DetectLaunchTarget(currentState.ServerRoot);
                isDirty = false;
                BindStateToUi();
                UpdateWindowTitle();
                SavePreferences();
                StartServerFileLogTail(currentState.ServerRoot);
                AppendLog("Loaded server root: " + currentState.ServerRoot);
                if (currentLaunchTarget != null)
                {
                    AppendLog("Launch target detected: " + currentLaunchTarget.DisplayName);
                }
                else
                {
                    AppendLog("No launch target detected in the loaded root. Start/stop controls will stay disabled.");
                }
                SetStatus("Loaded " + currentState.ServerRoot + ".", false);
            }
            catch (Exception ex)
            {
                var trimmedRoot = rootPath.Trim();
                var missingServerDescription = ex.Message != null && ex.Message.IndexOf("Could not find ServerDescription.json", StringComparison.OrdinalIgnoreCase) >= 0;
                if (missingServerDescription && WindroseRepository.IsLikelyInstalledServerRoot(trimmedRoot))
                {
                    var result = MessageBox.Show(
                        this,
                        "This folder looks like a valid Windrose server install, but ServerDescription.json is missing.\n\nRecommended: start the server once so Windrose generates its own config, then click Load Server.\n\nCreate a manager default config anyway?",
                        AppTitle,
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);

                    if (result == DialogResult.Yes)
                    {
                        try
                        {
                            var createdPath = WindroseRepository.CreateDefaultServerDescription(trimmedRoot);
                            AppendLog("Created default server config at " + createdPath);
                            currentLaunchTarget = DetectLaunchTarget(currentState.ServerRoot);
                            isDirty = false;
                            BindStateToUi();
                            UpdateWindowTitle();
                            StartServerFileLogTail(currentState.ServerRoot);
                            SetStatus("Created default ServerDescription.json and loaded server.", false);
                            return;
                        }
                        catch (Exception createEx)
                        {
                            SetConfigEditorsEnabled(false);
                            SetStatus("Could not create default ServerDescription.json: " + createEx.Message, true);
                            return;
                        }
                    }
                }

                SetConfigEditorsEnabled(false);
                SetStatus(ex.Message, true);
            }
        }

        private void SaveState()
        {
            if (currentState == null)
            {
                return;
            }

            try
            {
                currentState = WindroseRepository.Save(currentState);
                currentLaunchTarget = DetectLaunchTarget(currentState.ServerRoot);
                isDirty = false;
                BindStateToUi();
                UpdateWindowTitle();
                SetStatus("Saved successfully. Backup files were created before overwrite.", false);
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
        }

        private void BindStateToUi()
        {
            if (currentState == null)
            {
                return;
            }

            isBinding = true;

            var server = currentState.Server;
            serverNameTextBox.Text = server.ServerName;
            inviteCodeTextBox.Text = server.InviteCode;
            passwordTextBox.Text = server.Password;
            maxPlayersNumeric.Value = ClampNumeric(server.MaxPlayerCount, maxPlayersNumeric);
            activeWorldIdTextBox.Text = server.WorldIslandId;
            regionComboBox.SelectedItem = string.IsNullOrEmpty(server.UserSelectedRegion) ? "Auto" : server.UserSelectedRegion;
            directConnectionCheckBox.Checked = server.UseDirectConnection;
            directPortNumeric.Value = ClampNumeric(server.DirectConnectionServerPort, directPortNumeric);
            bindInterfaceTextBox.Text = server.DirectConnectionProxyAddress;
            listeningIpTextBox.Text = server.P2pProxyAddress;
            directAddressTextBox.Text = server.DirectConnectionServerAddress;
            installDirTextBox.Text = currentState.ServerRoot;

            RefreshWorldList();

            SelectWorldInListView();

            SetConfigEditorsEnabled(true);
            BindSelectedWorldToUi();
            RefreshWarnings();
            PopulateModsInfo();
            LoadRconSettingsIntoUi();
            RefreshModsProviderUi();
            setActiveWorldButton.Enabled = currentState.SelectedWorldKey != null;
            UpdateLaunchTargetUi();
            UpdateProcessUi();

            isDirty = false;
            UpdateWindowTitle();
            isBinding = false;
        }

        private void RefreshWorldList()
        {
            worldsListView.BeginUpdate();
            worldsListView.Items.Clear();
            if (currentState != null)
            {
                foreach (var world in currentState.Worlds)
                {
                    var status = world.FolderName == currentState.Server.WorldIslandId ? "Active" : (world.FolderName != world.IslandId ? "Mismatch" : "Ready");
                    var item = new ListViewItem(world.WorldName);
                    item.SubItems.Add(world.GameVersion);
                    item.SubItems.Add(status);
                    item.SubItems.Add(WindroseRepository.DetectPreset(world));
                    item.SubItems.Add(world.FolderName);
                    item.Tag = world;
                    worldsListView.Items.Add(item);
                }
            }
            worldsListView.EndUpdate();
        }

        private void OnSelectedWorldChanged()
        {
            if (isBinding || currentState == null)
            {
                return;
            }

            var item = worldsListView.SelectedItems.Count > 0 ? worldsListView.SelectedItems[0] : null;
            var world = item == null ? null : item.Tag as WorldEditableState;
            currentState.SelectedWorldKey = world == null ? null : world.Key;
            BindSelectedWorldToUi();
            RefreshWarnings();
        }

        private void BindSelectedWorldToUi()
        {
            isBinding = true;
            var world = GetSelectedWorld();
            var enabled = world != null;

            worldNameTextBox.Enabled = enabled;
            worldPresetTextBox.Enabled = false;
            combatDifficultyComboBox.Enabled = enabled;
            sharedQuestsCheckBox.Enabled = enabled;
            immersiveExploreCheckBox.Enabled = enabled;
            mobHealthNumeric.Enabled = enabled;
            mobDamageNumeric.Enabled = enabled;
            shipHealthNumeric.Enabled = enabled;
            shipDamageNumeric.Enabled = enabled;
            boardingNumeric.Enabled = enabled;
            coopStatsNumeric.Enabled = enabled;
            coopShipStatsNumeric.Enabled = enabled;
            setActiveWorldButton.Enabled = enabled;

            if (world == null)
            {
                worldNameTextBox.Text = string.Empty;
                worldPresetTextBox.Text = string.Empty;
                combatDifficultyComboBox.SelectedItem = null;
                sharedQuestsCheckBox.Checked = false;
                immersiveExploreCheckBox.Checked = false;
                mobHealthNumeric.Value = ClampNumeric(1, mobHealthNumeric);
                mobDamageNumeric.Value = ClampNumeric(1, mobDamageNumeric);
                shipHealthNumeric.Value = ClampNumeric(1, shipHealthNumeric);
                shipDamageNumeric.Value = ClampNumeric(1, shipDamageNumeric);
                boardingNumeric.Value = ClampNumeric(1, boardingNumeric);
                coopStatsNumeric.Value = ClampNumeric(1, coopStatsNumeric);
                coopShipStatsNumeric.Value = ClampNumeric(0, coopShipStatsNumeric);
                isBinding = false;
                return;
            }

            worldNameTextBox.Text = world.WorldName;
            worldPresetTextBox.Text = WindroseRepository.DetectPreset(world);
            combatDifficultyComboBox.SelectedItem = ConvertCombatTagToDisplay(world.CombatDifficulty);
            sharedQuestsCheckBox.Checked = world.SharedQuests;
            immersiveExploreCheckBox.Checked = world.EasyExplore;
            mobHealthNumeric.Value = ClampNumeric(world.MobHealthMultiplier, mobHealthNumeric);
            mobDamageNumeric.Value = ClampNumeric(world.MobDamageMultiplier, mobDamageNumeric);
            shipHealthNumeric.Value = ClampNumeric(world.ShipsHealthMultiplier, shipHealthNumeric);
            shipDamageNumeric.Value = ClampNumeric(world.ShipsDamageMultiplier, shipDamageNumeric);
            boardingNumeric.Value = ClampNumeric(world.BoardingDifficultyMultiplier, boardingNumeric);
            coopStatsNumeric.Value = ClampNumeric(world.CoopStatsCorrectionModifier, coopStatsNumeric);
            coopShipStatsNumeric.Value = ClampNumeric(world.CoopShipStatsCorrectionModifier, coopShipStatsNumeric);
            isBinding = false;
        }

        private void RefreshWarnings()
        {
            warningsListBox.Items.Clear();
            if (currentState == null)
            {
                return;
            }

            var warnings = WindroseRepository.Validate(currentState);
            if (warnings.Count == 0)
            {
                warningsListBox.Items.Add("Everything currently looks consistent.");
                return;
            }

            foreach (var warning in warnings)
            {
                warningsListBox.Items.Add("[" + warning.Severity.ToUpperInvariant() + "] " + warning.Message);
            }
        }

        private void SelectWorldInListView()
        {
            worldsListView.SelectedItems.Clear();
            if (currentState == null || string.IsNullOrEmpty(currentState.SelectedWorldKey))
            {
                return;
            }

            foreach (ListViewItem item in worldsListView.Items)
            {
                var world = item.Tag as WorldEditableState;
                if (world != null && world.Key == currentState.SelectedWorldKey)
                {
                    item.Selected = true;
                    item.Focused = true;
                    item.EnsureVisible();
                    break;
                }
            }
        }

        private void SetSelectedWorldActive()
        {
            var world = GetSelectedWorld();
            if (world == null || currentState == null)
            {
                return;
            }

            if (currentState.Server.WorldIslandId == world.FolderName)
            {
                return;
            }

            currentState.Server.WorldIslandId = world.FolderName;
            activeWorldIdTextBox.Text = world.FolderName;
            RefreshWorldList();
            RefreshWarnings();
            MarkDirty();
        }
    }
}
