namespace Ngaq.Core.Frontend.ImgBg;


public enum EBackgroundMode{
	Off = 0,
	RandomInDirs,

}

public static class ExtnEBackgroundMode{
	extension(EBackgroundMode z){
		public static EBackgroundMode Parse(str EnumStr){
			if(str.IsNullOrEmpty(EnumStr)){
				return EBackgroundMode.Off;
			}
			return (EBackgroundMode)Enum.Parse(typeof(EBackgroundMode), EnumStr);
		}
	}
}
