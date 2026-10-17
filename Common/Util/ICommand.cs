using Kodnix.Character;

namespace MollyPS.Util
{
    public static class IConsole
    {
        public const string PrefixContent = "> ";
        private const string PinkColor = "\e[38;2;255;192;203m";
        private const string RedColor = "\e[38;2;255;0;0m";
        private const string ResetColor = "\e[0m";

         
        public static string Prefix => $"{(IsCommandValid ? ResetColor : RedColor)}{PrefixContent}{ResetColor}";

        public static bool IsCommandValid { get; private set; } = true;
        private const int HistoryMaxCount = 10;

        public static List<char> Input { get; set; } = [];
        private static int CursorIndex { get; set; }
        private static readonly List<string> InputHistory = [];
        private static int HistoryIndex = -1;

        public static event Action<string>? OnConsoleExcuteCommand;

        public static bool IsInteractive { get; private set; } =
            !Console.IsOutputRedirected && !Console.IsInputRedirected;

        public static void InitConsole()
        {
            if (IsInteractive) Console.Title = "MollyPS Server";
        }

        public static int GetWidth(string str)
            => str.ToCharArray().Sum(EastAsianWidth.GetLength);

        public static void RedrawInput(List<char> input, bool hasPrefix = true)
            => RedrawInput(new string([.. input]), hasPrefix);

        public static void RedrawInput(string input, bool hasPrefix = true)
        {
            if (!IsInteractive) return;
             
            UpdateCommandValidity(input);

            var length = GetWidth(input);
            if (hasPrefix)
            {
                input = Prefix + input;
                length += GetWidth(PrefixContent);
            }

            if (Console.GetCursorPosition().Left > 0)
                Console.SetCursorPosition(0, Console.CursorTop);

            Console.Write(input + new string(' ', Console.BufferWidth - length));

            Console.SetCursorPosition(length, Console.CursorTop);
            CursorIndex = length - GetWidth(PrefixContent);
        }

         
        private static void UpdateCommandValidity(string input)
        {
            IsCommandValid = CheckCommandValid(input);
        }

        #region Handlers

        public static void HandleEnter()
        {
            var input = new string([.. Input]);
            if (string.IsNullOrWhiteSpace(input)) return;

             
            Console.WriteLine();
            Input = [];
            CursorIndex = 0;
            if (InputHistory.Count >= HistoryMaxCount)
                InputHistory.RemoveAt(0);
            InputHistory.Add(input);
            HistoryIndex = InputHistory.Count;

             
            if (input.StartsWith('/')) input = input[1..].Trim();
            OnConsoleExcuteCommand?.Invoke(input);

             
            IsCommandValid = true;
        }

        public static void HandleBackspace()
        {
            if (CursorIndex <= 0) return;
            CursorIndex--;
            var targetWidth = GetWidth(Input[CursorIndex].ToString());
            Input.RemoveAt(CursorIndex);

            var (left, _) = Console.GetCursorPosition();
            Console.SetCursorPosition(left - targetWidth, Console.CursorTop);
            var remain = new string([.. Input.Skip(CursorIndex)]);
            Console.Write(remain + new string(' ', targetWidth));
            Console.SetCursorPosition(left - targetWidth, Console.CursorTop);

             
            var prev = IsCommandValid;
            UpdateCommandValidity(new string([.. Input]));

            if (IsCommandValid != prev)
            {
                RedrawInput(Input);
            }
        }

        public static void HandleUpArrow()
        {
            if (InputHistory.Count == 0) return;
            if (HistoryIndex <= 0) return;

            HistoryIndex--;
            var history = InputHistory[HistoryIndex];
            Input = [.. history];
            CursorIndex = Input.Count;

             
            UpdateCommandValidity(history);
            RedrawInput(Input);
        }

        public static void HandleDownArrow()
        {
            if (HistoryIndex >= InputHistory.Count) return;

            HistoryIndex++;
            if (HistoryIndex >= InputHistory.Count)
            {
                HistoryIndex = InputHistory.Count;
                Input = [];
                CursorIndex = 0;
                IsCommandValid = true;
            }
            else
            {
                var history = InputHistory[HistoryIndex];
                Input = [.. history];
                CursorIndex = Input.Count;
                 
                UpdateCommandValidity(history);
            }
            RedrawInput(Input);
        }

        public static void HandleLeftArrow()
        {
            if (CursorIndex <= 0) return;

            var (left, _) = Console.GetCursorPosition();
            CursorIndex--;
            Console.SetCursorPosition(left - GetWidth(Input[CursorIndex].ToString()), Console.CursorTop);
        }

        public static void HandleRightArrow()
        {
            if (CursorIndex >= Input.Count) return;

            var (left, _) = Console.GetCursorPosition();
            CursorIndex++;
            Console.SetCursorPosition(left + GetWidth(Input[CursorIndex - 1].ToString()), Console.CursorTop);
        }

        public static void HandleInput(ConsoleKeyInfo keyInfo)
        {
            if (char.IsControl(keyInfo.KeyChar)) return;
            var newWidth = GetWidth(new string([.. Input])) + GetWidth(keyInfo.KeyChar.ToString());
            if (newWidth >= (Console.BufferWidth - GetWidth(PrefixContent))) return;
            HandleInput(keyInfo.KeyChar);
        }

        public static void HandleInput(char keyChar)
        {
            Input.Insert(CursorIndex, keyChar);
            CursorIndex++;

            var (left, _) = Console.GetCursorPosition();
            var newCursor = left + GetWidth(keyChar.ToString());
            if (newCursor > Console.BufferWidth - 1) newCursor = Console.BufferWidth - 1;

            Console.Write(new string([.. Input.Skip(CursorIndex - 1)]));
            Console.SetCursorPosition(newCursor, Console.CursorTop);

             
            var prev = IsCommandValid;
            UpdateCommandValidity(new string([.. Input]));

            if (IsCommandValid != prev)
            {
                RedrawInput(Input);
            }
        }

        #endregion

        public static string ListenConsole()
        {
            if (Console.IsInputRedirected)
            {
                while (Console.ReadLine() is { } command)
                    OnConsoleExcuteCommand?.Invoke(command);
                return "";
            }
            while (true)
            {
                ConsoleKeyInfo keyInfo;
                try { keyInfo = Console.ReadKey(true); }
                catch (InvalidOperationException) { continue; }

                switch (keyInfo.Key)
                {
                    case ConsoleKey.Enter:
                        HandleEnter();
                        break;
                    case ConsoleKey.Backspace:
                        HandleBackspace();
                        break;
                    case ConsoleKey.LeftArrow:
                        HandleLeftArrow();
                        break;
                    case ConsoleKey.RightArrow:
                        HandleRightArrow();
                        break;
                    case ConsoleKey.UpArrow:
                        HandleUpArrow();
                        break;
                    case ConsoleKey.DownArrow:
                        HandleDownArrow();
                        break;
                    default:
                        HandleInput(keyInfo);
                        break;
                }
            }
        }

        private static bool CheckCommandValid(string input)
        {
            if (string.IsNullOrEmpty(input))
                return true;

            var invalidChars = new[] { '@', '#', '$', '%', '&', '*' };
            return !invalidChars.Any(c => input.Contains(c));
        }
    }

}
