﻿namespace InteractiveModeExample.Services
{
    using System.IO;
    using System.Linq;
    using InteractiveModeExample.Models;
    using Newtonsoft.Json;

    public class LibraryService
    {
        private string StorageFilePath => Path.Combine(Directory.GetCurrentDirectory(), "Data.json");

        private void StoreLibrary(Library library)
        {
            var data = JsonConvert.SerializeObject(library);
            File.WriteAllText(StorageFilePath, data);
        }

        public Library GetLibrary()
        {
            if (!File.Exists(StorageFilePath))
                return Library.Empty;

            var data = File.ReadAllText(StorageFilePath);

            return JsonConvert.DeserializeObject<Library>(data) ?? Library.Empty;
        }

        public Book? GetBook(string title)
        {
            return GetLibrary().Books.FirstOrDefault(b => b.Title == title);
        }

        public void AddBook(Book book)
        {
            var updatedLibrary = GetLibrary().WithBook(book);
            StoreLibrary(updatedLibrary);
        }

        public void RemoveBook(Book book)
        {
            var updatedLibrary = GetLibrary().WithoutBook(book);
            StoreLibrary(updatedLibrary);
        }
    }
}