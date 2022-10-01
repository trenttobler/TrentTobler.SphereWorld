using NUnit.Framework;
using System;
using System.Linq;

namespace TrentTobler.RetroCog.Geometry.Cuboid;

public class CubitrixTests
{
    [Test]
    public void TestSetTrue()
    {
        var rand = new Random();
        Cubit3B[] samples = Enumerable.Repeat("gen", 50000)
            .Select(_ => new Cubit3B((byte)rand.Next(), (byte)rand.Next(), (byte)rand.Next()))
            .ToArray();

        var array = new Cubitrix();
        foreach(var sample in samples)
            array[sample] = true;

        var cubes = array.Cubes().ToArray();
        Assert.IsEmpty(cubes.Except(samples), "extra cubes");
        Assert.IsEmpty(samples.Except(cubes), "missing cubes");

        foreach(var sample in samples)
            Assert.AreEqual(array[sample], true, $"{sample} true");
    }

    [Test]
    public void TestSetFalse()
    {
        var rand = new Random();
        Cubit3B[] samples = Enumerable.Repeat("gen", 100000)
            .Select(_ => new Cubit3B((byte)rand.Next(), (byte)rand.Next(), (byte)rand.Next()))
            .ToArray();

        var array = new Cubitrix();
        foreach(var sample in samples)
            array[sample] = true;

        var lastHalf = samples.Take(samples.Length / 2).ToArray();
        var otherHalf = samples.Except(lastHalf).ToArray();
        foreach(var sample in lastHalf)
            array[sample] = false;

        var cubes = array.Cubes().ToArray();
        Assert.IsEmpty(cubes.Except(otherHalf), "extra cubes");
        Assert.IsEmpty(otherHalf.Except(cubes), "missing cubes");

        foreach(var sample in otherHalf)
            Assert.AreEqual(array[sample], true ,$"{sample} true");
        foreach(var sample in lastHalf)
            Assert.AreEqual(array[sample], false, $"{sample} false");
    }
}
