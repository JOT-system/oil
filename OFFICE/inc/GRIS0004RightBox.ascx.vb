Imports System.Drawing

Public Class GRIS0004RightBox
    Inherits UserControl
    ''' <summary>
    '''  レポート情報の取得固定文字列
    ''' </summary>
    Const C_FIX_VALUE_KEY As String = "REPORTID"
    ''' <summary>
    ''' 右リストボックスのタブインデックス
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum RIGHT_TAB_INDEX
        LS_ERROR_LIST
        LS_MEMO_LIST
    End Enum
    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <param name="I_MAPID">画面ID</param>
    ''' <param name="I_MAPVARI">画面の変数</param>
    ''' <param name="I_CAMPCODE">会社コード</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <param name="I_ORG_MODE">部署マスタモード</param>
    ''' <remarks></remarks>
    Public Sub Initialize(ByVal I_MAPID As String, ByVal I_MAPVARI As String, ByVal I_CAMPCODE As String, ByRef O_RTN As String, Optional ByVal I_ORG_MODE As Boolean = False)

        MAPID = I_MAPID
        MAPVARI = I_MAPVARI
        COMPCODE = I_CAMPCODE
        PROFID = C_DEFAULT_DATAKEY
        Initialize(O_RTN, I_ORG_MODE)
    End Sub
    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <param name="I_MAPID_MEMO">画面ID</param>
    ''' <param name="I_MAPID_REPORT">画面ID</param>
    ''' <param name="I_MAPVARI">画面の変数</param>
    ''' <param name="I_CAMPCODE">会社コード</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <param name="I_ORG_MODE">部署マスタモード</param>
    ''' <remarks></remarks>
    Public Sub Initialize(ByVal I_MAPID_MEMO As String, ByVal I_MAPID_REPORT As String, ByVal I_MAPVARI As String, ByVal I_CAMPCODE As String, ByRef O_RTN As String, Optional ByVal I_ORG_MODE As Boolean = False)

        MAPID_MEMO = I_MAPID_MEMO
        MAPID_REPORT = I_MAPID_REPORT
        MAPVARI = I_MAPVARI
        COMPCODE = I_CAMPCODE
        PROFID = C_DEFAULT_DATAKEY
        Initialize(O_RTN, I_ORG_MODE)
    End Sub
    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <param name="O_RTN">成功可否</param>
    ''' <param name="I_ORG_MODE">部署マスタモード</param>
    ''' <remarks></remarks>
    Public Sub Initialize(ByRef O_RTN As String, Optional ByVal I_ORG_MODE As Boolean = False)
        Dim CS0016ProfMValue As New CS0016ProfMValue        '変数情報取
        Dim GS0003MEMOget As New GS0003MEMOget              '画面RightBOXメモ情報取得
        Dim GS0005ReportList As New GS0005ReportList        '画面RightBOXレポートID取得

        '■対象日付
        If IsNothing(TARGETDATE) OrElse TARGETDATE = "" Then
            TARGETDATE = Date.Now.ToString("yyyy/MM/dd")
        End If

        '〇 RightBOX情報設定
        O_RTN = C_MESSAGE_NO.NORMAL
        RF_RIGHT_SW1.Checked = True

        '○メモ情報取得
        GS0003MEMOget.MAPID = MAPID_MEMO
        GS0003MEMOget.GS0003MEMOget()
        If isNormal(GS0003MEMOget.ERR) Then
            RF_MEMO.Text = GS0003MEMOget.MEMO
        Else
            O_RTN = GS0003MEMOget.ERR
            Exit Sub
        End If
        resetindex()
        '○部署マスタ系は自画面レイアウトなし
        If I_ORG_MODE Then
            Exit Sub
        End If

        '○レポートID情報
        Dim WW_REPORTID As ListBox = RF_REPORTID
        GS0005ReportList.COMPCODE = COMPCODE
        GS0005ReportList.PROFID = PROFID
        GS0005ReportList.MAPID = MAPID_REPORT
        GS0005ReportList.TARGETDATE = TARGETDATE
        GS0005ReportList.getList()
        If isNormal(GS0005ReportList.ERR) Then
            Try
                Dim REPORTOBJ As ListBox = CType(GS0005ReportList.REPORTOBJ, System.Web.UI.WebControls.ListBox)
                For Each item As ListItem In REPORTOBJ.Items
                    WW_REPORTID.Items.Add(New ListItem(item.Text, item.Value))
                Next
            Catch ex As Exception
            End Try
        Else
            O_RTN = GS0005ReportList.ERR
            Exit Sub
        End If

        '○レポートID変数取得
        CS0016ProfMValue.PROFID = PROFID
        CS0016ProfMValue.MAPID = MAPID_REPORT
        CS0016ProfMValue.CAMPCODE = COMPCODE
        CS0016ProfMValue.VARI = MAPVARI
        CS0016ProfMValue.FIELD = C_FIX_VALUE_KEY
        CS0016ProfMValue.TARGETDATE = TARGETDATE
        CS0016ProfMValue.getInfo()
        If isNormal(CS0016ProfMValue.ERR) Then
        Else
            O_RTN = CS0016ProfMValue.ERR
            Exit Sub
        End If

        '○レポートID(ListBox選択)
        WW_REPORTID.SelectedIndex = 0     '選択無しの場合、デフォルト
        For i As Integer = 0 To WW_REPORTID.Items.Count - 1
            If WW_REPORTID.Items(i).Value = CS0016ProfMValue.VALUE Then
                WW_REPORTID.SelectedIndex = i
                Exit For
            End If
        Next
    End Sub
    ''' <summary>
    ''' メモ欄とエラー情報の初期化
    ''' </summary>
    ''' <param name="O_RTN">成功可否</param>
    ''' <remarks></remarks>
    Public Sub InitMemoErrList(ByRef O_RTN As String)
        Dim GS0003MEMOget As New GS0003MEMOget              '画面RightBOXメモ情報取得

        '〇 RightBOX情報設定
        O_RTN = C_MESSAGE_NO.NORMAL
        RF_RIGHT_SW1.Checked = True

        '○メモ情報取得
        GS0003MEMOget.MAPID = MAPID_MEMO
        GS0003MEMOget.GS0003MEMOget()
        If isNormal(GS0003MEMOget.ERR) Then
            RF_MEMO.Text = GS0003MEMOget.MEMO
        Else
            O_RTN = GS0003MEMOget.ERR
            Exit Sub
        End If
        '〇エラーレポート初期化
        RF_ERR_REPORT.Text = ""
        resetindex()

    End Sub
    ''' <summary>
    ''' レポートID情報の初期化
    ''' </summary>
    ''' <param name="I_COMPCODE">会社コード</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <remarks></remarks>
    Public Sub InitReport(ByVal I_COMPCODE As String, ByRef O_RTN As String)
        Dim CS0016ProfMValue As New CS0016ProfMValue      '変数情報取
        Dim GS0005ReportList As New GS0005ReportList      '画面RightBOXレポートID取得

        COMPCODE = I_COMPCODE
        '〇 RightBOX情報設定
        O_RTN = C_MESSAGE_NO.NORMAL
        '○レポートID情報
        Dim WW_REPORTID As ListBox = CType(RF_REPORTID, System.Web.UI.WebControls.ListBox)

        GS0005ReportList.COMPCODE = COMPCODE
        GS0005ReportList.PROFID = PROFID
        GS0005ReportList.MAPID = MAPID_REPORT
        GS0005ReportList.getList()
        If isNormal(GS0005ReportList.ERR) Then
            Try
                Dim REPORTOBJ As ListBox = CType(GS0005ReportList.REPORTOBJ, System.Web.UI.WebControls.ListBox)
                For Each item As ListItem In REPORTOBJ.Items
                    WW_REPORTID.Items.Add(New ListItem(item.Text, item.Value))
                Next
            Catch ex As Exception
            End Try
        Else
            O_RTN = GS0005ReportList.ERR
            Exit Sub
        End If

        '○レポートID変数取得
        CS0016ProfMValue.PROFID = PROFID
        CS0016ProfMValue.MAPID = MAPID_REPORT
        CS0016ProfMValue.CAMPCODE = COMPCODE
        CS0016ProfMValue.VARI = MAPVARI
        CS0016ProfMValue.FIELD = C_FIX_VALUE_KEY
        CS0016ProfMValue.getInfo()
        If Not isNormal(CS0016ProfMValue.ERR) Then
            O_RTN = CS0016ProfMValue.ERR
            Exit Sub
        End If

        '○レポートID(ListBox選択)
        WW_REPORTID.SelectedIndex = 0     '選択無しの場合、デフォルト
        For i As Integer = 0 To WW_REPORTID.Items.Count - 1
            If WW_REPORTID.Items(i).Value = CS0016ProfMValue.VALUE Then
                WW_REPORTID.SelectedIndex = i
                Exit For
            End If
        Next

    End Sub
    ''' <summary>
    ''' メモ欄変更時処理
    ''' </summary>
    ''' <param name="I_USERID">更新ユーザID</param>
    ''' <param name="I_TERMID">更新端末ID</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <remarks></remarks>
    Public Sub Save(ByVal I_USERID As String, ByVal I_TERMID As String, ByRef O_RTN As String)
        Dim GS0004MEMOset As New GS0004MEMOset              '画面RightBOXメモ情報取得

        GS0004MEMOset.MAPID = MAPID_MEMO
        GS0004MEMOset.MEMO = RF_MEMO.Text
        GS0004MEMOset.USERID = I_USERID
        GS0004MEMOset.TERMID = I_TERMID
        GS0004MEMOset.GS0004MEMOset()
        O_RTN = GS0004MEMOset.ERR

    End Sub
    ''' <summary>
    ''' メモ＆エラーの選択を初期化する
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub ResetIndex()
        RF_RIGHTVIEW.ActiveViewIndex = RIGHT_TAB_INDEX.LS_ERROR_LIST
    End Sub
    ''' <summary>
    ''' メモ＆エラーの選択をする
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SelectIndex(ByVal index As RIGHT_TAB_INDEX)
        RF_RIGHTVIEW.ActiveViewIndex = index
    End Sub
    ''' <summary>
    ''' REPORTIDの値を取得する
    ''' </summary>
    ''' <returns>VIEWID</returns>
    ''' <remarks></remarks>
    Public Function GetReportId() As String
        GetReportId = RF_REPORTID.SelectedValue
    End Function
    ''' <summary>
    ''' エラーレポート欄取得
    ''' </summary>
    ''' <returns>レポート　</returns>
    ''' <remarks></remarks>
    Public Function GetErrorReport() As String
        Return RF_ERR_REPORT.Text
    End Function
    ''' <summary>
    ''' エラーレポート欄設定
    ''' </summary>
    ''' <param name="eReport">表示内容</param>
    ''' <remarks></remarks>
    Public Sub SetErrorReport(ByVal eReport As String)
        RF_ERR_REPORT.Text = eReport
    End Sub
    ''' <summary>
    ''' エラーレポート欄追記
    ''' </summary>
    ''' <param name="eReport">追記内容</param>
    ''' <remarks></remarks>
    Public Sub AddErrorReport(ByVal eReport As String)
        If RF_ERR_REPORT.Text <> "" Then
            RF_ERR_REPORT.Text = RF_ERR_REPORT.Text & ControlChars.NewLine
        End If
        RF_ERR_REPORT.Text = RF_ERR_REPORT.Text & eReport
    End Sub

