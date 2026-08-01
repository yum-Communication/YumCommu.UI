# YumCommu.UI.WrapPanel

CSSのFlexBoxに近い動作を実現したい


## 配置する向き

`Direction`プロパティで指定。

また、`SubDirection`プロパティで折り返したアイテムの並ぶ方向を指定できる。

### `LeftToRight`

水平方向（左から右）

### `RightToLeft`

水平方向（右から左）

### `TopToDown`

垂直方向（上から下）

### `DownToTo`

垂直方向（下から上）


## 揃え方

`JustifyContent`プロパティで指定

### `Start`

先頭側に寄せる

### `End`

末尾側に寄せる

### `Center`

中央揃え

### `SpaceBetween`

両端揃え

### `SpaceBetween`

均等配置（** 各アイテムの左右 **に均等に余白が入る。つまりアイテム間の余白は左右／上下端の余白の二倍）


### 垂直方向揃え

`AlignItems`プロパティで指定

### `Start`

先頭側に寄せる

### `End`

末尾側に寄せる

### `Center`

中央寄せ


### 複数行の垂直方向揃え

`AlignContent`プロパティで指定。※指定する値は`JustifyContent`と同じ

