namespace Ngaq.Ui.Views.Dictionary;

using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using Avalonia.Interactivity;
using Ngaq.Core.Shared.Word.Models.Dto;
using Ngaq.Ui.Icons;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.Ctrls;
using Ngaq.Ui.Tools;
using Ngaq.Ui.Views.Dictionary.LlmRawOutputEdit;
using Ngaq.Ui.Views.Dictionary.NormLangTag;
using Ngaq.Ui.Views.Dictionary.NormLangTagEdit;
using Ngaq.Ui.Views.Dictionary.SimpleWord;
using Ngaq.Ui.Views.Word.WordEditV2;
using Ngaq.Ui.Views.Word.WordManage.NormLang.NormLangPage;
using Ngaq.Ui.Views.Word.WordManage.NormLangToUserLang.NormLangToUserLangPage;
using Tsinswreng.AvlnTools;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsI18n;
using DictK = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;
using Ctx = VmDictionary;
using Ngaq.Core.Shared.Word.Models;
using Avalonia.Media;
using Avalonia.Layout;
using Tsinswreng.Avln.Grid;

/// 此文件只保存 ViewDictionary 的函數實現；公開聲明位於同名 .cs 文件。
public partial class ViewDictionary{
	public partial ViewDictionary(){
			Ctx = App.DiOrMk<Ctx>();
			if(Ctx is not null){
				// 把導航職責留在 View 層，VM 只產生要編輯的業務對象。
				Ctx.OnOpenWordEdit = OpenWordEditPage;
				Ctx.PropertyChanged += OnCtxPropertyChanged;
			}
			Style();
			Render();
			this.Loaded += async(s, e) => {
				//SearchTextBox?.Focus(); //在手機端會直接彈出輸入法、效果不佳
				if(Ctx is not null){
					await Ctx.InitLang(default);
					await Ctx.LoadSrcLangTags(default);
				}
			};
		}

	protected partial nil Style(){
			return NIL;
		}

	protected partial nil Render(){
			this.SetContent(Root.Grid, o=>{
				Root.RowDefs = new("Auto,Auto,Auto,*");
			});

			Root.A(MkSrcLangTagBar());

			var LangGrid = new GridStack(IsRow: false);
			_LangGrid = LangGrid;
			Root.A(LangGrid.Grid, o=>{
				LangGrid.ColDefs = new("*,Auto,*");
				o.ZIndex = 20;
			});
			{{
				LangGrid.A(MkLangButton(), o=>{
					Ctx.Bind(o, o=>o.Content, x=>x.SrcLangDisplay);
					o.Click += (s, e) => OpenNormLangSelector(true);
				})
				.A(MkLangSwapButton())
				.A(MkLangButton(), o=>{
					Ctx.Bind(o, o=>o.Content, x=>x.TgtLangDisplay);
					o.Click += (s, e) => OpenNormLangSelector(false);
				});
			}}

			var SearchGrid = new GridStack(IsRow: false);
			_searchGrid = SearchGrid;
			Root.A(SearchGrid.Grid, o=>{
				SearchGrid.ColDefs = new("1*,8*,1*,1*");
				o.ZIndex = 20;
			});
			{{
				SearchGrid
				.A(SearchBtn, o=>{
					InitToolbarBtn(
						o,
						Icons.Search().ToIcon(),
						Ct=>Ctx?.Lookup(Ct),
						UiCfg.Inst.MainColor
					);
				})
				.A(SearchTextBox, o=>{
					Ctx.Bind(o, o=>o.Text,x=>x.Input);
					o.Watermark = I[DictK.InputNewWordToSearch];
					o.KeyBindings.Add(
						new KeyBinding{
							Gesture = new(Key.Enter),
							Command = new RelayCommand(()=>SearchBtn.PerformClick())
						}
					);
				})
				.A(SaveToWordBtn, o=>{
					// 詞典頁主按鈕改為快速保存：
					// 1. 已查詞：直接保存到詞庫。
					// 2. 未查詞：仍直接進入自由加詞頁。
					ToolTip.SetTip(o, I[DictK.SaveToUserWordLibrary]);
					InitToolbarBtn(
						o,
						Icons.BookmarkOutlineAdd().ToIcon(),
						QuickSaveOrFreeAdd
					);
				})
				.A(MenuBtn, o=>{
					// 與搜尋/收藏按鈕統一使用 OpBtn 風格。
					InitToolbarBtn(o, Icons.Menu().ToIcon());
					o._Button.Click += (s,e)=>OpenTitleMenuNear(o._Button);
				});
			}}

			Root.A(MkResultArea());

			return NIL;
		}

