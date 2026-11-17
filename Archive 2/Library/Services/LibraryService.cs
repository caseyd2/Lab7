
using Library.Models;

namespace Library.Services;

public class LibraryService
{
    private readonly string BooksPath;
    private readonly string UsersPath;

    private readonly List<Book> books = new List<Book>();
    private readonly List<User> users = new List<User>();
    private readonly Dictionary<User, List<Book>> borrowedBooks = new Dictionary<User, List<Book>>();
    private readonly object _lock = new object();

    public LibraryService(string? dataDirectory = null)
    {
        string folder = dataDirectory ?? "./Data";

        BooksPath = Path.Combine(folder, "Books.csv");
        UsersPath = Path.Combine(folder, "Users.csv");

        ReadBooks();
        ReadUsers();
    }

    public Dictionary<Book, int> GetBooks()
    {
        Dictionary<Book, int> booksDict = new Dictionary<Book, int>();

        lock (_lock)
        {
            var bookGroups = books.GroupBy(b => b.ISBN)
                .Select(bookGroup => new
                {
                    Book = bookGroup.First(),
                    Count = bookGroup.Count(b => !IsBorrowed(b))
                });

            foreach (var group in bookGroups)
            {
                booksDict[group.Book] = group.Count;
            }
        }

        return booksDict;
    }

    public List<User> GetUsers()
    {
        return users;
    }

    public Book? GetBook(int id)
    {
        return books.FirstOrDefault(b => b.Id == id);
    }

    public User? GetUser(int id)
    {
        return users.FirstOrDefault(u => u.Id == id);
    }

    public void AddBook(Book book)
    {
        lock (_lock)
        {
            book.Id = books.Count == 0 ? 1 : books.Max(b => b.Id) + 1;
            books.Add(book);
            WriteBooks();
        }
    }

    public void EditBook(Book updated)
    {
        lock (_lock)
        {
            var book = books.FirstOrDefault(b => b.Id == updated.Id);
            if (book == null) return;

            book.Title = updated.Title;
            book.Author = updated.Author;
            book.ISBN = updated.ISBN;
            WriteBooks();
        }
    }

    public int CountCopies(string isbn)
    {
        return books.Count(b => b.ISBN == isbn);
    }

    public void DeleteBook(int id)
    {
        lock (_lock)
        {
            books.RemoveAll(b => b.Id == id);

            foreach (var borrowed in borrowedBooks.Values)
            {
                borrowed.RemoveAll(b => b.Id == id);
            }

            WriteBooks();
        }
    }

    public void DeleteAllCopies(string isbn)
    {
        lock (_lock)
        {
            books.RemoveAll(b => b.ISBN == isbn);

            foreach (var borrowed in borrowedBooks.Values)
            {
                borrowed.RemoveAll(b => b.ISBN == isbn);
            }

            WriteBooks();
        }
    }

    public void AddUser(User user)
    {
        lock (_lock)
        {
            user.Id = users.Count == 0 ? 1 : users.Max(u => u.Id) + 1;
            users.Add(user);
            WriteUsers();
        }
    }

    public void EditUser(User updated)
    {
        lock (_lock)
        {
            var user = users.FirstOrDefault(u => u.Id == updated.Id);
            if (user == null) return;

            user.Name = updated.Name;
            user.Email = updated.Email;
            WriteUsers();
        }
    }

    public void DeleteUser(int id)
    {
        lock (_lock)
        {
            var user = users.FirstOrDefault(u => u.Id == id);
            if (user == null) return;

            users.Remove(user);
            borrowedBooks.Remove(user);
            WriteUsers();
        }
    }

    private bool IsBorrowed(Book book)
    {
        return borrowedBooks.Values.Any(borrowed => borrowed.Contains(book));
    }

    public Dictionary<User, List<Book>> GetBorrowedBooks()
    {
        return borrowedBooks;
    }

    public List<Book> GetBorrowedBooks(int userId)
    {
        var user = users.FirstOrDefault(u => u.Id == userId);

        if (user == null || !borrowedBooks.ContainsKey(user))
        {
            return new List<Book>();
        }

        return borrowedBooks[user];
    }

    public int CountAvailableCopies(string isbn)
    {
        lock (_lock)
        {
            return books.Count(b => b.ISBN == isbn && !IsBorrowed(b));
        }
    }

    public void BorrowBook(int userId, string isbn)
    {
        lock (_lock)
        {
            var user = users.FirstOrDefault(u => u.Id == userId);
            var copy = books.FirstOrDefault(b => b.ISBN == isbn && !IsBorrowed(b));

            if (user == null || copy == null) return;

            if (!borrowedBooks.ContainsKey(user))
            {
                borrowedBooks[user] = new List<Book>();
            }

            borrowedBooks[user].Add(copy);
        }
    }

    public void ReturnBook(int userId, int bookId)
    {
        lock (_lock)
        {
            var user = users.FirstOrDefault(u => u.Id == userId);

            if (user == null || !borrowedBooks.ContainsKey(user))
            {
                return;
            }

            borrowedBooks[user].RemoveAll(b => b.Id == bookId);
        }
    }

    private void ReadBooks()
    {
        try
        {
            foreach (string line in File.ReadLines(BooksPath))
            {
                string[] fields = line.Replace("\"", "").Split(',');

                if (fields.Length >= 4)
                {
                    Book b = new Book
                    {
                        Id = int.Parse(fields[0].Trim()),
                        Title = string.Join(",", fields[1..^2]).Trim(),
                        Author = fields[^2].Trim(),
                        ISBN = fields[^1].Trim()
                    };

                    books.Add(b);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    private void ReadUsers()
    {
        try
        {
            foreach (string line in File.ReadLines(UsersPath))
            {
                string[] fields = line.Replace("\"", "").Split(',');

                if (fields.Length >= 3)
                {
                    User u = new User
                    {
                        Id = int.Parse(fields[0].Trim()),
                        Name = string.Join(",", fields[1..^1]).Trim(),
                        Email = fields[^1].Trim()
                    };

                    users.Add(u);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    private void WriteBooks()
    {
        var lines = books.Select(b => string.Join(",",
            b.Id,
            b.Title.Replace("\"", ""),
            b.Author.Replace("\"", "").Replace(",", ""),
            b.ISBN.Replace("\"", "").Replace(",", "")));

        File.WriteAllLines(BooksPath, lines);
    }

    private void WriteUsers()
    {
        var lines = users.Select(u => string.Join(",",
            u.Id,
            u.Name.Replace("\"", ""),
            u.Email.Replace("\"", "").Replace(",", "")));

        File.WriteAllLines(UsersPath, lines);
    }
}
