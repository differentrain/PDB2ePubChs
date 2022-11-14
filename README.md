# PDB2ePubChs
自动将竖式排版的繁体uPDB电子书转换为横式排版的简体中文ePub文件。

支持批量转换，可以将多本书打包成一个合集。

## 命令行参数

+ 转换单个文件： `-c uPdb文件路径 [ePub文件路径]`
+ 转换多个文件： `-l uPdb文件目录 [输出目录]`
+ 打包多个文件： `-p 书籍名称 [-a 作者名称] uPdb文件目录 [ePub文件路径]`

### 备注

+ 方括号（`[]`）表示可选内容。

+ 如果路径中包括空格，必须用双引号（`""`）把完整路径包裹起来。

+ 打包多个uPdb文件时，将按照文件名的顺序进行打包。

## GUI

可以拖拽项目进行排序。

## 其他

如果遇到有保持竖式排版的符号，或者发现其他错误，可以[提交给我](https://github.com/differentrain/PDB2ePubChs/issues/new)。
