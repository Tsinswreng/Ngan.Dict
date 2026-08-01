file class DirDoc{
	str Doc =
$$"""
#Sum[
AI 智能體
]
#Descr[
程序接入AI智能體



基于你提供的项目架构（.NET 10 + AOT、Avalonia 客户端、分层清晰），在客户端实现一个 AI Agent（对话式智能体）是一个很自然的扩展。你希望用户通过自然语言与智能体交互，智能体理解意图后调用后端接口完成操作。下面我会从需求描述和技术实现建议两个角度给出方案。

---

## 一、需求描述（可以作为 Spec 的一部分）

### 1. 功能目标
在客户端（Avalonia）内嵌一个对话式 AI Agent，用户通过自然语言输入指令，Agent 理解意图并调用已有业务接口（如单词查询、添加学习记录、切换学习方案、查词典等）完成任务，并返回友好的执行结果。

### 2. 用户场景举例
- 查单词：用户说“查询一下‘apple’的释义”，Agent 调用词典服务返回释义。
- 添加单词：用户说“把‘apple’加入生词本”，Agent 调用单词添加接口。
- 开始学习：用户说“我要复习英语单词”，Agent 获取待学习单词列表并展示。
- 切换学习方案：用户说“切换到日语学习方案”，Agent 调用学习方案接口。
- 统计查询：用户说“我本周学了多少个新词”，Agent 查询学习记录统计。

### 3. 非功能需求
- AOT 兼容：所有 Agent 相关代码必须兼容 Native AOT，不依赖反射或动态代码生成。
- 响应式：对话界面实时显示思考过程和结果。
- 安全：API Key 等敏感信息不得硬编码，使用安全存储（如用户级凭据管理器）。
- 可扩展：后续可支持更多指令和插件。

---

## 二、技术实现建议

### 1. 整体架构（在现有分层基础上扩展）

```
Avalonia UI (Ngaq.UI)
   ↓ 用户输入
Agent ViewModel (新增)
   ↓ 调用
Agent Service (新增，位于 Ngaq.Core 或 Ngaq.Frontend)
   ↓ 意图识别 + 参数提取
LLM Client (封装 OpenAI / Azure / 本地模型，AOT 安全)
   ↓ 调用
Existing Business Services (ISvcWordV2, ISvcDictionary, ISvcStudyPlan ...)
```

### 2. 关键模块设计

#### 2.1 Agent Service 接口（放在 `Ngaq.Core/Shared/Agent/`）

```csharp
public interface IAgentService
{
    IAsyncEnumerable<AgentResponse> ChatAsync(
        AgentRequest request,
        CT ct
    );
}

public record AgentRequest(
    string UserInput,
    List<ChatMessage> History // 可选，维护上下文
);

public record AgentResponse(
    string Content,          // 返回给用户的自然语言
    bool IsComplete,         // 是否结束（支持流式）
    object? DebugInfo = null // 可选，用于调试
);
```

#### 2.2 意图识别与工具调用（Function Calling）

推荐方案：使用 LLM 的 Function Calling 能力（OpenAI / Azure 支持良好），将你现有的业务接口封装为“工具”（Tools），由 LLM 决定调用哪个工具、传什么参数，然后 Agent 执行并返回结果。

AOT 友好实现方式：
- 不使用反射动态调用，而是预定义一个工具注册表（`Dictionary<string, Func<...>>`）。
- 每个工具对应一个业务接口方法，参数为强类型 JSON 对象（使用 `System.Text.Json` 序列化）。

示例工具定义：

```csharp
public class AgentTool
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public JsonDocument Parameters { get; set; } // JSON Schema
    public Func<JsonDocument, CT, Task<string>> Execute { get; set; }
}
```

注册示例：

```csharp
tools.Add(new AgentTool
{
    Name = "lookup_word",
    Description = "查询单词的释义",
    Parameters = GetWordLookupSchema(),
    Execute = async (args, ct) => {
        var word = args.RootElement.GetProperty("word").GetString();
        var result = await _dictSvc.Lookup(...);
        return result.Definition;
    }
});
```

#### 2.3 LLM 客户端实现（AOT 安全）

- 使用 `HttpClient` 直接调用 OpenAI/Azure API（避免使用 `RestSharp` 等可能依赖反射的库）。
- 使用 `System.Text.Json` 的源生成器（`JsonSourceGenerator`）进行序列化，避免运行时反射。
- 支持流式响应（`IAsyncEnumerable<string>`）以实现打字机效果。

示例 API 调用（简化）：

```csharp
var request = new {
    model = "gpt-4",
    messages = messages,
    tools = toolSchemas,
    stream = true
};
var response = await httpClient.PostAsJsonAsync(url, request, jsonOptions, ct);
```

#### 2.4 上下文管理

- 每个对话会话维护一个 `List<ChatMessage>`，包含 system prompt、user、assistant、tool 消息。
- System Prompt 描述 Agent 的角色、可用工具、调用规范（例如：“你是一个背单词助手，可以调用以下工具…”）。
- 建议限制上下文长度（如最近 20 轮），避免 token 超限。

#### 2.5 UI 集成（Avalonia）

- 在 `Ngaq.UI` 中新增一个 `AgentView`（聊天界面），绑定 `AgentViewModel`。
- ViewModel 负责：
  - 维护消息列表（`ObservableCollection<ChatMessage>`）。
  - 调用 `IAgentService.ChatAsync` 流式获取回复。
  - 处理工具调用结果并更新界面。
- 由于是客户端，建议在后台线程执行 LLM 请求，避免阻塞 UI。

---

## 三、AOT 特别注意事项

1. 禁用反射：不使用 `Assembly.Load`、`Activator.CreateInstance`、`MethodInfo.Invoke` 等。
2. JSON 序列化：必须使用源生成器模式（`JsonSerializerContext`）。
3. 动态代码：不要使用 `System.Linq.Expressions` 或 `Microsoft.CodeAnalysis.CSharp.Scripting`。
4. HTTP 客户端：使用 `IHttpClientFactory` 管理生命周期，避免 Socket 耗尽。
5. 依赖注入：Agent Service 可通过构造函数注入现有的业务服务（如 `ISvcWordV2`、`ISvcDictionary`）。

---

## 四、可选增强

- 本地模型支持：若用户有隐私要求或离线需求，可集成 `LLaMA.cpp` 的 C# 绑定（需确认 AOT 兼容性）。
- 语音输入/输出：Avalonia 可通过 `MediaElement` 或第三方库支持语音，但会增加复杂度。
- 多模态：未来可支持拍照识别单词（调用 OCR + LLM）。

---

## 五、总结建议

1. 先实现最小可行产品：支持 3~5 个最常用操作（查单词、加生词、开始学习），验证架构可行性。
2. 严格遵循 AOT 要求：避免任何运行时反射或动态生成。
3. 工具注册采用显式枚举：所有可调用的接口方法提前注册，不依赖自动发现。
4. 充分测试流式响应：Avalonia UI 需要正确处理异步流更新。
5. 文档化：在 `Doc/Spec/` 中增加 `Agent.typ` 说明设计和使用方式。

如果你需要，我可以进一步提供：
- 一个最小可运行的 `AgentService` 示例代码（兼容 AOT）。
- JSON Schema 源生成器的配置示例。
- Avalonia 聊天界面的 XAML + ViewModel 模板。

你觉得这个方向符合你的预期吗？
]
""";
}
