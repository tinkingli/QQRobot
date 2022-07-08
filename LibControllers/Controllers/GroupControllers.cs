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
	public static class GroupControllers
	{
		static Random rdm = new Random();

		[GroupMessage("入群")]
		public static async void JoinGroup(Group group, Member Member)
		{
			if (GroupHelper.Invalid(group.Id))
				return;
			if (!GroupHelper.ValidAt(group.Id))
				return;
			Console.WriteLine($"[JoinGroup]{Member.Name}-{Member.Id}");
			var at = new AtMessage()
			{
				Target = Member.Id,
				Type = Messages.At
			};
			var msg = new PlainMessage()
			{
				Text = @"【快到碗裡來】欢迎您来到【huatuo c#热更新】大家庭
下面的信息可能对你有用
项目地址：
解释器：https://github.com/focus-creative-games/huatuo
IL2CPP：https://github.com/pirunxi/il2cpp_huatuo
示例：https://github.com/focus-creative-games/huatuo_trial
请注意Unity版本和IL2CPP版本哦
教程：
图文版：https://zhuanlan.zhihu.com/p/513834841
视频版：https://www.bilibili.com/video/BV13a411a7SQ
工具地址：
1：https://github.com/focus-creative-games/huatuo_upm",
			};
			var b = new MessageChainBuilder();
			b.Append(at).Append(msg);
			await group.SendMessage(b.Build());
		}
		[GroupMessage("签到")]
		public static async void DailyCheck(Group group, Member friend)
		{
			if (GroupHelper.Invalid(group.Id))
				return;
			var info = DBHelper.GetOrCreateOne(friend.Id, friend.Name);
			if (ApiDateTime.IsToday(info.DailyCheckT))
			{
				await group.OnSendMessage($"今天已经签到过了");
				return;
			}
			info.DailyCheckT = ApiDateTime.SecondsFromBegin();
			info.Energy += 10;
			info.Dazuo = 0;
			var r = RandomHelper.Next(1, info.Jingjie * 10);
			info.Qiankunzhu += r;
			DBHelper.Save(info);
			await group.SendGroupMessageAsync($"{friend.Name}签到成功，精力+10，目前有精力{info.Energy}，签到时还不小心从兜里掉出来{r}个祖传的乾坤珠。");
		}
		[GroupMessage("摇点")]
		public static async void Dice(Group group, Member friend, string extra)
		{
			if (GroupHelper.Invalid(group.Id))
				return;
			var dice = new DiceMessage();
			if (friend.Id == "63419182")
				dice.Value = (extra == "我不能输" ? 6 : rdm.Next(4) + 3).ToString();
			else
				dice.Value = (rdm.Next(6) + 1).ToString();
			await group.SendGroupMessageAsync(dice);
		}
		[GroupMessage("文章")]
		public static async void WenZhang(Group group, Member friend, string content)
		{
			if (GroupHelper.Invalid(group.Id))
				return;
			await group.SendGroupMessageAsync($"http://www.kcwork.gq/webcn/?task/{content}");
		}
		[GroupMessage("@月色")]
		public static async void ChatWithMe(Group group, Member friend, string content)
		{
			if (GroupHelper.Invalid(group.Id))
				return;
			Chat(group, friend, content);
		}
		[GroupMessage("闲聊")]
		public static async void Chat(Group group, Member friend, string content)
		{
			Normal(group, friend, content);
			if (GroupHelper.Invalid(group.Id))
				return;
			if (string.IsNullOrEmpty(content))
				return;
			if (content.Contains("废话"))
			{
				await group.OnSendMessage( "这种事情我见的多了，我只想说懂得都懂，不懂的我也不多说了，细细品吧，你也别来问我怎么回事，这里面利益牵扯太大了，说了对你我都没有好处，你就当不知道就行了，其余的我只能说这里水很深，牵扯到很多东西，详细情况你们很难找到的，网上大部分都删干净了，所以我说懂得都懂。");
				return;
			}
            if (content.Contains("聊骚"))
            {
				await group.OnSendMessage( "去死吧！！！");
				return;
			}
			var reply = GroupHelper.GetRobotReply(content);
			await group.OnSendMessage( await HttpHelper.GetRobotReply(content));
		}

		[GroupMessage("acg")]
		public static async void Acg(Group group, Member m, string content)
		{
			try
			{
				if (GroupHelper.InvalidSetu(group.Id))
					return;
				var res = await HttpHelper.GetHttpReq("https://api.xiaobaibk.com/api/pic/acg-1/?return=json");
				var img = JsonDocument.Parse(res).RootElement.GetProperty("imgurl").ToString().Split('?')[0];
				Console.WriteLine($"AcgPic {img}");
				var reply = new ImageMessage()
				{
					Url = img,
				};
				await group.SendMessage( reply);
			}
			catch
			{
				await group.OnSendMessage( "获取图片失败");
			}
		}
		[GroupMessage("cos")]
		public static async void Cosplay(Group group, Member m, string content)
		{
			try
			{
				if (GroupHelper.InvalidSetu(group.Id))
					return;
				var res = await HttpHelper.GetHttpReq("https://api.dzzui.com/api/cosplay?format=json");
				var img = JsonDocument.Parse(res).RootElement.GetProperty("imgurl").ToString().Split('?')[0];
				Console.WriteLine($"GirlPic {img}");
				var reply = new ImageMessage()
				{
					Url = img,
				};
				await group.SendMessage( reply);
			}
			catch
			{
				await group.OnSendMessage( "获取图片失败");
			}
		}
		[GroupMessage("mjx")]
		public static async void Mjx(Group group, Member m, string content)
		{
			try
			{
				if (GroupHelper.InvalidSetu(group.Id))
					return;
				var res = await HttpHelper.GetHttpReq("https://api.dzzui.com/api/imgtaobao?format=json");
				var img = JsonDocument.Parse(res).RootElement.GetProperty("imgurl").ToString();
				Console.WriteLine($"GirlPic {img}");
				var reply = new ImageMessage()
				{
					Url = img,
				};
				await group.SendMessage( reply);
			}
			catch
			{
				await group.OnSendMessage( "获取图片失败");
			}
		}
		[GroupMessage("美图")]
		public static async void GirlPic(Group group, Member m, string content)
		{
			if (GroupHelper.InvalidSetu(group.Id))
				return;
			try
			{
				var res = await HttpHelper.GetHttpReq("http://api.btstu.cn/sjbz/?lx=m_meizi&format=json");
				var img = JsonDocument.Parse(res).RootElement.GetProperty("imgurl").ToString();
				Console.WriteLine($"MeituPic {img}");
				var reply = new ImageMessage()
				{
					Url = img,
				};
				await group.SendMessage( reply);
			}
			catch
			{
				await group.OnSendMessage( "获取图片失败");
			}
		}

		[GroupMessage("电影")]
		public static async void MovieWeb(Group group, Member m)
		{
			await group.OnSendMessage( "https://www.subaibaiys.com/movie/39921.html");
		}

		[GroupMessage]
		public static void Normal(Group group, Member friend, string content)
		{
			Console.WriteLine($"[Group-{group.Id}-{group.Name}][QQ-{friend.Id}][Lv:{friend.MmeberProfile.Level}]{friend.Name}({friend.MmeberProfile.NickName}):{content}");
		}

		[GroupMessage("青楼")]
		public static async void OnQinglou(Group group, Member friend)
		{
			await group.SendGroupMessageAsync($"{friend.Name} 独上青楼听艳曲，手擎玉箫无人吹。");
		}

	}
}
