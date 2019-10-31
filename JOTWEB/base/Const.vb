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
#End Region

End Module 'End BaseDllConst