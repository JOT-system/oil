Option Explicit On

Imports System.Data.SqlClient
Imports System.IO
Imports System.Reflection
Imports System.Text
Imports System.Linq

Namespace GRT00004COM
#Region "<< T4共通親クラス >>"
    ''' <summary>
    ''' T4系共通クラス
    ''' </summary>
    ''' <remarks></remarks>
    Public MustInherit Class GRT00004COM : Implements IDisposable

        ''' <summary>
        ''' SQLコネクション
        ''' </summary>
        ''' <remarks></remarks>
        Public Property SQLcon As SqlConnection

        ''' <summary>
        ''' ERRNoプロパティ
        ''' </summary>
        ''' <returns>ERRNo</returns>
        Public Property ERR As String


        ''' <summary>
        ''' セッション情報
        ''' </summary>
        Protected Property sm As CS0050SESSION

        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Me.Initialize()
        End Sub

        ''' <summary>
        ''' 初期化
        ''' </summary>
        ''' <remarks></remarks> 
        Protected Sub Initialize()
            ERR = C_MESSAGE_NO.NORMAL
            sm = New CS0050SESSION
            SQLcon = sm.getConnection
        End Sub

        ''' <summary>
        ''' 解放処理
        ''' </summary>
        Public Overridable Sub Dispose() Implements IDisposable.Dispose
            Me.SQLcon = Nothing
            sm = Nothing
        End Sub

        ''' <summary>
        ''' ログ出力
        ''' </summary>
        ''' <remarks></remarks> 
        Protected Sub PutLog(ByVal messageNo As String,
                       ByVal niwea As String,
                       Optional ByVal messageText As String = "",
                       <System.Runtime.CompilerServices.CallerMemberName> Optional callerMemberName As String = Nothing)
            Dim logWrite As New CS0011LOGWrite With {
            .INFSUBCLASS = Me.GetType.Name,
            .INFPOSI = callerMemberName,
            .NIWEA = niwea,
            .TEXT = messageText,
            .MESSAGENO = messageNo
        }
            logWrite.CS0011LOGWrite()
        End Sub
    End Class
#End Region

