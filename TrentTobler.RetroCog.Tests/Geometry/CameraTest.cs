using NUnit.Framework;
using OpenTK.Mathematics;
using System;

namespace TrentTobler.RetroCog.Geometry
{
    public class CameraTest
    {
        [Test]
        public void Constructor_Should_HaveInitialValues()
        {
            Assert.AreEqual(new Vector3(0, -1,0), new Camera().Eye, "Eye");
            Assert.AreEqual(new Vector3(0, 1, 0), new Camera().Heading, "Heading");
            Assert.AreEqual(new Vector3(0, 0, 1), new Camera().Up, "Up");
            Assert.AreEqual(new Matrix4(
                +1, +0, +0, +0,
                +0, +0, -1, +0,
                +0, +1, +0, +0,
                +0, +0, -1, +1
            ), new Camera().View, "View");
        }

        [TestCase(0f)]
        [TestCase(1f)]
        [TestCase(3f)]
        public void Forward_Should_MatchInvariants(float dist)
        {
            var orig = new Camera();

            var camera = new Camera();
            camera.Forward(dist);

            Assert.AreEqual(camera.Eye - orig.Eye, camera.Heading * dist, "distance");
            Assert.AreEqual(orig.Heading, camera.Heading, "Heading");
            Assert.AreEqual(orig.Up, camera.Up, "Up");
        }

        [TestCase(0f)]
        [TestCase(1f)]
        [TestCase(3f)]
        public void Strafe_Should_MatchInvariants(float dist)
        {
            var orig = new Camera();

            var camera = new Camera();
            camera.Strafe(dist);

            Assert.AreEqual(camera.Eye - orig.Eye, Vector3.Cross(camera.Heading, camera.Up) * dist, "distance");
            Assert.AreEqual(orig.Heading, camera.Heading, "Heading");
            Assert.AreEqual(orig.Up, camera.Up, "Up");
        }

        [TestCase(0f)]
        [TestCase(1f)]
        [TestCase(3f)]
        public void Elevate_Should_MatchInvariants(float dist)
        {
            var orig = new Camera();

            var camera = new Camera();
            camera.Elevate(dist);

            Assert.AreEqual(camera.Eye - orig.Eye, camera.Up * dist, "distance");
            Assert.AreEqual(orig.Heading, camera.Heading, "Heading");
            Assert.AreEqual(orig.Up, camera.Up, "Up");
        }

        [TestCase(0f)]
        [TestCase(90f)]
        [TestCase(180f)]
        [TestCase(-60f)]
        public void Turn_Should_MatchInvariants(float angle)
        {
            var orig = new Camera();

            var camera = new Camera();
            camera.Turn(angle);

            Assert.AreEqual(camera.Eye , orig.Eye, "Eye");
            Assert.AreEqual(orig.Up, camera.Up, "Up");

            Assert.AreEqual(orig.Heading.Length, camera.Heading.Length, 1e-4, "Heading");

            var cos = Math.Cos(angle * Math.PI / 180);
            var dot = Vector3.Dot(orig.Heading, camera.Heading);
            Assert.AreEqual(dot, cos, 1e-4, "Cosine angle");
        }
    }
}
