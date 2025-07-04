using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HannahBot;
internal class Logger
{
    public static void Log(string message)
    {
#if DEBUG
        Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
#endif
    }
}
