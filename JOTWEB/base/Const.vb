Option Strict On
''' <summary>
''' 共通で利用する定数定義
''' </summary>
Public Module BaseDllConst

    ''' <summary>
    ''' システムコード グループ会社向け(GR)
    ''' </summary>
    Public Const C_SYSCODE_GR As String = "GR"
    ''' <summary>
    '''  項目値の分割用デリミター値
    ''' </summary>
    Public Const C_VALUE_SPLIT_DELIMITER As String = "|"
    ''' <summary>
    ''' URL関連
    ''' </summary>
    Public Class C_URL
        ''' <summary>
        ''' ログインURL
        ''' </summary>
        Public Const LOGIN As String = "~/M00000LOGON.aspx"
        ''' <summary>
        ''' アップロード処理用ハンドラーURL
        ''' </summary>
        Public Const UPLOAD_HANDLER As String = "~/xx.ashx"
        ''' <summary>
        ''' 採番取得用ハンドラーURL
        ''' </summary>
        Public Const NUMBER_ASSIGNMENT As String = "/office/GR/GRCO0103AUTONUM.ashx"
        ''' <summary>
        ''' HELP画面
        ''' </summary>
        Public Const HELP As String = "~/GR/GRCO0105HELP.aspx"
    End Class
    ''' <summary>
    ''' 他システム届先用接頭文字列
    ''' </summary>
    Public Class C_ANOTHER_SYSTEMS_DISTINATION_PREFIX
        ''' <summary>
        ''' JX(TG含む)
        ''' </summary>
        Public Const JX As String = "JX"
        ''' <summary>
        ''' COSMO
        ''' </summary>
        Public Const COSMO As String = "COSMO"

    End Class
    ''' <summary>
    ''' 言語設定
    ''' </summary>
    Public Class C_LANG
        ''' <summary>
        ''' 日本語
        ''' </summary>
        Public Const JA As String = "JA"
        ''' <summary>
        ''' 英語
        ''' </summary>
        Public Const EN As String = "EN"
    End Class
    ''' <summary>
    ''' 実行区分
    ''' </summary>
    Public Class C_RUNKBN
        ''' <summary>
        ''' オンライン
        ''' </summary>
        Public Const ONLINE As String = "ONLINE"
        ''' <summary>
        ''' バッチ
        ''' </summary>
        Public Const BATCH As String = "BATCH"
    End Class
    ''' <summary>
    ''' 削除フラグ
    ''' </summary>
    Public Class C_DELETE_FLG
        ''' <summary>
        ''' 削除
        ''' </summary>
        Public Const DELETE As String = "1"
        ''' <summary>
        ''' 生存
        ''' </summary>
        Public Const ALIVE As String = "0"
    End Class
    ''' <summary>
    ''' ロールの値
    ''' </summary>
    ''' <remarks></remarks>
    Public Class C_ROLE_VARIANT
        ''' <summary>
        ''' ユーザの表示会社権限
        ''' </summary>
        Public Const USER_COMP As String = "CAMP"
        ''' <summary>
        ''' ユーザの操作部署権限
        ''' </summary>
        Public Const USER_ORG As String = "ORG"
        ''' <summary>
        ''' ユーザの更新権限（各画面）
        ''' </summary>
        Public Const USER_PERTMIT As String = "MAP"
        ''' <summary>
        ''' ユーザのプロファイル変更権限（各ユーザ）
        ''' </summary>
        Public Const USER_PROFILE As String = "USER"
        ''' <summary>
        ''' サーバの表示会社権限
        ''' </summary>
        Public Const SERV_COMP As String = "SRVCAMP"
        ''' <summary>
        ''' APサーバにおける操作部署権限
        ''' </summary>
        Public Const SERV_ORG As String = "SRVORG"
        ''' <summary>
        ''' APサーバにおける更新権限（各画面）
        ''' </summary>
        Public Const SERV_PERTMIT As String = "SRVMAP"

    End Class
    ''' <summary>
    ''' 権限コード
    ''' </summary>
    Public Class C_PERMISSION
        ''' <summary>
        ''' 参照・更新
        ''' </summary>
        Public Const UPDATE As String = "2"
        ''' <summary>
        ''' 参照のみ
        ''' </summary>
        Public Const REFERLANCE As String = "1"
        ''' <summary>
        ''' 権限なし
        ''' </summary>
        Public Const INVALID As String = "0"
    End Class
    ''' <summary>
    ''' 一覧のOPERATION項目に設定するコード
    ''' </summary>
    Public Class C_LIST_OPERATION_CODE
        ''' <summary>
        ''' データなし
        ''' </summary>
        Public Const NODATA As String = ""
        ''' <summary>
        ''' 表示なし
        ''' </summary>
        Public Const NODISP As String = "＆nbsp;"
        ''' <summary>
        ''' 行選択
        ''' </summary>
        Public Const SELECTED As String = "★"
        ''' <summary>
        ''' 追加対象
        ''' </summary>
        Public Const INSERTING As String = "追加"
        ''' <summary>
        ''' 更新対象
        ''' </summary>
        Public Const UPDATING As String = "更新"
        ''' <summary>
        ''' エラー行対象
        ''' </summary>
        Public Const ERRORED As String = "エラー"
        ''' <summary>
        ''' 更新（警告あり）対象
        ''' </summary>
        Public Const WARNING As String = "警告"
    End Class
    ''' <summary>
    ''' 検査日に対応したアラート用コード
    ''' </summary>
    Public Class C_INSPECTIONALERT
        ''' <summary>
        ''' 赤丸（3日以内のタンク車）
        ''' </summary>
        Public Const ALERT_RED As String = "検査日まで後、3日以内のタンク車"
        ''' <summary>
        ''' 黄丸（4日～6日のタンク車）
        ''' </summary>
        Public Const ALERT_YELLOW As String = "検査日まで後、4日～6日のタンク車"
        ''' <summary>
        ''' 緑丸（7日以上のタンク車）
        ''' </summary>
        Public Const ALERT_GREEN As String = "検査日まで後、7日以上のタンク車"
    End Class
    ''' <summary>
    ''' 端末分類（OIS0001_TERM TERMCLASS）
    ''' </summary>
    Public Class C_TERMCLASS
        ''' <summary>
        ''' 端末（未使用）
        ''' </summary>
        Public Const CLIENT As String = "0"
        ''' <summary>
        ''' 拠点サーバ（未使用）
        ''' </summary>
        Public Const BASE As String = "1"
        ''' <summary>
        ''' 本社サーバ
        ''' </summary>
        Public Const HEAD As String = "2"
        ''' <summary>
        ''' クラウド（全社）サーバ
        ''' </summary>
        Public Const CLOUD As String = "9"
    End Class
    ''' <summary>
    ''' SQL共通条件文
    ''' </summary>
    Public Const C_SQL_COMMON_COND As String = "   and STYMD   <= @STYMD " _
                                             & "   and ENDYMD  >= @ENDYMD " _
                                             & "   and DELFLG  <> @DELFLG "
    ''' <summary>
    ''' デフォルトデータ検索値
    ''' </summary>
    Public Const C_DEFAULT_DATAKEY As String = "Default"

    ''' <summary>
    ''' 日付デフォルト値
    ''' </summary>
    Public Const C_DEFAULT_YMD As String = "1950/01/01"
    ''' <summary>
    ''' 日付最大値
    ''' </summary>
    Public Const C_MAX_YMD As String = "2099/12/31"

