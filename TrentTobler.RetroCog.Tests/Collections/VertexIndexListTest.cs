using NUnit.Framework;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace TrentTobler.RetroCog.Collections;

public class VertexIndexListTest
{
    [Test]
    public void DefaultConstructor_Should_HaveExpectedPropertiesAndValues()
    {
        var instance = new VertexIndexList();
        Assert.AreEqual(DrawElementsType.UnsignedByte, instance.ElementType, "ElementType");
        Assert.AreEqual(0, instance.Count, "Count");
        CollectionAssert.AreEqual(Enumerable.Empty<uint>(), instance);

        IList<uint> collection = instance;
        Assert.AreEqual(false, collection.IsReadOnly, "IsReadOnly");
    }

    [TestCase(101, 1000)]
    public void CollectionConstructor_Should_HaveExpectedPropertiesAndValues(int seed, int samples)
    {
        var rand = new Random(seed);
        for (var sample = 0; sample < samples; ++sample)
        {
            var array = CreateSampleArray(rand);
            var instance = new VertexIndexList(array);
            AssertContents(array, instance);


            IList<uint> collection = instance;
            Assert.AreEqual(false, collection.IsReadOnly, "IsReadOnly");

            IEnumerable enumerable = instance;
            CollectionAssert.AreEqual(enumerable.Cast<uint>(), collection.Cast<uint>());
        }
    }

    [TestCase(101, 100)]
    public void Should_HaveListBehaviors(int seed, int samples)
    {
        var rand = new Random(seed);
        for (var sample = 0; sample < samples; ++sample)
        {
            var list = new List<uint>();
            var instance = new VertexIndexList();

            var mutators = new (string, Action<IList<uint>>)[]
            {
                ("add 0", x => x.Add(0)),
                ("add 256", x => x.Add(256)),
                ("add 65536", x => x.Add(65536)),
                ("insert Count at Count/2", x => x.Insert(x.Count / 2, (uint)x.Count)),
                ("add 1..100", x =>
                {
                    for(var i = 0; i < 100; ++i)
                        x.Add((uint)i);
                }),
                ("insert 1..100", x =>
                {
                    for(var i = 0; i < 100; ++i)
                        x.Insert(i, (uint)i);
                }),
                ("insert at end", x => x.Insert(x.Count, (uint)x.Count)),
                ("removeAt Count/3 or Add 99", x =>
                {
                    if (x.Count > 0)
                        x.RemoveAt(x.Count / 3);
                    else
                        x.Add(99);
                }),
                ("this[Count/5] = Count", x =>
                {
                    if (x.Count > 0)
                        x[x.Count/5] = (uint)x.Count;
                }),
                ("clear", x => x.Clear()),
            };

            var queries = new (string, Func<IList<uint>, object>)[]
            {
                ("Count", x => x.Count),
                ("Remove 0", x => x.Remove(0)),
                ("this[Count/4] or MaxValue", x => x.Count > 0 ? x[x.Count / 4] : uint.MaxValue),
                ("join", x => string.Join(",", x)),
                ("Contains 0", x => x.Contains(0)),
                ("Contains 256", x => x.Contains(256)),
                ("Contains 65536", x => x.Contains(65536)),
                ("IndexOf 0", x => x.IndexOf(0)),
                ("IndexOf 256", x => x.IndexOf(256)),
                ("IndexOf 65536", x => x.IndexOf(65536)),
                ("Remove 0", x => x.Remove(0)),
                ("Remove 256", x => x.Remove(256)),
                ("Remove 65536", x => x.Remove(65536)),
                ("last item", x => x.Count > 0 ? x[x.Count-1] : 9999),
            };

            var ops = new List<string>();
            try
            {
                for (var op = 0; op < 20; ++op)
                {
                    var (mutatorName, mutator) = mutators[rand.Next(mutators.Length)];
                    ops.Add(mutatorName);
                    mutator(list);
                    mutator(instance);
                    Assert.AreEqual(string.Join(",", list), string.Join(",", instance), mutatorName);

                    var (queryName, query) = queries[rand.Next(queries.Length)];
                    ops.Add(queryName);
                    var gotList = query(list);
                    var gotInstance = query(instance);
                    Assert.AreEqual(gotList, gotInstance, $"query: {query}", queryName);
                }
                Assert.AreEqual(string.Join(",", list), string.Join(",", instance));

                var copy = new uint[11 + instance.Count];
                instance.CopyTo(copy, 5);
                CollectionAssert.AreEqual(copy, Enumerable.Repeat(0U, 5).Concat(list).Concat(Enumerable.Repeat(0U, 6)));
            }
            catch (Exception)
            {
                Console.WriteLine($"Failed with ops: {string.Join(", ", ops)}");
                throw;
            }
        }
    }

    [TestCase()]
    [TestCase(1U)]
    [TestCase(1U, 256U)]
    [TestCase(1U, 256U, 65536U)]
    [TestCase(256U)]
    [TestCase(65536U)]
    public void Construction_Should_EqualOriginalSequence(params uint[] want)
        => CollectionAssert.AreEqual(want, new VertexIndexList(want));

    [TestCase("31,1; 32,256; 64, 65536")]
    public void Add_Should_HandleVariations(string additions)
    {
        var list = new List<uint>();
        foreach (var addition in additions.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var spec = addition.Split(',').Select(x => uint.Parse(x, CultureInfo.InvariantCulture)).ToArray();
            var (cnt, val) = (spec[0], spec[1]);
            var data = Enumerable.Repeat(val, (int)cnt);
            list.AddRange(Enumerable.Repeat(val, (int)cnt));
        }
        var instance = new VertexIndexList();
        foreach (var val in list)
            instance.Add(val);
        CollectionAssert.AreEqual(list, instance);
    }

