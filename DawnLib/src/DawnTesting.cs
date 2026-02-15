using System;

namespace Dawn;

internal class DawnTesting
{
    internal static void TestCommands()
    {
        TerminalCommandBasicInformation versionCommandBasicInformation = new TerminalCommandBasicInformation("DawnLibVersionCommand", "Test", "Prints the version of DawnLib!", ClearText.Result | ClearText.Query);
        DawnLib.DefineTerminalCommand(NamespacedKey<DawnTerminalCommandInfo>.From("dawn_lib", "version_command"), versionCommandBasicInformation, builder =>
        {
            builder.SetKeywords(["version"]);
            builder.DefineSimpleCommand(simpleBuilder =>
            {
                simpleBuilder.SetResultDisplayText(() => $"DawnLib version {MyPluginInfo.PLUGIN_VERSION}");
            });
        }); // Simple command

        TerminalCommandBasicInformation lightsCommandBasicInformation = new TerminalCommandBasicInformation("DawnLibLightsCommand", "Test", "Toggles the lights in the ship.", ClearText.Result | ClearText.Query);
        DawnLib.DefineTerminalCommand(NamespacedKey<DawnTerminalCommandInfo>.From("dawn_lib", "lights_command"), lightsCommandBasicInformation, builder =>
        {
            builder.SetKeywords(["lights", "dark", "vow", "help"]);
            builder.DefineSimpleCommand(simpleBuilder =>
            {
                simpleBuilder.SetResultDisplayText(BasicLightsCommand);
            });
        }); // Simple Command

        TerminalCommandBasicInformation complexCommandExampleBasicInformation = new TerminalCommandBasicInformation("DawnLibComplexCommand", "Test", "Test complex command", ClearText.Result | ClearText.Query);
        DawnLib.DefineTerminalCommand(NamespacedKey<DawnTerminalCommandInfo>.From("dawn_lib", "test_complex_command"), complexCommandExampleBasicInformation, builder =>
        {
            builder.SetKeywords(["complex"]);
            builder.DefineComplexCommand(complexBuilder =>
            {
                complexBuilder.SetSecondaryKeywords(["first", "second", "third"]);
                complexBuilder.SetResultsDisplayText([() => "result 1", () => "result 2", () => "result 3"]);
            });
        }); // Complex Command

        TerminalCommandBasicInformation inputCommandBasicInformation = new TerminalCommandBasicInformation("DawnLibInputs", "Test", "Takes the player's input and uses it on the next screen.", ClearText.Result | ClearText.Query);
        DawnLib.DefineTerminalCommand(NamespacedKey<DawnTerminalCommandInfo>.From("dawn_lib", "test_input_command"), inputCommandBasicInformation, builder =>
        {
            builder.SetKeywords(["input"]);
            builder.DefineInputCommand(inputBuilder =>
            {
                inputBuilder.SetResultDisplayText(InputCommandExample);
            });
        }); // Input Command

        Action<bool> dawnEvent = new(continued =>
        {
            if (!continued)
            {
                GameNetworkManager.Instance.localPlayerController.DamagePlayer(50);
            }
        });
        TerminalCommandBasicInformation queryCommandBasicInformation = new TerminalCommandBasicInformation("DawnLibQueryCommand", "Test", "Test query command with added compatible nouns", ClearText.Result | ClearText.Query);
        DawnLib.DefineTerminalCommand(NamespacedKey<DawnTerminalCommandInfo>.From("dawn_lib", "test_query_command"), queryCommandBasicInformation, builder =>
        {
            builder.SetKeywords(["qUEry", "version"]);
            builder.DefineSimpleQueryCommand(queryCommandBuilder =>
            {
                queryCommandBuilder.SetResult(() => "You have selected, YES!\n\n");
                queryCommandBuilder.SetContinueOrCancel(() => "This is a test query, respond [YES] or [NO]\n\n");
                queryCommandBuilder.SetCancel(() => "You have selected, NO!\n\n");
                queryCommandBuilder.SetContinueConditions(new Func<bool>(() => GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer != null));
                queryCommandBuilder.SetQueryEvent(dawnEvent);
                queryCommandBuilder.SetContinueWord("yes");
                queryCommandBuilder.SetCancelWord("no");
            });
        }); // Simple Query Command

        TerminalCommandBasicInformation complexQueryCommandBasicInformation = new TerminalCommandBasicInformation("DawnLibComplexQueryCommand", "Test", "Test complex query command", ClearText.Result | ClearText.Query);
        DawnLib.DefineTerminalCommand(NamespacedKey<DawnTerminalCommandInfo>.From("dawn_lib", "test_complex_query_command"), complexQueryCommandBasicInformation, builder =>
        {
           builder.SetKeywords(["complex query", "please"]);
           builder.DefineComplexQueryCommand(complexQueryCommandBuilder =>
           {
                complexQueryCommandBuilder.SetContinueOrCancelDisplayTexts(() => "This is a test complex query, what's your favourite mod?\n\n");
                complexQueryCommandBuilder.SetResultDisplayTexts([() => "wow, that's a nice mod", () => "Not a good mod", () => "It's had its ups and downs"]);
                complexQueryCommandBuilder.SetContinueKeywords(["coderebirth", "surfaced", "biodiversity"]);
                complexQueryCommandBuilder.SetCancelKeyword("usefulzapgun");
                complexQueryCommandBuilder.SetCancelDisplayText(() => "That's not a nice mod\n\n");
           });
        }); // Complex Query Command

        TerminalCommandBasicInformation eventDrivenCommandBasicInformation = new TerminalCommandBasicInformation("DawnLibEventDrivenCommand", "Test", "Test event driven command", ClearText.Result | ClearText.Query);
        DawnLib.DefineTerminalCommand(NamespacedKey<DawnTerminalCommandInfo>.From("dawn_lib", "test_event_driven_command"), eventDrivenCommandBasicInformation, builder =>
        {
           builder.SetKeywords(["event driven"]);
           builder.DefineEventDrivenCommand(eventDrivenCommandBuilder =>
           {
               eventDrivenCommandBuilder.SetResultNodeDisplayText(() => "This is a test event driven command, it copies the... eject command... wait what?\n\n");
               eventDrivenCommandBuilder.SetOnTerminalEvent(VanillaTerminalEvents.EjectPlayersEvent);
           });
        });
    }

    private static string BasicLightsCommand()
    {
        if (StartOfRound.Instance == null)
        {
            return "Command not Initialized!\n\n";
        }

        StartOfRound.Instance.shipRoomLights.ToggleShipLights();
        if (StartOfRound.Instance.shipRoomLights.areLightsOn)
        {
            return $"Ship Lights are [ON]\n\n";
        }
        else
        {
            return $"Ship Lights are [OFF]\n\n";
        }
    }

    private static string InputCommandExample(string userInput)
    {
        return $"This is a test command displaying user input after the command.\n\nUser input is [ {userInput} ]\n\n";
    }
}
