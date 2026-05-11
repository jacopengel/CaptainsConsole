using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Discord;
using Discord.WebSocket;

namespace WindroseServerManager.Desktop
{
    internal sealed partial class MainForm
    {
        private DiscordSocketClient discordClient;

        private sealed class DiscordSettingsSnapshot
        {
            public string BotToken { get; set; }
            public ulong GuildId { get; set; }
            public ulong PublicChannelId { get; set; }
            public ulong AdminChannelId { get; set; }
            public List<ulong> AdminRoleIds { get; set; }
            public int RefreshSeconds { get; set; }
            public bool AutoConnect { get; set; }
        }

        private void SaveDiscordSettings()
        {
            storedDiscordBotToken = (discordBotTokenTextBox.Text ?? string.Empty).Trim();
            storedDiscordGuildId = (discordGuildIdTextBox.Text ?? string.Empty).Trim();
            storedDiscordPublicChannelId = (discordPublicChannelIdTextBox.Text ?? string.Empty).Trim();
            storedDiscordAdminChannelId = (discordAdminChannelIdTextBox.Text ?? string.Empty).Trim();
            storedDiscordAdminRoleIds = (discordAdminRoleIdsTextBox.Text ?? string.Empty).Trim();
            discordAutoConnect = discordAutoConnectCheckBox.Checked;
            SavePreferences();
            RefreshDiscordUi();
            SetStatus("Discord settings saved locally.", false);
        }

        private void RefreshDiscordUi()
        {
            connectDiscordButton.Text = discordClient != null && discordClient.ConnectionState == ConnectionState.Connected
                ? "Disconnect Discord"
                : "Connect Discord";
            discordRefreshTimer.Interval = Math.Max(15000, Decimal.ToInt32(discordRefreshSecondsNumeric.Value) * 1000);
            discordStatusLabel.Text = BuildDiscordStatusText();
        }

        private string BuildDiscordStatusText()
        {
            var builder = new StringBuilder();
            var connected = discordClient != null && discordClient.ConnectionState == ConnectionState.Connected;
            builder.AppendLine(connected
                ? ("Connected as " + (discordClient.CurrentUser == null ? "(unknown user)" : discordClient.CurrentUser.Username))
                : "Discord bot is not connected.");
            builder.AppendLine("Public panel message: " + (discordPublicMessageId > 0 ? discordPublicMessageId.ToString() : "not published"));
            builder.AppendLine("Admin panel message: " + (discordAdminMessageId > 0 ? discordAdminMessageId.ToString() : "not published"));
            builder.Append("Discord only works while Captain's Console stays open.");
            return builder.ToString();
        }

        private void ToggleDiscordConnection()
        {
            if (discordClient != null && discordClient.ConnectionState == ConnectionState.Connected)
            {
                ShutdownDiscordRuntime();
                SetStatus("Discord bot disconnected.", false);
                return;
            }

            ConnectDiscordBot();
        }

        private void BeginDiscordAutoConnect()
        {
            if (discordAutoConnectCheckBox.Checked)
            {
                ConnectDiscordBot();
            }
        }

        private async void ConnectDiscordBot()
        {
            if (discordConnectionBusy)
            {
                return;
            }

            Exception shutdownException = null;
            try
            {
                discordConnectionBusy = true;
                RefreshDiscordUi();
                var settings = BuildDiscordSettingsFromUi();
                if (string.IsNullOrWhiteSpace(settings.BotToken))
                {
                    throw new InvalidOperationException("Enter a Discord bot token first.");
                }

                if (discordClient != null)
                {
                    await ShutdownDiscordRuntimeAsync();
                }

                var config = new DiscordSocketConfig
                {
                    GatewayIntents = GatewayIntents.Guilds,
                    AlwaysDownloadUsers = false,
                    LogGatewayIntentWarnings = false
                };

                discordClient = new DiscordSocketClient(config);
                discordClient.Log += OnDiscordLogAsync;
                discordClient.Ready += OnDiscordReadyAsync;
                discordClient.ButtonExecuted += OnDiscordButtonExecutedAsync;
                discordClient.SelectMenuExecuted += OnDiscordSelectMenuExecutedAsync;
                discordClient.ModalSubmitted += OnDiscordModalSubmittedAsync;
                await discordClient.LoginAsync(TokenType.Bot, settings.BotToken);
                await discordClient.StartAsync();
                discordRefreshTimer.Interval = Math.Max(15000, settings.RefreshSeconds * 1000);
                discordRefreshTimer.Start();
                RefreshDiscordUi();
                SetStatus("Discord bot connected.", false);
            }
            catch (Exception ex)
            {
                SetStatus("Discord connect failed: " + ex.Message, true);
                shutdownException = ex;
            }
            finally
            {
                discordConnectionBusy = false;
                RefreshDiscordUi();
            }

            if (shutdownException != null)
            {
                await ShutdownDiscordRuntimeAsync();
            }
        }

