Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' タンク車マスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class OIM0009PlantCreate
    Inherits Page

    '○ 検索結果格納Table
    Private OIM0009tbl As DataTable                                  '一覧格納用テーブル
    Private OIM0009INPtbl As DataTable                               'チェック用テーブル
    Private OIM0009UPDtbl As DataTable                               '更新用テーブル

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
                    Master.RecoverTable(OIM0009tbl, work.WF_SEL_INPTBL.Text)

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
            If Not IsNothing(OIM0009tbl) Then
                OIM0009tbl.Clear()
                OIM0009tbl.Dispose()
                OIM0009tbl = Nothing
            End If

            If Not IsNothing(OIM0009INPtbl) Then
                OIM0009INPtbl.Clear()
                OIM0009INPtbl.Dispose()
                OIM0009INPtbl = Nothing
            End If

            If Not IsNothing(OIM0009UPDtbl) Then
                OIM0009UPDtbl.Clear()
                OIM0009UPDtbl.Dispose()
                OIM0009UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIM0009WRKINC.MAPIDC
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0009L Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        End If

        '○ 名称設定処理
        '選択行
        WF_Sel_LINECNT.Text = work.WF_SEL_LINECNT.Text

        '基地コード
        WF_PLANTCODE.Text = work.WF_SEL_PLANTCODE2.Text

        '基地名
        WF_PLANTNAME.Text = work.WF_SEL_PLANTNAME.Text

        '基地名カナ
        WF_PLANTNAMEKANA.Text = work.WF_SEL_PLANTNAMEKANA.Text

        '荷主コード, 荷主名称
        WF_SHIPPERCODE.Text = work.WF_SEL_SHIPPERCODE.Text
        CODENAME_get("SHIPPERCODE", WF_SHIPPERCODE.Text, WF_SHIPPERCODE_TEXT.Text, WW_RTN_SW)

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
            & "    OIL.OIM0009_PLANT" _
            & " WHERE" _
            & "     PLANTCODE   = @P1" _
            & " AND DELFLG      <> @P2"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)  '基地コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 1)   '削除フラグ
                PARA1.Value = WF_PLANTCODE.Text
                PARA2.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    Dim OIM0009Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIM0009Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIM0009Chk.Load(SQLdr)

                    If OIM0009Chk.Rows.Count > 0 Then
                        '重複データエラー
                        O_MESSAGENO = Messages.C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR
                    Else
                        '正常終了時
                        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0009C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0009C UPDATE_INSERT"
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
        DetailBoxToOIM0009INPtbl(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ERR_SW) Then
            OIM0009tbl_UPD()
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIM0009tbl, work.WF_SEL_INPTBL.Text)

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
    Protected Sub DetailBoxToOIM0009INPtbl(ByRef O_RTN As String)

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

        Master.CreateEmptyTable(OIM0009INPtbl, work.WF_SEL_INPTBL.Text)
        Dim OIM0009INProw As DataRow = OIM0009INPtbl.NewRow

        '○ 初期クリア
        For Each OIM0009INPcol As DataColumn In OIM0009INPtbl.Columns
            If IsDBNull(OIM0009INProw.Item(OIM0009INPcol)) OrElse IsNothing(OIM0009INProw.Item(OIM0009INPcol)) Then
                Select Case OIM0009INPcol.ColumnName
                    Case "LINECNT"
                        OIM0009INProw.Item(OIM0009INPcol) = 0
                    Case "OPERATION"
                        OIM0009INProw.Item(OIM0009INPcol) = C_LIST_OPERATION_CODE.NODATA
                    Case "UPDTIMSTP"
                        OIM0009INProw.Item(OIM0009INPcol) = 0
                    Case "SELECT"
                        OIM0009INProw.Item(OIM0009INPcol) = 1
                    Case "HIDDEN"
                        OIM0009INProw.Item(OIM0009INPcol) = 0
                    Case Else
                        OIM0009INProw.Item(OIM0009INPcol) = ""
                End Select
            End If
        Next

        'LINECNT
        If WF_Sel_LINECNT.Text = "" Then
            OIM0009INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(WF_Sel_LINECNT.Text, OIM0009INProw("LINECNT"))
            Catch ex As Exception
                OIM0009INProw("LINECNT") = 0
            End Try
        End If

        OIM0009INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        OIM0009INProw("UPDTIMSTP") = 0
        OIM0009INProw("SELECT") = 1
        OIM0009INProw("HIDDEN") = 0

        OIM0009INProw("PLANTCODE") = WF_PLANTCODE.Text                                      '基地コード
        OIM0009INProw("PLANTNAME") = WF_PLANTNAME.Text                                      '基地名
        OIM0009INProw("PLANTNAMEKANA") = WF_PLANTNAMEKANA.Text                              '基地名カナ
        OIM0009INProw("SHIPPERCODE") = WF_SHIPPERCODE.Text                                  '荷主コード
        OIM0009INProw("DELFLG") = WF_DELFLG.Text                                            '削除フラグ

        '○ チェック用テーブルに登録する
        OIM0009INPtbl.Rows.Add(OIM0009INProw)

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
        For Each OIM0009row As DataRow In OIM0009tbl.Rows
            Select Case OIM0009row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0009row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0009row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0009row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0009row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0009row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIM0009tbl, work.WF_SEL_INPTBL.Text)

        WF_Sel_LINECNT.Text = ""                    'LINECNT

        WF_PLANTCODE.Text = ""                      '基地コード
        WF_PLANTNAME.Text = ""                      '基地名
        WF_PLANTNAMEKANA.Text = ""                  '基地名カナ
        WF_SHIPPERCODE.Text = ""                    '荷主コード
        WF_SHIPPERCODE_TEXT.Text = ""               '荷主名称
        WF_DELFLG.Text = ""                         '削除フラグ

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
                prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

                'フィールドによってパラメータを変える
                Select Case WF_FIELD.Value

                    Case "WF_SHIPPERCODE"       '荷主コード
                        prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "SHIPPERCODE")

                End Select

                .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                .ActiveListBox()
            End With
        End If

    End Sub

    Private Sub CreatePLANTCODE(ByVal I_PLANTCODE As String, ByVal I_PLANTNAME As String, ByRef O_TEXT As String, ByRef O_RTN As String, Optional toNarrow As Boolean = False)

        O_TEXT = ""
        O_RTN = ""

        If String.IsNullOrWhiteSpace(I_PLANTCODE) OrElse String.IsNullOrWhiteSpace(I_PLANTNAME) Then
            O_RTN = C_MESSAGE_NO.NORMAL
            Exit Sub
        End If

        Try
            If toNarrow Then
                O_TEXT = String.Format("{0}-{1}", StrConv(I_PLANTNAME, VbStrConv.Narrow), I_PLANTCODE)
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
            Case "WF_DELFLG"        '削除フラグ
                CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_RTN_SW)
            Case "WF_SHIPPERCODE"   '荷主コード
                CODENAME_get("SHIPPERCODE", WF_SHIPPERCODE.Text, WF_SHIPPERCODE_TEXT.Text, WW_RTN_SW)
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

                Case "WF_DELFLG"        '削除フラグ
                    WF_DELFLG.Text = WW_SelectValue
                    WF_DELFLG_TEXT.Text = WW_SelectText
                    WF_DELFLG.Focus()

                Case "WF_SHIPPERCODE"   '荷主コード
                    WF_SHIPPERCODE.Text = WW_SelectValue
                    WF_SHIPPERCODE_TEXT.Text = WW_SelectText
                    WF_SHIPPERCODE.Focus()

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
                Case "WF_DELFLG"                        '削除フラグ
                    WF_DELFLG.Focus()

                Case "WF_SHIPPERCODE"                   '荷主コード
                    WF_SHIPPERCODE.Focus()

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
        For Each OIM0009INProw As DataRow In OIM0009INPtbl.Rows

            WW_LINE_ERR = ""

            '削除フラグ(バリデーションチェック）
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DELFLG", OIM0009INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("DELFLG", OIM0009INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0009INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0009INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '基地コード(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "PLANTCODE", OIM0009INProw("PLANTCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(基地コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0009INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '基地名(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "PLANTNAME", OIM0009INProw("PLANTNAME"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(基地名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0009INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '基地名カナ(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "PLANTNAMEKANA", OIM0009INProw("PLANTNAMEKANA"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(基地名カナ入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0009INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '荷主コード(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SHIPPERCODE", OIM0009INProw("SHIPPERCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("SHIPPERCODE", OIM0009INProw("SHIPPERCODE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(荷主コード入力エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0009INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
                '「9999999999-キグナス石油」は除外
                If OIM0009INProw("SHIPPERCODE") = "9999999999" Then
                    WW_CheckMES1 = "・更新できないレコード(荷主コード入力エラー)です。"
                    WW_CheckMES2 = "登録できない荷主コードです。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0009INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(荷主コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0009INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '一意制約チェック
            '同一レコードの更新の場合、チェック対象外
            If OIM0009INProw("PLANTCODE") = work.WF_SEL_PLANTCODE2.Text Then

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
                                       "([" & OIM0009INProw("PLANTCODE") & "]"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0009INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR
                End If
            End If


            If WW_LINE_ERR = "" Then
                If OIM0009INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    OIM0009INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LINE_ERR = CONST_PATTERNERR Then
                    '関連チェックエラーをセット
                    OIM0009INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    '単項目チェックエラーをセット
                    OIM0009INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                End If
            End If
        Next

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="OIM0009row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIM0009row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIM0009row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 基地コード =" & OIM0009row("PLANTCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 基地名 =" & OIM0009row("PLANTNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 基地名カナ =" & OIM0009row("PLANTNAMEKANA") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 荷主コード =" & OIM0009row("SHIPPERCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除フラグ =" & OIM0009row("DELFLG")
        End If

        rightview.AddErrorReport(WW_ERR_MES)

    End Sub


    ''' <summary>
    ''' OIM0009tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub OIM0009tbl_UPD()

        '○ 画面状態設定
        For Each OIM0009row As DataRow In OIM0009tbl.Rows
            Select Case OIM0009row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0009row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0009row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0009row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0009row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0009row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each OIM0009INProw As DataRow In OIM0009INPtbl.Rows

            'エラーレコード読み飛ばし
            If OIM0009INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            OIM0009INProw.Item("OPERATION") = CONST_INSERT

            ' 既存レコードとの比較
            For Each OIM0009row As DataRow In OIM0009tbl.Rows
                ' KEY項目が等しい時
                If OIM0009row("PLANTCODE") = OIM0009INProw("PLANTCODE") Then
                    ' KEY項目以外の項目の差異をチェック
                    If OIM0009row("PLANTNAME") = OIM0009INProw("PLANTNAME") AndAlso
                        OIM0009row("PLANTNAMEKANA") = OIM0009INProw("PLANTNAMEKANA") AndAlso
                        OIM0009row("SHIPPERCODE") = OIM0009INProw("SHIPPERCODE") AndAlso
                        OIM0009row("DELFLG") = OIM0009INProw("DELFLG") Then
                        ' 変更がないときは「操作」の項目は空白にする
                        OIM0009INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Else
                        '変更がある時は「操作」の項目を「更新」に設定する
                        OIM0009INProw("OPERATION") = CONST_UPDATE
                    End If

                    Exit For

                End If
            Next
        Next

        '○ 変更有無判定　&　入力値反映
        For Each OIM0009INProw As DataRow In OIM0009INPtbl.Rows
            Select Case OIM0009INProw("OPERATION")
                Case CONST_UPDATE
                    TBL_UPDATE_SUB(OIM0009INProw)
                Case CONST_INSERT
                    TBL_INSERT_SUB(OIM0009INProw)
                Case CONST_PATTERNERR
                    '関連チェックエラーの場合、キーが変わるため、行追加してエラーレコードを表示させる
                    TBL_INSERT_SUB(OIM0009INProw)
                Case C_LIST_OPERATION_CODE.ERRORED
                    TBL_ERR_SUB(OIM0009INProw)
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="OIM0009INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef OIM0009INProw As DataRow)

        For Each OIM0009row As DataRow In OIM0009tbl.Rows

            '同一レコードか判定
            If OIM0009INProw("PLANTCODE") = OIM0009row("PLANTCODE") Then
                '画面入力テーブル項目設定
                OIM0009INProw("LINECNT") = OIM0009row("LINECNT")
                OIM0009INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                OIM0009INProw("UPDTIMSTP") = OIM0009row("UPDTIMSTP")
                OIM0009INProw("SELECT") = 1
                OIM0009INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0009row.ItemArray = OIM0009INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 追加予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0009INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef OIM0009INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim OIM0009row As DataRow = OIM0009tbl.NewRow
        OIM0009row.ItemArray = OIM0009INProw.ItemArray

        OIM0009row("LINECNT") = OIM0009tbl.Rows.Count + 1
        If OIM0009INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
            OIM0009row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        Else
            OIM0009row("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
        End If

        OIM0009row("UPDTIMSTP") = "0"
        OIM0009row("SELECT") = 1
        OIM0009row("HIDDEN") = 0

        OIM0009tbl.Rows.Add(OIM0009row)

    End Sub


    ''' <summary>
    ''' エラーデータの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0009INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_ERR_SUB(ByRef OIM0009INProw As DataRow)

        For Each OIM0009row As DataRow In OIM0009tbl.Rows

            '同一レコードか判定
            If OIM0009INProw("PLANTCODE") = OIM0009row("PLANTCODE") Then
                '画面入力テーブル項目設定
                OIM0009INProw("LINECNT") = OIM0009row("LINECNT")
                OIM0009INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                OIM0009INProw("UPDTIMSTP") = OIM0009row("UPDTIMSTP")
                OIM0009INProw("SELECT") = 1
                OIM0009INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0009row.ItemArray = OIM0009INProw.ItemArray
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
        Dim prmData As New Hashtable

        Try
            prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

            Select Case I_FIELD
                Case "SHIPPERCODE"                  '荷主コード
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, I_VALUE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SHIPPERSLIST, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "DELFLG"                       '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
