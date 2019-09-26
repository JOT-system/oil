Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Public Class GRT00008WRKINC
    Inherits System.Web.UI.UserControl

    Public Const MAPIDS As String = "T00008S"                        'MAPID(選択)
    Public Const MAPID As String = "T00008"                          'MAPID(実行)

    '○ 共通関数宣言(BASEDLL)
    Private CS0050SESSION As New CS0050SESSION          'セッション情報操作処理

    ''' <summary>
    ''' ワークデータ初期化処理
    ''' </summary>
    ''' <remarks></remarks>
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
    ''' 部署一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE"></param>
    ''' <returns></returns>
    ''' <remarks>車庫、部、事業所</remarks>
    Public Function CreateORGParam(ByVal COMPCODE As String, ByVal ORGCODE As String) As Hashtable

        Dim prmData As New Hashtable
        Dim ORGList As New ListBox

        Dim sb As StringBuilder = New StringBuilder
        sb.Append(" SELECT ")
        sb.Append("   rtrim(A.CODE)        as CODE        , ")
        sb.Append("   rtrim(B.NAMES)       as NAMES       , ")
        sb.Append("   rtrim(A.GRCODE01)    as CATEGORY    , ")
        sb.Append("   rtrim(A.SEQ)         as SEQ ")
        sb.Append(" FROM       M0006_STRUCT A ")
        sb.Append(" INNER JOIN M0002_ORG B ON ")
        sb.Append("         A.CAMPCODE = B.CAMPCODE ")
        sb.Append("   and   A.CODE     = B.ORGCODE ")
        sb.Append("   and   B.STYMD   <= @P5 ")
        sb.Append("   and   B.ENDYMD  >= @P4 ")
        sb.Append("   and   B.DELFLG  <> @P8 ")
        sb.Append(" INNER JOIN M0006_STRUCT C ON ")
        sb.Append("         A.CODE     = C.CODE ")
        sb.Append("   and   A.CAMPCODE = C.CAMPCODE ")
        sb.Append("   and   A.OBJECT   = C.OBJECT ")
        sb.Append("   and   C.STYMD   <= @P5 ")
        sb.Append("   and   C.ENDYMD  >= @P4 ")
        sb.Append("   and   C.DELFLG  <> @P8 ")
        sb.Append("   and   C.STRUCT  IN ( ")
        sb.Append("    SELECT @P2 + '_' + C2.GRCODE02 ")
        sb.Append("    FROM M0006_STRUCT C2 ")
        sb.Append("	WHERE ")
        sb.Append("           C2.OBJECT   = @P1 ")
        sb.Append("     and   C2.STRUCT   = @P2 ")
        sb.Append("     and   C2.CODE     = @P10 ")
        sb.Append("     and   C2.STYMD   <= @P5 ")
        sb.Append("     and   C2.ENDYMD  >= @P4 ")
        sb.Append("     and   C2.DELFLG  <> @P8 ")
        sb.Append("   ) ")
        sb.Append(" WHERE ")
        sb.Append("         A.OBJECT   = @P1 ")
        sb.Append("   and   A.STRUCT   = @P2 ")
        sb.Append("   and   A.STYMD   <= @P5 ")
        sb.Append("   and   A.ENDYMD  >= @P4 ")
        sb.Append("   and   A.DELFLG  <> @P8 ")
        sb.Append("   and   A.CAMPCODE = @P0 ")
        sb.Append("   and   (A.CODE=@P10 OR B.ORGLEVEL < ( ")
        sb.Append("    SELECT ORGLEVEL ")
        sb.Append("    FROM M0002_ORG ")
        sb.Append("    WHERE CAMPCODE=@P0 ")
        sb.Append("    AND   ORGCODE =@P10 ")
        sb.Append("    AND   STYMD   <= @P5 ")
        sb.Append("    AND   ENDYMD  >= @P4 ")
        sb.Append("    AND   DELFLG  <> @P8 ")
        sb.Append("   )) ")
        sb.Append(" GROUP BY A.CODE , B.NAMES , A.GRCODE01 , A.SEQ ")
        sb.Append(" ORDER BY A.SEQ ")

        '○配属部署リストボックス作成()
        Try
            Using SQLcon = CS0050SESSION.getConnection()
                SQLcon.Open()
                Using SQLcmd As New SqlCommand(sb.ToString, SQLcon)
                    Dim PARA0 As SqlParameter = SQLcmd.Parameters.Add("@P0", System.Data.SqlDbType.NVarChar)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
                    Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Date)
                    Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.NVarChar)
                    Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar)
                    PARA0.Value = COMPCODE
                    PARA1.Value = C_ROLE_VARIANT.USER_ORG
                    PARA2.Value = "管轄組織"
                    PARA4.Value = Date.Now
                    PARA5.Value = Date.Now
                    PARA8.Value = C_DELETE_FLG.DELETE
                    PARA10.Value = ORGCODE

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        While SQLdr.Read
                            ORGList.Items.Add(New ListItem(SQLdr("NAMES"), SQLdr("CODE")))
                        End While
                    End Using
                End Using
            End Using

            prmData.Item(C_PARAMETERS.LP_LIST) = ORGList
        Catch ex As Exception
        Finally
        End Try

        CreateORGParam = prmData

    End Function
    'Function createORGParam(ByVal COMPCODE As String, ByVal ORGCODE As String) As Hashtable
    '    Dim prmData As New Hashtable
    '    prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
    '    prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG_CATEGORYS) = New String() {GL0002OrgList.C_CATEGORY_LIST.DEPARTMENT,
    '                                                                                GL0002OrgList.C_CATEGORY_LIST.OFFICE_PLACE,
    '                                                                                GL0002OrgList.C_CATEGORY_LIST.CARAGE
    '                                                                               }
    '    prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_PERMISSION) = C_PERMISSION.UPDATE
    '    prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG) = ORGCODE
    '    prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY_WITH_ORG
    '    Return prmData
    'End Function

End Class