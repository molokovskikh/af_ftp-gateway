use customers;
ALTER TABLE `clients`
	ADD COLUMN `FtpIntegration` TINYINT(1) UNSIGNED NOT NULL DEFAULT '0' AFTER `SwapFirmCode`;
	
CREATE TABLE `WebFtpOutsiders` (
	`Id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
	`Login` VARCHAR(50) NOT NULL,
	`Name` VARCHAR(100) NOT NULL,
	`Enabled` TINYINT(1) NOT NULL DEFAULT '0',
	PRIMARY KEY (`Id`)
)
COLLATE='cp1251_general_ci'
ENGINE=InnoDB
;
