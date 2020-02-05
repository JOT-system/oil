Option Strict On
Option Explicit On
Imports System.Drawing

''' <summary>
'''  リピーター用Tableオブジェクト展開
''' </summary>
''' <remarks>GB.COA0014DetailViewから修正</remarks>
Public Class CS0052DetailView
    '画面実行URL取得dll Interface
    Private Const CONST_MEISAINO As String = "MEISAINO"             '非表示項目(左Box処理用・Repeater内行位置)
    Private Const CONST_LINEPOSITION As String = "LINEPOSITION"     '非表示項目(左Box処理用・Repeater内行位置)
    Private Const CONST_FIELDNM As String = "FIELDNM_"  '項目(名称)
    Private Const CONST_FIELD As String = "FIELD_"      '項目(記号名)
    Private Const CONST_VALUE As String = "VALUE_"      '値
    Private Const CONST_TEXT As String = "VALUE_TEXT_"  '値（名称）
    Private Const CONST_LABLE1 As String = "Label1_"    'ラベル（必須区分）

    ''' <summary>
    ''' [IN]会社コードプロパティ
    ''' </summary>
    ''' <returns>[IN]CAMPCODE</returns>
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' [IN]PROFIDプロパティ
    ''' </summary>
    ''' <returns>[IN]USERID</returns>
    Public Property PROFID() As String
    ''' <summary>
    ''' [IN]MAPIDプロパティ
    ''' </summary>
    ''' <returns>[IN]MAPID</returns>
    Public Property MAPID As String
    ''' <summary>
    ''' [IN]変数プロパティ
    ''' </summary>
    ''' <returns>[IN]変数</returns>
    Public Property VARI As String
    ''' <summary>
    ''' [IN]TABIDプロパティ
    ''' </summary>
    ''' <returns>[IN]TABID</returns>
    Public Property TABID As String
    ''' <summary>
    ''' [IN]元データテーブルプロパティ
    ''' </summary>
    ''' <returns>[IN]元データテーブル</returns>
    Public Property SRCDATA As DataTable
    ''' <summary>
    ''' [IN]展開対象リピーターオブジェクトプロパティ
    ''' </summary>
    ''' <returns>[IN]展開対象リピーターオブジェクト</returns>
    Public Property REPEATER() As Repeater
    ''' <summary>
    ''' [IN]カラムプレフィックスプロパティ
    ''' </summary>
    ''' <returns>[IN]カラムプレフィックス</returns>
    Public Property COLPREFIX() As String

    ''' <summary>
    ''' [OUT]最大行数プロパティ
    ''' </summary>
    ''' <returns>[OUT]最大行数</returns>
    Public Property ROWMAX As Integer
    ''' <summary>
    ''' [OUT]最大カラム数プロパティ
    ''' </summary>
    ''' <returns>[OUT]最大カラム</returns>
    Public Property COLMAX As Integer

    ''' <summary>
    ''' [OUT]ERRNoプロパティ
    ''' </summary>
    ''' <returns>[OUT]ERRNo</returns>
    Public Property ERR() As String

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
        TABID = String.Empty
        SRCDATA = Nothing
        REPEATER = Nothing
        COLPREFIX = String.Empty

        ROWMAX = 0
        COLMAX = 0
        ERR = C_MESSAGE_NO.NORMAL

    End Sub

    ''' <summary>
    ''' リピーター用Tableオブジェクト展開
    ''' </summary>
    ''' <remarks> OK:00000</remarks> 
    Public Sub MaketDetailView()

        Dim CS0029ProfViewD As New CS0029ProfViewD        'UPROFview・Detail取得
        Dim workValue As TextBox
        Dim workFieldnm As Label
        Dim workMeisaino As String = "000"

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
            If IsNothing(REPEATER) Then
                Throw New ArgumentNullException("REPEATER")
            End If

            Dim outRep As Repeater = Me.REPEATER

            CS0029ProfViewD.CAMPCODE = Me.CAMPCODE
            CS0029ProfViewD.PROFID = Me.PROFID
            CS0029ProfViewD.MAPID = Me.MAPID
            CS0029ProfViewD.VARI = Me.VARI
            CS0029ProfViewD.TABID = Me.TABID
            CS0029ProfViewD.CS0029ProfViewD()
            If CS0029ProfViewD.ERR = C_MESSAGE_NO.NORMAL Then

                Dim workRepTbl As DataTable = New DataTable
                For k As Integer = 1 To (Me.SRCDATA.Rows.Count * CS0029ProfViewD.ROWMAX)
                    Dim workRepRow As DataRow = workRepTbl.NewRow()
                    workRepTbl.Rows.Add(workRepRow)
                Next
                outRep.DataSource = workRepTbl
                outRep.DataBind()  'Bind処理記述を行っていないので空行だけ作成される。
                Dim i As Integer = 0
                Dim m As Integer = 0

                For k As Integer = 0 To Me.SRCDATA.Rows.Count - 1

                    For i = 0 To CS0029ProfViewD.ROWMAX - 1

                        m = (CS0029ProfViewD.ROWMAX * k) + i
                        If (m + 1) <= (CS0029ProfViewD.ROWMAX * Me.SRCDATA.Rows.Count) Then
                            workMeisaino = ((m \ CS0029ProfViewD.ROWMAX) + 1).ToString("000")
                            DirectCast(outRep.Items(m).FindControl(Me.COLPREFIX & CONST_MEISAINO), System.Web.UI.WebControls.TextBox).Text = workMeisaino
                        Else
                            DirectCast(outRep.Items(m).FindControl(Me.COLPREFIX & CONST_MEISAINO), System.Web.UI.WebControls.TextBox).Text = ""
                        End If

                        DirectCast(outRep.Items(m).FindControl(Me.COLPREFIX & CONST_LINEPOSITION), System.Web.UI.WebControls.TextBox).Text = Convert.ToString(m)

                        For j As Integer = 1 To CS0029ProfViewD.COLMAX
                            Dim effect = Convert.ToString(CS0029ProfViewD.TABLEDATA.Rows(i)("EFFECT_" & (j).ToString))
                            If effect <> "" AndAlso effect <> "N" Then

                                '項目(名称)　
                                workFieldnm = DirectCast(outRep.Items(m).FindControl(Me.COLPREFIX & CONST_FIELDNM & j), System.Web.UI.WebControls.Label)
                                workFieldnm.Text = Convert.ToString(CS0029ProfViewD.TABLEDATA.Rows(i)(CONST_FIELDNM & j))
                                workFieldnm.Attributes.Add("Title", workFieldnm.Text)
                                '必須入力
                                If Convert.ToString(CS0029ProfViewD.TABLEDATA.Rows(i)("REQUIRED_" & (j).ToString)) = "1" Then
                                    DirectCast(outRep.Items(m).FindControl(Me.COLPREFIX & CONST_LABLE1 & j), System.Web.UI.WebControls.Label).Text = "*"
                                End If
                                '項目(記号名)
                                DirectCast(outRep.Items(m).FindControl(Me.COLPREFIX & CONST_FIELD & j), System.Web.UI.WebControls.Label).Text = Convert.ToString(CS0029ProfViewD.TABLEDATA.Rows(i)(CONST_FIELD & j))
                                '値
                                workValue = DirectCast(outRep.Items(m).FindControl(Me.COLPREFIX & CONST_VALUE & j), System.Web.UI.WebControls.TextBox)
                                If Convert.ToString(CS0029ProfViewD.TABLEDATA.Rows(i)("FIELD_" & (j).ToString)) = "" Then
                                    workValue.ReadOnly = True
                                    workValue.Style.Remove("border")
                                    workValue.Style.Add("border", "1px solid rgb(220,230,240)")
                                    workValue.BackColor = Color.FromArgb(220, 230, 240)
                                    workValue.Style.Remove("background-color")
                                    workValue.Style.Add("background-color", "rgb(220,230,240)")
                                Else
                                    If effect = "Y" OrElse effect = "C" Then
                                        workValue.Style.Remove("visibility")
                                        workValue.Style.Add("visibility", "visible")
                                        Dim width = Convert.ToInt32(CS0029ProfViewD.TABLEDATA.Rows(i)("WIDTH_" & (j).ToString))
                                        Dim length = Convert.ToInt32(CS0029ProfViewD.TABLEDATA.Rows(i)("LENGTH_" & (j).ToString))
                                        If width < 0 Then
                                            workValue.Style.Remove("display")
                                            workValue.Style.Add("display", "none")
                                        ElseIf width > 0 Then
                                            width = width * 16
                                            workValue.Style.Remove("width")
                                            workValue.Style.Add("width", width.ToString & "px")
                                        End If
                                        If length = 0 Then
                                            workValue.ReadOnly = True
                                            workValue.Style.Remove("background-color")
                                            workValue.Style.Add("background-color", "rgb(220,230,240)")
                                            workValue.Style.Remove("border")
                                            workValue.Style.Add("border", "1px solid black")
                                        Else
                                            workValue.ReadOnly = False
                                            workValue.Style.Remove("border")
                                            workValue.Style.Add("border", "1px solid black")
                                            If length > 0 Then
                                                workValue.MaxLength = length
                                            End If

                                            For l As Integer = 1 To 5
                                                'イベント追加
                                                Dim addEvent As String = Convert.ToString(CS0029ProfViewD.TABLEDATA.Rows(i)("ADDEVENT" & l & "_" & (j).ToString))
                                                Dim addFunc As String = Convert.ToString(CS0029ProfViewD.TABLEDATA.Rows(i)("ADDFUNC" & l & "_" & (j).ToString))
                                                If addEvent <> "" AndAlso addFunc <> "" Then
                                                    Dim outCellFunc As String
                                                    outCellFunc = addFunc & "('" & Convert.ToString(CS0029ProfViewD.TABLEDATA.Rows(i)("FIELD_" & (j).ToString)) & "','" & workMeisaino & "','" & i & "');"
                                                    workValue.Attributes.Add(addEvent, outCellFunc)
                                                    If addEvent = "ondblclick" Then
                                                        'ダブルクリックイベントであれば下線を付加
                                                        workFieldnm.Attributes.Add("style", "text-decoration: underline;")
                                                    End If
                                                End If
                                            Next

                                        End If
                                    ElseIf effect = "T" Then
                                        workValue.Style.Remove("display")
                                        workValue.Style.Add("display", "none")
                                    End If
                                    Try

                                        If Not IsDBNull(Me.SRCDATA.Rows(k)(Convert.ToString(CS0029ProfViewD.TABLEDATA.Rows(i)(CONST_FIELD & j)))) Then
                                            workValue.Text = Convert.ToString(Me.SRCDATA.Rows(k)(Convert.ToString(CS0029ProfViewD.TABLEDATA.Rows(i)(CONST_FIELD & j))))
                                        End If

                                        '値（名称）
                                        Dim workValueText = DirectCast(outRep.Items(m).FindControl(Me.COLPREFIX & CONST_TEXT & j), System.Web.UI.WebControls.Label)
                                        If effect = "H" Then
                                            workValueText.Style.Remove("visibility")
                                            workValueText.Style.Add("visibility", "hidden")
                                        ElseIf effect = "C" Then
                                            workValueText.Style.Remove("display")
                                            workValueText.Style.Add("display", "none")
                                        End If
                                        'CType(outRep.Items(m).FindControl(I_COLPREFIX & CONST_TEXT & j), System.Web.UI.WebControls.Label).Text = CS0029ProfViewD.TABLEDATA.Rows(i)(CONST_TEXT & j)
                                    Catch
                                        '項目未設定の場合があるのでエラー検知しない
                                    End Try
                                End If
                            End If
                        Next
                    Next
                    '■■■ LINE表示設定（1明細目の最終行） ■■■
                    If k < (Me.SRCDATA.Rows.Count - 1) Then
                        Dim lineLabel As Label = DirectCast(outRep.Items(m).FindControl(Me.COLPREFIX & "LINE"), System.Web.UI.WebControls.Label)
                        If lineLabel IsNot Nothing Then
                            With lineLabel.Style
                                .Remove("display")
                                .Add("display", "block")
                            End With
                        End If
                    End If
                Next
            End If

            ROWMAX = CS0029ProfViewD.ROWMAX
            COLMAX = CS0029ProfViewD.COLMAX
            ERR = C_MESSAGE_NO.NORMAL

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
            ' その他例外

            Me.ERR = C_MESSAGE_NO.DB_ERROR

            Dim CS0011LOGWrite As New CS0011LOGWrite
            CS0011LOGWrite.INFSUBCLASS = Me.GetType.Name      'SUBクラス名
            CS0011LOGWrite.INFPOSI = "REPETER: CREATE"                  '
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT                              '
            CS0011LOGWrite.TEXT = ex.Message
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                     'ログ出力

        Finally

        End Try

    End Sub

    ''' <summary>
    ''' 画面の内容をデータテーブルに書き戻す
    ''' </summary>
    Public Sub WriteDetailView()
        '<< エラー説明 >>
        'O_ERR = OK:00000
        Dim CS0029ProfViewD As New CS0029ProfViewD        'UPROFview・Detail取得

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
            If IsNothing(REPEATER) Then
                Throw New ArgumentNullException("REPEATER")
            End If

            Dim outRep As Repeater = Me.REPEATER

            CS0029ProfViewD.CAMPCODE = Me.CAMPCODE
            CS0029ProfViewD.MAPID = Me.MAPID
            CS0029ProfViewD.VARI = Me.VARI
            CS0029ProfViewD.TABID = Me.TABID
            CS0029ProfViewD.CS0029ProfViewD()
            If CS0029ProfViewD.ERR = C_MESSAGE_NO.NORMAL Then

                For i As Integer = 0 To outRep.Items.Count - 1

                    Dim rowIndex = Integer.Parse(DirectCast(outRep.Items(i).FindControl(Me.COLPREFIX & CONST_MEISAINO), System.Web.UI.WebControls.TextBox).Text) - 1

                    For j As Integer = 1 To CS0029ProfViewD.COLMAX

                        Dim setColumn = DirectCast(outRep.Items(i).FindControl(Me.COLPREFIX & CONST_FIELD & j), System.Web.UI.WebControls.Label).Text
                        If setColumn <> "" AndAlso DirectCast(outRep.Items(i).FindControl(Me.COLPREFIX & CONST_VALUE & j), System.Web.UI.WebControls.TextBox).Text <> "" Then
                            Me.SRCDATA(rowIndex)(setColumn) = DirectCast(outRep.Items(i).FindControl(Me.COLPREFIX & CONST_VALUE & j), System.Web.UI.WebControls.TextBox).Text
                        End If
                    Next j
                Next i
            End If

            ERR = C_MESSAGE_NO.NORMAL

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
            ' その他例外

            Me.ERR = C_MESSAGE_NO.DB_ERROR

            Dim CS0011LOGWrite As New CS0011LOGWrite
            CS0011LOGWrite.INFSUBCLASS = Me.GetType.Name      'SUBクラス名
            CS0011LOGWrite.INFPOSI = "REPETER: CREATE"                  '
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT                              '
            CS0011LOGWrite.TEXT = ex.Message
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                     'ログ出力

        Finally

        End Try


    End Sub

End Class