Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' 積込予約マスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class OIM0021LoadReserveCreate
    Inherits Page

    '○ 検索結果格納Table
    Private OIM0021tbl As DataTable                                  '一覧格納用テーブル
    Private OIM0021INPtbl As DataTable                               'チェック用テーブル
    Private OIM0021UPDtbl As DataTable                               '更新用テーブル

    Private Const CONST_DISPROWCOUNT As Integer = 45                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 20                 'マウススクロール時稼働行数

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
                    Master.RecoverTable(OIM0021tbl, work.WF_SEL_INPTBL.Text)

                    Select Case WF_ButtonClick.Value
                        Case "WF_UPDATE"                '表更新ボタン押下
                            WF_UPDATE_Click()
                        Case "WF_CLEAR"                 'クリアボタン押下
                            WF_CLEAR_Click()
                        Case "WF_Field_DBClick"         'フィールドダブルクリック
                            WF_FIELD_DBClick()
                        Case "WF_LeftBoxSelectClick"    'フィールドチェンジ
                            WF_FIELD_Change()
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
            If Not IsNothing(OIM0021tbl) Then
                OIM0021tbl.Clear()
                OIM0021tbl.Dispose()
                OIM0021tbl = Nothing
            End If

            If Not IsNothing(OIM0021INPtbl) Then
                OIM0021INPtbl.Clear()
                OIM0021INPtbl.Dispose()
                OIM0021INPtbl = Nothing
            End If

            If Not IsNothing(OIM0021UPDtbl) Then
                OIM0021UPDtbl.Clear()
                OIM0021UPDtbl.Dispose()
                OIM0021UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIM0021WRKINC.MAPIDC
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
        rightview.COMPCODE = Master.USERCAMP
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0021L Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        End If

        '入力制限(数値0～9、.)
        WF_LOAD.Attributes("onkeyPress") = "CheckNumDot()"
        WF_RESERVEDQUANTITY.Attributes("onkeyPress") = "CheckNumDot()"

        '○ 名称設定処理
        '選択行
        WF_Sel_LINECNT.Text = work.WF_SEL_LINECNT.Text

        '管轄営業所
        WF_OFFICECODE.Text = work.WF_SEL_OFFICECODE2.Text
        CODENAME_get("OFFICECODE", WF_OFFICECODE.Text, WF_OFFICECODE_TEXT.Text, WW_RTN_SW)

        '適用開始年月日
        WF_FROMYMD.Text = work.WF_SEL_FROMYMD2.Text

        '適用終了年月日
        WF_TOYMD.Text = work.WF_SEL_TOYMD2.Text

        '型式 
        WF_MODEL.Text = work.WF_SEL_MODEL.Text

        '荷重
        WF_LOAD.Text = work.WF_SEL_LOAD2.Text

        '油種コード
        WF_OILCODE.Text = work.WF_SEL_OILCODE2.Text
        CODENAME_get("OILCODE", WF_OILCODE.Text, WF_OILCODE_TEXT.Text, WW_RTN_SW)

        '油種細分コード
        WF_SEGMENTOILCODE.Text = work.WF_SEL_SEGMENTOILCODE2.Text
        CODENAME_get("SEGMENTOILCODE", WF_SEGMENTOILCODE.Text, WF_SEGMENTOILCODE_TEXT.Text, WW_RTN_SW)

        '予約数量
        WF_RESERVEDQUANTITY.Text = work.WF_SEL_RESERVEDQUANTITY.Text

        '削除フラグ
        WF_DELFLG.Text = work.WF_SEL_DELFLG.Text
        CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_RTN_SW)

    End Sub

    ''' <summary>
    ''' 一意制約チェック
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub UniqueKeyCheck(ByVal SQLcon As SqlConnection, ByRef O_MESSAGENO As String)

        '○ 対象データ取得
        Dim SQLStr As String =
              " SELECT " _
            & "     OFFICECODE " _
            & "     , FROMYMD " _
            & "     , TOYMD " _
            & "     , MODEL " _
            & "     , LOAD " _
            & "     , OILCODE " _
            & "     , SEGMENTOILCODE " _
            & "     , RESERVEDQUANTITY " _
            & " FROM" _
            & "    OIL.OIM0021_LOADRESERVE" _
            & " WHERE" _
            & "     OFFICECODE      = @P1" _
            & " AND FROMYMD         = @P2" _
            & " AND LOAD            = @P3" _
            & " AND OILCODE         = @P4" _
            & " AND SEGMENTOILCODE  = @P5" _
            & " AND DELFLG         <> @P6"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 6)     '管轄営業所
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.Date)            '適用開始年月日
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.Float)           '荷重
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 10)    '油種コード
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 1)     '油種細分コード
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 1)     '削除フラグ
                PARA1.Value = WF_OFFICECODE.Text
                PARA2.Value = Date.Parse(WF_FROMYMD.Text)
                PARA3.Value = WF_LOAD.Text
                PARA4.Value = WF_OILCODE.Text
                PARA5.Value = WF_SEGMENTOILCODE.Text
                PARA6.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    Dim OIM0021Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIM0021Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIM0021Chk.Load(SQLdr)

                    If OIM0021Chk.Rows.Count > 0 Then
                        '重複データエラー
                        O_MESSAGENO = Messages.C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR
                    Else
                        '正常終了時
                        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0021C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0021C UPDATE_INSERT"
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
        DetailBoxToOIM0021INPtbl(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ERR_SW) Then
            OIM0021tbl_UPD()
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIM0021tbl, work.WF_SEL_INPTBL.Text)

        '○ メッセージ表示
        If WW_ERR_SW = "" Then
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)
        Else
            If isNormal(WW_ERR_SW) Then
                Master.Output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.INF)
            ElseIf WW_ERR_SW = C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR Then
                Master.Output(WW_ERR_SW, C_MESSAGE_TYPE.ERR, "管轄営業所, 適用開始年月日, 荷重, 油種コード, 油種細分コード", needsPopUp:=True)
            Else
                Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
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
    Protected Sub DetailBoxToOIM0021INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(WF_DELFLG.Text)            '削除フラグ

        '○ GridViewから未選択状態で表更新ボタンを押下時の例外を回避する
        If String.IsNullOrEmpty(WF_Sel_LINECNT.Text) AndAlso
            String.IsNullOrEmpty(WF_DELFLG.Text) Then
            Master.Output(C_MESSAGE_NO.INVALID_PROCCESS_ERROR, C_MESSAGE_TYPE.ERR, "no Detail", needsPopUp:=True)

            CS0011LOGWrite.INFSUBCLASS = "DetailBoxToINPtbl"        'SUBクラス名
            CS0011LOGWrite.INFPOSI = "non Detail"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWrite.TEXT = "non Detail"
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                         'ログ出力

            O_RTN = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            Exit Sub
        End If

        Master.CreateEmptyTable(OIM0021INPtbl, work.WF_SEL_INPTBL.Text)
        Dim OIM0021INProw As DataRow = OIM0021INPtbl.NewRow

        '○ 初期クリア
        For Each OIM0021INPcol As DataColumn In OIM0021INPtbl.Columns
            If IsDBNull(OIM0021INProw.Item(OIM0021INPcol)) OrElse IsNothing(OIM0021INProw.Item(OIM0021INPcol)) Then
                Select Case OIM0021INPcol.ColumnName
                    Case "LINECNT"
                        OIM0021INProw.Item(OIM0021INPcol) = 0
                    Case "OPERATION"
                        OIM0021INProw.Item(OIM0021INPcol) = C_LIST_OPERATION_CODE.NODATA
                    Case "UPDTIMSTP"
                        OIM0021INProw.Item(OIM0021INPcol) = 0
                    Case "SELECT"
                        OIM0021INProw.Item(OIM0021INPcol) = 1
                    Case "HIDDEN"
                        OIM0021INProw.Item(OIM0021INPcol) = 0
                    Case Else
                        OIM0021INProw.Item(OIM0021INPcol) = ""
                End Select
            End If
        Next

        'LINECNT
        If WF_Sel_LINECNT.Text = "" Then
            OIM0021INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(WF_Sel_LINECNT.Text, OIM0021INProw("LINECNT"))
            Catch ex As Exception
                OIM0021INProw("LINECNT") = 0
            End Try
        End If

        OIM0021INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        OIM0021INProw("UPDTIMSTP") = 0
        OIM0021INProw("SELECT") = 1
        OIM0021INProw("HIDDEN") = 0

        OIM0021INProw("OFFICECODE") = WF_OFFICECODE.Text                '管轄営業所
        OIM0021INProw("FROMYMD") = WF_FROMYMD.Text                      '適用開始年月日
        OIM0021INProw("TOYMD") = WF_TOYMD.Text                          '適用終了年月日
        OIM0021INProw("MODEL") = WF_MODEL.Text                          '型式
        OIM0021INProw("LOAD") = WF_LOAD.Text                            '荷重
        OIM0021INProw("OILCODE") = WF_OILCODE.Text                      '油種コード
        OIM0021INProw("SEGMENTOILCODE") = WF_SEGMENTOILCODE.Text        '油種細分コード
        OIM0021INProw("RESERVEDQUANTITY") = WF_RESERVEDQUANTITY.Text    '予約数量
        OIM0021INProw("DELFLG") = WF_DELFLG.Text                        '削除フラグ

        '○ チェック用テーブルに登録する
        OIM0021INPtbl.Rows.Add(OIM0021INProw)

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
        For Each OIM0021row As DataRow In OIM0021tbl.Rows
            Select Case OIM0021row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0021row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0021row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0021row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0021row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0021row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIM0021tbl, work.WF_SEL_INPTBL.Text)

        WF_Sel_LINECNT.Text = ""        'LINECNT

        WF_OFFICECODE.Text = ""         '管轄営業所
        WF_FROMYMD.Text = ""            '適用開始年月日
        WF_TOYMD.Text = ""              '適用終了年月日
        WF_MODEL.Text = ""              '型式
        WF_LOAD.Text = ""               '荷重
        WF_OILCODE.Text = ""            '油種コード
        WF_SEGMENTOILCODE.Text = ""     '油種細分コード
        WF_RESERVEDQUANTITY.Text = ""   '予約数量
        WF_DELFLG.Text = ""             '削除フラグ

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

                Select Case WF_LeftMViewChange.Value

                    Case LIST_BOX_CLASSIFICATION.LC_CALENDAR

                        Select Case WF_FIELD.Value
                            Case WF_FROMYMD.ID
                                '適用開始年月日
                                .WF_Calendar.Text = work.WF_SEL_FROMYMD.Text

                            Case WF_TOYMD.ID
                                '適用終了年月日
                                .WF_Calendar.Text = work.WF_SEL_TOYMD.Text

                        End Select
                        .ActiveCalendar()

                    Case Else

                        Dim prmData As New Hashtable

                        'フィールドによってパラメータを変える
                        Select Case WF_FIELD.Value

                            Case WF_OFFICECODE.ID
                                '管轄営業所
                                prmData = work.CreateOfficeCodeParam(Master.USER_ORG)

                            Case WF_OILCODE.ID
                                '油種コード
                                prmData = work.CreateFIXParam(Master.USERCAMP, "OILCODE")

                            Case WF_SEGMENTOILCODE.ID
                                '油種細分コード
                                prmData = work.CreateFIXParam(Master.USERCAMP, "SEGMENTOILCODE")

                            Case WF_DELFLG.ID
                                '削除フラグ
                                prmData = work.CreateFIXParam(Master.USERCAMP, "DELFLG")

                        End Select

                        .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                        .ActiveListBox()
                End Select

            End With
        End If

    End Sub

    ''' <summary>
    ''' フィールドチェンジ時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_Change()

        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value
            Case WF_DELFLG.ID
                '削除フラグ
                CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_RTN_SW)
            Case WF_OFFICECODE.ID
                '管轄営業所
                CODENAME_get("OFFICECODE", WF_OFFICECODE.Text, WF_OFFICECODE_TEXT.Text, WW_RTN_SW)
            Case WF_OILCODE.ID
                '油種コード
                CODENAME_get("OILCODE", WF_OILCODE.Text, WF_OILCODE_TEXT.Text, WW_RTN_SW)
            Case WF_SEGMENTOILCODE.ID
                '油種細分コード
                CODENAME_get("SEGMENTOILCODE", WF_SEGMENTOILCODE.Text, WF_SEGMENTOILCODE_TEXT.Text, WW_RTN_SW)
        End Select

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

        '○ 選択内容を画面項目へセット
        If WF_FIELD_REP.Value = "" Then
            Select Case WF_FIELD.Value

                Case WF_DELFLG.ID
                    '削除フラグ
                    WF_DELFLG.Text = WW_SelectValue
                    WF_DELFLG_TEXT.Text = WW_SelectText
                    WF_DELFLG.Focus()

                Case WF_OFFICECODE.ID
                    '管轄営業所
                    WF_OFFICECODE.Text = WW_SelectValue
                    WF_OFFICECODE_TEXT.Text = WW_SelectText
                    WF_OFFICECODE.Focus()

                Case WF_FROMYMD.ID
                    '適用開始年月日
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_FROMYMD.Text = ""
                        Else
                            WF_FROMYMD.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_FROMYMD.Focus()

                Case WF_TOYMD.ID
                    '適用終了年月日
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_TOYMD.Text = ""
                        Else
                            WF_TOYMD.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_TOYMD.Focus()

                Case WF_OILCODE.ID
                    '油種コード
                    WF_OILCODE.Text = WW_SelectValue
                    WF_OILCODE_TEXT.Text = WW_SelectText
                    WF_OILCODE.Focus()

                Case WF_SEGMENTOILCODE.ID
                    '油種細分コード
                    WF_SEGMENTOILCODE.Text = WW_SelectValue
                    WF_SEGMENTOILCODE_TEXT.Text = WW_SelectText
                    WF_SEGMENTOILCODE.Focus()

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

                Case WF_DELFLG.ID
                    '削除フラグ
                    WF_DELFLG.Focus()

                Case WF_OFFICECODE.ID
                    '管轄営業所
                    WF_OFFICECODE.Focus()

                Case WF_FROMYMD.ID
                    '適用開始年月日
                    WF_FROMYMD.Focus()

                Case WF_TOYMD.ID
                    '適用終了年月日
                    WF_TOYMD.Focus()

                Case WF_OILCODE.ID
                    '油種コード
                    WF_OILCODE.Focus()

                Case WF_SEGMENTOILCODE.ID
                    '油種細分コード
                    WF_SEGMENTOILCODE.Focus()

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
        Dim dateErrFlag As String = ""
        Dim WW_UniqueKeyCHECK As String = ""
        Dim WW_PKEY_ERR As String = ""

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
        For Each OIM0021INProw As DataRow In OIM0021INPtbl.Rows

            WW_LINE_ERR = ""

            '削除フラグ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "DELFLG", OIM0021INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("DELFLG", OIM0021INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0021INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0021INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '管轄営業所(バリデーションチェック)
            WW_TEXT = OIM0021INProw("OFFICECODE")
            Master.CheckField(Master.USERCAMP, "OFFICECODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("OFFICECODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(管轄営業所入力エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0021INProw)
                    WW_LINE_ERR = "ERR"
                    WW_PKEY_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(管轄営業所入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0021INProw)
                WW_LINE_ERR = "ERR"
                WW_PKEY_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If


            '適用開始年月日
            WW_TEXT = OIM0021INProw("FROMYMD")
            Master.CheckField(Master.USERCAMP, "FROMYMD", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WF_FROMYMD.Text <> "" Then
                    '年月日チェック
                    WW_CheckDate(WF_FROMYMD.Text, "適用開始年月日", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(適用開始年月日エラー)です。"
                        WW_CheckMES2 = WW_CS0024FCHECKERR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0021INProw)
                        WW_LINE_ERR = "ERR"
                        WW_PKEY_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0021INProw("FROMYMD") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(適用開始年月日エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKERR
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0021INProw)
                WW_LINE_ERR = "ERR"
                WW_PKEY_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '適用終了年月日
            WW_TEXT = OIM0021INProw("TOYMD")
            Master.CheckField(Master.USERCAMP, "TOYMD", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WF_TOYMD.Text <> "" Then
                    '年月日チェック
                    WW_CheckDate(WF_TOYMD.Text, "適用終了年月日", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(適用終了年月日エラー)です。"
                        WW_CheckMES2 = WW_CS0024FCHECKERR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0021INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0021INProw("TOYMD") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(適用終了年月日エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKERR
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0021INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '型式（バリデーションチェック）
            WW_TEXT = OIM0021INProw("MODEL")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "MODEL", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(型式エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0021INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '荷重
            WW_TEXT = OIM0021INProw("LOAD")
            Master.CheckField(Master.USERCAMP, "LOAD", WF_LOAD.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(荷重エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0021INProw)
                WW_LINE_ERR = "ERR"
                WW_PKEY_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種コード(バリデーションチェック)
            WW_TEXT = OIM0021INProw("OILCODE")
            Master.CheckField(Master.USERCAMP, "OILCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("OILCODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(油種コード入力エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0021INProw)
                    WW_LINE_ERR = "ERR"
                    WW_PKEY_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(油種コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0021INProw)
                WW_LINE_ERR = "ERR"
                WW_PKEY_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種細分コード(バリデーションチェック)
            WW_TEXT = OIM0021INProw("SEGMENTOILCODE")
            Master.CheckField(Master.USERCAMP, "SEGMENTOILCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("SEGMENTOILCODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(油種細分コード入力エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0021INProw)
                    WW_LINE_ERR = "ERR"
                    WW_PKEY_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(油種細分コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0021INProw)
                WW_LINE_ERR = "ERR"
                WW_PKEY_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '予約数量
            WW_TEXT = OIM0021INProw("RESERVEDQUANTITY")
            Master.CheckField(Master.USERCAMP, "RESERVEDQUANTITY", WF_RESERVEDQUANTITY.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(予約数量エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0021INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '一意制約チェック
            'プライマリキーの項目エラーがある場合、又は同一レコードの更新の場合、チェック対象外
            If WW_PKEY_ERR.Equals("ERR") OrElse
                (OIM0021INProw("OFFICECODE") = work.WF_SEL_OFFICECODE2.Text AndAlso
                OIM0021INProw("FROMYMD") = work.WF_SEL_FROMYMD2.Text AndAlso
                OIM0021INProw("LOAD") = work.WF_SEL_LOAD2.Text AndAlso
                OIM0021INProw("OILCODE") = work.WF_SEL_OILCODE2.Text AndAlso
                OIM0021INProw("SEGMENTOILCODE") = work.WF_SEL_SEGMENTOILCODE2.Text) Then
            Else
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    'DataBase接続
                    SQLcon.Open()

                    '一意制約チェック
                    UniqueKeyCheck(SQLcon, WW_UniqueKeyCHECK)
                End Using

                If Not isNormal(WW_UniqueKeyCHECK) Then
                    WW_CheckMES1 = "一意制約違反（管轄営業所, 適用開始年月日, 荷重, 油種コード, 油種細分コード）。"
                    WW_CheckMES2 = C_MESSAGE_NO.OVERLAP_DATA_ERROR &
                                       "([" & OIM0021INProw("OFFICECODE") &
                                       ", " & OIM0021INProw("FROMYMD") &
                                       ", " & OIM0021INProw("LOAD") &
                                       ", " & OIM0021INProw("OILCODE") &
                                       ", " & OIM0021INProw("SEGMENTOILCODE") & "]"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0021INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR
                End If
            End If

            If WW_LINE_ERR = "" Then
                If OIM0021INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    OIM0021INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LINE_ERR = CONST_PATTERNERR Then
                    '関連チェックエラーをセット
                    OIM0021INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    '単項目チェックエラーをセット
                    OIM0021INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                End If
            End If
        Next

    End Sub

    ''' <summary>
    ''' 年月日チェック
    ''' </summary>
    ''' <param name="I_DATE"></param>
    ''' <param name="I_DATENAME"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckDate(ByVal I_DATE As String, ByVal I_DATENAME As String, ByVal I_VALUE As String, ByRef dateErrFlag As String)

        dateErrFlag = "1"
        Try
            '年取得
            Dim chkLeapYear As String = I_DATE.Substring(0, 4)
            '月日を取得
            Dim getMMDD As String = I_DATE.Remove(0, I_DATE.IndexOf("/") + 1)
            '月取得
            Dim getMonth As String = getMMDD.Remove(getMMDD.IndexOf("/"))
            '日取得
            Dim getDay As String = getMMDD.Remove(0, getMMDD.IndexOf("/") + 1)

            '閏年の場合はその旨のメッセージを出力
            If Not DateTime.IsLeapYear(chkLeapYear) _
            AndAlso (getMonth = "2" OrElse getMonth = "02") AndAlso getDay = "29" Then
                Master.Output(C_MESSAGE_NO.OIL_LEAPYEAR_NOTFOUND, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
                '月と日の範囲チェック
            ElseIf getMonth >= 13 OrElse getDay >= 32 Then
                Master.Output(C_MESSAGE_NO.OIL_MONTH_DAY_OVER_ERROR, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
            Else
                'Master.Output(I_VALUE, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
                'エラーなし
                dateErrFlag = "0"
            End If
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DATE_FORMAT_ERROR, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
        End Try

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="OIM0021row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIM0021row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIM0021row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 管轄営業所 =" & OIM0021row("OFFICECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 適用開始年月日 =" & OIM0021row("FROMYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 適用終了年月日 =" & OIM0021row("TOYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 型式 =" & OIM0021row("MODEL") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 荷重 =" & OIM0021row("LOAD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種コード =" & OIM0021row("OILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種細分コード =" & OIM0021row("SEGMENTOILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 予約数量 =" & OIM0021row("RESERVEDQUANTITY") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除フラグ =" & OIM0021row("DELFLG")
        End If

        rightview.AddErrorReport(WW_ERR_MES)

    End Sub


    ''' <summary>
    ''' OIM0021tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub OIM0021tbl_UPD()

        '○ 画面状態設定
        For Each OIM0021row As DataRow In OIM0021tbl.Rows
            Select Case OIM0021row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0021row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0021row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0021row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0021row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0021row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each OIM0021INProw As DataRow In OIM0021INPtbl.Rows

            'エラーレコード読み飛ばし
            If OIM0021INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            OIM0021INProw.Item("OPERATION") = CONST_INSERT

            ' 既存レコードとの比較
            For Each OIM0021row As DataRow In OIM0021tbl.Rows
                ' KEY項目が等しい時
                If OIM0021row("OFFICECODE") = OIM0021INProw("OFFICECODE") AndAlso
                    OIM0021row("FROMYMD") = OIM0021INProw("FROMYMD") AndAlso
                    OIM0021row("LOAD") = OIM0021INProw("LOAD") AndAlso
                    OIM0021row("OILCODE") = OIM0021INProw("OILCODE") AndAlso
                    OIM0021row("SEGMENTOILCODE") = OIM0021INProw("SEGMENTOILCODE") Then
                    ' KEY項目以外の項目の差異をチェック
                    If OIM0021row("DELFLG") = OIM0021INProw("DELFLG") AndAlso
                        OIM0021row("TOYMD") = OIM0021INProw("TOYMD") AndAlso
                        OIM0021row("MODEL") = OIM0021INProw("MODEL") AndAlso
                        OIM0021row("RESERVEDQUANTITY") = OIM0021INProw("RESERVEDQUANTITY") Then
                        ' 変更がないときは「操作」の項目は空白にする
                        OIM0021INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Else
                        ' 変更がある時は「操作」の項目を「更新」に設定する
                        OIM0021INProw("OPERATION") = CONST_UPDATE
                    End If

                    Exit For

                End If
            Next
        Next

        '○ 変更有無判定　&　入力値反映
        For Each OIM0021INProw As DataRow In OIM0021INPtbl.Rows
            Select Case OIM0021INProw("OPERATION")
                Case CONST_UPDATE
                    TBL_UPDATE_SUB(OIM0021INProw)
                Case CONST_INSERT
                    TBL_INSERT_SUB(OIM0021INProw)
                Case CONST_PATTERNERR
                    '関連チェックエラーの場合、キーが変わるため、行追加してエラーレコードを表示させる
                    TBL_INSERT_SUB(OIM0021INProw)
                Case C_LIST_OPERATION_CODE.ERRORED
                    TBL_ERR_SUB(OIM0021INProw)
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="OIM0021INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef OIM0021INProw As DataRow)

        For Each OIM0021row As DataRow In OIM0021tbl.Rows

            '同一レコードか判定
            If OIM0021INProw("OFFICECODE") = OIM0021row("OFFICECODE") AndAlso
                OIM0021INProw("FROMYMD") = OIM0021row("FROMYMD") AndAlso
                OIM0021INProw("LOAD") = OIM0021row("LOAD") AndAlso
                OIM0021INProw("OILCODE") = OIM0021row("OILCODE") AndAlso
                OIM0021INProw("SEGMENTOILCODE") = OIM0021row("SEGMENTOILCODE") Then
                '画面入力テーブル項目設定
                OIM0021INProw("LINECNT") = OIM0021row("LINECNT")
                OIM0021INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                OIM0021INProw("UPDTIMSTP") = OIM0021row("UPDTIMSTP")
                OIM0021INProw("SELECT") = 1
                OIM0021INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0021row.ItemArray = OIM0021INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 追加予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0021INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef OIM0021INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim OIM0021row As DataRow = OIM0021tbl.NewRow
        OIM0021row.ItemArray = OIM0021INProw.ItemArray

        OIM0021row("LINECNT") = OIM0021tbl.Rows.Count + 1
        If OIM0021INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
            OIM0021row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        Else
            OIM0021row("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
        End If

        OIM0021row("UPDTIMSTP") = "0"
        OIM0021row("SELECT") = 1
        OIM0021row("HIDDEN") = 0

        OIM0021tbl.Rows.Add(OIM0021row)

    End Sub


    ''' <summary>
    ''' エラーデータの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0021INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_ERR_SUB(ByRef OIM0021INProw As DataRow)

        For Each OIM0021row As DataRow In OIM0021tbl.Rows

            '同一レコードか判定
            If OIM0021INProw("OFFICECODE") = OIM0021row("OFFICECODE") AndAlso
                OIM0021INProw("FROMYMD") = OIM0021row("FROMYMD") AndAlso
                OIM0021INProw("LOAD") = OIM0021row("LOAD") AndAlso
                OIM0021INProw("OILCODE") = OIM0021row("OILCODE") AndAlso
                OIM0021INProw("SEGMENTOILCODE") = OIM0021row("SEGMENTOILCODE") Then
                '画面入力テーブル項目設定
                OIM0021INProw("LINECNT") = OIM0021row("LINECNT")
                OIM0021INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                OIM0021INProw("UPDTIMSTP") = OIM0021row("UPDTIMSTP")
                OIM0021INProw("SELECT") = 1
                OIM0021INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0021row.ItemArray = OIM0021INProw.ItemArray
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
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByVal I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String)

        O_TEXT = ""
        O_RTN = ""

        If I_VALUE = "" Then
            O_RTN = C_MESSAGE_NO.NORMAL
            Exit Sub
        End If

        Try
            Dim prmData As New Hashtable

            Select Case I_FIELD
                Case "OFFICECODE"
                    '管轄営業所
                    prmData = work.CreateOfficeCodeParam(Master.USERCAMP)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SALESOFFICE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "OILCODE"
                    '油種コード
                    prmData = work.CreateFIXParam(Master.USERCAMP, "OILCODE")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "SEGMENTOILCODE"
                    '油種細分コード
                    prmData = work.CreateFIXParam(Master.USERCAMP, "SEGMENTOILCODE")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "DELFLG"
                    '削除
                    prmData = work.CreateFIXParam(Master.USERCAMP, "DELFLG")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
