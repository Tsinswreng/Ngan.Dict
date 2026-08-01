using Ngaq.Core.Frontend.Hotkey;
file class DirDoc{
	str Doc =
$$"""
#Sum[全局快捷键系统]

此模块实现一个跨平台的全局快捷键监听系统，使应用即便在后台窗口不活跃也能监听并处理全局热键事件。
]
#Descr[
## 概述
这是一个接口+实现的架构，用于在不同平台上提供统一的热键注册和回调功能。

## 架构

### 核心接口 （位于 Ngaq.Core）
- `IHotkeyListener`：平台无关监听器 + 管理接口。
- `EHotkeyModifiers`：修饰符枚举（Ctrl/Shift/Alt/Win）。
- `EHotkeyKey`：键枚举（字母、数字、功能键、特殊键等）。

### 服务实现 (Ngaq.Ui)
- `IHotkeyListener` 本身通过 DI 注入各平台实现，已足够使用。

### 平台注册接口

为了避免平台判断代码散落在公共组件，新添加了 `{{nameof(I_RegisterGlobalHotKeys)}}` 接口。
此接口位于 `Ngaq.Ui.Infra.Hotkey`，由各平台专用程序集提供实现，负责在
对应平台启动时统一注册所有需要的全局快捷键。例如 Windows 实现为
`WinGlobalHotkeyRegistrar`，Linux 实现为 `LinuxGlobalHotkeyRegistrar`。
接口定义简单：
```csharp
public interface I_RegisterGlobalHotKeys{
    public IAnswer<obj?> RegisterGlobalHotKeys();
}
```
注册器通过 DI 注入到 App 中，仅在平台入口调用。


## 使用方法
1. 获取监听器并调用 `Register`，示例：
```csharp
var HotkeyListener = App.GetSvc<IHotkeyListener>();
var hk = new HotKey{
    Id = "my_hotkey",
    Modifiers = EHotkeyModifiers.Ctrl | EHotkeyModifiers.Shift,
    Key = EHotkeyKey.T,
    OnHotkey = async (Req, Ct) => {
        // ...
        return null;
    }
};
var ans = HotkeyListener.Register(hk);
if(!ans.Ok){
    Console.WriteLine("register failed: " + string.Join(";", ans.Errors ?? Array.Empty<string>()));
}
```
2. 注销：`await HotkeyListener.Unregister("my_hotkey", default);`
3. 清理所有：`await HotkeyListener.Cleanup(default);`

## 支持的键
- 字母 A–Z
- 数字 D0–D9
- 功能键 F1–F12
- 特殊键（Enter、Escape、Tab、空格、方向键、Home/End/走页等）

## 修饰符
Ctrl、Shift、Alt、Win，可组合。

## DI 配置

Windows：`z.AddSingleton<IHotkeyListener, WinHotkeyListener>();`
Linux：`z.AddSingleton<IHotkeyListener, LinuxHotkeyListener>();`

## 技术细节
- Windows 实现依赖窗口句柄，全局生效。
- 按键码映射示例：A–Z→0x41–0x5A，数字0–9→0x30–0x39，F1–F12→0x70–0x7B。
- 修饰符码：Ctrl=0x2；Shift=0x4；Alt=0x1；Win=0x8。

## 注意事项
- Windows / Linux 平台都会调用注册逻辑。
- Linux 当前通过 X11/XWayland 后端提供全局热键。
- 应用退出前调用 Cleanup。
- 字典存储注册项，对并发需谨慎。


]
""";

}
