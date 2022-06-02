using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Data.Shared;
using Mirai.Net.Sessions.Http.Managers;
using Mirai.Net.Utils.Scaffolds;

namespace App
{
    public static class InfoControllers
    {

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
			await group.SendMessage(b.Build());
		}
	}
}
