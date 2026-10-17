using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI;

namespace YumCommu.UI;

public sealed partial class RecordGrid {

	/// <summary>
	/// 行 1 つの配置結果
	/// </summary>
	/// <param name="ViewIndex">表示行のインデックス</param>
	/// <param name="Y">一覧エリア上端を基準とした行の上端位置</param>
	/// <param name="Height">行の高さ</param>
	internal readonly record struct RowCell(int ViewIndex, double Y, double Height);

	/// <summary>
	/// 垂直スクロール位置とビューポート高から、描画対象の行を配置する。
	/// </summary>
	/// <param name="rowCount">表示行数</param>
	/// <param name="rowHeight">行の高さ</param>
	/// <param name="verticalOffset">垂直スクロールオフセット</param>
	/// <param name="viewportHeight">一覧エリアの高さ</param>
	/// <returns>表示範囲に含まれる行</returns>
	internal static IReadOnlyList<RowCell> LayoutRows(int rowCount, double rowHeight, double verticalOffset, double viewportHeight) {
		ArgumentOutOfRangeException.ThrowIfNegative(rowCount);
		if (!double.IsFinite(rowHeight) || rowHeight <= 0) {
			throw new ArgumentOutOfRangeException(nameof(rowHeight));
		}
		if (rowCount == 0 || viewportHeight <= 0) {
			return Array.Empty<RowCell>();
		}

		var engine = new VirtualizationEngine(rowHeight, rowCount);
		var range = engine.GetVisibleRange(verticalOffset, viewportHeight);
		var rows = new List<RowCell>(Math.Max(0, range.EndIndex - range.StartIndex));
		double y = -range.OffsetWithinFirstRow;
		for (int i = range.StartIndex; i < range.EndIndex; i++) {
			rows.Add(new RowCell(i, y, rowHeight));
			y += rowHeight;
		}
		return rows;
	}

	/// <summary>
	/// 一覧エリアの行を描画する。表示範囲の行と列だけを描画対象にする。
	/// </summary>
	/// <param name="session">描画セッション</param>
	/// <param name="viewportWidth">コントロールの幅</param>
	/// <param name="viewportHeight">コントロールの高さ (列ヘッダを含む)</param>
	private void DrawRows(CanvasDrawingSession session, float viewportWidth, float viewportHeight) {
		float headerHeight = (float)Math.Max(0, HeaderHeight);
		float bodyHeight = viewportHeight - headerHeight;
		if (bodyHeight <= 0 || Columns.Count == 0 || _view.Count == 0) {
			return;
		}

		IReadOnlyList<HeaderCell> columnCells = LayoutHeader(Columns, HorizontalOffset, viewportWidth);
		IReadOnlyList<RowCell> rowCells = LayoutRows(_view.Count, RowHeight, VerticalOffset, bodyHeight);
		if (columnCells.Count == 0 || rowCells.Count == 0) {
			return;
		}

		Color defaultForeground = Colors.Black;
		Color borderColor = Color.FromArgb(0xFF, 0xE0, 0xE0, 0xE0);

		var formats = new CanvasTextFormat[columnCells.Count];
		try {
			for (int k = 0; k < columnCells.Count; k++) {
				formats[k] = new CanvasTextFormat {
					FontSize = 14,
					VerticalAlignment = CanvasVerticalAlignment.Center,
					HorizontalAlignment = ToCanvasAlignment(Columns[columnCells[k].ColumnIndex].Alignment),
					WordWrapping = CanvasWordWrapping.NoWrap,
					TrimmingGranularity = CanvasTextTrimmingGranularity.Character,
				};
			}

			using (session.CreateLayer(1f, new Rect(0, headerHeight, viewportWidth, bodyHeight))) {
				foreach (RowCell row in rowCells) {
					object? record = _view.GetRecord(row.ViewIndex);
					float top = headerHeight + (float)row.Y;
					float rowHeight = (float)row.Height;

					for (int k = 0; k < columnCells.Count; k++) {
						HeaderCell cell = columnCells[k];
						RecordGridColumn column = Columns[cell.ColumnIndex];

						if (column.Background is SolidColorBrush background) {
							session.FillRectangle(new Rect(cell.X, top, cell.Width, rowHeight), background.Color);
						}

						string text = record is null ? string.Empty : column.FormatValue(column.GetValue(record));
						if (text.Length > 0) {
							var textRect = new Rect(cell.X + 4, top, Math.Max(0, cell.Width - 8), rowHeight);
							session.DrawText(text, textRect, ToColor(column.Foreground, defaultForeground), formats[k]);
						}
					}

					float bottom = top + rowHeight - 0.5f;
					session.DrawLine(0, bottom, viewportWidth, bottom, borderColor);
				}

				// 列の右端に縦罫線を引く
				float bodyBottom = headerHeight + bodyHeight;
				foreach (HeaderCell cell in columnCells) {
					float right = (float)(cell.X + cell.Width) - 0.5f;
					session.DrawLine(right, headerHeight, right, bodyBottom, borderColor);
				}
			}
		}
		finally {
			foreach (CanvasTextFormat? format in formats) {
				format?.Dispose();
			}
		}
	}
}