        private void ShutdownDiscordRuntime()
        {
            try
            {
                ShutdownDiscordRuntimeAsync().GetAwaiter().GetResult();
            }
            catch
            {
            }
        }

        private async Task ShutdownDiscordRuntimeAsync()
        {
            discordRefreshTimer.Stop();
            if (discordClient == null)
            {
                RefreshDiscordUi();
                return;
            }

            try
            {
                discordClient.ButtonExecuted -= OnDiscordButtonExecutedAsync;
                discordClient.SelectMenuExecuted -= OnDiscordSelectMenuExecutedAsync;
                discordClient.ModalSubmitted -= OnDiscordModalSubmittedAsync;
                discordClient.Ready -= OnDiscordReadyAsync;
                discordClient.Log -= OnDiscordLogAsync;
                if (discordClient.ConnectionState != ConnectionState.Disconnected)
                {
                    await discordClient.StopAsync();
                }
                await discordClient.LogoutAsync();
            }
            catch
            {
            }
            finally
            {
                discordClient.Dispose();
                discordClient = null;
                RefreshDiscordUi();
            }
        }

        private Task OnDiscordLogAsync(LogMessage message)
        {
            AppendLog("[discord] " + message.ToString());
            return Task.FromResult(0);
        }

        private Task OnDiscordReadyAsync()
        {
            BeginInvoke((MethodInvoker)delegate
            {
                RefreshDiscordUi();
                SetStatus("Discord bot ready.", false);
            });
            return Task.FromResult(0);
        }

        private void HandleDiscordRefreshTick()
        {
            if (discordClient == null || discordClient.ConnectionState != ConnectionState.Connected || discordPanelsBusy)
            {
                return;
            }

            RefreshDiscordPanels();
        }

        private async void RefreshDiscordPanels()
        {
            if (discordPanelsBusy)
            {
                return;
            }

            try
            {
                discordPanelsBusy = true;
                if (discordClient == null || discordClient.ConnectionState != ConnectionState.Connected)
                {
                    throw new InvalidOperationException("Connect Discord first.");
                }

                await UpdateDiscordPublicPanelMessageAsync();
                await UpdateDiscordAdminPanelMessageAsync();
                SavePreferences();
                RefreshDiscordUi();
                SetStatus("Discord panels refreshed.", false);
            }
            catch (Exception ex)
            {
                SetStatus("Discord refresh failed: " + ex.Message, true);
            }
            finally
            {
                discordPanelsBusy = false;
                RefreshDiscordUi();
            }
        }

        private async void PublishDiscordPublicPanel()
        {
            try
            {
                if (discordClient == null || discordClient.ConnectionState != ConnectionState.Connected)
                {
                    throw new InvalidOperationException("Connect Discord first.");
                }

                discordPublicMessageId = await PublishDiscordPanelAsync(false);
                SavePreferences();
                RefreshDiscordUi();
                SetStatus("Published Discord public panel.", false);
            }
            catch (Exception ex)
            {
                SetStatus("Publish public panel failed: " + ex.Message, true);
            }
        }

        private async void PublishDiscordAdminPanel()
        {
            try
            {
                if (discordClient == null || discordClient.ConnectionState != ConnectionState.Connected)
                {
                    throw new InvalidOperationException("Connect Discord first.");
                }

                discordAdminMessageId = await PublishDiscordPanelAsync(true);
                SavePreferences();
                RefreshDiscordUi();
                SetStatus("Published Discord admin panel.", false);
            }
            catch (Exception ex)
            {
                SetStatus("Publish admin panel failed: " + ex.Message, true);
            }
        }

        private async Task<ulong> PublishDiscordPanelAsync(bool adminPanel)
        {
            var channel = await GetDiscordTargetChannelAsync(adminPanel);
            var text = adminPanel ? BuildAdminPanelText() : BuildPublicPanelText();
            var embed = adminPanel ? BuildAdminPanelEmbed() : BuildPublicPanelEmbed();
            var component = adminPanel ? BuildAdminPanelComponents() : BuildPublicPanelComponents();
            var message = await channel.SendMessageAsync(text: text, embed: embed, components: component);
            return message.Id;
        }

