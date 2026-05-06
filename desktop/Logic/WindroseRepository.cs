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
    internal static class WindroseRepository
    {
        public const string SharedQuestsKey = "{\"TagName\": \"WDS.Parameter.Coop.SharedQuests\"}";
        public const string EasyExploreKey = "{\"TagName\": \"WDS.Parameter.EasyExplore\"}";
        public const string MobHealthKey = "{\"TagName\": \"WDS.Parameter.MobHealthMultiplier\"}";
        public const string MobDamageKey = "{\"TagName\": \"WDS.Parameter.MobDamageMultiplier\"}";
        public const string ShipsHealthKey = "{\"TagName\": \"WDS.Parameter.ShipsHealthMultiplier\"}";
        public const string ShipsDamageKey = "{\"TagName\": \"WDS.Parameter.ShipsDamageMultiplier\"}";
        public const string BoardingKey = "{\"TagName\": \"WDS.Parameter.BoardingDifficultyMultiplier\"}";
        public const string CoopStatsKey = "{\"TagName\": \"WDS.Parameter.Coop.StatsCorrectionModifier\"}";
        public const string CoopShipStatsKey = "{\"TagName\": \"WDS.Parameter.Coop.ShipStatsCorrectionModifier\"}";
        public const string CombatKey = "{\"TagName\": \"WDS.Parameter.CombatDifficulty\"}";
        public const string CombatEasyTag = "WDS.Parameter.CombatDifficulty.Easy";
        public const string CombatNormalTag = "WDS.Parameter.CombatDifficulty.Normal";
        public const string CombatHardTag = "WDS.Parameter.CombatDifficulty.Hard";

        public static WindroseState Discover(string inputRoot)
        {
            var root = Path.GetFullPath(inputRoot);
            var serverPath = FindServerDescriptionPath(root);
            var serializer = new JavaScriptSerializer();
            var serverDocument = serializer.Deserialize<ServerDescriptionDocument>(File.ReadAllText(serverPath));
            if (serverDocument == null || serverDocument.ServerDescription_Persistent == null)
            {
                throw new InvalidOperationException("ServerDescription.json could not be parsed.");
            }

            var state = new WindroseState();
            state.ServerRoot = root;
            state.ServerFilePath = serverPath;
            state.Server = ExtractServer(serverDocument);
            state.Worlds = DiscoverWorlds(root, serializer);
            state.SelectedWorldKey = FindSelectedWorldKey(state.Server.WorldIslandId, state.Worlds);
            return state;
        }

        public static WindroseState Save(WindroseState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }

            var serializer = new JavaScriptSerializer();
            var serverPath = FindServerDescriptionPath(state.ServerRoot);
            var serverDocument = serializer.Deserialize<ServerDescriptionDocument>(File.ReadAllText(serverPath));
            if (serverDocument == null || serverDocument.ServerDescription_Persistent == null)
            {
                throw new InvalidOperationException("ServerDescription.json could not be parsed.");
            }

            CreateBackup(serverPath);
            ApplyServer(serverDocument, state.Server);
            File.WriteAllText(serverPath, PrettyJson(serializer.Serialize(serverDocument)));

            var selectedWorld = state.Worlds.FirstOrDefault(delegate(WorldEditableState world)
            {
                return world.Key == state.SelectedWorldKey;
            });
            if (selectedWorld != null)
            {
                CreateBackup(selectedWorld.FilePath);
                var worldDocument = serializer.Deserialize<WorldDescriptionDocument>(File.ReadAllText(selectedWorld.FilePath));
                if (worldDocument == null || worldDocument.WorldDescription == null)
                {
                    throw new InvalidOperationException("WorldDescription.json could not be parsed for " + selectedWorld.WorldName + ".");
                }

                ApplyWorld(worldDocument, selectedWorld);
                File.WriteAllText(selectedWorld.FilePath, PrettyJson(serializer.Serialize(worldDocument)));
            }

            return Discover(state.ServerRoot);
        }

        public static bool IsLikelyInstalledServerRoot(string inputRoot)
        {
            if (string.IsNullOrWhiteSpace(inputRoot))
            {
                return false;
            }

            var root = Path.GetFullPath(inputRoot);
            return Directory.Exists(Path.Combine(root, "R5"))
                || File.Exists(Path.Combine(root, "StartServerForeground.bat"))
                || File.Exists(Path.Combine(root, "StartServer.bat"))
                || File.Exists(Path.Combine(root, "WindroseServer.exe"))
                || File.Exists(Path.Combine(root, "R5", "Binaries", "Win64", "WindroseServer-Win64-Shipping.exe"));
        }

        public static string CreateDefaultServerDescription(string inputRoot)
        {
            var root = Path.GetFullPath(inputRoot);
            var serverDir = Path.Combine(root, "R5");
            Directory.CreateDirectory(serverDir);
            var serverDescriptionPath = Path.Combine(serverDir, "ServerDescription.json");

            if (File.Exists(serverDescriptionPath))
            {
                return serverDescriptionPath;
            }

            var serializer = new JavaScriptSerializer();
            var initialWorldId = FindFirstWorldFolderId(root);
            var document = new ServerDescriptionDocument
            {
                Version = 1,
                DeploymentId = string.Empty,
                ServerDescription_Persistent = new ServerPersistent
                {
                    PersistentServerId = Guid.NewGuid().ToString("N").ToUpperInvariant(),
                    InviteCode = "WINDR01",
                    IsPasswordProtected = false,
                    Password = string.Empty,
                    ServerName = "Windrose Dedicated Server",
                    WorldIslandId = initialWorldId,
                    MaxPlayerCount = 4,
                    UserSelectedRegion = string.Empty,
                    P2pProxyAddress = string.Empty,
                    UseDirectConnection = false,
                    DirectConnectionServerAddress = string.Empty,
                    DirectConnectionServerPort = 7777,
                    DirectConnectionProxyAddress = "0.0.0.0"
                }
            };

            File.WriteAllText(serverDescriptionPath, PrettyJson(serializer.Serialize(document)));
            return serverDescriptionPath;
        }

        public static DataRepairResult RepairDataInconsistency(string inputRoot)
        {
            var root = Path.GetFullPath(inputRoot);
            var saveProfilesPath = Path.Combine(root, "R5", "Saved", "SaveProfiles");
            if (!Directory.Exists(saveProfilesPath))
            {
                throw new DirectoryNotFoundException("Could not find SaveProfiles under " + root + ".");
            }

            var backupPath = Path.Combine(root, "R5", "Saved", "RepairBackups", DateTime.Now.ToString("yyyyMMdd-HHmmss"));
            Directory.CreateDirectory(backupPath);

            var movedFolderCount = MoveDirectoryIfExists(saveProfilesPath, Path.Combine(backupPath, "SaveProfiles"));

            // Recreate clean profile structure so the server can generate fresh data on next launch.
            Directory.CreateDirectory(Path.Combine(saveProfilesPath, "Default"));

            if (movedFolderCount == 0)
            {
                throw new InvalidOperationException("No SaveProfiles folder content was available to repair under " + saveProfilesPath + ".");
            }

            var resetServerDescription = false;
            var serverDescriptionPath = Path.Combine(root, "R5", "ServerDescription.json");
            if (File.Exists(serverDescriptionPath))
            {
                var serverDescriptionBackupPath = Path.Combine(backupPath, "ServerDescription.json");
                File.Move(serverDescriptionPath, serverDescriptionBackupPath);
                resetServerDescription = true;
            }
            else
            {
                var legacyServerDescriptionPath = Path.Combine(root, "ServerDescription.json");
                if (File.Exists(legacyServerDescriptionPath))
                {
                    var legacyServerDescriptionBackupPath = Path.Combine(backupPath, "ServerDescription.json");
                    File.Move(legacyServerDescriptionPath, legacyServerDescriptionBackupPath);
                    resetServerDescription = true;
                }
            }

            return new DataRepairResult
            {
                BackupPath = backupPath,
                MovedFolderCount = movedFolderCount,
                ServerDescriptionReset = resetServerDescription
            };
        }

        public static string PrepareFreshInstallData(string inputRoot)
        {
            var root = Path.GetFullPath(inputRoot);
            var saveProfilesPath = Path.Combine(root, "R5", "Saved", "SaveProfiles");
            var serverDescriptionPath = Path.Combine(root, "R5", "ServerDescription.json");
            var legacyServerDescriptionPath = Path.Combine(root, "ServerDescription.json");

            var backupPath = Path.Combine(root, "R5", "Saved", "InstallBackups", DateTime.Now.ToString("yyyyMMdd-HHmmss"));
            Directory.CreateDirectory(backupPath);

            var movedAnything = false;
            if (Directory.Exists(saveProfilesPath))
            {
                MoveDirectoryIfExists(saveProfilesPath, Path.Combine(backupPath, "SaveProfiles"));
                Directory.CreateDirectory(Path.Combine(saveProfilesPath, "Default"));
                movedAnything = true;
            }

            if (File.Exists(serverDescriptionPath))
            {
                File.Move(serverDescriptionPath, Path.Combine(backupPath, "ServerDescription.json"));
                movedAnything = true;
            }
            else if (File.Exists(legacyServerDescriptionPath))
            {
                File.Move(legacyServerDescriptionPath, Path.Combine(backupPath, "ServerDescription.json"));
                movedAnything = true;
            }

            if (!movedAnything)
            {
                throw new InvalidOperationException("No existing save/config data was found to clean up.");
            }

            return backupPath;
        }

        public static void BackupAndReplaceFile(string targetFilePath, string sourceFilePath)
        {
            if (string.IsNullOrWhiteSpace(targetFilePath))
            {
                throw new ArgumentException("Target file path is required.", "targetFilePath");
            }

            if (string.IsNullOrWhiteSpace(sourceFilePath))
            {
                throw new ArgumentException("Source file path is required.", "sourceFilePath");
            }

            var normalizedTargetFilePath = Path.GetFullPath(targetFilePath);
            var normalizedSourceFilePath = Path.GetFullPath(sourceFilePath);

            if (!File.Exists(normalizedTargetFilePath))
            {
                throw new FileNotFoundException("Could not find the live settings file to restore into.", normalizedTargetFilePath);
            }

            if (!File.Exists(normalizedSourceFilePath))
            {
                throw new FileNotFoundException("Could not find the selected backup file.", normalizedSourceFilePath);
            }

            CreateBackup(normalizedTargetFilePath);
            File.Copy(normalizedSourceFilePath, normalizedTargetFilePath, true);
        }

        public static string ImportWorldIntoServer(string targetServerRoot, string sourceWorldDir)
        {
            var normalizedServerRoot = Path.GetFullPath(targetServerRoot);
            var normalizedSourceWorldDir = Path.GetFullPath(sourceWorldDir);
            var sourceWorldFile = Path.Combine(normalizedSourceWorldDir, "WorldDescription.json");
            if (!File.Exists(sourceWorldFile))
            {
                throw new FileNotFoundException("Could not find WorldDescription.json under " + normalizedSourceWorldDir + ".");
            }

            var sourceWorldId = Path.GetFileName(normalizedSourceWorldDir);
            if (string.IsNullOrWhiteSpace(sourceWorldId))
            {
                throw new InvalidOperationException("Could not determine the source world folder name for the selected import folder.");
            }

            var sourceVersionName = GetVersionNameFromWorldDirectory(normalizedSourceWorldDir);
            var targetVersionName = FindLatestVersionName(normalizedServerRoot);
            if (string.IsNullOrWhiteSpace(targetVersionName))
            {
                targetVersionName = !string.IsNullOrWhiteSpace(sourceVersionName) ? sourceVersionName : "1";
            }

            var targetWorldsRoot = Path.Combine(normalizedServerRoot, "R5", "Saved", "SaveProfiles", "Default", "RocksDB", targetVersionName, "Worlds");
            Directory.CreateDirectory(targetWorldsRoot);

            var targetWorldId = sourceWorldId;
            var targetWorldDir = Path.Combine(targetWorldsRoot, targetWorldId);
            if (Directory.Exists(targetWorldDir))
            {
                throw new InvalidOperationException(
                    "A world with ID '" + targetWorldId + "' already exists on this server. " +
                    "For safety, the importer will not rename RocksDB-backed worlds. " +
                    "Import this world into a clean server/profile first, or remove/archive the conflicting target world before importing.");
            }

            CopyDirectoryRecursive(normalizedSourceWorldDir, targetWorldDir);

            return targetWorldDir;
        }

        public static List<string> ValidateImportWorldSource(string sourceWorldDir)
        {
            var issues = new List<string>();
            if (string.IsNullOrWhiteSpace(sourceWorldDir))
            {
                issues.Add("Selected path is empty.");
                return issues;
            }

            var normalizedSourceWorldDir = Path.GetFullPath(sourceWorldDir);
            if (!Directory.Exists(normalizedSourceWorldDir))
            {
                issues.Add("World folder does not exist.");
                return issues;
            }

            var sourceWorldFile = Path.Combine(normalizedSourceWorldDir, "WorldDescription.json");
            if (!File.Exists(sourceWorldFile))
            {
                issues.Add("Missing required file: WorldDescription.json.");
            }

            return issues;
        }

        public static string CreateInitialWorld(string targetServerRoot, string worldName, string preferredWorldId)
        {
            var normalizedServerRoot = Path.GetFullPath(targetServerRoot);
            var targetVersionName = FindLatestVersionName(normalizedServerRoot);
            if (string.IsNullOrWhiteSpace(targetVersionName))
            {
                targetVersionName = InferVersionNameFromServerDescription(normalizedServerRoot);
            }
            if (string.IsNullOrWhiteSpace(targetVersionName))
            {
                throw new InvalidOperationException("Could not determine the Windrose world save version from the installed files.");
            }

            var targetWorldsRoot = Path.Combine(normalizedServerRoot, "R5", "Saved", "SaveProfiles", "Default", "RocksDB", targetVersionName, "Worlds");
            Directory.CreateDirectory(targetWorldsRoot);

            var targetWorldId = !string.IsNullOrWhiteSpace(preferredWorldId)
                ? preferredWorldId.Trim().ToUpperInvariant()
                : Guid.NewGuid().ToString("N").ToUpperInvariant();
            var targetWorldDir = Path.Combine(targetWorldsRoot, targetWorldId);
            if (Directory.Exists(targetWorldDir))
            {
                throw new InvalidOperationException("A world with ID '" + targetWorldId + "' already exists.");
            }

            Directory.CreateDirectory(targetWorldDir);

            var state = new WorldEditableState
            {
                WorldName = string.IsNullOrWhiteSpace(worldName) ? "The Archipelago" : worldName.Trim(),
                SharedQuests = true,
                EasyExplore = false,
                MobHealthMultiplier = 1,
                MobDamageMultiplier = 1,
                ShipsHealthMultiplier = 1,
                ShipsDamageMultiplier = 1,
                BoardingDifficultyMultiplier = 1,
                CoopStatsCorrectionModifier = 1,
                CoopShipStatsCorrectionModifier = 0,
                CombatDifficulty = CombatNormalTag
            };
            ApplyPreset(state, "Medium");

            var document = new WorldDescriptionDocument
            {
                Version = 1,
                WorldDescription = new WorldDescription
                {
                    islandId = targetWorldId,
                    WorldName = state.WorldName,
                    CreationTime = DateTime.UtcNow.Ticks,
                    WorldPresetType = "Medium",
                    WorldSettings = new WorldSettings
                    {
                        BoolParameters = new Dictionary<string, bool>(),
                        FloatParameters = new Dictionary<string, double>(),
                        TagParameters = new Dictionary<string, TagParameterValue>()
                    }
                }
            };

            ApplyWorld(document, state);

            var serializer = new JavaScriptSerializer();
            var targetWorldFile = Path.Combine(targetWorldDir, "WorldDescription.json");
            File.WriteAllText(targetWorldFile, PrettyJson(serializer.Serialize(document)));
            return targetWorldFile;
        }

        private static string InferVersionNameFromServerDescription(string root)
        {
            try
            {
                var serverDescriptionPath = FindServerDescriptionPathOrNull(root);
                if (string.IsNullOrWhiteSpace(serverDescriptionPath) || !File.Exists(serverDescriptionPath))
                {
                    return string.Empty;
                }

                var serializer = new JavaScriptSerializer();
                var document = serializer.Deserialize<ServerDescriptionDocument>(File.ReadAllText(serverDescriptionPath));
                if (document == null || string.IsNullOrWhiteSpace(document.DeploymentId))
                {
                    return string.Empty;
                }

                var deploymentId = document.DeploymentId.Trim();
                var parts = deploymentId.Split('.');
                if (parts.Length < 4)
                {
                    return string.Empty;
                }

                int parsedPart;
                if (!Int32.TryParse(parts[0], out parsedPart)
                    || !Int32.TryParse(parts[1], out parsedPart)
                    || !Int32.TryParse(parts[2], out parsedPart))
                {
                    return string.Empty;
                }

                var fourthPartChars = new string(parts[3].TakeWhile(Char.IsDigit).ToArray());
                if (string.IsNullOrWhiteSpace(fourthPartChars) || !Int32.TryParse(fourthPartChars, out parsedPart))
                {
                    return string.Empty;
                }

                return parts[0] + "." + parts[1] + "." + parts[2] + "." + fourthPartChars;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static List<ValidationWarning> Validate(WindroseState state)
        {
            var warnings = new List<ValidationWarning>();
            if (state == null || state.Server == null)
            {
                return warnings;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(state.Server.InviteCode ?? string.Empty, "^[0-9A-Za-z]{6,}$"))
            {
                warnings.Add(new ValidationWarning("error", "Invite code must be at least 6 characters and contain only 0-9, a-z, and A-Z."));
            }

            if (state.Server.MaxPlayerCount > 4)
            {
                warnings.Add(new ValidationWarning("warning", "Official guidance still recommends smaller crews for the best experience. Higher MaxPlayerCount values may need more RAM/CPU and can be less stable."));
            }

            if (state.Server.UseDirectConnection && state.Server.DirectConnectionServerPort <= 0)
            {
                warnings.Add(new ValidationWarning("error", "Direct IP mode is enabled, but the direct connection port is blank or invalid."));
            }

            if (state.Server.UseDirectConnection)
            {
                if (IsLoopbackAddress(state.Server.DirectConnectionServerAddress))
                {
                    warnings.Add(new ValidationWarning("error", "DirectConnectionServerAddress is set to a loopback address. Remote players cannot join through 127.0.0.1 / localhost."));
                }

                if (IsLoopbackAddress(state.Server.DirectConnectionProxyAddress))
                {
                    warnings.Add(new ValidationWarning("warning", "DirectConnectionProxyAddress is set to a loopback address. Use 0.0.0.0 or a real local adapter IP instead."));
                }
            }

            if (IsLoopbackAddress(state.Server.P2pProxyAddress))
            {
                warnings.Add(new ValidationWarning("warning", "P2pProxyAddress is set to 127.0.0.1. That is only suitable for local testing on the same PC."));
            }

            var activeWorld = state.Worlds.FirstOrDefault(delegate(WorldEditableState world)
            {
                return world.Key == state.SelectedWorldKey;
            });

            if (state.Worlds.Count == 0)
            {
                warnings.Add(new ValidationWarning("info", "No worlds discovered yet. Start the server once to generate world saves, then reload."));
            }
            else if (string.IsNullOrWhiteSpace(state.Server.WorldIslandId))
            {
                warnings.Add(new ValidationWarning("warning", "Active world ID is empty. Select a world and click 'Set Selected World Active', then save."));
            }
            else if (activeWorld == null)
            {
                warnings.Add(new ValidationWarning("warning", "The active world ID does not match a discovered world folder."));
            }

            foreach (var world in state.Worlds)
            {
                if (world.FolderName != world.IslandId)
                {
                    warnings.Add(new ValidationWarning("error", "World \"" + world.WorldName + "\" has a folder name / islandId mismatch (" + world.FolderName + " vs " + world.IslandId + ")."));
                }
            }

            warnings.Add(new ValidationWarning("info", "Stop the Windrose server before editing or saving config files."));
            return warnings;
        }

        private static bool IsWildcardAddress(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                || string.Equals(value.Trim(), "0.0.0.0", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value.Trim(), "::", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsLoopbackAddress(string value)
        {
            IPAddress address;
            return !string.IsNullOrWhiteSpace(value)
                && IPAddress.TryParse(value.Trim(), out address)
                && IPAddress.IsLoopback(address);
        }

        public static string DetectPreset(WorldEditableState world)
        {
            if (WorldMatchesPreset(world, "Easy"))
            {
                return "Easy";
            }
            if (WorldMatchesPreset(world, "Medium"))
            {
                return "Medium";
            }
            if (WorldMatchesPreset(world, "Hard"))
            {
                return "Hard";
            }
            return "Custom";
        }

        public static void ApplyPreset(WorldEditableState world, string presetName)
        {
            if (presetName == "Easy")
            {
                world.SharedQuests = true;
                world.EasyExplore = false;
                world.MobHealthMultiplier = 0.7;
                world.MobDamageMultiplier = 0.6;
                world.ShipsHealthMultiplier = 0.7;
                world.ShipsDamageMultiplier = 0.6;
                world.BoardingDifficultyMultiplier = 0.7;
                world.CoopStatsCorrectionModifier = 1;
                world.CoopShipStatsCorrectionModifier = 0;
                world.CombatDifficulty = CombatEasyTag;
                return;
            }

            if (presetName == "Hard")
            {
                world.SharedQuests = true;
                world.EasyExplore = false;
                world.MobHealthMultiplier = 1.5;
                world.MobDamageMultiplier = 1.25;
                world.ShipsHealthMultiplier = 1.5;
                world.ShipsDamageMultiplier = 1.25;
                world.BoardingDifficultyMultiplier = 1.5;
                world.CoopStatsCorrectionModifier = 1;
                world.CoopShipStatsCorrectionModifier = 0;
                world.CombatDifficulty = CombatHardTag;
                return;
            }

            world.SharedQuests = true;
            world.EasyExplore = false;
            world.MobHealthMultiplier = 1;
            world.MobDamageMultiplier = 1;
            world.ShipsHealthMultiplier = 1;
            world.ShipsDamageMultiplier = 1;
            world.BoardingDifficultyMultiplier = 1;
            world.CoopStatsCorrectionModifier = 1;
            world.CoopShipStatsCorrectionModifier = 0;
            world.CombatDifficulty = CombatNormalTag;
        }

        private static string FindServerDescriptionPath(string root)
        {
            var candidates = new[]
            {
                Path.Combine(root, "R5", "ServerDescription.json"),
                Path.Combine(root, "ServerDescription.json")
            };

            foreach (var candidate in candidates)
            {
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            throw new FileNotFoundException("Could not find ServerDescription.json under " + root + ".");
        }

        public static string FindServerDescriptionPathOrNull(string root)
        {
            var candidates = new[]
            {
                Path.Combine(root, "R5", "ServerDescription.json"),
                Path.Combine(root, "ServerDescription.json")
            };

            foreach (var candidate in candidates)
            {
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            return null;
        }

        private static List<WorldEditableState> DiscoverWorlds(string root, JavaScriptSerializer serializer)
        {
            var worlds = new List<WorldEditableState>();
            var rocksDbRoot = Path.Combine(root, "R5", "Saved", "SaveProfiles", "Default", "RocksDB");
            if (!Directory.Exists(rocksDbRoot))
            {
                return worlds;
            }

            foreach (var versionDir in Directory.GetDirectories(rocksDbRoot))
            {
                var versionName = Path.GetFileName(versionDir);
                var worldsRoot = Path.Combine(versionDir, "Worlds");
                if (!Directory.Exists(worldsRoot))
                {
                    continue;
                }

                foreach (var worldDir in Directory.GetDirectories(worldsRoot))
                {
                    var worldFilePath = Path.Combine(worldDir, "WorldDescription.json");
                    if (!File.Exists(worldFilePath))
                    {
                        continue;
                    }

                    var document = serializer.Deserialize<WorldDescriptionDocument>(File.ReadAllText(worldFilePath));
                    if (document == null || document.WorldDescription == null)
                    {
                        continue;
                    }

                    worlds.Add(ExtractWorld(document, versionName, Path.GetFileName(worldDir), worldFilePath));
                }
            }

            worlds.Sort(delegate(WorldEditableState left, WorldEditableState right)
            {
                var versionCompare = string.CompareOrdinal(right.GameVersion, left.GameVersion);
                if (versionCompare != 0)
                {
                    return versionCompare;
                }
                return string.Compare(left.WorldName, right.WorldName, StringComparison.OrdinalIgnoreCase);
            });

            return worlds;
        }

        private static ServerEditableState ExtractServer(ServerDescriptionDocument document)
        {
            var server = document.ServerDescription_Persistent;
            return new ServerEditableState
            {
                Version = document.Version,
                DeploymentId = document.DeploymentId ?? string.Empty,
                PersistentServerId = server.PersistentServerId ?? string.Empty,
                InviteCode = server.InviteCode ?? string.Empty,
                IsPasswordProtected = server.IsPasswordProtected,
                Password = server.Password ?? string.Empty,
                ServerName = server.ServerName ?? string.Empty,
                WorldIslandId = server.WorldIslandId ?? string.Empty,
                MaxPlayerCount = server.MaxPlayerCount,
                UserSelectedRegion = server.UserSelectedRegion ?? string.Empty,
                P2pProxyAddress = server.P2pProxyAddress ?? string.Empty,
                UseDirectConnection = server.UseDirectConnection,
                DirectConnectionServerAddress = server.DirectConnectionServerAddress ?? string.Empty,
                DirectConnectionServerPort = server.DirectConnectionServerPort <= 0 ? 7777 : server.DirectConnectionServerPort,
                DirectConnectionProxyAddress = string.IsNullOrEmpty(server.DirectConnectionProxyAddress) ? "0.0.0.0" : server.DirectConnectionProxyAddress
            };
        }

        private static WorldEditableState ExtractWorld(WorldDescriptionDocument document, string gameVersion, string folderName, string filePath)
        {
            var world = document.WorldDescription;
            if (world.WorldSettings == null)
            {
                world.WorldSettings = new WorldSettings();
            }
            if (world.WorldSettings.BoolParameters == null)
            {
                world.WorldSettings.BoolParameters = new Dictionary<string, bool>();
            }
            if (world.WorldSettings.FloatParameters == null)
            {
                world.WorldSettings.FloatParameters = new Dictionary<string, double>();
            }
            if (world.WorldSettings.TagParameters == null)
            {
                world.WorldSettings.TagParameters = new Dictionary<string, TagParameterValue>();
            }

            return new WorldEditableState
            {
                Key = gameVersion + ":" + folderName,
                GameVersion = gameVersion,
                FolderName = folderName,
                FilePath = filePath,
                Version = document.Version,
                IslandId = world.islandId ?? string.Empty,
                WorldName = world.WorldName ?? folderName,
                CreationTime = world.CreationTime,
                WorldPresetType = world.WorldPresetType ?? "Medium",
                SharedQuests = GetBool(world.WorldSettings.BoolParameters, SharedQuestsKey, true),
                EasyExplore = GetBool(world.WorldSettings.BoolParameters, EasyExploreKey, false),
                MobHealthMultiplier = GetDouble(world.WorldSettings.FloatParameters, MobHealthKey, 1),
                MobDamageMultiplier = GetDouble(world.WorldSettings.FloatParameters, MobDamageKey, 1),
                ShipsHealthMultiplier = GetDouble(world.WorldSettings.FloatParameters, ShipsHealthKey, 1),
                ShipsDamageMultiplier = GetDouble(world.WorldSettings.FloatParameters, ShipsDamageKey, 1),
                BoardingDifficultyMultiplier = GetDouble(world.WorldSettings.FloatParameters, BoardingKey, 1),
                CoopStatsCorrectionModifier = GetDouble(world.WorldSettings.FloatParameters, CoopStatsKey, 1),
                CoopShipStatsCorrectionModifier = GetDouble(world.WorldSettings.FloatParameters, CoopShipStatsKey, 0),
                CombatDifficulty = GetCombatTag(world.WorldSettings.TagParameters)
            };
        }

        private static void ApplyServer(ServerDescriptionDocument document, ServerEditableState state)
        {
            document.ServerDescription_Persistent.InviteCode = state.InviteCode;
            document.ServerDescription_Persistent.Password = state.Password;
            document.ServerDescription_Persistent.IsPasswordProtected = !string.IsNullOrEmpty(state.Password);
            document.ServerDescription_Persistent.ServerName = state.ServerName;
            document.ServerDescription_Persistent.WorldIslandId = state.WorldIslandId;
            document.ServerDescription_Persistent.MaxPlayerCount = state.MaxPlayerCount;
            document.ServerDescription_Persistent.UserSelectedRegion = state.UserSelectedRegion;
            document.ServerDescription_Persistent.P2pProxyAddress = state.P2pProxyAddress;
            document.ServerDescription_Persistent.UseDirectConnection = state.UseDirectConnection;
            document.ServerDescription_Persistent.DirectConnectionServerAddress = state.DirectConnectionServerAddress;
            document.ServerDescription_Persistent.DirectConnectionServerPort = state.DirectConnectionServerPort;
            document.ServerDescription_Persistent.DirectConnectionProxyAddress = state.DirectConnectionProxyAddress;
        }

        private static void ApplyWorld(WorldDescriptionDocument document, WorldEditableState state)
        {
            document.WorldDescription.WorldName = state.WorldName;
            document.WorldDescription.WorldPresetType = DetectPreset(state);

            if (document.WorldDescription.WorldSettings == null)
            {
                document.WorldDescription.WorldSettings = new WorldSettings();
            }
            if (document.WorldDescription.WorldSettings.BoolParameters == null)
            {
                document.WorldDescription.WorldSettings.BoolParameters = new Dictionary<string, bool>();
            }
            if (document.WorldDescription.WorldSettings.FloatParameters == null)
            {
                document.WorldDescription.WorldSettings.FloatParameters = new Dictionary<string, double>();
            }
            if (document.WorldDescription.WorldSettings.TagParameters == null)
            {
                document.WorldDescription.WorldSettings.TagParameters = new Dictionary<string, TagParameterValue>();
            }

            document.WorldDescription.WorldSettings.BoolParameters[SharedQuestsKey] = state.SharedQuests;
            document.WorldDescription.WorldSettings.BoolParameters[EasyExploreKey] = state.EasyExplore;
            document.WorldDescription.WorldSettings.FloatParameters[MobHealthKey] = state.MobHealthMultiplier;
            document.WorldDescription.WorldSettings.FloatParameters[MobDamageKey] = state.MobDamageMultiplier;
            document.WorldDescription.WorldSettings.FloatParameters[ShipsHealthKey] = state.ShipsHealthMultiplier;
            document.WorldDescription.WorldSettings.FloatParameters[ShipsDamageKey] = state.ShipsDamageMultiplier;
            document.WorldDescription.WorldSettings.FloatParameters[BoardingKey] = state.BoardingDifficultyMultiplier;
            document.WorldDescription.WorldSettings.FloatParameters[CoopStatsKey] = state.CoopStatsCorrectionModifier;
            document.WorldDescription.WorldSettings.FloatParameters[CoopShipStatsKey] = state.CoopShipStatsCorrectionModifier;
            document.WorldDescription.WorldSettings.TagParameters[CombatKey] = new TagParameterValue { TagName = state.CombatDifficulty };
        }

        private static void CreateBackup(string filePath)
        {
            var stamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var backupPath = filePath + "." + stamp + ".bak";
            File.Copy(filePath, backupPath, true);
        }

        private static string FindSelectedWorldKey(string activeWorldId, List<WorldEditableState> worlds)
        {
            var exact = worlds.FirstOrDefault(delegate(WorldEditableState world)
            {
                return world.FolderName == activeWorldId && world.IslandId == activeWorldId;
            });
            if (exact != null)
            {
                return exact.Key;
            }

            var folderMatch = worlds.FirstOrDefault(delegate(WorldEditableState world)
            {
                return world.FolderName == activeWorldId;
            });
            if (folderMatch != null)
            {
                return folderMatch.Key;
            }

            return worlds.Count > 0 ? worlds[0].Key : null;
        }

        private static string FindFirstWorldFolderId(string root)
        {
            var rocksDbRoot = Path.Combine(root, "R5", "Saved", "SaveProfiles", "Default", "RocksDB");
            if (!Directory.Exists(rocksDbRoot))
            {
                return string.Empty;
            }

            foreach (var versionDir in Directory.GetDirectories(rocksDbRoot).OrderByDescending(delegate(string item) { return item; }))
            {
                var worldsRoot = Path.Combine(versionDir, "Worlds");
                if (!Directory.Exists(worldsRoot))
                {
                    continue;
                }

                var worldDir = Directory.GetDirectories(worldsRoot).FirstOrDefault();
                if (!string.IsNullOrEmpty(worldDir))
                {
                    return Path.GetFileName(worldDir);
                }
            }

            return string.Empty;
        }

        private static string FindLatestVersionName(string root)
        {
            var rocksDbRoot = Path.Combine(root, "R5", "Saved", "SaveProfiles", "Default", "RocksDB");
            if (!Directory.Exists(rocksDbRoot))
            {
                return string.Empty;
            }

            return Directory
                .GetDirectories(rocksDbRoot)
                .Select(Path.GetFileName)
                .OrderByDescending(delegate(string item) { return item; }, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault() ?? string.Empty;
        }

        private static string GetVersionNameFromWorldDirectory(string worldDir)
        {
            var worldsDir = Path.GetDirectoryName(worldDir);
            if (string.IsNullOrWhiteSpace(worldsDir))
            {
                return string.Empty;
            }

            var versionDir = Path.GetDirectoryName(worldsDir);
            if (string.IsNullOrWhiteSpace(versionDir))
            {
                return string.Empty;
            }

            return Path.GetFileName(versionDir) ?? string.Empty;
        }

        private static void CopyDirectoryRecursive(string sourceDir, string destinationDir)
        {
            Directory.CreateDirectory(destinationDir);
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                File.Copy(file, Path.Combine(destinationDir, Path.GetFileName(file)), true);
            }

            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                CopyDirectoryRecursive(dir, Path.Combine(destinationDir, Path.GetFileName(dir)));
            }
        }

        private static bool GetBool(Dictionary<string, bool> values, string key, bool fallback)
        {
            bool value;
            return values.TryGetValue(key, out value) ? value : fallback;
        }

        private static double GetDouble(Dictionary<string, double> values, string key, double fallback)
        {
            double value;
            return values.TryGetValue(key, out value) ? value : fallback;
        }

        private static string GetCombatTag(Dictionary<string, TagParameterValue> values)
        {
            TagParameterValue value;
            return values.TryGetValue(CombatKey, out value) && value != null && !string.IsNullOrEmpty(value.TagName)
                ? value.TagName
                : CombatNormalTag;
        }

        private static bool WorldMatchesPreset(WorldEditableState world, string presetName)
        {
            if (presetName == "Easy")
            {
                return world.SharedQuests &&
                       !world.EasyExplore &&
                       world.MobHealthMultiplier == 0.7 &&
                       world.MobDamageMultiplier == 0.6 &&
                       world.ShipsHealthMultiplier == 0.7 &&
                       world.ShipsDamageMultiplier == 0.6 &&
                       world.BoardingDifficultyMultiplier == 0.7 &&
                       world.CoopStatsCorrectionModifier == 1 &&
                       world.CoopShipStatsCorrectionModifier == 0 &&
                       world.CombatDifficulty == CombatEasyTag;
            }

            if (presetName == "Hard")
            {
                return world.SharedQuests &&
                       !world.EasyExplore &&
                       world.MobHealthMultiplier == 1.5 &&
                       world.MobDamageMultiplier == 1.25 &&
                       world.ShipsHealthMultiplier == 1.5 &&
                       world.ShipsDamageMultiplier == 1.25 &&
                       world.BoardingDifficultyMultiplier == 1.5 &&
                       world.CoopStatsCorrectionModifier == 1 &&
                       world.CoopShipStatsCorrectionModifier == 0 &&
                       world.CombatDifficulty == CombatHardTag;
            }

            return world.SharedQuests &&
                   !world.EasyExplore &&
                   world.MobHealthMultiplier == 1 &&
                   world.MobDamageMultiplier == 1 &&
                   world.ShipsHealthMultiplier == 1 &&
                   world.ShipsDamageMultiplier == 1 &&
                   world.BoardingDifficultyMultiplier == 1 &&
                   world.CoopStatsCorrectionModifier == 1 &&
                   world.CoopShipStatsCorrectionModifier == 0 &&
                   world.CombatDifficulty == CombatNormalTag;
        }

        private static string PrettyJson(string rawJson)
        {
            var escaped = false;
            var inQuotes = false;
            var sb = new StringBuilder();
            var indentation = 0;

            for (var i = 0; i < rawJson.Length; i++)
            {
                var ch = rawJson[i];
                switch (ch)
                {
                    case '"':
                        sb.Append(ch);
                        if (!escaped)
                        {
                            inQuotes = !inQuotes;
                        }
                        escaped = false;
                        break;
                    case '\\':
                        sb.Append(ch);
                        escaped = !escaped;
                        break;
                    case '{':
                    case '[':
                        sb.Append(ch);
                        if (!inQuotes)
                        {
                            sb.AppendLine();
                            indentation++;
                            sb.Append(new string(' ', indentation * 2));
                        }
                        escaped = false;
                        break;
                    case '}':
                    case ']':
                        if (!inQuotes)
                        {
                            sb.AppendLine();
                            indentation--;
                            sb.Append(new string(' ', indentation * 2));
                            sb.Append(ch);
                        }
                        else
                        {
                            sb.Append(ch);
                        }
                        escaped = false;
                        break;
                    case ',':
                        sb.Append(ch);
                        if (!inQuotes)
                        {
                            sb.AppendLine();
                            sb.Append(new string(' ', indentation * 2));
                        }
                        escaped = false;
                        break;
                    case ':':
                        sb.Append(ch);
                        if (!inQuotes)
                        {
                            sb.Append(' ');
                        }
                        escaped = false;
                        break;
                    default:
                        sb.Append(ch);
                        escaped = false;
                        break;
                }
            }

            return sb.ToString() + Environment.NewLine;
        }

        private static int MoveDirectoryIfExists(string sourcePath, string destinationPath)
        {
            if (!Directory.Exists(sourcePath))
            {
                return 0;
            }

            Directory.Move(sourcePath, destinationPath);
            return 1;
        }
    }

}
