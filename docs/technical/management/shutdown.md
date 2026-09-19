# 停止运行

**停止运行 (Shutdown)** 是一个 [管理命令](/technical/management/index.md)，用于在紧急情况下绕过子悦机器的守护进程，直接停止子悦机器的运行。

使用该命令需要使用 [sudo](sudo.md) 提升 `ShutdownService` 特权。

## 用法 {#usage}

```
/sudo shutdown [reason]
```

## 参数 {#params}

* `reason` 是可选参数，用于记录停止运行的原因。

## 频率限制 {#rate-limit}

无
