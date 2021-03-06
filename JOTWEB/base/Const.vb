﻿Option Strict On
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
    ''' OT空回日報連携時の差分コード
    ''' </summary>
    Public Class C_OTKUUKAI_DIFF
        ''' <summary>
        ''' 新規作成
        ''' </summary>
        Public Const CONST_ITEMCD00 As String = "0"
        Public Const CONST_ITEMNM00 As String = "新規作成"
        ''' <summary>
        ''' 一致
        ''' </summary>
        Public Const CONST_ITEMCD01 As String = "1"
        Public Const CONST_ITEMNM01 As String = "一致"
        ''' <summary>
        ''' 油種変更
        ''' </summary>
        Public Const CONST_ITEMCD02 As String = "2"
        Public Const CONST_ITEMNM02 As String = "油種変更"
        ''' <summary>
        ''' 車番更新
        ''' </summary>
        Public Const CONST_ITEMCD03 As String = "3"
        Public Const CONST_ITEMNM03 As String = "車番更新"
        ''' <summary>
        ''' 削除
        ''' </summary>
        Public Const CONST_ITEMCD04 As String = "4"
        Public Const CONST_ITEMNM04 As String = "削除"
        ''' <summary>
        ''' 追加
        ''' </summary>
        Public Const CONST_ITEMCD05 As String = "5"
        Public Const CONST_ITEMNM05 As String = "追加"
        ''' <summary>
        ''' 前積
        ''' </summary>
        Public Const CONST_ITEMCD06 As String = "6"
        Public Const CONST_ITEMNM06 As String = "前積"
        ''' <summary>
        ''' OT輸送変更
        ''' </summary>
        Public Const CONST_ITEMCD07 As String = "7"
        Public Const CONST_ITEMNM07 As String = "OT輸送変更"
        ''' <summary>
        ''' JOINT変更
        ''' </summary>
        Public Const CONST_ITEMCD08 As String = "8"
        Public Const CONST_ITEMNM08 As String = "JOINT変更"
    End Class


#Region "検査日(交検・全検)アラート日数"
    ''' <summary>
    ''' 仙台新港営業所(異常対象日数(以内))
    ''' </summary>
    Public Const ALERT_010402_ABNOR As Integer = 1
    ''' <summary>
    ''' 仙台新港営業所(警告対象FROM日数(以上))
    ''' </summary>
    Public Const ALERT_010402_WANFM As Integer = 2
    ''' <summary>
    ''' 仙台新港営業所(警告対象TO日数(以内))
    ''' </summary>
    Public Const ALERT_010402_WANTO As Integer = 10
    ''' <summary>
    ''' 仙台新港営業所(正常対象日数)
    ''' </summary>
    Public Const ALERT_010402_NORML As Integer = 11

    ''' <summary>
    ''' 関東(五井・甲子・袖ヶ浦・根岸)営業所(異常対象日数(以内))
    ''' </summary>
    Public Const ALERT_011401_ABNOR As Integer = 1
    ''' <summary>
    ''' 関東(五井・甲子・袖ヶ浦・根岸)営業所(警告対象FROM日数(以上))
    ''' </summary>
    Public Const ALERT_011401_WANFM As Integer = 2
    ''' <summary>
    ''' 関東(五井・甲子・袖ヶ浦・根岸)営業所(警告対象TO日数(以内))
    ''' </summary>
    Public Const ALERT_011401_WANTO As Integer = 6
    ''' <summary>
    ''' 関東(五井・甲子・袖ヶ浦・根岸)営業所(正常対象日数)
    ''' </summary>
    Public Const ALERT_011401_NORML As Integer = 7

    ''' <summary>
    ''' 中部(四日市・三重塩浜)営業所(異常対象日数(以内))
    ''' </summary>
    Public Const ALERT_012301_ABNOR As Integer = 3
    ''' <summary>
    ''' 中部(四日市・三重塩浜)営業所(警告対象FROM日数(以上))
    ''' </summary>
    Public Const ALERT_012301_WANFM As Integer = 4
    ''' <summary>
    ''' 中部(四日市・三重塩浜)営業所(警告対象TO日数(以内))
    ''' </summary>
    Public Const ALERT_012301_WANTO As Integer = 6
    ''' <summary>
    ''' 中部(四日市・三重塩浜)営業所(正常対象日数)
    ''' </summary>
    Public Const ALERT_012301_NORML As Integer = 7
#End Region

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

#Region "会社コード"
    ''' <summary>
    ''' 日本石油輸送株式会社
    ''' </summary>
    Public Const CONST_CAMPCODE_01 As String = "01"
    ''' <summary>
    ''' 日本貨物鉄道株式会社
    ''' </summary>
    Public Const CONST_CAMPCODE_10 As String = "10"
    ''' <summary>
    ''' 日本オイルターミナル株式会社
    ''' </summary>
    Public Const CONST_CAMPCODE_11 As String = "11"
    ''' <summary>
    ''' 仙台臨海鉄道株式会社
    ''' </summary>
    Public Const CONST_CAMPCODE_12 As String = "12"
    ''' <summary>
    ''' 京葉臨海鉄道株式会社
    ''' </summary>
    Public Const CONST_CAMPCODE_13 As String = "13"
    ''' <summary>
    ''' 神奈川臨海鉄道株式会社
    ''' </summary>
    Public Const CONST_CAMPCODE_14 As String = "14"
    ''' <summary>
    ''' 名古屋臨海鉄道株式会社
    ''' </summary>
    Public Const CONST_CAMPCODE_15 As String = "15"
    ''' <summary>
    ''' ＥＮＥＯＳ株式会社
    ''' </summary>
    Public Const CONST_CAMPCODE_30 As String = "30"
    ''' <summary>
    ''' コスモ石油株式会社
    ''' </summary>
    Public Const CONST_CAMPCODE_31 As String = "31"
    ''' <summary>
    ''' 出光興産株式会社
    ''' </summary>
    Public Const CONST_CAMPCODE_32 As String = "32"
    ''' <summary>
    ''' ジャパンオイルネットワーク株式会社
    ''' </summary>
    Public Const CONST_CAMPCODE_33 As String = "33"
    ''' <summary>
    ''' 仙台臨海通運株式会社
    ''' </summary>
    Public Const CONST_CAMPCODE_50 As String = "50"
    ''' <summary>
    ''' 株式会社日新
    ''' </summary>
    Public Const CONST_CAMPCODE_51 As String = "51"
    ''' <summary>
    ''' コスモペトロサービス株式会社
    ''' </summary>
    Public Const CONST_CAMPCODE_52 As String = "52"
    ''' <summary>
    ''' 株式会社東伸工業
    ''' </summary>
    Public Const CONST_CAMPCODE_53 As String = "53"
    ''' <summary>
    ''' コスモテクノ四日市株式会社
    ''' </summary>
    Public Const CONST_CAMPCODE_54 As String = "54"
    ''' <summary>
    ''' 昭永工業株式会社
    ''' </summary>
    Public Const CONST_CAMPCODE_55 As String = "55"
    ''' <summary>
    ''' 日本通運株式会社
    ''' </summary>
    Public Const CONST_CAMPCODE_56 As String = "56"
    ''' <summary>
    ''' 富士臨海鉄道株式会社
    ''' </summary>
    Public Const CONST_CAMPCODE_57 As String = "57"
    ''' <summary>
    ''' 株式会社日陸
    ''' </summary>
    Public Const CONST_CAMPCODE_70 As String = "70"
    ''' <summary>
    ''' 在日米軍
    ''' </summary>
    Public Const CONST_CAMPCODE_71 As String = "71"
    ''' <summary>
    ''' 日本車両製造
    ''' </summary>
    Public Const CONST_CAMPCODE_80 As String = "80"
    ''' <summary>
    ''' 川崎重工業
    ''' </summary>
    Public Const CONST_CAMPCODE_81 As String = "81"
    ''' <summary>
    ''' 富士重工
    ''' </summary>
    Public Const CONST_CAMPCODE_82 As String = "82"
    ''' <summary>
    ''' 三菱重工業
    ''' </summary>
    Public Const CONST_CAMPCODE_83 As String = "83"