        private async Task UpdateDiscordPublicPanelMessageAsync()
        {
            if (discordPublicMessageId <= 0)
            {
                return;
            }

            var channel = await GetDiscordTargetChannelAsync(false);
            var message = await channel.GetMessageAsync(discordPublicMessageId) as IUserMessage;
            if (message == null)
            {
                discordPublicMessageId = 0;
                return;
            }

            var text = BuildPublicPanelText();
            var embed = BuildPublicPanelEmbed();
            var component = BuildPublicPanelComponents();
            await message.ModifyAsync(delegate(MessageProperties props)
            {
                props.Content = text;
                props.Embed = embed;
                props.Components = component;
            });
        }

        private async Task UpdateDiscordAdminPanelMessageAsync()
        {
            if (discordAdminMessageId <= 0)
            {
                return;
            }

            var channel = await GetDiscordTargetChannelAsync(true);
            var message = await channel.GetMessageAsync(discordAdminMessageId) as IUserMessage;
            if (message == null)
            {
                discordAdminMessageId = 0;
                return;
            }

            var text = BuildAdminPanelText();
            var embed = BuildAdminPanelEmbed();
            var component = BuildAdminPanelComponents();
            await message.ModifyAsync(delegate(MessageProperties props)
            {
                props.Content = text;
                props.Embed = embed;
                props.Components = component;
            });
        }

        private async Task<IMessageChannel> GetDiscordTargetChannelAsync(bool adminPanel)
        {
            var settings = BuildDiscordSettingsFromUi();
            var channelId = adminPanel ? settings.AdminChannelId : settings.PublicChannelId;
            if (channelId <= 0)
            {
                throw new InvalidOperationException(adminPanel ? "Enter an Admin Channel ID first." : "Enter a Public Channel ID first.");
            }

            var channel = discordClient.GetChannel(channelId) as IMessageChannel;
            if (channel != null)
            {
                return channel;
            }

            channel = await discordClient.GetChannelAsync(channelId) as IMessageChannel;
            if (channel == null)
            {
                throw new InvalidOperationException("Discord channel " + channelId + " was not found or is not a message channel.");
            }

            return channel;
        }

        private MessageComponent BuildPublicPanelComponents()
        {
            return new ComponentBuilder()
                .WithButton("Refresh", "cc_public_refresh", ButtonStyle.Primary)
                .WithButton("Show Players", "cc_public_players", ButtonStyle.Success)
                .Build();
        }

        private MessageComponent BuildAdminPanelComponents()
        {
            var builder = new ComponentBuilder();
            builder.WithButton("Help", "cc_admin_help", ButtonStyle.Secondary, row: 0);
            builder.WithButton("Info", "cc_admin_info", ButtonStyle.Secondary, row: 0);
            builder.WithButton("Show Players", "cc_admin_showplayers", ButtonStyle.Success, row: 0);
            builder.WithButton("Ban List", "cc_admin_banlist", ButtonStyle.Secondary, row: 0);
            builder.WithButton("Player Info", "cc_admin_playerinfo", ButtonStyle.Secondary, row: 1);
            builder.WithButton("Get Position", "cc_admin_getpos", ButtonStyle.Secondary, row: 1);
            builder.WithButton("Kick", "cc_admin_kick", ButtonStyle.Danger, row: 1);
            builder.WithButton("Ban", "cc_admin_ban", ButtonStyle.Danger, row: 1);
            builder.WithButton("Unban", "cc_admin_unban", ButtonStyle.Primary, row: 2);
            return builder.Build();
        }

        private string BuildPublicPanelText()
        {
            return string.Empty;
        }

        private Embed BuildPublicPanelEmbed()
        {
            var serverName = currentState != null && currentState.Server != null && !string.IsNullOrWhiteSpace(currentState.Server.ServerName)
                ? currentState.Server.ServerName
                : "Windrose Server";
            var currentWorld = currentState != null && currentState.Server != null && !string.IsNullOrWhiteSpace(currentState.Server.WorldIslandId)
                ? currentState.Server.WorldIslandId
                : "(unknown)";
            var uptime = GetCurrentServerUptimeDisplay();
            var playerCount = GetDiscordPlayerCountDisplay();
            var builder = new EmbedBuilder()
                .WithTitle(serverName)
                .WithDescription("Live status panel powered by Captain's Console.")
                .WithColor(GetDiscordPanelColor())
                .WithCurrentTimestamp();

            builder.AddField("Server State", SafeDiscordFieldValue(serverStateValueLabel.Text), true);
            builder.AddField("Players", SafeDiscordFieldValue(playerCount), true);
            builder.AddField("Uptime", SafeDiscordFieldValue(uptime), true);
            builder.AddField("World", SafeDiscordFieldValue(currentWorld), true);
            return builder.Build();
        }

