using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FancyZonesHotkeys.Interop
{
    public class KeyboardHook : IDisposable
    {
        private class Window : NativeWindow, IDisposable
        {
            private static int WM_HOTKEY = 0x0312;

            public Window()
            {
                CreateHandle(new CreateParams());
            }

            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);
                if (m.Msg == WM_HOTKEY)
                {
                    Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                    ModifierKeys modifier = (ModifierKeys)((int)m.LParam & 0xFFFF);
                    if (KeyPressed != null)
                    {
                        KeyPressed(this, new KeyPressedEventArgs(modifier, key));
                    }
                }
            }

            public event EventHandler<KeyPressedEventArgs>? KeyPressed;

            public void Dispose()
            {
                DestroyHandle();
            }
        }

        private Window _window = new Window();
        private int _currentId;

        public KeyboardHook()
        {
            _window.KeyPressed += delegate (object? sender, KeyPressedEventArgs args)
            {
                if (KeyPressed != null)
                {
                    KeyPressed(this, args);
                }
            };
        }

        public void RegisterHotKey(ModifierKeys modifier, Keys key)
        {
            _currentId++;
            if (!NativeMethods.RegisterHotKey(_window.Handle, _currentId, (uint)modifier, (uint)key))
            {
                throw new InvalidOperationException("Couldn’t register the hot key.");
            }
        }

        public event EventHandler<KeyPressedEventArgs>? KeyPressed;

        public void Dispose()
        {
            for (int i = _currentId; i > 0; i--)
            {
                NativeMethods.UnregisterHotKey(_window.Handle, i);
            }
            _window.Dispose();
        }
    }

    public class KeyPressedEventArgs : EventArgs
    {
        public ModifierKeys Modifier { get; }
        public Keys Key { get; }

        internal KeyPressedEventArgs(ModifierKeys modifier, Keys key)
        {
            Modifier = modifier;
            Key = key;
        }
    }

    [Flags]
    public enum ModifierKeys : uint
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        Win = 8
    }
}
