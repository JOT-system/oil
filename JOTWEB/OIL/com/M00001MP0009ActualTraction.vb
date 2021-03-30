Option Strict On
Imports Microsoft.Office.Interop
Imports System.Runtime.InteropServices
''' <summary>
''' ���j���[��ʂ̌��ԗ�ԕʌ������т̃_�E�����[�h�@�\
''' </summary>
Public Class M00001MP0009ActualTraction : Implements IDisposable
    ''' <summary>
    ''' �G�N�Z���A�v���P�[�V�����I�u�W�F�N�g
    ''' </summary>
    Private ExcelAppObj As Excel.Application
    ''' <summary>
    ''' �G�N�Z���u�b�N�R���N�V����
    ''' </summary>
    Private ExcelBooksObj As Excel.Workbooks
    ''' <summary>
    ''' �G�N�Z���u�b�N�I�u�W�F�N�g
    ''' </summary>
    Private ExcelBookObj As Excel.Workbook
    ''' <summary>
    ''' �G�N�Z���V�[�g�R���N�V����
    ''' </summary>
    Private ExcelWorkSheets As Excel.Sheets
    ''' <summary>
    ''' �G�N�Z���V�[�g�I�u�W�F�N�g
    ''' </summary>
    Private ExcelWorkSheet As Excel.Worksheet


    ''' <summary>
    ''' ��ʓW�J���Ă�����e�Ɠ����̃f�[�^�e�[�u��
    ''' </summary>
    Private DispData As DataTable
    Private OfficeCode As String = ""
    Private OfficeName As String = ""
    Private ArrStationCode As String = ""
    Private ArrStationName As String = ""
    Private YearMonth As String = ""


    ''' <summary>
    ''' Excel�v���Z�XID
    ''' </summary>
    Private xlProcId As Integer
    ''' <summary>
    ''' ���`�t�@�C���p�X
    ''' </summary>
    Private ExcelTemplatePath As String = ""
    Private UploadRootPath As String = ""
    Private UrlRoot As String = ""
    Private PrintFilePath As String = ""
    ''' <summary>
    ''' Window�n���h�����ProcessID���擾
    ''' </summary>
    ''' <param name="hwnd"></param>
    ''' <param name="lpdwProcessId"></param>
    ''' <returns></returns>
    ''' <remarks>Excel��Window�n���h����T���v���Z�XID���擾
    ''' �������Ŏg�p����Excel�̃v���Z�XID���c���Ă����ꍇKILL����׎g�p</remarks>
    Private Declare Auto Function GetWindowThreadProcessId Lib "user32.dll" (ByVal hwnd As IntPtr,
              ByRef lpdwProcessId As Integer) As Integer

    ''' <summary>
    ''' �R���X�g���N�^
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
        '�f�B���N�g�������݂��Ȃ��ꍇ�͐���
        If IO.Directory.Exists(Me.UploadRootPath) = False Then
            IO.Directory.CreateDirectory(Me.UploadRootPath)
        End If
        '�O���v���t�B�b�N�X�̃A�b�v���[�h�t�@�C�����c���Ă����ꍇ�͍폜
        Dim targetFiles = IO.Directory.GetFiles(Me.UploadRootPath, "*.*")
        Dim keepFilePrefix As String = Now.ToString("yyyyMMdd")
        For Each targetFile In targetFiles
            Dim fileName As String = IO.Path.GetFileName(targetFile)
            '�����̓��t���擪�̃t�@�C�����̏ꍇ�͎c��
            If fileName.StartsWith(keepFilePrefix) Then
                Continue For
            End If
            Try
                IO.File.Delete(targetFile)
            Catch ex As Exception
                '�폜���̃G���[�͖���
            End Try
        Next targetFile
        'URL�̃��[�g��\��
        Me.UrlRoot = String.Format("{0}://{1}/{3}/{2}/", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Host, CS0050SESSION.USERID, CS0050SESSION.PRINT_ROOT_URL_NAME)
        'Excel�A�v���P�[�V�����I�u�W�F�N�g�̐���
        Me.ExcelAppObj = New Excel.Application
        ExcelAppObj.DisplayAlerts = False
        ExcelAppObj.ScreenUpdating = False

        Dim xlHwnd As IntPtr = CType(Me.ExcelAppObj.Hwnd, IntPtr)
        GetWindowThreadProcessId(xlHwnd, Me.xlProcId)
        'Excel���[�N�u�b�N�I�u�W�F�N�g�̐���
        Me.ExcelBooksObj = Me.ExcelAppObj.Workbooks
        Me.ExcelBookObj = Me.ExcelBooksObj.Open(Me.ExcelTemplatePath,
                                                UpdateLinks:=Excel.XlUpdateLinks.xlUpdateLinksNever,
                                                [ReadOnly]:=Excel.XlFileAccess.xlReadOnly)
        ExcelAppObj.Calculation = Excel.XlCalculation.xlCalculationManual
        Me.ExcelWorkSheets = Me.ExcelBookObj.Sheets
        Me.ExcelWorkSheet = DirectCast(Me.ExcelWorkSheets("�����Ԑ��\"), Excel.Worksheet)

    End Sub

    ''' <summary>
    ''' Excel�f�[�^��Print�t�H���_�Ɋi�[��URL���쐬
    ''' </summary>
    ''' <returns></returns>
    Public Function CreateExcelPrintData() As String
        Dim rngWrite As Excel.Range = Nothing
        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)

        Try
            '�V�[�g�ݒ�
            InitSheet()
            '�^�C�g�������̐ݒ�
            CreateHeader()
            '�f�[�^������
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
            '�ۑ��������s
            ExcelAppObj.ScreenUpdating = True
            ExcelAppObj.Calculation = Excel.XlCalculation.xlCalculationAutomatic
            ExcelAppObj.Calculate()
            Dim saveExcelLock As New Object
            SyncLock saveExcelLock '����Excel�N���œ����Z�[�u����Ɨ�����̂ŗ}�~
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
    ''' �쐬�{�_�E�����[�h�������s
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

                '��{�ʒu���擾(�O���ŏI����)
                rngWork = Me.ExcelWorkSheet.Range("D:D")

                '�s�v�ȗ���폜
                Dim lastDay As Integer = dt.AddMonths(1).AddDays(-1).Day
                For day As Integer = 31 To lastDay + 1 Step -1
                    rngOffsetWork = rngWork.Offset(ColumnOffset:=day)
                    rngOffsetWork.Delete()
                    ExcelMemoryRelease(rngOffsetWork)
                Next

                '���������v�Z���␳ & ��\��
                rngOffsetWork = rngWork.Offset(ColumnOffset:=lastDay + 1)
                rngOffsetWork.Item(RowIndex:=5) = "=INDIRECT(ADDRESS(ROW(), COLUMN() - 1)) + 1"
                rngColumnsWork = rngOffsetWork.Columns
                ExcelMemoryRelease(rngOffsetWork)

                rngColumnsWork.Hidden = True
                ExcelMemoryRelease(rngColumnsWork)

                '�O���ŏI����\��
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
    ''' �w�b�_�[�����̐ݒ�
    ''' </summary>
    Private Sub CreateHeader()
        Dim rngWork As Excel.Range = Nothing

        Try

            Dim strDt As String = String.Format("{0}/01", Me.YearMonth)
            Dim dt As Date = Nothing
            If Date.TryParse(strDt, dt) Then
                rngWork = Me.ExcelWorkSheet.Range("A2")
                rngWork.Value = String.Format("{0:yyyy�NM��}", dt)
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
            '��{�ʒu���擾
            rngWork = Me.ExcelWorkSheet.Range(rngStr)
            '�ŏI���擾
            Dim lastDay As Integer = dt.AddMonths(1).AddDays(-1).Day

            '�O������
            columnOffset = 1
            strDate = dt.AddDays(-1).ToString("yyyy/MM/dd")
            If setData.ContainsKey(strDate) Then
                rngOffsetWork = rngWork.Offset(ColumnOffset:=columnOffset)
                rngOffsetWork.Value = setData(strDate)
                ExcelMemoryRelease(rngOffsetWork)
            End If

            '����
            For day As Integer = 0 To lastDay - 1
                columnOffset += 1
                strDate = dt.AddDays(day).ToString("yyyy/MM/dd")
                If setData.ContainsKey(strDate) Then
                    rngOffsetWork = rngWork.Offset(ColumnOffset:=columnOffset)
                    rngOffsetWork.Value = setData(strDate)
                    ExcelMemoryRelease(rngOffsetWork)
                End If
            Next

            '��������
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
    ''' ���
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

            '����-ENEOS-����-8081
            Dim setData = allData.Where(Function(r)
                                            Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                            r.ARRSTATIONCODE = "2018" AndAlso
                                            r.TRAINNO = "8081"
                                        End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C7", dt, setData)

            '����-ENEOS-����-5081
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                        r.ARRSTATIONCODE = "2018" AndAlso
                                        r.TRAINNO = "5081"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C8", dt, setData)

            '����-ENEOS-����-5575
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                        r.ARRSTATIONCODE = "2018" AndAlso
                                        r.TRAINNO = "5575"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C9", dt, setData)

            '����-ENEOS-�S�R-5090
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                        r.ARRSTATIONCODE = "2407" AndAlso
                                        r.TRAINNO = "5090"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C11", dt, setData)

            'OT-�R�X�� �o��-����-8081
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 OrElse
                                        r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0122700010 AndAlso
                                        r.ARRSTATIONCODE = "2018" AndAlso
                                        r.TRAINNO = "8081"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C13", dt, setData)

            'OT-�R�X�� �o��-����-5081
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 OrElse
                                        r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0122700010 AndAlso
                                        r.ARRSTATIONCODE = "2018" AndAlso
                                        r.TRAINNO = "5081"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C14", dt, setData)

            'OT-�R�X�� �o��-����-5575
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 OrElse
                                        r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0122700010 AndAlso
                                        r.ARRSTATIONCODE = "2018" AndAlso
                                        r.TRAINNO = "5575"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C15", dt, setData)

            '�ύ��񐔁i����́j

            '�����ύ��Ԑ�
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
    ''' �܈�
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

            '����-�R�X��-�q���-8877
            Dim setData = allData.Where(Function(r)
                                            Return r.OTTRANSPORTFLG = "2" AndAlso
                                            r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 AndAlso
                                            r.ARRSTATIONCODE = "4113" AndAlso
                                            r.TRAINNO = "8877"
                                        End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C7", dt, setData)

            '����-�R�X��-�q���-8883
            setData = allData.Where(Function(r)
                                        Return r.OTTRANSPORTFLG = "2" AndAlso
                                        r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 AndAlso
                                        r.ARRSTATIONCODE = "4113" AndAlso
                                        r.TRAINNO = "8883"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C8", dt, setData)

            '����-�R�X��-�q���-5972
            setData = allData.Where(Function(r)
                                        Return r.OTTRANSPORTFLG = "2" AndAlso
                                        r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 AndAlso
                                        r.ARRSTATIONCODE = "4113" AndAlso
                                        r.TRAINNO = "5972"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C9", dt, setData)

            'OT-�R�X��-�S�R-1071
            setData = allData.Where(Function(r)
                                        Return r.OTTRANSPORTFLG = "1" AndAlso
                                        r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 AndAlso
                                        r.ARRSTATIONCODE = "2407" AndAlso
                                        r.TRAINNO = "1071"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C12", dt, setData)

            'OT-�R�X��-�S�R-8179
            setData = allData.Where(Function(r)
                                        Return r.OTTRANSPORTFLG = "1" AndAlso
                                        r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 AndAlso
                                        r.ARRSTATIONCODE = "2407" AndAlso
                                        r.TRAINNO = "8179"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C13", dt, setData)

            'OT-�R�X��-�F�s�{-8681
            setData = allData.Where(Function(r)
                                        Return r.OTTRANSPORTFLG = "1" AndAlso
                                        r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 AndAlso
                                        r.ARRSTATIONCODE = "4425" AndAlso
                                        r.TRAINNO = "8681"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C15", dt, setData)

            'OT-�R�X��-�F�s�{-8685
            setData = allData.Where(Function(r)
                                        Return r.OTTRANSPORTFLG = "1" AndAlso
                                        r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 AndAlso
                                        r.ARRSTATIONCODE = "4425" AndAlso
                                        r.TRAINNO = "8685"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C16", dt, setData)

            'OT-�R�X��-�F�s�{-9175
            setData = allData.Where(Function(r)
                                        Return r.OTTRANSPORTFLG = "1" AndAlso
                                        r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 AndAlso
                                        r.ARRSTATIONCODE = "4425" AndAlso
                                        r.TRAINNO = "9175"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C17", dt, setData)

            'OT-�R�X��-�q���-8883
            setData = allData.Where(Function(r)
                                        Return r.OTTRANSPORTFLG = "1" AndAlso
                                        r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 AndAlso
                                        r.ARRSTATIONCODE = "4113" AndAlso
                                        r.TRAINNO = "8883"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C19", dt, setData)

            'OT-�R�X��-�q���-8877
            setData = allData.Where(Function(r)
                                        Return r.OTTRANSPORTFLG = "1" AndAlso
                                        r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 AndAlso
                                        r.ARRSTATIONCODE = "4113" AndAlso
                                        r.TRAINNO = "8877"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C20", dt, setData)

            'OT-�R�X��-�q���-8763
            setData = allData.Where(Function(r)
                                        Return r.OTTRANSPORTFLG = "1" AndAlso
                                        r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 AndAlso
                                        r.ARRSTATIONCODE = "4113" AndAlso
                                        r.TRAINNO = "8763"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C21", dt, setData)

            'OT-�R�X��-�����q-2461
            setData = allData.Where(Function(r)
                                        Return r.OTTRANSPORTFLG = "1" AndAlso
                                        r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 AndAlso
                                        r.ARRSTATIONCODE = "4610" AndAlso
                                        r.TRAINNO = "2461"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C23", dt, setData)

            'OT-�R�X��-�쏼�{-2081
            setData = allData.Where(Function(r)
                                        Return r.OTTRANSPORTFLG = "1" AndAlso
                                        r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 AndAlso
                                        r.ARRSTATIONCODE = "5141" AndAlso
                                        r.TRAINNO = "2081"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C24", dt, setData)

            'OT-�R�X��-�쏼�{-5972
            setData = allData.Where(Function(r)
                                        Return r.OTTRANSPORTFLG = "1" AndAlso
                                        r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 AndAlso
                                        r.ARRSTATIONCODE = "5141" AndAlso
                                        r.TRAINNO = "5972"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C25", dt, setData)

            'OT-�R�X��-�쏼�{-9672
            setData = allData.Where(Function(r)
                                        Return r.OTTRANSPORTFLG = "1" AndAlso
                                        r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 AndAlso
                                        r.ARRSTATIONCODE = "5141" AndAlso
                                        r.TRAINNO = "9672"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C26", dt, setData)

            '�ύ��񐔁i���̓����������Ԃ̎󒍖��ׂ̉���̍ő�l�j
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.LINE).Max())
            SetRowValues("C30", dt, setData)

            '�����ύ��Ԑ�
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
    ''' �b�q
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

            'OT-ENEOS-�F�s�{-8685
            Dim setData = allData.Where(Function(r)
                                            Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                            r.ARRSTATIONCODE = "4425" AndAlso
                                            r.TRAINNO = "8685"
                                        End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C7", dt, setData)

            'OT-ENEOS-�F�s�{-2685
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                        r.ARRSTATIONCODE = "4425" AndAlso
                                        r.TRAINNO = "2685"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C8", dt, setData)

            '�ύ��񐔁i���̓����������Ԃ̎󒍖��ׂ̉���̍ő�l�j
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.LINE).Max())
            SetRowValues("C12", dt, setData)

            '�����ύ��Ԑ�
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
    ''' �����Y
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

            '����-�o��-�q���-8877
            Dim setData = allData.Where(Function(r)
                                            Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0122700010 AndAlso
                                            r.ARRSTATIONCODE = "4113" AndAlso
                                            r.TRAINNO = "8877"
                                        End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C7", dt, setData)

            '����-�o��-�q���-8883
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0122700010 AndAlso
                                        r.ARRSTATIONCODE = "4113" AndAlso
                                        r.TRAINNO = "8883"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C8", dt, setData)

            '����-�o��-�쏼�{-5461
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0122700010 AndAlso
                                        r.ARRSTATIONCODE = "5141" AndAlso
                                        r.TRAINNO = "5461"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C10", dt, setData)

            '����-�o��-�쏼�{-9672
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0122700010 AndAlso
                                        r.ARRSTATIONCODE = "5141" AndAlso
                                        r.TRAINNO = "9672"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C11", dt, setData)

            '�ύ��񐔁i���̓����������Ԃ̎󒍖��ׂ̉���̍ő�l�j
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0122700010
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.LINE).Max())
            SetRowValues("C15", dt, setData)

            '�����ύ��Ԑ�
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
    ''' ����
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

            '����-ENEOS-�F�s�{-4091
            Dim setData = allData.Where(Function(r)
                                            Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                            r.ARRSTATIONCODE = "4425" AndAlso
                                            r.TRAINNO = "4091"
                                        End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C7", dt, setData)

            '����-ENEOS-�F�s�{-8571
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                        r.ARRSTATIONCODE = "4425" AndAlso
                                        r.TRAINNO = "8571"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C8", dt, setData)

            '����-ENEOS-�F�s�{-8569
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                        r.ARRSTATIONCODE = "4425" AndAlso
                                        r.TRAINNO = "8569"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C9", dt, setData)

            '����-ENEOS-�q���-3091
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                        r.ARRSTATIONCODE = "4113" AndAlso
                                        r.TRAINNO = "3091"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C11", dt, setData)

            '����-ENEOS-�q���-3093
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                        r.ARRSTATIONCODE = "4113" AndAlso
                                        r.TRAINNO = "3093"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C12", dt, setData)

            '����-ENEOS-�q���-8777
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                        r.ARRSTATIONCODE = "4113" AndAlso
                                        r.TRAINNO = "8777"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C13", dt, setData)

            '����-ENEOS-�����q-85
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                        r.ARRSTATIONCODE = "4610" AndAlso
                                        r.TRAINNO = "85"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C15", dt, setData)

            '����-ENEOS-�����q-87
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                        r.ARRSTATIONCODE = "4610" AndAlso
                                        r.TRAINNO = "87"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C16", dt, setData)

            '����-ENEOS-�����q-5692
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                        r.ARRSTATIONCODE = "4610" AndAlso
                                        r.TRAINNO = "5692"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C17", dt, setData)

            '����-ENEOS-�����q-8097
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                        r.ARRSTATIONCODE = "4610" AndAlso
                                        r.TRAINNO = "8097"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C18", dt, setData)

            '����-ENEOS-����-5575
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                        r.ARRSTATIONCODE = "4620" AndAlso
                                        r.TRAINNO = "81"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C20", dt, setData)

            '����-ENEOS-����-5575
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                        r.ARRSTATIONCODE = "4620" AndAlso
                                        r.TRAINNO = "83"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C21", dt, setData)

            '����-ENEOS-���-2085
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                        r.ARRSTATIONCODE = "5009" AndAlso
                                        r.TRAINNO = "2085"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C23", dt, setData)

            '����-ENEOS-���-5463
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                        r.ARRSTATIONCODE = "5009" AndAlso
                                        r.TRAINNO = "5463"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C24", dt, setData)

            '����-ENEOS-���-8471
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0005700010 AndAlso
                                        r.ARRSTATIONCODE = "5009" AndAlso
                                        r.TRAINNO = "8471"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C25", dt, setData)

            '�ύ��񐔁i����́j

            '�����ύ��Ԑ�
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
    ''' �l���s
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

            '����-�R�X��-�쏼�{-6078
            Dim setData = allData.Where(Function(r)
                                            Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 AndAlso
                                            r.ARRSTATIONCODE = "5141" AndAlso
                                            r.TRAINNO = "6078"
                                        End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C7", dt, setData)

            '����-�R�X��-�쏼�{-8380
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0094000010 AndAlso
                                        r.ARRSTATIONCODE = "5141" AndAlso
                                        r.TRAINNO = "8380"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C8", dt, setData)

            '�ύ��񐔁i����́j

            '�����ύ��Ԑ�
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
    ''' �O�d���l
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

            '����-�o��-�쏼�{-5282
            Dim setData = allData.Where(Function(r)
                                            Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0122700010 AndAlso
                                            r.ARRSTATIONCODE = "5141" AndAlso
                                            r.TRAINNO = "5282"
                                        End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C7", dt, setData)

            '����-�o��-�쏼�{-8072
            setData = allData.Where(Function(r)
                                        Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0122700010 AndAlso
                                        r.ARRSTATIONCODE = "5141" AndAlso
                                        r.TRAINNO = "8072"
                                    End Function).
                GroupBy(Function(r) New With {Key r.LODDATE}).
                ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            SetRowValues("C8", dt, setData)

            ''����-�o��-�쏼�{-174
            'setData = allData.Where(Function(r)
            '                            Return r.SHIPPERCODE = BaseDllConst.CONST_SHIPPERCODE_0122700010 AndAlso
            '                            r.ARRSTATIONCODE = "5141" AndAlso
            '                            r.TRAINNO = "174"
            '                        End Function).
            '    GroupBy(Function(r) New With {Key r.LODDATE}).
            '    ToDictionary(Function(g) g.Key.LODDATE, Function(g) g.Select(Function(r) r.CARSNUMBER).Sum())
            'SetRowValues("C9", dt, setData)

            '�ύ��񐔁i����́j

            '�����ύ��Ԑ�
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
    ''' Excel�I�u�W�F�N�g�̉��
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="objCom"></param>
    Private Sub ExcelMemoryRelease(Of T As Class)(ByRef objCom As T)

        '�����^�C�����s�Ώۂ�ComObject�̃A���}�l�[�W�R�[�h�̏ꍇ�A�������J��
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
    Private disposedValue As Boolean ' �d������Ăяo�������o����ɂ�

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: �}�l�[�W�h��Ԃ�j�����܂� (�}�l�[�W�h �I�u�W�F�N�g)�B
            End If

            ' TODO: �A���}�l�[�W�h ���\�[�X (�A���}�l�[�W�h �I�u�W�F�N�g) ��������A���� Finalize() ���I�[�o�[���C�h���܂��B
            ' TODO: �傫�ȃt�B�[���h�� null �ɐݒ肵�܂��B
        End If

        'Excel Sheet�I�u�W�F�N�g�̉��
        ExcelMemoryRelease(ExcelWorkSheet)
        'Excel Sheet�R���N�V�����̉��
        ExcelMemoryRelease(ExcelWorkSheets)
        'Excel Book�I�u�W�F�N�g�����
        If ExcelBookObj IsNot Nothing Then
            Try
                'ExcelBookObj.Close(Excel.XlSaveAction.xlDoNotSaveChanges)
                ExcelBookObj.Close(False)
            Catch ex As Exception
            End Try
        End If

        ExcelMemoryRelease(ExcelBookObj)
        'Excel Book�R���N�V�����̉��
        ExcelMemoryRelease(ExcelBooksObj)
        'Excel App�̏I��
        If ExcelAppObj IsNot Nothing Then
            Try
                ExcelAppObj.Quit()
            Catch ex As Exception
            End Try
        End If
        ExcelProcEnd()

        disposedValue = True
    End Sub

    ' TODO: ��� Dispose(disposing As Boolean) �ɃA���}�l�[�W�h ���\�[�X���������R�[�h���܂܂��ꍇ�ɂ̂� Finalize() ���I�[�o�[���C�h���܂��B
    'Protected Overrides Sub Finalize()
    '    ' ���̃R�[�h��ύX���Ȃ��ł��������B�N���[���A�b�v �R�[�h����� Dispose(disposing As Boolean) �ɋL�q���܂��B
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' ���̃R�[�h�́A�j���\�ȃp�^�[���𐳂��������ł���悤�� Visual Basic �ɂ���Ēǉ�����܂����B
    Public Sub Dispose() Implements IDisposable.Dispose
        ' ���̃R�[�h��ύX���Ȃ��ł��������B�N���[���A�b�v �R�[�h����� Dispose(disposing As Boolean) �ɋL�q���܂��B
        Dispose(True)
        ' TODO: ��� Finalize() ���I�[�o�[���C�h����Ă���ꍇ�́A���̍s�̃R�����g���������Ă��������B
        ' GC.SuppressFinalize(Me)
    End Sub

    ''' <summary>
    ''' Excel�v���Z�X�̏I��
    ''' </summary>
    Private Sub ExcelProcEnd()
        ExcelMemoryRelease(ExcelAppObj)
        Try
            '�O�̂��ߓ������ŋN�������v���Z�X���c���Ă�����Kill
            Dim xproc As Process = Process.GetProcessById(Me.xlProcId)
            System.Threading.Thread.Sleep(200) 'Wait�����Ȃ��ƃv���Z�X���I��������Ȃ���
            If Not xproc.HasExited Then
                xproc.Kill()
            End If
        Catch ex As Exception
        End Try
    End Sub

#End Region

End Class
