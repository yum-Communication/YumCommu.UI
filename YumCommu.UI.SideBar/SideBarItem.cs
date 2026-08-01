using Microsoft.UI.Xaml;

namespace YumCommu.UI;

/// <summary>
/// アイコンエリアに表示するアイコンと、対応するメニューパネルをペアで保持するアイテム。
/// <see cref="SideBar.Items"/> コレクションの要素として使用します。
/// </summary>
public sealed class SideBarItem : DependencyObject {

	/// <summary>アイコンエリアに表示するアイコン要素を取得または設定します。</summary>
	public UIElement? Icon {
		get => (UIElement?)GetValue(IconProperty);
		set => SetValue(IconProperty, value);
	}

	/// <summary><see cref="Icon"/> 依存関係プロパティを識別します。</summary>
	public static readonly DependencyProperty IconProperty;

	/// <summary>メニューエリアに表示するパネル要素を取得または設定します。</summary>
	public UIElement? Panel {
		get => (UIElement?)GetValue(PanelProperty);
		set => SetValue(PanelProperty, value);
	}

	/// <summary><see cref="Panel"/> 依存関係プロパティを識別します。</summary>
	public static readonly DependencyProperty PanelProperty;

	/// <summary>依存関係プロパティを静的に初期化します。</summary>
	static SideBarItem() {
		IconProperty = DependencyProperty.Register(nameof(Icon), typeof(UIElement), typeof(SideBarItem), new PropertyMetadata(null));
		PanelProperty = DependencyProperty.Register(nameof(Panel), typeof(UIElement), typeof(SideBarItem), new PropertyMetadata(null));
	}
}
