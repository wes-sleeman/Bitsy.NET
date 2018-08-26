Friend Class Lexer
    Public ReadOnly Property Current As IToken
        Get
            Return _Current
        End Get
    End Property

    Private ReadOnly Code$
    Private Index As Integer = 0
    Private _Current As IToken

    Sub New(Code As String)
        Me.Code = Code
        Advance()
    End Sub

    Sub Advance()
        _Current = TakeNext()
    End Sub

    Private Function TakeNext() As IToken
        Select Case Code(Index)
            Case "0"c To "9"c
                Return New IntLiteral(TakeWhileLike("#"))

            Case vbCr, vbLf, vbCrLf, vbTab, " "c
                TakeWhileIn(vbCr, vbLf, vbCrLf, vbTab, " "c)
                Return TakeNext()

            Case "a"c To "z"c, "A"c To "Z"c, "_"c
                Dim ident$ = TakeWhileLike("[a-zA-Z_]")
                Try
                    Return New Keyword(ident)
                Catch ex As ArgumentException
                    Return New Variable(ident)
                End Try

            Case "("c, ")"c
                Index += 1
                Return New Paren(Code(Index - 1))

            Case "{"c
                TakeWhileLike("[!}]")
                Index += 1
                Return TakeNext()

            Case "+"c, "-"c, "*"c, "/"c, "%"c, "="c
                Index += 1
                Return New [Operator](Code(Index - 1))

            Case Else
                Throw New ArgumentException($"Illegal character '{Code(Index)}'.")
        End Select
    End Function

    Private Function TakeWhileLike(pattern As String) As String
        Dim retval$ = ""
        Do
            retval &= Code(Index)
            Index += 1
            If Index >= Code.Length Then Exit Do
        Loop While Code(Index) Like pattern
        Return retval
    End Function

    Private Function TakeWhileIn(ParamArray chars() As Char) As String
        Dim retval$ = ""
        Do
            retval &= Code(Index)
            Index += 1
            If Index >= Code.Length Then Exit Do
        Loop While chars.Contains(Code(Index))
        Return retval
    End Function
End Class

#Region "Tokens"
Module Tokens
    Interface IToken
        ReadOnly Property Type As TokenType
        ReadOnly Property Value As String
    End Interface

    Structure IntLiteral
        Implements IToken
        Public ReadOnly Property Type As TokenType Implements IToken.Type
        Public Property Value As String Implements IToken.Value

        Public Sub New(value$)
            Me.Value = value
            Type = TokenType.IntLiteral
        End Sub
    End Structure

    Structure Variable
        Implements IToken
        Public ReadOnly Property Type As TokenType Implements IToken.Type
        Public Property Value As String Implements IToken.Value

        Public Sub New(value$)
            Me.Value = value
            Type = TokenType.Variable
        End Sub
    End Structure

    Structure Whitespace
        Implements IToken
        Public ReadOnly Property Type As TokenType Implements IToken.Type
        Public Property Value As String Implements IToken.Value

        Public Sub New(value$)
            Me.Value = value
            Type = TokenType.Whitespace
        End Sub
    End Structure

    Structure Comment
        Implements IToken
        Public ReadOnly Property Type As TokenType Implements IToken.Type
        Public Property Value As String Implements IToken.Value

        Public Sub New(value$)
            Me.Value = value
            Type = TokenType.Comment
        End Sub
    End Structure

    Structure Keyword
        Implements IToken
        Public ReadOnly Property Type As TokenType Implements IToken.Type
        Public ReadOnly Property Value As String Implements IToken.Value
            Get
                Return [Enum].GetName(GetType(TokenType), Type)
            End Get
        End Property

        Public Sub New(kw$)
            If Not [Enum].TryParse(kw, True, Type) Then
                Throw New ArgumentException($"Invalid keyword {kw}.")
            End If
        End Sub
    End Structure

    Structure Paren
        Implements IToken
        Public ReadOnly Property Type As TokenType Implements IToken.Type
        Public ReadOnly Property Value As String Implements IToken.Value
            Get
                Return [Enum].GetName(GetType(TokenType), Type)
            End Get
        End Property

        Public Sub New(kw As Char)
            Select Case kw
                Case "("c
                    Type = TokenType.LeftParen
                Case ")"c
                    Type = TokenType.RightParen

                Case Else
                    If Not [Enum].TryParse(kw, Type) Then
                        Throw New ArgumentException($"Invalid keyword {kw}.")
                    End If
            End Select
        End Sub
    End Structure

    Structure [Operator]
        Implements IToken
        Public ReadOnly Property Type As TokenType Implements IToken.Type
        Public ReadOnly Property Value As String Implements IToken.Value
            Get
                Return _Kwin
            End Get
        End Property

        Private ReadOnly Property _Kwin As Char

        Public Sub New(kw As Char)
            _Kwin = kw
            Select Case kw
                Case "+"c
                    Type = TokenType.Plus
                Case "-"c
                    Type = TokenType.Dash
                Case "*"c
                    Type = TokenType.Star
                Case "/"c
                    Type = TokenType.Slash
                Case "%"c
                    Type = TokenType.Percent
                Case "="c
                    Type = TokenType.Equals
            End Select
        End Sub
    End Structure

    Enum TokenType
        Whitespace
        Variable
        IntLiteral
        Comment
        LeftParen
        RightParen
        Plus
        Dash
        Star
        Slash
        Percent
        Equals
        Begin
        [End]
        IfP
        IfZ
        IfN
        [Else]
        [Loop]
        [Break]
        Print
        Read
    End Enum
End Module
#End Region