	private partial Control MkSrcLangTagBar(){
		var Bar = new GridStack(IsRow: false);
		_SrcLangTagBar = Bar;
		Bar.SetColDefs([new(1, GUT.Star), new(1, GUT.Auto)]);
		Bar.A(new ScrollViewer(), O=>{
			SrcLangTagScroll = O;
			O.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
			O.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
			O.SetContent(SrcLangTagList);
		})
		.A(SrcLangTagEditBtn, O=>{
			O.BtnContent = Icons.Edit().ToIcon();
			O._Button.StretchCenter();
			O.SetExe(Ct=>{
				OpenSrcLangTagEditor();
				return Task.FromResult<nil>(NIL);
			});
		});
		RebuildSrcLangTags();
		return Bar.Grid;
	}

	private partial Control MkSrcLangTag(VmNormLangTag Tag){
		var View = new ViewNormLangTag{Ctx = Tag};
		View.OnSelected += X=>Ctx?.ApplySrcNormLangTag(X);
		return View;
	}

	private partial void RebuildSrcLangTags(){
		SrcLangTagList = new StackPanel{
			Orientation = Orientation.Horizontal,
		};
		SrcLangTagScroll?.SetContent(SrcLangTagList);
		if(Ctx is null){
			return;
		}
		foreach(var Tag in Ctx.SrcLangTags){
			SrcLangTagList.A(MkSrcLangTag(Tag));
		}
	}

	private partial void OpenSrcLangTagEditor(){
		var View = new ViewNormLangTagEdit();
		if(View.Ctx is not null && Ctx is not null){
			View.Ctx.FromTags(Ctx.SrcLangTags);
			View.Ctx.SetOnSaved(Tags=>{
				_ = Ctx.ReplaceSrcLangTags(Tags);
			});
		}
		ViewNavi?.GoTo(ToolView.WithTitle(Todo.I18n("Edit source language shortcuts"), View));
	}

	private partial Button MkLangButton(){
			var R = new Button();
			R.HorizontalContentAlignment = HAlign.Center;
			R.VerticalContentAlignment = VAlign.Center;
			R.MinHeight = 24;
			R.Padding = new Avalonia.Thickness(0);
			return R;
		}

	private partial Button MkLangSwapButton(){
			var R = new Button();
			R.Content = "⇄";
			R.VerticalAlignment = VAlign.Center;
			R.HorizontalAlignment = HAlign.Center;
			R.MinHeight = 24;
			R.Padding = new Avalonia.Thickness(0);
			R.Click += (s, e) => {
				Ctx?.SwapLang();
			};
			return R;
		}

	private partial nil InitToolbarBtn(OpBtn Btn, Control Content, Func<CT, Task<nil>>? Exe, IBrush? Background){
			Btn._Button.StretchCenter();
			Btn.BtnContent = Content;
			if(Background is not null){
				Btn._Button.Background = Background;
			}
			if(Exe is not null){
				Btn.SetExe(Exe);
			}
			return NIL;
		}

	private partial str GuideText(){
			return I[DictK.DictionaryUsageGuide_];
		}

