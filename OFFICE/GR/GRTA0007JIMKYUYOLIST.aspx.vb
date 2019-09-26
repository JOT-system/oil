Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox

Public Class GRTA0007JIMKYUYOLIST
    Inherits Page

    '共通関数宣言(BASEDLL)
    ''' <summary>
    ''' LogOutput DirString Get
    ''' </summary>
    Private CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
    ''' <summary>
    ''' ユーザプロファイル（GridView）設定
    ''' </summary>
    Private CS0013ProfView As New CS0013ProfView                    'ユーザプロファイル（GridView）設定
    ''' <summary>
    ''' テーブルデータソー
    ''' </summary>
    Private CS0026TBLSORT As New CS0026TBLSORT                      'GridView用テーブルソート文字列取得
    ''' <summary>
    ''' 帳票出力(入力：TBL)
    ''' </summary>
    Private CS0030REPORT As New CS0030REPORT                        '帳票出力(入力：TBL)
    ''' <summary>
    ''' セッション情報管理
    ''' </summary>
    Private CS0050Session As New CS0050SESSION
    '機能特有共通クラス
    ''' <summary>
    ''' 勤怠共通
    ''' </summary>
    Private T0007COM As New GRT0007COM                              '勤怠共通
    ''' <summary>
    ''' 時間調整共通
    ''' </summary>
    Private T0009TIME As New GRT00009TIMEFORMAT                     '時間調整共通

    '検索結果格納
    Private TA0007ALL As DataTable                                  '全データテーブル
    Private TA0007VIEWtbl As DataTable                              'Grid格納用テーブル
    '共通処理結果
    ''' <summary>
    ''' 共通用エラーID保持枠
    ''' </summary>
    Private WW_ERRCODE As String = String.Empty                     'リターンコード
    ''' <summary>
    ''' 共通用戻値保持枠
    ''' </summary>
    Private WW_RTN_SW As String                                     '
    ''' <summary>
    ''' 共通用引数虚数設定用枠（使用は非推奨）
    ''' </summary>
    Private WW_DUMMY As String                                      '
    ''' <summary>
    ''' 一覧最大表示件数（一画面）
    ''' </summary>
    Private Const CONST_DSPROWCOUNT As Integer = 45                 '１画面表示対象
    ''' <summary>
    ''' 一覧のマウススクロール時の増分（件数）
    ''' </summary>
    Private Const CONST_SCROLLROWCOUNT As Integer = 20              'マウススクロール時の増分
    ''' <summary>
    ''' 詳細部タブID
    ''' </summary>
    Private Const CONST_DETAIL_TABID As String = "DTL1"             '詳細部タブID


    ''' <summary>
    ''' サーバ処理の遷移先
    ''' </summary>
    ''' <param name="sender">起動オブジェクト</param>
    ''' <param name="e">イベント発生時パラメータ</param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        If IsPostBack Then
            '■■■ 各ボタン押下処理 ■■■
            If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                Select Case WF_ButtonClick.Value
                    Case "WF_ButtonPDF"                 '■ 印刷ボタンクリック時処理
                        WF_ButtonPDF_Click()
                    Case "WF_ButtonXLS"                 '■ ダウンロードボタンクリック時処理
                        WF_ButtonXLS_Click()
                    Case "WF_ButtonFIRST"               '■ 最始行ボタンクリック時処理
                        WF_ButtonFIRST_Click()
                    Case "WF_ButtonLAST"                '■ 最終行ボタンクリック時処理
                        WF_ButtonLAST_Click()
                    Case "WF_ButtonEND"                 '■ 終了ボタンクリック時処理
                        WF_ButtonEND_Click()
                    Case "WF_RadioButonClick"           '■ 右ボックスラジオボタン選択時処理 
                        WF_RadioButon_Click()
                    Case "WF_MEMOChange"                '■ メモ欄保存処理
                        WF_MEMO_Change()
                End Select
            End If
            '○ 一覧再表示処理
            DisplayGrid()
        Else
            '〇初期化処理
            Initialize()
        End If

        If Not IsNothing(TA0007ALL) Then
            TA0007ALL.Dispose()
            TA0007ALL = Nothing
        End If
        If Not IsNothing(TA0007VIEWtbl) Then
            TA0007VIEWtbl.Dispose()
            TA0007VIEWtbl = Nothing
        End If
    End Sub
    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()
        '○初期値設定

        rightview.resetindex()
        leftview.activeListBox()
        '〇 条件抽出画面情報退避
        MAPrefelence()
        '〇ヘルプ無
        Master.dispHelp = False
        '〇ドラックアンドドロップOFF
        Master.eventDrop = False
        '■ ヘッダー表示
        WF_SEL_DATE.Text = work.WF_SEL_TAISHOYM.Text
        CodeToName("ORG", work.WF_SEL_HORG.Text, WF_SEL_ORG.Text, WW_DUMMY)
        '右Boxへの値設定
        rightview.MAPID = Master.MAPID
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_DUMMY)
        '○画面表示データ取得
        GetAllTA0007Tbl()
        'データソート
        CS0026TBLSORT.TABLE = TA0007ALL
        CS0026TBLSORT.SORTING = "LINECNT , SEQ ASC"
        CS0026TBLSORT.FILTER = String.Empty
        TA0007ALL = CS0026TBLSORT.sort()

        '○画面表示データ保存
        '■■■ 画面（GridView）表示データ保存 ■■■
        If Not Master.SaveTable(TA0007ALL) Then Exit Sub
        '１０分切上処理
        CopyT7Edit(TA0007ALL, TA0007VIEWtbl)
        '一覧表示データ編集（性能対策）
        Using TBLview As DataView = New DataView(TA0007VIEWtbl)
            TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & (CONST_DSPROWCOUNT)
            CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0013ProfView.PROFID = Master.PROF_VIEW
            CS0013ProfView.MAPID = GRTA0007WRKINC.MAPID
            CS0013ProfView.VARI = Master.VIEWID
            CS0013ProfView.SRCDATA = TBLview.ToTable
            CS0013ProfView.TBLOBJ = pnlListArea
            CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Horizontal
            CS0013ProfView.TITLEOPT = True
            CS0013ProfView.HIDEOPERATIONOPT = True
            CS0013ProfView.CS0013ProfView()
        End Using
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If

        '重複チェック
        Dim WW_MSG As String = C_MESSAGE_NO.NORMAL
        T0007COM.T0007_DuplCheck(TA0007ALL, WW_MSG, WW_ERRCODE)
        If Not isNormal(WW_ERRCODE) Then
            Master.output(WW_ERRCODE, C_MESSAGE_TYPE.ABORT)
        Else
            rightview.addErrorReport(ControlChars.NewLine & WW_MSG)
        End If

    End Sub
    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()
        '〇データリカバリ
        If IsNothing(TA0007ALL) Then
            If Not Master.RecoverTable(TA0007ALL) Then Exit Sub
        End If
        'TA0007VIEW設定
        GetViewTA0007Tbl("H")

        Dim WW_GridPosition As Integer                 '表示位置（開始）
        Dim WW_DataCNT As Integer = 0                  '(絞り込み後)有効Data数

        '表示対象行カウント(絞り込み対象)
        '　※　絞込（Cells(4)： 0=表示対象 , 1=非表示対象)
        For i As Integer = 0 To TA0007VIEWtbl.Rows.Count - 1
            If TA0007VIEWtbl.Rows(i)(4) = "0" Then
                WW_DataCNT = WW_DataCNT + 1
                '行（ラインカウント）を再設定する。既存項目（SELECT）を利用
                TA0007VIEWtbl.Rows(i)("SELECT") = WW_DataCNT
            End If
        Next

        '○表示Linecnt取得
        If WF_GridPosition.Text = "" Then
            WW_GridPosition = 1
        Else
            Try
                Integer.TryParse(WF_GridPosition.Text, WW_GridPosition)
            Catch ex As Exception
                WW_GridPosition = 1
            End Try
        End If

        '○表示格納位置決定

        '表示開始_格納位置決定(次頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelUp" Then
            If (WW_GridPosition + CONST_SCROLLROWCOUNT) <= WW_DataCNT Then
                WW_GridPosition = WW_GridPosition + CONST_SCROLLROWCOUNT
            End If
        End If

        '表示開始_位置決定(前頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelDown" Then
            If (WW_GridPosition - CONST_SCROLLROWCOUNT) > 0 Then
                WW_GridPosition = WW_GridPosition - CONST_SCROLLROWCOUNT
            Else
                WW_GridPosition = 1
            End If
        End If

        '○画面（GridView）表示
        Dim WW_TBLview As DataView = New DataView(TA0007VIEWtbl)

        'ソート
        WW_TBLview.Sort = "LINECNT"
        WW_TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString & " and SELECT < " & (WW_GridPosition + CONST_DSPROWCOUNT).ToString
        '一覧作成

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = GRTA0007WRKINC.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = WW_TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Horizontal
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.CS0013ProfView()

        '○クリア
        If WW_TBLview.Count = 0 Then
            WF_GridPosition.Text = "1"
        Else
            WF_GridPosition.Text = WW_TBLview.Item(0)("SELECT")
        End If

    End Sub
    ''' <summary>
    ''' ﾀﾞｳﾝﾛｰﾄﾞ(PDF出力)・一覧印刷ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonPDF_Click()

        '■ テーブルデータ 復元
        '○全表示データ 復元
        If IsNothing(TA0007ALL) Then
            If Not Master.RecoverTable(TA0007ALL) Then Exit Sub
        End If

        '■ 帳票出力
        '〇 TA0007VIEWtblカラム設定
        AddColumnToTA0007Tbl(TA0007VIEWtbl)

        '○TA0007VIEWtbl取得
        GetViewTA0007Tbl("H")

        '帳票出力用編集
        EditList(TA0007VIEWtbl)

        '○ 帳票出力
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = GRTA0007WRKINC.MAPID               '画面ID
        CS0030REPORT.REPORTID = rightview.getReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "pdf"                            '出力ファイル形式
        CS0030REPORT.TBLDATA = TA0007VIEWtbl                    'データ参照DataTable
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORTtbl")
            Exit Sub
        End If

        '○別画面でExcelを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

    End Sub
    ''' <summary>
    ''' ﾀﾞｳﾝﾛｰﾄﾞ(Excel出力)ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonXLS_Click()

        '■ テーブルデータ 復元
        '○全表示データ 復元
        If IsNothing(TA0007ALL) Then
            If Not Master.RecoverTable(TA0007ALL) Then Exit Sub
        End If

        '■ 帳票出力
        '〇 カラム設定
        AddColumnToTA0007Tbl(TA0007VIEWtbl)

        '○TA0007VIEWtbl取得
        GetViewTA0007Tbl("H")

        '帳票出力用編集
        EditList(TA0007VIEWtbl)

        '○ 帳票出力
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = GRTA0007WRKINC.MAPID               '画面ID
        CS0030REPORT.REPORTID = rightview.getReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "XLSX"                            '出力ファイル形式
        CS0030REPORT.TBLDATA = TA0007VIEWtbl                    'データ参照DataTable
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORTtbl")
            Exit Sub
        End If

        '○別画面でExcelを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

    End Sub
    ''' <summary>
    ''' 終了ボタン押下
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        '○ 画面遷移実行
        Master.transitionPrevPage()
    End Sub
    ''' <summary>
    ''' 右ボックスのラジオボタン選択時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RadioButon_Click()
        '〇RightBox処理（ラジオボタン選択）
        If Not String.IsNullOrEmpty(WF_RightViewChange.Value) Then
            Try
                Integer.TryParse(WF_RightViewChange.Value, WF_RightViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try
            rightview.selectIndex(WF_RightViewChange.Value)
            WF_RightViewChange.Value = ""
        End If
    End Sub
    ''' <summary>
    ''' メモ欄変更時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_MEMO_Change()
        '〇RightBox処理（右Boxメモ変更時）
        rightview.MAPID = Master.MAPID
        rightview.save(Master.USERID, Master.USERTERMID, WW_DUMMY)
    End Sub
    ''' <summary>
    ''' 先頭頁移動ボタン押下
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonFIRST_Click()

        '■ データリカバリ 
        '○データリカバリ 
        If IsNothing(TA0007ALL) Then
            If Not Master.RecoverTable(TA0007ALL) Then Exit Sub
        End If

        '〇 TA0007VIEWtblカラム設定
        AddColumnToTA0007Tbl(TA0007VIEWtbl)

        '○TA0007VIEWtbl取得
        GetViewTA0007Tbl("H")

        '■ GridView表示
        '○ 先頭頁に移動
        WF_GridPosition.Text = "1"

    End Sub

    ''' <summary>
    ''' 最終頁ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonLAST_Click()

        '■ データリカバリ 
        '○データリカバリ 
        If IsNothing(TA0007ALL) Then
            If Not Master.RecoverTable(TA0007ALL) Then Exit Sub
        End If

        '〇 TA0007VIEWtblカラム設定
        AddColumnToTA0007Tbl(TA0007VIEWtbl)

        '○TA0007VIEWtbl取得
        GetViewTA0007Tbl("H")

        '○ソート
        Dim WW_TBLview As DataView
        WW_TBLview = New DataView(TA0007VIEWtbl)
        WW_TBLview.RowFilter = "HIDDEN= '0'"

        '■ GridView表示
        '○ 最終頁に移動
        If WW_TBLview.Count Mod CONST_SCROLLROWCOUNT = 0 Then
            WF_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod CONST_SCROLLROWCOUNT)
        Else
            WF_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod CONST_SCROLLROWCOUNT) + 1
        End If

    End Sub

    ' ******************************************************************************
    ' ***  共通処理                                                              ***　
    ' ******************************************************************************

    ''' <summary>
    ''' TA0007All全表示データ取得処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GetAllTA0007Tbl()


        '■ 画面表示用データ取得
        If IsNothing(TA0007ALL) Then TA0007ALL = New DataTable
        'TA0007テンポラリDB項目作成
        AddColumnToTA0007Tbl(TA0007ALL)

        'オブジェクト内容検索
        Try
            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                'テンポラリーテーブルを作成する
                Dim SQLStr0 As String = "CREATE TABLE #MBtemp " _
                        & " ( " _
                        & "  CAMPCODE nvarchar(20)," _
                        & "  STAFFCODE nvarchar(20)," _
                        & "  HORG nvarchar(20)," _
                        & " ) "

                Dim SQLStr1 As String =
                     " SELECT  isnull(rtrim(MB1.CAMPCODE),'')           as  CAMPCODE  , " _
                   & "              isnull(rtrim(MB1.STAFFCODE),'')     as  STAFFCODE   " _
                   & " FROM       MB001_STAFF MB1                                       " _
                   & " INNER JOIN S0012_SRVAUTHOR X                                  ON " _
                   & "         X.TERMID         = @TERMID                               " _
                   & "   and   X.CAMPCODE       = @CAMPCODE                             " _
                   & "   and   X.OBJECT         = 'SRVORG'                              " _
                   & "   and   X.STYMD         <= @NOW                                  " _
                   & "   and   X.ENDYMD        >= @NOW                                  " _
                   & "   and   X.DELFLG        <> '1'                                   " _
                   & " INNER JOIN S0006_ROLE Y                                       ON " _
                   & "         Y.CAMPCODE       = X.CAMPCODE " _
                   & "   and   Y.OBJECT         = 'SRVORG' " _
                   & "   and   Y.ROLE           = X.ROLE" _
                   & "   and   Y.STYMD         <= @NOW " _
                   & "   and   Y.ENDYMD        >= @NOW " _
                   & "   and   Y.DELFLG        <> '1' " _
                   & " INNER JOIN M0006_STRUCT   Z  " _
                   & "   ON    Z.CAMPCODE      = @CAMPCODE " _
                   & "   and   Z.OBJECT        = 'ORG' " _
                   & "   and   Z.GRCODE01      = @HORG " _
                   & "   and   Z.STRUCT        = '勤怠管理組織' " _
                   & "   and   Z.STYMD        <= @NOW " _
                   & "   and   Z.ENDYMD       >= @NOW " _
                   & "   and   Z.DELFLG       <> '1'  " _
                   & "   and   Z.CODE          = Y.CODE " _
                   & "   and   Z.CODE          = MB1.HORG " _
                   & " where  MB1.CAMPCODE                         =  @CAMPCODE                               " _
                   & "   and  MB1.STAFFKBN                  not like '03%'                                    " _
                   & "   and  MB1.STYMD                           <=  @SEL_ENDYMD                             " _
                   & "   and  MB1.ENDYMD                          >=  @SEL_STYMD                              " _
                   & "   and  MB1.STAFFKBN                    NOT IN  ('01102','01412')                       " _
                   & "   and  MB1.DELFLG                          <>  '1'                                     " _
                   & " group by MB1.CAMPCODE, MB1.STAFFCODE                                                   "


                '検索SQL文     ※乗務員照会も可能にするため、ユーザ権限チェックは行わない。
                Dim SQLStr As String =
                     " SELECT * FROM ( " _
                   & " SELECT 0 as LINECNT , " _
                   & "       '' as OPERATION , " _
                   & "       '1' as HIDDEN , " _
                   & "       TIMSTP = cast(isnull(A.UPDTIMSTP,0) as bigint) , " _
                   & "       isnull(rtrim(A.CAMPCODE),'')  as CAMPCODE, " _
                   & "       ''  as CAMPNAMES, " _
                   & "       @TAISHOYM as TAISHOYM , " _
                   & "       isnull(rtrim(A.STAFFCODE),'') as STAFFCODE, " _
                   & "       isnull(rtrim(MB2.STAFFNAMES),'') as STAFFNAMES , " _
                   & "       isnull(rtrim(A.WORKDATE),'') as WORKDATE , " _
                   & "       isnull(rtrim(CAL.WORKINGWEEK),'0') as WORKINGWEEK , " _
                   & "       isnull(rtrim(F1.VALUE1),'') as WORKINGWEEKNAMES , " _
                   & "       isnull(rtrim(A.HDKBN),'H') as HDKBN , " _
                   & "       isnull(rtrim(A.RECODEKBN),'0') as RECODEKBN , " _
                   & "       isnull(rtrim(F3.VALUE1),'') as RECODEKBNNAMES , " _
                   & "       isnull(A.SEQ,'0') as SEQ , " _
                   & "       isnull(rtrim(A.ENTRYDATE),'') as ENTRYDATE , " _
                   & "       isnull(rtrim(A.NIPPOLINKCODE),'') as NIPPOLINKCODE , " _
                   & "       isnull(rtrim(A.MORG),'') as MORG , " _
                   & "       '' as MORGNAMES , " _
                   & "       isnull(rtrim(A.HORG),'') as HORG , " _
                   & "       '' as HORGNAMES , " _
                   & "       isnull(rtrim(A.SORG),'') as SORG , " _
                   & "       '' as SORGNAMES , " _
                   & "       isnull(rtrim(A.STAFFKBN),'') as STAFFKBN , " _
                   & "       isnull(rtrim(F8.VALUE1),'') as STAFFKBNNAMES , " _
                   & "       isnull(rtrim(A.HOLIDAYKBN),'') as HOLIDAYKBN , " _
                   & "       isnull(rtrim(F9.VALUE1),'') as HOLIDAYKBNNAMES , " _
                   & "       isnull(rtrim(A.PAYKBN),'00') as PAYKBN , " _
                   & "       isnull(rtrim(F10.VALUE1),'') as PAYKBNNAMES , " _
                   & "       isnull(rtrim(A.SHUKCHOKKBN),'0') as SHUKCHOKKBN , " _
                   & "       isnull(rtrim(F11.VALUE1),'') as SHUKCHOKKBNNAMES , " _
                   & "       isnull(rtrim(A.WORKKBN),'')  as WORKKBN , " _
                   & "       isnull(rtrim(F4.VALUE2),'') as WORKKBNNAMES , " _
                   & "       isnull(rtrim(A.STDATE),'') as STDATE , " _
                   & "       isnull(rtrim(A.STTIME),'') as STTIME , " _
                   & "       isnull(rtrim(A.ENDDATE),'') as ENDDATE , " _
                   & "       isnull(rtrim(A.ENDTIME),'') as ENDTIME , " _
                   & "       isnull(A.WORKTIME,0) as WORKTIME , " _
                   & "       isnull(A.MOVETIME,0) as MOVETIME , " _
                   & "       isnull(A.ACTTIME,0) as ACTTIME , " _
                   & "       isnull(rtrim(A.BINDSTDATE),'') as BINDSTDATE , " _
                   & "       isnull(A.BINDTIME,'0') as BINDTIME , " _
                   & "       isnull(A.BREAKTIME,0) as BREAKTIME , " _
                   & "       isnull(A.BREAKTIMECHO,0) as BREAKTIMECHO , " _
                   & "       isnull(A.NIPPOBREAKTIME,0) + isnull(A.BREAKTIME,0) + isnull(A.BREAKTIMECHO,0) as BREAKTIMETTL , " _
                   & "       isnull(A.NIGHTTIME,0) as NIGHTTIME , " _
                   & "       isnull(A.NIGHTTIMECHO,0) as NIGHTTIMECHO , " _
                   & "       isnull(A.NIGHTTIME,0) + isnull(A.NIGHTTIMECHO,0) as NIGHTTIMETTL , " _
                   & "       isnull(A.ORVERTIME,0) as ORVERTIME , " _
                   & "       isnull(A.ORVERTIMECHO,0) as ORVERTIMECHO , " _
                   & "       isnull(A.ORVERTIME,0) + isnull(A.ORVERTIMECHO,0) + isnull(A.ORVERTIMEADD,0) as ORVERTIMETTL , " _
                   & "       isnull(A.WNIGHTTIME,0) as WNIGHTTIME , " _
                   & "       isnull(A.WNIGHTTIMECHO,0) as WNIGHTTIMECHO , " _
                   & "       isnull(A.WNIGHTTIME,0) + isnull(A.WNIGHTTIMECHO,0) + isnull(A.WNIGHTTIMEADD,0) as WNIGHTTIMETTL , " _
                   & "       isnull(A.SWORKTIME,0) as SWORKTIME , " _
                   & "       isnull(A.SWORKTIMECHO,0) as SWORKTIMECHO , " _
                   & "       isnull(A.SWORKTIME,0) + isnull(A.SWORKTIMECHO,0) + isnull(A.SWORKTIMEADD,0) as SWORKTIMETTL , " _
                   & "       isnull(A.SNIGHTTIME,0) as SNIGHTTIME , " _
                   & "       isnull(A.SNIGHTTIMECHO,0) as SNIGHTTIMECHO , " _
                   & "       isnull(A.SNIGHTTIME,0) + isnull(A.SNIGHTTIMECHO,0) + isnull(A.SNIGHTTIMEADD,0) as SNIGHTTIMETTL , " _
                   & "       isnull(A.HWORKTIME,0) as HWORKTIME , " _
                   & "       isnull(A.HWORKTIMECHO,0) as HWORKTIMECHO , " _
                   & "       isnull(A.HWORKTIME,0) + isnull(A.HWORKTIMECHO,0) as HWORKTIMETTL , " _
                   & "       isnull(A.HNIGHTTIME,0) as HNIGHTTIME , " _
                   & "       isnull(A.HNIGHTTIMECHO,0) as HNIGHTTIMECHO , " _
                   & "       isnull(A.HNIGHTTIME,0) + isnull(A.HNIGHTTIMECHO,0) as HNIGHTTIMETTL , " _
                   & "       isnull(A.WORKNISSU,0) as WORKNISSU , " _
                   & "       isnull(A.WORKNISSUCHO,0) as WORKNISSUCHO , " _
                   & "       isnull(A.WORKNISSU, 0) + isnull(A.WORKNISSUCHO, 0) as WORKNISSUTTL , " _
                   & "       isnull(A.SHOUKETUNISSU,0) as SHOUKETUNISSU , " _
                   & "       isnull(A.SHOUKETUNISSUCHO,0) as SHOUKETUNISSUCHO , " _
                   & "       isnull(A.SHOUKETUNISSU, 0) + isnull(A.SHOUKETUNISSUCHO, 0) as SHOUKETUNISSUTTL , " _
                   & "       isnull(A.KUMIKETUNISSU,0) as KUMIKETUNISSU , " _
                   & "       isnull(A.KUMIKETUNISSUCHO,0) as KUMIKETUNISSUCHO , " _
                   & "       isnull(A.KUMIKETUNISSU, 0) + isnull(A.KUMIKETUNISSUCHO, 0) as KUMIKETUNISSUTTL , " _
                   & "       isnull(A.ETCKETUNISSU,0) as ETCKETUNISSU , " _
                   & "       isnull(A.ETCKETUNISSUCHO,0) as ETCKETUNISSUCHO , " _
                   & "       isnull(A.ETCKETUNISSU, 0) + isnull(A.ETCKETUNISSUCHO, 0) as ETCKETUNISSUTTL , " _
                   & "       isnull(A.NENKYUNISSU,0) as NENKYUNISSU , " _
                   & "       isnull(A.NENKYUNISSUCHO,0) as NENKYUNISSUCHO , " _
                   & "       isnull(A.NENKYUNISSU, 0) + isnull(A.NENKYUNISSUCHO, 0) as NENKYUNISSUTTL , " _
                   & "       isnull(A.TOKUKYUNISSU,0) as TOKUKYUNISSU , " _
                   & "       isnull(A.TOKUKYUNISSUCHO,0) as TOKUKYUNISSUCHO , " _
                   & "       isnull(A.TOKUKYUNISSU, 0) + isnull(A.TOKUKYUNISSUCHO, 0) as TOKUKYUNISSUTTL , " _
                   & "       isnull(A.CHIKOKSOTAINISSU,0) as CHIKOKSOTAINISSU , " _
                   & "       isnull(A.CHIKOKSOTAINISSUCHO,0) as CHIKOKSOTAINISSUCHO , " _
                   & "       isnull(A.CHIKOKSOTAINISSU, 0) + isnull(A.CHIKOKSOTAINISSUCHO, 0) as CHIKOKSOTAINISSUTTL , " _
                   & "       isnull(A.STOCKNISSU,0) as STOCKNISSU , " _
                   & "       isnull(A.STOCKNISSUCHO,0) as STOCKNISSUCHO , " _
                   & "       isnull(A.STOCKNISSU, 0) + isnull(A.STOCKNISSUCHO, 0) as STOCKNISSUTTL , " _
                   & "       isnull(A.KYOTEIWEEKNISSU,0) as KYOTEIWEEKNISSU , " _
                   & "       isnull(A.KYOTEIWEEKNISSUCHO,0) as KYOTEIWEEKNISSUCHO , " _
                   & "       isnull(A.KYOTEIWEEKNISSU, 0) + isnull(A.KYOTEIWEEKNISSUCHO, 0) as KYOTEIWEEKNISSUTTL , " _
                   & "       isnull(A.WEEKNISSU,0) as WEEKNISSU , " _
                   & "       isnull(A.WEEKNISSUCHO,0) as WEEKNISSUCHO , " _
                   & "       isnull(A.WEEKNISSU, 0) + isnull(A.WEEKNISSUCHO, 0) as WEEKNISSUTTL , " _
                   & "       isnull(A.DAIKYUNISSU,0) as DAIKYUNISSU , " _
                   & "       isnull(A.DAIKYUNISSUCHO,0) as DAIKYUNISSUCHO , " _
                   & "       isnull(A.DAIKYUNISSU, 0) + isnull(A.DAIKYUNISSUCHO, 0) as DAIKYUNISSUTTL , " _
                   & "       isnull(A.NENSHINISSU,0) as NENSHINISSU , " _
                   & "       isnull(A.NENSHINISSUCHO,0) as NENSHINISSUCHO , " _
                   & "       isnull(A.NENSHINISSU, 0) + isnull(A.NENSHINISSUCHO, 0) as NENSHINISSUTTL , " _
                   & "       isnull(A.NENMATUNISSU, 0) as NENMATUNISSU , " _
                   & "       isnull(A.NENMATUNISSUCHO, 0) as NENMATUNISSUCHO , " _
                   & "       isnull(A.NENMATUNISSU, 0) + isnull(A.NENMATUNISSUCHO, 0) as NENMATUNISSUTTL , " _
                   & "       isnull(A.SHUKCHOKNNISSU,0) as SHUKCHOKNNISSU , " _
                   & "       isnull(A.SHUKCHOKNNISSUCHO,0) as SHUKCHOKNNISSUCHO , " _
                   & "       isnull(A.SHUKCHOKNNISSU, 0) + isnull(A.SHUKCHOKNNISSUCHO, 0) as SHUKCHOKNNISSUTTL , " _
                   & "       isnull(A.SHUKCHOKNISSU,0) as SHUKCHOKNISSU , " _
                   & "       isnull(A.SHUKCHOKNISSUCHO,0) as SHUKCHOKNISSUCHO , " _
                   & "       isnull(A.SHUKCHOKNISSU, 0) + isnull(A.SHUKCHOKNISSUCHO, 0) as SHUKCHOKNISSUTTL , " _
                   & "       isnull(A.SHUKCHOKNHLDNISSU,0) as SHUKCHOKNHLDNISSU , " _
                   & "       isnull(A.SHUKCHOKNHLDNISSUCHO,0) as SHUKCHOKNHLDNISSUCHO , " _
                   & "       isnull(A.SHUKCHOKNHLDNISSU, 0) + isnull(A.SHUKCHOKNHLDNISSUCHO, 0) as SHUKCHOKNHLDNISSUTTL , " _
                   & "       isnull(A.SHUKCHOKHLDNISSU,0) as SHUKCHOKHLDNISSU , " _
                   & "       isnull(A.SHUKCHOKHLDNISSUCHO,0) as SHUKCHOKHLDNISSUCHO , " _
                   & "       isnull(A.SHUKCHOKHLDNISSU, 0) + isnull(A.SHUKCHOKHLDNISSUCHO, 0) as SHUKCHOKHLDNISSUTTL , " _
                   & "       isnull(A.TOKSAAKAISU,0) as TOKSAAKAISU , " _
                   & "       isnull(A.TOKSAAKAISUCHO,0) as TOKSAAKAISUCHO , " _
                   & "       isnull(A.TOKSAAKAISU, 0) + isnull(A.TOKSAAKAISUCHO, 0) as TOKSAAKAISUTTL , " _
                   & "       isnull(A.TOKSABKAISU,0) as TOKSABKAISU , " _
                   & "       isnull(A.TOKSABKAISUCHO,0) as TOKSABKAISUCHO , " _
                   & "       isnull(A.TOKSABKAISU, 0) + isnull(A.TOKSABKAISUCHO, 0) as TOKSABKAISUTTL , " _
                   & "       isnull(A.TOKSACKAISU,0) as TOKSACKAISU , " _
                   & "       isnull(A.TOKSACKAISUCHO,0) as TOKSACKAISUCHO , " _
                   & "       isnull(A.TOKSACKAISU, 0) + isnull(A.TOKSACKAISUCHO, 0) as TOKSACKAISUTTL , " _
                   & "       isnull(A.HOANTIME,0) as HOANTIME , " _
                   & "       isnull(A.HOANTIMECHO,0) as HOANTIMECHO , " _
                   & "       isnull(A.HOANTIME, 0) + isnull(A.HOANTIMECHO, 0) as HOANTIMETTL , " _
                   & "       isnull(A.KOATUTIME,0) as KOATUTIME , " _
                   & "       isnull(A.KOATUTIMECHO,0) as KOATUTIMECHO , " _
                   & "       isnull(A.KOATUTIME, 0) + isnull(A.KOATUTIMECHO, 0) as KOATUTIMETTL , " _
                   & "       isnull(A.TOKUSA1TIME,0) as TOKUSA1TIME , " _
                   & "       isnull(A.TOKUSA1TIMECHO,0) as TOKUSA1TIMECHO , " _
                   & "       isnull(A.TOKUSA1TIME, 0) + isnull(A.TOKUSA1TIMECHO, 0) as TOKUSA1TIMETTL , " _
                   & "       isnull(A.HAYADETIME,0) as HAYADETIME , " _
                   & "       isnull(A.HAYADETIMECHO,0) as HAYADETIMECHO , " _
                   & "       isnull(A.HAYADETIME, 0) + isnull(A.HAYADETIMECHO, 0) as HAYADETIMETTL , " _
                   & "       isnull(A.PONPNISSU,0) as PONPNISSU , " _
                   & "       isnull(A.PONPNISSUCHO,0) as PONPNISSUCHO , " _
                   & "       isnull(A.PONPNISSU, 0) + isnull(A.PONPNISSUCHO, 0) as PONPNISSUTTL , " _
                   & "       isnull(A.BULKNISSU,0) as BULKNISSU , " _
                   & "       isnull(A.BULKNISSUCHO,'0') as BULKNISSUCHO , " _
                   & "       isnull(A.BULKNISSU, 0) + isnull(A.BULKNISSUCHO, 0) as BULKNISSUTTL , " _
                   & "       isnull(A.TRAILERNISSU,0) as TRAILERNISSU , " _
                   & "       isnull(A.TRAILERNISSUCHO,0) as TRAILERNISSUCHO , " _
                   & "       isnull(A.TRAILERNISSU, 0) + isnull(A.TRAILERNISSUCHO, 0) as TRAILERNISSUTTL , " _
                   & "       isnull(A.BKINMUKAISU,0) as BKINMUKAISU , " _
                   & "       isnull(A.BKINMUKAISUCHO,0) as BKINMUKAISUCHO , " _
                   & "       isnull(A.BKINMUKAISU, 0) + isnull(A.BKINMUKAISUCHO, 0) as BKINMUKAISUTTL , " _
                   & "       isnull(rtrim(A.SHARYOKBN),'') as SHARYOKBN , " _
                   & "       isnull(rtrim(F6.VALUE1),'') as SHARYOKBNNAMES , " _
                   & "       isnull(rtrim(A.OILPAYKBN),'') as OILPAYKBN , " _
                   & "       isnull(rtrim(F7.VALUE1),'') as OILPAYKBNNAMES , " _
                   & "       isnull(A.HAIDISTANCE,0) as HAIDISTANCE , " _
                   & "       isnull(A.HAIDISTANCECHO,0) as HAIDISTANCECHO , " _
                   & "       isnull(A.HAIDISTANCE,0) + isnull(A.HAIDISTANCECHO, 0) as HAIDISTANCETTL , " _
                   & "       isnull(A.KAIDISTANCE,0) as KAIDISTANCE , " _
                   & "       isnull(A.KAIDISTANCECHO,0) as KAIDISTANCECHO , " _
                   & "       isnull(A.KAIDISTANCE, 0) + isnull(A.KAIDISTANCECHO, 0) as KAIDISTANCETTL , " _
                   & "       isnull(A.UNLOADCNT,0) as UNLOADCNT , " _
                   & "       isnull(A.UNLOADCNTCHO,0) as UNLOADCNTCHO , " _
                   & "       isnull(A.UNLOADCNT, 0) + isnull(A.UNLOADCNTCHO, 0) as UNLOADCNTTTL , " _
                   & "       isnull(A.JIKYUSHATIME, 0) as JIKYUSHATIME , " _
                   & "       isnull(A.JIKYUSHATIMECHO, 0) as JIKYUSHATIMECHO , " _
                   & "       isnull(A.JIKYUSHATIME, 0) + isnull(A.JIKYUSHATIMECHO, 0) as JIKYUSHATIMETTL , " _
                   & "       isnull(A.HDAIWORKTIME, 0) as HDAIWORKTIME , " _
                   & "       isnull(A.HDAIWORKTIMECHO, 0) as HDAIWORKTIMECHO , " _
                   & "       isnull(A.HDAIWORKTIME, 0) + isnull(A.HDAIWORKTIMECHO, 0) as HDAIWORKTIMETTL , " _
                   & "       isnull(A.HDAINIGHTTIME, 0) as HDAINIGHTTIME , " _
                   & "       isnull(A.HDAINIGHTTIMECHO, 0) as HDAINIGHTTIMECHO , " _
                   & "       isnull(A.HDAINIGHTTIME, 0) + isnull(A.HDAINIGHTTIMECHO, 0) as HDAINIGHTTIMETTL , " _
                   & "       isnull(A.SDAIWORKTIME, 0) as SDAIWORKTIME , " _
                   & "       isnull(A.SDAIWORKTIMECHO, 0) as SDAIWORKTIMECHO , " _
                   & "       isnull(A.SDAIWORKTIME, 0) + isnull(A.SDAIWORKTIMECHO, 0) as SDAIWORKTIMETTL , " _
                   & "       isnull(A.SDAINIGHTTIME, 0) as SDAINIGHTTIME , " _
                   & "       isnull(A.SDAINIGHTTIMECHO, 0) as SDAINIGHTTIMECHO , " _
                   & "       isnull(A.SDAINIGHTTIME, 0) + isnull(A.SDAINIGHTTIMECHO, 0) as SDAINIGHTTIMETTL , " _
                   & "       isnull(A.HWORKNISSU,0) as HWORKNISSU , " _
                   & "       isnull(A.HWORKNISSUCHO,0) as HWORKNISSUCHO , " _
                   & "       isnull(A.HWORKNISSU,0) + isnull(A.HWORKNISSUCHO,0) as HWORKNISSUTTL , " _
                   & "       isnull(rtrim(A.DELFLG),'') as DELFLG , " _
                   & "       '' as SHARYOKBN1 , " _
                   & "       '' as SHARYOKBNNAMES1 , " _
                   & "       '' as OILPAYKBN101 , " _
                   & "       '' as OILPAYKBNNAMES101 , " _
                   & "       0  as HAIDISTANCE101 , " _
                   & "       0 as HAIDISTANCECHO101 , " _
                   & "       0 as HAIDISTANCETTL101 , " _
                   & "       0 as KAIDISTANCE101 , " _
                   & "       0 as KAIDISTANCECHO101 , " _
                   & "       0 as KAIDISTANCETTL101 , " _
                   & "       0 as UNLOADCNT101 , " _
                   & "       0 as UNLOADCNTCHO101 , " _
                   & "       0 as UNLOADCNTTTL101 , " _
                   & "       '' as OILPAYKBN102 , " _
                   & "       '' as OILPAYKBNNAMES102 , " _
                   & "       0  as HAIDISTANCE102 , " _
                   & "       0 as HAIDISTANCECHO102 , " _
                   & "       0 as HAIDISTANCETTL102 , " _
                   & "       0 as KAIDISTANCE102 , " _
                   & "       0 as KAIDISTANCECHO102 , " _
                   & "       0 as KAIDISTANCETTL102 , " _
                   & "       0 as UNLOADCNT102 , " _
                   & "       0 as UNLOADCNTCHO102 , " _
                   & "       0 as UNLOADCNTTTL102 , " _
                   & "       '' as OILPAYKBN103 , " _
                   & "       '' as OILPAYKBNNAMES103 , " _
                   & "       0  as HAIDISTANCE103 , " _
                   & "       0 as HAIDISTANCECHO103 , " _
                   & "       0 as HAIDISTANCETTL103 , " _
                   & "       0 as KAIDISTANCE103 , " _
                   & "       0 as KAIDISTANCECHO103 , " _
                   & "       0 as KAIDISTANCETTL103 , " _
                   & "       0 as UNLOADCNT103 , " _
                   & "       0 as UNLOADCNTCHO103 , " _
                   & "       0 as UNLOADCNTTTL103 , " _
                   & "       '' as OILPAYKBN104 , " _
                   & "       '' as OILPAYKBNNAMES104 , " _
                   & "       0  as HAIDISTANCE104 , " _
                   & "       0 as HAIDISTANCECHO104 , " _
                   & "       0 as HAIDISTANCETTL104 , " _
                   & "       0 as KAIDISTANCE104 , " _
                   & "       0 as KAIDISTANCECHO104 , " _
                   & "       0 as KAIDISTANCETTL104 , " _
                   & "       0 as UNLOADCNT104 , " _
                   & "       0 as UNLOADCNTCHO104 , " _
                   & "       0 as UNLOADCNTTTL104 , " _
                   & "       '' as OILPAYKBN105 , " _
                   & "       '' as OILPAYKBNNAMES105 , " _
                   & "       0  as HAIDISTANCE105 , " _
                   & "       0 as HAIDISTANCECHO105 , " _
                   & "       0 as HAIDISTANCETTL105 , " _
                   & "       0 as KAIDISTANCE105 , " _
                   & "       0 as KAIDISTANCECHO105 , " _
                   & "       0 as KAIDISTANCETTL105 , " _
                   & "       0 as UNLOADCNT105 , " _
                   & "       0 as UNLOADCNTCHO105 , " _
                   & "       0 as UNLOADCNTTTL105 , " _
                   & "       '' as OILPAYKBN106 , " _
                   & "       '' as OILPAYKBNNAMES106 , " _
                   & "       0  as HAIDISTANCE106 , " _
                   & "       0 as HAIDISTANCECHO106 , " _
                   & "       0 as HAIDISTANCETTL106 , " _
                   & "       0 as KAIDISTANCE106 , " _
                   & "       0 as KAIDISTANCECHO106 , " _
                   & "       0 as KAIDISTANCETTL106 , " _
                   & "       0 as UNLOADCNT106 , " _
                   & "       0 as UNLOADCNTCHO106 , " _
                   & "       0 as UNLOADCNTTTL106 , " _
                   & "       '' as OILPAYKBN107 , " _
                   & "       '' as OILPAYKBNNAMES107 , " _
                   & "       0  as HAIDISTANCE107 , " _
                   & "       0 as HAIDISTANCECHO107 , " _
                   & "       0 as HAIDISTANCETTL107 , " _
                   & "       0 as KAIDISTANCE107 , " _
                   & "       0 as KAIDISTANCECHO107 , " _
                   & "       0 as KAIDISTANCETTL107 , " _
                   & "       0 as UNLOADCNT107 , " _
                   & "       0 as UNLOADCNTCHO107 , " _
                   & "       0 as UNLOADCNTTTL107 , " _
                   & "       '' as OILPAYKBN108 , " _
                   & "       '' as OILPAYKBNNAMES108 , " _
                   & "       0  as HAIDISTANCE108 , " _
                   & "       0 as HAIDISTANCECHO108 , " _
                   & "       0 as HAIDISTANCETTL108 , " _
                   & "       0 as KAIDISTANCE108 , " _
                   & "       0 as KAIDISTANCECHO108 , " _
                   & "       0 as KAIDISTANCETTL108 , " _
                   & "       0 as UNLOADCNT108 , " _
                   & "       0 as UNLOADCNTCHO108 , " _
                   & "       0 as UNLOADCNTTTL108 , " _
                   & "       '' as OILPAYKBN109 , " _
                   & "       '' as OILPAYKBNNAMES109 , " _
                   & "       0  as HAIDISTANCE109 , " _
                   & "       0 as HAIDISTANCECHO109 , " _
                   & "       0 as HAIDISTANCETTL109 , " _
                   & "       0 as KAIDISTANCE109 , " _
                   & "       0 as KAIDISTANCECHO109 , " _
                   & "       0 as KAIDISTANCETTL109 , " _
                   & "       0 as UNLOADCNT109 , " _
                   & "       0 as UNLOADCNTCHO109 , " _
                   & "       0 as UNLOADCNTTTL109 , " _
                   & "       '' as OILPAYKBN110 , " _
                   & "       '' as OILPAYKBNNAMES110 , " _
                   & "       0  as HAIDISTANCE110 , " _
                   & "       0 as HAIDISTANCECHO110 , " _
                   & "       0 as HAIDISTANCETTL110 , " _
                   & "       0 as KAIDISTANCE110 , " _
                   & "       0 as KAIDISTANCECHO110 , " _
                   & "       0 as KAIDISTANCETTL110 , " _
                   & "       0 as UNLOADCNT110 , " _
                   & "       0 as UNLOADCNTCHO110 , " _
                   & "       0 as UNLOADCNTTTL110 , " _
                   & "       '' as SHARYOKBN2 , " _
                   & "       '' as SHARYOKBNNAMES2 , " _
                   & "       '' as OILPAYKBN201 , " _
                   & "       '' as OILPAYKBNNAMES201 , " _
                   & "       0  as HAIDISTANCE201 , " _
                   & "       0 as HAIDISTANCECHO201 , " _
                   & "       0 as HAIDISTANCETTL201 , " _
                   & "       0 as KAIDISTANCE201 , " _
                   & "       0 as KAIDISTANCECHO201 , " _
                   & "       0 as KAIDISTANCETTL201 , " _
                   & "       0 as UNLOADCNT201 , " _
                   & "       0 as UNLOADCNTCHO201 , " _
                   & "       0 as UNLOADCNTTTL201 , " _
                   & "       '' as OILPAYKBN202 , " _
                   & "       '' as OILPAYKBNNAMES202 , " _
                   & "       0  as HAIDISTANCE202 , " _
                   & "       0 as HAIDISTANCECHO202 , " _
                   & "       0 as HAIDISTANCETTL202 , " _
                   & "       0 as KAIDISTANCE202 , " _
                   & "       0 as KAIDISTANCECHO202 , " _
                   & "       0 as KAIDISTANCETTL202 , " _
                   & "       0 as UNLOADCNT202 , " _
                   & "       0 as UNLOADCNTCHO202 , " _
                   & "       0 as UNLOADCNTTTL202 , " _
                   & "       '' as OILPAYKBN203 , " _
                   & "       '' as OILPAYKBNNAMES203 , " _
                   & "       0  as HAIDISTANCE203 , " _
                   & "       0 as HAIDISTANCECHO203 , " _
                   & "       0 as HAIDISTANCETTL203 , " _
                   & "       0 as KAIDISTANCE203 , " _
                   & "       0 as KAIDISTANCECHO203 , " _
                   & "       0 as KAIDISTANCETTL203 , " _
                   & "       0 as UNLOADCNT203 , " _
                   & "       0 as UNLOADCNTCHO203 , " _
                   & "       0 as UNLOADCNTTTL203 , " _
                   & "       '' as OILPAYKBN204 , " _
                   & "       '' as OILPAYKBNNAMES204 , " _
                   & "       0  as HAIDISTANCE204 , " _
                   & "       0 as HAIDISTANCECHO204 , " _
                   & "       0 as HAIDISTANCETTL204 , " _
                   & "       0 as KAIDISTANCE204 , " _
                   & "       0 as KAIDISTANCECHO204 , " _
                   & "       0 as KAIDISTANCETTL204 , " _
                   & "       0 as UNLOADCNT204 , " _
                   & "       0 as UNLOADCNTCHO204 , " _
                   & "       0 as UNLOADCNTTTL204 , " _
                   & "       '' as OILPAYKBN205 , " _
                   & "       '' as OILPAYKBNNAMES205 , " _
                   & "       0  as HAIDISTANCE205 , " _
                   & "       0 as HAIDISTANCECHO205 , " _
                   & "       0 as HAIDISTANCETTL205 , " _
                   & "       0 as KAIDISTANCE205 , " _
                   & "       0 as KAIDISTANCECHO205 , " _
                   & "       0 as KAIDISTANCETTL205 , " _
                   & "       0 as UNLOADCNT205 , " _
                   & "       0 as UNLOADCNTCHO205 , " _
                   & "       0 as UNLOADCNTTTL205 , " _
                   & "       '' as OILPAYKBN206 , " _
                   & "       '' as OILPAYKBNNAMES206 , " _
                   & "       0  as HAIDISTANCE206 , " _
                   & "       0 as HAIDISTANCECHO206 , " _
                   & "       0 as HAIDISTANCETTL206 , " _
                   & "       0 as KAIDISTANCE206 , " _
                   & "       0 as KAIDISTANCECHO206 , " _
                   & "       0 as KAIDISTANCETTL206 , " _
                   & "       0 as UNLOADCNT206 , " _
                   & "       0 as UNLOADCNTCHO206 , " _
                   & "       0 as UNLOADCNTTTL206 , " _
                   & "       '' as OILPAYKBN207 , " _
                   & "       '' as OILPAYKBNNAMES207 , " _
                   & "       0  as HAIDISTANCE207 , " _
                   & "       0 as HAIDISTANCECHO207 , " _
                   & "       0 as HAIDISTANCETTL207 , " _
                   & "       0 as KAIDISTANCE207 , " _
                   & "       0 as KAIDISTANCECHO207 , " _
                   & "       0 as KAIDISTANCETTL207 , " _
                   & "       0 as UNLOADCNT207 , " _
                   & "       0 as UNLOADCNTCHO207 , " _
                   & "       0 as UNLOADCNTTTL207 , " _
                   & "       '' as OILPAYKBN208 , " _
                   & "       '' as OILPAYKBNNAMES208 , " _
                   & "       0  as HAIDISTANCE208 , " _
                   & "       0 as HAIDISTANCECHO208 , " _
                   & "       0 as HAIDISTANCETTL208 , " _
                   & "       0 as KAIDISTANCE208 , " _
                   & "       0 as KAIDISTANCECHO208 , " _
                   & "       0 as KAIDISTANCETTL208 , " _
                   & "       0 as UNLOADCNT208 , " _
                   & "       0 as UNLOADCNTCHO208 , " _
                   & "       0 as UNLOADCNTTTL208 , " _
                   & "       '' as OILPAYKBN209 , " _
                   & "       '' as OILPAYKBNNAMES209 , " _
                   & "       0  as HAIDISTANCE209 , " _
                   & "       0 as HAIDISTANCECHO209 , " _
                   & "       0 as HAIDISTANCETTL209 , " _
                   & "       0 as KAIDISTANCE209 , " _
                   & "       0 as KAIDISTANCECHO209 , " _
                   & "       0 as KAIDISTANCETTL209 , " _
                   & "       0 as UNLOADCNT209 , " _
                   & "       0 as UNLOADCNTCHO209 , " _
                   & "       0 as UNLOADCNTTTL209 , " _
                   & "       '' as OILPAYKBN210 , " _
                   & "       '' as OILPAYKBNNAMES210 , " _
                   & "       0  as HAIDISTANCE210 , " _
                   & "       0 as HAIDISTANCECHO210 , " _
                   & "       0 as HAIDISTANCETTL210 , " _
                   & "       0 as KAIDISTANCE210 , " _
                   & "       0 as KAIDISTANCECHO210 , " _
                   & "       0 as KAIDISTANCETTL210 , " _
                   & "       0 as UNLOADCNT210 , " _
                   & "       0 as UNLOADCNTCHO210 , " _
                   & "       0 as UNLOADCNTTTL210 , " _
                   & "       'K' as DATAKBN  " _
                   & " FROM      #MBtemp        MB " _
                   & " INNER JOIN T0007_KINTAI  A " _
                   & "   ON    A.CAMPCODE     = @CAMPCODE " _
                   & "   and   A.TAISHOYM     = @TAISHOYM " _
                   & "   and   A.STAFFCODE    = MB.STAFFCODE " _
                   & "   and   A.RECODEKBN    = '2' " _
                   & "   and   A.DELFLG      <> '1' " _
                   & " LEFT JOIN MB001_STAFF    MB2 " _
                   & "   ON    MB2.CAMPCODE     = @CAMPCODE " _
                   & "   and   MB2.STAFFCODE    = MB.STAFFCODE " _
                   & "   and   MB2.STYMD       <= A.WORKDATE " _
                   & "   and   MB2.ENDYMD      >= A.WORKDATE " _
                   & "   and   MB2.STYMD        = (SELECT MAX(STYMD) FROM MB001_STAFF WHERE CAMPCODE = @CAMPCODE and STAFFCODE = MB.STAFFCODE and STYMD <= A.WORKDATE and ENDYMD >= A.WORKDATE and DELFLG <> '1' ) " _
                   & "   and   MB2.DELFLG      <> '1' " _
                   & " LEFT JOIN MB005_CALENDAR CAL " _
                   & "   ON    CAL.CAMPCODE    = @CAMPCODE " _
                   & "   and   CAL.WORKINGYMD  = A.WORKDATE " _
                   & "   and   CAL.DELFLG     <> '1' " _
                   & " LEFT JOIN MB004_WORKINGH B4 " _
                   & "   ON    B4.CAMPCODE    = @CAMPCODE " _
                   & "   and   B4.HORG        = MB2.HORG " _
                   & "   and   B4.STAFFKBN    = MB2.STAFFKBN " _
                   & "   and   B4.STYMD      <= @STYMD " _
                   & "   and   B4.ENDYMD     >= @ENDYMD " _
                   & "   and   B4.STYMD      = (SELECT MAX(STYMD) FROM MB004_WORKINGH WHERE CAMPCODE = @CAMPCODE and HORG = MB2.HORG and STAFFKBN = MB2.STAFFKBN and STYMD <= @STYMD and ENDYMD >= @ENDYMD and DELFLG <> '1') " _
                   & "   and   B4.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F1 " _
                   & "   ON    F1.CAMPCODE    = @CAMPCODE " _
                   & "   and   F1.CLASS       = 'WORKINGWEEK' " _
                   & "   and   F1.KEYCODE     = CAL.WORKINGWEEK " _
                   & "   and   F1.STYMD      <= @STYMD " _
                   & "   and   F1.ENDYMD     >= @ENDYMD " _
                   & "   and   F1.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F3 " _
                   & "   ON    F3.CAMPCODE    = @CAMPCODE " _
                   & "   and   F3.CLASS       = 'RECODEKBN' " _
                   & "   and   F3.KEYCODE     = A.RECODEKBN " _
                   & "   and   F3.STYMD      <= @STYMD " _
                   & "   and   F3.ENDYMD     >= @ENDYMD " _
                   & "   and   F3.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F4 " _
                   & "   ON    F4.CAMPCODE    = @CAMPCODE " _
                   & "   and   F4.CLASS       = 'WORKKBN' " _
                   & "   and   F4.KEYCODE     = A.WORKKBN " _
                   & "   and   F4.STYMD      <= @STYMD " _
                   & "   and   F4.ENDYMD     >= @ENDYMD " _
                   & "   and   F4.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F6 " _
                   & "   ON    F6.CAMPCODE    = @CAMPCODE " _
                   & "   and   F6.CLASS       = 'SHARYOKBN' " _
                   & "   and   F6.KEYCODE     = A.SHARYOKBN " _
                   & "   and   F6.STYMD      <= @STYMD " _
                   & "   and   F6.ENDYMD     >= @ENDYMD " _
                   & "   and   F6.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F7 " _
                   & "   ON    F7.CAMPCODE    = @CAMPCODE " _
                   & "   and   F7.CLASS       = 'OILPAYKBN' " _
                   & "   and   F7.KEYCODE     = A.OILPAYKBN " _
                   & "   and   F7.STYMD      <= @STYMD " _
                   & "   and   F7.ENDYMD     >= @ENDYMD " _
                   & "   and   F7.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F8 " _
                   & "   ON    F8.CAMPCODE    = @CAMPCODE " _
                   & "   and   F8.CLASS       = 'STAFFKBN' " _
                   & "   and   F8.KEYCODE     = MB2.STAFFKBN " _
                   & "   and   F8.STYMD      <= @STYMD " _
                   & "   and   F8.ENDYMD     >= @ENDYMD " _
                   & "   and   F8.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F9 " _
                   & "   ON    F9.CAMPCODE    = @CAMPCODE " _
                   & "   and   F9.CLASS       = 'HOLIDAYKBN' " _
                   & "   and   F9.KEYCODE     = A.HOLIDAYKBN " _
                   & "   and   F9.STYMD      <= @STYMD " _
                   & "   and   F9.ENDYMD     >= @ENDYMD " _
                   & "   and   F9.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F10 " _
                   & "   ON    F10.CAMPCODE    = @CAMPCODE " _
                   & "   and   F10.CLASS       = 'PAYKBN' " _
                   & "   and   F10.KEYCODE     = A.PAYKBN " _
                   & "   and   F10.STYMD      <= @STYMD " _
                   & "   and   F10.ENDYMD     >= @ENDYMD " _
                   & "   and   F10.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F11 " _
                   & "   ON    F11.CAMPCODE    = @CAMPCODE " _
                   & "   and   F11.CLASS       = 'SHUKCHOKKBN' " _
                   & "   and   F11.KEYCODE     = A.SHUKCHOKKBN " _
                   & "   and   F11.STYMD      <= @STYMD " _
                   & "   and   F11.ENDYMD     >= @ENDYMD " _
                   & "   and   F11.DELFLG     <> '1' " _
                   & " WHERE   MB.CAMPCODE     = @CAMPCODE " _
                   & ") TBL " _
                   & "WHERE 1 = 1 "

                Dim WW_SORT As String = "ORDER BY HORG, STAFFCODE, WORKDATE, HDKBN DESC, RECODEKBN, STDATE, STTIME, ENDDATE, ENDTIME"

                SQLStr = SQLStr & WW_SORT

                Using SQLcmd0 As New SqlCommand(SQLStr0, SQLcon), SQLcmd1 As New SqlCommand(SQLStr1, SQLcon), SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim P2_CAMPCODE As SqlParameter = SQLcmd1.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar, 20)
                    Dim P2_SEL_STYMD As SqlParameter = SQLcmd1.Parameters.Add("@SEL_STYMD", System.Data.SqlDbType.Date)
                    Dim P2_SEL_ENDYMD As SqlParameter = SQLcmd1.Parameters.Add("@SEL_ENDYMD", System.Data.SqlDbType.Date)
                    Dim P2_HORG As SqlParameter = SQLcmd1.Parameters.Add("@HORG", System.Data.SqlDbType.NVarChar, 20)
                    Dim P2_TERMID As SqlParameter = SQLcmd1.Parameters.Add("@TERMID", System.Data.SqlDbType.NVarChar, 20)
                    Dim P2_NOW As SqlParameter = SQLcmd1.Parameters.Add("@NOW", System.Data.SqlDbType.NVarChar)

                    Dim P_CAMPCODE As SqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar, 20)
                    Dim P_TAISHOYM As SqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", System.Data.SqlDbType.NVarChar, 7)
                    Dim P_STYMD As SqlParameter = SQLcmd.Parameters.Add("@STYMD", System.Data.SqlDbType.Date)
                    Dim P_ENDYMD As SqlParameter = SQLcmd.Parameters.Add("@ENDYMD", System.Data.SqlDbType.Date)
                    Dim P_SEL_STYMD As SqlParameter = SQLcmd.Parameters.Add("@SEL_STYMD", System.Data.SqlDbType.Date)
                    Dim P_SEL_ENDYMD As SqlParameter = SQLcmd.Parameters.Add("@SEL_ENDYMD", System.Data.SqlDbType.Date)
                    '〇ワークテーブルの作成
                    SQLcmd0.CommandTimeout = 300
                    SQLcmd0.ExecuteNonQuery()

                    P2_CAMPCODE.Value = work.WF_SEL_CAMPCODE.Text
                    P2_SEL_STYMD.Value = work.WF_SEL_TAISHOYM.Text & "/01"
                    Dim wDATE2 As Date
                    Try
                        wDATE2 = work.WF_SEL_TAISHOYM.Text & "/01"
                    Catch ex As Exception
                        wDATE2 = Date.Now
                    End Try
                    P2_SEL_ENDYMD.Value = work.WF_SEL_TAISHOYM.Text & "/" & DateTime.DaysInMonth(wDATE2.Year, wDATE2.Month).ToString("00")
                    Dim orgCode As String = ""
                    Dim retCode As String = ""
                    T0007COM.ConvORGCODE(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_HORG.Text, orgCode, retCode)
                    If retCode = C_MESSAGE_NO.NORMAL Then
                        P2_HORG.Value = orgCode
                    Else
                        P2_HORG.Value = work.WF_SEL_HORG.Text
                    End If
                    P2_TERMID.Value = CS0050Session.APSV_ID

                    P2_NOW.Value = Date.Now

                    Using SQLdr1 As SqlDataReader = SQLcmd1.ExecuteReader(), WW_MBtbl As DataTable = New DataTable
                        WW_MBtbl.Columns.Add("CAMPCODE", GetType(String))
                        WW_MBtbl.Columns.Add("STAFFCODE", GetType(String))

                        WW_MBtbl.Load(SQLdr1)
                        '一旦テンポラリテーブルに出力
                        Using bc As New SqlClient.SqlBulkCopy(SQLcon)
                            bc.DestinationTableName = "#MBtemp"
                            bc.WriteToServer(WW_MBtbl)
                            bc.Close()
                        End Using
                    End Using


                    P_CAMPCODE.Value = work.WF_SEL_CAMPCODE.Text
                    P_TAISHOYM.Value = work.WF_SEL_TAISHOYM.Text
                    P_STYMD.Value = Date.Now
                    P_ENDYMD.Value = Date.Now
                    P_SEL_STYMD.Value = work.WF_SEL_TAISHOYM.Text & "/01"
                    Dim wDATE As Date
                    Try
                        wDATE = work.WF_SEL_TAISHOYM.Text & "/01"
                    Catch ex As Exception
                        wDATE = Date.Now
                    End Try
                    P_SEL_ENDYMD.Value = work.WF_SEL_TAISHOYM.Text & "/" & DateTime.DaysInMonth(wDATE.Year, wDATE.Month).ToString("00")
                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        '■テーブル検索結果をテーブル格納
                        TA0007ALL.Load(SQLdr)
                    End Using
                    Dim WW_LINEcnt As Integer = 0
                    For Each TA0007ALLrow As DataRow In TA0007ALL.Rows

                        If TA0007ALLrow("HDKBN") = "H" Then
                            TA0007ALLrow("SELECT") = "1"
                            TA0007ALLrow("HIDDEN") = "0"      '表示
                            WW_LINEcnt += 1
                            TA0007ALLrow("LINECNT") = WW_LINEcnt
                        Else
                            TA0007ALLrow("SELECT") = "0"
                            TA0007ALLrow("HIDDEN") = "1"      '非表示
                            TA0007ALLrow("LINECNT") = 0
                        End If

                        TA0007ALLrow("SEQ") = CInt(TA0007ALLrow("SEQ")).ToString("000")
                        If IsDate(TA0007ALLrow("WORKDATE")) Then
                            TA0007ALLrow("WORKDATE") = CDate(TA0007ALLrow("WORKDATE")).ToString("yyyy/MM/dd")
                        Else
                            TA0007ALLrow("WORKDATE") = ""
                        End If
                        If IsDate(TA0007ALLrow("STDATE")) Then
                            TA0007ALLrow("STDATE") = CDate(TA0007ALLrow("STDATE")).ToString("yyyy/MM/dd")
                        Else
                            TA0007ALLrow("STDATE") = ""
                        End If
                        If IsDate(TA0007ALLrow("STTIME")) Then
                            TA0007ALLrow("STTIME") = CDate(TA0007ALLrow("STTIME")).ToString("HH:mm")
                        Else
                            TA0007ALLrow("STTIME") = ""
                        End If
                        If IsDate(TA0007ALLrow("ENDDATE")) Then
                            TA0007ALLrow("ENDDATE") = CDate(TA0007ALLrow("ENDDATE")).ToString("yyyy/MM/dd")
                        Else
                            TA0007ALLrow("ENDDATE") = ""
                        End If
                        If IsDate(TA0007ALLrow("ENDTIME")) Then
                            TA0007ALLrow("ENDTIME") = CDate(TA0007ALLrow("ENDTIME")).ToString("HH:mm")
                        Else
                            TA0007ALLrow("ENDTIME") = ""
                        End If
                        If IsDate(TA0007ALLrow("BINDSTDATE")) Then
                            TA0007ALLrow("BINDSTDATE") = CDate(TA0007ALLrow("BINDSTDATE")).ToString("HH:mm")
                        Else
                            TA0007ALLrow("BINDSTDATE") = ""
                        End If

                        TA0007ALLrow("WORKTIME") = T0009TIME.MinutesToHHMM(TA0007ALLrow("WORKTIME"))
                        TA0007ALLrow("MOVETIME") = T0009TIME.MinutesToHHMM(TA0007ALLrow("MOVETIME"))
                        TA0007ALLrow("ACTTIME") = T0009TIME.MinutesToHHMM(TA0007ALLrow("ACTTIME"))
                        TA0007ALLrow("BINDTIME") = T0009TIME.MinutesToHHMM(TA0007ALLrow("BINDTIME"))
                        TA0007ALLrow("BREAKTIME") = T0009TIME.MinutesToHHMM(TA0007ALLrow("BREAKTIME"))
                        TA0007ALLrow("BREAKTIMECHO") = T0009TIME.MinutesToHHMM(TA0007ALLrow("BREAKTIMECHO"))
                        TA0007ALLrow("BREAKTIMETTL") = T0009TIME.MinutesToHHMM(TA0007ALLrow("BREAKTIMETTL"))
                        TA0007ALLrow("NIGHTTIME") = T0009TIME.MinutesToHHMM(TA0007ALLrow("NIGHTTIME"))
                        TA0007ALLrow("NIGHTTIMECHO") = T0009TIME.MinutesToHHMM(TA0007ALLrow("NIGHTTIMECHO"))

                        '時間外計算対象外を判定し、対象外の場合は深夜ゼロとして、所定内深夜に設定する
                        Dim WW_FIND As String = "OFF"
                        Dim WW_TEXT As String = String.Empty
                        Dim WW_RTN As String = C_MESSAGE_NO.NORMAL
                        CodeToName("OVERTIMESTAFFKBN", TA0007ALLrow("STAFFKBN"), WW_TEXT, WW_RTN)
                        If Not String.IsNullOrEmpty(WW_TEXT) Then WW_FIND = "ON"

                        If WW_FIND = "ON" Then
                            '所定内深夜に平日深夜、休日深夜、日曜深夜を設定
                            TA0007ALLrow("NIGHTTIMETTL") = T0009TIME.MinutesToHHMM(Val(TA0007ALLrow("WNIGHTTIMETTL")) + Val(TA0007ALLrow("HNIGHTTIMETTL")) + Val(TA0007ALLrow("SNIGHTTIMETTL")))
                        Else
                            '所定内深夜そのまま
                            TA0007ALLrow("NIGHTTIMETTL") = T0009TIME.MinutesToHHMM(TA0007ALLrow("NIGHTTIMETTL"))
                        End If

                        TA0007ALLrow("ORVERTIME") = T0009TIME.MinutesToHHMM(TA0007ALLrow("ORVERTIME"))
                        TA0007ALLrow("ORVERTIMECHO") = T0009TIME.MinutesToHHMM(TA0007ALLrow("ORVERTIMECHO"))
                        TA0007ALLrow("ORVERTIMETTL") = T0009TIME.MinutesToHHMM(TA0007ALLrow("ORVERTIMETTL"))
                        TA0007ALLrow("WNIGHTTIME") = T0009TIME.MinutesToHHMM(TA0007ALLrow("WNIGHTTIME"))
                        TA0007ALLrow("WNIGHTTIMECHO") = T0009TIME.MinutesToHHMM(TA0007ALLrow("WNIGHTTIMECHO"))

                        If WW_FIND = "ON" Then
                            '平日深夜にゼロ
                            TA0007ALLrow("WNIGHTTIMETTL") = T0009TIME.MinutesToHHMM(0)
                        Else
                            '平日深夜に休日深夜を加算
                            TA0007ALLrow("WNIGHTTIMETTL") = T0009TIME.MinutesToHHMM(Val(TA0007ALLrow("WNIGHTTIMETTL")) + Val(TA0007ALLrow("HNIGHTTIMETTL")))
                        End If


                        TA0007ALLrow("SWORKTIME") = T0009TIME.MinutesToHHMM(TA0007ALLrow("SWORKTIME"))
                        TA0007ALLrow("SWORKTIMECHO") = T0009TIME.MinutesToHHMM(TA0007ALLrow("SWORKTIMECHO"))
                        TA0007ALLrow("SWORKTIMETTL") = T0009TIME.MinutesToHHMM(TA0007ALLrow("SWORKTIMETTL"))
                        TA0007ALLrow("SNIGHTTIME") = T0009TIME.MinutesToHHMM(TA0007ALLrow("SNIGHTTIME"))
                        TA0007ALLrow("SNIGHTTIMECHO") = T0009TIME.MinutesToHHMM(TA0007ALLrow("SNIGHTTIMECHO"))
                        If WW_FIND = "ON" Then
                            '日曜深夜にゼロを設定
                            TA0007ALLrow("SNIGHTTIMETTL") = T0009TIME.MinutesToHHMM(0)
                        Else
                            '日曜深夜そのまま
                            TA0007ALLrow("SNIGHTTIMETTL") = T0009TIME.MinutesToHHMM(TA0007ALLrow("SNIGHTTIMETTL"))
                        End If
                        TA0007ALLrow("HWORKTIME") = T0009TIME.MinutesToHHMM(TA0007ALLrow("HWORKTIME"))
                        TA0007ALLrow("HWORKTIMECHO") = T0009TIME.MinutesToHHMM(TA0007ALLrow("HWORKTIMECHO"))
                        TA0007ALLrow("HWORKTIMETTL") = T0009TIME.MinutesToHHMM(TA0007ALLrow("HWORKTIMETTL"))
                        TA0007ALLrow("HNIGHTTIME") = T0009TIME.MinutesToHHMM(TA0007ALLrow("HNIGHTTIME"))
                        TA0007ALLrow("HNIGHTTIMECHO") = T0009TIME.MinutesToHHMM(TA0007ALLrow("HNIGHTTIMECHO"))
                        '休日深夜にゼロを設定
                        TA0007ALLrow("HNIGHTTIMETTL") = T0009TIME.MinutesToHHMM(0)
                        TA0007ALLrow("HOANTIME") = T0009TIME.MinutesToHHMM(TA0007ALLrow("HOANTIME"))
                        TA0007ALLrow("HOANTIMECHO") = T0009TIME.MinutesToHHMM(TA0007ALLrow("HOANTIMECHO"))
                        TA0007ALLrow("HOANTIMETTL") = T0009TIME.MinutesToHHMM(TA0007ALLrow("HOANTIMETTL"))
                        TA0007ALLrow("KOATUTIME") = T0009TIME.MinutesToHHMM(TA0007ALLrow("KOATUTIME"))
                        TA0007ALLrow("KOATUTIMECHO") = T0009TIME.MinutesToHHMM(TA0007ALLrow("KOATUTIMECHO"))
                        TA0007ALLrow("KOATUTIMETTL") = T0009TIME.MinutesToHHMM(TA0007ALLrow("KOATUTIMETTL"))
                        TA0007ALLrow("TOKUSA1TIME") = T0009TIME.MinutesToHHMM(TA0007ALLrow("TOKUSA1TIME"))
                        TA0007ALLrow("TOKUSA1TIMECHO") = T0009TIME.MinutesToHHMM(TA0007ALLrow("TOKUSA1TIMECHO"))
                        TA0007ALLrow("TOKUSA1TIMETTL") = T0009TIME.MinutesToHHMM(TA0007ALLrow("TOKUSA1TIMETTL"))
                        TA0007ALLrow("HAYADETIME") = T0009TIME.MinutesToHHMM(TA0007ALLrow("HAYADETIME"))
                        TA0007ALLrow("HAYADETIMECHO") = T0009TIME.MinutesToHHMM(TA0007ALLrow("HAYADETIMECHO"))
                        TA0007ALLrow("HAYADETIMETTL") = T0009TIME.MinutesToHHMM(TA0007ALLrow("HAYADETIMETTL"))
                        TA0007ALLrow("JIKYUSHATIME") = T0009TIME.MinutesToHHMM(TA0007ALLrow("JIKYUSHATIME"))
                        TA0007ALLrow("JIKYUSHATIMECHO") = T0009TIME.MinutesToHHMM(TA0007ALLrow("JIKYUSHATIMECHO"))
                        TA0007ALLrow("JIKYUSHATIMETTL") = T0009TIME.MinutesToHHMM(TA0007ALLrow("JIKYUSHATIMETTL"))
                        TA0007ALLrow("HDAIWORKTIME") = T0009TIME.MinutesToHHMM(TA0007ALLrow("HDAIWORKTIME"))
                        TA0007ALLrow("HDAIWORKTIMECHO") = T0009TIME.MinutesToHHMM(TA0007ALLrow("HDAIWORKTIMECHO"))
                        TA0007ALLrow("HDAIWORKTIMETTL") = T0009TIME.MinutesToHHMM(TA0007ALLrow("HDAIWORKTIMETTL"))
                        TA0007ALLrow("HDAINIGHTTIME") = T0009TIME.MinutesToHHMM(TA0007ALLrow("HDAINIGHTTIME"))
                        TA0007ALLrow("HDAINIGHTTIMECHO") = T0009TIME.MinutesToHHMM(TA0007ALLrow("HDAINIGHTTIMECHO"))
                        TA0007ALLrow("HDAINIGHTTIMETTL") = T0009TIME.MinutesToHHMM(TA0007ALLrow("HDAINIGHTTIMETTL"))
                        TA0007ALLrow("SDAIWORKTIME") = T0009TIME.MinutesToHHMM(TA0007ALLrow("SDAIWORKTIME"))
                        TA0007ALLrow("SDAIWORKTIMECHO") = T0009TIME.MinutesToHHMM(TA0007ALLrow("SDAIWORKTIMECHO"))
                        TA0007ALLrow("SDAIWORKTIMETTL") = T0009TIME.MinutesToHHMM(TA0007ALLrow("SDAIWORKTIMETTL"))
                        TA0007ALLrow("SDAINIGHTTIME") = T0009TIME.MinutesToHHMM(TA0007ALLrow("SDAINIGHTTIME"))
                        TA0007ALLrow("SDAINIGHTTIMECHO") = T0009TIME.MinutesToHHMM(TA0007ALLrow("SDAINIGHTTIMECHO"))
                        TA0007ALLrow("SDAINIGHTTIMETTL") = T0009TIME.MinutesToHHMM(TA0007ALLrow("SDAINIGHTTIMETTL"))

                        TA0007ALLrow("HAIDISTANCE") = Val(TA0007ALLrow("HAIDISTANCE"))
                        TA0007ALLrow("HAIDISTANCECHO") = Val(TA0007ALLrow("HAIDISTANCECHO"))
                        TA0007ALLrow("HAIDISTANCETTL") = Val(TA0007ALLrow("HAIDISTANCETTL"))
                        TA0007ALLrow("KAIDISTANCE") = Val(TA0007ALLrow("KAIDISTANCE"))
                        TA0007ALLrow("KAIDISTANCECHO") = Val(TA0007ALLrow("KAIDISTANCECHO"))
                        TA0007ALLrow("KAIDISTANCETTL") = Val(TA0007ALLrow("KAIDISTANCETTL"))

                        If TA0007ALLrow("HDKBN") = "H" Then
                            For j As Integer = 0 To TA0007ALL.Rows.Count - 1
                                Dim WW_DTLrow As DataRow = TA0007ALL.Rows(j)
                                '次のヘッダまで
                                If WW_DTLrow("HDKBN") = "H" Then Continue For

                                If IsDate(WW_DTLrow("WORKDATE")) Then
                                    WW_DTLrow("WORKDATE") = CDate(WW_DTLrow("WORKDATE")).ToString("yyyy/MM/dd")
                                Else
                                    WW_DTLrow("WORKDATE") = ""
                                End If

                                If TA0007ALLrow("WORKDATE") = WW_DTLrow("WORKDATE") AndAlso
                                TA0007ALLrow("STAFFCODE") = WW_DTLrow("STAFFCODE") Then

                                    Select Case WW_DTLrow("SHARYOKBN")
                                        Case "1"
                                            TA0007ALLrow("SHARYOKBN1") = WW_DTLrow("SHARYOKBN")
                                            TA0007ALLrow("SHARYOKBNNAMES1") = WW_DTLrow("SHARYOKBNNAMES")

                                            Select Case WW_DTLrow("OILPAYKBN")
                                                Case "01"
                                                    TA0007ALLrow("OILPAYKBN101") = WW_DTLrow("OILPAYKBN")
                                                    TA0007ALLrow("OILPAYKBNNAMES101") = WW_DTLrow("OILPAYKBNNAMES")
                                                    TA0007ALLrow("HAIDISTANCE101") = WW_DTLrow("HAIDISTANCE")
                                                    TA0007ALLrow("HAIDISTANCECHO101") = WW_DTLrow("HAIDISTANCECHO")
                                                    TA0007ALLrow("HAIDISTANCETTL101") = WW_DTLrow("HAIDISTANCETTL")
                                                    TA0007ALLrow("KAIDISTANCE101") = WW_DTLrow("KAIDISTANCE")
                                                    TA0007ALLrow("KAIDISTANCECHO101") = WW_DTLrow("KAIDISTANCECHO")
                                                    TA0007ALLrow("KAIDISTANCETTL101") = WW_DTLrow("KAIDISTANCETTL")
                                                    TA0007ALLrow("UNLOADCNT101") = WW_DTLrow("UNLOADCNT")
                                                    TA0007ALLrow("UNLOADCNTCHO101") = WW_DTLrow("UNLOADCNTCHO")
                                                    TA0007ALLrow("UNLOADCNTTTL101") = WW_DTLrow("UNLOADCNTTTL")
                                                Case "02"
                                                    TA0007ALLrow("OILPAYKBN102") = WW_DTLrow("OILPAYKBN")
                                                    TA0007ALLrow("OILPAYKBNNAMES102") = WW_DTLrow("OILPAYKBNNAMES")
                                                    TA0007ALLrow("HAIDISTANCE102") = WW_DTLrow("HAIDISTANCE")
                                                    TA0007ALLrow("HAIDISTANCECHO102") = WW_DTLrow("HAIDISTANCECHO")
                                                    TA0007ALLrow("HAIDISTANCETTL102") = WW_DTLrow("HAIDISTANCETTL")
                                                    TA0007ALLrow("KAIDISTANCE102") = WW_DTLrow("KAIDISTANCE")
                                                    TA0007ALLrow("KAIDISTANCECHO102") = WW_DTLrow("KAIDISTANCECHO")
                                                    TA0007ALLrow("KAIDISTANCETTL102") = WW_DTLrow("KAIDISTANCETTL")
                                                    TA0007ALLrow("UNLOADCNT102") = WW_DTLrow("UNLOADCNT")
                                                    TA0007ALLrow("UNLOADCNTCHO102") = WW_DTLrow("UNLOADCNTCHO")
                                                    TA0007ALLrow("UNLOADCNTTTL102") = WW_DTLrow("UNLOADCNTTTL")
                                                Case "03"
                                                    TA0007ALLrow("OILPAYKBN103") = WW_DTLrow("OILPAYKBN")
                                                    TA0007ALLrow("OILPAYKBNNAMES103") = WW_DTLrow("OILPAYKBNNAMES")
                                                    TA0007ALLrow("HAIDISTANCE103") = WW_DTLrow("HAIDISTANCE")
                                                    TA0007ALLrow("HAIDISTANCECHO103") = WW_DTLrow("HAIDISTANCECHO")
                                                    TA0007ALLrow("HAIDISTANCETTL103") = WW_DTLrow("HAIDISTANCETTL")
                                                    TA0007ALLrow("KAIDISTANCE103") = WW_DTLrow("KAIDISTANCE")
                                                    TA0007ALLrow("KAIDISTANCECHO103") = WW_DTLrow("KAIDISTANCECHO")
                                                    TA0007ALLrow("KAIDISTANCETTL103") = WW_DTLrow("KAIDISTANCETTL")
                                                    TA0007ALLrow("UNLOADCNT103") = WW_DTLrow("UNLOADCNT")
                                                    TA0007ALLrow("UNLOADCNTCHO103") = WW_DTLrow("UNLOADCNTCHO")
                                                    TA0007ALLrow("UNLOADCNTTTL103") = WW_DTLrow("UNLOADCNTTTL")
                                                Case "04"
                                                    TA0007ALLrow("OILPAYKBN104") = WW_DTLrow("OILPAYKBN")
                                                    TA0007ALLrow("OILPAYKBNNAMES104") = WW_DTLrow("OILPAYKBNNAMES")
                                                    TA0007ALLrow("HAIDISTANCE104") = WW_DTLrow("HAIDISTANCE")
                                                    TA0007ALLrow("HAIDISTANCECHO104") = WW_DTLrow("HAIDISTANCECHO")
                                                    TA0007ALLrow("HAIDISTANCETTL104") = WW_DTLrow("HAIDISTANCETTL")
                                                    TA0007ALLrow("KAIDISTANCE104") = WW_DTLrow("KAIDISTANCE")
                                                    TA0007ALLrow("KAIDISTANCECHO104") = WW_DTLrow("KAIDISTANCECHO")
                                                    TA0007ALLrow("KAIDISTANCETTL104") = WW_DTLrow("KAIDISTANCETTL")
                                                    TA0007ALLrow("UNLOADCNT104") = WW_DTLrow("UNLOADCNT")
                                                    TA0007ALLrow("UNLOADCNTCHO104") = WW_DTLrow("UNLOADCNTCHO")
                                                    TA0007ALLrow("UNLOADCNTTTL104") = WW_DTLrow("UNLOADCNTTTL")
                                                Case "05"
                                                    TA0007ALLrow("OILPAYKBN105") = WW_DTLrow("OILPAYKBN")
                                                    TA0007ALLrow("OILPAYKBNNAMES105") = WW_DTLrow("OILPAYKBNNAMES")
                                                    TA0007ALLrow("HAIDISTANCE105") = WW_DTLrow("HAIDISTANCE")
                                                    TA0007ALLrow("HAIDISTANCECHO105") = WW_DTLrow("HAIDISTANCECHO")
                                                    TA0007ALLrow("HAIDISTANCETTL105") = WW_DTLrow("HAIDISTANCETTL")
                                                    TA0007ALLrow("KAIDISTANCE105") = WW_DTLrow("KAIDISTANCE")
                                                    TA0007ALLrow("KAIDISTANCECHO105") = WW_DTLrow("KAIDISTANCECHO")
                                                    TA0007ALLrow("KAIDISTANCETTL105") = WW_DTLrow("KAIDISTANCETTL")
                                                    TA0007ALLrow("UNLOADCNT105") = WW_DTLrow("UNLOADCNT")
                                                    TA0007ALLrow("UNLOADCNTCHO105") = WW_DTLrow("UNLOADCNTCHO")
                                                    TA0007ALLrow("UNLOADCNTTTL105") = WW_DTLrow("UNLOADCNTTTL")
                                                Case "06"
                                                    TA0007ALLrow("OILPAYKBN106") = WW_DTLrow("OILPAYKBN")
                                                    TA0007ALLrow("OILPAYKBNNAMES106") = WW_DTLrow("OILPAYKBNNAMES")
                                                    TA0007ALLrow("HAIDISTANCE106") = WW_DTLrow("HAIDISTANCE")
                                                    TA0007ALLrow("HAIDISTANCECHO106") = WW_DTLrow("HAIDISTANCECHO")
                                                    TA0007ALLrow("HAIDISTANCETTL106") = WW_DTLrow("HAIDISTANCETTL")
                                                    TA0007ALLrow("KAIDISTANCE106") = WW_DTLrow("KAIDISTANCE")
                                                    TA0007ALLrow("KAIDISTANCECHO106") = WW_DTLrow("KAIDISTANCECHO")
                                                    TA0007ALLrow("KAIDISTANCETTL106") = WW_DTLrow("KAIDISTANCETTL")
                                                    TA0007ALLrow("UNLOADCNT106") = WW_DTLrow("UNLOADCNT")
                                                    TA0007ALLrow("UNLOADCNTCHO106") = WW_DTLrow("UNLOADCNTCHO")
                                                    TA0007ALLrow("UNLOADCNTTTL106") = WW_DTLrow("UNLOADCNTTTL")
                                                Case "07"
                                                    TA0007ALLrow("OILPAYKBN107") = WW_DTLrow("OILPAYKBN")
                                                    TA0007ALLrow("OILPAYKBNNAMES107") = WW_DTLrow("OILPAYKBNNAMES")
                                                    TA0007ALLrow("HAIDISTANCE107") = WW_DTLrow("HAIDISTANCE")
                                                    TA0007ALLrow("HAIDISTANCECHO107") = WW_DTLrow("HAIDISTANCECHO")
                                                    TA0007ALLrow("HAIDISTANCETTL107") = WW_DTLrow("HAIDISTANCETTL")
                                                    TA0007ALLrow("KAIDISTANCE107") = WW_DTLrow("KAIDISTANCE")
                                                    TA0007ALLrow("KAIDISTANCECHO107") = WW_DTLrow("KAIDISTANCECHO")
                                                    TA0007ALLrow("KAIDISTANCETTL107") = WW_DTLrow("KAIDISTANCETTL")
                                                    TA0007ALLrow("UNLOADCNT107") = WW_DTLrow("UNLOADCNT")
                                                    TA0007ALLrow("UNLOADCNTCHO107") = WW_DTLrow("UNLOADCNTCHO")
                                                    TA0007ALLrow("UNLOADCNTTTL107") = WW_DTLrow("UNLOADCNTTTL")
                                                Case "08"
                                                    TA0007ALLrow("OILPAYKBN108") = WW_DTLrow("OILPAYKBN")
                                                    TA0007ALLrow("OILPAYKBNNAMES108") = WW_DTLrow("OILPAYKBNNAMES")
                                                    TA0007ALLrow("HAIDISTANCE108") = WW_DTLrow("HAIDISTANCE")
                                                    TA0007ALLrow("HAIDISTANCECHO108") = WW_DTLrow("HAIDISTANCECHO")
                                                    TA0007ALLrow("HAIDISTANCETTL108") = WW_DTLrow("HAIDISTANCETTL")
                                                    TA0007ALLrow("KAIDISTANCE108") = WW_DTLrow("KAIDISTANCE")
                                                    TA0007ALLrow("KAIDISTANCECHO108") = WW_DTLrow("KAIDISTANCECHO")
                                                    TA0007ALLrow("KAIDISTANCETTL108") = WW_DTLrow("KAIDISTANCETTL")
                                                    TA0007ALLrow("UNLOADCNT108") = WW_DTLrow("UNLOADCNT")
                                                    TA0007ALLrow("UNLOADCNTCHO108") = WW_DTLrow("UNLOADCNTCHO")
                                                    TA0007ALLrow("UNLOADCNTTTL108") = WW_DTLrow("UNLOADCNTTTL")
                                                Case "09"
                                                    TA0007ALLrow("OILPAYKBN109") = WW_DTLrow("OILPAYKBN")
                                                    TA0007ALLrow("OILPAYKBNNAMES109") = WW_DTLrow("OILPAYKBNNAMES")
                                                    TA0007ALLrow("HAIDISTANCE109") = WW_DTLrow("HAIDISTANCE")
                                                    TA0007ALLrow("HAIDISTANCECHO109") = WW_DTLrow("HAIDISTANCECHO")
                                                    TA0007ALLrow("HAIDISTANCETTL109") = WW_DTLrow("HAIDISTANCETTL")
                                                    TA0007ALLrow("KAIDISTANCE109") = WW_DTLrow("KAIDISTANCE")
                                                    TA0007ALLrow("KAIDISTANCECHO109") = WW_DTLrow("KAIDISTANCECHO")
                                                    TA0007ALLrow("KAIDISTANCETTL109") = WW_DTLrow("KAIDISTANCETTL")
                                                    TA0007ALLrow("UNLOADCNT109") = WW_DTLrow("UNLOADCNT")
                                                    TA0007ALLrow("UNLOADCNTCHO109") = WW_DTLrow("UNLOADCNTCHO")
                                                    TA0007ALLrow("UNLOADCNTTTL109") = WW_DTLrow("UNLOADCNTTTL")
                                                Case "10"
                                                    TA0007ALLrow("OILPAYKBN110") = WW_DTLrow("OILPAYKBN")
                                                    TA0007ALLrow("OILPAYKBNNAMES110") = WW_DTLrow("OILPAYKBNNAMES")
                                                    TA0007ALLrow("HAIDISTANCE110") = WW_DTLrow("HAIDISTANCE")
                                                    TA0007ALLrow("HAIDISTANCECHO110") = WW_DTLrow("HAIDISTANCECHO")
                                                    TA0007ALLrow("HAIDISTANCETTL110") = WW_DTLrow("HAIDISTANCETTL")
                                                    TA0007ALLrow("KAIDISTANCE110") = WW_DTLrow("KAIDISTANCE")
                                                    TA0007ALLrow("KAIDISTANCECHO110") = WW_DTLrow("KAIDISTANCECHO")
                                                    TA0007ALLrow("KAIDISTANCETTL110") = WW_DTLrow("KAIDISTANCETTL")
                                                    TA0007ALLrow("UNLOADCNT110") = WW_DTLrow("UNLOADCNT")
                                                    TA0007ALLrow("UNLOADCNTCHO110") = WW_DTLrow("UNLOADCNTCHO")
                                                    TA0007ALLrow("UNLOADCNTTTL110") = WW_DTLrow("UNLOADCNTTTL")
                                            End Select
                                        Case "2"
                                            TA0007ALLrow("SHARYOKBN2") = WW_DTLrow("SHARYOKBN")
                                            TA0007ALLrow("SHARYOKBNNAMES2") = WW_DTLrow("SHARYOKBNNAMES")

                                            Select Case WW_DTLrow("OILPAYKBN")
                                                Case "01"
                                                    TA0007ALLrow("OILPAYKBN201") = WW_DTLrow("OILPAYKBN")
                                                    TA0007ALLrow("OILPAYKBNNAMES201") = WW_DTLrow("OILPAYKBNNAMES")
                                                    TA0007ALLrow("HAIDISTANCE201") = WW_DTLrow("HAIDISTANCE")
                                                    TA0007ALLrow("HAIDISTANCECHO201") = WW_DTLrow("HAIDISTANCECHO")
                                                    TA0007ALLrow("HAIDISTANCETTL201") = WW_DTLrow("HAIDISTANCETTL")
                                                    TA0007ALLrow("KAIDISTANCE201") = WW_DTLrow("KAIDISTANCE")
                                                    TA0007ALLrow("KAIDISTANCECHO201") = WW_DTLrow("KAIDISTANCECHO")
                                                    TA0007ALLrow("KAIDISTANCETTL201") = WW_DTLrow("KAIDISTANCETTL")
                                                    TA0007ALLrow("UNLOADCNT201") = WW_DTLrow("UNLOADCNT")
                                                    TA0007ALLrow("UNLOADCNTCHO201") = WW_DTLrow("UNLOADCNTCHO")
                                                    TA0007ALLrow("UNLOADCNTTTL201") = WW_DTLrow("UNLOADCNTTTL")
                                                Case "02"
                                                    TA0007ALLrow("OILPAYKBN202") = WW_DTLrow("OILPAYKBN")
                                                    TA0007ALLrow("OILPAYKBNNAMES202") = WW_DTLrow("OILPAYKBNNAMES")
                                                    TA0007ALLrow("HAIDISTANCE202") = WW_DTLrow("HAIDISTANCE")
                                                    TA0007ALLrow("HAIDISTANCECHO202") = WW_DTLrow("HAIDISTANCECHO")
                                                    TA0007ALLrow("HAIDISTANCETTL202") = WW_DTLrow("HAIDISTANCETTL")
                                                    TA0007ALLrow("KAIDISTANCE202") = WW_DTLrow("KAIDISTANCE")
                                                    TA0007ALLrow("KAIDISTANCECHO202") = WW_DTLrow("KAIDISTANCECHO")
                                                    TA0007ALLrow("KAIDISTANCETTL202") = WW_DTLrow("KAIDISTANCETTL")
                                                    TA0007ALLrow("UNLOADCNT202") = WW_DTLrow("UNLOADCNT")
                                                    TA0007ALLrow("UNLOADCNTCHO202") = WW_DTLrow("UNLOADCNTCHO")
                                                    TA0007ALLrow("UNLOADCNTTTL202") = WW_DTLrow("UNLOADCNTTTL")
                                                Case "03"
                                                    TA0007ALLrow("OILPAYKBN203") = WW_DTLrow("OILPAYKBN")
                                                    TA0007ALLrow("OILPAYKBNNAMES203") = WW_DTLrow("OILPAYKBNNAMES")
                                                    TA0007ALLrow("HAIDISTANCE203") = WW_DTLrow("HAIDISTANCE")
                                                    TA0007ALLrow("HAIDISTANCECHO203") = WW_DTLrow("HAIDISTANCECHO")
                                                    TA0007ALLrow("HAIDISTANCETTL203") = WW_DTLrow("HAIDISTANCETTL")
                                                    TA0007ALLrow("KAIDISTANCE203") = WW_DTLrow("KAIDISTANCE")
                                                    TA0007ALLrow("KAIDISTANCECHO203") = WW_DTLrow("KAIDISTANCECHO")
                                                    TA0007ALLrow("KAIDISTANCETTL203") = WW_DTLrow("KAIDISTANCETTL")
                                                    TA0007ALLrow("UNLOADCNT203") = WW_DTLrow("UNLOADCNT")
                                                    TA0007ALLrow("UNLOADCNTCHO203") = WW_DTLrow("UNLOADCNTCHO")
                                                    TA0007ALLrow("UNLOADCNTTTL203") = WW_DTLrow("UNLOADCNTTTL")
                                                Case "04"
                                                    TA0007ALLrow("OILPAYKBN204") = WW_DTLrow("OILPAYKBN")
                                                    TA0007ALLrow("OILPAYKBNNAMES204") = WW_DTLrow("OILPAYKBNNAMES")
                                                    TA0007ALLrow("HAIDISTANCE204") = WW_DTLrow("HAIDISTANCE")
                                                    TA0007ALLrow("HAIDISTANCECHO204") = WW_DTLrow("HAIDISTANCECHO")
                                                    TA0007ALLrow("HAIDISTANCETTL204") = WW_DTLrow("HAIDISTANCETTL")
                                                    TA0007ALLrow("KAIDISTANCE204") = WW_DTLrow("KAIDISTANCE")
                                                    TA0007ALLrow("KAIDISTANCECHO204") = WW_DTLrow("KAIDISTANCECHO")
                                                    TA0007ALLrow("KAIDISTANCETTL204") = WW_DTLrow("KAIDISTANCETTL")
                                                    TA0007ALLrow("UNLOADCNT204") = WW_DTLrow("UNLOADCNT")
                                                    TA0007ALLrow("UNLOADCNTCHO204") = WW_DTLrow("UNLOADCNTCHO")
                                                    TA0007ALLrow("UNLOADCNTTTL204") = WW_DTLrow("UNLOADCNTTTL")
                                                Case "05"
                                                    TA0007ALLrow("OILPAYKBN205") = WW_DTLrow("OILPAYKBN")
                                                    TA0007ALLrow("OILPAYKBNNAMES205") = WW_DTLrow("OILPAYKBNNAMES")
                                                    TA0007ALLrow("HAIDISTANCE205") = WW_DTLrow("HAIDISTANCE")
                                                    TA0007ALLrow("HAIDISTANCECHO205") = WW_DTLrow("HAIDISTANCECHO")
                                                    TA0007ALLrow("HAIDISTANCETTL205") = WW_DTLrow("HAIDISTANCETTL")
                                                    TA0007ALLrow("KAIDISTANCE205") = WW_DTLrow("KAIDISTANCE")
                                                    TA0007ALLrow("KAIDISTANCECHO205") = WW_DTLrow("KAIDISTANCECHO")
                                                    TA0007ALLrow("KAIDISTANCETTL205") = WW_DTLrow("KAIDISTANCETTL")
                                                    TA0007ALLrow("UNLOADCNT205") = WW_DTLrow("UNLOADCNT")
                                                    TA0007ALLrow("UNLOADCNTCHO205") = WW_DTLrow("UNLOADCNTCHO")
                                                    TA0007ALLrow("UNLOADCNTTTL205") = WW_DTLrow("UNLOADCNTTTL")
                                                Case "06"
                                                    TA0007ALLrow("OILPAYKBN206") = WW_DTLrow("OILPAYKBN")
                                                    TA0007ALLrow("OILPAYKBNNAMES206") = WW_DTLrow("OILPAYKBNNAMES")
                                                    TA0007ALLrow("HAIDISTANCE206") = WW_DTLrow("HAIDISTANCE")
                                                    TA0007ALLrow("HAIDISTANCECHO206") = WW_DTLrow("HAIDISTANCECHO")
                                                    TA0007ALLrow("HAIDISTANCETTL206") = WW_DTLrow("HAIDISTANCETTL")
                                                    TA0007ALLrow("KAIDISTANCE206") = WW_DTLrow("KAIDISTANCE")
                                                    TA0007ALLrow("KAIDISTANCECHO206") = WW_DTLrow("KAIDISTANCECHO")
                                                    TA0007ALLrow("KAIDISTANCETTL206") = WW_DTLrow("KAIDISTANCETTL")
                                                    TA0007ALLrow("UNLOADCNT206") = WW_DTLrow("UNLOADCNT")
                                                    TA0007ALLrow("UNLOADCNTCHO206") = WW_DTLrow("UNLOADCNTCHO")
                                                    TA0007ALLrow("UNLOADCNTTTL206") = WW_DTLrow("UNLOADCNTTTL")
                                                Case "07"
                                                    TA0007ALLrow("OILPAYKBN207") = WW_DTLrow("OILPAYKBN")
                                                    TA0007ALLrow("OILPAYKBNNAMES207") = WW_DTLrow("OILPAYKBNNAMES")
                                                    TA0007ALLrow("HAIDISTANCE207") = WW_DTLrow("HAIDISTANCE")
                                                    TA0007ALLrow("HAIDISTANCECHO207") = WW_DTLrow("HAIDISTANCECHO")
                                                    TA0007ALLrow("HAIDISTANCETTL207") = WW_DTLrow("HAIDISTANCETTL")
                                                    TA0007ALLrow("KAIDISTANCE207") = WW_DTLrow("KAIDISTANCE")
                                                    TA0007ALLrow("KAIDISTANCECHO207") = WW_DTLrow("KAIDISTANCECHO")
                                                    TA0007ALLrow("KAIDISTANCETTL207") = WW_DTLrow("KAIDISTANCETTL")
                                                    TA0007ALLrow("UNLOADCNT207") = WW_DTLrow("UNLOADCNT")
                                                    TA0007ALLrow("UNLOADCNTCHO207") = WW_DTLrow("UNLOADCNTCHO")
                                                    TA0007ALLrow("UNLOADCNTTTL207") = WW_DTLrow("UNLOADCNTTTL")
                                                Case "08"
                                                    TA0007ALLrow("OILPAYKBN208") = WW_DTLrow("OILPAYKBN")
                                                    TA0007ALLrow("OILPAYKBNNAMES208") = WW_DTLrow("OILPAYKBNNAMES")
                                                    TA0007ALLrow("HAIDISTANCE208") = WW_DTLrow("HAIDISTANCE")
                                                    TA0007ALLrow("HAIDISTANCECHO208") = WW_DTLrow("HAIDISTANCECHO")
                                                    TA0007ALLrow("HAIDISTANCETTL208") = WW_DTLrow("HAIDISTANCETTL")
                                                    TA0007ALLrow("KAIDISTANCE208") = WW_DTLrow("KAIDISTANCE")
                                                    TA0007ALLrow("KAIDISTANCECHO208") = WW_DTLrow("KAIDISTANCECHO")
                                                    TA0007ALLrow("KAIDISTANCETTL208") = WW_DTLrow("KAIDISTANCETTL")
                                                    TA0007ALLrow("UNLOADCNT208") = WW_DTLrow("UNLOADCNT")
                                                    TA0007ALLrow("UNLOADCNTCHO208") = WW_DTLrow("UNLOADCNTCHO")
                                                    TA0007ALLrow("UNLOADCNTTTL208") = WW_DTLrow("UNLOADCNTTTL")
                                                Case "09"
                                                    TA0007ALLrow("OILPAYKBN209") = WW_DTLrow("OILPAYKBN")
                                                    TA0007ALLrow("OILPAYKBNNAMES209") = WW_DTLrow("OILPAYKBNNAMES")
                                                    TA0007ALLrow("HAIDISTANCE209") = WW_DTLrow("HAIDISTANCE")
                                                    TA0007ALLrow("HAIDISTANCECHO209") = WW_DTLrow("HAIDISTANCECHO")
                                                    TA0007ALLrow("HAIDISTANCETTL209") = WW_DTLrow("HAIDISTANCETTL")
                                                    TA0007ALLrow("KAIDISTANCE209") = WW_DTLrow("KAIDISTANCE")
                                                    TA0007ALLrow("KAIDISTANCECHO209") = WW_DTLrow("KAIDISTANCECHO")
                                                    TA0007ALLrow("KAIDISTANCETTL209") = WW_DTLrow("KAIDISTANCETTL")
                                                    TA0007ALLrow("UNLOADCNT209") = WW_DTLrow("UNLOADCNT")
                                                    TA0007ALLrow("UNLOADCNTCHO209") = WW_DTLrow("UNLOADCNTCHO")
                                                    TA0007ALLrow("UNLOADCNTTTL209") = WW_DTLrow("UNLOADCNTTTL")
                                                Case "10"
                                                    TA0007ALLrow("OILPAYKBN210") = WW_DTLrow("OILPAYKBN")
                                                    TA0007ALLrow("OILPAYKBNNAMES210") = WW_DTLrow("OILPAYKBNNAMES")
                                                    TA0007ALLrow("HAIDISTANCE210") = WW_DTLrow("HAIDISTANCE")
                                                    TA0007ALLrow("HAIDISTANCECHO210") = WW_DTLrow("HAIDISTANCECHO")
                                                    TA0007ALLrow("HAIDISTANCETTL210") = WW_DTLrow("HAIDISTANCETTL")
                                                    TA0007ALLrow("KAIDISTANCE210") = WW_DTLrow("KAIDISTANCE")
                                                    TA0007ALLrow("KAIDISTANCECHO210") = WW_DTLrow("KAIDISTANCECHO")
                                                    TA0007ALLrow("KAIDISTANCETTL210") = WW_DTLrow("KAIDISTANCETTL")
                                                    TA0007ALLrow("UNLOADCNT210") = WW_DTLrow("UNLOADCNT")
                                                    TA0007ALLrow("UNLOADCNTCHO210") = WW_DTLrow("UNLOADCNTCHO")
                                                    TA0007ALLrow("UNLOADCNTTTL210") = WW_DTLrow("UNLOADCNTTTL")
                                            End Select
                                    End Select
                                End If
                            Next
                        End If

                        '名前の取得
                        Dim O_RTN As String = C_MESSAGE_NO.NORMAL
                        TA0007ALLrow("CAMPNAMES") = ""
                        CodeToName("CAMPCODE", TA0007ALLrow("CAMPCODE"), TA0007ALLrow("CAMPNAMES"), O_RTN)
                        If TA0007ALLrow("STAFFNAMES") = "" Then
                            TA0007ALLrow("STAFFNAMES") = ""
                            CodeToName("STAFFCODE", TA0007ALLrow("STAFFCODE"), TA0007ALLrow("STAFFNAMES"), O_RTN)
                        End If
                        TA0007ALLrow("MORGNAMES") = ""
                        CodeToName("ORG", TA0007ALLrow("MORG"), TA0007ALLrow("MORGNAMES"), O_RTN)

                        If TA0007ALLrow("HORG") = "" Then
                            TA0007ALLrow("HORG") = work.WF_SEL_HORG.Text
                            TA0007ALLrow("HORGNAMES") = ""
                            CodeToName("ORG", TA0007ALLrow("HORG"), TA0007ALLrow("HORGNAMES"), O_RTN)
                        Else
                            TA0007ALLrow("HORGNAMES") = ""
                            CodeToName("ORG", TA0007ALLrow("HORG"), TA0007ALLrow("HORGNAMES"), O_RTN)
                        End If

                        If TA0007ALLrow("SORG") = "" Then
                            TA0007ALLrow("SORG") = TA0007ALLrow("HORG")
                        End If
                        TA0007ALLrow("SORGNAMES") = ""
                        CodeToName("ORG", TA0007ALLrow("SORG"), TA0007ALLrow("SORGNAMES"), O_RTN)


                        '○表示項目編集
                        If TA0007ALLrow("CAMPNAMES") = Nothing AndAlso TA0007ALLrow("CAMPCODE") = Nothing Then
                            TA0007ALLrow("CAMPCODE_TXT") = ""
                        Else
                            TA0007ALLrow("CAMPCODE_TXT") = TA0007ALLrow("CAMPNAMES") & " (" & TA0007ALLrow("CAMPCODE") & ")"
                        End If

                        TA0007ALLrow("TAISHOYM_TXT") = TA0007ALLrow("TAISHOYM")

                        If TA0007ALLrow("STAFFNAMES") = Nothing AndAlso TA0007ALLrow("STAFFCODE") = Nothing Then
                            TA0007ALLrow("STAFFCODE_TXT") = ""
                        Else
                            TA0007ALLrow("STAFFCODE_TXT") = TA0007ALLrow("STAFFNAMES") & " (" & TA0007ALLrow("STAFFCODE") & ")"
                        End If

                        If IsDate(TA0007ALLrow("WORKDATE")) Then
                            TA0007ALLrow("WORKDATE_TXT") = CDate(TA0007ALLrow("WORKDATE")).ToString("dd")
                        Else
                            TA0007ALLrow("WORKDATE_TXT") = ""
                        End If

                        If TA0007ALLrow("WORKINGWEEKNAMES") = Nothing Then
                            TA0007ALLrow("WORKINGWEEK_TXT") = ""
                        Else
                            TA0007ALLrow("WORKINGWEEK_TXT") = TA0007ALLrow("WORKINGWEEKNAMES")
                        End If

                        TA0007ALLrow("HDKBN_TXT") = TA0007ALLrow("HDKBN")

                        If TA0007ALLrow("RECODEKBNNAMES") = Nothing AndAlso TA0007ALLrow("RECODEKBN") = Nothing Then
                            TA0007ALLrow("RECODEKBN_TXT") = ""
                        Else
                            TA0007ALLrow("RECODEKBN_TXT") = TA0007ALLrow("RECODEKBNNAMES") & " (" & TA0007ALLrow("RECODEKBN") & ")"
                        End If

                        If TA0007ALLrow("WORKKBNNAMES") = Nothing AndAlso TA0007ALLrow("WORKKBN") = Nothing Then
                            TA0007ALLrow("WORKKBN_TXT") = ""
                        Else
                            TA0007ALLrow("WORKKBN_TXT") = TA0007ALLrow("WORKKBNNAMES") & " (" & TA0007ALLrow("WORKKBN") & ")"
                        End If

                        If TA0007ALLrow("SHARYOKBNNAMES") = Nothing AndAlso TA0007ALLrow("SHARYOKBN") = Nothing Then
                            TA0007ALLrow("SHARYOKBN_TXT") = ""
                        Else
                            TA0007ALLrow("SHARYOKBN_TXT") = TA0007ALLrow("SHARYOKBNNAMES") & " (" & TA0007ALLrow("SHARYOKBN") & ")"
                        End If

                        If TA0007ALLrow("OILPAYKBNNAMES") = Nothing AndAlso TA0007ALLrow("OILPAYKBN") = Nothing Then
                            TA0007ALLrow("OILPAYKBN_TXT") = ""
                        Else
                            TA0007ALLrow("OILPAYKBN_TXT") = TA0007ALLrow("OILPAYKBNNAMES") & " (" & TA0007ALLrow("OILPAYKBN") & ")"
                        End If

                        If TA0007ALLrow("STAFFKBNNAMES") = Nothing AndAlso TA0007ALLrow("STAFFKBN") = Nothing Then
                            TA0007ALLrow("STAFFKBN_TXT") = ""
                        Else
                            TA0007ALLrow("STAFFKBN_TXT") = TA0007ALLrow("STAFFKBNNAMES") & " (" & TA0007ALLrow("STAFFKBN") & ")"
                        End If

                        If TA0007ALLrow("MORGNAMES") = Nothing AndAlso TA0007ALLrow("MORG") = Nothing Then
                            TA0007ALLrow("MORG_TXT") = ""
                        Else
                            TA0007ALLrow("MORG_TXT") = TA0007ALLrow("MORGNAMES") & " (" & TA0007ALLrow("MORG") & ")"
                        End If

                        If TA0007ALLrow("HORGNAMES") = Nothing AndAlso TA0007ALLrow("HORG") = Nothing Then
                            TA0007ALLrow("HORG_TXT") = ""
                        Else
                            TA0007ALLrow("HORG_TXT") = TA0007ALLrow("HORGNAMES") & " (" & TA0007ALLrow("HORG") & ")"
                        End If

                        If TA0007ALLrow("SORGNAMES") = Nothing AndAlso TA0007ALLrow("SORG") = Nothing Then
                            TA0007ALLrow("SORG_TXT") = ""
                        Else
                            TA0007ALLrow("SORG_TXT") = TA0007ALLrow("SORGNAMES") & " (" & TA0007ALLrow("SORG") & ")"
                        End If

                        If TA0007ALLrow("HOLIDAYKBNNAMES") = Nothing AndAlso TA0007ALLrow("HOLIDAYKBN") = Nothing Then
                            TA0007ALLrow("HOLIDAYKBN_TXT") = ""
                        Else
                            TA0007ALLrow("HOLIDAYKBN_TXT") = TA0007ALLrow("HOLIDAYKBNNAMES") & " (" & TA0007ALLrow("HOLIDAYKBN") & ")"
                        End If

                        If TA0007ALLrow("PAYKBNNAMES") = Nothing AndAlso TA0007ALLrow("PAYKBN") = Nothing Then
                            TA0007ALLrow("PAYKBN_TXT") = ""
                        Else
                            TA0007ALLrow("PAYKBN_TXT") = TA0007ALLrow("PAYKBNNAMES") & " (" & TA0007ALLrow("PAYKBN") & ")"
                        End If

                        If TA0007ALLrow("SHUKCHOKKBNNAMES") = Nothing AndAlso TA0007ALLrow("SHUKCHOKKBN") = Nothing Then
                            TA0007ALLrow("SHUKCHOKKBN_TXT") = ""
                        Else
                            TA0007ALLrow("SHUKCHOKKBN_TXT") = TA0007ALLrow("SHUKCHOKKBNNAMES") & " (" & TA0007ALLrow("SHUKCHOKKBN") & ")"
                        End If

                        TA0007ALLrow("DELFLG_TXT") = TA0007ALLrow("DELFLG")

                    Next

                End Using
            End Using

        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0007_KINTAI SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0007_KINTAI Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ソート
        'ソート文字列取得
        CS0026TBLSORT.COMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0026TBLSORT.PROFID = Master.PROF_VIEW
        CS0026TBLSORT.MAPID = Master.MAPID
        CS0026TBLSORT.VARI = Master.VIEWID
        CS0026TBLSORT.TAB = ""
        CS0026TBLSORT.getSorting()
        If Not isNormal(CS0026TBLSORT.ERR) Then
            Master.output(CS0026TBLSORT.ERR, C_MESSAGE_TYPE.ABORT)
            Exit Sub
        End If
        'ソート＆データ抽出
        CS0026TBLSORT.TABLE = TA0007ALL
        CS0026TBLSORT.FILTER = "SELECT = 1"
        TA0007VIEWtbl = CS0026TBLSORT.sort()

    End Sub

    ''' <summary>
    '''  TA0007VIEW-GridView用テーブル作成
    ''' </summary>
    ''' <param name="I_CODE">表示区分コード</param>
    ''' <remarks></remarks>
    Protected Sub GetViewTA0007Tbl(ByVal I_CODE As String)

        '１０分切上処理
        CopyT7Edit(TA0007ALL, TA0007VIEWtbl)

        '〇 TA0007ALLよりデータ抽出
        CS0026TBLSORT.TABLE = TA0007VIEWtbl
        CS0026TBLSORT.FILTER = "HDKBN = '" & I_CODE & "'"
        CS0026TBLSORT.SORTING = "LINECNT, SEQ ASC"
        TA0007VIEWtbl = CS0026TBLSORT.sort()
        '○LineCNT付番・枝番再付番
        Dim WW_LINECNT As Integer = 0
        Dim WW_SEQ As Integer = 0

        For Each TA0007VIEWrow As DataRow In TA0007VIEWtbl.Rows
            TA0007VIEWrow("LINECNT") = 0
        Next

        For Each TA0007VIEWrow As DataRow In TA0007VIEWtbl.Rows

            If TA0007VIEWrow("LINECNT") = 0 Then
                TA0007VIEWrow("SELECT") = "1"
                TA0007VIEWrow("HIDDEN") = "0"      '表示
                WW_LINECNT += 1
                TA0007VIEWrow("LINECNT") = WW_LINECNT
            End If

        Next

    End Sub

    ''' <summary>
    '''  帳票出力用編集処理
    ''' </summary>
    ''' <param name="IO_TBL"></param>
    ''' <remarks></remarks>
    Protected Sub EditList(ByRef IO_TBL As DataTable)
        Dim WW_LINEcnt As Integer = 0

        Dim WW_TA0007tbl As DataTable = IO_TBL.Clone
        Dim WW_TA0007row As DataRow
        Dim WW_RTN As String = C_MESSAGE_NO.NORMAL

        For i As Integer = 0 To IO_TBL.Rows.Count - 1
            WW_TA0007row = WW_TA0007tbl.NewRow
            WW_TA0007row.ItemArray = IO_TBL.Rows(i).ItemArray

            '--------------------------------------
            '勤務状況リスト編集 
            '--------------------------------------
            WW_TA0007row("TAISHOYM_TXT") = Mid(WW_TA0007row("TAISHOYM"), 1, 4) & "年" & Mid(WW_TA0007row("TAISHOYM"), 6, 2) & "月"

            If WW_TA0007row("SHUKCHOKKBN") = "0" Then
                WW_TA0007row("SHUKCHOKKBN_TXT") = ""
                WW_TA0007row("SHUKCHOKKBNNAMES") = ""
            End If

            If WW_TA0007row("HOLIDAYKBN") = "0" Then
                WW_TA0007row("HOLIDAYKBN_TXT") = ""
                WW_TA0007row("HOLIDAYKBNNAMES") = ""
            End If

            WW_TA0007row("WORKTIME") = T0009TIME.ZeroToSpace(WW_TA0007row("WORKTIME"))
            WW_TA0007row("MOVETIME") = T0009TIME.ZeroToSpace(WW_TA0007row("MOVETIME"))
            WW_TA0007row("ACTTIME") = T0009TIME.ZeroToSpace(WW_TA0007row("ACTTIME"))

            WW_TA0007row("BINDTIME") = T0009TIME.ZeroToSpace(WW_TA0007row("BINDTIME"))
            WW_TA0007row("BREAKTIME") = T0009TIME.ZeroToSpace(WW_TA0007row("BREAKTIME"))
            WW_TA0007row("BREAKTIMECHO") = T0009TIME.ZeroToSpace(WW_TA0007row("BREAKTIMECHO"))
            WW_TA0007row("BREAKTIMETTL") = T0009TIME.ZeroToSpace(WW_TA0007row("BREAKTIMETTL"))
            WW_TA0007row("NIGHTTIME") = T0009TIME.ZeroToSpace(WW_TA0007row("NIGHTTIME"))
            WW_TA0007row("NIGHTTIMECHO") = T0009TIME.ZeroToSpace(WW_TA0007row("NIGHTTIMECHO"))
            WW_TA0007row("NIGHTTIMETTL") = T0009TIME.ZeroToSpace(WW_TA0007row("NIGHTTIMETTL"))
            WW_TA0007row("ORVERTIME") = T0009TIME.ZeroToSpace(WW_TA0007row("ORVERTIME"))
            WW_TA0007row("ORVERTIMECHO") = T0009TIME.ZeroToSpace(WW_TA0007row("ORVERTIMECHO"))
            WW_TA0007row("ORVERTIMETTL") = T0009TIME.ZeroToSpace(WW_TA0007row("ORVERTIMETTL"))
            WW_TA0007row("WNIGHTTIME") = T0009TIME.ZeroToSpace(WW_TA0007row("WNIGHTTIME"))
            WW_TA0007row("WNIGHTTIMECHO") = T0009TIME.ZeroToSpace(WW_TA0007row("WNIGHTTIMECHO"))
            WW_TA0007row("WNIGHTTIMETTL") = T0009TIME.ZeroToSpace(WW_TA0007row("WNIGHTTIMETTL"))
            WW_TA0007row("SWORKTIME") = T0009TIME.ZeroToSpace(WW_TA0007row("SWORKTIME"))
            WW_TA0007row("SWORKTIMECHO") = T0009TIME.ZeroToSpace(WW_TA0007row("SWORKTIMECHO"))
            WW_TA0007row("SWORKTIMETTL") = T0009TIME.ZeroToSpace(WW_TA0007row("SWORKTIMETTL"))
            WW_TA0007row("SNIGHTTIME") = T0009TIME.ZeroToSpace(WW_TA0007row("SNIGHTTIME"))
            WW_TA0007row("SNIGHTTIMECHO") = T0009TIME.ZeroToSpace(WW_TA0007row("SNIGHTTIMECHO"))
            WW_TA0007row("SNIGHTTIMETTL") = T0009TIME.ZeroToSpace(WW_TA0007row("SNIGHTTIMETTL"))
            WW_TA0007row("HWORKTIME") = T0009TIME.ZeroToSpace(WW_TA0007row("HWORKTIME"))
            WW_TA0007row("HWORKTIMECHO") = T0009TIME.ZeroToSpace(WW_TA0007row("HWORKTIMECHO"))
            WW_TA0007row("HWORKTIMETTL") = T0009TIME.ZeroToSpace(WW_TA0007row("HWORKTIMETTL"))
            WW_TA0007row("HNIGHTTIME") = T0009TIME.ZeroToSpace(WW_TA0007row("HNIGHTTIME"))
            WW_TA0007row("HNIGHTTIMECHO") = T0009TIME.ZeroToSpace(WW_TA0007row("HNIGHTTIMECHO"))
            'WW_TA0007row("HNIGHTTIMETTL") = T0009TIME.ZEROtoSpace(WW_TA0007row("HNIGHTTIMETTL"))
            WW_TA0007row("HNIGHTTIMETTL") = ""
            WW_TA0007row("HOANTIME") = T0009TIME.ZeroToSpace(WW_TA0007row("HOANTIME"))
            WW_TA0007row("HOANTIMECHO") = T0009TIME.ZeroToSpace(WW_TA0007row("HOANTIMECHO"))
            WW_TA0007row("HOANTIMETTL") = T0009TIME.ZeroToSpace(WW_TA0007row("HOANTIMETTL"))
            WW_TA0007row("KOATUTIME") = T0009TIME.ZeroToSpace(WW_TA0007row("KOATUTIME"))
            WW_TA0007row("KOATUTIMECHO") = T0009TIME.ZeroToSpace(WW_TA0007row("KOATUTIMECHO"))
            WW_TA0007row("KOATUTIMETTL") = T0009TIME.ZeroToSpace(WW_TA0007row("KOATUTIMETTL"))
            WW_TA0007row("TOKUSA1TIME") = T0009TIME.ZeroToSpace(WW_TA0007row("TOKUSA1TIME"))
            WW_TA0007row("TOKUSA1TIMECHO") = T0009TIME.ZeroToSpace(WW_TA0007row("TOKUSA1TIMECHO"))
            WW_TA0007row("TOKUSA1TIMETTL") = T0009TIME.ZeroToSpace(WW_TA0007row("TOKUSA1TIMETTL"))
            WW_TA0007row("TOKSAAKAISU") = T0009TIME.ZeroToSpace(WW_TA0007row("TOKSAAKAISU"))
            WW_TA0007row("TOKSAAKAISUCHO") = T0009TIME.ZeroToSpace(WW_TA0007row("TOKSAAKAISUCHO"))
            WW_TA0007row("TOKSAAKAISUTTL") = T0009TIME.ZeroToSpace(WW_TA0007row("TOKSAAKAISUTTL"))
            WW_TA0007row("TOKSABKAISU") = T0009TIME.ZeroToSpace(WW_TA0007row("TOKSABKAISU"))
            WW_TA0007row("TOKSABKAISUCHO") = T0009TIME.ZeroToSpace(WW_TA0007row("TOKSABKAISUCHO"))
            WW_TA0007row("TOKSABKAISUTTL") = T0009TIME.ZeroToSpace(WW_TA0007row("TOKSABKAISUTTL"))
            WW_TA0007row("TOKSACKAISU") = T0009TIME.ZeroToSpace(WW_TA0007row("TOKSACKAISU"))
            WW_TA0007row("TOKSACKAISUCHO") = T0009TIME.ZeroToSpace(WW_TA0007row("TOKSACKAISUCHO"))
            WW_TA0007row("TOKSACKAISUTTL") = T0009TIME.ZeroToSpace(WW_TA0007row("TOKSACKAISUTTL"))
            WW_TA0007row("HAYADETIME") = T0009TIME.ZeroToSpace(WW_TA0007row("HAYADETIME"))
            WW_TA0007row("HAYADETIMECHO") = T0009TIME.ZeroToSpace(WW_TA0007row("HAYADETIMECHO"))
            WW_TA0007row("HAYADETIMETTL") = T0009TIME.ZeroToSpace(WW_TA0007row("HAYADETIMETTL"))
            WW_TA0007row("JIKYUSHATIME") = T0009TIME.ZeroToSpace(WW_TA0007row("JIKYUSHATIME"))
            WW_TA0007row("JIKYUSHATIMECHO") = T0009TIME.ZeroToSpace(WW_TA0007row("JIKYUSHATIMECHO"))
            WW_TA0007row("JIKYUSHATIMETTL") = T0009TIME.ZeroToSpace(WW_TA0007row("JIKYUSHATIMETTL"))
            WW_TA0007row("HDAIWORKTIME") = T0009TIME.ZeroToSpace(WW_TA0007row("HDAIWORKTIME"))
            WW_TA0007row("HDAIWORKTIMECHO") = T0009TIME.ZeroToSpace(WW_TA0007row("HDAIWORKTIMECHO"))
            WW_TA0007row("HDAIWORKTIMETTL") = T0009TIME.ZeroToSpace(WW_TA0007row("HDAIWORKTIMETTL"))
            WW_TA0007row("HDAINIGHTTIME") = T0009TIME.ZeroToSpace(WW_TA0007row("HDAINIGHTTIME"))
            WW_TA0007row("HDAINIGHTTIMECHO") = T0009TIME.ZeroToSpace(WW_TA0007row("HDAINIGHTTIMECHO"))
            WW_TA0007row("HDAINIGHTTIMETTL") = T0009TIME.ZeroToSpace(WW_TA0007row("HDAINIGHTTIMETTL"))
            WW_TA0007row("SDAIWORKTIME") = T0009TIME.ZeroToSpace(WW_TA0007row("SDAIWORKTIME"))
            WW_TA0007row("SDAIWORKTIMECHO") = T0009TIME.ZeroToSpace(WW_TA0007row("SDAIWORKTIMECHO"))
            WW_TA0007row("SDAIWORKTIMETTL") = T0009TIME.ZeroToSpace(WW_TA0007row("SDAIWORKTIMETTL"))
            WW_TA0007row("SDAINIGHTTIME") = T0009TIME.ZeroToSpace(WW_TA0007row("SDAINIGHTTIME"))
            WW_TA0007row("SDAINIGHTTIMECHO") = T0009TIME.ZeroToSpace(WW_TA0007row("SDAINIGHTTIMECHO"))
            WW_TA0007row("SDAINIGHTTIMETTL") = T0009TIME.ZeroToSpace(WW_TA0007row("SDAINIGHTTIMETTL"))

            WW_TA0007row("WORKNISSU") = T0009TIME.ZeroToSpace(WW_TA0007row("WORKNISSU"))
            WW_TA0007row("WORKNISSUCHO") = T0009TIME.ZeroToSpace(WW_TA0007row("WORKNISSUCHO"))
            WW_TA0007row("WORKNISSUTTL") = T0009TIME.ZeroToSpace(WW_TA0007row("WORKNISSUTTL"))
            WW_TA0007row("SHOUKETUNISSU") = T0009TIME.ZeroToSpace(WW_TA0007row("SHOUKETUNISSU"))
            WW_TA0007row("SHOUKETUNISSUCHO") = T0009TIME.ZeroToSpace(WW_TA0007row("SHOUKETUNISSUCHO"))
            WW_TA0007row("SHOUKETUNISSUTTL") = T0009TIME.ZeroToSpace(WW_TA0007row("SHOUKETUNISSUTTL"))
            WW_TA0007row("KUMIKETUNISSU") = T0009TIME.ZeroToSpace(WW_TA0007row("KUMIKETUNISSU"))
            WW_TA0007row("KUMIKETUNISSUCHO") = T0009TIME.ZeroToSpace(WW_TA0007row("KUMIKETUNISSUCHO"))
            WW_TA0007row("KUMIKETUNISSUTTL") = T0009TIME.ZeroToSpace(WW_TA0007row("KUMIKETUNISSUTTL"))
            WW_TA0007row("ETCKETUNISSU") = T0009TIME.ZeroToSpace(WW_TA0007row("ETCKETUNISSU"))
            WW_TA0007row("ETCKETUNISSUCHO") = T0009TIME.ZeroToSpace(WW_TA0007row("ETCKETUNISSUCHO"))
            WW_TA0007row("ETCKETUNISSUTTL") = T0009TIME.ZeroToSpace(WW_TA0007row("ETCKETUNISSUTTL"))
            WW_TA0007row("NENKYUNISSU") = T0009TIME.ZeroToSpace(WW_TA0007row("NENKYUNISSU"))
            WW_TA0007row("NENKYUNISSUCHO") = T0009TIME.ZeroToSpace(WW_TA0007row("NENKYUNISSUCHO"))
            WW_TA0007row("NENKYUNISSUTTL") = T0009TIME.ZeroToSpace(WW_TA0007row("NENKYUNISSUTTL"))
            WW_TA0007row("TOKUKYUNISSU") = T0009TIME.ZeroToSpace(WW_TA0007row("TOKUKYUNISSU"))
            WW_TA0007row("TOKUKYUNISSUCHO") = T0009TIME.ZeroToSpace(WW_TA0007row("TOKUKYUNISSUCHO"))
            WW_TA0007row("TOKUKYUNISSUTTL") = T0009TIME.ZeroToSpace(WW_TA0007row("TOKUKYUNISSUTTL"))
            WW_TA0007row("CHIKOKSOTAINISSU") = T0009TIME.ZeroToSpace(WW_TA0007row("CHIKOKSOTAINISSU"))
            WW_TA0007row("CHIKOKSOTAINISSUCHO") = T0009TIME.ZeroToSpace(WW_TA0007row("CHIKOKSOTAINISSUCHO"))
            WW_TA0007row("CHIKOKSOTAINISSUTTL") = T0009TIME.ZeroToSpace(WW_TA0007row("CHIKOKSOTAINISSUTTL"))
            WW_TA0007row("STOCKNISSU") = T0009TIME.ZeroToSpace(WW_TA0007row("STOCKNISSU"))
            WW_TA0007row("STOCKNISSUCHO") = T0009TIME.ZeroToSpace(WW_TA0007row("STOCKNISSUCHO"))
            WW_TA0007row("STOCKNISSUTTL") = T0009TIME.ZeroToSpace(WW_TA0007row("STOCKNISSUTTL"))
            WW_TA0007row("KYOTEIWEEKNISSU") = T0009TIME.ZeroToSpace(WW_TA0007row("KYOTEIWEEKNISSU"))
            WW_TA0007row("KYOTEIWEEKNISSUCHO") = T0009TIME.ZeroToSpace(WW_TA0007row("KYOTEIWEEKNISSUCHO"))
            WW_TA0007row("KYOTEIWEEKNISSUTTL") = T0009TIME.ZeroToSpace(WW_TA0007row("KYOTEIWEEKNISSUTTL"))
            WW_TA0007row("WEEKNISSU") = T0009TIME.ZeroToSpace(WW_TA0007row("WEEKNISSU"))
            WW_TA0007row("WEEKNISSUCHO") = T0009TIME.ZeroToSpace(WW_TA0007row("WEEKNISSUCHO"))
            WW_TA0007row("WEEKNISSUTTL") = T0009TIME.ZeroToSpace(WW_TA0007row("WEEKNISSUTTL"))
            WW_TA0007row("DAIKYUNISSU") = T0009TIME.ZeroToSpace(WW_TA0007row("DAIKYUNISSU"))
            WW_TA0007row("DAIKYUNISSUCHO") = T0009TIME.ZeroToSpace(WW_TA0007row("DAIKYUNISSUCHO"))
            WW_TA0007row("DAIKYUNISSUTTL") = T0009TIME.ZeroToSpace(WW_TA0007row("DAIKYUNISSUTTL"))
            WW_TA0007row("NENSHINISSU") = T0009TIME.ZeroToSpace(WW_TA0007row("NENSHINISSU"))
            WW_TA0007row("NENSHINISSUCHO") = T0009TIME.ZeroToSpace(WW_TA0007row("NENSHINISSUCHO"))
            WW_TA0007row("NENSHINISSUTTL") = T0009TIME.ZeroToSpace(WW_TA0007row("NENSHINISSUTTL"))
            WW_TA0007row("NENMATUNISSU") = T0009TIME.ZeroToSpace(WW_TA0007row("NENMATUNISSU"))
            WW_TA0007row("NENMATUNISSUCHO") = T0009TIME.ZeroToSpace(WW_TA0007row("NENMATUNISSUCHO"))
            WW_TA0007row("NENMATUNISSUTTL") = T0009TIME.ZeroToSpace(WW_TA0007row("NENMATUNISSUTTL"))
            WW_TA0007row("SHUKCHOKNNISSU") = T0009TIME.ZeroToSpace(WW_TA0007row("SHUKCHOKNNISSU"))
            WW_TA0007row("SHUKCHOKNNISSUCHO") = T0009TIME.ZeroToSpace(WW_TA0007row("SHUKCHOKNNISSUCHO"))
            WW_TA0007row("SHUKCHOKNNISSUTTL") = T0009TIME.ZeroToSpace(WW_TA0007row("SHUKCHOKNNISSUTTL"))
            WW_TA0007row("SHUKCHOKNISSU") = T0009TIME.ZeroToSpace(WW_TA0007row("SHUKCHOKNISSU"))
            WW_TA0007row("SHUKCHOKNISSUCHO") = T0009TIME.ZeroToSpace(WW_TA0007row("SHUKCHOKNISSUCHO"))
            WW_TA0007row("SHUKCHOKNISSUTTL") = T0009TIME.ZeroToSpace(WW_TA0007row("SHUKCHOKNISSUTTL"))
            WW_TA0007row("SHUKCHOKNHLDNISSU") = T0009TIME.ZeroToSpace(WW_TA0007row("SHUKCHOKNHLDNISSU"))
            WW_TA0007row("SHUKCHOKNHLDNISSUCHO") = T0009TIME.ZeroToSpace(WW_TA0007row("SHUKCHOKNHLDNISSUCHO"))
            WW_TA0007row("SHUKCHOKNHLDNISSUTTL") = T0009TIME.ZeroToSpace(WW_TA0007row("SHUKCHOKNHLDNISSUTTL"))
            WW_TA0007row("SHUKCHOKHLDNISSU") = T0009TIME.ZeroToSpace(WW_TA0007row("SHUKCHOKHLDNISSU"))
            WW_TA0007row("SHUKCHOKHLDNISSUCHO") = T0009TIME.ZeroToSpace(WW_TA0007row("SHUKCHOKHLDNISSUCHO"))
            WW_TA0007row("SHUKCHOKHLDNISSUTTL") = T0009TIME.ZeroToSpace(WW_TA0007row("SHUKCHOKHLDNISSUTTL"))
            WW_TA0007row("TOKSAAKAISU") = T0009TIME.ZeroToSpace(WW_TA0007row("TOKSAAKAISU"))
            WW_TA0007row("TOKSAAKAISUCHO") = T0009TIME.ZeroToSpace(WW_TA0007row("TOKSAAKAISUCHO"))
            WW_TA0007row("TOKSAAKAISUTTL") = T0009TIME.ZeroToSpace(WW_TA0007row("TOKSAAKAISUTTL"))
            WW_TA0007row("TOKSABKAISU") = T0009TIME.ZeroToSpace(WW_TA0007row("TOKSABKAISU"))
            WW_TA0007row("TOKSABKAISUCHO") = T0009TIME.ZeroToSpace(WW_TA0007row("TOKSABKAISUCHO"))
            WW_TA0007row("TOKSABKAISUTTL") = T0009TIME.ZeroToSpace(WW_TA0007row("TOKSABKAISUTTL"))
            WW_TA0007row("TOKSACKAISU") = T0009TIME.ZeroToSpace(WW_TA0007row("TOKSACKAISU"))
            WW_TA0007row("TOKSACKAISUCHO") = T0009TIME.ZeroToSpace(WW_TA0007row("TOKSACKAISUCHO"))
            WW_TA0007row("TOKSACKAISUTTL") = T0009TIME.ZeroToSpace(WW_TA0007row("TOKSACKAISUTTL"))
            WW_TA0007row("HOANTIME") = T0009TIME.ZeroToSpace(WW_TA0007row("HOANTIME"))
            WW_TA0007row("HOANTIMECHO") = T0009TIME.ZeroToSpace(WW_TA0007row("HOANTIMECHO"))
            WW_TA0007row("HOANTIMETTL") = T0009TIME.ZeroToSpace(WW_TA0007row("HOANTIMETTL"))
            WW_TA0007row("KOATUTIME") = T0009TIME.ZeroToSpace(WW_TA0007row("KOATUTIME"))
            WW_TA0007row("KOATUTIMECHO") = T0009TIME.ZeroToSpace(WW_TA0007row("KOATUTIMECHO"))
            WW_TA0007row("KOATUTIMETTL") = T0009TIME.ZeroToSpace(WW_TA0007row("KOATUTIMETTL"))
            WW_TA0007row("TOKUSA1TIME") = T0009TIME.ZeroToSpace(WW_TA0007row("TOKUSA1TIME"))
            WW_TA0007row("TOKUSA1TIMECHO") = T0009TIME.ZeroToSpace(WW_TA0007row("TOKUSA1TIMECHO"))
            WW_TA0007row("TOKUSA1TIMETTL") = T0009TIME.ZeroToSpace(WW_TA0007row("TOKUSA1TIMETTL"))
            WW_TA0007row("PONPNISSU") = T0009TIME.ZeroToSpace(WW_TA0007row("PONPNISSU"))
            WW_TA0007row("PONPNISSUCHO") = T0009TIME.ZeroToSpace(WW_TA0007row("PONPNISSUCHO"))
            WW_TA0007row("PONPNISSUTTL") = T0009TIME.ZeroToSpace(WW_TA0007row("PONPNISSUTTL"))
            WW_TA0007row("BULKNISSU") = T0009TIME.ZeroToSpace(WW_TA0007row("BULKNISSU"))
            WW_TA0007row("BULKNISSUCHO") = T0009TIME.ZeroToSpace(WW_TA0007row("BULKNISSUCHO"))
            WW_TA0007row("BULKNISSUTTL") = T0009TIME.ZeroToSpace(WW_TA0007row("BULKNISSUTTL"))
            WW_TA0007row("TRAILERNISSU") = T0009TIME.ZeroToSpace(WW_TA0007row("TRAILERNISSU"))
            WW_TA0007row("TRAILERNISSUCHO") = T0009TIME.ZeroToSpace(WW_TA0007row("TRAILERNISSUCHO"))
            WW_TA0007row("TRAILERNISSUTTL") = T0009TIME.ZeroToSpace(WW_TA0007row("TRAILERNISSUTTL"))
            WW_TA0007row("BKINMUKAISU") = T0009TIME.ZeroToSpace(WW_TA0007row("BKINMUKAISU"))
            WW_TA0007row("BKINMUKAISUCHO") = T0009TIME.ZeroToSpace(WW_TA0007row("BKINMUKAISUCHO"))
            WW_TA0007row("BKINMUKAISUTTL") = T0009TIME.ZeroToSpace(WW_TA0007row("BKINMUKAISUTTL"))
            WW_TA0007row("HAYADETIME") = T0009TIME.ZeroToSpace(WW_TA0007row("HAYADETIME"))
            WW_TA0007row("HAYADETIMECHO") = T0009TIME.ZeroToSpace(WW_TA0007row("HAYADETIMECHO"))
            WW_TA0007row("HAYADETIMETTL") = T0009TIME.ZeroToSpace(WW_TA0007row("HAYADETIMETTL"))
            WW_TA0007row("JIKYUSHATIME") = T0009TIME.ZeroToSpace(WW_TA0007row("JIKYUSHATIME"))
            WW_TA0007row("JIKYUSHATIMECHO") = T0009TIME.ZeroToSpace(WW_TA0007row("JIKYUSHATIMECHO"))
            WW_TA0007row("JIKYUSHATIMETTL") = T0009TIME.ZeroToSpace(WW_TA0007row("JIKYUSHATIMETTL"))
            WW_TA0007row("HDAIWORKTIME") = T0009TIME.ZeroToSpace(WW_TA0007row("HDAIWORKTIME"))
            WW_TA0007row("HDAIWORKTIMECHO") = T0009TIME.ZeroToSpace(WW_TA0007row("HDAIWORKTIMECHO"))
            WW_TA0007row("HDAIWORKTIMETTL") = T0009TIME.ZeroToSpace(WW_TA0007row("HDAIWORKTIMETTL"))
            WW_TA0007row("HDAINIGHTTIME") = T0009TIME.ZeroToSpace(WW_TA0007row("HDAINIGHTTIME"))
            WW_TA0007row("HDAINIGHTTIMECHO") = T0009TIME.ZeroToSpace(WW_TA0007row("HDAINIGHTTIMECHO"))
            WW_TA0007row("HDAINIGHTTIMETTL") = T0009TIME.ZeroToSpace(WW_TA0007row("HDAINIGHTTIMETTL"))
            WW_TA0007row("SDAIWORKTIME") = T0009TIME.ZeroToSpace(WW_TA0007row("SDAIWORKTIME"))
            WW_TA0007row("SDAIWORKTIMECHO") = T0009TIME.ZeroToSpace(WW_TA0007row("SDAIWORKTIMECHO"))
            WW_TA0007row("SDAIWORKTIMETTL") = T0009TIME.ZeroToSpace(WW_TA0007row("SDAIWORKTIMETTL"))
            WW_TA0007row("SDAINIGHTTIME") = T0009TIME.ZeroToSpace(WW_TA0007row("SDAINIGHTTIME"))
            WW_TA0007row("SDAINIGHTTIMECHO") = T0009TIME.ZeroToSpace(WW_TA0007row("SDAINIGHTTIMECHO"))
            WW_TA0007row("SDAINIGHTTIMETTL") = T0009TIME.ZeroToSpace(WW_TA0007row("SDAINIGHTTIMETTL"))
            WW_TA0007row("HWORKNISSU") = T0009TIME.ZeroToSpace(WW_TA0007row("HWORKNISSU"))
            WW_TA0007row("HWORKNISSUCHO") = T0009TIME.ZeroToSpace(WW_TA0007row("HWORKNISSUCHO"))
            WW_TA0007row("HWORKNISSUTTL") = T0009TIME.ZeroToSpace(WW_TA0007row("HWORKNISSUTTL"))

            WW_TA0007row("OILPAYKBN101") = WW_TA0007row("OILPAYKBN101")
            WW_TA0007row("OILPAYKBNNAMES101") = WW_TA0007row("OILPAYKBNNAMES101")
            WW_TA0007row("HAIDISTANCE101") = Val(WW_TA0007row("HAIDISTANCE101")).ToString("#")
            WW_TA0007row("HAIDISTANCECHO101") = Val(WW_TA0007row("HAIDISTANCECHO101")).ToString("#")
            WW_TA0007row("HAIDISTANCETTL101") = Val(WW_TA0007row("HAIDISTANCETTL101")).ToString("#")
            WW_TA0007row("KAIDISTANCE101") = Val(WW_TA0007row("KAIDISTANCE101")).ToString("#")
            WW_TA0007row("KAIDISTANCECHO101") = Val(WW_TA0007row("KAIDISTANCECHO101")).ToString("#")
            WW_TA0007row("KAIDISTANCETTL101") = Val(WW_TA0007row("KAIDISTANCETTL101")).ToString("#")
            WW_TA0007row("UNLOADCNT101") = Val(WW_TA0007row("UNLOADCNT101")).ToString("#")
            WW_TA0007row("UNLOADCNTCHO101") = Val(WW_TA0007row("UNLOADCNTCHO101")).ToString("#")
            WW_TA0007row("UNLOADCNTTTL101") = Val(WW_TA0007row("UNLOADCNTTTL101")).ToString("#")

            WW_TA0007row("OILPAYKBN102") = WW_TA0007row("OILPAYKBN102")
            WW_TA0007row("OILPAYKBNNAMES102") = WW_TA0007row("OILPAYKBNNAMES102")
            WW_TA0007row("HAIDISTANCE102") = Val(WW_TA0007row("HAIDISTANCE102")).ToString("#")
            WW_TA0007row("HAIDISTANCECHO102") = Val(WW_TA0007row("HAIDISTANCECHO102")).ToString("#")
            WW_TA0007row("HAIDISTANCETTL102") = Val(WW_TA0007row("HAIDISTANCETTL102")).ToString("#")
            WW_TA0007row("KAIDISTANCE102") = Val(WW_TA0007row("KAIDISTANCE102")).ToString("#")
            WW_TA0007row("KAIDISTANCECHO102") = Val(WW_TA0007row("KAIDISTANCECHO102")).ToString("#")
            WW_TA0007row("KAIDISTANCETTL102") = Val(WW_TA0007row("KAIDISTANCETTL102")).ToString("#")
            WW_TA0007row("UNLOADCNT102") = Val(WW_TA0007row("UNLOADCNT102")).ToString("#")
            WW_TA0007row("UNLOADCNTCHO102") = Val(WW_TA0007row("UNLOADCNTCHO102")).ToString("#")
            WW_TA0007row("UNLOADCNTTTL102") = Val(WW_TA0007row("UNLOADCNTTTL102")).ToString("#")

            WW_TA0007row("OILPAYKBN103") = WW_TA0007row("OILPAYKBN103")
            WW_TA0007row("OILPAYKBNNAMES103") = WW_TA0007row("OILPAYKBNNAMES103")
            WW_TA0007row("HAIDISTANCE103") = Val(WW_TA0007row("HAIDISTANCE103")).ToString("#")
            WW_TA0007row("HAIDISTANCECHO103") = Val(WW_TA0007row("HAIDISTANCECHO103")).ToString("#")
            WW_TA0007row("HAIDISTANCETTL103") = Val(WW_TA0007row("HAIDISTANCETTL103")).ToString("#")
            WW_TA0007row("KAIDISTANCE103") = Val(WW_TA0007row("KAIDISTANCE103")).ToString("#")
            WW_TA0007row("KAIDISTANCECHO103") = Val(WW_TA0007row("KAIDISTANCECHO103")).ToString("#")
            WW_TA0007row("KAIDISTANCETTL103") = Val(WW_TA0007row("KAIDISTANCETTL103")).ToString("#")
            WW_TA0007row("UNLOADCNT103") = Val(WW_TA0007row("UNLOADCNT103")).ToString("#")
            WW_TA0007row("UNLOADCNTCHO103") = Val(WW_TA0007row("UNLOADCNTCHO103")).ToString("#")
            WW_TA0007row("UNLOADCNTTTL103") = Val(WW_TA0007row("UNLOADCNTTTL103")).ToString("#")

            WW_TA0007row("OILPAYKBN104") = WW_TA0007row("OILPAYKBN104")
            WW_TA0007row("OILPAYKBNNAMES104") = WW_TA0007row("OILPAYKBNNAMES104")
            WW_TA0007row("HAIDISTANCE104") = Val(WW_TA0007row("HAIDISTANCE104")).ToString("#")
            WW_TA0007row("HAIDISTANCECHO104") = Val(WW_TA0007row("HAIDISTANCECHO104")).ToString("#")
            WW_TA0007row("HAIDISTANCETTL104") = Val(WW_TA0007row("HAIDISTANCETTL104")).ToString("#")
            WW_TA0007row("KAIDISTANCE104") = Val(WW_TA0007row("KAIDISTANCE104")).ToString("#")
            WW_TA0007row("KAIDISTANCECHO104") = Val(WW_TA0007row("KAIDISTANCECHO104")).ToString("#")
            WW_TA0007row("KAIDISTANCETTL104") = Val(WW_TA0007row("KAIDISTANCETTL104")).ToString("#")
            WW_TA0007row("UNLOADCNT104") = Val(WW_TA0007row("UNLOADCNT104")).ToString("#")
            WW_TA0007row("UNLOADCNTCHO104") = Val(WW_TA0007row("UNLOADCNTCHO104")).ToString("#")
            WW_TA0007row("UNLOADCNTTTL104") = Val(WW_TA0007row("UNLOADCNTTTL104")).ToString("#")

            WW_TA0007row("OILPAYKBN105") = WW_TA0007row("OILPAYKBN105")
            WW_TA0007row("OILPAYKBNNAMES105") = WW_TA0007row("OILPAYKBNNAMES105")
            WW_TA0007row("HAIDISTANCE105") = Val(WW_TA0007row("HAIDISTANCE105")).ToString("#")
            WW_TA0007row("HAIDISTANCECHO105") = Val(WW_TA0007row("HAIDISTANCECHO105")).ToString("#")
            WW_TA0007row("HAIDISTANCETTL105") = Val(WW_TA0007row("HAIDISTANCETTL105")).ToString("#")
            WW_TA0007row("KAIDISTANCE105") = Val(WW_TA0007row("KAIDISTANCE105")).ToString("#")
            WW_TA0007row("KAIDISTANCECHO105") = Val(WW_TA0007row("KAIDISTANCECHO105")).ToString("#")
            WW_TA0007row("KAIDISTANCETTL105") = Val(WW_TA0007row("KAIDISTANCETTL105")).ToString("#")
            WW_TA0007row("UNLOADCNT105") = Val(WW_TA0007row("UNLOADCNT105")).ToString("#")
            WW_TA0007row("UNLOADCNTCHO105") = Val(WW_TA0007row("UNLOADCNTCHO105")).ToString("#")
            WW_TA0007row("UNLOADCNTTTL105") = Val(WW_TA0007row("UNLOADCNTTTL105")).ToString("#")

            WW_TA0007row("OILPAYKBN106") = WW_TA0007row("OILPAYKBN106")
            WW_TA0007row("OILPAYKBNNAMES106") = WW_TA0007row("OILPAYKBNNAMES106")
            WW_TA0007row("HAIDISTANCE106") = Val(WW_TA0007row("HAIDISTANCE106")).ToString("#")
            WW_TA0007row("HAIDISTANCECHO106") = Val(WW_TA0007row("HAIDISTANCECHO106")).ToString("#")
            WW_TA0007row("HAIDISTANCETTL106") = Val(WW_TA0007row("HAIDISTANCETTL106")).ToString("#")
            WW_TA0007row("KAIDISTANCE106") = Val(WW_TA0007row("KAIDISTANCE106")).ToString("#")
            WW_TA0007row("KAIDISTANCECHO106") = Val(WW_TA0007row("KAIDISTANCECHO106")).ToString("#")
            WW_TA0007row("KAIDISTANCETTL106") = Val(WW_TA0007row("KAIDISTANCETTL106")).ToString("#")
            WW_TA0007row("UNLOADCNT106") = Val(WW_TA0007row("UNLOADCNT106")).ToString("#")
            WW_TA0007row("UNLOADCNTCHO106") = Val(WW_TA0007row("UNLOADCNTCHO106")).ToString("#")
            WW_TA0007row("UNLOADCNTTTL106") = Val(WW_TA0007row("UNLOADCNTTTL106")).ToString("#")

            WW_TA0007row("OILPAYKBN107") = WW_TA0007row("OILPAYKBN107")
            WW_TA0007row("OILPAYKBNNAMES107") = WW_TA0007row("OILPAYKBNNAMES107")
            WW_TA0007row("HAIDISTANCE107") = Val(WW_TA0007row("HAIDISTANCE107")).ToString("#")
            WW_TA0007row("HAIDISTANCECHO107") = Val(WW_TA0007row("HAIDISTANCECHO107")).ToString("#")
            WW_TA0007row("HAIDISTANCETTL107") = Val(WW_TA0007row("HAIDISTANCETTL107")).ToString("#")
            WW_TA0007row("KAIDISTANCE107") = Val(WW_TA0007row("KAIDISTANCE107")).ToString("#")
            WW_TA0007row("KAIDISTANCECHO107") = Val(WW_TA0007row("KAIDISTANCECHO107")).ToString("#")
            WW_TA0007row("KAIDISTANCETTL107") = Val(WW_TA0007row("KAIDISTANCETTL107")).ToString("#")
            WW_TA0007row("UNLOADCNT107") = Val(WW_TA0007row("UNLOADCNT107")).ToString("#")
            WW_TA0007row("UNLOADCNTCHO107") = Val(WW_TA0007row("UNLOADCNTCHO107")).ToString("#")
            WW_TA0007row("UNLOADCNTTTL107") = Val(WW_TA0007row("UNLOADCNTTTL107")).ToString("#")

            WW_TA0007row("OILPAYKBN108") = WW_TA0007row("OILPAYKBN108")
            WW_TA0007row("OILPAYKBNNAMES108") = WW_TA0007row("OILPAYKBNNAMES108")
            WW_TA0007row("HAIDISTANCE108") = Val(WW_TA0007row("HAIDISTANCE108")).ToString("#")
            WW_TA0007row("HAIDISTANCECHO108") = Val(WW_TA0007row("HAIDISTANCECHO108")).ToString("#")
            WW_TA0007row("HAIDISTANCETTL108") = Val(WW_TA0007row("HAIDISTANCETTL108")).ToString("#")
            WW_TA0007row("KAIDISTANCE108") = Val(WW_TA0007row("KAIDISTANCE108")).ToString("#")
            WW_TA0007row("KAIDISTANCECHO108") = Val(WW_TA0007row("KAIDISTANCECHO108")).ToString("#")
            WW_TA0007row("KAIDISTANCETTL108") = Val(WW_TA0007row("KAIDISTANCETTL108")).ToString("#")
            WW_TA0007row("UNLOADCNT108") = Val(WW_TA0007row("UNLOADCNT108")).ToString("#")
            WW_TA0007row("UNLOADCNTCHO108") = Val(WW_TA0007row("UNLOADCNTCHO108")).ToString("#")
            WW_TA0007row("UNLOADCNTTTL108") = Val(WW_TA0007row("UNLOADCNTTTL108")).ToString("#")

            WW_TA0007row("OILPAYKBN109") = WW_TA0007row("OILPAYKBN109")
            WW_TA0007row("OILPAYKBNNAMES109") = WW_TA0007row("OILPAYKBNNAMES109")
            WW_TA0007row("HAIDISTANCE109") = Val(WW_TA0007row("HAIDISTANCE109")).ToString("#")
            WW_TA0007row("HAIDISTANCECHO109") = Val(WW_TA0007row("HAIDISTANCECHO109")).ToString("#")
            WW_TA0007row("HAIDISTANCETTL109") = Val(WW_TA0007row("HAIDISTANCETTL109")).ToString("#")
            WW_TA0007row("KAIDISTANCE109") = Val(WW_TA0007row("KAIDISTANCE109")).ToString("#")
            WW_TA0007row("KAIDISTANCECHO109") = Val(WW_TA0007row("KAIDISTANCECHO109")).ToString("#")
            WW_TA0007row("KAIDISTANCETTL109") = Val(WW_TA0007row("KAIDISTANCETTL109")).ToString("#")
            WW_TA0007row("UNLOADCNT109") = Val(WW_TA0007row("UNLOADCNT109")).ToString("#")
            WW_TA0007row("UNLOADCNTCHO109") = Val(WW_TA0007row("UNLOADCNTCHO109")).ToString("#")
            WW_TA0007row("UNLOADCNTTTL109") = Val(WW_TA0007row("UNLOADCNTTTL109")).ToString("#")

            WW_TA0007row("OILPAYKBN110") = WW_TA0007row("OILPAYKBN110")
            WW_TA0007row("OILPAYKBNNAMES110") = WW_TA0007row("OILPAYKBNNAMES110")
            WW_TA0007row("HAIDISTANCE110") = Val(WW_TA0007row("HAIDISTANCE110")).ToString("#")
            WW_TA0007row("HAIDISTANCECHO110") = Val(WW_TA0007row("HAIDISTANCECHO110")).ToString("#")
            WW_TA0007row("HAIDISTANCETTL110") = Val(WW_TA0007row("HAIDISTANCETTL110")).ToString("#")
            WW_TA0007row("KAIDISTANCE110") = Val(WW_TA0007row("KAIDISTANCE110")).ToString("#")
            WW_TA0007row("KAIDISTANCECHO110") = Val(WW_TA0007row("KAIDISTANCECHO110")).ToString("#")
            WW_TA0007row("KAIDISTANCETTL110") = Val(WW_TA0007row("KAIDISTANCETTL110")).ToString("#")
            WW_TA0007row("UNLOADCNT110") = Val(WW_TA0007row("UNLOADCNT110")).ToString("#")
            WW_TA0007row("UNLOADCNTCHO110") = Val(WW_TA0007row("UNLOADCNTCHO110")).ToString("#")
            WW_TA0007row("UNLOADCNTTTL110") = Val(WW_TA0007row("UNLOADCNTTTL110")).ToString("#")

            WW_TA0007row("OILPAYKBN201") = WW_TA0007row("OILPAYKBN201")
            WW_TA0007row("OILPAYKBNNAMES201") = WW_TA0007row("OILPAYKBNNAMES201")
            WW_TA0007row("HAIDISTANCE201") = Val(WW_TA0007row("HAIDISTANCE201")).ToString("#")
            WW_TA0007row("HAIDISTANCECHO201") = Val(WW_TA0007row("HAIDISTANCECHO201")).ToString("#")
            WW_TA0007row("HAIDISTANCETTL201") = Val(WW_TA0007row("HAIDISTANCETTL201")).ToString("#")
            WW_TA0007row("KAIDISTANCE201") = Val(WW_TA0007row("KAIDISTANCE201")).ToString("#")
            WW_TA0007row("KAIDISTANCECHO201") = Val(WW_TA0007row("KAIDISTANCECHO201")).ToString("#")
            WW_TA0007row("KAIDISTANCETTL201") = Val(WW_TA0007row("KAIDISTANCETTL201")).ToString("#")
            WW_TA0007row("UNLOADCNT201") = Val(WW_TA0007row("UNLOADCNT201")).ToString("#")
            WW_TA0007row("UNLOADCNTCHO201") = Val(WW_TA0007row("UNLOADCNTCHO201")).ToString("#")
            WW_TA0007row("UNLOADCNTTTL201") = Val(WW_TA0007row("UNLOADCNTTTL201")).ToString("#")

            WW_TA0007row("OILPAYKBN202") = WW_TA0007row("OILPAYKBN202")
            WW_TA0007row("OILPAYKBNNAMES202") = WW_TA0007row("OILPAYKBNNAMES202")
            WW_TA0007row("HAIDISTANCE202") = Val(WW_TA0007row("HAIDISTANCE202")).ToString("#")
            WW_TA0007row("HAIDISTANCECHO202") = Val(WW_TA0007row("HAIDISTANCECHO202")).ToString("#")
            WW_TA0007row("HAIDISTANCETTL202") = Val(WW_TA0007row("HAIDISTANCETTL202")).ToString("#")
            WW_TA0007row("KAIDISTANCE202") = Val(WW_TA0007row("KAIDISTANCE202")).ToString("#")
            WW_TA0007row("KAIDISTANCECHO202") = Val(WW_TA0007row("KAIDISTANCECHO202")).ToString("#")
            WW_TA0007row("KAIDISTANCETTL202") = Val(WW_TA0007row("KAIDISTANCETTL202")).ToString("#")
            WW_TA0007row("UNLOADCNT202") = Val(WW_TA0007row("UNLOADCNT202")).ToString("#")
            WW_TA0007row("UNLOADCNTCHO202") = Val(WW_TA0007row("UNLOADCNTCHO202")).ToString("#")
            WW_TA0007row("UNLOADCNTTTL202") = Val(WW_TA0007row("UNLOADCNTTTL202")).ToString("#")

            WW_TA0007row("OILPAYKBN203") = WW_TA0007row("OILPAYKBN203")
            WW_TA0007row("OILPAYKBNNAMES203") = WW_TA0007row("OILPAYKBNNAMES203")
            WW_TA0007row("HAIDISTANCE203") = Val(WW_TA0007row("HAIDISTANCE203")).ToString("#")
            WW_TA0007row("HAIDISTANCECHO203") = Val(WW_TA0007row("HAIDISTANCECHO203")).ToString("#")
            WW_TA0007row("HAIDISTANCETTL203") = Val(WW_TA0007row("HAIDISTANCETTL203")).ToString("#")
            WW_TA0007row("KAIDISTANCE203") = Val(WW_TA0007row("KAIDISTANCE203")).ToString("#")
            WW_TA0007row("KAIDISTANCECHO203") = Val(WW_TA0007row("KAIDISTANCECHO203")).ToString("#")
            WW_TA0007row("KAIDISTANCETTL203") = Val(WW_TA0007row("KAIDISTANCETTL203")).ToString("#")
            WW_TA0007row("UNLOADCNT203") = Val(WW_TA0007row("UNLOADCNT203")).ToString("#")
            WW_TA0007row("UNLOADCNTCHO203") = Val(WW_TA0007row("UNLOADCNTCHO203")).ToString("#")
            WW_TA0007row("UNLOADCNTTTL203") = Val(WW_TA0007row("UNLOADCNTTTL203")).ToString("#")

            WW_TA0007row("OILPAYKBN204") = WW_TA0007row("OILPAYKBN204")
            WW_TA0007row("OILPAYKBNNAMES204") = WW_TA0007row("OILPAYKBNNAMES204")
            WW_TA0007row("HAIDISTANCE204") = Val(WW_TA0007row("HAIDISTANCE204")).ToString("#")
            WW_TA0007row("HAIDISTANCECHO204") = Val(WW_TA0007row("HAIDISTANCECHO204")).ToString("#")
            WW_TA0007row("HAIDISTANCETTL204") = Val(WW_TA0007row("HAIDISTANCETTL204")).ToString("#")
            WW_TA0007row("KAIDISTANCE204") = Val(WW_TA0007row("KAIDISTANCE204")).ToString("#")
            WW_TA0007row("KAIDISTANCECHO204") = Val(WW_TA0007row("KAIDISTANCECHO204")).ToString("#")
            WW_TA0007row("KAIDISTANCETTL204") = Val(WW_TA0007row("KAIDISTANCETTL204")).ToString("#")
            WW_TA0007row("UNLOADCNT204") = Val(WW_TA0007row("UNLOADCNT204")).ToString("#")
            WW_TA0007row("UNLOADCNTCHO204") = Val(WW_TA0007row("UNLOADCNTCHO204")).ToString("#")
            WW_TA0007row("UNLOADCNTTTL204") = Val(WW_TA0007row("UNLOADCNTTTL204")).ToString("#")

            WW_TA0007row("OILPAYKBN205") = WW_TA0007row("OILPAYKBN205")
            WW_TA0007row("OILPAYKBNNAMES205") = WW_TA0007row("OILPAYKBNNAMES205")
            WW_TA0007row("HAIDISTANCE205") = Val(WW_TA0007row("HAIDISTANCE205")).ToString("#")
            WW_TA0007row("HAIDISTANCECHO205") = Val(WW_TA0007row("HAIDISTANCECHO205")).ToString("#")
            WW_TA0007row("HAIDISTANCETTL205") = Val(WW_TA0007row("HAIDISTANCETTL205")).ToString("#")
            WW_TA0007row("KAIDISTANCE205") = Val(WW_TA0007row("KAIDISTANCE205")).ToString("#")
            WW_TA0007row("KAIDISTANCECHO205") = Val(WW_TA0007row("KAIDISTANCECHO205")).ToString("#")
            WW_TA0007row("KAIDISTANCETTL205") = Val(WW_TA0007row("KAIDISTANCETTL205")).ToString("#")
            WW_TA0007row("UNLOADCNT205") = Val(WW_TA0007row("UNLOADCNT205")).ToString("#")
            WW_TA0007row("UNLOADCNTCHO205") = Val(WW_TA0007row("UNLOADCNTCHO205")).ToString("#")
            WW_TA0007row("UNLOADCNTTTL205") = Val(WW_TA0007row("UNLOADCNTTTL205")).ToString("#")

            WW_TA0007row("OILPAYKBN206") = WW_TA0007row("OILPAYKBN206")
            WW_TA0007row("OILPAYKBNNAMES206") = WW_TA0007row("OILPAYKBNNAMES206")
            WW_TA0007row("HAIDISTANCE206") = Val(WW_TA0007row("HAIDISTANCE206")).ToString("#")
            WW_TA0007row("HAIDISTANCECHO206") = Val(WW_TA0007row("HAIDISTANCECHO206")).ToString("#")
            WW_TA0007row("HAIDISTANCETTL206") = Val(WW_TA0007row("HAIDISTANCETTL206")).ToString("#")
            WW_TA0007row("KAIDISTANCE206") = Val(WW_TA0007row("KAIDISTANCE206")).ToString("#")
            WW_TA0007row("KAIDISTANCECHO206") = Val(WW_TA0007row("KAIDISTANCECHO206")).ToString("#")
            WW_TA0007row("KAIDISTANCETTL206") = Val(WW_TA0007row("KAIDISTANCETTL206")).ToString("#")
            WW_TA0007row("UNLOADCNT206") = Val(WW_TA0007row("UNLOADCNT206")).ToString("#")
            WW_TA0007row("UNLOADCNTCHO206") = Val(WW_TA0007row("UNLOADCNTCHO206")).ToString("#")
            WW_TA0007row("UNLOADCNTTTL206") = Val(WW_TA0007row("UNLOADCNTTTL206")).ToString("#")

            WW_TA0007row("OILPAYKBN207") = WW_TA0007row("OILPAYKBN207")
            WW_TA0007row("OILPAYKBNNAMES207") = WW_TA0007row("OILPAYKBNNAMES207")
            WW_TA0007row("HAIDISTANCE207") = Val(WW_TA0007row("HAIDISTANCE207")).ToString("#")
            WW_TA0007row("HAIDISTANCECHO207") = Val(WW_TA0007row("HAIDISTANCECHO207")).ToString("#")
            WW_TA0007row("HAIDISTANCETTL207") = Val(WW_TA0007row("HAIDISTANCETTL207")).ToString("#")
            WW_TA0007row("KAIDISTANCE207") = Val(WW_TA0007row("KAIDISTANCE207")).ToString("#")
            WW_TA0007row("KAIDISTANCECHO207") = Val(WW_TA0007row("KAIDISTANCECHO207")).ToString("#")
            WW_TA0007row("KAIDISTANCETTL207") = Val(WW_TA0007row("KAIDISTANCETTL207")).ToString("#")
            WW_TA0007row("UNLOADCNT207") = Val(WW_TA0007row("UNLOADCNT207")).ToString("#")
            WW_TA0007row("UNLOADCNTCHO207") = Val(WW_TA0007row("UNLOADCNTCHO207")).ToString("#")
            WW_TA0007row("UNLOADCNTTTL207") = Val(WW_TA0007row("UNLOADCNTTTL207")).ToString("#")

            WW_TA0007row("OILPAYKBN208") = WW_TA0007row("OILPAYKBN208")
            WW_TA0007row("OILPAYKBNNAMES208") = WW_TA0007row("OILPAYKBNNAMES208")
            WW_TA0007row("HAIDISTANCE208") = Val(WW_TA0007row("HAIDISTANCE208")).ToString("#")
            WW_TA0007row("HAIDISTANCECHO208") = Val(WW_TA0007row("HAIDISTANCECHO208")).ToString("#")
            WW_TA0007row("HAIDISTANCETTL208") = Val(WW_TA0007row("HAIDISTANCETTL208")).ToString("#")
            WW_TA0007row("KAIDISTANCE208") = Val(WW_TA0007row("KAIDISTANCE208")).ToString("#")
            WW_TA0007row("KAIDISTANCECHO208") = Val(WW_TA0007row("KAIDISTANCECHO208")).ToString("#")
            WW_TA0007row("KAIDISTANCETTL208") = Val(WW_TA0007row("KAIDISTANCETTL208")).ToString("#")
            WW_TA0007row("UNLOADCNT208") = Val(WW_TA0007row("UNLOADCNT208")).ToString("#")
            WW_TA0007row("UNLOADCNTCHO208") = Val(WW_TA0007row("UNLOADCNTCHO208")).ToString("#")
            WW_TA0007row("UNLOADCNTTTL208") = Val(WW_TA0007row("UNLOADCNTTTL208")).ToString("#")

            WW_TA0007row("OILPAYKBN209") = WW_TA0007row("OILPAYKBN209")
            WW_TA0007row("OILPAYKBNNAMES209") = WW_TA0007row("OILPAYKBNNAMES209")
            WW_TA0007row("HAIDISTANCE209") = Val(WW_TA0007row("HAIDISTANCE209")).ToString("#")
            WW_TA0007row("HAIDISTANCECHO209") = Val(WW_TA0007row("HAIDISTANCECHO209")).ToString("#")
            WW_TA0007row("HAIDISTANCETTL209") = Val(WW_TA0007row("HAIDISTANCETTL209")).ToString("#")
            WW_TA0007row("KAIDISTANCE209") = Val(WW_TA0007row("KAIDISTANCE209")).ToString("#")
            WW_TA0007row("KAIDISTANCECHO209") = Val(WW_TA0007row("KAIDISTANCECHO209")).ToString("#")
            WW_TA0007row("KAIDISTANCETTL209") = Val(WW_TA0007row("KAIDISTANCETTL209")).ToString("#")
            WW_TA0007row("UNLOADCNT209") = Val(WW_TA0007row("UNLOADCNT209")).ToString("#")
            WW_TA0007row("UNLOADCNTCHO209") = Val(WW_TA0007row("UNLOADCNTCHO209")).ToString("#")
            WW_TA0007row("UNLOADCNTTTL209") = Val(WW_TA0007row("UNLOADCNTTTL209")).ToString("#")

            WW_TA0007row("OILPAYKBN210") = WW_TA0007row("OILPAYKBN210")
            WW_TA0007row("OILPAYKBNNAMES210") = WW_TA0007row("OILPAYKBNNAMES210")
            WW_TA0007row("HAIDISTANCE210") = Val(WW_TA0007row("HAIDISTANCE210")).ToString("#")
            WW_TA0007row("HAIDISTANCECHO210") = Val(WW_TA0007row("HAIDISTANCECHO210")).ToString("#")
            WW_TA0007row("HAIDISTANCETTL210") = Val(WW_TA0007row("HAIDISTANCETTL210")).ToString("#")
            WW_TA0007row("KAIDISTANCE210") = Val(WW_TA0007row("KAIDISTANCE210")).ToString("#")
            WW_TA0007row("KAIDISTANCECHO210") = Val(WW_TA0007row("KAIDISTANCECHO210")).ToString("#")
            WW_TA0007row("KAIDISTANCETTL210") = Val(WW_TA0007row("KAIDISTANCETTL210")).ToString("#")
            WW_TA0007row("UNLOADCNT210") = Val(WW_TA0007row("UNLOADCNT210")).ToString("#")
            WW_TA0007row("UNLOADCNTCHO210") = Val(WW_TA0007row("UNLOADCNTCHO210")).ToString("#")
            WW_TA0007row("UNLOADCNTTTL210") = Val(WW_TA0007row("UNLOADCNTTTL210")).ToString("#")

            WW_TA0007tbl.Rows.Add(WW_TA0007row)
        Next

        IO_TBL = WW_TA0007tbl.Copy

        WW_TA0007tbl.Dispose()
        WW_TA0007tbl = Nothing

    End Sub

    ''' <summary>
    ''' TA0007ALLカラム設定
    ''' </summary>
    ''' <param name="IO_TBL">列情報設定対象テーブル</param>
    ''' <remarks></remarks>
    Protected Sub AddColumnToTA0007Tbl(ByRef IO_TBL As DataTable)

        If IsNothing(IO_TBL) Then IO_TBL = New DataTable
        If IO_TBL.Columns.Count <> 0 Then IO_TBL.Columns.Clear()

        'T0007DB項目作成
        IO_TBL.Clear()
        IO_TBL.Columns.Add("LINECNT", GetType(Integer))
        IO_TBL.Columns.Add("OPERATION", GetType(String))
        IO_TBL.Columns.Add("TIMSTP", GetType(String))
        IO_TBL.Columns.Add("SELECT", GetType(Integer))
        IO_TBL.Columns.Add("HIDDEN", GetType(Integer))

        IO_TBL.Columns.Add("CAMPCODE", GetType(String))
        IO_TBL.Columns.Add("CAMPNAMES", GetType(String))
        IO_TBL.Columns.Add("TAISHOYM", GetType(String))
        IO_TBL.Columns.Add("STAFFCODE", GetType(String))
        IO_TBL.Columns.Add("STAFFNAMES", GetType(String))
        IO_TBL.Columns.Add("WORKDATE", GetType(String))
        IO_TBL.Columns.Add("WORKINGWEEK", GetType(String))
        IO_TBL.Columns.Add("WORKINGWEEKNAMES", GetType(String))
        IO_TBL.Columns.Add("HDKBN", GetType(String))
        IO_TBL.Columns.Add("RECODEKBN", GetType(String))
        IO_TBL.Columns.Add("RECODEKBNNAMES", GetType(String))
        IO_TBL.Columns.Add("SEQ", GetType(String))
        IO_TBL.Columns.Add("ENTRYDATE", GetType(String))
        IO_TBL.Columns.Add("NIPPOLINKCODE", GetType(String))
        IO_TBL.Columns.Add("WORKKBN", GetType(String))
        IO_TBL.Columns.Add("WORKKBNNAMES", GetType(String))
        IO_TBL.Columns.Add("GSHABAN", GetType(String))
        IO_TBL.Columns.Add("GSHABANLICNPLTNO", GetType(String))
        IO_TBL.Columns.Add("STDATE", GetType(String))
        IO_TBL.Columns.Add("STTIME", GetType(String))
        IO_TBL.Columns.Add("ENDDATE", GetType(String))
        IO_TBL.Columns.Add("ENDTIME", GetType(String))
        IO_TBL.Columns.Add("WORKTIME", GetType(String))
        IO_TBL.Columns.Add("MOVETIME", GetType(String))
        IO_TBL.Columns.Add("ACTTIME", GetType(String))
        IO_TBL.Columns.Add("NIPPOBREAKTIME", GetType(String))
        IO_TBL.Columns.Add("SHARYOKBN", GetType(String))
        IO_TBL.Columns.Add("SHARYOKBNNAMES", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBN", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBNNAMES", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCE", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCECHO", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCETTL", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCE", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCECHO", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCETTL", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNT", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTCHO", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTTTL", GetType(String))
        IO_TBL.Columns.Add("STAFFKBN", GetType(String))
        IO_TBL.Columns.Add("STAFFKBNNAMES", GetType(String))
        IO_TBL.Columns.Add("MORG", GetType(String))
        IO_TBL.Columns.Add("MORGNAMES", GetType(String))
        IO_TBL.Columns.Add("HORG", GetType(String))
        IO_TBL.Columns.Add("HORGNAMES", GetType(String))
        IO_TBL.Columns.Add("SORG", GetType(String))
        IO_TBL.Columns.Add("SORGNAMES", GetType(String))
        IO_TBL.Columns.Add("HOLIDAYKBN", GetType(String))
        IO_TBL.Columns.Add("HOLIDAYKBNNAMES", GetType(String))
        IO_TBL.Columns.Add("PAYKBN", GetType(String))
        IO_TBL.Columns.Add("PAYKBNNAMES", GetType(String))
        IO_TBL.Columns.Add("SHUKCHOKKBN", GetType(String))
        IO_TBL.Columns.Add("SHUKCHOKKBNNAMES", GetType(String))
        IO_TBL.Columns.Add("BINDSTDATE", GetType(String))
        IO_TBL.Columns.Add("BINDTIME", GetType(String))
        IO_TBL.Columns.Add("BREAKTIME", GetType(String))
        IO_TBL.Columns.Add("BREAKTIMECHO", GetType(String))
        IO_TBL.Columns.Add("BREAKTIMETTL", GetType(String))
        IO_TBL.Columns.Add("NIGHTTIME", GetType(String))
        IO_TBL.Columns.Add("NIGHTTIMECHO", GetType(String))
        IO_TBL.Columns.Add("NIGHTTIMETTL", GetType(String))
        IO_TBL.Columns.Add("ORVERTIME", GetType(String))
        IO_TBL.Columns.Add("ORVERTIMECHO", GetType(String))
        IO_TBL.Columns.Add("ORVERTIMETTL", GetType(String))
        IO_TBL.Columns.Add("WNIGHTTIME", GetType(String))
        IO_TBL.Columns.Add("WNIGHTTIMECHO", GetType(String))
        IO_TBL.Columns.Add("WNIGHTTIMETTL", GetType(String))
        IO_TBL.Columns.Add("SWORKTIME", GetType(String))
        IO_TBL.Columns.Add("SWORKTIMECHO", GetType(String))
        IO_TBL.Columns.Add("SWORKTIMETTL", GetType(String))
        IO_TBL.Columns.Add("SNIGHTTIME", GetType(String))
        IO_TBL.Columns.Add("SNIGHTTIMECHO", GetType(String))
        IO_TBL.Columns.Add("SNIGHTTIMETTL", GetType(String))
        IO_TBL.Columns.Add("HWORKTIME", GetType(String))
        IO_TBL.Columns.Add("HWORKTIMECHO", GetType(String))
        IO_TBL.Columns.Add("HWORKTIMETTL", GetType(String))
        IO_TBL.Columns.Add("HNIGHTTIME", GetType(String))
        IO_TBL.Columns.Add("HNIGHTTIMECHO", GetType(String))
        IO_TBL.Columns.Add("HNIGHTTIMETTL", GetType(String))
        IO_TBL.Columns.Add("WORKNISSU", GetType(String))
        IO_TBL.Columns.Add("WORKNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("WORKNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("SHOUKETUNISSU", GetType(String))
        IO_TBL.Columns.Add("SHOUKETUNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("SHOUKETUNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("KUMIKETUNISSU", GetType(String))
        IO_TBL.Columns.Add("KUMIKETUNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("KUMIKETUNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("ETCKETUNISSU", GetType(String))
        IO_TBL.Columns.Add("ETCKETUNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("ETCKETUNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("NENKYUNISSU", GetType(String))
        IO_TBL.Columns.Add("NENKYUNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("NENKYUNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("TOKUKYUNISSU", GetType(String))
        IO_TBL.Columns.Add("TOKUKYUNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("TOKUKYUNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("CHIKOKSOTAINISSU", GetType(String))
        IO_TBL.Columns.Add("CHIKOKSOTAINISSUCHO", GetType(String))
        IO_TBL.Columns.Add("CHIKOKSOTAINISSUTTL", GetType(String))
        IO_TBL.Columns.Add("STOCKNISSU", GetType(String))
        IO_TBL.Columns.Add("STOCKNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("STOCKNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("KYOTEIWEEKNISSU", GetType(String))
        IO_TBL.Columns.Add("KYOTEIWEEKNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("KYOTEIWEEKNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("WEEKNISSU", GetType(String))
        IO_TBL.Columns.Add("WEEKNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("WEEKNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("DAIKYUNISSU", GetType(String))
        IO_TBL.Columns.Add("DAIKYUNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("DAIKYUNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("NENSHINISSU", GetType(String))
        IO_TBL.Columns.Add("NENSHINISSUCHO", GetType(String))
        IO_TBL.Columns.Add("NENSHINISSUTTL", GetType(String))
        IO_TBL.Columns.Add("NENMATUNISSU", GetType(String))
        IO_TBL.Columns.Add("NENMATUNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("NENMATUNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("SHUKCHOKNNISSU", GetType(String))
        IO_TBL.Columns.Add("SHUKCHOKNNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("SHUKCHOKNNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("SHUKCHOKNISSU", GetType(String))
        IO_TBL.Columns.Add("SHUKCHOKNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("SHUKCHOKNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("SHUKCHOKNHLDNISSU", GetType(String))
        IO_TBL.Columns.Add("SHUKCHOKNHLDNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("SHUKCHOKNHLDNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("SHUKCHOKHLDNISSU", GetType(String))
        IO_TBL.Columns.Add("SHUKCHOKHLDNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("SHUKCHOKHLDNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("TOKSAAKAISU", GetType(String))
        IO_TBL.Columns.Add("TOKSAAKAISUCHO", GetType(String))
        IO_TBL.Columns.Add("TOKSAAKAISUTTL", GetType(String))
        IO_TBL.Columns.Add("TOKSABKAISU", GetType(String))
        IO_TBL.Columns.Add("TOKSABKAISUCHO", GetType(String))
        IO_TBL.Columns.Add("TOKSABKAISUTTL", GetType(String))
        IO_TBL.Columns.Add("TOKSACKAISU", GetType(String))
        IO_TBL.Columns.Add("TOKSACKAISUCHO", GetType(String))
        IO_TBL.Columns.Add("TOKSACKAISUTTL", GetType(String))
        IO_TBL.Columns.Add("HOANTIME", GetType(String))
        IO_TBL.Columns.Add("HOANTIMECHO", GetType(String))
        IO_TBL.Columns.Add("HOANTIMETTL", GetType(String))
        IO_TBL.Columns.Add("KOATUTIME", GetType(String))
        IO_TBL.Columns.Add("KOATUTIMECHO", GetType(String))
        IO_TBL.Columns.Add("KOATUTIMETTL", GetType(String))
        IO_TBL.Columns.Add("TOKUSA1TIME", GetType(String))
        IO_TBL.Columns.Add("TOKUSA1TIMECHO", GetType(String))
        IO_TBL.Columns.Add("TOKUSA1TIMETTL", GetType(String))
        IO_TBL.Columns.Add("HAYADETIME", GetType(String))
        IO_TBL.Columns.Add("HAYADETIMECHO", GetType(String))
        IO_TBL.Columns.Add("HAYADETIMETTL", GetType(String))
        IO_TBL.Columns.Add("PONPNISSU", GetType(String))
        IO_TBL.Columns.Add("PONPNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("PONPNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("BULKNISSU", GetType(String))
        IO_TBL.Columns.Add("BULKNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("BULKNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("TRAILERNISSU", GetType(String))
        IO_TBL.Columns.Add("TRAILERNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("TRAILERNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("BKINMUKAISU", GetType(String))
        IO_TBL.Columns.Add("BKINMUKAISUCHO", GetType(String))
        IO_TBL.Columns.Add("BKINMUKAISUTTL", GetType(String))
        IO_TBL.Columns.Add("JIKYUSHATIME", GetType(String))
        IO_TBL.Columns.Add("JIKYUSHATIMECHO", GetType(String))
        IO_TBL.Columns.Add("JIKYUSHATIMETTL", GetType(String))
        IO_TBL.Columns.Add("HDAIWORKTIME", GetType(String))
        IO_TBL.Columns.Add("HDAIWORKTIMECHO", GetType(String))
        IO_TBL.Columns.Add("HDAIWORKTIMETTL", GetType(String))
        IO_TBL.Columns.Add("HDAINIGHTTIME", GetType(String))
        IO_TBL.Columns.Add("HDAINIGHTTIMECHO", GetType(String))
        IO_TBL.Columns.Add("HDAINIGHTTIMETTL", GetType(String))
        IO_TBL.Columns.Add("SDAIWORKTIME", GetType(String))
        IO_TBL.Columns.Add("SDAIWORKTIMECHO", GetType(String))
        IO_TBL.Columns.Add("SDAIWORKTIMETTL", GetType(String))
        IO_TBL.Columns.Add("SDAINIGHTTIME", GetType(String))
        IO_TBL.Columns.Add("SDAINIGHTTIMECHO", GetType(String))
        IO_TBL.Columns.Add("SDAINIGHTTIMETTL", GetType(String))
        IO_TBL.Columns.Add("HWORKNISSU", GetType(String))
        IO_TBL.Columns.Add("HWORKNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("HWORKNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("DELFLG", GetType(String))

        IO_TBL.Columns.Add("CAMPCODE_TXT", GetType(String))
        IO_TBL.Columns.Add("TAISHOYM_TXT", GetType(String))
        IO_TBL.Columns.Add("STAFFCODE_TXT", GetType(String))
        IO_TBL.Columns.Add("WORKDATE_TXT", GetType(String))
        IO_TBL.Columns.Add("WORKINGWEEK_TXT", GetType(String))
        IO_TBL.Columns.Add("HDKBN_TXT", GetType(String))
        IO_TBL.Columns.Add("RECODEKBN_TXT", GetType(String))
        IO_TBL.Columns.Add("WORKKBN_TXT", GetType(String))
        IO_TBL.Columns.Add("SHARYOKBN_TXT", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBN_TXT", GetType(String))
        IO_TBL.Columns.Add("STAFFKBN_TXT", GetType(String))
        IO_TBL.Columns.Add("MORG_TXT", GetType(String))
        IO_TBL.Columns.Add("HORG_TXT", GetType(String))
        IO_TBL.Columns.Add("SORG_TXT", GetType(String))
        IO_TBL.Columns.Add("HOLIDAYKBN_TXT", GetType(String))
        IO_TBL.Columns.Add("PAYKBN_TXT", GetType(String))
        IO_TBL.Columns.Add("SHUKCHOKKBN_TXT", GetType(String))
        IO_TBL.Columns.Add("DELFLG_TXT", GetType(String))

        IO_TBL.Columns.Add("SHARYOKBN1", GetType(String))
        IO_TBL.Columns.Add("SHARYOKBNNAMES1", GetType(String))

        IO_TBL.Columns.Add("OILPAYKBN101", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBNNAMES101", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCE101", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCECHO101", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCETTL101", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCE101", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCECHO101", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCETTL101", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNT101", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTCHO101", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTTTL101", GetType(String))

        IO_TBL.Columns.Add("OILPAYKBN102", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBNNAMES102", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCE102", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCECHO102", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCETTL102", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCE102", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCECHO102", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCETTL102", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNT102", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTCHO102", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTTTL102", GetType(String))

        IO_TBL.Columns.Add("OILPAYKBN103", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBNNAMES103", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCE103", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCECHO103", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCETTL103", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCE103", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCECHO103", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCETTL103", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNT103", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTCHO103", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTTTL103", GetType(String))

        IO_TBL.Columns.Add("OILPAYKBN104", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBNNAMES104", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCE104", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCECHO104", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCETTL104", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCE104", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCECHO104", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCETTL104", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNT104", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTCHO104", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTTTL104", GetType(String))

        IO_TBL.Columns.Add("OILPAYKBN105", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBNNAMES105", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCE105", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCECHO105", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCETTL105", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCE105", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCECHO105", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCETTL105", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNT105", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTCHO105", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTTTL105", GetType(String))

        IO_TBL.Columns.Add("OILPAYKBN106", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBNNAMES106", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCE106", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCECHO106", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCETTL106", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCE106", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCECHO106", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCETTL106", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNT106", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTCHO106", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTTTL106", GetType(String))

        IO_TBL.Columns.Add("OILPAYKBN107", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBNNAMES107", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCE107", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCECHO107", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCETTL107", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCE107", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCECHO107", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCETTL107", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNT107", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTCHO107", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTTTL107", GetType(String))

        IO_TBL.Columns.Add("OILPAYKBN108", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBNNAMES108", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCE108", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCECHO108", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCETTL108", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCE108", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCECHO108", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCETTL108", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNT108", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTCHO108", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTTTL108", GetType(String))

        IO_TBL.Columns.Add("OILPAYKBN109", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBNNAMES109", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCE109", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCECHO109", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCETTL109", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCE109", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCECHO109", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCETTL109", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNT109", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTCHO109", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTTTL109", GetType(String))

        IO_TBL.Columns.Add("OILPAYKBN110", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBNNAMES110", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCE110", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCECHO110", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCETTL110", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCE110", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCECHO110", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCETTL110", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNT110", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTCHO110", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTTTL110", GetType(String))

        IO_TBL.Columns.Add("SHARYOKBN2", GetType(String))
        IO_TBL.Columns.Add("SHARYOKBNNAMES2", GetType(String))

        IO_TBL.Columns.Add("OILPAYKBN201", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBNNAMES201", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCE201", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCECHO201", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCETTL201", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCE201", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCECHO201", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCETTL201", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNT201", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTCHO201", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTTTL201", GetType(String))

        IO_TBL.Columns.Add("OILPAYKBN202", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBNNAMES202", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCE202", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCECHO202", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCETTL202", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCE202", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCECHO202", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCETTL202", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNT202", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTCHO202", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTTTL202", GetType(String))

        IO_TBL.Columns.Add("OILPAYKBN203", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBNNAMES203", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCE203", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCECHO203", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCETTL203", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCE203", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCECHO203", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCETTL203", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNT203", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTCHO203", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTTTL203", GetType(String))

        IO_TBL.Columns.Add("OILPAYKBN204", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBNNAMES204", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCE204", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCECHO204", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCETTL204", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCE204", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCECHO204", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCETTL204", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNT204", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTCHO204", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTTTL204", GetType(String))

        IO_TBL.Columns.Add("OILPAYKBN205", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBNNAMES205", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCE205", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCECHO205", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCETTL205", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCE205", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCECHO205", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCETTL205", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNT205", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTCHO205", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTTTL205", GetType(String))

        IO_TBL.Columns.Add("OILPAYKBN206", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBNNAMES206", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCE206", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCECHO206", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCETTL206", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCE206", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCECHO206", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCETTL206", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNT206", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTCHO206", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTTTL206", GetType(String))

        IO_TBL.Columns.Add("OILPAYKBN207", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBNNAMES207", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCE207", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCECHO207", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCETTL207", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCE207", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCECHO207", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCETTL207", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNT207", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTCHO207", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTTTL207", GetType(String))

        IO_TBL.Columns.Add("OILPAYKBN208", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBNNAMES208", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCE208", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCECHO208", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCETTL208", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCE208", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCECHO208", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCETTL208", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNT208", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTCHO208", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTTTL208", GetType(String))

        IO_TBL.Columns.Add("OILPAYKBN209", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBNNAMES209", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCE209", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCECHO209", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCETTL209", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCE209", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCECHO209", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCETTL209", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNT209", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTCHO209", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTTTL209", GetType(String))

        IO_TBL.Columns.Add("OILPAYKBN210", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBNNAMES210", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCE210", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCECHO210", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCETTL210", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCE210", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCECHO210", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCETTL210", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNT210", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTCHO210", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTTTL210", GetType(String))

        IO_TBL.Columns.Add("DATAKBN", GetType(String))

    End Sub

    ''' <summary>
    ''' 名称取得
    ''' </summary>
    ''' <param name="I_FIELD">フィールド名</param>
    ''' <param name="I_VALUE">コード値</param>
    ''' <param name="O_TEXT">名称</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub CodeToName(ByVal I_FIELD As String,
                               ByRef I_VALUE As String,
                               ByRef O_TEXT As String,
                               ByRef O_RTN As String)

        '○名称取得
        O_TEXT = String.Empty
        O_RTN = C_MESSAGE_NO.NORMAL

        If Not String.IsNullOrEmpty(I_VALUE) Then
            Select Case I_FIELD
                Case "OVERTIMESTAFFKBN"
                    '社員区分名称
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "T0009_STAFFKBN"))
                Case "WORKKBN"
                    '作業区分名称
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "WORKKBN"))
                Case "DELFLG"
                    '削除フラグ名称
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DELFLG"))
                Case "STAFFCODE"
                    '乗務員名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, work.GetStaffCodeList(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_HORG.Text, work.WF_SEL_TAISHOYM.Text))
                Case "CAMPCODE"
                    '会社名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text))
                Case "ORG"
                    '出荷部署名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateHORGParam(work.WF_SEL_CAMPCODE.Text, C_PERMISSION.INVALID))
                Case "CREWKBN"
                    '実績登録区分名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "CREWKBN"))
            End Select
        End If

    End Sub

    ''' <summary>
    ''' テーブルデータのコピー処理
    ''' </summary>
    ''' <param name="I_TBL">コピー元</param>
    ''' <param name="O_TBL">コピー先</param>
    ''' <remarks>ついでに１０分切替も行っている</remarks>
    Protected Sub CopyT7Edit(ByVal I_TBL As DataTable, ByRef O_TBL As DataTable)

        O_TBL = I_TBL.Copy
        Dim WW_SPLIT_MINUITE As Integer = 0
        Dim WW_ROUND_TYPE As GRT00009TIMEFORMAT.EM_ROUND_TYPE
        work.GetRoundType(work.WF_SEL_CAMPCODE.Text, WW_SPLIT_MINUITE, WW_ROUND_TYPE)
        For Each Orow As DataRow In O_TBL.Rows
            Orow("NIGHTTIME") = T0009TIME.RoundMinute(Orow("NIGHTTIME"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("NIGHTTIMECHO") = T0009TIME.RoundMinute(Orow("NIGHTTIMECHO"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("NIGHTTIMETTL") = T0009TIME.RoundMinute(Orow("NIGHTTIMETTL"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("ORVERTIME") = T0009TIME.RoundMinute(Orow("ORVERTIME"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("ORVERTIMECHO") = T0009TIME.RoundMinute(Orow("ORVERTIMECHO"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("ORVERTIMETTL") = T0009TIME.RoundMinute(Orow("ORVERTIMETTL"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("WNIGHTTIME") = T0009TIME.RoundMinute(Orow("WNIGHTTIME"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("WNIGHTTIMECHO") = T0009TIME.RoundMinute(Orow("WNIGHTTIMECHO"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("WNIGHTTIMETTL") = T0009TIME.RoundMinute(Orow("WNIGHTTIMETTL"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("SWORKTIME") = T0009TIME.RoundMinute(Orow("SWORKTIME"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("SWORKTIMECHO") = T0009TIME.RoundMinute(Orow("SWORKTIMECHO"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("SWORKTIMETTL") = T0009TIME.RoundMinute(Orow("SWORKTIMETTL"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("SNIGHTTIME") = T0009TIME.RoundMinute(Orow("SNIGHTTIME"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("SNIGHTTIMECHO") = T0009TIME.RoundMinute(Orow("SNIGHTTIMECHO"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("SNIGHTTIMETTL") = T0009TIME.RoundMinute(Orow("SNIGHTTIMETTL"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("HWORKTIME") = T0009TIME.RoundMinute(Orow("HWORKTIME"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("HWORKTIMECHO") = T0009TIME.RoundMinute(Orow("HWORKTIMECHO"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("HWORKTIMETTL") = T0009TIME.RoundMinute(Orow("HWORKTIMETTL"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("HNIGHTTIME") = T0009TIME.RoundMinute(Orow("HNIGHTTIME"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("HNIGHTTIMECHO") = T0009TIME.RoundMinute(Orow("HNIGHTTIMECHO"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("HNIGHTTIMETTL") = T0009TIME.RoundMinute(Orow("HNIGHTTIMETTL"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("HOANTIME") = T0009TIME.RoundMinute(Orow("HOANTIME"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("HOANTIMECHO") = T0009TIME.RoundMinute(Orow("HOANTIMECHO"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("HOANTIMETTL") = T0009TIME.RoundMinute(Orow("HOANTIMETTL"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("KOATUTIME") = T0009TIME.RoundMinute(Orow("KOATUTIME"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("KOATUTIMECHO") = T0009TIME.RoundMinute(Orow("KOATUTIMECHO"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("KOATUTIMETTL") = T0009TIME.RoundMinute(Orow("KOATUTIMETTL"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("TOKUSA1TIME") = T0009TIME.RoundMinute(Orow("TOKUSA1TIME"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("TOKUSA1TIMECHO") = T0009TIME.RoundMinute(Orow("TOKUSA1TIMECHO"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("TOKUSA1TIMETTL") = T0009TIME.RoundMinute(Orow("TOKUSA1TIMETTL"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("HAYADETIME") = T0009TIME.RoundMinute(Orow("HAYADETIME"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("HAYADETIMECHO") = T0009TIME.RoundMinute(Orow("HAYADETIMECHO"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("HAYADETIMETTL") = T0009TIME.RoundMinute(Orow("HAYADETIMETTL"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("JIKYUSHATIME") = T0009TIME.RoundMinute(Orow("JIKYUSHATIME"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("JIKYUSHATIMECHO") = T0009TIME.RoundMinute(Orow("JIKYUSHATIMECHO"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("JIKYUSHATIMETTL") = T0009TIME.RoundMinute(Orow("JIKYUSHATIMETTL"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("HDAIWORKTIME") = T0009TIME.RoundMinute(Orow("HDAIWORKTIME"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("HDAIWORKTIMECHO") = T0009TIME.RoundMinute(Orow("HDAIWORKTIMECHO"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("HDAIWORKTIMETTL") = T0009TIME.RoundMinute(Orow("HDAIWORKTIMETTL"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("HDAINIGHTTIME") = T0009TIME.RoundMinute(Orow("HDAINIGHTTIME"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("HDAINIGHTTIMECHO") = T0009TIME.RoundMinute(Orow("HDAINIGHTTIMECHO"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("HDAINIGHTTIMETTL") = T0009TIME.RoundMinute(Orow("HDAINIGHTTIMETTL"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("SDAIWORKTIME") = T0009TIME.RoundMinute(Orow("SDAIWORKTIME"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("SDAIWORKTIMECHO") = T0009TIME.RoundMinute(Orow("SDAIWORKTIMECHO"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("SDAIWORKTIMETTL") = T0009TIME.RoundMinute(Orow("SDAIWORKTIMETTL"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("SDAINIGHTTIME") = T0009TIME.RoundMinute(Orow("SDAINIGHTTIME"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("SDAINIGHTTIMECHO") = T0009TIME.RoundMinute(Orow("SDAINIGHTTIMECHO"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("SDAINIGHTTIMETTL") = T0009TIME.RoundMinute(Orow("SDAINIGHTTIMETTL"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
        Next

    End Sub

    ''' <summary>
    ''' 遷移時の引き渡しパラメータの取得
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub MapRefelence()

        '■■■ 選択画面の入力初期値設定 ■■■
        If Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.TA0007S Then                                                       '条件画面からの画面遷移

            If Not IsNothing(Master.MAPID) Then Master.MAPID = GRTA0007WRKINC.MAPID
            '○Grid情報保存先のファイル名
            Master.createXMLSaveFile()
        End If

    End Sub

End Class





