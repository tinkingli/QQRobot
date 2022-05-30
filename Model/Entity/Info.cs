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
		public static int[] JingjieFenduan = { 10, 100, 1_000, 10_000, 100_000, 1_000_000, 10_000_000 };
		public static string[] JingjieDescs = { "炼气", "先天", "金丹", "元婴", "化神", "返虚", "合道" };
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
