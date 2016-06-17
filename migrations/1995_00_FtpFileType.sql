use customers;
 alter TABLE `customers`.`users` add column `FtpFileType` TINYINT(4) NOT NULL DEFAULT '0' after `UseFtpGateway`;