#End Region

#Region "管轄支店"
    ''' <summary>
    ''' 東北支店
    ''' </summary>
    Public Const CONST_BRANCHCODE_010401 As String = "010401"
    ''' <summary>
    ''' 関東支店
    ''' </summary>
    Public Const CONST_BRANCHCODE_011401 As String = "011401"
    ''' <summary>
    ''' 中部支店
    ''' </summary>
    Public Const CONST_BRANCHCODE_012301 As String = "012301"
    ''' <summary>
    ''' OT本社
    ''' </summary>
    Public Const CONST_BRANCHCODE_110001 As String = "110001"
    ''' <summary>
    ''' 株式会社日陸
    ''' </summary>
    Public Const CONST_BRANCHCODE_700001 As String = "700001"
    ''' <summary>
    ''' 在日米軍
    ''' </summary>
    Public Const CONST_BRANCHCODE_710001 As String = "710001"
#End Region

#Region "営業所"
    ''' <summary>
    ''' 情報システム部
    ''' </summary>
    Public Const CONST_OFFICECODE_010006 As String = "010006"
    ''' <summary>
    ''' 石油部
    ''' </summary>
    Public Const CONST_OFFICECODE_010007 As String = "010007"

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

#Region "日本石油輸送株式会社(JOT)"
    ''' <summary>
    ''' JOT本社
    ''' </summary>
    Public Const CONST_JOT_010001 As String = "010001"
    ''' <summary>
    ''' 内部監査室
    ''' </summary>
    Public Const CONST_JOT_010002 As String = "010002"
    ''' <summary>
    ''' 総務部
    ''' </summary>
    Public Const CONST_JOT_010003 As String = "010003"
    ''' <summary>
    ''' 人事部
    ''' </summary>
    Public Const CONST_JOT_010004 As String = "010004"
    ''' <summary>
    ''' 経理部
    ''' </summary>
    Public Const CONST_JOT_010005 As String = "010005"
    ''' <summary>
    ''' LNG部
    ''' </summary>
    Public Const CONST_JOT_010008 As String = "010008"
    ''' <summary>
    ''' 化成品部
    ''' </summary>
    Public Const CONST_JOT_010009 As String = "010009"
    ''' <summary>
    ''' 海外事業部
    ''' </summary>
    Public Const CONST_JOT_010010 As String = "010010"
    ''' <summary>
    ''' コンテナ部
    ''' </summary>
    Public Const CONST_JOT_010011 As String = "010011"
    ''' <summary>
    ''' グループ安全推進部
    ''' </summary>
    Public Const CONST_JOT_010012 As String = "010012"
    ''' <summary>
    ''' 北海道支店
    ''' </summary>
    Public Const CONST_JOT_010101 As String = "010101"
    ''' <summary>
    ''' 帯広営業所
    ''' </summary>
    Public Const CONST_JOT_010102 As String = "010102"
    ''' <summary>
    ''' 苫小牧営業所
    ''' </summary>
    Public Const CONST_JOT_010103 As String = "010103"
    ''' <summary>
    ''' 隅田川営業所
    ''' </summary>
    Public Const CONST_JOT_011301 As String = "011301"
    ''' <summary>
    ''' 大井営業所
    ''' </summary>
    Public Const CONST_JOT_011302 As String = "011302"
    ''' <summary>
    ''' メンテナンスセンター
    ''' </summary>
    Public Const CONST_JOT_011403 As String = "011403"
    ''' <summary>
    ''' 関東支店（千葉３営業所用）
    ''' </summary>
    Public Const CONST_JOT_011409 As String = "011409"
    ''' <summary>
    ''' 新潟事業所
    ''' </summary>
    Public Const CONST_JOT_011501 As String = "011501"
    ''' <summary>
    ''' 名古屋貨物営業所
    ''' </summary>
    Public Const CONST_JOT_012302 As String = "012302"
    ''' <summary>
    ''' 関西支店
    ''' </summary>
    Public Const CONST_JOT_012701 As String = "012701"
    ''' <summary>
    ''' 百済営業所
    ''' </summary>
    Public Const CONST_JOT_012702 As String = "012702"
    ''' <summary>
    ''' 水島営業所
    ''' </summary>
    Public Const CONST_JOT_013301 As String = "013301"
    ''' <summary>
    ''' 徳山営業所
    ''' </summary>
    Public Const CONST_JOT_013501 As String = "013501"
    ''' <summary>
    ''' 九州支店
    ''' </summary>
    Public Const CONST_JOT_014001 As String = "014001"
    ''' <summary>
    ''' シンガポール支店
    ''' </summary>
    Public Const CONST_JOT_019901 As String = "019901"
#End Region

#Region "貨物駅(営業所指定駅)"
    ''' <summary>
    ''' 仙台北港(仙台新港営業所)
    ''' </summary>
    Public Const CONST_STATION_243202 As String = "243202"
    ''' <summary>
    ''' 浜五井(五井営業所)
    ''' </summary>
    Public Const CONST_STATION_434103 As String = "434103"
    ''' <summary>
    ''' 甲子(甲子営業所)
    ''' </summary>
    Public Const CONST_STATION_434105 As String = "434105"
    ''' <summary>
    ''' 北袖(袖ヶ浦営業所)
    ''' </summary>
    Public Const CONST_STATION_434108 As String = "434108"
    ''' <summary>
    ''' 根岸(根岸営業所)
    ''' </summary>
    Public Const CONST_STATION_4532 As String = "4532"
    ''' <summary>
    ''' 四日市(四日市営業所)
    ''' </summary>
    Public Const CONST_STATION_5510 As String = "5510"
    ''' <summary>
    ''' 塩浜(三重塩浜営業所)
    ''' </summary>
    Public Const CONST_STATION_5512 As String = "5512"
