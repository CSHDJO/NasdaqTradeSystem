using HandlebarsDotNet;
using HandlebarsDotNet.IO;

namespace NasdaqTraderSystem.Html;

public class DecimalFormatter: IFormatter, IFormatterProvider
{
    private readonly string _format;


    public void Format<T>(T value, in EncodedTextWriter writer)
    {
        if(!(value is decimal decimalValue)) 
            throw new ArgumentException("decimal");
        
        writer.Write($"{decimalValue:C2}");
    }

    public bool TryCreateFormatter(Type type, out IFormatter formatter)
    {
        if (type != typeof(decimal))
        {
            formatter = null;
            return false;
        }

        formatter = this;
        return true;
    }
}