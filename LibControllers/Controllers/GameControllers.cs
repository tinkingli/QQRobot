using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Data.Shared;
using Mirai.Net.Sessions.Http.Managers;
using Mirai.Net.Utils.Scaffolds;
using System;

namespace App
{
    public static class GameControllers
    {
        [GroupMessage("福地")]
        public static async void OnFudi(Group group, Member friend, string target)
        {
            if (GroupHelper.Invalid(group.Id))
                return;
            if (string.IsNullOrEmpty(target) || target != "探险")
            {
                await group.OnSendMessage(@"·输入“福地 探险”命令可以探索新的福地
·输入“探险”可以进入自己的福地探险
·输入“探险”并at福地主人可以进入对方的福地探险
·输入“驯服”可以根据境界消耗灵气驯服探险中发现的怪兽
·输入“收工”可以放弃探险获得战利品
");
                return;
            }
            var info = DBHelper.GetOrCreateOne(friend.Id, friend.Name);
            if (info.Jingjie < 2)
            {
                await group.OnSendMessage($"福地之行危险至极，至少把你的境界提升到{Info.JingjieDescs[2]}之境再来吧");
                return;
            }
            if (ApiDateTime.IsToday(info.FudiStartT))
            {
                await group.OnSendMessage("福地可遇不可求，明日再探索吧");
                return;
            }
            info.FudiStartT = ApiDateTime.SecondsFromBegin();
            info.FudiEnded = false;
            info.FudiLayer = 1 + RandomHelper.Next(info.Jingjie);
            info.FudiLastGongli = 0;
            info.FudiNeedGongli = 0;
            info.FudiNeedLingqi = info.FudiLayer * info.FormerJieduanMax / 2 + RandomHelper.Next(info.FormerJieduanMax);
            info.FudiBlockLayer = info.FudiLayer + RandomHelper.Next(info.Jingjie) + 1;
            info.Save();
            await group.OnSendMessage($"{info.name}探索到了一处{info.FudiLayer}级福地，大家快来探险");
            await group.OnSendMessage($"{info.name}的{info.FudiLayer}级福地，内有{info.FudiNeedLingqi}狂暴的灵气，需将其中的灵气全部平复下来才能获取其中的乾坤珠。");
        }
        [GroupMessage("探险")]
        public static async void OnTanxian(Group group, Member friend, string target)
        {
            if (GroupHelper.Invalid(group.Id))
                return;
            var info = DBHelper.GetOrCreateOne(friend.Id, friend.Name);
            if (string.IsNullOrEmpty(target.Trim()))
                target = friend.Id;
            var fudi = DBHelper.Get(target);
            if (fudi == null)
            {
                await group.OnSendMessage($"这位道友还没有福地可以进行探险");
                return;
            }
            if (fudi.FudiEnded)
            {
                await group.OnSendMessage($"{fudi.name}的福地探险已经结束了，下次请早吧");
                return;
            }
            if (info.id != fudi.id && info.Jingjie > fudi.Jingjie + 1)
            {
                await group.OnSendMessage($"{fudi.name}的福地太过脆弱，承受不了{info.name}的{info.JingjieDes}境之威。");
                return;
            }
            if (fudi.FudiLastGongli > 0)
            {
                await group.OnSendMessage($"{fudi.name}的福地发现了一个功力为{fudi.FudiLastGongli}/{fudi.FudiNeedGongli}的神兽，需要功力高于{fudi.FudiNeedGongli}的高手先尝试“驯服”(每次消耗{fudi.FormerJieduanMax}灵气)它才能继续探索这处福地。");
                return;
            }
            if (info.Lingqi == 0)
            {
                await group.OnSendMessage($"{info.name}的{info.FudiLayer}级福地中灵气激荡({info.FudiNeedLingqi})，道友且先修炼一番，积攒些灵气护体再来吧。");
                return;
            }
            if (fudi.FudiNeedLingqi > info.Lingqi)
            {
                fudi.FudiNeedLingqi -= info.Lingqi;
                info.Lingqi = 0;
                info.Save();
                await group.OnSendMessage($"{info.name}在福地中探险一番，狂暴的灵气平息里不少");
                return;
            }
            info.Lingqi -= fudi.FudiNeedLingqi;
            if (fudi.FudiLayer >= fudi.FudiBlockLayer)
            {
                fudi.FudiBlockLayer = fudi.FudiLayer + RandomHelper.Next(fudi.Jingjie) + 1;
                fudi.FudiNeedGongli = fudi.Gongli + RandomHelper.Next(fudi.Gongli * fudi.FudiLayer / 2);
                fudi.FudiLastGongli = fudi.FudiNeedGongli;
                fudi.FudiNum1 = RandomHelper.Next(50);
                fudi.FudiNum2 = 0;
                fudi.Save();
                await group.OnSendMessage($"{fudi.name}的福地中的灵气被平息后，惊动了福地中酣睡的一头功力{fudi.FudiNeedGongli}的神兽。");
            }
            else
                FudiLevelUp(group, fudi, info);
        }
        private static async void FudiLevelUp(Group group, Info fudi, Info info)
        {
            fudi.FudiNeedLingqi = fudi.FudiLayer * fudi.FormerJieduanMax + RandomHelper.Next(fudi.FormerJieduanMax);
            var seprated = 0;
            if (info.id != fudi.id)
            {
                seprated = RandomHelper.Next(fudi.FudiLayer / 2);
                if (seprated <= 0)
                    seprated = 1;
                info.Qiankunzhu += seprated;
                await group.OnSendMessage($"{info.name}在{fudi.name}的福地中扫荡一番，发现了{seprated}颗乾坤珠。");
                seprated--;//外力平息福地额外减1乾坤珠
            }
            var last = fudi.FudiLayer - seprated;
            if (last > 0)
            {
                fudi.FudiQiankunzhu += last;
                await group.OnSendMessage($"{fudi.name}的福地中的灵气被平息下来，在其中发现了{last}颗乾坤珠。");
            }
            else
                await group.OnSendMessage($"{fudi.name}的福地中的灵气被平息下来，但是没有找到乾坤珠。");
            fudi.FudiLayer++;
            if (fudi.FudiMaxLayer < fudi.FudiLayer)
                fudi.FudiMaxLayer = fudi.FudiLayer;
            info.Save();
            if (info.id != fudi.id)
                fudi.Save();
            await group.OnSendMessage($"{fudi.name}的福地内有乾坤，发现了新的一层，内有{fudi.FudiNeedLingqi}狂暴的灵气，需将其中的灵气全部平复下来才能获取其中的宝物，目前累积已经发现了{fudi.FudiQiankunzhu}颗乾坤珠，退出可以立即收取。");
        }
        [GroupMessage("收工")]
        public static async void OnShougong(Group group, Member friend, string target)
        {
            if (GroupHelper.Invalid(group.Id))
                return;
            var info = DBHelper.GetOrCreateOne(friend.Id, friend.Name);
            if (info.FudiEnded)
            {
                await group.OnSendMessage("本次福地探险已经圆满结束。");
                return;
            }
            if (info.FudiNeedGongli > 0)
            {
                await group.OnSendMessage("福地中的乾坤珠都被神兽施法禁锢住了，无法带出福地！");
                return;
            }
            if (info.FudiQiankunzhu == 0)
            {
                await group.OnSendMessage("没有任何收获就此退出福地的话，心里还是有些不甘的。");
                return;
            }
            info.FudiEnded = true;
            info.Qiankunzhu += info.FudiQiankunzhu;
            await group.OnSendMessage($"{info.name}在本次福地探险中一共收获了{info.FudiQiankunzhu}颗乾坤珠。");
            info.FudiQiankunzhu = 0;
            info.FudiNum1 = 0;
            info.FudiNum2 = 0;
            info.Save();
        }
        [GroupMessage("神兽")]
        public static async void OnShenshou(Group group, Member friend, string target)
        {
            var info = DBHelper.GetOrCreateOne(friend.Id, friend.Name);
            if (!long.TryParse(friend.Id, out var lid))
                return;
            long inum;
            long.TryParse(target, out inum);
            if (info.FudiNum1 == 0)
            {
                await group.OnSendMessage($"神兽之题已解。");
                return;
            }
            if (info.FudiNum1 != inum)
            {
                info.FudiNum2++;
                if (info.FudiNum1 > inum)
                    await group.OnSendMessage(info.FudiNum1 - inum > 10 ? "吾心中之数多之甚矣" : $"吾心中之数较大");
                else
                    await group.OnSendMessage(inum - info.FudiNum1 > 10 ? "吾心中之数小之甚矣" : $"吾心中之数较小");
                info.Save();
                return;
            }
            info.FudiNum1 = 0;
            if (info.FudiNum2 <= 5)
            {
                var add = RandomHelper.Next(info.Jingjie * 10) + 1;
                info.FudiQiankunzhu += add;
                await group.OnSendMessage($"{info.name}猜中了神兽心中所想，福地乾坤珠增加了{add}个待收取。");
            }
            else
            {
                await group.OnSendMessage($"{info.name}猜中了神兽心中所想。");
            }
            info.Save();
        }
        [GroupMessage("驯服")]
        public static async void OnXunfu(Group group, Member friend, string target)
        {
            if (!long.TryParse(friend.Id, out var lid))
                return;
            var info = DBHelper.GetOrCreateOne(friend.Id, friend.Name);
            if (string.IsNullOrEmpty(target.Trim()))
                target = friend.Id;
            var acontent = target.Split(' ', 2);
            var fudi = DBHelper.Get(acontent[0]);
            if (fudi == null)
            {
                await group.OnSendMessage($"驯服？驯服啥？");
                return;
            }
            if (fudi.FudiEnded)
            {
                await group.OnSendMessage($"本次{fudi.name}的福地探险已经圆满结束{(fudi.FudiNeedGongli > 0 ? "(迫真)" : "")}。");
                return;
            }
            if (fudi.id != info.id && fudi.FudiNeedGongli > info.Gongli)
            {
                await group.OnSendMessage($"{fudi.name}的福地中的神兽厉害非凡，需要功力高于{fudi.FudiNeedGongli}的道友方可来协助降服！");
                return;
            }
            if (fudi.FormerJieduanMax > info.Lingqi)
            {
                await group.OnSendMessage($"驯服{fudi.name}的福地中的神兽至少需要{fudi.FormerJieduanMax}灵气。");
                return;
            }
            if (fudi.FudiLastGongli <= 0)
            {
                await group.OnSendMessage($"{fudi.name}的福地中的神兽已经远遁了。");
                return;
            }
            if (info.id != fudi.id && info.Jingjie > fudi.Jingjie + 1)
            {
                await group.OnSendMessage($"{fudi.name}的福地太过脆弱，承受不了{info.name}的{info.JingjieDes}境之威。");
                return;
            }
            var last = fudi.FudiBattleT - ApiDateTime.SecondsFromBegin();
            if (last > 0)
            {
                await group.OnSendMessage($"{fudi.name}的福地中的上一场驯服神兽的激战造成的灵气紊乱仍为平息，请大约{last / 60 + 1}分钟后再来吧。");
                return;
            }
            if (fudi.FudiNum1 != 0)
            {
                await group.OnSendMessage($"神兽口吐人言，“吾心中有一数字，汝每次猜测，吾会提示你大或小，五次内猜中，吾将予奖赏。”回复“神兽 答案”可作答。");
                return;
            }

            info.Lingqi -= fudi.FormerJieduanMax;
            var rdm = RandomHelper.Next(fudi.FudiNeedGongli / 2);
            fudi.FudiLastGongli -= rdm;
            if (fudi.FudiLastGongli > 0)
            {
                fudi.FudiBattleT = ApiDateTime.SecondsFromBegin() + RandomHelper.Next(300) + 300;
                fudi.Save();
                info.Save();
                await group.OnSendMessage($"经过一场大战，{fudi.name}的福地中的神兽元气大伤({fudi.FudiLastGongli}/{fudi.FudiNeedGongli})，隐匿起来。");
            }
            else
            {
                var rdm2 = RandomHelper.Next(fudi.FudiLayer) + 1;
                info.Qiankunzhu += rdm2;
                fudi.FudiBattleT = 0;
                fudi.FudiNeedGongli = 0;
                await group.OnSendMessage($"经过一场大战，{fudi.name}的福地中的神兽终于被{info.name}降服，{info.name}在神兽身上摸来摸去，摸到了{rdm2}颗乾坤珠。");
                FudiLevelUp(group, fudi, info);
            }
        }
        [GroupMessage("乾坤珠")]
        public static async void OnQiankunzhu(Group group, Member friend, string target)
        {
            if (GroupHelper.Invalid(group.Id))
                return;
            var info = DBHelper.GetOrCreateOne(friend.Id, friend.Name);
            var inum = 0;
            if (!int.TryParse(target, out inum))
            {
                await group.OnSendMessage($"输入“乾坤珠 数量（1或10的倍数）”可以使用乾坤珠，{info.name}当前持有{info.Qiankunzhu}颗乾坤珠。");
                return;
            }
            if (info.Jingjie < 2)
            {
                await group.OnSendMessage($"{info.name}的境界太低了，现在服用乾坤珠容易消化不良啊。");
                return;
            }
            if (inum <= 0)
                return;
            if (inum != 1 && inum % 10 != 0)
            {
                await group.OnSendMessage($"输入“乾坤珠 数量（1或10的倍数）”可以使用乾坤珠，{info.name}当前持有{info.Qiankunzhu}颗乾坤珠。");
                return;
            }
            if (info.Qiankunzhu < inum)
            {
                await group.OnSendMessage($"输入“乾坤珠 数量（1或10的倍数）”可以使用乾坤珠，{info.name}当前持有{info.Qiankunzhu}颗乾坤珠。");
                return;
            }
            if (info.QiankunzhuCDT > ApiDateTime.SecondsFromBegin())
            {
                await group.OnSendMessage($"上次使用的乾坤珠还未尽数吸收，且再稍等{info.QiankunzhuCDT - ApiDateTime.SecondsFromBegin()}秒。");
                return;
            }
            var irdm = RandomHelper.Next(10);
            if (irdm < 3)
            {
                var addlingqi = RandomHelper.Next(1, info.MaxGongli / 2) * inum;
                info.Qiankunzhu -= inum;
                info.Lingqi += addlingqi;
                await group.OnSendMessage($"{info.name}使用{inum}个乾坤珠获得了{addlingqi}点灵气，可喜可贺。");
            }
            else if (irdm < (info.DongtianCD() > 0 ? 9 : 10))
            {
                var q = new System.Collections.Concurrent.ConcurrentQueue<int>();
                q.Clear();

                info.Qiankunzhu -= inum;
                var addgongli = RandomHelper.Next(1, info.Jingjie * 5) * inum;
                info.Gongli += addgongli;
                if (info.Gongli > info.MaxGongli)
                {
                    var extra = info.Gongli - info.MaxGongli;
                    info.DujieExtra += extra;
                    info.Gongli = info.MaxGongli;
                    await group.OnSendMessage($"{info.name}使用{inum}个乾坤珠增长了{addgongli}点功力，其中{extra}点功力由于境界限制转换为渡劫成功率。");
                }
                else
                    await group.OnSendMessage($"{info.name}使用{inum}个乾坤珠增长了{addgongli}点功力，可喜可贺。");
            }
            else if (info.DongtianCD() > 0)
            {
                info.Qiankunzhu--;
                info.DongtianEndT = 0;
                await group.OnSendMessage($"{info.name}使用第一个乾坤珠时突有所悟，立刻盘膝而坐，压制下了自己由于在洞天中修炼而激荡的灵气。");
            }
            info.QiankunzhuCDT = ApiDateTime.SecondsFromBegin() + 10;
            info.Save();
        }
        [GroupMessage("修炼")]
        public static async void OnXiulian(Group group, Member friend, string target)
        {
            if (GroupHelper.Invalid(group.Id))
                return;
            var info = DBHelper.GetOrCreateOne(friend.Id, friend.Name);
            if (info.Jingjie < 1)
            {
                await group.OnSendMessage($"洞天之行危险至极，至少把你的境界提升到{Info.JingjieDescs[1]}之境再来吧");
                return;
            }
            var cd = info.DongtianEndT - ApiDateTime.SecondsFromBegin();
            if (cd > 0)
            {
                await group.OnSendMessage($"【{info.name}】洞天中汲取的外来灵气鼓荡全身，需一段时间静修，大约{cd / 60 + 1}分钟之后可彻底稳固修为");
                return;
            }
            Info? dongtian = null;
            if (string.IsNullOrEmpty(target))
            {
                dongtian = info;
            }
            else
            {
                dongtian = DBHelper.Get(target);
                if (dongtian == null)
                    return;
            }
            if (info.Energy <= 0)
            {
                await group.OnSendMessage($"{info.name}已经精疲力尽了，明日再来吧");
                return;
            }
            if (dongtian.DongtianLingqi <= 0)
            {
                if (dongtian.DongtianStartT == 0)
                    await group.OnSendMessage($"【{dongtian.name}】还没有探索过洞天");
                else
                    await group.OnSendMessage($"【{dongtian.name}】探索到的这处洞天已经枯竭了");
            }
            else
            {
                var jd = Info.JingjieFenduan[info.Jingjie - 1];
                var irdm = RandomHelper.Next(jd / 2) + 1;
                if (irdm > info.Energy)
                    irdm = (int)info.Energy;
                var beilv = 1;
                if (info.Jingjie > 1)
                    beilv = Info.JingjieFenduan[info.Jingjie - 2];

                if (irdm * beilv > dongtian.DongtianLingqi)
                    irdm = 1;
                info.Energy -= irdm;
                var gain = irdm * beilv;
                if (gain > dongtian.DongtianLingqi)
                    gain = dongtian.DongtianLingqi;
                dongtian.DongtianLingqi -= gain;
                if (dongtian.DongtianLingqi == 0)
                    dongtian.Gongli += beilv;
                info.Lingqi += gain;
                var str = CheckLingqi(info, irdm, gain, true);
                info.DongtianEndT = ApiDateTime.SecondsFromBegin() + irdm * 60;
                await group.OnSendMessage(str);
                dongtian.Save();
                info.Save();
                if (dongtian.DongtianLingqi > 0)
                    return;
                await group.OnSendMessage($"【{dongtian.name}】的洞天中灵气被席卷一空，【{dongtian.name}】似有所感，在其中盘膝而坐，感受到功力飞速增长（功力+10（{dongtian.Gongli}））。");

                var lucky = DBHelper.GetRandomCache();
                if (lucky.id == info.id)
                    return;
                if (lucky.NeedTupo)
                {
                    lucky.DujieExtra++;
                    await group.OnSendMessage($"【{dongtian.name}】的洞天崩裂时动静太大，打扰了【{lucky.name}】的清修，【{lucky.name}】一气之下把隔壁邻居家的窗玻璃擦得干干净净，擦完心情好了很多，竟然感觉境界瓶颈松动了【少许】");
                }
                else
                {
                    lucky.Gongli += 5;
                    await group.OnSendMessage($"【{dongtian.name}】的洞天崩裂过程中溢出的灵气被在附近遛狗的【{lucky.name}】不小心吸收掉了，【{lucky.name}】功力小幅上涨(功力+5({lucky.Gongli}))");
                }
                lucky.Save();
            }
        }

        private static string CheckLingqi(Info info, int cost, int gain, bool CostEnergy)
        {
            var str = "";
            var jd = Info.JingjieFenduan[info.Jingjie - 1];
            if (info.Lingqi > jd)
            {
                info.Lingqi -= jd;
                if (info.NeedTupo)
                {
                    info.DujieExtra++;
                    if (info.DujieExtra < 3)
                        str = $"【{info.name}】在洞天中消耗{cost}{(CostEnergy ? "精力" : "灵气")}汲取了{gain}灵气，感觉境界瓶颈松动了【少许】, {cost}分钟后可再次修炼";
                    else if (info.DujieExtra < 6)
                        str = $"【{info.name}】在洞天中消耗{cost}{(CostEnergy ? "精力" : "灵气")}汲取了{gain}灵气，感觉境界瓶颈松动了【一些】, {cost}分钟后可再次修炼";
                    else
                        str = $"【{info.name}】在洞天中消耗{cost}{(CostEnergy ? "精力" : "灵气")}汲取了{gain}灵气，感觉境界瓶颈松动了【很多】, {cost}分钟后可再次修炼";
                }
                else
                {
                    info.Gongli++;
                    str = $"【{info.name}】在洞天中修炼良久，消耗{cost}{(CostEnergy ? "精力" : "灵气")}汲取了{gain}灵气，终于有所感悟，功力+1, {cost}分钟后可再次修炼";
                }
            }
            else
            {
                str = $"【{info.name}】在洞天中全心修炼，消耗{cost}{(CostEnergy ? "精力" : "灵气")}汲取了{gain}灵气，似有所得, {cost}分钟后可再次修炼";
            }
            return str;
        }
        private const int SanhuajudingCost = 50;
        [GroupMessage("三花聚顶")]
        public static async void OnSanhuajuding(Group group, Member friend)
        {
            if (GroupHelper.Invalid(group.Id))
                return;
            var info = DBHelper.GetOrCreateOne(friend.Id, friend.Name);
            if (info.Lingqi < SanhuajudingCost)
            {
                await group.OnSendMessage($"{friend.Name}灵气不足{SanhuajudingCost}，不足以施展三花聚顶神功");
                return;
            }
            if (info.DongtianLingqi < SanhuajudingCost)
            {
                await group.OnSendMessage($"{friend.Name}探索到这这处洞天已灵气不足{SanhuajudingCost}");
                return;
            }
            var cd = info.DongtianEndT - ApiDateTime.SecondsFromBegin();
            if (cd > 0)
            {
                await group.OnSendMessage($"【{info.name}】洞天中汲取的外来灵气鼓荡全身，需一段时间静修，大约{cd / 60 + 1}分钟之后可彻底稳固修为");
                return;
            }
            var v = Math.Min(info.Gongli, info.Lingqi);
            var irdm = RandomHelper.Next(v) + v / 2;
            info.Lingqi -= SanhuajudingCost;
            if (irdm > info.DongtianLingqi)
                irdm = info.DongtianLingqi;
            info.DongtianLingqi -= irdm;
            if (info.DongtianLingqi == 0)
                info.Gongli += 10;

            info.Lingqi += irdm;
            info.DongtianEndT = ApiDateTime.SecondsFromBegin() + SanhuajudingCost * 60;
            await group.OnSendMessage(CheckLingqi(info, SanhuajudingCost, irdm, false));
            info.Save();
            if (info.DongtianLingqi > 0)
                return;
            await group.OnSendMessage($"【{info.name}】的洞天中灵气被席卷一空，【{info.name}】似有所感，在其中盘膝而坐，感受到功力飞速增长（功力+10（{info.Gongli}））。");

            var lucky = DBHelper.GetRandomCache();
            if (lucky.id == info.id)
                return;
            if (lucky.NeedTupo)
            {
                lucky.DujieExtra++;
                var t1 = new PlainMessage() { Text = $"【{info.name}】的洞天崩裂时动静太大，打扰了" };
                var at = new AtMessage();
                at.Target = lucky.id;
                var t2 = new PlainMessage() { Text = $"的清修，【{lucky.name}】一气之下把隔壁邻居家的窗玻璃擦得干干净净，擦完心情好了很多，竟然感觉境界瓶颈松动了【少许】" };
                await group.OnSendMessage(t1, at, t2);
            }
            else
            {
                lucky.Gongli += 5;
                var t1 = new PlainMessage() { Text = $"【{info.name}】的洞天崩裂过程中溢出的灵气被在附近遛狗的" };
                var at = new AtMessage();
                at.Target = lucky.id;
                var t2 = new PlainMessage() { Text = $"不小心吸收掉了，【{lucky.name}】功力小幅上涨(功力+5({lucky.Gongli}))" };
                await group.OnSendMessage(t1, at, t2);
            }
            lucky.Save();
        }

        [GroupMessage("洞天")]
        public static async void OnDongtian(Group group, Member friend, string content)
        {
            if (GroupHelper.Invalid(group.Id))
                return;
            if (string.IsNullOrEmpty(content))
            {
                await group.OnSendMessage($"·输入“洞天 新建”可以开始探索新的洞天\n" +
                    $"·探索到洞天后输入“修炼”可以消耗精力汲取灵气\n" +
                    $"·输入“修炼”后at洞天主人可以进入其已经开启的洞天汲取灵气");
                return;
            }
            var info = DBHelper.GetOrCreateOne(friend.Id, friend.Name);
            if (info.Jingjie < 1)
            {
                await group.OnSendMessage($"洞天之行危险至极，至少把你的境界提升到先天之境再来吧");
                return;
            }
            if (ApiDateTime.IsToday(info.DongtianStartT))
            {
                await group.OnSendMessage($"今日的洞天之行很辛苦，好好休息一下，明日再来吧");
                return;
            }
            if (content == "新建")
            {
                var v = Math.Max(info.Gongli, info.Lingqi);
                var dt = RandomHelper.Next(v) + (int)(v * 1.1);
                info.DongtianLingqi = dt;
                info.DongtianStartT = ApiDateTime.SecondsFromBegin();
                DBHelper.Save(info);
                await group.OnSendMessage($"【{info.name}】探索到了一个灵气值为{dt}的洞天宝地。");
            }
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
                await group.OnSendMessage("有已经开启的决斗了，请稍候再试。");
                return;
            }

            juedouid++;
            var jid = juedouid;
            juedou = new Juedou() { A = new JuedouCell() { id = friend.Id, Name = friend.Name } };
            if (content == "还有谁")
                juedou.LimitJingjie = false;
            var info = DBHelper.GetOrCreateOne(friend.Id, friend.Name);
            juedou.A.info = info;
            await group.OnSendMessage($"{friend.Name}发起了{(!juedou.LimitJingjie ? "【不限境界】的" : $"只限【{info.JingjieDes}境】的")}决斗，请于{Juedou.WaitSec}秒内回复“接受决斗”开启决斗。");

            await Task.Delay(Juedou.WaitSec * 1000);
            if (juedou == null || (juedou.A != null && juedou.B != null))
                return;
            if (jid != juedouid)
                return;
            await group.OnSendMessage("无人响应，决斗取消");
        }
        [GroupMessage("接受决斗")]
        public static async void OnAcceptJuedou(Group group, Member friend, string content)
        {
            if (GroupHelper.Invalid(group.Id))
                return;
            if (juedou == null || !juedou.Valid)
            {
                await group.OnSendMessage("没有等待接受的决斗");
                return;
            }
            if (juedou.A.id == friend.Id)
            {
                await group.OnSendMessage("禁止自杀");
                return;
            }
            if (juedou.B != null)
            {
                await group.OnSendMessage("决斗已经开始了");
                return;
            }
            var info = DBHelper.GetOrCreateOne(friend.Id, friend.Name);
            if (juedou.LimitJingjie && juedou.A.info.Jingjie != info.Jingjie)
            {
                await group.OnSendMessage($"本次决斗只限{juedou.A.info.JingjieDes}境界");
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
            await group.OnSendMessage($"{juedou.B.Name}({juedou.B.info.JingjieDes})接受了{juedou.A.Name}({juedou.A.info.JingjieDes})发起的决斗，双方开始轮流出杀。");

            juedouid++;
            await Task.Delay(1000);

            var all = killer.info.Gongli + bekill.info.Gongli + 2;

            var lprocess = new List<string>();
            lprocess.Add($"{killer.Name}将率先向{bekill.Name}发起攻击");

            var round = 0;
            while (true)
            {
                round++;
                var success = RandomHelper.Next(all) < killer.info.Gongli + (killer.info.Jingjie > bekill.info.Jingjie ? Info.JingjieFenduan[killer.info.Jingjie - 1] : 1);
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
            await group.SendMessage(lprocess);
            if (killer.info.Jingjie <= bekill.info.Jingjie && killer.info.NeedTupo)
            {
                var baseSucc = killer.info.DujieExtra;
                if (killer.info.Jingjie == bekill.info.Jingjie)
                    baseSucc++;
                var success = RandomHelper.Next(info.MaxGongli) < baseSucc;
                if (success)
                {
                    killer.info.Jingjie++;
                    DBHelper.Save(killer.info);
                    await group.OnSendMessage($"{killer.info.name}在决斗的生死压迫中感受到了命运的召唤，成功突破境界。");
                }
            }
            if (bekill.info.Jingjie <= killer.info.Jingjie && bekill.info.NeedTupo)
            {
                var success = RandomHelper.Next(10) > 7;
                if (success)
                {
                    bekill.info.Jingjie++;
                    DBHelper.Save(bekill.info);
                    await group.OnSendMessage($"{bekill.info.name}虽然惜败，但是在决斗的生死压迫中也感受到了命运的召唤，成功突破境界。");
                }
            }
            if (RandomHelper.Next(2) == 0)
                return;
            var lucky = DBHelper.GetRandomCache();
            if (lucky == null)
                return;
            if (lucky.Jingjie < killer.info.Jingjie)
            {
                var b = new MessageChainBuilder();
                var atmsg = new AtMessage()
                {
                    Target = lucky.id
                };
                PlainMessage msg = null;
                if (lucky.NeedTupo)
                {
                    lucky.Jingjie++;
                    DBHelper.Save(lucky);
                    msg = new PlainMessage()
                    {
                        Text = $"{lucky.name}在旁观决斗时忽然心有所悟，成功突破到了{lucky.JingjieDes}境界"
                    };
                }
                else
                {
                    lucky.Gongli++;
                    DBHelper.Save(lucky);
                    msg = new PlainMessage()
                    {
                        Text = $"{lucky.name}被决斗的余威扫中，身受重伤，不过伤愈后功力竟然略有增长（功力+1({lucky.Gongli})）"
                    };
                }
                b.Append(atmsg).Append(msg);
                await group.SendMessage(b.Build());
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
            if (info.Dazuo >= 100)
            {
                await group.OnSendMessage($"{friend.Name}今天打坐次数已经很多了，修仙为逆天行事，不可操之过急，明日签到之后再继续吧");
                return;
            }
            if (info.XunbaoT > 0)
            {
                await group.OnSendMessage($"{friend.Name}正在寻宝中，不能打坐");
                return;
            }
            if (info.DazuoStartT == 0)
            {
                info.DazuoStartT = ApiDateTime.SecondsFromBegin();
                DBHelper.Save(info);
                await group.OnSendMessage($"{friend.Name}开始打坐");
                return;
            }
            if (ApiDateTime.SecondsFromBegin() - info.DazuoStartT < 60)
            {
                info.DazuoStartT = 0;
                DBHelper.Save(info);
                await group.OnSendMessage($"{friend.Name}停止打坐，打坐不足一分钟，没有什么效果");
                return;
            }
            var s = (int)(ApiDateTime.SecondsFromBegin() - info.DazuoStartT);
            var ic = s / 60;
            var maxdazuot = 10;
            if (info.FudiBattleT > 0 && !info.FudiEnded)
                maxdazuot += 10;
            if (info.DongtianLingqi > 0)
                maxdazuot += 10;
            if (ic > maxdazuot)
                ic = maxdazuot;
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
            await group.OnSendMessage($"{friend.Name}打坐{s}秒，精力+{ic}，目前有精力{info.Energy}" + (dunwu ? $"\n打坐中顿悟，想通了人和宇宙以及植物和僵尸之间的紧密关系，功力+1({info.Gongli})" : ""));
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
                    await group.OnSendMessage($"{friend.Name}正在打坐，不能寻宝");
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
                            await group.OnSendMessage($"{friend.Name}逛街的时候买了一个高达10厘米的手办，功力大增（功力+3（{info.Gongli}））");
                        }
                        else if (irdm == 1)
                        {
                            info.Gongli += 2;
                            await group.OnSendMessage($"{friend.Name}机缘巧合下解开了鸡兔同笼谜题，得到了【兔笼宝刀】一把，功力大增（功力+2（{info.Gongli}））");
                        }
                        else if (irdm == 2)
                        {
                            info.Gongli += 2;
                            await group.OnSendMessage($"{friend.Name}只花了一天时间就逛遍了整个仙界，得到了【一天见】称号，功力大增（功力+2（{info.Gongli}））");
                        }
                        else if (irdm == 3)
                        {
                            info.Gongli++;
                            await group.OnSendMessage($"{friend.Name}寻宝的时候掉下悬崖，摔得遍体鳞伤，不过捡到了一包【因祸德福巧克力】，吃了之后功力+1（{info.Gongli}）");
                        }
                        else if (irdm == 4)
                        {
                            info.Gongli++;
                            await group.OnSendMessage($"{friend.Name}寻宝的时候在D站刷到一个修仙指南视频，看完之后颇有心得，功力+1（{info.Gongli}）");
                        }
                        else if (irdm == 5)
                        {
                            info.Energy += 5;
                            await group.OnSendMessage($"{friend.Name}寻宝累了回自己的洞府美美睡了一觉，精力恢复了5点（{info.Energy}）");
                        }
                        else if (irdm == 6)
                        {
                            var add = RandomHelper.Next(1, (info.Jingjie == 0 ? 1 : info.Jingjie) * 5);
                            info.Qiankunzhu += add;
                            await group.OnSendMessage($"{friend.Name}寻宝的时候发现了一处神兽遗弃的聚集地，翻垃圾桶翻到了{add}个乾坤珠，可喜可贺。");
                        }
                        else
                        {
                            await group.OnSendMessage(GetRandomXunbaoDesc(friend.Name));
                        }
                        DBHelper.Save(info);
                    }
                    else
                    {
                        await group.OnSendMessage($"{friend.Name}寻宝中，目前一无所获");
                    }
                    return;
                }
                if (info.Energy < 10)
                {
                    await group.OnSendMessage($"{friend.Name}精力不足10点，不能寻宝");
                    return;
                }
                if (info.NeedTupo)
                {
                    await group.OnSendMessage($"{friend.Name}功力达到瓶颈，需要突破当前境界之后才能继续寻宝。");
                    return;
                }
                info.Energy -= 10;
                info.XunbaoT = ApiDateTime.SecondsFromBegin();
                info.XunbaoEndT = info.XunbaoT + RandomHelper.Next(150) + 150;
                DBHelper.Save(info);
                var t = ApiDateTime.ToTime(info.XunbaoEndT);
                await group.OnSendMessage($"{friend.Name}开始寻宝，精力-10(目前剩余{info.Energy})，大概{t.ToString("HH:mm")}寻宝结束");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[寻宝Error]{ex}");
            }
        }

