## Release candidate: 0.1.0-beta.3.rc.1

Target: Unity 2022.3.22f1, full VRCFury 1.1470.0, VRChat SDK Base / Avatars 3.10.6. Older Fury compatibility is unverified and outside this candidate target. SPS for NDMF and VRCFury Stub are not substitutes. No third-party dependencies are bundled in the distribution ZIP.

The earlier isolated synthetic test passed generation, serialization and real SDK preprocess. Reinstallation of THIS distribution candidate in a separate project is pending. Real-avatar visuals and VRChat runtime behavior remain unverified.

Local installation: first install the exact dependencies with VCC/ALCOM; with Unity closed, extract the ZIP into Packages/com.lumakroma.modular-depth-animation-sps of a backed-up project, then open Unity. Do not install a second Assets copy alongside this package. This is a local candidate, not a published VPM repository.

Earlier notes below are historical; this target and verification status take precedence.

# 開発者向けメモ

Modular Depth Animation for SPS の保守用メモです。主に将来の自分向けに、各ファイルの責務、処理の流れ、機能追加時に触る場所をまとめています。

## VRCFury 正式版追従（2026-11-16）

2026-11-16 01:45:55 UTC に公式配布 `1.1470.0` を取得し、公開 SHA-256
`d9bfb257ab02a8179b1e16d91043919de76b7ef7de94c16189e2e266d34417b8` と照合しました。
公式 release: https://github.com/VRCFury/VRCFury/releases/tag/com.vrcfury.vrcfury/1.1470.0
公開日時は 2026-11-15 17:39:40 UTC です。`1.1460.1-beta.5` は beta 系として区別し、
今回の対応根拠には使用していません（GitHub の prerelease フラグだけでは beta を判別できません）。

- `FullController.injectParams` / `InjectParamEntry` に解決済み Socket を `sourceObject` として渡します。
  `sourceParam = __sps_depth_plug_lengths`、`targetParam = 生成した深度パラメーター` とし、
  従来の plug-length 単位を維持します。新 API が存在しない旧版では `injectSpsDepthParam` を使用します。
  新 API が存在するが構造が非互換の場合は、黙って旧経路へ切り替えずビルドを失敗させます。
- Socket 自動検出は `channelSettings.channels[].id` を読み取ります。移行前／旧版の `tags` も維持します。
  `tagsUpgradedToChannels` が true の場合、VRCFury が残す古い tags は無視し、削除・改名済みタグで誤選択しません。
  明示指定、既知パス、名前／oscId の優先順位は変更しません。
- `VF.Model.VRCFury`、`FullController.controllers` / `GuidController.objRef`、`globalParams`、
  `rootObjOverride` / `rootBindingsApplyToAvatar` は正式版ソース上で維持されています。
- SDK 順序は MDA `-10001` → VRCFury `VrcPreuploadHook` `-10000` のままです。
  VRCFury の FullController 処理が注入要求を登録し、Socket bake がそれを解決し、
  `HapticsAnimationRewrites` で未解決要求を検査します。
- Runtime の保存フィールド・enum・`.meta` と Action layer 生成ロジックは変更していません。

検証範囲: Unity 2022.3.22f1 の既存参照 DLL に対する外部 C# コンパイルと正式版ソース照合。
Tracker の管理対象 Editor が稼働中のため、1.1470.0 での実 Unity コンパイル／SDK preprocess／
Scene・Prefab 再読込／深度アニメーション動作は未検証です。旧版の過去試験を今回の実行結果とは扱いません。
次回は明示 Socket、channel のみの Socket、移行済み stale tags、旧 tags、複数 Installer、
全 Action と外部 FX float、保存設定／参照の保持を確認してください。

対応対象は実 VRCFury のみです。beta.2 で案内した SPS for NDMF 対応は今回の対象外で、ビルド機能のない VRCFury Stub (vfstub) も非対応です。

