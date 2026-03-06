using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CSVReader
{
    static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
    static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
    static char[] TRIM_CHARS = { '\"' };

    public static List<Dictionary<string, string>> Read(string file, char splitPivot = char.MinValue)
    {
        TextAsset data = Resources.Load(file) as TextAsset;

        return ReadData(data.text, splitPivot);
    }

    public static List<Dictionary<string, string>> ReadData(string text, char splitPivot = char.MinValue)
    {
        var list = new List<Dictionary<string, string>>();

        var lines = Regex.Split(text, LINE_SPLIT_RE);

        if (lines.Length <= 1) return list;



        string[] header;
        if (splitPivot == char.MinValue)
        {
            header = Regex.Split(lines[0], SPLIT_RE);
            for (var i = 1; i < lines.Length; i++)
            {

                var values = Regex.Split(lines[i], SPLIT_RE);
                if (values.Length == 0 || values[0] == "") continue;

                var entry = new Dictionary<string, string>();
                for (var j = 0; j < header.Length && j < values.Length; j++)
                {
                    string value = values[j];
                    value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");

                    value = value.Replace("<br>", "\n"); // 추가된 부분. 개행문자를 \n대신 <br>로 사용한다.
                    value = value.Replace("<c>", ",");

                    entry[header[j]] = value;
                }
                list.Add(entry);
            }
        }
        else
        {
            header = lines[0].Split(splitPivot);
            for (var i = 1; i < lines.Length; i++)
            {

                var values = lines[i].Split(splitPivot);
                if (values.Length == 0 || values[0] == "") continue;

                var entry = new Dictionary<string, string>();
                for (var j = 0; j < header.Length && j < values.Length; j++)
                {
                    entry[header[j]] = values[j];
                }
                list.Add(entry);
            }
        }

        return list;
    }
}