using System.Text.Json;
using System.Text.Json.Nodes;

namespace CLI.UnitTests;

public static class JsonNodeExtensions
{
    public static void ShouldHaveStringProperty(this JsonNode? json, string propertyName, string value)
    {
        json.ShouldNotBeNull();
        json.GetValueKind().ShouldBe(JsonValueKind.Object);

        var jObject = json.AsObject();

        var propertyNode = jObject[propertyName];
        propertyNode.ShouldNotBeNull($"{propertyName} should be present");
        propertyNode.GetValueKind().ShouldBe(JsonValueKind.String);
        propertyNode.GetValue<string>().ShouldBe(value);
    }

    public static void ShouldHaveBooleanProperty(this JsonNode? json, string propertyName, bool value)
    {
        json.ShouldNotBeNull();
        json.GetValueKind().ShouldBe(JsonValueKind.Object);

        var jObject = json.AsObject();

        var propertyNode = jObject[propertyName];
        propertyNode.ShouldNotBeNull($"{propertyName} should be present");
        propertyNode.GetValueKind().ShouldBeOneOf(JsonValueKind.True, JsonValueKind.False);
        propertyNode.GetValue<bool>().ShouldBe(value);
    }

    public static void ShouldHaveNullProperty(this JsonNode? json, string propertyName)
    {
        json.ShouldNotBeNull();
        json.GetValueKind().ShouldBe(JsonValueKind.Object);

        var jObject = json.AsObject();

        jObject.ContainsKey(propertyName).ShouldBeTrue($"{propertyName} should be present");

        var propertyNode = jObject[propertyName];
        propertyNode.ShouldBeNull($"{propertyName} should be null");
    }

    public static void ShouldNotHaveProperty(this JsonNode? json, string propertyName)
    {
        json.ShouldNotBeNull();
        json.GetValueKind().ShouldBe(JsonValueKind.Object);

        var jObject = json.AsObject();

        jObject.ContainsKey(propertyName).ShouldBeFalse($"{propertyName} should not be present");
        jObject[propertyName].ShouldBeNull($"{propertyName} should not be present");
    }

    public static void ShouldHaveObjectProperty(this JsonNode? json, string propertyName)
    {
        json.ShouldNotBeNull();
        json.GetValueKind().ShouldBe(JsonValueKind.Object);

        var jObject = json.AsObject();

        var propertyNode = jObject[propertyName];
        propertyNode.ShouldNotBeNull($"{propertyName} should be present");
        propertyNode.GetValueKind().ShouldBe(JsonValueKind.Object);
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
                valueNode.ShouldNotBeNull();
                valueNode.GetValueKind().ShouldBe(JsonValueKind.String);
                valueNode.GetValue<string>().ShouldBe(value.Value);
            }
            else
            {
                propertyObject.ContainsKey(value.Key).ShouldBeTrue($"{value.Key} should be present");
                valueNode.ShouldBeNull();
            }
        }

        propertyObject.Count.ShouldBe(values.Count);
    }

    public static void ShouldHaveStringArrayProperty(this JsonNode? json, string propertyName, IReadOnlyCollection<string> expected)
    {
        json.ShouldNotBeNull();
        json.GetValueKind().ShouldBe(JsonValueKind.Object);

        var jObject = json.AsObject();

        var propertyNode = jObject[propertyName];
        propertyNode.ShouldNotBeNull($"{propertyName} should be present");
        propertyNode.GetValueKind().ShouldBe(JsonValueKind.Array);

        var arrayNode = propertyNode.AsArray();

        var actual = new List<string>();

        foreach (var item in arrayNode)
        {
            item.ShouldNotBeNull();
            item.GetValueKind().ShouldBe(JsonValueKind.String);

            actual.Add(item.GetValue<string>());
        }

        actual.ShouldBe(expected.ToList(), ignoreOrder: true);
    }

    public static void ShouldHaveArrayProperty(this JsonNode? json, string propertyName)
    {
        json.ShouldNotBeNull();
        json.GetValueKind().ShouldBe(JsonValueKind.Object);

        var jObject = json.AsObject();

        var propertyNode = jObject[propertyName];
        propertyNode.ShouldNotBeNull($"{propertyName} should be present");
        propertyNode.GetValueKind().ShouldBe(JsonValueKind.Array);
    }

    public static void ShouldHaveArrayPropertyWithLength(this JsonNode? json, string propertyName, int expectedLength)
    {
        json.ShouldNotBeNull();
        json.GetValueKind().ShouldBe(JsonValueKind.Object);

        var jObject = json.AsObject();

        var propertyNode = jObject[propertyName];
        propertyNode.ShouldNotBeNull($"{propertyName} should be present");
        propertyNode.GetValueKind().ShouldBe(JsonValueKind.Array);

        var arrayNode = propertyNode.AsArray();

        arrayNode.Count.ShouldBe(expectedLength);
    }
}
