using YumCommu.UI;

namespace TestRecordGrid;

/// <summary>
/// RecordGrid のスクロール範囲計算に関するテストです。
/// </summary>
public class RecordGridScrollTests {

	/// <summary>
	/// 内容がビューポートより大きい場合、差がオフセットの最大値になります。
	/// </summary>
	[Fact]
	public void GetScrollRange_ContentLargerThanViewport_MaximumIsDifference() {
		var range = RecordGrid.GetScrollRange(1000, 300);

		Assert.Equal(700, range.Maximum);
		Assert.Equal(300, range.ViewportSize);
	}

	/// <summary>
	/// 内容がビューポート以下の場合、最大値は 0 になります。
	/// </summary>
	[Theory]
	[InlineData(300, 300)]
	[InlineData(100, 300)]
	[InlineData(0, 300)]
	public void GetScrollRange_ContentFits_MaximumIsZero(double content, double viewport) {
		Assert.Equal(0, RecordGrid.GetScrollRange(content, viewport).Maximum);
	}

	/// <summary>
	/// 負数や非有限値は 0 として扱われます。
	/// </summary>
	[Fact]
	public void GetScrollRange_InvalidValues_AreTreatedAsZero() {
		Assert.Equal((0.0, 0.0), RecordGrid.GetScrollRange(double.NaN, -5));
		Assert.Equal((100.0, 0.0), RecordGrid.GetScrollRange(100, double.PositiveInfinity));
	}

	/// <summary>
	/// 移動後のオフセットは 0 から最大値の範囲に収まります。
	/// </summary>
	[Theory]
	[InlineData(10, 20, 100, 30)]
	[InlineData(10, -50, 100, 0)]
	[InlineData(90, 50, 100, 100)]
	[InlineData(50, 10, 0, 0)]
	public void ScrollBy_ClampsToRange(double offset, double delta, double maximum, double expected) {
		Assert.Equal(expected, RecordGrid.ScrollBy(offset, delta, maximum));
	}

	/// <summary>
	/// 非有限値のオフセットや移動量は 0 として扱われます。
	/// </summary>
	[Fact]
	public void ScrollBy_NonFinite_IsTreatedAsZero() {
		Assert.Equal(5, RecordGrid.ScrollBy(double.NaN, 5, 100));
		Assert.Equal(7, RecordGrid.ScrollBy(7, double.NaN, 100));
	}
}
