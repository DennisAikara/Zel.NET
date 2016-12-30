// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zel.Helpers;

namespace Zel.Tests.Helpers
{
    [TestClass]
    public class StringHelperTests
    {
        #region SplitPascalCase Tests

        [TestMethod]
        public void SplitPascalCase_Splits_String_By_Pascal_Case()
        {
            const string pascalCaseWord = "HelloWorld!Darkhorse2";
            var split = StringHelper.SplitPascalCase(pascalCaseWord);

            Assert.AreEqual(3, split.Length);
            Assert.AreEqual("Hello", split[0]);
            Assert.AreEqual("World!", split[1]);
            Assert.AreEqual("Darkhorse2", split[2]);
        }

        #endregion

        #region RandomString Tests

        [TestMethod]
        public void RandomString_Creates_Random_String_Of_Specified_Size()
        {
            var randomString = StringHelper.RandomString(20);
            Assert.AreEqual(20, randomString.Length);
        }

        #endregion

        #region RemoveNonValidCharacters Tests

        [TestMethod]
        public void RemoveNonValidCharacters_Removes_Invalid_Characters_From_Specified_string()
        {
            const string str = "jkj*34kL#-32:!@%#_f";
            var validCharacters = new[] {'j', 'k', '3', '4', 'L', '-', '2', 'f'};

            var validString = StringHelper.RemoveNonValidCharacters(str, validCharacters);

            Assert.AreEqual("jkj34kL-32f", validString);
        }

        #endregion

        #region RemoveDiacritics Tests

        [TestMethod]
        public void RemoveDiacritics_Removes_Accent_Characters_From_Specified_String()
        {
            const string str = "Jk_k34#_7 23qrf^$#(%!@_ÀÇÈÍÑÒÙÚÛå";
            Assert.AreEqual("Jk_k34#_7 23qrf^$#(%!@_ACEINOUUUa", StringHelper.RemoveDiacritics(str));
        }

        #endregion

        #region TitleCaseString Tests

        [TestMethod]
        public void TitleCaseString_Converts_String_To_TitleCase()
        {
            const string str = "dog my MAN a857t t853F isKooL";

            Assert.AreEqual("Dog My Man A857t T853f Iskool", StringHelper.TitleCaseString(str));
        }

        #endregion

        #region HtmlDecodeString Tests

        [TestMethod]
        public void HtmlDecodeString_HtmlDecodes_Specified_String()
        {
            const string encodedString = "kjfs&lt;dog&gt;kjs&lt;/kjd&gt;";
            Assert.AreEqual("kjfs<dog>kjs</kjd>", StringHelper.HtmlDecodeString(encodedString));
        }

        #endregion

        #region HtmlEncodeString Tests

        [TestMethod]
        public void HtmlEncodeString_HtmlEncodes_Specified_String()
        {
            const string str = "kjfs<dog>kjs</kjd>";
            Assert.AreEqual("kjfs&lt;dog&gt;kjs&lt;/kjd&gt;", StringHelper.HtmlEncodeString(str));
        }

        #endregion

        #region UrlEncodeString Tests

        [TestMethod]
        public void UrlEncodeString_UrlEncodes_Specified_String()
        {
            const string str = "kjf*@#/3jf";
            Assert.AreEqual("kjf*%40%23%2f3jf", StringHelper.UrlEncodeString(str));
        }

        #endregion

        #region UrlDecodeString Tests

        [TestMethod]
        public void UrlDecodeString_UrlDecodes_Specified_String()
        {
            const string str = "kjf*%40%23%2f3jf";
            Assert.AreEqual("kjf*@#/3jf", StringHelper.UrlDecodeString(str));
        }

        #endregion

        #region ReplaceTrailingString Tests

        [TestMethod]
        public void ReplaceTrailingString_ReplacesTrailingString()
        {
            var str = "ToToyYyy";

            Assert.AreEqual("ToToyY", StringHelper.ReplaceTrailingString(str, "y", "", true, true),
                "Using recursion case sensitive");

            Assert.AreEqual("ToTo", StringHelper.ReplaceTrailingString(str, "y", "", false, true),
                "Using recursion case in-sensitive");

            Assert.AreEqual("ToToyYyy", StringHelper.ReplaceTrailingString(str, "Y", "", true),
                "Using non-recursion case sensitive");

            Assert.AreEqual("ToToyYy", StringHelper.ReplaceTrailingString(str, "Y", ""),
                "Using non-recursion case in-sensitive");

            Assert.AreEqual("ToToyYyy", StringHelper.ReplaceTrailingString(str, "y", "y", false, true),
                "Using recursion case in-sensitive same as trail");

            Assert.AreEqual("ToToyYyY", StringHelper.ReplaceTrailingString(str, "y", "Y", true, true),
                "Using recursion case sensitive same as trail");

            str = "ToToyYyy$";
            Assert.AreEqual("ToToyYyy", StringHelper.ReplaceTrailingString(str, "$", "", true, true),
                "Using recursion regex $");
        }

        #endregion

        #region ReplaceLeadingString Tests

        [TestMethod]
        public void ReplaceLeadingString_ReplacesLeadingStrings()
        {
            var str = "yyYyToTo";

            Assert.AreEqual("YyToTo", StringHelper.ReplaceLeadingString(str, "y", "", true, true),
                "Using recursion case sensitive");

            Assert.AreEqual("ToTo", StringHelper.ReplaceLeadingString(str, "y", "", false, true),
                "Using recursion case in-sensitive");

            Assert.AreEqual("yyYyToTo", StringHelper.ReplaceLeadingString(str, "Y", "", true),
                "Using non-recursion case sensitive");

            Assert.AreEqual("yYyToTo", StringHelper.ReplaceLeadingString(str, "Y", ""),
                "Using non-recursion case in-sensitive");

            Assert.AreEqual("yyYyToTo", StringHelper.ReplaceLeadingString(str, "y", "y", false, true),
                "Using recursion case in-sensitive same as trail");

            Assert.AreEqual("YyYyToTo", StringHelper.ReplaceLeadingString(str, "y", "Y", true, true),
                "Using recursion case sensitive same as trail");

            str = "^yyYyToTo";
            Assert.AreEqual("yyYyToTo", StringHelper.ReplaceLeadingString(str, "^", "", true, true),
                "Using recursion regex ^");

            str = "1 of jack";
            Assert.AreEqual("One of jack", StringHelper.ReplaceLeadingString(str, "1 ", "One ", true, true));
        }

        #endregion

        #region ReplaceWord Tests

        [TestMethod]
        public void ReplaceWord_ReplacesWords()
        {
            const string str = "nIb Black nib nib\\ Teddy Bear NIB";

            Assert.AreEqual("New Black New nib\\ Teddy Bear New", StringHelper.ReplaceWord(str, "NIB", "New"),
                "Case in-sensitive");

            Assert.AreEqual("nIb Black nib nib\\ Teddy Bear New", StringHelper.ReplaceWord(str, "NIB", "New", true),
                "Case sensitive");
        }

        #endregion

        #region ReplaceSubString Tests

        [TestMethod]
        public void ReplaceSubString_ReplacesSubString()
        {
            var str = "Simple title with double spacesddddd";
            Assert.AreEqual("Simple title with double spacesddd", StringHelper.ReplaceSubString(str, "dd", "d"),
                "Case in-sensitive non-recursive");

            str = "Simple title with double spacesddddd";
            Assert.AreEqual("Simple title with double spacesd",
                StringHelper.ReplaceSubString(str, "dd", "d", false, true),
                "Case sensitive recursive");
        }

        #endregion
    }
}