# 子悦机器数据库初始化
# 以下数据库表为 MySQL 5.7 设计，任何比 MySQL 正经的数据库应该都可以用

# 漂流云瓶
CREATE TABLE IF NOT EXISTS driftbottles
(
    id       int auto_increment primary key,
    userid   bigint   null,
    username tinytext null,
    created  datetime null,
    content  text     null,
    pickable boolean default true,
    views    int     default 0
) CHARSET = utf8mb4;

# 海峡云瓶
CREATE TABLE IF NOT EXISTS straitbottles
(
    id          int auto_increment primary key,
    userid      bigint   null,
    username    tinytext null,
    created     datetime null,
    content     text     null,
    fromDiscord boolean  null,
    picked      boolean default false,
    picked_time datetime null
) CHARSET = utf8mb4;

# 赢
CREATE TABLE IF NOT EXISTS win
(
    userid                bigint  default 0,
    username              tinytext null,
    channel               bigint  default 0,
    date                  date     null,
    score                 tinyint  null,
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
) CHARSET = utf8mb4;

# 愚人云瓶
CREATE TABLE IF NOT EXISTS aprilbottles
(
    id       int auto_increment primary key,
    username tinytext null,
    created  date     null,
    content  text     null
) CHARSET = utf8mb4;

# 黑名单
CREATE TABLE IF NOT EXISTS blacklists
(
    userid  bigint      default 0,
    command varchar(50) default 'all',
    time    datetime null,
    reason  text     null,
    PRIMARY KEY (userid, command)
) CHARSET = utf8mb4;

# 记过
CREATE TABLE IF NOT EXISTS penalty
(
    id         int auto_increment primary key,
    userid     bigint  default 0,
    channel_id bigint  default 0,
    created_at datetime null,
    created_by bigint   null,
    community  boolean default true,
    reason     text     null,
    removed    boolean default false,
    INDEX index_user (userid, removed)
) CHARSET = utf8mb4;

# 群管理记过
CREATE TABLE IF NOT EXISTS penalty_public
(
    id         int auto_increment primary key,
    userid     bigint  default 0,
    channel_id bigint  default 0,
    created_at datetime null,
    created_by bigint   null,
    reason     text     null,
    removed    boolean default false,
    INDEX index_user (userid, channel_id, removed)
) CHARSET = utf8mb4;

# 赞助者
CREATE TABLE IF NOT EXISTS sponsors
(
    userid bigint default 0 primary key,
    expiry date null
) CHARSET = utf8mb4;

# 绘画额度
CREATE TABLE IF NOT EXISTS draw
(
    userid        bigint default 0 primary key,
    current_month date null,
    limitation    int  null,
    consumed      int  null
) CHARSET = utf8mb4;

# 调用统计
CREATE TABLE IF NOT EXISTS invoke_records_general
(
    userid       bigint      not null,
    command      varchar(50) not null,
    first_invoke datetime    null,
    last_invoke  datetime    null,
    invoke_count int         null,
    PRIMARY KEY (userid, command),
    INDEX index_userid (userid)
) CHARSET = utf8mb4;

# 俄罗斯轮盘
CREATE TABLE IF NOT EXISTS revolver
(
    userid               bigint   not null primary key,
    first_invoke         datetime null,
    last_invoke          datetime null,
    start_count          int default 0,
    shooting_self_count  int default 0,
    shooting_other_count int default 0,
    shooting_self_death  int default 0,
    shooting_other_death int default 0,
    rotating_count       int default 0,
    restart_count        int default 0,
    being_shot           int default 0
) CHARSET = utf8mb4;

# 云瓶审核队列
# 以下为最基本的表结构，子悦机器实际上还有更多私有数据
CREATE TABLE IF NOT EXISTS driftbottles_queue
(
    queue_id int auto_increment primary key,
    userid   bigint   null,
    username tinytext null,
    created  datetime null,
    content  text     null,
    reviewed tinyint(1) default 0
) CHARSET = utf8mb4;

# 云瓶星标
CREATE TABLE IF NOT EXISTS stargazers
(
    userid    bigint     default 0,
    star_at   datetime null,
    bottle_id int        default 0,
    removed   tinyint(1) default 0,
    PRIMARY KEY (userid, bottle_id),
    INDEX index_bottle_id (bottle_id, removed),
    INDEX index_userid (userid, removed)
) CHARSET = utf8mb4;

# 特权
CREATE TABLE IF NOT EXISTS privileges
(
    userid        bigint not null primary key,
    flags         bigint null,
    active_counts int    null
) CHARSET = utf8mb4;