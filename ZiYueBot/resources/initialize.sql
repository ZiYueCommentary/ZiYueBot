CREATE TABLE IF NOT EXISTS driftbottles
(
    id       int auto_increment primary key,
    userid   bigint   null,
    username tinytext null,
    created  datetime null,
    content  text     null,
    pickable boolean default true,
    views    int     default 0
);

CREATE TABLE IF NOT EXISTS win
(
    userid                bigint  default 0,
    username              tinytext null,
    channel               bigint  default 0,
    date                  date     null,
    score                 smallint null,
    prospered             boolean default false,
    miniWinDays           tinyint default 0,
    invoke_days           int     default 0,
    flexible_win_days     int     default 0,
    mini_win_days         int     default 0,
    middle_win_days       int     default 0,
    big_win_days          int     default 0,
    very_big_win_days     int     default 0,
    ultra_win_days        int     default 0,
    lose_days             int     default 0,
    couple_win_days       int     default 0,
    wind_window_days      int     default 0,
    alleviated_days       int     default 0,
    prosperity_days       int     default 0,
    prosperity_other_days int     default 0,
    PRIMARY KEY (userid, channel)
);

CREATE TABLE IF NOT EXISTS blacklists
(
    userid  bigint      default 0,
    command varchar(50) default 'all',
    time    datetime null,
    reason  text     null,
    PRIMARY KEY (userid, command)
);

CREATE TABLE IF NOT EXISTS invoke_records_general
(
    userid       bigint      not null,
    command      varchar(50) not null,
    first_invoke datetime    null,
    last_invoke  datetime    null,
    invoke_count int         null,
    PRIMARY KEY (userid, command)
);
