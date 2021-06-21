''************************************************************
' タンク車マスタメンテ一覧画面
' 作成日 2019/11/08
' 更新日 2021/05/07
' 作成者 JOT遠藤
' 更新者 JOT伊草
'
' 修正履歴:2019/11/08 新規作成
'         :2021/04/13 1)項目「利用者フラグ」を区分値→名称で表示するように変更
'         :           2)登録・更新画面にて更新メッセージが設定された場合
'         :             画面下部に更新メッセージを表示するように修正
'         :2021/05/07 1)項目「油種中分類」「中間点検年月」「中間点検場所」「中間点検実施者」
'         :             「自主点検年月」「自主点検場所」「自主点検実施者」を追加
'         :2021/05/18 1)項目「点検実施者(社員名)」を追加
'         :2021/05/25 1)検索項目に「運用基地コード」「リース先」を追加
'         :2021/06/17 1)検索項目に「削除含む」チェックボックスを追加
'         :           2)削除済みのレコードの背景色を灰色に設定する
'         :           3)項目「運用基地（サブ）」「削除理由区分」「全検計画年月」
'         :             「休車フラグ」「休車日」「取得価格」「内部塗装」
'         :             「安全弁」「センターバルブ情報」を追加
'         :             項目名称変更「請負リース区分」→「請負請負リース区分」
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
    Private OIM0005tbl As DataTable                                 '一覧格納用テーブル
    Private OIM0005INPtbl As DataTable                              'チェック用テーブル
    Private OIM0005UPDtbl As DataTable                              '更新用テーブル

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
                        Case "WF_ButtonEND"             '戻るボタン押下
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
                        Case "WF_Field_DBClick"         'フィールドダブルクリック
                            WF_FIELD_DBClick()
                        Case "WF_LeftBoxSelectClick"    'フィールドチェンジ
                            WF_FIELD_Change()
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

        '〇 更新画面からの遷移の場合、更新完了メッセージを出力
        If Not String.IsNullOrEmpty(work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text) Then
            Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)
            work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""
        End If

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
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0005C Then
            Master.RecoverTable(OIM0005tbl, work.WF_SEL_INPTBL.Text)

            '長さフラグ
            WF_FIL_LENGTHFLG.Text = work.WF_SEL_LENGTHFLG.Text
            CODENAME_get("LENGTHFLG", WF_FIL_LENGTHFLG.Text, WF_FIL_LENGTHFLG_TEXT.Text, WW_RTN_SW)
        End If

        '〇 削除フラグ表示位置の初期化
        WF_DELFLG_INDEX_Initialize()

    End Sub

    ''' <summary>
    ''' 削除フラグ表示位置の初期化
    ''' </summary>
    Protected Sub WF_DELFLG_INDEX_Initialize()

        WF_DELFLG_INDEX.Value = "NON"

        '○ 削除フラグ表示位置をDBより取得する
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()

            Dim dt = New DataTable
            dt.Columns.Clear()
            dt.Clear()

            '検索SQL
            Dim SQLBldr = New StringBuilder(String.Empty)
            SQLBldr.AppendLine(" SELECT")
            SQLBldr.AppendLine("     TMP.SEQ")
            SQLBldr.AppendLine(" FROM (")
            SQLBldr.AppendLine("     SELECT")
            SQLBldr.AppendLine("         ROW_NUMBER() OVER(ORDER BY POSICOL) - 1 AS SEQ")
            SQLBldr.AppendLine("         , FIELD")
            SQLBldr.AppendLine("     FROM")
            SQLBldr.AppendLine("         com.OIS0012_PROFMVIEW")
            SQLBldr.AppendLine("     WHERE")
            SQLBldr.AppendLine("         MAPID = 'OIM0005L'")
            SQLBldr.AppendLine("     AND HDKBN = 'H'")
            SQLBldr.AppendLine("     AND EFFECT = 'Y'")
            SQLBldr.AppendLine("     AND DELFLG <> '1'")
            SQLBldr.AppendLine("     AND POSICOL > 0")
            SQLBldr.AppendLine(" ) TMP")
            SQLBldr.AppendLine(" WHERE")
            SQLBldr.AppendLine("     TMP.FIELD = 'DELFLG'")

            Try
                Using SQLcmd = New SqlCommand(SQLBldr.ToString, SQLcon)
                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        '○ フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            dt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        '○ テーブル検索結果をテーブル格納
                        dt.Load(SQLdr)

                        If dt.Rows.Count > 0 Then
                            '〇 削除フラグ表示位置に設定
                            WF_DELFLG_INDEX.Value = dt.Rows(0)("SEQ").ToString
                        End If

                    End Using
                End Using

            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0005L WF_DELFLG_INDEX_Initialize")

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:OIM0005L WF_DELFLG_INDEX_Initialize"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

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

        '〇 一覧の件数を取得
        Me.WF_ListCNT.Text = "件数：" + OIM0005tbl.Rows.Count.ToString()

        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each OIM0005row As DataRow In OIM0005tbl.Rows
            'Filter
            If WF_FIL_LENGTHFLG.Text <> "" AndAlso OIM0005row("LENGTHFLG") <> WF_FIL_LENGTHFLG.Text Then
                OIM0005row("HIDDEN") = 1
            End If

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

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(OIM0005tbl)

        TBLview.Sort = "LINECNT"
        TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DISPROWCOUNT).ToString()

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Horizontal
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

        Dim whereAddFlg As Boolean = False

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
        Dim SQLStr As String =
              " SELECT " _
            & "   0                                                            AS LINECNT " _
            & " , ''                                                           AS OPERATION " _
            & " , CAST(OIM0005.UPDTIMSTP AS bigint)                            AS UPDTIMSTP " _
            & " , 1                                                            AS 'SELECT' " _
            & " , 0                                                            AS HIDDEN " _
            & " , ISNULL(RTRIM(OIM0005.DELFLG), '')                            AS DELFLG " _
            & " , ISNULL(RTRIM(OIM0005.TANKNUMBER), '')                        AS TANKNUMBER " _
            & " , ISNULL(RTRIM(OIM0005.MODEL), '')                             AS MODEL " _
            & " , ISNULL(RTRIM(OIM0005.MODELKANA), '')                         AS MODELKANA " _
            & " , ISNULL(RTRIM(OIM0005.LOAD), '')                              AS LOAD " _
            & " , ISNULL(RTRIM(OIM0005.LOADUNIT), '')                          AS LOADUNIT " _
            & " , ISNULL(RTRIM(OIM0005.VOLUME), '')                            AS VOLUME " _
            & " , ISNULL(RTRIM(OIM0005.VOLUMEUNIT), '')                        AS VOLUMEUNIT " _
            & " , ISNULL(RTRIM(OIM0005.MYWEIGHT), '')                          AS MYWEIGHT " _
            & " , ISNULL(RTRIM(OIM0005.LENGTH), '')                            AS LENGTH " _
            & " , ISNULL(RTRIM(OIM0005.TANKLENGTH), '')                        AS TANKLENGTH " _
            & " , ISNULL(RTRIM(OIM0005.MAXCALIBER), '')                        AS MAXCALIBER " _
            & " , ISNULL(RTRIM(OIM0005.MINCALIBER), '')                        AS MINCALIBER " _
            & " , ISNULL(RTRIM(OIM0005.LENGTHFLG), '')                         AS LENGTHFLG " _
            & " , ISNULL(RTRIM(OIM0005.ORIGINOWNERCODE), '')                   AS ORIGINOWNERCODE " _
            & " , ISNULL(RTRIM(OIM0005.ORIGINOWNERNAME), '')                   AS ORIGINOWNERNAME " _
            & " , ISNULL(RTRIM(OIM0005.OWNERCODE), '')                         AS OWNERCODE " _
            & " , ISNULL(RTRIM(OIM0005.OWNERNAME), '')                         AS OWNERNAME " _
            & " , ISNULL(RTRIM(OIM0005.LEASECODE), '')                         AS LEASECODE " _
            & " , ISNULL(RTRIM(OIM0005.LEASENAME), '')                         AS LEASENAME " _
            & " , ISNULL(RTRIM(OIM0005.LEASECLASS), '')                        AS LEASECLASS " _
            & " , ISNULL(RTRIM(OIM0005.LEASECLASSNAME), '')                    AS LEASECLASSNAME " _
            & " , ISNULL(RTRIM(OIM0005.AUTOEXTENTION), '')                     AS AUTOEXTENTION " _
            & " , ISNULL(RTRIM(OIM0005.AUTOEXTENTIONNAME), '')                 AS AUTOEXTENTIONNAME " _
            & " , CASE WHEN OIM0005.LEASESTYMD IS NULL THEN '' " _
            & "   ELSE FORMAT(OIM0005.LEASESTYMD,'yyyy/MM/dd')" _
            & "   END                                                          AS LEASESTYMD " _
            & " , CASE WHEN OIM0005.LEASEENDYMD IS NULL THEN '' " _
            & "   ELSE FORMAT(OIM0005.LEASEENDYMD,'yyyy/MM/dd') " _
            & "   END                                                          AS LEASEENDYMD " _
            & " , ISNULL(RTRIM(OIM0005.USERCODE), '')                          AS USERCODE " _
            & " , ISNULL(RTRIM(OIM0005.USERNAME), '')                          AS USERNAME " _
            & " , ISNULL(RTRIM(OIM0005.CURRENTSTATIONCODE), '')                AS CURRENTSTATIONCODE " _
            & " , ISNULL(RTRIM(OIM0005.CURRENTSTATIONNAME), '')                AS CURRENTSTATIONNAME " _
            & " , ISNULL(RTRIM(OIM0005.EXTRADINARYSTATIONCODE), '')            AS EXTRADINARYSTATIONCODE " _
            & " , ISNULL(RTRIM(OIM0005.EXTRADINARYSTATIONNAME), '')            AS EXTRADINARYSTATIONNAME " _
            & " , CASE WHEN OIM0005.USERLIMIT IS NULL THEN '' " _
            & "   ELSE FORMAT(OIM0005.USERLIMIT,'yyyy/MM/dd') " _
            & "   END                                                          AS USERLIMIT " _
            & " , CASE WHEN OIM0005.LIMITTEXTRADIARYSTATION IS NULL THEN '' " _
            & "   ELSE FORMAT(OIM0005.LIMITTEXTRADIARYSTATION,'yyyy/MM/dd') " _
            & "   END                                                          AS LIMITTEXTRADIARYSTATION " _
            & " , ISNULL(RTRIM(OIM0005.DEDICATETYPECODE), '')                  AS DEDICATETYPECODE " _
            & " , ISNULL(RTRIM(OIM0005.DEDICATETYPENAME), '')                  AS DEDICATETYPENAME " _
            & " , ISNULL(RTRIM(OIM0005.EXTRADINARYTYPECODE), '')               AS EXTRADINARYTYPECODE " _
            & " , ISNULL(RTRIM(OIM0005.EXTRADINARYTYPENAME), '')               AS EXTRADINARYTYPENAME " _
            & " , CASE WHEN OIM0005.EXTRADINARYLIMIT IS NULL THEN '' " _
            & "   ELSE FORMAT(OIM0005.EXTRADINARYLIMIT,'yyyy/MM/dd') " _
            & "   END                                                          AS EXTRADINARYLIMIT " _
            & " , ISNULL(RTRIM(OIM0005.BIGOILCODE), '')                        AS BIGOILCODE " _
            & " , ISNULL(RTRIM(OIM0005.BIGOILNAME), '')                        AS BIGOILNAME " _
            & " , ISNULL(RTRIM(OIM0005.MIDDLEOILCODE), '')                     AS MIDDLEOILCODE " _
            & " , ISNULL(RTRIM(OIM0005.MIDDLEOILNAME), '')                     AS MIDDLEOILNAME " _
            & " , ISNULL(RTRIM(OIM0005.OPERATIONBASECODE), '')                 AS OPERATIONBASECODE " _
            & " , ISNULL(RTRIM(OIM0005.OPERATIONBASENAME), '')                 AS OPERATIONBASENAME " _
            & " , ISNULL(RTRIM(OIM0005.SUBOPERATIONBASECODE), '')              AS SUBOPERATIONBASECODE " _
            & " , ISNULL(RTRIM(OIM0005.SUBOPERATIONBASENAME), '')              AS SUBOPERATIONBASENAME " _
            & " , ISNULL(RTRIM(OIM0005.COLORCODE), '')                         AS COLORCODE " _
            & " , ISNULL(RTRIM(OIM0005.COLORNAME), '')                         AS COLORNAME " _
            & " , ISNULL(RTRIM(OIM0005.MARKCODE), '')                          AS MARKCODE " _
            & " , ISNULL(RTRIM(OIM0005.MARKNAME), '')                          AS MARKNAME " _
            & " , ISNULL(RTRIM(OIM0005.JXTGTAGCODE1), '')                      AS JXTGTAGCODE1 " _
            & " , ISNULL(RTRIM(OIM0005.JXTGTAGNAME1), '')                      AS JXTGTAGNAME1 " _
            & " , ISNULL(RTRIM(OIM0005.JXTGTAGCODE2), '')                      AS JXTGTAGCODE2 " _
            & " , ISNULL(RTRIM(OIM0005.JXTGTAGNAME2), '')                      AS JXTGTAGNAME2 " _
            & " , ISNULL(RTRIM(OIM0005.JXTGTAGCODE3), '')                      AS JXTGTAGCODE3 " _
            & " , ISNULL(RTRIM(OIM0005.JXTGTAGNAME3), '')                      AS JXTGTAGNAME3 " _
            & " , ISNULL(RTRIM(OIM0005.JXTGTAGCODE4), '')                      AS JXTGTAGCODE4 " _
            & " , ISNULL(RTRIM(OIM0005.JXTGTAGNAME4), '')                      AS JXTGTAGNAME4 " _
            & " , ISNULL(RTRIM(OIM0005.IDSSTAGCODE), '')                       AS IDSSTAGCODE " _
            & " , ISNULL(RTRIM(OIM0005.IDSSTAGNAME), '')                       AS IDSSTAGNAME " _
            & " , ISNULL(RTRIM(OIM0005.COSMOTAGCODE), '')                      AS COSMOTAGCODE " _
            & " , ISNULL(RTRIM(OIM0005.COSMOTAGNAME), '')                      AS COSMOTAGNAME " _
            & " , ISNULL(RTRIM(OIM0005.RESERVE1), '')                          AS RESERVE1 " _
            & " , ISNULL(RTRIM(OIM0005.RESERVE2), '')                          AS RESERVE2 " _
            & " , CASE WHEN OIM0005.JRINSPECTIONDATE IS NULL THEN '' " _
            & "   ELSE FORMAT(OIM0005.JRINSPECTIONDATE,'yyyy/MM/dd') " _
            & "   END                                                          AS JRINSPECTIONDATE " _
            & " , CASE WHEN OIM0005.INSPECTIONDATE IS NULL THEN '' " _
            & "   ELSE FORMAT(OIM0005.INSPECTIONDATE,'yyyy/MM/dd') " _
            & "   END                                                          AS INSPECTIONDATE " _
            & " , CASE WHEN OIM0005.JRSPECIFIEDDATE IS NULL THEN '' " _
            & "   ELSE FORMAT(OIM0005.JRSPECIFIEDDATE,'yyyy/MM/dd') " _
            & "   END                                                          AS JRSPECIFIEDDATE " _
            & " , CASE WHEN OIM0005.SPECIFIEDDATE IS NULL THEN '' " _
            & "   ELSE FORMAT(OIM0005.SPECIFIEDDATE,'yyyy/MM/dd') " _
            & "   END                                                          AS SPECIFIEDDATE " _
            & " , CASE WHEN OIM0005.JRALLINSPECTIONDATE IS NULL THEN '' " _
            & "   ELSE FORMAT(OIM0005.JRALLINSPECTIONDATE,'yyyy/MM/dd') " _
            & "   END                                                          AS JRALLINSPECTIONDATE " _
            & " , CASE WHEN OIM0005.ALLINSPECTIONDATE IS NULL THEN '' " _
            & "   ELSE FORMAT(OIM0005.ALLINSPECTIONDATE,'yyyy/MM/dd') " _
            & "   END                                                          AS ALLINSPECTIONDATE " _
            & " , CASE WHEN OIM0005.PREINSPECTIONDATE IS NULL THEN '' " _
            & "   ELSE FORMAT(OIM0005.PREINSPECTIONDATE,'yyyy/MM/dd') " _
            & "   END                                                          AS PREINSPECTIONDATE " _
            & " , CASE WHEN OIM0005.GETDATE IS NULL THEN '' " _
            & "   ELSE FORMAT(OIM0005.GETDATE,'yyyy/MM/dd') " _
            & "   END                                                          AS GETDATE " _
            & " , CASE WHEN OIM0005.TRANSFERDATE IS NULL THEN '' " _
            & "   ELSE FORMAT(OIM0005.TRANSFERDATE,'yyyy/MM/dd') " _
            & "   END                                                          AS TRANSFERDATE " _
            & " , ISNULL(RTRIM(OIM0005.OBTAINEDCODE), '')                      AS OBTAINEDCODE " _
            & " , ISNULL(RTRIM(OIM0005.OBTAINEDNAME), '')                      AS OBTAINEDNAME " _
            & " , CAST(ISNULL(RTRIM(OIM0005.PROGRESSYEAR), '') AS VarChar)     AS PROGRESSYEAR " _
            & " , CAST(ISNULL(RTRIM(OIM0005.NEXTPROGRESSYEAR), '') AS VarChar) AS NEXTPROGRESSYEAR " _
            & " , CASE WHEN OIM0005.EXCLUDEDATE IS NULL THEN '' " _
            & "   ELSE FORMAT(OIM0005.EXCLUDEDATE,'yyyy/MM/dd') " _
            & "   END                                                          AS EXCLUDEDATE " _
            & " , CASE WHEN OIM0005.RETIRMENTDATE IS NULL THEN '' " _
            & "   ELSE FORMAT(OIM0005.RETIRMENTDATE,'yyyy/MM/dd') " _
            & "   END                                                          AS RETIRMENTDATE " _
            & " , ISNULL(RTRIM(OIM0005.JRTANKNUMBER), '')                      AS JRTANKNUMBER " _
            & " , ISNULL(RTRIM(OIM0005.JRTANKTYPE), '')                        AS JRTANKTYPE " _
            & " , ISNULL(RTRIM(OIM0005.OLDTANKNUMBER), '')                     AS OLDTANKNUMBER " _
            & " , ISNULL(RTRIM(OIM0005.OTTANKNUMBER), '')                      AS OTTANKNUMBER " _
            & " , ISNULL(RTRIM(OIM0005.JXTGTANKNUMBER1), '')                   AS JXTGTANKNUMBER1 " _
            & " , ISNULL(RTRIM(OIM0005.JXTGTANKNUMBER2), '')                   AS JXTGTANKNUMBER2 " _
            & " , ISNULL(RTRIM(OIM0005.JXTGTANKNUMBER3), '')                   AS JXTGTANKNUMBER3 " _
            & " , ISNULL(RTRIM(OIM0005.JXTGTANKNUMBER4), '')                   AS JXTGTANKNUMBER4 " _
            & " , ISNULL(RTRIM(OIM0005.COSMOTANKNUMBER), '')                   AS COSMOTANKNUMBER " _
            & " , ISNULL(RTRIM(OIM0005.FUJITANKNUMBER), '')                    AS FUJITANKNUMBER " _
            & " , ISNULL(RTRIM(OIM0005.SHELLTANKNUMBER), '')                   AS SHELLTANKNUMBER " _
            & " , ISNULL(RTRIM(OIM0005.SAPSHELLTANKNUMBER), '')                AS SAPSHELLTANKNUMBER " _
            & " , ISNULL(RTRIM(OIM0005.RESERVE3), '')                          AS RESERVE3 " _
            & " , ISNULL(RTRIM(OIM0005.USEDFLG), '')                           AS USEDFLG " _
            & " , ''                                                           AS USEDFLGNAME " _
            & " , CASE WHEN OIM0005.INTERINSPECTYM IS NULL THEN '' " _
            & "   ELSE FORMAT(OIM0005.INTERINSPECTYM,'yyyy/MM') " _
            & "   END                                                          AS INTERINSPECTYM " _
            & " , ISNULL(RTRIM(OIM0005.INTERINSPECTSTATION), '')               AS INTERINSPECTSTATION " _
            & " , ''                                                           AS INTERINSPECTSTATIONNAME " _
            & " , ISNULL(RTRIM(OIM0005.INTERINSPECTORGCODE), '')               AS INTERINSPECTORGCODE " _
            & " , ''                                                           AS INTERINSPECTORGNAME " _
            & " , CASE WHEN OIM0005.SELFINSPECTYM IS NULL THEN '' " _
            & "   ELSE FORMAT(OIM0005.SELFINSPECTYM,'yyyy/MM') " _
            & "   END                                                          AS SELFINSPECTYM " _
            & " , ISNULL(RTRIM(OIM0005.SELFINSPECTSTATION), '')                AS SELFINSPECTSTATION " _
            & " , ''                                                           AS SELFINSPECTSTATIONNAME " _
            & " , ISNULL(RTRIM(OIM0005.SELFINSPECTORGCODE), '')                AS SELFINSPECTORGCODE " _
            & " , ''                                                           AS SELFINSPECTORGNAME " _
            & " , ISNULL(RTRIM(OIM0005.INSPECTMEMBERNAME), '')                 AS INSPECTMEMBERNAME " _
            & " , CASE WHEN OIM0005.ALLINSPECTPLANYM IS NULL THEN '' " _
            & "   ELSE FORMAT(OIM0005.ALLINSPECTPLANYM,'yyyy/MM') " _
            & "   END                                                          AS ALLINSPECTPLANYM " _
            & " , ISNULL(RTRIM(OIM0005.SUSPENDFLG), '')                        AS SUSPENDFLG " _
            & " , ''                                                           AS SUSPENDFLGNAME " _
            & " , CASE WHEN OIM0005.SUSPENDDATE IS NULL THEN '' " _
            & "   ELSE FORMAT(OIM0005.SUSPENDDATE,'yyyy/MM/dd') " _
            & "   END                                                          AS SUSPENDDATE " _
            & " , CASE WHEN OIM0005.PURCHASEPRICE IS NULL THEN '' " _
            & "   ELSE FORMAT(OIM0005.PURCHASEPRICE,'#,##0') " _
            & "   END                                                          AS PURCHASEPRICE " _
            & " , ISNULL(RTRIM(OIM0005.INTERNALCOATING), '')                   AS INTERNALCOATING " _
            & " , ''                                                           AS INTERNALCOATINGNAME " _
            & " , ISNULL(RTRIM(OIM0005.SAFETYVALVE), '')                       AS SAFETYVALVE " _
            & " , ISNULL(RTRIM(OIM0005.CENTERVALVEINFO), '')                   AS CENTERVALVEINFO " _
            & " , ISNULL(RTRIM(OIM0005.DELREASONKBN), '')                      AS DELREASONKBN " _
            & " , ''                                                           AS DELREASONKBNNAME " _
            & " FROM OIL.OIM0005_TANK OIM0005 "


        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '「削除含む」チェックボックスがONの場合、削除フラグは検索条件から除外する
        If work.WF_SEL_DELFLG_S.Text.Equals(False.ToString) Then
            SQLStr &= " WHERE DELFLG <> '1'"
            whereAddFlg = True
        End If

        'タンク車№
        If Not String.IsNullOrEmpty(work.WF_SEL_TANKNUMBER.Text) Then
            If whereAddFlg Then
                SQLStr &= String.Format("    AND OIM0005.TANKNUMBER = '{0}'", work.WF_SEL_TANKNUMBER.Text)
            Else
                SQLStr &= String.Format(" WHERE OIM0005.TANKNUMBER = '{0}'", work.WF_SEL_TANKNUMBER.Text)
            End If
        End If

        '型式
        If Not String.IsNullOrEmpty(work.WF_SEL_MODEL.Text) Then
            If whereAddFlg Then
                SQLStr &= String.Format("    AND OIM0005.MODEL = '{0}'", work.WF_SEL_MODEL.Text)
            Else
                SQLStr &= String.Format(" WHERE OIM0005.MODEL = '{0}'", work.WF_SEL_MODEL.Text)
            End If
        End If

        '利用フラグ
        If Not String.IsNullOrEmpty(work.WF_SEL_USEDFLG.Text) Then
            If whereAddFlg Then
                SQLStr &= String.Format("    AND OIM0005.USEDFLG = '{0}'", work.WF_SEL_USEDFLG.Text)
            Else
                SQLStr &= String.Format(" WHERE OIM0005.USEDFLG = '{0}'", work.WF_SEL_USEDFLG.Text)
            End If
        End If

        '運用基地コード
        If Not String.IsNullOrEmpty(work.WF_SEL_OPERATIONBASECODE_S.Text) Then
            If whereAddFlg Then
                SQLStr &= String.Format("    AND OIM0005.OPERATIONBASECODE = '{0}'",
                                        work.WF_SEL_OPERATIONBASECODE_S.Text)
            Else
                SQLStr &= String.Format(" WHERE OIM0005.OPERATIONBASECODE = '{0}'",
                                        work.WF_SEL_OPERATIONBASECODE_S.Text)
            End If
        End If

        'リース先コード
        If Not String.IsNullOrEmpty(work.WF_SEL_LEASECODE_S.Text) Then
            If whereAddFlg Then
                SQLStr &= String.Format("    AND OIM0005.LEASECODE = '{0}'",
                                        work.WF_SEL_LEASECODE_S.Text)
            Else
                SQLStr &= String.Format(" WHERE OIM0005.LEASECODE = '{0}'",
                                        work.WF_SEL_LEASECODE_S.Text)
            End If
        End If

        SQLStr &=
              " ORDER BY" _
            & "    RIGHT('0000000000' + CAST(OIM0005.TANKNUMBER AS NVARCHAR), 10)"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)

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

                    '利用フラグ
                    CODENAME_get("USEDFLG", OIM0005row("USEDFLG"), OIM0005row("USEDFLGNAME"), WW_DUMMY)

                    '中間点検場所
                    CODENAME_get("STATIONFOCUSON", OIM0005row("INTERINSPECTSTATION"),
                                 OIM0005row("INTERINSPECTSTATIONNAME"), WW_DUMMY)
                    '中間点検実施者
                    CODENAME_get("ORG", OIM0005row("INTERINSPECTORGCODE"),
                                 OIM0005row("INTERINSPECTORGNAME"), WW_DUMMY)
                    '自主点検場所
                    CODENAME_get("STATIONFOCUSON", OIM0005row("SELFINSPECTSTATION"),
                                 OIM0005row("SELFINSPECTSTATIONNAME"), WW_DUMMY)
                    '自主点検実施者
                    CODENAME_get("ORG", OIM0005row("SELFINSPECTORGCODE"),
                                 OIM0005row("SELFINSPECTORGNAME"), WW_DUMMY)
                    '休車フラグ
                    CODENAME_get("SUSPENDFLG", OIM0005row("SUSPENDFLG"),
                                 OIM0005row("SUSPENDFLGNAME"), WW_DUMMY)
                    '内部塗装
                    CODENAME_get("INTERNALCOATING", OIM0005row("INTERNALCOATING"),
                                 OIM0005row("INTERNALCOATINGNAME"), WW_DUMMY)
                    '削除理由区分
                    CODENAME_get("DELREASONKBN", OIM0005row("DELREASONKBN"),
                                 OIM0005row("DELREASONKBNNAME"), WW_DUMMY)
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
            'Filter
            If WF_FIL_LENGTHFLG.Text <> "" AndAlso OIM0005row("LENGTHFLG") <> WF_FIL_LENGTHFLG.Text Then
                OIM0005row("HIDDEN") = 1
            End If

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
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Horizontal
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
                Dim prmData As New Hashtable

                'フィールドによってパラメータを変える
                Select Case WF_FIELD.Value

                            '長さフラグ
                    Case "WF_FIL_LENGTHFLG"
                        prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "LENGTHFLG")

                End Select

                .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                .ActiveListBox()
            End With
        End If

    End Sub

    ''' <summary>
    ''' フィールドチェンジ時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_Change()

        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value
            '長さフラグ
            Case "WF_FIL_LENGTHFLG"
                CODENAME_get("LENGTHFLG", WF_FIL_LENGTHFLG.Text, WF_FIL_LENGTHFLG_TEXT.Text, WW_RTN_SW)
                WF_GridPosition.Text = "1"
        End Select

        '○ メッセージ表示
        If isNormal(WW_RTN_SW) Then
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.NOR)
        Else
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.ERR)
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

                Case "WF_FIL_LENGTHFLG"    '長さフラグ
                    WF_FIL_LENGTHFLG.Text = WW_SelectValue
                    WF_FIL_LENGTHFLG_TEXT.Text = WW_SelectText
                    WF_FIL_LENGTHFLG.Focus()

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

                Case "WF_FIL_LENGTHFLG"                     '長さフラグ
                    WF_FIL_LENGTHFLG.Focus()

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
    ''' 追加ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonINSERT_Click()

        '長さフラグ(Filter)
        work.WF_SEL_LENGTHFLG.Text = WF_FIL_LENGTHFLG.Text

        '選択行
        work.WF_SEL_LINECNT.Text = ""

        'JOT車番(登録(新規追加用))
        work.WF_SEL_TANKNUMBER2.Text = ""

        '原籍所有者C
        work.WF_SEL_ORIGINOWNERCODE.Text = ""

        '名義所有者C
        work.WF_SEL_OWNERCODE.Text = ""

        'リース先C
        work.WF_SEL_LEASECODE.Text = ""

        '請負リース区分C
        work.WF_SEL_LEASECLASS.Text = ""

        '自動延長
        work.WF_SEL_AUTOEXTENTION.Text = ""

        '自動延長名
        work.WF_SEL_AUTOEXTENTIONNAME.Text = ""

        'リース開始年月日
        work.WF_SEL_LEASESTYMD.Text = ""

        'リース満了年月日
        work.WF_SEL_LEASEENDYMD.Text = ""

        '第三者使用者C
        work.WF_SEL_USERCODE.Text = ""

        '原常備駅C
        work.WF_SEL_CURRENTSTATIONCODE.Text = ""

        '臨時常備駅C
        work.WF_SEL_EXTRADINARYSTATIONCODE.Text = ""

        '第三者使用期限
        work.WF_SEL_USERLIMIT.Text = ""

        '臨時常備駅期限
        work.WF_SEL_LIMITTEXTRADIARYSTATION.Text = ""

        '原専用種別C
        work.WF_SEL_DEDICATETYPECODE.Text = ""

        '臨時専用種別C
        work.WF_SEL_EXTRADINARYTYPECODE.Text = ""

        '臨時専用期限
        work.WF_SEL_EXTRADINARYLIMIT.Text = ""

        '油種大分類コード
        work.WF_SEL_BIGOILCODE.Text = ""

        '油種大分類名
        work.WF_SEL_BIGOILNAME.Text = ""

        '油種中分類コード
        work.WF_SEL_MIDDLEOILCODE.Text = ""

        '油種中分類名
        work.WF_SEL_MIDDLEOILNAME.Text = ""

        '運用基地C
        work.WF_SEL_OPERATIONBASECODE.Text = ""

        '運用基地C（サブ）
        work.WF_SEL_SUBOPERATIONBASECODE.Text = ""

        '塗色C
        work.WF_SEL_COLORCODE.Text = ""

        'マークコード
        work.WF_SEL_MARKCODE.Text = ""

        'マーク名
        work.WF_SEL_MARKNAME.Text = ""

        'JXTG仙台タグコード
        work.WF_SEL_JXTGTAGCODE1.Text = ""

        'JXTG仙台タグ名
        work.WF_SEL_JXTGTAGNAME1.Text = ""

        'JXTG千葉タグコード
        work.WF_SEL_JXTGTAGCODE2.Text = ""

        'JXTG千葉タグ名
        work.WF_SEL_JXTGTAGNAME2.Text = ""

        'JXTG川崎タグコード
        work.WF_SEL_JXTGTAGCODE3.Text = ""

        'JXTG川崎タグ名
        work.WF_SEL_JXTGTAGNAME3.Text = ""

        'JXTG根岸タグコード
        work.WF_SEL_JXTGTAGCODE4.Text = ""

        'JXTG根岸タグ名
        work.WF_SEL_JXTGTAGNAME4.Text = ""

        '出光昭和タグコード
        work.WF_SEL_IDSSTAGCODE.Text = ""

        '出光昭和タグ名
        work.WF_SEL_IDSSTAGNAME.Text = ""

        'コスモタグコード
        work.WF_SEL_COSMOTAGCODE.Text = ""

        'コスモタグ名
        work.WF_SEL_COSMOTAGNAME.Text = ""

        '次回全検年月日
        work.WF_SEL_ALLINSPECTIONDATE.Text = ""

        '前回全検年月日
        work.WF_SEL_PREINSPECTIONDATE.Text = ""

        '取得年月日
        work.WF_SEL_GETDATE.Text = ""

        '車籍編入年月日
        work.WF_SEL_TRANSFERDATE.Text = ""

        '取得先C
        work.WF_SEL_OBTAINEDCODE.Text = ""

        '取得先名
        work.WF_SEL_OBTAINEDNAME.Text = ""

        '車籍除外年月日
        work.WF_SEL_EXCLUDEDATE.Text = ""

        '資産除却年月日
        work.WF_SEL_RETIRMENTDATE.Text = ""

        '形式(登録(新規追加用))
        work.WF_SEL_MODEL2.Text = ""

        '形式カナ
        work.WF_SEL_MODELKANA.Text = ""

        '荷重
        work.WF_SEL_LOAD.Text = "0.0"

        '荷重単位
        work.WF_SEL_LOADUNIT.Text = ""

        '容積
        work.WF_SEL_VOLUME.Text = "0.0"

        '容積単位
        work.WF_SEL_VOLUMEUNIT.Text = ""

        '自重
        work.WF_SEL_MYWEIGHT.Text = "0.0"

        'タンク車長
        work.WF_SEL_LENGTH.Text = "0"

        'タンク車体長
        work.WF_SEL_TANKLENGTH.Text = "0"

        '最大口径
        work.WF_SEL_MAXCALIBER.Text = "0"

        '最小口径
        work.WF_SEL_MINCALIBER.Text = "0"

        '長さフラグ
        work.WF_SEL_LENGTHFLG2.Text = ""

        '原籍所有者
        work.WF_SEL_ORIGINOWNERNAME.Text = ""

        '名義所有者
        work.WF_SEL_OWNERNAME.Text = ""

        'リース先
        work.WF_SEL_LEASENAME.Text = ""

        '請負リース区分
        work.WF_SEL_LEASECLASSNAME.Text = ""

        '第三者使用者
        work.WF_SEL_USERNAME.Text = ""

        '原常備駅
        work.WF_SEL_CURRENTSTATIONNAME.Text = ""

        '臨時常備駅
        work.WF_SEL_EXTRADINARYSTATIONNAME.Text = ""

        '原専用種別
        work.WF_SEL_DEDICATETYPENAME.Text = ""

        '臨時専用種別
        work.WF_SEL_EXTRADINARYTYPENAME.Text = ""

        '運用場所
        work.WF_SEL_OPERATIONBASENAME.Text = ""

        '運用場所（サブ）
        work.WF_SEL_SUBOPERATIONBASENAME.Text = ""

        '塗色
        work.WF_SEL_COLORNAME.Text = ""

        '予備1
        work.WF_SEL_RESERVE1.Text = ""

        '予備2
        work.WF_SEL_RESERVE2.Text = ""

        '次回指定年月日
        work.WF_SEL_SPECIFIEDDATE.Text = ""

        '次回全検年月日(JR) 
        work.WF_SEL_JRALLINSPECTIONDATE.Text = ""

        '現在経年
        work.WF_SEL_PROGRESSYEAR.Text = ""

        '次回全検時経年
        work.WF_SEL_NEXTPROGRESSYEAR.Text = ""

        '次回交検年月日(JR）
        work.WF_SEL_JRINSPECTIONDATE.Text = ""

        '次回交検年月日
        work.WF_SEL_INSPECTIONDATE.Text = ""

        '次回指定年月日(JR)
        work.WF_SEL_JRSPECIFIEDDATE.Text = ""

        'JR車番
        work.WF_SEL_JRTANKNUMBER.Text = ""

        'JR車種コード
        work.WF_SEL_JRTANKTYPE.Text = ""

        '旧JOT車番
        work.WF_SEL_OLDTANKNUMBER.Text = ""

        'OT車番
        work.WF_SEL_OTTANKNUMBER.Text = ""

        'JXTG仙台車番
        work.WF_SEL_JXTGTANKNUMBER1.Text = ""

        'JXTG千葉車番
        work.WF_SEL_JXTGTANKNUMBER2.Text = ""

        'JXTG川崎車番
        work.WF_SEL_JXTGTANKNUMBER3.Text = ""

        'JXTG根岸車番
        work.WF_SEL_JXTGTANKNUMBER4.Text = ""

        'コスモ車番
        work.WF_SEL_COSMOTANKNUMBER.Text = ""

        '富士石油車番
        work.WF_SEL_FUJITANKNUMBER.Text = ""

        '出光昭シ車番
        work.WF_SEL_SHELLTANKNUMBER.Text = ""

        'SAP出光昭シ車番
        work.WF_SEL_SAPSHELLTANKNUMBER.Text = ""

        '予備
        work.WF_SEL_RESERVE3.Text = ""

        '利用フラグ(登録(新規追加用))
        work.WF_SEL_USEDFLG2.Text = "1"

        '中間点検年月
        work.WF_SEL_INTERINSPECTYM.Text = ""

        '中間点検場所
        work.WF_SEL_INTERINSPECTSTATION.Text = ""

        '中間点検実施者
        work.WF_SEL_INTERINSPECTORGCODE.Text = ""

        '自主点検年月
        work.WF_SEL_SELFINSPECTYM.Text = ""

        '自主点検場所
        work.WF_SEL_SELFINSPECTSTATION.Text = ""

        '自主点検実施者
        work.WF_SEL_SELFINSPECTORGCODE.Text = ""

        '点検実施者(社員名)
        work.WF_SEL_INSPECTMEMBERNAME.Text = ""

        '全検計画年月
        work.WF_SEL_ALLINSPECTPLANYM.Text = ""

        '休車フラグ
        work.WF_SEL_SUSPENDFLG.Text = ""

        '休車日
        work.WF_SEL_SUSPENDDATE.Text = ""

        '取得価格
        work.WF_SEL_PURCHASEPRICE.Text = "0"

        '内部塗装
        work.WF_SEL_INTERNALCOATING.Text = ""

        '安全弁
        work.WF_SEL_SAFETYVALVE.Text = ""

        'センターバルブ情報
        work.WF_SEL_CENTERVALVEINFO.Text = ""

        '削除
        work.WF_SEL_DELFLG.Text = "0"

        '削除理由区分
        work.WF_SEL_DELREASONKBN.Text = ""

        '詳細画面更新メッセージ
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(OIM0005tbl)

        WF_GridDBclick.Text = ""

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
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
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
            & "        , MARKCODE = @P19" _
            & "        , MARKNAME = @P20" _
            & "        , GETDATE = @P21" _
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
            & "        , LEASECLASSNAME = @P33" _
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
            & "        , JXTGTANKNUMBER1 = @P53" _
            & "        , COSMOTANKNUMBER = @P54" _
            & "        , FUJITANKNUMBER = @P55" _
            & "        , SHELLTANKNUMBER = @P56" _
            & "        , RESERVE3 = @P57" _
            & "        , USEDFLG = @P65" _
            & "        , UPDYMD = @P61" _
            & "        , UPDUSER = @P62" _
            & "        , UPDTERMID = @P63" _
            & "        , RECEIVEYMD = @P64" _
            & "        , MYWEIGHT = @P66" _
            & "        , AUTOEXTENTIONNAME = @P67" _
            & "        , BIGOILCODE = @P68" _
            & "        , BIGOILNAME = @P69" _
            & "        , JXTGTAGCODE1 = @P70" _
            & "        , JXTGTAGNAME1 = @P71" _
            & "        , JXTGTAGCODE2 = @P72" _
            & "        , JXTGTAGNAME2 = @P73" _
            & "        , JXTGTAGCODE3 = @P74" _
            & "        , JXTGTAGNAME3 = @P75" _
            & "        , JXTGTAGCODE4 = @P76" _
            & "        , JXTGTAGNAME4 = @P77" _
            & "        , IDSSTAGCODE = @P78" _
            & "        , IDSSTAGNAME = @P79" _
            & "        , COSMOTAGCODE = @P80" _
            & "        , COSMOTAGNAME = @P81" _
            & "        , ALLINSPECTIONDATE = @P82" _
            & "        , PREINSPECTIONDATE = @P83" _
            & "        , OBTAINEDNAME = @P84" _
            & "        , EXCLUDEDATE = @P85" _
            & "        , RETIRMENTDATE = @P86" _
            & "        , JRTANKTYPE = @P87" _
            & "        , JXTGTANKNUMBER2 = @P88" _
            & "        , JXTGTANKNUMBER3 = @P89" _
            & "        , JXTGTANKNUMBER4 = @P90" _
            & "        , SAPSHELLTANKNUMBER = @P91" _
            & "        , LENGTH = @P92" _
            & "        , TANKLENGTH = @P93" _
            & "        , MAXCALIBER = @P94" _
            & "        , MINCALIBER = @P95" _
            & "        , LENGTHFLG = @P96" _
            & "        , INTERINSPECTYM = @P97" _
            & "        , INTERINSPECTSTATION = @P98" _
            & "        , INTERINSPECTORGCODE = @P99" _
            & "        , SELFINSPECTYM = @P100" _
            & "        , SELFINSPECTSTATION = @P101" _
            & "        , SELFINSPECTORGCODE = @P102" _
            & "        , MIDDLEOILCODE = @P103" _
            & "        , MIDDLEOILNAME = @P104" _
            & "        , INSPECTMEMBERNAME = @P105" _
            & "        , SUBOPERATIONBASECODE = @P106" _
            & "        , SUBOPERATIONBASENAME = @P107" _
            & "        , ALLINSPECTPLANYM = @P108" _
            & "        , SUSPENDFLG = @P109" _
            & "        , SUSPENDDATE = @P110" _
            & "        , PURCHASEPRICE = @P111" _
            & "        , INTERNALCOATING = @P112" _
            & "        , SAFETYVALVE = @P113" _
            & "        , CENTERVALVEINFO = @P114" _
            & "        , DELREASONKBN = @P115" _
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
            & "        , MARKCODE" _
            & "        , MARKNAME" _
            & "        , GETDATE" _
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
            & "        , LEASECLASSNAME" _
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
            & "        , JXTGTANKNUMBER1" _
            & "        , COSMOTANKNUMBER" _
            & "        , FUJITANKNUMBER" _
            & "        , SHELLTANKNUMBER" _
            & "        , RESERVE3" _
            & "        , USEDFLG" _
            & "        , INITYMD" _
            & "        , INITUSER" _
            & "        , INITTERMID" _
            & "        , UPDYMD" _
            & "        , UPDUSER" _
            & "        , UPDTERMID" _
            & "        , RECEIVEYMD" _
            & "        , MYWEIGHT" _
            & "        , AUTOEXTENTIONNAME" _
            & "        , BIGOILCODE" _
            & "        , BIGOILNAME" _
            & "        , JXTGTAGCODE1" _
            & "        , JXTGTAGNAME1" _
            & "        , JXTGTAGCODE2" _
            & "        , JXTGTAGNAME2" _
            & "        , JXTGTAGCODE3" _
            & "        , JXTGTAGNAME3" _
            & "        , JXTGTAGCODE4" _
            & "        , JXTGTAGNAME4" _
            & "        , IDSSTAGCODE" _
            & "        , IDSSTAGNAME" _
            & "        , COSMOTAGCODE" _
            & "        , COSMOTAGNAME" _
            & "        , ALLINSPECTIONDATE" _
            & "        , PREINSPECTIONDATE" _
            & "        , OBTAINEDNAME" _
            & "        , EXCLUDEDATE" _
            & "        , RETIRMENTDATE" _
            & "        , JRTANKTYPE" _
            & "        , JXTGTANKNUMBER2" _
            & "        , JXTGTANKNUMBER3" _
            & "        , JXTGTANKNUMBER4" _
            & "        , SAPSHELLTANKNUMBER" _
            & "        , LENGTH" _
            & "        , TANKLENGTH" _
            & "        , MAXCALIBER" _
            & "        , MINCALIBER" _
            & "        , LENGTHFLG" _
            & "        , INTERINSPECTYM" _
            & "        , INTERINSPECTSTATION" _
            & "        , INTERINSPECTORGCODE" _
            & "        , SELFINSPECTYM" _
            & "        , SELFINSPECTSTATION" _
            & "        , SELFINSPECTORGCODE" _
            & "        , MIDDLEOILCODE" _
            & "        , MIDDLEOILNAME" _
            & "        , INSPECTMEMBERNAME" _
            & "        , SUBOPERATIONBASECODE" _
            & "        , SUBOPERATIONBASENAME" _
            & "        , ALLINSPECTPLANYM" _
            & "        , SUSPENDFLG" _
            & "        , SUSPENDDATE" _
            & "        , PURCHASEPRICE" _
            & "        , INTERNALCOATING" _
            & "        , SAFETYVALVE" _
            & "        , CENTERVALVEINFO" _
            & "        , DELREASONKBN)" _
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
            & "        , @P65" _
            & "        , @P58" _
            & "        , @P59" _
            & "        , @P60" _
            & "        , @P61" _
            & "        , @P62" _
            & "        , @P63" _
            & "        , @P64" _
            & "        , @P66" _
            & "        , @P67" _
            & "        , @P68" _
            & "        , @P69" _
            & "        , @P70" _
            & "        , @P71" _
            & "        , @P72" _
            & "        , @P73" _
            & "        , @P74" _
            & "        , @P75" _
            & "        , @P76" _
            & "        , @P77" _
            & "        , @P78" _
            & "        , @P79" _
            & "        , @P80" _
            & "        , @P81" _
            & "        , @P82" _
            & "        , @P83" _
            & "        , @P84" _
            & "        , @P85" _
            & "        , @P86" _
            & "        , @P87" _
            & "        , @P88" _
            & "        , @P89" _
            & "        , @P90" _
            & "        , @P91" _
            & "        , @P92" _
            & "        , @P93" _
            & "        , @P94" _
            & "        , @P95" _
            & "        , @P96" _
            & "        , @P97" _
            & "        , @P98" _
            & "        , @P99" _
            & "        , @P100" _
            & "        , @P101" _
            & "        , @P102" _
            & "        , @P103" _
            & "        , @P104" _
            & "        , @P105" _
            & "        , @P106" _
            & "        , @P107" _
            & "        , @P108" _
            & "        , @P109" _
            & "        , @P110" _
            & "        , @P111" _
            & "        , @P112" _
            & "        , @P113" _
            & "        , @P114" _
            & "        , @P115) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
              " SELECT" _
            & "     DELFLG" _
            & "     , TANKNUMBER" _
            & "     , MODEL" _
            & "     , MODELKANA" _
            & "     , LOAD" _
            & "     , LOADUNIT" _
            & "     , VOLUME" _
            & "     , VOLUMEUNIT" _
            & "     , MYWEIGHT" _
            & "     , LENGTH" _
            & "     , TANKLENGTH" _
            & "     , MAXCALIBER" _
            & "     , MINCALIBER" _
            & "     , LENGTHFLG" _
            & "     , ORIGINOWNERCODE" _
            & "     , ORIGINOWNERNAME" _
            & "     , OWNERCODE" _
            & "     , OWNERNAME" _
            & "     , LEASECODE" _
            & "     , LEASENAME" _
            & "     , LEASECLASS" _
            & "     , LEASECLASSNAME" _
            & "     , AUTOEXTENTION" _
            & "     , AUTOEXTENTIONNAME" _
            & "     , LEASESTYMD" _
            & "     , LEASEENDYMD" _
            & "     , USERCODE" _
            & "     , USERNAME" _
            & "     , CURRENTSTATIONCODE" _
            & "     , CURRENTSTATIONNAME" _
            & "     , EXTRADINARYSTATIONCODE" _
            & "     , EXTRADINARYSTATIONNAME" _
            & "     , USERLIMIT" _
            & "     , LIMITTEXTRADIARYSTATION" _
            & "     , DEDICATETYPECODE" _
            & "     , DEDICATETYPENAME" _
            & "     , EXTRADINARYTYPECODE" _
            & "     , EXTRADINARYTYPENAME" _
            & "     , EXTRADINARYLIMIT" _
            & "     , BIGOILCODE" _
            & "     , BIGOILNAME" _
            & "     , MIDDLEOILCODE" _
            & "     , MIDDLEOILNAME" _
            & "     , DOWNLOADDATE" _
            & "     , OPERATIONBASECODE" _
            & "     , OPERATIONBASENAME" _
            & "     , SUBOPERATIONBASECODE" _
            & "     , SUBOPERATIONBASENAME" _
            & "     , COLORCODE" _
            & "     , COLORNAME" _
            & "     , MARKCODE" _
            & "     , MARKNAME" _
            & "     , JXTGTAGCODE1" _
            & "     , JXTGTAGNAME1" _
            & "     , JXTGTAGCODE2" _
            & "     , JXTGTAGNAME2" _
            & "     , JXTGTAGCODE3" _
            & "     , JXTGTAGNAME3" _
            & "     , JXTGTAGCODE4" _
            & "     , JXTGTAGNAME4" _
            & "     , IDSSTAGCODE" _
            & "     , IDSSTAGNAME" _
            & "     , COSMOTAGCODE" _
            & "     , COSMOTAGNAME" _
            & "     , RESERVE1" _
            & "     , RESERVE2" _
            & "     , JRINSPECTIONDATE" _
            & "     , INSPECTIONDATE" _
            & "     , JRSPECIFIEDDATE" _
            & "     , SPECIFIEDDATE" _
            & "     , JRALLINSPECTIONDATE" _
            & "     , ALLINSPECTIONDATE" _
            & "     , PREINSPECTIONDATE" _
            & "     , GETDATE" _
            & "     , TRANSFERDATE" _
            & "     , OBTAINEDCODE" _
            & "     , OBTAINEDNAME" _
            & "     , PROGRESSYEAR" _
            & "     , NEXTPROGRESSYEAR" _
            & "     , EXCLUDEDATE" _
            & "     , RETIRMENTDATE" _
            & "     , JRTANKNUMBER" _
            & "     , JRTANKTYPE" _
            & "     , OLDTANKNUMBER" _
            & "     , OTTANKNUMBER" _
            & "     , JXTGTANKNUMBER1" _
            & "     , JXTGTANKNUMBER2" _
            & "     , JXTGTANKNUMBER3" _
            & "     , JXTGTANKNUMBER4" _
            & "     , COSMOTANKNUMBER" _
            & "     , FUJITANKNUMBER" _
            & "     , SHELLTANKNUMBER" _
            & "     , SAPSHELLTANKNUMBER" _
            & "     , RESERVE3" _
            & "     , USEDFLG" _
            & "     , INTERINSPECTYM" _
            & "     , INTERINSPECTSTATION" _
            & "     , INTERINSPECTORGCODE" _
            & "     , SELFINSPECTYM" _
            & "     , SELFINSPECTSTATION" _
            & "     , SELFINSPECTORGCODE" _
            & "     , INSPECTMEMBERNAME" _
            & "     , ALLINSPECTPLANYM" _
            & "     , SUSPENDFLG" _
            & "     , SUSPENDDATE" _
            & "     , PURCHASEPRICE" _
            & "     , INTERNALCOATING" _
            & "     , SAFETYVALVE" _
            & "     , CENTERVALVEINFO" _
            & "     , DELREASONKBN" _
            & "     , INITYMD" _
            & "     , INITUSER" _
            & "     , INITTERMID" _
            & "     , UPDYMD" _
            & "     , UPDUSER" _
            & "     , UPDTERMID" _
            & "     , RECEIVEYMD" _
            & "     , CAST(UPDTIMSTP As bigint) As UPDTIMSTP" _
            & " FROM" _
            & "     OIL.OIM0005_TANK" _
            & " WHERE" _
            & "     TANKNUMBER = @P01"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA00 As SqlParameter = SQLcmd.Parameters.Add("@P00", SqlDbType.NVarChar, 1)           '削除フラグ
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 8)           'JOT車番
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 20)          '原籍所有者C
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 20)          '名義所有者C
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 20)          'リース先C
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 20)          '請負リース区分C
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 1)           '自動延長
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.Date)                  'リース開始年月日
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.Date)                  'リース満了年月日
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", SqlDbType.NVarChar, 20)          '第三者使用者C
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 20)          '原常備駅C
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 20)          '臨時常備駅C
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.Date)                  '第三者使用期限
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.Date)                  '臨時常備駅期限
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 20)          '原専用種別C
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 20)          '臨時専用種別C
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.Date)                  '臨時専用期限
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.NVarChar, 20)          '運用基地C
                Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.NVarChar, 20)          '塗色C
                Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.NVarChar, 1)           'マークコード
                Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.NVarChar, 20)          'マーク名
                Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.Date)                  '取得年月日
                Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", SqlDbType.Date)                  '車籍編入年月日
                Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", SqlDbType.NVarChar, 2)           '取得先C
                Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", SqlDbType.NVarChar, 20)          '形式
                Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", SqlDbType.NVarChar, 10)          '形式カナ
                Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", SqlDbType.Float, 4, 1)           '荷重
                Dim PARA27 As SqlParameter = SQLcmd.Parameters.Add("@P27", SqlDbType.NVarChar, 2)           '荷重単位
                Dim PARA28 As SqlParameter = SQLcmd.Parameters.Add("@P28", SqlDbType.Float, 4, 1)           '容積
                Dim PARA29 As SqlParameter = SQLcmd.Parameters.Add("@P29", SqlDbType.NVarChar, 2)           '容積単位
                Dim PARA30 As SqlParameter = SQLcmd.Parameters.Add("@P30", SqlDbType.NVarChar, 20)          '原籍所有者
                Dim PARA31 As SqlParameter = SQLcmd.Parameters.Add("@P31", SqlDbType.NVarChar, 20)          '名義所有者
                Dim PARA32 As SqlParameter = SQLcmd.Parameters.Add("@P32", SqlDbType.NVarChar, 20)          'リース先
                Dim PARA33 As SqlParameter = SQLcmd.Parameters.Add("@P33", SqlDbType.NVarChar, 20)          '請負リース区分
                Dim PARA34 As SqlParameter = SQLcmd.Parameters.Add("@P34", SqlDbType.NVarChar, 20)          '第三者使用者
                Dim PARA35 As SqlParameter = SQLcmd.Parameters.Add("@P35", SqlDbType.NVarChar, 20)          '原常備駅
                Dim PARA36 As SqlParameter = SQLcmd.Parameters.Add("@P36", SqlDbType.NVarChar, 20)          '臨時常備駅
                Dim PARA37 As SqlParameter = SQLcmd.Parameters.Add("@P37", SqlDbType.NVarChar, 20)          '原専用種別
                Dim PARA38 As SqlParameter = SQLcmd.Parameters.Add("@P38", SqlDbType.NVarChar, 20)          '臨時専用種別
                Dim PARA39 As SqlParameter = SQLcmd.Parameters.Add("@P39", SqlDbType.NVarChar, 20)          '運用場所
                Dim PARA40 As SqlParameter = SQLcmd.Parameters.Add("@P40", SqlDbType.NVarChar, 20)          '塗色
                Dim PARA41 As SqlParameter = SQLcmd.Parameters.Add("@P41", SqlDbType.NVarChar, 20)          '予備1
                Dim PARA42 As SqlParameter = SQLcmd.Parameters.Add("@P42", SqlDbType.NVarChar, 20)          '予備2
                Dim PARA43 As SqlParameter = SQLcmd.Parameters.Add("@P43", SqlDbType.Date)                  '次回指定年月日
                Dim PARA44 As SqlParameter = SQLcmd.Parameters.Add("@P44", SqlDbType.Date)                  '次回全検年月日(JR) 
                Dim PARA45 As SqlParameter = SQLcmd.Parameters.Add("@P45", SqlDbType.Int)                   '現在経年
                Dim PARA46 As SqlParameter = SQLcmd.Parameters.Add("@P46", SqlDbType.Int)                   '次回全検時経年
                Dim PARA47 As SqlParameter = SQLcmd.Parameters.Add("@P47", SqlDbType.Date)                  '次回交検年月日(JR）
                Dim PARA48 As SqlParameter = SQLcmd.Parameters.Add("@P48", SqlDbType.Date)                  '次回交検年月日
                Dim PARA49 As SqlParameter = SQLcmd.Parameters.Add("@P49", SqlDbType.Date)                  '次回指定年月日(JR)
                Dim PARA50 As SqlParameter = SQLcmd.Parameters.Add("@P50", SqlDbType.NVarChar, 20)          'JR車番
                Dim PARA51 As SqlParameter = SQLcmd.Parameters.Add("@P51", SqlDbType.NVarChar, 20)          '旧JOT車番
                Dim PARA52 As SqlParameter = SQLcmd.Parameters.Add("@P52", SqlDbType.NVarChar, 20)          'OT車番
                Dim PARA53 As SqlParameter = SQLcmd.Parameters.Add("@P53", SqlDbType.NVarChar, 20)          'JXTG仙台車番
                Dim PARA54 As SqlParameter = SQLcmd.Parameters.Add("@P54", SqlDbType.NVarChar, 20)          'コスモ車番
                Dim PARA55 As SqlParameter = SQLcmd.Parameters.Add("@P55", SqlDbType.NVarChar, 20)          '富士石油車番
                Dim PARA56 As SqlParameter = SQLcmd.Parameters.Add("@P56", SqlDbType.NVarChar, 20)          '出光昭シ車番
                Dim PARA57 As SqlParameter = SQLcmd.Parameters.Add("@P57", SqlDbType.NVarChar, 20)          '予備
                Dim PARA65 As SqlParameter = SQLcmd.Parameters.Add("@P65", SqlDbType.NVarChar, 1)           '利用フラグ
                Dim PARA58 As SqlParameter = SQLcmd.Parameters.Add("@P58", SqlDbType.DateTime)              '登録年月日
                Dim PARA59 As SqlParameter = SQLcmd.Parameters.Add("@P59", SqlDbType.NVarChar, 20)          '登録ユーザーＩＤ
                Dim PARA60 As SqlParameter = SQLcmd.Parameters.Add("@P60", SqlDbType.NVarChar, 20)          '登録端末
                Dim PARA61 As SqlParameter = SQLcmd.Parameters.Add("@P61", SqlDbType.DateTime)              '更新年月日
                Dim PARA62 As SqlParameter = SQLcmd.Parameters.Add("@P62", SqlDbType.NVarChar, 20)          '更新ユーザーＩＤ
                Dim PARA63 As SqlParameter = SQLcmd.Parameters.Add("@P63", SqlDbType.NVarChar, 20)          '更新端末
                Dim PARA64 As SqlParameter = SQLcmd.Parameters.Add("@P64", SqlDbType.DateTime)              '集信日時
                Dim PARA66 As SqlParameter = SQLcmd.Parameters.Add("@P66", SqlDbType.Float, 4, 1)           '自重
                Dim PARA67 As SqlParameter = SQLcmd.Parameters.Add("@P67", SqlDbType.NVarChar, 20)          '自動延長名
                Dim PARA68 As SqlParameter = SQLcmd.Parameters.Add("@P68", SqlDbType.NVarChar, 1)           '油種大分類コード
                Dim PARA69 As SqlParameter = SQLcmd.Parameters.Add("@P69", SqlDbType.NVarChar, 10)          '油種大分類名
                Dim PARA70 As SqlParameter = SQLcmd.Parameters.Add("@P70", SqlDbType.NVarChar, 20)          'JXTG仙台タグコード
                Dim PARA71 As SqlParameter = SQLcmd.Parameters.Add("@P71", SqlDbType.NVarChar, 20)          'JXTG仙台タグ名
                Dim PARA72 As SqlParameter = SQLcmd.Parameters.Add("@P72", SqlDbType.NVarChar, 20)          'JXTG千葉タグコード
                Dim PARA73 As SqlParameter = SQLcmd.Parameters.Add("@P73", SqlDbType.NVarChar, 20)          'JXTG千葉タグ名
                Dim PARA74 As SqlParameter = SQLcmd.Parameters.Add("@P74", SqlDbType.NVarChar, 20)          'JXTG川崎タグコード
                Dim PARA75 As SqlParameter = SQLcmd.Parameters.Add("@P75", SqlDbType.NVarChar, 20)          'JXTG川崎タグ名
                Dim PARA76 As SqlParameter = SQLcmd.Parameters.Add("@P76", SqlDbType.NVarChar, 20)          'JXTG根岸タグコード
                Dim PARA77 As SqlParameter = SQLcmd.Parameters.Add("@P77", SqlDbType.NVarChar, 20)          'JXTG根岸タグ名
                Dim PARA78 As SqlParameter = SQLcmd.Parameters.Add("@P78", SqlDbType.NVarChar, 20)          '出光昭和シェルタグコード
                Dim PARA79 As SqlParameter = SQLcmd.Parameters.Add("@P79", SqlDbType.NVarChar, 20)          '出光昭和シェルタグ名
                Dim PARA80 As SqlParameter = SQLcmd.Parameters.Add("@P80", SqlDbType.NVarChar, 20)          'コスモタグコード
                Dim PARA81 As SqlParameter = SQLcmd.Parameters.Add("@P81", SqlDbType.NVarChar, 20)          'コスモタグ名
                Dim PARA82 As SqlParameter = SQLcmd.Parameters.Add("@P82", SqlDbType.Date)                  '次回全件年月日
                Dim PARA83 As SqlParameter = SQLcmd.Parameters.Add("@P83", SqlDbType.Date)                  '前回全件年月日
                Dim PARA84 As SqlParameter = SQLcmd.Parameters.Add("@P84", SqlDbType.Date)                  '取得年月日
                Dim PARA85 As SqlParameter = SQLcmd.Parameters.Add("@P85", SqlDbType.Date)                  '車籍除外年月日
                Dim PARA86 As SqlParameter = SQLcmd.Parameters.Add("@P86", SqlDbType.Date)                  '資産除却年月日
                Dim PARA87 As SqlParameter = SQLcmd.Parameters.Add("@P87", SqlDbType.NVarChar, 20)          'JR車種コード
                Dim PARA88 As SqlParameter = SQLcmd.Parameters.Add("@P88", SqlDbType.NVarChar, 20)          'JXTG千葉車番
                Dim PARA89 As SqlParameter = SQLcmd.Parameters.Add("@P89", SqlDbType.NVarChar, 20)          'JXTG川崎車番
                Dim PARA90 As SqlParameter = SQLcmd.Parameters.Add("@P90", SqlDbType.NVarChar, 20)          'JXTG根岸車番
                Dim PARA91 As SqlParameter = SQLcmd.Parameters.Add("@P91", SqlDbType.NVarChar, 20)          '出光昭シSAP車番
                Dim PARA92 As SqlParameter = SQLcmd.Parameters.Add("@P92", SqlDbType.Int)                   'タンク車長
                Dim PARA93 As SqlParameter = SQLcmd.Parameters.Add("@P93", SqlDbType.Int)                   'タンク車体長
                Dim PARA94 As SqlParameter = SQLcmd.Parameters.Add("@P94", SqlDbType.Int)                   '最大口径
                Dim PARA95 As SqlParameter = SQLcmd.Parameters.Add("@P95", SqlDbType.Int)                   '最大口径
                Dim PARA96 As SqlParameter = SQLcmd.Parameters.Add("@P96", SqlDbType.NVarChar, 1)           '長さフラグ
                Dim PARA97 As SqlParameter = SQLcmd.Parameters.Add("@P97", SqlDbType.Date)                  '中間点検年月
                Dim PARA98 As SqlParameter = SQLcmd.Parameters.Add("@P98", SqlDbType.NVarChar, 7)           '中間点検場所
                Dim PARA99 As SqlParameter = SQLcmd.Parameters.Add("@P99", SqlDbType.NVarChar, 7)           '中間点検実施者
                Dim PARA100 As SqlParameter = SQLcmd.Parameters.Add("@P100", SqlDbType.Date)                '自主点検年月
                Dim PARA101 As SqlParameter = SQLcmd.Parameters.Add("@P101", SqlDbType.NVarChar, 7)         '自主点検場所
                Dim PARA102 As SqlParameter = SQLcmd.Parameters.Add("@P102", SqlDbType.NVarChar, 7)         '自主点検実施者
                Dim PARA103 As SqlParameter = SQLcmd.Parameters.Add("@P103", SqlDbType.NVarChar, 1)         '油種中分類コード
                Dim PARA104 As SqlParameter = SQLcmd.Parameters.Add("@P104", SqlDbType.NVarChar, 10)        '油種中分類名
                Dim PARA105 As SqlParameter = SQLcmd.Parameters.Add("@P105", SqlDbType.NVarChar, 20)        '点検実施者(社員名)
                Dim PARA106 As SqlParameter = SQLcmd.Parameters.Add("@P106", SqlDbType.NVarChar, 20)        '運用基地C（サブ）
                Dim PARA107 As SqlParameter = SQLcmd.Parameters.Add("@P107", SqlDbType.NVarChar, 20)        '運用基地（サブ）
                Dim PARA108 As SqlParameter = SQLcmd.Parameters.Add("@P108", SqlDbType.Date)                '全検計画年月
                Dim PARA109 As SqlParameter = SQLcmd.Parameters.Add("@P109", SqlDbType.NVarChar, 1)         '休車フラグ
                Dim PARA110 As SqlParameter = SQLcmd.Parameters.Add("@P110", SqlDbType.Date)                '休車日
                Dim PARA111 As SqlParameter = SQLcmd.Parameters.Add("@P111", SqlDbType.Money)               '取得価格
                Dim PARA112 As SqlParameter = SQLcmd.Parameters.Add("@P112", SqlDbType.NVarChar, 1)         '内部塗装
                Dim PARA113 As SqlParameter = SQLcmd.Parameters.Add("@P113", SqlDbType.NVarChar, 20)        '安全弁
                Dim PARA114 As SqlParameter = SQLcmd.Parameters.Add("@P114", SqlDbType.NVarChar, 20)        'センターバルブ情報
                Dim PARA115 As SqlParameter = SQLcmd.Parameters.Add("@P115", SqlDbType.NVarChar, 1)         '削除理由区分

                Dim JPARA01 As SqlParameter = SQLcmdJnl.Parameters.Add("@P01", SqlDbType.NVarChar, 8)       'JOT車番

                For Each OIM0005row As DataRow In OIM0005tbl.Rows
                    If Trim(OIM0005row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING OrElse
                        Trim(OIM0005row("OPERATION")) = C_LIST_OPERATION_CODE.INSERTING OrElse
                        Trim(OIM0005row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED Then
                        Dim WW_DATENOW As DateTime = Date.Now

                        'DB更新
                        PARA00.Value = OIM0005row("DELFLG")
                        PARA01.Value = OIM0005row("TANKNUMBER")
                        PARA02.Value = OIM0005row("ORIGINOWNERCODE")
                        PARA03.Value = OIM0005row("OWNERCODE")
                        PARA04.Value = OIM0005row("LEASECODE")
                        PARA05.Value = OIM0005row("LEASECLASS")
                        PARA06.Value = OIM0005row("AUTOEXTENTION")
                        If OIM0005row("LEASESTYMD") <> "" Then
                            PARA07.Value = OIM0005row("LEASESTYMD")
                        Else
                            PARA07.Value = DBNull.Value
                        End If
                        If OIM0005row("LEASEENDYMD") <> "" Then
                            PARA08.Value = OIM0005row("LEASEENDYMD")
                        Else
                            PARA08.Value = DBNull.Value
                        End If
                        PARA09.Value = OIM0005row("USERCODE")
                        PARA10.Value = OIM0005row("CURRENTSTATIONCODE")
                        PARA11.Value = OIM0005row("EXTRADINARYSTATIONCODE")
                        If OIM0005row("USERLIMIT") <> "" Then
                            PARA12.Value = OIM0005row("USERLIMIT")
                        Else
                            PARA12.Value = DBNull.Value
                        End If
                        If OIM0005row("LIMITTEXTRADIARYSTATION") <> "" Then
                            PARA13.Value = OIM0005row("LIMITTEXTRADIARYSTATION")
                        Else
                            PARA13.Value = DBNull.Value
                        End If
                        PARA14.Value = OIM0005row("DEDICATETYPECODE")
                        PARA15.Value = OIM0005row("EXTRADINARYTYPECODE")
                        If OIM0005row("EXTRADINARYLIMIT") <> "" Then
                            PARA16.Value = OIM0005row("EXTRADINARYLIMIT")
                        Else
                            PARA16.Value = DBNull.Value
                        End If
                        PARA17.Value = OIM0005row("OPERATIONBASECODE")
                        PARA18.Value = OIM0005row("COLORCODE")
                        PARA19.Value = OIM0005row("MARKCODE")
                        PARA20.Value = OIM0005row("MARKNAME")
                        If OIM0005row("ALLINSPECTIONDATE") <> "" Then
                            PARA21.Value = OIM0005row("ALLINSPECTIONDATE")
                        Else
                            PARA21.Value = DBNull.Value
                        End If
                        If OIM0005row("TRANSFERDATE") <> "" Then
                            PARA22.Value = OIM0005row("TRANSFERDATE")
                        Else
                            PARA22.Value = DBNull.Value
                        End If
                        PARA23.Value = OIM0005row("OBTAINEDCODE")
                        PARA24.Value = OIM0005row("MODEL")
                        PARA25.Value = OIM0005row("MODELKANA")
                        If OIM0005row("LOAD") <> "" Then
                            PARA26.Value = OIM0005row("LOAD")
                        Else
                            PARA26.Value = "0.0"
                        End If
                        PARA27.Value = OIM0005row("LOADUNIT")
                        If OIM0005row("VOLUME") <> "" Then
                            PARA28.Value = OIM0005row("VOLUME")
                        Else
                            PARA28.Value = "0.0"
                        End If
                        PARA29.Value = OIM0005row("VOLUMEUNIT")
                        PARA30.Value = OIM0005row("ORIGINOWNERNAME")
                        PARA31.Value = OIM0005row("OWNERNAME")
                        PARA32.Value = OIM0005row("LEASENAME")
                        PARA33.Value = OIM0005row("LEASECLASSNAME")
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
                            PARA43.Value = DBNull.Value
                        End If
                        If OIM0005row("JRALLINSPECTIONDATE") <> "" Then
                            PARA44.Value = OIM0005row("JRALLINSPECTIONDATE")
                        Else
                            PARA44.Value = DBNull.Value
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
                            PARA47.Value = DBNull.Value
                        End If
                        If OIM0005row("INSPECTIONDATE") <> "" Then
                            PARA48.Value = OIM0005row("INSPECTIONDATE")
                        Else
                            PARA48.Value = DBNull.Value
                        End If
                        If OIM0005row("JRSPECIFIEDDATE") <> "" Then
                            PARA49.Value = OIM0005row("JRSPECIFIEDDATE")
                        Else
                            PARA49.Value = DBNull.Value
                        End If
                        PARA50.Value = OIM0005row("JRTANKNUMBER")
                        PARA51.Value = OIM0005row("OLDTANKNUMBER")
                        PARA52.Value = OIM0005row("OTTANKNUMBER")
                        PARA53.Value = OIM0005row("JXTGTANKNUMBER1")
                        PARA54.Value = OIM0005row("COSMOTANKNUMBER")
                        PARA55.Value = OIM0005row("FUJITANKNUMBER")
                        PARA56.Value = OIM0005row("SHELLTANKNUMBER")
                        PARA57.Value = OIM0005row("RESERVE3")
                        PARA65.Value = OIM0005row("USEDFLG")
                        PARA58.Value = WW_DATENOW
                        PARA59.Value = Master.USERID
                        PARA60.Value = Master.USERTERMID
                        PARA61.Value = WW_DATENOW
                        PARA62.Value = Master.USERID
                        PARA63.Value = Master.USERTERMID
                        PARA64.Value = C_DEFAULT_YMD
                        If OIM0005row("MYWEIGHT") <> "" Then
                            PARA66.Value = OIM0005row("MYWEIGHT")
                        Else
                            PARA66.Value = "0.0"
                        End If
                        PARA67.Value = OIM0005row("AUTOEXTENTIONNAME")
                        PARA68.Value = OIM0005row("BIGOILCODE")
                        PARA69.Value = OIM0005row("BIGOILNAME")
                        PARA70.Value = OIM0005row("JXTGTAGCODE1")
                        PARA71.Value = OIM0005row("JXTGTAGNAME1")
                        PARA72.Value = OIM0005row("JXTGTAGCODE2")
                        PARA73.Value = OIM0005row("JXTGTAGNAME2")
                        PARA74.Value = OIM0005row("JXTGTAGCODE3")
                        PARA75.Value = OIM0005row("JXTGTAGNAME3")
                        PARA76.Value = OIM0005row("JXTGTAGCODE4")
                        PARA77.Value = OIM0005row("JXTGTAGNAME4")
                        PARA78.Value = OIM0005row("IDSSTAGCODE")
                        PARA79.Value = OIM0005row("IDSSTAGNAME")
                        PARA80.Value = OIM0005row("COSMOTAGCODE")
                        PARA81.Value = OIM0005row("COSMOTAGNAME")
                        If OIM0005row("ALLINSPECTIONDATE") <> "" Then
                            PARA82.Value = OIM0005row("ALLINSPECTIONDATE")
                        Else
                            PARA82.Value = DBNull.Value
                        End If
                        If OIM0005row("PREINSPECTIONDATE") <> "" Then
                            PARA83.Value = OIM0005row("PREINSPECTIONDATE")
                        Else
                            PARA83.Value = DBNull.Value
                        End If
                        If OIM0005row("GETDATE") <> "" Then
                            PARA84.Value = OIM0005row("GETDATE")
                        Else
                            PARA84.Value = DBNull.Value
                        End If
                        If OIM0005row("EXCLUDEDATE") <> "" Then
                            PARA85.Value = OIM0005row("EXCLUDEDATE")
                        Else
                            PARA85.Value = DBNull.Value
                        End If
                        If OIM0005row("RETIRMENTDATE") <> "" Then
                            PARA86.Value = OIM0005row("RETIRMENTDATE")
                        Else
                            PARA86.Value = DBNull.Value
                        End If
                        PARA87.Value = OIM0005row("JRTANKTYPE")
                        PARA88.Value = OIM0005row("JXTGTANKNUMBER2")
                        PARA89.Value = OIM0005row("JXTGTANKNUMBER3")
                        PARA90.Value = OIM0005row("JXTGTANKNUMBER4")
                        PARA91.Value = OIM0005row("SAPSHELLTANKNUMBER")
                        If OIM0005row("LENGTH") <> "" Then
                            PARA92.Value = OIM0005row("LENGTH")
                        Else
                            PARA92.Value = "0"
                        End If
                        If OIM0005row("TANKLENGTH") <> "" Then
                            PARA93.Value = OIM0005row("TANKLENGTH")
                        Else
                            PARA93.Value = "0"
                        End If
                        If OIM0005row("MAXCALIBER") <> "" Then
                            PARA94.Value = OIM0005row("MAXCALIBER")
                        Else
                            PARA94.Value = "0"
                        End If
                        If OIM0005row("MINCALIBER") <> "" Then
                            PARA95.Value = OIM0005row("MINCALIBER")
                        Else
                            PARA95.Value = "0"
                        End If
                        PARA96.Value = OIM0005row("LENGTHFLG")
                        If OIM0005row("INTERINSPECTYM") <> "" Then
                            PARA97.Value = OIM0005row("INTERINSPECTYM") + "/01"
                        Else
                            PARA97.Value = DBNull.Value
                        End If
                        PARA98.Value = OIM0005row("INTERINSPECTSTATION")
                        PARA99.Value = OIM0005row("INTERINSPECTORGCODE")
                        If OIM0005row("SELFINSPECTYM") <> "" Then
                            PARA100.Value = OIM0005row("SELFINSPECTYM") + "/01"
                        Else
                            PARA100.Value = DBNull.Value
                        End If
                        PARA101.Value = OIM0005row("SELFINSPECTSTATION")
                        PARA102.Value = OIM0005row("SELFINSPECTORGCODE")
                        PARA103.Value = OIM0005row("MIDDLEOILCODE")
                        PARA104.Value = OIM0005row("MIDDLEOILNAME")
                        PARA105.Value = OIM0005row("INSPECTMEMBERNAME")
                        PARA106.Value = OIM0005row("SUBOPERATIONBASECODE")
                        PARA107.Value = OIM0005row("SUBOPERATIONBASENAME")
                        If OIM0005row("ALLINSPECTPLANYM") <> "" Then
                            PARA108.Value = OIM0005row("ALLINSPECTPLANYM") + "/01"
                        Else
                            PARA108.Value = DBNull.Value
                        End If
                        PARA109.Value = OIM0005row("SUSPENDFLG")
                        If OIM0005row("SUSPENDDATE") <> "" Then
                            PARA110.Value = OIM0005row("SUSPENDDATE")
                        Else
                            PARA110.Value = DBNull.Value
                        End If
                        If OIM0005row("PURCHASEPRICE") <> "" Then
                            PARA111.Value = OIM0005row("PURCHASEPRICE")
                        Else
                            PARA111.Value = 0
                        End If
                        PARA112.Value = OIM0005row("INTERNALCOATING")
                        PARA113.Value = OIM0005row("SAFETYVALVE")
                        PARA114.Value = OIM0005row("CENTERVALVEINFO")
                        PARA115.Value = OIM0005row("DELREASONKBN")

                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                        OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                        '更新ジャーナル出力
                        JPARA01.Value = OIM0005row("TANKNUMBER")

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
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
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
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
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
    ''' 戻るボタン押下時処理
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

        '長さフラグ(Filter)
        work.WF_SEL_LENGTHFLG.Text = WF_FIL_LENGTHFLG.Text

        '選択行
        work.WF_SEL_LINECNT.Text = OIM0005tbl.Rows(WW_LINECNT)("LINECNT")

        'JOT車番
        work.WF_SEL_TANKNUMBER2.Text = OIM0005tbl.Rows(WW_LINECNT)("TANKNUMBER")

        '形式
        work.WF_SEL_MODEL2.Text = OIM0005tbl.Rows(WW_LINECNT)("MODEL")

        '原籍所有者C
        work.WF_SEL_ORIGINOWNERCODE.Text = OIM0005tbl.Rows(WW_LINECNT)("ORIGINOWNERCODE")

        '名義所有者C
        work.WF_SEL_OWNERCODE.Text = OIM0005tbl.Rows(WW_LINECNT)("OWNERCODE")

        'リース先C
        work.WF_SEL_LEASECODE.Text = OIM0005tbl.Rows(WW_LINECNT)("LEASECODE")

        '請負リース区分C
        work.WF_SEL_LEASECLASS.Text = OIM0005tbl.Rows(WW_LINECNT)("LEASECLASS")

        '自動延長
        work.WF_SEL_AUTOEXTENTION.Text = OIM0005tbl.Rows(WW_LINECNT)("AUTOEXTENTION")

        'リース開始年月日
        work.WF_SEL_LEASESTYMD.Text = OIM0005tbl.Rows(WW_LINECNT)("LEASESTYMD")

        'リース満了年月日
        work.WF_SEL_LEASEENDYMD.Text = OIM0005tbl.Rows(WW_LINECNT)("LEASEENDYMD")

        '第三者使用者C
        work.WF_SEL_USERCODE.Text = OIM0005tbl.Rows(WW_LINECNT)("USERCODE")

        '原常備駅C
        work.WF_SEL_CURRENTSTATIONCODE.Text = OIM0005tbl.Rows(WW_LINECNT)("CURRENTSTATIONCODE")

        '臨時常備駅C
        work.WF_SEL_EXTRADINARYSTATIONCODE.Text = OIM0005tbl.Rows(WW_LINECNT)("EXTRADINARYSTATIONCODE")

        '第三者使用期限
        work.WF_SEL_USERLIMIT.Text = OIM0005tbl.Rows(WW_LINECNT)("USERLIMIT")

        '臨時常備駅期限
        work.WF_SEL_LIMITTEXTRADIARYSTATION.Text = OIM0005tbl.Rows(WW_LINECNT)("LIMITTEXTRADIARYSTATION")

        '原専用種別C
        work.WF_SEL_DEDICATETYPECODE.Text = OIM0005tbl.Rows(WW_LINECNT)("DEDICATETYPECODE")

        '臨時専用種別C
        work.WF_SEL_EXTRADINARYTYPECODE.Text = OIM0005tbl.Rows(WW_LINECNT)("EXTRADINARYTYPECODE")

        '臨時専用期限
        work.WF_SEL_EXTRADINARYLIMIT.Text = OIM0005tbl.Rows(WW_LINECNT)("EXTRADINARYLIMIT")

        '運用基地C
        work.WF_SEL_OPERATIONBASECODE.Text = OIM0005tbl.Rows(WW_LINECNT)("OPERATIONBASECODE")

        '運用基地C（サブ）
        work.WF_SEL_SUBOPERATIONBASECODE.Text = OIM0005tbl.Rows(WW_LINECNT)("SUBOPERATIONBASECODE")

        '塗色C
        work.WF_SEL_COLORCODE.Text = OIM0005tbl.Rows(WW_LINECNT)("COLORCODE")

        'マークコード
        work.WF_SEL_MARKCODE.Text = OIM0005tbl.Rows(WW_LINECNT)("MARKCODE")

        'マーク名
        work.WF_SEL_MARKNAME.Text = OIM0005tbl.Rows(WW_LINECNT)("MARKNAME")

        '取得年月日
        work.WF_SEL_ALLINSPECTIONDATE.Text = OIM0005tbl.Rows(WW_LINECNT)("GETDATE")

        '車籍編入年月日
        work.WF_SEL_TRANSFERDATE.Text = OIM0005tbl.Rows(WW_LINECNT)("TRANSFERDATE")

        '取得先C
        work.WF_SEL_OBTAINEDCODE.Text = OIM0005tbl.Rows(WW_LINECNT)("OBTAINEDCODE")

        '形式カナ
        work.WF_SEL_MODELKANA.Text = OIM0005tbl.Rows(WW_LINECNT)("MODELKANA")

        '荷重
        work.WF_SEL_LOAD.Text = OIM0005tbl.Rows(WW_LINECNT)("LOAD")

        '荷重単位
        work.WF_SEL_LOADUNIT.Text = OIM0005tbl.Rows(WW_LINECNT)("LOADUNIT")

        '容積
        work.WF_SEL_VOLUME.Text = OIM0005tbl.Rows(WW_LINECNT)("VOLUME")

        '容積単位
        work.WF_SEL_VOLUMEUNIT.Text = OIM0005tbl.Rows(WW_LINECNT)("VOLUMEUNIT")

        '原籍所有者
        work.WF_SEL_ORIGINOWNERNAME.Text = OIM0005tbl.Rows(WW_LINECNT)("ORIGINOWNERNAME")

        '名義所有者
        work.WF_SEL_OWNERNAME.Text = OIM0005tbl.Rows(WW_LINECNT)("OWNERNAME")

        'リース先
        work.WF_SEL_LEASENAME.Text = OIM0005tbl.Rows(WW_LINECNT)("LEASENAME")

        '請負リース区分
        work.WF_SEL_LEASECLASSNAME.Text = OIM0005tbl.Rows(WW_LINECNT)("LEASECLASSNAME")

        '第三者使用者
        work.WF_SEL_USERNAME.Text = OIM0005tbl.Rows(WW_LINECNT)("USERNAME")

        '原常備駅
        work.WF_SEL_CURRENTSTATIONNAME.Text = OIM0005tbl.Rows(WW_LINECNT)("CURRENTSTATIONNAME")

        '臨時常備駅
        work.WF_SEL_EXTRADINARYSTATIONNAME.Text = OIM0005tbl.Rows(WW_LINECNT)("EXTRADINARYSTATIONNAME")

        '原専用種別
        work.WF_SEL_DEDICATETYPENAME.Text = OIM0005tbl.Rows(WW_LINECNT)("DEDICATETYPENAME")

        '臨時専用種別
        work.WF_SEL_EXTRADINARYTYPENAME.Text = OIM0005tbl.Rows(WW_LINECNT)("EXTRADINARYTYPENAME")

        '運用場所
        work.WF_SEL_OPERATIONBASENAME.Text = OIM0005tbl.Rows(WW_LINECNT)("OPERATIONBASENAME")

        '運用場所（サブ）
        work.WF_SEL_SUBOPERATIONBASENAME.Text = OIM0005tbl.Rows(WW_LINECNT)("SUBOPERATIONBASENAME")

        '塗色
        work.WF_SEL_COLORNAME.Text = OIM0005tbl.Rows(WW_LINECNT)("COLORNAME")

        '予備1
        work.WF_SEL_RESERVE1.Text = OIM0005tbl.Rows(WW_LINECNT)("RESERVE1")

        '予備2
        work.WF_SEL_RESERVE2.Text = OIM0005tbl.Rows(WW_LINECNT)("RESERVE2")

        '次回指定年月日
        work.WF_SEL_SPECIFIEDDATE.Text = OIM0005tbl.Rows(WW_LINECNT)("SPECIFIEDDATE")

        '次回全検年月日(JR) 
        work.WF_SEL_JRALLINSPECTIONDATE.Text = OIM0005tbl.Rows(WW_LINECNT)("JRALLINSPECTIONDATE")

        '現在経年
        work.WF_SEL_PROGRESSYEAR.Text = OIM0005tbl.Rows(WW_LINECNT)("PROGRESSYEAR")

        '次回全検時経年
        work.WF_SEL_NEXTPROGRESSYEAR.Text = OIM0005tbl.Rows(WW_LINECNT)("NEXTPROGRESSYEAR")

        '次回交検年月日(JR）
        work.WF_SEL_JRINSPECTIONDATE.Text = OIM0005tbl.Rows(WW_LINECNT)("JRINSPECTIONDATE")

        '次回交検年月日
        work.WF_SEL_INSPECTIONDATE.Text = OIM0005tbl.Rows(WW_LINECNT)("INSPECTIONDATE")

        '次回指定年月日(JR)
        work.WF_SEL_JRSPECIFIEDDATE.Text = OIM0005tbl.Rows(WW_LINECNT)("JRSPECIFIEDDATE")

        'JR車番
        work.WF_SEL_JRTANKNUMBER.Text = OIM0005tbl.Rows(WW_LINECNT)("JRTANKNUMBER")

        '旧JOT車番
        work.WF_SEL_OLDTANKNUMBER.Text = OIM0005tbl.Rows(WW_LINECNT)("OLDTANKNUMBER")

        'OT車番
        work.WF_SEL_OTTANKNUMBER.Text = OIM0005tbl.Rows(WW_LINECNT)("OTTANKNUMBER")

        'JXTG仙台車番
        work.WF_SEL_JXTGTANKNUMBER1.Text = OIM0005tbl.Rows(WW_LINECNT)("JXTGTANKNUMBER1")

        'コスモ車番
        work.WF_SEL_COSMOTANKNUMBER.Text = OIM0005tbl.Rows(WW_LINECNT)("COSMOTANKNUMBER")

        '富士石油車番
        work.WF_SEL_FUJITANKNUMBER.Text = OIM0005tbl.Rows(WW_LINECNT)("FUJITANKNUMBER")

        '出光昭シ車番
        work.WF_SEL_SHELLTANKNUMBER.Text = OIM0005tbl.Rows(WW_LINECNT)("SHELLTANKNUMBER")

        '予備
        work.WF_SEL_RESERVE3.Text = OIM0005tbl.Rows(WW_LINECNT)("RESERVE3")

        '利用フラグ(登録(新規追加用))
        work.WF_SEL_USEDFLG2.Text = OIM0005tbl.Rows(WW_LINECNT)("USEDFLG")

        '削除フラグ
        work.WF_SEL_DELFLG.Text = OIM0005tbl.Rows(WW_LINECNT)("DELFLG")

        '自重
        work.WF_SEL_MYWEIGHT.Text = OIM0005tbl.Rows(WW_LINECNT)("MYWEIGHT")

        'タンク車長
        work.WF_SEL_LENGTH.Text = OIM0005tbl.Rows(WW_LINECNT)("LENGTH")

        'タンク車体長
        work.WF_SEL_TANKLENGTH.Text = OIM0005tbl.Rows(WW_LINECNT)("TANKLENGTH")

        '最大口径
        work.WF_SEL_MAXCALIBER.Text = OIM0005tbl.Rows(WW_LINECNT)("MAXCALIBER")

        '最小口径
        work.WF_SEL_MINCALIBER.Text = OIM0005tbl.Rows(WW_LINECNT)("MINCALIBER")

        '長さフラグ
        work.WF_SEL_LENGTHFLG2.Text = OIM0005tbl.Rows(WW_LINECNT)("LENGTHFLG")

        '自動延長名
        work.WF_SEL_AUTOEXTENTIONNAME.Text = OIM0005tbl.Rows(WW_LINECNT)("AUTOEXTENTIONNAME")

        '油種大分類コード
        work.WF_SEL_BIGOILCODE.Text = OIM0005tbl.Rows(WW_LINECNT)("BIGOILCODE")

        '油種大分類名
        work.WF_SEL_BIGOILNAME.Text = OIM0005tbl.Rows(WW_LINECNT)("BIGOILNAME")

        '油種中分類コード
        work.WF_SEL_MIDDLEOILCODE.Text = OIM0005tbl.Rows(WW_LINECNT)("MIDDLEOILCODE")

        '油種中分類名
        work.WF_SEL_MIDDLEOILNAME.Text = OIM0005tbl.Rows(WW_LINECNT)("MIDDLEOILNAME")

        'JXTG仙台タグコード
        work.WF_SEL_JXTGTAGCODE1.Text = OIM0005tbl.Rows(WW_LINECNT)("JXTGTAGCODE1")

        'JXTG仙台タグ名
        work.WF_SEL_JXTGTAGNAME1.Text = OIM0005tbl.Rows(WW_LINECNT)("JXTGTAGNAME1")

        'JXTG千葉タグコード
        work.WF_SEL_JXTGTAGCODE2.Text = OIM0005tbl.Rows(WW_LINECNT)("JXTGTAGCODE2")

        'JXTG千葉タグ名
        work.WF_SEL_JXTGTAGNAME2.Text = OIM0005tbl.Rows(WW_LINECNT)("JXTGTAGNAME2")

        'JXT川崎タグコード
        work.WF_SEL_JXTGTAGCODE3.Text = OIM0005tbl.Rows(WW_LINECNT)("JXTGTAGCODE3")

        'JXTG川崎タグ名
        work.WF_SEL_JXTGTAGNAME3.Text = OIM0005tbl.Rows(WW_LINECNT)("JXTGTAGNAME3")

        'JXTG根岸タグコード
        work.WF_SEL_JXTGTAGCODE4.Text = OIM0005tbl.Rows(WW_LINECNT)("JXTGTAGCODE4")

        'JXTG根岸タグ名
        work.WF_SEL_JXTGTAGNAME4.Text = OIM0005tbl.Rows(WW_LINECNT)("JXTGTAGNAME4")

        '出光昭シタグコード
        work.WF_SEL_IDSSTAGCODE.Text = OIM0005tbl.Rows(WW_LINECNT)("IDSSTAGCODE")

        '出光昭シタグ名
        work.WF_SEL_IDSSTAGNAME.Text = OIM0005tbl.Rows(WW_LINECNT)("IDSSTAGNAME")

        'コスモタグコード
        work.WF_SEL_COSMOTAGCODE.Text = OIM0005tbl.Rows(WW_LINECNT)("COSMOTAGCODE")

        'コスモタグ名
        work.WF_SEL_COSMOTAGNAME.Text = OIM0005tbl.Rows(WW_LINECNT)("COSMOTAGNAME")

        '次回全検年月日 
        work.WF_SEL_ALLINSPECTIONDATE.Text = OIM0005tbl.Rows(WW_LINECNT)("ALLINSPECTIONDATE")

        '前回全検年月日
        work.WF_SEL_PREINSPECTIONDATE.Text = OIM0005tbl.Rows(WW_LINECNT)("PREINSPECTIONDATE")

        '取得先名
        work.WF_SEL_OBTAINEDNAME.Text = OIM0005tbl.Rows(WW_LINECNT)("OBTAINEDNAME")

        '車籍除外年月日
        work.WF_SEL_EXCLUDEDATE.Text = OIM0005tbl.Rows(WW_LINECNT)("EXCLUDEDATE")

        '資産除却年月日
        work.WF_SEL_RETIRMENTDATE.Text = OIM0005tbl.Rows(WW_LINECNT)("RETIRMENTDATE")

        'JR車種コード
        work.WF_SEL_JRTANKTYPE.Text = OIM0005tbl.Rows(WW_LINECNT)("JRTANKTYPE")

        'JXTG千葉車番
        work.WF_SEL_JXTGTANKNUMBER2.Text = OIM0005tbl.Rows(WW_LINECNT)("JXTGTANKNUMBER2")

        'JXTG川崎車番
        work.WF_SEL_JXTGTANKNUMBER3.Text = OIM0005tbl.Rows(WW_LINECNT)("JXTGTANKNUMBER3")

        'JXTG根岸車番
        work.WF_SEL_JXTGTANKNUMBER4.Text = OIM0005tbl.Rows(WW_LINECNT)("JXTGTANKNUMBER4")

        '出光昭シSAP車番
        work.WF_SEL_SAPSHELLTANKNUMBER.Text = OIM0005tbl.Rows(WW_LINECNT)("SAPSHELLTANKNUMBER")

        '中間点検年月
        work.WF_SEL_INTERINSPECTYM.Text = OIM0005tbl.Rows(WW_LINECNT)("INTERINSPECTYM")

        '中間点検場所
        work.WF_SEL_INTERINSPECTSTATION.Text = OIM0005tbl.Rows(WW_LINECNT)("INTERINSPECTSTATION")

        '中間点検実施者
        work.WF_SEL_INTERINSPECTORGCODE.Text = OIM0005tbl.Rows(WW_LINECNT)("INTERINSPECTORGCODE")

        '自主点検年月
        work.WF_SEL_SELFINSPECTYM.Text = OIM0005tbl.Rows(WW_LINECNT)("SELFINSPECTYM")

        '自主点検場所
        work.WF_SEL_SELFINSPECTSTATION.Text = OIM0005tbl.Rows(WW_LINECNT)("SELFINSPECTSTATION")

        '自主点検実施者
        work.WF_SEL_SELFINSPECTORGCODE.Text = OIM0005tbl.Rows(WW_LINECNT)("SELFINSPECTORGCODE")

        '点検実施者(社員名)
        work.WF_SEL_INSPECTMEMBERNAME.Text = OIM0005tbl.Rows(WW_LINECNT)("INSPECTMEMBERNAME")

        '全検計画年月
        work.WF_SEL_ALLINSPECTPLANYM.Text = OIM0005tbl.Rows(WW_LINECNT)("ALLINSPECTPLANYM")

        '休車フラグ
        work.WF_SEL_SUSPENDFLG.Text = OIM0005tbl.Rows(WW_LINECNT)("SUSPENDFLG")

        '休車日
        work.WF_SEL_SUSPENDDATE.Text = OIM0005tbl.Rows(WW_LINECNT)("SUSPENDDATE")

        '取得価格
        work.WF_SEL_PURCHASEPRICE.Text = OIM0005tbl.Rows(WW_LINECNT)("PURCHASEPRICE")

        '内部塗装
        work.WF_SEL_INTERNALCOATING.Text = OIM0005tbl.Rows(WW_LINECNT)("INTERNALCOATING")

        '安全弁
        work.WF_SEL_SAFETYVALVE.Text = OIM0005tbl.Rows(WW_LINECNT)("SAFETYVALVE")

        'センターバルブ情報
        work.WF_SEL_CENTERVALVEINFO.Text = OIM0005tbl.Rows(WW_LINECNT)("CENTERVALVEINFO")

        '削除理由区分
        work.WF_SEL_DELREASONKBN.Text = OIM0005tbl.Rows(WW_LINECNT)("DELREASONKBN")

        '詳細画面更新メッセージ
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""

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
                Master.Output(C_MESSAGE_NO.REGISTRATION_RECORD_NOT_EXIST_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
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
            If WW_COLUMNS.IndexOf("TANKNUMBER") >= 0 Then
                For Each OIM0005row As DataRow In OIM0005tbl.Rows
                    If XLSTBLrow("TANKNUMBER") = OIM0005row("TANKNUMBER") Then
                        OIM0005INProw.ItemArray = OIM0005row.ItemArray
                        '更新種別は初期化する
                        OIM0005INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                        Exit For
                    End If
                Next
            End If

            '○ 項目セット
            'JOT車番
            If WW_COLUMNS.IndexOf("TANKNUMBER") >= 0 Then
                OIM0005INProw("TANKNUMBER") = XLSTBLrow("TANKNUMBER")
            End If

            '形式
            If WW_COLUMNS.IndexOf("MODEL") >= 0 Then
                OIM0005INProw("MODEL") = XLSTBLrow("MODEL")
            End If

            '形式カナ
            If WW_COLUMNS.IndexOf("MODELKANA") >= 0 Then
                OIM0005INProw("MODELKANA") = XLSTBLrow("MODELKANA")
            End If

            '荷重
            If WW_COLUMNS.IndexOf("LOAD") >= 0 Then
                OIM0005INProw("LOAD") = XLSTBLrow("LOAD")
            End If

            '荷重単位
            If WW_COLUMNS.IndexOf("LOADUNIT") >= 0 Then
                OIM0005INProw("LOADUNIT") = XLSTBLrow("LOADUNIT")
            End If

            '容積
            If WW_COLUMNS.IndexOf("VOLUME") >= 0 Then
                OIM0005INProw("VOLUME") = XLSTBLrow("VOLUME")
            End If

            '容積単位
            If WW_COLUMNS.IndexOf("VOLUMEUNIT") >= 0 Then
                OIM0005INProw("VOLUMEUNIT") = XLSTBLrow("VOLUMEUNIT")
            End If

            '自重
            If WW_COLUMNS.IndexOf("MYWEIGHT") >= 0 Then
                OIM0005INProw("MYWEIGHT") = XLSTBLrow("MYWEIGHT")
            End If

            'タンク車長
            If WW_COLUMNS.IndexOf("LENGTH") >= 0 Then
                OIM0005INProw("LENGTH") = XLSTBLrow("LENGTH")
            End If

            'タンク車体長
            If WW_COLUMNS.IndexOf("TANKLENGTH") >= 0 Then
                OIM0005INProw("TANKLENGTH") = XLSTBLrow("TANKLENGTH")
            End If

            '最大口径
            If WW_COLUMNS.IndexOf("MAXCALIBER") >= 0 Then
                OIM0005INProw("MAXCALIBER") = XLSTBLrow("MAXCALIBER")
            End If

            '最小口径
            If WW_COLUMNS.IndexOf("MINCALIBER") >= 0 Then
                OIM0005INProw("MINCALIBER") = XLSTBLrow("MINCALIBER")
            End If

            '長さフラグ
            If WW_COLUMNS.IndexOf("LENGTHFLG") >= 0 Then
                OIM0005INProw("LENGTHFLG") = XLSTBLrow("LENGTHFLG")
            End If

            '原籍所有者C
            If WW_COLUMNS.IndexOf("ORIGINOWNERCODE") >= 0 Then
                OIM0005INProw("ORIGINOWNERCODE") = XLSTBLrow("ORIGINOWNERCODE")

                '原籍所有者
                If Not String.IsNullOrEmpty(OIM0005INProw("ORIGINOWNERCODE")) Then
                    CODENAME_get("ORIGINOWNERCODE",
                        OIM0005INProw("ORIGINOWNERCODE"),
                        OIM0005INProw("ORIGINOWNERNAME"),
                        WW_DUMMY)
                Else
                    OIM0005INProw("ORIGINOWNERNAME") = ""
                End If
            End If

            '名義所有者C
            If WW_COLUMNS.IndexOf("OWNERCODE") >= 0 Then
                OIM0005INProw("OWNERCODE") = XLSTBLrow("OWNERCODE")

                '名義所有者
                If Not String.IsNullOrEmpty(OIM0005INProw("OWNERCODE")) Then
                    CODENAME_get("ORIGINOWNERCODE",
                        OIM0005INProw("OWNERCODE"),
                        OIM0005INProw("OWNERNAME"),
                        WW_DUMMY)
                Else
                    OIM0005INProw("OWNERNAME") = ""
                End If
            End If

            'リース先C
            If WW_COLUMNS.IndexOf("LEASECODE") >= 0 Then
                OIM0005INProw("LEASECODE") = XLSTBLrow("LEASECODE")

                'リース先
                If Not String.IsNullOrEmpty(OIM0005INProw("LEASECLASS")) Then
                    CODENAME_get("CAMPCODE",
                        OIM0005INProw("LEASECODE"),
                        OIM0005INProw("LEASENAME"),
                        WW_DUMMY)
                Else
                    OIM0005INProw("LEASENAME") = ""
                End If
            End If

            '請負リース区分C
            If WW_COLUMNS.IndexOf("LEASECLASS") >= 0 Then
                OIM0005INProw("LEASECLASS") = XLSTBLrow("LEASECLASS")

                '請負リース区分
                If Not String.IsNullOrEmpty(OIM0005INProw("LEASECLASS")) Then
                    CODENAME_get("LEASECLASS",
                        OIM0005INProw("LEASECLASS"),
                        OIM0005INProw("LEASECLASSNAME"),
                        WW_DUMMY)
                Else
                    OIM0005INProw("LEASECLASSNAME") = ""
                End If
            End If

            '自動延長
            If WW_COLUMNS.IndexOf("AUTOEXTENTION") >= 0 Then
                OIM0005INProw("AUTOEXTENTION") = XLSTBLrow("AUTOEXTENTION")

                '自動延長名
                If Not String.IsNullOrEmpty(OIM0005INProw("AUTOEXTENTION")) Then
                    CODENAME_get("AUTOEXTENTION",
                        OIM0005INProw("AUTOEXTENTION"),
                        OIM0005INProw("AUTOEXTENTIONNAME"),
                        WW_DUMMY)
                Else
                    OIM0005INProw("AUTOEXTENTIONNAME") = ""
                End If
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

                '第三者使用者
                If Not String.IsNullOrEmpty(OIM0005INProw("USERCODE")) Then
                    CODENAME_get("USERCODE",
                        OIM0005INProw("USERCODE"),
                        OIM0005INProw("USERNAME"),
                        WW_DUMMY)
                Else
                    OIM0005INProw("USERNAME") = ""
                End If
            End If

            '原常備駅C
            If WW_COLUMNS.IndexOf("CURRENTSTATIONCODE") >= 0 Then
                OIM0005INProw("CURRENTSTATIONCODE") = XLSTBLrow("CURRENTSTATIONCODE")

                '原常備駅
                If Not String.IsNullOrEmpty(OIM0005INProw("CURRENTSTATIONCODE")) Then
                    CODENAME_get("STATIONPATTERN",
                        OIM0005INProw("CURRENTSTATIONCODE"),
                        OIM0005INProw("CURRENTSTATIONNAME"),
                        WW_DUMMY)
                Else
                    OIM0005INProw("CURRENTSTATIONNAME") = ""
                End If
            End If

            '臨時常備駅C
            If WW_COLUMNS.IndexOf("EXTRADINARYSTATIONCODE") >= 0 Then
                OIM0005INProw("EXTRADINARYSTATIONCODE") = XLSTBLrow("EXTRADINARYSTATIONCODE")

                '臨時常備駅
                If Not String.IsNullOrEmpty(OIM0005INProw("EXTRADINARYSTATIONCODE")) Then
                    CODENAME_get(
                        "STATIONPATTERN",
                        OIM0005INProw("EXTRADINARYSTATIONCODE"),
                        OIM0005INProw("EXTRADINARYSTATIONNAME"),
                        WW_DUMMY)
                Else
                    OIM0005INProw("EXTRADINARYSTATIONNAME") = ""
                End If
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

                '原専用種別
                If Not String.IsNullOrEmpty(OIM0005INProw("DEDICATETYPECODE")) Then
                    CODENAME_get(
                        "DEDICATETYPECODE",
                        OIM0005INProw("DEDICATETYPECODE"),
                        OIM0005INProw("DEDICATETYPENAME"),
                        WW_DUMMY)
                Else
                    OIM0005INProw("DEDICATETYPENAME") = ""
                End If
            End If

            '臨時専用種別C
            If WW_COLUMNS.IndexOf("EXTRADINARYTYPECODE") >= 0 Then
                OIM0005INProw("EXTRADINARYTYPECODE") = XLSTBLrow("EXTRADINARYTYPECODE")

                '臨時専用種別
                If Not String.IsNullOrEmpty(OIM0005INProw("EXTRADINARYTYPECODE")) Then
                    CODENAME_get(
                        "EXTRADINARYTYPECODE",
                        OIM0005INProw("EXTRADINARYTYPECODE"),
                        OIM0005INProw("EXTRADINARYTYPENAME"),
                        WW_DUMMY)
                Else
                    OIM0005INProw("EXTRADINARYTYPECODE") = ""
                End If
            End If

            '臨時専用期限
            If WW_COLUMNS.IndexOf("EXTRADINARYLIMIT") >= 0 Then
                OIM0005INProw("EXTRADINARYLIMIT") = XLSTBLrow("EXTRADINARYLIMIT")
            End If

            '油種大分類コード
            If WW_COLUMNS.IndexOf("BIGOILCODE") >= 0 Then
                OIM0005INProw("BIGOILCODE") = XLSTBLrow("BIGOILCODE")

                '油種大分類名
                If Not String.IsNullOrEmpty(OIM0005INProw("BIGOILCODE")) Then
                    CODENAME_get(
                        "BIGOILCODE",
                        OIM0005INProw("BIGOILCODE"),
                        OIM0005INProw("BIGOILNAME"),
                        WW_DUMMY)
                Else
                    OIM0005INProw("BIGOILNAME") = ""
                End If
            End If

            '油種中分類コード
            If WW_COLUMNS.IndexOf("MIDDLEOILCODE") >= 0 Then
                OIM0005INProw("MIDDLEOILCODE") = XLSTBLrow("MIDDLEOILCODE")

                '油種中分類名
                If Not String.IsNullOrEmpty(OIM0005INProw("MIDDLEOILCODE")) Then
                    CODENAME_get(
                        "MIDDLEOILCODE",
                        OIM0005INProw("MIDDLEOILCODE"),
                        OIM0005INProw("MIDDLEOILNAME"),
                        WW_DUMMY)
                Else
                    OIM0005INProw("MIDDLEOILNAME") = ""
                End If
            End If

            '運用基地C
            If WW_COLUMNS.IndexOf("OPERATIONBASECODE") >= 0 Then
                OIM0005INProw("OPERATIONBASECODE") = XLSTBLrow("OPERATIONBASECODE")

                '運用場所
                If Not String.IsNullOrEmpty(OIM0005INProw("OPERATIONBASECODE")) Then
                    CODENAME_get(
                        "BASE",
                        OIM0005INProw("OPERATIONBASECODE"),
                        OIM0005INProw("OPERATIONBASENAME"),
                        WW_DUMMY)
                Else
                    OIM0005INProw("OPERATIONBASENAME") = ""
                End If
            End If

            '運用基地C（サブ）
            If WW_COLUMNS.IndexOf("SUBOPERATIONBASECODE") >= 0 Then
                OIM0005INProw("SUBOPERATIONBASECODE") = XLSTBLrow("SUBOPERATIONBASECODE")

                '運用場所
                If Not String.IsNullOrEmpty(OIM0005INProw("SUBOPERATIONBASECODE")) Then
                    CODENAME_get(
                        "BASE",
                        OIM0005INProw("SUBOPERATIONBASECODE"),
                        OIM0005INProw("SUBOPERATIONBASENAME"),
                        WW_DUMMY)
                Else
                    OIM0005INProw("SUBOPERATIONBASENAME") = ""
                End If
            End If

            '塗色C
            If WW_COLUMNS.IndexOf("COLORCODE") >= 0 Then
                OIM0005INProw("COLORCODE") = XLSTBLrow("COLORCODE")

                '塗色
                If Not String.IsNullOrEmpty(OIM0005INProw("COLORCODE")) Then
                    CODENAME_get(
                        "COLORCODE",
                        OIM0005INProw("COLORCODE"),
                        OIM0005INProw("COLORNAME"),
                        WW_DUMMY)
                Else
                    OIM0005INProw("COLORNAME") = ""
                End If
            End If

            'マークコード
            If WW_COLUMNS.IndexOf("MARKCODE") >= 0 Then
                OIM0005INProw("MARKCODE") = XLSTBLrow("MARKCODE")

                'マーク名
                If Not String.IsNullOrEmpty(OIM0005INProw("MARKCODE")) Then
                    CODENAME_get(
                        "MARKCODE",
                        OIM0005INProw("MARKCODE"),
                        OIM0005INProw("MARKNAME"),
                        WW_DUMMY)
                Else
                    OIM0005INProw("MARKNAME") = ""
                End If
            End If

            'JXTG仙台タグコード
            If WW_COLUMNS.IndexOf("JXTGTAGCODE1") >= 0 Then
                OIM0005INProw("JXTGTAGCODE1") = XLSTBLrow("JXTGTAGCODE1")
            End If

            'JXTG仙台タグ名
            If WW_COLUMNS.IndexOf("JXTGTAGNAME1") >= 0 Then
                OIM0005INProw("JXTGTAGNAME1") = XLSTBLrow("JXTGTAGNAME1")
            End If

            'JXTG千葉タグコード
            If WW_COLUMNS.IndexOf("JXTGTAGCODE2") >= 0 Then
                OIM0005INProw("JXTGTAGCODE2") = XLSTBLrow("JXTGTAGCODE2")

                'JXTG千葉タグ名
                If Not String.IsNullOrEmpty(OIM0005INProw("JXTGTAGCODE2")) Then
                    CODENAME_get(
                        "TAGCODE",
                        OIM0005INProw("JXTGTAGCODE2"),
                        OIM0005INProw("JXTGTAGNAME2"),
                        WW_DUMMY)
                Else
                    OIM0005INProw("JXTGTAGNAME2") = ""
                End If
            End If

            'JXTG川崎タグコード
            If WW_COLUMNS.IndexOf("JXTGTAGCODE3") >= 0 Then
                OIM0005INProw("JXTGTAGCODE3") = XLSTBLrow("JXTGTAGCODE3")
            End If

            'JXTG川崎タグ名
            If WW_COLUMNS.IndexOf("JXTGTAGNAME3") >= 0 Then
                OIM0005INProw("JXTGTAGNAME3") = XLSTBLrow("JXTGTAGNAME3")
            End If

            'JXTG根岸タグコード
            If WW_COLUMNS.IndexOf("JXTGTAGCODE4") >= 0 Then
                OIM0005INProw("JXTGTAGCODE4") = XLSTBLrow("JXTGTAGCODE4")
            End If

            'JXTG根岸タグ名
            If WW_COLUMNS.IndexOf("JXTGTAGNAME4") >= 0 Then
                OIM0005INProw("JXTGTAGNAME4") = XLSTBLrow("JXTGTAGNAME4")
            End If

            '出光昭シタグコード
            If WW_COLUMNS.IndexOf("IDSSTAGCODE") >= 0 Then
                OIM0005INProw("IDSSTAGCODE") = XLSTBLrow("IDSSTAGCODE")

                '出光昭シタグ名
                If Not String.IsNullOrEmpty(OIM0005INProw("IDSSTAGCODE")) Then
                    CODENAME_get(
                        "TAGCODE",
                        OIM0005INProw("IDSSTAGCODE"),
                        OIM0005INProw("IDSSTAGNAME"),
                        WW_DUMMY)
                Else
                    OIM0005INProw("IDSSTAGNAME") = ""
                End If
            End If

            'コスモタグコード
            If WW_COLUMNS.IndexOf("COSMOTAGCODE") >= 0 Then
                OIM0005INProw("COSMOTAGCODE") = XLSTBLrow("COSMOTAGCODE")
            End If

            'コスモタグ名
            If WW_COLUMNS.IndexOf("COSMOTAGNAME") >= 0 Then
                OIM0005INProw("COSMOTAGNAME") = XLSTBLrow("COSMOTAGNAME")
            End If

            '予備1
            If WW_COLUMNS.IndexOf("RESERVE1") >= 0 Then
                OIM0005INProw("RESERVE1") = XLSTBLrow("RESERVE1")
            End If

            '予備2
            If WW_COLUMNS.IndexOf("RESERVE2") >= 0 Then
                OIM0005INProw("RESERVE2") = XLSTBLrow("RESERVE2")
            End If

            '次回交検年月日(JR）
            If WW_COLUMNS.IndexOf("JRINSPECTIONDATE") >= 0 Then
                OIM0005INProw("JRINSPECTIONDATE") = XLSTBLrow("JRINSPECTIONDATE")
            End If

            '次回交検年月日
            If WW_COLUMNS.IndexOf("INSPECTIONDATE") >= 0 Then
                OIM0005INProw("INSPECTIONDATE") = XLSTBLrow("INSPECTIONDATE")
            End If

            '次回指定年月日(JR)
            If WW_COLUMNS.IndexOf("JRSPECIFIEDDATE") >= 0 Then
                OIM0005INProw("JRSPECIFIEDDATE") = XLSTBLrow("JRSPECIFIEDDATE")
            End If

            '次回指定年月日
            If WW_COLUMNS.IndexOf("SPECIFIEDDATE") >= 0 Then
                OIM0005INProw("SPECIFIEDDATE") = XLSTBLrow("SPECIFIEDDATE")
            End If

            '次回全検年月日(JR) 
            If WW_COLUMNS.IndexOf("JRALLINSPECTIONDATE") >= 0 Then
                OIM0005INProw("JRALLINSPECTIONDATE") = XLSTBLrow("JRALLINSPECTIONDATE")
            End If

            '次回全検年月日
            If WW_COLUMNS.IndexOf("ALLINSPECTIONDATE") >= 0 Then
                OIM0005INProw("ALLINSPECTIONDATE") = XLSTBLrow("ALLINSPECTIONDATE")
            End If

            '前回全検年月日
            If WW_COLUMNS.IndexOf("PREINSPECTIONDATE") >= 0 Then
                OIM0005INProw("PREINSPECTIONDATE") = XLSTBLrow("PREINSPECTIONDATE")
            End If

            '取得年月日
            If WW_COLUMNS.IndexOf("GETDATE") >= 0 Then
                OIM0005INProw("GETDATE") = XLSTBLrow("GETDATE")
            End If

            '車籍編入年月日
            If WW_COLUMNS.IndexOf("TRANSFERDATE") >= 0 Then
                OIM0005INProw("TRANSFERDATE") = XLSTBLrow("TRANSFERDATE")
            End If

            '取得先C
            If WW_COLUMNS.IndexOf("OBTAINEDCODE") >= 0 Then
                OIM0005INProw("OBTAINEDCODE") = XLSTBLrow("OBTAINEDCODE")

                '取得先名
                If Not String.IsNullOrEmpty(OIM0005INProw("OBTAINEDCODE")) Then
                    CODENAME_get(
                        "OBTAINEDCODE",
                        OIM0005INProw("OBTAINEDCODE"),
                        OIM0005INProw("OBTAINEDNAME"),
                        WW_DUMMY)
                Else
                    OIM0005INProw("OBTAINEDNAME") = ""
                End If
            End If

            '現在経年
            If WW_COLUMNS.IndexOf("PROGRESSYEAR") >= 0 Then
                OIM0005INProw("PROGRESSYEAR") = XLSTBLrow("PROGRESSYEAR")
            End If

            '次回全検時経年
            If WW_COLUMNS.IndexOf("NEXTPROGRESSYEAR") >= 0 Then
                OIM0005INProw("NEXTPROGRESSYEAR") = XLSTBLrow("NEXTPROGRESSYEAR")
            End If

            '車籍除外年月日
            If WW_COLUMNS.IndexOf("EXCLUDEDATE") >= 0 Then
                OIM0005INProw("EXCLUDEDATE") = XLSTBLrow("EXCLUDEDATE")
            End If

            '資産除却年月日
            If WW_COLUMNS.IndexOf("RETIRMENTDATE") >= 0 Then
                OIM0005INProw("RETIRMENTDATE") = XLSTBLrow("RETIRMENTDATE")
            End If

            'JR車番
            If WW_COLUMNS.IndexOf("JRTANKNUMBER") >= 0 Then
                OIM0005INProw("JRTANKNUMBER") = XLSTBLrow("JRTANKNUMBER")
            End If

            'JR車種コード
            If WW_COLUMNS.IndexOf("JRTANKTYPE") >= 0 Then
                OIM0005INProw("JRTANKTYPE") = XLSTBLrow("JRTANKTYPE")
            End If

            '旧JOT車番
            If WW_COLUMNS.IndexOf("OLDTANKNUMBER") >= 0 Then
                OIM0005INProw("OLDTANKNUMBER") = XLSTBLrow("OLDTANKNUMBER")
            End If

            'OT車番
            If WW_COLUMNS.IndexOf("OTTANKNUMBER") >= 0 Then
                OIM0005INProw("OTTANKNUMBER") = XLSTBLrow("OTTANKNUMBER")
            End If

            'JXTG仙台車番
            If WW_COLUMNS.IndexOf("JXTGTANKNUMBER1") >= 0 Then
                OIM0005INProw("JXTGTANKNUMBER1") = XLSTBLrow("JXTGTANKNUMBER1")
            End If

            'JXTG千葉車番
            If WW_COLUMNS.IndexOf("JXTGTANKNUMBER2") >= 0 Then
                OIM0005INProw("JXTGTANKNUMBER2") = XLSTBLrow("JXTGTANKNUMBER2")
            End If

            'JXTG川崎車番
            If WW_COLUMNS.IndexOf("JXTGTANKNUMBER3") >= 0 Then
                OIM0005INProw("JXTGTANKNUMBER3") = XLSTBLrow("JXTGTANKNUMBER3")
            End If

            'JXTG根岸車番
            If WW_COLUMNS.IndexOf("JXTGTANKNUMBER4") >= 0 Then
                OIM0005INProw("JXTGTANKNUMBER4") = XLSTBLrow("JXTGTANKNUMBER4")
            End If

            'コスモ車番
            If WW_COLUMNS.IndexOf("COSMOTANKNUMBER") >= 0 Then
                OIM0005INProw("COSMOTANKNUMBER") = XLSTBLrow("COSMOTANKNUMBER")
            End If

            '富士石油車番
            If WW_COLUMNS.IndexOf("FUJITANKNUMBER") >= 0 Then
                OIM0005INProw("FUJITANKNUMBER") = XLSTBLrow("FUJITANKNUMBER")
            End If

            '出光昭シ車番
            If WW_COLUMNS.IndexOf("SHELLTANKNUMBER") >= 0 Then
                OIM0005INProw("SHELLTANKNUMBER") = XLSTBLrow("SHELLTANKNUMBER")
            End If

            '出光昭シSAP車番
            If WW_COLUMNS.IndexOf("SAPSHELLTANKNUMBER") >= 0 Then
                OIM0005INProw("SAPSHELLTANKNUMBER") = XLSTBLrow("SAPSHELLTANKNUMBER")
            End If

            '予備
            If WW_COLUMNS.IndexOf("RESERVE3") >= 0 Then
                OIM0005INProw("RESERVE3") = XLSTBLrow("RESERVE3")
            End If

            '利用フラグ
            If WW_COLUMNS.IndexOf("USEDFLG") >= 0 Then
                OIM0005INProw("USEDFLG") = XLSTBLrow("USEDFLG")

                '利用フラグ名
                If Not String.IsNullOrEmpty(OIM0005INProw("USEDFLG")) Then
                    CODENAME_get("USEDFLG", OIM0005INProw("USEDFLG"), OIM0005INProw("USEDFLGNAME"), WW_DUMMY)
                Else
                    OIM0005INProw("USEDFLGNAME") = ""
                End If
            End If

            '中間点検年月
            If WW_COLUMNS.IndexOf("INTERINSPECTYM") >= 0 Then
                OIM0005INProw("INTERINSPECTYM") = XLSTBLrow("INTERINSPECTYM")
            End If

            '中間点検場所
            If WW_COLUMNS.IndexOf("INTERINSPECTSTATION") >= 0 Then
                OIM0005INProw("INTERINSPECTSTATION") = XLSTBLrow("INTERINSPECTSTATION")

                '中間点検場所名(駅)
                If Not String.IsNullOrEmpty(OIM0005INProw("INTERINSPECTSTATION")) Then
                    CODENAME_get("STATIONFOCUSON",
                                 OIM0005INProw("INTERINSPECTSTATION"),
                                 OIM0005INProw("INTERINSPECTSTATIONNAME"),
                                 WW_DUMMY)
                Else
                    OIM0005INProw("INTERINSPECTSTATIONNAME") = ""
                End If
            End If

            '中間点検実施者
            If WW_COLUMNS.IndexOf("INTERINSPECTORGCODE") >= 0 Then
                OIM0005INProw("INTERINSPECTORGCODE") = XLSTBLrow("INTERINSPECTORGCODE")

                '中間点検実施者名(組織)
                If Not String.IsNullOrEmpty(OIM0005INProw("INTERINSPECTORGCODE")) Then
                    CODENAME_get("ORG",
                                 OIM0005INProw("INTERINSPECTORGCODE"),
                                 OIM0005INProw("INTERINSPECTORGNAME"),
                                 WW_DUMMY)
                Else
                    OIM0005INProw("INTERINSPECTORGNAME") = ""
                End If
            End If

            '自主点検年月
            If WW_COLUMNS.IndexOf("SELFINSPECTYM") >= 0 Then
                OIM0005INProw("SELFINSPECTYM") = XLSTBLrow("SELFINSPECTYM")
            End If

            '自主点検場所
            If WW_COLUMNS.IndexOf("SELFINSPECTSTATION") >= 0 Then
                OIM0005INProw("SELFINSPECTSTATION") = XLSTBLrow("SELFINSPECTSTATION")

                '自主点検場所名(駅)
                If Not String.IsNullOrEmpty(OIM0005INProw("SELFINSPECTSTATION")) Then
                    CODENAME_get("STATIONFOCUSON",
                                 OIM0005INProw("SELFINSPECTSTATION"),
                                 OIM0005INProw("SELFINSPECTSTATIONNAME"),
                                 WW_DUMMY)
                Else
                    OIM0005INProw("SELFINSPECTSTATIONNAME") = ""
                End If
            End If

            '自主点検実施者
            If WW_COLUMNS.IndexOf("SELFINSPECTORGCODE") >= 0 Then
                OIM0005INProw("SELFINSPECTORGCODE") = XLSTBLrow("SELFINSPECTORGCODE")

                '自主点検実施者名(組織)
                If Not String.IsNullOrEmpty(OIM0005INProw("SELFINSPECTORGCODE")) Then
                    CODENAME_get("ORG",
                                 OIM0005INProw("SELFINSPECTORGCODE"),
                                 OIM0005INProw("SELFINSPECTORGNAME"),
                                 WW_DUMMY)
                Else
                    OIM0005INProw("SELFINSPECTORGNAME") = ""
                End If
            End If

            '点検実施者(社員名)
            If WW_COLUMNS.IndexOf("INSPECTMEMBERNAME") >= 0 Then
                OIM0005INProw("INSPECTMEMBERNAME") = XLSTBLrow("INSPECTMEMBERNAME")
            End If

            '全検計画年月
            If WW_COLUMNS.IndexOf("ALLINSPECTPLANYM") >= 0 Then
                OIM0005INProw("ALLINSPECTPLANYM") = XLSTBLrow("ALLINSPECTPLANYM")
            End If

            '休車フラグ
            If WW_COLUMNS.IndexOf("SUSPENDFLG") >= 0 Then
                OIM0005INProw("SUSPENDFLG") = XLSTBLrow("SUSPENDFLG")

                '休車フラグ(名)
                If Not String.IsNullOrEmpty(OIM0005INProw("SUSPENDFLG")) Then
                    CODENAME_get("SUSPENDFLG",
                                 OIM0005INProw("SUSPENDFLG"),
                                 OIM0005INProw("SUSPENDFLGNAME"),
                                 WW_DUMMY)
                Else
                    OIM0005INProw("SUSPENDFLGNAME") = ""
                End If
            End If

            '休車日
            If WW_COLUMNS.IndexOf("SUSPENDDATE") >= 0 Then
                OIM0005INProw("SUSPENDDATE") = XLSTBLrow("SUSPENDDATE")
            End If

            '取得価格
            If WW_COLUMNS.IndexOf("PURCHASEPRICE") >= 0 Then
                OIM0005INProw("PURCHASEPRICE") = XLSTBLrow("PURCHASEPRICE")
            End If

            '内部塗装
            If WW_COLUMNS.IndexOf("INTERNALCOATING") >= 0 Then
                OIM0005INProw("INTERNALCOATING") = XLSTBLrow("INTERNALCOATING")

                '内部塗装(名)
                If Not String.IsNullOrEmpty(OIM0005INProw("INTERNALCOATING")) Then
                    CODENAME_get("INTERNALCOATING",
                                 OIM0005INProw("INTERNALCOATING"),
                                 OIM0005INProw("INTERNALCOATINGNAME"),
                                 WW_DUMMY)
                Else
                    OIM0005INProw("INTERNALCOATING") = ""
                End If
            End If

            '安全弁
            If WW_COLUMNS.IndexOf("SAFETYVALVE") >= 0 Then
                OIM0005INProw("SAFETYVALVE") = XLSTBLrow("SAFETYVALVE")
            End If

            'センターバルブ情報
            If WW_COLUMNS.IndexOf("CENTERVALVEINFO") >= 0 Then
                OIM0005INProw("CENTERVALVEINFO") = XLSTBLrow("CENTERVALVEINFO")
            End If

            '削除フラグ
            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                OIM0005INProw("DELFLG") = XLSTBLrow("DELFLG")
            Else
                OIM0005INProw("DELFLG") = "0"
            End If

            '削除理由区分
            If WW_COLUMNS.IndexOf("DELREASONKBN") >= 0 Then
                OIM0005INProw("DELREASONKBN") = XLSTBLrow("DELREASONKBN")

                '削除理由区分(名)
                If Not String.IsNullOrEmpty(OIM0005INProw("DELREASONKBN")) Then
                    CODENAME_get("DELREASONKBN",
                                 OIM0005INProw("DELREASONKBN"),
                                 OIM0005INProw("DELREASONKBNNAME"),
                                 WW_DUMMY)
                Else
                    OIM0005INProw("DELREASONKBN") = ""
                End If
            End If

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
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
        End If

        '○ Close
        CS0023XLSUPLOAD.TBLDATA.Dispose()
        CS0023XLSUPLOAD.TBLDATA.Clear()

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
        Dim dateErrFlag As String = ""

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
            WW_TEXT = OIM0005INProw("DELFLG")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DELFLG", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
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
            '-----------------------------------------------------
            ' 削除フラグ＝削除の場合、削除理由区分の入力チェック
            '-----------------------------------------------------
            If OIM0005INProw("DELFLG").ToString.Equals(C_DELETE_FLG.DELETE) Then
                ' 削除理由区分（バリデーションチェック）
                WW_TEXT = OIM0005INProw("DELREASONKBN")
                Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DELREASONKBN", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If isNormal(WW_CS0024FCHECKERR) Then
                    If WW_TEXT <> "" Then
                        '値存在チェック
                        CODENAME_get("DELREASONKBN", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                        If Not isNormal(WW_RTN_SW) Then
                            WW_CheckMES1 = "・更新できないレコード(削除理由区分入力エラー)です。"
                            WW_CheckMES2 = "マスタに存在しません。"
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                            WW_LINE_ERR = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(削除理由区分入力エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            'JOT車番(バリデーションチェック)
            WW_TEXT = OIM0005INProw("TANKNUMBER")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TANKNUMBER", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JOT車番入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '原籍所有者C(バリデーションチェック)
            WW_TEXT = OIM0005INProw("ORIGINOWNERCODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ORIGINOWNERCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("ORIGINOWNERCODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(原籍所有者C入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(原籍所有者C入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '名義所有者C(バリデーションチェック)
            WW_TEXT = OIM0005INProw("OWNERCODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OWNERCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("ORIGINOWNERCODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(名義所有者C入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(名義所有者C入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'リース先C(バリデーションチェック)
            WW_TEXT = OIM0005INProw("LEASECODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LEASECODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("CAMPCODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(リース先C入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(リース先C入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '請負リース区分C(バリデーションチェック)
            WW_TEXT = OIM0005INProw("LEASECLASS")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LEASECLASS", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("LEASECLASS", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(請負リース区分C入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(請負リース区分C入力エラー)です。"
                WW_CheckMES1 = "請負リース区分C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '自動延長(バリデーションチェック)
            WW_TEXT = OIM0005INProw("AUTOEXTENTION")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "AUTOEXTENTION", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("AUTOEXTENTION", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(自動延長入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(自動延長入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'リース開始年月日(バリデーションチェック)
            WW_TEXT = OIM0005INProw("LEASESTYMD")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LEASESTYMD", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "リース開始年月日", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(リース開始年月日入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("LEASESTYMD") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(リース開始年月日入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'リース満了年月日(バリデーションチェック)
            WW_TEXT = OIM0005INProw("LEASEENDYMD")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LEASEENDYMD", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "リース満了年月日", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(リース満了年月日入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("LEASEENDYMD") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(リース満了年月日入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '第三者使用者C(バリデーションチェック)
            WW_TEXT = OIM0005INProw("USERCODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "USERCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("USERCODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(第三者使用者C入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(第三者使用者C入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '原常備駅C(バリデーションチェック)
            WW_TEXT = OIM0005INProw("CURRENTSTATIONCODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "CURRENTSTATIONCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("STATIONPATTERN", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(原常備駅C入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(原常備駅C入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '臨時常備駅C(バリデーションチェック)
            WW_TEXT = OIM0005INProw("EXTRADINARYSTATIONCODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "EXTRADINARYSTATIONCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("STATIONPATTERN", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(臨時常備駅C入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(臨時常備駅C入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '第三者使用期限(バリデーションチェック)
            WW_TEXT = OIM0005INProw("USERLIMIT")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "USERLIMIT", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "第三者使用期限", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(第三者使用期限入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("USERLIMIT") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(第三者使用期限入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '臨時常備駅期限(バリデーションチェック)
            WW_TEXT = OIM0005INProw("LIMITTEXTRADIARYSTATION")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LIMITTEXTRADIARYSTATION", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "臨時常備駅期限", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(臨時常備駅期限入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("LIMITTEXTRADIARYSTATION") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(臨時常備駅期限入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '原専用種別C(バリデーションチェック)
            WW_TEXT = OIM0005INProw("DEDICATETYPECODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DEDICATETYPECODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("DEDICATETYPECODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(原専用種別C入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(原専用種別C入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '臨時専用種別C(バリデーションチェック)
            WW_TEXT = OIM0005INProw("EXTRADINARYTYPECODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "EXTRADINARYTYPECODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("EXTRADINARYTYPECODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(臨時専用種別C入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(臨時専用種別C入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '臨時専用期限(バリデーションチェック)
            WW_TEXT = OIM0005INProw("EXTRADINARYLIMIT")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "EXTRADINARYLIMIT", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "臨時専用期限", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(臨時専用期限入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("EXTRADINARYLIMIT") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(臨時専用期限入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '運用基地C(バリデーションチェック)
            WW_TEXT = OIM0005INProw("OPERATIONBASECODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OPERATIONBASECODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("BASE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(運用基地C入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(運用基地C入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '運用基地C（サブ）(バリデーションチェック)
            WW_TEXT = OIM0005INProw("SUBOPERATIONBASECODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SUBOPERATIONBASECODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("BASE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(運用基地C（サブ）入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(運用基地C（サブ）入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '塗色C(バリデーションチェック)
            WW_TEXT = OIM0005INProw("COLORCODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "COLORCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("COLORCODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(塗色C入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(塗色C入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'マークコード(バリデーションチェック)
            WW_TEXT = OIM0005INProw("MARKCODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "MARKCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("MARKCODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(マークコード入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(マークコード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'マーク名(バリデーションチェック)
            WW_TEXT = OIM0005INProw("MARKNAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "MARKNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(マーク名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '取得年月日(バリデーションチェック)
            WW_TEXT = OIM0005INProw("ALLINSPECTIONDATE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ALLINSPECTIONDATE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "取得年月日", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(取得年月日入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("ALLINSPECTIONDATE") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(取得年月日入力エラー入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '車籍編入年月日(バリデーションチェック)
            WW_TEXT = OIM0005INProw("TRANSFERDATE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TRANSFERDATE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "車籍編入年月日", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(車籍編入年月日入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("TRANSFERDATE") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(車籍編入年月日入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '取得先C(バリデーションチェック)
            WW_TEXT = OIM0005INProw("OBTAINEDCODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OBTAINEDCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("OBTAINEDCODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(取得先C入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(取得先C入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 形式（バリデーションチェック）
            WW_TEXT = OIM0005INProw("MODEL")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "MODEL", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(形式入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 形式カナ（バリデーションチェック）
            WW_TEXT = OIM0005INProw("MODELKANA")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "MODELKANA", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(形式カナ入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 荷重（バリデーションチェック）
            WW_TEXT = OIM0005INProw("LOAD")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LOAD", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(荷重入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 荷重単位（バリデーションチェック）
            WW_TEXT = OIM0005INProw("LOADUNIT")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LOADUNIT", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("UNIT", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(荷重単位入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(荷重単位入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 容積（バリデーションチェック）
            WW_TEXT = OIM0005INProw("VOLUME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "VOLUME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(容積入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 容積単位（バリデーションチェック）
            WW_TEXT = OIM0005INProw("VOLUMEUNIT")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "VOLUMEUNIT", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("UNIT", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(容積単位入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(容積単位入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 自重（バリデーションチェック）
            WW_TEXT = OIM0005INProw("MYWEIGHT")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "MYWEIGHT", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(自重入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' タンク車長（バリデーションチェック）
            WW_TEXT = OIM0005INProw("LENGTH")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LENGTH", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(タンク車長入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' タンク車体長（バリデーションチェック）
            WW_TEXT = OIM0005INProw("TANKLENGTH")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TANKLENGTH", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(タンク車体長入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 最大口径（バリデーションチェック）
            WW_TEXT = OIM0005INProw("MAXCALIBER")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "MAXCALIBER", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(最大口径入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 最小口径（バリデーションチェック）
            WW_TEXT = OIM0005INProw("MINCALIBER")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "MINCALIBER", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(最小口径入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 長さフラグ（バリデーションチェック）
            WW_TEXT = OIM0005INProw("LENGTHFLG")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LENGTHFLG", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("LENGTHFLG", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(長さフラグ入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(長さフラグ入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 原籍所有者（バリデーションチェック）
            WW_TEXT = OIM0005INProw("ORIGINOWNERNAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ORIGINOWNERNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(原籍所有者入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 名義所有者（バリデーションチェック）
            WW_TEXT = OIM0005INProw("OWNERNAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OWNERNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(名義所有者入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' リース先（バリデーションチェック）
            WW_TEXT = OIM0005INProw("LEASENAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LEASENAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(リース先入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 請負リース区分（バリデーションチェック）
            WW_TEXT = OIM0005INProw("LEASECLASSNAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LEASECLASSNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(請負リース区分入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 自動延長名（バリデーションチェック）
            WW_TEXT = OIM0005INProw("AUTOEXTENTIONNAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "AUTOEXTENTIONNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(自動延長名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 第三者使用者（バリデーションチェック）
            WW_TEXT = OIM0005INProw("USERNAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "USERNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(第三者使用者入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 原常備駅（バリデーションチェック）
            WW_TEXT = OIM0005INProw("CURRENTSTATIONNAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "CURRENTSTATIONNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(原常備駅入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 臨時常備駅（バリデーションチェック）
            WW_TEXT = OIM0005INProw("EXTRADINARYSTATIONNAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "EXTRADINARYSTATIONNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(臨時常備駅入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 原専用種別名（バリデーションチェック）
            WW_TEXT = OIM0005INProw("DEDICATETYPENAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DEDICATETYPENAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(原専用種別入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 臨時専用種別名（バリデーションチェック）
            WW_TEXT = OIM0005INProw("EXTRADINARYTYPENAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "EXTRADINARYTYPENAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(臨時専用種別入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 油種大分類コード（バリデーションチェック）
            WW_TEXT = OIM0005INProw("BIGOILCODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "BIGOILCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("BIGOILCODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(油種大分類コード入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(油種大分類コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 油種大分類名（バリデーションチェック）
            WW_TEXT = OIM0005INProw("BIGOILNAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "BIGOILNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(油種大分類名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 油種中分類コード（バリデーションチェック）
            WW_TEXT = OIM0005INProw("MIDDLEOILCODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "MIDDLEOILCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("MIDDLEOILCODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(油種中分類コード入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(油種中分類コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 油種中分類名（バリデーションチェック）
            WW_TEXT = OIM0005INProw("MIDDLEOILNAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "MIDDLEOILNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(油種中分類名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 運用場所（バリデーションチェック）
            WW_TEXT = OIM0005INProw("OPERATIONBASENAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OPERATIONBASENAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(運用場所入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 運用場所（サブ）（バリデーションチェック）
            WW_TEXT = OIM0005INProw("SUBOPERATIONBASENAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SUBOPERATIONBASENAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(運用場所（サブ）入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 塗色名（バリデーションチェック）
            WW_TEXT = OIM0005INProw("COLORNAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "COLORNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(塗色入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JXTG仙台タグコード（バリデーションチェック）
            WW_TEXT = OIM0005INProw("JXTGTAGCODE1")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JXTGTAGCODE1", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JXTG仙台タグコード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JXTG仙台タグ名（バリデーションチェック）
            WW_TEXT = OIM0005INProw("JXTGTAGNAME1")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JXTGTAGNAME1", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JXTG仙台タグ名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JXTG千葉タグコード（バリデーションチェック）
            WW_TEXT = OIM0005INProw("JXTGTAGCODE2")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JXTGTAGCODE2", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JXTG千葉タグコード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JXTG千葉タグ名（バリデーションチェック）
            WW_TEXT = OIM0005INProw("JXTGTAGNAME2")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JXTGTAGNAME2", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JXTG千葉タグ名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JXTG川崎タグコード（バリデーションチェック）
            WW_TEXT = OIM0005INProw("JXTGTAGCODE3")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JXTGTAGCODE3", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JXTG川崎タグコード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JXTG川崎タグ名（バリデーションチェック）
            WW_TEXT = OIM0005INProw("JXTGTAGNAME3")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JXTGTAGNAME3", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JXTG川崎タグ名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JXTG根岸タグコード（バリデーションチェック）
            WW_TEXT = OIM0005INProw("JXTGTAGCODE4")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JXTGTAGCODE4", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JXTG根岸タグコード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JXTG根岸タグ名（バリデーションチェック）
            WW_TEXT = OIM0005INProw("JXTGTAGNAME4")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JXTGTAGNAME4", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JXTG根岸タグ名入力入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 出光昭シタグコード（バリデーションチェック）
            WW_TEXT = OIM0005INProw("IDSSTAGCODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "IDSSTAGCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(出光昭シタグコード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 出光昭シタグ名（バリデーションチェック）
            WW_TEXT = OIM0005INProw("IDSSTAGNAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "IDSSTAGNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(出光昭シタグ名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' コスモタグコード（バリデーションチェック）
            WW_TEXT = OIM0005INProw("COSMOTAGCODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "COSMOTAGCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(コスモタグコード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' コスモタグ名（バリデーションチェック）
            WW_TEXT = OIM0005INProw("COSMOTAGNAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "COSMOTAGNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(コスモタグ名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 予備1（バリデーションチェック）
            WW_TEXT = OIM0005INProw("RESERVE1")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "RESERVE1", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(予備1入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 予備2（バリデーションチェック）
            WW_TEXT = OIM0005INProw("RESERVE2")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "RESERVE2", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(予備2入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 次回交検年月日(JR）（バリデーションチェック）
            WW_TEXT = OIM0005INProw("JRINSPECTIONDATE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JRINSPECTIONDATE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "次回交検年月日(JR）", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(次回交検年月日(JR）入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("JRINSPECTIONDATE") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(次回交検年月日(JR）入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 次回交検年月日（バリデーションチェック）
            WW_TEXT = OIM0005INProw("INSPECTIONDATE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "INSPECTIONDATE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "次回交検年月日", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(次回交検年月日入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("INSPECTIONDATE") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(次回交検年月日入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 次回指定年月日(JR)（バリデーションチェック）
            WW_TEXT = OIM0005INProw("JRSPECIFIEDDATE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JRSPECIFIEDDATE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "次回指定年月日(JR)", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(次回指定年月日(JR)入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("JRSPECIFIEDDATE") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(次回指定年月日(JR)入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 次回指定年月日（バリデーションチェック）
            WW_TEXT = OIM0005INProw("SPECIFIEDDATE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SPECIFIEDDATE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "次回指定年月日", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(次回指定年月日入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("SPECIFIEDDATE") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(次回指定年月日入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 次回全検年月日(JR) （バリデーションチェック）
            WW_TEXT = OIM0005INProw("JRALLINSPECTIONDATE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JRALLINSPECTIONDATE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "次回全検年月日(JR)", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(次回全検年月日(JR)入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("JRALLINSPECTIONDATE") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(次回全検年月日(JR)入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 前回全検年月日（バリデーションチェック）
            WW_TEXT = OIM0005INProw("PREINSPECTIONDATE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "PREINSPECTIONDATE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "前回全検年月日", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(前回全検年月日入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("PREINSPECTIONDATE") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(前回全検年月日入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 取得年月日（バリデーションチェック）
            WW_TEXT = OIM0005INProw("GETDATE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "GETDATE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "取得年月日", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(取得年月日入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("GETDATE") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(取得年月日入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 取得先名（バリデーションチェック）
            WW_TEXT = OIM0005INProw("OBTAINEDNAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OBTAINEDNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(取得先名エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 現在経年（バリデーションチェック）
            WW_TEXT = OIM0005INProw("PROGRESSYEAR")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "PROGRESSYEAR", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(現在経年入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 次回全検時経年（バリデーションチェック）
            WW_TEXT = OIM0005INProw("NEXTPROGRESSYEAR")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "NEXTPROGRESSYEAR", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(次回全検時経年入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 車籍除外年月日（バリデーションチェック）
            WW_TEXT = OIM0005INProw("EXCLUDEDATE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "EXCLUDEDATE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "車籍除外年月日", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(車籍除外年月日入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("EXCLUDEDATE") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(車籍除外年月日入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 資産除却年月日（バリデーションチェック）
            WW_TEXT = OIM0005INProw("RETIRMENTDATE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "RETIRMENTDATE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "資産除却年月日", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(資産除却年月日入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("RETIRMENTDATE") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(資産除却年月日入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JR車番（バリデーションチェック）
            WW_TEXT = OIM0005INProw("JRTANKNUMBER")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JRTANKNUMBER", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JR車番エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JR車種コード（バリデーションチェック）
            WW_TEXT = OIM0005INProw("JRTANKTYPE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JRTANKTYPE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("JRTANKTYPE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(JR車種コード入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(JR車種コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 旧JOT車番（バリデーションチェック）
            WW_TEXT = OIM0005INProw("OLDTANKNUMBER")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OLDTANKNUMBER", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(旧JOT車番入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' OT車番（バリデーションチェック）
            WW_TEXT = OIM0005INProw("OTTANKNUMBER")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OTTANKNUMBER", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(OT車番入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JXTG仙台車番（バリデーションチェック）
            WW_TEXT = OIM0005INProw("JXTGTANKNUMBER1")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JXTGTANKNUMBER1", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JXTG仙台車番入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JXTG千葉車番（バリデーションチェック）
            WW_TEXT = OIM0005INProw("JXTGTANKNUMBER2")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JXTGTANKNUMBER2", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JXTG千葉車番入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JXTG川崎車番（バリデーションチェック）
            WW_TEXT = OIM0005INProw("JXTGTANKNUMBER3")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JXTGTANKNUMBER3", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JXTG川崎車番入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JXTG根岸車番（バリデーションチェック）
            WW_TEXT = OIM0005INProw("JXTGTANKNUMBER4")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JXTGTANKNUMBER4", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JXTG根岸車番入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' コスモ車番（バリデーションチェック）
            WW_TEXT = OIM0005INProw("COSMOTANKNUMBER")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "COSMOTANKNUMBER", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(コスモ車番入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 富士石油車番（バリデーションチェック）
            WW_TEXT = OIM0005INProw("FUJITANKNUMBER")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "FUJITANKNUMBER", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(富士石油車番入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 出光昭シ車番（バリデーションチェック）
            WW_TEXT = OIM0005INProw("SHELLTANKNUMBER")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SHELLTANKNUMBER", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(出光昭シ車番入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 出光昭シSAP車番（バリデーションチェック）
            WW_TEXT = OIM0005INProw("SAPSHELLTANKNUMBER")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SAPSHELLTANKNUMBER", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(出光昭シSAP車番入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 予備（バリデーションチェック）
            WW_TEXT = OIM0005INProw("RESERVE3")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "RESERVE3", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(予備入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 利用フラグ（バリデーションチェック）
            WW_TEXT = OIM0005INProw("USEDFLG")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "USEDFLG", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("USEDFLG", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(利用フラグ入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(利用フラグ入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 中間点検年月（バリデーションチェック）
            WW_TEXT = OIM0005INProw("INTERINSPECTYM")
            If Not String.IsNullOrEmpty(WW_TEXT) Then
                WW_TEXT = WW_TEXT + "/01"
            End If
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "INTERINSPECTYM", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "中間点検年月", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(中間点検年月入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("INTERINSPECTYM") = CDate(WW_TEXT).ToString("yyyy/MM")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(中間点検年月入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 中間点検場所（バリデーションチェック）
            WW_TEXT = OIM0005INProw("INTERINSPECTSTATION")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "INTERINSPECTSTATION", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("STATIONFOCUSON", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(中間点検場所入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(中間点検場所入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 中間点検実施者（バリデーションチェック）
            WW_TEXT = OIM0005INProw("INTERINSPECTORGCODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "INTERINSPECTORGCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("ORG", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(中間点検実施者入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(中間点検実施者入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 自主点検年月（バリデーションチェック）
            WW_TEXT = OIM0005INProw("SELFINSPECTYM")
            If Not String.IsNullOrEmpty(WW_TEXT) Then
                WW_TEXT = WW_TEXT + "/01"
            End If
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SELFINSPECTYM", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "自主点検年月", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(自主点検年月入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("SELFINSPECTYM") = CDate(WW_TEXT).ToString("yyyy/MM")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(自主点検年月入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 自主点検場所（バリデーションチェック）
            WW_TEXT = OIM0005INProw("SELFINSPECTSTATION")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SELFINSPECTSTATION", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("STATIONFOCUSON", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(自主点検場所入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(自主点検場所入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 自主点検実施者（バリデーションチェック）
            WW_TEXT = OIM0005INProw("SELFINSPECTORGCODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SELFINSPECTORGCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("ORG", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(自主点検実施者入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(自主点検実施者入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 自主点検実施者(社員名)（バリデーションチェック）
            WW_TEXT = OIM0005INProw("INSPECTMEMBERNAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "INSPECTMEMBERNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(自主点検実施者(社員名))です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 全検計画年月（バリデーションチェック）
            WW_TEXT = OIM0005INProw("ALLINSPECTPLANYM")
            If Not String.IsNullOrEmpty(WW_TEXT) Then
                WW_TEXT = WW_TEXT + "/01"
            End If
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ALLINSPECTPLANYM", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '全検計画年月
                    WW_CheckDate(WW_TEXT, "全検計画年月", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(全検計画年月入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("ALLINSPECTPLANYM") = CDate(WW_TEXT).ToString("yyyy/MM")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(全検計画年月入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 休車フラグ（バリデーションチェック）
            WW_TEXT = OIM0005INProw("SUSPENDFLG")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SUSPENDFLG", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("SUSPENDFLG", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(休車フラグ入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(休車フラグ入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 休車日（バリデーションチェック）
            WW_TEXT = OIM0005INProw("SUSPENDDATE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SUSPENDDATE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '休車日
                    WW_CheckDate(WW_TEXT, "休車日", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(休車日入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("SUSPENDDATE") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(休車日入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 取得価格（バリデーションチェック）
            WW_TEXT = OIM0005INProw("PURCHASEPRICE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "PURCHASEPRICE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(取得価格入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 内部塗装（バリデーションチェック）
            WW_TEXT = OIM0005INProw("INTERNALCOATING")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "INTERNALCOATING", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("INTERNALCOATING", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(内部塗装入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(内部塗装入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 安全弁（バリデーションチェック）
            WW_TEXT = OIM0005INProw("SAFETYVALVE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SAFETYVALVE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(安全弁入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' センターバルブ情報（バリデーションチェック）
            WW_TEXT = OIM0005INProw("CENTERVALVEINFO")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "CENTERVALVEINFO", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(センターバルブ情報入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

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
    ''' 年月日チェック
    ''' </summary>
    ''' <param name="I_DATE"></param>
    ''' <param name="I_DATENAME"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckDate(ByVal I_DATE As String, ByVal I_DATENAME As String, ByVal I_VALUE As String, ByRef dateErrFlag As String)

        dateErrFlag = "1"
        Try
            '年取得
            Dim chkLeapYear As String = I_DATE.Substring(0, 4)
            '月日を取得
            Dim getMMDD As String = I_DATE.Remove(0, I_DATE.IndexOf("/") + 1)
            '月取得
            Dim getMonth As String = getMMDD.Remove(getMMDD.IndexOf("/"))
            '日取得
            Dim getDay As String = getMMDD.Remove(0, getMMDD.IndexOf("/") + 1)

            '閏年の場合はその旨のメッセージを出力
            If Not DateTime.IsLeapYear(chkLeapYear) _
            AndAlso (getMonth = "2" OrElse getMonth = "02") AndAlso getDay = "29" Then
                Master.Output(C_MESSAGE_NO.OIL_LEAPYEAR_NOTFOUND, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
                '月と日の範囲チェック
            ElseIf getMonth >= 13 OrElse getDay >= 32 Then
                Master.Output(C_MESSAGE_NO.OIL_MONTH_DAY_OVER_ERROR, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
            Else
                'Master.Output(I_VALUE, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
                'エラーなし
                dateErrFlag = "0"
            End If
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DATE_FORMAT_ERROR, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
        End Try

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
            WW_ERR_MES &= ControlChars.NewLine & "  --> 形式 =" & OIM0005row("MODEL") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 形式カナ =" & OIM0005row("MODELKANA") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 荷重 =" & OIM0005row("LOAD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 荷重単位 =" & OIM0005row("LOADUNIT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 容積 =" & OIM0005row("VOLUME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 容積単位 =" & OIM0005row("VOLUMEUNIT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 自重 =" & OIM0005row("MYWEIGHT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> タンク車長 =" & OIM0005row("LENGTH") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> タンク車体長 =" & OIM0005row("TANKLENGTH") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 最大口径 =" & OIM0005row("MAXCALIBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 最小口径 =" & OIM0005row("MINCALIBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 長さフラグ =" & OIM0005row("LENGTHFLG") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 原籍所有者C =" & OIM0005row("ORIGINOWNERCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 原籍所有者 =" & OIM0005row("ORIGINOWNERNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 名義所有者C =" & OIM0005row("OWNERCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 名義所有者 =" & OIM0005row("OWNERNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> リース先C =" & OIM0005row("LEASECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> リース先 =" & OIM0005row("LEASENAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 請負リース区分C =" & OIM0005row("LEASECLASS") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 請負リース区分 =" & OIM0005row("LEASECLASSNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 自動延長 =" & OIM0005row("AUTOEXTENTION") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 自動延長名 =" & OIM0005row("AUTOEXTENTIONNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> リース開始年月日 =" & OIM0005row("LEASESTYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> リース満了年月日 =" & OIM0005row("LEASEENDYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 第三者使用者C =" & OIM0005row("USERCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 第三者使用者 =" & OIM0005row("USERNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 原常備駅C =" & OIM0005row("CURRENTSTATIONCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 原常備駅 =" & OIM0005row("CURRENTSTATIONNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 臨時常備駅C =" & OIM0005row("EXTRADINARYSTATIONCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 臨時常備駅 =" & OIM0005row("EXTRADINARYSTATIONNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 第三者使用期限 =" & OIM0005row("USERLIMIT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 臨時常備駅期限 =" & OIM0005row("LIMITTEXTRADIARYSTATION") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 原専用種別C =" & OIM0005row("DEDICATETYPECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 原専用種別名 =" & OIM0005row("DEDICATETYPENAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 臨時専用種別C =" & OIM0005row("EXTRADINARYTYPECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 臨時専用種別名 =" & OIM0005row("EXTRADINARYTYPENAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 臨時専用期限 =" & OIM0005row("EXTRADINARYLIMIT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種大分類コード =" & OIM0005row("BIGOILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種大分類名 =" & OIM0005row("BIGOILNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種中分類コード =" & OIM0005row("MIDDLEOILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種中分類名 =" & OIM0005row("MIDDLEOILNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 運用基地C =" & OIM0005row("OPERATIONBASECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 運用場所 =" & OIM0005row("OPERATIONBASENAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 運用基地C（サブ） =" & OIM0005row("SUBOPERATIONBASECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 運用場所（サブ） =" & OIM0005row("SUBOPERATIONBASENAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 塗色C =" & OIM0005row("COLORCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 塗色名 =" & OIM0005row("COLORNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> マークコード =" & OIM0005row("MARKCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> マーク名 =" & OIM0005row("MARKNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JXTG仙台タグコード =" & OIM0005row("JXTGTAGCODE1") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JXTG仙台タグ名 =" & OIM0005row("JXTGTAGNAME1") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JXTG千葉タグコード =" & OIM0005row("JXTGTAGCODE2") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JXTG千葉タグ名 =" & OIM0005row("JXTGTAGNAME2") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JXTG川崎タグコード =" & OIM0005row("JXTGTAGCODE3") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JXTG川崎タグ名 =" & OIM0005row("JXTGTAGNAME3") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JXTG根岸タグコード =" & OIM0005row("JXTGTAGCODE4") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JXTG根岸タグ名 =" & OIM0005row("JXTGTAGNAME4") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 出光昭シタグコード =" & OIM0005row("IDSSTAGCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 出光昭シタグ名 =" & OIM0005row("IDSSTAGNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> コスモタグコード =" & OIM0005row("COSMOTAGCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> コスモタグ名 =" & OIM0005row("COSMOTAGNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 予備1 =" & OIM0005row("RESERVE1") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 予備2 =" & OIM0005row("RESERVE2") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 次回交検年月日(JR） =" & OIM0005row("JRINSPECTIONDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 次回交検年月日 =" & OIM0005row("INSPECTIONDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 次回指定年月日(JR) =" & OIM0005row("JRSPECIFIEDDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 次回指定年月日 =" & OIM0005row("SPECIFIEDDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 次回全検年月日(JR)  =" & OIM0005row("JRALLINSPECTIONDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 次回全検年月日 =" & OIM0005row("ALLINSPECTIONDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 前回全検年月日 =" & OIM0005row("PREINSPECTIONDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 取得年月日 =" & OIM0005row("GETDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 車籍編入年月日 =" & OIM0005row("TRANSFERDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 取得先C =" & OIM0005row("OBTAINEDCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 取得先名 =" & OIM0005row("OBTAINEDNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 現在経年 =" & OIM0005row("PROGRESSYEAR") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 次回全検時経年 =" & OIM0005row("NEXTPROGRESSYEAR") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 車籍除外年月日 =" & OIM0005row("EXCLUDEDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 資産除却年月日 =" & OIM0005row("RETIRMENTDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JR車番 =" & OIM0005row("JRTANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JR車種コード =" & OIM0005row("JRTANKTYPE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 旧JOT車番 =" & OIM0005row("OLDTANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> OT車番 =" & OIM0005row("OTTANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JXTG仙台車番 =" & OIM0005row("JXTGTANKNUMBER1") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JXTG千葉車番 =" & OIM0005row("JXTGTANKNUMBER2") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JXTG川崎車番 =" & OIM0005row("JXTGTANKNUMBER3") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JXTG根岸車番 =" & OIM0005row("JXTGTANKNUMBER4") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> コスモ車番 =" & OIM0005row("COSMOTANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 富士石油車番 =" & OIM0005row("FUJITANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 出光昭シ車番 =" & OIM0005row("SHELLTANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 出光昭シSAP車番 =" & OIM0005row("SAPSHELLTANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 予備 =" & OIM0005row("RESERVE3") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 利用フラグ =" & OIM0005row("USEDFLGNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 中間点検年月 =" & OIM0005row("INTERINSPECTYM") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 中間点検場所 =" & OIM0005row("INTERINSPECTSTATION") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 中間点検実施者 =" & OIM0005row("INTERINSPECTORGCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 自主点検年月 =" & OIM0005row("SELFINSPECTYM") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 自主点検場所 =" & OIM0005row("SELFINSPECTSTATION") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 自主点検実施者 =" & OIM0005row("SELFINSPECTORGCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 点検実施者(社員名) =" & OIM0005row("INSPECTMEMBERNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 全検計画年月 =" & OIM0005row("ALLINSPECTPLANYM") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 休車フラグ =" & OIM0005row("SUSPENDFLG") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 休車日 =" & OIM0005row("SUSPENDDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 取得価格 =" & OIM0005row("PURCHASEPRICE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 内部塗装 =" & OIM0005row("INTERNALCOATING") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 安全弁 =" & OIM0005row("SAFETYVALVE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> センターバルブ情報 =" & OIM0005row("CENTERVALVEINFO") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除フラグ =" & OIM0005row("DELFLG") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除理由区分 =" & OIM0005row("DELREASONKBN") & " , "

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

            ' 既存レコードとの比較
            For Each OIM0005row As DataRow In OIM0005tbl.Rows
                ' KEY項目が等しい時
                If OIM0005row("TANKNUMBER") = OIM0005INProw("TANKNUMBER") Then
                    ' KEY項目以外の項目の差異チェック
                    If OIM0005row("MODEL") = OIM0005INProw("MODEL") AndAlso
                        OIM0005row("MODELKANA") = OIM0005INProw("MODELKANA") AndAlso
                        OIM0005row("LOAD") = OIM0005INProw("LOAD") AndAlso
                        OIM0005row("LOADUNIT") = OIM0005INProw("LOADUNIT") AndAlso
                        OIM0005row("VOLUME") = OIM0005INProw("VOLUME") AndAlso
                        OIM0005row("VOLUMEUNIT") = OIM0005INProw("VOLUMEUNIT") AndAlso
                        OIM0005row("MYWEIGHT") = OIM0005INProw("MYWEIGHT") AndAlso
                        OIM0005row("LENGTH") = OIM0005INProw("LENGTH") AndAlso
                        OIM0005row("TANKLENGTH") = OIM0005INProw("TANKLENGTH") AndAlso
                        OIM0005row("MAXCALIBER") = OIM0005INProw("MAXCALIBER") AndAlso
                        OIM0005row("MINCALIBER") = OIM0005INProw("MINCALIBER") AndAlso
                        OIM0005row("LENGTHFLG") = OIM0005INProw("LENGTHFLG") AndAlso
                        OIM0005row("ORIGINOWNERCODE") = OIM0005INProw("ORIGINOWNERCODE") AndAlso
                        OIM0005row("ORIGINOWNERNAME") = OIM0005INProw("ORIGINOWNERNAME") AndAlso
                        OIM0005row("OWNERCODE") = OIM0005INProw("OWNERCODE") AndAlso
                        OIM0005row("OWNERNAME") = OIM0005INProw("OWNERNAME") AndAlso
                        OIM0005row("LEASECODE") = OIM0005INProw("LEASECODE") AndAlso
                        OIM0005row("LEASENAME") = OIM0005INProw("LEASENAME") AndAlso
                        OIM0005row("LEASECLASS") = OIM0005INProw("LEASECLASS") AndAlso
                        OIM0005row("LEASECLASSNAME") = OIM0005INProw("LEASECLASSNAME") AndAlso
                        OIM0005row("AUTOEXTENTION") = OIM0005INProw("AUTOEXTENTION") AndAlso
                        OIM0005row("AUTOEXTENTIONNAME") = OIM0005INProw("AUTOEXTENTIONNAME") AndAlso
                        OIM0005row("LEASESTYMD") = OIM0005INProw("LEASESTYMD") AndAlso
                        OIM0005row("LEASEENDYMD") = OIM0005INProw("LEASEENDYMD") AndAlso
                        OIM0005row("USERCODE") = OIM0005INProw("USERCODE") AndAlso
                        OIM0005row("USERNAME") = OIM0005INProw("USERNAME") AndAlso
                        OIM0005row("CURRENTSTATIONCODE") = OIM0005INProw("CURRENTSTATIONCODE") AndAlso
                        OIM0005row("CURRENTSTATIONNAME") = OIM0005INProw("CURRENTSTATIONNAME") AndAlso
                        OIM0005row("EXTRADINARYSTATIONCODE") = OIM0005INProw("EXTRADINARYSTATIONCODE") AndAlso
                        OIM0005row("EXTRADINARYSTATIONNAME") = OIM0005INProw("EXTRADINARYSTATIONNAME") AndAlso
                        OIM0005row("USERLIMIT") = OIM0005INProw("USERLIMIT") AndAlso
                        OIM0005row("LIMITTEXTRADIARYSTATION") = OIM0005INProw("LIMITTEXTRADIARYSTATION") AndAlso
                        OIM0005row("DEDICATETYPECODE") = OIM0005INProw("DEDICATETYPECODE") AndAlso
                        OIM0005row("DEDICATETYPENAME") = OIM0005INProw("DEDICATETYPENAME") AndAlso
                        OIM0005row("EXTRADINARYTYPECODE") = OIM0005INProw("EXTRADINARYTYPECODE") AndAlso
                        OIM0005row("EXTRADINARYTYPENAME") = OIM0005INProw("EXTRADINARYTYPENAME") AndAlso
                        OIM0005row("EXTRADINARYLIMIT") = OIM0005INProw("EXTRADINARYLIMIT") AndAlso
                        OIM0005row("BIGOILCODE") = OIM0005INProw("BIGOILCODE") AndAlso
                        OIM0005row("BIGOILNAME") = OIM0005INProw("BIGOILNAME") AndAlso
                        OIM0005row("MIDDLEOILCODE") = OIM0005INProw("MIDDLEOILCODE") AndAlso
                        OIM0005row("MIDDLEOILNAME") = OIM0005INProw("MIDDLEOILNAME") AndAlso
                        OIM0005row("OPERATIONBASECODE") = OIM0005INProw("OPERATIONBASECODE") AndAlso
                        OIM0005row("OPERATIONBASENAME") = OIM0005INProw("OPERATIONBASENAME") AndAlso
                        OIM0005row("SUBOPERATIONBASECODE") = OIM0005INProw("SUBOPERATIONBASECODE") AndAlso
                        OIM0005row("SUBOPERATIONBASENAME") = OIM0005INProw("SUBOPERATIONBASENAME") AndAlso
                        OIM0005row("COLORCODE") = OIM0005INProw("COLORCODE") AndAlso
                        OIM0005row("COLORNAME") = OIM0005INProw("COLORNAME") AndAlso
                        OIM0005row("MARKCODE") = OIM0005INProw("MARKCODE") AndAlso
                        OIM0005row("MARKNAME") = OIM0005INProw("MARKNAME") AndAlso
                        OIM0005row("JXTGTAGCODE1") = OIM0005INProw("JXTGTAGCODE1") AndAlso
                        OIM0005row("JXTGTAGNAME1") = OIM0005INProw("JXTGTAGNAME1") AndAlso
                        OIM0005row("JXTGTAGCODE2") = OIM0005INProw("JXTGTAGCODE2") AndAlso
                        OIM0005row("JXTGTAGNAME2") = OIM0005INProw("JXTGTAGNAME2") AndAlso
                        OIM0005row("JXTGTAGCODE3") = OIM0005INProw("JXTGTAGCODE3") AndAlso
                        OIM0005row("JXTGTAGNAME3") = OIM0005INProw("JXTGTAGNAME3") AndAlso
                        OIM0005row("JXTGTAGCODE4") = OIM0005INProw("JXTGTAGCODE4") AndAlso
                        OIM0005row("JXTGTAGNAME4") = OIM0005INProw("JXTGTAGNAME4") AndAlso
                        OIM0005row("IDSSTAGCODE") = OIM0005INProw("IDSSTAGCODE") AndAlso
                        OIM0005row("IDSSTAGNAME") = OIM0005INProw("IDSSTAGNAME") AndAlso
                        OIM0005row("COSMOTAGCODE") = OIM0005INProw("COSMOTAGCODE") AndAlso
                        OIM0005row("COSMOTAGNAME") = OIM0005INProw("COSMOTAGNAME") AndAlso
                        OIM0005row("RESERVE1") = OIM0005INProw("RESERVE1") AndAlso
                        OIM0005row("RESERVE2") = OIM0005INProw("RESERVE2") AndAlso
                        OIM0005row("JRINSPECTIONDATE") = OIM0005INProw("JRINSPECTIONDATE") AndAlso
                        OIM0005row("INSPECTIONDATE") = OIM0005INProw("INSPECTIONDATE") AndAlso
                        OIM0005row("JRSPECIFIEDDATE") = OIM0005INProw("JRSPECIFIEDDATE") AndAlso
                        OIM0005row("SPECIFIEDDATE") = OIM0005INProw("SPECIFIEDDATE") AndAlso
                        OIM0005row("JRALLINSPECTIONDATE") = OIM0005INProw("JRALLINSPECTIONDATE") AndAlso
                        OIM0005row("ALLINSPECTIONDATE") = OIM0005INProw("ALLINSPECTIONDATE") AndAlso
                        OIM0005row("PREINSPECTIONDATE") = OIM0005INProw("PREINSPECTIONDATE") AndAlso
                        OIM0005row("GETDATE") = OIM0005INProw("GETDATE") AndAlso
                        OIM0005row("TRANSFERDATE") = OIM0005INProw("TRANSFERDATE") AndAlso
                        OIM0005row("OBTAINEDCODE") = OIM0005INProw("OBTAINEDCODE") AndAlso
                        OIM0005row("OBTAINEDNAME") = OIM0005INProw("OBTAINEDNAME") AndAlso
                        OIM0005row("PROGRESSYEAR") = OIM0005INProw("PROGRESSYEAR") AndAlso
                        OIM0005row("NEXTPROGRESSYEAR") = OIM0005INProw("NEXTPROGRESSYEAR") AndAlso
                        OIM0005row("EXCLUDEDATE") = OIM0005INProw("EXCLUDEDATE") AndAlso
                        OIM0005row("RETIRMENTDATE") = OIM0005INProw("RETIRMENTDATE") AndAlso
                        OIM0005row("JRTANKNUMBER") = OIM0005INProw("JRTANKNUMBER") AndAlso
                        OIM0005row("JRTANKTYPE") = OIM0005INProw("JRTANKTYPE") AndAlso
                        OIM0005row("OLDTANKNUMBER") = OIM0005INProw("OLDTANKNUMBER") AndAlso
                        OIM0005row("OTTANKNUMBER") = OIM0005INProw("OTTANKNUMBER") AndAlso
                        OIM0005row("JXTGTANKNUMBER1") = OIM0005INProw("JXTGTANKNUMBER1") AndAlso
                        OIM0005row("JXTGTANKNUMBER2") = OIM0005INProw("JXTGTANKNUMBER2") AndAlso
                        OIM0005row("JXTGTANKNUMBER3") = OIM0005INProw("JXTGTANKNUMBER3") AndAlso
                        OIM0005row("JXTGTANKNUMBER4") = OIM0005INProw("JXTGTANKNUMBER4") AndAlso
                        OIM0005row("COSMOTANKNUMBER") = OIM0005INProw("COSMOTANKNUMBER") AndAlso
                        OIM0005row("FUJITANKNUMBER") = OIM0005INProw("FUJITANKNUMBER") AndAlso
                        OIM0005row("SHELLTANKNUMBER") = OIM0005INProw("SHELLTANKNUMBER") AndAlso
                        OIM0005row("SAPSHELLTANKNUMBER") = OIM0005INProw("SAPSHELLTANKNUMBER") AndAlso
                        OIM0005row("RESERVE3") = OIM0005INProw("RESERVE3") AndAlso
                        OIM0005row("USEDFLG") = OIM0005INProw("USEDFLG") AndAlso
                        OIM0005row("INTERINSPECTYM") = OIM0005INProw("INTERINSPECTYM") AndAlso
                        OIM0005row("INTERINSPECTSTATION") = OIM0005INProw("INTERINSPECTSTATION") AndAlso
                        OIM0005row("INTERINSPECTORGCODE") = OIM0005INProw("INTERINSPECTORGCODE") AndAlso
                        OIM0005row("SELFINSPECTYM") = OIM0005INProw("SELFINSPECTYM") AndAlso
                        OIM0005row("SELFINSPECTSTATION") = OIM0005INProw("SELFINSPECTSTATION") AndAlso
                        OIM0005row("SELFINSPECTORGCODE") = OIM0005INProw("SELFINSPECTORGCODE") AndAlso
                        OIM0005row("INSPECTMEMBERNAME") = OIM0005INProw("INSPECTMEMBERNAME") AndAlso
                        OIM0005row("ALLINSPECTPLANYM") = OIM0005INProw("ALLINSPECTPLANYM") AndAlso
                        OIM0005row("SUSPENDFLG") = OIM0005INProw("SUSPENDFLG") AndAlso
                        OIM0005row("SUSPENDDATE") = OIM0005INProw("SUSPENDDATE") AndAlso
                        OIM0005row("PURCHASEPRICE") = OIM0005INProw("PURCHASEPRICE") AndAlso
                        OIM0005row("INTERNALCOATING") = OIM0005INProw("INTERNALCOATING") AndAlso
                        OIM0005row("SAFETYVALVE") = OIM0005INProw("SAFETYVALVE") AndAlso
                        OIM0005row("CENTERVALVEINFO") = OIM0005INProw("CENTERVALVEINFO") AndAlso
                        OIM0005row("DELFLG") = OIM0005INProw("DELFLG") AndAlso
                        OIM0005row("DELREASONKBN") = OIM0005INProw("DELREASONKBN") Then
                        ' 変更がないときは「操作」の項目は空白にする
                        OIM0005INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Else
                        ' 変更がある時は「操作」の項目を「更新」に設定する
                        OIM0005INProw("OPERATION") = CONST_UPDATE
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
            prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

            Select Case I_FIELD
                Case "CAMPCODE"                     '会社コード
                    prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0001CompList.LC_COMPANY_TYPE.ALL
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "ORG"                          '運用部署
                    prmData = work.CreateORGParam(Master.USERCAMP)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "UNIT"                         '荷重単位, 容積単位
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_UNIT, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "ORIGINOWNERCODE"              '原籍所有者C
                    prmData = work.CreateOriginOwnercodeParam(work.WF_SEL_CAMPCODE.Text, I_VALUE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORIGINOWNERCODE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "LEASECLASS"                   '請負リース区分C
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_LEASECLASS, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "AUTOEXTENTION"                '自動延長
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "AUTOEXTENTION")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "USERCODE"                     '第三者使用者C
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_THIRDUSER, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STATIONPATTERN"　             '原常備駅C、臨時常備駅C
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATIONCODE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "DEDICATETYPECODE"             '原専用種別C
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DEDICATETYPE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "EXTRADINARYTYPECODE"          '臨時専用種別C
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRADINARYTYPE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "BIGOILCODE"                   '油種大分類コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_BIGOILCODE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "MIDDLEOILCODE"                '油種中分類コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_MIDDLEOILCODE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "BASE"                         '運用基地
                    prmData = work.CreateBaseParam(work.WF_SEL_CAMPCODE.Text, I_VALUE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_BASE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "COLORCODE"                    '塗色C
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COLOR, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "MARKCODE"                     'マークコード
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "MARKCODE")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "TAGCODE"                      'タグコード
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "TAGCODE")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "OBTAINEDCODE"                 '取得先C
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_OBTAINED, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "JRTANKTYPE"                   'JR車種コード
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "JRTANKTYPE")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "LENGTHFLG"                    '長さフラグ
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "LENGTHFLG")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "USEDFLG"                      '利用フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_USEPROPRIETY, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "DELFLG"                       '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STATIONFOCUSON"　             '中間点検場所、自主点検場所(使用駅のみ)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATIONCODE_FOCUSON, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "SUSPENDFLG"                   '休車フラグ
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "SUSPENDFLG")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "INTERNALCOATING"              '内部塗装
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "INTERNALCOATING")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "DELREASONKBN"                 '削除理由区分
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DELREASONKBN")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
