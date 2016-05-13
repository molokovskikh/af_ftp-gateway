using System.Collections.Generic;

namespace app.Models
{
	public class Reject
	{
		public string PredId;
		public string DepId;
		public string To;
		public string From;
		public uint OrderId;

		public List<RejectItem> Items = new List<RejectItem>();
	}

	public class RejectItem
	{
		public RejectItem(uint orderId, string code, uint quantity, string name, decimal price, ulong offerId)
		{
			OrderId = orderId.ToString();
			Code = code;
			Quantity = quantity;
			Name = name;
			Price = price;
			OfferId = offerId;
		}

		public string OrderId;
		public string Code;
		public uint Quantity;
		public string Name;
		public decimal Price;
		public ulong OfferId;
	}
}