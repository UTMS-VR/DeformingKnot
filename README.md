結び目描画ツール
====
結び目を自由に描画、移動、変形させることのできるツール。

# Oculus Questのコントローラーのボタンとpcのキーの対応

| コントローラー | pc |
| ---- | ---- |
| 右人差し指のトリガー | R |
| 右中指のトリガー | E |
| 左人差し指のトリガー | Q |
| 左中指のトリガー | W |
| Aボタン | A |
| Bボタン | B |
| Xボタン | X |
| Yボタン | Y |

# 使い方
`BasicDeform`、`ContiDeform`の2つの状態がある。現在の状態は画面左上に表示される。左人差し指のトリガーで状態を切り替える。

## BaiscDeform
| 操作 | ボタン | 詳細 |
| ---- | ---- | ---- |
| 描画 | 右人差し指のトリガー | 右手の動きに合わせて曲線を描画する。|
| 移動 | 右中指のトリガー | 右手に十分近い曲線を右手の動きに合わせて平行移動・回転させる。 |
| 選択 | Aボタン | 曲線を選択・選択解除する。選択された曲線は黄色になる。 |
| 切断 | Bボタン | 選択された曲線を右手に十分近いところで切断する。 |
| 結合 | Xボタン | 選択れている曲線が1つの場合は曲線が閉じているか否かを切り替える。<br>2つの場合は結合して1つの曲線にする。<br>いずれの場合も端点同士が十分に近くなくてはならない。 |
| 削除 | Yボタン | 選択されている曲線を削除する。|
| 元に戻す | 左中指のトリガー | 直前の操作をキャンセルする。 |

<!-- * 左人差し指のトリガー : 選択されている曲線を Bezier 曲線で整形する。整形し終えると選択が解除される。 -->

## ContiDeform
`KnotStateBase`、`KnotStateChoose1`、`KnotStateChoose2`、`KnotStatePull`、`KnotStateOptimize`の5つの状態がある。

基本的にAボタンで"決定"、Bボタンで"キャンセル"する。
### KnotStateBase
| ボタン | 結果 |
| ---- | ---- |
| Aボタン | `KnotStatePull`に移る。 |
| Bボタン | `KnotStateChoose1`に移る。 |
| 右人差し指<br>または右中指のトリガー | `KnotStateOptimize`に移る。 |
### KnotStateChoose1
| ボタン | 結果 |
| ---- | ---- |
| Aボタン | 1つ目の点を確定して`KnotStateChoose2`に移る。 |
| Bボタン | `KnotStateBase`に戻る。 |
### KnotStateChoose2
| ボタン | 結果 |
| ---- | ---- |
| Aボタン | 2つ目の点を確定して`KnotStateBase`に移る。 |
| Bボタン | `KnotStateBase`に戻る。 |
### KnotStatePull
右手の動きに合わせて、選択した2点の間の短い方の弧を変形させる。
| ボタン | 結果 |
| ---- | ---- |
| Aボタン | 変更を確定して`KnotStateBase`に移る。 |
| Bボタン | 変更を取り消して`KnotStateBase`に移る。 |
### KnotStateOptimize
右人差し指または右中指のトリガーを押している間、結び目のエネルギーの勾配に沿って自動で変形する。
| ボタン | 結果 |
| ---- | ---- |
| Aボタン | 変更を確定して`KnotStateBase`に移る。 |
| Bボタン | 変更を取り消して`KnotStateBase`に移る。 |
| 右人差し指のトリガー | 自動で変形する。慣性を加えたため収束が早いが、ガタつくことがある。 |
| 右中指のトリガー | 自動で変形する。慣性は無いため収束は遅いが、なめらか。 |

## pc上の場合のコントローラーの位置操作
pc上で動作させる場合は白いキューブがコントローラーを表している。
コントローラーとカメラが同じ位置にあるのでPlay開始時にはコントローラーは見えない。
z軸正方向にいくらか動かすと視界に入るようになる。
| pcのキー | 移動方向 |
| ---- | ---- |
| ; | x軸正方向 |
| k | x軸負方向 |
| o | y軸正方向 |
| l | y軸負方向 |
| i | z軸正方向 |
| , | z軸負方向 |
