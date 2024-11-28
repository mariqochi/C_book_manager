

//Book_Manager
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

public class User
{
    public string Username { get; set; }
    public string PasswordHash { get; set; }

    public User(string username, string passwordHash)
    {
        Username = username;
        PasswordHash = passwordHash;
    }
}

public class Book
{
    public string Title { get; set; }
    public string Author { get; set; }
    public int YearOfPublication { get; set; }

    public Book(string title, string author, int yearOfPublication)
    {
        Title = title;
        Author = author;
        YearOfPublication = yearOfPublication;
    }

    public void DisplayBookInfo()
    {
        Console.WriteLine($"Title: {Title}");
        Console.WriteLine($"Author: {Author}");
        Console.WriteLine($"Year of Publication: {YearOfPublication}");
    }
}

public class BookManager
{
    private List<Book> books;
    private string filePath;

    public BookManager(string directoryPath)
    {
        filePath = Path.Combine(directoryPath, "books.json");
        books = LoadBooks();
    }

    private List<Book> LoadBooks()
    {
        if (File.Exists(filePath))
        {
            var jsonData = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<List<Book>>(jsonData) ?? new List<Book>();
        }
        return new List<Book>();
    }

    private void SaveBooks()
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var jsonData = JsonConvert.SerializeObject(books, Formatting.Indented);
        File.WriteAllText(filePath, jsonData);
    }

    public void AddBook(string title, string author, int yearOfPublication)
    {
        Book newBook = new Book(title, author, yearOfPublication);
        books.Add(newBook);
        SaveBooks();
        Console.WriteLine("Book added successfully.");
    }

    public void DisplayAllBooks()
    {
        if (books.Count == 0)
        {
            Console.WriteLine("No books available.");
            return;
        }

        foreach (var book in books)
        {
            book.DisplayBookInfo();
            Console.WriteLine();
        }
    }

    public void SearchBook(string searchQuery)
    {
        var foundBooks = books.Where(b =>
            b.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
            b.Author.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)).ToList();

        if (foundBooks.Count == 0)
        {
            Console.WriteLine("No books found with that title or author.");
        }
        else
        {
            foreach (var book in foundBooks)
            {
                book.DisplayBookInfo();
                Console.WriteLine();
            }
        }
    }
}

public class UserManager
{
    private List<User> users;
    private string usersFilePath;

    public UserManager(string directoryPath)
    {
        usersFilePath = Path.Combine(directoryPath, "users.json");
        users = LoadUsers();
    }

    private List<User> LoadUsers()
    {
        if (File.Exists(usersFilePath))
        {
            var jsonData = File.ReadAllText(usersFilePath);
            return JsonConvert.DeserializeObject<List<User>>(jsonData) ?? new List<User>();
        }
        return new List<User>();
    }

    private void SaveUsers()
    {
        var jsonData = JsonConvert.SerializeObject(users, Formatting.Indented);
        File.WriteAllText(usersFilePath, jsonData);
    }

    public bool RegisterUser(string username, string password, string confirmPassword)
    {
        // Check if passwords match
        if (password != confirmPassword)
        {
            Console.WriteLine("Passwords do not match. Please try again.");
            return false;
        }

        // Loop to allow re-entering a unique username
          username = username.Trim();

    // Loop to allow re-entering a unique username
    while (users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
    {
        Console.WriteLine("Username already exists. Please choose another one.");
        Console.Write("Choose a username: ");
        username = Console.ReadLine().Trim(); // Allow the user to input a new username
    }
        // Hash the password before saving it
        string passwordHash = HashPassword(password);
        users.Add(new User(username, passwordHash));
        SaveUsers();
        Console.WriteLine("User registered successfully.");
        return true;
    }

    public bool LoginUser(string username, string password)
    {
        var user = users.FirstOrDefault(u => u.Username == username);
        if (user != null && VerifyPassword(password, user.PasswordHash))
        {
            return true;
        }
        Console.WriteLine("Invalid username or password.");
        return false;
    }

    private string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    private bool VerifyPassword(string password, string storedHash)
    {
        string hash = HashPassword(password);
        return hash == storedHash;
    }
}
}

