Imports System.Data.SqlClient
Imports System.IO
Imports System.IO.Compression
Imports System.Net
Imports OFFICE.GRIS0005LeftBox
Imports OFFICE.GRT00004COM


''' <summary>
''' 車端ファイル作成（実行）
''' </summary>
''' <remarks></remarks>
Public Class GRT00006EXPORT
    Inherits System.Web.UI.Page

    '検索結果格納ds
    Private T00006ds As DataSet                                     '格納ＤＳ
    Private T00006tbl As DataTable                                  'Grid格納用テーブル
    Private T00006EXPtbl As DataTable                               'Grid格納用テーブル

    '*共通関数宣言(BASEDLL)
    Private CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
    Private CS0013ProfView As New CS0013ProfView                    'Tableオブジェクト展開
    Private CS0026TBLSORTget As New CS0026TBLSORT                   'GridView用テーブルソート文字列取得
    Private CS0030REPORT As New CS0030REPORT                        '帳票出力
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作

    '共通処理結果
    Private WW_ERRCODE As String = String.Empty                     'リターンコード
    Private WW_RTN_SW As String                                     '
    Private WW_DUMMY As String                                      '

    Private Const CONST_DSPROWCOUNT As Integer = 40                 '１画面表示対象
    Private Const CONST_SCROLLROWCOUNT As Integer = 20              'マウススクロール時の増分

    Private Const CONST_KOUEI As String = "JOTtoKouei"
    Private Const C_FILE_KOUEI As String = "kouei.zip"
    Private Const C_FILE_YAZAKI As String = "yazaki.zip"

    ''' <summary>
    ''' ファイル構造（矢崎）
    ''' </summary>
    Structure YAZAKI
        <VBFixedString(8)> Public GSHABAN As String
        <VBFixedString(1)> Public RYOME As String
        <VBFixedString(2)> Public TRIPNO As String
        <VBFixedString(2)> Public DROPNO As String
        <VBFixedString(8)> Public SHUKADATE As String
        <VBFixedString(8)> Public TODOKEDATE As String
        <VBFixedString(3)> Public TORICODE As String
        <VBFixedString(10)> Public TORICODENAME As String
        <VBFixedString(4)> Public SHUKABASHO As String
        <VBFixedString(16)> Public SHUKABASHONAME As String
        <VBFixedString(10)> Public TODOKECODE As String
        <VBFixedString(20)> Public TODOKECODENAME As String
        <VBFixedString(5)> Public PRODUCT As String
        <VBFixedString(20)> Public PRODUCTNAME As String
        <VBFixedString(11)> Public SURYO As String
        <VBFixedString(8)> Public STAFFCODE As String
        <VBFixedString(4)> Public STTIME As String
        <VBFixedString(8)> Public SUBSTAFFCODE As String
        <VBFixedString(1)> Public MARK As String
        <VBFixedArray(2)> Public crlf As String
    End Structure
    ''' <summary>
    ''' FTPターゲット(S0028_FTPFILES設定値)
    ''' </summary>
    Private Class FTP_TERGET
        Public Const JX As String = "配乗結果データ送信JX"
        Public Const JXTG As String = "配乗結果データ送信TG"
        Public Const COSMO As String = "配乗結果データ送信COSMO"
        Public Const JOT As String = "配乗結果データ送信JOT"
    End Class
    ''' <summary>
    ''' サーバ処理の遷移先
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Try

            '■■■ 作業用データベース設定 ■■■
            T00006ds = New DataSet()                                      '初期化
            T00006tbl = T00006ds.Tables.Add("T00006TBL")
            T00006EXPtbl = T00006ds.Tables.Add("T00006EXPTBL")

            If IsPostBack Then
                '○ チェックボックス保持
                FileSaveDisplayInput()

                '■■■ 各ボタン押下処理 ■■■
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    Select Case WF_ButtonClick.Value
                        '********* ヘッダ部 *********
                        Case "WF_ButtonALLSELECT"               '全選択
                            WF_ButtonALLSELECT_Click()
                        Case "WF_ButtonALLCANCEL"               '全解除
                            WF_ButtonALLCANCEL_Click()
                        Case "WF_ButtonPut"                     '光英送信
                            WF_ButtonPut_Click()
                        Case "WF_ButtonCSV"                     '光英CSV
                            WF_ButtonCSV_Click()
                        Case "WF_ButtonLOCAL"                   '矢崎Zip
                            WF_ButtonLOCAL_Click()
                        Case "WF_ButtonFIRST"                   '先頭頁[image]
                            WF_ButtonFIRST_Click()
                        Case "WF_ButtonLAST"                    '最終頁[image]
                            WF_ButtonLAST_Click()
                        Case "WF_ButtonEND"                     '終了
                            WF_ButtonEND_Click()

                            '********* 一覧 *********
                        Case "WF_MouseWheelDown"                'MouseDown
                            WF_GRID_Scrole()
                        Case "WF_MouseWheelUp"                  'MouseUp
                            WF_GRID_Scrole()

                            '********* 右BOX *********
                        Case "WF_RadioButonClick"               '選択時
                            WF_RadioButon_Click()
                        Case "WF_MEMOChange"                    'メモ欄変更時
                            WF_MEMO_Change()

                            '********* その他はMasterPageで処理 *********
                        Case Else
                    End Select
                    '○一覧再表示処理
                    DisplayGrid()

                End If
            Else
                '〇初期化処理
                Initialize()
            End If

        Catch ex As Exception
            Master.output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT)
        Finally
            '○Close
            If Not IsNothing(T00006EXPtbl) Then
                T00006EXPtbl.Dispose()
                T00006EXPtbl = Nothing
            End If
            If Not IsNothing(T00006tbl) Then
                T00006tbl.Dispose()
                T00006tbl = Nothing
            End If
            If Not IsNothing(T00006ds) Then
                T00006ds.Dispose()
                T00006ds = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()
        '○初期値設定
        Master.MAPID = GRT00006WRKINC.MAPID
        '〇ヘルプ無
        Master.dispHelp = False
        '〇ドラックアンドドロップOFF
        Master.eventDrop = False

        '○Grid情報保存先のファイル名
        Master.createXMLSaveFile()

        '左Boxへの値設定

        '右Boxへの値設定
        rightview.resetindex()
        rightview.MAPID = Master.MAPID
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_DUMMY)
        rightview.selectIndex(GRIS0004RightBox.RIGHT_TAB_INDEX.LS_ERROR_LIST)

        '〇画面モード（更新・参照）設定 
        If Master.MAPpermitcode = C_PERMISSION.UPDATE Then
            WF_MAPpermitcode.Value = "TRUE"
        Else
            WF_MAPpermitcode.Value = "FALSE"
        End If

        '光英送信ボタン非表示設定
        Dim T5Com = New GRT0005COM
        If Not T5Com.IsKoueiAvailableOrg(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_SHIPORG.Text, GRT00006WRKINC.C_KOUEI_CLASS_CODE, WW_ERRCODE) Then
            WF_IsHideKoueiButton.Value = "1"
            WF_ButtonPut.Visible = False
        End If
        T5Com = Nothing

        '○画面表示データ取得
        GRID_INITset()

        '○画面表示データ保存
        Master.SaveTable(T00006tbl)

        '○一覧表示処理
        DisplayGrid()

    End Sub


    ''' <summary>
    ''' 一覧表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer                 '表示位置（開始）
        Dim WW_DataCNT As Integer = 0                  '(絞り込み後)有効Data数

        '表示対象行カウント(絞り込み対象)
        If T00006tbl.Columns.Count = 0 Then
            '○画面表示データ復元
            If Master.RecoverTable(T00006tbl) <> True Then Exit Sub
        End If
        '　※　絞込（Cells("Hidden")： 0=表示対象 , 1=非表示対象)
        For Each T00006row In T00006tbl.Rows
            If T00006row("HIDDEN") = "0" Then
                WW_DataCNT = WW_DataCNT + 1
            End If
        Next

        '○表示Linecnt取得
        If WF_GridPosition.Text = "" Then
            WW_GridPosition = 1
        Else
            If Not Integer.TryParse(WF_GridPosition.Text, WW_GridPosition) Then
                WW_GridPosition = 1
            End If
        End If

        '○表示格納位置決定

        '表示開始_格納位置決定(次頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelUp" Then
            If (WW_GridPosition + CONST_SCROLLROWCOUNT) <= WW_DataCNT Then
                WW_GridPosition = WW_GridPosition + CONST_SCROLLROWCOUNT
            End If
        End If

        '表示開始_位置決定(前頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelDown" Then
            If (WW_GridPosition - CONST_SCROLLROWCOUNT) > 0 Then
                WW_GridPosition -= CONST_SCROLLROWCOUNT
            Else
                WW_GridPosition = 1
            End If
        End If

        '○画面（GridView）表示
        Dim WW_TBLview As DataView = New DataView(T00006tbl)

        'ソート
        WW_TBLview.Sort = "LINECNT"
        WW_TBLview.RowFilter = "HIDDEN = 0 and LINECNT >= " & WW_GridPosition.ToString & " and LINECNT < " & (WW_GridPosition + CONST_DSPROWCOUNT).ToString

        '一覧作成
        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = WW_TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.None
        CS0013ProfView.LEVENT = ""
        CS0013ProfView.LFUNC = ""
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.CS0013ProfView()

        '○クリア
        If WW_TBLview.Count = 0 Then
            WF_GridPosition.Text = "1"
        Else
            WF_GridPosition.Text = WW_TBLview.Item(0)("LINECNT")
        End If

        '1.現在表示しているLINECNTのリストをビューステートに保持
        '2.チェックがついているチェックボックスオブジェクトをチェック状態にする
        If WW_TBLview.ToTable IsNot Nothing AndAlso WW_TBLview.ToTable.Rows.Count > 0 Then
            Dim displayLineCnt As List(Of Integer) = (From dr As DataRow In WW_TBLview.ToTable
                                                      Select Convert.ToInt32(dr.Item("LINECNT"))).ToList
            ViewState("DISPLAY_LINECNT_LIST") = displayLineCnt
            Dim targetCheckBoxLineCnt = (From dr As DataRow In WW_TBLview.ToTable
                                         Where Convert.ToString(dr.Item("OPERATION")) <> ""
                                         Select Convert.ToInt32(dr.Item("LINECNT")))
            For Each lineCnt As Integer In targetCheckBoxLineCnt
                Dim chkObjId As String = "chk" & Me.pnlListArea.ID & "OPERATION" & lineCnt.ToString
                Dim tmpObj As Control = Me.pnlListArea.FindControl(chkObjId)
                Dim hchkObjId As String = "hchk" & Me.pnlListArea.ID & "OPERATION" & lineCnt.ToString
                Dim htmpObj As Control = Me.pnlListArea.FindControl(hchkObjId)
                If Not IsNothing(tmpObj) AndAlso Not IsNothing(htmpObj) Then
                    Dim chkObj As CheckBox = DirectCast(tmpObj, CheckBox)
                    Dim hchkObj As Label = DirectCast(htmpObj, Label)
                    If hchkObj.Text = "on" Then
                        chkObj.Checked = 1
                    Else
                        chkObj.Checked = 0
                    End If
                End If
            Next
        Else
            ViewState("DISPLAY_LINECNT_LIST") = Nothing
        End If

        WW_TBLview.Dispose()
        WW_TBLview = Nothing
    End Sub

    ''' <summary>
    ''' 全選択ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonALLSELECT_Click()

        '○画面表示データ復元
        Master.RecoverTable(T00006tbl)

        '全チェックボックスON
        For Each row In T00006tbl.Rows
            row("OPERATION") = "on"
        Next

        '○画面表示データ保存
        Master.SaveTable(T00006tbl)

        '画面先頭を表示
        WF_GridPosition.Text = "1"

    End Sub

    ''' <summary>
    ''' 全解除ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonALLCANCEL_Click()

        '○画面表示データ復元
        Master.RecoverTable(T00006tbl)

        '全チェックボックスON
        For Each row In T00006tbl.Rows
            row("OPERATION") = ""
        Next

        '○画面表示データ保存
        Master.SaveTable(T00006tbl)

        '画面先頭を表示
        WF_GridPosition.Text = "1"

    End Sub

    ''' <summary>
    ''' 矢崎ZIPボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonLOCAL_Click()
        '　WORK
        Dim WW_Dir As String = ""
        Dim WW_TEMPDir As String = ""
        Dim wERR As String = ""

        Try
            '　作業フォルダ存在確認＆作成(C:\apple\files\TEXTWORK)
            WW_Dir = Path.Combine(CS0050SESSION.UPLOAD_PATH, "TEXTWORK")
            If System.IO.Directory.Exists(WW_Dir) Then
            Else
                System.IO.Directory.CreateDirectory(WW_Dir)
            End If

            '　車端ファイル格納フォルダ存在確認＆作成(C:\apple\files\TEXTWORK\[部署])
            WW_Dir = Path.Combine(CS0050SESSION.UPLOAD_PATH, "TEXTWORK", work.WF_SEL_SHIPORG.Text)
            If System.IO.Directory.Exists(WW_Dir) Then
                '　車端ファイル格納フォルダ内不要ファイル削除(すべて削除)
                For Each tempFile As String In System.IO.Directory.GetFiles(WW_Dir, "*.*")
                    System.IO.File.Delete(tempFile)
                Next
            Else
                System.IO.Directory.CreateDirectory(WW_Dir)
            End If

            '　TEMPフォルダ存在確認＆作成(C:\apple\files\TEXTWORK\TEMP\[部署])
            WW_TEMPDir = Path.Combine(CS0050SESSION.UPLOAD_PATH, "TEXTWORK", "TEMP", work.WF_SEL_SHIPORG.Text)
            If System.IO.Directory.Exists(WW_TEMPDir) Then
                '　TEMPフォルダ内不要ファイル削除(すべて削除)
                For Each tempFile As String In System.IO.Directory.GetFiles(WW_TEMPDir, "*.*")
                    System.IO.File.Delete(tempFile)
                Next
            Else
                System.IO.Directory.CreateDirectory(WW_TEMPDir)
            End If


            '○画面表示データ復元
            Master.RecoverTable(T00006tbl)

            '　ファイル作成処理
            Call EditFileYAZAKI(WW_TEMPDir, wERR)

            If wERR = "" Then

                '　矢崎ファイルが出力されている場合
                Dim WW_FILECNT() As String = System.IO.Directory.GetFiles(WW_TEMPDir, "*.*")
                If WW_FILECNT.Length <> 0 Then
                    '○現在ログオンしているユーザーの既定の資格情報を使用
                    Dim WW_client As WebClient = New WebClient()
                    WW_client.UseDefaultCredentials = True

                    '○圧縮実行
                    ZipFile.CreateFromDirectory(WW_TEMPDir, Path.Combine(WW_Dir, C_FILE_YAZAKI))

                    'ダウンロード処理へ遷移
                    WF_ZipURL.Value = HttpContext.Current.Request.Url.Scheme & "://" & HttpContext.Current.Request.Url.Host & "/" & "TEXT" & "/" & work.WF_SEL_SHIPORG.Text & "/" & C_FILE_YAZAKI
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_DownLoad()", True)

                    '　正常終了メッセージ
                    Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
                Else
                    '　ファイル存在無しメッセージ
                    Master.Output(C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR, C_MESSAGE_TYPE.ERR)
                End If

            Else
                '　ファイル存在無しメッセージ
                Master.Output(C_MESSAGE_NO.ERROR_RECORD_EXIST, C_MESSAGE_TYPE.ERR)
            End If
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.FILE_IO_ERROR, C_MESSAGE_TYPE.ABORT)
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 矢崎ファイル作成処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub EditFileYAZAKI(ByVal WW_Dir As String, ByRef RTNCD As String)

        '○チェック済みのデータ以外を削除
        CS0026TBLSORTget.TABLE = T00006tbl
        CS0026TBLSORTget.SORTING = "TORICODE,OILTYPE,SHUKADATE,ORDERORG,SHIPORG,SHUKODATE,GSHABAN,RYOME,TRIPNO,DROPNO,PRODUCT1,PRODUCT2"
        CS0026TBLSORTget.FILTER = "OPERATION = 'on'"
        CS0026TBLSORTget.sort(T00006EXPtbl)

        '〇チェック処理（矢崎変換可否）
        For Each T00006row In T00006EXPtbl.Rows

            If IsNothing(T00006row("YTORICODE")) OrElse
               IsNothing(T00006row("YTODOKECODE")) OrElse
               IsNothing(T00006row("YSHUKABASHO")) OrElse
               IsNothing(T00006row("YPRODUCT")) OrElse
               IsNothing(T00006row("YAZKSHABAN")) Then

                RTNCD = "ERR"

                'エラーレポート編集
                rightview.AddErrorReport("・矢崎車端用コード未定義 ( 項番： " & T00006row("LINECNT") & " )")

                '矢崎-取引先
                If String.IsNullOrEmpty(T00006row("YTORICODE")) Then
                    rightview.AddErrorReport("  --> " & " 矢崎-取引先ナシ ")
                End If

                '矢崎-届先
                If String.IsNullOrEmpty(T00006row("YTODOKECODE")) Then
                    rightview.AddErrorReport("  --> " & " 矢崎-届先ナシ ")
                End If

                '矢崎-出荷場所
                If String.IsNullOrEmpty(T00006row("YSHUKABASHO")) Then
                    rightview.AddErrorReport("  --> " & " 矢崎-出荷場ナシ ")
                End If

                '矢崎-品名
                If String.IsNullOrEmpty(T00006row("YPRODUCT")) Then
                    rightview.AddErrorReport("  --> " & " 矢崎-品名ナシ ")
                End If

                '矢崎-車番
                If String.IsNullOrEmpty(T00006row("YAZKSHABAN")) Then
                    rightview.AddErrorReport("  --> " & " 矢崎-車番ナシ ")
                End If
            End If
        Next


        Dim WW_FILENAME As String = ""
        Dim WW_STAFFCODE As String = ""
        Dim WW_GSHABAN As String = ""
        Dim WW_YAZKSHABAN As String = ""
        Dim WW_SHUKODATE As String = ""
        Dim WW_RYOME As String = ""

        'ソート
        CS0026TBLSORTget.TABLE = T00006EXPtbl
        CS0026TBLSORTget.SORTING = "STAFFCODE ASC, YAZKSHABAN ASC, SHUKODATE ASC, RYOME ASC"
        CS0026TBLSORTget.FILTER = ""
        CS0026TBLSORTget.sort(T00006EXPtbl)

        For Each T00006row1 As DataRow In T00006EXPtbl.Rows

            '以下キーがブレイクしたデータをファイル作成
            If T00006row1("STAFFCODE") = WW_STAFFCODE AndAlso
               T00006row1("YAZKSHABAN") = WW_YAZKSHABAN AndAlso
               T00006row1("SHUKODATE") = WW_SHUKODATE AndAlso
               T00006row1("RYOME") = WW_RYOME Then
            Else
                Dim WW_YAZAKI As New YAZAKI
                Dim WW_POSITION As Integer = 1
                Dim WW_FIXFILENO As Integer = 0

                'ブレイクキーデータ作成
                WW_STAFFCODE = T00006row1("STAFFCODE")
                WW_YAZKSHABAN = T00006row1("YAZKSHABAN")
                WW_SHUKODATE = T00006row1("SHUKODATE")
                WW_RYOME = T00006row1("RYOME")

                'ファイル名作成
                WW_FILENAME = Right("00000000" & Trim(T00006row1("STAFFCODE")), 8) & "-" &
                              Right("00000000" & Trim(T00006row1("YAZKSHABAN")), 8) & "-" &
                              T00006row1("SHUKODATE").Replace("/", "") & "-" &
                              T00006row1("SHUKODATE").Replace("/", "") & "-" &
                              T00006row1("RYOME").PadLeft(2, "0"c)
                'WW_FILENAME = T00006row1("STAFFCODE").PadLeft(8, "0"c) & "-" & _
                '              T00006row1("YAZKSHABAN").PadLeft(8, "0"c) & "-" & _
                '              T00006row1("SHUKADATE").Replace("/", "") & "-" & _
                '              T00006row1("TODOKEDATE").Replace("/", "") & "-" & _
                '              T00006row1("RYOME").PadLeft(2, "0"c)

                'ファイルオープン
                Using YFile As New System.IO.StreamWriter(WW_Dir & "\" & WW_FILENAME & ".TXT", True, System.Text.Encoding.GetEncoding("sjis"))
                    Dim WW_TEXT As StringBuilder = New StringBuilder()

                    '出力ファイル内容設定
                    For Each T00006row2 As DataRow In T00006EXPtbl.Rows

                        '以下キーがブレイクするまでのレコードを1ファイル分作成
                        If T00006row2("STAFFCODE") = WW_STAFFCODE And
                           T00006row2("YAZKSHABAN") = WW_YAZKSHABAN And
                           T00006row2("SHUKODATE") = WW_SHUKODATE And
                           T00006row2("RYOME") = WW_RYOME Then

                            'トリップが複数存在する場合、[届先Ｃ]、[届先略称]に[トリップ：下1桁]を付与
                            Dim WW_TODOKECODE As String = T00006row2("YTODOKECODE").PadLeft(9, "0"c)
                            Dim WW_TODOKECODENAME As String = StrConv(T00006row2("TODOKECODENAME"), VbStrConv.Wide).PadRight(9, "　")
                            If T00006row2("TRIPNO") <> "01" Then
                                WW_TODOKECODE = WW_TODOKECODE + Right(T00006row2("TRIPNO"), 1)
                                WW_TODOKECODENAME = StrConv(Right(T00006row2("TRIPNO"), 1), VbStrConv.Wide) + WW_TODOKECODENAME
                            Else
                                WW_TODOKECODE = WW_TODOKECODE + "0"
                                WW_TODOKECODENAME = WW_TODOKECODENAME + "　"         '全角スペース
                            End If






                            'レイアウトに従って明細を作成
                            '★業務車番
                            If String.IsNullOrEmpty(T00006row2("YAZKSHABAN")) Then
                                WW_TEXT.Append(Space(8))
                            Else
                                'WW_TEXT = WW_TEXT & T00006row2("YAZKSHABAN").PadRight(8, "0"c)
                                WW_TEXT.Append(Right("00000000" & Trim(T00006row2("YAZKSHABAN")), 8))
                            End If

                            '★両目
                            If String.IsNullOrEmpty(T00006row2("RYOME")) Then
                                WW_TEXT.Append(Space(1))
                            Else
                                WW_TEXT.Append(Right("0" & Trim(T00006row2("RYOME")), 1))
                            End If

                            '★トリップ
                            If String.IsNullOrEmpty(T00006row2("TRIPNO")) Then
                                WW_TEXT.Append(Space(2))
                            Else
                                'WW_TEXT = WW_TEXT & Mid(T00006row2("TRIPNO"), 2, 2).PadLeft(2, "0"c)
                                WW_TEXT.Append(Right("00" & Trim(T00006row2("TRIPNO")), 2))
                            End If

                            '★卸順
                            If String.IsNullOrEmpty(T00006row2("DROPNO")) Then
                                WW_TEXT.Append(Space(2))
                            Else
                                'WW_TEXT = WW_TEXT & Mid(T00006row2("DROPNO"), 2, 2)
                                WW_TEXT.Append(Right("00" & Trim(T00006row2("DROPNO")), 2))
                            End If

                            '出荷年月日
                            If String.IsNullOrEmpty(T00006row2("SHUKODATE")) Then
                                WW_TEXT.Append(Space(8))
                            Else
                                WW_TEXT.Append(T00006row2("SHUKODATE").Replace("/", ""))
                            End If

                            '荷卸年月日
                            If String.IsNullOrEmpty(T00006row2("SHUKODATE")) Then
                                WW_TEXT.Append(Space(8))
                            Else
                                WW_TEXT.Append(T00006row2("SHUKODATE").Replace("/", ""))
                            End If

                            '★荷主Ｃ、荷主名
                            If String.IsNullOrEmpty(T00006row2("YTORICODE")) Then
                                WW_TEXT.Append(Space(3))
                                WW_TEXT.Append(Mid(StrConv(Space(5), VbStrConv.Wide), 1, 5))
                            Else
                                'WW_TEXT = WW_TEXT & T00006row2("YTORICODE").PadLeft(3, "0"c)
                                WW_TEXT.Append(Right("000" & Trim(T00006row2("YTORICODE")), 3))
                                WW_TEXT.Append(Mid(StrConv(T00006row2("TORICODENAME"), VbStrConv.Wide).PadRight(5, "　"), 1, 5))
                            End If

                            '★出荷基地Ｃ、出荷基地略称
                            If String.IsNullOrEmpty(T00006row2("YSHUKABASHO")) Then
                                WW_TEXT.Append(Space(4))
                                WW_TEXT.Append(Mid(StrConv(Space(8), VbStrConv.Wide), 1, 8))
                            Else
                                'WW_TEXT = WW_TEXT & Mid(T00006row2("YSHUKABASHO") & "    ", 1, 4)
                                WW_TEXT.Append(Right("0000" & Trim(T00006row2("YSHUKABASHO")), 4))
                                WW_TEXT.Append(Mid(StrConv(T00006row2("SHUKABASHONAME"), VbStrConv.Wide).PadRight(8, "　"), 1, 8))
                            End If

                            '★届先Ｃ、届先略称
                            If String.IsNullOrEmpty(T00006row2("TODOKECODE")) Then
                                WW_TEXT.Append(Space(10))
                                WW_TEXT.Append(Mid(StrConv(Space(10), VbStrConv.Wide), 1, 10))
                            Else
                                'WW_TEXT = WW_TEXT & WW_TODOKECODE.PadRight(10, "0"c)
                                WW_TEXT.Append(Right("0000000000" & Trim(WW_TODOKECODE), 10))
                                WW_TEXT.Append(Mid(StrConv(WW_TODOKECODENAME, VbStrConv.Wide).PadRight(10, "　"), 1, 10))
                            End If

                            '★品名Ｃ、品名略称
                            If String.IsNullOrEmpty(T00006row2("YPRODUCT")) Then
                                WW_TEXT.Append(Space(5))
                                WW_TEXT.Append(Mid(StrConv(Space(10), VbStrConv.Wide), 1, 10))
                            Else
                                'WW_TEXT = WW_TEXT & T00006row2("YPRODUCT")
                                WW_TEXT.Append(Right("00000" & Trim(T00006row2("YPRODUCT")), 5))
                                WW_TEXT.Append(Mid(StrConv(T00006row2("PRODUCT2NAME"), VbStrConv.Wide).PadRight(10, "　"), 1, 10))
                            End If

                            '数量
                            If String.IsNullOrEmpty(T00006row2("SURYO")) Then
                                WW_TEXT.Append(Space(11))
                            Else
                                Dim WW_SURYO As String() = T00006row2("SURYO").split(".")
                                WW_TEXT.Append(WW_SURYO(0).PadLeft(6, "0"c) & WW_SURYO(1).PadRight(5, "0"c))
                            End If

                            '★正乗務員Ｃ
                            If String.IsNullOrEmpty(T00006row2("STAFFCODE")) Then
                                WW_TEXT.Append(Space(8))
                            Else
                                'WW_TEXT = WW_TEXT & T00006row2("STAFFCODE").PadLeft(8, "0"c)
                                WW_TEXT.Append(Right("00000000" & Trim(T00006row2("STAFFCODE")), 8))
                            End If

                            '出勤指定時刻
                            If String.IsNullOrEmpty(T00006row2("STTIME")) Then
                                WW_TEXT.Append(Space(4))
                            Else
                                WW_TEXT.Append(T00006row2("STTIME").Replace(":", "").PadLeft(4, "0"c))
                            End If

                            '★副乗務員Ｃ
                            If String.IsNullOrEmpty(T00006row2("SUBSTAFFCODE")) Then
                                WW_TEXT.Append(Space(8))
                            Else
                                'WW_TEXT = WW_TEXT & T00006row2("SUBSTAFFCODE").PadLeft(8, "0"c)
                                WW_TEXT.Append(Right("00000000" & Trim(T00006row2("SUBSTAFFCODE")), 8))
                            End If

                            '改行
                            WW_TEXT.AppendLine()

                            '次行設定
                            WW_POSITION += 1
                        End If
                    Next
                    'ファイルクローズ
                    YFile.Write(WW_TEXT)
                    YFile.Close()
                End Using

            End If
        Next

    End Sub

    ''' <summary>
    ''' 光英CSVボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCSV_Click()

        Dim WW_Dir As String = ""
        Dim wERR As String = ""

        Try

            '　作業フォルダ存在確認＆作成(C:\apple\files\TEXTWORK)
            WW_Dir = Path.Combine(CS0050SESSION.UPLOAD_PATH, "TEXTWORK")
            If System.IO.Directory.Exists(WW_Dir) Then
            Else
                System.IO.Directory.CreateDirectory(WW_Dir)
            End If

            '　車端ファイル格納フォルダ存在確認＆作成(C:\apple\files\TEXTWORK\[部署])
            WW_Dir = Path.Combine(CS0050SESSION.UPLOAD_PATH, "TEXTWORK", work.WF_SEL_SHIPORG.Text)
            If System.IO.Directory.Exists(WW_Dir) Then
                '　車端ファイル格納フォルダ内不要ファイル削除(すべて削除)
                For Each tempFile As String In System.IO.Directory.GetFiles(WW_Dir, "*.*")
                    System.IO.File.Delete(tempFile)
                Next
            Else
                System.IO.Directory.CreateDirectory(WW_Dir)
            End If

            '○画面表示データ復元
            If Master.RecoverTable(T00006tbl) <> True Then Exit Sub

            Dim WW_FileName = CONST_KOUEI & ".CSV"
            Call EditFileJOTKOUEI(Path.Combine(WW_Dir, WW_FileName), wERR)
            If wERR = "" Then

                'ダウンロード処理へ遷移
                WF_ZipURL.Value = HttpContext.Current.Request.Url.Scheme & "://" & HttpContext.Current.Request.Url.Host & "/" & "TEXT" & "/" & work.WF_SEL_SHIPORG.Text & "/" & WW_FileName
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_DownLoad()", True)

                '　正常終了メッセージ
                Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
            End If

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.FILE_IO_ERROR, C_MESSAGE_TYPE.ABORT)
            Exit Sub
        End Try

    End Sub


    ''' <summary>
    ''' 光英(JOT)ファイル作成処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub EditFileJOTKOUEI(ByVal FILEPATH As String, ByRef RTNCD As String)

        '○選択データの抽出
        '○JXオーダーのデータを除外
        'ソート
        CS0026TBLSORTget.TABLE = T00006tbl
        CS0026TBLSORTget.SORTING = "TORICODE,OILTYPE,SHUKADATE,ORDERORG,SHIPORG,SHUKODATE,GSHABAN,RYOME,TRIPNO,DROPNO,PRODUCT1,PRODUCT2"
        CS0026TBLSORTget.FILTER = "OPERATION = 'on' and JXORDERID = ''"
        CS0026TBLSORTget.sort(T00006EXPtbl)

        ''出庫日で染める
        'For Each WW_row As DataRow In T00006EXPtbl.Rows
        '    WW_row("SHUKADATE") = WW_row("SHUKODATE")
        '    WW_row("TODOKEDATE") = WW_row("SHUKODATE")
        'Next

        '○ 帳票出力dll Interface
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text           '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                    'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                           'PARAM01:画面ID
        CS0030REPORT.REPORTID = rightview.GetReportId()             'PARAM02:帳票ID
        CS0030REPORT.FILEtyp = "CSV"                                'PARAM03:出力ファイル形式
        CS0030REPORT.TBLDATA = T00006EXPtbl                         'PARAM04:データ参照tabledata
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORT")
            RTNCD = "ERR"
            Exit Sub
        End If

        '---------------------------------------------------------------------------------------------------
        'CSV出力されたファイルの編集（CSVからタブ区切り、項目単位にダブルクォートで括る）　開始 2017/03/13
        '---------------------------------------------------------------------------------------------------
        Using InputF As New System.IO.StreamReader(CS0030REPORT.FILEpath, System.Text.Encoding.GetEncoding("sjis")),
              SaveF As New System.IO.StreamWriter(FILEPATH, True, System.Text.Encoding.GetEncoding("sjis"))
            Dim WW_LineData As String
            Dim WW_LineDataOut As String

            'File内容をすべて読み込む
            While (Not InputF.EndOfStream)
                '先頭、最後尾にダブルクォートを設定
                WW_LineData = """" & InputF.ReadLine() & """"
                'カンマをタブに置換し、前後にダブルクォート付加する
                WW_LineDataOut = Replace(WW_LineData, ",", """" & vbTab & """")
                'ファイル書き込み
                SaveF.WriteLine(WW_LineDataOut)
            End While
            InputF.Close()
            SaveF.Close()

        End Using
        '---------------------------------------------------------------------------------------------------
        'CSV出力されたファイルの編集（CSVからタブ区切り、項目単位にダブルクォートで括る）　終了 2017/03/13
        '---------------------------------------------------------------------------------------------------

    End Sub

    ''' <summary>
    ''' 光英送信ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonPut_Click()

        Dim WW_Dir As String = ""
        Dim wERR As String = ""

        Try

            '　作業フォルダ存在確認＆作成(C:\apple\files\TEXTWORK)
            WW_Dir = Path.Combine(CS0050SESSION.UPLOAD_PATH, "TEXTWORK")
            If System.IO.Directory.Exists(WW_Dir) Then
            Else
                System.IO.Directory.CreateDirectory(WW_Dir)
            End If

            '　車端ファイル格納フォルダ存在確認＆作成(C:\apple\files\TEXTWORK\[部署])
            WW_Dir = Path.Combine(CS0050SESSION.UPLOAD_PATH, "TEXTWORK", work.WF_SEL_SHIPORG.Text)
            If System.IO.Directory.Exists(WW_Dir) Then
                '　車端ファイル格納フォルダ内不要ファイル削除(すべて削除)
                For Each tempFile As String In System.IO.Directory.GetFiles(WW_Dir, "*.*")
                    System.IO.File.Delete(tempFile)
                Next
            Else
                System.IO.Directory.CreateDirectory(WW_Dir)
            End If

            '○画面表示データ復元
            If Master.RecoverTable(T00006tbl) <> True Then Exit Sub

            'FTP送信対象リスト
            Dim lstTarget As List(Of String) = New List(Of String)

            '光英(JOT)ファイル編集処理
            Call EditFileJOTKOUEI(Path.Combine(WW_Dir, "jot_" & Date.Today.ToString("yyyyMMdd") & ".csv"), wERR)
            If Not String.IsNullOrEmpty(wERR) Then
                Exit Sub
            End If
            lstTarget.Add(FTP_TERGET.JOT)

            '光英データ管理
            Dim koueiMng As GRW0001KOUEIORDER = New GRW0001KOUEIORDER With {
                .CAMPCODE = work.WF_SEL_CAMPCODE.Text,
                .ORGCODE = work.WF_SEL_SHIPORG.Text,
                .KIJUNDATEF = work.WF_SEL_SHUKODATEF.Text,
                .KIJUNDATET = work.WF_SEL_SHUKODATET.Text
            }
            If koueiMng.ReadOrder <> True Then
                Exit Sub
            End If
            Dim KOUEItbl = koueiMng.GetOrder()
            '光英ファイル編集処理
            Call EditFileKOUEI(KOUEItbl, wERR)
            '光英ファイルCSV出力
            If Not String.IsNullOrEmpty(wERR) Then
                Exit Sub
            End If

            Dim lstType = KOUEItbl.GroupBy(Function(x) x.Value.KOUEITYPE)
            For Each type In lstType
                Dim lstDate = type.GroupBy(Function(x) x.Value.KIJUNDATE)
                For Each kijunDate In lstDate
                    'CSV書込
                    If koueiMng.WriteCSV(type.Key, kijunDate.Key, WW_Dir) <> True Then
                        Exit Sub
                    End If
                Next

                Select Case type.Key
                    Case GRW0001KOUEIORDER.KOUEITYPE_PREFIX.JX
                        lstTarget.Add(FTP_TERGET.JX)
                    Case GRW0001KOUEIORDER.KOUEITYPE_PREFIX.TG
                        lstTarget.Add(FTP_TERGET.JXTG)
                    Case GRW0001KOUEIORDER.KOUEITYPE_PREFIX.COSMO
                        lstTarget.Add(FTP_TERGET.COSMO)
                End Select
            Next

            'FTP送信処理
            If PutFileKOUEI(lstTarget) <> True Then
                Exit Sub
            End If

            '　正常終了メッセージ
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.FILE_IO_ERROR, C_MESSAGE_TYPE.ABORT)
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 光英ファイル作成処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub EditFileKOUEI(ByRef koueiOrder As Dictionary(Of String, GRW0001KOUEIORDER.KOUEI_ORDER),
                                  ByRef RTNCD As String)

        '光英マスターデータ管理
        Dim koueiMasterMng As KOUEI_MASTER = New KOUEI_MASTER With {
            .ORGCODE = work.WF_SEL_SHIPORG.Text
        }
        '乗務員マスタ取得
        If koueiMasterMng.ReadStaff <> True Then
            RTNCD = "ERR"
            Exit Sub
        End If

        For Each T00006row As DataRow In T00006tbl.Rows
            '○JXオーダーのデータ以外を除外
            If String.IsNullOrEmpty(T00006row("JXORDERID").ToString) Then Continue For

            Dim order As GRW0001KOUEIORDER.KOUEI_ORDER = Nothing
            'TRIPSEQ=0（回順：始業）に乗務員情報を編集する
            '※同一トリップのドロップ毎で異なる乗務員が設定されていた場合はあと勝ちとなる

            '光英オーダーDictionary検索キー（ORDERID|TRIPSEQ）
            Dim tmpOrderId As String = T00006row("JXORDERID").Replace("W", "")
            Dim targetKey = tmpOrderId & C_VALUE_SPLIT_DELIMITER & GRW0001KOUEIORDER.TRIPSEQ_TYPE.START

            If koueiOrder.ContainsKey(targetKey) Then
                order = koueiOrder.Item(targetKey)
            Else
                '右Boxエラー内容出力'
                '光英オーダーNotFound
                RTNCD = "ERR"
                Exit Sub
            End If

            '光英タイプ取得
            Dim tmpkoueiType As String = tmpOrderId.Chars(0)
            Dim koueiType As String = String.Empty
            If tmpkoueiType = GRW0001KOUEIORDER.KOUEITYPE_PREFIX.JXTG.ToUpper.Chars(0) Then
                koueiType = GRW0001KOUEIORDER.KOUEITYPE_PREFIX.JXTG
            ElseIf tmpkoueiType = GRW0001KOUEIORDER.KOUEITYPE_PREFIX.COSMO.ToUpper.Chars(0) Then
                koueiType = GRW0001KOUEIORDER.KOUEITYPE_PREFIX.COSMO
            End If

            If Not String.IsNullOrEmpty(T00006row("STAFFCODE")) Then
                Dim staff = koueiMasterMng.GetStaff2No(koueiType, T00006row("STAFFCODE"))
                If Not IsNothing(staff) Then
                    order.STAFFCODE = staff.STAFFCODE
                    order.SetStaffInfo(staff.STAFFNO, staff.STAFFNAME)
                Else
                    '光英変換エラー
                    order.SetStaffInfo(T00006row("STAFFCODE"), T00006row("STAFFCODENAME"))
                End If
            End If

            If Not String.IsNullOrEmpty(T00006row("SUBSTAFFCODE")) Then
                Dim staff = koueiMasterMng.GetStaff2No(koueiType, T00006row("SUBSTAFFCODE"))
                If Not IsNothing(staff) Then
                    order.SUBSTAFFCODE = staff.STAFFCODE
                    order.SetSubStaffInfo(staff.STAFFNO, staff.STAFFNAME)
                Else
                    '光英変換エラー
                    order.SetSubStaffInfo(T00006row("SUBSTAFFCODE"), T00006row("SUBSTAFFCODENAME"))
                End If
            End If

        Next

    End Sub

    ''' <summary>
    ''' 光英ファイル送信処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Function PutFileKOUEI(ByRef lstTarget As List(Of String)) As Boolean

        '〇ファイル受信
        Dim FtpControl As New FtpControl With {
            .OrgCode = work.WF_SEL_SHIPORG.Text
        }

        For Each target In lstTarget

            FtpControl.Request(target)
            If Not isNormal(FtpControl.ERR) Then
                '対象ターゲットでの処理でエラー
                Master.Output(FtpControl.ERR, C_MESSAGE_TYPE.ABORT, FtpControl.TargetID)
                Return False
            Else
                Dim result = FtpControl.Result.TrueForAll(Function(x) x.Status = FtpControl.FTP_RESULT.OK)
                If result = False Then
                    '対象ターゲットのファイル一式全てが正常ではない場合はエラー
                    Master.Output(C_MESSAGE_NO.FTP_FILE_PUT_ERROR, C_MESSAGE_TYPE.ABORT, FtpControl.TargetID)
                    Return False
                End If
            End If

        Next

        Return True

    End Function

    ''' <summary>
    ''' 終了ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()
        '画面遷移実行
        Master.TransitionPrevPage()

    End Sub

    ''' <summary>
    ''' 先頭頁移動ボタン押下
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonFIRST_Click()

        '○画面表示データ復元
        Master.RecoverTable(T00006tbl)

        '○先頭頁に移動
        WF_GridPosition.Text = "1"
    End Sub
    ''' <summary>
    ''' 最終頁ボタン処理 
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonLAST_Click()

        '○画面表示データ復元
        Master.RecoverTable(T00006tbl)

        '○ソート
        Dim WW_TBLview As DataView
        WW_TBLview = New DataView(T00006tbl)
        WW_TBLview.RowFilter = "HIDDEN= '0'"

        '最終頁に移動
        If WW_TBLview.Count Mod CONST_SCROLLROWCOUNT = 0 Then
            WF_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod CONST_SCROLLROWCOUNT)
        Else
            WF_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod CONST_SCROLLROWCOUNT) + 1
        End If
    End Sub

    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_GRID_Scrole()

        '○画面表示データ復元
        Master.RecoverTable(T00006tbl)
    End Sub


    ''' <summary>
    ''' 右ボックスのラジオボタン選択時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RadioButon_Click()
        '〇RightBox処理（ラジオボタン選択）
        If Not String.IsNullOrEmpty(WF_RightViewChange.Value) Then
            If Integer.TryParse(WF_RightViewChange.Value, WF_RightViewChange.Value) Then
                rightview.SelectIndex(WF_RightViewChange.Value)
                WF_RightViewChange.Value = ""
            End If
        End If
    End Sub

    ''' <summary>
    ''' メモ欄変更時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_MEMO_Change()
        '〇RightBox処理（右Boxメモ変更時）
        rightview.Save(Master.USERID, Master.USERTERMID, WW_DUMMY)
    End Sub

    ' ******************************************************************************
    ' ***  共通処理                                                              ***　★
    ' ******************************************************************************

    ''' <summary>
    ''' LeftBoxより名称取得＆チェック
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByRef I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String, Optional ByVal args As Hashtable = Nothing)

        '○名称取得
        O_TEXT = ""
        O_RTN = C_MESSAGE_NO.NORMAL

        '入力値が空は終了
        If String.IsNullOrEmpty(I_VALUE) Then Exit Sub
        With leftview
            Select Case I_FIELD
                Case "STAFFCODE", "SUBSTAFFCODE"
                    '乗務員コード/副乗務員コード名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_STAFFCODE, I_VALUE, O_TEXT, O_RTN, work.createSTAFFParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_SHIPORG.Text))
                Case "GSHABAN"
                    '業務車番名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_WORKLORRY, I_VALUE, O_TEXT, O_RTN, work.createWorkLorryParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_SHIPORG.Text))
                Case "TORICODE"
                    '取引先名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_CUSTOMER, I_VALUE, O_TEXT, O_RTN, work.createTORIParam(work.WF_SEL_CAMPCODE.Text))
                Case "SHUKABASHO"
                    '出荷場所名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_DISTINATION, I_VALUE, O_TEXT, O_RTN, work.createDistinationParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_SHIPORG.Text, "", "2"))
                Case "TODOKECODE"
                    '届先コード名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_DISTINATION, I_VALUE, O_TEXT, O_RTN, work.createDistinationParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_SHIPORG.Text, "", "1"))
                Case "OILTYPE"
                    '油種名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_OILTYPE, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text))
                Case "PRODUCTCODE"
                    '品名名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_GOODS, I_VALUE, O_TEXT, O_RTN, work.createGoodsParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_SHIPORG.Text))
            End Select
        End With

    End Sub

    ''' <summary>
    ''' 画面表示用データ取得
    ''' </summary>
    ''' <remarks>データベース（T00004）を検索し画面表示用データを取得する</remarks>
    Private Sub DBselect_T4SELECT()

        Dim WW_DATE As Date
        Dim WW_TIME As DateTime

        '■■■ 画面表示用データ取得 ■■■
        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection()
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            Dim SQLStr As String =
                 "SELECT 0                                     as LINECNT ,                 " _
               & "       ''                                    as OPERATION ,                 " _
               & "       1                                     as 'SELECT' ,                 " _
               & "       0                                     as HIDDEN ,                  " _
               & "       '0'                                   as WORK_NO ,                 " _
               & "       isnull(rtrim(A.CAMPCODE),'')          as CAMPCODE ,                " _
               & "       isnull(rtrim(A.TERMORG),'')           as TERMORG ,                 " _
               & "       isnull(rtrim(A.ORDERNO),'')           as ORDERNO ,                 " _
               & "       isnull(rtrim(A.DETAILNO),'')          as DETAILNO ,                " _
               & "       isnull(rtrim(A.TRIPNO),'')            as TRIPNO ,                  " _
               & "       isnull(rtrim(A.DROPNO),'')            as DROPNO ,                  " _
               & "       isnull(rtrim(A.SEQ),'00')             as SEQ ,                     " _
               & "       isnull(rtrim(A.TORICODE),'')          as TORICODE ,                " _
               & "       isnull(rtrim(A.OILTYPE),'')           as OILTYPE ,                 " _
               & "       isnull(rtrim(A.STORICODE),'')         as STORICODE ,               " _
               & "       isnull(rtrim(A.ORDERORG),'')          as ORDERORG ,                " _
               & "       isnull(rtrim(A.SHUKODATE),'')         as SHUKODATE ,               " _
               & "       isnull(rtrim(A.KIKODATE),'')          as KIKODATE ,                " _
               & "       isnull(rtrim(A.SHUKADATE),'')         as SHUKADATE ,               " _
               & "       isnull(rtrim(A.TUMIOKIKBN),'')        as TUMIOKIKBN ,              " _
               & "       isnull(rtrim(A.URIKBN),'')            as URIKBN ,                  " _
               & "       isnull(rtrim(A.STATUS),'')            as STATUS ,                  " _
               & "       isnull(rtrim(A.SHIPORG),'')           as SHIPORG ,                 " _
               & "       isnull(rtrim(A.SHUKABASHO),'')        as SHUKABASHO ,              " _
               & "       isnull(rtrim(A.INTIME),'')            as INTIME ,                  " _
               & "       isnull(rtrim(A.OUTTIME),'')           as OUTTIME ,                 " _
               & "       isnull(rtrim(A.SHUKADENNO),'')        as SHUKADENNO ,              " _
               & "       isnull(rtrim(A.TUMISEQ),'')           as TUMISEQ ,                 " _
               & "       isnull(rtrim(A.TUMIBA),'')            as TUMIBA ,                  " _
               & "       isnull(rtrim(A.GATE),'')              as GATE ,                    " _
               & "       isnull(rtrim(A.GSHABAN),'')           as GSHABAN ,                 " _
               & "       isnull(rtrim(A.RYOME),'')             as RYOME ,                   " _
               & "       isnull(rtrim(A.CONTCHASSIS),'')       as CONTCHASSIS ,             " _
               & "       isnull(rtrim(A.SHAFUKU),'')           as SHAFUKU ,                 " _
               & "       isnull(rtrim(A.STAFFCODE),'')         as STAFFCODE ,               " _
               & "       isnull(rtrim(A.SUBSTAFFCODE),'')      as SUBSTAFFCODE ,            " _
               & "       isnull(rtrim(A.STTIME),'')            as STTIME ,                  " _
               & "       isnull(rtrim(A.TORIORDERNO),'')       as TORIORDERNO ,             " _
               & "       isnull(rtrim(A.TODOKEDATE),'')        as TODOKEDATE ,              " _
               & "       isnull(rtrim(A.TODOKETIME),'')        as TODOKETIME ,              " _
               & "       isnull(rtrim(A.TODOKECODE),'')        as TODOKECODE ,              " _
               & "       isnull(rtrim(A.PRODUCT1),'')          as PRODUCT1 ,                " _
               & "       isnull(rtrim(A.PRODUCT2),'')          as PRODUCT2 ,                " _
               & "       isnull(rtrim(A.PRODUCTCODE),'')       as PRODUCTCODE ,             " _
               & "       isnull(rtrim(A.PRATIO),'')            as PRATIO ,                  " _
               & "       isnull(rtrim(A.SMELLKBN),'')          as SMELLKBN ,                " _
               & "       isnull(rtrim(A.CONTNO),'')            as CONTNO ,                  " _
               & "       isnull(rtrim(A.HTANI),'')             as HTANI ,                   " _
               & "       isnull(rtrim(A.SURYO),'')             as SURYO ,                   " _
               & "       isnull(rtrim(A.DAISU),'')             as DAISU ,                   " _
               & "       isnull(rtrim(A.JSURYO),'')            as JSURYO ,                  " _
               & "       isnull(rtrim(A.JDAISU),'')            as JDAISU ,                  " _
               & "       isnull(rtrim(A.REMARKS1),'')          as REMARKS1 ,                " _
               & "       isnull(rtrim(A.REMARKS2),'')          as REMARKS2 ,                " _
               & "       isnull(rtrim(A.REMARKS3),'')          as REMARKS3 ,                " _
               & "       isnull(rtrim(A.REMARKS4),'')          as REMARKS4 ,                " _
               & "       isnull(rtrim(A.REMARKS5),'')          as REMARKS5 ,                " _
               & "       isnull(rtrim(A.REMARKS6),'')          as REMARKS6 ,                " _
               & "       isnull(rtrim(A.JXORDERID),'')         as JXORDERID ,               " _
               & "       isnull(rtrim(A.DELFLG),'')            as DELFLG ,                  " _
               & "       TIMSTP = cast(A.UPDTIMSTP  as bigint) ,                            " _
               & "       isnull(rtrim(B.SHARYOINFO1),'')       as SHARYOINFO1 ,             " _
               & "       isnull(rtrim(B.SHARYOINFO2),'')       as SHARYOINFO2 ,             " _
               & "       isnull(rtrim(B.SHARYOINFO3),'')       as SHARYOINFO3 ,             " _
               & "       isnull(rtrim(B.SHARYOINFO4),'')       as SHARYOINFO4 ,             " _
               & "       isnull(rtrim(B.SHARYOINFO5),'')       as SHARYOINFO5 ,             " _
               & "       isnull(rtrim(B.SHARYOINFO6),'')       as SHARYOINFO6 ,             " _
               & "       isnull(rtrim(C.ARRIVTIME),'')         as ARRIVTIME ,               " _
               & "       isnull(rtrim(C.DISTANCE),'')          as DISTANCE ,                " _
               & "       isnull(rtrim(D.ADDR1),'') +              				            " _
               & "       isnull(rtrim(D.ADDR2),'') +            				            " _
               & "       isnull(rtrim(D.ADDR3),'') +             				            " _
               & "       isnull(rtrim(D.ADDR4),'')             as ADDR ,                    " _
               & "       isnull(rtrim(D.NOTES1),'')        	   as NOTES1 ,                  " _
               & "       isnull(rtrim(D.NOTES2),'')            as NOTES2 ,                  " _
               & "       isnull(rtrim(D.NOTES3),'')            as NOTES3 ,                  " _
               & "       isnull(rtrim(D.NOTES4),'')            as NOTES4 ,                  " _
               & "       isnull(rtrim(D.NOTES5),'')            as NOTES5 ,                  " _
               & "       ''                                    as TUMIOKI ,                 " _
               & "       isnull(M1.NAMES,'')                   as CAMPCODENAME ,            " _
               & "       isnull(M2_T.NAMES,'')                 as TERMORGNAME ,             " _
               & "       ''                                    as TORICODENAME ,            " _
               & "       ''                                    as OILTYPENAME ,             " _
               & "       ''                                    as STORICODENAME ,           " _
               & "       isnull(M2_O.NAMES,'')                 as ORDERORGNAME ,            " _
               & "       isnull(MC1_5.VALUE1,'')               as URIKBNNAME ,              " _
               & "       isnull(MC1_1.VALUE1,'')               as STATUSNAME ,              " _
               & "       isnull(M2_S.NAMES,'')                 as SHIPORGNAME ,             " _
               & "       isnull(MC1_2.VALUE1,'')               as TUMIOKIKBNNAME ,          " _
               & "       ''                                    as SHUKABASHONAME ,          " _
               & "       ''                                    as GSHABANLICNPLTNO ,        " _
               & "       isnull(rtrim(MA4.LICNPLTNO1),'') +                                 " _
               & "       isnull(rtrim(MA4.LICNPLTNO2),'')      as CONTCHASSISLICNPLTNO ,    " _
               & "       ''                                    as STAFFCODENAME ,           " _
               & "       ''                                    as SUBSTAFFCODENAME ,        " _
               & "       ''                                    as TODOKECODENAME ,          " _
               & "       isnull(MC1_3.VALUE1,'')               as PRODUCT1NAME ,            " _
               & "       ''                                    as PRODUCT2NAME ,            " _
               & "       ''                                    as PRODUCTNAME ,             " _
               & "       isnull(MC1_4.VALUE1,'')               as SMELLKBNNAME ,            " _
               & "       ''                                    as SURYO_SUM ,               " _
               & "       ''                                    as DAISU_SUM ,               " _
               & "       isnull(rtrim(E.YTORICODE),'')         as YTORICODE ,               " _
               & "       isnull(rtrim(C.YTODOKECODE),'')       as YTODOKECODE ,             " _
               & "       isnull(rtrim(G.YTODOKECODE),'')       as YSHUKABASHO ,             " _
               & "       isnull(rtrim(H.YPRODUCT),'')          as YPRODUCT ,                " _
               & "       isnull(rtrim(B.YAZKSHABAN),'')        as YAZKSHABAN,               " _
               & "       isnull(rtrim(B.SHARYOTYPEF),'')       as SHARYOTYPEF,              " _
               & "       isnull(rtrim(B.TSHABANF),'')          as TSHABANF,                 " _
               & "       isnull(rtrim(B.SHARYOTYPEB),'')       as SHARYOTYPEB,              " _
               & "       isnull(rtrim(B.TSHABANB),'')          as TSHABANB,                 " _
               & "       isnull(rtrim(B.SHARYOTYPEB2),'')      as SHARYOTYPEB2 ,            " _
               & "       isnull(rtrim(B.TSHABANB2),'')         as TSHABANB2 ,               " _
               & "       isnull(rtrim(B.SHARYOTYPEF),'') + isnull(rtrim(B.TSHABANF),'')      as TSHABANFRONT,  " _
               & "       isnull(rtrim(B.SHARYOTYPEB),'') + isnull(rtrim(B.TSHABANB),'')      as TSHABANBACK    " _
               & " FROM  T0004_HORDER AS A								                    " _
               & " INNER JOIN ( SELECT Y.CAMPCODE ,	Y.CODE                                  " _
               & "               FROM  S0006_ROLE Y				                            " _
               & "               WHERE Y.CAMPCODE 	 	   = @P01		                    " _
               & "                 and Y.OBJECT       	   = 'ORG'		                    " _
               & "                 and Y.ROLE              = @P02		                    " _
               & "                 and Y.PERMITCODE        = '2'                            " _
               & "                 and Y.STYMD            <= @P03		                    " _
               & "                 and Y.ENDYMD           >= @P03		                    " _
               & "                 and Y.DELFLG           <> '1'		                    " _
               & "            ) AS Z									                    " _
               & "    ON  Z.CAMPCODE		               = A.CAMPCODE    		            " _
               & "   and  Z.CODE       	                   = A.SHIPORG 	    	            " _
               & "  LEFT JOIN MA006_SHABANORG B 						                    " _
               & "    ON B.CAMPCODE     	               = A.CAMPCODE 				    " _
               & "   and B.GSHABAN      	               = A.GSHABAN 				        " _
               & "   and B.MANGUORG     	               = A.SHIPORG 				        " _
               & "   and B.DELFLG                         <> '1' 						    " _
               & "  LEFT JOIN MC007_TODKORG C 							                    " _
               & "    ON C.CAMPCODE     	               = A.CAMPCODE 				    " _
               & "   and C.TODOKECODE   	               = A.TODOKECODE 				    " _
               & "   and C.UORG         	               = A.SHIPORG 				        " _
               & "   and C.DELFLG                         <> '1' 						    " _
               & "  INNER JOIN MC006_TODOKESAKI D 						                    " _
               & "    ON D.CAMPCODE     	               = C.CAMPCODE 				    " _
               & "   and D.TODOKECODE   	               = C.TODOKECODE 				    " _
               & "   and D.STYMD                          <= A.SHUKODATE				    " _
               & "   and D.ENDYMD                         >= A.SHUKODATE				    " _
               & "   and D.DELFLG                         <> '1' 						    " _
               & "  LEFT JOIN MC003_TORIORG E 							                    " _
               & "    ON E.CAMPCODE     	               = A.CAMPCODE 				    " _
               & "   and E.TORICODE     	               = A.TORICODE 				    " _
               & "   and E.UORG         	               = A.SHIPORG 				        " _
               & "   and E.DELFLG                         <> '1' 						    " _
               & "  LEFT JOIN MC007_TODKORG F 							                    " _
               & "    ON F.CAMPCODE     	               = A.CAMPCODE 				    " _
               & "   and F.TORICODE     	               = A.TORICODE 				    " _
               & "   and F.TODOKECODE   	               = A.TODOKECODE 				    " _
               & "   and F.UORG         	               = A.SHIPORG 				        " _
               & "   and F.DELFLG                         <> '1' 						    " _
               & "  LEFT JOIN MC007_TODKORG G 							                    " _
               & "    ON G.CAMPCODE     	               = A.CAMPCODE 				    " _
               & "   and G.TORICODE     	               = A.TORICODE 				    " _
               & "   and G.TODOKECODE   	               = A.SHUKABASHO 				    " _
               & "   and G.UORG         	               = A.SHIPORG 				        " _
               & "   and G.DELFLG                         <> '1' 						    " _
               & "  LEFT JOIN MD002_PRODORG H 							                    " _
               & "    ON H.CAMPCODE     	               = A.CAMPCODE 				    " _
               & "   and H.UORG         	               = A.SHIPORG 				        " _
               & "   and H.PRODUCTCODE     	               = A.PRODUCTCODE 				    " _
               & "   and H.STYMD                          <= @P03       				    " _
               & "   and H.ENDYMD                         >= @P03 			        	    " _
               & "   and H.DELFLG                         <> '1' 						    " _
               & "  INNER JOIN M0001_CAMP M1 							                    " _
               & "    ON M1.CAMPCODE     	               = A.CAMPCODE 				    " _
               & "   and M1.STYMD                         <= A.SHUKODATE   				    " _
               & "   and M1.ENDYMD                        >= A.SHUKODATE 	        	    " _
               & "   and M1.DELFLG                        <> '1' 						    " _
               & "  INNER JOIN M0002_ORG M2_O 							                    " _
               & "    ON M2_O.CAMPCODE     	               = A.CAMPCODE 				    " _
               & "   and M2_O.ORGCODE                      = A.ORDERORG   				    " _
               & "   and M2_O.STYMD                       <= A.SHUKODATE   				    " _
               & "   and M2_O.ENDYMD                      >= A.SHUKODATE 	        	    " _
               & "   and M2_O.DELFLG                      <> '1' 						    " _
               & "  INNER JOIN M0002_ORG M2_S 							                    " _
               & "    ON M2_S.CAMPCODE     	               = A.CAMPCODE 				    " _
               & "   and M2_S.ORGCODE                      = A.SHIPORG   				    " _
               & "   and M2_S.STYMD                       <= A.SHUKODATE   				    " _
               & "   and M2_S.ENDYMD                      >= A.SHUKODATE 	        	    " _
               & "   and M2_S.DELFLG                      <> '1' 						    " _
               & "  INNER JOIN M0002_ORG M2_T 							                    " _
               & "    ON M2_T.CAMPCODE     	               = A.CAMPCODE 				    " _
               & "   and M2_T.ORGCODE                      = A.TERMORG   				    " _
               & "   and M2_T.STYMD                       <= A.SHUKODATE   				    " _
               & "   and M2_T.ENDYMD                      >= A.SHUKODATE 	        	    " _
               & "   and M2_T.DELFLG                      <> '1' 						    " _
               & "  INNER JOIN MC001_FIXVALUE MC1_1						                    " _
               & "    ON MC1_1.CAMPCODE     	           = A.CAMPCODE     		        " _
               & "   and MC1_1.CLASS                       = 'STATUS'   				    " _
               & "   and MC1_1.KEYCODE                     = A.STATUS  				        " _
               & "   and MC1_1.STYMD                      <= A.SHUKODATE   				    " _
               & "   and MC1_1.ENDYMD                     >= A.SHUKODATE 	        	    " _
               & "   and MC1_1.DELFLG                     <> '1' 						    " _
               & "  INNER JOIN MC001_FIXVALUE MC1_2						                    " _
               & "    ON MC1_2.CAMPCODE     	           = A.CAMPCODE                     " _
               & "   and MC1_2.CLASS                       = 'TUMIOKIKBN'   			    " _
               & "   and MC1_2.KEYCODE                     = A.TUMIOKIKBN  				    " _
               & "   and MC1_2.STYMD                      <= A.SHUKODATE   				    " _
               & "   and MC1_2.ENDYMD                     >= A.SHUKODATE 	        	    " _
               & "   and MC1_2.DELFLG                     <> '1' 						    " _
               & "  INNER JOIN MC001_FIXVALUE MC1_3						                    " _
               & "    ON MC1_3.CAMPCODE     	           = A.CAMPCODE                     " _
               & "   and MC1_3.CLASS                       = 'PRODUCT1'   				    " _
               & "   and MC1_3.KEYCODE                     = A.PRODUCT1  				    " _
               & "   and MC1_3.STYMD                      <= A.SHUKODATE   				    " _
               & "   and MC1_3.ENDYMD                     >= A.SHUKODATE 	        	    " _
               & "   and MC1_3.DELFLG                     <> '1' 						    " _
               & "  INNER JOIN MC001_FIXVALUE MC1_4						                    " _
               & "    ON MC1_4.CAMPCODE     	           = A.CAMPCODE                     " _
               & "   and MC1_4.CLASS                       = 'SMELLKBN'   				    " _
               & "   and MC1_4.KEYCODE                     = A.SMELLKBN  				    " _
               & "   and MC1_4.STYMD                      <= A.SHUKODATE   				    " _
               & "   and MC1_4.ENDYMD                     >= A.SHUKODATE 	        	    " _
               & "   and MC1_4.DELFLG                     <> '1' 						    " _
               & "  INNER JOIN MC001_FIXVALUE MC1_5						                    " _
               & "    ON MC1_5.CAMPCODE     	           = A.CAMPCODE                     " _
               & "   and MC1_5.CLASS                       = 'URIKBN'   				    " _
               & "   and MC1_5.KEYCODE                     = A.URIKBN  				        " _
               & "   and MC1_5.STYMD                      <= A.SHUKODATE   				    " _
               & "   and MC1_5.ENDYMD                     >= A.SHUKODATE 	        	    " _
               & "   and MC1_5.DELFLG                     <> '1' 						    " _
               & "  LEFT JOIN MA006_SHABANORG MA1                                           " _
               & "    ON MA1.CAMPCODE     	               = A.CAMPCODE 				    " _
               & "   and MA1.GSHABAN      	               = A.CONTCHASSIS 				    " _
               & "   and MA1.MANGUORG     	               = A.SHIPORG 				        " _
               & "   and MA1.DELFLG                       <> '1' 						    " _
               & "  LEFT JOIN MA004_SHARYOC MA4                  						    " _
               & "    ON MA4.CAMPCODE   	               = MA1.CAMPCODE 				    " _
               & "   and MA4.SHARYOTYPE                    = MA1.SHARYOTYPEF 		        " _
               & "   and MA4.TSHABAN                       = MA1.TSHABANF 	                " _
               & "   and MA4.STYMD                        <= A.SHUKODATE                    " _
               & "   and MA4.ENDYMD                       >= A.SHUKODATE                    " _
               & "   and MA4.DELFLG                       <> '1' 						    " _
               & " WHERE A.CAMPCODE     	               = @P01                           " _
               & "   and A.SHUKODATE                      <= @P04 		                    " _
               & "   and A.SHUKODATE                      >= @P05  		                    " _
               & "   and A.SHIPORG                         = @P06  		                    " _
               & "   and (A.STATUS                         = '2' 		                    " _
               & "    or  A.JXORDERID                     <> '') 		                    " _
               & "   and A.DELFLG                         <> '1'		                    " _
               & " ORDER BY A.TORICODE  ,A.OILTYPE  ,A.SHUKADATE ,A.ORDERORG  ,A.SHIPORG ,	" _
               & "          A.SHUKODATE ,A.GSHABAN  ,A.RYOME     ,                          " _
               & "          A.TRIPNO    ,A.DROPNO	,A.PRODUCTCODE                          "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.VarChar)  '会社
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.VarChar)  '部署権限
            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.Date)      'Now（権限）
            Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Date)      '出庫日FROM
            Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.Date)      '出庫日TO
            Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.VarChar)  '出荷部署
            PARA01.Value = work.WF_SEL_CAMPCODE.Text
            PARA02.Value = Master.ROLE_ORG
            PARA03.Value = Date.Now

            If String.IsNullOrEmpty(work.WF_SEL_SHUKODATET.Text) Then
                PARA04.Value = WW_DATE
            Else
                PARA04.Value = work.WF_SEL_SHUKODATET.Text
            End If

            If String.IsNullOrEmpty(work.WF_SEL_SHUKODATEF.Text) Then
                PARA05.Value = C_DEFAULT_YMD
            Else
                PARA05.Value = work.WF_SEL_SHUKODATEF.Text
            End If

            PARA06.Value = work.WF_SEL_SHIPORG.Text

            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            'フィールド名とフィールドの型を取得
            For index As Integer = 0 To SQLdr.FieldCount - 1
                T00006tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
            Next
            '■テーブル検索結果をテーブル格納
            T00006tbl.Load(SQLdr)

            'Close
            SQLdr.Close() 'Reader(Close)
            SQLdr = Nothing

            SQLcmd.Dispose()
            SQLcmd = Nothing

            SQLcon.Close() 'DataBase接続(Close)
            SQLcon.Dispose()
            SQLcon = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0004_HORDER SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0004_HORDER Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                 '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        'T00006tbl値設定
        Dim WW_DATA_CNT As Integer = -1

        For Each T00006row In T00006tbl.Rows

            '○データ設定

            '固定項目
            WW_DATA_CNT = WW_DATA_CNT + 1
            T00006row("WORK_NO") = WW_DATA_CNT.ToString
            T00006row("LINECNT") = 0
            T00006row("OPERATION") = "on"
            T00006row("SELECT") = 1   '1:表示
            T00006row("HIDDEN") = 0   '0:表示

            '画面毎の設定項目
            If Date.TryParse(T00006row("SHUKODATE"), WW_DATE) Then
                T00006row("SHUKODATE") = WW_DATE.ToString("yyyy/MM/dd")
            Else
                T00006row("SHUKODATE") = ""
            End If
            If Date.TryParse(T00006row("KIKODATE"), WW_DATE) Then
                T00006row("KIKODATE") = WW_DATE.ToString("yyyy/MM/dd")
            Else
                T00006row("KIKODATE") = ""
            End If
            If Date.TryParse(T00006row("SHUKADATE"), WW_DATE) Then
                T00006row("SHUKADATE") = WW_DATE.ToString("yyyy/MM/dd")
            Else
                T00006row("SHUKADATE") = ""
            End If
            If Date.TryParse(T00006row("TODOKEDATE"), WW_DATE) Then
                T00006row("TODOKEDATE") = WW_DATE.ToString("yyyy/MM/dd")
            Else
                T00006row("TODOKEDATE") = ""
            End If

            If Date.TryParse(T00006row("ARRIVTIME"), WW_TIME) Then
                T00006row("ARRIVTIME") = WW_TIME.ToString("H:mm")
            Else
                T00006row("ARRIVTIME") = ""
            End If

            '項目名称設定
            CODENAME_set(T00006row)

            If T00006row("TUMIOKIKBN") = "1" Then
                If T00006row("SHUKODATE") = T00006row("SHUKADATE") Then
                    T00006row("TUMIOKI") = "積置"
                Else
                    T00006row("TUMIOKI") = "積配"
                End If
            Else
                T00006row("TUMIOKI") = ""
            End If

            If T00006row("YAZKSHABAN") = "" Then
                T00006row("YAZKSHABAN") = "99999999"
            End If

        Next

    End Sub



    ''' <summary>
    ''' GridView表示設定
    ''' </summary>
    ''' <remarks>データベース（T00004）を検索し画面表示する一覧を作成する</remarks>
    Private Sub GRID_INITset()

        '○画面表示データ取得
        DBselect_T4SELECT()

        'サマリ処理
        SUMMRY_SET()

        '○ソート
        'ソート文字列取得
        CS0026TBLSORTget.COMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0026TBLSORTget.MAPID = Master.MAPID
        CS0026TBLSORTget.PROFID = Master.PROF_VIEW
        CS0026TBLSORTget.VARI = Master.MAPvariant
        CS0026TBLSORTget.TAB = ""
        CS0026TBLSORTget.TABLE = T00006tbl
        CS0026TBLSORTget.getSorting()
        CS0026TBLSORTget.sort()

        CS0026TBLSORTget.TABLE = T00006tbl
        CS0026TBLSORTget.SORTING = "TORICODE,OILTYPE,SHUKADATE,ORDERORG,SHIPORG,SHUKODATE,GSHABAN,RYOME,TRIPNO,DROPNO,PRODUCT1,PRODUCT2"
        CS0026TBLSORTget.FILTER = ""
        CS0026TBLSORTget.sort(T00006tbl)


        '項番をナンバーリング
        Dim WW_LineCNT As Long = 0
        For Each T00006row In T00006tbl.Rows

            If T00006row("HIDDEN") = "0" Then
                WW_LineCNT += 1
            End If

            T00006row("LINECNT") = WW_LineCNT
        Next

    End Sub

    ''' <summary>
    ''' GridViewサマリ処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub SUMMRY_SET()

        Dim SURYO_SUM As Decimal = 0
        Dim DAISU_SUM As Integer = 0
        Dim WW_SURYO As Decimal = 0
        Dim WW_DAISU As Integer = 0

        CS0026TBLSORTget.TABLE = T00006tbl
        CS0026TBLSORTget.SORTING = "TORICODE ,OILTYPE ,SHUKADATE ,ORDERORG ,SHIPORG ,SHUKODATE ,GSHABAN ,RYOME ,TRIPNO ,DROPNO ,SEQ"
        CS0026TBLSORTget.FILTER = ""
        CS0026TBLSORTget.sort(T00006tbl)

        '最終行から初回行へループ
        For i As Integer = 0 To T00006tbl.Rows.Count - 1

            Dim T00006row = T00006tbl.Rows(i)

            If T00006row("SEQ") = "01" And T00006row("DELFLG") <> "1" Then
                SURYO_SUM = 0
                DAISU_SUM = 0

                For j As Integer = i To T00006tbl.Rows.Count - 1
                    If OrderCompare(T00006row, T00006tbl.Rows(j)) AndAlso
                        T00006tbl.Rows(j)("DELFLG") <> C_DELETE_FLG.DELETE Then

                        If Decimal.TryParse(T00006tbl.Rows(j)("SURYO"), WW_SURYO) Then
                            SURYO_SUM += WW_SURYO
                        End If
                        If Int32.TryParse(T00006tbl.Rows(j)("DAISU"), WW_DAISU) Then
                            DAISU_SUM += WW_DAISU
                        End If
                    Else
                        Exit For
                    End If

                Next

                '表示行にサマリ結果を反映
                T00006row("SURYO_SUM") = SURYO_SUM.ToString("0.000")
                T00006row("DAISU_SUM") = DAISU_SUM.ToString("0")
                T00006row("HIDDEN") = 0   '0:表示

            Else
                T00006row("HIDDEN") = 1   '1:非表示
            End If

        Next

    End Sub

    ''' <summary>
    ''' LeftBox項目名称設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub CODENAME_set(ByRef T00006row As DataRow)

        '乗務員名称
        CODENAME_get("STAFFCODE", T00006row("STAFFCODE"), T00006row("STAFFCODENAME"), WW_DUMMY)

        '副乗務員名称
        CODENAME_get("SUBSTAFFCODE", T00006row("SUBSTAFFCODE"), T00006row("SUBSTAFFCODENAME"), WW_DUMMY)

        '業務車番ナンバー
        CODENAME_get("GSHABAN", T00006row("GSHABAN"), T00006row("GSHABANLICNPLTNO"), WW_DUMMY)

        '取引先名称
        CODENAME_get("TORICODE", T00006row("TORICODE"), T00006row("TORICODENAME"), WW_DUMMY)

        '出荷場所名称
        CODENAME_get("SHUKABASHO", T00006row("SHUKABASHO"), T00006row("SHUKABASHONAME"), WW_DUMMY)

        '届先名称
        CODENAME_get("TODOKECODE", T00006row("TODOKECODE"), T00006row("TODOKECODENAME"), WW_DUMMY)

        '油種名称
        CODENAME_get("OILTYPE", T00006row("OILTYPE"), T00006row("OILTYPENAME"), WW_DUMMY)

        '品名名称
        CODENAME_get("PRODUCTCODE", T00006row("PRODUCTCODE"), T00006row("PRODUCTNAME"), WW_DUMMY)
        T00006row("PRODUCT2NAME") = T00006row("PRODUCTNAME")
    End Sub

    ''' <summary>
    ''' 同一オーダー判定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Function OrderCompare(ByRef src As DataRow, ByRef dst As DataRow) As Boolean

        '取引先、油種、基準日（出荷日or届日）、受注部署、出荷部署、出庫日、業務車番、両目、トリップ、ドロップが同一
        If src("TORICODE") = dst("TORICODE") AndAlso
           src("OILTYPE") = dst("OILTYPE") AndAlso
           src("SHUKADATE") = dst("SHUKADATE") AndAlso
           src("ORDERORG") = dst("ORDERORG") AndAlso
           src("SHIPORG") = dst("SHIPORG") AndAlso
           src("SHUKODATE") = dst("SHUKODATE") AndAlso
           src("GSHABAN") = dst("GSHABAN") AndAlso
           src("RYOME") = dst("RYOME") AndAlso
           src("TRIPNO") = dst("TRIPNO") AndAlso
           src("DROPNO") = dst("DROPNO") Then

            Return True
        Else
            Return False
        End If

    End Function
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
        Master.RecoverTable(T00006tbl)

        'この段階でありえないがデータテーブルがない場合は終了
        If T00006tbl Is Nothing OrElse T00006tbl.Rows.Count = 0 Then
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
                For Each row In T00006tbl.Rows
                    If row("LINECNT") = i Then
                        row(fieldId.Key) = displayValue
                    End If
                Next
            Next
        Next

        '○ 画面表示データ保存
        Master.SaveTable(T00006tbl)

        Return
    End Sub

End Class





