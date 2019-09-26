Imports System.IO
Imports BASEDLL
Imports BASEDLL.CS0033AutoNumber

''' <summary>
''' 全社サーバ自動採番(HTTPハンドラ)
''' </summary>
''' <remarks></remarks>
Public Class GRCO0103AUTONUM
    Implements IHttpHandler, IRequiresSessionState

    ''' <summary>
    ''' 自動採番処理
    ''' </summary>
    ''' <param name="context"></param>
    ''' <remarks>セッション変数は連携不可</remarks>
    Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        Dim autoNumber As New CS0033AutoNumber

        Dim WW_POSTitems As New Dictionary(Of String, String)
        Dim WW_SEQTYPE As String = String.Empty
        Dim WW_CAMPCODE As String = String.Empty
        Dim WW_MORG As String = String.Empty
        Dim WW_USERID As String = String.Empty
        '要求KeyWordを取得
        Using WW_Reader As New StreamReader(context.Request.InputStream, Encoding.UTF8)
            While Not WW_Reader.EndOfStream
                Dim WW_ReqSTR As String = WW_Reader.ReadLine()
                WW_ReqSTR.Replace(vbTab, String.Empty)

                Dim WW_POSTitemWk As String() = WW_ReqSTR.Split(C_VALUE_SPLIT_DELIMITER)
                For Each itemWk As String In WW_POSTitemWk
                    Dim WW_POSTitemWk2 As String() = itemWk.Split("=")
                    If WW_POSTitemWk2.Count = 2 Then
                        WW_POSTitems.Add(WW_POSTitemWk2(0).Trim(), WW_POSTitemWk2(1).Trim())
                    End If
                Next
            End While
            '閉じる
            WW_Reader.Close()
        End Using

        ' KeyWord[自動採番タイプ]存在チェック
        If Not WW_POSTitems.TryGetValue(C_POST_KEYWORD.SEQTYPE, WW_SEQTYPE) Then
            context.Response.StatusCode = C_POST_STATUSCODE.ILLEGAL_KEYWORD_ERROR  'エラーリターン(textStatus:errorとなる)
            Exit Sub
        End If
        ' KeyWord[会社コード]存在チェック
        If Not WW_POSTitems.TryGetValue(C_POST_KEYWORD.CAMPCODE, WW_CAMPCODE) Then
            context.Response.StatusCode = C_POST_STATUSCODE.ILLEGAL_KEYWORD_ERROR   'エラーリターン(textStatus:errorとなる)
            Exit Sub
        End If
        ' KeyWord[ユーザID]存在チェック
        If Not WW_POSTitems.TryGetValue(C_POST_KEYWORD.USERID, WW_USERID) Then
            context.Response.StatusCode = C_POST_STATUSCODE.ILLEGAL_KEYWORD_ERROR   'エラーリターン(textStatus:errorとなる)
            Exit Sub
        End If
        ' KeyWord[管理部署]取得のみ
        If Not WW_POSTitems.TryGetValue(C_POST_KEYWORD.MORG, WW_MORG) Then
            WW_MORG = String.Empty
        End If
        ' 自動採番処理
        autoNumber.SEQTYPE = WW_SEQTYPE
        autoNumber.CAMPCODE = WW_CAMPCODE
        autoNumber.MORG = WW_MORG
        autoNumber.USERID = WW_USERID
        autoNumber.getAutoNumberProc()
        If Not isNormal(autoNumber.ERR) Then
            context.Response.StatusCode = autoNumber.ERR_DETAIL
            Exit Sub
        End If
        Dim WW_SEQ = autoNumber.SEQ

        '結果送信
        context.Response.ContentType = "text/plain"
        context.Response.Write(autoNumber.SEQ)

    End Sub

    ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class
