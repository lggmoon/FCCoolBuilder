using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCCoolBuilder;

class CoolemuBuilder : Builder
{
    internal CoolemuBuilder(Config c): base(c) {
        TargetMapperId = 999;
        DefaultOutFile = "coolemu.nes";
    }
    protected override bool LoadGames(List<RomFileInfo> roms)
    {
        m_games.Clear();
        skipinfos.Clear();

        // load games
        m_saveId = 0;
        foreach (var rom in roms) {
            Game game;
            try {
                game = Game.LoadGame(rom.FilePath, rom.MenuName);
            } catch (Exception e) {
                skipinfos.Add(new SkipFileInfo(rom.Idx, $"{e.ToString()}"));
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

        m_usedSpace += LOADER_SIZE;
        // sort by size
        var sort_games = m_games.OrderByDescending(g => g.FileData.Length)
            .Where(g => g.FileData.Length > 0);
        foreach (var game in sort_games) {
            int blocksize = (game.FileData.Length + 4 + 1023) / 1024 * 1024;
            Console.Write($"Fitting FileSize of {Path.GetFileName(game.FileName)} ({blocksize}KB)... ");
            game.FileOffset = m_usedSpace;
            m_usedSpace += blocksize;
            Console.WriteLine($"offset: 0x{game.FileOffset:X8}");
            GC.Collect();
        }

        m_result = Enumerable.Repeat(byte.MaxValue, m_usedSpace).ToArray();
        foreach (var game in sort_games) {
            byte[] lenbtyes = BitConverter.GetBytes(game.FileData.Length);
            Array.Copy(lenbtyes, 0, m_result, game.FileOffset, 4);
            Array.Copy(game.FileData, 0, m_result, game.FileOffset + 4, game.FileData.Length);
            GC.Collect();
        }
        m_romSize = m_usedSpace;

        // Print some stats
        Console.WriteLine($"Total games: {m_games.Count()}");
        Console.WriteLine($"Final ROM size: {Math.Round(m_usedSpace / 1024.0 / 1024.0, 3)}MB");
        return true;
    }

    protected override bool CreateRegs()
    {
        m_regs["offset_l"] = new List<string>();
        m_regs["offset_h"] = new List<string>();
        m_regs["size_l"] = new List<string>();
        m_regs["size_h"] = new List<string>();
        m_regs["idx_l"] = new List<string>();
        m_regs["idx_h"] = new List<string>();

        ushort idx = 1;
        foreach (var game in m_games) {
            ushort gameOffset = (ushort)(game.FileOffset / 1024);
            ushort gameSize = (ushort)((game.FileData.Length + 1023) / 1024);
            m_regs["offset_l"].Add(string.Format("${0:X2}", (byte)(gameOffset & 0xFF)));
            m_regs["offset_h"].Add(string.Format("${0:X2}", (byte)(gameOffset >> 8) & 0xFF));
            m_regs["size_l"].Add(string.Format("${0:X2}", (byte)(gameSize & 0xFF)));
            m_regs["size_h"].Add(string.Format("${0:X2}", (byte)(gameSize >> 8) & 0xFF));
            m_regs["idx_l"].Add(string.Format("${0:X2}", (byte)(idx & 0xFF)));
            m_regs["idx_h"].Add(string.Format("${0:X2}", (byte)(idx >> 8) & 0xFF));
            idx++;
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
        asmResult.AppendLine(BytesToAsm(m_theme.StringToThemeTiles("@COOLEMU")));
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
                    asmResult.Append($"loader_data_game_{reg}{(c == 0 ? "" : $"_{c}")}:");
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

        File.WriteAllText(Path.Combine(config.SourcesDir, AsmFile), asmResult.ToString(), Encoding.GetEncoding("gbk"));
        return true;
    }
}
