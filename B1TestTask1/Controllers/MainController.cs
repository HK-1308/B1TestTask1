﻿
using B1TestTask1.Data.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Security.Cryptography;


namespace B1TestTask1.Controllers
{
    public class MainController : Controller
    {
        private const int RANGE_FOR_DATE_GENERATION = 5;
        private const int RANGE_FOR_NUMBER_GENERATION = 100000000;
        private const string LATIN_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        private const string CYRILLIC_CHARS = "АБВГДЕЁЖЗИКЛМНОПРСТУФХЦЧШЩЫЪЬЭЮЯабвгдеёжзиклмнопрстуфхцчшщыъьэюя";
        private const string NUMMERIC_CHARS = "0123456789";
        private const int LENGHT_OF_RANDOM_STRINGS = 10;
        private const int LENGHT_OF_RANDOM_DECIMAL = 8;
        private const int MAX_INTEGER_PART_OF_DECIMAL = 19;
        private const int MIN_INTEGER_PART_OF_DECIMAL = 1;
        private readonly IWebHostEnvironment hostEnvironment;

        public MainController(IWebHostEnvironment hostEnvironment)
        {
            this.hostEnvironment = hostEnvironment;

        }


        public async Task<IActionResult> Index()
        {
            var median = GetMedian();
            var sum = GetSum();
            IndexViewModel indexViewModel = new IndexViewModel();
            indexViewModel.Median = await median;
            indexViewModel.Sum = await sum;
            return await Task.Run(() => View(indexViewModel));
        }

