# 海峡云瓶列表 {#list-straitbottle}

**海峡云瓶列表 (List Straitbottle)** 是一个 [一般命令](/general/)，可以查看当前 [海峡云瓶](/general/straitbottle/) 的数据。

该命令会列出瓶子的总数量、当前平台可捞起的瓶子数量，以及由调用者扔出的瓶子数量。

## 用法 {#usage}

```
/海峡云瓶列表
```

## 参数 {#params}

无

## 输出 {#output}

* 正常调用时：

  ```
  海峡中共有 {总数} 支瓶子，其中 {可捞起数} 支可被 {当前平台} 捞起，{调用者扔出数} 支由你扔出
  ```

* 调用频率达到限制时：

  ```
  频率已达限制（10 分钟 1 条）
  ```

## 频率限制 {#rate-limit}

每次有效调用间隔不小于 10 分钟。