        private string BuildAdminPanelText()
        {
            return string.Empty;
        }

        private Embed BuildAdminPanelEmbed()
        {
            var builder = new EmbedBuilder()
                .WithTitle("Captain's Console Admin Panel")
                .WithDescription("Use the controls below to query WindroseRCON and run admin actions while Captain's Console is open.")
                .WithColor(GetDiscordPanelColor())
                .WithCurrentTimestamp()
                .WithFooter("Admin actions are live only while Captain's Console is running.");

            builder.AddField("Server State", SafeDiscordFieldValue(serverStateValueLabel.Text), true);
            builder.AddField("Players", SafeDiscordFieldValue(GetDiscordPlayerCountDisplay()), true);
            builder.AddField("Uptime", SafeDiscordFieldValue(GetCurrentServerUptimeDisplay()), true);
            builder.AddField("Read Commands", "Help, Info, Show Players, Ban List", false);
            builder.AddField("Player Actions", "Player Info, Get Position, Kick, Ban, Unban", false);
            builder.AddField("Security", "Admin buttons are restricted by the configured Discord role IDs.", false);
            return builder.Build();
        }

        private string GetCurrentServerUptimeDisplay()
        {
            var process = ResolveRunningServerProcess();
            if (process == null)
            {
                return "offline";
            }

            if (!serverStartUtc.HasValue)
            {
                return "running";
            }

            var uptime = DateTime.UtcNow - serverStartUtc.Value;
            if (uptime.TotalSeconds < 0)
            {
                return "running";
            }

            return string.Format("{0:00}:{1:00}:{2:00}", (int)uptime.TotalHours, uptime.Minutes, uptime.Seconds);
        }

        private string GetDiscordPlayerCountDisplay()
        {
            var settings = LoadRconSettingsFromDisk();
            return settings == null ? lastKnownPlayerCount : QueryPlayerCountDisplay(settings);
        }

        private DiscordSettingsSnapshot BuildDiscordSettingsFromUi()
        {
            return new DiscordSettingsSnapshot
            {
                BotToken = (discordBotTokenTextBox.Text ?? string.Empty).Trim(),
                GuildId = ParseUlongOrDefault(discordGuildIdTextBox.Text),
                PublicChannelId = ParseUlongOrDefault(discordPublicChannelIdTextBox.Text),
                AdminChannelId = ParseUlongOrDefault(discordAdminChannelIdTextBox.Text),
                AdminRoleIds = ParseUlongList(discordAdminRoleIdsTextBox.Text),
                RefreshSeconds = Decimal.ToInt32(discordRefreshSecondsNumeric.Value),
                AutoConnect = discordAutoConnectCheckBox.Checked
            };
        }

        private static ulong ParseUlongOrDefault(string value)
        {
            ulong parsed;
            return ulong.TryParse((value ?? string.Empty).Trim(), out parsed) ? parsed : 0UL;
        }

