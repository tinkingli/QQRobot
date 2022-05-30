using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class BaseAttribute : Attribute
{
	public string name;
	public BaseAttribute(string name)
	{
		this.name = name;
	}
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class GroupMessageAttribute : BaseAttribute
{
	public GroupMessageAttribute(string name = "") : base(name)
	{
	}
}
public class FriendMessageAttribute : BaseAttribute
{
	public FriendMessageAttribute(string name = "") : base(name)
	{
	}
}
