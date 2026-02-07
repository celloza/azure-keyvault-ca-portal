using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CaManager.Models;

namespace CaManager.Controllers;

    /// <summary>
    /// Controller for the home page.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Redirects to the Certificates controller.
        /// </summary>
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Certificates");
        }


        /// <summary>
        /// Displays the error page.
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
}
