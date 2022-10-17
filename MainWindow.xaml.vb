Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.Windows.Forms
Imports System.Diagnostics.Process
Imports System.IO

Class MainWindow

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function GetForegroundWindow() As IntPtr
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SetForegroundWindow(ByVal hwnd As Integer) As Integer
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function GetWindowThreadProcessId(ByVal hwnd As IntPtr, ByRef lpdwProcessId As Integer) As Integer
    End Function

    Dim isOK = False
    Dim GamePath = My.Computer.FileSystem.ReadAllText(AppStart() & "\assets\game_path.ini")
    Dim GameArgs = "--launch-product=league_of_legends --launch-patchline=live"
    Dim Username = "**uid**"
    Dim Password = "**upw**"
    Dim ClientId = "RiotClientUx"

    Function AppStart() As String
        Return System.AppDomain.CurrentDomain.BaseDirectory
    End Function
    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        clientStatus.Text = "Process Waiting..."
        processBtn.Text = "Start"

        If Not My.Computer.FileSystem.FileExists(GamePath) Then
            MsgBox("Please select lol client")
            Dim x As New OpenFileDialog
            x.FileName = "RiotClientServices.exe"
            x.Filter = "RiotClientServices.exe|RiotClientServices.exe"
            If x.ShowDialog() = Forms.DialogResult.OK Then
                GamePath = x.FileName
                My.Computer.FileSystem.WriteAllText(AppStart() & "\assets\game_path.ini", GamePath, False, Text.Encoding.UTF8)
            Else
                MsgBox("I can't work without lol client :(")
            End If
        End If

        GameData.Text = "Client: " & GamePath
    End Sub

    Sub Login()
        If (isOK = False) Then
            clientStatus.Text = "Client Waiting..."
            processBtn.IsEnabled = False
            Task.Run(AddressOf KeySender)
        End If
    End Sub

    Private Sub KeySender()
        For Each app As Process In Process.GetProcesses
            If (app.ProcessName = ClientId) Then
                clientStatus.Dispatcher.Invoke(
                    Sub()
                        clientStatus.Text = "Client Detected!"
                    End Sub
                )
                app.Kill()
                Process.Start(GamePath, GameArgs)
                isOK = True
            End If
        Next

        If (isOK = False) Then
            Process.Start(GamePath, GameArgs)
            Thread.Sleep(5000)
            Task.Run(AddressOf KeySender)
        Else
            Task.Run(AddressOf SendLogin)
        End If
    End Sub

    Private Sub SendLogin()
        While isOK = True
            Thread.Sleep(5000)
            Dim fgWin = GetForegroundWindow()
            Dim fgPid As New Integer()
            GetWindowThreadProcessId(fgWin, fgPid)
            Dim proc = System.Diagnostics.Process.GetProcessById(fgPid)
            If (proc.ProcessName = ClientId) Then
                SendKeys.SendWait(Username)
                SendKeys.SendWait("{TAB}")
                SendKeys.SendWait(Password)
                SendKeys.SendWait("{ENTER}")
                clientStatus.Dispatcher.Invoke(
                    Sub()
                        clientStatus.Text = "Login Request Sended To Lol Api!"
                    End Sub
                )
                processBtn.Dispatcher.Invoke(
                    Sub()
                        processBtn.IsEnabled = False
                    End Sub
                )
                isOK = False
            End If
        End While
    End Sub

End Class
