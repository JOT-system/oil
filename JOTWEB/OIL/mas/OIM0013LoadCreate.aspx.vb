﻿Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' タンク車マスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class OIM0013LoadCreate
    Inherits Page

    '○ 検索結果格納Table
    Private OIM0013tbl As DataTable                                  '一覧格納用テーブル
    Private OIM0013INPtbl As DataTable                               'チェック用テーブル
    Private OIM0013UPDtbl As DataTable                               '更新用テーブル

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
                    Master.RecoverTable(OIM0013tbl, work.WF_SEL_INPTBL.Text)

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
            If Not IsNothing(OIM0013tbl) Then
                OIM0013tbl.Clear()
                OIM0013tbl.Dispose()
                OIM0013tbl = Nothing
            End If

            If Not IsNothing(OIM0013INPtbl) Then
                OIM0013INPtbl.Clear()
                OIM0013INPtbl.Dispose()
                OIM0013INPtbl = Nothing
            End If

            If Not IsNothing(OIM0013UPDtbl) Then
                OIM0013UPDtbl.Clear()
                OIM0013UPDtbl.Dispose()
                OIM0013UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIM0013WRKINC.MAPIDC
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0013L Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        End If

        '入力制限(数値0～9)
        WF_MAXLINECNT.Attributes("onkeyPress") = "CheckNum()"
        WF_LOADINGPOINT.Attributes("onkeyPress") = "CheckNum()"

        '○ 名称設定処理
        '選択行
        WF_Sel_LINECNT.Text = work.WF_SEL_LINECNT.Text

        '基地コード
        WF_PLANTCODE.Text = work.WF_SEL_PLANTCODE2.Text
        CODENAME_get("PLANTCODE", WF_PLANTCODE.Text, WF_PLANTCODE_TEXT.Text, WW_RTN_SW)

        '最大回線数
        WF_MAXLINECNT.Text = work.WF_SEL_MAXLINECNT.Text

        '積込ポイント
        WF_LOADINGPOINT.Text = work.WF_SEL_LOADINGPOINT2.Text

        '油種コード
        WF_OILCODE.Text = work.WF_SEL_OILCODE2.Text
        CODENAME_get("OILCODE", WF_OILCODE.Text, WF_OILCODE_TEXT.Text, WW_RTN_SW)

        '油種細分コード
        WF_SEGMENTOILCODE.Text = work.WF_SEL_SEGMENTOILCODE2.Text
        CODENAME_get("SEGMENTOILCODE", WF_SEGMENTOILCODE.Text, WF_SEGMENTOILCODE_TEXT.Text, WW_RTN_SW)

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
            & "     PLANTCODE " _
            & " FROM" _
            & "    OIL.OIM0013_LOAD" _
            & " WHERE" _
            & "     PLANTCODE       = @P1" _
            & " AND MAXLINECNT      = @P2" _
            & " AND LOADINGPOINT    = @P3" _
            & " AND OILCODE         = @P4" _
            & " AND SEGMENTOILCODE  = @P5" _
            & " AND DELFLG         <> @P6"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 4)     '基地コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.Int)             '最大回線数
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.Int)             '積込ポイント
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 4)     '油種コード
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 1)     '油種細分コード
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 1)     '削除フラグ
                PARA1.Value = WF_PLANTCODE.Text
                PARA2.Value = WF_MAXLINECNT.Text
                PARA3.Value = WF_LOADINGPOINT.Text
                PARA4.Value = WF_OILCODE.Text
                PARA5.Value = WF_SEGMENTOILCODE.Text
                PARA6.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    Dim OIM0013Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIM0013Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIM0013Chk.Load(SQLdr)

                    If OIM0013Chk.Rows.Count > 0 Then
                        '重複データエラー
                        O_MESSAGENO = Messages.C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR
                    Else
                        '正常終了時
                        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0013C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0013C UPDATE_INSERT"
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
        DetailBoxToOIM0013INPtbl(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ERR_SW) Then
            OIM0013tbl_UPD()
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIM0013tbl, work.WF_SEL_INPTBL.Text)

        '○ メッセージ表示
        If WW_ERR_SW = "" Then
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)
        Else
            If isNormal(WW_ERR_SW) Then
                Master.Output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.INF)

            ElseIf WW_ERR_SW = C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR Then
                Master.Output(WW_ERR_SW, C_MESSAGE_TYPE.ERR, "基地コード", needsPopUp:=True)

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
    Protected Sub DetailBoxToOIM0013INPtbl(ByRef O_RTN As String)

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

        Master.CreateEmptyTable(OIM0013INPtbl, work.WF_SEL_INPTBL.Text)
        Dim OIM0013INProw As DataRow = OIM0013INPtbl.NewRow

        '○ 初期クリア
        For Each OIM0013INPcol As DataColumn In OIM0013INPtbl.Columns
            If IsDBNull(OIM0013INProw.Item(OIM0013INPcol)) OrElse IsNothing(OIM0013INProw.Item(OIM0013INPcol)) Then
                Select Case OIM0013INPcol.ColumnName
                    Case "LINECNT"
                        OIM0013INProw.Item(OIM0013INPcol) = 0
                    Case "OPERATION"
                        OIM0013INProw.Item(OIM0013INPcol) = C_LIST_OPERATION_CODE.NODATA
                    Case "UPDTIMSTP"
                        OIM0013INProw.Item(OIM0013INPcol) = 0
                    Case "SELECT"
                        OIM0013INProw.Item(OIM0013INPcol) = 1
                    Case "HIDDEN"
                        OIM0013INProw.Item(OIM0013INPcol) = 0
                    Case Else
                        OIM0013INProw.Item(OIM0013INPcol) = ""
                End Select
            End If
        Next

        'LINECNT
        If WF_Sel_LINECNT.Text = "" Then
            OIM0013INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(WF_Sel_LINECNT.Text, OIM0013INProw("LINECNT"))
            Catch ex As Exception
                OIM0013INProw("LINECNT") = 0
            End Try
        End If

        OIM0013INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        OIM0013INProw("UPDTIMSTP") = 0
        OIM0013INProw("SELECT") = 1
        OIM0013INProw("HIDDEN") = 0

        OIM0013INProw("PLANTCODE") = WF_PLANTCODE.Text              '基地コード
        OIM0013INProw("MAXLINECNT") = WF_MAXLINECNT.Text            '最大回線数
        OIM0013INProw("LOADINGPOINT") = WF_LOADINGPOINT.Text        '積込ポイント
        OIM0013INProw("OILCODE") = WF_OILCODE.Text                  '油種コード
        OIM0013INProw("SEGMENTOILCODE") = WF_SEGMENTOILCODE.Text    '油種細分コード
        OIM0013INProw("DELFLG") = WF_DELFLG.Text                    '削除フラグ

        '○ チェック用テーブルに登録する
        OIM0013INPtbl.Rows.Add(OIM0013INProw)

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
        For Each OIM0013row As DataRow In OIM0013tbl.Rows
            Select Case OIM0013row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0013row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0013row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0013row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0013row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0013row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIM0013tbl, work.WF_SEL_INPTBL.Text)

        WF_Sel_LINECNT.Text = ""        'LINECNT

        WF_PLANTCODE.Text = ""          '基地コード
        WF_MAXLINECNT.Text = ""         '最大回線数
        WF_LOADINGPOINT.Text = ""       '積込ポイント
        WF_OILCODE.Text = ""            '油種コード
        WF_SEGMENTOILCODE.Text = ""     '油種細分コード
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
                Dim prmData As New Hashtable

                'フィールドによってパラメータを変える
                Select Case WF_FIELD.Value

                    Case WF_PLANTCODE.ID
                        '基地コード
                        prmData = work.CreateFIXParam(Master.USERCAMP, "PLANTMASTER")

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
            Case WF_PLANTCODE.ID
                '基地コード
                CODENAME_get("PLANTCODE", WF_PLANTCODE.Text, WF_PLANTCODE_TEXT.Text, WW_RTN_SW)
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

                Case WF_PLANTCODE.ID
                    '基地コード
                    WF_PLANTCODE.Text = WW_SelectValue
                    WF_PLANTCODE_TEXT.Text = WW_SelectText
                    WF_PLANTCODE.Focus()

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

                Case WF_PLANTCODE.ID
                    '基地コード
                    WF_PLANTCODE.Focus()

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
        For Each OIM0013INProw As DataRow In OIM0013INPtbl.Rows

            WW_LINE_ERR = ""

            '削除フラグ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "DELFLG", OIM0013INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("DELFLG", OIM0013INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0013INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0013INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '基地コード(バリデーションチェック)
            WW_TEXT = OIM0013INProw("PLANTCODE")
            Master.CheckField(Master.USERCAMP, "PLANTCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("PLANTCODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(基地コード入力エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0013INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(基地コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0013INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '最大回線数(バリデーションチェック)
            WW_TEXT = OIM0013INProw("MAXLINECNT")
            Master.CheckField(Master.USERCAMP, "MAXLINECNT", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(最大回線数入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0013INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '積込ポイント(バリデーションチェック)
            WW_TEXT = OIM0013INProw("LOADINGPOINT")
            Master.CheckField(Master.USERCAMP, "LOADINGPOINT", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(積込ポイント入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0013INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種コード(バリデーションチェック)
            WW_TEXT = OIM0013INProw("OILCODE")
            Master.CheckField(Master.USERCAMP, "OILCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("OILCODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(油種コード入力エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0013INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(油種コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0013INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種細分コード(バリデーションチェック)
            WW_TEXT = OIM0013INProw("SEGMENTOILCODE")
            Master.CheckField(Master.USERCAMP, "SEGMENTOILCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("SEGMENTOILCODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(油種細分コード入力エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0013INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(油種細分コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0013INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '一意制約チェック
            '同一レコードの更新の場合、チェック対象外
            If OIM0013INProw("PLANTCODE") = work.WF_SEL_PLANTCODE2.Text AndAlso
                OIM0013INProw("MAXLINECNT") = work.WF_SEL_MAXLINECNT.Text AndAlso
                OIM0013INProw("LOADINGPOINT") = work.WF_SEL_LOADINGPOINT2.Text AndAlso
                OIM0013INProw("OILCODE") = work.WF_SEL_OILCODE2.Text AndAlso
                OIM0013INProw("SEGMENTOILCODE") = work.WF_SEL_SEGMENTOILCODE2.Text Then

            Else
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    'DataBase接続
                    SQLcon.Open()

                    '一意制約チェック
                    UniqueKeyCheck(SQLcon, WW_UniqueKeyCHECK)
                End Using

                If Not isNormal(WW_UniqueKeyCHECK) Then
                    WW_CheckMES1 = "一意制約違反（基地コード）。"
                    WW_CheckMES2 = C_MESSAGE_NO.OVERLAP_DATA_ERROR &
                                       "([" & OIM0013INProw("PLANTCODE") & "]"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0013INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR
                End If
            End If


            If WW_LINE_ERR = "" Then
                If OIM0013INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    OIM0013INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LINE_ERR = CONST_PATTERNERR Then
                    '関連チェックエラーをセット
                    OIM0013INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    '単項目チェックエラーをセット
                    OIM0013INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                End If
            End If
        Next

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="OIM0013row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIM0013row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIM0013row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 基地コード =" & OIM0013row("PLANTCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 最大回線数 =" & OIM0013row("MAXLINECNT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 積込ポイント =" & OIM0013row("LOADINGPOINT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種コード =" & OIM0013row("OILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種細分コード =" & OIM0013row("SEGMENTOILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除フラグ =" & OIM0013row("DELFLG")
        End If

        rightview.AddErrorReport(WW_ERR_MES)

    End Sub


    ''' <summary>
    ''' OIM0013tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub OIM0013tbl_UPD()

        '○ 画面状態設定
        For Each OIM0013row As DataRow In OIM0013tbl.Rows
            Select Case OIM0013row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0013row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0013row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0013row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0013row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0013row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each OIM0013INProw As DataRow In OIM0013INPtbl.Rows

            'エラーレコード読み飛ばし
            If OIM0013INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            OIM0013INProw.Item("OPERATION") = CONST_INSERT

            'KEY項目が等しい時
            For Each OIM0013row As DataRow In OIM0013tbl.Rows
                If OIM0013row("PLANTCODE") = OIM0013INProw("PLANTCODE") AndAlso
                    OIM0013row("MAXLINECNT") = OIM0013row("MAXLINECNT") AndAlso
                    OIM0013row("LOADINGPOINT") = OIM0013row("LOADINGPOINT") AndAlso
                    OIM0013row("OILCODE") = OIM0013row("OILCODE") AndAlso
                    OIM0013row("SEGMENTOILCODE") = OIM0013row("SEGMENTOILCODE") Then
                    'KEY項目以外の項目に変更がないときは「操作」の項目は空白にする
                    If OIM0013row("DELFLG") = OIM0013INProw("DELFLG") AndAlso
                        OIM0013INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA Then
                    Else
                        'KEY項目以外の項目に変更がある時は「操作」の項目を「更新」に設定する
                        OIM0013INProw("OPERATION") = CONST_UPDATE
                        Exit For
                    End If

                    Exit For

                End If
            Next
        Next

        '○ 変更有無判定　&　入力値反映
        For Each OIM0013INProw As DataRow In OIM0013INPtbl.Rows
            Select Case OIM0013INProw("OPERATION")
                Case CONST_UPDATE
                    TBL_UPDATE_SUB(OIM0013INProw)
                Case CONST_INSERT
                    TBL_INSERT_SUB(OIM0013INProw)
                Case CONST_PATTERNERR
                    '関連チェックエラーの場合、キーが変わるため、行追加してエラーレコードを表示させる
                    TBL_INSERT_SUB(OIM0013INProw)
                Case C_LIST_OPERATION_CODE.ERRORED
                    TBL_ERR_SUB(OIM0013INProw)
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="OIM0013INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef OIM0013INProw As DataRow)

        For Each OIM0013row As DataRow In OIM0013tbl.Rows

            '同一レコードか判定
            If OIM0013INProw("PLANTCODE") = OIM0013row("PLANTCODE") AndAlso
                OIM0013INProw("MAXLINECNT") = OIM0013row("MAXLINECNT") AndAlso
                OIM0013INProw("LOADINGPOINT") = OIM0013row("LOADINGPOINT") AndAlso
                OIM0013INProw("OILCODE") = OIM0013row("OILCODE") AndAlso
                OIM0013INProw("SEGMENTOILCODE") = OIM0013row("SEGMENTOILCODE") Then
                '画面入力テーブル項目設定
                OIM0013INProw("LINECNT") = OIM0013row("LINECNT")
                OIM0013INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                OIM0013INProw("UPDTIMSTP") = OIM0013row("UPDTIMSTP")
                OIM0013INProw("SELECT") = 1
                OIM0013INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0013row.ItemArray = OIM0013INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 追加予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0013INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef OIM0013INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim OIM0013row As DataRow = OIM0013tbl.NewRow
        OIM0013row.ItemArray = OIM0013INProw.ItemArray

        OIM0013row("LINECNT") = OIM0013tbl.Rows.Count + 1
        If OIM0013INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
            OIM0013row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        Else
            OIM0013row("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
        End If

        OIM0013row("UPDTIMSTP") = "0"
        OIM0013row("SELECT") = 1
        OIM0013row("HIDDEN") = 0

        OIM0013tbl.Rows.Add(OIM0013row)

    End Sub


    ''' <summary>
    ''' エラーデータの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0013INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_ERR_SUB(ByRef OIM0013INProw As DataRow)

        For Each OIM0013row As DataRow In OIM0013tbl.Rows

            '同一レコードか判定
            If OIM0013INProw("PLANTCODE") = OIM0013row("PLANTCODE") AndAlso
                OIM0013INProw("MAXLINECNT") = OIM0013row("MAXLINECNT") AndAlso
                OIM0013INProw("LOADINGPOINT") = OIM0013row("LOADINGPOINT") AndAlso
                OIM0013INProw("OILCODE") = OIM0013row("OILCODE") AndAlso
                OIM0013INProw("SEGMENTOILCODE") = OIM0013row("SEGMENTOILCODE") Then
                '画面入力テーブル項目設定
                OIM0013INProw("LINECNT") = OIM0013row("LINECNT")
                OIM0013INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                OIM0013INProw("UPDTIMSTP") = OIM0013row("UPDTIMSTP")
                OIM0013INProw("SELECT") = 1
                OIM0013INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0013row.ItemArray = OIM0013INProw.ItemArray
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
                Case "PLANTCODE"
                    '基地コード
                    prmData = work.CreateFIXParam(Master.USERCAMP, "PLANTMASTER")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
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
