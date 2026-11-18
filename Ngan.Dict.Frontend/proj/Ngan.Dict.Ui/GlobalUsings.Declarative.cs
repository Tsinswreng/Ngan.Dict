// Ngan.Dict.Ui 專屬的全域 using。
//
// *為何放在專案內而不是前端共用的 GlobalUsing.cs*：只有本專案引了
// Avalonia.Markup.Declarative（及其生成器 MarkupDeclarative.Roslyn4），
// 放到共用檔會讓 Windows／Linux／Android 等宿主專案編譯失敗。
//
// *為何是全域*：聲明式寫法大量使用 .Text(vm, x => x.X)／.Children(...)／.With(...) 這類
// 生成器產生的擴展方法；每個檔案都手寫一次 using 只是噪聲。
global using Avalonia.Markup.Declarative;
