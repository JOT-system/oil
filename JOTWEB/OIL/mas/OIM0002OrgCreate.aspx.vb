﻿''************************************************************
' 組織マスタメンテ登録画面
' 作成日 2020/05/26
' 更新日 2021/04/15
' 作成者 JOT杉山
' 更新者 JOT伊草
'
' 修正履歴:2020/05/26 新規作成
'         :2021/04/09 1)表更新→更新、クリア→戻る、に名称変更
'                     2)戻るボタン押下時、確認ダイアログ表示→
'                       確認ダイアログでOK押下時、一覧画面に戻るように修正
'                     3)更新ボタン押下時、この画面でDB更新→
'                       一覧画面の表示データに更新後の内容反映して戻るように修正
'         :2021/04/15 1)新規登録を行った際に、一覧画面に新規登録データが追加されないバグに対応
'                     2)検索画面で'01'以外の会社コードを入力した場合に
'                       登録・更新画面に遷移できなくなるバグを修正
''************************************************************
Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' 組織マスタ登録（登録）
''' </summary>
''' <remarks></remarks>
Public Class OIM0002OrgCreate
    Inherits Page

    Private CS0051UserInfo As New CS0051UserInfo                    'ユーザ情報取得

    '○ 検索結果格納Table
    Private OIM0002tbl As DataTable                                 '一覧格納用テーブル
    Private OIM0002INPtbl As DataTable                              'チェック用テーブル
    Private OIM0002UPDtbl As DataTable                              '更新用テーブル

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
                    Master.RecoverTable(OIM0002tbl, work.WF_SEL_INPTBL.Text)

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
            If Not IsNothing(OIM0002tbl) Then
                OIM0002tbl.Clear()
                OIM0002tbl.Dispose()
                OIM0002tbl = Nothing
            End If

            If Not IsNothing(OIM0002INPtbl) Then
                OIM0002INPtbl.Clear()
                OIM0002INPtbl.Dispose()
                OIM0002INPtbl = Nothing
            End If

            If Not IsNothing(OIM0002UPDtbl) Then
                OIM0002UPDtbl.Clear()
                OIM0002UPDtbl.Dispose()
                OIM0002UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIM0002WRKINC.MAPIDC
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0002L Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        End If

        '会社コード、組織コード・削除フラグを入力するテキストボックスは数値(0～9)のみ可能とする。
        Me.TxtCampCode.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtOrgCode.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtDelFlg.Attributes("onkeyPress") = "CheckNum()"
        '開始年月日・終了年月日を入力するテキストボックスは数値(0～9)＋記号(/)のみ可能とする。
        Me.TxtStYmd.Attributes("onkeyPress") = "CheckCalendar()"
        Me.TxtEndYmd.Attributes("onkeyPress") = "CheckCalendar()"

        '選択行
        WF_Sel_LINECNT.Text = work.WF_SEL_LINECNT.Text

        '会社コード
        TxtCampCode.Text = work.WF_SEL_CAMPCODE_L.Text
        CODENAME_get("CAMPCODE", TxtCampCode.Text, Label2.Text, WW_RTN_SW)

        '組織コード
        TxtOrgCode.Text = work.WF_SEL_ORGCODE_L.Text

        '組織名称
        TxtOrgName.Text = work.WF_SEL_ORGNAME.Text

        '組織名称（短）
        TxtOrgNameS.Text = work.WF_SEL_ORGNAMES.Text

        '組織名称カナ
        TxtOrgNameKana.Text = work.WF_SEL_ORGNAMEKANA.Text

        '組織名称カナ（短）
        TxtOrgNameKanaS.Text = work.WF_SEL_ORGNAMEKANAS.Text

        '開始年月日
        TxtStYmd.Text = work.WF_SEL_STYMD.Text

        '終了年月日
        TxtEndYmd.Text = work.WF_SEL_ENDYMD.Text

        '削除
        TxtDelFlg.Text = work.WF_SEL_SELECT.Text
        CODENAME_get("DELFLG", TxtDelFlg.Text, Label1.Text, WW_DUMMY)

    End Sub

    ''' <summary>
    ''' 一意制約チェック
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub UniqueKeyCheck(ByVal SQLcon As SqlConnection, ByRef O_MESSAGENO As String)

        '○ 対象データ取得
        Dim SQLStr As String =
              " SELECT" _
            & "    CAMPCODE" _
            & "    , ORGCODE" _
            & " FROM" _
            & "    OIL.OIM0002_ORG" _
            & " WHERE" _
            & "        CAMPCODE      = @P1" _
            & "    AND DELFLG           <> @P2"

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '組織コード
        If Not String.IsNullOrEmpty(TxtOrgCode.Text) Then
            SQLStr &= String.Format("    AND ORGCODE = '{0}'", TxtOrgCode.Text)
        Else
            SQLStr &= String.Format("    AND ORGCODE = '{0}'", "")
        End If

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 4)            '会社コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 1)            '削除フラグ

                PARA1.Value = TxtCampCode.Text
                PARA2.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    Dim OIM0002Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIM0002Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIM0002Chk.Load(SQLdr)

                    If OIM0002Chk.Rows.Count > 0 Then
                        '重複データエラー
                        O_MESSAGENO = Messages.C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR
                    Else
                        '正常終了時
                        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0002C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0002C UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 組織マスタ登録更新
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
            & "        OIL.OIM0002_ORG" _
            & "    WHERE" _
            & "        CAMPCODE           = @P1" _
            & "        AND ORGCODE        = @P2 ;" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE OIL.OIM0002_ORG" _
            & "    SET" _
            & "          STYMD            = @P3  , ENDYMD          = @P4" _
            & "        , NAME             = @P5  , NAMES           = @P6" _
            & "        , NAMEKANA         = @P7  , NAMEKANAS       = @P8" _
            & "        , DELFLG           = @P9" _
            & "        , UPDYMD           = @P13 , UPDUSER         = @P14 , UPDTERMID = @P15" _
            & "        , RECEIVEYMD       = @P16" _
            & "    WHERE" _
            & "        CAMPCODE           = @P1" _
            & "        AND ORGCODE        = @P2 ;" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO OIL.OIM0002_ORG" _
            & "        ( CAMPCODE   , ORGCODE " _
            & "        , STYMD      , ENDYMD       , NAME          , NAMES" _
            & "        , NAMEKANA   , NAMEKANAS    ,  DELFLG" _
            & "        , INITYMD    , INITUSER     , INITTERMID" _
            & "        , UPDYMD     , UPDUSER      , UPDTERMID" _
            & "        , RECEIVEYMD)" _
            & "    VALUES" _
            & "        ( @P1  , @P2" _
            & "        , @P3  , @P4 , @P5  , @P6" _
            & "        , @P7  , @P8 , @P9" _
            & "        , @P10 , @P11 ,@P12" _
            & "        , @P13 , @P14, @P15" _
            & "        , @P16) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
              " SELECT" _
            & "    CAMPCODE" _
            & "    , ORGCODE" _
            & "    , STYMD" _
            & "    , ENDYMD" _
            & "    , NAME" _
            & "    , NAMES" _
            & "    , NAMEKANA" _
            & "    , NAMEKANAS" _
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
            & "    OIL.OIM0002_ORG" _
            & " WHERE" _
            & "        CAMPCODE      = @P1" _
            & "        AND ORGCODE       = @P2"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 2)            '会社コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 6)            '組織コード
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.DateTime)               '開始年月日
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.DateTime)               '終了年月日
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 200)          '組織名称
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 100)          '組織名称（短）
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.NVarChar, 100)          '組織名称カナ
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.NVarChar, 100)          '組織名称カナ（短）
                Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", SqlDbType.NVarChar, 1)            '削除フラグ
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.DateTime)             '登録年月日
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 20)         '登録ユーザーID
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 20)         '登録端末
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.DateTime)             '更新年月日
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 20)         '更新ユーザーID
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 20)         '更新端末
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.DateTime)             '集信日時

                Dim JPARA1 As SqlParameter = SQLcmdJnl.Parameters.Add("@P1", SqlDbType.NVarChar, 2)        '会社コード
                Dim JPARA2 As SqlParameter = SQLcmdJnl.Parameters.Add("@P2", SqlDbType.NVarChar, 6)        '組織コード

                Dim OIM0002row As DataRow = OIM0002INPtbl.Rows(0)
                Dim WW_DATENOW As DateTime = Date.Now

                'DB更新
                PARA1.Value = OIM0002row("CAMPCODE")
                PARA2.Value = OIM0002row("ORGCODE")
                PARA3.Value = OIM0002row("STYMD")
                PARA4.Value = OIM0002row("ENDYMD")
                PARA5.Value = OIM0002row("NAME")
                PARA6.Value = OIM0002row("NAMES")
                PARA7.Value = OIM0002row("NAMEKANA")
                PARA8.Value = OIM0002row("NAMEKANAS")
                PARA9.Value = OIM0002row("DELFLG")
                PARA10.Value = WW_DATENOW
                PARA11.Value = Master.USERID
                PARA12.Value = Master.USERTERMID
                PARA13.Value = WW_DATENOW
                PARA14.Value = Master.USERID
                PARA15.Value = Master.USERTERMID
                PARA16.Value = C_DEFAULT_YMD

                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

                OIM0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                '更新ジャーナル出力
                JPARA1.Value = OIM0002row("CAMPCODE")
                JPARA2.Value = OIM0002row("ORGCODE")

                Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                    If IsNothing(OIM0002UPDtbl) Then
                        OIM0002UPDtbl = New DataTable

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            OIM0002UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    End If

                    OIM0002UPDtbl.Clear()
                    OIM0002UPDtbl.Load(SQLdr)
                End Using

                For Each OIM0002UPDrow As DataRow In OIM0002UPDtbl.Rows
                    CS0020JOURNAL.TABLENM = "OIM0002C"
                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                    CS0020JOURNAL.ROW = OIM0002UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0002C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0002C UPDATE_INSERT"
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
        DetailBoxToOIM0002INPtbl(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ERR_SW) Then
            OIM0002tbl_UPD()
            '入力レコードに変更がない場合は、メッセージダイアログを表示して処理打ち切り
            If C_MESSAGE_NO.NO_CHANGE_UPDATE.Equals(WW_ERRCODE) Then
                Master.Output(C_MESSAGE_NO.NO_CHANGE_UPDATE, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
                Exit Sub
            End If
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIM0002tbl, work.WF_SEL_INPTBL.Text)

        '○ メッセージ表示
        If WW_ERR_SW = "" Then
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)
        Else
            If isNormal(WW_ERR_SW) Then
                Master.Output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.INF)

            ElseIf WW_ERR_SW = C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR Then
                Master.Output(WW_ERR_SW, C_MESSAGE_TYPE.ERR, "会社コード", needsPopUp:=True)

            Else
                Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)

            End If
        End If

        If isNormal(WW_ERR_SW) Then
            '前ページ遷移
            Master.TransitionPrevPage(Master.USERCAMP)
        End If

    End Sub

    ''' <summary>
    ''' 詳細画面-テーブル退避
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub DetailBoxToOIM0002INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(TxtDelFlg.Text)            '削除

        '○ GridViewから未選択状態で表更新ボタンを押下時の例外を回避する
        If String.IsNullOrEmpty(WF_Sel_LINECNT.Text) AndAlso
            String.IsNullOrEmpty(TxtDelFlg.Text) Then
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

        Master.CreateEmptyTable(OIM0002INPtbl, work.WF_SEL_INPTBL.Text)
        Dim OIM0002INProw As DataRow = OIM0002INPtbl.NewRow

        '○ 初期クリア
        For Each OIM0002INPcol As DataColumn In OIM0002INPtbl.Columns
            If IsDBNull(OIM0002INProw.Item(OIM0002INPcol)) OrElse IsNothing(OIM0002INProw.Item(OIM0002INPcol)) Then
                Select Case OIM0002INPcol.ColumnName
                    Case "LINECNT"
                        OIM0002INProw.Item(OIM0002INPcol) = 0
                    Case "OPERATION"
                        OIM0002INProw.Item(OIM0002INPcol) = C_LIST_OPERATION_CODE.NODATA
                    Case "TIMSTP"
                        OIM0002INProw.Item(OIM0002INPcol) = 0
                    Case "SELECT"
                        OIM0002INProw.Item(OIM0002INPcol) = 1
                    Case "HIDDEN"
                        OIM0002INProw.Item(OIM0002INPcol) = 0
                    Case Else
                        OIM0002INProw.Item(OIM0002INPcol) = ""
                End Select
            End If
        Next

        'LINECNT
        If WF_Sel_LINECNT.Text = "" Then
            OIM0002INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(WF_Sel_LINECNT.Text, OIM0002INProw("LINECNT"))
            Catch ex As Exception
                OIM0002INProw("LINECNT") = 0
            End Try
        End If

        OIM0002INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        OIM0002INProw("TIMSTP") = 0
        OIM0002INProw("SELECT") = 1
        OIM0002INProw("HIDDEN") = 0

        OIM0002INProw("CAMPCODE") = Me.TxtCampCode.Text                  '会社コード
        OIM0002INProw("ORGCODE") = Me.TxtOrgCode.Text                    '組織コード
        OIM0002INProw("STYMD") = Me.TxtStYmd.Text                        '開始年月日
        OIM0002INProw("ENDYMD") = Me.TxtEndYmd.Text                      '組織コード
        OIM0002INProw("NAME") = Me.TxtOrgName.Text                       '組織名称
        OIM0002INProw("NAMES") = Me.TxtOrgNameS.Text                     '組織名称（短）
        OIM0002INProw("NAMEKANA") = Me.TxtOrgNameKana.Text               '組織名称カナ
        OIM0002INProw("NAMEKANAS") = Me.TxtOrgNameKanaS.Text             '組織名称カナ（短）
        OIM0002INProw("DELFLG") = Me.TxtDelFlg.Text                      '削除

        '○ 名称取得
        '会社名
        CODENAME_get("CAMPCODE", OIM0002INProw("CAMPCODE"), OIM0002INProw("CAMPNAME"), WW_DUMMY)

        '○ チェック用テーブルに登録する
        OIM0002INPtbl.Rows.Add(OIM0002INProw)

    End Sub

    ''' <summary>
    ''' 詳細画面-戻るボタン押下時処理
    ''' </summary>
    Protected Sub WF_CLEAR_Click()

        '○ DetailBoxをINPtblへ退避
        DetailBoxToOIM0002INPtbl(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        Dim inputChangeFlg As Boolean = True
        Dim OIM0002INProw As DataRow = OIM0002INPtbl.Rows(0)

        ' 既存レコードとの比較
        For Each OIM0002row As DataRow In OIM0002tbl.Rows
            ' KEY項目が等しい時
            If OIM0002row("CAMPCODE") = OIM0002INProw("CAMPCODE") AndAlso
                OIM0002row("ORGCODE") = OIM0002INProw("ORGCODE") AndAlso
                OIM0002row("STYMD") = OIM0002INProw("STYMD") Then
                ' KEY項目以外の項目の差異をチェック
                If OIM0002row("DELFLG") = OIM0002INProw("DELFLG") AndAlso
                    OIM0002row("ENDYMD") = OIM0002INProw("ENDYMD") AndAlso
                    OIM0002row("NAME") = OIM0002INProw("NAME") AndAlso
                    OIM0002row("NAMES") = OIM0002INProw("NAMES") AndAlso
                    OIM0002row("NAMEKANA") = OIM0002INProw("NAMEKANA") AndAlso
                    OIM0002row("NAMEKANAS") = OIM0002INProw("NAMEKANAS") Then
                    '変更がない場合、入力変更フラグをOFFにする
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
    ''' 詳細画面-戻るボタン押下時、確認ダイアログOKボタン押下時処理
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

        Master.TransitionPrevPage(Master.USERCAMP)

    End Sub

    ''' <summary>
    ''' 詳細画面初期化
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailBoxClear()

        '○ 状態をクリア
        For Each OIM0002row As DataRow In OIM0002tbl.Rows
            Select Case OIM0002row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0002row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0002row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0002row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIM0002tbl, work.WF_SEL_INPTBL.Text)

        WF_Sel_LINECNT.Text = ""            'LINECNT
        TxtCampCode.Text = ""               '会社コード
        TxtOrgCode.Text = ""                '組織コード
        TxtOrgName.Text = ""                '組織名称
        TxtOrgNameS.Text = ""               '組織名称（短）
        TxtOrgNameKana.Text = ""            '組織名称カナ
        TxtOrgNameKanaS.Text = ""           '組織名称カナ（短）
        TxtDelFlg.Text = ""                 '削除
        Label1.Text = ""                    '削除名称

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
                            Case "WF_STYMD"         '有効年月日(From)
                                .WF_Calendar.Text = TxtStYmd.Text
                            Case "WF_ENDYMD"        '有効年月日(To)
                                .WF_Calendar.Text = TxtEndYmd.Text
                        End Select
                        .ActiveCalendar()

                    Case Else
                        Dim prmData As New Hashtable

                        'フィールドによってパラメータを変える
                        Select Case WF_FIELD.Value
                            Case "WF_CAMPCODE"       '会社コード
                                'If Master.USER_ORG = CONST_ORGCODE_INFOSYS Or CONST_ORGCODE_OIL Then   '情報システムか石油部の場合
                                prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0001CompList.LC_COMPANY_TYPE.ALL
                                'Else
                                '    prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0001CompList.LC_COMPANY_TYPE.ROLE
                                'End If
                                prmData.Item(C_PARAMETERS.LP_COMPANY) = TxtCampCode.Text

                            Case "WF_ORGCODE"       '組織コード
                                Dim AUTHORITYALL_FLG As String = "0"
                                'If Master.USER_ORG = CONST_ORGCODE_INFOSYS Or CONST_ORGCODE_OIL Then   '情報システムか石油部の場合
                                If TxtCampCode.Text = "" Then '会社コードが空の場合
                                    AUTHORITYALL_FLG = "1"
                                Else '会社コードに入力済みの場合
                                    AUTHORITYALL_FLG = "2"
                                End If
                                'End If
                                prmData = work.CreateORGParam(TxtCampCode.Text, AUTHORITYALL_FLG)
                                'prmData = work.CreateORGParam2(TxtCampCode.Text)
                            'Case "WF_MENUROLE"       'メニュー表示制御ロール
                            '    prmData = work.CreateRoleList(WF_CAMPCODE.Text, "MENU")
                            'Case "WF_MAPROLE"       '画面参照更新制御ロール
                            '    prmData = work.CreateRoleList(WF_CAMPCODE.Text, "MAP")
                            'Case "WF_VIEWPROFID"       '画面表示項目制御ロール
                            '    prmData = work.CreateRoleList(WF_CAMPCODE.Text, "VIEW")
                            'Case "WF_RPRTPROFID"       'エクセル出力制御ロール
                            '    prmData = work.CreateRoleList(WF_CAMPCODE.Text, "XML")
                            'Case "WF_APPROVALID"       '承認権限ロール
                            '    prmData = work.CreateRoleList(WF_CAMPCODE.Text, "APPROVAL")
                            Case "WF_DELFLG"
                                prmData.Item(C_PARAMETERS.LP_COMPANY) = Master.USERCAMP
                                prmData.Item(C_PARAMETERS.LP_TYPEMODE) = "2"
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
            ''会社コード
            Case "WF_CAMPCODE"
                CODENAME_get("CAMPCODE", TxtCampCode.Text, Label2.Text, WW_RTN_SW)
            ''組織コード
            'Case "WF_UORG"
            '    CODENAME_get("UORG", WF_UORG.Text, WF_UORG_TEXT.Text, WW_RTN_SW)
            '削除フラグ
            Case "TxtDelFlg"
                CODENAME_get("DELFLG", TxtDelFlg.Text, Label1.Text, WW_RTN_SW)

        End Select

        '○ メッセージ表示
        If isNormal(WW_RTN_SW) Then
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.NOR)
        Else
            If WF_FIELD.Value = "TxtDelFlg" Then
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
                    TxtDelFlg.Text = WW_SelectValue
                    Label1.Text = WW_SelectText
                    TxtDelFlg.Focus()
                Case "WF_STYMD"             '開始年月日(From)
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            TxtStYmd.Text = ""
                        Else
                            TxtStYmd.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    TxtStYmd.Focus()

                Case "WF_ENDYMD"            '終了年月日(To)
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            TxtEndYmd.Text = ""
                        Else
                            TxtEndYmd.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    TxtEndYmd.Focus()

                Case "WF_CAMPCODE"               '会社コード
                    TxtCampCode.Text = WW_SelectValue
                    Label2.Text = WW_SelectText
                    TxtCampCode.Focus()

                Case "WF_ORGCODE"               '組織コード
                    TxtOrgCode.Text = WW_SelectValue
                    Label3.Text = WW_SelectText
                    TxtOrgCode.Focus()

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
                    TxtDelFlg.Focus()
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
        For Each OIM0002INProw As DataRow In OIM0002INPtbl.Rows

            WW_LINE_ERR = ""

            '削除フラグ(バリデーションチェック）
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DELFLG", OIM0002INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("DELFLG", OIM0002INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0002INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0002INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            '開始年月日(バリデーションチェック）
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "STYMD", OIM0002INProw("STYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '年月日チェック
                WW_CheckDate(OIM0002INProw("STYMD"), "開始年月日", WW_CS0024FCHECKERR, dateErrFlag)
                If dateErrFlag = "1" Then
                    WW_CheckMES1 = "・更新できないレコード(開始年月日エラー)です。"
                    WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                    O_RTN = "ERR"
                    Exit Sub
                Else
                    OIM0002INProw("STYMD") = CDate(OIM0002INProw("STYMD")).ToString("yyyy/MM/dd")
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(開始年月日エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0002INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '終了年月日(バリデーションチェック）
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ENDYMD", OIM0002INProw("ENDYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '年月日チェック
                WW_CheckDate(OIM0002INProw("ENDYMD"), "終了年月日", WW_CS0024FCHECKERR, dateErrFlag)
                If dateErrFlag = "1" Then
                    WW_CheckMES1 = "・更新できないレコード(終了年月日エラー)です。"
                    WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                    O_RTN = "ERR"
                    Exit Sub
                Else
                    OIM0002INProw("ENDYMD") = CDate(OIM0002INProw("ENDYMD")).ToString("yyyy/MM/dd")
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(終了年月日エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0002INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '会社コード(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", OIM0002INProw("CAMPCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "会社コード入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0002INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '組織コード(バリデーションチェック)
            '組織コードが設定されている場合のみチェック
            If Not String.IsNullOrEmpty(OIM0002INProw("ORGCODE")) Then
                Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ORGCODE", OIM0002INProw("ORGCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If Not isNormal(WW_CS0024FCHECKERR) Then
                    WW_CheckMES1 = "組織コード入力エラー。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0002INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '一意制約チェック
            '同一レコードの更新の場合、チェック対象外
            '2020/06/16杉山修正
            If OIM0002INProw("CAMPCODE") = work.WF_SEL_CAMPCODE_L.Text _
                AndAlso OIM0002INProw("ORGCODE") = work.WF_SEL_ORGCODE_L.Text Then

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
                                   "([" & OIM0002INProw("CAMPCODE") & "]" &
                                   " [" & OIM0002INProw("ORGCODE") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0002INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR
                End If
            End If

            If WW_LINE_ERR = "" Then
                If OIM0002INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    OIM0002INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LINE_ERR = CONST_PATTERNERR Then
                    '関連チェックエラーをセット
                    OIM0002INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    '単項目チェックエラーをセット
                    OIM0002INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
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
    ''' <param name="OIM0002row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIM0002row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIM0002row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 会社コード         =" & OIM0002row("CAMPCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 組織コード         =" & OIM0002row("ORGCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 開始年月日         =" & OIM0002row("STYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 終了年月日         =" & OIM0002row("ENDYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 組織名称           =" & OIM0002row("NAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 組織名称（短）     =" & OIM0002row("NAMES") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 組織名称カナ       =" & OIM0002row("NAMEKANA") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 組織名称カナ（短） =" & OIM0002row("NAMEKANAS") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除               =" & OIM0002row("DELFLG")
        End If

        rightview.AddErrorReport(WW_ERR_MES)

    End Sub

    ''' <summary>
    ''' OIM0002tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub OIM0002tbl_UPD()

        '○ 画面状態設定
        For Each OIM0002row As DataRow In OIM0002tbl.Rows
            Select Case OIM0002row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0002row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0002row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each OIM0002INProw As DataRow In OIM0002INPtbl.Rows

            'エラーレコード読み飛ばし
            If OIM0002INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            OIM0002INProw.Item("OPERATION") = CONST_INSERT

            ' 既存レコードとの比較
            For Each OIM0002row As DataRow In OIM0002tbl.Rows
                ' KEY項目が等しい時
                If OIM0002row("CAMPCODE") = OIM0002INProw("CAMPCODE") AndAlso
                    OIM0002row("ORGCODE") = OIM0002INProw("ORGCODE") AndAlso
                    OIM0002row("STYMD") = OIM0002INProw("STYMD") Then
                    ' KEY項目以外の項目の差異をチェック
                    If OIM0002row("DELFLG") = OIM0002INProw("DELFLG") AndAlso
                        OIM0002row("ENDYMD") = OIM0002INProw("ENDYMD") AndAlso
                        OIM0002row("NAME") = OIM0002INProw("NAME") AndAlso
                        OIM0002row("NAMES") = OIM0002INProw("NAMES") AndAlso
                        OIM0002row("NAMEKANA") = OIM0002INProw("NAMEKANA") AndAlso
                        OIM0002row("NAMEKANAS") = OIM0002INProw("NAMEKANAS") AndAlso
                        Not C_LIST_OPERATION_CODE.UPDATING.Equals(OIM0002row("OPERATION")) Then
                        ' 変更がないときは「操作」の項目は空白にする
                        OIM0002INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Else
                        ' 変更がある時は「操作」の項目を「更新」に設定する
                        OIM0002INProw("OPERATION") = CONST_UPDATE
                    End If

                    Exit For

                End If
            Next
        Next

        '更新チェック
        If C_LIST_OPERATION_CODE.NODATA.Equals(OIM0002INPtbl.Rows(0)("OPERATION")) Then
            '更新なしの場合、エラーコードに変更なしエラーをセットして処理打ち切り
            WW_ERRCODE = C_MESSAGE_NO.NO_CHANGE_UPDATE
            Exit Sub
        ElseIf CONST_UPDATE.Equals(OIM0002INPtbl.Rows(0)("OPERATION")) OrElse
            CONST_INSERT.Equals(OIM0002INPtbl.Rows(0)("OPERATION")) Then
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
        For Each OIM0002INProw As DataRow In OIM0002INPtbl.Rows
            '発見フラグ
            Dim isFound As Boolean = False

            For Each OIM0002row As DataRow In OIM0002tbl.Rows

                '同一レコードか判定
                If OIM0002INProw("CAMPCODE") = OIM0002row("CAMPCODE") AndAlso
                    OIM0002INProw("ORGCODE") = OIM0002row("ORGCODE") AndAlso
                    OIM0002INProw("STYMD") = OIM0002row("STYMD") Then
                    '画面入力テーブル項目設定
                    OIM0002INProw("LINECNT") = OIM0002row("LINECNT")
                    OIM0002INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    OIM0002INProw("TIMSTP") = OIM0002row("TIMSTP")
                    OIM0002INProw("SELECT") = 0
                    OIM0002INProw("HIDDEN") = 0

                    '項目テーブル項目設定
                    OIM0002row.ItemArray = OIM0002INProw.ItemArray

                    '発見フラグON
                    isFound = True
                    Exit For
                End If
            Next

            '同一レコードが発見できない場合は、追加する
            If Not isFound Then
                Dim nrow = OIM0002tbl.NewRow
                nrow.ItemArray = OIM0002INProw.ItemArray

                '画面入力テーブル項目設定
                nrow("LINECNT") = OIM0002tbl.Rows.Count + 1
                nrow("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                nrow("TIMSTP") = "0"
                nrow("SELECT") = 0
                nrow("HIDDEN") = 0

                OIM0002tbl.Rows.Add(nrow)
            End If
        Next

    End Sub

#Region "未使用"
    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="OIM0002INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef OIM0002INProw As DataRow)

        For Each OIM0002row As DataRow In OIM0002tbl.Rows

            '同一レコードか判定
            If OIM0002INProw("CAMPCODE") = OIM0002row("CAMPCODE") AndAlso
                OIM0002INProw("ORGCODE") = OIM0002row("ORGCODE") AndAlso
                OIM0002INProw("STYMD") = OIM0002row("STYMD") Then
                '画面入力テーブル項目設定
                OIM0002INProw("LINECNT") = OIM0002row("LINECNT")
                OIM0002INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                OIM0002INProw("TIMSTP") = OIM0002row("TIMSTP")
                OIM0002INProw("SELECT") = 1
                OIM0002INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0002row.ItemArray = OIM0002INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 追加予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0002INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef OIM0002INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim OIM0002row As DataRow = OIM0002tbl.NewRow
        OIM0002row.ItemArray = OIM0002INProw.ItemArray

        OIM0002row("LINECNT") = OIM0002tbl.Rows.Count + 1
        If OIM0002INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
            OIM0002row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        Else
            '            OIM0002row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            OIM0002row("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
        End If

        OIM0002row("TIMSTP") = "0"
        OIM0002row("SELECT") = 1
        OIM0002row("HIDDEN") = 0

        OIM0002tbl.Rows.Add(OIM0002row)

    End Sub

    ''' <summary>
    ''' エラーデータの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0002INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_ERR_SUB(ByRef OIM0002INProw As DataRow)

        For Each OIM0002row As DataRow In OIM0002tbl.Rows

            '同一レコードか判定
            If OIM0002INProw("CAMPCODE") = OIM0002row("CAMPCODE") AndAlso
                OIM0002INProw("ORGCODE") = OIM0002row("ORGCODE") AndAlso
                OIM0002INProw("STYMD") = OIM0002row("STYMD") Then
                '画面入力テーブル項目設定
                OIM0002INProw("LINECNT") = OIM0002row("LINECNT")
                OIM0002INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                OIM0002INProw("TIMSTP") = OIM0002row("TIMSTP")
                OIM0002INProw("SELECT") = 1
                OIM0002INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0002row.ItemArray = OIM0002INProw.ItemArray
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
                    prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0001CompList.LC_COMPANY_TYPE.ALL
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "ORGCODE"             '運用部署
                    Dim wkCampCode As String = TxtCampCode.Text
                    If String.IsNullOrEmpty(wkCampCode) Then
                        wkCampCode = work.WF_SEL_CAMPCODE_L.Text
                    End If
                    prmData = work.CreateORGParam2(wkCampCode)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "DELFLG"           '削除
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DELFLG"))

            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
