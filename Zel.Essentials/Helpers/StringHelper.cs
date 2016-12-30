// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Zel.Helpers
{
    public static class StringHelper
    {
        #region Misc Methods

        /// <summary>
        ///     Splits the specified string at each upper case character
        /// </summary>
        /// <param name="pascalCaseString">String to split</param>
        /// <returns>Array of string</returns>
        public static string[] SplitPascalCase(string pascalCaseString)
        {
            var stringBuilder = new StringBuilder();
            var pos = 0;

            foreach (var chr in pascalCaseString)
            {
                if ((pos > 0) && char.IsUpper(chr))
                {
                    stringBuilder.Append(" ");
                }
                stringBuilder.Append(chr);
                pos++;
            }

            return stringBuilder.ToString().Split(' ');
        }

        /// <summary>
        ///     Creates a random string of the specified size
        /// </summary>
        /// <param name="size">String size</param>
        /// <returns>Random string</returns>
        public static string RandomString(int size)
        {
            var stringBuilder = new StringBuilder();
            var random = new Random(MiscHelper.GetSequentialInteger());

            for (var i = 0; i < size; i++)
            {
                var ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26*random.NextDouble() + 65)));
                stringBuilder.Append(ch);
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        ///     Title case the specified string
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="language">Language to use</param>
        /// <returns>Title cased string</returns>
        public static string TitleCaseString(string str, string language = "English")
        {
            var culture = (from c in CultureInfo.GetCultures(CultureTypes.AllCultures)
                where c.EnglishName == language
                select c).FirstOrDefault();

            if (culture != null)
            {
                var textInfo = culture.TextInfo;
                return textInfo.ToTitleCase(str.ToLower());
            }

            return str;
        }

        #endregion

        #region Html & Http Methods

        /// <summary>
        ///     Html decode the specified html encoded string
        /// </summary>
        /// <param name="str">Html encoded string</param>
        /// <returns>Html decoded string</returns>
        public static string HtmlDecodeString(string str)
        {
            return HttpUtility.HtmlDecode(str);
        }

        /// <summary>
        ///     Html encode the specified string
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Html encoded string</returns>
        public static string HtmlEncodeString(string str)
        {
            return HttpUtility.HtmlEncode(str);
        }

        /// <summary>
        ///     Url encode the specified string
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Url encoded string</returns>
        public static string UrlEncodeString(string str)
        {
            return HttpUtility.UrlEncode(str);
        }

        /// <summary>
        ///     Url decode the specified url encoded string
        /// </summary>
        /// <param name="str">Url encoded string</param>
        /// <returns>Url decoed string</returns>
        public static string UrlDecodeString(string str)
        {
            return HttpUtility.UrlDecode(str);
        }

        #endregion

        #region Cleanup Methods

        /// <summary>
        ///     Removes all the characters from the specified string that are not in the specified valid characters
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="validCharacters">Valid characters</param>
        /// <returns></returns>
        public static string RemoveNonValidCharacters(string str, char[] validCharacters)
        {
            var stringBuilder = new StringBuilder();

            foreach (var chr in str)
            {
                if (validCharacters.Contains(chr))
                {
                    stringBuilder.Append(chr);
                }
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        ///     Replaces accent characters in the specified string with regular characters
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>String with accent characters removed</returns>
        public static string RemoveDiacritics(string str)
        {
            var strFormD = str.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var chr in strFormD)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(chr);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(chr);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        #endregion

        #region Repalce Methods

        /// <summary>
        ///     Replace the specified trailing string on the specified string with the new string
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="oldString">Trailing string to replace</param>
        /// <param name="newString">New trailing string</param>
        /// <param name="caseSensitive">Indicate case sensitivity</param>
        /// <param name="useRecursion">
        ///     Indicate if recursion should be used to keep removing matching trailing string after the
        ///     first replace
        /// </param>
        /// <returns>String with replaced trailing string</returns>
        public static string ReplaceTrailingString(string str, string oldString, string newString,
            bool caseSensitive = false,
            bool useRecursion = false)
        {
            return ReplaceSubString(str, string.Format("{0}$", Regex.Escape(oldString)), newString, caseSensitive,
                useRecursion,
                false);
        }

        /// <summary>
        ///     Replace the specified leading string on the specified string with the new string
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="oldString">Leading string to replace</param>
        /// <param name="newString">New leading string</param>
        /// <param name="caseSensitive">Indicate case sensitivity</param>
        /// <param name="useRecursion">
        ///     Indicate if recursion should be used to keep removing matching leading string after the
        ///     first replace
        /// </param>
        /// <returns>String with replaced leading string</returns>
        public static string ReplaceLeadingString(string str, string oldString, string newString,
            bool caseSensitive = false,
            bool useRecursion = false)
        {
            return ReplaceSubString(str, string.Format("^{0}", Regex.Escape(oldString)), newString, caseSensitive,
                useRecursion,
                false);
        }

        /// <summary>
        ///     Replace the specified word in the specified string with the new word
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="oldWord">Word to replace</param>
        /// <param name="newWord">New word</param>
        /// <param name="caseSensitive">Indicate case sensitivity</param>
        /// <param name="useRecursion">Indicate if recursion should be used to keep removing matching word after the first replace</param>
        /// <returns>String with replaced word</returns>
        public static string ReplaceWord(string str, string oldWord, string newWord, bool caseSensitive = false,
            bool useRecursion = false)
        {
            //Words at the middle of the string
            str = ReplaceSubString(str, string.Format(" {0} ", oldWord), string.Format(" {0} ", newWord), caseSensitive,
                useRecursion);

            //Words at the beginning of the string
            str = ReplaceSubString(str, string.Format("^{0} ", Regex.Escape(oldWord)), string.Format("{0} ", newWord),
                caseSensitive, useRecursion, false);

            //Words at the end of the original string
            str = ReplaceSubString(str, string.Format(" {0}$", Regex.Escape(oldWord)), string.Format(" {0}", newWord),
                caseSensitive, useRecursion, false);

            return str;
        }

        /// <summary>
        ///     Replace the specified string in the specified string with the new word
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="oldString">String to replace</param>
        /// <param name="newString">New string</param>
        /// <param name="caseSensitive">Indicate case sensitivity</param>
        /// <param name="useRecursion">
        ///     Indicate if recursion should be used to keep removing matching string after the first
        ///     replace
        /// </param>
        /// <param name="regexEscape">Indicate if the old string should be regular expression escaped</param>
        /// <returns>String with replaced string</returns>
        public static string ReplaceSubString(string str, string oldString, string newString, bool caseSensitive = false,
            bool useRecursion = false, bool regexEscape = true)
        {
            var orginalHash = str.GetHashCode();
            var tmpOldString = regexEscape ? Regex.Escape(oldString) : oldString;

            str = !caseSensitive
                ? Regex.Replace(str, tmpOldString, newString, RegexOptions.IgnoreCase)
                : Regex.Replace(str, tmpOldString, newString);

            if (useRecursion)
            {
                if (orginalHash != str.GetHashCode())
                {
                    //hash is not the same, that means we replaced something, recurse until no more replacement
                    str = ReplaceSubString(str, oldString, newString, caseSensitive, true, regexEscape);
                }
            }

            return str;
        }

        #endregion
    }
}