#End Region

#Region "荷主"
    ''' <summary>
    ''' ENEOS
    ''' </summary>
    Public Const CONST_SHIPPERCODE_0005700010 As String = "0005700010"
    ''' <summary>
    ''' コスモ
    ''' </summary>
    Public Const CONST_SHIPPERCODE_0094000010 As String = "0094000010"
    ''' <summary>
    ''' 出光昭和シェル
    ''' </summary>
    Public Const CONST_SHIPPERCODE_0122700010 As String = "0122700010"
    ''' <summary>
    ''' キグナス石油
    ''' </summary>
    Public Const CONST_SHIPPERCODE_9999999999 As String = "9999999999"
#End Region

#Region "基地"
    ''' <summary>
    ''' ENEOS仙台
    ''' </summary>
    Public Const CONST_PLANTCODE_0401 As String = "0401"
    ''' <summary>
    ''' コスモ千葉
    ''' </summary>
    Public Const CONST_PLANTCODE_1201 As String = "1201"
    ''' <summary>
    ''' ENEOS千葉
    ''' </summary>
    Public Const CONST_PLANTCODE_1202 As String = "1202"
    ''' <summary>
    ''' 富士袖ヶ浦
    ''' </summary>
    Public Const CONST_PLANTCODE_1203 As String = "1203"
    ''' <summary>
    ''' ENEOS根岸
    ''' </summary>
    Public Const CONST_PLANTCODE_1401 As String = "1401"
    ''' <summary>
    ''' コスモ四日市
    ''' </summary>
    Public Const CONST_PLANTCODE_2401 As String = "2401"
    ''' <summary>
    ''' 出光昭和四日市
    ''' </summary>
    Public Const CONST_PLANTCODE_2402 As String = "2402"
#End Region

#Region "荷受人"
    ''' <summary>
    ''' JXTG北信油槽所
    ''' </summary>
    Public Const CONST_CONSIGNEECODE_10 As String = "10"
    ''' <summary>
    ''' JXTG甲府油槽所
    ''' </summary>
    Public Const CONST_CONSIGNEECODE_20 As String = "20"
    ''' <summary>
    ''' コウショウ高崎
    ''' </summary>
    Public Const CONST_CONSIGNEECODE_30 As String = "30"
    ''' <summary>
    ''' JONET松本
    ''' </summary>
    Public Const CONST_CONSIGNEECODE_40 As String = "40"
    ''' <summary>
    ''' OT盛岡
    ''' </summary>
    Public Const CONST_CONSIGNEECODE_51 As String = "51"
    ''' <summary>
    ''' OT郡山
    ''' </summary>
    Public Const CONST_CONSIGNEECODE_52 As String = "52"
    ''' <summary>
    ''' OT宇都宮
    ''' </summary>
    Public Const CONST_CONSIGNEECODE_53 As String = "53"
    ''' <summary>
    ''' OT高崎
    ''' </summary>
    Public Const CONST_CONSIGNEECODE_54 As String = "54"
    ''' <summary>
    ''' OT八王子
    ''' </summary>
    Public Const CONST_CONSIGNEECODE_55 As String = "55"
    ''' <summary>
    ''' OT松本
    ''' </summary>
    Public Const CONST_CONSIGNEECODE_56 As String = "56"
    ''' <summary>
    ''' 愛知機関区
    ''' </summary>
    Public Const CONST_CONSIGNEECODE_70 As String = "70"
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
    ''' 入線順重複
    ''' </summary>
    Public Const CONST_ORDERINFO_ALERT_97 As String = "97"
    ''' <summary>
    ''' 前回揮発油
    ''' </summary>
    Public Const CONST_ORDERINFO_ALERT_98 As String = "98"
    ''' <summary>
    ''' 前回黒油
    ''' </summary>
    Public Const CONST_ORDERINFO_ALERT_99 As String = "99"
    ''' <summary>
    ''' 発送順重複
    ''' </summary>
    Public Const CONST_ORDERINFO_ALERT_100 As String = "100"
    ''' <summary>
    ''' タンク車状態未到着
    ''' </summary>
    Public Const CONST_ORDERINFO_ALERT_101 As String = "101"
    ''' <summary>
    ''' タンク車所属外
    ''' </summary>
    Public Const CONST_ORDERINFO_ALERT_102 As String = "102"
    ''' <summary>
    ''' 列車未設定
    ''' </summary>
    Public Const CONST_ORDERINFO_ALERT_103 As String = "103"
    ''' <summary>
    ''' 前回灯軽油
    ''' </summary>
    Public Const CONST_ORDERINFO_ALERT_104 As String = "104"
    ''' <summary>
    ''' 積込日エラー((一覧)積込日と(予定)発日チェック用)
    ''' </summary>
    Public Const CONST_ORDERINFO_ALERT_105 As String = "105"
    ''' <summary>
    ''' 品種出荷期間エラー
    ''' </summary>
    Public Const CONST_ORDERINFO_ALERT_106 As String = "106"
    ''' <summary>
    ''' 受注オーダー中
    ''' </summary>
    Public Const CONST_ORDERINFO_ALERT_107 As String = "107"
    ''' <summary>
    ''' 回送オーダー中
    ''' </summary>
    Public Const CONST_ORDERINFO_ALERT_108 As String = "108"
    ''' <summary>
    ''' 日付(発駅戻り日)エラー
    ''' </summary>
    Public Const CONST_ORDERINFO_ALERT_109 As String = "109"
    ''' <summary>
    ''' 発送日エラー
    ''' </summary>
    Public Const CONST_ORDERINFO_ALERT_110 As String = "110"

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
    ''' 205:手配中（千葉(根岸を除く)以外）
    ''' </summary>
    Public Const CONST_ORDERSTATUS_205 As String = "205"
    ''' <summary>
    ''' 210:手配中（入換指示入力済）
    ''' </summary>
    Public Const CONST_ORDERSTATUS_210 As String = "210"
    ''' <summary>
    ''' 220:手配中（積込指示入力済）
    ''' </summary>
    Public Const CONST_ORDERSTATUS_220 As String = "220"
    ''' <summary>
    ''' 230:手配中（託送指示手配済）
    ''' </summary>
    Public Const CONST_ORDERSTATUS_230 As String = "230"
    ''' <summary>
    ''' 240:手配中（入換指示未入力）
    ''' </summary>
    Public Const CONST_ORDERSTATUS_240 As String = "240"
    ''' <summary>
    ''' 250:手配中（積込指示未入力）
    ''' </summary>
    Public Const CONST_ORDERSTATUS_250 As String = "250"
    ''' <summary>
    ''' 260:手配中（託送指示未手配）入換積込手配連絡（未手配）
    ''' </summary>
    Public Const CONST_ORDERSTATUS_260 As String = "260"
    ''' <summary>
    ''' 270:手配中（入換積込指示手配済）
    ''' </summary>
    Public Const CONST_ORDERSTATUS_270 As String = "270"
    ''' <summary>
    ''' 280:手配中（託送指示未手配）入換積込手配連絡（手配・結果受理）
    ''' </summary>
    Public Const CONST_ORDERSTATUS_280 As String = "280"
    ''' <summary>
    ''' 290:手配中（入換積込未連絡）
    ''' </summary>
    Public Const CONST_ORDERSTATUS_290 As String = "290"
    ''' <summary>
    ''' 300:手配中（入換積込未確認）
    ''' </summary>
    Public Const CONST_ORDERSTATUS_300 As String = "300"
    ''' <summary>
    ''' 305:手配完了（託送未）
    ''' </summary>
    Public Const CONST_ORDERSTATUS_305 As String = "305"
    ''' <summary>
    ''' 310:手配完了
    ''' </summary>
    Public Const CONST_ORDERSTATUS_310 As String = "310"
    ''' <summary>
    ''' 320:受注確定
    ''' </summary>
    Public Const CONST_ORDERSTATUS_320 As String = "320"
    ''' <summary>
    ''' 350:受注確定((実績)発日設定済み)
    ''' </summary>
    Public Const CONST_ORDERSTATUS_350 As String = "350"
    ''' <summary>
    ''' 400:受入確認中
    ''' </summary>
    Public Const CONST_ORDERSTATUS_400 As String = "400"
    ''' <summary>
    ''' 450:受入確認中((実績)受入日設定済み)
    ''' </summary>
    Public Const CONST_ORDERSTATUS_450 As String = "450"
    ''' <summary>
    ''' 500:輸送完了
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

#Region "回送進行ステータス"
    ''' <summary>
    ''' 100:回送受付
    ''' </summary>
    Public Const CONST_KAISOUSTATUS_100 As String = "100"
    ''' <summary>
    ''' 200:手配
    ''' </summary>
    Public Const CONST_KAISOUSTATUS_200 As String = "200"
    ''' <summary>
    ''' 210:手配中
    ''' </summary>
    Public Const CONST_KAISOUSTATUS_210 As String = "210"
    ''' <summary>
    ''' 250:手配完了
    ''' </summary>
    Public Const CONST_KAISOUSTATUS_250 As String = "250"
    ''' <summary>
    ''' 300:回送確定
    ''' </summary>
    Public Const CONST_KAISOUSTATUS_300 As String = "300"
    ''' <summary>
    ''' 350:回送確定(発日入力済み)
    ''' </summary>
    Public Const CONST_KAISOUSTATUS_350 As String = "350"
    ''' <summary>
    ''' 400:受入確認中
    ''' </summary>
    Public Const CONST_KAISOUSTATUS_400 As String = "400"
    ''' <summary>
    ''' 450:受入確認中(受入日入力済み)
    ''' </summary>
    Public Const CONST_KAISOUSTATUS_450 As String = "450"
    ''' <summary>
    ''' 500:検収中
    ''' </summary>
    Public Const CONST_KAISOUSTATUS_500 As String = "500"
    ''' <summary>
    ''' 550:検収済み
    ''' </summary>
    Public Const CONST_KAISOUSTATUS_550 As String = "550"
    ''' <summary>
    ''' 600:費用確定
    ''' </summary>
    Public Const CONST_KAISOUSTATUS_600 As String = "600"
    ''' <summary>
    ''' 700:経理未計上
    ''' </summary>
    Public Const CONST_KAISOUSTATUS_700 As String = "700"
    ''' <summary>
    ''' 800:経理計上
    ''' </summary>
    Public Const CONST_KAISOUSTATUS_800 As String = "800"
    ''' <summary>
    ''' 900:回送キャンセル
    ''' </summary>
    Public Const CONST_KAISOUSTATUS_900 As String = "900"
#End Region

#Region "目的"
    ''' <summary>
    ''' 20:修理
    ''' </summary>
    Public Const CONST_OBJECTCODE_20 As String = "20"
    ''' <summary>
    ''' 21:MC
    ''' </summary>
    Public Const CONST_OBJECTCODE_21 As String = "21"
    ''' <summary>
    ''' 22:交検
    ''' </summary>
    Public Const CONST_OBJECTCODE_22 As String = "22"
    ''' <summary>
    ''' 23:全検
    ''' </summary>
    Public Const CONST_OBJECTCODE_23 As String = "23"
    ''' <summary>
    ''' 24:疎開留置
    ''' </summary>
    Public Const CONST_OBJECTCODE_24 As String = "24"
    ''' <summary>
    ''' 25:移動
    ''' </summary>
    Public Const CONST_OBJECTCODE_25 As String = "25"

#End Region

#Region "輸送形態"
    ''' <summary>
    ''' C:請負
    ''' </summary>
    Public Const CONST_TRKBN_C As String = "C"
    ''' <summary>
    ''' O:OT輸送
    ''' </summary>
    Public Const CONST_TRKBN_O As String = "O"
    ''' <summary>
    ''' M:請負OT混載
    ''' </summary>
    Public Const CONST_TRKBN_M As String = "M"
    ''' <summary>
    ''' F:回送
    ''' </summary>
    Public Const CONST_TRKBN_F As String = "F"
#End Region

#Region "回送パターン"
    ''' <summary>
    ''' 01:修理-JOT負担発払
    ''' </summary>
    Public Const CONST_KAISOUPATTERN_01 As String = "修理-JOT負担発払"
    ''' <summary>
    ''' 02:修理-JOT負担着払
    ''' </summary>
    Public Const CONST_KAISOUPATTERN_02 As String = "修理-JOT負担着払"
    ''' <summary>
    ''' 03:修理-他社負担
    ''' </summary>
    Public Const CONST_KAISOUPATTERN_03 As String = "修理-他社負担"
    ''' <summary>
    ''' 04:ＭＣ-JOT負担発払
    ''' </summary>
    Public Const CONST_KAISOUPATTERN_04 As String = "ＭＣ-JOT負担発払"
    ''' <summary>
    ''' 05:ＭＣ-JOT負担着払
    ''' </summary>
    Public Const CONST_KAISOUPATTERN_05 As String = "ＭＣ-JOT負担着払"
    ''' <summary>
    ''' 06:ＭＣ-他社負担
    ''' </summary>
    Public Const CONST_KAISOUPATTERN_06 As String = "ＭＣ-他社負担"
    ''' <summary>
    ''' 07:交検-他社負担
    ''' </summary>
    Public Const CONST_KAISOUPATTERN_07 As String = "交検-他社負担"
    ''' <summary>
    ''' 08:全検-他社負担
    ''' </summary>
    Public Const CONST_KAISOUPATTERN_08 As String = "全検-他社負担"
    ''' <summary>
    ''' 09:疎開留置-JOT負担発払
    ''' </summary>
    Public Const CONST_KAISOUPATTERN_09 As String = "疎開留置-JOT負担発払"
    ''' <summary>
    ''' 10:疎開留置-JOT負担着払
    ''' </summary>
    Public Const CONST_KAISOUPATTERN_10 As String = "疎開留置-JOT負担着払"
    ''' <summary>
    ''' 11:疎開留置-他社負担
    ''' </summary>
    Public Const CONST_KAISOUPATTERN_11 As String = "疎開留置-他社負担"
    ''' <summary>
    ''' 12:移動-JOT負担発払
    ''' </summary>
    Public Const CONST_KAISOUPATTERN_12 As String = "移動-JOT負担発払"
    ''' <summary>
    ''' 13:移動-JOT負担着払
    ''' </summary>
    Public Const CONST_KAISOUPATTERN_13 As String = "移動-JOT負担着払"
    ''' <summary>
    ''' 14:移動-他社負担
    ''' </summary>
    Public Const CONST_KAISOUPATTERN_14 As String = "移動-他社負担"

#End Region

#Region "タンク車状態コード"
    ''' <summary>
    ''' 01:発送
    ''' </summary>
    Public Const CONST_TANKSTATUS_01 As String = "1"
    ''' <summary>
    ''' 02:到着予定
    ''' </summary>
    Public Const CONST_TANKSTATUS_02 As String = "2"
    ''' <summary>
    ''' 03:到着
    ''' </summary>
    Public Const CONST_TANKSTATUS_03 As String = "3"
