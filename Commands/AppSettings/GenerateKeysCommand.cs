﻿using config.Models;
using config.Settings.AppSettings;
using config.Singleton;
using config.Utils;
using config.Utils.Messages;
using Spectre.Console;
using Spectre.Console.Cli;

namespace config.Commands.AppSettings;
internal class GenerateKeysCommand : Command<GenereateKeysSettings>
{
    public override int Execute(CommandContext context, GenereateKeysSettings settings)
    {
        var appSettings = AppSettingsSingleton.Instance.Lines();

        var keys = appSettings.SelectMany(x => x.Keys.Select(x => x.Key));

        if (settings.SelectKeys)
        {
            keys = DisplayMultiSelection(appSettings);
        }

        var strings = CreateListResult(appSettings, keys, settings.Json);

        if (settings.DisplayPerLines)
        {
            RepeatableStatus.Run(strings,
                KeysMsg.INF001,
                KeysMsg.INF007,
                KeysMsg.INF006);
        }
        else
        {
            var rows = strings.Select(x => new Text(x));

            AnsiConsole.Write(new Rows(rows));
        }

        return 0;
    }

    private static IEnumerable<string> DisplayMultiSelection(IEnumerable<AppSettingsGroup> appSettings)
    {
        var multiSelection = new MultiSelectionPrompt<string>()
            .Title("Select [green]keys[/]:")
            .Required()
            .PageSize(10)
            .InstructionsText("[grey](Press [blue]<space>[/] to toggle a database, " +
                              "[green]<enter>[/] to accept)[/]");

        foreach (var group in appSettings)
        {
            multiSelection.AddChoiceGroup(group.GroupName, group.Keys.Select(x=> x.Key));
        }

        return AnsiConsole.Prompt(multiSelection);
    }

    private IEnumerable<string> CreateListResult(List<AppSettingsGroup> appSettings, IEnumerable<string> keys, bool json)
    {
        var output = appSettings
            .SelectMany(appSetting =>
                appSetting.Keys
                    .Select(x =>!json 
                            ? x.ToConfig() 
                            : x.ToJson()));

        return output;
    }
}
