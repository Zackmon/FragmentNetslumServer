
################################################Create BBS Tables #####################################################
create table BBS_Category
(
    categoryID   int         auto_increment,
    categoryName varchar(33) not null,
    PRIMARY KEY (categoryID),
    constraint BBS_Category_categoryID_uindex
        unique (categoryID)
);

create table BBS_Threads
(
    threadID  int             auto_increment,
    threadTitle varchar(37)   not null,
    categoryID  int           not null,
    PRIMARY KEY (threadID),
    constraint BBS_Threads_threadID_uindex
        unique (threadID),
    constraint BBS_Threads_BBS_Category_categoryID_fk
        foreign key (categoryID) references BBS_Category (categoryID)
);

create table BBS_Post_Meta
(
    unk0     int           not null,
    postID   int           auto_increment,
    unk2     int           not null,
    Date     datetime      not null,
    username varchar(20)   not null,
    subtitle varchar(20)   not null,
    title    varchar(37)   not null,
    unk3     varchar(44)   not null,
    threadID int           not null,
    PRIMARY KEY (postID),
    constraint BBS_Post_Meta_postID_uindex
        unique (postID),
    constraint BBS_Post_Meta_unk0_uindex
        unique (unk0),
    constraint BBS_Post_Meta_BBS_Threads_threadID_fk
        foreign key (threadID) references BBS_Threads (threadID)
);


create table BBS_Post_Body
(
    postBodyID int           auto_increment,
    postBody   varchar(600)  not null,
    postID     int           not null,
    PRIMARY KEY (postBodyID),
    constraint BBS_Post_Body_postBodyID_uindex
        unique (postBodyID),
    constraint BBS_Post_Body_BBS_Post_Meta_postID_fk
        foreign key (postID) references BBS_Post_Meta (postID)
);

################################################Create BBS Tables #####################################################





################################################Create Mail Tables #####################################################
create table MAIL_META
(
    MAIL_ID             int auto_increment,
    RECEIVER_ACCOUNT_ID int          not null,
    DATE                datetime     not null,
    SENDER_ACCOUNT_ID   int          not null,
    SENDER_NAME         varchar(18)  null,
    RECEIVER_NAME       varchar(18)  not null,
    MAIL_SUBJECT        varchar(128) not null,
    PRIMARY KEY (MAIL_ID),
    constraint MAIL_META_MAIL_ID_uindex
        unique (MAIL_ID)
);


create table MAIL_BODY
(
    MAIL_BODY_ID int auto_increment,
    MAIL_ID   int   not null ,
    MAIL_BODY varchar(1200) null,
    FACE_ID   varchar(30)   not null,
    PRIMARY KEY (MAIL_BODY_ID),
    constraint MAIL_BODY_MAIL_ID_uindex
        unique (MAIL_ID),
    constraint MAIL_BODY_MAIL_META_MAIL_ID_fk
        foreign key (MAIL_ID) references MAIL_META (MAIL_ID)
);

################################################Create Mail Tables #####################################################





################################################Create Ranking Tables #####################################################
CREATE TABLE `ranking_data` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `AntiCheatEngineResult` tinytext NOT NULL,
  `loginTime` text NOT NULL,
  `DiskID` text NOT NULL,
  `SaveID` text NOT NULL,
  `CharacterSaveID` text NOT NULL,
  `CharacterName` text NOT NULL,
  `CharacterLevel` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `CharacterClassName` text NOT NULL,
  `CharacterHP` smallint(5) unsigned NOT NULL DEFAULT '0',
  `CharacterSP` smallint(5) unsigned NOT NULL DEFAULT '0',
  `CharacterGP` mediumint(8) unsigned NOT NULL DEFAULT '0',
  `GodStatueCounterOnline` mediumint(8) unsigned NOT NULL DEFAULT '0',
  `AverageFieldLevel` tinyint(3) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`ID`)
) AUTO_INCREMENT=40820 DEFAULT CHARSET=utf8mb4;

################################################Create Ranking Tables #####################################################
