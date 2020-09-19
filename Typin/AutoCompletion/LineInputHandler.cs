﻿namespace Typin.AutoCompletion
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Typin.Console;
    using Typin.Exceptions;

    internal sealed class LineInputHandler
    {
        private readonly IConsole _console;
        private readonly KeyHandler _keyHandler;
        private readonly HashSet<ShortcutDefinition> _shortcuts;
        private readonly StringBuilder _text = new StringBuilder();
        private bool _isReading;

        /// <summary>
        /// Cursor position relative to input.
        /// </summary>
        public int CursorPosition { get; private set; }

        public bool IsStartOfLine => CursorPosition == 0;
        public bool IsEndOfLine => CursorPosition == _text.Length;
        public bool IsEndOfBuffer => _console.CursorLeft == _console.BufferWidth - 1;

        /// <summary>
        /// Current input text.
        /// </summary>
        public string Text => _text.ToString();

        #region ctor
        /// <summary>
        /// Initializes an instance of <see cref="LineInputHandler"/>.
        /// </summary>
        public LineInputHandler(IConsole console)
        {
            _console = console;

            _shortcuts = new HashSet<ShortcutDefinition>
            {
                new ShortcutDefinition(ConsoleKey.LeftArrow, () => MoveCursorLeft()),
                new ShortcutDefinition(ConsoleKey.RightArrow, MoveCursorRight),
                new ShortcutDefinition(ConsoleKey.Home, () =>
                {
                    while (!IsStartOfLine)
                        MoveCursorLeft();
                }),
                new ShortcutDefinition(ConsoleKey.End, () =>
                {
                    while (!IsEndOfLine)
                        MoveCursorRight();
                }),
                new ShortcutDefinition(ConsoleKey.Backspace, () => Backspace()),
                new ShortcutDefinition(ConsoleKey.Delete, Delete),
                new ShortcutDefinition(ConsoleKey.Insert, () => { }),
                new ShortcutDefinition(ConsoleKey.Escape, ClearLine),

                new ShortcutDefinition(ConsoleKey.LeftArrow, ConsoleModifiers.Control, () => DoUntilPrevWordOrWhitespace(() => MoveCursorLeft())),
                new ShortcutDefinition(ConsoleKey.RightArrow, ConsoleModifiers.Control, () => DoUntilNextWordOrWhitespace(MoveCursorRight)),
                new ShortcutDefinition(ConsoleKey.Backspace, ConsoleModifiers.Control, BackspacePrevWord),
                new ShortcutDefinition(ConsoleKey.Delete, ConsoleModifiers.Control, () => DoUntilPrevWordOrWhitespace(Delete))
            };

            _keyHandler = new KeyHandler(console, _shortcuts);
            _keyHandler.UnhandledControlSequenceDetected += ControlSequenceDetected;
            _keyHandler.UnhandledKeyDetected += UnhandledKeyDetected;
            _keyHandler.NewLineDetected += NewLineDetected;
        }

        /// <summary>
        /// Initializes an instance of <see cref="LineInputHandler"/>.
        /// </summary>
        public LineInputHandler(IConsole console,
                                HashSet<ShortcutDefinition> internalShortcuts,
                                HashSet<ShortcutDefinition>? userDefinedShortcut = null) :
            this(console)
        {
            //TODO: maybe hashset is not the best collection
            //_shortcuts.Union(internalShortcuts);
            foreach (ShortcutDefinition shortcut in internalShortcuts)
            {
                if (!_shortcuts.Add(shortcut))
                {
                    //Replace when already exists
                    _shortcuts.Remove(shortcut);
                    _shortcuts.Add(shortcut);
                }
            }

            if (userDefinedShortcut != null)
            {
                //_shortcuts.Union(userDefinedShortcut);
                foreach (ShortcutDefinition shortcut in userDefinedShortcut)
                {
                    if (!_shortcuts.Add(shortcut))
                    {
                        //Throw an error when already exists
                        throw TypinException.DuplicatedShortcut(shortcut);
                    }
                }
            }
        }
        #endregion

        #region KeyHandler callbacks
        private void NewLineDetected()
        {
            _isReading = false;
        }

        private void UnhandledKeyDetected(ref ConsoleKeyInfo keyInfo)
        {
            Write(keyInfo.KeyChar);
        }

        private void ControlSequenceDetected(ref ConsoleKeyInfo keyInfo)
        {
            Write('^');
            Write(keyInfo.Key.ToString());
        }
        #endregion

        #region ReadLine
        /// <summary>
        /// Reads a line from input.
        /// </summary>
        public string ReadLine()
        {
            _isReading = true;
            do
            {
                _keyHandler.ReadKey();
            } while (_isReading);

            string text = Text.TrimEnd('\n', '\r');
            _console.Output.WriteLine();
            Reset();

            return text;
        }

        /// <summary>
        /// Reads a line from array.
        /// </summary>
        public string ReadLine(params ConsoleKeyInfo[] line)
        {
            //TODO: To fix
            _isReading = true;

            int i = 0;
            for (; i < line.Length && _isReading; ++i)
            {
                ConsoleKeyInfo keyInfo = line[i];
                _keyHandler.ReadKey(keyInfo);
            }
            _isReading = false;

            //if (line.ElementAtOrDefault(i - 1).Key != ConsoleKey.Enter)
            //    _keyHandler.ReadKey(ConsoleKeyInfoExtensions.Enter);

            string text = Text.TrimEnd('\n', '\r');
            _console.Output.WriteLine();
            Reset();

            return text;
        }
        #endregion

        /// <summary>
        /// Resets key handler to allow proper process of next line.
        /// </summary>
        private void Reset()
        {
            CursorPosition = 0;
            _text.Clear();
        }

        //TODO: rewrite: must work when line is wrapped
        private void MoveCursorLeft(int count = 1)
        {
            if (CursorPosition < count)
                count = CursorPosition;

            if (_console.CursorLeft < count)
                _console.SetCursorPosition(_console.BufferWidth - 1, _console.CursorTop - 1);
            else
                _console.SetCursorPosition(_console.CursorLeft - count, _console.CursorTop);

            CursorPosition -= count;
        }

        private void MoveCursorRight()
        {
            if (IsEndOfLine)
                return;

            if (IsEndOfBuffer)
                _console.SetCursorPosition(0, _console.CursorTop + 1);
            else
                _console.SetCursorPosition(_console.CursorLeft + 1, _console.CursorTop);

            ++CursorPosition;
        }

        public void ClearLine()
        {
            while (!IsStartOfLine)
                Backspace();

            _text.Clear();
        }

        public void Write(string str)
        {
            foreach (char character in str)
                Write(character);
        }

        public void Write(char c)
        {
            if (IsEndOfLine)
            {
                _text.Append(c);
                _console.Output.Write(c.ToString());
                CursorPosition++;
            }
            else
            {
                int left = _console.CursorLeft;
                int top = _console.CursorTop;
                string str = _text.ToString().Substring(CursorPosition);
                _text.Insert(CursorPosition, c);
                _console.Output.Write(c.ToString() + str);
                _console.SetCursorPosition(left, top);
                MoveCursorRight();
            }
        }
        public void Backspace(int count = 1)
        {
            for (; count > 0; --count)
            {
                if (CursorPosition == 0)
                    return;

                MoveCursorLeft(1);
                int index = CursorPosition;
                _text.Remove(index, 1);

                string replacement = _text.ToString().Substring(index);
                int left = _console.CursorLeft;
                int top = _console.CursorTop;

                string spaces = new string(' ', 1);
                _console.Output.Write(string.Format("{0}{1}", replacement, spaces));
                _console.SetCursorPosition(left, top);
            }
        }

        public void BackspacePrevWord()
        {
            DoUntilPrevWordOrWhitespace(() => Backspace());
        }

        private void Delete()
        {
            if (IsEndOfLine)
                return;

            int index = CursorPosition;
            _text.Remove(index, 1);

            string replacement = _text.ToString().Substring(index);
            int left = _console.CursorLeft;
            int top = _console.CursorTop;
            _console.Output.Write(string.Format("{0} ", replacement));
            _console.SetCursorPosition(left, top);
        }

        private void DoUntilPrevWordOrWhitespace(Action action)
        {
            int v = CursorPosition - 1;
            if (v < 0)
                return;

            if (char.IsWhiteSpace(_text[v]))
            {
                do
                {
                    action();
                }
                while (!IsStartOfLine && char.IsWhiteSpace(_text[CursorPosition - 1]));

                return;
            }

            do
            {
                action();
            }
            while (!IsStartOfLine && !char.IsWhiteSpace(_text[CursorPosition - 1]));
        }

        private void DoUntilNextWordOrWhitespace(Action action)
        {
            if (IsEndOfLine)
                return;

            if (char.IsWhiteSpace(_text[CursorPosition]))
            {
                do
                {
                    action();
                }
                while (!IsEndOfLine && char.IsWhiteSpace(_text[CursorPosition]));

                return;
            }

            do
            {
                action();
            }
            while (!IsEndOfLine && !char.IsWhiteSpace(_text[CursorPosition]));
        }
    }
}