公開 beta.2 は現ソース / beta.1 系と GUID が異なり、更新互換性は未確認です。beta.2 への単純上書きやソース版への置換は案内せず、既存 Prefab / Scene を持つ beta.1 系と beta.2 系を分けて実インポート検証します。既存 `.meta` は維持し、二系統の GUID を一括統一しません。次回配布は新規 / beta.1 向けにソース GUID、beta.2 向けに beta.2 の既存 GUID を保持する同一ソースの更新用パッケージを候補とし、検証後に確定します。今回、移行コード・ユーザーアセット書換え・二重 Runtime shim は追加せず、パッケージの生成・公開も行いません。

## 全体の流れ

1. `SpsDepthRuntimeFullControllerInstaller` が Unity 上の設定を保持する。
2. `SpsDepthRuntimeFullControllerPreprocessor` が VRCFury のビルド処理直前に実行される。
3. `SpsDepthSocketResolver` が対象 SPS Socket を解決する。
4. `SpsDepthAnimatorControllerFactory` が AnimatorController アセットを生成する。
5. `SpsDepthActionLayerBuilder` が各 Depth Action を AnimatorController のレイヤーや BlendTree に変換する。
6. `SpsDepthVrcfuryFullControllerFactory` が生成オブジェクトに VRCFury Full Controller を設定する。
7. その後、VRCFury が通常のビルド処理で生成された Full Controller を読み込む。

## Runtime

### `Runtime/Components/SpsDepthRuntimeFullControllerInstaller.cs`

Unity 上でユーザーが追加するコンポーネントです。

ここに置くもの:

- ユーザーが保存する設定
- `DepthActionType`
- `SocketPreset`
- `DepthAction`
- `socketPreset`
- `targetSocketObject`
- `depthActions`

ここに置かないもの:

- Editor 専用処理
- VRCFury への反射アクセス
- AnimatorController 生成処理
- UI 表示処理

Action を増やす場合:

- `DepthActionType` に enum を追加する。
- 必要なら `DepthAction` に設定フィールドを追加する。
- Runtime 側はなるべくデータ定義だけに留める。

## Build

### `Editor/Build/SpsDepthRuntimeFullControllerPreprocessor.cs`

VRCSDK の preprocess 入口です。

責務:

- アバター配下の有効な Installer を探す。
- 有効な Installer があれば、生成前に実 VRCFury のビルド能力を確認する。
- 対象 SPS Socket を解決する。
- AnimatorController 生成を呼び出す。
- SPS Socket の下に生成用 GameObject を作る。
- Socket ごとの旧生成物掃除は同じビルド内で一度だけ行い、同じ Socket を使う全 Installer の生成物を保持する。
- バッチ内で一意の Installer 番号を生成 Object 名・depth parameter 名・controller path に渡す。
- 生成用 GameObject に VRCFury Full Controller を設定する。
- ビルドログや警告を出す。

注意:

- ここは処理の司令塔に留める。
- Action ごとの処理は書かない。
- VRCFury の細かい反射処理も書かない。

### `Editor/Build/Animator/SpsDepthAnimatorControllerFactory.cs`

AnimatorController アセット生成の入口です。

責務:

- avatar root、Installer、depth parameter などの入力検証
- 生成済み controller asset の削除
- 新しい AnimatorController asset の作成
- SPS depth parameter の追加
- 各 Action のレイヤー生成を `SpsDepthActionLayerBuilder` に委譲
- asset の保存と import
- `SpsDepthAnimatorControllerBuildResult` の返却

ここには Action 固有の処理を増やさないこと。

### `Editor/Build/Animator/SpsDepthActionLayerBuilder.cs`

Depth Action を AnimatorController のレイヤーへ変換する中心です。

責務:

- Action ごとの入力検証
- Action ごとのレイヤー生成
- BlendTree の生成
- BlendShape 用 AnimationClip の生成
- Animation Clip action のサンプリング
- Material Property action の shader property clip 生成
- Set FX Float action の Animator parameter curve 生成
- Object Toggle action の GameObject active curve 生成
- Scale action の Transform localScale curve 生成
- Component Toggle action の Component enabled curve 生成
- Poiyomi UV Tile / Flipbook Frame action の material property curve 生成
- SCSS Shader Inventory action の material property curve 生成
- binding path の収集

