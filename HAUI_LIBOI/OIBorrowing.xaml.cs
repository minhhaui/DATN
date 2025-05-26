using HAUI_LIBOI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Text.RegularExpressions;

using QRCoder;
using System.Drawing.Imaging; // để dùng ImageFormat.Png

//in
using System.IO;
using System.Diagnostics;
using HAUI_LIBOI.Interface;
using HAUI_LIBOI.A;
namespace HAUI_LIBOI
{
    /// <summary>
    /// Interaction logic for OIBorrowing.xaml
    /// </summary>
    public partial class OIBorrowing : Window
    {
        private readonly ILibraryReservationApi _libraryReservationApi;
        private readonly ILibraryBorrowerNameApi _libraryBorrowerNameApi;
        public string roomID;
        public string roomName;
        public string roomType;
        public OI oiForm; // nhận từ OI
                          //   public List<ReservationsDTO> allCopiesReservation = new();
                          //lưu lại để gửi mail
        public List<CopiesDTO> reservations;
        public List<CopiesDTO> _selectedBooks = new();
        public OIBorrowing(string roomID, string roomName, string roomType, OI oi)
        {
            InitializeComponent();
            this.roomID = roomID;
            this.roomName = roomName;
            this.roomType = roomType;
            this.oiForm = oi;
           // LoadStudentCodes();
            txt_tenphong.Text = roomName;
            txt_tong.Visibility = Visibility.Collapsed;

            var api = new LibraryApiClient();
            _libraryReservationApi = api;
            _libraryBorrowerNameApi = api;
            
        }
        //Task<T> = hứa rằng sẽ trả về kiểu T sau khi tác vụ hoàn thành(thường là API, I/O, v.v.).
        private async Task<string?> LoadStudent(string txt)
        {
           return await _libraryBorrowerNameApi.GetBorrowerNameAsync(txt);
        }
        bool isInternalChange = false;
        private async void txt_msv1_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (isInternalChange) return;

            string code = txt_msv.Text.Trim();

            if (string.IsNullOrEmpty(code))
            {
                txt_msv.BorderBrush = Brushes.Red;
                error_msv.Text = "Mã sinh viên không được để trống";
                success_msv.Text = "";
                return;
            }

            try
            {
                var studentName = await LoadStudent(code);

                if (string.IsNullOrEmpty(studentName))
                {
                    txt_msv.BorderBrush = Brushes.Red;
                    error_msv.Text = "Mã sinh viên không hợp lệ hoặc không tồn tại";
                    success_msv.Text = "";
                }
                else
                {
                    txt_msv.ClearValue(Border.BorderBrushProperty);
                    error_msv.Text = "";
                    success_msv.Text = studentName;
                    txt_msv.Foreground = Brushes.White; // Đổi màu chữ sang trắng
                    btn_Tracuu1.Focus();
                }
            }
            catch (Exception ex)
            {
                error_msv.Text = "Lỗi khi gọi API: " + ex.Message;
            }
        }

