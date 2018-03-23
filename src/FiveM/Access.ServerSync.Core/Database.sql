
/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `DevServerState`
--

DROP TABLE IF EXISTS `DevServerState`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `DevServerState` (
  `IP` varchar(45) NOT NULL,
  `State` varchar(16) DEFAULT NULL,
  PRIMARY KEY (`IP`),
  KEY `IP` (`IP`),
  KEY `State` (`State`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ForcedWhitelist`
--

DROP TABLE IF EXISTS `ForcedWhitelist`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ForcedWhitelist` (
  `IP` varchar(45) NOT NULL,
  `Comment` varchar(255) NOT NULL DEFAULT 'Unspecified user',
  `ForceOnPublic` tinyint(4) NOT NULL DEFAULT '1',
  `ForceOnWhitelist` tinyint(4) DEFAULT '0',
  `ForceOnDev` tinyint(4) DEFAULT '0',
  PRIMARY KEY (`IP`),
  UNIQUE KEY `IP_UNIQUE` (`IP`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ForumServerIpViewLog`
--

DROP TABLE IF EXISTS `ForumServerIpViewLog`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ForumServerIpViewLog` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `Date` datetime DEFAULT CURRENT_TIMESTAMP,
  `ForumId` int(11) DEFAULT NULL,
  `ViewedIp` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=17 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `FullSpectrumDevWhitelist`
--

DROP TABLE IF EXISTS `FullSpectrumDevWhitelist`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `FullSpectrumDevWhitelist` (
  `IP` varchar(45) NOT NULL,
  `State` varchar(20) DEFAULT NULL,
  PRIMARY KEY (`IP`),
  KEY `State` (`State`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `GroupMembership`
--

DROP TABLE IF EXISTS `GroupMembership`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `GroupMembership` (
  `ForumId` int(11) NOT NULL,
  `Group` int(11) NOT NULL,
  PRIMARY KEY (`ForumId`,`Group`),
  KEY `Group` (`Group`),
  KEY `ForumId` (`ForumId`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `GtaAccounts`
--

DROP TABLE IF EXISTS `GtaAccounts`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `GtaAccounts` (
  `GtaLicense` varchar(60) NOT NULL,
  `UserForumId` int(11) DEFAULT NULL,
  `IsBanned` tinyint(4) DEFAULT NULL,
  `RowCreated` datetime DEFAULT CURRENT_TIMESTAMP,
  `RowUpdated` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `BannedUntil` datetime DEFAULT '0001-01-01 00:00:00',
  PRIMARY KEY (`GtaLicense`),
  KEY `RowUpdated` (`RowUpdated`),
  KEY `ForumId` (`UserForumId`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `IpAddresses`
--

DROP TABLE IF EXISTS `IpAddresses`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `IpAddresses` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `ForumId` int(11) DEFAULT NULL,
  `IP` varchar(45) DEFAULT NULL,
  `IsBanned` int(11) DEFAULT NULL,
  `RowCreated` datetime DEFAULT NULL,
  `RowUpdated` datetime DEFAULT NULL,
  `BannedUntil` datetime DEFAULT '0001-01-01 00:00:00',
  PRIMARY KEY (`id`),
  KEY `IP` (`IP`),
  KEY `RowUpdated` (`RowUpdated`),
  KEY `IsBanned` (`IsBanned`)
) ENGINE=InnoDB AUTO_INCREMENT=27767 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Log`
--

DROP TABLE IF EXISTS `Log`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Log` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `Date` datetime DEFAULT NULL,
  `AccountType` text,
  `ChangeType` text,
  `OldValue` text,
  `NewValue` text,
  `ForumId` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `Date` (`Date`),
  KEY `ForumId` (`ForumId`)
) ENGINE=InnoDB AUTO_INCREMENT=102302 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ParsedDiscordAccounts`
--

DROP TABLE IF EXISTS `ParsedDiscordAccounts`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ParsedDiscordAccounts` (
  `DiscordId` varchar(20) NOT NULL,
  `DiscordCreated` datetime DEFAULT NULL,
  `RowCreated` datetime DEFAULT NULL,
  `DiscordUsername` varchar(255) DEFAULT NULL,
  `DiscordDiscriminator` varchar(10) DEFAULT NULL,
  `RowUpdated` datetime DEFAULT NULL,
  `IsParsed` int(1) DEFAULT NULL,
  `IsBanned` int(1) DEFAULT NULL,
  `ShouldParserIgnore` int(1) DEFAULT '0',
  `BannedUntil` datetime DEFAULT '0001-01-01 00:00:00',
  PRIMARY KEY (`DiscordId`),
  KEY `IsParsed` (`IsParsed`),
  KEY `ShouldParserIgnore` (`ShouldParserIgnore`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ParsedSteamAccounts`
--

DROP TABLE IF EXISTS `ParsedSteamAccounts`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ParsedSteamAccounts` (
  `SteamId` varchar(45) NOT NULL,
  `IsParsed` int(11) NOT NULL DEFAULT '0',
  `SteamName` varchar(255) DEFAULT NULL,
  `SteamVisibility` varchar(45) DEFAULT NULL,
  `SteamCreated` datetime DEFAULT NULL,
  `RowCreated` datetime DEFAULT NULL,
  `IsBanned` int(1) DEFAULT NULL,
  `SteamBans` int(11) DEFAULT NULL,
  `RowUpdated` datetime DEFAULT NULL,
  `NumSteamFriends` int(11) DEFAULT NULL,
  `ShouldParserIgnore` int(1) DEFAULT '0',
  `BannedUntil` datetime DEFAULT '0001-01-01 00:00:00',
  PRIMARY KEY (`SteamId`),
  KEY `IsParsed` (`IsParsed`),
  KEY `IsBanned` (`IsBanned`),
  KEY `ShouldParserIgnore` (`ShouldParserIgnore`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ParsedTwitchAccounts`
--

DROP TABLE IF EXISTS `ParsedTwitchAccounts`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ParsedTwitchAccounts` (
  `TwitchId` int(11) NOT NULL,
  `IsParsed` int(1) DEFAULT NULL,
  `TwitchName` varchar(45) DEFAULT NULL,
  `TwitchFollowerCount` int(11) DEFAULT NULL,
  `TwitchCreated` datetime DEFAULT NULL,
  `RowCreated` datetime DEFAULT NULL,
  `RowUpdated` datetime DEFAULT NULL,
  `IsBanned` int(1) DEFAULT NULL,
  `ShouldParserIgnore` int(1) DEFAULT '0',
  `BannedUntil` datetime DEFAULT '0001-01-01 00:00:00',
  PRIMARY KEY (`TwitchId`),
  KEY `IsParsed` (`IsParsed`),
  KEY `ShouldParserIgnore` (`ShouldParserIgnore`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `PublicServerState`
--

DROP TABLE IF EXISTS `PublicServerState`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `PublicServerState` (
  `IP` varchar(45) NOT NULL,
  `State` varchar(20) NOT NULL,
  PRIMARY KEY (`IP`),
  KEY `IP` (`IP`),
  KEY `State` (`State`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ServerWhitelistLog`
--

DROP TABLE IF EXISTS `ServerWhitelistLog`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ServerWhitelistLog` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Date` datetime DEFAULT NULL,
  `ActionType` varchar(45) DEFAULT NULL,
  `Ip` varchar(45) DEFAULT NULL,
  `WhitelistType` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `Ip` (`Ip`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COMMENT='ServerWhitelistLog (Date, ActionType, Ip)';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Users`
--

DROP TABLE IF EXISTS `Users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Users` (
  `ForumId` int(11) NOT NULL,
  `SteamId` varchar(45) DEFAULT NULL,
  `DiscordId` varchar(45) DEFAULT NULL,
  `TwitchId` int(11) DEFAULT NULL,
  `ForumBanned` int(11) DEFAULT NULL,
  `ForumPostCount` int(11) DEFAULT NULL,
  `ServerBanned` int(11) DEFAULT NULL,
  `ForumPmCount` int(11) DEFAULT NULL,
  `ForumGroups` varchar(45) DEFAULT NULL,
  `IsAdmin` int(11) DEFAULT NULL,
  `IsDev` int(11) DEFAULT NULL,
  `IsPolice` int(11) DEFAULT NULL,
  `IsEMS` int(11) DEFAULT NULL,
  `IsFireDept` int(11) DEFAULT NULL,
  `LastLoggedInForum` datetime DEFAULT NULL,
  `LastLoggedInGame` datetime DEFAULT NULL,
  `ForumDbRowChecksum` varchar(45) DEFAULT NULL,
  `CurrentIP` varchar(45) DEFAULT NULL,
  `RowUpdated` datetime DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  `RowCreated` datetime DEFAULT CURRENT_TIMESTAMP,
  `GtaLicense` varchar(60) DEFAULT NULL,
  `Email` varchar(320) DEFAULT NULL,
  `BannedUntil` datetime DEFAULT '0001-01-01 00:00:00',
  PRIMARY KEY (`ForumId`),
  UNIQUE KEY `id_UNIQUE` (`ForumId`),
  KEY `SteamId` (`SteamId`),
  KEY `DiscordId` (`DiscordId`),
  KEY `TwitchId` (`TwitchId`),
  KEY `IP` (`CurrentIP`),
  KEY `SteamId_idx` (`SteamId`),
  KEY `TwitchId_idx` (`TwitchId`),
  KEY `GtaLicense_idx` (`GtaLicense`),
  KEY `IP_idx` (`CurrentIP`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COMMENT='For all forum users';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `WhitelistServerState`
--

DROP TABLE IF EXISTS `WhitelistServerState`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `WhitelistServerState` (
  `IP` varchar(45) NOT NULL,
  `State` varchar(20) NOT NULL,
  PRIMARY KEY (`IP`),
  KEY `State` (`State`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping events for database 'FamilyRpServerAccess'
--

--
-- Dumping routines for database 'FamilyRpServerAccess'
--
/*!50003 DROP PROCEDURE IF EXISTS `BanUser` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`myuser`@`%` PROCEDURE `BanUser`(IN SteamId VARCHAR(45))
BEGIN
	UPDATE FamilyRpServerAccess.ParsedSteamAccounts AS psa SET IsBanned = 1 WHERE psa.SteamId = SteamId;
    
	UPDATE FamilyRpServerAccess.Users AS us SET ServerBanned = 1 WHERE us.SteamId = SteamId;

	UPDATE FamilyRpServerAccess.Users AS us
	INNER JOIN FamilyRpServerAccess.ParsedDiscordAccounts AS pda ON us.DiscordId = pda.DiscordId
	SET pda.IsBanned = 1 WHERE us.SteamId = SteamId;

	UPDATE FamilyRpServerAccess.Users AS us
	INNER JOIN FamilyRpServerAccess.ParsedTwitchAccounts AS pta ON us.TwitchId = pta.TwitchId
	SET pta.IsBanned = 1 WHERE us.SteamId = SteamId;

	UPDATE FamilyRpServerAccess.GtaAccounts AS gta
	INNER JOIN FamilyRpServerAccess.Users AS us ON us.ForumId = gta.UserForumId
	SET gta.IsBanned = 1
    WHERE us.SteamId = SteamId;

	UPDATE FamilyRpServerAccess.IpAddresses AS ipa
	INNER JOIN FamilyRpServerAccess.Users AS us ON us.ForumId = ipa.ForumId
	SET ipa.IsBanned = 1
    WHERE us.SteamId = SteamId;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `BanUserForHours` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`myuser`@`%` PROCEDURE `BanUserForHours`(IN SteamId VARCHAR(45), IN BanHours INT)
BEGIN
	DECLARE BannedUntil DATETIME;
	SET BannedUntil = NOW() + INTERVAL BanHours HOUR;

	UPDATE FamilyRpServerAccess.ParsedSteamAccounts AS psa SET psa.BannedUntil = BannedUntil WHERE psa.SteamId = SteamId;
    
	UPDATE FamilyRpServerAccess.Users AS us SET us.BannedUntil = BannedUntil WHERE us.SteamId = SteamId;

	UPDATE FamilyRpServerAccess.Users AS us
	INNER JOIN FamilyRpServerAccess.ParsedDiscordAccounts AS pda ON us.DiscordId = pda.DiscordId
	SET pda.BannedUntil = BannedUntil WHERE us.SteamId = SteamId;

	UPDATE FamilyRpServerAccess.Users AS us
	INNER JOIN FamilyRpServerAccess.ParsedTwitchAccounts AS pta ON us.TwitchId = pta.TwitchId
	SET pta.BannedUntil = BannedUntil WHERE us.SteamId = SteamId;

	UPDATE FamilyRpServerAccess.GtaAccounts AS gta
	INNER JOIN FamilyRpServerAccess.Users AS us ON us.ForumId = gta.UserForumId
	SET gta.BannedUntil = BannedUntil
    WHERE us.SteamId = SteamId;

	UPDATE FamilyRpServerAccess.IpAddresses AS ipa
	INNER JOIN FamilyRpServerAccess.Users AS us ON us.ForumId = ipa.ForumId
	SET ipa.BannedUntil = BannedUntil
    WHERE us.SteamId = SteamId;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `DeveloperProcedure` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`myuser`@`%` PROCEDURE `DeveloperProcedure`()
BEGIN
CREATE TEMPORARY TABLE IF NOT EXISTS topTwoIps ENGINE = MEMORY
AS
SELECT ForumId, IP
FROM 
	(SELECT @prev := '', @n := 0) init
JOIN
	(SELECT @n := if(ForumId != @prev, 1, @n + 1) AS n,
    @prev := ForumId,
    ForumId, IP, RowCreated
    FROM FamilyRpServerAccess.IpAddresses
    WHERE CHAR_LENGTH(IP) <= 16
    ORDER BY
    ForumId ASC,
    RowCreated DESC
    ) x
WHERE n <= 2
ORDER BY ForumId, n;

CREATE TEMPORARY TABLE IF NOT EXISTS WhitelistIntermediateTableA ENGINE = MEMORY
(SELECT DISTINCT * FROM (SELECT DISTINCT tti.IP
FROM topTwoIps AS tti
INNER JOIN FamilyRpServerAccess.GroupMembership as gm ON tti.ForumId = gm.ForumId
INNER JOIN FamilyRpServerAccess.Users as us ON tti.ForumId = us.ForumId
WHERE
gm.Group NOT IN (16)
AND gm.Group IN (11)) AS u WHERE IP NOT IN (SELECT IP FROM IpAddresses WHERE IsBanned = TRUE));
# 8 - Endorsed Whitelisted
# 22 - Applied Whitelisted
# 16 - Banned on forum

-- Because MySQL does not allow the same temporary table to be used multiple times in one query
CREATE TEMPORARY TABLE IF NOT EXISTS WhitelistIntermediateTableB ENGINE = MEMORY
SELECT * FROM WhitelistIntermediateTableA;

CREATE TEMPORARY TABLE IF NOT EXISTS WhitelistJoined ENGINE = MEMORY
(SELECT * FROM (SELECT wit.IP AS WhitelistByForum, wss.IP AS WhitelistByServer, State
FROM WhitelistIntermediateTableA AS wit
LEFT OUTER JOIN FamilyRpServerAccess.FullSpectrumDevWhitelist AS wss ON wss.IP = wit.IP
UNION
SELECT wit.IP AS WhitelistByForum, wss.IP AS WhitelistByServer, State
FROM WhitelistIntermediateTableB AS wit
RIGHT OUTER JOIN FamilyRpServerAccess.FullSpectrumDevWhitelist AS wss ON wss.IP = wit.IP) AS t);
-- t alias is irrelevant; just required

INSERT INTO FamilyRpServerAccess.FullSpectrumDevWhitelist (IP, State)
SELECT WhitelistByForum, 'Add'
FROM WhitelistJoined AS wj
WHERE WhitelistByServer IS NULL;

UPDATE FamilyRpServerAccess.FullSpectrumDevWhitelist AS wss
INNER JOIN WhitelistJoined AS wj ON wj.WhitelistByServer = wss.IP
SET wss.State = 'Add'
WHERE WhitelistByForum IS NOT NULL AND wj.State = 'Remove';

UPDATE FamilyRpServerAccess.FullSpectrumDevWhitelist AS wss
INNER JOIN WhitelistJoined AS wj ON wj.WhitelistByServer = wss.IP
SET wss.State = 'Remove'
WHERE WhitelistByForum IS NULL AND wj.State <> 'Remove';

DELETE wss
FROM FamilyRpServerAccess.FullSpectrumDevWhitelist AS wss
INNER JOIN WhitelistJoined AS wj ON wj.WhitelistByServer = wss.IP
WHERE WhitelistByForum IS NULL AND wj.State = 'Add';

DROP TEMPORARY TABLE topTwoIps;
DROP TEMPORARY TABLE WhitelistIntermediateTableA;
DROP TEMPORARY TABLE WhitelistIntermediateTableB;
DROP TEMPORARY TABLE WhitelistJoined;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `DoesUserHaveAllAccountTypes` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`myuser`@`%` PROCEDURE `DoesUserHaveAllAccountTypes`(IN SteamId VARCHAR(45))
BEGIN
	SELECT CASE WHEN COUNT(1) > 0 THEN 1 ELSE 0 END AS IsAllowed
	FROM FamilyRpServerAccess.Users as us
	INNER JOIN FamilyRpServerAccess.GroupMembership as gm ON us.ForumId = gm.ForumId
#	INNER JOIN FamilyRpServerAccess.ParsedDiscordAccounts as pda ON us.DiscordId = pda.DiscordId
	INNER JOIN FamilyRpServerAccess.ParsedSteamAccounts as psa ON us.SteamId = psa.SteamId
#	INNER JOIN FamilyRpServerAccess.ParsedTwitchAccounts as pta ON us.TwitchId = pta.TwitchId
	LEFT JOIN FamilyRpServerAccess.GtaAccounts as ga ON us.ForumId = ga.UserForumId
	LEFT JOIN FamilyRpServerAccess.IpAddresses as ips ON us.CurrentIP = ips.IP
	WHERE
	us.SteamId = SteamId
	AND gm.Group NOT IN (16)
#	AND us.DiscordId IS NOT NULL
#	AND us.TwitchId IS NOT NULL
	AND (psa.IsBanned = 0 OR psa.IsBanned IS NULL)
#	AND (pta.IsBanned = 0 OR pta.IsBanned IS NULL)
#	AND (pda.IsBanned = 0 OR pda.IsBanned IS NULL)
	AND (ips.IsBanned = 0 OR ips.IsBanned IS NULL)
	AND (ga.IsBanned = 0 OR ga.IsBanned IS NULL)
	AND (psa.BannedUntil < NOW() OR psa.BannedUntil IS NULL)
#	AND (pta.BannedUntil < NOW())
#	AND (pda.BannedUntil < NOW())
	AND (ips.BannedUntil < NOW() OR ips.BannedUntil IS NULL)
	AND (ga.BannedUntil < NOW() OR ga.BannedUntil IS NULL);
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `EnsureGtaLicenseRegistered` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`myuser`@`%` PROCEDURE `EnsureGtaLicenseRegistered`(IN SteamId VARCHAR(45), IN GtaLicense VARCHAR(45))
BEGIN
	SET @IsGtaLicenseRegistered = (SELECT
		CASE WHEN COUNT(1) > 0 THEN 1 ELSE 0 END
		FROM FamilyRpServerAccess.GtaAccounts AS gta
		WHERE gta.GtaLicense = GtaLicense);
        
        SELECT @IsGtaLicenseRegistered;

	IF @IsGtaLicenseRegistered = 0 THEN
		INSERT INTO FamilyRpServerAccess.GtaAccounts (GtaLicense, UserForumId)
		SELECT GtaLicense, us.ForumId
		FROM FamilyRpServerAccess.Users AS us
		WHERE us.SteamId = SteamId
        AND us.SteamId IS NOT NULL
        ORDER BY us.ForumId DESC
        LIMIT 1;
	END IF;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `IsForumUserAllowedToViewPublicIp` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`myuser`@`%` PROCEDURE `IsForumUserAllowedToViewPublicIp`(IN ForumId INT)
BEGIN
	SELECT CASE WHEN COUNT(1) > 0 THEN 1 ELSE 0 END AS IsAllowed
	FROM FamilyRpServerAccess.Users as us
	INNER JOIN FamilyRpServerAccess.GroupMembership as gm ON us.ForumId = gm.ForumId
	INNER JOIN FamilyRpServerAccess.ParsedDiscordAccounts as pda ON us.DiscordId = pda.DiscordId
	INNER JOIN FamilyRpServerAccess.ParsedSteamAccounts as psa ON us.SteamId = psa.SteamId
	INNER JOIN FamilyRpServerAccess.ParsedTwitchAccounts as pta ON us.TwitchId = pta.TwitchId
	LEFT JOIN FamilyRpServerAccess.GtaAccounts as ga ON us.ForumId = ga.UserForumId
	LEFT JOIN FamilyRpServerAccess.IpAddresses as ips ON us.CurrentIP = ips.IP
	WHERE
	us.ForumId = ForumId
	AND gm.Group NOT IN (16)
	AND us.DiscordId IS NOT NULL
	AND us.TwitchId IS NOT NULL
	AND (us.ServerBanned = 0 OR us.ServerBanned IS NULL)
	AND (psa.IsBanned = 0 OR psa.IsBanned IS NULL)
	AND (pta.IsBanned = 0 OR pta.IsBanned IS NULL)
	AND (pda.IsBanned = 0 OR pda.IsBanned IS NULL)
	AND (ips.IsBanned = 0 OR ips.IsBanned IS NULL)
	AND (ga.IsBanned = 0 OR ga.IsBanned IS NULL)
	AND (psa.BannedUntil < NOW())
	AND (pta.BannedUntil < NOW())
	AND (pda.BannedUntil < NOW())
	AND (ips.BannedUntil < NOW())
	AND (ga.BannedUntil < NOW());
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `IsForumUserAllowedToViewWhitelistIp` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`myuser`@`%` PROCEDURE `IsForumUserAllowedToViewWhitelistIp`(IN ForumId INT)
BEGIN
	SELECT CASE WHEN COUNT(1) > 0 THEN 1 ELSE 0 END AS IsWhitelisted
	FROM FamilyRpServerAccess.Users AS us
	INNER JOIN FamilyRpServerAccess.GroupMembership as gm ON us.ForumId = gm.ForumId
	WHERE gm.Group IN (8, 22) AND gm.Group NOT IN (16) AND us.ForumId = ForumId;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `IsUserAdmin` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`myuser`@`%` PROCEDURE `IsUserAdmin`(IN SteamId VARCHAR(45))
BEGIN
	SELECT
		CASE WHEN COUNT(1) > 0 THEN 1 ELSE 0 END AS IsAdmin
		FROM FamilyRpServerAccess.Users AS us
        INNER JOIN FamilyRpServerAccess.GroupMembership as gm ON us.ForumId = gm.ForumId
		WHERE gm.Group IN (4, 10, 11, 19) AND gm.Group NOT IN (16) AND us.SteamId = SteamId;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `IsUserPriority` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`myuser`@`%` PROCEDURE `IsUserPriority`(IN SteamId VARCHAR(45))
BEGIN
	SELECT
		CASE WHEN COUNT(1) > 0 THEN 1 ELSE 0 END AS IsPriority
		FROM FamilyRpServerAccess.Users AS us
		WHERE (us.IsEMS = 1 OR us.IsFireDept = 1 OR us.IsPolice = 1) AND us.SteamId = SteamId;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `IsUserWhitelisted` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`myuser`@`%` PROCEDURE `IsUserWhitelisted`(IN SteamId VARCHAR(45))
BEGIN
	SELECT
		CASE WHEN COUNT(1) > 0 THEN 1 ELSE 0 END AS IsWhitelisted
		FROM FamilyRpServerAccess.Users AS us
        INNER JOIN FamilyRpServerAccess.GroupMembership as gm ON us.ForumId = gm.ForumId
		WHERE gm.Group IN (8, 22) AND gm.Group NOT IN (16) AND us.SteamId = SteamId;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `LogForumIpView` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`myuser`@`%` PROCEDURE `LogForumIpView`(IN ForumId INT, IN ViewedIP VARCHAR(45))
BEGIN
	INSERT INTO ForumServerIpViewLog (ForumId, ViewedIp) VALUES (ForumId, ViewedIp);
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `ProcessPrivateSteamIds` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`myuser`@`%` PROCEDURE `ProcessPrivateSteamIds`()
BEGIN
	UPDATE FamilyRpServerAccess.ParsedSteamAccounts AS psa
		SET psa.SteamCreated =
		(SELECT SteamCreated FROM
		(SELECT * FROM FamilyRpServerAccess.ParsedSteamAccounts WHERE SteamCreated <> '0001-01-01') AS psaSub
		WHERE psaSub.SteamId <= psa.SteamId ORDER BY SteamId DESC LIMIT 1)
		WHERE psa.SteamCreated = '0001-01-01';
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `PublicProcedure` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`myuser`@`%` PROCEDURE `PublicProcedure`()
BEGIN
CREATE TEMPORARY TABLE IF NOT EXISTS topTwoIps ENGINE = MEMORY
AS
SELECT ForumId, IP
FROM 
	(SELECT @prev := '', @n := 0) init
JOIN
	(SELECT @n := if(ForumId != @prev, 1, @n + 1) AS n,
    @prev := ForumId,
    ForumId, IP, RowCreated
    FROM FamilyRpServerAccess.IpAddresses
    WHERE CHAR_LENGTH(IP) <= 16
    ORDER BY
    ForumId ASC,
    RowCreated DESC
    ) x
WHERE n <= 2
ORDER BY ForumId, n;

CREATE TEMPORARY TABLE IF NOT EXISTS WhitelistIntermediateTableA ENGINE = MEMORY
(SELECT DISTINCT * FROM (SELECT DISTINCT tti.IP
FROM topTwoIps AS tti
INNER JOIN FamilyRpServerAccess.GroupMembership as gm ON tti.ForumId = gm.ForumId
INNER JOIN FamilyRpServerAccess.Users as us ON tti.ForumId = us.ForumId
INNER JOIN FamilyRpServerAccess.ParsedDiscordAccounts as pda ON us.DiscordId = pda.DiscordId
INNER JOIN FamilyRpServerAccess.ParsedSteamAccounts as psa ON us.SteamId = psa.SteamId
INNER JOIN FamilyRpServerAccess.ParsedTwitchAccounts as pta ON us.TwitchId = pta.TwitchId
LEFT JOIN FamilyRpServerAccess.GtaAccounts as ga ON us.ForumId = ga.UserForumId
LEFT JOIN FamilyRpServerAccess.IpAddresses as ips ON us.CurrentIP = ips.IP
WHERE gm.Group NOT IN (16)
AND us.SteamId IS NOT NULL
AND us.DiscordId IS NOT NULL
AND us.TwitchId IS NOT NULL
AND (psa.IsBanned = 0 OR psa.IsBanned IS NULL)
AND (pta.IsBanned = 0 OR pta.IsBanned IS NULL)
AND (pda.IsBanned = 0 OR pda.IsBanned IS NULL)
AND (ips.IsBanned = 0 OR ips.IsBanned IS NULL)
AND (ga.IsBanned = 0 OR ga.IsBanned IS NULL)
UNION
SELECT IP FROM FamilyRpServerAccess.ForcedWhitelist WHERE ForceOnPublic = TRUE) AS u WHERE IP NOT IN (SELECT IP FROM IpAddresses WHERE IsBanned = TRUE));
# 8 - Endorsed Whitelisted
# 22 - Applied Whitelisted
# 16 - Banned on forum

-- Because MySQL does not allow the same temporary table to be used multiple times in one query
CREATE TEMPORARY TABLE IF NOT EXISTS WhitelistIntermediateTableB ENGINE = MEMORY
SELECT * FROM WhitelistIntermediateTableA;

CREATE TEMPORARY TABLE IF NOT EXISTS WhitelistJoined ENGINE = MEMORY
(SELECT * FROM (SELECT wit.IP AS WhitelistByForum, wss.IP AS WhitelistByServer, State
FROM WhitelistIntermediateTableA AS wit
LEFT OUTER JOIN FamilyRpServerAccess.PublicServerState AS wss ON wss.IP = wit.IP
UNION
SELECT wit.IP AS WhitelistByForum, wss.IP AS WhitelistByServer, State
FROM WhitelistIntermediateTableB AS wit
RIGHT OUTER JOIN FamilyRpServerAccess.PublicServerState AS wss ON wss.IP = wit.IP) AS t);
-- t alias is irrelevant; just required

INSERT INTO FamilyRpServerAccess.PublicServerState (IP, State)
SELECT WhitelistByForum, 'Add'
FROM WhitelistJoined AS wj
WHERE WhitelistByServer IS NULL;

UPDATE FamilyRpServerAccess.PublicServerState AS wss
INNER JOIN WhitelistJoined AS wj ON wj.WhitelistByServer = wss.IP
SET wss.State = 'Add'
WHERE WhitelistByForum IS NOT NULL AND wj.State = 'Remove';

UPDATE FamilyRpServerAccess.PublicServerState AS wss
INNER JOIN WhitelistJoined AS wj ON wj.WhitelistByServer = wss.IP
SET wss.State = 'Remove'
WHERE WhitelistByForum IS NULL AND wj.State <> 'Remove';

DELETE wss
FROM FamilyRpServerAccess.PublicServerState AS wss
INNER JOIN WhitelistJoined AS wj ON wj.WhitelistByServer = wss.IP
WHERE WhitelistByForum IS NULL AND wj.State = 'Add';

DROP TEMPORARY TABLE topTwoIps;
DROP TEMPORARY TABLE WhitelistIntermediateTableA;
DROP TEMPORARY TABLE WhitelistIntermediateTableB;
DROP TEMPORARY TABLE WhitelistJoined;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `RetrievePublicServerPublicIp` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`myuser`@`%` PROCEDURE `RetrievePublicServerPublicIp`(IN ForumId INT)
BEGIN
	SELECT '255.255.255.255';
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `RetrievePublicServerWhitelistIp` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`myuser`@`%` PROCEDURE `RetrievePublicServerWhitelistIp`(IN ForumId INT)
BEGIN
	SELECT '1.1.1.1';
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `TesterProcedure` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`myuser`@`%` PROCEDURE `TesterProcedure`()
BEGIN
CREATE TEMPORARY TABLE IF NOT EXISTS topTwoIps ENGINE = MEMORY
AS
SELECT ForumId, IP
FROM 
	(SELECT @prev := '', @n := 0) init
JOIN
	(SELECT @n := if(ForumId != @prev, 1, @n + 1) AS n,
    @prev := ForumId,
    ForumId, IP, RowCreated
    FROM FamilyRpServerAccess.IpAddresses
    WHERE CHAR_LENGTH(IP) <= 16
    ORDER BY
    ForumId ASC,
    RowCreated DESC
    ) x
WHERE n <= 2
ORDER BY ForumId, n;

CREATE TEMPORARY TABLE IF NOT EXISTS WhitelistIntermediateTableA ENGINE = MEMORY
(SELECT DISTINCT * FROM (SELECT DISTINCT tti.IP
FROM topTwoIps AS tti
INNER JOIN FamilyRpServerAccess.GroupMembership as gm ON tti.ForumId = gm.ForumId
INNER JOIN FamilyRpServerAccess.Users as us ON tti.ForumId = us.ForumId
WHERE
gm.Group NOT IN (16)
AND gm.Group IN (10, 11, 17, 19)
UNION
SELECT IP FROM FamilyRpServerAccess.ForcedWhitelist WHERE ForceOnDev = TRUE) AS u WHERE IP NOT IN (SELECT IP FROM IpAddresses WHERE IsBanned = TRUE));
# 8 - Endorsed Whitelisted
# 22 - Applied Whitelisted
# 16 - Banned on forum

-- Because MySQL does not allow the same temporary table to be used multiple times in one query
CREATE TEMPORARY TABLE IF NOT EXISTS WhitelistIntermediateTableB ENGINE = MEMORY
SELECT * FROM WhitelistIntermediateTableA;

CREATE TEMPORARY TABLE IF NOT EXISTS WhitelistJoined ENGINE = MEMORY
(SELECT * FROM (SELECT wit.IP AS WhitelistByForum, wss.IP AS WhitelistByServer, State
FROM WhitelistIntermediateTableA AS wit
LEFT OUTER JOIN FamilyRpServerAccess.DevServerState AS wss ON wss.IP = wit.IP
UNION
SELECT wit.IP AS WhitelistByForum, wss.IP AS WhitelistByServer, State
FROM WhitelistIntermediateTableB AS wit
RIGHT OUTER JOIN FamilyRpServerAccess.DevServerState AS wss ON wss.IP = wit.IP) AS t);
-- t alias is irrelevant; just required

INSERT INTO FamilyRpServerAccess.DevServerState (IP, State)
SELECT WhitelistByForum, 'Add'
FROM WhitelistJoined AS wj
WHERE WhitelistByServer IS NULL;

UPDATE FamilyRpServerAccess.DevServerState AS wss
INNER JOIN WhitelistJoined AS wj ON wj.WhitelistByServer = wss.IP
SET wss.State = 'Add'
WHERE WhitelistByForum IS NOT NULL AND wj.State = 'Remove';

UPDATE FamilyRpServerAccess.DevServerState AS wss
INNER JOIN WhitelistJoined AS wj ON wj.WhitelistByServer = wss.IP
SET wss.State = 'Remove'
WHERE WhitelistByForum IS NULL AND wj.State <> 'Remove';

DELETE wss
FROM FamilyRpServerAccess.DevServerState AS wss
INNER JOIN WhitelistJoined AS wj ON wj.WhitelistByServer = wss.IP
WHERE WhitelistByForum IS NULL AND wj.State = 'Add';

DROP TEMPORARY TABLE topTwoIps;
DROP TEMPORARY TABLE WhitelistIntermediateTableA;
DROP TEMPORARY TABLE WhitelistIntermediateTableB;
DROP TEMPORARY TABLE WhitelistJoined;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `WhitelistProcedure` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`myuser`@`%` PROCEDURE `WhitelistProcedure`()
BEGIN
CREATE TEMPORARY TABLE IF NOT EXISTS topTwoIps ENGINE = MEMORY
AS
SELECT ForumId, n, IP
FROM 
	(SELECT @prev := '', @n := 0) init
JOIN
	(SELECT @n := if(ForumId != @prev, 1, @n + 1) AS n,
    @prev := ForumId,
    ForumId, IP, RowCreated
    FROM FamilyRpServerAccess.IpAddresses
    WHERE CHAR_LENGTH(IP) <= 16
    ORDER BY
    ForumId ASC,
    RowUpdated DESC
    ) x
WHERE n <= 2
ORDER BY ForumId, n;

CREATE TEMPORARY TABLE IF NOT EXISTS WhitelistIntermediateTableA ENGINE = MEMORY
(SELECT DISTINCT * FROM (SELECT tti.IP
FROM topTwoIps AS tti
INNER JOIN FamilyRpServerAccess.GroupMembership as gm ON tti.ForumId = gm.ForumId
WHERE gm.Group IN (8, 22) AND gm.Group NOT IN (16)
UNION
SELECT IP FROM FamilyRpServerAccess.ForcedWhitelist WHERE ForceOnWhitelist = TRUE) AS u WHERE IP NOT IN (SELECT IP FROM IpAddresses WHERE IsBanned = TRUE));
# 8 - Endorsed Whitelisted
# 22 - Applied Whitelisted
# 16 - Banned on forum

-- Because MySQL does not allow the same temporary table to be used multiple times in one query
CREATE TEMPORARY TABLE IF NOT EXISTS WhitelistIntermediateTableB ENGINE = MEMORY
SELECT * FROM WhitelistIntermediateTableA;

CREATE TEMPORARY TABLE IF NOT EXISTS WhitelistJoined ENGINE = MEMORY
(SELECT * FROM (SELECT wit.IP AS WhitelistByForum, wss.IP AS WhitelistByServer, State
FROM WhitelistIntermediateTableA AS wit
LEFT OUTER JOIN FamilyRpServerAccess.WhitelistServerState AS wss ON wss.IP = wit.IP
UNION
SELECT wit.IP AS WhitelistByForum, wss.IP AS WhitelistByServer, State
FROM WhitelistIntermediateTableB AS wit
RIGHT OUTER JOIN FamilyRpServerAccess.WhitelistServerState AS wss ON wss.IP = wit.IP) AS t);
-- t alias is irrelevant; just required

INSERT INTO FamilyRpServerAccess.WhitelistServerState (IP, State)
SELECT WhitelistByForum, 'Add'
FROM WhitelistJoined AS wj
WHERE WhitelistByServer IS NULL AND (wj.State <> 'Add' OR wj.State IS NULL);

UPDATE FamilyRpServerAccess.WhitelistServerState AS wss
INNER JOIN WhitelistJoined AS wj ON wj.WhitelistByServer = wss.IP
SET wss.State = 'Remove'
WHERE WhitelistByForum IS NULL AND (wj.State <> 'Remove' OR wj.State IS NULL);

DELETE FROM FamilyRpServerAccess.WhitelistServerState WHERE State = 'Remove';

DROP TEMPORARY TABLE topTwoIps;
DROP TEMPORARY TABLE WhitelistIntermediateTableA;
DROP TEMPORARY TABLE WhitelistIntermediateTableB;
DROP TEMPORARY TABLE WhitelistJoined;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;