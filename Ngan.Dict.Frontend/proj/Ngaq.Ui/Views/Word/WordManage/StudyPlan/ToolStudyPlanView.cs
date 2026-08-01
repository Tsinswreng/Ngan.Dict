namespace Ngaq.Ui.Views.Word.WordManage.StudyPlan;

using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data;
using Ngaq.Core.Infra;
using Ngaq.Ui.Tools;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsTempus;

/// StudyPlan 相关页面的通用视图拼装工具。

public static class ToolStudyPlanView{
	/// 统一格式化 UniqName：超长时保留头尾，中间用省略号替代。

	public static str FormatUniqName(str? UniqName, int HeadLen = 10, int TailLen = 6){
		return ToolText.FormatCompactText(UniqName, HeadLen, TailLen);
	}

	/// 统一格式化日期：按当前用户所在时区显示短格式 yy-MM-dd。

	public static str FormatDateShort(UnixMs Time, TimeZoneInfo? TimeZone = null){
		if(Time == UnixMs.Zero){
			return "-";
		}
		var tz = TimeZone ?? TimeZoneInfo.Local;
		var dto = DateTimeOffset.FromUnixTimeMilliseconds(Time.Value);
		var local = TimeZoneInfo.ConvertTime(dto, tz);
		return local.ToString("yy-MM-dd", CultureInfo.InvariantCulture);
	}

	/// 统一处理“更新时间优先，否则创建时间”的短日期显示。
	public static str FormatUpdatedDateShort(UnixMs UpdatedAt, UnixMs CreatedAt, TimeZoneInfo? TimeZone = null){
		var t = UpdatedAt == UnixMs.Zero ? CreatedAt : UpdatedAt;
		return FormatDateShort(t, TimeZone);
	}

	/// 创建“只读文本 + 选择按钮”的一行输入控件。
	/// <param name="Label">行标题文本。</param>
	/// <param name="Binding">文本框绑定。</param>
	/// <param name="BtnContent">按钮内容（可传图标+文本）。</param>
	/// <param name="OnBtnClick">按钮点击回调。</param>
	/// <param name="ReadOnly">文本框是否只读。</param>
	/// <returns>可直接加入布局树的控件。</returns>
	public static Control MkInputWithBtnRow(
		str Label, IBinding Binding, object BtnContent, Action OnBtnClick, bool ReadOnly = false
	){
		var sp = new StackPanel{Spacing = 3};
		sp.Children.Add(new TextBlock{Text = Label});
		var row = new GridStack(IsRow:false);
		row.Grid.SetColDefs([
			new(7, GUT.Star),
			new(2, GUT.Star),
		]);
		row.A(new TextBox(), o=>{
			o.IsReadOnly = ReadOnly;
			o.Bind(TextBox.TextProperty, Binding);
		})
		.A(new Button(), o=>{
			o.HorizontalContentAlignment = HAlign.Center;
			o.Content = BtnContent;
			o.Click += (s,e)=>OnBtnClick();
		});
		sp.Children.Add(row.Grid);
		return sp;
	}
}

