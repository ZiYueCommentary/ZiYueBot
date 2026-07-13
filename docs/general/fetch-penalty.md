# 查询记过

**查询记过 (Fetch Penalty)** 是一个 [一般命令](/general/)，可以查询特定用户的 [记过](/technical/management/penalty.md) 和所在群的 [群记过](/technical/management/public-penalty.md)。

## 用法 {#usage}

```
/查询记过 [user]
```

## 参数 {#params}

* `user` 是被查询的用户，留空默认为自己。该参数必须为提及消息，否则无法正常调用。

## 输出 {#output}

```
{用户名} ({账户ID}) 的记过数据统计：
该用户共有全局记过 {数量} 条
{全局记过列表}
该用户共有本群记过 {数量} 条
{群记过列表}
```

## 频率限制 {#rate-limit}

每次调用间隔 10 分钟。