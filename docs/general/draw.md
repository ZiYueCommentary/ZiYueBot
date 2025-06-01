# 绘画 <Badge type="tip" text="赞助者命令" /> {#draw}

> [!IMPORTANT] 注意
> 该命令仅允许子悦机器的 [赞助者](../sponsors) 调用。

**绘画 (Draw)** 是一个 [一般命令](/general/)，可以使用 [通义万相](https://tongyi.aliyun.com/wanxiang/) 通过文字生成图片。

调用此命令后，机器会生成一张1024*1024像素的图片。

**[子悦生日](/timeline/#ziyue-birthday) 时对所有用户开放。**

## 用法 {#usage}

```
/draw [prompt]
```

## 参数 {#params}

* `prompt` 是一段描述图片的**正向提示词**
  ，支持中英文。另见：[《文生图Prompt指南》](https://help.aliyun.com/zh/model-studio/use-cases/text-to-image-prompt)

## 输出 {#output}

* 绘画成功时：

    ```
    {图片}
    ```

* 内容违规或绘画失败时：

    ```
  任务执行失败：{错误}
  ```
  
  ```
  服务连接超时。
  ```

  ```
  第三方拒绝：涉嫌知识产权风险。
  ```

* 未找到调用者赞助信息时：

    ```
  您不是子悦机器的赞助者。
  本命令仅供赞助者使用，请在爱发电赞助“子悦机器”方案以调用命令。
  https://afdian.com/a/ziyuecommentary2020
  ```

* 调用者赞助已过期时：

    ```
  您的赞助已过期（{赞助日期}）
  子悦机器每次赞助的有效期为 365 天。
  本命令仅供赞助者使用，请在爱发电赞助“子悦机器”方案以调用命令。
  https://afdian.com/a/ziyuecommentary2020
    ```

## 频率限制 {#rate-limit}

每次有效调用间隔不小于 1 分钟。