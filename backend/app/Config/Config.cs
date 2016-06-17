using System;
using System.Collections.Specialized;

namespace app.Config
{
	public class Config
	{
		public string RootDir;
		public TimeSpan LookupTime;
		public uint SupplierId;
		public int FtpFileType;

		public Config()
		{
		}

		public Config(NameValueCollection appSettings)
		{
			RootDir = appSettings["RootDir"];
			LookupTime = TimeSpan.Parse(appSettings["LookupTime"]);
			SupplierId = Convert.ToUInt32(appSettings["SupplierId"]);
		}
	}
}