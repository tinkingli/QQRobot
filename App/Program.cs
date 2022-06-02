using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions;
using Mirai.Net.Sessions.Http.Managers;
using Mirai.Net.Utils.Scaffolds;
using System.Reactive.Linq;
using App;
using Mirai.Net.Data.Events.Concretes.Group;

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

Random rdm = new Random();
const string jdcloudkey = "758408c8351085593c85b08730d71bcf";
async void ConnectMirai()
{
	try
	{
		EventSystem.Instance.Add(DllHelper.GetHotfixAssembly());

		var exit = new ManualResetEvent(false);
		using var bot = new MiraiBot
		{
			Address = "127.0.0.1:8080",
			//QQ = "2531650036",//310683yuese
			QQ = "844013495",
			VerifyKey = "174291464"
		};

		await bot.LaunchAsync();

		bot.MessageReceived
			.OfType<GroupMessageReceiver>()
			.Subscribe(r =>
			{
				SaveToLocal(r);
				var at = "";
				foreach (var c in r.MessageChain)
				{
					if (c is AtMessage atMessage)
					{
						if (atMessage.Target == bot.QQ)
						{
							EventSystem.Instance.InvokeMothod(r.Sender.Group, r.Sender, "闲聊", r.MessageChain.GetPlainMessage());
							return;
						}
						else
						{
							at = atMessage.Target;
						}
					}
				}
				var amsg = r.MessageChain.GetPlainMessage().Split(' ', 2);
				EventSystem.Instance.InvokeMothod(r.Sender.Group, r.Sender, amsg[0], amsg.Length > 1 ? amsg[1] : at);
			});

		bot.MessageReceived
			.OfType<FriendMessageReceiver>()
			.Subscribe(async r =>
			{
				Console.WriteLine($"[Friend][{r.Sender.Id}][Lv:{r.Sender.FriendProfile.Level}]{r.Sender.NickName}:{r.MessageChain.GetPlainMessage()}");
				Console.WriteLine("");
				var amsg = r.MessageChain.GetPlainMessage().Split(' ', 2);
				EventSystem.Instance.InvokeMothod(r.Sender, amsg[0], amsg.Length > 1 ? amsg[1] : "");
			});

		bot.EventReceived
			.OfType<MemberJoinedEvent>()
			.Subscribe(async r =>
			{
				EventSystem.Instance.InvokeMothod(r.Member.Group, r.Member, "入群", "");
			});


		Console.WriteLine(bot.QQ);
		exit.WaitOne();
	}
	catch (Exception ex)
	{
		Console.WriteLine(ex);
	}
}

void SaveToLocal(GroupMessageReceiver r)
{
	try
	{
		var msg = r.MessageChain.GetPlainMessage();
		if (string.IsNullOrEmpty(msg))
			return;
		if (!Directory.Exists("logs"))
			Directory.CreateDirectory("logs");
		if (!Directory.Exists($"logs/{r.GroupName}"))
			Directory.CreateDirectory($"logs/{r.GroupName}");
		var f = $"logs/{r.GroupName}/{DateTime.Now.ToString("yyyy-MM-dd")}.txt";
		File.AppendAllText(f, $"{r}\r\n[{DateTime.Now.ToString("HH:mm:ss")}]{msg}\r\n");
	}
	catch (Exception ex)
	{
		Console.WriteLine($"[SaveToLocal]Error:{ex.Message}\r\n{ex.StackTrace}");
	}
}

Task.Run(() =>
{
	ConnectMirai();
});
while (true)
{
	var r = Console.ReadLine();
	var acmd = r.Split(' ');
	if (acmd[0] == "r")
		EventSystem.Instance.Add(DllHelper.GetHotfixAssembly());
	else if (acmd[0] == "c")
		return;
}
