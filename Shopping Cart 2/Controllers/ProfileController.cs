using Microsoft.AspNetCore.Http; // للاستفادة من IFormFile
using Microsoft.AspNetCore.Mvc;
using Shopping_Cart_2.Models;
using Microsoft.AspNetCore.Identity;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ProfileController(UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment)
    {
        _userManager = userManager;
        _webHostEnvironment = webHostEnvironment;
    }

    // عند تحميل الصفحة يتم جلب بيانات المستخدم
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user != null)
        {
            var model = new ApplicationUser // نموذج البيانات الشخصية للمستخدم
            {
                Name = user.Name ?? "",
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                City = user.City ?? "",
                Street = user.Street ?? "",
                ProfilePic = user.ProfilePic // عرض الصورة الشخصية
            };

            return View("Index", model);  // عرض البيانات للمستخدم
        }

        // في حالة عدم وجود المستخدم، يمكن توجيه المستخدم إلى صفحة تسجيل الدخول
        return RedirectToPage("/Account/Login", new { area = "Identity" });
    }

    // صفحة تعديل البيانات
    public async Task<IActionResult> EditProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            var model = new ApplicationUser // نموذج البيانات المعدلة
            {
                Name = user.Name ?? "",
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                City = user.City ?? "",
                Street = user.Street ?? "",
                ProfilePic = user.ProfilePic // عرض الصورة الشخصية
            };

            return View(user); // عرض البيانات في صفحة التعديل
        }

        return RedirectToAction("Login", "Account");
    }

    // عند إرسال البيانات المعدلة
    [HttpPost]
    public async Task<IActionResult> EditProfile(ApplicationUser model, IFormFile profilePic)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                // تحديث البيانات فقط إذا كانت مملوءة
                user.Name = !string.IsNullOrEmpty(model.Name) ? model.Name : user.Name;
                user.PhoneNumber = !string.IsNullOrEmpty(model.PhoneNumber) ? model.PhoneNumber : user.PhoneNumber;
                user.City = !string.IsNullOrEmpty(model.City) ? model.City : user.City;
                user.Street = !string.IsNullOrEmpty(model.Street) ? model.Street : user.Street;

                // تعديل صورة البروفايل إذا كانت موجودة
                if (profilePic != null)
                {
                    // التأكد من أن المجلد "uploads" موجود
                    var uploadDirectory = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

                    if (!Directory.Exists(uploadDirectory))
                    {
                        Directory.CreateDirectory(uploadDirectory); // إنشاء المجلد إذا لم يكن موجودًا
                    }

                    // إنشاء مسار جديد للصورة
                    var filePath = Path.Combine(uploadDirectory, profilePic.FileName);

                    // حفظ الصورة في المجلد
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await profilePic.CopyToAsync(stream);
                    }

                    // حفظ مسار الصورة في قاعدة البيانات
                    user.ProfilePic = "/uploads/" + profilePic.FileName;
                }

                // تحديث بيانات المستخدم في قاعدة البيانات
                await _userManager.UpdateAsync(user);

                // إعادة توجيه المستخدم إلى صفحة البروفايل بعد التعديل
                return RedirectToAction("Index");
            }
        }

        // في حالة وجود أخطاء في البيانات
        return View(model);
    }
}
