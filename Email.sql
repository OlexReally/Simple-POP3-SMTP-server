-- --------------------------------------------------------
-- Сервер:                       127.0.0.1
-- Версія сервера:               5.5.23 - MySQL Community Server (GPL)
-- ОС сервера:                   Win64
-- HeidiSQL Версія:              9.2.0.4947
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8mb4 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;

-- Dumping database structure for mail
CREATE DATABASE IF NOT EXISTS `mail` /*!40100 DEFAULT CHARACTER SET cp1251 COLLATE cp1251_ukrainian_ci */;
USE `mail`;


-- Dumping structure for таблиця mail.email
CREATE TABLE IF NOT EXISTS `email` (
  `login` char(50) COLLATE cp1251_ukrainian_ci DEFAULT NULL,
  `date` timestamp NULL DEFAULT NULL,
  `delete` int(11) DEFAULT '0',
  `read` int(11) DEFAULT '0',
  `text` text COLLATE cp1251_ukrainian_ci,
  KEY `FK_email_users` (`login`),
  CONSTRAINT `FK_email_users` FOREIGN KEY (`login`) REFERENCES `users` (`login`)
) ENGINE=InnoDB DEFAULT CHARSET=cp1251 COLLATE=cp1251_ukrainian_ci COMMENT='users email';

-- Dumping data for table mail.email: ~10 rows (приблизно)
/*!40000 ALTER TABLE `email` DISABLE KEYS */;
INSERT INTO `email` (`login`, `date`, `delete`, `read`, `text`) VALUES
	('admin', '2015-05-28 19:38:31', 0, 0, 'This is first email message for admin'),
	('admin', '2015-05-28 19:40:27', 0, 0, 'This is second email message for admin'),
	('serg', '2015-05-28 19:50:44', 0, 0, 'This is first email message for serg'),
	('serg', '2015-05-28 19:52:40', 0, 0, 'This is second email message for serg'),
	('olex', '2015-05-31 21:01:01', 0, 0, 'new text'),
	('olex', '2015-05-31 21:19:15', 0, 0, 'TEST MESSAGE!!!.'),
	('olex', '2015-05-31 23:59:17', 0, 0, 'Test message! HI!!!\nQWERTY\n'),
	('olex', '2015-06-01 00:48:32', 0, 0, 'THIS IS TEXT FOR ALL USERS!\nHELLO ALL!!!\n'),
	('admin', '2015-06-01 00:48:32', 0, 0, 'THIS IS TEXT FOR ALL USERS!\nHELLO ALL!!!\n'),
	('serg', '2015-06-01 00:48:32', 0, 0, 'THIS IS TEXT FOR ALL USERS!\nHELLO ALL!!!\n');
/*!40000 ALTER TABLE `email` ENABLE KEYS */;


-- Dumping structure for таблиця mail.olex
CREATE TABLE IF NOT EXISTS `olex` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `date` datetime DEFAULT NULL,
  `delete` int(11) DEFAULT NULL,
  `read` int(11) DEFAULT NULL,
  `text` text COLLATE cp1251_ukrainian_ci,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=cp1251 COLLATE=cp1251_ukrainian_ci;

-- Dumping data for table mail.olex: ~3 rows (приблизно)
/*!40000 ALTER TABLE `olex` DISABLE KEYS */;
INSERT INTO `olex` (`id`, `date`, `delete`, `read`, `text`) VALUES
	(1, '2015-05-30 20:22:46', 0, 0, 'qweqweqw'),
	(3, '2015-05-30 20:22:46', 0, 0, 'qweqweqw'),
	(4, '2015-05-30 20:22:46', 0, 0, 'qweqweqw');
/*!40000 ALTER TABLE `olex` ENABLE KEYS */;


-- Dumping structure for таблиця mail.users
CREATE TABLE IF NOT EXISTS `users` (
  `login` char(50) COLLATE cp1251_ukrainian_ci NOT NULL,
  `password` varchar(50) COLLATE cp1251_ukrainian_ci DEFAULT NULL,
  `email` varchar(50) COLLATE cp1251_ukrainian_ci DEFAULT NULL,
  PRIMARY KEY (`login`)
) ENGINE=InnoDB DEFAULT CHARSET=cp1251 COLLATE=cp1251_ukrainian_ci COMMENT='logins/pass''s';

-- Dumping data for table mail.users: ~3 rows (приблизно)
/*!40000 ALTER TABLE `users` DISABLE KEYS */;
INSERT INTO `users` (`login`, `password`, `email`) VALUES
	('admin', 'admin', 'admin@admin.nulp'),
	('olex', 'olex', 'olex@olex.ua'),
	('serg', 'serg', 'serg@yandex.ua');
/*!40000 ALTER TABLE `users` ENABLE KEYS */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IF(@OLD_FOREIGN_KEY_CHECKS IS NULL, 1, @OLD_FOREIGN_KEY_CHECKS) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
