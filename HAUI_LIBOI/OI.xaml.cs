using HAUI_LIBOI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace HAUI_LIBOI
{
    /// <summary>
    /// Interaction logic for OI.xaml
    /// </summary>
    public partial class OI : Window
    {
        public string roomID;
        public string roomName;
        public string roomType;
        public OI(string roomID, string roomName, string roomType)
        {
            InitializeComponent();
            this.roomID = roomID;
            this.roomName = roomName;
            this.roomType = roomType;
            txt_tenphong.Text = roomName;
            //if (roomType == "Reading")
            //{
            //    btn_muonvenha.IsEnabled = false;
            //}
            //else if (roomType == "Borrowing")
            //{
            //    btn_doctaicho.IsEnabled = false;
            //}
            //txt_tenphong.Text = roomName;
        }

        private void btn_doctaicho_Click(object sender, RoutedEventArgs e)
        {
            if (roomType == "Reading")
            {
                this.Hide(); //ẩn 
                var form = new OIBorrowingInplace(roomID, roomName, roomType, this);
                form.ShowDialog(); // Đợi đến khi đóng Borrowing
                this.Show(); // // quay lại nếu borrowingForm đóng xong
            }
            else MessageBox.Show("Chức năng 'Mượn sách tại chỗ' không khả dụng cho phòng mượn","Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void btn_muonvenha_Click(object sender, RoutedEventArgs e)
        {
            if (roomType == "Borrowing")
            {
                this.Hide(); //ẩn 
                var form = new OIBorrowing(roomID, roomName, roomType, this);
                form.ShowDialog(); // Đợi đến khi đóng Borrowing
                this.Show(); // // quay lại nếu borrowingForm đóng xong
            }
            else MessageBox.Show("Chức năng 'Mượn sách mang về' không khả dụng cho phòng đọc","Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void btn_Trasach_Click(object sender, RoutedEventArgs e)
        {
            this.Hide(); //ẩn 
            var form = new OIReturn(roomID, roomName, roomType,this);
            form.ShowDialog(); // Đợi đến khi đóng Borrowing
            this.Show(); // // quay lại nếu borrowingForm đóng xong
        }
    }
}