#End Region

#Region "タンク車状況コード"
    ''' <summary>
    ''' 01:残車
    ''' </summary>
    Public Const CONST_TANKSITUATION_01 As String = "1"
    ''' <summary>
    ''' 02:輸送中
    ''' </summary>
    Public Const CONST_TANKSITUATION_02 As String = "2"
    ''' <summary>
    ''' 03:回送中（交検）
    ''' </summary>
    Public Const CONST_TANKSITUATION_03 As String = "3"
    ''' <summary>
    ''' 04:回送中（全検）
    ''' </summary>
    Public Const CONST_TANKSITUATION_04 As String = "4"
    ''' <summary>
    ''' 05:回送中（修理）
    ''' </summary>
    Public Const CONST_TANKSITUATION_05 As String = "5"
    ''' <summary>
    ''' 06:回送中（疎開留置）
    ''' </summary>
    Public Const CONST_TANKSITUATION_06 As String = "6"
    ''' <summary>
    ''' 07:回送中（ＭＣ）
    ''' </summary>
    Public Const CONST_TANKSITUATION_07 As String = "7"
    ''' <summary>
    ''' 08:回送中(移動)
    ''' </summary>
    Public Const CONST_TANKSITUATION_08 As String = "8"

    ''' <summary>
    ''' 11:修理中(回送(片道))
    ''' </summary>
    Public Const CONST_TANKSITUATION_11 As String = "11"
    ''' <summary>
    ''' 12:MC中(回送(片道))
    ''' </summary>
    Public Const CONST_TANKSITUATION_12 As String = "12"
    ''' <summary>
    ''' 13:交検中(回送(片道))
    ''' </summary>
    Public Const CONST_TANKSITUATION_13 As String = "13"
    ''' <summary>
    ''' 14:全検中(回送(片道))
    ''' </summary>
    Public Const CONST_TANKSITUATION_14 As String = "14"
    ''' <summary>
    ''' 15:留置中(回送(片道))
    ''' </summary>
    Public Const CONST_TANKSITUATION_15 As String = "15"

    ''' <summary>
    ''' 20:未卸(受注用)
    ''' </summary>
    Public Const CONST_TANKSITUATION_20 As String = "20"
    ''' <summary>
    ''' 21:交検中(仙台(受注用))
    ''' </summary>
    Public Const CONST_TANKSITUATION_21 As String = "21"
    ''' <summary>
    ''' 22:留置中(仙台(受注用))
    ''' </summary>
    Public Const CONST_TANKSITUATION_22 As String = "22"

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
    Public Const CONST_ATank2 As String = "2102"
    Public Const CONST_ATank3 As String = "2103"
