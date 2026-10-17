using System;
using System.Collections.Generic;

namespace YumCommu.UI;


/// <summary>
/// 行高と列幅から表示範囲を計算します。
/// </summary>
internal class VirtualizationEngine {
	private readonly Dictionary<int, double> _rowHeights = new();
	private readonly Dictionary<int, double> _columnWidths = new();
	private double _defaultRowHeight;
	private double _defaultColumnWidth;
	private int _totalRowCount;
	private int _totalColumnCount;

	/// <summary>
	/// 個別幅が指定されていない列に使用する幅を設定します。
	/// </summary>
	public double DefaultColumnWidth {
		get => _defaultColumnWidth;
		set {
			if (!double.IsFinite(value) || value <= 0) {
				throw new ArgumentOutOfRangeException(nameof(value));
			}
			_defaultColumnWidth = value;
		}
	}

	/// <summary>
	/// 表示するデータの総列数を設定します。
	/// </summary>
	public int TotalColumnCount {
		get => _totalColumnCount;
		set {
			if (value < 0) {
				throw new ArgumentOutOfRangeException(nameof(value));
			}
			if (value < _totalColumnCount) {
				for (int i = value; i < _totalColumnCount; i++) {
					_columnWidths.Remove(i);
				}
			}
			_totalColumnCount = value;
		}
	}

	/// <summary>
	/// 全列の幅の合計を取得します。
	/// </summary>
	public double TotalWidth {
		get {
			double totalWidth = 0;
			for (int i = 0; i < TotalColumnCount; i++) {
				totalWidth += _columnWidths.TryGetValue(i, out var customWidth) ? customWidth : DefaultColumnWidth;
			}
			return totalWidth;
		}
	}

	/// <summary>
	/// デフォルトの行の高さを設定します。これは、行の高さが不明な場合に使用されます。
	/// </summary>
	public double DefaultRowHeight {
		get => _defaultRowHeight;
		set {
			if (!double.IsFinite(value) || value <= 0) {
				throw new ArgumentOutOfRangeException(nameof(value));
			}
			_defaultRowHeight = value;
		}
	}

	/// <summary>
	/// 表示するデータの総行数を設定します。これにより、仮想化エンジンはスクロール位置に基づいて正しい行をレンダリングできます。
	/// </summary>
	public int TotalRowCount {
		get => _totalRowCount;
		set {
			if (value < 0) {
				throw new ArgumentOutOfRangeException(nameof(value));
			}
			if (value < _totalRowCount) {
				for (int i = value; i < _totalRowCount; i++) {
					_rowHeights.Remove(i);
				}
			}
			_totalRowCount = value;
		}
	}

	/// <summary>
	/// 総行数と各行の高さに基づいて、すべての行の合計高さを計算します。これにより、スクロールバーのサイズやスクロール位置の計算に使用できます。
	/// </summary>
	public double TotalHeight {
		get {
			double totalHeight = 0;
			for (int i = 0; i < TotalRowCount; i++) {
				totalHeight += _rowHeights.TryGetValue(i, out var customHeight) ? customHeight : DefaultRowHeight;
			}
			return totalHeight;
		}
	}

	/// <summary>
	/// 既定行高と総行数を設定します。
	/// </summary>
	public VirtualizationEngine(double height, int count): this(height, count, 100, 0) {
	}

	/// <summary>
	/// 既定行高、総行数、既定列幅、総列数を設定します。
	/// </summary>
	/// <param name="height">既定行高</param>
	/// <param name="rowCount">総行数</param>
	/// <param name="width">既定列幅</param>
	/// <param name="columnCount">総列数</param>
	public VirtualizationEngine(double height, int rowCount, double width, int columnCount) {
		DefaultRowHeight = height;
		TotalRowCount = rowCount;
		DefaultColumnWidth = width;
		TotalColumnCount = columnCount;
	}

	/// <summary>
	/// コンストラクタでは、デフォルトの行の高さと総行数を初期化します。
	/// </summary>
	public VirtualizationEngine(): this(30, 0) {
	}


	/// <summary>
	/// インデックスに基づいて特定の行の高さを設定します。これにより、特定の行の高さをカスタマイズできます。
	/// </summary>
	/// <param name="rowIndex">行のインデックス</param>
	/// <param name="height">行の高さ</param>
	public void SetRowHeight(int rowIndex, double height) {
		if (rowIndex < 0 || rowIndex >= TotalRowCount) {
			throw new ArgumentOutOfRangeException(nameof(rowIndex));
		}
		if (!double.IsFinite(height) || height <= 0) {
			throw new ArgumentOutOfRangeException(nameof(height));
		}
		_rowHeights[rowIndex] = height;
	}

