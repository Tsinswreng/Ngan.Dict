using System.Diagnostics.CodeAnalysis;

namespace Ngaq.Ui;
public partial class GlobalTools{
	// public static bool AnyNull(params object?[]? objs){
	// 	if(objs is null){
	// 		return true;
	// 	}
	// 	return objs.Any(o => o is null);
	// }

	public static bool AnyNull(
		[NotNullWhen(false)] obj? a0
	){
		if(a0 is null){
			return true;
		}
		return false;
	}


	public static bool AnyNull(
		[NotNullWhen(false)] obj? a0
		,[NotNullWhen(false)] obj? a1
	){
		if(
			a0 is null
			||a1 is null
		){
			return true;
		}
		return false;
	}

	public static bool AnyNull(
		[NotNullWhen(false)] obj? a0
		,[NotNullWhen(false)] obj? a1
		,[NotNullWhen(false)] obj? a2
	){
		if(
			a0 is null
			||a1 is null
			||a2 is null
		){
			return true;
		}
		return false;
	}

	public static bool AnyNull(
		[NotNullWhen(false)] obj? a0
		,[NotNullWhen(false)] obj? a1
		,[NotNullWhen(false)] obj? a2
		,[NotNullWhen(false)] obj? a3
	){
		if(
			a0 is null
			||a1 is null
			||a2 is null
			||a3 is null
		){
			return true;
		}
		return false;
	}

	public static bool AnyNull(
		[NotNullWhen(false)] obj? a0
		,[NotNullWhen(false)] obj? a1
		,[NotNullWhen(false)] obj? a2
		,[NotNullWhen(false)] obj? a3
		,[NotNullWhen(false)] obj? a4
	){
		if(
			a0 is null
			||a1 is null
			||a2 is null
			||a3 is null
			||a4 is null
		){
			return true;
		}
		return false;
	}

	public static bool AnyNull(
		[NotNullWhen(false)] obj? a0
		,[NotNullWhen(false)] obj? a1
		,[NotNullWhen(false)] obj? a2
		,[NotNullWhen(false)] obj? a3
		,[NotNullWhen(false)] obj? a4
		,[NotNullWhen(false)] obj? a5
	){
		if(
			a0 is null
			||a1 is null
			||a2 is null
			||a3 is null
			||a4 is null
			||a5 is null
		){
			return true;
		}
		return false;
	}

	public static bool AnyNull(
		[NotNullWhen(false)] obj? a0
		,[NotNullWhen(false)] obj? a1
		,[NotNullWhen(false)] obj? a2
		,[NotNullWhen(false)] obj? a3
		,[NotNullWhen(false)] obj? a4
		,[NotNullWhen(false)] obj? a5
		,[NotNullWhen(false)] obj? a6
	){
		if(
			a0 is null
			||a1 is null
			||a2 is null
			||a3 is null
			||a4 is null
			||a5 is null
			||a6 is null
		){
			return true;
		}
		return false;
	}

	public static bool AnyNull(
		[NotNullWhen(false)] obj? a0
		,[NotNullWhen(false)] obj? a1
		,[NotNullWhen(false)] obj? a2
		,[NotNullWhen(false)] obj? a3
		,[NotNullWhen(false)] obj? a4
		,[NotNullWhen(false)] obj? a5
		,[NotNullWhen(false)] obj? a6
		,[NotNullWhen(false)] obj? a7
	){
		if(
			a0 is null
			||a1 is null
			||a2 is null
			||a3 is null
			||a4 is null
			||a5 is null
			||a6 is null
			||a7 is null
		){
			return true;
		}
		return false;
	}

}
