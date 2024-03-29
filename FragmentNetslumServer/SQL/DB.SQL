
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
    threadTitle blob not null,
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
    username blob   not null,
    subtitle blob   not null,
    title    blob   not null,
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
    postBody   blob  not null,
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
    RECEIVER_ACCOUNT_ID int                  not null,
    DATE                datetime             not null,
    SENDER_ACCOUNT_ID   int                  not null,
    SENDER_NAME         blob          null,
    RECEIVER_NAME       blob         not null,
    MAIL_SUBJECT        blob         not null,
    MAIL_DELIVERED      tinyint(1) default 0 null,
    PRIMARY KEY (MAIL_ID),
    constraint MAIL_META_MAIL_ID_uindex
        unique (MAIL_ID)
);

create table MAIL_BODY
(
    MAIL_BODY_ID int auto_increment
        primary key,
    MAIL_ID      int         not null,
    MAIL_BODY    blob        null,
    FACE_ID      varchar(30) not null,
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
  `AccountID` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`ID`)
) AUTO_INCREMENT=40820 DEFAULT CHARSET=utf8mb4;

################################################Create Ranking Tables #####################################################


################################################ Player Account ID #####################################################
create table PLAYER_ACCOUNT_ID
(
    ID      int auto_increment
        primary key,
    SAVEID varchar(100) not null,
    constraint PLAYER_ACCOUNT_ID_SAVEID_uindex
        unique (SAVEID)
)AUTO_INCREMENT=1000;

################################################ Player Account ID #####################################################


################################################ Message Of The Day #####################################################

create table MessageOfTheDay
(
    ID      int auto_increment
        primary key,
    Message varchar(500) not null
);
################################################ Message Of The Day #####################################################




################################################ Guild and Ranking Update################################################
create table CharacterRepository
(
    playerID int auto_increment
        primary key,
    characterName blob null,
    classID int null,
    characterLevel int null,
    greeting blob null,
    guildID int null,
    guildMaster int null,
    modelNumber int null,
    onlineStatus tinyint(1) null,
    accountID int null,
    characterSaveID varchar(255) null,
    charHP int null,
    charSP int null,
    charGP int null,
    charOnlineGoat int null,
    charOfflineGoat int null,
    charGoldCoin int null,
    charSilverCoin int null,
    charBronzeCoin int null
)AUTO_INCREMENT=1000;


create table GuildRepository
(
    guildID int auto_increment
        primary key,
    guildName blob null,
    guildEmblem blob null,
    guildComment blob null,
    establishmentDate varchar(255) null,
    masterPlayerID int null,
    goldCoin int null,
    silverCoin int null,
    bronzeCoin int null,
    gp int null
)AUTO_INCREMENT=1000;

create table GuildItemShop
(
    itemShopID          int auto_increment
        primary key,
    guildID             int        null,
    itemID              int        null,
    quantity            int        null,
    generalPrice        int        null,
    memberPrice         int        null,
    availableForGeneral tinyint(1) null,
    availableForMember  tinyint(1) null
)AUTO_INCREMENT=1000;



################################################ Guild and Ranking Update################################################


################################################ News Section ###########################################################


create table news_section
(
    articleID    smallint unsigned auto_increment
        primary key,
    articleTitle varchar(33)  not null,
    articleBody  varchar(412) not null,
    articleDate  datetime     not null,
    articleImage blob         null,
    constraint news_section_articleID_uindex
        unique (articleID)
);


create table news_section_log
(
    id        int auto_increment
        primary key,
    saveId    varchar(100)      not null,
    articleId smallint unsigned not null,
    constraint news_section_log_id_uindex
        unique (id)
);

create index news_section_log_savedId_index
    on news_section_log (saveId);



################################################ News Section ###########################################################