#Region "営業所"
    ''' <summary>
    ''' 東北支店
    ''' </summary>
    Public Const CONST_OFFICECODE_010401 As String = "010401"
    ''' <summary>
    ''' 仙台新港営業所
    ''' </summary>
    Public Const CONST_OFFICECODE_010402 As String = "010402"

    ''' <summary>
    ''' 関東支店
    ''' </summary>
    Public Const CONST_OFFICECODE_011401 As String = "011401"
    ''' <summary>
    ''' 五井営業所
    ''' </summary>
    Public Const CONST_OFFICECODE_011201 As String = "011201"
    ''' <summary>
    ''' 甲子営業所
    ''' </summary>
    Public Const CONST_OFFICECODE_011202 As String = "011202"
    ''' <summary>
    ''' 袖ヶ浦営業所
    ''' </summary>
    Public Const CONST_OFFICECODE_011203 As String = "011203"
    ''' <summary>
    ''' 根岸営業所
    ''' </summary>
    Public Const CONST_OFFICECODE_011402 As String = "011402"

    ''' <summary>
    ''' 中部支店
    ''' </summary>
    Public Const CONST_OFFICECODE_012301 As String = "012301"
    ''' <summary>
    ''' 四日市営業所
    ''' </summary>
    Public Const CONST_OFFICECODE_012401 As String = "012401"
    ''' <summary>
    ''' 三重塩浜営業所
    ''' </summary>
    Public Const CONST_OFFICECODE_012402 As String = "012402"

