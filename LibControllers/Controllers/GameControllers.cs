using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Data.Shared;
using Mirai.Net.Sessions.Http.Managers;
using Mirai.Net.Utils.Scaffolds;

namespace App
{
	public static class GameControllers
	{
		class JuedouCell
		{
			public string id;
			private string _name;
			public Info info;
			public string Name
			{
				get
				{
					return $"【{_name}】";
				}
				set
				{
					_name = value;
				}
			}
		}
		class Juedou
		{
			public Juedou()
			{
				startt = DateTime.Now;
			}

			public JuedouCell A;
			public JuedouCell B;
			public DateTime startt;
			public bool Valid
			{
				get
				{
					return (DateTime.Now - startt).TotalSeconds < WaitSec;
				}
			}
			public const int WaitSec = 20;
		}
		private static int juedouid;
		private static Juedou juedou;

		[GroupMessage("决斗")]
		public static async void OnJuedou(Group group, Member friend, string content)
		{
			if (GroupHelper.Invalid(group.Id))
				return;
			if (juedou != null && juedou.Valid)
			{
				await MessageManager.SendGroupMessageAsync(group.Id, "有已经开启的决斗了，请稍候再试。");
				return;
			}

			juedouid++;
			var jid = juedouid;
			juedou = new Juedou() { A = new JuedouCell() { id = friend.Id, Name = friend.Name } };
			await MessageManager.SendGroupMessageAsync(group.Id, $"{friend.Name}发起了决斗，请于{Juedou.WaitSec}秒内回复“接受决斗”开启决斗。");

			await Task.Delay(Juedou.WaitSec * 1000);
			if (juedou == null || (juedou.A != null && juedou.B != null))
				return;
			if (jid != juedouid)
				return;
			await MessageManager.SendGroupMessageAsync(group.Id, "无人响应，决斗取消");
		}

		[GroupMessage("接受决斗")]
		public static async void OnAcceptJuedou(Group group, Member friend, string content)
		{
			if (juedou == null || !juedou.Valid)
			{
				await MessageManager.SendGroupMessageAsync(group.Id, "没有等待接受的决斗");
				return;
			}
			if (juedou.A.id == friend.Id)
			{
				await MessageManager.SendGroupMessageAsync(group.Id, "禁止自杀");
				return;
			}
			if (juedou.B != null)
			{
				await MessageManager.SendGroupMessageAsync(group.Id, "决斗已经开始了");
				return;
			}
			juedou.startt = DateTime.Now.AddDays(1);
			juedou.B = new JuedouCell() { id = friend.Id, Name = friend.Name };
			JuedouCell killer = null;
			JuedouCell bekill = null;
			if (RandomHelper.Next(100) > 50)
			{
				killer = juedou.A;
				bekill = juedou.B;
			}
			else
			{
				killer = juedou.B;
				bekill = juedou.A;
			}
			killer.info = DBHelper.GetOrCreateOne(killer.id, "");
			bekill.info = DBHelper.GetOrCreateOne(bekill.id, "");
			await MessageManager.SendGroupMessageAsync(group.Id, $"{juedou.B.Name}({juedou.B.info.JingjieDes})接受了{juedou.A.Name}({juedou.A.info.JingjieDes})发起的决斗，双方开始轮流出杀。");

			juedouid++;
			await Task.Delay(1000);

			var all = killer.info.Gongli + bekill.info.Gongli + 2;

			var lprocess = new List<string>();
			lprocess.Add($"{killer.Name}将率先向{bekill.Name}发起攻击");

			var round = 0;
			while (true)
			{
				round++;
				var success = RandomHelper.Next(all) < killer.info.Gongli + 1;
				if (success)
				{
					success = RandomHelper.Next(all) < killer.info.Gongli + 1;
					if (success)
					{
						if (round > 5)
							lprocess.Add($"{killer.Name}击中了{bekill.Name}，{bekill.Name}想了想，还是输了算了，{killer.Name}胜出。");
						else
							lprocess.Add($"{killer.Name}击中了{bekill.Name}，{bekill.Name}一个躲闪不及被打翻在地，{killer.Name}胜出。");
						killer.info.JuedouWin++;
						DBHelper.Save(killer.info);
						juedou = null;
						break;
					}
					else
					{
						lprocess.Add(GetRandomDesc(killer, bekill));
					}
				}
				else
				{
					lprocess.Add(GetRandomDesc(killer, bekill));
				}
				ObjectHelper.Swap(ref killer, ref bekill);
			}
			await GroupHelper.SendNormalMessage(group.Id, lprocess);
			if (killer.info.Jingjie <= bekill.info.Jingjie && killer.info.NeedTupo)
			{
				var success = RandomHelper.Next(10) > 3;
				if (success)
				{
					killer.info.Jingjie++;
					DBHelper.Save(killer.info);
					await group.SendGroupMessageAsync($"{killer.info.name}在决斗的生死压迫中感受到了命运的召唤，成功突破境界。");
				}
			}
			if (bekill.info.Jingjie <= killer.info.Jingjie && bekill.info.NeedTupo)
			{
				var success = RandomHelper.Next(10) > 7;
				if (success)
				{
					bekill.info.Jingjie++;
					DBHelper.Save(bekill.info);
					await group.SendGroupMessageAsync($"{bekill.info.name}虽然惜败，但是在决斗的生死压迫中也感受到了命运的召唤，成功突破境界。");
				}
			}
			if (RandomHelper.Next(2) == 0)
				return;
			var lucky = DBHelper.GetRandomCache();
			if (lucky == null)
				return;
			if (lucky.Jingjie < killer.info.Jingjie)
			{
				if (lucky.NeedTupo)
				{
					lucky.Jingjie++;
					DBHelper.Save(lucky);
					await group.SendGroupMessageAsync($"{lucky.name}在旁观决斗时忽然心有所悟，成功突破到了{lucky.Jingjie}境界");
					return;
				}
				lucky.Gongli++;
				DBHelper.Save(lucky);
				await group.SendGroupMessageAsync($"{lucky.name}被决斗的余威扫中，身受重伤，不过伤愈后功力竟然略有增长（功力+1({lucky.Gongli})）");
			}
		}

