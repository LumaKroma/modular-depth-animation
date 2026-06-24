# Modular Depth Animation for SPS

VRCFury の SPS Socket に、モジュール的に Depth Animation を追加するための小さな補助ツールです。

Unity 上で `MDA Fury SPS Depth` コンポーネントを追加し、BlendShape や Animation Clip を設定すると、ビルド時に対象 SPS Socket の下へ VRCFury Full Controller を自動生成します。

## 現在の状態

このツールは beta 版です。個人的な用途を前提に作っているため、将来の互換性や継続的な保守は保証しません。

## 必要なもの

- VRChat アバター用 Unity プロジェクト
- VRCFury
- VRCFury SPS Socket が導入済みのアバター

## 使い方

1. アバター、または導入用のモジュールオブジェクトに `MDA Fury SPS Depth` を追加します。
2. `Mouth`、`Pussy`、`Anal`、`Other` などから対象ソケットの種類を選びます。
3. `Target Socket Object` は空のままでも構いません。空の場合はビルド時に自動検出します。
4. 明示的に指定したい場合は、VRCFury SPS Socket コンポーネントが付いた GameObject を `Target Socket Object` に入れます。
5. `Depth Action` を追加します。
6. `BlendShape`、`Animation Clip`、`Material Property`、`Set FX Float`、`Object Toggle`、`Scale` のいずれかを設定します。
7. 通常通りアバターをビルドします。

`Target Socket Object` が空、または Missing の場合は、自動検出に切り替わります。対象ソケットが見つからない場合、その Installer はスキップされ、ビルド全体は失敗しません。

## Action

- `BlendShape`: 指定した BlendShape を Far / Off 側の値から Near / On 側の値へ変化させます。
- `Animation Clip`: 1つの Animation Clip を Depth に沿ってサンプリングします。先頭フレームが Far / Off、最終フレームが Near / On として扱われます。
- `Material Property`: Renderer の Material にある float / range 系 shader property を Far / Off 側の値から Near / On 側の値へ変化させます。
- `Set FX Float`: FX Animator の float parameter を Far / Off 側の値から Near / On 側の値へ変化させます。この parameter は VRCFury Full Controller のマージ時に global parameter として扱われます。
- `Object Toggle`: GameObject の active 状態を Far / Off 側から Near / On 側へ変化させます。
- `Scale`: GameObject の local scale を Far / Off 側の倍率から Near / On 側の倍率へ変化させます。

## Activation Distance

- 単位は SPS plug length 基準です。
- `Entrance` は `0` です。
- 挿入側、つまり Near 側は負の値です。
- 外側、つまり Far 側は正の値です。

## 生成されるもの

ビルド時に、対象 SPS Socket の下へ生成用 GameObject を作成し、その下に VRCFury Full Controller を設定します。

生成される AnimatorController アセットは `Assets/MDAGenerated` に作成されます。

## その他

- このツールは VRCFury の公式機能ではないので、VRCFury 側の内部構造変更により動かなくなる可能性があります。
- Full Controller 経由の実装のため、VRCFury SPS Socket の Depth Animation と完全に同じ機能ではありません。

## ライセンス

MIT License です。詳細は [LICENSE](LICENSE) を参照してください。
