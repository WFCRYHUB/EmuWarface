/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;


-- Dumping database structure for emuwarface
CREATE DATABASE IF NOT EXISTS `emuwarface` /*!40100 DEFAULT CHARACTER SET utf8 COLLATE utf8_general_ci */;
USE `emuwarface`;

-- Dumping structure for table emuwarface.emu_abuse_reports
CREATE TABLE IF NOT EXISTS `emu_abuse_reports` (
  `initiator` varchar(16) DEFAULT NULL,
  `target` varchar(16) NOT NULL,
  `type` varchar(12) NOT NULL,
  `comment` varchar(300) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Dumping data for table emuwarface.emu_abuse_reports: ~0 rows (approximately)

-- Dumping structure for table emuwarface.emu_achievements
CREATE TABLE IF NOT EXISTS `emu_achievements` (
  `profile_id` bigint(20) unsigned NOT NULL,
  `achievement_id` int(10) unsigned NOT NULL,
  `progress` int(10) NOT NULL DEFAULT 0,
  `completion_time` bigint(20) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Dumping data for table emuwarface.emu_achievements: ~0 rows (approximately)

-- Dumping structure for table emuwarface.emu_anticheat_punish_mode
CREATE TABLE IF NOT EXISTS `emu_anticheat_punish_mode` (
  `profile_id` bigint(20) unsigned NOT NULL,
  `punish_mode` varchar(50) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Dumping data for table emuwarface.emu_anticheat_punish_mode: ~0 rows (approximately)

-- Dumping structure for table emuwarface.emu_anticheat_report
CREATE TABLE IF NOT EXISTS `emu_anticheat_report` (
  `profile_id` bigint(20) unsigned NOT NULL,
  `type` varchar(50) NOT NULL,
  `score` int(11) NOT NULL,
  `calls` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Dumping data for table emuwarface.emu_anticheat_report: ~0 rows (approximately)

-- Dumping structure for table emuwarface.emu_banforleave
CREATE TABLE IF NOT EXISTS `emu_banforleave` (
  `profile_id` bigint(20) unsigned NOT NULL,
  `type` int(11) NOT NULL DEFAULT 0,
  `unban_time` int(11) NOT NULL DEFAULT 0,
  PRIMARY KEY (`profile_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Dumping data for table emuwarface.emu_banforleave: ~0 rows (approximately)

-- Dumping structure for table emuwarface.emu_bans
CREATE TABLE IF NOT EXISTS `emu_bans` (
  `user_id` bigint(20) unsigned NOT NULL DEFAULT 0,
  `rule` varchar(5) NOT NULL,
  `unban_time` bigint(20) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Dumping data for table emuwarface.emu_bans: ~0 rows (approximately)

-- Dumping structure for table emuwarface.emu_clans
CREATE TABLE IF NOT EXISTS `emu_clans` (
  `clan_id` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(16) NOT NULL,
  `description` varchar(2000) NOT NULL,
  `creation_date` int(10) unsigned NOT NULL,
  PRIMARY KEY (`clan_id`),
  UNIQUE KEY `name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_general_ci;

-- Dumping data for table emuwarface.emu_clans: ~0 rows (approximately)

-- Dumping structure for table emuwarface.emu_clan_members
CREATE TABLE IF NOT EXISTS `emu_clan_members` (
  `profile_id` bigint(20) unsigned NOT NULL,
  `clan_id` bigint(20) unsigned NOT NULL,
  `clan_role` int(10) NOT NULL DEFAULT 3,
  `clan_points` int(10) NOT NULL DEFAULT 0,
  `invite_date` bigint(20) unsigned NOT NULL,
  PRIMARY KEY (`profile_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Dumping data for table emuwarface.emu_clan_members: ~0 rows (approximately)

-- Dumping structure for table emuwarface.emu_connects
CREATE TABLE IF NOT EXISTS `emu_connects` (
  `user_id` bigint(20) unsigned NOT NULL,
  `ipaddress` varchar(16) NOT NULL,
  UNIQUE KEY `user_id` (`user_id`,`ipaddress`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Dumping data for table emuwarface.emu_connects: ~0 rows (approximately)

-- Dumping structure for table emuwarface.emu_dynamic_multipliers
CREATE TABLE IF NOT EXISTS `emu_dynamic_multipliers` (
  `name` varchar(50) NOT NULL,
  `multiplier` int(11) DEFAULT NULL,
  PRIMARY KEY (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Dumping data for table emuwarface.emu_dynamic_multipliers: ~0 rows (approximately)

-- Dumping structure for table emuwarface.emu_friends
CREATE TABLE IF NOT EXISTS `emu_friends` (
  `first_id` bigint(20) unsigned NOT NULL,
  `second_id` bigint(20) unsigned NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_general_ci;

-- Dumping data for table emuwarface.emu_friends: ~0 rows (approximately)

-- Dumping structure for table emuwarface.emu_items
CREATE TABLE IF NOT EXISTS `emu_items` (
  `id` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  `profile_id` bigint(20) unsigned NOT NULL,
  `type` tinyint(4) unsigned NOT NULL DEFAULT 0,
  `name` varchar(50) NOT NULL,
  `config` varchar(50) NOT NULL DEFAULT 'dm=0;material=default',
  `attached_to` tinyint(1) unsigned NOT NULL DEFAULT 0,
  `slot` int(11) NOT NULL DEFAULT 0,
  `equipped` int(10) NOT NULL DEFAULT 0,
  `expired_confirmed` tinyint(1) NOT NULL DEFAULT 0,
  `buy_time_utc` bigint(20) NOT NULL DEFAULT 0,
  `expiration_time_utc` bigint(20) NOT NULL DEFAULT 0,
  `quantity` int(11) NOT NULL DEFAULT -1,
  `total_durability_points` mediumint(9) NOT NULL DEFAULT -1,
  `durability_points` mediumint(9) NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_general_ci;

-- Dumping data for table emuwarface.emu_items: ~0 rows (approximately)

-- Dumping structure for table emuwarface.emu_login_bonus
CREATE TABLE IF NOT EXISTS `emu_login_bonus` (
  `profile_id` bigint(20) unsigned NOT NULL,
  `current_streak` tinyint(3) NOT NULL DEFAULT 0,
  `current_reward` tinyint(3) NOT NULL DEFAULT -1,
  `last_seen_reward` bigint(20) NOT NULL DEFAULT 0,
  PRIMARY KEY (`profile_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Dumping data for table emuwarface.emu_login_bonus: ~0 rows (approximately)

-- Dumping structure for table emuwarface.emu_mutes
CREATE TABLE IF NOT EXISTS `emu_mutes` (
  `user_id` bigint(20) NOT NULL DEFAULT 0,
  `rule` varchar(5) NOT NULL,
  `unmute_time` bigint(20) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Dumping data for table emuwarface.emu_mutes: ~0 rows (approximately)

-- Dumping structure for table emuwarface.emu_notifications
CREATE TABLE IF NOT EXISTS `emu_notifications` (
  `id` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  `profile_id` bigint(20) unsigned NOT NULL DEFAULT 0,
  `type` mediumint(7) NOT NULL,
  `expiration_time_utc` bigint(20) NOT NULL DEFAULT 0,
  `confirmation` tinyint(1) NOT NULL DEFAULT 0,
  `data` varchar(1000) NOT NULL DEFAULT '',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Dumping data for table emuwarface.emu_notifications: ~0 rows (approximately)

-- Dumping structure for table emuwarface.emu_pc_info
CREATE TABLE IF NOT EXISTS `emu_pc_info` (
  `profile_id` bigint(20) unsigned NOT NULL DEFAULT 0,
  `hwid` int(11) NOT NULL,
  `os_64` tinyint(4) NOT NULL,
  `os_ver` tinyint(4) NOT NULL,
  `gpu_device_id` smallint(6) NOT NULL DEFAULT 0,
  `gpu_vendor_id` smallint(6) NOT NULL DEFAULT 0,
  `cpu_model` tinyint(4) NOT NULL DEFAULT 0,
  `cpu_family` tinyint(4) NOT NULL DEFAULT 0,
  `cpu_vendor` tinyint(4) NOT NULL DEFAULT 0,
  `cpu_num_cores` tinyint(4) NOT NULL DEFAULT 0,
  `cpu_stepping` tinyint(4) NOT NULL DEFAULT 0,
  `physical_memory` mediumint(9) NOT NULL DEFAULT 0,
  PRIMARY KEY (`profile_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Dumping data for table emuwarface.emu_pc_info: ~0 rows (approximately)

-- Dumping structure for table emuwarface.emu_persistent_settings
CREATE TABLE IF NOT EXISTS `emu_persistent_settings` (
  `profile_id` bigint(20) unsigned NOT NULL,
  `type` varchar(10) NOT NULL,
  `name` varchar(50) NOT NULL,
  `value` varchar(50) NOT NULL,
  UNIQUE KEY `s_key` (`profile_id`,`type`,`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Dumping data for table emuwarface.emu_persistent_settings: ~0 rows (approximately)

-- Dumping structure for table emuwarface.emu_pin_codes
CREATE TABLE IF NOT EXISTS `emu_pin_codes` (
  `pin` varchar(30) NOT NULL,
  `ammount` int(11) NOT NULL DEFAULT 0,
  `reward` text NOT NULL,
  UNIQUE KEY `pin` (`pin`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Dumping data for table emuwarface.emu_pin_codes: ~0 rows (approximately)

-- Dumping structure for table emuwarface.emu_pin_used
CREATE TABLE IF NOT EXISTS `emu_pin_used` (
  `user_id` int(11) NOT NULL,
  `pin` varchar(30) NOT NULL,
  UNIQUE KEY `unique` (`user_id`,`pin`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Dumping data for table emuwarface.emu_pin_used: ~0 rows (approximately)

-- Dumping structure for table emuwarface.emu_profiles
CREATE TABLE IF NOT EXISTS `emu_profiles` (
  `profile_id` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  `user_id` bigint(20) unsigned NOT NULL,
  `nickname` varchar(16) NOT NULL,
  `head` varchar(16) NOT NULL DEFAULT 'default_head_01',
  `experience` int(11) NOT NULL DEFAULT 0,
  `exp_freezed` tinyint(3) unsigned NOT NULL DEFAULT 0,
  `current_class` int(10) NOT NULL DEFAULT 0,
  `height` int(10) NOT NULL DEFAULT 1,
  `fatness` int(10) NOT NULL DEFAULT 0,
  `banner_badge` int(10) unsigned NOT NULL DEFAULT 4294967295,
  `banner_mark` int(10) unsigned NOT NULL DEFAULT 4294967295,
  `banner_stripe` int(10) unsigned NOT NULL DEFAULT 4294967295,
  `game_money` int(11) NOT NULL DEFAULT 50000,
  `cry_money` int(11) NOT NULL DEFAULT 30000,
  `crown_money` int(11) NOT NULL DEFAULT 1000,
  `last_seen_date` bigint(20) NOT NULL,
  PRIMARY KEY (`profile_id`),
  UNIQUE KEY `nickname` (`nickname`),
  UNIQUE KEY `user_id` (`user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_general_ci;

-- Dumping data for table emuwarface.emu_profiles: ~0 rows (approximately)

-- Dumping structure for table emuwarface.emu_profile_bans
CREATE TABLE IF NOT EXISTS `emu_profile_bans` (
  `profile_id` bigint(20) unsigned NOT NULL,
  `room_type` tinyint(4) unsigned NOT NULL,
  `ban_type` tinyint(4) unsigned NOT NULL,
  `unban_utc` bigint(20) NOT NULL,
  `untrial_utc` bigint(20) NOT NULL,
  `last_ban_index` tinyint(4) unsigned NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Dumping data for table emuwarface.emu_profile_bans: ~0 rows (approximately)

-- Dumping structure for table emuwarface.emu_profile_progression_state
CREATE TABLE IF NOT EXISTS `emu_profile_progression_state` (
  `profile_id` bigint(20) unsigned NOT NULL,
  `mission_unlocked` varchar(600) NOT NULL DEFAULT 'trainingmission,easymission,normalmission,hardmission,zombieeasy,zombienormal,zombiehard,survivalmission,campaignsections,campaignsection1,campaignsection2,campaignsection3,volcanoeasy,volcanonormal,volcanohard,volcanosurvival,anubiseasy,anubisnormal,anubishard,anubiseasy2,anubisnormal2,anubishard2,zombietowereasy,zombietowernormal,zombietowerhard,icebreakereasy,icebreakernormal,icebreakerhard,chernobyleasy,chernobylnormal,chernobylhard,japaneasy,japannormal,japanhard,marseasy,marsnormal,marshard,blackwood,pve_arena',
  `tutorial_unlocked` tinyint(3) unsigned NOT NULL DEFAULT 1,
  `tutorial_passed` tinyint(2) unsigned NOT NULL DEFAULT 1,
  `class_unlocked` tinyint(2) unsigned NOT NULL DEFAULT 31,
  PRIMARY KEY (`profile_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Dumping data for table emuwarface.emu_profile_progression_state: ~0 rows (approximately)

-- Dumping structure for table emuwarface.emu_pvp_rating
CREATE TABLE IF NOT EXISTS `emu_pvp_rating` (
  `profile_id` bigint(20) unsigned NOT NULL,
  `rank` int(11) NOT NULL DEFAULT 0,
  `max_rank` int(11) NOT NULL DEFAULT 0,
  `games_history` varchar(32) NOT NULL DEFAULT '',
  PRIMARY KEY (`profile_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Dumping data for table emuwarface.emu_pvp_rating: ~0 rows (approximately)

-- Dumping structure for table emuwarface.emu_sponsors
CREATE TABLE IF NOT EXISTS `emu_sponsors` (
  `profile_id` bigint(20) unsigned NOT NULL,
  `sponsor_id` tinyint(3) unsigned NOT NULL,
  `sponsor_points` tinyint(3) unsigned NOT NULL,
  `next_unlock_item` varchar(64) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Dumping data for table emuwarface.emu_sponsors: ~0 rows (approximately)

-- Dumping structure for table emuwarface.emu_stats
CREATE TABLE IF NOT EXISTS `emu_stats` (
  `id` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  `profile_id` bigint(20) unsigned NOT NULL,
  `stat` varchar(50) NOT NULL,
  `class` tinyint(4) unsigned DEFAULT NULL,
  `mode` tinyint(4) unsigned DEFAULT NULL,
  `difficulty` varchar(25) DEFAULT NULL,
  `item_type` varchar(50) DEFAULT NULL,
  `value` bigint(20) unsigned NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`),
  UNIQUE KEY `s_key` (`profile_id`,`stat`,`class`,`mode`,`difficulty`,`item_type`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Dumping data for table emuwarface.emu_stats: ~0 rows (approximately)

-- Dumping structure for table emuwarface.emu_users
CREATE TABLE IF NOT EXISTS `emu_users` (
  `user_id` bigint(20) NOT NULL AUTO_INCREMENT,
  `vk_id` int(11) NOT NULL,
  `login` varchar(64) NOT NULL,
  `password` varchar(64) NOT NULL,
  `token` varchar(64) NOT NULL,
  `cry_token` varchar(64) NOT NULL,
  `ipaddress` varchar(16) NOT NULL,
  `balance` int(11) NOT NULL,
  PRIMARY KEY (`user_id`),
  UNIQUE KEY `vk_id` (`vk_id`,`login`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Dumping data for table emuwarface.emu_users: ~2 rows (approximately)
INSERT IGNORE INTO `emu_users` (`user_id`, `vk_id`, `login`, `password`, `token`, `cry_token`, `ipaddress`, `balance`) VALUES
	(1, 0, 'user1', '', '', '1', '', 0),
	(2, 0, 'user2', '', '', '1', '', 0);

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
