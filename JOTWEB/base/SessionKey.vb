Option Strict On
''' <summary>
''' 共通で利用する定数定義
''' </summary>
Public Module SessionKey

    ''' <summary>
    ''' セッションのKEY一覧
    ''' </summary>
    Public Class C_SESSION_KEY
        ''' <summary>
        ''' 名前空間名
        ''' </summary>
        Public Const NAMESPACE_VALUE As String = "NamespaceStr"
        ''' <summary>
        ''' クラス名
        ''' </summary>
        Public Const CLASS_NAME As String = "ClassStr"
        ''' <summary>
        ''' DB　接続文字列
        ''' </summary>
        Public Const DB_CONNECT As String = "DBcon"
        ''' <summary>
        ''' ログインユーザID
        ''' </summary>
        Public Const USER_ID As String = "Userid"
        ''' <summary>
        ''' ユーザの端末
        ''' </summary>
        Public Const USER_TERM_ID As String = "Term"
        ''' <summary>
        ''' ユーザの端末設置会社
        ''' </summary>
        Public Const TERM_COMPANY As String = "TermCamp"
        ''' <summary>
        ''' ユーザの端末設置部署
        ''' </summary>
        Public Const TERM_ORGANIZATION As String = "TermORG"
        ''' <summary>
        ''' ユーザの端末保持部署
        ''' </summary>
        Public Const TERM_MANAGMENT_ORGANIZATION As String = "TermMORG"
        ''' <summary>
        ''' ユーザの管轄外接続会社
        ''' </summary>
        Public Const SELECTED_ANOTHER_COMPANY As String = "SelectedCamp"

        Public Const TERM_DRIVERS As String = "DRIVERS"
        ''' <summary>
        ''' ログ格納ディレクトリ
        ''' </summary>
        Public Const LOGGING_PATH As String = "LOGdir"
        ''' <summary>
        ''' 情報退避XML格納ディレクトリ
        ''' </summary>
        Public Const PDF_PRINT_PATH As String = "PDFdir"
        ''' <summary>
        ''' アップロードFILE格納ディレクトリ
        ''' </summary>
        Public Const UPLOADED_PATH As String = "FILEdir"
        ''' <summary>
        ''' 更新ジャーナル格納ディレクトリ
        ''' </summary>
        Public Const UPDATE_JORNALING_PATH As String = "JNLdir"
        ''' <summary>
        ''' システム格納ディレクトリ
        ''' </summary>
        Public Const SYSTEM_PATH As String = "SYSdir"
        ''' <summary>
        ''' APサーバ端末ID
        ''' </summary>
        Public Const APSV_TERM_ID As String = "APSRVname"
        ''' <summary>
        ''' APサーバー設置(使用)会社
        ''' </summary>
        Public Const APSV_FOUNDIION_COMPAY As String = "APSRVCamp"
        ''' <summary>
        ''' APサーバー設置(使用)部署
        ''' </summary>
        Public Const APSV_FOUNDIION_ORGANIZATION As String = "APSRVOrg"
        ''' <summary>
        ''' APサーバー管理部署
        ''' </summary>
        Public Const APSV_MANAGMENT_ORGANIZATION As String = "MOrg"
        ''' <summary>
        ''' 画面ID
        ''' </summary>
        Public Const MAPPING_DISPLAY_MAP_ID As String = "MAPmapid"
        ''' <summary>
        ''' メニュー表示権限
        ''' </summary>
        Public Const MAPPING_USER_MENU_MODE As String = "MAPMenuMode"
        ''' <summary>
        ''' 画面参照更新権限
        ''' </summary>
        Public Const MAPPING_USER_MAP_MODE As String = "MAPMapMode"
        ''' <summary>
        ''' 画面表示項目権限
        ''' </summary>
        Public Const MAPPING_USER_VIEWPROF_MODE As String = "MAPViewProfmode"
        ''' <summary>
        ''' エクセル出力権限
        ''' </summary>
        Public Const MAPPING_USER_RPRTPROF_MODE As String = "MAPRprtProfmode"
        ''' <summary>
        ''' 承認権限
        ''' </summary>
        Public Const MAPPING_USER_APPROVALID As String = "MAPApprovalID"
        ''' <summary>
        ''' 画面表示バリアント
        ''' </summary>
        Public Const MAPPING_USER_MAP_VARIANT As String = "MAPvariant"
        ''' <summary>
        ''' 画面権限
        ''' </summary>
        Public Const MAPPING_PERMISSION_MODE As String = "MAPpermitcode"
        ''' <summary>
        ''' その他画面情報
        ''' </summary>
        Public Const MAPPING_ETC_VALUE As String = "MAPetc"
        ''' <summary>
        ''' ヘルプ表示画面ID
        ''' </summary>
        Public Const MAPPING_HELP_MAP_ID As String = "HELPId"
        ''' <summary>
        ''' ヘルプ表示会社コード
        ''' </summary>
        Public Const MAPPING_HELP_COMP_CD As String = "HELPComp"
        ''' <summary>
        ''' ログオン年月日
        ''' </summary>
        Public Const LOGON_LOGIN_DATE As String = "LogonYMD"
        ''' <summary>
        ''' 選択開始年月日
        ''' </summary>
        Public Const SELECTED_START_DATE As String = "Selected_STYMD"
        ''' <summary>
        ''' 選択終了年月日
        ''' </summary>
        Public Const SELECTED_END_DATE As String = "Selected_ENDYMD"
        ''' <summary>
        ''' メニューカスタム情報リスト
        ''' </summary>
        Public Const USERMENU_COSTOM_LIST As String = "UserMenuCostomList"
    End Class

End Module 'End BaseDllConst