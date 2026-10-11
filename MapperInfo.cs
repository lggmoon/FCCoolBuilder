namespace FCCoolBuilder;

class MapperInfo
{
    public ushort MapperId = 0;
    public byte SubMapperId = 0;
    public bool PrgRamEnabled = false;
    public bool ChrRamBanking = false;
    public int SaveRamSize = 0;
    public int WorkRamSize = 0;
    public int ChrRamSize = 0;
    public bool BusConflicts = false;

    public static MapperInfo FromGame(Game game)
    {
        var info = new MapperInfo();
        info.MapperId = game.MapperId;
        info.SubMapperId = game.SubMapperId;

        switch (game.MapperId)
        {
        case 0: info.SaveRamSize = 0x2000; break;
        case 1: info.SaveRamSize = 0x2000; info.PrgRamEnabled = true; break;
        case 2: info.BusConflicts = true; break;
        case 3: info.BusConflicts = true; break;
        case 4: case 118: case 189: case 206: info.SaveRamSize = 0x2000; break;
        case 5: info.SaveRamSize = 0x2000; info.WorkRamSize = 0x2000; break;
        case 7: info.BusConflicts = true; break;
        case 9: case 10: info.SaveRamSize = 0x2000; break;
        case 11: info.BusConflicts = true; break;
        case 19: info.SaveRamSize = 0x2000; break;
        case 21: case 22: case 23: case 25: info.SaveRamSize = 0x2000; break;
        case 24: case 26: info.SaveRamSize = 0x2000; break;
        case 30: info.ChrRamSize = 0x2000; break;
        case 32: info.SaveRamSize = 0x2000; break;
        case 33: case 48: info.SaveRamSize = 0x2000; break;
        case 34: info.BusConflicts = true; break;
        case 42: info.WorkRamSize = 0x2000; break;
        case 66: info.BusConflicts = true; break;
        case 67: info.ChrRamSize = 0x2000; break;
        case 69: info.SaveRamSize = 0x2000; break;
        case 70: info.BusConflicts = true; break;
        case 73: info.SaveRamSize = 0x2000; break;
        case 85: info.SaveRamSize = 0x2000; break;
        case 90: info.SaveRamSize = 0x2000; break;
        case 112: info.SaveRamSize = 0x2000; break;
        case 152: info.BusConflicts = true; break;
        case 163: info.SaveRamSize = 0x2000; break;
        default:
            if (game.PrgRamSize.HasValue)
            {
                info.PrgRamEnabled = game.PrgRamSize.Value > 0;
                info.SaveRamSize = game.PrgRamSize.Value;
            }
            break;
        }

        return info;
    }

    public byte EncodeSaveRamSize()
    {
        return SaveRamSize switch
        {
            0 => 0,
            0x2000 => 1,
            0x4000 => 2,
            0x8000 => 3,
            _ => 1
        };
    }

    public byte EncodeWorkRamSize()
    {
        return WorkRamSize switch
        {
            0 => 0,
            0x2000 => 1,
            0x4000 => 2,
            0x8000 => 3,
            _ => 0
        };
    }

    public byte EncodeBusConflicts()
    {
        return BusConflicts ? (byte)0x01 : (byte)0x00;
    }
}
