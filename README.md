## Release candidate: 0.1.0-beta.3.rc.1

Target: Unity 2022.3.22f1, full VRCFury 1.1470.0, VRChat SDK Base / Avatars 3.10.6. Older Fury compatibility is unverified and outside this candidate target. SPS for NDMF and VRCFury Stub are not substitutes. No third-party dependencies are bundled in the distribution ZIP.

The earlier isolated synthetic test passed generation, serialization and real SDK preprocess. Reinstallation of THIS distribution candidate in a separate project is pending. Real-avatar visuals and VRChat runtime behavior remain unverified.

Local installation: first install the exact dependencies with VCC/ALCOM; with Unity closed, extract the ZIP into Packages/com.lumakroma.modular-depth-animation-sps of a backed-up project, then open Unity. Do not install a second Assets copy alongside this package. This is a local candidate, not a published VPM repository.

Earlier notes below are historical; this target and verification status take precedence.

# Modular Depth Animation for SPS

VRCFury の SPS Socket に、距離に応じたアニメーションを追加するための小さな Unity Editor 拡張です。

`MDA Fury SPS Depth` コンポーネントで Action を設定すると、ビルド時に対象 SPS Socket の下へ Full Controller を自動生成します。

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
- ビルド機能を含む VRCFury 本体（VRCFury Stub / vfstub は不可）
- SPS Socket が設定済みのアバター

beta.2 で案内した SPS for NDMF 対応は、今回の対象外です。実 VRCFury のみ対応します。

## 導入

β版の間は、配布された `.unitypackage` を Unity にインポートしてください。

正式公開する場合は、VCC / ALCOM から追加できる VPM package として配布する予定です。

公開 beta.2 と現ソースではアセットの GUID が異なり、更新互換性は未確認です。beta.2 への単純上書きやソース版への置換は現時点では案内しません。既存 Prefab / Scene を持つ beta.1 系と beta.2 系を分けた更新検証が必要です。両系統の GUID を一括統一せず、次回は新規 / beta.1 向けにソース GUID、beta.2 向けに beta.2 の既存 GUID を保持する同一ソースの更新用パッケージを候補とし、実インポート確認後に確定します。

## 使い方

1. アバター、または導入用の子オブジェクトに `MDA Fury SPS Depth` を追加します。
2. 対象にしたい SPS Socket の種類を選びます。
3. 必要なら `Target Socket Object` に SPS Socket の GameObject を直接指定します。
4. `Add Action` で Action を追加します。
5. Action Type と各設定を入力します。
6. いつも通りアバターをビルドします。

`Target Socket Object` が空、または Missing の場合は、選択した Socket 種類から自動検出します。対象が見つからない場合、その Installer はスキップされます。

SPS for NDMF の Socket を明示指定した場合はエラーになります。別の Socket への自動切り替えは行いません。VRCFury のビルド機能がない場合も、有効な Installer があればビルドをエラーで停止します。

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

ビルド時に、対象 SPS Socket の下へ生成用 GameObject を作り、その下に Full Controller を設定します。

生成される AnimatorController アセットは `Assets/MDAGenerated` に作られます。

同じ Socket を複数の Installer で指定した場合も、各 Installer の生成物を保持します。

## 注意

- このツールは VRCFury 公式の機能ではありません。
- VRCFury 側の仕様変更により動かなくなる可能性があります。
- SPS Socket 標準の Depth Animation と完全に同じ機能ではありません。

## License

MIT License です。詳細は [LICENSE](LICENSE) を参照してください。
