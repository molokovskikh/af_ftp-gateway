using System.Collections.Generic;
using web_app.Services;

namespace web_app.Helper
{
	public class Paginator<T>
	{
		public Paginator(IList<T> collection, int pageCurrent = 1, int itemsPerPage = 50)
		{
			List = collection ?? new List<T>();
			ItemsPerPage = itemsPerPage;
			PageCurrent = pageCurrent;
		}

		public Paginator(int totalItems, int pageCurrent = 1, int itemsPerPage = 50)
		{
			List = new List<T>();
			ItemsTotalProp = totalItems;
			ItemsPerPage = itemsPerPage;
			PageCurrent = pageCurrent;
		}

		public IList<T> List { get; private set; }
		public int ItemsTotal => ItemsTotalProp == 0 ? List.Count : ItemsTotalProp;
		private int ItemsTotalProp { get; set; }
		public int ItemsPerPage { get; private set; }

		public int ItemCurrent =>
			ItemsTotal > 0 && PageCurrent * ItemsPerPage < 0 ? PageFirst * ItemsPerPage :
				ItemsTotal > 0 && PageCurrent * ItemsPerPage > ItemsTotal ? (PagesTotal - 1) * ItemsPerPage :
					((PageCurrent * ItemsPerPage) - ItemsPerPage);

		public int PageCurrent { get; private set; }
		public int PageFirst => 1;
		public int PagesTotal => ((ItemsTotal / ItemsPerPage) + (ItemsTotal % ItemsPerPage > 0 ? 1 : 0));

		public void SetTotalItems(int number)
		{
			ItemsTotalProp = number;
		}

		public void SetList(IList<T> collection)
		{
			List = collection;
		}
	}
}