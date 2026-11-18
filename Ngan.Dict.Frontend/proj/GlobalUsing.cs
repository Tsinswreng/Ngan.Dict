//[2025-06-26T13:37:22.811+08:00_W26-4]
global using Tsinswreng.CsCore;
global using BindingMode = Avalonia.Data.BindingMode;
global using IBinding = Avalonia.Data.BindingBase;

#region Grid
global using ColDef = Avalonia.Controls.ColumnDefinition;
global using RowDef = Avalonia.Controls.RowDefinition;
global using GUT = Avalonia.Controls.GridUnitType;
global using Style = Avalonia.Styling.Style;
#endregion Grid
global using TxtAlign = Avalonia.Media.TextAlignment;
global using VAlign = Avalonia.Layout.VerticalAlignment;
global using HAlign = Avalonia.Layout.HorizontalAlignment;
// 舊寫法：Tsinswreng.Avln.Dsl 正逐步撤除，新寫與改動過的檔案不准再用。
// （Avalonia.Markup.Declarative 的 global using 放在 Ngan.Dict.Ui 專案內，
//  因為只有該專案引了那個套件；本檔是整個前端共用的。）
global using Tsinswreng.Avln.Dsl;

