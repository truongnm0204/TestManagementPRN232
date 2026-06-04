using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestManagement.Client.Models.Users;
using TestManagement.Client.Services;

namespace TestManagement.Client.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    public async Task<IActionResult> Index(string? keyword, string? role, bool? isActive, int page = 1, int pageSize = 10)
    {
        var result = await _userService.GetListAsync(keyword, role, isActive, page, pageSize);
        if (!result.Success)
        {
            TempData["Error"] = result.Error;
        }

        return View(new UserListViewModel
        {
            Items = result.Data?.Items ?? new List<UserItemViewModel>(),
            Count = result.Data?.Count,
            Keyword = keyword,
            Role = role,
            IsActive = isActive,
            Page = page,
            PageSize = pageSize
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetForEdit(int id)
    {
        var result = await _userService.GetByIdAsync(id);
        return result.Success && result.Data != null ? Json(result.Data) : BadRequest(result.Error);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserFormViewModel model)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
        {
            TempData["Error"] = "Vui lòng nhập đầy đủ thông tin người dùng.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _userService.CreateAsync(model);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Tạo người dùng thành công." : result.Error;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(UserFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Dữ liệu người dùng không hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _userService.UpdateAsync(model);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Cập nhật người dùng thành công." : result.Error;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, bool isActive)
    {
        var result = await _userService.UpdateStatusAsync(id, isActive);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Cập nhật trạng thái thành công." : result.Error;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _userService.DeleteAsync(id);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Xóa người dùng thành công." : result.Error;
        return RedirectToAction(nameof(Index));
    }
}
