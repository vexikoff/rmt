using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Timers;
using Microsoft.Win32;
using Timer = System.Timers.Timer;

namespace RustMeTweaker;

public class MainViewModel
{
    private const double BytesPerGb = 1073741824.0;
    private const double AfOff = 0.0;
    private const double AfOn = 2.0;
    private const int SystemMemoryListInformation = 80;
    private const int MemoryPurgeStandbyList = 4;
    private const uint TokenAdjustPrivileges = 0x0020;
    private const uint TokenQuery = 0x0008;
    private const uint SePrivilegeEnabled = 0x00000002;
    private const int ErrorNotAllAssigned = 1300;
    private const int OfnExplorer = 0x00080000;
    private const int OfnNoChangeDir = 0x00000008;
    private const int OfnOverwritePrompt = 0x00000002;
    private const int OfnFileMustExist = 0x00001000;
    private const int OfnPathMustExist = 0x00000800;

    public static event Action<bool>? AutoCrunkToggled;

    public static MainViewModel? Current { get; private set; }

    private static readonly JsonSerializerOptions StateOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly JsonSerializerOptions FileOptions = new()
    {
        WriteIndented = true
    };

    private static readonly string[] DefaultCrunkBlacklist =
    {
        "chrome", "firefox", "msedge", "brave", "opera", "vivaldi",
        "steam", "steamwebhelper", "discord", "telegram", "spotify"
    };

    private static readonly string[] SoundKeys =
    {
        "master_volume", "music_volume", "ambient_volume", "blocks_volume",
        "hostile_volume", "neutral_volume", "players_volume", "records_volume",
        "weather_volume", "interface_volume", "entities_volume",
        "explosions_volume", "weapons_volume"
    };

    private static readonly string[] CopyGraphicKeys =
    {
        "af_level", "aa_level", "render_distance", "mipmap_levels", "minimap_quality",
        "new_render", "render_regions", "biomes_quality", "fog_quality", "light_quality",
        "math_quality", "particles_quality", "sky_quality", "tree_quality"
    };

    private static readonly string[] CopyGeneralKeys =
    {
        "fov", "dynamic_fov", "show_tips", "snooper", "freezing_overlay",
        "bleeding_overlay", "debug_mode", "show_fps", "camera_shake"
    };

    private static readonly string[] CopyScreenKeys =
    {
        "fps_limit", "v_sync"
    };

    private static readonly string[] CopyCrosshairKeys =
    {
        "enabled", "style", "color", "opacity", "gap", "line_length", "line_width",
        "dot_size", "outline", "outline_color", "outline_width",
        "dynamic_spacing", "dynamic_visibility"
    };

    private string _launcherDir = "";
    private string _rootPath = "";
    private string _profilePath = "";
    private string _profileName = "";
    private JsonObject? _root;
    private JsonObject? _profile;
    private bool _found;
    private ulong _totalRamBytes;
    private static bool _crunk;
    private Timer? _crunkTimer;
    private bool _autoBackup;
    private int _crunkPriority = 2;
    private int _crunkIntervalMs = 5000;
    private List<string> _crunkBlacklist = new(DefaultCrunkBlacklist);
    private string _customLauncherDir = "";
    private string _customJavaPath = "";
    private bool _standbyClear;
    private bool _gameWasRunning;

    public static bool CrunkOn => _crunk;

    private string BackupsDir => Path.Combine(_launcherDir, "backups");

    private string GameProfilesDir => Path.Combine(_launcherDir, "profiles");

    private string ProfilesDir => Path.Combine(Path.GetDirectoryName(TweakerPath)!, "profiles");