	/// <summary>
	/// 指定した列の個別幅を設定します。
	/// </summary>
	/// <param name="columnIndex">列のインデックス</param>
	/// <param name="width">列の幅</param>
	public void SetColumnWidth(int columnIndex, double width) {
		if (columnIndex < 0 || columnIndex >= TotalColumnCount) {
			throw new ArgumentOutOfRangeException(nameof(columnIndex));
		}
		if (!double.IsFinite(width) || width <= 0) {
			throw new ArgumentOutOfRangeException(nameof(width));
		}
		_columnWidths[columnIndex] = width;
	}

	/// <summary>
	/// 水平スクロール位置とビューポート幅から表示される列の半開区間を計算します。
	/// </summary>
	/// <param name="scrollOffset">水平スクロールオフセット</param>
	/// <param name="viewportWidth">ビューポートの幅</param>
	/// <returns>開始列、排他的な終了列、先頭列内のオフセット</returns>
	public (int StartIndex, int EndIndex, double OffsetWithinFirstColumn) GetVisibleColumnRange(double scrollOffset, double viewportWidth) {
		if (!double.IsFinite(scrollOffset)) {
			throw new ArgumentOutOfRangeException(nameof(scrollOffset));
		}
		if (!double.IsFinite(viewportWidth) || viewportWidth < 0) {
			throw new ArgumentOutOfRangeException(nameof(viewportWidth));
		}
		if (TotalColumnCount == 0 || viewportWidth == 0) {
			return (0, 0, 0);
		}

		scrollOffset = Math.Clamp(scrollOffset, 0, Math.Max(0, TotalWidth - viewportWidth));
		double accumulatedWidth = 0;
		int startIndex = 0;
		double offsetWithinFirstColumn = 0;

		for (int i = 0; i < TotalColumnCount; i++) {
			double columnWidth = _columnWidths.TryGetValue(i, out var customWidth) ? customWidth : DefaultColumnWidth;
			if (accumulatedWidth + columnWidth > scrollOffset) {
				startIndex = i;
				offsetWithinFirstColumn = scrollOffset - accumulatedWidth;
				break;
			}
			accumulatedWidth += columnWidth;
		}

		double rightEdge = scrollOffset + viewportWidth;
		for (int i = startIndex; i < TotalColumnCount; i++) {
			double columnWidth = _columnWidths.TryGetValue(i, out var customWidth) ? customWidth : DefaultColumnWidth;
			accumulatedWidth += columnWidth;
			if (accumulatedWidth >= rightEdge) {
				return (startIndex, i + 1, offsetWithinFirstColumn);
			}
		}

		return (startIndex, TotalColumnCount, offsetWithinFirstColumn);
	}


	/// <summary>
	/// 表示される行の範囲を計算します。スクロールオフセットとビューポートの高さに基づいて、開始インデックスと終了インデックスを返します。
	/// </summary>
	/// <param name="scrollOffset">スクロールオフセット</param>
	/// <param name="viewportHeight">ビューポートの高さ</param>
	/// <returns>開始インデックスと終了インデックスのタプル</returns>
	public (int StartIndex, int EndIndex, double OffsetWithinFirstRow) GetVisibleRange(double scrollOffset, double viewportHeight) {
		if (!double.IsFinite(scrollOffset)) {
			throw new ArgumentOutOfRangeException(nameof(scrollOffset));
		}
		if (!double.IsFinite(viewportHeight) || viewportHeight < 0) {
			throw new ArgumentOutOfRangeException(nameof(viewportHeight));
		}
		if (TotalRowCount == 0 || viewportHeight == 0) {
			return (0, 0, 0);
		}

		// スクロール位置から開始インデックスを計算する
		scrollOffset = Math.Clamp(scrollOffset, 0, Math.Max(0, TotalHeight - viewportHeight));
		double accumulatedHeight = 0;
		int startIndex = 0;
		double offsetWithinFirstRow = 0;

		for (int i = 0; i < TotalRowCount; i++) {
			double rowHeight = _rowHeights.TryGetValue(i, out var customHeight)	? customHeight : DefaultRowHeight;
			if (accumulatedHeight + rowHeight > scrollOffset) {
				startIndex = i;
				offsetWithinFirstRow = scrollOffset - accumulatedHeight;
				break;
			}
			accumulatedHeight += rowHeight;
		}

		// 終了インデックスを計算する
		double Y = scrollOffset + viewportHeight;
		for (int i = startIndex; i < TotalRowCount; i++) {
			double rowHeight = _rowHeights.TryGetValue(i, out var customHeight) ? customHeight : DefaultRowHeight;
			accumulatedHeight += rowHeight;
			if (accumulatedHeight >= Y) {
				return (startIndex, i + 1, offsetWithinFirstRow);
			}
		}

		return (startIndex, TotalRowCount, offsetWithinFirstRow);
	}
}
