using System;
using System.Text;

namespace com.hideakin.textsearch.input
{
    internal static class Keyboard
    {
        public static string GetPassword()
        {
            var sb = new StringBuilder();
            while (true)
            {
                var input = Console.ReadKey(true);
                if (input.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                else if (input.Key == ConsoleKey.Backspace)
                {
                    if (sb.Length > 0)
                    {
                        sb.Length = sb.Length - 1;
                        Console.Write("\b \b");
                    }
                }
                else if (0x20 <= input.KeyChar && input.KeyChar <= 0x7E)
                {
                    sb.Append(input.KeyChar);
                    Console.Write("*");
                }
            }
            return sb.ToString();
        }
    }
}
