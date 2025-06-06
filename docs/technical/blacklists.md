# 黑名单 {#blacklists}

**黑名单 (Blacklists)** 是一种针对特定用户的惩罚措施，用于剥夺用户对于部分或全部命令的调用权。

在用户调用命令时，子悦机器会先检查用户是否在黑名单内。在黑名单内则拒绝调用，并返回被封禁的通知。

子悦机器中的黑名单记录不包括封禁时长，因此在技术上，**黑名单是永久的**。但同时，黑名单中的记录可以被删除，意味着针对用户的封禁可以被撤销。

**黑名单记录只能被手动添加到数据库表中。**

黑名单被储存在子悦机器数据库的“blacklists”表中，包含以下字段：

| 字段      | 类型          | 解释        |
|---------|-------------|-----------|
| userid  | bigint(20)  | 用户 ID（主键） |
| command | varchar(50) | 命令 ID（主键） |
| time    | datetime    | 封禁的开始时间   |
| reason  | text        | 封禁原因      |

黑名单分为 **命令封禁** 和 **全局封禁**。命令封禁针对特定命令，而全局封禁则是所有命令。

如果要添加全局封禁记录，只需要把 `command` 字段设置为 `all`。

子悦机器找到全局封禁记录时：

```
您已被禁止使用子悦机器！
时间：{日期时间}
原因：{原因}
用户协议：https://docs.ziyuebot.cn/tos.html
```

找到命令封禁记录时：

```
您已被禁止使用该命令！
时间：{日期时间}
原因：{原因}
用户协议：https://docs.ziyuebot.cn/tos.html
```