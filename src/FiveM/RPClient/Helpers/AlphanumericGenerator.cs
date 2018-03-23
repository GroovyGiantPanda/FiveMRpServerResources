using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client.Helpers
{
    class AlphanumericGenerator
    {
        internal const string alphanumeric =   "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        internal const string capitalLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        internal const string numbers =                                  "0123456789";
        internal static Random random = new Random();

        /// <summary>
        /// Returns a string of random capital letters
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string CapitalLetters(int length)
        {
            return _generate(capitalLetters, length);
        }

        /// <summary>
        /// Returns a string of random number characters
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Numbers(int length)
        {
            return _generate(numbers, length);
        }

        /// <summary>
        /// Returns an alphanumeric random string of specified length
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Generate(int length)
        {
            return _generate(alphanumeric, length);
        }

        /// <summary>
        /// Returns a string of random characters based on specified source string
        /// </summary>
        /// <param name="source"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        internal static string _generate(string source, int length)
        {
            return new string(Enumerable.Repeat(source, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
