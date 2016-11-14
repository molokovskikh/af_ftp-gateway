using System;
using System.Collections.Generic;
using System.ComponentModel;
using Common.Models;

namespace app.Models
{
	public enum DocType
	{
		Docs,
		[Description("Накладная")] Waybill = 1,
		[Description("Отказ")] Reject = 2
	}

	public class Document
	{
		public Document()
		{
			Lines = new List<DocumentLine>();
		}

		public virtual uint Id { get; set; }

		public virtual DateTime WriteTime { get; set; }

		public virtual Supplier Supplier { get; set; }

		public virtual uint ClientCode { get; set; }

		public virtual Address Address { get; set; }

		public virtual DocType DocumentType { get; set; }

		/// <summary>
		/// Номер накладной.
		/// </summary>
		public virtual string ProviderDocumentId { get; set; }

		/// <summary>
		/// Дата накладной.
		/// </summary>
		public virtual DateTime? DocumentDate { get; set; }

		/// <summary>
		/// наш номер заявки, на основании кот. сформирована накладная
		/// </summary>
		public virtual uint? OrderId { get; set; }

		public virtual IList<DocumentLine> Lines { get; set; }

		public virtual Invoice Invoice { get; set; }
	}
}