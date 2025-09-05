using NasdaqTrader.Bot.Core;

namespace YurritBot
{
    public class DateCalculator
    {
        public int CalculateBusinessDaysBetween(DateOnly startDate, DateOnly endDate)
        {
            DateTime startDateTime = startDate.ToDateTime(TimeOnly.MinValue);
            DateTime endDateTime = endDate.ToDateTime(TimeOnly.MinValue);

            int totalDays = (endDateTime - startDateTime).Days;
            if (totalDays < 0)
            {
                return 0;
            }

            int businessDays = 0;
            for (int i = 0; i < totalDays; i++)
            {
                DateTime currentDate = startDateTime.AddDays(i);
                if (currentDate.DayOfWeek != DayOfWeek.Saturday &&
                    currentDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    businessDays++;
                }
            }

            return businessDays;
        }

        public int DetermineDateIndex(DateOnly date, IEnumerable<IPricePoint> pricePoints)
            => pricePoints.Select((pricePoint, index) => (pricePoint, index))
                .First(pointIndexPair => pointIndexPair.pricePoint.Date.Equals(date))
                .index;
    }
}
