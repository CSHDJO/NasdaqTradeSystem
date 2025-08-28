using NasdaqTrader.Bot.Core;
using YurritBot.Logging;

namespace YurritBot
{
    internal interface ITrader
    {
        int IndexToday { get; }
        int IndexReferenceDay { get; }
        ILogger Logger { get; }
        ITraderBot TraderBot { get; }
        ITraderSystemContext SystemContext { get; }

        void ExecuteStrategy();
    }
}