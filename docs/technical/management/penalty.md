# 记过

::: tip 注意
该命令不是 [群记过](public-penalty.md)。
:::

**记过 (Penalty)** 是一个 [管理命令](/technical/management/index.md)，用于记录特定用户的违规记录。一般而言，行为尚未严重到需要加入 [黑名单](/technical/blacklists.md) 的用户，都会被记录在此。与黑名单不同，记过系统不附带任何的惩罚措施，仅作为“犯罪记录”使用。

使用该命令需要 `CreatePenalty` 特权。

## 用法 {#usage}

```
/记过 [user] [reason]
```

## 参数 {#params}

* `user` 是被记过的用户。该参数必须为提及消息，否则无法正常调用。
* `reason` 是记过原因。

## 频率限制 {#rate-limit}

无

## 数据库表结构 {#structure}

记过被储存在子悦机器数据库的“penalty”表中。

| 字段         | 类型         | 解释                             |
|------------|------------|--------------------------------|
| id         | int(11)    | 自增 ID（主键）                      |
| userid     | bigint(20) | 被记过的用户                         |
| channel_id | bigint(20) | 记过发生的所在群/频道 ID                 |
| created_at | datetime   | 记过时间                           |
| created_by | bigint(20) | 进行记过操作的用户 ID                   |
| community  | tinyint(1) | 记过是否来自无对应特权的群管理（保留，固定 `false`） |
| reason     | text       | 记过原因                           |
| removed    | tinyint(1) | 记过是否被撤销                        |