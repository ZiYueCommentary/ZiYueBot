# 群记过

::: tip 注意
该命令不是 [记过](penalty.md)。
:::

**群记过 (Public Penalty)** 是一个 [管理命令](/technical/management/index.md)，用于记录特定用户在群内的违规记录。与 [记过](penalty.md) 命令不同，该命令用于记录违反群规的行为，主要负责群内管理职能。

**使用该命令需要群管理员身份。**

## 用法 {#usage}

```
/群记过 [user] [reason]
```

## 参数 {#params}

* `user` 是被记过的用户。该参数必须为提及消息，否则无法正常调用。
* `reason` 是记过原因。

## 频率限制 {#rate-limit}

无

## 数据库表结构 {#structure}

群记过被储存在子悦机器数据库的“penalty_public”表中。

| 字段         | 类型         | 解释           |
|------------|------------|--------------|
| id         | int(11)    | 自增 ID（主键）    |
| userid     | bigint(20) | 被记过的用户       |
| channel_id | bigint(20) | 记过对应的群/频道 ID |
| created_at | datetime   | 记过时间         |
| created_by | bigint(20) | 进行记过操作的用户 ID |
| reason     | text       | 记过原因         |
| removed    | tinyint(1) | 记过是否被撤销      |