using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace YumCommu.UI;

public class RecordGridColumn {

	/// <summary>
	/// ヘッダに表示される見出し文字列
	/// </summary>
	public string Caption { get; set; } = string.Empty;


	/// <summary>
	/// 列の幅 (ピクセル)
	/// </summary>
	public double Width { get; set; } = 100;


	/// <summary>
	/// 値の型
	/// </summary>
	public Type DataType { get; set; } = typeof(string);


	/// <summary>
	/// 書式化文字列 (例: "C2", "yyyy/MM/dd")
	/// </summary>
	public string FormatString { get; set; } = string.Empty;


	/// <summary>
	/// 左寄せ・中央寄せ・右寄せの指定
	/// </summary>
	public TextAlignment Alignment { get; set; } = TextAlignment.Left;


	/// <summary>
	/// 背景色の指定 (null の場合はデフォルトの背景色が使用される)
	/// </summary>
	public Brush? Background { get; set; }


	/// <summary>
	/// 文字色の指定 (null の場合はデフォルトの文字色が使用される)
	/// </summary>
	public Brush? Foreground { get; set; }
}