        [GroupMessage("打招呼")]
        public static async void OnDujie(Group group, Member friend, string target)
        {
            if (string.IsNullOrEmpty(target))
                return;
            var info = DBHelper.Get(target);
            if (info == null)
                return;
            var p1 = new PlainMessage($"{friend.Name}向");
            var at = new AtMessage() { Target = target };
            var p2 = new PlainMessage($"招了招手");
            var b = new MessageChainBuilder();
            b.Append(p1).Append(at).Append(p2);
            await group.SendMessage(b.Build());
        }

        [GroupMessage("渡劫")]
        public static async void OnDujie(Group group, Member friend)
        {
            if (GroupHelper.Invalid(group.Id))
                return;
            var info = DBHelper.GetOrCreateOne(friend.Id, friend.Name);
            if (!info.NeedTupo)
            {
                await group.OnSendMessage($"道友功力尚浅，贸然渡劫恐伤及仙根，影响日后的修行。");
                return;
            }
            var success = RandomHelper.Next(info.MaxGongli) < 10 + info.DujieExtra * 0.01 * info.MaxGongli + (info.Gongli - info.MaxGongli);
            if (success)
            {
                info.Jingjie++;
                info.DujieExtra = 0;
                DBHelper.Save(info);
                await group.OnSendMessage($"{friend.Name}渡劫成功，晋升{info.JingjieDes}境，喜大普奔，红旗招展，鞭炮齐鸣，人山人海，一举成为全村最靓的仔。");
                return;
            }
            info.Gongli = info.MaxGongli - 1;
            DBHelper.Save(info);
            await group.OnSendMessage($"{friend.Name}渡劫失败，遭受反噬，功力大减（{info.Gongli}）。");
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
            if (GroupHelper.Invalid(group.Id))
                return;
            await group.OnSendMessage($"{friend.Name} 独上青楼听艳曲，手擎玉箫无人吹！");
        }

