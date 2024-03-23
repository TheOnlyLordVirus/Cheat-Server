/* 10.11.2-MariaDB
create database CHEATER_DATA;
use CHEATER_DATA;
set global sql_mode = 'NO_AUTO_VALUE_ON_ZERO';
set global time_zone = '-05:00';
set time_zone = '-05:00';

CREATE USER 'admin'@'localhost' IDENTIFIED BY '&NopeChuckTesta7843' require ssl;
GRANT ALL PRIVILEGES ON CHEATER_DATA.* TO 'admin'@'localhost';
*/

create table USERS
(
	ID uuid primary key default uuid(),
	USER_EMAIL varchar(30) not null,
	USER_NAME varchar(30) not null,
	USER_PASS varchar(1000) not null,
	IS_ADMIN boolean not null default false,
	REGISTRATION_IP varchar(15) not null,
	RECENT_IP varchar(15) not null,
	CREATION_DATE datetime not null default now(),
    HWID varchar(1000) not null,
	ACTIVE boolean not null default true
);

create table GAMES
(
	ID uuid primary key default (uuid()),
    GAME_PROCESS_NAME varchar(60) not null,
    GAME_NAME varchar(60) not null,
    GAME_VERSION varchar(30) not null
);

create table ACCESS_LEVELS
(
	ID int primary key auto_increment,
    NAME varchar(60) not null
);

create table CHEAT_BINARYS
(
	ID int primary key auto_increment,
    GAME_ID uuid not null,
    ACCESS_LEVEL int not null,
    CHEAT longtext not null,
    DESCRIPTION varchar(255),
    constraint FK_CHEAT_BINARYS_GAME foreign key (GAME_ID) references GAMES(ID) on delete cascade,
    constraint FK_CHEAT_BINARYS_ACCESS foreign key (ACCESS_LEVEL) references ACCESS_LEVELS(ID) on delete cascade
);

create table USER_CHEATS
(
	USER_ID uuid not null,
    GAME_ID uuid not null,
    ACCESS_LEVEL int not null,
    AUTH_END_DATE datetime not null,
    constraint FK_USER_CHEATS_USER foreign key (USER_ID) references USERS(ID) on delete cascade,
    constraint FK_USER_CHEATS_GAME foreign key (GAME_ID) references GAMES(ID) on delete cascade,
    constraint FK_USER_CHEATS_ACCESS foreign key (ACCESS_LEVEL) references ACCESS_LEVELS(ID) on delete cascade,
    primary key (USER_ID, GAME_ID, ACCESS_LEVEL)
);

CREATE TABLE TIME_KEYS
(
	TIME_KEY char(23) primary key,
    GAME_CHEAT_ID int not null,
	TIME_VALUE int null,
	KEY_GEN_DATE datetime not null default now(),
	ACTIVE boolean not null default true,
    constraint FK_TIME_KEYS_GAME_CHEAT foreign key (GAME_CHEAT_ID) references CHEAT_BINARYS(ID) on delete cascade
);