class Program
{
    static void Main(string[] args)
    {
        string directoryPath = @"C:\Users\marina.kochiashvili\Desktop\HomeworksC#\book_manager"; // Your desired directory
        UserManager userManager = new UserManager(directoryPath);
        BookManager bookManager = new BookManager(directoryPath);

        bool isRunning = true;

        // User registration and login
        Console.Clear();
        Console.WriteLine("Welcome to the Book Manager!");

        if (!UserLogin(userManager))
        {
            Console.WriteLine("Exiting the application.");
            return;
        }

        while (isRunning)
        {
            Console.Clear();
            Console.WriteLine("Book Management System");
            Console.WriteLine("1. Add a new book");
            Console.WriteLine("2. View all books");
            Console.WriteLine("3. Search for a book by title or author");
            Console.WriteLine("4. Exit");
            Console.Write("Please select an option (1-4): ");

            string userInput = Console.ReadLine();

            switch (userInput)
            {
                case "1":
                    AddNewBook(bookManager);
                    break;

                case "2":
                    bookManager.DisplayAllBooks();
                    Pause();
                    break;

                case "3":
                    SearchBook(bookManager);
                    break;

                case "4":
                    isRunning = false;
                    Console.WriteLine("Exiting the application.");
                    break;

                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    Pause();
                    break;
            }
        }
    }

    static bool UserLogin(UserManager userManager)
    {
        Console.WriteLine("Login or Register:");
        Console.WriteLine("1. Login");
        Console.WriteLine("2. Register");
        Console.Write("Choose an option: ");
        string choice = Console.ReadLine();

        string username, password, confirmPassword;

        if (choice == "1")
        {
            Console.Write("Username: ");
            username = Console.ReadLine();
            Console.Write("Password: ");
            password = Console.ReadLine();

            if (userManager.LoginUser(username, password))
            {
                return true;
            }
        }
        else if (choice == "2")
        {
            bool registrationSuccess = false;
            while (!registrationSuccess)
            {
                Console.Write("Choose a username: ");
                username = Console.ReadLine();
                Console.Write("Choose a password: ");
                password = Console.ReadLine();

                Console.Write("Confirm your password: ");
                confirmPassword = Console.ReadLine();

                registrationSuccess = userManager.RegisterUser(username, password, confirmPassword);
            }

            return true;
        }

        return false;
    }

    static void AddNewBook(BookManager bookManager)
    {
        Console.Clear();
        Console.WriteLine("Add a New Book");

        Console.Write("Enter book title: ");
        string title = Console.ReadLine().Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            Console.WriteLine("Title cannot be empty.");
            Pause();
            return;
        }

        Console.Write("Enter author name: ");
        string author = Console.ReadLine().Trim();
        if (string.IsNullOrWhiteSpace(author))
        {
            Console.WriteLine("Author cannot be empty.");
            Pause();
            return;
        }

        int yearOfPublication;
        Console.Write("Enter year of publication: ");
        while (!int.TryParse(Console.ReadLine(), out yearOfPublication) || yearOfPublication <= 0)
        {
            Console.Write("Invalid input. Please enter a valid year: ");
        }

        bookManager.AddBook(title, author, yearOfPublication);
        Pause();
    }

    static void SearchBook(BookManager bookManager)
    {
        Console.Clear();
        Console.WriteLine("Search for a Book by Title or Author");

        Console.Write("Enter title or author to search: ");
        string searchQuery = Console.ReadLine().Trim();
        if (string.IsNullOrWhiteSpace(searchQuery))
        {
            Console.WriteLine("Search query cannot be empty.");
            Pause();
            return;
        }

        bookManager.SearchBook(searchQuery);
        Pause();
    }

    static void Pause()
    {
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }
}
