Option Strict On
Imports System.Data.SqlClient
''' <summary>
''' GS系根底クラス
''' </summary>
''' <remarks></remarks>
Public MustInherit Class GL0000 : Implements IDisposable

    ''' <summary>
    ''' タイトル区分
    ''' </summary>
    Protected Friend Class C_TITLEKBN
        ''' <summary>
        ''' ヘッダー
        ''' </summary>
        Public Const HEADER As String = "H"
        ''' <summary>
        ''' タイトル
        ''' </summary>
        Public Const TITLE As String = "T"
        ''' <summary>
        ''' 明細
        ''' </summary>
        Public Const DETAIL As String = "I"
        ''' <summary>
        ''' 繰り返しデータのキー項目
        ''' </summary>
        Public Const REPEAT_KEY As String = "I_DataKey"
        ''' <summary>
        ''' 繰り返しデータ
        ''' </summary>
        Public Const REPEAT_DATA As String = "I_Data"
    End Class
    ''' <summary>
    ''' HD区分
    ''' </summary>
    Protected Friend Class C_HDKBN
        ''' <summary>
        ''' ヘッダー
        ''' </summary>
        Public Const HEADER As String = "H"
        ''' <summary>
        ''' 明細
        ''' </summary>
        Public Const DETAIL As String = "I"
    End Class
    ''' <summary>
    ''' 初期状態の並び順
    ''' </summary>
    Public Class C_DEFAULT_SORT
        Public Const CODE As String = "CODE"

        Public Const NAMES As String = "NAMES"

        Public Const SEQ As String = "SEQ"
    End Class
    ''' <summary>
    ''' 一覧表示形式
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum C_VIEW_FORMAT_PATTERN
        ''' <summary>
        ''' 名称表示
        ''' </summary>
        NAMES
        ''' <summary>
        ''' コード表示
        ''' </summary>
        CODE
        ''' <summary>
        ''' 併記
        ''' </summary>
        BOTH
    End Enum
    ''' <summary>
    ''' 構造体文字列
    ''' </summary>
    Protected Friend Class C_STRUCT_CODE

        Public Const ORG_LIST_CODE As String = "管轄組織"

        Public Const ATTENDANCE_CODE As String = "勤怠管理組織"
    End Class
    ''' <summary>
    ''' 開始年月日
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property STYMD() As Date
    ''' <summary>
    ''' 終了年月日
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ENDYMD() As Date
    ''' <summary>
    ''' エラーメッセージ
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ERR As String = C_MESSAGE_NO.NORMAL
    ''' <summary>
    ''' リストボックス表示用
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VIEW_FORMAT As C_VIEW_FORMAT_PATTERN = C_VIEW_FORMAT_PATTERN.NAMES
    ''' <summary>
    ''' 初期並び順
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property DEFAULT_SORT As String = C_DEFAULT_SORT.SEQ

    ''' <summary>
    ''' 結果リストボックス
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LIST() As ListBox
    'セッション制御宣言
    Protected sm As New CS0050SESSION
    ''' <summary>
    ''' パラメータチェック処理
    ''' </summary>
    ''' <param name="subclass"></param>
    ''' <param name="value"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Function checkParam(ByVal subclass As String, ByVal value As Object) As String

        If IsNothing(value) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = subclass
            CS0011LOGWRITE.INFPOSI = ""
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            checkParam = C_MESSAGE_NO.DLL_IF_ERROR
        Else
            checkParam = C_MESSAGE_NO.NORMAL
        End If
    End Function


    Public MustOverride Sub getList()

    ''' <summary>
    ''' 一覧作成処理
    ''' </summary>
    ''' <param name="I_SQLDR"></param>
    ''' <remarks></remarks>
    Protected Sub addListData(ByVal I_SQLDR As SqlDataReader)
        addListData(I_SQLDR, "CODE", "NAMES")
    End Sub
    ''' <summary>
    ''' 一覧作成処理
    ''' </summary>
    ''' <param name="I_SQLDR"></param>
    ''' <param name="CODE_NAME"></param>
    ''' <param name="NAMES_NAME"></param>
    ''' <remarks></remarks>
    Protected Sub addListData(ByVal I_SQLDR As SqlDataReader, ByVal CODE_NAME As String, ByVal NAMES_NAME As String)
        While I_SQLDR.Read

            '            If Not extracheck(I_SQLDR) Then Continue While
            '○出力編集
            Select Case VIEW_FORMAT
                Case C_VIEW_FORMAT_PATTERN.NAMES
                    LIST.Items.Add(New ListItem(Convert.ToString(I_SQLDR(NAMES_NAME)), Convert.ToString(I_SQLDR(CODE_NAME))))

                Case C_VIEW_FORMAT_PATTERN.CODE
                    LIST.Items.Add(New ListItem(Convert.ToString(I_SQLDR(CODE_NAME)), Convert.ToString(I_SQLDR(CODE_NAME))))

                Case C_VIEW_FORMAT_PATTERN.BOTH
                    LIST.Items.Add(New ListItem(String.Format("{0}({1})", I_SQLDR(NAMES_NAME), I_SQLDR(CODE_NAME)), Convert.ToString(I_SQLDR(CODE_NAME))))

                Case Else
                    addExtraListData(I_SQLDR, CODE_NAME, NAMES_NAME)
            End Select
        End While
    End Sub
    ''' <summary>
    ''' 一覧作成処理
    ''' </summary>
    ''' <param name="I_SQLDR"></param>
    ''' <remarks></remarks>
    Protected Overridable Function extracheck(ByVal I_SQLDR As SqlDataReader) As Boolean
        Return True
    End Function
    ''' <summary>
    ''' 一覧作成処理
    ''' </summary>
    ''' <param name="I_SQLDR"></param>
    ''' <param name="CODE_NAME"></param>
    ''' <param name="NAMES_NAME"></param>
    ''' <remarks></remarks>
    Public Overridable Sub addExtraListData(ByVal I_SQLDR As SqlDataReader, ByVal CODE_NAME As String, ByVal NAMES_NAME As String)
        LIST.Items.Add(New ListItem(Convert.ToString(I_SQLDR(NAMES_NAME)), Convert.ToString(I_SQLDR(CODE_NAME))))
    End Sub

    ''' <summary>
    ''' テーブルオブジェクト展開
    ''' </summary>
    ''' <param name="profTbl">PROFVIEWデータ</param>
    ''' <param name="outArea">出力先(Panel)コントロール</param>
    Protected Sub MakeTableObject(ByVal profTbl As String(,), ByVal srcTbl As DataTable, outArea As Panel)

        '●項目定義取得
        Dim outTHCell = New TableHeaderCell With {.ViewStateMode = UI.ViewStateMode.Disabled}
        Dim lenghtFix As Integer = 0
        Dim leftFixAll As Integer = 32
        Dim rightLengthFixAll As Integer = 0

        'テーブルに属性追加
        outArea.Attributes.Add("data-generated", "1")
        outArea.Attributes.Add("data-scrolltype", "1")

        'ソートキー領域作成
        Dim sortItemId As String = "hdnListSortValue" & outArea.Page.Form.ClientID & outArea.ID
        Dim sortValue As String = ""
        Dim sortItems As New HiddenField With {.ID = sortItemId, .ViewStateMode = UI.ViewStateMode.Disabled}
        If outArea.Page.Request.Form.GetValues(sortItemId) IsNot Nothing Then
            sortValue = outArea.Page.Request.Form.GetValues(sortItemId)(0)
        End If
        sortItems.Value = sortValue
        outArea.Controls.Add(sortItems)

        ' ヘッダー作成
        Dim outPanel = New Panel
        outPanel.ID = Trim(outArea.ID) & "_H"
        Dim outTable = New Table() With {.ViewStateMode = UI.ViewStateMode.Disabled}
        Dim outTHeader = New TableHeaderRow With {.ViewStateMode = UI.ViewStateMode.Disabled}
        outTHeader = New TableHeaderRow With {.ViewStateMode = UI.ViewStateMode.Disabled}
        For i As Integer = 0 To profTbl.GetLength(0) - 1

            outTHCell = New TableHeaderCell With {.ViewStateMode = UI.ViewStateMode.Disabled}
            outTHCell.Attributes.Add("cellfieldname", profTbl(i, 0))

            outTHCell.Text = profTbl(i, 1)
            lenghtFix = (CInt(profTbl(i, 2)) * 16)

            If lenghtFix = 0 Then
                outTHCell.Style.Add("display", "none")
            Else
                outTHCell.Style.Add("width", lenghtFix.ToString & "px")
            End If
            outTHeader.Cells.Add(outTHCell)
            rightLengthFixAll = rightLengthFixAll + lenghtFix + If(lenghtFix = 0, 0, 2) '内報するテーブルの左右ボーダー2px分も加味
        Next

        outTable.Rows.Add(outTHeader)
        outTable.Style.Add("background-color", "aqua")
        outTable.Style.Add("width", rightLengthFixAll.ToString & "px")
        outPanel.Controls.Add(outTable)
        outArea.Controls.Add(outPanel)

        ' データ
        outPanel = New Panel
        outPanel.ID = Trim(outArea.ID) & "_D"
        outTable = New Table()
        Dim outTData = New TableRow
        Dim scrDr As DataRow = Nothing
        For i As Integer = 0 To srcTbl.Rows.Count - 1
            scrDr = srcTbl(i)
            outTData = New TableRow

            '〇転送用パラメタの作成
            Dim prmData As String = String.Empty
            For index As Integer = 0 To profTbl.GetLength(0) - 1
                Dim fieldName As String = Convert.ToString(profTbl(index, 0))
                prmData = If(String.IsNullOrEmpty(prmData), "", prmData & C_VALUE_SPLIT_DELIMITER) &
                            fieldName & "=" & Convert.ToString(scrDr(fieldName))
            Next

            For j As Integer = 0 To profTbl.GetLength(0) - 1
                Dim outCell = New TableCell
                Dim fieldName As String = Convert.ToString(profTbl(j, 0))

                outCell.Text = Convert.ToString(scrDr(fieldName))

                'テーブルセルのサイズ
                If CInt(profTbl(j, 2)) * 16 = 0 Then
                    outCell.Style.Add("display", "none")
                Else
                    Dim cellWidth As String = ((CInt(profTbl(j, 2)) * 16)).ToString
                    outCell.Style.Add("width", cellWidth & "px")
                End If
                outCell.Attributes.Add("id", "LTD" & j & "_" & i)
                outCell.Attributes.Add(fieldName, Convert.ToString(scrDr(fieldName)))
                'イベント追加
                outCell.Attributes.Add("ondblclick", "WF_TableF_DbClick('" & prmData & "');")
                '生成したセルの追加先
                outTData.Cells.Add(outCell)
            Next
            outTable.Rows.Add(outTData)

        Next

        outTable.Style.Add("width", rightLengthFixAll.ToString & "px")
        outTable.Style.Add("background-color", "white")
        outPanel.Controls.Add(outTable)

        outArea.Controls.Add(outPanel)

    End Sub
    ''' <summary>
    ''' 解放処理
    ''' </summary>
    Protected Sub Dispose() Implements IDisposable.Dispose
        If Not isnothing(LIST) Then LIST.Dispose()
        'GC.SuppressFinalize(Me)
    End Sub
End Class

