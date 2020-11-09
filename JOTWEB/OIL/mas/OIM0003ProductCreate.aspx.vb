Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' 品種マスタ登録（登録）
''' </summary>
''' <remarks></remarks>
Public Class OIM0003ProductCreate
    Inherits Page

    Private CS0051UserInfo As New CS0051UserInfo                    'ユーザ情報取得

    '○ 検索結果格納Table
    Private OIM0003tbl As DataTable                                 '一覧格納用テーブル
    Private OIM0003INPtbl As DataTable                              'チェック用テーブル
    Private OIM0003UPDtbl As DataTable                              '更新用テーブル

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
                    Master.RecoverTable(OIM0003tbl, work.WF_SEL_INPTBL.Text)

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
            If Not IsNothing(OIM0003tbl) Then
                OIM0003tbl.Clear()
                OIM0003tbl.Dispose()
                OIM0003tbl = Nothing
            End If

            If Not IsNothing(OIM0003INPtbl) Then
                OIM0003INPtbl.Clear()
                OIM0003INPtbl.Dispose()
                OIM0003INPtbl = Nothing
            End If

            If Not IsNothing(OIM0003UPDtbl) Then
                OIM0003UPDtbl.Clear()
                OIM0003UPDtbl.Dispose()
                OIM0003UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIM0003WRKINC.MAPIDC
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0003L Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        End If

        '開始年月日・終了年月日を入力するテキストボックスは数値(0～9)＋記号(/)のみ可能とする。
        WF_ORDERFROMDATE.Attributes("onkeyPress") = "CheckCalendar()"
        WF_ORDERTODATE.Attributes("onkeyPress") = "CheckCalendar()"

        '選択行
        WF_Sel_LINECNT.Text = work.WF_SEL_LINECNT.Text

        '営業所コード
        WF_OFFICECODE.Text = work.WF_SEL_OFFICECODE2.Text
        CODENAME_get("OFFICECODE", WF_OFFICECODE.Text, WF_OFFICECODE_TEXT.Text, WW_DUMMY)

        '荷主コード
        WF_SHIPPERCODE.Text = work.WF_SEL_SHIPPERCODE2.Text
        CODENAME_get("SHIPPERCODE", WF_SHIPPERCODE.Text, WF_SHIPPERCODE_TEXT.Text, WW_DUMMY)

        '基地コード
        WF_PLANTCODE.Text = work.WF_SEL_PLANTCODE2.Text
        CODENAME_get("PLANTCODE", WF_PLANTCODE.Text, WF_PLANTCODE_TEXT.Text, WW_DUMMY)

        '油種大分類コード
        WF_BIGOILCODE.Text = work.WF_SEL_BIGOILCODE2.Text
        'CODENAME_get("BIGOILCODE", WF_BIGOILCODE.Text, WF_BIGOILCODE_TEXT.Text, WW_DUMMY)

        '油種大分類名
        WF_BIGOILNAME.Text = work.WF_SEL_BIGOILNAME.Text
        'CODENAME_get("BIGOILNAME", WF_BIGOILNAME.Text, WF_BIGOILNAME_TEXT.Text, WW_DUMMY)

        '油種大分類名カナ
        WF_BIGOILKANA.Text = work.WF_SEL_BIGOILKANA.Text
        'CODENAME_get("BIGOILKANA", WF_BIGOILKANA.Text, WF_BIGOILKANA_TEXT.Text, WW_DUMMY)

        '油種中分類コード
        WF_MIDDLEOILCODE.Text = work.WF_SEL_MIDDLEOILCODE2.Text
        'CODENAME_get("MIDDLEOILCODE", WF_MIDDLEOILCODE.Text, WF_MIDDLEOILCODE_TEXT.Text, WW_DUMMY)

        '油種中分類名
        WF_MIDDLEOILNAME.Text = work.WF_SEL_MIDDLEOILNAME.Text
        'CODENAME_get("MIDDLEOILNAME", WF_MIDDLEOILNAME.Text, WF_MIDDLEOILNAME_TEXT.Text, WW_DUMMY)

        '油種中分類名カナ
        WF_MIDDLEOILKANA.Text = work.WF_SEL_MIDDLEOILKANA.Text
        'CODENAME_get("MIDDLEOILKANA", WF_MIDDLEOILKANA.Text, WF_MIDDLEOILKANA_TEXT.Text, WW_DUMMY)

        '油種コード
        WF_OILCODE.Text = work.WF_SEL_OILCODE2.Text
        'CODENAME_get("OILCODE", WF_OILCODE.Text, WF_OILCODE_TEXT.Text, WW_DUMMY)

        '油種名
        WF_OILNAME.Text = work.WF_SEL_OILNAME.Text
        'CODENAME_get("OILNAME", WF_OILNAME.Text, WF_OILNAME_TEXT.Text, WW_DUMMY)

        '油種名カナ
        WF_OILKANA.Text = work.WF_SEL_OILKANA.Text
        'CODENAME_get("OILKANA", WF_OILKANA.Text, WF_OILKANA_TEXT.Text, WW_DUMMY)

        '油種細分コード
        WF_SEGMENTOILCODE.Text = work.WF_SEL_SEGMENTOILCODE.Text
        'CODENAME_get("SEGMENTOILCODE", WF_SEGMENTOILCODE.Text, WF_SEGMENTOILCODE_TEXT.Text, WW_DUMMY)

        '油種名（細分）
        WF_SEGMENTOILNAME.Text = work.WF_SEL_SEGMENTOILNAME.Text
        'CODENAME_get("SEGMENTOILNAME", WF_SEGMENTOILNAME.Text, WF_SEGMENTOILNAME_TEXT.Text, WW_DUMMY)

        'OT油種コード
        WF_OTOILCODE.Text = work.WF_SEL_OTOILCODE.Text
        'CODENAME_get("OTOILCODE", WF_OTOILCODE.Text, WF_OTOILCODE_TEXT.Text, WW_DUMMY)

        'OT油種名
        WF_OTOILNAME.Text = work.WF_SEL_OTOILNAME.Text
        'CODENAME_get("OTOILNAME", WF_OTOILNAME.Text, WF_OTOILNAME_TEXT.Text, WW_DUMMY)

        '荷主油種コード
        WF_SHIPPEROILCODE.Text = work.WF_SEL_SHIPPEROILCODE.Text
        'CODENAME_get("SHIPPEROILCODE", WF_SHIPPEROILCODE.Text, WF_SHIPPEROILCODE_TEXT.Text, WW_DUMMY)

        '荷主油種名
        WF_SHIPPEROILNAME.Text = work.WF_SEL_SHIPPEROILNAME.Text
        'CODENAME_get("SHIPPEROILNAME", WF_SHIPPEROILNAME.Text, WF_SHIPPEROILNAME_TEXT.Text, WW_DUMMY)

        '積込チェック用油種コード
        WF_CHECKOILCODE.Text = work.WF_SEL_CHECKOILCODE.Text
        'CODENAME_get("CHECKOILCODE", WF_CHECKOILCODE.Text, WF_CHECKOILCODE_TEXT.Text, WW_DUMMY)

        '積込チェック用油種名
        WF_CHECKOILNAME.Text = work.WF_SEL_CHECKOILNAME.Text
        'CODENAME_get("CHECKOILNAME", WF_CHECKOILNAME.Text, WF_CHECKOILNAME_TEXT.Text, WW_DUMMY)

        '在庫管理対象フラグ
        WF_STOCKFLG.Text = work.WF_SEL_STOCKFLG.Text
        CODENAME_get("STOCKFLG", WF_STOCKFLG.Text, WF_STOCKFLG_TEXT.Text, WW_DUMMY)

        '受注登録可能期間FROM
        WF_ORDERFROMDATE.Text = work.WF_SEL_ORDERFROMDATE.Text

        '受注登録可能期間TO
        WF_ORDERTODATE.Text = work.WF_SEL_ORDERTODATE.Text

        '削除フラグ
        WF_DELFLG.Text = work.WF_SEL_DELFLG2.Text
        CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_DUMMY)

    End Sub

    ''' <summary>
    ''' 一意制約チェック
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub UniqueKeyCheck(ByVal SQLcon As SqlConnection, ByRef O_MESSAGENO As String)

        '○ 対象データ取得
        Dim SQLStr As String =
              " SELECT" _
            & "    OFFICECODE" _
            & "    , SHIPPERCODE" _
            & "    , PLANTCODE" _
            & "    , OILCODE" _
            & "    , SEGMENTOILCODE" _
            & " FROM" _
            & "    OIL.OIM0003_PRODUCT" _
            & " WHERE" _
            & "    OFFICECODE         =  @P01" _
            & "    AND SHIPPERCODE    =  @P02" _
            & "    AND PLANTCODE      =  @P03" _
            & "    AND OILCODE        =  @P04" _
            & "    AND SEGMENTOILCODE =  @P05" _
            & "    AND DELFLG         <> @P06"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 6)            '営業所コード
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 10)           '荷主コード
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 4)            '基地コード
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 4)            '油種コード
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 1)            '油種細分コード
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 1)            '削除フラグ

                PARA01.Value = WF_OFFICECODE.Text
                PARA02.Value = WF_SHIPPERCODE.Text
                PARA03.Value = WF_PLANTCODE.Text
                PARA04.Value = WF_OILCODE.Text
                PARA05.Value = WF_SEGMENTOILCODE.Text
                PARA06.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    Dim OIM0003Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIM0003Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIM0003Chk.Load(SQLdr)

                    If OIM0003Chk.Rows.Count > 0 Then
                        '重複データエラー
                        O_MESSAGENO = Messages.C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR
                    Else
                        '正常終了時
                        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0003C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0003C UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

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
        DetailBoxToOIM0003INPtbl(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ERR_SW) Then
            OIM0003tbl_UPD()
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIM0003tbl, work.WF_SEL_INPTBL.Text)

        '○ メッセージ表示
        If WW_ERR_SW = "" Then
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)
        Else
            If isNormal(WW_ERR_SW) Then
                Master.Output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.INF)

            ElseIf WW_ERR_SW = C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR Then
                Master.Output(WW_ERR_SW, C_MESSAGE_TYPE.ERR, "営業所コード,荷主コード,基地コード,油種コード,油種細分コード,削除フラグ", needsPopUp:=True)

            Else
                Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)

            End If
        End If

        '○画面切替設定
        'WF_BOXChange.Value = "headerbox"

        '############# おためし #############
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
    Protected Sub DetailBoxToOIM0003INPtbl(ByRef O_RTN As String)

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

        Master.CreateEmptyTable(OIM0003INPtbl, work.WF_SEL_INPTBL.Text)
        Dim OIM0003INProw As DataRow = OIM0003INPtbl.NewRow

        '○ 初期クリア
        For Each OIM0003INPcol As DataColumn In OIM0003INPtbl.Columns
            If IsDBNull(OIM0003INProw.Item(OIM0003INPcol)) OrElse IsNothing(OIM0003INProw.Item(OIM0003INPcol)) Then
                Select Case OIM0003INPcol.ColumnName
                    Case "LINECNT"
                        OIM0003INProw.Item(OIM0003INPcol) = 0
                    Case "OPERATION"
                        OIM0003INProw.Item(OIM0003INPcol) = C_LIST_OPERATION_CODE.NODATA
                    Case "TIMSTP"
                        OIM0003INProw.Item(OIM0003INPcol) = 0
                    Case "SELECT"
                        OIM0003INProw.Item(OIM0003INPcol) = 1
                    Case "HIDDEN"
                        OIM0003INProw.Item(OIM0003INPcol) = 0
                    Case Else
                        OIM0003INProw.Item(OIM0003INPcol) = ""
                End Select
            End If
        Next

        'LINECNT
        If WF_Sel_LINECNT.Text = "" Then
            OIM0003INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(WF_Sel_LINECNT.Text, OIM0003INProw("LINECNT"))
            Catch ex As Exception
                OIM0003INProw("LINECNT") = 0
            End Try
        End If

        OIM0003INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        OIM0003INProw("TIMSTP") = 0
        OIM0003INProw("SELECT") = 1
        OIM0003INProw("HIDDEN") = 0

        OIM0003INProw("OFFICECODE") = WF_OFFICECODE.Text            '営業所コード
        OIM0003INProw("SHIPPERCODE") = WF_SHIPPERCODE.Text          '荷主コード
        OIM0003INProw("PLANTCODE") = WF_PLANTCODE.Text              '基地コード
        OIM0003INProw("BIGOILCODE") = WF_BIGOILCODE.Text            '油種大分類コード
        OIM0003INProw("BIGOILNAME") = WF_BIGOILNAME.Text            '油種大分類名
        OIM0003INProw("BIGOILKANA") = WF_BIGOILKANA.Text            '油種大分類名カナ
        OIM0003INProw("MIDDLEOILCODE") = WF_MIDDLEOILCODE.Text      '油種中分類コード
        OIM0003INProw("MIDDLEOILNAME") = WF_MIDDLEOILNAME.Text      '油種中分類名
        OIM0003INProw("MIDDLEOILKANA") = WF_MIDDLEOILKANA.Text      '油種中分類名カナ
        OIM0003INProw("OILCODE") = WF_OILCODE.Text                  '油種コード
        OIM0003INProw("OILNAME") = WF_OILNAME.Text                  '油種名
        OIM0003INProw("OILKANA") = WF_OILKANA.Text                  '油種名カナ
        OIM0003INProw("SEGMENTOILCODE") = WF_SEGMENTOILCODE.Text    '油種細分コード
        OIM0003INProw("SEGMENTOILNAME") = WF_SEGMENTOILNAME.Text    '油種名（細分）
        OIM0003INProw("OTOILCODE") = WF_OTOILCODE.Text              'OT油種コード
        OIM0003INProw("OTOILNAME") = WF_OTOILNAME.Text              'OT油種名
        OIM0003INProw("SHIPPEROILCODE") = WF_SHIPPEROILCODE.Text    '荷主油種コード
        OIM0003INProw("SHIPPEROILNAME") = WF_SHIPPEROILNAME.Text    '荷主油種名
        OIM0003INProw("CHECKOILCODE") = WF_CHECKOILCODE.Text        '積込チェック用油種コード
        OIM0003INProw("CHECKOILNAME") = WF_CHECKOILNAME.Text        '積込チェック用油種名
        OIM0003INProw("STOCKFLG") = WF_STOCKFLG.Text                '在庫管理対象フラグ
        OIM0003INProw("ORDERFROMDATE") = WF_ORDERFROMDATE.Text      '受注登録可能期間FROM
        OIM0003INProw("ORDERTODATE") = WF_ORDERTODATE.Text          '受注登録可能期間TO
        OIM0003INProw("DELFLG") = WF_DELFLG.Text                    '削除フラグ

        '○ チェック用テーブルに登録する
        OIM0003INPtbl.Rows.Add(OIM0003INProw)

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
        For Each OIM0003row As DataRow In OIM0003tbl.Rows
            Select Case OIM0003row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIM0003tbl, work.WF_SEL_INPTBL.Text)

        WF_Sel_LINECNT.Text = ""            'LINECNT

        WF_OFFICECODE.Text = ""             '営業所コード
        WF_SHIPPERCODE.Text = ""            '荷主コード
        WF_PLANTCODE.Text = ""              '基地コード
        WF_BIGOILCODE.Text = ""             '油種大分類コード
        WF_BIGOILNAME.Text = ""             '油種大分類名
        WF_BIGOILKANA.Text = ""             '油種大分類名カナ
        WF_MIDDLEOILCODE.Text = ""          '油種中分類コード
        WF_MIDDLEOILNAME.Text = ""          '油種中分類名
        WF_MIDDLEOILKANA.Text = ""          '油種中分類名カナ
        WF_OILCODE.Text = ""                '油種コード
        WF_OILNAME.Text = ""                '油種名
        WF_OILKANA.Text = ""                '油種名カナ
        WF_SEGMENTOILCODE.Text = ""         '油種細分コード
        WF_SEGMENTOILNAME.Text = ""         '油種名（細分）
        WF_OTOILCODE.Text = ""              'OT油種コード
        WF_OTOILNAME.Text = ""              'OT油種名
        WF_SHIPPEROILCODE.Text = ""         '荷主油種コード
        WF_SHIPPEROILNAME.Text = ""         '荷主油種名
        WF_CHECKOILCODE.Text = ""           '積込チェック用油種コード
        WF_CHECKOILNAME.Text = ""           '積込チェック用油種名
        WF_STOCKFLG.Text = ""               '在庫管理対象フラグ
        WF_ORDERFROMDATE.Text = ""          '受注登録可能期間FROM
        WF_ORDERTODATE.Text = ""            '受注登録可能期間TO
        WF_DELFLG.Text = ""                 '削除フラグ

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
                        '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                        Select Case WF_FIELD.Value
                            Case WF_ORDERFROMDATE.ID
                                '受注登録可能期間FROM
                                .WF_Calendar.Text = WF_ORDERFROMDATE.Text
                            Case WF_ORDERTODATE.ID
                                '受注登録可能期間TO
                                .WF_Calendar.Text = WF_ORDERTODATE.Text
                        End Select
                        .ActiveCalendar()

                    Case Else
                        Dim prmData As New Hashtable

                        'フィールドによってパラメータを変える
                        Select Case WF_FIELD.Value
                            Case WF_OFFICECODE.ID
                                '営業所コード
                                prmData = work.CreateSALESOFFICEParam(Master.USERCAMP, WF_OFFICECODE.Text)
                            Case WF_SHIPPERCODE.ID
                                '荷主コード
                                prmData = work.CreateFIXParam(Master.USERCAMP, "JOINTMASTER")
                            Case WF_PLANTCODE.ID
                                '基地コード
                                prmData = work.CreateFIXParam(Master.USERCAMP, "PLANTMASTER")
                            Case WF_BIGOILCODE.ID
                                '油種大分類コード
                                prmData = work.CreateFIXParam(Master.USERCAMP, "BIGOILCODE")
                            Case WF_MIDDLEOILCODE.ID
                                '油種中分類コード
                                prmData = work.CreateFIXParam(Master.USERCAMP, "MIDDLEOILCODE")
                            Case WF_OTOILCODE.ID
                                'OT油種コード
                                prmData = work.CreateFIXParam(Master.USERCAMP, "OTOILCODE")
                            Case WF_STOCKFLG.ID
                                '在庫管理対象フラグ
                                prmData = work.CreateFIXParam(Master.USERCAMP, "PRODUCTSTOCKFLG")
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
            Case WF_OFFICECODE.ID
                '営業所コード
                CODENAME_get("OFFICECODE", WF_OFFICECODE.Text, WF_OFFICECODE_TEXT.Text, WW_RTN_SW)
            Case WF_SHIPPERCODE.ID
                '荷主コード
                CODENAME_get("SHIPPERCODE", WF_SHIPPERCODE.Text, WF_SHIPPERCODE_TEXT.Text, WW_RTN_SW)
            Case WF_PLANTCODE.ID
                '基地コード
                CODENAME_get("PLANTCODE", WF_PLANTCODE.Text, WF_PLANTCODE_TEXT.Text, WW_RTN_SW)
            Case WF_BIGOILCODE.ID
                '油種大分類コード
                CODENAME_get("BIGOILCODE", WF_BIGOILCODE.Text, WF_BIGOILCODE_TEXT.Text, WW_RTN_SW)
            Case WF_MIDDLEOILCODE.ID
                '油種中分類コード
                CODENAME_get("MIDDLEOILCODE", WF_MIDDLEOILCODE_TEXT.Text, WF_MIDDLEOILCODE_TEXT.Text, WW_RTN_SW)
            Case WF_OTOILCODE.ID
                'OT油種コード
                CODENAME_get("OTOILCODE", WF_OTOILCODE.Text, WF_OTOILCODE_TEXT.Text, WW_RTN_SW)
            Case WF_STOCKFLG.ID
                '在庫管理対象フラグ
                CODENAME_get("STOCKFLG", WF_STOCKFLG.Text, WF_STOCKFLG_TEXT.Text, WW_RTN_SW)
            Case WF_DELFLG.ID
                '削除フラグ
                CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_RTN_SW)

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
                Case WF_OFFICECODE.ID
                    '営業所コード
                    WF_OFFICECODE.Text = WW_SelectValue
                    WF_OFFICECODE_TEXT.Text = WW_SelectText
                    WF_OFFICECODE.Focus()
                Case WF_SHIPPERCODE.ID
                    '荷主コード
                    WF_SHIPPERCODE.Text = WW_SelectValue
                    WF_SHIPPERCODE_TEXT.Text = WW_SelectText
                    WF_SHIPPERCODE.Focus()
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
                Case WF_MIDDLEOILCODE.ID
                    '油種中分類コード
                    WF_MIDDLEOILCODE.Text = WW_SelectValue
                    WF_MIDDLEOILCODE_TEXT.Text = WW_SelectText
                    WF_MIDDLEOILCODE.Focus()
                Case WF_OTOILCODE.ID
                    'OT油種コード
                    WF_OTOILCODE.Text = WW_SelectValue
                    WF_OTOILCODE_TEXT.Text = WW_SelectText
                    WF_OTOILCODE.Focus()
                Case WF_STOCKFLG.ID
                    '在庫管理対象フラグ
                    WF_STOCKFLG.Text = WW_SelectValue
                    WF_STOCKFLG_TEXT.Text = WW_SelectText
                    WF_STOCKFLG.Focus()
                Case WF_ORDERFROMDATE.ID
                    '受注登録可能期間FROM
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_ORDERFROMDATE.Text = ""
                        Else
                            WF_ORDERFROMDATE.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_ORDERFROMDATE.Focus()
                Case WF_ORDERTODATE.ID
                    '受注登録可能期間TO
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_ORDERTODATE.Text = ""
                        Else
                            WF_ORDERTODATE.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_ORDERTODATE.Focus()
                Case WF_DELFLG.ID
                    '削除フラグ
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
                Case WF_OFFICECODE.ID
                    '営業所コード
                    WF_OFFICECODE.Focus()
                Case WF_SHIPPERCODE.ID
                    '荷主コード
                    WF_SHIPPERCODE.Focus()
                Case WF_PLANTCODE.ID
                    '基地コード
                    WF_PLANTCODE.Focus()
                Case WF_BIGOILCODE.ID
                    '油種大分類コード
                    WF_BIGOILCODE.Focus()
                Case WF_MIDDLEOILCODE.ID
                    '油種中分類コード
                    WF_MIDDLEOILCODE.Focus()
                Case WF_OTOILCODE.ID
                    'OT油種コード
                    WF_OTOILCODE.Focus()
                Case WF_STOCKFLG.ID
                    '在庫管理対象フラグ
                    WF_STOCKFLG.Focus()
                Case WF_ORDERFROMDATE.ID
                    '受注登録可能期間FROM
                    WF_ORDERFROMDATE.Focus()
                Case WF_ORDERTODATE.ID
                    '受注登録可能期間TO
                    WF_ORDERTODATE.Focus()
                Case WF_DELFLG.ID
                    '削除フラグ
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
        For Each OIM0003INProw As DataRow In OIM0003INPtbl.Rows

            WW_LINE_ERR = ""

            '削除フラグ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "DELFLG", OIM0003INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If Not String.IsNullOrEmpty(OIM0003INProw("DELFLG")) Then
                    '値存在チェック
                    CODENAME_get("DELFLG", OIM0003INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(削除フラグ入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除フラグ入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '営業所コード(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "OFFICECODE", OIM0003INProw("OFFICECODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If Not String.IsNullOrEmpty(OIM0003INProw("OFFICECODE")) Then
                    '値存在チェック
                    CODENAME_get("OFFICECODE", OIM0003INProw("OFFICECODE"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(営業所コード入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(営業所コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '荷主コード(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "SHIPPERCODE", OIM0003INProw("SHIPPERCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If Not String.IsNullOrEmpty(OIM0003INProw("SHIPPERCODE")) Then
                    '値存在チェック
                    CODENAME_get("SHIPPERCODE", OIM0003INProw("SHIPPERCODE"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(荷主コード入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(荷主コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '基地コード(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "PLANTCODE", OIM0003INProw("PLANTCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If Not String.IsNullOrEmpty(OIM0003INProw("PLANTCODE")) Then
                    '値存在チェック
                    CODENAME_get("PLANTCODE", OIM0003INProw("PLANTCODE"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(基地コード入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(基地コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種大分類コード(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "BIGOILCODE", OIM0003INProw("BIGOILCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If Not String.IsNullOrEmpty(OIM0003INProw("BIGOILCODE")) Then
                    '値存在チェック
                    CODENAME_get("BIGOILCODE", OIM0003INProw("BIGOILCODE"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(油種大分類コード入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(油種大分類コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種大分類名(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "BIGOILNAME", OIM0003INProw("BIGOILNAME"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(油種大分類名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種大分類名カナ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "BIGOILKANA", OIM0003INProw("BIGOILKANA"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(油種大分類名カナ入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種中分類コード(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "MIDDLEOILCODE", OIM0003INProw("MIDDLEOILCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If Not String.IsNullOrEmpty(OIM0003INProw("MIDDLEOILCODE")) Then
                    '値存在チェック
                    CODENAME_get("MIDDLEOILCODE", OIM0003INProw("MIDDLEOILCODE"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(油種中分類コード入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(油種中分類コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種中分類名(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "MIDDLEOILNAME", OIM0003INProw("MIDDLEOILNAME"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(油種中分類名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種中分類名カナ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "MIDDLEOILKANA", OIM0003INProw("MIDDLEOILKANA"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(油種中分類名カナ入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種コード(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "OILCODE", OIM0003INProw("OILCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(油種コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種名(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "OILNAME", OIM0003INProw("OILNAME"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(油種名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種名カナ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "OILKANA", OIM0003INProw("OILKANA"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(油種名カナ入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種細分コード(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "SEGMENTOILCODE", OIM0003INProw("SEGMENTOILCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(油種細分コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種名（細分）(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "SEGMENTOILNAME", OIM0003INProw("SEGMENTOILNAME"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(油種名（細分）入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'OT油種コード(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "OTOILCODE", OIM0003INProw("OTOILCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If Not String.IsNullOrEmpty(OIM0003INProw("OTOILCODE")) Then
                    '値存在チェック
                    CODENAME_get("OTOILCODE", OIM0003INProw("OTOILCODE"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(OT油種コード入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(OT油種コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'OT油種名(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "OTOILNAME", OIM0003INProw("OTOILNAME"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(OT油種名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '荷主油種コード(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "SHIPPEROILCODE", OIM0003INProw("SHIPPEROILCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(荷主油種コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '荷主油種名(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "SHIPPEROILNAME", OIM0003INProw("SHIPPEROILNAME"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(荷主油種名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '積込チェック用油種コード(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "CHECKOILCODE", OIM0003INProw("CHECKOILCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(積込チェック用油種コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '積込チェック用油種名(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "CHECKOILNAME", OIM0003INProw("CHECKOILNAME"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(積込チェック用油種名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '在庫管理対象フラグ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "STOCKFLG", OIM0003INProw("STOCKFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If Not String.IsNullOrEmpty(OIM0003INProw("STOCKFLG")) Then
                    '値存在チェック
                    CODENAME_get("STOCKFLG", OIM0003INProw("STOCKFLG"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(在庫管理対象フラグ入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(在庫管理対象フラグ入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '受注登録可能期間FROM(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "ORDERFROMDATE", OIM0003INProw("ORDERFROMDATE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If Not String.IsNullOrEmpty(OIM0003INProw("ORDERFROMDATE")) Then
                    '年月日チェック
                    WW_CheckDate(OIM0003INProw("ORDERFROMDATE"), "受注登録可能期間FROM", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(受注登録可能期間FROM入力エラー)です。"
                        WW_CheckMES2 = WW_CS0024FCHECKERR
                        O_RTN = "ERR"
                        Exit Sub
                    Else
                        OIM0003INProw("ORDERFROMDATE") = CDate(OIM0003INProw("ORDERFROMDATE")).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(受注登録可能期間FROM入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '受注登録可能期間TO(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "ORDERTODATE", OIM0003INProw("ORDERTODATE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If Not String.IsNullOrEmpty(OIM0003INProw("ORDERTODATE")) Then
                    '年月日チェック
                    WW_CheckDate(OIM0003INProw("ORDERTODATE"), "受注登録可能期間TO", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(受注登録可能期間TO入力エラー)です。"
                        WW_CheckMES2 = WW_CS0024FCHECKERR
                        O_RTN = "ERR"
                        Exit Sub
                    Else
                        OIM0003INProw("ORDERTODATE") = CDate(OIM0003INProw("ORDERTODATE")).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(受注登録可能期間TO入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '受注登録可能期間FROM-TOチェック
            If Not String.IsNullOrEmpty(OIM0003INProw("ORDERFROMDATE")) AndAlso Not String.IsNullOrEmpty(OIM0003INProw("ORDERTODATE")) Then
                If CDate(OIM0003INProw("ORDERFROMDATE")).CompareTo(CDate(OIM0003INProw("ORDERTODATE"))) > 0 Then
                    WW_CheckMES1 = "・更新できないレコード(受注登録可能期間FROM-TO入力エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.START_END_DATE_RELATION_ERROR
                End If
            End If

            '一意制約チェック
            '同一レコードの更新の場合、チェック対象外
            '2020/06/16杉山修正
            If OIM0003INProw("OFFICECODE") = work.WF_SEL_OFFICECODE2.Text AndAlso
                OIM0003INProw("SHIPPERCODE") = work.WF_SEL_SHIPPERCODE2.Text AndAlso
                OIM0003INProw("PLANTCODE") = work.WF_SEL_PLANTCODE2.Text AndAlso
                OIM0003INProw("OILCODE") = work.WF_SEL_OILCODE2.Text AndAlso
                OIM0003INProw("SEGMENTOILCODE") = work.WF_SEL_SEGMENTOILCODE.Text AndAlso
                OIM0003INProw("DELFLG") = work.WF_SEL_DELFLG2.Text Then

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
                                   "([" & OIM0003INProw("OFFICECODE") & "]" &
                                   "([" & OIM0003INProw("SHIPPERCODE") & "]" &
                                   "([" & OIM0003INProw("PLANTCODE") & "]" &
                                   "([" & OIM0003INProw("OILCODE") & "]" &
                                   "([" & OIM0003INProw("SEGMENTOILCODE") & "]" &
                                   " [" & OIM0003INProw("DELFLG") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR
                End If
            End If

            If WW_LINE_ERR = "" Then
                If OIM0003INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    OIM0003INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LINE_ERR = CONST_PATTERNERR Then
                    '関連チェックエラーをセット
                    OIM0003INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    '単項目チェックエラーをセット
                    OIM0003INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
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
    ''' <param name="OIM0003row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIM0003row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIM0003row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 営業所コード             =" & OIM0003row("OFFICECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 荷主コード               =" & OIM0003row("SHIPPERCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 基地コード               =" & OIM0003row("PLANTCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種大分類コード         =" & OIM0003row("BIGOILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種大分類名             =" & OIM0003row("BIGOILNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種大分類名カナ         =" & OIM0003row("BIGOILKANA") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種中分類コード         =" & OIM0003row("MIDDLEOILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種中分類名             =" & OIM0003row("MIDDLEOILNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種中分類名カナ         =" & OIM0003row("MIDDLEOILKANA") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種コード               =" & OIM0003row("OILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種名                   =" & OIM0003row("OILNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種名カナ               =" & OIM0003row("OILKANA") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種細分コード           =" & OIM0003row("SEGMENTOILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種名（細分）           =" & OIM0003row("SEGMENTOILNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> OT油種コード             =" & OIM0003row("OTOILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> OT油種名                 =" & OIM0003row("OTOILNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 荷主油種コード           =" & OIM0003row("SHIPPEROILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 荷主油種名               =" & OIM0003row("SHIPPEROILNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 積込チェック用油種コード =" & OIM0003row("CHECKOILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 積込チェック用油種名     =" & OIM0003row("CHECKOILNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 在庫管理対象フラグ       =" & OIM0003row("STOCKFLG") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 受注登録可能期間FROM     =" & OIM0003row("ORDERFROMDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 受注登録可能期間TO       =" & OIM0003row("ORDERTODATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除フラグ               =" & OIM0003row("DELFLG")
        End If

        rightview.AddErrorReport(WW_ERR_MES)

    End Sub


    ''' <summary>
    ''' OIM0003tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub OIM0003tbl_UPD()

        '○ 画面状態設定
        For Each OIM0003row As DataRow In OIM0003tbl.Rows
            Select Case OIM0003row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each OIM0003INProw As DataRow In OIM0003INPtbl.Rows

            'エラーレコード読み飛ばし
            If OIM0003INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            OIM0003INProw.Item("OPERATION") = CONST_INSERT

            'KEY項目が等しい時
            For Each OIM0003row As DataRow In OIM0003tbl.Rows
                If OIM0003row("OFFICECODE") = OIM0003INProw("OFFICECODE") AndAlso
                    OIM0003row("SHIPPERCODE") = OIM0003INProw("SHIPPERCODE") AndAlso
                    OIM0003row("PLANTCODE") = OIM0003INProw("PLANTCODE") AndAlso
                    OIM0003row("OILCODE") = OIM0003INProw("OILCODE") AndAlso
                    OIM0003row("SEGMENTOILCODE") = OIM0003INProw("SEGMENTOILCODE") Then
                    'KEY項目以外の項目に変更がないときは「操作」の項目は空白にする
                    If OIM0003row("DELFLG") = OIM0003INProw("DELFLG") AndAlso
                        OIM0003INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA Then
                    Else
                        'KEY項目以外の項目に変更がある時は「操作」の項目を「更新」に設定する
                        OIM0003INProw("OPERATION") = CONST_UPDATE
                        Exit For
                    End If

                    Exit For

                End If
            Next
        Next

        '○ 変更有無判定　&　入力値反映
        For Each OIM0003INProw As DataRow In OIM0003INPtbl.Rows
            Select Case OIM0003INProw("OPERATION")
                Case CONST_UPDATE
                    TBL_UPDATE_SUB(OIM0003INProw)
                Case CONST_INSERT
                    TBL_INSERT_SUB(OIM0003INProw)
                Case CONST_PATTERNERR
                    '関連チェックエラーの場合、キーが変わるため、行追加してエラーレコードを表示させる
                    TBL_INSERT_SUB(OIM0003INProw)
                Case C_LIST_OPERATION_CODE.ERRORED
                    TBL_ERR_SUB(OIM0003INProw)
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="OIM0003INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef OIM0003INProw As DataRow)

        For Each OIM0003row As DataRow In OIM0003tbl.Rows

            '同一レコードか判定
            If OIM0003INProw("OFFICECODE") = OIM0003row("OFFICECODE") AndAlso
                OIM0003INProw("SHIPPERCODE") = OIM0003row("SHIPPERCODE") AndAlso
                OIM0003INProw("PLANTCODE") = OIM0003row("PLANTCODE") AndAlso
                OIM0003INProw("OILCODE") = OIM0003row("OILCODE") AndAlso
                OIM0003INProw("SEGMENTOILCODE") = OIM0003row("SEGMENTOILCODE") Then
                '画面入力テーブル項目設定
                OIM0003INProw("LINECNT") = OIM0003row("LINECNT")
                OIM0003INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                OIM0003INProw("TIMSTP") = OIM0003row("TIMSTP")
                OIM0003INProw("SELECT") = 1
                OIM0003INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0003row.ItemArray = OIM0003INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 追加予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0003INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef OIM0003INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim OIM0003row As DataRow = OIM0003tbl.NewRow
        OIM0003row.ItemArray = OIM0003INProw.ItemArray

        OIM0003row("LINECNT") = OIM0003tbl.Rows.Count + 1
        If OIM0003INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
            OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        Else
            OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
        End If

        OIM0003row("TIMSTP") = "0"
        OIM0003row("SELECT") = 1
        OIM0003row("HIDDEN") = 0

        OIM0003tbl.Rows.Add(OIM0003row)

    End Sub


    ''' <summary>
    ''' エラーデータの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0003INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_ERR_SUB(ByRef OIM0003INProw As DataRow)

        For Each OIM0003row As DataRow In OIM0003tbl.Rows

            '同一レコードか判定
            If OIM0003INProw("OFFICECODE") = OIM0003row("OFFICECODE") AndAlso
                OIM0003INProw("SHIPPERCODE") = OIM0003row("SHIPPERCODE") AndAlso
                OIM0003INProw("PLANTCODE") = OIM0003row("PLANTCODE") AndAlso
                OIM0003INProw("OILCODE") = OIM0003row("OILCODE") AndAlso
                OIM0003INProw("SEGMENTOILCODE") = OIM0003row("SEGMENTOILCODE") Then
                '画面入力テーブル項目設定
                OIM0003INProw("LINECNT") = OIM0003row("LINECNT")
                OIM0003INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                OIM0003INProw("TIMSTP") = OIM0003row("TIMSTP")
                OIM0003INProw("SELECT") = 1
                OIM0003INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0003row.ItemArray = OIM0003INProw.ItemArray
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
            Select Case I_FIELD
                Case "OFFICECODE"
                    '営業所コード
                    prmData = work.CreateSALESOFFICEParam(Master.USERCAMP, I_VALUE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SALESOFFICE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "SHIPPERCODE"
                    '荷主コード
                    prmData = work.CreateFIXParam(Master.USERCAMP, "JOINTMASTER")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_JOINTLIST, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "PLANTCODE"
                    '基地コード
                    prmData = work.CreateFIXParam(Master.USERCAMP, "PLANTMASTER")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "BIGOILCODE"
                    '油種大分類コード
                    prmData = work.CreateFIXParam(Master.USERCAMP, "BIGOILCODE")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "MIDDLEOILCODE"
                    '油種中分類コード
                    prmData = work.CreateFIXParam(Master.USERCAMP, "MIDDLEOILCODE")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "OTOILCODE"
                    'OT油種コード
                    prmData = work.CreateFIXParam(Master.USERCAMP, "OTOILCODE")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STOCKFLG"
                    '在庫管理対象フラグ
                    prmData = work.CreateFIXParam(Master.USERCAMP, "PRODUCTSTOCKFLG")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "DELFLG"
                    '削除フラグ
                    prmData = work.CreateFIXParam(Master.USERCAMP, "DELFLG")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
