using System;
using System.Text.RegularExpressions;

namespace K2D2
{
    public static class GeneralTools
    {
        /// <summary>
        /// Converts a string to a double, if the string contains a number. Else returns -1
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static double GetNumberString(string str)
        {
            string number = Regex.Replace(str, "[^0-9.]", "");

            return number.Length > 0 ? double.Parse(number) : -1;

        }
        
    }
}