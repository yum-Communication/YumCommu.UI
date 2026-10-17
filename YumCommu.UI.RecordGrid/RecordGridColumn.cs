using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace YumCommu.UI;

/// <summary>
/// RecordGrid の列定義。見出し・幅・配置・値の取得方法を保持する。
/// </summary>
public class RecordGridColumn: INotifyPropertyChanged {

	private string _caption = string.Empty;
	private double _width = 100;
	private double _minWidth = 10;
	private Type _dataType = typeof(string);
	private string _formatString = string.Empty;
	private TextAlignment _alignment = TextAlignment.Left;
	private Brush? _background;
	private Brush? _foreground;
	private Func<object, object?>? _valueGetter;

	/// <summary>
	/// プロパティが変更されたときに発生する。
	/// </summary>
	public event PropertyChangedEventHandler? PropertyChanged;

	/// <summary>
	/// ヘッダに表示される見出し文字列
	/// </summary>
	public string Caption {
		get => _caption;
		set => SetField(ref _caption, value);
	}

	/// <summary>
	/// 列の幅 (ピクセル)。初期値として設定し、ユーザーのドラッグ操作で更新される。
	/// </summary>
	public double Width {
		get => _width;
		set => SetField(ref _width, value);
	}

	/// <summary>
	/// 列の最小幅 (ピクセル)。ドラッグによる幅変更の下限として使用する。
	/// </summary>
	public double MinWidth {
		get => _minWidth;
		set => SetField(ref _minWidth, value);
	}

	/// <summary>
	/// 値の型
	/// </summary>
	public Type DataType {
		get => _dataType;
		set => SetField(ref _dataType, value);
	}

	/// <summary>
	/// 書式化文字列 (例: "C2", "yyyy/MM/dd")
	/// </summary>
	public string FormatString {
		get => _formatString;
		set => SetField(ref _formatString, value);
	}

	/// <summary>
	/// 左寄せ・中央寄せ・右寄せの指定
	/// </summary>
	public TextAlignment Alignment {
		get => _alignment;
		set => SetField(ref _alignment, value);
	}

	/// <summary>
	/// 背景色の指定 (null の場合はデフォルトの背景色が使用される)
	/// </summary>
	public Brush? Background {
		get => _background;
		set => SetField(ref _background, value);
	}

	/// <summary>
	/// 文字色の指定 (null の場合はデフォルトの文字色が使用される)
	/// </summary>
	public Brush? Foreground {
		get => _foreground;
		set => SetField(ref _foreground, value);
	}

	/// <summary>
	/// 行オブジェクトからこの列の値を取得するデリゲート (null の場合は値なし)
	/// </summary>
	public Func<object, object?>? ValueGetter {
		get => _valueGetter;
		set => SetField(ref _valueGetter, value);
	}

	/// <summary>
	/// 行オブジェクトからこの列の値を取得する。
	/// </summary>
	/// <param name="record">行オブジェクト</param>
	/// <returns>列の値。ValueGetter 未設定の場合は null</returns>
	public object? GetValue(object record) {
		ArgumentNullException.ThrowIfNull(record);
		return _valueGetter?.Invoke(record);
	}

	/// <summary>
	/// 値を書式化文字列で文字列に変換する。
	/// </summary>
	/// <param name="value">列の値</param>
	/// <returns>表示用文字列。値が null の場合は空文字列</returns>
	public string FormatValue(object? value) {
		if (value is null) {
			return string.Empty;
		}
		if (_formatString.Length > 0 && value is IFormattable formattable) {
			return formattable.ToString(_formatString, null);
		}
		return value.ToString() ?? string.Empty;
	}

	/// <summary>
	/// 型付きの値取得関数から列を生成する。
	/// </summary>
	/// <typeparam name="TRecord">行オブジェクトの型</typeparam>
	/// <param name="caption">見出し</param>
	/// <param name="getter">行オブジェクトから値を取得する関数</param>
	/// <param name="width">幅の初期値 (ピクセル)</param>
	/// <param name="alignment">文字の配置</param>
	/// <returns>生成した列</returns>
	public static RecordGridColumn Create<TRecord>(
		string caption,
		Func<TRecord, object?> getter,
		double width = 100,
		TextAlignment alignment = TextAlignment.Left) where TRecord : notnull {
		ArgumentNullException.ThrowIfNull(getter);
		return new RecordGridColumn {
			Caption = caption,
			Width = width,
			Alignment = alignment,
			ValueGetter = record => getter((TRecord)record),
		};
	}

	/// <summary>
	/// 値が変わった場合のみフィールドを更新し、変更を通知する。
	/// </summary>
	/// <typeparam name="T">フィールドの型</typeparam>
	/// <param name="field">更新するフィールド</param>
	/// <param name="value">新しい値</param>
	/// <param name="propertyName">プロパティ名</param>
	private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
		if (EqualityComparer<T>.Default.Equals(field, value)) {
			return;
		}
		field = value;
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