        private async void btnTraCuu_Click(object sender, RoutedEventArgs e)
        {
            string studentCode = txt_msv.Text.Trim();
            string roomId = roomID;

            if (string.IsNullOrEmpty(studentCode))
            {
                MessageBox.Show("Vui lòng nhập mã sinh viên", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

             reservations = await _libraryReservationApi.getCopyIDsReservation(studentCode, roomId);
          

                if (reservations == null || reservations.Count == 0)
                {
                    MessageBox.Show($"Bạn chưa có đơn đặt sách trên LIBOL tại {roomName}\nVui lòng đặt mượn sách trên web trước khi thao tác với OI", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    BooksDataGrid1.ItemsSource = null;
                    txt_tong.Visibility = Visibility.Collapsed;
                    this.Close();
                    return;
                }

                // Có dữ liệu
                BooksDataGrid1.ItemsSource = reservations;
                BooksDataGrid1.Visibility = Visibility.Visible;
                LibOIImage.Visibility = Visibility.Collapsed;
                txt_tong.Visibility = Visibility.Visible;
                btn_Tracuu1.IsEnabled = false;
                btn_xacnhan1.IsEnabled = true;
        }

        private void BooksDataGrid1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = BooksDataGrid1.SelectedItem as CopiesDTO;
            if (selectedItem == null)
             return;

            // Toggle chọn/bỏ chọn
            selectedItem.IsSelected = !selectedItem.IsSelected;

            // Refresh lại DataGrid để hiển thị checkbox cập nhật
            BooksDataGrid1.Items.Refresh();

            // Cập nhật tổng tiền
            UpdateTongTien();

            // Cập nhật trạng thái các điều khiển
            var anySelected = reservations.Any(b => b.IsSelected);
           // txt_email.IsEnabled = anySelected;
            //btn_xacnhan1.Visibility = anySelected ? Visibility.Visible : Visibility.Collapsed;

            // Bỏ chọn dòng để lần sau chọn lại vẫn được kích hoạt
            BooksDataGrid1.UnselectAll();
        }
        private void UpdateTongTien()
        {
            var list = BooksDataGrid1.ItemsSource as IEnumerable<CopiesDTO>;
            if (list == null) return;

            decimal total = list
                .Where(b => b.IsSelected)
                .Sum(b => b.Price.GetValueOrDefault()); // vì Price là decimal?

            txtTongTien.Text = $"{total:N0} đ";
        }
        public string studentCode;
        public string email;
        private async void btn_xacnhan1_Click(object sender, RoutedEventArgs e)
        {
            studentCode = txt_msv.Text.Trim();
            email = success_email.Text.Trim();

            if (string.IsNullOrEmpty(studentCode))
            {
                MessageBox.Show("Vui lòng nhập mã sinh viên!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var selectedBooks = reservations.Where(b => b.IsSelected).ToList();
            if (!selectedBooks.Any())
            {
                MessageBox.Show("Vui lòng chọn ít nhất 1 cuốn sách!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Vui lòng nhập email!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var result = MessageBox.Show($"Lưu ý: LIBOI sẽ gửi mã OTP đến email {email} để xác nhận mượn sách và lấy email này căn cứ để xác thực quy trình trả sách(nếu có). \nBạn có muốn tiếp tục không?", "Xác nhận Email", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
            {
                txt_email.IsEnabled = true;
                txt_email.Focus();
                success_email.Text = "";
                return;
            }

            _selectedBooks = selectedBooks;
            // Chỉ mở form, chưa gửi OTP ở đây
            var authForm = new Authorize(this);
            authForm.ShowDialog();
        }
        public async void SendBillEmail(string toEmail, string studentCode, string borrowCode)
        {
            try
            {
                var message = new MimeKit.MimeMessage();
                message.From.Add(new MimeKit.MailboxAddress("Thư viện Đại học Công nghiệp Hà Nội", "minhnnhaui@gmail.com"));
                message.To.Add(new MimeKit.MailboxAddress("Sinh viên", toEmail));
                message.Subject = "HOÁ ĐƠN MƯỢN SÁCH VỀ NHÀ";

                var builder = new MimeKit.BodyBuilder
                {
                    HtmlBody = await GenerateBillHtml(studentCode, borrowCode)
                };

                //string pdfPath = GenerateBillPdf(studentCode);
                //bodyBuilder.Attachments.Add(pdfPath); // Gắn vào mail
                //message.Body = bodyBuilder.ToMessageBody();
                string logoPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Image/LibOI.png");
                string qrPath = GenerateQrCode(studentCode); // Hàm tạo mã QR từ studentCode hoặc dữ liệu bất kỳ


                // Gắn logo và QR với CID đúng
                builder.LinkedResources.Add(logoPath).ContentId = "logo-liboi";
                builder.LinkedResources.Add(qrPath).ContentId = "qr-confirm";

                // Đính file PDF vào mail
                //builder.Attachments.Add(GenerateBillPdf(studentCode));
                message.Body = builder.ToMessageBody();


                using var client = new MailKit.Net.Smtp.SmtpClient();
                await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync("minhnnhaui@gmail.com", "mlsw oibd isbh wvkx");
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                //MessageBox.Show("Bạn đã mượn sách thành công. \nLIBOI đã gửi hóa đơn qua email, vui lòng gặp nhân viên để hoàn tất thủ tục mượn sách!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gửi email thất bại: " + ex.Message);
            }
        }
        private async Task<string> GenerateBillHtml(string studentCode, string borrowCode)
        {
            var studentName = await LoadStudent(txt_msv.Text);
            DateTime ngayDangKy = DateTime.Now;
            DateTime hanTra = ngayDangKy.AddDays(100);

            var borrowedBooks = _selectedBooks; // Hoặc lọc theo studentCode nếu cần

            decimal total = borrowedBooks.Sum(b => b.Price?? 0);

            string logoCid = "logo-liboi";
            string qrCid = "qr-confirm";

            return $@"
                <div style='font-family: Arial, sans-serif; max-width: 700px; margin: 0 auto; border: 1px solid #ccc; padding: 20px; border-radius: 10px; color: #333;'>
                    <!-- Header -->
                    <img src='cid:{logoCid}' alt='LibOI Logo' style='height: 80px;' />
                    <h2 style='text-align: center; color: #2c3e50; margin: 0; font-size: 25px;'>HOÁ ĐƠN MƯỢN SÁCH</h2>
                    <p style='text-align: center; color: #000; font-style: italic; font-size: 12px; margin:0;'>Số: {borrowCode}</p>    
                    <p style='text-align: center; color: #000; font-style: italic; font-size: 12px; margin:0;'>Ngày lập: {ngayDangKy:dd/MM/yyyy}</p>
                    <hr style='border: none; border-top: 2px solid #000;' />
                   <!-- <div style= 'display: flex; color: #000;'>
                      <div style='margin-right: 300px;'><strong>Họ và tên:</strong> {studentName}</div>
                      <div><strong>Mã sinh viên:</strong> {studentCode}</div>
                    </div> -->
                    <p style='color: #000;'><strong>Họ và tên:</strong> {studentName}</p>
                    <p style='color: #000;'><strong>Mã sinh viên:</strong> {studentCode}</p>
                    <p style='color: #000;'><strong>Phòng:</strong> {roomName}</p>
                    <h4 style='color: #000;'>Danh sách sách đã mượn:</h4>
                    <table style='width: 100%; border-collapse: collapse; color: #000;'>
                        <thead>
                            <tr style='background-color: #f2f2f2;'>
                                <th style='border: 1px solid #ddd; padding: 8px;'>STT</th>
                                <th style='border: 1px solid #ddd; padding: 8px;'>Mã ĐKCB</th>
                                <th style='border: 1px solid #ddd; padding: 8px;'>Tên sách</th>
                                <th style='border: 1px solid #ddd; padding: 8px;'>Kệ</th>
                                <th style='border: 1px solid #ddd; padding: 8px;'>Tầng</th>
                                <th style='border: 1px solid #ddd; padding: 8px;'>Ô</th>
                                <th style='border: 1px solid #ddd; padding: 8px;'>Giá tiền</th>
                            </tr>
                        </thead>
                        <tbody>
                            {string.Join("", borrowedBooks.Select((book, i) => $@"
                                <tr>
                                    <td style='border: 1px solid #ddd; padding: 8px; text-align: center;'>{i + 1}</td>
                                    <td style='border: 1px solid #ddd; padding: 8px; text-align: center;'>{book.CopyID}</td>
                                    <td style='border: 1px solid #ddd; padding: 8px; text-align: center;'>{book.Title}</td>
                                    <td style='border: 1px solid #ddd; padding: 8px; text-align: center;'>{book.ShelfCode}</td>
                                    <td style='border: 1px solid #ddd; padding: 8px; text-align: center;'>{book.ShelfLevel}</td>
                                    <td style='border: 1px solid #ddd; padding: 8px; text-align: center;'>{book.ShelfPosition}</td>
                                    <td style='border: 1px solid #ddd; padding: 8px; text-align: right;'>{book.Price:N0}</td>
                                </tr>"))}
                        </tbody>
                    </table>

                    <p style='color: #000;'><strong>Tổng tiền sách:</strong> <span style='color: #e74c3c; font-weight: bold;'>{total:N0} đ</span></p>
                    <p style='color: #000;'><strong>Ghi chú:</strong> Vui lòng trả sách trước ngày <span style='color: #e74c3c; font-weight: bold;'>{hanTra:dd/MM/yyyy}</span> để nhận lại tiền cọc và tránh phát sinh phí phạt.</p>
                    <div style='margin-top: 40px; display: flex; justify-content: space-between; align-items: flex-start;'>
                        <!-- Mã xác nhận -->
                        <div style='text-align: center; width: 45%;'>
                            <img src='cid:{qrCid}' alt='QR Code' style='height: 100px; margin-bottom: 5px;' />
                            <div style='font-size: 12px; color: #555;'>Mã sinh viên</div>
                        </div>
                        <div style='text-align: center; font-size: 14px; color: #000; width: 65%; '>
                            <p style='font-weight: bold; margin-bottom: 60px;'>THỦ THƯ XÁC NHẬN</p>
                            <p style='margin: 0;'>Nguyễn Ngọc Minh</p>
                        </div>
                    </div>
                    <p style=""color: gray; font-style: italic;"">(*) Đây là email tự động, vui lòng không trả lời thư này.</p>
                    <div><p style='display:none;'>Token: {Guid.NewGuid()}</p> </div>
                </div>";
        }
        private string GenerateQrCode(string content)
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "QR_" + content + ".png");

            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new QRCode(qrData);
                using (var bitmap = qrCode.GetGraphic(20))
                {
                    bitmap.Save(path, ImageFormat.Png);
                }
            }

            return path;
        }

        private void txt_email_TextChanged(object sender, TextChangedEventArgs e)
        {
            string email = txt_email.Text.Trim();
            if (isInternalChange) return;

            if (string.IsNullOrEmpty(email))
            {
                txt_email.BorderBrush = System.Windows.Media.Brushes.Red;
                error_email.Text = "Email không được để trống";
                success_email.Text = "";
                return;
            }

            // Kiểm tra định dạng email và kết thúc bằng @gmail.com
            string pattern = @"^[a-zA-Z0-9._%+-]+@gmail\.com$";
            if (Regex.IsMatch(email, pattern))
            {
                txt_email.ClearValue(System.Windows.Controls.Border.BorderBrushProperty);
                error_email.Text = "";
                success_email.Text = email;
                isInternalChange = true;

                txt_email.Text = "";
                isInternalChange = false;
                btn_xacnhan1.Focus();
            }
            else
            {
                txt_email.BorderBrush = System.Windows.Media.Brushes.Red;
                success_email.Text = "";
                error_email.Text = "Vui lòng nhập đúng định dạng @gmail.com";
            }
        }

    }   
}
