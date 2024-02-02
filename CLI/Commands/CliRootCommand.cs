using System.CommandLine;

namespace Kong.Portal.CLI.Commands;

public class CliRootCommand : RootCommand
{
    public CliRootCommand()
        : base("Kong Portal CLI") { }
}
