using SingularityGroup.HotReload.DTO;

namespace SingularityGroup.HotReload.Localization {

	internal static partial class Translations {
		public static class MenuItems {
			public const string UIControls = PackageConst.DefaultLocale == Locale.SimplifiedChinese ? "UI 控件" : "UI controls";
			public const string Information = PackageConst.DefaultLocale == Locale.SimplifiedChinese ? "信息" : "Information";
			public const string Other = PackageConst.DefaultLocale == Locale.SimplifiedChinese ? "其他" : "Other";
			public const string FalllbackEventSystem = PackageConst.DefaultLocale == Locale.SimplifiedChinese ? "当项目未能及早创建 EventSystem 时使用" : "Used when project does not create an EventSystem early enough";
			public const string BuildSettings = PackageConst.DefaultLocale == Locale.SimplifiedChinese ? "构建设置" : "Build Settings";
#if UNITY_6000_8_OR_NEWER
			// Development Build no longer defines DEVELOPMENT_BUILD, the managed code variant is what includes us
			public const string IncludeInBuildTooltip = PackageConst.DefaultLocale == Locale.SimplifiedChinese ? "Hot Reload 运行时是否应包含在托管代码变体不是 Release 的构建中？HotReload 永远不会包含在 Release 构建中。" : "Should the Hot Reload runtime be included in builds with a Managed Code Variant other than Release? HotReload is never included in Release builds.";
#else
			public const string IncludeInBuildTooltip = PackageConst.DefaultLocale == Locale.SimplifiedChinese ? "Hot Reload 运行时是否应包含在开发版本中？HotReload 永远不会包含在发布版本中。" : "Should the Hot Reload runtime be included in development builds? HotReload is never included in release builds.";
#endif
			public const string PlayerSettings = PackageConst.DefaultLocale == Locale.SimplifiedChinese ? "播放器设置" : "Player Settings";
		}
	}
}
