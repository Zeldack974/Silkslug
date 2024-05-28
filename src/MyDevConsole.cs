using BepInEx.Logging;
using DevConsole;
using DevConsole.Commands;
using Silkslug.ColosseumRubicon;
using System;
using System.Linq;
using UnityEngine;

namespace Silkslug
{
    internal static class MyDevConsole
    {
        public static ManualLogSource logSource = BepInEx.Logging.Logger.CreateLogSource("SilkSlug:ConsoleWrite");
        public static RainWorld RW => UnityEngine.Object.FindObjectOfType<RainWorld>();

        // Register Commands
        internal static void RegisterCommands()
        {
            new CommandBuilder("silkslug")
            .Run(args =>
            {
                try
                {
                    DebugCommand(args);
                }
                catch (Exception e) { ConsoleWrite("Error in command", Color.red); Plugin.LogError(e); }
            })
            .AutoComplete(new string[][] {
                new string[] { "hkmenu", "1", "apply_skin", "lock_arenas" }
            })
            .Register();
            new CommandBuilder("setnextchallenge")
            .Run(args =>
            {
                try
                {
                    ColosseumRubicon.Manager.SetChallengeCommand(args);
                }
                catch (Exception e) { ConsoleWrite("Error in command", Color.red); Plugin.LogError(e.Message); }
            })
            //.AutoComplete(new string[][] {
            //                new string[] { "1", "2" }
            //})
            .Register();
        }

        // Debugs commands
        internal static void DebugCommand(string[] args)
        {
            if (args.Length == 0)
            {
                ConsoleWrite("Error: invalid parameter", Color.red);
                return;
            }
            if (args[0] == "hkmenu")
            {
                RW.processManager.RequestMainProcessSwitch(HKMainMenu.HKMainMenuID);
                ConsoleWrite("DebugCommand[1] : " + "hello world", Color.green);
            }
            else if (args[0] == "1")
            {
                ConsoleWrite("DebugCommand[2] : " + "hello world", Color.yellow);
            }
            else if (args[0] == "apply_skin")
            {
                SkinApplyer.SetSlornetSkin();
            }
            else if (args[0] == "lock_arenas")
            {
                int val = RW.progression.miscProgressionData.levelTokens.RemoveAll(t =>
                {
                    ConsoleWrite("remove one CRHell from life");
                    return t.value == "CRHell";
                });
                ConsoleWrite($"removed ");
            }
        }

        public static void ConsoleWrite(string message, Color color)
        {
            try
            {
                GameConsoleWriteLine(message, color);
            }
            catch { }
        }
        public static void ConsoleWrite(string message = "")
        {
            try
            {
                GameConsoleWriteLine(message, Color.white);
            }
            catch { }
        }

        private static void GameConsoleWriteLine(string message, Color color)
        {
            DevConsole.GameConsole.WriteLine(message, color);
            logSource.LogMessage(message);
        }
    }
}
