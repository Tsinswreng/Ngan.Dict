using Microsoft.Extensions.DependencyInjection;
using Ngaq.Core.Frontend.Clipboard;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.I18n;
using Ngaq.Ui.Views.Dictionary;
using Ngaq.Ui.Views.Settings.LearnWord;
using Ngaq.Ui.Views.User;
using Ngaq.Ui.Views.User.AboutMe;
using Ngaq.Ui.Views.Word.Learn;
using Ngaq.Ui.Views.Word.Pronunciation;
using Ngaq.Ui.Views.Word.WordManage.AddWord;
using Ngaq.Ui.Views.Word.WordManage.EditWord;
using Ngaq.Ui.Views.Word.WordManage.SearchWords;
using Ngaq.Ui.Views.Word.WordManage.Statistics;
using Ngaq.Ui.Views.Word.WordManage.WordSync;
using Ngaq.Ui.Views.Word.WordManage.WordSyncV2;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.PreFilterPage;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit.PreFilterVisualEdit;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit.PreFilterVisualEditV2;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit.FilterCardEditV2;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit.PreFilterJsonEdit;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.StudyPlanPage;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.StudyPlanEdit;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.SetCurStudyPlan;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.WeightArgPage;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.WeightArgEdit;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.WeightCalculatorPage;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.WeightCalculatorEdit;
using Ngaq.Ui.Views.Word.WordManage.NormLang.NormLangPage;
using Ngaq.Ui.Views.Word.WordManage.NormLang.NormLangEdit;
using Ngaq.Ui.Views.Word.WordManage.UserLang.UserLangPage;
using Ngaq.Ui.Views.Word.WordManage.UserLang.UserLangEdit;
using Ngaq.Ui.Views.Word.WordManage.NormLangToUserLang.NormLangToUserLangPage;
using Ngaq.Ui.Views.Word.WordManage.NormLangToUserLang.NormLangToUserLangEdit;
using Ngaq.Ui.Views.Dictionary.SimpleWord;
using Ngaq.Ui.Views.Word.WordCard;
using Ngaq.Ui.Views.Word.WordEditV2;
using Ngaq.Ui.Views.Word.WordEditV2.WordPropEdit;
using Ngaq.Ui.Views.Word.WordEditV2.WordLearnEdit;
using System.IO.Abstractions;
using Ngaq.Ui.Views.Settings.Lang;
using Ngaq.Ui.Views.Settings.LlmDictionary;
using Ngaq.Ui.Views.User.Profile;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit.PreFilterPayloadJsonEdit;

namespace Ngaq.Ui;

public static class DiUi{
	public static IServiceCollection SetupUi(
		this IServiceCollection z
	){

		//z.AddSingleton<II18n>(I18n.Inst);

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


