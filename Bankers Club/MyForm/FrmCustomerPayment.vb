Imports DevComponents.DotNetBar

Public Class FrmCustomerPayment
    Dim TransID As String

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        Me.Close()
    End Sub

    Private Sub FrmCustomer_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        Me.Text = "Payment"
    End Sub

    Private Sub FrmCustomer_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.Size = New Point(670, 370)
        ClearAll(TableLayoutPanel1, Highlighter1)
        swPayment.Value = True
        txtBank.Text = "N/A"
        txtCheque.Text = "N/A"
        txtRemarks.Text = "N/A"

        txtSearch.Select()

    End Sub

    Private Sub Discount_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles swPayment.KeyDown
        If e.KeyCode = Keys.Enter Then
            If swPayment.Value = False Then
                swPayment.Value = True
            ElseIf swPayment.Value = True Then
                swPayment.Value = False
            End If
        End If
    End Sub

    Private Sub swPayment_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles swPayment.ValueChanged
        If swPayment.Value = True Then
            txtBank.Enabled = False
            txtCheque.Enabled = False
            txtBank.Text = "N/A"
            txtCheque.Text = "N/A"
            txtRemarks.Text = "N/A"
            txtDiscount.Text = 0
            txtPayment.Clear()
            txtPayment.Select()
        ElseIf swPayment.Value = False Then
            txtBank.Enabled = True
            txtCheque.Enabled = True
            txtBank.Clear()
            txtCheque.Clear()
            txtRemarks.Text = "N/A"
            txtDiscount.Text = 0
            txtPayment.Clear()
            txtBank.Select()
        End If
    End Sub

    Private Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave.Click
        Dim PaidDue As Double
        Dim PaidAmount, DueAmount As Double

        If FillTextBox(TableLayoutPanel1, Highlighter1) = False Then
            Exit Sub
        End If

        DueAmount = CDbl(txtDue.Text)

        PaidAmount = CDbl(txtPayment.Text) + CDbl(txtDiscount.Text)

        If PaidAmount > DueAmount Then
            MessageBoxEx.Show("Payment Amount Should be Equal or " & vbNewLine & "Less Than Due Amount.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Stop)
            Highlighter1.SetHighlightColor(txtPayment, Validator.eHighlightColor.Red)
            txtPayment.Focus()
            Exit Sub
        Else
            PaidDue = DueAmount - PaidAmount
            Highlighter1.SetHighlightColor(txtPayment, Validator.eHighlightColor.None)
        End If


        TransID = AutoIDPreFix("tbl_Customers_Paid", "Paid_ID", PreFix)

        sqlSTR = "INSERT INTO tbl_Customers_Paid(Paid_ID, Cust_ID, Paid_Date, Payment_Type, Bank_Name, Cheque_No, Paid_Taka, Discount, Due_Amt, Remarks, Sales_ID) " & _
                 "VALUES ('" & TransID & "', '" & txtID.Text & "', '" & Format(dtpDate.Value, "yyyy/MM/dd") & "', '" & IIf(swPayment.Value = True, "Cash", "Cheque") & _
                 "', '" & txtBank.Text & "', '" & txtCheque.Text & "', " & CDbl(txtPayment.Text) & ", " & CDbl(txtDiscount.Text) & ", " & CDbl(PaidDue) & ", '" & txtRemarks.Text & "', '0')"
        ExecuteSQLQuery(sqlSTR)

        sqlSTR = "UPDATE tbl_Customers SET Due_Amount = " & CDbl(PaidDue) & " WHERE Cust_ID = '" & txtID.Text & "'"
        ExecuteSQLQuery(sqlSTR)

        '============= tbl_Balance ==================

        Dim PaidAmt As Double
        PaidAmt = CDbl(txtPayment.Text)

        sqlSTR = "SELECT Bal_ID FROM tbl_Balance WHERE Bal_Date='" & Format(Now, "yyyy/MM/dd") & "'"
        ExecuteSQLQuery(sqlSTR)

        If sqlDT.Rows.Count > 0 Then
            sqlSTR = "UPDATE tbl_Balance SET Bal_Amt=Bal_Amt+" & CDbl(PaidAmt) & " WHERE Bal_ID=" & sqlDT.Rows(0)("Bal_ID")
            ExecuteSQLQuery(sqlSTR)
        Else
            sqlSTR = "SELECT Bal_Amt FROM tbl_Balance WHERE Bal_Date<'" & Format(Now, "yyyy/MM/dd") & "' ORDER BY Bal_Date DESC"
            ExecuteSQLQuery(sqlSTR)

            If sqlDT.Rows.Count > 0 Then
                BalAmt = sqlDT.Rows(0)("Bal_Amt")
            Else
                BalAmt = 0
            End If

            sqlSTR = "INSERT INTO tbl_Balance(Bal_Amt, Bal_Date) VALUES(" & CDbl(BalAmt) + CDbl(PaidAmt) & ", '" & Format(Now, "yyyy/MM/dd") & "')"
            ExecuteSQLQuery(sqlSTR)
        End If

        sqlSTR = "SELECT Bal_ID FROM tbl_Balance WHERE Bal_Date>'" & Format(Now, "yyyy/MM/dd") & "'"
        ExecuteSQLQuery(sqlSTR)

        For i As Integer = 0 To sqlDT.Rows.Count - 1
            sqlSTR = "UPDATE tbl_Balance SET Bal_Amt=Bal_Amt+" & CDbl(PaidAmt) & " WHERE Bal_ID=" & sqlDT.Rows(i)("Bal_ID")
            GetSQLQuery(sqlSTR)
        Next

        MessageBoxEx.Show("Record Save Successfully...", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information)

        FrmCustomerPaymentDetails.RefreshCustomerPayment()
        Me.Close()
    End Sub

    Private Sub txtPayment_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtPayment.KeyPress
        NumKey(txtPayment.Text, e)
    End Sub

    Private Sub txtDiscount_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtDiscount.KeyPress
        NumKey(txtDiscount.Text, e)
    End Sub

    Private Sub txtSearch_KeyDown(sender As Object, e As KeyEventArgs) Handles txtSearch.KeyDown
        If e.KeyCode = Keys.Enter Then
            sqlSTR = "SELECT Cust_ID, Cust_Name, Contact_No, Due_Amount FROM tbl_Customers WHERE LEFT(Cust_ID, " & Len(txtSearch.Text) & ")='" & txtSearch.Text & "' OR LEFT(Contact_No, " & Len(txtSearch.Text) & ")='" & txtSearch.Text & "'"
            ExecuteSQLQuery(sqlSTR)

            If sqlDT.Rows.Count > 0 Then
                txtID.Text = sqlDT.Rows(0)("Cust_ID")
                txtName.Text = sqlDT.Rows(0)("Cust_Name")
                txtContact.Text = sqlDT.Rows(0)("Contact_No")
                txtDue.Text = Format(sqlDT.Rows(0)("Due_Amount"), "###,###.00")
            Else
                ClearAll(TableLayoutPanel1, Highlighter1)
                txtSearch.Select()
                MessageBoxEx.Show("Record not found..", "Search", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        End If
    End Sub
End Class