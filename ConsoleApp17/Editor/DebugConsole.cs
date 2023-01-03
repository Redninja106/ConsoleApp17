using ImGuiNET;
using SimulationFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ConsoleApp17.Editor;

internal class DebugConsole : DebugWindow
{
    public static int MaxLines { get; set; } = 500;

    public static readonly DebugConsole Instance = new();

    private readonly List<string> lines = new();
    private readonly Dictionary<string, Action<string[]>> commands = new();
    private readonly Dictionary<string, string> commandHelp = new();

    private string inputText = string.Empty;

    public override Key? KeyBind => Key.F1;
    public override string Title => "Debug Console (F1)";
    public override ImGuiWindowFlags WindowFlags => ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;

    private DebugConsole()
    {
        static DebugWindow FindDebugWindow(string[] args)
        {
            if (args.Length < 1)
                throw new Exception("Expected an argument");

            if (args.Length > 1)
                throw new Exception("Too many arguments");

            var result = Windows.Where(window => window.GetType().Name.ToUpper() == args[0].ToUpper() || window.Title.ToUpper() == args[0].ToUpper()).FirstOrDefault();

            if (result is null)
                throw new Exception($"Could not find debug window '{args[0]}'");

            return result;
        }

        RegisterCommand("window", args =>
        {
            switch (args[0])
            {
                case "show":
                    FindDebugWindow(args[1..]).Show();
                    break;
                case "hide":
                    FindDebugWindow(args[1..]).Hide();
                    break;
                case "list":
                    foreach (var window in Windows)
                    {
                        Console.WriteLine(window.GetType().Name);
                    }
                    break;
                default:
                    throw new Exception($"unknown action '{args[0]}'");
            }
        });

        Console.SetOut(new DebugConsoleWriter(this, Console.Out));

        RegisterHelpDesc("echo", "echo: Writes it's arguments to the console.");
        RegisterCommand("echo", args =>
        {
            Write(string.Join(' ', args));

            Write(Environment.NewLine);
        });

        RegisterCommand("!", args => Console.WriteLine("am panick!"));

        RegisterCommand("help", args =>
        Console.WriteLine(commandHelp[args[0]]));
        RegisterAlias("?", "help");

        RegisterCommand("clear", args =>
        {
            lines.Clear();
        });
    }

    protected override void OnLayout()
    {
        var style = ImGui.GetStyle();

        if (ImGui.BeginChildFrame(ImGui.GetID("Console Content"), new(-style.WindowBorderSize, -(style.WindowBorderSize * 2 + style.ItemInnerSpacing.Y + style.ItemSpacing.Y + 13))))
        {
            foreach (var line in lines)
            {
                ImGui.Text(line);
            }

            ImGui.EndChild();
        }

        if (ImGui.IsWindowFocused() && ImGui.IsWindowAppearing() && !ImGui.IsMouseDown(0))
            ImGui.SetKeyboardFocusHere();

        ImGui.PushItemWidth(-style.WindowBorderSize);
        if (ImGui.InputText(string.Empty, ref inputText, 256, ImGuiInputTextFlags.EnterReturnsTrue))
        {
            Submit(inputText);
            inputText = string.Empty;
        }
        ImGui.PopItemWidth();
    }

    public void RegisterCommand(string name, Action<string[]> action)
    {
        commands.Add(name, action);
    }

    public void RegisterHelpDesc(string name, string help)
    {
        commandHelp.Add(name, help);
    }

    public void RegisterAlias(string alias, string command)
    {
        RegisterCommand(alias, args => Submit($"{command} {string.Join(' ', args)}", true));
    }

    public void UnregisterCommand(string name)
    {
        commands.Remove(name);
    }

    public void Write(string text)
    {
        foreach (var c in text)
        {
            Write(c);
        }
    }

    public void Write(char value)
    {
        if (!lines.Any() || lines[^1].EndsWith('\n'))
        {
            lines.Add(value.ToString());
        }
        else
        {
            lines[^1] += value;
        }

        // if (lines.Count >= 2 && lines[^1] == lines[^2])
        // {
        //     lines[^2] += " (x2)";
        //     lines.RemoveAt(lines.Count - 1);
        // }

        if (lines.Count > MaxLines)
        {
            lines.RemoveAt(0);
        }
    }

    public void WriteLine(string text)
    {
        Write(text + Environment.NewLine);
    }

    public void Submit(string command, bool silent = false)
    {
        Debug.Assert(!command.Contains(Environment.NewLine));

        if (!silent)
        {
            lines.Add($"> {command}{Environment.NewLine}");
        }

        var parts = command.Split(' ');

        if (commands.ContainsKey(parts[0]))
        {
            try
            {
                commands[parts[0]](parts[1..]);
            }
            catch (Exception ex)
            {
                lines.Add(ex.Message);
            }
        }
        else
        {
            lines.Add($"Unknown command '{parts[0]}'.{Environment.NewLine}");
        }
    }

    class DebugConsoleWriter : TextWriter
    {
        private DebugConsole console;
        private TextWriter writer;

        public DebugConsoleWriter(DebugConsole console, TextWriter writer)
        {
            this.console = console;
            this.writer = writer;
        }

        public override Encoding Encoding => writer.Encoding;

        public override void Write(char value)
        {
            console.Write(value);
        }
    }
}