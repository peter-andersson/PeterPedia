using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;

namespace Library.Api.Functions;

public class Query
{
    private readonly ILogger<Query> _log;
    private readonly IDataStorage<BookEntity> _dataStorage;

    public Query(ILogger<Query> log, IDataStorage<BookEntity> dataStorage)
    {
        _log = log;
        _dataStorage = dataStorage;
    }

    [FunctionName("Query")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "query")] HttpRequest req,
        CancellationToken _)
    {
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(requestBody))
        {
            return req.BadRequest("Missing query data");
        }

        QueryData query = JsonConvert.DeserializeObject<QueryData>(requestBody);

        try
        {
            var queryBuilder = new StringBuilder();

            queryBuilder.Append("SELECT * FROM c");

            var addedWhere = false;
            if (!string.IsNullOrEmpty(query.Search))
            {
                addedWhere = true;
                queryBuilder.Append(" WHERE LOWER(c.Title) LIKE @search");
            }

            if (query.IncludeWantToRead && query.IncludeRead && query.IncludeReading)
            {
                queryBuilder.Append(" ORDER BY c.Title");
            }
            else
            {
                if (!addedWhere)
                {
                    addedWhere = true;
                    queryBuilder.Append(" WHERE (");
                }
                else
                {
                    queryBuilder.Append(" AND (");
                }

                var addOr = false;
                if (query.IncludeRead)
                {
                    if (addOr)
                    {
                        queryBuilder.Append(" OR ");
                    }
                    else
                    {
                        addOr = true;
                    }
                    queryBuilder.Append("c.Read");
                }

                if (query.IncludeReading)
                {
                    if (addOr)
                    {
                        queryBuilder.Append(" OR ");
                    }
                    else
                    {
                        addOr = true;
                    }

                    queryBuilder.Append("c.Reading");
                }

                if (query.IncludeWantToRead)
                {
                    if (addOr)
                    {
                        queryBuilder.Append(" OR ");
                    }
                    else
                    {
                        addOr = true;
                    }

                    queryBuilder.Append("c.WantToRead");
                 }

                queryBuilder.Append(") ORDER BY c.Title");
            }

            QueryDefinition queryDefinition = new QueryDefinition(query: queryBuilder.ToString())
                .WithParameter("@search", query.Search);

            List<BookEntity> entities = await _dataStorage.QueryAsync(queryDefinition);
            var result = new List<Book>(entities.Count);
            foreach (BookEntity entity in entities)
            {
                result.Add(entity.ConvertToBook());
            }

            return req.Ok(result);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Something went wrong.");
            return req.InternalServerError();
        }
    }
}
