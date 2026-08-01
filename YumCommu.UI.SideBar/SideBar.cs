using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace YumCommu.UI;

/// <summary>メニューエリアの表示状態を表します。</summary>
public enum SideBarMenuMode {
	/// <summary>メニューエリアを非表示にします。</summary>
	Hidden,
	/// <summary>メニューエリアをコンテンツの上にオーバーレイ表示します。</summary>
	Overlay,
	/// <summary>メニューエリアをコンテンツの隣にドッキング表示します。</summary>
	Docked
}

/// <summary>
/// アイコンエリア・メニューエリア・メインコンテンツエリア・プロパティエリアの
/// 4 領域で構成されるサイドバーコントロール。
/// </summary>
public sealed partial class SideBar: Control {

	/// <summary>
	/// アイコンとメニューパネルのペアを保持するアイテムのコレクション
	/// </summary>
	public ObservableCollection<SideBarItem> Items { get; } = [];

	/// <summary>
	/// メインコンテンツエリアに表示する要素
	/// </summary>
	public UIElement? Content {
		get => (UIElement?)GetValue(ContentProperty);
		set => SetValue(ContentProperty, value);
	}

	/// <summary><see cref="Content"/> 依存関係プロパティを識別します。</summary>
	public static readonly DependencyProperty ContentProperty;

	/// <summary>
	/// プロパティエリアに表示する要素
	/// </summary>
	public UIElement? PropertyPanel {
		get => (UIElement?)GetValue(PropertyPanelProperty);
		set => SetValue(PropertyPanelProperty, value);
	}

	/// <summary><see cref="PropertyPanel"/> 依存関係プロパティを識別します。</summary>
	public static readonly DependencyProperty PropertyPanelProperty;

	/// <summary>
	/// メニューエリアの表示状態
	/// </summary>
	public SideBarMenuMode MenuMode {
		get => (SideBarMenuMode)GetValue(MenuModeProperty);
		set => SetValue(MenuModeProperty, value);
	}

	/// <summary><see cref="MenuMode"/> 依存関係プロパティを識別します。</summary>
	public static readonly DependencyProperty MenuModeProperty;

	/// <summary>
	/// 選択中のアイコンインデックス (-1 は未選択)
	/// </summary>
	public int SelectedIndex {
		get => (int)GetValue(SelectedIndexProperty);
		set => SetValue(SelectedIndexProperty, value);
	}

	/// <summary><see cref="SelectedIndex"/> 依存関係プロパティを識別します。</summary>
	public static readonly DependencyProperty SelectedIndexProperty;

	/// <summary>
	/// メニューエリアをユーザーがドラッグで幅変更できるかどうか
	/// </summary>
	public bool IsMenuResizable {
		get => (bool)GetValue(IsMenuResizableProperty);
		set => SetValue(IsMenuResizableProperty, value);
	}

	/// <summary><see cref="IsMenuResizable"/> 依存関係プロパティを識別します。</summary>
	public static readonly DependencyProperty IsMenuResizableProperty;

	/// <summary>
	/// プロパティエリアの幅 (ピクセル)。<see cref="double.NaN"/> を指定すると Auto になります。
	/// </summary>
	public double PropertyWidth {
		get => (double)GetValue(PropertyWidthProperty);
		set => SetValue(PropertyWidthProperty, value);
	}

	/// <summary><see cref="PropertyWidth"/> 依存関係プロパティを識別します。</summary>
	public static readonly DependencyProperty PropertyWidthProperty;

	/// <summary>
	/// プロパティエリアの表示状態
	/// </summary>
	public Visibility PropertyVisibility {
		get => (Visibility)GetValue(PropertyVisibilityProperty);
		set => SetValue(PropertyVisibilityProperty, value);
	}

	/// <summary><see cref="PropertyVisibility"/> 依存関係プロパティを識別します。</summary>
	public static readonly DependencyProperty PropertyVisibilityProperty;

