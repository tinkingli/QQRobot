using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace App
{
	public static class DBHelper
	{
		private static ADBAccessor DB
		{
			get
			{
				return ADBManager.Get("mongodb://127.0.0.1:27018", "xiuxian");
			}
		}
		private static Dictionary<string, Info> dCache = new Dictionary<string, Info>();
		public static Info Get(string id)
		{
			if (dCache.ContainsKey(id))
				return dCache[id];
			return DB.FindOneData<Info>(id);
		}
		public static Info GetOrCreateOne(string id, string name)
		{
			if (dCache.ContainsKey(id))
				return dCache[id];
			var info = DB.FindOneData<Info>(id);
			if (info == null)
			{
				info = new Info();
				info.id = id;
				info.name = name;
				if (info.CreateTime == 0)
					info.CreateTime = ApiDateTime.SecondsFromBegin();
				info.UpdateTime = ApiDateTime.SecondsFromBegin();
				Save(info);
			}
			else if (!string.IsNullOrEmpty(name))
				info.name = name;
			dCache[id] = info;
			return info;
		}
		public static void Save(Info info)
		{
			DB.UpdateOneData(info);
			dCache[info.id] = info;
		}
		internal static Dictionary<int, Info> GetRank(string v)
		{
			var res = DB.FindManyData("Info", ADBAccessor.filter_Gt(v, 0), ADBAccessor.projections("name", v), 9, 0, ADBAccessor.sort_Descending(v));
			var l = new Dictionary<int, Info>();
			for (var i = 0; i < res.Count; i++)
			{
				var info = new Info();
				l[i] = info;
				info.name = res[i]["name"]?.ToString();
				switch (v)
				{
					case nameof(info.Gongli):
						info.Gongli = res[i]["Gongli"].ToInt32();
						break;
					case nameof(info.Energy):
						info.Energy = res[i]["Energy"].ToInt32();
						break;
					case nameof(info.JuedouWin):
						info.JuedouWin = res[i]["JuedouWin"].ToInt32();
						break;
					case nameof(info.Jingjie):
						info.Jingjie = res[i]["Jingjie"].ToInt32();
						break;
				}
			}
			return l;
		}
		public static Info GetRandomCache()
		{
			if (dCache.Count == 0)
				return null;
			var keys = dCache.Keys.ToArray();
			return dCache[keys[RandomHelper.Next(keys.Length)]];
		}
	}
}
