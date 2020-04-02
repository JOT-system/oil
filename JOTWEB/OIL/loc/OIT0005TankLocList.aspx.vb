Option Strict On
Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

Public Class OIT0005TankLocList
    Inherits System.Web.UI.Page
    '○ 検索結果格納Table
    Private OIT0005tbl As DataTable                                 '一覧格納用テーブル

    Private Const CONST_DISPROWCOUNT As Integer = 45                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 20                 'マウススクロール時稼働行数

    Private CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理
    Private CS0013ProfView As New CS0013ProfView                    'Tableオブジェクト展開

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            If IsPostBack Then
                'Dim dispDataObj As DemoDispDataClass
                'dispDataObj = GetThisScreenData(Me.frvSuggest, Me.repStockOilTypeItem)
                '○ 画面表示データ復元
                Master.RecoverTable(OIT0005tbl)

                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then

                    Select Case WF_ButtonClick.Value
                        Case "WF_MouseWheelUp"          'マウスホイール(Up)
                            WF_Grid_Scroll()
                        Case "WF_MouseWheelDown"        'マウスホイール(Down)
                            WF_Grid_Scroll()
                        Case "WF_ButtonEND"                 '戻るボタン押下
                            WF_ButtonEND_Click()
                    End Select
                End If
                DisplayGrid()
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
            If OIT0005tbl IsNot Nothing Then
                OIT0005tbl.Clear()
                OIT0005tbl.Dispose()
                OIT0005tbl = Nothing
            End If
        End Try

    End Sub
    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIT0005WRKINC.MAPIDL
        '○HELP表示有無設定
        Master.dispHelp = False
        '○D&D有無設定
        Master.eventDrop = True
        '○Grid情報保存先のファイル名
        Master.CreateXMLSaveFile()

        ''○初期値設定
        'WF_FIELD.Value = ""
        'WF_ButtonClick.Value = ""
        'WF_LeftboxOpen.Value = ""
        'WF_RightboxOpen.Value = ""
        'rightview.ResetIndex()
        'leftview.ActiveListBox()

        ''右Boxへの値設定
        'rightview.MAPID = Master.MAPID
        'rightview.MAPVARI = Master.MAPvariant
        'rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        'rightview.PROFID = Master.PROF_REPORT
        'rightview.Initialize(WW_DUMMY)

        '○ 画面の値設定
        WW_MAPValueSet()

    End Sub
    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIT0005L Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        End If
        '**********************************************
        '状況名称をヘッダー左下に設定
        '**********************************************
        Master.SetTitleLeftBottomText(work.WF_COND_DETAILTYPENAME.Text)
        '****************************************
        '生成したデータを画面に貼り付け
        '*************************
        GridViewInitialize()
    End Sub
    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        'リアルタイム性が重要な為、マスタからの一括更新はしない。単票で完結保存させる想定
        '○ 画面表示データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(OIT0005tbl)

        '〇 一覧の件数を取得
        'Me.WF_ListCNT.Text = "件数：" + OIT0005tbl.Rows.Count.ToString()

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(OIT0005tbl)

        TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CInt(CS0013ProfView.SCROLLTYPE_ENUM.None).ToString
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.OPERATIONCOLUMNWIDTHOPT = -1
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
    Protected Function MAPDataGet(ByVal SQLcon As SqlConnection) As Boolean
        If OIT0005tbl Is Nothing Then
            OIT0005tbl = New DataTable
        End If

        If OIT0005tbl.Columns.Count <> 0 Then
            OIT0005tbl.Columns.Clear()
        End If

        OIT0005tbl.Clear()
        Dim viewName As String = work.GetTankViewName(work.WF_COND_DETAILTYPE.Text)
        Dim sotrOrderValue As String = work.GetTankViewOrderByString(work.WF_COND_DETAILTYPE.Text)
        Dim sqlStat As New StringBuilder
        sqlStat.AppendFormat("SELECT ROW_NUMBER() OVER(ORDER BY {0})  AS LINECNT", sotrOrderValue).AppendLine()
        sqlStat.AppendLine("      ,'' AS OPERATION")
        'sqlStat.AppendLine("     ,0  AS TIMSTP)
        sqlStat.AppendLine("      ,1  AS 'SELECT'")
        sqlStat.AppendLine("      ,0  AS HIDDEN")
        sqlStat.AppendLine("      ,VTS.* ") 'ビューのフィールド追加しても動作可能なようにしている(削った場合は要稼働確認)
        sqlStat.AppendFormat("  FROM {0} VTS", viewName).AppendLine()
        sqlStat.AppendLine(" WHERE VTS.OFFICECODE = @OFFICECODE")
        sqlStat.AppendFormat(" ORDER BY {0}", sotrOrderValue).AppendLine()

        Try
            Using sqlCmd As New SqlCommand(sqlStat.ToString, SQLcon)
                With sqlCmd.Parameters
                    .Add("@OFFICECODE", SqlDbType.NVarChar).Value = work.WF_SEL_SALESOFFICECODE.Text
                End With
                Using SQLdr As SqlDataReader = sqlCmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0005tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next index

                    '○ テーブル検索結果をテーブル格納
                    OIT0005tbl.Load(SQLdr)
                End Using
            End Using
            Return True
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0005L SELECT", needsPopUp:=True)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:VIW0008 Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Return False
        End Try

    End Function
    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_Scroll()

    End Sub
    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        '○ 前画面遷移
        Master.TransitionPrevPage()

    End Sub
    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each OIT0005row As DataRow In OIT0005tbl.Rows
            If Convert.ToString(OIT0005row("HIDDEN")) = "0" Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                OIT0005row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(OIT0005tbl)

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
        CS0013ProfView.SCROLLTYPE = CInt(CS0013ProfView.SCROLLTYPE_ENUM.None).ToString
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.OPERATIONCOLUMNWIDTHOPT = -1
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
End Class