        [GroupMessage("信息")]
        public static async void OnXinxi(Group group, Member friend, string target)
        {
            if (GroupHelper.Invalid(group.Id))
                return;
            Info info = null;
            if (!string.IsNullOrEmpty(target))
                info = DBHelper.Get(target);
            else
                info = DBHelper.GetOrCreateOne(friend.Id, friend.Name);
            if (info == null)
                return;
            var res = $"【{info.name}】\n"
                + $"功力：{info.Gongli}\n"
                + $"境界：{info.JingjieDes}\n"
                + $"精力：{info.Energy}\n"
                + $"决斗胜利：{info.JuedouWin}\n"
                + $"打坐进度：{info.Dazuo}/100\n"
                + $"灵气：{info.Lingqi}\n"
                + $"洞天灵气：{info.DongtianLingqi}\n"
                + FudiInfo(info)
                ;
            var b = new MessageChainBuilder();
            var m = new ImageMessage() { Base64 = Convert.ToBase64String(System.IO.File.ReadAllBytes("logo.png")) };
            b.Append(m).Append(new PlainMessage() { Text = res });
            await group.SendMessage(b.Build());
        }

        private static string FudiInfo(Info info)
        {
            var res = $"现在持有{info.Qiankunzhu}个乾坤珠\n";
            if (!info.FudiEnded)
                if (info.FudiLastGongli > 0)
                    res += $"福地（{info.FudiLayer}）中的神兽驯服状态为{info.FudiLastGongli}/{info.FudiNeedGongli}，已找到{info.FudiQiankunzhu}个乾坤珠\n";
                else
                    res += $"福地（{info.FudiLayer}）中剩余狂暴灵气{info.FudiNeedLingqi}，已找到{info.FudiQiankunzhu}个乾坤珠\n";
            return res;
        }

