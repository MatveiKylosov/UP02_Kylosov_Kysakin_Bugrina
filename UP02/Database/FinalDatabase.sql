-- MySQL dump 10.13  Distrib 8.0.38, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: EquipmentDB
-- ------------------------------------------------------
-- Server version	9.2.0

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `Audiences`
--

DROP TABLE IF EXISTS `Audiences`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Audiences` (
  `AudienceID` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) NOT NULL,
  `ShortName` varchar(50) DEFAULT NULL,
  `ResponsibleUserID` int DEFAULT NULL,
  `TempResponsibleUserID` int DEFAULT NULL,
  PRIMARY KEY (`AudienceID`),
  KEY `ResponsibleUserID` (`ResponsibleUserID`),
  KEY `TempResponsibleUserID` (`TempResponsibleUserID`),
  CONSTRAINT `audiences_ibfk_1` FOREIGN KEY (`ResponsibleUserID`) REFERENCES `Users` (`UserID`),
  CONSTRAINT `audiences_ibfk_2` FOREIGN KEY (`TempResponsibleUserID`) REFERENCES `Users` (`UserID`)
) ENGINE=InnoDB AUTO_INCREMENT=29 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ConsumableCharacteristicValues`
--

DROP TABLE IF EXISTS `ConsumableCharacteristicValues`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ConsumableCharacteristicValues` (
  `CharacteristicsValueID` int NOT NULL AUTO_INCREMENT,
  `CharacteristicID` int DEFAULT NULL,
  `ConsumablesID` int DEFAULT NULL,
  `Value` varchar(255) NOT NULL,
  PRIMARY KEY (`CharacteristicsValueID`),
  KEY `consumablecharacteristicvalues_ibfk_2_idx` (`ConsumablesID`),
  KEY `consumablecharacteristicvalues_ibfk_3_idx` (`CharacteristicID`),
  CONSTRAINT `consumablecharacteristicvalues_ibfk_1` FOREIGN KEY (`CharacteristicID`) REFERENCES `ConsumableCharacteristics` (`CharacteristicID`),
  CONSTRAINT `consumablecharacteristicvalues_ibfk_2` FOREIGN KEY (`ConsumablesID`) REFERENCES `Consumables` (`ConsumableID`)
) ENGINE=InnoDB AUTO_INCREMENT=54 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ConsumableCharacteristics`
--

DROP TABLE IF EXISTS `ConsumableCharacteristics`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ConsumableCharacteristics` (
  `CharacteristicID` int NOT NULL AUTO_INCREMENT,
  `TypeConsumablesID` int DEFAULT NULL,
  `CharacteristicName` varchar(255) NOT NULL,
  PRIMARY KEY (`CharacteristicID`),
  KEY `TypeConsumablesID` (`TypeConsumablesID`),
  CONSTRAINT `consumablecharacteristics_ibfk_1` FOREIGN KEY (`TypeConsumablesID`) REFERENCES `TypesConsumables` (`TypeConsumablesID`)
) ENGINE=InnoDB AUTO_INCREMENT=35 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Consumables`
--

DROP TABLE IF EXISTS `Consumables`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Consumables` (
  `ConsumableID` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) NOT NULL,
  `Description` text,
  `ReceiptDate` date DEFAULT NULL,
  `Photo` longblob,
  `Quantity` int DEFAULT NULL,
  `TempResponsibleUserID` int DEFAULT NULL,
  `TypeConsumablesID` int DEFAULT NULL,
  PRIMARY KEY (`ConsumableID`),
  KEY `TypeConsumablesID` (`TypeConsumablesID`),
  KEY `TempResponsibleUserID` (`TempResponsibleUserID`),
  CONSTRAINT `consumables_ibfk_1` FOREIGN KEY (`TypeConsumablesID`) REFERENCES `TypesConsumables` (`TypeConsumablesID`),
  CONSTRAINT `consumables_ibfk_2` FOREIGN KEY (`TempResponsibleUserID`) REFERENCES `Users` (`UserID`)
) ENGINE=InnoDB AUTO_INCREMENT=17 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Directions`
--

