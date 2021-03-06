﻿/**
# Documentation

## Deserializing graphs of objects

First let's import the required namespaces.
 **/

using System;
using JsonLD.Entities;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

/**
The example below will deserialize to instances of a `PersonWithAddress` class, which contains a reference to an `Address`, which in turn references a `City`.
 **/

public class PersonWithAddress
{
    public Uri Id { get; set; }

    public Address Address { get; set; }
}

public class Address
{
    public string Street { get; set; }

    public string PostalCode { get; set; }

    public City City { get; set; }
}

public class City
{
    public Uri Id { get; set; }

    public string Name { get; set; }
}

/**
#### Deserialize with JSON-LD frame

In a case like above, when we expect JSON-LD document, which contains multiple JSON objects, simply [compacting][compacting] it won't work, 
because the resulting JSON will be an array of all objects disconnected.

For that reason the [framing algorithm][framing] has been defined, which is similar to [compacting][compacting], but allows influencing
the JSON tree structure.

In this example, the JSON-LD object will use the [`@graph`][atGraph] keyword to demonstrate a structure, which does not form a coherent
object tree. Such could be the result of converting data in another RDF format into JSON-LD.
**/

[TestFixture]
public class FramedDeserialization
{

private static readonly JToken JsonLd = JToken.Parse(@"
{
  '@graph': [
    {
      '@id': '_:b0',
      '@type': 'http://www.w3.org/2006/vcard/ns#VCard',
      'http://www.w3.org/2006/vcard/ns#hasLocality': {
        '@id': 'http://sws.geonames.org/5328041/'
      },
      'http://www.w3.org/2006/vcard/ns#postal-code': '90-210',
      'http://www.w3.org/2006/vcard/ns#street-address': 'Programmer\'s Avenue 1337'
    },
    {
      '@id': 'http://t-code.pl/#tomasz',
      '@type': 'http://xmlns.com/foaf/0.1/Person',
      'http://www.w3.org/2006/vcard/ns#address': {
        '@id': '_:b0'
      }
    },
    {
      '@id': 'http://sws.geonames.org/5328041/',
      'http://www.geonames.org/ontology#name': 'Beverly Hills'
    }
  ]
}");

/**
Using the context below simplifies the document structure by hiding all the property URIs, but it will not alter the general outline. In 
other words, the `@graph` array will still contain three objects for the person, address and city as shown in the
[JSON-LD playground][sample-compact].
**/

private static readonly JObject Context = JObject.Parse(@"
{
  'vcard': 'http://www.w3.org/2006/vcard/ns#',
  'street': 'vcard:street-address',
  'postalCode': 'vcard:postal-code',
  'address': 'vcard:address',
  'city': 'vcard:hasLocality',
  'name': 'http://www.geonames.org/ontology#name',
  'Address': 'vcard:VCard',
  'Person': 'http://xmlns.com/foaf/0.1/Person'
}
");

/**
This is where the [framing][framing] comes in. Another [example][sample-frame] in the playgroud demonstrates the result, in which the JSON-LD
object in `@graph` has been _compressed_ into a single root by matching the `@id` property values. As you can see, the frame is an object,
which reuses the `@context` but also defines the structure.

In JsonLd.Entities, the `@context` is ignored and effectively replaced by the one declared above. It will only contain the `@type` property
to ensure that the preson is the root of out object
**/

private static readonly JObject Frame = JObject.Parse(@"
{
  '@type': 'http://xmlns.com/foaf/0.1/Person'
}
");

/**
And lastly, here's the complete example using the above frame and context.
**/

[Test]
public void Can_deserialize_framed_document()
{
    // given
    var contextProvider = new StaticContextProvider();
    contextProvider.SetContext(typeof(PersonWithAddress), Context);

    var frameProvider = new StaticFrameProvider();
    frameProvider.SetFrame(typeof(PersonWithAddress), Frame);

    // when
    IEntitySerializer serializer = new EntitySerializer(contextProvider, frameProvider);
    var person = serializer.Deserialize<PersonWithAddress>(JsonLd);

    // then
    Assert.That(person.Address.PostalCode, Is.EqualTo("90-210"));
    Assert.That(person.Address.Street, Is.EqualTo("Programmer's Avenue 1337"));
    Assert.That(person.Address.City.Id, Is.EqualTo(new Uri("http://sws.geonames.org/5328041/")));
    Assert.That(person.Address.City.Name, Is.EqualTo("Beverly Hills"));
}
}

/**
[framing]: http://json-ld.org/spec/latest/json-ld-framing/
[compacting]: http://www.w3.org/TR/json-ld-api/#compaction
[atGraph]: http://www.w3.org/TR/json-ld/#named-graphs
[sample-compact]: http://json-ld.org/playground/#startTab=tab-compacted&json-ld=%7B%22%40graph%22%3A%5B%7B%22%40id%22%3A%22_%3Ab0%22%2C%22%40type%22%3A%22http%3A%2F%2Fwww.w3.org%2F2006%2Fvcard%2Fns%23VCard%22%2C%22http%3A%2F%2Fwww.w3.org%2F2006%2Fvcard%2Fns%23hasLocality%22%3A%7B%22%40id%22%3A%22http%3A%2F%2Fsws.geonames.org%2F5328041%2F%22%7D%2C%22http%3A%2F%2Fwww.w3.org%2F2006%2Fvcard%2Fns%23postal-code%22%3A%2290-210%22%2C%22http%3A%2F%2Fwww.w3.org%2F2006%2Fvcard%2Fns%23street-address%22%3A%22Programmer's%20Avenue%201337%22%7D%2C%7B%22%40id%22%3A%22http%3A%2F%2Ft-code.pl%2F%23tomasz%22%2C%22%40type%22%3A%22http%3A%2F%2Fxmlns.com%2Ffoaf%2F0.1%2FPerson%22%2C%22http%3A%2F%2Fwww.w3.org%2F2006%2Fvcard%2Fns%23address%22%3A%7B%22%40id%22%3A%22_%3Ab0%22%7D%7D%2C%7B%22%40id%22%3A%22http%3A%2F%2Fsws.geonames.org%2F5328041%2F%22%2C%22http%3A%2F%2Fwww.geonames.org%2Fontology%23name%22%3A%22Beverly%20Hills%22%7D%5D%7D&frame=%7B%22%40context%22%3A%7B%22vcard%22%3A%22http%3A%2F%2Fwww.w3.org%2F2006%2Fvcard%2Fns%23%22%2C%22street%22%3A%22vcard%3Astreet-address%22%2C%22postalCode%22%3A%22vcard%3Apostal-code%22%2C%22address%22%3A%22vcard%3Aaddress%22%2C%22city%22%3A%22vcard%3AhasLocality%22%2C%22name%22%3A%22http%3A%2F%2Fwww.geonames.org%2Fontology%23name%22%2C%22Address%22%3A%22vcard%3AVCard%22%2C%22Person%22%3A%22http%3A%2F%2Fxmlns.com%2Ffoaf%2F0.1%2FPerson%22%7D%2C%22%40type%22%3A%22Person%22%7D&context=%7B%22vcard%22%3A%22http%3A%2F%2Fwww.w3.org%2F2006%2Fvcard%2Fns%23%22%2C%22street%22%3A%22vcard%3Astreet-address%22%2C%22postalCode%22%3A%22vcard%3Apostal-code%22%2C%22address%22%3A%22vcard%3Aaddress%22%2C%22city%22%3A%22vcard%3AhasLocality%22%2C%22name%22%3A%22http%3A%2F%2Fwww.geonames.org%2Fontology%23name%22%2C%22Address%22%3A%22vcard%3AVCard%22%2C%22Person%22%3A%22http%3A%2F%2Fxmlns.com%2Ffoaf%2F0.1%2FPerson%22%7D
[sample-frame]: http://json-ld.org/playground/#startTab=tab-framed&json-ld=%7B%22%40graph%22%3A%5B%7B%22%40id%22%3A%22_%3Ab0%22%2C%22%40type%22%3A%22http%3A%2F%2Fwww.w3.org%2F2006%2Fvcard%2Fns%23VCard%22%2C%22http%3A%2F%2Fwww.w3.org%2F2006%2Fvcard%2Fns%23hasLocality%22%3A%7B%22%40id%22%3A%22http%3A%2F%2Fsws.geonames.org%2F5328041%2F%22%7D%2C%22http%3A%2F%2Fwww.w3.org%2F2006%2Fvcard%2Fns%23postal-code%22%3A%2290-210%22%2C%22http%3A%2F%2Fwww.w3.org%2F2006%2Fvcard%2Fns%23street-address%22%3A%22Programmer's%20Avenue%201337%22%7D%2C%7B%22%40id%22%3A%22http%3A%2F%2Ft-code.pl%2F%23tomasz%22%2C%22%40type%22%3A%22http%3A%2F%2Fxmlns.com%2Ffoaf%2F0.1%2FPerson%22%2C%22http%3A%2F%2Fwww.w3.org%2F2006%2Fvcard%2Fns%23address%22%3A%7B%22%40id%22%3A%22_%3Ab0%22%7D%7D%2C%7B%22%40id%22%3A%22http%3A%2F%2Fsws.geonames.org%2F5328041%2F%22%2C%22http%3A%2F%2Fwww.geonames.org%2Fontology%23name%22%3A%22Beverly%20Hills%22%7D%5D%7D&frame=%7B%22%40context%22%3A%7B%22vcard%22%3A%22http%3A%2F%2Fwww.w3.org%2F2006%2Fvcard%2Fns%23%22%2C%22street%22%3A%22vcard%3Astreet-address%22%2C%22postalCode%22%3A%22vcard%3Apostal-code%22%2C%22address%22%3A%22vcard%3Aaddress%22%2C%22city%22%3A%22vcard%3AhasLocality%22%2C%22name%22%3A%22http%3A%2F%2Fwww.geonames.org%2Fontology%23name%22%2C%22Address%22%3A%22vcard%3AVCard%22%2C%22Person%22%3A%22http%3A%2F%2Fxmlns.com%2Ffoaf%2F0.1%2FPerson%22%7D%2C%22%40type%22%3A%22Person%22%7D
**/
