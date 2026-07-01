using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TestManagement.BAL.Hubs;

// Hub cho teacher giám sát realtime một (đề × lớp).
// Teacher mở trang monitor sẽ gọi JoinExamRoom để vào group "monitor-{examId}-{classId}".
[Authorize(Roles = "Teacher,Admin")]
public class ExamMonitorHub : Hub
{
    public async Task JoinExamRoom(int examId, int classId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"monitor-{examId}-{classId}");
    }

    public async Task LeaveExamRoom(int examId, int classId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"monitor-{examId}-{classId}");
    }
}
