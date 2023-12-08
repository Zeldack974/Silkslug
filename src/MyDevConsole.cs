using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevConsole.Commands;
using UnityEngine;

namespace Silkslug
{
    internal class MyDevConsole
    {
        // Register Commands
        internal static void RegisterCommands()
        {
            new CommandBuilder("silkslug_debug")
            .Run(args =>
            {
                try
                {
                    DebugCommand(args);
                }
                catch { ConsoleWrite("Error in command", Color.red); }
            })
            .AutoComplete(new string[][] {
                new string[] { "1", "2" }
            })
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
            if (args[0] == "1")
            {
                ConsoleWrite("DebugCommand[1] : " + "hello world", Color.green);
            }
            else if (args[0] == "2")
            {
                ConsoleWrite("DebugCommand[1] : " + "hello world", Color.yellow);
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
        }
    }
}
