using System;

namespace app.Models
{
	public class Invoice
	{
		public virtual uint Id { get; set; }

		public virtual Document Document { get; set; }

		/// <summary>
		/// Номер счет-фактуры
		/// </summary>
		public virtual string InvoiceNumber { get; set; }

		/// <summary>
		/// Дата счет-фактуры
		/// </summary>
		public virtual DateTime? InvoiceDate { get; set; }

		/// <summary>
		/// Наименование продавца
		/// </summary>
		public virtual string SellerName { get; set; }

		/// <summary>
		/// Адрес продавца
		/// </summary>
		public virtual string SellerAddress { get; set; }

		/// <summary>
		/// ИНН продавца
		/// </summary>
		public virtual string SellerINN { get; set; }

		/// <summary>
		/// КПП продавца
		/// </summary>
		public virtual string SellerKPP { get; set; }

		/// <summary>
		/// Грузоотправитель и его адрес
		/// </summary>
		public virtual string ShipperInfo { get; set; }

		/// <summary>
		/// Название грузополучателя
		/// </summary>
		public virtual string RecipientName { get; set; }

		/// <summary>
		/// Код грузополучателя в кодировке поставщика
		/// </summary>
		public virtual int? RecipientId { get; set; }

		/// <summary>
		/// Грузополучатель и его адрес
		/// </summary>
		public virtual string RecipientAddress { get; set; }

		/// <summary>
		/// Поле К платежно-расчетному документу N
		/// </summary>
		public virtual string PaymentDocumentInfo { get; set; }

		/// <summary>
		/// Код покупателя в кодировки поставщика
		/// </summary>
		public virtual int? BuyerId { get; set; }

		/// <summary>
		/// Наименование покупателя
		/// </summary>
		public virtual string BuyerName { get; set; }

		/// <summary>
		/// Адрес покупателя
		/// </summary>
		public virtual string BuyerAddress { get; set; }

		/// <summary>
		/// ИНН покупателя
		/// </summary>
		public virtual string BuyerINN { get; set; }

		/// <summary>
		/// КПП покупателя
		/// </summary>
		public virtual string BuyerKPP { get; set; }

		/// <summary>
		/// Стоимость товаров для группы товаров, облагаемых ставкой 0% НДС
		/// </summary>
		public virtual decimal? AmountWithoutNDS0 { get; set; }

		/// <summary>
		/// Стоимость товаров без налога для группы товаров, облагаемых ставкой 10% НДС
		/// </summary>
		public virtual decimal? AmountWithoutNDS10 { get; set; }

		/// <summary>
		/// Сумма налога для группы товаров, облагаемых ставкой 10% НДС
		/// </summary>
		public virtual decimal? NDSAmount10 { get; set; }

		/// <summary>
		/// Стоимость товаров для группы товаров, облагаемых ставкой 10% НДС всего с учетом налога
		/// </summary>
		public virtual decimal? Amount10 { get; set; }

		/// <summary>
		/// Стоимость товаров без налога для группы товаров, облагаемых ставкой 18% НДС
		/// </summary>
		public virtual decimal? AmountWithoutNDS18 { get; set; }

		/// <summary>
		/// Сумма налога для группы товаров, облагаемых ставкой 18% НДС
		/// </summary>
		public virtual decimal? NDSAmount18 { get; set; }

		/// <summary>
		/// Стоимость товаров для группы товаров , облагаемых ставкой 18% НДС всего с учетом налога
		/// </summary>
		public virtual decimal? Amount18 { get; set; }

		/// <summary>
		/// Общая стоимость товаров без налога (указывается в конце таблицы счет-фактуры по строке «ИТОГО»)
		/// </summary>
		public virtual decimal? AmountWithoutNDS { get; set; }

		/// <summary>
		/// Общая сумма налога (указывается в конце таблицы счет-фактуры по строке «ИТОГО»)
		/// </summary>
		public virtual decimal? NDSAmount { get; set; }

		/// <summary>
		/// Общая стоимость товаров с налогом (указывается в конце таблицы счет-фактуры по строке «ИТОГО»)
		/// </summary>
		public virtual decimal? Amount { get; set; }

		/// <summary>
		/// Отсрочка платежа (календарные дни)
		/// </summary>
		public virtual int? DelayOfPaymentInDays { get; set; }

		/// <summary>
		/// Отсрочка платежа (банковские дни)
		/// </summary>
		public virtual int? DelayOfPaymentInBankDays { get; set; }

		/// <summary>
		/// Номер договора (комиссии)
		/// </summary>
		public virtual string CommissionFeeContractId { get; set; }

		/// <summary>
		/// Ставка комиссионного вознаграждения
		/// </summary>
		public virtual decimal? CommissionFee { get; set; }
	}}