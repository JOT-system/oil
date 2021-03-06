﻿Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' タンク車マスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class OIM0014LoadcalcCreate
    Inherits Page

    '○ 検索結果格納Table
    Private OIM0014tbl As DataTable                                  '一覧格納用テーブル
    Private OIM0014INPtbl As DataTable                               'チェック用テーブル
    Private OIM0014UPDtbl As DataTable                               '更新用テーブル

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
                    Master.RecoverTable(OIM0014tbl, work.WF_SEL_INPTBL.Text)

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
            If Not IsNothing(OIM0014tbl) Then
                OIM0014tbl.Clear()
                OIM0014tbl.Dispose()
                OIM0014tbl = Nothing
            End If

            If Not IsNothing(OIM0014INPtbl) Then
                OIM0014INPtbl.Clear()
                OIM0014INPtbl.Dispose()
                OIM0014INPtbl = Nothing
            End If

            If Not IsNothing(OIM0014UPDtbl) Then
                OIM0014UPDtbl.Clear()
                OIM0014UPDtbl.Dispose()
                OIM0014UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIM0014WRKINC.MAPIDC
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0014L Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        End If

        '○ 名称設定処理
        '選択行
        WF_Sel_LINECNT.Text = work.WF_SEL_LINECNT.Text

        '基地コード
        WF_PLANTCODE.Text = work.WF_SEL_PLANTCODE2.Text
        CODENAME_get("PLANTCODE", WF_PLANTCODE.Text, WF_PLANTCODE_TEXT.Text, WW_RTN_SW)

        '油種大分類コード
        WF_BIGOILCODE.Text = work.WF_SEL_BIGOILCODE2.Text
        CODENAME_get("BIGOILCODE", WF_BIGOILCODE.Text, WF_BIGOILCODE_TEXT.Text, WW_RTN_SW)

        '積込チェック用油種コード
        WF_CHECKOILCODE.Text = work.WF_SEL_CHECKOILCODE2.Text
        CODENAME_get("CHECKOILCODE", WF_CHECKOILCODE.Text, WF_CHECKOILCODE_TEXT.Text, WW_RTN_SW)

        '車数
        WF_TANKCOUNT.Text = work.WF_SEL_TANKCOUNT2.Text

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
            & "    OIL.OIM0014_LOADCALC" _
            & " WHERE" _
            & "     PLANTCODE   = @P1" _
            & " AND DELFLG      <> @P2"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 4)     '基地コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 1)     '削除フラグ
                PARA1.Value = WF_PLANTCODE.Text
                PARA2.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    Dim OIM0014Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIM0014Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIM0014Chk.Load(SQLdr)

                    If OIM0014Chk.Rows.Count > 0 Then
                        '重複データエラー
                        O_MESSAGENO = Messages.C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR
                    Else
                        '正常終了時
                        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0014C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0014C UPDATE_INSERT"
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
        DetailBoxToOIM0014INPtbl(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ERR_SW) Then
            OIM0014tbl_UPD()
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIM0014tbl, work.WF_SEL_INPTBL.Text)

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
    Protected Sub DetailBoxToOIM0014INPtbl(ByRef O_RTN As String)

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

        Master.CreateEmptyTable(OIM0014INPtbl, work.WF_SEL_INPTBL.Text)
        Dim OIM0014INProw As DataRow = OIM0014INPtbl.NewRow

        '○ 初期クリア
        For Each OIM0014INPcol As DataColumn In OIM0014INPtbl.Columns
            If IsDBNull(OIM0014INProw.Item(OIM0014INPcol)) OrElse IsNothing(OIM0014INProw.Item(OIM0014INPcol)) Then
                Select Case OIM0014INPcol.ColumnName
                    Case "LINECNT"
                        OIM0014INProw.Item(OIM0014INPcol) = 0
                    Case "OPERATION"
                        OIM0014INProw.Item(OIM0014INPcol) = C_LIST_OPERATION_CODE.NODATA
                    Case "UPDTIMSTP"
                        OIM0014INProw.Item(OIM0014INPcol) = 0
                    Case "SELECT"
                        OIM0014INProw.Item(OIM0014INPcol) = 1
                    Case "HIDDEN"
                        OIM0014INProw.Item(OIM0014INPcol) = 0
                    Case Else
                        OIM0014INProw.Item(OIM0014INPcol) = ""
                End Select
            End If
        Next

        'LINECNT
        If WF_Sel_LINECNT.Text = "" Then
            OIM0014INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(WF_Sel_LINECNT.Text, OIM0014INProw("LINECNT"))
            Catch ex As Exception
                OIM0014INProw("LINECNT") = 0
            End Try
        End If

        OIM0014INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        OIM0014INProw("UPDTIMSTP") = 0
        OIM0014INProw("SELECT") = 1
        OIM0014INProw("HIDDEN") = 0

        OIM0014INProw("PLANTCODE") = WF_PLANTCODE.Text              '基地コード
        OIM0014INProw("BIGOILCODE") = WF_BIGOILCODE.Text            '油種大分類コード
        OIM0014INProw("CHECKOILCODE") = WF_CHECKOILCODE.Text        '積込チェック用油種コード
        OIM0014INProw("TANKCOUNT") = WF_TANKCOUNT.Text              '車数
        OIM0014INProw("DELFLG") = WF_DELFLG.Text                    '削除フラグ

        '○ チェック用テーブルに登録する
        OIM0014INPtbl.Rows.Add(OIM0014INProw)

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
        For Each OIM0014row As DataRow In OIM0014tbl.Rows
            Select Case OIM0014row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0014row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0014row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0014row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0014row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0014row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIM0014tbl, work.WF_SEL_INPTBL.Text)

        WF_Sel_LINECNT.Text = ""        'LINECNT

        WF_PLANTCODE.Text = ""          '基地コード
        WF_BIGOILCODE.Text = ""         '油種大分類コード
        WF_CHECKOILCODE.Text = ""       '積込チェック用油種コード
        WF_TANKCOUNT.Text = ""          '車数
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

                    Case WF_BIGOILCODE.ID
                        '油種大分類コード
                        prmData = work.CreateFIXParam(Master.USERCAMP, "BIGOILCODE")

                    Case WF_CHECKOILCODE.ID
                        '積込チェック用油種コード
                        prmData = work.CreateFIXParam(Master.USERCAMP, "CHECKOILCODE")

                    Case WF_DELFLG.ID
                        '削除フラグ
                        prmData = work.CreateFIXParam(Master.USERCAMP, "DELFLG")

                End Select

                .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                .ActiveListBox()
            End With
        End If

    End Sub

    Private Sub CreatePLANTCODE(ByVal I_PLANTCODE As String, ByVal I_CONSIGNEENAME As String, ByRef O_TEXT As String, ByRef O_RTN As String, Optional toNarrow As Boolean = False)

        O_TEXT = ""
        O_RTN = ""

        If String.IsNullOrWhiteSpace(I_PLANTCODE) OrElse String.IsNullOrWhiteSpace(I_CONSIGNEENAME) Then
            O_RTN = C_MESSAGE_NO.NORMAL
            Exit Sub
        End If

        Try
            If toNarrow Then
                O_TEXT = String.Format("{0}-{1}", StrConv(I_CONSIGNEENAME, VbStrConv.Narrow), I_PLANTCODE)
            Else

            End If
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.CAST_FORMAT_ERROR_EX
            Exit Sub
        End Try

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
            Case WF_BIGOILCODE.ID
                '油種大分類コード
                CODENAME_get("BIGOILCODE", WF_BIGOILCODE.Text, WF_BIGOILCODE_TEXT.Text, WW_RTN_SW)
            Case WF_CHECKOILCODE.ID
                '積込チェック用油種コード
                CODENAME_get("CHECKOILCODE", WF_CHECKOILCODE.Text, WF_CHECKOILCODE_TEXT.Text, WW_RTN_SW)
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

                Case WF_BIGOILCODE.ID
                    '油種大分類コード
                    WF_BIGOILCODE.Text = WW_SelectValue
                    WF_BIGOILCODE_TEXT.Text = WW_SelectText
                    WF_BIGOILCODE.Focus()

                Case WF_CHECKOILCODE.ID
                    '積込チェック用油種コード
                    WF_CHECKOILCODE.Text = WW_SelectValue
                    WF_CHECKOILCODE_TEXT.Text = WW_SelectText
                    WF_CHECKOILCODE.Focus()

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

                Case WF_BIGOILCODE.ID
                    '油種大分類コード
                    WF_BIGOILCODE.Focus()

                Case WF_CHECKOILCODE.ID
                    '積込チェック用油種コード
                    WF_CHECKOILCODE.Focus()

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
        For Each OIM0014INProw As DataRow In OIM0014INPtbl.Rows

            WW_LINE_ERR = ""

            '削除フラグ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "DELFLG", OIM0014INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("DELFLG", OIM0014INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0014INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0014INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '基地コード(バリデーションチェック)
            WW_TEXT = OIM0014INProw("PLANTCODE")
            Master.CheckField(Master.USERCAMP, "PLANTCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("PLANTCODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(基地コード入力エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0014INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(基地コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0014INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種大分類コード(バリデーションチェック)
            WW_TEXT = OIM0014INProw("BIGOILCODE")
            Master.CheckField(Master.USERCAMP, "BIGOILCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("BIGOILCODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(油種大分類コード入力エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0014INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(油種大分類コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0014INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '積込チェック用油種コード(バリデーションチェック)
            WW_TEXT = OIM0014INProw("CHECKOILCODE")
            Master.CheckField(Master.USERCAMP, "CHECKOILCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(積込チェック用油種コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0014INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '車数(バリデーションチェック)
            WW_TEXT = OIM0014INProw("TANKCOUNT")
            Master.CheckField(Master.USERCAMP, "TANKCOUNT", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(車数入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0014INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '一意制約チェック
            '同一レコードの更新の場合、チェック対象外
            If OIM0014INProw("PLANTCODE") = work.WF_SEL_PLANTCODE2.Text AndAlso
                OIM0014INProw("BIGOILCODE") = work.WF_SEL_BIGOILCODE2.Text AndAlso
                OIM0014INProw("CHECKOILCODE") = work.WF_SEL_CHECKOILCODE2.Text Then

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
                                       "([" & OIM0014INProw("PLANTCODE") & "]"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0014INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR
                End If
            End If


            If WW_LINE_ERR = "" Then
                If OIM0014INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    OIM0014INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LINE_ERR = CONST_PATTERNERR Then
                    '関連チェックエラーをセット
                    OIM0014INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    '単項目チェックエラーをセット
                    OIM0014INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                End If
            End If
        Next

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="OIM0014row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIM0014row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIM0014row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 基地コード =" & OIM0014row("PLANTCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種大分類コード =" & OIM0014row("BIGOILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 積込チェック用油種コード =" & OIM0014row("CHECKOILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 車数 =" & OIM0014row("TANKCOUNT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除フラグ =" & OIM0014row("DELFLG")
        End If

        rightview.AddErrorReport(WW_ERR_MES)

    End Sub


    ''' <summary>
    ''' OIM0014tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub OIM0014tbl_UPD()

        '○ 画面状態設定
        For Each OIM0014row As DataRow In OIM0014tbl.Rows
            Select Case OIM0014row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0014row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0014row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0014row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0014row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0014row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each OIM0014INProw As DataRow In OIM0014INPtbl.Rows

            'エラーレコード読み飛ばし
            If OIM0014INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            OIM0014INProw.Item("OPERATION") = CONST_INSERT

            ' 既存レコードとの比較
            For Each OIM0014row As DataRow In OIM0014tbl.Rows
                ' KEY項目が等しい時
                If OIM0014row("PLANTCODE") = OIM0014INProw("PLANTCODE") AndAlso
                    OIM0014row("BIGOILCODE") = OIM0014INProw("BIGOILCODE") AndAlso
                    OIM0014row("CHECKOILCODE") = OIM0014INProw("CHECKOILCODE") Then
                    ' KEY項目以外の項目の差異をチェック
                    If OIM0014row("TANKCOUNT") = OIM0014INProw("TANKCOUNT") AndAlso
                        OIM0014row("DELFLG") = OIM0014INProw("DELFLG") Then
                        ' 変更がないときは「操作」の項目は空白にする
                        OIM0014INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Else
                        ' 変更がある時は「操作」の項目を「更新」に設定する
                        OIM0014INProw("OPERATION") = CONST_UPDATE
                    End If

                    Exit For

                End If
            Next
        Next

        '○ 変更有無判定　&　入力値反映
        For Each OIM0014INProw As DataRow In OIM0014INPtbl.Rows
            Select Case OIM0014INProw("OPERATION")
                Case CONST_UPDATE
                    TBL_UPDATE_SUB(OIM0014INProw)
                Case CONST_INSERT
                    TBL_INSERT_SUB(OIM0014INProw)
                Case CONST_PATTERNERR
                    '関連チェックエラーの場合、キーが変わるため、行追加してエラーレコードを表示させる
                    TBL_INSERT_SUB(OIM0014INProw)
                Case C_LIST_OPERATION_CODE.ERRORED
                    TBL_ERR_SUB(OIM0014INProw)
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="OIM0014INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef OIM0014INProw As DataRow)

        For Each OIM0014row As DataRow In OIM0014tbl.Rows

            '同一レコードか判定
            If OIM0014INProw("PLANTCODE") = OIM0014row("PLANTCODE") AndAlso
                OIM0014INProw("BIGOILCODE") = OIM0014row("BIGOILCODE") AndAlso
                OIM0014INProw("CHECKOILCODE") = OIM0014row("CHECKOILCODE") Then
                '画面入力テーブル項目設定
                OIM0014INProw("LINECNT") = OIM0014row("LINECNT")
                OIM0014INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                OIM0014INProw("UPDTIMSTP") = OIM0014row("UPDTIMSTP")
                OIM0014INProw("SELECT") = 1
                OIM0014INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0014row.ItemArray = OIM0014INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 追加予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0014INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef OIM0014INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim OIM0014row As DataRow = OIM0014tbl.NewRow
        OIM0014row.ItemArray = OIM0014INProw.ItemArray

        OIM0014row("LINECNT") = OIM0014tbl.Rows.Count + 1
        If OIM0014INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
            OIM0014row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        Else
            OIM0014row("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
        End If

        OIM0014row("UPDTIMSTP") = "0"
        OIM0014row("SELECT") = 1
        OIM0014row("HIDDEN") = 0

        OIM0014tbl.Rows.Add(OIM0014row)

    End Sub


    ''' <summary>
    ''' エラーデータの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0014INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_ERR_SUB(ByRef OIM0014INProw As DataRow)

        For Each OIM0014row As DataRow In OIM0014tbl.Rows

            '同一レコードか判定
            If OIM0014INProw("PLANTCODE") = OIM0014row("PLANTCODE") AndAlso
                OIM0014INProw("BIGOILCODE") = OIM0014row("BIGOILCODE") AndAlso
                OIM0014INProw("CHECKOILCODE") = OIM0014row("CHECKOILCODE") Then
                '画面入力テーブル項目設定
                OIM0014INProw("LINECNT") = OIM0014row("LINECNT")
                OIM0014INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                OIM0014INProw("UPDTIMSTP") = OIM0014row("UPDTIMSTP")
                OIM0014INProw("SELECT") = 1
                OIM0014INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0014row.ItemArray = OIM0014INProw.ItemArray
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
                Case "BIGOILCODE"
                    '油種大分類コード
                    prmData = work.CreateFIXParam(Master.USERCAMP, "BIGOILCODE")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "CHECKOILCODE"
                    '積込チェック用油種コード
                    prmData = work.CreateFIXParam(Master.USERCAMP, "CHECKOILCODE")
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