	/// <summary>依存関係プロパティを静的に初期化します。</summary>
	static SideBar() {
		ContentProperty = DependencyProperty.Register(nameof(Content), typeof(UIElement), typeof(SideBar), new PropertyMetadata(null));
		PropertyPanelProperty = DependencyProperty.Register(nameof(PropertyPanel), typeof(UIElement), typeof(SideBar), new PropertyMetadata(null));
		MenuModeProperty = DependencyProperty.Register(nameof(MenuMode), typeof(SideBarMenuMode), typeof(SideBar), new PropertyMetadata(SideBarMenuMode.Hidden, OnMenuModeChanged));
		SelectedIndexProperty = DependencyProperty.Register(nameof(SelectedIndex), typeof(int), typeof(SideBar), new PropertyMetadata(-1, OnSelectedIndexChanged));
		IsMenuResizableProperty = DependencyProperty.Register(nameof(IsMenuResizable), typeof(bool), typeof(SideBar), new PropertyMetadata(false, OnIsMenuResizableChanged));
		PropertyWidthProperty = DependencyProperty.Register(nameof(PropertyWidth), typeof(double), typeof(SideBar), new PropertyMetadata(double.NaN, OnPropertyWidthChanged));
		PropertyVisibilityProperty = DependencyProperty.Register(nameof(PropertyVisibility), typeof(Visibility), typeof(SideBar), new PropertyMetadata(Visibility.Visible, OnPropertyVisibilityChanged));
	}

	/// <summary><see cref="SideBar"/> の新しいインスタンスを初期化します。</summary>
	public SideBar() {
		DefaultStyleKey = typeof(SideBar);
		Items.CollectionChanged += OnItemsChanged;
	}

	// テンプレートパーツ
	private StackPanel? _iconPanel;
	private Border? _menuPanel;
	private ContentPresenter? _menuContent;
	private Grid? _overlayBackdrop;
	private ColumnDefinition? _menuColumn;
	private Grid? _menuResizer;
	private ColumnDefinition? _propertyColumn;
	private ContentPresenter? _propertyContent;

	// リサイズ状態
	private double _menuPanelWidth = 280;
	private bool _isResizing;
	private double _resizingStartX;
	private double _resizingStartWidth;

	/// <summary>
	/// コントロールテンプレートが適用されたときに呼び出されます。
	/// テンプレートパーツを取得し、アイコンパネルの初期構築とメニュー状態の反映を行います。
	/// </summary>
	protected override void OnApplyTemplate() {
		base.OnApplyTemplate();

		_overlayBackdrop?.Tapped -= OnOverlayBackdropTapped;
		UnsubscribeResizerEvents();

		_iconPanel = GetTemplateChild("PART_IconPanel") as StackPanel;
		_menuPanel = GetTemplateChild("PART_MenuPanel") as Border;
		_menuContent = GetTemplateChild("PART_MenuContent") as ContentPresenter;
		_overlayBackdrop = GetTemplateChild("PART_OverlayBackdrop") as Grid;
		_menuColumn = GetTemplateChild("PART_MenuColumn") as ColumnDefinition;
		_menuResizer = GetTemplateChild("PART_MenuResizer") as Grid;
		_propertyColumn = GetTemplateChild("PART_PropertyColumn") as ColumnDefinition;
		_propertyContent = GetTemplateChild("PART_PropertyContent") as ContentPresenter;

		_overlayBackdrop?.Tapped += OnOverlayBackdropTapped;
		SubscribeResizerEvents();

		PopulateIconPanel();
		UpdateMenuMode();
		UpdateResizerVisibility();
		UpdateSelectedPanel();
		UpdateIconSelection();
		UpdatePropertyColumn();
	}

