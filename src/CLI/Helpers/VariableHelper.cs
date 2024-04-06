using System.Text.RegularExpressions;

namespace Kong.Portal.CLI;

internal class VariableHelper(IEnvironmentVariableReader environmentVariableReader)
{
    private static readonly Regex VariableDefinitionRegex = new(@"^(?<Key>\w+)=(?<Value>.*)$");
    private static readonly Regex VariableReplaceRegex = new(@"\${{\s*var (?<VariableName>\w+)\s*}}");
    private static readonly Regex EnvVariableReplaceRegex = new(@"\${{\s*env (?<VariableName>\w+)\s*}}");

    public static IReadOnlyDictionary<string, string> Parse(IEnumerable<string> variables)
    {
        var dictionary = new Dictionary<string, string>();

        foreach (var variable in variables)
        {
            var match = VariableDefinitionRegex.Match(variable);
            if (!match.Success)
            {
                throw new OutputErrorException($"Invalid variable: {variable}");
            }

            dictionary.Add(match.Groups["Key"].Value, match.Groups["Value"].Value);
        }

        return dictionary.AsReadOnly();
    }

    public string Replace(string value, IReadOnlyDictionary<string, string> variables)
    {
        var updatedValue = VariableReplaceRegex.Replace(
            value,
            match => !variables.TryGetValue(match.Groups["VariableName"].Value, out var variableValue) ? match.Value : variableValue
        );

        updatedValue = EnvVariableReplaceRegex.Replace(
            updatedValue,
            match => environmentVariableReader.Get(match.Groups["VariableName"].Value) ?? match.Value
        );

        return updatedValue;
    }
}
