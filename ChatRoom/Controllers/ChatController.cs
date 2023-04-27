using Microsoft.AspNetCore.Mvc;

namespace ChatRoom.Controllers
{
    public class ChatController : Controller
    {
        public static Dictionary<int, string> Rooms = new Dictionary<int, string>()
        {
            { 1, "Programación" },
            { 2, "Arte/Diseño" },
            { 3, "Automotriz" }
        };

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Room(int room)
        {
            return View("Room", room);
        }
    }
}
