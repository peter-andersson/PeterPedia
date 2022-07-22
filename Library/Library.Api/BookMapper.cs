namespace Library.Api;

public static class BookMapper
{
    public static Book ConvertToBook(this BookEntity entity) =>
        new()
        {
            Id = entity.Id,
            Title = entity.Title,
            Reading = entity.Reading,
            Read = entity.Read,
            WantToRead = entity.WantToRead,
            Authors = entity.Authors
        };
}
