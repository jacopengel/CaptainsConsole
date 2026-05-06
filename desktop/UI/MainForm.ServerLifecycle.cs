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

            SchedulePlayerCountRefresh(running || runningSignal);

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
            installRconButton.Enabled = !provisioningBusy && !string.IsNullOrWhiteSpace(GetCurrentServerRoot());
            uninstallRconButton.Enabled = !provisioningBusy && !string.IsNullOrWhiteSpace(GetCurrentServerRoot());
            saveRconSettingsButton.Enabled = !provisioningBusy && !string.IsNullOrWhiteSpace(GetCurrentServerRoot());
            testRconButton.Enabled = !provisioningBusy && !string.IsNullOrWhiteSpace(GetCurrentServerRoot());
            refreshRconPlayersButton.Enabled = !provisioningBusy && !string.IsNullOrWhiteSpace(GetCurrentServerRoot());
            rconBindAddressTextBox.Enabled = !provisioningBusy;
            rconPortNumeric.Enabled = !provisioningBusy;
            rconPasswordTextBox.Enabled = !provisioningBusy;
            rconAllowedIpsTextBox.Enabled = !provisioningBusy;
            rconMaxFailedAttemptsNumeric.Enabled = !provisioningBusy;
            rconTimeoutNumeric.Enabled = !provisioningBusy;
            rconEnableLoggingCheckBox.Enabled = !provisioningBusy;
            rconSecureEnabledCheckBox.Enabled = !provisioningBusy;
            rconAesKeyTextBox.Enabled = !provisioningBusy;
        }

        private void SetServerStateIndicator(string stateText, Color color)
        {
            serverStateDotPanel.BackColor = color;
            serverStateDotPanel.Invalidate();
            serverStateValueLabel.Text = stateText;
            playerCountValueLabel.Text = currentPlayerCountDisplay;
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
            if (!serverLikelyRunning)
            {
                currentPlayerCountDisplay = "n/a";
                playerCountValueLabel.Text = currentPlayerCountDisplay;
                return;
            }

            if (playerCountPollInFlight)
            {
                return;
            }

            if (lastPlayerCountPollUtc.HasValue && DateTime.UtcNow - lastPlayerCountPollUtc.Value < TimeSpan.FromSeconds(6))
            {
                return;
            }

            var rconSettings = LoadRconSettingsFromDisk();
            if (rconSettings == null || string.IsNullOrWhiteSpace(rconSettings.Password))
            {
                currentPlayerCountDisplay = "no rcon";
                playerCountValueLabel.Text = currentPlayerCountDisplay;
                return;
            }

            playerCountPollInFlight = true;
            lastPlayerCountPollUtc = DateTime.UtcNow;
            System.Threading.ThreadPool.QueueUserWorkItem(delegate
            {
                var nextDisplay = QueryPlayerCountDisplay(rconSettings);
                BeginInvoke((MethodInvoker)delegate
                {
                    currentPlayerCountDisplay = nextDisplay;
                    playerCountValueLabel.Text = currentPlayerCountDisplay;
                    playerCountPollInFlight = false;
                });
            });
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
            rconPortNumeric.Value = Math.Min(rconPortNumeric.Maximum, Math.Max(rconPortNumeric.Minimum, settings.Port));
            rconPasswordTextBox.Text = settings.Password;
            rconAllowedIpsTextBox.Text = settings.AllowedIPs;
            rconMaxFailedAttemptsNumeric.Value = Math.Min(rconMaxFailedAttemptsNumeric.Maximum, Math.Max(rconMaxFailedAttemptsNumeric.Minimum, settings.MaxFailedAttempts));
            rconTimeoutNumeric.Value = Math.Min(rconTimeoutNumeric.Maximum, Math.Max(rconTimeoutNumeric.Minimum, settings.TimeoutSeconds));
            rconEnableLoggingCheckBox.Checked = settings.EnableLogging;
            rconSecureEnabledCheckBox.Checked = settings.SecureEnabled;
            rconAesKeyTextBox.Text = settings.AesKey;
            RefreshRconStatusUi();
        }

        private void RefreshRconStatusUi()
        {
            var versionDllPath = GetRconVersionDllPath();
            var settingsPath = GetRconSettingsPath();
            var hasDll = !string.IsNullOrWhiteSpace(versionDllPath) && File.Exists(versionDllPath);
            var hasSettings = !string.IsNullOrWhiteSpace(settingsPath) && File.Exists(settingsPath);
            rconStatusLabel.Text = hasDll
                ? (hasSettings ? "RCON status: installed and configured" : "RCON status: version.dll installed, settings not found yet")
                : "RCON status: not installed";
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
