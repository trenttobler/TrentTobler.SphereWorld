using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TrentTobler.RetroCog;

public class RetroCogExtensionsTest
{
    [TestCase("", "")]
    [TestCase(" this   line has  no  comment ", " this   line has  no  comment ")]
    [TestCase("# remove the entire line", "")]
    [TestCase("remove half # of the line", "remove half ")]
    public void TestTrimHashComment(string sample, string want)
        => Assert.AreEqual(want, sample.TrimHashComment());

    public static IEnumerable<TestCaseData> LineEnumerableSamples => new[]
    {
        new TestCaseData(
            "",
            new string[] { }
        ).SetArgDisplayNames("empty file"),

        new TestCaseData(
            "this\nhas\nthree lines\n",
            new string[]{"this", "has", "three lines" }
        ).SetArgDisplayNames("three line lf"),

        new TestCaseData(
            "this\r\nuses\r\ncr and lf and no last eol",
            new string[]{ "this", "uses", "cr and lf and no last eol" }
        ).SetArgDisplayNames("three line crlf")
    };

    [TestCaseSource(nameof(LineEnumerableSamples))]
    public void TestToLines(string text, string[] want)
        => CollectionAssert.AreEqual(want, new StringReader(text).ToLines());

    public static IEnumerable<TestCaseData> CombineBackslashLinesSamples => new[]
    {
        new TestCaseData(Enumerable.Empty<string>(), Enumerable.Empty<string>()).SetArgDisplayNames("empty list"),
        new TestCaseData(new[]{ "one", "two", "three"}, new[]{ "one", "two", "three"}).SetArgDisplayNames("no escapes multiline"),
        new TestCaseData(new[]{ "one\\", "two\\ ", "three \\", "four"}, new[]{"onetwo\\ ", "three four" }).SetArgDisplayNames("multiline escapes"),
        new TestCaseData(new[]{ "one\\", "two\\ ", "three \\"}, new[]{"onetwo\\ ", "three " }).SetArgDisplayNames("last line with backslash"),
    };
    [TestCaseSource(nameof(CombineBackslashLinesSamples))]
    public void TestCombineBackslashLines(IEnumerable<string> data, IEnumerable<string> want)
        => CollectionAssert.AreEqual(want, data.CombineBackslashedLines());

    public static IEnumerable<TestCaseData> SplitWordsSamples => new[]
    {
        new TestCaseData("", new string[]{ }),
        new TestCaseData("   ", new string[] { }),
        new TestCaseData(" one  + two  ", new string[] {"one", "+", "two"}),
        new TestCaseData("first-word second-word", new string[] {"first-word", "second-word"}),
    };

    [TestCaseSource(nameof(SplitWordsSamples))]
    public void TestToWords(string line, string[] want)
        => CollectionAssert.AreEqual(want, line.SplitWords());
}
