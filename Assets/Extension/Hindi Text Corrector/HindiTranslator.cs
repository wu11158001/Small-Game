using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;

public class HindiTranslator
{
    /// <summary>
    /// 可替換的印度字
    /// </summary>
    private static string[] arrayHindi = new string[] {
        "् ","०",  "१",  "२",  "३",     "४",   "५",  "६",   "७",   "८",   "९", "x","न्न",

        "ल्म",

        "ङ",

        "ऩ",

        "ऱ",

        "य़",

        "ग़",

        "ड़",

        "ढ़",

        "ख़्य", "ख़्","ख़",

        "क़्य", "क़्", "क़",

        "फ़्","फ़",

        "ज़्य","ज़्","ज़",

        "त्त्", "त्त", "क्त",  "दृ",  "कृ",

        "ह्न",  "ह्य",  "हृ",  "ह्म",  "ह्र",  "ह्",   "द्द",  "क्ष्", "क्ष", "त्र्", "त्र","ज्ञ",
        "छ्य",  "ट्य",  "ठ्य",  "ड्य",  "ढ्य", "द्य","द्व",
        "श्र",  "ट्र",    "ड्र",    "ढ्र",    "छ्र",   "क्र",  "फ्र",  "द्र",   "प्र",   "ग्र", "रु",  "रू",
        "्र",

        "ओ",  "औ",  "आ",   "अ",   "ई",   "इ",  "उ",   "ऊ",  "ऐ",  "ए", "ऋ",

        "क्",  "क",  "क्क",  "ख्",   "ख",    "ग्",   "ग",  "घ्",  "घ",    "ङ",
        "चै",   "च्",   "च",   "छ",  "ज्", "ज",   "झ्",  "झ",   "ञ",

        "ट्ट",   "ट्ठ",   "ट",   "ठ",   "ड्ड",   "ड्ढ",  "ड",   "ढ",  "ण्", "ण",
        "त्",  "त",  "थ्", "थ",  "द्ध",  "द", "ध्", "ध",  "न्",  "न",

        "प्",  "प",  "फ्", "फ",  "ब्",  "ब", "भ्",  "भ",  "म्",  "म",
        "य्",  "य",  "र",  "ल्", "ल",  "ळ",  "व्",  "व",
        "श्", "श",  "ष्", "ष",  "स्",   "स",   "ह",

        "ऑ",   "ॉ",  "ो",   "ौ",   "ा",   "ी",   "ु",   "ू",   "ृ",   "े",   "ै",
        "ं",   "ँ",   "ः",   "ॅ",    "ऽ", "्"
    };

    /// <summary>
    /// 對應字體的Unicode
    /// </summary>
    private static string[] arrayUnicode = new string[] {
        "09C60020","0A1D",  "09C8",  "09C9",   "09CA",   "09CB",   "09CC",   "09CD",   "09CE",   "09CF",   "09D0","0A02","0A0D",

        "09AD09A4",

        "09E6",

        "0A0209BD",

        "0A0209B2",

        "0A020988",

        "0A0209C0",

        "0A020998",

        "0A020989",

        "09880A0209A6","0A0209A6","0A0209B309A6",

        "09880A02098F","0A02098F", "0A0209AC",

        "0A0209E8","0A02099C",

        "09880A02099F","0A02099F","0A0209BC",

        "0A00",   "09B30A00",   "09BA098F",    "09D3",   "09FA",

        "0A05",   "0A06",    "0A07",   "0A08",   "09C209EB",  "09EB",   "0A11", "09C3", "09B309C3",  "09E2", "098A","0996",
        "0A120999",   "0A1209A1",    "0A12098D",   "0A120998",   "0A120989", "09C4","09C5",
        "0995",   "09E109A1",   "09E10998",  "09E109E10989",  "09E10999",   "09FF",  "0A04",   "0A0A", "0A0B", "09C209C0", "0981", "0987",
        "09C2",

        "09BB09B309BE",  "099E09B309BE",  "09B309BE",    "09BE",   "09A509AA",  "09AA",  "09B5",  "09EF",  "09BB0984",  "0984",   "09A7",

        "098F",  "09AC",    "0A18",     "09A6",     "09B309A6",    "09A3",   "09C0",  "098C",    "09B3098C",   "09E6",
        "099E09B309B8",  "099B",    "09B8",  "0999",   "099F",    "09BC",   "0A1B",  "098B",   "09DD",

        "0A0E",      "0A0F",      "09A1",  "098D",   "0A10",       "0A13",     "0998",  "0989",  "0985", "09B30985",
        "099D",  "09BA",   "0991", "09B30991",  "0983",    "09B6", "0986",  "09B30986",  "09A0", "09BD",

        "0994",  "09B1",   "09E8", "099C",   "098E",  "09AB",  "0993",  "09B30993", "0990",   "09AD",
        "09EA",   "0988",    "09B2",  "09A4",   "09C1",  "0992",  "099A",  "09B7",
        "0982", "09B30982",  "0980", "09B30980", "0997",   "09B4",   "09AF",

        "09C709BE",    "09C7",    "09BB09B3",   "099E09B3",   "09B3",     "09B0",    "09B9",   "09BF",   "09A8",    "09BB",    "099E",
        "09A9",    "09D9",    "003A",     "09A2",   "09E9", "09C6"
    };

