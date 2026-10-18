using YumCommu.UI;

namespace TestRecordGrid;

/// <summary>
/// RecordGrid の列ヘッダ配置に関するテストです。
/// </summary>
public class RecordGridHeaderTests {
	private static List<RecordGridColumn> CreateColumns(params double[] widths) {
		return widths.Select(w => new RecordGridColumn { Caption = "c", Width = w }).ToList();
	}

	/// <summary>
	/// 先頭から全列が連続して配置されます。
	/// </summary>
	[Fact]
	public void LayoutHeader_FromStart_PlacesColumnsContiguously() {
		var cells = RecordGrid.LayoutHeader(CreateColumns(100, 150, 80), 0, 1000);

		Assert.Equal(3, cells.Count);
		Assert.Equal(new RecordGrid.HeaderCell(0, 0, 100), cells[0]);
		Assert.Equal(new RecordGrid.HeaderCell(1, 100, 150), cells[1]);
		Assert.Equal(new RecordGrid.HeaderCell(2, 250, 80), cells[2]);
	}

	/// <summary>
	/// 水平オフセット分だけ先頭セルが左にはみ出し、範囲外の列は含まれません。
	/// </summary>
	[Fact]
	public void LayoutHeader_WithOffset_ShiftsFirstCellAndSkipsHiddenColumns() {
		var cells = RecordGrid.LayoutHeader(CreateColumns(100, 100, 100, 100, 100), 120, 200);

		Assert.Equal(new[] { 1, 2, 3 }, cells.Select(c => c.ColumnIndex));
		Assert.Equal(-20, cells[0].X);
		Assert.Equal(80, cells[1].X);
	}

	/// <summary>
	/// 列がない場合やビューポート幅が 0 の場合は空になります。
	/// </summary>
	[Fact]
	public void LayoutHeader_EmptyOrZeroViewport_ReturnsNoCells() {
		Assert.Empty(RecordGrid.LayoutHeader(CreateColumns(), 0, 500));
		Assert.Empty(RecordGrid.LayoutHeader(CreateColumns(100), 0, 0));
	}

	/// <summary>
	/// 幅が不正な列は既定幅で配置されます。
	/// </summary>
	[Fact]
	public void LayoutHeader_InvalidWidth_UsesDefaultWidth() {
		var cells = RecordGrid.LayoutHeader(CreateColumns(0, 50), 0, 1000);

		Assert.Equal(100, cells[0].Width);
		Assert.Equal(100, cells[1].X);
	}

	/// <summary>
	/// null の列コレクションは例外になります。
	/// </summary>
	[Fact]
	public void LayoutHeader_NullColumns_Throws() {
		Assert.Throws<ArgumentNullException>(() => RecordGrid.LayoutHeader(null!, 0, 100));
	}
}