#End Region

#Region "OT油種"
    ''' <summary>
    ''' OT油種(ハイオク)
    ''' </summary>
    Public Const CONST_OTHTank As String = "1100"
    ''' <summary>
    ''' OT油種(ハイオク)※根岸－郡山限定
    ''' </summary>
    Public Const CONST_OTHTank_1900 As String = "1900"
    ''' <summary>
    ''' OT油種(ハイオク)※根岸－宇都宮限定
    ''' </summary>
    Public Const CONST_OTHTank_1200 As String = "1200"
    ''' <summary>
    ''' OT油種(ハイオク)※根岸－南松本限定
    ''' </summary>
    Public Const CONST_OTHTank_1400 As String = "1400"
    ''' <summary>
    ''' OT油種(レギュラー)
    ''' </summary>
    Public Const CONST_OTRTank As String = "2100"
    ''' <summary>
    ''' OT油種(灯油)
    ''' </summary>
    Public Const CONST_OTTTank As String = "3100"
    ''' <summary>
    ''' OT油種(軽油)
    ''' </summary>
    Public Const CONST_OTKTank As String = "4100"
    ''' <summary>
    ''' OT３号軽油
    ''' </summary>
    Public Const CONST_OTK3Tank As String = "4930"
    ''' <summary>
    ''' OTＬＳＡ
    ''' </summary>
    Public Const CONST_OTLTank As String = "5100"
    ''' <summary>
    ''' OTＡ重油
    ''' </summary>
    Public Const CONST_OTATank As String = "5200"
