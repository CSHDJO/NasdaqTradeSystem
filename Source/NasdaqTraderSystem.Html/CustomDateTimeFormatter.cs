using HandlebarsDotNet;
using HandlebarsDotNet.IO;

namespace NasdaqTraderSystem.Html;

public sealed class CustomDateTimeFormatter : IFormatter, IFormatterProvider
{
    private readonly string _format;

    public CustomDateTimeFormatter(string format) => _format = format;

    public void Format<T>(T value, in EncodedTextWriter writer)
    {
        if (value is DateOnly dateOnly)
        {
            writer.Write($"{dateOnly.ToString(_format)}");
            return;
        }


        if (!(value is DateTime dateTime))
            throw new ArgumentException("supposed to be DateTime");

        writer.Write($"{dateTime.ToString(_format)}");
    }

    public bool TryCreateFormatter(Type type, out IFormatter formatter)
    {
        if (type == typeof(DateTime))
        {
            formatter = this;
            return true;
        }

        if (type == typeof(DateOnly))
        {
            formatter = this;
            return true;
        }

        formatter = null;
        return false;
    }
}