		private static string GetRandomDesc(JuedouCell killer, JuedouCell bekill)
		{
			var irdm = RandomHelper.Next(6);
			switch (irdm)
			{
				case 0:
					return $"{bekill.Name}使出【临门一脚】闪过了{killer.Name}的一击。";
				case 1:
					return $"{killer.Name}击中了{bekill.Name}，{bekill.Name}依靠顽强的意志使出了【朝申大那多】，化险为夷。";
				case 2:
					return $"{killer.Name}一枪打中了{bekill.Name}的心口，但是被{bekill.Name}未来的女朋友送的怀表挡住了这致命一击。";
				case 3:
					return $"{bekill.Name}被{killer.Name}爆头了，好在{bekill.Name}还有个备用的，换上以后跟新的一样。";
				case 4:
					return $"{killer.Name}把【方天画戟】耍的天昏地暗，日月无光，但可惜没打着{bekill.Name}";
				case 5:
				default:
					return $"{bekill.Name}掏出多啦B萌的随意门，躲过了{killer.Name}的攻击";
			}
		}

		[GroupMessage("打坐")]
		public static async void OnDazuo(Group group, Member friend)
		{
			if (GroupHelper.Invalid(group.Id))
				return;
			var info = DBHelper.GetOrCreateOne(friend.Id, friend.Name);
			if (info.Dazuo > 100)
			{
				await GroupHelper.SendNormalMessage(group.Id, $"{friend.Name}今天打坐次数已经很多了，修仙为逆天行事，不可操之过急，明日签到之后再继续吧");
				return;
			}
			if (info.XunbaoT > 0)
			{
				await GroupHelper.SendNormalMessage(group.Id, $"{friend.Name}正在寻宝中，不能打坐");
				return;
			}
			if (info.DazuoStartT == 0)
			{
				info.DazuoStartT = ApiDateTime.SecondsFromBegin();
				DBHelper.Save(info);
				await GroupHelper.SendNormalMessage(group.Id, $"{friend.Name}开始打坐");
				return;
			}
			if (ApiDateTime.SecondsFromBegin() - info.DazuoStartT < 60)
			{
				info.DazuoStartT = 0;
				DBHelper.Save(info);
				await GroupHelper.SendNormalMessage(group.Id, $"{friend.Name}打坐不足一分钟");
				return;
			}
			var s = (int)(ApiDateTime.SecondsFromBegin() - info.DazuoStartT);
			var ic = s / 60;
			if (ic > 10)
				ic = 10;
			info.DazuoStartT = 0;
			info.Dazuo += ic;
			info.Energy += ic;
			var dunwu = false;
			if (!info.NeedTupo && RandomHelper.Next(s) >= 600)
			{
				dunwu = true;
				info.Gongli++;
			}
			DBHelper.Save(info);
			await GroupHelper.SendNormalMessage(group.Id, $"{friend.Name}打坐{s}秒，精力+{ic}，目前有精力{info.Energy}" + (dunwu ? $"\n打坐中顿悟，想通了人和宇宙以及植物和僵尸之间的紧密关系，功力+1({info.Gongli})" : ""));
		}

