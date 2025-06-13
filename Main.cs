using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;

namespace SimpleBroadcast
{
    public class TimedMessage
    {
        public string Text { get; set; } = string.Empty;
        public int Interval { get; set; }
    }

    public class Main : BasePlugin
    {
        public override string ModuleName => "SimpleBroadcast";
        public override string ModuleVersion => "1.0.0";
        public override string ModuleAuthor => "cut1ness";
        public override string ModuleDescription => "Broadcast system!";

        private List<TimedMessage> _messages = new List<TimedMessage>();
        private int _currentMessageIndex = 0;
        private System.Timers.Timer? _messageTimer;
        private int _globalInterval = 30;

        public override void Load(bool hotReload)
        {
            string pluginFolderPath = ModuleDirectory;
            string filePath = Path.Combine(pluginFolderPath, "texts.txt");

            if (File.Exists(filePath))
            {
                LoadMessages(filePath);
                Console.WriteLine("[SimpleBroadcast] Plugin loaded!");
            }
            else
            {
                Console.WriteLine($"[TimedMessages] File '{filePath}' not found. Example file created, restart your server.");
                MakeExampleFile(filePath);
            }

            if (_messages.Any())
            {
                _currentMessageIndex = 0;

                AddTimer(_globalInterval, () => {
                    var messageToShow = _messages[_currentMessageIndex];

                    _currentMessageIndex = (_currentMessageIndex + 1) % _messages.Count;

                    Server.PrintToChatAll(messageToShow.Text);
                }, TimerFlags.REPEAT);
            }

            else
            {
                Console.WriteLine("[TimedMessages] There are no messages to display. Timer has not started.");
            }
        }

        public override void Unload(bool hotReload)
        {
            Console.WriteLine("[SimpleBroadcast] Plugin unloaded!");
        }

        private void LoadMessages(string filePath)
        {
            _messages.Clear();
            _globalInterval = 30;

            try
            {
                string[] lines = File.ReadAllLines(filePath);
                if (lines.Length == 0)
                {
                    Console.WriteLine($"[SimpleBroadcast] Файл '{filePath}' пуст.");
                    return;
                }

                string firstLine = lines[0].Trim();
                if (!string.IsNullOrWhiteSpace(firstLine) && firstLine.StartsWith("\"time\""))
                {
                    int firstQuoteIndex = firstLine.IndexOf('"');
                    int lastQuoteIndex = firstLine.LastIndexOf('"');

                    if (firstQuoteIndex != -1 && lastQuoteIndex != -1 && firstQuoteIndex < lastQuoteIndex)
                    {
                        string timeValuePart = firstLine.Substring(lastQuoteIndex + 1).TrimStart(',', ' ').Trim('"');
                        if (int.TryParse(timeValuePart, out int parsedInterval) && parsedInterval > 0)
                        {
                            _globalInterval = parsedInterval;
                            Console.WriteLine($"[SimpleBroadcast] The global message interval is set to: {_globalInterval} seconds.");
                        }

                        else
                        {
                            Console.WriteLine($"[SimpleBroadcast] Error parsing global interval in first line: '{firstLine}'. Using default interval ({_globalInterval}s).");
                        }
                    }

                    else
                    {
                        Console.WriteLine($"[SimpleBroadcast] Invalid first line format: '{firstLine}'. Expecting \"time\", \"interval\". Using default interval ({_globalInterval}s).");
                    }

                    lines = lines.Skip(1).ToArray();
                }

                else
                {
                    Console.WriteLine($"[SimpleBroadcast] The first line is not a global interval or has an invalid format. Using the default interval ({_globalInterval}s).");
                }

                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();
                    if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("//"))
                    {
                        continue;
                    }

                    int firstTextQuoteIndex = trimmedLine.IndexOf('"');
                    int lastTextQuoteIndex = trimmedLine.IndexOf('"', firstTextQuoteIndex + 1);

                    if (firstTextQuoteIndex == 0 && lastTextQuoteIndex > 0)
                    {
                        string textPart = trimmedLine.Substring(firstTextQuoteIndex + 1, lastTextQuoteIndex - firstTextQuoteIndex - 1);
                        _messages.Add(new TimedMessage { Text = textPart });
                    }

                    else
                    {
                        Console.WriteLine($"[SimpleBroadcast] Invalid message format at line: '{line}'. Expecting \"text\".");
                    }
                }
                Console.WriteLine($"[SimpleBroadcast] Loaded {_messages.Count} messages from '{filePath}'.");
            }

            catch (Exception ex)
            {
                Console.WriteLine($"[SimpleBroadcast] Error reading file '{filePath}': {ex.Message}");
            }
        }

        private void MakeExampleFile(string filePath)
        {
            try
            {
                string[] exampleContent = new string[]
                {
                    "\"time\", \"30\"",
                    "// Example of config file. Upper, you can edit interval between messages",
                    "// Next lines will contain a text for messages, like:",
                    "\"Hello World!\"",
                    "\"Testing!\"",
                };
                File.WriteAllLines(filePath, exampleContent);
            }

            catch (Exception ex)
            {
                Console.WriteLine($"[SimpleBroadcast] Error creating file '{filePath}': {ex.Message}");
            }
        }
    }
}
