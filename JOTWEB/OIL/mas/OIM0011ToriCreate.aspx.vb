''************************************************************
' 取引先マスタメンテ一覧画面
' 作成日 2020/10/07
' 更新日 
' 作成者 JOT常井
' 更新者 JOT伊草
'
' 修正履歴:2020/10/07 新規作成
'         :2021/06/09 1)表更新→更新、クリア→戻る、に名称変更
'         :           2)戻るボタン押下時、確認ダイアログ表示→
'         :             確認ダイアログでOK押下時、一覧画面に戻るように修正
'         :           3)更新ボタン押下時、この画面でDB更新→
'         :             一覧画面の表示データに更新後の内容反映して戻るように修正
'         :           4)項目追加「請求先銀行外部コード」「支払先銀行外部コード」
'         :           5)項目削除「銀行コード」「支店コード」「口座種別」「口座番号」「口座名義」
''************************************************************
Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' 取引先マスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class OIM0011ToriCreate
    Inherits Page

    '○ 検索結果格納Table
    Private OIM0011tbl As DataTable                                 '一覧格納用テーブル
    Private OIM0011INPtbl As DataTable                              'チェック用テーブル
    Private OIM0011UPDtbl As DataTable                              '更新用テーブル

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
                    Master.RecoverTable(OIM0011tbl, work.WF_SEL_INPTBL.Text)

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
            If Not IsNothing(OIM0011tbl) Then
                OIM0011tbl.Clear()
                OIM0011tbl.Dispose()
                OIM0011tbl = Nothing
            End If

            If Not IsNothing(OIM0011INPtbl) Then
                OIM0011INPtbl.Clear()
                OIM0011INPtbl.Dispose()
                OIM0011INPtbl = Nothing
            End If

            If Not IsNothing(OIM0011UPDtbl) Then
                OIM0011UPDtbl.Clear()
                OIM0011UPDtbl.Dispose()
                OIM0011UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIM0011WRKINC.MAPIDC
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0011L Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        End If

        '○ 名称設定処理
        '選択行
        WF_Sel_LINECNT.Text = work.WF_SEL_LINECNT.Text

        '取引先コード
        WF_TORICODE.Text = work.WF_SEL_TORICODE2.Text

        '開始年月日
        WF_STYMD.Text = work.WF_SEL_STYMD2.Text

        '終了年月日
        WF_ENDYMD.Text = work.WF_SEL_ENDYMD2.Text

        '取引先名称
        WF_TORINAME.Text = work.WF_SEL_TORINAME.Text

        '取引先略称
        WF_TORINAMES.Text = work.WF_SEL_TORINAMES.Text

        '取引先カナ名称
        WF_TORINAMEKANA.Text = work.WF_SEL_TORINAMEKANA.Text

        '部門名称
        WF_DEPTNAME.Text = work.WF_SEL_DEPTNAME.Text

        '郵便番号（上）
        WF_POSTNUM1.Text = work.WF_SEL_POSTNUM1.Text

        '郵便番号（下）
        WF_POSTNUM2.Text = work.WF_SEL_POSTNUM2.Text

        '住所１
        WF_ADDR1.Text = work.WF_SEL_ADDR1.Text

        '住所２
        WF_ADDR2.Text = work.WF_SEL_ADDR2.Text

        '住所３
        WF_ADDR3.Text = work.WF_SEL_ADDR3.Text

        '住所４
        WF_ADDR4.Text = work.WF_SEL_ADDR4.Text

        '電話番号
        WF_TEL.Text = work.WF_SEL_TEL.Text

        'ＦＡＸ番号
        WF_FAX.Text = work.WF_SEL_FAX.Text

        'メールアドレス
        WF_MAIL.Text = work.WF_SEL_MAIL.Text

        '石油利用フラグ
        WF_OILUSEFLG.Text = work.WF_SEL_OILUSEFLG.Text
        CODENAME_get("OILUSEFLG", WF_OILUSEFLG.Text, WF_OILUSEFLG_TEXT.Text, WW_RTN_SW)

        '請求先銀行外部コード
        WF_INVOICEBANKOUTSIDECODE.Text = work.WF_SEL_INVOICEBANKOUTSIDECODE.Text

        '支払先銀行外部コード
        WF_PAYEEBANKOUTSIDECODE.Text = work.WF_SEL_PAYEEBANKOUTSIDECODE.Text

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
            & "     TORICODE " _
            & " FROM" _
            & "    OIL.OIM0011_TORI" _
            & " WHERE" _
            & "     TORICODE   = @P1" _
            & " AND DELFLG      <> @P2"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)  '取引先コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 1)   '削除フラグ
                PARA1.Value = WF_TORICODE.Text
                PARA2.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    Dim OIM0011Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIM0011Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIM0011Chk.Load(SQLdr)

                    If OIM0011Chk.Rows.Count > 0 Then
                        '重複データエラー
                        O_MESSAGENO = Messages.C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR
                    Else
                        '正常終了時
                        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0011C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0011C UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 取引先マスタ登録更新
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
            & "        OIL.OIM0011_TORI" _
            & "    WHERE" _
            & "        TORICODE       = @P01 " _
            & "        AND STYMD      = @P02 ;" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE OIL.OIM0011_TORI" _
            & "    SET" _
            & "        DELFLG = @P00" _
            & "        , ENDYMD = @P03" _
            & "        , TORINAME = @P04" _
            & "        , TORINAMES = @P05" _
            & "        , TORINAMEKANA = @P06" _
            & "        , DEPTNAME = @P07" _
            & "        , POSTNUM1 = @P08" _
            & "        , POSTNUM2 = @P09" _
            & "        , ADDR1 = @P10" _
            & "        , ADDR2 = @P11" _
            & "        , ADDR3 = @P12" _
            & "        , ADDR4 = @P13" _
            & "        , TEL = @P14" _
            & "        , FAX = @P15" _
            & "        , MAIL = @P16" _
            & "        , OILUSEFLG = @P17" _
            & "        , INVOICEBANKOUTSIDECODE = @P18" _
            & "        , PAYEEBANKOUTSIDECODE = @P19" _
            & "        , UPDYMD = @P23" _
            & "        , UPDUSER = @P24" _
            & "        , UPDTERMID = @P25" _
            & "    WHERE" _
            & "        TORICODE       = @P01" _
            & "        AND STYMD      = @P02 ;" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO OIL.OIM0011_TORI" _
            & "        (DELFLG" _
            & "        , TORICODE" _
            & "        , STYMD" _
            & "        , ENDYMD" _
            & "        , TORINAME" _
            & "        , TORINAMES" _
            & "        , TORINAMEKANA" _
            & "        , DEPTNAME" _
            & "        , POSTNUM1" _
            & "        , POSTNUM2" _
            & "        , ADDR1" _
            & "        , ADDR2" _
            & "        , ADDR3" _
            & "        , ADDR4" _
            & "        , TEL" _
            & "        , FAX" _
            & "        , MAIL" _
            & "        , OILUSEFLG" _
            & "        , INVOICEBANKOUTSIDECODE" _
            & "        , PAYEEBANKOUTSIDECODE" _
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
            & "        , @P15" _
            & "        , @P16" _
            & "        , @P17" _
            & "        , @P18" _
            & "        , @P19" _
            & "        , @P23" _
            & "        , @P24" _
            & "        , @P25" _
            & "        , @P23" _
            & "        , @P24" _
            & "        , @P25" _
            & "        , @P29) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
              " Select" _
            & "     DELFLG" _
            & "     , TORICODE" _
            & "     , STYMD" _
            & "     , ENDYMD" _
            & "     , TORINAME" _
            & "     , TORINAMES" _
            & "     , TORINAMEKANA" _
            & "     , DEPTNAME" _
            & "     , POSTNUM1" _
            & "     , POSTNUM2" _
            & "     , ADDR1" _
            & "     , ADDR2" _
            & "     , ADDR3" _
            & "     , ADDR4" _
            & "     , TEL" _
            & "     , FAX" _
            & "     , MAIL" _
            & "     , OILUSEFLG" _
            & "     , INVOICEBANKOUTSIDECODE" _
            & "     , PAYEEBANKOUTSIDECODE" _
            & "     , INITYMD" _
            & "     , INITUSER" _
            & "     , INITTERMID" _
            & "     , UPDYMD" _
            & "     , UPDUSER" _
            & "     , UPDTERMID" _
            & "     , RECEIVEYMD" _
            & "     , CAST(UPDTIMSTP As bigint) As UPDTIMSTP" _
            & " FROM" _
            & "     OIL.OIM0011_TORI" _
            & " WHERE" _
            & "     TORICODE = @P01" _
            & " AND STYMD = @P02"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA00 As SqlParameter = SQLcmd.Parameters.Add("@P00", SqlDbType.NVarChar, 1)           '削除フラグ
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 10)          '取引先コード
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.Date)                  '開始年月日
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.Date)                  '終了年月日
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 100)         '取引先名称
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 50)          '取引先略称
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 100)         '取引先カナ名称
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.NVarChar, 20)          '部門名称
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.NVarChar, 3)           '郵便番号（上）
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", SqlDbType.NVarChar, 4)           '郵便番号（下）
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 120)         '住所１
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 120)         '住所２
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 120)         '住所３
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 120)         '住所４
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 15)          '電話番号
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 15)          'ＦＡＸ番号
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.NVarChar, 128)         'メールアドレス
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.NVarChar, 1)           '石油利用フラグ
                Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.NVarChar, 4)           '請求先銀行外部コード
                Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.NVarChar, 4)           '支払先銀行外部コード

                Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", SqlDbType.DateTime)              '登録年月日
                Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", SqlDbType.NVarChar, 20)          '登録ユーザーＩＤ
                Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", SqlDbType.NVarChar, 20)          '登録端末
                Dim PARA29 As SqlParameter = SQLcmd.Parameters.Add("@P29", SqlDbType.DateTime)              '集信日時

                Dim JPARA00 As SqlParameter = SQLcmdJnl.Parameters.Add("@P00", SqlDbType.NVarChar, 1)       '削除フラグ
                Dim JPARA01 As SqlParameter = SQLcmdJnl.Parameters.Add("@P01", SqlDbType.NVarChar, 10)      '取引先コード
                Dim JPARA02 As SqlParameter = SQLcmdJnl.Parameters.Add("@P02", SqlDbType.Date)              '開始年月日

                Dim OIM0011row As DataRow = OIM0011INPtbl.Rows(0)

                Dim WW_DATENOW As DateTime = Date.Now

                'DB更新
                PARA00.Value = OIM0011row("DELFLG")
                PARA01.Value = OIM0011row("TORICODE")
                If RTrim(OIM0011row("STYMD")) <> "" Then
                    PARA02.Value = RTrim(OIM0011row("STYMD"))
                Else
                    PARA02.Value = C_DEFAULT_YMD
                End If
                If RTrim(OIM0011row("ENDYMD")) <> "" Then
                    PARA03.Value = RTrim(OIM0011row("ENDYMD"))
                Else
                    PARA03.Value = C_DEFAULT_YMD
                End If
                PARA04.Value = OIM0011row("TORINAME")
                PARA05.Value = OIM0011row("TORINAMES")
                PARA06.Value = OIM0011row("TORINAMEKANA")
                PARA07.Value = OIM0011row("DEPTNAME")
                PARA08.Value = OIM0011row("POSTNUM1")
                PARA09.Value = OIM0011row("POSTNUM2")
                PARA10.Value = OIM0011row("ADDR1")
                PARA11.Value = OIM0011row("ADDR2")
                PARA12.Value = OIM0011row("ADDR3")
                PARA13.Value = OIM0011row("ADDR4")
                PARA14.Value = OIM0011row("TEL")
                PARA15.Value = OIM0011row("FAX")
                PARA16.Value = OIM0011row("MAIL")
                PARA17.Value = OIM0011row("OILUSEFLG")
                PARA18.Value = OIM0011row("INVOICEBANKOUTSIDECODE")
                PARA19.Value = OIM0011row("PAYEEBANKOUTSIDECODE")

                PARA23.Value = WW_DATENOW
                PARA24.Value = Master.USERID
                PARA25.Value = Master.USERTERMID

                PARA29.Value = C_DEFAULT_YMD
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

                OIM0011row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                '更新ジャーナル出力
                JPARA00.Value = OIM0011row("DELFLG")
                JPARA01.Value = OIM0011row("TORICODE")
                If RTrim(OIM0011row("STYMD")) <> "" Then
                    JPARA02.Value = RTrim(OIM0011row("STYMD"))
                Else
                    JPARA02.Value = C_DEFAULT_YMD
                End If

                Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                    If IsNothing(OIM0011UPDtbl) Then
                        OIM0011UPDtbl = New DataTable

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            OIM0011UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    End If

                    OIM0011UPDtbl.Clear()
                    OIM0011UPDtbl.Load(SQLdr)
                End Using

                For Each OIM0011UPDrow As DataRow In OIM0011UPDtbl.Rows
                    CS0020JOURNAL.TABLENM = "OIM0011L"
                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                    CS0020JOURNAL.ROW = OIM0011UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0011L UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0011L UPDATE_INSERT"
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
        DetailBoxToOIM0011INPtbl(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ERR_SW) Then
            OIM0011tbl_UPD()
            '入力レコードに変更がない場合は、メッセージダイアログを表示して処理打ち切り
            If C_MESSAGE_NO.NO_CHANGE_UPDATE.Equals(WW_ERRCODE) Then
                Master.Output(C_MESSAGE_NO.NO_CHANGE_UPDATE, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
                Exit Sub
            End If
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIM0011tbl, work.WF_SEL_INPTBL.Text)

        '○ メッセージ表示
        If WW_ERR_SW = "" Then
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)
        Else
            If isNormal(WW_ERR_SW) Then
                Master.Output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.INF)
            ElseIf WW_ERR_SW = C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR Then
                Master.Output(WW_ERR_SW, C_MESSAGE_TYPE.ERR, "取引先コード", needsPopUp:=True)
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
    Protected Sub DetailBoxToOIM0011INPtbl(ByRef O_RTN As String)

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

        Master.CreateEmptyTable(OIM0011INPtbl, work.WF_SEL_INPTBL.Text)
        Dim OIM0011INProw As DataRow = OIM0011INPtbl.NewRow

        '○ 初期クリア
        For Each OIM0011INPcol As DataColumn In OIM0011INPtbl.Columns
            If IsDBNull(OIM0011INProw.Item(OIM0011INPcol)) OrElse IsNothing(OIM0011INProw.Item(OIM0011INPcol)) Then
                Select Case OIM0011INPcol.ColumnName
                    Case "LINECNT"
                        OIM0011INProw.Item(OIM0011INPcol) = 0
                    Case "OPERATION"
                        OIM0011INProw.Item(OIM0011INPcol) = C_LIST_OPERATION_CODE.NODATA
                    Case "UPDTIMSTP"
                        OIM0011INProw.Item(OIM0011INPcol) = 0
                    Case "SELECT"
                        OIM0011INProw.Item(OIM0011INPcol) = 1
                    Case "HIDDEN"
                        OIM0011INProw.Item(OIM0011INPcol) = 0
                    Case Else
                        OIM0011INProw.Item(OIM0011INPcol) = ""
                End Select
            End If
        Next

        'LINECNT
        If WF_Sel_LINECNT.Text = "" Then
            OIM0011INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(WF_Sel_LINECNT.Text, OIM0011INProw("LINECNT"))
            Catch ex As Exception
                OIM0011INProw("LINECNT") = 0
            End Try
        End If

        OIM0011INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        OIM0011INProw("UPDTIMSTP") = 0
        OIM0011INProw("SELECT") = 1
        OIM0011INProw("HIDDEN") = 0

        OIM0011INProw("TORICODE") = WF_TORICODE.Text                '取引先コード
        OIM0011INProw("STYMD") = WF_STYMD.Text                      '開始年月日
        OIM0011INProw("ENDYMD") = WF_ENDYMD.Text                    '終了年月日
        OIM0011INProw("TORINAME") = WF_TORINAME.Text                '取引先名称
        OIM0011INProw("TORINAMES") = WF_TORINAMES.Text              '取引先略称
        OIM0011INProw("TORINAMEKANA") = WF_TORINAMEKANA.Text        '取引先カナ名称
        OIM0011INProw("DEPTNAME") = WF_DEPTNAME.Text                '部門名称
        OIM0011INProw("POSTNUM1") = WF_POSTNUM1.Text                '郵便番号（上）
        OIM0011INProw("POSTNUM2") = WF_POSTNUM2.Text                '郵便番号（下）
        OIM0011INProw("ADDR1") = WF_ADDR1.Text                      '住所１
        OIM0011INProw("ADDR2") = WF_ADDR2.Text                      '住所２
        OIM0011INProw("ADDR3") = WF_ADDR3.Text                      '住所３
        OIM0011INProw("ADDR4") = WF_ADDR4.Text                      '住所４
        OIM0011INProw("TEL") = WF_TEL.Text                          '電話番号
        OIM0011INProw("FAX") = WF_FAX.Text                          'ＦＡＸ番号
        OIM0011INProw("MAIL") = WF_MAIL.Text                        'メールアドレス
        OIM0011INProw("OILUSEFLG") = WF_OILUSEFLG.Text              '石油利用フラグ
        '請求先銀行外部コード
        OIM0011INProw("INVOICEBANKOUTSIDECODE") = WF_INVOICEBANKOUTSIDECODE.Text
        '支払先銀行外部コード
        OIM0011INProw("PAYEEBANKOUTSIDECODE") = WF_PAYEEBANKOUTSIDECODE.Text
        OIM0011INProw("DELFLG") = WF_DELFLG.Text                    '削除フラグ

        '○ チェック用テーブルに登録する
        OIM0011INPtbl.Rows.Add(OIM0011INProw)

    End Sub

    ''' <summary>
    ''' 詳細画面-クリアボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_CLEAR_Click()

        '○ DetailBoxをINPtblへ退避
        DetailBoxToOIM0011INPtbl(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        Dim inputChangeFlg As Boolean = True
        Dim OIM0011INProw As DataRow = OIM0011INPtbl.Rows(0)

        ' 既存レコードとの比較
        For Each OIM0011row As DataRow In OIM0011tbl.Rows
            ' KEY項目が等しい時
            If OIM0011row("TORICODE") = OIM0011INProw("TORICODE") AndAlso
                OIM0011row("STYMD") = OIM0011INProw("STYMD") Then
                ' KEY項目以外の項目の差異をチェック
                If OIM0011row("ENDYMD") = OIM0011INProw("ENDYMD") AndAlso
                    OIM0011row("TORINAME") = OIM0011INProw("TORINAME") AndAlso
                    OIM0011row("TORINAMES") = OIM0011INProw("TORINAMES") AndAlso
                    OIM0011row("TORINAMEKANA") = OIM0011INProw("TORINAMEKANA") AndAlso
                    OIM0011row("DEPTNAME") = OIM0011INProw("DEPTNAME") AndAlso
                    OIM0011row("POSTNUM1") = OIM0011INProw("POSTNUM1") AndAlso
                    OIM0011row("POSTNUM2") = OIM0011INProw("POSTNUM2") AndAlso
                    OIM0011row("ADDR1") = OIM0011INProw("ADDR1") AndAlso
                    OIM0011row("ADDR2") = OIM0011INProw("ADDR2") AndAlso
                    OIM0011row("ADDR3") = OIM0011INProw("ADDR3") AndAlso
                    OIM0011row("ADDR4") = OIM0011INProw("ADDR4") AndAlso
                    OIM0011row("TEL") = OIM0011INProw("TEL") AndAlso
                    OIM0011row("FAX") = OIM0011INProw("FAX") AndAlso
                    OIM0011row("MAIL") = OIM0011INProw("MAIL") AndAlso
                    OIM0011row("OILUSEFLG") = OIM0011INProw("OILUSEFLG") AndAlso
                    OIM0011row("INVOICEBANKOUTSIDECODE") = OIM0011INProw("INVOICEBANKOUTSIDECODE") AndAlso
                    OIM0011row("PAYEEBANKOUTSIDECODE") = OIM0011INProw("PAYEEBANKOUTSIDECODE") AndAlso
                    OIM0011row("DELFLG") = OIM0011INProw("DELFLG") Then
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
    ''' 詳細画面詳細画面-戻るボタン押下時、確認ダイアログOKボタン押下時処理
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
        For Each OIM0011row As DataRow In OIM0011tbl.Rows
            Select Case OIM0011row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0011row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0011row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0011row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0011row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0011row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIM0011tbl, work.WF_SEL_INPTBL.Text)

        WF_Sel_LINECNT.Text = ""             'LINECNT

        WF_TORICODE.Text = ""                '取引先コード
        WF_STYMD.Text = ""                   '開始年月日
        WF_ENDYMD.Text = ""                  '終了年月日
        WF_TORINAME.Text = ""                '取引先名称
        WF_TORINAMES.Text = ""               '取引先略称
        WF_TORINAMEKANA.Text = ""            '取引先カナ名称
        WF_DEPTNAME.Text = ""                '部門名称
        WF_POSTNUM1.Text = ""                '郵便番号（上）
        WF_POSTNUM2.Text = ""                '郵便番号（下）
        WF_ADDR1.Text = ""                   '住所１
        WF_ADDR2.Text = ""                   '住所２
        WF_ADDR3.Text = ""                   '住所３
        WF_ADDR4.Text = ""                   '住所４
        WF_TEL.Text = ""                     '電話番号
        WF_FAX.Text = ""                     'ＦＡＸ番号
        WF_MAIL.Text = ""                    'メールアドレス
        WF_OILUSEFLG.Text = ""               '石油利用フラグ
        WF_INVOICEBANKOUTSIDECODE.Text = ""  '請求先銀行外部コード
        WF_PAYEEBANKOUTSIDECODE.Text = ""    '支払先銀行外部コード
        WF_DELFLG.Text = ""                  '削除フラグ

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
                                .WF_Calendar.Text = WF_STYMD.Text
                            Case "WF_ENDYMD"        '有効年月日(To)
                                .WF_Calendar.Text = WF_ENDYMD.Text
                        End Select
                        .ActiveCalendar()

                    Case Else
                        Dim prmData As New Hashtable
                        prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

                        'フィールドによってパラメータを変える
                        Select Case WF_FIELD.Value

                            Case "WF_OILUSEFLG"     '石油利用フラグ
                                prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "OILUSEFLG")

                            Case "WF_ACCOUNTTYPE"   '口座種別
                                prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "BANKACCOUNTTYPE")

                            Case "WF_DELFLG"        '削除フラグ
                                prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DELFLG")

                        End Select

                        .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                        .ActiveListBox()
                End Select
            End With
        End If

    End Sub

    Private Sub CreateTORICODE(ByVal I_TORICODE As String, ByVal I_TORINAME As String, ByRef O_TEXT As String, ByRef O_RTN As String, Optional toNarrow As Boolean = False)

        O_TEXT = ""
        O_RTN = ""

        If String.IsNullOrWhiteSpace(I_TORICODE) OrElse String.IsNullOrWhiteSpace(I_TORINAME) Then
            O_RTN = C_MESSAGE_NO.NORMAL
            Exit Sub
        End If

        Try
            If toNarrow Then
                O_TEXT = String.Format("{0}-{1}", StrConv(I_TORINAME, VbStrConv.Narrow), I_TORICODE)
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
            Case "WF_OILUSEFLG"     '石油利用フラグ
                CODENAME_get("OILUSEFLG", WF_OILUSEFLG.Text, WF_OILUSEFLG_TEXT.Text, WW_RTN_SW)
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

                Case "WF_STYMD"             '有効年月日(From)
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_STYMD.Text = ""
                        Else
                            WF_STYMD.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_STYMD.Focus()

                Case "WF_ENDYMD"            '有効年月日(To)
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

                Case "WF_OILUSEFLG"     '石油利用フラグ
                    WF_OILUSEFLG.Text = WW_SelectValue
                    WF_OILUSEFLG_TEXT.Text = WW_SelectText
                    WF_OILUSEFLG.Focus()

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

                Case "WF_OILUSEFLG"                     '石油利用フラグ
                    WF_OILUSEFLG.Focus()

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
        For Each OIM0011INProw As DataRow In OIM0011INPtbl.Rows

            WW_LINE_ERR = ""

            '削除フラグ(バリデーションチェック）
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DELFLG", OIM0011INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("DELFLG", OIM0011INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '取引先コード(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TORICODE", OIM0011INProw("TORICODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(取引先コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '開始年月日(バリデーションチェック）
            WW_TEXT = OIM0011INProw("STYMD")
            Master.CheckField(Master.USERCAMP, "STYMD", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '年月日チェック
                WW_CheckDate(WW_TEXT, "開始年月日", WW_CS0024FCHECKERR, dateErrFlag)
                If dateErrFlag = "1" Then
                    WW_CheckMES1 = "・更新できないレコード(開始年月日エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKERR
                    O_RTN = "ERR"
                    Exit Sub
                Else
                    OIM0011INProw("STYMD") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(開始年月日エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '終了年月日(バリデーションチェック）
            WW_TEXT = OIM0011INProw("ENDYMD")
            Master.CheckField(Master.USERCAMP, "ENDYMD", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '年月日チェック
                WW_CheckDate(WW_TEXT, "終了年月日", WW_CS0024FCHECKERR, dateErrFlag)
                If dateErrFlag = "1" Then
                    WW_CheckMES1 = "・更新できないレコード(終了年月日エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKERR
                    O_RTN = "ERR"
                    Exit Sub
                Else
                    OIM0011INProw("ENDYMD") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(終了年月日エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '取引先名(バリデーションチェック)
            WW_TEXT = OIM0011INProw("TORINAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TORINAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(取引先名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '取引先略称(バリデーションチェック)
            WW_TEXT = OIM0011INProw("TORINAMES")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TORINAMES", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(取引先略称入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '取引先名カナ(バリデーションチェック)
            WW_TEXT = OIM0011INProw("TORINAMEKANA")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TORINAMEKANA", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(取引先名カナ入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '部門名称(バリデーションチェック)
            WW_TEXT = OIM0011INProw("DEPTNAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DEPTNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(部門名称入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '郵便番号（上）(バリデーションチェック)
            WW_TEXT = OIM0011INProw("POSTNUM1")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "POSTNUM1", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(郵便番号（上）入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '郵便番号（下）(バリデーションチェック)
            WW_TEXT = OIM0011INProw("POSTNUM2")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "POSTNUM2", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(郵便番号（下）入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '住所１(バリデーションチェック)
            WW_TEXT = OIM0011INProw("ADDR1")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ADDR1", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(住所１入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '住所２(バリデーションチェック)
            WW_TEXT = OIM0011INProw("ADDR2")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ADDR2", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(住所２入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '住所３(バリデーションチェック)
            WW_TEXT = OIM0011INProw("ADDR3")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ADDR3", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(住所３入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '住所４(バリデーションチェック)
            WW_TEXT = OIM0011INProw("ADDR4")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ADDR4", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(住所４入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '電話番号(バリデーションチェック)
            WW_TEXT = OIM0011INProw("TEL")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TEL", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(電話番号入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'ＦＡＸ番号(バリデーションチェック)
            WW_TEXT = OIM0011INProw("FAX")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "FAX", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(ＦＡＸ番号入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'メールアドレス(バリデーションチェック)
            WW_TEXT = OIM0011INProw("MAIL")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "MAIL", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(メールアドレス入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '石油利用フラグ(バリデーションチェック)
            WW_TEXT = OIM0011INProw("OILUSEFLG")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OILUSEFLG", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("OILUSEFLG", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(石油利用フラグ入力エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(石油利用フラグ入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ''銀行コード(バリデーションチェック)
            'WW_TEXT = OIM0011INProw("BANKCODE")
            'Master.CheckField(work.WF_SEL_CAMPCODE.Text, "BANKCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            'If Not isNormal(WW_CS0024FCHECKERR) Then
            '    WW_CheckMES1 = "・更新できないレコード(銀行コード入力エラー)です。"
            '    WW_CheckMES2 = WW_CS0024FCHECKREPORT
            '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
            '    WW_LINE_ERR = "ERR"
            '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            'End If

            ''支店コード(バリデーションチェック)
            'WW_TEXT = OIM0011INProw("BANKBRANCHCODE")
            'Master.CheckField(work.WF_SEL_CAMPCODE.Text, "BANKBRANCHCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            'If Not isNormal(WW_CS0024FCHECKERR) Then
            '    WW_CheckMES1 = "・更新できないレコード(支店コード入力エラー)です。"
            '    WW_CheckMES2 = WW_CS0024FCHECKREPORT
            '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
            '    WW_LINE_ERR = "ERR"
            '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            'End If

            ''口座種別(バリデーションチェック）
            'Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ACCOUNTTYPE", OIM0011INProw("ACCOUNTTYPE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            'If isNormal(WW_CS0024FCHECKERR) Then
            '    '値存在チェック
            '    CODENAME_get("ACCOUNTTYPE", OIM0011INProw("ACCOUNTTYPE"), WW_DUMMY, WW_RTN_SW)
            '    If Not isNormal(WW_RTN_SW) Then
            '        WW_CheckMES1 = "・更新できないレコード(口座種別エラー)です。"
            '        WW_CheckMES2 = "マスタに存在しません。"
            '        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
            '        WW_LINE_ERR = "ERR"
            '        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            '    End If
            'Else
            '    WW_CheckMES1 = "・更新できないレコード(口座種別エラー)です。"
            '    WW_CheckMES2 = WW_CS0024FCHECKREPORT
            '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
            '    WW_LINE_ERR = "ERR"
            '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            'End If

            ''口座番号(バリデーションチェック)
            'WW_TEXT = OIM0011INProw("ACCOUNTNUMBER")
            'Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ACCOUNTNUMBER", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            'If Not isNormal(WW_CS0024FCHECKERR) Then
            '    WW_CheckMES1 = "・更新できないレコード(口座番号入力エラー)です。"
            '    WW_CheckMES2 = WW_CS0024FCHECKREPORT
            '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
            '    WW_LINE_ERR = "ERR"
            '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            'End If

            ''口座名義(バリデーションチェック)
            'WW_TEXT = OIM0011INProw("ACCOUNTNAME")
            'Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ACCOUNTNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            'If Not isNormal(WW_CS0024FCHECKERR) Then
            '    WW_CheckMES1 = "・更新できないレコード(口座名義入力エラー)です。"
            '    WW_CheckMES2 = WW_CS0024FCHECKREPORT
            '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
            '    WW_LINE_ERR = "ERR"
            '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            'End If

            '請求先銀行外部コード(バリデーションチェック)
            WW_TEXT = OIM0011INProw("INVOICEBANKOUTSIDECODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "INVOICEBANKOUTSIDECODE",
                              WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(請求先銀行外部コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '支払先銀行外部コード(バリデーションチェック)
            WW_TEXT = OIM0011INProw("PAYEEBANKOUTSIDECODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "PAYEEBANKOUTSIDECODE",
                              WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(支払先銀行外部コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '一意制約チェック
            '同一レコードの更新の場合、チェック対象外
            If OIM0011INProw("TORICODE") = work.WF_SEL_TORICODE2.Text AndAlso
                OIM0011INProw("STYMD") = work.WF_SEL_STYMD2.Text Then
            Else
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    'DataBase接続
                    SQLcon.Open()

                    '一意制約チェック
                    UniqueKeyCheck(SQLcon, WW_UniqueKeyCHECK)
                End Using

                If Not isNormal(WW_UniqueKeyCHECK) Then
                    WW_CheckMES1 = "一意制約違反（取引先コード）。"
                    WW_CheckMES2 = C_MESSAGE_NO.OVERLAP_DATA_ERROR &
                                       "([" & OIM0011INProw("TORICODE") & "]"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR
                End If
            End If

            If WW_LINE_ERR = "" Then
                If OIM0011INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    OIM0011INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LINE_ERR = CONST_PATTERNERR Then
                    '関連チェックエラーをセット
                    OIM0011INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    '単項目チェックエラーをセット
                    OIM0011INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
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
    ''' <param name="OIM0011row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIM0011row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIM0011row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 取引先コード =" & OIM0011row("TORICODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 開始年月日 =" & OIM0011row("STYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 終了年月日 =" & OIM0011row("ENDYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 取引先名称 =" & OIM0011row("TORINAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 取引先略称 =" & OIM0011row("TORINAMES") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 取引先カナ名称 =" & OIM0011row("TORINAMEKANA") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 部門名称 =" & OIM0011row("DEPTNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 郵便番号（上） =" & OIM0011row("POSTNUM1") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 郵便番号（下） =" & OIM0011row("POSTNUM2") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 住所１ =" & OIM0011row("ADDR1") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 住所２ =" & OIM0011row("ADDR2") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 住所３ =" & OIM0011row("ADDR3") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 住所４ =" & OIM0011row("ADDR4") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 電話番号 =" & OIM0011row("TEL") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> ＦＡＸ番号 =" & OIM0011row("FAX") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> メールアドレス =" & OIM0011row("MAIL") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 石油利用フラグ =" & OIM0011row("OILUSEFLG") & " , "
            'WW_ERR_MES &= ControlChars.NewLine & "  --> 銀行コード =" & OIM0011row("BANKCODE") & " , "
            'WW_ERR_MES &= ControlChars.NewLine & "  --> 支店コード =" & OIM0011row("BANKBRANCHCODE") & " , "
            'WW_ERR_MES &= ControlChars.NewLine & "  --> 口座種別 =" & OIM0011row("ACCOUNTTYPE") & " , "
            'WW_ERR_MES &= ControlChars.NewLine & "  --> 口座番号 =" & OIM0011row("ACCOUNTNUMBER") & " , "
            'WW_ERR_MES &= ControlChars.NewLine & "  --> 口座名義 =" & OIM0011row("ACCOUNTNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 請求先銀行外部コード =" & OIM0011row("INVOICEBANKOUTSIDECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 支払先銀行外部コード =" & OIM0011row("PAYEEBANKOUTSIDECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除フラグ =" & OIM0011row("DELFLG")
        End If

        rightview.AddErrorReport(WW_ERR_MES)

    End Sub

    ''' <summary>
    ''' OIM0011tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub OIM0011tbl_UPD()

        '○ 画面状態設定
        For Each OIM0011row As DataRow In OIM0011tbl.Rows
            Select Case OIM0011row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0011row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0011row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0011row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0011row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0011row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each OIM0011INProw As DataRow In OIM0011INPtbl.Rows

            'エラーレコード読み飛ばし
            If OIM0011INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            OIM0011INProw.Item("OPERATION") = CONST_INSERT

            ' 既存レコードとの比較
            For Each OIM0011row As DataRow In OIM0011tbl.Rows
                ' KEY項目が等しい時
                If OIM0011row("TORICODE") = OIM0011INProw("TORICODE") AndAlso
                    OIM0011row("STYMD") = OIM0011INProw("STYMD") Then
                    ' KEY項目以外の項目の差異をチェック
                    If OIM0011row("ENDYMD") = OIM0011INProw("ENDYMD") AndAlso
                        OIM0011row("TORINAME") = OIM0011INProw("TORINAME") AndAlso
                        OIM0011row("TORINAMES") = OIM0011INProw("TORINAMES") AndAlso
                        OIM0011row("TORINAMEKANA") = OIM0011INProw("TORINAMEKANA") AndAlso
                        OIM0011row("DEPTNAME") = OIM0011INProw("DEPTNAME") AndAlso
                        OIM0011row("POSTNUM1") = OIM0011INProw("POSTNUM1") AndAlso
                        OIM0011row("POSTNUM2") = OIM0011INProw("POSTNUM2") AndAlso
                        OIM0011row("ADDR1") = OIM0011INProw("ADDR1") AndAlso
                        OIM0011row("ADDR2") = OIM0011INProw("ADDR2") AndAlso
                        OIM0011row("ADDR3") = OIM0011INProw("ADDR3") AndAlso
                        OIM0011row("ADDR4") = OIM0011INProw("ADDR4") AndAlso
                        OIM0011row("TEL") = OIM0011INProw("TEL") AndAlso
                        OIM0011row("FAX") = OIM0011INProw("FAX") AndAlso
                        OIM0011row("MAIL") = OIM0011INProw("MAIL") AndAlso
                        OIM0011row("OILUSEFLG") = OIM0011INProw("OILUSEFLG") AndAlso
                        OIM0011row("INVOICEBANKOUTSIDECODE") = OIM0011INProw("INVOICEBANKOUTSIDECODE") AndAlso
                        OIM0011row("PAYEEBANKOUTSIDECODE") = OIM0011INProw("PAYEEBANKOUTSIDECODE") AndAlso
                        OIM0011row("DELFLG") = OIM0011INProw("DELFLG") Then
                        ' 変更がないときは「操作」の項目は空白にする
                        OIM0011INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Else
                        ' 変更がある時は「操作」の項目を「更新」に設定する
                        OIM0011INProw("OPERATION") = CONST_UPDATE
                    End If

                    Exit For

                End If
            Next
        Next

        '更新チェック
        If C_LIST_OPERATION_CODE.NODATA.Equals(OIM0011INPtbl.Rows(0)("OPERATION")) Then
            '更新なしの場合、エラーコードに変更なしエラーをセットして処理打ち切り
            WW_ERRCODE = C_MESSAGE_NO.NO_CHANGE_UPDATE
            Exit Sub
        ElseIf CONST_UPDATE.Equals(OIM0011INPtbl.Rows(0)("OPERATION")) OrElse
            CONST_INSERT.Equals(OIM0011INPtbl.Rows(0)("OPERATION")) Then
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
        For Each OIM0011INProw As DataRow In OIM0011INPtbl.Rows
            '発見フラグ
            Dim isFound As Boolean = False

            ' 既存レコードとの比較
            For Each OIM0011row As DataRow In OIM0011tbl.Rows
                ' KEY項目が等しい時
                If OIM0011row("TORICODE") = OIM0011INProw("TORICODE") AndAlso
                    OIM0011row("STYMD") = OIM0011INProw("STYMD") Then

                    '画面入力テーブル項目設定
                    OIM0011INProw("LINECNT") = OIM0011row("LINECNT")
                    OIM0011INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    OIM0011INProw("UPDTIMSTP") = OIM0011row("UPDTIMSTP")
                    OIM0011INProw("SELECT") = 0
                    OIM0011INProw("HIDDEN") = 0

                    '項目テーブル項目設定
                    OIM0011row.ItemArray = OIM0011INProw.ItemArray

                    '発見フラグON
                    isFound = True
                    Exit For
                End If
            Next

            '同一レコードが発見できない場合は、追加する
            If Not isFound Then
                Dim nrow = OIM0011tbl.NewRow
                nrow.ItemArray = OIM0011INProw.ItemArray

                '画面入力テーブル項目設定
                nrow("LINECNT") = OIM0011tbl.Rows.Count + 1
                nrow("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                nrow("UPDTIMSTP") = "0"
                nrow("SELECT") = 0
                nrow("HIDDEN") = 0

                OIM0011tbl.Rows.Add(nrow)
            End If
        Next

    End Sub

#Region "未使用"
    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="OIM0011INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef OIM0011INProw As DataRow)

        For Each OIM0011row As DataRow In OIM0011tbl.Rows

            '同一レコードか判定
            If OIM0011INProw("TORICODE") = OIM0011row("TORICODE") AndAlso
                OIM0011INProw("STYMD") = OIM0011row("STYMD") Then
                '画面入力テーブル項目設定
                OIM0011INProw("LINECNT") = OIM0011row("LINECNT")
                OIM0011INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                OIM0011INProw("UPDTIMSTP") = OIM0011row("UPDTIMSTP")
                OIM0011INProw("SELECT") = 1
                OIM0011INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0011row.ItemArray = OIM0011INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 追加予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0011INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef OIM0011INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim OIM0011row As DataRow = OIM0011tbl.NewRow
        OIM0011row.ItemArray = OIM0011INProw.ItemArray

        OIM0011row("LINECNT") = OIM0011tbl.Rows.Count + 1
        If OIM0011INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
            OIM0011row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        Else
            OIM0011row("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
        End If

        OIM0011row("UPDTIMSTP") = "0"
        OIM0011row("SELECT") = 1
        OIM0011row("HIDDEN") = 0

        OIM0011tbl.Rows.Add(OIM0011row)

    End Sub

    ''' <summary>
    ''' エラーデータの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0011INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_ERR_SUB(ByRef OIM0011INProw As DataRow)

        For Each OIM0011row As DataRow In OIM0011tbl.Rows

            '同一レコードか判定
            If OIM0011INProw("TORICODE") = OIM0011row("TORICODE") AndAlso
                OIM0011INProw("STYMD") = OIM0011row("STYMD") Then
                '画面入力テーブル項目設定
                OIM0011INProw("LINECNT") = OIM0011row("LINECNT")
                OIM0011INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                OIM0011INProw("UPDTIMSTP") = OIM0011row("UPDTIMSTP")
                OIM0011INProw("SELECT") = 1
                OIM0011INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0011row.ItemArray = OIM0011INProw.ItemArray
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
            prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

            Select Case I_FIELD
                Case "OILUSEFLG"                    '石油利用フラグ
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "OILUSEFLG")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "DELFLG"                       '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
