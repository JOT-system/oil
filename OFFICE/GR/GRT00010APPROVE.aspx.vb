Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' 残業申請承認（実行）
''' </summary>
''' <remarks></remarks>
Public Class GRT00010APPROVE
    Inherits Page

    '○ 検索結果格納Table
    Private T00010tbl As DataTable                          '一覧格納用テーブル
    Private T00010INPtbl As DataTable                       'チェック用テーブル
    Private T00010UPDtbl As DataTable                       '更新用テーブル
    Private T00010row As DataRow

    Private Const CONST_DISPROWCOUNT As Integer = 45        '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 10         'マウススクロール時稼働行数

    '○ 共通関数宣言(BASEDLL)
    Private CS0010CHARstr As New CS0010CHARget              '文字編集
    Private CS0011LOGWrite As New CS0011LOGWrite            'ログ出力
    Private CS0013ProfView As New CS0013ProfView            'Tableオブジェクト展開
    Private CS0020JOURNAL As New CS0020JOURNAL              '更新ジャーナル出力
    Private CS0023XLSUPLOAD As New CS0023XLSUPLOAD          'XLSアップロード
    Private CS0025AUTHORget As New CS0025AUTHORget          '権限チェック(マスタチェック)
    Private CS0026TBLSORT As New CS0026TBLSORT              '表示画面情報ソート
    Private CS0030REPORT As New CS0030REPORT                '帳票出力
    Private CS0048Apploval As New CS0048Apploval            '申請承認
    Private CS0050SESSION As New CS0050SESSION              'セッション情報操作処理
    Private T0007COM As New GRT0007COM                      '勤怠共通

    '○ 共通処理結果
    Private WW_ERR_SW As String = ""
    Private WW_RTN_SW As String = ""
    Private WW_DUMMY As String = ""

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

                    '○ チェックボックス保持
                    FileSaveDisplayInput()

                    '○ 画面表示データ復元
                    Master.RecoverTable(T00010tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonALLSELECT"       '全選択ボタン押下
                            WF_ButtonALLSELECT_Click()
                        Case "WF_ButtonALLCANCEL"       '全解除ボタン押下
                            WF_ButtonALLCANCEL_Click()
                        Case "WF_ButtonExtract"         '絞り込みボタン押下
                            WF_ButtonExtract_Click()
                        Case "WF_ButtonAPPROVAL"        '承認ボタン押下
                            WF_ButtonAPPROVAL_Click()
                        Case "WF_ButtonREJECT"          '否認ボタン押下
                            WF_ButtonREJECT_Click()
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

                    '承認ボタン押下、否認ボタン押下以外
                    If Not (WF_ButtonClick.Value = "WF_ButtonAPPROVAL" OrElse WF_ButtonClick.Value = "WF_ButtonREJECT") Then
                        '○ 一覧再表示処理
                        DisplayGrid()
                    End If

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
            If Not IsNothing(T00010tbl) Then
                T00010tbl.Clear()
                T00010tbl.Dispose()
                T00010tbl = Nothing
            End If

            If Not IsNothing(T00010INPtbl) Then
                T00010INPtbl.Clear()
                T00010INPtbl.Dispose()
                T00010INPtbl = Nothing
            End If

            If Not IsNothing(T00010UPDtbl) Then
                T00010UPDtbl.Clear()
                T00010UPDtbl.Dispose()
                T00010UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = GRT00010WRKINC.MAPID

        WF_APPLICANTID.Focus()
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""
        WF_RightboxOpen.Value = ""
        leftview.activeListBox()
        rightview.resetindex()

        '○ 画面の値設定
        WW_MAPValueSet()

        '○ 右ボックスへの値設定
        rightview.MAPID = Master.MAPID
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_DUMMY)

        '○ GridView初期設定
        GridViewInitialize()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        'Grid情報保存先のファイル名
        Master.createXMLSaveFile()

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.T00010S Then

        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.T00009S Then

        End If

        '○ ヘルプボタン非表示
        Master.dispHelp = False

        '○ ファイルドロップ有無
        Master.eventDrop = True

    End Sub

    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        '○ 画面表示データ取得
        Dim SQLcon = CS0050SESSION.getConnection
        SQLcon.Open()               'DataBase接続
        Try
            MAPDataGet(SQLcon)
        Finally
            SQLcon.Close()          'DataBase切断
            SQLcon.Dispose()
            SQLcon = Nothing
        End Try

        '○ 画面表示データ保存
        Master.SaveTable(T00010tbl)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(T00010tbl)

        TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Horizontal
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = False
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.CS0013ProfView()
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If

        If TBLview.ToTable IsNot Nothing AndAlso TBLview.ToTable.Rows.Count > 0 Then
            Dim displayLineCnt As List(Of Integer) = (From dr As DataRow In TBLview.ToTable
                                                      Select Convert.ToInt32(dr.Item("LINECNT"))).ToList
            ViewState("DISPLAY_LINECNT_LIST") = displayLineCnt
        Else
            ViewState("DISPLAY_LINECNT_LIST") = Nothing
        End If

        '選択設定
        Dim divDrCont As Control = pnlListArea.FindControl(pnlListArea.ID & "_DL")
        Dim tblCont As Table = DirectCast(divDrCont.Controls(0), Table)
        Dim checkedValue As Boolean
        For Each T00010Row As DataRow In TBLview.ToTable.Rows

            If Convert.ToString(T00010Row.Item("OPERATION")) = "on" Then
                checkedValue = True
            Else
                checkedValue = False
            End If

            Dim chkId As String = "chk" & pnlListArea.ID & "OPERATION" & Convert.ToString(T00010Row.Item("LINECNT"))
            Dim chk As CheckBox = DirectCast(tblCont.FindControl(chkId), CheckBox)

            If chk IsNot Nothing Then
                chk.Checked = checkedValue
                chk.Enabled = Convert.ToBoolean(T00010Row.Item("ENABLED"))
            End If

        Next

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

        If IsNothing(T00010tbl) Then
            T00010tbl = New DataTable
        End If

        If T00010tbl.Columns.Count <> 0 Then
            T00010tbl.Columns.Clear()
        End If

        T00010tbl.Clear()

        '○ 検索SQL
        Dim SQLStr As New StringBuilder
        SQLStr.AppendLine(" SELECT ")
        SQLStr.AppendLine("        0                                         as LINECNT             　  ")
        SQLStr.AppendLine("      , 0                                         as SEQNO               　  ")
        SQLStr.AppendLine("      , ''                                        as OPERATION           　  ")
        SQLStr.AppendLine("      , 'False'                                   as ENABLED             　  ")
        SQLStr.AppendLine("      , cast(isnull(A.UPDTIMSTP, 0) AS bigint)    as TIMSTP              　  ")
        SQLStr.AppendLine("      , 1                                         as 'SELECT'            　  ")
        SQLStr.AppendLine("      , 0                                         as HIDDEN              　  ")
        SQLStr.AppendLine("      , isnull(rtrim(S4.STAFFCODE),'')            as LOGONSTAFFCODE        　")
        SQLStr.AppendLine("      , isnull(rtrim(A.CAMPCODE),'')              as CAMPCODE                ")
        SQLStr.AppendLine("      , isnull(rtrim(M1.NAMES),'')                as CAMPCODENAMES           ")
        SQLStr.AppendLine("      , isnull(rtrim(A.APPLYID),'')               as APPLYID                 ")
        SQLStr.AppendLine("      , isnull(rtrim(A.STEP),'')                  as STEP                    ")
        SQLStr.AppendLine("      , isnull((SELECT isnull(rtrim(MIN(STEP)),'01')                     　  ")
        SQLStr.AppendLine("                  FROM T0009_APPROVALHIST                                　  ")
        SQLStr.AppendLine("                 WHERE CAMPCODE = @CAMPCODE                              　  ")
        SQLStr.AppendLine("                 and APPLYID  = A.APPLYID                                　  ")
        SQLStr.AppendLine("                 and STATUS   < '03'                                     　  ")
        SQLStr.AppendLine("                 and DELFLG   <> @DELFLG),'')     as CURSTEP                 ")
        SQLStr.AppendLine("      , isnull(rtrim(A.MAPID),'')                 as MAPID                   ")
        SQLStr.AppendLine("      , isnull(rtrim(A.EVENTCODE),'')             as EVENTCODE               ")
        SQLStr.AppendLine("      , isnull(rtrim(A.SUBCODE),'')               as SUBCODE                 ")
        SQLStr.AppendLine("      , isnull(rtrim(A.APPLYDATE),'')             as APPLYDATE               ")
        SQLStr.AppendLine("      , isnull(rtrim(A.APPLICANTID),'')           as APPLICANTID             ")
        SQLStr.AppendLine("      , isnull(rtrim(MB1.STAFFNAMES),'')          as APPLICANTNAMES          ")
        SQLStr.AppendLine("      , isnull(rtrim(A.APPROVEDATE),'')           as APPROVEDATE             ")
        SQLStr.AppendLine("      , isnull(rtrim(A.APPROVERID),'')            as APPROVERID              ")
        SQLStr.AppendLine("      , case when isnull(rtrim(MB2.STAFFNAMES),'') = ''                  　  ")
        SQLStr.AppendLine("             then isnull(rtrim(A.APPROVERID),'')                         　  ")
        SQLStr.AppendLine("             else isnull(rtrim(MB2.STAFFNAMES),'')                       　  ")
        SQLStr.AppendLine("             end                                  as APPROVERNAMES       　  ")
        SQLStr.AppendLine("      , isnull(rtrim(S22.APPROVALTYPE),'')        as APPROVALTYPE        　  ")
        SQLStr.AppendLine("      , isnull(rtrim(MC1_2.VALUE1),'')            as APPROVALTYPENAMES   　  ")
        SQLStr.AppendLine("      , isnull(rtrim(A.STATUS),'')                as STATUS              　  ")
        SQLStr.AppendLine("      , isnull(rtrim(MC1_1.VALUE1),'')            as STATUSNAMES         　  ")
        SQLStr.AppendLine("      , isnull(rtrim(A.APPROVEDTEXT),'')          as APPROVEDTEXT        　  ")
        SQLStr.AppendLine("      , isnull(rtrim(A.VALUE_C1),'')              as VALUE_C1            　  ")
        SQLStr.AppendLine("      , isnull(rtrim(A.VALUE_C2),'')              as VALUE_C2            　  ")
        SQLStr.AppendLine("      , isnull(rtrim(A.VALUE_C3),'')              as VALUE_C3            　  ")
        SQLStr.AppendLine("      , isnull(rtrim(A.VALUE_C4),'')              as VALUE_C4            　  ")
        SQLStr.AppendLine("      , isnull(rtrim(A.VALUE_C5),'')              as VALUE_C5            　  ")
        SQLStr.AppendLine("      , isnull(rtrim(A.REMARKS),'')               as REMARKS             　  ")
        SQLStr.AppendLine("      , isnull(rtrim(S22.STAFFCODE),'')           as MAINSTAFFCODE       　  ")
        SQLStr.AppendLine("      , isnull(rtrim(MB3.STAFFNAMES),'')          as MAINSTAFFNAMES      　  ")
        SQLStr.AppendLine("      , isnull(rtrim(A.DELFLG),'')                as DELFLG              　  ")
        SQLStr.AppendLine("      FROM  T0009_APPROVALHIST AS A                                      　  ")
        SQLStr.AppendLine("      INNER JOIN (select CODE from M0006_STRUCT ORG                 　       ")
        SQLStr.AppendLine("                   where ORG.CAMPCODE  = @CAMPCODE                       　  ")
        SQLStr.AppendLine("                     and  ORG.OBJECT   = 'ORG'                      　       ")
        SQLStr.AppendLine("                     and  ORG.STRUCT   = '勤怠管理組織'                      ")
        SQLStr.AppendLine("                     and  ORG.GRCODE01 = @P06                            　  ")
        SQLStr.AppendLine("                     and  ORG.STYMD   <= @P04                            　  ")
        SQLStr.AppendLine("                     and  ORG.ENDYMD  >= @P04                            　  ")
        SQLStr.AppendLine("                     and  ORG.DELFLG  <> @DELFLG                         　  ")
        SQLStr.AppendLine("                 ) Z3                                                    　  ")
        SQLStr.AppendLine("        ON    Z3.CODE                  = A.SUBCODE                           ")
        SQLStr.AppendLine("      INNER JOIN M0001_CAMP M1                                               ")
        SQLStr.AppendLine("        ON M1.CAMPCODE     	          = A.CAMPCODE                          ")
        SQLStr.AppendLine("       and M1.STYMD                    <= @P04       			            ")
        SQLStr.AppendLine("       and M1.ENDYMD                   >= @P04 			                    ")
        SQLStr.AppendLine("       and M1.DELFLG                   <> @DELFLG 				            ")
        SQLStr.AppendLine("      INNER JOIN S0004_USER S4      				                            ")
        SQLStr.AppendLine("        ON S4.USERID     	          = @P01       		                    ")
        SQLStr.AppendLine("       and S4.STYMD                    <= @P04       			            ")
        SQLStr.AppendLine("       and S4.ENDYMD                   >= @P04 			       	            ")
        SQLStr.AppendLine("       and S4.DELFLG                   <> @DELFLG 				            ")
        SQLStr.AppendLine("      INNER JOIN S0022_APPROVAL S22  				                        ")
        SQLStr.AppendLine("        ON S22.CAMPCODE     	          = A.CAMPCODE 			                ")
        SQLStr.AppendLine("       and S22.MAPID     	          = A.MAPID     		                ")
        SQLStr.AppendLine("       and S22.EVENTCODE     	      = A.EVENTCODE    	                    ")
        SQLStr.AppendLine("       and S22.SUBCODE     	          = A.SUBCODE     			            ")
        SQLStr.AppendLine("       and S22.STEP     	              = A.STEP     			                ")
        SQLStr.AppendLine("       and S22.STYMD                   <= @P04       			            ")
        SQLStr.AppendLine("       and S22.ENDYMD                  >= @P04 			                    ")
        SQLStr.AppendLine("       and S22.DELFLG                  <> @DELFLG 				            ")
        SQLStr.AppendLine("      LEFT JOIN MB001_STAFF MB1   				                            ")
        SQLStr.AppendLine("        ON MB1.CAMPCODE     	          = A.CAMPCODE 			                ")
        SQLStr.AppendLine("       and MB1.STAFFCODE     	      = A.APPLICANTID 		                ")
        SQLStr.AppendLine("       and MB1.STYMD                   <= @P04       			            ")
        SQLStr.AppendLine("       and MB1.ENDYMD                  >= @P04 			       	            ")
        SQLStr.AppendLine("       and MB1.DELFLG                  <> @DELFLG 				            ")
        SQLStr.AppendLine("      LEFT JOIN MB001_STAFF MB2   				                            ")
        SQLStr.AppendLine("        ON MB2.CAMPCODE     	          = A.CAMPCODE 			                ")
        SQLStr.AppendLine("       and MB2.STAFFCODE     	      = A.APPROVERID 		                ")
        SQLStr.AppendLine("       and MB2.STYMD                   <= @P04       		                ")
        SQLStr.AppendLine("       and MB2.ENDYMD                  >= @P04 			                    ")
        SQLStr.AppendLine("       and MB2.DELFLG                  <> @DELFLG 				            ")
        SQLStr.AppendLine("      LEFT JOIN MB001_STAFF MB3   				                            ")
        SQLStr.AppendLine("        ON MB3.CAMPCODE     	          = A.CAMPCODE 			                ")
        SQLStr.AppendLine("       and MB3.STAFFCODE     	      = S22.STAFFCODE 		                ")
        SQLStr.AppendLine("       and MB3.STYMD                   <= @P04       			            ")
        SQLStr.AppendLine("       and MB3.ENDYMD                  >= @P04 			       	            ")
        SQLStr.AppendLine("       and MB3.DELFLG                  <> @DELFLG 				            ")
        SQLStr.AppendLine("      LEFT JOIN MC001_FIXVALUE MC1_1				                            ")
        SQLStr.AppendLine("        ON MC1_1.CAMPCODE     	      = A.CAMPCODE 				            ")
        SQLStr.AppendLine("       and MC1_1.CLASS                 = 'APPROVAL'   			            ")
        SQLStr.AppendLine("       and MC1_1.KEYCODE               = A.STATUS  			                ")
        SQLStr.AppendLine("       and MC1_1.STYMD                 <= @P04   				            ")
        SQLStr.AppendLine("       and MC1_1.ENDYMD                >= @P04 	        	                ")
        SQLStr.AppendLine("       and MC1_1.DELFLG                <> @DELFLG 				            ")
        SQLStr.AppendLine("      LEFT JOIN MC001_FIXVALUE MC1_2				                            ")
        SQLStr.AppendLine("        ON MC1_2.CAMPCODE     	      = A.CAMPCODE 				            ")
        SQLStr.AppendLine("       and MC1_2.CLASS                 = 'APPROVALTYPE'   				    ")
        SQLStr.AppendLine("       and MC1_2.KEYCODE               = S22.APPROVALTYPE  				    ")
        SQLStr.AppendLine("       and MC1_2.STYMD                 <= @P04   				            ")
        SQLStr.AppendLine("       and MC1_2.ENDYMD                >= @P04 	        	                ")
        SQLStr.AppendLine("       and MC1_2.DELFLG                <> @DELFLG 						    ")
        SQLStr.AppendLine("     WHERE A.CAMPCODE     	          = @CAMPCODE                           ")
        SQLStr.AppendLine("       and A.APPLYDATE                 >= @P05		                        ")
        SQLStr.AppendLine("       and A.STATUS                    <> '03'		                        ")
        SQLStr.AppendLine("       and A.DELFLG                    <> @DELFLG		                    ")
        SQLStr.AppendLine("     ORDER BY A.APPLYID ,A.VALUE_C1 ,A.APPLYDATE, S22.STEP, S22.APPROVALTYPE ")

        Dim SQLcmd As New SqlCommand()
        Dim SQLdr As SqlDataReader = Nothing

        Try
            SQLcmd = New SqlCommand(SQLStr.ToString, SQLcon)

            Dim ParmUserId As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
            Dim ParmCampCode As SqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar)
            Dim ParmDate As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Date)
            Dim ParmApplyDate As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.Date)
            Dim ParmHorg As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar)
            Dim ParmDelflg As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar)           '削除フラグ

            ParmUserId.Value = CS0050SESSION.USERID
            ParmCampCode.Value = work.WF_SEL_CAMPCODE.Text
            ParmDate.Value = Date.Now
            ParmApplyDate.Value = work.WF_SEL_TAISHOYM.Text & "/01"
            Dim orgCode As String = ""
            Dim retCode As String = ""
            T0007COM.ConvORGCODE(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_HORG.Text, orgCode, retCode)
            If retCode = C_MESSAGE_NO.NORMAL Then
                ParmHorg.Value = orgCode
            Else
                ParmHorg.Value = work.WF_SEL_HORG.Text
            End If
            ParmDelflg.Value = C_DELETE_FLG.DELETE

            SQLdr = SQLcmd.ExecuteReader()

            '○ フィールド名とフィールドの型を取得
            For index As Integer = 0 To SQLdr.FieldCount - 1
                T00010tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
            Next

            '○ テーブル検索結果をテーブル格納
            T00010tbl.Load(SQLdr)

            Dim WW_CNT As Integer = 0
            For Each T00010row As DataRow In T00010tbl.Rows
                '画面固有項目
                Dim WW_DATE As Date
                Try
                    If T00010row("APPLYDATE") <> "" Then
                        Date.TryParse(T00010row("APPLYDATE"), WW_DATE)
                        T00010row("APPLYDATE") = WW_DATE.ToString("yyyy/MM/dd HH:mm:dd")
                    End If
                Catch ex As Exception
                    T00010row("APPLYDATE") = ""
                End Try

                Try
                    If T00010row("APPROVEDATE") <> "" Then
                        Date.TryParse(T00010row("APPROVEDATE"), WW_DATE)
                        T00010row("APPROVEDATE") = WW_DATE.ToString("yyyy/MM/dd HH:mm:dd")
                    End If
                Catch ex As Exception
                    T00010row("APPROVEDATE") = ""
                End Try

                '○データ設定
                WW_CNT = WW_CNT + 1
                '固定項目
                T00010row("SEQNO") = WW_CNT - 1

            Next
        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0009_APPROVALHIST SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:T0009_APPROVALHIST Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        Finally
            If Not IsNothing(SQLdr) Then
                SQLdr.Close()
                SQLdr = Nothing
            End If

            SQLcmd.Dispose()
            SQLcmd = Nothing
        End Try

        '○ 画面表示データソート
        CS0026TBLSORT.COMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0026TBLSORT.PROFID = Master.PROF_VIEW
        CS0026TBLSORT.MAPID = Master.MAPID
        CS0026TBLSORT.VARI = Master.VIEWID
        CS0026TBLSORT.TABLE = T00010tbl
        CS0026TBLSORT.TAB = ""
        CS0026TBLSORT.FILTER = ""
        CS0026TBLSORT.SortandNumbring()
        If isNormal(CS0026TBLSORT.ERR) Then
            T00010tbl = CS0026TBLSORT.TABLE
        End If

        '項番（LineCNT）をナンバーリング
        SetLineCNT(T00010tbl)

    End Sub

    ''' <summary>
    ''' 項番ナンバーリング処理
    ''' </summary>
    ''' <param name="iTBL"></param>
    Protected Sub SetLineCNT(ByRef iTBL As DataTable)
        '項番をナンバーリング
        Dim WW_LineCNT As Long = 0
        For i As Integer = 0 To iTBL.Rows.Count - 1

            T00010row = iTBL.Rows(i)
            T00010row("HIDDEN") = "1"
            T00010row("LINECNT") = 0
            T00010row("ENABLED") = False

            '全て
            If work.WF_SEL_APPROVALDISPTYPE.Text = "3" Then
                If WF_APPLICANTID.Text = "" Then
                    If T00010row("LOGONSTAFFCODE") = T00010row("MAINSTAFFCODE") Then
                        T00010row("HIDDEN") = "0"
                        WW_LineCNT = WW_LineCNT + 1
                        T00010row("LINECNT") = WW_LineCNT
                    End If
                    '申請中のみ表示（但し、自分の担当分のみ活性化）
                    If T00010row("STATUS") = "02" AndAlso
                       T00010row("LOGONSTAFFCODE") = T00010row("MAINSTAFFCODE") AndAlso
                       T00010row("APPLICANTID") <> T00010row("MAINSTAFFCODE") Then
                        T00010row("ENABLED") = True                 'チェックボックス活性化
                    End If
                Else
                    If WF_APPLICANTID.Text = T00010row("APPLICANTID") Then
                        If T00010row("LOGONSTAFFCODE") = T00010row("MAINSTAFFCODE") Then
                            T00010row("HIDDEN") = "0"
                            WW_LineCNT = WW_LineCNT + 1
                            T00010row("LINECNT") = WW_LineCNT
                        End If
                        '申請中のみ表示（但し、自分の担当分のみ活性化）
                        If T00010row("STATUS") = "02" AndAlso
                           T00010row("LOGONSTAFFCODE") = T00010row("MAINSTAFFCODE") AndAlso
                           T00010row("APPLICANTID") <> T00010row("MAINSTAFFCODE") Then
                            T00010row("ENABLED") = True              'チェックボックス活性化
                        End If
                    End If
                End If
            ElseIf work.WF_SEL_APPROVALDISPTYPE.Text = "2" Then
                If WF_APPLICANTID.Text = "" Then
                    '承認済のみ表示
                    If T00010row("STATUS") = "10" AndAlso
                       T00010row("LOGONSTAFFCODE") = T00010row("MAINSTAFFCODE") Then
                        T00010row("HIDDEN") = "0"
                        WW_LineCNT = WW_LineCNT + 1
                        T00010row("LINECNT") = WW_LineCNT
                        T00010row("ENABLED") = False                 'チェックボックス非活性
                    End If
                Else
                    If WF_APPLICANTID.Text = T00010row("APPLICANTID") Then
                        '承認済のみ表示
                        If T00010row("STATUS") = "10" AndAlso
                           T00010row("LOGONSTAFFCODE") = T00010row("MAINSTAFFCODE") Then
                            T00010row("HIDDEN") = "0"
                            WW_LineCNT = WW_LineCNT + 1
                            T00010row("LINECNT") = WW_LineCNT
                            T00010row("ENABLED") = False             'チェックボックス非活性
                        End If
                    End If
                End If
            Else
                '申請中のみ活性
                If T00010row("STEP") = T00010row("CURSTEP") Then
                    If WF_APPLICANTID.Text = "" Then
                        If T00010row("STATUS") = "02" AndAlso
                           T00010row("LOGONSTAFFCODE") = T00010row("MAINSTAFFCODE") AndAlso
                           T00010row("APPLICANTID") <> T00010row("MAINSTAFFCODE") Then
                            T00010row("HIDDEN") = "0"
                            WW_LineCNT = WW_LineCNT + 1
                            T00010row("LINECNT") = WW_LineCNT
                            T00010row("ENABLED") = True                  'チェックボックス活性化
                        End If
                    Else
                        If WF_APPLICANTID.Text = T00010row("APPLICANTID") Then
                            If T00010row("STATUS") = "02" AndAlso
                               T00010row("LOGONSTAFFCODE") = T00010row("MAINSTAFFCODE") AndAlso
                               T00010row("APPLICANTID") <> T00010row("MAINSTAFFCODE") Then
                                T00010row("HIDDEN") = "0"
                                WW_LineCNT = WW_LineCNT + 1
                                T00010row("LINECNT") = WW_LineCNT
                                T00010row("ENABLED") = True              'チェックボックス活性化
                            End If
                        End If
                    End If
                End If
            End If
        Next

        '表示対象
        If (From iDr In iTBL Where iDr.Item("HIDDEN") = "0").Count > 0 Then
            iTBL = (From iDr In iTBL Where iDr.Item("HIDDEN") = "0").CopyToDataTable
        End If

    End Sub

    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each T00010row As DataRow In T00010tbl.Rows
            If T00010row("HIDDEN") = "0" Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                T00010row("SELECT") = WW_DataCNT
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
            If (WW_GridPosition + CONST_SCROLLCOUNT) > 0 Then
                WW_GridPosition -= CONST_SCROLLCOUNT
            Else
                WW_GridPosition = 1
            End If
        End If

        '○ 画面(GridView)表示
        Dim TBLview As DataView = New DataView(T00010tbl)

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
        CS0013ProfView.TITLEOPT = False
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.CS0013ProfView()

        '選択設定
        Dim divDrCont As Control = pnlListArea.FindControl(pnlListArea.ID & "_DL")
        Dim tblCont As Table = DirectCast(divDrCont.Controls(0), Table)

        For Each T00010Row As DataRow In TBLview.ToTable.Rows

            Dim chkId As String = "chk" & pnlListArea.ID & "OPERATION" & Convert.ToString(T00010Row.Item("LINECNT"))
            Dim chk As CheckBox = DirectCast(tblCont.FindControl(chkId), CheckBox)
            If chk IsNot Nothing Then
                chk.Enabled = Convert.ToBoolean(T00010Row.Item("ENABLED"))
            End If
        Next

        '1.現在表示しているLINECNTのリストをビューステートに保持
        '2.チェックがついているチェックボックスオブジェクトをチェック状態にする
        If TBLview.ToTable IsNot Nothing AndAlso TBLview.ToTable.Rows.Count > 0 Then
            Dim displayLineCnt As List(Of Integer) = (From dr As DataRow In TBLview.ToTable
                                                      Select Convert.ToInt32(dr.Item("LINECNT"))).ToList
            ViewState("DISPLAY_LINECNT_LIST") = displayLineCnt
            Dim targetCheckBoxLineCnt = (From dr As DataRow In TBLview.ToTable
                                         Where Convert.ToString(dr.Item("OPERATION")) <> ""
                                         Select Convert.ToInt32(dr.Item("LINECNT")))
            For Each lineCnt As Integer In targetCheckBoxLineCnt
                Dim chkObjId As String = "chk" & Me.pnlListArea.ID & "OPERATION" & lineCnt.ToString
                Dim tmpObj As Control = Me.pnlListArea.FindControl(chkObjId)
                If tmpObj IsNot Nothing Then
                    Dim chkObj As CheckBox = DirectCast(tmpObj, CheckBox)
                    chkObj.Checked = True
                End If
            Next
        Else
            ViewState("DISPLAY_LINECNT_LIST") = Nothing
        End If

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
    ''' 全選択ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonALLSELECT_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(T00010tbl)

        '全チェックボックスON
        For i As Integer = 0 To T00010tbl.Rows.Count - 1
            If T00010tbl.Rows(i)("HIDDEN") = "0" And T00010tbl.Rows(i)("ENABLED") = True Then
                T00010tbl.Rows(i)("OPERATION") = "on"
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(T00010tbl)

    End Sub

    ''' <summary>
    ''' 全解除ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonALLCANCEL_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(T00010tbl)

        '全チェックボックスOFF
        For i As Integer = 0 To T00010tbl.Rows.Count - 1
            If T00010tbl.Rows(i)("HIDDEN") = "0" And T00010tbl.Rows(i)("ENABLED") = True Then
                T00010tbl.Rows(i)("OPERATION") = ""
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(T00010tbl)

    End Sub

    ''' <summary>
    ''' 絞り込みボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonExtract_Click()

        '○ 使用禁止文字排除
        Master.eraseCharToIgnore(WF_APPLICANTID.Text)

        '○ 名称取得
        CODENAME_get("APPLICANTID", WF_APPLICANTID.Text, WF_APPLICANTID_TEXT.Text, WW_DUMMY)

        '○ 絞り込み操作(GridView明細Hidden設定)
        For Each T00010row As DataRow In T00010tbl.Rows

            '一度非表示にする
            T00010row("HIDDEN") = 1

            Dim WW_HANTEI As Boolean = True

            '管理部署による絞込判定
            If WF_APPLICANTID.Text <> "" And
                WF_APPLICANTID.Text <> T00010row("APPLICANTID") Then
                WW_HANTEI = False
            End If

            '画面(GridView)のHIDDENに結果格納
            If WW_HANTEI Then
                T00010row("HIDDEN") = 0
            End If
        Next

        '○ 画面先頭を表示
        WF_GridPosition.Text = "1"

        '○ 画面表示データ保存
        Master.SaveTable(T00010tbl)

        '○ メッセージ表示
        Master.output(C_MESSAGE_NO.DATA_FILTER_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        WF_APPLICANTID.Focus()

    End Sub

    ''' <summary>
    ''' 承認ボタン処理
    ''' </summary>
    Protected Sub WF_ButtonAPPROVAL_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(T00010tbl)

        ''○ 画面表示データソート
        'CS0026TBLSORT.COMPCODE = work.WF_SEL_CAMPCODE.Text
        'CS0026TBLSORT.PROFID = Master.PROF_VIEW
        'CS0026TBLSORT.MAPID = Master.MAPID
        'CS0026TBLSORT.VARI = Master.VIEWID
        'CS0026TBLSORT.TABLE = T00010tbl
        'CS0026TBLSORT.TAB = ""
        'CS0026TBLSORT.FILTER = ""
        'CS0026TBLSORT.SortandNumbring()
        'If isNormal(CS0026TBLSORT.ERR) Then
        '    T00010tbl = CS0026TBLSORT.TABLE
        'End If

        For i = 0 To T00010tbl.Rows.Count - 1
            Dim T00010row As DataRow = T00010tbl.Rows(i)
            If T00010row("OPERATION") = "on" Then
                CS0048Apploval.I_CAMPCODE = T00010row("CAMPCODE")
                CS0048Apploval.I_APPLYID = T00010row("APPLYID")
                CS0048Apploval.I_STEP = T00010row("STEP")
                CS0048Apploval.I_STAFFCODE = T00010row("LOGONSTAFFCODE")
                CS0048Apploval.I_UPDUSER = Master.USERID
                CS0048Apploval.I_UPDTERMID = Master.USERTERMID
                CS0048Apploval.CS0048setApproval()
                If CS0048Apploval.O_ERR <> C_MESSAGE_NO.NORMAL And CS0048Apploval.O_ERR <> "99999" Then
                    Master.output(CS0048Apploval.O_ERR, C_MESSAGE_TYPE.ABORT, "承認登録エラー")
                    Exit Sub
                End If
            End If
        Next

        '○ GridView初期設定
        GridViewInitialize()

        ''○ 画面表示データ保存
        'Master.SaveTable(T00010tbl)

    End Sub

    ''' <summary>
    ''' 否認ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonREJECT_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(T00010tbl)

        ''○ 画面表示データソート
        'CS0026TBLSORT.COMPCODE = work.WF_SEL_CAMPCODE.Text
        'CS0026TBLSORT.PROFID = Master.PROF_VIEW
        'CS0026TBLSORT.MAPID = Master.MAPID
        'CS0026TBLSORT.VARI = Master.VIEWID
        'CS0026TBLSORT.TABLE = T00010tbl
        'CS0026TBLSORT.TAB = ""
        'CS0026TBLSORT.FILTER = ""
        'CS0026TBLSORT.SortandNumbring()
        'If isNormal(CS0026TBLSORT.ERR) Then
        '    T00010tbl = CS0026TBLSORT.TABLE
        'End If

        For i = 0 To T00010tbl.Rows.Count - 1
            Dim T00010row As DataRow = T00010tbl.Rows(i)
            If T00010row("OPERATION") = "on" Then
                CS0048Apploval.I_CAMPCODE = T00010row("CAMPCODE")
                CS0048Apploval.I_APPLYID = T00010row("APPLYID")
                CS0048Apploval.I_STEP = T00010row("STEP")
                CS0048Apploval.I_STAFFCODE = T00010row("LOGONSTAFFCODE")
                CS0048Apploval.I_UPDUSER = Master.USERID
                CS0048Apploval.I_UPDTERMID = Master.USERTERMID
                CS0048Apploval.CS0048setDenial()
                If CS0048Apploval.O_ERR <> C_MESSAGE_NO.NORMAL And CS0048Apploval.O_ERR <> "99999" Then
                    Master.output(CS0048Apploval.O_ERR, C_MESSAGE_TYPE.ABORT, "否認登録エラー")
                    Exit Sub
                End If
            End If
        Next

        '○ GridView初期設定
        GridViewInitialize()

        ''○ 画面表示データ保存
        'Master.SaveTable(T00010tbl)

    End Sub

    ''' <summary>
    ''' 終了ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        Master.transitionPrevPage()

    End Sub

    ''' <summary>
    ''' 先頭頁ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonFIRST_Click()

        '○ 先頭頁に移動
        WF_GridPosition.Text = "1"
        WF_APPLICANTID.Focus()

    End Sub

    ''' <summary>
    ''' 最終頁ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonLAST_Click()

        '○ ソート
        Dim TBLview As New DataView(T00010tbl)
        TBLview.RowFilter = "HIDDEN = 0"

        '○ 最終頁に移動
        If TBLview.Count Mod 10 = 0 Then
            WF_GridPosition.Text = TBLview.Count - (TBLview.Count Mod 10)
        Else
            WF_GridPosition.Text = TBLview.Count - (TBLview.Count Mod 10) + 1
        End If

        WF_APPLICANTID.Focus()

        TBLview.Dispose()
        TBLview = Nothing

    End Sub

    ' ******************************************************************************
    ' ***  一覧表示(GridView)関連操作                                            ***
    ' ******************************************************************************

    ''' <summary>
    ''' 一覧画面-明細行ダブルクリック時処理
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
            'WW_LINECNT -= 1
        Catch ex As Exception
            Exit Sub
        End Try

        '■■■ Grid内容(T00010tbl)よりセッション変数編集 ■■■
        For i As Integer = 0 To T00010tbl.Rows.Count - 1
            Dim WW_T0010row As DataRow = T00010tbl.Rows(i)
            If WW_T0010row("SELECT") = "1" And WW_T0010row("LINECNT") = WW_LINECNT Then
                work.WF_T09_CAMPCODE.Text = WW_T0010row("CAMPCODE")
                Dim orgCode As String = ""
                Dim retCode As String = ""
                T0007COM.ConvORGCODE(WW_T0010row("CAMPCODE"), WW_T0010row("SUBCODE"), orgCode, retCode)
                If retCode = C_MESSAGE_NO.NORMAL Then
                    work.WF_T09_HORG.Text = orgCode
                Else
                    work.WF_T09_HORG.Text = WW_T0010row("SUBCODE")
                End If
                work.WF_T09_STAFFCODE.Text = WW_T0010row("APPLICANTID")
                work.WF_T09_TAISHOYM.Text = Mid(WW_T0010row("VALUE_C1"), 1, 7)
                Exit For
            End If
        Next
        work.WF_T09_STAFFKBN.Text = ""
        work.WF_T09_STAFFNAME.Text = ""
        work.WF_T09_MAPID.Text = Master.MAPID
        work.WF_T09_MAPVARIANT.Text = Master.MAPvariant

        work.WF_SEL_GridPosition.Text = WF_GridPosition.Text

        ''★★★ 画面遷移先URL取得 ★★★
        'CS0018DOURLget.MAPIDP = "T00009S"
        'CS0018DOURLget.VARIP = Master.MAPvariant
        'CS0018DOURLget.CS0018DOURLget()
        'If Not isNormal(CS0018DOURLget.ERR) Then
        '    Master.output(CS0018DOURLget.ERR, C_MESSAGE_TYPE.ABORT, "画面実行URL取得エラー")
        '    Exit Sub
        'End If

        ''画面遷移実行
        'Server.Transfer(CS0018DOURLget.URL)

        Master.transitionPage(work.WF_SEL_CAMPCODE.Text)

    End Sub

    ''' <summary>
    ''' GridView値設定
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Function REP_ITEM_FORMAT(ByVal I_FIELD As String, ByRef I_VALUE As String) As String

        REP_ITEM_FORMAT = I_VALUE
        Select Case I_FIELD
            Case "SEQ"
                Try
                    REP_ITEM_FORMAT = Format(CInt(I_VALUE), "0")
                Catch ex As Exception
                End Try
        End Select

    End Function

    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_Scroll()

        ''○ 画面表示データ復元
        'Master.RecoverTable(T00010tbl)

        'For Each checkDr As DataRow In T00010tbl.Rows

        '    Dim chkObj As String = "ctl00$contents1$chk" & pnlListArea.ID & "OPERATION" & Convert.ToString(checkDr.Item("LINECNT"))

        '    If checkDr.Item("HIDDEN") = "0" And checkDr.Item("ENABLED") = True Then
        '        If Not IsNothing(Request.Form(chkObj)) Then
        '            If Request.Form(chkObj) = "on" Then
        '                checkDr.Item("OPERATION") = True
        '            Else
        '                checkDr.Item("OPERATION") = False
        '            End If
        '        End If
        '    End If
        'Next

        ''○ 画面表示データ保存
        'Master.SaveTable(T00010tbl)

        WF_APPLICANTID.Focus()

    End Sub

    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_DBClick()

        '〇フィールドダブルクリック処理
        If String.IsNullOrEmpty(WF_LeftMViewChange.Value) OrElse WF_LeftMViewChange.Value = "" Then
        Else
            Try
                Integer.TryParse(WF_LeftMViewChange.Value, WF_LeftMViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try

            With leftview
                Select Case WF_LeftMViewChange.Value
                    Case LIST_BOX_CLASSIFICATION.LC_CALENDAR

                    Case Else
                        '上記以外

                        Dim prmData As New Hashtable
                        prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

                        'フィールドによってパラメータを変える
                        Select Case WF_FIELD.Value
                            Case "WF_APPLICANTID"              '申請者
                                prmData = work.CreateStaffCodeParam()
                        End Select

                        .setListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                        .activeListBox()
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
            Case "WF_APPLICANTID"          '申請者
                CODENAME_get("APPLICANTID", WF_APPLICANTID.Text, WF_APPLICANTID_TEXT.Text, WW_RTN_SW)
        End Select

        '○ メッセージ表示
        If isNormal(WW_RTN_SW) Then
            Master.output(WW_RTN_SW, C_MESSAGE_TYPE.NOR)
        Else
            Master.output(WW_RTN_SW, C_MESSAGE_TYPE.ERR)
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

        Dim WW_SelectValues As String() = Nothing

        '○ 選択内容を取得
        If Not IsNothing(leftview.getActiveValue) Then
            WW_SelectValues = leftview.getActiveValue
        End If

        '○ 選択内容を画面項目へセット
        Select Case WF_FIELD.Value
            Case "WF_APPLICANTID"       '申請者
                WF_APPLICANTID.Text = WW_SelectValues(0)
                WF_APPLICANTID_TEXT.Text = WW_SelectValues(1)
                WF_APPLICANTID.Focus()

        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub

    ''' <summary>
    ''' LeftBoxキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○ フォーカスセット
        Select Case WF_FIELD.Value
            Case "WF_APPLICANTID"       '申請者
                WF_APPLICANTID.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub

    ''' <summary>
    ''' RightBoxラジオボタン選択処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RadioButton_Click()

        If WF_RightViewChange.Value = Nothing Or WF_RightViewChange.Value = "" Then
        Else
            Try
                Integer.TryParse(WF_RightViewChange.Value, WF_RightViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try

            rightview.selectIndex(WF_RightViewChange.Value)
            WF_RightViewChange.Value = ""
        End If

    End Sub

    ''' <summary>
    ''' RightBoxメモ欄更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_Change()

        rightview.save(Master.USERID, Master.USERTERMID, WW_DUMMY)

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="T00010row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal T00010row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(T00010row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 会社         =" & T00010row("CAMPCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 申請分類     =" & T00010row("EVENTCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 申請ID       =" & T00010row("APPLYID") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 承認ステップ =" & T00010row("STEP") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 申請者       =" & T00010row("APPLICANTNAMES") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 申請日       =" & T00010row("APPLYDATE")
        End If

        rightview.addErrorReport(WW_ERR_MES)

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
        prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

        Try
            Select Case I_FIELD
                Case "APPLICANTID"        '申請者
                    prmData = work.CreateStaffCodeParam()
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 画面グリッドのデータを取得しファイルに保存する。
    ''' </summary>
    Private Sub FileSaveDisplayInput()
        'そもそも画面表示データがない状態の場合はそのまま終了
        If ViewState("DISPLAY_LINECNT_LIST") Is Nothing Then
            Return
        End If
        Dim displayLineCnt = DirectCast(ViewState("DISPLAY_LINECNT_LIST"), List(Of Integer))

        '○ 画面表示データ復元
        Master.RecoverTable(T00010tbl)

        'この段階でありえないがデータテーブルがない場合は終了
        If T00010tbl Is Nothing OrElse T00010tbl.Rows.Count = 0 Then
            Return
        End If

        'サフィックス抜き（LISTID)抜きのオブジェクト名リスト
        Dim objChkPrifix As String = "ctl00$contents1$chk" & Me.pnlListArea.ID
        Dim fieldIdList As New Dictionary(Of String, String) From {{"OPERATION", objChkPrifix}}

        Dim formToPost = New NameValueCollection(Request.Form)
        For Each i In displayLineCnt
            For Each fieldId As KeyValuePair(Of String, String) In fieldIdList
                Dim dispObjId As String = fieldId.Value & fieldId.Key & i
                Dim displayValue As String = ""
                If Request.Form.AllKeys.Contains(dispObjId) Then
                    displayValue = Request.Form(dispObjId)
                    formToPost.Remove(dispObjId)
                End If
                Dim T00010Dr As DataRow = T00010tbl.Rows(i - 1)
                T00010Dr.Item(fieldId.Key) = displayValue
            Next
        Next

        '○ 画面表示データ保存
        Master.SaveTable(T00010tbl)

        Return
    End Sub

End Class