        [GroupMessage("功力榜")]
        public static async void OnGonglibang(Group group, Member friend)
        {
            if (GroupHelper.Invalid(group.Id))
                return;
            var ress = DBHelper.GetRank("Gongli");
            var res = "《烦人修仙传》功力榜\n";
            foreach (var kv in ress)
            {
                res += $"【{kv.Key + 1}】{kv.Value.name} 功力：({kv.Value.Gongli})\n";
            }
            var b = new MessageChainBuilder();
            var m = new ImageMessage() { Base64 = Convert.ToBase64String(System.IO.File.ReadAllBytes("logo.png")) };
            b.Append(m).Append(new PlainMessage() { Text = res });
            await group.SendMessage(b.Build());
        }

        [GroupMessage("决斗榜")]
        public static async void OnJuedoubang(Group group, Member friend)
        {
            if (GroupHelper.Invalid(group.Id))
                return;
            var ress = DBHelper.GetRank("JuedouWin");
            var res = "《烦人修仙传》决斗榜\n";
            foreach (var kv in ress)
            {
                res += $"【{kv.Key + 1}】{kv.Value.name} 决斗胜利 {kv.Value.JuedouWin} 次\n";
            }
            var b = new MessageChainBuilder();
            var m = new ImageMessage() { Base64 = Convert.ToBase64String(System.IO.File.ReadAllBytes("logo.png")) };
            b.Append(m).Append(new PlainMessage() { Text = res });
            await group.SendMessage(b.Build());
        }

