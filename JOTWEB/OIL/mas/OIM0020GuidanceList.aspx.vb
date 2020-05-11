Option Strict On
Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox
''' <summary>
''' ガイダンスマスタメンテナンス一覧画面クラス
''' </summary>
Public Class OIM0020GuidanceList
    Inherits System.Web.UI.Page
    '○ 検索結果格納Table
    Private OIM0020tbl As DataTable                                  '一覧格納用テーブル

    Private Const CONST_DISPROWCOUNT As Integer = 45                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 20                 'マウススクロール時稼働行数
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
    ''' ページロード時処理
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            If IsPostBack Then
                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    '○ 画面表示データ復元
                    Master.RecoverTable(OIM0020tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonINSERT"          '追加ボタン押下
                            WF_ButtonINSERT_Click()
                        Case "WF_ButtonCSV"             'ダウンロードボタン押下
                            WF_ButtonDownload_Click()
                        Case "WF_ButtonPrint"           '一覧印刷ボタン押下
                            WF_ButtonPrint_Click()
                        Case "WF_ButtonEND"             '戻るボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_ButtonFIRST"           '先頭頁ボタン押下
                            WF_ButtonFIRST_Click()
                        Case "WF_ButtonLAST"            '最終頁ボタン押下
                            WF_ButtonLAST_Click()
                        Case "WF_GridDBclick"           'GridViewダブルクリック
                            WF_Grid_DBClick()
                        Case "WF_MouseWheelUp"          'マウスホイール(Up)
                            WF_Grid_Scroll()
                        Case "WF_MouseWheelDown"        'マウスホイール(Down)
                            WF_Grid_Scroll()
                        Case "WF_RadioButonClick"       '(右ボックス)ラジオボタン選択
                            WF_RadioButton_Click()
                        Case "WF_MEMOChange"            '(右ボックス)メモ欄更新
                            WF_RIGHTBOX_Change()
                    End Select

                    '○ 一覧再表示処理
                    DisplayGrid()
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

        Finally
            '○ 格納Table Close
            If Not IsNothing(OIM0020tbl) Then
                OIM0020tbl.Clear()
                OIM0020tbl.Dispose()
                OIM0020tbl = Nothing
            End If

        End Try
    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIM0020WRKINC.MAPIDL
        '○HELP表示有無設定
        Master.dispHelp = False
        '○D&D有無設定
        Master.eventDrop = True
        '○Grid情報保存先のファイル名
        Master.CreateXMLSaveFile()

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

        '○ GridView初期設定
        GridViewInitialize()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0020S Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0020C Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        End If

    End Sub

    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        '○ 画面表示データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続
            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(OIM0020tbl)

        '〇 一覧の件数を取得
        Me.WF_ListCNT.Text = "件数：" + OIM0020tbl.Rows.Count.ToString()

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(OIM0020tbl)

        TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CInt(CS0013ProfView.SCROLLTYPE_ENUM.Both).ToString
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.CS0013ProfView()
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If

        '○ 先頭行に合わせる
        WF_GridPosition.Text = "1"

        TBLview.Dispose()
        TBLview = Nothing

    End Sub

    ''' <summary>
    ''' 画面表示データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MAPDataGet(ByVal SQLcon As SqlConnection)

        If IsNothing(OIM0020tbl) Then
            OIM0020tbl = New DataTable
        End If

        If OIM0020tbl.Columns.Count <> 0 Then
            OIM0020tbl.Columns.Clear()
        End If

        OIM0020tbl.Clear()
        '前画面より選択した対象フラグクラスを復元
        Dim flagList = work.DecodeDisplayFlags(work.WF_SEL_DISPFLAGS_LIST.Text)
        '対象フラグよりチェックしたもののみを抜き出す
        Dim selectedList = (From itm In flagList Where itm.Checked).ToList

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データをタンク車マスタから取得する
        Dim sotrOrderValue As String = "MG.GUIDANCENO DESC"
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT ")
        sqlStat.AppendFormat("      ROW_NUMBER() OVER(ORDER BY {0})  AS LINECNT", sotrOrderValue).AppendLine()
        sqlStat.AppendLine("       ,'' AS OPERATION")
        'sqlStat.AppendLine("     ,0  AS TIMSTP)
        sqlStat.AppendLine("       ,1  AS 'SELECT'")
        sqlStat.AppendLine("       ,0  AS HIDDEN")
        sqlStat.AppendLine("       ,MG.GUIDANCENO")
        sqlStat.AppendLine("       ,ISNULL(FORMAT(MG.FROMYMD, 'yyyy/MM/dd'), NULL) AS FROMYMD")
        sqlStat.AppendLine("       ,ISNULL(FORMAT(MG.ENDYMD,  'yyyy/MM/dd'), NULL) AS ENDYMD")
        sqlStat.AppendLine("       ,'<div class=""type' + MG.TYPE + '""></div>' AS DISPTYPE")
        sqlStat.AppendLine("       ,MG.TITTLE AS TITTLE")
        sqlStat.AppendLine("       ,MG.OUTFLG")
        sqlStat.AppendLine("       ,MG.INFLG1")
        sqlStat.AppendLine("       ,MG.INFLG2")
        sqlStat.AppendLine("       ,MG.INFLG3")
        sqlStat.AppendLine("       ,MG.INFLG4")
        sqlStat.AppendLine("       ,MG.INFLG5")
        sqlStat.AppendLine("       ,MG.INFLG6")
        sqlStat.AppendLine("       ,MG.INFLG7")
        sqlStat.AppendLine("       ,MG.INFLG8")
        sqlStat.AppendLine("       ,MG.INFLG9")
        sqlStat.AppendLine("       ,MG.INFLG10")
        sqlStat.AppendLine("       ,MG.INFLG11")

        sqlStat.AppendLine("       ,CASE WHEN MG.OUTFLG='1'  THEN '<div class=""checked""></div>' ELSE '' END AS DISPOUTFLG")
        sqlStat.AppendLine("       ,CASE WHEN MG.INFLG1='1'  THEN '<div class=""checked""></div>' ELSE '' END AS DISPINFLG1")
        sqlStat.AppendLine("       ,CASE WHEN MG.INFLG2='1'  THEN '<div class=""checked""></div>' ELSE '' END AS DISPINFLG2")
        sqlStat.AppendLine("       ,CASE WHEN MG.INFLG3='1'  THEN '<div class=""checked""></div>' ELSE '' END AS DISPINFLG3")
        sqlStat.AppendLine("       ,CASE WHEN MG.INFLG4='1'  THEN '<div class=""checked""></div>' ELSE '' END AS DISPINFLG4")
        sqlStat.AppendLine("       ,CASE WHEN MG.INFLG5='1'  THEN '<div class=""checked""></div>' ELSE '' END AS DISPINFLG5")
        sqlStat.AppendLine("       ,CASE WHEN MG.INFLG6='1'  THEN '<div class=""checked""></div>' ELSE '' END AS DISPINFLG6")
        sqlStat.AppendLine("       ,CASE WHEN MG.INFLG7='1'  THEN '<div class=""checked""></div>' ELSE '' END AS DISPINFLG7")
        sqlStat.AppendLine("       ,CASE WHEN MG.INFLG8='1'  THEN '<div class=""checked""></div>' ELSE '' END AS DISPINFLG8")
        sqlStat.AppendLine("       ,CASE WHEN MG.INFLG9='1'  THEN '<div class=""checked""></div>' ELSE '' END AS DISPINFLG9")
        sqlStat.AppendLine("       ,CASE WHEN MG.INFLG10='1' THEN '<div class=""checked""></div>' ELSE '' END AS DISPINFLG10")
        sqlStat.AppendLine("       ,CASE WHEN MG.INFLG11='1' THEN '<div class=""checked""></div>' ELSE '' END AS DISPINFLG11")
        sqlStat.AppendLine("       ,MG.NAIYOU")
        sqlStat.AppendLine("       ,MG.FAILE1")
        sqlStat.AppendLine("       ,MG.FAILE2")
        sqlStat.AppendLine("       ,MG.FAILE3")
        sqlStat.AppendLine("       ,MG.FAILE4")
        sqlStat.AppendLine("       ,MG.FAILE5")
        sqlStat.AppendLine("       ,CASE WHEN ISNULL(MG.FAILE1,'') <> '' THEN '<div class=""hasAttachment""></div>' ELSE '' END AS HASATTACHMENT")
        sqlStat.AppendLine("       ,format(MG.INITYMD,'yyyy/MM/dd HH:mm:ss.fff')    AS INITYMD")
        sqlStat.AppendLine("       ,format(MG.UPDYMD ,'yyyy/MM/dd HH:mm:ss.fff')    AS UPDYMD")
        sqlStat.AppendLine("  FROM OIL.OIM0020_GUIDANCE MG")
        sqlStat.AppendLine(" WHERE MG.DELFLG = @DELFLG_NO")
        If work.WF_SEL_FROMYMD.Text <> "" Then
            sqlStat.AppendLine("   AND MG.FROMYMD <= @FROMYMD")
        End If
        If work.WF_SEL_ENDYMD.Text <> "" Then
            sqlStat.AppendLine("   AND MG.ENDYMD >= @ENDYMD")
        End If
        If selectedList IsNot Nothing AndAlso selectedList.Count > 0 Then
            sqlStat.AppendLine("   AND (")
        End If
        Dim isFirst = True
        For Each selectedFlg In selectedList
            If Not isFirst Then
                sqlStat.Append("    OR ")
            Else
                sqlStat.Append("       ")
            End If
            isFirst = False
            sqlStat.AppendFormat("MG.{0} = @FLAGON", selectedFlg.FieldName).AppendLine()
        Next
        If selectedList IsNot Nothing AndAlso selectedList.Count > 0 Then
            sqlStat.AppendLine("   )")
        End If
        sqlStat.AppendFormat(" ORDER BY {0}", sotrOrderValue).AppendLine()

        Try
            Using sqlCmd As New SqlCommand(sqlStat.ToString, SQLcon)
                With sqlCmd.Parameters
                    .Add("@DELFLG_NO", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE

                    If work.WF_SEL_FROMYMD.Text <> "" Then
                        .Add("@FROMYMD", SqlDbType.Date).Value = work.WF_SEL_FROMYMD.Text
                    End If
                    If work.WF_SEL_ENDYMD.Text <> "" Then
                        .Add("@ENDYMD", SqlDbType.Date).Value = work.WF_SEL_ENDYMD.Text
                    End If
                    .Add("@FLAGON", SqlDbType.NVarChar).Value = "1"
                End With

                Using SQLdr As SqlDataReader = sqlCmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIM0020tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIM0020tbl.Load(SQLdr)
                End Using

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0020L SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0020L Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each OIM0020row As DataRow In OIM0020tbl.Rows
            If CInt(OIM0020row("HIDDEN")) = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                OIM0020row("SELECT") = WW_DataCNT
            End If
        Next

        '○ 表示LINECNT取得
        If WF_GridPosition.Text = "" Then
            WW_GridPosition = 1
        Else
            Try
                Integer.TryParse(WF_GridPosition.Text, WW_GridPosition)
            Catch ex As Exception
                WW_GridPosition = 1
            End Try
        End If

        '○ 表示格納位置決定

        '表示開始_格納位置決定(次頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelUp" Then
            If (WW_GridPosition + CONST_SCROLLCOUNT) <= WW_DataCNT Then
                WW_GridPosition += CONST_SCROLLCOUNT
            End If
        End If

        '表示開始_格納位置決定(前頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelDown" Then
            If (WW_GridPosition - CONST_SCROLLCOUNT) > 0 Then
                WW_GridPosition -= CONST_SCROLLCOUNT
            Else
                WW_GridPosition = 1
            End If
        End If

        '○ 画面(GridView)表示
        Dim TBLview As DataView = New DataView(OIM0020tbl)

        '○ ソート
        TBLview.Sort = "LINECNT"
        TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DISPROWCOUNT).ToString()

        '○ 一覧作成
        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CInt(CS0013ProfView.SCROLLTYPE_ENUM.Both).ToString
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.CS0013ProfView()

        '○ クリア
        If TBLview.Count = 0 Then
            WF_GridPosition.Text = "1"
        Else
            WF_GridPosition.Text = Convert.ToString(TBLview.Item(0)("SELECT"))
        End If

        TBLview.Dispose()
        TBLview = Nothing

    End Sub

    ''' <summary>
    ''' 追加ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonINSERT_Click()



        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(OIM0020tbl)

        WF_GridDBclick.Text = ""

        '○ 次ページ遷移
        Master.TransitionPage()

    End Sub

    ''' <summary>
    ''' 登録データ関連チェック
    ''' </summary>
    ''' <param name="O_RTNCODE"></param>
    ''' <remarks></remarks>
    Protected Sub RelatedCheck(ByRef O_RTNCODE As String)

        '○初期値設定
        O_RTNCODE = C_MESSAGE_NO.NORMAL

        Dim WW_LINEERR_SW As String = ""
        Dim WW_DUMMY As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""

    End Sub


    ''' <summary>
    ''' ﾀﾞｳﾝﾛｰﾄﾞ(Excel出力)ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDownload_Click()

        '○ 帳票出力
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                       '画面ID
        CS0030REPORT.REPORTID = rightview.GetReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "XLSX"                           '出力ファイル形式
        CS0030REPORT.TBLDATA = OIM0020tbl                        'データ参照  Table
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            If CS0030REPORT.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Else
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORT")
            End If
            Exit Sub
        End If

        '○ 別画面でExcelを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

    End Sub

    ''' <summary>
    ''' ﾀﾞｳﾝﾛｰﾄﾞ(PDF出力)・一覧印刷ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonPrint_Click()

        '○ 帳票出力
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                       '画面ID
        CS0030REPORT.REPORTID = rightview.GetReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "pdf"                            '出力ファイル形式
        CS0030REPORT.TBLDATA = OIM0020tbl                        'データ参照Table
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            If CS0030REPORT.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Else
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORT")
            End If
            Exit Sub
        End If

        '○ 別画面でPDFを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_PDFPrint();", True)

    End Sub


    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        Master.TransitionPrevPage()

    End Sub


    ''' <summary>
    ''' 先頭頁ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonFIRST_Click()

        '○ 先頭頁に移動
        WF_GridPosition.Text = "1"

    End Sub

    ''' <summary>
    ''' 最終頁ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonLAST_Click()

        '○ ソート
        Dim TBLview As New DataView(OIM0020tbl)
        TBLview.RowFilter = "HIDDEN = 0"

        '○ 最終頁に移動
        If TBLview.Count Mod 10 = 0 Then
            WF_GridPosition.Text = (TBLview.Count - (TBLview.Count Mod 10)).ToString
        Else
            WF_GridPosition.Text = (TBLview.Count - (TBLview.Count Mod 10) + 1).ToString
        End If

        TBLview.Dispose()
        TBLview = Nothing

    End Sub


    ' ******************************************************************************
    ' ***  一覧表示(GridView)関連操作                                            ***
    ' ******************************************************************************

    ''' <summary>
    ''' 一覧画面-明細行ダブルクリック時処理 (GridView ---> detailbox)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_DBClick()

        Dim WW_LINECNT As Integer = 0
        Dim WW_FIELD_OBJ As Object = Nothing
        Dim WW_VALUE As String = ""
        Dim WW_TEXT As String = ""

        '○ LINECNT取得
        Try
            Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT)
            WW_LINECNT -= 1
        Catch ex As Exception
            Exit Sub
        End Try

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(OIM0020tbl)

        WF_GridDBclick.Text = ""


        '登録画面ページへ遷移
        Master.TransitionPage()

    End Sub

    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_Scroll()

    End Sub


    ''' <summary>
    ''' RightBoxラジオボタン選択処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RadioButton_Click()

        'If Not String.IsNullOrEmpty(WF_RightViewChange.Value) Then
        '    Try
        '        Integer.TryParse(WF_RightViewChange.Value, WF_RightViewChange.Value)
        '    Catch ex As Exception
        '        Exit Sub
        '    End Try

        '    rightview.SelectIndex(WF_RightViewChange.Value)
        '    WF_RightViewChange.Value = ""
        'End If

    End Sub

    ''' <summary>
    ''' RightBoxメモ欄更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_Change()

        rightview.Save(Master.USERID, Master.USERTERMID, WW_DUMMY)

    End Sub

End Class