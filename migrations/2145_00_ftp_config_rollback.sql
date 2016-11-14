alter table Customers.FtpConfigs drop foreign key FK_Customers_FtpConfigs_SupplierId;
alter table Customers.FtpConfigs drop foreign key FK_Customers_FtpConfigs_UserId;
drop table if exists Customers.FtpConfigs;
