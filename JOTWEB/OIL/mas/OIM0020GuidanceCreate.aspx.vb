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
                        Case "WF_UPDATE"                '表更新ボタン押下
                            WF_UPDATE_Click()
                        Case "WF_CLEAR"                 'クリアボタン押下
                            WF_CLEAR_Click()
                        Case "WF_DELETE"
                            WF_DELETE_Click()
                        Case "WF_Field_DBClick"         'フィールドダブルクリック
                            WF_FIELD_DBClick()
                        'Case "WF_LeftBoxSelectClick"    'フィールドチェンジ
                        '    WF_FIELD_Change()
                        Case "WF_ButtonSel"             '(左ボックス)選択ボタン押下
                            WF_ButtonSel_Click()
                        Case "WF_ButtonCan"             '(左ボックス)キャンセルボタン押下
                            WF_ButtonCan_Click()
                        Case "WF_ListboxDBclick"        '左ボックスダブルクリック
                            WF_ButtonSel_Click()
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
        ElseIf Context.Handler.ToString().ToUpper().startsWith("ASP.OIL_M00001MENU") Then
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
        sqlStat.AppendLine("       ,MG.TITLE")
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
        sqlStat.AppendLine("       ,MG.FILE1")
        sqlStat.AppendLine("       ,MG.FILE2")
        sqlStat.AppendLine("       ,MG.FILE3")
        sqlStat.AppendLine("       ,MG.FILE4")
        sqlStat.AppendLine("       ,MG.FILE5")
        sqlStat.AppendLine("       ,format(MG.INITYMD,'yyyy/MM/dd HH:mm')    AS INITYMD")
        sqlStat.AppendLine("       ,format(MG.UPDYMD ,'yyyy/MM/dd HH:mm:ss.fff')    AS UPDYMD")
        sqlStat.AppendLine("  FROM OIL.OIM0020_GUIDANCE MG")
        sqlStat.AppendLine(" WHERE MG.GUIDANCENO = @GUIDANCENO")
        Using sqlCon As SqlConnection = CS0050SESSION.getConnection,
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open()
            sqlCmd.CommandTimeout = 300
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
                retVal.Title = Convert.ToString(SQLdr("TITLE"))
                retVal.DispFlags = OIM0020WRKINC.GetNewDisplayFlags()
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
                keyValues = New List(Of String) From {"FILE1", "FILE2", "FILE3", "FILE4", "FILE5"}
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
    ''' 入力チェック
    ''' </summary>
    ''' <param name="dispVal"></param>
    ''' <returns></returns>
    Protected Function INPCheck(dispVal As OIM0020WRKINC.GuidanceItemClass) As PropMes
        Dim retMes As New PropMes With {.MessageNo = C_MESSAGE_NO.NORMAL}
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""

        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "FROMYMD", dispVal.FromYmd, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            retMes.MessageNo = WW_CS0024FCHECKERR
            retMes.Pram01 = "掲載開始日"
            Me.txtFromYmd.Focus()
            Return retMes
        End If

        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ENDYMD", dispVal.EndYmd, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            retMes.MessageNo = WW_CS0024FCHECKERR
            retMes.Pram01 = "掲載終了日"
            Me.txtFromYmd.Focus()
            Return retMes
        End If
        Dim fromDtm As Date = CDate(dispVal.FromYmd)
        Dim toDtm As Date = CDate(dispVal.EndYmd)
        If fromDtm > toDtm Then
            retMes.MessageNo = C_MESSAGE_NO.START_END_RELATION_ERROR
            retMes.Pram01 = ""
            Me.txtEndYmd.Focus()
            Return retMes
        End If
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TYPE", dispVal.Type, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            retMes.MessageNo = WW_CS0024FCHECKERR
            retMes.Pram01 = "種類"
            Return retMes
        End If

        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TITLE", dispVal.Title, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            retMes.MessageNo = WW_CS0024FCHECKERR
            retMes.Pram01 = "タイトル"
            Me.txtTitle.Focus()
            Return retMes
        End If

        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "NAIYO", dispVal.Naiyo, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            retMes.MessageNo = WW_CS0024FCHECKERR
            retMes.Pram01 = "内容"
            Me.txtNaiyou.Focus()
            Return retMes
        End If

        For Each fileItm In dispVal.Attachments
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "FILE", fileItm.FileName, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                retMes.MessageNo = WW_CS0024FCHECKERR
                retMes.Pram01 = String.Format("ファイル名({0})", fileItm.FileName)
                Return retMes
            End If
        Next

        Return retMes
    End Function
    ''' <summary>
    ''' ガイダンステーブル更新処理
    ''' </summary>
    ''' <param name="dispVal"></param>
    ''' <param name="sqlCon"></param>
    ''' <param name="sqlTran"></param>
    ''' <returns></returns>
    Private Function UpdateGuidance(dispVal As OIM0020WRKINC.GuidanceItemClass, sqlCon As SqlConnection, sqlTran As SqlTransaction) As PropMes
        Dim retMes As New PropMes With {.MessageNo = C_MESSAGE_NO.NORMAL}
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("UPDATE OIL.OIM0020_GUIDANCE")
        sqlStat.AppendLine("   SET FROMYMD    = @FROMYMD")
        sqlStat.AppendLine("      ,ENDYMD     = @ENDYMD")
        sqlStat.AppendLine("      ,TYPE       = @TYPE")
        sqlStat.AppendLine("      ,TITLE      = @TITLE")
        sqlStat.AppendLine("      ,OUTFLG     = @OUTFLG")
        sqlStat.AppendLine("      ,INFLG1     = @INFLG1")
        sqlStat.AppendLine("      ,INFLG2     = @INFLG2")
        sqlStat.AppendLine("      ,INFLG3     = @INFLG3")
        sqlStat.AppendLine("      ,INFLG4     = @INFLG4")
        sqlStat.AppendLine("      ,INFLG5     = @INFLG5")
        sqlStat.AppendLine("      ,INFLG6     = @INFLG6")
        sqlStat.AppendLine("      ,INFLG7     = @INFLG7")
        sqlStat.AppendLine("      ,INFLG8     = @INFLG8")
        sqlStat.AppendLine("      ,INFLG9     = @INFLG9")
        sqlStat.AppendLine("      ,INFLG10    = @INFLG10")
        sqlStat.AppendLine("      ,INFLG11    = @INFLG11")
        sqlStat.AppendLine("      ,NAIYOU     = @NAIYOU")
        sqlStat.AppendLine("      ,FILE1      = @FILE1")
        sqlStat.AppendLine("      ,FILE2      = @FILE2")
        sqlStat.AppendLine("      ,FILE3      = @FILE3")
        sqlStat.AppendLine("      ,FILE4      = @FILE4")
        sqlStat.AppendLine("      ,FILE5      = @FILE5")
        sqlStat.AppendLine("      ,UPDYMD     = @UPDYMD")
        sqlStat.AppendLine("      ,UPDUSER    = @UPDUSER")
        sqlStat.AppendLine("      ,UPDTERMID  = @UPDTERMID")
        sqlStat.AppendLine("      ,RECEIVEYMD = @RECEIVEYMD")
        sqlStat.AppendLine(" WHERE GUIDANCENO = @GUIDANCENO")

        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, sqlTran)
            sqlCmd.CommandTimeout = 300
            With sqlCmd.Parameters

                .Add("@GUIDANCENO", SqlDbType.NVarChar).Value = dispVal.GuidanceNo

                .Add("@FROMYMD", SqlDbType.Date).Value = dispVal.FromYmd
                .Add("@ENDYMD", SqlDbType.Date).Value = dispVal.EndYmd
                .Add("@TYPE", SqlDbType.NVarChar).Value = dispVal.Type
                .Add("@TITLE", SqlDbType.NVarChar).Value = dispVal.Title
                .Add("@NAIYOU", SqlDbType.NVarChar).Value = dispVal.Naiyo
                For Each flagFiels In {"OUTFLG", "INFLG1", "INFLG2", "INFLG3", "INFLG4", "INFLG5",
                                       "INFLG6", "INFLG7", "INFLG8", "INFLG9", "INFLG10", "INFLG11"}
                    Dim findFlag = From flagitm In dispVal.DispFlags Where flagitm.FieldName = flagFiels AndAlso flagitm.Checked
                    If findFlag.Any Then
                        .Add("@" & flagFiels, SqlDbType.NVarChar).Value = "1"
                    Else
                        .Add("@" & flagFiels, SqlDbType.NVarChar).Value = "0"
                    End If
                Next
                Dim fileNo As Integer = 0
                For Each attachItm In dispVal.Attachments
                    If fileNo >= 5 Then
                        Exit For
                    End If
                    fileNo = fileNo + 1
                    .Add(String.Format("@FILE{0}", fileNo), SqlDbType.NVarChar).Value = attachItm.FileName
                Next

                If fileNo < 5 Then
                    fileNo = fileNo + 1
                    For i = fileNo To 5
                        .Add(String.Format("@FILE{0}", i), SqlDbType.NVarChar).Value = ""
                    Next
                End If
                .Add("@UPDYMD", SqlDbType.DateTime).Value = Now.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                .Add("@UPDUSER", SqlDbType.NVarChar).Value = Master.USERID
                .Add("@UPDTERMID", SqlDbType.NVarChar).Value = Master.USERTERMID
                .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
            End With
            sqlCmd.ExecuteNonQuery()
        End Using

        Return retMes
    End Function
    ''' <summary>
    ''' ガイダンステーブル追加処理
    ''' </summary>
    ''' <param name="dispVal"></param>
    ''' <param name="sqlCon"></param>
    ''' <param name="sqlTran"></param>
    ''' <returns></returns>
    Private Function InsertGuidance(dispVal As OIM0020WRKINC.GuidanceItemClass, sqlCon As SqlConnection, sqlTran As SqlTransaction, ByRef newGuidanceNo As String, ByRef entDtm As String) As PropMes
        Dim retMes As New PropMes With {.MessageNo = C_MESSAGE_NO.NORMAL}
        '新ガイダンス番号の取得
        Dim newGdWork As String = ""
        Dim dateYmd As String = Now.ToString("yyyyMMdd")
        Dim sqlGetNgd As New StringBuilder
        sqlGetNgd.AppendLine("SELECT @YMD + FORMAT(ISNULL(MAX(CONVERT(int,REPLACE(GD.GUIDANCENO,@YMD,''))),0) + 1,'0000') NGD")
        sqlGetNgd.AppendLine("  FROM OIL.OIM0020_GUIDANCE GD")
        sqlGetNgd.AppendLine(" WHERE GD.GUIDANCENO LIKE @YMD + '%'")
        Using sqlCmd As New SqlCommand(sqlGetNgd.ToString, sqlCon, sqlTran)
            sqlCmd.Parameters.Add("@YMD", SqlDbType.NVarChar).Value = dateYmd
            newGdWork = Convert.ToString(sqlCmd.ExecuteScalar())
        End Using
        'インサート処理実行
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("INSERT INTO OIL.OIM0020_GUIDANCE (")
        sqlStat.AppendLine("     GUIDANCENO")
        sqlStat.AppendLine("    ,FROMYMD")
        sqlStat.AppendLine("    ,ENDYMD")
        sqlStat.AppendLine("    ,TYPE")
        sqlStat.AppendLine("    ,TITLE")
        sqlStat.AppendLine("    ,OUTFLG")
        sqlStat.AppendLine("    ,INFLG1")
        sqlStat.AppendLine("    ,INFLG2")
        sqlStat.AppendLine("    ,INFLG3")
        sqlStat.AppendLine("    ,INFLG4")
        sqlStat.AppendLine("    ,INFLG5")
        sqlStat.AppendLine("    ,INFLG6")
        sqlStat.AppendLine("    ,INFLG7")
        sqlStat.AppendLine("    ,INFLG8")
        sqlStat.AppendLine("    ,INFLG9")
        sqlStat.AppendLine("    ,INFLG10")
        sqlStat.AppendLine("    ,INFLG11")
        sqlStat.AppendLine("    ,NAIYOU")
        sqlStat.AppendLine("    ,FILE1")
        sqlStat.AppendLine("    ,FILE2")
        sqlStat.AppendLine("    ,FILE3")
        sqlStat.AppendLine("    ,FILE4")
        sqlStat.AppendLine("    ,FILE5")
        sqlStat.AppendLine("    ,DELFLG")
        sqlStat.AppendLine("    ,INITYMD")
        sqlStat.AppendLine("    ,INITUSER")
        sqlStat.AppendLine("    ,INITTERMID")
        sqlStat.AppendLine("    ,UPDYMD")
        sqlStat.AppendLine("    ,UPDUSER")
        sqlStat.AppendLine("    ,UPDTERMID")
        sqlStat.AppendLine("    ,RECEIVEYMD")
        sqlStat.AppendLine(" ) VALUES (")
        sqlStat.AppendLine("     @GUIDANCENO")
        sqlStat.AppendLine("    ,@FROMYMD")
        sqlStat.AppendLine("    ,@ENDYMD")
        sqlStat.AppendLine("    ,@TYPE")
        sqlStat.AppendLine("    ,@TITLE")
        sqlStat.AppendLine("    ,@OUTFLG")
        sqlStat.AppendLine("    ,@INFLG1")
        sqlStat.AppendLine("    ,@INFLG2")
        sqlStat.AppendLine("    ,@INFLG3")
        sqlStat.AppendLine("    ,@INFLG4")
        sqlStat.AppendLine("    ,@INFLG5")
        sqlStat.AppendLine("    ,@INFLG6")
        sqlStat.AppendLine("    ,@INFLG7")
        sqlStat.AppendLine("    ,@INFLG8")
        sqlStat.AppendLine("    ,@INFLG9")
        sqlStat.AppendLine("    ,@INFLG10")
        sqlStat.AppendLine("    ,@INFLG11")
        sqlStat.AppendLine("    ,@NAIYOU")
        sqlStat.AppendLine("    ,@FILE1")
        sqlStat.AppendLine("    ,@FILE2")
        sqlStat.AppendLine("    ,@FILE3")
        sqlStat.AppendLine("    ,@FILE4")
        sqlStat.AppendLine("    ,@FILE5")
        sqlStat.AppendLine("    ,@DELFLG")
        sqlStat.AppendLine("    ,@INITYMD")
        sqlStat.AppendLine("    ,@INITUSER")
        sqlStat.AppendLine("    ,@INITTERMID")
        sqlStat.AppendLine("    ,@UPDYMD")
        sqlStat.AppendLine("    ,@UPDUSER")
        sqlStat.AppendLine("    ,@UPDTERMID")
        sqlStat.AppendLine("    ,@RECEIVEYMD")
        sqlStat.AppendLine(" )")
        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, sqlTran)
            sqlCmd.CommandTimeout = 300
            With sqlCmd.Parameters
                .Add("@GUIDANCENO", SqlDbType.NVarChar).Value = newGdWork

                .Add("@FROMYMD", SqlDbType.Date).Value = dispVal.FromYmd
                .Add("@ENDYMD", SqlDbType.Date).Value = dispVal.EndYmd
                .Add("@TYPE", SqlDbType.NVarChar).Value = dispVal.Type
                .Add("@TITLE", SqlDbType.NVarChar).Value = dispVal.Title
                .Add("@NAIYOU", SqlDbType.NVarChar).Value = dispVal.Naiyo
                For Each flagFiels In {"OUTFLG", "INFLG1", "INFLG2", "INFLG3", "INFLG4", "INFLG5",
                                       "INFLG6", "INFLG7", "INFLG8", "INFLG9", "INFLG10", "INFLG11"}
                    Dim findFlag = From flagitm In dispVal.DispFlags Where flagitm.FieldName = flagFiels AndAlso flagitm.Checked
                    If findFlag.Any Then
                        .Add("@" & flagFiels, SqlDbType.NVarChar).Value = "1"
                    Else
                        .Add("@" & flagFiels, SqlDbType.NVarChar).Value = "0"
                    End If
                Next
                Dim fileNo As Integer = 0
                For Each attachItm In dispVal.Attachments
                    If fileNo >= 5 Then
                        Exit For
                    End If
                    fileNo = fileNo + 1
                    .Add(String.Format("@FILE{0}", fileNo), SqlDbType.NVarChar).Value = attachItm.FileName
                Next

                If fileNo < 5 Then
                    fileNo = fileNo + 1
                    For i = fileNo To 5
                        .Add(String.Format("@FILE{0}", i), SqlDbType.NVarChar).Value = ""
                    Next
                End If
                .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
                Dim entDate As String = Now.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                entDtm = entDate
                .Add("@INITYMD", SqlDbType.DateTime).Value = entDate
                .Add("@INITUSER", SqlDbType.NVarChar).Value = Master.USERID
                .Add("@INITTERMID", SqlDbType.NVarChar).Value = Master.USERTERMID
                .Add("@UPDYMD", SqlDbType.DateTime).Value = entDate
                .Add("@UPDUSER", SqlDbType.NVarChar).Value = Master.USERID
                .Add("@UPDTERMID", SqlDbType.NVarChar).Value = Master.USERTERMID
                .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
            End With
            sqlCmd.ExecuteNonQuery()
        End Using
        newGuidanceNo = newGdWork
        Return retMes
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
        retVal.DispFlags = OIM0020WRKINC.GetNewDisplayFlags()
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
    ''' 更新ボタン押下時処理
    ''' </summary>
    Protected Sub WF_UPDATE_Click()
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""
        '画面の値収集
        Dim dispVal = CollectDispValue()
        '入力チェック
        Dim retMes = INPCheck(dispVal)
        If retMes.MessageNo <> C_MESSAGE_NO.NORMAL Then
            Master.Output(retMes.MessageNo, C_MESSAGE_TYPE.ERR, retMes.Pram01, needsPopUp:=True)
            Return
        End If
        '登録処理
        Dim newGuidanceNo As String = ""
        Using sqlCon = CS0050SESSION.getConnection
            sqlCon.Open()
            Using sqlTrn = sqlCon.BeginTransaction
                Dim workGuidance As String = ""
                Dim entDtm As String = ""
                If dispVal.GuidanceNo <> "" Then
                    UpdateGuidance(dispVal, sqlCon, sqlTrn)
                    workGuidance = dispVal.GuidanceNo
                    entDtm = dispVal.InitYmd
                Else
                    InsertGuidance(dispVal, sqlCon, sqlTrn, newGuidanceNo, entDtm)
                    workGuidance = newGuidanceNo
                    entDtm = CDate(entDtm).ToString("yyyy/MM/dd HH:mm")
                End If
                'ジャーナル生成
                SaveJournal(workGuidance, sqlCon, sqlTrn)
                'ファイル移動
                MoveAttachments(workGuidance, dispVal)
                'トランザクションコミット
                sqlTrn.Commit()
                'ガイダンス番号転記
                dispVal.GuidanceNo = workGuidance
                dispVal.InitYmd = entDtm
                ViewState("DISPVALUE") = dispVal
                Me.lblGuidanceEntryDate.Text = entDtm
                Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF, retMes.Pram01, needsPopUp:=True)
            End Using
        End Using
    End Sub
    ''' <summary>
    ''' 画面入力値収集
    ''' </summary>
    ''' <returns></returns>
    Public Function CollectDispValue() As OIM0020WRKINC.GuidanceItemClass
        Dim dispVal = DirectCast(ViewState("DISPVALUE"), OIM0020WRKINC.GuidanceItemClass)
        dispVal.FromYmd = Me.txtFromYmd.Text
        dispVal.EndYmd = Me.txtEndYmd.Text
        dispVal.Title = Me.txtTitle.Text
        dispVal.Naiyo = Me.txtNaiyou.Text
        If Me.rblType.SelectedItem IsNot Nothing Then
            dispVal.Type = Me.rblType.SelectedValue
        Else
            dispVal.Type = ""
        End If
        For Each flag In dispVal.DispFlags
            Dim chkObj = Me.chklFlags.Items.FindByValue(flag.FieldName)
            If chkObj IsNot Nothing AndAlso chkObj.Selected Then
                flag.Checked = True
            Else
                flag.Checked = False
            End If
        Next
        Return dispVal
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
    ''' フィールドダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_DBClick()
        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                Integer.Parse(WF_LeftMViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try
            With leftview
                Dim enumVal = DirectCast([Enum].ToObject(GetType(LIST_BOX_CLASSIFICATION), CInt(WF_LeftMViewChange.Value)), LIST_BOX_CLASSIFICATION)
                If enumVal = LIST_BOX_CLASSIFICATION.LC_CALENDAR Then
                    '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                    Select Case WF_FIELD.Value
                        Case "WF_FROMYMD"
                            .WF_Calendar.Text = txtFromYmd.Text
                        Case "WF_ENDYMD"
                            .WF_Calendar.Text = txtEndYmd.Text
                    End Select
                    .ActiveCalendar()
                End If
            End With
        End If

    End Sub

    ''' <summary>
    ''' フィールドチェンジ時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_Change()

    End Sub


    ' ******************************************************************************
    ' ***  LeftBox関連操作                                                       ***
    ' ******************************************************************************

    ''' <summary>
    ''' LeftBox選択時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()


        '○ 選択内容を画面項目へセット
        Select Case WF_FIELD.Value
            Case "WF_FROMYMD"
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < CDate(C_DEFAULT_YMD) Then
                        txtFromYmd.Text = ""
                    Else
                        txtFromYmd.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                txtFromYmd.Focus()
            Case "WF_ENDYMD"
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < CDate(C_DEFAULT_YMD) Then
                        txtEndYmd.Text = ""
                    Else
                        txtEndYmd.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                txtEndYmd.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""

    End Sub


    ''' <summary>
    ''' LeftBoxキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○ フォーカスセット
        Select Case WF_FIELD.Value
            Case "WF_FROMYMD"
                txtFromYmd.Focus()
            Case "WF_ENDYMD"
                txtEndYmd.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""

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
            For Each filePath In fileNames
                Dim fileName As String = IO.Path.GetFileName(filePath)
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
            Next filePath
            'テーブルにあり実体がない場合は消去
            If fileNames IsNot Nothing OrElse fileNames.Count > 0 Then
                Dim fileNameList = (From filItm In fileNames Select IO.Path.GetFileName(filItm)).ToList
                For i = guidanceItem.Attachments.Count - 1 To 0 Step -1

                    If fileNameList.Contains(guidanceItem.Attachments(i).FileName) = False Then
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
        If Not Directory.Exists(uploadWorkDir) Then
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
    ''' ガイダンス作業フォルダから実体保存フォルダにコピー
    ''' </summary>
    ''' <param name="targetGuiganceNo">実際に移動するガイダンス番号、下の画面クラスには新規作成の場合振られていないのでこちらを利用</param>
    ''' <param name="guidanceItem">画面情報クラス</param>
    ''' <returns></returns>
    Private Function MoveAttachments(targetGuiganceNo As String, guidanceItem As OIM0020WRKINC.GuidanceItemClass) As PropMes
        Dim retMes = New PropMes With {.MessageNo = C_MESSAGE_NO.NORMAL}
        'ガイダンス用作業フォルダ
        Dim guidanceWorkDir As String = IO.Path.Combine(CS0050SESSION.UPLOAD_PATH, OIM0020WRKINC.GUIDANCEROOT, "USERWORKS", CS0050SESSION.USERID)
        If Not Directory.Exists(guidanceWorkDir) Then
            Directory.CreateDirectory(guidanceWorkDir)
        End If
        'ガイダンス実体保存フォルダ
        Dim guidanceSaveDir = IO.Path.Combine(CS0050SESSION.UPLOAD_PATH, OIM0020WRKINC.GUIDANCEROOT, targetGuiganceNo)
        If Not Directory.Exists(guidanceSaveDir) Then
            Directory.CreateDirectory(guidanceSaveDir)
        Else
            '既保存ファイルを削除
            For Each tempFile As String In Directory.GetFiles(guidanceSaveDir, "*.*")
                ' ファイルパスからファイル名を取得
                Try
                    File.Delete(tempFile)
                Catch ex As Exception
                End Try
            Next
        End If
        'ガイダンス添付ファイルを作業から実体フォルダにコピー
        Dim uploadFiles = (From attItm In guidanceItem.Attachments).ToList

        For Each uploadFile In uploadFiles
            Dim targetFile As String = IO.Path.Combine(guidanceWorkDir, uploadFile.FileName)
            Dim copyPath As String = IO.Path.Combine(guidanceSaveDir, uploadFile.FileName)
            Try
                System.IO.File.Copy(targetFile, copyPath, True)
            Catch ex As Exception
            End Try
        Next

        Return retMes
    End Function
    ''' <summary>
    ''' ジャーナル保存
    ''' </summary>
    ''' <param name="guidanceNo"></param>
    ''' <param name="sqlCon"></param>
    ''' <param name="sqlTran"></param>
    ''' <returns></returns>
    Function SaveJournal(guidanceNo As String, sqlCon As SqlConnection, sqlTran As SqlTransaction) As PropMes
        Dim retMes = New PropMes With {.MessageNo = C_MESSAGE_NO.NORMAL}
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT ")
        sqlStat.AppendLine("     GUIDANCENO")
        sqlStat.AppendLine("    ,convert(nvarchar,isnull(FROMYMD, '')) AS FROMYMD")
        sqlStat.AppendLine("    ,convert(nvarchar,isnull(ENDYMD, ''))  AS ENDYMD")
        sqlStat.AppendLine("    ,isnull(TYPE,   '') AS TYPE")
        sqlStat.AppendLine("    ,isnull(TITLE, '')  AS TITLE")
        sqlStat.AppendLine("    ,isnull(OUTFLG, '') AS OUTFLG")
        sqlStat.AppendLine("    ,isnull(INFLG1, '') AS INFLG1")
        sqlStat.AppendLine("    ,isnull(INFLG2, '') AS INFLG2")
        sqlStat.AppendLine("    ,isnull(INFLG3, '') AS INFLG3")
        sqlStat.AppendLine("    ,isnull(INFLG4, '') AS INFLG4")
        sqlStat.AppendLine("    ,isnull(INFLG5, '') AS INFLG5")
        sqlStat.AppendLine("    ,isnull(INFLG6, '') AS INFLG6")
        sqlStat.AppendLine("    ,isnull(INFLG7, '') AS INFLG7")
        sqlStat.AppendLine("    ,isnull(INFLG8, '') AS INFLG8")
        sqlStat.AppendLine("    ,isnull(INFLG9, '') AS INFLG9")
        sqlStat.AppendLine("    ,isnull(INFLG10,'') AS INFLG10")
        sqlStat.AppendLine("    ,isnull(INFLG11,'') AS INFLG11")
        sqlStat.AppendLine("    ,isnull(NAIYOU,'') AS NAIYOU")
        sqlStat.AppendLine("    ,isnull(FILE1,'') AS FILE1")
        sqlStat.AppendLine("    ,isnull(FILE2,'') AS FILE2")
        sqlStat.AppendLine("    ,isnull(FILE3,'') AS FILE3")
        sqlStat.AppendLine("    ,isnull(FILE4,'') AS FILE4")
        sqlStat.AppendLine("    ,isnull(FILE5,'') AS FILE5")
        sqlStat.AppendLine("    ,convert(nvarchar,isnull(DELFLG,null))      AS DELFLG")
        sqlStat.AppendLine("    ,convert(nvarchar,isnull(INITYMD,null))     AS INITYMD")
        sqlStat.AppendLine("    ,convert(nvarchar,isnull(INITUSER,null))    AS INITUSER")
        sqlStat.AppendLine("    ,convert(nvarchar,isnull(INITTERMID,null))  AS INITTERMID")
        sqlStat.AppendLine("    ,convert(nvarchar,isnull(UPDYMD,null))      AS UPDYMD")
        sqlStat.AppendLine("    ,convert(nvarchar,isnull(UPDUSER,null))     AS UPDUSER")
        sqlStat.AppendLine("    ,convert(nvarchar,isnull(UPDTERMID,null))   AS UPDTERMID")
        sqlStat.AppendLine("    ,convert(nvarchar,isnull(RECEIVEYMD,null))  AS RECEIVEYMD")
        sqlStat.AppendLine("  FROM OIL.OIM0020_GUIDANCE WITH(nolock)")
        sqlStat.AppendLine(" WHERE GUIDANCENO = @GUIDANCENO")
        'トランザクションしない場合は「sqlCon.BeginTransaction」→「nothing」
        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, sqlTran)
            sqlCmd.CommandTimeout = 300
            '固定パラメータ
            With sqlCmd.Parameters
                .Add("@GUIDANCENO", SqlDbType.NVarChar).Value = guidanceNo
            End With
            'ジャーナル用のデータ取得
            Using journalDt As New DataTable,
                  SQLdr As SqlDataReader = sqlCmd.ExecuteReader()
                For index As Integer = 0 To SQLdr.FieldCount - 1
                    journalDt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                Next

                journalDt.Load(SQLdr)

                CS0020JOURNAL.TABLENM = "OIM0020_GUIDANCE"
                CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                CS0020JOURNAL.ROW = journalDt.Rows(0)
                CS0020JOURNAL.CS0020JOURNAL()
                If Not isNormal(CS0020JOURNAL.ERR) Then
                    Master.Output(CS0020JOURNAL.ERR, C_MESSAGE_TYPE.ABORT, "CS0020JOURNAL JOURNAL")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                     'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "CS0020JOURNAL JOURNAL"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = "CS0020JOURNAL Call Err!"
                    CS0011LOGWrite.MESSAGENO = CS0020JOURNAL.ERR
                    CS0011LOGWrite.CS0011LOGWrite()                         'ログ出力
                    retMes.MessageNo = CS0020JOURNAL.ERR
                    retMes.Pram01 = "CS0020JOURNAL JOURNAL"
                    Return retMes
                End If
                journalDt.Clear()
            End Using 'journalDt,journalDt
        End Using 'tran,sqlCmd
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