#End Region

#Region "受注情報"
    ''' <summary>
    ''' 積置
    ''' </summary>
    Public Const CONST_ORDERINFO_10 As String = "10"
    ''' <summary>
    ''' スポット
    ''' </summary>
    Public Const CONST_ORDERINFO_11 As String = "11"
    ''' <summary>
    ''' スポット含
    ''' </summary>
    Public Const CONST_ORDERINFO_12 As String = "12"
    ''' <summary>
    ''' 複数着地
    ''' </summary>
    Public Const CONST_ORDERINFO_13 As String = "13"

    ''' <summary>
    ''' タンク車数オーバー
    ''' </summary>
    Public Const CONST_ORDERINFO_ALERT_80 As String = "80"
    ''' <summary>
    ''' 積込エラー
    ''' </summary>
    Public Const CONST_ORDERINFO_ALERT_81 As String = "81"
    ''' <summary>
    ''' 検査間近有
    ''' </summary>
    Public Const CONST_ORDERINFO_ALERT_82 As String = "82"
    ''' <summary>
    ''' 前回油種確認
    ''' </summary>
    Public Const CONST_ORDERINFO_ALERT_83 As String = "83"
    ''' <summary>
    ''' 高速列車非対応
    ''' </summary>
    Public Const CONST_ORDERINFO_ALERT_84 As String = "84"
    ''' <summary>
    ''' タンク車重複
    ''' </summary>
    Public Const CONST_ORDERINFO_ALERT_85 As String = "85"
    ''' <summary>
    ''' 充填ポイント重複 
    ''' </summary>
    Public Const CONST_ORDERINFO_ALERT_86 As String = "86"
    ''' <summary>
    ''' 積込可能(油種毎)件数オーバー 
    ''' </summary>
    Public Const CONST_ORDERINFO_ALERT_87 As String = "87"
    ''' <summary>
    ''' 積込可能(油種大分類毎)件数オーバー 
    ''' </summary>
    Public Const CONST_ORDERINFO_ALERT_88 As String = "88"
    ''' <summary>
    ''' 積込可能(油種合計)件数オーバー 
    ''' </summary>
    Public Const CONST_ORDERINFO_ALERT_89 As String = "89"
    ''' <summary>
    ''' 荷受人(油槽所)受入油種NG
    ''' </summary>
    Public Const CONST_ORDERINFO_ALERT_90 As String = "90"
    ''' <summary>
    ''' 日付(積込日)エラー
    ''' </summary>
    Public Const CONST_ORDERINFO_ALERT_91 As String = "91"
    ''' <summary>
    ''' 日付(発日)エラー
    ''' </summary>
    Public Const CONST_ORDERINFO_ALERT_92 As String = "92"
    ''' <summary>
    ''' 日付(積車着日)エラー
    ''' </summary>
    Public Const CONST_ORDERINFO_ALERT_93 As String = "93"
    ''' <summary>
    ''' 日付(受入日)エラー
    ''' </summary>
    Public Const CONST_ORDERINFO_ALERT_94 As String = "94"
    ''' <summary>
    ''' 日付(空車着日)エラー
    ''' </summary>
    Public Const CONST_ORDERINFO_ALERT_95 As String = "95"
    ''' <summary>
    ''' タンク数量未設定
    ''' </summary>
    Public Const CONST_ORDERINFO_ALERT_96 As String = "96"

    ''' <summary>
    ''' 失注（荷主都合）
    ''' </summary>
    Public Const CONST_ORDERINFO_CANCEL_50 As String = "50"
    ''' <summary>
    ''' 失注（荷受人都合）
    ''' </summary>
    Public Const CONST_ORDERINFO_CANCEL_51 As String = "51"
    ''' <summary>
    ''' 災害
    ''' </summary>
    Public Const CONST_ORDERINFO_CANCEL_52 As String = "52"
    ''' <summary>
    ''' 事故
    ''' </summary>
    Public Const CONST_ORDERINFO_CANCEL_53 As String = "53"
#End Region

#Region "受注進行ステータス"
    ''' <summary>
    ''' 100:受注受付
    ''' </summary>
    Public Const CONST_ORDERSTATUS_100 As String = "100"
    ''' <summary>
    ''' 200:手配
    ''' </summary>
    Public Const CONST_ORDERSTATUS_200 As String = "200"
    ''' <summary>
    ''' 210:手配中（入換指示手配済）
    ''' </summary>
    Public Const CONST_ORDERSTATUS_210 As String = "210"
    ''' <summary>
    ''' 220:手配中（積込指示手配済）
    ''' </summary>
    Public Const CONST_ORDERSTATUS_220 As String = "220"
    ''' <summary>
    ''' 230:手配中（託送指示手配済）
    ''' </summary>
    Public Const CONST_ORDERSTATUS_230 As String = "230"
    ''' <summary>
    ''' 240:手配中（入換指示未手配）
    ''' </summary>
    Public Const CONST_ORDERSTATUS_240 As String = "240"
    ''' <summary>
    ''' 250:手配中（積込指示未手配）
    ''' </summary>
    Public Const CONST_ORDERSTATUS_250 As String = "250"
    ''' <summary>
    ''' 260;手配中（託送指示未手配）
    ''' </summary>
    Public Const CONST_ORDERSTATUS_260 As String = "260"
    ''' <summary>
    ''' 270:手配完了
    ''' </summary>
    Public Const CONST_ORDERSTATUS_270 As String = "270"
    ''' <summary>
    ''' 300:受注確定
    ''' </summary>
    Public Const CONST_ORDERSTATUS_300 As String = "300"
    ''' <summary>
    ''' 400:受入確認中
    ''' </summary>
    Public Const CONST_ORDERSTATUS_400 As String = "400"
    ''' <summary>
    ''' 500:検収中
    ''' </summary>
    Public Const CONST_ORDERSTATUS_500 As String = "500"
    ''' <summary>
    ''' 550:検収済
    ''' </summary>
    Public Const CONST_ORDERSTATUS_550 As String = "550"
    ''' <summary>
    ''' 600:費用確定
    ''' </summary>
    Public Const CONST_ORDERSTATUS_600 As String = "600"
    ''' <summary>
    ''' 700:経理未計上
    ''' </summary>
    Public Const CONST_ORDERSTATUS_700 As String = "700"
    ''' <summary>
    ''' 800:経理計上
    ''' </summary>
    Public Const CONST_ORDERSTATUS_800 As String = "800"
    ''' <summary>
    ''' 900:受注キャンセル
    ''' </summary>
    Public Const CONST_ORDERSTATUS_900 As String = "900"
