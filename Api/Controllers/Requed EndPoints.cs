using Microsoft.AspNetCore.Mvc;
using System.Data;
using Api.Service;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Requed_EndPoints : ControllerBase
    {
        #region Mebers
        private readonly IDbConnection _dbConnection;
        private readonly DbService _dbService;
        #endregion
        #region Constructor
        public Requed_EndPoints(IDbConnection dbConnection, DbService service)
        {
            _dbConnection = dbConnection;
            _dbService = service;
        }
        #endregion

        #region API implementation

        [HttpPost("Task 1")]
        public IActionResult Task1()
        {
            // Pobranie produktów
            string url = "https://rekturacjazadanie.blob.core.windows.net/zadanie/Products.csv";
            string localPath = "Products.csv";
            _dbService.Downloadfile(url, localPath);
            // Zapisywanie Productów
            _dbService.GetProducts(localPath);

            //Pobieranie Inventory
            url = "https://rekturacjazadanie.blob.core.windows.net/zadanie/Inventory.csv";
            localPath = "Inventory.csv";
            _dbService.Downloadfile(url, localPath);
            //Zapisywanie Inventory 
            _dbService.GetInventory(localPath);

            //Pobieranie Cen
             url = "https://rekturacjazadanie.blob.core.windows.net/zadanie/Prices.csv";
             localPath = "Prices.csv";
            _dbService.Downloadfile(url, localPath);

            //Zapisywanie Cen
            _dbService.GetPrice(localPath);
            return Ok();
        }

        [HttpGet("{sku}")]
        public IActionResult GetProductInfo(string sku)
        {
            var result = _dbService.GetProductInfo(sku);
            return Ok(result);
        }

        #endregion

        #region Helppers
        [HttpPost("Create Database (if you need)")]
        public IActionResult CreateDatabase()
        {

            _dbService.CreateSQLiteDatabase();
            return Ok();
        }
        #endregion
    }



}
