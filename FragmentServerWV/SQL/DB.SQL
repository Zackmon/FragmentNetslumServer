create table BBS_Category
(
    categoryID   int         not null,
    categoryName varchar(33) not null,
    constraint BBS_Category_categoryID_uindex
        unique (categoryID)
);

alter table BBS_Category
    add primary key (categoryID);

create table BBS_Threads
(
    threadID    int default 1 not null,
    threadTitle varchar(37)   not null,
    categoryID  int           not null,
    constraint BBS_Threads_threadID_uindex
        unique (threadID),
    constraint BBS_Threads_BBS_Category_categoryID_fk
        foreign key (categoryID) references BBS_Category (categoryID)
);

alter table BBS_Threads
    add primary key (threadID);

create table BBS_Post_Meta
(
    unk0     int           not null,
    postID   int default 1 not null,
    unk2     int           not null,
    Date     datetime      not null,
    username varchar(20)   not null,
    subtitle varchar(20)   not null,
    title    varchar(37)   not null,
    unk3     varchar(44)   not null,
    threadID int           not null,
    constraint BBS_Post_Meta_postID_uindex
        unique (postID),
    constraint BBS_Post_Meta_unk0_uindex
        unique (unk0),
    constraint BBS_Post_Meta_BBS_Threads_threadID_fk
        foreign key (threadID) references BBS_Threads (threadID)
);

alter table BBS_Post_Meta
    add primary key (postID);

create table BBS_Post_Body
(
    postBodyID int default 1 not null,
    postBody   varchar(600)  not null,
    postID     int           not null,
    constraint BBS_Post_Body_postBodyID_uindex
        unique (postBodyID),
    constraint BBS_Post_Body_BBS_Post_Meta_postID_fk
        foreign key (postID) references BBS_Post_Meta (postID)
);

alter table BBS_Post_Body
    add primary key (postBodyID);

