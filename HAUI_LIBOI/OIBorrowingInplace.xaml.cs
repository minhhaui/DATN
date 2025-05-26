using HAUI_LIBOI.Models;
using Newtonsoft.Json;
using QRCoder;
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
using System.Windows.Threading;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
using QRCoder;
using System.Drawing.Imaging;
using HAUI_LIBOI.Interface; // để dùng ImageFormat.Png
using HAUI_LIBOI.A;
namespace HAUI_LIBOI
{
    /// <summary>
    /// Interaction logic for OIBorrowingInplace.xaml
    /// </summary>
    public partial class OIBorrowingInplace : Window
    {
       // private DispatcherTimer refreshTimer;

        //private void StartAutoRefresh()
        //{
        //    refreshTimer = new DispatcherTimer();
        //    refreshTimer.Interval = TimeSpan.FromSeconds(1); // gọi mỗi 10 giây
        //    refreshTimer.Tick += async (sender, e) =>
        //    {
        //        await LoadCopies();
        //        await LoadCopiesBorrowingInplacebyRoom();
        //    };
        //    refreshTimer.Start();
        //}
        private readonly ILibraryBorrowerNameApi _libraryBorrowerNameApi;
        private readonly ILibraryCopyIDDetailApi _libraryCopyIDDetailApi;
        public List<CopiesDTO> allCopiesInput = new(); //đẩy vào datagrid
        public List<CopiesDTO> _selectedBooks = new(); //thao tác với Email
        private string roomID;
        private string roomName;
        public OI oiForm; // nhận từ OI
        public OIBorrowingInplace(string roomID, string roomName, string roomType, OI oi)
        {
            InitializeComponent();
            this.roomID = roomID;
            this.roomName = roomName;
            this.oiForm = oi;
            //LoadStudentCodes();
            //LoadCopiesinRoom();
            //StartAutoRefresh();
            txt_tenphong.Text = roomName;
            var api = new LibraryApiClient();
            _libraryBorrowerNameApi = api;
            _libraryCopyIDDetailApi = api;
        }

        //Task<T> = hứa rằng sẽ trả về kiểu T sau khi tác vụ hoàn thành(thường là API, I/O, v.v.).
        private async Task<string?> LoadStudent(string txt)
        {
            return await _libraryBorrowerNameApi.GetBorrowerNameAsync(txt);
        }
        private async Task<CopiesDTO?> LoadCopyID(string txt)
        {
            return await _libraryCopyIDDetailApi.GetCopyIDDetail(txt);
        }


