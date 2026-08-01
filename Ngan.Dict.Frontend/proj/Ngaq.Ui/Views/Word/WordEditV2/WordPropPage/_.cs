namespace Ngaq.Ui.Views.Word.WordEditV2.WordPropPage;

file class DirDoc{
	str Doc =
"""
#Sum[
單詞屬性分頁與單行屬性 ViewModel。
]

#Descr[
屬性表只顯示三列：
- 序號
- 鍵
- 值

值列只取 `PoWordProp` 中實際有值的那一個字段做展示；
若文本過長，則統一走公用文本略縮工具，避免表格被超長內容撐壞。

列表列寬規則：
- 前兩列 `Auto`
- 最後一列 `1 Star`
]
""";
}
