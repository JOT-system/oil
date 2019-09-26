Imports System.IO
Imports BASEDLL
Imports OFFICE.GRIS0005LeftBox

Public Class GRMA0006AJAX
    Implements IHttpHandler, IRequiresSessionState
    ''' <summary>
    ''' 一覧変更時に呼ばれ、名称取得を行う。
    ''' </summary>
    ''' <param name="context"></param>
    ''' <remarks></remarks>
    Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        '共通宣言
        '*共通関数宣言(BASEDLL)
        Dim CS0050SESSION As New CS0050SESSION              'セッション情報操作処理


        '★★★ セッション情報（ユーザ）未設定時の処理(ログオンへ画面遷移)　★★★ 
        '  ※直接URL指定で起動した場合、ログオン画面へ遷移
        If CS0050SESSION.USERID = "" Then
            'エラーリターン(textStatus:errorとなる)
            context.Response.StatusCode = 300
            Exit Sub
        End If
        '〇画面より引数パラメータの取得
        Dim WF_INPARAM As String = context.Request.Form.Get("INPARAM")
        Dim WF_ACTION As String = context.Request.Form.Get("ACTION")
        Dim WF_COMPANY As String = context.Request.Form.Get("COMPANY")
        Dim WF_ROLE As String = context.Request.Form.Get("ROLE")

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = WF_COMPANY
        prmData.Item(C_PARAMETERS.LP_ROLE) = WF_ROLE

        Dim O_TEXT As String = String.Empty
        Dim O_RTN As String = String.Empty

        Using leftview As New GRIS0005LeftBox
            Dim work As New GRMA0006WRKINC
            Try
                Select Case WF_ACTION
                    Case "CAMPCODE"             '会社コード
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, WF_INPARAM, O_TEXT, O_RTN, prmData)
                    Case "UORG"                 '運用部署
                        prmData = work.CreateUORGParam(work.WF_SEL_CAMPCODE.Text)
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, WF_INPARAM, O_TEXT, O_RTN, prmData)
                    Case "SHARYOTYPE"           '統一車番(上)
                        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "SHARYOTYPE"
                        leftview.CodeToName(WF_INPARAM, O_TEXT, O_RTN, prmData)
                    Case "TSHARYOF", "TSHARYOB", "TSHARYOBB"             '統一車番
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CARCODE, WF_INPARAM, O_TEXT, O_RTN, prmData)
                    Case "MANGOILTYPE"          '油種
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_OILTYPE, WF_INPARAM, O_TEXT, O_RTN, prmData)
                    Case "MANGOWNCONT"          '契約区分
                        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "MANGOWNCONT"
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, WF_INPARAM, O_TEXT, O_RTN, prmData)
                    Case "MANGSUPPL"            '庸車会社
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CUSTOMER, WF_INPARAM, O_TEXT, O_RTN, prmData)
                    Case "SUISOKBN"             '水素フラグ
                        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "SUISOKBN"
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, WF_INPARAM, O_TEXT, O_RTN, prmData)
                    Case "OILKBN"               '油種区分（勤怠用）
                        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "OILPAYKBN"
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, WF_INPARAM, O_TEXT, O_RTN, prmData)
                    Case "SHARYOKBN"            '車両区分（勤怠用）
                        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "SHARYOKBN"
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, WF_INPARAM, O_TEXT, O_RTN, prmData)
                    Case "DELFLG"               '削除
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, WF_INPARAM, O_TEXT, O_RTN, prmData)
                End Select
            Catch ex As Exception
                O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
                context.Response.StatusCode = 300
                Exit Sub
            End Try
        End Using

        context.Response.Write(O_TEXT)

        context.Response.StatusCode = 200
    End Sub

    ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class