DROP TABLE IF EXISTS `Directions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Directions` (
  `DirectionID` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) NOT NULL,
  PRIMARY KEY (`DirectionID`)
) ENGINE=InnoDB AUTO_INCREMENT=24 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Equipment`
--

DROP TABLE IF EXISTS `Equipment`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Equipment` (
  `EquipmentID` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) NOT NULL,
  `Photo` longblob,
  `InventoryNumber` varchar(50) NOT NULL,
  `AudienceID` int DEFAULT NULL,
  `ResponsibleUserID` int DEFAULT NULL,
  `TempResponsibleUserID` int DEFAULT NULL,
  `Cost` decimal(18,2) DEFAULT NULL,
  `DirectionID` int DEFAULT NULL,
  `StatusID` int DEFAULT NULL,
  `ModelID` int DEFAULT NULL,
  `Comment` text,
  PRIMARY KEY (`EquipmentID`),
  KEY `AudienceID` (`AudienceID`),
  KEY `ResponsibleUserID` (`ResponsibleUserID`),
  KEY `TempResponsibleUserID` (`TempResponsibleUserID`),
  KEY `DirectionID` (`DirectionID`),
  KEY `StatusID` (`StatusID`),
  KEY `ModelID` (`ModelID`),
  CONSTRAINT `equipment_ibfk_1` FOREIGN KEY (`AudienceID`) REFERENCES `Audiences` (`AudienceID`),
  CONSTRAINT `equipment_ibfk_2` FOREIGN KEY (`ResponsibleUserID`) REFERENCES `Users` (`UserID`),
  CONSTRAINT `equipment_ibfk_3` FOREIGN KEY (`TempResponsibleUserID`) REFERENCES `Users` (`UserID`),
  CONSTRAINT `equipment_ibfk_4` FOREIGN KEY (`DirectionID`) REFERENCES `Directions` (`DirectionID`),
  CONSTRAINT `equipment_ibfk_5` FOREIGN KEY (`StatusID`) REFERENCES `Statuses` (`StatusID`),
  CONSTRAINT `equipment_ibfk_6` FOREIGN KEY (`ModelID`) REFERENCES `EquipmentModels` (`ModelID`)
) ENGINE=InnoDB AUTO_INCREMENT=71 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `EquipmentConsumables`
--

DROP TABLE IF EXISTS `EquipmentConsumables`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `EquipmentConsumables` (
  `EquipmentConsumableID` int NOT NULL AUTO_INCREMENT,
  `EquipmentID` int NOT NULL,
  `ConsumableID` int NOT NULL,
  PRIMARY KEY (`EquipmentConsumableID`),
  KEY `EquipmentID` (`EquipmentID`),
  KEY `ConsumableID` (`ConsumableID`),
  CONSTRAINT `equipmentconsumables_ibfk_1` FOREIGN KEY (`EquipmentID`) REFERENCES `Equipment` (`EquipmentID`),
  CONSTRAINT `equipmentconsumables_ibfk_2` FOREIGN KEY (`ConsumableID`) REFERENCES `Consumables` (`ConsumableID`)
) ENGINE=InnoDB AUTO_INCREMENT=21 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `EquipmentLocationHistory`
--

