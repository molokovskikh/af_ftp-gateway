using System;
using Common.Models.Catalogs;

namespace app.Models
{
	public class DocumentLine
	{
		public DocumentLine()
		{
		}

		public virtual uint Id { get; set; }

		public virtual Document Document { get; set; }

		/// <summary>
		/// Если не null, то содержит ссылку на сопоставленный продукт из catalogs.products
		/// </summary>
		public virtual Product ProductEntity { get; set; }

		/// <summary>
		/// Наименование продукта
		/// </summary>
		public virtual string Product { get; set; }

		/// <summary>
		/// Код товара поставщика
		/// </summary>
		public virtual string Code { get; set; }

		/// <summary>
		/// Информация о сертификате это строка что то вроде РОСС.NL.ФМ09.Д00778
		/// </summary>
		public virtual string Certificates { get; set; }

		/// <summary>
		/// Дата сертификата
		/// </summary>
		public virtual string CertificatesDate { get; set; }

		/// <summary>
		/// Орган, выдавший документа качества
		/// </summary>
		public virtual string CertificateAuthority { get; set; }

		/// <summary>
		/// Срок годности. А точнее Дата окончания срока годности.
		/// </summary>
		public virtual string Period { get; set; }

		/// <summary>
		/// Срок годности в месяцах
		/// </summary>
		public virtual int? ExpireInMonths { get; set; }

		/// <summary>
		/// Дата изготовления
		/// </summary>
		public virtual DateTime? DateOfManufacture { get; set; }

		/// <summary>
		/// Id производителя
		/// </summary>
		public virtual int? ProducerId { get; set; }

		/// <summary>
		/// Производитель
		/// </summary>
		public virtual string Producer { get; set; }

		/// <summary>
		/// Страна производителя
		/// </summary>
		public virtual string Country { get; set; }

		/// <summary>
		/// Цена производителя без НДС
		/// </summary>
		public virtual decimal? ProducerCostWithoutNDS { get; set; }

		/// <summary>
		/// Цена государственного реестра
		/// </summary>
		public virtual decimal? RegistryCost { get; set; }

		/// <summary>
		/// Дата регистрации цены в Гос-Реестре
		/// </summary>
		public virtual DateTime? RegistryDate { get; set; }

		/// <summary>
		/// Наценка поставщика
		/// </summary>
		public virtual decimal? SupplierPriceMarkup { get; set; }

		/// <summary>
		/// Ставка налога на добавленную стоимость
		/// </summary>
		public virtual uint? Nds { get; set; }

		/// <summary>
		/// Цена поставщика без НДС
		/// </summary>
		public virtual decimal? SupplierCostWithoutNDS { get; set; }

		/// <summary>
		/// Цена поставщика с НДС
		/// </summary>
		public virtual decimal? SupplierCost { get; set; }

		/// <summary>
		/// Количество
		/// </summary>
		public virtual uint? Quantity { get; set; }

		/// <summary>
		/// Признак ЖНВЛС
		/// </summary>
		public virtual bool? VitallyImportant { get; set; }

		/// <summary>
		/// Серийный номер продукта
		/// </summary>
		public virtual string SerialNumber { get; set; }

		/// <summary>
		/// Сумма НДС
		/// </summary>
		public virtual decimal? NdsAmount { get; set; }

		/// <summary>
		/// Сумма с НДС
		/// </summary>
		public virtual decimal? Amount { get; set; }

		/// <summary>
		/// Единица измерения
		/// </summary>
		public virtual string Unit { get; set; }

		/// <summary>
		/// В том числе акциз
		/// </summary>
		public virtual decimal? ExciseTax { get; set; }

		/// <summary>
		/// № Таможенной декларации
		/// </summary>
		public virtual string BillOfEntryNumber { get; set; }

		/// <summary>
		/// Код EAN-13 (штрих-код)
		/// </summary>
		public virtual string EAN13 { get; set; }

		/// <summary>
		/// Код ОКДП
		/// </summary>
		public virtual string CodeOKDP { get; set; }

		/// <summary>
		/// Имя файла образа сертификата
		/// </summary>
		public virtual string CertificateFilename { get; set; }

		/// <summary>
		/// Имя файла образа протокола
		/// </summary>
		public virtual string ProtocolFilemame { get; set; }

		/// <summary>
		/// Имя файла образа паспорта
		/// </summary>
		public virtual string PassportFilename { get; set; }

		/// <summary>
		/// ошибка говорящая о том почему не удалось загрузить сертификат
		/// </summary>
		public virtual string CertificateError { get; set; }
	}
}