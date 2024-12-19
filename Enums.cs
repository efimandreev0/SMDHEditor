using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMDHEditor
{
    public enum Region
    {
        Japanese, 
        English,
        French,
        German,
        Italian,
        Spanish,
        SimplifiedChinese,
        Korean,
        Dutch,
        Portugese,
        Russian,
        TraditionalChinese,
        NoRegion
    }
    public enum GameFlag : uint
    {
        Visibility = 0x0001,
        AutoBoot = 0x0002,
        Allow3D = 0x0004,
        RequireEULA = 0x0008,
        Autosave = 0x0010,
        ExtendedBanner = 0x0020,
        RegionRating = 0x0040,
        UsesSaveData = 0x0080,
        RecordUsage = 0x0100,
        DisableSDBackup = 0x0400,
        New3DSExclusive = 0x1000
    }

}