		[GroupMessage("寻宝")]
		public static async void OnXunbao(Group group, Member friend)
		{
			try
			{
				if (GroupHelper.Invalid(group.Id))
					return;
				var info = DBHelper.GetOrCreateOne(friend.Id, friend.Name);
				if (info.DazuoStartT != 0)
				{
					await GroupHelper.SendNormalMessage(group.Id, $"{friend.Name}正在打坐，不能寻宝");
					return;
				}
				if (info.XunbaoT != 0)
				{
					if (info.XunbaoEndT < ApiDateTime.SecondsFromBegin())
					{
						info.XunbaoT = 0;
						info.XunbaoEndT = 0;

						var irdm = RandomHelper.Next(10);
						if (irdm == 0)
						{
							info.Gongli += 3;
							await GroupHelper.SendNormalMessage(group.Id, $"{friend.Name}逛街的时候买了一个高达10厘米的手办，功力大增（功力+3（{info.Gongli}））");
						}
						else if (irdm == 1)
						{
							info.Gongli += 2;
							await GroupHelper.SendNormalMessage(group.Id, $"{friend.Name}机缘巧合下解开了鸡兔同笼谜题，得到了【兔笼宝刀】一把，功力大增（功力+2（{info.Gongli}））");
						}
						else if (irdm == 2)
						{
							info.Gongli += 2;
							await GroupHelper.SendNormalMessage(group.Id, $"{friend.Name}只花了一天时间就逛遍了整个仙界，得到了【一天见】称号，功力大增（功力+2（{info.Gongli}））");
						}
						else if (irdm == 3)
						{
							info.Gongli++;
							await group.SendGroupMessageAsync($"{friend.Name}寻宝的时候掉下悬崖，摔得遍体鳞伤，不过捡到了一包【因祸德福巧克力】，吃了之后功力+1（{info.Gongli}）");
						}
						else
						{
							await group.SendGroupMessageAsync(GetRandomXunbaoDesc(friend.Name));
						}
						DBHelper.Save(info);
					}
					else
					{
						await GroupHelper.SendNormalMessage(group.Id, $"{friend.Name}寻宝中，目前一无所获");
					}
					return;
				}
				if (info.Energy < 10)
				{
					await GroupHelper.SendNormalMessage(group.Id, $"{friend.Name}精力不足10点，不能寻宝");
					return;
				}
				if (info.NeedTupo)
				{
					await GroupHelper.SendNormalMessage(group.Id, $"{friend.Name}功力达到瓶颈，需要渡劫成功，突破当前境界之后才能继续寻宝。");
					return;
				}
				info.Energy -= 10;
				info.XunbaoT = ApiDateTime.SecondsFromBegin();
				info.XunbaoEndT = info.XunbaoT + RandomHelper.Next(300) + 300;
				DBHelper.Save(info);
				var t = ApiDateTime.ToTime(info.XunbaoEndT);
				await GroupHelper.SendNormalMessage(group.Id, $"{friend.Name}开始寻宝，精力-10(目前剩余{info.Energy})，大概{t.ToString("HH:mm")}寻宝结束");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[寻宝Error]{ex}");
			}
		}

		[GroupMessage("渡劫")]
		public static async void OnDujie(Group group, Member friend)
		{
			var info = DBHelper.GetOrCreateOne(friend.Id, friend.Name);
			if (!info.NeedTupo)
			{
				await group.SendGroupMessageAsync($"小屁孩别捣乱。");
				return;
			}
			var success = RandomHelper.Next(info.MaxGongli) + 5 < 10;
			if (success)
			{
				info.Jingjie++;
				DBHelper.Save(info);
				await group.SendGroupMessageAsync($"{friend.Name}渡劫成功，喜大普奔，红旗招展，鞭炮齐鸣，人山人海，一举成为全村最靓的仔。");
				return;
			}
			info.Gongli = info.MaxGongli - 1;
			DBHelper.Save(info);
			await group.SendGroupMessageAsync($"{friend.Name}渡劫失败，遭受反噬，功力大减（{info.Gongli}）。");
		}

		private static string GetRandomXunbaoDesc(string name)
		{
			var irdm = RandomHelper.Next(10);
			switch (irdm)
			{
				case 0:
					return $"{name}寻宝一天，只从隔壁王奶奶家翻到破袜子一只。";
				case 1:
					return $"{name}在隔壁李二狗家看了一天蚂蚁搬家，寻宝结束。";
				case 2:
					return $"{name}在D站刷了一天钢琴视频，寻宝结束。";
				case 3:
					return $"{name}在山顶吹了一天风，也没找到什么有用的东西。";
				case 4:
					return $"{name}在树林里找到了一根白杆杆的红伞伞，吃完之后发现什么也没发生。";
				case 5:
				case 6:
					return $"{name}寻宝过程中偶遇一只菜虚鲲，跟它玩儿了一天篮球。";
				case 7:
				case 8:
					return $"{name}在寻宝过程中突发奇想，美美地睡了一觉，发现地球原来是圆的，成为了一代知名大画家。";
			}
			return $"{name}寻宝一天，只从隔壁王奶奶家翻到吃剩的大闸蟹一只。";
		}

