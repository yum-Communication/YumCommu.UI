using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using System;

namespace YumCommu.UI;

public sealed partial class RecordGrid {

	/// <summary>
	/// 列幅が不正な場合に使用する幅 (ピクセル)
	/// </summary>
	private const double DefaultColumnWidth = 100;

	/// <summary>
	/// 横スクロールバーの小さい移動量 (ピクセル)
	/// </summary>
	private const double HorizontalSmallChange = 20;

	/// <summary>
	/// マウスホイール 1 ノッチあたりにスクロールする行数
	/// </summary>
	private const int WheelRowsPerNotch = 3;

	private ScrollBar? _horizontalScrollBar;
	private ScrollBar? _verticalScrollBar;
	private bool _updatingScrollBars;

	/// <summary>
	/// 内容の大きさとビューポートの大きさから、スクロールバーの範囲を計算する。
	/// </summary>
	/// <param name="contentSize">内容全体の大きさ</param>
	/// <param name="viewportSize">表示領域の大きさ</param>
	/// <returns>スクロールオフセットの最大値と、スクロールバーのビューポートサイズ</returns>
	internal static (double Maximum, double ViewportSize) GetScrollRange(double contentSize, double viewportSize) {
		double content = double.IsFinite(contentSize) ? Math.Max(0, contentSize) : 0;
		double viewport = double.IsFinite(viewportSize) ? Math.Max(0, viewportSize) : 0;
		return (Math.Max(0, content - viewport), viewport);
	}

	/// <summary>
	/// スクロールオフセットを移動量だけ動かし、0 から最大値の範囲に収める。
	/// </summary>
	/// <param name="offset">現在のオフセット</param>
	/// <param name="delta">移動量</param>
	/// <param name="maximum">オフセットの最大値</param>
	/// <returns>補正後のオフセット</returns>
	internal static double ScrollBy(double offset, double delta, double maximum) {
		double value = double.IsFinite(offset) ? offset : 0;
		double change = double.IsFinite(delta) ? delta : 0;
		return Math.Clamp(value + change, 0, Math.Max(0, maximum));
	}

	/// <summary>
	/// 列幅の合計 (ピクセル)
	/// </summary>
	/// <returns>全列の幅の合計</returns>
	private double GetContentWidth() {
		double total = 0;
		foreach (RecordGridColumn column in Columns) {
			total += double.IsFinite(column.Width) && column.Width > 0 ? column.Width : DefaultColumnWidth;
		}
		return total;
	}

	/// <summary>
	/// 列・データの変更をスクロール範囲の更新に結び付ける。
	/// </summary>
	private void InitializeScroll() {
		ColumnsChanged += (_, _) => UpdateScrollBars();
		_view.Changed += (_, _) => UpdateScrollBars();
		PointerWheelChanged += OnPointerWheel;
	}

	/// <summary>
	/// テンプレート内のスクロールバーとキャンバスのイベントを購読する。
	/// </summary>
	private void AttachScrollParts() {
		_horizontalScrollBar = GetTemplateChild("PART_ScrollBar_H") as ScrollBar;
		_verticalScrollBar = GetTemplateChild("PART_ScrollBar_V") as ScrollBar;

		if (_horizontalScrollBar is not null) {
			_horizontalScrollBar.ValueChanged += OnHorizontalScrollBarValueChanged;
		}
		if (_verticalScrollBar is not null) {
			_verticalScrollBar.ValueChanged += OnVerticalScrollBarValueChanged;
		}
		if (_canvas is not null) {
			_canvas.SizeChanged += OnCanvasSizeChanged;
		}
		UpdateScrollBars();
	}

	/// <summary>
	/// 購読中のスクロールバーとキャンバスのイベントを解除する。
	/// </summary>
	private void DetachScrollParts() {
		if (_horizontalScrollBar is not null) {
			_horizontalScrollBar.ValueChanged -= OnHorizontalScrollBarValueChanged;
			_horizontalScrollBar = null;
		}
		if (_verticalScrollBar is not null) {
			_verticalScrollBar.ValueChanged -= OnVerticalScrollBarValueChanged;
			_verticalScrollBar = null;
		}
		if (_canvas is not null) {
			_canvas.SizeChanged -= OnCanvasSizeChanged;
		}
	}

	/// <summary>
	/// 内容とビューポートの大きさに合わせて、スクロールバーの範囲と値を更新する。
	/// 範囲が縮んでオフセットが範囲外になった場合は最大値に補正する。
	/// </summary>
	private void UpdateScrollBars() {
		if (_canvas is null) {
			return;
		}

		var horizontal = GetScrollRange(GetContentWidth(), _canvas.ActualWidth);
		var vertical = GetScrollRange(_view.Count * RowHeight, _canvas.ActualHeight - HeaderHeight);

		_updatingScrollBars = true;
		try {
			if (_horizontalScrollBar is not null) {
				ApplyScrollRange(_horizontalScrollBar, horizontal, HorizontalSmallChange, HorizontalOffset);
			}
			if (_verticalScrollBar is not null) {
				ApplyScrollRange(_verticalScrollBar, vertical, RowHeight, VerticalOffset);
			}
		}
		finally {
			_updatingScrollBars = false;
		}

		double horizontalOffset = ScrollBy(HorizontalOffset, 0, horizontal.Maximum);
		if (horizontalOffset != HorizontalOffset) {
			HorizontalOffset = horizontalOffset;
		}
		double verticalOffset = ScrollBy(VerticalOffset, 0, vertical.Maximum);
		if (verticalOffset != VerticalOffset) {
			VerticalOffset = verticalOffset;
		}
	}