#Region "<< 光英オーダー連携関連 >>"

    ''' <summary>
    ''' 光英オーダー管理
    ''' </summary>
    Public Class GRW0001KOUEIORDER
        Inherits GRT00004COM

        ''' <summary>
        ''' 光英オーダーCSV項目数
        ''' </summary>
        Public Const C_KOUEI_CSV_COLUMS As Integer = 110
        ''' <summary>
        ''' 光英オーダーCSV項目内区切り文字数
        ''' </summary>
        '''<remarks >CSV区切り文字ではありません。項目内用</remarks>
        Public Const C_KOUEI_CSV_COLUMS_DELIMITER As String = "|"

        ''' <summary>
        ''' 光英タイプ
        ''' </summary>
        Public Class KOUEITYPE_PREFIX
            Public Const JOT As String = "jot"
            Public Const JX As String = "jx"
            Public Const TG As String = "tg"
            Public Const COSMO As String = "cosmo"

            Public Const JXTG As String = "jxtg"
        End Class

        ''' <summary>
        ''' 回順（明細№）
        ''' </summary>
        Public Enum TRIPSEQ_TYPE As Byte
            START = 0   '始業
            DEPT = 1    '基地発
            'DELV = 2～  配送先 
            'RTN = xx    基地戻（配送先MAX+1） 
            FIN = 99    '終業
        End Enum

        ''' <summary>
        ''' 光英オーダー
        ''' </summary>
        Public Class KOUEI_ORDER
            ''' <summary>
            ''' 光英区分
            ''' </summary>
            Public KOUEITYPE As String
            ''' <summary>
            ''' 基準日（出庫日）
            ''' </summary>
            Public KIJUNDATE As String
            ''' <summary>
            ''' オーダー識別ID(KOUEITYPE_KIJUNDATE_SHABANB_TRIP)
            ''' </summary>
            ''' <remarks >光英区分及びNo.86 コードベースキー</remarks>
            Public ORDERID As String
            ''' <summary>
            ''' CSV行番号
            ''' </summary>
            Public ROWNO As Integer
            ''' <summary>
            ''' 出力対象
            ''' </summary>
            Public TARGET As Boolean

            ''' <summary>
            ''' 光英オーダーCSV
            ''' </summary>
            ''' <remarks>光英オーダーの正確なレイアウト定義がない為、全て文字列リストとして管理</remarks> 
            Private _csvData As List(Of String)
            ''' <summary>
            ''' 光英オーダーCSV文字列
            ''' </summary>
            Public Property CSVDATA As String
                Get
                    Return String.Join(",", _csvData)
                End Get
                Set(value As String)
                    Dim work As String() = value.Split(",")
                    ' CSVフォーマットチェック
                    If work.Count = C_KOUEI_CSV_COLUMS Then
                        _csvData = work.ToList
                    Else
                        _csvData.Clear()
                    End If
                End Set
            End Property
            ''' <summary>
            ''' 光英オーダーCSV 各フィールドアクセサ
            ''' </summary>
            ''' <param name="fieldNo" >CSV項目№</param>
            ''' <remarks >№1～№108まで</remarks>
            Public Function FIELD(ByVal fieldNo As Integer) As String
                Return _csvData(fieldNo - 1)
            End Function
            ''' <summary>
            ''' 光英オーダーCSV No.1:トリップ
            ''' </summary>
            Public ReadOnly Property TRIP As String
                Get
                    Return _csvData(0)
                End Get
            End Property
            ''' <summary>
            ''' 光英オーダーCSV No.6:車番
            ''' </summary>
            Public ReadOnly Property SHABANCD As String
                Get
                    Return _csvData(5)
                End Get
            End Property
            ''' <summary>
            ''' 光英オーダーCSV No.8:ルート番号([j]or[a] + 車番CD)  
            ''' </summary>
            Public ReadOnly Property ROUTENO As String
                Get
                    Return _csvData(7)
                End Get
            End Property
            ''' <summary>
            ''' 光英オーダーCSV No.9:回順
            ''' </summary>
            Public ReadOnly Property TRIPSEQ As String
                Get
                    Return _csvData(8)
                End Get
            End Property

            ''' <summary>
            ''' 光英オーダーCSV No.13:行先コード
            ''' </summary>
            Public ReadOnly Property DESTCODE As String
                Get
                    Return _csvData(12)
                End Get
            End Property

            ''' <summary>
            ''' 光英オーダーCSV No.14:届先名称
            ''' </summary>
            Public ReadOnly Property DESTNAME As String
                Get
                    Return _csvData(13)
                End Get
            End Property

            ''' <summary>
            ''' 光英オーダーCSV No.26:光英X座標
            ''' </summary>
            Public ReadOnly Property POSI_X As String
                Get
                    Return _csvData(25)
                End Get
            End Property
            ''' <summary>
            ''' 光英オーダーCSV No.27:光英Y座標
            ''' </summary>
            Public ReadOnly Property POSI_Y As String
                Get
                    Return _csvData(26)
                End Get
            End Property

            ''' <summary>
            ''' 光英オーダーCSV No.28:車両コード
            ''' </summary>
            Public ReadOnly Property SHARYOCODE As String
                Get
                    Return _csvData(27)
                End Get
            End Property
            ''' <summary>
            ''' 光英オーダーCSV No.29:正乗務員ｺｰﾄﾞ
            ''' </summary>
            Public Property STAFFCODE As String
                Get
                    Return _csvData(28)
                End Get
                Set(value As String)
                    _csvData(28) = value
                    TARGET = True
                End Set
            End Property
            ''' <summary>
            ''' 光英オーダーCSV No.30:副乗務員ｺｰﾄﾞ
            ''' </summary>
            Public Property SUBSTAFFCODE As String
                Get
                    Return _csvData(29)
                End Get
                Set(value As String)
                    _csvData(29) = value
                End Set
            End Property
            ''' <summary>
            ''' 光英オーダーCSV No.51:デポフラグ
            ''' </summary>
            Public ReadOnly Property DEPOFLAG As String
                Get
                    Return _csvData(50)
                End Get
            End Property
            ''' <summary>
            ''' 光英オーダーCSV No.56:前車番
            ''' </summary>
            Public ReadOnly Property SHABANF As String
                Get
                    Return _csvData(55)
                End Get
            End Property
            ''' <summary>
            ''' 光英オーダーCSV No.57:後車番
            ''' </summary>
            Public ReadOnly Property SHABANB As String
                Get
                    Return _csvData(56)
                End Get
            End Property
            ''' <summary>
            ''' 光英オーダーCSV No.58:乗務員情報
            ''' </summary>
            Public Property STAFFINFO As String
                Get
                    Return _csvData(57)
                End Get
                Set(value As String)
                    _csvData(57) = value
                End Set
            End Property
            ''' <summary>
            ''' 光英オーダーCSV No.59:副乗務員情報
            ''' </summary>
            Public Property SUBSTAFFINFO As String
                Get
                    Return _csvData(58)
                End Get
                Set(value As String)
                    _csvData(58) = value
                End Set
            End Property
            ''' <summary>
            ''' 光英オーダーCSV No.69:注釈１
            ''' </summary>
            Public ReadOnly Property MEMO1 As String
                Get
                    Return _csvData(68)
                End Get
            End Property
            ''' <summary>
            ''' 光英オーダーCSV No.88:注釈２
            ''' </summary>
            Public ReadOnly Property MEMO2 As String
                Get
                    Return _csvData(87)
                End Get
            End Property
            ''' <summary>
            ''' 光英オーダーCSV No.72:ジョイント情報
            ''' </summary>
            Public ReadOnly Property JOINT As String
                Get
                    Return _csvData(71)
                End Get
            End Property

            ''' <summary>
            ''' 光英オーダーCSV No.73:受注品名名称
            ''' </summary>
            Public ReadOnly Property PRODUCTNAME As String()
                Get
                    Return _csvData(72).Split(C_KOUEI_CSV_COLUMS_DELIMITER)
                End Get
            End Property

            ''' <summary>
            ''' 光英オーダーCSV No.74:受注品名コード
            ''' </summary>
            Public ReadOnly Property PRODUCTCODE As String()
                Get
                    Return _csvData(73).Split(C_KOUEI_CSV_COLUMS_DELIMITER)
                End Get
            End Property

            ''' <summary>
            ''' 光英オーダーCSV No.75:受注品名別量
            ''' </summary>
            Public ReadOnly Property PRODUCTNUM As String()
                Get
                    Return _csvData(74).Split(C_KOUEI_CSV_COLUMS_DELIMITER)
                End Get
            End Property

            ''' <summary>
            ''' 光英オーダーCSV No.79:注文主情報
            ''' </summary>
            Public ReadOnly Property OWNERINFO As String
                Get
                    Return _csvData(78)
                End Get
            End Property

            ''' <summary>
            ''' 光英オーダーCSV No.86:コースベースキー
            ''' </summary>
            Public ReadOnly Property COURSEBASEKEY As String
                Get
                    Return _csvData(85)
                End Get
            End Property
            ''' <summary>
            ''' 光英オーダーCSV No.86:コースベースキー出荷日
            ''' </summary>
            Public ReadOnly Property COURSEBASEKEY_DATE As String
                Get
                    Return COURSEBASEKEY.Split(C_KOUEI_CSV_COLUMS_DELIMITER)(0)
                End Get
            End Property

            ''' <summary>
            ''' コンストラクタ
            ''' </summary>
            Public Sub New()
                _csvData = New List(Of String)(C_KOUEI_CSV_COLUMS)
            End Sub
            ''' <summary>
            ''' コンストラクタ
            ''' </summary>
            ''' <param name="I_data">CSVデータ</param> 
            Public Sub New(ByVal I_data As String)
                Me.New()
                CSVDATA = I_data
            End Sub
            ''' <summary>
            ''' CSVデータ設定
            ''' </summary>
            ''' <param name="csvList">CSVデータ</param> 
            Public Sub SetCSVDATA(ByVal csvList As List(Of String))
                Me._csvData = csvList
            End Sub
            ''' <summary>
            ''' CSVデータ設定
            ''' </summary>
            ''' <param name="csvList">CSVデータ</param> 
            Public Sub SetCSVDATA(ByVal csvList As String())
                Me._csvData = csvList.ToList
            End Sub
            ''' <summary>
            ''' CSVデータ乗務員設定
            ''' </summary>
            ''' <param name="staffNo">乗務員番号</param> 
            ''' <param name="staffName">乗務員名</param> 
            ''' <param name="addInfo">乗務員追加情報（会社コード,部門コード,ふりがな,乗務員略称,備考）</param> 
            Public Sub SetStaffInfo(ByVal staffNo As String, ByVal staffName As String, Optional ByVal addInfo As String() = Nothing)
                Me.STAFFINFO = staffNo & C_KOUEI_CSV_COLUMS_DELIMITER & staffName
                If Not IsNothing(addInfo) Then
                    Me.STAFFINFO &= C_KOUEI_CSV_COLUMS_DELIMITER & String.Join(C_KOUEI_CSV_COLUMS_DELIMITER, addInfo)
                Else
                    Me.STAFFINFO &= C_KOUEI_CSV_COLUMS_DELIMITER & String.Join(C_KOUEI_CSV_COLUMS_DELIMITER, {"", "", "", "", ""})
                End If
                TARGET = True
            End Sub
            ''' <summary>
            ''' CSVデータ副乗務員設定
            ''' </summary>
            ''' <param name="staffNo">乗務員番号</param> 
            ''' <param name="staffName">乗務員名</param> 
            ''' <param name="addInfo">乗務員追加情報（会社コード,部門コード,ふりがな,乗務員略称,備考）</param> 
            Public Sub SetSubStaffInfo(ByVal staffNo As String, ByVal staffName As String, Optional ByVal addInfo As String() = Nothing)
                Me.SUBSTAFFINFO = staffNo & C_KOUEI_CSV_COLUMS_DELIMITER & staffName
                If Not IsNothing(addInfo) Then
                    Me.SUBSTAFFINFO &= C_KOUEI_CSV_COLUMS_DELIMITER & String.Join(C_KOUEI_CSV_COLUMS_DELIMITER, addInfo)
                Else
                    Me.SUBSTAFFINFO &= C_KOUEI_CSV_COLUMS_DELIMITER & String.Join(C_KOUEI_CSV_COLUMS_DELIMITER, {"", "", "", "", ""})
                End If
            End Sub

            ''' <summary>
            ''' オーダー識別ID作成
            ''' </summary>
            ''' <returns >文字列</returns>  
            ''' <remarks >光英タイプ_基準日_後車番_TRIP</remarks>
            Public Function MakeOrderId() As String
                Dim sb As StringBuilder = New StringBuilder
                sb.Append(KOUEITYPE.ToUpper.Chars(0))
                sb.Append(KIJUNDATE.Replace("/", ""))
                sb.Append(SHABANB)
                sb.Append(TRIP)
                Return sb.ToString
            End Function

            ''' <summary>
            ''' Dictionaryキー作成
            ''' </summary>
            ''' <returns >キー文字列</returns>  
            ''' <remarks >オーダー識別ID|回順</remarks>
            Public Function MakeDicKey() As String
                Dim sb As StringBuilder = New StringBuilder
                sb.Append(ORDERID)
                sb.Append(C_VALUE_SPLIT_DELIMITER)
                sb.Append(TRIPSEQ)
                Return sb.ToString
            End Function

        End Class

        ''' <summary>
        ''' 光英オーダー管理
        ''' </summary>
        Private _dicOrder As Dictionary(Of String, KOUEI_ORDER)

        ''' <summary>
        ''' [IN]CAMPCODEプロパティ
        ''' </summary>
        ''' <returns>[IN]CAMPCODE</returns>
        Public Property CAMPCODE() As String
        ''' <summary>
        ''' [IN]ORGCODEプロパティ
        ''' </summary>
        ''' <returns>[IN]ORGCODE</returns>
        Public Property ORGCODE() As String
        ''' <summary>
        ''' [IN]KIJUNDATE_FROM プロパティ
        ''' </summary>
        ''' <returns>[IN]KIJUNDATE</returns>
        Public Property KIJUNDATEF() As Date
        ''' <summary>
        ''' [IN]KIJUNDATE TO プロパティ
        ''' </summary>
        ''' <returns>[IN]KIJUNDATE</returns>
        Public Property KIJUNDATET() As Date

        ''' <summary>
        ''' 初期化
        ''' </summary>
        ''' <remarks></remarks> 
        Public Overloads Sub Initialize()
            MyBase.Initialize()

            CAMPCODE = String.Empty
            ORGCODE = String.Empty
            KIJUNDATEF = Nothing
            KIJUNDATET = Nothing
            If Not IsNothing(_dicOrder) Then
                _dicOrder.Clear()
                _dicOrder = Nothing
            End If
        End Sub

        ''' <summary>
        ''' 解放処理
        ''' </summary>
        Public Overrides Sub Dispose()
            If Not IsNothing(_dicOrder) Then
                _dicOrder.Clear()
                _dicOrder = Nothing
            End If
            MyBase.Dispose()
        End Sub


        ''' <summary>
        ''' 光英データ取得
        ''' </summary>
        ''' <param name="koueiType" >光英タイプ</param>
        ''' <returns>光英データDictionary</returns>
        Public Function GetOrder(Optional ByVal koueiType As String = "") As Dictionary(Of String, KOUEI_ORDER)
            If IsNothing(_dicOrder) Then
                Return Nothing
            End If

            If String.IsNullOrEmpty(koueiType) Then
                Return _dicOrder
            Else
                Return _dicOrder.Where(Function(x) x.Value.KOUEITYPE = koueiType).ToDictionary(Function(x) x.Key, Function(x) x.Value)
            End If
        End Function
        ''' <summary>
        ''' 光英データ件数
        ''' </summary>
        ''' <param name="koueiType" >光英タイプ</param>
        ''' <returns>件数</returns>
        Public Function Count(Optional ByVal koueiType As String = "") As Integer
            If IsNothing(_dicOrder) Then
                Return 0
            End If

            If String.IsNullOrEmpty(koueiType) Then
                Return _dicOrder.Count
            Else
                Return _dicOrder.Where(Function(x) x.Value.KOUEITYPE = koueiType).Count()
            End If
        End Function

        ''' <summary>
        ''' 光英データ読み込み
        ''' </summary>
        ''' <returns>TRUE|FALSE</returns>
        ''' <remarks> OK:00000</remarks> 
        Public Function ReadOrder() As Boolean

            'SQL
            Dim sb As StringBuilder = New StringBuilder
            sb.Append("SELECT ")
            sb.Append("  CAMPCODE ")
            sb.Append("  , ORGCODE ")
            sb.Append("  , KIJUNDATE ")
            sb.Append("  , KOUEITYPE ")
            sb.Append("  , ORDERID ")
            sb.Append("  , TRIPSEQ ")
            sb.Append("  , ROWNO ")
            sb.Append("  , CSVDATA ")
            sb.Append("FROM ")
            sb.Append("  W0001_KOUEIORDER ")
            sb.Append("WHERE ")
            sb.Append("  CAMPCODE = @CAMPCODE ")
            sb.Append("  and ORGCODE = @ORGCODE ")
            sb.Append("  and KIJUNDATE >= @KIJUNDATEF ")
            sb.Append("  and KIJUNDATE <= @KIJUNDATET ")
            sb.Append("ORDER BY ")
            sb.Append("  CAMPCODE ")
            sb.Append("  , ORGCODE ")
            sb.Append("  , KIJUNDATE ")
            sb.Append("  , KOUEITYPE ")
            sb.Append("  , ROWNO ")

            Try
                If IsNothing(_dicOrder) Then
                    _dicOrder = New Dictionary(Of String, KOUEI_ORDER)
                Else
                    _dicOrder.Clear()
                End If

                If SQLcon.State <> ConnectionState.Open Then
                    SQLcon.Open() 'DataBase接続(Open)
                End If

                Dim SQLcmd As New SqlCommand(sb.ToString, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar)
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@ORGCODE", System.Data.SqlDbType.NVarChar)
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@KIJUNDATEF", System.Data.SqlDbType.Date)
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@KIJUNDATET", System.Data.SqlDbType.Date)
                PARA01.Value = Me.CAMPCODE
                PARA02.Value = Me.ORGCODE
                PARA03.Value = Me.KIJUNDATEF
                PARA04.Value = Me.KIJUNDATET

                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                'フィールド名とフィールドの型を取得
                While SQLdr.Read()
                    Dim order = New KOUEI_ORDER With {
                        .KOUEITYPE = SQLdr.Item("KOUEITYPE"),
                        .KIJUNDATE = SQLdr.Item("KIJUNDATE"),
                        .ORDERID = SQLdr.Item("ORDERID"),
                        .ROWNO = SQLdr.Item("ROWNO")
                    }
                    order.CSVDATA = SQLdr.Item("CSVDATA")
                    If Not String.IsNullOrEmpty(order.STAFFCODE) Then
                        order.TARGET = True
                    End If
                    _dicOrder.Add(order.MakeDicKey, order)
                End While

                Return True
            Catch ex As Exception
                ' その他例外

                Me.ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
                PutLog(Me.ERR, C_MESSAGE_TYPE.ABORT, ex.Message)

                Return False
            Finally
                SQLcon.Close()
            End Try

        End Function
        ''' <summary>
        ''' 光英CSVファイル読み込み
        ''' </summary>
        ''' <returns>TRUE|FALSE</returns>
        ''' <remarks> OK:00000</remarks> 
        Public Function ReadCSV(ByVal file As FileInfo) As Boolean
            'ファイル名（PATHなし、拡張子なし）
            Dim fileName As String = file.FullName
            Dim temp As String() = file.Name.Replace(file.Extension, "").Split("_")
            Dim koueiType As String = temp(0)               '光英タイプ
            Dim fileDate As String = temp(1)                '基準日
            Dim filetimestamp As String = temp(2)           '未使用


            Dim hasHeader As Boolean = True

            Try
                If IsNothing(_dicOrder) Then
                    _dicOrder = New Dictionary(Of String, KOUEI_ORDER)
                End If

                'Shift JISで読み込みます。
                Using WW_Text As New FileIO.TextFieldParser(fileName, System.Text.Encoding.GetEncoding(932))

                    'フィールドが文字で区切られている設定を行います。
                    '（初期値がDelimited）
                    WW_Text.TextFieldType = FileIO.FieldType.Delimited

                    '区切り文字を「,（カンマ）」に設定します。
                    WW_Text.Delimiters = New String() {","}

                    'フィールドを"で囲み、改行文字、区切り文字を含めることが 'できるかを設定します。
                    '（初期値がtrue）
                    WW_Text.HasFieldsEnclosedInQuotes = True

                    'フィールドの前後からスペースを削除する設定を行います。
                    '（初期値がtrue）
                    WW_Text.TrimWhiteSpace = True

                    Dim tmpOrderId As String = String.Empty
                    While Not WW_Text.EndOfData
                        'ヘッダカラムは行数に含めない
                        Dim WW_RowNo As Integer = WW_Text.LineNumber - 1
                        'CSVファイルのフィールドを読み込みます。
                        Dim fields As String() = WW_Text.ReadFields()
                        If hasHeader = True Then
                            'ヘッダーカラム読み飛ばし
                            hasHeader = False
                            Continue While
                        End If
                        If fields.Length <> C_KOUEI_CSV_COLUMS Then
                            Me.ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
                            PutLog(C_MESSAGE_NO.FILE_IO_ERROR, C_MESSAGE_TYPE.ABORT, "CSV項目数不正")
                            Return False
                        End If
                        Dim data As KOUEI_ORDER = New KOUEI_ORDER With {
                            .KOUEITYPE = koueiType,
                            .KIJUNDATE = fileDate,
                            .ROWNO = WW_RowNo
                        }
                        data.SetCSVDATA(fields)
                        If data.TRIPSEQ = TRIPSEQ_TYPE.START Then
                            tmpOrderId = data.MakeOrderId()
                        End If
                        data.ORDERID = tmpOrderId
                        If Not String.IsNullOrEmpty(data.STAFFCODE) Then
                            data.TARGET = True
                        End If

                        _dicOrder.Add(data.MakeDicKey, data)
                    End While

                End Using

                Return True

            Catch ex As Exception
                Me.ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
                PutLog(C_MESSAGE_NO.FILE_IO_ERROR, C_MESSAGE_TYPE.ABORT, ex.ToString)

                Return False
            End Try
        End Function

        ''' <summary>
        ''' 光英CSVファイル書込み
        ''' </summary>
        ''' <remarks></remarks>
        Public Function WriteCSV(ByVal koueiType As String, ByVal kijunDate As String, ByVal filePath As String) As Boolean
            Try
                If IsNothing(_dicOrder) Then
                    Me.ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
                    PutLog(C_MESSAGE_NO.DLL_IF_ERROR, C_MESSAGE_TYPE.ABORT)
                    Return False
                End If

                Dim fileName = Path.Combine(filePath, String.Format("{0}_{1}.{2}", koueiType, kijunDate.Replace("/", ""), "csv"))

                Using sr As New System.IO.StreamWriter(fileName, False, System.Text.Encoding.GetEncoding(932))
                    Dim sb As StringBuilder = New StringBuilder()

                    'ヘッダ行作成
                    For i As Integer = 1 To C_KOUEI_CSV_COLUMS
                        sb.Append(i.ToString("000"))
                        sb.Append(","c)
                    Next
                    '終端カンマを改行に置換
                    sb.Replace(",", vbNewLine, sb.Length - 1, 1)
                    sr.Write(sb)

                    '出力対象光栄オーダー取得
                    Dim order = Me.GetOrder(koueiType).Where(Function(x) x.Value.KIJUNDATE = kijunDate)

                    'レコード書込
                    For Each row In order
                        sr.WriteLine(row.Value.CSVDATA)
                    Next

                End Using

                Return True

            Catch ex As Exception
                Me.ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
                PutLog(C_MESSAGE_NO.FILE_IO_ERROR, C_MESSAGE_TYPE.ABORT, ex.ToString)

                Return False
            End Try

        End Function

        ''' <summary>
        ''' 光英データ保存
        ''' </summary>
        ''' <returns>TRUE|FALSE</returns>
        ''' <remarks> OK:00000</remarks> 
        Public Function WriteOrder() As Boolean
            Dim now As DateTime = Date.Now

            'SQL
            Dim SQLStr As String =
               " DECLARE @hensuu as bigint ; " _
             & " set @hensuu = 0 ; " _
             & " DECLARE hensuu CURSOR FOR " _
             & "   SELECT CAST(UPDTIMSTP as bigint) as hensuu " _
             & "     FROM W0001_KOUEIORDER " _
             & "     WHERE CAMPCODE     = @P01 " _
             & "     AND   ORGCODE      = @P02 " _
             & "     AND   KIJUNDATE    = @P03 " _
             & "     AND   KOUEITYPE    = @P04 " _
             & "     AND   ORDERID      = @P05 " _
             & "     AND   TRIPSEQ      = @P06 " _
             & " OPEN hensuu ; " _
             & " FETCH NEXT FROM hensuu INTO @hensuu ; " _
             & " IF ( @@FETCH_STATUS = 0 ) " _
             & "    UPDATE W0001_KOUEIORDER " _
             & "       SET ROWNO        = @P07 , " _
             & "           CSVDATA      = @P08 , " _
             & "           DELFLG       = @P09 , " _
             & "           UPDYMD       = @P11 , " _
             & "           UPDUSER      = @P12 , " _
             & "           UPDTERMID    = @P13 , " _
             & "           RECEIVEYMD   = @P14   " _
             & "     WHERE CAMPCODE     = @P01 " _
             & "     AND   ORGCODE      = @P02 " _
             & "     AND   KIJUNDATE    = @P03 " _
             & "     AND   KOUEITYPE    = @P04 " _
             & "     AND   ORDERID      = @P05 " _
             & "     AND   TRIPSEQ      = @P06 ; " _
             & " IF ( @@FETCH_STATUS <> 0 ) " _
             & "    INSERT INTO W0001_KOUEIORDER " _
             & "          (CAMPCODE , " _
             & "           ORGCODE , " _
             & "           KIJUNDATE , " _
             & "           KOUEITYPE , " _
             & "           ORDERID , " _
             & "           TRIPSEQ , " _
             & "           ROWNO , " _
             & "           CSVDATA , " _
             & "           DELFLG , " _
             & "           INITYMD , " _
             & "           UPDYMD , " _
             & "           UPDUSER , " _
             & "           UPDTERMID , " _
             & "           RECEIVEYMD" _
             & "       ) " _
             & "        VALUES (@P01,@P02,@P03,@P04,@P05,@P06,@P07,@P08," _
             & "                @P09,@P10,@P11,@P12,@P13,@P14) ; " _
             & " CLOSE hensuu ; " _
             & " DEALLOCATE hensuu ; "

            Try

                If IsNothing(_dicOrder) Then
                    Me.ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
                    PutLog(C_MESSAGE_NO.DLL_IF_ERROR, C_MESSAGE_TYPE.ABORT)
                    Return False
                End If

                If SQLcon.State <> ConnectionState.Open Then
                    SQLcon.Open() 'DataBase接続(Open)
                End If

                Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.Date)
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar)
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar)
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar)
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.Int)
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.NVarChar)
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.NVarChar)
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.DateTime)
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar)
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar)
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.DateTime)
                Dim PARA000 As List(Of SqlParameter) = New List(Of SqlParameter)

                For Each row In _dicOrder.Values
                    PARA01.Value = Me.CAMPCODE
                    PARA02.Value = Me.ORGCODE
                    PARA03.Value = Date.ParseExact(row.KIJUNDATE, "yyyyMMdd", Nothing)
                    PARA04.Value = row.KOUEITYPE
                    PARA05.Value = row.ORDERID
                    PARA06.Value = row.TRIPSEQ
                    PARA07.Value = row.ROWNO
                    PARA08.Value = row.CSVDATA

                    PARA09.Value = C_DELETE_FLG.ALIVE
                    PARA10.Value = now
                    PARA11.Value = now
                    PARA12.Value = sm.USERID
                    PARA13.Value = sm.TERMID
                    PARA14.Value = C_DEFAULT_YMD

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                Next

                Return True

            Catch ex As Exception
                ' その他例外
                Me.ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
                PutLog(Me.ERR, C_MESSAGE_TYPE.ABORT, ex.Message)

                Return False
            Finally
                SQLcon.Close()
            End Try

        End Function

    End Class

    ''' <summary>
    '''  光英マスタ管理クラス
    ''' </summary>
    ''' <remarks></remarks>
    Public Class KOUEI_MASTER
        Inherits GRT00004COM

        ''' <summary>
        ''' 光英乗務員マスタ
        ''' </summary>
        Public Class KOUEI_STAFF

            ''' <summary>
            ''' 光英区分
            ''' </summary>
            Public KOUEITYPE As String

            ''' <summary>
            ''' 乗務員コード
            ''' </summary>
            Public STAFFCODE As String
            ''' <summary>
            ''' 乗務員番号
            ''' </summary>
            Public STAFFNO As String
            ''' <summary>
            ''' 乗務員名
            ''' </summary>
            Public STAFFNAME As String

            ''' <summary>
            ''' DictionaryKey作成
            ''' </summary>
            ''' <remarks></remarks>
            Public Function MakeDicKey() As String
                Dim sb As StringBuilder = New StringBuilder

                sb.Append(KOUEITYPE)
                sb.Append(C_VALUE_SPLIT_DELIMITER)
                sb.Append(STAFFCODE)
                Return sb.ToString
            End Function

        End Class
        ''' <summary>
        ''' 光英車両マスタ
        ''' </summary>
        Public Class KOUEI_SHARYO

            ''' <summary>
            ''' 光英区分
            ''' </summary>
            Public KOUEITYPE As String

            ''' <summary>
            ''' 車両コード
            ''' </summary>
            Public SHARYOCODE As String
            ''' <summary>
            ''' 車番
            ''' </summary>
            Public SHABAN As String
            ''' <summary>
            ''' 登録車番
            ''' </summary>
            Public REGISTERSHABAN As String
            ''' <summary>
            ''' 陸事車番
            ''' </summary>
            Public LICNPLTNO As String
            ''' <summary>
            ''' TRACTOR区分
            ''' </summary>
            Public TRACTORTYPE As String

            ''' <summary>
            ''' DictionaryKey作成
            ''' </summary>
            ''' <remarks></remarks>
            Public Function MakeDicKey() As String
                Dim sb As StringBuilder = New StringBuilder

                sb.Append(KOUEITYPE)
                sb.Append(C_VALUE_SPLIT_DELIMITER)
                sb.Append(SHABAN)
                Return sb.ToString
            End Function

        End Class
        ''' <summary>
        ''' 光英納入先マスタ
        ''' </summary>
        Public Class KOUEI_TODOKE

            ''' <summary>
            ''' 光英区分
            ''' </summary>
            Public KOUEITYPE As String

            ''' <summary>
            ''' 納入先番号
            ''' </summary>
            Public TODOKESAKICODE As String
            ''' <summary>
            ''' 納入先名
            ''' </summary>
            Public NAME As String
            ''' <summary>
            ''' WGS緯度
            ''' </summary>
            Public LATITUDE As String
            ''' <summary>
            ''' WGS経度
            ''' </summary>
            Public LONGTIDUDE As String

            ''' <summary>
            ''' DictionaryKey作成
            ''' </summary>
            ''' <remarks></remarks>
            Public Function MakeDicKey() As String
                Dim sb As StringBuilder = New StringBuilder

                sb.Append(KOUEITYPE)
                sb.Append(C_VALUE_SPLIT_DELIMITER)
                sb.Append(TODOKESAKICODE)
                Return sb.ToString
            End Function

            ''' <summary>
            ''' 届先マスタ追加用コード作成
            ''' </summary>
            ''' <remarks></remarks>
            Public Function GetDBEntryCode() As String
                Dim sb As StringBuilder = New StringBuilder

                Select Case KOUEITYPE
                    Case GRW0001KOUEIORDER.KOUEITYPE_PREFIX.JXTG,
                         GRW0001KOUEIORDER.KOUEITYPE_PREFIX.JX,
                         GRW0001KOUEIORDER.KOUEITYPE_PREFIX.TG
                        sb.Append(UCase(GRW0001KOUEIORDER.KOUEITYPE_PREFIX.JX))
                        sb.Append(TODOKESAKICODE)
                    Case GRW0001KOUEIORDER.KOUEITYPE_PREFIX.COSMO
                        sb.Append(UCase(GRW0001KOUEIORDER.KOUEITYPE_PREFIX.COSMO))
                        sb.Append(TODOKESAKICODE)
                    Case Else
                End Select
                Return sb.ToString
            End Function
        End Class
        ''' <summary>
        ''' 光英乗務員マスタ
        ''' </summary>
        Private _dicStaff As Dictionary(Of String, KOUEI_STAFF)
        ''' <summary>
        ''' 光英届先マスタ（）
        ''' </summary>
        Private _dicTodoke As Dictionary(Of String, KOUEI_TODOKE)
        ''' <summary>
        ''' 光英車両マスタ
        ''' </summary>
        Private _dicSharyo As Dictionary(Of String, KOUEI_SHARYO)

        ''' <summary>
        ''' [IN]ORGCODEプロパティ
        ''' </summary>
        ''' <returns>[IN]ORGCODE</returns>
        Public Property ORGCODE() As String

        ''' <summary>
        ''' 初期化
        ''' </summary>
        ''' <remarks></remarks> 
        Public Overloads Sub Initialize()
            MyBase.Initialize()

            ORGCODE = String.Empty

            If Not IsNothing(_dicTodoke) Then
                _dicTodoke.Clear()
                _dicTodoke = Nothing
            End If
            If Not IsNothing(_dicSharyo) Then
                _dicSharyo.Clear()
                _dicSharyo = Nothing
            End If
            If Not IsNothing(_dicStaff) Then
                _dicStaff.Clear()
                _dicStaff = Nothing
            End If
        End Sub
        ''' <summary>
        ''' 解放処理
        ''' </summary>
        Public Overrides Sub Dispose()
            If Not IsNothing(_dicTodoke) Then
                _dicTodoke.Clear()
                _dicTodoke = Nothing
            End If
            If Not IsNothing(_dicSharyo) Then
                _dicSharyo.Clear()
                _dicSharyo = Nothing
            End If
            If Not IsNothing(_dicStaff) Then
                _dicStaff.Clear()
                _dicStaff = Nothing
            End If
            MyBase.Dispose()
        End Sub

        ''' <summary>
        ''' 光英タイプ取得（マスタ）
        ''' </summary>
        ''' <param name="koueiType">光英タイプ</param>
        ''' <returns>光英タイプ（マスタ）</returns>
        Private Function _getMasterKOUEITYPE(ByVal koueiType As String) As String
            If koueiType = GRW0001KOUEIORDER.KOUEITYPE_PREFIX.JX OrElse
               koueiType = GRW0001KOUEIORDER.KOUEITYPE_PREFIX.TG Then
                Return GRW0001KOUEIORDER.KOUEITYPE_PREFIX.JXTG
            Else
                Return koueiType
            End If
        End Function

        ''' <summary>
        ''' 光英乗務員取得（乗務員コード）
        ''' </summary>
        ''' <param name="koueiType">光英タイプ</param>
        ''' <param name="staffCode">乗務員コード</param>
        ''' <returns>光英乗務員</returns>
        Public Function GetStaff2Code(ByVal koueiType As String, ByVal staffCode As String) As KOUEI_STAFF
            Dim koueiTypeMst As String = _getMasterKOUEITYPE(koueiType)

            Dim key As String = koueiTypeMst & C_VALUE_SPLIT_DELIMITER & staffCode
            If _dicStaff.ContainsKey(key) Then
                Return _dicStaff.Item(key)
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' 光英乗務員取得（乗務員番号）
        ''' </summary>
        ''' <param name="koueiType">光英タイプ</param>
        ''' <param name="staffNo">乗務員番号</param>
        ''' <returns>光英乗務員</returns>
        ''' <remarks >複数存在時は先頭</remarks>
        Public Function GetStaff2No(ByVal koueiType As String, ByVal staffNo As String) As KOUEI_STAFF
            Dim koueiTypeMst As String = _getMasterKOUEITYPE(koueiType)

            Dim staff = From dic In _dicStaff
                        Select dic.Value
                        Where Value.KOUEITYPE = koueiTypeMst And Value.STAFFNO = staffNo
            If staff.Count > 0 Then
                Return staff.First
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' 光英届先取得（届先コード）
        ''' </summary>
        ''' <param name="koueiType">光英タイプ</param>
        ''' <param name="todokeCode">届先コード</param>
        ''' <returns>光英届先</returns>
        Public Function GetTodoke(ByVal koueiType As String, ByVal todokeCode As String) As KOUEI_TODOKE
            Dim koueiTypeMst As String = _getMasterKOUEITYPE(koueiType)

            Dim key As String = koueiTypeMst & C_VALUE_SPLIT_DELIMITER & todokeCode
            If _dicTodoke.ContainsKey(key) Then
                Return _dicTodoke.Item(key)
            Else
                Return Nothing
            End If
        End Function
        ''' <summary>
        ''' 光英車両取得（車番）
        ''' </summary>
        ''' <param name="koueiType">光英タイプ</param>
        ''' <param name="shaban">車両コード</param>
        ''' <returns>光英車両</returns>
        Public Function GetSharyo(ByVal koueiType As String, ByVal shaban As String) As KOUEI_SHARYO
            Dim koueiTypeMst As String = _getMasterKOUEITYPE(koueiType)

            Dim key As String = koueiTypeMst & C_VALUE_SPLIT_DELIMITER & shaban
            If _dicSharyo.ContainsKey(key) Then
                Return _dicSharyo.Item(key)
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' 光英マスタデータ一括読込  
        ''' </summary>
        ''' <remarks></remarks>
        Public Function ReadMasterData() As Boolean

            ERR = C_MESSAGE_NO.NORMAL

            If String.IsNullOrEmpty(ORGCODE) Then
                ERR = C_MESSAGE_NO.DLL_IF_ERROR
                PutLog(C_MESSAGE_NO.DLL_IF_ERROR, C_MESSAGE_TYPE.ABORT, "未設定:ORGCODE")
                Return False
            End If

            '光英コード届先読込
            If ReadTodoke() = False Then
                Return False
            End If
            '光英コード車両読込
            If ReadSharyo() = False Then
                Return False
            End If
            '光英コード従業員読込
            If ReadStaff() = False Then
                Return False
            End If

            Return True

        End Function

        ''' <summary>
        ''' 光英届先マスタ読み込み
        ''' </summary>
        ''' <returns>TRUE|FALSE</returns>
        ''' <remarks> OK:00000</remarks> 
        Public Function ReadTodoke() As Boolean
            Me.ERR = C_MESSAGE_NO.NORMAL

            If IsNothing(_dicTodoke) Then
                _dicTodoke = New Dictionary(Of String, KOUEI_TODOKE)
            Else
                _dicTodoke.Clear()
            End If

            'SQL
            Dim sb As StringBuilder = New StringBuilder
            sb.Append("SELECT ")
            sb.Append("    KOUEITYPE ")
            sb.Append("  , TODOKESAKICODE ")
            sb.Append("  , NAME ")
            sb.Append("  , LATITUDE ")
            sb.Append("  , LONGITUDE ")
            sb.Append("FROM ")
            sb.Append("  W0002_KOUEITODOKESAKI ")

            Try
                If SQLcon.State <> ConnectionState.Open Then
                    SQLcon.Open() 'DataBase接続(Open)
                End If

                Dim SQLcmd As New SqlCommand(sb.ToString, SQLcon)

                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                'フィールド名とフィールドの型を取得
                While SQLdr.Read()
                    Dim todoke = New KOUEI_TODOKE With {
                        .KOUEITYPE = SQLdr.Item("KOUEITYPE"),
                        .TODOKESAKICODE = SQLdr.Item("TODOKESAKICODE"),
                        .NAME = SQLdr.Item("NAME"),
                        .LATITUDE = SQLdr.Item("LATITUDE"),
                        .LONGTIDUDE = SQLdr.Item("LONGITUDE")
                    }

                    _dicTodoke.Add(todoke.MakeDicKey, todoke)
                End While

                Return True

            Catch ex As Exception
                ' その他例外

                Me.ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
                PutLog(Me.ERR, C_MESSAGE_TYPE.ABORT, ex.Message)

                Return False
            Finally
                SQLcon.Close()
            End Try

        End Function

        ''' <summary>
        ''' 光英車両マスタ読み込み
        ''' </summary>
        ''' <returns>TRUE|FALSE</returns>
        ''' <remarks> OK:00000</remarks> 
        Public Function ReadSharyo() As Boolean
            Me.ERR = C_MESSAGE_NO.NORMAL

            'SQL
            Dim sb As StringBuilder = New StringBuilder
            sb.Append("SELECT ")
            sb.Append("    KOUEITYPE ")
            sb.Append("  , SHARYOCODE ")
            sb.Append("  , SHABAN ")
            sb.Append("  , REGISTERSHABAN ")
            sb.Append("  , LICNPLTNO ")
            sb.Append("  , TRACTORTYPE ")
            sb.Append("FROM ")
            sb.Append("  W0003_KOUEISHARYO ")
            sb.Append("WHERE ")
            sb.Append("  ORGCODE = @ORGCODE ")

            Try
                If IsNothing(_dicSharyo) Then
                    _dicSharyo = New Dictionary(Of String, KOUEI_SHARYO)
                Else
                    _dicSharyo.Clear()
                End If

                If SQLcon.State <> ConnectionState.Open Then
                    SQLcon.Open() 'DataBase接続(Open)
                End If

                Dim SQLcmd As New SqlCommand(sb.ToString, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@ORGCODE", System.Data.SqlDbType.NVarChar)
                PARA01.Value = Me.ORGCODE

                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                'フィールド名とフィールドの型を取得
                While SQLdr.Read()
                    Dim sharyo = New KOUEI_SHARYO With {
                        .KOUEITYPE = SQLdr.Item("KOUEITYPE"),
                        .SHARYOCODE = SQLdr.Item("SHARYOCODE"),
                        .SHABAN = SQLdr.Item("SHABAN"),
                        .REGISTERSHABAN = SQLdr.Item("REGISTERSHABAN"),
                        .LICNPLTNO = SQLdr.Item("LICNPLTNO"),
                        .TRACTORTYPE = SQLdr.Item("TRACTORTYPE")
                    }

                    _dicSharyo.Add(sharyo.MakeDicKey, sharyo)
                End While

                Return True

            Catch ex As Exception
                ' その他例外

                Me.ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
                PutLog(Me.ERR, C_MESSAGE_TYPE.ABORT, ex.Message)

                Return False
            Finally
                SQLcon.Close()
            End Try

        End Function

        ''' <summary>
        ''' 光英乗務員マスタ読み込み
        ''' </summary>
        ''' <returns>TRUE|FALSE</returns>
        ''' <remarks> OK:00000</remarks> 
        Public Function ReadStaff() As Boolean
            Me.ERR = C_MESSAGE_NO.NORMAL

            'SQL
            Dim sb As StringBuilder = New StringBuilder
            sb.Append("SELECT ")
            sb.Append("    KOUEITYPE ")
            sb.Append("  , STAFFCODE ")
            sb.Append("  , STAFFNO ")
            sb.Append("  , STAFFNAME ")
            sb.Append("FROM ")
            sb.Append("  W0004_KOUEISTAFF ")
            sb.Append("WHERE ")
            sb.Append("  ORGCODE = @ORGCODE ")

            Try
                If IsNothing(_dicStaff) Then
                    _dicStaff = New Dictionary(Of String, KOUEI_STAFF)
                Else
                    _dicStaff.Clear()
                End If

                If SQLcon.State <> ConnectionState.Open Then
                    SQLcon.Open() 'DataBase接続(Open)
                End If

                Dim SQLcmd As New SqlCommand(sb.ToString, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@ORGCODE", System.Data.SqlDbType.NVarChar)
                PARA01.Value = Me.ORGCODE

                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                'フィールド名とフィールドの型を取得
                While SQLdr.Read()
                    Dim staff = New KOUEI_STAFF With {
                        .KOUEITYPE = SQLdr.Item("KOUEITYPE"),
                        .STAFFCODE = SQLdr.Item("STAFFCODE"),
                        .STAFFNO = SQLdr.Item("STAFFNO"),
                        .STAFFNAME = SQLdr.Item("STAFFNAME")
                    }

                    _dicStaff.Add(staff.MakeDicKey, staff)
                End While

                Return True

            Catch ex As Exception
                ' その他例外

                Me.ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
                PutLog(Me.ERR, C_MESSAGE_TYPE.ABORT, ex.Message)

                Return False
            Finally
                SQLcon.Close()
            End Try

        End Function

    End Class

#End Region

#Region "<< JSRコード関連 >>"

    ''' <summary>
    ''' JSRマスタ管理クラス   
    ''' </summary>
    ''' <remarks></remarks>
    Public Class JSRCODE_MASTER
        Inherits GRT00004COM

        ''' <summary>
        ''' グループ作業判定名称（NOTES1：特定要件１）
        ''' </summary>
        Private Const C_GROUPWORK_NOTES As String = "グループ"

        ''' <summary>
        ''' JSR変換コード（届先）
        ''' </summary>
        Public Class JSRCODE_TODOKE
            ''' <summary>
            ''' JSR届先コード
            ''' </summary>
            Public JSRTODOKECODE As String

            ''' <summary>
            ''' 取引先コード
            ''' </summary>
            Public TORICODE As String
            ''' <summary>
            ''' 届先コード
            ''' </summary>
            Public TODOKECODE As String
            ''' <summary>
            ''' 出荷場所（届先コード：出荷場）
            ''' </summary>
            Public SHUKABASHO As String
            ''' <summary>
            ''' 特定要件１（グループ作業）
            ''' </summary>
            Public NOTES1 As String
            ''' <summary>
            ''' グループ作業判定
            ''' </summary>
            ''' <remarks>特定要件１"グループ"(かな・半角・全角)</remarks>
            ReadOnly Property IsGroupWork As Boolean
                Get
                    If Not String.IsNullOrEmpty(NOTES1) AndAlso
                        StrConv(Trim(NOTES1), VbStrConv.Katakana Or VbStrConv.Wide) = C_GROUPWORK_NOTES Then
                        Return True
                    Else
                        Return False
                    End If
                End Get
            End Property

        End Class
        ''' <summary>
        ''' JSR変換コード（品名）
        ''' </summary>
        Public Class JSRCODE_PRODUCT
            ''' <summary>
            ''' JSR品名コード
            ''' </summary>
            Public JSRPRODUCT As String

            ''' <summary>
            ''' 油種
            ''' </summary>
            Public OILTYPE As String
            ''' <summary>
            ''' 品名１
            ''' </summary>
            Public PRODUCT1 As String
            ''' <summary>
            ''' 品名２
            ''' </summary>
            Public PRODUCT2 As String
            ''' <summary>
            ''' 品名コード
            ''' </summary>
            Public PRODUCTCODE As String
        End Class
        ''' <summary>
        ''' JSR変換コード（車両）
        ''' </summary>
        Public Class JSRCODE_SHABAN
            ''' <summary>
            ''' JSR車番
            ''' </summary>
            Public JSRSHABAN As String

            ''' <summary>
            ''' 業務車番
            ''' </summary>
            Public GSHABAN As String
            ''' <summary>
            ''' 車輛タイプ（前）
            ''' </summary>
            Public SHARYOTYPEF As String
            ''' <summary>
            ''' 統一車番（前）
            ''' </summary>
            Public TSHABANF As String
            ''' <summary>
            ''' 車輛タイプ（後）
            ''' </summary>
            Public SHARYOTYPEB As String
            ''' <summary>
            ''' 統一車番（後）
            ''' </summary>
            Public TSHABANB As String
            ''' <summary>
            ''' 車輛タイプ２（後）
            ''' </summary>
            Public SHARYOTYPEB2 As String
            ''' <summary>
            ''' 統一車番２（後）
            ''' </summary>
            Public TSHABANB2 As String
        End Class
        ''' <summary>
        ''' JSR変換コード（従業員）
        ''' </summary>
        Public Class JSRCODE_STAFF
            ''' <summary>
            ''' JSR乗務員コード
            ''' </summary>
            Public JSRSTAFFCODE As String

            ''' <summary>
            ''' 乗務員コード
            ''' </summary>
            Public STAFFCODE As String
        End Class
        ''' <summary>
        ''' JSR届先マスタ
        ''' </summary>
        Private _dicTodoke As Dictionary(Of String, JSRCODE_TODOKE)
        ''' <summary>
        ''' JSR品名マスタ
        ''' </summary>
        Private _dicProduct As Dictionary(Of String, JSRCODE_PRODUCT)
        ''' <summary>
        ''' JSR車両マスタ
        ''' </summary>
        Private _dicShaban As Dictionary(Of String, JSRCODE_SHABAN)
        ''' <summary>
        ''' JSR乗務員マスタ
        ''' </summary>
        Private _dicStaff As Dictionary(Of String, JSRCODE_STAFF)

        ''' <summary>
        ''' 会社コード   
        ''' </summary>
        ''' <remarks></remarks>
        Public Property CAMPCODE As String
        ''' <summary>
        ''' [IN]ORGCODEプロパティ
        ''' </summary>
        ''' <returns>[IN]ORGCODE</returns>
        Public Property ORGCODE() As String

        ''' <summary>
        ''' 初期化
        ''' </summary>
        ''' <remarks></remarks> 
        Public Overloads Sub Initialize()
            MyBase.Initialize()

            CAMPCODE = String.Empty
            ORGCODE = String.Empty

            If Not IsNothing(_dicTodoke) Then
                _dicTodoke.Clear()
                _dicTodoke = Nothing
            End If
            If Not IsNothing(_dicProduct) Then
                _dicProduct.Clear()
                _dicProduct = Nothing
            End If
            If Not IsNothing(_dicShaban) Then
                _dicShaban.Clear()
                _dicShaban = Nothing
            End If
            If Not IsNothing(_dicStaff) Then
                _dicStaff.Clear()
                _dicStaff = Nothing
            End If
        End Sub
        ''' <summary>
        ''' 解放処理
        ''' </summary>
        Public Overrides Sub Dispose()
            If Not IsNothing(_dicTodoke) Then
                _dicTodoke.Clear()
                _dicTodoke = Nothing
            End If
            If Not IsNothing(_dicProduct) Then
                _dicProduct.Clear()
                _dicProduct = Nothing
            End If
            If Not IsNothing(_dicShaban) Then
                _dicShaban.Clear()
                _dicShaban = Nothing
            End If
            If Not IsNothing(_dicStaff) Then
                _dicStaff.Clear()
                _dicStaff = Nothing
            End If
            MyBase.Dispose()
        End Sub
        ''' <summary>
        ''' JSRコードデータ一括読込  
        ''' </summary>
        ''' <remarks></remarks>
        Public Function ReadJSRData() As Boolean

            ERR = C_MESSAGE_NO.NORMAL

            If String.IsNullOrEmpty(ORGCODE) Then
                PutLog(C_MESSAGE_NO.DLL_IF_ERROR, C_MESSAGE_TYPE.ABORT, "未設定:ORGCODE")
                Return False
            End If

            If IsNothing(_dicTodoke) Then
                _dicTodoke = New Dictionary(Of String, JSRCODE_TODOKE)
            Else
                _dicTodoke.Clear()
            End If
            If IsNothing(_dicProduct) Then
                _dicProduct = New Dictionary(Of String, JSRCODE_PRODUCT)
            Else
                _dicProduct.Clear()
            End If
            If IsNothing(_dicShaban) Then
                _dicShaban = New Dictionary(Of String, JSRCODE_SHABAN)
            Else
                _dicShaban.Clear()
            End If
            If IsNothing(_dicStaff) Then
                _dicStaff = New Dictionary(Of String, JSRCODE_STAFF)
            Else
                _dicStaff.Clear()
            End If

            'JSR変換コード届先読込
            If ReadTodoke() = False Then
                Return False
            End If
            'JSR変換コード品名読込
            If ReadProduct() = False Then
                Return False
            End If
            'JSR変換コード車両読込
            If ReadShaban() = False Then
                Return False
            End If
            'JSR変換コード従業員読込
            If ReadStaff() = False Then
                Return False
            End If

            Return True

        End Function

        ''' <summary>
        ''' JSR変換コード届先取得   
        ''' </summary>
        ''' <param name="I_JSRCODE">JSRコード</param>
        ''' <returns >変換コード</returns>
        ''' <remarks></remarks>
        Public Function GetTodokeCode(ByVal I_JSRCODE As String) As JSRCODE_TODOKE
            Dim wkValue As JSRCODE_TODOKE = New JSRCODE_TODOKE
            CovertTodokeCode(I_JSRCODE, wkValue)
            Return wkValue
        End Function
        ''' <summary>
        ''' JSR変換コード品名取得   
        ''' </summary>
        ''' <param name="I_JSRCODE">JSRコード</param>
        ''' <returns >変換コード</returns>
        ''' <remarks></remarks>
        Public Function GetProductCode(ByVal I_JSRCODE As String) As JSRCODE_PRODUCT
            Dim wkValue As JSRCODE_PRODUCT = New JSRCODE_PRODUCT
            CovertProductCode(I_JSRCODE, wkValue)
            Return wkValue
        End Function
        ''' <summary>
        ''' JSR変換コード車番取得   
        ''' </summary>
        ''' <param name="I_JSRCODE">JSRコード</param>
        ''' <returns >変換コード</returns>
        ''' <remarks></remarks>
        Public Function GetShabanCode(ByVal I_JSRCODE As String) As JSRCODE_SHABAN
            Dim wkValue As JSRCODE_SHABAN = New JSRCODE_SHABAN
            CovertShabanCode(I_JSRCODE, wkValue)
            Return wkValue
        End Function
        ''' <summary>
        ''' JSR変換コード従業員取得   
        ''' </summary>
        ''' <param name="I_JSRCODE">JSRコード</param>
        ''' <returns >変換コード</returns>
        ''' <remarks></remarks>
        Public Function GetStaffCode(ByVal I_JSRCODE As String) As JSRCODE_STAFF
            Dim wkValue As JSRCODE_STAFF = New JSRCODE_STAFF
            CovertStaffCode(I_JSRCODE, wkValue)
            Return wkValue
        End Function

        ''' <summary>
        ''' JSR変換コード届先取得   
        ''' </summary>
        ''' <param name="I_JSRCODE">JSRコード</param>
        ''' <param name="O_CODEOBJ" >変換コード</param>
        ''' <remarks></remarks>
        Public Function CovertTodokeCode(ByVal I_JSRCODE As String, ByRef O_CODEOBJ As JSRCODE_TODOKE) As Boolean
            Dim rtn As Boolean = True
            ERR = C_MESSAGE_NO.NORMAL

            If _dicTodoke.Count = 0 Then
                'JSR変換コード届先格納
                If ReadTodoke(I_JSRCODE) = False Then
                    Return False
                End If
            End If

            'DictionaryKey作成
            ' 部署|JSR届先コード
            Dim wkKey = MakeDicKey(I_JSRCODE)

            'Dictionary存在チェック
            Dim wkValue = New JSRCODE_TODOKE
            If _dicTodoke.TryGetValue(wkKey, wkValue) Then
                O_CODEOBJ = wkValue
            Else
                ERR = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
                rtn = False
            End If

            Return rtn

        End Function
        ''' <summary>
        ''' JSR変換コード品名取得   
        ''' </summary>
        ''' <param name="I_JSRCODE">JSRコード</param>
        ''' <param name="O_CODEOBJ" >変換コード</param>
        ''' <remarks></remarks>
        Public Function CovertProductCode(ByVal I_JSRCODE As String, ByRef O_CODEOBJ As JSRCODE_PRODUCT) As Boolean
            Dim rtn As Boolean = True
            ERR = C_MESSAGE_NO.NORMAL

            If _dicProduct.Count = 0 Then
                'JSR変換コード品名格納
                If ReadProduct(I_JSRCODE) = False Then
                    Return False
                End If
            End If

            'DictionaryKey作成
            ' 部署|JSRコード
            Dim wkKey = MakeDicKey(I_JSRCODE)

            'Dictionary存在チェック
            Dim wkValue = New JSRCODE_PRODUCT
            If _dicProduct.TryGetValue(wkKey, wkValue) Then
                O_CODEOBJ = wkValue
            Else
                ERR = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
                rtn = False
            End If

            Return rtn

        End Function
        ''' <summary>
        ''' JSR変換コード車番取得   
        ''' </summary>
        ''' <param name="I_JSRCODE">JSRコード</param>
        ''' <param name="O_CODEOBJ" >変換コード</param>
        ''' <remarks></remarks>
        Public Function CovertShabanCode(ByVal I_JSRCODE As String, ByRef O_CODEOBJ As JSRCODE_SHABAN) As Boolean
            Dim rtn As Boolean = True
            ERR = C_MESSAGE_NO.NORMAL

            If _dicShaban.Count = 0 Then
                'JSR変換コード車番格納
                If ReadShaban(I_JSRCODE) = False Then
                    Return False
                End If
            End If

            'DictionaryKey作成
            ' 部署|JSRコード
            Dim wkKey = MakeDicKey(I_JSRCODE)

            'Dictionary存在チェック
            Dim wkValue = New JSRCODE_SHABAN
            If _dicShaban.TryGetValue(wkKey, wkValue) Then
                O_CODEOBJ = wkValue
            Else
                ERR = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
                rtn = False
            End If

            Return rtn

        End Function
        ''' <summary>
        ''' JSR変換コード従業員取得   
        ''' </summary>
        ''' <param name="I_JSRCODE">JSR従業員コード</param>
        ''' <param name="O_CODEOBJ" >JSR変換コード</param>
        ''' <remarks></remarks>
        Public Function CovertStaffCode(ByVal I_JSRCODE As String, ByRef O_CODEOBJ As JSRCODE_STAFF) As Boolean
            Dim rtn As Boolean = True
            ERR = C_MESSAGE_NO.NORMAL

            If _dicStaff.Count = 0 Then
                'JSR変換コード従業員格納
                If ReadStaff(I_JSRCODE) = False Then
                    Return False
                End If
            End If

            'DictionaryKey作成
            ' 部署|JSRコード
            Dim wkKey = MakeDicKey(I_JSRCODE)

            'Dictionary存在チェック
            Dim wkValue = New JSRCODE_STAFF
            If _dicStaff.TryGetValue(wkKey, wkValue) Then
                O_CODEOBJ = wkValue
            Else
                ERR = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
                rtn = False
            End If

            Return rtn

        End Function

        ''' <summary>
        ''' JSR変換コード届先読込
        ''' </summary>
        ''' <param name="I_JSRCODE" >未指定時は部署内全部</param>
        ''' <remarks></remarks>
        Private Function ReadTodoke(Optional ByVal I_JSRCODE As String = "") As Boolean
            Dim rtn As Boolean = True
            ERR = C_MESSAGE_NO.NORMAL

            '初回アクセス時Dictionary作成
            If IsNothing(_dicTodoke) Then
                _dicTodoke = New Dictionary(Of String, JSRCODE_TODOKE)
            End If
            If String.IsNullOrEmpty(I_JSRCODE) Then
                _dicTodoke.Clear()
            End If

            'SQL
            Dim sb As StringBuilder = New StringBuilder()
            sb.Append("SELECT ")
            sb.Append("  rtrim(A.JSRTODOKECODE) as JSRTODOKECODE ")
            sb.Append("  , rtrim(A.TORICODE)    as TORICODE ")
            sb.Append("  , rtrim(A.TODOKECODE)  as TODOKECODE ")
            sb.Append("  , rtrim(A.SHUKABASHO)  as SHUKABASHO ")
            sb.Append("  , rtrim(B.NOTES1)      as NOTES1 ")
            sb.Append("FROM ")
            sb.Append("  MC007_TODKORG as A ")
            sb.Append("  INNER JOIN MC006_TODOKESAKI as B ")
            sb.Append("     ON B.CAMPCODE = A.CAMPCODE ")
            sb.Append("    and B.TORICODE = A.TORICODE ")
            sb.Append("    and B.TODOKECODE = A.TODOKECODE ")
            sb.Append("    and B.STYMD <= @P1 ")
            sb.Append("    and B.ENDYMD >= @P1 ")
            sb.Append("    and B.DELFLG <> '1' ")
            sb.Append("Where ")
            sb.Append("  A.CAMPCODE = @P2 ")
            sb.Append("  and A.UORG = @P3 ")
            sb.Append("  and A.DELFLG <> '1' ")
            If Not String.IsNullOrEmpty(I_JSRCODE) Then
                sb.Append("  and A.JSRTODOKECODE = @P4 ")
            End If

            Try
                If SQLcon.State <> ConnectionState.Open Then
                    SQLcon.Open() 'DataBase接続(Open)
                End If

                Using SQLcmd As New SqlCommand(sb.ToString, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar)
                    PARA1.Value = Date.Now
                    PARA2.Value = Me.CAMPCODE
                    PARA3.Value = Me.ORGCODE
                    PARA4.Value = I_JSRCODE

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        While SQLdr.Read
                            Dim wkValue = New JSRCODE_TODOKE With {
                                .JSRTODOKECODE = SQLdr("JSRTODOKECODE").ToString,
                                .TORICODE = SQLdr("TORICODE").ToString,
                                .TODOKECODE = SQLdr("TODOKECODE").ToString,
                                .SHUKABASHO = SQLdr("SHUKABASHO").ToString,
                                .NOTES1 = SQLdr("NOTES1").ToString
                            }
                            If String.IsNullOrEmpty(wkValue.JSRTODOKECODE) Then
                                Continue While
                            End If
                            'DictionaryKey作成
                            ' 部署|JSRコード
                            Dim wkKey = MakeDicKey(wkValue.JSRTODOKECODE)
                            '複数呼出OK
                            _dicTodoke(wkKey) = wkValue
                            '_dicTodoke.Add(wkKey, wkValue)
                        End While
                    End Using
                End Using

            Catch ex As Exception
                ERR = C_MESSAGE_NO.DB_ERROR
                rtn = False
            End Try

            Return rtn

        End Function
        ''' <summary>
        ''' JSR変換コード品名読込
        ''' </summary>
        ''' <param name="I_JSRCODE" >未指定時は部署内全部</param>
        ''' <remarks></remarks>
        Private Function ReadProduct(Optional ByVal I_JSRCODE As String = "") As Boolean
            Dim rtn As Boolean = True
            ERR = C_MESSAGE_NO.NORMAL

            '初回アクセス時Dictionary作成
            If IsNothing(_dicProduct) Then
                _dicProduct = New Dictionary(Of String, JSRCODE_PRODUCT)
            End If
            If String.IsNullOrEmpty(I_JSRCODE) Then
                _dicProduct.Clear()
            End If

            'SQL
            Dim sb As StringBuilder = New StringBuilder()
            sb.Append("SELECT ")
            sb.Append("  rtrim(A.JSRPRODUCT) as JSRPRODUCT ")
            sb.Append("  , rtrim(A.PRODUCTCODE) as PRODUCTCODE ")
            sb.Append("  , rtrim(B.OILTYPE) as OILTYPE ")
            sb.Append("  , rtrim(B.PRODUCT1) as PRODUCT1 ")
            sb.Append("  , rtrim(B.PRODUCT2) as PRODUCT2 ")
            sb.Append("FROM ")
            sb.Append("  MD002_PRODORG as A ")
            sb.Append("  INNER JOIN MD001_PRODUCT as B ")
            sb.Append("    ON B.CAMPCODE = A.CAMPCODE ")
            sb.Append("    and B.PRODUCTCODE = A.PRODUCTCODE ")
            sb.Append("    and B.STYMD <= @P1 ")
            sb.Append("    and B.ENDYMD >= @P1 ")
            sb.Append("    and B.DELFLG <> '1' ")
            sb.Append("Where ")
            sb.Append("  A.CAMPCODE = @P2 ")
            sb.Append("  and A.UORG = @P3 ")
            sb.Append("  and A.DELFLG <> '1' ")
            If Not String.IsNullOrEmpty(I_JSRCODE) Then
                sb.Append("  and A.JSRPRODUCT = @P4 ")
            End If

            Try
                If SQLcon.State <> ConnectionState.Open Then
                    SQLcon.Open() 'DataBase接続(Open)
                End If

                Using SQLcmd As New SqlCommand(sb.ToString, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar)
                    PARA1.Value = Date.Now
                    PARA2.Value = Me.CAMPCODE
                    PARA3.Value = Me.ORGCODE
                    PARA4.Value = I_JSRCODE

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        While SQLdr.Read
                            Dim wkValue = New JSRCODE_PRODUCT With {
                                .JSRPRODUCT = SQLdr("JSRPRODUCT").ToString,
                                .OILTYPE = SQLdr("OILTYPE").ToString,
                                .PRODUCT1 = SQLdr("PRODUCT1").ToString,
                                .PRODUCT2 = SQLdr("PRODUCT2").ToString,
                                .PRODUCTCODE = SQLdr("PRODUCTCODE").ToString
                            }
                            If String.IsNullOrEmpty(wkValue.JSRPRODUCT) Then
                                Continue While
                            End If
                            'DictionaryKey作成
                            ' 部署|JSRコード
                            Dim wkKey = MakeDicKey(wkValue.JSRPRODUCT)
                            '複数呼出OK
                            _dicProduct(wkKey) = wkValue
                            '_dicProduct.Add(wkKey, wkValue)
                        End While
                    End Using
                End Using

            Catch ex As Exception
                ERR = C_MESSAGE_NO.DB_ERROR
                rtn = False
            End Try

            Return rtn

        End Function
        ''' <summary>
        ''' JSR変換コード車番読込
        ''' </summary>
        ''' <param name="I_JSRCODE" >未指定時は部署内全部</param>
        ''' <remarks></remarks>
        Private Function ReadShaban(Optional ByVal I_JSRCODE As String = "") As Boolean
            Dim rtn As Boolean = True
            ERR = C_MESSAGE_NO.NORMAL

            '初回アクセス時Dictionary作成
            If IsNothing(_dicShaban) Then
                _dicShaban = New Dictionary(Of String, JSRCODE_SHABAN)
            End If
            If String.IsNullOrEmpty(I_JSRCODE) Then
                _dicShaban.Clear()
            End If

            'SQL
            Dim sb As StringBuilder = New StringBuilder()
            sb.Append("SELECT ")
            sb.Append("  rtrim(A.JSRSHABAN) as JSRSHABAN ")
            sb.Append("  , rtrim(A.GSHABAN) as GSHABAN ")
            sb.Append("  , rtrim(A.SHARYOTYPEF) as SHARYOTYPEF ")
            sb.Append("  , rtrim(A.TSHABANF) as TSHABANF ")
            sb.Append("  , rtrim(A.TSHABANFNAMES) as TSHABANFNAMES ")
            sb.Append("  , rtrim(A.SHARYOTYPEB) as SHARYOTYPEB ")
            sb.Append("  , rtrim(A.TSHABANB) as TSHABANB ")
            sb.Append("  , rtrim(A.TSHABANBNAMES) as TSHABANBNAMES ")
            sb.Append("  , rtrim(A.SHARYOTYPEB2) as SHARYOTYPEB2 ")
            sb.Append("  , rtrim(A.TSHABANB2) as TSHABANB2 ")
            sb.Append("  , rtrim(A.TSHABANB2NAMES) as TSHABANB2NAMES ")
            sb.Append("FROM ")
            sb.Append("  MA006_SHABANORG as A ")
            sb.Append("  INNER JOIN MA002_SHARYOA as B ")
            sb.Append("    ON B.CAMPCODE = A.CAMPCODE ")
            sb.Append("    and B.SHARYOTYPE = A.SHARYOTYPEF ")
            sb.Append("    and B.TSHABAN = A.TSHABANF ")
            sb.Append("    and B.STYMD <= @P1 ")
            sb.Append("    and B.ENDYMD >= @P1 ")
            sb.Append("    and B.DELFLG <> '1' ")
            sb.Append("  INNER JOIN MA002_SHARYOA as C ")
            sb.Append("    ON C.CAMPCODE = A.CAMPCODE ")
            sb.Append("    and C.SHARYOTYPE = A.SHARYOTYPEF ")
            sb.Append("    and C.TSHABAN = A.TSHABANF ")
            sb.Append("    and C.STYMD <= @P1 ")
            sb.Append("    and C.ENDYMD >= @P1 ")
            sb.Append("    and C.DELFLG <> '1' ")
            sb.Append("Where ")
            sb.Append("  A.CAMPCODE = @P2 ")
            sb.Append("  and A.MANGUORG = @P3 ")
            sb.Append("  and A.DELFLG <> '1' ")
            If Not String.IsNullOrEmpty(I_JSRCODE) Then
                sb.Append("  and A.JSRSHABAN = @P4 ")
            End If

            Try
                If SQLcon.State <> ConnectionState.Open Then
                    SQLcon.Open() 'DataBase接続(Open)
                End If

                Using SQLcmd As New SqlCommand(sb.ToString, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar)
                    PARA1.Value = Date.Now
                    PARA2.Value = Me.CAMPCODE
                    PARA3.Value = Me.ORGCODE
                    PARA4.Value = I_JSRCODE

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        While SQLdr.Read
                            Dim wkValue = New JSRCODE_SHABAN With {
                                .JSRSHABAN = SQLdr("JSRSHABAN").ToString,
                                .GSHABAN = SQLdr("GSHABAN").ToString,
                                .SHARYOTYPEF = SQLdr("SHARYOTYPEF").ToString,
                                .TSHABANF = SQLdr("TSHABANF").ToString,
                                .SHARYOTYPEB = SQLdr("SHARYOTYPEB").ToString,
                                .TSHABANB = SQLdr("TSHABANB").ToString,
                                .SHARYOTYPEB2 = SQLdr("SHARYOTYPEB2").ToString,
                                .TSHABANB2 = SQLdr("TSHABANB2").ToString
                            }
                            If String.IsNullOrEmpty(wkValue.JSRSHABAN) Then
                                Continue While
                            End If
                            'DictionaryKey作成
                            ' 部署|JSRコード
                            Dim wkKey = MakeDicKey(wkValue.JSRSHABAN)
                            '複数呼出OK
                            _dicShaban(wkKey) = wkValue
                            '_dicShaban.Add(wkKey, wkValue)
                        End While
                    End Using
                End Using

            Catch ex As Exception
                ERR = C_MESSAGE_NO.DB_ERROR
                rtn = False
            End Try

            Return rtn

        End Function
        ''' <summary>
        ''' JSR変換コード従業員読込
        ''' </summary>
        ''' <param name="I_JSRCODE" >未指定時は部署内全部</param>
        ''' <remarks></remarks>
        Private Function ReadStaff(Optional ByVal I_JSRCODE As String = "") As Boolean
            Dim rtn As Boolean = True
            ERR = C_MESSAGE_NO.NORMAL

            If IsNothing(_dicStaff) Then
                _dicStaff = New Dictionary(Of String, JSRCODE_STAFF)
            End If
            If String.IsNullOrEmpty(I_JSRCODE) Then
                _dicStaff.Clear()
            End If

            'SQL
            Dim sb As StringBuilder = New StringBuilder()
            sb.Append("SELECT ")
            sb.Append("  rtrim(A.JSRSTAFFCODE) as JSRSTAFFCODE ")
            sb.Append("  , rtrim(A.STAFFCODE) as STAFFCODE ")
            sb.Append("FROM ")
            sb.Append("  MB002_STAFFORG as A ")
            sb.Append("  INNER JOIN MB001_STAFF as B ")
            sb.Append("    ON B.CAMPCODE = A.CAMPCODE ")
            sb.Append("    and B.STAFFCODE = A.STAFFCODE ")
            sb.Append("    and B.STYMD <= @P1 ")
            sb.Append("    and B.ENDYMD >= @P1 ")
            sb.Append("    and B.DELFLG <> '1' ")
            sb.Append("Where ")
            sb.Append("  A.CAMPCODE = @P2 ")
            sb.Append("  and A.SORG = @P3 ")
            sb.Append("  and A.DELFLG <> '1' ")
            If Not String.IsNullOrEmpty(I_JSRCODE) Then
                sb.Append("  and A.JSRSTAFFCODE = @P4 ")
            End If

            Try
                If SQLcon.State <> ConnectionState.Open Then
                    SQLcon.Open() 'DataBase接続(Open)
                End If

                Using SQLcmd As New SqlCommand(sb.ToString, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar)
                    PARA1.Value = Date.Now
                    PARA2.Value = Me.CAMPCODE
                    PARA3.Value = Me.ORGCODE
                    PARA4.Value = I_JSRCODE

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        While SQLdr.Read
                            Dim wkValue = New JSRCODE_STAFF With {
                                .JSRSTAFFCODE = SQLdr("JSRSTAFFCODE").ToString,
                                .STAFFCODE = SQLdr("STAFFCODE").ToString
                            }

                            If String.IsNullOrEmpty(wkValue.JSRSTAFFCODE) Then
                                Continue While
                            End If
                            'DictionaryKey作成
                            ' 部署|JSRコード
                            Dim wkKey = MakeDicKey(wkValue.JSRSTAFFCODE)
                            '複数呼出OK
                            _dicStaff(wkKey) = wkValue
                            '_dicStaff.Add(wkKey, wkValue)
                        End While
                    End Using
                End Using

            Catch ex As Exception
                ERR = C_MESSAGE_NO.DB_ERROR
                rtn = False
            End Try

            Return rtn

        End Function

        ''' <summary>
        ''' DictionaryKey作成
        ''' </summary>
        ''' <remarks></remarks>
        Private Function MakeDicKey(ByVal I_JSRCODE As String) As String
            Dim wkKey As String = String.Format("{1}{0}{2}", C_VALUE_SPLIT_DELIMITER, Me.ORGCODE, I_JSRCODE)
            ' 部署コード|JSRコード
            Return wkKey

        End Function
    End Class
#End Region

#Region "<< L1統計DB関連 >>"
    ''' <summary>
    ''' L1統計DB
    ''' </summary>
    ''' <remarks></remarks>
    Public Class L1TOKEI
        Inherits GRT00004COM

        Private CS0044L1INSERT As New BASEDLL.CS0044L1INSERT            '統計DB出力

        Private CS0033AutoNumber As New BASEDLL.CS0033AutoNumber        '自動採番
        Private CS0038ACCODEget As New BASEDLL.CS0038ACCODEget          '勘定科目判定
        Private CS0041TORIORGget As New BASEDLL.CS0041TORIORGget        '取引先タイプ取得
        Private CS0043STAFFORGget As New BASEDLL.CS0043STAFFORGget      '社員管理部署取得
        Private CS0045GSHABANORGget As New BASEDLL.CS0045GSHABANORGget  '車両管理部署取得

        Private L00001tbl As DataTable                                  '統計DB出力用テーブル

        Private ReadOnly UPDUSERID As String                            '更新ユーザID
        Private ReadOnly UPDTERMID As String                            '更新端末ID
        ''' <summary>
        ''' 休日区分カレンダー
        ''' </summary>
        ''' <remarks></remarks>
        Private _dicCal As Dictionary(Of String, String) = New Dictionary(Of String, String)

        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            MyBase.New()
            Initialize()

            '統計DB格納テーブル作成
            L00001tbl = New DataTable
            CS0044L1INSERT.CS0044L1ColmnsAdd(L00001tbl)

        End Sub
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="SQLCon" >DB接続</param>
        ''' <remarks></remarks>
        Public Sub New(ByRef SQLCon As SqlConnection, ByVal UPDUSERID As String, ByVal UPDUSERTERMID As String)
            Me.New()
            Me.SQLcon = SQLCon
            Me.UPDUSERID = UPDUSERID
            Me.UPDTERMID = UPDUSERTERMID
            CS0044L1INSERT.SQLCON = Me.SQLcon

        End Sub

        ''' <summary>
        ''' クローズ
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Close()
            CS0044L1INSERT.SQLCON = Nothing

            L00001tbl.Clear()
            L00001tbl = Nothing
            MyBase.Dispose()
        End Sub

        ''' <summary>
        ''' 統計DBレコード編集
        ''' </summary>
        ''' <param name="T00004UPDtbl" >T4更新データ</param>
        ''' <param name="O_RTN" >ERR</param>
        ''' <remarks></remarks>
        Public Sub Edit(ByRef T00004UPDtbl As DataTable, ByRef O_RTN As String)

            Dim WW_DATENOW As Date = Date.Now
            Dim WW_M0008tbl As New DataTable
            Dim MC003tbl = New DataTable                                   '取引先部署テーブル
            Dim MA003tbl = New DataTable                                   '車両台帳テーブル
            Dim MA006tbl = New DataTable                                   '車両部署マスタテーブル
            Dim MB001tbl = New DataTable                                   '社員マスタテーブル

            Dim WW_hokidaykbn As String = ""

            O_RTN = C_MESSAGE_NO.NORMAL

            '■■■ T00004UPDtblより統計ＤＢ追加 ■■■
            '
            For Each T00004UPDrow As DataRow In T00004UPDtbl.Rows

                If T00004UPDrow("DELFLG") = C_DELETE_FLG.ALIVE AndAlso
                    T00004UPDrow("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
                Else
                    Continue For
                End If

                '着地基準の場合、積配で予定を作成し積置は捨てる
                If T00004UPDrow("URIKBN") = "2" Then
                    If T00004UPDrow("TUMIOKIKBN") = "1" AndAlso
                       T00004UPDrow("SHUKODATE") = T00004UPDrow("SHUKADATE") Then
                        Continue For
                    End If
                End If


                Dim L00001row As DataRow = L00001tbl.NewRow

                Dim WW_SEQ As String = "000000"

                '伝票番号採番
                CS0033AutoNumber.SEQTYPE = CS0033AutoNumber.C_SEQTYPE.DENNO
                CS0033AutoNumber.CAMPCODE = T00004UPDrow("CAMPCODE")
                CS0033AutoNumber.MORG = T00004UPDrow("ORDERORG")
                CS0033AutoNumber.USERID = Me.UPDUSERID
                CS0033AutoNumber.getAutoNumber()
                If isNormal(CS0033AutoNumber.ERR) Then
                    WW_SEQ = CS0033AutoNumber.SEQ
                Else
                    O_RTN = CS0033AutoNumber.ERR
                    Exit Sub
                End If

                '---------------------------------------------------------
                'L1出力編集
                '---------------------------------------------------------
                L00001row("CAMPCODE") = T00004UPDrow("CAMPCODE")                              '会社コード
                L00001row("MOTOCHO") = "LOPLAN"                                               '元帳（非会計予定を設定）
                L00001row("VERSION") = "000"                                                  'バージョン
                L00001row("DENTYPE") = "T04"                                                  '伝票タイプ
                L00001row("TENKI") = "0"                                                      '統計転記
                L00001row("KEIJOYMD") = T00004UPDrow("KIJUNDATE")                             '計上日付（基準日を設定）
                L00001row("DENYMD") = T00004UPDrow("SHUKODATE")                               '伝票日付（出庫日を設定）
                '伝票番号
                L00001row("DENNO") = T00004UPDrow("ORDERORG") &
                                    CDate(T00004UPDrow("KIJUNDATE")).ToString("yyyy") &
                                    WW_SEQ
                '関連伝票No＋明細No
                L00001row("KANRENDENNO") = T00004UPDrow("ORDERORG") & " " _
                              & T00004UPDrow("ORDERNO") & " " _
                              & T00004UPDrow("TRIPNO") & " " _
                              & T00004UPDrow("DROPNO") & " " _
                              & T00004UPDrow("SEQ")

                L00001row("ACTORICODE") = ""                                                  '取引先コード
                L00001row("ACOILTYPE") = ""                                                   '油種
                L00001row("ACSHARYOTYPE") = ""                                                '統一車番(上)
                L00001row("ACTSHABAN") = ""                                                   '統一車番(下)
                L00001row("ACSTAFFCODE") = ""                                                 '従業員コード
                L00001row("ACBANKAC") = ""                                                    '銀行口座

                L00001row("ACKEIJOMORG") = T00004UPDrow("ORDERORG")                           '計上管理部署コード（受注部署）

                L00001row("ACTAXKBN") = ""                                                    '税区分
                L00001row("ACAMT") = 0                                                        '金額
                L00001row("NACSHUKODATE") = T00004UPDrow("SHUKODATE")                         '出庫日
                L00001row("NACSHUKADATE") = T00004UPDrow("SHUKADATE")                         '出荷日
                L00001row("NACTODOKEDATE") = T00004UPDrow("TODOKEDATE")                       '届日
                L00001row("NACTORICODE") = T00004UPDrow("TORICODE")                           '荷主コード
                L00001row("NACURIKBN") = T00004UPDrow("URIKBN")                               '売上計上基準
                L00001row("NACTODOKECODE") = T00004UPDrow("TODOKECODE")                       '届先コード
                L00001row("NACSTORICODE") = T00004UPDrow("STORICODE")                         '販売店コード
                L00001row("NACSHUKABASHO") = T00004UPDrow("SHUKABASHO")                       '出荷場所

                '取引先ORGより取得
                CS0041TORIORGget.TBL = MC003tbl
                CS0041TORIORGget.CAMPCODE = T00004UPDrow("CAMPCODE")
                CS0041TORIORGget.TORICODE = T00004UPDrow("TORICODE")
                CS0041TORIORGget.UORG = T00004UPDrow("SHIPORG")
                CS0041TORIORGget.CS0041TORIORGget()

                L00001row("NACTORITYPE01") = CS0041TORIORGget.TORITYPE01                    '取引先・取引タイプ01
                L00001row("NACTORITYPE02") = CS0041TORIORGget.TORITYPE02                    '取引先・取引タイプ02
                L00001row("NACTORITYPE03") = CS0041TORIORGget.TORITYPE03                    '取引先・取引タイプ03
                L00001row("NACTORITYPE04") = CS0041TORIORGget.TORITYPE04                    '取引先・取引タイプ04
                L00001row("NACTORITYPE05") = CS0041TORIORGget.TORITYPE05                    '取引先・取引タイプ05

                L00001row("NACOILTYPE") = T00004UPDrow("OILTYPE")                           '油種
                L00001row("NACPRODUCT1") = T00004UPDrow("PRODUCT1")                         '品名１
                L00001row("NACPRODUCT2") = T00004UPDrow("PRODUCT2")                         '品名２
                L00001row("NACPRODUCTCODE") = T00004UPDrow("PRODUCTCODE")                   '品名コード

                L00001row("NACGSHABAN") = T00004UPDrow("GSHABAN")                           '業務車番

                '車両マスタより
                CS0045GSHABANORGget.TBL = MA006tbl
                CS0045GSHABANORGget.CAMPCODE = T00004UPDrow("CAMPCODE")
                CS0045GSHABANORGget.UORG = T00004UPDrow("SHIPORG")
                CS0045GSHABANORGget.GSHABAN = T00004UPDrow("GSHABAN")
                CS0045GSHABANORGget.STYMD = T00004UPDrow("KIJUNDATE")
                CS0045GSHABANORGget.ENDYMD = T00004UPDrow("KIJUNDATE")
                CS0045GSHABANORGget.CS0045GSHABANORGget()

                If CS0045GSHABANORGget.MANGSUPPL = "" Then
                    L00001row("NACSUPPLIERKBN") = "1"                                       '社有・庸車区分
                    L00001row("NACSUPPLIER") = ""                                           '庸車会社
                Else
                    L00001row("NACSUPPLIERKBN") = "2"                                       '社有・庸車区分
                    L00001row("NACSUPPLIER") = CS0045GSHABANORGget.MANGSUPPL                '庸車会社
                End If

                L00001row("NACSHARYOOILTYPE") = CS0045GSHABANORGget.MANGOILTYPE             '車両登録油種

                L00001row("NACSHARYOTYPE1") = T00004UPDrow("SHARYOTYPEF")                   '統一車番(上)1
                L00001row("NACTSHABAN1") = T00004UPDrow("TSHABANF")                         '統一車番(下)1
                L00001row("NACMANGMORG1") = CS0045GSHABANORGget.MANGMORGF                   '車両管理部署1
                L00001row("NACMANGSORG1") = CS0045GSHABANORGget.MANGSORGF                   '車両設置部署1
                L00001row("NACMANGUORG1") = T00004UPDrow("SHIPORG")                         '車両運用部署1
                L00001row("NACBASELEASE1") = CS0045GSHABANORGget.BASELEASEF                 '車両所有1

                L00001row("NACSHARYOTYPE2") = T00004UPDrow("SHARYOTYPEB")                   '統一車番(上)2
                L00001row("NACTSHABAN2") = T00004UPDrow("TSHABANB")                         '統一車番(下)2
                L00001row("NACMANGMORG2") = CS0045GSHABANORGget.MANGMORGB                   '車両管理部署2
                L00001row("NACMANGSORG2") = CS0045GSHABANORGget.MANGSORGB                   '車両設置部署2
                L00001row("NACMANGUORG2") = T00004UPDrow("SHIPORG")                         '車両運用部署1
                L00001row("NACBASELEASE2") = CS0045GSHABANORGget.BASELEASEB                 '車両所有2

                L00001row("NACSHARYOTYPE3") = T00004UPDrow("SHARYOTYPEB2")                  '統一車番(上)3
                L00001row("NACTSHABAN3") = T00004UPDrow("TSHABANB2")                        '統一車番(下)3
                L00001row("NACMANGMORG3") = CS0045GSHABANORGget.MANGMORGB2                  '車両管理部署3
                L00001row("NACMANGSORG3") = CS0045GSHABANORGget.MANGSORGB2                  '車両設置部署3
                L00001row("NACMANGUORG3") = T00004UPDrow("SHIPORG")                         '車両運用部署1
                L00001row("NACBASELEASE3") = CS0045GSHABANORGget.BASELEASEB2                '車両所有3

                L00001row("NACCREWKBN") = "1"                                               '正副区分
                L00001row("NACSTAFFCODE") = T00004UPDrow("STAFFCODE")                       '従業員コード（正）
                '社員マスターより
                CS0043STAFFORGget.TBL = MB001tbl
                CS0043STAFFORGget.CAMPCODE = T00004UPDrow("CAMPCODE")
                CS0043STAFFORGget.STAFFCODE = T00004UPDrow("STAFFCODE")
                CS0043STAFFORGget.SORG = T00004UPDrow("SHIPORG")
                CS0043STAFFORGget.STYMD = T00004UPDrow("KIJUNDATE")
                CS0043STAFFORGget.ENDYMD = T00004UPDrow("KIJUNDATE")
                CS0043STAFFORGget.CS0043STAFFORGget()

                L00001row("NACSTAFFKBN") = CS0043STAFFORGget.STAFFKBN                       '社員区分（正）
                L00001row("NACMORG") = CS0043STAFFORGget.MORG                               '管理部署（正）
                L00001row("NACHORG") = CS0043STAFFORGget.HORG                               '配属部署（正）
                L00001row("NACSORG") = T00004UPDrow("SHIPORG")                              '作業部署（正）

                L00001row("NACSTAFFCODE2") = T00004UPDrow("SUBSTAFFCODE")                   '従業員コード（副）
                '社員マスターより
                CS0043STAFFORGget.TBL = MB001tbl
                CS0043STAFFORGget.CAMPCODE = T00004UPDrow("CAMPCODE")
                CS0043STAFFORGget.STAFFCODE = T00004UPDrow("SUBSTAFFCODE")
                CS0043STAFFORGget.SORG = T00004UPDrow("SHIPORG")
                CS0043STAFFORGget.STYMD = T00004UPDrow("KIJUNDATE")
                CS0043STAFFORGget.ENDYMD = T00004UPDrow("KIJUNDATE")
                CS0043STAFFORGget.CS0043STAFFORGget()

                L00001row("NACSTAFFKBN2") = CS0043STAFFORGget.STAFFKBN                      '社員区分（副）
                L00001row("NACMORG2") = CS0043STAFFORGget.MORG                              '管理部署（副）
                L00001row("NACHORG2") = CS0043STAFFORGget.HORG                              '配属部署（副）
                If T00004UPDrow("SUBSTAFFCODE") = "" Then
                    L00001row("NACSORG2") = ""                                              '作業部署（副）
                Else
                    L00001row("NACSORG2") = T00004UPDrow("SHIPORG")                         '作業部署（副）
                End If

                L00001row("NACORDERNO") = T00004UPDrow("ORDERNO")                           '受注番号
                L00001row("NACDETAILNO") = T00004UPDrow("DETAILNO")                         '明細№
                L00001row("NACTRIPNO") = T00004UPDrow("TRIPNO")                             'トリップ
                L00001row("NACDROPNO") = T00004UPDrow("DROPNO")                             'ドロップ
                L00001row("NACSEQ") = T00004UPDrow("SEQ")                                   'SEQ

                L00001row("NACORDERORG") = T00004UPDrow("ORDERORG")                         '受注部署
                L00001row("NACSHIPORG") = T00004UPDrow("SHIPORG")                           '配送部署
                L00001row("NACSURYO") = T00004UPDrow("SURYO")                               '受注・数量
                L00001row("NACTANI") = T00004UPDrow("HTANI")                                '受注・単位
                L00001row("NACJSURYO") = 0                                                  '実績・配送数量
                L00001row("NACSTANI") = ""                                                  '実績・配送単位
                L00001row("NACHAIDISTANCE") = 0                                             '実績・配送距離
                L00001row("NACKAIDISTANCE") = 0                                             '実績・回送作業距離
                L00001row("NACCHODISTANCE") = 0                                             '実績・勤怠調整距離
                L00001row("NACTTLDISTANCE") = 0                                             '実績・配送距離合計Σ
                L00001row("NACHAISTDATE") = C_DEFAULT_YMD                                   '実績・配送作業開始日時
                L00001row("NACHAIENDDATE") = C_DEFAULT_YMD                                  '実績・配送作業終了日時
                L00001row("NACHAIWORKTIME") = 0                                             '実績・配送作業時間（分）
                L00001row("NACGESSTDATE") = C_DEFAULT_YMD                                   '実績・下車作業開始日時
                L00001row("NACGESENDDATE") = C_DEFAULT_YMD                                  '実績・下車作業終了日時
                L00001row("NACGESWORKTIME") = 0                                             '実績・下車作業時間（分）
                L00001row("NACCHOWORKTIME") = 0                                             '実績・勤怠調整時間（分）
                L00001row("NACTTLWORKTIME") = 0                                             '実績・配送合計時間Σ（分）
                L00001row("NACOUTWORKTIME") = 0                                             '実績・就業外時間（分）
                L00001row("NACBREAKSTDATE") = C_DEFAULT_YMD                                 '実績・休憩開始日時
                L00001row("NACBREAKENDDATE") = C_DEFAULT_YMD                                '実績・休憩終了日時
                L00001row("NACBREAKTIME") = 0                                               '実績・休憩時間（分）
                L00001row("NACCHOBREAKTIME") = 0                                            '実績・休憩調整時間（分）
                L00001row("NACTTLBREAKTIME") = 0                                            '実績・休憩合計時間Σ（分）
                L00001row("NACCASH") = 0                                                    '実績・現金
                L00001row("NACETC") = 0                                                     '実績・ETC
                L00001row("NACTICKET") = 0                                                  '実績・回数券
                L00001row("NACKYUYU") = 0                                                   '実績・軽油
                L00001row("NACUNLOADCNT") = 0                                               '実績・荷卸回数
                L00001row("NACCHOUNLOADCNT") = 0                                            '実績・荷卸回数調整
                L00001row("NACTTLUNLOADCNT") = 0                                            '実績・荷卸回数合計Σ
                L00001row("NACKAIJI") = 0                                                   '実績・回次
                L00001row("NACJITIME") = 0                                                  '実績・実車時間（分）
                L00001row("NACJICHOSTIME") = 0                                              '実績・実車時間調整（分）
                L00001row("NACJITTLETIME") = 0                                              '実績・実車時間合計Σ（分）
                L00001row("NACKUTIME") = 0                                                  '実績・空車時間（分）
                L00001row("NACKUCHOTIME") = 0                                               '実績・空車時間調整（分）
                L00001row("NACKUTTLTIME") = 0                                               '実績・空車時間合計Σ（分）
                L00001row("NACJIDISTANCE") = 0                                              '実績・実車距離
                L00001row("NACJICHODISTANCE") = 0                                           '実績・実車距離調整
                L00001row("NACJITTLDISTANCE") = 0                                           '実績・実車距離合計Σ
                L00001row("NACKUDISTANCE") = 0                                              '実績・空車距離
                L00001row("NACKUCHODISTANCE") = 0                                           '実績・空車距離調整
                L00001row("NACKUTTLDISTANCE") = 0                                           '実績・空車距離合計Σ
                L00001row("NACTARIFFFARE") = 0                                              '実績・運賃タリフ額
                L00001row("NACFIXEDFARE") = 0                                               '実績・運賃固定額
                L00001row("NACINCHOFARE") = 0                                               '実績・運賃手入力調整額
                L00001row("NACTTLFARE") = 0                                                 '実績・運賃合計額Σ
                L00001row("NACOFFICESORG") = ""                                             '実績・作業部署
                L00001row("NACOFFICETIME") = 0                                              '実績・事務時間
                L00001row("NACOFFICEBREAKTIME") = 0                                         '実績・事務休憩時間
                L00001row("PAYSHUSHADATE") = C_DEFAULT_YMD                                  '出社日時
                L00001row("PAYTAISHADATE") = C_DEFAULT_YMD                                  '退社日時
                L00001row("PAYSTAFFCODE") = T00004UPDrow("STAFFCODE")                       '従業員コード
                L00001row("PAYSTAFFKBN") = L00001row("NACSTAFFKBN")                         '社員区分
                L00001row("PAYMORG") = L00001row("NACMORG")                                 '従業員管理部署
                L00001row("PAYHORG") = L00001row("NACHORG")                                 '従業員配属部署

                '休日区分取得
                GetHOLIDAYKBN(T00004UPDrow("CAMPCODE"), T00004UPDrow("SHUKODATE"), WW_hokidaykbn)

                L00001row("PAYHOLIDAYKBN") = WW_hokidaykbn                                  '休日区分
                L00001row("PAYKBN") = ""                                                    '勤怠区分
                L00001row("PAYSHUKCHOKKBN") = ""                                            '宿日直区分
                L00001row("PAYJYOMUKBN") = ""                                               '乗務区分
                L00001row("PAYOILKBN") = ""                                                 '勤怠用油種区分
                L00001row("PAYSHARYOKBN") = ""                                              '勤怠用車両区分
                L00001row("PAYWORKNISSU") = 0                                               '所労
                L00001row("PAYSHOUKETUNISSU") = 0                                           '傷欠
                L00001row("PAYKUMIKETUNISSU") = 0                                           '組欠
                L00001row("PAYETCKETUNISSU") = 0                                            '他欠
                L00001row("PAYNENKYUNISSU") = 0                                             '年休
                L00001row("PAYTOKUKYUNISSU") = 0                                            '特休
                L00001row("PAYCHIKOKSOTAINISSU") = 0                                        '遅早
                L00001row("PAYSTOCKNISSU") = 0                                              'ストック休暇
                L00001row("PAYKYOTEIWEEKNISSU") = 0                                         '協定週休
                L00001row("PAYWEEKNISSU") = 0                                               '週休
                L00001row("PAYDAIKYUNISSU") = 0                                             '代休
                L00001row("PAYWORKTIME") = 0                                                '所定労働時間（分）
                L00001row("PAYNIGHTTIME") = 0                                               '所定深夜時間（分）
                L00001row("PAYORVERTIME") = 0                                               '平日残業時間（分）
                L00001row("PAYWNIGHTTIME") = 0                                              '平日深夜時間（分）
                L00001row("PAYWSWORKTIME") = 0                                              '日曜出勤時間（分）
                L00001row("PAYSNIGHTTIME") = 0                                              '日曜深夜時間（分）
                L00001row("PAYHWORKTIME") = 0                                               '休日出勤時間（分）
                L00001row("PAYHNIGHTTIME") = 0                                              '休日深夜時間（分）
                L00001row("PAYBREAKTIME") = 0                                               '休憩時間（分）

                L00001row("PAYNENSHINISSU") = 0                                             '年始出勤
                L00001row("PAYSHUKCHOKNNISSU") = 0                                          '宿日直年始
                L00001row("PAYSHUKCHOKNISSU") = 0                                           '宿日直通常
                L00001row("PAYSHUKCHOKNHLDNISSU") = 0                                       '宿日直年始（翌日休み）
                L00001row("PAYSHUKCHOKHLDNISSU") = 0                                        '宿日直通常（翌日休み）
                L00001row("PAYTOKSAAKAISU") = 0                                             '特作A
                L00001row("PAYTOKSABKAISU") = 0                                             '特作B
                L00001row("PAYTOKSACKAISU") = 0                                             '特作C
                L00001row("PAYTENKOKAISU") = 0                                              '点呼回数
                L00001row("PAYHOANTIME") = 0                                                '保安検査入力（分）
                L00001row("PAYKOATUTIME") = 0                                               '高圧作業入力（分）
                L00001row("PAYTOKUSA1TIME") = 0                                             '特作Ⅰ（分）
                L00001row("PAYPONPNISSU") = 0                                               'ポンプ
                L00001row("PAYBULKNISSU") = 0                                               'バルク
                L00001row("PAYTRAILERNISSU") = 0                                            'トレーラ
                L00001row("PAYBKINMUKAISU") = 0                                             'B勤務
                L00001row("PAYYENDTIME") = "00:00"                                          '予定退社時刻
                L00001row("PAYAPPLYID") = ""                                                '申請ID
                L00001row("PAYRIYU") = ""                                                   '理由コード
                L00001row("PAYRIYUETC") = ""                                                '理由その他
                L00001row("APPKIJUN") = ""                                                  '配賦基準
                L00001row("APPKEY") = ""                                                    '配賦統計キー

                L00001row("WORKKBN") = ""                                                   '作業区分
                L00001row("KEYSTAFFCODE") = T00004UPDrow("STAFFCODE")                       '従業員コードキー
                L00001row("KEYGSHABAN") = T00004UPDrow("GSHABAN")                           '業務車番キー
                L00001row("KEYTRIPNO") = T00004UPDrow("TRIPNO")                             'トリップキー
                L00001row("KEYDROPNO") = T00004UPDrow("DROPNO")                             'ドロップキー

                L00001row("DELFLG") = C_DELETE_FLG.ALIVE                                    '削除フラグ

                '勘定科目判定テーブル検索（共通設定項目）
                CS0038ACCODEget.TBL = WW_M0008tbl                                           '勘定科目判定テーブル
                CS0038ACCODEget.CAMPCODE = L00001row("CAMPCODE")                            '会社コード
                CS0038ACCODEget.STYMD = L00001row("KEIJOYMD")                               '開始日
                CS0038ACCODEget.ENDYMD = L00001row("KEIJOYMD")                              '終了日
                CS0038ACCODEget.MOTOCHO = "LOPLAN"                                          '元帳
                CS0038ACCODEget.DENTYPE = "T04"                                             '伝票タイプ

                CS0038ACCODEget.TORICODE = L00001row("NACTORICODE")                         '荷主コード
                CS0038ACCODEget.TORITYPE01 = L00001row("NACTORITYPE01")                     '取引タイプ01
                CS0038ACCODEget.TORITYPE02 = L00001row("NACTORITYPE02")                     '取引タイプ02
                CS0038ACCODEget.TORITYPE03 = L00001row("NACTORITYPE03")                     '取引タイプ03
                CS0038ACCODEget.TORITYPE04 = L00001row("NACTORITYPE04")                     '取引タイプ04
                CS0038ACCODEget.TORITYPE05 = L00001row("NACTORITYPE05")                     '取引タイプ05
                CS0038ACCODEget.URIKBN = L00001row("NACURIKBN")                             '売上計上基準
                CS0038ACCODEget.STORICODE = L00001row("NACSTORICODE")                       '販売店コード
                CS0038ACCODEget.OILTYPE = L00001row("NACOILTYPE")                           '油種
                CS0038ACCODEget.PRODUCT1 = L00001row("NACPRODUCT1")                         '品名１
                CS0038ACCODEget.SUPPLIERKBN = L00001row("NACSUPPLIERKBN")                   '社有・庸車区分
                CS0038ACCODEget.MANGSORG = L00001row("NACMANGSORG1")                        '車両設置部署
                CS0038ACCODEget.MANGUORG = L00001row("NACMANGUORG1")                        '車両運用部署
                CS0038ACCODEget.BASELEASE = L00001row("NACBASELEASE1")                      '車両所有
                CS0038ACCODEget.STAFFKBN = L00001row("NACSTAFFKBN")                         '社員区分
                CS0038ACCODEget.HORG = L00001row("NACHORG")                                 '配属部署
                CS0038ACCODEget.SORG = L00001row("NACSORG")                                 '作業部署

                '勘定科目判定テーブル検索（借方）
                CS0038ACCODEget.ACHANTEI = "HID"                                            '勘定科目判定コード
                CS0038ACCODEget.CS0038ACCODEget()
                Dim WW_ACCODE_D As String = CS0038ACCODEget.ACCODE
                Dim WW_SUBACCODE_D As String = CS0038ACCODEget.SUBACCODE
                Dim WW_INQKBN_D As String = CS0038ACCODEget.INQKBN

                '勘定科目判定テーブル検索（貸方）
                CS0038ACCODEget.ACHANTEI = "HIC"                                            '勘定科目判定コード
                CS0038ACCODEget.CS0038ACCODEget()
                Dim WW_ACCODE_C As String = CS0038ACCODEget.ACCODE
                Dim WW_SUBACCODE_C As String = CS0038ACCODEget.SUBACCODE
                Dim WW_INQKBN_C As String = CS0038ACCODEget.INQKBN

                Dim WW_ROW As DataRow

                '------------------------------------------------------
                '追加データ
                '------------------------------------------------------
                '●借方
                If WW_INQKBN_D = "1" Then
                    L00001row("ACCODE") = WW_ACCODE_D                                 '勘定科目コード
                    L00001row("SUBACCODE") = WW_SUBACCODE_D                           '補助科目コード
                    L00001row("INQKBN") = WW_INQKBN_D                                 '照会区分
                    L00001row("ACDCKBN") = "D"                                        '貸借区分
                    L00001row("ACACHANTEI") = "HID"                                   '勘定科目判定コード
                    L00001row("DTLNO") = "01"                                         '明細番号
                    L00001row("ACKEIJOORG") = T00004UPDrow("ORDERORG")                '計上部署コード（受注部署）

                    WW_ROW = L00001tbl.NewRow
                    WW_ROW.ItemArray = L00001row.ItemArray
                    L00001tbl.Rows.Add(WW_ROW)
                End If
                '●貸方
                If WW_INQKBN_C = "1" Then
                    L00001row("ACCODE") = WW_ACCODE_C                                 '勘定科目コード
                    L00001row("SUBACCODE") = WW_SUBACCODE_C                           '補助科目コード
                    L00001row("INQKBN") = WW_INQKBN_C                                 '照会区分
                    L00001row("ACDCKBN") = "C"                                        '貸借区分
                    L00001row("ACACHANTEI") = "HIC"                                   '勘定科目判定コード
                    L00001row("DTLNO") = "02"                                         '明細番号
                    L00001row("ACKEIJOORG") = T00004UPDrow("SHIPORG")                 '計上部署コード（配送部署）

                    WW_ROW = L00001tbl.NewRow
                    WW_ROW.ItemArray = L00001row.ItemArray
                    L00001tbl.Rows.Add(WW_ROW)
                End If

            Next

        End Sub

        ''' <summary>
        ''' 統計DB更新
        ''' </summary>
        ''' <param name="O_RTN" >ERR</param>
        ''' <remarks></remarks>
        Public Sub Update(ByRef T00004UPDtbl As DataTable, ByRef O_RTN As String)

            Dim WW_DATENOW As Date = Date.Now

            O_RTN = C_MESSAGE_NO.NORMAL

            If IsNothing(Me.SQLcon) Then
                'DataBase接続文字
                Me.SQLcon = sm.getConnection
                SQLcon.Open() 'DataBase接続(Open)
            End If

            '日報ＤＢ更新
            Dim SQLStr As String =
                        "UPDATE L0001_TOKEI " _
                      & "SET DELFLG         = '1' " _
                      & "  , UPDYMD         = @P08 " _
                      & "  , UPDUSER        = @P09 " _
                      & "  , UPDTERMID      = @P10 " _
                      & "  , RECEIVEYMD     = @P11  " _
                      & "WHERE CAMPCODE     = @P01 " _
                      & "  and DENTYPE      = @P02 " _
                      & "  and NACSHUKODATE = @P03 " _
                      & "  and KEYSTAFFCODE = @P04 " _
                      & "  and KEYGSHABAN   = @P05 " _
                      & "  and KEYTRIPNO    = @P06 " _
                      & "  and KEYDROPNO    = @P07 " _
                      & "  and DELFLG      <> '1' ; "

            Dim SQLcmd As SqlCommand = New SqlCommand(SQLStr, SQLcon)
            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.Date)
            Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.DateTime)
            Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar, 30)
            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)


            '■■■ 統計ＤＢ出力 ■■■
            '
            For Each T00004UPDrow In T00004UPDtbl.Rows

                If T00004UPDrow("TIMSTP") <> "0" AndAlso
                   T00004UPDrow("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
                Else
                    Continue For
                End If

                Try

                    PARA01.Value = T00004UPDrow("CAMPCODE")
                    PARA02.Value = "T04"
                    PARA03.Value = T00004UPDrow("SHUKODATE")
                    PARA04.Value = T00004UPDrow("STAFFCODE")
                    PARA05.Value = T00004UPDrow("GSHABAN")
                    PARA06.Value = T00004UPDrow("TRIPNO")
                    PARA07.Value = T00004UPDrow("DROPNO")
                    PARA08.Value = WW_DATENOW
                    PARA09.Value = UPDUSERID
                    PARA10.Value = UPDTERMID
                    PARA11.Value = C_DEFAULT_YMD

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                Catch ex As Exception
                    O_RTN = C_MESSAGE_NO.DB_ERROR
                    PutLog(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, ex.ToString)

                    Exit Sub
                End Try

            Next
            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

            For Each L00001row In L00001tbl.Rows

                L00001row("INITYMD") = WW_DATENOW '登録年月日
                L00001row("UPDYMD") = WW_DATENOW  '更新年月日
                L00001row("UPDUSER") = UPDUSERID  '更新ユーザＩＤ
                L00001row("UPDTERMID") = UPDTERMID   '更新端末
                L00001row("RECEIVEYMD") = C_DEFAULT_YMD  '集信日時

            Next

            CS0044L1INSERT.CS0044L1Insert(L00001tbl)

        End Sub

        ''' <summary>
        ''' カレンダー取得 
        ''' </summary>
        ''' <param name="I_CAMPCODE" >会社コード</param>
        ''' <param name="I_WORKINGYMD" >日付</param>
        ''' <param name="O_HOLIDAYKBN" >休日区分</param>
        ''' <remarks></remarks>
        Private Sub GetHOLIDAYKBN(ByVal I_CAMPCODE As String, ByVal I_WORKINGYMD As Date, ByRef O_HOLIDAYKBN As String)

            Dim dicKey As String = I_CAMPCODE & "_" & I_WORKINGYMD.ToString("yyyy/MM/dd")

            Try
                ' 指定された会社コード・日付が未取得時は指定会社の日付全件取得
                If Not _dicCal.ContainsKey(dicKey) Then

                    Dim SQLStr As String =
                         "SELECT CAMPCODE " _
                       & ", WORKINGYMD " _
                       & ", isnull(rtrim(WORKINGKBN),'') as WORKINGKBN " _
                       & " FROM  MB005_CALENDAR " _
                       & " Where CAMPCODE   = @CAMPCODE " _
                       & "   and DELFLG    <> @DELFLG "

                    Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim P_CAMPCODE As SqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar, 20)
                    Dim P_DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", System.Data.SqlDbType.NVarChar, 1)
                    P_CAMPCODE.Value = I_CAMPCODE
                    P_DELFLG.Value = C_DELETE_FLG.DELETE

                    Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    While SQLdr.Read
                        _dicCal(SQLdr("CAMPCODE") & "_" & SQLdr("WORKINGYMD")) = SQLdr("WORKINGKBN")
                    End While
                    'Close
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing

                    SQLcmd.Dispose()
                    SQLcmd = Nothing

                End If

                O_HOLIDAYKBN = _dicCal.Item(dicKey)

            Catch ex As Exception
                PutLog(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT)
                'ログ出力
                Exit Sub
            End Try

        End Sub
    End Class
#End Region

#Region "<< 届先マスタ関連 >>"

    ''' <summary>
    ''' 届先マスタ更新
    ''' </summary>
    ''' <remarks></remarks>
    Public Class GRMC006UPDATE
        Inherits GRT00004COM

        ''' <summary>
        ''' トランザクション
        ''' </summary>
        ''' <returns></returns>
        Public Property SQLtrn As SqlTransaction
        ''' <summary>
        ''' トランザクション
        ''' </summary>
        ''' <returns></returns>
        Public Property CAMPCODE As String
        ''' <summary>
        ''' トランザクション
        ''' </summary>
        ''' <returns></returns>
        Public Property UORG As String
        ''' <summary>
        ''' 更新ユーザID
        ''' </summary>
        ''' <returns></returns>
        Public Property UPDUSERID As String
        ''' <summary>
        ''' 更新端末ID
        ''' </summary>
        ''' <returns></returns>
        Public Property UPDTERMID As String

        ''' <summary>
        ''' 届先マスタテーブル
        ''' </summary>
        Private MC006tbl As DataTable

        ''' <summary>
        '''  MC006tbl（届先マスタ）編集
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Update(ByVal order As IEnumerable(Of GRW0001KOUEIORDER.KOUEI_ORDER), ByRef master As KOUEI_MASTER)

            Dim WW_RTN As String = String.Empty

            ERR = C_MESSAGE_NO.NORMAL

            '届先追加データ
            Dim table = (From x In order
                         Select koueitype = x.KOUEITYPE,
                                toriCode = If(x.KOUEITYPE = GRW0001KOUEIORDER.KOUEITYPE_PREFIX.JX, GRT00004WRKINC.C_TORICODE_JX, GRT00004WRKINC.C_TORICODE_COSMO),
                                destCode = x.DESTCODE,
                                destName = x.DESTNAME,
                                destClass = If(x.TRIPSEQ = GRW0001KOUEIORDER.TRIPSEQ_TYPE.DEPT, "2", "1")
                         ).Distinct

            For Each row In table

                Dim todoke = master.GetTodoke(row.koueitype, row.destCode)
                If IsNothing(todoke) Then
                    todoke = New KOUEI_MASTER.KOUEI_TODOKE With {
                        .KOUEITYPE = row.koueitype,
                        .TODOKESAKICODE = row.destCode,
                        .NAME = row.destName,
                        .LATITUDE = String.Empty,
                        .LONGTIDUDE = String.Empty
                    }
                End If

                '届先マスタ追加データ編集
                MC006_set(row.koueitype, row.toriCode, todoke.TODOKESAKICODE, row.destClass, todoke.NAME, todoke.LATITUDE, todoke.LONGTIDUDE)
                '届先マスタ追加
                MC006_Update(WW_RTN)
                If Not isNormal(WW_RTN) Then
                    ERR = WW_RTN
                    Exit Sub
                End If
            Next

        End Sub

        ''' <summary>
        ''' MC006の項目を設定する（光英マスタから）
        ''' </summary>
        ''' <param name="I_CSVTYPE"></param>
        ''' <param name="I_TORICODE"></param>
        ''' <param name="I_TODOKECODE"></param>
        ''' <param name="I_CLASS"></param>
        ''' <param name="I_LATITUDE"></param>
        ''' <param name="I_LONGITUDE"></param>
        ''' <remarks></remarks>
        Protected Sub MC006_set(ByVal I_CSVTYPE As String,
                                ByVal I_TORICODE As String,
                                ByVal I_TODOKECODE As String,
                                ByVal I_CLASS As String,
                                ByVal I_NAME As String,
                                ByVal I_LATITUDE As String,
                                ByVal I_LONGITUDE As String)

            Try
                '〇MC006tbl作成
                MC006tbl_ColumnsAdd(MC006tbl)
                Dim MC006Row As DataRow = MC006tbl.NewRow()
                MC006tbl.Rows.Add(MC006Row)
                '〇初期化
                MC006tbl_Init(MC006Row)
                '〇項目の設定
                MC006Row("CAMPCODE") = CAMPCODE
                MC006Row("UORG") = UORG
                MC006Row("TORICODE") = I_TORICODE

                If I_CLASS = "1" Then
                    '届先
                    Select Case I_CSVTYPE
                        Case GRW0001KOUEIORDER.KOUEITYPE_PREFIX.JX, GRW0001KOUEIORDER.KOUEITYPE_PREFIX.TG
                            'MC006Row("TODOKECODE") = C_ANOTHER_SYSTEMS_DISTINATION_PREFIX.JX & I_TODOKECODE.PadLeft(9, "0")
                            MC006Row("TODOKECODE") = C_ANOTHER_SYSTEMS_DISTINATION_PREFIX.JX & I_TODOKECODE
                        Case GRW0001KOUEIORDER.KOUEITYPE_PREFIX.COSMO
                            'MC006Row("TODOKECODE") = C_ANOTHER_SYSTEMS_DISTINATION_PREFIX.COSMO & I_TODOKECODE.PadLeft(11, "0")
                            MC006Row("TODOKECODE") = C_ANOTHER_SYSTEMS_DISTINATION_PREFIX.COSMO & I_TODOKECODE
                        Case Else
                            MC006Row("TODOKECODE") = I_TODOKECODE
                    End Select
                ElseIf I_CLASS = "2" Then
                    '出荷場所
                    Select Case I_CSVTYPE
                        Case GRW0001KOUEIORDER.KOUEITYPE_PREFIX.JX, GRW0001KOUEIORDER.KOUEITYPE_PREFIX.TG
                            'MC006Row("TODOKECODE") = C_ANOTHER_SYSTEMS_DISTINATION_PREFIX.JX & I_TODOKECODE.PadLeft(4, "0")
                            MC006Row("TODOKECODE") = C_ANOTHER_SYSTEMS_DISTINATION_PREFIX.JX & I_TODOKECODE
                        Case GRW0001KOUEIORDER.KOUEITYPE_PREFIX.COSMO
                            'MC006Row("TODOKECODE") = C_ANOTHER_SYSTEMS_DISTINATION_PREFIX.COSMO & I_TODOKECODE.PadLeft(4, "0")
                            MC006Row("TODOKECODE") = C_ANOTHER_SYSTEMS_DISTINATION_PREFIX.COSMO & I_TODOKECODE
                        Case Else
                            MC006Row("TODOKECODE") = I_TODOKECODE
                    End Select
                End If

                MC006Row("TODOKENAMEL") = I_NAME
                MC006Row("TODOKENAMES") = I_NAME
                MC006Row("CLASS") = I_CLASS

                '緯度経度が設定されている場合は緯度経度及び住所・市町村コードを設定する
                If Not String.IsNullOrEmpty(I_LATITUDE) AndAlso Not String.IsNullOrEmpty(I_LATITUDE) Then
                    MC006Row("LATITUDE") = I_LATITUDE
                    MC006Row("LONGITUDE") = I_LONGITUDE

                    'YOPLGeoCoderから住所取得
                    Dim clsGeoCoder = New CS0055GeoCoder()
                    Dim address As CS0055GeoCoder.AddressInfo = clsGeoCoder.GetAddress(I_LATITUDE, I_LONGITUDE)
                    If IsNothing(address) Then
                        '取得エラー時継続
                    Else
                        MC006Row("ADDR1") = address.Address1
                        MC006Row("ADDR2") = address.Address2
                        MC006Row("ADDR3") = address.Address3
                        MC006Row("ADDR4") = address.Address4
                        MC006Row("CITIES") = address.CityCode
                    End If
                    clsGeoCoder = Nothing
                End If

            Catch ex As Exception

            End Try
        End Sub

        ''' <summary>
        ''' 届先マスタ登録
        ''' TODO: 更新処理の追加＋光英マスタの参照が必要
        ''' </summary>
        ''' <param name="O_RTN">可否判定</param>
        ''' <remarks></remarks>
        Protected Sub MC006_Update(ByRef O_RTN As String)

            Dim WW_DATENOW As DateTime = Date.Now
            Dim WW_ORDERNO As String = String.Empty
            O_RTN = C_MESSAGE_NO.NORMAL

            Try

                Dim SQLStr As String =
                       " DECLARE @hensuu as bigint ;                                        " _
                     & " set @hensuu = 0 ;                                                  " _
                     & " DECLARE hensuu CURSOR FOR                                          " _
                     & "   SELECT CAST(UPDTIMSTP as bigint) as hensuu                       " _
                     & "     FROM MC006_TODOKESAKI                                          " _
                     & "     WHERE    CAMPCODE      = @P01                                  " _
                     & "       and    rtrim(TODOKECODE)    = @P03 ;                         " _
                     & "                                                                    " _
                     & " OPEN hensuu ;                                                      " _
                     & " FETCH NEXT FROM hensuu INTO @hensuu ;                              " _
                     & " IF ( @@FETCH_STATUS <> 0 )                                         " _
                     & "    INSERT INTO MC006_TODOKESAKI                                    " _
                     & "             (CAMPCODE ,                                            " _
                     & "              TORICODE ,                                            " _
                     & "              TODOKECODE ,                                          " _
                     & "              NAMES ,                                               " _
                     & "              NAMEL ,                                               " _
                     & "              NAMESK ,                                              " _
                     & "              NAMELK ,                                              " _
                     & "              POSTNUM1 ,                                            " _
                     & "              POSTNUM2 ,                                            " _
                     & "              ADDR1 ,                                               " _
                     & "              ADDR2 ,                                               " _
                     & "              ADDR3 ,                                               " _
                     & "              ADDR4 ,                                               " _
                     & "              TEL ,                                                 " _
                     & "              FAX ,                                                 " _
                     & "              MAIL ,                                                " _
                     & "              LATITUDE ,                                            " _
                     & "              LONGITUDE ,                                           " _
                     & "              CITIES ,                                              " _
                     & "              MORG ,                                                " _
                     & "              NOTES1 ,                                              " _
                     & "              NOTES2 ,                                              " _
                     & "              NOTES3 ,                                              " _
                     & "              NOTES4 ,                                              " _
                     & "              NOTES5 ,                                              " _
                     & "              NOTES6 ,                                              " _
                     & "              NOTES7 ,                                              " _
                     & "              NOTES8 ,                                              " _
                     & "              NOTES9 ,                                              " _
                     & "              NOTES10 ,                                             " _
                     & "              CLASS ,                                               " _
                     & "              STYMD ,                                               " _
                     & "              ENDYMD ,                                              " _
                     & "              DELFLG ,                                              " _
                     & "              INITYMD ,                                             " _
                     & "              UPDYMD ,                                              " _
                     & "              UPDUSER ,                                             " _
                     & "              UPDTERMID ,                                           " _
                     & "              RECEIVEYMD )                                          " _
                     & "      VALUES (@P01,@P02,@P03,@P04,@P05,@P06,@P07,@P08,@P09,@P10,    " _
                     & "              @P11,@P12,@P13,@P14,@P15,@P16,@P17,@P18,@P19,@P20,    " _
                     & "              @P21,@P22,@P23,@P24,@P25,@P26,@P27,@P28,@P29,@P30,    " _
                     & "              @P31,@P32,@P33,@P34,@P35,@P36,@P37,@P38,@P39);        " _
                     & " CLOSE hensuu ;                                                     " _
                     & " DEALLOCATE hensuu ;                                                "

                Using SQLcmd As SqlCommand = New SqlCommand(SQLStr, SQLcon, SQLtrn)
                    Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
                    Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
                    Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar)
                    Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar)
                    Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar)
                    Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar)
                    Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.NVarChar)
                    Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.NVarChar)
                    Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.NVarChar)
                    Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar)
                    Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.NVarChar)
                    Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar)
                    Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar)
                    Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.NVarChar)
                    Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", System.Data.SqlDbType.NVarChar)
                    Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", System.Data.SqlDbType.NVarChar)
                    Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", System.Data.SqlDbType.NVarChar)
                    Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", System.Data.SqlDbType.NVarChar)
                    Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", System.Data.SqlDbType.NVarChar)
                    Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", System.Data.SqlDbType.NVarChar)
                    Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", System.Data.SqlDbType.NVarChar)
                    Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", System.Data.SqlDbType.NVarChar)
                    Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", System.Data.SqlDbType.NVarChar)
                    Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", System.Data.SqlDbType.NVarChar)
                    Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", System.Data.SqlDbType.NVarChar)
                    Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", System.Data.SqlDbType.NVarChar)
                    Dim PARA27 As SqlParameter = SQLcmd.Parameters.Add("@P27", System.Data.SqlDbType.NVarChar)
                    Dim PARA28 As SqlParameter = SQLcmd.Parameters.Add("@P28", System.Data.SqlDbType.NVarChar)
                    Dim PARA29 As SqlParameter = SQLcmd.Parameters.Add("@P29", System.Data.SqlDbType.NVarChar)
                    Dim PARA30 As SqlParameter = SQLcmd.Parameters.Add("@P30", System.Data.SqlDbType.NVarChar)
                    Dim PARA31 As SqlParameter = SQLcmd.Parameters.Add("@P31", System.Data.SqlDbType.NVarChar)
                    Dim PARA32 As SqlParameter = SQLcmd.Parameters.Add("@P32", System.Data.SqlDbType.DateTime)
                    Dim PARA33 As SqlParameter = SQLcmd.Parameters.Add("@P33", System.Data.SqlDbType.DateTime)
                    Dim PARA34 As SqlParameter = SQLcmd.Parameters.Add("@P34", System.Data.SqlDbType.NVarChar)
                    Dim PARA35 As SqlParameter = SQLcmd.Parameters.Add("@P35", System.Data.SqlDbType.DateTime)
                    Dim PARA36 As SqlParameter = SQLcmd.Parameters.Add("@P36", System.Data.SqlDbType.DateTime)
                    Dim PARA37 As SqlParameter = SQLcmd.Parameters.Add("@P37", System.Data.SqlDbType.NVarChar)
                    Dim PARA38 As SqlParameter = SQLcmd.Parameters.Add("@P38", System.Data.SqlDbType.NVarChar)
                    Dim PARA39 As SqlParameter = SQLcmd.Parameters.Add("@P39", System.Data.SqlDbType.DateTime)

                    PARA01.Value = MC006tbl.Rows(0)("CAMPCODE")
                    PARA02.Value = MC006tbl.Rows(0)("TORICODE")
                    PARA03.Value = MC006tbl.Rows(0)("TODOKECODE")
                    PARA04.Value = MC006tbl.Rows(0)("TODOKENAMES")
                    PARA05.Value = MC006tbl.Rows(0)("TODOKENAMEL")
                    PARA06.Value = MC006tbl.Rows(0)("NAMESK")
                    PARA07.Value = MC006tbl.Rows(0)("NAMELK")
                    PARA08.Value = MC006tbl.Rows(0)("POSTNUM1")
                    PARA09.Value = MC006tbl.Rows(0)("POSTNUM2")
                    PARA10.Value = MC006tbl.Rows(0)("ADDR1")
                    PARA11.Value = MC006tbl.Rows(0)("ADDR2")
                    PARA12.Value = MC006tbl.Rows(0)("ADDR3")
                    PARA13.Value = MC006tbl.Rows(0)("ADDR4")
                    PARA14.Value = MC006tbl.Rows(0)("TEL")
                    PARA15.Value = MC006tbl.Rows(0)("FAX")
                    PARA16.Value = MC006tbl.Rows(0)("MAIL")
                    PARA17.Value = MC006tbl.Rows(0)("LATITUDE")
                    PARA18.Value = MC006tbl.Rows(0)("LONGITUDE")
                    PARA19.Value = MC006tbl.Rows(0)("CITIES")
                    PARA20.Value = MC006tbl.Rows(0)("MORG")
                    PARA21.Value = MC006tbl.Rows(0)("NOTES1")
                    PARA22.Value = MC006tbl.Rows(0)("NOTES2")
                    PARA23.Value = MC006tbl.Rows(0)("NOTES3")
                    PARA24.Value = MC006tbl.Rows(0)("NOTES4")
                    PARA25.Value = MC006tbl.Rows(0)("NOTES5")
                    PARA26.Value = MC006tbl.Rows(0)("NOTES6")
                    PARA27.Value = MC006tbl.Rows(0)("NOTES7")
                    PARA28.Value = MC006tbl.Rows(0)("NOTES8")
                    PARA29.Value = MC006tbl.Rows(0)("NOTES9")
                    PARA30.Value = MC006tbl.Rows(0)("NOTES10")
                    PARA31.Value = MC006tbl.Rows(0)("CLASS")
                    PARA32.Value = MC006tbl.Rows(0)("STYMD")
                    PARA33.Value = MC006tbl.Rows(0)("ENDYMD")

                    PARA34.Value = MC006tbl.Rows(0)("DELFLG")
                    PARA35.Value = WW_DATENOW
                    PARA36.Value = WW_DATENOW
                    PARA37.Value = MC006tbl.Rows(0)("UPDUSER")
                    PARA38.Value = MC006tbl.Rows(0)("UPDTERMID")
                    PARA39.Value = C_DEFAULT_YMD
                    SQLcmd.ExecuteNonQuery()
                End Using

                Dim SQLStr2 As String =
                       " DECLARE @hensuu as bigint ;                                        " _
                     & " set @hensuu = 0 ;                                                  " _
                     & " DECLARE hensuu CURSOR FOR                                          " _
                     & "   SELECT CAST(UPDTIMSTP as bigint) as hensuu                       " _
                     & "     FROM MC007_TODKORG                                             " _
                     & "     WHERE    CAMPCODE      = @P01                                  " _
                     & "       and    TODOKECODE    = @P03                                  " _
                     & "       and    UORG          = @P04 ;                                " _
                     & "                                                                    " _
                     & " OPEN hensuu ;                                                      " _
                     & " FETCH NEXT FROM hensuu INTO @hensuu ;                              " _
                     & " IF ( @@FETCH_STATUS <> 0 )                                         " _
                     & "    INSERT INTO MC007_TODKORG                                       " _
                     & "             (CAMPCODE ,                                            " _
                     & "              TORICODE ,                                            " _
                     & "              TODOKECODE ,                                          " _
                     & "              UORG ,                                                " _
                     & "              ARRIVTIME ,                                           " _
                     & "              DISTANCE ,                                            " _
                     & "              SEQ ,                                                 " _
                     & "              YTODOKECODE ,                                         " _
                     & "              JSRTODOKECODE ,                                       " _
                     & "              SHUKABASHO ,                                          " _
                     & "              DELFLG ,                                              " _
                     & "              INITYMD ,                                             " _
                     & "              UPDYMD ,                                              " _
                     & "              UPDUSER ,                                             " _
                     & "              UPDTERMID ,                                           " _
                     & "              RECEIVEYMD )                                          " _
                     & "      VALUES (@P01,@P02,@P03,@P04,                                  " _
                     & "              @P05,@P06,@P07,@P08,@P09,@P10,                        " _
                     & "              @P11,@P12,@P13,@P14,@P15,@P16);                       " _
                     & " CLOSE hensuu ;                                                     " _
                     & " DEALLOCATE hensuu ;                                                "

                Using SQLcmd2 As New SqlCommand(SQLStr2, SQLcon, SQLtrn)
                    Dim PARA201 As SqlParameter = SQLcmd2.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
                    Dim PARA202 As SqlParameter = SQLcmd2.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
                    Dim PARA203 As SqlParameter = SQLcmd2.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar)
                    Dim PARA204 As SqlParameter = SQLcmd2.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar)

                    Dim PARA205 As SqlParameter = SQLcmd2.Parameters.Add("@P05", System.Data.SqlDbType.Time)
                    Dim PARA206 As SqlParameter = SQLcmd2.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar)
                    Dim PARA207 As SqlParameter = SQLcmd2.Parameters.Add("@P07", System.Data.SqlDbType.Int)
                    Dim PARA208 As SqlParameter = SQLcmd2.Parameters.Add("@P08", System.Data.SqlDbType.NVarChar)
                    Dim PARA209 As SqlParameter = SQLcmd2.Parameters.Add("@P09", System.Data.SqlDbType.NVarChar)
                    Dim PARA210 As SqlParameter = SQLcmd2.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar)

                    Dim PARA211 As SqlParameter = SQLcmd2.Parameters.Add("@P11", System.Data.SqlDbType.NVarChar)
                    Dim PARA212 As SqlParameter = SQLcmd2.Parameters.Add("@P12", System.Data.SqlDbType.DateTime)
                    Dim PARA213 As SqlParameter = SQLcmd2.Parameters.Add("@P13", System.Data.SqlDbType.DateTime)
                    Dim PARA214 As SqlParameter = SQLcmd2.Parameters.Add("@P14", System.Data.SqlDbType.NVarChar)
                    Dim PARA215 As SqlParameter = SQLcmd2.Parameters.Add("@P15", System.Data.SqlDbType.NVarChar)
                    Dim PARA216 As SqlParameter = SQLcmd2.Parameters.Add("@P16", System.Data.SqlDbType.DateTime)

                    PARA201.Value = MC006tbl.Rows(0)("CAMPCODE")
                    PARA202.Value = MC006tbl.Rows(0)("TORICODE")
                    PARA203.Value = MC006tbl.Rows(0)("TODOKECODE")
                    PARA204.Value = MC006tbl.Rows(0)("UORG")
                    PARA205.Value = MC006tbl.Rows(0)("ARRIVTIME")
                    PARA206.Value = MC006tbl.Rows(0)("DISTANCE")
                    PARA207.Value = MC006tbl.Rows(0)("SEQ")
                    PARA208.Value = MC006tbl.Rows(0)("YTODOKECODE")
                    PARA209.Value = MC006tbl.Rows(0)("JSRTODOKECODE")
                    PARA210.Value = MC006tbl.Rows(0)("SHUKABASHO")

                    PARA211.Value = MC006tbl.Rows(0)("DELFLG")
                    PARA212.Value = WW_DATENOW
                    PARA213.Value = WW_DATENOW
                    PARA214.Value = MC006tbl.Rows(0)("UPDUSER")
                    PARA215.Value = MC006tbl.Rows(0)("UPDTERMID")
                    PARA216.Value = C_DEFAULT_YMD
                    SQLcmd2.ExecuteNonQuery()

                    'CLOSE
                End Using

            Catch ex As Exception

                PutLog(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, ex.ToString)
                O_RTN = C_MESSAGE_NO.DB_ERROR
                Exit Sub

            End Try

        End Sub

        ''' <summary>
        ''' 届先ローカルテーブル項目設定
        ''' </summary>
        ''' <param name="IO_TBL">ローカルテーブル</param>
        ''' <remarks></remarks>
        Public Sub MC006tbl_ColumnsAdd(ByRef IO_TBL As DataTable)

            If IsNothing(IO_TBL) Then IO_TBL = New DataTable

            If IO_TBL.Columns.Count <> 0 Then IO_TBL.Columns.Clear()

            IO_TBL.Clear()
            IO_TBL.Columns.Add("LINECNT", GetType(Integer))
            IO_TBL.Columns.Add("OPERATION", GetType(String))
            IO_TBL.Columns.Add("TIMSTP", GetType(String))
            IO_TBL.Columns.Add("SELECT", GetType(Integer))
            IO_TBL.Columns.Add("HIDDEN", GetType(Integer))

            IO_TBL.Columns.Add("CAMPCODE", GetType(String))
            IO_TBL.Columns.Add("CAMPNAMES", GetType(String))
            IO_TBL.Columns.Add("TORICODE", GetType(String))
            IO_TBL.Columns.Add("TORINAMES", GetType(String))
            IO_TBL.Columns.Add("TORINAMEL", GetType(String))
            IO_TBL.Columns.Add("TODOKECODE", GetType(String))
            IO_TBL.Columns.Add("TODOKENAMES", GetType(String))
            IO_TBL.Columns.Add("TODOKENAMEL", GetType(String))
            IO_TBL.Columns.Add("NAMESK", GetType(String))
            IO_TBL.Columns.Add("NAMELK", GetType(String))
            IO_TBL.Columns.Add("POSTNUM1", GetType(String))
            IO_TBL.Columns.Add("POSTNUM2", GetType(String))
            IO_TBL.Columns.Add("ADDR1", GetType(String))
            IO_TBL.Columns.Add("ADDR2", GetType(String))
            IO_TBL.Columns.Add("ADDR3", GetType(String))
            IO_TBL.Columns.Add("ADDR4", GetType(String))
            IO_TBL.Columns.Add("TEL", GetType(String))
            IO_TBL.Columns.Add("FAX", GetType(String))
            IO_TBL.Columns.Add("MAIL", GetType(String))
            IO_TBL.Columns.Add("LATITUDE", GetType(String))
            IO_TBL.Columns.Add("LONGITUDE", GetType(String))
            IO_TBL.Columns.Add("CITIES", GetType(String))
            IO_TBL.Columns.Add("MORG", GetType(String))
            IO_TBL.Columns.Add("NOTES1", GetType(String))
            IO_TBL.Columns.Add("NOTES2", GetType(String))
            IO_TBL.Columns.Add("NOTES3", GetType(String))
            IO_TBL.Columns.Add("NOTES4", GetType(String))
            IO_TBL.Columns.Add("NOTES5", GetType(String))
            IO_TBL.Columns.Add("NOTES6", GetType(String))
            IO_TBL.Columns.Add("NOTES7", GetType(String))
            IO_TBL.Columns.Add("NOTES8", GetType(String))
            IO_TBL.Columns.Add("NOTES9", GetType(String))
            IO_TBL.Columns.Add("NOTES10", GetType(String))
            IO_TBL.Columns.Add("CLASS", GetType(String))
            IO_TBL.Columns.Add("STYMD", GetType(String))
            IO_TBL.Columns.Add("ENDYMD", GetType(String))

            IO_TBL.Columns.Add("UORG", GetType(String))
            IO_TBL.Columns.Add("ARRIVTIME", GetType(String))
            IO_TBL.Columns.Add("DISTANCE", GetType(Integer))
            IO_TBL.Columns.Add("SEQ", GetType(Integer))
            IO_TBL.Columns.Add("YTODOKECODE", GetType(String))
            IO_TBL.Columns.Add("JSRTODOKECODE", GetType(String))
            IO_TBL.Columns.Add("SHUKABASHO", GetType(String))

            IO_TBL.Columns.Add("DELFLG", GetType(String))
            IO_TBL.Columns.Add("INITYMD", GetType(String))
            IO_TBL.Columns.Add("UPDYMD", GetType(String))
            IO_TBL.Columns.Add("UPDUSER", GetType(String))
            IO_TBL.Columns.Add("UPDTERMID", GetType(String))
            IO_TBL.Columns.Add("RECEIVEYMD", GetType(String))

        End Sub
        ''' <summary>
        ''' 届先ローカルテーブル項目初期化
        ''' </summary>
        ''' <param name="IO_ROW">ローカル行</param>
        ''' <remarks></remarks>
        Public Sub MC006tbl_Init(ByRef IO_ROW As DataRow)

            IO_ROW("LINECNT") = 0
            IO_ROW("OPERATION") = String.Empty
            IO_ROW("TIMSTP") = 0
            IO_ROW("SELECT") = 0
            IO_ROW("HIDDEN") = 0

            IO_ROW("CAMPCODE") = CAMPCODE
            IO_ROW("TORICODE") = String.Empty
            IO_ROW("TORINAMES") = String.Empty
            IO_ROW("TORINAMEL") = String.Empty
            IO_ROW("TODOKECODE") = String.Empty
            IO_ROW("TODOKENAMES") = String.Empty
            IO_ROW("TODOKENAMEL") = String.Empty
            IO_ROW("NAMESK") = String.Empty
            IO_ROW("NAMELK") = String.Empty
            IO_ROW("POSTNUM1") = String.Empty
            IO_ROW("POSTNUM2") = String.Empty
            IO_ROW("ADDR1") = String.Empty
            IO_ROW("ADDR2") = String.Empty
            IO_ROW("ADDR3") = String.Empty
            IO_ROW("ADDR4") = String.Empty
            IO_ROW("TEL") = String.Empty
            IO_ROW("FAX") = String.Empty
            IO_ROW("MAIL") = String.Empty
            IO_ROW("LATITUDE") = String.Empty
            IO_ROW("LONGITUDE") = String.Empty
            IO_ROW("CITIES") = String.Empty
            IO_ROW("MORG") = UORG
            IO_ROW("NOTES1") = String.Empty
            IO_ROW("NOTES2") = String.Empty
            IO_ROW("NOTES3") = String.Empty
            IO_ROW("NOTES4") = String.Empty
            IO_ROW("NOTES5") = String.Empty
            IO_ROW("NOTES6") = String.Empty
            IO_ROW("NOTES7") = String.Empty
            IO_ROW("NOTES8") = String.Empty
            IO_ROW("NOTES9") = String.Empty
            IO_ROW("NOTES10") = String.Empty
            IO_ROW("CLASS") = String.Empty
            IO_ROW("STYMD") = "2000/01/01"
            IO_ROW("ENDYMD") = C_MAX_YMD

            IO_ROW("UORG") = UORG
            IO_ROW("ARRIVTIME") = "00:00:00"
            IO_ROW("DISTANCE") = 0
            IO_ROW("SEQ") = 1000
            IO_ROW("YTODOKECODE") = String.Empty
            IO_ROW("JSRTODOKECODE") = String.Empty
            IO_ROW("SHUKABASHO") = String.Empty

            IO_ROW("DELFLG") = C_DELETE_FLG.ALIVE
            IO_ROW("INITYMD") = String.Empty
            IO_ROW("UPDYMD") = String.Empty
            IO_ROW("UPDUSER") = UPDUSERID
            IO_ROW("UPDTERMID") = UPDTERMID
            IO_ROW("RECEIVEYMD") = String.Empty

        End Sub
        ''' <summary>
        ''' CSVのモードから光英のマスタタイプに変換する
        ''' </summary>
        ''' <param name="I_MODE"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Function convKoueiType(ByVal I_MODE) As String
            Select Case I_MODE
                Case GRW0001KOUEIORDER.KOUEITYPE_PREFIX.JX, GRW0001KOUEIORDER.KOUEITYPE_PREFIX.TG
                    Return GRW0001KOUEIORDER.KOUEITYPE_PREFIX.JXTG
                Case GRW0001KOUEIORDER.KOUEITYPE_PREFIX.COSMO
                    Return GRW0001KOUEIORDER.KOUEITYPE_PREFIX.COSMO
                Case Else
                    Return String.Empty
            End Select
        End Function
    End Class
#End Region
End Namespace

