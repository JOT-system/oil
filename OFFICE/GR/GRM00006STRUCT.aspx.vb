Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' 構造マスタ入力（実行）
''' </summary>
''' <remarks></remarks>
Public Class GRM00006STRUCT
    Inherits Page

    '○ 検索結果格納Table
    Private M00006tbl As DataTable                          '一覧格納用テーブル
    Private M00006INPtbl As DataTable                       'チェック用テーブル
    Private M00006UPDtbl As DataTable                       '更新用テーブル

    Private Const CONST_DISPROWCOUNT As Integer = 45        '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 10         'マウススクロール時稼働行数
    Private Const CONST_BLANK_LINE As Integer = 10          '空行数

    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite            'ログ出力
    Private CS0013ProfView As New CS0013ProfView            'Tableオブジェクト展開
    Private CS0020JOURNAL As New CS0020JOURNAL              '更新ジャーナル出力
    Private CS0023XLSUPLOAD As New CS0023XLSUPLOAD          'XLSアップロード
    Private CS0025AUTHORget As New CS0025AUTHORget          '権限チェック(マスタチェック)
    Private CS0026TBLSORT As New CS0026TBLSORT              '表示画面情報ソート
    Private CS0030REPORT As New CS0030REPORT                '帳票出力
    Private CS0050SESSION As New CS0050SESSION              'セッション情報操作処理

    '○ 共通処理結果
    Private WW_ERR_SW As String = ""
    Private WW_RTN_SW As String = ""
    Private WW_DUMMY As String = ""

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
                    If Not Master.RecoverTable(M00006tbl) Then
                        Exit Sub
                    End If

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonExtract"         '絞り込みボタン押下
                            WF_ButtonExtract_Click()
                        Case "WF_ButtonUPDATE"          'DB更新ボタン押下
                            WF_ButtonUPDATE_Click()
                        Case "WF_ButtonCSV"             'ダウンロードボタン押下
                            WF_ButtonDownload_Click()
                        Case "WF_ButtonPrint"           '一覧印刷ボタン押下
                            WF_ButtonPrint_Click()
                        Case "WF_ButtonEND"             '終了ボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_ButtonFIRST"           '先頭頁ボタン押下
                            WF_ButtonFIRST_Click()
                        Case "WF_ButtonLAST"            '最終頁ボタン押下
                            WF_ButtonLAST_Click()
                        Case "WF_GridDBclick"           'GridViewダブルクリック
                            WF_Grid_DBClick()
                        Case "WF_MouseWheelUp"          'マウスホイール(Up)
                            WF_Grid_Scroll()
                        Case "WF_MouseWheelDown"        'マウスホイール(Down)
                            WF_Grid_Scroll()
                        Case "WF_EXCEL_UPLOAD"          'ファイルアップロード
                            WF_FILEUPLOAD()
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

                    '○ 一覧再表示処理
                    DisplayGrid()
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

        Finally
            '○ 格納Table Close
            If Not IsNothing(M00006tbl) Then
                M00006tbl.Clear()
                M00006tbl.Dispose()
                M00006tbl = Nothing
            End If

            If Not IsNothing(M00006INPtbl) Then
                M00006INPtbl.Clear()
                M00006INPtbl.Dispose()
                M00006INPtbl = Nothing
            End If

            If Not IsNothing(M00006UPDtbl) Then
                M00006UPDtbl.Clear()
                M00006UPDtbl.Dispose()
                M00006UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = GRM00006WRKINC.MAPID

        WF_USERID.Focus()
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""
        WF_RightboxOpen.Value = ""
        leftview.activeListBox()
        rightview.resetindex()

        '○ 画面の値設定
        WW_MAPValueSet()

        '○ 右ボックスへの値設定
        rightview.MAPID = Master.MAPID
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_DUMMY)

        '○ GridView初期設定
        GridViewInitialize()

        '○ 詳細画面初期設定
        DetailInitialize()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.M00006S Then
            'Grid情報保存先のファイル名
            Master.createXMLSaveFile()
        End If

        '○ ヘルプボタン非表示
        Master.dispHelp = False

        '○ ファイルドロップ有無
        Master.eventDrop = True

    End Sub

    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        '○ 画面表示データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(M00006tbl)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(M00006tbl)

        TBLview.RowFilter = "TITLEKBN = 'H' and LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Horizontal
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.CS0013ProfView()
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If

        '○ 先頭行に合わせる
        WF_GridPosition.Text = "1"

        TBLview.Dispose()
        TBLview = Nothing

    End Sub

    ''' <summary>
    ''' 画面表示データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MAPDataGet(ByVal SQLcon As SqlConnection)

        If IsNothing(M00006tbl) Then
            M00006tbl = New DataTable
        End If

        If M00006tbl.Columns.Count <> 0 Then
            M00006tbl.Columns.Clear()
        End If

        M00006tbl.Clear()

        '○ 検索SQL

        Dim SQLStr As String =
              " SELECT" _
            & "    0                                               AS LINECNT" _
            & "    , ''                                            AS OPERATION" _
            & "    , CAST(M006.UPDTIMSTP AS bigint)                AS TIMSTP" _
            & "    , 1                                             AS 'SELECT'" _
            & "    , 0                                             AS HIDDEN" _
            & "    , 'I'                                           AS TITLEKBN" _
            & "    , ISNULL(RTRIM(M006.USERID), '')                AS USERID" _
            & "    , ''                                            AS USERNAMES" _
            & "    , ISNULL(RTRIM(M006.CAMPCODE), '')              AS CAMPCODE" _
            & "    , ''                                            AS CAMPNAMES" _
            & "    , ISNULL(RTRIM(M006.OBJECT), '')                AS OBJECT" _
            & "    , ''                                            AS OBJECTNAMES" _
            & "    , ISNULL(RTRIM(M006.STRUCT), '')                AS STRUCT" _
            & "    , ISNULL(M006.SEQ, 0)                           AS SEQ" _
            & "    , ''                                            AS CHKSEQ" _
            & "    , ISNULL(RTRIM(M006.CODE), '')                  AS CODE" _
            & "    , ''                                            AS CODENAMES" _
            & "    , ISNULL(FORMAT(M006.STYMD, 'yyyy/MM/dd'), '')  AS STYMD" _
            & "    , ISNULL(FORMAT(M006.ENDYMD, 'yyyy/MM/dd'), '') AS ENDYMD" _
            & "    , ISNULL(RTRIM(M006.DELFLG), '')                AS DELFLG" _
            & " FROM" _
            & "    M0006_STRUCT M006" _
            & " WHERE" _
            & "    (M006.USERID       = '" & C_DEFAULT_DATAKEY & "'" _
            & "    OR  M006.USERID   IN (" _
            & "        SELECT" _
            & "            S006.CODE" _
            & "        FROM" _
            & "            S0005_AUTHOR S005" _
            & "            INNER JOIN S0006_ROLE S006" _
            & "                ON  S006.CAMPCODE = S005.CAMPCODE" _
            & "                AND S006.OBJECT   = S005.OBJECT" _
            & "                AND S006.ROLE     = S005.ROLE" _
            & "                AND S006.STYMD   <= @P6" _
            & "                AND S006.ENDYMD  >= @P6" _
            & "                AND S006.DELFLG  <> @P7" _
            & "        WHERE" _
            & "            S005.USERID        = @P1" _
            & "            AND S005.CAMPCODE IN (@P2, '" & C_DEFAULT_DATAKEY & "')" _
            & "            AND S005.OBJECT    = @P3" _
            & "            AND S005.STYMD    <= @P6" _
            & "            AND S005.ENDYMD   >= @P6" _
            & "            AND S005.DELFLG   <> @P7))" _
            & "    AND M006.CAMPCODE IN (@P2, '" & C_DEFAULT_DATAKEY & "')" _
            & "    AND M006.STYMD    <= @P4" _
            & "    AND M006.ENDYMD   >= @P5" _
            & "    AND M006.DELFLG   <> @P7"

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        'オブジェクト
        If Not String.IsNullOrEmpty(work.WF_SEL_OBJECT.Text) Then
            SQLStr &= String.Format("    AND M006.OBJECT    = '{0}'", work.WF_SEL_OBJECT.Text)
        End If
        '構造コード
        If Not String.IsNullOrEmpty(work.WF_SEL_STRUCT.Text) Then
            SQLStr &= String.Format("    AND M006.STRUCT    = '{0}'", work.WF_SEL_STRUCT.Text)
        End If

        SQLStr &=
              " ORDER BY" _
            & "    M006.USERID" _
            & "    , M006.CAMPCODE" _
            & "    , M006.OBJECT" _
            & "    , M006.STRUCT" _
            & "    , M006.STYMD" _
            & "    , M006.SEQ" _
            & "    , M006.CODE"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        'ユーザーID
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)        '会社コード
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 20)        'オブジェクト
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.Date)                '有効年月日(To)
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.Date)                '有効年月日(From)
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.Date)                '現在日付
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.NVarChar, 1)         '削除フラグ

                PARA1.Value = work.WF_SEL_USERID.Text
                PARA2.Value = work.WF_SEL_CAMPCODE.Text
                PARA3.Value = C_ROLE_VARIANT.USER_PROFILE
                PARA4.Value = work.WF_SEL_ENDYMD.Text
                PARA5.Value = work.WF_SEL_STYMD.Text
                PARA6.Value = Date.Now
                PARA7.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        M00006tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    M00006tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                Dim WW_SAVEKEY As String = ""
                For Each M00006row As DataRow In M00006tbl.Rows
                    'ヘッダーを設定
                    Dim WW_KEY As String = M00006row("USERID") & "," & M00006row("CAMPCODE") & "," & M00006row("OBJECT") & "," & M00006row("STRUCT") & "," & M00006row("STYMD")
                    If WW_SAVEKEY <> WW_KEY Then
                        i += 1
                        M00006row("TITLEKBN") = "H"
                        WW_SAVEKEY = WW_KEY
                    End If
                    M00006row("LINECNT") = i

                    '名称取得
                    CODENAME_get("USERID", M00006row("USERID"), M00006row("USERNAMES"), WW_DUMMY)                           'ユーザーID
                    CODENAME_get("CAMPCODE", M00006row("CAMPCODE"), M00006row("CAMPNAMES"), WW_DUMMY)                       '会社コード
                    CODENAME_get("OBJECT", M00006row("OBJECT"), M00006row("OBJECTNAMES"), WW_DUMMY)                         'オブジェクト
                    CODENAME_get("CODE", M00006row("CODE"), M00006row("CODENAMES"), WW_DUMMY, M00006row("OBJECT"))          'コード
                Next
            End Using
        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "M0006_STRUCT SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:M0006_STRUCT Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
        End Try

    End Sub

    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each M00006row As DataRow In M00006tbl.Rows
            If M00006row("HIDDEN") = 0 AndAlso M00006row("TITLEKBN") = "H" Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                M00006row("SELECT") = WW_DataCNT
            End If
        Next

        '○ 表示LINECNT取得
        If WF_GridPosition.Text = "" Then
            WW_GridPosition = 1
        Else
            Try
                Integer.TryParse(WF_GridPosition.Text, WW_GridPosition)
            Catch ex As Exception
                WW_GridPosition = 1
            End Try
        End If

        '○ 表示格納位置決定

        '表示開始_格納位置決定(次頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelUp" Then
            If (WW_GridPosition + CONST_SCROLLCOUNT) <= WW_DataCNT Then
                WW_GridPosition += CONST_SCROLLCOUNT
            End If
        End If

        '表示開始_格納位置決定(前頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelDown" Then
            If (WW_GridPosition - CONST_SCROLLCOUNT) > 0 Then
                WW_GridPosition -= CONST_SCROLLCOUNT
            Else
                WW_GridPosition = 1
            End If
        End If

        '○ 画面(GridView)表示
        Dim TBLview As DataView = New DataView(M00006tbl)

        '○ ソート
        TBLview.Sort = "LINECNT"
        TBLview.RowFilter = "HIDDEN = 0 and TITLEKBN = 'H'" _
            & " and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DISPROWCOUNT).ToString()

        '○ 一覧作成
        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Horizontal
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.CS0013ProfView()

        '○ クリア
        If TBLview.Count = 0 Then
            WF_GridPosition.Text = "1"
        Else
            WF_GridPosition.Text = TBLview.Item(0)("SELECT")
        End If

        TBLview.Dispose()
        TBLview = Nothing

    End Sub


    ''' <summary>
    ''' 絞り込みボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonExtract_Click()

        '○ 使用禁止文字排除
        Master.eraseCharToIgnore(WF_SELOBJECT.Text)

        '○ 名称取得
        CODENAME_get("OBJECT", WF_SELOBJECT.Text, WF_SELOBJECT_TEXT.Text, WW_DUMMY)

        '○ 絞り込み操作(GridView明細Hidden設定)
        For Each M00006row As DataRow In M00006tbl.Rows

            '一度非表示にする
            M00006row("HIDDEN") = 1

            Dim WW_HANTEI As Boolean = True

            'オブジェクトによる絞込判定
            If WF_SELOBJECT.Text <> "" AndAlso
                WF_SELOBJECT.Text <> M00006row("OBJECT") Then
                WW_HANTEI = False
            End If

            '画面(GridView)のHIDDENに結果格納
            If WW_HANTEI Then
                M00006row("HIDDEN") = 0
            End If
        Next

        '○ 画面先頭を表示
        WF_GridPosition.Text = "1"

        '○ 画面表示データ保存
        Master.SaveTable(M00006tbl)

        '○ メッセージ表示
        Master.output(C_MESSAGE_NO.DATA_FILTER_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        WF_USERID.Focus()

    End Sub


    ''' <summary>
    ''' DB更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        '○ 関連チェック
        RelatedCheck(WW_ERR_SW)

        '○ 配列再設定(削除分優先)
        CS0026TBLSORT.TABLE = M00006tbl
        CS0026TBLSORT.SORTING = "LINECNT, DELFLG DESC, SEQ, CODE"
        CS0026TBLSORT.FILTER = ""
        CS0026TBLSORT.sort(M00006tbl)

        If isNormal(WW_ERR_SW) Then
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                '構造マスタ更新
                UpdateStructMaster(SQLcon)
            End Using
        End If

        '○ 配列再設定(削除分は除く)
        CS0026TBLSORT.TABLE = M00006tbl
        CS0026TBLSORT.SORTING = "LINECNT, SEQ, CODE"
        CS0026TBLSORT.FILTER = "DELFLG <> '" & C_DELETE_FLG.DELETE & "'"
        CS0026TBLSORT.sort(M00006tbl)

        '○ 画面表示データ保存
        Master.SaveTable(M00006tbl)

        '○ 詳細画面クリア
        If isNormal(WW_ERR_SW) Then
            DetailBoxClear()
        End If

        '○ メッセージ表示
        If Not isNormal(WW_ERR_SW) Then
            Master.output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
        End If

        WF_USERID.Focus()

    End Sub

    ''' <summary>
    ''' 登録データ関連チェック
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub RelatedCheck(ByRef O_RTN As String)

        '○ 初期値設定
        O_RTN = C_MESSAGE_NO.NORMAL
        rightview.setErrorReport("")

        Dim WW_LINE_ERR As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_DATE_ST As Date
        Dim WW_DATE_END As Date
        Dim WW_DATE_ST2 As Date
        Dim WW_DATE_END2 As Date

        '○ 同一KEY内一致チェック
        For Each M00006row As DataRow In M00006tbl.Rows

            '読み飛ばし
            If (M00006row("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING AndAlso
                M00006row("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED) OrElse
                M00006row("DELFLG") = C_DELETE_FLG.DELETE OrElse
                M00006row("STYMD") = "" OrElse
                M00006row("TITLEKBN") <> "H" Then
                Continue For
            End If

            WW_LINE_ERR = ""

            For Each M00006chk As DataRow In M00006tbl.Rows

                '同一KEY以外は読み飛ばし
                If M00006row("USERID") <> M00006chk("USERID") OrElse
                    M00006row("CAMPCODE") <> M00006chk("CAMPCODE") OrElse
                    M00006row("OBJECT") <> M00006chk("OBJECT") OrElse
                    M00006row("STRUCT") <> M00006chk("STRUCT") OrElse
                    M00006row("STYMD") <> M00006chk("STYMD") OrElse
                    M00006chk("DELFLG") = C_DELETE_FLG.DELETE Then
                    Continue For
                End If

                Try
                    Date.TryParse(M00006row("ENDYMD"), WW_DATE_END)
                    Date.TryParse(M00006chk("ENDYMD"), WW_DATE_END2)
                Catch ex As Exception
                    Master.output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
                    Exit Sub
                End Try

                '終了日チェック
                If WW_DATE_END <> WW_DATE_END2 Then
                    WW_CheckMES1 = "・エラー(終了日が一致しないデータ)が存在します。"
                    WW_CheckMES2 = WW_DATE_END.ToString("yyyy/MM/dd") & ":" & WW_DATE_END2.ToString("yyyy/MM/dd")
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, M00006row)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINE_ERR = "ERR"
                    Exit For
                End If
            Next

            If WW_LINE_ERR = "" Then
                M00006row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            Else
                M00006row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

        '○ 日付重複チェック
        For Each M00006row As DataRow In M00006tbl.Rows

            '読み飛ばし
            If (M00006row("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING AndAlso
                M00006row("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED) OrElse
                M00006row("DELFLG") = C_DELETE_FLG.DELETE OrElse
                M00006row("STYMD") = "" Then
                Continue For
            End If

            WW_LINE_ERR = ""

            '期間重複チェック
            For Each M00006chk As DataRow In M00006tbl.Rows

                '同一KEY以外は読み飛ばし
                If M00006row("USERID") <> M00006chk("USERID") OrElse
                    M00006row("CAMPCODE") <> M00006chk("CAMPCODE") OrElse
                    M00006row("OBJECT") <> M00006chk("OBJECT") OrElse
                    M00006row("STRUCT") <> M00006chk("STRUCT") OrElse
                    M00006chk("DELFLG") = C_DELETE_FLG.DELETE Then
                    Continue For
                End If

                '期間変更対象は読み飛ばし
                If M00006row("STYMD") = M00006chk("STYMD") Then
                    Continue For
                End If

                Try
                    Date.TryParse(M00006row("STYMD"), WW_DATE_ST)
                    Date.TryParse(M00006row("ENDYMD"), WW_DATE_END)
                    Date.TryParse(M00006chk("STYMD"), WW_DATE_ST2)
                    Date.TryParse(M00006chk("ENDYMD"), WW_DATE_END2)
                Catch ex As Exception
                    Master.output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
                    Exit Sub
                End Try

                '開始日チェック
                If WW_DATE_ST >= WW_DATE_ST2 AndAlso WW_DATE_ST <= WW_DATE_END2 Then
                    WW_CheckMES1 = "・エラー(期間重複)が存在します。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, M00006row)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINE_ERR = "ERR"
                    Exit For
                End If

                '終了日チェック
                If WW_DATE_END >= WW_DATE_ST2 AndAlso WW_DATE_END <= WW_DATE_END2 Then
                    WW_CheckMES1 = "・エラー(期間重複)が存在します。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, M00006row)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINE_ERR = "ERR"
                    Exit For
                End If
            Next

            If WW_LINE_ERR = "" Then
                M00006row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            Else
                M00006row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

    End Sub

    ''' <summary>
    ''' 構造マスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateStructMaster(ByVal SQLcon As SqlConnection)

        '○ ＤＢ更新
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;" _
            & "    SET @hensuu = 0 ;" _
            & " DECLARE hensuu CURSOR FOR" _
            & "    SELECT" _
            & "        CAST(UPDTIMSTP AS bigint) AS hensuu" _
            & "    FROM" _
            & "        M0006_STRUCT" _
            & "    WHERE" _
            & "        USERID       = @P1" _
            & "        AND CAMPCODE = @P2" _
            & "        AND OBJECT   = @P3" _
            & "        AND STRUCT   = @P4" _
            & "        AND SEQ      = @P5" _
            & "        AND STYMD    = @P7 ;" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE M0006_STRUCT" _
            & "    SET" _
            & "        CODE         = @P6     , ENDYMD     = @P8" _
            & "        , DELFLG     = @P21    , UPDYMD     = @P23" _
            & "        , UPDUSER    = @P24    , UPDTERMID  = @P25" _
            & "        , RECEIVEYMD = @P26" _
            & "    WHERE" _
            & "        USERID       = @P1" _
            & "        AND CAMPCODE = @P2" _
            & "        AND OBJECT   = @P3" _
            & "        AND STRUCT   = @P4" _
            & "        AND SEQ      = @P5" _
            & "        AND STYMD    = @P7 ;" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO M0006_STRUCT" _
            & "        (USERID        , CAMPCODE    , OBJECT" _
            & "        , STRUCT       , SEQ         , CODE" _
            & "        , STYMD        , ENDYMD      , CODENAMES" _
            & "        , CODENAMEL    , GRCODE01    , GRCODE02" _
            & "        , GRCODE03     , GRCODE04    , GRCODE05" _
            & "        , GRCODE06     , GRCODE07    , GRCODE08" _
            & "        , GRCODE09     , GRCODE10    , DELFLG" _
            & "        , INITYMD      , UPDYMD      , UPDUSER" _
            & "        , UPDTERMID    , RECEIVEYMD)" _
            & "    VALUES" _
            & "        (@P1      , @P2     , @P3" _
            & "        , @P4     , @P5     , @P6" _
            & "        , @P7     , @P8     , @P9" _
            & "        , @P10    , @P11    , @P12" _
            & "        , @P13    , @P14    , @P15" _
            & "        , @P16    , @P17    , @P18" _
            & "        , @P19    , @P20    , @P21" _
            & "        , @P22    , @P23    , @P24" _
            & "        , @P25    , @P26) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
              " SELECT" _
            & "    USERID" _
            & "    , CAMPCODE" _
            & "    , OBJECT" _
            & "    , STRUCT" _
            & "    , SEQ" _
            & "    , CODE" _
            & "    , STYMD" _
            & "    , ENDYMD" _
            & "    , CODENAMES" _
            & "    , CODENAMEL" _
            & "    , GRCODE01" _
            & "    , GRCODE02" _
            & "    , GRCODE03" _
            & "    , GRCODE04" _
            & "    , GRCODE05" _
            & "    , GRCODE06" _
            & "    , GRCODE07" _
            & "    , GRCODE08" _
            & "    , GRCODE09" _
            & "    , GRCODE10" _
            & "    , DELFLG" _
            & "    , INITYMD" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & "    , CAST(UPDTIMSTP AS bigint) TIMSTP" _
            & " FROM" _
            & "    M0006_STRUCT" _
            & " WHERE" _
            & "    USERID       = @P1" _
            & "    AND CAMPCODE = @P2" _
            & "    AND OBJECT   = @P3" _
            & "    AND STRUCT   = @P4" _
            & "    AND SEQ      = @P5" _
            & "    AND CODE     = @P6" _
            & "    AND STYMD    = @P7"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)            'ユーザID
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)            '会社コード
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 20)            'オブジェクト
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 50)            '構造コード
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.Int)                     '表示順番
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 20)            'コード
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.Date)                    '開始年月日
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.Date)                    '終了年月日
                Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", SqlDbType.NVarChar, 20)            'コード名称(短)
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 50)          'コード名称(長)
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 20)          'グループコード1
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 20)          'グループコード2
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 20)          'グループコード3
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 20)          'グループコード4
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 20)          'グループコード5
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.NVarChar, 20)          'グループコード6
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.NVarChar, 20)          'グループコード7
                Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.NVarChar, 20)          'グループコード8
                Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.NVarChar, 20)          'グループコード9
                Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.NVarChar, 20)          'グループコード10
                Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.NVarChar, 1)           '削除フラグ
                Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", SqlDbType.DateTime)              '登録年月日
                Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", SqlDbType.DateTime)              '更新年月日
                Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", SqlDbType.NVarChar, 20)          '更新ユーザＩＤ
                Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", SqlDbType.NVarChar, 30)          '更新端末
                Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", SqlDbType.DateTime)              '集信日時

                Dim JPARA1 As SqlParameter = SQLcmdJnl.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        'ユーザID
                Dim JPARA2 As SqlParameter = SQLcmdJnl.Parameters.Add("@P2", SqlDbType.NVarChar, 20)        '会社コード
                Dim JPARA3 As SqlParameter = SQLcmdJnl.Parameters.Add("@P3", SqlDbType.NVarChar, 20)        'オブジェクト
                Dim JPARA4 As SqlParameter = SQLcmdJnl.Parameters.Add("@P4", SqlDbType.NVarChar, 50)        '構造コード
                Dim JPARA5 As SqlParameter = SQLcmdJnl.Parameters.Add("@P5", SqlDbType.Int)                 '表示順番
                Dim JPARA6 As SqlParameter = SQLcmdJnl.Parameters.Add("@P6", SqlDbType.NVarChar, 20)        'コード
                Dim JPARA7 As SqlParameter = SQLcmdJnl.Parameters.Add("@P7", SqlDbType.Date)                '開始年月日

                For Each M00006row As DataRow In M00006tbl.Rows
                    If Trim(M00006row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING OrElse
                        Trim(M00006row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING Then
                        Dim WW_DATENOW As Date = Date.Now

                        'ＤＢ更新
                        PARA1.Value = M00006row("USERID")
                        PARA2.Value = M00006row("CAMPCODE")
                        PARA3.Value = M00006row("OBJECT")
                        PARA4.Value = M00006row("STRUCT")
                        PARA5.Value = M00006row("SEQ")
                        PARA6.Value = M00006row("CODE")
                        PARA7.Value = M00006row("STYMD")
                        PARA8.Value = M00006row("ENDYMD")
                        PARA9.Value = ""
                        PARA10.Value = ""
                        PARA11.Value = ""
                        PARA12.Value = ""
                        PARA13.Value = ""
                        PARA14.Value = ""
                        PARA15.Value = ""
                        PARA16.Value = ""
                        PARA17.Value = ""
                        PARA18.Value = ""
                        PARA19.Value = ""
                        PARA20.Value = ""
                        PARA21.Value = M00006row("DELFLG")
                        PARA22.Value = WW_DATENOW
                        PARA23.Value = WW_DATENOW
                        PARA24.Value = Master.USERID
                        PARA25.Value = Master.USERTERMID
                        PARA26.Value = C_DEFAULT_YMD

                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                        M00006row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                        '更新ジャーナル出力
                        JPARA1.Value = M00006row("USERID")
                        JPARA2.Value = M00006row("CAMPCODE")
                        JPARA3.Value = M00006row("OBJECT")
                        JPARA4.Value = M00006row("STRUCT")
                        JPARA5.Value = M00006row("SEQ")
                        JPARA6.Value = M00006row("CODE")
                        JPARA7.Value = M00006row("STYMD")

                        Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                            If IsNothing(M00006UPDtbl) Then
                                M00006UPDtbl = New DataTable

                                For index As Integer = 0 To SQLdr.FieldCount - 1
                                    M00006UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                Next
                            End If

                            M00006UPDtbl.Clear()
                            M00006UPDtbl.Load(SQLdr)
                        End Using

                        For Each M00006UPDrow As DataRow In M00006UPDtbl.Rows
                            CS0020JOURNAL.TABLENM = "M0006_STRUCT"
                            CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                            CS0020JOURNAL.ROW = M00006UPDrow
                            CS0020JOURNAL.CS0020JOURNAL()
                            If Not isNormal(CS0020JOURNAL.ERR) Then
                                Master.output(CS0020JOURNAL.ERR, C_MESSAGE_TYPE.ABORT, "CS0020JOURNAL JOURNAL")

                                CS0011LOGWrite.INFSUBCLASS = "MAIN"                     'SUBクラス名
                                CS0011LOGWrite.INFPOSI = "CS0020JOURNAL JOURNAL"
                                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                                CS0011LOGWrite.TEXT = "CS0020JOURNAL Call Err!"
                                CS0011LOGWrite.MESSAGENO = CS0020JOURNAL.ERR
                                CS0011LOGWrite.CS0011LOGWrite()                         'ログ出力
                                Exit Sub
                            End If
                        Next
                    End If
                Next
            End Using
        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "M0006_STRUCT UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:M0006_STRUCT UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        Master.output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub


    ''' <summary>
    ''' ﾀﾞｳﾝﾛｰﾄﾞ(Excel出力)ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDownload_Click()

        '○ 帳票出力
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                       '画面ID
        CS0030REPORT.REPORTID = rightview.getReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "XLSX"                           '出力ファイル形式
        CS0030REPORT.TBLDATA = M00006tbl                        'データ参照  Table
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            If CS0030REPORT.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
                Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR)
            Else
                Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORT")
            End If
            Exit Sub
        End If

        '○ 別画面でExcelを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

        WF_USERID.Focus()

    End Sub

    ''' <summary>
    ''' ﾀﾞｳﾝﾛｰﾄﾞ(PDF出力)・一覧印刷ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonPrint_Click()

        '○ 帳票出力
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                       '画面ID
        CS0030REPORT.REPORTID = rightview.getReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "pdf"                            '出力ファイル形式
        CS0030REPORT.TBLDATA = M00006tbl                        'データ参照Table
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            If CS0030REPORT.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
                Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR)
            Else
                Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORT")
            End If
            Exit Sub
        End If

        '○ 別画面でPDFを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_PDFPrint();", True)

        WF_USERID.Focus()

    End Sub


    ''' <summary>
    ''' 終了ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        Master.transitionPrevPage()

    End Sub


    ''' <summary>
    ''' 先頭頁ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonFIRST_Click()

        '○ 先頭頁に移動
        WF_GridPosition.Text = "1"
        WF_USERID.Focus()

    End Sub

    ''' <summary>
    ''' 最終頁ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonLAST_Click()

        '○ ソート
        Dim TBLview As New DataView(M00006tbl)
        TBLview.RowFilter = "HIDDEN = 0 and TITLEKBN = 'H'"

        '○ 最終頁に移動
        If TBLview.Count Mod 10 = 0 Then
            WF_GridPosition.Text = TBLview.Count - (TBLview.Count Mod 10)
        Else
            WF_GridPosition.Text = TBLview.Count - (TBLview.Count Mod 10) + 1
        End If

        WF_USERID.Focus()

        TBLview.Dispose()
        TBLview = Nothing

    End Sub


    ' ******************************************************************************
    ' ***  一覧表示(GridView)関連操作                                            ***
    ' ******************************************************************************

    ''' <summary>
    ''' 一覧画面-明細行ダブルクリック時処理 (GridView ---> detailbox)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_DBClick()

        Dim WW_LINECNT As Integer = 0
        Dim WW_FIELD_OBJ As Object = Nothing
        Dim WW_VALUE As String = ""
        Dim WW_TEXT As String = ""

        '○ LINECNT取得
        Try
            Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT)
        Catch ex As Exception
            Exit Sub
        End Try

        For i As Integer = 0 To M00006tbl.Rows.Count - 1
            If M00006tbl.Rows(i)("LINECNT") = WW_LINECNT Then
                WW_LINECNT = i
                Exit For
            End If
        Next

        '選択行
        WF_Sel_LINECNT.Text = M00006tbl.Rows(WW_LINECNT)("LINECNT")

        'ユーザーID
        WF_USERID.Text = M00006tbl.Rows(WW_LINECNT)("USERID")
        CODENAME_get("USERID", WF_USERID.Text, WF_USERID_TEXT.Text, WW_DUMMY)

        '会社コード
        WF_CAMPCODE.Text = M00006tbl.Rows(WW_LINECNT)("CAMPCODE")
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)

        'オブジェクト
        WF_OBJECT.Text = M00006tbl.Rows(WW_LINECNT)("OBJECT")
        CODENAME_get("OBJECT", WF_OBJECT.Text, WF_OBJECT_TEXT.Text, WW_DUMMY)

        '構造コード
        WF_STRUCT.Text = M00006tbl.Rows(WW_LINECNT)("STRUCT")

        '有効年月日
        WF_STYMD.Text = M00006tbl.Rows(WW_LINECNT)("STYMD")
        WF_ENDYMD.Text = M00006tbl.Rows(WW_LINECNT)("ENDYMD")

        '削除
        WF_DELFLG.Text = M00006tbl.Rows(WW_LINECNT)("DELFLG")
        CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_DUMMY)

        '○ 選択した対象データを抽出する
        Master.CreateEmptyTable(M00006INPtbl)
        CS0026TBLSORT.TABLE = M00006tbl
        CS0026TBLSORT.SORTING = "SEQ, CODE"
        CS0026TBLSORT.FILTER = "USERID = '" & WF_USERID.Text & "'" _
            & " and CAMPCODE = '" & WF_CAMPCODE.Text & "'" _
            & " and OBJECT = '" & WF_OBJECT.Text & "'" _
            & " and STRUCT = '" & WF_STRUCT.Text & "'" _
            & " and STYMD = '" & WF_STYMD.Text & "'" _
            & " and DELFLG = '" & C_DELETE_FLG.ALIVE & "'"
        CS0026TBLSORT.sort(M00006INPtbl)

        '○ +10行分空行作成
        For i As Integer = 0 To CONST_BLANK_LINE - 1
            Dim M00006INProw As DataRow = M00006INPtbl.NewRow

            For Each M00006INPcol As DataColumn In M00006INPtbl.Columns
                Select Case M00006INPcol.ColumnName
                    Case "LINECNT", "HIDDEN", "SEQ", "TIMSTP"
                        M00006INProw.Item(M00006INPcol) = 0
                    Case "SELECT"
                        M00006INProw.Item(M00006INPcol) = 1
                    Case "TITLEKBN"
                        M00006INProw.Item(M00006INPcol) = "I"
                    Case Else
                        M00006INProw.Item(M00006INPcol) = ""
                End Select
            Next

            M00006INPtbl.Rows.Add(M00006INProw)
        Next

        '○ 明細へデータ貼り付け
        WF_Repeater.Visible = True
        WF_Repeater.DataSource = M00006INPtbl
        WF_Repeater.DataBind()

        '○ 明細作成
        For i As Integer = 0 To WF_Repeater.Items.Count - 1
            '項番
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_LINECNT"), Label).Text = (i + 1).ToString()

            'SEQ
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_SEQ"), TextBox).Text = If(M00006INPtbl.Rows(i)("CODE") <> "", M00006INPtbl.Rows(i)("SEQ"), "")

            WW_VALUE = M00006INPtbl.Rows(i)("CODE")
            CODENAME_get("CODE", WW_VALUE, WW_TEXT, WW_DUMMY, WF_OBJECT.Text)

            'コード
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_CODE"), TextBox).Text = WW_VALUE
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_CODE_TEXT"), Label).Text = WW_TEXT
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_CODE"), TextBox).Attributes.Remove("ondblclick")
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_CODE"), TextBox).Attributes.Add("ondblclick", "REF_Field_DBclick('" & i & "', 'WF_Rep_CODE', '" & LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST & "')")
        Next

        '○ 状態をクリア
        For Each M00006row As DataRow In M00006tbl.Rows
            Select Case M00006row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    M00006row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    M00006row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    M00006row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    M00006row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    M00006row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 選択明細の状態を設定
        Select Case M00006tbl.Rows(WW_LINECNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                M00006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                M00006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                M00006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                M00006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                M00006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
        End Select

        '○ 画面表示データ保存
        Master.SaveTable(M00006tbl)

        WF_USERID.Focus()
        WF_GridDBclick.Text = ""

    End Sub


    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_Scroll()

        WF_USERID.Focus()

    End Sub


    ''' <summary>
    ''' ファイルアップロード時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FILEUPLOAD()

        '○ エラーレポート準備
        rightview.setErrorReport("")

        '○ UPLOAD XLSデータ取得
        CS0023XLSUPLOAD.CAMPCODE = work.WF_SEL_CAMPCODE.Text        '会社コード
        CS0023XLSUPLOAD.MAPID = Master.MAPID                        '画面ID
        CS0023XLSUPLOAD.CS0023XLSUPLOAD()
        If isNormal(CS0023XLSUPLOAD.ERR) Then
            If CS0023XLSUPLOAD.TBLDATA.Rows.Count = 0 Then
                Master.output(C_MESSAGE_NO.REGISTRATION_RECORD_NOT_EXIST_ERROR, C_MESSAGE_TYPE.ERR)
                Exit Sub
            End If
        Else
            Master.output(CS0023XLSUPLOAD.ERR, C_MESSAGE_TYPE.ABORT, "CS0023XLSUPLOAD")
            Exit Sub
        End If

        '○ CS0023XLSUPLOAD.TBLDATAの入力値整備
        Dim WW_COLUMNS As New List(Of String)
        For Each XLSTBLcol As DataColumn In CS0023XLSUPLOAD.TBLDATA.Columns
            WW_COLUMNS.Add(XLSTBLcol.ColumnName.ToString())
        Next

        Dim CS0023XLSTBLrow As DataRow = CS0023XLSUPLOAD.TBLDATA.NewRow
        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            CS0023XLSTBLrow.ItemArray = XLSTBLrow.ItemArray

            For Each XLSTBLcol As DataColumn In CS0023XLSUPLOAD.TBLDATA.Columns
                If IsDBNull(CS0023XLSTBLrow.Item(XLSTBLcol)) OrElse IsNothing(CS0023XLSTBLrow.Item(XLSTBLcol)) Then
                    CS0023XLSTBLrow.Item(XLSTBLcol) = ""
                End If
            Next

            XLSTBLrow.ItemArray = CS0023XLSTBLrow.ItemArray
        Next

        '○ XLSUPLOAD明細⇒INPtbl
        Master.CreateEmptyTable(M00006INPtbl)

        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            Dim M00006INProw As DataRow = M00006INPtbl.NewRow

            '○ 初期クリア
            For Each M00006INPcol As DataColumn In M00006INPtbl.Columns
                If IsDBNull(M00006INProw.Item(M00006INPcol)) OrElse IsNothing(M00006INProw.Item(M00006INPcol)) Then
                    Select Case M00006INPcol.ColumnName
                        Case "LINECNT"
                            M00006INProw.Item(M00006INPcol) = 0
                        Case "OPERATION"
                            M00006INProw.Item(M00006INPcol) = C_LIST_OPERATION_CODE.NODATA
                        Case "TIMSTP"
                            M00006INProw.Item(M00006INPcol) = 0
                        Case "SELECT"
                            M00006INProw.Item(M00006INPcol) = 1
                        Case "HIDDEN"
                            M00006INProw.Item(M00006INPcol) = 0
                        Case "SEQ"
                            M00006INProw.Item(M00006INPcol) = 0
                        Case Else
                            M00006INProw.Item(M00006INPcol) = ""
                    End Select
                End If
            Next

            '○ 変更元情報をデフォルト設定
            If WW_COLUMNS.IndexOf("USERID") >= 0 AndAlso
                WW_COLUMNS.IndexOf("CAMPCODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("OBJECT") >= 0 AndAlso
                WW_COLUMNS.IndexOf("STRUCT") >= 0 AndAlso
                WW_COLUMNS.IndexOf("SEQ") >= 0 AndAlso
                WW_COLUMNS.IndexOf("CODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("STYMD") >= 0 Then
                For Each M00006row As DataRow In M00006tbl.Rows
                    If XLSTBLrow("USERID") = M00006row("USERID") AndAlso
                        XLSTBLrow("CAMPCODE") = M00006row("CAMPCODE") AndAlso
                        XLSTBLrow("OBJECT") = M00006row("OBJECT") AndAlso
                        XLSTBLrow("STRUCT") = M00006row("STRUCT") AndAlso
                        XLSTBLrow("SEQ") = M00006row("SEQ") AndAlso
                        XLSTBLrow("CODE") = M00006row("CODE") AndAlso
                        XLSTBLrow("STYMD") = M00006row("STYMD") Then
                        M00006INProw.ItemArray = M00006row.ItemArray
                        Exit For
                    End If
                Next
            End If

            '○ 項目セット
            'ユーザID
            If WW_COLUMNS.IndexOf("USERID") >= 0 Then
                M00006INProw("USERID") = XLSTBLrow("USERID")
            End If

            '会社コード
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 Then
                M00006INProw("CAMPCODE") = XLSTBLrow("CAMPCODE")
            End If

            'オブジェクト
            If WW_COLUMNS.IndexOf("OBJECT") >= 0 Then
                M00006INProw("OBJECT") = XLSTBLrow("OBJECT")
            End If

            '構造コード
            If WW_COLUMNS.IndexOf("STRUCT") >= 0 Then
                M00006INProw("STRUCT") = XLSTBLrow("STRUCT")
            End If

            '表示順番
            If WW_COLUMNS.IndexOf("SEQ") >= 0 Then
                Dim WW_NUM As Integer
                Try
                    Integer.TryParse(XLSTBLrow("SEQ"), WW_NUM)
                    M00006INProw("SEQ") = WW_NUM
                    M00006INProw("CHKSEQ") = WW_NUM.ToString()
                Catch ex As Exception
                    M00006INProw("SEQ") = 0
                    M00006INProw("CHKSEQ") = "0"
                End Try
            End If

            'コード
            If WW_COLUMNS.IndexOf("CODE") >= 0 Then
                M00006INProw("CODE") = XLSTBLrow("CODE")
            End If

            '開始年月日
            If WW_COLUMNS.IndexOf("STYMD") >= 0 Then
                Dim WW_DATE As Date
                Try
                    Date.TryParse(XLSTBLrow("STYMD"), WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        M00006INProw("STYMD") = ""
                    Else
                        M00006INProw("STYMD") = WW_DATE.ToString("yyyy/MM/dd")
                    End If
                Catch ex As Exception
                    M00006INProw("STYMD") = ""
                End Try
            End If

            '終了年月日
            If WW_COLUMNS.IndexOf("ENDYMD") >= 0 Then
                Dim WW_DATE As Date
                Try
                    Date.TryParse(XLSTBLrow("ENDYMD"), WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        M00006INProw("ENDYMD") = ""
                    Else
                        M00006INProw("ENDYMD") = WW_DATE.ToString("yyyy/MM/dd")
                    End If
                Catch ex As Exception
                    M00006INProw("ENDYMD") = ""
                End Try
            End If

            '削除フラグ
            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                M00006INProw("DELFLG") = XLSTBLrow("DELFLG")
            End If

            '名称取得
            CODENAME_get("USERID", M00006INProw("USERID"), M00006INProw("USERNAMES"), WW_DUMMY)                             'ユーザID
            CODENAME_get("CAMPCODE", M00006INProw("CAMPCODE"), M00006INProw("CAMPNAMES"), WW_DUMMY)                         '会社コード
            CODENAME_get("OBJECT", M00006INProw("OBJECT"), M00006INProw("OBJECTNAMES"), WW_DUMMY)                           'オブジェクト
            CODENAME_get("CODE", M00006INProw("CODE"), M00006INProw("CODENAMES"), WW_DUMMY, M00006INProw("OBJECT"))         'コード

            M00006INPtbl.Rows.Add(M00006INProw)
        Next

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        M00006tbl_UPD()

        '○ 画面表示データ保存
        Master.SaveTable(M00006tbl)

        '○ メッセージ表示
        If isNormal(WW_ERR_SW) Then
            Master.output(C_MESSAGE_NO.IMPORT_SUCCESSFUL, C_MESSAGE_TYPE.INF)
        Else
            Master.output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
        End If

        '○ Close
        CS0023XLSUPLOAD.TBLDATA.Dispose()
        CS0023XLSUPLOAD.TBLDATA.Clear()

        WF_USERID.Focus()

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
        rightview.setErrorReport("")

        '○ DetailBoxをINPtblへ退避
        DetailBoxToM00006INPtbl(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ERR_SW) Then
            M00006tbl_UPD()
        End If

        '○ 画面表示データ保存
        Master.SaveTable(M00006tbl)

        '○ 詳細画面初期化
        If isNormal(WW_ERR_SW) Then
            DetailBoxClear()
        End If

        '○ メッセージ表示
        If isNormal(WW_ERR_SW) Then
            Master.output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.INF)
        Else
            Master.output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
        End If

        WF_USERID.Focus()

    End Sub

    ''' <summary>
    ''' 詳細画面-テーブル退避
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub DetailBoxToM00006INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.eraseCharToIgnore(WF_USERID.Text)            'ユーザーID
        Master.eraseCharToIgnore(WF_CAMPCODE.Text)          '会社コード
        Master.eraseCharToIgnore(WF_OBJECT.Text)            'オブジェクト
        Master.eraseCharToIgnore(WF_STRUCT.Text)            '構造コード
        Master.eraseCharToIgnore(WF_STYMD.Text)             '開始年月日
        Master.eraseCharToIgnore(WF_ENDYMD.Text)            '終了年月日
        Master.eraseCharToIgnore(WF_DELFLG.Text)            '削除

        '○ GridViewから未選択状態で表更新ボタンを押下時の例外を回避する
        If String.IsNullOrEmpty(WF_Sel_LINECNT.Text) AndAlso
            String.IsNullOrEmpty(WF_USERID.Text) AndAlso
            String.IsNullOrEmpty(WF_CAMPCODE.Text) AndAlso
            String.IsNullOrEmpty(WF_OBJECT.Text) AndAlso
            String.IsNullOrEmpty(WF_STRUCT.Text) AndAlso
            String.IsNullOrEmpty(WF_STYMD.Text) AndAlso
            String.IsNullOrEmpty(WF_ENDYMD.Text) AndAlso
            String.IsNullOrEmpty(WF_DELFLG.Text) Then
            Master.output(C_MESSAGE_NO.INVALID_PROCCESS_ERROR, C_MESSAGE_TYPE.ERR, "no Detail")

            CS0011LOGWrite.INFSUBCLASS = "DetailBoxToINPtbl"        'SUBクラス名
            CS0011LOGWrite.INFPOSI = "non Detail"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWrite.TEXT = "non Detail"
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                         'ログ出力

            O_RTN = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            Exit Sub
        End If

        Master.CreateEmptyTable(M00006INPtbl)

        '○ 入力した明細行分作成
        For Each reitem As RepeaterItem In WF_Repeater.Items

            '未入力は登録しない
            If CType(reitem.FindControl("WF_Rep_SEQ"), TextBox).Text = "" AndAlso
                CType(reitem.FindControl("WF_Rep_CODE"), TextBox).Text = "" Then
                Continue For
            End If

            Master.eraseCharToIgnore(CType(reitem.FindControl("WF_Rep_SEQ"), TextBox).Text)         'SEQ
            Master.eraseCharToIgnore(CType(reitem.FindControl("WF_Rep_CODE"), TextBox).Text)        'コード

            Dim M00006INProw As DataRow = M00006INPtbl.NewRow
            
            '○ 初期クリア
            For Each M00006INPcol As DataColumn In M00006INPtbl.Columns
                If IsDBNull(M00006INProw.Item(M00006INPcol)) OrElse IsNothing(M00006INProw.Item(M00006INPcol)) Then
                    Select Case M00006INPcol.ColumnName
                        Case "LINECNT"
                            M00006INProw.Item(M00006INPcol) = 0
                        Case "OPERATION"
                            M00006INProw.Item(M00006INPcol) = C_LIST_OPERATION_CODE.NODATA
                        Case "TIMSTP"
                            M00006INProw.Item(M00006INPcol) = 0
                        Case "SELECT"
                            M00006INProw.Item(M00006INPcol) = 1
                        Case "HIDDEN"
                            M00006INProw.Item(M00006INPcol) = 0
                        Case "SEQ"
                            M00006INProw.Item(M00006INPcol) = 0
                        Case Else
                            M00006INProw.Item(M00006INPcol) = ""
                    End Select
                End If
            Next

            'LINECNT
            If WF_Sel_LINECNT.Text = "" Then
                M00006INProw("LINECNT") = 0
            Else
                Try
                    Integer.TryParse(WF_Sel_LINECNT.Text, M00006INProw("LINECNT"))
                Catch ex As Exception
                    M00006INProw("LINECNT") = 0
                End Try
            End If

            M00006INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
            M00006INProw("TIMSTP") = 0
            M00006INProw("SELECT") = 1
            M00006INProw("HIDDEN") = 0
            M00006INProw("TITLEKBN") = "I"

            M00006INProw("USERID") = WF_USERID.Text             'ユーザーID
            M00006INProw("CAMPCODE") = WF_CAMPCODE.Text         '会社コード
            M00006INProw("OBJECT") = WF_OBJECT.Text             'オブジェクト
            M00006INProw("STRUCT") = WF_STRUCT.Text             '構造コード
            M00006INProw("STYMD") = WF_STYMD.Text               '有効年月日(From)
            M00006INProw("ENDYMD") = WF_ENDYMD.Text             '有効年月日(To)
            M00006INProw("DELFLG") = WF_DELFLG.Text             '削除

            M00006INProw("USERNAMES") = ""
            M00006INProw("CAMPNAMES") = ""
            M00006INProw("OBJECTNAMES") = ""
            M00006INProw("CODENAMES") = ""

            M00006INProw("CHKSEQ") = CType(reitem.FindControl("WF_Rep_SEQ"), TextBox).Text
            M00006INProw("CODE") = CType(reitem.FindControl("WF_Rep_CODE"), TextBox).Text

            '名称取得
            CODENAME_get("USERID", M00006INProw("USERID"), M00006INProw("USERNAMES"), WW_DUMMY)                             'ユーザーID
            CODENAME_get("CAMPCODE", M00006INProw("CAMPCODE"), M00006INProw("CAMPNAMES"), WW_DUMMY)                         '会社コード
            CODENAME_get("OBJECT", M00006INProw("OBJECT"), M00006INProw("OBJECTNAMES"), WW_DUMMY)                           'オブジェクト
            CODENAME_get("CODE", M00006INProw("CODE"), M00006INProw("CODENAMES"), WW_DUMMY, M00006INProw("OBJECT"))         'コード

            'チェック用テーブルに登録する
            M00006INPtbl.Rows.Add(M00006INProw)
        Next

    End Sub


    ''' <summary>
    ''' 詳細画面-クリアボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_CLEAR_Click()

        '○ 詳細画面初期化
        DetailBoxClear()

        '○ メッセージ表示
        Master.output(C_MESSAGE_NO.DATA_CLEAR_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        WF_USERID.Focus()

    End Sub

    ''' <summary>
    ''' 詳細画面初期化
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailBoxClear()

        '○ 状態をクリア
        For Each M00006row As DataRow In M00006tbl.Rows
            Select Case M00006row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    M00006row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    M00006row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    M00006row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    M00006row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    M00006row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(M00006tbl)

        WF_Sel_LINECNT.Text = ""            'LINECNT
        WF_USERID.Text = ""                 'ユーザーID
        WF_USERID_TEXT.Text = ""            'ユーザー名称
        WF_CAMPCODE.Text = ""               '会社コード
        WF_CAMPCODE_TEXT.Text = ""          '会社名称
        WF_OBJECT.Text = ""                 'オブジェクト
        WF_OBJECT_TEXT.Text = ""            'オブジェクト名称
        WF_STRUCT.Text = ""                 '構造コード
        WF_STYMD.Text = ""                  '有効年月日(From)
        WF_ENDYMD.Text = ""                 '有効年月日(To)
        WF_DELFLG.Text = ""                 '削除
        WF_DELFLG_TEXT.Text = ""            '削除名称

        '○ 詳細画面初期設定
        DetailInitialize()

    End Sub

    ''' <summary>
    ''' 詳細画面-初期設定 (空明細作成 ＆ イベント追加)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailInitialize()

        Master.CreateEmptyTable(M00006INPtbl)

        '○ +10行分空行作成
        For i As Integer = 0 To CONST_BLANK_LINE - 1
            Dim M00006INProw As DataRow = M00006INPtbl.NewRow

            For Each M00006INPcol As DataColumn In M00006INPtbl.Columns
                Select Case M00006INPcol.ColumnName
                    Case "LINECNT", "HIDDEN", "SEQ", "TIMSTP"
                        M00006INProw.Item(M00006INPcol) = 0
                    Case "SELECT"
                        M00006INProw.Item(M00006INPcol) = 1
                    Case "TITLEKBN"
                        M00006INProw.Item(M00006INPcol) = "I"
                    Case Else
                        M00006INProw.Item(M00006INPcol) = ""
                End Select
            Next

            M00006INPtbl.Rows.Add(M00006INProw)
        Next

        '○ 明細へデータ貼り付け
        WF_Repeater.Visible = True
        WF_Repeater.DataSource = M00006INPtbl
        WF_Repeater.DataBind()

        '○ 明細作成
        For i As Integer = 0 To WF_Repeater.Items.Count - 1
            '項番
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_LINECNT"), Label).Text = (i + 1).ToString()

            'SEQ
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_SEQ"), TextBox).Text = ""

            'コード
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_CODE"), TextBox).Text = ""
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_CODE_TEXT"), Label).Text = ""
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_CODE"), TextBox).Attributes.Remove("ondblclick")
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_CODE"), TextBox).Attributes.Add("ondblclick", "REF_Field_DBclick('" & i & "', 'WF_Rep_CODE', '" & LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST & "')")
        Next

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

            With leftview
                Select Case WF_LeftMViewChange.Value
                    Case LIST_BOX_CLASSIFICATION.LC_CALENDAR
                        '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                        Select Case WF_FIELD.Value
                            Case "WF_STYMD"         '有効年月日(From)
                                .WF_Calendar.Text = WF_STYMD.Text
                            Case "WF_ENDYMD"        '有効年月日(To)
                                .WF_Calendar.Text = WF_ENDYMD.Text
                        End Select
                        .activeCalendar()

                    Case Else
                        '以外
                        Dim prmData As New Hashtable
                        prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

                        'フィールドによってパラメーターを変える
                        Select Case WF_FIELD.Value
                            Case "WF_SELOBJECT", "WF_OBJECT"        'オブジェクト
                                prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "OBJECT"
                            Case "WF_USERID"                        'ユーザーID
                                prmData = work.CreateUserIDParam(work.WF_SEL_CAMPCODE.Text)
                            Case "WF_Rep_CODE"                      'コード
                                prmData = work.CreateCodeParam(WF_LeftMViewChange.Value, work.WF_SEL_CAMPCODE.Text, WF_OBJECT.Text)
                        End Select

                        .setListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                        .activeListBox()
                End Select
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
        If Not IsNothing(leftview.getActiveValue) Then
            WW_SelectValue = leftview.getActiveValue(0)
            WW_SelectText = leftview.getActiveValue(1)
        End If

        '○ 選択内容を画面項目へセット
        Select Case WF_FIELD.Value
            Case "WF_SELOBJECT"         'オブジェクト
                WF_SELOBJECT.Text = WW_SelectValue
                WF_SELOBJECT_TEXT.Text = WW_SelectText
                WF_SELOBJECT.Focus()

            Case "WF_USERID"            'ユーザーID
                WF_USERID.Text = WW_SelectValue
                WF_USERID_TEXT.Text = WW_SelectText
                WF_USERID.Focus()

            Case "WF_CAMPCODE"          '会社コード
                WF_CAMPCODE.Text = WW_SelectValue
                WF_CAMPCODE_TEXT.Text = WW_SelectText
                WF_CAMPCODE.Focus()

            Case "WF_OBJECT"            'オブジェクト
                WF_OBJECT.Text = WW_SelectValue
                WF_OBJECT_TEXT.Text = WW_SelectText
                WF_OBJECT.Focus()

            Case "WF_STYMD"             '有効年月日(From)
                Dim WW_DATE As Date
                Try
                    Date.TryParse(WW_SelectValue, WW_DATE)
                    WF_STYMD.Text = WW_DATE.ToString("yyyy/MM/dd")
                Catch ex As Exception
                End Try
                WF_STYMD.Focus()

            Case "WF_ENDYMD"            '有効年月日(To)
                Dim WW_DATE As Date
                Try
                    Date.TryParse(WW_SelectValue, WW_DATE)
                    WF_ENDYMD.Text = WW_DATE.ToString("yyyy/MM/dd")
                Catch ex As Exception
                End Try
                WF_ENDYMD.Focus()

            Case "WF_DELFLG"            '削除
                WF_DELFLG.Text = WW_SelectValue
                WF_DELFLG_TEXT.Text = WW_SelectText
                WF_DELFLG.Focus()

            Case "WF_Rep_CODE"          'コード
                CType(WF_Repeater.Items(WF_FIELD_REP.Value).FindControl("WF_Rep_CODE"), TextBox).Text = WW_SelectValue
                CType(WF_Repeater.Items(WF_FIELD_REP.Value).FindControl("WF_Rep_CODE_TEXT"), Label).Text = WW_SelectText
                CType(WF_Repeater.Items(WF_FIELD_REP.Value).FindControl("WF_Rep_CODE"), TextBox).Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub

    ''' <summary>
    ''' LeftBoxキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○ フォーカスセット
        Select Case WF_FIELD.Value
            Case "WF_SELOBJECT"         'オブジェクト
                WF_SELOBJECT.Focus()
            Case "WF_USERID"            'ユーザーID
                WF_USERID.Focus()
            Case "WF_CAMPCODE"          '会社コード
                WF_CAMPCODE.Focus()
            Case "WF_OBJECT"            'オブジェクト
                WF_OBJECT.Focus()
            Case "WF_STYMD"             '有効年月日(From)
                WF_STYMD.Focus()
            Case "WF_ENDYMD"            '有効年月日(To)
                WF_ENDYMD.Focus()
            Case "WF_DELFLG"            '削除
                WF_DELFLG.Focus()
            Case "WF_Rep_CODE"          'コード
                CType(WF_Repeater.Items(WF_FIELD_REP.Value).FindControl("WF_Rep_CODE"), TextBox).Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

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

            rightview.selectIndex(WF_RightViewChange.Value)
            WF_RightViewChange.Value = ""
        End If

    End Sub

    ''' <summary>
    ''' RightBoxメモ欄更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_Change()

		rightview.save(Master.USERID, Master.USERTERMID, WW_DUMMY)

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
        For Each M00006INProw As DataRow In M00006INPtbl.Rows

            WW_LINE_ERR = ""

            'ユーザーID
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "USERID", M00006INProw("USERID"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("USERID", M00006INProw("USERID"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(ユーザーIDエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, M00006INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '権限チェック
                CS0025AUTHORget.USERID = CS0050SESSION.USERID
                CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_PROFILE
                CS0025AUTHORget.CODE = M00006INProw("USERID")
                CS0025AUTHORget.STYMD = Date.Now
                CS0025AUTHORget.ENDYMD = Date.Now
                CS0025AUTHORget.CS0025AUTHORget()
                If isNormal(CS0025AUTHORget.ERR) AndAlso CS0025AUTHORget.PERMITCODE = C_PERMISSION.UPDATE Then
                Else
                    WW_CheckMES1 = "・更新できないレコード(ユーザ権限無)です。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, M00006INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(ユーザーIDエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, M00006INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '会社コード
            If M00006INProw("CAMPCODE") <> C_DEFAULT_DATAKEY Then
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", M00006INProw("CAMPCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If isNormal(WW_CS0024FCHECKERR) Then
                    '存在チェック
                    CODENAME_get("CAMPCODE", M00006INProw("CAMPCODE"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, M00006INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, M00006INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            'オブジェクト
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "OBJECT", M00006INProw("OBJECT"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("OBJECT", M00006INProw("OBJECT"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(オブジェクトエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, M00006INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(オブジェクトエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, M00006INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '会社コード関連チェック
            If (M00006INProw("OBJECT") = "ARTTICLE1" OrElse
                M00006INProw("OBJECT") = "ARTTICLE2" OrElse
                M00006INProw("OBJECT") = "CAMP" OrElse
                M00006INProw("OBJECT") = "CUSTOMER" OrElse
                M00006INProw("OBJECT") = "MAP" OrElse
                M00006INProw("OBJECT") = "USER") AndAlso
                M00006INProw("CAMPCODE") <> C_DEFAULT_DATAKEY Then
                WW_CheckMES1 = "・更新できないレコード(会社コードが'" & C_DEFAULT_DATAKEY & "'指定ではない)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, M00006INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If (M00006INProw("OBJECT") = "LORRY" OrElse
                M00006INProw("OBJECT") = "ORG" OrElse
                M00006INProw("OBJECT") = "REPORT" OrElse
                M00006INProw("OBJECT") = "STAFF" OrElse
                M00006INProw("OBJECT") = "VEHICLE") AndAlso
                M00006INProw("CAMPCODE") = C_DEFAULT_DATAKEY Then
                WW_CheckMES1 = "・更新できないレコード(会社コードが'" & C_DEFAULT_DATAKEY & "'指定不可)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, M00006INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR

                If work.WF_SEL_CAMPCODE.Text <> M00006INProw("CAMPCODE") Then
                    WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, M00006INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '構造コード
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "STRUCT", M00006INProw("STRUCT"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(構造コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, M00006INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '開始年月日
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "STYMD", M00006INProw("STYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(有効日付：開始エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, M00006INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '終了年月日
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "ENDYMD", M00006INProw("ENDYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(有効日付：終了エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, M00006INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '日付大小チェック
            If M00006INProw("STYMD") > M00006INProw("ENDYMD") Then
                WW_CheckMES1 = "・更新できないレコード(有効開始日＞有効終了日)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, M00006INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '範囲チェック
            If work.WF_SEL_STYMD.Text > M00006INProw("STYMD") AndAlso
                work.WF_SEL_STYMD.Text > M00006INProw("ENDYMD") Then
                WW_CheckMES1 = "・更新できないレコード(開始、終了日付が範囲外)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, M00006INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            If work.WF_SEL_ENDYMD.Text < M00006INProw("STYMD") AndAlso
                work.WF_SEL_ENDYMD.Text < M00006INProw("ENDYMD") Then
                WW_CheckMES1 = "・更新できないレコード(開始、終了日付が範囲外)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, M00006INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '削除フラグ
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "DELFLG", M00006INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(削除CD不正)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, M00006INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'SEQ
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "SEQ", M00006INProw("CHKSEQ"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                Dim WW_NUM As Integer
                Try
                    Integer.TryParse(M00006INProw("CHKSEQ"), WW_NUM)
                    M00006INProw("SEQ") = WW_NUM
                Catch ex As Exception
                    WW_CheckMES1 = "・更新できないレコード(表示順番エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, M00006INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End Try
            Else
                WW_CheckMES1 = "・更新できないレコード(表示順番エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, M00006INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'コード
            If M00006INProw("OBJECT") <> "REPORT" Then
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "CODE", M00006INProw("CODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If isNormal(WW_CS0024FCHECKERR) Then
                    '存在チェック
                    CODENAME_get("CODE", M00006INProw("CODE"), WW_DUMMY, WW_RTN_SW, M00006INProw("OBJECT"))
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(コードエラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, M00006INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(コードエラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, M00006INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            If WW_LINE_ERR = "" Then
                If M00006INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    M00006INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                M00006INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

        '○ 重複チェック
        For i As Integer = 0 To M00006INPtbl.Rows.Count - 1
            WW_LINE_ERR = ""

            For j As Integer = i + 1 To M00006INPtbl.Rows.Count - 1
                If M00006INPtbl.Rows(i)("USERID") = M00006INPtbl.Rows(j)("USERID") AndAlso
                    M00006INPtbl.Rows(i)("CAMPCODE") = M00006INPtbl.Rows(j)("CAMPCODE") AndAlso
                    M00006INPtbl.Rows(i)("OBJECT") = M00006INPtbl.Rows(j)("OBJECT") AndAlso
                    M00006INPtbl.Rows(i)("STRUCT") = M00006INPtbl.Rows(j)("STRUCT") AndAlso
                    M00006INPtbl.Rows(i)("STYMD") = M00006INPtbl.Rows(j)("STYMD") Then
                    If M00006INPtbl.Rows(i)("SEQ") = M00006INPtbl.Rows(j)("SEQ") Then
                        WW_CheckMES1 = "・更新できないレコード(SEQエラー)です。"
                        WW_CheckMES2 = "重複行が存在しています。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, M00006INPtbl.Rows(i))
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        Exit For
                    End If

                    If M00006INPtbl.Rows(i)("CODE") = M00006INPtbl.Rows(j)("CODE") Then
                        WW_CheckMES1 = "・更新できないレコード(コードエラー)です。"
                        WW_CheckMES2 = "重複行が存在しています。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, M00006INPtbl.Rows(i))
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        Exit For
                    End If
                End If
            Next

            If WW_LINE_ERR = "" Then
                If M00006INPtbl.Rows(i)("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    M00006INPtbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                M00006INPtbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="M00006row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal M00006row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(M00006row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> ユーザＩＤ   =" & M00006row("USERID") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 会社         =" & M00006row("CAMPCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> オブジェクト =" & M00006row("OBJECT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 構造コード   =" & M00006row("STRUCT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示順番     =" & M00006row("CHKSEQ") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> コード       =" & M00006row("CODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 開始年月日   =" & M00006row("STYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 終了年月日   =" & M00006row("ENDYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除         =" & M00006row("DELFLG")
        End If

        rightview.addErrorReport(WW_ERR_MES)

    End Sub


    ''' <summary>
    ''' M00006tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub M00006tbl_UPD()

        '○ 画面状態設定
        For Each M00006row As DataRow In M00006tbl.Rows
            Select Case M00006row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    M00006row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    M00006row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    M00006row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    M00006row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    M00006row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each M00006INProw As DataRow In M00006INPtbl.Rows

            'エラーレコード読み飛ばし
            If M00006INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            M00006INProw("OPERATION") = "Insert"

            'KEY項目が等しい(ENDYMD以外のKEYが同じ)
            For Each M00006row As DataRow In M00006tbl.Rows
                If M00006row("USERID") = M00006INProw("USERID") AndAlso
                    M00006row("CAMPCODE") = M00006INProw("CAMPCODE") AndAlso
                    M00006row("OBJECT") = M00006INProw("OBJECT") AndAlso
                    M00006row("STRUCT") = M00006INProw("STRUCT") AndAlso
                    M00006row("SEQ") = M00006INProw("SEQ") AndAlso
                    M00006row("CODE") = M00006INProw("CODE") AndAlso
                    M00006row("STYMD") = M00006INProw("STYMD") Then

                    '変更無は操作無
                    If M00006row("ENDYMD") = M00006INProw("ENDYMD") AndAlso
                        M00006row("DELFLG") = M00006INProw("DELFLG") Then
                        M00006INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                        Exit For
                    End If

                    M00006INProw("OPERATION") = "Update"
                    Exit For
                End If
            Next
        Next

        '○ 変更有無判定　&　入力値反映
        For Each M00006INProw As DataRow In M00006INPtbl.Rows
            Select Case M00006INProw("OPERATION")
                Case "Update"
                    TBL_UPDATE_SUB(M00006INProw)
                Case "Insert"
                    TBL_INSERT_SUB(M00006INProw)
                Case "エラー"
            End Select
        Next

        '○ 更新対象でSEQとコードが更新テーブルに存在しないデータは削除
        For Each M00006row As DataRow In M00006tbl.Rows

            Dim WW_DELETE As String = M00006row("DELFLG")

            For Each M00006INProw As DataRow In M00006INPtbl.Rows
                If M00006row("USERID") = M00006INProw("USERID") AndAlso
                    M00006row("CAMPCODE") = M00006INProw("CAMPCODE") AndAlso
                    M00006row("OBJECT") = M00006INProw("OBJECT") AndAlso
                    M00006row("STRUCT") = M00006INProw("STRUCT") AndAlso
                    M00006row("STYMD") = M00006INProw("STYMD") Then

                    '一度削除フラグを立てる
                    WW_DELETE = C_DELETE_FLG.DELETE

                    'SEQとコードが存在する時、更新レコードの削除フラグに置き換え
                    If M00006row("SEQ") = M00006INProw("SEQ") AndAlso
                        M00006row("CODE") = M00006INProw("CODE") Then
                        WW_DELETE = M00006INProw("DELFLG")
                        Exit For
                    End If
                End If
            Next

            If M00006row("DELFLG") <> WW_DELETE Then
                M00006row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If

            M00006row("DELFLG") = WW_DELETE
        Next

        '○ 更新レコード設定
        For Each M00006row As DataRow In M00006tbl.Rows
            If M00006row("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            '同一KEYは全て更新対象にする
            For Each M00006ope As DataRow In M00006tbl.Rows
                If M00006ope("USERID") = M00006row("USERID") AndAlso
                    M00006ope("CAMPCODE") = M00006row("CAMPCODE") AndAlso
                    M00006ope("OBJECT") = M00006row("OBJECT") AndAlso
                    M00006ope("STRUCT") = M00006row("STRUCT") AndAlso
                    M00006ope("STYMD") = M00006row("STYMD") Then
                    M00006ope("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Next
        Next

        '○ 配列再設定
        CS0026TBLSORT.TABLE = M00006tbl
        CS0026TBLSORT.SORTING = "USERID, CAMPCODE, OBJECT, STRUCT, STYMD, DELFLG, SEQ, CODE"
        CS0026TBLSORT.FILTER = ""
        CS0026TBLSORT.sort(M00006tbl)

        Dim i As Integer = 0
        Dim WW_SAVEKEY As String = ""
        For Each M00006row As DataRow In M00006tbl.Rows
            M00006row("LINECNT") = 0
            M00006row("TITLEKBN") = "I"

            Dim WW_KEY As String = M00006row("USERID") & "," & M00006row("CAMPCODE") & "," & M00006row("OBJECT") & "," & M00006row("STRUCT") & "," & M00006row("STYMD")
            If WW_SAVEKEY <> WW_KEY Then
                i += 1
                M00006row("TITLEKBN") = "H"
                WW_SAVEKEY = WW_KEY
            End If

            M00006row("LINECNT") = i
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="M00006INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef M00006INProw As DataRow)

        For Each M00006row As DataRow In M00006tbl.Rows

            '同一(ENDYMD以外が同一KEY)レコード
            If M00006INProw("USERID") = M00006row("USERID") AndAlso
                M00006INProw("CAMPCODE") = M00006row("CAMPCODE") AndAlso
                M00006INProw("OBJECT") = M00006row("OBJECT") AndAlso
                M00006INProw("STRUCT") = M00006row("STRUCT") AndAlso
                M00006INProw("SEQ") = M00006row("SEQ") AndAlso
                M00006INProw("CODE") = M00006row("CODE") AndAlso
                M00006INProw("STYMD") = M00006row("STYMD") Then

                '画面入力テーブル項目設定
                M00006INProw("LINECNT") = M00006row("LINECNT")
                M00006INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                M00006INProw("TIMSTP") = M00006row("TIMSTP")
                M00006INProw("SELECT") = 1
                M00006INProw("HIDDEN") = 0

                '項目テーブル項目設定
                M00006row.ItemArray = M00006INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="M00006INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef M00006INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim M00006row As DataRow = M00006tbl.NewRow
        M00006row.ItemArray = M00006INProw.ItemArray

        M00006row("LINECNT") = 0
        M00006row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        M00006row("TIMSTP") = "0"
        M00006row("SELECT") = 1
        M00006row("HIDDEN") = 0
        M00006row("TITLEKBN") = "I"

        M00006tbl.Rows.Add(M00006row)

    End Sub

    ''' <summary>
    ''' 名称取得
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    ''' <param name="I_OBJECT"></param>
    ''' <remarks></remarks>
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByVal I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String, Optional ByVal I_OBJECT As String = Nothing)

        O_TEXT = ""
        O_RTN = ""

        If I_VALUE = "" Then
            O_RTN = C_MESSAGE_NO.NORMAL
            Exit Sub
        End If

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

        Try
            Select Case I_FIELD
                Case "OBJECT"           'オブジェクト
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "OBJECT"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "USERID"           'ユーザーID
                    prmData = work.CreateUserIDParam(work.WF_SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "CAMPCODE"         '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "DELFLG"           '削除
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "CODE"             'コード
                    Dim LIST As LIST_BOX_CLASSIFICATION
                    prmData = work.CreateCodeParam(LIST, work.WF_SEL_CAMPCODE.Text, I_OBJECT)
                    leftview.CodeToName(LIST, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
