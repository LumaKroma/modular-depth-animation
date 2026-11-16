# Modular Depth Animation for SPS 0.1.0-beta.3.rc.1

最新版の VRCFury を対象とする公開プレリリースです。

## 対応・確認範囲
- Unity 2022.3.22f1 / VRCFury 1.1470.0（本体）/ VRChat SDK Base・Avatars 3.10.6。
- 配布 ZIP を別プロジェクトへ再導入し、65 項目の生成・保存再読込チェックと実 SDK preprocess 3 項目が成功。
- MANUKA の口への設定デモを用意し、ユーザー確認後に公開しています。VRChat へのアップロードは行っていません。
- 古い VRCFury、SPS for NDMF、VRCFury Stub は今回の対応対象外です。
- 旧 unitypackage から今回の Packages 形式への移行、他アバター、Poiyomi / SCSS の実描画、VRChat 実機の接触・深度追従は未検証です。

## 導入
同梱 ZIP はローカル導入用 UPM/VPM パッケージです。VCC 追加用の公開 VPM リポジトリではありません。プロジェクトをバックアップし、指定依存を導入後、Unity を閉じて ZIP を `Packages/com.lumakroma.modular-depth-animation-sps` に展開してください。既存の Assets 版と重複導入せず、旧版利用者はまず検証用コピーで移行してください。

SDK、VRCFury、MANUKA など第三者素材は同梱していません。配布 ZIP は再導入試験済みの同一バイト列を維持しています。ZIP 内 README の「再導入検証待ち」は作成時点の記録であり、本リリースノートの検証結果が最新です。

SHA256: `463a768daa455b1a867f3fdf0ae2ee162e1406c8156cc7a0e61ab95ffe36161b`
