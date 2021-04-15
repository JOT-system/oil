''************************************************************
' 油槽所諸元マスタメンテナンス登録・更新
' 作成日 2020/11/18
' 更新日 2021/04/15
' 作成者 JOT常井
' 更新者 JOT伊草
'
' 修正履歴:2020/11/18 新規作成
'         :2021/04/15 1)表更新→更新、クリア→戻る、に名称変更
'                     2)戻るボタン押下時、確認ダイアログ表示→
'                       確認ダイアログでOK押下時、一覧画面に戻るように修正
'                     3)更新ボタン押下時、この画面でDB更新→
'                       一覧画面の表示データに更新後の内容反映して戻るように修正
''************************************************************
Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' 油槽所諸元マスタメンテナンス登録・更新（実行）
''' </summary>
''' <remarks></remarks>
Public Class OIM0015SyogenCreate
    Inherits Page

    '○ 検索結果格納Table
    Private OIM0015tbl As DataTable                                  '一覧格納用テーブル
    Private OIM0015INPtbl As DataTable                               'チェック用テーブル
    Private OIM0015UPDtbl As DataTable                               '更新用テーブル

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
                    Master.RecoverTable(OIM0015tbl, work.WF_SEL_INPTBL.Text)

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
                        Case "btnClearConfirmOk"        '戻るボタン押下後の確認ダイアログでOK押下
                            WF_CLEAR_ConfirmOkClick()
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
            If Not IsNothing(OIM0015tbl) Then
                OIM0015tbl.Clear()
                OIM0015tbl.Dispose()
                OIM0015tbl = Nothing
            End If

            If Not IsNothing(OIM0015INPtbl) Then
                OIM0015INPtbl.Clear()
                OIM0015INPtbl.Dispose()
                OIM0015INPtbl = Nothing
            End If

            If Not IsNothing(OIM0015UPDtbl) Then
                OIM0015UPDtbl.Clear()
                OIM0015UPDtbl.Dispose()
                OIM0015UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIM0015WRKINC.MAPIDC
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0015L Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        End If

        '○ 名称設定処理
        '選択行
        WF_Sel_LINECNT.Text = work.WF_SEL_LINECNT.Text

        '荷受人コード
        WF_CONSIGNEECODE.Text = work.WF_SEL_CONSIGNEECODE2.Text
        CODENAME_get("CONSIGNEECODE", WF_CONSIGNEECODE.Text, WF_CONSIGNEECODE_TEXT.Text, WW_RTN_SW)

        '荷主コード
        WF_SHIPPERSCODE.Text = work.WF_SEL_SHIPPERSCODE2.Text
        CODENAME_get("SHIPPERSCODE", WF_SHIPPERSCODE.Text, WF_SHIPPERSCODE_TEXT.Text, WW_RTN_SW)

        '開始月日
        WF_FROMMD.Text = work.WF_SEL_FROMMD.Text

        '終了月日
        WF_TOMD.Text = work.WF_SEL_TOMD.Text

        '油種コード
        WF_OILCODE.Text = work.WF_SEL_OILCODE2.Text
        CODENAME_get("OILCODE", WF_OILCODE.Text, WF_OILCODE_TEXT.Text, WW_RTN_SW)

        'タンク容量
        WF_TANKCAP.Text = work.WF_SEL_TANKCAP.Text

        '目標在庫率
        WF_TARGETCAPRATE.Text = work.WF_SEL_TARGETCAPRATE.Text

        'Ｄ／Ｓ
        WF_DS.Text = work.WF_SEL_DS.Text

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
            & "     CONSIGNEECODE " _
            & "     , SHIPPERSCODE " _
            & "     , OILCODE " _
            & " FROM" _
            & "    OIL.OIM0015_SYOGEN" _
            & " WHERE" _
            & "     CONSIGNEECODE   = @P1" _
            & " AND SHIPPERSCODE    = @P2" _
            & " AND OILCODE         = @P3" _
            & " AND DELFLG         <> @P0"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA0 As SqlParameter = SQLcmd.Parameters.Add("@P0", SqlDbType.NVarChar, 1)     '削除フラグ
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 10)    '荷受人コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 10)    '荷主コード
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 4)     '油種コード
                PARA0.Value = C_DELETE_FLG.DELETE
                PARA1.Value = WF_CONSIGNEECODE.Text
                PARA2.Value = WF_SHIPPERSCODE.Text
                PARA3.Value = WF_OILCODE.Text

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    Dim OIM0015Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIM0015Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIM0015Chk.Load(SQLdr)

                    If OIM0015Chk.Rows.Count > 0 Then
                        '重複データエラー
                        O_MESSAGENO = Messages.C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR
                    Else
                        '正常終了時
                        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0015C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0015C UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 油槽所諸元マスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateMaster(ByVal SQLcon As SqlConnection)

        '○ ＤＢ更新
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;" _
            & "    SET @hensuu = 0 ;" _
            & " DECLARE hensuu CURSOR FOR" _
            & "    SELECT" _
            & "        CAST(UPDTIMSTP AS bigint) AS hensuu" _
            & "    FROM" _
            & "        OIL.OIM0015_SYOGEN" _
            & "    WHERE" _
            & "        CONSIGNEECODE  = @P01 " _
            & "        AND " _
            & "        SHIPPERSCODE   = @P02 " _
            & "        AND " _
            & "        OILCODE        = @P05 ;" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE OIL.OIM0015_SYOGEN" _
            & "    SET" _
            & "        DELFLG = @P00" _
            & "        , FROMMD = @P03" _
            & "        , TOMD = @P04" _
            & "        , TANKCAP = @P06" _
            & "        , TARGETCAPRATE = @P07" _
            & "        , DS = @P08" _
            & "    WHERE" _
            & "        CONSIGNEECODE  = @P01 " _
            & "        AND " _
            & "        SHIPPERSCODE   = @P02 " _
            & "        AND " _
            & "        OILCODE        = @P05 ;" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO OIL.OIM0015_SYOGEN" _
            & "        (DELFLG" _
            & "        , CONSIGNEECODE" _
            & "        , SHIPPERSCODE" _
            & "        , FROMMD" _
            & "        , TOMD" _
            & "        , OILCODE" _
            & "        , TANKCAP" _
            & "        , TARGETCAPRATE" _
            & "        , DS" _
            & "        , INITYMD" _
            & "        , INITUSER" _
            & "        , INITTERMID" _
            & "        , UPDYMD" _
            & "        , UPDUSER" _
            & "        , UPDTERMID" _
            & "        , RECEIVEYMD)" _
            & "    VALUES" _
            & "        (@P00" _
            & "        , @P01" _
            & "        , @P02" _
            & "        , @P03" _
            & "        , @P04" _
            & "        , @P05" _
            & "        , @P06" _
            & "        , @P07" _
            & "        , @P08" _
            & "        , @P09" _
            & "        , @P10" _
            & "        , @P11" _
            & "        , @P12" _
            & "        , @P13" _
            & "        , @P14" _
            & "        , @P15) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
              " Select" _
            & "    DELFLG" _
            & "    , CONSIGNEECODE" _
            & "    , SHIPPERSCODE" _
            & "    , FROMMD" _
            & "    , TOMD" _
            & "    , OILCODE" _
            & "    , TANKCAP" _
            & "    , TARGETCAPRATE" _
            & "    , DS" _
            & "    , INITYMD" _
            & "    , INITUSER" _
            & "    , INITTERMID" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & "    , CAST(UPDTIMSTP As bigint) As UPDTIMSTP" _
            & " FROM" _
            & "    OIL.OIM0015_SYOGEN" _
            & " WHERE" _
            & "        CONSIGNEECODE  = @P01 " _
            & "        AND " _
            & "        SHIPPERSCODE   = @P02 " _
            & "        AND " _
            & "        OILCODE        = @P03 ;"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                'DB更新パラメータ
                Dim PARA00 As SqlParameter = SQLcmd.Parameters.Add("@P00", SqlDbType.NVarChar, 1)           '削除フラグ

                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 10)          '荷受人コード
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 10)          '荷主コード
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 10)          '開始月日
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 10)          '終了月日
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 10)          '油種コード
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 10)          'タンク容量
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.NVarChar, 10)          '目標在庫率
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.NVarChar, 10)          'Ｄ／Ｓ

                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", SqlDbType.DateTime)              '登録年月日
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 20)          '登録ユーザーＩＤ
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 20)          '登録端末
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.DateTime)              '更新年月日
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 20)          '更新ユーザーＩＤ
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 20)          '更新端末
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.DateTime)              '集信日時

                '更新ジャーナル取得パラメータ
                Dim JPARA00 As SqlParameter = SQLcmdJnl.Parameters.Add("@P00", SqlDbType.NVarChar, 1)       '削除フラグ
                Dim JPARA01 As SqlParameter = SQLcmdJnl.Parameters.Add("@P01", SqlDbType.NVarChar, 4)       '荷受人コード
                Dim JPARA02 As SqlParameter = SQLcmdJnl.Parameters.Add("@P02", SqlDbType.NVarChar, 4)       '荷主コード
                Dim JPARA03 As SqlParameter = SQLcmdJnl.Parameters.Add("@P03", SqlDbType.NVarChar, 4)       '油種コード

                Dim OIM0015row As DataRow = OIM0015INPtbl.Rows(0)
                Dim WW_DATENOW As DateTime = Date.Now

                'DB更新
                PARA00.Value = OIM0015row("DELFLG")
                PARA01.Value = OIM0015row("CONSIGNEECODE")
                PARA02.Value = OIM0015row("SHIPPERSCODE")
                PARA03.Value = OIM0015row("FROMMD")
                PARA04.Value = OIM0015row("TOMD")
                PARA05.Value = OIM0015row("OILCODE")
                PARA06.Value = OIM0015row("TANKCAP")
                PARA07.Value = OIM0015row("TARGETCAPRATE")
                PARA08.Value = OIM0015row("DS")
                PARA09.Value = WW_DATENOW
                PARA10.Value = Master.USERID
                PARA11.Value = Master.USERTERMID
                PARA12.Value = WW_DATENOW
                PARA13.Value = Master.USERID
                PARA14.Value = Master.USERTERMID
                PARA15.Value = C_DEFAULT_YMD
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

                OIM0015row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                '更新ジャーナル出力
                JPARA00.Value = OIM0015row("DELFLG")
                JPARA01.Value = OIM0015row("CONSIGNEECODE")
                JPARA02.Value = OIM0015row("SHIPPERSCODE")
                JPARA03.Value = OIM0015row("OILCODE")

                Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                    If IsNothing(OIM0015UPDtbl) Then
                        OIM0015UPDtbl = New DataTable

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            OIM0015UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    End If

                    OIM0015UPDtbl.Clear()
                    OIM0015UPDtbl.Load(SQLdr)
                End Using

                For Each OIM0015UPDrow As DataRow In OIM0015UPDtbl.Rows
                    CS0020JOURNAL.TABLENM = "OIM0015L"
                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                    CS0020JOURNAL.ROW = OIM0015UPDrow
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
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0015L UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0015L UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

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
        DetailBoxToOIM0015INPtbl(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ERR_SW) Then
            OIM0015tbl_UPD()
            '入力レコードに変更がない場合は、メッセージダイアログを表示して処理打ち切り
            If C_MESSAGE_NO.NO_CHANGE_UPDATE.Equals(WW_ERRCODE) Then
                Master.Output(C_MESSAGE_NO.NO_CHANGE_UPDATE, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
                Exit Sub
            End If
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIM0015tbl, work.WF_SEL_INPTBL.Text)

        '○ メッセージ表示
        If WW_ERR_SW = "" Then
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)
        Else
            If isNormal(WW_ERR_SW) Then
                Master.Output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.INF)

            ElseIf WW_ERR_SW = C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR Then
                Master.Output(WW_ERR_SW, C_MESSAGE_TYPE.ERR, "荷受人コード、荷主コード、油種コード", needsPopUp:=True)

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
    Protected Sub DetailBoxToOIM0015INPtbl(ByRef O_RTN As String)

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

        Master.CreateEmptyTable(OIM0015INPtbl, work.WF_SEL_INPTBL.Text)
        Dim OIM0015INProw As DataRow = OIM0015INPtbl.NewRow

        '○ 初期クリア
        For Each OIM0015INPcol As DataColumn In OIM0015INPtbl.Columns
            If IsDBNull(OIM0015INProw.Item(OIM0015INPcol)) OrElse IsNothing(OIM0015INProw.Item(OIM0015INPcol)) Then
                Select Case OIM0015INPcol.ColumnName
                    Case "LINECNT"
                        OIM0015INProw.Item(OIM0015INPcol) = 0
                    Case "OPERATION"
                        OIM0015INProw.Item(OIM0015INPcol) = C_LIST_OPERATION_CODE.NODATA
                    Case "UPDTIMSTP"
                        OIM0015INProw.Item(OIM0015INPcol) = 0
                    Case "SELECT"
                        OIM0015INProw.Item(OIM0015INPcol) = 1
                    Case "HIDDEN"
                        OIM0015INProw.Item(OIM0015INPcol) = 0
                    Case Else
                        OIM0015INProw.Item(OIM0015INPcol) = ""
                End Select
            End If
        Next

        'LINECNT
        If WF_Sel_LINECNT.Text = "" Then
            OIM0015INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(WF_Sel_LINECNT.Text, OIM0015INProw("LINECNT"))
            Catch ex As Exception
                OIM0015INProw("LINECNT") = 0
            End Try
        End If

        OIM0015INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        OIM0015INProw("UPDTIMSTP") = 0
        OIM0015INProw("SELECT") = 1
        OIM0015INProw("HIDDEN") = 0

        OIM0015INProw("CONSIGNEECODE") = WF_CONSIGNEECODE.Text              '荷受人コード
        OIM0015INProw("SHIPPERSCODE") = WF_SHIPPERSCODE.Text                '荷主コード
        OIM0015INProw("FROMMD") = WF_FROMMD.Text                            '開始月日
        OIM0015INProw("TOMD") = WF_TOMD.Text                                '終了月日
        OIM0015INProw("OILCODE") = WF_OILCODE.Text                          '油種コード
        OIM0015INProw("TANKCAP") = WF_TANKCAP.Text                          'タンク容量
        OIM0015INProw("TARGETCAPRATE") = WF_TARGETCAPRATE.Text              '目標在庫率
        OIM0015INProw("DS") = WF_DS.Text                                    'Ｄ／Ｓ
        OIM0015INProw("DELFLG") = WF_DELFLG.Text                            '削除フラグ

        '○ チェック用テーブルに登録する
        OIM0015INPtbl.Rows.Add(OIM0015INProw)

    End Sub

    ''' <summary>
    ''' 詳細画面-クリアボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_CLEAR_Click()

        '○ DetailBoxをINPtblへ退避
        DetailBoxToOIM0015INPtbl(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        Dim inputChangeFlg As Boolean = True
        Dim OIM0015INProw As DataRow = OIM0015INPtbl.Rows(0)

        ' 既存レコードとの比較
        For Each OIM0015row As DataRow In OIM0015tbl.Rows
            ' KEY項目が等しい時
            If OIM0015row("CONSIGNEECODE") = OIM0015INProw("CONSIGNEECODE") AndAlso
                OIM0015row("SHIPPERSCODE") = OIM0015INProw("SHIPPERSCODE") AndAlso
                OIM0015row("OILCODE") = OIM0015INProw("OILCODE") Then
                ' KEY項目以外の項目の差異チェック
                If OIM0015row("FROMMD") = OIM0015INProw("FROMMD") AndAlso
                    OIM0015row("TOMD") = OIM0015INProw("TOMD") AndAlso
                    OIM0015row("TANKCAP") = OIM0015INProw("TANKCAP") AndAlso
                    OIM0015row("TARGETCAPRATE") = OIM0015INProw("TARGETCAPRATE") AndAlso
                    OIM0015row("DS") = OIM0015INProw("DS") AndAlso
                    OIM0015row("DELFLG") = OIM0015INProw("DELFLG") Then
                    ' 変更がないときは、入力変更フラグをOFFにする
                    inputChangeFlg = False
                End If

                Exit For

            End If
        Next

        If inputChangeFlg Then
            '変更がある場合は、確認ダイアログを表示
            Master.Output(C_MESSAGE_NO.UPDATE_CANCEL_CONFIRM, C_MESSAGE_TYPE.QUES, I_PARA02:="W",
                needsPopUp:=True, messageBoxTitle:="確認", IsConfirm:=True, YesButtonId:="btnClearConfirmOk")
        Else
            '変更がない場合は、確認ダイアログを表示せずに、前画面に戻る
            WF_CLEAR_ConfirmOkClick()
        End If

    End Sub

    ''' <summary>
    ''' 詳細画面-クリアボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_CLEAR_ConfirmOkClick()

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
        For Each OIM0015row As DataRow In OIM0015tbl.Rows
            Select Case OIM0015row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0015row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0015row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0015row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0015row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0015row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIM0015tbl, work.WF_SEL_INPTBL.Text)

        WF_Sel_LINECNT.Text = ""        'LINECNT

        WF_CONSIGNEECODE.Text = ""      '荷受人コード
        WF_SHIPPERSCODE.Text = ""       '荷主コード
        WF_FROMMD.Text = ""             '開始月日
        WF_TOMD.Text = ""               '終了月日
        WF_OILCODE.Text = ""            '油種コード
        WF_TANKCAP.Text = ""            'タンク容量
        WF_TARGETCAPRATE.Text = ""      '目標在庫率
        WF_DS.Text = ""                 'Ｄ／Ｓ
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

                    Case WF_CONSIGNEECODE.ID
                        '荷受人コード
                        prmData.Item(C_PARAMETERS.LP_COMPANY) = Master.USERCAMP

                    Case WF_SHIPPERSCODE.ID
                        '荷主コード
                        prmData.Item(C_PARAMETERS.LP_COMPANY) = Master.USERCAMP

                    Case WF_OILCODE.ID
                        '油種コード
                        prmData = work.CreateFIXParam(Master.USERCAMP, "OILCODE")

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
            Case WF_CONSIGNEECODE.ID
                '荷受人コード
                CODENAME_get("CONSIGNEECODE", WF_CONSIGNEECODE.Text, WF_CONSIGNEECODE_TEXT.Text, WW_RTN_SW)
            Case WF_SHIPPERSCODE.ID
                '荷主コード
                CODENAME_get("SHIPPERSCODE", WF_SHIPPERSCODE.Text, WF_SHIPPERSCODE_TEXT.Text, WW_RTN_SW)
            Case WF_OILCODE.ID
                '油種コード
                CODENAME_get("OILCODE", WF_OILCODE.Text, WF_OILCODE_TEXT.Text, WW_RTN_SW)
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

                Case WF_CONSIGNEECODE.ID
                    '荷受人コード
                    WF_CONSIGNEECODE.Text = WW_SelectValue
                    WF_CONSIGNEECODE_TEXT.Text = WW_SelectText
                    WF_CONSIGNEECODE.Focus()

                Case WF_SHIPPERSCODE.ID
                    '荷主コード
                    WF_SHIPPERSCODE.Text = WW_SelectValue
                    WF_SHIPPERSCODE_TEXT.Text = WW_SelectText
                    WF_SHIPPERSCODE.Focus()

                Case WF_OILCODE.ID
                    '油種コード
                    WF_OILCODE.Text = WW_SelectValue
                    WF_OILCODE_TEXT.Text = WW_SelectText
                    WF_OILCODE.Focus()

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

                Case WF_CONSIGNEECODE.ID
                    '荷受人コード
                    WF_CONSIGNEECODE.Focus()

                Case WF_SHIPPERSCODE.ID
                    '荷主コード
                    WF_SHIPPERSCODE.Focus()

                Case WF_OILCODE.ID
                    '油種コード
                    WF_OILCODE.Focus()

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
        For Each OIM0015INProw As DataRow In OIM0015INPtbl.Rows

            WW_LINE_ERR = ""

            '削除フラグ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "DELFLG", OIM0015INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("DELFLG", OIM0015INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0015INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0015INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '荷受人コード(バリデーションチェック)
            WW_TEXT = OIM0015INProw("CONSIGNEECODE")
            Master.CheckField(Master.USERCAMP, "CONSIGNEECODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("CONSIGNEECODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(荷受人コード入力エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0015INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(荷受人コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0015INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '荷主コード(バリデーションチェック)
            WW_TEXT = OIM0015INProw("SHIPPERSCODE")
            Master.CheckField(Master.USERCAMP, "SHIPPERSCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("SHIPPERSCODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(荷主コード入力エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0015INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(荷主コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0015INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '開始月日(バリデーションチェック)
            WW_TEXT = OIM0015INProw("FROMMD")
            Master.CheckField(Master.USERCAMP, "FROMMD", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '月日チェック
                If Not String.IsNullOrEmpty(WW_TEXT) Then
                    '月日チェック
                    WW_CheckMD(WW_TEXT, "開始月日", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(開始月日入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0015INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(開始月日入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0015INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '終了月日(バリデーションチェック)
            WW_TEXT = OIM0015INProw("TOMD")
            Master.CheckField(Master.USERCAMP, "TOMD", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '月日チェック
                If Not String.IsNullOrEmpty(WW_TEXT) Then
                    '月日チェック
                    WW_CheckMD(WW_TEXT, "終了月日", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(終了月日入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0015INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(終了月日入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0015INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種コード(バリデーションチェック)
            WW_TEXT = OIM0015INProw("OILCODE")
            Master.CheckField(Master.USERCAMP, "OILCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("OILCODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(油種コード入力エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0015INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(油種コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0015INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'タンク容量(バリデーションチェック)
            WW_TEXT = OIM0015INProw("TANKCAP")
            Master.CheckField(Master.USERCAMP, "TANKCAP", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(タンク容量入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0015INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '目標在庫率(バリデーションチェック)
            WW_TEXT = OIM0015INProw("TARGETCAPRATE")
            Master.CheckField(Master.USERCAMP, "TARGETCAPRATE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(目標在庫率入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0015INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'Ｄ／Ｓ(バリデーションチェック)
            WW_TEXT = OIM0015INProw("DS")
            Master.CheckField(Master.USERCAMP, "DS", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(Ｄ／Ｓ入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0015INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '一意制約チェック
            '同一レコードの更新の場合、チェック対象外
            If OIM0015INProw("CONSIGNEECODE") = work.WF_SEL_CONSIGNEECODE2.Text AndAlso
                OIM0015INProw("SHIPPERSCODE") = work.WF_SEL_SHIPPERSCODE2.Text AndAlso
                OIM0015INProw("OILCODE") = work.WF_SEL_OILCODE2.Text Then

            Else
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    'DataBase接続
                    SQLcon.Open()

                    '一意制約チェック
                    UniqueKeyCheck(SQLcon, WW_UniqueKeyCHECK)
                End Using

                If Not isNormal(WW_UniqueKeyCHECK) Then
                    WW_CheckMES1 = "一意制約違反（荷受人コード、荷主コード、油種コード）。"
                    WW_CheckMES2 = C_MESSAGE_NO.OVERLAP_DATA_ERROR &
                                       "([" & OIM0015INProw("CONSIGNEECODE") & "]"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0015INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR
                End If
            End If


            If WW_LINE_ERR = "" Then
                If OIM0015INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    OIM0015INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LINE_ERR = CONST_PATTERNERR Then
                    '関連チェックエラーをセット
                    OIM0015INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    '単項目チェックエラーをセット
                    OIM0015INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                End If
            End If
        Next

    End Sub

    ''' <summary>
    ''' 月日チェック
    ''' </summary>
    ''' <param name="I_MD"></param>
    ''' <param name="I_MDNAME"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckMD(ByVal I_MD As String, ByVal I_MDNAME As String, ByVal I_VALUE As String, ByRef mdErrFlag As String)

        mdErrFlag = "1"
        Try
            '月取得
            Dim getMonth As String = I_MD.Remove(I_MD.IndexOf("/"))
            '日取得
            Dim getDay As String = I_MD.Remove(0, I_MD.IndexOf("/") + 1)

            '月と日の範囲チェック
            If getMonth >= 13 OrElse getDay >= 32 Then
                Master.Output(C_MESSAGE_NO.OIL_MONTH_DAY_OVER_ERROR, C_MESSAGE_TYPE.ERR, I_MDNAME, needsPopUp:=True)
            Else
                'エラーなし
                mdErrFlag = "0"
            End If
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DATE_FORMAT_ERROR, C_MESSAGE_TYPE.ERR, I_MDNAME, needsPopUp:=True)
        End Try

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="OIM0015row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIM0015row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIM0015row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 荷受人コード =" & OIM0015row("CONSIGNEECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 荷主コード =" & OIM0015row("SHIPPERSCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 開始月日 =" & OIM0015row("FROMMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 終了月日 =" & OIM0015row("TOMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種コード =" & OIM0015row("OILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> タンク容量 =" & OIM0015row("TANKCAP") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 目標在庫率 =" & OIM0015row("TARGETCAPRATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> Ｄ／Ｓ =" & OIM0015row("DS") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除フラグ =" & OIM0015row("DELFLG")
        End If

        rightview.AddErrorReport(WW_ERR_MES)

    End Sub

    ''' <summary>
    ''' OIM0015tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub OIM0015tbl_UPD()

        '○ 画面状態設定
        For Each OIM0015row As DataRow In OIM0015tbl.Rows
            Select Case OIM0015row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0015row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0015row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0015row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0015row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0015row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each OIM0015INProw As DataRow In OIM0015INPtbl.Rows

            'エラーレコード読み飛ばし
            If OIM0015INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            OIM0015INProw.Item("OPERATION") = CONST_INSERT

            ' 既存レコードとの比較
            For Each OIM0015row As DataRow In OIM0015tbl.Rows
                ' KEY項目が等しい時
                If OIM0015row("CONSIGNEECODE") = OIM0015INProw("CONSIGNEECODE") AndAlso
                    OIM0015row("SHIPPERSCODE") = OIM0015INProw("SHIPPERSCODE") AndAlso
                    OIM0015row("OILCODE") = OIM0015INProw("OILCODE") Then
                    ' KEY項目以外の項目の差異チェック
                    If OIM0015row("FROMMD") = OIM0015INProw("FROMMD") AndAlso
                        OIM0015row("TOMD") = OIM0015INProw("TOMD") AndAlso
                        OIM0015row("TANKCAP") = OIM0015INProw("TANKCAP") AndAlso
                        OIM0015row("TARGETCAPRATE") = OIM0015INProw("TARGETCAPRATE") AndAlso
                        OIM0015row("DS") = OIM0015INProw("DS") AndAlso
                        OIM0015row("DELFLG") = OIM0015INProw("DELFLG") AndAlso
                        Not C_LIST_OPERATION_CODE.UPDATING.Equals(OIM0015row("OPERATION")) Then
                        ' 変更がないときは「操作」の項目は空白にする
                        OIM0015INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Else
                        ' 変更がある時は「操作」の項目を「更新」に設定する
                        OIM0015INProw("OPERATION") = CONST_UPDATE
                    End If

                    Exit For

                End If
            Next
        Next

        '更新チェック
        If C_LIST_OPERATION_CODE.NODATA.Equals(OIM0015INPtbl.Rows(0)("OPERATION")) Then
            '更新なしの場合、エラーコードに変更なしエラーをセットして処理打ち切り
            WW_ERRCODE = C_MESSAGE_NO.NO_CHANGE_UPDATE
            Exit Sub
        ElseIf CONST_UPDATE.Equals(OIM0015INPtbl.Rows(0)("OPERATION")) OrElse
            CONST_INSERT.Equals(OIM0015INPtbl.Rows(0)("OPERATION")) Then
            '追加/更新の場合、DB更新処理
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                'DataBase接続
                SQLcon.Open()

                'マスタ更新
                UpdateMaster(SQLcon)

                work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = "Update Success!!"
            End Using
        End If

        '○ 変更有無判定　&　入力値反映
        For Each OIM0015INProw As DataRow In OIM0015INPtbl.Rows
            '発見フラグ
            Dim isFound As Boolean = False

            For Each OIM0015row As DataRow In OIM0015tbl.Rows

                '同一レコードか判定
                If OIM0015INProw("CONSIGNEECODE") = OIM0015row("CONSIGNEECODE") AndAlso
                    OIM0015INProw("SHIPPERSCODE") = OIM0015row("SHIPPERSCODE") AndAlso
                    OIM0015INProw("OILCODE") = OIM0015row("OILCODE") Then
                    '画面入力テーブル項目設定
                    OIM0015INProw("LINECNT") = OIM0015row("LINECNT")
                    OIM0015INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    OIM0015INProw("UPDTIMSTP") = OIM0015row("UPDTIMSTP")
                    OIM0015INProw("SELECT") = 0
                    OIM0015INProw("HIDDEN") = 0

                    '項目テーブル項目設定
                    OIM0015row.ItemArray = OIM0015INProw.ItemArray

                    '〇名称設定
                    '荷受人
                    CODENAME_get("CONSIGNEECODE", OIM0015row("CONSIGNEECODE"), OIM0015row("CONSIGNEENAME"), WW_DUMMY)
                    '荷主
                    CODENAME_get("SHIPPERSCODE", OIM0015row("SHIPPERSCODE"), OIM0015row("SHIPPERSNAME"), WW_DUMMY)
                    '油種
                    CODENAME_get("OILCODE", OIM0015row("OILCODE"), OIM0015row("OILNAME"), WW_DUMMY)

                    '発見フラグON
                    isFound = True
                    Exit For
                End If
            Next

            '同一レコードが発見できない場合は、追加する
            If Not isFound Then
                Dim nrow = OIM0015tbl.NewRow
                nrow.ItemArray = OIM0015INProw.ItemArray

                '画面入力テーブル項目設定
                nrow("LINECNT") = OIM0015tbl.Rows.Count + 1
                nrow("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                nrow("UPDTIMSTP") = "0"
                nrow("SELECT") = 0
                nrow("HIDDEN") = 0

                '〇名称設定
                '荷受人
                CODENAME_get("CONSIGNEECODE", nrow("CONSIGNEECODE"), nrow("CONSIGNEENAME"), WW_DUMMY)
                '荷主
                CODENAME_get("SHIPPERSCODE", nrow("SHIPPERSCODE"), nrow("SHIPPERSNAME"), WW_DUMMY)
                '油種
                CODENAME_get("OILCODE", nrow("OILCODE"), nrow("OILNAME"), WW_DUMMY)

                OIM0015tbl.Rows.Add(nrow)
            End If
        Next

    End Sub

#Region "未使用"
    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="OIM0015INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef OIM0015INProw As DataRow)

        For Each OIM0015row As DataRow In OIM0015tbl.Rows

            '同一レコードか判定
            If OIM0015INProw("CONSIGNEECODE") = OIM0015row("CONSIGNEECODE") AndAlso
                OIM0015INProw("SHIPPERSCODE") = OIM0015row("SHIPPERSCODE") AndAlso
                OIM0015INProw("OILCODE") = OIM0015row("OILCODE") Then
                '画面入力テーブル項目設定
                OIM0015INProw("LINECNT") = OIM0015row("LINECNT")
                OIM0015INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                OIM0015INProw("UPDTIMSTP") = OIM0015row("UPDTIMSTP")
                OIM0015INProw("SELECT") = 1
                OIM0015INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0015row.ItemArray = OIM0015INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 追加予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0015INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef OIM0015INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim OIM0015row As DataRow = OIM0015tbl.NewRow
        OIM0015row.ItemArray = OIM0015INProw.ItemArray

        OIM0015row("LINECNT") = OIM0015tbl.Rows.Count + 1
        If OIM0015INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
            OIM0015row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        Else
            OIM0015row("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
        End If

        OIM0015row("UPDTIMSTP") = "0"
        OIM0015row("SELECT") = 1
        OIM0015row("HIDDEN") = 0

        OIM0015tbl.Rows.Add(OIM0015row)

    End Sub

    ''' <summary>
    ''' エラーデータの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0015INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_ERR_SUB(ByRef OIM0015INProw As DataRow)

        For Each OIM0015row As DataRow In OIM0015tbl.Rows

            '同一レコードか判定
            If OIM0015INProw("CONSIGNEECODE") = OIM0015row("CONSIGNEECODE") AndAlso
                OIM0015INProw("SHIPPERSCODE") = OIM0015row("SHIPPERSCODE") AndAlso
                OIM0015INProw("OILCODE") = OIM0015row("OILCODE") Then
                '画面入力テーブル項目設定
                OIM0015INProw("LINECNT") = OIM0015row("LINECNT")
                OIM0015INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                OIM0015INProw("UPDTIMSTP") = OIM0015row("UPDTIMSTP")
                OIM0015INProw("SELECT") = 1
                OIM0015INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0015row.ItemArray = OIM0015INProw.ItemArray
                Exit For
            End If
        Next

    End Sub
#End Region

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
                Case "CONSIGNEECODE"
                    '荷受人コード
                    prmData.Item(C_PARAMETERS.LP_COMPANY) = Master.USERCAMP
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CONSIGNEELIST, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "SHIPPERSCODE"
                    '荷主コード
                    prmData.Item(C_PARAMETERS.LP_COMPANY) = Master.USERCAMP
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_JOINTLIST, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "OILCODE"
                    '油種コード
                    prmData = work.CreateFIXParam(Master.USERCAMP, "OILCODE")
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
