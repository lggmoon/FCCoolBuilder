using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FCCoolBuilder;

class Theme
{
    [JsonPropertyName("Description")]
    public string Description { get; set; } = "";
    // config
    [JsonPropertyName("Title")]
    public string Title { get; set; } = "## IN 1";
    [JsonPropertyName("PageMenuNum")]
    public byte PageMenuNum { get; set; } = 20;
    [JsonPropertyName("MenuTitleX")]
    public byte MenuTitleX { get; set; } = 12;
    [JsonPropertyName("MenuTitleY")]
    public byte MenuTitleY { get; set; } = 3;
    [JsonPropertyName("MenuStartX")]
    public byte MenuStartX { get; set; } = 9;
    [JsonPropertyName("MenuStartY")]
    public byte MenuStartY { get; set; } = 7;
    [JsonPropertyName("MenuInterSpace")]
    public byte MenuInterSpace { get; set; } = 1;
    [JsonPropertyName("HighLightCode")]
    public byte HighLightCode { get; set; } = 0;
    [JsonPropertyName("CursorOffsetX")]
    public int CursorOffsetX { get; set; } = 0;
    [JsonPropertyName("CursorOffsetY")]
    public int CursorOffsetY { get; set; } = 0;
    [JsonPropertyName("CursorColumnCount")]
    public byte CursorColumnCount { get; set; } = 1;
    [JsonPropertyName("CursorFrameCount")]
    public byte CursorFrameCount { get; set; } = 1;
    [JsonPropertyName("CursorCode")]
    public byte CursorCode { get; set; } = 0x3e;
    [JsonPropertyName("CursorRefreshTime")]
    public byte CursorRefreshTime { get; set; } = 5;
    [JsonPropertyName("NoneCode")]
    public byte NoneCode { get; set; } = 0x02;
    [JsonPropertyName("UseAscii")]
    public bool UseAscii { get; set; } = true;
    [JsonPropertyName("UseBigFont")]
    public bool UseBigFont { get; set; } = false;
    [JsonPropertyName("UseBigCursor")]
    public bool UseBigCursor { get; set; } = false;        
    [JsonPropertyName("Symbols")]
    public Dictionary<char, byte> Symbols { get; set; } = [];

    // config
    public bool EnableLastGameSaveing = false;
    public bool EnableCursorBling = false;
    public bool ShowIdx = true;
    public bool IdxFillZero = true;
    public int CursorBlingCount = 1;
    public int CursorSpeed = 4;

    // music
    public int MusicIdx = 0;
    public string MusicName = "";
    public int SfxIdx = 1;
    public string SfxName = "";

    static private JsonSerializerOptions JsonOptions = new JsonSerializerOptions()
    {
        WriteIndented = true,
        //DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public string ConvertIdxName(string name, int idx, int numcount)
    {
        if (ShowIdx)
        {
            if (IdxFillZero)
            {
                return String.Format($"{{0:D{numcount}}}.{{1}}", idx, name);
            }
            else
            {
                int maxid = (idx-1) / PageMenuNum;
                maxid = maxid * PageMenuNum + PageMenuNum;
                numcount = 1;
                while ((maxid /= 10) > 0)
                {
                    numcount++;
                }
                //string temp = $"{{0,-{numcount}}}.{{1}}";
                return String.Format("{0}.{1}", idx, name);
            }
        }
        else
        {
            return name;
        }
    }

    public string GenerateTitle(int gameCount)
    {
        if (Title.Contains("##"))
        {
            return Title.Replace("##", $"{gameCount}");
        }
        return Title;
    }

    static public Theme Load(string name)
    {
        string filePath = $"themes/{name}/theme.json";
        if (Path.Exists(filePath))
        {
            
            string jsonText = File.ReadAllText(filePath);
            var t = JsonSerializer.Deserialize<Theme>(jsonText, JsonOptions);
            if (t != null)
            {
                return t;
            }
        }
        return new();
    }

    public void Save(string name)
    {
        string text = JsonSerializer.Serialize(this, JsonOptions);
        string filePath = $"themes/{name}/theme.json";
        try
        {
            
            File.WriteAllText(filePath, text);
        }
        catch
        {
            MessageBox.Show($"保存theme文件错误，{filePath}");                
        }
    }

    public byte[] StringToThemeTiles(string text)
    {
        if (text.Length == 0) return [];

        var result = new byte[text.Length + 1];

        if (UseAscii)
        {
            for (int c = 0; c < result.Length; c++)
            {
                if (c < text.Length)
                {
                    result[c] = (byte)text[c];
                }
            }
        }
        else
        {
            for (int c = 0; c < result.Length; c++)
            {
                if (c < text.Length)
                {
                    byte charCode;
                    if (Symbols.TryGetValue(text[c], out charCode))
                        result[c] = charCode;
                    else
                    {                            
                        if (text[c] > '0' && text[c] <= '9')
                        {
                            if (Symbols.TryGetValue('0', out charCode))
                            {
                                result[c] = (byte)(charCode + (text[c] - '0'));
                                continue;
                            }
                        }
                        if (text[c] > 'a' && text[c] <= 'z')
                        {
                            if (Symbols.TryGetValue('a', out charCode))
                            {
                                result[c] = (byte)(charCode + (text[c] - 'a'));
                                continue;
                            }
                        }
                        if (text[c] > 'A' && text[c] <= 'Z')
                        {
                            if (Symbols.TryGetValue('A', out charCode))
                            {
                                result[c] = (byte)(charCode + (text[c] - 'A'));
                                continue;
                            }
                        }
                        result[c] = NoneCode;
                    }
                }
            }
        }
        return result;
    }

    public byte GetZeroCode()
    {
        if (UseAscii)
        {
            return (byte)'0';
        }
        else
        {
            byte charCode;
            if (Symbols.TryGetValue('0', out charCode))
                return charCode;
            return NoneCode;
        }
    }
    public byte GetDotCode()
    {
        if (UseAscii)
        {
            return (byte)'.';
        }
        else
        {
            byte charCode;
            if (Symbols.TryGetValue('.', out charCode))
                return charCode;
            return NoneCode;
        }
    }

    public int GetEnableLastGameSaveingValue()
    {
        return EnableLastGameSaveing ? 1 : 0;
    }

    public int GetUseBigFontValue()
    {
        return UseBigFont ? 1 : 0;
    }
}