#Region "<< Property Accessor >>"
    ''' <summary>
    ''' 結果画面ID
    ''' </summary>
    Public Property MAPID As String
        Get
            Return RF_MAPID_REPORT.Value
        End Get
        Set(value As String)
            RF_MAPID_REPORT.Value = value
            RF_MAPID_MEMO.Value = value
        End Set
    End Property
    ''' <summary>
    ''' 結果画面ID
    ''' </summary>
    Public Property MAPID_REPORT As String
        Get
            Return RF_MAPID_REPORT.Value
        End Get
        Set(value As String)
            RF_MAPID_REPORT.Value = value
        End Set
    End Property
    ''' <summary>
    ''' 結果画面ID
    ''' </summary>
    Public Property MAPID_MEMO As String
        Get
            Return RF_MAPID_MEMO.Value
        End Get
        Set(value As String)
            RF_MAPID_MEMO.Value = value
        End Set
    End Property
    ''' <summary>
    ''' 画面変数
    ''' </summary>
    Public Property MAPVARI As String
        Get
            Return RF_MAPVARI.Value
        End Get
        Set(value As String)
            RF_MAPVARI.Value = value
        End Set
    End Property
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    Public Property COMPCODE As String
        Get
            Return RF_COMPCODE.Value
        End Get
        Set(value As String)
            RF_COMPCODE.Value = value
        End Set
    End Property
    ''' <summary>
    ''' プロファイルID
    ''' </summary>
    Public Property PROFID As String
        Get
            Return RF_PROFID.Value
        End Get
        Set(value As String)
            RF_PROFID.Value = value
        End Set
    End Property
    ''' <summary>
    ''' 対象日付
    ''' </summary>
    Public Property TARGETDATE As String
        Get
            Return RF_TARGETDATE.Value
        End Get
        Set(value As String)
            RF_TARGETDATE.Value = value
        End Set
    End Property
#End Region

End Class