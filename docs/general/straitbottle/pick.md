# 捞海峡云瓶 {#pick-straitbottle}

**捞海峡云瓶 (Pick Straitbottle)** 是一个 [一般命令](/general/)，可以捞起一支由 [扔海峡云瓶](/general/straitbottle/throw.md) 命令扔出的 [海峡云瓶](/general/straitbottle/)。

在 QQ 只能捞起由 Discord 扔出的云瓶，反之亦然。被捞起过的瓶子将不会被捞到。

## 用法 {#usage}

```
/捞海峡云瓶
```

## 参数 {#params}

无

## 输出 {#output}

* 正常调用时：

  ```
  你捞到了 {作者} 的瓶子！
  日期：{时间}
  
  {内容}
  ```

* 回复了其他消息时：

  ```
  使用云瓶命令时不可回复消息！
  ```

* 没有漂流瓶时：

  ```
  找不到瓶子！
  ```

* 调用频率达到限制时：

  ```
  频率已达限制（每分钟 1 条）
  ```

## 频率限制 {#rate-limit}

每次有效调用间隔不小于 1 分钟。