using System.ComponentModel.DataAnnotations;

namespace ThikanaTenantVerification.ViewModels
{
    public class SystemAdminLoginVM
    {
        [Required]
        [EmailAddress]
        [Display(Name = "ইমেইল")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "পাসওয়ার্ড")]
        public string Password { get; set; }

        [Display(Name = "মনে রাখুন")]
        public bool RememberMe { get; set; }
    }

    public class SystemAdminCreateVM
    {
        [Required]
        [Display(Name = "নাম")]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "ইমেইল")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "পাসওয়ার্ড")]
        [MinLength(8, ErrorMessage = "পাসওয়ার্ড কমপক্ষে ৮ অক্ষর দীর্ঘ হতে হবে")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
            ErrorMessage = "পাসওয়ার্ডে অন্তত একটি বড় হাতের অক্ষর, একটি ছোট হাতের অক্ষর, একটি সংখ্যা এবং একটি বিশেষ অক্ষর থাকতে হবে")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "পাসওয়ার্ড নিশ্চিত করুন")]
        [Compare("Password", ErrorMessage = "পাসওয়ার্ড মিলছে না")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "পদবী")]
        public string Designation { get; set; }

        [Required]
        [Display(Name = "মোবাইল নম্বর")]
        [RegularExpression(@"^01[3-9]\d{8}$", ErrorMessage = "সঠিক মোবাইল নম্বর দিন")]
        public string MobileNumber { get; set; }

        [Display(Name = "এনআইডি নম্বর")]
        public string? NIDNumber { get; set; }

        [Display(Name = "পুলিশ স্টেশন")]
        public string? PoliceStation { get; set; }

        [Display(Name = "থানা")]
        public string? Thana { get; set; }

        [Display(Name = "জেলা")]
        public string? District { get; set; }

        [Display(Name = "বিভাগ")]
        public string? Division { get; set; }

        [Required]
        [Display(Name = "অনুমোদন স্তর")]
        public string AccessLevel { get; set; }
    }

    public class ChangePasswordVM
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "বর্তমান পাসওয়ার্ড")]
        public string CurrentPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "নতুন পাসওয়ার্ড")]
        [MinLength(8, ErrorMessage = "পাসওয়ার্ড কমপক্ষে ৮ অক্ষর দীর্ঘ হতে হবে")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
            ErrorMessage = "পাসওয়ার্ডে অন্তত একটি বড় হাতের অক্ষর, একটি ছোট হাতের অক্ষর, একটি সংখ্যা এবং একটি বিশেষ অক্ষর থাকতে হবে")]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "নতুন পাসওয়ার্ড নিশ্চিত করুন")]
        [Compare("NewPassword", ErrorMessage = "পাসওয়ার্ড মিলছে না")]
        public string ConfirmPassword { get; set; }
    }
}
