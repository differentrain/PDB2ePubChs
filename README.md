# PDB2ePubChs
自动将竖式排版的繁体uPDB/PDB电子书文件转换为横式排版的简体中文ePub文件。

支持批量转换，可以将多本书打包成一个合集。

## 命令行参数

+ 转换单个文件： `-c [-a 作者名称] uPdb文件路径 [ePub文件路径]`
+ 转换多个文件： `-l uPdb文件目录 [输出目录]`
+ 打包多个文件： `-p 书籍名称 [-a 作者名称] uPdb文件目录 [ePub文件路径]`

### 备注

+ 方括号（`[]`）表示可选内容。

+ 如果路径中包括空格，必须用双引号（`""`）把完整路径包裹起来。

+ 打包多个uPdb文件时，将按照文件名的顺序进行打包。

## GUI

可以拖拽项目进行排序。

`[1.1.0]` 当打包成合集时，如果只指定了作者名称，则默认使用《XXX作品集》作为合集名称。

## 配置字符转换表

作者精力有限，不可能找到所有需要转换的符号，有些汉字也无法准确从繁体映射到简体。

现在，可以通过程序目录下的 `ReplacedChars.xml` 文件来配置字符转换表，程序会根据转换表来进行相应的处理。

以下是目前程序默认的转换表：

```
<?xml version="1.0" encoding="utf-8"?>
<ArrayOfReplacedChar xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <ReplacedChar Org="︿" Rep="〈" />
  <ReplacedChar Org="﹀" Rep="〉" />
  <ReplacedChar Org="︽" Rep="《" />
  <ReplacedChar Org="︾" Rep="》" />
  <ReplacedChar Org="︹" Rep="〔" />
  <ReplacedChar Org="︺" Rep="〕" />
  <ReplacedChar Org="︻" Rep="【" />
  <ReplacedChar Org="︼" Rep="】" />
  <ReplacedChar Org="﹃" Rep="‘" />
  <ReplacedChar Org="﹄" Rep="’" />
  <ReplacedChar Org="﹁" Rep="“" />
  <ReplacedChar Org="﹂" Rep="”" />
  <ReplacedChar Org="︷" Rep="｛" />
  <ReplacedChar Org="︸" Rep="｝" />
  <ReplacedChar Org="︵" Rep="（" />
  <ReplacedChar Org="︶" Rep="）" />
  <ReplacedChar Org="｜" Rep="—" />
  <ReplacedChar Org="│" Rep="…" />
  <ReplacedChar Org="︙" Rep="…" />
  <ReplacedChar Org="︱" Rep="—" />
  <ReplacedChar Org="殭" Rep="僵" />
  <ReplacedChar Org="屍" Rep="尸" />
  <ReplacedChar Org="摀" Rep="捂" />
  <ReplacedChar Org="紮" Rep="扎" />
  <ReplacedChar Org="慾" Rep="欲" />
</ArrayOfReplacedChar>
```
当需要进行增补时，直接将 `<ReplacedChar Org="要转换的文字" Rep="对应文字" />` 添加到列表中即可。

## 其他

如果遇到有保持竖式排版的符号，或者发现其他错误，可以[提交给我](https://github.com/differentrain/PDB2ePubChs/issues/new)。
