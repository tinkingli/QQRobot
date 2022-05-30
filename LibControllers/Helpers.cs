using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Utils.Scaffolds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace App
{
	public static class ExceptionHandler
	{
		private static bool Caught;
		public static void CatchUnhandle()
		{
			if (Caught)
				return;
			Caught = true;
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
			{
				Console.WriteLine($"[CurrentDomain_UnhandledException]{e.ExceptionObject}");
			}
			TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
			void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
			{
				Console.WriteLine($"[TaskScheduler_UnobservedTaskException]{e.Exception}");
			}

		}
	}
	public static class HttpHelper
	{
		public static async Task<string> GetRobotReply(string content)
		{
			var hc = new HttpClient();
			var resp = await hc.GetAsync($"http://api.qingyunke.com/api.php?key=free&appid=0&msg={content}");
			var res = await resp.Content.ReadAsStringAsync();
			var j = JsonDocument.Parse(res);
			return j.RootElement.GetProperty("content").GetString().Replace("{br}", "\n");
		}
		public static async Task<string> GetHttpReq(string url)
		{
			var hc = new HttpClient();
			var resp = await hc.GetAsync(url);
			return await resp.Content.ReadAsStringAsync();
		}

		private static async Task<string> ImageGetHttpReq(string v)
		{
			var hc = new HttpClient();
			var resp = await hc.GetAsync(v);
			return System.Convert.ToBase64String(await resp.Content.ReadAsByteArrayAsync());
		}
	}
	public static class RandomHelper
	{
		public static System.Random random = new Random();
		public static int Next(int maxValue)
		{
			return random.Next(maxValue);
		}
		public static int Next(int minValue, int maxValue)
		{
			return random.Next(minValue, maxValue);
		}
	}
}
