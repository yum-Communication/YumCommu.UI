using System;
using System.Collections.Generic;

namespace YumCommu.UI;


/// <summary>
/// 行高から表示範囲を計算します。
/// </summary>
internal class VirtualizationEngine {
	private readonly Dictionary<int, double> _rowHeights = new();
	private double _defaultRowHeight;
	private int _totalRowCount;

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
	public VirtualizationEngine(double height, int count) {
		DefaultRowHeight = height;
		TotalRowCount = count;
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