#End Region

#Region "油種"
    ''' <summary>
    ''' 油種(ハイオク)
    ''' </summary>
    Public Const CONST_HTank As String = "1001"
    ''' <summary>
    ''' 油種(レギュラー)
    ''' </summary>
    Public Const CONST_RTank As String = "1101"
    ''' <summary>
    ''' 油種(灯油)
    ''' </summary>
    Public Const CONST_TTank As String = "1301"
    ''' <summary>
    ''' 油種(未添加灯油)
    ''' </summary>
    Public Const CONST_MTTank As String = "1302"
    ''' <summary>
    ''' 油種(軽油)
    ''' </summary>
    Public Const CONST_KTank1 As String = "1401"
    Public Const CONST_KTank2 As String = "1406"
    ''' <summary>
    ''' ３号軽油
    ''' </summary>
    Public Const CONST_K3Tank1 As String = "1404"
    Public Const CONST_K3Tank2 As String = "1405"
    ''' <summary>
    ''' ５号軽油
    ''' </summary>
    Public Const CONST_K5Tank As String = "1402"
    ''' <summary>
    ''' １０号軽油
    ''' </summary>
    Public Const CONST_K10Tank As String = "1403"
    ''' <summary>
    ''' ＬＳＡ
    ''' </summary>
    Public Const CONST_LTank1 As String = "2201"
    Public Const CONST_LTank2 As String = "2202"
    ''' <summary>
    ''' Ａ重油
    ''' </summary>
    Public Const CONST_ATank As String = "2101"
