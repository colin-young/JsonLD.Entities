﻿/**
# Documentation

## Building the `@context` shorthands

The JSON-LD `@context` can be build manually from scratch, but in some cases
it may be possible to reduce the context to some common base for each property
and set it up accordingly.

Currently there are two strategies for creating terms for properties.

### Class identifier as base

First way is to concatenate the class identifier with property names. This can be done
in two ways:

1. If the type is a hash URI, append to the hash fragment:

    `http://example.com/vocab#Person` -> `http://example.com/vocab#Person/propertyName`

1. Otherwise append the property name as hash fragment:

    `http://example.com/vocab/Person` -> `http://example.com/vocab/Person#propertyName`
 **/

using System;
using JsonLD.Entities.Context;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

public class AutomaticContextBuilding
{
    private class Person
    {
        private static string Type => "http://example.com/vocab/Person";

        public string Name { get; set; }

        // also works with custom property names
        [JsonProperty("lastName")]
        public string Surname { get; set; }

        public static JObject Context => new AutoContext<Person>();
    }

    [Test]
    public void Context_can_be_built_from_type_id()
    {
        // given
        dynamic context = Person.Context;

        // then
        Assert.That((string)context.name, Is.EqualTo("http://example.com/vocab/Person#name"));
        Assert.That((string)context.lastName, Is.EqualTo("http://example.com/vocab/Person#lastName"));
    }
}

/**

Note that is if the class type doesn't have a statically resolvable Type identifier
(that is, using a static property or annotation), then it will be necessary to create
`AutoContext` with a constructor which takes URL as one of its parameter.
 
**/

/**

### Vocabulary IRI as base

Another way is to use a common vocabulary base URI to construct identifiers for class'
properties. It is done similarly as above, with a VocabContext&lt;T&gt; type. Property names
will simply be concatenated with it.

**/

public class VocabularyBasedContextBuilding
{
    private class Person
    {
        public string Name { get; set; }

        // also works with custom property names
        [JsonProperty("lastName")]
        public string Surname { get; set; }

        public static JObject Context => new VocabContext<Person>("http://vocab.example.com/terms#");
    }

    [Test]
    public void Context_can_be_built_with_vocabulary_base()
    {
        // given
        dynamic context = Person.Context;

        // then
        Assert.That((string)context.name, Is.EqualTo("http://vocab.example.com/terms#name"));
        Assert.That((string)context.lastName, Is.EqualTo("http://vocab.example.com/terms#lastName"));
    }
}

/**

### Modifying the automatic context

It is possible to retrieve the generated mapping and modify it using the [context API][api].
**/

public class ModifyingAutoGeneratedContext
{
    private class Multilanguage
    {
        public string Translations { get; set; }

        public static JObject Context
        {
            get
            {
                return new VocabContext<Multilanguage>("http://vocab.example.com/terms#")
                    .Property(p => p.Translations, property => property.Type().Id().Container().Language());
            }
        }
    }

    [Test]
    public void Context_can_be_built_with_vocabulary_base()
    {
        // given
        var expected = @"
{
    'translations': {
        '@id': 'http://vocab.example.com/terms#translations',
        '@type': '@id',
        '@container': '@language'
    }
}";

        // when
        dynamic context = Multilanguage.Context;

        // then
        Assert.That(JToken.DeepEquals(context, JObject.Parse(expected)), "Actual context was {0}", context);
    }
}

/**
### Custom automatic context

If you ever find the need to implement a different logic for generating the 
`@context` you can derive from the abstract [`AutoContextBase&lt;T&gt;`][acb] class and implement
an `AutoContextStrategy`.

**/

public class SplitPersonalityAutoContext<T> : AutoContextBase<T>
{
    private const string Base = "http://example.com/vocab#";

    public SplitPersonalityAutoContext()
        : base(new SplittingStrategy())
    {
    }

    public SplitPersonalityAutoContext(JObject context)
        : base(context, new SplittingStrategy())
    {
    }

    /// <summary>
    /// Reverses the property name, if is starts with a letter a-m
    /// </summary>
    private class SplittingStrategy : AutoContextStrategy
    {
        protected override string GetPropertyId(string propertyName)
        {
            if (propertyName[0] <= 'm')
            {
                var charArray = propertyName.ToCharArray();
                Array.Reverse(charArray);
                propertyName = new string(charArray);
            }

            return Base + propertyName;
        }
    }
}

/**
### Extending an existing context

The `AutoContextBase&lt;T&gt;` type and it's default implementation have constructors, which take
a `JObject` parameter. When provided, any existing properties already declared will not be
overridden by those generated.

[acb]: https://github.com/wikibus/JsonLD.Entities/blob/master/src/JsonLD.Entities/Context/AutoContextBase.cs
[api]: /wikibus/JsonLD.Entities/tree/master/src/Documentation/CreatingContext/FluentContext

**/
