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
	/// ヘッダ 1 セルの配置結果
	/// </summary>
	/// <param name="ColumnIndex">列のインデックス</param>
	/// <param name="X">ビューポート左端を基準としたセルの左端位置</param>
	/// <param name="Width">セルの幅</param>
	internal readonly record struct HeaderCell(int ColumnIndex, double X, double Width);

	/// <summary>
	/// 水平スクロール位置とビューポート幅から、描画対象のヘッダセルを配置する。
	/// </summary>
	/// <param name="columns">列定義</param>
	/// <param name="horizontalOffset">水平スクロールオフセット</param>
	/// <param name="viewportWidth">ビューポートの幅</param>
	/// <returns>表示範囲に含まれるヘッダセル</returns>
	internal static IReadOnlyList<HeaderCell> LayoutHeader(IReadOnlyList<RecordGridColumn> columns, double horizontalOffset, double viewportWidth) {
		ArgumentNullException.ThrowIfNull(columns);

		var engine = new VirtualizationEngine(30, 0, 100, columns.Count);
		for (int i = 0; i < columns.Count; i++) {
			double width = columns[i].Width;
			if (double.IsFinite(width) && width > 0) {
				engine.SetColumnWidth(i, width);
			}
		}

		var range = engine.GetVisibleColumnRange(horizontalOffset, viewportWidth);
		var cells = new List<HeaderCell>(Math.Max(0, range.EndIndex - range.StartIndex));
		double x = -range.OffsetWithinFirstColumn;
		for (int i = range.StartIndex; i < range.EndIndex; i++) {
			double width = columns[i].Width;
			if (!double.IsFinite(width) || width <= 0) {
				width = engine.DefaultColumnWidth;
			}
			cells.Add(new HeaderCell(i, x, width));
			x += width;
		}
		return cells;
	}

	/// <summary>
	/// 列ヘッダを描画する。
	/// </summary>
	/// <param name="session">描画セッション</param>
	/// <param name="viewportWidth">ビューポートの幅</param>
	private void DrawHeader(CanvasDrawingSession session, float viewportWidth) {
		float headerHeight = (float)HeaderHeight;
		if (headerHeight <= 0 || Columns.Count == 0) {
			return;
		}

		Color defaultBackground = Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0);
		Color defaultForeground = Colors.Black;
		Color borderColor = Color.FromArgb(0xFF, 0xC8, 0xC8, 0xC8);

		foreach (HeaderCell cell in LayoutHeader(Columns, HorizontalOffset, viewportWidth)) {
			RecordGridColumn column = Columns[cell.ColumnIndex];
			var rect = new Rect(cell.X, 0, cell.Width, headerHeight);

			session.FillRectangle(rect, ToColor(column.Background, defaultBackground));

			using (session.CreateLayer(1f, rect)) {
				using var format = new CanvasTextFormat {
					FontSize = 14,
					VerticalAlignment = CanvasVerticalAlignment.Center,
					HorizontalAlignment = ToCanvasAlignment(column.Alignment),
					WordWrapping = CanvasWordWrapping.NoWrap,
					TrimmingGranularity = CanvasTextTrimmingGranularity.Character,
				};
				var textRect = new Rect(cell.X + 4, 0, Math.Max(0, cell.Width - 8), headerHeight);
				session.DrawText(column.Caption, textRect, ToColor(column.Foreground, defaultForeground), format);
			}

			// セルの右端と下端に罫線を引く
			float right = (float)(cell.X + cell.Width) - 0.5f;
			session.DrawLine(right, 0, right, headerHeight, borderColor);
			session.DrawLine((float)cell.X, headerHeight - 0.5f, (float)(cell.X + cell.Width), headerHeight - 0.5f, borderColor);
		}
	}

	/// <summary>
	/// ブラシを描画用の色に変換する。SolidColorBrush 以外は既定色を返す。
	/// </summary>
	/// <param name="brush">変換元のブラシ</param>
	/// <param name="fallback">変換できない場合の色</param>
	/// <returns>描画用の色</returns>
	private static Color ToColor(Brush? brush, Color fallback) {
		return brush is SolidColorBrush solid ? solid.Color : fallback;
	}

	/// <summary>
	/// 文字配置を Win2D の水平配置に変換する。
	/// </summary>
	/// <param name="alignment">文字配置</param>
	/// <returns>Win2D の水平配置</returns>
	private static CanvasHorizontalAlignment ToCanvasAlignment(TextAlignment alignment) {
		return alignment switch {
			TextAlignment.Center => CanvasHorizontalAlignment.Center,
			TextAlignment.Right => CanvasHorizontalAlignment.Right,
			_ => CanvasHorizontalAlignment.Left,
		};
	}
}
