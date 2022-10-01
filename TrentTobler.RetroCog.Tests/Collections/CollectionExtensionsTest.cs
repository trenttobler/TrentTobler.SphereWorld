using NUnit.Framework;
using System;
using System.Linq;

namespace TrentTobler.RetroCog.Collections
{
    public class CollectionExtensionsTest
    {
        [TestCase("", "one=1", "1", "one=1")]
        [TestCase("one=1", "one=2", "1", "one=1")]
        public void TestGetOrAdd(string before, string arg, string want, string after)
        {
            var instance = before.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(kv => kv.Split('='))
                .ToDictionary(items => items[0], items => items[1]);
            var data = arg.Split('=');

            var got = instance.GetOrAdd(data[0], () => data[1]);

            Assert.AreEqual(want, got);
            CollectionAssert.AreEquivalent(
                instance,
                after.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(kv => kv.Split('='))
                    .ToDictionary(items => items[0], items => items[1]));

        }

        [TestCase("1,2", "2,1")]
        [TestCase("1,1", "1,1")]
        public void TestReverseTuple(string text, string want)
            => Assert.AreEqual(
                ParseStringTuple(want),
                ParseStringTuple(text).Reverse());

        [TestCase("2,1", "1,2")]
        [TestCase("1,2", "1,2")]
        [TestCase("1,1", "1,1")]
        public void TestSortTuple(string tuple, string want)
            => Assert.AreEqual(
                ParseStringTuple(want),
                ParseStringTuple(tuple).Sort());

        [TestCase("a,b", " (a) , (b) ")]
        public void TestSelectTuple(string tuple, string want)
            => Assert.AreEqual(
                ParseStringTuple(want),
                ParseStringTuple(tuple).Select(s => $" ({s}) "));

        [TestCase("", "")]
        [TestCase("a", "(a, a)")]
        [TestCase("a,b", "(a, b) (b, a)")]
        [TestCase("a,b,c", "(a, b) (b, c) (c, a)")]
        [TestCase("a,b,c,d", "(a, b) (b, c) (c, d) (d, a)")]
        public void TestCyclicPairs(string list, string want)
            => Assert.AreEqual(
                want,
                string.Join(" ",
                    list.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .ToCyclicPairs()));

        [TestCase("", "")]
        [TestCase("a", "(a, a, a)")]
        [TestCase("a,b", "(a, b, a) (b, a, b)")]
        [TestCase("a,b,c", "(a, b, c) (b, c, a) (c, a, b)")]
        [TestCase("a,b,c,d", "(a, b, c) (b, c, d) (c, d, a) (d, a, b)")]
        public void TestCyclicTriples(string list, string want)
            => Assert.AreEqual(
                want,
                string.Join(" ",
                    list.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .ToCyclicTriples()));

        private static (string, string) ParseStringTuple(string tuple)
        {
            var args = tuple.Split(',');
            var instance = (args[0], args[1]);
            return instance;
        }
    }
}
