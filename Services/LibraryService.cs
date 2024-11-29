using Lab5prime.Models;
using System.Text;

//adding comment here
namespace Lab5prime.Services
{
    public interface ILibraryService
    {
        Task<List<Book>> GetBooksAsync();
        Task<List<User>> GetUsersAsync();
        Task AddBookAsync(Book book);
        Task EditBookAsync(Book book);
        Task DeleteBookAsync(string isbn);
        Task AddUserAsync(User user);
        Task EditUserAsync(User user);
        Task DeleteUserAsync(int id);
        Task BorrowBookAsync(int userId, string isbn);
        Task ReturnBookAsync(int userId, string isbn);
        Task<Dictionary<int, List<string>>> GetBorrowedBooksAsync();
    }
    public class LibraryService : ILibraryService
    {
        private List<Book> _books = new List<Book>();
        private List<User> _users = new List<User>();
        private Dictionary<int, List<string>> _borrowedBooks = new Dictionary<int, List<string>>();
        private readonly string _booksPath;
        private readonly string _usersPath;
        public LibraryService(IWebHostEnvironment webHostEnvironment)
        {
            _booksPath = Path.Combine(webHostEnvironment.ContentRootPath, "data", "Books.csv");
            _usersPath = Path.Combine(webHostEnvironment.ContentRootPath, "data", "Users.csv");
        }

        private async Task ReadBooksAsync()
        {
            try
            {
                if (File.Exists(_booksPath))
                {
                    var lines = await File.ReadAllLinesAsync(_booksPath);
                    _books = lines.Skip(1).Select(line =>
                    {
                        var parts = SplitCsvLine(line);
                        if (parts.Length >= 4)
                        {
                            return new Book
                            {
                                Id = int.Parse(parts[0].Trim()),
                                Title = parts[1].Trim(),
                                Author = parts[2].Trim(),
                                ISBN = parts[3].Trim()
                            };
                        }
                        return null;
                    }).Where(book => book != null).ToList();
                    Console.WriteLine($"Successfully read {_books.Count} books");
                }
                else
                {
                    Console.WriteLine($"Books file not found at path: {_booksPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading books: {ex.Message}");
            }
        }

        private string[] SplitCsvLine(string line)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            foreach (char c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            result.Add(current.ToString());
            return result.ToArray();
        }

        private async Task WriteBooksAsync()
        {
            var lines = new List<string> { "Id,Title,Author,ISBN" };
            lines.AddRange(_books.Select(b => $"{b.ISBN},{b.Title},{b.Author},{b.Id}"));
            await File.WriteAllLinesAsync(_booksPath, lines);
        }

        private async Task ReadUsersAsync()
        {
            if (File.Exists(_usersPath))
            {
                var lines = await File.ReadAllLinesAsync(_usersPath);
                _users = lines.Skip(1).Select(line =>
                {
                    var parts = line.Split(',');
                    return new User
                    {
                        Id = int.Parse(parts[0]),
                        Name = parts[1],
                        Email = parts[2]
                    };
                }).ToList();
            }
        }

        private async Task WriteUsersAsync()
        {
            var lines = new List<string> { "Id,Name,Email" };
            lines.AddRange(_users.Select(u => $"{u.Id},{u.Name},{u.Email}"));
            await File.WriteAllLinesAsync(_usersPath, lines);
        }

        public async Task<List<Book>> GetBooksAsync()
        {
            await ReadBooksAsync();
            return _books;
        }

        public async Task<List<User>> GetUsersAsync()
        {
            await ReadUsersAsync();
            return _users;
        }

        public async Task AddBookAsync(Book book)
        {
            _books.Add(book);
            await WriteBooksAsync();
        }

        public async Task EditBookAsync(Book book)
        {
            var index = _books.FindIndex(b => b.ISBN == book.ISBN);
            if (index != -1)
            {
                _books[index] = book;
                await WriteBooksAsync();
            }
        }

        public async Task DeleteBookAsync(string isbn)
        {
            _books.RemoveAll(b => b.ISBN == isbn);
            await WriteBooksAsync();
        }

        public async Task AddUserAsync(User user)
        {
            _users.Add(user);
            await WriteUsersAsync();
        }

        public async Task EditUserAsync(User user)
        {
            var index = _users.FindIndex(u => u.Id == user.Id);
            if (index != -1)
            {
                _users[index] = user;
                await WriteUsersAsync();
            }
        }

        public async Task DeleteUserAsync(int id)
        {
            _users.RemoveAll(u => u.Id == id);
            await WriteUsersAsync();
        }

        public async Task BorrowBookAsync(int userId, string isbn)
        {
            if (!_borrowedBooks.ContainsKey(userId))
            {
                _borrowedBooks[userId] = new List<string>();
            }
            _borrowedBooks[userId].Add(isbn);
        }

        public async Task ReturnBookAsync(int userId, string isbn)
        {
            if (_borrowedBooks.ContainsKey(userId))
            {
                _borrowedBooks[userId].Remove(isbn);
            }
        }

        public async Task<Dictionary<int, List<string>>> GetBorrowedBooksAsync()
        {
            return borrowedBooks;
        }
    }


}