DROP TABLE IF EXISTS `EquipmentLocationHistory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `EquipmentLocationHistory` (
  `HistoryID` int NOT NULL AUTO_INCREMENT,
  `EquipmentID` int DEFAULT NULL,
  `AudienceID` int DEFAULT NULL,
  `ChangeDate` datetime DEFAULT CURRENT_TIMESTAMP,
  `Comment` text,
  PRIMARY KEY (`HistoryID`),
  KEY `EquipmentID` (`EquipmentID`),
  KEY `AudienceID` (`AudienceID`),
  CONSTRAINT `equipmentlocationhistory_ibfk_1` FOREIGN KEY (`EquipmentID`) REFERENCES `Equipment` (`EquipmentID`),
  CONSTRAINT `equipmentlocationhistory_ibfk_2` FOREIGN KEY (`AudienceID`) REFERENCES `Audiences` (`AudienceID`)
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `EquipmentModels`
--

DROP TABLE IF EXISTS `EquipmentModels`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `EquipmentModels` (
  `ModelID` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) NOT NULL,
  PRIMARY KEY (`ModelID`)
) ENGINE=InnoDB AUTO_INCREMENT=22 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `EquipmentResponsibleHistory`
--

DROP TABLE IF EXISTS `EquipmentResponsibleHistory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `EquipmentResponsibleHistory` (
  `HistoryID` int NOT NULL AUTO_INCREMENT,
  `EquipmentID` int DEFAULT NULL,
  `OldUserID` int DEFAULT NULL,
  `ChangeDate` datetime DEFAULT CURRENT_TIMESTAMP,
  `Comment` text,
  PRIMARY KEY (`HistoryID`),
  KEY `EquipmentID` (`EquipmentID`),
  KEY `OldUserID` (`OldUserID`),
  CONSTRAINT `equipmentresponsiblehistory_ibfk_1` FOREIGN KEY (`EquipmentID`) REFERENCES `Equipment` (`EquipmentID`),
  CONSTRAINT `equipmentresponsiblehistory_ibfk_2` FOREIGN KEY (`OldUserID`) REFERENCES `Users` (`UserID`)
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Errors`
--

DROP TABLE IF EXISTS `Errors`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Errors` (
  `ErrorID` int NOT NULL AUTO_INCREMENT,
  `ErrorTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ErrorMessage` text NOT NULL,
  PRIMARY KEY (`ErrorID`)
) ENGINE=InnoDB AUTO_INCREMENT=31 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Inventories`
--

DROP TABLE IF EXISTS `Inventories`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Inventories` (
  `InventoryID` int NOT NULL AUTO_INCREMENT,
  `StartDate` date NOT NULL,
  `UserID` int DEFAULT NULL,
  `EndDate` date NOT NULL,
  `Name` varchar(255) NOT NULL,
  PRIMARY KEY (`InventoryID`),
  KEY `UserID` (`UserID`),
  CONSTRAINT `inventories_ibfk_1` FOREIGN KEY (`UserID`) REFERENCES `Users` (`UserID`)
) ENGINE=InnoDB AUTO_INCREMENT=26 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `InventoryChecks`
--

DROP TABLE IF EXISTS `InventoryChecks`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `InventoryChecks` (
  `CheckID` int NOT NULL AUTO_INCREMENT,
  `InventoryID` int DEFAULT NULL,
  `EquipmentID` int DEFAULT NULL,
  `UserID` int DEFAULT NULL,
  `CheckDate` datetime DEFAULT CURRENT_TIMESTAMP,
  `Comment` text,
  PRIMARY KEY (`CheckID`),
  KEY `InventoryID` (`InventoryID`),
  KEY `EquipmentID` (`EquipmentID`),
  KEY `inventorychecks_ibfk_3_idx` (`UserID`),
  CONSTRAINT `inventorychecks_ibfk_1` FOREIGN KEY (`InventoryID`) REFERENCES `Inventories` (`InventoryID`),
  CONSTRAINT `inventorychecks_ibfk_2` FOREIGN KEY (`EquipmentID`) REFERENCES `Equipment` (`EquipmentID`),
  CONSTRAINT `inventorychecks_ibfk_3` FOREIGN KEY (`UserID`) REFERENCES `Users` (`UserID`)
) ENGINE=InnoDB AUTO_INCREMENT=37 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `NetworkSettings`
--

DROP TABLE IF EXISTS `NetworkSettings`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `NetworkSettings` (
  `NetworkID` int NOT NULL AUTO_INCREMENT,
  `EquipmentID` int DEFAULT NULL,
  `IPAddress` varchar(15) NOT NULL,
  `SubnetMask` varchar(15) DEFAULT NULL,
  `Gateway` varchar(15) DEFAULT NULL,
  `DNSServers` text,
  PRIMARY KEY (`NetworkID`),
  KEY `EquipmentID` (`EquipmentID`),
  CONSTRAINT `networksettings_ibfk_1` FOREIGN KEY (`EquipmentID`) REFERENCES `Equipment` (`EquipmentID`)
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Software`
--

DROP TABLE IF EXISTS `Software`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Software` (
  `SoftwareID` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) NOT NULL,
  `DeveloperID` int DEFAULT NULL,
  `EquipmentID` int DEFAULT NULL,
  `Version` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`SoftwareID`),
  KEY `DeveloperID` (`DeveloperID`),
  KEY `EquipmentID` (`EquipmentID`),
  CONSTRAINT `software_ibfk_1` FOREIGN KEY (`DeveloperID`) REFERENCES `SoftwareDevelopers` (`DeveloperID`),
  CONSTRAINT `software_ibfk_2` FOREIGN KEY (`EquipmentID`) REFERENCES `Equipment` (`EquipmentID`)
) ENGINE=InnoDB AUTO_INCREMENT=18 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SoftwareDevelopers`
--

DROP TABLE IF EXISTS `SoftwareDevelopers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `SoftwareDevelopers` (
  `DeveloperID` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) NOT NULL,
  PRIMARY KEY (`DeveloperID`)
) ENGINE=InnoDB AUTO_INCREMENT=22 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Statuses`
--

DROP TABLE IF EXISTS `Statuses`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Statuses` (
  `StatusID` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) NOT NULL,
  PRIMARY KEY (`StatusID`)
) ENGINE=InnoDB AUTO_INCREMENT=21 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `TypesConsumables`
--

DROP TABLE IF EXISTS `TypesConsumables`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `TypesConsumables` (
  `TypeConsumablesID` int NOT NULL AUTO_INCREMENT,
  `Type` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`TypeConsumablesID`)
) ENGINE=InnoDB AUTO_INCREMENT=31 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Users`
--

DROP TABLE IF EXISTS `Users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Users` (
  `UserID` int NOT NULL AUTO_INCREMENT,
  `Login` varchar(50) NOT NULL,
  `Password` varchar(255) NOT NULL,
  `Role` varchar(50) NOT NULL,
  `Email` varchar(255) DEFAULT NULL,
  `LastName` varchar(100) NOT NULL,
  `FirstName` varchar(100) NOT NULL,
  `MiddleName` varchar(100) DEFAULT NULL,
  `Phone` varchar(20) DEFAULT NULL,
  `Address` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`UserID`),
  UNIQUE KEY `Login` (`Login`)
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-03-26 22:53:51
