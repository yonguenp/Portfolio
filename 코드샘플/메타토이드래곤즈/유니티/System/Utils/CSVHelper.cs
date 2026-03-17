using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;

namespace SandboxNetwork.Tools
{
    public class CSVHelper
    {
        static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
        static string LINE_SPLIT_RE = @"\r\n|\n\r|\r";

        public static List<Dictionary<string, object>> Read(string file)
        {
            var list = new List<Dictionary<string, object>>();
            TextAsset data = Resources.Load(file) as TextAsset;

            if (data == null)
            {
                Debug.Log(file + " not exist file");
                return null;
            }

            var lines = Regex.Split(data.text, LINE_SPLIT_RE);

            if (lines.Length <= 1) return list;

            var header = Regex.Split(lines[0], SPLIT_RE);
            for (var i = 1; i < lines.Length; i++)
            {
                var values = Regex.Split(lines[i], SPLIT_RE);
                if (values.Length == 0 || values[0] == "") continue;

                var entry = new Dictionary<string, object>();
                for (var j = 0; j < header.Length && j < values.Length; j++)
                {
                    string value = values[j];

                    if(value.Length > 1)
                    {
                        var first = value[0];
                        var last = value[value.Length - 1];

                        if (first == '\"' && last == '\"')
                        {
                            value = value.Substring(1, value.Length - 2);
                            value = ReplaceValue(value);
                            object fvalue = value;
                            int p;
                            float o;
                            if (int.TryParse(value, out p))
                            {
                                fvalue = p;
                            }
                            else if (float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out o))
                            {
                                fvalue = o;
                            }
                            entry[header[j]] = fvalue;

                            continue;
                        }
                    }

                    value = ReplaceValue(value);
                    object finalvalue = value;
                    int n;
                    float f;
                    if (int.TryParse(value, out n))
                    {
                        finalvalue = n;
                    }
                    else if (float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture,  out f))
                    {
                        finalvalue = f;
                    }
                    entry[header[j]] = finalvalue;
                }
                list.Add(entry);
            }
            return list;
        }

        public static string ReplaceValue(string value)
        {
            value = value.Replace("\"\"", "\"");
            value = value.Replace("\\n", "\n");
            value = value.Replace("\\r", "\r");
            value = value.Replace("\\r\\n", "\r\n");
            value = value.Replace("\\n\\r", "\n\r");
            return value;
        }

        public static void WriteToFile(string filePath, string objectToWrite, bool append = false)
        {
            StreamWriter stream = new StreamWriter(filePath);
            using (stream)
            {
                stream.Write(objectToWrite);
                stream.Dispose();
                stream.Close();
            }
        }

        public static string[] Split(string _str, string _cut)
        {
            var splits = Regex.Split(_str, _cut);

            return splits;
        }

        public static List<Dictionary<string, object>> ReadToFile(string filePath)
        {
            var list = new List<Dictionary<string, object>>();

            if (!File.Exists(filePath))
                WriteToFile(filePath, "");
            StreamReader stream = new StreamReader(filePath);
            using (stream)
            {
                if (!stream.EndOfStream)
                {
                    var header = Regex.Split(stream.ReadLine(), SPLIT_RE);

                    int count = 0;
                    while (true)
                    {
                        var line = stream.ReadLine();
                        if (line == null)
                            break;
                        if (count > 150000)
                            break;

                        count++;

                        var values = Regex.Split(line, SPLIT_RE);
                        if (values.Length == 0 || values[0] == "") continue;

                        var entry = new Dictionary<string, object>();
                        for (var j = 0; j < header.Length && j < values.Length; j++)
                        {
                            string value = values[j];

                            if (value.Length > 1)
                            {
                                var first = value[0];
                                var last = value[value.Length - 1];

                                if (first == '\"' && last == '\"')
                                {
                                    value = value.Substring(1, value.Length - 2);
                                    value = ReplaceValue(value);
                                    object fvalue = value;
                                    int p;
                                    float o;
                                    if (int.TryParse(value, out p))
                                    {
                                        fvalue = p;
                                    }
                                    else if (float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out o))
                                    {
                                        fvalue = o;
                                    }
                                    entry[header[j]] = fvalue;

                                    continue;
                                }
                            }

                            value = ReplaceValue(value);
                            object finalvalue = value;
                            int n;
                            float f;
                            if (int.TryParse(value, out n))
                            {
                                finalvalue = n;
                            }
                            else if (float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out f))
                            {
                                finalvalue = f;
                            }
                            entry[header[j]] = finalvalue;
                        }
                        list.Add(entry);
                    }
                }

                stream.Dispose();
                stream.Close();
            }

            return list;
        }
        public static void WriteToFileEncrypt(string filePath, string objectToWrite, bool append = false)
        {
            StreamWriter stream = new StreamWriter(filePath);
            using (stream)
            {
                var data = Crypt.Encrypt(objectToWrite, Crypt.key);
                stream.Write(data);
                stream.Dispose();
                stream.Close();
            }
        }
        public static List<Dictionary<string, object>> ReadToFileDecrypt(string filePath)
        {
            var list = new List<Dictionary<string, object>>();

            if (!File.Exists(filePath))
                WriteToFile(filePath, "");
            StreamReader stream = new StreamReader(filePath);
            using (stream)
            {
                if (!stream.EndOfStream)
                {
                    var data = Crypt.Decrypt(stream.ReadToEnd(), Crypt.key);

                    var lines = Regex.Split(data, "\n");
                    var header = Regex.Split(lines[0], SPLIT_RE);

                    for (int i = 1; i < lines.Length; ++i)
                    {
                        var line = lines[i];
                        if (line.CompareTo("") == 0) continue;

                        var values = Regex.Split(line, SPLIT_RE);
                        if (values.Length == 0 || values[0] == "") continue;

                        var entry = new Dictionary<string, object>();
                        for (var j = 0; j < header.Length && j < values.Length; j++)
                        {
                            string value = values[j];

                            if (value.Length > 1)
                            {
                                var first = value[0];
                                var last = value[value.Length - 1];

                                if (first == '\"' && last == '\"')
                                {
                                    value = value.Substring(1, value.Length - 2);
                                    value = value.Replace("\"\"", "\"");
                                    object fvalue = value;
                                    int p;
                                    float o;
                                    if (int.TryParse(value, out p))
                                    {
                                        fvalue = p;
                                    }
                                    else if (float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out o))
                                    {
                                        fvalue = o;
                                    }
                                    entry[header[j]] = fvalue;

                                    continue;
                                }
                            }

                            value = value.Replace("\"\"", "\"");
                            object finalvalue = value;
                            int n;
                            float f;
                            if (int.TryParse(value, out n))
                            {
                                finalvalue = n;
                            }
                            else if (float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out f))
                            {
                                finalvalue = f;
                            }
                            entry[header[j]] = finalvalue;
                        }
                        list.Add(entry);
                    }
                }

                stream.Dispose();
                stream.Close();
            }

            return list;
        }


        public static void WriteToBinaryFile<T>(string filePath, T objectToWrite, bool append = false)
        {
            using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, objectToWrite);
                stream.Close();
            }
        }
        public static T ReadFromBinaryFile<T>(string filePath)
        {
            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                var deserialize = (T)binaryFormatter.Deserialize(stream);
                stream.Close();
                return deserialize;
            }
        }
    }
}