		[GroupMessage("青楼")]
		public static async void OnQinglou(Group group, Member friend)
		{
			await group.SendGroupMessageAsync($"{friend.Name} 独上青楼听艳曲，手擎玉箫无人吹。");
		}

		[GroupMessage("信息")]
		public static async void OnXinxi(Group group, Member friend)
		{
			if (GroupHelper.Invalid(group.Id))
				return;
			var info = DBHelper.GetOrCreateOne(friend.Id, friend.Name);
			var res = $"【{friend.Name}】\n" +
				$"功力：{info.Gongli}\n" +
				$"境界：{info.JingjieDes}\n" +
				$"精力：{info.Energy}\n" +
				$"打坐进度：{info.Dazuo}/100"
				;
			var b = new MessageChainBuilder();
			var m = new ImageMessage() { Base64 = Convert.ToBase64String(System.IO.File.ReadAllBytes("logo.png")) };
			b.Append(m).Append(new PlainMessage() { Text = res });
			await group.SendGroupMessageAsync(b.Build());
		}

		[GroupMessage("功力榜")]
		public static async void OnGonglibang(Group group, Member friend)
		{
			var ress = DBHelper.GetRank("Gongli");
			var res = "《烦人修仙传》功力榜\n";
			foreach (var kv in ress)
			{
				res += $"【{kv.Key + 1}】{kv.Value.name} 功力：({kv.Value.Gongli})\n";
			}
			var b = new MessageChainBuilder();
			var m = new ImageMessage() { Base64 = Convert.ToBase64String(System.IO.File.ReadAllBytes("logo.png")) };
			b.Append(m).Append(new PlainMessage() { Text = res });
			await group.SendGroupMessageAsync(b.Build());
		}

		[GroupMessage("精力榜")]
		public static async void OnJinglibang(Group group, Member friend)
		{
			var ress = DBHelper.GetRank("Energy");
			var res = "《烦人修仙传》精力榜\n";
			foreach (var kv in ress)
			{
				res += $"【{kv.Key + 1}】{kv.Value.name} 精力：({kv.Value.Energy})\n";
			}
			var b = new MessageChainBuilder();
			var m = new ImageMessage() { Base64 = Convert.ToBase64String(System.IO.File.ReadAllBytes("logo.png")) };
			b.Append(m).Append(new PlainMessage() { Text = res });
			await group.SendGroupMessageAsync(b.Build());
		}

		[GroupMessage("决斗榜")]
		public static async void OnJuedoubang(Group group, Member friend)
		{
			var ress = DBHelper.GetRank("JuedouWin");
			var res = "《烦人修仙传》决斗榜\n";
			foreach (var kv in ress)
			{
				res += $"【{kv.Key + 1}】{kv.Value.name} 决斗胜利 {kv.Value.JuedouWin} 次\n";
			}
			var b = new MessageChainBuilder();
			var m = new ImageMessage() { Base64 = Convert.ToBase64String(System.IO.File.ReadAllBytes("logo.png")) };
			b.Append(m).Append(new PlainMessage() { Text = res });
			await group.SendGroupMessageAsync(b.Build());
		}

		[GroupMessage("境界榜")]
		public static async void OnJingjiebang(Group group, Member friend)
		{
			var ress = DBHelper.GetRank("Jingjie");
			var res = "《烦人修仙传》境界榜\n";
			foreach (var kv in ress)
			{
				res += $"【{kv.Key + 1}】{kv.Value.name} 境界：{kv.Value.JingjieDes}\n";
			}
			var b = new MessageChainBuilder();
			var m = new ImageMessage() { Base64 = Convert.ToBase64String(System.IO.File.ReadAllBytes("logo.png")) };
			b.Append(m).Append(new PlainMessage() { Text = res });
			await group.SendGroupMessageAsync(b.Build());
		}

		[GroupMessage("功能")]
		public static async void OnFunctions(Group group, Member friend)
		{
			var b = new MessageChainBuilder();
			var m = new ImageMessage() { Base64 = Convert.ToBase64String(System.IO.File.ReadAllBytes("logo.png")) };
			var res = "【打坐】开始/结束打坐，可以增加精力\n" +
				"【寻宝】开始/结束寻宝，消耗精力概率涨功力\n" +
				"【签到】每天一次，增加精力并重置打坐进度\n" +
				"【信息】查看自己的信息\n" +
				"【功力榜】【精力榜】【决斗榜】【境界榜】\n" +
				"更多功能敬请期待\n" +
				"《烦人修仙传》烦人，我们是认真的！";
			b.Append(m).Append(new PlainMessage() { Text = res });
			await group.SendGroupMessageAsync(b.Build());
		}
	}
}
