using YumCommu.UI;

namespace TestRecordGrid;

/// <summary>
/// 列方向の仮想化に関するテストです。
/// </summary>
public class VirtualizationEngineColumnTests {
	/// <summary>
	/// 個別幅を含む総幅と可視列範囲を確認します。
	/// </summary>
	[Fact]
	public void GetVisibleColumnRange_WithCustomWidth_ReturnsIntersectingColumns() {
		var engine = new VirtualizationEngine(25, 2, 100, 5);
		engine.SetColumnWidth(1, 150);

		Assert.Equal(550, engine.TotalWidth);
		var range = engine.GetVisibleColumnRange(120, 200);
		Assert.Equal(1, range.StartIndex);
		Assert.Equal(3, range.EndIndex);
		Assert.Equal(20, range.OffsetWithinFirstColumn);
	}

	/// <summary>
	/// 境界に一致する列は次の列から開始し、右端の次の列を含めません。
	/// </summary>
	[Fact]
	public void GetVisibleColumnRange_AtBoundaries_UsesHalfOpenRange() {
		var engine = new VirtualizationEngine(25, 1, 100, 5);

		var range = engine.GetVisibleColumnRange(100, 200);
		Assert.Equal(1, range.StartIndex);
		Assert.Equal(3, range.EndIndex);
		Assert.Equal(0, range.OffsetWithinFirstColumn);
	}

	/// <summary>
	/// 末尾を超えるオフセットは最大スクロール位置に補正します。
	/// </summary>
	[Fact]
	public void GetVisibleColumnRange_BeyondEnd_ClampsToLastViewport() {
		var engine = new VirtualizationEngine(25, 1, 100, 5);

		var range = engine.GetVisibleColumnRange(1000, 200);
		Assert.Equal(3, range.StartIndex);
		Assert.Equal(5, range.EndIndex);
		Assert.Equal(0, range.OffsetWithinFirstColumn);
	}

	/// <summary>
	/// 空の列と幅0のビューポートでは空範囲を返します。
	/// </summary>
	[Fact]
	public void GetVisibleColumnRange_WithEmptyColumnsOrViewport_ReturnsEmptyRange() {
		var engine = new VirtualizationEngine();
		Assert.Equal((0, 0, 0.0), engine.GetVisibleColumnRange(0, 100));
		engine.TotalColumnCount = 3;
		Assert.Equal((0, 0, 0.0), engine.GetVisibleColumnRange(0, 0));
	}

	/// <summary>
	/// 列数を減らすと除外された列の個別幅を破棄します。
	/// </summary>
	[Fact]
	public void TotalColumnCount_WhenShrunkAndExpanded_DiscardsRemovedWidths() {
		var engine = new VirtualizationEngine(25, 1, 100, 3);
		engine.SetColumnWidth(2, 150);
		engine.TotalColumnCount = 2;
		engine.TotalColumnCount = 3;
		Assert.Equal(300, engine.TotalWidth);
	}

	/// <summary>
	/// 既定幅の変更は個別幅に影響しません。
	/// </summary>
	[Fact]
	public void DefaultColumnWidth_WhenChanged_PreservesCustomWidth() {
		var engine = new VirtualizationEngine(25, 1, 100, 3);
		engine.SetColumnWidth(1, 150);
		engine.DefaultColumnWidth = 120;
		Assert.Equal(390, engine.TotalWidth);
	}

	/// <summary>
	/// 無効な列設定は拒否します。
	/// </summary>
	[Fact]
	public void ColumnSettings_WhenInvalid_ThrowArgumentOutOfRangeException() {
		var engine = new VirtualizationEngine(25, 1, 100, 2);
		Assert.Throws<ArgumentOutOfRangeException>(() => engine.TotalColumnCount = -1);
		Assert.Throws<ArgumentOutOfRangeException>(() => engine.DefaultColumnWidth = double.NaN);
		Assert.Throws<ArgumentOutOfRangeException>(() => engine.SetColumnWidth(2, 100));
		Assert.Throws<ArgumentOutOfRangeException>(() => engine.SetColumnWidth(0, 0));
		Assert.Throws<ArgumentOutOfRangeException>(() => new VirtualizationEngine(25, 1, double.PositiveInfinity, 1));
	}

	/// <summary>
	/// 無効な水平スクロール入力は拒否します。
	/// </summary>
	[Fact]
	public void GetVisibleColumnRange_WithInvalidInput_ThrowsArgumentOutOfRangeException() {
		var engine = new VirtualizationEngine(25, 1, 100, 2);
		Assert.Throws<ArgumentOutOfRangeException>(() => engine.GetVisibleColumnRange(double.NaN, 100));
		Assert.Throws<ArgumentOutOfRangeException>(() => engine.GetVisibleColumnRange(0, double.PositiveInfinity));
		Assert.Throws<ArgumentOutOfRangeException>(() => engine.GetVisibleColumnRange(0, -1));
	}
}