#End Region

#Region "油種名(受発注用)"
    Public Const CONST_1001A As String = "ハイオク"
    Public Const CONST_1101A As String = "レギュラー"

    Public Const CONST_1301A As String = "灯油"
    Public Const CONST_1302A As String = "未添加灯油"

    Public Const CONST_1401A As String = "軽油"
    Public Const CONST_1401D As String = "軽油１号"
    Public Const CONST_1401E As String = "軽油２号"
    Public Const CONST_1402B As String = "軽油５"
    Public Const CONST_1403C As String = "軽油１０"
    Public Const CONST_1404A As String = "３号軽油"
    Public Const CONST_1404B As String = "３号軽油２０"
    Public Const CONST_1404C As String = "３号軽油５０"
    Public Const CONST_1404D As String = "３号軽油TCH"
    Public Const CONST_1404E As String = "寒冷軽油"

    Public Const CONST_1501A As String = "JP-8"

    Public Const CONST_2101A As String = "Ａ重油"
    Public Const CONST_2101B As String = "0.5A重油"
    Public Const CONST_2101C As String = "LTA"
    Public Const CONST_2101D As String = "ＬＳＡ－５"
    Public Const CONST_2101E As String = "AFOーSP"
    Public Const CONST_2101F As String = "ＬＳＡ－５（山岳）"
    Public Const CONST_2101G As String = "AFOーSP（山岳）"
    Public Const CONST_2101H As String = "AFOーブレンド（山岳）"

    Public Const CONST_2201A As String = "ＬＳＡ"
    Public Const CONST_2201B As String = "0.1A重油"
    Public Const CONST_2201C As String = "ＬＳＡ－１"
    Public Const CONST_2201D As String = "ＬＳＡーブレンド"
    Public Const CONST_2201E As String = "ＬＳＡ－１（山岳）"
    Public Const CONST_2201F As String = "ＬＳＡーブレンド（山岳）"
#End Region

#Region "油種(コスモ石油)"
    ''' <summary>
    ''' ハイオク
    ''' </summary>
    Public Const CONST_COSMO_HIG As String = "HI-G"
    ''' <summary>
    ''' レギュラー
    ''' </summary>
    Public Const CONST_COSMO_REG As String = "RE-G"
    ''' <summary>
    ''' 灯油
    ''' </summary>
    Public Const CONST_COSMO_WKO As String = "WKO"
    ''' <summary>
    ''' 軽油
    ''' </summary>
    Public Const CONST_COSMO_DGO As String = "DGO"
    ''' <summary>
    ''' 軽油５
    ''' </summary>
    Public Const CONST_COSMO_DGO5 As String = "DGO.5"
    ''' <summary>
    ''' 軽油１０
    ''' </summary>
    Public Const CONST_COSMO_DGO10 As String = "DGO.10"
    ''' <summary>
    ''' ３号軽油
    ''' </summary>
    Public Const CONST_COSMO_DGO3 As String = "DGO.3"
    ''' <summary>
    ''' Ａ重油
    ''' </summary>
    Public Const CONST_COSMO_AFO As String = "AFO"
    ''' <summary>
    ''' Ａ重油SP
    ''' </summary>
    Public Const CONST_COSMO_AFOSP As String = "A-SP"
    ''' <summary>
    ''' Ａ重油ブレンド
    ''' </summary>
    Public Const CONST_COSMO_AFOBU As String = "A(ブ"
    ''' <summary>
    ''' ＬＳＡ
    ''' </summary>
    Public Const CONST_COSMO_LSA As String = "LA-1"
    ''' <summary>
    ''' ＬＳＡブレンド
    ''' </summary>
    Public Const CONST_COSMO_LSABU As String = "LAブ"
#End Region

