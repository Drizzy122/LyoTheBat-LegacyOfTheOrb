using System.Collections.Generic;
using System.Threading.Tasks;

public class CommandInvoker
{
    public async Task ExecuteCommand(List<ICommand> commands)
    {
        foreach (var command in commands)
        {
            await command.Execute();
        }
    }
}