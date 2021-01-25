Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' 勘定科目マスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class OIM0019AccountCreate
    Inherits Page

    '○ 検索結果格納Table
    Private OIM0019tbl As DataTable                                 '一覧格納用テーブル
    Private OIM0019INPtbl As DataTable                              'チェック用テーブル
    Private OIM0019UPDtbl As DataTable                              '更新用テーブル

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
                    Master.RecoverTable(OIM0019tbl, work.WF_SEL_INPTBL.Text)

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
            If Not IsNothing(OIM0019tbl) Then
                OIM0019tbl.Clear()
                OIM0019tbl.Dispose()
                OIM0019tbl = Nothing
            End If

            If Not IsNothing(OIM0019INPtbl) Then
                OIM0019INPtbl.Clear()
                OIM0019INPtbl.Dispose()
                OIM0019INPtbl = Nothing
            End If

            If Not IsNothing(OIM0019UPDtbl) Then
                OIM0019UPDtbl.Clear()
                OIM0019UPDtbl.Dispose()
                OIM0019UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIM0019WRKINC.MAPIDC
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0019L Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        End If

        '○ 名称設定処理
        '選択行
        WF_Sel_LINECNT.Text = work.WF_SEL_LINECNT.Text

        '適用開始年月日
        WF_FROMYMD.Text = work.WF_SEL_FROMYMD2.Text

        '適用終了年月日
        WF_ENDYMD.Text = work.WF_SEL_ENDYMD2.Text

        '科目コード
        WF_ACCOUNTCODE.Text = work.WF_SEL_ACCOUNTCODE2.Text

        '科目名
        WF_ACCOUNTNAME.Text = work.WF_SEL_ACCOUNTNAME.Text

        'セグメント
        WF_SEGMENTCODE.Text = work.WF_SEL_SEGMENTCODE2.Text

        'セグメント名
        WF_SEGMENTNAME.Text = work.WF_SEL_SEGMENTNAME.Text

        'セグメント枝番
        WF_SEGMENTBRANCHCODE.Text = work.WF_SEL_SEGMENTBRANCHCODE2.Text

        'セグメント枝番名
        WF_SEGMENTBRANCHNAME.Text = work.WF_SEL_SEGMENTBRANCHNAME.Text

        '科目区分
        WF_ACCOUNTTYPE.Text = work.WF_SEL_ACCOUNTTYPE2.Text

        '科目区分名
        WF_ACCOUNTTYPENAME.Text = work.WF_SEL_ACCOUNTTYPENAME.Text

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
              " SELECT" _
            & "     FROMYMD" _
            & "     , ENDYMD" _
            & "     , ACCOUNTCODE" _
            & "     , ACCOUNTNAME" _
            & "     , SEGMENTCODE" _
            & "     , SEGMENTNAME" _
            & "     , SEGMENTBRANCHCODE" _
            & "     , SEGMENTBRANCHNAME" _
            & "     , ACCOUNTTYPE" _
            & "     , ACCOUNTTYPENAME" _
            & " FROM" _
            & "    OIL.OIM0019_ACCOUNT" _
            & " WHERE" _
            & "     FROMYMD           = @P01" _
            & " AND ENDYMD            = @P02" _
            & " AND ACCOUNTCODE       = @P03" _
            & " AND SEGMENTCODE       = @P04" _
            & " AND SEGMENTBRANCHCODE = @P05" _
            & " AND DELFLG           <> @P00"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA00 As SqlParameter = SQLcmd.Parameters.Add("@P00", SqlDbType.NVarChar, 1)   '削除フラグ
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.Date)          '適用開始年月日
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.Date)          '適用終了年月日
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 8)   '科目コード
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 5)   'セグメント
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 2)   'セグメント枝番

                PARA00.Value = C_DELETE_FLG.DELETE
                PARA01.Value = WF_FROMYMD.Text
                PARA02.Value = WF_ENDYMD.Text
                PARA03.Value = WF_ACCOUNTCODE.Text
                PARA04.Value = WF_SEGMENTCODE.Text
                PARA05.Value = WF_SEGMENTBRANCHCODE.Text

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    Dim OIM0019Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIM0019Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIM0019Chk.Load(SQLdr)

                    If OIM0019Chk.Rows.Count > 0 Then
                        '重複データエラー
                        O_MESSAGENO = Messages.C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR
                    Else
                        '正常終了時
                        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0019C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0019C UPDATE_INSERT"
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
        DetailBoxToOIM0019INPtbl(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ERR_SW) Then
            OIM0019tbl_UPD()
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIM0019tbl, work.WF_SEL_INPTBL.Text)

        '○ メッセージ表示
        If WW_ERR_SW = "" Then
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)
        Else
            If isNormal(WW_ERR_SW) Then
                Master.Output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.INF)
            ElseIf WW_ERR_SW = C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR Then
                Master.Output(WW_ERR_SW, C_MESSAGE_TYPE.ERR, "適用開始年月日, 適用終了年月日, 科目コード, セグメント, セグメント枝番", needsPopUp:=True)
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
    Protected Sub DetailBoxToOIM0019INPtbl(ByRef O_RTN As String)

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

        Master.CreateEmptyTable(OIM0019INPtbl, work.WF_SEL_INPTBL.Text)
        Dim OIM0019INProw As DataRow = OIM0019INPtbl.NewRow

        '○ 初期クリア
        For Each OIM0019INPcol As DataColumn In OIM0019INPtbl.Columns
            If IsDBNull(OIM0019INProw.Item(OIM0019INPcol)) OrElse IsNothing(OIM0019INProw.Item(OIM0019INPcol)) Then
                Select Case OIM0019INPcol.ColumnName
                    Case "LINECNT"
                        OIM0019INProw.Item(OIM0019INPcol) = 0
                    Case "OPERATION"
                        OIM0019INProw.Item(OIM0019INPcol) = C_LIST_OPERATION_CODE.NODATA
                    Case "UPDTIMSTP"
                        OIM0019INProw.Item(OIM0019INPcol) = 0
                    Case "SELECT"
                        OIM0019INProw.Item(OIM0019INPcol) = 1
                    Case "HIDDEN"
                        OIM0019INProw.Item(OIM0019INPcol) = 0
                    Case Else
                        OIM0019INProw.Item(OIM0019INPcol) = ""
                End Select
            End If
        Next

        'LINECNT
        If WF_Sel_LINECNT.Text = "" Then
            OIM0019INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(WF_Sel_LINECNT.Text, OIM0019INProw("LINECNT"))
            Catch ex As Exception
                OIM0019INProw("LINECNT") = 0
            End Try
        End If

        OIM0019INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        OIM0019INProw("UPDTIMSTP") = 0
        OIM0019INProw("SELECT") = 1
        OIM0019INProw("HIDDEN") = 0

        OIM0019INProw("FROMYMD") = WF_FROMYMD.Text                      '適用開始年月日
        OIM0019INProw("ENDYMD") = WF_ENDYMD.Text                        '適用終了年月日
        OIM0019INProw("ACCOUNTCODE") = WF_ACCOUNTCODE.Text              '科目コード
        OIM0019INProw("ACCOUNTNAME") = WF_ACCOUNTNAME.Text              '科目名
        OIM0019INProw("SEGMENTCODE") = WF_SEGMENTCODE.Text              'セグメント
        OIM0019INProw("SEGMENTNAME") = WF_SEGMENTNAME.Text              'セグメント名
        OIM0019INProw("SEGMENTBRANCHCODE") = WF_SEGMENTBRANCHCODE.Text  'セグメント枝番
        OIM0019INProw("SEGMENTBRANCHNAME") = WF_SEGMENTBRANCHNAME.Text  'セグメント枝番名
        OIM0019INProw("ACCOUNTTYPE") = WF_ACCOUNTTYPE.Text              '科目区分
        OIM0019INProw("ACCOUNTTYPENAME") = WF_ACCOUNTTYPENAME.Text      '科目区分名
        OIM0019INProw("DELFLG") = WF_DELFLG.Text                        '削除フラグ

        '○ チェック用テーブルに登録する
        OIM0019INPtbl.Rows.Add(OIM0019INProw)

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
        For Each OIM0019row As DataRow In OIM0019tbl.Rows
            Select Case OIM0019row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0019row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0019row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0019row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0019row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0019row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIM0019tbl, work.WF_SEL_INPTBL.Text)

        WF_Sel_LINECNT.Text = ""        'LINECNT

        WF_FROMYMD.Text = ""            '適用開始年月日
        WF_ENDYMD.Text = ""             '適用終了年月日
        WF_ACCOUNTCODE.Text = ""        '科目コード
        WF_ACCOUNTNAME.Text = ""        '科目名
        WF_SEGMENTCODE.Text = ""        'セグメント
        WF_SEGMENTNAME.Text = ""        'セグメント名
        WF_SEGMENTBRANCHCODE.Text = ""  'セグメント枝番
        WF_SEGMENTBRANCHNAME.Text = ""  'セグメント枝番名
        WF_ACCOUNTTYPE.Text = ""        '科目区分
        WF_ACCOUNTTYPENAME.Text = ""    '科目区分名

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

                            Case WF_ENDYMD.ID
                                '適用終了年月日
                                .WF_Calendar.Text = work.WF_SEL_ENDYMD.Text
                        End Select
                        .ActiveCalendar()

                    Case Else

                        Dim prmData As New Hashtable
                        Dim WW_FIXCODE As String = ""

                        'フィールドによってパラメータを変える
                        Select Case WF_FIELD.Value

                            Case WF_ACCOUNTCODE.ID
                                '科目コード
                                WW_FIXCODE = "ACCOUNTCODE"

                            Case WF_SEGMENTCODE.ID
                                'セグメント
                                WW_FIXCODE = "ACCOUNTSEGMENTCODE"

                            Case WF_ACCOUNTTYPE.ID
                                '科目区分
                                WW_FIXCODE = "ACCOUNTTYPE"

                            Case WF_DELFLG.ID
                                '削除フラグ
                                WW_FIXCODE = "DELFLG"

                        End Select

                        prmData = work.CreateFIXParam(Master.USERCAMP, WW_FIXCODE)
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

        Dim WW_CODE As String = ""
        Dim WW_NAME As String = ""

        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value
            Case WF_DELFLG.ID
                '削除フラグ
                CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_RTN_SW)

            Case WF_ACCOUNTCODE.ID
                '科目コード
                WW_CODE = WF_ACCOUNTCODE.Text
                CODENAME_get("ACCOUNTCODE", WW_CODE, WW_NAME, WW_RTN_SW)
                If isNormal(WW_RTN_SW) Then
                    WF_ACCOUNTNAME.Text = WW_NAME
                End If

            Case WF_SEGMENTCODE.ID
                'セグメント
                WW_CODE = WF_SEGMENTCODE.Text
                CODENAME_get("SEGMENTCODE", WW_CODE, WW_NAME, WW_RTN_SW)
                If isNormal(WW_RTN_SW) Then
                    WF_SEGMENTNAME.Text = WW_NAME
                End If

            Case WF_ACCOUNTTYPE.ID
                '科目区分
                WW_CODE = WF_ACCOUNTTYPE.Text
                CODENAME_get("ACCOUNTTYPE", WW_CODE, WW_NAME, WW_RTN_SW)
                If isNormal(WW_RTN_SW) Then
                    WF_ACCOUNTTYPENAME.Text = WW_NAME
                End If

        End Select

        '○ メッセージ表示
        If isNormal(WW_RTN_SW) Then
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.NOR)
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

                Case WF_ENDYMD.ID
                    '適用終了年月日
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_ENDYMD.Text = ""
                        Else
                            WF_ENDYMD.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_ENDYMD.Focus()

                Case WF_ACCOUNTCODE.ID
                    '科目コード
                    WF_ACCOUNTCODE.Text = WW_SelectValue
                    WF_ACCOUNTNAME.Text = WW_SelectText
                    WF_ACCOUNTCODE.Focus()

                Case WF_SEGMENTCODE.ID
                    'セグメント
                    WF_SEGMENTCODE.Text = WW_SelectValue
                    WF_SEGMENTNAME.Text = WW_SelectText
                    WF_SEGMENTCODE.Focus()

                Case WF_ACCOUNTTYPE.ID
                    '科目区分
                    WF_ACCOUNTTYPE.Text = WW_SelectValue
                    WF_ACCOUNTTYPENAME.Text = WW_SelectText
                    WF_ACCOUNTTYPE.Focus()

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

                Case WF_ACCOUNTCODE.ID
                    '科目コード
                    WF_ACCOUNTCODE.Focus()

                Case WF_SEGMENTCODE.ID
                    'セグメント
                    WF_SEGMENTCODE.Focus()

                Case WF_ACCOUNTTYPE.ID
                    '科目区分
                    WF_ACCOUNTTYPE.Focus()

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
        For Each OIM0019INProw As DataRow In OIM0019INPtbl.Rows

            WW_LINE_ERR = ""

            '削除フラグ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "DELFLG", OIM0019INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("DELFLG", OIM0019INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0019INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0019INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '適用開始年月日(バリデーションチェック）
            WW_TEXT = OIM0019INProw("FROMYMD")
            Master.CheckField(Master.USERCAMP, "FROMYMD", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '年月日チェック
                WW_CheckDate(WW_TEXT, "適用開始年月日", WW_CS0024FCHECKERR, dateErrFlag)
                If dateErrFlag = "1" Then
                    WW_CheckMES1 = "・更新できないレコード(適用開始年月日エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKERR
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0019INProw)
                    WW_LINE_ERR = "ERR"
                    WW_PKEY_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                Else
                    OIM0019INProw("FROMYMD") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(適用開始年月日エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0019INProw)
                WW_LINE_ERR = "ERR"
                WW_PKEY_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '適用終了年月日(バリデーションチェック）
            WW_TEXT = OIM0019INProw("ENDYMD")
            Master.CheckField(Master.USERCAMP, "ENDYMD", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '年月日チェック
                WW_CheckDate(WW_TEXT, "適用終了年月日", WW_CS0024FCHECKERR, dateErrFlag)
                If dateErrFlag = "1" Then
                    WW_CheckMES1 = "・更新できないレコード(適用終了年月日エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKERR
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0019INProw)
                    WW_LINE_ERR = "ERR"
                    WW_PKEY_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                Else
                    OIM0019INProw("ENDYMD") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(適用終了年月日エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0019INProw)
                WW_LINE_ERR = "ERR"
                WW_PKEY_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '科目コード(バリデーションチェック)
            WW_TEXT = OIM0019INProw("ACCOUNTCODE")
            Master.CheckField(Master.USERCAMP, "ACCOUNTCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(科目コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0019INProw)
                WW_LINE_ERR = "ERR"
                WW_PKEY_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '科目名(バリデーションチェック)
            WW_TEXT = OIM0019INProw("ACCOUNTNAME")
            Master.CheckField(Master.USERCAMP, "ACCOUNTNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(科目名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0019INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'セグメント(バリデーションチェック)
            WW_TEXT = OIM0019INProw("SEGMENTCODE")
            Master.CheckField(Master.USERCAMP, "SEGMENTCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(セグメント入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0019INProw)
                WW_LINE_ERR = "ERR"
                WW_PKEY_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'セグメント名(バリデーションチェック)
            WW_TEXT = OIM0019INProw("SEGMENTNAME")
            Master.CheckField(Master.USERCAMP, "SEGMENTNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(セグメント名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0019INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'セグメント枝番(バリデーションチェック)
            WW_TEXT = OIM0019INProw("SEGMENTBRANCHCODE")
            Master.CheckField(Master.USERCAMP, "SEGMENTBRANCHCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(セグメント枝番入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0019INProw)
                WW_LINE_ERR = "ERR"
                WW_PKEY_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'セグメント枝番名(バリデーションチェック)
            WW_TEXT = OIM0019INProw("SEGMENTBRANCHNAME")
            Master.CheckField(Master.USERCAMP, "SEGMENTBRANCHNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(セグメント枝番名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0019INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '科目区分(バリデーションチェック)
            WW_TEXT = OIM0019INProw("ACCOUNTTYPE")
            Master.CheckField(Master.USERCAMP, "ACCOUNTTYPE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(科目区分入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0019INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '科目区分名(バリデーションチェック)
            WW_TEXT = OIM0019INProw("ACCOUNTTYPENAME")
            Master.CheckField(Master.USERCAMP, "ACCOUNTTYPENAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(科目区分名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0019INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '一意制約チェック
            'プライマリキーの項目エラーがある場合、又は同一レコードの更新の場合、チェック対象外
            If WW_PKEY_ERR.Equals("ERR") OrElse
                (OIM0019INProw("FROMYMD") = work.WF_SEL_FROMYMD2.Text AndAlso
                OIM0019INProw("ENDYMD") = work.WF_SEL_ENDYMD2.Text AndAlso
                OIM0019INProw("ACCOUNTCODE") = work.WF_SEL_ACCOUNTCODE2.Text AndAlso
                OIM0019INProw("SEGMENTCODE") = work.WF_SEL_SEGMENTCODE2.Text AndAlso
                OIM0019INProw("SEGMENTBRANCHCODE") = work.WF_SEL_SEGMENTBRANCHCODE2.Text) Then
            Else
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    'DataBase接続
                    SQLcon.Open()

                    '一意制約チェック
                    UniqueKeyCheck(SQLcon, WW_UniqueKeyCHECK)
                End Using

                If Not isNormal(WW_UniqueKeyCHECK) Then
                    WW_CheckMES1 = "一意制約違反（適用開始年月日, 適用終了年月日, 科目コード, セグメント, セグメント枝番）。"
                    WW_CheckMES2 = C_MESSAGE_NO.OVERLAP_DATA_ERROR &
                                       "([" & OIM0019INProw("FROMYMD") &
                                       ", " & OIM0019INProw("ENDYMD") &
                                       ", " & OIM0019INProw("ACCOUNTCODE") &
                                       ", " & OIM0019INProw("SEGMENTCODE") &
                                       ", " & OIM0019INProw("SEGMENTBRANCHCODE") & "]"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0019INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR
                End If
            End If

            If WW_LINE_ERR = "" Then
                If OIM0019INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    OIM0019INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LINE_ERR = CONST_PATTERNERR Then
                    '関連チェックエラーをセット
                    OIM0019INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    '単項目チェックエラーをセット
                    OIM0019INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
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
    ''' <param name="OIM0019row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIM0019row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIM0019row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 適用開始年月日 =" & OIM0019row("FROMYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 適用終了年月日 =" & OIM0019row("ENDYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 科目コード =" & OIM0019row("ACCOUNTCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 科目名 =" & OIM0019row("ACCOUNTNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> セグメント =" & OIM0019row("SEGMENTCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> セグメント名 =" & OIM0019row("SEGMENTNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> セグメント枝番 =" & OIM0019row("SEGMENTBRANCHCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> セグメント枝番名 =" & OIM0019row("SEGMENTBRANCHNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 科目区分 =" & OIM0019row("ACCOUNTTYPE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 科目区分名 =" & OIM0019row("ACCOUNTTYPENAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除フラグ =" & OIM0019row("DELFLG")
        End If

        rightview.AddErrorReport(WW_ERR_MES)

    End Sub


    ''' <summary>
    ''' OIM0019tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub OIM0019tbl_UPD()

        '○ 画面状態設定
        For Each OIM0019row As DataRow In OIM0019tbl.Rows
            Select Case OIM0019row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0019row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0019row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0019row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0019row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0019row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each OIM0019INProw As DataRow In OIM0019INPtbl.Rows

            'エラーレコード読み飛ばし
            If OIM0019INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            OIM0019INProw.Item("OPERATION") = CONST_INSERT

            'KEY項目が等しい時
            For Each OIM0019row As DataRow In OIM0019tbl.Rows

                If OIM0019row("FROMYMD") = OIM0019INProw("FROMYMD") AndAlso
                    OIM0019row("ENDYMD") = OIM0019INProw("ENDYMD") AndAlso
                    OIM0019row("ACCOUNTCODE") = OIM0019INProw("ACCOUNTCODE") AndAlso
                    OIM0019row("SEGMENTCODE") = OIM0019INProw("SEGMENTCODE") AndAlso
                    OIM0019row("SEGMENTBRANCHCODE") = OIM0019INProw("SEGMENTBRANCHCODE") Then

                    'KEY項目以外の項目に変更がないときは「操作」の項目は空白にする
                    If OIM0019row("ACCOUNTNAME") = OIM0019INProw("ACCOUNTNAME") AndAlso
                        OIM0019row("SEGMENTNAME") = OIM0019INProw("SEGMENTNAME") AndAlso
                        OIM0019row("SEGMENTBRANCHNAME") = OIM0019INProw("SEGMENTBRANCHNAME") AndAlso
                        OIM0019row("ACCOUNTTYPE") = OIM0019INProw("ACCOUNTTYPE") AndAlso
                        OIM0019row("ACCOUNTTYPENAME") = OIM0019INProw("ACCOUNTTYPENAME") AndAlso
                        OIM0019row("DELFLG") = OIM0019INProw("DELFLG") AndAlso
                        OIM0019INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA Then
                    Else
                        'KEY項目以外の項目に変更がある時は「操作」の項目を「更新」に設定する
                        OIM0019INProw("OPERATION") = CONST_UPDATE
                        Exit For
                    End If

                    Exit For

                End If
            Next
        Next

        '○ 変更有無判定　&　入力値反映
        For Each OIM0019INProw As DataRow In OIM0019INPtbl.Rows
            Select Case OIM0019INProw("OPERATION")
                Case CONST_UPDATE
                    TBL_UPDATE_SUB(OIM0019INProw)
                Case CONST_INSERT
                    TBL_INSERT_SUB(OIM0019INProw)
                Case CONST_PATTERNERR
                    '関連チェックエラーの場合、キーが変わるため、行追加してエラーレコードを表示させる
                    TBL_INSERT_SUB(OIM0019INProw)
                Case C_LIST_OPERATION_CODE.ERRORED
                    TBL_ERR_SUB(OIM0019INProw)
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="OIM0019INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef OIM0019INProw As DataRow)

        For Each OIM0019row As DataRow In OIM0019tbl.Rows

            '同一レコードか判定
            If OIM0019row("FROMYMD") = OIM0019INProw("FROMYMD") AndAlso
                OIM0019row("ENDYMD") = OIM0019INProw("ENDYMD") AndAlso
                OIM0019row("ACCOUNTCODE") = OIM0019INProw("ACCOUNTCODE") AndAlso
                OIM0019row("SEGMENTCODE") = OIM0019INProw("SEGMENTCODE") AndAlso
                OIM0019row("SEGMENTBRANCHCODE") = OIM0019INProw("SEGMENTBRANCHCODE") Then

                '画面入力テーブル項目設定
                OIM0019INProw("LINECNT") = OIM0019row("LINECNT")
                OIM0019INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                OIM0019INProw("UPDTIMSTP") = OIM0019row("UPDTIMSTP")
                OIM0019INProw("SELECT") = 1
                OIM0019INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0019row.ItemArray = OIM0019INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 追加予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0019INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef OIM0019INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim OIM0019row As DataRow = OIM0019tbl.NewRow
        OIM0019row.ItemArray = OIM0019INProw.ItemArray

        OIM0019row("LINECNT") = OIM0019tbl.Rows.Count + 1
        If OIM0019INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
            OIM0019row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        Else
            OIM0019row("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
        End If

        OIM0019row("UPDTIMSTP") = "0"
        OIM0019row("SELECT") = 1
        OIM0019row("HIDDEN") = 0

        OIM0019tbl.Rows.Add(OIM0019row)

    End Sub


    ''' <summary>
    ''' エラーデータの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0019INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_ERR_SUB(ByRef OIM0019INProw As DataRow)

        For Each OIM0019row As DataRow In OIM0019tbl.Rows

            '同一レコードか判定
            If OIM0019row("FROMYMD") = OIM0019INProw("FROMYMD") AndAlso
                OIM0019row("ENDYMD") = OIM0019INProw("ENDYMD") AndAlso
                OIM0019row("ACCOUNTCODE") = OIM0019INProw("ACCOUNTCODE") AndAlso
                OIM0019row("SEGMENTCODE") = OIM0019INProw("SEGMENTCODE") AndAlso
                OIM0019row("SEGMENTBRANCHCODE") = OIM0019INProw("SEGMENTBRANCHCODE") Then
                '画面入力テーブル項目設定
                OIM0019INProw("LINECNT") = OIM0019row("LINECNT")
                OIM0019INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                OIM0019INProw("UPDTIMSTP") = OIM0019row("UPDTIMSTP")
                OIM0019INProw("SELECT") = 1
                OIM0019INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0019row.ItemArray = OIM0019INProw.ItemArray
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
                Case "ACCOUNTCODE"
                    '科目コード
                    prmData = work.CreateFIXParam(Master.USERCAMP, "ACCOUNTCODE")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "SEGMENTCODE"
                    'セグメント
                    prmData = work.CreateFIXParam(Master.USERCAMP, "ACCOUNTSEGMENTCODE")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "ACCOUNTTYPE"
                    '科目区分
                    prmData = work.CreateFIXParam(Master.USERCAMP, "ACCOUNTTYPE")
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
