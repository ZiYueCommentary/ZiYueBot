# 评价 {#ask}

**评价 (Ask)** 是一个 [鸿蒙命令](/harmony)，可以从 [语录库](https://raw.githubusercontent.com/ZiYueCommentary/ZiYueBot/refs/heads/main/ZiYueBot/resources/words.txt) 中随机获得一句 [张维为](https://baike.baidu.com/item/%E5%BC%A0%E7%BB%B4%E4%B8%BA/2650478) 教授语录。

## 用法 {#usage}

```
/ask [question]
```

## 参数 {#params}

* `question` 是可选参数。不为空时可以让张教授对此问题做出回答。

## 输出 {#output}

* `question` 为空时：

    ```
    张教授的评价是：{语录}
    ```

* `question` 不为空时：

    ```
    张教授对 {问题} 的评价是：{语录}
    ```

## 频率限制 {#rate-limit}

无