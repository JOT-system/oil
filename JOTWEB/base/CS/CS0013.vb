Option Explicit On

Imports System.Data.SqlClient
Imports System.Web
Imports System.Web.UI.WebControls
Imports System.Web.UI.HtmlControls

''' <summary>
''' Tableオブジェクト展開
''' </summary>
''' <remarks>CS0013UPROFview置換　GB.COA0013TableObjectから修正</remarks>
Public Class CS0013ProfView
    ''' <summary>
    ''' スクロールタイププロパティ用enum
    ''' </summary>
    Enum SCROLLTYPE_ENUM
        ''' <summary>
        ''' スクロールバーなし
        ''' </summary>
        None = 0
        ''' <summary>
        ''' 縦
        ''' </summary>
        Vertical
        ''' <summary>
        ''' 横
        ''' </summary>
        Horizontal
        ''' <summary>
        ''' 縦横両方
        ''' </summary>
        Both
    End Enum

    ''' <summary>
    ''' [IN]会社コードプロパティ
    ''' </summary>
    ''' <returns>[IN]CAMPCODE</returns>
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' [IN]PROFIDプロパティ
    ''' </summary>
    ''' <returns>[IN]PROFID</returns>
    Public Property PROFID() As String
    ''' <summary>
    ''' [IN]画面IDプロパティ
    ''' </summary>
    ''' <returns>[IN]画面ID</returns>
    Public Property MAPID() As String
    ''' <summary>
    ''' [IN]変数プロパティ
    ''' </summary>
    ''' <returns>[IN]変数</returns>
    Public Property VARI As String
    ''' <summary>
    ''' [IN]一覧表元データテーブルプロパティ
    ''' </summary>
    ''' <returns>[IN]一覧表元データテーブル</returns>
    Public Property SRCDATA As DataTable
    ''' <summary>
    ''' [IN]一覧表展開先パネルオブジェクトプロパティ
    ''' </summary>
    ''' <returns>[IN]一覧表展開先パネルオブジェクト</returns>
    Public Property TBLOBJ As Panel
    ''' <summary>
    ''' [IN]行紐づけイベント名プロパティ
    ''' </summary>
    ''' <returns>[IN]行紐づけイベント名</returns>
    Public Property LEVENT As String
    ''' <summary>
    ''' [IN]行紐づけイベント名実行JavaScript関数名プロパティ
    ''' </summary>
    ''' <returns>[IN]行紐づけイベント名実行JavaScript関数名</returns>
    Public Property LFUNC As String
    ''' <summary>
    ''' [IN]スクロールタイプ(1:縦のみ,2:横のみ,3:両方)プロパティ
    ''' </summary>
    ''' <returns>[IN]スクロールタイプ(1:縦のみ,2:横のみ,3:両方)</returns>
    Public Property SCROLLTYPE As String
    ''' <summary>
    ''' [IN]タイトル設定(セル内容チップ表示)プロパティ(未設定は表示しない)
    ''' </summary>
    ''' <returns>[IN]タイトル設定(セル内容チップ表示)</returns>
    Public Property TITLEOPT As Boolean
    ''' <summary>
    ''' [IN]No列非表示プロパティ
    ''' </summary>
    ''' <returns>[IN]No列非表示</returns>
    Public Property HIDENOOPT As Boolean
    ''' <summary>
    ''' [IN]OPERATION列非表示プロパティ
    ''' </summary>
    ''' <returns>[IN]OPERATION列非表示</returns>
    Public Property HIDEOPERATIONOPT As Boolean
    ''' <summary>
    ''' [IN]No列サイズプロパティ
    ''' </summary>
    ''' <returns>[IN]No列サイズ</returns>
    Public Property NOCOLUMNWIDTHOPT As Integer
    ''' <summary>
    ''' [IN]OPERATION列サイズプロパティ
    ''' </summary>
    ''' <returns>[IN]OPERATION列サイズ</returns>
    Public Property OPERATIONCOLUMNWIDTHOPT As Integer
    ''' <summary>
    ''' [IN]ユーザーソート機能オプション(0:ユーザーソート機能なし(デフォルト),1:ユーザーソート機能あり))
    ''' </summary>
    ''' <returns>[IN]ユーザーソート機能オプション</returns>
    Public Property USERSORTOPT As Integer
    ''' <summary>
    ''' [IN]タグ名設定機能オプション(FALSE:名称無(デフォルト),TRUE:名称有))
    ''' </summary>
    ''' <returns>[IN]タグ名設定機能オプション</returns>
    Public Property WITHTAGNAMES As Boolean
    ''' <summary>
    ''' [IN]対象年月
    ''' </summary>
    ''' <returns></returns>
    Public Property TARGETDATE() As String
    ''' <summary>
    ''' [OUT]ERRNoプロパティ
    ''' </summary>
    ''' <returns>[OUT]ERRNo</returns>
    Public Property ERR As String

    ''' <summary>
    ''' セッション管理
    ''' </summary>
    Private sm As New CS0050SESSION
    ''' <summary>
    ''' リストの管理テーブル
    ''' </summary>
    Private lmp As New Hashtable

    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    ''' <remarks></remarks> 
    Public Sub New()

        'プロパティ初期化
        Initialize()

    End Sub

    ''' <summary>
    ''' 初期化
    ''' </summary>
    ''' <remarks></remarks> 
    Public Sub Initialize()

        CAMPCODE = String.Empty
        PROFID = String.Empty
        MAPID = String.Empty
        VARI = String.Empty
        SRCDATA = Nothing
        TBLOBJ = Nothing
        LEVENT = String.Empty
        LFUNC = String.Empty
        SCROLLTYPE = String.Empty
        TITLEOPT = False
        HIDENOOPT = False
        HIDEOPERATIONOPT = False
        NOCOLUMNWIDTHOPT = False
        OPERATIONCOLUMNWIDTHOPT = False
        USERSORTOPT = 0
        WITHTAGNAMES = False
        TARGETDATE = String.Empty
        lmp.Clear()

        ERR = C_MESSAGE_NO.NORMAL

    End Sub

    ''' <summary>
    ''' テーブルオブジェクト展開
    ''' </summary>
    ''' <remarks></remarks> 
    Public Sub CS0013ProfView()

        Try
            '●In PARAMチェック
            '必須設定チェック
            If IsNothing(CAMPCODE) Then
                Throw New ArgumentNullException("CAMPCODE")
            End If
            If IsNothing(MAPID) Then
                Throw New ArgumentNullException("MAPID")
            End If
            If IsNothing(VARI) Then
                Throw New ArgumentNullException("VARI")
            End If
            If IsNothing(SRCDATA) Then
                Throw New ArgumentNullException("SRCDATA")
            End If
            If IsNothing(TBLOBJ) Then
                Throw New ArgumentNullException("TBLOBJ")
            End If
            If IsNothing(TARGETDATE) OrElse TARGETDATE = "" Then
                TARGETDATE = Date.Now.ToString("yyyy/MM/dd")
            End If

            ' 設定格納
            Dim profTbl As DataTable = New DataTable("WORKTABLE")
            Dim columnNames As New List(Of String) From {"FIELD", "FIELDNAMES", "EFFECT", "POSICOL",
                                                        "WIDTH", "ALIGN", "REQUIRED",
                                                        "OBJECTTYPE", "FORMATTYPE", "FORMATVALUE",
                                                        "FIXCOL", "COLORSET",
                                                        "ADDEVENT1", "ADDFUNC1", "ADDEVENT2", "ADDFUNC2",
                                                        "ADDEVENT3", "ADDFUNC3", "ADDEVENT4", "ADDFUNC4",
                                                        "ADDEVENT5", "ADDFUNC5"}
            For Each columnName In columnNames
                profTbl.Columns.Add(columnName, GetType(String))
            Next

            '●項目定義取得
            '検索SQL文
            Dim SQLStr As String =
                 "SELECT rtrim(FIELD) as FIELD , rtrim(FIELDNAMES) as FIELDNAMES , " _
                & " rtrim(EFFECT) as EFFECT , " _
                & " POSICOL , " _
                & " rtrim(LENGTH) as LENGTH , " _
                & " rtrim(WIDTH) as WIDTH , " _
                & " rtrim(ALIGN) as ALIGN , " _
                & " rtrim(REQUIRED) as REQUIRED, " _
                & " rtrim(OBJECTTYPE) as OBJECTTYPE , " _
                & " rtrim(FORMATTYPE) as FORMATTYPE, " _
                & " rtrim(FORMATVALUE) as FORMATVALUE, " _
                & " rtrim(FIXCOL) as FIXCOL , " _
                & " isnull(rtrim(COLORSET),'')  as COLORSET , " _
                & " isnull(rtrim(ADDEVENT1),'') as ADDEVENT1 , isnull(rtrim(ADDFUNC1),'') as ADDFUNC1 , " _
                & " isnull(rtrim(ADDEVENT2),'') as ADDEVENT2 , isnull(rtrim(ADDFUNC2),'') as ADDFUNC2 , " _
                & " isnull(rtrim(ADDEVENT3),'') as ADDEVENT3 , isnull(rtrim(ADDFUNC3),'') as ADDFUNC3 , " _
                & " isnull(rtrim(ADDEVENT4),'') as ADDEVENT4 , isnull(rtrim(ADDFUNC4),'') as ADDFUNC4 , " _
                & " isnull(rtrim(ADDEVENT5),'') as ADDEVENT5 , isnull(rtrim(ADDFUNC5),'') as ADDFUNC5   " _
                & " FROM  com.S0025_PROFMVIEW  " _
                & " Where CAMPCODE = @CAMPCODE " _
                & "   and PROFID   = @PROFID " _
                & "   and MAPID    = @MAPID " _
                & "   and VARIANT  = @VARIANT " _
                & "   and HDKBN    = 'H' " _
                & "   and TITLEKBN = 'I' " _
                & "   and STYMD   <= @STYMD " _
                & "   and ENDYMD  >= @ENDYMD " _
                & "   and DELFLG  <> '" & C_DELETE_FLG.DELETE & "' " _
                & "ORDER BY POSICOL "

            'DataBase接続文字
            Using SQLcon As New SqlConnection(sm.DBCon),
                  SQLcmd As New SqlCommand(SQLStr, SQLcon)
                SQLcon.Open() 'DataBase接続(Open)
                Dim param As SqlParameter = SQLcmd.Parameters.Add("@PROFID", SqlDbType.NVarChar)
                With SQLcmd.Parameters
                    .Add("@CAMPCODE", SqlDbType.NVarChar).Value = Me.CAMPCODE
                    .Add("@MAPID", SqlDbType.NVarChar).Value = Me.MAPID
                    .Add("@VARIANT", SqlDbType.NVarChar).Value = Me.VARI

                    .Add("@STYMD", SqlDbType.Date).Value = TARGETDATE
                    .Add("@ENDYMD", SqlDbType.Date).Value = TARGETDATE
                End With
                'セッション変数のPROFIDでデータを取得し、取得できない場合は'Default'で検索
                For Each key As String In {PROFID, C_DEFAULT_DATAKEY}
                    param.Value = key '動的パラメータに値を設定
                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        If SQLdr.HasRows = True Then
                            profTbl.Load(SQLdr)
                            Exit For
                        End If
                    End Using
                Next

            End Using

            'タイトル表示設定
            If IsNothing(Me.TITLEOPT) Then
                Me.TITLEOPT = False
            End If

            ' テーブルオブジェクト展開
            MakeTableObject(profTbl, Me.TBLOBJ)

            Me.ERR = C_MESSAGE_NO.NORMAL

        Catch ex As ArgumentNullException
            ' パラメータ（必須プロパティ）例外

            Me.ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = Me.GetType.Name             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = ex.ParamName
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

        Catch ex As Exception
            ' その他例外（基本的にDBエラー）

            Me.ERR = C_MESSAGE_NO.DB_ERROR

            Dim CS0011LOGWrite As New CS0011LOGWrite
            CS0011LOGWrite.INFSUBCLASS = Me.GetType.Name      'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:GRS0010_PROFVIEW Select"                  '
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT                              '
            CS0011LOGWrite.TEXT = ex.Message
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                     'ログ出力

        Finally

        End Try

    End Sub

    ''' <summary>
    ''' テーブルオブジェクト展開
    ''' </summary>
    ''' <param name="profTbl">PROFVIEWデータ</param>
    ''' <param name="outArea">出力先(Panel)コントロール</param>
    Private Sub MakeTableObject(ByRef profTbl As DataTable, ByRef outArea As Object)

        '●項目定義取得
        Dim outTHCell = New TableHeaderCell With {.ViewStateMode = UI.ViewStateMode.Disabled}
        Dim outLineFunc As String
        Dim flgFixCol As String = "ON"
        Dim lenghtFix As Integer = 0
        Dim lenghtFixAll As Integer = 0
        Dim rightLengthFixAll As Integer = 0

        'テーブルに属性追加
        outArea.Attributes.Add("data-generated", "1")
        outArea.Attributes.Add("data-scrolltype", Me.SCROLLTYPE)
        outArea.Attributes.Add("data-usersort", Convert.ToString(Me.USERSORTOPT))

        'ソートキー領域作成
        Dim sortItemId As String = "hdnListSortValue" & outArea.Page.Form.ClientID & outArea.ID
        Dim sortValue As String = ""
        Dim sortItems As New HiddenField With {.ID = sortItemId, .ViewStateMode = UI.ViewStateMode.Disabled}
        If outArea.Page.Request.Form.GetValues(sortItemId) IsNot Nothing Then
            sortValue = outArea.Page.Request.Form.GetValues(sortItemId)(0)
        End If
        sortItems.Value = sortValue
        outArea.Controls.Add(sortItems)

        ' ヘッダー作成（左）
        Dim outPanelL = New Panel With {.ViewStateMode = UI.ViewStateMode.Disabled}
        outPanelL.ID = Trim(outArea.ID) & "_HL"
        Dim outTableL = New Table() With {.ViewStateMode = UI.ViewStateMode.Disabled}
        Dim outTHeaderL = New TableHeaderRow With {.ViewStateMode = UI.ViewStateMode.Disabled}
        outTHeaderL = New TableHeaderRow With {.ViewStateMode = UI.ViewStateMode.Disabled}

        ' ヘッダー作成（右）
        Dim outPanelR = New Panel With {.ViewStateMode = UI.ViewStateMode.Disabled}
        outPanelR.ID = Trim(outArea.ID) & "_HR"
        Dim outTableR = New Table() With {.ViewStateMode = UI.ViewStateMode.Disabled}
        Dim outTHeaderR = New TableHeaderRow With {.ViewStateMode = UI.ViewStateMode.Disabled}
        outTHeaderR = New TableHeaderRow With {.ViewStateMode = UI.ViewStateMode.Disabled}

        If Me.HIDENOOPT = False Then
            outTHCell = New TableHeaderCell With {.ViewStateMode = UI.ViewStateMode.Disabled}
            outTHCell.Attributes.Add("cellfieldname", "LINECNT")
            outTHCell.Text = "項番"
            If Me.NOCOLUMNWIDTHOPT = 0 Then
                lenghtFix = 32
            ElseIf Me.NOCOLUMNWIDTHOPT < 0 Then
                lenghtFix = 0
            Else
                lenghtFix = Me.NOCOLUMNWIDTHOPT
            End If
            If lenghtFix = 0 Then
                outTHCell.Style.Add("display", "none")
            Else
                outTHCell.Style.Add("width", lenghtFix.ToString & "px")
            End If

            lenghtFixAll = lenghtFixAll + lenghtFix + If(lenghtFix = 0, 0, 2) '内報するテーブルの左右ボーダー2px分も加味

            outTHeaderL.Cells.Add(outTHCell)
        End If

        If Me.HIDEOPERATIONOPT = False Then
            outTHCell = New TableHeaderCell With {.ViewStateMode = UI.ViewStateMode.Disabled}
            outTHCell.Attributes.Add("cellfieldname", "OPERATION")
            outTHCell.Text = "操作"
            If Me.OPERATIONCOLUMNWIDTHOPT = 0 Then
                lenghtFix = 48
            ElseIf Me.OPERATIONCOLUMNWIDTHOPT < 0 Then
                lenghtFix = 0
            Else
                lenghtFix = Me.OPERATIONCOLUMNWIDTHOPT
            End If
            If lenghtFix = 0 Then
                outTHCell.Style.Add("display", "none")
            Else
                outTHCell.Style.Add("width", lenghtFix.ToString & "px")
            End If

            lenghtFixAll = lenghtFixAll + lenghtFix + If(lenghtFix = 0, 0, 2) '内報するテーブルの左右ボーダー2px分も加味
            outTHeaderL.Cells.Add(outTHCell)
        End If

        'ヘッダー部可変域作成
        For Each profRow In profTbl.Rows

            If profRow("EFFECT").ToString = "N" Then
                Continue For
            End If

            outTHCell = New TableHeaderCell With {.ViewStateMode = UI.ViewStateMode.Disabled}
            outTHCell.Attributes.Add("cellfieldname", profRow("FIELD").ToString)

            '必須項目
            If profRow("FIELDNAMES").ToString.StartsWith("*") Then
                outTHCell.Text = "<span class=""textLeft requiredMark2"" > "
                outTHCell.Text = outTHCell.Text & Replace(profRow("FIELDNAMES").ToString, "*", "", 1, 1)
                outTHCell.Text = outTHCell.Text & "</span>"
            Else
                outTHCell.Text = Convert.ToString(profRow("FIELDNAMES"))
            End If

            If TITLEOPT = True Then
                outTHCell.Attributes.Add("Title", profRow("FIELDNAMES").ToString)
            End If
            If Convert.ToString(profRow("OBJECTTYPE")) = "2" Then 'TextBox
                lenghtFix = (CInt(profRow("WIDTH")) * 16) + 16
            Else
                lenghtFix = (CInt(profRow("WIDTH")) * 16)
            End If

            If profRow("COLORSET").ToString.Trim <> "" Then
                outTHCell.Attributes.Add("data-colorset", profRow("COLORSET").ToString.Trim)
            End If

            If Convert.ToString(profRow("FIXCOL")) = "1" AndAlso flgFixCol = "ON" Then
                If lenghtFix = 0 Then
                    outTHCell.Style.Add("display", "none")
                Else
                    outTHCell.Style.Add("width", lenghtFix.ToString & "px")
                End If

                outTHeaderL.Cells.Add(outTHCell)
                lenghtFixAll = lenghtFixAll + lenghtFix + If(lenghtFix = 0, 0, 2) '内報するテーブルの左右ボーダー2px分も加味
            Else
                flgFixCol = "OFF"
                If (CInt(profRow("WIDTH")) * 16) = 0 Then
                    outTHCell.Style.Add("display", "none")
                Else
                    outTHCell.Style.Add("width", lenghtFix.ToString & "px")
                End If
                outTHeaderR.Cells.Add(outTHCell)
                rightLengthFixAll = rightLengthFixAll + lenghtFix + If(lenghtFix = 0, 0, 2) '内報するテーブルの左右ボーダー2px分も加味
            End If

            'イベント存在時は表題に下線追加
            Dim eventString As String = ""
            For funcCnt = 1 To 5
                Dim eventFieldName As String = String.Format("ADDEVENT{0}", funcCnt).Trim
                eventString &= eventFieldName.ToLower
            Next funcCnt
            If eventString.Contains("ondblclick") Then
                outTHCell.Attributes.Add("style", "text-decoration: underline;")
            End If

        Next
        SetSortFunction(outTHeaderL, outArea.ID, sortValue)
        outTableL.Rows.Add(outTHeaderL)
        outTableL.Style.Add("width", lenghtFixAll.ToString & "px")
        outPanelL.Style.Add("width", lenghtFixAll.ToString & "px")
        outPanelL.Controls.Add(outTableL)
        outArea.Controls.Add(outPanelL)

        SetSortFunction(outTHeaderR, outArea.ID, sortValue)
        outTableR.Rows.Add(outTHeaderR)
        outPanelR.Style.Add("left", lenghtFixAll.ToString & "px")
        outTableR.Style.Add("width", rightLengthFixAll.ToString & "px")
        outPanelR.Controls.Add(outTableR)
        outArea.Controls.Add(outPanelR)

        ' データ（左）
        outPanelL = New Panel With {.ViewStateMode = UI.ViewStateMode.Disabled}
        outPanelL.ID = Trim(outArea.ID) & "_DL"
        outTableL = New Table() With {.ViewStateMode = UI.ViewStateMode.Disabled}
        Dim outTDataL = New TableHeaderRow With {.ViewStateMode = UI.ViewStateMode.Disabled}
        outPanelR = New Panel With {.ViewStateMode = UI.ViewStateMode.Disabled}
        outPanelR.ID = Trim(outArea.ID) & "_DR"
        outTableR = New Table() With {.ViewStateMode = UI.ViewStateMode.Disabled}
        Dim outTDataR = New TableRow With {.ViewStateMode = UI.ViewStateMode.Disabled}
        Dim lineCnt As Integer = 0
        For Each dataRow In Me.SRCDATA.Rows
            lineCnt += 1

            outTDataL = New TableHeaderRow With {.ViewStateMode = UI.ViewStateMode.Disabled}

            outTDataR = New TableRow With {.ViewStateMode = UI.ViewStateMode.Disabled}
            flgFixCol = "ON"
            '固定列番号
            Dim leftColNum As Integer = 0
            '移動列番号
            Dim rightColNum As Integer = 0
            '固定項目編集
            If Me.HIDENOOPT = False Then
                '項番
                outTHCell = New TableHeaderCell With {.ViewStateMode = UI.ViewStateMode.Disabled}
                outTHCell.Text = Convert.ToString(dataRow("LINECNT"))

                Dim cellWidth As String = "32px"
                If Me.NOCOLUMNWIDTHOPT <> 0 Then
                    cellWidth = Me.NOCOLUMNWIDTHOPT & "px"
                End If
                If Me.NOCOLUMNWIDTHOPT < 0 Then
                    outTHCell.Style.Add("display", "none")
                Else
                    outTHCell.Style.Add("width", cellWidth)
                End If

                If WITHTAGNAMES Then outTHCell.Attributes.Add("name", "L_LINECNT_" & lineCnt)
                leftColNum += 1

                outTHCell.Style.Add("text-align", "center")
                outTDataL.Cells.Add(outTHCell)
            End If

            If Me.HIDEOPERATIONOPT = False Then
                '操作
                outTHCell = New TableHeaderCell With {.ViewStateMode = UI.ViewStateMode.Disabled}

                'outTHCell.Style.Value = STYLE_TH
                outTHCell.Text = Convert.ToString(dataRow("OPERATION"))

                Dim cellWidth As String = "48px"
                If Me.OPERATIONCOLUMNWIDTHOPT <> 0 Then
                    cellWidth = Me.OPERATIONCOLUMNWIDTHOPT & "px"
                End If
                If Me.OPERATIONCOLUMNWIDTHOPT < 0 Then
                    outTHCell.Style.Add("display", "none")
                Else
                    outTHCell.Style.Add("width", cellWidth)
                End If

                If WITHTAGNAMES Then outTHCell.Attributes.Add("name", "L_OPERATION" & lineCnt)
                leftColNum += 1

                outTHCell.Style.Add("text-align", "center")
                outTDataL.Cells.Add(outTHCell)
            End If

            '文字色を設定
            If Me.SRCDATA.Columns.Contains("FONTCOLOR") AndAlso Trim(Convert.ToString(dataRow("FONTCOLOR"))) <> "" Then
                outTDataL.Style.Add("color", Trim(Convert.ToString(dataRow("FONTCOLOR"))))
                outTDataR.Style.Add("color", Trim(Convert.ToString(dataRow("FONTCOLOR"))))
            End If

            For Each profRow In profTbl.Rows

                If profRow("EFFECT").ToString = "N" Then
                    Continue For
                End If

                Dim outCell = New TableCell With {.ViewStateMode = UI.ViewStateMode.Disabled}
                Dim fieldName As String = Convert.ToString(profRow("FIELD"))
                Select Case Convert.ToString(profRow("OBJECTTYPE"))
                    Case "0", String.Empty 'Default
                        Select Case Convert.ToString(profRow("FORMATTYPE"))
                            Case "D"    '日付フォーマット
                                If Convert.ToString(dataRow(fieldName)) <> "" Then
                                    If IsDBNull(profRow("FORMATVALUE")) OrElse String.IsNullOrEmpty(Convert.ToString(profRow("FORMATVALUE"))) Then
                                        outCell.Text = CDate(dataRow(fieldName)).ToString(Convert.ToString(HttpContext.Current.Session("DateFormat")))
                                    Else
                                        outCell.Text = CDate(dataRow(fieldName)).ToString(Convert.ToString(profRow("FORMATVALUE")))
                                    End If
                                Else
                                    outCell.Text = Convert.ToString(dataRow(fieldName))
                                End If
                            Case "F"    '小数桁

                            Case Else
                                outCell.Text = Convert.ToString(dataRow(fieldName))
                        End Select
                        'outCell.Text = I_SRCDATA(i)(fieldName)
                        If TITLEOPT = True Then
                            outCell.Attributes.Add("Title", outCell.Text)
                        End If
                        outCell.Style.Add("text-align", Convert.ToString(profRow("ALIGN")))
                    Case "1" 'CheckBox
                        Dim outCheckBox As CheckBox
                        Dim outHidden = New Label With {.ViewStateMode = UI.ViewStateMode.Disabled}
                        outCheckBox = New CheckBox() With {.ViewStateMode = UI.ViewStateMode.Disabled}

                        outCheckBox.Attributes.Add("rownum", (Integer.Parse(outTDataL.Cells(0).Text)).ToString)
                        outCheckBox.ID = "chk" & Me.TBLOBJ.ID & fieldName & (Integer.Parse(outTDataL.Cells(0).Text)).ToString
                        outCell.Controls.Add(outCheckBox)
                        outHidden.ID = "hchk" & Me.TBLOBJ.ID & fieldName & (Integer.Parse(outTDataL.Cells(0).Text)).ToString
                        outHidden.Text = Convert.ToString(dataRow(fieldName))
                        outHidden.Style.Add("display", "none")
                        outCell.Controls.Add(outHidden)
                        outCell.Style.Add("text-align", Convert.ToString(profRow("ALIGN")))
                    Case "2" 'TextBox
                        Dim outTextBox As TextBox
                        outTextBox = New TextBox With {.ViewStateMode = UI.ViewStateMode.Disabled}
                        Dim textValue As String = ""
                        Select Case Convert.ToString(profRow("FORMATTYPE"))
                            Case "D"    '日付フォーマット
                                If Convert.ToString(dataRow(fieldName)) <> "" Then
                                    If IsDBNull(profRow("FORMATVALUE")) OrElse String.IsNullOrEmpty(Convert.ToString(profRow("FORMATVALUE"))) Then
                                        textValue = CDate(dataRow(fieldName)).ToString(Convert.ToString(HttpContext.Current.Session("DateFormat")))
                                    Else
                                        textValue = CDate(dataRow(fieldName)).ToString(Convert.ToString(profRow("FORMATVALUE")))
                                    End If
                                Else
                                    textValue = Convert.ToString(dataRow(fieldName))
                                End If
                            Case "F"    '小数桁
                            Case Else
                                textValue = Convert.ToString(dataRow(fieldName))
                        End Select
                        Dim textTagBase = "<input id=""{0}"" name=""{0}"" style=""width:{1};font-size:{2};height:{3};text-align:{4};"" type=""text"" rownum=""{5}"" value=""{6}"">"

                        Dim textTagString = String.Format(textTagBase,
                                                          "txt" & Me.TBLOBJ.ID & Convert.ToString(fieldName) & (Integer.Parse(outTDataL.Cells(0).Text)).ToString,
                                                          (CInt(profRow("WIDTH")) * 16).ToString & "px",
                                                          "small",
                                                          "16px",
                                                          profRow("ALIGN"),
                                                          (Integer.Parse(outTDataL.Cells(0).Text)).ToString,
                                                          HttpUtility.HtmlEncode(textValue))
                        outCell.EnableViewState = False
                        outCell.Text = textTagString
                        outCell.Style.Add("text-align", "center")
                    Case "3" 'Button
                        Dim outButton As HtmlButton
                        outButton = New HtmlButton() With {.ViewStateMode = UI.ViewStateMode.Disabled}
                        outButton.Attributes.Add("rownum", (Integer.Parse(outTDataL.Cells(0).Text)).ToString)
                        outButton.ID = "btn" & Me.TBLOBJ.ID & fieldName & (Integer.Parse(outTDataL.Cells(0).Text)).ToString
                        outCell.Controls.Add(outButton)
                        outCell.Style.Add("text-align", Convert.ToString(profRow("ALIGN")))
                    Case "4" 'RadioButton
                        If Not String.IsNullOrEmpty(Convert.ToString(profRow("FORMATTYPE"))) Then
                            Dim classKey As String = Convert.ToString(profRow("FORMATTYPE"))
                            Dim formatvalue As String = Convert.ToString(profRow("FORMATVALUE"))
                            Dim list As ListBox = Nothing
                            If getFixVal(classKey, formatvalue, list) = True Then
                                Dim outRadioList = New RadioButtonList With {.ViewStateMode = UI.ViewStateMode.Disabled}
                                Dim outHidden = New Label With {.ViewStateMode = UI.ViewStateMode.Disabled}
                                For Each item As ListItem In list.Items
                                    outRadioList.Items.Add(item)
                                Next
                                outRadioList.RepeatDirection = RepeatDirection.Horizontal
                                outRadioList.RepeatLayout = RepeatLayout.Flow

                                outRadioList.ID = "rbl" & classKey & fieldName & (Integer.Parse(outTDataL.Cells(0).Text)).ToString
                                outHidden.ID = "lrbl" & classKey & fieldName & (Integer.Parse(outTDataL.Cells(0).Text)).ToString
                                outHidden.Text = Convert.ToString(dataRow(fieldName))
                                outHidden.Style.Add("display", "none")
                                outCell.Controls.Add(outRadioList)
                                outCell.Controls.Add(outHidden)
                                outCell.Style.Add("text-align", Convert.ToString(profRow("ALIGN")))
                            End If
                        End If
                    Case "5" 'ListBox
                        If Not String.IsNullOrEmpty(Convert.ToString(profRow("FORMATTYPE"))) Then
                            Dim classKey As String = Convert.ToString(profRow("FORMATTYPE"))
                            Dim formatvalue As String = Convert.ToString(profRow("FORMATVALUE"))

                            Dim list As ListBox = Nothing
                            If getFixVal(classKey, formatvalue, list) = True Then
                                Dim outList = New ListBox With {.ViewStateMode = UI.ViewStateMode.Disabled}
                                For Each item As ListItem In list.Items
                                    outList.Items.Add(item)
                                Next
                                outList.Rows = 1

                                outList.SelectionMode = ListSelectionMode.Single

                                outList.ClearSelection()

                                For Each item As ListItem In outList.Items
                                    If item.Value = Convert.ToString(dataRow(fieldName)) Then
                                        item.Selected = True
                                    End If
                                Next
                                If IsNothing(outList.SelectedItem) Then
                                    outList.Items(0).Selected = True
                                End If
                                outList.ID = "lb" & classKey & fieldName & (Integer.Parse(outTDataL.Cells(0).Text)).ToString
                                outCell.Controls.Add(outList)
                                outCell.Style.Add("text-align", Convert.ToString(profRow("ALIGN")))
                            End If
                        End If
                    Case Else
                End Select
                'テーブルセルのサイズ
                If CInt(profRow("WIDTH")) * 16 = 0 Then
                    outCell.Style.Add("display", "none")
                Else
                    Dim cellWidth As String = ((CInt(profRow("WIDTH")) * 16) + If(Convert.ToString(profRow("OBJECTTYPE")) = "2", 16, 0)).ToString
                    outCell.Style.Add("width", cellWidth & "px")
                End If
                'イベント追加
                For funcCnt = 1 To 5
                    Dim eventFieldName As String = String.Format("ADDEVENT{0}", funcCnt)
                    Dim funcFieldName As String = String.Format("ADDFUNC{0}", funcCnt)
                    If Convert.ToString(profRow(eventFieldName)) <> "" AndAlso Convert.ToString(profRow(funcFieldName)) <> "" Then
                        Dim outCellFunc As String
                        outCellFunc = Convert.ToString(profRow(funcFieldName)) & "(this," & (Integer.Parse(outTDataL.Cells(0).Text)).ToString & ",'" & fieldName & "');"
                        outCell.Attributes.Add(Convert.ToString(profRow(eventFieldName)), outCellFunc)
                    End If
                Next funcCnt
                '色変更定義の追加
                If profRow("COLORSET").ToString.Trim <> "" Then
                    outCell.Attributes.Add("data-colorset", profRow("COLORSET").ToString.Trim)
                End If

                '生成したセルの追加先
                If Convert.ToString(profRow("FIXCOL")) = "1" AndAlso flgFixCol = "ON" Then
                    If WITHTAGNAMES Then outCell.Attributes.Add("name", "L_" & fieldName & Integer.Parse(outTDataL.Cells(0).Text))
                    leftColNum += 1

                    outTDataL.Cells.Add(outCell)
                Else
                    If WITHTAGNAMES Then outCell.Attributes.Add("name", "R_" & fieldName & Integer.Parse(outTDataL.Cells(0).Text))
                    rightColNum += 1

                    outTDataR.Cells.Add(outCell)
                End If
            Next


            If Not String.IsNullOrEmpty(Me.LEVENT) AndAlso Not String.IsNullOrEmpty(Me.LFUNC) Then
                outLineFunc = Me.LFUNC & "(this," & (Integer.Parse(outTDataL.Cells(0).Text)).ToString & ");"
                outTDataL.Attributes.Add(Me.LEVENT, outLineFunc)
                outTDataR.Attributes.Add(Me.LEVENT, outLineFunc)
            End If

            outTableL.Rows.Add(outTDataL)
            outTableR.Rows.Add(outTDataR)

        Next

        outPanelL.Style.Add("width", lenghtFixAll.ToString & "px")
        outTableL.Style.Add("width", lenghtFixAll.ToString & "px")
        outPanelL.Controls.Add(outTableL)

        outPanelR.Style.Add("left", lenghtFixAll.ToString & "px")
        outTableR.Style.Add("width", rightLengthFixAll.ToString & "px")
        outPanelR.Controls.Add(outTableR)

        outArea.Controls.Add(outPanelL)
        outArea.Controls.Add(outPanelR)

    End Sub

    ''' <summary>
    ''' Sort関数紐づけ
    ''' </summary>
    ''' <param name="headerRow"></param>
    Private Sub SetSortFunction(ByRef headerRow As TableHeaderRow, ByVal parentPanelId As String, ByVal currentSortString As String)
        'ユーザーソートオプションなし、ヘッダーオブジェクト未存在じはそのまま終了
        If Me.USERSORTOPT = 0 OrElse headerRow Is Nothing OrElse headerRow.Cells.Count = 0 Then
            Return
        End If
        Dim currentSortArray = currentSortString.Split(","c)
        Dim dicCurrentSort As New Dictionary(Of String, String)
        If currentSortString <> "" AndAlso currentSortArray IsNot Nothing AndAlso currentSortArray.Count >= 0 Then
            For Each sortKeyOrder In currentSortArray
                Dim sortKey As String = sortKeyOrder.Trim.Split(" "c)(0)
                Dim sortOrder As String = sortKeyOrder.Trim.Split(" "c)(1)
                dicCurrentSort.Add(sortKey, sortOrder)
            Next
        End If

        For Each headerCell In headerRow.Cells
            Dim tabCell As TableCell = Nothing
            If TypeOf headerCell Is TableCell Then
                tabCell = DirectCast(headerCell, TableCell)
            Else
                Continue For
            End If
            If tabCell.Style.Item("display") IsNot Nothing AndAlso tabCell.Style.Item("display") = "none" Then
                Continue For
            End If
            Dim appendScriptText = "<span onclick='commonListSortClick(""{0}"",""{1}"");' class='listSort {2}'>{3}</span>"
            Dim sortOrder As String = ""
            Dim fieldName As String = tabCell.Attributes("cellfieldname")
            If dicCurrentSort.ContainsKey(fieldName) Then
                sortOrder = dicCurrentSort(fieldName)
            End If
            tabCell.Text = String.Format(appendScriptText, parentPanelId, fieldName, sortOrder, tabCell.Text)
        Next
    End Sub
    ''' <summary>
    ''' ソートしたデータテーブルを返却
    ''' </summary>
    ''' <param name="dt">全件データを格納したデータテーブル</param>
    ''' <param name="pnlObj"></param>
    ''' <param name="hdnListPositionObj"></param>
    ''' <param name="additionalFilterString"></param>
    ''' <returns></returns>
    Public Shared Function GetSortedDatatable(dt As DataTable, pnlObj As Panel,
                                              Optional dispRowCount As Integer = 0,
                                              Optional listPosition As Integer = 0,
                                              Optional hdnListPositionObj As HiddenField = Nothing,
                                              Optional additionalFilterString As String = "") As DataTable
        Dim sortedDt As DataTable = dt.Clone '行データのコピーはなく入力のデータテーブルのガワを作成
        '対象のテーブルが未存在またはレコード1件の場合はソートの意味がないので何もせず終了
        If dt Is Nothing OrElse dt.Rows.Count <= 1 Then
            Return dt
        End If
        '現在設定のソート順を取得
        Dim sortValue As String = ""
        Dim sortItemId As String = "hdnListSortValue" & pnlObj.Page.Form.ClientID & pnlObj.ID
        If pnlObj.Page.Request.Form.GetValues(sortItemId) IsNot Nothing Then
            sortValue = pnlObj.Page.Request.Form.GetValues(sortItemId)(0)
        End If
        If sortValue = "" Then
            sortValue = "LINECNT"
        End If
        Using dvw As DataView = New DataView(dt)
            dvw.Sort = sortValue
            dvw.RowFilter = "HIDDEN= '0'" & If(additionalFilterString = "", "", " AND " & additionalFilterString)
            If dvw.Count <> 0 Then
                If dispRowCount <> 0 Then
                    If dvw.Count >= listPosition Then
                        For idx As Integer = listPosition - 1 To (listPosition - 1) + dispRowCount
                            Dim dr As DataRow = sortedDt.NewRow
                            dr.ItemArray = dvw(idx).Row.ItemArray
                            sortedDt.Rows.Add(dr)
                            If dvw.Count = idx + 1 Then
                                Exit For
                            End If
                        Next
                        hdnListPositionObj.Value = Convert.ToString(listPosition)
                    Else
                        hdnListPositionObj.Value = "1"
                    End If
                Else
                    sortedDt = dvw.ToTable
                End If
            End If

        End Using

        Return sortedDt
    End Function

    Private Function getFixVal(ByVal classKey As String, ByVal formatvalue As String, ByRef itemList As ListBox) As Boolean

        Dim wkList As ListBox = New ListBox

        If String.IsNullOrEmpty(formatvalue) Then
            formatvalue = "VALUE1"
        End If

        If lmp.ContainsKey(classKey & formatvalue) Then
            Dim lst As ListBox = lmp.Item(classKey & formatvalue)

            For Each item As ListItem In lst.Items
                wkList.Items.Add(New ListItem(item.Text, item.Value))
            Next
            itemList = wkList
            Return True
        Else
            Dim formats As String() = formatvalue.Split(",")
            Dim where As String = ""
            Dim keyvalue As String = ""
            For Each value As String In formats
                If value.Contains("=") OrElse value.Contains("<") OrElse value.Contains(">") Then
                    where = where & " and " & value
                Else
                    keyvalue = value
                End If
            Next
            Try
                'DataBase接続文字
                Dim SQLcon = sm.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                Dim SQLStr As String = String.Empty
                SQLStr =
                      " SELECT                           " _
                    & "      rtrim(KEYCODE) as KEYCODE , " _
                    & "      rtrim(VALUE1)  as VALUE1  , " _
                    & "      rtrim(VALUE2)  as VALUE2  , " _
                    & "      rtrim(VALUE3)  as VALUE3  , " _
                    & "      rtrim(VALUE4)  as VALUE4  , " _
                    & "      rtrim(VALUE5)  as VALUE5    " _
                    & " FROM  oil.MC001_FIXVALUE             " _
                    & " Where CAMPCODE  = @P1 " _
                    & "   and CLASS     = @P2 " _
                    & "   and STYMD    <= @P3 " _
                    & "   and ENDYMD   >= @P4 " _
                    & "   and DELFLG   <> @P5 " _
                    & where _
                    & " ORDER BY KEYCODE "

                Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.NVarChar, 1)
                PARA1.Value = CAMPCODE
                PARA2.Value = classKey
                PARA3.Value = Date.Now
                PARA4.Value = Date.Now
                PARA5.Value = C_DELETE_FLG.DELETE
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                While SQLdr.Read
                    If SQLdr("KEYCODE") <> "" Then
                        wkList.Items.Add(New ListItem(SQLdr(keyvalue), SQLdr("KEYCODE")))
                    End If
                End While

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

                SQLcmd.Dispose()
                SQLcmd = Nothing

                SQLcon.Close() 'DataBase接続(Close)
                SQLcon.Dispose()
                SQLcon = Nothing

                lmp.Add(classKey & formatvalue, wkList)

                itemList = wkList
                Return True

            Catch ex As Exception
                ERR = C_MESSAGE_NO.DB_ERROR
                Return False
            End Try
        End If

    End Function

End Class