        private static List<ulong> ParseUlongList(string raw)
        {
            return (raw ?? string.Empty)
                .Split(new[] { ',', ';', ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(ParseUlongOrDefault)
                .Where(delegate(ulong value) { return value > 0; })
                .Distinct()
                .ToList();
        }

        private async Task OnDiscordButtonExecutedAsync(SocketMessageComponent component)
        {
            string errorMessage = null;
            try
            {
                switch (component.Data.CustomId)
                {
                    case "cc_public_refresh":
                        await UpdateDiscordPublicPanelMessageAsync();
                        await component.RespondAsync("Public panel refreshed.", ephemeral: true);
                        break;
                    case "cc_public_players":
                        await component.RespondAsync(FormatDiscordCodeBlock(GetRconPlayersText()), ephemeral: true);
                        break;
                    case "cc_admin_help":
                        await HandleDiscordAdminSimpleCommandAsync(component, "help");
                        break;
                    case "cc_admin_info":
                        await HandleDiscordAdminSimpleCommandAsync(component, "info");
                        break;
                    case "cc_admin_showplayers":
                        await HandleDiscordAdminSimpleCommandAsync(component, "showplayers");
                        break;
                    case "cc_admin_banlist":
                        await HandleDiscordAdminSimpleCommandAsync(component, "banlist");
                        break;
                    case "cc_admin_playerinfo":
                        await RespondWithPlayerSelectAsync(component, "playerinfo");
                        break;
                    case "cc_admin_getpos":
                        await RespondWithPlayerSelectAsync(component, "getpos");
                        break;
                    case "cc_admin_kick":
                        await RespondWithPlayerSelectAsync(component, "kick");
                        break;
                    case "cc_admin_ban":
                        await RespondWithPlayerSelectAsync(component, "ban");
                        break;
                    case "cc_admin_unban":
                        await ShowUnbanModalAsync(component);
                        break;
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Discord admin action failed: " + ex.Message;
            }

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                await SafeRespondToInteractionAsync(component, errorMessage);
            }
        }

        private async Task OnDiscordSelectMenuExecutedAsync(SocketMessageComponent component)
        {
            string errorMessage = null;
            try
            {
                if (!component.Data.CustomId.StartsWith("cc_select_", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                EnsureDiscordAdminAuthorized(component);
                var action = component.Data.CustomId.Substring("cc_select_".Length);
                var accountId = component.Data.Values != null && component.Data.Values.Count > 0
                    ? component.Data.Values.FirstOrDefault()
                    : string.Empty;
                if (string.IsNullOrWhiteSpace(accountId))
                {
                    throw new InvalidOperationException("No account ID was selected.");
                }

                var command = BuildDiscordRconCommand(action, accountId, string.Empty);
                var body = ExecuteRconCommand(BuildRconSettingsFromUi(), command);
                await component.RespondAsync(FormatDiscordCodeBlock(body), ephemeral: true);
                if (string.Equals(action, "showplayers", StringComparison.OrdinalIgnoreCase))
                {
                    PopulateRconPlayersList(body);
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Discord player action failed: " + ex.Message;
            }

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                await SafeRespondToInteractionAsync(component, errorMessage);
            }
        }

        private async Task OnDiscordModalSubmittedAsync(SocketModal modal)
        {
            string errorMessage = null;
            try
            {
                if (!string.Equals(modal.Data.CustomId, "cc_modal_unban", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                EnsureDiscordAdminAuthorized(modal);
                var accountId = modal.Data.Components.FirstOrDefault(delegate(SocketMessageComponentData component)
                {
                    return string.Equals(component.CustomId, "account_id", StringComparison.OrdinalIgnoreCase);
                });
                var selectedAccountId = accountId == null ? string.Empty : accountId.Value;
                if (string.IsNullOrWhiteSpace(selectedAccountId))
                {
                    throw new InvalidOperationException("Enter an account ID to unban.");
                }

                var body = ExecuteRconCommand(BuildRconSettingsFromUi(), "unban " + selectedAccountId.Trim());
                await modal.RespondAsync(FormatDiscordCodeBlock(body), ephemeral: true);
            }
            catch (Exception ex)
            {
                errorMessage = "Discord unban failed: " + ex.Message;
            }

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                await SafeRespondToInteractionAsync(modal, errorMessage);
            }
        }

        private async Task HandleDiscordAdminSimpleCommandAsync(SocketMessageComponent component, string commandName)
        {
            EnsureDiscordAdminAuthorized(component);
            var body = ExecuteRconCommand(BuildRconSettingsFromUi(), commandName);
            if (string.Equals(commandName, "showplayers", StringComparison.OrdinalIgnoreCase))
            {
                PopulateRconPlayersList(body);
            }
            await component.RespondAsync(FormatDiscordCodeBlock(body), ephemeral: true);
        }

        private async Task RespondWithPlayerSelectAsync(SocketMessageComponent component, string action)
        {
            EnsureDiscordAdminAuthorized(component);
            var players = GetDiscordOnlinePlayers();
            if (players.Count == 0)
            {
                await component.RespondAsync("No online players were found.", ephemeral: true);
                return;
            }

            var menu = new SelectMenuBuilder()
                .WithCustomId("cc_select_" + action)
                .WithPlaceholder("Choose a player")
                .WithMinValues(1)
                .WithMaxValues(1);

            foreach (var player in players.Take(25))
            {
                menu.AddOption(player.Item1, player.Item2, player.Item3);
            }

            var componentBuilder = new ComponentBuilder()
                .WithSelectMenu(menu);
            await component.RespondAsync("Choose a player for `" + action + "`.", components: componentBuilder.Build(), ephemeral: true);
        }

        private async Task ShowUnbanModalAsync(SocketMessageComponent component)
        {
            EnsureDiscordAdminAuthorized(component);
            var modal = new ModalBuilder()
                .WithTitle("Unban Player")
                .WithCustomId("cc_modal_unban")
                .AddTextInput("Account ID", "account_id", TextInputStyle.Short, placeholder: "Enter account ID", required: true, maxLength: 64);
            await component.RespondWithModalAsync(modal.Build());
        }

        private List<Tuple<string, string, string>> GetDiscordOnlinePlayers()
        {
            var responseBody = ExecuteRconCommand(BuildRconSettingsFromUi(), "showplayers");
            var players = new List<Tuple<string, string, string>>();
            foreach (var line in SplitRconResponseLines(responseBody))
            {
                var parsed = TryParseRconPlayerLine(line);
                if (parsed != null)
                {
                    players.Add(parsed);
                }
            }

            return players;
        }

        private string GetRconPlayersText()
        {
            try
            {
                return ExecuteRconCommand(BuildRconSettingsFromUi(), "showplayers");
            }
            catch (Exception ex)
            {
                return "RCON player query failed: " + ex.Message;
            }
        }

        private static string FormatDiscordCodeBlock(string body)
        {
            var text = string.IsNullOrWhiteSpace(body) ? "(no response body returned)" : body.Trim();
            if (text.Length > 1800)
            {
                text = text.Substring(0, 1800) + "\n...";
            }

            return "```text\n" + text.Replace("```", "'''") + "\n```";
        }

        private Color GetDiscordPanelColor()
        {
            var state = (serverStateValueLabel.Text ?? string.Empty).Trim();
            if (state.Equals("Running", StringComparison.OrdinalIgnoreCase))
            {
                return new Color(46, 204, 113);
            }

            if (state.Equals("Stopped", StringComparison.OrdinalIgnoreCase) || state.Equals("Offline", StringComparison.OrdinalIgnoreCase))
            {
                return new Color(231, 76, 60);
            }

            if (state.Equals("Starting", StringComparison.OrdinalIgnoreCase) || state.Equals("Stopping", StringComparison.OrdinalIgnoreCase))
            {
                return new Color(241, 196, 15);
            }

            return new Color(52, 152, 219);
        }

        private static string TrimLabelValue(string labelText)
        {
            var raw = (labelText ?? string.Empty).Trim();
            var colonIndex = raw.IndexOf(':');
            if (colonIndex >= 0 && colonIndex < raw.Length - 1)
            {
                return raw.Substring(colonIndex + 1).Trim();
            }

            return raw;
        }

        private static string SafeDiscordFieldValue(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "Unavailable" : value.Trim();
        }

        private string BuildDiscordRconCommand(string action, string accountId, string reason)
        {
            if (string.Equals(action, "kick", StringComparison.OrdinalIgnoreCase))
            {
                return "kick " + accountId;
            }

            if (string.Equals(action, "ban", StringComparison.OrdinalIgnoreCase))
            {
                return string.IsNullOrWhiteSpace(reason)
                    ? ("ban " + accountId)
                    : ("ban " + accountId + " " + reason);
            }

            return action + " " + accountId;
        }

        private void EnsureDiscordAdminAuthorized(SocketInteraction interaction)
        {
            var settings = BuildDiscordSettingsFromUi();
            if (settings.AdminRoleIds == null || settings.AdminRoleIds.Count == 0)
            {
                return;
            }

            var guildUser = interaction.User as SocketGuildUser;
            if (guildUser == null)
            {
                throw new InvalidOperationException("Discord admin actions must be used in a guild.");
            }

            var userRoleIds = guildUser.Roles.Select(delegate(SocketRole role) { return role.Id; }).ToList();
            if (!settings.AdminRoleIds.Any(delegate(ulong roleId) { return userRoleIds.Contains(roleId); }))
            {
                throw new InvalidOperationException("You do not have permission to use this admin panel.");
            }
        }

        private async Task SafeRespondToInteractionAsync(SocketInteraction interaction, string message)
        {
            var text = string.IsNullOrWhiteSpace(message) ? "Unknown Discord interaction error." : message;
            if (interaction.HasResponded)
            {
                await interaction.FollowupAsync(text, ephemeral: true);
            }
            else
            {
                await interaction.RespondAsync(text, ephemeral: true);
            }
        }
    }
}
