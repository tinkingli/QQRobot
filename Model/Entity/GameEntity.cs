using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App
{
	public class JuedouCell
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

	public class Juedou
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
		public const int WaitSec = 60;
		public bool LimitJingjie = true;
	}


}
