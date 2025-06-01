# 对话 <Badge type="warning" text="测试中命令" /> {#chat}

**对话 (Chat)** 是一个 [一般命令](/general/)，可以与 [DeepSeek-R1](https://api-docs.deepseek.com/zh-cn/news/news250120) 对话。

每次调用此命令时都为一次新对话。对话不支持联网搜索。

在 QQ 中，对话的每次回答被限制在 800 字以内，**且不包括思考过程**。QQ 中的回答被要求严格遵守中国的法律法规，且不得出现政治敏感、宗教信仰、历史事件等言论。

由于 Discord 的消息长度限制，超过 1000 字（大约）的内容会被截断。

**[愚人节](/timeline/#april) 时，对话被要求用百度贴吧风格，尽量刻薄地回答问题。**

## 用法 {#usage}

```
/chat [question]
```

## 参数 {#params}

* `question` 是问题内容。

## 输出 {#output}

```
已深度思考 {时间} 秒

{思考}

{内容}
```

## 频率限制 {#rate-limit}

* QQ：每次有效调用间隔不小于 5 分钟；
* Discord：每次有效调用间隔不小于 1 分钟；