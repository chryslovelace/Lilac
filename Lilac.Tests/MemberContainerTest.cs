using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Lilac.Attributes;
using Lilac.Utilities;
using Lilac.Values;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Boolean = Lilac.Values.Boolean;

namespace Lilac.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class MemberContainerTest
    {
        private class TestClass
        {
            [BuiltInMember("getonly", GetOnly = true)]
            public Boolean TestGetOnlyProperty { get; set; }

            [BuiltInMember("getset", GetOnly = false)]
            public Boolean TestGetSetProperty { get; set; }

            [BuiltInMethod("method", typeof(Func<Boolean>))]
            public Boolean TestMethod()
            {
                return TestGetOnlyProperty;
            }
        }

        private TestClass TestObject { get; set; }

        [TestInitialize]
        public void Init()
        {
            TestObject = new TestClass
            {
                TestGetOnlyProperty = Boolean.True,
                TestGetSetProperty = Boolean.True
            };
        }

        [TestMethod]
        public void TestGetOnlyMember()
        {
            MemberContainer<TestClass>.GetMember(TestObject, "getonly").Should().Be(Boolean.True);

            MemberContainer<TestClass>.SetMember(TestObject, "getonly", Boolean.False).Should().BeFalse();
            TestObject.TestGetOnlyProperty.Should().Be(Boolean.True);
        }

        [TestMethod]
        public void TestGetSetMember()
        {
            MemberContainer<TestClass>.GetMember(TestObject, "getset").Should().Be(Boolean.True);

            MemberContainer<TestClass>.SetMember(TestObject, "getset", Boolean.False).Should().BeTrue();
            TestObject.TestGetSetProperty.Should().Be(Boolean.False);

            MemberContainer<TestClass>.GetMember(TestObject, "getset").Should().Be(Boolean.False);
        }

        [TestMethod]
        public void TestMethod()
        {
            var method = MemberContainer<TestClass>.GetMember(TestObject, "method");
            method.Should().BeOfType<BuiltInFunction>();
            var func = (BuiltInFunction) method;
            func.Method.Target.Should().Be(TestObject);
            func.Method.Method.Should().BeSameAs(typeof(TestClass).GetMethod("TestMethod"));
            func.ParameterCount.Should().Be(0);
            func.IsCallable().Should().BeTrue();
            func.Method.DynamicInvoke().Should().Be(Boolean.True);
        }
    }
}
