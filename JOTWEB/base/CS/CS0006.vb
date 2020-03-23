Option Strict On
Imports System.Data.SqlClient
''' <summary>
''' �R���s���[�^�����݃`�F�b�N
''' </summary>
''' <remarks>�w�肳�ꂽ�[������DB�ɓo�^����Ă��邩�m�F����</remarks>
Public Structure CS0006TERMchk

    ''' <summary>
    ''' �R���s���[�^��
    ''' </summary>
    ''' <value>�m�F����R���s���[�^��</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TERMID() As String

    ''' <summary>
    ''' �[���ݒu���
    ''' </summary>
    ''' <value>��ЃR�[�h</value>
    ''' <returns>�[���ݒu�ꏊ�̉�ЃR�[�h</returns>
    ''' <remarks></remarks>
    Public Property TERMCAMP() As String

    ''' <summary>
    ''' �[���ݒu����
    ''' </summary>
    ''' <value>�����R�[�h</value>
    ''' <returns>�[���ݒu�ꏊ�̕����R�[�h</returns>
    ''' <remarks></remarks>
    Public Property TERMORG() As String

    ''' <summary>
    ''' �Ǘ�����
    ''' </summary>
    ''' <value>�����R�[�h</value>
    ''' <returns>�[���Ǘ��̕����R�[�h</returns>
    ''' <remarks></remarks>
    Public Property MORG() As String

    ''' <summary>
    ''' �G���[�R�[�h
    ''' </summary>
    ''' <value>�G���[�R�[�h</value>
    ''' <returns>0;����A����ȊO�F�G���[</returns>
    ''' <remarks>OK:00000,ERR:00002(Customize),ERR:00003(DBerr),ERR:00005(TERM err)</remarks>
    Public Property ERR() As String


    ''' <summary>
    ''' �\����/�֐���
    ''' </summary>
    ''' <remarks></remarks>
    Public Const METHOD_NAME As String = "CS0006TERMchk"

    ''' <summary>
    ''' �`�F�b�N����
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0006TERMchk()
        '��In PARAM�`�F�b�N
        'PARAM01:�R���s���[�^��
        If IsNothing(TERMID) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite                'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUB�N���X��
            CS0011LOGWRITE.INFPOSI = "TERMID"                            '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT             '���b�Z�[�W�^�C�v
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                            '���O�o��
            Exit Sub
        End If
        '�Z�b�V�����Ǘ�
        Dim sm As New CS0050SESSION
        '****************
        '*** ���ʐ錾 ***
        '****************
        'DataBase�ڑ�����
        Using SQLcon = sm.getConnection
            '���R���s���[�^���̗L���`�F�b�N
            Try


                SQLcon.Open() 'DataBase�ڑ�(Open)

                Dim WW_CNT As Integer = 0
                TERMCAMP = ""
                TERMORG = ""

                'Message����SQL��
                Dim SQLStr As String =
                     "SELECT rtrim(TERMID) as TERMID , rtrim(TERMCAMP) as TERMCAMP , rtrim(TERMORG) as TERMORG , rtrim(MORG) as MORG" _
                   & " FROM  com.OIS0001_TERM " _
                   & " Where TERMID = @P1 " _
                   & "   and STYMD <= @P2 " _
                   & "   and ENDYMD >= @P3 " _
                   & "   and DELFLG <> @P4 "
                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    With SQLcmd.Parameters
                        .Add("@P1", SqlDbType.NVarChar, 30).Value = TERMID
                        .Add("@P2", SqlDbType.Date).Value = Date.Now
                        .Add("@P3", SqlDbType.Date).Value = Date.Now
                        .Add("@P4", SqlDbType.NVarChar, 1).Value = C_DELETE_FLG.DELETE
                    End With
                    Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    If SQLdr.Read Then
                        TERMCAMP = Convert.ToString(SQLdr("TERMCAMP"))
                        TERMORG = Convert.ToString(SQLdr("TERMORG"))
                        MORG = Convert.ToString(SQLdr("MORG"))
                        WW_CNT = 1
                    End If

                    If WW_CNT = 0 Then
                        ERR = C_MESSAGE_NO.SYSTEM_CANNOT_WAKEUP
                    Else
                        ERR = C_MESSAGE_NO.NORMAL
                    End If

                    'Close
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing
                End Using
            Catch ex As Exception

                Dim CS0011LOGWrite As New CS0011LOGWrite                    'LogOutput DirString Get
                CS0011LOGWrite.INFSUBCLASS = METHOD_NAME                    'SUB�N���X��
                CS0011LOGWRITE.INFPOSI = "DB:OIS0001_TERM Select"             '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             '���O�o��

                ERR = C_MESSAGE_NO.DB_ERROR

                Exit Sub

            End Try

        End Using

    End Sub

End Structure
