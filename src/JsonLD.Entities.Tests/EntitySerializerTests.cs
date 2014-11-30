﻿using FakeItEasy;
using JsonLD.Entities.Tests.Entities;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JsonLD.Entities.Tests
{
    [TestFixture]
    public class EntitySerializerTests
    {
        private IContextProvider _provider;
        private EntitySerializer _serializer;

        [SetUp]
        public void Setup()
        {
            _provider = A.Fake<IContextProvider>();
            _serializer = new EntitySerializer(_provider);
        }

        [Test]
        [ExpectedException(typeof(ContextNotFoundException))]
        public void Deserializing_quads_should_throw_when_context_isnt_found()
        {
            // given
            A.CallTo(() => _provider.GetContext(typeof(Person))).Returns(null);

            // when
            _serializer.Deserialize<Person>(string.Empty);
        }
    }
}