Action を増やす場合:

- `ValidateAction` に分岐を追加する。
- `CreateActionLayer` に分岐を追加する。
- `ValidateXAction` と `CreateXAction` のように、Action ごとに小さいメソッドへ分ける。
- Inspector 側にも UI を追加する。

Set FX Float action を増やす・変更する場合は、VRCFury Full Controller の parameter rewrite に注意すること。
外部の FX layer から読ませる parameter は `globalParams` に入れる必要があるため、`SpsDepthVrcfuryFullControllerFactory` への受け渡しも確認する。

#### Action Roadmap

実装済み:

- `BlendShape`
- `Animation Clip`
- `Material Property` (float / range)
- `Set FX Float`
- `Object Toggle`
- `Scale`
- `Material Swap`
- `Material Property` color/vector/ST 対応
- `Component Toggle`
- `Poiyomi UV Tile`
- `Poiyomi Flipbook Frame`
- `SCSS Shader Inventory`

今後追加する Action の優先度:

| Priority | Action | 理由 / 実装メモ |
| --- | --- | --- |
| P3 | `Reset Physbone` | VRCFury の `PhysboneResetService` が生成する一時 parameter に依存する。MDA の Full Controller 単体で再現するには VRCFury service 連携が必要。 |
| P3 | `Disable Hand Controls` / `Disable Visemes` / `Disable Blinking` | VRCFury の tracking / gesture inhibitor service が生成する parameter に依存する。Depth Action としての UX も特殊なため、直接実装は保留。 |
| P4 | `Flipbook Builder` / `Smooth Loop Builder` | VRCFury の高水準 builder 系。MDA が直接再実装するより、既存 `Animation Clip` や `Material Property` で代替できる場面が多い。 |
| P4 | `Enable SPS` / `World Drop` | Depth Action としての意味が薄い、またはビルド時 feature の性格が強い。原則として対象外候補。 |

優先度は固定ではない。実際のユーザー用途で「よく使う SPS Socket Action」が見えてきたら、P2 / P3 は入れ替えてよい。

### `Editor/Build/Animator/SpsDepthActionBuildContext.cs`

Action 生成時に渡す文脈オブジェクトです。

含むもの:

- Avatar root
- Installer
- 注入する SPS depth parameter 名
- 生成中の AnimatorController
- 診断ログ用の binding path 集合

### `Editor/Build/Animator/SpsDepthAnimatorControllerBuildResult.cs`

AnimatorController 生成結果の DTO です。

含むもの:

- controller 参照
- asset path
- binding path の概要
- controller の概要

### `Editor/Build/Vrcfury/SpsDepthVrcfuryFullControllerFactory.cs`

VRCFury の Full Controller を反射で作成する場所です。

責務:

- VRCFury の型を探す。
- `VF.Builder.VRCFuryBuilder, VRCFury-Editor-Avatars` と必要な `VF.*` Runtime 型を確認する。Runtime 型だけの vfstub は拒否する。Inspector / Socket 解決 / Build は同じ `IsAvailable` 判定を使う。
- VRCFury コンポーネントを生成用 GameObject に追加する。
- FullController feature を作成する。
- 生成した AnimatorController を FullController に設定する。
- 現行版では `injectParams` に対象 Socket と深度パラメーターを設定し、旧版では `injectSpsDepthParam` を設定する。

VRCFury 側の内部構造が変わって壊れた場合、まずこのファイルを見ること。

## Socket

### `Editor/Sockets/SpsDepthSocketResolver.cs`

対象 SPS Socket の解決を担当します。

責務:

- 明示指定された `targetSocketObject` を優先する。
- Missing 参照の場合は自動検出へフォールバックする。
- 明示指定されたオブジェクトに VRCFury SPS Socket コンポーネントがあるか検証する。
- SPS for NDMF Socket の明示指定はエラーにし、別 Socket へフォールバックしない。
- 既知パスから自動検出する。
- オブジェクト名と Socket の `channelSettings.channels[].id` / 移行前の `tags` / `name` / `oscId` の alias から自動検出する（SPS2 向け alias も維持）。
- `Found`、`FoundWithWarning`、`Skip`、`Error` を返す。