	private async partial Task<nil> QuickSaveOrFreeAdd(CT Ct){
			if(Ctx is null){
				return NIL;
			}
			if(Ctx.HasLookupStarted){
				if(!Ctx.CanQuickSaveCurrentLookup || Ctx.LastReqLlmDict is null || Ctx.LastRespLlmDict is null){
					Ctx.ShowToast(I[DictK.CompleteDictionaryQueryBeforeSave]);
					return NIL;
				}
				if(Ctx.HasQuickSavedCurrentLookup){
					return NIL;
				}
				var ok = await Ctx.QuickSaveToWord(Ct);
				if(ok){
					MarkQuickSaveBtnAsSaved();
				}
				return NIL;
			}
			return OpenFreeAddWordPage();
		}

	private async partial Task<nil> OpenSaveViaWordEditOrFreeAdd(CT Ct){
			if(Ctx?.LastReqLlmDict is not null && Ctx.LastRespLlmDict is not null){
				return await Ctx.ToWordEdit(Ct);
			}
			return OpenFreeAddWordPage();
		}

	private partial nil MarkQuickSaveBtnAsSaved(){
			SaveToWordBtn._Button.Background = Brushes.Green;
			return NIL;
		}

	private partial nil ResetQuickSaveBtnToDefaultStyle(){
			SaveToWordBtn._Button.ClearValue(Button.BackgroundProperty);
			SaveToWordBtn._Button.ClearValue(Button.BorderBrushProperty);
			return NIL;
		}

	private partial void OnCtxPropertyChanged(object? Sender, PropertyChangedEventArgs E){
			if(E.PropertyName == nameof(Ctx.SrcLangTags)){
				RebuildSrcLangTags();
				return;
			}
			if(E.PropertyName != nameof(Ctx.HasQuickSavedCurrentLookup)){
				return;
			}
			if(Ctx?.HasQuickSavedCurrentLookup == true){
				MarkQuickSaveBtnAsSaved();
				return;
			}
			ResetQuickSaveBtnToDefaultStyle();
		}

	private partial Control MkResultArea(){
			var Area = new Grid();
			_resultArea = Area;
			Area.ZIndex = 0;
			Area.ClipToBounds = true;
			// 結果區與上方工具欄留出固定縫隙，避免邊界像素落到 ScrollViewer 命中區。
			Area.Margin = new Avalonia.Thickness(0, UiCfg.Inst.BaseFontSize * 0.35, 0, 0);
			Area.A(new ScrollViewer(), o=>{
				Ctx.Bind(o, IsVisibleProperty, x=>x.ShowLookupResult, Mode: BindingMode.OneWay);
				o.SetContent(new ViewSimpleWord(), o=>{
					Ctx.Bind(o,o.PropDataContext,x=>x.Result);
				});
			})
			.A(new TextBlock(), o=>{
				o.Foreground = Brushes.LightGray;
				o.TextWrapping = TextWrapping.Wrap;
				o.VerticalAlignment = VerticalAlignment.Top;
				o.Margin = new Avalonia.Thickness(0, UiCfg.Inst.BaseFontSize * 0.5, 0, 0);
				Ctx.Bind(o, IsVisibleProperty, x=>x.ShowUsageGuide, Mode: BindingMode.OneWay);
			})
			;
			return Area;
		}

	private partial void OpenNormLangSelector(bool IsSrc){
			var view = new ViewNormLangPage();
			if(view.Ctx is not null){
				if(IsSrc){
					view.Ctx.Input = Ctx?.SrcLang ?? "";
				}else{
					view.Ctx.Input = Ctx?.TgtLang ?? "";
				}
			}
			if(IsSrc){
				view.Ctx?.SetSelectMode(po=>{
					Ctx?.ApplySrcNormLang(po);
					view.ViewNavi?.Back();
				});
			}else{
				view.Ctx?.SetSelectMode(po=>{
					Ctx?.ApplyTgtNormLang(po);
					view.ViewNavi?.Back();
				});
			}
			ViewNavi?.GoTo(ToolView.WithTitle(
				IsSrc ? I[DictK.SelectDictionarySrcLang] : I[DictK.SelectDictionaryTgtLang],
				view
			));
		}