#End Region

#Region "海外向け"
    ''' <summary>
    ''' 集信日時の追加/更新時のデフォルト値
    ''' </summary>
    Public Const CONST_DEFAULT_RECEIVEYMD As String = "1950/01/01"
    ''' <summary>
    ''' フラグ用 有効値
    ''' </summary>
    Public Const CONST_FLAG_YES As String = "Y" '"1"
    ''' <summary>
    ''' フラグ用 無効値
    ''' </summary>
    Public Const CONST_FLAG_NO As String = "N" '"0"
    ''' <summary>
    ''' 申請：起点の承認ステップ("01")
    ''' </summary>
    Public Const C_APP_FIRSTSTEP As String = "01"
    ''' <summary>
    ''' 申請ステータス
    ''' </summary>
    ''' <remarks>このコード値群の追加修正をする場合は
    ''' 合わせてテーブル「FIXVALUE」のCLASS='APPROVAL'も併せて対応</remarks>
    Public Class C_APP_STATUS
        ''' <summary>
        ''' 入力中("00")
        ''' </summary>
        Public Const EDITING As String = "00"
        ''' <summary>
        ''' 未申請("01")
        ''' </summary>
        Public Const APPAGAIN As String = "01"
        ''' <summary>
        ''' 承認中("02")
        ''' </summary>
        Public Const APPLYING As String = "02"
        ''' <summary>
        ''' 訂正中("03")
        ''' </summary>
        Public Const REVISE As String = "03"
        ''' <summary>
        ''' 否認("09")
        ''' </summary>
        Public Const REJECT As String = "09"
        ''' <summary>
        ''' 承認("10")
        ''' </summary>
        Public Const APPROVED As String = "10"
        ''' <summary>
        ''' 終了("11")
        ''' </summary>
        Public Const COMPLETE As String = "11"
    End Class

    ''' <summary>
    ''' メッセージタイプ
    ''' </summary>
    Public Class C_NAEIW
        ''' <summary>
        ''' 正常メッセージ
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property NORMAL As String = "N"
        ''' <summary>
        ''' アブノーマルエラー
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property ABNORMAL As String = "A"
        ''' <summary>
        ''' エラー
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property [ERROR] As String = "E"
        ''' <summary>
        ''' 情報
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property INFORMATION As String = "I"
        ''' <summary>
        ''' 警告
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property WARNING As String = "W"
        ''' <summary>
        ''' 確認
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property QUESTION As String = "Q"
    End Class
#End Region

End Module 'End BaseDllConst