        bool isInternalChange = false;
        private async void txt_msv_TextChanged(object sender, TextChangedEventArgs e)
        {
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
                    txt_dkcb.Focus();
                }
            }
            catch (Exception ex)
            {
                error_msv.Text = "Lỗi khi gọi API: " + ex.Message;
            }
        }
        private async void txt_dkcb_TextChanged(object sender, TextChangedEventArgs e)
        {
            string code = txt_dkcb.Text.Trim();

            if (string.IsNullOrEmpty(code)) return;
            try 
            {
                // Gọi API lấy chi tiết bản in
                var copyDetail = await LoadCopyID(code);
                if (copyDetail == null)
                {
                    txt_dkcb.BorderBrush = Brushes.Red;
                    error_dkcb.Text = "Mã ĐKCB không tồn tại trong hệ thống.";
                    success_dkcb.Text = "";
                    return;
                }

                // So sánh roomName
                if (copyDetail.RoomName != roomName)
                {
                    txt_dkcb.ClearValue(Border.BorderBrushProperty);
                    error_dkcb.Text = "";
                    MessageBox.Show($"Bản in không được định vị tại {roomName}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txt_dkcb.Text = "";
                    return;
                }
                if (copyDetail.RoomName == roomName && copyDetail.Status == "Unavailable")
                {
                    txt_dkcb.ClearValue(Border.BorderBrushProperty);
                    error_dkcb.Text = "";
                    MessageBox.Show($"Bản in chưa được thao tác tại trạm OI.Return \nVui lòng thông báo cho thủ thư hoặc để tại kệ chờ xử lý!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txt_dkcb.Text = "";
                    return;
                }

                // Hợp lệ => xóa viền đỏ, thêm vào DataGrid nếu chưa có
                txt_dkcb.ClearValue(Border.BorderBrushProperty);
                error_dkcb.Text = "";

                if (!allCopiesInput.Any(x => x.CopyID == code))
                {
                    allCopiesInput.Add(copyDetail);
                    BooksDataGrid2.ItemsSource = null;
                    BooksDataGrid2.ItemsSource = allCopiesInput;
                    success_dkcb.Text = code;
                    BooksDataGrid2.Visibility = Visibility.Visible;
                    LibOIImage.Visibility = Visibility.Collapsed;
                }
                else
                {
                    MessageBox.Show($"Mã ĐKCB \"{code}\" đã được ghi nhận trước đó.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    success_dkcb.Text = "";
                }
                txt_dkcb.Text = "";
            }
            catch (Exception ex)
            {
                error_dkcb.Text = "Lỗi khi gọi API: " + ex.Message;
            }
            
        }
        //private void BooksDataGrid2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    var selectedItem = BooksDataGrid2.SelectedItem as CopiesDTO;
        //    if (selectedItem == null)   
        //        return;

        //    // Toggle chọn/bỏ chọn
        //    selectedItem.IsSelected = !selectedItem.IsSelected;

        //    // Refresh lại DataGrid để hiển thị checkbox cập nhật
        //    BooksDataGrid2.Items.Refresh();

        //    // Cập nhật trạng thái các điều khiển
        //    var anySelected = allCopiesInput.Any(b => b.IsSelected);
        //    txt_email.IsEnabled = anySelected;
        //    btn_xacnhan.IsEnabled = anySelected;

        //    // Bỏ chọn dòng để lần sau chọn lại vẫn được kích hoạt
        //    BooksDataGrid2.UnselectAll();
        //}

        private void txt_email_TextChanged(object sender, TextChangedEventArgs e)
        {
            string email = txt_email.Text.Trim();
            if (isInternalChange) return;

            if (string.IsNullOrEmpty(email))
            {
                txt_email.BorderBrush = System.Windows.Media.Brushes.Red;
                error_email.Text = "Vui lòng nhập email dạng @gmail.com";
                success_email.Text = "";
                btn_xacnhan.IsEnabled = false;
                return;
            }

            // Kiểm tra định dạng email và kết thúc bằng @gmail.com
            string pattern = @"^[a-zA-Z0-9._%+-]+@gmail\.com$";
            if (Regex.IsMatch(email, pattern))
            {
                txt_email.ClearValue(Border.BorderBrushProperty);
                error_email.Text = "";
                success_email.Text = email;
                btn_xacnhan.IsEnabled = true;
                isInternalChange = true;

                txt_email.Text = "";
                isInternalChange = false;
                btn_xacnhan.Focus();
            }
            else
            {
                txt_email.BorderBrush = System.Windows.Media.Brushes.Red;
                success_email.Text = "";
                error_email.Text = "Vui lòng nhập đúng định dạng @gmail.com";
                btn_xacnhan.IsEnabled = false;
            }
        }
        public string studentCode;
        public string email;
        private async void btn_xacnhan_Click(object sender, RoutedEventArgs e)
        {
            studentCode = txt_msv.Text.Trim();
            email = success_email.Text.Trim();
            //var selectedBooks = allCopiesInput;

            if (string.IsNullOrEmpty(studentCode))
            {
                MessageBox.Show("Vui lòng nhập mã sinh viên!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            //if (!selectedBooks.Any())
            //{
            //    MessageBox.Show("Vui lòng chọn ít nhất 1 cuốn sách!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            //    return;
            //}
            if (allCopiesInput.Count == 0)
            {
                MessageBox.Show("Vui lòng scan ít nhất 1 bản in muốn mượn!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
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

            _selectedBooks = allCopiesInput;
            // Chỉ mở form, chưa gửi OTP ở đây
            var authForm = new AuthorizeInplace(this);
            authForm.ShowDialog();
        }
        public async void SendBillEmail(string toEmail, string studentCode, string borrowCode)
        {
            try
            {
                var message = new MimeKit.MimeMessage();
                message.From.Add(new MimeKit.MailboxAddress("Thư viện Đại học Đại học Công nghiệp Hà Nội", "minhnnhaui@gmail.com"));
                message.To.Add(new MimeKit.MailboxAddress("Sinh viên", toEmail));
                message.Subject = "PHIẾU XÁC NHẬN MƯỢN SÁCH TẠI CHỖ";
                
                var builder = new MimeKit.BodyBuilder
                {
                    HtmlBody = await GenerateBillHtml(studentCode, borrowCode),
                };

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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gửi email thất bại: " + ex.Message);
            }
        }
        private async Task<string> GenerateBillHtml(string studentCode, string borrowCode)
        {
            //var student = allStudents.FirstOrDefault(s => s.StudentCode == studentCode);
            //string studentName = student?.UserName ?? "Không rõ";
            var studentName = await LoadStudent(txt_msv.Text);
            DateTime ngayDangKy = DateTime.Now;
            var borrowedBooks = _selectedBooks; // Hoặc lọc theo studentCode nếu cần

           // decimal total = borrowedBooks.Sum(b => b.Price ?? 0);

            string logoCid = "logo-liboi";
            string qrCid = "qr-confirm";

            return $@"
                <div style='font-family: Arial, sans-serif; max-width: 700px; margin: 0 auto; border: 1px solid #ccc; padding: 20px; border-radius: 10px; color: #333;'>
                    <img src='cid:{logoCid}' alt='LibOI Logo' style='height: 80px;' />
                    <h2 style='text-align: center; color: #2c3e50; margin: 0; font-size: 25px;'>PHIẾU XÁC NHẬN MƯỢN SÁCH TẠI CHỖ</h2>
                    <p style='text-align: center; color: #000; font-style: italic; font-size: 12px; margin:0;'>Số: {borrowCode}</p>
                    <p style='text-align: center; color: #000; font-style: italic; font-size: 12px; margin:0;'>Ngày lập: {ngayDangKy:dd/MM/yyyy}</p>
                    <hr style='border: none; border-top: 2px solid #000;' />
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
                                <th style='border: 1px solid #ddd; padding: 8px;'>Giá tiền</th>
                            </tr>
                        </thead>
                        <tbody>
                            {string.Join("", borrowedBooks.Select((book, i) => $@"
                                <tr>
                                    <td style='border: 1px solid #ddd; padding: 8px; text-align: center;'>{i + 1}</td>
                                    <td style='border: 1px solid #ddd; padding: 8px; text-align: center;'>{book.CopyID}</td>
                                    <td style='border: 1px solid #ddd; padding: 8px; text-align: center;'>{book.Title}</td>
                                    <td style='border: 1px solid #ddd; padding: 8px; text-align: right;'>{book.Price:N0}</td>
                                </tr>"))}
                        </tbody>
                    </table>
                    <p style='color: #000;'><strong>Ghi chú:</strong><span style='color: #e74c3c; font-weight: bold;'> Vui lòng thao tác trả sách với LIBOI trước khi ra về!</span></p>
                    <div style='margin-top: 40px; display: flex; justify-content: space-between; align-items: flex-start;'>
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
    }
}
