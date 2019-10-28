using System;
using System.Text;

namespace PostBoy.Helpers
{
    public static class Converter
    {
        public static string FromUrlEncode(string input)
        {
            return Uri.UnescapeDataString(input.Replace('+', '\x20'));
        }

        public static string ToUrlEncode(string input)
        {
            return Uri.EscapeUriString(input);
        }

        public static string FromBase64(string input, string charset = "utf-8")
        {
            return Encoding.GetEncoding(charset).GetString(Convert.FromBase64String(input));
        }

        public static string ToBase64(string input, string charset = "utf-8")
        {
            return Convert.ToBase64String(Encoding.GetEncoding(charset).GetBytes(input));
        }
    }
}
