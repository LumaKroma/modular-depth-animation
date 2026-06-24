# Modular Depth Animation for SPS

VRCFury の SPS Socket に、距離に応じたアニメーションを追加するための小さな Unity Editor 拡張です。

`MDA Fury SPS Depth` コンポーネントで Action を設定すると、ビルド時に対象 SPS Socket の下へ VRCFury Full Controller を自動生成します。

## できること

SPS の挿入深度に合わせて、次のような値を変化させられます。

- BlendShape
- Animation Clip
- Material Property
- FX Float parameter
- Object / Component の ON/OFF
- Scale
- Material Swap
- Poiyomi UV Tile / Flipbook Frame
- SCSS Shader Inventory

## 必要なもの

- Unity 2022.3
- VRChat Avatar SDK
- VRCFury
- VRCFury SPS Socket が設定済みのアバター

## 導入

Unity Package Manager から、このリポジトリを Git package として追加してください。

```text
https://github.com/s1f10220274/modular-depth-animation.git
```

ローカルで編集している場合は、Package Manager の `Add package from disk...` から `package.json` を指定しても使えます。

## 使い方

1. アバター、または導入用の子オブジェクトに `MDA Fury SPS Depth` を追加します。
2. 対象にしたい SPS Socket の種類を選びます。
3. 必要なら `Target Socket Object` に SPS Socket の GameObject を直接指定します。
4. `Add Action` で Action を追加します。
5. Action Type と各設定を入力します。
6. いつも通りアバターをビルドします。

`Target Socket Object` が空、または Missing の場合は、選択した Socket 種類から自動検出します。対象が見つからない場合、その Installer はスキップされます。

Action は `Copy` / `コピー` ボタンで複製できます。似た設定を複数作るときに使ってください。

## Activation Distance

`Activation Distance` は SPS plug length 基準です。

- `0`: 入口
- 負の値: 挿入側、Near / On 側
- 正の値: 外側、Far / Off 側

基本的には、左側を Near / On、右側を Far / Off と考えると分かりやすいです。

## Action 一覧

| Action | 内容 |
| --- | --- |
| `BlendShape` | BlendShape の値を Far / Off から Near / On へ変化させます。 |
| `Animation Clip` | 1つの Animation Clip を深度に沿ってサンプリングします。先頭が Far / Off、末尾が Near / On です。 |
| `Material Property` | Renderer の float / range / color / vector / texture scale-offset を変化させます。 |
| `Set FX Float` | FX Animator の float parameter を変化させます。 |
| `Object Toggle` | GameObject の active 状態を切り替えます。 |
| `Scale` | GameObject の local scale を倍率で変化させます。 |
| `Material Swap` | Renderer の material slot を差し替えます。 |
| `Component Toggle` | `enabled` を持つ Component の ON/OFF を切り替えます。 |
| `Poiyomi UV Tile` | Poiyomi の UV Tile discard / dissolve property を切り替えます。 |
| `Poiyomi Flipbook Frame` | Poiyomi の flipbook frame を切り替えます。 |
| `SCSS Shader Inventory` | SCSS Shader Inventory の item slot を切り替えます。 |

## 生成されるもの

ビルド時に、対象 SPS Socket の下へ生成用 GameObject を作り、その下に VRCFury Full Controller を設定します。

生成される AnimatorController アセットは `Assets/MDAGenerated` に作られます。

## 注意

- このツールは VRCFury 公式の機能ではありません。
- VRCFury 側の仕様変更により動かなくなる可能性があります。
- VRCFury SPS Socket 標準の Depth Animation と完全に同じ機能ではありません。

## License

MIT License です。詳細は [LICENSE](LICENSE) を参照してください。
