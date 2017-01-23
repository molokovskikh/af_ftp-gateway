using System;
using Common.Models;

namespace app.Models
{
	public class DocumentLog
	{
		public virtual uint Id { get; set; }
		public virtual Address Address { get; set; }
		public virtual Common.Models.DocType DocumentType { get; set; }
		public virtual Supplier Supplier { get; set; }
		public virtual string Filename { get; set; }
		public virtual bool IsFake { get; set; }
		public virtual bool PreserveFilename { get; set; }
		public virtual DateTime LogTime { get; set; }
	}

	public class DocumentSendLog
	{
		public virtual uint Id { get; set; }
		public virtual User User { get; set; }
		public virtual DocumentLog Document { get; set; }
		public virtual bool Committed { get; set; }
		public virtual bool DocumentDelivered { get; set; }
		public virtual bool FileDelivered { get; set; }
		public virtual DateTime? SendDate { get; set; }

		public void Commit()
		{
			Committed = true;
			DocumentDelivered = true;
			SendDate = DateTime.Now;
		}
	}
}