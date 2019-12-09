﻿''************************************************************
' タンク車マスタメンテ登録画面
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
Public Class OIM0005TankCreate
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
                    Master.RecoverTable(OIM0005tbl, work.WF_SEL_INPTBL.Text)

                    Select Case WF_ButtonClick.Value
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
        Master.MAPID = OIM0005WRKINC.MAPIDC
        '○HELP表示有無設定
        Master.dispHelp = False
        '○D&D有無設定
        Master.eventDrop = True
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

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0005L Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        End If

        '○ 名称設定処理
        '選択行
        WF_Sel_LINECNT.Text = work.WF_SEL_LINECNT.Text

        'JOT車番
        WF_TANKNUMBER.Text = work.WF_SEL_TANKNUMBER2.Text
        CODENAME_get("TANKNUMBER", WF_TANKNUMBER.Text, WF_TANKNUMBER_TEXT.Text, WW_DUMMY)

        '原籍所有者C
        WF_ORIGINOWNERCODE.Text = work.WF_SEL_ORIGINOWNERCODE.Text

        '名義所有者C
        WF_OWNERCODE.Text = work.WF_SEL_OWNERCODE.Text

        'リース先C
        WF_LEASECODE.Text = work.WF_SEL_LEASECODE.Text

        'リース区分C
        WF_LEASECLASS.Text = work.WF_SEL_LEASECLASS.Text

        '自動延長
        WF_AUTOEXTENTION.Text = work.WF_SEL_AUTOEXTENTION.Text

        'リース開始年月日
        WF_LEASESTYMD.Text = work.WF_SEL_LEASESTYMD.Text

        'リース満了年月日
        WF_LEASEENDYMD.Text = work.WF_SEL_LEASEENDYMD.Text

        '第三者使用者C
        WF_USERCODE.Text = work.WF_SEL_USERCODE.Text

        '原常備駅C
        WF_CURRENTSTATIONCODE.Text = work.WF_SEL_CURRENTSTATIONCODE.Text

        '臨時常備駅C
        WF_EXTRADINARYSTATIONCODE.Text = work.WF_SEL_EXTRADINARYSTATIONCODE.Text

        '第三者使用期限
        WF_USERLIMIT.Text = work.WF_SEL_USERLIMIT.Text

        '臨時常備駅期限
        WF_LIMITTEXTRADIARYSTATION.Text = work.WF_SEL_LIMITTEXTRADIARYSTATION.Text

        '原専用種別C
        WF_DEDICATETYPECODE.Text = work.WF_SEL_DEDICATETYPECODE.Text

        '臨時専用種別C
        WF_EXTRADINARYTYPECODE.Text = work.WF_SEL_EXTRADINARYTYPECODE.Text

        '臨時専用期限
        WF_EXTRADINARYLIMIT.Text = work.WF_SEL_EXTRADINARYLIMIT.Text

        '運用基地C
        WF_OPERATIONBASECODE.Text = work.WF_SEL_OPERATIONBASECODE.Text

        '塗色C
        WF_COLORCODE.Text = work.WF_SEL_COLORCODE.Text

        'エネオス
        WF_ENEOS.Text = work.WF_SEL_ENEOS.Text

        'エコレール
        WF_ECO.Text = work.WF_SEL_ECO.Text

        '取得年月日
        WF_ALLINSPECTIONDATE.Text = work.WF_SEL_ALLINSPECTIONDATE.Text

        '車籍編入年月日
        WF_TRANSFERDATE.Text = work.WF_SEL_TRANSFERDATE.Text

        '取得先C
        WF_OBTAINEDCODE.Text = work.WF_SEL_OBTAINEDCODE.Text

        '形式
        WF_MODEL.Text = work.WF_SEL_MODEL2.Text

        '形式カナ
        WF_MODELKANA.Text = work.WF_SEL_MODELKANA.Text

        '荷重
        WF_LOAD.Text = work.WF_SEL_LOAD.Text

        '荷重単位
        WF_LOADUNIT.Text = work.WF_SEL_LOADUNIT.Text

        '容積
        WF_VOLUME.Text = work.WF_SEL_VOLUME.Text

        '容積単位
        WF_VOLUMEUNIT.Text = work.WF_SEL_VOLUMEUNIT.Text

        '原籍所有者
        WF_ORIGINOWNERNAME.Text = work.WF_SEL_ORIGINOWNERNAME.Text

        '名義所有者
        WF_OWNERNAME.Text = work.WF_SEL_OWNERNAME.Text

        'リース先
        WF_LEASENAME.Text = work.WF_SEL_LEASENAME.Text

        'リース区分
        WF_LEASECLASSNEMAE.Text = work.WF_SEL_LEASECLASSNEMAE.Text

        '第三者使用者
        WF_USERNAME.Text = work.WF_SEL_USERNAME.Text

        '原常備駅
        WF_CURRENTSTATIONNAME.Text = work.WF_SEL_CURRENTSTATIONNAME.Text

        '臨時常備駅
        WF_EXTRADINARYSTATIONNAME.Text = work.WF_SEL_EXTRADINARYSTATIONNAME.Text

        '原専用種別
        WF_DEDICATETYPENAME.Text = work.WF_SEL_DEDICATETYPENAME.Text

        '臨時専用種別
        WF_EXTRADINARYTYPENAME.Text = work.WF_SEL_EXTRADINARYTYPENAME.Text

        '運用場所
        WF_OPERATIONBASENAME.Text = work.WF_SEL_OPERATIONBASENAME.Text

        '塗色
        WF_COLORNAME.Text = work.WF_SEL_COLORNAME.Text

        '予備1
        WF_RESERVE1.Text = work.WF_SEL_RESERVE1.Text

        '予備2
        WF_RESERVE2.Text = work.WF_SEL_RESERVE2.Text

        '次回指定年月日
        WF_SPECIFIEDDATE.Text = work.WF_SEL_SPECIFIEDDATE.Text

        '次回全検年月日(JR) 
        WF_JRALLINSPECTIONDATE.Text = work.WF_SEL_JRALLINSPECTIONDATE.Text

        '現在経年
        WF_PROGRESSYEAR.Text = work.WF_SEL_PROGRESSYEAR.Text

        '次回全検時経年
        WF_NEXTPROGRESSYEAR.Text = work.WF_SEL_NEXTPROGRESSYEAR.Text

        '次回交検年月日(JR）
        WF_JRINSPECTIONDATE.Text = work.WF_SEL_JRINSPECTIONDATE.Text

        '次回交検年月日
        WF_INSPECTIONDATE.Text = work.WF_SEL_INSPECTIONDATE.Text

        '次回指定年月日(JR)
        WF_JRSPECIFIEDDATE.Text = work.WF_SEL_JRSPECIFIEDDATE.Text

        'JR車番
        WF_JRTANKNUMBER.Text = work.WF_SEL_JRTANKNUMBER.Text

        '旧JOT車番
        WF_OLDTANKNUMBER.Text = work.WF_SEL_OLDTANKNUMBER.Text

        'OT車番
        WF_OTTANKNUMBER.Text = work.WF_SEL_OTTANKNUMBER.Text

        'JXTG車番
        WF_JXTGTANKNUMBER.Text = work.WF_SEL_JXTGTANKNUMBER.Text

        'コスモ車番
        WF_COSMOTANKNUMBER.Text = work.WF_SEL_COSMOTANKNUMBER.Text

        '富士石油車番
        WF_FUJITANKNUMBER.Text = work.WF_SEL_FUJITANKNUMBER.Text

        '出光昭シ車番
        WF_SHELLTANKNUMBER.Text = work.WF_SEL_SHELLTANKNUMBER.Text

        '予備
        WF_RESERVE3.Text = work.WF_SEL_RESERVE3.Text

        '削除
        WF_DELFLG.Text = work.WF_SEL_DELFLG.Text
        CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_DUMMY)

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
        Dim SQLStr As String =
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
            & " FROM OIL.OIM0005_TANK OIM0005 "

        If work.WF_SEL_TANKNUMBER.Text = "" And
            work.WF_SEL_MODEL.Text = "" Then
            SQLStr &=
              " WHERE OIM0005.DELFLG      <> @P3"
        Else
            SQLStr &=
              " WHERE OIM0005.TANKNUMBER = @P1" _
            & "   OR OIM0005.MODEL = @P2" _
            & "   AND OIM0005.DELFLG      <> @P3"
        End If

        SQLStr &=
              " ORDER BY" _
            & "    RIGHT('0000000000' + CAST(OIM0005.TANKNUMBER AS NVARCHAR), 10)"

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
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0005C")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0005C Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 一意制約チェック
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub UniqueKeyCheck(ByVal SQLcon As SqlConnection, ByRef O_MESSAGENO As String)

        '○ 対象データ取得
        Dim SQLStr As String =
              " SELECT " _
            & "     TANKNUMBER " _
            & " FROM" _
            & "    OIL.OIM0005_TANK" _
            & " WHERE" _
            & "     TANKNUMBER      = @P01"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20)            'JOT車番
                PARA1.Value = WF_TANKNUMBER.Text

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    Dim OIM0005Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIM0005Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIM0005Chk.Load(SQLdr)

                    If OIM0005Chk.Rows.Count > 0 Then
                        '重複データエラー
                        O_MESSAGENO = Messages.C_MESSAGE_NO.OVERLAP_DATA_ERROR
                    Else
                        '正常終了時
                        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0005C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0005C UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_Scroll()

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
        Master.SaveTable(OIM0005tbl, work.WF_SEL_INPTBL.Text)

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

        Master.CreateEmptyTable(OIM0005INPtbl, work.WF_SEL_INPTBL.Text)
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

        OIM0005INProw("MODELKANA") = WF_MODELKANA.Text              '形式カナ

        OIM0005INProw("LOAD") = WF_LOAD.Text              '荷重

        OIM0005INProw("LOADUNIT") = WF_LOADUNIT.Text              '荷重単位

        OIM0005INProw("VOLUME") = WF_VOLUME.Text              '容積

        OIM0005INProw("VOLUMEUNIT") = WF_VOLUMEUNIT.Text              '容積単位

        OIM0005INProw("ORIGINOWNERNAME") = WF_ORIGINOWNERNAME.Text              '原籍所有者

        OIM0005INProw("OWNERNAME") = WF_OWNERNAME.Text              '名義所有者

        OIM0005INProw("LEASENAME") = WF_LEASENAME.Text              'リース先

        OIM0005INProw("LEASECLASSNEMAE") = WF_LEASECLASSNEMAE.Text              'リース区分

        OIM0005INProw("USERNAME") = WF_USERNAME.Text              '第三者使用者

        OIM0005INProw("CURRENTSTATIONNAME") = WF_CURRENTSTATIONNAME.Text              '原常備駅

        OIM0005INProw("EXTRADINARYSTATIONNAME") = WF_EXTRADINARYSTATIONNAME.Text              '臨時常備駅

        OIM0005INProw("DEDICATETYPENAME") = WF_DEDICATETYPENAME.Text              '原専用種別

        OIM0005INProw("EXTRADINARYTYPENAME") = WF_EXTRADINARYTYPENAME.Text              '臨時専用種別

        OIM0005INProw("OPERATIONBASENAME") = WF_OPERATIONBASENAME.Text              '運用場所

        OIM0005INProw("COLORNAME") = WF_COLORNAME.Text              '塗色

        OIM0005INProw("RESERVE1") = WF_RESERVE1.Text              '予備1

        OIM0005INProw("RESERVE2") = WF_RESERVE2.Text              '予備2

        OIM0005INProw("SPECIFIEDDATE") = WF_SPECIFIEDDATE.Text              '次回指定年月日

        OIM0005INProw("JRALLINSPECTIONDATE") = WF_JRALLINSPECTIONDATE.Text              '次回全検年月日(JR) 

        OIM0005INProw("PROGRESSYEAR") = WF_PROGRESSYEAR.Text              '現在経年

        OIM0005INProw("NEXTPROGRESSYEAR") = WF_NEXTPROGRESSYEAR.Text              '次回全検時経年

        OIM0005INProw("JRINSPECTIONDATE") = WF_JRINSPECTIONDATE.Text              '次回交検年月日(JR）

        OIM0005INProw("INSPECTIONDATE") = WF_INSPECTIONDATE.Text              '次回交検年月日

        OIM0005INProw("JRSPECIFIEDDATE") = WF_JRSPECIFIEDDATE.Text              '次回指定年月日(JR)

        OIM0005INProw("JRTANKNUMBER") = WF_JRTANKNUMBER.Text              'JR車番

        OIM0005INProw("OLDTANKNUMBER") = WF_OLDTANKNUMBER.Text              '旧JOT車番

        OIM0005INProw("OTTANKNUMBER") = WF_OTTANKNUMBER.Text              'OT車番

        OIM0005INProw("JXTGTANKNUMBER") = WF_JXTGTANKNUMBER.Text              'JXTG車番

        OIM0005INProw("COSMOTANKNUMBER") = WF_COSMOTANKNUMBER.Text              'コスモ車番

        OIM0005INProw("FUJITANKNUMBER") = WF_FUJITANKNUMBER.Text              '富士石油車番

        OIM0005INProw("SHELLTANKNUMBER") = WF_SHELLTANKNUMBER.Text              '出光昭シ車番

        OIM0005INProw("RESERVE3") = WF_RESERVE3.Text              '予備

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
        Master.SaveTable(OIM0005tbl, work.WF_SEL_INPTBL.Text)

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
                Select Case WF_LeftMViewChange.Value
                    Case LIST_BOX_CLASSIFICATION.LC_CALENDAR
                        '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                        Select Case WF_FIELD.Value
                            Case "WF_LEASESTYMD"         'リース開始年月日
                                .WF_Calendar.Text = WF_LEASESTYMD.Text
                            Case "WF_LEASEENDYMD"         'リース満了年月日
                                .WF_Calendar.Text = WF_LEASEENDYMD.Text
                            Case "WF_USERLIMIT"         '第三者使用期限
                                .WF_Calendar.Text = WF_USERLIMIT.Text
                            Case "WF_LIMITTEXTRADIARYSTATION"         '臨時常備駅期限
                                .WF_Calendar.Text = WF_LIMITTEXTRADIARYSTATION.Text
                            Case "WF_EXTRADINARYLIMIT"         '臨時専用期限
                                .WF_Calendar.Text = WF_EXTRADINARYLIMIT.Text
                            Case "WF_ALLINSPECTIONDATE"         '取得年月日
                                .WF_Calendar.Text = WF_ALLINSPECTIONDATE.Text
                            Case "WF_TRANSFERDATE"         '車籍編入年月日
                                .WF_Calendar.Text = WF_TRANSFERDATE.Text
                            Case "WF_SPECIFIEDDATE"         '次回指定年月日
                                .WF_Calendar.Text = WF_SPECIFIEDDATE.Text
                            Case "WF_JRALLINSPECTIONDATE"         '次回全検年月日(JR)
                                .WF_Calendar.Text = WF_JRALLINSPECTIONDATE.Text
                            Case "WF_JRINSPECTIONDATE"         '次回交検年月日(JR）
                                .WF_Calendar.Text = WF_JRINSPECTIONDATE.Text
                            Case "WF_INSPECTIONDATE"         '次回交検年月日
                                .WF_Calendar.Text = WF_INSPECTIONDATE.Text
                            Case "WF_JRSPECIFIEDDATE"         '次回指定年月日(JR)
                                .WF_Calendar.Text = WF_JRSPECIFIEDDATE.Text
                        End Select
                        .ActiveCalendar()

                    Case Else
                        '以外
                        Dim prmData As New Hashtable
                        prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

                        'フィールドによってパラメータを変える
                        Select Case WF_FIELD.Value
                            Case "WF_TANKNUMBER"       'タンク車番号
                                prmData = work.CreateTankParam(work.WF_SEL_CAMPCODE.Text, "TANKNUMBER")
                            Case "WF_MODEL"       'タンク車型式
                                prmData = work.CreateTankParam(work.WF_SEL_CAMPCODE.Text, "TANKMODEL")
                        End Select

                        .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                        .ActiveListBox()
                End Select
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

                Case "WF_TANKNUMBER"               'JOT車番
                    WF_TANKNUMBER.Text = WW_SelectValue
                    WF_TANKNUMBER_TEXT.Text = WW_SelectText
                    WF_TANKNUMBER.Focus()

                Case "WF_LEASESTYMD"             'リース開始年月日
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_LEASESTYMD.Text = ""
                        Else
                            WF_LEASESTYMD.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_LEASESTYMD.Focus()

                Case "WF_LEASEENDYMD"             'リース満了年月日
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_LEASEENDYMD.Text = ""
                        Else
                            WF_LEASEENDYMD.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_LEASEENDYMD.Focus()

                Case "WF_USERLIMIT"            '第三者使用期限
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_USERLIMIT.Text = ""
                        Else
                            WF_USERLIMIT.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_USERLIMIT.Focus()

                Case "WF_LIMITTEXTRADIARYSTATION"             '臨時常備駅期限
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_LIMITTEXTRADIARYSTATION.Text = ""
                        Else
                            WF_LIMITTEXTRADIARYSTATION.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_LIMITTEXTRADIARYSTATION.Focus()

                Case "WF_EXTRADINARYLIMIT"            '臨時専用期限
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_EXTRADINARYLIMIT.Text = ""
                        Else
                            WF_EXTRADINARYLIMIT.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_EXTRADINARYLIMIT.Focus()

                Case "WF_ALLINSPECTIONDATE"             '取得年月日
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_ALLINSPECTIONDATE.Text = ""
                        Else
                            WF_ALLINSPECTIONDATE.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_ALLINSPECTIONDATE.Focus()

                Case "WF_TRANSFERDATE"            '車籍編入年月日
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_TRANSFERDATE.Text = ""
                        Else
                            WF_TRANSFERDATE.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_TRANSFERDATE.Focus()

                Case "WF_MODEL"               '型式
                    WF_MODEL.Text = WW_SelectValue
                    'WF_MODEL_TEXT.Text = WW_SelectText
                    WF_MODEL.Focus()

                Case "WF_SPECIFIEDDATE"             '次回指定年月日
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_SPECIFIEDDATE.Text = ""
                        Else
                            WF_SPECIFIEDDATE.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_SPECIFIEDDATE.Focus()

                Case "WF_JRALLINSPECTIONDATE"            '次回全検年月日(JR)
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_JRALLINSPECTIONDATE.Text = ""
                        Else
                            WF_JRALLINSPECTIONDATE.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_JRALLINSPECTIONDATE.Focus()

                Case "WF_JRINSPECTIONDATE"             '次回交検年月日(JR）
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_JRINSPECTIONDATE.Text = ""
                        Else
                            WF_JRINSPECTIONDATE.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_JRINSPECTIONDATE.Focus()

                Case "WF_INSPECTIONDATE"            '次回交検年月日
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_INSPECTIONDATE.Text = ""
                        Else
                            WF_INSPECTIONDATE.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_INSPECTIONDATE.Focus()

                Case "WF_JRSPECIFIEDDATE"            '次回指定年月日(JR)
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_JRSPECIFIEDDATE.Text = ""
                        Else
                            WF_JRSPECIFIEDDATE.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_JRSPECIFIEDDATE.Focus()
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
                Case "WF_DELFLG"                '削除フラグ
                    WF_DELFLG.Focus()

                Case "WF_TANKNUMBER"               'JOT車番
                    WF_TANKNUMBER.Focus()

                Case "WF_LEASESTYMD"             'リース開始年月日
                    WF_LEASESTYMD.Focus()

                Case "WF_LEASEENDYMD"             'リース満了年月日
                    WF_LEASEENDYMD.Focus()

                Case "WF_USERLIMIT"            '第三者使用期限
                    WF_USERLIMIT.Focus()

                Case "WF_LIMITTEXTRADIARYSTATION"             '臨時常備駅期限
                    WF_LIMITTEXTRADIARYSTATION.Focus()

                Case "WF_EXTRADINARYLIMIT"            '臨時専用期限
                    WF_EXTRADINARYLIMIT.Focus()

                Case "WF_ALLINSPECTIONDATE"             '取得年月日
                    WF_ALLINSPECTIONDATE.Focus()

                Case "WF_TRANSFERDATE"            '車籍編入年月日
                    WF_TRANSFERDATE.Focus()

                Case "WF_MODEL"               '型式
                    WF_MODEL.Focus()

                Case "WF_SPECIFIEDDATE"             '次回指定年月日
                    WF_SPECIFIEDDATE.Focus()

                Case "WF_JRALLINSPECTIONDATE"            '次回全検年月日(JR)
                    WF_JRALLINSPECTIONDATE.Focus()

                Case "WF_JRINSPECTIONDATE"             '次回交検年月日(JR）
                    WF_JRINSPECTIONDATE.Focus()

                Case "WF_INSPECTIONDATE"            '次回交検年月日
                    WF_INSPECTIONDATE.Focus()

                Case "WF_JRSPECIFIEDDATE"            '次回指定年月日(JR)
                    WF_JRSPECIFIEDDATE.Focus()
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

            '一意制約チェック
            '同一レコードの更新の場合、チェック対象外
            If OIM0005INProw("TANKNUMBER") = work.WF_SEL_TANKNUMBER2.Text Then

            Else
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    'DataBase接続
                    SQLcon.Open()

                    '一意制約チェック
                    UniqueKeyCheck(SQLcon, WW_UniqueKeyCHECK)
                End Using

                If Not isNormal(WW_UniqueKeyCHECK) Then
                    WW_CheckMES1 = "一意制約違反。"
                    WW_CheckMES2 = C_MESSAGE_NO.OVERLAP_DATA_ERROR &
                                       "([" & OIM0005INProw("TANKNUMBER") & "]" &
                                       " [" & OIM0005INProw("MODEL") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.OVERLAP_DATA_ERROR
                End If
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
                If OIM0005row("TANKNUMBER") = OIM0005INProw("TANKNUMBER") Then
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
            OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
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
                Case "ORG"             '運用部署
                    prmData = work.CreateORGParam(work.WF_SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "TANKNUMBER"        'JOT車番
                    prmData = work.CreateTankParam(work.WF_SEL_CAMPCODE.Text, I_VALUE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_TANKNUMBER, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "MODEL"        '型式
                    prmData = work.CreateTankParam(WF_MODEL.Text, I_VALUE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_TANKMODEL, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