        [GroupMessage("境界榜")]
        public static async void OnJingjiebang(Group group, Member friend)
        {
            if (GroupHelper.Invalid(group.Id))
                return;
            var ress = DBHelper.GetRank("Jingjie");
            var res = "《烦人修仙传》境界榜\n";
            foreach (var kv in ress)
            {
                res += $"【{kv.Key + 1}】{kv.Value.name} 境界：{kv.Value.JingjieDes}\n";
            }
            var b = new MessageChainBuilder();
            var m = new ImageMessage() { Base64 = Convert.ToBase64String(System.IO.File.ReadAllBytes("logo.png")) };
            b.Append(m).Append(new PlainMessage() { Text = res });
            await group.SendMessage(b.Build());
        }

        [GroupMessage("灵气榜")]
        public static async void OnLingqi(Group group, Member friend)
        {
            if (GroupHelper.Invalid(group.Id))
                return;
            var ress = DBHelper.GetRank("DongtianLingqi");
            var res = "《烦人修仙传》洞天灵气榜\n";
            foreach (var kv in ress)
            {
                res += $"【{kv.Key + 1}】{kv.Value.name} 洞天灵气：{kv.Value.DongtianLingqi}\n";
            }
            var b = new MessageChainBuilder();
            var m = new ImageMessage() { Base64 = Convert.ToBase64String(System.IO.File.ReadAllBytes("logo.png")) };
            b.Append(m).Append(new PlainMessage() { Text = res });
            await group.SendMessage(b.Build());
        }

