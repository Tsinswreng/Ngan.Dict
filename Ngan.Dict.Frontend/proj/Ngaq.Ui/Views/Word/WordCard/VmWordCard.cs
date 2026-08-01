namespace Ngaq.Ui.Views.Word.WordCard;
using System.Collections.ObjectModel;
using Ctx = VmWordListCard;
public partial class VmWordListCard
	:VmBaseWordListCard
{
	public new static ObservableCollection<Ctx> Samples = [];
	static VmWordListCard(){
		#if DEBUG

		#endif
	}

}
