using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App
{
    public class Info
    {
        public string id;
        public string name;
        public long CreateTime;
        public long UpdateTime;
        public long Energy;
        public long MaxEnergy;
        public int Dazuo;
        public long DazuoStartT;
        public long DailyCheckT;
        public long XunbaoT;
        public long XunbaoEndT;
        public int Gongli;
        public int JuedouWin;
        public int Jingjie;
        public int Lingqi;
        public int DongtianLingqi;
        public long DongtianStartT;
        public long DongtianEndT;
        public int DujieExtra;
        public long FudiStartT;
        public bool FudiEnded;
        public int FudiLastGongli;
        public long FudiBattleT;
        public int FudiNeedGongli;
        public Dictionary<int, int> FudiRewards;
        public int FudiLayer; //福地层数
        public int FudiBlockLayer;
        public int FudiNeedLingqi;
        public int FudiQiankunzhu;
        public int FudiNum1;
        public int FudiNum2;
        public int Qiankunzhu;
        public long QiankunzhuCDT;
        public int FudiMaxLayer;
        public static int[] JingjieFenduan = { 10, 100, 1_000, 10_000, 100_000, 1_000_000, 10_000_000 };
        public static string[] JingjieDescs = { "炼气", "先天", "金丹", "元婴", "化神", "返虚", "合道" };
        public int FormerJieduanMax
        {
            get
            {
                if (Jingjie < 1)
                    return 0;
                return Info.JingjieFenduan[Jingjie - 1]; ;
            }
        }
        public string JingjieDes
        {
            get
            {
                if (Jingjie >= JingjieDescs.Length)
                    return JingjieDescs[Jingjie - 1];
                return JingjieDescs[Jingjie];
            }
        }
        public int MaxGongli
        {
            get
            {
                if (Jingjie >= JingjieFenduan.Length)
                    return JingjieFenduan[Jingjie - 1];
                return JingjieFenduan[Jingjie];
            }
        }
        public bool NeedTupo
        {
            get
            {
                if (Jingjie >= JingjieFenduan.Length)
                    return true;
                return Gongli >= JingjieFenduan[Jingjie];
            }
        }
    }
}
