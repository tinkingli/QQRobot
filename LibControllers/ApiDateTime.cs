using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App
{
	public static class ApiDateTime
	{
		private static readonly DateTime time_begin19700101 = new DateTime(1970, 1, 1);
		public static long MillionSecondsFromBegin()
		{
			return (DateTime.Now - time_begin19700101).Ticks / 10000;
		}
		public static long Day0SecondsFromBegin()
		{
			return SecondsFromBegin() / DaySecond * DaySecond;
		}
		public static long SecondsFromBegin()
		{
			return MillionSecondsFromBegin() / 1000;
		}
		public static DateTime ToTime(long sec)
		{
			return time_begin19700101.AddSeconds(sec);
		}
		public const long DaySecond = 24 * 3600;
		public static bool IsToday(long time)
		{
			return time / DaySecond == SecondsFromBegin() / DaySecond;
		}
	}
}
