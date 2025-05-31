# 愚人云瓶 {#aprilbottles}

**愚人云瓶 (Aprilbottles)** 是只在 [愚人节](/timeline#april) 使用的云瓶数据库表。每次调用 [捞云瓶](/general/driftbottle/pick) 时都有 50% 的概率查询“愚人云瓶”数据库，而不是“[云瓶](/general/driftbottle/throw)”数据库。

一般而言，愚人云瓶包含子悦机器出现（2024 年 3 月 10 日）之前的 “QQ 群精华消息”。愚人云瓶在 [捞云瓶](/general/driftbottle/pick) 中显示为“负编号云瓶”，即使在数据库里 ID 是正的。

**愚人云瓶记录只能被手动添加到数据库表中。**

愚人云瓶被储存在子悦机器数据库的“aprilbottles”表中，包含以下字段：

| 字段       | 类型       | 解释        |
|----------|----------|-----------|
| id       | int(11)  | 云瓶 ID（主键） |
| username | tinytext | 作者用户名     |
| created  | date     | 创建日期      |
| content  | text     | 云瓶内容      |

[捞云瓶](/general/driftbottle/pick) 捞到愚人云瓶时：

  ```
  你捞到了 -{编号} 号瓶子！
  来自：{作者}
  日期：{时间}
  
  {内容}
  ```