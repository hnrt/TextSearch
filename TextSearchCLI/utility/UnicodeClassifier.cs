using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.hideakin.textsearch.utility
{
    internal static class UnicodeClassifier
    {
        public static bool IsHiragana(int c)
        {
            return 0x3040 <= c && c <= 0x309F;
        }

        public static bool IsKatakana(int c)
        {
            return 0x30A0 <= c && c <= 0x30FF;
        }

        public static bool IsKatakanaPhoneticExtensions(int c)
        {
            return 0x31F0 <= c && c <= 0x31FF;
        }

        public static bool IsIdeograph(int c)
        {
            return 0x4E00 <= c && c <= 0x9FFC;
        }

        public static bool IsHalfwidthKatakana(int c)
        {
            return 0xFF66 <= c && c <= 0xFF9F;
        }

        public static bool IsJapaneseLetter(int c)
        {
            return IsHiragana(c) || IsKatakana(c) || IsKatakanaPhoneticExtensions(c) || IsIdeograph(c) || IsHalfwidthKatakana(c);
        }

        public static bool IsFullwidthDigit(int c)
        {
            return 0xFF10 <= c && c <= 0xFF19;
        }

        public static bool IsFullwidthUppercaseAlphabet(int c)
        {
            return 0xFF21 <= c && c <= 0xFF3A;
        }

        public const int FULLWIDTH_COMMERCIAL_AT = 0xFF20;

        public static bool IsFullwidthLowercaseAlphabet(int c)
        {
            return 0xFF41 <= c && c <= 0xFF5A;
        }

        public static bool IsFullwidthAlphabet(int c)
        {
            return IsFullwidthUppercaseAlphabet(c) || IsFullwidthLowercaseAlphabet(c);
        }

        public static bool IsFullwidthAlphaNumeric(int c)
        {
            return IsFullwidthAlphabet(c) || IsFullwidthDigit(c);
        }

    }
}
