using System.Text;
using System.Xml.Linq;
using DigitalLibraryManagementSystem.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DigitalLibraryManagementSystem.Data;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IWebHostEnvironment _environment;

    public DatabaseSeeder(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IWebHostEnvironment environment)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _environment = environment;
    }

    public async Task SeedAsync()
    {
        await _context.Database.MigrateAsync();

        await SeedRolesAsync();
        await SeedDefaultUsersAsync();
        await SeedCategoriesAsync();
        await SeedAuthorsAsync();
        await SeedPublishersAsync();
        await SeedBooksAsync();
        await SeedBookCoversAsync();
    }

    private async Task SeedRolesAsync()
    {
        string[] roles = ["Admin", "Librarian", "Member"];

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private async Task SeedDefaultUsersAsync()
    {
        var adminEmail = "admin@digitallibrary.com";
        var adminUser = await _userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Administrator",
                EmailConfirmed = true,
                IsActive = true,
                RegistrationDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
        else if (!await _userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            await _userManager.AddToRoleAsync(adminUser, "Admin");
        }

        var librarianEmail = "librarian@digitallibrary.com";
        var librarianUser = await _userManager.FindByEmailAsync(librarianEmail);

        if (librarianUser == null)
        {
            librarianUser = new ApplicationUser
            {
                UserName = librarianEmail,
                Email = librarianEmail,
                FullName = "Head Librarian",
                EmailConfirmed = true,
                IsActive = true,
                RegistrationDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(librarianUser, "Library@123");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(librarianUser, "Librarian");
            }
        }
        else if (!await _userManager.IsInRoleAsync(librarianUser, "Librarian"))
        {
            await _userManager.AddToRoleAsync(librarianUser, "Librarian");
        }
    }

    private async Task SeedCategoriesAsync()
    {
        var categories = new[]
        {
            new Category { Name = "Computer Science", Description = "Computing and software engineering topics" },
            new Category { Name = "Programming", Description = "Programming languages and development" },
            new Category { Name = "Artificial Intelligence", Description = "AI, ML and intelligent systems" },
            new Category { Name = "Database", Description = "Database systems and design" },
            new Category { Name = "Networking", Description = "Networking and communication" },
            new Category { Name = "Cyber Security", Description = "Security and cyber defense" },
            new Category { Name = "Literature", Description = "Books on literature and culture" },
            new Category { Name = "Business", Description = "Business and management" },
            new Category { Name = "Science", Description = "Scientific disciplines" },
            new Category { Name = "History", Description = "Historical narratives, civilizations, and world events" },
            new Category { Name = "Biography", Description = "Lives of influential people and world leaders" },
            new Category { Name = "Philosophy", Description = "Thought, ethics, and human ideas" },
            new Category { Name = "Economics", Description = "Finance, markets, and development studies" },
            new Category { Name = "Mathematics", Description = "Mathematical theory and problem solving" },
            new Category { Name = "English", Description = "Language, literature, and composition" },
            new Category { Name = "Novels", Description = "Fictional storytelling and literary works" },
            new Category { Name = "Self Development", Description = "Personal growth, productivity, and habits" },
            new Category { Name = "Travel", Description = "Journeys, culture, and exploration" },
            new Category { Name = "Health", Description = "Wellbeing, fitness, and practical healthcare" },
            new Category { Name = "Reference", Description = "Quick lookup, dictionaries, and general guides" },
            new Category { Name = "Religion", Description = "Faith, spirituality, and culture" },
            new Category { Name = "General Knowledge", Description = "Broad learning across everyday human knowledge" }
        };

        var existingNames = new HashSet<string>(
            await _context.Categories.AsNoTracking().Select(c => c.Name).ToListAsync(),
            StringComparer.OrdinalIgnoreCase);
        var missingCategories = categories.Where(c => !existingNames.Contains(c.Name)).ToList();

        if (missingCategories.Count > 0)
        {
            await _context.Categories.AddRangeAsync(missingCategories);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedAuthorsAsync()
    {
        var authors = new[]
        {
            new Author { Name = "Andrew Tanenbaum", Biography = "Computer systems author and educator." },
            new Author { Name = "Robert C. Martin", Biography = "Known for clean code and software craftsmanship." },
            new Author { Name = "Stuart Russell", Biography = "AI and machine learning researcher." },
            new Author { Name = "C. J. Date", Biography = "Database theory and SQL expert." },
            new Author { Name = "Kurose Ross", Biography = "Networking and communications book author." },
            new Author { Name = "William Stallings", Biography = "Computer security author." },
            new Author { Name = "Jane Austen", Biography = "Classic literature author." },
            new Author { Name = "Peter Drucker", Biography = "Management and strategy thinker." },
            new Author { Name = "Stephen Hawking", Biography = "Science communicator and cosmologist." },
            new Author { Name = "Mark Lutz", Biography = "Python language and development expert." },
            new Author { Name = "David McCullough", Biography = "Pulitzer Prize-winning historian and storyteller." },
            new Author { Name = "Walter Isaacson", Biography = "Biographer and historian of science and innovation." },
            new Author { Name = "Maya Angelou", Biography = "Poet and memoirist known for deeply human storytelling." },
            new Author { Name = "Marcus Aurelius", Biography = "Roman emperor and stoic philosopher." },
            new Author { Name = "Thomas Merton", Biography = "Writer and spiritual thinker." },
            new Author { Name = "N. Gregory Mankiw", Biography = "Economics educator and author." },
            new Author { Name = "Steven Strogatz", Biography = "Mathematician and science writer." },
            new Author { Name = "Shawn Achor", Biography = "Expert on positive psychology and performance." },
            new Author { Name = "Paul Theroux", Biography = "Travel writer and novelist." },
            new Author { Name = "Deepak Chopra", Biography = "Author and wellness educator." },
            new Author { Name = "Atul Gawande", Biography = "Surgeon and physician-author focused on health systems." },
            new Author { Name = "Khaled Hosseini", Biography = "Author known for human-centered fiction and cultural storytelling." },
            new Author { Name = "Yuval Noah Harari", Biography = "Historian and thinker on civilization and culture." },
            new Author { Name = "John Green", Biography = "Contemporary novelist and essayist." },
            new Author { Name = "Dale Carnegie", Biography = "Writer on communication and personal development." },
            new Author { Name = "Ray Dalio", Biography = "Investor and author of decision-making frameworks." },
            new Author { Name = "James Clear", Biography = "Author focused on behavior change and habit design." },
            new Author { Name = "Paulo Coelho", Biography = "Writer known for philosophical and reflective fiction." },
            new Author { Name = "Bill Bryson", Biography = "Humorous travel and science writer." },
            new Author { Name = "Bessel van der Kolk", Biography = "Psychiatrist and trauma researcher." },
            new Author { Name = "Jason Fung", Biography = "Physician and fasting and metabolic health expert." },
            new Author { Name = "Sarah Janssen", Biography = "Reference editor and author of factual works." },
            new Author { Name = "DK", Biography = "Publishing brand focused on illustrated reference and knowledge." },
            new Author { Name = "Strunk & White", Biography = "Writers of a canonical English style guide." },
            new Author { Name = "David Grann", Biography = "Author of narrative nonfiction and investigative stories." },
            new Author { Name = "Various", Biography = "Collective authorship used for multi-author reference texts." },
            new Author { Name = "Islamic Scholars", Biography = "Collective contributors to religious and scholarly texts." }
        };

        var existingNames = new HashSet<string>(
            await _context.Authors.AsNoTracking().Select(a => a.Name).ToListAsync(),
            StringComparer.OrdinalIgnoreCase);
        var missingAuthors = authors.Where(a => !existingNames.Contains(a.Name)).ToList();

        if (missingAuthors.Count > 0)
        {
            await _context.Authors.AddRangeAsync(missingAuthors);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedPublishersAsync()
    {
        var publishers = new[]
        {
            new Publisher { Name = "Pearson", Address = "New York, USA", Phone = "+1-212-000-1000" },
            new Publisher { Name = "Prentice Hall", Address = "Boston, USA", Phone = "+1-617-000-2000" },
            new Publisher { Name = "MIT Press", Address = "Cambridge, USA", Phone = "+1-617-000-3000" },
            new Publisher { Name = "OReilly", Address = "California, USA", Phone = "+1-415-000-4000" },
            new Publisher { Name = "Oxford University Press", Address = "Oxford, UK", Phone = "+44-1865-000-5000" },
            new Publisher { Name = "McGraw Hill", Address = "Chicago, USA", Phone = "+1-312-000-6000" },
            new Publisher { Name = "Simon & Schuster", Address = "New York, USA", Phone = "+1-212-000-7000" },
            new Publisher { Name = "HarperOne", Address = "New York, USA", Phone = "+1-212-000-7100" },
            new Publisher { Name = "Penguin Random House", Address = "London, UK", Phone = "+44-207-000-8000" },
            new Publisher { Name = "The Experiment", Address = "New York, USA", Phone = "+1-212-000-8200" },
            new Publisher { Name = "W. W. Norton & Company", Address = "New York, USA", Phone = "+1-212-000-8300" },
            new Publisher { Name = "Bloomsbury", Address = "London, UK", Phone = "+44-020-000-8400" },
            new Publisher { Name = "Hachette Book Group", Address = "New York, USA", Phone = "+1-212-000-8500" },
            new Publisher { Name = "Routledge", Address = "London, UK", Phone = "+44-20-000-8600" },
            new Publisher { Name = "Cengage", Address = "Boston, USA", Phone = "+1-617-000-9000" },
            new Publisher { Name = "Houghton Mifflin Harcourt", Address = "Boston, USA", Phone = "+1-617-000-9100" },
            new Publisher { Name = "Avery", Address = "New York, USA", Phone = "+1-212-000-9200" },
            new Publisher { Name = "Greystone Books", Address = "Vancouver, Canada", Phone = "+1-604-000-9300" },
            new Publisher { Name = "World Almanac", Address = "New York, USA", Phone = "+1-212-000-9400" },
            new Publisher { Name = "HarperCollins", Address = "New York, USA", Phone = "+1-212-000-9500" },
            new Publisher { Name = "Dorling Kindersley", Address = "London, UK", Phone = "+44-020-000-9600" },
            new Publisher { Name = "Dar Al-Maarifa", Address = "Beirut, Lebanon", Phone = "+961-1-000-9700" },
            new Publisher { Name = "Doubleday", Address = "New York, USA", Phone = "+1-212-000-9800" }
        };

        var existingNames = new HashSet<string>(
            await _context.Publishers.AsNoTracking().Select(p => p.Name).ToListAsync(),
            StringComparer.OrdinalIgnoreCase);
        var missingPublishers = publishers.Where(p => !existingNames.Contains(p.Name)).ToList();

        if (missingPublishers.Count > 0)
        {
            await _context.Publishers.AddRangeAsync(missingPublishers);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedBooksAsync()
    {
        var categories = await _context.Categories.ToListAsync();
        var authors = await _context.Authors.ToListAsync();
        var publishers = await _context.Publishers.ToListAsync();

        var categoryMap = categories.ToDictionary(c => c.Name, c => c.CategoryId, StringComparer.OrdinalIgnoreCase);
        var authorMap = authors.ToDictionary(a => a.Name, a => a.AuthorId, StringComparer.OrdinalIgnoreCase);
        var publisherMap = publishers.ToDictionary(p => p.Name, p => p.PublisherId, StringComparer.OrdinalIgnoreCase);

        var books = new[]
        {
            new { Title = "Modern Operating Systems", ISBN = "9780133591624", Description = "A complete operating systems reference.", PublishedYear = 2020, Edition = "4th", Language = "English", TotalCopies = 5, AvailableCopies = 4, ShelfLocation = "QA76-3", Category = "Computer Science", Author = "Andrew Tanenbaum", Publisher = "Pearson" },
            new { Title = "Clean Code", ISBN = "9780132350884", Description = "Practical software engineering principles.", PublishedYear = 2019, Edition = "1st", Language = "English", TotalCopies = 6, AvailableCopies = 5, ShelfLocation = "QA76-4", Category = "Programming", Author = "Robert C. Martin", Publisher = "Prentice Hall" },
            new { Title = "Artificial Intelligence: A Modern Approach", ISBN = "9780137903955", Description = "Foundational AI concepts and practical theory.", PublishedYear = 2021, Edition = "4th", Language = "English", TotalCopies = 4, AvailableCopies = 2, ShelfLocation = "Q335-1", Category = "Artificial Intelligence", Author = "Stuart Russell", Publisher = "Pearson" },
            new { Title = "An Introduction to Database Systems", ISBN = "9780201385907", Description = "Theoretical and practical guide to databases.", PublishedYear = 2018, Edition = "8th", Language = "English", TotalCopies = 5, AvailableCopies = 3, ShelfLocation = "QA76-9", Category = "Database", Author = "C. J. Date", Publisher = "Pearson" },
            new { Title = "Computer Networking: A Top-Down Approach", ISBN = "9780132856201", Description = "Network architecture and routing fundamentals.", PublishedYear = 2022, Edition = "8th", Language = "English", TotalCopies = 5, AvailableCopies = 1, ShelfLocation = "TK5105-8", Category = "Networking", Author = "Kurose Ross", Publisher = "Pearson" },
            new { Title = "Computer Security: Principles and Practice", ISBN = "9780133773922", Description = "Security principles and applied techniques.", PublishedYear = 2019, Edition = "4th", Language = "English", TotalCopies = 4, AvailableCopies = 2, ShelfLocation = "QA76-8", Category = "Cyber Security", Author = "William Stallings", Publisher = "Pearson" },
            new { Title = "Pride and Prejudice", ISBN = "9780141040349", Description = "Classic literary novel examining society and relationships.", PublishedYear = 1813, Edition = "Classic", Language = "English", TotalCopies = 3, AvailableCopies = 3, ShelfLocation = "PR-2", Category = "Literature", Author = "Jane Austen", Publisher = "Oxford University Press" },
            new { Title = "The Essential Drucker", ISBN = "9780060820324", Description = "A guide to management and leadership fundamentals.", PublishedYear = 2001, Edition = "1st", Language = "English", TotalCopies = 5, AvailableCopies = 4, ShelfLocation = "HD62.7-3", Category = "Business", Author = "Peter Drucker", Publisher = "McGraw Hill" },
            new { Title = "A Brief History of Time", ISBN = "9780553380163", Description = "A concise introduction to cosmology and black holes.", PublishedYear = 1988, Edition = "10th", Language = "English", TotalCopies = 4, AvailableCopies = 2, ShelfLocation = "QB981-1", Category = "Science", Author = "Stephen Hawking", Publisher = "MIT Press" },
            new { Title = "Learning Python", ISBN = "9781449355739", Description = "Python language programming essentials and examples.", PublishedYear = 2023, Edition = "6th", Language = "English", TotalCopies = 6, AvailableCopies = 4, ShelfLocation = "QA76-5", Category = "Programming", Author = "Mark Lutz", Publisher = "OReilly" },
            new { Title = "The Story of the World", ISBN = "9781602201234", Description = "An engaging overview of global history from ancient civilizations to modern times.", PublishedYear = 2016, Edition = "2nd", Language = "English", TotalCopies = 5, AvailableCopies = 4, ShelfLocation = "D-101", Category = "History", Author = "Yuval Noah Harari", Publisher = "HarperOne" },
            new { Title = "The Wright Brothers", ISBN = "9781476728752", Description = "A vivid biography of the pioneering aviation brothers and their relentless drive.", PublishedYear = 2015, Edition = "1st", Language = "English", TotalCopies = 4, AvailableCopies = 3, ShelfLocation = "B-42", Category = "Biography", Author = "David McCullough", Publisher = "Simon & Schuster" },
            new { Title = "Steve Jobs", ISBN = "9781451648539", Description = "A deeply reported portrait of one of the most influential innovators of the modern era.", PublishedYear = 2011, Edition = "1st", Language = "English", TotalCopies = 6, AvailableCopies = 5, ShelfLocation = "B-44", Category = "Biography", Author = "Walter Isaacson", Publisher = "Simon & Schuster" },
            new { Title = "Meditations", ISBN = "9780141399167", Description = "A timeless Stoic guide to self-discipline, ethics, and calm reflection.", PublishedYear = 2017, Edition = "Modern", Language = "English", TotalCopies = 7, AvailableCopies = 6, ShelfLocation = "B-77", Category = "Philosophy", Author = "Marcus Aurelius", Publisher = "Penguin Random House" },
            new { Title = "The Republic", ISBN = "9780140449148", Description = "A foundational text on justice, governance, and the ideal state.", PublishedYear = 2018, Edition = "Classic", Language = "English", TotalCopies = 4, AvailableCopies = 2, ShelfLocation = "P-11", Category = "Philosophy", Author = "Marcus Aurelius", Publisher = "Penguin Random House" },
            new { Title = "Principles", ISBN = "9781501124020", Description = "A practical framework for making better decisions in economics, life, and work.", PublishedYear = 2018, Edition = "1st", Language = "English", TotalCopies = 5, AvailableCopies = 4, ShelfLocation = "EC-23", Category = "Economics", Author = "Ray Dalio", Publisher = "Simon & Schuster" },
            new { Title = "Economics", ISBN = "9781337096552", Description = "A clear, student-friendly introduction to economic principles and policy.", PublishedYear = 2020, Edition = "10th", Language = "English", TotalCopies = 6, AvailableCopies = 5, ShelfLocation = "EC-31", Category = "Economics", Author = "N. Gregory Mankiw", Publisher = "Cengage" },
            new { Title = "The Joy of x", ISBN = "9780544105858", Description = "A compelling and accessible exploration of mathematical ideas in daily life.", PublishedYear = 2013, Edition = "1st", Language = "English", TotalCopies = 5, AvailableCopies = 3, ShelfLocation = "M-15", Category = "Mathematics", Author = "Steven Strogatz", Publisher = "Houghton Mifflin Harcourt" },
            new { Title = "The Elements of Style", ISBN = "9780205309023", Description = "A classic guide to clear writing, grammar, and effective English usage.", PublishedYear = 2017, Edition = "4th", Language = "English", TotalCopies = 8, AvailableCopies = 6, ShelfLocation = "E-22", Category = "English", Author = "Strunk & White", Publisher = "Pearson" },
            new { Title = "The Alchemist", ISBN = "9780061122419", Description = "A beloved novel about destiny, purpose, and personal courage.", PublishedYear = 2005, Edition = "1st", Language = "English", TotalCopies = 8, AvailableCopies = 7, ShelfLocation = "N-10", Category = "Novels", Author = "Paulo Coelho", Publisher = "HarperOne" },
            new { Title = "The Kite Runner", ISBN = "9781594480003", Description = "A moving and deeply human novel about friendship, guilt, and redemption.", PublishedYear = 2003, Edition = "1st", Language = "English", TotalCopies = 6, AvailableCopies = 4, ShelfLocation = "N-18", Category = "Novels", Author = "Khaled Hosseini", Publisher = "Penguin Random House" },
            new { Title = "Atomic Habits", ISBN = "9780735211292", Description = "A practical guide to building small habits that create lasting personal change.", PublishedYear = 2018, Edition = "1st", Language = "English", TotalCopies = 10, AvailableCopies = 8, ShelfLocation = "SD-02", Category = "Self Development", Author = "James Clear", Publisher = "Avery" },
            new { Title = "How to Win Friends and Influence People", ISBN = "9780671027032", Description = "A classic self-development book on effective communication and relationships.", PublishedYear = 1936, Edition = "Classic", Language = "English", TotalCopies = 7, AvailableCopies = 5, ShelfLocation = "SD-08", Category = "Self Development", Author = "Dale Carnegie", Publisher = "Simon & Schuster" },
            new { Title = "In a Sunburned Country", ISBN = "9780140258092", Description = "A humorous and reflective journey through the landscape and culture of Australia.", PublishedYear = 2000, Edition = "1st", Language = "English", TotalCopies = 4, AvailableCopies = 3, ShelfLocation = "T-12", Category = "Travel", Author = "Bill Bryson", Publisher = "Penguin Random House" },
            new { Title = "The Lost City of Z", ISBN = "9780307417137", Description = "A gripping account of exploration, obsession, and the unknown in the Amazon.", PublishedYear = 2009, Edition = "1st", Language = "English", TotalCopies = 5, AvailableCopies = 2, ShelfLocation = "T-09", Category = "Travel", Author = "David Grann", Publisher = "Doubleday" },
            new { Title = "The Body Keeps the Score", ISBN = "9780670785933", Description = "A practical look at trauma, healing, and resilience in modern life.", PublishedYear = 2015, Edition = "1st", Language = "English", TotalCopies = 8, AvailableCopies = 6, ShelfLocation = "H-03", Category = "Health", Author = "Bessel van der Kolk", Publisher = "Penguin Random House" },
            new { Title = "The Complete Guide to Fasting", ISBN = "9781401955433", Description = "A clear and evidence-based guide to intermittent fasting and healthy routines.", PublishedYear = 2016, Edition = "1st", Language = "English", TotalCopies = 5, AvailableCopies = 4, ShelfLocation = "H-05", Category = "Health", Author = "Jason Fung", Publisher = "Greystone Books" },
            new { Title = "The World Almanac and Book of Facts", ISBN = "9781510761338", Description = "A dependable reference volume for general information and current facts.", PublishedYear = 2024, Edition = "2024", Language = "English", TotalCopies = 6, AvailableCopies = 5, ShelfLocation = "R-12", Category = "Reference", Author = "Sarah Janssen", Publisher = "World Almanac" },
            new { Title = "The Book of Knowledge", ISBN = "9780062560100", Description = "A broad family reference covering science, history, and practical discovery.", PublishedYear = 2019, Edition = "1st", Language = "English", TotalCopies = 4, AvailableCopies = 3, ShelfLocation = "R-22", Category = "Reference", Author = "Various", Publisher = "HarperCollins" },
            new { Title = "The Holy Quran", ISBN = "9789960657803", Description = "A spiritual and literary classic that guides faith, reflection, and moral life.", PublishedYear = 2012, Edition = "Standard", Language = "Arabic", TotalCopies = 5, AvailableCopies = 3, ShelfLocation = "RE-18", Category = "Religion", Author = "Islamic Scholars", Publisher = "Dar Al-Maarifa" },
            new { Title = "The Book of Genesis", ISBN = "9780060834741", Description = "A foundational religious text exploring origin, covenant, and human meaning.", PublishedYear = 2015, Edition = "Classic", Language = "English", TotalCopies = 4, AvailableCopies = 2, ShelfLocation = "RE-09", Category = "Religion", Author = "Various", Publisher = "HarperOne" },
            new { Title = "The Knowledge Book", ISBN = "9780241981199", Description = "A wide-ranging reference on science, geography, culture, and everyday understanding.", PublishedYear = 2021, Edition = "1st", Language = "English", TotalCopies = 6, AvailableCopies = 5, ShelfLocation = "GK-04", Category = "General Knowledge", Author = "DK", Publisher = "Dorling Kindersley" },
            new { Title = "A Short History of Nearly Everything", ISBN = "9780767908184", Description = "A lively and accessible guide to science, history, and the human world.", PublishedYear = 2004, Edition = "1st", Language = "English", TotalCopies = 6, AvailableCopies = 4, ShelfLocation = "GK-17", Category = "General Knowledge", Author = "Bill Bryson", Publisher = "Doubleday" }
        };

        var existingIsbns = new HashSet<string>(
            await _context.Books.AsNoTracking().Select(b => b.ISBN).ToListAsync(),
            StringComparer.OrdinalIgnoreCase);
        var newBooks = new List<Book>();

        foreach (var book in books)
        {
            if (existingIsbns.Contains(book.ISBN))
            {
                continue;
            }

            if (!categoryMap.TryGetValue(book.Category, out var categoryId) ||
                !authorMap.TryGetValue(book.Author, out var authorId) ||
                !publisherMap.TryGetValue(book.Publisher, out var publisherId))
            {
                continue;
            }

            newBooks.Add(new Book
            {
                Title = book.Title,
                ISBN = book.ISBN,
                Description = book.Description,
                PublishedYear = book.PublishedYear,
                Edition = book.Edition,
                Language = book.Language,
                TotalCopies = book.TotalCopies,
                AvailableCopies = book.AvailableCopies,
                ShelfLocation = book.ShelfLocation,
                CoverImagePath = string.Empty,
                DigitalFilePath = string.Empty,
                IsDigitalAvailable = false,
                CategoryId = categoryId,
                AuthorId = authorId,
                PublisherId = publisherId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        if (newBooks.Count > 0)
        {
            await _context.Books.AddRangeAsync(newBooks);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedBookCoversAsync()
    {
        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var coversFolder = Path.Combine(webRoot, "uploads", "covers");
        Directory.CreateDirectory(coversFolder);

        var books = await _context.Books
            .Include(book => book.Author)
            .Include(book => book.Category)
            .ToListAsync();
        var changed = false;

        foreach (var book in books)
        {
            if (HasExistingUploadedCover(book.CoverImagePath, coversFolder))
            {
                continue;
            }

            var fileName = $"sample-book-{book.BookId}.svg";
            var coverPath = $"/uploads/covers/{fileName}";
            var physicalPath = Path.Combine(coversFolder, fileName);
            var svg = CreateBookCoverSvg(book);

            if (!File.Exists(physicalPath) || !string.Equals(await File.ReadAllTextAsync(physicalPath), svg, StringComparison.Ordinal))
            {
                await File.WriteAllTextAsync(physicalPath, svg, new UTF8Encoding(false));
            }

            if (!string.Equals(book.CoverImagePath, coverPath, StringComparison.Ordinal))
            {
                book.CoverImagePath = coverPath;
                changed = true;
            }
        }

        if (changed)
        {
            await _context.SaveChangesAsync();
        }
    }

    private static bool HasExistingUploadedCover(string? relativePath, string coversFolder)
    {
        const string prefix = "/uploads/covers/";
        if (string.IsNullOrWhiteSpace(relativePath)
            || !relativePath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            || relativePath.Contains('\\')
            || relativePath.Contains('?')
            || relativePath.Contains('#'))
        {
            return false;
        }

        var fileName = relativePath[prefix.Length..];
        if (string.IsNullOrWhiteSpace(fileName) || fileName.Contains('/') || fileName is "." or "..")
        {
            return false;
        }

        var folderPath = Path.GetFullPath(coversFolder);
        var physicalPath = Path.GetFullPath(Path.Combine(folderPath, fileName));
        return physicalPath.StartsWith(folderPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            && File.Exists(physicalPath);
    }

    private static string CreateBookCoverSvg(Book book)
    {
        XNamespace svg = "http://www.w3.org/2000/svg";
        var palettes = new[]
        {
            (Background: "#102B46", Accent: "#D6AD62", Paper: "#F1F5F2"),
            (Background: "#24465A", Accent: "#D88B61", Paper: "#F5F1E8"),
            (Background: "#263E38", Accent: "#C5AE72", Paper: "#F1F3EA"),
            (Background: "#3E3448", Accent: "#D4A66A", Paper: "#F4F0EA"),
            (Background: "#4A3537", Accent: "#D7B26E", Paper: "#F6F0E8"),
            (Background: "#28505A", Accent: "#D89E64", Paper: "#F2F4EF")
        };
        var palette = palettes[Math.Abs(book.CategoryId * 17 + book.BookId) % palettes.Length];
        var titleLines = WrapCoverText(book.Title, 23, 4);
        var authorLines = WrapCoverText(book.Author?.Name ?? "Unknown author", 30, 2);
        var category = book.Category?.Name ?? "General Collection";
        var titleStartY = 340;
        var title = new XElement(svg + "text",
            new XAttribute("x", "76"),
            new XAttribute("y", titleStartY),
            new XAttribute("fill", palette.Paper),
            new XAttribute("font-family", "Georgia, serif"),
            new XAttribute("font-size", "38"),
            new XAttribute("font-weight", "700"),
            new XAttribute("line-height", "1.25"),
            titleLines.Select((line, index) => new XElement(svg + "tspan",
                new XAttribute("x", "76"),
                new XAttribute("dy", index == 0 ? "0" : "1.22em"),
                line)));

        var authorStartY = titleStartY + titleLines.Count * 48 + 42;
        var author = new XElement(svg + "text",
            new XAttribute("x", "78"),
            new XAttribute("y", authorStartY),
            new XAttribute("fill", palette.Paper),
            new XAttribute("font-family", "Arial, sans-serif"),
            new XAttribute("font-size", "20"),
            authorLines.Select((line, index) => new XElement(svg + "tspan",
                new XAttribute("x", "78"),
                new XAttribute("dy", index == 0 ? "0" : "1.35em"),
                line)));

        var document = new XDocument(
            new XElement(svg + "svg",
                new XAttribute("width", "600"),
                new XAttribute("height", "900"),
                new XAttribute("viewBox", "0 0 600 900"),
                new XElement(svg + "title", $"{book.Title} by {book.Author?.Name ?? "Unknown author"}"),
                new XElement(svg + "rect", new XAttribute("width", "600"), new XAttribute("height", "900"), new XAttribute("fill", palette.Background)),
                new XElement(svg + "rect", new XAttribute("x", "26"), new XAttribute("y", "26"), new XAttribute("width", "548"), new XAttribute("height", "848"), new XAttribute("fill", "none"), new XAttribute("stroke", palette.Accent), new XAttribute("stroke-width", "2")),
                new XElement(svg + "rect", new XAttribute("x", "48"), new XAttribute("y", "48"), new XAttribute("width", "8"), new XAttribute("height", "804"), new XAttribute("fill", palette.Accent)),
                new XElement(svg + "path", new XAttribute("d", "M600 0H360L600 240Z"), new XAttribute("fill", palette.Accent), new XAttribute("opacity", "0.16")),
                new XElement(svg + "circle", new XAttribute("cx", "486"), new XAttribute("cy", "226"), new XAttribute("r", "88"), new XAttribute("fill", "none"), new XAttribute("stroke", palette.Accent), new XAttribute("stroke-width", "2"), new XAttribute("opacity", "0.7")),
                new XElement(svg + "circle", new XAttribute("cx", "486"), new XAttribute("cy", "226"), new XAttribute("r", "64"), new XAttribute("fill", "none"), new XAttribute("stroke", palette.Accent), new XAttribute("stroke-width", "1"), new XAttribute("opacity", "0.55")),
                new XElement(svg + "text", new XAttribute("x", "78"), new XAttribute("y", "112"), new XAttribute("fill", palette.Accent), new XAttribute("font-family", "Arial, sans-serif"), new XAttribute("font-size", "16"), new XAttribute("font-weight", "700"), "DIGITAL LIBRARY"),
                new XElement(svg + "text", new XAttribute("x", "78"), new XAttribute("y", "150"), new XAttribute("fill", palette.Paper), new XAttribute("font-family", "Arial, sans-serif"), new XAttribute("font-size", "13"), "ACADEMIC  /  GENERAL COLLECTION"),
                new XElement(svg + "path", new XAttribute("d", "M78 184H202"), new XAttribute("stroke", palette.Accent), new XAttribute("stroke-width", "3")),
                title,
                author,
                new XElement(svg + "rect", new XAttribute("x", "78"), new XAttribute("y", "742"), new XAttribute("width", "444"), new XAttribute("height", "52"), new XAttribute("rx", "4"), new XAttribute("fill", palette.Accent), new XAttribute("opacity", "0.18")),
                new XElement(svg + "text", new XAttribute("x", "96"), new XAttribute("y", "775"), new XAttribute("fill", palette.Paper), new XAttribute("font-family", "Arial, sans-serif"), new XAttribute("font-size", "16"), new XAttribute("font-weight", "600"), category),
                new XElement(svg + "text", new XAttribute("x", "78"), new XAttribute("y", "828"), new XAttribute("fill", palette.Accent), new XAttribute("font-family", "Arial, sans-serif"), new XAttribute("font-size", "12"), "LIBRARY EDITION  •  " + book.PublishedYear)));

        return document.ToString(SaveOptions.DisableFormatting);
    }

    private static List<string> WrapCoverText(string text, int maxCharacters, int maxLines)
    {
        var lines = new List<string>();
        var currentLine = new StringBuilder();

        foreach (var word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (currentLine.Length > 0 && currentLine.Length + word.Length + 1 > maxCharacters)
            {
                lines.Add(currentLine.ToString());
                currentLine.Clear();
                if (lines.Count == maxLines)
                {
                    lines[^1] = lines[^1].TrimEnd('.') + "...";
                    return lines;
                }
            }

            if (currentLine.Length > 0)
            {
                currentLine.Append(' ');
            }

            currentLine.Append(word);
        }

        if (currentLine.Length > 0 && lines.Count < maxLines)
        {
            lines.Add(currentLine.ToString());
        }

        return lines;
    }
}
