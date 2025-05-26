using HAUI_LIBOI.Interface;
using HAUI_LIBOI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
using HAUI_LIBOI.A;
namespace HAUI_LIBOI
{
    /// <summary>
    /// Interaction logic for AuthorizeInplace.xaml
    /// </summary>
    public partial class AuthorizeInplace : Window
    {
        private OIBorrowingInplace borrowingInplaceForm;
        private readonly ILibraryOtpApi _otpApi;
        private readonly ILibraryBorrowingInplaceApi _borrowingInplaceApi;
        public AuthorizeInplace(OIBorrowingInplace borrowingInplaceForm)
        {
            InitializeComponent();
            this.borrowingInplaceForm = borrowingInplaceForm;
            txt_xn1.Focus();

            var api = new LibraryApiClient();
            _otpApi = api;
            _borrowingInplaceApi = api;
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            bool success = await _otpApi.SendOtpAsync(borrowingInplaceForm.email);

            if (success)
            {
                txt_notice.Text = "OTP đã được gửi thành công và có hiệu lực trong 5 phút!";
                txt_notice.Foreground = Brushes.Green;
            }
            else
            {
                MessageBox.Show("Không thể gửi OTP. Vui lòng thử lại." + success);
                this.Close();
                borrowingInplaceForm.Close();
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

            string email = borrowingInplaceForm.email;
            string studentCode = borrowingInplaceForm.studentCode;
            var selectedBooks = borrowingInplaceForm._selectedBooks;

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

                string? borrowCode = await _borrowingInplaceApi.SubmitBorrowingInplaceAsync(studentCode, email, CopyIDs);
                if (borrowCode == null)
                {
                    MessageBox.Show("Mượn sách thất bại sau khi xác thực.", "Lỗi mượn sách", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                borrowingInplaceForm.SendBillEmail(email, studentCode, borrowCode);

                // Cập nhật lại danh sách còn lại
                borrowingInplaceForm.allCopiesInput = borrowingInplaceForm.allCopiesInput.Where(b => !b.IsSelected).ToList();
                borrowingInplaceForm.BooksDataGrid2.ItemsSource = null;
                borrowingInplaceForm.BooksDataGrid2.ItemsSource = borrowingInplaceForm.allCopiesInput;

                MessageBoxResult result = MessageBox.Show("Mượn sách thành công", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                if (result == MessageBoxResult.OK)
                {
                    this.Close();
                    borrowingInplaceForm.Close();
                    borrowingInplaceForm.oiForm.Show();
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
                borrowingInplaceForm.Close();
            }
        }
    }
}
