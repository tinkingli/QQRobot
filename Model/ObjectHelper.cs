using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App
{
	public static class ObjectHelper
	{
		public static void Swap<T>(ref T t1, ref T t2)
		{
			(t1, t2) = (t2, t1);
		}
	}
}
