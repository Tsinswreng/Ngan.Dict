global using static global::Ngan.Dict.Ui.GlobalTools;

// 屬性選擇器與樣式糖（原 Tsinswreng.AvlnTools.Dsl.PropExtn，已隨 AvlnTools 一併搬入本專案）。
// 用 static import 與搬入前的寫法保持一致：調用點寫 `txt.PropText`、`MkrStyle.NoMargin(...)`。
global using static Ngan.Dict.Ui.Infra.AvlnSugar.ExtnProp;
global using Ngan.Dict.Ui.Infra.AvlnSugar;
