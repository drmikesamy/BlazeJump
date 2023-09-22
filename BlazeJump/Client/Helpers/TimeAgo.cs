namespace BlazeJump.Helpers
{
    public static class TimeAgo
    {
        private static Dictionary<double, Func<double, string>> timeSpansDictionary = null;
        private static Dictionary<double, Func<double, string>> SetupTimeSpansDictionary()
        {
            var timeSpansDictionary = new Dictionary<double, Func<double, string>>();
            timeSpansDictionary.Add(0, (mins) => "just now");
            timeSpansDictionary.Add(1, (mins) => "1 min ago");
            timeSpansDictionary.Add(60, (mins) => string.Format("{0} mins ago", Math.Round(mins)));
            timeSpansDictionary.Add(120, (mins) => "1 hr ago");
            timeSpansDictionary.Add(1440, (mins) => string.Format("{0} hrs ago", Math.Round(mins / 60)));
            timeSpansDictionary.Add(2880, (mins) => "1 day ago");
            return timeSpansDictionary;
        }
        public static string GetTimeAgo(DateTime date)
        {
            TimeSpan ts = DateTime.UtcNow.Subtract(date);
            double minutesElapsed = ts.TotalMinutes;
            if(timeSpansDictionary == null)
            {
                timeSpansDictionary = SetupTimeSpansDictionary();
            }
            if(minutesElapsed < 2880)
            {
                return timeSpansDictionary.First(n => minutesElapsed < n.Key).Value.Invoke(minutesElapsed);
            }
            return date.ToString("dddd, dd MMMM yyyy h:mm tt");
        }
    }
}