Socket preset を増やす場合:

- Runtime の `SocketPreset` enum に追加する。
- Localization に表示名を追加する。
- `PresetDefinitions` に alias を追加する。

## Inspector

### `Editor/Inspectors/SpsDepthRuntimeFullControllerInstallerEditor.cs`

Installer の custom inspector です。

責務:

- Target Socket UI の表示
- Target Socket Object の検証
- 自動検出ボタン
- Depth Action 一覧の表示
- Action の追加、削除、初期化
- Activation Distance の range slider
- Action ごとの設定 UI

Action を増やす場合:

- Action Type popup の選択肢を増やす。
- Action ごとの UI を追加する。
- `InitializeAction` で初期値を設定する。
- 必要なら `GetActionTitle` を更新する。
- 新しい表示文言は localization JSON に追加する。

### `Editor/Inspectors/SpsDepthComponentHeader.cs`

VRCFury 風のヘッダー表示を担当します。

責務:

- `MDA Fury` バッジの表示
- 言語切り替え UI の表示
- Unity 標準ヘッダーの右クリックやメニューを邪魔しないこと

ここにはビルド処理を書かないこと。

## Localization

### `Editor/Localization/SpsDepthLocalization.cs`

ローカライズのキー定義と読み込み処理です。

責務:

- 安定した key 定義
- UTF-8 JSON の読み込み
- 現在の言語判定
- `T`
- `Format`
- `Content`

大きな翻訳文は C# に直接書かない。文言は JSON に置く。

### `Editor/Localization/SpsDepthLocalization.en.json`

英語表示用の文言です。

### `Editor/Localization/SpsDepthLocalization.ja.json`

日本語表示用の文言です。

文言を増やす場合:

- `SpsDepthLocalization.Key` に key を追加する。
- `en.json` と `ja.json` の両方に同じ key を追加する。
- UI やログでは `SpsDepthLocalization.T`、`Format`、`Content` を使う。

## Utilities

### `Editor/Utilities/SpsDepthGeneratedNames.cs`

生成名を一元管理する場所です。

責務:

- 生成用 GameObject 名
- 生成 asset folder
- SPS depth parameter 名
- generated controller path
- Unity `GlobalObjectId` から安定した hash を作る処理
- hash だけで一意と仮定せず、バッチ内の Installer 番号を付ける処理（単一 Installer の既存呼出しは既定値 `0`）

生成名や生成先 path を他のファイルに直書きしないこと。

## 生成物

ビルド時に作られるもの:

- 対象 SPS Socket 下の生成用 GameObject
- `Assets/MDAGenerated` 以下の AnimatorController
- 生成用 GameObject 上の VRCFury Full Controller

生成用 GameObject 名は `SpsDepthGeneratedNames.GetGeneratedObjectName` で管理する。`GeneratedObjectName` の旧固定名と、その名前に `_` を付けた接頭辞は MDA 生成物用に予約し、次のビルドの掃除対象とする。

## 保守方針

- Runtime はできるだけデータ定義だけにする。
- Build の入口は Preprocessor に集約する。
- Action 固有の処理は `SpsDepthActionLayerBuilder` に置く。
- VRCFury への反射アクセスは `SpsDepthVrcfuryFullControllerFactory` に閉じ込める。
- ユーザー向け文言は localization JSON に置く。
- 生成名は `SpsDepthGeneratedNames` に置く。
- Socket が見つからないだけなら、原則としてビルド失敗ではなく警告とスキップにする。

## 既知の制限

- VRCFury SPS Socket の Depth Animation と完全に同等ではない。
- Full Controller 経由で実装できない機能は対象外。
- Meter 基準の挙動は実装しない。
- Unity の Object Picker 自体を SPS Socket 付き GameObject のみに絞ることはできない。選択後に検証して弾く。
- beta 版なので互換性は保証しない。
