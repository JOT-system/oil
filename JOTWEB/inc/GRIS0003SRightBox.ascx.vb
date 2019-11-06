Imports System.Drawing

Public Class GRIS0003SRightBox
    Inherits UserControl
    ''' <summary>
    '''  レポート情報の取得固定文字列
    ''' </summary>
    Private Const C_FIX_VALUE_KEY As String = "VIEWID"
    Private Const C_HEAD_VALUE_KEY As String = "VIEWID_HEAD"
    Private Const C_DTL_VALUE_KEY As String = "VIEWID_DTL"

    Private CS0016ProfMValue As New CS0016ProfMValue        '変数情報取
    Private GS0003MEMOget As New GS0003MEMOget              '画面RightBOXメモ情報取得
    Private GS0006ViewList As New GS0006ViewList            '画面RightBOX用ビューID取得
    ''' <summary>
    ''' 右リストボックスの初期化処理
    ''' </summary>
    ''' <param name="I_VIEW_TITLE">検索結果一覧のタイトル名</param>
    ''' <param name="I_MAPIDS">検索画面の画面ID</param>
    ''' <param name="I_MAPID">結果画面の画面ID</param>
    ''' <param name="I_MAPVARI">画面の設定値</param>
    ''' <param name="O_RTN">結果可否</param>
    ''' <param name="I_ORG_MODE">部署マスタ画面フラグ</param>
    ''' <remarks></remarks>
    Public Sub Initialize(ByVal I_VIEW_TITLE As String, ByVal I_MAPIDS As String, ByVal I_MAPID As String, ByVal I_MAPVARI As String, ByRef O_RTN As String, Optional ByVal I_ORG_MODE As Boolean = False)

        MAPID = I_MAPID
        MAPVARI = I_MAPVARI
        MAPIDS = I_MAPIDS
        MAPID_DTL = String.Empty
        PROFID = C_DEFAULT_DATAKEY
        COMPCODE = String.Empty
        Initialize(I_VIEW_TITLE, O_RTN, I_ORG_MODE)
    End Sub
    ''' <summary>
    ''' 右リストボックスの初期化処理
    ''' </summary>
    ''' <param name="I_VIEW_TITLE">検索結果一覧のタイトル名</param>
    ''' <param name="I_VIEW_DTL_TITLE">検索結果(明細)一覧のタイトル名</param>
    ''' <param name="I_MAPIDS">検索画面の画面ID</param>
    ''' <param name="I_MAPID">結果画面の画面ID</param>
    ''' <param name="I_MAPID_DTL">結果画面(明細)の画面ID</param>
    ''' <param name="I_MAPVARI">画面の設定値</param>
    ''' <param name="O_RTN">結果可否</param>
    ''' <param name="I_ORG_MODE">部署マスタ画面フラグ</param>
    ''' <remarks></remarks>
    Public Sub Initialize(ByVal I_VIEW_TITLE As String, ByVal I_VIEW_DTL_TITLE As String, ByVal I_MAPIDS As String, ByVal I_MAPID As String, ByVal I_MAPID_DTL As String, ByVal I_MAPVARI As String, ByRef O_RTN As String, Optional ByVal I_ORG_MODE As Boolean = False)

        MAPID = I_MAPID
        MAPVARI = I_MAPVARI
        MAPIDS = I_MAPIDS
        MAPID_DTL = I_MAPID_DTL
        PROFID = C_DEFAULT_DATAKEY
        COMPCODE = String.Empty
        Initialize(I_VIEW_TITLE, I_VIEW_DTL_TITLE, O_RTN, I_ORG_MODE)
    End Sub
    ''' <summary>
    ''' 右リストボックスの初期化処理
    ''' </summary>
    ''' <param name="I_VIEW_TITLE">検索結果一覧のタイトル名</param>
    ''' <param name="O_RTN">結果可否</param>
    ''' <param name="I_ORG_MODE">部署マスタモード</param>
    ''' <remarks></remarks>
    Public Sub Initialize(ByVal I_VIEW_TITLE As String, ByRef O_RTN As String, Optional ByVal I_ORG_MODE As Boolean = False)
        '〇 RightBOX情報設定
        O_RTN = C_MESSAGE_NO.NORMAL
        '○メモ情報取得
        GS0003MEMOget.MAPID = MAPID
        GS0003MEMOget.GS0003MEMOget()
        If isNormal(GS0003MEMOget.ERR) Then
            RF_MEMO.Text = GS0003MEMOget.MEMO
        Else
            O_RTN = GS0003MEMOget.ERR
            Exit Sub
        End If

        '○部署マスタ系は自画面レイアウトなし
        If I_ORG_MODE Then Exit Sub

        '〇レイアウトヘッダー設定
        RF_RIGHT_VIEW_NAME.Text = I_VIEW_TITLE

        '○次画面レイアウト情報取得
        Dim RW_ListBOX As ListBox = CType(RF_VIEW, System.Web.UI.WebControls.ListBox)

        GS0006ViewList.COMPCODE = COMPCODE
        GS0006ViewList.MAPID = MAPID
        GS0006ViewList.PROFID = PROFID
        GS0006ViewList.VIEW = RW_ListBOX
        GS0006ViewList.getList()
        If isNormal(GS0006ViewList.ERR) Then
            For Each Item As ListItem In GS0006ViewList.VIEW.Items
                RW_ListBOX.Items.Add(New ListItem(Item.Text, Item.Value))
            Next
        Else
            O_RTN = GS0006ViewList.ERR
            Exit Sub
        End If

        '○ビューID変数検索
        CS0016ProfMValue.PROFID = PROFID
        CS0016ProfMValue.MAPID = MAPIDS
        CS0016ProfMValue.CAMPCODE = COMPCODE
        CS0016ProfMValue.VARI = MAPVARI
        CS0016ProfMValue.FIELD = C_FIX_VALUE_KEY
        CS0016ProfMValue.getInfo()
        If Not isNormal(CS0016ProfMValue.ERR) Then
            O_RTN = CS0016ProfMValue.ERR
            Exit Sub
        End If

        '○ListBox選択
        RW_ListBOX.SelectedIndex = 0     '選択無しの場合、デフォルト
        For i As Integer = 0 To RW_ListBOX.Items.Count - 1
            If RW_ListBOX.Items(i).Value = CS0016ProfMValue.VALUE Then
                RW_ListBOX.SelectedIndex = i
                Exit For
            End If
        Next
        '〇高さ調整
        RF_MEMO.Height = New Unit(16.9, UnitType.Em)
        RF_VIEW.Height = New Unit(15, UnitType.Em)
    End Sub
    ''' <summary>
    ''' 右リストボックスの初期化処理
    ''' </summary>
    ''' <param name="I_VIEW_TITLE">検索結果一覧のタイトル名</param>
    ''' <param name="I_VIEW_DTL_TITLE">明細結果一覧のタイトル名</param>
    ''' <param name="O_RTN">結果可否</param>
    ''' <param name="I_ORG_MODE">部署マスタモード</param>
    ''' <remarks></remarks>
    Public Sub Initialize(ByVal I_VIEW_TITLE As String, ByVal I_VIEW_DTL_TITLE As String, ByRef O_RTN As String, Optional ByVal I_ORG_MODE As Boolean = False)
        '〇 RightBOX情報設定
        O_RTN = C_MESSAGE_NO.NORMAL
        '○メモ情報取得
        GS0003MEMOget.MAPID = MAPID
        GS0003MEMOget.GS0003MEMOget()
        If isNormal(GS0003MEMOget.ERR) Then
            RF_MEMO.Text = GS0003MEMOget.MEMO
        Else
            O_RTN = GS0003MEMOget.ERR
            Exit Sub
        End If

        '○部署マスタ系は自画面レイアウトなし
        If I_ORG_MODE Then Exit Sub

        '〇レイアウトヘッダー設定
        RF_RIGHT_VIEW_NAME.Text = I_VIEW_TITLE

        '○次画面レイアウト情報取得
        Dim RW_ListBOX As ListBox = CType(RF_VIEW, System.Web.UI.WebControls.ListBox)

        GS0006ViewList.COMPCODE = COMPCODE
        GS0006ViewList.MAPID = MAPID
        GS0006ViewList.PROFID = PROFID
        GS0006ViewList.VIEW = RW_ListBOX
        GS0006ViewList.getList()
        If isNormal(GS0006ViewList.ERR) Then
            For Each Item As ListItem In GS0006ViewList.VIEW.Items
                RW_ListBOX.Items.Add(New ListItem(Item.Text, Item.Value))
            Next
        Else
            O_RTN = GS0006ViewList.ERR
            Exit Sub
        End If

        '○ビューID変数検索
        CS0016ProfMValue.PROFID = PROFID
        CS0016ProfMValue.MAPID = MAPIDS
        CS0016ProfMValue.CAMPCODE = COMPCODE
        CS0016ProfMValue.VARI = MAPVARI
        CS0016ProfMValue.FIELD = C_HEAD_VALUE_KEY
        CS0016ProfMValue.getInfo()
        If Not isNormal(CS0016ProfMValue.ERR) Then
            O_RTN = CS0016ProfMValue.ERR
            Exit Sub
        End If

        '○ListBox選択
        RW_ListBOX.SelectedIndex = 0     '選択無しの場合、デフォルト
        For i As Integer = 0 To RW_ListBOX.Items.Count - 1
            If RW_ListBOX.Items(i).Value = CS0016ProfMValue.VALUE Then
                RW_ListBOX.SelectedIndex = i
                Exit For
            End If
        Next

        '〇レイアウトヘッダー設定
        RF_RIGHT_VIEW_DTL_NAME.Text = I_VIEW_DTL_TITLE
        '○次画面レイアウト情報取得
        Dim RW_DTL_ListBOX As ListBox = CType(RF_VIEW_DTL, System.Web.UI.WebControls.ListBox)
        If IsNothing(RW_DTL_ListBOX) Then
            O_RTN = C_MESSAGE_NO.DLL_IF_ERROR
            Exit Sub
        End If

        GS0006ViewList.MAPID = MAPID_DTL
        GS0006ViewList.COMPCODE = COMPCODE
        GS0006ViewList.PROFID = PROFID
        GS0006ViewList.VIEW = RW_DTL_ListBOX
        GS0006ViewList.getList()
        If isNormal(GS0006ViewList.ERR) Then
            For Each Item As ListItem In GS0006ViewList.VIEW.Items
                RW_DTL_ListBOX.Items.Add(New ListItem(Item.Text, Item.Value))
            Next
        Else
            O_RTN = GS0006ViewList.ERR
            Exit Sub
        End If
        '○ビューID変数検索
        CS0016ProfMValue.PROFID = PROFID
        CS0016ProfMValue.MAPID = MAPIDS
        CS0016ProfMValue.CAMPCODE = COMPCODE
        CS0016ProfMValue.VARI = MAPVARI
        CS0016ProfMValue.FIELD = C_DTL_VALUE_KEY
        CS0016ProfMValue.getInfo()
        If isNormal(CS0016ProfMValue.ERR) Then
        Else
            O_RTN = CS0016ProfMValue.ERR
            Exit Sub
        End If

        '○ListBox選択
        RW_DTL_ListBOX.SelectedIndex = 0     '選択無しの場合、デフォルト
        For i As Integer = 0 To RW_DTL_ListBOX.Items.Count - 1
            If RW_DTL_ListBOX.Items(i).Value = CS0016ProfMValue.VALUE Then
                RW_DTL_ListBOX.SelectedIndex = i
                Exit For
            End If
        Next
        '〇高さ調整
        RF_MEMO.Height = New Unit(13, UnitType.Em)
        RF_VIEW.Height = New Unit(10, UnitType.Em)
        RF_VIEW_DTL.Height = New Unit(10, UnitType.Em)
    End Sub
    ''' <summary>
    ''' VIEWID情報の初期化
    ''' </summary>
    ''' <param name="I_COMPCODE">会社コード</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <remarks></remarks>
    Public Sub InitViewID(ByVal I_COMPCODE As String, ByVal O_RTN As String)
        COMPCODE = I_COMPCODE
        If (String.IsNullOrEmpty(MAPID_DTL)) Then
            initViewID(O_RTN)
        Else
            initViewHEADID(O_RTN)
            initViewDtlID(O_RTN)
        End If
    End Sub
    ''' <summary>
    ''' VIEWID情報の初期化
    ''' </summary>
    ''' <param name="O_RTN">成功可否</param>
    ''' <remarks></remarks>
    Public Sub InitViewID(ByVal O_RTN As String)
        '○次画面レイアウト情報取得
        Dim RW_ListBOX As ListBox = CType(RF_VIEW, System.Web.UI.WebControls.ListBox)
        If IsNothing(RW_ListBOX) Then
            O_RTN = C_MESSAGE_NO.DLL_IF_ERROR
            Exit Sub
        Else

            RW_ListBOX.Items.Clear()
        End If

        GS0006ViewList.MAPID = MAPID
        GS0006ViewList.COMPCODE = COMPCODE
        GS0006ViewList.PROFID = PROFID
        GS0006ViewList.VIEW = RW_ListBOX
        GS0006ViewList.getList()
        If isNormal(GS0006ViewList.ERR) Then
            For Each Item As ListItem In GS0006ViewList.VIEW.Items
                RW_ListBOX.Items.Add(New ListItem(Item.Text, Item.Value))
            Next
        Else
            O_RTN = GS0006ViewList.ERR
            Exit Sub
        End If
        '○ビューID変数検索
        CS0016ProfMValue.PROFID = PROFID
        CS0016ProfMValue.MAPID = MAPIDS
        CS0016ProfMValue.CAMPCODE = COMPCODE
        CS0016ProfMValue.VARI = MAPVARI
        CS0016ProfMValue.FIELD = C_FIX_VALUE_KEY
        CS0016ProfMValue.getInfo()
        If Not isNormal(CS0016ProfMValue.ERR) Then
            O_RTN = CS0016ProfMValue.ERR
            Exit Sub
        End If

        '○ListBox選択
        RW_ListBOX.SelectedIndex = 0     '選択無しの場合、デフォルト
        For i As Integer = 0 To RW_ListBOX.Items.Count - 1
            If RW_ListBOX.Items(i).Value = CS0016ProfMValue.VALUE Then
                RW_ListBOX.SelectedIndex = i
                Exit For
            End If
        Next
    End Sub

    ''' <summary>
    ''' VIEWID情報の初期化
    ''' </summary>
    ''' <param name="O_RTN">成功可否</param>
    ''' <remarks></remarks>
    Public Sub InitViewHEADID(ByVal O_RTN As String)
        '○次画面レイアウト情報取得
        Dim RW_ListBOX As ListBox = CType(RF_VIEW, System.Web.UI.WebControls.ListBox)
        If IsNothing(RW_ListBOX) Then
            O_RTN = C_MESSAGE_NO.DLL_IF_ERROR
            Exit Sub
        Else

            RW_ListBOX.Items.Clear()
        End If

        GS0006ViewList.MAPID = MAPID
        GS0006ViewList.COMPCODE = COMPCODE
        GS0006ViewList.PROFID = PROFID
        GS0006ViewList.VIEW = RW_ListBOX
        GS0006ViewList.getList()
        If isNormal(GS0006ViewList.ERR) Then
            For Each Item As ListItem In GS0006ViewList.VIEW.Items
                RW_ListBOX.Items.Add(New ListItem(Item.Text, Item.Value))
            Next
        Else
            O_RTN = GS0006ViewList.ERR
            Exit Sub
        End If
        '○ビューID変数検索
        CS0016ProfMValue.PROFID = PROFID
        CS0016ProfMValue.MAPID = MAPIDS
        CS0016ProfMValue.CAMPCODE = COMPCODE
        CS0016ProfMValue.VARI = MAPVARI
        CS0016ProfMValue.FIELD = C_HEAD_VALUE_KEY
        CS0016ProfMValue.getInfo()
        If Not isNormal(CS0016ProfMValue.ERR) Then
            O_RTN = CS0016ProfMValue.ERR
            Exit Sub
        End If

        '○ListBox選択
        RW_ListBOX.SelectedIndex = 0     '選択無しの場合、デフォルト
        For i As Integer = 0 To RW_ListBOX.Items.Count - 1
            If RW_ListBOX.Items(i).Value = CS0016ProfMValue.VALUE Then
                RW_ListBOX.SelectedIndex = i
                Exit For
            End If
        Next
    End Sub

    ''' <summary>
    ''' VIEWID情報の初期化
    ''' </summary>
    ''' <param name="O_RTN">成功可否</param>
    ''' <remarks></remarks>
    Public Sub InitViewDtlID(ByVal O_RTN As String)
        '○次画面レイアウト情報取得
        Dim RW_DTL_ListBOX As ListBox = CType(RF_VIEW_DTL, System.Web.UI.WebControls.ListBox)
        If IsNothing(RW_DTL_ListBOX) Then
            O_RTN = C_MESSAGE_NO.DLL_IF_ERROR
            Exit Sub
        Else

            RW_DTL_ListBOX.Items.Clear()
        End If

        GS0006ViewList.MAPID = MAPID_DTL
        GS0006ViewList.COMPCODE = COMPCODE
        GS0006ViewList.PROFID = PROFID
        GS0006ViewList.VIEW = RW_DTL_ListBOX
        GS0006ViewList.getList()
        If isNormal(GS0006ViewList.ERR) Then
            For Each Item As ListItem In GS0006ViewList.VIEW.Items
                RW_DTL_ListBOX.Items.Add(New ListItem(Item.Text, Item.Value))
            Next
        Else
            O_RTN = GS0006ViewList.ERR
            Exit Sub
        End If
        '○ビューID変数検索
        CS0016ProfMValue.PROFID = PROFID
        CS0016ProfMValue.MAPID = MAPIDS
        CS0016ProfMValue.CAMPCODE = COMPCODE
        CS0016ProfMValue.VARI = MAPVARI
        CS0016ProfMValue.FIELD = C_DTL_VALUE_KEY
        CS0016ProfMValue.getInfo()
        If Not isNormal(CS0016ProfMValue.ERR) Then
            O_RTN = CS0016ProfMValue.ERR
            Exit Sub
        End If

        '○ListBox選択
        RW_DTL_ListBOX.SelectedIndex = 0     '選択無しの場合、デフォルト
        For i As Integer = 0 To RW_DTL_ListBOX.Items.Count - 1
            If RW_DTL_ListBOX.Items(i).Value = CS0016ProfMValue.VALUE Then
                RW_DTL_ListBOX.SelectedIndex = i
                Exit For
            End If
        Next
    End Sub
    ''' <summary>
    ''' メモ欄の保存処理
    ''' </summary>
    ''' <param name="I_USERID">更新ユーザID</param>
    ''' <param name="I_TERMID">更新端末ID</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <remarks></remarks>
    Public Sub Save(ByVal I_USERID As String, ByVal I_TERMID As String, ByRef O_RTN As String)
        Dim GS0004MEMOset As New GS0004MEMOset

        GS0004MEMOset.MAPID = MAPID
        GS0004MEMOset.MEMO = RF_MEMO.Text
        GS0004MEMOset.USERID = I_USERID
        GS0004MEMOset.TERMID = I_TERMID
        GS0004MEMOset.GS0004MEMOset()
        O_RTN = GS0004MEMOset.ERR

    End Sub
    ''' <summary>
    ''' VIEWIDの値を取得する
    ''' </summary>
    ''' <returns>VIEWID</returns>
    ''' <remarks></remarks>
    Public Function GetViewId(ByVal I_COMPCODE As String) As String
        If I_COMPCODE <> COMPCODE Then
            Dim O_RTN As String = String.Empty
            InitViewID(I_COMPCODE, O_RTN)
        End If
        Return RF_VIEW.SelectedValue
    End Function
    ''' <summary>
    ''' VIEWIDの値を取得する
    ''' </summary>
    ''' <returns>VIEWID</returns>
    ''' <remarks></remarks>
    Public Function GetViewDtlId(ByVal I_COMPCODE As String) As String
        If I_COMPCODE <> COMPCODE Then
            COMPCODE = I_COMPCODE
            Dim O_RTN As String = String.Empty
            InitViewDtlID(O_RTN)
        End If
        Return RF_VIEW_DTL.SelectedValue
    End Function
#Region "<< Property Accessor >>"
    ''' <summary>
    ''' 結果画面ID
    ''' </summary>
    Public Property MAPID As String
        Get
            Return RF_MAPID.Value
        End Get
        Set(value As String)
            RF_MAPID.Value = value
        End Set
    End Property
    ''' <summary>
    ''' 詳細画面ID
    ''' </summary>
    Public Property MAPID_DTL As String
        Get
            Return RF_MAPID_DTL.Value
        End Get
        Set(value As String)
            RF_MAPID_DTL.Value = value
        End Set
    End Property
    ''' <summary>
    ''' 画面ID
    ''' </summary>
    Public Property MAPIDS As String
        Get
            Return RF_MAPIDS.Value
        End Get
        Set(value As String)
            RF_MAPIDS.Value = value
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
#End Region
End Class