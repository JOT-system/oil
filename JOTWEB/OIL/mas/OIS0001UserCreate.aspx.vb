''************************************************************
' ユーザIDマスタメンテ登録画面
' 作成日 2019/11/14
' 更新日 2019/11/14
' 作成者 JOT遠藤
' 更新車 JOT遠藤
'
' 修正履歴:
'         :
''************************************************************
Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' ユーザIDマスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class OIS0001UserCreate
    Inherits Page

    '○ 検索結果格納Table
    Private OIS0001tbl As DataTable                                  '一覧格納用テーブル
    Private OIS0001INPtbl As DataTable                               'チェック用テーブル
    Private OIS0001UPDtbl As DataTable                               '更新用テーブル

    Private Const CONST_DISPROWCOUNT As Integer = 45                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 20                 'マウススクロール時稼働行数
    Private Const CONST_DETAIL_TABID As String = "DTL1"             '明細部ID

    '○ データOPERATION用
    Private Const CONST_INSERT As String = "Insert"                 'データ追加
    Private Const CONST_UPDATE As String = "Update"                 'データ更新
    Private Const CONST_PATTERNERR As String = "PATTEN ERR"         '関連チェックエラー

    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
    Private CS0013ProfView As New CS0013ProfView                    'Tableオブジェクト展開
    Private CS0020JOURNAL As New CS0020JOURNAL                      '更新ジャーナル出力
    Private CS0023XLSUPLOAD As New CS0023XLSUPLOAD                  'XLSアップロード
    Private CS0025AUTHORget As New CS0025AUTHORget                  '権限チェック(マスタチェック)
    Private CS0030REPORT As New CS0030REPORT                        '帳票出力
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理

    '○ 共通処理結果
    Private WW_ERR_SW As String = ""
    Private WW_RTN_SW As String = ""
    Private WW_DUMMY As String = ""
    Private WW_ERRCODE As String                                    'サブ用リターンコード

    ''' <summary>
    ''' サーバー処理の遷移先
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        Try
            If IsPostBack Then
                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    '○ 画面表示データ復元
                    Master.RecoverTable(OIS0001tbl, work.WF_SEL_INPTBL.Text)

                    Select Case WF_ButtonClick.Value
                        Case "WF_UPDATE"                '表更新ボタン押下
                            WF_UPDATE_Click()
                        Case "WF_CLEAR"                 'クリアボタン押下
                            WF_CLEAR_Click()
                        Case "WF_Field_DBClick"         'フィールドダブルクリック
                            WF_FIELD_DBClick()
                        Case "WF_ButtonSel"             '(左ボックス)選択ボタン押下
                            WF_ButtonSel_Click()
                        Case "WF_ButtonCan"             '(左ボックス)キャンセルボタン押下
                            WF_ButtonCan_Click()
                        Case "WF_ListboxDBclick"        '左ボックスダブルクリック
                            WF_ButtonSel_Click()
                        Case "WF_RadioButonClick"       '(右ボックス)ラジオボタン選択
                            WF_RadioButton_Click()
                        Case "WF_MEMOChange"            '(右ボックス)メモ欄更新
                            WF_RIGHTBOX_Change()
                    End Select
                End If
            Else
                '○ 初期化処理
                Initialize()
            End If

            '○ 画面モード(更新・参照)設定
            If Master.MAPpermitcode = C_PERMISSION.UPDATE Then
                WF_MAPpermitcode.Value = "TRUE"
            Else
                WF_MAPpermitcode.Value = "FALSE"
            End If

            WF_BOXChange.Value = "detailbox"

        Finally
            '○ 格納Table Close
            If Not IsNothing(OIS0001tbl) Then
                OIS0001tbl.Clear()
                OIS0001tbl.Dispose()
                OIS0001tbl = Nothing
            End If

            If Not IsNothing(OIS0001INPtbl) Then
                OIS0001INPtbl.Clear()
                OIS0001INPtbl.Dispose()
                OIS0001INPtbl = Nothing
            End If

            If Not IsNothing(OIS0001UPDtbl) Then
                OIS0001UPDtbl.Clear()
                OIS0001UPDtbl.Dispose()
                OIS0001UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIS0001WRKINC.MAPIDC
        '○HELP表示有無設定
        Master.dispHelp = False
        '○D&D有無設定
        Master.eventDrop = True
        '○Grid情報保存先のファイル名
        'Master.CreateXMLSaveFile()

        '○初期値設定
        WF_FIELD.Value = ""
        WF_ButtonClick.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_RightboxOpen.Value = ""
        rightview.ResetIndex()
        leftview.ActiveListBox()

        '右Boxへの値設定
        rightview.MAPID = Master.MAPID
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_DUMMY)

        '○ 画面の値設定
        WW_MAPValueSet()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIS0001L Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        End If

        '○ 名称設定処理
        '選択行
        WF_Sel_LINECNT.Text = work.WF_SEL_LINECNT.Text

        'ユーザID
        WF_USERID.Text = work.WF_SEL_USERID.Text

        '社員名（短）
        WF_STAFFNAMES.Text = work.WF_SEL_STAFFNAMES.Text

        '社員名（長）
        WF_STAFFNAMEL.Text = work.WF_SEL_STAFFNAMEL.Text

        '画面ＩＤ
        WF_MAPID.Text = work.WF_SEL_MAPID.Text

        'パスワード
        WF_PASSWORD.Text = work.WF_SEL_PASSWORD.Text

        '誤り回数
        WF_MISSCNT.Text = work.WF_SEL_MISSCNT.Text

        'パスワード有効期限
        WF_PASSENDYMD.Text = work.WF_SEL_PASSENDYMD.Text

        '開始年月日
        WF_STYMD.Text = work.WF_SEL_STYMD.Text

        '終了年月日
        WF_ENDYMD.Text = work.WF_SEL_ENDYMD.Text

        '会社コード
        WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text

        '組織コード
        WF_ORG.Text = work.WF_SEL_ORG.Text

        'メールアドレス
        WF_EMAIL.Text = work.WF_SEL_EMAIL.Text

        'メニュー表示制御ロール
        WF_MENUROLE.Text = work.WF_SEL_MENUROLE.Text

        '画面参照更新制御ロール
        WF_MAPROLE.Text = work.WF_SEL_MAPROLE.Text

        '画面表示項目制御ロール
        WF_VIEWPROFID.Text = work.WF_SEL_VIEWPROFID.Text

        'エクセル出力制御ロール
        WF_RPRTPROFID.Text = work.WF_SEL_RPRTPROFID.Text

        '画面初期値ロール
        WF_VARIANT.Text = work.WF_SEL_VARIANT.Text

        '承認権限ロール
        WF_APPROVALID.Text = work.WF_SEL_APPROVALID.Text

        '削除
        WF_DELFLG.Text = work.WF_SEL_DELFLG.Text
        CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_DUMMY)

    End Sub

    ''' <summary>
    ''' 画面表示データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MAPDataGet(ByVal SQLcon As SqlConnection)

        If IsNothing(OIS0001tbl) Then
            OIS0001tbl = New DataTable
        End If

        If OIS0001tbl.Columns.Count <> 0 Then
            OIS0001tbl.Columns.Clear()
        End If

        OIS0001tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データをユーザIDマスタから取得する

        Dim SQLStr As String =
              " SELECT" _
            & "    0                                                   AS LINECNT" _
            & "    , ''                                                AS OPERATION" _
            & "    , CAST(OIS0004.UPDTIMSTP AS BIGINT)                    AS TIMSTP" _
            & "    , 1                                                 AS 'SELECT'" _
            & "    , 0                                                 AS HIDDEN" _
            & "    , ISNULL(RTRIM(OIS0004.USERID), '')                    AS USERID" _
            & "    , ISNULL(FORMAT(OIS0004.STYMD, 'yyyy/MM/dd'), '')      AS STYMD" _
            & "    , ISNULL(FORMAT(OIS0004.ENDYMD, 'yyyy/MM/dd'), '')     AS ENDYMD" _
            & "    , ISNULL(RTRIM(OIS0004.CAMPCODE), '')                  AS CAMPCODE" _
            & "    , ''                                                AS CAMPNAMES" _
            & "    , ISNULL(RTRIM(OIS0004.ORG), '')                       AS ORG" _
            & "    , ''                                                AS ORGNAMES" _
            & "    , ISNULL(RTRIM(OIS0004.STAFFCODE), '')                 AS STAFFCODE" _
            & "    , ISNULL(RTRIM(OIS0004.STAFFNAMES), '')                AS STAFFNAMES" _
            & "    , ISNULL(RTRIM(OIS0004.STAFFNAMEL), '')                AS STAFFNAMEL" _
            & "    , ISNULL(RTRIM(OIS0004.CAMPROLE), '')                  AS CAMPROLE" _
            & "    , ISNULL(RTRIM(OIS0004.MAPROLE), '')                   AS MAPROLE" _
            & "    , ISNULL(RTRIM(OIS0004.ORGROLE), '')                   AS ORGROLE" _
            & "    , ISNULL(RTRIM(OIS0004.VIEWPROFID), '')                AS VIEWPROFID" _
            & "    , ISNULL(RTRIM(OIS0004.RPRTPROFID), '')                AS RPRTPROFID" _
            & "    , ISNULL(RTRIM(OIS0004.MAPID), '')                     AS MAPID" _
            & "    , ISNULL(RTRIM(OIS0004.VARIANT), '')                   AS VARIANT" _
            & "    , ISNULL(RTRIM(S014.PASSWORD), '')                  AS PASSWORD" _
            & "    , ISNULL(RTRIM(S014.MISSCNT), '')                   AS MISSCNT" _
            & "    , ISNULL(FORMAT(S014.PASSENDYMD, 'yyyy/MM/dd'), '') AS PASSENDYMD" _
            & "    , ISNULL(RTRIM(S051.ROLE), '')                      AS ROLEAPPROVAL1" _
            & "    , ''                                                AS ROLEAPPROVAL1TYPE" _
            & "    , ISNULL(RTRIM(S052.ROLE), '')                      AS ROLEAPPROVAL2" _
            & "    , ''                                                AS ROLEAPPROVAL2TYPE" _
            & "    , ISNULL(RTRIM(OIS0004.DELFLG), '')                    AS DELFLG" _
            & " FROM" _
            & "    S0004_USER OIS0004" _
            & "    INNER JOIN S0014_USERPASS S014" _
            & "        ON  S014.USERID   = OIS0004.USERID" _
            & "        AND S014.DELFLG  <> @P6" _
            & "    LEFT JOIN S0005_AUTHOR S051" _
            & "        ON  S051.USERID   = OIS0004.USERID" _
            & "        AND S051.CAMPCODE = OIS0004.CAMPCODE" _
            & "        AND S051.OBJECT   = @P2" _
            & "        AND S051.STYMD    = OIS0004.STYMD" _
            & "        AND S051.DELFLG  <> @P6" _
            & "    LEFT JOIN S0005_AUTHOR S052" _
            & "        ON  S052.USERID   = OIS0004.USERID" _
            & "        AND S052.CAMPCODE = OIS0004.CAMPCODE" _
            & "        AND S052.OBJECT   = @P3" _
            & "        AND S052.STYMD    = OIS0004.STYMD" _
            & "        AND S052.DELFLG  <> @P6" _
            & " WHERE" _
            & "    OIS0004.CAMPCODE    = @P1" _
            & "    AND OIS0004.STYMD  <= @P4" _
            & "    AND OIS0004.ENDYMD >= @P5" _
            & "    AND OIS0004.DELFLG <> @P6"

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '所属部署
        If Not String.IsNullOrEmpty(work.WF_SEL_ORG.Text) Then
            SQLStr &= String.Format("    AND S004.ORG     = '{0}'", work.WF_SEL_ORG.Text)
        End If

        SQLStr &=
              " ORDER BY" _
            & "    S004.ORG" _
            & "    , S004.USERID"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)        '第一承認
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 20)        '最終承認
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.Date)                '有効年月日(To)
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.Date)                '有効年月日(From)
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 1)         '削除フラグ

                PARA1.Value = work.WF_SEL_CAMPCODE.Text
                PARA2.Value = "APPROVAL1"
                PARA3.Value = "APPROVAL2"
                PARA4.Value = work.WF_SEL_ENDYMD.Text
                PARA5.Value = work.WF_SEL_STYMD.Text
                PARA6.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIS0001tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIS0001tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIS0001row As DataRow In OIS0001tbl.Rows
                    i += 1
                    OIS0001row("LINECNT") = i        'LINECNT
                    '名称取得
                    CODENAME_get("CAMPCODE", OIS0001row("CAMPCODE"), OIS0001row("CAMPNAMES"), WW_DUMMY)                               '会社コード
                    CODENAME_get("ORG", OIS0001row("ORG"), OIS0001row("ORGNAMES"), WW_DUMMY)                                          '所属部署
                    CODENAME_get("ROLEAPPROVAL1", OIS0001row("ROLEAPPROVAL1"), OIS0001row("ROLEAPPROVAL1TYPE"), WW_DUMMY, "2")        '権限(第一承認)
                    CODENAME_get("ROLEAPPROVAL2", OIS0001row("ROLEAPPROVAL2"), OIS0001row("ROLEAPPROVAL2TYPE"), WW_DUMMY, "2")        '権限(最終承認)
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIS0001L SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIS0001L Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 一意制約チェック
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub UniqueKeyCheck(ByVal SQLcon As SqlConnection, ByRef O_MESSAGENO As String)

        '○ 対象データ取得
        Dim SQLStr As String =
              " SELECT " _
            & "     TANKNUMBER " _
            & " FROM" _
            & "    OIL.OIS0004_TANK" _
            & " WHERE" _
            & "     TANKNUMBER      = @P01"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20)            'JOT車番
                PARA1.Value = WF_USERID.Text

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    Dim OIS0001Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIS0001Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIS0001Chk.Load(SQLdr)

                    If OIS0001Chk.Rows.Count > 0 Then
                        '重複データエラー
                        O_MESSAGENO = Messages.C_MESSAGE_NO.OVERLAP_DATA_ERROR
                    Else
                        '正常終了時
                        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIS0001C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIS0001C UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_Scroll()

    End Sub

    ' ******************************************************************************
    ' ***  詳細表示関連操作                                                      ***
    ' ******************************************************************************

    ''' <summary>
    ''' 詳細画面-表更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_UPDATE_Click()

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        '○ DetailBoxをINPtblへ退避
        DetailBoxToOIS0001INPtbl(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ERR_SW) Then
            OIS0001tbl_UPD()
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIS0001tbl, work.WF_SEL_INPTBL.Text)

        '○ メッセージ表示
        If WW_ERR_SW = "" Then
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)
        Else
            If isNormal(WW_ERR_SW) Then
                Master.Output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.INF)
            Else
                Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
            End If
        End If

        If isNormal(WW_ERR_SW) Then
            '前ページ遷移
            Master.TransitionPrevPage()
        End If

    End Sub

    ''' <summary>
    ''' 詳細画面-テーブル退避
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub DetailBoxToOIS0001INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(WF_DELFLG.Text)            '削除フラグ

        '○ GridViewから未選択状態で表更新ボタンを押下時の例外を回避する
        If String.IsNullOrEmpty(WF_Sel_LINECNT.Text) AndAlso
            String.IsNullOrEmpty(WF_DELFLG.Text) Then
            Master.Output(C_MESSAGE_NO.INVALID_PROCCESS_ERROR, C_MESSAGE_TYPE.ERR, "no Detail")

            CS0011LOGWrite.INFSUBCLASS = "DetailBoxToINPtbl"        'SUBクラス名
            CS0011LOGWrite.INFPOSI = "non Detail"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWrite.TEXT = "non Detail"
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                         'ログ出力

            O_RTN = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            Exit Sub
        End If

        Master.CreateEmptyTable(OIS0001INPtbl, work.WF_SEL_INPTBL.Text)
        Dim OIS0001INProw As DataRow = OIS0001INPtbl.NewRow

        '○ 初期クリア
        For Each OIS0001INPcol As DataColumn In OIS0001INPtbl.Columns
            If IsDBNull(OIS0001INProw.Item(OIS0001INPcol)) OrElse IsNothing(OIS0001INProw.Item(OIS0001INPcol)) Then
                Select Case OIS0001INPcol.ColumnName
                    Case "LINECNT"
                        OIS0001INProw.Item(OIS0001INPcol) = 0
                    Case "OPERATION"
                        OIS0001INProw.Item(OIS0001INPcol) = C_LIST_OPERATION_CODE.NODATA
                    Case "UPDTIMSTP"
                        OIS0001INProw.Item(OIS0001INPcol) = 0
                    Case "SELECT"
                        OIS0001INProw.Item(OIS0001INPcol) = 1
                    Case "HIDDEN"
                        OIS0001INProw.Item(OIS0001INPcol) = 0
                    Case Else
                        OIS0001INProw.Item(OIS0001INPcol) = ""
                End Select
            End If
        Next

        'LINECNT
        If WF_Sel_LINECNT.Text = "" Then
            OIS0001INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(WF_Sel_LINECNT.Text, OIS0001INProw("LINECNT"))
            Catch ex As Exception
                OIS0001INProw("LINECNT") = 0
            End Try
        End If

        OIS0001INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        OIS0001INProw("UPDTIMSTP") = 0
        OIS0001INProw("SELECT") = 1
        OIS0001INProw("HIDDEN") = 0

        OIS0001INProw("DELFLG") = WF_DELFLG.Text                     '削除フラグ

        OIS0001INProw("USERID") = WF_USERID.Text              'ユーザID

        OIS0001INProw("STAFFNAMES") = WF_STAFFNAMES.Text              '社員名（短）

        OIS0001INProw("STAFFNAMEL") = WF_STAFFNAMEL.Text              '社員名（長）

        OIS0001INProw("MAPID") = WF_MAPID.Text              '画面ＩＤ

        OIS0001INProw("PASSWORD") = WF_PASSWORD.Text              'パスワード

        OIS0001INProw("MISSCNT") = WF_MISSCNT.Text              '誤り回数

        OIS0001INProw("PASSENDYMD") = WF_PASSENDYMD.Text              'パスワード有効期限

        OIS0001INProw("STYMD") = WF_STYMD.Text              '開始年月日

        OIS0001INProw("ENDYMD") = WF_ENDYMD.Text              '終了年月日

        OIS0001INProw("CAMPCODE") = WF_CAMPCODE.Text              '会社コード

        OIS0001INProw("ORG") = WF_ORG.Text              '組織コード

        OIS0001INProw("EMAIL") = WF_EMAIL.Text              'メールアドレス

        OIS0001INProw("MENUROLE") = WF_MENUROLE.Text              'メニュー表示制御ロール

        OIS0001INProw("MAPROLE") = WF_MAPROLE.Text              '画面参照更新制御ロール

        OIS0001INProw("VIEWPROFID") = WF_VIEWPROFID.Text              '画面表示項目制御ロール

        OIS0001INProw("RPRTPROFID") = WF_RPRTPROFID.Text              'エクセル出力制御ロール

        OIS0001INProw("VARIANT") = WF_VARIANT.Text              '画面初期値ロール

        OIS0001INProw("APPROVALID") = WF_APPROVALID.Text              '承認権限ロール

        '○ チェック用テーブルに登録する
        OIS0001INPtbl.Rows.Add(OIS0001INProw)

    End Sub


    ''' <summary>
    ''' 詳細画面-クリアボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_CLEAR_Click()

        '○ 詳細画面初期化
        DetailBoxClear()

        '○ メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_CLEAR_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""

        Master.TransitionPrevPage()

    End Sub

    ''' <summary>
    ''' 詳細画面初期化
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailBoxClear()

        '○ 状態をクリア
        For Each OIS0001row As DataRow In OIS0001tbl.Rows
            Select Case OIS0001row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.NODISP
                    OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.SELECTED
                    OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIS0001tbl, work.WF_SEL_INPTBL.Text)

        WF_Sel_LINECNT.Text = ""            'LINECNT

        WF_USERID.Text = ""            'ユーザID
        WF_STAFFNAMES.Text = ""            '社員名（短）
        WF_STAFFNAMEL.Text = ""            '社員名（長）
        WF_MAPID.Text = ""            '画面ＩＤ
        WF_PASSWORD.Text = ""            'パスワード
        WF_MISSCNT.Text = ""            '誤り回数
        WF_PASSENDYMD.Text = ""            'パスワード有効期限
        WF_STYMD.Text = ""            '開始年月日
        WF_ENDYMD.Text = ""            '終了年月日
        WF_CAMPCODE.Text = ""            '会社コード
        WF_ORG.Text = ""            '組織コード
        WF_EMAIL.Text = ""            'メールアドレス
        WF_MENUROLE.Text = ""            'メニュー表示制御ロール
        WF_MAPROLE.Text = ""            '画面参照更新制御ロール
        WF_VIEWPROFID.Text = ""            '画面表示項目制御ロール
        WF_RPRTPROFID.Text = ""            'エクセル出力制御ロール
        WF_VARIANT.Text = ""            '画面初期値ロール
        WF_APPROVALID.Text = ""            '承認権限ロール
        WF_DELFLG.Text = ""                 '削除フラグ
        WF_DELFLG_TEXT.Text = ""            '削除フラグ名称

    End Sub


    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_DBClick()

        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                Integer.TryParse(WF_LeftMViewChange.Value, WF_LeftMViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try

            Dim WW_FIELD As String = ""
            If WF_FIELD_REP.Value = "" Then
                WW_FIELD = WF_FIELD.Value
            Else
                WW_FIELD = WF_FIELD_REP.Value
            End If

            With leftview
                '会社コード
                Dim prmData As New Hashtable

                'フィールドによってパラメーターを変える
                Select Case WW_FIELD
                    Case "WF_DELFLG"
                        prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = "2"
                End Select

                .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                .ActiveListBox()
            End With
        End If

    End Sub


    ' ******************************************************************************
    ' ***  leftBOX関連操作                                                       ***
    ' ******************************************************************************

    ''' <summary>
    ''' LeftBox選択時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()

        Dim WW_SelectValue As String = ""
        Dim WW_SelectText As String = ""

        '○ 選択内容を取得
        If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
            WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
            WW_SelectValue = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Value
            WW_SelectText = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Text
        End If

        '○ 選択内容を画面項目へセット
        If WF_FIELD_REP.Value = "" Then
            Select Case WF_FIELD.Value
                '削除フラグ
                Case "WF_DELFLG"
                    WF_DELFLG.Text = WW_SelectValue
                    WF_DELFLG_TEXT.Text = WW_SelectText
                    WF_DELFLG.Focus()
            End Select
        Else
        End If

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_RightboxOpen.Value = ""

    End Sub

    ''' <summary>
    ''' LeftBoxキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○ フォーカスセット
        If WF_FIELD_REP.Value = "" Then
            Select Case WF_FIELD.Value
                '削除フラグ
                Case "WF_DELFLG"
                    WF_DELFLG.Focus()
            End Select
        Else
        End If

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_RightboxOpen.Value = ""

    End Sub


    ''' <summary>
    ''' RightBoxラジオボタン選択処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RadioButton_Click()

        If Not String.IsNullOrEmpty(WF_RightViewChange.Value) Then
            Try
                Integer.TryParse(WF_RightViewChange.Value, WF_RightViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try

            rightview.SelectIndex(WF_RightViewChange.Value)
            WF_RightViewChange.Value = ""
        End If

    End Sub

    ''' <summary>
    ''' RightBoxメモ欄更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_Change()

        rightview.Save(Master.USERID, Master.USERTERMID, WW_DUMMY)

    End Sub


    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    ''' 入力値チェック
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub INPTableCheck(ByRef O_RTN As String)
        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_LINE_ERR As String = ""
        Dim WW_TEXT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""
        Dim WW_UniqueKeyCHECK As String = ""

        '○ 画面操作権限チェック
        '権限チェック(操作者がデータ内USERの更新権限があるかチェック
        '　※権限判定時点：現在
        CS0025AUTHORget.USERID = CS0050SESSION.USERID
        CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_PERTMIT
        CS0025AUTHORget.CODE = Master.MAPID
        CS0025AUTHORget.STYMD = Date.Now
        CS0025AUTHORget.ENDYMD = Date.Now
        CS0025AUTHORget.CS0025AUTHORget()
        If isNormal(CS0025AUTHORget.ERR) AndAlso CS0025AUTHORget.PERMITCODE = C_PERMISSION.UPDATE Then
        Else
            WW_CheckMES1 = "・更新できないレコード(ユーザ更新権限なし)です。"
            WW_CheckMES2 = ""
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LINE_ERR = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Exit Sub
        End If

        '○ 単項目チェック
        For Each OIS0001INProw As DataRow In OIS0001INPtbl.Rows

            WW_LINE_ERR = ""

            '削除フラグ(バリデーションチェック）
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DELFLG", OIS0001INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("DELFLG", OIS0001INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'JOT車番(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TANKNUMBER", OIS0001INProw("TANKNUMBER"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "JOT車番入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '原籍所有者C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ORIGINOWNERCODE", OIS0001INProw("ORIGINOWNERCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "原籍所有者C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '名義所有者C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OWNERCODE", OIS0001INProw("OWNERCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "名義所有者C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'リース先C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LEASECODE", OIS0001INProw("LEASECODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "リース先C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'リース区分C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LEASECLASS", OIS0001INProw("LEASECLASS"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "リース区分C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '自動延長(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "AUTOEXTENTION", OIS0001INProw("AUTOEXTENTION"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "自動延長入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'リース開始年月日(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LEASESTYMD", OIS0001INProw("LEASESTYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "リース開始年月日入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'リース満了年月日(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LEASEENDYMD", OIS0001INProw("LEASEENDYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "リース満了年月日入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '第三者使用者C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "USERCODE", OIS0001INProw("USERCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "第三者使用者C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '原常備駅C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "CURRENTSTATIONCODE", OIS0001INProw("CURRENTSTATIONCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "原常備駅C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '臨時常備駅C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "EXTRADINARYSTATIONCODE", OIS0001INProw("EXTRADINARYSTATIONCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "臨時常備駅C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '第三者使用期限(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "USERLIMIT", OIS0001INProw("USERLIMIT"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "第三者使用期限入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '臨時常備駅期限(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LIMITTEXTRADIARYSTATION", OIS0001INProw("LIMITTEXTRADIARYSTATION"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "臨時常備駅期限入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '原専用種別C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DEDICATETYPECODE", OIS0001INProw("DEDICATETYPECODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "原専用種別C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '臨時専用種別C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "EXTRADINARYTYPECODE", OIS0001INProw("EXTRADINARYTYPECODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "臨時専用種別C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '臨時専用期限(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "EXTRADINARYLIMIT", OIS0001INProw("EXTRADINARYLIMIT"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "臨時専用期限入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '運用基地C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OPERATIONBASECODE", OIS0001INProw("OPERATIONBASECODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "運用基地C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '塗色C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "COLORCODE", OIS0001INProw("COLORCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "塗色C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'エネオス(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ENEOS", OIS0001INProw("ENEOS"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "エネオス入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'エコレール(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ECO", OIS0001INProw("ECO"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "エコレール入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '取得年月日(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ALLINSPECTIONDATE", OIS0001INProw("ALLINSPECTIONDATE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "取得年月日入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '車籍編入年月日(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TRANSFERDATE", OIS0001INProw("TRANSFERDATE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "車籍編入年月日入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '取得先C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OBTAINEDCODE", OIS0001INProw("OBTAINEDCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "取得先C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '一意制約チェック
            '同一レコードの更新の場合、チェック対象外
            If OIS0001INProw("TANKNUMBER") = work.WF_SEL_USERID.Text Then

            Else
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    'DataBase接続
                    SQLcon.Open()

                    '一意制約チェック
                    UniqueKeyCheck(SQLcon, WW_UniqueKeyCHECK)
                End Using

                If Not isNormal(WW_UniqueKeyCHECK) Then
                    WW_CheckMES1 = "一意制約違反。"
                    WW_CheckMES2 = C_MESSAGE_NO.OVERLAP_DATA_ERROR &
                                       "([" & OIS0001INProw("TANKNUMBER") & "]" &
                                       " [" & OIS0001INProw("MODEL") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.OVERLAP_DATA_ERROR
                End If
            End If

            If WW_LINE_ERR = "" Then
                If OIS0001INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    OIS0001INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LINE_ERR = CONST_PATTERNERR Then
                    '関連チェックエラーをセット
                    OIS0001INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    '単項目チェックエラーをセット
                    OIS0001INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                End If
            End If
        Next

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="OIS0001row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIS0001row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIS0001row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> JOT車番 =" & OIS0001row("TANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 原籍所有者C =" & OIS0001row("ORIGINOWNERCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 名義所有者C =" & OIS0001row("OWNERCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> リース先C =" & OIS0001row("LEASECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> リース区分C =" & OIS0001row("LEASECLASS") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 自動延長 =" & OIS0001row("AUTOEXTENTION") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> リース開始年月日 =" & OIS0001row("LEASESTYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> リース満了年月日 =" & OIS0001row("LEASEENDYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 第三者使用者C =" & OIS0001row("USERCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 原常備駅C =" & OIS0001row("CURRENTSTATIONCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 臨時常備駅C =" & OIS0001row("EXTRADINARYSTATIONCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 第三者使用期限 =" & OIS0001row("USERLIMIT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 臨時常備駅期限 =" & OIS0001row("LIMITTEXTRADIARYSTATION") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 原専用種別C =" & OIS0001row("DEDICATETYPECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 臨時専用種別C =" & OIS0001row("EXTRADINARYTYPECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 臨時専用期限 =" & OIS0001row("EXTRADINARYLIMIT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 運用基地C =" & OIS0001row("OPERATIONBASECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 塗色C =" & OIS0001row("COLORCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> エネオス =" & OIS0001row("ENEOS") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> エコレール =" & OIS0001row("ECO") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 取得年月日 =" & OIS0001row("ALLINSPECTIONDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 車籍編入年月日 =" & OIS0001row("TRANSFERDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 取得先C =" & OIS0001row("OBTAINEDCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 形式 =" & OIS0001row("MODEL") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 形式カナ =" & OIS0001row("MODELKANA") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 荷重 =" & OIS0001row("LOAD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 荷重単位 =" & OIS0001row("LOADUNIT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 容積 =" & OIS0001row("VOLUME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 容積単位 =" & OIS0001row("VOLUMEUNIT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 原籍所有者 =" & OIS0001row("ORIGINOWNERNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 名義所有者 =" & OIS0001row("OWNERNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> リース先 =" & OIS0001row("LEASENAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> リース区分 =" & OIS0001row("LEASECLASSNEMAE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 第三者使用者 =" & OIS0001row("USERNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 原常備駅 =" & OIS0001row("CURRENTSTATIONNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 臨時常備駅 =" & OIS0001row("EXTRADINARYSTATIONNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 原専用種別 =" & OIS0001row("DEDICATETYPENAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 臨時専用種別 =" & OIS0001row("EXTRADINARYTYPENAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 運用場所 =" & OIS0001row("OPERATIONBASENAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 塗色 =" & OIS0001row("COLORNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 予備1 =" & OIS0001row("RESERVE1") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 予備2 =" & OIS0001row("RESERVE2") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 次回指定年月日 =" & OIS0001row("SPECIFIEDDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 次回全検年月日(JR)  =" & OIS0001row("JRALLINSPECTIONDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 現在経年 =" & OIS0001row("PROGRESSYEAR") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 次回全検時経年 =" & OIS0001row("NEXTPROGRESSYEAR") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 次回交検年月日(JR） =" & OIS0001row("JRINSPECTIONDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 次回交検年月日 =" & OIS0001row("INSPECTIONDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 次回指定年月日(JR) =" & OIS0001row("JRSPECIFIEDDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JR車番 =" & OIS0001row("JRTANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 旧JOT車番 =" & OIS0001row("OLDTANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> OT車番 =" & OIS0001row("OTTANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JXTG車番 =" & OIS0001row("JXTGTANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> コスモ車番 =" & OIS0001row("COSMOTANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 富士石油車番 =" & OIS0001row("FUJITANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 出光昭シ車番 =" & OIS0001row("SHELLTANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 予備 =" & OIS0001row("RESERVE3") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除フラグ =" & OIS0001row("DELFLG")
        End If

        rightview.AddErrorReport(WW_ERR_MES)

    End Sub


    ''' <summary>
    ''' OIS0001tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub OIS0001tbl_UPD()

        '○ 画面状態設定
        For Each OIS0001row As DataRow In OIS0001tbl.Rows
            Select Case OIS0001row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each OIS0001INProw As DataRow In OIS0001INPtbl.Rows

            'エラーレコード読み飛ばし
            If OIS0001INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            OIS0001INProw.Item("OPERATION") = CONST_INSERT

            'KEY項目が等しい時
            For Each OIS0001row As DataRow In OIS0001tbl.Rows
                If OIS0001row("TANKNUMBER") = OIS0001INProw("TANKNUMBER") Then
                    'KEY項目以外の項目に変更がないときは「操作」の項目は空白にする
                    If OIS0001row("DELFLG") = OIS0001INProw("DELFLG") AndAlso
                        OIS0001row("ORIGINOWNERCODE") = OIS0001INProw("ORIGINOWNERCODE") AndAlso
                        OIS0001row("OWNERCODE") = OIS0001INProw("OWNERCODE") AndAlso
                        OIS0001row("LEASECODE") = OIS0001INProw("LEASECODE") AndAlso
                        OIS0001row("LEASECLASS") = OIS0001INProw("LEASECLASS") AndAlso
                        OIS0001row("AUTOEXTENTION") = OIS0001INProw("AUTOEXTENTION") AndAlso
                        OIS0001row("LEASESTYMD") = OIS0001INProw("LEASESTYMD") AndAlso
                        OIS0001row("LEASEENDYMD") = OIS0001INProw("LEASEENDYMD") AndAlso
                        OIS0001row("USERCODE") = OIS0001INProw("USERCODE") AndAlso
                        OIS0001row("CURRENTSTATIONCODE") = OIS0001INProw("CURRENTSTATIONCODE") AndAlso
                        OIS0001row("EXTRADINARYSTATIONCODE") = OIS0001INProw("EXTRADINARYSTATIONCODE") AndAlso
                        OIS0001row("USERLIMIT") = OIS0001INProw("USERLIMIT") AndAlso
                        OIS0001row("LIMITTEXTRADIARYSTATION") = OIS0001INProw("LIMITTEXTRADIARYSTATION") AndAlso
                        OIS0001row("DEDICATETYPECODE") = OIS0001INProw("DEDICATETYPECODE") AndAlso
                        OIS0001row("EXTRADINARYTYPECODE") = OIS0001INProw("EXTRADINARYTYPECODE") AndAlso
                        OIS0001row("EXTRADINARYLIMIT") = OIS0001INProw("EXTRADINARYLIMIT") AndAlso
                        OIS0001row("OPERATIONBASECODE") = OIS0001INProw("OPERATIONBASECODE") AndAlso
                        OIS0001row("COLORCODE") = OIS0001INProw("COLORCODE") AndAlso
                        OIS0001row("ENEOS") = OIS0001INProw("ENEOS") AndAlso
                        OIS0001row("ECO") = OIS0001INProw("ECO") AndAlso
                        OIS0001row("ALLINSPECTIONDATE") = OIS0001INProw("ALLINSPECTIONDATE") AndAlso
                        OIS0001row("TRANSFERDATE") = OIS0001INProw("TRANSFERDATE") AndAlso
                        OIS0001row("OBTAINEDCODE") = OIS0001INProw("OBTAINEDCODE") AndAlso
                        OIS0001row("MODEL") = OIS0001INProw("MODEL") AndAlso
                        OIS0001row("MODELKANA") = OIS0001INProw("MODELKANA") AndAlso
                        OIS0001row("LOAD") = OIS0001INProw("LOAD") AndAlso
                        OIS0001row("LOADUNIT") = OIS0001INProw("LOADUNIT") AndAlso
                        OIS0001row("VOLUME") = OIS0001INProw("VOLUME") AndAlso
                        OIS0001row("VOLUMEUNIT") = OIS0001INProw("VOLUMEUNIT") AndAlso
                        OIS0001row("ORIGINOWNERNAME") = OIS0001INProw("ORIGINOWNERNAME") AndAlso
                        OIS0001row("OWNERNAME") = OIS0001INProw("OWNERNAME") AndAlso
                        OIS0001row("LEASENAME") = OIS0001INProw("LEASENAME") AndAlso
                        OIS0001row("LEASECLASSNEMAE") = OIS0001INProw("LEASECLASSNEMAE") AndAlso
                        OIS0001row("USERNAME") = OIS0001INProw("USERNAME") AndAlso
                        OIS0001row("CURRENTSTATIONNAME") = OIS0001INProw("CURRENTSTATIONNAME") AndAlso
                        OIS0001row("EXTRADINARYSTATIONNAME") = OIS0001INProw("EXTRADINARYSTATIONNAME") AndAlso
                        OIS0001row("DEDICATETYPENAME") = OIS0001INProw("DEDICATETYPENAME") AndAlso
                        OIS0001row("EXTRADINARYTYPENAME") = OIS0001INProw("EXTRADINARYTYPENAME") AndAlso
                        OIS0001row("OPERATIONBASENAME") = OIS0001INProw("OPERATIONBASENAME") AndAlso
                        OIS0001row("COLORNAME") = OIS0001INProw("COLORNAME") AndAlso
                        OIS0001row("RESERVE1") = OIS0001INProw("RESERVE1") AndAlso
                        OIS0001row("RESERVE2") = OIS0001INProw("RESERVE2") AndAlso
                        OIS0001row("SPECIFIEDDATE") = OIS0001INProw("SPECIFIEDDATE") AndAlso
                        OIS0001row("JRALLINSPECTIONDATE") = OIS0001INProw("JRALLINSPECTIONDATE") AndAlso
                        OIS0001row("PROGRESSYEAR") = OIS0001INProw("PROGRESSYEAR") AndAlso
                        OIS0001row("NEXTPROGRESSYEAR") = OIS0001INProw("NEXTPROGRESSYEAR") AndAlso
                        OIS0001row("JRINSPECTIONDATE") = OIS0001INProw("JRINSPECTIONDATE") AndAlso
                        OIS0001row("INSPECTIONDATE") = OIS0001INProw("INSPECTIONDATE") AndAlso
                        OIS0001row("JRSPECIFIEDDATE") = OIS0001INProw("JRSPECIFIEDDATE") AndAlso
                        OIS0001row("JRTANKNUMBER") = OIS0001INProw("JRTANKNUMBER") AndAlso
                        OIS0001row("OLDTANKNUMBER") = OIS0001INProw("OLDTANKNUMBER") AndAlso
                        OIS0001row("OTTANKNUMBER") = OIS0001INProw("OTTANKNUMBER") AndAlso
                        OIS0001row("JXTGTANKNUMBER") = OIS0001INProw("JXTGTANKNUMBER") AndAlso
                        OIS0001row("COSMOTANKNUMBER") = OIS0001INProw("COSMOTANKNUMBER") AndAlso
                        OIS0001row("FUJITANKNUMBER") = OIS0001INProw("FUJITANKNUMBER") AndAlso
                        OIS0001row("SHELLTANKNUMBER") = OIS0001INProw("SHELLTANKNUMBER") AndAlso
                        OIS0001row("RESERVE3") = OIS0001INProw("RESERVE3") AndAlso
                        OIS0001INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA Then
                    Else
                        'KEY項目以外の項目に変更がある時は「操作」の項目を「更新」に設定する
                        OIS0001INProw("OPERATION") = CONST_UPDATE
                        Exit For
                    End If

                    Exit For

                End If
            Next
        Next

        '○ 変更有無判定　&　入力値反映
        For Each OIS0001INProw As DataRow In OIS0001INPtbl.Rows
            Select Case OIS0001INProw("OPERATION")
                Case CONST_UPDATE
                    TBL_UPDATE_SUB(OIS0001INProw)
                Case CONST_INSERT
                    TBL_INSERT_SUB(OIS0001INProw)
                Case CONST_PATTERNERR
                    '関連チェックエラーの場合、キーが変わるため、行追加してエラーレコードを表示させる
                    TBL_INSERT_SUB(OIS0001INProw)
                Case C_LIST_OPERATION_CODE.ERRORED
                    TBL_ERR_SUB(OIS0001INProw)
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="OIS0001INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef OIS0001INProw As DataRow)

        For Each OIS0001row As DataRow In OIS0001tbl.Rows

            '同一レコードか判定
            If OIS0001INProw("TANKNUMBER") = OIS0001row("TANKNUMBER") Then
                '画面入力テーブル項目設定
                OIS0001INProw("LINECNT") = OIS0001row("LINECNT")
                OIS0001INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                OIS0001INProw("UPDTIMSTP") = OIS0001row("UPDTIMSTP")
                OIS0001INProw("SELECT") = 1
                OIS0001INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIS0001row.ItemArray = OIS0001INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 追加予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="OIS0001INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef OIS0001INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim OIS0001row As DataRow = OIS0001tbl.NewRow
        OIS0001row.ItemArray = OIS0001INProw.ItemArray

        OIS0001row("LINECNT") = OIS0001tbl.Rows.Count + 1
        If OIS0001INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
            OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        Else
            OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
        End If

        OIS0001row("UPDTIMSTP") = "0"
        OIS0001row("SELECT") = 1
        OIS0001row("HIDDEN") = 0

        OIS0001tbl.Rows.Add(OIS0001row)

    End Sub


    ''' <summary>
    ''' エラーデータの一覧登録時処理
    ''' </summary>
    ''' <param name="OIS0001INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_ERR_SUB(ByRef OIS0001INProw As DataRow)

        For Each OIS0001row As DataRow In OIS0001tbl.Rows

            '同一レコードか判定
            If OIS0001INProw("TANKNUMBER") = OIS0001row("TANKNUMBER") Then
                '画面入力テーブル項目設定
                OIS0001INProw("LINECNT") = OIS0001row("LINECNT")
                OIS0001INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                OIS0001INProw("UPDTIMSTP") = OIS0001row("UPDTIMSTP")
                OIS0001INProw("SELECT") = 1
                OIS0001INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIS0001row.ItemArray = OIS0001INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 名称取得
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByVal I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String, Optional ByVal I_SUBCODE As String = "1")

        O_TEXT = ""
        O_RTN = ""

        If I_VALUE = "" Then
            O_RTN = C_MESSAGE_NO.NORMAL
            Exit Sub
        End If
        Dim prmData As New Hashtable

        Try
            Select Case I_FIELD
                Case "CAMPCODE"         '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "DELFLG"           '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.CreateFixValueParam(work.WF_SEL_CAMPCODE.Text, "DELFLG"))

            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
