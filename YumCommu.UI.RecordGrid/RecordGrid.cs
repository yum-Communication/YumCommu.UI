using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;

// TESTプロジェクトから内部クラスにアクセスできるようにするための属性
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("TestRecordGrid")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("UITestRecordGrid")]

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace YumCommu.UI;

public sealed partial class RecordGrid: Control {

	private CanvasControl? _canvas;

	/// <summary>
	/// 縦スクロールバーの幅
	/// </summary>
	public double VerticalScrollBarWidth {
		get => (double)GetValue(VerticalScrollBarWidthProperty);
		set => SetValue(VerticalScrollBarWidthProperty, value);
	}

	/// <summary>
	/// 縦スクロールバーの幅（依存関係プロパティ）
	/// </summary>
	public static readonly DependencyProperty VerticalScrollBarWidthProperty =
		DependencyProperty.Register(
			nameof(VerticalScrollBarWidth),
			typeof(double),
			typeof(RecordGrid),
			new PropertyMetadata(20.0));

	/// <summary>
	/// 横スクロールバーの高さ
	/// </summary>
	public double HorizontalScrollBarHeight {
		get => (double)GetValue(HorizontalScrollBarHeightProperty);
		set => SetValue(HorizontalScrollBarHeightProperty, value);
	}


	/// <summary>
	/// 横スクロールバーの高さ（依存関係プロパティ）
	/// </summary>
	public static readonly DependencyProperty HorizontalScrollBarHeightProperty =
		DependencyProperty.Register(
			nameof(HorizontalScrollBarHeight),
			typeof(double),
			typeof(RecordGrid),
			new PropertyMetadata(20.0));


	/// <summary>
	/// コンストラクタ
	/// </summary>
	public RecordGrid() {
		DefaultStyleKey = typeof(RecordGrid);
	}

	/// <summary>
	/// テンプレート適用時の処理
	/// </summary>
	protected override void OnApplyTemplate() {
		base.OnApplyTemplate();

		if (_canvas is not null) {
			_canvas.Draw -= Canvas_Draw;
		}

		_canvas = GetTemplateChild("PART_Canvas") as CanvasControl;

		if (_canvas is not null) {
			_canvas.Draw += Canvas_Draw;
			_canvas.Invalidate();
		}
	}


	private void Canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args) {
		float width = (float)sender.ActualWidth;
		float height = (float)sender.ActualHeight;

		var center = new Vector2(
			width / 2,
			height / 2);

		float radius = Math.Min(width, height) / 2 - 8;

		//DrawDial(args.DrawingSession, center, radius);
		//DrawHands(args.DrawingSession, center, radius, DateTime.Now);
	}
}