	public partial Control MkTitleMenu(){
			var menu = new ContextMenu();
			var item = menu.Items;
			item
			.A(new MenuItem(), o=>{
				o.Header = Todo.I18n("Edit & Save");
				o.Click += async(s,e)=>{
					await OpenSaveViaWordEditOrFreeAdd(default);
				};
			})
			.A(new MenuItem(), o=>{
				o.Header = I[DictK.Help];
				o.Click += (s,e)=>{
					ViewNavi?.GoTo(
						ToolView.WithTitle(
							I[DictK.Help],
							new TextBox{
								Classes = {App.Cls.RoTextBox},
								Text= GuideText(),
							}
						)
					);
				};
			})
			.A(new MenuItem(), o=>{
				o.Header = I[DictK.NormLang];
				o.Click += (s,e)=>{
					var view = new ViewNormLangPage();
					ViewNavi?.GoTo(ToolView.WithTitle(I[DictK.NormLangManage], view));
				};
			}).A(new MenuItem(), o=>{
				o.Header = I[DictK.ConfigureLangMapping];
				o.Click += (s,e)=>{
					var view = new ViewNormLangToUserLangPage();
					ViewNavi?.GoTo(ToolView.WithTitle(I[DictK.ConfigureLangMapping], view));
				};
			}).A(new MenuItem(), o=>{
				o.Header = I[DictK.ViewLlmRawOutput];
				o.Click += (s,e)=>{
					var DictCtx = Ctx;
					if(DictCtx is null){
						return;
					}
					var view = new ViewLlmRawOutputEdit();
					if(view.Ctx is not null){
						view.Ctx.SetRawOutput(DictCtx.LastLlmRawOutput);
						view.Ctx.SetOnConfirm((raw, ct)=>{
							return DictCtx.ReparseFromRawOutput(raw, ct);
						});
					}
					ViewNavi?.GoTo(ToolView.WithTitle(I[DictK.ViewLlmRawOutput], view));
				};
			});
			return menu;
		}

	private partial void OpenTitleMenuNear(Control? Anchor){
			if(Anchor is null){
				return;
			}
			var menu = MkTitleMenu() as ContextMenu;
			if(menu is null){
				return;
			}
			// 顯式傳入錨點控件，避免 ContextMenu 內部取到空 PlacementTarget。
			menu.Open(Anchor);
		}

	private partial nil OpenWordEditPage(JnWord JnWord){
			var view = new ViewWordEditV2();
			if(view.Ctx is null){
				Ctx?.ShowDialog(I[DictK.WordEditorContextIsNull]);
				return NIL;
			}
			view.Ctx.FromJnWord(JnWord);
			view.Ctx.SaveMode = VmWordEditV2.ESaveMode.Merge;
			var title = str.IsNullOrWhiteSpace(JnWord.Word.Head)
				? I[DictK.WordEdit]
				: JnWord.Word.Head
			;
			ViewNavi?.GoTo(ToolView.WithTitle(title, view));
			return NIL;
		}

	private partial nil OpenFreeAddWordPage(){
			var view = new ViewWordEditV2();
			if(view.Ctx is null){
				Ctx?.ShowDialog(I[DictK.WordEditorContextIsNull]);
				return NIL;
			}
			view.Ctx.InitFreeAddDraft(Ctx?.SrcLang ?? "");
			ViewNavi?.GoTo(ToolView.WithTitle(I[DictK.AddWords], view));
			return NIL;
		}

	public partial void ClickLookupBtn(str? SearchText){
			if(SearchText != null){
				SearchTextBox.Text = SearchText;
			}
			SearchBtn.PerformClick();
		}
}
