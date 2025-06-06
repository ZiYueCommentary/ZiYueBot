# 赢 {#win}

> [!INFO] 免责声明
> 该命令中的内容仅供娱乐，请勿上升价值。

**赢 (Win)** 是一个 [一般命令](/general/)，是以 [张维为](https://baike.baidu.com/item/%E5%BC%A0%E7%BB%B4%E4%B8%BA/2650478) 教授为主题的 [今日人品](/harmony/jrrp.md) 命令。

该命令包含“[精准扶 win](#alleviation)”、“[共同富 win](#prosperity)”、“[风口飞 win](#fly)”和“[心心相 win](#couple)”四种事件。对应的事件内容 [详见下文](#events)。

该命令只能在群聊中调用。

## 用法 {#usage}

```
/win
```

## 参数 {#params}

无

## 术语 {#terms}

### 赢级 {#levels}

**赢级 (Levels)** 是指该命令中“今日人品”的数值，共有 7 个等级。

每日赢级的 **初始值** 为随机数生成，范围为 0~100。赢级会在当天调用者第一次调用此命令时生成。

| 赢级      | 等级  |
|---------|-----|
| 0~2     | 灵活赢 |
| 3~50    | 小赢  |
| 51~75   | 中赢  |
| 76~92   | 大赢  |
| 93~97   | 特大赢 |
| 98~99   | 赢麻了 |
| 100 及以上 | 输！  |

### 寄语 {#reviews}

**寄语 (Reviews)** 是张维为的随机语录，会在生成 [赢级](#levels) 时显示。根据赢级的等级，寄语的内容也会有所不同。

:::: tabs

::: tab 灵活赢
* 我真的觉得我们千万不能太天真。
* 好像真的要出大问题。
* 现在这个水准还是太低了。
* 我们决不允许这样。
* 这个差距将被克服。
* 真是什么问题都不能回避了。
:::

::: tab 小赢
* 我觉得我们真的要自信一点。
* 只要你自信，怎么表达都可以。
* 我们一点都不害怕竞争。
* 我们的回旋余地特别大。
* 很显然就是觉得不服气。
:::

::: tab 中赢
* 我想更精彩的故事还在后面。
* 这使美国感到害怕了。
* 现在确实在开始超越美国了。
* 至少美国今天还做不到。
:::

::: tab 大赢
* 这个趋势还会持续下去。
* 我们已经不是一般的先进了。
* 我们不是一般的领先，对不对？
* 别人都不可能超越我们。
* 很好地展示了一种自信。
* 这是基本的趋势。
:::

::: tab 特大赢
* 这是中国崛起最精彩的地方。
* 我们已经对美国形成了巨大的压力。
* 必须给美国迎头痛击！
* 你真可能会创造世界奇迹的。
* 这种自信令人有点回味无穷。
* 完胜所有西方国家。
* 孰优孰劣一目了然。
:::

::: tab 赢麻了
* 已经震撼了这个世界。
* 这是一种发自内心的钦佩。
* 这种震撼效果前所未有。
* 至今引以为荣。
* 结果是一锤定音、釜底抽薪的胜利。
:::

::: tab 输！
* 你赢赢赢，最后是输光光。
:::

::::


当生成赢级时触发了 [事件](#events)，将显示不属于该列表内的寄语。详情请查看 [事件](#events)。

### 风口 {#window}

**风口 (Wind window)** 是一个可以提高赢级的时间段。在该时间段内生成的每日赢级将会变为初始值的 1.4 倍。

该命令每天会随机抽取 1 小时作为风口的时间段，范围为 9 时~ 21 时。

## 事件 {#events}

### 精准扶 win {#alleviation}

**精准扶 win (精准扶贫, Targeted poverty alleviation)** 是一个在生成每日赢级时尝试激活的事件。

生成每日赢级时，如果 [赢级等级](#levels) 属于小赢，则增加数据库的小赢天数计数。当生成每日赢级时发现计数器达到三天，则激活“精准扶 win”事件。

激活“精准扶 win”事件后，数据库的小赢计数器将被清零，当天的赢级将会变为初始值的 1.5 倍。

激活该事件后会显示的 [寄语](#reviews) 包括：

* 现在美国竞争不过我们。
* 我们要更上一层楼了。
* 我们手中的牌太多了。
* 现在我们有很多新的牌可以打。
* 该出手的时候一定要出手。
* 局面马上就打开了。
* 通过了这场全方位的压力测试。

### 共同富 win {#prosperity}

**共同富 win (共同富裕, Common prosperity)** 是一个在每次命令被调用时尝试激活的事件。

每次命令被调用时，命令将查找该群聊内当天赢级最高的人。如果其 [赢级等级](#levels) 达到大赢或以上，且不是命令调用者本人，则激活“共同富 win”事件。

“共同富 win”事件会将命令调用者的当天赢级，变为调用者和赢级最高人的平均值。激活此事件后，数据库的 [小赢计数器](#alleviation) 将被清零。

激活该事件后会显示的 [寄语](#reviews) 包括：

* 令人感动之至。
* 有时候是能合作共赢的。
* 不要再不自信了。
* 这一定是美丽的。

### 风口飞 win {#fly}

**风口飞 win** 是一个在生成每日赢级时尝试激活的事件。

生成每日赢级时，如果当前时间处于 [风口](#window)，则每日赢级变为初始值的 1.4 倍。

“风口飞 win”事件在机器全局只能被激活一次。

### 心心相 win {#couple}

**心心相 win (心心相印, Winning couple)** 是一个在每次命令被调用时尝试激活的事件。

每次命令被调用时，命令将查找该群聊内当天赢级与调用者赢级和为 99 的用户，并输出祝福语。

## 输出 {#output}

* 生成每日赢级，且未激活任何 [事件](#events) 时：

    ```
  恭喜 {用户名} 在 {日期} 赢了一次！
  {用户名} 的赢级是：{赢级}，属于 {赢级等级}。
  维为寄语：{寄语}
  ```

* 生成每日赢级，且激活 [精准扶 win](#alleviation) 事件时：

    ```
  恭喜 {用户名} 在 {日期} 受到精准扶 win，赢级提高 50%！
  {用户名} 的赢级是：{赢级}，属于 {赢级等级}。
  维为寄语：{寄语}
  ```

* 生成每日赢级，且激活 [风口飞 win](#fly) 事件时：

    ```
  恭喜 {用户名} 在 {日期} 乘上风口，赢级提高 40%！
  {用户名} 的赢级是：{赢级}，属于 {赢级等级}。
  维为寄语：{寄语}
  ```

* 今日赢级已生成时：

    ```
  {用户名} 已经在 {日期} 赢过了，请明天再继续赢。
  你今天的赢级是：{赢级}，属于 {赢级等级}。
  ```

* 激活 [共同富 win](#prosperity) 事件成功时：

    ```
  恭喜 {用户名} 在 {赢级最高人} 的帮扶下实现共同富 win，使赢级达到了 {新赢级}！
  维为寄语：{寄语}
  ```

* 激活 [共同富 win](#prosperity) 事件失败时：

    ```
  最赢者不够努力，赢级尚未达到大赢，无力帮扶。
  ```

* 激活 [心心相 win](#couple) 事件时：

    ```
  恭喜 {用户名1} 与 {用户名2} 的赢级之和达到 99，实现心心相 win！
  愿你们永结同心，在未来的日子里风雨同舟、携手共赢！
  {下文图片}
  ```
  
   ![zvv](/zvv.jpeg)


## 频率限制 {#rate-limit}

无