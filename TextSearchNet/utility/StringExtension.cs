using System;

namespace com.hideakin.textsearch.utility
{
    public static class StringExtension
    {
        /// <summary>
        /// Examines the string if it has a lower-case drive letter. If it does, this method will replace it with the corresponding upper-case letter.
        /// </summary>
        /// <returns>A string that has been processed or the original string if it is not the case.</returns>
        public static string NormalizePath(this string src)
        {
            if (src.Length >= 2 && Char.IsLower(src[0]) && src[1] == ':')
            {
                return Char.ToUpper(src[0]).ToString() + src.Substring(1);
            }
            return src;
        }
    }
}