        private async Task<decimal> GetMedian()
        {
            
            return await Task.Run(() =>
            {
                decimal decimalData = new decimal();
                string sqlExpression = $"SELECT AVG(DecimalData)" +
                $" FROM(SELECT DecimalData" +
                $" FROM TestTaskTable" +
                $" ORDER BY DecimalData" +
                $" LIMIT 2 - (SELECT COUNT(*) FROM TestTaskTable) % 2" +
                $" OFFSET(SELECT(COUNT(*) - 1) / 2 FROM TestTaskTable))";


                using (var connection = new SqliteConnection("Data Source=Task.db"))
                {
                    connection.Open();

                    SqliteCommand command = new SqliteCommand(sqlExpression, connection);
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                decimalData = reader.GetDecimal(0);
                            }
                        }
                    }
                }
                return decimalData;
            });
        }

        private async Task<long> GetSum()
        {
            return await Task.Run(() =>
            {
               long longData = new long();
                string sqlExpression = $"SELECT SUM(NumericData)" +
                $" FROM TestTaskTable";


                using (var connection = new SqliteConnection("Data Source=Task.db"))
                {
                    connection.Open();

                    SqliteCommand command = new SqliteCommand(sqlExpression, connection);
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                longData = reader.GetInt64(0);
                            }
                        }
                    }
                }
                return longData;
            });

        }
        //Генерация файлов
        public async Task<IActionResult> GenerateFiles()
        {
            for (int fileNumber = 0; fileNumber < 100; fileNumber++)
            {
                string wwwRootPath = hostEnvironment.WebRootPath;
                var fileName = $"file{fileNumber}.txt";
                string path = Path.Combine(wwwRootPath + "/files/", fileName);
                using (StreamWriter writer = new StreamWriter(path, false, System.Text.Encoding.Default))
                {
                    for (int lineNumber = 0; lineNumber < 1000; lineNumber++)
                    {
                        var resultDate = GenerateDateString();
                        var resultLatinString =  GenerateLatinString();
                        var resultCyrillicString =  GenerateCyrillicString();
                        var resultNumber =  GenerateNumericalString();
                        string resultDecimal = GenerateDecimalString();
                        await writer.WriteLineAsync(resultDate + "||" + resultLatinString + "||" + resultCyrillicString + "||" + resultNumber + "||" + resultDecimal);
                    }
                }
            }
            return RedirectToAction("Index");
        }

        private string GenerateDateString()
        {
            string resultDate = "";
            var currentDate = DateTime.Now;
            var lastDate = new DateTime(currentDate.Year - RANGE_FOR_DATE_GENERATION, currentDate.Month, currentDate.Day);
            var range = currentDate.Subtract(lastDate).Days;
            resultDate = lastDate.AddDays(RandomNumberGenerator.GetInt32(range)).ToString("dd.MM.yyyy");
            return resultDate;
        }

        private string GenerateLatinString()
        {
            string resultLatinString = "";
            for (var index = 0; index < LENGHT_OF_RANDOM_STRINGS; index++)
            {
                resultLatinString += (LATIN_CHARS[RandomNumberGenerator.GetInt32(LATIN_CHARS.Length)]);
            }
            return resultLatinString;
        }

        private string GenerateCyrillicString()
        {
            string resultCyrillicString = "";
            for (var index = 0; index < LENGHT_OF_RANDOM_STRINGS; index++)
            {
                resultCyrillicString += (CYRILLIC_CHARS[RandomNumberGenerator.GetInt32(CYRILLIC_CHARS.Length)]);
            }
            return resultCyrillicString;
        }

        private string GenerateNumericalString()
        {
            long resultNumber = (long)(2 * RandomNumberGenerator.GetInt32(1, ((RANGE_FOR_NUMBER_GENERATION / 2) + 1)));
            return resultNumber.ToString();
        }

        private string GenerateDecimalString()
        {
            string resultDecimal = "";
            int integerPart = RandomNumberGenerator.GetInt32(MIN_INTEGER_PART_OF_DECIMAL, MAX_INTEGER_PART_OF_DECIMAL + 1);
            resultDecimal = integerPart.ToString() + ",";
            for (var index = 0; index < LENGHT_OF_RANDOM_DECIMAL; index++)
            {
                resultDecimal += (NUMMERIC_CHARS[RandomNumberGenerator.GetInt32(NUMMERIC_CHARS.Length)]);
            }
            return resultDecimal;
        }
        //Объединение файлов
        [HttpPost]
        public async Task<IActionResult> MergeFile(string param)
        {
            if (String.IsNullOrEmpty(param)) param = "";
            int countOfDeletedStrings = 0;
            string wwwRootPath = hostEnvironment.WebRootPath;
            var resultFileName = $"ResultFile{Guid.NewGuid().ToString()}.txt";
            string resultFilePath = Path.Combine(wwwRootPath + "/files/", resultFileName);
            using (StreamWriter writer = new StreamWriter(resultFilePath, false, System.Text.Encoding.Default))
            {
                for (int fileNumber = 0; fileNumber < 100; fileNumber++)
                {
                    var fileName = $"file{fileNumber}.txt";
                    string path = Path.Combine(wwwRootPath + "/files/", fileName);
                    using (StreamReader reader = new StreamReader(path))
                    {
                        string? line;
                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            if (line.IndexOf(param) > -1 && param!="")
                            {
                                countOfDeletedStrings++;
                            }
                            else
                            {
                                await writer.WriteLineAsync(line);
                            }
                        }
                    }
                }
            }
            ViewBag.CountOfDeletedStrings = countOfDeletedStrings;
            return View("Index");
        }
        //Импорт файлов в субд
        [HttpPost]
        public async Task<IActionResult> ImportFileToDatabase(IndexViewModel model)
        {
            string wwwRootPath = hostEnvironment.WebRootPath;
            string fileName = Path.GetFileNameWithoutExtension(model.File.FileName);
            string extension = Path.GetExtension(model.File.FileName);
            fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
            string path = Path.Combine(wwwRootPath + "/files/", fileName);
            using (var fileStream = new FileStream(path,FileMode.Create))
            {
                await model.File.CopyToAsync(fileStream);
            }
            using (StreamReader reader = new StreamReader(path))
            {
                long numberOfLines = 0;
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {

                    var lineArray = line.Split("||");
                    decimal decimalRecord = Convert.ToDecimal(lineArray[4]);
                    long numericRecord = Convert.ToInt64(lineArray[3]);

                    await Task.Run(() =>
                    {
                        using (var connection = new SqliteConnection("Data Source=Task.db"))
                        {
                            connection.Open();

                            SqliteCommand command = new SqliteCommand();
                            command.Connection = connection;
                            command.CommandText = $"INSERT INTO TestTaskTable (Date, LatinString, CyrillicString, NumericData, DecimalData) " +
                             $"VALUES (@Date, @LatinString, @CyrillicString, @NumericData, @DecimalData)";
                            command.Parameters.AddWithValue("@Date", lineArray[0]);
                            command.Parameters.AddWithValue("@LatinString", lineArray[1]);
                            command.Parameters.AddWithValue("@CyrillicString", lineArray[2]);
                            command.Parameters.AddWithValue("@NumericData", numericRecord);
                            command.Parameters.AddWithValue("@DecimalData", decimalRecord);
                            command.ExecuteNonQuery();
                        }
                    });
                    
                    //TODO Вывод количества записанных и оставшихкся строк
                    //await dataContext.SaveChangesAsync();
                }
            }

            return View("Index");
        }


    }
}
