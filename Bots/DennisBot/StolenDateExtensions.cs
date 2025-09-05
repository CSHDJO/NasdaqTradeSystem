namespace DennisMoneyBot;
internal static class StolenDateExtensions
{
    /// <summary>
    /// Determines if this date is a federal holiday.
    /// </summary>
    /// <param name="date">This date</param>
    /// <returns>True if this date is a federal holiday</returns>
    public static bool IsFederalHoliday(this DateOnly date)
    {
        // to ease typing
        int nthWeekDay = (int)(Math.Ceiling((double)date.Day / 7.0d));
        DayOfWeek dayName = date.DayOfWeek;
        bool isThursday = dayName == DayOfWeek.Thursday;
        bool isFriday = dayName == DayOfWeek.Friday;
        bool isMonday = dayName == DayOfWeek.Monday;
        bool isWeekend = dayName == DayOfWeek.Saturday || dayName == DayOfWeek.Sunday;


        //Junteeth
        if (new DateOnly(date.Year, 6, 19) == date) return true;
        //good friday
        if (DateOnly.FromDateTime(EasterSunday(date.Year)).AddDays(-2) == date) return true;

        // New Years Day (Jan 1, or preceding Friday/following Monday if weekend)
        if ((date.Month == 12 && date.Day == 31 && isFriday) ||
            (date.Month == 1 && date.Day == 1 && !isWeekend) ||
            (date.Month == 1 && date.Day == 2 && isMonday)) return true;

        // MLK day (3rd monday in January)
        if (date.Month == 1 && isMonday && nthWeekDay == 3) return true;

        // President’s Day (3rd Monday in February)
        if (date.Month == 2 && isMonday && nthWeekDay == 3) return true;

        // Memorial Day (Last Monday in May)
        if (date.Month == 5 && isMonday && date.AddDays(7).Month == 6) return true;

        // Independence Day (July 4, or preceding Friday/following Monday if weekend)
        if ((date.Month == 7 && date.Day == 3 && isFriday) ||
            (date.Month == 7 && date.Day == 4 && !isWeekend) ||
            (date.Month == 7 && date.Day == 5 && isMonday)) return true;

        // Labor Day (1st Monday in September)
        if (date.Month == 9 && isMonday && nthWeekDay == 1) return true;

        // Columbus Day (2nd Monday in October)
        if (date.Month == 10 && isMonday && nthWeekDay == 2) return true;

        // Veteran’s Day (November 11, or preceding Friday/following Monday if weekend))
        if ((date.Month == 11 && date.Day == 10 && isFriday) ||
            (date.Month == 11 && date.Day == 11 && !isWeekend) ||
            (date.Month == 11 && date.Day == 12 && isMonday)) return true;

        // Thanksgiving Day (4th Thursday in November)
        if (date.Month == 11 && isThursday && nthWeekDay == 4) return true;

        // Christmas Day (December 25, or preceding Friday/following Monday if weekend))
        if ((date.Month == 12 && date.Day == 24 && isFriday) ||
            (date.Month == 12 && date.Day == 25 && !isWeekend) ||
            (date.Month == 12 && date.Day == 26 && isMonday)) return true;

        return false;
    }

    public static DateTime EasterSunday(int year)
    {
        int day = 0;
        int month = 0;

        int g = year % 19;
        int c = year / 100;
        int h = (c - (int)(c / 4) - (int)((8 * c + 13) / 25) + 19 * g + 15) % 30;
        int i = h - (int)(h / 28) * (1 - (int)(h / 28) * (int)(29 / (h + 1)) * (int)((21 - g) / 11));

        day = i - ((year + (int)(year / 4) + i + 2 - c + (int)(c / 4)) % 7) + 28;
        month = 3;

        if (day > 31)
        {
            month++;
            day -= 31;
        }

        return new DateTime(year, month, day);
    }
}
