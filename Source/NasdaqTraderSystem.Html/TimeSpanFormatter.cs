using HandlebarsDotNet;
using HandlebarsDotNet.IO;

namespace NasdaqTraderSystem.Html;
internal class TimeSpanFormatter(string format) : IFormatter, IFormatterProvider
{
    public void Format<T>(T value, in EncodedTextWriter writer)
    {
        if (value is not TimeSpan timeSpan)
            throw new ArgumentException("TimeSpan expected", nameof(value));

        writer.Write(timeSpan.ToString(format));
    }

    public object? GetFormat(Type? formatType)
    {
        throw new NotImplementedException();
    }

    public bool TryCreateFormatter(Type type, out IFormatter formatter)
    {
        if (type != typeof(TimeSpan))
        {
            formatter = null;
            return false;
        }

        formatter = this;
        return true;
    }
}
