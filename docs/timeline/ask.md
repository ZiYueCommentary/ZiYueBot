# 评价 <Badge type="info" text="愚人节" /> {#april-ask}

> 关于正常时间的“评价”命令，另见 *[评价](/harmony/ask.md)*。

**评价（愚人节） (April Ask)** 是一个 [鸿蒙命令](/harmony/)，是一个只在 [愚人节](/timeline/#april) 使用的 [鸿蒙命令](/harmony/)，可以从 [语录库](#reviews) 中随机获得一句语录。

## 语录库 {#reviews}

每次调用时，分别有 25% 的概率选择以下语录库，并随机抽取语录。

目前，子悦机器内拥有 [子悦](https://space.bilibili.com/383046501)、[EasyT_T（义贼哥）](https://space.bilibili.com/626767336)、[Asriel（山羊）](https://space.bilibili.com/417000557/) 和水豚哥的语录。

::: details 子悦
* 玩原神玩的
* 建议关注子悦解说
* 这是人能说出的话吗？
* 这是违反公序良俗的
* 说的道理
* 只有天知道
* 可能是脑子的问题
* 阅
* 我拒绝评价
* 退订
* 没事找事
* 这不可疑吗？
* What can I say?
* 我是你跌
:::

::: details EasyT_T（义贼哥）
* 不如 Rust
* 让 MSDN 为此解释吧
* 恩情课文
* 不安全，应该使用 Rust
* 赢赢赢
* 你说得不对
* 这是 unsafe 的
* 交给 Java 点评吧
* 建议 push 到 GitHub
* 建议写入原神剧情
* Made in heaven
:::

::: details Asriel（山羊）
* 我还没有想好
* 你自己不知道吗
* 你这样不好
* to explaIn this, we needs to talk about the elephant in the room, which is the Most obviouS prOblem, So what is that probleM, well, i dont know, but we cAn figure this out togetheR, frisT, lets talk about the trutH of our universe, this is a very tricky problem, why it is a very tricky problem? how cAn we know its a tricky problem? here is a example, wHats that exAmple? for example, i dont want to respond you, so i just talking about bullshits.
* 就是有人比较傻
* 你如此索求他人的智慧，是因为自己没有吗？
:::

::: details 水豚哥
* 据我所知，我一无所知
* 我们水豚爱好者怎么你了
* 肺雾啊，老弟
* 过于城市化了
* 别让等待，成为遗憾
:::

## 用法 {#usage}

```
/ask [question]
```

## 参数 {#params}

* `question` 是可选参数。

## 输出 {#output}

* `question` 为空时：

    ```
    子悦/义贼哥/山羊/水豚哥的评价是：{语录}
    ```

* `question` 不为空时：

    ```
    子悦/义贼哥/山羊/水豚哥对 {问题} 的评价是：{语录}
    ```

## 频率限制 {#rate-limit}

无