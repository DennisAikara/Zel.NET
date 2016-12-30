// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zel.Helpers;

namespace Zel.Tests.Helpers
{
    [TestClass]
    public class MiscHelperTests
    {
        #region GetSequentialInteger Tests

        [TestMethod]
        public void GetSequentialInteger_Increments()
        {
            var seq1 = MiscHelper.GetSequentialInteger();
            var seq2 = MiscHelper.GetSequentialInteger();
            Assert.AreEqual(1, seq2 - seq1);
        }

        #endregion

        #region GetSequentialLongTests

        [TestMethod]
        public void GetSequentialLong_Increments()
        {
            var seq1 = MiscHelper.GetSequentialLong();
            var seq2 = MiscHelper.GetSequentialLong();
            Assert.AreEqual(1, seq2 - seq1);
        }

        #endregion

        #region GetSequentialGuid Tests

        [TestMethod]
        public void GetSequentialGuid_Returns_Sequential_Guids()
        {
            var guid1 = MiscHelper.GetSequentialGuid().ToString();
            var guid2 = MiscHelper.GetSequentialGuid().ToString();

            Assert.AreNotEqual(guid1, guid2);
        }

        #endregion

        #region CopyStream Tests

        [TestMethod]
        public void CopyStream_Copies_Stream()
        {
            using (var sourceStream = new MemoryStream(Encoding.ASCII.GetBytes("fkjslf")))
            {
                using (var targetStream = new MemoryStream())
                {
                    MiscHelper.CopyStream(sourceStream, targetStream);

                    Assert.IsTrue(targetStream.ToArray().SequenceEqual(targetStream.ToArray()));
                }
            }
        }

        #endregion

        #region ToRelativeDate

        [TestMethod]
        public void ToRelativeDate_Measures_Seconds()
        {
            const string relativeDate = "5 second(s) ago";
            var time = DateTime.Now.AddSeconds(-5);
            Assert.AreEqual(relativeDate, MiscHelper.ToRelativeDate(time));
        }

        [TestMethod]
        public void ToRelativeDate_Measures_Minutes()
        {
            const string relativeDate1 = "about 5 minutes ago";
            const string relativeDate2 = "about a minute ago";
            var time = DateTime.Now.AddSeconds(-65);
            Assert.AreEqual(relativeDate2, MiscHelper.ToRelativeDate(time));
            time = DateTime.Now.AddMinutes(-5);
            Assert.AreEqual(relativeDate1, MiscHelper.ToRelativeDate(time));
        }

        [TestMethod]
        public void ToRelativeDate_Measures_Hours()
        {
            const string relativeDate1 = "about 5 hours ago";
            const string relativeDate2 = "about an hour ago";
            var time = DateTime.Now.AddMinutes(-65);
            Assert.AreEqual(relativeDate2, MiscHelper.ToRelativeDate(time));
            time = DateTime.Now.AddHours(-5);
            Assert.AreEqual(relativeDate1, MiscHelper.ToRelativeDate(time));
        }

        [TestMethod]
        public void ToRelativeDate_Measures_Days()
        {
            const string relativeDate1 = "about 5 days ago";
            const string relativeDate2 = "about a day ago";
            var time = DateTime.Now.AddHours(-26);
            Assert.AreEqual(relativeDate2, MiscHelper.ToRelativeDate(time));
            time = DateTime.Now.AddDays(-5);
            Assert.AreEqual(relativeDate1, MiscHelper.ToRelativeDate(time));
        }

        [TestMethod]
        public void ToRelativeDate_Measures_Months()
        {
            const string relativeDate1 = "about 5 month(s) ago";
            const string relativeDate2 = "about a month ago";
            var time = DateTime.Now.AddDays(-30);
            Assert.AreEqual(relativeDate2, MiscHelper.ToRelativeDate(time));
            time = DateTime.Now.AddMonths(-5);
            Assert.AreEqual(relativeDate1, MiscHelper.ToRelativeDate(time));
        }

        [TestMethod]
        public void ToRelativeDate_Measures_Years()
        {
            const string relativeDate1 = "about 5 year(s) ago";
            const string relativeDate2 = "about a year ago";
            var time = DateTime.Now.AddDays(-365);
            Assert.AreEqual(relativeDate2, MiscHelper.ToRelativeDate(time));
            time = DateTime.Now.AddYears(-5);
            Assert.AreEqual(relativeDate1, MiscHelper.ToRelativeDate(time));
        }

        #endregion

        #region UrlIsListening Tests

        [TestMethod]
        public void UrlIsListening_Returns_True_When_Url_Is_Listening()
        {
            var urlIsListening = MiscHelper.UrlIsListening(new Uri("http://www.google.com"));
            Assert.IsTrue(urlIsListening);
        }

        [TestMethod]
        public void UrlIsListening_Returns_True_When_Url_Is_Not_Listening()
        {
            var urlIsListening = MiscHelper.UrlIsListening(new Uri("http://www.3fa35aga3.com"));
            Assert.IsFalse(urlIsListening);
        }

        #endregion
    }
}