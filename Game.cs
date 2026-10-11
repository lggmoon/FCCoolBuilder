using com.clusterrr.Famicom.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Intrinsics.Arm;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FCCoolBuilder;

internal class Game
{
    public enum NesContainerType { iNES = 1, UNIF = 2 };

    [Flags]
    public enum GameFlags
    {
        None = 0,
        WillNotWorkOnNtsc = 0x01,
        WillNotWorkOnPal = 0x02,
        WillNotWorkOnDendy = 0x04,
        WillNotWorkOnNewFamiclone = 0x08,
        Hidden = 0x10,
        Separator = 0x80,
    };


    // coolemu
    public byte[] FileData = [];
    public int FileOffset = 0;

    // coolgirl
    public byte[] PRG = [];
    public int PrgOffset = 0;
    public byte[] CHR = [];
    public int ChrOffset = 0;

    // common
    public string FileName = "";
    public string MenuName = "";
    public int PrgLength = 0;
    public int ChrLength = 0;
    public ushort MapperId = 0;
    public byte SubMapperId = 0;
    public MirroringType Mirroring = MirroringType.Vertical;
    public bool Battery = false;
    public NesContainerType ContainerType = NesContainerType.iNES;
    public bool Trained = false;
    public int? PrgRamSize = null;
    public int? ChrRamSize = null;
    public byte SaveId = 0;
    public string crc32 = string.Empty;
    public string md5 = string.Empty;
    public GameFlags Flags = GameFlags.None;

    public bool ChrIsRam => ChrLength == 0;

    public int ChrRamOrRomSize {
        get {
            if (ChrLength > 0) return ChrLength;
            return ChrRamSize ?? 0x2000;
        }
    }
    public string Mapper;
    public string MapperName;

    public Game(NesFile nesFile)
    {
        PRG = nesFile.PRG;
        CHR = nesFile.CHR;
        PrgLength = nesFile.PRG.Length;
        ChrLength = nesFile.CHR.Length;
        Battery = nesFile.Battery;
        MapperId = nesFile.Mapper;
        SubMapperId = nesFile.Submapper;
        Mapper = $"{MapperId:D3}" + (SubMapperId > 0 ? $":{SubMapperId}" : "");
        MapperName = $"{MapperId}" + (SubMapperId > 0 ? $":{SubMapperId}" : "");
        Mirroring = nesFile.Mirroring;
        ContainerType = NesContainerType.iNES;
        Trained = nesFile.Trainer != null && nesFile.Trainer.Length > 0;
        if (nesFile.Version == NesFile.iNesVersion.NES20) {
            PrgRamSize = (int?)(nesFile.PrgRamSize + nesFile.PrgNvRamSize);
            ChrRamSize = (int?)(nesFile.ChrRamSize + nesFile.ChrNvRamSize);
        }
        crc32 = $"{nesFile.CalculateCRC32():x08}";
        var md5full = nesFile.CalculateMD5();
        md5 = $"{md5full[8]:x02}{md5full[9]:x02}{md5full[10]:x02}{md5full[11]:x02}{md5full[12]:x02}{md5full[13]:x02}{md5full[14]:x02}{md5full[15]:x02}"; // lower 8 bytes of MD5
    }

    public Game(UnifFile unifFile)
    {
        PRG = [.. unifFile.Where(k => k.Key.StartsWith("PRG")).OrderBy(k => k.Key).SelectMany(i => i.Value)];
        CHR = [.. unifFile.Where(k => k.Key.StartsWith("CHR")).OrderBy(k => k.Key).SelectMany(i => i.Value)];
        PrgLength = PRG.Length;
        ChrLength = CHR.Length;
        Battery = unifFile.Battery ?? false;
        MapperId = unifFile.Mapper != null ? UnifMapper.GetNesMapper(unifFile.Mapper) : UnifMapper.UnknowMapperID;
        Mapper = $"{MapperId:D3}" + (SubMapperId > 0 ? $":{SubMapperId}" : "");
        MapperName = $"{MapperId}" + (SubMapperId > 0 ? $":{SubMapperId}" : "");
        Mirroring = unifFile.Mirroring ?? MirroringType.MapperControlled;
        ContainerType = NesContainerType.UNIF;
        crc32 = $"{unifFile.CalculateCRC32():x08}";
        var md5full = unifFile.CalculateMD5();
        md5 = $"{md5full[8]:x02}{md5full[9]:x02}{md5full[10]:x02}{md5full[11]:x02}{md5full[12]:x02}{md5full[13]:x02}{md5full[14]:x02}{md5full[15]:x02}"; // lower 8 bytes of MD5
    }

    public override string ToString() => MenuName;

    public static string Limit(string name)
    {
        name = name.Trim();
        if (name.Length > 29) name = name[..26].Trim() + "...";
        return name.Trim();
    }

    public static string AutoMenuName(string filename)
    {
        return Limit(Path.GetFileNameWithoutExtension(filename).Replace("_", " ").ToUpper().Replace(", THE", "").Trim());
    }

