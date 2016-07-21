CREATE TABLE `playerdata` (
  `PlayerID` int(11) NOT NULL,
  `PlayerName` varchar(60) NOT NULL,
  `Credits` int(10) unsigned NOT NULL,
  `LifetimeSpins` int(10) unsigned NOT NULL,
  `SaltValue` varchar(20) NOT NULL,
  PRIMARY KEY (`PlayerID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