	/// <summary>
	/// <see cref="Items"/> コレクションが変更されたときに呼び出されます。
	/// アイコンパネルへの追加・削除を反映し、イベントハンドラーのサブスクライブ状態を同期します。
	/// </summary>
	/// <param name="sender">イベントの送信元。</param>
	/// <param name="e">コレクション変更の詳細。</param>
	private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) {
		if (_iconPanel is null)
			return;

		if (e.Action == NotifyCollectionChangedAction.Reset) {
			foreach (UIElement child in _iconPanel.Children) {
				if (child is SideBarIcon sbi) {
					sbi.Clicked -= OnIconClicked;
					sbi.DoubleClicked -= OnIconDoubleClicked;
				}
			}
			_iconPanel.Children.Clear();
			foreach (var item in Items)
				AddItemToPanel(item);
		} else {
			if (e.OldItems is not null)
				foreach (SideBarItem item in e.OldItems)
					RemoveItemFromPanel(item);

			if (e.NewItems is not null) {
				int insertAt = e.NewStartingIndex;
				foreach (SideBarItem item in e.NewItems)
					AddItemToPanel(item, insertAt++);
			}
		}
		UpdateSelectedPanel();
	}

	/// <summary>
	/// <see cref="_iconPanel"/> に <see cref="Items"/> の全要素のアイコンを追加します。
	/// テンプレート適用直後の初期化で使用します。
	/// </summary>
	private void PopulateIconPanel() {
		if (_iconPanel is null)
			return;
		_iconPanel.Children.Clear();
		foreach (var item in Items)
			AddItemToPanel(item);
	}

	/// <summary>
	/// 指定した <see cref="SideBarItem"/> のアイコンをアイコンパネルに追加します。
	/// アイコンが <see cref="SideBarIcon"/> の場合はクリックイベントもサブスクライブします。
	/// </summary>
	/// <param name="item">追加するアイテム。</param>
	/// <param name="index">挿入位置。負値または範囲外の場合は末尾に追加します。</param>
	private void AddItemToPanel(SideBarItem item, int index = -1) {
		if (_iconPanel is null || item.Icon is null)
			return;
		if (item.Icon is SideBarIcon sbi) {
			sbi.Clicked += OnIconClicked;
			sbi.DoubleClicked += OnIconDoubleClicked;
		}
		if (index < 0 || index >= _iconPanel.Children.Count)
			_iconPanel.Children.Add(item.Icon);
		else
			_iconPanel.Children.Insert(index, item.Icon);
	}

	/// <summary>
	/// 指定した <see cref="SideBarItem"/> のアイコンをアイコンパネルから削除します。
	/// アイコンが <see cref="SideBarIcon"/> の場合はイベントのサブスクライブも解除します。
	/// </summary>
	/// <param name="item">削除するアイテム。</param>
	private void RemoveItemFromPanel(SideBarItem item) {
		if (item.Icon is null)
			return;
		if (item.Icon is SideBarIcon sbi) {
			sbi.Clicked -= OnIconClicked;
			sbi.DoubleClicked -= OnIconDoubleClicked;
		}
		_iconPanel?.Children.Remove(item.Icon);
	}

	/// <summary>
	/// アイコンがシングルクリックされたときに呼び出されます。
	/// クリックされたアイコンを選択状態にし、<see cref="MenuMode"/> が <see cref="SideBarMenuMode.Hidden"/> なら
	/// <see cref="SideBarMenuMode.Overlay"/> に切り替えます。
	/// </summary>
	/// <param name="sender">クリックされた <see cref="SideBarIcon"/>。</param>
	/// <param name="e">イベントデータ。</param>
	private void OnIconClicked(object? sender, EventArgs e) {
		int index = GetIconIndex(sender as UIElement);
		if (index >= 0) {
			if (SelectedIndex == index) {
				// 選択されているアイコンをクリックされた
				if (MenuMode == SideBarMenuMode.Overlay) {
					MenuMode = SideBarMenuMode.Hidden;
				}else if (MenuMode == SideBarMenuMode.Hidden) {
					MenuMode = SideBarMenuMode.Overlay;
				}

			} else {
				// 選択されているアイコンとは別のアイコンがをクリックされた
				SelectedIndex = index;
				if (MenuMode == SideBarMenuMode.Hidden) {
					MenuMode = SideBarMenuMode.Overlay;
				}
			}
		}
	}

	/// <summary>
	/// アイコンがダブルクリックされたときに呼び出されます。
	/// <see cref="SideBarMenuMode.Docked"/> と <see cref="SideBarMenuMode.Hidden"/> をトグルします。
	/// </summary>
	/// <param name="sender">ダブルクリックされた <see cref="SideBarIcon"/>。</param>
	/// <param name="e">イベントデータ。</param>
	private void OnIconDoubleClicked(object? sender, EventArgs e) {
		MenuMode = MenuMode == SideBarMenuMode.Docked
			? SideBarMenuMode.Hidden
			: SideBarMenuMode.Docked;
	}

	/// <summary>
	/// オーバーレイ背景がタップされたときに呼び出されます。
	/// <see cref="MenuMode"/> が <see cref="SideBarMenuMode.Overlay"/> の場合は <see cref="SideBarMenuMode.Hidden"/> に切り替えます。
	/// </summary>
	/// <param name="sender">イベントの送信元。</param>
	/// <param name="e">イベントデータ。</param>
	private void OnOverlayBackdropTapped(object sender, TappedRoutedEventArgs e) {
		if (MenuMode == SideBarMenuMode.Overlay)
			MenuMode = SideBarMenuMode.Hidden;
	}

	/// <summary>
	/// 指定した <see cref="UIElement"/> が <see cref="Items"/> の何番目のアイコンかを返します。
	/// </summary>
	/// <param name="icon">検索するアイコン要素。<see langword="null"/> の場合は -1 を返します。</param>
	/// <returns>0 始まりのインデックス。見つからない場合は -1。</returns>
	private int GetIconIndex(UIElement? icon) {
		if (icon is null)
			return -1;
		for (int i = 0; i < Items.Count; i++) {
			if (Items[i].Icon == icon)
				return i;
		}
		return -1;
	}

	/// <summary>
	/// <see cref="MenuMode"/> 依存関係プロパティの値が変更されたときに呼び出されます。
	/// </summary>
	/// <param name="d">プロパティが変更された <see cref="SideBar"/> インスタンス。</param>
	/// <param name="e">変更前後の値を含むイベントデータ。</param>
	private static void OnMenuModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
		((SideBar)d).UpdateMenuMode();

	/// <summary>
	/// <see cref="IsMenuResizable"/> 依存関係プロパティの値が変更されたときに呼び出されます。
	/// </summary>
	/// <param name="d">プロパティが変更された <see cref="SideBar"/> インスタンス。</param>
	/// <param name="e">変更前後の値を含むイベントデータ。</param>
	private static void OnIsMenuResizableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
		((SideBar)d).UpdateResizerVisibility();

	/// <summary>
	/// <see cref="SelectedIndex"/> 依存関係プロパティの値が変更されたときに呼び出されます。
	/// </summary>
	/// <param name="d">プロパティが変更された <see cref="SideBar"/> インスタンス。</param>
	/// <param name="e">変更前後の値を含むイベントデータ。</param>
	private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		var sb = (SideBar)d;
		sb.UpdateSelectedPanel();
		sb.UpdateIconSelection();
	}

	/// <summary>
	/// <see cref="MenuMode"/> の値に応じてメニューパネルの表示・配置・オーバーレイ背景を更新します。
	/// </summary>
	private void UpdateMenuMode() {
		if (_menuPanel is null)
			return;

		_menuPanel.Visibility = MenuMode is SideBarMenuMode.Hidden ? Visibility.Collapsed : Visibility.Visible;
		_menuPanel.Width = _menuPanelWidth;

		if (_menuColumn is { } col)
			col.Width = new GridLength(MenuMode is SideBarMenuMode.Docked ? _menuPanelWidth : 0);

		if (MenuMode is SideBarMenuMode.Overlay) {
			Grid.SetColumn(_menuPanel, 2);
			Canvas.SetZIndex(_menuPanel, 10);
		} else if (MenuMode is SideBarMenuMode.Docked) {
			Grid.SetColumn(_menuPanel, 1);
			Canvas.SetZIndex(_menuPanel, 0);
		}

		if (_overlayBackdrop is { } bd)
			bd.Visibility = MenuMode is SideBarMenuMode.Overlay ? Visibility.Visible : Visibility.Collapsed;
	}

	/// <summary>
	/// <see cref="IsMenuResizable"/> に応じてリサイズハンドルの表示を更新します。
	/// </summary>
	private void UpdateResizerVisibility() {
		if (_menuResizer is not null)
			_menuResizer.Visibility = IsMenuResizable ? Visibility.Visible : Visibility.Collapsed;
	}

	private void SubscribeResizerEvents() {
		if (_menuResizer is null) return;
		_menuResizer.PointerEntered += OnResizerPointerEntered;
		_menuResizer.PointerExited += OnResizerPointerExited;
		_menuResizer.PointerPressed += OnResizerPointerPressed;
		_menuResizer.PointerMoved += OnResizerPointerMoved;
		_menuResizer.PointerReleased += OnResizerPointerReleased;
		_menuResizer.PointerCaptureLost += OnResizerPointerCaptureLost;
	}

	private void UnsubscribeResizerEvents() {
		if (_menuResizer is null) return;
		_menuResizer.PointerEntered -= OnResizerPointerEntered;
		_menuResizer.PointerExited -= OnResizerPointerExited;
		_menuResizer.PointerPressed -= OnResizerPointerPressed;
		_menuResizer.PointerMoved -= OnResizerPointerMoved;
		_menuResizer.PointerReleased -= OnResizerPointerReleased;
		_menuResizer.PointerCaptureLost -= OnResizerPointerCaptureLost;
	}

	private void OnResizerPointerEntered(object sender, PointerRoutedEventArgs e) {
		ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.SizeWestEast);
	}

	private void OnResizerPointerExited(object sender, PointerRoutedEventArgs e) {
		if (!_isResizing)
			ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
	}

	private void OnResizerPointerPressed(object sender, PointerRoutedEventArgs e) {
		if (_menuResizer is null) return;
		_menuResizer.CapturePointer(e.Pointer);
		_isResizing = true;
		_resizingStartX = e.GetCurrentPoint(this).Position.X;
		_resizingStartWidth = _menuPanelWidth;
		e.Handled = true;
	}

	private void OnResizerPointerMoved(object sender, PointerRoutedEventArgs e) {
		if (!_isResizing) return;
		double delta = e.GetCurrentPoint(this).Position.X - _resizingStartX;
		double newWidth = Math.Clamp(_resizingStartWidth + delta, 144, 800);
		_menuPanelWidth = newWidth;
		if (_menuPanel is not null)
			_menuPanel.Width = newWidth;
		if (_menuColumn is not null && MenuMode == SideBarMenuMode.Docked)
			_menuColumn.Width = new GridLength(newWidth);
		e.Handled = true;
	}

	private void OnResizerPointerReleased(object sender, PointerRoutedEventArgs e) {
		if (!_isResizing) return;
		_isResizing = false;
		_menuResizer?.ReleasePointerCapture(e.Pointer);
		ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
		e.Handled = true;
	}

	private void OnResizerPointerCaptureLost(object sender, PointerRoutedEventArgs e) {
		_isResizing = false;
		ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
	}

	/// <summary>
	/// <see cref="SelectedIndex"/> に対応する <see cref="SideBarItem.Panel"/> をメニューコンテンツに表示します。
	/// インデックスが範囲外の場合はコンテンツを <see langword="null"/> にクリアします。
	/// </summary>
	private void UpdateSelectedPanel() {
		if (_menuContent is null)
			return;
		int index = SelectedIndex;
		_menuContent.Content = index >= 0 && index < Items.Count ? Items[index].Panel : null;
	}

	/// <summary>
	/// 全 <see cref="SideBarIcon"/> の <see cref="SideBarIcon.IsSelected"/> を
	/// <see cref="SelectedIndex"/> と照合して更新します。
	/// </summary>
	private void UpdateIconSelection() {
		for (int i = 0; i < Items.Count; i++) {
			if (Items[i].Icon is SideBarIcon icon) {
				icon.IsSelected = i == SelectedIndex;
			}
		}
	}

	/// <summary>
	/// <see cref="PropertyWidth"/> 依存関係プロパティの値が変更されたときに呼び出されます。
	/// </summary>
	/// <param name="d">プロパティが変更された <see cref="SideBar"/> インスタンス。</param>
	/// <param name="e">変更前後の値を含むイベントデータ。</param>
	private static void OnPropertyWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
		((SideBar)d).UpdatePropertyColumn();

	/// <summary>
	/// <see cref="PropertyVisibility"/> 依存関係プロパティの値が変更されたときに呼び出されます。
	/// </summary>
	/// <param name="d">プロパティが変更された <see cref="SideBar"/> インスタンス。</param>
	/// <param name="e">変更前後の値を含むイベントデータ。</param>
	private static void OnPropertyVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
		((SideBar)d).UpdatePropertyColumn();

	/// <summary>
	/// <see cref="PropertyVisibility"/> および <see cref="PropertyWidth"/> に応じて
	/// プロパティエリアの表示・列幅を更新します。
	/// </summary>
	private void UpdatePropertyColumn() {
		if (_propertyContent is not null) {
			_propertyContent.Visibility = PropertyVisibility;
		}

		if (_propertyColumn is not null) {
			if (PropertyVisibility == Visibility.Collapsed) {
				_propertyColumn.Width = new GridLength(0);
			} else if (double.IsNaN(PropertyWidth)) {
				_propertyColumn.Width = GridLength.Auto;
			} else {
				_propertyColumn.Width = new GridLength(PropertyWidth);
			}
		}
	}
}