    [TestCase(101, 1000)]
    public void AddRange_Should_HaveExpectedPropertiesAndValues(
        int seed,
        int samples)
    {
        var rand = new Random(seed);
        for (var sample = 0; sample < samples; ++sample)
        {
            var initArray = CreateSampleArray(rand);
            var addArray = CreateSampleArray(rand);

            var want = initArray.ToList();
            want.AddRange(addArray);

            var instance = new VertexIndexList(initArray);
            instance.AddRange(addArray);

            AssertContents(want, instance);
        }
    }

    [TestCase(101, 1000)]
    public void WithArray_Should_QueryCorrectMethod(
        int seed, int samples)
    {
        var rand = new Random(seed);
        for (var sample = 0; sample < samples; ++sample)
        {
            var array = CreateSampleArray(rand);
            var instance = new VertexIndexList(array);
            var content = string.Join(",", array);

            int callCount = 0;
            (string, int, int) Queried<T>(Span<T> data)
                => ($"{typeof(T)}: {string.Join(",", data.ToArray())}", data.Length, ++callCount);

            var got = string.Join(" | ",
                instance.TryByteSpan(out var byteSpan) ? Queried(byteSpan) : "not byte",
                instance.TryShortSpan(out var shortSpan) ? Queried(shortSpan) : "not ushort",
                instance.TryIntSpan(out var intSpan) ? Queried(intSpan) : "not uint");

            Assert.AreEqual(
                instance.ElementType switch
                {
                    DrawElementsType.UnsignedByte => $"({typeof(byte)}: {content}, {array.Length}, 1) | not ushort | not uint",
                    DrawElementsType.UnsignedShort => $"not byte | ({typeof(ushort)}: {content}, {array.Length}, 1) | not uint",
                    DrawElementsType.UnsignedInt => $"not byte | not ushort | ({typeof(uint)}: {content}, {array.Length}, 1)",
                    _ => throw new InvalidOperationException(),
                },
                got,
                 $"[{sample}] {content}: argument data");
        }
    }

    [TestCase()]
    [TestCase(1U)]
    [TestCase(1U, 256U)]
    [TestCase(1U, 256U, 65536U)]
    [TestCase(256U)]
    [TestCase(65536U)]
    public void Indexer_Should_ThrowIndexOutOfRange(params uint[] items)
    {
        var instance = new VertexIndexList(items);

        // Getters
        Assert.Throws<IndexOutOfRangeException>(() => _ = instance[-1]);
        Assert.Throws<IndexOutOfRangeException>(() => _ = instance[instance.Count]);

        // Setters
        Assert.Throws<IndexOutOfRangeException>(() => instance[-1] = 0);
        Assert.Throws<IndexOutOfRangeException>(() => instance[instance.Count] = 0);
    }

    [TestCase(-1, 0)]
    [TestCase(-1, 10)]
    [TestCase(2, 0)]
    [TestCase(11, 10)]
    public void Insert_Should_ThrowIndexOutOfRange(int index, int count)
    {
        var instance = new VertexIndexList(Enumerable.Range(0, count).Select(x => (uint)x));
        Assert.Throws<IndexOutOfRangeException>(() => instance.Insert(index, 999));
    }

    [TestCase(-1, 0)]
    [TestCase(-1, 10)]
    [TestCase(0, 0)]
    [TestCase(10, 10)]
    public void RemoveAt_Should_ThrowIndexOutOfRange(int index, int count)
    {
        var instance = new VertexIndexList(Enumerable.Range(0, count).Select(x => (uint)x));
        Assert.Throws<IndexOutOfRangeException>(() => instance.RemoveAt(index));
    }

    [TestCase(101, 20)]
    public void InvalidState_Should_ThrowInvalidOperationException(int seed, int samples)
    {
        var rand = new Random(seed);
        for (var sample = 0; sample < samples; ++sample)
        {
            var array = CreateSampleArray(rand);
            var instance = new VertexIndexList(array);

            ForceInvalidState(instance);

            Assert.Throws<InvalidOperationException>(() => instance.Insert(0, 0), "Insert");

            Assert.Throws<InvalidOperationException>(() => instance.IndexOf(0), "IndexOf");

            Assert.Throws<InvalidOperationException>(() => _ = instance[0], "this[]");
        }
    }

    private static void ForceInvalidState(VertexIndexList instance)
    {
        // Have to use reflection in order to set up a bad state.
        instance.GetType().GetProperty(nameof(instance.ElementType))!.SetValue(instance, (DrawElementsType)(-1));
    }

    private static uint[] CreateSampleArray(Random rand)
    {
        var count = rand.Next(10);

        var limit = new[] { 256, 65536, int.MaxValue }[rand.Next(3)];
        var array = new uint[count];
        for (var i = 0; i < count; ++i)
            array[i] = (uint)rand.Next(limit);
        return array;
    }

    private static void AssertContents(IReadOnlyList<uint> array, VertexIndexList instance)
    {
        var content = string.Join(",", array);
        Assert.AreEqual(content, string.Join(",", instance), "string.Join");

        Assert.AreEqual(ExpectedTypeProperty(array), instance.ElementType, $"ElementType: {content}");
        Assert.AreEqual(array.Count, instance.Count, $"Count: {content}");

        CollectionAssert.AreEqual(array, instance);
    }

    private static DrawElementsType ExpectedTypeProperty(IEnumerable<uint> array)
    {
        var type = DrawElementsType.UnsignedByte;
        foreach (var item in array)
        {
            if (item >= 65536)
                return DrawElementsType.UnsignedInt;
            if (item >= 256)
                type = DrawElementsType.UnsignedShort;
        }
        return type;
    }
}
