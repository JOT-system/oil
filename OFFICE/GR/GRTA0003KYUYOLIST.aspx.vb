Imports System.Data.SqlClient

Public Class GRTA0003KYUYOLIST
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
    ''' GridView用テーブルソート文字列取得
    ''' </summary>
    Private CS0026TblSort As New CS0026TBLSORT                      'GridView用テーブルソート文字列取得
    ''' <summary>
    ''' 帳票出力(入力：TBL)
    ''' </summary>
    Private CS0030REPORT As New CS0030REPORT                        '帳票出力(入力：TBL)
    ''' <summary>
    ''' セッション情報
    ''' </summary>
    Private CS0050Session As New CS0050SESSION                      'セッション情報
    ''' <summary>
    ''' 明細項目設定用
    ''' </summary>
    Private CS0052DetailView As New CS0052DetailView                '明細項目設定用
    ''' <summary>
    ''' 勤怠共通
    ''' </summary>
    Private T0007COM As New GRT0007COM                              '勤怠共通
    ''' <summary>
    ''' 営勤共通
    ''' </summary>
    Private T0009COM As New GRT00009COM                             '営勤共通
    ''' <summary>
    ''' 時間調整共通
    ''' </summary>
    Private T0009TIME As New GRT00009TIMEFORMAT                     '時間調整共通
    '検索結果格納ds
    Private TA0003ALL As DataTable                                  '全データテーブル
    Private TA0003VIEWtbl As DataTable                              'Grid格納用テーブル

    '共通処理結果
    ''' <summary>
    ''' 共通用エラーID保持枠
    ''' </summary>
    Private WW_ERRCODE As String = String.Empty             'リターンコード
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
    Private Const CONST_DSPROWCOUNT As Integer = 40         '１画面表示対象
    ''' <summary>
    ''' 一覧のマウススクロール時の増分（件数）
    ''' </summary>
    Private Const CONST_SCROLLROWCOUNT As Integer = 20      'マウススクロール時の増分
    ''' <summary>
    ''' 詳細部タブID
    ''' </summary>
    Private Const CONST_DETAIL_TABID As String = "DTL1"     '詳細部タブID
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
                    Case "WF_BACK"                      '■ 戻るボタンクリック時処理
                        WF_BACK_Click()
                    Case "WF_ButtonEND"                 '■ 終了ボタンクリック時処理
                        WF_ButtonEND_Click()
                    Case "WF_GridDBclick"               '■ GridViewダブルクリック処理
                        WF_Grid_DBclick()
                    Case "WF_RadioButonClick"           '■ 右ボックスラジオボタン選択時処理 
                        WF_RadioButon_Click()
                    Case "WF_MEMOChange"                '■ メモ欄保存処理
                        WF_MEMO_Change()
                End Select
                '○ 一覧再表示処理
                DisplayGrid()
            End If
        Else
            '〇初期化処理
            Initialize()
        End If

        If Not IsNothing(TA0003ALL) Then
            TA0003ALL.Dispose()
            TA0003ALL = Nothing
        End If
        If Not IsNothing(TA0003VIEWtbl) Then
            TA0003VIEWtbl.Dispose()
            TA0003VIEWtbl = Nothing
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

        '右Boxへの値設定
        rightview.MAPID = Master.MAPID
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.TARGETDATE = work.WF_SEL_TAISHOYM.Text & "/01"
        rightview.Initialize(WW_DUMMY)
        '〇ヘッダー項目表示
        WF_SEL_DATE.Text = work.WF_SEL_TAISHOYM.Text
        CodeToName("ORG", work.WF_SEL_HORG.Text, WF_SEL_ORG.Text, WW_DUMMY)

        '■ 全データ取得
        '○TA0003ALL取得
        GetAllTA0003Tbl()
        '○ソート＆データ抽出
        CS0026TblSort.TABLE = TA0003ALL
        CS0026TblSort.SORTING = "LINECNT , SEQ ASC"
        CS0026TblSort.FILTER = String.Empty
        TA0003ALL = CS0026TblSort.sort()

        '○画面（GridView）表示データ保存
        If Not Master.SaveTable(TA0003ALL) Then Exit Sub
        '１０分切上処理（ENEX、NJS、KNK）、５分四捨五入処理（ＪＫＴ）
        CopyT7EditTbl(TA0003ALL, TA0003VIEWtbl)
        '重複チェック
        Dim WW_MSG As String = ""
        T0007COM.T0007_DuplCheck(TA0003ALL, WW_MSG, WW_ERRCODE)
        If Not isNormal(WW_ERRCODE) Then
            Master.output(WW_ERRCODE, C_MESSAGE_TYPE.ABORT)
        Else
            rightview.addErrorReport(ControlChars.NewLine & WW_MSG)
        End If

        '一覧表示データ編集（性能対策）
        Using TBLview As DataView = New DataView(TA0003VIEWtbl)
            TBLview.Sort = "LINECNT"
            TBLview.RowFilter = "HIDDEN = 0 and SELECT >= 1 and SELECT < " & (CONST_DSPROWCOUNT).ToString
            CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0013ProfView.PROFID = Master.PROF_VIEW
            CS0013ProfView.MAPID = GRTA0003WRKINC.MAPID
            CS0013ProfView.VARI = Master.VIEWID
            CS0013ProfView.SRCDATA = TBLview.ToTable
            CS0013ProfView.TBLOBJ = pnlListArea
            CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Horizontal
            CS0013ProfView.LEVENT = "ondblclick"
            CS0013ProfView.LFUNC = "ListDbClick"
            CS0013ProfView.TITLEOPT = True
            CS0013ProfView.HIDEOPERATIONOPT = True
            CS0013ProfView.TARGETDATE = work.WF_SEL_TAISHOYM.Text & "/01"
            CS0013ProfView.CS0013ProfView()
        End Using
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If

        work.WF_IsHideDetailBox.text = "1"
    End Sub
    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()
        '〇データリカバリ
        If IsNothing(TA0003ALL) Then
            If Not Master.RecoverTable(TA0003ALL) Then Exit Sub
        End If
        'TA0003VIEW設定
        GetViewToTA0003("H")

        Dim WW_GridPosition As Integer                 '表示位置（開始）
        Dim WW_DataCNT As Integer = 0                  '(絞り込み後)有効Data数

        '表示対象行カウント(絞り込み対象)
        '　※　絞込（Cells(4)： 0=表示対象 , 1=非表示対象)
        For i As Integer = 0 To TA0003VIEWtbl.Rows.Count - 1
            If TA0003VIEWtbl.Rows(i)(4) = "0" Then
                WW_DataCNT = WW_DataCNT + 1
                '行（ラインカウント）を再設定する。既存項目（SELECT）を利用
                TA0003VIEWtbl.Rows(i)("SELECT") = WW_DataCNT
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
        Using WW_TBLview As DataView = New DataView(TA0003VIEWtbl)

            'ソート
            WW_TBLview.Sort = "LINECNT"
            WW_TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString & " and SELECT < " & (WW_GridPosition + CONST_DSPROWCOUNT).ToString
            '一覧作成

            CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0013ProfView.PROFID = Master.PROF_VIEW
            CS0013ProfView.MAPID = GRTA0003WRKINC.MAPID
            CS0013ProfView.VARI = Master.VIEWID
            CS0013ProfView.SRCDATA = WW_TBLview.ToTable
            CS0013ProfView.TBLOBJ = pnlListArea
            CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Horizontal
            CS0013ProfView.LEVENT = "ondblclick"
            CS0013ProfView.LFUNC = "ListDbClick"
            CS0013ProfView.TITLEOPT = True
            CS0013ProfView.HIDEOPERATIONOPT = True
            CS0013ProfView.TARGETDATE = work.WF_SEL_TAISHOYM.Text & "/01"
            CS0013ProfView.CS0013ProfView()

            '○クリア
            If WW_TBLview.Count = 0 Then
                WF_GridPosition.Text = "1"
            Else
                WF_GridPosition.Text = WW_TBLview.Item(0)("SELECT")
            End If
        End Using
    End Sub
    ''' <summary>
    ''' ﾀﾞｳﾝﾛｰﾄﾞ(PDF出力)・一覧印刷ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonPDF_Click()

        '■ テーブルデータ 復元
        '○全表示データ 復元
        If IsNothing(TA0003ALL) Then
            If Not Master.RecoverTable(TA0003ALL) Then Exit Sub
        End If

        '■ 帳票出力
        '〇 TA0003VIEWtblカラム設定
        AddColumToTA0003Tbl(TA0003VIEWtbl)

        '○TA0003VIEWtbl取得
        GetViewToTA0003("H")

        '帳票出力用編集
        ListEdit(TA0003VIEWtbl)

        '○ 帳票出力
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text               '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                        'プロファイルID
        CS0030REPORT.MAPID = GRTA0003WRKINC.MAPID                       '画面ID
        CS0030REPORT.REPORTID = rightview.GetReportId()                 '帳票ID
        CS0030REPORT.FILEtyp = "pdf"                                    '出力ファイル形式
        CS0030REPORT.TBLDATA = TA0003VIEWtbl                            'データ参照DataTable
        CS0030REPORT.TARGETDATE = work.WF_SEL_TAISHOYM.Text & "/01"     '対象日付
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
        If IsNothing(TA0003ALL) Then
            If Not Master.RecoverTable(TA0003ALL) Then Exit Sub
        End If

        '■ 帳票出力
        '〇 カラム設定
        AddColumToTA0003Tbl(TA0003VIEWtbl)

        '○TA0003VIEWtbl取得
        GetViewToTA0003("H")

        '帳票出力用編集
        ListEdit(TA0003VIEWtbl)

        '○ 帳票出力
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text               '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                        'プロファイルID
        CS0030REPORT.MAPID = GRTA0003WRKINC.MAPID                       '画面ID
        CS0030REPORT.REPORTID = rightview.GetReportId()                 '帳票ID
        CS0030REPORT.FILEtyp = "XLSX"                                   '出力ファイル形式
        CS0030REPORT.TBLDATA = TA0003VIEWtbl                            'データ参照DataTable
        CS0030REPORT.TARGETDATE = work.WF_SEL_TAISHOYM.Text & "/01"     '対象日付
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
        If IsNothing(TA0003ALL) Then
            If Not Master.RecoverTable(TA0003ALL) Then Exit Sub
        End If

        '〇 TA0003VIEWtblカラム設定
        AddColumToTA0003Tbl(TA0003VIEWtbl)

        '○TA0003VIEWtbl取得
        GetViewToTA0003("H")

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
        If IsNothing(TA0003ALL) Then
            If Not Master.RecoverTable(TA0003ALL) Then Exit Sub
        End If

        '〇 TA0003VIEWtblカラム設定
        AddColumToTA0003Tbl(TA0003VIEWtbl)

        '○TA0003VIEWtbl取得
        GetViewToTA0003("H")

        '○ソート
        Using WW_TBLview As DataView = New DataView(TA0003VIEWtbl)
            WW_TBLview.RowFilter = "HIDDEN= '0'"

            '■ GridView表示
            '○ 最終頁に移動
            If WW_TBLview.Count Mod CONST_SCROLLROWCOUNT = 0 Then
                WF_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod CONST_SCROLLROWCOUNT)
            Else
                WF_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod CONST_SCROLLROWCOUNT) + 1
            End If
        End Using
    End Sub

    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_BACK_Click()

        '■ GridView表示データ作成
        '○データリカバリ 
        If IsNothing(TA0003ALL) Then If Not Master.RecoverTable(TA0003ALL) Then Exit Sub

        '〇 TA0003VIEWtblカラム設定
        AddColumToTA0003Tbl(TA0003VIEWtbl)

        '○TA0003VIEWtbl取得
        GetViewToTA0003("H")

        '〇画面切替設定
        work.WF_IsHideDetailBox.Text = "1"

    End Sub


    ' ******************************************************************************
    ' ***  共通処理                                                              ***　
    ' ******************************************************************************

    ''' <summary>
    ''' TA0003All全表示データ取得処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GetAllTA0003Tbl()
        '■ 画面表示用データ取得
        'TA0003テンポラリDB項目作成
        AddColumToTA0003Tbl(TA0003ALL)

        'オブジェクト内容検索
        Try
            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)


                'テンポラリーテーブルを作成する
                Dim SQLStr0 As String = "CREATE TABLE #MBtemp  " _
                        & " (                                  " _
                        & "  CAMPCODE nvarchar(20)  ,          " _
                        & "  STAFFCODE nvarchar(20) ,          " _
                        & "  HORG nvarchar(20)      ,          " _
                        & " )                                  "

                '検索SQL文
                Dim SQLStr1 As String = " SELECT  isnull(rtrim(MB1.CAMPCODE),'')      as  CAMPCODE, " _
                   & "              isnull(rtrim(MB1.STAFFCODE),'')     as  STAFFCODE " _
                   & " from   MB001_STAFF MB1 " _
                   & " INNER JOIN S0012_SRVAUTHOR X " _
                   & "   ON    X.TERMID       = @TERMID " _
                   & "   and   X.CAMPCODE     = @CAMPCODE " _
                   & "   and   X.OBJECT       = 'SRVORG' " _
                   & "   and   X.STYMD       <= @NOW " _
                   & "   and   X.ENDYMD      >= @NOW " _
                   & "   and   X.DELFLG      <> '1' " _
                   & " INNER JOIN S0006_ROLE Y " _
                   & "   ON    Y.CAMPCODE      = X.CAMPCODE " _
                   & "   and   Y.OBJECT        = 'SRVORG' " _
                   & "   and   Y.ROLE          = X.ROLE" _
                   & "   and   Y.STYMD        <= @NOW " _
                   & "   and   Y.ENDYMD       >= @NOW " _
                   & "   and   Y.DELFLG       <> '1' " _
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
                   & " where  MB1.CAMPCODE     =  @CAMPCODE " _
                   & "   and  MB1.STAFFKBN like  '03%' " _
                   & "   and  MB1.STYMD      <=  @SEL_ENDYMD " _
                   & "   and  MB1.ENDYMD     >=  @SEL_STYMD " _
                   & "   and  MB1.DELFLG     <>  '1' " _
                   & " group by MB1.CAMPCODE, MB1.STAFFCODE "
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
                   & "       isnull(A.ORVERTIME,0) + isnull(A.ORVERTIMECHO,0) as ORVERTIMETTL , " _
                   & "       isnull(A.WNIGHTTIME,0) as WNIGHTTIME , " _
                   & "       isnull(A.WNIGHTTIMECHO,0) as WNIGHTTIMECHO , " _
                   & "       isnull(A.WNIGHTTIME,0) + isnull(A.WNIGHTTIMECHO,0) as WNIGHTTIMETTL , " _
                   & "       isnull(A.SWORKTIME,0) as SWORKTIME , " _
                   & "       isnull(A.SWORKTIMECHO,0) as SWORKTIMECHO , " _
                   & "       isnull(A.SWORKTIME,0) + isnull(A.SWORKTIMECHO,0) as SWORKTIMETTL , " _
                   & "       isnull(A.SNIGHTTIME,0) as SNIGHTTIME , " _
                   & "       isnull(A.SNIGHTTIMECHO,0) as SNIGHTTIMECHO , " _
                   & "       isnull(A.SNIGHTTIME,0) + isnull(A.SNIGHTTIMECHO,0) as SNIGHTTIMETTL , " _
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
                   & "       isnull(A.SHUKCHOKNNISSU,0) as SHUKCHOKNNISSU , " _
                   & "       isnull(A.SHUKCHOKNNISSUCHO,0) as SHUKCHOKNNISSUCHO , " _
                   & "       isnull(A.SHUKCHOKNNISSU, 0) + isnull(A.SHUKCHOKNNISSUCHO, 0) as SHUKCHOKNNISSUTTL , " _
                   & "       isnull(A.SHUKCHOKNISSU,0) as SHUKCHOKNISSU , " _
                   & "       isnull(A.SHUKCHOKNISSUCHO,0) as SHUKCHOKNISSUCHO , " _
                   & "       isnull(A.SHUKCHOKNISSU, 0) + isnull(A.SHUKCHOKNISSUCHO, 0) as SHUKCHOKNISSUTTL , " _
                   & "       isnull(A.TOKSAAKAISU,0) as TOKSAAKAISU , " _
                   & "       isnull(A.TOKSAAKAISUCHO,0) as TOKSAAKAISUCHO , " _
                   & "       isnull(A.TOKSAAKAISU, 0) + isnull(A.TOKSAAKAISUCHO, 0) as TOKSAAKAISUTTL , " _
                   & "       isnull(A.TOKSABKAISU,0) as TOKSABKAISU , " _
                   & "       isnull(A.TOKSABKAISUCHO,0) as TOKSABKAISUCHO , " _
                   & "       isnull(A.TOKSABKAISU, 0) + isnull(A.TOKSABKAISUCHO, 0) as TOKSABKAISUTTL , " _
                   & "       isnull(A.TOKSACKAISU,0) as TOKSACKAISU , " _
                   & "       isnull(A.TOKSACKAISUCHO,0) as TOKSACKAISUCHO , " _
                   & "       isnull(A.TOKSACKAISU, 0) + isnull(A.TOKSACKAISUCHO, 0) as TOKSACKAISUTTL , " _
                   & "       isnull(A.TENKOKAISU,0) as TENKOKAISU , " _
                   & "       isnull(A.TENKOKAISUCHO,0) as TENKOKAISUCHO , " _
                   & "       isnull(A.TENKOKAISU, 0) + isnull(A.TENKOKAISUCHO, 0) as TENKOKAISUTTL , " _
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
                   & "       0 as KAITENCNT101 , " _
                   & "       0 as KAITENCNTCHO101 , " _
                   & "       0 as KAITENCNTTTL101 , " _
                   & "       0 as MODELDISTANCE0101 , " _
                   & "       0 as MODELDISTANCECHO0101 , " _
                   & "       0 as MODELDISTANCETTL0101 , " _
                   & "       '' as OILPAYKBN102 , " _
                   & "       '' as OILPAYKBNNAMES102 , " _
                   & "       0  as HAIDISTANCE102 , " _
                   & "       0 as HAIDISTANCECHO102 , " _
                   & "       0 as HAIDISTANCETTL102 , " _
                   & "       0 as KAIDISTANCE102 , " _
                   & "       0 as KAIDISTANCECHO102 , " _
                   & "       0 as KAIDISTANCETTL102 , " _
                   & "       0 as KAITENCNT102 , " _
                   & "       0 as KAITENCNTCHO102 , " _
                   & "       0 as KAITENCNTTTL102 , " _
                   & "       0 as MODELDISTANCE0102 , " _
                   & "       0 as MODELDISTANCECHO0102 , " _
                   & "       0 as MODELDISTANCETTL0102 , " _
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
                   & "       0 as KAITENCNT103 , " _
                   & "       0 as KAITENCNTCHO103 , " _
                   & "       0 as KAITENCNTTTL103 , " _
                   & "       0 as MODELDISTANCE0103 , " _
                   & "       0 as MODELDISTANCECHO0103 , " _
                   & "       0 as MODELDISTANCETTL0103 , " _
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
                   & "       0 as KAITENCNT104 , " _
                   & "       0 as KAITENCNTCHO104 , " _
                   & "       0 as KAITENCNTTTL104 , " _
                   & "       0 as MODELDISTANCE104 , " _
                   & "       0 as MODELDISTANCECHO104 , " _
                   & "       0 as MODELDISTANCETTL104 , " _
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
                   & "       0 as KAITENCNT105 , " _
                   & "       0 as KAITENCNTCHO105 , " _
                   & "       0 as KAITENCNTTTL105 , " _
                   & "       0 as MODELDISTANCE0105 , " _
                   & "       0 as MODELDISTANCECHO0105 , " _
                   & "       0 as MODELDISTANCETTL0105 , " _
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
                   & "       0 as KAITENCNT106 , " _
                   & "       0 as KAITENCNTCHO106 , " _
                   & "       0 as KAITENCNTTTL106 , " _
                   & "       0 as MODELDISTANCE0106 , " _
                   & "       0 as MODELDISTANCECHO0106 , " _
                   & "       0 as MODELDISTANCETTL0106 , " _
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
                   & "       0 as KAITENCNT107 , " _
                   & "       0 as KAITENCNTCHO107 , " _
                   & "       0 as KAITENCNTTTL107 , " _
                   & "       0 as MODELDISTANCE0107 , " _
                   & "       0 as MODELDISTANCECHO0107 , " _
                   & "       0 as MODELDISTANCETTL0107 , " _
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
                   & "       0 as KAITENCNT108 , " _
                   & "       0 as KAITENCNTCHO108 , " _
                   & "       0 as KAITENCNTTTL108 , " _
                   & "       0 as MODELDISTANCE0108 , " _
                   & "       0 as MODELDISTANCECHO0108 , " _
                   & "       0 as MODELDISTANCETTL0108 , " _
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
                   & "       0 as KAITENCNT109 , " _
                   & "       0 as KAITENCNTCHO109 , " _
                   & "       0 as KAITENCNTTTL109 , " _
                   & "       0 as MODELDISTANCE109 , " _
                   & "       0 as MODELDISTANCECHO109 , " _
                   & "       0 as MODELDISTANCETTL109 , " _
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
                   & "       0 as KAITENCNT110 , " _
                   & "       0 as KAITENCNTCHO110 , " _
                   & "       0 as KAITENCNTTTL110 , " _
                   & "       0 as MODELDISTANCE0110 , " _
                   & "       0 as MODELDISTANCECHO0110 , " _
                   & "       0 as MODELDISTANCETTL0110 , " _
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
                   & "       0 as KAITENCNT201 , " _
                   & "       0 as KAITENCNTCHO201 , " _
                   & "       0 as KAITENCNTTTL201 , " _
                   & "       0 as MODELDISTANCE0201 , " _
                   & "       0 as MODELDISTANCECHO0201 , " _
                   & "       0 as MODELDISTANCETTL0201 , " _
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
                   & "       0 as KAITENCNT202 , " _
                   & "       0 as KAITENCNTCHO202 , " _
                   & "       0 as KAITENCNTTTL202 , " _
                   & "       0 as MODELDISTANCE0202 , " _
                   & "       0 as MODELDISTANCECHO0202 , " _
                   & "       0 as MODELDISTANCETTL0202 , " _
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
                   & "       0 as KAITENCNT203 , " _
                   & "       0 as KAITENCNTCHO203 , " _
                   & "       0 as KAITENCNTTTL203 , " _
                   & "       0 as MODELDISTANCE0203 , " _
                   & "       0 as MODELDISTANCECHO0203 , " _
                   & "       0 as MODELDISTANCETTL0203 , " _
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
                   & "       0 as KAITENCNT204 , " _
                   & "       0 as KAITENCNTCHO204 , " _
                   & "       0 as KAITENCNTTTL204 , " _
                   & "       0 as MODELDISTANCE204 , " _
                   & "       0 as MODELDISTANCECHO204 , " _
                   & "       0 as MODELDISTANCETTL204 , " _
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
                   & "       0 as KAITENCNT205 , " _
                   & "       0 as KAITENCNTCHO205 , " _
                   & "       0 as KAITENCNTTTL205 , " _
                   & "       0 as MODELDISTANCE0205 , " _
                   & "       0 as MODELDISTANCECHO0205 , " _
                   & "       0 as MODELDISTANCETTL0205 , " _
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
                   & "       0 as KAITENCNT206 , " _
                   & "       0 as KAITENCNTCHO206 , " _
                   & "       0 as KAITENCNTTTL206 , " _
                   & "       0 as MODELDISTANCE0206 , " _
                   & "       0 as MODELDISTANCECHO0206 , " _
                   & "       0 as MODELDISTANCETTL0206 , " _
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
                   & "       0 as KAITENCNT207 , " _
                   & "       0 as KAITENCNTCHO207 , " _
                   & "       0 as KAITENCNTTTL207 , " _
                   & "       0 as MODELDISTANCE0207 , " _
                   & "       0 as MODELDISTANCECHO0207 , " _
                   & "       0 as MODELDISTANCETTL0207 , " _
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
                   & "       0 as KAITENCNT208 , " _
                   & "       0 as KAITENCNTCHO208 , " _
                   & "       0 as KAITENCNTTTL208 , " _
                   & "       0 as MODELDISTANCE0208 , " _
                   & "       0 as MODELDISTANCECHO0208 , " _
                   & "       0 as MODELDISTANCETTL0208 , " _
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
                   & "       0 as KAITENCNT209 , " _
                   & "       0 as KAITENCNTCHO209 , " _
                   & "       0 as KAITENCNTTTL209 , " _
                   & "       0 as MODELDISTANCE209 , " _
                   & "       0 as MODELDISTANCECHO209 , " _
                   & "       0 as MODELDISTANCETTL209 , " _
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
                   & "       0 as KAITENCNT210 , " _
                   & "       0 as KAITENCNTCHO210 , " _
                   & "       0 as KAITENCNTTTL210 , " _
                   & "       0 as MODELDISTANCE0210 , " _
                   & "       0 as MODELDISTANCECHO0210 , " _
                   & "       0 as MODELDISTANCETTL0210 , " _
                   & "       'K' as DATAKBN , " _
                   & "       isnull(A.HAISOTIME, 0) as HAISOTIME , " _
                   & "       isnull(A.NENMATUNISSU, 0) as NENMATUNISSU , " _
                   & "       isnull(A.NENMATUNISSUCHO, 0) as NENMATUNISSUCHO , " _
                   & "       isnull(A.NENMATUNISSU, 0) + isnull(A.NENMATUNISSUCHO, 0) as NENMATUNISSUTTL , " _
                   & "       isnull(A.SHACHUHAKKBN, 0) as SHACHUHAKKBN , " _
                   & "       '' as SHACHUHAKKBNNAMES , " _
                   & "       isnull(A.SHACHUHAKNISSU, 0) as SHACHUHAKNISSU , " _
                   & "       isnull(A.SHACHUHAKNISSUCHO, 0) as SHACHUHAKNISSUCHO , " _
                   & "       isnull(A.SHACHUHAKNISSU, 0) + isnull(A.SHACHUHAKNISSUCHO, 0) as SHACHUHAKNISSUTTL , " _
                   & "       isnull(A.MODELDISTANCE, 0) as MODELDISTANCE , " _
                   & "       isnull(A.MODELDISTANCECHO, 0) as MODELDISTANCECHO , " _
                   & "       isnull(A.MODELDISTANCE, 0) + isnull(A.MODELDISTANCECHO, 0) as MODELDISTANCETTL , " _
                   & "       isnull(A.JIKYUSHATIME, 0) as JIKYUSHATIME , " _
                   & "       isnull(A.JIKYUSHATIMECHO, 0) as JIKYUSHATIMECHO , " _
                   & "       isnull(A.JIKYUSHATIME, 0) + isnull(A.JIKYUSHATIMECHO, 0) as JIKYUSHATIMETTL , " _
                   & "       isnull(A.SDAIWORKTIME,0) as SDAIWORKTIME , " _
                   & "       isnull(A.SDAIWORKTIMECHO,0) as SDAIWORKTIMECHO , " _
                   & "       isnull(A.SDAIWORKTIME,0) + isnull(A.SDAIWORKTIMECHO,0) as SDAIWORKTIMETTL , " _
                   & "       isnull(A.SDAINIGHTTIME,0) as SDAINIGHTTIME , " _
                   & "       isnull(A.SDAINIGHTTIMECHO,0) as SDAINIGHTTIMECHO , " _
                   & "       isnull(A.SDAINIGHTTIME,0) + isnull(A.SDAINIGHTTIMECHO,0) as SDAINIGHTTIMETTL , " _
                   & "       isnull(A.HDAIWORKTIME,0) as HDAIWORKTIME , " _
                   & "       isnull(A.HDAIWORKTIMECHO,0) as HDAIWORKTIMECHO , " _
                   & "       isnull(A.HDAIWORKTIME,0) + isnull(A.HDAIWORKTIMECHO,0) as HDAIWORKTIMETTL , " _
                   & "       isnull(A.HDAINIGHTTIME,0) as HDAINIGHTTIME , " _
                   & "       isnull(A.HDAINIGHTTIMECHO,0) as HDAINIGHTTIMECHO , " _
                   & "       isnull(A.HDAINIGHTTIME,0) + isnull(A.HDAINIGHTTIMECHO,0) as HDAINIGHTTIMETTL , " _
                   & "       isnull(A.WWORKTIME,0) as WWORKTIME , " _
                   & "       isnull(A.WWORKTIMECHO,0) as WWORKTIMECHO , " _
                   & "       isnull(A.WWORKTIME,0) + isnull(A.WWORKTIMECHO,0) as WWORKTIMETTL , " _
                   & "       isnull(A.JYOMUTIME,0) as JYOMUTIME , " _
                   & "       isnull(A.JYOMUTIMECHO,0) as JYOMUTIMECHO , " _
                   & "       isnull(A.JYOMUTIME,0) + isnull(A.JYOMUTIMECHO,0) as JYOMUTIMETTL , " _
                   & "       isnull(A.HWORKNISSU,0) as HWORKNISSU , " _
                   & "       isnull(A.HWORKNISSUCHO,0) as HWORKNISSUCHO , " _
                   & "       isnull(A.HWORKNISSU,0) + isnull(A.HWORKNISSUCHO,0) as HWORKNISSUTTL , " _
                   & "       isnull(A.KAITENCNT,0) as KAITENCNT , " _
                   & "       isnull(A.KAITENCNTCHO,0) as KAITENCNTCHO , " _
                   & "       isnull(A.KAITENCNT,0) + isnull(A.KAITENCNTCHO,0) as KAITENCNTTTL , " _
                   & "       isnull(A.SENJYOCNT,0) as SENJYOCNT , " _
                   & "       isnull(A.SENJYOCNTCHO,0) as SENJYOCNTCHO , " _
                   & "       isnull(A.SENJYOCNT, 0) + isnull(A.SENJYOCNTCHO, 0) as SENJYOCNTTTL , " _
                   & "       isnull(A.UNLOADADDCNT1,0) as UNLOADADDCNT1 , " _
                   & "       isnull(A.UNLOADADDCNT1CHO,0) as UNLOADADDCNT1CHO , " _
                   & "       isnull(A.UNLOADADDCNT1, 0) + isnull(A.UNLOADADDCNT1CHO, 0) as UNLOADADDCNT1TTL , " _
                   & "       isnull(A.UNLOADADDCNT2,0) as UNLOADADDCNT2 , " _
                   & "       isnull(A.UNLOADADDCNT2CHO,0) as UNLOADADDCNT2CHO , " _
                   & "       isnull(A.UNLOADADDCNT2, 0) + isnull(A.UNLOADADDCNT2CHO, 0) as UNLOADADDCNT2TTL , " _
                   & "       isnull(A.UNLOADADDCNT3,0) as UNLOADADDCNT3 , " _
                   & "       isnull(A.UNLOADADDCNT3CHO,0) as UNLOADADDCNT3CHO , " _
                   & "       isnull(A.UNLOADADDCNT3, 0) + isnull(A.UNLOADADDCNT3CHO, 0) as UNLOADADDCNT3TTL , " _
                   & "       isnull(A.UNLOADADDCNT4,0) as UNLOADADDCNT4 , " _
                   & "       isnull(A.UNLOADADDCNT4CHO,0) as UNLOADADDCNT4CHO , " _
                   & "       isnull(A.UNLOADADDCNT4, 0) + isnull(A.UNLOADADDCNT4CHO, 0) as UNLOADADDCNT4TTL , " _
                   & "       isnull(A.LOADINGCNT1,0) as LOADINGCNT1 , " _
                   & "       isnull(A.LOADINGCNT1CHO,0) as LOADINGCNT1CHO , " _
                   & "       isnull(A.LOADINGCNT1, 0) + isnull(A.LOADINGCNT1CHO, 0) as LOADINGCNT1TTL , " _
                   & "       isnull(A.LOADINGCNT2,0) as LOADINGCNT2 , " _
                   & "       isnull(A.LOADINGCNT2CHO,0) as LOADINGCNT2CHO , " _
                   & "       isnull(A.LOADINGCNT2, 0) + isnull(A.LOADINGCNT2CHO, 0) as LOADINGCNT2TTL , " _
                   & "       isnull(A.SHORTDISTANCE1,0) as SHORTDISTANCE1 , " _
                   & "       isnull(A.SHORTDISTANCE1CHO,0) as SHORTDISTANCE1CHO , " _
                   & "       isnull(A.SHORTDISTANCE1, 0) + isnull(A.SHORTDISTANCE1CHO, 0) as SHORTDISTANCE1TTL , " _
                   & "       isnull(A.SHORTDISTANCE2,0) as SHORTDISTANCE2 , " _
                   & "       isnull(A.SHORTDISTANCE2CHO,0) as SHORTDISTANCE2CHO , " _
                   & "       isnull(A.SHORTDISTANCE2, 0) + isnull(A.SHORTDISTANCE2CHO, 0) as SHORTDISTANCE2TTL , " _
                   & "       isnull(MB3.SEQ, 0) as ORGSEQ " _
                   & " FROM       #MBtemp           MB " _
                   & " INNER JOIN T0007_KINTAI      A " _
                   & "   ON    A.CAMPCODE     = @CAMPCODE " _
                   & "   and   A.TAISHOYM     = @TAISHOYM " _
                   & "   and   A.STAFFCODE    = MB.STAFFCODE " _
                   & "   and   A.RECODEKBN    = '2' " _
                   & "   and   A.DELFLG      <> '1' " _
                   & " LEFT JOIN MB001_STAFF        MB2 " _
                   & "   ON    MB2.CAMPCODE     = @CAMPCODE " _
                   & "   and   MB2.STAFFCODE    = MB.STAFFCODE " _
                   & "   and   MB2.STYMD       <= @SEL_ENDYMD " _
                   & "   and   MB2.ENDYMD      >= @SEL_STYMD " _
                   & "   and   MB2.STYMD        = (SELECT MAX(STYMD) FROM MB001_STAFF WHERE CAMPCODE = @CAMPCODE and STAFFCODE = MB.STAFFCODE and STYMD <= @SEL_ENDYMD and ENDYMD >= @SEL_STYMD and DELFLG <> '1' ) " _
                   & "   and   MB2.DELFLG      <> '1' " _
                   & " LEFT JOIN MB002_STAFFORG MB3 " _
                   & "   ON    MB3.CAMPCODE     = @CAMPCODE " _
                   & "   and   MB3.STAFFCODE    = MB2.STAFFCODE " _
                   & "   and   MB3.SORG         = MB2.HORG " _
                   & "   and   MB3.DELFLG      <> '1' " _
                   & " LEFT JOIN MB005_CALENDAR CAL " _
                   & "   ON    CAL.CAMPCODE    = @CAMPCODE " _
                   & "   and   CAL.WORKINGYMD  = A.WORKDATE " _
                   & "   and   CAL.DELFLG     <> '1' " _
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

                Dim WW_SORT As String = "ORDER BY ORGSEQ, STAFFCODE, WORKDATE, HDKBN DESC, RECODEKBN, STDATE, STTIME, ENDDATE, ENDTIME"

                SQLStr = SQLStr & WW_SORT

                Using SQLcmd0 As New SqlCommand(SQLStr0, SQLcon), SQLcmd1 As New SqlCommand(SQLStr1, SQLcon), SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim P2_CAMPCODE As SqlParameter = SQLcmd1.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar, 20)
                    Dim P2_SEL_STYMD As SqlParameter = SQLcmd1.Parameters.Add("@SEL_STYMD", System.Data.SqlDbType.Date)
                    Dim P2_SEL_ENDYMD As SqlParameter = SQLcmd1.Parameters.Add("@SEL_ENDYMD", System.Data.SqlDbType.Date)
                    Dim P2_HORG As SqlParameter = SQLcmd1.Parameters.Add("@HORG", System.Data.SqlDbType.NVarChar, 20)
                    Dim P2_TERMID As SqlParameter = SQLcmd1.Parameters.Add("@TERMID", System.Data.SqlDbType.NVarChar, 20)
                    Dim P2_NOW As SqlParameter = SQLcmd1.Parameters.Add("@NOW", System.Data.SqlDbType.Date)

                    Dim P_CAMPCODE As SqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar, 20)
                    Dim P_TAISHOYM As SqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", System.Data.SqlDbType.NVarChar, 7)
                    Dim P_STYMD As SqlParameter = SQLcmd.Parameters.Add("@STYMD", System.Data.SqlDbType.Date)
                    Dim P_ENDYMD As SqlParameter = SQLcmd.Parameters.Add("@ENDYMD", System.Data.SqlDbType.Date)
                    Dim P_SEL_STYMD As SqlParameter = SQLcmd.Parameters.Add("@SEL_STYMD", System.Data.SqlDbType.Date)
                    Dim P_SEL_ENDYMD As SqlParameter = SQLcmd.Parameters.Add("@SEL_ENDYMD", System.Data.SqlDbType.Date)

                    'テンポラリテーブル作成
                    SQLcmd0.CommandTimeout = 300
                    SQLcmd0.ExecuteNonQuery()

                    '〇テンポラリに登録するデータの取得
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

                    '一覧データの取得
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
                        TA0003ALL.Load(SQLdr)
                    End Using
                    Dim WW_LINEcnt As Integer = 0
                    For Each TA0003ALLrow As DataRow In TA0003ALL.Rows

                        If TA0003ALLrow("HDKBN") = "H" Then
                            TA0003ALLrow("SELECT") = "1"
                            TA0003ALLrow("HIDDEN") = "0"      '表示
                            WW_LINEcnt += 1
                            TA0003ALLrow("LINECNT") = WW_LINEcnt
                        Else
                            TA0003ALLrow("SELECT") = "0"
                            TA0003ALLrow("HIDDEN") = "1"      '非表示
                            TA0003ALLrow("LINECNT") = 0
                        End If

                        TA0003ALLrow("SEQ") = CInt(TA0003ALLrow("SEQ")).ToString("000")
                        If IsDate(TA0003ALLrow("WORKDATE")) Then
                            TA0003ALLrow("WORKDATE") = CDate(TA0003ALLrow("WORKDATE")).ToString("yyyy/MM/dd")
                        Else
                            TA0003ALLrow("WORKDATE") = ""
                        End If
                        If IsDate(TA0003ALLrow("STDATE")) Then
                            TA0003ALLrow("STDATE") = CDate(TA0003ALLrow("STDATE")).ToString("yyyy/MM/dd")
                        Else
                            TA0003ALLrow("STDATE") = ""
                        End If
                        If IsDate(TA0003ALLrow("STTIME")) Then
                            TA0003ALLrow("STTIME") = CDate(TA0003ALLrow("STTIME")).ToString("HH:mm")
                        Else
                            TA0003ALLrow("STTIME") = ""
                        End If
                        If IsDate(TA0003ALLrow("ENDDATE")) Then
                            TA0003ALLrow("ENDDATE") = CDate(TA0003ALLrow("ENDDATE")).ToString("yyyy/MM/dd")
                        Else
                            TA0003ALLrow("ENDDATE") = ""
                        End If
                        If IsDate(TA0003ALLrow("ENDTIME")) Then
                            TA0003ALLrow("ENDTIME") = CDate(TA0003ALLrow("ENDTIME")).ToString("HH:mm")
                        Else
                            TA0003ALLrow("ENDTIME") = ""
                        End If
                        If IsDate(TA0003ALLrow("BINDSTDATE")) Then
                            TA0003ALLrow("BINDSTDATE") = CDate(TA0003ALLrow("BINDSTDATE")).ToString("HH:mm")
                        Else
                            TA0003ALLrow("BINDSTDATE") = ""
                        End If

                        TA0003ALLrow("WORKTIME") = T0009TIME.MinutestoHHMM(TA0003ALLrow("WORKTIME"))
                        TA0003ALLrow("MOVETIME") = T0009TIME.MinutestoHHMM(TA0003ALLrow("MOVETIME"))
                        TA0003ALLrow("ACTTIME") = T0009TIME.MinutestoHHMM(TA0003ALLrow("ACTTIME"))
                        TA0003ALLrow("BINDTIME") = T0009TIME.MinutestoHHMM(TA0003ALLrow("BINDTIME"))
                        TA0003ALLrow("BREAKTIME") = T0009TIME.MinutestoHHMM(TA0003ALLrow("BREAKTIME"))
                        TA0003ALLrow("BREAKTIMECHO") = T0009TIME.MinutestoHHMM(TA0003ALLrow("BREAKTIMECHO"))
                        TA0003ALLrow("BREAKTIMETTL") = T0009TIME.MinutestoHHMM(TA0003ALLrow("BREAKTIMETTL"))
                        TA0003ALLrow("NIGHTTIME") = T0009TIME.MinutestoHHMM(TA0003ALLrow("NIGHTTIME"))
                        TA0003ALLrow("NIGHTTIMECHO") = T0009TIME.MinutestoHHMM(TA0003ALLrow("NIGHTTIMECHO"))
                        TA0003ALLrow("NIGHTTIMETTL") = T0009TIME.MinutestoHHMM(TA0003ALLrow("NIGHTTIMETTL"))
                        TA0003ALLrow("ORVERTIME") = T0009TIME.MinutestoHHMM(TA0003ALLrow("ORVERTIME"))
                        TA0003ALLrow("ORVERTIMECHO") = T0009TIME.MinutestoHHMM(TA0003ALLrow("ORVERTIMECHO"))
                        TA0003ALLrow("ORVERTIMETTL") = T0009TIME.MinutestoHHMM(TA0003ALLrow("ORVERTIMETTL"))
                        TA0003ALLrow("WNIGHTTIME") = T0009TIME.MinutestoHHMM(TA0003ALLrow("WNIGHTTIME"))
                        TA0003ALLrow("WNIGHTTIMECHO") = T0009TIME.MinutestoHHMM(TA0003ALLrow("WNIGHTTIMECHO"))
                        TA0003ALLrow("WNIGHTTIMETTL") = T0009TIME.MinutestoHHMM(Val(TA0003ALLrow("WNIGHTTIMETTL")) + Val(TA0003ALLrow("HNIGHTTIMETTL")))
                        TA0003ALLrow("SWORKTIME") = T0009TIME.MinutestoHHMM(TA0003ALLrow("SWORKTIME"))
                        TA0003ALLrow("SWORKTIMECHO") = T0009TIME.MinutestoHHMM(TA0003ALLrow("SWORKTIMECHO"))
                        TA0003ALLrow("SWORKTIMETTL") = T0009TIME.MinutestoHHMM(TA0003ALLrow("SWORKTIMETTL"))
                        TA0003ALLrow("SNIGHTTIME") = T0009TIME.MinutestoHHMM(TA0003ALLrow("SNIGHTTIME"))
                        TA0003ALLrow("SNIGHTTIMECHO") = T0009TIME.MinutestoHHMM(TA0003ALLrow("SNIGHTTIMECHO"))
                        TA0003ALLrow("SNIGHTTIMETTL") = T0009TIME.MinutestoHHMM(TA0003ALLrow("SNIGHTTIMETTL"))
                        TA0003ALLrow("HWORKTIME") = T0009TIME.MinutestoHHMM(TA0003ALLrow("HWORKTIME"))
                        TA0003ALLrow("HWORKTIMECHO") = T0009TIME.MinutestoHHMM(TA0003ALLrow("HWORKTIMECHO"))
                        TA0003ALLrow("HWORKTIMETTL") = T0009TIME.MinutestoHHMM(TA0003ALLrow("HWORKTIMETTL"))
                        TA0003ALLrow("HNIGHTTIME") = T0009TIME.MinutestoHHMM(TA0003ALLrow("HNIGHTTIME"))
                        TA0003ALLrow("HNIGHTTIMECHO") = T0009TIME.MinutestoHHMM(TA0003ALLrow("HNIGHTTIMECHO"))
                        TA0003ALLrow("HNIGHTTIMETTL") = T0009TIME.MinutestoHHMM(0)
                        TA0003ALLrow("HOANTIME") = T0009TIME.MinutestoHHMM(TA0003ALLrow("HOANTIME"))
                        TA0003ALLrow("HOANTIMECHO") = T0009TIME.MinutestoHHMM(TA0003ALLrow("HOANTIMECHO"))
                        TA0003ALLrow("HOANTIMETTL") = T0009TIME.MinutestoHHMM(TA0003ALLrow("HOANTIMETTL"))
                        TA0003ALLrow("KOATUTIME") = T0009TIME.MinutestoHHMM(TA0003ALLrow("KOATUTIME"))
                        TA0003ALLrow("KOATUTIMECHO") = T0009TIME.MinutestoHHMM(TA0003ALLrow("KOATUTIMECHO"))
                        TA0003ALLrow("KOATUTIMETTL") = T0009TIME.MinutestoHHMM(TA0003ALLrow("KOATUTIMETTL"))
                        TA0003ALLrow("TOKUSA1TIME") = T0009TIME.MinutestoHHMM(TA0003ALLrow("TOKUSA1TIME"))
                        TA0003ALLrow("TOKUSA1TIMECHO") = T0009TIME.MinutestoHHMM(TA0003ALLrow("TOKUSA1TIMECHO"))
                        TA0003ALLrow("TOKUSA1TIMETTL") = T0009TIME.MinutestoHHMM(TA0003ALLrow("TOKUSA1TIMETTL"))
                        TA0003ALLrow("HAYADETIME") = T0009TIME.MinutestoHHMM(TA0003ALLrow("HAYADETIME"))
                        TA0003ALLrow("HAYADETIMECHO") = T0009TIME.MinutestoHHMM(TA0003ALLrow("HAYADETIMECHO"))
                        TA0003ALLrow("HAYADETIMETTL") = T0009TIME.MinutestoHHMM(TA0003ALLrow("HAYADETIMETTL"))

                        TA0003ALLrow("HAIDISTANCE") = Val(TA0003ALLrow("HAIDISTANCE"))
                        TA0003ALLrow("HAIDISTANCECHO") = Val(TA0003ALLrow("HAIDISTANCECHO"))
                        TA0003ALLrow("HAIDISTANCETTL") = Val(TA0003ALLrow("HAIDISTANCETTL"))
                        TA0003ALLrow("KAIDISTANCE") = Val(TA0003ALLrow("KAIDISTANCE"))
                        TA0003ALLrow("KAIDISTANCECHO") = Val(TA0003ALLrow("KAIDISTANCECHO"))
                        TA0003ALLrow("KAIDISTANCETTL") = Val(TA0003ALLrow("KAIDISTANCETTL"))

                        If TA0003ALLrow("SHACHUHAKKBN") = "1" Then
                            TA0003ALLrow("SHACHUHAKKBNNAMES") = "✔"
                        Else
                            TA0003ALLrow("SHACHUHAKKBNNAMES") = ""
                        End If
                        TA0003ALLrow("JIKYUSHATIME") = T0009TIME.MinutestoHHMM(TA0003ALLrow("JIKYUSHATIME"))
                        TA0003ALLrow("JIKYUSHATIMECHO") = T0009TIME.MinutestoHHMM(TA0003ALLrow("JIKYUSHATIMECHO"))
                        TA0003ALLrow("JIKYUSHATIMETTL") = T0009TIME.MinutestoHHMM(TA0003ALLrow("JIKYUSHATIMETTL"))

                        TA0003ALLrow("SDAIWORKTIME") = T0009TIME.MinutestoHHMM(TA0003ALLrow("SDAIWORKTIME"))
                        TA0003ALLrow("SDAIWORKTIMECHO") = T0009TIME.MinutestoHHMM(TA0003ALLrow("SDAIWORKTIMECHO"))
                        TA0003ALLrow("SDAIWORKTIMETTL") = T0009TIME.MinutestoHHMM(TA0003ALLrow("SDAIWORKTIMETTL"))
                        TA0003ALLrow("SDAINIGHTTIME") = T0009TIME.MinutestoHHMM(TA0003ALLrow("SDAINIGHTTIME"))
                        TA0003ALLrow("SDAINIGHTTIMECHO") = T0009TIME.MinutestoHHMM(TA0003ALLrow("SDAINIGHTTIMECHO"))
                        TA0003ALLrow("SDAINIGHTTIMETTL") = T0009TIME.MinutestoHHMM(TA0003ALLrow("SDAINIGHTTIMETTL"))
                        TA0003ALLrow("HDAIWORKTIME") = T0009TIME.MinutestoHHMM(TA0003ALLrow("HDAIWORKTIME"))
                        TA0003ALLrow("HDAIWORKTIMECHO") = T0009TIME.MinutestoHHMM(TA0003ALLrow("HDAIWORKTIMECHO"))
                        TA0003ALLrow("HDAIWORKTIMETTL") = T0009TIME.MinutestoHHMM(TA0003ALLrow("HDAIWORKTIMETTL"))
                        TA0003ALLrow("HDAINIGHTTIME") = T0009TIME.MinutestoHHMM(TA0003ALLrow("HDAINIGHTTIME"))
                        TA0003ALLrow("HDAINIGHTTIMECHO") = T0009TIME.MinutestoHHMM(TA0003ALLrow("HDAINIGHTTIMECHO"))
                        TA0003ALLrow("HDAINIGHTTIMETTL") = T0009TIME.MinutestoHHMM(TA0003ALLrow("HDAINIGHTTIMETTL"))
                        TA0003ALLrow("WWORKTIME") = T0009TIME.MinutestoHHMM(TA0003ALLrow("WWORKTIME"))
                        TA0003ALLrow("WWORKTIMECHO") = T0009TIME.MinutestoHHMM(TA0003ALLrow("WWORKTIMECHO"))
                        TA0003ALLrow("WWORKTIMETTL") = T0009TIME.MinutestoHHMM(TA0003ALLrow("WWORKTIMETTL"))
                        TA0003ALLrow("JYOMUTIME") = T0009TIME.MinutestoHHMM(TA0003ALLrow("JYOMUTIME"))
                        TA0003ALLrow("JYOMUTIMECHO") = T0009TIME.MinutestoHHMM(TA0003ALLrow("JYOMUTIMECHO"))
                        TA0003ALLrow("JYOMUTIMETTL") = T0009TIME.MinutestoHHMM(TA0003ALLrow("JYOMUTIMETTL"))

                        If TA0003ALLrow("HDKBN") = "H" Then
                            For Each WW_DTLrow As DataRow In TA0003ALL.Rows
                                '次のヘッダまで
                                If WW_DTLrow("HDKBN") = "H" Then Continue For

                                If IsDate(WW_DTLrow("WORKDATE")) Then
                                    WW_DTLrow("WORKDATE") = CDate(WW_DTLrow("WORKDATE")).ToString("yyyy/MM/dd")
                                Else
                                    WW_DTLrow("WORKDATE") = ""
                                End If

                                If TA0003ALLrow("WORKDATE") = WW_DTLrow("WORKDATE") AndAlso
                                   TA0003ALLrow("STAFFCODE") = WW_DTLrow("STAFFCODE") Then

                                    Dim WW_SHARYOKBN As String = "SHARYOKBN" & WW_DTLrow("SHARYOKBN")
                                    Dim WW_SHARYOKBNNAMES As String = "SHARYOKBNNAMES" & WW_DTLrow("SHARYOKBN")
                                    Dim WW_OILPAYKBN As String = "OILPAYKBN" & WW_DTLrow("SHARYOKBN") & WW_DTLrow("OILPAYKBN")
                                    Dim WW_OILPAYKBNNAMES As String = "OILPAYKBNNAMES" & WW_DTLrow("SHARYOKBN") & WW_DTLrow("OILPAYKBN")
                                    Dim WW_HAIDISTANCE As String = "HAIDISTANCE" & WW_DTLrow("SHARYOKBN") & WW_DTLrow("OILPAYKBN")
                                    Dim WW_HAIDISTANCECHO As String = "HAIDISTANCECHO" & WW_DTLrow("SHARYOKBN") & WW_DTLrow("OILPAYKBN")
                                    Dim WW_HAIDISTANCETTL As String = "HAIDISTANCETTL" & WW_DTLrow("SHARYOKBN") & WW_DTLrow("OILPAYKBN")
                                    Dim WW_KAIDISTANCE As String = "KAIDISTANCE" & WW_DTLrow("SHARYOKBN") & WW_DTLrow("OILPAYKBN")
                                    Dim WW_KAIDISTANCECHO As String = "KAIDISTANCECHO" & WW_DTLrow("SHARYOKBN") & WW_DTLrow("OILPAYKBN")
                                    Dim WW_KAIDISTANCETTL As String = "KAIDISTANCETTL" & WW_DTLrow("SHARYOKBN") & WW_DTLrow("OILPAYKBN")
                                    Dim WW_UNLOADCNT As String = "UNLOADCNT" & WW_DTLrow("SHARYOKBN") & WW_DTLrow("OILPAYKBN")
                                    Dim WW_UNLOADCNTCHO As String = "UNLOADCNTCHO" & WW_DTLrow("SHARYOKBN") & WW_DTLrow("OILPAYKBN")
                                    Dim WW_UNLOADCNTTTL As String = "UNLOADCNTTTL" & WW_DTLrow("SHARYOKBN") & WW_DTLrow("OILPAYKBN")
                                    Dim WW_KAITENCNT As String = "KAITENCNT" & WW_DTLrow("SHARYOKBN") & WW_DTLrow("OILPAYKBN")
                                    Dim WW_KAITENCNTCHO As String = "KAITENCNTCHO" & WW_DTLrow("SHARYOKBN") & WW_DTLrow("OILPAYKBN")
                                    Dim WW_KAITENCNTTTL As String = "KAITENCNTTTL" & WW_DTLrow("SHARYOKBN") & WW_DTLrow("OILPAYKBN")
                                    Dim WW_MODELDISTANCE As String = "MODELDISTANCE" & WW_DTLrow("SHARYOKBN") & WW_DTLrow("OILPAYKBN")
                                    Dim WW_MODELDISTANCECHO As String = "MODELDISTANCECHO" & WW_DTLrow("SHARYOKBN") & WW_DTLrow("OILPAYKBN")
                                    Dim WW_MODELDISTANCETTL As String = "MODELDISTANCETTL" & WW_DTLrow("SHARYOKBN") & WW_DTLrow("OILPAYKBN")

                                    TA0003ALLrow(WW_SHARYOKBN) = WW_DTLrow("SHARYOKBN")
                                    TA0003ALLrow(WW_SHARYOKBNNAMES) = WW_DTLrow("SHARYOKBNNAMES")
                                    TA0003ALLrow(WW_OILPAYKBN) = WW_DTLrow("OILPAYKBN")
                                    TA0003ALLrow(WW_OILPAYKBNNAMES) = WW_DTLrow("OILPAYKBNNAMES")
                                    TA0003ALLrow(WW_HAIDISTANCE) = WW_DTLrow("HAIDISTANCE")
                                    TA0003ALLrow(WW_HAIDISTANCECHO) = WW_DTLrow("HAIDISTANCECHO")
                                    TA0003ALLrow(WW_HAIDISTANCETTL) = WW_DTLrow("HAIDISTANCETTL")
                                    TA0003ALLrow(WW_KAIDISTANCE) = WW_DTLrow("KAIDISTANCE")
                                    TA0003ALLrow(WW_KAIDISTANCECHO) = WW_DTLrow("KAIDISTANCECHO")
                                    TA0003ALLrow(WW_KAIDISTANCETTL) = WW_DTLrow("KAIDISTANCETTL")
                                    TA0003ALLrow(WW_UNLOADCNT) = WW_DTLrow("UNLOADCNT")
                                    TA0003ALLrow(WW_UNLOADCNTCHO) = WW_DTLrow("UNLOADCNTCHO")
                                    TA0003ALLrow(WW_UNLOADCNTTTL) = WW_DTLrow("UNLOADCNTTTL")
                                    TA0003ALLrow(WW_KAITENCNT) = WW_DTLrow("KAITENCNT")
                                    TA0003ALLrow(WW_KAITENCNTCHO) = WW_DTLrow("KAITENCNTCHO")
                                    TA0003ALLrow(WW_KAITENCNTTTL) = WW_DTLrow("KAITENCNTTTL")
                                    TA0003ALLrow(WW_MODELDISTANCE) = WW_DTLrow("MODELDISTANCE")
                                    TA0003ALLrow(WW_MODELDISTANCECHO) = WW_DTLrow("MODELDISTANCECHO")
                                    TA0003ALLrow(WW_MODELDISTANCETTL) = WW_DTLrow("MODELDISTANCETTL")
                                End If
                            Next

                        End If

                        '名前の取得
                        TA0003ALLrow("CAMPNAMES") = ""
                        CodeToName("CAMPCODE", TA0003ALLrow("CAMPCODE"), TA0003ALLrow("CAMPNAMES"), WW_DUMMY)
                        If TA0003ALLrow("STAFFNAMES") = "" Then
                            TA0003ALLrow("STAFFNAMES") = ""
                            CodeToName("STAFFCODE", TA0003ALLrow("STAFFCODE"), TA0003ALLrow("STAFFNAMES"), WW_DUMMY)
                        End If
                        TA0003ALLrow("MORGNAMES") = ""
                        CodeToName("ORG", TA0003ALLrow("MORG"), TA0003ALLrow("MORGNAMES"), WW_DUMMY)

                        If TA0003ALLrow("HORG") = "" Then
                            TA0003ALLrow("HORG") = work.WF_SEL_HORG.Text
                            TA0003ALLrow("HORGNAMES") = ""
                            CodeToName("ORG", TA0003ALLrow("HORG"), TA0003ALLrow("HORGNAMES"), WW_DUMMY)
                        Else
                            TA0003ALLrow("HORGNAMES") = ""
                            CodeToName("ORG", TA0003ALLrow("HORG"), TA0003ALLrow("HORGNAMES"), WW_DUMMY)
                        End If

                        If TA0003ALLrow("SORG") = "" Then
                            TA0003ALLrow("SORG") = TA0003ALLrow("HORG")
                        End If
                        TA0003ALLrow("SORGNAMES") = ""
                        CodeToName("ORG", TA0003ALLrow("SORG"), TA0003ALLrow("SORGNAMES"), WW_DUMMY)


                        '○表示項目編集
                        If TA0003ALLrow("CAMPNAMES") = Nothing AndAlso TA0003ALLrow("CAMPCODE") = Nothing Then
                            TA0003ALLrow("CAMPCODE_TXT") = ""
                        Else
                            TA0003ALLrow("CAMPCODE_TXT") = TA0003ALLrow("CAMPNAMES") & " (" & TA0003ALLrow("CAMPCODE") & ")"
                        End If

                        TA0003ALLrow("TAISHOYM_TXT") = TA0003ALLrow("TAISHOYM")

                        If TA0003ALLrow("STAFFNAMES") = Nothing AndAlso TA0003ALLrow("STAFFCODE") = Nothing Then
                            TA0003ALLrow("STAFFCODE_TXT") = ""
                        Else
                            TA0003ALLrow("STAFFCODE_TXT") = TA0003ALLrow("STAFFNAMES") & " (" & TA0003ALLrow("STAFFCODE") & ")"
                        End If

                        If IsDate(TA0003ALLrow("WORKDATE")) Then
                            TA0003ALLrow("WORKDATE_TXT") = CDate(TA0003ALLrow("WORKDATE")).ToString("dd")
                        Else
                            TA0003ALLrow("WORKDATE_TXT") = ""
                        End If

                        If TA0003ALLrow("WORKINGWEEKNAMES") = Nothing Then
                            TA0003ALLrow("WORKINGWEEK_TXT") = ""
                        Else
                            TA0003ALLrow("WORKINGWEEK_TXT") = TA0003ALLrow("WORKINGWEEKNAMES")
                        End If

                        TA0003ALLrow("HDKBN_TXT") = TA0003ALLrow("HDKBN")

                        If TA0003ALLrow("RECODEKBNNAMES") = Nothing AndAlso TA0003ALLrow("RECODEKBN") = Nothing Then
                            TA0003ALLrow("RECODEKBN_TXT") = ""
                        Else
                            TA0003ALLrow("RECODEKBN_TXT") = TA0003ALLrow("RECODEKBNNAMES") & " (" & TA0003ALLrow("RECODEKBN") & ")"
                        End If

                        If TA0003ALLrow("WORKKBNNAMES") = Nothing AndAlso TA0003ALLrow("WORKKBN") = Nothing Then
                            TA0003ALLrow("WORKKBN_TXT") = ""
                        Else
                            TA0003ALLrow("WORKKBN_TXT") = TA0003ALLrow("WORKKBNNAMES") & " (" & TA0003ALLrow("WORKKBN") & ")"
                        End If

                        If TA0003ALLrow("SHARYOKBNNAMES") = Nothing AndAlso TA0003ALLrow("SHARYOKBN") = Nothing Then
                            TA0003ALLrow("SHARYOKBN_TXT") = ""
                        Else
                            TA0003ALLrow("SHARYOKBN_TXT") = TA0003ALLrow("SHARYOKBNNAMES") & " (" & TA0003ALLrow("SHARYOKBN") & ")"
                        End If

                        If TA0003ALLrow("OILPAYKBNNAMES") = Nothing AndAlso TA0003ALLrow("OILPAYKBN") = Nothing Then
                            TA0003ALLrow("OILPAYKBN_TXT") = ""
                        Else
                            TA0003ALLrow("OILPAYKBN_TXT") = TA0003ALLrow("OILPAYKBNNAMES") & " (" & TA0003ALLrow("OILPAYKBN") & ")"
                        End If

                        If TA0003ALLrow("STAFFKBNNAMES") = Nothing AndAlso TA0003ALLrow("STAFFKBN") = Nothing Then
                            TA0003ALLrow("STAFFKBN_TXT") = ""
                        Else
                            TA0003ALLrow("STAFFKBN_TXT") = TA0003ALLrow("STAFFKBNNAMES") & " (" & TA0003ALLrow("STAFFKBN") & ")"
                        End If

                        If TA0003ALLrow("MORGNAMES") = Nothing AndAlso TA0003ALLrow("MORG") = Nothing Then
                            TA0003ALLrow("MORG_TXT") = ""
                        Else
                            TA0003ALLrow("MORG_TXT") = TA0003ALLrow("MORGNAMES") & " (" & TA0003ALLrow("MORG") & ")"
                        End If

                        If TA0003ALLrow("HORGNAMES") = Nothing AndAlso TA0003ALLrow("HORG") = Nothing Then
                            TA0003ALLrow("HORG_TXT") = ""
                        Else
                            TA0003ALLrow("HORG_TXT") = TA0003ALLrow("HORGNAMES") & " (" & TA0003ALLrow("HORG") & ")"
                        End If

                        If TA0003ALLrow("SORGNAMES") = Nothing AndAlso TA0003ALLrow("SORG") = Nothing Then
                            TA0003ALLrow("SORG_TXT") = ""
                        Else
                            TA0003ALLrow("SORG_TXT") = TA0003ALLrow("SORGNAMES") & " (" & TA0003ALLrow("SORG") & ")"
                        End If

                        If TA0003ALLrow("HOLIDAYKBNNAMES") = Nothing AndAlso TA0003ALLrow("HOLIDAYKBN") = Nothing Then
                            TA0003ALLrow("HOLIDAYKBN_TXT") = ""
                        Else
                            TA0003ALLrow("HOLIDAYKBN_TXT") = TA0003ALLrow("HOLIDAYKBNNAMES") & " (" & TA0003ALLrow("HOLIDAYKBN") & ")"
                        End If

                        If TA0003ALLrow("PAYKBNNAMES") = Nothing AndAlso TA0003ALLrow("PAYKBN") = Nothing Then
                            TA0003ALLrow("PAYKBN_TXT") = ""
                        Else
                            TA0003ALLrow("PAYKBN_TXT") = TA0003ALLrow("PAYKBNNAMES") & " (" & TA0003ALLrow("PAYKBN") & ")"
                        End If

                        If TA0003ALLrow("SHUKCHOKKBNNAMES") = Nothing AndAlso TA0003ALLrow("SHUKCHOKKBN") = Nothing Then
                            TA0003ALLrow("SHUKCHOKKBN_TXT") = ""
                        Else
                            TA0003ALLrow("SHUKCHOKKBN_TXT") = TA0003ALLrow("SHUKCHOKKBNNAMES") & " (" & TA0003ALLrow("SHUKCHOKKBN") & ")"
                        End If

                        TA0003ALLrow("DELFLG_TXT") = TA0003ALLrow("DELFLG")

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
        CS0026TblSort.COMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0026TblSort.PROFID = Master.PROF_VIEW
        CS0026TblSort.MAPID = Master.MAPID
        CS0026TblSort.VARI = Master.VIEWID
        CS0026TblSort.TAB = ""
        CS0026TblSort.getSorting()
        If Not isNormal(CS0026TblSort.ERR) Then
            Master.output(CS0026TblSort.ERR, C_MESSAGE_TYPE.ABORT)
            Exit Sub
        End If
        'ソート＆データ抽出
        CS0026TblSort.TABLE = TA0003ALL
        CS0026TblSort.FILTER = "SELECT = 1"
        TA0003ALL = CS0026TblSort.sort()
    End Sub

    ''' <summary>
    ''' TA0003VIEW-GridView用テーブル作成
    ''' </summary>
    ''' <param name="I_CODE">取得コード</param>
    Protected Sub GetViewToTA0003(ByVal I_CODE As String)

        '１０分切上処理
        CopyT7EditTbl(TA0003ALL, TA0003VIEWtbl)

        '〇 TA0003ALLよりデータ抽出
        CS0026TblSort.TABLE = TA0003VIEWtbl
        CS0026TblSort.FILTER = "HDKBN = '" & I_CODE & "'"
        CS0026TblSort.SORTING = "LINECNT, SEQ ASC"
        TA0003VIEWtbl = CS0026TblSort.sort()
        '○LineCNT付番・枝番再付番
        Dim WW_LINECNT As Integer = 0

        For Each TA0003VIEWrow As DataRow In TA0003VIEWtbl.Rows
            TA0003VIEWrow("LINECNT") = 0
        Next

        For Each TA0003VIEWrow As DataRow In TA0003VIEWtbl.Rows

            If TA0003VIEWrow("LINECNT") = 0 Then
                TA0003VIEWrow("SELECT") = "1"
                TA0003VIEWrow("HIDDEN") = "0"      '表示
                WW_LINECNT += 1
                TA0003VIEWrow("LINECNT") = WW_LINECNT
            End If
        Next

    End Sub

    ''' <summary>
    ''' 帳票用編集
    ''' </summary>
    ''' <param name="IO_TBL"></param>
    ''' <remarks></remarks>
    Protected Sub ListEdit(ByRef IO_TBL As DataTable)
        Dim WW_LINEcnt As Integer = 0

        Dim WW_TA0003tbl As DataTable = IO_TBL.Clone
        Dim WW_TA0003row As DataRow

        For i As Integer = 0 To IO_TBL.Rows.Count - 1
            WW_TA0003row = WW_TA0003tbl.NewRow
            WW_TA0003row.ItemArray = IO_TBL.Rows(i).ItemArray

            '--------------------------------------
            '勤務状況リスト編集 
            '--------------------------------------
            WW_TA0003row("TAISHOYM_TXT") = Mid(WW_TA0003row("TAISHOYM"), 1, 4) & "年" & Mid(WW_TA0003row("TAISHOYM"), 6, 2) & "月"

            If WW_TA0003row("SHUKCHOKKBN") = "0" Then
                WW_TA0003row("SHUKCHOKKBN_TXT") = ""
                WW_TA0003row("SHUKCHOKKBNNAMES") = ""
            End If

            If WW_TA0003row("HOLIDAYKBN") = "0" Then
                WW_TA0003row("HOLIDAYKBN_TXT") = ""
                WW_TA0003row("HOLIDAYKBNNAMES") = ""
            End If

            WW_TA0003row("WORKTIME") = T0009TIME.ZEROtoSpace(WW_TA0003row("WORKTIME"))
            WW_TA0003row("MOVETIME") = T0009TIME.ZEROtoSpace(WW_TA0003row("MOVETIME"))
            WW_TA0003row("ACTTIME") = T0009TIME.ZEROtoSpace(WW_TA0003row("ACTTIME"))

            WW_TA0003row("BINDTIME") = T0009TIME.ZEROtoSpace(WW_TA0003row("BINDTIME"))
            WW_TA0003row("BREAKTIME") = T0009TIME.ZEROtoSpace(WW_TA0003row("BREAKTIME"))
            WW_TA0003row("BREAKTIMECHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("BREAKTIMECHO"))
            WW_TA0003row("BREAKTIMETTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("BREAKTIMETTL"))
            WW_TA0003row("NIGHTTIME") = T0009TIME.ZEROtoSpace(WW_TA0003row("NIGHTTIME"))
            WW_TA0003row("NIGHTTIMECHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("NIGHTTIMECHO"))
            WW_TA0003row("NIGHTTIMETTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("NIGHTTIMETTL"))
            WW_TA0003row("ORVERTIME") = T0009TIME.ZEROtoSpace(WW_TA0003row("ORVERTIME"))
            WW_TA0003row("ORVERTIMECHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("ORVERTIMECHO"))
            WW_TA0003row("ORVERTIMETTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("ORVERTIMETTL"))
            WW_TA0003row("WNIGHTTIME") = T0009TIME.ZEROtoSpace(WW_TA0003row("WNIGHTTIME"))
            WW_TA0003row("WNIGHTTIMECHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("WNIGHTTIMECHO"))
            WW_TA0003row("WNIGHTTIMETTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("WNIGHTTIMETTL"))
            WW_TA0003row("SWORKTIME") = T0009TIME.ZEROtoSpace(WW_TA0003row("SWORKTIME"))
            WW_TA0003row("SWORKTIMECHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("SWORKTIMECHO"))
            WW_TA0003row("SWORKTIMETTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("SWORKTIMETTL"))
            WW_TA0003row("SNIGHTTIME") = T0009TIME.ZEROtoSpace(WW_TA0003row("SNIGHTTIME"))
            WW_TA0003row("SNIGHTTIMECHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("SNIGHTTIMECHO"))
            WW_TA0003row("SNIGHTTIMETTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("SNIGHTTIMETTL"))
            WW_TA0003row("HWORKTIME") = T0009TIME.ZEROtoSpace(WW_TA0003row("HWORKTIME"))
            WW_TA0003row("HWORKTIMECHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("HWORKTIMECHO"))
            WW_TA0003row("HWORKTIMETTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("HWORKTIMETTL"))
            WW_TA0003row("HNIGHTTIME") = T0009TIME.ZEROtoSpace(WW_TA0003row("HNIGHTTIME"))
            WW_TA0003row("HNIGHTTIMECHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("HNIGHTTIMECHO"))
            WW_TA0003row("HNIGHTTIMETTL") = ""

            WW_TA0003row("WORKNISSU") = T0009TIME.ZEROtoSpace(WW_TA0003row("WORKNISSU"))
            WW_TA0003row("WORKNISSUCHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("WORKNISSUCHO"))
            WW_TA0003row("WORKNISSUTTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("WORKNISSUTTL"))
            WW_TA0003row("SHOUKETUNISSU") = T0009TIME.ZEROtoSpace(WW_TA0003row("SHOUKETUNISSU"))
            WW_TA0003row("SHOUKETUNISSUCHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("SHOUKETUNISSUCHO"))
            WW_TA0003row("SHOUKETUNISSUTTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("SHOUKETUNISSUTTL"))
            WW_TA0003row("KUMIKETUNISSU") = T0009TIME.ZEROtoSpace(WW_TA0003row("KUMIKETUNISSU"))
            WW_TA0003row("KUMIKETUNISSUCHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("KUMIKETUNISSUCHO"))
            WW_TA0003row("KUMIKETUNISSUTTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("KUMIKETUNISSUTTL"))
            WW_TA0003row("ETCKETUNISSU") = T0009TIME.ZEROtoSpace(WW_TA0003row("ETCKETUNISSU"))
            WW_TA0003row("ETCKETUNISSUCHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("ETCKETUNISSUCHO"))
            WW_TA0003row("ETCKETUNISSUTTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("ETCKETUNISSUTTL"))
            WW_TA0003row("NENKYUNISSU") = T0009TIME.ZEROtoSpace(WW_TA0003row("NENKYUNISSU"))
            WW_TA0003row("NENKYUNISSUCHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("NENKYUNISSUCHO"))
            WW_TA0003row("NENKYUNISSUTTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("NENKYUNISSUTTL"))
            WW_TA0003row("TOKUKYUNISSU") = T0009TIME.ZEROtoSpace(WW_TA0003row("TOKUKYUNISSU"))
            WW_TA0003row("TOKUKYUNISSUCHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("TOKUKYUNISSUCHO"))
            WW_TA0003row("TOKUKYUNISSUTTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("TOKUKYUNISSUTTL"))
            WW_TA0003row("CHIKOKSOTAINISSU") = T0009TIME.ZEROtoSpace(WW_TA0003row("CHIKOKSOTAINISSU"))
            WW_TA0003row("CHIKOKSOTAINISSUCHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("CHIKOKSOTAINISSUCHO"))
            WW_TA0003row("CHIKOKSOTAINISSUTTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("CHIKOKSOTAINISSUTTL"))
            WW_TA0003row("STOCKNISSU") = T0009TIME.ZEROtoSpace(WW_TA0003row("STOCKNISSU"))
            WW_TA0003row("STOCKNISSUCHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("STOCKNISSUCHO"))
            WW_TA0003row("STOCKNISSUTTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("STOCKNISSUTTL"))
            WW_TA0003row("KYOTEIWEEKNISSU") = T0009TIME.ZEROtoSpace(WW_TA0003row("KYOTEIWEEKNISSU"))
            WW_TA0003row("KYOTEIWEEKNISSUCHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("KYOTEIWEEKNISSUCHO"))
            WW_TA0003row("KYOTEIWEEKNISSUTTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("KYOTEIWEEKNISSUTTL"))
            WW_TA0003row("WEEKNISSU") = T0009TIME.ZEROtoSpace(WW_TA0003row("WEEKNISSU"))
            WW_TA0003row("WEEKNISSUCHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("WEEKNISSUCHO"))
            WW_TA0003row("WEEKNISSUTTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("WEEKNISSUTTL"))
            WW_TA0003row("DAIKYUNISSU") = T0009TIME.ZEROtoSpace(WW_TA0003row("DAIKYUNISSU"))
            WW_TA0003row("DAIKYUNISSUCHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("DAIKYUNISSUCHO"))
            WW_TA0003row("DAIKYUNISSUTTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("DAIKYUNISSUTTL"))
            WW_TA0003row("NENSHINISSU") = T0009TIME.ZEROtoSpace(WW_TA0003row("NENSHINISSU"))
            WW_TA0003row("NENSHINISSUCHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("NENSHINISSUCHO"))
            WW_TA0003row("NENSHINISSUTTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("NENSHINISSUTTL"))
            WW_TA0003row("SHUKCHOKNNISSU") = T0009TIME.ZEROtoSpace(WW_TA0003row("SHUKCHOKNNISSU"))
            WW_TA0003row("SHUKCHOKNNISSUCHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("SHUKCHOKNNISSUCHO"))
            WW_TA0003row("SHUKCHOKNNISSUTTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("SHUKCHOKNNISSUTTL"))
            WW_TA0003row("SHUKCHOKNISSU") = T0009TIME.ZEROtoSpace(WW_TA0003row("SHUKCHOKNISSU"))
            WW_TA0003row("SHUKCHOKNISSUCHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("SHUKCHOKNISSUCHO"))
            WW_TA0003row("SHUKCHOKNISSUTTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("SHUKCHOKNISSUTTL"))
            WW_TA0003row("TOKSAAKAISU") = T0009TIME.ZEROtoSpace(WW_TA0003row("TOKSAAKAISU"))
            WW_TA0003row("TOKSAAKAISUCHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("TOKSAAKAISUCHO"))
            WW_TA0003row("TOKSAAKAISUTTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("TOKSAAKAISUTTL"))
            WW_TA0003row("TOKSABKAISU") = T0009TIME.ZEROtoSpace(WW_TA0003row("TOKSABKAISU"))
            WW_TA0003row("TOKSABKAISUCHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("TOKSABKAISUCHO"))
            WW_TA0003row("TOKSABKAISUTTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("TOKSABKAISUTTL"))
            WW_TA0003row("TOKSACKAISU") = T0009TIME.ZEROtoSpace(WW_TA0003row("TOKSACKAISU"))
            WW_TA0003row("TOKSACKAISUCHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("TOKSACKAISUCHO"))
            WW_TA0003row("TOKSACKAISUTTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("TOKSACKAISUTTL"))
            WW_TA0003row("TENKOKAISU") = T0009TIME.ZEROtoSpace(WW_TA0003row("TENKOKAISU"))
            WW_TA0003row("TENKOKAISUCHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("TENKOKAISUCHO"))
            WW_TA0003row("TENKOKAISUTTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("TENKOKAISUTTL"))
            WW_TA0003row("HOANTIME") = T0009TIME.ZEROtoSpace(WW_TA0003row("HOANTIME"))
            WW_TA0003row("HOANTIMECHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("HOANTIMECHO"))
            WW_TA0003row("HOANTIMETTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("HOANTIMETTL"))
            WW_TA0003row("KOATUTIME") = T0009TIME.ZEROtoSpace(WW_TA0003row("KOATUTIME"))
            WW_TA0003row("KOATUTIMECHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("KOATUTIMECHO"))
            WW_TA0003row("KOATUTIMETTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("KOATUTIMETTL"))
            WW_TA0003row("TOKUSA1TIME") = T0009TIME.ZEROtoSpace(WW_TA0003row("TOKUSA1TIME"))
            WW_TA0003row("TOKUSA1TIMECHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("TOKUSA1TIMECHO"))
            WW_TA0003row("TOKUSA1TIMETTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("TOKUSA1TIMETTL"))
            WW_TA0003row("HAYADETIME") = T0009TIME.ZEROtoSpace(WW_TA0003row("HAYADETIME"))
            WW_TA0003row("HAYADETIMECHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("HAYADETIMECHO"))
            WW_TA0003row("HAYADETIMETTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("HAYADETIMETTL"))
            WW_TA0003row("PONPNISSU") = T0009TIME.ZEROtoSpace(WW_TA0003row("PONPNISSU"))
            WW_TA0003row("PONPNISSUCHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("PONPNISSUCHO"))
            WW_TA0003row("PONPNISSUTTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("PONPNISSUTTL"))
            WW_TA0003row("BULKNISSU") = T0009TIME.ZEROtoSpace(WW_TA0003row("BULKNISSU"))
            WW_TA0003row("BULKNISSUCHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("BULKNISSUCHO"))
            WW_TA0003row("BULKNISSUTTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("BULKNISSUTTL"))
            WW_TA0003row("TRAILERNISSU") = T0009TIME.ZEROtoSpace(WW_TA0003row("TRAILERNISSU"))
            WW_TA0003row("TRAILERNISSUCHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("TRAILERNISSUCHO"))
            WW_TA0003row("TRAILERNISSUTTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("TRAILERNISSUTTL"))
            WW_TA0003row("BKINMUKAISU") = T0009TIME.ZEROtoSpace(WW_TA0003row("BKINMUKAISU"))
            WW_TA0003row("BKINMUKAISUCHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("BKINMUKAISUCHO"))
            WW_TA0003row("BKINMUKAISUTTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("BKINMUKAISUTTL"))

            WW_TA0003row("NENMATUNISSU") = T0009TIME.ZEROtoSpace(WW_TA0003row("NENMATUNISSU"))
            WW_TA0003row("NENMATUNISSUCHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("NENMATUNISSUCHO"))
            WW_TA0003row("NENMATUNISSUTTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("NENMATUNISSUTTL"))
            WW_TA0003row("SHACHUHAKNISSU") = T0009TIME.ZEROtoSpace(WW_TA0003row("SHACHUHAKNISSU"))
            WW_TA0003row("SHACHUHAKNISSUCHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("SHACHUHAKNISSUCHO"))
            WW_TA0003row("SHACHUHAKNISSUTTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("SHACHUHAKNISSUTTL"))

            WW_TA0003row("OILPAYKBN101") = WW_TA0003row("OILPAYKBN101")
            WW_TA0003row("OILPAYKBNNAMES101") = WW_TA0003row("OILPAYKBNNAMES101")
            WW_TA0003row("HAIDISTANCE101") = Val(WW_TA0003row("HAIDISTANCE101")).ToString("#")
            WW_TA0003row("HAIDISTANCECHO101") = Val(WW_TA0003row("HAIDISTANCECHO101")).ToString("#")
            WW_TA0003row("HAIDISTANCETTL101") = Val(WW_TA0003row("HAIDISTANCETTL101")).ToString("#")
            WW_TA0003row("KAIDISTANCE101") = Val(WW_TA0003row("KAIDISTANCE101")).ToString("#")
            WW_TA0003row("KAIDISTANCECHO101") = Val(WW_TA0003row("KAIDISTANCECHO101")).ToString("#")
            WW_TA0003row("KAIDISTANCETTL101") = Val(WW_TA0003row("KAIDISTANCETTL101")).ToString("#")
            WW_TA0003row("UNLOADCNT101") = Val(WW_TA0003row("UNLOADCNT101")).ToString("#")
            WW_TA0003row("UNLOADCNTCHO101") = Val(WW_TA0003row("UNLOADCNTCHO101")).ToString("#")
            WW_TA0003row("UNLOADCNTTTL101") = Val(WW_TA0003row("UNLOADCNTTTL101")).ToString("#")
            WW_TA0003row("KAITENCNT101") = Val(WW_TA0003row("KAITENCNT101")).ToString("#")
            WW_TA0003row("KAITENCNTCHO101") = Val(WW_TA0003row("KAITENCNTCHO101")).ToString("#")
            WW_TA0003row("KAITENCNTTTL101") = Val(WW_TA0003row("KAITENCNTTTL101")).ToString("#")
            WW_TA0003row("MODELDISTANCE101") = Val(WW_TA0003row("MODELDISTANCE101")).ToString("#")
            WW_TA0003row("MODELDISTANCECHO101") = Val(WW_TA0003row("MODELDISTANCECHO101")).ToString("#")
            WW_TA0003row("MODELDISTANCETTL101") = Val(WW_TA0003row("MODELDISTANCETTL101")).ToString("#")

            WW_TA0003row("OILPAYKBN102") = WW_TA0003row("OILPAYKBN102")
            WW_TA0003row("OILPAYKBNNAMES102") = WW_TA0003row("OILPAYKBNNAMES102")
            WW_TA0003row("HAIDISTANCE102") = Val(WW_TA0003row("HAIDISTANCE102")).ToString("#")
            WW_TA0003row("HAIDISTANCECHO102") = Val(WW_TA0003row("HAIDISTANCECHO102")).ToString("#")
            WW_TA0003row("HAIDISTANCETTL102") = Val(WW_TA0003row("HAIDISTANCETTL102")).ToString("#")
            WW_TA0003row("KAIDISTANCE102") = Val(WW_TA0003row("KAIDISTANCE102")).ToString("#")
            WW_TA0003row("KAIDISTANCECHO102") = Val(WW_TA0003row("KAIDISTANCECHO102")).ToString("#")
            WW_TA0003row("KAIDISTANCETTL102") = Val(WW_TA0003row("KAIDISTANCETTL102")).ToString("#")
            WW_TA0003row("UNLOADCNT102") = Val(WW_TA0003row("UNLOADCNT102")).ToString("#")
            WW_TA0003row("UNLOADCNTCHO102") = Val(WW_TA0003row("UNLOADCNTCHO102")).ToString("#")
            WW_TA0003row("UNLOADCNTTTL102") = Val(WW_TA0003row("UNLOADCNTTTL102")).ToString("#")
            WW_TA0003row("KAITENCNT102") = Val(WW_TA0003row("KAITENCNT102")).ToString("#")
            WW_TA0003row("KAITENCNTCHO102") = Val(WW_TA0003row("KAITENCNTCHO102")).ToString("#")
            WW_TA0003row("KAITENCNTTTL102") = Val(WW_TA0003row("KAITENCNTTTL102")).ToString("#")
            WW_TA0003row("MODELDISTANCE102") = Val(WW_TA0003row("MODELDISTANCE102")).ToString("#")
            WW_TA0003row("MODELDISTANCECHO102") = Val(WW_TA0003row("MODELDISTANCECHO102")).ToString("#")
            WW_TA0003row("MODELDISTANCETTL102") = Val(WW_TA0003row("MODELDISTANCETTL102")).ToString("#")

            WW_TA0003row("OILPAYKBN103") = WW_TA0003row("OILPAYKBN103")
            WW_TA0003row("OILPAYKBNNAMES103") = WW_TA0003row("OILPAYKBNNAMES103")
            WW_TA0003row("HAIDISTANCE103") = Val(WW_TA0003row("HAIDISTANCE103")).ToString("#")
            WW_TA0003row("HAIDISTANCECHO103") = Val(WW_TA0003row("HAIDISTANCECHO103")).ToString("#")
            WW_TA0003row("HAIDISTANCETTL103") = Val(WW_TA0003row("HAIDISTANCETTL103")).ToString("#")
            WW_TA0003row("KAIDISTANCE103") = Val(WW_TA0003row("KAIDISTANCE103")).ToString("#")
            WW_TA0003row("KAIDISTANCECHO103") = Val(WW_TA0003row("KAIDISTANCECHO103")).ToString("#")
            WW_TA0003row("KAIDISTANCETTL103") = Val(WW_TA0003row("KAIDISTANCETTL103")).ToString("#")
            WW_TA0003row("UNLOADCNT103") = Val(WW_TA0003row("UNLOADCNT103")).ToString("#")
            WW_TA0003row("UNLOADCNTCHO103") = Val(WW_TA0003row("UNLOADCNTCHO103")).ToString("#")
            WW_TA0003row("UNLOADCNTTTL103") = Val(WW_TA0003row("UNLOADCNTTTL103")).ToString("#")
            WW_TA0003row("KAITENCNT103") = Val(WW_TA0003row("KAITENCNT103")).ToString("#")
            WW_TA0003row("KAITENCNTCHO103") = Val(WW_TA0003row("KAITENCNTCHO103")).ToString("#")
            WW_TA0003row("KAITENCNTTTL103") = Val(WW_TA0003row("KAITENCNTTTL103")).ToString("#")
            WW_TA0003row("MODELDISTANCE103") = Val(WW_TA0003row("MODELDISTANCE103")).ToString("#")
            WW_TA0003row("MODELDISTANCECHO103") = Val(WW_TA0003row("MODELDISTANCECHO103")).ToString("#")
            WW_TA0003row("MODELDISTANCETTL103") = Val(WW_TA0003row("MODELDISTANCETTL103")).ToString("#")

            WW_TA0003row("OILPAYKBN104") = WW_TA0003row("OILPAYKBN104")
            WW_TA0003row("OILPAYKBNNAMES104") = WW_TA0003row("OILPAYKBNNAMES104")
            WW_TA0003row("HAIDISTANCE104") = Val(WW_TA0003row("HAIDISTANCE104")).ToString("#")
            WW_TA0003row("HAIDISTANCECHO104") = Val(WW_TA0003row("HAIDISTANCECHO104")).ToString("#")
            WW_TA0003row("HAIDISTANCETTL104") = Val(WW_TA0003row("HAIDISTANCETTL104")).ToString("#")
            WW_TA0003row("KAIDISTANCE104") = Val(WW_TA0003row("KAIDISTANCE104")).ToString("#")
            WW_TA0003row("KAIDISTANCECHO104") = Val(WW_TA0003row("KAIDISTANCECHO104")).ToString("#")
            WW_TA0003row("KAIDISTANCETTL104") = Val(WW_TA0003row("KAIDISTANCETTL104")).ToString("#")
            WW_TA0003row("UNLOADCNT104") = Val(WW_TA0003row("UNLOADCNT104")).ToString("#")
            WW_TA0003row("UNLOADCNTCHO104") = Val(WW_TA0003row("UNLOADCNTCHO104")).ToString("#")
            WW_TA0003row("UNLOADCNTTTL104") = Val(WW_TA0003row("UNLOADCNTTTL104")).ToString("#")
            WW_TA0003row("KAITENCNT104") = Val(WW_TA0003row("KAITENCNT104")).ToString("#")
            WW_TA0003row("KAITENCNTCHO104") = Val(WW_TA0003row("KAITENCNTCHO104")).ToString("#")
            WW_TA0003row("KAITENCNTTTL104") = Val(WW_TA0003row("KAITENCNTTTL104")).ToString("#")
            WW_TA0003row("MODELDISTANCE104") = Val(WW_TA0003row("MODELDISTANCE104")).ToString("#")
            WW_TA0003row("MODELDISTANCECHO104") = Val(WW_TA0003row("MODELDISTANCECHO104")).ToString("#")
            WW_TA0003row("MODELDISTANCETTL104") = Val(WW_TA0003row("MODELDISTANCETTL104")).ToString("#")

            WW_TA0003row("OILPAYKBN105") = WW_TA0003row("OILPAYKBN105")
            WW_TA0003row("OILPAYKBNNAMES105") = WW_TA0003row("OILPAYKBNNAMES105")
            WW_TA0003row("HAIDISTANCE105") = Val(WW_TA0003row("HAIDISTANCE105")).ToString("#")
            WW_TA0003row("HAIDISTANCECHO105") = Val(WW_TA0003row("HAIDISTANCECHO105")).ToString("#")
            WW_TA0003row("HAIDISTANCETTL105") = Val(WW_TA0003row("HAIDISTANCETTL105")).ToString("#")
            WW_TA0003row("KAIDISTANCE105") = Val(WW_TA0003row("KAIDISTANCE105")).ToString("#")
            WW_TA0003row("KAIDISTANCECHO105") = Val(WW_TA0003row("KAIDISTANCECHO105")).ToString("#")
            WW_TA0003row("KAIDISTANCETTL105") = Val(WW_TA0003row("KAIDISTANCETTL105")).ToString("#")
            WW_TA0003row("UNLOADCNT105") = Val(WW_TA0003row("UNLOADCNT105")).ToString("#")
            WW_TA0003row("UNLOADCNTCHO105") = Val(WW_TA0003row("UNLOADCNTCHO105")).ToString("#")
            WW_TA0003row("UNLOADCNTTTL105") = Val(WW_TA0003row("UNLOADCNTTTL105")).ToString("#")
            WW_TA0003row("KAITENCNT105") = Val(WW_TA0003row("KAITENCNT105")).ToString("#")
            WW_TA0003row("KAITENCNTCHO105") = Val(WW_TA0003row("KAITENCNTCHO105")).ToString("#")
            WW_TA0003row("KAITENCNTTTL105") = Val(WW_TA0003row("KAITENCNTTTL105")).ToString("#")
            WW_TA0003row("MODELDISTANCE105") = Val(WW_TA0003row("MODELDISTANCE105")).ToString("#")
            WW_TA0003row("MODELDISTANCECHO105") = Val(WW_TA0003row("MODELDISTANCECHO105")).ToString("#")
            WW_TA0003row("MODELDISTANCETTL105") = Val(WW_TA0003row("MODELDISTANCETTL105")).ToString("#")

            WW_TA0003row("OILPAYKBN106") = WW_TA0003row("OILPAYKBN106")
            WW_TA0003row("OILPAYKBNNAMES106") = WW_TA0003row("OILPAYKBNNAMES106")
            WW_TA0003row("HAIDISTANCE106") = Val(WW_TA0003row("HAIDISTANCE106")).ToString("#")
            WW_TA0003row("HAIDISTANCECHO106") = Val(WW_TA0003row("HAIDISTANCECHO106")).ToString("#")
            WW_TA0003row("HAIDISTANCETTL106") = Val(WW_TA0003row("HAIDISTANCETTL106")).ToString("#")
            WW_TA0003row("KAIDISTANCE106") = Val(WW_TA0003row("KAIDISTANCE106")).ToString("#")
            WW_TA0003row("KAIDISTANCECHO106") = Val(WW_TA0003row("KAIDISTANCECHO106")).ToString("#")
            WW_TA0003row("KAIDISTANCETTL106") = Val(WW_TA0003row("KAIDISTANCETTL106")).ToString("#")
            WW_TA0003row("UNLOADCNT106") = Val(WW_TA0003row("UNLOADCNT106")).ToString("#")
            WW_TA0003row("UNLOADCNTCHO106") = Val(WW_TA0003row("UNLOADCNTCHO106")).ToString("#")
            WW_TA0003row("UNLOADCNTTTL106") = Val(WW_TA0003row("UNLOADCNTTTL106")).ToString("#")
            WW_TA0003row("KAITENCNT106") = Val(WW_TA0003row("KAITENCNT106")).ToString("#")
            WW_TA0003row("KAITENCNTCHO106") = Val(WW_TA0003row("KAITENCNTCHO106")).ToString("#")
            WW_TA0003row("KAITENCNTTTL106") = Val(WW_TA0003row("KAITENCNTTTL106")).ToString("#")
            WW_TA0003row("MODELDISTANCE106") = Val(WW_TA0003row("MODELDISTANCE106")).ToString("#")
            WW_TA0003row("MODELDISTANCECHO106") = Val(WW_TA0003row("MODELDISTANCECHO106")).ToString("#")
            WW_TA0003row("MODELDISTANCETTL106") = Val(WW_TA0003row("MODELDISTANCETTL106")).ToString("#")

            WW_TA0003row("OILPAYKBN107") = WW_TA0003row("OILPAYKBN107")
            WW_TA0003row("OILPAYKBNNAMES107") = WW_TA0003row("OILPAYKBNNAMES107")
            WW_TA0003row("HAIDISTANCE107") = Val(WW_TA0003row("HAIDISTANCE107")).ToString("#")
            WW_TA0003row("HAIDISTANCECHO107") = Val(WW_TA0003row("HAIDISTANCECHO107")).ToString("#")
            WW_TA0003row("HAIDISTANCETTL107") = Val(WW_TA0003row("HAIDISTANCETTL107")).ToString("#")
            WW_TA0003row("KAIDISTANCE107") = Val(WW_TA0003row("KAIDISTANCE107")).ToString("#")
            WW_TA0003row("KAIDISTANCECHO107") = Val(WW_TA0003row("KAIDISTANCECHO107")).ToString("#")
            WW_TA0003row("KAIDISTANCETTL107") = Val(WW_TA0003row("KAIDISTANCETTL107")).ToString("#")
            WW_TA0003row("UNLOADCNT107") = Val(WW_TA0003row("UNLOADCNT107")).ToString("#")
            WW_TA0003row("UNLOADCNTCHO107") = Val(WW_TA0003row("UNLOADCNTCHO107")).ToString("#")
            WW_TA0003row("UNLOADCNTTTL107") = Val(WW_TA0003row("UNLOADCNTTTL107")).ToString("#")
            WW_TA0003row("KAITENCNT107") = Val(WW_TA0003row("KAITENCNT107")).ToString("#")
            WW_TA0003row("KAITENCNTCHO107") = Val(WW_TA0003row("KAITENCNTCHO107")).ToString("#")
            WW_TA0003row("KAITENCNTTTL107") = Val(WW_TA0003row("KAITENCNTTTL107")).ToString("#")
            WW_TA0003row("MODELDISTANCE107") = Val(WW_TA0003row("MODELDISTANCE107")).ToString("#")
            WW_TA0003row("MODELDISTANCECHO107") = Val(WW_TA0003row("MODELDISTANCECHO107")).ToString("#")
            WW_TA0003row("MODELDISTANCETTL107") = Val(WW_TA0003row("MODELDISTANCETTL107")).ToString("#")

            WW_TA0003row("OILPAYKBN108") = WW_TA0003row("OILPAYKBN108")
            WW_TA0003row("OILPAYKBNNAMES108") = WW_TA0003row("OILPAYKBNNAMES108")
            WW_TA0003row("HAIDISTANCE108") = Val(WW_TA0003row("HAIDISTANCE108")).ToString("#")
            WW_TA0003row("HAIDISTANCECHO108") = Val(WW_TA0003row("HAIDISTANCECHO108")).ToString("#")
            WW_TA0003row("HAIDISTANCETTL108") = Val(WW_TA0003row("HAIDISTANCETTL108")).ToString("#")
            WW_TA0003row("KAIDISTANCE108") = Val(WW_TA0003row("KAIDISTANCE108")).ToString("#")
            WW_TA0003row("KAIDISTANCECHO108") = Val(WW_TA0003row("KAIDISTANCECHO108")).ToString("#")
            WW_TA0003row("KAIDISTANCETTL108") = Val(WW_TA0003row("KAIDISTANCETTL108")).ToString("#")
            WW_TA0003row("UNLOADCNT108") = Val(WW_TA0003row("UNLOADCNT108")).ToString("#")
            WW_TA0003row("UNLOADCNTCHO108") = Val(WW_TA0003row("UNLOADCNTCHO108")).ToString("#")
            WW_TA0003row("UNLOADCNTTTL108") = Val(WW_TA0003row("UNLOADCNTTTL108")).ToString("#")
            WW_TA0003row("KAITENCNT108") = Val(WW_TA0003row("KAITENCNT108")).ToString("#")
            WW_TA0003row("KAITENCNTCHO108") = Val(WW_TA0003row("KAITENCNTCHO108")).ToString("#")
            WW_TA0003row("KAITENCNTTTL108") = Val(WW_TA0003row("KAITENCNTTTL108")).ToString("#")
            WW_TA0003row("MODELDISTANCE108") = Val(WW_TA0003row("MODELDISTANCE108")).ToString("#")
            WW_TA0003row("MODELDISTANCECHO108") = Val(WW_TA0003row("MODELDISTANCECHO108")).ToString("#")
            WW_TA0003row("MODELDISTANCETTL108") = Val(WW_TA0003row("MODELDISTANCETTL108")).ToString("#")

            WW_TA0003row("OILPAYKBN109") = WW_TA0003row("OILPAYKBN109")
            WW_TA0003row("OILPAYKBNNAMES109") = WW_TA0003row("OILPAYKBNNAMES109")
            WW_TA0003row("HAIDISTANCE109") = Val(WW_TA0003row("HAIDISTANCE109")).ToString("#")
            WW_TA0003row("HAIDISTANCECHO109") = Val(WW_TA0003row("HAIDISTANCECHO109")).ToString("#")
            WW_TA0003row("HAIDISTANCETTL109") = Val(WW_TA0003row("HAIDISTANCETTL109")).ToString("#")
            WW_TA0003row("KAIDISTANCE109") = Val(WW_TA0003row("KAIDISTANCE109")).ToString("#")
            WW_TA0003row("KAIDISTANCECHO109") = Val(WW_TA0003row("KAIDISTANCECHO109")).ToString("#")
            WW_TA0003row("KAIDISTANCETTL109") = Val(WW_TA0003row("KAIDISTANCETTL109")).ToString("#")
            WW_TA0003row("UNLOADCNT109") = Val(WW_TA0003row("UNLOADCNT109")).ToString("#")
            WW_TA0003row("UNLOADCNTCHO109") = Val(WW_TA0003row("UNLOADCNTCHO109")).ToString("#")
            WW_TA0003row("UNLOADCNTTTL109") = Val(WW_TA0003row("UNLOADCNTTTL109")).ToString("#")
            WW_TA0003row("KAITENCNT109") = Val(WW_TA0003row("KAITENCNT109")).ToString("#")
            WW_TA0003row("KAITENCNTCHO109") = Val(WW_TA0003row("KAITENCNTCHO109")).ToString("#")
            WW_TA0003row("KAITENCNTTTL109") = Val(WW_TA0003row("KAITENCNTTTL109")).ToString("#")
            WW_TA0003row("MODELDISTANCE109") = Val(WW_TA0003row("MODELDISTANCE109")).ToString("#")
            WW_TA0003row("MODELDISTANCECHO109") = Val(WW_TA0003row("MODELDISTANCECHO109")).ToString("#")
            WW_TA0003row("MODELDISTANCETTL109") = Val(WW_TA0003row("MODELDISTANCETTL109")).ToString("#")

            WW_TA0003row("OILPAYKBN110") = WW_TA0003row("OILPAYKBN110")
            WW_TA0003row("OILPAYKBNNAMES110") = WW_TA0003row("OILPAYKBNNAMES110")
            WW_TA0003row("HAIDISTANCE110") = Val(WW_TA0003row("HAIDISTANCE110")).ToString("#")
            WW_TA0003row("HAIDISTANCECHO110") = Val(WW_TA0003row("HAIDISTANCECHO110")).ToString("#")
            WW_TA0003row("HAIDISTANCETTL110") = Val(WW_TA0003row("HAIDISTANCETTL110")).ToString("#")
            WW_TA0003row("KAIDISTANCE110") = Val(WW_TA0003row("KAIDISTANCE110")).ToString("#")
            WW_TA0003row("KAIDISTANCECHO110") = Val(WW_TA0003row("KAIDISTANCECHO110")).ToString("#")
            WW_TA0003row("KAIDISTANCETTL110") = Val(WW_TA0003row("KAIDISTANCETTL110")).ToString("#")
            WW_TA0003row("UNLOADCNT110") = Val(WW_TA0003row("UNLOADCNT110")).ToString("#")
            WW_TA0003row("UNLOADCNTCHO110") = Val(WW_TA0003row("UNLOADCNTCHO110")).ToString("#")
            WW_TA0003row("UNLOADCNTTTL110") = Val(WW_TA0003row("UNLOADCNTTTL110")).ToString("#")
            WW_TA0003row("KAITENCNT110") = Val(WW_TA0003row("KAITENCNT110")).ToString("#")
            WW_TA0003row("KAITENCNTCHO110") = Val(WW_TA0003row("KAITENCNTCHO110")).ToString("#")
            WW_TA0003row("KAITENCNTTTL110") = Val(WW_TA0003row("KAITENCNTTTL110")).ToString("#")
            WW_TA0003row("MODELDISTANCE110") = Val(WW_TA0003row("MODELDISTANCE110")).ToString("#")
            WW_TA0003row("MODELDISTANCECHO110") = Val(WW_TA0003row("MODELDISTANCECHO110")).ToString("#")
            WW_TA0003row("MODELDISTANCETTL110") = Val(WW_TA0003row("MODELDISTANCETTL110")).ToString("#")

            WW_TA0003row("OILPAYKBN201") = WW_TA0003row("OILPAYKBN201")
            WW_TA0003row("OILPAYKBNNAMES201") = WW_TA0003row("OILPAYKBNNAMES201")
            WW_TA0003row("HAIDISTANCE201") = Val(WW_TA0003row("HAIDISTANCE201")).ToString("#")
            WW_TA0003row("HAIDISTANCECHO201") = Val(WW_TA0003row("HAIDISTANCECHO201")).ToString("#")
            WW_TA0003row("HAIDISTANCETTL201") = Val(WW_TA0003row("HAIDISTANCETTL201")).ToString("#")
            WW_TA0003row("KAIDISTANCE201") = Val(WW_TA0003row("KAIDISTANCE201")).ToString("#")
            WW_TA0003row("KAIDISTANCECHO201") = Val(WW_TA0003row("KAIDISTANCECHO201")).ToString("#")
            WW_TA0003row("KAIDISTANCETTL201") = Val(WW_TA0003row("KAIDISTANCETTL201")).ToString("#")
            WW_TA0003row("UNLOADCNT201") = Val(WW_TA0003row("UNLOADCNT201")).ToString("#")
            WW_TA0003row("UNLOADCNTCHO201") = Val(WW_TA0003row("UNLOADCNTCHO201")).ToString("#")
            WW_TA0003row("UNLOADCNTTTL201") = Val(WW_TA0003row("UNLOADCNTTTL201")).ToString("#")
            WW_TA0003row("KAITENCNT201") = Val(WW_TA0003row("KAITENCNT201")).ToString("#")
            WW_TA0003row("KAITENCNTCHO201") = Val(WW_TA0003row("KAITENCNTCHO201")).ToString("#")
            WW_TA0003row("KAITENCNTTTL201") = Val(WW_TA0003row("KAITENCNTTTL201")).ToString("#")
            WW_TA0003row("MODELDISTANCE201") = Val(WW_TA0003row("MODELDISTANCE201")).ToString("#")
            WW_TA0003row("MODELDISTANCECHO201") = Val(WW_TA0003row("MODELDISTANCECHO201")).ToString("#")
            WW_TA0003row("MODELDISTANCETTL201") = Val(WW_TA0003row("MODELDISTANCETTL201")).ToString("#")

            WW_TA0003row("OILPAYKBN202") = WW_TA0003row("OILPAYKBN202")
            WW_TA0003row("OILPAYKBNNAMES202") = WW_TA0003row("OILPAYKBNNAMES202")
            WW_TA0003row("HAIDISTANCE202") = Val(WW_TA0003row("HAIDISTANCE202")).ToString("#")
            WW_TA0003row("HAIDISTANCECHO202") = Val(WW_TA0003row("HAIDISTANCECHO202")).ToString("#")
            WW_TA0003row("HAIDISTANCETTL202") = Val(WW_TA0003row("HAIDISTANCETTL202")).ToString("#")
            WW_TA0003row("KAIDISTANCE202") = Val(WW_TA0003row("KAIDISTANCE202")).ToString("#")
            WW_TA0003row("KAIDISTANCECHO202") = Val(WW_TA0003row("KAIDISTANCECHO202")).ToString("#")
            WW_TA0003row("KAIDISTANCETTL202") = Val(WW_TA0003row("KAIDISTANCETTL202")).ToString("#")
            WW_TA0003row("UNLOADCNT202") = Val(WW_TA0003row("UNLOADCNT202")).ToString("#")
            WW_TA0003row("UNLOADCNTCHO202") = Val(WW_TA0003row("UNLOADCNTCHO202")).ToString("#")
            WW_TA0003row("UNLOADCNTTTL202") = Val(WW_TA0003row("UNLOADCNTTTL202")).ToString("#")
            WW_TA0003row("KAITENCNT202") = Val(WW_TA0003row("KAITENCNT202")).ToString("#")
            WW_TA0003row("KAITENCNTCHO202") = Val(WW_TA0003row("KAITENCNTCHO202")).ToString("#")
            WW_TA0003row("KAITENCNTTTL202") = Val(WW_TA0003row("KAITENCNTTTL202")).ToString("#")
            WW_TA0003row("MODELDISTANCE202") = Val(WW_TA0003row("MODELDISTANCE202")).ToString("#")
            WW_TA0003row("MODELDISTANCECHO202") = Val(WW_TA0003row("MODELDISTANCECHO202")).ToString("#")
            WW_TA0003row("MODELDISTANCETTL202") = Val(WW_TA0003row("MODELDISTANCETTL202")).ToString("#")

            WW_TA0003row("OILPAYKBN203") = WW_TA0003row("OILPAYKBN203")
            WW_TA0003row("OILPAYKBNNAMES203") = WW_TA0003row("OILPAYKBNNAMES203")
            WW_TA0003row("HAIDISTANCE203") = Val(WW_TA0003row("HAIDISTANCE203")).ToString("#")
            WW_TA0003row("HAIDISTANCECHO203") = Val(WW_TA0003row("HAIDISTANCECHO203")).ToString("#")
            WW_TA0003row("HAIDISTANCETTL203") = Val(WW_TA0003row("HAIDISTANCETTL203")).ToString("#")
            WW_TA0003row("KAIDISTANCE203") = Val(WW_TA0003row("KAIDISTANCE203")).ToString("#")
            WW_TA0003row("KAIDISTANCECHO203") = Val(WW_TA0003row("KAIDISTANCECHO203")).ToString("#")
            WW_TA0003row("KAIDISTANCETTL203") = Val(WW_TA0003row("KAIDISTANCETTL203")).ToString("#")
            WW_TA0003row("UNLOADCNT203") = Val(WW_TA0003row("UNLOADCNT203")).ToString("#")
            WW_TA0003row("UNLOADCNTCHO203") = Val(WW_TA0003row("UNLOADCNTCHO203")).ToString("#")
            WW_TA0003row("UNLOADCNTTTL203") = Val(WW_TA0003row("UNLOADCNTTTL203")).ToString("#")
            WW_TA0003row("KAITENCNT203") = Val(WW_TA0003row("KAITENCNT203")).ToString("#")
            WW_TA0003row("KAITENCNTCHO203") = Val(WW_TA0003row("KAITENCNTCHO203")).ToString("#")
            WW_TA0003row("KAITENCNTTTL203") = Val(WW_TA0003row("KAITENCNTTTL203")).ToString("#")
            WW_TA0003row("MODELDISTANCE203") = Val(WW_TA0003row("MODELDISTANCE203")).ToString("#")
            WW_TA0003row("MODELDISTANCECHO203") = Val(WW_TA0003row("MODELDISTANCECHO203")).ToString("#")
            WW_TA0003row("MODELDISTANCETTL203") = Val(WW_TA0003row("MODELDISTANCETTL203")).ToString("#")

            WW_TA0003row("OILPAYKBN204") = WW_TA0003row("OILPAYKBN204")
            WW_TA0003row("OILPAYKBNNAMES204") = WW_TA0003row("OILPAYKBNNAMES204")
            WW_TA0003row("HAIDISTANCE204") = Val(WW_TA0003row("HAIDISTANCE204")).ToString("#")
            WW_TA0003row("HAIDISTANCECHO204") = Val(WW_TA0003row("HAIDISTANCECHO204")).ToString("#")
            WW_TA0003row("HAIDISTANCETTL204") = Val(WW_TA0003row("HAIDISTANCETTL204")).ToString("#")
            WW_TA0003row("KAIDISTANCE204") = Val(WW_TA0003row("KAIDISTANCE204")).ToString("#")
            WW_TA0003row("KAIDISTANCECHO204") = Val(WW_TA0003row("KAIDISTANCECHO204")).ToString("#")
            WW_TA0003row("KAIDISTANCETTL204") = Val(WW_TA0003row("KAIDISTANCETTL204")).ToString("#")
            WW_TA0003row("UNLOADCNT204") = Val(WW_TA0003row("UNLOADCNT204")).ToString("#")
            WW_TA0003row("UNLOADCNTCHO204") = Val(WW_TA0003row("UNLOADCNTCHO204")).ToString("#")
            WW_TA0003row("UNLOADCNTTTL204") = Val(WW_TA0003row("UNLOADCNTTTL204")).ToString("#")
            WW_TA0003row("KAITENCNT204") = Val(WW_TA0003row("KAITENCNT204")).ToString("#")
            WW_TA0003row("KAITENCNTCHO204") = Val(WW_TA0003row("KAITENCNTCHO204")).ToString("#")
            WW_TA0003row("KAITENCNTTTL204") = Val(WW_TA0003row("KAITENCNTTTL204")).ToString("#")
            WW_TA0003row("MODELDISTANCE204") = Val(WW_TA0003row("MODELDISTANCE204")).ToString("#")
            WW_TA0003row("MODELDISTANCECHO204") = Val(WW_TA0003row("MODELDISTANCECHO204")).ToString("#")
            WW_TA0003row("MODELDISTANCETTL204") = Val(WW_TA0003row("MODELDISTANCETTL204")).ToString("#")

            WW_TA0003row("OILPAYKBN205") = WW_TA0003row("OILPAYKBN205")
            WW_TA0003row("OILPAYKBNNAMES205") = WW_TA0003row("OILPAYKBNNAMES205")
            WW_TA0003row("HAIDISTANCE205") = Val(WW_TA0003row("HAIDISTANCE205")).ToString("#")
            WW_TA0003row("HAIDISTANCECHO205") = Val(WW_TA0003row("HAIDISTANCECHO205")).ToString("#")
            WW_TA0003row("HAIDISTANCETTL205") = Val(WW_TA0003row("HAIDISTANCETTL205")).ToString("#")
            WW_TA0003row("KAIDISTANCE205") = Val(WW_TA0003row("KAIDISTANCE205")).ToString("#")
            WW_TA0003row("KAIDISTANCECHO205") = Val(WW_TA0003row("KAIDISTANCECHO205")).ToString("#")
            WW_TA0003row("KAIDISTANCETTL205") = Val(WW_TA0003row("KAIDISTANCETTL205")).ToString("#")
            WW_TA0003row("UNLOADCNT205") = Val(WW_TA0003row("UNLOADCNT205")).ToString("#")
            WW_TA0003row("UNLOADCNTCHO205") = Val(WW_TA0003row("UNLOADCNTCHO205")).ToString("#")
            WW_TA0003row("UNLOADCNTTTL205") = Val(WW_TA0003row("UNLOADCNTTTL205")).ToString("#")
            WW_TA0003row("KAITENCNT205") = Val(WW_TA0003row("KAITENCNT205")).ToString("#")
            WW_TA0003row("KAITENCNTCHO205") = Val(WW_TA0003row("KAITENCNTCHO205")).ToString("#")
            WW_TA0003row("KAITENCNTTTL205") = Val(WW_TA0003row("KAITENCNTTTL205")).ToString("#")
            WW_TA0003row("MODELDISTANCE205") = Val(WW_TA0003row("MODELDISTANCE205")).ToString("#")
            WW_TA0003row("MODELDISTANCECHO205") = Val(WW_TA0003row("MODELDISTANCECHO205")).ToString("#")
            WW_TA0003row("MODELDISTANCETTL205") = Val(WW_TA0003row("MODELDISTANCETTL205")).ToString("#")

            WW_TA0003row("OILPAYKBN206") = WW_TA0003row("OILPAYKBN206")
            WW_TA0003row("OILPAYKBNNAMES206") = WW_TA0003row("OILPAYKBNNAMES206")
            WW_TA0003row("HAIDISTANCE206") = Val(WW_TA0003row("HAIDISTANCE206")).ToString("#")
            WW_TA0003row("HAIDISTANCECHO206") = Val(WW_TA0003row("HAIDISTANCECHO206")).ToString("#")
            WW_TA0003row("HAIDISTANCETTL206") = Val(WW_TA0003row("HAIDISTANCETTL206")).ToString("#")
            WW_TA0003row("KAIDISTANCE206") = Val(WW_TA0003row("KAIDISTANCE206")).ToString("#")
            WW_TA0003row("KAIDISTANCECHO206") = Val(WW_TA0003row("KAIDISTANCECHO206")).ToString("#")
            WW_TA0003row("KAIDISTANCETTL206") = Val(WW_TA0003row("KAIDISTANCETTL206")).ToString("#")
            WW_TA0003row("UNLOADCNT206") = Val(WW_TA0003row("UNLOADCNT206")).ToString("#")
            WW_TA0003row("UNLOADCNTCHO206") = Val(WW_TA0003row("UNLOADCNTCHO206")).ToString("#")
            WW_TA0003row("UNLOADCNTTTL206") = Val(WW_TA0003row("UNLOADCNTTTL206")).ToString("#")
            WW_TA0003row("KAITENCNT206") = Val(WW_TA0003row("KAITENCNT206")).ToString("#")
            WW_TA0003row("KAITENCNTCHO206") = Val(WW_TA0003row("KAITENCNTCHO206")).ToString("#")
            WW_TA0003row("KAITENCNTTTL206") = Val(WW_TA0003row("KAITENCNTTTL206")).ToString("#")
            WW_TA0003row("MODELDISTANCE206") = Val(WW_TA0003row("MODELDISTANCE206")).ToString("#")
            WW_TA0003row("MODELDISTANCECHO206") = Val(WW_TA0003row("MODELDISTANCECHO206")).ToString("#")
            WW_TA0003row("MODELDISTANCETTL206") = Val(WW_TA0003row("MODELDISTANCETTL206")).ToString("#")

            WW_TA0003row("OILPAYKBN207") = WW_TA0003row("OILPAYKBN207")
            WW_TA0003row("OILPAYKBNNAMES207") = WW_TA0003row("OILPAYKBNNAMES207")
            WW_TA0003row("HAIDISTANCE207") = Val(WW_TA0003row("HAIDISTANCE207")).ToString("#")
            WW_TA0003row("HAIDISTANCECHO207") = Val(WW_TA0003row("HAIDISTANCECHO207")).ToString("#")
            WW_TA0003row("HAIDISTANCETTL207") = Val(WW_TA0003row("HAIDISTANCETTL207")).ToString("#")
            WW_TA0003row("KAIDISTANCE207") = Val(WW_TA0003row("KAIDISTANCE207")).ToString("#")
            WW_TA0003row("KAIDISTANCECHO207") = Val(WW_TA0003row("KAIDISTANCECHO207")).ToString("#")
            WW_TA0003row("KAIDISTANCETTL207") = Val(WW_TA0003row("KAIDISTANCETTL207")).ToString("#")
            WW_TA0003row("UNLOADCNT207") = Val(WW_TA0003row("UNLOADCNT207")).ToString("#")
            WW_TA0003row("UNLOADCNTCHO207") = Val(WW_TA0003row("UNLOADCNTCHO207")).ToString("#")
            WW_TA0003row("UNLOADCNTTTL207") = Val(WW_TA0003row("UNLOADCNTTTL207")).ToString("#")
            WW_TA0003row("KAITENCNT207") = Val(WW_TA0003row("KAITENCNT207")).ToString("#")
            WW_TA0003row("KAITENCNTCHO207") = Val(WW_TA0003row("KAITENCNTCHO207")).ToString("#")
            WW_TA0003row("KAITENCNTTTL207") = Val(WW_TA0003row("KAITENCNTTTL207")).ToString("#")
            WW_TA0003row("MODELDISTANCE207") = Val(WW_TA0003row("MODELDISTANCE207")).ToString("#")
            WW_TA0003row("MODELDISTANCECHO207") = Val(WW_TA0003row("MODELDISTANCECHO207")).ToString("#")
            WW_TA0003row("MODELDISTANCETTL207") = Val(WW_TA0003row("MODELDISTANCETTL207")).ToString("#")

            WW_TA0003row("OILPAYKBN208") = WW_TA0003row("OILPAYKBN208")
            WW_TA0003row("OILPAYKBNNAMES208") = WW_TA0003row("OILPAYKBNNAMES208")
            WW_TA0003row("HAIDISTANCE208") = Val(WW_TA0003row("HAIDISTANCE208")).ToString("#")
            WW_TA0003row("HAIDISTANCECHO208") = Val(WW_TA0003row("HAIDISTANCECHO208")).ToString("#")
            WW_TA0003row("HAIDISTANCETTL208") = Val(WW_TA0003row("HAIDISTANCETTL208")).ToString("#")
            WW_TA0003row("KAIDISTANCE208") = Val(WW_TA0003row("KAIDISTANCE208")).ToString("#")
            WW_TA0003row("KAIDISTANCECHO208") = Val(WW_TA0003row("KAIDISTANCECHO208")).ToString("#")
            WW_TA0003row("KAIDISTANCETTL208") = Val(WW_TA0003row("KAIDISTANCETTL208")).ToString("#")
            WW_TA0003row("UNLOADCNT208") = Val(WW_TA0003row("UNLOADCNT208")).ToString("#")
            WW_TA0003row("UNLOADCNTCHO208") = Val(WW_TA0003row("UNLOADCNTCHO208")).ToString("#")
            WW_TA0003row("UNLOADCNTTTL208") = Val(WW_TA0003row("UNLOADCNTTTL208")).ToString("#")
            WW_TA0003row("KAITENCNT208") = Val(WW_TA0003row("KAITENCNT208")).ToString("#")
            WW_TA0003row("KAITENCNTCHO208") = Val(WW_TA0003row("KAITENCNTCHO208")).ToString("#")
            WW_TA0003row("KAITENCNTTTL208") = Val(WW_TA0003row("KAITENCNTTTL208")).ToString("#")
            WW_TA0003row("MODELDISTANCE208") = Val(WW_TA0003row("MODELDISTANCE208")).ToString("#")
            WW_TA0003row("MODELDISTANCECHO208") = Val(WW_TA0003row("MODELDISTANCECHO208")).ToString("#")
            WW_TA0003row("MODELDISTANCETTL208") = Val(WW_TA0003row("MODELDISTANCETTL208")).ToString("#")

            WW_TA0003row("OILPAYKBN209") = WW_TA0003row("OILPAYKBN209")
            WW_TA0003row("OILPAYKBNNAMES209") = WW_TA0003row("OILPAYKBNNAMES209")
            WW_TA0003row("HAIDISTANCE209") = Val(WW_TA0003row("HAIDISTANCE209")).ToString("#")
            WW_TA0003row("HAIDISTANCECHO209") = Val(WW_TA0003row("HAIDISTANCECHO209")).ToString("#")
            WW_TA0003row("HAIDISTANCETTL209") = Val(WW_TA0003row("HAIDISTANCETTL209")).ToString("#")
            WW_TA0003row("KAIDISTANCE209") = Val(WW_TA0003row("KAIDISTANCE209")).ToString("#")
            WW_TA0003row("KAIDISTANCECHO209") = Val(WW_TA0003row("KAIDISTANCECHO209")).ToString("#")
            WW_TA0003row("KAIDISTANCETTL209") = Val(WW_TA0003row("KAIDISTANCETTL209")).ToString("#")
            WW_TA0003row("UNLOADCNT209") = Val(WW_TA0003row("UNLOADCNT209")).ToString("#")
            WW_TA0003row("UNLOADCNTCHO209") = Val(WW_TA0003row("UNLOADCNTCHO209")).ToString("#")
            WW_TA0003row("UNLOADCNTTTL209") = Val(WW_TA0003row("UNLOADCNTTTL209")).ToString("#")
            WW_TA0003row("KAITENCNT209") = Val(WW_TA0003row("KAITENCNT209")).ToString("#")
            WW_TA0003row("KAITENCNTCHO209") = Val(WW_TA0003row("KAITENCNTCHO209")).ToString("#")
            WW_TA0003row("KAITENCNTTTL209") = Val(WW_TA0003row("KAITENCNTTTL209")).ToString("#")
            WW_TA0003row("MODELDISTANCE209") = Val(WW_TA0003row("MODELDISTANCE209")).ToString("#")
            WW_TA0003row("MODELDISTANCECHO209") = Val(WW_TA0003row("MODELDISTANCECHO209")).ToString("#")
            WW_TA0003row("MODELDISTANCETTL209") = Val(WW_TA0003row("MODELDISTANCETTL209")).ToString("#")

            WW_TA0003row("OILPAYKBN210") = WW_TA0003row("OILPAYKBN210")
            WW_TA0003row("OILPAYKBNNAMES210") = WW_TA0003row("OILPAYKBNNAMES210")
            WW_TA0003row("HAIDISTANCE210") = Val(WW_TA0003row("HAIDISTANCE210")).ToString("#")
            WW_TA0003row("HAIDISTANCECHO210") = Val(WW_TA0003row("HAIDISTANCECHO210")).ToString("#")
            WW_TA0003row("HAIDISTANCETTL210") = Val(WW_TA0003row("HAIDISTANCETTL210")).ToString("#")
            WW_TA0003row("KAIDISTANCE210") = Val(WW_TA0003row("KAIDISTANCE210")).ToString("#")
            WW_TA0003row("KAIDISTANCECHO210") = Val(WW_TA0003row("KAIDISTANCECHO210")).ToString("#")
            WW_TA0003row("KAIDISTANCETTL210") = Val(WW_TA0003row("KAIDISTANCETTL210")).ToString("#")
            WW_TA0003row("UNLOADCNT210") = Val(WW_TA0003row("UNLOADCNT210")).ToString("#")
            WW_TA0003row("UNLOADCNTCHO210") = Val(WW_TA0003row("UNLOADCNTCHO210")).ToString("#")
            WW_TA0003row("UNLOADCNTTTL210") = Val(WW_TA0003row("UNLOADCNTTTL210")).ToString("#")
            WW_TA0003row("KAITENCNT210") = Val(WW_TA0003row("KAITENCNT210")).ToString("#")
            WW_TA0003row("KAITENCNTCHO210") = Val(WW_TA0003row("KAITENCNTCHO210")).ToString("#")
            WW_TA0003row("KAITENCNTTTL210") = Val(WW_TA0003row("KAITENCNTTTL210")).ToString("#")
            WW_TA0003row("MODELDISTANCE210") = Val(WW_TA0003row("MODELDISTANCE210")).ToString("#")
            WW_TA0003row("MODELDISTANCECHO210") = Val(WW_TA0003row("MODELDISTANCECHO210")).ToString("#")
            WW_TA0003row("MODELDISTANCETTL210") = Val(WW_TA0003row("MODELDISTANCETTL210")).ToString("#")

            WW_TA0003row("JIKYUSHATIME") = T0009TIME.ZEROtoSpace(WW_TA0003row("JIKYUSHATIME"))
            WW_TA0003row("JIKYUSHATIMECHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("JIKYUSHATIMECHO"))
            WW_TA0003row("JIKYUSHATIMETTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("JIKYUSHATIMETTL"))

            WW_TA0003row("HAIDISTANCE") = Val(WW_TA0003row("HAIDISTANCE")).ToString("#")
            WW_TA0003row("HAIDISTANCECHO") = Val(WW_TA0003row("HAIDISTANCECHO")).ToString("#")
            WW_TA0003row("HAIDISTANCETTL") = Val(WW_TA0003row("HAIDISTANCETTL")).ToString("#")
            WW_TA0003row("SDAIWORKTIME") = T0009TIME.ZEROtoSpace(WW_TA0003row("SDAIWORKTIME"))
            WW_TA0003row("SDAIWORKTIMECHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("SDAIWORKTIMECHO"))
            WW_TA0003row("SDAIWORKTIMETTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("SDAIWORKTIMETTL"))
            WW_TA0003row("SDAINIGHTTIME") = T0009TIME.ZEROtoSpace(WW_TA0003row("SDAINIGHTTIME"))
            WW_TA0003row("SDAINIGHTTIMECHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("SDAINIGHTTIMECHO"))
            WW_TA0003row("SDAINIGHTTIMETTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("SDAINIGHTTIMETTL"))
            WW_TA0003row("HDAIWORKTIME") = T0009TIME.ZEROtoSpace(WW_TA0003row("HDAIWORKTIME"))
            WW_TA0003row("HDAIWORKTIMECHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("HDAIWORKTIMECHO"))
            WW_TA0003row("HDAIWORKTIMETTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("HDAIWORKTIMETTL"))
            WW_TA0003row("HDAINIGHTTIME") = T0009TIME.ZEROtoSpace(WW_TA0003row("HDAINIGHTTIME"))
            WW_TA0003row("HDAINIGHTTIMECHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("HDAINIGHTTIMECHO"))
            WW_TA0003row("HDAINIGHTTIMETTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("HDAINIGHTTIMETTL"))
            WW_TA0003row("WWORKTIME") = T0009TIME.ZEROtoSpace(WW_TA0003row("WWORKTIME"))
            WW_TA0003row("WWORKTIMECHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("WWORKTIMECHO"))
            WW_TA0003row("WWORKTIMETTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("WWORKTIMETTL"))
            WW_TA0003row("JYOMUTIME") = T0009TIME.ZEROtoSpace(WW_TA0003row("JYOMUTIME"))
            WW_TA0003row("JYOMUTIMECHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("JYOMUTIMECHO"))
            WW_TA0003row("JYOMUTIMETTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("JYOMUTIMETTL"))
            WW_TA0003row("HWORKNISSU") = T0009TIME.ZEROtoSpace(WW_TA0003row("HWORKNISSU"))
            WW_TA0003row("HWORKNISSUCHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("HWORKNISSUCHO"))
            WW_TA0003row("HWORKNISSUTTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("HWORKNISSUTTL"))
            WW_TA0003row("KAITENCNT") = T0009TIME.ZEROtoSpace(WW_TA0003row("KAITENCNT"))
            WW_TA0003row("KAITENCNTCHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("KAITENCNTCHO"))
            WW_TA0003row("KAITENCNTTTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("KAITENCNTTTL"))

            WW_TA0003row("SENJYOCNT") = T0009TIME.ZEROtoSpace(WW_TA0003row("SENJYOCNT"))
            WW_TA0003row("SENJYOCNTCHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("SENJYOCNTCHO"))
            WW_TA0003row("SENJYOCNTTTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("SENJYOCNTTTL"))
            WW_TA0003row("UNLOADADDCNT1") = T0009TIME.ZEROtoSpace(WW_TA0003row("UNLOADADDCNT1"))
            WW_TA0003row("UNLOADADDCNT1CHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("UNLOADADDCNT1CHO"))
            WW_TA0003row("UNLOADADDCNT1TTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("UNLOADADDCNT1TTL"))
            WW_TA0003row("UNLOADADDCNT2") = T0009TIME.ZEROtoSpace(WW_TA0003row("UNLOADADDCNT2"))
            WW_TA0003row("UNLOADADDCNT2CHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("UNLOADADDCNT2CHO"))
            WW_TA0003row("UNLOADADDCNT2TTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("UNLOADADDCNT2TTL"))
            WW_TA0003row("UNLOADADDCNT3") = T0009TIME.ZEROtoSpace(WW_TA0003row("UNLOADADDCNT3"))
            WW_TA0003row("UNLOADADDCNT3CHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("UNLOADADDCNT3CHO"))
            WW_TA0003row("UNLOADADDCNT3TTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("UNLOADADDCNT3TTL"))
            WW_TA0003row("UNLOADADDCNT4") = T0009TIME.ZEROtoSpace(WW_TA0003row("UNLOADADDCNT4"))
            WW_TA0003row("UNLOADADDCNT4CHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("UNLOADADDCNT4CHO"))
            WW_TA0003row("UNLOADADDCNT4TTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("UNLOADADDCNT4TTL"))
            WW_TA0003row("LOADINGCNT1") = T0009TIME.ZeroToSpace(WW_TA0003row("LOADINGCNT1"))
            WW_TA0003row("LOADINGCNT1CHO") = T0009TIME.ZeroToSpace(WW_TA0003row("LOADINGCNT1CHO"))
            WW_TA0003row("LOADINGCNT1TTL") = T0009TIME.ZeroToSpace(WW_TA0003row("LOADINGCNT1TTL"))
            WW_TA0003row("LOADINGCNT2") = T0009TIME.ZeroToSpace(WW_TA0003row("LOADINGCNT2"))
            WW_TA0003row("LOADINGCNT2CHO") = T0009TIME.ZeroToSpace(WW_TA0003row("LOADINGCNT2CHO"))
            WW_TA0003row("LOADINGCNT2TTL") = T0009TIME.ZeroToSpace(WW_TA0003row("LOADINGCNT2TTL"))
            WW_TA0003row("SHORTDISTANCE1") = T0009TIME.ZeroToSpace(WW_TA0003row("SHORTDISTANCE1"))
            WW_TA0003row("SHORTDISTANCE1CHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("SHORTDISTANCE1CHO"))
            WW_TA0003row("SHORTDISTANCE1TTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("SHORTDISTANCE1TTL"))
            WW_TA0003row("SHORTDISTANCE2") = T0009TIME.ZEROtoSpace(WW_TA0003row("SHORTDISTANCE2"))
            WW_TA0003row("SHORTDISTANCE2CHO") = T0009TIME.ZEROtoSpace(WW_TA0003row("SHORTDISTANCE2CHO"))
            WW_TA0003row("SHORTDISTANCE2TTL") = T0009TIME.ZEROtoSpace(WW_TA0003row("SHORTDISTANCE2TTL"))

            WW_TA0003tbl.Rows.Add(WW_TA0003row)
        Next


        IO_TBL = WW_TA0003tbl.Copy

        WW_TA0003tbl.Dispose()
        WW_TA0003tbl = Nothing

    End Sub

    ''' <summary>
    '''  GridView 明細行ダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_DBclick()

        '■ 処理準備
        '○ GridViewのダブルクリック行位置取得
        Dim WW_LINECNT As Integer                                   'GridViewのダブルクリック行位置
        Try
            Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT)
        Catch ex As Exception
            WW_LINECNT = 1
        End Try
        WF_REP_LINECNT.Value = WW_LINECNT

        '○ テーブルデータ 復元(Xmlファイルより復元)
        If Not Master.RecoverTable(TA0003ALL) Then Exit Sub

        '対象データ抽出(月合計入力））
        Dim WW_TA003TTLtbl As DataTable = TA0003ALL.Clone
        Dim WW_FILTER As String = ""
        WW_FILTER = ""
        WW_FILTER = WW_FILTER & "LINECNT  = '" & WF_GridDBclick.Text & "' and "
        WW_FILTER = WW_FILTER & "SELECT    = '1' and RECODEKBN = '2'"
        'データソート
        CS0026TblSort.TABLE = TA0003ALL
        CS0026TblSort.SORTING = "SELECT, STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
        CS0026TblSort.FILTER = WW_FILTER
        WW_TA003TTLtbl = CS0026TblSort.sort()

        For Each TA003row As DataRow In WW_TA003TTLtbl.Rows
            WF_STAFFCODETTL.Text = TA003row("STAFFCODE") '従業員
            WF_STAFFCODETTL_TEXT.Text = TA003row("STAFFNAMES") '従業員名称
            WF_HORGTTL.Text = TA003row("HORG") '従業員
            WF_HORGTTL_TEXT.Text = TA003row("HORGNAMES") '従業員名称


            Select Case work.WF_SEL_CAMPCODE.Text
                Case GRTA0003WRKINC.CONST_CAMP_ENEX  'エネックス

                    '月合計（編集）
                    WF_WORKNISSUTTL.Text = TA003row("WORKNISSUTTL") '所労
                    WF_NENKYUNISSUTTL.Text = TA003row("NENKYUNISSUTTL") '年休
                    WF_KYOTEIWEEKNISSUTTL.Text = TA003row("KYOTEIWEEKNISSUTTL") '協約週休
                    WF_SHOUKETUNISSUTTL.Text = TA003row("SHOUKETUNISSUTTL") '傷欠
                    WF_TOKUKYUNISSUTTL.Text = TA003row("TOKUKYUNISSUTTL") '特休
                    WF_WEEKNISSUTTL.Text = TA003row("WEEKNISSUTTL") '週休
                    WF_KUMIKETUNISSUTTL.Text = TA003row("KUMIKETUNISSUTTL") '組休
                    WF_CHIKOKSOTAINISSUTTL.Text = TA003row("CHIKOKSOTAINISSUTTL") '遅早
                    WF_DAIKYUNISSUTTL.Text = TA003row("DAIKYUNISSUTTL") '代休
                    WF_ETCKETUNISSUTTL.Text = TA003row("ETCKETUNISSUTTL") '他休
                    WF_STOCKNISSUTTL.Text = TA003row("STOCKNISSUTTL") 'ｽﾄｯｸ休暇
                    WF_NENSHINISSUTTL.Text = TA003row("NENSHINISSUTTL") '年始出勤日数
                    WF_SHUKCHOKNNISSUTTL.Text = TA003row("SHUKCHOKNNISSUTTL") '宿日直年始
                    WF_SHUKCHOKNISSUTTL.Text = TA003row("SHUKCHOKNISSUTTL") '宿日直通常
                    WF_PONPNISSUTTL.Text = TA003row("PONPNISSUTTL") 'ポンプ日数
                    WF_BULKNISSUTTL.Text = TA003row("BULKNISSUTTL") 'バルク日数
                    WF_TRAILERNISSUTTL.Text = TA003row("TRAILERNISSUTTL") 'トレーラ日数
                    WF_BKINMUKAISUTTL.Text = TA003row("BKINMUKAISUTTL") 'Ｂ勤務回数
                    WF_TOKSAAKAISUTTL.Text = TA003row("TOKSAAKAISUTTL") '特作A
                    WF_TOKSABKAISUTTL.Text = TA003row("TOKSABKAISUTTL") '特作B
                    WF_TOKSACKAISUTTL.Text = TA003row("TOKSACKAISUTTL") '特作C
                    WF_TENKOKAISUTTL.Text = TA003row("TENKOKAISUTTL") '点呼回数
                    WF_ORVERTIMETTL.Text = TA003row("ORVERTIMETTL") '平日残業
                    WF_WNIGHTTIMETTL.Text = TA003row("WNIGHTTIMETTL") '平日深夜
                    WF_TOKUSA1TIMETTL.Text = TA003row("TOKUSA1TIMETTL") '特作I
                    WF_HWORKTIMETTL.Text = TA003row("HWORKTIMETTL") '休日出勤
                    WF_HNIGHTTIMETTL.Text = TA003row("HNIGHTTIMETTL") '休日深夜
                    WF_HOANTIMETTL.Text = TA003row("HOANTIMETTL") '保安検査
                    WF_SWORKTIMETTL.Text = TA003row("SWORKTIMETTL") '日曜出勤
                    WF_SNIGHTTIMETTL.Text = TA003row("SNIGHTTIMETTL") '日曜深夜
                    WF_KOATUTIMETTL.Text = TA003row("KOATUTIMETTL") '高圧作業
                    WF_NIGHTTIMETTL.Text = TA003row("NIGHTTIMETTL") '所定深夜
                    WF_HAYADETIMETTL.Text = TA003row("HAYADETIMETTL") '早出補填

                    WF_UNLOADCNTTTL.Text = TA003row("UNLOADCNTTTL") '荷卸回数
                    WF_HAIDISTANCETTL.Text = Val(TA003row("HAIDISTANCETTL")).ToString("0") '走行距離
                    '一般
                    WF_UNLOADCNT_IPPAN1.Text = TA003row("UNLOADCNTTTL101")
                    WF_HAIDISTANCE_IPPAN1.Text = Val(TA003row("HAIDISTANCETTL101")).ToString("0")
                    WF_UNLOADCNT_IPPAN2.Text = TA003row("UNLOADCNTTTL201")
                    WF_HAIDISTANCE_IPPAN2.Text = Val(TA003row("HAIDISTANCETTL201")).ToString("0")
                    '潤滑油
                    WF_UNLOADCNT_JUN1.Text = TA003row("UNLOADCNTTTL102")
                    WF_HAIDISTANCE_JUN1.Text = Val(TA003row("HAIDISTANCETTL102")).ToString("0")
                    WF_UNLOADCNT_JUN2.Text = TA003row("UNLOADCNTTTL202")
                    WF_HAIDISTANCE_JUN2.Text = Val(TA003row("HAIDISTANCETTL202")).ToString("0")
                    'ＬＰ等
                    WF_UNLOADCNT_LPG1.Text = TA003row("UNLOADCNTTTL103")
                    WF_HAIDISTANCE_LPG1.Text = Val(TA003row("HAIDISTANCETTL103")).ToString("0")
                    WF_UNLOADCNT_LPG2.Text = TA003row("UNLOADCNTTTL203")
                    WF_HAIDISTANCE_LPG2.Text = Val(TA003row("HAIDISTANCETTL203")).ToString("0")
                    'ＬＮＧ
                    WF_UNLOADCNT_LNG1.Text = TA003row("UNLOADCNTTTL104")
                    WF_HAIDISTANCE_LNG1.Text = Val(TA003row("HAIDISTANCETTL104")).ToString("0")
                    WF_UNLOADCNT_LNG2.Text = TA003row("UNLOADCNTTTL204")
                    WF_HAIDISTANCE_LNG2.Text = Val(TA003row("HAIDISTANCETTL204")).ToString("0")
                    'コンテナ
                    WF_UNLOADCNT_CONT1.Text = TA003row("UNLOADCNTTTL105")
                    WF_HAIDISTANCE_CONT1.Text = Val(TA003row("HAIDISTANCETTL105")).ToString("0")
                    WF_UNLOADCNT_CONT2.Text = TA003row("UNLOADCNTTTL205")
                    WF_HAIDISTANCE_CONT2.Text = Val(TA003row("HAIDISTANCETTL205")).ToString("0")
                    '酸素
                    WF_UNLOADCNT_SANS1.Text = TA003row("UNLOADCNTTTL106")
                    WF_HAIDISTANCE_SANS1.Text = Val(TA003row("HAIDISTANCETTL106")).ToString("0")
                    WF_UNLOADCNT_SANS2.Text = TA003row("UNLOADCNTTTL206")
                    WF_HAIDISTANCE_SANS2.Text = Val(TA003row("HAIDISTANCETTL206")).ToString("0")
                    '窒素・ｱﾙｺﾞﾝ
                    WF_UNLOADCNT_CHIS1.Text = TA003row("UNLOADCNTTTL107")
                    WF_HAIDISTANCE_CHIS1.Text = Val(TA003row("HAIDISTANCETTL107")).ToString("0")
                    WF_UNLOADCNT_CHIS2.Text = TA003row("UNLOADCNTTTL207")
                    WF_HAIDISTANCE_CHIS2.Text = Val(TA003row("HAIDISTANCETTL207")).ToString("0")
                    'メタノール
                    WF_UNLOADCNT_META1.Text = TA003row("UNLOADCNTTTL108")
                    WF_HAIDISTANCE_META1.Text = Val(TA003row("HAIDISTANCETTL108")).ToString("0")
                    WF_UNLOADCNT_META2.Text = TA003row("UNLOADCNTTTL208")
                    WF_HAIDISTANCE_META2.Text = Val(TA003row("HAIDISTANCETTL208")).ToString("0")
                    'ラテックス
                    WF_UNLOADCNT_RATE1.Text = TA003row("UNLOADCNTTTL109")
                    WF_HAIDISTANCE_RATE1.Text = Val(TA003row("HAIDISTANCETTL109")).ToString("0")
                    WF_UNLOADCNT_RATE2.Text = TA003row("UNLOADCNTTTL209")
                    WF_HAIDISTANCE_RATE2.Text = Val(TA003row("HAIDISTANCETTL209")).ToString("0")
                    '水素
                    WF_UNLOADCNT_SUIS1.Text = TA003row("UNLOADCNTTTL110")
                    WF_HAIDISTANCE_SUIS1.Text = Val(TA003row("HAIDISTANCETTL110")).ToString("0")
                    WF_UNLOADCNT_SUIS2.Text = TA003row("UNLOADCNTTTL210")
                    WF_HAIDISTANCE_SUIS2.Text = Val(TA003row("HAIDISTANCETTL210")).ToString("0")
                Case GRTA0003WRKINC.CONST_CAMP_KNK  '近石
                    '月合計（編集）
                    WF_WORKNISSUTTL_KNK.Text = TA003row("WORKNISSUTTL") '所労
                    WF_NENKYUNISSUTTL_KNK.Text = TA003row("NENKYUNISSUTTL") '年休
                    WF_KYOTEIWEEKNISSUTTL_KNK.Text = TA003row("KYOTEIWEEKNISSUTTL") '協約週休
                    WF_SHOUKETUNISSUTTL_KNK.Text = TA003row("SHOUKETUNISSUTTL") '傷欠
                    WF_TOKUKYUNISSUTTL_KNK.Text = TA003row("TOKUKYUNISSUTTL") '特休
                    WF_WEEKNISSUTTL_KNK.Text = TA003row("WEEKNISSUTTL") '週休
                    WF_KUMIKETUNISSUTTL_KNK.Text = TA003row("KUMIKETUNISSUTTL") '組休
                    WF_CHIKOKSOTAINISSUTTL_KNK.Text = TA003row("CHIKOKSOTAINISSUTTL") '遅早
                    WF_DAIKYUNISSUTTL_KNK.Text = TA003row("DAIKYUNISSUTTL") '代休
                    WF_ETCKETUNISSUTTL_KNK.Text = TA003row("ETCKETUNISSUTTL") '他休
                    WF_STOCKNISSUTTL_KNK.Text = TA003row("STOCKNISSUTTL") 'ｽﾄｯｸ休暇
                    WF_HWORKNISSUTTL_KNK.Text = TA003row("HWORKNISSUTTL") '休日出勤日数
                    WF_NENSHINISSUTTL_KNK.Text = TA003row("NENSHINISSUTTL") '年始出勤日数
                    WF_SHUKCHOKNNISSUTTL_KNK.Text = TA003row("SHUKCHOKNNISSUTTL") '宿日直年始
                    WF_SHUKCHOKNISSUTTL_KNK.Text = TA003row("SHUKCHOKNISSUTTL") '宿日直通常
                    WF_ORVERTIMETTL_KNK.Text = TA003row("ORVERTIMETTL") '平日残業
                    WF_WNIGHTTIMETTL_KNK.Text = TA003row("WNIGHTTIMETTL") '平日深夜
                    WF_TOKUSA1TIMETTL_KNK.Text = TA003row("TOKUSA1TIMETTL") '特作I
                    WF_HWORKTIMETTL_KNK.Text = TA003row("HWORKTIMETTL") '休日出勤
                    WF_HNIGHTTIMETTL_KNK.Text = TA003row("HNIGHTTIMETTL") '休日深夜
                    WF_HDAIWORKTIMETTL_KNK.Text = TA003row("HDAIWORKTIMETTL") '代休出勤
                    WF_HDAINIGHTTIMETTL_KNK.Text = TA003row("HDAINIGHTTIMETTL") '代休深夜
                    WF_SWORKTIMETTL_KNK.Text = TA003row("SWORKTIMETTL") '日曜出勤
                    WF_SNIGHTTIMETTL_KNK.Text = TA003row("SNIGHTTIMETTL") '日曜深夜
                    WF_SDAIWORKTIMETTL_KNK.Text = TA003row("SDAIWORKTIMETTL") '日曜代休
                    WF_SDAINIGHTTIMETTL_KNK.Text = TA003row("SDAINIGHTTIMETTL") '日曜代休深夜
                    WF_NIGHTTIMETTL_KNK.Text = TA003row("NIGHTTIMETTL") '所定深夜
                    WF_WWORKTIMETTL_KNK.Text = TA003row("WWORKTIMETTL") '所定内計
                    WF_JYOMUTIMETTL_KNK.Text = TA003row("JYOMUTIMETTL") '乗務日計

                    WF_KAITENCNTTTL_KNK.Text = TA003row("UNLOADCNTTTL") '回転数
                    WF_HAIDISTANCETTL_KNK.Text = Val(TA003row("HAIDISTANCETTL")).ToString("0") '走行距離
                    '白油
                    WF_KAITENCNT_WHITE1_KNK.Text = TA003row("UNLOADCNTTTL101")
                    WF_HAIDISTANCE_WHITE1_KNK.Text = Val(TA003row("HAIDISTANCETTL101")).ToString("0")
                    WF_KAITENCNT_WHITE2_KNK.Text = TA003row("UNLOADCNTTTL201")
                    WF_HAIDISTANCE_WHITE2_KNK.Text = Val(TA003row("HAIDISTANCETTL201")).ToString("0")
                    '黒油
                    WF_KAITENCNT_BLACK1_KNK.Text = TA003row("UNLOADCNTTTL102")
                    WF_HAIDISTANCE_BLACK1_KNK.Text = Val(TA003row("HAIDISTANCETTL102")).ToString("0")
                    WF_KAITENCNT_BLACK2_KNK.Text = TA003row("UNLOADCNTTTL202")
                    WF_HAIDISTANCE_BLACK2_KNK.Text = Val(TA003row("HAIDISTANCETTL202")).ToString("0")
                    'ＬＰ等
                    WF_KAITENCNT_LPG1_KNK.Text = TA003row("UNLOADCNTTTL103")
                    WF_HAIDISTANCE_LPG1_KNK.Text = Val(TA003row("HAIDISTANCETTL103")).ToString("0")
                    WF_KAITENCNT_LPG2_KNK.Text = TA003row("UNLOADCNTTTL203")
                    WF_HAIDISTANCE_LPG2_KNK.Text = Val(TA003row("HAIDISTANCETTL203")).ToString("0")
                    'ＬＮＧ
                    WF_KAITENCNT_LNG1_KNK.Text = TA003row("UNLOADCNTTTL104")
                    WF_HAIDISTANCE_LNG1_KNK.Text = Val(TA003row("HAIDISTANCETTL104")).ToString("0")
                    WF_KAITENCNT_LNG2_KNK.Text = TA003row("UNLOADCNTTTL204")
                    WF_HAIDISTANCE_LNG2_KNK.Text = Val(TA003row("HAIDISTANCETTL204")).ToString("0")
                Case GRTA0003WRKINC.CONST_CAMP_NJS  'NJS
                    '月合計（編集）
                    WF_WORKNISSUTTL_NJS.Text = TA003row("WORKNISSUTTL") '所労
                    WF_NENKYUNISSUTTL_NJS.Text = TA003row("NENKYUNISSUTTL") '年休
                    WF_KYOTEIWEEKNISSUTTL_NJS.Text = TA003row("KYOTEIWEEKNISSUTTL") '協約週休
                    WF_SHOUKETUNISSUTTL_NJS.Text = TA003row("SHOUKETUNISSUTTL") '傷欠
                    WF_TOKUKYUNISSUTTL_NJS.Text = TA003row("TOKUKYUNISSUTTL") '特休
                    WF_WEEKNISSUTTL_NJS.Text = TA003row("WEEKNISSUTTL") '週休
                    WF_KUMIKETUNISSUTTL_NJS.Text = TA003row("KUMIKETUNISSUTTL") '組休
                    WF_CHIKOKSOTAINISSUTTL_NJS.Text = TA003row("CHIKOKSOTAINISSUTTL") '遅早
                    WF_DAIKYUNISSUTTL_NJS.Text = TA003row("DAIKYUNISSUTTL") '代休
                    WF_ETCKETUNISSUTTL_NJS.Text = TA003row("ETCKETUNISSUTTL") '他休
                    WF_STOCKNISSUTTL_NJS.Text = TA003row("STOCKNISSUTTL") 'ｽﾄｯｸ休暇
                    WF_NENMATUNISSUTTL_NJS.Text = TA003row("NENMATUNISSUTTL") '年末出勤日数
                    WF_NENSHINISSUTTL_NJS.Text = TA003row("NENSHINISSUTTL") '年始出勤日数
                    WF_SHACHUHAKNISSUTTL_NJS.Text = TA003row("SHACHUHAKNISSUTTL") '車中泊１日目
                    WF_ORVERTIMETTL_NJS.Text = TA003row("ORVERTIMETTL") '平日残業
                    WF_WNIGHTTIMETTL_NJS.Text = TA003row("WNIGHTTIMETTL") '平日深夜
                    WF_TOKUSA1TIMETTL_NJS.Text = TA003row("TOKUSA1TIMETTL") '特作I
                    WF_HWORKTIMETTL_NJS.Text = TA003row("HWORKTIMETTL") '休日出勤
                    WF_HNIGHTTIMETTL_NJS.Text = TA003row("HNIGHTTIMETTL") '休日深夜
                    WF_SWORKTIMETTL_NJS.Text = TA003row("SWORKTIMETTL") '日曜出勤
                    WF_SNIGHTTIMETTL_NJS.Text = TA003row("SNIGHTTIMETTL") '日曜深夜
                    WF_NIGHTTIMETTL_NJS.Text = TA003row("NIGHTTIMETTL") '所定深夜
                    WF_JIKYUSHATIMETTL_NJS.Text = TA003row("JIKYUSHATIMETTL") '時給者作業

                    WF_MODELDISTANCETTL_NJS.Text = Val(TA003row("MODELDISTANCETTL")).ToString("0") '走行距離
                    'LNG
                    WF_MODELDISTANCE_RATE1_NJS.Text = Val(TA003row("MODELDISTANCETTL109")).ToString("0")
                    WF_MODELDISTANCE_RATE2_NJS.Text = Val(TA003row("MODELDISTANCETTL209")).ToString("0")
                    'ラテックス
                    WF_MODELDISTANCE_LNG1_NJS.Text = Val(TA003row("MODELDISTANCETTL104")).ToString("0")
                    WF_MODELDISTANCE_LNG2_NJS.Text = Val(TA003row("MODELDISTANCETTL204")).ToString("0")


                Case GRTA0003WRKINC.CONST_CAMP_JKT 'ＪＫトランス
                    '月合計（編集）
                    WF_WORKNISSUTTL_JKT.Text = TA003row("WORKNISSUTTL") '所労
                    WF_NENKYUNISSUTTL_JKT.Text = TA003row("NENKYUNISSUTTL") '年休
                    WF_KYOTEIWEEKNISSUTTL_JKT.Text = TA003row("KYOTEIWEEKNISSUTTL") '協約週休
                    WF_SHOUKETUNISSUTTL_JKT.Text = TA003row("SHOUKETUNISSUTTL") '傷欠
                    WF_TOKUKYUNISSUTTL_JKT.Text = TA003row("TOKUKYUNISSUTTL") '特休
                    WF_KUMIKETUNISSUTTL_JKT.Text = TA003row("WEEKNISSUTTL") '休業日数
                    WF_CHIKOKSOTAINISSUTTL_JKT.Text = TA003row("CHIKOKSOTAINISSUTTL") '遅早
                    WF_DAIKYUNISSUTTL_JKT.Text = TA003row("DAIKYUNISSUTTL") '代休
                    WF_ETCKETUNISSUTTL_JKT.Text = TA003row("ETCKETUNISSUTTL") '他休
                    WF_STOCKNISSUTTL_JKT.Text = TA003row("STOCKNISSUTTL") 'ｽﾄｯｸ休暇
                    WF_NENSHINISSUTTL_JKT.Text = TA003row("NENSHINISSUTTL") '年始出勤日数
                    WF_SHUKCHOKNNISSUTTL_JKT.Text = TA003row("SHUKCHOKNNISSUTTL") '宿日直年始
                    WF_SHUKCHOKNISSUTTL_JKT.Text = TA003row("SHUKCHOKNISSUTTL") '宿日直通常
                    WF_SHACHUHAKNISSUTTL_JKT.Text = TA003row("SHACHUHAKNISSUTTL") '車中泊回数
                    WF_SENJYOCNTTTL_JKT.Text = TA003row("SENJYOCNTTTL") '洗浄回数
                    WF_ORVERTIMETTL_JKT.Text = TA003row("ORVERTIMETTL") '平日残業
                    WF_WNIGHTTIMETTL_JKT.Text = TA003row("WNIGHTTIMETTL") '平日深夜
                    WF_TOKUSA1TIMETTL_JKT.Text = TA003row("TOKUSA1TIMETTL") '特作I
                    WF_HWORKTIMETTL_JKT.Text = TA003row("HWORKTIMETTL") '休日出勤
                    WF_HNIGHTTIMETTL_JKT.Text = TA003row("HNIGHTTIMETTL") '休日深夜
                    WF_SWORKTIMETTL_JKT.Text = TA003row("SWORKTIMETTL") '日曜出勤
                    WF_SNIGHTTIMETTL_JKT.Text = TA003row("SNIGHTTIMETTL") '日曜深夜
                    WF_NIGHTTIMETTL_JKT.Text = TA003row("NIGHTTIMETTL") '所定深夜
                    WF_JIKYUSHATIMETTL_JKT.Text = TA003row("JIKYUSHATIMETTL") '時間給者所定内時間

                    WF_UNLOADADDCNT1TTL_JKT.Text = TA003row("UNLOADADDCNT1TTL") '卸危険品100
                    WF_UNLOADADDCNT2TTL_JKT.Text = TA003row("UNLOADADDCNT2TTL") '卸危険品200
                    WF_UNLOADADDCNT3TTL_JKT.Text = TA003row("UNLOADADDCNT3TTL") '卸危険品800
                    WF_LOADINGCNT1TTL_JKT.Text = TA003row("LOADINGCNT1TTL") '積危険品1000
                    WF_SHORTDISTANCE1TTL_JKT.Text = TA003row("SHORTDISTANCE1TTL") '短距離手当1000
                    WF_SHORTDISTANCE2TTL_JKT.Text = TA003row("SHORTDISTANCE2TTL") '短距離手当2000

                    WF_UNLOADCNTTTL_JKT.Text = TA003row("UNLOADCNTTTL") '荷卸回数
                    WF_HAIDISTANCETTL_JKT.Text = Val(TA003row("HAIDISTANCETTL")).ToString("0") '走行距離
                    '燃料油
                    WF_UNLOADCNT_IPPAN1_JKT.Text = TA003row("UNLOADCNTTTL101")
                    WF_HAIDISTANCE_IPPAN1_JKT.Text = Val(TA003row("HAIDISTANCETTL101")).ToString("0")
                    WF_UNLOADCNT_IPPAN2_JKT.Text = TA003row("UNLOADCNTTTL201")
                    WF_HAIDISTANCE_IPPAN2_JKT.Text = Val(TA003row("HAIDISTANCETTL201")).ToString("0")
                    '太陽油脂
                    WF_UNLOADCNT_TAIYO1_JKT.Text = TA003row("UNLOADCNTTTL102")
                    WF_HAIDISTANCE_TAIYO1_JKT.Text = Val(TA003row("HAIDISTANCETTL102")).ToString("0")
                    WF_UNLOADCNT_TAIYO2_JKT.Text = TA003row("UNLOADCNTTTL202")
                    WF_HAIDISTANCE_TAIYO2_JKT.Text = Val(TA003row("HAIDISTANCETTL202")).ToString("0")
                    'ｲﾝﾌｨﾆｱﾑ
                    WF_UNLOADCNT_INF1_JKT.Text = TA003row("UNLOADCNTTTL103")
                    WF_HAIDISTANCE_INF1_JKT.Text = Val(TA003row("HAIDISTANCETTL103")).ToString("0")
                    WF_UNLOADCNT_INF2_JKT.Text = TA003row("UNLOADCNTTTL203")
                    WF_HAIDISTANCE_INF2_JKT.Text = Val(TA003row("HAIDISTANCETTL203")).ToString("0")
                    '化成・潤滑
                    WF_UNLOADCNT_JUN1_JKT.Text = TA003row("UNLOADCNTTTL104")
                    WF_HAIDISTANCE_JUN1_JKT.Text = Val(TA003row("HAIDISTANCETTL104")).ToString("0")
                    WF_UNLOADCNT_JUN2_JKT.Text = TA003row("UNLOADCNTTTL204")
                    WF_HAIDISTANCE_JUN2_JKT.Text = Val(TA003row("HAIDISTANCETTL204")).ToString("0")
                    'コンテナ
                    WF_UNLOADCNT_CONT1_JKT.Text = TA003row("UNLOADCNTTTL105")
                    WF_HAIDISTANCE_CONT1_JKT.Text = Val(TA003row("HAIDISTANCETTL105")).ToString("0")
                    WF_UNLOADCNT_CONT2_JKT.Text = TA003row("UNLOADCNTTTL205")
                    WF_HAIDISTANCE_CONT2_JKT.Text = Val(TA003row("HAIDISTANCETTL205")).ToString("0")
                    '高圧
                    WF_UNLOADCNT_LPG1_JKT.Text = TA003row("UNLOADCNTTTL106")
                    WF_HAIDISTANCE_LPG1_JKT.Text = Val(TA003row("HAIDISTANCETTL106")).ToString("0")
                    WF_UNLOADCNT_LPG2_JKT.Text = TA003row("UNLOADCNTTTL206")
                    WF_HAIDISTANCE_LPG2_JKT.Text = Val(TA003row("HAIDISTANCETTL206")).ToString("0")

            End Select

        Next

        '〇明細の非表示
        work.WF_IsHideDetailBox.Text = "0"

        WW_TA003TTLtbl.Dispose()
        WW_TA003TTLtbl = Nothing
    End Sub

    ''' <summary>
    ''' TA0003ALLカラム設定
    ''' </summary>
    ''' <param name="IO_TBL">列登録対象テーブル</param>
    ''' <remarks></remarks>
    Protected Sub AddColumToTA0003Tbl(ByRef IO_TBL As DataTable)
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
        IO_TBL.Columns.Add("SHUKCHOKNNISSU", GetType(String))
        IO_TBL.Columns.Add("SHUKCHOKNNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("SHUKCHOKNNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("SHUKCHOKNISSU", GetType(String))
        IO_TBL.Columns.Add("SHUKCHOKNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("SHUKCHOKNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("TOKSAAKAISU", GetType(String))
        IO_TBL.Columns.Add("TOKSAAKAISUCHO", GetType(String))
        IO_TBL.Columns.Add("TOKSAAKAISUTTL", GetType(String))
        IO_TBL.Columns.Add("TOKSABKAISU", GetType(String))
        IO_TBL.Columns.Add("TOKSABKAISUCHO", GetType(String))
        IO_TBL.Columns.Add("TOKSABKAISUTTL", GetType(String))
        IO_TBL.Columns.Add("TOKSACKAISU", GetType(String))
        IO_TBL.Columns.Add("TOKSACKAISUCHO", GetType(String))
        IO_TBL.Columns.Add("TOKSACKAISUTTL", GetType(String))
        IO_TBL.Columns.Add("TENKOKAISU", GetType(String))
        IO_TBL.Columns.Add("TENKOKAISUCHO", GetType(String))
        IO_TBL.Columns.Add("TENKOKAISUTTL", GetType(String))
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
        IO_TBL.Columns.Add("KAITENCNT101", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTCHO101", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTTTL101", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCE101", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCECHO101", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCETTL101", GetType(String))

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
        IO_TBL.Columns.Add("KAITENCNT102", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTCHO102", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTTTL102", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCE102", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCECHO102", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCETTL102", GetType(String))

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
        IO_TBL.Columns.Add("KAITENCNT103", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTCHO103", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTTTL103", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCE103", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCECHO103", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCETTL103", GetType(String))

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
        IO_TBL.Columns.Add("KAITENCNT104", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTCHO104", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTTTL104", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCE104", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCECHO104", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCETTL104", GetType(String))

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
        IO_TBL.Columns.Add("KAITENCNT105", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTCHO105", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTTTL105", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCE105", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCECHO105", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCETTL105", GetType(String))

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
        IO_TBL.Columns.Add("KAITENCNT106", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTCHO106", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTTTL106", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCE106", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCECHO106", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCETTL106", GetType(String))

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
        IO_TBL.Columns.Add("KAITENCNT107", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTCHO107", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTTTL107", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCE107", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCECHO107", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCETTL107", GetType(String))

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
        IO_TBL.Columns.Add("KAITENCNT108", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTCHO108", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTTTL108", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCE108", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCECHO108", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCETTL108", GetType(String))

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
        IO_TBL.Columns.Add("KAITENCNT109", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTCHO109", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTTTL109", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCE109", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCECHO109", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCETTL109", GetType(String))

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
        IO_TBL.Columns.Add("KAITENCNT110", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTCHO110", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTTTL110", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCE110", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCECHO110", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCETTL110", GetType(String))

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
        IO_TBL.Columns.Add("KAITENCNT201", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTCHO201", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTTTL201", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCE201", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCECHO201", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCETTL201", GetType(String))

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
        IO_TBL.Columns.Add("KAITENCNT202", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTCHO202", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTTTL202", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCE202", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCECHO202", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCETTL202", GetType(String))

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
        IO_TBL.Columns.Add("KAITENCNT203", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTCHO203", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTTTL203", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCE203", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCECHO203", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCETTL203", GetType(String))

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
        IO_TBL.Columns.Add("KAITENCNT204", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTCHO204", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTTTL204", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCE204", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCECHO204", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCETTL204", GetType(String))

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
        IO_TBL.Columns.Add("KAITENCNT205", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTCHO205", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTTTL205", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCE205", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCECHO205", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCETTL205", GetType(String))

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
        IO_TBL.Columns.Add("KAITENCNT206", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTCHO206", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTTTL206", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCE206", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCECHO206", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCETTL206", GetType(String))

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
        IO_TBL.Columns.Add("KAITENCNT207", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTCHO207", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTTTL207", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCE207", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCECHO207", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCETTL207", GetType(String))

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
        IO_TBL.Columns.Add("KAITENCNT208", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTCHO208", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTTTL208", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCE208", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCECHO208", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCETTL208", GetType(String))

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
        IO_TBL.Columns.Add("KAITENCNT209", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTCHO209", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTTTL209", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCE209", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCECHO209", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCETTL209", GetType(String))

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
        IO_TBL.Columns.Add("KAITENCNT210", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTCHO210", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTTTL210", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCE210", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCECHO210", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCETTL210", GetType(String))

        IO_TBL.Columns.Add("DATAKBN", GetType(String))

        IO_TBL.Columns.Add("SHACHUHAKKBN", GetType(String))
        IO_TBL.Columns.Add("SHACHUHAKKBNNAMES", GetType(String))
        IO_TBL.Columns.Add("HAISOTIME", GetType(String))
        IO_TBL.Columns.Add("NENMATUNISSU", GetType(String))
        IO_TBL.Columns.Add("NENMATUNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("NENMATUNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("SHACHUHAKNISSU", GetType(String))
        IO_TBL.Columns.Add("SHACHUHAKNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("SHACHUHAKNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCE", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCECHO", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCETTL", GetType(String))
        IO_TBL.Columns.Add("JIKYUSHATIME", GetType(String))
        IO_TBL.Columns.Add("JIKYUSHATIMECHO", GetType(String))
        IO_TBL.Columns.Add("JIKYUSHATIMETTL", GetType(String))

        IO_TBL.Columns.Add("SDAIWORKTIME", GetType(String))
        IO_TBL.Columns.Add("SDAIWORKTIMECHO", GetType(String))
        IO_TBL.Columns.Add("SDAIWORKTIMETTL", GetType(String))
        IO_TBL.Columns.Add("SDAINIGHTTIME", GetType(String))
        IO_TBL.Columns.Add("SDAINIGHTTIMECHO", GetType(String))
        IO_TBL.Columns.Add("SDAINIGHTTIMETTL", GetType(String))
        IO_TBL.Columns.Add("HDAIWORKTIME", GetType(String))
        IO_TBL.Columns.Add("HDAIWORKTIMECHO", GetType(String))
        IO_TBL.Columns.Add("HDAIWORKTIMETTL", GetType(String))
        IO_TBL.Columns.Add("HDAINIGHTTIME", GetType(String))
        IO_TBL.Columns.Add("HDAINIGHTTIMECHO", GetType(String))
        IO_TBL.Columns.Add("HDAINIGHTTIMETTL", GetType(String))
        IO_TBL.Columns.Add("WWORKTIME", GetType(String))
        IO_TBL.Columns.Add("WWORKTIMECHO", GetType(String))
        IO_TBL.Columns.Add("WWORKTIMETTL", GetType(String))
        IO_TBL.Columns.Add("JYOMUTIME", GetType(String))
        IO_TBL.Columns.Add("JYOMUTIMECHO", GetType(String))
        IO_TBL.Columns.Add("JYOMUTIMETTL", GetType(String))
        IO_TBL.Columns.Add("HWORKNISSU", GetType(String))
        IO_TBL.Columns.Add("HWORKNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("HWORKNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("KAITENCNT", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTCHO", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTTTL", GetType(String))

        IO_TBL.Columns.Add("SENJYOCNT", GetType(String))
        IO_TBL.Columns.Add("SENJYOCNTCHO", GetType(String))
        IO_TBL.Columns.Add("SENJYOCNTTTL", GetType(String))
        IO_TBL.Columns.Add("UNLOADADDCNT1", GetType(String))
        IO_TBL.Columns.Add("UNLOADADDCNT1CHO", GetType(String))
        IO_TBL.Columns.Add("UNLOADADDCNT1TTL", GetType(String))
        IO_TBL.Columns.Add("UNLOADADDCNT2", GetType(String))
        IO_TBL.Columns.Add("UNLOADADDCNT2CHO", GetType(String))
        IO_TBL.Columns.Add("UNLOADADDCNT2TTL", GetType(String))
        IO_TBL.Columns.Add("UNLOADADDCNT3", GetType(String))
        IO_TBL.Columns.Add("UNLOADADDCNT3CHO", GetType(String))
        IO_TBL.Columns.Add("UNLOADADDCNT3TTL", GetType(String))
        IO_TBL.Columns.Add("UNLOADADDCNT4", GetType(String))
        IO_TBL.Columns.Add("UNLOADADDCNT4CHO", GetType(String))
        IO_TBL.Columns.Add("UNLOADADDCNT4TTL", GetType(String))
        IO_TBL.Columns.Add("LOADINGCNT1", GetType(String))
        IO_TBL.Columns.Add("LOADINGCNT1CHO", GetType(String))
        IO_TBL.Columns.Add("LOADINGCNT1TTL", GetType(String))
        IO_TBL.Columns.Add("LOADINGCNT2", GetType(String))
        IO_TBL.Columns.Add("LOADINGCNT2CHO", GetType(String))
        IO_TBL.Columns.Add("LOADINGCNT2TTL", GetType(String))
        IO_TBL.Columns.Add("SHORTDISTANCE1", GetType(String))
        IO_TBL.Columns.Add("SHORTDISTANCE1CHO", GetType(String))
        IO_TBL.Columns.Add("SHORTDISTANCE1TTL", GetType(String))
        IO_TBL.Columns.Add("SHORTDISTANCE2", GetType(String))
        IO_TBL.Columns.Add("SHORTDISTANCE2CHO", GetType(String))
        IO_TBL.Columns.Add("SHORTDISTANCE2TTL", GetType(String))
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
                Case "WORKKBN"
                    '作業区分名称
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "WORKKBN"))
                Case "DELFLG"
                    '削除フラグ名称
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DELFLG"))
                Case "STAFFCODE"
                    '乗務員名
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_STAFFCODE, I_VALUE, O_TEXT, O_RTN, work.GetStaffCodeList(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_HORG.Text, work.WF_SEL_TAISHOYM.Text))
                Case "CAMPCODE"
                    '会社名
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text))
                Case "ORG"
                    '出荷部署名
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateHORGParam(work.WF_SEL_CAMPCODE.Text, C_PERMISSION.INVALID, work.WF_SEL_TAISHOYM.Text & "/01"))
                Case "CREWKBN"
                    '実績登録区分名
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "CREWKBN"))
            End Select
        End If

    End Sub

    ''' <summary>
    ''' テーブルデータのコピー処理
    ''' </summary>
    ''' <param name="I_TBL">コピー元</param>
    ''' <param name="O_TBL">コピー先</param>
    ''' <remarks>ついでに１０分切替も行っている</remarks>
    Protected Sub CopyT7EditTbl(ByVal I_TBL As DataTable, ByRef O_TBL As DataTable)

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

            Orow("JIKYUSHATIME") = T0009TIME.RoundMinute(Orow("JIKYUSHATIME"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("JIKYUSHATIMECHO") = T0009TIME.RoundMinute(Orow("JIKYUSHATIMECHO"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("JIKYUSHATIMETTL") = T0009TIME.RoundMinute(Orow("JIKYUSHATIMETTL"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)

            Orow("SDAIWORKTIME") = T0009TIME.RoundMinute(Orow("SDAIWORKTIME"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("SDAIWORKTIMECHO") = T0009TIME.RoundMinute(Orow("SDAIWORKTIMECHO"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("SDAIWORKTIMETTL") = T0009TIME.RoundMinute(Orow("SDAIWORKTIMETTL"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("SDAINIGHTTIME") = T0009TIME.RoundMinute(Orow("SDAINIGHTTIME"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("SDAINIGHTTIMECHO") = T0009TIME.RoundMinute(Orow("SDAINIGHTTIMECHO"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("SDAINIGHTTIMETTL") = T0009TIME.RoundMinute(Orow("SDAINIGHTTIMETTL"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("HDAIWORKTIME") = T0009TIME.RoundMinute(Orow("HDAIWORKTIME"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("HDAIWORKTIMECHO") = T0009TIME.RoundMinute(Orow("HDAIWORKTIMECHO"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("HDAIWORKTIMETTL") = T0009TIME.RoundMinute(Orow("HDAIWORKTIMETTL"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("HDAINIGHTTIME") = T0009TIME.RoundMinute(Orow("HDAINIGHTTIME"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("HDAINIGHTTIMECHO") = T0009TIME.RoundMinute(Orow("HDAINIGHTTIMECHO"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("HDAINIGHTTIMETTL") = T0009TIME.RoundMinute(Orow("HDAINIGHTTIMETTL"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("WWORKTIME") = T0009TIME.RoundMinute(Orow("WWORKTIME"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("WWORKTIMECHO") = T0009TIME.RoundMinute(Orow("WWORKTIMECHO"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("WWORKTIMETTL") = T0009TIME.RoundMinute(Orow("WWORKTIMETTL"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("JYOMUTIME") = T0009TIME.RoundMinute(Orow("JYOMUTIME"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("JYOMUTIMECHO") = T0009TIME.RoundMinute(Orow("JYOMUTIMECHO"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
            Orow("JYOMUTIMETTL") = T0009TIME.RoundMinute(Orow("JYOMUTIMETTL"), WW_SPLIT_MINUITE, WW_ROUND_TYPE)
        Next
    End Sub

    ''' <summary>
    ''' 遷移時の引き渡しパラメータの取得
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub MapRefelence()

        '■■■ 選択画面の入力初期値設定 ■■■
        If Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.TA0003S Then                                                   '条件画面からの画面遷移

            If IsNothing(Master.MAPID) Then Master.MAPID = GRTA0003WRKINC.MAPID
            '○Grid情報保存先のファイル名
            Master.createXMLSaveFile()
        End If

        '※非表示にするCSSクラスをコントロールする
        Select Case work.WF_SEL_CAMPCODE.Text
            Case GRTA0003WRKINC.CONST_CAMP_ENEX
                detailbox.Attributes("class") = "Detail ENEX detailboxOnly"
            Case GRTA0003WRKINC.CONST_CAMP_KNK
                detailbox.Attributes("class") = "Detail KNK detailboxOnly"
            Case GRTA0003WRKINC.CONST_CAMP_NJS
                detailbox.Attributes("class") = "Detail NJS detailboxOnly"
            Case GRTA0003WRKINC.CONST_CAMP_JKT
                detailbox.Attributes("class") = "Detail JKT detailboxOnly"
        End Select

        '～2019年9月は、早出補填手当を表示しない
        If work.WF_SEL_TAISHOYM.Text <= "2019/09" Then
            WF_HAYADETIMETTL_LABEL.Visible = False
            WF_HAYADETIMETTL.Visible = False
        Else
            WF_HAYADETIMETTL_LABEL.Visible = True
            WF_HAYADETIMETTL.Visible = True
        End If

    End Sub

End Class





