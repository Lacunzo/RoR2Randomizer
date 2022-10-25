using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Utility
{
    public static class StringUtils
    {
        static readonly StringBuilder _stringBuilder = new StringBuilder();

        public static string Repeat(this string str, uint count)
        {
            _stringBuilder.Clear();

            for (uint i = 0; i < count; i++)
            {
                _stringBuilder.Append(str);
            }

            return _stringBuilder.ToString();
        }
    }
}
