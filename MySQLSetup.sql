USE playerdata;

CREATE TABLE IF NOT EXISTS verifications (
	robloxid varchar(255), 
    discordid varchar(255)
);

DROP TABLE IF EXISTS statistics; 

CREATE TABLE statistics (
	robloxid varchar(255),
	experience int,
	resistance int,
    strength int, 
    intelligence int,
    agility int, 
    charm int
);