namespace IMS.Controllers
{
    using IMS.APPLICATION.Interface.Services;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/report")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _service;

        public ReportController(IReportService service)
        {
            _service = service;
        }

        [HttpGet("weekly")]
        public async Task<IActionResult> GetWeeklyReport()
        {
            var data = await _service.GetWeeklyReportAsync();
            return Ok(data);
        }
    }
}