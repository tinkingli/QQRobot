using Mirai.Net.Data.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace App
{
	public class EventSystem
	{
		private static EventSystem sinstance;
		public static EventSystem Instance
		{
			get
			{
				if (sinstance == null)
					sinstance = new EventSystem();
				return sinstance;
			}
		}
		private readonly Dictionary<string, MethodInfo> allMethod = new Dictionary<string, MethodInfo>();
		private readonly Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();
		public void Add(Assembly assembly)
		{
			assemblies[$"{assembly.GetName().Name}.dll"] = assembly;
			allMethod.Clear();

			foreach (Assembly ass in this.assemblies.Values)
			{
				foreach (Type type in ass.GetTypes())
				{
					var amethods = type.GetMethods();
					foreach (var m in amethods)
					{
						var attribute = m.GetCustomAttribute<BaseAttribute>();
						if (attribute == null)
							continue;
						allMethod[(string.IsNullOrEmpty(attribute.name) ? m.Name : attribute.name).ToLower()] = m;
					}
				}
			}
			Console.WriteLine($"Loaded {allMethod.Count} methods");
		}
		public void InvokeMothod(Friend friend, string name, string extra)
		{
			try
			{
				var m = GetMothod(name);
				if (m == null)
				{
					m = GetMothod("Normal");
					extra = name + " " + extra;
					if (m == null)
						return;
				}

				var objs = new object[m.GetParameters().Length];
				objs[0] = friend;
				if (objs.Length > 1)
					objs[1] = extra;
				m.Invoke(null, objs);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[InvokeMothod - friend]{name} Invoke Error:{ex.Message}");
				Console.WriteLine(ex.StackTrace);
			}
		}
		public void InvokeMothod(Group group, Member member, string name, string extra)
		{
			try
			{
				var m = GetMothod(name);
				if (m == null)
				{
					m = GetMothod("Normal");
					extra = name + " " + extra;
					if (m == null)
						return;
				}

				var objs = new object[m.GetParameters().Length];
				objs[0] = group;
				objs[1] = member;
				if (objs.Length > 2)
					objs[2] = extra;
				m.Invoke(null, objs);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[InvokeMothod - member]{name} Invoke Error:{ex.Message}");
				Console.WriteLine(ex.StackTrace);
			}
		}
		public MethodInfo GetMothod(string name)
		{
			MethodInfo m = null;
			if (allMethod.TryGetValue(name.ToLower(), out m))
				return m;
			return null;
		}
	}
}
