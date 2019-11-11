''************************************************************
' タンク車マスタメンテ一覧画面
' 作成日 2019/11/08
' 更新日 2019/11/08
' 作成者 JOT遠藤
' 更新車 JOT遠藤
'
' 修正履歴:
'         :
''************************************************************
Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' タンク車マスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class OIM0005TankList
    Inherits Page

    '○ 検索結果格納Table
    Private OIM0005tbl As DataTable                                  '一覧格納用テーブル
    Private OIM0005INPtbl As DataTable                               'チェック用テーブル
    Private OIM0005UPDtbl As DataTable                               '更新用テーブル

    Private Const CONST_DISPROWCOUNT As Integer = 45                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 20                 'マウススクロール時稼働行数
    Private Const CONST_DETAIL_TABID As String = "DTL1"             '明細部ID

    '○ データOPERATION用
    Private Const CONST_INSERT As String = "Insert"                 'データ追加
    Private Const CONST_UPDATE As String = "Update"                 'データ更新
    Private Const CONST_PATTERNERR As String = "PATTEN ERR"         '関連チェックエラー

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

    ''' <summary>
    ''' サーバー処理の遷移先
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        Try
            If IsPostBack Then
                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    '○ 画面表示データ復元
                    Master.RecoverTable(OIM0005tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonINSERT"          '追加ボタン押下
                            WF_ButtonINSERT_Click()
                        Case "WF_ButtonUPDATE"          'DB更新ボタン押下
                            WF_ButtonUPDATE_Click()
                        Case "WF_ButtonCSV"             'ダウンロードボタン押下
                            WF_ButtonDownload_Click()
                        Case "WF_ButtonPrint"           '一覧印刷ボタン押下
                            WF_ButtonPrint_Click()
                        Case "WF_ButtonEND"             '終了ボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_ButtonFIRST"           '先頭頁ボタン押下
                            WF_ButtonFIRST_Click()
                        Case "WF_ButtonLAST"            '最終頁ボタン押下
                            WF_ButtonLAST_Click()
                        Case "WF_GridDBclick"           'GridViewダブルクリック
                            WF_Grid_DBClick()
                        Case "WF_MouseWheelUp"          'マウスホイール(Up)
                            WF_Grid_Scroll()
                        Case "WF_MouseWheelDown"        'マウスホイール(Down)
                            WF_Grid_Scroll()
                        Case "WF_EXCEL_UPLOAD"          'ファイルアップロード
                            WF_FILEUPLOAD()
                        Case "WF_UPDATE"                '表更新ボタン押下
                            WF_UPDATE_Click()
                        Case "WF_CLEAR"                 'クリアボタン押下
                            WF_CLEAR_Click()
                        Case "WF_Field_DBClick"         'フィールドダブルクリック
                            WF_FIELD_DBClick()
                        Case "WF_ButtonSel"             '(左ボックス)選択ボタン押下
                            WF_ButtonSel_Click()
                        Case "WF_ButtonCan"             '(左ボックス)キャンセルボタン押下
                            WF_ButtonCan_Click()
                        Case "WF_ListboxDBclick"        '左ボックスダブルクリック
                            WF_ButtonSel_Click()
                        Case "WF_RadioButonClick"       '(右ボックス)ラジオボタン選択
                            WF_RadioButton_Click()
                        Case "WF_MEMOChange"            '(右ボックス)メモ欄更新
                            WF_RIGHTBOX_Change()
                    End Select

                    '○ 一覧再表示処理
                    DisplayGrid()
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

        Finally
            '○ 格納Table Close
            If Not IsNothing(OIM0005tbl) Then
                OIM0005tbl.Clear()
                OIM0005tbl.Dispose()
                OIM0005tbl = Nothing
            End If

            If Not IsNothing(OIM0005INPtbl) Then
                OIM0005INPtbl.Clear()
                OIM0005INPtbl.Dispose()
                OIM0005INPtbl = Nothing
            End If

            If Not IsNothing(OIM0005UPDtbl) Then
                OIM0005UPDtbl.Clear()
                OIM0005UPDtbl.Dispose()
                OIM0005UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIM0005WRKINC.MAPIDL
        '○HELP表示有無設定
        Master.dispHelp = False
        '○D&D有無設定
        Master.eventDrop = True
        '○Grid情報保存先のファイル名
        Master.CreateXMLSaveFile()

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

        '○ GridView初期設定
        GridViewInitialize()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0005S Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
            '######### おためし ##########################
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0005C Then
            Master.RecoverTable(OIM0005tbl, work.WF_SEL_INPTBL.Text)
        End If

        '○ 名称設定処理
        CODENAME_get("CAMPCODE", work.WF_SEL_CAMPCODE.Text, WF_SEL_CAMPNAME.Text, WW_DUMMY)             '会社コード

    End Sub

    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        '######### おためし ##########################
        '登録画面からの遷移の場合はテーブルから取得しない
        If Context.Handler.ToString().ToUpper() <> C_PREV_MAP_LIST.OIM0005C Then
            '○ 画面表示データ取得
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                MAPDataGet(SQLcon)
            End Using
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIM0005tbl)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(OIM0005tbl)

        TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.CS0013ProfView()
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If

        '○ 先頭行に合わせる
        WF_GridPosition.Text = "1"

        TBLview.Dispose()
        TBLview = Nothing

    End Sub

    ''' <summary>
    ''' 画面表示データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MAPDataGet(ByVal SQLcon As SqlConnection)

        If IsNothing(OIM0005tbl) Then
            OIM0005tbl = New DataTable
        End If

        If OIM0005tbl.Columns.Count <> 0 Then
            OIM0005tbl.Columns.Clear()
        End If

        OIM0005tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データをタンク車マスタから取得する

        Dim SQLStr As String

        If work.WF_SEL_TANKNUMBER.Text = "" And
            work.WF_SEL_MODEL.Text = "" Then
            SQLStr =
              " SELECT " _
            & "   0                                     AS LINECNT " _
            & " , ''                                    AS OPERATION " _
            & " , CAST(OIM0005.UPDTIMSTP AS bigint)       AS UPDTIMSTP " _
            & " , 1                                     AS 'SELECT' " _
            & " , 0                                     AS HIDDEN " _
            & " , ISNULL(RTRIM(OIM0005.DELFLG), '')         AS DELFLG " _
            & " , ISNULL(RTRIM(OIM0005.TANKNUMBER), '')         AS TANKNUMBER " _
            & " , ISNULL(RTRIM(OIM0005.MODEL), '')         AS MODEL " _
            & " , ISNULL(RTRIM(OIM0005.MODELKANA), '')         AS MODELKANA " _
            & " , ISNULL(RTRIM(OIM0005.LOAD), '')         AS LOAD " _
            & " , ISNULL(RTRIM(OIM0005.LOADUNIT), '')         AS LOADUNIT " _
            & " , ISNULL(RTRIM(OIM0005.VOLUME), '')         AS VOLUME " _
            & " , ISNULL(RTRIM(OIM0005.VOLUMEUNIT), '')         AS VOLUMEUNIT " _
            & " , ISNULL(RTRIM(OIM0005.ORIGINOWNERCODE), '')         AS ORIGINOWNERCODE " _
            & " , ISNULL(RTRIM(OIM0005.ORIGINOWNERNAME), '')         AS ORIGINOWNERNAME " _
            & " , ISNULL(RTRIM(OIM0005.OWNERCODE), '')         AS OWNERCODE " _
            & " , ISNULL(RTRIM(OIM0005.OWNERNAME), '')         AS OWNERNAME " _
            & " , ISNULL(RTRIM(OIM0005.LEASECODE), '')         AS LEASECODE " _
            & " , ISNULL(RTRIM(OIM0005.LEASENAME), '')         AS LEASENAME " _
            & " , ISNULL(RTRIM(OIM0005.LEASECLASS), '')         AS LEASECLASS " _
            & " , ISNULL(RTRIM(OIM0005.LEASECLASSNEMAE), '')         AS LEASECLASSNEMAE " _
            & " , ISNULL(RTRIM(OIM0005.AUTOEXTENTION), '')         AS AUTOEXTENTION " _
            & " , CASE WHEN OIM0005.LEASESTYMD IS NULL THEN ''                   " _
            & "   ELSE FORMAT(OIM0005.LEASESTYMD,'yyyy/MM/dd')              " _
            & "   END                                     as LEASESTYMD   " _
            & " , CASE WHEN OIM0005.LEASEENDYMD IS NULL THEN ''                   " _
            & "   ELSE FORMAT(OIM0005.LEASEENDYMD,'yyyy/MM/dd')              " _
            & "   END                                     as LEASEENDYMD   " _
            & " , ISNULL(RTRIM(OIM0005.USERCODE), '')         AS USERCODE " _
            & " , ISNULL(RTRIM(OIM0005.USERNAME), '')         AS USERNAME " _
            & " , ISNULL(RTRIM(OIM0005.CURRENTSTATIONCODE), '')         AS CURRENTSTATIONCODE " _
            & " , ISNULL(RTRIM(OIM0005.CURRENTSTATIONNAME), '')         AS CURRENTSTATIONNAME " _
            & " , ISNULL(RTRIM(OIM0005.EXTRADINARYSTATIONCODE), '')         AS EXTRADINARYSTATIONCODE " _
            & " , ISNULL(RTRIM(OIM0005.EXTRADINARYSTATIONNAME), '')         AS EXTRADINARYSTATIONNAME " _
            & " , CASE WHEN OIM0005.USERLIMIT IS NULL THEN ''                   " _
            & "              ELSE FORMAT(OIM0005.USERLIMIT,'yyyy/MM/dd')              " _
            & "   END                                     as USERLIMIT   " _
            & " , CASE WHEN OIM0005.LIMITTEXTRADIARYSTATION IS NULL THEN ''                   " _
            & "              ELSE FORMAT(OIM0005.LIMITTEXTRADIARYSTATION,'yyyy/MM/dd')              " _
            & "   END                                     as LIMITTEXTRADIARYSTATION   " _
            & " , ISNULL(RTRIM(OIM0005.DEDICATETYPECODE), '')         AS DEDICATETYPECODE " _
            & " , ISNULL(RTRIM(OIM0005.DEDICATETYPENAME), '')         AS DEDICATETYPENAME " _
            & " , ISNULL(RTRIM(OIM0005.EXTRADINARYTYPECODE), '')         AS EXTRADINARYTYPECODE " _
            & " , ISNULL(RTRIM(OIM0005.EXTRADINARYTYPENAME), '')         AS EXTRADINARYTYPENAME " _
            & " , CASE WHEN OIM0005.EXTRADINARYLIMIT IS NULL THEN ''                   " _
            & "              ELSE FORMAT(OIM0005.EXTRADINARYLIMIT,'yyyy/MM/dd')              " _
            & "   END                                     as EXTRADINARYLIMIT   " _
            & " , ISNULL(RTRIM(OIM0005.OPERATIONBASECODE), '')         AS OPERATIONBASECODE " _
            & " , ISNULL(RTRIM(OIM0005.OPERATIONBASENAME), '')         AS OPERATIONBASENAME " _
            & " , ISNULL(RTRIM(OIM0005.COLORCODE), '')         AS COLORCODE " _
            & " , ISNULL(RTRIM(OIM0005.COLORNAME), '')         AS COLORNAME " _
            & " , ISNULL(RTRIM(OIM0005.ENEOS), '')         AS ENEOS " _
            & " , ISNULL(RTRIM(OIM0005.ECO), '')         AS ECO " _
            & " , ISNULL(RTRIM(OIM0005.RESERVE1), '')         AS RESERVE1 " _
            & " , ISNULL(RTRIM(OIM0005.RESERVE2), '')         AS RESERVE2 " _
            & " , CASE WHEN OIM0005.JRINSPECTIONDATE IS NULL THEN ''                   " _
            & "              ELSE FORMAT(OIM0005.JRINSPECTIONDATE,'yyyy/MM/dd')              " _
            & "   END                                     as JRINSPECTIONDATE   " _
            & " , CASE WHEN OIM0005.INSPECTIONDATE IS NULL THEN ''                   " _
            & "              ELSE FORMAT(OIM0005.INSPECTIONDATE,'yyyy/MM/dd')              " _
            & "   END                                     as INSPECTIONDATE   " _
            & " , CASE WHEN OIM0005.JRSPECIFIEDDATE IS NULL THEN ''                   " _
            & "              ELSE FORMAT(OIM0005.JRSPECIFIEDDATE,'yyyy/MM/dd')              " _
            & "   END                                     as JRSPECIFIEDDATE   " _
            & " , CASE WHEN OIM0005.SPECIFIEDDATE IS NULL THEN ''                   " _
            & "              ELSE FORMAT(OIM0005.SPECIFIEDDATE,'yyyy/MM/dd')              " _
            & "   END                                     as SPECIFIEDDATE   " _
            & " , CASE WHEN OIM0005.JRALLINSPECTIONDATE IS NULL THEN ''                   " _
            & "              ELSE FORMAT(OIM0005.JRALLINSPECTIONDATE,'yyyy/MM/dd')              " _
            & "   END                                     as JRALLINSPECTIONDATE   " _
            & " , CASE WHEN OIM0005.ALLINSPECTIONDATE IS NULL THEN ''                   " _
            & "              ELSE FORMAT(OIM0005.ALLINSPECTIONDATE,'yyyy/MM/dd')              " _
            & "   END                                     as ALLINSPECTIONDATE   " _
            & " , CASE WHEN OIM0005.TRANSFERDATE IS NULL THEN ''                   " _
            & "              ELSE FORMAT(OIM0005.TRANSFERDATE,'yyyy/MM/dd')              " _
            & "   END                                     as TRANSFERDATE   " _
            & " , ISNULL(RTRIM(OIM0005.OBTAINEDCODE), '')         AS OBTAINEDCODE " _
            & " , CAST(ISNULL(RTRIM(OIM0005.PROGRESSYEAR), '') AS VarChar)         AS PROGRESSYEAR " _
            & " , CAST(ISNULL(RTRIM(OIM0005.NEXTPROGRESSYEAR), '') AS VarChar)         AS NEXTPROGRESSYEAR " _
            & " , ISNULL(RTRIM(OIM0005.JRTANKNUMBER), '')         AS JRTANKNUMBER " _
            & " , ISNULL(RTRIM(OIM0005.OLDTANKNUMBER), '')         AS OLDTANKNUMBER " _
            & " , ISNULL(RTRIM(OIM0005.OTTANKNUMBER), '')         AS OTTANKNUMBER " _
            & " , ISNULL(RTRIM(OIM0005.JXTGTANKNUMBER), '')         AS JXTGTANKNUMBER " _
            & " , ISNULL(RTRIM(OIM0005.COSMOTANKNUMBER), '')         AS COSMOTANKNUMBER " _
            & " , ISNULL(RTRIM(OIM0005.FUJITANKNUMBER), '')         AS FUJITANKNUMBER " _
            & " , ISNULL(RTRIM(OIM0005.SHELLTANKNUMBER), '')         AS SHELLTANKNUMBER " _
            & " , ISNULL(RTRIM(OIM0005.RESERVE3), '')         AS RESERVE3 " _
            & " FROM OIL.OIM0005_TANK OIM0005 " _
            & " WHERE OIM0005.DELFLG      <> @P3"
        Else
            SQLStr =
              " SELECT " _
            & "   0                                     AS LINECNT " _
            & " , ''                                    AS OPERATION " _
            & " , CAST(OIM0005.UPDTIMSTP AS bigint)       AS UPDTIMSTP " _
            & " , 1                                     AS 'SELECT' " _
            & " , 0                                     AS HIDDEN " _
            & " , ISNULL(RTRIM(OIM0005.DELFLG), '')         AS DELFLG " _
            & " , ISNULL(RTRIM(OIM0005.TANKNUMBER), '')         AS TANKNUMBER " _
            & " , ISNULL(RTRIM(OIM0005.MODEL), '')         AS MODEL " _
            & " , ISNULL(RTRIM(OIM0005.MODELKANA), '')         AS MODELKANA " _
            & " , ISNULL(RTRIM(OIM0005.LOAD), '')         AS LOAD " _
            & " , ISNULL(RTRIM(OIM0005.LOADUNIT), '')         AS LOADUNIT " _
            & " , ISNULL(RTRIM(OIM0005.VOLUME), '')         AS VOLUME " _
            & " , ISNULL(RTRIM(OIM0005.VOLUMEUNIT), '')         AS VOLUMEUNIT " _
            & " , ISNULL(RTRIM(OIM0005.ORIGINOWNERCODE), '')         AS ORIGINOWNERCODE " _
            & " , ISNULL(RTRIM(OIM0005.ORIGINOWNERNAME), '')         AS ORIGINOWNERNAME " _
            & " , ISNULL(RTRIM(OIM0005.OWNERCODE), '')         AS OWNERCODE " _
            & " , ISNULL(RTRIM(OIM0005.OWNERNAME), '')         AS OWNERNAME " _
            & " , ISNULL(RTRIM(OIM0005.LEASECODE), '')         AS LEASECODE " _
            & " , ISNULL(RTRIM(OIM0005.LEASENAME), '')         AS LEASENAME " _
            & " , ISNULL(RTRIM(OIM0005.LEASECLASS), '')         AS LEASECLASS " _
            & " , ISNULL(RTRIM(OIM0005.LEASECLASSNEMAE), '')         AS LEASECLASSNEMAE " _
            & " , ISNULL(RTRIM(OIM0005.AUTOEXTENTION), '')         AS AUTOEXTENTION " _
            & " , CASE WHEN OIM0005.LEASESTYMD IS NULL THEN ''                   " _
            & "   ELSE FORMAT(OIM0005.LEASESTYMD,'yyyy/MM/dd')              " _
            & "   END                                     as LEASESTYMD   " _
            & " , CASE WHEN OIM0005.LEASEENDYMD IS NULL THEN ''                   " _
            & "   ELSE FORMAT(OIM0005.LEASEENDYMD,'yyyy/MM/dd')              " _
            & "   END                                     as LEASEENDYMD   " _
            & " , ISNULL(RTRIM(OIM0005.USERCODE), '')         AS USERCODE " _
            & " , ISNULL(RTRIM(OIM0005.USERNAME), '')         AS USERNAME " _
            & " , ISNULL(RTRIM(OIM0005.CURRENTSTATIONCODE), '')         AS CURRENTSTATIONCODE " _
            & " , ISNULL(RTRIM(OIM0005.CURRENTSTATIONNAME), '')         AS CURRENTSTATIONNAME " _
            & " , ISNULL(RTRIM(OIM0005.EXTRADINARYSTATIONCODE), '')         AS EXTRADINARYSTATIONCODE " _
            & " , ISNULL(RTRIM(OIM0005.EXTRADINARYSTATIONNAME), '')         AS EXTRADINARYSTATIONNAME " _
            & " , CASE WHEN OIM0005.USERLIMIT IS NULL THEN ''                   " _
            & "              ELSE FORMAT(OIM0005.USERLIMIT,'yyyy/MM/dd')              " _
            & "   END                                     as USERLIMIT   " _
            & " , CASE WHEN OIM0005.LIMITTEXTRADIARYSTATION IS NULL THEN ''                   " _
            & "              ELSE FORMAT(OIM0005.LIMITTEXTRADIARYSTATION,'yyyy/MM/dd')              " _
            & "   END                                     as LIMITTEXTRADIARYSTATION   " _
            & " , ISNULL(RTRIM(OIM0005.DEDICATETYPECODE), '')         AS DEDICATETYPECODE " _
            & " , ISNULL(RTRIM(OIM0005.DEDICATETYPENAME), '')         AS DEDICATETYPENAME " _
            & " , ISNULL(RTRIM(OIM0005.EXTRADINARYTYPECODE), '')         AS EXTRADINARYTYPECODE " _
            & " , ISNULL(RTRIM(OIM0005.EXTRADINARYTYPENAME), '')         AS EXTRADINARYTYPENAME " _
            & " , CASE WHEN OIM0005.EXTRADINARYLIMIT IS NULL THEN ''                   " _
            & "              ELSE FORMAT(OIM0005.EXTRADINARYLIMIT,'yyyy/MM/dd')              " _
            & "   END                                     as EXTRADINARYLIMIT   " _
            & " , ISNULL(RTRIM(OIM0005.OPERATIONBASECODE), '')         AS OPERATIONBASECODE " _
            & " , ISNULL(RTRIM(OIM0005.OPERATIONBASENAME), '')         AS OPERATIONBASENAME " _
            & " , ISNULL(RTRIM(OIM0005.COLORCODE), '')         AS COLORCODE " _
            & " , ISNULL(RTRIM(OIM0005.COLORNAME), '')         AS COLORNAME " _
            & " , ISNULL(RTRIM(OIM0005.ENEOS), '')         AS ENEOS " _
            & " , ISNULL(RTRIM(OIM0005.ECO), '')         AS ECO " _
            & " , ISNULL(RTRIM(OIM0005.RESERVE1), '')         AS RESERVE1 " _
            & " , ISNULL(RTRIM(OIM0005.RESERVE2), '')         AS RESERVE2 " _
            & " , CASE WHEN OIM0005.JRINSPECTIONDATE IS NULL THEN ''                   " _
            & "              ELSE FORMAT(OIM0005.JRINSPECTIONDATE,'yyyy/MM/dd')              " _
            & "   END                                     as JRINSPECTIONDATE   " _
            & " , CASE WHEN OIM0005.INSPECTIONDATE IS NULL THEN ''                   " _
            & "              ELSE FORMAT(OIM0005.INSPECTIONDATE,'yyyy/MM/dd')              " _
            & "   END                                     as INSPECTIONDATE   " _
            & " , CASE WHEN OIM0005.JRSPECIFIEDDATE IS NULL THEN ''                   " _
            & "              ELSE FORMAT(OIM0005.JRSPECIFIEDDATE,'yyyy/MM/dd')              " _
            & "   END                                     as JRSPECIFIEDDATE   " _
            & " , CASE WHEN OIM0005.SPECIFIEDDATE IS NULL THEN ''                   " _
            & "              ELSE FORMAT(OIM0005.SPECIFIEDDATE,'yyyy/MM/dd')              " _
            & "   END                                     as SPECIFIEDDATE   " _
            & " , CASE WHEN OIM0005.JRALLINSPECTIONDATE IS NULL THEN ''                   " _
            & "              ELSE FORMAT(OIM0005.JRALLINSPECTIONDATE,'yyyy/MM/dd')              " _
            & "   END                                     as JRALLINSPECTIONDATE   " _
            & " , CASE WHEN OIM0005.ALLINSPECTIONDATE IS NULL THEN ''                   " _
            & "              ELSE FORMAT(OIM0005.ALLINSPECTIONDATE,'yyyy/MM/dd')              " _
            & "   END                                     as ALLINSPECTIONDATE   " _
            & " , CASE WHEN OIM0005.TRANSFERDATE IS NULL THEN ''                   " _
            & "              ELSE FORMAT(OIM0005.TRANSFERDATE,'yyyy/MM/dd')              " _
            & "   END                                     as TRANSFERDATE   " _
            & " , ISNULL(RTRIM(OIM0005.OBTAINEDCODE), '')         AS OBTAINEDCODE " _
            & " , CAST(ISNULL(RTRIM(OIM0005.PROGRESSYEAR), '') AS VarChar)         AS PROGRESSYEAR " _
            & " , CAST(ISNULL(RTRIM(OIM0005.NEXTPROGRESSYEAR), '') AS VarChar)         AS NEXTPROGRESSYEAR " _
            & " , ISNULL(RTRIM(OIM0005.JRTANKNUMBER), '')         AS JRTANKNUMBER " _
            & " , ISNULL(RTRIM(OIM0005.OLDTANKNUMBER), '')         AS OLDTANKNUMBER " _
            & " , ISNULL(RTRIM(OIM0005.OTTANKNUMBER), '')         AS OTTANKNUMBER " _
            & " , ISNULL(RTRIM(OIM0005.JXTGTANKNUMBER), '')         AS JXTGTANKNUMBER " _
            & " , ISNULL(RTRIM(OIM0005.COSMOTANKNUMBER), '')         AS COSMOTANKNUMBER " _
            & " , ISNULL(RTRIM(OIM0005.FUJITANKNUMBER), '')         AS FUJITANKNUMBER " _
            & " , ISNULL(RTRIM(OIM0005.SHELLTANKNUMBER), '')         AS SHELLTANKNUMBER " _
            & " , ISNULL(RTRIM(OIM0005.RESERVE3), '')         AS RESERVE3 " _
            & " FROM OIL.OIM0005_TANK OIM0005 " _
            & " WHERE OIM0005.TANKNUMBER = @P1" _
            & "   OR OIM0005.MODEL = @P2" _
            & "   AND OIM0005.DELFLG      <> @P3"
        End If

        ''○ 条件指定で指定されたものでSQLで可能なものを追加する
        ''JOT車番
        'If Not String.IsNullOrEmpty(work.WF_SEL_TANKNUMBER.Text) Then
        '    SQLStr &= String.CONVERT(DATE, "    AND OIM0005.TANKNUMBER = '{0}'", work.WF_SEL_TANKNUMBER.Text)
        'End If

        SQLStr &=
              " ORDER BY" _
            & "    OIM0005.TANKNUMBER"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 8)        'JOT車番
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)       '型式
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 1)        '削除フラグ

                PARA1.Value = work.WF_SEL_TANKNUMBER.Text
                PARA2.Value = work.WF_SEL_MODEL.Text
                PARA3.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIM0005tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIM0005tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIM0005row As DataRow In OIM0005tbl.Rows
                    i += 1
                    OIM0005row("LINECNT") = i        'LINECNT

                    '取引先名称(出荷先)
                    'CODENAME_get("TORICODES", OIM0005row("TORICODES"), OIM0005row("TORINAMES"), WW_DUMMY)
                    'work.WF_SEL_TORICODES.Text = OIM0005row("TORICODES")

                    ''出荷場所名称
                    'CODENAME_get("SHUKABASHO", OIM0005row("SHUKABASHO"), OIM0005row("SHUKABASHONAMES"), WW_DUMMY)

                    ''取引先名称(届先)
                    'CODENAME_get("TORICODET", OIM0005row("TORICODET"), OIM0005row("TORINAMET"), WW_DUMMY)
                    'work.WF_SEL_TORICODET.Text = OIM0005row("TORICODET")

                    ''届先名称
                    'CODENAME_get("TODOKECODE", OIM0005row("TODOKECODE"), OIM0005row("TODOKENAME"), WW_DUMMY)
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0005L SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0005L Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each OIM0005row As DataRow In OIM0005tbl.Rows
            If OIM0005row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                OIM0005row("SELECT") = WW_DataCNT
            End If
        Next

        '○ 表示LINECNT取得
        If WF_GridPosition.Text = "" Then
            WW_GridPosition = 1
        Else
            Try
                Integer.TryParse(WF_GridPosition.Text, WW_GridPosition)
            Catch ex As Exception
                WW_GridPosition = 1
            End Try
        End If

        '○ 表示格納位置決定

        '表示開始_格納位置決定(次頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelUp" Then
            If (WW_GridPosition + CONST_SCROLLCOUNT) <= WW_DataCNT Then
                WW_GridPosition += CONST_SCROLLCOUNT
            End If
        End If

        '表示開始_格納位置決定(前頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelDown" Then
            If (WW_GridPosition - CONST_SCROLLCOUNT) > 0 Then
                WW_GridPosition -= CONST_SCROLLCOUNT
            Else
                WW_GridPosition = 1
            End If
        End If

        '○ 画面(GridView)表示
        Dim TBLview As DataView = New DataView(OIM0005tbl)

        '○ ソート
        TBLview.Sort = "LINECNT"
        TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DISPROWCOUNT).ToString()

        '○ 一覧作成
        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.CS0013ProfView()

        '○ クリア
        If TBLview.Count = 0 Then
            WF_GridPosition.Text = "1"
        Else
            WF_GridPosition.Text = TBLview.Item(0)("SELECT")
        End If

        TBLview.Dispose()
        TBLview = Nothing

    End Sub

    ''' <summary>
    ''' 追加ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonINSERT_Click()

        '選択行
        WF_Sel_LINECNT.Text = ""
        work.WF_SEL_LINECNT.Text = ""

        'JOT車番
        WF_TANKNUMBER.Text = ""
        work.WF_SEL_TANKNUMBER2.Text = ""

        '原籍所有者C
        WF_ORIGINOWNERCODE.Text = ""
        work.WF_SEL_ORIGINOWNERCODE.Text = ""

        '名義所有者C
        WF_OWNERCODE.Text = ""
        work.WF_SEL_OWNERCODE.Text = ""

        'リース先C
        WF_LEASECODE.Text = ""
        work.WF_SEL_LEASECODE.Text = ""

        'リース区分C
        WF_LEASECLASS.Text = ""
        work.WF_SEL_LEASECLASS.Text = ""

        '自動延長
        WF_AUTOEXTENTION.Text = ""
        work.WF_SEL_AUTOEXTENTION.Text = ""

        'リース開始年月日
        WF_LEASESTYMD.Text = ""
        work.WF_SEL_LEASESTYMD.Text = ""

        'リース満了年月日
        WF_LEASEENDYMD.Text = ""
        work.WF_SEL_LEASEENDYMD.Text = ""

        '第三者使用者C
        WF_USERCODE.Text = ""
        work.WF_SEL_USERCODE.Text = ""

        '原常備駅C
        WF_CURRENTSTATIONCODE.Text = ""
        work.WF_SEL_CURRENTSTATIONCODE.Text = ""

        '臨時常備駅C
        WF_EXTRADINARYSTATIONCODE.Text = ""
        work.WF_SEL_EXTRADINARYSTATIONCODE.Text = ""

        '第三者使用期限
        WF_USERLIMIT.Text = ""
        work.WF_SEL_USERLIMIT.Text = ""

        '臨時常備駅期限
        WF_LIMITTEXTRADIARYSTATION.Text = ""
        work.WF_SEL_LIMITTEXTRADIARYSTATION.Text = ""

        '原専用種別C
        WF_DEDICATETYPECODE.Text = ""
        work.WF_SEL_DEDICATETYPECODE.Text = ""

        '臨時専用種別C
        WF_EXTRADINARYTYPECODE.Text = ""
        work.WF_SEL_EXTRADINARYTYPECODE.Text = ""

        '臨時専用期限
        WF_EXTRADINARYLIMIT.Text = ""
        work.WF_SEL_EXTRADINARYLIMIT.Text = ""

        '運用基地C
        WF_OPERATIONBASECODE.Text = ""
        work.WF_SEL_OPERATIONBASECODE.Text = ""

        '塗色C
        WF_COLORCODE.Text = ""
        work.WF_SEL_COLORCODE.Text = ""

        'エネオス
        WF_ENEOS.Text = ""
        work.WF_SEL_ENEOS.Text = ""

        'エコレール
        WF_ECO.Text = ""
        work.WF_SEL_ECO.Text = ""

        '取得年月日
        WF_ALLINSPECTIONDATE.Text = ""
        work.WF_SEL_ALLINSPECTIONDATE.Text = ""

        '車籍編入年月日
        WF_TRANSFERDATE.Text = ""
        work.WF_SEL_TRANSFERDATE.Text = ""

        '取得先C
        WF_OBTAINEDCODE.Text = ""
        work.WF_SEL_OBTAINEDCODE.Text = ""

        '形式
        WF_MODEL.Text = ""
        work.WF_SEL_MODEL.Text = ""

        '形式カナ
        WF_MODELKANA.Text = ""
        work.WF_SEL_MODELKANA.Text = ""

        '荷重
        WF_LOAD.Text = ""
        work.WF_SEL_LOAD.Text = ""

        '荷重単位
        WF_LOADUNIT.Text = ""
        work.WF_SEL_LOADUNIT.Text = ""

        '容積
        WF_VOLUME.Text = ""
        work.WF_SEL_VOLUME.Text = ""

        '容積単位
        WF_VOLUMEUNIT.Text = ""
        work.WF_SEL_VOLUMEUNIT.Text = ""

        '原籍所有者
        WF_ORIGINOWNERNAME.Text = ""
        work.WF_SEL_ORIGINOWNERNAME.Text = ""

        '名義所有者
        WF_OWNERNAME.Text = ""
        work.WF_SEL_OWNERNAME.Text = ""

        'リース先
        WF_LEASENAME.Text = ""
        work.WF_SEL_LEASENAME.Text = ""

        'リース区分
        WF_LEASECLASSNEMAE.Text = ""
        work.WF_SEL_LEASECLASSNEMAE.Text = ""

        '第三者使用者
        WF_USERNAME.Text = ""
        work.WF_SEL_USERNAME.Text = ""

        '原常備駅
        WF_CURRENTSTATIONNAME.Text = ""
        work.WF_SEL_CURRENTSTATIONNAME.Text = ""

        '臨時常備駅
        WF_EXTRADINARYSTATIONNAME.Text = ""
        work.WF_SEL_EXTRADINARYSTATIONNAME.Text = ""

        '原専用種別
        WF_DEDICATETYPENAME.Text = ""
        work.WF_SEL_DEDICATETYPENAME.Text = ""

        '臨時専用種別
        WF_EXTRADINARYTYPENAME.Text = ""
        work.WF_SEL_EXTRADINARYTYPENAME.Text = ""

        '運用場所
        WF_OPERATIONBASENAME.Text = ""
        work.WF_SEL_OPERATIONBASENAME.Text = ""

        '塗色
        WF_COLORNAME.Text = ""
        work.WF_SEL_COLORNAME.Text = ""

        '予備1
        WF_RESERVE1.Text = ""
        work.WF_SEL_RESERVE1.Text = ""

        '予備2
        WF_RESERVE2.Text = ""
        work.WF_SEL_RESERVE2.Text = ""

        '次回指定年月日
        WF_SPECIFIEDDATE.Text = ""
        work.WF_SEL_SPECIFIEDDATE.Text = ""

        '次回全検年月日(JR) 
        WF_JRALLINSPECTIONDATE.Text = ""
        work.WF_SEL_JRALLINSPECTIONDATE.Text = ""

        '現在経年
        WF_PROGRESSYEAR.Text = ""
        work.WF_SEL_PROGRESSYEAR.Text = ""

        '次回全検時経年
        WF_NEXTPROGRESSYEAR.Text = ""
        work.WF_SEL_NEXTPROGRESSYEAR.Text = ""

        '次回交検年月日(JR）
        WF_JRINSPECTIONDATE.Text = ""
        work.WF_SEL_JRINSPECTIONDATE.Text = ""

        '次回交検年月日
        WF_INSPECTIONDATE.Text = ""
        work.WF_SEL_INSPECTIONDATE.Text = ""

        '次回指定年月日(JR)
        WF_JRSPECIFIEDDATE.Text = ""
        work.WF_SEL_JRSPECIFIEDDATE.Text = ""

        'JR車番
        WF_JRTANKNUMBER.Text = ""
        work.WF_SEL_JRTANKNUMBER.Text = ""

        '旧JOT車番
        WF_OLDTANKNUMBER.Text = ""
        work.WF_SEL_OLDTANKNUMBER.Text = ""

        'OT車番
        WF_OTTANKNUMBER.Text = ""
        work.WF_SEL_OTTANKNUMBER.Text = ""

        'JXTG車番
        WF_JXTGTANKNUMBER.Text = ""
        work.WF_SEL_JXTGTANKNUMBER.Text = ""

        'コスモ車番
        WF_COSMOTANKNUMBER.Text = ""
        work.WF_SEL_COSMOTANKNUMBER.Text = ""

        '富士石油車番
        WF_FUJITANKNUMBER.Text = ""
        work.WF_SEL_FUJITANKNUMBER.Text = ""

        '出光昭シ車番
        WF_SHELLTANKNUMBER.Text = ""
        work.WF_SEL_SHELLTANKNUMBER.Text = ""

        '予備
        WF_RESERVE3.Text = ""
        work.WF_SEL_RESERVE3.Text = ""

        '削除
        WF_DELFLG.Text = "0"
        CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_DUMMY)

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(OIM0005tbl)

        WF_GridDBclick.Text = ""

        '############# おためし #############
        work.WF_SEL_DELFLG.Text = "0"

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存
        Master.SaveTable(OIM0005tbl, work.WF_SEL_INPTBL.Text)

        '○ 次ページ遷移
        Master.TransitionPage()

    End Sub

    ''' <summary>
    ''' DB更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        Dim WW_RESULT As String = ""

        '○関連チェック
        RelatedCheck(WW_ERRCODE)

        '○ 同一レコードチェック
        If isNormal(WW_ERRCODE) Then
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                'マスタ更新
                UpdateMaster(SQLcon)
            End Using
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIM0005tbl)

        '○ GridView初期設定
        '○ 画面表示データ再取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(OIM0005tbl)

        '○ 詳細画面クリア
        If isNormal(WW_ERRCODE) Then
            DetailBoxClear()
        End If

        '○ メッセージ表示
        If Not isNormal(WW_ERRCODE) Then
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
        End If

    End Sub

    ''' <summary>
    ''' 登録データ関連チェック
    ''' </summary>
    ''' <param name="O_RTNCODE"></param>
    ''' <remarks></remarks>
    Protected Sub RelatedCheck(ByRef O_RTNCODE As String)

        '○初期値設定
        O_RTNCODE = C_MESSAGE_NO.NORMAL

        Dim WW_LINEERR_SW As String = ""
        Dim WW_DUMMY As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""

        '○同一レコードチェック
        '※開始終了期間を持っていないため現状意味無し
        'For Each OIM0005row As DataRow In OIM0005tbl.Rows
        '    '読み飛ばし
        '    If OIM0005row("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING OrElse
        '        OIM0005row("DELFLG") = C_DELETE_FLG.DELETE Then
        '        Continue For
        '    End If

        '    WW_LINEERR_SW = ""

        '    '期間重複チェック
        '    For Each checkRow As DataRow In OIM0005tbl.Rows
        '        '同一KEY以外は読み飛ばし
        '        If checkRow("CAMPCODE") = OIM0005row("CAMPCODE") AndAlso
        '            checkRow("UORG") = OIM0005row("UORG") AndAlso
        '            checkRow("MODELPATTERN") = OIM0005row("MODELPATTERN") AndAlso
        '            checkRow("TORICODES") = OIM0005row("TORICODES") AndAlso
        '            checkRow("SHUKABASHO") = OIM0005row("SHUKABASHO") AndAlso
        '            checkRow("TORICODET") = OIM0005row("TORICODET") AndAlso
        '            checkRow("TODOKECODE") = OIM0005row("TODOKECODE") Then
        '        Else
        '            Continue For
        '        End If
        '    Next

        '    If WW_LINEERR_SW = "" Then
        '        If OIM0005row("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
        '            OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        '        End If
        '    Else
        '        OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
        '    End If
        'Next

    End Sub


    ''' <summary>
    ''' タンク車マスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateMaster(ByVal SQLcon As SqlConnection)

        '○ ＤＢ更新
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;" _
            & "    SET @hensuu = 0 ;" _
            & " DECLARE hensuu CURSOR FOR" _
            & "    SELECT" _
            & "        CAST(UPDTIMSTP AS bigint) AS hensuu" _
            & "    FROM" _
            & "        OIL.OIM0005_TANK" _
            & "    WHERE" _
            & "        TANKNUMBER       = @P01 ;" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE OIL.OIM0005_TANK" _
            & "    SET" _
            & "        DELFLG = @P00" _
            & "        , ORIGINOWNERCODE = @P02" _
            & "        , OWNERCODE = @P03" _
            & "        , LEASECODE = @P04" _
            & "        , LEASECLASS = @P05" _
            & "        , AUTOEXTENTION = @P06" _
            & "        , LEASESTYMD = @P07" _
            & "        , LEASEENDYMD = @P08" _
            & "        , USERCODE = @P09" _
            & "        , CURRENTSTATIONCODE = @P10" _
            & "        , EXTRADINARYSTATIONCODE = @P11" _
            & "        , USERLIMIT = @P12" _
            & "        , LIMITTEXTRADIARYSTATION = @P13" _
            & "        , DEDICATETYPECODE = @P14" _
            & "        , EXTRADINARYTYPECODE = @P15" _
            & "        , EXTRADINARYLIMIT = @P16" _
            & "        , OPERATIONBASECODE = @P17" _
            & "        , COLORCODE = @P18" _
            & "        , ENEOS = @P19" _
            & "        , ECO = @P20" _
            & "        , ALLINSPECTIONDATE = @P21" _
            & "        , TRANSFERDATE = @P22" _
            & "        , OBTAINEDCODE = @P23" _
            & "        , MODEL = @P24" _
            & "        , MODELKANA = @P25" _
            & "        , LOAD = @P26" _
            & "        , LOADUNIT = @P27" _
            & "        , VOLUME = @P28" _
            & "        , VOLUMEUNIT = @P29" _
            & "        , ORIGINOWNERNAME = @P30" _
            & "        , OWNERNAME = @P31" _
            & "        , LEASENAME = @P32" _
            & "        , LEASECLASSNEMAE = @P33" _
            & "        , USERNAME = @P34" _
            & "        , CURRENTSTATIONNAME = @P35" _
            & "        , EXTRADINARYSTATIONNAME = @P36" _
            & "        , DEDICATETYPENAME = @P37" _
            & "        , EXTRADINARYTYPENAME = @P38" _
            & "        , OPERATIONBASENAME = @P39" _
            & "        , COLORNAME = @P40" _
            & "        , RESERVE1 = @P41" _
            & "        , RESERVE2 = @P42" _
            & "        , SPECIFIEDDATE = @P43" _
            & "        , JRALLINSPECTIONDATE = @P44" _
            & "        , PROGRESSYEAR = @P45" _
            & "        , NEXTPROGRESSYEAR = @P46" _
            & "        , JRINSPECTIONDATE = @P47" _
            & "        , INSPECTIONDATE = @P48" _
            & "        , JRSPECIFIEDDATE = @P49" _
            & "        , JRTANKNUMBER = @P50" _
            & "        , OLDTANKNUMBER = @P51" _
            & "        , OTTANKNUMBER = @P52" _
            & "        , JXTGTANKNUMBER = @P53" _
            & "        , COSMOTANKNUMBER = @P54" _
            & "        , FUJITANKNUMBER = @P55" _
            & "        , SHELLTANKNUMBER = @P56" _
            & "        , RESERVE3 = @P57" _
            & "        , INITYMD = @P58" _
            & "        , INITUSER = @P59" _
            & "        , INITTERMID = @P60" _
            & "        , UPDYMD = @P61" _
            & "        , UPDUSER = @P62" _
            & "        , UPDTERMID = @P63" _
            & "        , RECEIVEYMD = @P64" _
            & "    WHERE" _
            & "        TANKNUMBER       = @P01 ;" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO OIL.OIM0005_TANK" _
            & "        (DELFLG" _
            & "        , TANKNUMBER" _
            & "        , ORIGINOWNERCODE" _
            & "        , OWNERCODE" _
            & "        , LEASECODE" _
            & "        , LEASECLASS" _
            & "        , AUTOEXTENTION" _
            & "        , LEASESTYMD" _
            & "        , LEASEENDYMD" _
            & "        , USERCODE" _
            & "        , CURRENTSTATIONCODE" _
            & "        , EXTRADINARYSTATIONCODE" _
            & "        , USERLIMIT" _
            & "        , LIMITTEXTRADIARYSTATION" _
            & "        , DEDICATETYPECODE" _
            & "        , EXTRADINARYTYPECODE" _
            & "        , EXTRADINARYLIMIT" _
            & "        , OPERATIONBASECODE" _
            & "        , COLORCODE" _
            & "        , ENEOS" _
            & "        , ECO" _
            & "        , ALLINSPECTIONDATE" _
            & "        , TRANSFERDATE" _
            & "        , OBTAINEDCODE" _
            & "        , MODEL" _
            & "        , MODELKANA" _
            & "        , LOAD" _
            & "        , LOADUNIT" _
            & "        , VOLUME" _
            & "        , VOLUMEUNIT" _
            & "        , ORIGINOWNERNAME" _
            & "        , OWNERNAME" _
            & "        , LEASENAME" _
            & "        , LEASECLASSNEMAE" _
            & "        , USERNAME" _
            & "        , CURRENTSTATIONNAME" _
            & "        , EXTRADINARYSTATIONNAME" _
            & "        , DEDICATETYPENAME" _
            & "        , EXTRADINARYTYPENAME" _
            & "        , OPERATIONBASENAME" _
            & "        , COLORNAME" _
            & "        , RESERVE1" _
            & "        , RESERVE2" _
            & "        , SPECIFIEDDATE" _
            & "        , JRALLINSPECTIONDATE" _
            & "        , PROGRESSYEAR" _
            & "        , NEXTPROGRESSYEAR" _
            & "        , JRINSPECTIONDATE" _
            & "        , INSPECTIONDATE" _
            & "        , JRSPECIFIEDDATE" _
            & "        , JRTANKNUMBER" _
            & "        , OLDTANKNUMBER" _
            & "        , OTTANKNUMBER" _
            & "        , JXTGTANKNUMBER" _
            & "        , COSMOTANKNUMBER" _
            & "        , FUJITANKNUMBER" _
            & "        , SHELLTANKNUMBER" _
            & "        , RESERVE3" _
            & "        , INITYMD" _
            & "        , INITUSER" _
            & "        , INITTERMID" _
            & "        , UPDYMD" _
            & "        , UPDUSER" _
            & "        , UPDTERMID" _
            & "        , RECEIVEYMD)" _
            & "    VALUES" _
            & "        (@P00" _
            & "        , @P01" _
            & "        , @P02" _
            & "        , @P03" _
            & "        , @P04" _
            & "        , @P05" _
            & "        , @P06" _
            & "        , @P07" _
            & "        , @P08" _
            & "        , @P09" _
            & "        , @P10" _
            & "        , @P11" _
            & "        , @P12" _
            & "        , @P13" _
            & "        , @P14" _
            & "        , @P15" _
            & "        , @P16" _
            & "        , @P17" _
            & "        , @P18" _
            & "        , @P19" _
            & "        , @P20" _
            & "        , @P21" _
            & "        , @P22" _
            & "        , @P23" _
            & "        , @P24" _
            & "        , @P25" _
            & "        , @P26" _
            & "        , @P27" _
            & "        , @P28" _
            & "        , @P29" _
            & "        , @P30" _
            & "        , @P31" _
            & "        , @P32" _
            & "        , @P33" _
            & "        , @P34" _
            & "        , @P35" _
            & "        , @P36" _
            & "        , @P37" _
            & "        , @P38" _
            & "        , @P39" _
            & "        , @P40" _
            & "        , @P41" _
            & "        , @P42" _
            & "        , @P43" _
            & "        , @P44" _
            & "        , @P45" _
            & "        , @P46" _
            & "        , @P47" _
            & "        , @P48" _
            & "        , @P49" _
            & "        , @P50" _
            & "        , @P51" _
            & "        , @P52" _
            & "        , @P53" _
            & "        , @P54" _
            & "        , @P55" _
            & "        , @P56" _
            & "        , @P57" _
            & "        , @P58" _
            & "        , @P59" _
            & "        , @P60" _
            & "        , @P61" _
            & "        , @P62" _
            & "        , @P63" _
            & "        , @P64) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
              " Select" _
            & "    DELFLG" _
            & "    , TANKNUMBER" _
            & "    , ORIGINOWNERCODE" _
            & "    , OWNERCODE" _
            & "    , LEASECODE" _
            & "    , LEASECLASS" _
            & "    , AUTOEXTENTION" _
            & "    , LEASESTYMD" _
            & "    , LEASEENDYMD" _
            & "    , USERCODE" _
            & "    , CURRENTSTATIONCODE" _
            & "    , EXTRADINARYSTATIONCODE" _
            & "    , USERLIMIT" _
            & "    , LIMITTEXTRADIARYSTATION" _
            & "    , DEDICATETYPECODE" _
            & "    , EXTRADINARYTYPECODE" _
            & "    , EXTRADINARYLIMIT" _
            & "    , OPERATIONBASECODE" _
            & "    , COLORCODE" _
            & "    , ENEOS" _
            & "    , ECO" _
            & "    , ALLINSPECTIONDATE" _
            & "    , TRANSFERDATE" _
            & "    , OBTAINEDCODE" _
            & "    , MODEL" _
            & "    , MODELKANA" _
            & "    , LOAD" _
            & "    , LOADUNIT" _
            & "    , VOLUME" _
            & "    , VOLUMEUNIT" _
            & "    , ORIGINOWNERNAME" _
            & "    , OWNERNAME" _
            & "    , LEASENAME" _
            & "    , LEASECLASSNEMAE" _
            & "    , USERNAME" _
            & "    , CURRENTSTATIONNAME" _
            & "    , EXTRADINARYSTATIONNAME" _
            & "    , DEDICATETYPENAME" _
            & "    , EXTRADINARYTYPENAME" _
            & "    , OPERATIONBASENAME" _
            & "    , COLORNAME" _
            & "    , RESERVE1" _
            & "    , RESERVE2" _
            & "    , SPECIFIEDDATE" _
            & "    , JRALLINSPECTIONDATE" _
            & "    , PROGRESSYEAR" _
            & "    , NEXTPROGRESSYEAR" _
            & "    , JRINSPECTIONDATE" _
            & "    , INSPECTIONDATE" _
            & "    , JRSPECIFIEDDATE" _
            & "    , JRTANKNUMBER" _
            & "    , OLDTANKNUMBER" _
            & "    , OTTANKNUMBER" _
            & "    , JXTGTANKNUMBER" _
            & "    , COSMOTANKNUMBER" _
            & "    , FUJITANKNUMBER" _
            & "    , SHELLTANKNUMBER" _
            & "    , RESERVE3" _
            & "    , INITYMD" _
            & "    , INITUSER" _
            & "    , INITTERMID" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & "    , CAST(UPDTIMSTP As bigint) As UPDTIMSTP" _
            & " FROM" _
            & "    OIL.OIM0005_TANK" _
            & " WHERE" _
            & "        TANKNUMBER = @P01"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA00 As SqlParameter = SQLcmd.Parameters.Add("@P00", SqlDbType.NVarChar, 1)            '削除フラグ
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 8)            'JOT車番
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 20)            '原籍所有者C
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 20)            '名義所有者C
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 20)            'リース先C
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 20)            'リース区分C
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 20)            '自動延長
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.Date)            'リース開始年月日
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.Date)            'リース満了年月日
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", SqlDbType.NVarChar, 20)            '第三者使用者C
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 20)            '原常備駅C
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 20)            '臨時常備駅C
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.Date)            '第三者使用期限
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.Date)            '臨時常備駅期限
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 20)            '原専用種別C
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 20)            '臨時専用種別C
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.Date)            '臨時専用期限
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.NVarChar, 20)            '運用基地C
                Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.NVarChar, 20)            '塗色C
                Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.NVarChar, 20)            'エネオス
                Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.NVarChar, 20)            'エコレール
                Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.Date)            '取得年月日
                Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", SqlDbType.Date)            '車籍編入年月日
                Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", SqlDbType.NVarChar, 20)            '取得先C
                Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", SqlDbType.NVarChar, 20)            '形式
                Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", SqlDbType.NVarChar, 10)            '形式カナ
                Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", SqlDbType.Float, 4, 1)            '荷重
                Dim PARA27 As SqlParameter = SQLcmd.Parameters.Add("@P27", SqlDbType.NVarChar, 2)            '荷重単位
                Dim PARA28 As SqlParameter = SQLcmd.Parameters.Add("@P28", SqlDbType.Float, 4, 1)            '容積
                Dim PARA29 As SqlParameter = SQLcmd.Parameters.Add("@P29", SqlDbType.NVarChar, 2)            '容積単位
                Dim PARA30 As SqlParameter = SQLcmd.Parameters.Add("@P30", SqlDbType.NVarChar, 20)            '原籍所有者
                Dim PARA31 As SqlParameter = SQLcmd.Parameters.Add("@P31", SqlDbType.NVarChar, 20)            '名義所有者
                Dim PARA32 As SqlParameter = SQLcmd.Parameters.Add("@P32", SqlDbType.NVarChar, 20)            'リース先
                Dim PARA33 As SqlParameter = SQLcmd.Parameters.Add("@P33", SqlDbType.NVarChar, 20)            'リース区分
                Dim PARA34 As SqlParameter = SQLcmd.Parameters.Add("@P34", SqlDbType.NVarChar, 20)            '第三者使用者
                Dim PARA35 As SqlParameter = SQLcmd.Parameters.Add("@P35", SqlDbType.NVarChar, 20)            '原常備駅
                Dim PARA36 As SqlParameter = SQLcmd.Parameters.Add("@P36", SqlDbType.NVarChar, 20)            '臨時常備駅
                Dim PARA37 As SqlParameter = SQLcmd.Parameters.Add("@P37", SqlDbType.NVarChar, 20)            '原専用種別
                Dim PARA38 As SqlParameter = SQLcmd.Parameters.Add("@P38", SqlDbType.NVarChar, 20)            '臨時専用種別
                Dim PARA39 As SqlParameter = SQLcmd.Parameters.Add("@P39", SqlDbType.NVarChar, 20)            '運用場所
                Dim PARA40 As SqlParameter = SQLcmd.Parameters.Add("@P40", SqlDbType.NVarChar, 20)            '塗色
                Dim PARA41 As SqlParameter = SQLcmd.Parameters.Add("@P41", SqlDbType.NVarChar, 20)            '予備1
                Dim PARA42 As SqlParameter = SQLcmd.Parameters.Add("@P42", SqlDbType.NVarChar, 20)            '予備2
                Dim PARA43 As SqlParameter = SQLcmd.Parameters.Add("@P43", SqlDbType.Date)            '次回指定年月日
                Dim PARA44 As SqlParameter = SQLcmd.Parameters.Add("@P44", SqlDbType.Date)            '次回全検年月日(JR) 
                Dim PARA45 As SqlParameter = SQLcmd.Parameters.Add("@P45", SqlDbType.Int)            '現在経年
                Dim PARA46 As SqlParameter = SQLcmd.Parameters.Add("@P46", SqlDbType.Int)            '次回全検時経年
                Dim PARA47 As SqlParameter = SQLcmd.Parameters.Add("@P47", SqlDbType.Date)            '次回交検年月日(JR）
                Dim PARA48 As SqlParameter = SQLcmd.Parameters.Add("@P48", SqlDbType.Date)            '次回交検年月日
                Dim PARA49 As SqlParameter = SQLcmd.Parameters.Add("@P49", SqlDbType.Date)            '次回指定年月日(JR)
                Dim PARA50 As SqlParameter = SQLcmd.Parameters.Add("@P50", SqlDbType.NVarChar, 20)            'JR車番
                Dim PARA51 As SqlParameter = SQLcmd.Parameters.Add("@P51", SqlDbType.NVarChar, 20)            '旧JOT車番
                Dim PARA52 As SqlParameter = SQLcmd.Parameters.Add("@P52", SqlDbType.NVarChar, 20)            'OT車番
                Dim PARA53 As SqlParameter = SQLcmd.Parameters.Add("@P53", SqlDbType.NVarChar, 20)            'JXTG車番
                Dim PARA54 As SqlParameter = SQLcmd.Parameters.Add("@P54", SqlDbType.NVarChar, 20)            'コスモ車番
                Dim PARA55 As SqlParameter = SQLcmd.Parameters.Add("@P55", SqlDbType.NVarChar, 20)            '富士石油車番
                Dim PARA56 As SqlParameter = SQLcmd.Parameters.Add("@P56", SqlDbType.NVarChar, 20)            '出光昭シ車番
                Dim PARA57 As SqlParameter = SQLcmd.Parameters.Add("@P57", SqlDbType.NVarChar, 20)            '予備
                Dim PARA58 As SqlParameter = SQLcmd.Parameters.Add("@P58", SqlDbType.DateTime)            '登録年月日
                Dim PARA59 As SqlParameter = SQLcmd.Parameters.Add("@P59", SqlDbType.NVarChar, 20)            '登録ユーザーＩＤ
                Dim PARA60 As SqlParameter = SQLcmd.Parameters.Add("@P60", SqlDbType.NVarChar, 20)            '登録端末
                Dim PARA61 As SqlParameter = SQLcmd.Parameters.Add("@P61", SqlDbType.DateTime)            '更新年月日
                Dim PARA62 As SqlParameter = SQLcmd.Parameters.Add("@P62", SqlDbType.NVarChar, 20)            '更新ユーザーＩＤ
                Dim PARA63 As SqlParameter = SQLcmd.Parameters.Add("@P63", SqlDbType.NVarChar, 20)            '更新端末
                Dim PARA64 As SqlParameter = SQLcmd.Parameters.Add("@P64", SqlDbType.DateTime)            '集信日時

                Dim JPARA00 As SqlParameter = SQLcmdJnl.Parameters.Add("@P00", SqlDbType.NVarChar, 1)            '削除フラグ
                Dim JPARA01 As SqlParameter = SQLcmdJnl.Parameters.Add("@P01", SqlDbType.NVarChar, 8)            'JOT車番
                Dim JPARA02 As SqlParameter = SQLcmdJnl.Parameters.Add("@P02", SqlDbType.NVarChar, 20)            '原籍所有者C
                Dim JPARA03 As SqlParameter = SQLcmdJnl.Parameters.Add("@P03", SqlDbType.NVarChar, 20)            '名義所有者C
                Dim JPARA04 As SqlParameter = SQLcmdJnl.Parameters.Add("@P04", SqlDbType.NVarChar, 20)            'リース先C
                Dim JPARA05 As SqlParameter = SQLcmdJnl.Parameters.Add("@P05", SqlDbType.NVarChar, 20)            'リース区分C
                Dim JPARA06 As SqlParameter = SQLcmdJnl.Parameters.Add("@P06", SqlDbType.NVarChar, 20)            '自動延長
                Dim JPARA07 As SqlParameter = SQLcmdJnl.Parameters.Add("@P07", SqlDbType.Date)            'リース開始年月日
                Dim JPARA08 As SqlParameter = SQLcmdJnl.Parameters.Add("@P08", SqlDbType.Date)            'リース満了年月日
                Dim JPARA09 As SqlParameter = SQLcmdJnl.Parameters.Add("@P09", SqlDbType.NVarChar, 20)            '第三者使用者C
                Dim JPARA10 As SqlParameter = SQLcmdJnl.Parameters.Add("@P10", SqlDbType.NVarChar, 20)            '原常備駅C
                Dim JPARA11 As SqlParameter = SQLcmdJnl.Parameters.Add("@P11", SqlDbType.NVarChar, 20)            '臨時常備駅C
                Dim JPARA12 As SqlParameter = SQLcmdJnl.Parameters.Add("@P12", SqlDbType.Date)            '第三者使用期限
                Dim JPARA13 As SqlParameter = SQLcmdJnl.Parameters.Add("@P13", SqlDbType.Date)            '臨時常備駅期限
                Dim JPARA14 As SqlParameter = SQLcmdJnl.Parameters.Add("@P14", SqlDbType.NVarChar, 20)            '原専用種別C
                Dim JPARA15 As SqlParameter = SQLcmdJnl.Parameters.Add("@P15", SqlDbType.NVarChar, 20)            '臨時専用種別C
                Dim JPARA16 As SqlParameter = SQLcmdJnl.Parameters.Add("@P16", SqlDbType.Date)            '臨時専用期限
                Dim JPARA17 As SqlParameter = SQLcmdJnl.Parameters.Add("@P17", SqlDbType.NVarChar, 20)            '運用基地C
                Dim JPARA18 As SqlParameter = SQLcmdJnl.Parameters.Add("@P18", SqlDbType.NVarChar, 20)            '塗色C
                Dim JPARA19 As SqlParameter = SQLcmdJnl.Parameters.Add("@P19", SqlDbType.NVarChar, 20)            'エネオス
                Dim JPARA20 As SqlParameter = SQLcmdJnl.Parameters.Add("@P20", SqlDbType.NVarChar, 20)            'エコレール
                Dim JPARA21 As SqlParameter = SQLcmdJnl.Parameters.Add("@P21", SqlDbType.Date)            '取得年月日
                Dim JPARA22 As SqlParameter = SQLcmdJnl.Parameters.Add("@P22", SqlDbType.Date)            '車籍編入年月日
                Dim JPARA23 As SqlParameter = SQLcmdJnl.Parameters.Add("@P23", SqlDbType.NVarChar, 20)            '取得先C
                'Dim JPARA24 As SqlParameter = SQLcmdJnl.Parameters.Add("@P24", SqlDbType.NVarChar, 20)            '形式
                'Dim JPARA25 As SqlParameter = SQLcmdJnl.Parameters.Add("@P25", SqlDbType.NVarChar, 10)            '形式カナ
                'Dim JPARA26 As SqlParameter = SQLcmdJnl.Parameters.Add("@P26", SqlDbType.Float, 4, 1)            '荷重
                'Dim JPARA27 As SqlParameter = SQLcmdJnl.Parameters.Add("@P27", SqlDbType.NVarChar, 2)            '荷重単位
                'Dim JPARA28 As SqlParameter = SQLcmdJnl.Parameters.Add("@P28", SqlDbType.Float, 4, 1)            '容積
                'Dim JPARA29 As SqlParameter = SQLcmdJnl.Parameters.Add("@P29", SqlDbType.NVarChar, 2)            '容積単位
                'Dim JPARA30 As SqlParameter = SQLcmdJnl.Parameters.Add("@P30", SqlDbType.NVarChar, 20)            '原籍所有者
                'Dim JPARA31 As SqlParameter = SQLcmdJnl.Parameters.Add("@P31", SqlDbType.NVarChar, 20)            '名義所有者
                'Dim JPARA32 As SqlParameter = SQLcmdJnl.Parameters.Add("@P32", SqlDbType.NVarChar, 20)            'リース先
                'Dim JPARA33 As SqlParameter = SQLcmdJnl.Parameters.Add("@P33", SqlDbType.NVarChar, 20)            'リース区分
                'Dim JPARA34 As SqlParameter = SQLcmdJnl.Parameters.Add("@P34", SqlDbType.NVarChar, 20)            '第三者使用者
                'Dim JPARA35 As SqlParameter = SQLcmdJnl.Parameters.Add("@P35", SqlDbType.NVarChar, 20)            '原常備駅
                'Dim JPARA36 As SqlParameter = SQLcmdJnl.Parameters.Add("@P36", SqlDbType.NVarChar, 20)            '臨時常備駅
                'Dim JPARA37 As SqlParameter = SQLcmdJnl.Parameters.Add("@P37", SqlDbType.NVarChar, 20)            '原専用種別
                'Dim JPARA38 As SqlParameter = SQLcmdJnl.Parameters.Add("@P38", SqlDbType.NVarChar, 20)            '臨時専用種別
                'Dim JPARA39 As SqlParameter = SQLcmdJnl.Parameters.Add("@P39", SqlDbType.NVarChar, 20)            '運用場所
                'Dim JPARA40 As SqlParameter = SQLcmdJnl.Parameters.Add("@P40", SqlDbType.NVarChar, 20)            '塗色
                'Dim JPARA41 As SqlParameter = SQLcmdJnl.Parameters.Add("@P41", SqlDbType.NVarChar, 20)            '予備1
                'Dim JPARA42 As SqlParameter = SQLcmdJnl.Parameters.Add("@P42", SqlDbType.NVarChar, 20)            '予備2
                'Dim JPARA43 As SqlParameter = SQLcmdJnl.Parameters.Add("@P43", SqlDbType.Date)            '次回指定年月日
                'Dim JPARA44 As SqlParameter = SQLcmdJnl.Parameters.Add("@P44", SqlDbType.Date)            '次回全検年月日(JR) 
                'Dim JPARA45 As SqlParameter = SQLcmdJnl.Parameters.Add("@P45", SqlDbType.Int)            '現在経年
                'Dim JPARA46 As SqlParameter = SQLcmdJnl.Parameters.Add("@P46", SqlDbType.Int)            '次回全検時経年
                'Dim JPARA47 As SqlParameter = SQLcmdJnl.Parameters.Add("@P47", SqlDbType.Date)            '次回交検年月日(JR）
                'Dim JPARA48 As SqlParameter = SQLcmdJnl.Parameters.Add("@P48", SqlDbType.Date)            '次回交検年月日
                'Dim JPARA49 As SqlParameter = SQLcmdJnl.Parameters.Add("@P49", SqlDbType.Date)            '次回指定年月日(JR)
                'Dim JPARA50 As SqlParameter = SQLcmdJnl.Parameters.Add("@P50", SqlDbType.NVarChar, 20)            'JR車番
                'Dim JPARA51 As SqlParameter = SQLcmdJnl.Parameters.Add("@P51", SqlDbType.NVarChar, 20)            '旧JOT車番
                'Dim JPARA52 As SqlParameter = SQLcmdJnl.Parameters.Add("@P52", SqlDbType.NVarChar, 20)            'OT車番
                'Dim JPARA53 As SqlParameter = SQLcmdJnl.Parameters.Add("@P53", SqlDbType.NVarChar, 20)            'JXTG車番
                'Dim JPARA54 As SqlParameter = SQLcmdJnl.Parameters.Add("@P54", SqlDbType.NVarChar, 20)            'コスモ車番
                'Dim JPARA55 As SqlParameter = SQLcmdJnl.Parameters.Add("@P55", SqlDbType.NVarChar, 20)            '富士石油車番
                'Dim JPARA56 As SqlParameter = SQLcmdJnl.Parameters.Add("@P56", SqlDbType.NVarChar, 20)            '出光昭シ車番
                'Dim JPARA57 As SqlParameter = SQLcmdJnl.Parameters.Add("@P57", SqlDbType.NVarChar, 20)            '予備
                'Dim JPARA58 As SqlParameter = SQLcmdJnl.Parameters.Add("@P58", SqlDbType.DateTime)            '登録年月日
                'Dim JPARA59 As SqlParameter = SQLcmdJnl.Parameters.Add("@P59", SqlDbType.NVarChar, 20)            '登録ユーザーＩＤ
                'Dim JPARA60 As SqlParameter = SQLcmdJnl.Parameters.Add("@P60", SqlDbType.NVarChar, 20)            '登録端末
                'Dim JPARA61 As SqlParameter = SQLcmdJnl.Parameters.Add("@P61", SqlDbType.DateTime)            '更新年月日
                'Dim JPARA62 As SqlParameter = SQLcmdJnl.Parameters.Add("@P62", SqlDbType.NVarChar, 20)            '更新ユーザーＩＤ
                'Dim JPARA63 As SqlParameter = SQLcmdJnl.Parameters.Add("@P63", SqlDbType.NVarChar, 20)            '更新端末
                'Dim JPARA64 As SqlParameter = SQLcmdJnl.Parameters.Add("@P64", SqlDbType.DateTime)            '集信日時

                For Each OIM0005row As DataRow In OIM0005tbl.Rows
                    If Trim(OIM0005row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING OrElse
                        Trim(OIM0005row("OPERATION")) = C_LIST_OPERATION_CODE.INSERTING OrElse
                        Trim(OIM0005row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED Then
                        '                        Trim(OIM0005row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING Then
                        Dim WW_DATENOW As DateTime = Date.Now

                        'DB更新
                        PARA00.Value = OIM0005row("DELFLG")
                        PARA01.Value = OIM0005row("TANKNUMBER")
                        PARA02.Value = OIM0005row("ORIGINOWNERCODE")
                        PARA03.Value = OIM0005row("OWNERCODE")
                        PARA04.Value = OIM0005row("LEASECODE")
                        PARA05.Value = OIM0005row("LEASECLASS")
                        PARA06.Value = OIM0005row("AUTOEXTENTION")
                        PARA07.Value = RTrim(OIM0005row("LEASESTYMD"))
                        PARA08.Value = RTrim(OIM0005row("LEASEENDYMD"))
                        PARA09.Value = OIM0005row("USERCODE")
                        PARA10.Value = OIM0005row("CURRENTSTATIONCODE")
                        PARA11.Value = OIM0005row("EXTRADINARYSTATIONCODE")
                        PARA12.Value = RTrim(OIM0005row("USERLIMIT"))
                        PARA13.Value = RTrim(OIM0005row("LIMITTEXTRADIARYSTATION"))
                        PARA14.Value = OIM0005row("DEDICATETYPECODE")
                        PARA15.Value = OIM0005row("EXTRADINARYTYPECODE")
                        PARA16.Value = RTrim(OIM0005row("EXTRADINARYLIMIT"))
                        PARA17.Value = OIM0005row("OPERATIONBASECODE")
                        PARA18.Value = OIM0005row("COLORCODE")
                        PARA19.Value = OIM0005row("ENEOS")
                        PARA20.Value = OIM0005row("ECO")
                        PARA21.Value = RTrim(OIM0005row("ALLINSPECTIONDATE"))
                        PARA22.Value = RTrim(OIM0005row("TRANSFERDATE"))
                        PARA23.Value = OIM0005row("OBTAINEDCODE")
                        PARA24.Value = OIM0005row("MODEL")
                        PARA25.Value = OIM0005row("MODELKANA")
                        PARA26.Value = OIM0005row("LOAD")
                        PARA27.Value = OIM0005row("LOADUNIT")
                        PARA28.Value = OIM0005row("VOLUME")
                        PARA29.Value = OIM0005row("VOLUMEUNIT")
                        PARA30.Value = OIM0005row("ORIGINOWNERNAME")
                        PARA31.Value = OIM0005row("OWNERNAME")
                        PARA32.Value = OIM0005row("LEASENAME")
                        PARA33.Value = OIM0005row("LEASECLASSNEMAE")
                        PARA34.Value = OIM0005row("USERNAME")
                        PARA35.Value = OIM0005row("CURRENTSTATIONNAME")
                        PARA36.Value = OIM0005row("EXTRADINARYSTATIONNAME")
                        PARA37.Value = OIM0005row("DEDICATETYPENAME")
                        PARA38.Value = OIM0005row("EXTRADINARYTYPENAME")
                        PARA39.Value = OIM0005row("OPERATIONBASENAME")
                        PARA40.Value = OIM0005row("COLORNAME")
                        PARA41.Value = OIM0005row("RESERVE1")
                        PARA42.Value = OIM0005row("RESERVE2")
                        If RTrim(OIM0005row("SPECIFIEDDATE")) <> "" Then
                            PARA43.Value = OIM0005row("SPECIFIEDDATE")
                        Else
                            PARA43.Value = C_DEFAULT_YMD
                        End If
                        If OIM0005row("JRALLINSPECTIONDATE") <> "" Then
                            PARA44.Value = OIM0005row("JRALLINSPECTIONDATE")
                        Else
                            PARA44.Value = C_DEFAULT_YMD
                        End If
                        If OIM0005row("PROGRESSYEAR") <> "" Then
                            PARA45.Value = OIM0005row("PROGRESSYEAR")
                        Else
                            PARA45.Value = "0"
                        End If
                        If OIM0005row("NEXTPROGRESSYEAR") <> "" Then
                            PARA46.Value = OIM0005row("NEXTPROGRESSYEAR")
                        Else
                            PARA46.Value = "0"
                        End If
                        If OIM0005row("JRINSPECTIONDATE") <> "" Then
                            PARA47.Value = OIM0005row("JRINSPECTIONDATE")
                        Else
                            PARA47.Value = C_DEFAULT_YMD
                        End If
                        If OIM0005row("INSPECTIONDATE") <> "" Then
                            PARA48.Value = OIM0005row("INSPECTIONDATE")
                        Else
                            PARA48.Value = C_DEFAULT_YMD
                        End If
                        If OIM0005row("JRSPECIFIEDDATE") <> "" Then
                            PARA49.Value = OIM0005row("JRSPECIFIEDDATE")
                        Else
                            PARA49.Value = C_DEFAULT_YMD
                        End If
                        PARA50.Value = OIM0005row("JRTANKNUMBER")
                        PARA51.Value = OIM0005row("OLDTANKNUMBER")
                        PARA52.Value = OIM0005row("OTTANKNUMBER")
                        PARA53.Value = OIM0005row("JXTGTANKNUMBER")
                        PARA54.Value = OIM0005row("COSMOTANKNUMBER")
                        PARA55.Value = OIM0005row("FUJITANKNUMBER")
                        PARA56.Value = OIM0005row("SHELLTANKNUMBER")
                        PARA57.Value = OIM0005row("RESERVE3")
                        PARA58.Value = WW_DATENOW
                        PARA59.Value = Master.USERID
                        PARA60.Value = Master.USERTERMID
                        PARA61.Value = WW_DATENOW
                        PARA62.Value = Master.USERID
                        PARA63.Value = Master.USERTERMID
                        PARA64.Value = C_DEFAULT_YMD
                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                        OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                        '更新ジャーナル出力
                        JPARA00.Value = OIM0005row("DELFLG")
                        JPARA01.Value = OIM0005row("TANKNUMBER")
                        JPARA02.Value = OIM0005row("ORIGINOWNERCODE")
                        JPARA03.Value = OIM0005row("OWNERCODE")
                        JPARA04.Value = OIM0005row("LEASECODE")
                        JPARA05.Value = OIM0005row("LEASECLASS")
                        JPARA06.Value = OIM0005row("AUTOEXTENTION")
                        JPARA07.Value = RTrim(OIM0005row("LEASESTYMD"))
                        JPARA08.Value = RTrim(OIM0005row("LEASEENDYMD"))
                        JPARA09.Value = OIM0005row("USERCODE")
                        JPARA10.Value = OIM0005row("CURRENTSTATIONCODE")
                        JPARA11.Value = OIM0005row("EXTRADINARYSTATIONCODE")
                        JPARA12.Value = RTrim(OIM0005row("USERLIMIT"))
                        JPARA13.Value = RTrim(OIM0005row("LIMITTEXTRADIARYSTATION"))
                        JPARA14.Value = OIM0005row("DEDICATETYPECODE")
                        JPARA15.Value = OIM0005row("EXTRADINARYTYPECODE")
                        JPARA16.Value = RTrim(OIM0005row("EXTRADINARYLIMIT"))
                        JPARA17.Value = OIM0005row("OPERATIONBASECODE")
                        JPARA18.Value = OIM0005row("COLORCODE")
                        JPARA19.Value = OIM0005row("ENEOS")
                        JPARA20.Value = OIM0005row("ECO")
                        JPARA21.Value = RTrim(OIM0005row("ALLINSPECTIONDATE"))
                        JPARA22.Value = RTrim(OIM0005row("TRANSFERDATE"))
                        JPARA23.Value = OIM0005row("OBTAINEDCODE")
                        'JPARA24.Value = OIM0005row("MODEL")
                        'JPARA25.Value = OIM0005row("MODELKANA")
                        'JPARA26.Value = OIM0005row("LOAD")
                        'JPARA27.Value = OIM0005row("LOADUNIT")
                        'JPARA28.Value = OIM0005row("VOLUME")
                        'JPARA29.Value = OIM0005row("VOLUMEUNIT")
                        'JPARA30.Value = OIM0005row("ORIGINOWNERNAME")
                        'JPARA31.Value = OIM0005row("OWNERNAME")
                        'JPARA32.Value = OIM0005row("LEASENAME")
                        'JPARA33.Value = OIM0005row("LEASECLASSNEMAE")
                        'JPARA34.Value = OIM0005row("USERNAME")
                        'JPARA35.Value = OIM0005row("CURRENTSTATIONNAME")
                        'JPARA36.Value = OIM0005row("EXTRADINARYSTATIONNAME")
                        'JPARA37.Value = OIM0005row("DEDICATETYPENAME")
                        'JPARA38.Value = OIM0005row("EXTRADINARYTYPENAME")
                        'JPARA39.Value = OIM0005row("OPERATIONBASENAME")
                        'JPARA40.Value = OIM0005row("COLORNAME")
                        'JPARA41.Value = OIM0005row("RESERVE1")
                        'JPARA42.Value = OIM0005row("RESERVE2")
                        'If RTrim(OIM0005row("SPECIFIEDDATE")) <> "" Then
                        '    JPARA43.Value = OIM0005row("SPECIFIEDDATE")
                        'Else
                        '    JPARA43.Value = C_DEFAULT_YMD
                        'End If
                        'If OIM0005row("JRALLINSPECTIONDATE") <> "" Then
                        '    JPARA44.Value = OIM0005row("JRALLINSPECTIONDATE")
                        'Else
                        '    JPARA44.Value = C_DEFAULT_YMD
                        'End If
                        'If OIM0005row("PROGRESSYEAR") <> "" Then
                        '    JPARA45.Value = OIM0005row("PROGRESSYEAR")
                        'Else
                        '    JPARA45.Value = "0"
                        'End If
                        'If OIM0005row("NEXTPROGRESSYEAR") <> "" Then
                        '    JPARA46.Value = OIM0005row("NEXTPROGRESSYEAR")
                        'Else
                        '    JPARA46.Value = "0"
                        'End If
                        'If OIM0005row("JRINSPECTIONDATE") <> "" Then
                        '    JPARA47.Value = OIM0005row("JRINSPECTIONDATE")
                        'Else
                        '    JPARA47.Value = C_DEFAULT_YMD
                        'End If
                        'If OIM0005row("INSPECTIONDATE") <> "" Then
                        '    JPARA48.Value = OIM0005row("INSPECTIONDATE")
                        'Else
                        '    JPARA48.Value = C_DEFAULT_YMD
                        'End If
                        'If OIM0005row("JRSPECIFIEDDATE") <> "" Then
                        '    JPARA49.Value = OIM0005row("JRSPECIFIEDDATE")
                        'Else
                        '    JPARA49.Value = C_DEFAULT_YMD
                        'End If
                        'JPARA50.Value = OIM0005row("JRTANKNUMBER")
                        'JPARA51.Value = OIM0005row("OLDTANKNUMBER")
                        'JPARA52.Value = OIM0005row("OTTANKNUMBER")
                        'JPARA53.Value = OIM0005row("JXTGTANKNUMBER")
                        'JPARA54.Value = OIM0005row("COSMOTANKNUMBER")
                        'JPARA55.Value = OIM0005row("FUJITANKNUMBER")
                        'JPARA56.Value = OIM0005row("SHELLTANKNUMBER")
                        'JPARA57.Value = OIM0005row("RESERVE3")
                        'JPARA58.Value = WW_DATENOW
                        'JPARA59.Value = Master.USERID
                        'JPARA60.Value = Master.USERTERMID
                        'JPARA61.Value = WW_DATENOW
                        'JPARA62.Value = Master.USERID
                        'JPARA63.Value = Master.USERTERMID
                        'JPARA64.Value = C_DEFAULT_YMD

                        Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                            If IsNothing(OIM0005UPDtbl) Then
                                OIM0005UPDtbl = New DataTable

                                For index As Integer = 0 To SQLdr.FieldCount - 1
                                    OIM0005UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                Next
                            End If

                            OIM0005UPDtbl.Clear()
                            OIM0005UPDtbl.Load(SQLdr)
                        End Using

                        For Each OIM0005UPDrow As DataRow In OIM0005UPDtbl.Rows
                            CS0020JOURNAL.TABLENM = "OIM0005L"
                            CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                            CS0020JOURNAL.ROW = OIM0005UPDrow
                            CS0020JOURNAL.CS0020JOURNAL()
                            If Not isNormal(CS0020JOURNAL.ERR) Then
                                Master.Output(CS0020JOURNAL.ERR, C_MESSAGE_TYPE.ABORT, "CS0020JOURNAL JOURNAL")

                                CS0011LOGWrite.INFSUBCLASS = "MAIN"                     'SUBクラス名
                                CS0011LOGWrite.INFPOSI = "CS0020JOURNAL JOURNAL"
                                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                                CS0011LOGWrite.TEXT = "CS0020JOURNAL Call Err!"
                                CS0011LOGWrite.MESSAGENO = CS0020JOURNAL.ERR
                                CS0011LOGWrite.CS0011LOGWrite()                         'ログ出力
                                Exit Sub
                            End If
                        Next
                    End If
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0005L UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0005L UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub


    ''' <summary>
    ''' ﾀﾞｳﾝﾛｰﾄﾞ(Excel出力)ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDownload_Click()

        '○ 帳票出力
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                       '画面ID
        CS0030REPORT.REPORTID = rightview.GetReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "XLSX"                           '出力ファイル形式
        CS0030REPORT.TBLDATA = OIM0005tbl                        'データ参照  Table
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            If CS0030REPORT.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR)
            Else
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORT")
            End If
            Exit Sub
        End If

        '○ 別画面でExcelを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

    End Sub

    ''' <summary>
    ''' ﾀﾞｳﾝﾛｰﾄﾞ(PDF出力)・一覧印刷ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonPrint_Click()

        '○ 帳票出力
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                       '画面ID
        CS0030REPORT.REPORTID = rightview.GetReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "pdf"                            '出力ファイル形式
        CS0030REPORT.TBLDATA = OIM0005tbl                        'データ参照Table
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            If CS0030REPORT.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR)
            Else
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORT")
            End If
            Exit Sub
        End If

        '○ 別画面でPDFを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_PDFPrint();", True)

    End Sub


    ''' <summary>
    ''' 終了ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        Master.TransitionPrevPage()

    End Sub


    ''' <summary>
    ''' 先頭頁ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonFIRST_Click()

        '○ 先頭頁に移動
        WF_GridPosition.Text = "1"

    End Sub

    ''' <summary>
    ''' 最終頁ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonLAST_Click()

        '○ ソート
        Dim TBLview As New DataView(OIM0005tbl)
        TBLview.RowFilter = "HIDDEN = 0"

        '○ 最終頁に移動
        If TBLview.Count Mod 10 = 0 Then
            WF_GridPosition.Text = TBLview.Count - (TBLview.Count Mod 10)
        Else
            WF_GridPosition.Text = TBLview.Count - (TBLview.Count Mod 10) + 1
        End If

        TBLview.Dispose()
        TBLview = Nothing

    End Sub


    ' ******************************************************************************
    ' ***  一覧表示(GridView)関連操作                                            ***
    ' ******************************************************************************

    ''' <summary>
    ''' 一覧画面-明細行ダブルクリック時処理 (GridView ---> detailbox)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_DBClick()

        Dim WW_LINECNT As Integer = 0
        Dim WW_FIELD_OBJ As Object = Nothing
        Dim WW_VALUE As String = ""
        Dim WW_TEXT As String = ""

        '○ LINECNT取得
        Try
            Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT)
            WW_LINECNT -= 1
        Catch ex As Exception
            Exit Sub
        End Try

        '選択行
        WF_Sel_LINECNT.Text = OIM0005tbl.Rows(WW_LINECNT)("LINECNT")
        work.WF_SEL_LINECNT.Text = OIM0005tbl.Rows(WW_LINECNT)("LINECNT")

        'JOT車番
        WF_TANKNUMBER.Text = OIM0005tbl.Rows(WW_LINECNT)("TANKNUMBER")
        work.WF_SEL_TANKNUMBER2.Text = OIM0005tbl.Rows(WW_LINECNT)("TANKNUMBER")

        '形式
        WF_MODEL.Text = OIM0005tbl.Rows(WW_LINECNT)("MODEL")
        work.WF_SEL_MODEL.Text = OIM0005tbl.Rows(WW_LINECNT)("MODEL")

        '原籍所有者C
        WF_ORIGINOWNERCODE.Text = OIM0005tbl.Rows(WW_LINECNT)("ORIGINOWNERCODE")
        work.WF_SEL_ORIGINOWNERCODE.Text = OIM0005tbl.Rows(WW_LINECNT)("ORIGINOWNERCODE")

        '名義所有者C
        WF_OWNERCODE.Text = OIM0005tbl.Rows(WW_LINECNT)("OWNERCODE")
        work.WF_SEL_OWNERCODE.Text = OIM0005tbl.Rows(WW_LINECNT)("OWNERCODE")

        'リース先C
        WF_LEASECODE.Text = OIM0005tbl.Rows(WW_LINECNT)("LEASECODE")
        work.WF_SEL_LEASECODE.Text = OIM0005tbl.Rows(WW_LINECNT)("LEASECODE")

        'リース区分C
        WF_LEASECLASS.Text = OIM0005tbl.Rows(WW_LINECNT)("LEASECLASS")
        work.WF_SEL_LEASECLASS.Text = OIM0005tbl.Rows(WW_LINECNT)("LEASECLASS")

        '自動延長
        WF_AUTOEXTENTION.Text = OIM0005tbl.Rows(WW_LINECNT)("AUTOEXTENTION")
        work.WF_SEL_AUTOEXTENTION.Text = OIM0005tbl.Rows(WW_LINECNT)("AUTOEXTENTION")

        'リース開始年月日
        WF_LEASESTYMD.Text = OIM0005tbl.Rows(WW_LINECNT)("LEASESTYMD")
        work.WF_SEL_LEASESTYMD.Text = OIM0005tbl.Rows(WW_LINECNT)("LEASESTYMD")

        'リース満了年月日
        WF_LEASEENDYMD.Text = OIM0005tbl.Rows(WW_LINECNT)("LEASEENDYMD")
        work.WF_SEL_LEASEENDYMD.Text = OIM0005tbl.Rows(WW_LINECNT)("LEASEENDYMD")

        '第三者使用者C
        WF_USERCODE.Text = OIM0005tbl.Rows(WW_LINECNT)("USERCODE")
        work.WF_SEL_USERCODE.Text = OIM0005tbl.Rows(WW_LINECNT)("USERCODE")

        '原常備駅C
        WF_CURRENTSTATIONCODE.Text = OIM0005tbl.Rows(WW_LINECNT)("CURRENTSTATIONCODE")
        work.WF_SEL_CURRENTSTATIONCODE.Text = OIM0005tbl.Rows(WW_LINECNT)("CURRENTSTATIONCODE")

        '臨時常備駅C
        WF_EXTRADINARYSTATIONCODE.Text = OIM0005tbl.Rows(WW_LINECNT)("EXTRADINARYSTATIONCODE")
        work.WF_SEL_EXTRADINARYSTATIONCODE.Text = OIM0005tbl.Rows(WW_LINECNT)("EXTRADINARYSTATIONCODE")

        '第三者使用期限
        WF_USERLIMIT.Text = OIM0005tbl.Rows(WW_LINECNT)("USERLIMIT")
        work.WF_SEL_USERLIMIT.Text = OIM0005tbl.Rows(WW_LINECNT)("USERLIMIT")

        '臨時常備駅期限
        WF_LIMITTEXTRADIARYSTATION.Text = OIM0005tbl.Rows(WW_LINECNT)("LIMITTEXTRADIARYSTATION")
        work.WF_SEL_LIMITTEXTRADIARYSTATION.Text = OIM0005tbl.Rows(WW_LINECNT)("LIMITTEXTRADIARYSTATION")

        '原専用種別C
        WF_DEDICATETYPECODE.Text = OIM0005tbl.Rows(WW_LINECNT)("DEDICATETYPECODE")
        work.WF_SEL_DEDICATETYPECODE.Text = OIM0005tbl.Rows(WW_LINECNT)("DEDICATETYPECODE")

        '臨時専用種別C
        WF_EXTRADINARYTYPECODE.Text = OIM0005tbl.Rows(WW_LINECNT)("EXTRADINARYTYPECODE")
        work.WF_SEL_EXTRADINARYTYPECODE.Text = OIM0005tbl.Rows(WW_LINECNT)("EXTRADINARYTYPECODE")

        '臨時専用期限
        WF_EXTRADINARYLIMIT.Text = OIM0005tbl.Rows(WW_LINECNT)("EXTRADINARYLIMIT")
        work.WF_SEL_EXTRADINARYLIMIT.Text = OIM0005tbl.Rows(WW_LINECNT)("EXTRADINARYLIMIT")

        '運用基地C
        WF_OPERATIONBASECODE.Text = OIM0005tbl.Rows(WW_LINECNT)("OPERATIONBASECODE")
        work.WF_SEL_OPERATIONBASECODE.Text = OIM0005tbl.Rows(WW_LINECNT)("OPERATIONBASECODE")

        '塗色C
        WF_COLORCODE.Text = OIM0005tbl.Rows(WW_LINECNT)("COLORCODE")
        work.WF_SEL_COLORCODE.Text = OIM0005tbl.Rows(WW_LINECNT)("COLORCODE")

        'エネオス
        WF_ENEOS.Text = OIM0005tbl.Rows(WW_LINECNT)("ENEOS")
        work.WF_SEL_ENEOS.Text = OIM0005tbl.Rows(WW_LINECNT)("ENEOS")

        'エコレール
        WF_ECO.Text = OIM0005tbl.Rows(WW_LINECNT)("ECO")
        work.WF_SEL_ECO.Text = OIM0005tbl.Rows(WW_LINECNT)("ECO")

        '取得年月日
        WF_ALLINSPECTIONDATE.Text = OIM0005tbl.Rows(WW_LINECNT)("ALLINSPECTIONDATE")
        work.WF_SEL_ALLINSPECTIONDATE.Text = OIM0005tbl.Rows(WW_LINECNT)("ALLINSPECTIONDATE")

        '車籍編入年月日
        WF_TRANSFERDATE.Text = OIM0005tbl.Rows(WW_LINECNT)("TRANSFERDATE")
        work.WF_SEL_TRANSFERDATE.Text = OIM0005tbl.Rows(WW_LINECNT)("TRANSFERDATE")

        '取得先C
        WF_OBTAINEDCODE.Text = OIM0005tbl.Rows(WW_LINECNT)("OBTAINEDCODE")
        work.WF_SEL_OBTAINEDCODE.Text = OIM0005tbl.Rows(WW_LINECNT)("OBTAINEDCODE")

        '形式
        WF_MODEL.Text = OIM0005tbl.Rows(WW_LINECNT)("MODEL")
        work.WF_SEL_MODEL.Text = OIM0005tbl.Rows(WW_LINECNT)("MODEL")

        '形式カナ
        WF_MODELKANA.Text = OIM0005tbl.Rows(WW_LINECNT)("MODELKANA")
        work.WF_SEL_MODELKANA.Text = OIM0005tbl.Rows(WW_LINECNT)("MODELKANA")

        '荷重
        WF_LOAD.Text = OIM0005tbl.Rows(WW_LINECNT)("LOAD")
        work.WF_SEL_LOAD.Text = OIM0005tbl.Rows(WW_LINECNT)("LOAD")

        '荷重単位
        WF_LOADUNIT.Text = OIM0005tbl.Rows(WW_LINECNT)("LOADUNIT")
        work.WF_SEL_LOADUNIT.Text = OIM0005tbl.Rows(WW_LINECNT)("LOADUNIT")

        '容積
        WF_VOLUME.Text = OIM0005tbl.Rows(WW_LINECNT)("VOLUME")
        work.WF_SEL_VOLUME.Text = OIM0005tbl.Rows(WW_LINECNT)("VOLUME")

        '容積単位
        WF_VOLUMEUNIT.Text = OIM0005tbl.Rows(WW_LINECNT)("VOLUMEUNIT")
        work.WF_SEL_VOLUMEUNIT.Text = OIM0005tbl.Rows(WW_LINECNT)("VOLUMEUNIT")

        '原籍所有者
        WF_ORIGINOWNERNAME.Text = OIM0005tbl.Rows(WW_LINECNT)("ORIGINOWNERNAME")
        work.WF_SEL_ORIGINOWNERNAME.Text = OIM0005tbl.Rows(WW_LINECNT)("ORIGINOWNERNAME")

        '名義所有者
        WF_OWNERNAME.Text = OIM0005tbl.Rows(WW_LINECNT)("OWNERNAME")
        work.WF_SEL_OWNERNAME.Text = OIM0005tbl.Rows(WW_LINECNT)("OWNERNAME")

        'リース先
        WF_LEASENAME.Text = OIM0005tbl.Rows(WW_LINECNT)("LEASENAME")
        work.WF_SEL_LEASENAME.Text = OIM0005tbl.Rows(WW_LINECNT)("LEASENAME")

        'リース区分
        WF_LEASECLASSNEMAE.Text = OIM0005tbl.Rows(WW_LINECNT)("LEASECLASSNEMAE")
        work.WF_SEL_LEASECLASSNEMAE.Text = OIM0005tbl.Rows(WW_LINECNT)("LEASECLASSNEMAE")

        '第三者使用者
        WF_USERNAME.Text = OIM0005tbl.Rows(WW_LINECNT)("USERNAME")
        work.WF_SEL_USERNAME.Text = OIM0005tbl.Rows(WW_LINECNT)("USERNAME")

        '原常備駅
        WF_CURRENTSTATIONNAME.Text = OIM0005tbl.Rows(WW_LINECNT)("CURRENTSTATIONNAME")
        work.WF_SEL_CURRENTSTATIONNAME.Text = OIM0005tbl.Rows(WW_LINECNT)("CURRENTSTATIONNAME")

        '臨時常備駅
        WF_EXTRADINARYSTATIONNAME.Text = OIM0005tbl.Rows(WW_LINECNT)("EXTRADINARYSTATIONNAME")
        work.WF_SEL_EXTRADINARYSTATIONNAME.Text = OIM0005tbl.Rows(WW_LINECNT)("EXTRADINARYSTATIONNAME")

        '原専用種別
        WF_DEDICATETYPENAME.Text = OIM0005tbl.Rows(WW_LINECNT)("DEDICATETYPENAME")
        work.WF_SEL_DEDICATETYPENAME.Text = OIM0005tbl.Rows(WW_LINECNT)("DEDICATETYPENAME")

        '臨時専用種別
        WF_EXTRADINARYTYPENAME.Text = OIM0005tbl.Rows(WW_LINECNT)("EXTRADINARYTYPENAME")
        work.WF_SEL_EXTRADINARYTYPENAME.Text = OIM0005tbl.Rows(WW_LINECNT)("EXTRADINARYTYPENAME")

        '運用場所
        WF_OPERATIONBASENAME.Text = OIM0005tbl.Rows(WW_LINECNT)("OPERATIONBASENAME")
        work.WF_SEL_OPERATIONBASENAME.Text = OIM0005tbl.Rows(WW_LINECNT)("OPERATIONBASENAME")

        '塗色
        WF_COLORNAME.Text = OIM0005tbl.Rows(WW_LINECNT)("COLORNAME")
        work.WF_SEL_COLORNAME.Text = OIM0005tbl.Rows(WW_LINECNT)("COLORNAME")

        '予備1
        WF_RESERVE1.Text = OIM0005tbl.Rows(WW_LINECNT)("RESERVE1")
        work.WF_SEL_RESERVE1.Text = OIM0005tbl.Rows(WW_LINECNT)("RESERVE1")

        '予備2
        WF_RESERVE2.Text = OIM0005tbl.Rows(WW_LINECNT)("RESERVE2")
        work.WF_SEL_RESERVE2.Text = OIM0005tbl.Rows(WW_LINECNT)("RESERVE2")

        '次回指定年月日
        WF_SPECIFIEDDATE.Text = OIM0005tbl.Rows(WW_LINECNT)("SPECIFIEDDATE")
        work.WF_SEL_SPECIFIEDDATE.Text = OIM0005tbl.Rows(WW_LINECNT)("SPECIFIEDDATE")

        '次回全検年月日(JR) 
        WF_JRALLINSPECTIONDATE.Text = OIM0005tbl.Rows(WW_LINECNT)("JRALLINSPECTIONDATE")
        work.WF_SEL_JRALLINSPECTIONDATE.Text = OIM0005tbl.Rows(WW_LINECNT)("JRALLINSPECTIONDATE")

        '現在経年
        WF_PROGRESSYEAR.Text = OIM0005tbl.Rows(WW_LINECNT)("PROGRESSYEAR")
        work.WF_SEL_PROGRESSYEAR.Text = OIM0005tbl.Rows(WW_LINECNT)("PROGRESSYEAR")

        '次回全検時経年
        WF_NEXTPROGRESSYEAR.Text = OIM0005tbl.Rows(WW_LINECNT)("NEXTPROGRESSYEAR")
        work.WF_SEL_NEXTPROGRESSYEAR.Text = OIM0005tbl.Rows(WW_LINECNT)("NEXTPROGRESSYEAR")

        '次回交検年月日(JR）
        WF_JRINSPECTIONDATE.Text = OIM0005tbl.Rows(WW_LINECNT)("JRINSPECTIONDATE")
        work.WF_SEL_JRINSPECTIONDATE.Text = OIM0005tbl.Rows(WW_LINECNT)("JRINSPECTIONDATE")

        '次回交検年月日
        WF_INSPECTIONDATE.Text = OIM0005tbl.Rows(WW_LINECNT)("INSPECTIONDATE")
        work.WF_SEL_INSPECTIONDATE.Text = OIM0005tbl.Rows(WW_LINECNT)("INSPECTIONDATE")

        '次回指定年月日(JR)
        WF_JRSPECIFIEDDATE.Text = OIM0005tbl.Rows(WW_LINECNT)("JRSPECIFIEDDATE")
        work.WF_SEL_JRSPECIFIEDDATE.Text = OIM0005tbl.Rows(WW_LINECNT)("JRSPECIFIEDDATE")

        'JR車番
        WF_JRTANKNUMBER.Text = OIM0005tbl.Rows(WW_LINECNT)("JRTANKNUMBER")
        work.WF_SEL_JRTANKNUMBER.Text = OIM0005tbl.Rows(WW_LINECNT)("JRTANKNUMBER")

        '旧JOT車番
        WF_OLDTANKNUMBER.Text = OIM0005tbl.Rows(WW_LINECNT)("OLDTANKNUMBER")
        work.WF_SEL_OLDTANKNUMBER.Text = OIM0005tbl.Rows(WW_LINECNT)("OLDTANKNUMBER")

        'OT車番
        WF_OTTANKNUMBER.Text = OIM0005tbl.Rows(WW_LINECNT)("OTTANKNUMBER")
        work.WF_SEL_OTTANKNUMBER.Text = OIM0005tbl.Rows(WW_LINECNT)("OTTANKNUMBER")

        'JXTG車番
        WF_JXTGTANKNUMBER.Text = OIM0005tbl.Rows(WW_LINECNT)("JXTGTANKNUMBER")
        work.WF_SEL_JXTGTANKNUMBER.Text = OIM0005tbl.Rows(WW_LINECNT)("JXTGTANKNUMBER")

        'コスモ車番
        WF_COSMOTANKNUMBER.Text = OIM0005tbl.Rows(WW_LINECNT)("COSMOTANKNUMBER")
        work.WF_SEL_COSMOTANKNUMBER.Text = OIM0005tbl.Rows(WW_LINECNT)("COSMOTANKNUMBER")

        '富士石油車番
        WF_FUJITANKNUMBER.Text = OIM0005tbl.Rows(WW_LINECNT)("FUJITANKNUMBER")
        work.WF_SEL_FUJITANKNUMBER.Text = OIM0005tbl.Rows(WW_LINECNT)("FUJITANKNUMBER")

        '出光昭シ車番
        WF_SHELLTANKNUMBER.Text = OIM0005tbl.Rows(WW_LINECNT)("SHELLTANKNUMBER")
        work.WF_SEL_SHELLTANKNUMBER.Text = OIM0005tbl.Rows(WW_LINECNT)("SHELLTANKNUMBER")

        '予備
        WF_RESERVE3.Text = OIM0005tbl.Rows(WW_LINECNT)("RESERVE3")
        work.WF_SEL_RESERVE3.Text = OIM0005tbl.Rows(WW_LINECNT)("RESERVE3")

        '削除フラグ
        WF_DELFLG.Text = OIM0005tbl.Rows(WW_LINECNT)("DELFLG")
        CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_DUMMY)
        work.WF_SEL_DELFLG.Text = OIM0005tbl.Rows(WW_LINECNT)("DELFLG")

        '○ 状態をクリア
        For Each OIM0005row As DataRow In OIM0005tbl.Rows
            Select Case OIM0005row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 選択明細の状態を設定
        Select Case OIM0005tbl.Rows(WW_LINECNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                OIM0005tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                OIM0005tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                OIM0005tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                OIM0005tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                OIM0005tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
        End Select

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(OIM0005tbl)

        WF_GridDBclick.Text = ""

        '############# おためし #############
        'work.WF_SEL_INPTBL.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
        '    Master.USERID & "-" & Master.MAPID & "-" & CS0050SESSION.VIEW_MAP_VARIANT & "-" & Date.Now.ToString("HHmmss") & "INPTBL.txt"
        '遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '画面表示データ保存(遷移先(登録画面)向け)
        Master.SaveTable(OIM0005tbl, work.WF_SEL_INPTBL.Text)

        '登録画面ページへ遷移
        Master.TransitionPage()

    End Sub

    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_Scroll()

    End Sub


    ''' <summary>
    ''' ファイルアップロード時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FILEUPLOAD()

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        '○ UPLOAD XLSデータ取得
        CS0023XLSUPLOAD.CAMPCODE = work.WF_SEL_CAMPCODE.Text        '会社コード
        CS0023XLSUPLOAD.MAPID = Master.MAPID                        '画面ID
        CS0023XLSUPLOAD.CS0023XLSUPLOAD()
        If isNormal(CS0023XLSUPLOAD.ERR) Then
            If CS0023XLSUPLOAD.TBLDATA.Rows.Count = 0 Then
                Master.Output(C_MESSAGE_NO.REGISTRATION_RECORD_NOT_EXIST_ERROR, C_MESSAGE_TYPE.ERR)
                Exit Sub
            End If
        Else
            Master.Output(CS0023XLSUPLOAD.ERR, C_MESSAGE_TYPE.ABORT, "CS0023XLSUPLOAD")
            Exit Sub
        End If

        '○ CS0023XLSUPLOAD.TBLDATAの入力値整備
        Dim WW_COLUMNS As New List(Of String)
        For Each XLSTBLcol As DataColumn In CS0023XLSUPLOAD.TBLDATA.Columns
            WW_COLUMNS.Add(XLSTBLcol.ColumnName.ToString())
        Next

        Dim CS0023XLSTBLrow As DataRow = CS0023XLSUPLOAD.TBLDATA.NewRow
        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            CS0023XLSTBLrow.ItemArray = XLSTBLrow.ItemArray

            For Each XLSTBLcol As DataColumn In CS0023XLSUPLOAD.TBLDATA.Columns
                If IsDBNull(CS0023XLSTBLrow.Item(XLSTBLcol)) OrElse IsNothing(CS0023XLSTBLrow.Item(XLSTBLcol)) Then
                    CS0023XLSTBLrow.Item(XLSTBLcol) = ""
                End If
            Next

            XLSTBLrow.ItemArray = CS0023XLSTBLrow.ItemArray
        Next

        '○ XLSUPLOAD明細⇒INPtbl
        Master.CreateEmptyTable(OIM0005INPtbl)

        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            Dim OIM0005INProw As DataRow = OIM0005INPtbl.NewRow

            '○ 初期クリア
            For Each OIM0005INPcol As DataColumn In OIM0005INPtbl.Columns
                If IsDBNull(OIM0005INProw.Item(OIM0005INPcol)) OrElse IsNothing(OIM0005INProw.Item(OIM0005INPcol)) Then
                    Select Case OIM0005INPcol.ColumnName
                        Case "LINECNT"
                            OIM0005INProw.Item(OIM0005INPcol) = 0
                        Case "OPERATION"
                            OIM0005INProw.Item(OIM0005INPcol) = C_LIST_OPERATION_CODE.NODATA
                        Case "UPDTIMSTP"
                            OIM0005INProw.Item(OIM0005INPcol) = 0
                        Case "SELECT"
                            OIM0005INProw.Item(OIM0005INPcol) = 1
                        Case "HIDDEN"
                            OIM0005INProw.Item(OIM0005INPcol) = 0
                        Case Else
                            OIM0005INProw.Item(OIM0005INPcol) = ""
                    End Select
                End If
            Next

            '○ 変更元情報をデフォルト設定
            If WW_COLUMNS.IndexOf("TANKNUMBER") >= 0 AndAlso
                WW_COLUMNS.IndexOf("MODEL") >= 0 AndAlso
                WW_COLUMNS.IndexOf("ORIGINOWNERCODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("OWNERCODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("LEASECODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("LEASECLASS") >= 0 AndAlso
                WW_COLUMNS.IndexOf("AUTOEXTENTION") >= 0 AndAlso
                WW_COLUMNS.IndexOf("LEASESTYMD") >= 0 AndAlso
                WW_COLUMNS.IndexOf("LEASEENDYMD") >= 0 AndAlso
                WW_COLUMNS.IndexOf("USERCODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("CURRENTSTATIONCODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("EXTRADINARYSTATIONCODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("USERLIMIT") >= 0 AndAlso
                WW_COLUMNS.IndexOf("LIMITTEXTRADIARYSTATION") >= 0 AndAlso
                WW_COLUMNS.IndexOf("DEDICATETYPECODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("EXTRADINARYTYPECODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("EXTRADINARYLIMIT") >= 0 AndAlso
                WW_COLUMNS.IndexOf("OPERATIONBASECODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("COLORCODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("ENEOS") >= 0 AndAlso
                WW_COLUMNS.IndexOf("ECO") >= 0 AndAlso
                WW_COLUMNS.IndexOf("ALLINSPECTIONDATE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("TRANSFERDATE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("OBTAINEDCODE") >= 0 Then
                For Each OIM0005row As DataRow In OIM0005tbl.Rows
                    If XLSTBLrow("TANKNUMBER") = OIM0005row("TANKNUMBER") AndAlso
                        XLSTBLrow("MODEL") = OIM0005row("MODEL") AndAlso
                        XLSTBLrow("ORIGINOWNERCODE") = OIM0005row("ORIGINOWNERCODE") AndAlso
                        XLSTBLrow("OWNERCODE") = OIM0005row("OWNERCODE") AndAlso
                        XLSTBLrow("LEASECODE") = OIM0005row("LEASECODE") AndAlso
                        XLSTBLrow("LEASECLASS") = OIM0005row("LEASECLASS") AndAlso
                        XLSTBLrow("AUTOEXTENTION") = OIM0005row("AUTOEXTENTION") AndAlso
                        XLSTBLrow("LEASESTYMD") = OIM0005row("LEASESTYMD") AndAlso
                        XLSTBLrow("LEASEENDYMD") = OIM0005row("LEASEENDYMD") AndAlso
                        XLSTBLrow("USERCODE") = OIM0005row("USERCODE") AndAlso
                        XLSTBLrow("CURRENTSTATIONCODE") = OIM0005row("CURRENTSTATIONCODE") AndAlso
                        XLSTBLrow("EXTRADINARYSTATIONCODE") = OIM0005row("EXTRADINARYSTATIONCODE") AndAlso
                        XLSTBLrow("USERLIMIT") = OIM0005row("USERLIMIT") AndAlso
                        XLSTBLrow("LIMITTEXTRADIARYSTATION") = OIM0005row("LIMITTEXTRADIARYSTATION") AndAlso
                        XLSTBLrow("DEDICATETYPECODE") = OIM0005row("DEDICATETYPECODE") AndAlso
                        XLSTBLrow("EXTRADINARYTYPECODE") = OIM0005row("EXTRADINARYTYPECODE") AndAlso
                        XLSTBLrow("EXTRADINARYLIMIT") = OIM0005row("EXTRADINARYLIMIT") AndAlso
                        XLSTBLrow("OPERATIONBASECODE") = OIM0005row("OPERATIONBASECODE") AndAlso
                        XLSTBLrow("COLORCODE") = OIM0005row("COLORCODE") AndAlso
                        XLSTBLrow("ENEOS") = OIM0005row("ENEOS") AndAlso
                        XLSTBLrow("ECO") = OIM0005row("ECO") AndAlso
                        XLSTBLrow("ALLINSPECTIONDATE") = OIM0005row("ALLINSPECTIONDATE") AndAlso
                        XLSTBLrow("TRANSFERDATE") = OIM0005row("TRANSFERDATE") AndAlso
                        XLSTBLrow("OBTAINEDCODE") = OIM0005row("OBTAINEDCODE") Then
                        OIM0005INProw.ItemArray = OIM0005row.ItemArray
                        Exit For
                    End If
                Next
            End If

            '○ 項目セット
            ''会社コード
            'OIM0005INProw.Item("CAMPCODE") = work.WF_SEL_CAMPCODE.Text

            'JOT車番
            If WW_COLUMNS.IndexOf("TANKNUMBER") >= 0 Then
                OIM0005INProw("TANKNUMBER") = XLSTBLrow("TANKNUMBER")
            End If
            'OIM0005INProw.Item("TANKNUMBER") = work.WF_SEL_TANKNUMBER.Text

            '型式
            If WW_COLUMNS.IndexOf("MODEL") >= 0 Then
                OIM0005INProw("MODEL") = XLSTBLrow("MODEL")
            End If
            'OIM0005INProw.Item("MODEL") = work.WF_SEL_MODEL.Text

            '原籍所有者C
            If WW_COLUMNS.IndexOf("ORIGINOWNERCODE") >= 0 Then
                OIM0005INProw("ORIGINOWNERCODE") = XLSTBLrow("ORIGINOWNERCODE")
            End If

            '名義所有者C
            If WW_COLUMNS.IndexOf("OWNERCODE") >= 0 Then
                OIM0005INProw("OWNERCODE") = XLSTBLrow("OWNERCODE")
            End If

            'リース先C
            If WW_COLUMNS.IndexOf("LEASECODE") >= 0 Then
                OIM0005INProw("LEASECODE") = XLSTBLrow("LEASECODE")
            End If

            'リース区分C
            If WW_COLUMNS.IndexOf("LEASECLASS") >= 0 Then
                OIM0005INProw("LEASECLASS") = XLSTBLrow("LEASECLASS")
            End If

            '自動延長
            If WW_COLUMNS.IndexOf("AUTOEXTENTION") >= 0 Then
                OIM0005INProw("AUTOEXTENTION") = XLSTBLrow("AUTOEXTENTION")
            End If

            'リース開始年月日
            If WW_COLUMNS.IndexOf("LEASESTYMD") >= 0 Then
                OIM0005INProw("LEASESTYMD") = XLSTBLrow("LEASESTYMD")
            End If

            'リース満了年月日
            If WW_COLUMNS.IndexOf("LEASEENDYMD") >= 0 Then
                OIM0005INProw("LEASEENDYMD") = XLSTBLrow("LEASEENDYMD")
            End If

            '第三者使用者C
            If WW_COLUMNS.IndexOf("USERCODE") >= 0 Then
                OIM0005INProw("USERCODE") = XLSTBLrow("USERCODE")
            End If

            '原常備駅C
            If WW_COLUMNS.IndexOf("CURRENTSTATIONCODE") >= 0 Then
                OIM0005INProw("CURRENTSTATIONCODE") = XLSTBLrow("CURRENTSTATIONCODE")
            End If

            '臨時常備駅C
            If WW_COLUMNS.IndexOf("EXTRADINARYSTATIONCODE") >= 0 Then
                OIM0005INProw("EXTRADINARYSTATIONCODE") = XLSTBLrow("EXTRADINARYSTATIONCODE")
            End If

            '第三者使用期限
            If WW_COLUMNS.IndexOf("USERLIMIT") >= 0 Then
                OIM0005INProw("USERLIMIT") = XLSTBLrow("USERLIMIT")
            End If

            '臨時常備駅期限
            If WW_COLUMNS.IndexOf("LIMITTEXTRADIARYSTATION") >= 0 Then
                OIM0005INProw("LIMITTEXTRADIARYSTATION") = XLSTBLrow("LIMITTEXTRADIARYSTATION")
            End If

            '原専用種別C
            If WW_COLUMNS.IndexOf("DEDICATETYPECODE") >= 0 Then
                OIM0005INProw("DEDICATETYPECODE") = XLSTBLrow("DEDICATETYPECODE")
            End If

            '臨時専用種別C
            If WW_COLUMNS.IndexOf("EXTRADINARYTYPECODE") >= 0 Then
                OIM0005INProw("EXTRADINARYTYPECODE") = XLSTBLrow("EXTRADINARYTYPECODE")
            End If

            '臨時専用期限
            If WW_COLUMNS.IndexOf("EXTRADINARYLIMIT") >= 0 Then
                OIM0005INProw("EXTRADINARYLIMIT") = XLSTBLrow("EXTRADINARYLIMIT")
            End If

            '運用基地C
            If WW_COLUMNS.IndexOf("OPERATIONBASECODE") >= 0 Then
                OIM0005INProw("OPERATIONBASECODE") = XLSTBLrow("OPERATIONBASECODE")
            End If

            '塗色C
            If WW_COLUMNS.IndexOf("COLORCODE") >= 0 Then
                OIM0005INProw("COLORCODE") = XLSTBLrow("COLORCODE")
            End If

            'エネオス
            If WW_COLUMNS.IndexOf("ENEOS") >= 0 Then
                OIM0005INProw("ENEOS") = XLSTBLrow("ENEOS")
            End If

            'エコレール
            If WW_COLUMNS.IndexOf("ECO") >= 0 Then
                OIM0005INProw("ECO") = XLSTBLrow("ECO")
            End If

            '取得年月日
            If WW_COLUMNS.IndexOf("ALLINSPECTIONDATE") >= 0 Then
                OIM0005INProw("ALLINSPECTIONDATE") = XLSTBLrow("ALLINSPECTIONDATE")
            End If

            '車籍編入年月日
            If WW_COLUMNS.IndexOf("TRANSFERDATE") >= 0 Then
                OIM0005INProw("TRANSFERDATE") = XLSTBLrow("TRANSFERDATE")
            End If

            '取得先C
            If WW_COLUMNS.IndexOf("OBTAINEDCODE") >= 0 Then
                OIM0005INProw("OBTAINEDCODE") = XLSTBLrow("OBTAINEDCODE")
            End If

            '削除フラグ
            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                OIM0005INProw("DELFLG") = XLSTBLrow("DELFLG")
            Else
                OIM0005INProw("DELFLG") = "0"
            End If

            '○ 名称取得
            'CODENAME_get("TORICODES", OIM0005INProw("TORICODES"), OIM0005INProw("TORINAMES"), WW_DUMMY)           '取引先名称(出荷先)
            'CODENAME_get("SHUKABASHO", OIM0005INProw("SHUKABASHO"), OIM0005INProw("SHUKABASHONAMES"), WW_DUMMY)   '出荷場所名称

            'CODENAME_get("TORICODET", OIM0005INProw("TORICODET"), OIM0005INProw("TORINAMET"), WW_DUMMY)           '取引先名称(届先)
            'CODENAME_get("TODOKECODE", OIM0005INProw("TODOKECODE"), OIM0005INProw("TODOKENAME"), WW_DUMMY)        '届先名称

            OIM0005INPtbl.Rows.Add(OIM0005INProw)
        Next

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        OIM0005tbl_UPD()

        '○ 画面表示データ保存
        Master.SaveTable(OIM0005tbl)

        '○ メッセージ表示
        If isNormal(WW_ERR_SW) Then
            Master.Output(C_MESSAGE_NO.IMPORT_SUCCESSFUL, C_MESSAGE_TYPE.INF)
        Else
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
        End If

        '○ Close
        CS0023XLSUPLOAD.TBLDATA.Dispose()
        CS0023XLSUPLOAD.TBLDATA.Clear()

    End Sub


    ' ******************************************************************************
    ' ***  詳細表示関連操作                                                      ***
    ' ******************************************************************************

    ''' <summary>
    ''' 詳細画面-表更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_UPDATE_Click()

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        '○ DetailBoxをINPtblへ退避
        DetailBoxToOIM0005INPtbl(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ERR_SW) Then
            OIM0005tbl_UPD()
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIM0005tbl)

        '○ 詳細画面初期化
        If isNormal(WW_ERR_SW) Then
            DetailBoxClear()
        End If

        '○ メッセージ表示
        If WW_ERR_SW = "" Then
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)
        Else
            If isNormal(WW_ERR_SW) Then
                Master.Output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.INF)
            Else
                Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
            End If
        End If

        '○画面切替設定
        WF_BOXChange.Value = "headerbox"

    End Sub

    ''' <summary>
    ''' 詳細画面-テーブル退避
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub DetailBoxToOIM0005INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(WF_DELFLG.Text)            '削除フラグ

        '○ GridViewから未選択状態で表更新ボタンを押下時の例外を回避する
        If String.IsNullOrEmpty(WF_Sel_LINECNT.Text) AndAlso
            String.IsNullOrEmpty(WF_DELFLG.Text) Then
            Master.Output(C_MESSAGE_NO.INVALID_PROCCESS_ERROR, C_MESSAGE_TYPE.ERR, "no Detail")

            CS0011LOGWrite.INFSUBCLASS = "DetailBoxToINPtbl"        'SUBクラス名
            CS0011LOGWrite.INFPOSI = "non Detail"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWrite.TEXT = "non Detail"
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                         'ログ出力

            O_RTN = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            Exit Sub
        End If

        Master.CreateEmptyTable(OIM0005INPtbl)
        Dim OIM0005INProw As DataRow = OIM0005INPtbl.NewRow

        '○ 初期クリア
        For Each OIM0005INPcol As DataColumn In OIM0005INPtbl.Columns
            If IsDBNull(OIM0005INProw.Item(OIM0005INPcol)) OrElse IsNothing(OIM0005INProw.Item(OIM0005INPcol)) Then
                Select Case OIM0005INPcol.ColumnName
                    Case "LINECNT"
                        OIM0005INProw.Item(OIM0005INPcol) = 0
                    Case "OPERATION"
                        OIM0005INProw.Item(OIM0005INPcol) = C_LIST_OPERATION_CODE.NODATA
                    Case "UPDTIMSTP"
                        OIM0005INProw.Item(OIM0005INPcol) = 0
                    Case "SELECT"
                        OIM0005INProw.Item(OIM0005INPcol) = 1
                    Case "HIDDEN"
                        OIM0005INProw.Item(OIM0005INPcol) = 0
                    Case Else
                        OIM0005INProw.Item(OIM0005INPcol) = ""
                End Select
            End If
        Next

        'LINECNT
        If WF_Sel_LINECNT.Text = "" Then
            OIM0005INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(WF_Sel_LINECNT.Text, OIM0005INProw("LINECNT"))
            Catch ex As Exception
                OIM0005INProw("LINECNT") = 0
            End Try
        End If

        OIM0005INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        OIM0005INProw("UPDTIMSTP") = 0
        OIM0005INProw("SELECT") = 1
        OIM0005INProw("HIDDEN") = 0

        'OIM0005INProw("CAMPCODE") = work.WF_SEL_CAMPCODE.Text        '会社コード
        OIM0005INProw("TANKNUMBER") = WF_TANKNUMBER.Text        'JOT車番
        OIM0005INProw("MODEL") = WF_MODEL.Text        '型式

        OIM0005INProw("DELFLG") = WF_DELFLG.Text                     '削除フラグ

        OIM0005INProw("ORIGINOWNERCODE") = WF_ORIGINOWNERCODE.Text              '原籍所有者C

        OIM0005INProw("OWNERCODE") = WF_OWNERCODE.Text              '名義所有者C

        OIM0005INProw("LEASECODE") = WF_LEASECODE.Text              'リース先C

        OIM0005INProw("LEASECLASS") = WF_LEASECLASS.Text              'リース区分C

        OIM0005INProw("AUTOEXTENTION") = WF_AUTOEXTENTION.Text              '自動延長

        OIM0005INProw("LEASESTYMD") = WF_LEASESTYMD.Text              'リース開始年月日

        OIM0005INProw("LEASEENDYMD") = WF_LEASEENDYMD.Text              'リース満了年月日

        OIM0005INProw("USERCODE") = WF_USERCODE.Text              '第三者使用者C

        OIM0005INProw("CURRENTSTATIONCODE") = WF_CURRENTSTATIONCODE.Text              '原常備駅C

        OIM0005INProw("EXTRADINARYSTATIONCODE") = WF_EXTRADINARYSTATIONCODE.Text              '臨時常備駅C

        OIM0005INProw("USERLIMIT") = WF_USERLIMIT.Text              '第三者使用期限

        OIM0005INProw("LIMITTEXTRADIARYSTATION") = WF_LIMITTEXTRADIARYSTATION.Text              '臨時常備駅期限

        OIM0005INProw("DEDICATETYPECODE") = WF_DEDICATETYPECODE.Text              '原専用種別C

        OIM0005INProw("EXTRADINARYTYPECODE") = WF_EXTRADINARYTYPECODE.Text              '臨時専用種別C

        OIM0005INProw("EXTRADINARYLIMIT") = WF_EXTRADINARYLIMIT.Text              '臨時専用期限

        OIM0005INProw("OPERATIONBASECODE") = WF_OPERATIONBASECODE.Text              '運用基地C

        OIM0005INProw("COLORCODE") = WF_COLORCODE.Text              '塗色C

        OIM0005INProw("ENEOS") = WF_ENEOS.Text              'エネオス

        OIM0005INProw("ECO") = WF_ECO.Text              'エコレール

        OIM0005INProw("ALLINSPECTIONDATE") = WF_ALLINSPECTIONDATE.Text              '取得年月日

        OIM0005INProw("TRANSFERDATE") = WF_TRANSFERDATE.Text              '車籍編入年月日

        OIM0005INProw("OBTAINEDCODE") = WF_OBTAINEDCODE.Text              '取得先C

        '○ 名称取得
        'CODENAME_get("TORICODES", OIM0005INProw("TORICODES"), OIM0005INProw("TORINAMES"), WW_DUMMY)           '取引先名称(出荷先)
        'CODENAME_get("SHUKABASHO", OIM0005INProw("SHUKABASHO"), OIM0005INProw("SHUKABASHONAMES"), WW_DUMMY)   '出荷場所名称

        'CODENAME_get("TORICODET", OIM0005INProw("TORICODET"), OIM0005INProw("TORINAMET"), WW_DUMMY)           '取引先名称(届先)
        'CODENAME_get("TODOKECODE", OIM0005INProw("TODOKECODE"), OIM0005INProw("TODOKENAME"), WW_DUMMY)        '届先名称

        '○ チェック用テーブルに登録する
        OIM0005INPtbl.Rows.Add(OIM0005INProw)

    End Sub


    ''' <summary>
    ''' 詳細画面-クリアボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_CLEAR_Click()

        '○ 詳細画面初期化
        DetailBoxClear()

        '○ メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_CLEAR_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        '○画面切替設定
        WF_BOXChange.Value = "headerbox"

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""

    End Sub

    ''' <summary>
    ''' 詳細画面初期化
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailBoxClear()

        '○ 状態をクリア
        For Each OIM0005row As DataRow In OIM0005tbl.Rows
            Select Case OIM0005row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIM0005tbl)

        WF_Sel_LINECNT.Text = ""            'LINECNT

        WF_TANKNUMBER.Text = ""            'JOT車番
        WF_MODEL.Text = ""            '型式
        WF_ORIGINOWNERCODE.Text = ""            '原籍所有者C
        WF_OWNERCODE.Text = ""            '名義所有者C
        WF_LEASECODE.Text = ""            'リース先C
        WF_LEASECLASS.Text = ""            'リース区分C
        WF_AUTOEXTENTION.Text = ""            '自動延長
        WF_LEASESTYMD.Text = ""            'リース開始年月日
        WF_LEASEENDYMD.Text = ""            'リース満了年月日
        WF_USERCODE.Text = ""            '第三者使用者C
        WF_CURRENTSTATIONCODE.Text = ""            '原常備駅C
        WF_EXTRADINARYSTATIONCODE.Text = ""            '臨時常備駅C
        WF_USERLIMIT.Text = ""            '第三者使用期限
        WF_LIMITTEXTRADIARYSTATION.Text = ""            '臨時常備駅期限
        WF_DEDICATETYPECODE.Text = ""            '原専用種別C
        WF_EXTRADINARYTYPECODE.Text = ""            '臨時専用種別C
        WF_EXTRADINARYLIMIT.Text = ""            '臨時専用期限
        WF_OPERATIONBASECODE.Text = ""            '運用基地C
        WF_COLORCODE.Text = ""            '塗色C
        WF_ENEOS.Text = ""            'エネオス
        WF_ECO.Text = ""            'エコレール
        WF_ALLINSPECTIONDATE.Text = ""            '取得年月日
        WF_TRANSFERDATE.Text = ""            '車籍編入年月日
        WF_OBTAINEDCODE.Text = ""            '取得先C
        WF_DELFLG.Text = ""                 '削除フラグ
        WF_DELFLG_TEXT.Text = ""            '削除フラグ名称

    End Sub


    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_DBClick()

        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                Integer.TryParse(WF_LeftMViewChange.Value, WF_LeftMViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try

            Dim WW_FIELD As String = ""
            If WF_FIELD_REP.Value = "" Then
                WW_FIELD = WF_FIELD.Value
            Else
                WW_FIELD = WF_FIELD_REP.Value
            End If

            With leftview
                '会社コード
                Dim prmData As New Hashtable

                'フィールドによってパラメーターを変える
                Select Case WW_FIELD
                    'Case "WF_TORICODES"                             '取引先(出荷場所)
                    '    prmData = work.CreateTORIParam(work.WF_SEL_CAMPCODE.Text)

                        '    Case "WF_SHUKABASHO"                            '出荷場所
                        '        prmData = work.CreateTODOKEParam(work.WF_SEL_CAMPCODE.Text, WF_TORICODES.Text)

                        '    Case "WF_TORICODET"                             '取引先(届先)
                        '        prmData = work.CreateTORIParam(work.WF_SEL_CAMPCODE.Text)

                        '    Case "WF_TODOKECODE"                            '届先
                        '        prmData = work.CreateTODOKEParam(work.WF_SEL_CAMPCODE.Text, WF_TORICODET.Text)

                        '    Case "WF_MODELPT"                               'モデル距離パターン
                        '        prmData = work.CreateMODELPTParam(work.WF_SEL_CAMPCODE.Text, WF_MODELPT.Text)

                    Case "WF_DELFLG"
                        prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = "2"
                End Select

                .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                .ActiveListBox()
            End With
        End If

    End Sub


    ' ******************************************************************************
    ' ***  leftBOX関連操作                                                       ***
    ' ******************************************************************************

    ''' <summary>
    ''' LeftBox選択時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()

        Dim WW_SelectValue As String = ""
        Dim WW_SelectText As String = ""

        '○ 選択内容を取得
        If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
            WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
            WW_SelectValue = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Value
            WW_SelectText = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Text
        End If

        '○ 選択内容を画面項目へセット
        If WF_FIELD_REP.Value = "" Then
            Select Case WF_FIELD.Value
                '削除フラグ
                Case "WF_DELFLG"
                    WF_DELFLG.Text = WW_SelectValue
                    WF_DELFLG_TEXT.Text = WW_SelectText
                    WF_DELFLG.Focus()

                    '    'モデル距離パターン
                    'Case "WF_MODELPT"
                    '    WF_MODELPT.Text = WW_SelectValue
                    '    WF_MODELPT_TEXT.Text = WW_SelectText
                    '    WF_MODELPT.Focus()

                    '    '取引先（出荷場所）
                    'Case "WF_TORICODES"
                    '    WF_TORICODES.Text = WW_SelectValue
                    '    WF_TORICODES_TEXT.Text = WW_SelectText
                    '    WF_TORICODES.Focus()

                    '    '出荷場所
                    'Case "WF_SHUKABASHO"
                    '    WF_SHUKABASHO.Text = WW_SelectValue
                    '    WF_SHUKABASHO_TEXT.Text = WW_SelectText
                    '    WF_SHUKABASHO.Focus()

                    '    '取引先（届先）
                    'Case "WF_TORICODET"
                    '    WF_TORICODET.Text = WW_SelectValue
                    '    WF_TORICODET_TEXT.Text = WW_SelectText
                    '    WF_TORICODET.Focus()

                    '    '届先
                    'Case "WF_TODOKECODE"
                    '    WF_TODOKECODE.Text = WW_SelectValue
                    '    WF_TODOKECODE_TEXT.Text = WW_SelectText
                    '    WF_TODOKECODE.Focus()
            End Select
        Else
        End If

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_RightboxOpen.Value = ""

    End Sub

    ''' <summary>
    ''' LeftBoxキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○ フォーカスセット
        If WF_FIELD_REP.Value = "" Then
            Select Case WF_FIELD.Value
                '削除フラグ
                Case "WF_DELFLG"
                    WF_DELFLG.Focus()

                    '    'モデル距離パターン
                    'Case "WF_MODELPT"
                    '    WF_MODELPT.Focus()

                    '    '取引先（出荷場所）
                    'Case "WF_TORICODES"
                    '    WF_TORICODES.Focus()

                    '    '出荷場所
                    'Case "WF_SHUKABASHO"
                    '    WF_SHUKABASHO.Focus()

                    '    '取引先（届先）
                    'Case "WF_TORICODET"
                    '    WF_TORICODET.Focus()

                    '    '届先
                    'Case "WF_TODOKECODE"
                    '    WF_TODOKECODE.Focus()

            End Select
        Else
        End If

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_RightboxOpen.Value = ""

    End Sub


    ''' <summary>
    ''' RightBoxラジオボタン選択処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RadioButton_Click()

        If Not String.IsNullOrEmpty(WF_RightViewChange.Value) Then
            Try
                Integer.TryParse(WF_RightViewChange.Value, WF_RightViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try

            rightview.SelectIndex(WF_RightViewChange.Value)
            WF_RightViewChange.Value = ""
        End If

    End Sub

    ''' <summary>
    ''' RightBoxメモ欄更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_Change()

        rightview.Save(Master.USERID, Master.USERTERMID, WW_DUMMY)

    End Sub


    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    ''' 入力値チェック
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub INPTableCheck(ByRef O_RTN As String)
        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_LINE_ERR As String = ""
        Dim WW_TEXT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""

        '○ 画面操作権限チェック
        '権限チェック(操作者がデータ内USERの更新権限があるかチェック
        '　※権限判定時点：現在
        CS0025AUTHORget.USERID = CS0050SESSION.USERID
        CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_PERTMIT
        CS0025AUTHORget.CODE = Master.MAPID
        CS0025AUTHORget.STYMD = Date.Now
        CS0025AUTHORget.ENDYMD = Date.Now
        CS0025AUTHORget.CS0025AUTHORget()
        If isNormal(CS0025AUTHORget.ERR) AndAlso CS0025AUTHORget.PERMITCODE = C_PERMISSION.UPDATE Then
        Else
            WW_CheckMES1 = "・更新できないレコード(ユーザ更新権限なし)です。"
            WW_CheckMES2 = ""
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LINE_ERR = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Exit Sub
        End If

        '○ 単項目チェック
        For Each OIM0005INProw As DataRow In OIM0005INPtbl.Rows

            WW_LINE_ERR = ""

            '削除フラグ(バリデーションチェック）
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DELFLG", OIM0005INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("DELFLG", OIM0005INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'JOT車番(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TANKNUMBER", OIM0005INProw("TANKNUMBER"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "JOT車番入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '原籍所有者C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ORIGINOWNERCODE", OIM0005INProw("ORIGINOWNERCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "原籍所有者C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '名義所有者C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OWNERCODE", OIM0005INProw("OWNERCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "名義所有者C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'リース先C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LEASECODE", OIM0005INProw("LEASECODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "リース先C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'リース区分C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LEASECLASS", OIM0005INProw("LEASECLASS"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "リース区分C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '自動延長(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "AUTOEXTENTION", OIM0005INProw("AUTOEXTENTION"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "自動延長入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'リース開始年月日(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LEASESTYMD", OIM0005INProw("LEASESTYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "リース開始年月日入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'リース満了年月日(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LEASEENDYMD", OIM0005INProw("LEASEENDYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "リース満了年月日入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '第三者使用者C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "USERCODE", OIM0005INProw("USERCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "第三者使用者C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '原常備駅C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "CURRENTSTATIONCODE", OIM0005INProw("CURRENTSTATIONCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "原常備駅C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '臨時常備駅C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "EXTRADINARYSTATIONCODE", OIM0005INProw("EXTRADINARYSTATIONCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "臨時常備駅C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '第三者使用期限(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "USERLIMIT", OIM0005INProw("USERLIMIT"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "第三者使用期限入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '臨時常備駅期限(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LIMITTEXTRADIARYSTATION", OIM0005INProw("LIMITTEXTRADIARYSTATION"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "臨時常備駅期限入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '原専用種別C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DEDICATETYPECODE", OIM0005INProw("DEDICATETYPECODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "原専用種別C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '臨時専用種別C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "EXTRADINARYTYPECODE", OIM0005INProw("EXTRADINARYTYPECODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "臨時専用種別C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '臨時専用期限(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "EXTRADINARYLIMIT", OIM0005INProw("EXTRADINARYLIMIT"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "臨時専用期限入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '運用基地C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OPERATIONBASECODE", OIM0005INProw("OPERATIONBASECODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "運用基地C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '塗色C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "COLORCODE", OIM0005INProw("COLORCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "塗色C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'エネオス(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ENEOS", OIM0005INProw("ENEOS"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "エネオス入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'エコレール(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ECO", OIM0005INProw("ECO"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "エコレール入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '取得年月日(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ALLINSPECTIONDATE", OIM0005INProw("ALLINSPECTIONDATE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "取得年月日入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '車籍編入年月日(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TRANSFERDATE", OIM0005INProw("TRANSFERDATE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "車籍編入年月日入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '取得先C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OBTAINEDCODE", OIM0005INProw("OBTAINEDCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "取得先C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '削除フラグ　有効なら関連チェックする。　※削除なら関連チェックせず、削除フラグを立てる
            'If OIM0005INProw("DELFLG") = C_DELETE_FLG.ALIVE Then

            'モデル距離パターン(バリデーションチェック)
            'Master.CheckField(work.WF_SEL_CAMPCODE.Text, "MODELPATTERN", OIM0005INProw("MODELPATTERN"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            'If isNormal(WW_CS0024FCHECKERR) Then
            '    'モデル距離パターン存在チェック
            '    CODENAME_get("MODELPATTERN", OIM0005INProw("MODELPATTERN"), WW_DUMMY, WW_RTN_SW)
            '    If Not isNormal(WW_RTN_SW) Then
            '        WW_CheckMES1 = "モデル距離パターンエラー。'1'：届先のみ  '2':出荷場所、届先指定   '3':出荷場所のみ　のいずれかを入力してください。"
            '        WW_CheckMES2 = WW_CS0024FCHECKREPORT
            '        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
            '        WW_LINE_ERR = "ERR"
            '        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            '    End If
            'Else
            '    WW_CheckMES1 = "モデル距離パターンエラー。'1'：届先のみ  '2':出荷場所、届先指定   '3':出荷場所のみ　のいずれかを入力してください。"
            '    WW_CheckMES2 = WW_CS0024FCHECKREPORT
            '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
            '    WW_LINE_ERR = "ERR"
            '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            'End If

            '関連チェック

            '        Select Case OIM0005INProw("MODELPATTERN")
            '            Case CONST_PATTERN1 '届先のみ
            '                '取引先（届先）、届先、モデル距離が入力されている事
            '                '取引先（届先）、届先がマスタに登録されていること

            '                If OIM0005INProw("TORICODES") = "" AndAlso OIM0005INProw("SHUKABASHO") = "" AndAlso
            '                    OIM0005INProw("TORICODET") <> "" AndAlso OIM0005INProw("TODOKECODE") <> "" Then

            '                    '取引先(届先)コード存在チェック
            '                    CODENAME_get("TORICODET", OIM0005INProw("TORICODET"), WW_DUMMY, WW_RTN_SW)
            '                    If Not isNormal(WW_RTN_SW) Then
            '                        WW_CheckMES1 = "・取引先(届先)コードエラー。マスタに存在しないコードです。"
            '                        WW_CheckMES2 = WW_CS0024FCHECKREPORT
            '                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
            '                        WW_LINE_ERR = "PATTEN ERR"
            '                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            '                    End If

            '                    '届先コード存在チェック
            '                    work.WF_SEL_TORICODET.Text = OIM0005INProw("TORICODET")
            '                    CODENAME_get("TODOKECODE", OIM0005INProw("TODOKECODE"), WW_DUMMY, WW_RTN_SW)  '届先名称
            '                    If Not isNormal(WW_RTN_SW) Then
            '                        WW_CheckMES1 = "・届先コードエラー。マスタに存在しないコードです。"
            '                        WW_CheckMES2 = WW_CS0024FCHECKREPORT
            '                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
            '                        WW_LINE_ERR = "PATTEN ERR"
            '                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            '                    End If
            '                Else
            '                    WW_CheckMES1 = "・モデル距離パターン組合せエラー。取引先（届先）コードと届先コードのみ入力してください。"
            '                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
            '                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
            '                    WW_LINE_ERR = "PATTEN ERR"
            '                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            '                End If

            '            Case CONST_PATTERN2 '出荷場所、届先
            '                '取引先（出荷場所）、出荷場所、取引先（届先）、届先、モデル距離が入力されている事
            '                '取引先（出荷場所）、出荷場所、取引先（届先）、届先がマスタに登録されていること

            '                If OIM0005INProw("TORICODES") <> "" AndAlso OIM0005INProw("SHUKABASHO") <> "" AndAlso
            '                    OIM0005INProw("TORICODET") <> "" AndAlso OIM0005INProw("TODOKECODE") <> "" Then

            '                    '取引先(出荷先)コード存在チェック
            '                    CODENAME_get("TORICODES", OIM0005INProw("TORICODES"), WW_DUMMY, WW_RTN_SW)
            '                    If Not isNormal(WW_RTN_SW) Then
            '                        WW_CheckMES1 = "・取引先(出荷先)コードエラー。マスタに存在しないコードです。"
            '                        WW_CheckMES2 = WW_CS0024FCHECKREPORT
            '                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
            '                        WW_LINE_ERR = "PATTEN ERR"
            '                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            '                    End If

            '                    '出荷場所コード存在チェック
            '                    work.WF_SEL_TORICODES.Text = OIM0005INProw("TORICODES")
            '                    CODENAME_get("SHUKABASHO", OIM0005INProw("SHUKABASHO"), WW_DUMMY, WW_RTN_SW)  '出荷場所名
            '                    If Not isNormal(WW_RTN_SW) Then
            '                        WW_CheckMES1 = "・出荷場所コードエラー。マスタに存在しないコードです。"
            '                        WW_CheckMES2 = WW_CS0024FCHECKREPORT
            '                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
            '                        WW_LINE_ERR = "PATTEN ERR"
            '                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            '                    End If

            '                    '取引先(届先)コード存在チェック
            '                    CODENAME_get("TORICODET", OIM0005INProw("TORICODET"), WW_DUMMY, WW_RTN_SW)
            '                    If Not isNormal(WW_RTN_SW) Then
            '                        WW_CheckMES1 = "・取引先(届先)コードエラー。マスタに存在しないコードです。"
            '                        WW_CheckMES2 = WW_CS0024FCHECKREPORT
            '                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
            '                        WW_LINE_ERR = "PATTEN ERR"
            '                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            '                    End If

            '                    '届先コード存在チェック
            '                    work.WF_SEL_TORICODET.Text = OIM0005INProw("TORICODET")
            '                    CODENAME_get("TODOKECODE", OIM0005INProw("TODOKECODE"), WW_DUMMY, WW_RTN_SW)  '届先名称
            '                    If Not isNormal(WW_RTN_SW) Then
            '                        WW_CheckMES1 = "・届先コードエラー。マスタに存在しないコードです。"
            '                        WW_CheckMES2 = WW_CS0024FCHECKREPORT
            '                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
            '                        WW_LINE_ERR = "PATTEN ERR"
            '                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            '                    End If
            '                Else
            '                    WW_CheckMES1 = "モデル距離パターン組合せエラー。取引先（出荷場所）コード、出荷場所コード、取引先（届先）コード、届先コードを入力してください。"
            '                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
            '                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
            '                    WW_LINE_ERR = "PATTEN ERR"
            '                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            '                End If

            '            Case CONST_PATTERN3 '出荷場所のみ
            '                '取引先（出荷場所）、出荷場所、モデル距離が入力されている事
            '                '取引先（出荷場所）、出荷場所がマスタに登録されていること
            '                If OIM0005INProw("TORICODES") <> "" AndAlso OIM0005INProw("SHUKABASHO") <> "" AndAlso
            '                    OIM0005INProw("TORICODET") = "" AndAlso OIM0005INProw("TODOKECODE") = "" Then

            '                    '取引先(出荷先)コード存在チェック
            '                    CODENAME_get("TORICODES", OIM0005INProw("TORICODES"), WW_DUMMY, WW_RTN_SW)
            '                    If Not isNormal(WW_RTN_SW) Then
            '                        WW_CheckMES1 = "・取引先(出荷先)コードエラー。マスタに存在しないコードです。"
            '                        WW_CheckMES2 = WW_CS0024FCHECKREPORT
            '                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
            '                        WW_LINE_ERR = "PATTEN ERR"
            '                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            '                    End If

            '                    '出荷場所コード存在チェック
            '                    work.WF_SEL_TORICODES.Text = OIM0005INProw("TORICODES")
            '                    CODENAME_get("SHUKABASHO", OIM0005INProw("SHUKABASHO"), WW_DUMMY, WW_RTN_SW)  '出荷場所名
            '                    If Not isNormal(WW_RTN_SW) Then
            '                        WW_CheckMES1 = "・出荷場所コードエラー。マスタに存在しないコードです。"
            '                        WW_CheckMES2 = WW_CS0024FCHECKREPORT
            '                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
            '                        WW_LINE_ERR = "PATTEN ERR"
            '                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            '                    End If
            '                Else
            '                    WW_CheckMES1 = "・モデル距離パターン組合せエラー。取引先（出荷先）コードと出荷場所コードのみ入力してください。"
            '                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
            '                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
            '                    WW_LINE_ERR = "PATTEN ERR"
            '                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            '                End If
            '        End Select
            'End If

            If WW_LINE_ERR = "" Then
                If OIM0005INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    OIM0005INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LINE_ERR = CONST_PATTERNERR Then
                    '関連チェックエラーをセット
                    OIM0005INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    '単項目チェックエラーをセット
                    OIM0005INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                End If
            End If
        Next

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="OIM0005row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIM0005row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIM0005row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> JOT車番 =" & OIM0005row("TANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 原籍所有者C =" & OIM0005row("ORIGINOWNERCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 名義所有者C =" & OIM0005row("OWNERCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> リース先C =" & OIM0005row("LEASECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> リース区分C =" & OIM0005row("LEASECLASS") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 自動延長 =" & OIM0005row("AUTOEXTENTION") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> リース開始年月日 =" & OIM0005row("LEASESTYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> リース満了年月日 =" & OIM0005row("LEASEENDYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 第三者使用者C =" & OIM0005row("USERCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 原常備駅C =" & OIM0005row("CURRENTSTATIONCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 臨時常備駅C =" & OIM0005row("EXTRADINARYSTATIONCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 第三者使用期限 =" & OIM0005row("USERLIMIT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 臨時常備駅期限 =" & OIM0005row("LIMITTEXTRADIARYSTATION") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 原専用種別C =" & OIM0005row("DEDICATETYPECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 臨時専用種別C =" & OIM0005row("EXTRADINARYTYPECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 臨時専用期限 =" & OIM0005row("EXTRADINARYLIMIT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 運用基地C =" & OIM0005row("OPERATIONBASECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 塗色C =" & OIM0005row("COLORCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> エネオス =" & OIM0005row("ENEOS") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> エコレール =" & OIM0005row("ECO") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 取得年月日 =" & OIM0005row("ALLINSPECTIONDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 車籍編入年月日 =" & OIM0005row("TRANSFERDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 取得先C =" & OIM0005row("OBTAINEDCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 形式 =" & OIM0005row("MODEL") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 形式カナ =" & OIM0005row("MODELKANA") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 荷重 =" & OIM0005row("LOAD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 荷重単位 =" & OIM0005row("LOADUNIT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 容積 =" & OIM0005row("VOLUME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 容積単位 =" & OIM0005row("VOLUMEUNIT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 原籍所有者 =" & OIM0005row("ORIGINOWNERNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 名義所有者 =" & OIM0005row("OWNERNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> リース先 =" & OIM0005row("LEASENAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> リース区分 =" & OIM0005row("LEASECLASSNEMAE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 第三者使用者 =" & OIM0005row("USERNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 原常備駅 =" & OIM0005row("CURRENTSTATIONNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 臨時常備駅 =" & OIM0005row("EXTRADINARYSTATIONNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 原専用種別 =" & OIM0005row("DEDICATETYPENAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 臨時専用種別 =" & OIM0005row("EXTRADINARYTYPENAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 運用場所 =" & OIM0005row("OPERATIONBASENAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 塗色 =" & OIM0005row("COLORNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 予備1 =" & OIM0005row("RESERVE1") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 予備2 =" & OIM0005row("RESERVE2") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 次回指定年月日 =" & OIM0005row("SPECIFIEDDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 次回全検年月日(JR)  =" & OIM0005row("JRALLINSPECTIONDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 現在経年 =" & OIM0005row("PROGRESSYEAR") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 次回全検時経年 =" & OIM0005row("NEXTPROGRESSYEAR") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 次回交検年月日(JR） =" & OIM0005row("JRINSPECTIONDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 次回交検年月日 =" & OIM0005row("INSPECTIONDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 次回指定年月日(JR) =" & OIM0005row("JRSPECIFIEDDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JR車番 =" & OIM0005row("JRTANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 旧JOT車番 =" & OIM0005row("OLDTANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> OT車番 =" & OIM0005row("OTTANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JXTG車番 =" & OIM0005row("JXTGTANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> コスモ車番 =" & OIM0005row("COSMOTANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 富士石油車番 =" & OIM0005row("FUJITANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 出光昭シ車番 =" & OIM0005row("SHELLTANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 予備 =" & OIM0005row("RESERVE3") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除フラグ =" & OIM0005row("DELFLG")
        End If

        rightview.AddErrorReport(WW_ERR_MES)

    End Sub

    ''' <summary>
    ''' 遷移先(登録画面)退避データ保存先の作成
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_CreateXMLSaveFile()
        work.WF_SEL_INPTBL.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
            Master.USERID & "-" & Master.MAPID & "-" & CS0050SESSION.VIEW_MAP_VARIANT & "-" & Date.Now.ToString("HHmmss") & "INPTBL.txt"

    End Sub

    ''' <summary>
    ''' OIM0005tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub OIM0005tbl_UPD()

        '○ 画面状態設定
        For Each OIM0005row As DataRow In OIM0005tbl.Rows
            Select Case OIM0005row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each OIM0005INProw As DataRow In OIM0005INPtbl.Rows

            'エラーレコード読み飛ばし
            If OIM0005INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            OIM0005INProw.Item("OPERATION") = CONST_INSERT

            'KEY項目が等しい時
            For Each OIM0005row As DataRow In OIM0005tbl.Rows
                If OIM0005row("TANKNUMBER") = OIM0005INProw("TANKNUMBER") AndAlso
                    OIM0005row("MODEL") = OIM0005INProw("MODEL") Then
                    'KEY項目以外の項目に変更がないときは「操作」の項目は空白にする
                    If OIM0005row("DELFLG") = OIM0005INProw("DELFLG") AndAlso
                        OIM0005row("ORIGINOWNERCODE") = OIM0005INProw("ORIGINOWNERCODE") AndAlso
                        OIM0005row("OWNERCODE") = OIM0005INProw("OWNERCODE") AndAlso
                        OIM0005row("LEASECODE") = OIM0005INProw("LEASECODE") AndAlso
                        OIM0005row("LEASECLASS") = OIM0005INProw("LEASECLASS") AndAlso
                        OIM0005row("AUTOEXTENTION") = OIM0005INProw("AUTOEXTENTION") AndAlso
                        OIM0005row("LEASESTYMD") = OIM0005INProw("LEASESTYMD") AndAlso
                        OIM0005row("LEASEENDYMD") = OIM0005INProw("LEASEENDYMD") AndAlso
                        OIM0005row("USERCODE") = OIM0005INProw("USERCODE") AndAlso
                        OIM0005row("CURRENTSTATIONCODE") = OIM0005INProw("CURRENTSTATIONCODE") AndAlso
                        OIM0005row("EXTRADINARYSTATIONCODE") = OIM0005INProw("EXTRADINARYSTATIONCODE") AndAlso
                        OIM0005row("USERLIMIT") = OIM0005INProw("USERLIMIT") AndAlso
                        OIM0005row("LIMITTEXTRADIARYSTATION") = OIM0005INProw("LIMITTEXTRADIARYSTATION") AndAlso
                        OIM0005row("DEDICATETYPECODE") = OIM0005INProw("DEDICATETYPECODE") AndAlso
                        OIM0005row("EXTRADINARYTYPECODE") = OIM0005INProw("EXTRADINARYTYPECODE") AndAlso
                        OIM0005row("EXTRADINARYLIMIT") = OIM0005INProw("EXTRADINARYLIMIT") AndAlso
                        OIM0005row("OPERATIONBASECODE") = OIM0005INProw("OPERATIONBASECODE") AndAlso
                        OIM0005row("COLORCODE") = OIM0005INProw("COLORCODE") AndAlso
                        OIM0005row("ENEOS") = OIM0005INProw("ENEOS") AndAlso
                        OIM0005row("ECO") = OIM0005INProw("ECO") AndAlso
                        OIM0005row("ALLINSPECTIONDATE") = OIM0005INProw("ALLINSPECTIONDATE") AndAlso
                        OIM0005row("TRANSFERDATE") = OIM0005INProw("TRANSFERDATE") AndAlso
                        OIM0005row("OBTAINEDCODE") = OIM0005INProw("OBTAINEDCODE") AndAlso
                        OIM0005row("MODEL") = OIM0005INProw("MODEL") AndAlso
                        OIM0005row("MODELKANA") = OIM0005INProw("MODELKANA") AndAlso
                        OIM0005row("LOAD") = OIM0005INProw("LOAD") AndAlso
                        OIM0005row("LOADUNIT") = OIM0005INProw("LOADUNIT") AndAlso
                        OIM0005row("VOLUME") = OIM0005INProw("VOLUME") AndAlso
                        OIM0005row("VOLUMEUNIT") = OIM0005INProw("VOLUMEUNIT") AndAlso
                        OIM0005row("ORIGINOWNERNAME") = OIM0005INProw("ORIGINOWNERNAME") AndAlso
                        OIM0005row("OWNERNAME") = OIM0005INProw("OWNERNAME") AndAlso
                        OIM0005row("LEASENAME") = OIM0005INProw("LEASENAME") AndAlso
                        OIM0005row("LEASECLASSNEMAE") = OIM0005INProw("LEASECLASSNEMAE") AndAlso
                        OIM0005row("USERNAME") = OIM0005INProw("USERNAME") AndAlso
                        OIM0005row("CURRENTSTATIONNAME") = OIM0005INProw("CURRENTSTATIONNAME") AndAlso
                        OIM0005row("EXTRADINARYSTATIONNAME") = OIM0005INProw("EXTRADINARYSTATIONNAME") AndAlso
                        OIM0005row("DEDICATETYPENAME") = OIM0005INProw("DEDICATETYPENAME") AndAlso
                        OIM0005row("EXTRADINARYTYPENAME") = OIM0005INProw("EXTRADINARYTYPENAME") AndAlso
                        OIM0005row("OPERATIONBASENAME") = OIM0005INProw("OPERATIONBASENAME") AndAlso
                        OIM0005row("COLORNAME") = OIM0005INProw("COLORNAME") AndAlso
                        OIM0005row("RESERVE1") = OIM0005INProw("RESERVE1") AndAlso
                        OIM0005row("RESERVE2") = OIM0005INProw("RESERVE2") AndAlso
                        OIM0005row("SPECIFIEDDATE") = OIM0005INProw("SPECIFIEDDATE") AndAlso
                        OIM0005row("JRALLINSPECTIONDATE") = OIM0005INProw("JRALLINSPECTIONDATE") AndAlso
                        OIM0005row("PROGRESSYEAR") = OIM0005INProw("PROGRESSYEAR") AndAlso
                        OIM0005row("NEXTPROGRESSYEAR") = OIM0005INProw("NEXTPROGRESSYEAR") AndAlso
                        OIM0005row("JRINSPECTIONDATE") = OIM0005INProw("JRINSPECTIONDATE") AndAlso
                        OIM0005row("INSPECTIONDATE") = OIM0005INProw("INSPECTIONDATE") AndAlso
                        OIM0005row("JRSPECIFIEDDATE") = OIM0005INProw("JRSPECIFIEDDATE") AndAlso
                        OIM0005row("JRTANKNUMBER") = OIM0005INProw("JRTANKNUMBER") AndAlso
                        OIM0005row("OLDTANKNUMBER") = OIM0005INProw("OLDTANKNUMBER") AndAlso
                        OIM0005row("OTTANKNUMBER") = OIM0005INProw("OTTANKNUMBER") AndAlso
                        OIM0005row("JXTGTANKNUMBER") = OIM0005INProw("JXTGTANKNUMBER") AndAlso
                        OIM0005row("COSMOTANKNUMBER") = OIM0005INProw("COSMOTANKNUMBER") AndAlso
                        OIM0005row("FUJITANKNUMBER") = OIM0005INProw("FUJITANKNUMBER") AndAlso
                        OIM0005row("SHELLTANKNUMBER") = OIM0005INProw("SHELLTANKNUMBER") AndAlso
                        OIM0005row("RESERVE3") = OIM0005INProw("RESERVE3") AndAlso
                        OIM0005INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA Then
                    Else
                        'KEY項目以外の項目に変更がある時は「操作」の項目を「更新」に設定する
                        OIM0005INProw("OPERATION") = CONST_UPDATE
                        Exit For
                    End If

                    Exit For

                End If
            Next
        Next

        '○ 変更有無判定　&　入力値反映
        For Each OIM0005INProw As DataRow In OIM0005INPtbl.Rows
            Select Case OIM0005INProw("OPERATION")
                Case CONST_UPDATE
                    TBL_UPDATE_SUB(OIM0005INProw)
                Case CONST_INSERT
                    TBL_INSERT_SUB(OIM0005INProw)
                Case CONST_PATTERNERR
                    '関連チェックエラーの場合、キーが変わるため、行追加してエラーレコードを表示させる
                    TBL_INSERT_SUB(OIM0005INProw)
                Case C_LIST_OPERATION_CODE.ERRORED
                    TBL_ERR_SUB(OIM0005INProw)
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="OIM0005INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef OIM0005INProw As DataRow)

        For Each OIM0005row As DataRow In OIM0005tbl.Rows

            '同一レコードか判定
            If OIM0005INProw("TANKNUMBER") = OIM0005row("TANKNUMBER") Then
                '画面入力テーブル項目設定
                OIM0005INProw("LINECNT") = OIM0005row("LINECNT")
                OIM0005INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                OIM0005INProw("UPDTIMSTP") = OIM0005row("UPDTIMSTP")
                OIM0005INProw("SELECT") = 1
                OIM0005INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0005row.ItemArray = OIM0005INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 追加予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0005INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef OIM0005INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim OIM0005row As DataRow = OIM0005tbl.NewRow
        OIM0005row.ItemArray = OIM0005INProw.ItemArray

        OIM0005row("LINECNT") = OIM0005tbl.Rows.Count + 1
        If OIM0005INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
            OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        Else
            OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
        End If

        OIM0005row("UPDTIMSTP") = "0"
        OIM0005row("SELECT") = 1
        OIM0005row("HIDDEN") = 0

        OIM0005tbl.Rows.Add(OIM0005row)

    End Sub


    ''' <summary>
    ''' エラーデータの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0005INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_ERR_SUB(ByRef OIM0005INProw As DataRow)

        For Each OIM0005row As DataRow In OIM0005tbl.Rows

            '同一レコードか判定
            If OIM0005INProw("TANKNUMBER") = OIM0005row("TANKNUMBER") Then
                '画面入力テーブル項目設定
                OIM0005INProw("LINECNT") = OIM0005row("LINECNT")
                OIM0005INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                OIM0005INProw("UPDTIMSTP") = OIM0005row("UPDTIMSTP")
                OIM0005INProw("SELECT") = 1
                OIM0005INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0005row.ItemArray = OIM0005INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 名称取得
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByVal I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String)

        O_TEXT = ""
        O_RTN = ""

        If I_VALUE = "" Then
            O_RTN = C_MESSAGE_NO.NORMAL
            Exit Sub
        End If
        Dim prmData As New Hashtable

        Try
            Select Case I_FIELD
                Case "CAMPCODE"         '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)

                'Case "UORG"             '運用部署
                '    prmData = work.CreateUORGParam(work.WF_SEL_CAMPCODE.Text)
                '    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)

                    'Case "TORICODES"     '取引先名称(出荷先)
                    '    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CUSTOMER, I_VALUE, O_TEXT, O_RTN, work.CreateTORIParam(work.WF_SEL_CAMPCODE.Text))

                    'Case "SHUKABASHO"   '出荷場所名称
                    '    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DISTINATION, I_VALUE, O_TEXT, O_RTN, work.CreateTODOKEParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_TORICODES.Text))

                    'Case "TORICODET"     '取引先名称（届先）
                    '    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CUSTOMER, I_VALUE, O_TEXT, O_RTN, work.CreateTORIParam(work.WF_SEL_CAMPCODE.Text))

                    'Case "TODOKECODE"   '届先名称
                    '    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DISTINATION, I_VALUE, O_TEXT, O_RTN, work.CreateTODOKEParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_TORICODET.Text))

                    'Case "MODELPATTERN" 'モデル距離パターン
                    '    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_MODELPT, I_VALUE, O_TEXT, O_RTN, work.CreateMODELPTParam(work.WF_SEL_CAMPCODE.Text, WF_MODELPT.Text))

                Case "DELFLG"           '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DELFLG"))

            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
