''************************************************************
' タンク車マスタメンテ(甲子専用)画面
' 作成日 2021/03/08
' 更新日 2021/03/08
' 作成者 JOT三宅
' 更新車 JOT三宅
'
' 修正履歴:
'         :
''************************************************************
Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' タンク車マスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class OIM0005TankListKinoene
    Inherits Page

    '○ 検索結果格納Table
    Private OIM0005tbl As DataTable                                  '一覧格納用テーブル
    Private OIM0005INPtbl As DataTable                               'チェック用テーブル
    Private OIM0005UPDtbl As DataTable                               '更新用テーブル

    Private Const CONST_DISPROWCOUNT As Integer = 45                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 20                 'マウススクロール時稼働行数
    Private Const CONST_OFFICE_KINOENE As String = "011202"         '甲子営業所コード

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
                    Master.RecoverTable(OIM0005tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonUPDATE"          'DB更新ボタン押下
                            WF_ButtonUPDATE_Click()
                        Case "WF_ButtonLIST"            '一覧ダウンロードボタン押下
                            WF_ButtonDownload_Click()
                        Case "WF_ButtonCSV"             'CSVダウンロードボタン押下
                            WF_ButtonPrint_Click()
                        Case "WF_ButtonEND"             '戻るボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_ButtonFIRST"           '先頭頁ボタン押下
                            WF_ButtonFIRST_Click()
                        Case "WF_ButtonLAST"            '最終頁ボタン押下
                            WF_ButtonLAST_Click()
                        Case "WF_MouseWheelUp"          'マウスホイール(Up)
                            WF_Grid_Scroll()
                        Case "WF_MouseWheelDown"        'マウスホイール(Down)
                            WF_Grid_Scroll()
                        Case "WF_Field_DBClick"         'フィールドダブルクリック
                            WF_FIELD_DBClick()
                        Case "WF_ListChange"            'フィールドチェンジ
                            WF_FIELD_Change()
                        Case "WF_ButtonSel"             '(左ボックス)選択ボタン押下
                            WF_ButtonSel_Click()
                        Case "WF_ButtonCan"             '(左ボックス)キャンセルボタン押下
                            WF_ButtonCan_Click()
                        Case "WF_ListboxDBclick"        '左ボックスダブルクリック
                            WF_ButtonSel_Click()
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
            If Not IsNothing(OIM0005tbl) Then
                OIM0005tbl.Clear()
                OIM0005tbl.Dispose()
                OIM0005tbl = Nothing
            End If

            If Not IsNothing(OIM0005INPtbl) Then
                OIM0005INPtbl.Clear()
                OIM0005INPtbl.Dispose()
                OIM0005INPtbl = Nothing
            End If

            If Not IsNothing(OIM0005UPDtbl) Then
                OIM0005UPDtbl.Clear()
                OIM0005UPDtbl.Dispose()
                OIM0005UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIM0005WRKINC.MAPIDLK
        '○HELP表示有無設定
        Master.dispHelp = False
        '○D&D有無設定
        Master.eventDrop = True
        '○Grid情報保存先のファイル名
        Master.CreateXMLSaveFile()

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

        '○ GridView初期設定
        GridViewInitialize()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        'Grid情報保存先のファイル名
        Master.CreateXMLSaveFile()

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
        Master.SaveTable(OIM0005tbl)

        '〇 一覧の件数を取得
        Me.WF_ListCNT.Text = "件数：" + OIM0005tbl.Rows.Count.ToString()

        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each OIM0005row As DataRow In OIM0005tbl.Rows
            If OIM0005row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                OIM0005row("SELECT") = WW_DataCNT
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

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(OIM0005tbl)

        TBLview.Sort = "LINECNT"
        TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DISPROWCOUNT).ToString()

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.CS0013ProfView()
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
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

        If IsNothing(OIM0005tbl) Then
            OIM0005tbl = New DataTable
        End If

        If OIM0005tbl.Columns.Count <> 0 Then
            OIM0005tbl.Columns.Clear()
        End If

        OIM0005tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データをタンク車マスタから取得する
        Dim SQLStr As String =
              " SELECT " _
            & "   0                                                            AS LINECNT " _
            & " , ''                                                           AS OPERATION " _
            & " , CAST(OIM0005.UPDTIMSTP AS bigint)                            AS UPDTIMSTP " _
            & " , 1                                                            AS 'SELECT' " _
            & " , 0                                                            AS HIDDEN " _
            & " , ISNULL(RTRIM(OIM0005.DELFLG), '')                            AS DELFLG " _
            & " , ''                                                           AS PROCKBN " _
            & " , ISNULL(RTRIM(OIM0005.TANKNUMBER), '')                        AS TANKNUMBER " _
            & " , ISNULL(RTRIM(OIM0005.MODEL), '')                             AS MODEL " _
            & " , ISNULL(RTRIM(OIM0005.LOAD), '')                              AS LOAD " _
            & " , ISNULL(RTRIM(OIM0005.MIDDLEOILCODE), '')                     AS MIDDLEOILCODE " _
            & " , ISNULL(RTRIM(OIM0005.MIDDLEOILNAME), '')                     AS MIDDLEOILNAME " _
            & " , CASE WHEN OIM0005.DOWNLOADDATE IS NULL THEN '' " _
            & "              ELSE FORMAT(OIM0005.DOWNLOADDATE,'yyyy/MM/dd') " _
            & "   END                                                          AS DOWNLOADDATE " _
            & " FROM OIL.OIM0005_TANK OIM0005 " _
            & " WHERE OIM0005.OPERATIONBASECODE = @P1 "

        SQLStr &=
              " ORDER BY" _
            & "    RIGHT('0000000000' + CAST(OIM0005.TANKNUMBER AS NVARCHAR), 10)"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)       '営業所コード

                PARA1.Value = CONST_OFFICE_KINOENE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIM0005tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIM0005tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIM0005row As DataRow In OIM0005tbl.Rows
                    i += 1
                    OIM0005row("LINECNT") = i        'LINECNT
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0005LK SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0005LK Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
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
        For Each OIM0005row As DataRow In OIM0005tbl.Rows

            If OIM0005row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                OIM0005row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(OIM0005tbl)

        '○ ソート
        TBLview.Sort = "LINECNT"
        TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DISPROWCOUNT).ToString()

        '○ 一覧作成
        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
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
                Dim prmData As New Hashtable

                prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

                'フィールドによってパラメータを変える
                Select Case WF_FIELD.Value
                    Case "PROCKBN"       '処理区分
                        prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "PROCKBN")

                    Case "MIDDLEOILCODE" '油種中分類
                        prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "MIDDLEOILCODE")

                End Select

                .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                .ActiveListBox()
            End With
        End If

    End Sub

    ''' <summary>
    ''' フィールドチェンジ時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_Change()

        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        '○ LINECNT取得
        Dim WW_LINECNT As Integer = 0
        If Not Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT) Then Exit Sub

        '○ 対象ヘッダー取得
        Dim updHeader = OIM0005tbl.AsEnumerable.
                    FirstOrDefault(Function(x) x.Item("LINECNT") = WW_LINECNT)
        If IsNothing(updHeader) Then Exit Sub

        '○ 設定項目取得
        '対象フォーム項目取得
        Dim WW_ListValue = Request.Form("txt" & pnlListArea.ID & WF_FIELD.Value & WF_GridDBclick.Text)

        '○ 変更した項目の名称をセット
        Dim WW_TEXT As String = ""
        Select Case WF_FIELD.Value
            Case "PROCKBN"       '処理区分
                CODENAME_get("PROCKBN", WW_ListValue, WW_TEXT, WW_RTN_SW)
                updHeader.Item(WF_FIELD.Value) = WW_ListValue

            Case "MIDDLEOILCODE" '油種中分類
                CODENAME_get("MIDDLEOILCODE", WW_ListValue, WW_TEXT, WW_RTN_SW)
                updHeader.Item(WF_FIELD.Value) = WW_ListValue
                updHeader.Item("MIDDLEOILNAME") = WW_TEXT
                updHeader.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End Select

        '○ 画面表示データ保存
        Master.SaveTable(OIM0005tbl)

        '○ メッセージ表示
        If isNormal(WW_RTN_SW) Then
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.NOR)
        Else
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.ERR)
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

        Dim prmData As New Hashtable

        '○ 選択内容を画面項目へセット
        If WF_FIELD_REP.Value = "" Then

            '○ LINECNT取得
            Dim WW_LINECNT As Integer = 0
            If Not Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT) Then Exit Sub

            '○ 画面表示データ復元
            If Not Master.RecoverTable(OIM0005tbl) Then Exit Sub

            '○ 対象ヘッダー取得
            Dim updHeader = OIM0005tbl.AsEnumerable.
                            FirstOrDefault(Function(x) x.Item("LINECNT") = WW_LINECNT)
            If IsNothing(updHeader) Then Exit Sub

            Select Case WF_FIELD.Value
                Case "PROCKBN"       '処理区分
                    updHeader.Item(WF_FIELD.Value) = WW_SelectValue

                Case "MIDDLEOILCODE" '油種中分類
                    If updHeader.Item(WF_FIELD.Value) <> WW_SelectValue Then
                        updHeader.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    End If
                    updHeader.Item("MIDDLEOILNAME") = WW_SelectText
                    updHeader.Item(WF_FIELD.Value) = WW_SelectValue
            End Select

            '○ 画面表示データ保存
            If Not Master.SaveTable(OIM0005tbl) Then Exit Sub
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

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_RightboxOpen.Value = ""

    End Sub


    ''' <summary>
    ''' DB更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        '○チェック
        INPTableCheck(WW_ERRCODE)

        '○更新処理
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            'マスタ更新
            UpdateMaster(SQLcon, "UPD")
        End Using


        '現在入力されている処理区分をキープ（再表示する）
        Dim WW_OIM0005tbl As DataTable = Nothing
        Dim WW_TBLview = New DataView(OIM0005tbl)
        WW_TBLview.RowFilter = "PROCKBN <> '' or OPERATION = '" & C_LIST_OPERATION_CODE.ERRORED & "'"
        WW_OIM0005tbl = WW_TBLview.ToTable

        '○ GridView初期設定
        '○ 画面表示データ再取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            OIM0005tbl = Nothing
            MAPDataGet(SQLcon)

            '処理区分の再設定（更新ボタン押下の場合、DB項目に持たない処理区分を現在の表示内容から再設定する）
            For Each WW_OIM0005row As DataRow In WW_OIM0005tbl.Rows
                Dim updHeader = OIM0005tbl.AsEnumerable.
                    FirstOrDefault(Function(x) x.Item("TANKNUMBER") = WW_OIM0005row("TANKNUMBER"))
                If IsNothing(updHeader) Then Continue For
                updHeader.ItemArray = WW_OIM0005row.ItemArray
            Next

        End Using

        '○ 画面表示データ保存
        Master.SaveTable(OIM0005tbl)

        '○ 詳細画面クリア
        If isNormal(WW_ERRCODE) Then
            DetailBoxClear()
        End If

        '○ メッセージ表示
        If Not isNormal(WW_ERRCODE) Then
            Master.Output(C_MESSAGE_NO.ERROR_RECORD_EXIST, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
        End If

    End Sub

    ''' <summary>
    ''' 登録データ関連チェック
    ''' </summary>
    ''' <param name="O_RTNCODE"></param>
    ''' <remarks></remarks>
    Protected Sub RelatedCheck(ByRef O_RTNCODE As String)

        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""

        '○初期値設定
        O_RTNCODE = C_MESSAGE_NO.NORMAL

        For Each OIM0005row As DataRow In OIM0005tbl.Rows
            If Trim(OIM0005row("OPERATION")) <> "" Then
                If Trim(OIM0005row("MIDDLEOILCODE")) = Nothing Then
                    OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_CheckMES1 = "・存在しない油種中分類です。"
                    WW_CheckMES2 = "項番 = " & OIM0005row("LINECNT")
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                End If
            End If
        Next

    End Sub


    ''' <summary>
    ''' タンク車マスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateMaster(ByVal SQLcon As SqlConnection, ByVal iProc As String)

        '○ ＤＢ更新
        Dim SQLStr As String = ""
        If iProc = "UPD" Then
            SQLStr =
                  "    UPDATE OIL.OIM0005_TANK" _
                & "    SET" _
                & "          MIDDLEOILCODE = @P02" _
                & "        , MIDDLEOILNAME = @P03" _
                & "        , UPDYMD = @P05" _
                & "        , UPDUSER = @P06" _
                & "        , UPDTERMID = @P07" _
                & "        , RECEIVEYMD = @P08" _
                & "    WHERE" _
                & "        TANKNUMBER       = @P01 ;"
        Else
            SQLStr =
                  "    UPDATE OIL.OIM0005_TANK" _
                & "    SET" _
                & "          DOWNLOADDATE  = @P04" _
                & "        , UPDYMD = @P05" _
                & "        , UPDUSER = @P06" _
                & "        , UPDTERMID = @P07" _
                & "        , RECEIVEYMD = @P08" _
                & "    WHERE" _
                & "        TANKNUMBER       = @P01 ;"
        End If

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
              " Select" _
            & "    DELFLG" _
            & "    , TANKNUMBER" _
            & "    , ORIGINOWNERCODE" _
            & "    , OWNERCODE" _
            & "    , LEASECODE" _
            & "    , LEASECLASS" _
            & "    , AUTOEXTENTION" _
            & "    , LEASESTYMD" _
            & "    , LEASEENDYMD" _
            & "    , USERCODE" _
            & "    , CURRENTSTATIONCODE" _
            & "    , EXTRADINARYSTATIONCODE" _
            & "    , USERLIMIT" _
            & "    , LIMITTEXTRADIARYSTATION" _
            & "    , DEDICATETYPECODE" _
            & "    , EXTRADINARYTYPECODE" _
            & "    , EXTRADINARYLIMIT" _
            & "    , OPERATIONBASECODE" _
            & "    , COLORCODE" _
            & "    , MARKCODE" _
            & "    , MARKNAME" _
            & "    , GETDATE" _
            & "    , TRANSFERDATE" _
            & "    , OBTAINEDCODE" _
            & "    , MODEL" _
            & "    , MODELKANA" _
            & "    , LOAD" _
            & "    , LOADUNIT" _
            & "    , VOLUME" _
            & "    , VOLUMEUNIT" _
            & "    , ORIGINOWNERNAME" _
            & "    , OWNERNAME" _
            & "    , LEASENAME" _
            & "    , LEASECLASSNEMAE" _
            & "    , USERNAME" _
            & "    , CURRENTSTATIONNAME" _
            & "    , EXTRADINARYSTATIONNAME" _
            & "    , DEDICATETYPENAME" _
            & "    , EXTRADINARYTYPENAME" _
            & "    , OPERATIONBASENAME" _
            & "    , COLORNAME" _
            & "    , RESERVE1" _
            & "    , RESERVE2" _
            & "    , SPECIFIEDDATE" _
            & "    , JRALLINSPECTIONDATE" _
            & "    , PROGRESSYEAR" _
            & "    , NEXTPROGRESSYEAR" _
            & "    , JRINSPECTIONDATE" _
            & "    , INSPECTIONDATE" _
            & "    , JRSPECIFIEDDATE" _
            & "    , JRTANKNUMBER" _
            & "    , OLDTANKNUMBER" _
            & "    , OTTANKNUMBER" _
            & "    , JXTGTANKNUMBER1" _
            & "    , COSMOTANKNUMBER" _
            & "    , FUJITANKNUMBER" _
            & "    , SHELLTANKNUMBER" _
            & "    , RESERVE3" _
            & "    , USEDFLG" _
            & "    , MYWEIGHT" _
            & "    , LENGTH" _
            & "    , TANKLENGTH" _
            & "    , MAXCALIBER" _
            & "    , MINCALIBER" _
            & "    , LENGTHFLG" _
            & "    , AUTOEXTENTIONNAME" _
            & "    , BIGOILCODE" _
            & "    , BIGOILNAME" _
            & "    , MIDDLEOILCODE" _
            & "    , MIDDLEOILNAME" _
            & "    , DOWNLOADDATE" _
            & "    , JXTGTAGCODE1" _
            & "    , JXTGTAGNAME1" _
            & "    , JXTGTAGCODE2" _
            & "    , JXTGTAGNAME2" _
            & "    , JXTGTAGCODE3" _
            & "    , JXTGTAGNAME3" _
            & "    , JXTGTAGCODE4" _
            & "    , JXTGTAGNAME4" _
            & "    , IDSSTAGCODE" _
            & "    , IDSSTAGNAME" _
            & "    , COSMOTAGCODE" _
            & "    , COSMOTAGNAME" _
            & "    , ALLINSPECTIONDATE" _
            & "    , PREINSPECTIONDATE" _
            & "    , OBTAINEDNAME" _
            & "    , EXCLUDEDATE" _
            & "    , RETIRMENTDATE" _
            & "    , JRTANKTYPE" _
            & "    , JXTGTANKNUMBER2" _
            & "    , JXTGTANKNUMBER3" _
            & "    , JXTGTANKNUMBER4" _
            & "    , SAPSHELLTANKNUMBER" _
            & "    , INITYMD" _
            & "    , INITUSER" _
            & "    , INITTERMID" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & "    , CAST(UPDTIMSTP As bigint) As UPDTIMSTP" _
            & " FROM" _
            & "    OIL.OIM0005_TANK" _
            & " WHERE" _
            & "        TANKNUMBER = @P01"

        Dim WW_UPDFLG As String = ""
        Try

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 8)           'JOT車番
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)           '油種中分類コード
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 10)          '油種中分類名
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.Date)                  'ダウンロード日
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.DateTime)              '更新年月日
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 20)          '更新ユーザーＩＤ
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.NVarChar, 20)          '更新端末
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.DateTime)              '集信日時

                Dim JPARA01 As SqlParameter = SQLcmdJnl.Parameters.Add("@P01", SqlDbType.NVarChar, 8)       'JOT車番

                For Each OIM0005row As DataRow In OIM0005tbl.Rows
                    'DB更新ボタン押下の場合
                    If iProc = "UPD" AndAlso
                       OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
                        Dim WW_DATENOW As DateTime = Date.Now

                        'DB更新
                        PARA01.Value = OIM0005row("TANKNUMBER")
                        PARA02.Value = OIM0005row("MIDDLEOILCODE")
                        PARA03.Value = OIM0005row("MIDDLEOILNAME")
                        PARA04.Value = WW_DATENOW
                        PARA05.Value = WW_DATENOW
                        PARA06.Value = Master.USERID
                        PARA07.Value = Master.USERTERMID
                        PARA08.Value = C_DEFAULT_YMD

                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()
                        WW_UPDFLG = "ON"

                        OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                        '更新ジャーナル出力
                        JPARA01.Value = OIM0005row("TANKNUMBER")
                        Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                            If IsNothing(OIM0005UPDtbl) Then
                                OIM0005UPDtbl = New DataTable

                                For index As Integer = 0 To SQLdr.FieldCount - 1
                                    OIM0005UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                Next
                            End If

                            OIM0005UPDtbl.Clear()
                            OIM0005UPDtbl.Load(SQLdr)
                        End Using

                        For Each OIM0005UPDrow As DataRow In OIM0005UPDtbl.Rows
                            CS0020JOURNAL.TABLENM = "OIM0005LK"
                            CS0020JOURNAL.ACTION = "UPDATE"
                            CS0020JOURNAL.ROW = OIM0005UPDrow
                            CS0020JOURNAL.CS0020JOURNAL()
                            If Not isNormal(CS0020JOURNAL.ERR) Then
                                Master.Output(CS0020JOURNAL.ERR, C_MESSAGE_TYPE.ABORT, "CS0020JOURNAL JOURNAL")

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

                    'ダウンロードボタン押下の場合
                    If iProc = "DOWNLOAD" AndAlso
                       OIM0005row("PROCKBN") <> "" Then
                        Dim WW_DATENOW As DateTime = Date.Now

                        'DB更新
                        PARA01.Value = OIM0005row("TANKNUMBER")
                        PARA02.Value = OIM0005row("MIDDLEOILCODE")
                        PARA03.Value = OIM0005row("MIDDLEOILNAME")
                        PARA04.Value = WW_DATENOW
                        PARA05.Value = WW_DATENOW
                        PARA06.Value = Master.USERID
                        PARA07.Value = Master.USERTERMID
                        PARA08.Value = C_DEFAULT_YMD

                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()
                        WW_UPDFLG = "ON"

                        '更新ジャーナル出力
                        JPARA01.Value = OIM0005row("TANKNUMBER")
                        Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                            If IsNothing(OIM0005UPDtbl) Then
                                OIM0005UPDtbl = New DataTable

                                For index As Integer = 0 To SQLdr.FieldCount - 1
                                    OIM0005UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                Next
                            End If

                            OIM0005UPDtbl.Clear()
                            OIM0005UPDtbl.Load(SQLdr)
                        End Using

                        For Each OIM0005UPDrow As DataRow In OIM0005UPDtbl.Rows
                            CS0020JOURNAL.TABLENM = "OIM0005LK"
                            CS0020JOURNAL.ACTION = "UPDATE"
                            CS0020JOURNAL.ROW = OIM0005UPDrow
                            CS0020JOURNAL.CS0020JOURNAL()
                            If Not isNormal(CS0020JOURNAL.ERR) Then
                                Master.Output(CS0020JOURNAL.ERR, C_MESSAGE_TYPE.ABORT, "CS0020JOURNAL JOURNAL")

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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0005LK UPDATE")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0005LK UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        If WW_UPDFLG = "ON" Then
            Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)
        End If

    End Sub

    ''' <summary>
    ''' 一覧ﾀﾞｳﾝﾛｰﾄﾞ(Excel出力)ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDownload_Click()

        '******************************
        '帳票作成処理の実行
        '******************************
        Dim officeCode As String = BaseDllConst.CONST_OFFICECODE_011202
        Using repCbj = New OIM0005CustomReport(Master.MAPID, Master.MAPID & ".xlsx", OIM0005tbl)
            Dim url As String
            Try
                url = repCbj.CreateExcelPrintData(officeCode)
            Catch ex As Exception
                Return
            End Try
            '○ 別画面でExcelを表示
            WF_PrintURL.Value = url
            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
        End Using

    End Sub

    ''' <summary>
    ''' CSVﾀﾞｳﾝﾛｰﾄﾞボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonPrint_Click()

        Dim WW_ERR As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        '関連チェック
        For Each OIM0005row As DataRow In OIM0005tbl.Rows
            If Trim(OIM0005row("PROCKBN")) <> Nothing Then
                If Trim(OIM0005row("MIDDLEOILCODE")) = Nothing Then
                    OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_CheckMES1 = "・油種中分類が未入力です。"
                    WW_CheckMES2 = "項番 = " & OIM0005row("LINECNT")
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_ERR = "ERR"
                End If
            End If
        Next
        If WW_ERR = "ERR" Then
            '○ 画面表示データ保存
            Master.SaveTable(OIM0005tbl)
            Master.Output(C_MESSAGE_NO.ERROR_RECORD_EXIST, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Exit Sub
        End If

        '******************************
        'DB更新（ダウウンロード日）
        '******************************
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            'マスタ更新
            UpdateMaster(SQLcon, "DOWNLOAD")
        End Using

        '******************************
        'CSV作成処理の実行
        '******************************
        Dim WW_OIM0005tbl As DataTable = Nothing
        Dim WW_TBLview = New DataView(OIM0005tbl)
        WW_TBLview.RowFilter = "PROCKBN <> ''"
        WW_OIM0005tbl = WW_TBLview.ToTable

        If WW_OIM0005tbl.Rows.Count > 0 Then
            '車番変換＆屯数編集（小数点なし）
            For Each WW_OIM0005row As DataRow In WW_OIM0005tbl.Rows
                Dim LOADcnv As Integer = CInt(WW_OIM0005row("LOAD"))
                WW_OIM0005row("LOAD") = LOADcnv
                If Trim(WW_OIM0005row("MODEL")) = "タキ1000" Then
                    Dim TANKcnv As Integer = CInt(WW_OIM0005row("TANKNUMBER")) + 1000000
                    WW_OIM0005row("TANKNUMBER") = TANKcnv.ToString
                End If
            Next

            '出力ファイル編集（処理区分,車番,油種中分類,屯数）
            WW_TBLview = New DataView(WW_OIM0005tbl)
            Dim isDistinct As Boolean = True
            Dim cols() As String = {"PROCKBN", "TANKNUMBER", "MIDDLEOILCODE", "LOAD"}
            WW_OIM0005tbl = WW_TBLview.ToTable(isDistinct, cols)

            Dim OTFileName As String = "TCMASCMV.csv"
            Using repCbj = New CsvCreate(WW_OIM0005tbl, I_FileName:=OTFileName, I_Enc:="Shift_JIS")
                Dim url As String
                Try
                    url = repCbj.ConvertDataTableToCsv(False, False, True)
                Catch ex As Exception
                    Return
                End Try
                '○ CSVをダウンロード
                WF_PrintURL.Value = url
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
            End Using
        End If

        'ダウンロードの場合、操作欄（OPERATION）をキープ（再表示する）
        WW_TBLview = New DataView(OIM0005tbl)
        WW_TBLview.RowFilter = "PROCKBN <> '' or OPERATION <> ''"
        WW_OIM0005tbl = WW_TBLview.ToTable

        '○ GridView初期設定
        '○ 画面表示データ再取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            OIM0005tbl = Nothing
            MAPDataGet(SQLcon)

            '操作欄の再設定（未更新の場合更新ボタンで更新できるように）
            For Each WW_OIM0005row As DataRow In WW_OIM0005tbl.Rows
                Dim updHeader = OIM0005tbl.AsEnumerable.
                    FirstOrDefault(Function(x) x.Item("TANKNUMBER") = WW_OIM0005row("TANKNUMBER"))
                If IsNothing(updHeader) Then Continue For
                updHeader.Item("OPERATION") = WW_OIM0005row("OPERATION")
                updHeader.Item("MIDDLEOILCODE") = WW_OIM0005row("MIDDLEOILCODE")
                updHeader.Item("MIDDLEOILNAME") = WW_OIM0005row("MIDDLEOILNAME")
            Next
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(OIM0005tbl)

        WW_TBLview.Dispose()
        WW_TBLview = Nothing
        WW_OIM0005tbl.Dispose()
        WW_OIM0005tbl = Nothing

    End Sub


    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        Master.TransitionPrevPage(work.WF_SEL_CAMPCODE.Text & "1")

    End Sub


    ''' <summary>
    ''' 先頭頁ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonFIRST_Click()

        '○ 先頭頁に移動
        WF_GridPosition.Text = "1"

    End Sub

    ''' <summary>
    ''' 最終頁ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonLAST_Click()

        '○ ソート
        Dim TBLview As New DataView(OIM0005tbl)
        TBLview.RowFilter = "HIDDEN = 0"

        '○ 最終頁に移動
        If TBLview.Count Mod 10 = 0 Then
            WF_GridPosition.Text = TBLview.Count - (TBLview.Count Mod 10)
        Else
            WF_GridPosition.Text = TBLview.Count - (TBLview.Count Mod 10) + 1
        End If

        TBLview.Dispose()
        TBLview = Nothing

    End Sub


    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_Scroll()

    End Sub


    ''' <summary>
    ''' 詳細画面初期化
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailBoxClear()

        '○ 状態をクリア
        For Each OIM0005row As DataRow In OIM0005tbl.Rows
            Select Case OIM0005row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIM0005tbl)

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

        Dim WW_TEXT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""
        Dim dateErrFlag As String = ""

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
            WW_CheckMES1 = "・ユーザ更新権限がありません。"
            WW_CheckMES2 = ""
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Exit Sub
        End If

        '○ 単項目チェック
        For Each OIM0005row As DataRow In OIM0005tbl.Rows

            If OIM0005row("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            '処理区分(バリデーションチェック）
            WW_TEXT = OIM0005row("PROCKBN")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "PROCKBN", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("PROCKBN", OIM0005row("PROCKBN"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・存在しない処理区分です。"
                    WW_CheckMES2 = "項番 = " & OIM0005row("LINECNT")
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                End If
            Else
                WW_CheckMES1 = "・処理区分エラーです。"
                WW_CheckMES2 = "項番 = " & OIM0005row("LINECNT")
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If

            '油種中分類(バリデーションチェック）
            WW_TEXT = OIM0005row("MIDDLEOILCODE")
            If WW_TEXT = "" Then
                WW_CheckMES1 = "・油種中分類が未入力です。"
                WW_CheckMES2 = "項番 = " & OIM0005row("LINECNT")
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            Else
                Master.CheckField(work.WF_SEL_CAMPCODE.Text, "MIDDLEOILCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If isNormal(WW_CS0024FCHECKERR) Then
                    '値存在チェック
                    CODENAME_get("MIDDLEOILCODE", OIM0005row("MIDDLEOILCODE"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・存在しない油種中分類です。"
                        WW_CheckMES2 = "項番 = " & OIM0005row("LINECNT")
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    End If
                Else
                    WW_CheckMES1 = "・油種中分類エラーです。"
                    WW_CheckMES2 = "項番 = " & OIM0005row("LINECNT")
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                End If
            End If
        Next

    End Sub


    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="OIM0005row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIM0005row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIM0005row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> JOT車番 =" & OIM0005row("TANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 形式 =" & OIM0005row("MODEL") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種中分類コード =" & OIM0005row("MIDDLEOILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種中分類名 =" & OIM0005row("MIDDLEOILNAME") & " , "

        End If

        rightview.AddErrorReport(WW_ERR_MES)

    End Sub

    ''' <summary>
    ''' 遷移先(登録画面)退避データ保存先の作成
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_CreateXMLSaveFile()
        work.WF_SEL_INPTBL.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
            Master.USERID & "-" & Master.MAPID & "-" & CS0050SESSION.VIEW_MAP_VARIANT & "-" & Date.Now.ToString("HHmmss") & "INPTBL.txt"

    End Sub


    ''' <summary>
    ''' 名称取得
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByVal I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String)

        O_TEXT = ""
        O_RTN = ""

        If I_VALUE = "" Then
            O_RTN = C_MESSAGE_NO.NORMAL
            Exit Sub
        End If
        Dim prmData As New Hashtable

        Try
            prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

            Select Case I_FIELD
                Case "PROCKBN"                     '処理区分
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "PROCKBN")
                    leftview.CodeToName("999", I_VALUE, O_TEXT, O_RTN, prmData)

                Case "MIDDLEOILCODE"               '油種中分類
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "MIDDLEOILCODE")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_MIDDLEOILCODE, I_VALUE, O_TEXT, O_RTN, prmData)

            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