    private string TweakerPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RustMeTweaker",
        "settings.json"
    );

    public void Initialize()
    {
        Current = this;
        _totalRamBytes = GetTotalRam();
        LoadTweakerConfig();
        if (_standbyClear && !TryEnableStandbyPrivilege())
        {
            _standbyClear = false;
            SaveTweakerConfig();
        }
        _found = Locate();
        if (_found)
        {
            Load();
        }
    }

    public void ShutdownCrunk()
    {
        StopCrunkTimer();
        if (_crunk)
        {
            SetCrunkPriorities(false);
            _crunk = false;
        }
    }

    private static List<string> GameProfileNames(string profilesDir)
    {
        List<string> list = new();
        if (!Directory.Exists(profilesDir))
        {
            return list;
        }
        foreach (string dir in Directory.GetDirectories(profilesDir).OrderBy(d => Path.GetFileName(d), StringComparer.OrdinalIgnoreCase))
        {
            if (File.Exists(Path.Combine(dir, "settings.json")))
            {
                list.Add(Path.GetFileName(dir));
            }
        }
        return list;
    }

    private string? ResolveProfileDir(string launcherDir)
    {
        string dir = Path.Combine(launcherDir, "profiles");
        List<string> names = GameProfileNames(dir);
        if (names.Count == 0)
        {
            return null;
        }
        if (_profileName.Length > 0 && names.Contains(_profileName))
        {
            return Path.Combine(dir, _profileName);
        }
        if (names.Contains("prod-a"))
        {
            return Path.Combine(dir, "prod-a");
        }
        return Path.Combine(dir, names[0]);
    }

    private bool Locate()
    {
        string roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        List<string> candidates = new();
        if (_customLauncherDir.Length > 0)
        {
            candidates.Add(_customLauncherDir);
        }
        candidates.Add(Path.Combine(roaming, "rustme-launcher"));
        candidates.Add(Path.Combine(local, "rustme-launcher"));

        foreach (string candidate in candidates)
        {
            string root = Path.Combine(candidate, "settings.json");
            if (!Directory.Exists(candidate) || !File.Exists(root))
            {
                continue;
            }
            string? profileDir = ResolveProfileDir(candidate);
            if (profileDir == null)
            {
                continue;
            }
            _launcherDir = candidate;
            _rootPath = root;
            _profileName = Path.GetFileName(profileDir);
            _profilePath = Path.Combine(profileDir, "settings.json");
            return true;
        }

        return false;
    }

    private void Load()
    {
        _root = JsonNode.Parse(File.ReadAllText(_rootPath)) as JsonObject;
        _profile = JsonNode.Parse(File.ReadAllText(_profilePath)) as JsonObject;
        if (_root == null || _profile == null)
        {
            _found = false;
        }
    }

    private void LoadTweakerConfig()
    {
        if (!File.Exists(TweakerPath))
        {
            return;
        }
        JsonObject? node = null;
        try
        {
            node = JsonNode.Parse(File.ReadAllText(TweakerPath)) as JsonObject;
        }
        catch
        {
            node = null;
        }
        if (node == null)
        {
            return;
        }
        _autoBackup = ReadJsonBool(node, "autoBackup", false);
        _standbyClear = ReadJsonBool(node, "standbyClear", false);
        _crunkPriority = Math.Clamp(ReadJsonInt(node, "crunkPriority", 2), 0, 2);
        _crunkIntervalMs = Math.Clamp(ReadJsonInt(node, "crunkIntervalMs", 5000), 2000, 20000);
        _customLauncherDir = ReadJsonString(node, "customLauncherDir", "");
        _customJavaPath = ReadJsonString(node, "customJavaPath", "");
        _profileName = ReadJsonString(node, "activeProfile", "");
        if (node["crunkBlacklist"] is JsonArray arr)
        {
            List<string> list = new();
            foreach (JsonNode? item in arr)
            {
                if (item is JsonValue jv && jv.TryGetValue<string>(out string? raw) && raw != null)
                {
                    string norm = NormalizeProcessName(raw);
                    if (norm.Length > 0)
                    {
                        list.Add(norm);
                    }
                }
            }
            _crunkBlacklist = list;
        }
    }

    private void SaveTweakerConfig()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(TweakerPath)!);
        JsonArray bl = new();
        foreach (string name in _crunkBlacklist)
        {
            bl.Add(name);
        }
        JsonObject node = new()
        {
            ["autoBackup"] = _autoBackup,
            ["standbyClear"] = _standbyClear,
            ["crunkPriority"] = _crunkPriority,
            ["crunkIntervalMs"] = _crunkIntervalMs,
            ["crunkBlacklist"] = bl,
            ["customLauncherDir"] = _customLauncherDir,
            ["customJavaPath"] = _customJavaPath,
            ["activeProfile"] = _profileName
        };
        File.WriteAllText(TweakerPath, node.ToJsonString(FileOptions));
    }

    public string HandleMessage(string request)
    {
        int id = 0;
        try
        {
            JsonObject root = JsonNode.Parse(request) as JsonObject ?? throw new InvalidOperationException("bad request");
            id = ReadJsonInt(root, "id", 0);
            string action = ReadJsonString(root, "action", "");
            JsonObject payload = root["payload"] as JsonObject ?? new JsonObject();

            object? data = action switch
            {
                "getState" => GetState(),
                "setAf" => SetAf(payload["disabled"]?.GetValue<bool>() ?? false),
                "setProfile" => SetProfile(payload["key"]?.GetValue<string>() ?? "", payload["value"]),
                "setLauncher" => SetLauncher(payload["key"]?.GetValue<string>() ?? "", payload["value"]),
                "setTweakBool" => SetTweakBool(payload["key"]?.GetValue<string>() ?? "", payload["value"]?.GetValue<bool>() ?? false),
                "setMinimapHigh" => SetMinimapHigh(payload["high"]?.GetValue<bool>() ?? false),
                "setSoundVolume" => SetSoundVolume(payload["key"]?.GetValue<string>() ?? "", payload["value"]?.GetValue<double>() ?? 0.0),
                "setCrosshair" => SetCrosshair(payload["key"]?.GetValue<string>() ?? "", payload["value"]),
                "setGameProfile" => SetGameProfile(payload["name"]?.GetValue<string>() ?? ""),
                "copyProfileToAll" => CopyProfileToAll(),
                "applyPreset" => ApplyPreset(payload["preset"]?.GetValue<string>() ?? ""),
                "applyGeneralPreset" => ApplyGeneralPreset(payload["preset"]?.GetValue<string>() ?? ""),
                "saveSettings" => SaveSettings(),
                "createBackup" => CreateBackup(),
                "listBackups" => ListBackups(),
                "restoreBackup" => RestoreBackup(payload["name"]?.GetValue<string>() ?? ""),
                "deleteBackup" => DeleteBackup(payload["name"]?.GetValue<string>() ?? ""),
                "setKeybind" => SetKeybind(payload["key"]?.GetValue<string>() ?? "", payload["value"]),
                "setAutoCrunk" => SetAutoCrunk(payload["enabled"]?.GetValue<bool>() ?? false),
                "setAutoBackup" => SetAutoBackup(payload["enabled"]?.GetValue<bool>() ?? false),
                "setCrunkPriority" => SetCrunkPriority(payload["priority"]?.GetValue<int>() ?? 2),
                "setCrunkInterval" => SetCrunkInterval(payload["ms"]?.GetValue<int>() ?? 5000),
                "setCrunkBlacklist" => SetCrunkBlacklist(payload["list"]),
                "setStandbyClear" => SetStandbyClear(payload["enabled"]?.GetValue<bool>() ?? false),
                "listProfileSlots" => ListProfileSlots(),
                "saveProfileSlot" => SaveProfileSlot(payload["slot"]?.GetValue<int>() ?? 0, payload["name"]?.GetValue<string>() ?? ""),
                "applyProfileSlot" => ApplyProfileSlot(payload["slot"]?.GetValue<int>() ?? 0),
                "deleteProfileSlot" => DeleteProfileSlot(payload["slot"]?.GetValue<int>() ?? 0),
                "pickExportPath" => PickProfilePath(true, payload["name"]?.GetValue<string>() ?? ""),
                "pickImportPath" => PickProfilePath(false, ""),
                "exportProfile" => ExportProfile(payload["path"]?.GetValue<string>() ?? "", payload["name"]?.GetValue<string>() ?? ""),
                "importProfile" => ImportProfile(payload["path"]?.GetValue<string>() ?? ""),
                "setCustomPaths" => SetCustomPaths(payload["launcherDir"]?.GetValue<string>() ?? "", payload["javaPath"]?.GetValue<string>() ?? ""),
                "optimize" => Optimize(),
                "megaoptimize" => MegaOptimize(),
                "rollback" => Rollback(),
                _ => throw new InvalidOperationException("unknown action")
            };

            return JsonSerializer.Serialize(new { id, ok = true, data }, StateOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { id, ok = false, error = ex.Message }, StateOptions);
        }
    }

    private object ListSoundVolumes()
    {
        List<object> list = new();
        if (_profile == null)
        {
            return list;
        }
        foreach (string key in SoundKeys)
        {
            if (HasKey(_profile, "sound_settings", key))
            {
                list.Add(new
                {
                    key,
                    value = ReadDouble(_profile, "sound_settings", key)
                });
            }
        }
        return list;
    }

    private object ListGameProfiles()
    {
        List<object> list = new();
        if (!_found)
        {
            return list;
        }
        foreach (string name in GameProfileNames(GameProfilesDir))
        {
            list.Add(new
            {
                name,
                active = string.Equals(name, _profileName, StringComparison.OrdinalIgnoreCase)
            });
        }
        return list;
    }

    private object SetGameProfile(string name)
    {
        if (!_found)
        {
            throw new InvalidOperationException("RustMe не найден");
        }
        string clean = name.Trim();
        if (clean.Length == 0 || clean != Path.GetFileName(clean))
        {
            throw new InvalidOperationException("bad profile");
        }
        string path = Path.Combine(GameProfilesDir, clean, "settings.json");
        if (!File.Exists(path))
        {
            throw new InvalidOperationException("Профиль не найден");
        }
        _profileName = clean;
        _profilePath = path;
        SaveTweakerConfig();
        Load();
        return GetState();
    }

    private object CopyProfileToAll()
    {
        EnsureWritable();
        if (!Directory.Exists(GameProfilesDir))
        {
            throw new InvalidOperationException("Папка profiles не найдена");
        }
        MakeBackup();
        foreach (string name in GameProfileNames(GameProfilesDir))
        {
            if (string.Equals(name, _profileName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            string path = Path.Combine(GameProfilesDir, name, "settings.json");
            JsonObject? target = JsonNode.Parse(File.ReadAllText(path)) as JsonObject;
            if (target == null)
            {
                continue;
            }
            CopySection(_profile!, target, "graphic_settings", CopyGraphicKeys);
            CopySection(_profile!, target, "general_settings", CopyGeneralKeys);
            CopySection(_profile!, target, "screen_settings", CopyScreenKeys);
            CopySection(_profile!, target, "sound_settings", SoundKeys);
            CopySection(_profile!, target, "crosshair_settings", CopyCrosshairKeys);
            CopyBindings(_profile!, target);
            Save(target, path);
        }
        return GetState();
    }

    private static JsonNode? CloneNode(JsonNode? node)
    {
        if (node == null)
        {
            return null;
        }
        return JsonNode.Parse(node.ToJsonString());
    }

    private static void CopySection(JsonObject source, JsonObject target, string section, string[] keys)
    {
        if (source[section] is not JsonObject src)
        {
            return;
        }
        if (target[section] is not JsonObject dst)
        {
            return;
        }
        foreach (string key in keys)
        {
            if (!src.ContainsKey(key) || !dst.ContainsKey(key))
            {
                continue;
            }
            dst[key] = CloneNode(src[key]);
        }
    }

    private static void CopyBindings(JsonObject source, JsonObject target)
    {
        if (source["keybinding_settings"] is not JsonObject src)
        {
            return;
        }
        if (target["keybinding_settings"] is not JsonObject dst)
        {
            return;
        }
        foreach (KeyValuePair<string, JsonNode?> pair in src)
        {
            if (pair.Value is JsonValue jv && dst.ContainsKey(pair.Key))
            {
                if (jv.TryGetValue<int>(out int code))
                {
                    dst[pair.Key] = JsonValue.Create(code);
                }
                else if (jv.TryGetValue<double>(out double number))
                {
                    dst[pair.Key] = JsonValue.Create((int)Math.Round(number));
                }
            }
        }
    }

    private object GetState()
    {
        bool ready = _found && _root != null && _profile != null;
        return new
        {
            found = ready,
            launcherDir = ready ? _launcherDir : "",
            account = ready ? ReadJsonString(_root!, "selectedAccount", "") : "",
            running = ready && IsClientRunning(),
            autoCrunk = _crunk,
            autoBackup = _autoBackup,
            standbyClear = _standbyClear,
            crunkPriority = _crunkPriority,
            crunkIntervalMs = _crunkIntervalMs,
            crunkBlacklist = _crunkBlacklist,
            customLauncherDir = _customLauncherDir,
            customJavaPath = _customJavaPath,
            activeProfile = _profileName,
            profiles = ListGameProfiles(),
            totalRamGb = _totalRamBytes / BytesPerGb,
            soundVolumes = ListSoundVolumes(),
            profile = BuildProfileState(),
            launcher = BuildLauncherState(),
            bindings = GetBindings()
        };
    }

    private object BuildProfileState()
    {
        JsonObject profile = _profile ?? new JsonObject();
        return new
        {
            afLevel = ReadDouble(profile, "graphic_settings", "af_level"),
            renderDistance = ReadDouble(profile, "graphic_settings", "render_distance"),
            mipmapLevels = ReadDouble(profile, "graphic_settings", "mipmap_levels"),
            minimapHigh = ReadInt(profile, "graphic_settings", "minimap_quality") == 1,
            newRender = ReadBool(profile, "graphic_settings", "new_render"),
            renderRegions = ReadBool(profile, "graphic_settings", "render_regions"),
            showTips = ReadBool(profile, "general_settings", "show_tips"),
            snooper = ReadBool(profile, "general_settings", "snooper"),
            freezingOverlay = ReadBool(profile, "general_settings", "freezing_overlay"),
            bleedingOverlay = ReadBool(profile, "general_settings", "bleeding_overlay"),
            debugGame = ReadBool(profile, "general_settings", "debug_mode"),
            fpsLimit = ReadDouble(profile, "screen_settings", "fps_limit"),
            vSync = ReadBool(profile, "screen_settings", "v_sync"),
            fov = ReadDouble(profile, "general_settings", "fov"),
            showFps = ReadBool(profile, "general_settings", "show_fps"),
            cameraShake = ReadBool(profile, "general_settings", "camera_shake"),
            hasCameraShake = HasKey(profile, "general_settings", "camera_shake"),
            hasCrosshair = profile["crosshair_settings"] is JsonObject,
            crosshair = new
            {
                enabled = ReadBool(profile, "crosshair_settings", "enabled"),
                style = ReadString(profile, "crosshair_settings", "style"),
                color = ReadInt(profile, "crosshair_settings", "color"),
                outlineColor = ReadInt(profile, "crosshair_settings", "outline_color"),
                opacity = ReadDouble(profile, "crosshair_settings", "opacity"),
                gap = ReadDouble(profile, "crosshair_settings", "gap"),
                lineLength = ReadDouble(profile, "crosshair_settings", "line_length"),
                lineWidth = ReadDouble(profile, "crosshair_settings", "line_width"),
                dotSize = ReadDouble(profile, "crosshair_settings", "dot_size"),
                outline = ReadBool(profile, "crosshair_settings", "outline"),
                outlineWidth = ReadDouble(profile, "crosshair_settings", "outline_width"),
                dynamicSpacing = ReadBool(profile, "crosshair_settings", "dynamic_spacing"),
                dynamicVisibility = ReadBool(profile, "crosshair_settings", "dynamic_visibility")
            }
        };
    }

    private object BuildLauncherState()
    {
        JsonObject settings = _root?["gameSettings"] as JsonObject ?? new JsonObject();
        return new
        {
            memoryGb = ReadJsonDouble(settings, "maxMemoryBytes", 0.0) / BytesPerGb,
            fullScreen = ReadJsonBool(settings, "isFullScreenEnabled", false),
            debug = ReadJsonBool(settings, "isDebugEnabled", false)
        };
    }

    private object GetBindings()
    {
        List<object> list = new();
        if (_profile?["keybinding_settings"] is JsonObject kb)
        {
            foreach (KeyValuePair<string, JsonNode?> pair in kb)
            {
                if (pair.Value is JsonValue jv)
                {
                    if (jv.TryGetValue<int>(out int code))
                    {
                        list.Add(new { key = pair.Key, code });
                    }
                    else if (jv.TryGetValue<double>(out double number))
                    {
                        list.Add(new { key = pair.Key, code = (int)Math.Round(number) });
                    }
                }
            }
        }
        return list;
    }

    private void MaybeBackup()
    {
        if (_autoBackup)
        {
            MakeBackup();
        }
    }

    private object SetAf(bool disabled)
    {
        EnsureWritable();
        MaybeBackup();
        SetSectionValue(_profile!, "graphic_settings", "af_level", JsonValue.Create(disabled ? AfOff : AfOn));
        Save(_profile!, _profilePath);
        return GetState();
    }

    private object SetProfile(string key, JsonNode? value)
    {
        EnsureWritable();
        if (value == null)
        {
            throw new InvalidOperationException("no value");
        }
        MaybeBackup();

        switch (key)
        {
            case "render_distance":
                SetSectionValue(_profile!, "graphic_settings", key, JsonValue.Create(value.GetValue<double>()));
                break;
            case "mipmap_levels":
                SetSectionValue(_profile!, "graphic_settings", key, JsonValue.Create(value.GetValue<double>()));
                break;
            case "fps_limit":
                SetSectionValue(_profile!, "screen_settings", key, JsonValue.Create(value.GetValue<double>()));
                break;
            case "v_sync":
                SetSectionValue(_profile!, "screen_settings", key, JsonValue.Create(value.GetValue<bool>()));
                break;
            case "fov":
                SetSectionValue(_profile!, "general_settings", key, JsonValue.Create(Math.Clamp(value.GetValue<double>(), 70.0, 90.0)));
                break;
            case "show_fps":
                SetSectionValue(_profile!, "general_settings", key, JsonValue.Create(value.GetValue<bool>()));
                break;
            default:
                throw new InvalidOperationException("key not allowed");
        }

        Save(_profile!, _profilePath);
        return GetState();
    }

    private object SetLauncher(string key, JsonNode? value)
    {
        EnsureWritable();
        if (value == null)
        {
            throw new InvalidOperationException("no value");
        }
        MaybeBackup();

        JsonObject gameSettings = EnsureSection(_root!, "gameSettings");
        switch (key)
        {
            case "memory_gb":
                long bytes = (long)(value.GetValue<double>() * BytesPerGb);
                long min = 1073741824L;
                long max = (long)_totalRamBytes - 1073741824L;
                if (max < min)
                {
                    max = min;
                }
                gameSettings["maxMemoryBytes"] = JsonValue.Create(Math.Clamp(bytes, min, max));
                break;
            case "full_screen":
                gameSettings["isFullScreenEnabled"] = JsonValue.Create(value.GetValue<bool>());
                break;
            case "debug":
                gameSettings["isDebugEnabled"] = JsonValue.Create(value.GetValue<bool>());
                break;
            default:
                throw new InvalidOperationException("key not allowed");
        }

        Save(_root!, _rootPath);
        return GetState();
    }

    private object SetTweakBool(string key, bool value)
    {
        EnsureWritable();
        MaybeBackup();

        switch (key)
        {
            case "new_render":
                SetSectionValue(_profile!, "graphic_settings", key, JsonValue.Create(value));
                break;
            case "render_regions":
                SetSectionValue(_profile!, "graphic_settings", key, JsonValue.Create(value));
                break;
            case "show_tips":
                SetSectionValue(_profile!, "general_settings", key, JsonValue.Create(value));
                break;
            case "snooper":
                SetSectionValue(_profile!, "general_settings", key, JsonValue.Create(value));
                break;
            case "freezing_overlay":
                SetSectionValue(_profile!, "general_settings", key, JsonValue.Create(value));
                break;
            case "bleeding_overlay":
                SetSectionValue(_profile!, "general_settings", key, JsonValue.Create(value));
                break;
            case "debug_mode":
                SetSectionValue(_profile!, "general_settings", key, JsonValue.Create(value));
                break;
            case "camera_shake":
                RequireKey(_profile!, "general_settings", key);
                SetSectionValue(_profile!, "general_settings", key, JsonValue.Create(value));
                break;
            default:
                throw new InvalidOperationException("key not allowed");
        }

        Save(_profile!, _profilePath);
        return GetState();
    }

    private object SetMinimapHigh(bool high)
    {
        EnsureWritable();
        MaybeBackup();
        SetSectionValue(_profile!, "graphic_settings", "minimap_quality", JsonValue.Create(high ? 1 : 0));
        Save(_profile!, _profilePath);
        return GetState();
    }

    private object SetSoundVolume(string key, double value)
    {
        EnsureWritable();
        if (!Array.Exists(SoundKeys, k => string.Equals(k, key, StringComparison.Ordinal)))
        {
            throw new InvalidOperationException("key not allowed");
        }
        RequireKey(_profile!, "sound_settings", key);
        MaybeBackup();
        SetSectionValue(_profile!, "sound_settings", key, JsonValue.Create(Math.Clamp(value, 0.0, 1.0)));
        Save(_profile!, _profilePath);
        return GetState();
    }

    private object SetCrosshair(string key, JsonNode? value)
    {
        EnsureWritable();
        if (value == null)
        {
            throw new InvalidOperationException("no value");
        }
        if (_profile!["crosshair_settings"] is not JsonObject)
        {
            throw new InvalidOperationException("В этом профиле нет настроек прицела");
        }
        RequireKey(_profile!, "crosshair_settings", key);
        MaybeBackup();

        switch (key)
        {
            case "enabled":
            case "outline":
            case "dynamic_spacing":
            case "dynamic_visibility":
                SetSectionValue(_profile!, "crosshair_settings", key, JsonValue.Create(value.GetValue<bool>()));
                break;
            case "style":
                if (value is not JsonValue sv || !sv.TryGetValue<string>(out string? style) || style == null || !IsValidStyle(style))
                {
                    throw new InvalidOperationException("bad style");
                }
                SetSectionValue(_profile!, "crosshair_settings", key, JsonValue.Create(style));
                break;
            case "opacity":
            case "gap":
            case "line_length":
            case "line_width":
            case "outline_width":
            case "dot_size":
                SetSectionValue(_profile!, "crosshair_settings", key, JsonValue.Create(ClampCrosshairNumber(key, value.GetValue<double>())));
                break;
            case "color":
            case "outline_color":
                SetSectionValue(_profile!, "crosshair_settings", key, JsonValue.Create(Math.Clamp(JsonNodeInt(value), 0, 16777215)));
                break;
            default:
                throw new InvalidOperationException("key not allowed");
        }

        Save(_profile!, _profilePath);
        return GetState();
    }

    private static bool IsValidStyle(string style)
    {
        if (style.Length == 0 || style.Length > 32)
        {
            return false;
        }
        foreach (char c in style)
        {
            if (!char.IsLetterOrDigit(c))
            {
                return false;
            }
        }
        return true;
    }

    private static double ClampCrosshairNumber(string key, double value)
    {
        return key switch
        {
            "opacity" => Math.Clamp(value, 0.0, 1.0),
            "gap" => Math.Clamp(value, 0.0, 20.0),
            "line_length" => Math.Clamp(value, 0.0, 20.0),
            "line_width" => Math.Clamp(value, 0.1, 5.0),
            "outline_width" => Math.Clamp(value, 0.1, 5.0),
            "dot_size" => Math.Clamp(value, 0.5, 10.0),
            _ => value
        };
    }

    private static int JsonNodeInt(JsonNode node)
    {
        if (node is JsonValue jv)
        {
            if (jv.TryGetValue<int>(out int value))
            {
                return value;
            }
            if (jv.TryGetValue<double>(out double number))
            {
                return (int)Math.Round(number);
            }
        }
        throw new InvalidOperationException("bad number");
    }

    private object ApplyPreset(string preset)
    {
        EnsureWritable();
        MaybeBackup();
        double render;
        double mipmap;
        double af;
        double fps;

        switch (preset)
        {
            case "low":
                render = 8;
                mipmap = 0;
                af = AfOff;
                fps = 120;
                break;
            case "medium":
                render = 12;
                mipmap = 1;
                af = AfOn;
                fps = 160;
                break;
            case "high":
                render = 20;
                mipmap = 4;
                af = AfOn;
                fps = 240;
                break;
            default:
                throw new InvalidOperationException("unknown preset");
        }

        SetSectionValue(_profile!, "graphic_settings", "render_distance", JsonValue.Create(render));
        SetSectionValue(_profile!, "graphic_settings", "mipmap_levels", JsonValue.Create(mipmap));
        SetSectionValue(_profile!, "graphic_settings", "af_level", JsonValue.Create(af));
        SetSectionValue(_profile!, "screen_settings", "fps_limit", JsonValue.Create(fps));
        Save(_profile!, _profilePath);
        return GetState();
    }

    private object ApplyGeneralPreset(string preset)
    {
        EnsureWritable();
        MaybeBackup();

        switch (preset)
        {
            case "combat":
                SetSectionValue(_profile!, "general_settings", "fov", JsonValue.Create(90.0));
                SetSectionValue(_profile!, "general_settings", "dynamic_fov", JsonValue.Create(false));
                SetSectionValue(_profile!, "general_settings", "show_tips", JsonValue.Create(false));
                SetSectionValue(_profile!, "general_settings", "snooper", JsonValue.Create(false));
                SetSectionValue(_profile!, "general_settings", "freezing_overlay", JsonValue.Create(false));
                SetSectionValue(_profile!, "general_settings", "bleeding_overlay", JsonValue.Create(false));
                SetSectionValue(_profile!, "general_settings", "debug_mode", JsonValue.Create(false));
                SetSectionValue(_profile!, "general_settings", "show_fps", JsonValue.Create(true));
                SetSectionValue(_profile!, "graphic_settings", "render_distance", JsonValue.Create(24.0));
                SetSectionValue(_profile!, "graphic_settings", "mipmap_levels", JsonValue.Create(2.0));
                SetSectionValue(_profile!, "graphic_settings", "minimap_quality", JsonValue.Create(1));
                SetSectionValue(_profile!, "graphic_settings", "new_render", JsonValue.Create(true));
                SetSectionValue(_profile!, "graphic_settings", "render_regions", JsonValue.Create(false));
                SetSectionValue(_profile!, "screen_settings", "v_sync", JsonValue.Create(false));
                SetSectionValue(_profile!, "screen_settings", "fps_limit", JsonValue.Create(240.0));
                break;
            case "optimize":
                SetSectionValue(_profile!, "general_settings", "fov", JsonValue.Create(80.0));
                SetSectionValue(_profile!, "general_settings", "dynamic_fov", JsonValue.Create(false));
                SetSectionValue(_profile!, "general_settings", "show_tips", JsonValue.Create(false));
                SetSectionValue(_profile!, "general_settings", "snooper", JsonValue.Create(false));
                SetSectionValue(_profile!, "general_settings", "freezing_overlay", JsonValue.Create(false));
                SetSectionValue(_profile!, "general_settings", "bleeding_overlay", JsonValue.Create(false));
                SetSectionValue(_profile!, "general_settings", "debug_mode", JsonValue.Create(false));
                SetSectionValue(_profile!, "general_settings", "show_fps", JsonValue.Create(false));
                SetSectionValue(_profile!, "graphic_settings", "render_distance", JsonValue.Create(8.0));
                SetSectionValue(_profile!, "graphic_settings", "mipmap_levels", JsonValue.Create(0.0));
                SetSectionValue(_profile!, "graphic_settings", "minimap_quality", JsonValue.Create(0));
                SetSectionValue(_profile!, "graphic_settings", "new_render", JsonValue.Create(true));
                SetSectionValue(_profile!, "graphic_settings", "render_regions", JsonValue.Create(true));
                SetSectionValue(_profile!, "screen_settings", "v_sync", JsonValue.Create(false));
                SetSectionValue(_profile!, "screen_settings", "fps_limit", JsonValue.Create(160.0));
                break;
            case "beauty":
                SetSectionValue(_profile!, "general_settings", "fov", JsonValue.Create(90.0));
                SetSectionValue(_profile!, "general_settings", "dynamic_fov", JsonValue.Create(true));
                SetSectionValue(_profile!, "general_settings", "show_tips", JsonValue.Create(true));
                SetSectionValue(_profile!, "general_settings", "snooper", JsonValue.Create(true));
                SetSectionValue(_profile!, "general_settings", "freezing_overlay", JsonValue.Create(true));
                SetSectionValue(_profile!, "general_settings", "bleeding_overlay", JsonValue.Create(true));
                SetSectionValue(_profile!, "general_settings", "debug_mode", JsonValue.Create(false));
                SetSectionValue(_profile!, "general_settings", "show_fps", JsonValue.Create(false));
                SetSectionValue(_profile!, "graphic_settings", "render_distance", JsonValue.Create(24.0));
                SetSectionValue(_profile!, "graphic_settings", "mipmap_levels", JsonValue.Create(4.0));
                SetSectionValue(_profile!, "graphic_settings", "minimap_quality", JsonValue.Create(1));
                SetSectionValue(_profile!, "graphic_settings", "new_render", JsonValue.Create(true));
                SetSectionValue(_profile!, "graphic_settings", "render_regions", JsonValue.Create(false));
                SetSectionValue(_profile!, "screen_settings", "v_sync", JsonValue.Create(true));
                SetSectionValue(_profile!, "screen_settings", "fps_limit", JsonValue.Create(240.0));
                break;
            default:
                throw new InvalidOperationException("unknown preset");
        }

        Save(_profile!, _profilePath);
        return GetState();
    }

    private object SaveSettings()
    {
        EnsureWritable();
        Save(_root!, _rootPath);
        Save(_profile!, _profilePath);
        return GetState();
    }

    private object CreateBackup()
    {
        EnsureFound();
        MakeBackup();
        return GetState();
    }

    private object ListBackups()
    {
        if (!_found || !Directory.Exists(BackupsDir))
        {
            return new List<object>();
        }

        List<object> list = new();
        foreach (string dir in Directory.GetDirectories(BackupsDir).OrderByDescending(d => Directory.GetCreationTime(d)))
        {
            if (!File.Exists(Path.Combine(dir, "root.json")) || !File.Exists(Path.Combine(dir, "profile.json")))
            {
                continue;
            }
            list.Add(new
            {
                name = Path.GetFileName(dir),
                created = Directory.GetCreationTime(dir).ToString("dd.MM.yyyy HH:mm:ss"),
                profile = BackupProfileName(dir)
            });
        }
        return list;
    }

    private object RestoreBackup(string name)
    {
        EnsureFound();
        string dir = ResolveBackup(name);
        EnsureWritable();
        DoRestore(dir);
        return GetState();
    }

    private object DeleteBackup(string name)
    {
        EnsureFound();
        string dir = ResolveBackup(name);
        Directory.Delete(dir, true);
        return GetState();
    }

    private object SetKeybind(string key, JsonNode? value)
    {
        EnsureWritable();
        if (value == null)
        {
            throw new InvalidOperationException("no value");
        }
        MaybeBackup();
        if (_profile!["keybinding_settings"] is not JsonObject kb || !kb.ContainsKey(key))
        {
            throw new InvalidOperationException("key not allowed");
        }
        kb[key] = JsonValue.Create(JsonNodeInt(value));
        Save(_profile!, _profilePath);
        return GetState();
    }

    private string SlotPath(int slot)
    {
        return Path.Combine(ProfilesDir, "slot" + slot + ".json");
    }

    private static int EnsureSlot(int slot)
    {
        if (slot < 1 || slot > 3)
        {
            throw new InvalidOperationException("bad slot");
        }
        return slot;
    }

    private object ListProfileSlots()
    {
        List<object> list = new();
        for (int slot = 1; slot <= 3; slot++)
        {
            string path = SlotPath(slot);
            string name = "";
            string created = "";
            bool filled = false;
            if (File.Exists(path))
            {
                try
                {
                    JsonObject node = JsonNode.Parse(File.ReadAllText(path)) as JsonObject ?? new JsonObject();
                    if (node["kind"]?.GetValue<string>() == "profile")
                    {
                        name = node["name"]?.GetValue<string>() ?? "";
                        created = node["created"]?.GetValue<string>() ?? "";
                        filled = true;
                    }
                }
                catch
                {
                }
            }
            list.Add(new { slot, name, created, filled });
        }
        return list;
    }

    private object SaveProfileSlot(int slot, string name)
    {
        EnsureFound();
        EnsureSlot(slot);
        string clean = name.Trim();
        if (clean.Length == 0)
        {
            clean = "Профиль " + slot;
        }
        if (clean.Length > 48)
        {
            clean = clean.Substring(0, 48);
        }
        Directory.CreateDirectory(ProfilesDir);
        File.WriteAllText(SlotPath(slot), BuildProfileNode(clean).ToJsonString(FileOptions));
        return GetState();
    }

    private object ApplyProfileSlot(int slot)
    {
        EnsureWritable();
        string path = SlotPath(EnsureSlot(slot));
        if (!File.Exists(path))
        {
            throw new InvalidOperationException("Слот пуст");
        }
        JsonObject node = ReadProfileFile(path);
        MaybeBackup();
        ApplyProfileNode(node);
        Save(_profile!, _profilePath);
        return GetState();
    }

    private object DeleteProfileSlot(int slot)
    {
        string path = SlotPath(EnsureSlot(slot));
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        return GetState();
    }

    private object ExportProfile(string path, string name)
    {
        EnsureFound();
        if (path.Length == 0)
        {
            throw new InvalidOperationException("Путь не выбран");
        }
        string clean = name.Trim();
        if (clean.Length == 0)
        {
            clean = "rustme-profile";
        }
        if (clean.Length > 48)
        {
            clean = clean.Substring(0, 48);
        }
        File.WriteAllText(path, BuildProfileNode(clean).ToJsonString(FileOptions));
        return GetState();
    }

    private object ImportProfile(string path)
    {
        EnsureWritable();
        JsonObject node = ReadProfileFile(path);
        MaybeBackup();
        ApplyProfileNode(node);
        Save(_profile!, _profilePath);
        return GetState();
    }

    private object PickProfilePath(bool save, string defaultName)
    {
        if (save)
        {
            EnsureFound();
        }
        else
        {
            EnsureWritable();
        }

        string initial = save ? SafeFileName(defaultName) + ".json" : "";
        IntPtr buffer = Marshal.StringToHGlobalUni(initial + new string('\0', 1024));
        try
        {
            IntPtr owner = IntPtr.Zero;
            if (MainWindow.Instance != null)
            {
                owner = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow.Instance);
            }
            OPENFILENAMEW ofn = new()
            {
                lStructSize = Marshal.SizeOf<OPENFILENAMEW>(),
                hwndOwner = owner,
                lpstrFilter = "Профиль RustMeTweaker (*.json)\0*.json\0Все файлы (*.*)\0*.*\0",
                nFilterIndex = 1,
                lpstrFile = buffer,
                nMaxFile = initial.Length + 1025,
                lpstrTitle = save ? "Экспортировать профиль" : "Импортировать профиль",
                lpstrDefExt = "json",
                Flags = OfnExplorer | OfnNoChangeDir | (save ? OfnOverwritePrompt : (OfnFileMustExist | OfnPathMustExist))
            };
            bool ok = save ? GetSaveFileNameW(ref ofn) : GetOpenFileNameW(ref ofn);
            string path = ok ? Marshal.PtrToStringUni(buffer) ?? "" : "";
            return new { path };
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private static string SafeFileName(string raw)
    {
        string clean = raw.Trim();
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            clean = clean.Replace(c, '_');
        }
        if (clean.Length == 0)
        {
            clean = "rustme-profile";
        }
        if (clean.Length > 48)
        {
            clean = clean.Substring(0, 48);
        }
        return clean;
    }

    private JsonObject BuildProfileNode(string name)
    {
        JsonObject graphic = new()
        {
            ["af_level"] = JsonValue.Create(ReadDouble(_profile!, "graphic_settings", "af_level")),
            ["render_distance"] = JsonValue.Create(ReadDouble(_profile!, "graphic_settings", "render_distance")),
            ["mipmap_levels"] = JsonValue.Create(ReadDouble(_profile!, "graphic_settings", "mipmap_levels")),
            ["minimap_quality"] = JsonValue.Create(ReadInt(_profile!, "graphic_settings", "minimap_quality")),
            ["new_render"] = JsonValue.Create(ReadBool(_profile!, "graphic_settings", "new_render")),
            ["render_regions"] = JsonValue.Create(ReadBool(_profile!, "graphic_settings", "render_regions"))
        };
        JsonObject general = new()
        {
            ["fov"] = JsonValue.Create(ReadDouble(_profile!, "general_settings", "fov")),
            ["dynamic_fov"] = JsonValue.Create(ReadBool(_profile!, "general_settings", "dynamic_fov")),
            ["show_tips"] = JsonValue.Create(ReadBool(_profile!, "general_settings", "show_tips")),
            ["snooper"] = JsonValue.Create(ReadBool(_profile!, "general_settings", "snooper")),
            ["freezing_overlay"] = JsonValue.Create(ReadBool(_profile!, "general_settings", "freezing_overlay")),
            ["bleeding_overlay"] = JsonValue.Create(ReadBool(_profile!, "general_settings", "bleeding_overlay")),
            ["debug_mode"] = JsonValue.Create(ReadBool(_profile!, "general_settings", "debug_mode")),
            ["show_fps"] = JsonValue.Create(ReadBool(_profile!, "general_settings", "show_fps"))
        };
        JsonObject screen = new()
        {
            ["fps_limit"] = JsonValue.Create(ReadDouble(_profile!, "screen_settings", "fps_limit")),
            ["v_sync"] = JsonValue.Create(ReadBool(_profile!, "screen_settings", "v_sync"))
        };
        JsonObject sound = new();
        foreach (string key in SoundKeys)
        {
            if (HasKey(_profile!, "sound_settings", key))
            {
                sound[key] = JsonValue.Create(ReadDouble(_profile!, "sound_settings", key));
            }
        }
        JsonObject? crosshair = null;
        if (_profile!["crosshair_settings"] is JsonObject xh)
        {
            crosshair = new JsonObject();
            foreach (string key in CopyCrosshairKeys)
            {
                if (xh.ContainsKey(key))
                {
                    crosshair[key] = CloneNode(xh[key]);
                }
            }
        }
        JsonObject bindings = new();
        if (_profile!["keybinding_settings"] is JsonObject kb)
        {
            foreach (KeyValuePair<string, JsonNode?> pair in kb)
            {
                if (pair.Value is JsonValue jv && jv.TryGetValue<int>(out int code))
                {
                    bindings[pair.Key] = JsonValue.Create(code);
                }
            }
        }
        JsonObject node = new()
        {
            ["app"] = JsonValue.Create("RustMeTweaker"),
            ["kind"] = JsonValue.Create("profile"),
            ["version"] = JsonValue.Create(1),
            ["name"] = JsonValue.Create(name),
            ["created"] = JsonValue.Create(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")),
            ["graphic"] = graphic,
            ["general"] = general,
            ["screen"] = screen,
            ["sound"] = sound,
            ["bindings"] = bindings
        };
        if (crosshair != null)
        {
            node["crosshair"] = crosshair;
        }
        return node;
    }

    private static JsonObject ReadProfileFile(string path)
    {
        if (path.Length == 0 || !File.Exists(path))
        {
            throw new InvalidOperationException("Файл профиля не найден");
        }
        JsonObject node;
        try
        {
            node = JsonNode.Parse(File.ReadAllText(path)) as JsonObject ?? throw new InvalidOperationException();
            if (node["app"]?.GetValue<string>() != "RustMeTweaker" || node["kind"]?.GetValue<string>() != "profile" || node["version"]?.GetValue<int>() != 1)
            {
                throw new InvalidOperationException();
            }
            if (node["graphic"] is not JsonObject || node["general"] is not JsonObject || node["screen"] is not JsonObject)
            {
                throw new InvalidOperationException();
            }
        }
        catch
        {
            throw new InvalidOperationException("Это не профиль RustMeTweaker или файл повреждён");
        }
        return node;
    }

    private void ApplyProfileNode(JsonObject node)
    {
        JsonObject graphic = (JsonObject)node["graphic"]!;
        JsonObject general = (JsonObject)node["general"]!;
        JsonObject screen = (JsonObject)node["screen"]!;

        double af = ProfileDouble(graphic, "af_level", AfOff);
        double render = Math.Clamp(Math.Round(ProfileDouble(graphic, "render_distance", 12.0)), 2.0, 32.0);
        double mipmap = Math.Clamp(Math.Round(ProfileDouble(graphic, "mipmap_levels", 1.0)), 0.0, 4.0);
        int minimap = ProfileDouble(graphic, "minimap_quality", 0.0) >= 0.5 ? 1 : 0;
        bool newrender = ProfileBool(graphic, "new_render", true);
        bool regions = ProfileBool(graphic, "render_regions", false);

        double fov = Math.Clamp(Math.Round(ProfileDouble(general, "fov", 80.0)), 70.0, 90.0);
        bool dynamicfov = ProfileBool(general, "dynamic_fov", false);
        bool tips = ProfileBool(general, "show_tips", false);
        bool snooper = ProfileBool(general, "snooper", false);
        bool freezing = ProfileBool(general, "freezing_overlay", false);
        bool bleeding = ProfileBool(general, "bleeding_overlay", false);
        bool debug = ProfileBool(general, "debug_mode", false);
        bool showfps = ProfileBool(general, "show_fps", false);

        double fps = ProfileDouble(screen, "fps_limit", 160.0);
        if (fps >= 9999.0)
        {
            fps = 9999.0;
        }
        else if (fps <= 0.0)
        {
            fps = 0.0;
        }
        else
        {
            fps = Math.Clamp(Math.Round(fps / 10.0, MidpointRounding.AwayFromZero) * 10.0, 30.0, 240.0);
        }
        bool vsync = ProfileBool(screen, "v_sync", false);

        SetSectionValue(_profile!, "graphic_settings", "af_level", JsonValue.Create(af <= 0.0 ? AfOff : AfOn));
        SetSectionValue(_profile!, "graphic_settings", "render_distance", JsonValue.Create(render));
        SetSectionValue(_profile!, "graphic_settings", "mipmap_levels", JsonValue.Create(mipmap));
        SetSectionValue(_profile!, "graphic_settings", "minimap_quality", JsonValue.Create(minimap));
        SetSectionValue(_profile!, "graphic_settings", "new_render", JsonValue.Create(newrender));
        SetSectionValue(_profile!, "graphic_settings", "render_regions", JsonValue.Create(regions));
        SetSectionValue(_profile!, "general_settings", "fov", JsonValue.Create(fov));
        SetSectionValue(_profile!, "general_settings", "dynamic_fov", JsonValue.Create(dynamicfov));
        SetSectionValue(_profile!, "general_settings", "show_tips", JsonValue.Create(tips));
        SetSectionValue(_profile!, "general_settings", "snooper", JsonValue.Create(snooper));
        SetSectionValue(_profile!, "general_settings", "freezing_overlay", JsonValue.Create(freezing));
        SetSectionValue(_profile!, "general_settings", "bleeding_overlay", JsonValue.Create(bleeding));
        SetSectionValue(_profile!, "general_settings", "debug_mode", JsonValue.Create(debug));
        SetSectionValue(_profile!, "general_settings", "show_fps", JsonValue.Create(showfps));
        SetSectionValue(_profile!, "screen_settings", "fps_limit", JsonValue.Create(fps));
        SetSectionValue(_profile!, "screen_settings", "v_sync", JsonValue.Create(vsync));

        if (node["sound"] is JsonObject sound && _profile!["sound_settings"] is JsonObject snd)
        {
            foreach (string key in SoundKeys)
            {
                if (sound[key] is JsonValue jv && snd.ContainsKey(key) && jv.TryGetValue<double>(out double volume))
                {
                    snd[key] = JsonValue.Create(Math.Clamp(volume, 0.0, 1.0));
                }
            }
        }

        if (node["crosshair"] is JsonObject cross && _profile!["crosshair_settings"] is JsonObject xh)
        {
            foreach (string key in CopyCrosshairKeys)
            {
                if (cross[key] is not JsonValue jv || !xh.ContainsKey(key))
                {
                    continue;
                }
                switch (key)
                {
                    case "enabled":
                    case "outline":
                    case "dynamic_spacing":
                    case "dynamic_visibility":
                        if (jv.TryGetValue<bool>(out bool flag))
                        {
                            xh[key] = JsonValue.Create(flag);
                        }
                        break;
                    case "style":
                        if (jv.TryGetValue<string>(out string? style) && style != null && IsValidStyle(style))
                        {
                            xh[key] = JsonValue.Create(style);
                        }
                        break;
                    case "color":
                    case "outline_color":
                        if (jv.TryGetValue<int>(out int color))
                        {
                            xh[key] = JsonValue.Create(Math.Clamp(color, 0, 16777215));
                        }
                        else if (jv.TryGetValue<double>(out double colorNumber))
                        {
                            xh[key] = JsonValue.Create(Math.Clamp((int)Math.Round(colorNumber), 0, 16777215));
                        }
                        break;
                    default:
                        if (jv.TryGetValue<double>(out double number))
                        {
                            xh[key] = JsonValue.Create(ClampCrosshairNumber(key, number));
                        }
                        break;
                }
            }
        }

        if (node["bindings"] is JsonObject binds && _profile!["keybinding_settings"] is JsonObject kb)
        {
            foreach (KeyValuePair<string, JsonNode?> pair in binds)
            {
                if (pair.Value is JsonValue jv && jv.TryGetValue<int>(out int code) && kb.ContainsKey(pair.Key) && code >= -100 && code <= 348)
                {
                    kb[pair.Key] = JsonValue.Create(code);
                }
            }
        }
    }

    private static double ProfileDouble(JsonObject section, string key, double fallback)
    {
        if (section[key] is JsonValue jv)
        {
            if (jv.TryGetValue<double>(out double value))
            {
                return value;
            }
            if (jv.TryGetValue<long>(out long number))
            {
                return number;
            }
        }
        return fallback;
    }

    private static bool ProfileBool(JsonObject section, string key, bool fallback)
    {
        if (section[key] is JsonValue jv && jv.TryGetValue<bool>(out bool value))
        {
            return value;
        }
        return fallback;
    }

    private object SetAutoCrunk(bool enabled)
    {
        if (_crunk == enabled)
        {
            return GetState();
        }
        _crunk = enabled;
        if (enabled)
        {
            SetCrunkPriorities(true);
            StartCrunkTimer();
            _gameWasRunning = IsClientRunning();
            if (_standbyClear && !_gameWasRunning)
            {
                try
                {
                    PurgeStandbyList();
                }
                catch
                {
                }
            }
        }
        else
        {
            StopCrunkTimer();
            SetCrunkPriorities(false);
        }
        AutoCrunkToggled?.Invoke(enabled);
        return GetState();
    }

    private object SetAutoBackup(bool enabled)
    {
        _autoBackup = enabled;
        SaveTweakerConfig();
        return GetState();
    }

    private object SetCrunkPriority(int priority)
    {
        _crunkPriority = Math.Clamp(priority, 0, 2);
        SaveTweakerConfig();
        if (_crunk)
        {
            SetCrunkPriorities(true);
        }
        return GetState();
    }

    private object SetCrunkInterval(int ms)
    {
        _crunkIntervalMs = Math.Clamp(ms, 2000, 20000);
        SaveTweakerConfig();
        if (_crunk)
        {
            StartCrunkTimer();
        }
        return GetState();
    }

    private object SetCrunkBlacklist(JsonNode? list)
    {
        List<string> parsed = new();
        if (list is JsonArray arr)
        {
            foreach (JsonNode? item in arr)
            {
                if (item is JsonValue jv && jv.TryGetValue<string>(out string? raw) && raw != null)
                {
                    string norm = NormalizeProcessName(raw);
                    if (norm.Length > 0)
                    {
                        parsed.Add(norm);
                    }
                }
            }
        }
        _crunkBlacklist = parsed;
        SaveTweakerConfig();
        if (_crunk)
        {
            SetCrunkPriorities(false);
            SetCrunkPriorities(true);
        }
        return GetState();
    }

    private object SetStandbyClear(bool enabled)
    {
        if (enabled)
        {
            if (IsClientRunning())
            {
                if (!TryEnableStandbyPrivilege())
                {
                    throw new InvalidOperationException("Очистка standby доступна только при запуске твикера от администратора");
                }
            }
            else
            {
                PurgeStandbyList();
            }
        }
        _standbyClear = enabled;
        _gameWasRunning = IsClientRunning();
        SaveTweakerConfig();
        return GetState();
    }

    private object SetCustomPaths(string launcherDir, string javaPath)
    {
        if (_crunk)
        {
            SetCrunkPriorities(false);
        }
        StopCrunkTimer();
        _crunk = false;
        AutoCrunkToggled?.Invoke(false);
        _customLauncherDir = launcherDir.Trim();
        _customJavaPath = javaPath.Trim();
        SaveTweakerConfig();
        _found = Locate();
        if (_found)
        {
            Load();
        }
        return GetState();
    }

    private object Optimize()
    {
        EnsureWritable();
        MakeBackup();
        SetSectionValue(_profile!, "graphic_settings", "mipmap_levels", JsonValue.Create(0.0));
        SetSectionValue(_profile!, "graphic_settings", "af_level", JsonValue.Create(AfOff));
        SetSectionValue(_profile!, "graphic_settings", "aa_level", JsonValue.Create(0.0));
        SetSectionValue(_profile!, "graphic_settings", "minimap_quality", JsonValue.Create(0));
        SetSectionValue(_profile!, "graphic_settings", "new_render", JsonValue.Create(true));
        SetSectionValue(_profile!, "general_settings", "show_tips", JsonValue.Create(false));
        SetSectionValue(_profile!, "general_settings", "snooper", JsonValue.Create(false));
        SetSectionValue(_profile!, "general_settings", "freezing_overlay", JsonValue.Create(false));
        SetSectionValue(_profile!, "general_settings", "bleeding_overlay", JsonValue.Create(false));
        EnsureSection(_root!, "gameSettings")["isDebugEnabled"] = JsonValue.Create(false);
        Save(_profile!, _profilePath);
        Save(_root!, _rootPath);
        return GetState();
    }

    private object MegaOptimize()
    {
        EnsureWritable();
        MakeBackup();
        SetSectionValue(_profile!, "graphic_settings", "mipmap_levels", JsonValue.Create(0.0));
        SetSectionValue(_profile!, "graphic_settings", "af_level", JsonValue.Create(AfOff));
        SetSectionValue(_profile!, "graphic_settings", "aa_level", JsonValue.Create(0.0));
        SetSectionValue(_profile!, "graphic_settings", "minimap_quality", JsonValue.Create(0));
        SetSectionValue(_profile!, "graphic_settings", "biomes_quality", JsonValue.Create(0));
        SetSectionValue(_profile!, "graphic_settings", "fog_quality", JsonValue.Create(0));
        SetSectionValue(_profile!, "graphic_settings", "light_quality", JsonValue.Create(0));
        SetSectionValue(_profile!, "graphic_settings", "math_quality", JsonValue.Create(0));
        SetSectionValue(_profile!, "graphic_settings", "particles_quality", JsonValue.Create(0));
        SetSectionValue(_profile!, "graphic_settings", "render_regions", JsonValue.Create(true));
        SetSectionValue(_profile!, "graphic_settings", "sky_quality", JsonValue.Create(0));
        SetSectionValue(_profile!, "graphic_settings", "tree_quality", JsonValue.Create(0));
        SetSectionValue(_profile!, "graphic_settings", "new_render", JsonValue.Create(true));
        SetSectionValue(_profile!, "general_settings", "freezing_overlay", JsonValue.Create(false));
        SetSectionValue(_profile!, "general_settings", "bleeding_overlay", JsonValue.Create(false));
        EnsureSection(_root!, "gameSettings")["isDebugEnabled"] = JsonValue.Create(false);
        Save(_profile!, _profilePath);
        Save(_root!, _rootPath);
        ApplyFullscreenCompat();
        SetHighPerformancePowerPlan();
        return GetState();
    }

    private void ApplyFullscreenCompat()
    {
        using RegistryKey layers = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers");
        if (_customJavaPath.Length > 0 && File.Exists(_customJavaPath))
        {
            layers.SetValue(_customJavaPath, "~ DISABLEDXMAXIMIZEDWINDOWEDMODE");
            return;
        }

        string[] targets = { "javaw.exe", "java.exe" };
        foreach (string exe in Directory.GetFiles(_launcherDir, "*.exe", SearchOption.AllDirectories))
        {
            string name = Path.GetFileName(exe);
            if (Array.Exists(targets, t => string.Equals(t, name, StringComparison.OrdinalIgnoreCase)))
            {
                layers.SetValue(exe, "~ DISABLEDXMAXIMIZEDWINDOWEDMODE");
            }
        }
    }

    private void SetHighPerformancePowerPlan()
    {
        ProcessStartInfo psi = new()
        {
            FileName = "powercfg",
            Arguments = "/setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c",
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using Process proc = Process.Start(psi) ?? throw new InvalidOperationException("powercfg не запущен");
        proc.WaitForExit();
        if (proc.ExitCode != 0)
        {
            throw new InvalidOperationException("Схема высокой производительности недоступна на этой системе");
        }
    }

    private object Rollback()
    {
        EnsureWritable();
        DoRestore(LatestBackupDir());
        return GetState();
    }

    private void StartCrunkTimer()
    {
        StopCrunkTimer();
        _crunkTimer = new Timer(_crunkIntervalMs);
        _crunkTimer.AutoReset = true;
        _crunkTimer.Elapsed += (_, _) =>
        {
            SetCrunkPriorities(true);
            CheckStandbyClear();
        };
        _crunkTimer.Start();
    }

    private void StopCrunkTimer()
    {
        _crunkTimer?.Stop();
        _crunkTimer?.Dispose();
        _crunkTimer = null;
    }

    private void CheckStandbyClear()
    {
        bool running = IsClientRunning();
        if (_standbyClear && running && !_gameWasRunning)
        {
            try
            {
                PurgeStandbyList();
            }
            catch
            {
            }
        }
        _gameWasRunning = running;
    }

    private string MakeBackup()
    {
        string stamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff");
        string dir = Path.Combine(BackupsDir, "backup_" + stamp);
        if (Directory.Exists(dir))
        {
            dir = Path.Combine(BackupsDir, "backup_" + stamp + "_" + Guid.NewGuid().ToString("N").Substring(0, 6));
        }
        Directory.CreateDirectory(dir);
        File.Copy(_rootPath, Path.Combine(dir, "root.json"), true);
        File.Copy(_profilePath, Path.Combine(dir, "profile.json"), true);
        JsonObject meta = new()
        {
            ["profile"] = _profileName
        };
        File.WriteAllText(Path.Combine(dir, "meta.json"), meta.ToJsonString(FileOptions));
        return dir;
    }

    private string LatestBackupDir()
    {
        if (!Directory.Exists(BackupsDir))
        {
            throw new InvalidOperationException("no backups");
        }
        string? latest = Directory.GetDirectories(BackupsDir)
            .OrderByDescending(d => Directory.GetCreationTime(d))
            .FirstOrDefault(d => File.Exists(Path.Combine(d, "root.json")) && File.Exists(Path.Combine(d, "profile.json")));
        if (latest == null)
        {
            throw new InvalidOperationException("no backups");
        }
        return latest;
    }

    private void DoRestore(string dir)
    {
        string targetProfile = _profilePath;
        string sourceProfile = BackupProfileName(dir);
        if (sourceProfile.Length > 0 && !string.Equals(sourceProfile, _profileName, StringComparison.OrdinalIgnoreCase))
        {
            string path = Path.Combine(GameProfilesDir, sourceProfile, "settings.json");
            if (File.Exists(path))
            {
                targetProfile = path;
            }
        }
        File.Copy(Path.Combine(dir, "root.json"), _rootPath, true);
        File.Copy(Path.Combine(dir, "profile.json"), targetProfile, true);
        if (!string.Equals(targetProfile, _profilePath, StringComparison.OrdinalIgnoreCase))
        {
            _profileName = sourceProfile;
            _profilePath = targetProfile;
            SaveTweakerConfig();
        }
        Load();
    }

    private static string BackupProfileName(string dir)
    {
        try
        {
            string metaPath = Path.Combine(dir, "meta.json");
            if (!File.Exists(metaPath))
            {
                return "";
            }
            if (JsonNode.Parse(File.ReadAllText(metaPath)) is not JsonObject meta)
            {
                return "";
            }
            string name = ReadJsonString(meta, "profile", "");
            if (name.Length == 0 || name != Path.GetFileName(name))
            {
                return "";
            }
            return name;
        }
        catch
        {
            return "";
        }
    }

    private string ResolveBackup(string name)
    {
        string safe = Path.GetFileName(name);
        if (safe.Length == 0 || safe != name)
        {
            throw new InvalidOperationException("bad backup name");
        }
        string dir = Path.Combine(BackupsDir, safe);
        if (!Directory.Exists(dir))
        {
            throw new InvalidOperationException("backup not found");
        }
        return dir;
    }

    private void SetCrunkPriorities(bool boost)
    {
        if (!_found || _launcherDir.Length == 0)
        {
            return;
        }

        ProcessPriorityClass gamePriority = _crunkPriority switch
        {
            0 => ProcessPriorityClass.Normal,
            1 => ProcessPriorityClass.AboveNormal,
            _ => ProcessPriorityClass.High
        };

        string prefix = Path.TrimEndingDirectorySeparator(_launcherDir) + Path.DirectorySeparatorChar;
        foreach (Process process in Process.GetProcesses())
        {
            try
            {
                string? path = process.MainModule?.FileName;
                if (path != null && path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    process.PriorityClass = boost ? gamePriority : ProcessPriorityClass.Normal;
                }
                else if (_crunkBlacklist.Exists(n => string.Equals(n, process.ProcessName, StringComparison.OrdinalIgnoreCase)))
                {
                    process.PriorityClass = boost ? ProcessPriorityClass.BelowNormal : ProcessPriorityClass.Normal;
                }
            }
            catch
            {
            }
            finally
            {
                process.Dispose();
            }
        }
    }

    private static string NormalizeProcessName(string raw)
    {
        string s = raw.Trim();
        if (s.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            s = s.Substring(0, s.Length - 4);
        }
        return s;
    }

    private static bool TryEnableStandbyPrivilege()
    {
        if (!OpenProcessToken(GetCurrentProcess(), TokenAdjustPrivileges | TokenQuery, out IntPtr token))
        {
            return false;
        }
        try
        {
            if (!LookupPrivilegeValueW(null, "SeProfileSingleProcessPrivilege", out LUID luid))
            {
                return false;
            }
            TOKEN_PRIVILEGES state = new()
            {
                PrivilegeCount = 1,
                Luid = luid,
                Attributes = SePrivilegeEnabled
            };
            if (!AdjustTokenPrivileges(token, false, ref state, 0, IntPtr.Zero, IntPtr.Zero))
            {
                return false;
            }
            return Marshal.GetLastWin32Error() != ErrorNotAllAssigned;
        }
        finally
        {
            CloseHandle(token);
        }
    }

    private static void PurgeStandbyList()
    {
        if (!TryEnableStandbyPrivilege())
        {
            throw new InvalidOperationException("Очистка standby доступна только при запуске твикера от администратора");
        }
        int command = MemoryPurgeStandbyList;
        uint status = NtSetSystemInformation(SystemMemoryListInformation, ref command, sizeof(int));
        if (status != 0)
        {
            throw new InvalidOperationException("Windows отклонила очистку standby (код 0x" + status.ToString("X8") + ")");
        }
    }

    private void EnsureFound()
    {
        if (!_found || _root == null || _profile == null)
        {
            throw new InvalidOperationException("RustMe не найден");
        }
    }

    private void EnsureWritable()
    {
        if (!_found || _root == null || _profile == null)
        {
            throw new InvalidOperationException("RustMe не найден");
        }
        if (IsClientRunning())
        {
            throw new InvalidOperationException("Закрой лаунчер и игру перед изменением настроек");
        }
    }

    private static bool HasKey(JsonObject root, string section, string key)
    {
        return root[section] is JsonObject obj && obj.ContainsKey(key);
    }

    private static void RequireKey(JsonObject root, string section, string key)
    {
        if (!HasKey(root, section, key))
        {
            throw new InvalidOperationException("В этом профиле нет такой настройки");
        }
    }

    private static bool ReadJsonBool(JsonObject node, string key, bool fallback)
    {
        if (node[key] is JsonValue jv && jv.TryGetValue<bool>(out bool value))
        {
            return value;
        }
        return fallback;
    }

    private static double ReadJsonDouble(JsonObject node, string key, double fallback)
    {
        if (node[key] is JsonValue jv)
        {
            if (jv.TryGetValue<double>(out double value))
            {
                return value;
            }
            if (jv.TryGetValue<long>(out long number))
            {
                return number;
            }
        }
        return fallback;
    }

    private static int ReadJsonInt(JsonObject node, string key, int fallback)
    {
        if (node[key] is JsonValue jv)
        {
            if (jv.TryGetValue<int>(out int value))
            {
                return value;
            }
            if (jv.TryGetValue<double>(out double number))
            {
                return (int)Math.Round(number);
            }
        }
        return fallback;
    }

    private static string ReadJsonString(JsonObject node, string key, string fallback)
    {
        if (node[key] is JsonValue jv && jv.TryGetValue<string>(out string? value) && value != null)
        {
            return value;
        }
        return fallback;
    }

    private static double ReadDouble(JsonObject root, string section, string key)
    {
        if (root[section] is JsonObject obj)
        {
            return ReadJsonDouble(obj, key, 0.0);
        }
        return 0.0;
    }

    private static int ReadInt(JsonObject root, string section, string key)
    {
        if (root[section] is JsonObject obj)
        {
            return ReadJsonInt(obj, key, 0);
        }
        return 0;
    }

    private static bool ReadBool(JsonObject root, string section, string key)
    {
        if (root[section] is JsonObject obj)
        {
            return ReadJsonBool(obj, key, false);
        }
        return false;
    }

    private static string ReadString(JsonObject root, string section, string key)
    {
        if (root[section] is JsonObject obj)
        {
            return ReadJsonString(obj, key, "");
        }
        return "";
    }

    private static JsonObject EnsureSection(JsonObject root, string section)
    {
        if (root[section] is not JsonObject obj)
        {
            obj = new JsonObject();
            root[section] = obj;
        }
        return obj;
    }

    private static void SetSectionValue(JsonObject root, string section, string key, JsonNode value)
    {
        EnsureSection(root, section)[key] = value;
    }

    private static void Save(JsonObject node, string path)
    {
        string temp = path + ".tmp";
        File.WriteAllText(temp, node.ToJsonString(FileOptions));
        if (File.Exists(path))
        {
            File.Copy(path, path + ".bak", true);
        }
        File.Move(temp, path, true);
        string diskText = File.ReadAllText(path);
        string memText = node.ToJsonString(FileOptions);
        if (diskText != memText)
        {
            throw new InvalidOperationException("файл перезаписан снаружи сразу после записи");
        }
    }

    private bool IsClientRunning()
    {
        if (!_found || _launcherDir.Length == 0)
        {
            return false;
        }

        string prefix = Path.TrimEndingDirectorySeparator(_launcherDir) + Path.DirectorySeparatorChar;
        foreach (Process process in Process.GetProcesses())
        {
            try
            {
                string? path = process.MainModule?.FileName;
                if (path != null && path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            catch
            {
            }
            finally
            {
                process.Dispose();
            }
        }

        return false;
    }

    private static ulong GetTotalRam()
    {
        MEMORYSTATUSEX status = new()
        {
            dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>()
        };
        if (GlobalMemoryStatusEx(ref status))
        {
            return status.ullTotalPhys;
        }
        return 8589934592UL;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetCurrentProcess();

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr hObject);

    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool OpenProcessToken(IntPtr processHandle, uint desiredAccess, out IntPtr tokenHandle);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool LookupPrivilegeValueW(string? systemName, string name, out LUID lpLuid);

    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AdjustTokenPrivileges(IntPtr tokenHandle, [MarshalAs(UnmanagedType.Bool)] bool disableAllPrivileges, ref TOKEN_PRIVILEGES newState, uint bufferLength, IntPtr previousState, IntPtr returnLength);

    [DllImport("ntdll.dll")]
    private static extern uint NtSetSystemInformation(int systemInformationClass, ref int systemInformation, int systemInformationLength);

    [DllImport("comdlg32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetSaveFileNameW(ref OPENFILENAMEW lpofn);

    [DllImport("comdlg32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetOpenFileNameW(ref OPENFILENAMEW lpofn);

    [StructLayout(LayoutKind.Sequential)]
    private struct MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct LUID
    {
        public uint LowPart;
        public int HighPart;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct TOKEN_PRIVILEGES
    {
        public uint PrivilegeCount;
        public LUID Luid;
        public uint Attributes;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct OPENFILENAMEW
    {
        public int lStructSize;
        public IntPtr hwndOwner;
        public IntPtr hInstance;
        public string? lpstrFilter;
        public IntPtr lpstrCustomFilter;
        public int nMaxCustFilter;
        public int nFilterIndex;
        public IntPtr lpstrFile;
        public int nMaxFile;
        public IntPtr lpstrFileTitle;
        public int nMaxFileTitle;
        public string? lpstrInitialDir;
        public string? lpstrTitle;
        public int Flags;
        public ushort nFileOffset;
        public ushort nFileExtension;
        public string? lpstrDefExt;
        public IntPtr lCustData;
        public IntPtr lpfnHook;
        public string? lpTemplateName;
        public IntPtr pvReserved;
        public int dwReserved;
        public int FlagsEx;
    }
}