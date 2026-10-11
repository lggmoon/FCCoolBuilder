using com.clusterrr.Famicom.Containers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FCCoolBuilder;

class RomFileInfo
{
    public string FilePath;
    public string MenuName;
    public int Idx;

    public RomFileInfo(string filepath, string menuname, int idx)
    {
        FilePath = filepath;
        MenuName = menuname;
        Idx = idx;
    }
}

class SkipFileInfo
{
    public int idx;
    public string text;
    public SkipFileInfo(int idx, string text)
    {
        this.idx = idx;
        this.text = text;
    }
}

abstract class Builder
{
    protected const int LOADER_OFFSET = 0;
    protected static int LOADER_SIZE = 128 * 1024;
    protected const int FLASH_SECTOR_SIZE = 128 * 1024;
    protected const int MAX_GAME_COUNT = 256 * 6;
    protected const int MAX_SAVE_COUNT = byte.MaxValue;

    protected Config config = new();
    protected Theme m_theme = new();
    protected List<Exception> problems = [];
    protected List<SkipFileInfo> skipinfos = [];
    protected Dictionary<string, List<string>> m_regs = [];
    protected List<Game> m_games = [];

    protected byte[] m_result = [];
    protected int m_romSize = 0;
    protected int m_usedSpace = 0;
    protected int m_notFittedSize = 0;
    protected int m_saveId = 0;

    protected string SourcesDir = string.Empty;
    protected string AsmFile = string.Empty;

    protected string DefaultOutFile = "multicard.nes";

    protected ushort TargetMapperId = 0;

    protected abstract bool LoadGames(List<RomFileInfo> roms);
    protected abstract bool CreateRegs();
    protected abstract bool CreateGameASM();

    internal Builder(Config c)
    {
        config = c;
        SourcesDir = c.GetCardtypeSourcesDir();
        AsmFile = Path.Combine(SourcesDir, "build.asm");
    }

    public bool Build(List<RomFileInfo> roms, string? outpath=null)
    {
        problems.Clear();

        m_usedSpace = 0;

        if (!LoadTheme()) return false;

        if (!LoadGames(roms)) return false;

        if (!CreateRegs()) return false;

        if (!CreateGameASM()) return false;

        if (!CompileASM()) return false;

        if (string.IsNullOrEmpty(outpath)) {
            outpath = Path.Combine(Config.GetConfigDir(), DefaultOutFile);
        }
        Save(outpath);

        return true;
    }

    protected bool LoadTheme()
    {
        m_theme = Theme.Load(config.ThemeName);
        m_theme.EnableLastGameSaveing = config.EnableLastGameSaveing;
        m_theme.EnableCursorBling = config.EnableCursorBling;
        m_theme.ShowIdx = config.ShowIdx;
        m_theme.IdxFillZero = config.IdxFillZero;
        m_theme.CursorBlingCount = config.CursorBlingCount;
        m_theme.CursorSpeed = config.CursorSpeed;

        m_theme.MusicIdx = config.MusicIdx;
        m_theme.MusicName = config.MusicName;
        m_theme.SfxIdx = config.SfxIdx;
        m_theme.SfxName = config.SfxName;
        return true;
    }

    protected bool CompileASM()
    {
        Console.Write("Compiling using nesasm... ");
        if (m_romSize < m_result.Length) Array.Resize(ref m_result, m_romSize);

        var process = new Process();
        var cp866 = CodePagesEncodingProvider.Instance.GetEncoding(866) ?? Encoding.ASCII;
        process.StartInfo.FileName = config.NesAsmFile;
        //process.StartInfo.Arguments = $"\"main.asm\" -r -o - -C \"GAMES_DB={config.AsmFile}\" " + config.NesAsmArgs;
        process.StartInfo.Arguments = "\"main.asm\" -raw -";
        process.StartInfo.WorkingDirectory = SourcesDir;
        process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.StandardOutputEncoding = cp866;
        process.StartInfo.StandardErrorEncoding = cp866;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.Start();

        int b;
        var stdout = new List<char>();
        var stderr = new StringBuilder();

        while (!process.HasExited || !process.StandardOutput.EndOfStream || !process.StandardError.EndOfStream) {
            while ((b = process.StandardOutput.Read()) >= 0)
                stdout.Add((char)b);
            while ((b = process.StandardError.Read()) >= 0)
                stderr.Append((char)b);
            Thread.Sleep(10);
        }

        if (stderr.Length > 0)
            Console.WriteLine(stderr);
        if (process.ExitCode != 0 || stderr.Length > 0) {
            Console.WriteLine(string.Join("", stdout));
            problems.Add(new InvalidOperationException($"nesasm returned error code {process.ExitCode}. Args: {stderr}"));
            return false;
        }

        var loader = cp866.GetBytes(stdout.ToArray());
        if (!loader.Any()) {
            problems.Add(new InvalidDataException($"nesasm returned empty data, maybe version is too old?. Args: {stderr}"));
            return false;
        }

        Array.Copy(loader, 0, m_result, LOADER_OFFSET, loader.Length);
        Console.WriteLine("OK");
        return true;
    }

    protected void Save(string outpath)
    {
        Console.Write("Saving NES file... ");
        NesFile nes = new() {
            Version = NesFile.iNesVersion.NES20,
            PRG = m_result!,
            CHR = Array.Empty<byte>(),
            Mapper = TargetMapperId,
            PrgNvRamSize = 32 * 1024,
            ChrRamSize = config.MaxChrRamSizeKB * 1024,
            Battery = true
        };
        nes.Save(outpath);
        Console.WriteLine("OK");
    }

    protected static byte[] StringToTiles(string text, Dictionary<char, byte> symbolTable)
    {
        text = text.ToUpper();
        var result = new byte[text.Length + 1];
        for (int c = 0; c < result.Length; c++) {
            if (c < text.Length) {
                byte charCode;
                if (symbolTable.TryGetValue(text[c], out charCode))
                    result[c] = charCode;
                else
                    result[c] = 0xFF;
            }
        }
        return result;
    }

    protected static string BytesToAsm(byte[] name)
    {
        var asmResult = new StringBuilder();
        for (int ch = 0; ch < name.Length; ch++) {
            if (ch % 15 == 0) {
                if (ch > 0) asmResult.AppendLine();
                asmResult.Append("  .db");
            }
            asmResult.AppendFormat(((ch % 15 > 0) ? "," : "") + " ${0:X2}", name[ch]);
        }
        asmResult.AppendLine();
        return asmResult.ToString();
    }

    protected static string FirstCharToUpper(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        return input.First().ToString().ToUpper() + input[1..];
    }

    public int GetSkipInfoCnt()
    {
        return skipinfos.Count;
    }
    public string GetSkipInfoText()
    {
        StringBuilder outer = new();
        outer.AppendLine($"{skipinfos.Count} files skip.");
        foreach (var sk in skipinfos) {
            outer.AppendLine($"Idx:#{sk.idx}, {sk.text}");
        }
        return outer.ToString();
    }

    public List<int> GetSkipInfoIdxList()
    {
        List<int> ls = [];
        foreach (var sk in skipinfos) {
            ls.Add(sk.idx);
        }
        return ls;
    }

    public int GetProblemCnt()
    {
        return problems.Count;
    }
    public string GetProblemText()
    {
        StringBuilder outer = new();
        int cnt = 0;
        foreach (var problem in problems) {
            cnt++;
            outer.AppendLine($"{cnt} . {problem.ToString()}");
        }

        return outer.ToString();
    }
}
