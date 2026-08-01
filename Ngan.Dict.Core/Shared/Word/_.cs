using Ngaq.Core.Service.Parser;
using Ngaq.Core.Shared.Word;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Learn;
using Ngaq.Core.Shared.Word.Models.Po.UserLang;
using Ngaq.Core.Shared.Word.Models.Po.Word;

file class DirDoc{
	str Doc =
$$"""
#Sum[

]
#Descr[
单词学习系统代码（Ngaq.Core/Shared/Word模块），包含单词模型定义、学习记录管理、权重计算、文本解析、服务接口等

单词聚合根（{{nameof(JnWord)}}）：作为单词的核心聚合根，实现了深克隆、ID重置、按时间/ID对比差异、同步等核心能力。
- `{{nameof(PoWord)}}`（单词基础信息）
- `{{nameof(PoWordProp)}}`（单词属性，如释义、標籤、鍵值對形式）
- `{{nameof(PoWordLearn)}}`（学习记录），
{{nameof(PoUserLang)}} 用戶自定義的語言 與 標準語言標識 的映射

- 学习用单词（WordForLearn）：对`{{nameof(JnWord)}}`的封装，适配学习场景，包含权重（Weight）、索引（Index）、已保存/未保存学习记录、上轮学习记录等字段

- 学习记录（LearnRecord/ILearnRecord）：定义学习行为（Add/Rmb/Fgt：添加/记住/忘记）和时间戳，支持与数据库实体（PoWordLearn）的互相转换。

3. 单词文本DSL解析模块
- {{nameof(WordListParser)}} ：解析特定格式的单词表文本

4. 学习状态管理模块
- {{nameof(MgrLearn)}} ：学习流程的核心管理器，负责：
  - 加载单词并计算权重
  - 学习/撤销学习操作
  - 保存学习记录到服务（Save）；
  - 管理学习状态（StateLearnWords）：待学习单词列表、已学习单词管理器、操作状态（加载/启动/保存）。
- MgrLearnedWords：管理已学习单词的分组（按学习类型ELearn），提供添加/删除/查询已学习单词的能力。

	5. 服务接口层
	- ISvcWord(基于舊架構 不再維護)：单词核心服务接口，定义单词的增删改查、批量添加、同步、压缩打包/解包等能力。
	- ISvcWordV2：
		- `SoftDelPoWordInId` 仅软删聚合根，不改资产删除状态；
		- `HardDelSoftDeleted` 用于清理回收站，把已软删的根词与其相关资产真正从数据库中删除；
		- `SoftDelJnWordInId` 仍保留旧语义，表示根词与附属资产一起软删。
		- `MergeWord` 遇到已软删的同 BizId 词时，会先恢复聚合根再合并新资产。
	- ISvcParseWordList：单词表解析服务接口，支持从文件、URL、文本流解析单词。

同Owner下 (Head, Lang) 纔是一詞ʹ 理則ʸʹ 唯一標識、洏非Id
(如異ʹ節點蜮在同步前皆各新增一詞芝有同ʹ(Head,Lang)、則雖同ʹ詞、猶將被予異ʹId)

]
""";
}
