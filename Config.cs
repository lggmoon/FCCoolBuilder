using com.clusterrr.Famicom.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace FCCoolBuilder;

class Config
{
    public enum Cardtypes { Coolemu = 1, Coolgirl = 2 };

    static public Dictionary<Cardtypes, string> CardtypeNameMap = new(){
        {Cardtypes.Coolemu,"coolemu"},
        {Cardtypes.Coolgirl,"coolgirl"},
    };

    //defaults 
    const string DEFAULT_CONFIG_FILE = "settings.json";
    const string DEFAULT_GAMELIST_FILE = "gamelist.sav";
    
    const string DEFAULT_TOOLS_DIR = "tools";
    const string DEFAULT_SOURCES_DIR = "sources";
    const string DEFAULT_THEMES_DIR = "themes";
    const string DEFAULT_MUSICS_DIR = "musics";
    const string DEFAULT_SFXES_DIR = "sfxes";

    const string DEFAULT_MAPPERS_FILE = @"coolgirl-mappers.json";
    const string DEFAULT_SYMBOLS_FILE = @"coolgirl-symbols.json";
    const string DEFAULT_FIXES_FILE = @"nes-fixes.json";
    const string DEFAULT_NESASM_FILE = "nesasm.exe";

    const uint DEFAULT_MAX_ROM_SIZE_MB = 256;
    const uint DEFAULT_MAX_CHR_RAM_SIZE_KB = 512;

    // 1. Json序列化Options
    static private JsonSerializerOptions JsonOptions = new JsonSerializerOptions() {
        WriteIndented = true,
        //DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        TypeInfoResolver = new DefaultJsonTypeInfoResolver {
            Modifiers = { IncludeOnlyMarkedProperties }
        }
    };
    // 2. 修饰器：仅保留带 [JsonInclude] 的公有属性（也可扩展支持字段）
    static void IncludeOnlyMarkedProperties(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object) return;

        var propertiesToKeep = typeInfo.Properties
            .Where(prop => prop.AttributeProvider?.IsDefined(typeof(JsonIncludeAttribute), true) == true)
            .ToList();

        typeInfo.Properties.Clear();
        foreach (var prop in propertiesToKeep)
            typeInfo.Properties.Add(prop);
    }

    // configs
    public string ToolsDir { get; private set; } 
    public string SourcesDir { get; private set; }
    public string ThemesDir { get; private set; }
    public string MusicsDir { get; private set; }
    public string SfxesDir { get; private set; }

    public string NesAsmFile { get; private set; } 
    public string MappersFile { get; private set; }
    public string FixesFile { get; private set; }
    public string SymbolsFile { get; private set; }
    public string GameListFile { get; private set; }
    public HashSet<int> BadSectors { get; private set; } = [];

    public uint MaxRomSizeMB { get; private set; } = DEFAULT_MAX_ROM_SIZE_MB;
    public uint MaxChrRamSizeKB { get; private set; } = DEFAULT_MAX_CHR_RAM_SIZE_KB;

    public int MaxSfxIdx = 0;

    // config
    [JsonInclude]
    public Cardtypes Cardtype = Cardtypes.Coolemu;

    [JsonInclude]
    public string ThemeName = "default";

    // theme config
    [JsonInclude]
    public bool EnableLastGameSaveing = false;
    [JsonInclude]
    public bool EnableCursorBling = false;
    [JsonInclude]
    public bool ShowIdx = true;
    [JsonInclude]
    public bool IdxFillZero = true;
    [JsonInclude]
    public int CursorBlingCount = 1;
    [JsonInclude]
    public int CursorSpeed = 4;

    // music config
    [JsonInclude]
    public int MusicIdx = 0;
    [JsonInclude]
    public string MusicName = "";
    [JsonInclude]
    public int SfxIdx = 1;
    [JsonInclude]
    public string SfxName = "";


    public Config()
    {
        string currentDirectory = GetConfigDir();

        ToolsDir = Path.Combine(currentDirectory, DEFAULT_TOOLS_DIR);        
        SourcesDir = Path.Combine(currentDirectory, DEFAULT_SOURCES_DIR);
        ThemesDir = DEFAULT_THEMES_DIR;
        MusicsDir = DEFAULT_MUSICS_DIR;
        SfxesDir = DEFAULT_SFXES_DIR;

        NesAsmFile = Path.Combine(ToolsDir, DEFAULT_NESASM_FILE);
        MappersFile = Path.Combine(ToolsDir, DEFAULT_MAPPERS_FILE);
        SymbolsFile = Path.Combine(ToolsDir, DEFAULT_SYMBOLS_FILE);
        FixesFile = Path.Combine(ToolsDir, DEFAULT_FIXES_FILE);

        GameListFile = Path.Combine(currentDirectory, DEFAULT_GAMELIST_FILE);
    }

    static public string GetConfigDir() { 
        return OperatingSystem.IsWindows() ? Environment.CurrentDirectory : "/etc"; 
    }

    static public string GetConfigFilePath() {
        return Path.Combine(GetConfigDir(),DEFAULT_CONFIG_FILE);
    }

    static public Config Load()
    {
        var filePath = GetConfigFilePath();
        if (Path.Exists(filePath)) {
            string jsonText = File.ReadAllText(filePath);
            var t = JsonSerializer.Deserialize<Config>(jsonText, JsonOptions);
            if (t != null) {
                return t;
            }
        }
        return new();
    }

    public void Save()
    {
        var filePath = GetConfigFilePath();
        string text = JsonSerializer.Serialize(this, JsonOptions);
        try {
            File.WriteAllText(filePath, text);
        } catch {
            MessageBox.Show($"保存settings文件错误，{filePath}");
        }
    }

    public string GetCardtypeSourcesDir()
    {
        return Path.Combine(SourcesDir, CardtypeNameMap[this.Cardtype]);
    }

    public string GetAsmMusicFile()
    {
        return $@"../../{MusicsDir}/{ MusicName}.asm";
    }

    public string GetAsmSfxFile()
    {
        return $@"../../{SfxesDir}/{SfxName}.asm";
    }

    public string GetAsmThemeDir()
    {
        return $@"../../{ThemesDir}/{ThemeName}";
    }

}