#Region "列車(京葉臨海)"
    ''' <summary>
    ''' 列車番号(入線 回線1) 五井営業所
    ''' </summary>
    Public Const CONST_RTRAIN_I01_201_011201 As String = "201"
    ''' <summary>
    ''' 列車番号(出線 回線1) 五井営業所
    ''' </summary>
    Public Const CONST_RTRAIN_O01_204_011201 As String = "204"
    ''' <summary>
    ''' 列車番号(入線 回線2) 五井営業所
    ''' </summary>
    Public Const CONST_RTRAIN_I02_203_011201 As String = "203"
    ''' <summary>
    ''' 列車番号(出線 回線2) 五井営業所
    ''' </summary>
    Public Const CONST_RTRAIN_O02_206_011201 As String = "206"
    ''' <summary>
    ''' 列車番号(入線 回線3) 五井営業所
    ''' </summary>
    Public Const CONST_RTRAIN_I03_205_011201 As String = "205"
    ''' <summary>
    ''' 列車番号(出線 回線3) 五井営業所
    ''' </summary>
    Public Const CONST_RTRAIN_O03_208_011201 As String = "208"
    ''' <summary>
    ''' 列車番号(入線 回線4) 五井営業所
    ''' </summary>
    Public Const CONST_RTRAIN_I04_207_011201 As String = "207"
    ''' <summary>
    ''' 列車番号(出線 回線4) 五井営業所
    ''' </summary>
    Public Const CONST_RTRAIN_O04_210_011201 As String = "210"
    ''' <summary>
    ''' 列車番号(入線 回線5) 五井営業所
    ''' </summary>
    Public Const CONST_RTRAIN_I05_209_011201 As String = "209"
    ''' <summary>
    ''' 列車番号(出線 回線5) 五井営業所
    ''' </summary>
    Public Const CONST_RTRAIN_O05_212_011201 As String = "212"
    ''' <summary>
    ''' 列車番号(入線 回線6) 五井営業所
    ''' </summary>
    Public Const CONST_RTRAIN_I06_211_011201 As String = "211"
    ''' <summary>
    ''' 列車番号(出線 回線6) 五井営業所
    ''' </summary>
    Public Const CONST_RTRAIN_O06_214_011201 As String = "214"
    ''' <summary>
    ''' 列車番号(入線 回線7) 五井営業所
    ''' </summary>
    Public Const CONST_RTRAIN_I07_213_011201 As String = "213"
    ''' <summary>
    ''' 列車番号(出線 回線7) 五井営業所
    ''' </summary>
    Public Const CONST_RTRAIN_O07_216_011201 As String = "216"

    ''' <summary>
    ''' 列車番号(入線 回線11) 甲子営業所
    ''' </summary>
    Public Const CONST_RTRAIN_I11_301_011202 As String = "301"
    ''' <summary>
    ''' 列車番号(出線 回線11) 甲子営業所
    ''' </summary>
    Public Const CONST_RTRAIN_O11_504_011202 As String = "504"
    ''' <summary>
    ''' 列車番号(入線 回線12) 甲子営業所
    ''' </summary>
    Public Const CONST_RTRAIN_I12_301_011202 As String = "301"
    ''' <summary>
    ''' 列車番号(出線 回線12) 甲子営業所
    ''' </summary>
    Public Const CONST_RTRAIN_O12_504_011202 As String = "504"
    ''' <summary>
    ''' 列車番号(入線 回線21) 甲子営業所
    ''' </summary>
    Public Const CONST_RTRAIN_I21_503_011202 As String = "503"
    ''' <summary>
    ''' 列車番号(出線 回線21) 甲子営業所
    ''' </summary>
    Public Const CONST_RTRAIN_O21_508_011202 As String = "508"

    ''' <summary>
    ''' 列車番号(入線 回線1) 袖ヶ浦営業所
    ''' </summary>
    Public Const CONST_RTRAIN_I01_501_011203 As String = "501"
    ''' <summary>
    ''' 列車番号(出線 回線1) 袖ヶ浦営業所
    ''' </summary>
    Public Const CONST_RTRAIN_O01_402_011203 As String = "402"
    ''' <summary>
    ''' 列車番号(入線 回線2) 袖ヶ浦営業所
    ''' </summary>
    Public Const CONST_RTRAIN_I02_401_011203 As String = "401"
    ''' <summary>
    ''' 列車番号(出線 回線2) 袖ヶ浦営業所
    ''' </summary>
    Public Const CONST_RTRAIN_O02_404_011203 As String = "404"
#End Region

