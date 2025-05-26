using HAUI_LIBOI.Interface;
using HAUI_LIBOI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static QRCoder.PayloadGenerator;
using static System.Net.Mime.MediaTypeNames;

namespace HAUI_LIBOI.A
{
    public class LibraryApiClient : ILibraryOtpApi, ILibraryBorrowingApi, ILibraryBranchApi, ILibraryReservationApi, ILibraryBorrowerNameApi, ILibraryCopyIDDetailApi, ILibraryBorrowingInplaceApi, ILibraryCopyIDsinBorrrowingApi, ILibraryReturnApi
    {
        private readonly HttpClient _client;

        public LibraryApiClient()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("HAUI", "MINHMAX");
            _client.Timeout = TimeSpan.FromSeconds(15);
        }

        //RoomsByEveryBranch
        public async Task<List<LibraryBranchDTO>?> GetRoomsByEveryBranch()
        {
            var response = await _client.GetAsync("http://localhost:800/api/getRoomsbyLibraryBranch");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            var json = await response.Content.ReadAsStringAsync();
            var branches = JsonConvert.DeserializeObject<List<LibraryBranchDTO>>(json);
            return branches;
        }
        //Tên
        public async Task<string?> GetBorrowerNameAsync(string code)
        {
            string url = $"http://localhost:800/api/students/{code}";
            var response = await _client.GetAsync(url);

            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<StudentNameDTO>>(json);
            return result?.FirstOrDefault()?.StudentName;
        }

        //Reservations
        public async Task<List<CopiesDTO>?> getCopyIDsReservation(string studentCode, string roomID)
        {
            string url = $"http://localhost:800/api/reservations/{studentCode}/{roomID}";
            var response = await _client.GetAsync(url);

            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            var reservations = JsonConvert.DeserializeObject<List<CopiesDTO>>(json);
            return reservations;
        }
        //Thông tin sách
        public async Task<CopiesDTO?> GetCopyIDDetail(string copyID)
        {
            var response = await _client.GetAsync($"http://localhost:800/api/copyIDDetail/{copyID}");

            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            var copyIDs = JsonConvert.DeserializeObject<List<CopiesDTO>>(json);
            return copyIDs.FirstOrDefault();
        }
        // OTP
        public async Task<bool> SendOtpAsync(string email)
        {
            var request = new { Email = email };
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("http://localhost:800/api/otp/send", content);
            return response.IsSuccessStatusCode;

        }

        public async Task<bool> VerifyOtpAsync(string email, string otp)
        {
            var verify = new { Email = email, Otp = otp };
            var json = JsonConvert.SerializeObject(verify);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("http://localhost:800/api/otp/verify", content);
            return response.IsSuccessStatusCode;
        }

        // Mượn sách về nhà 
        public async Task<string?> SubmitBorrowingAsync(string studentCode, string email, List<string> copyIDs)
        {
            var borrowDto = new { StudentCode = studentCode, Email = email, CopyIDs = copyIDs };
            var json = JsonConvert.SerializeObject(borrowDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("http://localhost:800/api/borrowings", content);
            if (!response.IsSuccessStatusCode) return null;

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<BorrowResultDTO>(body);
            return result?.BorrowCode;
        }
        // Đọc sách tại chỗ 
        public async Task<string?> SubmitBorrowingInplaceAsync(string studentCode, string email, List<string> copyIDs)
        {
            var borrowDto = new { StudentCode = studentCode, Email = email, CopyIDs = copyIDs };
            var json = JsonConvert.SerializeObject(borrowDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("http://localhost:800/api/borrowings/inplace", content);
            if (!response.IsSuccessStatusCode) return null;

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<BorrowResultDTO>(body);
            return result?.BorrowCode;
        }

        //Lấy list sách đang mượn chưa quá hạn
        public async Task<List<CopiesDTO>?> getCopyIDsinBorrrowing(string studentCode, string roomID)
        {
            string url = $"http://localhost:800/api/copiesinBorrowingbyRoom/{studentCode}/{roomID}";
            var response = await _client.GetAsync(url);

            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            var copyIDs = JsonConvert.DeserializeObject<List<CopiesDTO>>(json);
            return copyIDs;
        }

        //Trả sách
        public async Task<bool> SubmitReturnAsync(string studentCode, string updatedEmail, List<string> copyIDs)
        {
            var returnDto = new { StudentCode = studentCode, UpdatedEmail = updatedEmail, CopyIDs = copyIDs };

            var json = JsonConvert.SerializeObject(returnDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PatchAsync("http://localhost:800/api/return", content);
            return response.IsSuccessStatusCode;

        }
    }
}
