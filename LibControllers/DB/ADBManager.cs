using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using System.Threading;

public static class ADBManager
{
	private static Dictionary<string, ADBAccessor> dDbPool = new Dictionary<string, ADBAccessor>();
	public static ADBAccessor Get(string dbConnect, string dbName)
	{
		if (!dDbPool.ContainsKey(dbConnect + "." + dbName))
		{
			var _DB = new ADBAccessor(dbConnect, dbName);
			dDbPool.Add(dbConnect + "." + dbName, _DB);
		}
		return dDbPool[dbConnect + "." + dbName];
	}

	private static Dictionary<string, MongoClient> m_vMongoServer = new Dictionary<string, MongoClient>();
	private static MongoClient GetMongoServer(String sInitString)
	{
		if (!m_vMongoServer.ContainsKey(sInitString))
		{
			MongoClient mc = new MongoClient(sInitString);
			m_vMongoServer.Add(sInitString, mc);
		}
		return m_vMongoServer[sInitString];
	}
	public static IMongoDatabase GetDB(string sConnect, string sDbName)
	{
		try
		{
			return GetMongoServer(sConnect).GetDatabase(sDbName);
		}
		catch
		{
			return null;
		}
	}
	public static void RemoteRequestDB(Action<object> remoteAction)
	{
		ThreadPool.QueueUserWorkItem(new WaitCallback(remoteAction));
	}
}