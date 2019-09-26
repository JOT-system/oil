Imports System
Imports System.IO
Imports System.Text
Imports System.Globalization
Imports System.Data.SqlClient
Imports Microsoft.VisualBasic
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Web.UI.Control
Imports System.Web.UI.Page
Imports System.Windows

Imports System.Drawing
Imports System.Net
Imports System.Data
''' <summary>
''' 共通処理
''' </summary>
''' <remarks></remarks>
Public Structure COMMON

    ''' <summary>
    ''' オンラインチェック処理
    ''' </summary>
    ''' <param name="I_USER">ユーザID</param>
    ''' <param name="O_RTN">オフライン判定</param>
    ''' <remarks></remarks>
    Public Sub ONLINEcheck(ByRef I_USER As String, ByRef O_RTN As String)

        Dim CS0008ONLINEstat As New BASEDLL.CS0008ONLINEstat        'ONLINE状態

        O_RTN = ""

        '〇 全画面共通チェック
        'セッションタイムアウト判定
        If I_USER = "" Then
            O_RTN = "OFFLINE"
            Exit Sub
        End If

        '〇 オンラインサービス判定 
        CS0008ONLINEstat.CS0008ONLINEstat()
        If BASEDLL.isNormal(CS0008ONLINEstat.ERR) Then
            If CS0008ONLINEstat.ONLINESW = 0 Then
                O_RTN = "OFFLINE"
            Else
                O_RTN = ""
            End If
        Else
            O_RTN = "OFFLINE"
        End If

    End Sub

    ''' <summary>
    ''' 画面表示ヘッダー部のタイトルを設定する
    ''' </summary>
    ''' <param name="I_MAP">現在の画面情報</param>
    ''' <param name="I_MAPID">現在の画面ID</param>
    ''' <param name="I_USERCOMP">会社コード</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <remarks></remarks>
    Public Sub MAPTITLEset(ByRef I_MAP As Page, ByRef I_MAPID As String, ByRef I_USERCOMP As String, ByRef O_RTN As String)
        Dim CS0017RETURNURLget As New BASEDLL.CS0017RETURNURLget    '画面戻先URL取得
        Dim GS0001CAMPget As New BASEDLL.GS0001CAMPget              '会社情報取得

        '初期化
        O_RTN = ""

        'ID、表題設定
        CType(I_MAP.FindControl("WF_TITLEID"), System.Web.UI.WebControls.Label).Text = "ID: " & I_MAPID

        '自画面MAPID・変数より名称を取得
        CS0017RETURNURLget.MAPID = I_MAPID
        CS0017RETURNURLget.VARI = CType(I_MAP.FindControl("WF_SEL_MAPvariant"), System.Web.UI.WebControls.TextBox).Text
        CS0017RETURNURLget.CS0017RETURNURLget()
        If CS0017RETURNURLget.ERR = "00000" Then
            CType(I_MAP.FindControl("WF_TITLETEXT"), System.Web.UI.WebControls.Label).Text = CS0017RETURNURLget.NAMES
        Else
            MESSAGEout(I_MAP, "A", CS0017RETURNURLget.ERR, "CS0017RETURNURLget")
            O_RTN = "ERR"
            Exit Sub
        End If

        '会社設定
        GS0001CAMPget.CAMPCODE = I_USERCOMP
        GS0001CAMPget.STYMD = Date.Now
        GS0001CAMPget.ENDYMD = Date.Now
        GS0001CAMPget.GS0001CAMPget()
        If GS0001CAMPget.ERR = "00000" Then
            CType(I_MAP.FindControl("WF_TITLECAMP"), System.Web.UI.WebControls.Label).Text = GS0001CAMPget.NAMES
        Else
            MESSAGEout(I_MAP, "A", GS0001CAMPget.ERR, "GS0001CAMPget")
            O_RTN = "ERR"
            Exit Sub
        End If

        '現在日付設定
        CType(I_MAP.FindControl("WF_TITLEDATE"), System.Web.UI.WebControls.Label).Text = DateTime.Now.ToString("yyyy年MM月dd日 HH時mm分")

        'メッセージクリア
        CType(I_MAP.FindControl("WF_MESSAGE"), System.Web.UI.WebControls.Label).ForeColor = Color.Black
        CType(I_MAP.FindControl("WF_MESSAGE"), System.Web.UI.WebControls.Label).Font.Bold = False

    End Sub

    ''' <summary>
    ''' RightBOXのListBox値設定(検索画面用）
    ''' </summary>
    ''' <param name="I_MAP">現在の画面情報</param>
    ''' <param name="I_MAPIDS">検索画面ID</param>
    ''' <param name="I_MAPID">結果画面ID</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <param name="I_NOVIEW">部署系画面のフラグ値</param>
    ''' <remarks></remarks>
    Sub RightBOXS_init(ByRef I_MAP As Page, ByRef I_MAPIDS As String, ByRef I_MAPID As String, ByRef O_RTN As String, Optional ByVal I_NOVIEW As String = Nothing)
        Dim CS0016VARIget As New BASEDLL.CS0016VARIget              '変数情報取
        Dim GS0003MEMOget As New BASEDLL.GS0003MEMOget              '画面RightBOXメモ情報取得
        Dim GS0005REPORTIDget As New BASEDLL.GS0005REPORTIDget      '画面RightBOXレポートID取得
        Dim GS0006VIEWIDget As New BASEDLL.GS0006VIEWIDget          '画面RightBOX用ビューID取得

        '〇 RightBOX情報設定
        O_RTN = ""

        '○メモ情報取得
        GS0003MEMOget.MAPID = I_MAPIDS
        GS0003MEMOget.GS0003MEMOget()
        If GS0003MEMOget.ERR = "00000" Then
            CType(I_MAP.FindControl("WF_MEMO"), System.Web.UI.WebControls.TextBox).Text = GS0003MEMOget.MEMO
        Else
            MESSAGEout(I_MAP, "A", GS0003MEMOget.ERR, "GS0003MEMOget")
            O_RTN = "ERR"
            Exit Sub
        End If

        '○部署マスタ系は自画面レイアウトなし
        If I_NOVIEW <> Nothing Then
            Exit Sub
        End If

        '○次画面レイアウト情報取得
        Dim WW_ListBOX As ListBox = CType(I_MAP.FindControl("WF_VIEW"), System.Web.UI.WebControls.ListBox)

        GS0006VIEWIDget.MAPID = I_MAPID
        GS0006VIEWIDget.VIEW = WW_ListBOX
        GS0006VIEWIDget.GS0006VIEWIDget()
        If GS0006VIEWIDget.ERR = "00000" Then
            For i As Integer = 0 To GS0006VIEWIDget.VIEW.Items.Count - 1
                WW_ListBOX.Items.Add(New ListItem(GS0006VIEWIDget.VIEW.Items(i).Text, GS0006VIEWIDget.VIEW.Items(i).Value))
            Next
        Else
            MESSAGEout(I_MAP, "A", GS0006VIEWIDget.ERR, "GS0006VIEWIDget")
            O_RTN = "ERR"
            Exit Sub
        End If

        '○ビューID変数検索
        CS0016VARIget.MAPID = I_MAPIDS
        CS0016VARIget.CAMPCODE = ""
        CS0016VARIget.VARI = CType(I_MAP.FindControl("WF_SEL_MAPvariant"), System.Web.UI.WebControls.TextBox).Text
        CS0016VARIget.FIELD = "VIEWID"
        CS0016VARIget.CS0016VARIget()
        If CS0016VARIget.ERR = "00000" Then
        Else
            MESSAGEout(I_MAP, "A", CS0016VARIget.ERR, "CS0016VARIget")
            O_RTN = "ERR"
            Exit Sub
        End If

        '○ListBox選択
        WW_ListBOX.SelectedIndex = 0     '選択無しの場合、デフォルト
        For i As Integer = 0 To WW_ListBOX.Items.Count - 1
            If WW_ListBOX.Items(i).Value = CS0016VARIget.VALUE Then
                WW_ListBOX.SelectedIndex = i
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' RightBOXのListBox値設定（メモ）検索画面用
    ''' </summary>
    ''' <param name="I_MAP">現在の画面情報</param>
    ''' <param name="I_MAPIDS">検索画面ID</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <remarks></remarks>
    Sub RightBOXSMemo_init(ByRef I_MAP As Page, ByRef I_MAPIDS As String, ByRef O_RTN As String)
        Dim GS0003MEMOget As New BASEDLL.GS0003MEMOget              '画面RightBOXメモ情報取得

        '〇 RightBOX情報設定
        O_RTN = ""

        '○メモ情報取得
        GS0003MEMOget.MAPID = I_MAPIDS
        GS0003MEMOget.GS0003MEMOget()
        If GS0003MEMOget.ERR = "00000" Then
            CType(I_MAP.FindControl("WF_MEMO"), System.Web.UI.WebControls.TextBox).Text = GS0003MEMOget.MEMO
        Else
            MESSAGEout(I_MAP, "A", GS0003MEMOget.ERR, "GS0003MEMOget")
            O_RTN = "ERR"
            Exit Sub
        End If

    End Sub

    ''' <summary>
    ''' RightBOXのListBox値設定（ビューレイアウト）(検索画面用）
    ''' </summary>
    ''' <param name="I_MAP">現在の画面情報</param>
    ''' <param name="I_MAPIDS">検索画面ID</param>
    ''' <param name="I_MAPID">結果画面ID</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <remarks></remarks>
    Sub RightBOXSMap_init(ByRef I_MAP As Page, ByRef I_MAPIDS As String, ByRef I_MAPID As String, ByRef O_RTN As String)
        Dim CS0016VARIget As New BASEDLL.CS0016VARIget              '変数情報取
        Dim GS0005REPORTIDget As New BASEDLL.GS0005REPORTIDget      '画面RightBOXレポートID取得
        Dim GS0006VIEWIDget As New BASEDLL.GS0006VIEWIDget          '画面RightBOX用ビューID取得

        '〇 RightBOX情報設定
        O_RTN = ""

        '○次画面レイアウト情報取得
        Dim WW_ListBOX As ListBox = CType(I_MAP.FindControl("WF_VIEW"), System.Web.UI.WebControls.ListBox)

        GS0006VIEWIDget.MAPID = I_MAPID
        GS0006VIEWIDget.VIEW = WW_ListBOX
        GS0006VIEWIDget.GS0006VIEWIDget()
        If GS0006VIEWIDget.ERR = "00000" Then
            For i As Integer = 0 To GS0006VIEWIDget.VIEW.Items.Count - 1
                WW_ListBOX.Items.Add(New ListItem(GS0006VIEWIDget.VIEW.Items(i).Text, GS0006VIEWIDget.VIEW.Items(i).Value))
            Next
        Else
            MESSAGEout(I_MAP, "A", GS0006VIEWIDget.ERR, "GS0006VIEWIDget")
            O_RTN = "ERR"
            Exit Sub
        End If

        '○ビューID変数検索
        CS0016VARIget.MAPID = I_MAPIDS
        CS0016VARIget.CAMPCODE = ""
        CS0016VARIget.VARI = CType(I_MAP.FindControl("WF_SEL_MAPvariant"), System.Web.UI.WebControls.TextBox).Text
        CS0016VARIget.FIELD = "VIEWID"
        CS0016VARIget.CS0016VARIget()
        If CS0016VARIget.ERR = "00000" Then
        Else
            MESSAGEout(I_MAP, "A", CS0016VARIget.ERR, "CS0016VARIget")
            O_RTN = "ERR"
            Exit Sub
        End If

        '○ListBox選択
        WW_ListBOX.SelectedIndex = 0     '選択無しの場合、デフォルト
        For i As Integer = 0 To WW_ListBOX.Items.Count - 1
            If WW_ListBOX.Items(i).Value = CS0016VARIget.VALUE Then
                WW_ListBOX.SelectedIndex = i
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' RightBOXのListBox値設定(結果画面用）
    ''' </summary>
    ''' <param name="I_MAP">現在の画面情報</param>
    ''' <param name="I_MAPID">結果画面ID</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <param name="I_NOVIEW">部署系画面のフラグ値</param>
    ''' <remarks></remarks>
    Sub RightBOX_init(ByRef I_MAP As Page, ByRef I_MAPID As String, ByRef O_RTN As String, Optional ByVal I_NOVIEW As String = Nothing)
        Dim CS0016VARIget As New BASEDLL.CS0016VARIget              '変数情報取
        Dim GS0003MEMOget As New BASEDLL.GS0003MEMOget              '画面RightBOXメモ情報取得
        Dim GS0005REPORTIDget As New BASEDLL.GS0005REPORTIDget      '画面RightBOXレポートID取得

        '〇 RightBOX情報設定
        O_RTN = ""
        CType(I_MAP.FindControl("WF_right_SW1"), System.Web.UI.WebControls.RadioButton).Checked = True

        '○メモ情報取得
        GS0003MEMOget.MAPID = I_MAPID
        GS0003MEMOget.GS0003MEMOget()
        If GS0003MEMOget.ERR = "00000" Then
            CType(I_MAP.FindControl("WF_MEMO"), System.Web.UI.WebControls.TextBox).Text = GS0003MEMOget.MEMO
        Else
            MESSAGEout(I_MAP, "A", GS0003MEMOget.ERR, "GS0003MEMOget")
            O_RTN = "ERR"
            Exit Sub
        End If

        '○部署マスタ系は自画面レイアウトなし
        If I_NOVIEW <> Nothing Then
            Exit Sub
        End If

        '○レポートID情報
        Dim WW_REPORTID As ListBox = CType(I_MAP.FindControl("WF_REPORTID"), System.Web.UI.WebControls.ListBox)

        GS0005REPORTIDget.MAPID = I_MAPID
        GS0005REPORTIDget.GS0005REPORTIDget()
        If GS0005REPORTIDget.ERR = "00000" Then
            Try
                Dim REPORTOBJ As ListBox = CType(GS0005REPORTIDget.REPORTOBJ, System.Web.UI.WebControls.ListBox)
                For i As Integer = 0 To REPORTOBJ.Items.Count - 1
                    WW_REPORTID.Items.Add(New ListItem(REPORTOBJ.Items(i).Text, REPORTOBJ.Items(i).Value))
                Next
            Catch ex As Exception
            End Try
        Else
            MESSAGEout(I_MAP, "A", GS0005REPORTIDget.ERR, "GS0005REPORTIDget")
            O_RTN = "ERR"
            Exit Sub
        End If

        '○レポートID変数取得
        CS0016VARIget.MAPID = I_MAPID
        CS0016VARIget.CAMPCODE = "Default"
        CS0016VARIget.VARI = CType(I_MAP.FindControl("WF_SEL_MAPvariant"), System.Web.UI.WebControls.TextBox).Text
        CS0016VARIget.FIELD = "REPORTID"
        CS0016VARIget.CS0016VARIget()
        If CS0016VARIget.ERR = "00000" Then
        Else
            MESSAGEout(I_MAP, "A", CS0016VARIget.ERR)
            O_RTN = "ERR"
            Exit Sub
        End If

        '○レポートID(ListBox選択)
        WW_REPORTID.SelectedIndex = 0     '選択無しの場合、デフォルト
        For i As Integer = 0 To WW_REPORTID.Items.Count - 1
            If WW_REPORTID.Items(i).Value = CS0016VARIget.VALUE Then
                WW_REPORTID.SelectedIndex = i
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' RightBOXのListBox値設定（メモ＆エラー）(結果画面用）
    ''' </summary>
    ''' <param name="I_MAP">現在の画面情報</param>
    ''' <param name="I_MAPID">結果画面ID</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <remarks></remarks>
    Sub RightBOXMemo_init(ByRef I_MAP As Page, ByRef I_MAPID As String, ByRef O_RTN As String)
        Dim GS0003MEMOget As New BASEDLL.GS0003MEMOget              '画面RightBOXメモ情報取得

        '〇 RightBOX情報設定
        O_RTN = ""
        CType(I_MAP.FindControl("WF_right_SW1"), System.Web.UI.WebControls.RadioButton).Checked = True

        '○メモ情報取得
        GS0003MEMOget.MAPID = I_MAPID
        GS0003MEMOget.GS0003MEMOget()
        If GS0003MEMOget.ERR = "00000" Then
            CType(I_MAP.FindControl("WF_MEMO"), System.Web.UI.WebControls.TextBox).Text = GS0003MEMOget.MEMO
        Else
            MESSAGEout(I_MAP, "A", GS0003MEMOget.ERR, "GS0003MEMOget")
            O_RTN = "ERR"
            Exit Sub
        End If

    End Sub

    ''' <summary>
    ''' RightBOXのListBox値設定（レポート）(結果画面用）
    ''' </summary>
    ''' <param name="I_MAP">現在の画面情報</param>
    ''' <param name="I_MAPID">結果画面ID</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <remarks></remarks>
    Sub RightBOXRep_init(ByRef I_MAP As Page, ByRef I_MAPID As String, ByRef O_RTN As String)
        Dim CS0016VARIget As New BASEDLL.CS0016VARIget              '変数情報取
        Dim GS0005REPORTIDget As New BASEDLL.GS0005REPORTIDget      '画面RightBOXレポートID取得

        '〇 RightBOX情報設定
        O_RTN = ""
        '○レポートID情報
        Dim WW_REPORTID As ListBox = CType(I_MAP.FindControl("WF_REPORTID"), System.Web.UI.WebControls.ListBox)

        GS0005REPORTIDget.MAPID = I_MAPID
        GS0005REPORTIDget.GS0005REPORTIDget()
        If GS0005REPORTIDget.ERR = "00000" Then
            Try
                Dim REPORTOBJ As ListBox = CType(GS0005REPORTIDget.REPORTOBJ, System.Web.UI.WebControls.ListBox)
                For i As Integer = 0 To REPORTOBJ.Items.Count - 1
                    WW_REPORTID.Items.Add(New ListItem(REPORTOBJ.Items(i).Text, REPORTOBJ.Items(i).Value))
                Next
            Catch ex As Exception
            End Try
        Else
            MESSAGEout(I_MAP, "A", GS0005REPORTIDget.ERR, "GS0005REPORTIDget")
            O_RTN = "ERR"
            Exit Sub
        End If

        '○レポートID変数取得
        CS0016VARIget.MAPID = I_MAPID
        CS0016VARIget.CAMPCODE = "Default"
        CS0016VARIget.VARI = CType(I_MAP.FindControl("WF_SEL_MAPvariant"), System.Web.UI.WebControls.TextBox).Text
        CS0016VARIget.FIELD = "REPORTID"
        CS0016VARIget.CS0016VARIget()
        If CS0016VARIget.ERR = "00000" Then
        Else
            MESSAGEout(I_MAP, "A", CS0016VARIget.ERR)
            O_RTN = "ERR"
            Exit Sub
        End If

        '○レポートID(ListBox選択)
        WW_REPORTID.SelectedIndex = 0     '選択無しの場合、デフォルト
        For i As Integer = 0 To WW_REPORTID.Items.Count - 1
            If WW_REPORTID.Items(i).Value = CS0016VARIget.VALUE Then
                WW_REPORTID.SelectedIndex = i
                Exit For
            End If
        Next

    End Sub


    ''' <summary>
    ''' RightBOXのListBox値更新（メモ）(結果画面用）
    ''' </summary>
    ''' <param name="I_MAP">現在の画面情報</param>
    ''' <param name="I_MAPID">結果画面ID</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <remarks></remarks>
    Sub MEMO_Changed(ByRef I_MAP As Page, ByRef I_MAPID As String, ByRef O_RTN As String)
        Dim GS0004MEMOset As New BASEDLL.GS0004MEMOset              '画面RightBOXメモ情報取得

        GS0004MEMOset.MAPID = I_MAPID
        GS0004MEMOset.MEMO = CType(I_MAP.FindControl("WF_MEMO"), System.Web.UI.WebControls.TextBox).Text
        GS0004MEMOset.GS0004MEMOset()
        If GS0004MEMOset.ERR = "00000" Then
        Else
            MESSAGEout(I_MAP, "A", GS0004MEMOset.ERR, "GS0004MEMOset")
            O_RTN = "ERR"
            Exit Sub
        End If

    End Sub

    ''' <summary>
    ''' メッセージ表示（フッター）
    ''' </summary>
    ''' <param name="I_MAP">現在の画面情報</param>
    ''' <param name="I_NAEIW">エラータイプ</param>
    ''' <param name="IO_MESSAGENO">メッセージ番号</param>
    ''' <param name="I_PARA01">パラメータ１</param>
    ''' <param name="I_PARA02">パラメータ２</param>
    ''' <remarks></remarks>
    Sub MESSAGEout(ByRef I_MAP As Page, ByVal I_NAEIW As String, ByRef IO_MESSAGENO As String, Optional ByVal I_PARA01 As String = Nothing, Optional ByVal I_PARA02 As String = Nothing)
        Dim CS0009MESSAGEout As New BASEDLL.CS0009MESSAGEout        'Message out

        CS0009MESSAGEout.MESSAGENO = IO_MESSAGENO
        CS0009MESSAGEout.NAEIW = I_NAEIW
        CS0009MESSAGEout.MESSAGEBOX = CType(I_MAP.FindControl("WF_MESSAGE"), System.Web.UI.WebControls.Label)
        If I_PARA01 = Nothing Then
        Else
            CS0009MESSAGEout.PARA01 = I_PARA01
        End If
        If I_PARA02 = Nothing Then
        Else
            CS0009MESSAGEout.PARA02 = I_PARA02
        End If
        CS0009MESSAGEout.CS0009MESSAGEout()

        If CS0009MESSAGEout.ERR = "00000" Then
            CType(I_MAP.FindControl("WF_MESSAGE"), System.Web.UI.WebControls.Label).Text = CS0009MESSAGEout.MESSAGEBOX.text
        End If

    End Sub

    ''' <summary>
    ''' 使用禁止文字を除去する
    ''' </summary>
    ''' <param name="IO_CHARin">除去対象文字列</param>
    ''' <remarks></remarks>
    Sub CHARstr(ByRef IO_CHARin As String)
        Dim CS0010CHARstr As New BASEDLL.CS0010CHARget              '例外文字排除 String Get

        '○ 入力文字置き換え ※画面PassWord内の使用禁止文字排除
        CS0010CHARstr.CHARin = IO_CHARin
        CS0010CHARstr.CS0010CHARget()
        IO_CHARin = CS0010CHARstr.CHAROUT

    End Sub

    ''' <summary>
    ''' 単項目チェック処理
    ''' </summary>
    ''' <param name="I_FIELD">フィールド名</param>
    ''' <param name="IO_VALUE">チェック対象の値</param>
    ''' <param name="O_MESSAGENO">エラーメッセージ</param>
    ''' <param name="O_CHECKREPORT">エラー内容</param>
    ''' <param name="I_MAPID">画面ID</param>
    ''' <remarks></remarks>
    Sub ITEMCheckSub(ByRef I_FIELD As String, ByRef IO_VALUE As String, ByRef O_MESSAGENO As String, ByRef O_CHECKREPORT As String, ByRef I_MAPID As String)
        Dim CS0024FCHECK As New BASEDLL.CS0024FCHECK                '項目チェック

        CS0024FCHECK.CAMPCODE = "Default"                           '会社コード
        CS0024FCHECK.MAPID = I_MAPID                                '画面ID
        CS0024FCHECK.FIELD = I_FIELD                                '項目名
        CS0024FCHECK.VALUE = IO_VALUE                               '値
        CS0024FCHECK.CS0024FCHECK()

        O_MESSAGENO = CS0024FCHECK.ERR
        O_CHECKREPORT = CS0024FCHECK.CHECKREPORT

        If CS0024FCHECK.ERR = "00000" Then
            IO_VALUE = CS0024FCHECK.VALUEOUT
        End If

    End Sub

    ''' <summary>
    ''' 単項目チェック処理
    ''' </summary>
    ''' <param name="I_FIELD">フィールド名</param>
    ''' <param name="IO_VALUE">チェック対象の値</param>
    ''' <param name="O_MESSAGENO">エラーメッセージ</param>
    ''' <param name="O_CHECKREPORT">エラー内容</param>
    ''' <param name="I_MAPID">画面ID</param>
    ''' <param name="I_TBL" >チェック用DATGAFIELDテーブル</param>
    ''' <remarks></remarks>
    Sub ITEMCheckSub2(ByRef I_FIELD As String, ByRef IO_VALUE As String, ByRef O_MESSAGENO As String, ByRef O_CHECKREPORT As String, ByRef I_MAPID As String, ByRef I_TBL As DataTable)
        Dim CS0036FCHECK As New BASEDLL.CS0036FCHECK                '項目チェック

        CS0036FCHECK.CAMPCODE = "Default"                           '会社コード
        CS0036FCHECK.MAPID = I_MAPID                                '画面ID
        CS0036FCHECK.FIELD = I_FIELD                                '項目名
        CS0036FCHECK.VALUE = IO_VALUE                               '値
        CS0036FCHECK.TBL = I_TBL                                    'S0013_DATAFIELDテーブル
        CS0036FCHECK.CS0036FCHECK()

        O_MESSAGENO = CS0036FCHECK.ERR
        O_CHECKREPORT = CS0036FCHECK.CHECKREPORT

        If CS0036FCHECK.ERR = "00000" Then
            IO_VALUE = CS0036FCHECK.VALUEOUT
        End If

    End Sub

    ''' <summary>
    ''' ヘルプ画面表示
    ''' </summary>
    ''' <param name="I_MAP">現在の画面情報</param>
    ''' <param name="I_MAPID">表示したいヘルプの画面ID</param>
    ''' <remarks></remarks>
    Sub HELPDisplay(ByRef I_MAP As Page, ByRef I_MAPID As String)

        '■■■ 画面遷移実行 ■■■
        Dim WW_SCRIPT As String = "<script language=""javascript"">window.open('/CO0105HELP.aspx', '_blank', 'menubar=1, location=1, status=1, scrollbars=1, resizable=1');</script>"

        HttpContext.Current.Session("HELPid") = I_MAPID
        I_MAP.ClientScript.RegisterStartupScript(I_MAP.GetType, "OpenNewWindow", WW_SCRIPT)

    End Sub

    '■ 画面表示データ保存
    ''' <summary>
    ''' 取得データの保存
    ''' </summary>
    ''' <param name="I_MAP">画面ID</param>
    ''' <param name="IO_TABLE">保存するデータ情報</param>
    ''' <param name="I_DIR">保存先</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <param name="I_SAVEHEADER">ヘッダー部を保存するかの判定区分　１：保存</param>
    ''' <remarks></remarks>
    Public Sub TABLEsave(ByRef I_MAP As Page, ByRef IO_TABLE As DataTable, ByRef I_DIR As String, ByRef O_RTN As String, Optional ByVal I_SAVEHEADER As Integer = -1)
        Dim CS0031TABLEsave As New BASEDLL.CS0031TABLEsave          'TableData(Grid)退避

        O_RTN = ""

        CS0031TABLEsave.FILEDIR = I_DIR
        CS0031TABLEsave.TBLDATA = IO_TABLE
        If (I_SAVEHEADER >= 0) Then
            CS0031TABLEsave.SAVEMODE = I_SAVEHEADER
        End If
        CS0031TABLEsave.CS0031TABLEsave()
        If CS0031TABLEsave.ERR <> "00000" Then
            O_RTN = "ERR"
            MESSAGEout(I_MAP, BASEDLL.C_MESSAGE_TYPE.ABORT, CS0031TABLEsave.ERR, "CS0031TABLEsave")
            Exit Sub
        End If

    End Sub
    ''' <summary>
    ''' 画面表示データ復元
    ''' </summary>
    ''' <param name="I_MAP"></param>
    ''' <param name="IO_TABLE"></param>
    ''' <param name="I_DIR"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Public Sub TABLERecover(ByRef I_MAP As Page, ByRef IO_TABLE As DataTable, ByRef I_DIR As String, ByRef O_RTN As String, Optional ByVal I_RECOVERMODE As Integer = -1)
        Dim CS0032TABLERecover As New BASEDLL.CS0032TABLERecover    'TableData(Grid)復元

        O_RTN = ""

        CS0032TABLERecover.FILEDIR = I_DIR
        CS0032TABLERecover.TBLDATA = IO_TABLE
        If (I_RECOVERMODE >= 0) Then
            CS0032TABLERecover.RECOVERMODE = I_RECOVERMODE
        End If
        CS0032TABLERecover.CS0032TABLERecover()
        If BASEDLL.isNormal(CS0032TABLERecover.ERR) Then
            IO_TABLE = CS0032TABLERecover.OUTTBL
        Else
            O_RTN = "ERR"
            MESSAGEout(I_MAP, BASEDLL.C_MESSAGE_TYPE.ABORT, CS0032TABLERecover.ERR, "CS0032TABLERecover")
        End If

    End Sub

    '■ 変数取得
    Sub VARIget(ByVal I_FIELD As String, ByRef O_VALUE As String, ByRef I_MAPvariant As String, ByRef I_MAPID As String)
        Dim CS0016VARIget As New BASEDLL.CS0016VARIget              '変数情報取

        '○変数設定処理 
        CS0016VARIget.MAPID = I_MAPID
        CS0016VARIget.CAMPCODE = ""
        CS0016VARIget.VARI = I_MAPvariant
        CS0016VARIget.FIELD = I_FIELD
        CS0016VARIget.CS0016VARIget()
        If CS0016VARIget.ERR = "00000" Then
            O_VALUE = CS0016VARIget.VALUE
        Else
            O_VALUE = Nothing
        End If

    End Sub

    '■ ソート処理
    Sub DATATBL_SORT(ByRef IN_TBL As DataTable,
                        ByVal IN_SORTstr As String,
                        ByVal IN_FILTERstr As String,
                        Optional ByRef OUT_TBL As DataTable = Nothing)

        Dim WW_TBLview As DataView
        WW_TBLview = New DataView(IN_TBL)
        WW_TBLview.Sort = IN_SORTstr
        If IN_FILTERstr <> "" Then
            WW_TBLview.RowFilter = IN_FILTERstr
        End If

        If OUT_TBL Is Nothing Then
            IN_TBL = WW_TBLview.ToTable
        Else
            OUT_TBL = WW_TBLview.ToTable
        End If

        WW_TBLview.Dispose()
        WW_TBLview = Nothing

    End Sub


End Structure




