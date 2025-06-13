//using Microsoft.AspNetCore.Mvc;
//using Todo_List_3.Services;
//using Todo_List_3.Models;
//using Todo_List_3.Enums;

//namespace Todo_List_3.Controllers
//{
//	public class StorageController : Controller
//	{
//		private readonly IStorageSelectionService? _storageSelectionService;

//		public StorageController(IStorageSelectionService? storageSelectionService)
//		{
//			_storageSelectionService = storageSelectionService;
//		}

//		public IActionResult Index()
//		{
//			if (_storageSelectionService == null)
//			{
//				return StatusCode(500, "Storage selection service is not available.");
//			}

//			var currentStorageType = _storageSelectionService.GetCurrentStorageType();

//			var viewModel = new StorageSelectionViewModel
//			{
//				CurrentStorage = currentStorageType
//			};

//			return View(viewModel);
//		}

//		[HttpPost]
//		public IActionResult SelectStorage(StorageType selectedStorageType)
//		{
//			if (_storageSelectionService != null)
//			{
//				_storageSelectionService.SetCurrentStorageType(selectedStorageType);
//			}
//			return RedirectToAction("Index", "Tasks");
//		}

//	}
//}
