Option Strict On
Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox
Imports System.IO
''' <summary>
''' ガイダンス登録画面クラス
''' </summary>
Public Class OIM0020GuidanceCreate
    Inherits System.Web.UI.Page
    '○ 検索結果格納Table
    Private OIM0020tbl As DataTable                                  '一覧格納用テーブル

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

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            If IsPostBack Then
                '添付ファイルアップロード処理
                If Me.WF_FILENAMELIST.Value <> "" Then
                    Dim retMes = UploadAttachments()
                    If retMes.MessageNo <> C_MESSAGE_NO.NORMAL Then
                        Master.Output(retMes.MessageNo, C_MESSAGE_TYPE.ERR, retMes.Pram01, needsPopUp:=True)
                    End If
                    Me.WF_FILENAMELIST.Value = ""
                End If
                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then

                    Select Case WF_ButtonClick.Value
                    '    Case "WF_UPDATE"                '表更新ボタン押下
                        '        WF_UPDATE_Click()
                        Case "WF_CLEAR"                 'クリアボタン押下
                            WF_CLEAR_Click()
                        Case "WF_DELETE"
                            WF_DELETE_Click()
                            '    Case "WF_Field_DBClick"         'フィールドダブルクリック
                            '        WF_FIELD_DBClick()
                            '    Case "WF_LeftBoxSelectClick"    'フィールドチェンジ
                            '        WF_FIELD_Change()
                            '    Case "WF_ButtonSel"             '(左ボックス)選択ボタン押下
                            '    WF_ButtonSel_Click()
                            'Case "WF_ButtonCan"             '(左ボックス)キャンセルボタン押下
                            '    WF_ButtonCan_Click()
                            'Case "WF_ListboxDBclick"        '左ボックスダブルクリック
                            '    WF_ButtonSel_Click()
                            'Case "WF_RadioButonClick"       '(右ボックス)ラジオボタン選択
                            '    WF_RadioButton_Click()
                            'Case "WF_MEMOChange"            '(右ボックス)メモ欄更新
                            '    WF_RIGHTBOX_Change()
                    End Select
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

            WF_BOXChange.Value = "detailbox"

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
        Master.MAPID = OIM0020WRKINC.MAPIDC
        '○HELP表示有無設定
        Master.dispHelp = False
        '○D&D有無設定
        Master.eventDrop = False '共通のD&Dは使わない
        '○Grid情報保存先のファイル名
        'Master.CreateXMLSaveFile()

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

    End Sub
    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()
        Dim dispVal As OIM0020WRKINC.GuidanceItemClass = Nothing
        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0020L Then
            dispVal = GetGuidance(work.WF_LIST_GUIDANCENO.Text)
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.MENU Then
            Dim prev As M00001MENU = DirectCast(Me.PreviousPage, M00001MENU)
            dispVal = GetGuidance(prev.SelectedGuidanceNo)
            Me.Form.Attributes.Add("REFONLY", "1")
            Me.txtEndYmd.Enabled = False
            Me.txtFromYmd.Enabled = False
            Me.txtTitle.Enabled = False
            Me.txtNaiyou.Enabled = False
            Me.chklFlags.Enabled = False
            Me.rblType.Enabled = False
        End If
        '添付ファイル作業フォルダの生成
        CreateInitDir(dispVal)

        '〇選択肢初期値設定
        Me.rblType.Items.Add(New ListItem("障害", "E"))
        Me.rblType.Items.Add(New ListItem("インフォメーション", "I"))
        Me.rblType.Items.Add(New ListItem("注意", "W"))
        '○ 名称設定処理
        Me.lblGuidanceEntryDate.Text = dispVal.InitYmd
        If Me.rblType.Items.FindByValue(dispVal.Type) IsNot Nothing Then
            Me.rblType.SelectedValue = dispVal.Type
        End If
        Me.txtFromYmd.Text = dispVal.FromYmd
        Me.txtEndYmd.Text = dispVal.EndYmd
        Me.chklFlags.DataSource = dispVal.DispFlags
        Me.chklFlags.DataTextField = "DispName"
        Me.chklFlags.DataValueField = "FieldName"
        Me.chklFlags.DataBind()
        Me.txtTitle.Text = dispVal.Title
        Me.txtNaiyou.Text = dispVal.Naiyo
        Me.repAttachments.DataSource = dispVal.Attachments
        Me.repAttachments.DataBind()
        ViewState("DISPVALUE") = dispVal
    End Sub
    ''' <summary>
    ''' ガイダンスマスタよりデータ取得
    ''' </summary>
    ''' <param name="guidanceNo"></param>
    ''' <returns></returns>
    Private Function GetGuidance(guidanceNo As String) As OIM0020WRKINC.GuidanceItemClass
        Dim retVal As New OIM0020WRKINC.GuidanceItemClass
        'ガイダンス番号が無い場合は新規作成扱い
        If guidanceNo = "" Then
            Return GetNewGuidanceItem()
        End If


        Dim sqlStat As New StringBuilder
            sqlStat.AppendLine("SELECT ")
            sqlStat.AppendLine("        MG.GUIDANCENO")
            sqlStat.AppendLine("       ,ISNULL(FORMAT(MG.FROMYMD, 'yyyy/MM/dd'), NULL) AS FROMYMD")
            sqlStat.AppendLine("       ,ISNULL(FORMAT(MG.ENDYMD,  'yyyy/MM/dd'), NULL) AS ENDYMD")
            sqlStat.AppendLine("       ,MG.TYPE")
            sqlStat.AppendLine("       ,MG.TITTLE")
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

            sqlStat.AppendLine("       ,MG.NAIYOU")
            sqlStat.AppendLine("       ,MG.FAILE1")
            sqlStat.AppendLine("       ,MG.FAILE2")
            sqlStat.AppendLine("       ,MG.FAILE3")
            sqlStat.AppendLine("       ,MG.FAILE4")
            sqlStat.AppendLine("       ,MG.FAILE5")
        sqlStat.AppendLine("       ,format(MG.INITYMD,'yyyy/MM/dd HH:mm')    AS INITYMD")
        sqlStat.AppendLine("       ,format(MG.UPDYMD ,'yyyy/MM/dd HH:mm:ss.fff')    AS UPDYMD")
            sqlStat.AppendLine("  FROM OIL.OIM0020_GUIDANCE MG")
        sqlStat.AppendLine(" WHERE MG.GUIDANCENO = @GUIDANCENO")
        Using sqlCon As SqlConnection = CS0050SESSION.getConnection,
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open()
            sqlCmd.Parameters.Add("@GUIDANCENO", SqlDbType.NVarChar).Value = guidanceNo

            Using SQLdr As SqlDataReader = sqlCmd.ExecuteReader()
                If SQLdr.HasRows = False Then
                    Return GetNewGuidanceItem()
                End If
                SQLdr.Read()
                retVal.GuidanceNo = Convert.ToString(SQLdr("GUIDANCENO"))
                retVal.FromYmd = Convert.ToString(SQLdr("FROMYMD"))
                retVal.EndYmd = Convert.ToString(SQLdr("ENDYMD"))
                retVal.Type = Convert.ToString(SQLdr("TYPE"))
                retVal.Title = Convert.ToString(SQLdr("TITTLE"))
                retVal.DispFlags = work.GetNewDisplayFlags
                Dim keyValues As New List(Of String) From {"OUTFLG", "INFLG1", "INFLG2", "INFLG3", "INFLG4", "INFLG5",
                                                   "INFLG6", "INFLG7", "INFLG8", "INFLG9", "INFLG9", "INFLG10", "INFLG11"}
                'フラグの初期値設定
                Dim stringVal As String = ""
                For Each keyVal In keyValues
                    stringVal = Convert.ToString(SQLdr(keyVal))

                    Dim item = From dispFlg In retVal.DispFlags Where dispFlg.FieldName = keyVal
                    If item.Any Then
                        Dim fstItem = item.FirstOrDefault
                        If stringVal = "1" Then
                            fstItem.Checked = True
                        End If
                    End If
                Next
                retVal.Naiyo = Convert.ToString(SQLdr("NAIYOU"))
                keyValues = New List(Of String) From {"FAILE1", "FAILE2", "FAILE3", "FAILE4", "FAILE5"}
                For Each keyVal In keyValues
                    stringVal = Convert.ToString(SQLdr(keyVal))
                    If stringVal <> "" Then
                        Dim fileInf As New OIM0020WRKINC.FileItemClass
                        fileInf.FileName = stringVal
                        retVal.Attachments.Add(fileInf)
                    End If
                Next
                retVal.InitYmd = Convert.ToString(SQLdr("INITYMD"))
            End Using
        End Using
        Return retVal
    End Function
    ''' <summary>
    ''' 新規ガイダンス情報の作成
    ''' </summary>
    ''' <returns></returns>
    Private Function GetNewGuidanceItem() As OIM0020WRKINC.GuidanceItemClass
        Dim retVal As New OIM0020WRKINC.GuidanceItemClass
        Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "FROMYMD", retVal.FromYmd)
        Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "ENDYMD", retVal.EndYmd)

        Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "TYPE", retVal.Type)
        retVal.DispFlags = work.GetNewDisplayFlags
        Dim keyValues As New List(Of String) From {"OUTFLG", "INFLG1", "INFLG2", "INFLG3", "INFLG4", "INFLG5",
                                                   "INFLG6", "INFLG7", "INFLG8", "INFLG9", "INFLG9", "INFLG10", "INFLG11"}
        'フラグの初期値設定
        Dim stringVal As String = ""
        For Each keyVal In keyValues
            stringVal = ""
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, keyVal, stringVal)
            Dim item = From dispFlg In retVal.DispFlags Where dispFlg.FieldName = keyVal
            If item.Any Then
                Dim fstItem = item.FirstOrDefault
                If stringVal = "1" Then
                    fstItem.Checked = True
                End If
            End If
        Next
        Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "NAIYOU", retVal.Naiyo)
        Return retVal
    End Function
    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_CLEAR_Click()
        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""

        Master.TransitionPrevPage()

    End Sub
    ''' <summary>
    ''' 添付ファイル削除ボタン押下時処理
    ''' </summary>
    Protected Sub WF_DELETE_Click()
        Dim retMes = New PropMes With {.MessageNo = C_MESSAGE_NO.NORMAL}
        Dim dispVal = DirectCast(ViewState("DISPVALUE"), OIM0020WRKINC.GuidanceItemClass)
        'ガイダンス用作業フォルダ
        Dim guidanceWorkDir As String = IO.Path.Combine(CS0050SESSION.UPLOAD_PATH, OIM0020WRKINC.GUIDANCEROOT, "USERWORKS", CS0050SESSION.USERID)
        If Not Directory.Exists(guidanceWorkDir) Then
            Directory.CreateDirectory(guidanceWorkDir)
        End If
        Dim deleteFileName As String = Me.WF_DELETEFILENAME.Value
        For i = dispVal.Attachments.Count - 1 To 0 Step -1
            If dispVal.Attachments(i).FileName = deleteFileName Then
                dispVal.Attachments.RemoveAt(i)
                Exit For
            End If
        Next
        Dim delFilePath As String = IO.Path.Combine(guidanceWorkDir, deleteFileName)
        If IO.File.Exists(delFilePath) Then
            Try
                IO.File.Delete(delFilePath)
            Catch ex As Exception
            End Try
        End If
        '画面情報を書き換え
        Me.repAttachments.DataSource = dispVal.Attachments
        Me.repAttachments.DataBind()
        ViewState("DISPVALUE") = dispVal
        Return
    End Sub
    ''' <summary>
    ''' チェックボックスデータバインド時イベント
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks>チェックの状態を設定する</remarks>
    Private Sub chklFlags_DataBound(sender As Object, e As EventArgs) Handles chklFlags.DataBound
        Dim chklObj As CheckBoxList = DirectCast(sender, CheckBoxList)
        Dim chkBindItm As List(Of OIM0020WRKINC.DisplayFlag) = DirectCast(chklObj.DataSource, List(Of OIM0020WRKINC.DisplayFlag))
        For i = 0 To chklObj.Items.Count - 1 Step 1
            chklObj.Items(i).Selected = chkBindItm(i).Checked
        Next
    End Sub
    ''' <summary>
    ''' ガイダンス処理の作業フォルダを作成する
    ''' </summary>
    ''' <param name="guidanceItem"></param>
    Private Sub CreateInitDir(guidanceItem As OIM0020WRKINC.GuidanceItemClass)

        '実体保存フォルダよりファイルのコピーを行う
        If guidanceItem.GuidanceNo = "" Then
            'ガイダンスNoが無い場合は既登録の添付ファイルはない前提なのでここで終了
            Return
        End If
        'ガイダンス用作業フォルダ
        Dim guidanceWorkDir As String = IO.Path.Combine(CS0050SESSION.UPLOAD_PATH, OIM0020WRKINC.GUIDANCEROOT, "USERWORKS", CS0050SESSION.USERID)
        If Not Directory.Exists(guidanceWorkDir) Then
            Directory.CreateDirectory(guidanceWorkDir)
        End If
        For Each tempFile As String In Directory.GetFiles(guidanceWorkDir, "*.*")
            ' ファイルパスからファイル名を取得
            Try
                File.Delete(tempFile)
            Catch ex As Exception
            End Try
        Next
        '既存ファイルを作業フォルダにコピー
        Dim guidanceDir As String = IO.Path.Combine(CS0050SESSION.UPLOAD_PATH, OIM0020WRKINC.GUIDANCEROOT, guidanceItem.GuidanceNo)

        If IO.Directory.Exists(guidanceDir) = True Then
            Dim fileNames = IO.Directory.GetFiles(guidanceDir)
            For Each fileName In fileNames
                If fileName = "" Then
                    Continue For
                End If
                Dim targetFile As String = IO.Path.Combine(guidanceDir, fileName)
                Dim copyPath As String = IO.Path.Combine(guidanceWorkDir, fileName)
                Try
                    System.IO.File.Copy(targetFile, copyPath, True)
                Catch ex As Exception
                End Try
                'テーブルに登録した情報と比較、実体があり、テーブルにない場合は追加
                If (From gitm In guidanceItem.Attachments Where gitm.FileName = fileName).Any = False Then
                    guidanceItem.Attachments.Add(New OIM0020WRKINC.FileItemClass With {.FileName = fileName})
                End If
            Next fileName
            'テーブルにあり実体がない場合は消去
            If fileNames IsNot Nothing OrElse fileNames.Count > 0 Then
                For i = guidanceItem.Attachments.Count - 1 To 0 Step -1
                    If fileNames.Contains(guidanceItem.Attachments(i).FileName) = False Then
                        guidanceItem.Attachments.RemoveAt(i)
                    End If
                Next
            Else
                guidanceItem.Attachments = New List(Of OIM0020WRKINC.FileItemClass)
            End If
        Else
            guidanceItem.Attachments = New List(Of OIM0020WRKINC.FileItemClass)
        End If
    End Sub
    ''' <summary>
    ''' ファイルアップロード処理
    ''' </summary>
    ''' <remarks>OIM0020FILEUPLOADの処理が完了後にこちらの処理が実行されます。</remarks>
    Private Function UploadAttachments() As PropMes
        Dim retMes = New PropMes With {.MessageNo = C_MESSAGE_NO.NORMAL}
        Dim tp As Type = GetType(List(Of AttachmentFile))
        Dim serializer As New Runtime.Serialization.Json.DataContractJsonSerializer(tp)
        Dim uploadFiles As New List(Of AttachmentFile)
        Dim dispVal = DirectCast(ViewState("DISPVALUE"), OIM0020WRKINC.GuidanceItemClass)
        Try
            Using stream As New IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(Me.WF_FILENAMELIST.Value))
                uploadFiles = DirectCast(serializer.ReadObject(stream), List(Of AttachmentFile))
            End Using
        Catch ex As Exception
            Return retMes
        End Try
        'ガイダンス用作業フォルダ
        Dim guidanceWorkDir As String = IO.Path.Combine(CS0050SESSION.UPLOAD_PATH, OIM0020WRKINC.GUIDANCEROOT, "USERWORKS", CS0050SESSION.USERID)
        If Not Directory.Exists(guidanceWorkDir) Then
            Directory.CreateDirectory(guidanceWorkDir)
        End If
        'アップロードワークフォルダ
        Dim uploadWorkDir = IO.Path.Combine(CS0050SESSION.UPLOAD_PATH, "UPLOAD_TMP", CS0050SESSION.USERID)
        If Not Directory.Exists(guidanceWorkDir) Then
            Return retMes
        End If
        'アップロードしたファイルと現在画面にあるファイルをファイル名重複なしてマージ
        Dim fileNames As List(Of String) = (From itm In dispVal.Attachments Select itm.FileName).ToList
        Dim addedFileList As New List(Of OIM0020WRKINC.FileItemClass)
        For Each uploadFile In uploadFiles
            If Not fileNames.Contains(uploadFile.FileName) Then
                fileNames.Add(uploadFile.FileName)
                addedFileList.Add(New OIM0020WRKINC.FileItemClass With {.FileName = uploadFile.FileName})
            End If
        Next
        'ファイル数が5を超えた場合はアップさせずにエラー
        If fileNames.Count > 5 Then
            retMes.MessageNo = C_MESSAGE_NO.OIL_ATTACHMENT_COUNTOVER
            retMes.Pram01 = "5"
            Return retMes
        End If
        'ガイダンスファイル作業フォルダにコピー
        For Each uploadFile In uploadFiles
            Dim targetFile As String = IO.Path.Combine(uploadWorkDir, uploadFile.FileName)
            Dim copyPath As String = IO.Path.Combine(guidanceWorkDir, uploadFile.FileName)
            Try
                System.IO.File.Copy(targetFile, copyPath, True)
            Catch ex As Exception
            End Try
        Next
        If addedFileList.Count > 0 Then
            dispVal.Attachments.AddRange(addedFileList)
        End If
        '画面情報を書き換え
        Me.repAttachments.DataSource = dispVal.Attachments
        Me.repAttachments.DataBind()
        ViewState("DISPVALUE") = dispVal
        Return retMes
    End Function
    ''' <summary>
    ''' ファイル情報クラス
    ''' </summary>
    <System.Runtime.Serialization.DataContract()>
    Public Class AttachmentFile
        <System.Runtime.Serialization.DataMember()>
        Public Property FileName As String
    End Class
    Public Class PropMes
        Public Property MessageNo As String = ""
        Public Property Pram01 As String = ""
    End Class
End Class