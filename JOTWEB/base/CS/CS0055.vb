Option Explicit On

Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Xml.Serialization
Imports System.Collections.Specialized

''' <summary>
'''  GEOCODERクラス
''' </summary>
''' <remarks></remarks>
Public Class CS0055GeoCoder
    ''' <summary>
    ''' リバースジオコードAPI
    ''' </summary>
    ''' <remarks></remarks> 
    Private Const C_YAPL_URI As String = "https://map.yahooapis.jp/geoapi/V1/reverseGeoCoder"
    ''' <summary>
    ''' リバースジオコードAPI APPID
    ''' </summary>
    ''' <remarks></remarks> 
    Private Const C_APPID As String = "dj00aiZpPVBvTkNBSWJSZU5FNCZzPWNvbnN1bWVyc2VjcmV0Jng9Nzg-"
    ''' <summary>
    ''' リバースジオコードAPI 結果ステータス
    ''' </summary>
    ''' <remarks></remarks> 
    Private Enum C_YAPL_STATUS As Integer
        SUCCESS = 200                       '正常終了
        NOTFOUND = 204                      '指定された場所の住所情報が見つからない場合に返されます。
        ERROR_PARAM = 400                   '渡されたパラメータがWeb APIで期待されたものと一致しない場合に返されます。
        INVARID_VALUE = 1004                '緯度・経度の指定が正しくない場合に返されます。
    End Enum

    ''' <summary>
    ''' レスポンスフィールドXSD
    ''' </summary>
    ''' <remarks>このプログラム内では使用しません</remarks> 
    Private Const C_YAPL_XSD As String = "http://olp.yahooapis.jp/OpenLocalPlatform/V1/YDF.xsd"


    ''' <summary>
    ''' 住所情報
    ''' </summary>
    ''' <remarks></remarks> 
    Public Class AddressInfo
        ''' <summary>
        ''' 緯度
        ''' </summary>
        ''' <remarks></remarks> 
        Property Latitude As Decimal
        ''' <summary>
        ''' 経度
        ''' </summary>
        ''' <remarks></remarks> 
        Property Longitude As Decimal

        ''' <summary>
        ''' 住所
        ''' </summary>
        ''' <remarks>１行表記</remarks> 
        Property Address As String
        ''' <summary>
        ''' 住所１
        ''' </summary>
        ''' <remarks>都道府県</remarks> 
        Property Address1 As String
        ''' <summary>
        ''' 住所２
        ''' </summary>
        ''' <remarks>市区町村</remarks> 
        Property Address2 As String
        ''' <summary>
        ''' 住所３
        ''' </summary>
        ''' <remarks>大字/字（丁目）/街区（番地）/（号）</remarks> 
        Property Address3 As String
        ''' <summary>
        ''' 住所４
        ''' </summary>
        ''' <remarks>ビル情報</remarks> 
        Property Address4 As String
        ''' <summary>
        ''' 市区町村コード
        ''' </summary>
        ''' <remarks></remarks> 
        Property CityCode As String
    End Class

    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    ''' <remarks></remarks> 
    Public Sub New()

        'プロパティ初期化
        Initialize()

    End Sub

    ''' <summary>
    ''' 初期化
    ''' </summary>
    ''' <remarks></remarks> 
    Public Sub Initialize()

    End Sub

    ''' <summary>
    ''' 住所取得
    ''' </summary>
    ''' <param name="lat">緯度</param>
    ''' <param name="lon">経度</param>
    ''' <returns>TRUE|FALSE</returns>
    ''' <remarks></remarks> 
    Public Function GetAddress(ByVal lat As Decimal, ByVal lon As Decimal) As AddressInfo

        '入力緯度経度
        If lat = 0D OrElse lon = 0D Then
            PutLog(C_MESSAGE_NO.DLL_IF_ERROR, C_MESSAGE_TYPE.ABORT, String.Format("緯度経度不正({0},{1})", lat, lon))
            Return Nothing
        End If

        Dim info As AddressInfo = New AddressInfo With {
                .Latitude = lat,
                .Longitude = lon,
                .Address = String.Empty,
                .Address1 = String.Empty,
                .Address2 = String.Empty,
                .Address3 = String.Empty,
                .Address4 = String.Empty,
                .CityCode = String.Empty
        }

        'クエリパラメータ作成
        Dim nvc As NameValueCollection = New NameValueCollection From
        {
            {"appid", C_APPID},                     'APPID
            {"lat", lat},                           '緯度
            {"lon", lon}                            '経度
        }
        Dim query As String = String.Join("&", nvc.AllKeys.Select(Function(k) String.Format("{0}={1}", k, nvc(k))))
        'リクエストURI作成
        Dim uri As String = String.Format("{0}?{1}", C_YAPL_URI, query)

        'TLS1.2設定
        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12

        Try
            Dim resYDF As YDF                       'YDFフォーマット

            'サーバリクエスト実行
            Dim webreq As System.Net.HttpWebRequest = WebRequest.Create(uri)
            'サーバーからの応答を受信するためのHttpWebResponseを取得
            Dim webres As System.Net.HttpWebResponse = webreq.GetResponse()

            '応答データを受信するためのStreamを取得
            Using st As System.IO.Stream = webres.GetResponseStream()
                '文字コードを指定して、StreamReaderを作成
                Using sr As New StreamReader(st, Encoding.UTF8)
                    Dim msr = New MemoryStream(Encoding.UTF8.GetBytes(sr.ReadToEnd))
                    'YDFフォーマットデシリアライズ
                    Dim serializer As XmlSerializer = New XmlSerializer(GetType(YDF))
                    resYDF = CType(serializer.Deserialize(msr), YDF)
                End Using
            End Using
            webres.Close()

            '検索結果
            Dim reslut As ResultType = resYDF.Items(0)
            '結果ステータスチェック
            If reslut.Status <> C_YAPL_STATUS.SUCCESS Then
                PutLog(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, String.Format("YOPL取得エラー ({0},{1}) STATUS[{2}] {3}", lat, lon, reslut.Status, reslut.Description))
                Return Nothing
            End If

            '検索結果データ
            Dim feature As FeatureType = resYDF.Items(resYDF.firstResultPosition)
            '住所データ
            Dim dt As PropertyType = feature.Items(0)
            Dim wkAddress As String = ""
            For Each element In dt.Items
                Select Case element.Name
                    Case "Address"                  '住所（1行）
                        info.Address = element.FirstChild.Value
                    Case "AddressElement"           '部分住所情報
                        '住所レベル
                        Dim level As String = element("Level").FirstChild.Value
                        If Not IsNothing(element("Name").FirstChild) Then
                            Dim val As String = element("Name").FirstChild.Value
                            Select Case level
                                Case "prefecture"               '都道府県
                                    info.Address1 = val
                                Case "city"                     '市区町村
                                    info.Address2 = val
                                    info.CityCode = element("Code").FirstChild.Value
                                Case "oaza", "aza", "detail1"   '大字・字（丁目）・街区（番地）
                                    wkAddress &= val
                                Case "Building"                 'ビル情報
                                    info.Address4 = val
                            End Select
                        End If
                    Case Else
                End Select
            Next

            '住所情報
            ' 住居番号（号）が取得できない為、住所（1行）と部分住所情報(大字・字・街区)の差分の残りを設定する
            info.Address3 = info.Address.Substring(info.Address1.Length + info.Address2.Length)

            Return info

        Catch ex As Exception

            PutLog(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, String.Format("YOPL取得エラー ({0},{1})", lat, lon))
            Return Nothing
        End Try

    End Function

    ''' <summary>
    ''' ログ出力
    ''' </summary>
    ''' <remarks></remarks> 
    Private Sub PutLog(ByVal messageNo As String,
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