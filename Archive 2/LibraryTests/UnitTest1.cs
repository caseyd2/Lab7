
using Xunit;
using Library.Models;
using Library.Services;

namespace LibraryTests;

public class LibraryServiceTests
{
    [Fact]
    public void ReadBooks_LoadsBooksFromCsv()
    {
        string folder = CreateTestFolder();

        try
        {
            // Arrange
            File.WriteAllText(
                Path.Combine(folder, "Books.csv"),
                "1,Test Book,Test Author,1234567890"
            );

            // Act
            var service = new LibraryService(folder);

            // Assert
            Assert.NotNull(service.GetBook(1));
            Assert.Equal("Test Book", service.GetBook(1)!.Title);
            Assert.Equal("Test Author", service.GetBook(1)!.Author);
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public void AddBook_AddsBook()
    {
        string folder = CreateTestFolder();

        try
        {
            // Arrange
            var service = new LibraryService(folder);
            var book = new Book
            {
                Title = "Test Book",
                Author = "Test Author",
                ISBN = "1234567890"
            };

            // Act
            service.AddBook(book);

            // Assert
            Assert.NotNull(service.GetBook(book.Id));
            Assert.Equal("Test Book", service.GetBook(book.Id)!.Title);
            Assert.Single(File.ReadAllLines(Path.Combine(folder, "Books.csv")));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public void EditBook_UpdatesBook()
    {
        string folder = CreateTestFolder();

        try
        {
            // Arrange
            var service = new LibraryService(folder);
            var book = new Book
            {
                Title = "Original Title",
                Author = "Test Author",
                ISBN = "1234567890"
            };
            service.AddBook(book);

            // Act
            service.EditBook(new Book
            {
                Id = book.Id,
                Title = "Updated Title",
                Author = "Updated Author",
                ISBN = "9876543210"
            });

            // Assert
            Assert.Equal("Updated Title", service.GetBook(book.Id)!.Title);
            Assert.Equal("Updated Author", service.GetBook(book.Id)!.Author);
            Assert.Equal("9876543210", service.GetBook(book.Id)!.ISBN);
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public void DeleteBook_RemovesBook()
    {
        string folder = CreateTestFolder();

        try
        {
            // Arrange
            var service = new LibraryService(folder);
            var book = new Book
            {
                Title = "Book to Delete",
                Author = "Test Author",
                ISBN = "1234567890"
            };
            service.AddBook(book);

            // Act
            service.DeleteBook(book.Id);

            // Assert
            Assert.Null(service.GetBook(book.Id));
            Assert.Empty(File.ReadAllLines(Path.Combine(folder, "Books.csv")));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public void AddUser_AddsUser()
    {
        string folder = CreateTestFolder();

        try
        {
            // Arrange
            var service = new LibraryService(folder);
            var user = new User
            {
                Name = "Test User",
                Email = "test@example.com"
            };

            // Act
            service.AddUser(user);

            // Assert
            Assert.NotNull(service.GetUser(user.Id));
            Assert.Equal("Test User", service.GetUser(user.Id)!.Name);
            Assert.Single(File.ReadAllLines(Path.Combine(folder, "Users.csv")));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public void EditUser_UpdatesUser()
    {
        string folder = CreateTestFolder();

        try
        {
            // Arrange
            var service = new LibraryService(folder);
            var user = new User
            {
                Name = "Original User",
                Email = "original@example.com"
            };
            service.AddUser(user);

            // Act
            service.EditUser(new User
            {
                Id = user.Id,
                Name = "Updated User",
                Email = "updated@example.com"
            });

            // Assert
            Assert.Equal("Updated User", service.GetUser(user.Id)!.Name);
            Assert.Equal("updated@example.com", service.GetUser(user.Id)!.Email);
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public void DeleteUser_RemovesUser()
    {
        string folder = CreateTestFolder();

        try
        {
            // Arrange
            var service = new LibraryService(folder);
            var user = new User
            {
                Name = "User to Delete",
                Email = "delete@example.com"
            };
            service.AddUser(user);

            // Act
            service.DeleteUser(user.Id);

            // Assert
            Assert.Null(service.GetUser(user.Id));
            Assert.Empty(File.ReadAllLines(Path.Combine(folder, "Users.csv")));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public void EditBook_InvalidId_DoesNotChangeBooks()
    {
        string folder = CreateTestFolder();

        try
        {
            // Arrange
            var service = new LibraryService(folder);
            var book = new Book
            {
                Title = "Original Title",
                Author = "Test Author",
                ISBN = "1234567890"
            };
            service.AddBook(book);

            // Act
            service.EditBook(new Book
            {
                Id = 999,
                Title = "Wrong Title",
                Author = "Wrong Author",
                ISBN = "0000000000"
            });

            // Assert
            Assert.Equal("Original Title", service.GetBook(book.Id)!.Title);
            Assert.Null(service.GetBook(999));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public void DeleteBook_InvalidId_KeepsBook()
    {
        string folder = CreateTestFolder();

        try
        {
            // Arrange
            var service = new LibraryService(folder);
            var book = new Book
            {
                Title = "Keep This Book",
                Author = "Test Author",
                ISBN = "1234567890"
            };
            service.AddBook(book);

            // Act
            service.DeleteBook(999);

            // Assert
            Assert.NotNull(service.GetBook(book.Id));
            Assert.Single(File.ReadAllLines(Path.Combine(folder, "Books.csv")));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    private static string CreateTestFolder()
    {
        string folder = Path.Combine(
            Path.GetTempPath(),
            "LibraryTests_" + Guid.NewGuid()
        );

        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, "Books.csv"), "");
        File.WriteAllText(Path.Combine(folder, "Users.csv"), "");

        return folder;
    }
}