	/// <summary>
	/// スクロールバーに範囲・移動量・値を設定する。
	/// </summary>
	/// <param name="scrollBar">設定先のスクロールバー</param>
	/// <param name="range">スクロール範囲</param>
	/// <param name="smallChange">小さい移動量</param>
	/// <param name="offset">現在のオフセット</param>
	private static void ApplyScrollRange(ScrollBar scrollBar, (double Maximum, double ViewportSize) range, double smallChange, double offset) {
		scrollBar.Minimum = 0;
		scrollBar.Maximum = range.Maximum;
		scrollBar.ViewportSize = range.ViewportSize;
		scrollBar.SmallChange = smallChange;
		scrollBar.LargeChange = range.ViewportSize;
		scrollBar.Value = ScrollBy(offset, 0, range.Maximum);
	}

	/// <summary>
	/// オフセットの変更をスクロールバーの値と再描画に反映する。
	/// </summary>
	/// <param name="d">変更されたグリッド</param>
	/// <param name="e">変更の内容</param>
	private static void OnScrollOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		var grid = (RecordGrid)d;
		grid.SyncScrollBarValues();
		grid._canvas?.Invalidate();
	}

	/// <summary>
	/// ヘッダ高や行高の変更をスクロール範囲の更新と再描画に反映する。
	/// </summary>
	/// <param name="d">変更されたグリッド</param>
	/// <param name="e">変更の内容</param>
	private static void OnScrollLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		var grid = (RecordGrid)d;
		grid.UpdateScrollBars();
		grid._canvas?.Invalidate();
	}

	/// <summary>
	/// スクロールバーの値をオフセットに合わせる。
	/// </summary>
	private void SyncScrollBarValues() {
		if (_updatingScrollBars) {
			return;
		}
		_updatingScrollBars = true;
		try {
			if (_horizontalScrollBar is not null) {
				_horizontalScrollBar.Value = ScrollBy(HorizontalOffset, 0, _horizontalScrollBar.Maximum);
			}
			if (_verticalScrollBar is not null) {
				_verticalScrollBar.Value = ScrollBy(VerticalOffset, 0, _verticalScrollBar.Maximum);
			}
		}
		finally {
			_updatingScrollBars = false;
		}
	}

	/// <summary>
	/// 横スクロールバーの操作をオフセットに反映する。
	/// </summary>
	/// <param name="sender">イベント発行元</param>
	/// <param name="e">値の変更内容</param>
	private void OnHorizontalScrollBarValueChanged(object sender, RangeBaseValueChangedEventArgs e) {
		if (!_updatingScrollBars) {
			HorizontalOffset = e.NewValue;
		}
	}

	/// <summary>
	/// 縦スクロールバーの操作をオフセットに反映する。
	/// </summary>
	/// <param name="sender">イベント発行元</param>
	/// <param name="e">値の変更内容</param>
	private void OnVerticalScrollBarValueChanged(object sender, RangeBaseValueChangedEventArgs e) {
		if (!_updatingScrollBars) {
			VerticalOffset = e.NewValue;
		}
	}

	/// <summary>
	/// キャンバスの大きさが変わったときにスクロール範囲を更新する。
	/// </summary>
	/// <param name="sender">イベント発行元</param>
	/// <param name="e">サイズ変更の内容</param>
	private void OnCanvasSizeChanged(object sender, SizeChangedEventArgs e) {
		UpdateScrollBars();
	}

	/// <summary>
	/// マウスホイールでスクロールする。Shift キー押下または横ホイールの場合は横方向にスクロールする。
	/// </summary>
	/// <param name="sender">イベント発行元</param>
	/// <param name="e">ホイール操作の内容</param>
	private void OnPointerWheel(object sender, PointerRoutedEventArgs e) {
		var properties = e.GetCurrentPoint(this).Properties;
		double notches = properties.MouseWheelDelta / 120.0;
		bool horizontal = properties.IsHorizontalMouseWheel
			|| (e.KeyModifiers & Windows.System.VirtualKeyModifiers.Shift) == Windows.System.VirtualKeyModifiers.Shift;

		if (horizontal) {
			double maximum = _horizontalScrollBar?.Maximum ?? 0;
			// 横ホイールは右方向が正、縦ホイールは上方向が正のため符号が異なる。
			double direction = properties.IsHorizontalMouseWheel ? 1 : -1;
			HorizontalOffset = ScrollBy(HorizontalOffset, direction * notches * HorizontalSmallChange * WheelRowsPerNotch, maximum);
		}
		else {
			double maximum = _verticalScrollBar?.Maximum ?? 0;
			VerticalOffset = ScrollBy(VerticalOffset, -notches * RowHeight * WheelRowsPerNotch, maximum);
		}
		e.Handled = true;
	}
}
