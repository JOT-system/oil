Imports System.IO
Imports System.Net
Imports BASEDLL

Public Class GRCO0105HELP
    Inherits Page

    '○ 共通関数宣言(BASEDLL)
    Private CS0050SESSION As New CS0050SESSION          'セッション情報操作処理

    ''' <summary>
    ''' サーバ処理の遷移先
    ''' </summary>
    ''' <param name="sender">起動オブジェクト</param>
    ''' <param name="e">イベント発生時パラメータ</param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        If Not IsPostBack Then
            HELP_Display()
        Else
            '■■■ Detail PFD内容表示処理 ■■■
            If Not String.IsNullOrEmpty(WF_FileDisplay.Value) Then
                FileDisplay()
                WF_FileDisplay.Value = ""
            End If
        End If

    End Sub
    ''' <summary>
    ''' ヘルプファイル一覧表示  
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub HELP_Display()

        '■■■ 初期設定 ■■■
        Dim WW_Dir As String = ""

        Dim MC0105tbl As New DataTable
        Dim MC0105row As DataRow

        'MC0105tblテンポラリDB準備
        MC0105tbl.Clear()
        MC0105tbl.Columns.Clear()
        MC0105tbl.Clear()
        MC0105tbl.Columns.Add("WF_Rep_FILENAME", GetType(String))
        MC0105tbl.Columns.Add("WF_Rep_FILEPATH", GetType(String))

        '■■■ 画面編集 ■■■
        '○PDF格納ディレクトリ編集    
        WW_Dir = ""
        WW_Dir = WW_Dir & CS0050SESSION.UPLOAD_PATH
        If String.IsNullOrEmpty(Page.Request.QueryString("HELPid")) Then
            WW_Dir = WW_Dir & "\HELP\" &
                If(String.IsNullOrEmpty(CS0050SESSION.HELP_COMP), String.Empty, CS0050SESSION.HELP_COMP & "\") &
                CS0050SESSION.HELP_ID
            WF_USERID.Value = CS0050SESSION.USERID
        Else
            WW_Dir = WW_Dir & "\HELP\" &
                If(String.IsNullOrEmpty(Page.Request.QueryString("HELPcomp")), String.Empty, Page.Request.QueryString("HELPcomp") & "\") &
                Page.Request.QueryString("HELPid")
            WF_USERID.Value = Page.Request.QueryString("HELPuserid")
        End If

        '○指定HELPフォルダ内ファイル取得
        Dim WW_Files_dir As New List(Of String)     'Dir + FileName
        Dim WW_Files_name As New List(Of String)    'FileName
        Dim WW_HELPfiles As String()

        If Directory.Exists(WW_Dir) Then
            WW_HELPfiles = Directory.GetFiles(WW_Dir, "*", SearchOption.AllDirectories)
            For Each tempFile As String In WW_HELPfiles
                Dim WW_tempFile As String = tempFile
                Do
                    If InStr(WW_tempFile, "\") > 0 Then
                        'ファイル名編集
                        WW_tempFile = Mid(WW_tempFile, InStr(WW_tempFile, "\") + 1, 100)
                    End If

                    If InStr(WW_tempFile, "\") = 0 AndAlso WW_Files_name.IndexOf(WW_tempFile) = -1 Then
                        MC0105row = MC0105tbl.NewRow

                        'ファイル名格納
                        MC0105row("WF_Rep_FILENAME") = WW_tempFile
                        'ファイルパス格納
                        MC0105row("WF_Rep_FILEPATH") = tempFile
                        MC0105tbl.Rows.Add(MC0105row)
                        Exit Do
                    End If

                Loop Until InStr(WW_tempFile, "\") = 0
            Next
        End If

        'Repeaterバインド
        WF_DViewRepPDF.DataSource = MC0105tbl
        WF_DViewRepPDF.DataBind()

        '■■■ データ設定 ■■■
        'Repeaterへデータをセット
        For i As Integer = 0 To MC0105tbl.Rows.Count - 1

            'ファイル記号名称
            CType(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILENAME"), Label).Text = MC0105tbl.Rows(i)("WF_Rep_FILENAME")
            'FILEPATH
            CType(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILEPATH"), Label).Text = MC0105tbl.Rows(i)("WF_Rep_FILEPATH")

        Next

        '■■■ イベント設定 ■■■
        Dim WW_ATTR As String = ""
        For Each reitem As RepeaterItem In WF_DViewRepPDF.Items
            'ダブルクリック時コード検索イベント追加(ファイル名称用)
            WW_ATTR = "FileDisplay('" & CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Text & "');"
            CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Attributes.Remove("ondblclick")
            CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Attributes.Add("ondblclick", WW_ATTR)
        Next
    End Sub

    ''' <summary>
    ''' DetailPDF内容表示（Detail・PDFダブルクリック時（内容照会））
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub FileDisplay()

        '○ APサーバー名称取得(InParm無し)
        Dim WW_Dir As String = CS0050SESSION.UPLOAD_PATH & "\PRINTWORK\" & WF_USERID.Value

        For Each reitem As RepeaterItem In WF_DViewRepPDF.Items
            'ダブルクリック時コード検索イベント追加
            If CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Text = WF_FileDisplay.Value Then

                'ディレクトリが存在しない場合、作成する
                If Not Directory.Exists(WW_Dir) Then Directory.CreateDirectory(WW_Dir)

                'ダウンロードファイル送信準備
                File.Copy(CType(reitem.FindControl("WF_Rep_FILEPATH"), Label).Text,
                    WW_Dir & "\" & CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Text,
                    True)

                'ダウンロード処理へ遷移
                WF_HELPURL.Value = HttpContext.Current.Request.Url.Scheme & "://" & HttpContext.Current.Request.Url.Host & "/print/" & WF_USERID.Value & "/" & CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Text
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_DownLoad()", True)

                Exit For
            End If
        Next

    End Sub

End Class
