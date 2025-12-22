using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Shiko
{
    // Log messages from the inner library to the Unity console
    public static class UnityConsole
    {
        private class UnityTextWriter : TextWriter
        {
            private StringBuilder buffer = new StringBuilder();

            public override void Flush()
            {
                string message = buffer.ToString().Trim();

                // Empty
                if (string.IsNullOrEmpty(message))
                    return;

                // Log with warning
                if (message.Contains("[WARN]"))
                    Debug.LogWarning(message);
                // Log with error
                else if (message.Contains("[ERROR]") || message.Contains("[FATAL]"))
                    Debug.LogError(message);
                // Log with debug
                else
                    Debug.Log(message);

                buffer.Length = 0;
            }

            public override void Write(string value)
            {
                buffer.Append(value);
                if (value != null)
                {
                    var len = value.Length;
                    if (len > 0)
                    {
                        var lastChar = value[len - 1];
                        if (lastChar == '\n')
                        {
                            Flush();
                        }
                    }
                }
            }

            public override void Write(char value)
            {
                buffer.Append(value);
                if (value == '\n')
                {
                    Flush();
                }
            }

            public override void Write(char[] value, int index, int count)
            {
                Write(new string(value, index, count));
            }

            public override Encoding Encoding
            {
                get { return Encoding.Default; }
            }
        }

        public static void Redirect()
        {
            Console.SetOut(new UnityTextWriter());
        }
    }
}
