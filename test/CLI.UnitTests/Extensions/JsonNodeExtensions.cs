﻿using System.Text.Json;
using System.Text.Json.Nodes;

namespace CLI.UnitTests;

public static class JsonNodeExtensions
{
    public static void ShouldHaveStringProperty(this JsonNode? json, string propertyName, string value)
    {
        json.Should().NotBeNull();
        json!.GetValueKind().Should().Be(JsonValueKind.Object);

        var jObject = json.AsObject();

        var propertyNode = jObject[propertyName];
        propertyNode.Should().NotBeNull($"{propertyName} should be present");
        propertyNode!.GetValueKind().Should().Be(JsonValueKind.String);
        propertyNode.GetValue<string>().Should().Be(value);
    }

    public static void ShouldHaveBooleanProperty(this JsonNode? json, string propertyName, bool value)
    {
        json.Should().NotBeNull();
        json!.GetValueKind().Should().Be(JsonValueKind.Object);

        var jObject = json.AsObject();

        var propertyNode = jObject[propertyName];
        propertyNode.Should().NotBeNull($"{propertyName} should be present");
        propertyNode!.GetValueKind().Should().BeOneOf(JsonValueKind.True, JsonValueKind.False);
        propertyNode.GetValue<bool>().Should().Be(value);
    }

    public static void ShouldHaveNullProperty(this JsonNode? json, string propertyName)
    {
        json.Should().NotBeNull();
        json!.GetValueKind().Should().Be(JsonValueKind.Object);

        var jObject = json.AsObject();

        jObject.ContainsKey(propertyName).Should().BeTrue($"{propertyName} should be present");

        var propertyNode = jObject[propertyName];
        propertyNode.Should().BeNull($"{propertyName} should be null");
    }

    public static void ShouldNotHaveProperty(this JsonNode? json, string propertyName)
    {
        json.Should().NotBeNull();
        json!.GetValueKind().Should().Be(JsonValueKind.Object);

        var jObject = json.AsObject();

        jObject.ContainsKey(propertyName).Should().BeFalse($"{propertyName} should not be present");
        jObject[propertyName].Should().BeNull($"{propertyName} should not be present");
    }

    public static void ShouldHaveObjectProperty(this JsonNode? json, string propertyName)
    {
        json.Should().NotBeNull();
        json!.GetValueKind().Should().Be(JsonValueKind.Object);

        var jObject = json.AsObject();

        var propertyNode = jObject[propertyName];
        propertyNode.Should().NotBeNull($"{propertyName} should be present");
        propertyNode!.GetValueKind().Should().Be(JsonValueKind.Object);
    }

    public static void ShouldHaveMapProperty(this JsonNode? json, string propertyName, IDictionary<string, string?> values)
    {
        json.ShouldHaveObjectProperty(propertyName);

        var propertyObject = json![propertyName]!.AsObject();

        foreach (var value in values)
        {
            var valueNode = propertyObject[value.Key];

            if (value.Value != null)
            {
                valueNode.Should().NotBeNull();
                valueNode!.GetValueKind().Should().Be(JsonValueKind.String);
                valueNode.GetValue<string>().Should().Be(value.Value);
            }
            else
            {
                propertyObject.ContainsKey(value.Key).Should().BeTrue($"{value.Key} should be present");
                valueNode.Should().BeNull();
            }
        }

        propertyObject.Count.Should().Be(values.Count);
    }

    public static void ShouldHaveStringArrayProperty(this JsonNode? json, string propertyName, IReadOnlyCollection<string> expected)
    {
        json.Should().NotBeNull();
        json!.GetValueKind().Should().Be(JsonValueKind.Object);

        var jObject = json.AsObject();

        var propertyNode = jObject[propertyName];
        propertyNode.Should().NotBeNull($"{propertyName} should be present");
        propertyNode!.GetValueKind().Should().Be(JsonValueKind.Array);

        var arrayNode = propertyNode.AsArray();

        var actual = new List<string>();

        foreach (var item in arrayNode)
        {
            item.Should().NotBeNull();
            item!.GetValueKind().Should().Be(JsonValueKind.String);

            actual.Add(item.GetValue<string>());
        }

        actual.Should().BeEquivalentTo(expected);
    }

    public static void ShouldHaveArrayProperty(this JsonNode? json, string propertyName)
    {
        json.Should().NotBeNull();
        json!.GetValueKind().Should().Be(JsonValueKind.Object);

        var jObject = json.AsObject();

        var propertyNode = jObject[propertyName];
        propertyNode.Should().NotBeNull($"{propertyName} should be present");
        propertyNode!.GetValueKind().Should().Be(JsonValueKind.Array);
    }

    public static void ShouldHaveArrayPropertyWithLength(this JsonNode? json, string propertyName, int expectedLength)
    {
        json.Should().NotBeNull();
        json!.GetValueKind().Should().Be(JsonValueKind.Object);

        var jObject = json.AsObject();

        var propertyNode = jObject[propertyName];
        propertyNode.Should().NotBeNull($"{propertyName} should be present");
        propertyNode!.GetValueKind().Should().Be(JsonValueKind.Array);

        var arrayNode = propertyNode.AsArray();

        arrayNode.Count.Should().Be(expectedLength);
    }
}
