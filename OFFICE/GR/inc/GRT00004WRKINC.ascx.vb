Imports System.IO

Public Class GRT00004WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "T00004S"                       'MAPID(選択)
    Public Const MAPID As String = "T00004"                         'MAPID(実行)

    Public Const C_TORICODE_JX As String = "0005700000"             '取引先コード（JX)
    Public Const C_TORICODE_COSMO As String = "0094000000"          '取引先コード（COSMO)

    Public Const C_CAMPCODE_NJS As String = "04"                    '会社コード(NJS)

    Public Const C_PRODUCT_OIL As String = "01"                     '石油
    Public Const C_PRODUCT_WHITEOIL As String = "11"                '白油

    Public Const C_KOUEI_CLASS_CODE As String = "T00004_KOUEIORG"   '光英連携可否判定用FIXVAL KEY
    Public Const C_DIR_KOUEI As String = "KOUEI"                    '光英連携ディレクトリ名(LOCAL)
    Public Const C_DIR_KOUEI_ORDER As String = "order"              '光英連携配車ディレクトリ名
    Public Const C_KOUEI_ORDER_FILE_SERCH As String = "*_*_*.csv"   '光英連携配車ファイルSearchPattern

    ''' <summary>
    ''' FTPターゲット(S0028_FTPFILES設定値)
    ''' </summary>
    Private ReadOnly FTP_LIST As String() = {
        "配車データ受信JX",
        "配車データ受信JXTG",
        "配車データ受信COSMO"
    }

    Private sm As New CS0050SESSION

    ''' <summary>
    ''' ワークデータ初期化処理
    ''' </summary>
    ''' <remarks>EMPTY以外</remarks>]
    Public Sub initialize()
    End Sub

    ''' <summary>
    ''' コントロールオブジェクト取得
    ''' </summary>
    ''' <param name="I_FIELD" >コントロール名称</param>
    ''' <returns >Control</returns>
    ''' <remarks>マスターページ内コンテンツ領域(contents1)が対象</remarks>
    Public Function getControl(ByVal I_FIELD As String) As Control
        Try
            Return Page.Master.FindControl("contents1").FindControl(I_FIELD)
        Catch ex As Exception
            ' 指定コントロール不明
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' 検索条件ワークテーブル用カラム設定
    ''' </summary>
    ''' <param name="iTbl"></param>
    ''' <remarks></remarks>
    Sub PARMtbl_ColumnsAdd(ByRef iTbl As DataTable)

        If iTbl.Columns.Count = 0 Then
        Else
            iTbl.Columns.Clear()
        End If

        iTbl.Columns.Add("LINECNT", GetType(Integer))
        iTbl.Columns.Add("CAMPCODE", GetType(String))
        iTbl.Columns.Add("SHUKODATEF", GetType(String))
        iTbl.Columns.Add("SHUKODATET", GetType(String))
        iTbl.Columns.Add("SHUKADATEF", GetType(String))
        iTbl.Columns.Add("SHUKADATET", GetType(String))
        iTbl.Columns.Add("TODOKEDATEF", GetType(String))
        iTbl.Columns.Add("TODOKEDATET", GetType(String))
        iTbl.Columns.Add("ORDERORG", GetType(String))
        iTbl.Columns.Add("SHIPORG", GetType(String))
        iTbl.Columns.Add("OILTYPE", GetType(String))
        iTbl.Columns.Add("KOUEILOADFILE", GetType(String))
    End Sub
    ''' <summary>
    ''' 固定値マスタから一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE"></param>
    ''' <param name="FIXCODE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function createFIXParam(ByVal COMPCODE As String, Optional ByVal FIXCODE As String = "") As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_FIX_CLASS) = FIXCODE
        Return prmData
    End Function

    ''' <summary>
    ''' 取引先一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function createTORIParam(ByVal COMPCODE As String, Optional ByVal ORGCODE As String = Nothing) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG) = ORGCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0003CustomerList.LC_CUSTOMER_TYPE.OWNER
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_PERMISSION) = C_PERMISSION.INVALID
        createTORIParam = prmData
    End Function

    ''' <summary>
    ''' 部署一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function createORGParam(ByVal COMPCODE As String, ByVal ISORDER As Boolean) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        If ISORDER Then
            ' 受注部署
            prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG_CATEGORYS) = New String() {GL0002OrgList.C_CATEGORY_LIST.CARAGE}
        Else
            ' 出荷部署
            prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG_CATEGORYS) = New String() {GL0002OrgList.C_CATEGORY_LIST.CARAGE}
        End If
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_PERMISSION) = C_PERMISSION.INVALID
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0002OrgList.LS_AUTHORITY_WITH.USER
        createORGParam = prmData
    End Function

    ''' <summary>
    ''' 届先一覧取得用パラメータ設定
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE">部署コード</param>
    ''' <param name="SHIPCODE">取引先コード</param>
    ''' <param name="CLASSCODE">区分コード</param>
    ''' <returns>検索条件一覧</returns>
    ''' <remarks></remarks>
    Function createDistinationParam(ByVal COMPCODE As String, ByVal ORGCODE As String, ByVal SHIPCODE As String, ByVal CLASSCODE As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG) = ORGCODE

        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_CUSTOMER) = SHIPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_CLASSCODE) = CLASSCODE

        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0004DestinationList.LC_CUSTOMER_TYPE.ALL
        Return prmData
    End Function

    ''' <summary>
    ''' 品名一覧取得用パラメータ設定
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE" >部署コード</param>
    ''' <returns>検索条件一覧</returns>
    ''' <remarks></remarks>
    Function createGoodsParam(ByVal COMPCODE As String, ByVal ORGCODE As String, Optional ByVal isMaster As Boolean = False) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG_COMP) = COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG) = ORGCODE
        If isMaster Then
            '品名名称にコード表示追加
            prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0006GoodsList.LC_GOODS_TYPE.GOODS_MST
        Else
            prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0006GoodsList.LC_GOODS_TYPE.GOODS
        End If

        Return prmData
    End Function

    ''' <summary>
    ''' 品名1一覧取得用パラメータ設定
    ''' </summary>
    ''' <param name="COMPCODE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function createGoods1Param(ByVal COMPCODE As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE

        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0006GoodsList.LC_GOODS_TYPE.GOODS1_MST

        Return prmData
    End Function

    ''' <summary>
    ''' 品名2一覧取得用パラメータ設定
    ''' </summary>
    ''' <param name="COMPCODE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function createGoods2Param(ByVal COMPCODE As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE

        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0006GoodsList.LC_GOODS_TYPE.GOODS2_MST

        Return prmData
    End Function
    ''' <summary>
    ''' 社員一覧取得用パラメータ設定
    ''' </summary>
    ''' <param name="COMPCODE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function createSTAFFParam(ByVal COMPCODE As String, ByVal ORGCODE As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        If Not String.IsNullOrEmpty(ORGCODE) Then
            prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG) = ORGCODE

        End If
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0005StaffList.LC_STAFF_TYPE.DRIVER
        Return prmData
    End Function

    ''' <summary>
    ''' 業務車番一覧取得用パラメータ設定
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE">部署コード</param>
    ''' <returns>検索条件一覧</returns>
    ''' <remarks></remarks>
    Function createWorkLorryParam(ByVal COMPCODE As String, ByVal ORGCODE As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG) = ORGCODE

        Return prmData
    End Function

    ''' <summary>
    ''' 統一車番一覧取得用パラメータ設定
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ISFRONT">前方車両フラグ</param>
    ''' <returns>検索条件一覧</returns>
    ''' <remarks></remarks>
    Function createCarCodeParam(ByVal COMPCODE As String, ByVal ISFRONT As Boolean) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        If ISFRONT Then
            prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0007CarList.LC_LORRY_TYPE.FRONT
        Else
            prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0007CarList.LC_LORRY_TYPE.REAR
        End If

        Return prmData
    End Function
    ''' <summary>
    ''' 統一車番一覧取得用パラメータ設定
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <returns>検索条件一覧</returns>
    ''' <remarks></remarks>
    Function createCarCodeParam(ByVal COMPCODE As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        Return prmData
    End Function


    ''' <summary>
    '''光英ファイル取得処理
    ''' </summary>
    ''' <param name="orgCode">対象部署</param>
    ''' <param name="dicDataFile">受信ファイルリスト</param>
    ''' <param name="O_RTN">エラーコード</param>
    ''' <param name="IsReadOnly">参照のみ</param>
    ''' <remarks></remarks>
    Public Sub GetKoueiFile(ByVal orgCode As String,
                            ByRef dicDataFile As Dictionary(Of String, List(Of FileInfo)),
                            ByRef O_RTN As String,
                            Optional ByVal IsReadOnly As Boolean = False)

        If IsNothing(dicDataFile) Then
            dicDataFile = New Dictionary(Of String, List(Of FileInfo))
        Else
            dicDataFile.Clear()
        End If

        'FTPコントロール
        Dim control As FtpControl = Nothing

        Try
            control = New FtpControl
            control.IsReadOnly = IsReadOnly

            For Each target In FTP_LIST
                'FTPリクエスト実行
                control.Request(target, orgCode)
                If Not isNormal(control.ERR) Then
                    'O_RTN = control.ERR
                    'Exit Sub
                    'FTP受信エラーでもエラーとしない
                End If
            Next

            '受信済みローカル光英ファイル取得
            Dim koueiPath As String = Path.Combine(sm.UPLOAD_PATH, C_DIR_KOUEI, orgCode, C_DIR_KOUEI_ORDER)
            Dim localDir = New DirectoryInfo(koueiPath)
            '[koueiType]_[kijundate]_[受信日時].csv
            Dim localFiles = localDir.GetFiles(C_KOUEI_ORDER_FILE_SERCH)
            If localFiles.Count = 0 Then
                Exit Sub
            End If

            For Each file In localFiles
                'ファイル名の2項目目は基準日（区切り文字:"_"）
                Dim wk = file.Name.Split("_")
                Dim kijundate = wk(1)
                '基準日別の世代別ファイル一覧作成
                If Not dicDataFile.ContainsKey(kijundate) Then
                    dicDataFile.Add(kijundate, New List(Of FileInfo))
                End If
                dicDataFile(kijundate).Add(file)
            Next

        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.SYSTEM_ADM_ERROR
        Finally
            If Not IsNothing(control) Then
                control.Close()
            End If
        End Try
    End Sub

End Class