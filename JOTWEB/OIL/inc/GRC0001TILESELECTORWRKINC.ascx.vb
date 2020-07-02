Option Strict On
''' <summary>
''' タイル選択をするコントロール
''' </summary>
Public Class GRC0001TILESELECTORWRKINC
    Inherits System.Web.UI.UserControl
    ''' <summary>
    ''' 選択後にポストバックが必要か(True:ポストバックする,False:ポストバックしない(初期値))
    ''' </summary>
    ''' <returns></returns>
    Public Property NeedsPostbackAfterSelect As Boolean = False
    ''' <summary>
    ''' ListBoxClass設定(LeftBoxを設定と同様に設定する)
    ''' </summary>
    ''' <returns></returns>
    Public Property ListBoxClassification As GRIS0005LeftBox.LIST_BOX_CLASSIFICATION = Nothing
    ''' <summary>
    ''' パラメータデータ(LeftBoxを設定と同様に設定する)
    ''' </summary>
    ''' <returns></returns>
    Public Property ParamData As Hashtable = Nothing
    ''' <summary>
    ''' 使用可否フラグ
    ''' </summary>
    ''' <returns></returns>
    Public Property Enabled As Boolean = True
    ''' <summary>
    ''' 複数選択可否(初期値Multiple(複数選択可能)
    ''' </summary>
    ''' <returns></returns>
    Public Property SelectionMode As ListSelectionMode = ListSelectionMode.Multiple
    ''' <summary>
    ''' 左ボックスのカスタムコントロール
    ''' </summary>
    ''' <returns></returns>
    Public Property LeftObj As GRIS0005LeftBox = Nothing
    ''' <summary>
    ''' ロード時処理
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub
    ''' <summary>
    ''' 選択ボックスのタイルの値を設定
    ''' </summary>
    Public Sub SetTileValues()
        Me.chklGrc0001SelectionBox.Enabled = Me.Enabled
        Me.txtGrc0001ListClass.Text = CInt(Me.ListBoxClassification).ToString
        Me.txtGrc0001SelectionMode.Text = CInt(Me.SelectionMode).ToString
        Me.txtGrc0001NeedsAfterPostBack.Text = Me.NeedsPostbackAfterSelect.ToString
        ResetChkValues()
    End Sub

    '''' <summary>
    '''' 選択されているリストボックスのキーと値（表示値）を取得
    '''' </summary>
    '''' <returns></returns>
    'Public Function GetSelectedValues() As List(Of KeyValuePair(Of String, String))

    'End Function
    '''' <summary>
    '''' 選択未選択を除き全リストの値を取得
    '''' </summary>
    '''' <returns></returns>
    'Public Function GetAllValues() As List(Of KeyValuePair(Of String, String))

    'End Function
    ''' <summary>
    ''' 画面設定値をリセットする
    ''' </summary>
    Private Sub ResetChkValues()
        Dim retValDummy As String = ""
        Me.LeftObj.SetListBox(Me.ListBoxClassification, retValDummy, Me.ParamData)

        '取得結果が無い場合はタイルをクリアし終了
        If Me.LeftObj.WF_LeftListBox IsNot Nothing AndAlso Me.LeftObj.WF_LeftListBox.Items.Count = 0 Then
            Me.chklGrc0001SelectionBox.Items.Clear()
            Return
        End If
        '取得結果をタイルに紐づけ
        Dim chkItmList As List(Of KeyValuePair(Of String, String)) = (From lstItm As ListItem In Me.LeftObj.WF_LeftListBox.Items.Cast(Of ListItem)
                                                                      Select New KeyValuePair(Of String, String)(lstItm.Value, lstItm.Text)).ToList
        Me.chklGrc0001SelectionBox.DataSource = chkItmList
        Me.chklGrc0001SelectionBox.DataTextField = "value"
        Me.chklGrc0001SelectionBox.DataValueField = "key"
        Me.chklGrc0001SelectionBox.DataBind()

    End Sub
    ''' <summary>
    ''' パラメータを変更しチェック
    ''' </summary>
    ''' <param name="paramData">選択タイルの再設定</param>
    Public Sub ResetChkValues(paramData As Hashtable)
        Me.ParamData = paramData
        ResetChkValues()
    End Sub
    ''' <summary>
    ''' 表示しているアイテムをすべて選択状態にする
    ''' </summary>
    Public Sub SelectAll()
        For Each chkItm As ListItem In chklGrc0001SelectionBox.Items
            chkItm.Selected = True
        Next
    End Sub
    ''' <summary>
    ''' 一つのアイテムのみ選択状態にする(他が選択されている場合、Offにされます)
    ''' </summary>
    ''' <param name="key">選択するキー</param>
    Public Sub SelectSingleItem(key As String)
        For Each chkItm As ListItem In chklGrc0001SelectionBox.Items
            If chkItm.Value = key Then
                chkItm.Selected = True
            Else
                chkItm.Selected = False
            End If
        Next
    End Sub
    ''' <summary>
    ''' 複数アイテムの選択
    ''' </summary>
    ''' <param name="keys">選択するキーの配列</param>
    Public Sub SelectMultiItems(keys As List(Of String))
        For Each chkItm As ListItem In chklGrc0001SelectionBox.Items
            If keys.Contains(chkItm.Value) Then
                chkItm.Selected = True
            Else
                chkItm.Selected = False
            End If
        Next
    End Sub
    ''' <summary>
    ''' 選択したデータ有無(True:選択あり,False:選択なし)
    ''' </summary>
    ''' <returns></returns>
    Public Function HasSelectedValue() As Boolean
        Dim find As Boolean = False
        If chklGrc0001SelectionBox Is Nothing OrElse chklGrc0001SelectionBox.Items.Count = 0 Then
            Return find
        End If
        find = (From itm In chklGrc0001SelectionBox.Items.Cast(Of ListItem) Where itm.Selected).Any
        Return find
    End Function
    ''' <summary>
    ''' この画面に戻る際の復元
    ''' </summary>
    ''' <param name="base64Str"></param>
    Public Sub Recover(base64Str As String)
        Dim transVal = DecodeBase64(base64Str)
        '保持情報の復元
        Me.txtGrc0001ListClass.Text = CInt(transVal.ListBoxClassification).ToString
        Me.txtGrc0001NeedsAfterPostBack.Text = Convert.ToString(transVal.NeedsPostbackAfterSelect)
        Me.txtGrc0001SelectionMode.Text = CInt(transVal.SelectionMode).ToString
        'タイルチェックの復元
        'リストが無い場合は何もしない
        If transVal.ListValues Is Nothing OrElse transVal.ListValues.Count = 0 Then
            Return
        End If
        Dim qselectedItm = (From itm In transVal.ListValues Select New ListItem(itm.Value, itm.Key) With {.Selected = itm.Selected})
        If qselectedItm.Any = False Then
            Return
        End If
        Me.chklGrc0001SelectionBox.Items.AddRange(qselectedItm.ToArray)
    End Sub
    ''' <summary>
    ''' 値をWorkIncに退避するBase64Encode文字
    ''' </summary>
    ''' <returns></returns>
    Public Function GetListItemsStr() As String
        Dim transVal As New TransKeepValues
        Dim enumSelModeVal = DirectCast([Enum].ToObject(GetType(ListSelectionMode), CInt(Me.txtGrc0001SelectionMode.Text)), ListSelectionMode)
        transVal.SelectionMode = enumSelModeVal

        transVal.NeedsPostbackAfterSelect = Convert.ToBoolean(Me.txtGrc0001NeedsAfterPostBack.Text)

        Dim enmListClassVal = DirectCast([Enum].ToObject(GetType(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION), CInt(Me.txtGrc0001ListClass.Text)), GRIS0005LeftBox.LIST_BOX_CLASSIFICATION)
        transVal.ListBoxClassification = enmListClassVal

        transVal.ListValues = (From itm In Me.chklGrc0001SelectionBox.Items.Cast(Of ListItem)
                               Select New TransKeepValues.ListValue(itm.Value, itm.Text, itm.Selected)).ToList

        Dim formatter As New Runtime.Serialization.Formatters.Binary.BinaryFormatter()
        Dim base64Str As String = ""
        Dim noConpressionByte As Byte()
        'クラスをシリアライズ
        Using ms As New IO.MemoryStream()
            formatter.Serialize(ms, transVal)
            noConpressionByte = ms.ToArray
        End Using
        '圧縮シリアライズしたByteデータを圧縮し圧縮したByteデータをBase64に変換
        Using ms As New IO.MemoryStream(),
              ds As New IO.Compression.DeflateStream(ms, IO.Compression.CompressionMode.Compress, True)
            ds.Write(noConpressionByte, 0, noConpressionByte.Length)
            ds.Close()
            Dim byteDat = ms.ToArray
            base64Str = Convert.ToBase64String(byteDat, 0, byteDat.Length, Base64FormattingOptions.None)
        End Using
        Return base64Str
    End Function
    ''' <summary>
    ''' 選択(複数)されたアイテムのみのリストボックスを取得。※単一の値が取りたい場合は別メソッドがあります
    ''' </summary>
    ''' <param name="base64Str">GetListItemsStrで退避した文字列</param>
    ''' <returns></returns>
    Public Shared Function GetSelectedListData(base64Str As String) As ListBox
        '未選択を含むリストの情報をすべて復元
        Dim allValues As TransKeepValues = DecodeBase64(base64Str)
        Dim retVal As New ListBox
        'リストが無い場合は何もしない
        If allValues.ListValues Is Nothing OrElse allValues.ListValues.Count = 0 Then
            Return retVal
        End If
        Dim qselectedItm = (From itm In allValues.ListValues Where itm.Selected Select New ListItem(itm.Value, itm.Key) With {.Selected = itm.Selected})
        If qselectedItm.Any = False Then
            Return retVal
        End If
        retVal.Items.AddRange(qselectedItm.ToArray)
        Return retVal
    End Function
    ''' <summary>
    ''' 選択状態のリストボックスデータを取得(画面用)
    ''' </summary>
    ''' <returns></returns>
    Public Function GetSelectedListData() As ListBox
        Dim qselectedItm = (From itm In Me.chklGrc0001SelectionBox.Items.Cast(Of ListItem) Where itm.Selected = True)
        Dim retVal As New ListBox
        If qselectedItm.Any = False Then
            Return retVal
        End If
        retVal.Items.AddRange(qselectedItm.ToArray)
        Return retVal
    End Function
    ''' <summary>
    ''' 選択されたコード値を取得(複数選択の場合は先頭、選択が無ければブランク)
    ''' </summary>
    ''' <param name="base64Str">GetListItemsStrで退避した文字列</param>
    ''' <returns></returns>
    Public Shared Function GetSelectedSingleValue(base64Str As String) As String
        Dim listData = GetSelectedListData(base64Str)
        If listData Is Nothing OrElse listData.Items.Count = 0 Then
            Return ""
        End If
        Return listData.Items(0).Value
    End Function
    ''' <summary>
    ''' 選択されたコード値を取得(複数選択の場合は先頭、選択が無ければブランク)画面用
    ''' </summary>
    ''' <returns></returns>
    Public Function GetSelectedSingleValue() As String
        Dim listData = GetSelectedListData()
        If listData Is Nothing OrElse listData.Items.Count = 0 Then
            Return ""
        End If
        Return listData.Items(0).Value
    End Function
    ''' <summary>
    ''' 選択された表示値を取得(複数選択の場合は先頭、選択が無ければブランク)
    ''' </summary>
    ''' <param name="base64Str">GetListItemsStrで退避した文字列</param>
    ''' <returns></returns>
    Public Shared Function GetSelectedSingleText(base64Str As String) As String
        Dim listData = GetSelectedListData(base64Str)
        If listData Is Nothing OrElse listData.Items.Count = 0 Then
            Return ""
        End If
        Return listData.Items(0).Text
    End Function
    ''' <summary>
    ''' 選択された表示値を取得(複数選択の場合は先頭、選択が無ければブランク)画面用
    ''' </summary>
    ''' <returns></returns>
    Public Function GetSelectedSingleText() As String
        Dim listData = GetSelectedListData()

        If listData Is Nothing OrElse listData.Items.Count = 0 Then
            Return ""
        End If
        Return listData.Items(0).Text
    End Function
    ''' <summary>
    ''' SQLのInステートメント用の文字列を生成(選択されたコード値を'[コード]','[コード]'・・・で返却)
    ''' </summary>
    ''' <param name="base64Str">GetListItemsStrで退避した文字列</param>
    ''' <returns></returns>
    ''' <remarks>選択が無ければブランクで返却します</remarks>
    Public Shared Function GetSelectedSqlInStatement(base64Str As String) As String

        '未選択を含むリストの情報をすべて復元
        Dim allValues As TransKeepValues = DecodeBase64(base64Str)
        Dim retVal As New ListBox
        'リストが無い場合は何もしない
        If allValues.ListValues Is Nothing OrElse allValues.ListValues.Count = 0 Then
            Return ""
        End If
        Dim qselectedItm = (From itm In allValues.ListValues Where itm.Selected Select "'" & itm.Key & "'")
        If qselectedItm.Any = False Then
            Return ""
        End If
        Return String.Join(",", qselectedItm)
    End Function
    ''' <summary>
    ''' 画面引継ぎのBase64文字をデコードしTransKeepValuesに格納
    ''' </summary>
    ''' <param name="base64Str">GetListItemsStrで退避した文字列</param>
    ''' <returns></returns>
    Private Shared Function DecodeBase64(base64Str As String) As TransKeepValues
        Dim retVal As TransKeepValues
        Dim formatter As New Runtime.Serialization.Formatters.Binary.BinaryFormatter()
        Dim conmressedByte As Byte()
        conmressedByte = Convert.FromBase64String(base64Str)
        '取得した文字をByte化し解凍、画面利用クラスに再格納
        Using inpMs As New IO.MemoryStream(conmressedByte),
              outMs As New IO.MemoryStream,
              ds As New IO.Compression.DeflateStream(inpMs, IO.Compression.CompressionMode.Decompress)
            ds.CopyTo(outMs)
            outMs.Position = 0
            retVal = DirectCast(formatter.Deserialize(outMs), TransKeepValues)
        End Using
        Return retVal
    End Function
    ''' <summary>
    ''' 次画面遷移時に保持する情報クラス
    ''' </summary>
    <Serializable>
    Public Class TransKeepValues
        ''' <summary>
        ''' 選択後にポストバックが必要か(True:ポストバックする,False:ポストバックしない(初期値))
        ''' </summary>
        ''' <returns></returns>
        Public Property NeedsPostbackAfterSelect As Boolean = False
        ''' <summary>
        ''' ListBoxClass設定(LeftBoxを設定と同様に設定する)
        ''' </summary>
        ''' <returns></returns>
        Public Property ListBoxClassification As GRIS0005LeftBox.LIST_BOX_CLASSIFICATION = Nothing
        ''' <summary>
        ''' パラメータデータ(LeftBoxを設定と同様に設定する)
        ''' </summary>
        ''' <returns></returns>
        Public Property ParamData As Hashtable = Nothing
        ''' <summary>
        ''' 使用可否フラグ
        ''' </summary>
        ''' <returns></returns>
        Public Property Enabled As Boolean = True
        ''' <summary>
        ''' 複数選択可否(初期値Multiple(複数選択可能)
        ''' </summary>
        ''' <returns></returns>
        Public Property SelectionMode As ListSelectionMode = ListSelectionMode.Multiple
        ''' <summary>
        ''' 画面上のタイル情報
        ''' </summary>
        ''' <returns></returns>
        Public Property ListValues As List(Of ListValue)
        ''' <summary>
        ''' リストアイテム情報保持クラス
        ''' </summary>
        <Serializable>
        Public Class ListValue
            ''' <summary>
            ''' コンストラクタ
            ''' </summary>
            ''' <param name="key">コード値</param>
            ''' <param name="value">表示値</param>
            ''' <param name="selected">選択</param>
            Public Sub New(key As String, value As String, selected As Boolean)
                Me.Key = key
                Me.Value = value
                Me.Selected = selected
            End Sub

            ''' <summary>
            ''' コード値
            ''' </summary>
            ''' <returns></returns>
            Public Property Key As String
            ''' <summary>
            ''' 表示値
            ''' </summary>
            ''' <returns></returns>
            Public Property Value As String
            '選択(True:選択、False:未選択)
            Public Property Selected As Boolean
        End Class
    End Class
End Class