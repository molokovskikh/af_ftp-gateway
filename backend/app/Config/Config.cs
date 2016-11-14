using System;
using System.Collections.Specialized;

namespace app.Config
{
	public class Config
	{
		public string RootDir;
		public TimeSpan LookupTime;
		public uint SupplierId;
		public string FtpExportPlan;
		public string FtpImportPlan;

		public Config()
		{
			FtpExportPlan = "0 0 8,15 * * ?";
			FtpImportPlan = "0 0 * * * ?";
		}

		public Config(NameValueCollection appSettings)
			: this()
		{
			RootDir = appSettings["RootDir"];
			LookupTime = TimeSpan.Parse(appSettings["LookupTime"]);
			SupplierId = Convert.ToUInt32(appSettings["SupplierId"]);
			FtpExportPlan = appSettings["FtpExportPlan"] ?? FtpExportPlan;
			FtpImportPlan = appSettings["FtpImportPlan"] ?? FtpImportPlan;
		}
	}
}