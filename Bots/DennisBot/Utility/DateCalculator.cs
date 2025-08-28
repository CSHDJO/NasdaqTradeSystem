namespace DennisMoneyBot.Utility;
internal static class DateCalculator
{
    public static DateOnly GetNextValidDate(DateOnly currentDate)
    {
        DateOnly nextDate = currentDate.AddDays(1);
        while (nextDate.DayOfWeek == DayOfWeek.Saturday || nextDate.DayOfWeek == DayOfWeek.Sunday || nextDate.IsFederalHoliday())
        {
            nextDate = nextDate.AddDays(1);
        }
        return nextDate;
    }

    public static DateOnly GetBusinessDaysInTheFuture(DateOnly currentDate, int amountOfDays)
    {
        var nextDay = currentDate;
        for (int i = 0; i < amountOfDays; i++)
        {
            nextDay = GetNextValidDate(nextDay);
        }
        return nextDay;
    }
}
