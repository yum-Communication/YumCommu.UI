using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;

namespace YumCommu.UI;

/// <summary>
/// サイドバーのアイコンエリアに表示される1つのアイコン項目を表すコントロール。
/// <see cref="SideBar"/> によって管理され、クリック・ダブルクリックイベントを通じて
/// メニューエリアの表示状態を制御する。
/// </summary>
public sealed partial class SideBarIcon: ContentControl {

	/// <summary>ホバー時などに表示するツールチップのテキストを取得または設定します。</summary>
	public string ToolTip {
		get => (string)GetValue(ToolTipProperty);
		set => SetValue(ToolTipProperty, value);
	}

	/// <summary><see cref="ToolTip"/> 依存関係プロパティを識別します。</summary>
	public static readonly DependencyProperty ToolTipProperty;

	/// <summary>このアイコンが選択状態かどうかを取得または設定します。</summary>
	/// <remarks>
	/// 選択状態は <see cref="SideBar"/> が一元管理します。
	/// 直接セットした場合も内部の <see cref="ToggleButton"/> に反映されます。
	/// </remarks>
	public bool IsSelected {
		get => (bool)GetValue(IsSelectedProperty);
		set => SetValue(IsSelectedProperty, value);
	}

	/// <summary><see cref="IsSelected"/> 依存関係プロパティを識別します。</summary>
	public static readonly DependencyProperty IsSelectedProperty;

	/// <summary>ユーザーがアイコンをシングルクリックしたときに発生します。</summary>
	internal event EventHandler? Clicked;

	/// <summary>ユーザーがアイコンをダブルクリックしたときに発生します。</summary>
	internal event EventHandler? DoubleClicked;

	/// <summary>
	/// 依存関係プロパティを静的に初期化します。
	/// </summary>
	static SideBarIcon() {
		ToolTipProperty = DependencyProperty.Register(nameof(ToolTip), typeof(string), typeof(SideBarIcon), new PropertyMetadata(""));
		IsSelectedProperty = DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(SideBarIcon), new PropertyMetadata(false, OnIsSelectedChanged));
	}

	/// <summary>
	/// <see cref="SideBarIcon"/> の新しいインスタンスを初期化します。
	/// </summary>
	public SideBarIcon() {
		DefaultStyleKey = typeof(SideBarIcon);
		AddHandler(DoubleTappedEvent, new DoubleTappedEventHandler(OnSelfDoubleTapped), handledEventsToo: true);
	}

	/// <summary>テンプレートから取得した内部の <see cref="ToggleButton"/> への参照。</summary>
	private ToggleButton? _toggleButton;

	/// <summary>
	/// コントロールテンプレートが適用されたときに呼び出されます。
	/// テンプレート内の <c>TabBtn</c> を取得してイベントをサブスクライブします。
	/// </summary>
	protected override void OnApplyTemplate() {
		base.OnApplyTemplate();

		_toggleButton?.Click -= OnToggleButtonClick;
		_toggleButton = GetTemplateChild("TabBtn") as ToggleButton;

		if (_toggleButton is not null) {
			_toggleButton.Click += OnToggleButtonClick;
			_toggleButton.IsChecked = IsSelected;
		}
	}

	/// <summary>
	/// 内部の <see cref="ToggleButton"/> がクリックされたときに呼び出されます。
	/// 選択状態は <see cref="SideBar"/> が管理するため、ボタンの <c>IsChecked</c> を元の値に戻してから
	/// <see cref="Clicked"/> イベントを発行します。
	/// </summary>
	/// <param name="sender">イベントの送信元。</param>
	/// <param name="e">イベントデータ。</param>
	private void OnToggleButtonClick(object sender, RoutedEventArgs e) {
		// SideBarがIsSelectedを管理するため、ToggleButtonの状態を元に戻す
		if (_toggleButton is not null)
			_toggleButton.IsChecked = IsSelected;
		Clicked?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// コントロール自体がダブルタップされたときに呼び出されます。
	/// <see cref="DoubleClicked"/> イベントを発行します。
	/// </summary>
	/// <param name="sender">イベントの送信元。</param>
	/// <param name="e">イベントデータ。</param>
	private void OnSelfDoubleTapped(object sender, DoubleTappedRoutedEventArgs e) {
		DoubleClicked?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// <see cref="IsSelected"/> 依存関係プロパティの値が変更されたときに呼び出されます。
	/// 内部の <see cref="ToggleButton"/> の <c>IsChecked</c> を新しい値と同期します。
	/// </summary>
	/// <param name="d">プロパティが変更された <see cref="SideBarIcon"/> インスタンス。</param>
	/// <param name="e">変更前後の値を含むイベントデータ。</param>
	private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		var icon = (SideBarIcon)d;
		if (icon._toggleButton is not null)
			icon._toggleButton.IsChecked = (bool)e.NewValue;
	}
}
