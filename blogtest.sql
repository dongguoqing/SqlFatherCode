/*
Navicat MySQL Data Transfer

Source Server         : 103.28.213.9
Source Server Version : 50727
Source Host           : 103.28.213.9:3306
Source Database       : blogtest

Target Server Type    : MYSQL
Target Server Version : 50727
File Encoding         : 65001

Date: 2022-05-04 15:13:44
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for article
-- ----------------------------
DROP TABLE IF EXISTS `article`;
CREATE TABLE `article` (
  `AddTime` datetime(6) DEFAULT NULL,
  `UpdateTime` datetime(6) DEFAULT NULL,
  `AddUserId` int(11) NOT NULL,
  `UpdateUserId` int(11) NOT NULL,
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `Title` longtext,
  `Content` longtext,
  `Author` longtext,
  `AdmireCount` int(11) NOT NULL,
  `LanguageTypeId` int(11) DEFAULT NULL,
  `Avater` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `IX_Article_AddUserId` (`AddUserId`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for articlecomment
-- ----------------------------
DROP TABLE IF EXISTS `articlecomment`;
CREATE TABLE `articlecomment` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Comment` longtext,
  `ArticleId` int(11) DEFAULT NULL,
  `AddTime` datetime DEFAULT NULL,
  `AdmireCount` int(11) DEFAULT NULL,
  `ToId` int(11) DEFAULT NULL,
  `CommentId` int(11) DEFAULT NULL,
  `UpdateTime` datetime DEFAULT NULL,
  `UpdateUserId` int(11) DEFAULT NULL,
  `AddUserId` int(11) DEFAULT NULL,
  `HasReply` bit(1) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for blogdictionary
-- ----------------------------
DROP TABLE IF EXISTS `blogdictionary`;
CREATE TABLE `blogdictionary` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(50) DEFAULT NULL,
  `Value` varchar(50) DEFAULT NULL,
  `AddTime` datetime DEFAULT NULL,
  `UpdateTime` datetime DEFAULT NULL,
  `AddUserId` int(11) DEFAULT NULL,
  `UpdateUserId` int(11) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for blogrole
-- ----------------------------
DROP TABLE IF EXISTS `blogrole`;
CREATE TABLE `blogrole` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) DEFAULT NULL,
  `Remark` varchar(255) DEFAULT NULL,
  `AddTime` datetime DEFAULT NULL,
  `UpdateTime` datetime DEFAULT NULL,
  `AddUserId` int(11) DEFAULT NULL,
  `UpdateUserId` int(11) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for blogroleresource
-- ----------------------------
DROP TABLE IF EXISTS `blogroleresource`;
CREATE TABLE `blogroleresource` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `RoleId` int(11) NOT NULL,
  `ResourceId` int(11) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=171 DEFAULT CHARSET=utf8mb4;

-- ----------------------------
-- Table structure for blogsysinfo
-- ----------------------------
DROP TABLE IF EXISTS `blogsysinfo`;
CREATE TABLE `blogsysinfo` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `InfoId` longtext,
  `InfoValue` longtext,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for bloguser
-- ----------------------------
DROP TABLE IF EXISTS `bloguser`;
CREATE TABLE `bloguser` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `UserName` longtext,
  `PassWord` longtext,
  `PhoneNumber` longtext,
  `Avater` longtext,
  `RealName` longtext,
  `Email` longtext,
  `IsEnabled` int(11) DEFAULT NULL,
  `Sex` int(11) DEFAULT NULL,
  `DeptId` int(11) DEFAULT NULL,
  `DeptName` longtext,
  `AddTime` datetime(6) DEFAULT NULL,
  `UpdateTime` datetime(6) DEFAULT NULL,
  `AddUserId` int(11) DEFAULT NULL,
  `UpdateUserId` int(11) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=12 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for bloguserrole
-- ----------------------------
DROP TABLE IF EXISTS `bloguserrole`;
CREATE TABLE `bloguserrole` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `UserId` int(11) DEFAULT NULL,
  `RoleId` int(11) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=12 DEFAULT CHARSET=utf8mb4;

-- ----------------------------
-- Table structure for refreshtoken
-- ----------------------------
DROP TABLE IF EXISTS `refreshtoken`;
CREATE TABLE `refreshtoken` (
  `Id` int(11) DEFAULT NULL,
  `Token` varchar(255) DEFAULT NULL,
  `Expires` datetime DEFAULT NULL,
  `IsExpired` bit(1) DEFAULT NULL,
  `Created` datetime DEFAULT NULL,
  `CreatedByIp` varchar(50) DEFAULT NULL,
  `Revoked` datetime DEFAULT NULL,
  `RevokedByIp` varchar(50) DEFAULT NULL,
  `ReplacedByToken` varchar(50) DEFAULT NULL,
  `IsActive` bit(1) DEFAULT NULL,
  `UserId` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for resource
-- ----------------------------
DROP TABLE IF EXISTS `resource`;
CREATE TABLE `resource` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `label` varchar(255) DEFAULT NULL,
  `url` varchar(255) DEFAULT NULL,
  `resourceType` int(11) DEFAULT NULL,
  `superior` int(255) DEFAULT NULL,
  `delete` bit(1) DEFAULT NULL,
  `remark` varchar(255) DEFAULT NULL,
  `AddTime` datetime DEFAULT NULL,
  `UpdateTime` datetime DEFAULT NULL,
  `AddUserId` int(11) DEFAULT NULL,
  `UpdateUserId` int(11) DEFAULT NULL,
  `seq` decimal(11,0) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for __efmigrationshistory
-- ----------------------------
DROP TABLE IF EXISTS `__efmigrationshistory`;
CREATE TABLE `__efmigrationshistory` (
  `MigrationId` varchar(95) NOT NULL,
  `ProductVersion` varchar(32) NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
