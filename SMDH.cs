using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMDHEditor
{
    internal class SMDH
    {
        public static Format f;
        public class Title
        {
            public string ShortDesc { get; set; }
            public string LongDesc { get; set; }
            public string Publishier { get; set; }
            public Region region { get; set; }
        }
        public class AppSettings
        {
            public List<byte> Rating { get; set; } //0x10
            public int RegionLock { get; set; } //0x4
            public List<byte> OnlineIDs { get; set; } //0xC
            public uint FlagsTMP { get; set; }
            public List<GameFlag> Flags { get; set; }
            public short EULAVersion { get; set; }
            public short Reserved { get; set; }
            public int OADF { get; set; }
            public int CEC { get; set; }
        }

        public class Format
        {
            public string Magic { get; set; } //0x4
            public int Version { get; set; } //0x4
            public List<Title> titles { get; set; } //0x2000
            public AppSettings settings { get; set; } //0x30
            public long Reserved { get; set; } //0x8
            public List<short> IconSmall { get; set; } //0x480
            public List<short> IconBig { get; set; } //0x1200
        }
        public static Bitmap smallIcon = new Bitmap(24, 24), bigIcon = new Bitmap(48, 48);

        public static readonly Dictionary<GameFlag, string> flagDescriptions = new Dictionary<GameFlag, string>
        {
            { GameFlag.Visibility, "Visibility Flag (Required for visibility on the Home Menu)" },
            { GameFlag.AutoBoot, "Auto-boot this gamecard title" },
            { GameFlag.Allow3D, "Allow use of 3D? (For use with parental Controls)" },
            { GameFlag.RequireEULA, "Require accepting CTR EULA before being launched by Home" },
            { GameFlag.Autosave, "Autosave on exit?" },
            { GameFlag.ExtendedBanner, "Uses an Extended Banner?" },
            { GameFlag.RegionRating, "Region game rating required" },
            { GameFlag.UsesSaveData, "Uses save data?" },
            { GameFlag.RecordUsage, "Application usage is to be recorded." },
            { GameFlag.DisableSDBackup, "Disables SD Savedata Backups for this title." },
            { GameFlag.New3DSExclusive, "New 3DS exclusive title." }
        };
        static byte[] tileOrder = { 0, 1, 8, 9, 2, 3, 10, 11, 16, 17, 24, 25, 18, 19, 26, 27, 4, 5, 12, 13, 6, 7, 14, 15, 20, 21, 28, 29, 22, 23, 30, 31, 32, 33, 40, 41, 34, 35, 42, 43, 48, 49, 56, 57, 50, 51, 58, 59, 36, 37, 44, 45, 38, 39, 46, 47, 52, 53, 60, 61, 54, 55, 62, 63 };

        private static void convertBigIcon(bool toBitmap)
        {
            if (toBitmap)
            {
                int i = 0;
                for (int tile_y = 0; tile_y < 48; tile_y += 8)
                {
                    for (int tile_x = 0; tile_x < 48; tile_x += 8)
                    {
                        for (int k = 0; k < 8 * 8; k += 1)
                        {
                            int x = tileOrder[k] & 0x7;
                            int y = tileOrder[k] >> 3;
                            int color = f.IconBig[i];
                            i += 1;

                            int b = (color & 0x1f) << 3;
                            int g = ((color >> 5) & 0x3f) << 2;
                            int r = ((color >> 11) & 0x1f) << 3;

                            bigIcon.SetPixel(x + tile_x, y + tile_y, Color.FromArgb(r, g, b));
                            //this.smallIcon.SetPixel(x, y, Color.FromArgb(255, 0, 0));
                        }
                    }
                }
            }
            else
            {
                int i = 0;
                for (int tile_y = 0; tile_y < 48; tile_y += 8)
                {
                    for (int tile_x = 0; tile_x < 48; tile_x += 8)
                    {
                        for (int k = 0; k < 8 * 8; k += 1)
                        {
                            int x = tileOrder[k] & 0x7;
                            int y = tileOrder[k] >> 3;

                            int r = bigIcon.GetPixel(x + tile_x, y + tile_y).R >> 3;
                            int g = bigIcon.GetPixel(x + tile_x, y + tile_y).G >> 2;
                            int b = bigIcon.GetPixel(x + tile_x, y + tile_y).B >> 3;

                            f.IconBig[i] = (Int16)((r << 11) | (g << 5) | b);
                            i += 1;
                        }
                    }
                }
            }
        }
        private static void convertSmallIcon(bool toBitmap)
        {
            if (toBitmap)
            {
                int i = 0;
                for (int tile_y = 0; tile_y < 24; tile_y += 8)
                {
                    for (int tile_x = 0; tile_x < 24; tile_x += 8)
                    {
                        for (int k = 0; k < 8 * 8; k += 1)
                        {
                            int x = tileOrder[k] & 0x7;
                            int y = tileOrder[k] >> 3;
                            int color = f.IconSmall[i];
                            i += 1;

                            int b = (color & 0x1f) << 3;
                            int g = ((color >> 5) & 0x3f) << 2;
                            int r = ((color >> 11) & 0x1f) << 3;

                            smallIcon.SetPixel(x + tile_x, y + tile_y, Color.FromArgb(r, g, b));
                            //this.smallIcon.SetPixel(x, y, Color.FromArgb(255, 0, 0));
                        }
                    }
                }
            }
            else
            {
                int i = 0;
                for (int tile_y = 0; tile_y < 24; tile_y += 8)
                {
                    for (int tile_x = 0; tile_x < 24; tile_x += 8)
                    {
                        for (int k = 0; k < 8 * 8; k += 1)
                        {
                            int x = tileOrder[k] & 0x7;
                            int y = tileOrder[k] >> 3;

                            int r = smallIcon.GetPixel(x + tile_x, y + tile_y).R >> 3;
                            int g = smallIcon.GetPixel(x + tile_x, y + tile_y).G >> 2;
                            int b = smallIcon.GetPixel(x + tile_x, y + tile_y).B >> 3;

                            f.IconSmall[i] = (Int16)((r << 11) | (g << 5) | b);
                            i += 1;
                        }
                    }
                }
            }
        }

        public static void Read(string file)
        {
            using (BinaryReader reader = new(File.OpenRead(file)))
            {
                f = new() { Magic = Encoding.UTF8.GetString(reader.ReadBytes(4)), Version = reader.ReadInt32() };
                f.titles = [];
                for (int i = 0; i < 16; i++)
                {
                    Title tmp = new() { ShortDesc = Encoding.Unicode.GetString(reader.ReadBytes(0x80)).TrimEnd('\0').Replace("\n", "<lf>").Replace("\r", "<br>"), LongDesc = Encoding.Unicode.GetString(reader.ReadBytes(0x100)).TrimEnd('\0').Replace("\n", "<lf>").Replace("\r", "<br>"), Publishier = Encoding.Unicode.GetString(reader.ReadBytes(0x80)).TrimEnd('\0').Replace("\n", "<lf>").Replace("\r", "<br>") };
                    if (i < 12) tmp.region = (Region)(i);
                    else tmp.region = Region.NoRegion;
                    f.titles.Add(tmp);
                }
                f.settings = new AppSettings();
                f.settings.Rating = [];
                f.settings.Rating.AddRange(reader.ReadBytes(0x10));
                f.settings.RegionLock = reader.ReadInt32();
                f.settings.OnlineIDs = [];
                f.settings.OnlineIDs.AddRange(reader.ReadBytes(0xC));
                f.settings.FlagsTMP = reader.ReadUInt32();
                f.settings.Flags = [];
                foreach (var item in flagDescriptions)
                    if ((f.settings.FlagsTMP & (uint)item.Key) != 0) f.settings.Flags.Add(item.Key);
                //AnalyzeFlags(f.settings.Flags);
                f.settings.EULAVersion = reader.ReadInt16();
                f.settings.Reserved = reader.ReadInt16();
                f.settings.OADF = reader.ReadInt32();
                f.settings.CEC = reader.ReadInt32();
                f.Reserved = reader.ReadInt64();
                f.IconSmall = [];
                f.IconBig = [];
                for (int i = 0; i < 0x480 / 2; i++)
                    f.IconSmall.Add(reader.ReadInt16());
                for (int i = 0; i < 0x1200 / 2; i++)
                    f.IconBig.Add(reader.ReadInt16());
                convertSmallIcon(true);
                convertBigIcon(true);
                //Rating = [], RegionLock = reader.ReadInt32(), OnlineIDs = [], Flags = reader.ReadInt32(), EULAVersion = reader.ReadInt16(), Reserved = reader.ReadInt16(), OADF = reader.ReadInt32(), CEC = reader.ReadInt32() }
            }
        }
        /*public static byte[] Write()
        {
            List<byte> fullB = [];
            fullB.AddRange(Encoding.UTF8.GetBytes(f.Magic));
            fullB.AddRange(BitConverter.GetBytes(f.Version));
            for (int i = 0; i < f.titles.Count; i++)
            {
                List<byte> b = [];
                b.AddRange(Encoding.Unicode.GetBytes(f.titles[i].ShortDesc.Replace("<lf>", "\n").Replace("<br>", "\r")));
                b.AddRange(new byte[0x80 - b.Count]);
                b.AddRange(Encoding.Unicode.GetBytes(f.titles[i].LongDesc.Replace("<lf>", "\n").Replace("<br>", "\r")));
                b.AddRange(new byte[0x100 - b.Count]);
                b.AddRange(Encoding.Unicode.GetBytes(f.titles[i].Publishier.Replace("<lf>", "\n").Replace("<br>", "\r")));
                b.AddRange(new byte[0x80 - b.Count]);
                fullB.AddRange(b.ToArray());
            }
            fullB.AddRange((f.settings.Rating.ToArray()));
            fullB.AddRange(BitConverter.GetBytes((f.settings.RegionLock)));
            fullB.AddRange(f.settings.OnlineIDs.ToArray());
            fullB.AddRange(BitConverter.GetBytes(f.settings.FlagsTMP));
            fullB.AddRange(BitConverter.GetBytes(f.settings.EULAVersion));
            fullB.AddRange(BitConverter.GetBytes(f.settings.Reserved));
            fullB.AddRange(BitConverter.GetBytes(f.settings.OADF));
            fullB.AddRange(BitConverter.GetBytes(f.settings.CEC));
            fullB.AddRange(BitConverter.GetBytes(f.Reserved));
            for (int i = 0; i < f.IconSmall.Count; i++)
            {
                fu
            }
            fullB.AddRange(f.IconSmall.ToArray());
            fullB.AddRange(f.IconSmall.ToArray());
            return fullB.ToArray();
        }*/
        public static void Write(string file)
        {
            using (BinaryWriter writer = new(File.OpenWrite(file)))
            {
                writer.Write(Encoding.UTF8.GetBytes(f.Magic));
                writer.Write(f.Version);
                for (int i = 0; i < f.titles.Count; i++)
                {
                    List<byte> b = [];
                    b.AddRange(Encoding.Unicode.GetBytes(f.titles[i].ShortDesc.Replace("<lf>", "\n").Replace("<br>", "\r")));
                    b.AddRange(new byte[0x80 - b.Count]);
                    b.AddRange(Encoding.Unicode.GetBytes(f.titles[i].LongDesc.Replace("<lf>", "\n").Replace("<br>", "\r")));
                    b.AddRange(new byte[0x180 - b.Count]);
                    b.AddRange(Encoding.Unicode.GetBytes(f.titles[i].Publishier.Replace("<lf>", "\n").Replace("<br>", "\r")));
                    b.AddRange(new byte[0x200 - b.Count]);
                    writer.Write(b.ToArray());
                }
                writer.Write(f.settings.Rating.ToArray());
                writer.Write(f.settings.RegionLock);
                writer.Write(f.settings.OnlineIDs.ToArray());
                writer.Write(f.settings.FlagsTMP);
                writer.Write(f.settings.EULAVersion);
                writer.Write(f.settings.Reserved);
                writer.Write(f.settings.OADF);
                writer.Write(f.settings.CEC);
                writer.Write(f.Reserved);

                convertBigIcon(false);
                convertSmallIcon(false);
                for (int i = 0; i < f.IconSmall.Count; i++) 
                    writer.Write(f.IconSmall[i]);
                for (int i = 0; i < f.IconBig.Count; i++)
                    writer.Write(f.IconBig[i]);

            }
        }


    }
}
