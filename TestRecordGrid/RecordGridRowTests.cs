using YumCommu.UI;

namespace TestRecordGrid;

/// <summary>
/// RecordGrid の行配置に関するテストです。
/// </summary>
public class RecordGridRowTests {

	/// <summary>
	/// 先頭から全行が連続して配置されます。
	/// </summary>
	[Fact]
	public void LayoutRows_FromStart_PlacesRowsContiguously() {
		var rows = RecordGrid.LayoutRows(3, 20, 0, 1000);

		Assert.Equal(3, rows.Count);
		Assert.Equal(new RecordGrid.RowCell(0, 0, 20), rows[0]);
		Assert.Equal(new RecordGrid.RowCell(1, 20, 20), rows[1]);
		Assert.Equal(new RecordGrid.RowCell(2, 40, 20), rows[2]);
	}

	/// <summary>
	/// スクロール量分だけ先頭行が上にはみ出し、範囲外の行は含まれません。
	/// </summary>
	[Fact]
	public void LayoutRows_WithOffset_ShiftsFirstRowAndSkipsHiddenRows() {
		var rows = RecordGrid.LayoutRows(100, 20, 50, 60);

		Assert.Equal(new[] { 2, 3, 4, 5 }, rows.Select(r => r.ViewIndex));
		Assert.Equal(-10, rows[0].Y);
		Assert.Equal(10, rows[1].Y);
	}

	/// <summary>
	/// 最大位置を超えたスクロール量は末尾が見える位置に補正されます。
	/// </summary>
	[Fact]
	public void LayoutRows_OffsetBeyondEnd_IsClampedToLastRows() {
		var rows = RecordGrid.LayoutRows(10, 20, 10000, 60);

		Assert.Equal(new[] { 7, 8, 9 }, rows.Select(r => r.ViewIndex));
		Assert.Equal(0, rows[0].Y);
	}

	/// <summary>
	/// 行がない場合やビューポート高が 0 以下の場合は空になります。
	/// </summary>
	[Fact]
	public void LayoutRows_EmptyOrZeroViewport_ReturnsEmpty() {
		Assert.Empty(RecordGrid.LayoutRows(0, 20, 0, 100));
		Assert.Empty(RecordGrid.LayoutRows(5, 20, 0, 0));
		Assert.Empty(RecordGrid.LayoutRows(5, 20, 0, -10));
	}

	/// <summary>
	/// 無効な行数や行高は例外になります。
	/// </summary>
	[Fact]
	public void LayoutRows_InvalidArguments_Throw() {
		Assert.Throws<ArgumentOutOfRangeException>(() => RecordGrid.LayoutRows(-1, 20, 0, 100));
		Assert.Throws<ArgumentOutOfRangeException>(() => RecordGrid.LayoutRows(1, 0, 0, 100));
		Assert.Throws<ArgumentOutOfRangeException>(() => RecordGrid.LayoutRows(1, double.NaN, 0, 100));
	}
}
