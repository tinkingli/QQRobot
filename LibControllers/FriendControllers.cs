using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Data.Shared;
using Mirai.Net.Sessions.Http.Managers;
using Mirai.Net.Utils.Scaffolds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace App
{
	public static class FriendControllers
	{
		static Random rdm = new Random();

		[FriendMessage]
		public static async void Echo(Friend friend, string content)
		{
			await friend.SendFriendMessageAsync(content);
		}
		[FriendMessage]
		public static async void Task(Friend friend, string content)
		{
			/*var image = new ImageMessage()
			{
				Url = "https://ilikecomix.com/comic/2022/05/Team-Players-6-Alison-Hale.jpg"
			};*/
			var msg = new PlainMessage()
			{
				Text = $"http://www.kcwork.gq/webcn/?task/{content}"
			};
			var builder = new MessageChainBuilder();
			//builder.Append(image);
			builder.Append(msg);
			await friend.SendFriendMessageAsync(builder.Build());
		}
		[FriendMessage]
		public static void Normal(Friend friend, string content)
		{
			Chat(friend, content);
		}
		[FriendMessage("对话")]
		public static async void Chat(Friend friend, string content)
		{
			if (string.IsNullOrEmpty(content))
				return;
			await friend.SendFriendMessageAsync(await HttpHelper.GetRobotReply(content));
		}
	}
}
