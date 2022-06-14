using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Sessions.Http.Managers;
using Mirai.Net.Utils.Scaffolds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App
{
	public static class GroupHelper
	{
		private const string groupfile = "groups.txt";
		private static List<string> lValidGroups;
		public static bool Invalid(string groupid)
		{
			if (lValidGroups == null)
			{
				if (!File.Exists(groupfile))
					return true;
				lValidGroups = File.ReadAllLines(groupfile).ToList();
			}
			return !lValidGroups.Contains(groupid);
		}
		private const string setufile = "setus.txt";
		private static List<string> lValidSetus;
		public static bool InvalidSetu(string groupid)
		{
			if (lValidSetus == null)
			{
				if (!File.Exists(setufile))
					return true;
				lValidSetus = File.ReadAllLines(setufile).ToList();
			}
			return !lValidSetus.Contains(groupid);
		}

		private static string[] files = new string[]
		{
			"gifs",
		};
		private static Dictionary<string, List<string>> _dTagFiles;
		private static Dictionary<string, List<string>> dTagFiles
		{
			get
			{
				if (_dTagFiles == null)
				{
					_dTagFiles = new Dictionary<string, List<string>>();
					foreach (var file in files)
					{
						var f = file;
						if (!f.EndsWith(".txt"))
							f = f + ".txt";

						if (!File.Exists(f))
						{
							Console.WriteLine($"File {f} not exist.");
							continue;
						}
						_dTagFiles[f] = File.ReadAllLines(f).ToList();
					}
					Console.WriteLine($"Loaded {_dTagFiles.Count} files");
				}
				return _dTagFiles;
			}
		}
		public static string GetTaged(string f)
		{
			if (!f.EndsWith(".txt"))
				f = f + ".txt";
			if (!dTagFiles.ContainsKey(f))
				return "";
			var fs = dTagFiles[f];
			return fs[RandomHelper.random.Next(fs.Count)];
		}

		private const string atfile = "ats.txt";
		private static List<string> lValidAts;
		public static bool ValidAt(string groupid)
		{
			if (lValidAts == null)
			{
				if (!File.Exists(atfile))
					return false;
				lValidAts = File.ReadAllLines(atfile).ToList();
			}
			return lValidAts.Contains(groupid);
		}

		private static Dictionary<string, string> lRobotReply;
		private static string robotreplyfile = "robotreply.txt";
		public static string GetRobotReply(string keyword)
		{
			if (lRobotReply == null)
			{
				lRobotReply = new Dictionary<string, string>();
				if (!File.Exists(robotreplyfile))
					return "";
				var all = File.ReadAllLines(robotreplyfile);
				foreach (var line in all)
				{
					var aline = line.Split(new char[] { '=' }, 2);
					if (aline.Length != 2)
						continue;
					lRobotReply[aline[0]] = aline[1];
				}
			}
			if (lRobotReply.ContainsKey(keyword))
				return lRobotReply[keyword];
			return "";
		}
		public static async Task OnSendMessage(this Mirai.Net.Data.Shared.Group g, string content)
		{
			await SendMessage(g.Id, content);
		}
		public static async Task SendMessage(this Mirai.Net.Data.Shared.Group g, MessageChain content)
		{
			try
			{
				await MessageManager.SendGroupMessageAsync(g.Id, content);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[SendMessageFailed]{ex}");
			}
		}

		public static async Task SendMessage(string gid, string content)
		{
			try
			{
				await MessageManager.SendGroupMessageAsync(gid, content);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[SendMessageFailed]{ex}");
			}
		}
		public static async Task SendMessage(this Mirai.Net.Data.Shared.Group g, List<string> lprocess)
		{
			await SendMessage(g.Id, lprocess);
		}
		public static async Task SendMessage(string gid, List<string> lprocess)
		{
			try
			{
				var b = new MessageChainBuilder();
				foreach (var pro in lprocess)
					b.Append(new PlainMessage() { Text = pro + "\n" });
				await MessageManager.SendGroupMessageAsync(gid, b.Build());
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[SendMessageFailed]{ex}");
			}
		}
	}
}
