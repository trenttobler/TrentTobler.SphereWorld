using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TrentTobler.RetroCog.Collections
{
    public class SpannableListTest
    {
        [Test]
        public void Should_LookLikeListT()
        {
            var list = new List<string>();
            var array = new SpannableList<string>();

            Mutate("empty", xs => { }, list, array);
            Throws("xs[0] empty", xs => xs[0] = "bye", list, array);
            Query("Count", xs => xs.Count, list, array);
            Query("Contains empty", xs => xs.Contains("one"), list, array);
            Mutate("Add empty", xs => xs.Add("one"), list, array);
            Query("Contains one", xs => xs.Contains("one"), list, array);
            Query("xs[0]", xs => xs[0], list, array);
            Mutate("Insert 0, two", xs => xs.Insert(0, "two"), list, array);
            Query("IndexOf one", xs => xs.IndexOf("one"), list, array);
            Throws("Insert bad start", xs => xs.Insert(-1, "bad start"), list, array);
            Throws("Insert bad end", xs => xs.Insert(5, "bad end"), list, array);
            Throws("RemoveAt bad start", xs => xs.RemoveAt(-1), list, array);
            Throws("RemoveAt bad end", xs => xs.RemoveAt(5), list, array);
            Mutate("Insert 2 three", xs => xs.Insert(2, "three"), list, array);
            Query("ToArray", xs => xs.ToArray(), list, array);
            Query("IsReadOnly", xs => xs.IsReadOnly, list, array);

            CollectionAssert.AreEqual(list.ToArray().AsSpan().ToArray(), array.AsSpan().ToArray(), "AsSpan");
            CollectionAssert.AreEqual(list.ToArray().AsSpan(1).ToArray(), array.AsSpan(1).ToArray(), "AsSpan 1");
            CollectionAssert.AreEqual(list.ToArray().AsSpan(1, 1).ToArray(), array.AsSpan(1, 1).ToArray(), "AsSpan 1 1");

            Mutate("RemoveAt 0", (xs) => xs.RemoveAt(0), list, array);
            Mutate("xs[0] = first", (xs) => xs[0] = "first", list, array);
            Mutate("Remove none", (xs) => xs.Remove("none"), list, array);
            Mutate("Remove three", (xs) => xs.Remove("three"), list, array);
            Mutate("Clear", (xs) => xs.Clear(), list, array);
        }

        [Test]
        public void TestCapacity()
        {
            var array = new SpannableList<int>();

            array.Capacity = 10;
            Assert.AreEqual(10, array.Capacity, "set Capacity");
            array.AddRange(Enumerable.Range(0, 11));
            Assert.AreEqual(20, array.Capacity, "Capacity x2");
            array.Capacity = 20;
            Assert.AreEqual(20, array.Capacity, "Capacity still 20");
            array.EnsureCapacity(10);
            Assert.AreEqual(20, array.Capacity, "Capacity still 20 after ensure 10");
            array.EnsureCapacity(21);
            Assert.AreEqual(40, array.Capacity, "Capacity at 40 after ensure 21");
            array.Capacity = 20;
            Assert.AreEqual(20, array.Capacity, "Capacity at 20 again");
            array.EnsureCapacity(41);
            Assert.AreEqual(41, array.Capacity, "Capacity at 41 after ensure 41");
            CollectionAssert.AreEqual(Enumerable.Range(0, 11), array, "items");

            Assert.Throws<ArgumentOutOfRangeException>(() => array.Capacity = 2, "Capacity = 2");
        }

        [TestCase("", 0)]
        [TestCase("a,b,c", 3)]
        public void Should_Construct_Array(string text, int wantCount)
        {
            var array = new SpannableList<string>(text.Split(',', StringSplitOptions.RemoveEmptyEntries));
            Assert.AreEqual(text, string.Join(",", array));
            Assert.AreEqual(wantCount, array.Count, "Count");
            Assert.AreEqual(array.Count, array.Capacity, "Capacity");
        }

        [TestCase("", 0, 0)]
        [TestCase("1", 16, 16)]
        [TestCase("123456789abcdefgh", 17, 32)]
        public void AddRange_Should_UseCollectionCount(string text, int wantCollectionCapacity, int wantUnknownCapacity)
        {
            var array = new SpannableList<char>();
            array.AddRange(text.ToArray());
            Assert.AreEqual(wantCollectionCapacity, array.Capacity, "collection capacity");

            var unknown = new SpannableList<char>();
            unknown.AddRange(text.Where(c => c != 0));
            Assert.AreEqual(wantUnknownCapacity, unknown.Capacity, "Unknown capacity");
        }

        private static void Mutate<T>(string name, Action<IList<T>> mutate, List<T> list, SpannableList<T> array)
        {
            mutate(list);
            mutate(array);
            CollectionAssert.AreEqual(list, array, name);
        }

        private static void Query<TIn, TOut>(string name, Func<IList<TIn>, TOut> query, List<TIn> list, SpannableList<TIn> array)
        {
            var listValue = query(list);
            var arrayValue = query(array);
            Assert.AreEqual(listValue, arrayValue, name);
        }

        private static void Throws<T>(string name, Action<IList<T>> thrower, List<T> list, SpannableList<T> array)
        {
            Exception? Catcher(IList<T> xs)
            {
                try
                {
                    thrower(xs);
                }
                catch (Exception err)
                {
                    return err;
                }
                Assert.Fail("did not throw");
                return null;
            }

            // Exception type doesn't need to match strictly, but in general, it will.
            var listErr = Catcher(list);
            var arrayErr = Catcher(array);
            Assert.AreEqual(listErr != null, arrayErr != null, name);
        }
    }
}