#Region "型式(タンク車)"
    ''' <summary>
    ''' タキ1000
    ''' </summary>
    Public Const CONST_MODEL_1000 As String = "タキ1000"
    ''' <summary>
    ''' タキ43000
    ''' </summary>
    Public Const CONST_MODEL_43000 As String = "タキ43000"
    ''' <summary>
    ''' タキ243000
    ''' </summary>
    Public Const CONST_MODEL_243000 As String = "タキ243000"
#End Region

#Region "科目コード"
    ''' <summary>
    ''' 元請輸送
    ''' </summary>
    Public Const CONST_ACCOUNT_41010101 As String = "41010101"
    ''' <summary>
    ''' 開示対象外
    ''' </summary>
    Public Const CONST_ACCOUNT_41010203 As String = "41010203"
    ''' <summary>
    ''' 洗浄・ヤード
    ''' </summary>
    Public Const CONST_ACCOUNT_41010501 As String = "41010501"
    ''' <summary>
    ''' 往路運賃
    ''' </summary>
    Public Const CONST_ACCOUNT_51010101 As String = "51010101"
    ''' <summary>
    ''' 復路運賃
    ''' </summary>
    Public Const CONST_ACCOUNT_51010102 As String = "51010102"
    ''' <summary>
    ''' 回送運賃
    ''' </summary>
    Public Const CONST_ACCOUNT_51010103 As String = "51010103"
    ''' <summary>
    ''' 委託作業費
    ''' </summary>
    Public Const CONST_ACCOUNT_51020104 As String = "51020104"
    ''' <summary>
    ''' 移送・留置料 容器運用費・留置料
    ''' </summary>
    Public Const CONST_ACCOUNT_51040101 As String = "51040101"
    ''' <summary>
    ''' 維持修繕費 外注費
    ''' </summary>
    Public Const CONST_ACCOUNT_51050101 As String = "51050101"
    ''' <summary>
    ''' 維持修繕費 定期検査
    ''' </summary>
    Public Const CONST_ACCOUNT_51050102 As String = "51050102"
    ''' <summary>
    ''' 維持修繕費 再塗装
    ''' </summary>
    Public Const CONST_ACCOUNT_51050104 As String = "51050104"
    ''' <summary>
    ''' 維持修繕費 引当金繰入
    ''' </summary>
    Public Const CONST_ACCOUNT_51050107 As String = "51050107"
    ''' <summary>
    ''' 消耗品費 修理部品（技）
    ''' </summary>
    Public Const CONST_ACCOUNT_51060101 As String = "51060101"
    ''' <summary>
    ''' 消耗品費 その他
    ''' </summary>
    Public Const CONST_ACCOUNT_51060102 As String = "51060102"
    ''' <summary>
    ''' 保険料 動産保険料
    ''' </summary>
    Public Const CONST_ACCOUNT_51070101 As String = "51070101"
    ''' <summary>
    ''' 保険料 生産物保険料
    ''' </summary>
    Public Const CONST_ACCOUNT_51070102 As String = "51070102"
    ''' <summary>
    ''' 諸税公課 固定資産税
    ''' </summary>
    Public Const CONST_ACCOUNT_51080101 As String = "51080101"
    ''' <summary>
    ''' 減価償却費 営業費・自社
    ''' </summary>
    Public Const CONST_ACCOUNT_51090101 As String = "51090101"
    ''' <summary>
    ''' 雑費 直接・手数料
    ''' </summary>
    Public Const CONST_ACCOUNT_51110101 As String = "51110101"
    ''' <summary>
    ''' 雑費 直接・その他
    ''' </summary>
    Public Const CONST_ACCOUNT_51110102 As String = "51110102"
#End Region

#Region "セグメント"
    ''' <summary>
    ''' タンク車使用料
    ''' </summary>
    Public Const CONST_SEGMENT_10101 As String = "10101"
    ''' <summary>
    ''' 鉄道運賃
    ''' </summary>
    Public Const CONST_SEGMENT_10102 As String = "10102"
    ''' <summary>
    ''' 業務料
    ''' </summary>
    Public Const CONST_SEGMENT_10103 As String = "10103"
    ''' <summary>
    ''' 取扱料
    ''' </summary>
    Public Const CONST_SEGMENT_10104 As String = "10104"
    ''' <summary>
    ''' ＯＴ業務料
    ''' </summary>
    Public Const CONST_SEGMENT_10105 As String = "10105"
    ''' <summary>
    ''' 通運取扱その他
    ''' </summary>
    Public Const CONST_SEGMENT_10106 As String = "10106"
    ''' <summary>
    ''' タンク車リース
    ''' </summary>
    Public Const CONST_SEGMENT_10107 As String = "10107"
    ''' <summary>
    ''' 石油共通
    ''' </summary>
    Public Const CONST_SEGMENT_10109 As String = "10109"
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

#Region "実績帳票出力"

    '費用管理
    Public Const CONST_MAPID_COST_MANAGEMENT As String = "OIT0008M"

    '輸送費明細
    Public Const CONST_REPORTNAME_TRANSPORT_COST_DETAIL As String = "輸送費明細"
    Public Const CONST_TEMPNAME_TRANSPORT_COST_DETAIL As String = "OIT0008M_TRASPORT_COST_DETAIL.xlsx"

    '科目別一覧
    Public Const CONST_REPORTNAME_ACCOUNT_BRANCH_BUSINESS_INCOME As String = "科目別支店別計上額一覧表"
    Public Const CONST_TEMPNAME_ACCOUNT_BRANCH_BUSINESS_INCOME As String = "OIT0008M_ACCOUNT_BRANCH_BUSINESS_INCOME.xlsx"

    '請求先別一覧
    Public Const CONST_REPORTNAME_INVOICE_BRANCH_BUSINESS_INCOME As String = "科目別支店別計上額一覧表"
    Public Const CONST_TEMPNAME_INVOICE_BRANCH_BUSINESS_INCOME As String = "OIT0008M_INVOICE_BRANCH_BUSINESS_INCOME.xlsx"

    '経理連携IF
    Public Const CONST_REPORTNAME_FINANCE_COOPERATION_IF As String = "im_SW"
    Public Const CONST_TEMPNAME_FINANCE_COOPERATION_IF As String = "OIT0008M_FINANCE_COOPERATION_IF.xlsx"

    'タンク車運賃実績表(列車別)
    Public Const CONST_REPORTNAME_TANK_TRANSPORT_RESULT As String = "タンク車運賃実績表"
    Public Const CONST_TEMPNAME_TANK_TRANSPORT_RESULT As String = "OIT0008M_TANK_TRASPORT_RESULT.xlsx"

    'タンク車運賃実績表(列車別・仙台)
    Public Const CONST_TEMPNAME_TANK_TRANSPORT_RESULT_010402 As String = "OIT0008M_TANK_TRASPORT_RESULT_010402.xlsx"

    'タンク車運賃実績表(着駅別)
    Public Const CONST_REPORTNAME_TANK_TRANSPORT_RESULT_ARR As String = "タンク車運賃実績表（着駅別）"
    Public Const CONST_TEMPNAME_TANK_TRANSPORT_RESULT_ARR As String = "OIT0008M_TANK_TRASPORT_RESULT_ARR.xlsx"

    'タンク車運賃実績表(着駅別・仙台)
    Public Const CONST_TEMPNAME_TANK_TRANSPORT_RESULT_ARR_010402 As String = "OIT0008M_TANK_TRASPORT_RESULT_ARR_010402.xlsx"

    '輸送実績表
    Public Const CONST_REPORTNAME_TRANSPORT_RESULT As String = "輸送実績表"
    Public Const CONST_TEMPNAME_TRANSPORT_RESULT As String = "OIT0008M_TRASPORT_RESULT.xlsx"

    '輸送実績表（仙台）
    Public Const CONST_TEMPNAME_TRANSPORT_RESULT_010402 As String = "OIT0008M_TRASPORT_RESULT_010402.xlsx"

    '輸送実績表（五井）
    Public Const CONST_TEMPNAME_TRANSPORT_RESULT_011201 As String = "OIT0008M_TRASPORT_RESULT_011201.xlsx"

#End Region

End Module 'End BaseDllConst