using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SMDHEditor
{
    public static class Utils
    {
        public static string[] ConvertToStringArray(object input)
        {
            // Если входное значение является массивом строк
            if (input is string[] stringArray)
            {
                return stringArray;
            }

            // Если входное значение является списком строк
            if (input is List<string> stringList)
            {
                return stringList.ToArray();
            }

            // Если входное значение является одиночной строкой
            if (input is string singleString)
            {
                // Здесь вы можете установить свой разделитель, например, запятая
                return singleString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                   .Select(s => s.Trim()) // Удаляем лишние пробелы
                                   .ToArray();
            }

            // Если входное значение является массивом объектов
            if (input is Array objectArray)
            {
                // Преобразование всех элементов массива в строки
                return objectArray.Cast<object>()
                                  .Select(obj => obj?.ToString() ?? string.Empty) // Приводим каждый элемент к строке
                                  .ToArray();
            }

            // Если входное значение — коллекция объектов (например, List<object>)
            if (input is IEnumerable<object> objectEnumerable)
            {
                return objectEnumerable.Select(obj => obj?.ToString() ?? string.Empty).ToArray();
            }

            // Если входное значение не подходит ни под один из случаев
            throw new ArgumentException("Неподдерживаемый тип входного значения", nameof(input));
        }

        public static string ReplaceSymbols(string tsvFilePath, byte[] bytes)
        {
            // Словарь соответствий байтов и символов
            List<string> Byte = new List<string>();
            List<string> Char = new List<string>();
            string[] tmp = File.ReadAllLines(tsvFilePath);
            for (int i = 0; i < tmp.Length; i++)
            {
                string[] result = tmp[i].Split('\t');
                Byte.Add(result[0]);
                Char.Add(result[1]);
            }

            string byts = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                byts += bytes[i].ToString("X2");
            }
            for (int i = 0; i < tmp.Length; i++)
            {
                byts = byts.Replace(Byte[i], Char[i]);
            }
            return byts;
        }
        /*public static string ConvertToHexFormat(string input)
        {
            string result = "";
            foreach (char c in input)
            {
                // Если символ является управляющим, конвертируем его в формат <0xXX>
                if (char.IsControl(c))
                {
                    result += $"<0x{(int)c:X2}>";
                }
                else
                {
                    result += c;
                }
            }
            return result;
        }*/
        public static string ParseBytesString(byte[] bytes)
        {
            var result = new StringBuilder();

            // Добавим информацию о командах и их длинах
            var commands = new Dictionary<ushort, bool>
        {
            { 0x08, true }, { 0x0E, true }, { 0x0F, true }, { 0x11, true }, { 0x19, true }
        };

            var commandsLength = new Dictionary<ushort, int>
        {
            { 0x08, 2 }, { 0x0E, 2 }, { 0x0F, 2 }, { 0x11, 1 }, { 0x19, 4 }
        };

            using (var memoryStream = new MemoryStream(bytes))
            using (var binaryReader = new BinaryReader(memoryStream))
            {
                while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
                {
                    byte c1 = binaryReader.ReadByte();
                    if (c1 == 0x7F)
                    {
                        if (binaryReader.BaseStream.Position % 2 != 0)
                            binaryReader.BaseStream.Position++;

                        ushort r1 = binaryReader.ReadUInt16();
                        if (r1 == 0x0)
                        {
                            break;
                        }
                        else if (r1 == 0x01)
                        {
                            result.Append("[0x01]");
                        }
                        else if (r1 == 0x02)
                        {
                            result.Append("[0x02]");
                        }
                        else
                        {
                            if (commands.ContainsKey(r1))
                            {
                                if (r1 != 0x11)
                                    binaryReader.BaseStream.Position = PositionAlignment(binaryReader.BaseStream.Position, 4);

                                var codes = new StringBuilder($"[0x{r1:X}");

                                for (int i = 0; i < commandsLength[r1]; i++)
                                {
                                    ushort value = binaryReader.ReadUInt16();
                                    if (value != 0x0)
                                    {
                                        codes.Append($", 0x{value:X}");
                                    }
                                }

                                codes.Append("]");
                                result.Append(codes);
                            }
                            else if (r1 == 0x15)
                            {
                                binaryReader.BaseStream.Position = PositionAlignment(binaryReader.BaseStream.Position, 4);

                                ushort value = binaryReader.ReadUInt16();
                                if (value == 0xFFFF)
                                {
                                    if (binaryReader.ReadUInt16() == 0xFFFF)
                                    {
                                        result.Append("</span>");
                                    }
                                }
                                else if (value != 0x0)
                                {
                                    result.Append($"<span class=\"color-{value}\">");
                                }
                            }
                            else
                            {
                                result.Append($"[0x{r1:X}]");
                            }
                        }
                    }
                    else if (c1 != 0x0)
                    {
                        result.Append((char)c1);
                    }
                }
            }

            return result.ToString();
        }
        static long PositionAlignment(long position, int alignment)
        {
            return (position + alignment - 1) / alignment * alignment;
        }
        public static int StringLength(string s)
        {
            int i = 0;
            int count = 0;
            while (i < s.Length)
            {
                char c = s[i];
                if (c == '[')
                {
                    while (s[i] != ']')
                    {
                        i++;
                    }
                }
                else if (c == '<')
                {
                    while (s[i] != '>')
                    {
                        i++;
                    }
                }
                else
                {
                    count++;
                }
                i++;
            }
            return count;
        }

        public static void WriteString(int id, string s, BinaryWriter bw)
        {
            char[] t = s.ToCharArray();
            int i = 0;
            while (i < t.Length)
            {
                char c = t[i];
                if (c == '[')
                {
                    int j = Array.IndexOf(t, ']', i);
                    if (j == -1)
                    {
                        throw new Exception(string.Format("Error in line {0} = {1}", id, s));
                    }
                    string temp = new string(t, i + 1, j - i - 1);

                    WriteAlign2Codepoint(bw, 0x7f);

                    string[] codes = Regex.Split(temp, @"[^0-9a-fA-F]+");
                    ushort[] codepoints = new ushort[codes.Length];
                    for (int k = 0; k < codes.Length; k++)
                    {
                        codepoints[k] = Convert.ToUInt16(codes[k], 16);
                    }
                    ushort firstCode = codepoints[0];
                    Array.Copy(codepoints, 1, codepoints, 0, codepoints.Length - 1);
                    Array.Resize(ref codepoints, codepoints.Length - 1);

                    if (commands.ContainsKey(firstCode))
                    {
                        if (firstCode != 0x11)
                        {
                            WriteAlign4Codepoint(bw, firstCode);
                        }
                        else
                        {
                            bw.Write((short)firstCode);
                        }

                        for (int k = 0; k < commandsLength[firstCode]; k++)
                        {
                            if (k % 2 == 0)
                            {
                                ushort code = codepoints[k / 2];
                                bw.Write((short)(code != 0 ? code : 0x0));
                            }
                            else
                            {
                                bw.Write((short)0x0);
                            }
                        }
                    }
                    else
                    {
                        foreach (ushort code in codepoints)
                        {
                            bw.Write(code);
                        }
                    }
                    i = j;
                }
                else if (c == '<')
                {
                    int j = Array.IndexOf(t, '>', i);
                    if (j == -1)
                    {
                        throw new Exception(string.Format("Error in line {0} = {1}", id, s));
                    }
                    string tag = new string(t, i + 1, j - i - 1);

                    WriteAlign2Codepoint(bw, 0x7f);

                    if (tag == "br")
                    {
                        bw.Write((short)0x01);
                    }
                    else if (tag == "hr")
                    {
                        bw.Write((short)0x02);
                    }
                    else if (tag.Contains("span"))
                    {
                        Match match = Regex.Match(tag, @"span class=""color-(\d+)""");
                        if (match.Success)
                        {
                            int colorValue = int.Parse(match.Groups[1].Value);
                            WriteAlign4Codepoint(bw, 0x15);
                            bw.Write((short)colorValue);
                            bw.Write((short)0x0);
                        }
                    }
                    else if (tag.Contains("/span"))
                    {
                        WriteAlign4Codepoint(bw, 0x15);
                        bw.Write((ushort)0xFFFF);
                        bw.Write((ushort)0xFFFF);
                    }
                    else
                    {
                        throw new Exception(string.Format("Invalid tag on {0} = {1}", id, s));
                    }
                    i = j;
                }
                else
                {
                    bw.Write(c);
                }
                i++;
            }
            WriteAlign2Codepoint(bw, 0x7f);
            WriteAlign4Codepoint(bw, 0x00);
        }

        static void WriteAlign2Codepoint(BinaryWriter bw, ushort codepoint)
        {
            bw.Write((byte)(codepoint >> 8));
            bw.Write((byte)codepoint);
        }

        static void WriteAlign4Codepoint(BinaryWriter bw, ushort codepoint)
        {
            bw.Write((byte)(codepoint >> 24));
            bw.Write((byte)(codepoint >> 16));
            bw.Write((byte)(codepoint >> 8));
            bw.Write((byte)codepoint);
        }

        // Добавим информацию о командах и их длинах
        static readonly Dictionary<ushort, bool> commands = new Dictionary<ushort, bool>
        {
            { 0x08, true }, { 0x0E, true }, { 0x0F, true }, { 0x11, true }, { 0x19, true }
        };

            static readonly Dictionary<ushort, int> commandsLength = new Dictionary<ushort, int>
        {
            { 0x08, 2 }, { 0x0E, 2 }, { 0x0F, 2 }, { 0x11, 1 }, { 0x19, 4 }
        };
        public static string[] ReplaceTags(string[] lines, string tagFile, bool exportMode)
        {
            if (!File.Exists(tagFile))
            {
                Console.WriteLine("{0} doesn't not exist", tagFile);
                return lines;
            }

            List<(string, string)> dict = new List<(string, string)>();
            string[] tagText = File.ReadAllLines(tagFile);
            int c = 1;
            foreach (string line in tagText)
            {
                string[] spl = line.Split(new string[] { " = " }, StringSplitOptions.None);
                if (spl.Length != 2)
                {
                    Console.WriteLine("Ты чё, наркоман, что ли, сука? Убери лишнее \'=\' на строке {0}", c++);
                    Console.ReadKey();
                    throw new Exception();
                }

                dict.Add((spl[0], spl[1]));
            }

            if (exportMode)
                for (int i = 0; i < lines.Length; i++)
                for (int k = 0; k < dict.Count; k++)
                    lines[i] = lines[i].Replace(dict[k].Item1, dict[k].Item2);
            else
                for (int i = 0; i < lines.Length; i++)
                for (int k = 0; k < dict.Count; k++)
                    lines[i] = lines[i].Replace(dict[k].Item2, dict[k].Item1);
            return lines;
        }

        public static byte[] EncodeMSBTUnicode(string str)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                for (int i = 0; i < str.Length; i++)
                    switch (str[i])
                    {
                        case '{':
                        {
                            List<byte> bytes = new List<byte>();
                            string sub = str.Substring(i + 1, 4);
                            short val = Convert.ToInt16(sub, 16);
                            writer.Write(val);
                            i += 5;
                            break;
                        }
                        default:
                        {
                            writer.Write(Encoding.Unicode.GetBytes(str[i].ToString()));
                            break;
                        }
                    }

                return ms.ToArray();
            }
        }

        public static string DecodeMSBTUnicode(byte[] array)
        {
            if (array.Length % 2 != 0) throw new Exception("Not unicode.");
            string result = "";
            for (int i = 0; i < array.Length; i += 2)
            {
                ushort uniCharCode = BitConverter.ToUInt16(array, i);
                switch (uniCharCode)
                {
                    case 0x0:
                    case 0x1:
                    case 0x2:
                    case 0x3:
                    case 0x4:
                    case 0x5:
                    case 0x6:
                    case 0x7:
                    case 0x8:
                    case 0x9:
                    case 0xB:
                    case 0xC:
                    case 0xE:
                    case 0xF:
                    case 0x10:
                    case 0x11:
                    case 0x12:
                    case 0x13:
                    case 0x14:
                    case 0x15:
                    case 0x16:
                    case 0x17:
                    case 0x18:
                    case 0x19:
                    case 0x1A:
                    case 0x1B:
                    case 0x1C:
                    case 0x1D:
                    case 0x1E:
                    case 0x1F:
                    case 0x96:
                    {
                        result += "{" + uniCharCode.ToString("X4") + "}";
                        break;
                    }
                    case 0xFFFF:
                    {
                        result += "{FFFF}";
                        break;
                    }
                    case 0xE000:
                    {
                        result += "{E000}";
                        break;
                    }
                    case 0xA:
                    {
                        result += "<lf>";
                        break;
                    }
                    case 0xD:
                    {
                        result += "<cr>";
                        break;
                    }
                    default:
                    {
                        result += (char)uniCharCode;
                        break;
                    }
                }
            }

            return result;
        }

        public static string GetKuriimuString(string str)
        {
            try
            {
                Func<string, byte[], string> Fix = (id, bytes) =>
                {
                    return $"n{(int)id[0]}.{(int)id[1]}:" + BitConverter.ToString(bytes);
                };

                int i;
                while ((i = str.IndexOf('\xE')) >= 0)
                {
                    string id = str.Substring(i + 1, 2);
                    byte[] data = str.Substring(i + 4, str[i + 3]).Select(c => (byte)c).ToArray();

                    str = str.Remove(i, data.Length + 4).Insert(i, $"<{Fix(id, data)}>");
                }

                return str;
            }
            catch
            {
                return str;
            }
        }

        public static void FillAlignedSpace(BinaryWriter writer, long align = 0x10, byte c = 0xAB)
        {
            long pos = writer.BaseStream.Position;
            if (pos % align != 0)
            {
                byte[] buf = new byte[align - pos % align];
                for (int i = 0; i < buf.Length; i++)
                    buf[i] = c;
                writer.Write(buf);
            }
        }

        public static string ReadFourCC(BinaryReader reader)
        {
            string result = Encoding.ASCII.GetString(reader.ReadBytes(4));
            reader.BaseStream.Position -= 4;
            return result;
        }

        public static void AlignPosition(BinaryReader reader, long align = 0x10)
        {
            long pos = reader.BaseStream.Position;
            if (pos % align != 0)
                reader.BaseStream.Position = align - pos % align + pos;
        }

        public static void AlignPosition(BinaryWriter writer, long align = 0x10)
        {
            long pos = writer.BaseStream.Position;
            if (pos % align != 0)
                writer.BaseStream.Position = align - pos % align + pos;
        }

        public static long GetAlign(BinaryWriter writer, long align = 0x10)
        {
            long pos = writer.BaseStream.Length;
            if (pos % align != 0)
                return align - pos % align;
            else return 0;
        }

        public static string ReadString(this BinaryReader binaryReader, Encoding encoding)
        {
            if (binaryReader == null) throw new ArgumentNullException("binaryReader");
            if (encoding == null) throw new ArgumentNullException("encoding");

            List<byte> data = new List<byte>();

            while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
            {
                data.Add(binaryReader.ReadByte());

                string partialString = encoding.GetString(data.ToArray(), 0, data.Count);

                if (partialString.Length > 0 && partialString.Last() == '\0')
                    return encoding.GetString(data.SkipLast(encoding.GetByteCount("\0")).ToArray()).TrimEnd('\0');
            }

            throw new InvalidDataException("Hit end of stream while reading null-terminated string.");
        }

        private static IEnumerable<TSource> SkipLast<TSource>(this IEnumerable<TSource> source, int count)
        {
            if (source == null) throw new ArgumentNullException("source");

            Queue<TSource> queue = new Queue<TSource>();

            foreach (TSource item in source)
            {
                queue.Enqueue(item);

                if (queue.Count > count) yield return queue.Dequeue();
            }
        }

        public static long[] DQbinFind(string filename)
        {
            List<long> positions = new List<long>();

            using (FileStream file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[1024]; // Буфер для чтения данных из файла
                int bytesRead;
                long currentPosition = 0;

                while ((bytesRead = file.Read(buffer, 0, buffer.Length)) > 0)
                {
                    string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    int lastIndex = -1;
                    int startIndex = 0;

                    while ((startIndex = data.IndexOf(".bin", lastIndex + 1, StringComparison.OrdinalIgnoreCase)) >= 0)
                    {
                        lastIndex = startIndex + ".bin".Length - 1;
                        long position = currentPosition + startIndex;
                        positions.Add(position);
                    }

                    currentPosition += bytesRead;
                }
            }

            return positions.ToArray();
        }


        public static int FindBytes(byte[] fileBytes, byte[] bytesToSearch)
        {
            int maxPosition = fileBytes.Length - bytesToSearch.Length;
            for (int i = 0; i <= maxPosition; i++)
            {
                bool match = true;
                for (int j = 0; j < bytesToSearch.Length; j++)
                    if (fileBytes[i + j] != bytesToSearch[j])
                    {
                        match = false;
                        break;
                    }

                if (match) return i;
            }

            return -1; // Возвращаем -1, если байты не найдены
        }

        public static int GetRelativePointerOffset(BinaryReader reader, int offset)
        {
            int result = -1;
            reader.BaseStream.Position = offset - 4;
            while (reader.BaseStream.Position > 0)
            {
                int check = reader.ReadInt32();
                int difference = (int)(offset - reader.BaseStream.Position);
                //Console.WriteLine("Pos: {0}/Diff: {1}/Value: {2}", reader.BaseStream.Position, difference, check);
                if (check == difference + 4 && difference != 0)
                    return (int)(reader.BaseStream.Position - 4);
                else
                    reader.BaseStream.Position -= 8;
            }

            return result;
        }

        public static string PersonaEncoding(byte[] namebuff)
        {
            StringBuilder sb = new StringBuilder();
            if (namebuff[0] == 0x80)
            {
                for (int i = 0; i < namebuff.Length - 1; i += 2) sb.Append((char)(namebuff[i + 1] - 0x60));
                return sb.ToString();
            }
            else if (namebuff[0] == 0x81)
            {
                for (int i = 0; i < namebuff.Length - 1; i += 2) sb.Append((char)(namebuff[i + 1] + 0x20));
                return sb.ToString();
            }
            else if (namebuff[0] == 0x82)
            {
                for (int i = 0; i < namebuff.Length - 1; i += 2) sb.Append((char)(namebuff[i + 1] - 0x100));
                return sb.ToString();
            }
            else
            {
                return Utils.ReadString(namebuff, Encoding.UTF8);
            }
        }

        public static string ApplicationDirectory { get; } =
            Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

        public static string AutoFitLine(string text, int maxLength)
        {
            string[] splitted = text.Split();
            string result = "";
            int symbols = 0;
            int line_count = 0;
            foreach (string word in splitted)
                if (word.Length + symbols > maxLength)
                {
                    result += "\n";
                    symbols = word.Length;
                    result += word;
                    line_count++;
                }
                else
                {
                    if (symbols > 0)
                    {
                        result += " ";
                        symbols++;
                    }

                    result += word;
                    symbols += word.Length;
                }

            return result;
        }

        public static byte[] ReadByteArray(BinaryReader reader, int offset, int size)
        {
            byte[] result = new byte[size];
            long savepos = reader.BaseStream.Position;
            reader.BaseStream.Position = offset;
            result = reader.ReadBytes(size);
            reader.BaseStream.Position = savepos;
            return result;
        }

        public static byte[] DecompressZlib(byte[] data)
        {
            using (MemoryStream compressedStream = new MemoryStream(data))
            using (GZipStream zipStream =
                   new GZipStream(compressedStream, System.IO.Compression.CompressionMode.Decompress))
            using (MemoryStream resultStream = new MemoryStream())
            {
                zipStream.CopyTo(resultStream);
                return resultStream.ToArray();
            }
        }

        public static byte[] CompressBytes(byte[] inputBytes)
        {
            byte[] compressedBytes;

            using (MemoryStream outputStream = new MemoryStream())
            {
                using (DeflateStream deflateStream = new DeflateStream(outputStream, CompressionMode.Compress))
                {
                    deflateStream.Write(inputBytes, 0, inputBytes.Length);
                }

                compressedBytes = outputStream.ToArray();
            }

            return compressedBytes;
        }


        public static string[] SortNames(string[] names)
        {
            // Используем метод Array.Sort для сортировки массива имен
            Array.Sort(names, StringComparer.InvariantCultureIgnoreCase);
            return names;
        }


        public static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        public static string ReadSubtitle(BinaryReader reader, int offset, bool return2pos)
        {
            string sub = string.Empty;
            long savepos = reader.BaseStream.Position;
            reader.BaseStream.Position = offset;
            sub = Utils.ReadString(reader, Encoding.UTF8);
            if (return2pos)
                reader.BaseStream.Position = savepos;
            return sub;
        }


        public static void AlignPosition(BinaryReader reader)
        {
            long pos = reader.BaseStream.Position;
            if (pos % 0x10 != 0)
                reader.BaseStream.Position = 0x10 - pos % 0x10 + pos;
        }

        public static void AlignPosition(BinaryReader reader, int align)
        {
            long pos = reader.BaseStream.Position;
            if (pos % align != 0)
                reader.BaseStream.Position = align - pos % align + pos;
        }

        public static long GetAlignLength(BinaryWriter writer, long align)
        {
            long length = 0;
            long pos = writer.BaseStream.Position;
            if (pos % align != 0)
                length = align - pos % align + pos - pos;
            return length;
        }

        public static long GetAlignLength(BinaryWriter writer)
        {
            long length = 0;
            long pos = writer.BaseStream.Position;
            if (pos % 0x10 != 0)
                length = 0x10 - pos % 0x10 + pos - pos;
            return length;
        }

        public static void AlignPosition(BinaryWriter writer, int align)
        {
            long pos = writer.BaseStream.Position;
            if (pos % align != 0)
                writer.BaseStream.Position = align - pos % align + pos;
        }

        public static long GetAlignLength(BinaryReader reader)
        {
            long length = 0;
            long pos = reader.BaseStream.Position;
            if (pos % 0x10 != 0)
                length = 0x10 - pos % 0x10 + pos - pos;
            return length;
        }

        public static string ReadString(byte[] namebuf, Encoding encoding)
        {
            BinaryReader binaryReader = new BinaryReader(new MemoryStream(namebuf));
            if (encoding == null) throw new ArgumentNullException("encoding");

            List<byte> data = new List<byte>();

            while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
            {
                data.Add(binaryReader.ReadByte());

                string partialString = encoding.GetString(data.ToArray(), 0, data.Count);

                if (partialString.Length > 0 && partialString.Last() == '\0')
                    return encoding.GetString(data.SkipLast(encoding.GetByteCount("\0")).ToArray()).TrimEnd('\0');
            }

            throw new InvalidDataException("Hit end of stream while reading null-terminated string.");
        }
    }
}