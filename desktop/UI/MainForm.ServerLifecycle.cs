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
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace WindroseServerManager.Desktop
{
    internal sealed partial class MainForm : Form
    {
        private void StartServer()
        {
            if (currentLaunchTarget == null)
            {
                SetStatus("No Windrose launch target was found in the loaded server root.", true);
                return;
            }

            if (managedServerProcess != null && !managedServerProcess.HasExited)
            {
                SetStatus("A Windrose server process launched from this app is already running.", true);
                return;
            }

            try
            {
                isServerStarting = true;
                isServerStopping = false;
                isServerReady = false;
                hasActiveServerSession = false;
                serverStartUtc = DateTime.UtcNow;
                lastServerLogUtc = null;
                lastServerOutputUtc = null;
                UpdateProcessUi();

                var startInfo = new ProcessStartInfo();
                if (currentLaunchTarget.Kind == "batch")
                {
                    startInfo.FileName = currentLaunchTarget.Path;
                    startInfo.Arguments = string.Empty;
                    startInfo.WorkingDirectory = currentLaunchTarget.WorkingDirectory;
                    startInfo.UseShellExecute = true;
                    startInfo.CreateNoWindow = false;
                    startInfo.WindowStyle = ProcessWindowStyle.Normal;
                }
                else
                {
                    startInfo.FileName = currentLaunchTarget.Path;
                    startInfo.Arguments = string.Empty;
                    startInfo.WorkingDirectory = currentLaunchTarget.WorkingDirectory;
                    startInfo.UseShellExecute = false;
                    startInfo.RedirectStandardOutput = true;
                    startInfo.RedirectStandardError = true;
                    startInfo.CreateNoWindow = false;
                    startInfo.WindowStyle = ProcessWindowStyle.Normal;
                }

                managedServerProcess = new Process();
                managedServerProcess.StartInfo = startInfo;
                managedServerProcess.EnableRaisingEvents = true;
                managedServerProcess.Exited += delegate
                {
                    AppendLog("Server process exited.");
                    BeginInvoke((MethodInvoker)delegate
                    {
                        if (!IsServerStillRunning())
                        {
                            isServerStarting = false;
                            isServerStopping = false;
                            isServerReady = false;
                            serverStartUtc = null;
                            lastServerLogUtc = null;
                            lastServerOutputUtc = null;
                        }
                        UpdateProcessUi();
                    });
                };

                if (!managedServerProcess.Start())
                {
                    throw new InvalidOperationException("The server process could not be started.");
                }
                hasActiveServerSession = true;
                if (!startInfo.UseShellExecute)
                {
                    managedServerProcess.OutputDataReceived += delegate(object sender, DataReceivedEventArgs args)
                    {
                        if (!string.IsNullOrEmpty(args.Data))
                        {
                            lastServerOutputUtc = DateTime.UtcNow;
                            ConsiderMarkingServerReady(args.Data);
                            AppendLog(args.Data);
                        }
                    };
                    managedServerProcess.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs args)
                    {
                        if (!string.IsNullOrEmpty(args.Data))
                        {
                            lastServerOutputUtc = DateTime.UtcNow;
                            ConsiderMarkingServerReady(args.Data);
                            AppendLog("[stderr] " + args.Data);
                        }
                    };
                    managedServerProcess.BeginOutputReadLine();
                    managedServerProcess.BeginErrorReadLine();
                }

                AppendLog("Started server using " + currentLaunchTarget.DisplayName);
                StartServerFileLogTail(currentState != null ? currentState.ServerRoot : installDirTextBox.Text.Trim());
                SetStatus("Server process started from the app.", false);
                UpdateProcessUi();
            }
            catch (Exception ex)
            {
                isServerStarting = false;
                isServerStopping = false;
                isServerReady = false;
                hasActiveServerSession = false;
                serverStartUtc = null;
                lastServerLogUtc = null;
                lastServerOutputUtc = null;
                UpdateProcessUi();
                AppendLog("Start failed: " + ex.Message);
                SetStatus(ex.Message, true);
            }
        }

        private void StopServer()
        {
            var runningProcess = ResolveRunningServerProcess();
            if (runningProcess == null)
            {
                SetStatus("No running Windrose process launched from this app was found.", true);
                UpdateProcessUi();
                return;
            }

            try
            {
                isServerStopping = true;
                hasActiveServerSession = false;
                UpdateProcessUi();
                managedServerProcess = runningProcess;
                var pid = runningProcess.Id;
                AppendLog("Stopping server process for PID " + pid + "...");

                var stoppedGracefully = false;
                if (!runningProcess.HasExited)
                {
                    try
                    {
                        if (runningProcess.CloseMainWindow())
                        {
                            stoppedGracefully = runningProcess.WaitForExit(8000);
                        }
                    }
                    catch
                    {
                    }
                }

                if (!stoppedGracefully && !runningProcess.HasExited)
                {
                    AppendLog("Graceful stop did not complete. Terminating process...");
                    runningProcess.Kill();
                    runningProcess.WaitForExit(8000);
                }

                if (!runningProcess.HasExited)
                {
                    throw new InvalidOperationException("The server process did not exit after the stop request.");
                }

                if (managedServerProcess != null)
                {
                    try
                    {
                        managedServerProcess.Refresh();
                    }
                    catch
                    {
                    }
                }

                if (stoppedGracefully)
                {
                    AppendLog("Server stopped gracefully.");
                }
                else
                {
                    AppendLog("Server process terminated.");
                }

                SetStatus("Stop signal sent to the server process.", false);
            }
            catch (Exception ex)
            {
                AppendLog("Stop failed: " + ex.Message);
                SetStatus(ex.Message, true);
            }
            finally
            {
                isServerStopping = false;
                if (!IsServerStillRunning())
                {
                    isServerReady = false;
                    hasActiveServerSession = false;
                    serverStartUtc = null;
                    lastServerLogUtc = null;
                    lastServerOutputUtc = null;
                }
                UpdateProcessUi();
            }
        }

        private void RestartServer()
        {
            StopServer();
            StartServer();
        }

        private void OpenLoadedRoot()
        {
            if (currentState == null || string.IsNullOrEmpty(currentState.ServerRoot) || !Directory.Exists(currentState.ServerRoot))
            {
                SetStatus("No loaded server root is available to open.", true);
                return;
            }

            try
            {
                OpenPathInShell(currentState.ServerRoot);
            }
            catch (Exception ex)
            {
                currentState = null;
                currentLaunchTarget = null;
                UpdateLaunchTargetUi();
                UpdateProcessUi();
                SetStatus(ex.Message, true);
            }
        }

        private void OnTabsSelecting(object sender, TabControlCancelEventArgs e)
        {
            if (!isDirty)
            {
                return;
            }

            var result = MessageBox.Show(
                this,
                "You have unsaved changes. Save before switching tabs?",
                AppTitle,
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }

            if (result == DialogResult.Yes)
            {
                SaveState();
                if (isDirty)
                {
                    e.Cancel = true;
                }
            }
        }

        private void UpdateLaunchTargetUi()
        {
            if (currentLaunchTarget == null)
            {
                launchTargetLabel.Text = "Launch target: not detected";
            }
            else
            {
                launchTargetLabel.Text = "Launch target: " + currentLaunchTarget.DisplayName;
            }
        }

        private void UpdateProcessUi()
        {
            var runningProcess = ResolveRunningServerProcess();
            var running = runningProcess != null;
            var recentLogActivity = lastServerLogUtc.HasValue && DateTime.UtcNow - lastServerLogUtc.Value <= TimeSpan.FromSeconds(8);
            var recentOutputActivity = lastServerOutputUtc.HasValue && DateTime.UtcNow - lastServerOutputUtc.Value <= TimeSpan.FromSeconds(8);
            var runningSignal = running || (hasActiveServerSession && (recentLogActivity || recentOutputActivity));
            var withinStartupWindow = isServerStarting && serverStartUtc.HasValue && DateTime.UtcNow - serverStartUtc.Value < TimeSpan.FromSeconds(8);
            var taskRunning = managedTaskProcess != null && !managedTaskProcess.HasExited;
            var provisioningBusy = taskRunning || isAutomaticProvisioning;

            if (running)
            {
                managedServerProcess = runningProcess;
            }
            else if (managedServerProcess != null && managedServerProcess.HasExited)
            {
                managedServerProcess = null;
                if (!runningSignal)
                {
                    isServerStarting = false;
                    isServerReady = false;
                    hasActiveServerSession = false;
                    serverStartUtc = null;
                    lastServerLogUtc = null;
                    lastServerOutputUtc = null;
                }
            }

            if (!withinStartupWindow && runningSignal)
            {
                isServerStarting = false;
                isServerReady = true;
            }

            processStatusLabel.Text = running
                ? "Process status: running (PID " + runningProcess.Id + ")"
                : (((hasActiveServerSession && (recentLogActivity || recentOutputActivity)) ? "Process status: running (server activity detected)" : (provisioningBusy ? "Process status: provisioning task running..." : "Process status: idle")));

            if (isServerStopping)
            {
                SetServerStateIndicator("Stopping", currentThemeColors.ServerStateStarting);
            }
            else if (running)
            {
                SetServerStateIndicator("Running", currentThemeColors.ServerStateRunning);
            }
            else if (withinStartupWindow)
            {
                SetServerStateIndicator("Starting", currentThemeColors.ServerStateStarting);
            }
            else if (runningSignal)
            {
                SetServerStateIndicator("Running", currentThemeColors.ServerStateRunning);
            }
            else
            {
                SetServerStateIndicator("Stopped", currentThemeColors.ServerStateStopped);
            }

            provisioningProgressBar.Visible = provisioningBusy;

            loadButton.Enabled = !provisioningBusy;
            browseButton.Enabled = !provisioningBusy;
            startServerButton.Enabled = currentLaunchTarget != null && !running && !provisioningBusy;
            stopServerButton.Enabled = running && !provisioningBusy;
            restartServerButton.Enabled = currentLaunchTarget != null && !provisioningBusy;
            openRootButton.Enabled = currentState != null && Directory.Exists(currentState.ServerRoot) && !provisioningBusy;
            openInstallDirButton.Enabled = !string.IsNullOrWhiteSpace(installDirTextBox.Text) && !provisioningBusy;
            chooseInstallFolderButton.Enabled = !provisioningBusy;
            browseInstallDirButton.Enabled = !provisioningBusy;
            installDirTextBox.Enabled = !provisioningBusy;
            steamCmdPathTextBox.Enabled = !provisioningBusy;
            installServerButton.Enabled = !provisioningBusy;
            updateServerButton.Enabled = !provisioningBusy;
            installSteamCmdButton.Enabled = !provisioningBusy;
            deleteServerButton.Enabled = !provisioningBusy && !(managedServerProcess != null && !managedServerProcess.HasExited);
            backupServerButton.Enabled = !provisioningBusy && currentState != null && Directory.Exists(currentState.ServerRoot);
            restoreFullServerButton.Enabled = !provisioningBusy && !running && currentState != null && Directory.Exists(currentState.ServerRoot);
            backupCaptainSettingsButton.Enabled = !provisioningBusy && currentState != null;
            restoreCaptainSettingsButton.Enabled = !provisioningBusy && !running && currentState != null;
            backupWorldSettingsButton.Enabled = !provisioningBusy && GetSelectedWorld() != null;
            restoreWorldSettingsButton.Enabled = !provisioningBusy && !running && GetSelectedWorld() != null;
            browseBackupFolderButton.Enabled = !provisioningBusy;
            backupFolderTextBox.Enabled = !provisioningBusy;
            scheduledBackupEnabledCheckBox.Enabled = !provisioningBusy;
            scheduledBackupIntervalNumeric.Enabled = !provisioningBusy;
            scheduledBackupTypeComboBox.Enabled = !provisioningBusy;
            scheduledBackupRetentionNumeric.Enabled = !provisioningBusy;
            zipFullBackupsCheckBox.Enabled = !provisioningBusy;
            scheduledRebootEnabledCheckBox.Enabled = !provisioningBusy;
            scheduledRebootDatePicker.Enabled = !provisioningBusy;
            scheduledRebootTimePicker.Enabled = !provisioningBusy;
            recurringRebootCheckBox.Enabled = !provisioningBusy;
            recurringRebootIntervalNumeric.Enabled = !provisioningBusy;
            recurringRebootIntervalUnitComboBox.Enabled = !provisioningBusy;
            rebootNowButton.Enabled = !provisioningBusy && running;
            createNewWorldButton.Enabled = !provisioningBusy && !running && currentState != null;
            importWorldButton.Enabled = !provisioningBusy && !running && currentState != null;
            deleteWorldButton.Enabled = !provisioningBusy && !running && currentState != null && GetSelectedWorld() != null;
            openModsFolderButton.Enabled = !string.IsNullOrEmpty(GetModsRootPath());
            importModFolderButton.Enabled = !string.IsNullOrEmpty(GetModsRootPath());
            modsProviderComboBox.Enabled = !provisioningBusy;
            modsApiKeyTextBox.Enabled = !provisioningBusy;
            saveModsApiKeyButton.Enabled = !provisioningBusy;
            searchCurseForgeButton.Enabled = !provisioningBusy && SelectedModsProviderSupportsSearch();
            installSelectedCurseForgeButton.Enabled = !provisioningBusy && SelectedModsProviderSupportsInstall() && availableModsListView.SelectedItems.Count > 0 && !string.IsNullOrWhiteSpace(GetModsRootPath());
            refreshInstalledModsButton.Enabled = !provisioningBusy;
            enableModButton.Enabled = !provisioningBusy && installedModsListView.SelectedItems.Count > 0;
            disableModButton.Enabled = !provisioningBusy && installedModsListView.SelectedItems.Count > 0;
            updateModButton.Enabled = !provisioningBusy && SelectedModsProviderSupportsUpdates() && installedModsListView.SelectedItems.Count > 0;
            removeModButton.Enabled = !provisioningBusy && installedModsListView.SelectedItems.Count > 0;
            curseForgeSearchModeComboBox.Enabled = !provisioningBusy && SelectedModsProviderSupportsSearch();
            curseForgeSearchTextBox.Enabled = !provisioningBusy && SelectedModsProviderSupportsSearch();
            var hasServerRoot = !string.IsNullOrWhiteSpace(GetCurrentServerRoot());
            var hasRconSettings = !string.IsNullOrWhiteSpace(GetRconSettingsPath()) && File.Exists(GetRconSettingsPath());
            var hasInstalledRconDll = !string.IsNullOrWhiteSpace(GetRconVersionDllPath()) && File.Exists(GetRconVersionDllPath());
            var hasSelectedRconAccountId = !string.IsNullOrWhiteSpace(GetSelectedRconAccountId());
            browseRconDllButton.Enabled = !provisioningBusy;
            installRconButton.Enabled = !provisioningBusy && hasServerRoot && !string.IsNullOrWhiteSpace(FindLocalRconVersionDllSource());
            uninstallRconButton.Enabled = !provisioningBusy && hasServerRoot && (hasInstalledRconDll || hasRconSettings);
            saveRconSettingsButton.Enabled = !provisioningBusy && hasServerRoot;
            testRconButton.Enabled = !provisioningBusy && hasRconSettings;
            refreshRconPlayersButton.Enabled = !provisioningBusy && hasRconSettings;
            rconHelpButton.Enabled = !provisioningBusy && hasRconSettings;
            rconInfoButton.Enabled = !provisioningBusy && hasRconSettings;
            rconShowPlayersButton.Enabled = !provisioningBusy && hasRconSettings;
            rconPlayerInfoButton.Enabled = !provisioningBusy && hasRconSettings && hasSelectedRconAccountId;
            rconGetPosButton.Enabled = !provisioningBusy && hasRconSettings && hasSelectedRconAccountId;
            rconKickButton.Enabled = !provisioningBusy && hasRconSettings && hasSelectedRconAccountId;
            rconBanButton.Enabled = !provisioningBusy && hasRconSettings && hasSelectedRconAccountId;
            rconUnbanButton.Enabled = !provisioningBusy && hasRconSettings && hasSelectedRconAccountId;
            rconBanListButton.Enabled = !provisioningBusy && hasRconSettings;
            rconDllPathTextBox.Enabled = false;
            rconSelectedAccountIdTextBox.Enabled = !provisioningBusy && hasRconSettings;
            rconBanReasonTextBox.Enabled = !provisioningBusy && hasRconSettings;
            rconBindAddressTextBox.Enabled = !provisioningBusy && hasServerRoot;
            rconPortNumeric.Enabled = !provisioningBusy && hasServerRoot;
            rconPasswordTextBox.Enabled = !provisioningBusy && hasServerRoot;
            rconAllowedIpsTextBox.Enabled = !provisioningBusy && hasServerRoot;
            rconMaxFailedAttemptsNumeric.Enabled = !provisioningBusy && hasServerRoot;
            rconTimeoutNumeric.Enabled = !provisioningBusy && hasServerRoot;
            rconEnableLoggingCheckBox.Enabled = !provisioningBusy && hasServerRoot;
            rconSecureEnabledCheckBox.Enabled = !provisioningBusy && hasServerRoot;
            rconAesKeyTextBox.Enabled = !provisioningBusy && hasServerRoot;
            viewRconLicenseButton.Enabled = true;
            SchedulePlayerCountRefresh(runningSignal);
            RefreshRconStatusUi();
        }

        private void SetServerStateIndicator(string stateText, Color color)
        {
            serverStateDotPanel.BackColor = color;
            serverStateDotPanel.Invalidate();
            serverStateValueLabel.Text = stateText;
        }

        private Process ResolveRunningServerProcess()
        {
            if (managedServerProcess != null)
            {
                try
                {
                    if (!managedServerProcess.HasExited && IsLikelyServerProcess(managedServerProcess))
                    {
                        return managedServerProcess;
                    }
                }
                catch
                {
                }
            }

            var serverRoot = currentState != null && !string.IsNullOrWhiteSpace(currentState.ServerRoot)
                ? currentState.ServerRoot
                : installDirTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(serverRoot) || !Directory.Exists(serverRoot))
            {
                return null;
            }

            foreach (var candidatePath in GetServerExecutableCandidates(serverRoot))
            {
                var process = FindProcessByExecutablePath(candidatePath);
                if (process != null)
                {
                    return process;
                }
            }

            return null;
        }

        private bool IsServerStillRunning()
        {
            return ResolveRunningServerProcess() != null;
        }

        private static IEnumerable<string> GetServerExecutableCandidates(string serverRoot)
        {
            var candidates = new[]
            {
                Path.Combine(serverRoot, "WindroseServer.exe"),
                Path.Combine(serverRoot, "R5", "Binaries", "Win64", "WindroseServer-Win64-Shipping.exe")
            };

            return candidates.Where(File.Exists);
        }

        private static Process FindProcessByExecutablePath(string executablePath)
        {
            var processName = Path.GetFileNameWithoutExtension(executablePath);
            foreach (var process in Process.GetProcessesByName(processName))
            {
                try
                {
                    var mainModule = process.MainModule;
                    if (mainModule != null && string.Equals(mainModule.FileName, executablePath, StringComparison.OrdinalIgnoreCase))
                    {
                        return process;
                    }
                }
                catch
                {
                    // Ignore processes that deny module inspection.
                }
            }

            return null;
        }

        private static bool IsLikelyServerProcess(Process process)
        {
            try
            {
                var name = process.ProcessName;
                return string.Equals(name, "WindroseServer", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(name, "WindroseServer-Win64-Shipping", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private void LoadPreferencesIntoUi()
        {
            try
            {
                if (!File.Exists(PreferencesPath))
                {
                    return;
                }

                var serializer = new JavaScriptSerializer();
                var json = File.ReadAllText(PreferencesPath);
                var prefs = serializer.Deserialize<ManagerPreferences>(json);
                if (prefs == null)
                {
                    return;
                }

                if (!string.IsNullOrWhiteSpace(prefs.LastServerRoot))
                {
                    pathTextBox.Text = prefs.LastServerRoot;
                }

                if (!string.IsNullOrWhiteSpace(prefs.LastInstallDir))
                {
                    installDirTextBox.Text = prefs.LastInstallDir;
                }

                if (!string.IsNullOrWhiteSpace(prefs.LastSteamCmdDir))
                {
                    steamCmdPathTextBox.Text = prefs.LastSteamCmdDir;
                }

                if (!string.IsNullOrWhiteSpace(prefs.LastBackupDir))
                {
                    backupFolderTextBox.Text = prefs.LastBackupDir;
                }

                storedCurseForgeApiKey = prefs.CurseForgeApiKey ?? string.Empty;
                storedNexusModsApiKey = prefs.NexusModsApiKey ?? string.Empty;
                selectedModsProvider = string.Equals(prefs.ModsProvider, ModProviderNexusMods, StringComparison.OrdinalIgnoreCase)
                    ? ModProviderNexusMods
                    : ModProviderCurseForge;
                modsProviderComboBox.SelectedItem = selectedModsProvider;
                modsApiKeyTextBox.Text = GetStoredApiKeyForProvider(selectedModsProvider);

                if (prefs.ScheduledBackupIntervalHours > 0)
                {
                    scheduledBackupIntervalNumeric.Value = Math.Min(720, Math.Max(1, prefs.ScheduledBackupIntervalHours));
                }

                if (!string.IsNullOrWhiteSpace(prefs.ScheduledBackupType) && scheduledBackupTypeComboBox.Items.Contains(prefs.ScheduledBackupType))
                {
                    scheduledBackupTypeComboBox.SelectedItem = prefs.ScheduledBackupType;
                }

                if (prefs.ScheduledFullBackupRetentionCount > 0)
                {
                    scheduledBackupRetentionNumeric.Value = Math.Min(100, Math.Max(1, prefs.ScheduledFullBackupRetentionCount));
                }

                zipFullBackupsCheckBox.Checked = prefs.ZipFullServerBackups;

                if (!string.IsNullOrWhiteSpace(prefs.NextScheduledBackupUtc))
                {
                    DateTime parsedNext;
                    if (DateTime.TryParse(prefs.NextScheduledBackupUtc, null, System.Globalization.DateTimeStyles.RoundtripKind, out parsedNext))
                    {
                        nextScheduledBackupUtc = parsedNext;
                        if (nextScheduledBackupUtc <= DateTime.UtcNow)
                        {
                            nextScheduledBackupUtc = DateTime.UtcNow.AddHours(Decimal.ToInt32(scheduledBackupIntervalNumeric.Value));
                        }
                    }
                }

                scheduledBackupEnabledCheckBox.Checked = prefs.ScheduledBackupEnabled;

                if (prefs.ScheduledBackupEnabled)
                {
                    scheduledBackupTimer.Start();
                }

                UpdateScheduledBackupNextLabel();

                if (prefs.ScheduledRebootEveryValue > 0)
                {
                    recurringRebootIntervalNumeric.Value = Math.Min(365, Math.Max(1, prefs.ScheduledRebootEveryValue));
                }

                if (!string.IsNullOrWhiteSpace(prefs.ScheduledRebootEveryUnit) && recurringRebootIntervalUnitComboBox.Items.Contains(prefs.ScheduledRebootEveryUnit))
                {
                    recurringRebootIntervalUnitComboBox.SelectedItem = prefs.ScheduledRebootEveryUnit;
                }

                recurringRebootCheckBox.Checked = prefs.ScheduledRebootRecurring;

                if (!string.IsNullOrWhiteSpace(prefs.ScheduledRebootStartLocal))
                {
                    DateTime parsedStartLocal;
                    if (DateTime.TryParse(prefs.ScheduledRebootStartLocal, out parsedStartLocal))
                    {
                        scheduledRebootDatePicker.Value = parsedStartLocal.Date;
                        scheduledRebootTimePicker.Value = DateTime.Today.Add(parsedStartLocal.TimeOfDay);
                    }
                }

                if (!string.IsNullOrWhiteSpace(prefs.NextScheduledRebootUtc))
                {
                    DateTime parsedNextReboot;
                    if (DateTime.TryParse(prefs.NextScheduledRebootUtc, null, System.Globalization.DateTimeStyles.RoundtripKind, out parsedNextReboot))
                    {
                        nextScheduledRebootUtc = parsedNextReboot;
                    }
                }

                scheduledRebootEnabledCheckBox.Checked = prefs.ScheduledRebootEnabled;
                if (prefs.ScheduledRebootEnabled)
                {
                    scheduledRebootTimer.Start();
                }

                UpdateScheduledRebootNextLabel();

                var savedTheme = prefs.LastTheme;
                if (string.Equals(savedTheme, "Pirate", StringComparison.OrdinalIgnoreCase))
                {
                    savedTheme = "Windrose";
                }

                if (!string.IsNullOrWhiteSpace(savedTheme) && themeComboBox.Items.Contains(savedTheme))
                {
                    currentThemeName = savedTheme;
                    themeComboBox.SelectedItem = savedTheme;
                    ApplyTheme();
                }

                RefreshModsProviderUi();
                ApplySavedWindowLayout(prefs);
            }
            catch
            {
                // Keep defaults if preferences fail to load.
            }
        }

        private void SavePreferences()
        {
            try
            {
                var prefsDir = Path.GetDirectoryName(PreferencesPath);
                if (string.IsNullOrWhiteSpace(prefsDir))
                {
                    return;
                }

                Directory.CreateDirectory(prefsDir);
                var serializer = new JavaScriptSerializer();
                var boundsToSave = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
                var prefs = new ManagerPreferences
                {
                    LastServerRoot = currentState != null && !string.IsNullOrWhiteSpace(currentState.ServerRoot)
                        ? currentState.ServerRoot
                        : pathTextBox.Text.Trim(),
                    LastInstallDir = installDirTextBox.Text.Trim(),
                    LastSteamCmdDir = steamCmdPathTextBox.Text.Trim(),
                    LastBackupDir = backupFolderTextBox.Text.Trim(),
                    LastTheme = currentThemeName,
                    ModsProvider = GetSelectedModsProvider(),
                    CurseForgeApiKey = storedCurseForgeApiKey,
                    NexusModsApiKey = storedNexusModsApiKey,
                    ScheduledBackupEnabled = scheduledBackupEnabledCheckBox.Checked,
                    ScheduledBackupIntervalHours = Decimal.ToInt32(scheduledBackupIntervalNumeric.Value),
                    ScheduledBackupType = scheduledBackupTypeComboBox.SelectedItem != null ? scheduledBackupTypeComboBox.SelectedItem.ToString() : "Captain + World Settings",
                    NextScheduledBackupUtc = nextScheduledBackupUtc.HasValue ? nextScheduledBackupUtc.Value.ToString("o") : string.Empty,
                    ScheduledFullBackupRetentionCount = Decimal.ToInt32(scheduledBackupRetentionNumeric.Value),
                    ZipFullServerBackups = zipFullBackupsCheckBox.Checked,
                    ScheduledRebootEnabled = scheduledRebootEnabledCheckBox.Checked,
                    ScheduledRebootRecurring = recurringRebootCheckBox.Checked,
                    ScheduledRebootEveryValue = Decimal.ToInt32(recurringRebootIntervalNumeric.Value),
                    ScheduledRebootEveryUnit = recurringRebootIntervalUnitComboBox.SelectedItem != null ? recurringRebootIntervalUnitComboBox.SelectedItem.ToString() : "Days",
                    ScheduledRebootStartLocal = (scheduledRebootDatePicker.Value.Date + scheduledRebootTimePicker.Value.TimeOfDay).ToString("o"),
                    NextScheduledRebootUtc = nextScheduledRebootUtc.HasValue ? nextScheduledRebootUtc.Value.ToString("o") : string.Empty,
                    WindowX = boundsToSave.X,
                    WindowY = boundsToSave.Y,
                    WindowWidth = boundsToSave.Width,
                    WindowHeight = boundsToSave.Height,
                    WindowState = WindowState == FormWindowState.Maximized ? "Maximized" : "Normal"
                };
                File.WriteAllText(PreferencesPath, serializer.Serialize(prefs));
            }
            catch
            {
                // Do not interrupt normal use if preferences cannot be saved.
            }
        }

        private void ApplySavedWindowLayout(ManagerPreferences prefs)
        {
            if (prefs == null)
            {
                return;
            }

            if (prefs.WindowWidth < MinimumSize.Width || prefs.WindowHeight < MinimumSize.Height)
            {
                return;
            }

            var savedBounds = new Rectangle(prefs.WindowX, prefs.WindowY, prefs.WindowWidth, prefs.WindowHeight);
            if (!IsReasonableSavedWindowBounds(savedBounds))
            {
                return;
            }

            StartPosition = FormStartPosition.Manual;
            Bounds = savedBounds;

            if (string.Equals(prefs.WindowState, "Maximized", StringComparison.OrdinalIgnoreCase))
            {
                WindowState = FormWindowState.Maximized;
            }
            else
            {
                WindowState = FormWindowState.Normal;
            }
        }

        private static bool IsReasonableSavedWindowBounds(Rectangle bounds)
        {
            if (bounds.Width < 100 || bounds.Height < 100)
            {
                return false;
            }

            foreach (var screen in Screen.AllScreens)
            {
                var inflatedWorkingArea = screen.WorkingArea;
                inflatedWorkingArea.Inflate(80, 80);
                if (inflatedWorkingArea.IntersectsWith(bounds))
                {
                    return true;
                }
            }

            return false;
        }


        private void TryAutoLoadLastServer()
        {
            var candidates = new[]
            {
                pathTextBox.Text.Trim(),
                installDirTextBox.Text.Trim(),
                DefaultServerDir
            };

            foreach (var candidate in candidates.Where(delegate(string item)
            {
                return !string.IsNullOrWhiteSpace(item);
            }).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!Directory.Exists(candidate))
                {
                    continue;
                }

                var hasServerConfig = File.Exists(Path.Combine(candidate, "R5", "ServerDescription.json"))
                    || File.Exists(Path.Combine(candidate, "ServerDescription.json"));
                if (!hasServerConfig)
                {
                    continue;
                }

                LoadServer(candidate);
                if (currentState != null)
                {
                    return;
                }
            }
        }

        private void StartServerFileLogTail(string rootPath)
        {
            if (string.IsNullOrWhiteSpace(rootPath))
            {
                return;
            }

            var normalized = Path.GetFullPath(rootPath.Trim());
            if (string.Equals(fileLogRoot, normalized, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            fileLogRoot = normalized;
            fileLogPath = null;
            fileLogPosition = 0;
            AppendLog("Watching server file logs under " + Path.Combine(normalized, "R5", "Saved", "Logs"));
        }

        private void PollServerFileLog()
        {
            if (string.IsNullOrWhiteSpace(fileLogRoot))
            {
                return;
            }

            var logsDir = Path.Combine(fileLogRoot, "R5", "Saved", "Logs");
            if (!Directory.Exists(logsDir))
            {
                return;
            }

            string latestLogPath;
            try
            {
                latestLogPath = Directory
                    .GetFiles(logsDir, "*.log")
                    .OrderByDescending(delegate(string item) { return File.GetLastWriteTime(item); })
                    .FirstOrDefault();
            }
            catch
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(latestLogPath))
            {
                return;
            }

            if (!string.Equals(fileLogPath, latestLogPath, StringComparison.OrdinalIgnoreCase))
            {
                fileLogPath = latestLogPath;
                try
                {
                    var length = new FileInfo(fileLogPath).Length;
                    fileLogPosition = Math.Max(0, length - 65536);
                }
                catch
                {
                    fileLogPosition = 0;
                }
                AppendLog("Switched file log source to " + Path.GetFileName(fileLogPath));
            }

            try
            {
                using (var stream = new FileStream(fileLogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    if (stream.Length < fileLogPosition)
                    {
                        fileLogPosition = 0;
                    }

                    if (stream.Length == fileLogPosition)
                    {
                        return;
                    }

                    stream.Seek(fileLogPosition, SeekOrigin.Begin);
                    using (var reader = new StreamReader(stream))
                    {
                        var newContent = reader.ReadToEnd();
                        fileLogPosition = stream.Position;
                        if (string.IsNullOrWhiteSpace(newContent))
                        {
                            return;
                        }

                        var lines = newContent
                            .Replace("\r\n", "\n")
                            .Split(new[] { '\n' }, StringSplitOptions.None);

                        foreach (var line in lines)
                        {
                            if (string.IsNullOrWhiteSpace(line))
                            {
                                continue;
                            }

                            lastServerLogUtc = DateTime.UtcNow;
                            ConsiderMarkingServerReady(line);
                            AppendLog("[server-file] " + line);
                        }
                    }
                }
            }
            catch
            {
                // Ignore transient file read exceptions while the game is writing logs.
            }
        }

        private void ConsiderMarkingServerReady(string logLine)
        {
            if (!isServerStarting || isServerReady || string.IsNullOrWhiteSpace(logLine))
            {
                return;
            }

            var normalized = logLine.Trim();
            if (serverStartUtc.HasValue && DateTime.UtcNow - serverStartUtc.Value >= TimeSpan.FromSeconds(4))
            {
                isServerStarting = false;
                isServerReady = true;
                BeginInvoke((MethodInvoker)delegate { UpdateProcessUi(); });
                return;
            }

            if (normalized.IndexOf("listening", StringComparison.OrdinalIgnoreCase) >= 0
                || normalized.IndexOf("server started", StringComparison.OrdinalIgnoreCase) >= 0
                || normalized.IndexOf("startup complete", StringComparison.OrdinalIgnoreCase) >= 0
                || normalized.IndexOf("game engine initialized", StringComparison.OrdinalIgnoreCase) >= 0
                || normalized.IndexOf("initialized", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                isServerStarting = false;
                isServerReady = true;
                BeginInvoke((MethodInvoker)delegate { UpdateProcessUi(); });
            }
        }

        private LaunchTarget DetectLaunchTarget(string serverRoot)
        {
            if (string.IsNullOrEmpty(serverRoot))
            {
                return null;
            }

            var batchForegroundPath = Path.Combine(serverRoot, "StartServerForeground.bat");
            if (File.Exists(batchForegroundPath))
            {
                return new LaunchTarget(batchForegroundPath, "batch");
            }

            var batchPath = Path.Combine(serverRoot, "StartServer.bat");
            if (File.Exists(batchPath))
            {
                return new LaunchTarget(batchPath, "batch");
            }

            var windroseExePath = Path.Combine(serverRoot, "WindroseServer.exe");
            if (File.Exists(windroseExePath))
            {
                return new LaunchTarget(windroseExePath, "exe");
            }

            var r5ShippingExePath = Path.Combine(serverRoot, "R5", "Binaries", "Win64", "R5Server-Win64-Shipping.exe");
            if (File.Exists(r5ShippingExePath))
            {
                return new LaunchTarget(r5ShippingExePath, "exe");
            }

            var windroseShippingExePath = Path.Combine(serverRoot, "R5", "Binaries", "Win64", "WindroseServer-Win64-Shipping.exe");
            if (File.Exists(windroseShippingExePath))
            {
                return new LaunchTarget(windroseShippingExePath, "exe");
            }

            return null;
        }

        private void SchedulePlayerCountRefresh(bool serverLikelyRunning)
        {
            var hasInstalledRcon = !string.IsNullOrWhiteSpace(GetRconVersionDllPath()) && File.Exists(GetRconVersionDllPath())
                && !string.IsNullOrWhiteSpace(GetRconSettingsPath()) && File.Exists(GetRconSettingsPath());
            playerCountTextLabel.Visible = hasInstalledRcon;
            playerCountValueLabel.Visible = hasInstalledRcon;
            if (!hasInstalledRcon)
            {
                lastKnownPlayerCount = "offline";
                playerCountValueLabel.Text = lastKnownPlayerCount;
                return;
            }

            if (!serverLikelyRunning)
            {
                lastKnownPlayerCount = "offline";
                playerCountValueLabel.Text = lastKnownPlayerCount;
                return;
            }

            playerCountValueLabel.Text = lastKnownPlayerCount;
            if (playerCountRefreshBusy)
            {
                return;
            }

            if (lastPlayerCountRefreshUtc.HasValue && DateTime.UtcNow - lastPlayerCountRefreshUtc.Value < TimeSpan.FromSeconds(5))
            {
                return;
            }

            playerCountRefreshBusy = true;
            lastPlayerCountRefreshUtc = DateTime.UtcNow;
            var settings = LoadRconSettingsFromDisk();
            if (settings == null)
            {
                playerCountRefreshBusy = false;
                lastKnownPlayerCount = "offline";
                playerCountValueLabel.Text = lastKnownPlayerCount;
                return;
            }

            ThreadPool.QueueUserWorkItem(delegate
            {
                var result = QueryPlayerCountDisplay(settings);
                BeginInvoke((MethodInvoker)delegate
                {
                    lastKnownPlayerCount = string.IsNullOrWhiteSpace(result) ? "unknown" : result;
                    playerCountValueLabel.Text = lastKnownPlayerCount;
                    playerCountRefreshBusy = false;
                });
            });
        }

        private void OnSelectedRconPlayerChanged()
        {
            if (rconPlayersListView.SelectedItems.Count == 0)
            {
                UpdateProcessUi();
                return;
            }

            var accountId = rconPlayersListView.SelectedItems[0].SubItems.Count > 1
                ? rconPlayersListView.SelectedItems[0].SubItems[1].Text
                : string.Empty;
            if (!string.IsNullOrWhiteSpace(accountId))
            {
                rconSelectedAccountIdTextBox.Text = accountId;
            }

            UpdateProcessUi();
        }

        private string GetSelectedRconAccountId()
        {
            return (rconSelectedAccountIdTextBox.Text ?? string.Empty).Trim();
        }

        private void RunNamedRconCommand(string commandName)
        {
            try
            {
                var fullCommand = BuildRconCommand(commandName);
                var body = ExecuteRconCommand(BuildRconSettingsFromUi(), fullCommand);
                rconPlayersTextBox.Text = string.IsNullOrWhiteSpace(body) ? "(No response body)" : body;
                if (string.Equals(commandName, "showplayers", StringComparison.OrdinalIgnoreCase))
                {
                    PopulateRconPlayersList(body);
                }
                SetStatus("RCON command completed: " + commandName, false);
            }
            catch (Exception ex)
            {
                SetStatus("RCON command failed: " + ex.Message, true);
            }
        }

        private string BuildRconCommand(string commandName)
        {
            var accountId = GetSelectedRconAccountId();
            var reason = (rconBanReasonTextBox.Text ?? string.Empty).Trim();
            if (string.Equals(commandName, "kick", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(accountId))
                {
                    throw new InvalidOperationException("Select a player or enter an Account ID first.");
                }
                return "kick " + accountId;
            }

            if (string.Equals(commandName, "ban", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(accountId))
                {
                    throw new InvalidOperationException("Select a player or enter an Account ID first.");
                }
                return string.IsNullOrWhiteSpace(reason)
                    ? ("ban " + accountId)
                    : ("ban " + accountId + " " + reason);
            }

            if (string.Equals(commandName, "unban", StringComparison.OrdinalIgnoreCase)
                || string.Equals(commandName, "playerinfo", StringComparison.OrdinalIgnoreCase)
                || string.Equals(commandName, "getpos", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(accountId))
                {
                    throw new InvalidOperationException("Select a player or enter an Account ID first.");
                }
                return commandName + " " + accountId;
            }

            return commandName;
        }

        private void PopulateRconPlayersList(string responseBody)
        {
            rconPlayersListView.BeginUpdate();
            try
            {
                rconPlayersListView.Items.Clear();
                foreach (var line in SplitRconResponseLines(responseBody))
                {
                    var parsed = TryParseRconPlayerLine(line);
                    if (parsed == null)
                    {
                        continue;
                    }

                    var item = new ListViewItem(parsed.Item1);
                    item.SubItems.Add(parsed.Item2);
                    item.SubItems.Add(parsed.Item3);
                    rconPlayersListView.Items.Add(item);
                }
            }
            finally
            {
                rconPlayersListView.EndUpdate();
            }
        }

        private static IEnumerable<string> SplitRconResponseLines(string responseBody)
        {
            return (responseBody ?? string.Empty)
                .Replace("\r\n", "\n")
                .Split('\n')
                .Select(delegate(string line) { return line == null ? string.Empty : line.Trim(); })
                .Where(delegate(string line) { return !string.IsNullOrWhiteSpace(line); });
        }

        private static Tuple<string, string, string> TryParseRconPlayerLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return null;
            }

            if (line.StartsWith("Players:", StringComparison.OrdinalIgnoreCase)
                || line.StartsWith("Online Players", StringComparison.OrdinalIgnoreCase)
                || line.StartsWith("ID", StringComparison.OrdinalIgnoreCase)
                || line.StartsWith("---", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var accountId = ExtractRconAccountId(line);
            if (string.IsNullOrWhiteSpace(accountId))
            {
                return null;
            }

            var name = line;
            var accountIndex = line.IndexOf(accountId, StringComparison.OrdinalIgnoreCase);
            if (accountIndex > 0)
            {
                name = line.Substring(0, accountIndex).Trim().Trim('-', ':', '|', '(', ')', '[', ']');
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                name = "(Unknown Player)";
            }

            return Tuple.Create(name, accountId, line);
        }

        private static string ExtractRconAccountId(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return string.Empty;
            }

            var labeledMatch = Regex.Match(line, @"(?i)(account\s*id|accountid|id)\s*[:=]\s*([A-Za-z0-9_\-]+)");
            if (labeledMatch.Success)
            {
                return labeledMatch.Groups[2].Value;
            }

            var bracketMatch = Regex.Match(line, @"[\[\(]([A-Za-z0-9_\-]{6,})[\]\)]");
            if (bracketMatch.Success)
            {
                return bracketMatch.Groups[1].Value;
            }

            var tokenMatch = Regex.Match(line, @"\b[A-Za-z0-9_\-]{8,}\b");
            return tokenMatch.Success ? tokenMatch.Value : string.Empty;
        }

        private string GetCurrentServerRoot()
        {
            return currentState != null && !string.IsNullOrWhiteSpace(currentState.ServerRoot)
                ? currentState.ServerRoot
                : installDirTextBox.Text.Trim();
        }

        private string GetRconWin64Directory()
        {
            var serverRoot = GetCurrentServerRoot();
            return string.IsNullOrWhiteSpace(serverRoot)
                ? string.Empty
                : Path.Combine(serverRoot, "R5", "Binaries", "Win64");
        }

        private string GetRconSettingsPath()
        {
            var win64Dir = GetRconWin64Directory();
            return string.IsNullOrWhiteSpace(win64Dir)
                ? string.Empty
                : Path.Combine(win64Dir, "windrosercon", "settings.ini");
        }

        private string GetRconVersionDllPath()
        {
            var win64Dir = GetRconWin64Directory();
            return string.IsNullOrWhiteSpace(win64Dir)
                ? string.Empty
                : Path.Combine(win64Dir, "version.dll");
        }

        private RconSettingsSnapshot LoadRconSettingsFromDisk()
        {
            var settingsPath = GetRconSettingsPath();
            if (string.IsNullOrWhiteSpace(settingsPath) || !File.Exists(settingsPath))
            {
                return null;
            }

            var settings = CreateDefaultRconSettings();

            var inRconSection = false;
            var inSecureSection = false;
            foreach (var rawLine in File.ReadAllLines(settingsPath))
            {
                var line = rawLine == null ? string.Empty : rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#") || line.StartsWith(";"))
                {
                    continue;
                }

                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    inRconSection = string.Equals(line, "[RCON]", StringComparison.OrdinalIgnoreCase);
                    inSecureSection = string.Equals(line, "[SecureRCON]", StringComparison.OrdinalIgnoreCase);
                    continue;
                }

                var separatorIndex = line.IndexOf('=');
                if (separatorIndex <= 0)
                {
                    continue;
                }

                var key = line.Substring(0, separatorIndex).Trim();
                var value = line.Substring(separatorIndex + 1).Trim();
                if (inRconSection && string.Equals(key, "BindAddress", StringComparison.OrdinalIgnoreCase))
                {
                    settings.BindAddress = value;
                }
                else if (inRconSection && string.Equals(key, "Port", StringComparison.OrdinalIgnoreCase))
                {
                    int parsedPort;
                    if (int.TryParse(value, out parsedPort) && parsedPort > 0)
                    {
                        settings.Port = parsedPort;
                    }
                }
                else if (inRconSection && string.Equals(key, "Password", StringComparison.OrdinalIgnoreCase))
                {
                    settings.Password = value;
                }
                else if (inRconSection && string.Equals(key, "AllowedIPs", StringComparison.OrdinalIgnoreCase))
                {
                    settings.AllowedIPs = value;
                }
                else if (inRconSection && string.Equals(key, "MaxFailedAttempts", StringComparison.OrdinalIgnoreCase))
                {
                    int parsedAttempts;
                    if (int.TryParse(value, out parsedAttempts) && parsedAttempts > 0)
                    {
                        settings.MaxFailedAttempts = parsedAttempts;
                    }
                }
                else if (inRconSection && string.Equals(key, "Timeout", StringComparison.OrdinalIgnoreCase))
                {
                    int parsedTimeout;
                    if (int.TryParse(value, out parsedTimeout) && parsedTimeout > 0)
                    {
                        settings.TimeoutSeconds = parsedTimeout;
                    }
                }
                else if (inRconSection && string.Equals(key, "EnableLogging", StringComparison.OrdinalIgnoreCase))
                {
                    settings.EnableLogging = value.Equals("true", StringComparison.OrdinalIgnoreCase);
                }
                else if (inSecureSection && string.Equals(key, "Enabled", StringComparison.OrdinalIgnoreCase))
                {
                    settings.SecureEnabled = value.Equals("true", StringComparison.OrdinalIgnoreCase);
                }
                else if (inSecureSection && string.Equals(key, "AESKey", StringComparison.OrdinalIgnoreCase))
                {
                    settings.AesKey = value;
                }
            }

            return settings;
        }

        private void LoadRconSettingsIntoUi()
        {
            var settings = LoadRconSettingsFromDisk() ?? CreateDefaultRconSettings();
            rconBindAddressTextBox.Text = settings.BindAddress;
            rconPortNumeric.Value = Math.Max(rconPortNumeric.Minimum, Math.Min(rconPortNumeric.Maximum, settings.Port));
            rconPasswordTextBox.Text = settings.Password ?? string.Empty;
            rconAllowedIpsTextBox.Text = settings.AllowedIPs ?? string.Empty;
            rconMaxFailedAttemptsNumeric.Value = Math.Max(rconMaxFailedAttemptsNumeric.Minimum, Math.Min(rconMaxFailedAttemptsNumeric.Maximum, settings.MaxFailedAttempts));
            rconTimeoutNumeric.Value = Math.Max(rconTimeoutNumeric.Minimum, Math.Min(rconTimeoutNumeric.Maximum, settings.TimeoutSeconds));
            rconEnableLoggingCheckBox.Checked = settings.EnableLogging;
            rconSecureEnabledCheckBox.Checked = settings.SecureEnabled;
            rconAesKeyTextBox.Text = settings.AesKey ?? string.Empty;
        }

        private void RefreshRconStatusUi()
        {
            var serverRoot = GetCurrentServerRoot();
            if (string.IsNullOrWhiteSpace(serverRoot))
            {
                rconStatusLabel.Text = "Load a server root or choose an install directory to manage WindroseRCON.";
                return;
            }

            var selectedDll = FindLocalRconVersionDllSource();
            var installedVersionDllPath = GetRconVersionDllPath();
            var settingsPath = GetRconSettingsPath();
            var versionDllState = File.Exists(installedVersionDllPath) ? "Installed" : "Not installed";
            var settingsState = File.Exists(settingsPath) ? "settings.ini found" : "settings.ini missing";
            var selectedState = string.IsNullOrWhiteSpace(selectedDll)
                ? "No local version.dll selected."
                : "Selected DLL: " + Path.GetFileName(selectedDll);
            rconStatusLabel.Text = selectedState
                + "\nTarget Win64: " + GetRconWin64Directory()
                + "\nInstall state: " + versionDllState + " | " + settingsState;
        }

        private static RconSettingsSnapshot CreateDefaultRconSettings()
        {
            return new RconSettingsSnapshot
            {
                BindAddress = "0.0.0.0",
                Port = 27065,
                Password = "windrose_admin",
                AllowedIPs = string.Empty,
                MaxFailedAttempts = 5,
                TimeoutSeconds = 60,
                EnableLogging = true,
                SecureEnabled = false,
                AesKey = string.Empty
            };
        }

        private RconSettingsSnapshot BuildRconSettingsFromUi()
        {
            return new RconSettingsSnapshot
            {
                BindAddress = (rconBindAddressTextBox.Text ?? string.Empty).Trim(),
                Port = Decimal.ToInt32(rconPortNumeric.Value),
                Password = (rconPasswordTextBox.Text ?? string.Empty).Trim(),
                AllowedIPs = (rconAllowedIpsTextBox.Text ?? string.Empty).Trim(),
                MaxFailedAttempts = Decimal.ToInt32(rconMaxFailedAttemptsNumeric.Value),
                TimeoutSeconds = Decimal.ToInt32(rconTimeoutNumeric.Value),
                EnableLogging = rconEnableLoggingCheckBox.Checked,
                SecureEnabled = rconSecureEnabledCheckBox.Checked,
                AesKey = (rconAesKeyTextBox.Text ?? string.Empty).Trim()
            };
        }

        private void WriteRconSettingsSnapshot(RconSettingsSnapshot settings)
        {
            var settingsPath = GetRconSettingsPath();
            if (string.IsNullOrWhiteSpace(settingsPath))
            {
                throw new InvalidOperationException("Load a server root or choose an install directory first.");
            }

            var settingsDirectory = Path.GetDirectoryName(settingsPath);
            if (string.IsNullOrWhiteSpace(settingsDirectory))
            {
                throw new InvalidOperationException("Could not resolve the windrosercon settings folder.");
            }

            Directory.CreateDirectory(settingsDirectory);
            var lines = new List<string>();
            lines.Add("[RCON]");
            lines.Add("BindAddress=" + (settings.BindAddress ?? string.Empty));
            lines.Add("Port=" + settings.Port);
            lines.Add("Password=" + (settings.Password ?? string.Empty));
            lines.Add("AllowedIPs=" + (settings.AllowedIPs ?? string.Empty));
            lines.Add("MaxFailedAttempts=" + settings.MaxFailedAttempts);
            lines.Add("Timeout=" + settings.TimeoutSeconds);
            lines.Add("EnableLogging=" + (settings.EnableLogging ? "true" : "false"));
            lines.Add(string.Empty);
            lines.Add("[SecureRCON]");
            lines.Add("Enabled=" + (settings.SecureEnabled ? "true" : "false"));
            lines.Add("AESKey=" + (settings.AesKey ?? string.Empty));
            File.WriteAllLines(settingsPath, lines.ToArray());
        }

        private static string QueryPlayerCountDisplay(RconSettingsSnapshot settings)
        {
            try
            {
                var infoBody = ExecuteRconCommand(settings, "info");
                if (string.IsNullOrWhiteSpace(infoBody))
                {
                    return "no data";
                }

                foreach (var rawLine in infoBody.Replace("\r\n", "\n").Split('\n'))
                {
                    var line = rawLine.Trim();
                    if (!line.StartsWith("Players:", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var separator = line.IndexOf(':');
                    if (separator >= 0 && separator + 1 < line.Length)
                    {
                        return line.Substring(separator + 1).Trim();
                    }
                }

                return "unknown";
            }
            catch
            {
                return "offline";
            }
        }

        private static string ExecuteRconCommand(RconSettingsSnapshot settings, string command)
        {
            using (var client = new TcpClient())
            {
                client.ReceiveTimeout = 2000;
                client.SendTimeout = 2000;
                var host = string.IsNullOrWhiteSpace(settings.BindAddress) || settings.BindAddress == "0.0.0.0" || settings.BindAddress == "::"
                    ? "127.0.0.1"
                    : settings.BindAddress;
                client.Connect(host, settings.Port);
                using (var stream = client.GetStream())
                {
                    var requestId = 1;
                    SendRconPacket(stream, requestId, 3, settings.Password ?? string.Empty);
                    var authResponse = ReceiveRconPacket(stream);
                    if (authResponse == null || authResponse.Item1 == -1)
                    {
                        throw new InvalidOperationException("RCON authentication failed.");
                    }

                    requestId++;
                    SendRconPacket(stream, requestId, 2, command);
                    var response = ReceiveRconPacket(stream);
                    return response != null ? response.Item3 : string.Empty;
                }
            }
        }

        private static void SendRconPacket(NetworkStream stream, int requestId, int packetType, string body)
        {
            var bodyBytes = Encoding.UTF8.GetBytes((body ?? string.Empty) + "\0");
            var packetSize = bodyBytes.Length + 10;
            var packet = new List<byte>(packetSize + 4);
            packet.AddRange(BitConverter.GetBytes(packetSize));
            packet.AddRange(BitConverter.GetBytes(requestId));
            packet.AddRange(BitConverter.GetBytes(packetType));
            packet.AddRange(bodyBytes);
            packet.Add(0);
            stream.Write(packet.ToArray(), 0, packet.Count);
        }

        private static Tuple<int, int, string> ReceiveRconPacket(NetworkStream stream)
        {
            var sizeBuffer = ReadExact(stream, 4);
            if (sizeBuffer == null)
            {
                return null;
            }

            var size = BitConverter.ToInt32(sizeBuffer, 0);
            if (size < 10 || size > 65536)
            {
                return null;
            }

            var data = ReadExact(stream, size);
            if (data == null || data.Length < 10)
            {
                return null;
            }

            var requestId = BitConverter.ToInt32(data, 0);
            var packetType = BitConverter.ToInt32(data, 4);
            var bodyLength = Math.Max(0, data.Length - 10);
            var body = bodyLength > 0 ? Encoding.UTF8.GetString(data, 8, bodyLength) : string.Empty;
            return Tuple.Create(requestId, packetType, body.TrimEnd('\0'));
        }

        private static byte[] ReadExact(NetworkStream stream, int count)
        {
            var buffer = new byte[count];
            var offset = 0;
            while (offset < count)
            {
                var read = stream.Read(buffer, offset, count - offset);
                if (read <= 0)
                {
                    return null;
                }
                offset += read;
            }
            return buffer;
        }

        private sealed class RconSettingsSnapshot
        {
            public string BindAddress { get; set; }
            public int Port { get; set; }
            public string Password { get; set; }
            public string AllowedIPs { get; set; }
            public int MaxFailedAttempts { get; set; }
            public int TimeoutSeconds { get; set; }
            public bool EnableLogging { get; set; }
            public bool SecureEnabled { get; set; }
            public string AesKey { get; set; }
        }
    }
}
