using System;

namespace Postbus.Client.IO
{
    class OutputWriter
    {
        public void WriteLine() => Console.WriteLine();

        public void Write(string message) => Console.Write(message);

        public void Clear() => Console.Clear();

        public void WriteLine(string message) => Console.WriteLine(message);
    }
}