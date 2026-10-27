using Microsoft.Extensions.DependencyInjection;
using Ngan.Dict.Core.Frontend.Clipboard;
using Ngan.Dict.Ui.Infra;
using Ngan.Dict.Ui.Infra.I18n;
using Ngan.Dict.Ui.Views.Dictionary;
using Ngan.Dict.Ui.Views.Settings.LearnWord;
using Ngan.Dict.Ui.Views.User;
using Ngan.Dict.Ui.Views.User.AboutMe;
using Ngan.Dict.Ui.Views.Word.Learn;
using Ngan.Dict.Ui.Views.Word.Pronunciation;
using Ngan.Dict.Ui.Views.Word.WordManage.AddWord;
using Ngan.Dict.Ui.Views.Word.WordManage.EditWord;
using Ngan.Dict.Ui.Views.Word.WordManage.SearchWords;
using Ngan.Dict.Ui.Views.Word.WordManage.Statistics;
using Ngan.Dict.Ui.Views.Word.WordManage.WordSync;
using Ngan.Dict.Ui.Views.Word.WordManage.WordSyncV2;
using Ngan.Dict.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit;
using Ngan.Dict.Ui.Views.Word.WordManage.StudyPlan.PreFilterPage;
using Ngan.Dict.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit.PreFilterVisualEdit;
using Ngan.Dict.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit.PreFilterVisualEditV2;
using Ngan.Dict.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit.FilterCardEditV2;
using Ngan.Dict.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit.PreFilterJsonEdit;
using Ngan.Dict.Ui.Views.Word.WordManage.StudyPlan.StudyPlanPage;
using Ngan.Dict.Ui.Views.Word.WordManage.StudyPlan.StudyPlanEdit;
using Ngan.Dict.Ui.Views.Word.WordManage.StudyPlan.SetCurStudyPlan;
using Ngan.Dict.Ui.Views.Word.WordManage.StudyPlan.WeightArgPage;
using Ngan.Dict.Ui.Views.Word.WordManage.StudyPlan.WeightArgEdit;
using Ngan.Dict.Ui.Views.Word.WordManage.StudyPlan.WeightCalculatorPage;
using Ngan.Dict.Ui.Views.Word.WordManage.StudyPlan.WeightCalculatorEdit;
using Ngan.Dict.Ui.Views.Word.WordManage.NormLang.NormLangPage;
using Ngan.Dict.Ui.Views.Word.WordManage.NormLang.NormLangEdit;
using Ngan.Dict.Ui.Views.Word.WordManage.UserLang.UserLangPage;
using Ngan.Dict.Ui.Views.Word.WordManage.UserLang.UserLangEdit;
using Ngan.Dict.Ui.Views.Word.WordManage.NormLangToUserLang.NormLangToUserLangPage;
using Ngan.Dict.Ui.Views.Word.WordManage.NormLangToUserLang.NormLangToUserLangEdit;
using Ngan.Dict.Ui.Views.Dictionary.SimpleWord;
using Ngan.Dict.Ui.Views.Word.WordCard;
using Ngan.Dict.Ui.Views.Word.WordEditV2;
using Ngan.Dict.Ui.Views.Word.WordEditV2.WordPropEdit;
using Ngan.Dict.Ui.Views.Word.WordEditV2.WordLearnEdit;
using System.IO.Abstractions;
using Ngan.Dict.Ui.Views.Settings.Lang;
using Ngan.Dict.Ui.Views.Settings.LlmDictionary;
using Ngan.Dict.Ui.Views.User.Profile;
using Ngan.Dict.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit.PreFilterPayloadJsonEdit;

namespace Ngan.Dict.Ui;

public static class DiUi{
	public static IServiceCollection SetupUi(
		this IServiceCollection z
	){
		z.AddTransient<VmAddWord>();
		z.AddTransient<VmLearnWords>();
		z.AddTransient<VmLoginRegister>();
		z.AddTransient<VmSearchWords>();
		z.AddTransient<VmEditJsonWord>();
		z.AddTransient<VmAboutMe>();
		z.AddTransient<VmWordSync>();
		z.AddTransient<VmWordSyncV2>();
		z.AddTransient<VmStatistics>();
		z.AddTransient<VmCfgLearnWord>();
		z.AddTransient<VmCfgLlmDictionary>();
		z.AddTransient<VmPronunciation>();
		z.AddTransient<VmDictionary>();
		z.AddTransient<VmPreFilterVisualEdit>();
		z.AddTransient<VmPreFilterVisualEditV2>();
		z.AddTransient<VmFilterCardEditV2>();
		z.AddTransient<VmPreFilterJsonEdit>();
		z.AddTransient<VmPreFilterPayloadJsonEdit>();
		z.AddTransient<VmPreFilterPage>();
		z.AddTransient<VmStudyPlanPage>();
		z.AddTransient<VmStudyPlanEdit>();
		z.AddTransient<VmSetCurStudyPlan>();
		z.AddTransient<VmWeightArgPage>();
		z.AddTransient<VmWeightArgEdit>();
		z.AddTransient<VmWeightCalculatorPage>();
		z.AddTransient<VmWeightCalculatorEdit>();
		z.AddTransient<VmNormLangPage>();
		z.AddTransient<VmNormLangEdit>();
		z.AddTransient<VmUserLangPage>();
		z.AddTransient<VmUserLangEdit>();
		z.AddTransient<VmNormLangToUserLangPage>();
		z.AddTransient<VmNormLangToUserLangEdit>();
		z.AddTransient<VmSimpleWord>();
		z.AddTransient<VmWordEditV2>();
		z.AddTransient<VmWordPropEdit>();
		z.AddTransient<VmWordLearnEdit>();
		z.AddTransient<VmCfgLang>();
		z.AddTransient<VmUserProfile>();
		z.AddTransient<IWordCardPronounceBiz, SvcWordCardPronounceBiz>();
		// 使用 Avalonia 跨平臺能力實現剪貼板服務，供各平臺共用。
		z.AddSingleton<ISvcClipboard, SvcClipboard>();
		// 熱鍵字典入口：重用頁面並避免重複壓棧。
		z.AddSingleton<ISvcDictionaryHotkeyNavigator, SvcDictionaryHotkeyNavigator>();
		z.AddSingleton<IHotkeyDictionaryLookupAction, HotkeyDictionaryLookupAction>();
		// 平臺層共用：統一解析「查詞熱鍵」配置，避免重複解析邏輯。
		z.AddSingleton<IParseDictionaryLookupHotkeyCfg, DictionaryLookupHotkeyCfgParser>();
		z.AddSingleton<I_GetViewNavi>(MgrViewNavi.Inst);
		return z;
	}
}


