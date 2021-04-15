''************************************************************
' 貨物駅マスタメンテ登録画面
' 作成日 2019/10/29
' 更新日 2021/04/15
' 作成者 JOT森川
' 更新者 JOT伊草
'
' 修正履歴:2019/10/29 新規作成
'         :2021/04/09 1)表更新→更新、クリア→戻る、に名称変更
'                     2)戻るボタン押下時、確認ダイアログ表示→
'                       確認ダイアログでOK押下時、一覧画面に戻るように修正
'                     3)更新ボタン押下時、この画面でDB更新→
'                       一覧画面の表示データに更新後の内容反映して戻るように修正
'         :2021/04/15 新規登録を行った際に、一覧画面に新規登録データが追加されないバグに対応
''************************************************************
Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' 貨物駅マスタ登録（登録）
''' </summary>
''' <remarks></remarks>
Public Class OIM0004StationCreate
    Inherits Page

    '○ 検索結果格納Table
    Private OIM0004tbl As DataTable                                 '一覧格納用テーブル
    Private OIM0004INPtbl As DataTable                              'チェック用テーブル
    Private OIM0004UPDtbl As DataTable                              '更新用テーブル

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
                    Master.RecoverTable(OIM0004tbl, work.WF_SEL_INPTBL.Text)

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
            If Not IsNothing(OIM0004tbl) Then
                OIM0004tbl.Clear()
                OIM0004tbl.Dispose()
                OIM0004tbl = Nothing
            End If

            If Not IsNothing(OIM0004INPtbl) Then
                OIM0004INPtbl.Clear()
                OIM0004INPtbl.Dispose()
                OIM0004INPtbl = Nothing
            End If

            If Not IsNothing(OIM0004UPDtbl) Then
                OIM0004UPDtbl.Clear()
                OIM0004UPDtbl.Dispose()
                OIM0004UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIM0004WRKINC.MAPIDC
        '○HELP表示有無設定
        Master.dispHelp = False
        '○D&D有無設定
        Master.eventDrop = True

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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0004L Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        End If

        '貨物駅コード・貨物コード枝番・発着駅フラグ・削除フラグを入力するテキストボックスは数値(0～9)のみ可能とする。
        Me.TxtStationCode.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtBranch.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtDepArrStation.Attributes("onkeyPress") = "CheckNum()"
        Me.WF_DELFLG.Attributes("onkeyPress") = "CheckNum()"

        '選択行
        WF_Sel_LINECNT.Text = work.WF_SEL_LINECNT.Text

        '貨物車コード
        TxtStationCode.Text = work.WF_SEL_STATIONCODE2.Text

        '貨物コード枝番
        TxtBranch.Text = work.WF_SEL_BRANCH2.Text

        '貨物駅名称
        TxtStationName.Text = work.WF_SEL_STATONNAME.Text

        '貨物駅名称カナ
        TxtStationNameKana.Text = work.WF_SEL_STATIONNAMEKANA.Text

        '貨物駅種別名称
        TxtTypeName.Text = work.WF_SEL_TYPENAME.Text

        '貨物駅種別名称カナ
        TxtTypeNameKana.Text = work.WF_SEL_TYPENAMEKANA.Text

        '発着駅フラグ
        TxtDepArrStation.Text = work.WF_SEL_DEPARRSTATIONFLG2.Text
        CODENAME_get("DEPARRSTATIONFLG", TxtDepArrStation.Text, LblDepArrStationName.Text, WW_DUMMY)

        '削除
        WF_DELFLG.Text = work.WF_SEL_DELFLG.Text
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
            & "    STATIONCODE" _
            & "    , BRANCH" _
            & " FROM" _
            & "    OIL.OIM0004_STATION" _
            & " WHERE" _
            & "        STATIONCODE      = @P1" _
            & "    AND DELFLG           <> @P2"

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '貨物コード枝番
        If Not String.IsNullOrEmpty(TxtBranch.Text) Then
            SQLStr &= String.Format("    AND BRANCH = '{0}'", TxtBranch.Text)
        Else
            SQLStr &= String.Format("    AND BRANCH = '{0}'", "")
        End If

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 4)            '貨物駅コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 1)            '削除フラグ

                PARA1.Value = TxtStationCode.Text
                PARA2.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    Dim OIM0004Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIM0004Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIM0004Chk.Load(SQLdr)

                    If OIM0004Chk.Rows.Count > 0 Then
                        '重複データエラー
                        O_MESSAGENO = Messages.C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR
                    Else
                        '正常終了時
                        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0004C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0004C UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 貨物駅マスタ登録更新
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
            & "        OIL.OIM0004_STATION" _
            & "    WHERE" _
            & "        STATIONCODE      = @P1" _
            & "        AND BRANCH       = @P2 ;" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE OIL.OIM0004_STATION" _
            & "    SET" _
            & "        STATONNAME         = @P3  , STATIONNAMEKANA = @P4" _
            & "        , TYPENAME         = @P5  , TYPENAMEKANA    = @P6" _
            & "        , DEPARRSTATIONFLG = @P15 , DELFLG          = @P7" _
            & "        , UPDYMD           = @P11 , UPDUSER         = @P12 , UPDTERMID = @P13" _
            & "        , RECEIVEYMD       = @P14" _
            & "    WHERE" _
            & "        STATIONCODE       = @P1" _
            & "        AND BRANCH       = @P2 ;" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO OIL.OIM0004_STATION" _
            & "        ( STATIONCODE, BRANCH       , STATONNAME       , STATIONNAMEKANA" _
            & "        , TYPENAME   , TYPENAMEKANA , DEPARRSTATIONFLG , DELFLG" _
            & "        , INITYMD    , INITUSER     , INITTERMID" _
            & "        , UPDYMD     , UPDUSER      , UPDTERMID" _
            & "        , RECEIVEYMD)" _
            & "    VALUES" _
            & "        ( @P1  , @P2 , @P3  , @P4" _
            & "        , @P5  , @P6 , @P15 , @P7" _
            & "        , @P8  , @P9 , @P10" _
            & "        , @P11 , @P12, @P13" _
            & "        , @P14) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
              " SELECT" _
            & "    STATIONCODE" _
            & "    , BRANCH" _
            & "    , STATONNAME" _
            & "    , STATIONNAMEKANA" _
            & "    , TYPENAME" _
            & "    , TYPENAMEKANA" _
            & "    , DEPARRSTATIONFLG" _
            & "    , DELFLG" _
            & "    , INITYMD" _
            & "    , INITUSER" _
            & "    , INITTERMID" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & "    , CAST(UPDTIMSTP AS bigint) AS TIMSTP" _
            & " FROM" _
            & "    OIL.OIM0004_STATION" _
            & " WHERE" _
            & "        STATIONCODE      = @P1" _
            & "        AND BRANCH       = @P2"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 4)            '貨物駅コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 3)            '貨物コード枝番
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 200)          '貨物駅名称
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 100)          '貨物駅名称カナ
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 40)           '貨物駅種別名称
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 20)           '貨物駅種別名称カナ
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 1)          '発着駅フラグ
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.NVarChar, 1)            '削除フラグ
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.DateTime)               '登録年月日
                Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", SqlDbType.NVarChar, 20)           '登録ユーザーID
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 20)         '登録端末
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.DateTime)             '更新年月日
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 20)         '更新ユーザーID
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 20)         '更新端末
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.DateTime)             '集信日時

                Dim JPARA1 As SqlParameter = SQLcmdJnl.Parameters.Add("@P1", SqlDbType.NVarChar, 4)        '貨物駅コード
                Dim JPARA2 As SqlParameter = SQLcmdJnl.Parameters.Add("@P2", SqlDbType.NVarChar, 3)        '貨物コード枝番

                Dim OIM0004row As DataRow = OIM0004INPtbl.Rows(0)

                Dim WW_DATENOW As DateTime = Date.Now

                'DB更新
                PARA1.Value = OIM0004row("STATIONCODE")
                PARA2.Value = OIM0004row("BRANCH")
                PARA3.Value = OIM0004row("STATONNAME")
                PARA4.Value = OIM0004row("STATIONNAMEKANA")
                PARA5.Value = OIM0004row("TYPENAME")
                PARA6.Value = OIM0004row("TYPENAMEKANA")
                PARA15.Value = OIM0004row("DEPARRSTATIONFLG")
                PARA7.Value = OIM0004row("DELFLG")
                PARA8.Value = WW_DATENOW
                PARA9.Value = Master.USERID
                PARA10.Value = Master.USERTERMID
                PARA11.Value = WW_DATENOW
                PARA12.Value = Master.USERID
                PARA13.Value = Master.USERTERMID
                PARA14.Value = C_DEFAULT_YMD

                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

                OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                '更新ジャーナル出力
                JPARA1.Value = OIM0004row("STATIONCODE")
                JPARA2.Value = OIM0004row("BRANCH")

                Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                    If IsNothing(OIM0004UPDtbl) Then
                        OIM0004UPDtbl = New DataTable

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            OIM0004UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    End If

                    OIM0004UPDtbl.Clear()
                    OIM0004UPDtbl.Load(SQLdr)
                End Using

                For Each OIM0004UPDrow As DataRow In OIM0004UPDtbl.Rows
                    CS0020JOURNAL.TABLENM = "OIM0004L"
                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                    CS0020JOURNAL.ROW = OIM0004UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0004L UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0004L UPDATE_INSERT"
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
        DetailBoxToOIM0004INPtbl(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ERR_SW) Then
            OIM0004tbl_UPD()
            '入力レコードに変更がない場合は、メッセージダイアログを表示して処理打ち切り
            If C_MESSAGE_NO.NO_CHANGE_UPDATE.Equals(WW_ERRCODE) Then
                Master.Output(C_MESSAGE_NO.NO_CHANGE_UPDATE, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
                Exit Sub
            End If
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIM0004tbl, work.WF_SEL_INPTBL.Text)

        '○ メッセージ表示
        If WW_ERR_SW = "" Then
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)
        Else
            If isNormal(WW_ERR_SW) Then
                Master.Output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.INF)

            ElseIf WW_ERR_SW = C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR Then
                Master.Output(WW_ERR_SW, C_MESSAGE_TYPE.ERR, "貨物駅コード", needsPopUp:=True)

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
    Protected Sub DetailBoxToOIM0004INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(WF_DELFLG.Text)            '削除

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

        Master.CreateEmptyTable(OIM0004INPtbl, work.WF_SEL_INPTBL.Text)
        Dim OIM0004INProw As DataRow = OIM0004INPtbl.NewRow

        '○ 初期クリア
        For Each OIM0004INPcol As DataColumn In OIM0004INPtbl.Columns
            If IsDBNull(OIM0004INProw.Item(OIM0004INPcol)) OrElse IsNothing(OIM0004INProw.Item(OIM0004INPcol)) Then
                Select Case OIM0004INPcol.ColumnName
                    Case "LINECNT"
                        OIM0004INProw.Item(OIM0004INPcol) = 0
                    Case "OPERATION"
                        OIM0004INProw.Item(OIM0004INPcol) = C_LIST_OPERATION_CODE.NODATA
                    Case "TIMSTP"
                        OIM0004INProw.Item(OIM0004INPcol) = 0
                    Case "SELECT"
                        OIM0004INProw.Item(OIM0004INPcol) = 1
                    Case "HIDDEN"
                        OIM0004INProw.Item(OIM0004INPcol) = 0
                    Case Else
                        OIM0004INProw.Item(OIM0004INPcol) = ""
                End Select
            End If
        Next

        'LINECNT
        If WF_Sel_LINECNT.Text = "" Then
            OIM0004INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(WF_Sel_LINECNT.Text, OIM0004INProw("LINECNT"))
            Catch ex As Exception
                OIM0004INProw("LINECNT") = 0
            End Try
        End If

        OIM0004INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        OIM0004INProw("TIMSTP") = 0
        OIM0004INProw("SELECT") = 1
        OIM0004INProw("HIDDEN") = 0

        'OIM0004INProw("CAMPCODE") = work.WF_SEL_CAMPCODE.Text        '会社コード
        'OIM0004INProw("UORG") = work.WF_SEL_UORG.Text                '運用部署

        OIM0004INProw("DELFLG") = Me.WF_DELFLG.Text                     '削除

        OIM0004INProw("STATIONCODE") = Me.TxtStationCode.Text           '貨物駅コード
        OIM0004INProw("BRANCH") = Me.TxtBranch.Text                     '貨物コード枝番
        OIM0004INProw("STATONNAME") = Me.TxtStationName.Text            '貨物駅名称
        OIM0004INProw("STATIONNAMEKANA") = Me.TxtStationNameKana.Text   '貨物駅名称カナ
        OIM0004INProw("TypeName") = Me.TxtTypeName.Text                 '貨物駅種別名称
        OIM0004INProw("TYPENAMEKANA") = Me.TxtTypeNameKana.Text         '貨物駅種別名称カナ
        OIM0004INProw("DEPARRSTATIONFLG") = Me.TxtDepArrStation.Text    '発着駅フラグ

        '○ 名称取得
        '発着駅フラグ名
        CODENAME_get("DEPARRSTATIONFLG", OIM0004INProw("DEPARRSTATIONFLG"), OIM0004INProw("DEPARRSTATIONNAME"), WW_DUMMY)

        '○ チェック用テーブルに登録する
        OIM0004INPtbl.Rows.Add(OIM0004INProw)

    End Sub

    ''' <summary>
    ''' 詳細画面-クリアボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_CLEAR_Click()

        '○ DetailBoxをINPtblへ退避
        DetailBoxToOIM0004INPtbl(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        Dim inputChangeFlg As Boolean = True
        Dim OIM0004INProw As DataRow = OIM0004INPtbl.Rows(0)

        ' 既存レコードとの比較
        For Each OIM0004row As DataRow In OIM0004tbl.Rows
            ' KEY項目が等しい時
            If OIM0004row("STATIONCODE") = OIM0004INProw("STATIONCODE") AndAlso
                OIM0004row("BRANCH") = OIM0004INProw("BRANCH") Then
                ' KEY項目以外の項目の差異をチェック
                If OIM0004row("STATONNAME") = OIM0004INProw("STATONNAME") AndAlso
                    OIM0004row("STATIONNAMEKANA") = OIM0004INProw("STATIONNAMEKANA") AndAlso
                    OIM0004row("TYPENAME") = OIM0004INProw("TYPENAME") AndAlso
                    OIM0004row("TYPENAMEKANA") = OIM0004INProw("TYPENAMEKANA") AndAlso
                    OIM0004row("DEPARRSTATIONFLG") = OIM0004INProw("DEPARRSTATIONFLG") AndAlso
                    OIM0004row("DELFLG") = OIM0004INProw("DELFLG") Then
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
        For Each OIM0004row As DataRow In OIM0004tbl.Rows
            Select Case OIM0004row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIM0004tbl, work.WF_SEL_INPTBL.Text)

        WF_Sel_LINECNT.Text = ""            'LINECNT
        TxtStationCode.Text = ""            '貨物駅コード
        TxtBranch.Text = ""                 '貨物コード枝番
        TxtStationName.Text = ""            '貨物駅名称
        TxtStationNameKana.Text = ""        '貨物駅名称カナ
        TxtTypeName.Text = ""               '貨物駅種別名称
        TxtTypeNameKana.Text = ""           '貨物駅種別名称カナ
        TxtDepArrStation.Text = ""          '発着駅フラグ
        WF_DELFLG.Text = ""                 '削除
        WF_DELFLG_TEXT.Text = ""            '削除名称

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
                    '発着駅フラグ 
                    Case "TxtDepArrStation"
                        prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, TxtDepArrStation.Text)

                    '削除フラグ   
                    Case "WF_DELFLG"
                        prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = "2"
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
            '発着駅フラグ
            Case "TxtDepArrStation"
                CODENAME_get("DEPARRSTATIONFLG", TxtDepArrStation.Text, LblDepArrStationName.Text, WW_RTN_SW)
            '削除フラグ
            Case "WF_DELFLG"
                CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_RTN_SW)
        End Select

        '○ メッセージ表示
        If isNormal(WW_RTN_SW) Then
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.NOR)
        Else
            If WF_FIELD.Value = "WF_DELFLG" Then
                Master.Output(C_MESSAGE_NO.OIL_DELFLG_NOTFOUND, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Else
                Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.ERR)
            End If
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
                '削除
                Case "WF_DELFLG"
                    WF_DELFLG.Text = WW_SelectValue
                    WF_DELFLG_TEXT.Text = WW_SelectText
                    WF_DELFLG.Focus()

                '発着駅フラグ
                Case "TxtDepArrStation"
                    TxtDepArrStation.Text = WW_SelectValue
                    LblDepArrStationName.Text = WW_SelectText
                    TxtDepArrStation.Focus()
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
                '削除
                Case "WF_DELFLG"
                    WF_DELFLG.Focus()

                '発着駅フラグ
                Case "TxtDepArrStation"
                    TxtDepArrStation.Focus()
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
        For Each OIM0004INProw As DataRow In OIM0004INPtbl.Rows

            WW_LINE_ERR = ""

            '削除フラグ(バリデーションチェック）
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DELFLG", OIM0004INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("DELFLG", OIM0004INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0004INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '貨物駅コード(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "STATIONCODE", OIM0004INProw("STATIONCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "貨物駅コード入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '貨物コード枝番(バリデーションチェック)
            '貨物コード枝番が設定されている場合のみチェック
            If Not String.IsNullOrEmpty(OIM0004INProw("BRANCH")) Then
                Master.CheckField(work.WF_SEL_CAMPCODE.Text, "BRANCH", OIM0004INProw("BRANCH"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If Not isNormal(WW_CS0024FCHECKERR) Then
                    WW_CheckMES1 = "貨物コード枝番入力エラー。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0004INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '発着駅フラグ(バリデーションチェック）
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DEPARRSTATIONFLG", OIM0004INProw("DEPARRSTATIONFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値が設定されている場合のみ、値存在チェック
                If Not String.IsNullOrEmpty(OIM0004INProw("DEPARRSTATIONFLG")) Then
                    CODENAME_get("DEPARRSTATIONFLG", OIM0004INProw("DEPARRSTATIONFLG"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(発着駅フラグエラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0004INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(発着駅フラグエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '〇一意制約チェック
            '同一レコードの更新の場合、チェック対象外
            If Not (OIM0004INProw("STATIONCODE") = work.WF_SEL_STATIONCODE2.Text AndAlso
                OIM0004INProw("BRANCH") = work.WF_SEL_BRANCH2.Text) Then

                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    'DataBase接続
                    SQLcon.Open()

                    '一意制約チェック
                    UniqueKeyCheck(SQLcon, WW_UniqueKeyCHECK)
                End Using

                If Not isNormal(WW_UniqueKeyCHECK) Then
                    WW_CheckMES1 = "一意制約違反。"
                    WW_CheckMES2 = C_MESSAGE_NO.OVERLAP_DATA_ERROR &
                                   "([" & OIM0004INProw("STATIONCODE") & "]" &
                                   " [" & OIM0004INProw("BRANCH") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0004INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR
                End If
            End If

            If WW_LINE_ERR = "" Then
                If OIM0004INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    OIM0004INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LINE_ERR = CONST_PATTERNERR Then
                    '関連チェックエラーをセット
                    OIM0004INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    '単項目チェックエラーをセット
                    OIM0004INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                End If
            End If
        Next

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="OIM0004row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIM0004row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIM0004row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 貨物駅コード       =" & OIM0004row("STATIONCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 貨物コード枝番     =" & OIM0004row("BRANCH") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 貨物駅名称         =" & OIM0004row("STATONNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 貨物駅名称カナ     =" & OIM0004row("STATIONNAMEKANA") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 貨物駅種別名称     =" & OIM0004row("TYPENAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 貨物駅種別名称カナ =" & OIM0004row("TYPENAMEKANA") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 発着駅フラグ       =" & OIM0004row("DEPARRSTATIONFLG") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除               =" & OIM0004row("DELFLG")
        End If

        rightview.AddErrorReport(WW_ERR_MES)

    End Sub

    ''' <summary>
    ''' OIM0004tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub OIM0004tbl_UPD()

        '○ 画面状態設定
        For Each OIM0004row As DataRow In OIM0004tbl.Rows
            Select Case OIM0004row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each OIM0004INProw As DataRow In OIM0004INPtbl.Rows

            'エラーレコード読み飛ばし
            If OIM0004INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            OIM0004INProw.Item("OPERATION") = CONST_INSERT

            ' 既存レコードとの比較
            For Each OIM0004row As DataRow In OIM0004tbl.Rows
                ' KEY項目が等しい時
                If OIM0004row("STATIONCODE") = OIM0004INProw("STATIONCODE") AndAlso
                    OIM0004row("BRANCH") = OIM0004INProw("BRANCH") Then
                    ' KEY項目以外の項目の差異をチェック
                    If OIM0004row("STATONNAME") = OIM0004INProw("STATONNAME") AndAlso
                        OIM0004row("STATIONNAMEKANA") = OIM0004INProw("STATIONNAMEKANA") AndAlso
                        OIM0004row("TYPENAME") = OIM0004INProw("TYPENAME") AndAlso
                        OIM0004row("TYPENAMEKANA") = OIM0004INProw("TYPENAMEKANA") AndAlso
                        OIM0004row("DEPARRSTATIONFLG") = OIM0004INProw("DEPARRSTATIONFLG") AndAlso
                        OIM0004row("DELFLG") = OIM0004INProw("DELFLG") AndAlso
                        Not C_LIST_OPERATION_CODE.UPDATING.Equals(OIM0004row("OPERATION")) Then
                        ' 変更がないときは「操作」の項目は空白にする
                        OIM0004INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Else
                        ' 変更がある時は「操作」の項目を「更新」に設定する
                        OIM0004INProw("OPERATION") = CONST_UPDATE
                    End If

                    Exit For

                End If
            Next
        Next

        '更新チェック
        If C_LIST_OPERATION_CODE.NODATA.Equals(OIM0004INPtbl.Rows(0)("OPERATION")) Then
            '更新なしの場合、エラーコードに変更なしエラーをセットして処理打ち切り
            WW_ERRCODE = C_MESSAGE_NO.NO_CHANGE_UPDATE
            Exit Sub
        ElseIf CONST_UPDATE.Equals(OIM0004INPtbl.Rows(0)("OPERATION")) OrElse
            CONST_INSERT.Equals(OIM0004INPtbl.Rows(0)("OPERATION")) Then
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
        For Each OIM0004INProw As DataRow In OIM0004INPtbl.Rows
            '発見フラグ
            Dim isFound As Boolean = False

            For Each OIM0004row As DataRow In OIM0004tbl.Rows

                '同一レコードか判定
                If OIM0004INProw("STATIONCODE") = OIM0004row("STATIONCODE") AndAlso
                    OIM0004INProw("BRANCH") = OIM0004row("BRANCH") Then
                    '画面入力テーブル項目設定
                    OIM0004INProw("LINECNT") = OIM0004row("LINECNT")
                    OIM0004INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    OIM0004INProw("TIMSTP") = OIM0004row("TIMSTP")
                    OIM0004INProw("SELECT") = 0
                    OIM0004INProw("HIDDEN") = 0

                    '項目テーブル項目設定
                    OIM0004row.ItemArray = OIM0004INProw.ItemArray

                    '発見フラグON
                    isFound = True
                    Exit For
                End If
            Next

            '同一レコードが発見できない場合は、追加する
            If Not isFound Then
                Dim nrow = OIM0004tbl.NewRow
                nrow.ItemArray = OIM0004INProw.ItemArray

                '画面入力テーブル項目設定
                nrow("LINECNT") = OIM0004tbl.Rows.Count + 1
                nrow("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                nrow("TIMSTP") = "0"
                nrow("SELECT") = 0
                nrow("HIDDEN") = 0

                OIM0004tbl.Rows.Add(nrow)
            End If
        Next

    End Sub

#Region "未使用"
    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="OIM0004INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef OIM0004INProw As DataRow)

        For Each OIM0004row As DataRow In OIM0004tbl.Rows

            '同一レコードか判定
            If OIM0004INProw("STATIONCODE") = OIM0004row("STATIONCODE") AndAlso
                OIM0004INProw("BRANCH") = OIM0004row("BRANCH") Then
                '画面入力テーブル項目設定
                OIM0004INProw("LINECNT") = OIM0004row("LINECNT")
                OIM0004INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                OIM0004INProw("TIMSTP") = OIM0004row("TIMSTP")
                OIM0004INProw("SELECT") = 1
                OIM0004INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0004row.ItemArray = OIM0004INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 追加予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0004INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef OIM0004INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim OIM0004row As DataRow = OIM0004tbl.NewRow
        OIM0004row.ItemArray = OIM0004INProw.ItemArray

        OIM0004row("LINECNT") = OIM0004tbl.Rows.Count + 1
        If OIM0004INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
            OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        Else
            '            OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
        End If

        OIM0004row("TIMSTP") = "0"
        OIM0004row("SELECT") = 1
        OIM0004row("HIDDEN") = 0

        OIM0004tbl.Rows.Add(OIM0004row)

    End Sub


    ''' <summary>
    ''' エラーデータの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0004INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_ERR_SUB(ByRef OIM0004INProw As DataRow)

        For Each OIM0004row As DataRow In OIM0004tbl.Rows

            '同一レコードか判定
            If OIM0004INProw("STATIONCODE") = OIM0004row("STATIONCODE") AndAlso
               OIM0004INProw("BRANCH") = OIM0004row("BRANCH") Then
                '画面入力テーブル項目設定
                OIM0004INProw("LINECNT") = OIM0004row("LINECNT")
                OIM0004INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                OIM0004INProw("TIMSTP") = OIM0004row("TIMSTP")
                OIM0004INProw("SELECT") = 1
                OIM0004INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0004row.ItemArray = OIM0004INProw.ItemArray
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
        Dim prmData As New Hashtable

        Try
            Select Case I_FIELD
                Case "CAMPCODE"         '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "UORG"             '運用部署
                    prmData = work.CreateUORGParam(work.WF_SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "DEPARRSTATIONFLG" '発着駅フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DEPARRSTATIONLIST, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DEPARRSTATIONFLG"))

                Case "DELFLG"           '削除
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DELFLG"))

            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
