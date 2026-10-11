using com.clusterrr.Famicom.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FCCoolBuilder;

internal class CoolgirlBuilder : Builder
{
    readonly JsonSerializerOptions m_jsonOptions = new JsonSerializerOptions() {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    
    Dictionary<string, Mapper> mappers = [];
    Dictionary<string, GameFix> m_gameFixes = [];

    internal CoolgirlBuilder(Config c): base(c) {
        TargetMapperId = 342;
        DefaultOutFile = "coolgirl.nes";
    }
    private bool LoadMappers()
    {
        if (!File.Exists(config.MappersFile)) {
            problems.Add(new InvalidDataException("Mappers file not exists."));
            return false;
        }
        var mappersJson = File.ReadAllText(config.MappersFile);
        var tempmappers = JsonSerializer.Deserialize<Dictionary<string, Mapper>>(mappersJson, m_jsonOptions);
        if (tempmappers == null) {
            problems.Add(new InvalidDataException("Can't read mappers file."));
            return false;
        }

        mappers = tempmappers;
        // Add padding zeros
        // Select numeric mappers
        var mapperNumbers = mappers.Keys.Where(k => uint.TryParse(k, out uint t)).ToArray();
        foreach (var mapperNumber in mapperNumbers) {
            var t = uint.Parse(mapperNumber);
            var padded = $"{t:D3}";
            if (mapperNumber != padded) {
                mappers[padded] = mappers[mapperNumber];
                mappers.Remove(mapperNumber);
            }
        }

        return true;
    }

    private bool LoadGameFix()
    {
        if (!File.Exists(config.FixesFile)) {
            problems.Add(new InvalidDataException("Fixes file not exists."));
            return false;
        }

        var fixesJson = File.ReadAllText(config.FixesFile);
        var fixesStr = JsonSerializer.Deserialize<Dictionary<string, GameFix>>(fixesJson, m_jsonOptions);
        if (fixesStr == null) {
            problems.Add(new InvalidDataException("Can't read fixes file."));
            return false;
        }

        // Convert string CRC32 to uint
        m_gameFixes = fixesStr.ToDictionary(
                    // Check for hexademical values
                    kv => kv.Key.ToLower().StartsWith("0x")
                        ? kv.Key[2..].ToLower()
                        : kv.Key.ToLower(),
                    kv => kv.Value);

        return true;
    }

    static bool WillFit(byte[] dest, int pos, byte[] source, HashSet<int>? badSectors)
    {
        for (int addr = pos; addr < pos + source.Length; addr++) {
            if (addr % 0x2000 == 0) {
                if ((addr >= LOADER_OFFSET) && (addr < LOADER_OFFSET + LOADER_SIZE))
                    return false;
                if ((badSectors != null) && badSectors.Contains(addr / FLASH_SECTOR_SIZE))
                    return false;
            }
            if (addr >= dest.Length)
                return false;
            if ((dest[addr] != byte.MaxValue) && (dest[addr] != source[addr - pos]))
                return false;
        }
        return true;
    }

    protected override bool LoadGames(List<RomFileInfo> roms)
    {
        if (!LoadMappers()) return false;
        if (!LoadGameFix()) return false;

        m_games.Clear();
        skipinfos.Clear();

        m_result = [.. Enumerable.Repeat(byte.MaxValue, (int)(config.MaxRomSizeMB * 1024 * 1024))];

        // load games
        m_saveId = 0;
        foreach (var rom in roms) {
            Game game;
            try {
                game = Game.LoadGame(rom.FilePath, rom.MenuName, fixes: m_gameFixes);
            } catch (Exception e) {
                skipinfos.Add(new SkipFileInfo(rom.Idx, $"{e.ToString()}"));
                continue;
            }
            if (!string.IsNullOrEmpty(game.Mapper)) {
                if (!mappers.ContainsKey(game.Mapper)) {
                    skipinfos.Add(new SkipFileInfo(rom.Idx, $"Unknown mapper \"{game.Mapper}\" for \"{Path.GetFileName(game.FileName)}\""));
                    continue;
                }
            }
            if (game.CHR.Length > config.MaxChrRamSizeKB * 1024) {
                skipinfos.Add(new SkipFileInfo(rom.Idx, $"CHR size is too big in \"{Path.GetFileName(game.FileName)}\""));
                continue;
            }
            if ((game.Mirroring == MirroringType.FourScreenVram) && (game.CHR.Length > config.MaxChrRamSizeKB * 1024 - 0x1000)) {
                skipinfos.Add(new SkipFileInfo(rom.Idx, $"Four-screen mode and such big CHR ({config.MaxChrRamSizeKB}KB) is not supported for \"{Path.GetFileName(game.FileName)}\""));
                continue;
            }
            if (game.Trained) {
                skipinfos.Add(new SkipFileInfo(rom.Idx, $"Trained games are not supported for \"{game.FileName}\""));
                continue;
            }
            //
            if (game.Battery) {
                m_saveId++;
                game.SaveId = (byte)m_saveId;
            }
            m_games.Add(game);

        }
        if (m_games.Count == 0) return false;


        m_usedSpace = LOADER_SIZE;
        m_notFittedSize = 0;

        // sort by prg
        var sortprg_games = m_games.OrderByDescending(g => g.PRG.Length).Where(g => g.PRG.Length > 0);
        foreach (var game in sortprg_games) {
            Console.Write($"Fitting PRG of {Path.GetFileName(game.FileName)} ({game.PRG.Length / 1024}KB)... ");
            bool fitted = false;
            for (int pos = 0; pos < config.MaxRomSizeMB * 1024 * 1024; pos += game.PRG.Length) {
                if (WillFit(m_result, pos, game.PRG, config.BadSectors)) {
                    game.PrgOffset = pos;
                    Array.Copy(game.PRG, 0, m_result, pos, game.PRG.Length);
                    m_usedSpace = Math.Max(LOADER_OFFSET + LOADER_SIZE, Math.Max(m_usedSpace, pos + game.PRG.Length));
                    fitted = true;
                    Console.WriteLine($"offset: 0x{pos:X8}");
                    break;
                }
            }
            if (!fitted) {
                Console.WriteLine("Failed.");
                m_notFittedSize += game.PRG.Length;
            }
            GC.Collect();
        }

        // sort by chr
        var sortchr_games = m_games.OrderByDescending(g => g.CHR.Length).Where(g => g.CHR.Length > 0);
        foreach (var game in sortchr_games) {
            Console.Write($"Fitting CHR of {Path.GetFileName(game.FileName)} ({game.CHR.Length / 1024}KB)... ");
            bool fitted = false;
            for (int pos = 0; pos < config.MaxRomSizeMB * 1024 * 1024; pos += 0x2000) {
                if (WillFit(m_result, pos, game.CHR, config.BadSectors)) {
                    game.ChrOffset = pos;
                    Array.Copy(game.CHR, 0, m_result, pos, game.CHR.Length);
                    m_usedSpace = Math.Max(LOADER_OFFSET + LOADER_SIZE, Math.Max(m_usedSpace, pos + game.CHR.Length));
                    fitted = true;
                    Console.WriteLine($"offset: 0x{pos:X8}");
                    break;
                }
            }
            if (!fitted) {
                Console.WriteLine("Failed.");
                m_notFittedSize += game.CHR.Length;
            }
            GC.Collect();
        }

        // Calculate output ROM size
        m_usedSpace += m_notFittedSize;
        // Round up to minimum PRG bank size
        m_usedSpace = 0x4000 * (int)Math.Ceiling((float)m_usedSpace / (float)0x4000);
        m_romSize = m_usedSpace;
        // Round up to sector size
        m_usedSpace = FLASH_SECTOR_SIZE * (int)Math.Ceiling((float)m_usedSpace / (float)FLASH_SECTOR_SIZE);
        // Space for saves
        m_usedSpace += FLASH_SECTOR_SIZE * (int)Math.Ceiling(m_saveId / 4.0);

        // Print some stats
        Console.WriteLine($"Total games: {m_games.Count()}");
        Console.WriteLine($"Final ROM size: {Math.Round(m_usedSpace / 1024.0 / 1024.0, 3)}MB");
        Console.WriteLine($"Battery-backed games: {m_saveId}");

        return true;
    }


    protected override bool CreateRegs()
    {
        m_regs["reg_0"] = new List<string>();
        m_regs["reg_1"] = new List<string>();
        m_regs["reg_2"] = new List<string>();
        m_regs["reg_3"] = new List<string>();
        m_regs["reg_4"] = new List<string>();
        m_regs["reg_5"] = new List<string>();
        m_regs["reg_6"] = new List<string>();
        m_regs["reg_7"] = new List<string>();
        m_regs["chr_start_bank_h"] = new List<string>();
        m_regs["chr_start_bank_l"] = new List<string>();
        m_regs["chr_start_bank_s"] = new List<string>();
        m_regs["chr_count"] = new List<string>();
        m_regs["game_save"] = new List<string>();
        m_regs["game_flags"] = new List<string>();
        m_regs["cursor_pos"] = new List<string>();

        if ((m_notFittedSize > 0) && (m_usedSpace > config.MaxRomSizeMB * 1024 * 1024))
            problems.Add(new OutOfMemoryException($"ROM is too big: {Math.Round(m_usedSpace / 1024.0 / 1024.0, 3)}MB"));
        if (m_games.Count > MAX_GAME_COUNT)
            problems.Add(new InvalidDataException($"Too many ROMs: {m_games.Count} (maximum {MAX_GAME_COUNT})"));
        if (m_saveId > MAX_SAVE_COUNT)
            problems.Add(new InvalidDataException($"Too many battery backed games: {m_saveId} (maximum {byte.MaxValue})"));

        foreach (var game in m_games) {
            Mapper? mapperInfo;
            if (!string.IsNullOrEmpty(game.Mapper)) {
                if (!mappers.TryGetValue(game.Mapper, out mapperInfo)) {
                    problems.Add(new NotSupportedException($"Unknown mapper \"{game.Mapper}\" for \"{Path.GetFileName(game.FileName)}\""));
                    return false;
                }
            } else mapperInfo = new Mapper();
            if (game.CHR.Length > config.MaxChrRamSizeKB * 1024) {
                problems.Add(new InvalidDataException($"CHR size is too big in \"{Path.GetFileName(game.FileName)}\""));
                return false;
            }
            if ((game.Mirroring == MirroringType.FourScreenVram) && (game.CHR.Length > config.MaxChrRamSizeKB * 1024 - 0x1000)) {
                problems.Add(new InvalidDataException($"Four-screen mode and such big CHR ({config.MaxChrRamSizeKB}KB) is not supported for \"{Path.GetFileName(game.FileName)}\""));
                return false;
            }
            if (game.Trained) {
                problems.Add(new NotImplementedException($"Trained games are not supported for \"{game.FileName}\""));
                return false;
            }

            bool prgRamEnabled;
            var flags = mapperInfo.Flags;

            // Some unusual games
            switch (game.PrgRamSize) {
                case null:
                    prgRamEnabled = mapperInfo.PrgRamEnabled;
                    // default value
                    break;
                case 0:
                    prgRamEnabled = false;
                    break;
                case 8 * 1024:
                    prgRamEnabled = true;
                    break;
                case 16 * 1024:
                    prgRamEnabled = true;
                    flags |= mapperInfo.Flags16kPrgRam;
                    break;
                case 32 * 1024:
                    problems.Add(new NotImplementedException($"32KB of PRG RAM is not supported for \"{Path.GetFileName(game.FileName)}\""));
                    continue;
                default:
                    problems.Add(new NotImplementedException($"Weird PRG RAM value {game.PrgRamSize} for \"{Path.GetFileName(game.FileName)}\""));
                    continue;
            }
            if ((game.Flags & Game.GameFlags.WillNotWorkOnDendy) != 0)
                Console.WriteLine($"WARNING! \"{Path.GetFileName(game.FileName)}\" is not compatible with Dendy");
            if ((game.Flags & Game.GameFlags.WillNotWorkOnNtsc) != 0)
                Console.WriteLine($"WARNING! \"{Path.GetFileName(game.FileName)}\" is not compatible with NTSC consoles");
            if ((game.Flags & Game.GameFlags.WillNotWorkOnPal) != 0)
                Console.WriteLine($"WARNING! \"{Path.GetFileName(game.FileName)}\" is not compatible with PAL consoles");
            if ((game.Flags & Game.GameFlags.WillNotWorkOnNewFamiclone) != 0)
                Console.WriteLine($"WARNING! \"{Path.GetFileName(game.FileName)}\" is not compatible with new Famiclones");

            int chrBankingSize = game.CHR.Length;
            // if using CHR RAM...
            bool usingCharRam = false;
            if (chrBankingSize == 0) {
                if (!game.ChrRamSize.HasValue) {
                    // CHR RAM size is unknown
                    // if CHR RAM banking is supported by the mapper
                    // set the maximum size
                    if (mapperInfo.ChrRamBanking)
                        chrBankingSize = (int)config.MaxChrRamSizeKB * 1024;
                    else // else banking is disabled
                        chrBankingSize = 0x2000;
                } else {
                    // CHR RAM size is specified by NES 2.0 header or fixes.json file
                    chrBankingSize = game.ChrRamSize.Value;
                }
                usingCharRam = true;
            } else {
                if (game.Mapper == "074") {
                    usingCharRam = true;
                }
            }
            int prgMask = ~(game.PRG.Length / 0x4000 - 1);
            int chrMask = ~(chrBankingSize / 0x2000 - 1);

            byte @params = 0;
            if (prgRamEnabled || game.Battery) @params |= (1 << 0); // enable SRAM
            if (game.CHR.Length == 0 || usingCharRam) @params |= (1 << 1); // enable CHR write
            if (game.Mirroring == MirroringType.Horizontal) @params |= (1 << 3); // default mirroring
            if (game.Mirroring == MirroringType.FourScreenVram) @params |= (1 << 5); // four-screen mirroring
            @params |= (1 << 7); // lockout

            // TODO: replace magic numbers
            m_regs["reg_0"].Add(string.Format("${0:X2}", ((game.PrgOffset / 0x4000) >> 8) & 0xFF));                                       // none[7:5], prg_base[26:22]
            m_regs["reg_1"].Add(string.Format("${0:X2}", (game.PrgOffset / 0x4000) & 0xFF));                                              // prg_base[21:14]
            m_regs["reg_2"].Add(string.Format("${0:X2}", ((chrMask & 0x20) << 2) | (prgMask & 0x7F)));                                    // chr_mask[18], prg_mask[20:14]
            m_regs["reg_3"].Add(string.Format("${0:X2}", (mapperInfo.PrgMode << 5) | 0));                                                 // prg_mode[2:0], chr_bank_a[7:3]
            m_regs["reg_4"].Add(string.Format("${0:X2}", (byte)(mapperInfo.ChrMode << 5) | (chrMask & 0x1F)));                            // chr_mode[2:0], chr_mask[17:13]
            m_regs["reg_5"].Add(string.Format("${0:X2}", (((mapperInfo.PrgBankA & 0x1F) << 2) | (game.Battery ? 0x02 : 0x01)) & 0xFF));   // chr_bank[8], prg_bank_a[5:1], sram_page[1:0]
            m_regs["reg_6"].Add(string.Format("${0:X2}", (flags << 5) | (mapperInfo.MapperRegister & 0x1F)));                             // flag[2:0], mapper[4:0]
            m_regs["reg_7"].Add(string.Format("${0:X2}", @params | ((mapperInfo.MapperRegister & 0x20) << 1)));                           // lockout, mapper[5], four_screen, mirroring[1:0], prg_write_on, chr_write_en, sram_enabled
            m_regs["chr_start_bank_h"].Add(string.Format("${0:X2}", ((game.ChrOffset / 0x4000) >> 8) & 0xFF));
            m_regs["chr_start_bank_l"].Add(string.Format("${0:X2}", ((game.ChrOffset / 0x4000)) & 0xFF));
            m_regs["chr_start_bank_s"].Add(string.Format("${0:X2}", ((game.ChrOffset % 0x4000) >> 8) | 0x80));
            m_regs["chr_count"].Add(string.Format("${0:X2}", game.CHR.Length / 0x2000));
            m_regs["game_save"].Add(string.Format("${0:X2}", !game.Battery ? 0 : game.SaveId));
            m_regs["game_flags"].Add(string.Format("${0:X2}", (byte)game.Flags));
            m_regs["cursor_pos"].Add(string.Format("${0:X2}", game.ToString().Length));
        }
        return true;
    }
    protected override bool CreateGameASM()
    {
        // It's time to generate assembly file
        const byte baseBank = 0;
        var asmResult = new StringBuilder();
        asmResult.AppendLine("; Common constants");
        asmResult.AppendLine($"GAMES_COUNT = {m_games.Count}");
        asmResult.AppendLine($"PAGE_MENU_NUM = {m_theme.PageMenuNum}");
        asmResult.AppendLine($"MENU_TITLE_X = {m_theme.MenuTitleX}");
        asmResult.AppendLine($"MENU_TITLE_Y = {m_theme.MenuTitleY}");
        asmResult.AppendLine($"MENU_START_X = {m_theme.MenuStartX}");
        asmResult.AppendLine($"MENU_START_Y = {m_theme.MenuStartY}");
        asmResult.AppendLine($"MENU_INTER_SPACE = {m_theme.MenuInterSpace}");
        asmResult.AppendLine($"HIGHLIGHT_CODE = {m_theme.HighLightCode}");
        asmResult.AppendLine($"CURSOR_OFFSET_X = {m_theme.CursorOffsetX}");
        asmResult.AppendLine($"CURSOR_OFFSET_Y = {m_theme.CursorOffsetY}");
        asmResult.AppendLine($"CURSOR_COLUMN_COUNT = {m_theme.CursorColumnCount}");
        asmResult.AppendLine($"CURSOR_FRAME_COUNT = {m_theme.CursorFrameCount}");
        asmResult.AppendLine($"CURSOR_BLING_COUNT = {m_theme.CursorBlingCount}");
        asmResult.AppendLine($"CURSOR_CODE = {m_theme.CursorCode}");
        asmResult.AppendLine($"CURSOR_SPEED = {m_theme.CursorSpeed}");
        asmResult.AppendLine($"ZERO_CODE = {m_theme.GetZeroCode()}");
        asmResult.AppendLine($"DOT_CODE = {m_theme.GetDotCode()}");

        if (m_theme.CursorBlingCount > 1 || m_theme.CursorColumnCount > 1) asmResult.AppendLine($"CURSOR_REFRESH_TIME = {m_theme.CursorRefreshTime}");

        if (m_theme.UseBigFont) asmResult.AppendLine("ENABLE_BIG_FONT = 1");
        if (m_theme.UseBigFont || m_theme.UseBigCursor) asmResult.AppendLine("ENABLE_BIG_CURSOR = 1");
        if (m_theme.EnableLastGameSaveing) asmResult.AppendLine("ENABLE_LAST_GAME_SAVING = 1");

        if (m_theme.SfxIdx > 0 && m_theme.SfxIdx <= config.MaxSfxIdx) {
            asmResult.AppendLine($"ENABLE_SYSTEM_SOUND = {m_theme.SfxIdx}\nFAMISTUDIO_MUSIC_DATA = 0\nFAMISTUDIO_SFX_DATA = 0");
        } else {
            asmResult.AppendLine("ENABLE_SYSTEM_SOUND = 0");
            if (m_theme.MusicIdx > 0) {
                asmResult.AppendLine();
                asmResult.AppendLine(" .bank 13");
                asmResult.AppendLine(" .org $A000");
                asmResult.AppendLine("FAMISTUDIO_MUSIC_DATA");
                asmResult.AppendLine($" .include \"{config.GetAsmMusicFile()}\"");
            } else {
                asmResult.AppendLine("FAMISTUDIO_MUSIC_DATA = 0");
            }
            if (m_theme.SfxIdx > 0) {
                asmResult.AppendLine();
                asmResult.AppendLine(" .bank 14");
                asmResult.AppendLine(" .org $D000");
                asmResult.AppendLine("FAMISTUDIO_SFX_DATA");
                asmResult.AppendLine($" .include \"{config.GetAsmSfxFile()}\"");
            } else {
                asmResult.AppendLine("FAMISTUDIO_SFX_DATA = 0");
            }
        }
        asmResult.AppendLine();
        asmResult.AppendLine("; CHR Data");
        asmResult.AppendLine(" .bank 12");
        asmResult.AppendLine(" .org $8000");
        asmResult.AppendLine("chr_data:");
        asmResult.AppendLine($" .incbin \"{config.GetAsmThemeDir()}/0.chr\"");
        asmResult.AppendLine("ppu_palette_data:");
        asmResult.AppendLine($" .incbin \"{config.GetAsmThemeDir()}/0.col\"");
        asmResult.AppendLine("background_nametable_data:");
        asmResult.AppendLine($" .incbin \"{config.GetAsmThemeDir()}/0.nam\"");
        asmResult.AppendLine("background_attrtable_data:");
        asmResult.AppendLine($" .incbin \"{config.GetAsmThemeDir()}/0.atb\"");
        asmResult.AppendLine();
        asmResult.AppendLine(" .bank 15");
        asmResult.AppendLine(" .org $FF00");
        asmResult.AppendLine("menu_foot:");
        asmResult.AppendLine(BytesToAsm(m_theme.StringToThemeTiles("@COOLGIRL")));
        asmResult.AppendLine("menu_title:");
        asmResult.AppendLine(BytesToAsm(m_theme.StringToThemeTiles(m_theme.GenerateTitle(m_games.Count))));
        asmResult.AppendLine();
        asmResult.AppendLine("; Loader data");
        int regCount = 0;
        int c = 0;
        foreach (var reg in m_regs.Keys) {
            c = 0;
            foreach (var r in m_regs[reg]) {
                if (c % 256 == 0) {
                    asmResult.AppendLine();
                    asmResult.AppendLine($"  .bank {baseBank + c / 256 * 2}");
                    asmResult.AppendLine($"  .org ${0x8000 + regCount * 0x100:X4}");
                    asmResult.Append($"loader_data_{reg}{(c == 0 ? "" : $"_{c}")}:");
                }
                if (c % 16 == 0) {
                    asmResult.AppendLine();
                    asmResult.Append("  .db");
                }
                asmResult.Append(((c % 16 != 0) ? ", " : " ") + r);
                c++;
            }
            asmResult.AppendLine();
            regCount++;
        }

        asmResult.AppendLine();
        asmResult.AppendLine("; Game names");
        c = 0;
        foreach (var game in m_games) {
            if (c % 256 == 0) {
                asmResult.AppendLine();
                asmResult.AppendLine($"  .bank {baseBank + c / 256 * 2}");
                asmResult.AppendLine($"  .org $9000");
                asmResult.AppendLine($"game_names{(c == 0 ? "" : $"_{c}")}:");
            }
            asmResult.AppendLine($"  .dw game_name_{c}");
            c++;
        }

        int maxid = m_games.Count;
        int numcount = 1;
        while ((maxid /= 10) > 0) {
            numcount++;
        }

        c = 0;
        foreach (var game in m_games) {
            asmResult.AppendLine();
            if (c % 256 == 0) {
                asmResult.AppendLine();
                asmResult.AppendLine("  .bank " + (baseBank + c / 256 * 2 + 1));
                if (baseBank + c / 256 * 2 + 1 >= 62) {
                    problems.Add(new OutOfMemoryException("Bank overflow! Too many games?"));
                    return false;
                }
                asmResult.AppendLine("  .org $A000");
            }
            asmResult.AppendLine("; " + Path.GetFileName(game.FileName));
            asmResult.AppendLine("game_name_" + c + ":");
            var name = m_theme.StringToThemeTiles(m_theme.ConvertIdxName(game.MenuName, c + 1, numcount));
            var asm = BytesToAsm(name);
            asmResult.Append(asm);
            c++;
        }
        File.WriteAllText(Path.Combine(config.SourcesDir, AsmFile!), asmResult.ToString(), Encoding.GetEncoding("gbk"));
        return true;
    }



}
