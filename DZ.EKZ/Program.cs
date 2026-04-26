using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using Dapper;
using System.Linq;

namespace DZ.EKZ;

public class DataRepository
{
    private readonly string _connectionString;

    public DataRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    // Массовая вставка авторов
    public int BulkInsertAuthors(IEnumerable<Author> authors)
    {
        string sql = "INSERT INTO table_authors (name, bio) VALUES (@Name, @Bio);";

        using (var connection = new Npgsql.NpgsqlConnection(_connectionString))
        {
            connection.Open();
            return connection.Execute(sql, authors);
        }
    }

    // Получение постов с авторами (Маппинг many-to-one)
    public IEnumerable<Post> GetPostsWithAuthors()
    {
        string sql = @"
            SELECT p.id, p.title, p.content, p.author_id, a.id, a.name, a.bio
            FROM table_posts p
            JOIN table_authors a ON p.author_id = a.id";

        using (var connection = new Npgsql.NpgsqlConnection(_connectionString))
        {
            connection.Open();
            var posts = connection.Query<Post, Author, Post>(
                sql,
                (post, author) => { post.Author = author; return post; },
                splitOn: "id" // разделитель для объектов
            );
            return posts;
        }
    }

    // Получение постов по массиву авторов
    public IEnumerable<Post> GetPostsByAuthorIds(int[] authorIds)
    {
        if (authorIds == null || authorIds.Length == 0)
            return Enumerable.Empty<Post>();

        string sql = "SELECT * FROM table_posts WHERE author_id = ANY(@Ids)";

        using (var connection = new Npgsql.NpgsqlConnection(_connectionString))
        {
            connection.Open();
            return connection.Query<Post>(sql, new { Ids = authorIds });
        }
    }
    
    // ------------------- CRUD Операции -------------------
    
    // Добавить новую книгу
    public int AddPost(Post newPost)
    {
        string sql = "INSERT INTO table_posts (title, content, author_id) VALUES (@Title, @Content, @AuthorId) RETURNING id;";
        using (var connection = new Npgsql.NpgsqlConnection(_connectionString))
        {
            connection.Open();
            return connection.ExecuteScalar<int>(sql, newPost);
        }
    }
    // Просмотреть все книги
    public IEnumerable<Post> GetAllPosts()
    {
        string sql = "SELECT * FROM table_posts";
        using (var connection = new Npgsql.NpgsqlConnection(_connectionString))
        {
            connection.Open();
            return connection.Query<Post>(sql);
        }
    }
    // Редактировать книгу
    public bool UpdatePost(Post post)
    {
        string sql = "UPDATE table_posts SET title=@Title, content=@Content, author_id=@AuthorId WHERE id=@Id";
        using (var connection = new Npgsql.NpgsqlConnection(_connectionString))
        {
            connection.Open();
            int affected = connection.Execute(sql, post);
            return affected > 0;
        }
    }
    // Удалить книгу по ID
    public bool DeletePost(int postId)
    {
        string sql = "DELETE FROM table_posts WHERE id=@Id";
        using (var connection = new Npgsql.NpgsqlConnection(_connectionString))
        {
            connection.Open();
            int affected = connection.Execute(sql, new { Id = postId });
            return affected > 0;
        }
    }
    // Создать автора
    public int AddAuthor(Author author)
    {
        string sql = "INSERT INTO table_authors (name, bio) VALUES (@Name, @Bio) RETURNING id;";
        using (var connection = new Npgsql.NpgsqlConnection(_connectionString))
        {
            connection.Open();
            return connection.ExecuteScalar<int>(sql, author);
        }
    }
    // Обновить автора
    public bool UpdateAuthor(Author author)
    {
        string sql = "UPDATE table_authors SET name=@Name, bio=@Bio WHERE id=@Id";
        using (var connection = new Npgsql.NpgsqlConnection(_connectionString))
        {
            connection.Open();
            return connection.Execute(sql, author) > 0;
        }
    }
    // Удалить автора
    public bool DeleteAuthor(int authorId)
    {
        string sql = "DELETE FROM table_authors WHERE id=@Id";
        using (var connection = new Npgsql.NpgsqlConnection(_connectionString))
        {
            connection.Open();
            return connection.Execute(sql, new { Id = authorId }) > 0;
        }
    }
    // Добавить издательство
    public int AddPublisher(Publisher publisher)
    {
        string sql = "INSERT INTO table_publishers (name) VALUES (@Name) RETURNING id;";
        using (var connection = new Npgsql.NpgsqlConnection(_connectionString))
        {
            connection.Open();
            return connection.ExecuteScalar<int>(sql, publisher);
        }
    }
    // Обноваить издательство 
    public bool UpdatePublisher(Publisher publisher)
    {
        string sql = "UPDATE table_publishers SET name=@Name WHERE id=@Id";
        using (var connection = new Npgsql.NpgsqlConnection(_connectionString))
        {
            connection.Open();
            return connection.Execute(sql, publisher) > 0;
        }
    }
    // Удалить издательство
    public bool DeletePublisher(int publisherId)
    {
        string sql = "DELETE FROM table_publishers WHERE id=@Id";
        using (var connection = new Npgsql.NpgsqlConnection(_connectionString))
        {
            connection.Open();
            return connection.Execute(sql, new { Id = publisherId }) > 0;
        }
    }
    
    // ------------------- Отчеты и статистика -------------------
    
    // Количество книг
    public int GetTotalBooks()
    {
        string sql = "SELECT COUNT(*) FROM table_posts";
        using (var connection = new Npgsql.NpgsqlConnection(_connectionString))
        {
            connection.Open();
            return connection.ExecuteScalar<int>(sql);
        }
    }
    
    // Топ-3 авторов по количеству книг
    public IEnumerable<dynamic> GetTopAuthors(int topCount = 3)
    {
        string sql = @"
            SELECT a.id, a.name, COUNT(p.id) AS BookCount
            FROM table_authors a
            JOIN table_posts p ON p.author_id = a.id
            GROUP BY a.id, a.name
            ORDER BY BookCount DESC
            LIMIT @TopCount";

        using (var connection = new Npgsql.NpgsqlConnection(_connectionString))
        {
            connection.Open();
            return connection.Query(sql, new { TopCount = topCount });
        }
    }
    
    // Группировка: книги по авторам
    public IEnumerable<dynamic> GetBooksGroupedByAuthors()
    {
        string sql = @"
            SELECT a.name AS AuthorName, COUNT(p.id) AS BooksCount
            FROM table_authors a
            LEFT JOIN table_posts p ON p.author_id = a.id
            GROUP BY a.name";

        using (var connection = new Npgsql.NpgsqlConnection(_connectionString))
        {
            connection.Open();
            return connection.Query(sql);
        }
    }
}