using System;
using System.Collections.Generic;
using System.Text;

namespace YumCommu.UI;


internal class VirtualizationEngine(double height, int count) {

	/// <summary>
	/// デフォルトの行の高さを設定します。これは、行の高さが不明な場合に使用されます。
	/// </summary>
	public double DefaultRowHeight { get; set; } = height;

	/// <summary>
	/// 表示するデータの総行数を設定します。これにより、仮想化エンジンはスクロール位置に基づいて正しい行をレンダリングできます。
	/// </summary>
	public int TotalRowCount { get; set; } = count;

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

	private Dictionary<int, double> _rowHeights = new();


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
		// ここでは、特定の行の高さを設定するロジックを実装します。
		// 例えば、行の高さを保持するデータ構造を使用して、指定された行の高さを更新します。
		_rowHeights[rowIndex] = height;
	}


	/// <summary>
	/// 表示される行の範囲を計算します。スクロールオフセットとビューポートの高さに基づいて、開始インデックスと終了インデックスを返します。
	/// </summary>
	/// <param name="scrollOffset">スクロールオフセット</param>
	/// <param name="viewportHeight">ビューポートの高さ</param>
	/// <returns>開始インデックスと終了インデックスのタプル</returns>
	public (int StartIndex, int EndIndex, double SubHeight) GetVisibleRange(double scrollOffset, double viewportHeight) {
		// スクロール位置から開始インデックスを計算する
		if (scrollOffset < 0) {
			scrollOffset = 0;
		}
		double accumulatedHeight = 0;
		int startIndex = 0;
		double subHeight = 0;
		bool found = false;

		for (int i = 0; i < TotalRowCount; i++) {
			double rowHeight = _rowHeights.TryGetValue(i, out var customHeight)	? customHeight : DefaultRowHeight;
			if (accumulatedHeight + rowHeight > scrollOffset) {
				startIndex = i;
				subHeight = scrollOffset - accumulatedHeight;
				found = true;
				break;
			}
			accumulatedHeight += rowHeight;
		}

		if (!found) {
			int last = Math.Max(0, TotalRowCount - 1);
			return (last, last, 0);
		}

		// 終了インデックスを計算する
		double Y = scrollOffset + viewportHeight;
		for (int i = startIndex; i < TotalRowCount; i++) {
			double rowHeight = _rowHeights.TryGetValue(i, out var customHeight) ? customHeight : DefaultRowHeight;
			accumulatedHeight += rowHeight;
			if (accumulatedHeight >= Y) {
				return (startIndex, i, subHeight);
			}
		}

		return (startIndex, TotalRowCount, subHeight);
	}
}
