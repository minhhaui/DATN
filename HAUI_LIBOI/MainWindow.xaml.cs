using HAUI_LIBOI.Models;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using HAUI_LIBOI.Interface;
using HAUI_LIBOI.A;
namespace HAUI_LIBOI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ILibraryBranchApi _libraryBranchApi;
        public MainWindow()
        {
            InitializeComponent();
            var api = new LibraryApiClient();
            _libraryBranchApi = api;
            LoadBranchesAsync();
            //var a = new OINavigation();
            //a.Show();
        }
        private async void LoadBranchesAsync()
        {
            List<LibraryBranchDTO> branches = await _libraryBranchApi.GetRoomsByEveryBranch();
            if(branches == null)
            {
                MessageBox.Show("Không có dữ liệu cơ sở thư viện!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                cbo_branch.ItemsSource = branches;
            }
        }
        private void cbo_branch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedBranch = (LibraryBranchDTO)cbo_branch.SelectedItem;
            if (selectedBranch != null)
            {
                cbo_room.ItemsSource = selectedBranch.ListRooms;
                cbo_room.SelectedIndex = -1;
            }
        }
        private void btn_tieptuc_Click(object sender, RoutedEventArgs e)
        {
            var selectedRoom = (RoomsDTO)cbo_room.SelectedItem;
            if (selectedRoom != null)
            {
                string roomID = selectedRoom.RoomID;
                string roomType = selectedRoom.RoomType; // hoặc dùng giá trị cụ thể nếu ko có property Type
                string roomName = selectedRoom.RoomName;

                var form = new OI(roomID, roomName, roomType);
                form.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Chưa định vị!");
            }
        }

    }
}