Module Parser
    Private Property Lexer As Lexer
    Private outputbuffer As List(Of String)

    Function Parse(Lexer As Lexer) As String()
        outputbuffer = New List(Of String)()
        Parser.Lexer = Lexer
        Program()
        Return outputbuffer.ToArray
    End Function

    Private Sub Program()
        Match(TokenType.Begin)
        Setup()

        Block()

        Match(TokenType.End, False)
        Teardown()
    End Sub

    Private Sub Block()
        Do Until Lexer.Current.Type = TokenType.End OrElse Lexer.Current.Type = TokenType.Else
            Select Case Lexer.Current.Type
                Case TokenType.IfN, TokenType.IfZ, TokenType.IfP
                    [If]()
                Case TokenType.Loop
                    [Loop]()
                Case TokenType.Break
                    [Break]()
                Case TokenType.Print
                    Print()
                Case TokenType.Read
                    Read()
                Case Else
                    Assignment()
            End Select
        Loop
    End Sub

#Region "Expressions"
    Private Sub Expression()
        Term()

        While Lexer.Current.Value Like "[+-]"
            Push()
            Dim op = Lexer.Current.Type
            Match(op)
            Term()
            Pop(op)
        End While
    End Sub

    Private Sub Term()
        SignedFactor()

        While Lexer.Current.Value Like "[*/%]"
            Push()
            Dim op = Lexer.Current.Type
            Match(op)
            Factor()
            Pop(op)
        End While
    End Sub

    Private Sub SignedFactor()
        Dim op = TokenType.Plus
        If Lexer.Current.Value Like "[+-]" Then
            op = Lexer.Current.Type
            Match(op)
        End If

        Factor()

        If op = TokenType.Dash Then
            Emit("Register = -Register")
        End If
    End Sub

    Private Sub Factor()
        If Lexer.Current.Type = TokenType.IntLiteral Then
            Dim int = Match(TokenType.IntLiteral)
            Emit($"Register = {int}")
        ElseIf Lexer.Current.Type = TokenType.Variable Then
            Dim varname = Match(TokenType.Variable)
            Emit($"Register = Variable(""{varname}"")")
        Else
            Match(TokenType.LeftParen)
            Expression()
            Match(TokenType.RightParen)
        End If
    End Sub

    Private Sub Push()
        Emit("Stack.Push(Register)")
    End Sub

    Private Sub Pop(Optional optype As TokenType? = Nothing)
        If optype Is Nothing Then
            Emit("Register = Stack.Pop()")
            Return
        End If
        Dim op$ = ""
        Select Case optype.Value
            Case TokenType.Plus
                op = "+"
            Case TokenType.Dash
                op = "-"
            Case TokenType.Star
                op = "*"
            Case TokenType.Slash
                op = "\"
            Case TokenType.Percent
                op = "Mod"
        End Select
        Emit($"Register = Stack.Pop() {op} Register")
    End Sub
#End Region

    Private Sub [If]()
        Dim condtype = Lexer.Current.Type
        Match(condtype)
        Expression()

        Dim comp = ""
        Select Case condtype
            Case TokenType.IfN
                comp = "<"
            Case TokenType.IfZ
                comp = "="
            Case TokenType.IfP
                comp = ">"
        End Select

        Emit($"If Register {comp} 0 Then")

        Block()

        If Lexer.Current.Type = TokenType.Else Then
            Match(TokenType.Else)
            Emit("Else")
            Block()
        End If

        Match(TokenType.End)
        Emit("End If")
    End Sub

    Private Sub [Loop]()
        Match(TokenType.Loop)
        Emit("Do")

        Block()

        Match(TokenType.End)
        Emit("Loop")
    End Sub

    Private Sub [Break]()
        Match(TokenType.Break)
        Emit("Exit Do")
    End Sub

    Private Sub Print()
        Match(TokenType.Print)
        Expression()
        Emit("Console.WriteLine(Register)")
    End Sub

    Private Sub Read()
        Match(TokenType.Read)
        Dim varname = Match(TokenType.Variable)
        Emit($"Variable(""{varname}"") = ReadIn()")
    End Sub

    Private Sub Assignment()
        Dim varname = Match(TokenType.Variable)
        Match(TokenType.Equals)
        Expression()
        Emit($"Variable(""{varname}"") = Register")
    End Sub

    Private Function Match(Type As TokenType, Optional Advance As Boolean = True) As String
        If Not Lexer.Current.Type = Type Then
            Throw New ArgumentException($"Error: Expected {Type} but received {Lexer.Current.Type}.")
        End If

        Dim retval$ = Lexer.Current.Value
        If Advance Then Lexer.Advance()
        Return retval
    End Function

    Private Sub Emit(output$)
        outputbuffer.Add(vbTab & output)
    End Sub

    Sub Setup()
        Emit("Imports System : Imports System.Collections.Generic
Module Program
    Class VariableDictionary
        Inherits Dictionary(Of String, Integer)

        Default Public Shadows Property Subscript(key As String) As Integer
            Get
                If ContainsKey(key) Then
                    Return Item(key)
                Else
                    Return 0
                End If
            End Get
            Set(val As Integer)
                Item(key) = val
            End Set
        End Property
    End Class

    Dim Register As Integer = 0
    ReadOnly Variable As New VariableDictionary()
    Dim Stack As New Stack(Of Integer)
    Function ReadIn() As Integer
        Dim input = Console.ReadLine()
        Try
            Return Convert.ToInt32(input)
        Catch ex As FormatException
            Return 0
        End Try
    End Function

    Sub Main()

    'BEGIN USER GENERATED CODE")
    End Sub

    Sub Teardown()
        Emit("  'END USER GENERATED CODE

    Console.WriteLine(""Press any key to continue..."")
    Console.ReadKey()
End Sub
End Module")
    End Sub
End Module