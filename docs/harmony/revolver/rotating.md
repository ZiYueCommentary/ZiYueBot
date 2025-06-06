# 转轮 {#rotating}

**转轮 (Rotating)** 是一个 [鸿蒙命令](/harmony/)，可以在 [俄罗斯轮盘](/harmony/revolver/) 游戏中，使左轮转到下一个膛室，而不向任何人开枪。在调用该命令之前，必须先调用 [开始俄罗斯轮盘](/harmony/revolver/start) 命令。

当在最后一个膛室转轮时，该局游戏结束。

该命令只能在群聊中调用。

## 用法 {#usage}

```
/转轮
```

## 参数 {#params}

无

## 输出 {#output}

* 转轮后还有剩余机会时：

    ```
  已转轮，轮盘中还剩 {剩余开枪机会} 个膛室未击发。
  ```

* 转轮后没有剩余机会时：

    ```
  已转轮，轮盘中还剩 0 个膛室未击发。本局俄罗斯轮盘结束。
  ```

* 游戏未开始时：

  ```
  游戏未开始，发送“开始俄罗斯轮盘”来开始
  ```

* 调用频率达到限制时：

    ```
  频率已达限制（每 3 秒 1 条）
  ```

## 频率限制 {#rate-limit}

每次有效调用间隔不小于 3 秒。