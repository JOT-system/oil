Option Strict On
Imports Microsoft.Office.Interop
Imports System.Runtime.InteropServices
''' <summary>
''' メニュー画面の月間列車別牽引実績のダウンロード機能
''' </summary>
Public Class M00001MP0009ActualTraction : Implements IDisposable
    ''' <summary>
    ''' エクセルアプリケーションオブジェクト
    ''' </summary>
    Private ExcelAppObj As Excel.Application
    ''' <summary>
    ''' エクセルブックコレクション
    ''' </summary>
    Private ExcelBooksObj As Excel.Workbooks
    ''' <summary>
    ''' エクセルブックオブジェクト
    ''' </summary>
    Private ExcelBookObj As Excel.Workbook
    ''' <summary>
    ''' エクセルシートコレクション
    ''' </summary>
    Private ExcelWorkSheets As Excel.Sheets
    ''' <summary>
    ''' エクセルシートオブジェクト
    ''' </summary>
    Private ExcelWorkSheet As Excel.Worksheet


    ''' <summary>
    ''' 画面展開している内容と同等のデータテーブル
    ''' </summary>
    Private DispData As DataTable
    Private OfficeCode As String = ""
    Private OfficeName As String = ""
    Private ArrStationCode As String = ""
    Private ArrStationName As String = ""
    Private YearMonth As String = ""


    ''' <summary>
    ''' ExcelプロセスID
    ''' </summary>
    Private xlProcId As Integer
    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Private ExcelTemplatePath As String = ""
    Private UploadRootPath As String = ""
    Private UrlRoot As String = ""
    Private PrintFilePath As String = ""
    ''' <summary>
    ''' WindowハンドルよりProcessIDを取得
    ''' </summary>
    ''' <param name="hwnd"></param>
    ''' <param name="lpdwProcessId"></param>
    ''' <returns></returns>
    ''' <remarks>ExcelのWindowハンドルを探しプロセスIDを取得
    ''' 当処理で使用したExcelのプロセスIDが残っていた場合KILLする為使用</remarks>
    Private Declare Auto Function GetWindowThreadProcessId Lib "user32.dll" (ByVal hwnd As IntPtr,
              ByRef lpdwProcessId As Integer) As Integer

    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    ''' <param name="mapId"></param>
    ''' <param name="excelFileName"></param>
    ''' <param name="dispData"></param>
    Public Sub New(mapId As String, excelFileName As String, dispData As DataTable,
                   officeCode As String, officeName As String, arrStationCode As String, arrStationName As String, yearMonth As String)
        Dim CS0050SESSION As New CS0050SESSION
        Me.DispData = dispData
        Me.OfficeCode = officeCode
        Me.OfficeName = officeName
        Me.ArrStationCode = arrStationCode
        Me.ArrStationName = arrStationName
        Me.YearMonth = yearMonth
        Me.ExcelTemplatePath = System.IO.Path.Combine(CS0050SESSION.UPLOAD_PATH,
                                              "PRINTFORMAT",
                                              C_DEFAULT_DATAKEY,
                                              mapId, excelFileName)
        Me.UploadRootPath = System.IO.Path.Combine(CS0050SESSION.UPLOAD_PATH,
                                           "PRINTWORK",
                                           CS0050SESSION.USERID)
        'ディレクトリが存在しない場合は生成
        If IO.Directory.Exists(Me.UploadRootPath) = False Then
            IO.Directory.CreateDirectory(Me.UploadRootPath)
        End If
        '前日プリフィックスのアップロードファイルが残っていた場合は削除
        Dim targetFiles = IO.Directory.GetFiles(Me.UploadRootPath, "*.*")
        Dim keepFilePrefix As String = Now.ToString("yyyyMMdd")
        For Each targetFile In targetFiles
            Dim fileName As String = IO.Path.GetFileName(targetFile)
            '今日の日付が先頭のファイル名の場合は残す
            If fileName.StartsWith(keepFilePrefix) Then
                Continue For
            End If
            Try
                IO.File.Delete(targetFile)
            Catch ex As Exception
                '削除時のエラーは無視
            End Try
        Next targetFile
        'URLのルートを表示
        Me.UrlRoot = String.Format("{0}://{1}/{3}/{2}/", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Host, CS0050SESSION.USERID, CS0050SESSION.PRINT_ROOT_URL_NAME)
        'Excelアプリケーションオブジェクトの生成
        Me.ExcelAppObj = New Excel.Application
        ExcelAppObj.DisplayAlerts = False
        ExcelAppObj.ScreenUpdating = False

        Dim xlHwnd As IntPtr = CType(Me.ExcelAppObj.Hwnd, IntPtr)
        GetWindowThreadProcessId(xlHwnd, Me.xlProcId)
        'Excelワークブックオブジェクトの生成
        Me.ExcelBooksObj = Me.ExcelAppObj.Workbooks
        Me.ExcelBookObj = Me.ExcelBooksObj.Open(Me.ExcelTemplatePath,
                                                UpdateLinks:=Excel.XlUpdateLinks.xlUpdateLinksNever,
                                                [ReadOnly]:=Excel.XlFileAccess.xlReadOnly)
        ExcelAppObj.Calculation = Excel.XlCalculation.xlCalculationManual
        Me.ExcelWorkSheets = Me.ExcelBookObj.Sheets
        Me.ExcelWorkSheet = DirectCast(Me.ExcelWorkSheets("発送車数表"), Excel.Worksheet)

    End Sub

    ''' <summary>
    ''' ExcelデータをPrintフォルダに格納しURLを作成
    ''' </summary>
    ''' <returns></returns>
    Public Function CreateExcelPrintData() As String
        Dim rngWrite As Excel.Range = Nothing
        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)

        Try
            'シート設定
            InitSheet()
            'タイトル文言の設定
            CreateHeader()
            'データ部生成
            Select Case Me.OfficeCode
                Case BaseDllConst.CONST_OFFICECODE_010402
                    CreateView010402()
                Case BaseDllConst.CONST_OFFICECODE_011201
                    CreateView011201()
                Case BaseDllConst.CONST_OFFICECODE_011202
                    CreateView011202()
                Case BaseDllConst.CONST_OFFICECODE_011203
                    CreateView011203()
                Case BaseDllConst.CONST_OFFICECODE_011402
                    CreateView011402()
                Case BaseDllConst.CONST_OFFICECODE_012401
                    CreateView012401()
                Case BaseDllConst.CONST_OFFICECODE_012402
                    CreateView012402()
            End Select
            '保存処理実行
            ExcelAppObj.ScreenUpdating = True
            ExcelAppObj.Calculation = Excel.XlCalculation.xlCalculationAutomatic
            ExcelAppObj.Calculate()
            Dim saveExcelLock As New Object
            SyncLock saveExcelLock '複数Excel起動で同時セーブすると落ちるので抑止
                Me.ExcelBookObj.SaveAs(tmpFilePath, Excel.XlFileFormat.xlOpenXMLWorkbook)
            End SyncLock
            Me.ExcelBookObj.Close(False)
            Me.PrintFilePath = tmpFilePath
            Return UrlRoot & tmpFileName
        Catch ex As Exception
            Throw
        End Try

    End Function

    ''' <summary>
    ''' 作成＋ダウンロード処理実行
    ''' </summary>
    ''' <param name="currentPage"></param>
    Public Sub CreateExcelFileStream(currentPage As Page, Optional dlFileName As String = "")
        Dim url = CreateExcelPrintData()
        If Me.PrintFilePath = "" OrElse
            IO.File.Exists(Me.PrintFilePath) = False Then
            Return
        End If

        Dim fileName As String = IO.Path.GetFileName(Me.PrintFilePath)
        If dlFileName <> "" Then
            fileName = dlFileName
        End If

        Dim fi = New IO.FileInfo(Me.PrintFilePath)
        Dim encodeFileName As String = HttpUtility.UrlEncode(fileName)
        encodeFileName = encodeFileName.Replace("+", "%20")
        With currentPage
            .Response.ContentType = "application/octet-stream"
            .Response.AddHeader("Content-Disposition", String.Format("attachment;filename*=utf-8''{0}", encodeFileName))
            .Response.AddHeader("Content-Length", fi.Length.ToString())
            .Response.AddHeader("Pragma", "no-cache")
            .Response.AddHeader("Cache-Control", "no-cache")
            .Response.WriteFile(Me.PrintFilePath)
            .Response.End()
        End With

    End Sub

    Private Sub InitSheet()
        Dim rngWork As Excel.Range = Nothing
        Dim rngOffsetWork As Excel.Range = Nothing
        Dim rngColumnsWork As Excel.Range = Nothing

        Try

            Dim strDt As String = String.Format("{0}/01", Me.YearMonth)
            Dim dt As Date = Nothing
            If Date.TryParse(strDt, dt) Then

                '基本位置を取得(前月最終日列)
                rngWork = Me.ExcelWorkSheet.Range("D:D")

                '不要な列を削除
                Dim lastDay As Integer = dt.AddMonths(1).AddDays(-1).Day
                For day As Integer = 31 To lastDay + 1 Step -1
                    rngOffsetWork = rngWork.Offset(ColumnOffset:=day)
                    rngOffsetWork.Delete()
                    ExcelMemoryRelease(rngOffsetWork)
                Next

                '翌月初日計算式補正 & 非表示
                rngOffsetWork = rngWork.Offset(ColumnOffset:=lastDay + 1)
                rngOffsetWork.Item(RowIndex:=5) = "=INDIRECT(ADDRESS(ROW(), COLUMN() - 1)) + 1"
                rngColumnsWork = rngOffsetWork.Columns
                ExcelMemoryRelease(rngOffsetWork)

                rngColumnsWork.Hidden = True
                ExcelMemoryRelease(rngColumnsWork)

                '前月最終日非表示
                rngColumnsWork = rngWork.Columns
                ExcelMemoryRelease(rngWork)

                rngColumnsWork.Hidden = True
                ExcelMemoryRelease(rngColumnsWork)

            End If

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngWork)
            ExcelMemoryRelease(rngOffsetWork)
            ExcelMemoryRelease(rngColumnsWork)
        End Try
    End Sub

    ''' <summary>
    ''' ヘッダー文言の設定
    ''' </summary>
    Private Sub CreateHeader()
        Dim rngWork As Excel.Range = Nothing

        Try

            Dim strDt As String = String.Format("{0}/01", Me.YearMonth)
            Dim dt As Date = Nothing
            If Date.TryParse(strDt, dt) Then
                rngWork = Me.ExcelWorkSheet.Range("A2")
                rngWork.Value = String.Format("{0:yyyy年M月}", dt)
                ExcelMemoryRelease(rngWork)
            End If

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngWork)
        End Try
    End Sub

    Private Sub SetRowValues(ByVal rngStr As String, ByVal dt As Date, ByVal setData As Dictionary(Of String, Integer))
        Dim rngWork As Excel.Range = Nothing
        Dim rngOffsetWork As Excel.Range = Nothing
        Dim columnOffset As Integer = 0
        Dim strDate As String = ""

        Try
            '基本位置を取得
            rngWork = Me.ExcelWorkSheet.Range(rngStr)
            '最終日取得
            Dim lastDay As Integer = dt.AddMonths(1).AddDays(-1).Day

            '前月末日
            columnOffset = 1
            strDate = dt.AddDays(-1).ToString("yyyy/MM/dd")
            If setData.ContainsKey(strDate) Then
                rngOffsetWork = rngWork.Offset(ColumnOffset:=columnOffset)
                rngOffsetWork.Value = setData(strDate)
                ExcelMemoryRelease(rngOffsetWork)
            End If

            '当月
            For day As Integer = 0 To lastDay - 1
                columnOffset += 1
                strDate = dt.AddDays(day).ToString("yyyy/MM/dd")
                If setData.ContainsKey(strDate) Then
                    rngOffsetWork = rngWork.Offset(ColumnOffset:=columnOffset)
                    rngOffsetWork.Value = setData(strDate)
                    ExcelMemoryRelease(rngOffsetWork)
                End If
            Next

            '翌月初日
            columnOffset += 1
            strDate = dt.AddDays(lastDay).ToString("yyyy/MM/dd")
            If setData.ContainsKey(strDate) Then
                rngOffsetWork = rngWork.Offset(ColumnOffset:=columnOffset)
                rngOffsetWork.Value = setData(strDate)
                ExcelMemoryRelease(rngOffsetWork)
            End If

        Catch ex As Exception
            ExcelMemoryRelease(rngWork)
            ExcelMemoryRelease(rngOffsetWork)
        Finally
            ExcelMemoryRelease(rngWork)
            ExcelMemoryRelease(rngOffsetWork)
        End Try
    End Sub

    ''' <summary>
    ''' 仙台
    ''' </summary>
    Private Sub CreateView010402()

        Try
            Dim strDt As String = String.Format("{0}/01", Me.YearMonth)
            Dim dt As Date = Now
            Date.TryParse(strDt, dt)

            Dim allData = Me.DispData.AsEnumerable().
                Select(Function(r)
                           Return New With {
                                .OFFICECODE = r("OFFICECODE").ToString(),
                                .SHIPPERCODE = r("SHIPPERCODE").ToString(),
                                .ARRSTATIONCODE = r("ARRSTATIONCODE").ToString(),
                                .TRAINNO = r("TRAINNO").ToString(),
                                .LODDATE = r("LODDATE").ToString(),
                                .DEPDATE = r("DEPDATE").ToString(),
                                .CARSNUMBER = CInt(r("CARSNUMBER")),
                                .LINE = CInt(r("LINE"))
                           }
                       End Function)

            '請負-ENEOS-盛岡-8081
            Dim setData = allData.Where(Function(r)
                                            Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                            r.ARRSTATIONCODE = "2018" AndAlso
                                            r.TRAINNO = "8081"
                                        End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C7", dt, setData)

            '請負-ENEOS-盛岡-5081
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                        r.ARRSTATIONCODE = "2018" AndAlso
                                        r.TRAINNO = "5081"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C8", dt, setData)

            '請負-ENEOS-盛岡-5575
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                        r.ARRSTATIONCODE = "2018" AndAlso
                                        r.TRAINNO = "5575"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C9", dt, setData)

            '請負-ENEOS-郡山-5090
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                        r.ARRSTATIONCODE = "2407" AndAlso
                                        r.TRAINNO = "5090"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C11", dt, setData)

            'OT-コスモ 出光-盛岡-8081
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 OrElse
                                        r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0122700010 AndAlso
                                        r.ARRSTATIONCODE = "2018" AndAlso
                                        r.TRAINNO = "8081"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C13", dt, setData)

            'OT-コスモ 出光-盛岡-5081
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 OrElse
                                        r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0122700010 AndAlso
                                        r.ARRSTATIONCODE = "2018" AndAlso
                                        r.TRAINNO = "5081"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C14", dt, setData)

            'OT-コスモ 出光-盛岡-5575
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 OrElse
                                        r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0122700010 AndAlso
                                        r.ARRSTATIONCODE = "2018" AndAlso
                                        r.TRAINNO = "5575"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C15", dt, setData)

            '積込回数（手入力）

            '当日積込車数
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 OrElse
                                        r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 OrElse
                                        r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0122700010
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C19", dt, setData)

        Catch ex As Exception
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' 五井
    ''' </summary>
    Private Sub CreateView011201()

        Try
            Dim strDt As String = String.Format("{0}/01", Me.YearMonth)
            Dim dt As Date = Now
            Date.TryParse(strDt, dt)

            Dim allData = Me.DispData.AsEnumerable().
                Select(Function(r)
                           Return New With {
                                .OFFICECODE = r("OFFICECODE").ToString(),
                                .OTTRANSPORTFLG = r("OTTRANSPORTFLG").ToString(),
                                .SHIPPERCODE = r("SHIPPERCODE").ToString(),
                                .ARRSTATIONCODE = r("ARRSTATIONCODE").ToString(),
                                .TRAINNO = r("TRAINNO").ToString(),
                                .LODDATE = r("LODDATE").ToString(),
                                .DEPDATE = r("DEPDATE").ToString(),
                                .CARSNUMBER = CInt(r("CARSNUMBER")),
                                .LINE = CInt(r("LINE"))
                           }
                       End Function)

            '請負-コスモ-倉賀野-8877
            Dim setData = allData.Where(Function(r)
                                            Return r.OTTRANSPORTFLG = "2" AndAlso
                                            r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 AndAlso
                                            r.ARRSTATIONCODE = "4113" AndAlso
                                            r.TRAINNO = "8877"
                                        End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C7", dt, setData)

            '請負-コスモ-倉賀野-8883
            setData = allData.Where(Function(r)
                                        Return r.OTTRANSPORTFLG = "2" AndAlso
                                        r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 AndAlso
                                        r.ARRSTATIONCODE = "4113" AndAlso
                                        r.TRAINNO = "8883"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C8", dt, setData)

            '請負-コスモ-倉賀野-5972
            setData = allData.Where(Function(r)
                                        Return r.OTTRANSPORTFLG = "2" AndAlso
                                        r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 AndAlso
                                        r.ARRSTATIONCODE = "4113" AndAlso
                                        r.TRAINNO = "5972"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C9", dt, setData)

            'OT-コスモ-郡山-1071
            setData = allData.Where(Function(r)
                                        Return r.OTTRANSPORTFLG = "1" AndAlso
                                        r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 AndAlso
                                        r.ARRSTATIONCODE = "2407" AndAlso
                                        r.TRAINNO = "1071"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C12", dt, setData)

            'OT-コスモ-郡山-8179
            setData = allData.Where(Function(r)
                                        Return r.OTTRANSPORTFLG = "1" AndAlso
                                        r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 AndAlso
                                        r.ARRSTATIONCODE = "2407" AndAlso
                                        r.TRAINNO = "8179"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C13", dt, setData)

            'OT-コスモ-宇都宮-8681
            setData = allData.Where(Function(r)
                                        Return r.OTTRANSPORTFLG = "1" AndAlso
                                        r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 AndAlso
                                        r.ARRSTATIONCODE = "4425" AndAlso
                                        r.TRAINNO = "8681"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C15", dt, setData)

            'OT-コスモ-宇都宮-8685
            setData = allData.Where(Function(r)
                                        Return r.OTTRANSPORTFLG = "1" AndAlso
                                        r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 AndAlso
                                        r.ARRSTATIONCODE = "4425" AndAlso
                                        r.TRAINNO = "8685"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C16", dt, setData)

            'OT-コスモ-宇都宮-9175
            setData = allData.Where(Function(r)
                                        Return r.OTTRANSPORTFLG = "1" AndAlso
                                        r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 AndAlso
                                        r.ARRSTATIONCODE = "4425" AndAlso
                                        r.TRAINNO = "9175"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C17", dt, setData)

            'OT-コスモ-倉賀野-8883
            setData = allData.Where(Function(r)
                                        Return r.OTTRANSPORTFLG = "1" AndAlso
                                        r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 AndAlso
                                        r.ARRSTATIONCODE = "4113" AndAlso
                                        r.TRAINNO = "8883"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C19", dt, setData)

            'OT-コスモ-倉賀野-8877
            setData = allData.Where(Function(r)
                                        Return r.OTTRANSPORTFLG = "1" AndAlso
                                        r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 AndAlso
                                        r.ARRSTATIONCODE = "4113" AndAlso
                                        r.TRAINNO = "8877"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C20", dt, setData)

            'OT-コスモ-倉賀野-8763
            setData = allData.Where(Function(r)
                                        Return r.OTTRANSPORTFLG = "1" AndAlso
                                        r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 AndAlso
                                        r.ARRSTATIONCODE = "4113" AndAlso
                                        r.TRAINNO = "8763"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C21", dt, setData)

            'OT-コスモ-八王子-2461
            setData = allData.Where(Function(r)
                                        Return r.OTTRANSPORTFLG = "1" AndAlso
                                        r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 AndAlso
                                        r.ARRSTATIONCODE = "4610" AndAlso
                                        r.TRAINNO = "2461"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C23", dt, setData)

            'OT-コスモ-南松本-2081
            setData = allData.Where(Function(r)
                                        Return r.OTTRANSPORTFLG = "1" AndAlso
                                        r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 AndAlso
                                        r.ARRSTATIONCODE = "5141" AndAlso
                                        r.TRAINNO = "2081"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C24", dt, setData)

            'OT-コスモ-南松本-5972
            setData = allData.Where(Function(r)
                                        Return r.OTTRANSPORTFLG = "1" AndAlso
                                        r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 AndAlso
                                        r.ARRSTATIONCODE = "5141" AndAlso
                                        r.TRAINNO = "5972"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C25", dt, setData)

            'OT-コスモ-南松本-9672
            setData = allData.Where(Function(r)
                                        Return r.OTTRANSPORTFLG = "1" AndAlso
                                        r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 AndAlso
                                        r.ARRSTATIONCODE = "5141" AndAlso
                                        r.TRAINNO = "9672"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C26", dt, setData)

            '積込回数（その日発送する列車の受注明細の回線の最大値）
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.LINE).Max())
            SetRowValues("C30", dt, setData)

            '当日積込車数
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C31", dt, setData)

        Catch ex As Exception
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' 甲子
    ''' </summary>
    Private Sub CreateView011202()

        Try
            Dim strDt As String = String.Format("{0}/01", Me.YearMonth)
            Dim dt As Date = Now
            Date.TryParse(strDt, dt)

            Dim allData = Me.DispData.AsEnumerable().
                Select(Function(r)
                           Return New With {
                                .OFFICECODE = r("OFFICECODE").ToString(),
                                .SHIPPERCODE = r("SHIPPERCODE").ToString(),
                                .ARRSTATIONCODE = r("ARRSTATIONCODE").ToString(),
                                .TRAINNO = r("TRAINNO").ToString(),
                                .LODDATE = r("LODDATE").ToString(),
                                .DEPDATE = r("DEPDATE").ToString(),
                                .CARSNUMBER = CInt(r("CARSNUMBER")),
                                .LINE = CInt(r("LINE"))
                           }
                       End Function)

            'OT-ENEOS-宇都宮-8685
            Dim setData = allData.Where(Function(r)
                                            Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                            r.ARRSTATIONCODE = "4425" AndAlso
                                            r.TRAINNO = "8685"
                                        End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C7", dt, setData)

            'OT-ENEOS-宇都宮-2685
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                        r.ARRSTATIONCODE = "4425" AndAlso
                                        r.TRAINNO = "2685"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C8", dt, setData)

            '積込回数（その日発送する列車の受注明細の回線の最大値）
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.LINE).Max())
            SetRowValues("C12", dt, setData)

            '当日積込車数
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C13", dt, setData)

        Catch ex As Exception
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' 袖ヶ浦
    ''' </summary>
    Private Sub CreateView011203()

        Try
            Dim strDt As String = String.Format("{0}/01", Me.YearMonth)
            Dim dt As Date = Now
            Date.TryParse(strDt, dt)

            Dim allData = Me.DispData.AsEnumerable().
                Select(Function(r)
                           Return New With {
                                .OFFICECODE = r("OFFICECODE").ToString(),
                                .SHIPPERCODE = r("SHIPPERCODE").ToString(),
                                .ARRSTATIONCODE = r("ARRSTATIONCODE").ToString(),
                                .TRAINNO = r("TRAINNO").ToString(),
                                .LODDATE = r("LODDATE").ToString(),
                                .DEPDATE = r("DEPDATE").ToString(),
                                .CARSNUMBER = CInt(r("CARSNUMBER")),
                                .LINE = CInt(r("LINE"))
                           }
                       End Function)

            '請負-出光-倉賀野-8877
            Dim setData = allData.Where(Function(r)
                                            Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0122700010 AndAlso
                                            r.ARRSTATIONCODE = "4113" AndAlso
                                            r.TRAINNO = "8877"
                                        End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C7", dt, setData)

            '請負-出光-倉賀野-8883
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0122700010 AndAlso
                                        r.ARRSTATIONCODE = "4113" AndAlso
                                        r.TRAINNO = "8883"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C8", dt, setData)

            '請負-出光-南松本-5461
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0122700010 AndAlso
                                        r.ARRSTATIONCODE = "5141" AndAlso
                                        r.TRAINNO = "5461"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C10", dt, setData)

            '請負-出光-南松本-9672
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0122700010 AndAlso
                                        r.ARRSTATIONCODE = "5141" AndAlso
                                        r.TRAINNO = "9672"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C11", dt, setData)

            '積込回数（その日発送する列車の受注明細の回線の最大値）
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0122700010
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.LINE).Max())
            SetRowValues("C15", dt, setData)

            '当日積込車数
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0122700010
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C16", dt, setData)

        Catch ex As Exception
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' 根岸
    ''' </summary>
    Private Sub CreateView011402()

        Try
            Dim strDt As String = String.Format("{0}/01", Me.YearMonth)
            Dim dt As Date = Now
            Date.TryParse(strDt, dt)

            Dim allData = Me.DispData.AsEnumerable().
                Select(Function(r)
                           Return New With {
                                .OFFICECODE = r("OFFICECODE").ToString(),
                                .SHIPPERCODE = r("SHIPPERCODE").ToString(),
                                .ARRSTATIONCODE = r("ARRSTATIONCODE").ToString(),
                                .TRAINNO = r("TRAINNO").ToString(),
                                .LODDATE = r("LODDATE").ToString(),
                                .DEPDATE = r("DEPDATE").ToString(),
                                .CARSNUMBER = CInt(r("CARSNUMBER"))
                           }
                       End Function)

            '請負-ENEOS-宇都宮-4091
            Dim setData = allData.Where(Function(r)
                                            Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                            r.ARRSTATIONCODE = "4425" AndAlso
                                            r.TRAINNO = "4091"
                                        End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C7", dt, setData)

            '請負-ENEOS-宇都宮-8571
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                        r.ARRSTATIONCODE = "4425" AndAlso
                                        r.TRAINNO = "8571"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C8", dt, setData)

            '請負-ENEOS-宇都宮-8569
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                        r.ARRSTATIONCODE = "4425" AndAlso
                                        r.TRAINNO = "8569"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C9", dt, setData)

            '請負-ENEOS-倉賀野-3091
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                        r.ARRSTATIONCODE = "4113" AndAlso
                                        r.TRAINNO = "3091"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C11", dt, setData)

            '請負-ENEOS-倉賀野-3093
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                        r.ARRSTATIONCODE = "4113" AndAlso
                                        r.TRAINNO = "3093"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C12", dt, setData)

            '請負-ENEOS-倉賀野-8777
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                        r.ARRSTATIONCODE = "4113" AndAlso
                                        r.TRAINNO = "8777"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C13", dt, setData)

            '請負-ENEOS-八王子-85
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                        r.ARRSTATIONCODE = "4610" AndAlso
                                        r.TRAINNO = "85"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C15", dt, setData)

            '請負-ENEOS-八王子-87
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                        r.ARRSTATIONCODE = "4610" AndAlso
                                        r.TRAINNO = "87"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C16", dt, setData)

            '請負-ENEOS-八王子-5692
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                        r.ARRSTATIONCODE = "4610" AndAlso
                                        r.TRAINNO = "5692"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C17", dt, setData)

            '請負-ENEOS-八王子-8097
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                        r.ARRSTATIONCODE = "4610" AndAlso
                                        r.TRAINNO = "8097"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C18", dt, setData)

            '請負-ENEOS-竜王-5575
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                        r.ARRSTATIONCODE = "4620" AndAlso
                                        r.TRAINNO = "81"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C20", dt, setData)

            '請負-ENEOS-竜王-5575
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                        r.ARRSTATIONCODE = "4620" AndAlso
                                        r.TRAINNO = "83"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C21", dt, setData)

            '請負-ENEOS-坂城-2085
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                        r.ARRSTATIONCODE = "5009" AndAlso
                                        r.TRAINNO = "2085"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C23", dt, setData)

            '請負-ENEOS-坂城-5463
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                        r.ARRSTATIONCODE = "5009" AndAlso
                                        r.TRAINNO = "5463"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C24", dt, setData)

            '請負-ENEOS-坂城-8471
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                        r.ARRSTATIONCODE = "5009" AndAlso
                                        r.TRAINNO = "8471"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C25", dt, setData)

            '積込回数（手入力）

            '当日積込車数
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C30", dt, setData)

        Catch ex As Exception
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' 四日市
    ''' </summary>
    Private Sub CreateView012401()

        Try
            Dim strDt As String = String.Format("{0}/01", Me.YearMonth)
            Dim dt As Date = Now
            Date.TryParse(strDt, dt)

            Dim allData = Me.DispData.AsEnumerable().
                Select(Function(r)
                           Return New With {
                                .OFFICECODE = r("OFFICECODE").ToString(),
                                .SHIPPERCODE = r("SHIPPERCODE").ToString(),
                                .ARRSTATIONCODE = r("ARRSTATIONCODE").ToString(),
                                .TRAINNO = r("TRAINNO").ToString(),
                                .LODDATE = r("LODDATE").ToString(),
                                .DEPDATE = r("DEPDATE").ToString(),
                                .CARSNUMBER = CInt(r("CARSNUMBER"))
                           }
                       End Function)

            '請負-コスモ-南松本-6078
            Dim setData = allData.Where(Function(r)
                                            Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 AndAlso
                                            r.ARRSTATIONCODE = "5141" AndAlso
                                            r.TRAINNO = "6078"
                                        End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C7", dt, setData)

            '請負-コスモ-南松本-8380
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 AndAlso
                                        r.ARRSTATIONCODE = "5141" AndAlso
                                        r.TRAINNO = "8380"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C8", dt, setData)

            '積込回数（手入力）

            '当日積込車数
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C13", dt, setData)

        Catch ex As Exception
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' 三重塩浜
    ''' </summary>
    Private Sub CreateView012402()

        Try
            Dim strDt As String = String.Format("{0}/01", Me.YearMonth)
            Dim dt As Date = Now
            Date.TryParse(strDt, dt)

            Dim allData = Me.DispData.AsEnumerable().
                Select(Function(r)
                           Return New With {
                                .OFFICECODE = r("OFFICECODE").ToString(),
                                .SHIPPERCODE = r("SHIPPERCODE").ToString(),
                                .ARRSTATIONCODE = r("ARRSTATIONCODE").ToString(),
                                .TRAINNO = r("TRAINNO").ToString(),
                                .LODDATE = r("LODDATE").ToString(),
                                .DEPDATE = r("DEPDATE").ToString(),
                                .CARSNUMBER = CInt(r("CARSNUMBER"))
                           }
                       End Function)

            '請負-出光-南松本-5282
            Dim setData = allData.Where(Function(r)
                                            Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0122700010 AndAlso
                                            r.ARRSTATIONCODE = "5141" AndAlso
                                            r.TRAINNO = "5282"
                                        End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C7", dt, setData)

            '請負-出光-南松本-8072
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0122700010 AndAlso
                                        r.ARRSTATIONCODE = "5141" AndAlso
                                        r.TRAINNO = "8072"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C8", dt, setData)

            ''請負-出光-南松本-174
            'setData = allData.Where(Function(r)
            '                            Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0122700010 AndAlso
            '                            r.ARRSTATIONCODE = "5141" AndAlso
            '                            r.TRAINNO = "174"
            '                        End Function).
            '    GroupBy(Function(r) New With {Key r.LODDATE}).
            '    ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            'SetRowValues("C9", dt, setData)

            '積込回数（手入力）

            '当日積込車数
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0122700010
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C13", dt, setData)

        Catch ex As Exception
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Excelオブジェクトの解放
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="objCom"></param>
    Private Sub ExcelMemoryRelease(Of T As Class)(ByRef objCom As T)

        'ランタイム実行対象がComObjectのアンマネージコードの場合、メモリ開放
        If objCom Is Nothing Then
            Return
        Else
            Try
                If Marshal.IsComObject(objCom) Then
                    Dim count As Integer = Marshal.FinalReleaseComObject(objCom)
                End If
            Finally
                objCom = Nothing
            End Try
        End If

    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' 重複する呼び出しを検出するには

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: マネージド状態を破棄します (マネージド オブジェクト)。
            End If

            ' TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、下の Finalize() をオーバーライドします。
            ' TODO: 大きなフィールドを null に設定します。
        End If

        'Excel Sheetオブジェクトの解放
        ExcelMemoryRelease(ExcelWorkSheet)
        'Excel Sheetコレクションの解放
        ExcelMemoryRelease(ExcelWorkSheets)
        'Excel Bookオブジェクトを閉じる
        If ExcelBookObj IsNot Nothing Then
            Try
                'ExcelBookObj.Close(Excel.XlSaveAction.xlDoNotSaveChanges)
                ExcelBookObj.Close(False)
            Catch ex As Exception
            End Try
        End If

        ExcelMemoryRelease(ExcelBookObj)
        'Excel Bookコレクションの解放
        ExcelMemoryRelease(ExcelBooksObj)
        'Excel Appの終了
        If ExcelAppObj IsNot Nothing Then
            Try
                ExcelAppObj.Quit()
            Catch ex As Exception
            End Try
        End If
        ExcelProcEnd()

        disposedValue = True
    End Sub

    ' TODO: 上の Dispose(disposing As Boolean) にアンマネージド リソースを解放するコードが含まれる場合にのみ Finalize() をオーバーライドします。
    'Protected Overrides Sub Finalize()
    '    ' このコードを変更しないでください。クリーンアップ コードを上の Dispose(disposing As Boolean) に記述します。
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' このコードは、破棄可能なパターンを正しく実装できるように Visual Basic によって追加されました。
    Public Sub Dispose() Implements IDisposable.Dispose
        ' このコードを変更しないでください。クリーンアップ コードを上の Dispose(disposing As Boolean) に記述します。
        Dispose(True)
        ' TODO: 上の Finalize() がオーバーライドされている場合は、次の行のコメントを解除してください。
        ' GC.SuppressFinalize(Me)
    End Sub

    ''' <summary>
    ''' Excelプロセスの終了
    ''' </summary>
    Private Sub ExcelProcEnd()
        ExcelMemoryRelease(ExcelAppObj)
        Try
            '念のため当処理で起動したプロセスが残っていたらKill
            Dim xproc As Process = Process.GetProcessById(Me.xlProcId)
            System.Threading.Thread.Sleep(200) 'Waitかけないとプロセスが終了しきらない為
            If Not xproc.HasExited Then
                xproc.Kill()
            End If
        Catch ex As Exception
        End Try
    End Sub

#End Region

End Class