        [GroupMessage("福地榜")]
        public static async void OnFudi(Group group, Member friend)
        {
            if (GroupHelper.Invalid(group.Id))
                return;
            var ress = DBHelper.GetRank("FudiMaxLayer");
            var res = "《烦人修仙传》福地榜\n";
            foreach (var kv in ress)
            {
                res += $"【{kv.Key + 1}】{kv.Value.name} 最高福地层数：{kv.Value.FudiMaxLayer}\n";
            }
            var b = new MessageChainBuilder();
            var m = new ImageMessage() { Base64 = Convert.ToBase64String(System.IO.File.ReadAllBytes("logo.png")) };
            b.Append(m).Append(new PlainMessage() { Text = res });
            await group.SendMessage(b.Build());
        }
        [GroupMessage("功能")]
        public static async void OnFunctions(Group group, Member friend)
        {
            if (GroupHelper.Invalid(group.Id))
                return;
            var b = new MessageChainBuilder();
            var m = new ImageMessage() { Base64 = Convert.ToBase64String(System.IO.File.ReadAllBytes("logo.png")) };
            var res = "【打坐】开始/结束打坐，可以增加精力\n" +
                "\t打坐时间越长，越可能顿悟额外增加功力\n" +
                "\t打坐时长最长为10分钟，如有洞天未修炼完，可额外延迟10分钟，如有福地可以探险，可再额外延长10分钟，不影响顿悟几率\n" +
                "【寻宝】开始/结束寻宝，消耗精力概率涨功力\n" +
                "\t寻宝10点精力可以获取0~3点功力或随机数量的乾坤珠\n" +
                "【签到】每天一次，增加精力并重置打坐进度\n" +
                "【信息】查看自己的信息\n" +
                "【洞天】探索洞天，通过修炼提升功力或渡劫成功率\n" +
                "\t修炼10点精力可以固定获取1点功力，如果是需要突破了，则转化为10%渡劫成功率\n" +
                "【三花聚顶】施展三花聚顶神功消耗50点灵气随机汲取洞天中的剩余灵气\n" +
                "【福地】在福地中探险可以获得幻化万物的乾坤珠\n" +
                "【功力榜】【决斗榜】【境界榜】【灵气榜】【福地榜】\n" +
                "\t更多功能敬请期待\n" +
                "《烦人修仙传》烦人，我们是认真的！";
            b.Append(m).Append(new PlainMessage() { Text = res });
            await group.SendMessage(b.Build());
        }
        [GroupMessage("活跃")]
        public static async void OnHuoyue(Group group, Member friend)
        {
            if (GroupHelper.Invalid(group.Id))
                return;
            var c = DBHelper.TodayCount();
            await group.OnSendMessage($"今日活跃{c}");
        }
        [GroupMessage("InitController")]
        public static void InitController(Group group, Member friend)
        {
            DBHelper.InitDB();
            Console.WriteLine($"DB Inited.");
        }
    }
}