    public static Game TryLoadRom(string filename)
    {
        Game game;
        byte[] data = File.ReadAllBytes(filename);
        var ext = Path.GetExtension(filename).ToLower();
        if (ext == ".nes") {
            try {
                NesFile nesFile = NesFile.FromBytes(data);
                game = new Game(nesFile);
            } catch {
                UnifFile unifFile = UnifFile.FromBytes(data);
                game = new Game(unifFile);
            }
            game.FileData = data;
            return game;
        } else if (ext == ".unf" || ext == ".unif") {
            try {
                UnifFile unifFile = UnifFile.FromBytes(data);
                game = new Game(unifFile);
            } catch {
                NesFile nesFile = NesFile.FromBytes(data);
                game = new Game(nesFile);
            }
            game.FileData = data;
            return game;
        }
        throw new InvalidDataException($"Unknown file extension: {ext}");
    }

    public static Game LoadGame(string filePath, string? menuName = null, Dictionary<string, GameFix>? fixes = null)
    {
        Game game = TryLoadRom(filePath);
        if (fixes is not null) {
            game.FillInfo(filePath, menuName, fixes);
        } else {
            game.FileName = System.IO.Path.GetFileName(filePath);
            game.MenuName = menuName ?? System.IO.Path.GetFileNameWithoutExtension(filePath);
        }
        return game;
    }

    private void FillInfo(string filename, string? menuName = null, Dictionary<string, GameFix>? fixes = null)
    {
        Console.WriteLine($"Loading {Path.GetFileName(filename)}...");
        FileName = filename;
        if (string.IsNullOrWhiteSpace(menuName)) {
            // Menu name based on filename
            MenuName = Limit(Regex.Replace(Path.GetFileNameWithoutExtension(filename), @"( ?\[.*?\])|( \(.\))", string.Empty).Replace("_", " ").ToUpper().Replace(", THE", "").Trim());
        } else {
            MenuName = Limit(menuName.Trim());
        }
        // Check for fixes database
        if (fixes != null) {
            if (fixes.TryGetValue(crc32, out GameFix? fix) || fixes.TryGetValue(md5, out fix)) {
                if (!string.IsNullOrEmpty(fix.Mapper) && Mapper != fix.Mapper) {
                    Mapper = fix.Mapper;
                    Console.WriteLine($"Fix based on checksum: {Path.GetFileName(filename)} has {fix.Mapper} mapper");
                }
                if (!string.IsNullOrEmpty(fix.Mirroring) && (Mapper.ToString() != fix.Mirroring)) {
                    Mirroring = Enum.Parse<MirroringType>(fix.Mirroring);
                    Console.WriteLine($"Fix based on checksum: {Path.GetFileName(filename)} has {fix.Mirroring} mirroring type");
                }
                if (fix.PrgRamSize.HasValue && (PrgRamSize != fix.PrgRamSize * 1024)) {
                    PrgRamSize = (int?)(fix.PrgRamSize * 1024);
                    Console.WriteLine($"Fix based on checksum: {Path.GetFileName(filename)} has {fix.PrgRamSize}KB PRG RAM");
                }
                if (fix.ChrRamSize.HasValue && (ChrRamSize != fix.ChrRamSize * 1024)) {
                    ChrRamSize = (int?)(fix.ChrRamSize * 1024);
                    Console.WriteLine($"Fix based on checksum: {Path.GetFileName(filename)} has {fix.ChrRamSize}KB CHR RAM");
                }
                if (fix.Battery.HasValue && (Battery != fix.Battery.Value)) {
                    Battery = fix.Battery.Value;
                    Console.WriteLine($"Fix based on checksum: {Path.GetFileName(filename)} battery saves = {fix.Battery}");
                }
                if (fix.WillNotWorkOnPal) {
                    Flags |= GameFlags.WillNotWorkOnPal;
                }
                if (fix.WillNotWorkOnNtsc) {
                    Flags |= GameFlags.WillNotWorkOnNtsc;
                }
                if (fix.WillNotWorkOnDendy) {
                    Flags |= GameFlags.WillNotWorkOnDendy;
                }
                if (fix.WillNotWorkOnNewFamiclone) {
                    Flags |= GameFlags.WillNotWorkOnNewFamiclone;
                }
            }
        }
        // External NTRAM is not supported on new famiclones
        if (Mirroring == MirroringType.FourScreenVram)
            Flags |= GameFlags.WillNotWorkOnNewFamiclone;
        // Check for round sizes and add padding
        if (PRG.Length > 0) {
            uint roundSize = 1;
            while (roundSize < PRG.Length)
                roundSize <<= 1;
            if (roundSize > PRG.Length) {
                PRG = Enumerable.Concat(PRG, Enumerable.Repeat(byte.MaxValue, (int)(roundSize - PRG.Length))).ToArray();
            }
        }
        if (CHR.Length > 0) {
            uint roundSize = 1;
            while (roundSize < CHR.Length)
                roundSize <<= 1;
            if (roundSize > CHR.Length)
                CHR = Enumerable.Concat(CHR, Enumerable.Repeat(byte.MaxValue, (int)(roundSize - CHR.Length))).ToArray();
        }
    }
}
