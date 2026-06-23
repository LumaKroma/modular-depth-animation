# 開発者向けメモ

Modular Depth Animation for SPS の保守用メモです。主に将来の自分向けに、各ファイルの責務、処理の流れ、機能追加時に触る場所をまとめています。

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
- 対象 SPS Socket を解決する。
- AnimatorController 生成を呼び出す。
- SPS Socket の下に生成用 GameObject を作る。
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
- binding path の収集

Action を増やす場合:

- `ValidateAction` に分岐を追加する。
- `CreateActionLayer` に分岐を追加する。
- `ValidateXAction` と `CreateXAction` のように、Action ごとに小さいメソッドへ分ける。
- Inspector 側にも UI を追加する。

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
- VRCFury コンポーネントを生成用 GameObject に追加する。
- FullController feature を作成する。
- 生成した AnimatorController を FullController に設定する。
- `injectSpsDepthParam` を設定する。

VRCFury 側の内部構造が変わって壊れた場合、まずこのファイルを見ること。

## Socket

### `Editor/Sockets/SpsDepthSocketResolver.cs`

対象 SPS Socket の解決を担当します。

責務:

- 明示指定された `targetSocketObject` を優先する。
- Missing 参照の場合は自動検出へフォールバックする。
- 明示指定されたオブジェクトに VRCFury SPS Socket コンポーネントがあるか検証する。
- 既知パスから自動検出する。
- オブジェクト名の alias から自動検出する。
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

生成名や生成先 path を他のファイルに直書きしないこと。

## 生成物

ビルド時に作られるもの:

- 対象 SPS Socket 下の生成用 GameObject
- `Assets/MDAGenerated` 以下の AnimatorController
- 生成用 GameObject 上の VRCFury Full Controller

生成用 GameObject 名は `SpsDepthGeneratedNames.GeneratedObjectName` で管理する。

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
