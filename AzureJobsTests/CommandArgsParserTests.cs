using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using AzureJobs.Common;

namespace AzureJobsTests {
    [TestClass]
    public class CommandArgsParserTests {
        [TestClass]
        public class TheParseArgsMethod {
            [TestMethod]
            public void ReturnsTheValues() {
                string[] args = new string[8];
                args[0] = "--logFile";
                args[1] = "one.txt";
                args[2] = "--name";
                args[3] = "Azure";
                args[4] = "Image";
                args[5] = "Optimizer";
                args[6] = "--color";
                args[7] = "Green";

                IDictionary<string, string> result = new CommandArgsParser().ParseArgs(args);
                Assert.AreEqual(3, result.Count);
                Assert.IsTrue(result.Keys.Contains("--logFile"));
                Assert.IsTrue(result.Keys.Contains("--name"));
                Assert.IsTrue(result.Keys.Contains("--color"));
                Assert.AreEqual("one.txt", result["--logFile"]);
                Assert.AreEqual("Azure Image Optimizer", result["--name"]);
                Assert.AreEqual("Green", result["--color"]);
            }

            [TestMethod]
            public void HandlesHelpAsShortName() {
                string[] args = new string[1];
                args[0] = "/?";

                IDictionary<string, string> result = new CommandArgsParser().ParseArgs(args);
                Assert.IsTrue(result.Keys.Contains("--help"));
                Assert.AreEqual(true.ToString(), result["--help"]);
                string foo = "bar";
            }
        }
    }
}
