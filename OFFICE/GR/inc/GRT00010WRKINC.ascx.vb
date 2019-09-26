Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

Public Class GRT00010WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "T00010S"       'MAPID(条件)
    Public Const MAPID As String = "T00010"         'MAPID(実行)

    '○ 共通関数宣言(BASEDLL)
    Private CS0050SESSION As New CS0050SESSION              'セッション情報操作処理

    ''' <summary>
    ''' ワークデータ初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Initialize()
    End Sub

    ''' <summary>
    ''' SQL異常対応
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SQLAbnormalityRepair()

        '会社コード
        If WF_SEL_CAMPCODE.Text = Nothing Then
            WF_SEL_CAMPCODE.Text = ""
        End If

        '申請年月
        If WF_SEL_TAISHOYM.Text = Nothing Then
            WF_SEL_TAISHOYM.Text = ""
        End If

        '配属部署
        If WF_SEL_HORG.Text = Nothing Then
            WF_SEL_HORG.Text = ""
        End If

        '承認区分
        If WF_SEL_APPROVALDISPTYPE.Text = Nothing Then
            WF_SEL_APPROVALDISPTYPE.Text = ""
        End If

        'グリッドポジション保管
        If WF_SEL_GRIDPOSITION.Text = Nothing Then
            WF_SEL_GRIDPOSITION.Text = ""
        End If

        '会社
        If WF_T09_CAMPCODE.Text = Nothing Then
            WF_T09_CAMPCODE.Text = ""
        End If

        '配属部署
        If WF_T09_TAISHOYM.Text = Nothing Then
            WF_T09_TAISHOYM.Text = ""
        End If

        '対象年月
        If WF_T09_HORG.Text = Nothing Then
            WF_T09_HORG.Text = ""
        End If

        '従業員
        If WF_T09_STAFFCODE.Text = Nothing Then
            WF_T09_STAFFCODE.Text = ""
        End If

        '従業員名
        If WF_T09_STAFFNAME.Text = Nothing Then
            WF_T09_STAFFNAME.Text = ""
        End If

        '職種区分
        If WF_T09_STAFFKBN.Text = Nothing Then
            WF_T09_STAFFKBN.Text = ""
        End If

        '画面ID
        If WF_T09_MAPID.Text = Nothing Then
            WF_T09_MAPID.Text = ""
        End If

        'MAP変数
        If WF_T09_MAPVARIANT.Text = Nothing Then
            WF_T09_MAPVARIANT.Text = ""
        End If

    End Sub

    ''' <summary>
    ''' 配属部署パラメーター
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <returns></returns>
    ''' <remarks>車庫</remarks>
    Public Function CreateHORGParam(ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0002OrgList.LS_AUTHORITY_WITH.USER
        prmData.Item(C_PARAMETERS.LP_PERMISSION) = C_PERMISSION.UPDATE
        prmData.Item(C_PARAMETERS.LP_ORG_CATEGORYS) = New String() {
            GL0002OrgList.C_CATEGORY_LIST.DEPARTMENT,
            GL0002OrgList.C_CATEGORY_LIST.CARAGE,
            GL0002OrgList.C_CATEGORY_LIST.OFFICE_PLACE}

        CreateHORGParam = prmData

    End Function

    ''' <summary>
    ''' 承認区分パラメーター
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <returns></returns>
    ''' <remarks>車庫</remarks>
    Public Function CreateAPPROVALDISPTYPEParam(ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_FIX_CLASS) = "APPROVALDISPTYPE"

        CreateAPPROVALDISPTYPEParam = prmData

    End Function

    ''' <summary>
    ''' 申請者パラメーター
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CreateStaffCodeParam() As Hashtable

        Dim prmData As New Hashtable
        Dim ApplicantIdList As New ListBox
        Dim SQLcon = CS0050SESSION.getConnection()
        SQLcon.Open()
        Dim SQLcmd As New SqlCommand()
        Dim SQLdr As SqlDataReader = Nothing

        '○ 申請者リストボックス作成
        Try
            '検索SQL文
            Dim SQLStr As String =
                 "SELECT isnull(rtrim(A.APPLICANTID),'')       as APPLICANTID ,             " _
               & "       isnull(rtrim(MB1.STAFFNAMES),'')      as APPLICANTNAMES            " _
               & " FROM  T0009_APPROVALHIST AS A					                        " _
               & " INNER JOIN (select CODE from M0006_STRUCT ORG                            " _
               & "             where ORG.CAMPCODE = @P01                                    " _
               & "              and  ORG.OBJECT   = 'ORG'                                   " _
               & "              and  ORG.STRUCT   = '勤怠管理組織'                          " _
               & "              and  ORG.GRCODE01   = @P02                                  " _
               & "              and  ORG.STYMD   <= @P04                                    " _
               & "              and  ORG.ENDYMD  >= @P04                                    " _
               & "              and  ORG.DELFLG  <> '1'                                     " _
               & "            ) Z3                                                          " _
               & "   ON    Z3.CODE      = A.SUBCODE                                         " _
               & "  LEFT JOIN MB001_STAFF MB1   						                    " _
               & "    ON MB1.CAMPCODE     	               = A.CAMPCODE 				    " _
               & "   and MB1.STAFFCODE     	               = A.APPLICANTID 			        " _
               & "   and MB1.STYMD                        <= @P04       				    " _
               & "   and MB1.ENDYMD                       >= @P04 			        	    " _
               & "   and MB1.DELFLG                       <> '1' 						    " _
               & " WHERE A.CAMPCODE     	               = @P01                           " _
               & "   and A.APPLYDATE                      >= @P03		                    " _
               & "   and A.STATUS                         <> '03'		                    " _
               & "   and A.DELFLG                         <> '1'		                    " _
               & " GROUP BY A.APPLICANTID ,MB1.STAFFNAMES                                   "

            SQLcmd = New SqlCommand(SQLStr, SQLcon)
            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.Date)
            Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Date)
            PARA01.Value = WF_SEL_CAMPCODE.Text
            PARA02.Value = WF_SEL_HORG.Text
            PARA03.Value = WF_SEL_TAISHOYM.Text & "/01"
            PARA04.Value = Date.Now
            SQLdr = SQLcmd.ExecuteReader()

            While SQLdr.Read
                ApplicantIdList.Items.Add(New ListItem(SQLdr("APPLICANTNAMES"), SQLdr("APPLICANTID")))
            End While

            prmData.Item(C_PARAMETERS.LP_LIST) = ApplicantIdList
        Finally
            If Not IsNothing(SQLdr) Then
                SQLdr.Close()
                SQLdr = Nothing
            End If

            SQLcmd.Dispose()
            SQLcmd = Nothing

            SQLcon.Close()
            SQLcon.Dispose()
            SQLcon = Nothing
        End Try

        CreateStaffCodeParam = prmData

    End Function

End Class