﻿''************************************************************
' 列車マスタメンテ登録・更新画面
' 作成日 2020/09/04
' 更新日 2021/04/15
' 作成者 JOT常井
' 更新者 JOT伊草
'
' 修正履歴:2020/09/04 新規作成
'         :2021/04/14 1)表更新→更新、クリア→戻る、に名称変更
'                     2)戻るボタン押下時、確認ダイアログ表示→
'                       確認ダイアログでOK押下時、一覧画面に戻るように修正
'                     3)更新ボタン押下時、この画面でDB更新→
'                       一覧画面の表示データに更新後の内容反映して戻るように修正
'         :2021/04/15 新規登録を行った際に、一覧画面に新規登録データが追加されないバグに対応
''************************************************************
Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' 列車マスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class OIM0007TrainCreate
    Inherits Page

    '○ 検索結果格納Table
    Private OIM0007tbl As DataTable                                  '一覧格納用テーブル
    Private OIM0007INPtbl As DataTable                               'チェック用テーブル
    Private OIM0007UPDtbl As DataTable                               '更新用テーブル

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
                    Master.RecoverTable(OIM0007tbl, work.WF_SEL_INPTBL.Text)

                    Select Case WF_ButtonClick.Value
                        Case "WF_UPDATE"                '更新ボタン押下
                            WF_UPDATE_Click()
                        Case "WF_CLEAR"                 '戻るボタン押下
                            WF_CLEAR_Click()
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
                        Case "btnClearConfirmOk"        '戻るボタン押下後の確認ダイアログでOK押下
                            WF_CLEAR_ConfirmOkClick()
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
            If Not IsNothing(OIM0007tbl) Then
                OIM0007tbl.Clear()
                OIM0007tbl.Dispose()
                OIM0007tbl = Nothing
            End If

            If Not IsNothing(OIM0007INPtbl) Then
                OIM0007INPtbl.Clear()
                OIM0007INPtbl.Dispose()
                OIM0007INPtbl = Nothing
            End If

            If Not IsNothing(OIM0007UPDtbl) Then
                OIM0007UPDtbl.Clear()
                OIM0007UPDtbl.Dispose()
                OIM0007UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIM0007WRKINC.MAPIDC
        '○HELP表示有無設定
        Master.dispHelp = False
        '○D&D有無設定
        Master.eventDrop = True

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

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0007L Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        End If

        '○ 名称設定処理
        '選択行
        WF_SEL_LINECNT.Text = work.WF_SEL_LINECNT.Text

        '管轄受注営業所
        WF_OFFICECODE.Text = work.WF_SEL_OFFICECODE2.Text
        CODENAME_get("OFFICECODE", WF_OFFICECODE.Text, WF_OFFICECODE_TEXT.Text, WW_DUMMY)

        '本線列車番号
        WF_TRAINNO.Text = work.WF_SEL_TRAINNO2.Text

        '本線列車番号名
        WF_TRAINNAME.Text = work.WF_SEL_TRAINNAME.Text

        '積置フラグ
        WF_TSUMI.Text = work.WF_SEL_TSUMI2.Text
        CODENAME_get("TSUMI", WF_TSUMI.Text, WF_TSUMI_TEXT.Text, WW_DUMMY)

        'OT列車番号
        WF_OTTRAINNO.Text = work.WF_SEL_OTTRAINNO.Text

        'OT発送日報送信フラグ
        WF_OTFLG.Text = work.WF_SEL_OTFLG.Text
        CODENAME_get("OTFLG", WF_OTFLG.Text, WF_OTFLG_TEXT.Text, WW_RTN_SW)

        '発駅コード
        WF_DEPSTATION.Text = work.WF_SEL_DEPSTATION.Text
        CODENAME_get("STATION", WF_DEPSTATION.Text, WF_DEPSTATION_TEXT.Text, WW_DUMMY)

        '着駅コード
        WF_ARRSTATION.Text = work.WF_SEL_ARRSTATION.Text
        CODENAME_get("STATION", WF_ARRSTATION.Text, WF_ARRSTATION_TEXT.Text, WW_DUMMY)

        'JR発列車番号
        WF_JRTRAINNO1.Text = work.WF_SEL_JRTRAINNO1.Text

        'JR発列車牽引車数
        WF_MAXTANK1.Text = work.WF_SEL_MAXTANK1.Text

        'JR中継列車番号
        WF_JRTRAINNO2.Text = work.WF_SEL_JRTRAINNO2.Text

        'JR中継列車牽引車数
        WF_MAXTANK2.Text = work.WF_SEL_MAXTANK2.Text

        'JR最終列車番号
        WF_JRTRAINNO3.Text = work.WF_SEL_JRTRAINNO3.Text

        'JR最終列車牽引車数
        WF_MAXTANK3.Text = work.WF_SEL_MAXTANK3.Text

        '列車区分
        WF_TRAINCLASS.Text = work.WF_SEL_TRAINCLASS.Text
        CODENAME_get("TRAINCLASS", WF_TRAINCLASS.Text, WF_TRAINCLASS_TEXT.Text, WW_RTN_SW)

        '高速列車区分
        WF_SPEEDCLASS.Text = work.WF_SEL_SPEEDCLASS.Text
        CODENAME_get("SPEEDCLASS", WF_SPEEDCLASS.Text, WF_SPEEDCLASS_TEXT.Text, WW_RTN_SW)

        '発送順区分
        WF_SHIPORDERCLASS.Text = work.WF_SEL_SHIPORDERCLASS.Text
        CODENAME_get("SHIPORDERCLASS", WF_SHIPORDERCLASS.Text, WF_SHIPORDERCLASS_TEXT.Text, WW_RTN_SW)

        '発日日数
        WF_DEPDAYS.Text = work.WF_SEL_DEPDAYS.Text

        '特継日数
        WF_MARGEDAYS.Text = work.WF_SEL_MARGEDAYS.Text

        '積車着日数
        WF_ARRDAYS.Text = work.WF_SEL_ARRDAYS.Text

        '受入日数
        WF_ACCDAYS.Text = work.WF_SEL_ACCDAYS.Text

        '空車着日数
        WF_EMPARRDAYS.Text = work.WF_SEL_EMPARRDAYS.Text

        '当日利用日数
        WF_USEDAYS.Text = work.WF_SEL_USEDAYS.Text
        CODENAME_get("USEDAYS", WF_USEDAYS.Text, WF_USEDAYS_TEXT.Text, WW_RTN_SW)

        '料金マスタ区分
        WF_FEEKBN.Text = work.WF_SEL_FEEKBN.Text
        CODENAME_get("FEEKBN", WF_FEEKBN.Text, WF_FEEKBN_TEXT.Text, WW_RTN_SW)

        '稼働フラグ
        WF_RUN.Text = work.WF_SEL_RUN.Text
        CODENAME_get("RUN", WF_RUN.Text, WF_RUN_TEXT.Text, WW_RTN_SW)

        '在庫管理表表示ソート区分
        WF_ZAIKOSORT.Text = work.WF_SEL_ZAIKOSORT.Text
        CODENAME_get("ZAIKOSORT", WF_ZAIKOSORT.Text, WF_ZAIKOSORT_TEXT.Text, WW_RTN_SW)

        '備考
        WF_BIKOU.Text = work.WF_SEL_BIKOU.Text

        '削除フラグ
        WF_DELFLG.Text = work.WF_SEL_DELFLG.Text
        CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_DUMMY)

    End Sub

    ''' <summary>
    ''' 一意制約チェック
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub UniqueKeyCheck(ByVal SQLcon As SqlConnection, ByRef O_MESSAGENO As String)

        '○ 対象データ取得
        Dim SQLStrBldr As New StringBuilder
        SQLStrBldr.AppendLine(" SELECT ")
        SQLStrBldr.AppendLine("       0                                               AS LINECNT ")          ' 行番号
        SQLStrBldr.AppendLine("     , ''                                              AS OPERATION ")        ' 編集
        SQLStrBldr.AppendLine("     , CAST(OIM0007.UPDTIMSTP AS bigint)               AS UPDTIMSTP ")        ' タイムスタンプ
        SQLStrBldr.AppendLine("     , 1                                               AS 'SELECT' ")         ' 選択
        SQLStrBldr.AppendLine("     , 0                                               AS HIDDEN ")           ' 非表示
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.OFFICECODE), '')           AS OFFICECODE ")       ' 管轄受注営業所
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.TRAINNO), '')              AS TRAINNO ")          ' 本線列車番号
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.TRAINNAME), '')            AS TRAINNAME ")        ' 本線列車番号名
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.TSUMI), '')                AS TSUMI ")            ' 積置フラグ
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.OTTRAINNO), '')            AS OTTRAINNO ")        ' OT列車番号
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.OTFLG), '')                AS OTFLG ")            ' OT発送日報送信フラグ
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.DEPSTATION), '')           AS DEPSTATION ")       ' 発駅コード
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.ARRSTATION), '')           AS ARRSTATION ")       ' 着駅コード
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.JRTRAINNO1), '')           AS JRTRAINNO1 ")       ' JR発列車番号
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.MAXTANK1), '')             AS MAXTANK1 ")         ' JR発列車牽引車数
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.JRTRAINNO2), '')           AS JRTRAINNO2 ")       ' JR中継列車番号
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.MAXTANK2), '')             AS MAXTANK2 ")         ' JR中継列車牽引車数
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.JRTRAINNO3), '')           AS JRTRAINNO3 ")       ' JR最終列車番号
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.MAXTANK3), '')             AS MAXTANK3 ")         ' JR最終列車牽引車数
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.TRAINCLASS), '')           AS TRAINCLASS ")       ' 列車区分
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.SPEEDCLASS), '')           AS SPEEDCLASS ")       ' 高速列車区分
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.SHIPORDERCLASS), '')       AS SHIPORDERCLASS ")   ' 発送順区分
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.DEPDAYS), '')              AS DEPDAYS ")          ' 発日日数
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.MARGEDAYS), '')            AS MARGEDAYS ")        ' 特継日数
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.ARRDAYS), '')              AS ARRDAYS ")          ' 積車着日数
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.ACCDAYS), '')              AS ACCDAYS ")          ' 受入日数
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.EMPARRDAYS), '')           AS EMPARRDAYS ")       ' 空車着日数
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.USEDAYS), '')              AS USEDAYS ")          ' 当日利用日数
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.FEEKBN), '')               AS FEEKBN ")           ' 料金マスタ区分
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.RUN), '')                  AS RUN ")              ' 稼働フラグ
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.ZAIKOSORT), '')            AS ZAIKOSORT ")        ' 在庫管理表表示ソート区分
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.BIKOU), '')                AS BIKOU ")            ' 備考
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.DELFLG), '')               AS DELFLG ")           ' 削除フラグ
        SQLStrBldr.AppendLine(" FROM ")
        SQLStrBldr.AppendLine("     [oil].OIM0007_TRAIN OIM0007 ")
        SQLStrBldr.AppendLine(" WHERE ")
        SQLStrBldr.AppendLine("     OIM0007.OFFICECODE = @P1 ")
        SQLStrBldr.AppendLine("     AND ")
        SQLStrBldr.AppendLine("     OIM0007.TRAINNO = @P2 ")
        SQLStrBldr.AppendLine("     AND ")
        SQLStrBldr.AppendLine("     OIM0007.TSUMI = @P3 ")
        SQLStrBldr.AppendLine("     AND ")
        SQLStrBldr.AppendLine("     OIM0007.OTTRAINNO = @P4 ")
        SQLStrBldr.AppendLine("     AND ")
        SQLStrBldr.AppendLine("     OIM0007.DEPSTATION = @P5 ")
        SQLStrBldr.AppendLine("     AND ")
        SQLStrBldr.AppendLine("     OIM0007.ARRSTATION = @P6 ")

        Try
            Using SQLcmd As New SqlCommand(SQLStrBldr.ToString(), SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 6)     ' 管轄受注営業所
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 4)     ' 本線列車番号
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 1)     ' 積置フラグ
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 4)     ' OT列車番号
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 7)     ' 発駅コード
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 7)     ' 着駅コード
                PARA1.Value = WF_OFFICECODE.Text
                PARA2.Value = WF_TRAINNO.Text
                PARA3.Value = WF_TSUMI.Text
                PARA4.Value = WF_OTTRAINNO.Text
                PARA5.Value = WF_DEPSTATION.Text
                PARA6.Value = WF_ARRSTATION.Text

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    Dim OIM0007Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIM0007Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIM0007Chk.Load(SQLdr)

                    If OIM0007Chk.Rows.Count > 0 Then
                        '重複データエラー
                        O_MESSAGENO = Messages.C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR
                    Else
                        '正常終了時
                        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0007C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0007C UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 列車マスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateMaster(ByVal SQLcon As SqlConnection)

        '○ ＤＢ更新
        Dim SQLStrBldr As New StringBuilder
        SQLStrBldr.AppendLine(" DECLARE @hensuu AS bigint; ")
        SQLStrBldr.AppendLine("    SET @hensuu = 0; ")
        SQLStrBldr.AppendLine(" DECLARE hensuu CURSOR FOR ")
        SQLStrBldr.AppendLine("    SELECT ")
        SQLStrBldr.AppendLine("        CAST(UPDTIMSTP AS bigint) AS hensuu ")
        SQLStrBldr.AppendLine("    FROM ")
        SQLStrBldr.AppendLine("        OIL.OIM0007_TRAIN ")
        SQLStrBldr.AppendLine("    WHERE ")
        SQLStrBldr.AppendLine("        OFFICECODE = @P00 ")
        SQLStrBldr.AppendLine("        AND ")
        SQLStrBldr.AppendLine("        TRAINNO = @P01 ")
        SQLStrBldr.AppendLine("        AND ")
        SQLStrBldr.AppendLine("        TSUMI = @P03 ")
        SQLStrBldr.AppendLine("        AND ")
        SQLStrBldr.AppendLine("        OTTRAINNO = @P04 ")
        SQLStrBldr.AppendLine("        AND ")
        SQLStrBldr.AppendLine("        DEPSTATION = @P06 ")
        SQLStrBldr.AppendLine("        AND ")
        SQLStrBldr.AppendLine("        ARRSTATION = @P07; ")
        SQLStrBldr.AppendLine(" OPEN hensuu; ")
        SQLStrBldr.AppendLine(" FETCH NEXT FROM hensuu INTO @hensuu; ")
        SQLStrBldr.AppendLine(" IF (@@FETCH_STATUS = 0) ")
        SQLStrBldr.AppendLine("     UPDATE OIL.OIM0007_TRAIN ")
        SQLStrBldr.AppendLine("     SET ")
        SQLStrBldr.AppendLine("         TRAINNAME = @P02 ")
        SQLStrBldr.AppendLine("         , OTFLG = @P05 ")
        SQLStrBldr.AppendLine("         , JRTRAINNO1 = @P08 ")
        SQLStrBldr.AppendLine("         , MAXTANK1 = @P09 ")
        SQLStrBldr.AppendLine("         , JRTRAINNO2 = @P10 ")
        SQLStrBldr.AppendLine("         , MAXTANK2 = @P11 ")
        SQLStrBldr.AppendLine("         , JRTRAINNO3 = @P12 ")
        SQLStrBldr.AppendLine("         , MAXTANK3 = @P13 ")
        SQLStrBldr.AppendLine("         , TRAINCLASS = @P14 ")
        SQLStrBldr.AppendLine("         , SPEEDCLASS = @P15 ")
        SQLStrBldr.AppendLine("         , SHIPORDERCLASS = @P16 ")
        SQLStrBldr.AppendLine("         , DEPDAYS = @P17 ")
        SQLStrBldr.AppendLine("         , MARGEDAYS = @P18 ")
        SQLStrBldr.AppendLine("         , ARRDAYS = @P19 ")
        SQLStrBldr.AppendLine("         , ACCDAYS = @P20 ")
        SQLStrBldr.AppendLine("         , EMPARRDAYS = @P21 ")
        SQLStrBldr.AppendLine("         , USEDAYS = @P22 ")
        SQLStrBldr.AppendLine("         , FEEKBN = @P23 ")
        SQLStrBldr.AppendLine("         , RUN = @P24 ")
        SQLStrBldr.AppendLine("         , ZAIKOSORT = @P25 ")
        SQLStrBldr.AppendLine("         , BIKOU = @P26 ")
        SQLStrBldr.AppendLine("         , DELFLG = @P27 ")
        SQLStrBldr.AppendLine("         , UPDYMD = @P31 ")
        SQLStrBldr.AppendLine("         , UPDUSER = @P32 ")
        SQLStrBldr.AppendLine("         , UPDTERMID = @P33 ")
        SQLStrBldr.AppendLine("         , RECEIVEYMD = @P34 ")
        SQLStrBldr.AppendLine("     WHERE ")
        SQLStrBldr.AppendLine("         OFFICECODE = @P00 ")
        SQLStrBldr.AppendLine("         AND ")
        SQLStrBldr.AppendLine("         TRAINNO = @P01 ")
        SQLStrBldr.AppendLine("         AND ")
        SQLStrBldr.AppendLine("         TSUMI = @P03 ")
        SQLStrBldr.AppendLine("         AND ")
        SQLStrBldr.AppendLine("         OTTRAINNO = @P04 ")
        SQLStrBldr.AppendLine("         AND ")
        SQLStrBldr.AppendLine("         DEPSTATION = @P06 ")
        SQLStrBldr.AppendLine("         AND ")
        SQLStrBldr.AppendLine("         ARRSTATION = @P07; ")
        SQLStrBldr.AppendLine("  IF (@@FETCH_STATUS <> 0) ")
        SQLStrBldr.AppendLine("     INSERT INTO OIL.OIM0007_TRAIN( ")
        SQLStrBldr.AppendLine("         OFFICECODE ")
        SQLStrBldr.AppendLine("         , TRAINNO ")
        SQLStrBldr.AppendLine("         , TRAINNAME ")
        SQLStrBldr.AppendLine("         , TSUMI ")
        SQLStrBldr.AppendLine("         , OTTRAINNO ")
        SQLStrBldr.AppendLine("         , OTFLG ")
        SQLStrBldr.AppendLine("         , DEPSTATION ")
        SQLStrBldr.AppendLine("         , ARRSTATION ")
        SQLStrBldr.AppendLine("         , JRTRAINNO1 ")
        SQLStrBldr.AppendLine("         , MAXTANK1 ")
        SQLStrBldr.AppendLine("         , JRTRAINNO2 ")
        SQLStrBldr.AppendLine("         , MAXTANK2 ")
        SQLStrBldr.AppendLine("         , JRTRAINNO3 ")
        SQLStrBldr.AppendLine("         , MAXTANK3 ")
        SQLStrBldr.AppendLine("         , TRAINCLASS ")
        SQLStrBldr.AppendLine("         , SPEEDCLASS ")
        SQLStrBldr.AppendLine("         , SHIPORDERCLASS ")
        SQLStrBldr.AppendLine("         , DEPDAYS ")
        SQLStrBldr.AppendLine("         , MARGEDAYS ")
        SQLStrBldr.AppendLine("         , ARRDAYS ")
        SQLStrBldr.AppendLine("         , ACCDAYS ")
        SQLStrBldr.AppendLine("         , EMPARRDAYS ")
        SQLStrBldr.AppendLine("         , USEDAYS ")
        SQLStrBldr.AppendLine("         , FEEKBN ")
        SQLStrBldr.AppendLine("         , RUN ")
        SQLStrBldr.AppendLine("         , ZAIKOSORT ")
        SQLStrBldr.AppendLine("         , BIKOU ")
        SQLStrBldr.AppendLine("         , DELFLG ")
        SQLStrBldr.AppendLine("         , INITYMD ")
        SQLStrBldr.AppendLine("         , INITUSER ")
        SQLStrBldr.AppendLine("         , INITTERMID ")
        SQLStrBldr.AppendLine("         , UPDYMD ")
        SQLStrBldr.AppendLine("         , UPDUSER ")
        SQLStrBldr.AppendLine("         , UPDTERMID ")
        SQLStrBldr.AppendLine("         , RECEIVEYMD) ")
        SQLStrBldr.AppendLine("     VALUES ")
        SQLStrBldr.AppendLine("         (@P00 ")
        SQLStrBldr.AppendLine("         , @P01 ")
        SQLStrBldr.AppendLine("         , @P02 ")
        SQLStrBldr.AppendLine("         , @P03 ")
        SQLStrBldr.AppendLine("         , @P04 ")
        SQLStrBldr.AppendLine("         , @P05 ")
        SQLStrBldr.AppendLine("         , @P06 ")
        SQLStrBldr.AppendLine("         , @P07 ")
        SQLStrBldr.AppendLine("         , @P08 ")
        SQLStrBldr.AppendLine("         , @P09 ")
        SQLStrBldr.AppendLine("         , @P10 ")
        SQLStrBldr.AppendLine("         , @P11 ")
        SQLStrBldr.AppendLine("         , @P12 ")
        SQLStrBldr.AppendLine("         , @P13 ")
        SQLStrBldr.AppendLine("         , @P14 ")
        SQLStrBldr.AppendLine("         , @P15 ")
        SQLStrBldr.AppendLine("         , @P16 ")
        SQLStrBldr.AppendLine("         , @P17 ")
        SQLStrBldr.AppendLine("         , @P18 ")
        SQLStrBldr.AppendLine("         , @P19 ")
        SQLStrBldr.AppendLine("         , @P20 ")
        SQLStrBldr.AppendLine("         , @P21 ")
        SQLStrBldr.AppendLine("         , @P22 ")
        SQLStrBldr.AppendLine("         , @P23 ")
        SQLStrBldr.AppendLine("         , @P24 ")
        SQLStrBldr.AppendLine("         , @P25 ")
        SQLStrBldr.AppendLine("         , @P26 ")
        SQLStrBldr.AppendLine("         , @P27 ")
        SQLStrBldr.AppendLine("         , @P28 ")
        SQLStrBldr.AppendLine("         , @P29 ")
        SQLStrBldr.AppendLine("         , @P30 ")
        SQLStrBldr.AppendLine("         , @P31 ")
        SQLStrBldr.AppendLine("         , @P32 ")
        SQLStrBldr.AppendLine("         , @P33 ")
        SQLStrBldr.AppendLine("         , @P34); ")
        SQLStrBldr.AppendLine("  CLOSE hensuu; ")
        SQLStrBldr.AppendLine("  DEALLOCATE hensuu; ")

        '○ 更新ジャーナル出力
        Dim SQLJnlBldr As New StringBuilder
        SQLJnlBldr.AppendLine(" SELECT ")
        SQLJnlBldr.AppendLine("    OFFICECODE ")
        SQLJnlBldr.AppendLine("    , TRAINNO ")
        SQLJnlBldr.AppendLine("    , TRAINNAME ")
        SQLJnlBldr.AppendLine("    , TSUMI ")
        SQLJnlBldr.AppendLine("    , OTTRAINNO ")
        SQLJnlBldr.AppendLine("    , OTFLG ")
        SQLJnlBldr.AppendLine("    , DEPSTATION ")
        SQLJnlBldr.AppendLine("    , ARRSTATION ")
        SQLJnlBldr.AppendLine("    , JRTRAINNO1 ")
        SQLJnlBldr.AppendLine("    , MAXTANK1 ")
        SQLJnlBldr.AppendLine("    , JRTRAINNO2 ")
        SQLJnlBldr.AppendLine("    , MAXTANK2 ")
        SQLJnlBldr.AppendLine("    , JRTRAINNO3 ")
        SQLJnlBldr.AppendLine("    , MAXTANK3 ")
        SQLJnlBldr.AppendLine("    , TRAINCLASS ")
        SQLJnlBldr.AppendLine("    , SPEEDCLASS ")
        SQLJnlBldr.AppendLine("    , SHIPORDERCLASS ")
        SQLJnlBldr.AppendLine("    , DEPDAYS ")
        SQLJnlBldr.AppendLine("    , MARGEDAYS ")
        SQLJnlBldr.AppendLine("    , ARRDAYS ")
        SQLJnlBldr.AppendLine("    , ACCDAYS ")
        SQLJnlBldr.AppendLine("    , EMPARRDAYS ")
        SQLJnlBldr.AppendLine("    , USEDAYS ")
        SQLJnlBldr.AppendLine("    , FEEKBN ")
        SQLJnlBldr.AppendLine("    , RUN ")
        SQLJnlBldr.AppendLine("    , ZAIKOSORT ")
        SQLJnlBldr.AppendLine("    , BIKOU ")
        SQLJnlBldr.AppendLine("    , DELFLG ")
        SQLJnlBldr.AppendLine("    , INITYMD ")
        SQLJnlBldr.AppendLine("    , INITUSER ")
        SQLJnlBldr.AppendLine("    , INITTERMID ")
        SQLJnlBldr.AppendLine("    , UPDYMD ")
        SQLJnlBldr.AppendLine("    , UPDUSER ")
        SQLJnlBldr.AppendLine("    , UPDTERMID ")
        SQLJnlBldr.AppendLine("    , RECEIVEYMD ")
        SQLJnlBldr.AppendLine("    , CAST(UPDTIMSTP As bigint) As UPDTIMSTP ")
        SQLJnlBldr.AppendLine(" FROM ")
        SQLJnlBldr.AppendLine("    OIL.OIM0007_TRAIN ")
        SQLJnlBldr.AppendLine(" WHERE ")
        SQLJnlBldr.AppendLine("        OFFICECODE = @P00 ")
        SQLJnlBldr.AppendLine("        AND ")
        SQLJnlBldr.AppendLine("        TRAINNO = @P01 ")
        SQLJnlBldr.AppendLine("        AND ")
        SQLJnlBldr.AppendLine("        TSUMI = @P03 ")
        SQLJnlBldr.AppendLine("        AND ")
        SQLJnlBldr.AppendLine("        OTTRAINNO = @P04 ")
        SQLJnlBldr.AppendLine("        AND ")
        SQLJnlBldr.AppendLine("        DEPSTATION = @P06 ")
        SQLJnlBldr.AppendLine("        AND ")
        SQLJnlBldr.AppendLine("        ARRSTATION = @P07 ")

        Try
            Using SQLcmd As New SqlCommand(SQLStrBldr.ToString(), SQLcon), SQLcmdJnl As New SqlCommand(SQLJnlBldr.ToString(), SQLcon)
                Dim PARA00 As SqlParameter = SQLcmd.Parameters.Add("@P00", SqlDbType.NVarChar, 6)           ' 管轄受注営業所
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 4)           ' 本線列車番号
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 20)          ' 本線列車番号名
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 1)           ' 積置フラグ
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 4)           ' OT列車番号
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 1)           ' OT発送日報送信フラグ
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 7)           ' 発駅コード
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.NVarChar, 7)           ' 着駅コード
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.NVarChar, 4)           ' JR発列車番号
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", SqlDbType.Int)                   ' JR発列車牽引車数
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 4)           ' JR中継列車番号
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.Int)                   ' JR中継列車牽引車数
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 4)           ' JR最終列車番号
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.Int)                   ' JR最終列車牽引車数
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 1)           ' 列車区分
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 1)           ' 高速列車区分
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.NVarChar, 1)           ' 発送順区分
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.Int)                   ' 発日日数
                Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.Int)                   ' 特継日数
                Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.Int)                   ' 積車着日数
                Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.Int)                   ' 受入日数
                Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.Int)                   ' 空車着日数
                Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", SqlDbType.Int)                   ' 当日利用日数
                Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", SqlDbType.NVarChar, 1)           ' 料金マスタ区分
                Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", SqlDbType.NVarChar, 1)           ' 稼働フラグ
                Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", SqlDbType.Int)                   ' 在庫管理表表示ソート区分
                Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", SqlDbType.NVarChar, 200)         ' 備考
                Dim PARA27 As SqlParameter = SQLcmd.Parameters.Add("@P27", SqlDbType.NVarChar, 1)           ' 削除フラグ
                Dim PARA28 As SqlParameter = SQLcmd.Parameters.Add("@P28", SqlDbType.DateTime)              ' 登録年月日
                Dim PARA29 As SqlParameter = SQLcmd.Parameters.Add("@P29", SqlDbType.NVarChar, 20)          ' 登録ユーザーＩＤ
                Dim PARA30 As SqlParameter = SQLcmd.Parameters.Add("@P30", SqlDbType.NVarChar, 20)          ' 登録端末
                Dim PARA31 As SqlParameter = SQLcmd.Parameters.Add("@P31", SqlDbType.DateTime)              ' 更新年月日
                Dim PARA32 As SqlParameter = SQLcmd.Parameters.Add("@P32", SqlDbType.NVarChar, 20)          ' 更新ユーザーＩＤ
                Dim PARA33 As SqlParameter = SQLcmd.Parameters.Add("@P33", SqlDbType.NVarChar, 20)          ' 更新端末
                Dim PARA34 As SqlParameter = SQLcmd.Parameters.Add("@P34", SqlDbType.DateTime)              ' 集信日時


                Dim JPARA00 As SqlParameter = SQLcmdJnl.Parameters.Add("@P00", SqlDbType.NVarChar, 6)       ' 管轄受注営業所
                Dim JPARA01 As SqlParameter = SQLcmdJnl.Parameters.Add("@P01", SqlDbType.NVarChar, 4)       ' 本線列車番号
                Dim JPARA03 As SqlParameter = SQLcmdJnl.Parameters.Add("@P03", SqlDbType.NVarChar, 1)       ' 積置フラグ
                Dim JPARA04 As SqlParameter = SQLcmdJnl.Parameters.Add("@P04", SqlDbType.NVarChar, 4)       ' OT列車番号
                Dim JPARA06 As SqlParameter = SQLcmdJnl.Parameters.Add("@P06", SqlDbType.NVarChar, 7)       ' 発駅コード
                Dim JPARA07 As SqlParameter = SQLcmdJnl.Parameters.Add("@P07", SqlDbType.NVarChar, 7)       ' 着駅コード

                Dim OIM0007row As DataRow = OIM0007INPtbl.Rows(0)
                Dim WW_DATENOW As DateTime = Date.Now

                'DB更新
                PARA00.Value = OIM0007row("OFFICECODE")
                PARA01.Value = OIM0007row("TRAINNO")
                PARA02.Value = OIM0007row("TRAINNAME")
                PARA03.Value = OIM0007row("TSUMI")
                PARA04.Value = OIM0007row("OTTRAINNO")
                PARA05.Value = OIM0007row("OTFLG")
                PARA06.Value = OIM0007row("DEPSTATION")
                PARA07.Value = OIM0007row("ARRSTATION")
                PARA08.Value = OIM0007row("JRTRAINNO1")
                PARA09.Value = If(String.IsNullOrEmpty(OIM0007row("MAXTANK1")), SqlTypes.SqlInt32.Null, OIM0007row("MAXTANK1"))
                PARA10.Value = OIM0007row("JRTRAINNO2")
                PARA11.Value = If(String.IsNullOrEmpty(OIM0007row("MAXTANK2")), SqlTypes.SqlInt32.Null, OIM0007row("MAXTANK2"))
                PARA12.Value = OIM0007row("JRTRAINNO3")
                PARA13.Value = If(String.IsNullOrEmpty(OIM0007row("MAXTANK3")), SqlTypes.SqlInt32.Null, OIM0007row("MAXTANK3"))
                PARA14.Value = OIM0007row("TRAINCLASS")
                PARA15.Value = OIM0007row("SPEEDCLASS")
                PARA16.Value = OIM0007row("SHIPORDERCLASS")
                PARA17.Value = If(String.IsNullOrEmpty(OIM0007row("DEPDAYS")), SqlTypes.SqlInt32.Null, OIM0007row("DEPDAYS"))
                PARA18.Value = If(String.IsNullOrEmpty(OIM0007row("MARGEDAYS")), SqlTypes.SqlInt32.Null, OIM0007row("MARGEDAYS"))
                PARA19.Value = If(String.IsNullOrEmpty(OIM0007row("ARRDAYS")), SqlTypes.SqlInt32.Null, OIM0007row("ARRDAYS"))
                PARA20.Value = If(String.IsNullOrEmpty(OIM0007row("ACCDAYS")), SqlTypes.SqlInt32.Null, OIM0007row("ACCDAYS"))
                PARA21.Value = If(String.IsNullOrEmpty(OIM0007row("EMPARRDAYS")), SqlTypes.SqlInt32.Null, OIM0007row("EMPARRDAYS"))
                PARA22.Value = If(String.IsNullOrEmpty(OIM0007row("USEDAYS")), SqlTypes.SqlInt32.Null, OIM0007row("USEDAYS"))
                PARA23.Value = OIM0007row("FEEKBN")
                PARA24.Value = OIM0007row("RUN")
                PARA25.Value = If(String.IsNullOrEmpty(OIM0007row("ZAIKOSORT")), SqlTypes.SqlInt32.Null, OIM0007row("ZAIKOSORT"))
                PARA26.Value = OIM0007row("BIKOU")
                PARA27.Value = OIM0007row("DELFLG")
                PARA28.Value = WW_DATENOW
                PARA29.Value = Master.USERID
                PARA30.Value = Master.USERTERMID
                PARA31.Value = WW_DATENOW
                PARA32.Value = Master.USERID
                PARA33.Value = Master.USERTERMID
                PARA34.Value = C_DEFAULT_YMD

                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

                OIM0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                ' 更新ジャーナル出力
                JPARA00.Value = OIM0007row("OFFICECODE")
                JPARA01.Value = OIM0007row("TRAINNO")
                JPARA03.Value = OIM0007row("TSUMI")
                JPARA04.Value = OIM0007row("OTTRAINNO")
                JPARA06.Value = OIM0007row("DEPSTATION")
                JPARA07.Value = OIM0007row("ARRSTATION")

                Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                    If IsNothing(OIM0007UPDtbl) Then
                        OIM0007UPDtbl = New DataTable

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            OIM0007UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    End If

                    OIM0007UPDtbl.Clear()
                    OIM0007UPDtbl.Load(SQLdr)
                End Using

                For Each OIM0007UPDrow As DataRow In OIM0007UPDtbl.Rows
                    CS0020JOURNAL.TABLENM = "OIM0007L"
                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                    CS0020JOURNAL.ROW = OIM0007UPDrow
                    CS0020JOURNAL.CS0020JOURNAL()
                    If Not isNormal(CS0020JOURNAL.ERR) Then
                        Master.Output(CS0020JOURNAL.ERR, C_MESSAGE_TYPE.ABORT, "CS0020JOURNAL JOURNAL")

                        CS0011LOGWrite.INFSUBCLASS = "MAIN"                     ' SUBクラス名
                        CS0011LOGWrite.INFPOSI = "CS0020JOURNAL JOURNAL"
                        CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                        CS0011LOGWrite.TEXT = "CS0020JOURNAL Call Err!"
                        CS0011LOGWrite.MESSAGENO = CS0020JOURNAL.ERR
                        CS0011LOGWrite.CS0011LOGWrite()                         ' ログ出力
                        Exit Sub
                    End If
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0007L UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             ' SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0007L UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 ' ログ出力
            Exit Sub
        End Try

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
        DetailBoxToOIM0007INPtbl(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ERR_SW) Then
            OIM0007tbl_UPD()
            '入力レコードに変更がない場合は、メッセージダイアログを表示して処理打ち切り
            If C_MESSAGE_NO.NO_CHANGE_UPDATE.Equals(WW_ERRCODE) Then
                Master.Output(C_MESSAGE_NO.NO_CHANGE_UPDATE, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
                Exit Sub
            End If
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIM0007tbl, work.WF_SEL_INPTBL.Text)

        '○ メッセージ表示
        If WW_ERR_SW = "" Then
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)
        Else
            If isNormal(WW_ERR_SW) Then
                Master.Output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.INF)

            ElseIf WW_ERR_SW = C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR Then
                Master.Output(WW_ERR_SW, C_MESSAGE_TYPE.ERR, "管轄受注営業所, 本線列車番号, 積置フラグ, 発駅コード, 着駅コード", needsPopUp:=True)

            Else
                Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            End If
        End If

        If isNormal(WW_ERR_SW) Then
            '前ページ遷移
            Master.TransitionPrevPage()
        End If

    End Sub

    ''' <summary>
    ''' 詳細画面-テーブル退避
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub DetailBoxToOIM0007INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(WF_DELFLG.Text)            '削除フラグ

        '○ GridViewから未選択状態で表更新ボタンを押下時の例外を回避する
        If String.IsNullOrEmpty(WF_Sel_LINECNT.Text) AndAlso
            String.IsNullOrEmpty(WF_DELFLG.Text) Then
            Master.Output(C_MESSAGE_NO.INVALID_PROCCESS_ERROR, C_MESSAGE_TYPE.ERR, "no Detail", needsPopUp:=True)

            CS0011LOGWrite.INFSUBCLASS = "DetailBoxToINPtbl"        'SUBクラス名
            CS0011LOGWrite.INFPOSI = "non Detail"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWrite.TEXT = "non Detail"
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                         'ログ出力

            O_RTN = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            Exit Sub
        End If

        Master.CreateEmptyTable(OIM0007INPtbl, work.WF_SEL_INPTBL.Text)
        Dim OIM0007INProw As DataRow = OIM0007INPtbl.NewRow

        '○ 初期クリア
        For Each OIM0007INPcol As DataColumn In OIM0007INPtbl.Columns
            If IsDBNull(OIM0007INProw.Item(OIM0007INPcol)) OrElse IsNothing(OIM0007INProw.Item(OIM0007INPcol)) Then
                Select Case OIM0007INPcol.ColumnName
                    Case "LINECNT"
                        OIM0007INProw.Item(OIM0007INPcol) = 0
                    Case "OPERATION"
                        OIM0007INProw.Item(OIM0007INPcol) = C_LIST_OPERATION_CODE.NODATA
                    Case "UPDTIMSTP"
                        OIM0007INProw.Item(OIM0007INPcol) = 0
                    Case "SELECT"
                        OIM0007INProw.Item(OIM0007INPcol) = 1
                    Case "HIDDEN"
                        OIM0007INProw.Item(OIM0007INPcol) = 0
                    Case Else
                        OIM0007INProw.Item(OIM0007INPcol) = ""
                End Select
            End If
        Next

        'LINECNT
        If WF_Sel_LINECNT.Text = "" Then
            OIM0007INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(WF_Sel_LINECNT.Text, OIM0007INProw("LINECNT"))
            Catch ex As Exception
                OIM0007INProw("LINECNT") = 0
            End Try
        End If

        OIM0007INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        OIM0007INProw("UPDTIMSTP") = 0
        OIM0007INProw("SELECT") = 1
        OIM0007INProw("HIDDEN") = 0

        OIM0007INProw("OFFICECODE") = WF_OFFICECODE.Text　　              ' 管轄受注営業所
        OIM0007INProw("TRAINNO") = WF_TRAINNO.Text                        ' 本線列車番号
        OIM0007INProw("TRAINNAME") = WF_TRAINNAME.Text                    ' 本線列車番号名
        OIM0007INProw("TSUMI") = WF_TSUMI.Text                            ' 積置フラグ
        OIM0007INProw("OTTRAINNO") = WF_OTTRAINNO.Text                    ' OT列車番号
        OIM0007INProw("OTFLG") = WF_OTFLG.Text                            ' OT発送日報送信フラグ
        OIM0007INProw("DEPSTATION") = WF_DEPSTATION.Text                  ' 発駅コード
        OIM0007INProw("ARRSTATION") = WF_ARRSTATION.Text                  ' 着駅コード
        OIM0007INProw("JRTRAINNO1") = WF_JRTRAINNO1.Text                  ' JR発列車番号
        OIM0007INProw("MAXTANK1") = WF_MAXTANK1.Text                      ' JR発列車牽引車数
        OIM0007INProw("JRTRAINNO2") = WF_JRTRAINNO2.Text                  ' JR中継列車番号
        OIM0007INProw("MAXTANK2") = WF_MAXTANK2.Text                      ' JR中継列車牽引車数
        OIM0007INProw("JRTRAINNO3") = WF_JRTRAINNO3.Text                  ' JR最終列車番号
        OIM0007INProw("MAXTANK3") = WF_MAXTANK3.Text                      ' JR最終列車牽引車数
        OIM0007INProw("TRAINCLASS") = WF_TRAINCLASS.Text                  ' 列車区分
        OIM0007INProw("SPEEDCLASS") = WF_SPEEDCLASS.Text                  ' 高速列車区分
        OIM0007INProw("SHIPORDERCLASS") = WF_SHIPORDERCLASS.Text          ' 発送順区分
        OIM0007INProw("DEPDAYS") = WF_DEPDAYS.Text                        ' 発日日数
        OIM0007INProw("MARGEDAYS") = WF_MARGEDAYS.Text                    ' 特継日数
        OIM0007INProw("ARRDAYS") = WF_ARRDAYS.Text                        ' 積車着日数
        OIM0007INProw("ACCDAYS") = WF_ACCDAYS.Text                        ' 受入日数
        OIM0007INProw("EMPARRDAYS") = WF_EMPARRDAYS.Text                  ' 空車着日数
        OIM0007INProw("USEDAYS") = WF_USEDAYS.Text                        ' 当日利用日数
        OIM0007INProw("FEEKBN") = WF_FEEKBN.Text                          ' 料金マスタ区分
        OIM0007INProw("RUN") = WF_RUN.Text                                ' 稼働フラグ
        OIM0007INProw("ZAIKOSORT") = WF_ZAIKOSORT.Text                    ' 在庫管理表表示ソート区分
        OIM0007INProw("BIKOU") = WF_BIKOU.Text                            ' 備考
        OIM0007INProw("DELFLG") = WF_DELFLG.Text                          ' 削除フラグ

        '○ チェック用テーブルに登録する
        OIM0007INPtbl.Rows.Add(OIM0007INProw)

    End Sub

    ''' <summary>
    ''' 詳細画面-クリアボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_CLEAR_Click()

        '○ DetailBoxをINPtblへ退避
        DetailBoxToOIM0007INPtbl(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        Dim inputChangeFlg As Boolean = True
        Dim OIM0007INProw As DataRow = OIM0007INPtbl.Rows(0)

        ' 既存レコードとの比較
        For Each OIM0007row As DataRow In OIM0007tbl.Rows
            ' KEY項目が等しい時
            If OIM0007row("OFFICECODE") = OIM0007INProw("OFFICECODE") AndAlso
                OIM0007row("TRAINNO") = OIM0007INProw("TRAINNO") AndAlso
                OIM0007row("TSUMI") = OIM0007INProw("TSUMI") AndAlso
                OIM0007row("OTTRAINNO") = OIM0007INProw("OTTRAINNO") AndAlso
                OIM0007row("DEPSTATION") = OIM0007INProw("DEPSTATION") AndAlso
                OIM0007row("ARRSTATION") = OIM0007INProw("ARRSTATION") Then
                ' KEY項目以外の項目に変更があるかチェック
                If OIM0007row("TRAINNAME") = OIM0007INProw("TRAINNAME") AndAlso
                    OIM0007row("OTFLG") = OIM0007INProw("OTFLG") AndAlso
                    OIM0007row("JRTRAINNO1") = OIM0007INProw("JRTRAINNO1") AndAlso
                    OIM0007row("MAXTANK1") = OIM0007INProw("MAXTANK1") AndAlso
                    OIM0007row("JRTRAINNO2") = OIM0007INProw("JRTRAINNO2") AndAlso
                    OIM0007row("MAXTANK2") = OIM0007INProw("MAXTANK2") AndAlso
                    OIM0007row("JRTRAINNO3") = OIM0007INProw("JRTRAINNO3") AndAlso
                    OIM0007row("MAXTANK3") = OIM0007INProw("MAXTANK3") AndAlso
                    OIM0007row("TRAINCLASS") = OIM0007INProw("TRAINCLASS") AndAlso
                    OIM0007row("SPEEDCLASS") = OIM0007INProw("SPEEDCLASS") AndAlso
                    OIM0007row("SHIPORDERCLASS") = OIM0007INProw("SHIPORDERCLASS") AndAlso
                    OIM0007row("DEPDAYS") = OIM0007INProw("DEPDAYS") AndAlso
                    OIM0007row("MARGEDAYS") = OIM0007INProw("MARGEDAYS") AndAlso
                    OIM0007row("ARRDAYS") = OIM0007INProw("ARRDAYS") AndAlso
                    OIM0007row("ACCDAYS") = OIM0007INProw("ACCDAYS") AndAlso
                    OIM0007row("EMPARRDAYS") = OIM0007INProw("EMPARRDAYS") AndAlso
                    OIM0007row("USEDAYS") = OIM0007INProw("USEDAYS") AndAlso
                    OIM0007row("FEEKBN") = OIM0007INProw("FEEKBN") AndAlso
                    OIM0007row("RUN") = OIM0007INProw("RUN") AndAlso
                    OIM0007row("ZAIKOSORT") = OIM0007INProw("ZAIKOSORT") AndAlso
                    OIM0007row("BIKOU") = OIM0007INProw("BIKOU") AndAlso
                    OIM0007row("DELFLG") = OIM0007INProw("DELFLG") Then
                    ' 変更がないときは、入力変更フラグをOFFにする
                    inputChangeFlg = False
                End If

                Exit For

            End If
        Next

        If inputChangeFlg Then
            '変更がある場合は、確認ダイアログを表示
            Master.Output(C_MESSAGE_NO.UPDATE_CANCEL_CONFIRM, C_MESSAGE_TYPE.QUES, I_PARA02:="W",
                needsPopUp:=True, messageBoxTitle:="確認", IsConfirm:=True, YesButtonId:="btnClearConfirmOk")
        Else
            '変更がない場合は、確認ダイアログを表示せずに、前画面に戻る
            WF_CLEAR_ConfirmOkClick()
        End If

    End Sub

    ''' <summary>
    ''' 詳細画面-クリアボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_CLEAR_ConfirmOkClick()

        '○ 詳細画面初期化
        DetailBoxClear()

        '○ メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_CLEAR_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""

        Master.TransitionPrevPage()

    End Sub

    ''' <summary>
    ''' 詳細画面初期化
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailBoxClear()

        '○ 状態をクリア
        For Each OIM0007row As DataRow In OIM0007tbl.Rows
            Select Case OIM0007row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0007row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0007row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0007row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIM0007tbl, work.WF_SEL_INPTBL.Text)

        WF_Sel_LINECNT.Text = ""            'LINECNT

        WF_OFFICECODE.Text = ""             ' 管轄受注営業所
        WF_TRAINNO.Text = ""                ' 本線列車番号
        WF_TRAINNAME.Text = ""              ' 本線列車番号名
        WF_TSUMI.Text = ""                  ' 積置フラグ
        WF_OTTRAINNO.Text = ""              ' OT列車番号
        WF_OTFLG.Text = ""                  ' OT発送日報送信フラグ
        WF_DEPSTATION.Text = ""             ' 発駅コード
        WF_ARRSTATION.Text = ""             ' 着駅コード
        WF_JRTRAINNO1.Text = ""             ' JR発列車番号
        WF_MAXTANK1.Text = ""               ' JR発列車牽引車数
        WF_JRTRAINNO2.Text = ""             ' JR中継列車番号
        WF_MAXTANK2.Text = ""               ' JR中継列車牽引車数
        WF_JRTRAINNO3.Text = ""             ' JR最終列車番号
        WF_MAXTANK3.Text = ""               ' JR最終列車牽引車数
        WF_TRAINCLASS.Text = ""             ' 列車区分
        WF_SPEEDCLASS.Text = ""             ' 高速列車区分
        WF_SHIPORDERCLASS.Text = ""         ' 発送順区分
        WF_DEPDAYS.Text = ""                ' 発日日数
        WF_MARGEDAYS.Text = ""              ' 特継日数
        WF_ARRDAYS.Text = ""                ' 積車着日数
        WF_ACCDAYS.Text = ""                ' 受入日数
        WF_EMPARRDAYS.Text = ""             ' 空車着日数
        WF_USEDAYS.Text = ""                ' 当日利用日数
        WF_FEEKBN.Text = ""                 ' 料金マスタ区分
        WF_RUN.Text = ""                    ' 稼働フラグ
        WF_ZAIKOSORT.Text = ""              ' 在庫管理表表示ソート区分
        WF_BIKOU.Text = ""                  ' 備考
        WF_DELFLG.Text = ""                 ' 削除フラグ

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
                Select Case WF_LeftMViewChange.Value
                    Case LIST_BOX_CLASSIFICATION.LC_CALENDAR
                        ' 日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す

                    Case Else
                        ' 以外
                        Dim prmData As New Hashtable
                        prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

                        'フィールドによってパラメータを変える
                        Select Case WF_FIELD.Value
                            Case WF_OFFICECODE.ID
                                ' 管轄受注営業所
                                prmData = work.CreateOfficeCodeParam(Master.USER_ORG)
                            Case WF_TSUMI.ID
                                ' 積置フラグ
                                prmData = work.CreateFIXParam(Master.USERCAMP, "TSUMI")
                            Case WF_DEPSTATION.ID
                                ' 発駅コード
                                prmData = work.CreateFIXParam(Master.USERCAMP, "STATION")
                            Case WF_ARRSTATION.ID
                                ' 着駅コード
                                prmData = work.CreateFIXParam(Master.USERCAMP, "STATION")
                            Case WF_OTFLG.ID
                                ' OT発送日報送信フラグ
                                prmData = work.CreateFIXParam(Master.USERCAMP, "OTFLG")

                            Case WF_TRAINCLASS.ID
                                ' 列車区分
                                prmData = work.CreateFIXParam(Master.USERCAMP, "TRAINCLASS")

                            Case WF_SPEEDCLASS.ID
                                ' 高速列車区分
                                prmData = work.CreateFIXParam(Master.USERCAMP, "SPEEDCLASS")

                            Case WF_SHIPORDERCLASS.ID
                                ' 発送順区分
                                prmData = work.CreateFIXParam(Master.USERCAMP, "SHIPORDERCLASS")

                            Case WF_USEDAYS.ID
                                ' 当日利用日数
                                prmData = work.CreateFIXParam(Master.USERCAMP, "USEDAYS")

                            Case WF_RUN.ID
                                ' 稼働フラグ
                                prmData = work.CreateFIXParam(Master.USERCAMP, "RUN")

                            Case WF_DELFLG.ID
                                ' 削除フラグ
                                prmData = work.CreateFIXParam(Master.USERCAMP, "DELFLG")

                            Case Else

                        End Select

                        .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                        .ActiveListBox()
                End Select
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
            Case WF_OFFICECODE.ID
                ' 管轄受注営業所
                CODENAME_get("OFFICECODE", WF_OFFICECODE.Text, WF_OFFICECODE_TEXT.Text, WW_RTN_SW)

            Case WF_TSUMI.ID
                ' 積置フラグ
                CODENAME_get("TSUMI", WF_TSUMI.Text, WF_TSUMI_TEXT.Text, WW_RTN_SW)

            Case WF_DEPSTATION.ID
                ' 発駅コード.ID
                CODENAME_get("STATION", WF_DEPSTATION.Text, WF_DEPSTATION_TEXT.Text, WW_RTN_SW)

            Case WF_ARRSTATION.ID
                ' 着駅コード
                CODENAME_get("STATION", WF_ARRSTATION.Text, WF_ARRSTATION_TEXT.Text, WW_RTN_SW)

            Case WF_OTFLG.ID
                ' OT発送日報送信フラグ
                CODENAME_get("OTFLG", WF_OTFLG.Text, WF_OTFLG_TEXT.Text, WW_RTN_SW)

            Case WF_TRAINCLASS.ID
                ' 列車区分
                CODENAME_get("TRAINCLASS", WF_TRAINCLASS.Text, WF_TRAINCLASS_TEXT.Text, WW_RTN_SW)

            Case WF_SPEEDCLASS.ID
                ' 高速列車区分
                CODENAME_get("SPEEDCLASS", WF_SPEEDCLASS.Text, WF_SPEEDCLASS_TEXT.Text, WW_RTN_SW)

            Case WF_SHIPORDERCLASS.ID
                ' 発送順区分
                CODENAME_get("SHIPORDERCLASS", WF_SHIPORDERCLASS.Text, WF_SHIPORDERCLASS_TEXT.Text, WW_RTN_SW)

            Case WF_USEDAYS.ID
                ' 在庫管理表表示ソート区分
                CODENAME_get("USEDAYS", WF_USEDAYS.Text, WF_USEDAYS_TEXT.Text, WW_RTN_SW)

            Case WF_RUN.ID
                ' 稼働フラグ
                CODENAME_get("RUN", WF_RUN.Text, WF_RUN_TEXT.Text, WW_RTN_SW)

            Case WF_DELFLG.ID
                ' 削除フラグ
                CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_RTN_SW)

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
                '削除フラグ
                Case "WF_OFFICECODE"
                    ' 管轄受注営業所
                    WF_OFFICECODE.Text = WW_SelectValue
                    WF_OFFICECODE_TEXT.Text = WW_SelectText
                    WF_OFFICECODE.Focus()

                Case "WF_TSUMI"
                    ' 積置フラグ
                    WF_TSUMI.Text = WW_SelectValue
                    WF_TSUMI_TEXT.Text = WW_SelectText
                    WF_TSUMI.Focus()

                Case "WF_DEPSTATION"
                    ' 発駅コード
                    WF_DEPSTATION.Text = WW_SelectValue
                    WF_DEPSTATION_TEXT.Text = WW_SelectText
                    WF_DEPSTATION.Focus()

                Case "WF_ARRSTATION"
                    ' 着駅コード
                    WF_ARRSTATION.Text = WW_SelectValue
                    WF_ARRSTATION_TEXT.Text = WW_SelectText
                    WF_ARRSTATION.Focus()

                Case "WF_OTFLG"
                    ' OT発送日報送信フラグ
                    WF_OTFLG.Text = WW_SelectValue
                    WF_OTFLG_TEXT.Text = WW_SelectText
                    WF_OTFLG.Focus()

                Case "WF_TRAINCLASS"
                    ' 列車区分
                    WF_TRAINCLASS.Text = WW_SelectValue
                    WF_TRAINCLASS_TEXT.Text = WW_SelectText
                    WF_TRAINCLASS.Focus()

                Case "WF_SPEEDCLASS"
                    ' 高速列車区分
                    WF_SPEEDCLASS.Text = WW_SelectValue
                    WF_SPEEDCLASS_TEXT.Text = WW_SelectText
                    WF_SPEEDCLASS.Focus()

                Case "WF_SHIPORDERCLASS"
                    ' 発送順区分
                    WF_SHIPORDERCLASS.Text = WW_SelectValue
                    WF_SHIPORDERCLASS_TEXT.Text = WW_SelectText
                    WF_SHIPORDERCLASS.Focus()

                Case "WF_USEDAYS"
                    ' 当日利用日数
                    WF_USEDAYS.Text = WW_SelectValue
                    WF_USEDAYS_TEXT.Text = WW_SelectText
                    WF_USEDAYS.Focus()

                Case "WF_RUN"
                    ' 稼働フラグ
                    WF_RUN.Text = WW_SelectValue
                    WF_RUN_TEXT.Text = WW_SelectText
                    WF_RUN.Focus()

                Case "WF_DELFLG"
                    ' 削除フラグ
                    WF_DELFLG.Text = WW_SelectValue
                    WF_DELFLG_TEXT.Text = WW_SelectText
                    WF_DELFLG.Focus()

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
                Case "WF_OFFICECODE"                    ' 管轄受注営業所
                    WF_OFFICECODE.Focus()
                Case "WF_TSUMI"                         ' 積置フラグ
                    WF_TSUMI.Focus()
                Case "WF_DEPSTATION"                    ' 発駅コード
                    WF_DEPSTATION.Focus()
                Case "WF_ARRSTATION"                    ' 着駅コード
                    WF_ARRSTATION.Focus()
                Case "WF_OTFLG"                         ' OT発送日報送信フラグ
                    WF_OTFLG.Focus()
                Case "WF_TRAINCLASS"                    ' 列車区分
                    WF_TRAINCLASS.Focus()
                Case "WF_SPEEDCLASS"                    ' 高速列車区分
                    WF_SPEEDCLASS.Focus()
                Case "WF_SHIPORDERCLASS"                ' 発送順区分
                    WF_SHIPORDERCLASS.Focus()
                Case "WF_USEDAYS"                       ' 当日利用日数
                    WF_USEDAYS.Focus()
                Case "WF_RUN"                           ' 稼働フラグ
                    WF_RUN.Focus()
                Case "WF_DELFLG"                        ' 削除フラグ
                    WF_DELFLG.Focus()
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
        Dim dateErrFlag As String = ""
        Dim WW_UniqueKeyCHECK As String = ""

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
        For Each OIM0007INProw As DataRow In OIM0007INPtbl.Rows

            WW_LINE_ERR = ""

            ' 管轄受注営業所（バリデーションチェック）
            WW_TEXT = OIM0007INProw("OFFICECODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OFFICECODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                ' 値存在チェックT
                CODENAME_get("OFFICECODE", OIM0007INProw("OFFICECODE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(管轄受注営業所エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(管轄受注営業所エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 本線列車番号（バリデーションチェック）
            WW_TEXT = OIM0007INProw("TRAINNO")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TRAINNO", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(本線列車番号エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 本線列車番号名（バリデーションチェック）
            WW_TEXT = OIM0007INProw("TRAINNAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TRAINNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(本線列車番号名エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 積置フラグ（バリデーションチェック）
            WW_TEXT = OIM0007INProw("TSUMI")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TSUMI", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                ' 値存在チェックT
                CODENAME_get("TSUMI", OIM0007INProw("TSUMI"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(積置フラグエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(積置フラグエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' OT列車番号（バリデーションチェック）
            WW_TEXT = OIM0007INProw("OTTRAINNO")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OTTRAINNO", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) OrElse OIM0007INProw("OTTRAINNO") Is DBNull.Value Then
                WW_CheckMES1 = "・更新できないレコード(OT列車番号エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' OT発送日報送信フラグ（バリデーションチェック）
            WW_TEXT = OIM0007INProw("OTFLG")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OTFLG", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                ' 値存在チェックT
                CODENAME_get("OTFLG", OIM0007INProw("OTFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(OT発送日報送信フラグエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(OT発送日報送信フラグエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 発駅コード（バリデーションチェック）
            WW_TEXT = OIM0007INProw("DEPSTATION")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DEPSTATION", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                ' 値存在チェックT
                CODENAME_get("STATION", OIM0007INProw("DEPSTATION"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(発駅コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(発駅コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 着駅コード（バリデーションチェック）
            WW_TEXT = OIM0007INProw("ARRSTATION")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ARRSTATION", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                ' 値存在チェックT
                CODENAME_get("STATION", OIM0007INProw("ARRSTATION"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(着駅コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(着駅コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JR発列車番号（バリデーションチェック）
            WW_TEXT = OIM0007INProw("JRTRAINNO1")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JRTRAINNO1", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JR発列車番号エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JR発列車牽引車数（バリデーションチェック）
            WW_TEXT = OIM0007INProw("MAXTANK1")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "MAXTANK1", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JR発列車牽引車数エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JR中継列車番号（バリデーションチェック）
            WW_TEXT = OIM0007INProw("JRTRAINNO2")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JRTRAINNO2", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JR中継列車番号エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JR中継列車牽引車数（バリデーションチェック）
            WW_TEXT = OIM0007INProw("MAXTANK2")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "MAXTANK2", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JR中継列車牽引車数エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JR最終列車番号（バリデーションチェック）
            WW_TEXT = OIM0007INProw("JRTRAINNO3")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JRTRAINNO3", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JR最終列車番号エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JR最終列車牽引車数（バリデーションチェック）
            WW_TEXT = OIM0007INProw("MAXTANK3")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "MAXTANK3", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JR最終列車牽引車数エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 列車区分（バリデーションチェック）
            WW_TEXT = OIM0007INProw("TRAINCLASS")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TRAINCLASS", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                ' 値存在チェックT
                CODENAME_get("TRAINCLASS", OIM0007INProw("TRAINCLASS"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(列車区分エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(列車区分エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 高速列車区分（バリデーションチェック）
            WW_TEXT = OIM0007INProw("SPEEDCLASS")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SPEEDCLASS", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                ' 値存在チェックT
                CODENAME_get("SPEEDCLASS", OIM0007INProw("SPEEDCLASS"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(高速列車区分エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(高速列車区分エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 発送順区分（バリデーションチェック）
            WW_TEXT = OIM0007INProw("SHIPORDERCLASS")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SHIPORDERCLASS", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                ' 値存在チェックT
                CODENAME_get("SHIPORDERCLASS", OIM0007INProw("SHIPORDERCLASS"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(発送順区分エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(発送順区分エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 発日日数（バリデーションチェック）
            WW_TEXT = OIM0007INProw("DEPDAYS")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DEPDAYS", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(発日日数エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 特継日数（バリデーションチェック）
            WW_TEXT = OIM0007INProw("MARGEDAYS")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "MARGEDAYS", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(特継日数エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 積車着日数（バリデーションチェック）
            WW_TEXT = OIM0007INProw("ARRDAYS")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ARRDAYS", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(積車着日数エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 受入日数（バリデーションチェック）
            WW_TEXT = OIM0007INProw("ACCDAYS")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ACCDAYS", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(受入日数エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 空車着日数（バリデーションチェック）
            WW_TEXT = OIM0007INProw("EMPARRDAYS")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "EMPARRDAYS", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(空車着日数エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 当日利用日数（バリデーションチェック）
            WW_TEXT = OIM0007INProw("USEDAYS")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "USEDAYS", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                ' 値存在チェックT
                CODENAME_get("USEDAYS", OIM0007INProw("USEDAYS"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(当日利用日数エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(当日利用日数エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 料金マスタ区分（バリデーションチェック）
            WW_TEXT = OIM0007INProw("FEEKBN")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "FEEKBN", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(料金マスタ区分エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 稼働フラグ（バリデーションチェック）
            WW_TEXT = OIM0007INProw("RUN")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "RUN", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                ' 値存在チェックT
                CODENAME_get("RUN", OIM0007INProw("RUN"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(稼働フラグエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(稼働フラグエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 在庫管理表表示ソート区分（バリデーションチェック）
            WW_TEXT = OIM0007INProw("ZAIKOSORT")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ZAIKOSORT", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(在庫管理表表示ソート区分エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 備考（バリデーションチェック）
            WW_TEXT = OIM0007INProw("BIKOU")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "BIKOU", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(備考エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 削除フラグ（バリデーションチェック）
            WW_TEXT = OIM0007INProw("DELFLG")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DELFLG", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                ' 値存在チェックT
                CODENAME_get("DELFLG", OIM0007INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除フラグエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除フラグエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If


            '一意制約チェック
            '同一レコードの更新の場合、チェック対象外
            If OIM0007INProw("OFFICECODE") = work.WF_SEL_OFFICECODE2.Text AndAlso
                OIM0007INProw("TRAINNO") = work.WF_SEL_TRAINNO2.Text AndAlso
                OIM0007INProw("TSUMI") = work.WF_SEL_TSUMI2.Text AndAlso
                OIM0007INProw("OTTRAINNO") = work.WF_SEL_OTTRAINNO.Text AndAlso
                OIM0007INProw("DEPSTATION") = work.WF_SEL_DEPSTATION.Text AndAlso
                OIM0007INProw("ARRSTATION") = work.WF_SEL_ARRSTATION.Text Then

            Else
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    'DataBase接続
                    SQLcon.Open()

                    '一意制約チェック
                    UniqueKeyCheck(SQLcon, WW_UniqueKeyCHECK)
                End Using

                If Not isNormal(WW_UniqueKeyCHECK) Then
                    WW_CheckMES1 = "一意制約違反"
                    WW_CheckMES2 = C_MESSAGE_NO.OVERLAP_DATA_ERROR
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR
                End If
            End If


            If WW_LINE_ERR = "" Then
                If OIM0007INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    OIM0007INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LINE_ERR = CONST_PATTERNERR Then
                    '関連チェックエラーをセット
                    OIM0007INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    '単項目チェックエラーをセット
                    OIM0007INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
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
    ''' <param name="OIM0007row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIM0007row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIM0007row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 管轄受注営業所 =" & OIM0007row("OFFICECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 本線列車番号 =" & OIM0007row("TRAINNO") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 本線列車番号名 =" & OIM0007row("TRAINNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 積置フラグ =" & OIM0007row("TSUMI") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> OT列車番号 =" & OIM0007row("OTTRAINNO") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> OT発送日報送信フラグ =" & OIM0007row("OTFLG") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 発駅コード =" & OIM0007row("DEPSTATION") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 着駅コード =" & OIM0007row("ARRSTATION") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JR発列車番号 =" & OIM0007row("JRTRAINNO1") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JR発列車牽引車数 =" & OIM0007row("MAXTANK1") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JR中継列車番号 =" & OIM0007row("JRTRAINNO2") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JR中継列車牽引車数 =" & OIM0007row("MAXTANK2") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JR最終列車番号 =" & OIM0007row("JRTRAINNO3") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JR最終列車牽引車数 =" & OIM0007row("MAXTANK3") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 列車区分 =" & OIM0007row("TRAINCLASS") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 高速列車区分 =" & OIM0007row("SPEEDCLASS") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 発送順区分 =" & OIM0007row("SHIPORDERCLASS") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 発日日数 =" & OIM0007row("DEPDAYS") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 特継日数 =" & OIM0007row("MARGEDAYS") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 積車着日数 =" & OIM0007row("ARRDAYS") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 受入日数 =" & OIM0007row("ACCDAYS") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 空車着日数 =" & OIM0007row("EMPARRDAYS") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 当日利用日数 =" & OIM0007row("USEDAYS") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 料金マスタ区分 =" & OIM0007row("FEEKBN") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 稼働フラグ =" & OIM0007row("RUN") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 在庫管理表表示ソート区分 =" & OIM0007row("ZAIKOSORT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 備考 =" & OIM0007row("BIKOU") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除フラグ =" & OIM0007row("DELFLG")
        End If

        rightview.AddErrorReport(WW_ERR_MES)

    End Sub

    ''' <summary>
    ''' OIM0007tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub OIM0007tbl_UPD()

        '○ 画面状態設定
        For Each OIM0007row As DataRow In OIM0007tbl.Rows
            Select Case OIM0007row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0007row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0007row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each OIM0007INProw As DataRow In OIM0007INPtbl.Rows

            'エラーレコード読み飛ばし
            If OIM0007INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            OIM0007INProw.Item("OPERATION") = CONST_INSERT

            ' 既存レコードとの比較
            For Each OIM0007row As DataRow In OIM0007tbl.Rows
                ' KEY項目が等しい時
                If OIM0007row("OFFICECODE") = OIM0007INProw("OFFICECODE") AndAlso
                    OIM0007row("TRAINNO") = OIM0007INProw("TRAINNO") AndAlso
                    OIM0007row("TSUMI") = OIM0007INProw("TSUMI") AndAlso
                    OIM0007row("OTTRAINNO") = OIM0007INProw("OTTRAINNO") AndAlso
                    OIM0007row("DEPSTATION") = OIM0007INProw("DEPSTATION") AndAlso
                    OIM0007row("ARRSTATION") = OIM0007INProw("ARRSTATION") Then
                    ' KEY項目以外の項目に変更があるかチェック
                    If OIM0007row("TRAINNAME") = OIM0007INProw("TRAINNAME") AndAlso
                        OIM0007row("OTFLG") = OIM0007INProw("OTFLG") AndAlso
                        OIM0007row("JRTRAINNO1") = OIM0007INProw("JRTRAINNO1") AndAlso
                        OIM0007row("MAXTANK1") = OIM0007INProw("MAXTANK1") AndAlso
                        OIM0007row("JRTRAINNO2") = OIM0007INProw("JRTRAINNO2") AndAlso
                        OIM0007row("MAXTANK2") = OIM0007INProw("MAXTANK2") AndAlso
                        OIM0007row("JRTRAINNO3") = OIM0007INProw("JRTRAINNO3") AndAlso
                        OIM0007row("MAXTANK3") = OIM0007INProw("MAXTANK3") AndAlso
                        OIM0007row("TRAINCLASS") = OIM0007INProw("TRAINCLASS") AndAlso
                        OIM0007row("SPEEDCLASS") = OIM0007INProw("SPEEDCLASS") AndAlso
                        OIM0007row("SHIPORDERCLASS") = OIM0007INProw("SHIPORDERCLASS") AndAlso
                        OIM0007row("DEPDAYS") = OIM0007INProw("DEPDAYS") AndAlso
                        OIM0007row("MARGEDAYS") = OIM0007INProw("MARGEDAYS") AndAlso
                        OIM0007row("ARRDAYS") = OIM0007INProw("ARRDAYS") AndAlso
                        OIM0007row("ACCDAYS") = OIM0007INProw("ACCDAYS") AndAlso
                        OIM0007row("EMPARRDAYS") = OIM0007INProw("EMPARRDAYS") AndAlso
                        OIM0007row("USEDAYS") = OIM0007INProw("USEDAYS") AndAlso
                        OIM0007row("FEEKBN") = OIM0007INProw("FEEKBN") AndAlso
                        OIM0007row("RUN") = OIM0007INProw("RUN") AndAlso
                        OIM0007row("ZAIKOSORT") = OIM0007INProw("ZAIKOSORT") AndAlso
                        OIM0007row("BIKOU") = OIM0007INProw("BIKOU") AndAlso
                        OIM0007row("DELFLG") = OIM0007INProw("DELFLG") AndAlso
                        Not C_LIST_OPERATION_CODE.UPDATING.Equals(OIM0007row("OPERATION")) Then
                        ' 変更がないときは「操作」の項目は空白にする
                        OIM0007INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Else
                        ' 変更がある時は「操作」の項目を「更新」に設定する
                        OIM0007INProw("OPERATION") = CONST_UPDATE
                    End If

                    Exit For

                End If
            Next
        Next

        '更新チェック
        If C_LIST_OPERATION_CODE.NODATA.Equals(OIM0007INPtbl.Rows(0)("OPERATION")) Then
            '更新なしの場合、エラーコードに変更なしエラーをセットして処理打ち切り
            WW_ERRCODE = C_MESSAGE_NO.NO_CHANGE_UPDATE
            Exit Sub
        ElseIf CONST_UPDATE.Equals(OIM0007INPtbl.Rows(0)("OPERATION")) OrElse
            CONST_INSERT.Equals(OIM0007INPtbl.Rows(0)("OPERATION")) Then
            '追加/更新の場合、DB更新処理
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                'DataBase接続
                SQLcon.Open()

                'マスタ更新
                UpdateMaster(SQLcon)

                work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = "Update Success!!"
            End Using
        End If

        '○ 変更有無判定　&　入力値反映
        For Each OIM0007INProw As DataRow In OIM0007INPtbl.Rows
            '発見フラグ
            Dim isFound As Boolean = False

            For Each OIM0007row As DataRow In OIM0007tbl.Rows

                '同一レコードか判定
                If OIM0007INProw("OFFICECODE") = OIM0007row("OFFICECODE") AndAlso
                    OIM0007INProw("TRAINNO") = OIM0007row("TRAINNO") AndAlso
                    OIM0007INProw("TSUMI") = OIM0007row("TSUMI") AndAlso
                    OIM0007INProw("OTTRAINNO") = OIM0007row("OTTRAINNO") AndAlso
                    OIM0007INProw("DEPSTATION") = OIM0007row("DEPSTATION") AndAlso
                    OIM0007INProw("ARRSTATION") = OIM0007row("ARRSTATION") Then
                    '画面入力テーブル項目設定
                    OIM0007INProw("LINECNT") = OIM0007row("LINECNT")
                    OIM0007INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    OIM0007INProw("UPDTIMSTP") = OIM0007row("UPDTIMSTP")
                    OIM0007INProw("SELECT") = 0
                    OIM0007INProw("HIDDEN") = 0

                    '項目テーブル項目設定
                    OIM0007row.ItemArray = OIM0007INProw.ItemArray

                    '〇名称設定
                    '管轄営業所(名)
                    If Not String.IsNullOrEmpty(OIM0007row("OFFICECODE")) Then
                        CODENAME_get("OFFICECODE", OIM0007row("OFFICECODE"), OIM0007row("OFFICENAME"), WW_DUMMY)
                    End If
                    '積置フラグ(名)
                    If Not String.IsNullOrEmpty(OIM0007row("TSUMI")) Then
                        CODENAME_get("TSUMI", OIM0007row("TSUMI"), OIM0007row("TSUMINAME"), WW_DUMMY)
                    End If
                    'OT発送日報送信フラグ(名)
                    If Not String.IsNullOrEmpty(OIM0007row("OTFLG")) Then
                        CODENAME_get("OTFLG", OIM0007row("OTFLG"), OIM0007row("OTFLGNAME"), WW_DUMMY)
                    End If
                    '発駅(名)
                    If Not String.IsNullOrEmpty(OIM0007row("DEPSTATION")) Then
                        CODENAME_get("STATION", OIM0007row("DEPSTATION"), OIM0007row("DEPSTATIONNAME"), WW_DUMMY)
                    End If
                    '着駅(名)
                    If Not String.IsNullOrEmpty(OIM0007row("ARRSTATION")) Then
                        CODENAME_get("STATION", OIM0007row("ARRSTATION"), OIM0007row("ARRSTATIONNAME"), WW_DUMMY)
                    End If
                    '列車区分(名)
                    If Not String.IsNullOrEmpty(OIM0007row("TRAINCLASS")) Then
                        CODENAME_get("TRAINCLASS", OIM0007row("TRAINCLASS"), OIM0007row("TRAINCLASSNAME"), WW_DUMMY)
                    End If
                    '高速列車区分(名)
                    If Not String.IsNullOrEmpty(OIM0007row("SPEEDCLASS")) Then
                        CODENAME_get("SPEEDCLASS", OIM0007row("SPEEDCLASS"), OIM0007row("SPEEDCLASSNAME"), WW_DUMMY)
                    End If
                    '発送順区分(名)
                    If Not String.IsNullOrEmpty(OIM0007row("SHIPORDERCLASS")) Then
                        CODENAME_get("SHIPORDERCLASS", OIM0007row("SHIPORDERCLASS"), OIM0007row("SHIPORDERCLASSNAME"), WW_DUMMY)
                    End If
                    '当日利用日数(名)
                    If Not String.IsNullOrEmpty(OIM0007row("USEDAYS")) Then
                        CODENAME_get("USEDAYS", OIM0007row("USEDAYS"), OIM0007row("USEDAYSNAME"), WW_DUMMY)
                    End If
                    '稼働フラグ(名)
                    If Not String.IsNullOrEmpty(OIM0007row("RUN")) Then
                        CODENAME_get("RUN", OIM0007row("RUN"), OIM0007row("RUNNAME"), WW_DUMMY)
                    End If

                    '発見フラグON
                    isFound = True
                    Exit For
                End If
            Next

            '同一レコードが発見できない場合は、追加する
            If Not isFound Then
                Dim nrow = OIM0007tbl.NewRow
                nrow.ItemArray = OIM0007INProw.ItemArray

                '画面入力テーブル項目設定
                nrow("LINECNT") = OIM0007tbl.Rows.Count + 1
                nrow("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                nrow("UPDTIMSTP") = "0"
                nrow("SELECT") = 0
                nrow("HIDDEN") = 0

                '〇名称設定
                '管轄営業所(名)
                If Not String.IsNullOrEmpty(nrow("OFFICECODE")) Then
                    CODENAME_get("OFFICECODE", nrow("OFFICECODE"), nrow("OFFICENAME"), WW_DUMMY)
                End If
                '積置フラグ(名)
                If Not String.IsNullOrEmpty(nrow("TSUMI")) Then
                    CODENAME_get("TSUMI", nrow("TSUMI"), nrow("TSUMINAME"), WW_DUMMY)
                End If
                'OT発送日報送信フラグ(名)
                If Not String.IsNullOrEmpty(nrow("OTFLG")) Then
                    CODENAME_get("OTFLG", nrow("OTFLG"), nrow("OTFLGNAME"), WW_DUMMY)
                End If
                '発駅(名)
                If Not String.IsNullOrEmpty(nrow("DEPSTATION")) Then
                    CODENAME_get("STATION", nrow("DEPSTATION"), nrow("DEPSTATIONNAME"), WW_DUMMY)
                End If
                '着駅(名)
                If Not String.IsNullOrEmpty(nrow("ARRSTATION")) Then
                    CODENAME_get("STATION", nrow("ARRSTATION"), nrow("ARRSTATIONNAME"), WW_DUMMY)
                End If
                '列車区分(名)
                If Not String.IsNullOrEmpty(nrow("TRAINCLASS")) Then
                    CODENAME_get("TRAINCLASS", nrow("TRAINCLASS"), nrow("TRAINCLASSNAME"), WW_DUMMY)
                End If
                '高速列車区分(名)
                If Not String.IsNullOrEmpty(nrow("SPEEDCLASS")) Then
                    CODENAME_get("SPEEDCLASS", nrow("SPEEDCLASS"), nrow("SPEEDCLASSNAME"), WW_DUMMY)
                End If
                '発送順区分(名)
                If Not String.IsNullOrEmpty(nrow("SHIPORDERCLASS")) Then
                    CODENAME_get("SHIPORDERCLASS", nrow("SHIPORDERCLASS"), nrow("SHIPORDERCLASSNAME"), WW_DUMMY)
                End If
                '当日利用日数(名)
                If Not String.IsNullOrEmpty(nrow("USEDAYS")) Then
                    CODENAME_get("USEDAYS", nrow("USEDAYS"), nrow("USEDAYSNAME"), WW_DUMMY)
                End If
                '稼働フラグ(名)
                If Not String.IsNullOrEmpty(nrow("RUN")) Then
                    CODENAME_get("RUN", nrow("RUN"), nrow("RUNNAME"), WW_DUMMY)
                End If

                OIM0007tbl.Rows.Add(nrow)
            End If
        Next

    End Sub

#Region "未使用"
    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="OIM0007INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef OIM0007INProw As DataRow)

        For Each OIM0007row As DataRow In OIM0007tbl.Rows

            '同一レコードか判定
            If OIM0007INProw("OFFICECODE") = OIM0007row("OFFICECODE") AndAlso
                OIM0007INProw("TRAINNO") = OIM0007row("TRAINNO") AndAlso
                OIM0007INProw("TSUMI") = OIM0007row("TSUMI") AndAlso
                OIM0007INProw("OTTRAINNO") = OIM0007row("OTTRAINNO") AndAlso
                OIM0007INProw("DEPSTATION") = OIM0007row("DEPSTATION") AndAlso
                OIM0007INProw("ARRSTATION") = OIM0007row("ARRSTATION") Then
                '画面入力テーブル項目設定
                OIM0007INProw("LINECNT") = OIM0007row("LINECNT")
                OIM0007INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                OIM0007INProw("UPDTIMSTP") = OIM0007row("UPDTIMSTP")
                OIM0007INProw("SELECT") = 1
                OIM0007INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0007row.ItemArray = OIM0007INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 追加予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0007INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef OIM0007INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim OIM0007row As DataRow = OIM0007tbl.NewRow
        OIM0007row.ItemArray = OIM0007INProw.ItemArray

        OIM0007row("LINECNT") = OIM0007tbl.Rows.Count + 1
        If OIM0007INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
            OIM0007row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        Else
            OIM0007row("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
        End If

        OIM0007row("UPDTIMSTP") = "0"
        OIM0007row("SELECT") = 1
        OIM0007row("HIDDEN") = 0

        OIM0007tbl.Rows.Add(OIM0007row)

    End Sub

    ''' <summary>
    ''' エラーデータの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0007INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_ERR_SUB(ByRef OIM0007INProw As DataRow)

        For Each OIM0007row As DataRow In OIM0007tbl.Rows

            '同一レコードか判定
            If OIM0007INProw("OFFICECODE") = OIM0007row("OFFICECODE") AndAlso
                OIM0007INProw("TRAINNO") = OIM0007row("TRAINNO") AndAlso
                OIM0007INProw("TSUMI") = OIM0007row("TSUMI") AndAlso
                OIM0007INProw("OTTRAINNO") = OIM0007row("OTTRAINNO") AndAlso
                OIM0007INProw("DEPSTATION") = OIM0007row("DEPSTATION") AndAlso
                OIM0007INProw("ARRSTATION") = OIM0007row("ARRSTATION") Then
                '画面入力テーブル項目設定
                OIM0007INProw("LINECNT") = OIM0007row("LINECNT")
                OIM0007INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                OIM0007INProw("UPDTIMSTP") = OIM0007row("UPDTIMSTP")
                OIM0007INProw("SELECT") = 1
                OIM0007INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0007row.ItemArray = OIM0007INProw.ItemArray
                Exit For
            End If
        Next

    End Sub
#End Region

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
                Case "OFFICECODE"
                    ' 管轄受注営業所
                    prmData = work.CreateOfficeCodeParam(Master.USER_ORG)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SALESOFFICE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "TRAINNO"
                    ' 本線列車番号
                    prmData = work.CreateTrainNoParam(work.WF_SEL_OFFICECODE.Text, I_VALUE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_TRAINNUMBER, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "TSUMI"
                    ' 積置フラグ
                    prmData = work.CreateFIXParam(Master.USERCAMP, "TSUMI")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STATION"
                    ' 駅
                    prmData = work.CreateFIXParam(Master.USERCAMP)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATIONCODE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "OTFLG"
                    ' OT発送日報送信フラグ
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "OTFLG")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "TRAINCLASS"
                    ' 列車区分
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "TRAINCLASS")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_TRAINCLASS, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "SPEEDCLASS"
                    ' 高速列車区分
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SPEEDCLASS, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "SHIPORDERCLASS"
                    ' 発送順区分
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "SHIPORDERCLASS")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "USEDAYS"
                    ' 当日利用日数
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "USEDAYS")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "RUN"
                    ' 稼働フラグ
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "RUN")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "DELFLG"
                    ' 削除
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DELFLG")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
