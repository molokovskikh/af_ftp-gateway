create table Customers.FtpConfigs (
        Id INTEGER NOT NULL AUTO_INCREMENT,
       UserId INTEGER UNSIGNED,
       SupplierId INTEGER UNSIGNED,
       PriceUrl VARCHAR(255),
       WaybillUrl VARCHAR(255),
       OrderUrl VARCHAR(255),
       primary key (Id)
    );
alter table Customers.FtpConfigs add index (UserId), add constraint FK_Customers_FtpConfigs_UserId foreign key (UserId) references Customers.Users (Id);
alter table Customers.FtpConfigs add index (SupplierId), add constraint FK_Customers_FtpConfigs_SupplierId foreign key (SupplierId) references Customers.Suppliers (Id);
