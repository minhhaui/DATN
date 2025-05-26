using HAUI_LIBOI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using HAUI_LIBOI.Interface;
using HAUI_LIBOI.A;
using static QRCoder.PayloadGenerator;
namespace HAUI_LIBOI
{
    /// <summary>
    /// Interaction logic for Authorize.xaml
    /// </summary>
    public partial class Authorize : Window
    {
        private OIBorrowing borrowingForm;
        private readonly ILibraryOtpApi _otpApi;
        private readonly ILibraryBorrowingApi _borrowingApi;

        public Authorize(OIBorrowing borrowingForm)
        {
            InitializeComponent();
            this.borrowingForm = borrowingForm;
            txt_xn1.Focus();
            var api = new LibraryApiClient();
            _otpApi = api;
            _borrowingApi = api;
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            bool success = await _otpApi.SendOtpAsync(borrowingForm.email);

            if (success)
            {
                txt_notice.Text = "OTP đã được gửi thành công và có hiệu lực trong 5 phút!";
                txt_notice.Foreground = Brushes.Green;
            }
            else
            {
                MessageBox.Show("Không thể gửi OTP. Vui lòng thử lại." + success);
                this.Close();
                borrowingForm.Close();
            }
        }
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Chỉ cho phép nhập số
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]$");
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox current = sender as TextBox;
            if (current.Text.Length == 1)
            {
                current.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }
        private async void btn_Xacnhan_Click(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            sb.Append(txt_xn1.Text.Trim());
            sb.Append(txt_xn2.Text.Trim());
            sb.Append(txt_xn3.Text.Trim());
            sb.Append(txt_xn4.Text.Trim());
            sb.Append(txt_xn5.Text.Trim());
            sb.Append(txt_xn6.Text.Trim());
            string otp = sb.ToString();

            string email = borrowingForm.email;
            string studentCode = borrowingForm.studentCode;
            var selectedBooks = borrowingForm._selectedBooks;

            if (string.IsNullOrWhiteSpace(otp))
            {
                MessageBox.Show("Vui lòng nhập mã xác nhận.", "Thiếu OTP", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                bool verify = await _otpApi.VerifyOtpAsync(email, otp);
                if (!verify)
                {
                    MessageBox.Show("Mã xác thực không đúng hoặc đã hết hạn. Vui lòng thử lại sau", "Xác thực thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                    return;
                }

                // Nếu xác thực thành công → tiến hành mượn sách


                List<string> CopyIDs = selectedBooks.Select(b => b.CopyID).ToList();

                string? borrowCode = await _borrowingApi.SubmitBorrowingAsync(studentCode, email, CopyIDs);
                if (borrowCode == null)
                {
                    MessageBox.Show("Mượn sách thất bại sau khi xác thực.", "Lỗi mượn sách", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                borrowingForm.SendBillEmail(email, studentCode, borrowCode);

                // Cập nhật lại danh sách còn lại
                borrowingForm.reservations = borrowingForm.reservations.Where(b => !b.IsSelected).ToList();
                borrowingForm.BooksDataGrid1.ItemsSource = null;
                borrowingForm.BooksDataGrid1.ItemsSource = borrowingForm.reservations;

                MessageBoxResult result = MessageBox.Show("Mượn sách thành công", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                if (result == MessageBoxResult.OK)
                {
                    this.Close();
                    borrowingForm.Close();
                    borrowingForm.oiForm.Show();
                }
            }
            catch (TaskCanceledException)
            {
                MessageBox.Show("Yêu cầu bị quá thời gian. Kiểm tra kết nối mạng hoặc thử lại.", "Timeout", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xác thực: " + ex.Message, "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btn_Huy_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult rs = MessageBox.Show("Bạn chắc chắn muốn huỷ xác thực chứ?", "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (rs == MessageBoxResult.Yes)
            {
                this.Close();
                borrowingForm.Close();
            }
        }
    }
}