    /// <summary>
    /// 將輸入的Unicode轉換為對應字串
    /// </summary>
    public static string UnicodeToString(string unicode)
    {
        string result;
        switch (unicode.Length)
        {
            case 4:
                {
                    byte[] bytes = new byte[2];
                    bytes[1] = Convert.ToByte(unicode.Substring(0, 2), 16);
                    bytes[0] = Convert.ToByte(unicode.Substring(2, 2), 16);
                    result = Encoding.Unicode.GetString(bytes);
                    break;
                }
            case 8:
                {
                    byte[] bytes = new byte[4];
                    bytes[3] = Convert.ToByte(unicode.Substring(0, 2), 16);
                    bytes[2] = Convert.ToByte(unicode.Substring(2, 2), 16);
                    bytes[1] = Convert.ToByte(unicode.Substring(4, 2), 16);
                    bytes[0] = Convert.ToByte(unicode.Substring(6, 2), 16);
                    result = Encoding.Unicode.GetString(bytes);
                    break;
                }
            case 12:
                {
                    byte[] bytes = new byte[6];
                    bytes[5] = Convert.ToByte(unicode.Substring(0, 2), 16);
                    bytes[4] = Convert.ToByte(unicode.Substring(2, 2), 16);
                    bytes[3] = Convert.ToByte(unicode.Substring(4, 2), 16);
                    bytes[2] = Convert.ToByte(unicode.Substring(6, 2), 16);
                    bytes[1] = Convert.ToByte(unicode.Substring(8, 2), 16);
                    bytes[0] = Convert.ToByte(unicode.Substring(10, 2), 16);
                    result = Encoding.Unicode.GetString(bytes);
                    break;
                }
            default:
                {
                    result = "";
                    break;
                }
        }
        return result;
    }

    private static string letterF = "09AE";
    private static string letterZ = "09A5";

    /// <summary>
    /// 將輸入的印度文字串進行轉換
    /// </summary>
    public static string Translate_Hindi(string unicode_substring)
    {
        int array_one_length = arrayHindi.Length;
        string modified_substring = unicode_substring;

        modified_substring = modified_substring.Replace("क़", "क़");
        modified_substring = modified_substring.Replace("ख़", "ख़");
        modified_substring = modified_substring.Replace("ग़", "ग़");
        modified_substring = modified_substring.Replace("ज़", "ज़");
        modified_substring = modified_substring.Replace("ड़", "ड़");
        modified_substring = modified_substring.Replace("ढ़", "ढ़");
        modified_substring = modified_substring.Replace("ऩ", "ऩ");
        modified_substring = modified_substring.Replace("फ़", "फ़");
        modified_substring = modified_substring.Replace("य़", "य़");
        modified_substring = modified_substring.Replace("ऱ", "ऱ"); 
         // code for replacing "ि" (chhotee ee kii maatraa) with "f"  and correcting its position too.

         var position_of_f = modified_substring.IndexOf("ि");
        while (position_of_f != -1)  
        {
            var character_left_to_f = modified_substring[position_of_f - 1];
            var s = UnicodeToString(letterF) + character_left_to_f;
            modified_substring = modified_substring.Substring(0, position_of_f - 1) + s + modified_substring.Substring(position_of_f + 1);

            position_of_f = position_of_f - 1;

            while ((position_of_f != 0) && (modified_substring[position_of_f - 1] == '्'))
            {
                var string_to_be_Replaced = modified_substring[position_of_f - 2] + "्";
                modified_substring = modified_substring.Replace(string_to_be_Replaced + UnicodeToString(letterF), UnicodeToString(letterF) + string_to_be_Replaced);

                position_of_f = position_of_f - 2;
            }
            position_of_f = modified_substring.IndexOf("ि", position_of_f + 1); // search for f ahead of the current position.

        } 

        //************************************************************     
        //     modified_substring = modified_substring.Replace( /fर्", "£"  )  ;
        //************************************************************     
        // Eliminating "र्" and putting  Z  at proper position for this.

        string set_of_matras = "ािीुूृेैोौं:ँॅ";
        modified_substring += "  ";

        var position_of_half_R = modified_substring.IndexOf("र्");
        while (position_of_half_R > 0)  // while-04
        {
            // "र्"  is two bytes long
            var probable_position_of_Z = position_of_half_R + 2;

            var character_right_to_probable_position_of_Z = modified_substring[probable_position_of_Z + 1];

            // trying to find non-maatra position right to probable_position_of_Z .

            while (set_of_matras.IndexOf(character_right_to_probable_position_of_Z) != -1)
            {
                probable_position_of_Z = probable_position_of_Z + 1;
                character_right_to_probable_position_of_Z = modified_substring[probable_position_of_Z + 1];
            } // end of while-05

            var string_to_be_Replaced = modified_substring.Substring(position_of_half_R + 2, (probable_position_of_Z - position_of_half_R - 1));
            modified_substring = modified_substring.Replace("र्" + string_to_be_Replaced, string_to_be_Replaced + UnicodeToString(letterZ));
            position_of_half_R = modified_substring.IndexOf("र्");
        } // end of while-04

        modified_substring = modified_substring.Substring(0, modified_substring.Length - 2);

        for (int input_symbol_idx = 0; input_symbol_idx < array_one_length; input_symbol_idx++)
        {
            int idx = 0;  // index of the symbol being searched for Replacement

            int a = 0;
            while (idx != -1) //whie-00
            {
                string r = UnicodeToString(arrayUnicode[input_symbol_idx]);
                modified_substring = modified_substring.Replace(arrayHindi[input_symbol_idx], r);
                idx = modified_substring.IndexOf(arrayHindi[input_symbol_idx]);
                if (a > 50000)
                {
                    break;
                }
                a += 1;
            }
        } 
        return modified_substring;
    }

    public static string ReplaceFirstOccurrence(string Source, string Find, string Replace)
    {
        int Place = Source.IndexOf(Find);
        string result = Source.Remove(Place, Find.Length).Insert(Place, Replace);
        return result;
    }
}
