using AzureJobs.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureJobsTests {
    [TestClass]
    public class CsvHelperTests {
        [TestClass]
        public class TheCleanMessageForCsvMethod {

            [TestMethod]
            public void DoesNotModifyNormalCharatersTest1() {
                string str = "no escaping here";
                string escapedStr = new CsvHelper().Escape(str);

                Assert.AreEqual(str, escapedStr);
            }
            [TestMethod]
            public void EscapesNewLineCharacters1() {
                string str =
@"foo bar
New line";
                string escapedStr = new CsvHelper().Escape(str);

                string expetedStr = "\"foo bar\r\nNew line\"";

                Assert.AreEqual(expetedStr, escapedStr);
            }

            [TestMethod]
            public void EscapesNewLineCharaters2() {
                string str = "foo bar\n new line";
                string escapedStr = new CsvHelper().Escape(str);
                string expectedStr = "\"foo bar\n new line\"";
                Assert.AreEqual(expectedStr, escapedStr);
            }

            [TestMethod]
            public void EscapesNewLineCharaters3() {
                string str = "foo bar\r new line";
                string escapedStr = new CsvHelper().Escape(str);
                string expectedStr = "\"foo bar\r new line\"";
                Assert.AreEqual(expectedStr, escapedStr);
            }

            [TestMethod]
            public void EscapesComma() {
                string str = "foo bar, comma";
                string escapedStr = new CsvHelper().Escape(str);
                string expectedStr = "\"foo bar, comma\"";
                Assert.AreEqual(expectedStr, escapedStr);
            }
            [TestMethod]
            public void EscapesQuotes() {
                string str = "foo bar\"double";
                string escapedStr = new CsvHelper().Escape(str);
                string expectedStr = "\"foo bar\"\"double\"";
                Assert.AreEqual(expectedStr,escapedStr);
            }
        }
    }
}
